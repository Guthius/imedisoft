using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormUserPassword : FormODBase
{
    private readonly bool _isCreate;
    private readonly bool _isPasswordReset;
    private readonly bool _isCopiedUser;

    public bool IsInSecurityWindow { get; set; }
    public bool IsPasswordStrong { get; set; }
    public string PasswordTyped { get; set; } = string.Empty;
    public PasswordContainer Password { get; set; }

    public FormUserPassword(bool isCreate, string username, bool isPasswordReset = false, bool isCopiedUser = false)
    {
        InitializeComponent();

        _isCreate = isCreate;

        textUserName.Text = username;

        _isPasswordReset = isPasswordReset;
        _isCopiedUser = isCopiedUser;
    }

    private void FormUserPassword_Load(object sender, EventArgs e)
    {
        if (_isCreate)
        {
            Text = "Create Password";
        }

        if (_isCopiedUser)
        {
            Text = "Create Password for Copied User";
        }

        if (IsInSecurityWindow)
        {
            labelCurrent.Visible = false;
            textCurrent.Visible = false;
        }

        if (!_isPasswordReset)
        {
            return;
        }

        labelCurrent.Text = "New Password";
        labelNew.Text = "Re-Enter Password";
    }

    private void CheckBoxShow_Click(object sender, EventArgs e)
    {
        textPassword.PasswordChar = checkShow.Checked ? '\0' : '*';
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (_isPasswordReset)
        {
            if (textPassword.Text != textCurrent.Text || string.IsNullOrWhiteSpace(textPassword.Text))
            {
                ShowError("Passwords must match and not be empty.");
                return;
            }
        }
        else if (!IsInSecurityWindow && !Authentication.CheckPassword(Security.CurUser, textCurrent.Text))
        {
            ShowError("Current password incorrect.");
            return;
        }

        var explanation = Userods.IsPasswordStrong(textPassword.Text);
        if (PrefC.GetBool(PrefName.PasswordsMustBeStrong))
        {
            if (!string.IsNullOrEmpty(explanation))
            {
                ShowError(explanation);
                return;
            }
        }

        IsPasswordStrong = string.IsNullOrEmpty(explanation);
        Password = Authentication.GenerateLoginDetailsSha512(textPassword.Text);

        PasswordTyped = textPassword.Text;

        DialogResult = DialogResult.OK;
    }
}