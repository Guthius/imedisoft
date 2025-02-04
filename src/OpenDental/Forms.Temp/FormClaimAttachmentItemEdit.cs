using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental;

public partial class FormClaimAttachmentItemEdit : FormODBase
{
    private readonly Image _imageForClaim;
    private readonly EclaimsCommBridge _eclaimsCommBridge;
    private readonly string _narrative;

    public bool DoNewSnip { get; set; }
    public bool IsSnip { get; set; }
    public ClaimConnect.ImageAttachment ImageAttachmentDxc { get; set; }
    public EDS.ImageAttachment ImageAttachmentEds { get; set; }

    public FormClaimAttachmentItemEdit(Image image, string fileName, DateTime dateTime, Enum enumImageType, bool isRightOrientation, EclaimsCommBridge eclaimsCommBridge, string narrative = "") : this(image, eclaimsCommBridge)
    {
        textFileName.Text = fileName;
        textDateCreated.Text = dateTime.ToShortDateString();

        listBoxImageType.SetSelectedEnum(enumImageType);

        checkIsXrayMirrored.Checked = !isRightOrientation;

        _narrative = narrative;
    }

    public FormClaimAttachmentItemEdit(Image image, EclaimsCommBridge eclaimsCommBridge)
    {
        InitializeComponent();

        _imageForClaim = image;
        _eclaimsCommBridge = eclaimsCommBridge;

        listBoxImageType.Items.Clear();

        switch (_eclaimsCommBridge)
        {
            case EclaimsCommBridge.ClaimConnect:
                listBoxImageType.Items.AddEnums<ClaimConnect.ImageTypeCode>();
                break;

            case EclaimsCommBridge.EDS:
                listBoxImageType.Items.AddEnums<EDS.EnumDocumentTypeCode>();
                break;
        }

        listBoxImageType.SelectedIndex = 0;

        textDateCreated.Text = DateTime.Today.ToShortDateString();

        ODImaging.ImageApplyOrientation(image);

        pictureBoxImagePreview.Image = ODImaging.ImageScaleMaxHeightAndWidth(image, pictureBoxImagePreview.Height, pictureBoxImagePreview.Width);
        pictureBoxImagePreview.Invalidate();
    }

    private void FormClaimAttachmentItemEdit_Load(object sender, EventArgs e)
    {
        if (IsSnip)
        {
            return;
        }

        butNewSnip.Visible = false;
        labelNewSnip.Visible = false;
    }

    private void ButtonNewSnip_Click(object sender, EventArgs e)
    {
        if (!ValidateAndCreateAttachment())
        {
            return;
        }

        DoNewSnip = true;

        DialogResult = DialogResult.OK;
    }

    private void CreateImageAttachment(DateTime dateCreated)
    {
        switch (_eclaimsCommBridge)
        {
            case EclaimsCommBridge.ClaimConnect:
                ImageAttachmentDxc = ClaimConnect.ImageAttachment.Create(
                    fileName: textFileName.Text,
                    createdDate: dateCreated,
                    typeCodeImage: listBoxImageType.GetSelected<ClaimConnect.ImageTypeCode>(),
                    imageClaim: _imageForClaim,
                    rightOrientation: !checkIsXrayMirrored.Checked);
                break;

            case EclaimsCommBridge.EDS:
                ImageAttachmentEds = EDS.ImageAttachment.Create(
                    fileName: textFileName.Text,
                    dateTimeCreated: dateCreated,
                    documentTypeCode: listBoxImageType.GetSelected<EDS.EnumDocumentTypeCode>(),
                    imageClaim: _imageForClaim,
                    isRightOriented: !checkIsXrayMirrored.Checked,
                    narrative: _narrative);
                break;
        }
    }

    private bool ValidateAndCreateAttachment()
    {
        if (string.IsNullOrWhiteSpace(textFileName.Text))
        {
            ShowError("Enter the filename for this attachment.");
            return false;
        }

        if (textFileName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            ShowError("Invalid characters detected in the filename. Please remove them and try again.");
            return false;
        }

        if (!DateTime.TryParse(textDateCreated.Text, out var dateCreated))
        {
            ShowError("Enter a valid date.");
            return false;
        }

        if (listBoxImageType.SelectedIndex == -1)
        {
            ShowError("Select an image type.");
            return false;
        }

        CreateImageAttachment(dateCreated);
        return true;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!ValidateAndCreateAttachment())
        {
            return;
        }

        DialogResult = DialogResult.OK;
    }
}