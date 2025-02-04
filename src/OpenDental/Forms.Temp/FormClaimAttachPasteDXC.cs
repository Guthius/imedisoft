using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental;

public partial class FormClaimAttachPasteDXC : FormODBase
{
    public Claim ClaimCur;
    public Patient PatientCur;

    private readonly List<AttachmentItem> _attachmentItems = [];
    private List<int> _imageReferenceIds;

    public FormClaimAttachPasteDXC()
    {
        InitializeComponent();
    }

    private void FormClaimAttachPasteDXC_Load(object sender, EventArgs e)
    {
        var claimAttachmentsDefs = GetImageCatDefs();
        if (claimAttachmentsDefs.Count > 0)
        {
            labelClaimAttachWarning.Visible = false;
        }

        if (!GetImagesFromClipboard())
        {
            Close();

            return;
        }

        FillGrid();

        ValidateClaimDxc();

        if (_attachmentItems.Count < 1)
        {
            return;
        }

        var bitmap = _attachmentItems[0].Bitmap;
        if (bitmap == null)
        {
            return;
        }

        pictureBox.Image = bitmap;
        try
        {
            gridMain.SetSelected(0);
        }
        catch
        {
            return;
        }

        textNarrative.Text = ClaimCur.Narrative;
    }

    private void FillGrid()
    {
        if (_attachmentItems.Count < 1)
        {
            ShowError("All images have been deleted.");

            Close();
            return;
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Image Name", 150));
        gridMain.Columns.Add(new GridColumn("Date", 75));
        gridMain.Columns.Add(new GridColumn("Image Type", 150));

        gridMain.ListGridRows.Clear();
        foreach (var attachmentItem in _attachmentItems)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(attachmentItem.ImageAttachment.ImageFileNameDisplay);
            gridRow.Cells.Add(attachmentItem.ImageAttachment.ImageDate.ToShortDateString());
            gridRow.Cells.Add(attachmentItem.HasTypeBeenSet ? attachmentItem.ImageAttachment.ImageType.ToString() : "");

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private bool GetImagesFromClipboard()
    {
        var bitmap = ODClipboard.GetImage();

        string[] fileNames = null;
        if (bitmap == null)
        {
            try
            {
                fileNames = ODClipboard.GetFileDropList();
            }
            catch
            {
                // ignored
            }
        }

        var paths = fileNames?.ToList();
        if (paths is null && bitmap is null)
        {
            ShowError("There are no Images saved to your clipboard");
            return false;
        }

        if (bitmap is not null)
        {
            var imageAttachment = ClaimConnect.ImageAttachment.Create("Attachment", DateTime.Today, ClaimConnect.ImageTypeCode.ReferralForm, bitmap);

            _attachmentItems.Add(new AttachmentItem
            {
                ImageAttachment = imageAttachment,
                Bitmap = bitmap
            });
            return true;
        }

        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                ShowError("A file on the clipboard could not be located. It may have been moved, deleted or renamed.");
                return false;
            }

            Bitmap bitmapFromFile;
            try
            {
                bitmapFromFile = new Bitmap(path);
            }
            catch (ArgumentException)
            {
                ShowError("One or more invalid file types were pasted. Valid file types include BMP, GIF, JPEG, PNG, and TIFF.");
                return false;
            }

            var bitmapCopy = new Bitmap(bitmapFromFile);

            bitmapFromFile.Dispose();

            string imageName;
            try
            {
                imageName = Path.GetFileNameWithoutExtension(path);
            }
            catch
            {
                ShowError("There was an issue getting the name of a file on the clipboard.");
                return false;
            }

            if (imageName.IsNullOrEmpty())
            {
                imageName = "Image";
            }

            var imageAttachment = ClaimConnect.ImageAttachment.Create(imageName, DateTime.Today, ClaimConnect.ImageTypeCode.ReferralForm, bitmapCopy);

            _attachmentItems.Add(new AttachmentItem
            {
                ImageAttachment = imageAttachment,
                Bitmap = bitmapCopy
            });
        }

        return true;
    }

    private static List<Def> GetImageCatDefs()
    {
        return Defs.GetCatList((int) DefCat.ImageCats).ToList().FindAll(x => x.ItemValue.Contains("C") && !x.IsHidden);
    }

    private bool ValidateClaimDxc()
    {
        var clearinghouse = ClaimConnect.GetClearingHouseForClaim(ClaimCur);

        return XConnect.IsEnabled(clearinghouse) ? ValidateXConnect() : ValidateClaimConnect();
    }

    private bool ValidateClaimConnect()
    {
        ClaimConnect.ValidateClaimResponse validateClaimResponse = null;

        var progress = new ProgressWin
        {
            ActionMain = () => validateClaimResponse = ClaimConnect.ValidateClaim(ClaimCur, true),
            StartingMessage = "Communicating with DentalXChange..."
        };

        try
        {
            progress.ShowDialog();
        }
        catch (Exception ex)
        {
            textClaimStatus.Text = ex.Message;
            return false;
        }

        if (progress.IsCancelled)
        {
            return false;
        }

        if (validateClaimResponse._isValidClaim)
        {
            textClaimStatus.Text = "The claim is valid.";
            return true;
        }

        var stringBuilder = new StringBuilder();

        foreach (var errorMessage in validateClaimResponse.ValidationErrors)
        {
            stringBuilder.AppendLine(errorMessage);
        }

        textClaimStatus.Text = stringBuilder.ToString();
        return false;
    }

    private bool ValidateXConnect()
    {
        XConnectWebResponse xConnectWebResponse = null;

        var progress = new ProgressWin
        {
            ActionMain = () => xConnectWebResponse = XConnect.ValidateClaim(ClaimCur),
            StartingMessage = "Communicating with DentalXChange..."
        };

        try
        {
            progress.ShowDialog();
        }
        catch (Exception ex)
        {
            textClaimStatus.Text = ex.Message;
            return false;
        }

        if (progress.IsCancelled)
        {
            return false;
        }

        if (xConnectWebResponse.response.claimStatus.message.Length > 0)
        {
            textClaimStatus.Text = xConnectWebResponse.response.claimStatus.message;
            return false;
        }

        foreach (var item in xConnectWebResponse.response.claimItems)
        {
            if (string.IsNullOrEmpty(item.itemStatus.message))
            {
                continue;
            }

            textClaimStatus.Text = item.itemStatus.message;
            return false;
        }

        textClaimStatus.Text = "The claim is valid";
        return true;
    }

    private void AddAttachments(List<ClaimConnect.ImageAttachment> imageAttachments)
    {
        if (string.IsNullOrWhiteSpace(ClaimCur.AttachmentID))
        {
            var attachmentId = ClaimConnect.OpenAttachment(ClaimCur, textNarrative.Text);

            ClaimCur.AttachmentID = attachmentId;

            _imageReferenceIds = ClaimConnect.AddAttachmentImage(ClaimCur, imageAttachments);

            ClaimConnect.SubmitAttachment(ClaimCur);

            ClaimCur.AttachedFlags = "Misc";
        }
        else
        {
            _imageReferenceIds = ClaimConnect.AddAttachmentImage(ClaimCur, imageAttachments);
            if (ClaimCur.Narrative != textNarrative.Text)
            {
                ClaimConnect.AddNarrative(ClaimCur, textNarrative.Text);
            }
        }

        ClaimCur.Narrative = textNarrative.Text;

        Claims.Update(ClaimCur);
    }

    private void BatchAddAttachments(List<ClaimConnect.ImageAttachment> imageAttachments)
    {
        foreach (var imageAttachment in imageAttachments)
        {
            AddAttachments([imageAttachment]);
        }
    }

    private bool SendAttachmentsToDxcAndSaveLocally()
    {
        if (_attachmentItems.Any(x => x.HasTypeBeenSet == false))
        {
            ShowError("The image type for one or more attachments has not been set");
            return false;
        }

        if (_attachmentItems.Any(x => x.ImageAttachment.ImageFileAsBase64 == null) || _attachmentItems.Any(x => x.Bitmap == null))
        {
            ShowError("An image for one or more attachments has not been set");
            return false;
        }

        var imageAttachments = new List<ClaimConnect.ImageAttachment>();
        foreach (var attachmentItem in _attachmentItems)
        {
            imageAttachments.Add(attachmentItem.ImageAttachment);
        }

        try
        {
            AddAttachments(imageAttachments);
        }
        catch (TimeoutException)
        {
            var progressWin = new ProgressWin
            {
                ActionMain = () => BatchAddAttachments(imageAttachments),
                StartingMessage = "Sending attachments timed out. Attempting to send individually. Please wait."
            };

            progressWin.ShowDialog();

            if (progressWin.IsCancelled)
            {
                return false;
            }
        }
        catch (ODException ex)
        {
            ShowError(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            ShowException(ex, "An error has occurred while trying to add attachments. If the problem persists please contact your clearinghouse's support.");
            return false;
        }

        if (!ValidateClaimDxc())
        {
            if (!Confirm("There were errors validating the claim, would you like to continue?"))
            {
                return false;
            }
        }

        var claimAttachCatDef = GetImageCatDefs().FirstOrDefault();
        var defNumImageType = claimAttachCatDef?.DefNum ?? Defs.GetCatList((int) DefCat.ImageCats).FirstOrDefault(x => !x.IsHidden)?.DefNum ?? 0;

        var claimAttaches = new List<ClaimAttach>();
        for (var i = 0; i < imageAttachments.Count; i++)
        {
            if (PrefC.GetBool(PrefName.SaveDXCAttachments))
            {
                var document = ImageStore.Import(imageAttachments[i].ImageFileAsBase64, defNumImageType, ImageType.Attachment, PatientCur);

                imageAttachments[i].ImageFileNameActual = document.FileName;

                var documentCopy = document.Copy();

                document.Description = imageAttachments[i].ImageFileNameDisplay;

                Documents.Update(document, documentCopy);
            }

            claimAttaches.Add(new ClaimAttach
            {
                DisplayedFileName = imageAttachments[i].ImageFileNameDisplay,
                ActualFileName = imageAttachments[i].ImageFileNameActual,
                ClaimNum = ClaimCur.ClaimNum,
                ImageReferenceId = _imageReferenceIds[i]
            });
        }

        ClaimCur.Attachments.AddRange(claimAttaches);

        Claims.Update(ClaimCur);

        ShowInfo("Attachment sent successfully!");

        return true;
    }

    private void TextBoxNarrative_TextChanged(object sender, EventArgs e)
    {
        labelCharCount.Text = textNarrative.Text.Length + "/2000";
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formClaimAttachPasteDxcItem = new FormClaimAttachPasteDXCItem();

        formClaimAttachPasteDxcItem.AttachmentItemCur = _attachmentItems[e.Row];
        formClaimAttachPasteDxcItem.ShowDialog();

        if (formClaimAttachPasteDxcItem.AttachmentItemCur.ImageAttachment == null)
        {
            _attachmentItems[e.Row].Bitmap?.Dispose();
            _attachmentItems.RemoveAt(e.Row);

            pictureBox.Image?.Dispose();
            pictureBox.Image = null;
        }
        else if (formClaimAttachPasteDxcItem.DialogResult == DialogResult.OK)
        {
            _attachmentItems[e.Row].ImageAttachment = formClaimAttachPasteDxcItem.AttachmentItemCur.ImageAttachment;
            _attachmentItems[e.Row].HasTypeBeenSet = formClaimAttachPasteDxcItem.AttachmentItemCur.HasTypeBeenSet;
        }

        FillGrid();
    }

    private void FormClaimAttachPasteDXC_FormClosing(object sender, FormClosingEventArgs e)
    {
        foreach (var attachmentItem in _attachmentItems)
        {
            attachmentItem.Bitmap?.Dispose();
        }
    }

    private void GridMain_CellClick(object sender, ODGridClickEventArgs e)
    {
        var bitmap = _attachmentItems[e.Row].Bitmap;
        if (bitmap is null)
        {
            ShowError("Cannot load the image for this file");
            return;
        }

        pictureBox.Image = bitmap;
    }

    private void ContextMenuImageGrid_Popup(object sender, EventArgs e)
    {
        var selectedIndex = gridMain.GetSelectedIndex();
        if (selectedIndex == -1)
        {
            return;
        }

        var bitmap = _attachmentItems[selectedIndex].Bitmap;
        if (bitmap is null)
        {
            ShowError("Cannot load the image for this file");
            return;
        }

        pictureBox.Image = bitmap;
    }

    private void SetImageTypeCode(ClaimConnect.ImageTypeCode imageTypeCode)
    {
        var selectedIndex = gridMain.GetSelectedIndex();
        if (selectedIndex == -1)
        {
            return;
        }

        _attachmentItems[selectedIndex].HasTypeBeenSet = true;
        _attachmentItems[selectedIndex].ImageAttachment.ImageType = imageTypeCode;

        FillGrid();
    }

    private void MenuItemReferralForm_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.ReferralForm);
    }

    private void MenuItemDiagnosticReport_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.DiagnosticReport);
    }

    private void MenuItemExplanationOfBenefits_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.ExplanationOfBenefits);
    }

    private void MenuItemOtherAttachments_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.OtherAttachments);
    }

    private void MenuItemPeriodontalCharts_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.PeriodontalCharts);
    }

    private void MenuItemXRays_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.XRays);
    }

    private void MenuItemDentalModels_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.DentalModels);
    }

    private void MenuItemRadiologyReports_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.RadiologyReports);
    }

    private void MenuItemIntraOralPhotograph_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.IntraOralPhotograph);
    }

    private void MenuItemNarrative_Click(object sender, EventArgs e)
    {
        SetImageTypeCode(ClaimConnect.ImageTypeCode.Narrative);
    }

    private void ButtonPasteAgain_Click(object sender, EventArgs e)
    {
        if (!GetImagesFromClipboard())
        {
            return;
        }

        FillGrid();
    }

    private void ButtonSend_Click(object sender, EventArgs e)
    {
        var attachmentSentAndSaved = SendAttachmentsToDxcAndSaveLocally();
        if (attachmentSentAndSaved)
        {
            Close();
        }
    }

    public class AttachmentItem
    {
        public ClaimConnect.ImageAttachment ImageAttachment;
        public bool HasTypeBeenSet;
        public Bitmap Bitmap;
    }
}