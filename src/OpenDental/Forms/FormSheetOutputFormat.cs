using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormSheetOutputFormat : FormODBase
{
    public int QtyPaperCopies { get; set; }
    public bool IsEmailPatOrLab { get; set; }
    public string EmailPatOrLabAddress { get; set; }
    public bool Email2 { get; set; }
    public string Email2Address { get; set; }
    public bool IsEmail2Visible { get; set; }
    public bool IsForLab { get; set; }

    public FormSheetOutputFormat()
    {
        InitializeComponent();
    }

    private void FormSheetOutputFormat_Load(object sender, EventArgs e)
    {
        textPaperCopies.Text = QtyPaperCopies.ToString();
        checkEmailPat.Checked = IsEmailPatOrLab;

        if (IsForLab)
        {
            checkEmailPat.Text = "E-mail to Lab:";
        }

        textEmailPat.Text = EmailPatOrLabAddress;
        if (IsEmail2Visible)
        {
            checkEmail2.Checked = Email2;
            textEmail2.Text = Email2Address;
        }
        else
        {
            checkEmail2.Visible = false;
            textEmail2.Visible = false;
        }

        if (Security.IsAuthorized(EnumPermType.EmailSend, true))
        {
            return;
        }

        textEmail2.Enabled = false;
        textEmailPat.Enabled = false;
    }

    private void CheckBoxEmailPat_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.EmailSend))
        {
            checkEmailPat.Checked = false;
        }
    }

    private void CheckBoxEmail2_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.EmailSend))
        {
            checkEmail2.Checked = false;
        }
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (!textPaperCopies.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        if (checkEmailPat.Checked && textEmailPat.Text == "")
        {
            ShowError("Please enter an email address or uncheck the email box.");
            return;
        }

        if (IsEmail2Visible)
        {
            if (checkEmail2.Checked && textEmail2.Text == "")
            {
                ShowError("Please enter an email address or uncheck the email box.");
                return;
            }
        }

        if (SIn.Long(textPaperCopies.Text) == 0 && !checkEmailPat.Checked && !checkEmail2.Checked)
        {
            ShowError("There are no output methods selected.");
            return;
        }

        QtyPaperCopies = SIn.Int(textPaperCopies.Text);
        IsEmailPatOrLab = checkEmailPat.Checked;
        
        EmailPatOrLabAddress = textEmailPat.Text;

        if (IsEmail2Visible)
        {
            Email2 = checkEmail2.Checked;
            Email2Address = textEmail2.Text;
        }

        DialogResult = DialogResult.OK;
    }
}