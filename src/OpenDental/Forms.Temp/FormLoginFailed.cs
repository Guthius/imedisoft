using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormLoginFailed : FormODBase
{
    private readonly string _errorMessage;

    public FormLoginFailed(string errorMessage)
    {
        InitializeComponent();

        _errorMessage = errorMessage;
    }

    private void FormLoginFailed_Load(object sender, EventArgs e)
    {
        labelErrMsg.Text = _errorMessage;

        textUser.Text = Security.CurUser.UserName;
        textPassword.Focus();
    }

    private void ButtonLogin_Click(object sender, EventArgs e)
    {
        Userod user;

        var password = textPassword.Text;
        var username = textUser.Text;

        try
        {
            user = Userods.CheckUserAndPassword(username, password);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return;
        }

        Security.CurUser = user;
        Security.IsUserLoggedIn = true;

        if (PrefC.GetBool(PrefName.PasswordsMustBeStrong) && PrefC.GetBool(PrefName.PasswordsWeakChangeToStrong) && Userods.IsPasswordStrong(textPassword.Text) != "") //Password is not strong
        {
            ShowInfo("You must change your password to a strong password due to the current Security settings.");

            if (!SecurityL.ChangePassword(true))
            {
                return;
            }
        }

        SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserNum + " has logged on.");

        DialogResult = DialogResult.OK;
    }
}