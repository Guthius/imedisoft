using System;
using System.IO;
using System.Windows.Forms;
using OpenDentBusiness.Eclaims;

namespace OpenDental.Forms;

public partial class FormClaimAttachPasteDXCItem : FormODBase
{
    public FormClaimAttachPasteDXC.AttachmentItem AttachmentItemCur;

    public FormClaimAttachPasteDXCItem()
    {
        InitializeComponent();
    }

    private void FormClaimAttachPasteDXCItem_Load(object sender, EventArgs e)
    {
        textFileName.Text = AttachmentItemCur.ImageAttachment.ImageFileNameDisplay;
        textDateCreated.Text = AttachmentItemCur.ImageAttachment.ImageDate.ToShortDateString();

        listBoxImageType.Items.Clear();
        listBoxImageType.Items.AddEnums<ClaimConnect.ImageTypeCode>();
        listBoxImageType.SetSelected(-1);

        if (AttachmentItemCur.HasTypeBeenSet)
        {
            listBoxImageType.SetSelectedEnum(AttachmentItemCur.ImageAttachment.ImageType);
        }
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        AttachmentItemCur.ImageAttachment = null;

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textFileName.Text))
        {
            ShowError("Enter the filename for this attachment.");
            return;
        }

        if (textFileName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            ShowError("Invalid characters detected in the filename. Please remove them and try again.");
            return;
        }

        if (!DateTime.TryParse(textDateCreated.Text, out var dateCreated))
        {
            ShowError("Enter a valid date.");
            return;
        }

        if (listBoxImageType.SelectedIndex == -1)
        {
            ShowError("Select an image type.");
            return;
        }

        AttachmentItemCur.ImageAttachment.ImageFileNameDisplay = textFileName.Text;
        AttachmentItemCur.ImageAttachment.ImageDate = dateCreated;
        AttachmentItemCur.ImageAttachment.ImageType = listBoxImageType.GetSelected<ClaimConnect.ImageTypeCode>();
        AttachmentItemCur.HasTypeBeenSet = true;

        DialogResult = DialogResult.OK;
    }
}