using System;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.User_Controls.SetupWizard;

public partial class UserControlSetupWizRegKey : SetupWizControl
{
    public UserControlSetupWizRegKey()
    {
        InitializeComponent();
    }

    private void UserControlSetupWizRegKey_Load(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.SecurityAdmin, true))
        {
            butChangeRegKey.Enabled = false;
        }

        FillControls();
    }

    private void FillControls()
    {
        var regkey = PrefC.GetString(PrefName.RegistrationKey);
        if (regkey.Length == 16)
        {
            textRegKey.Text = regkey.Substring(0, 4) + "-" + regkey.Substring(4, 4) + "-" + regkey.Substring(8, 4) + "-" + regkey.Substring(12, 4);
        }
        else
        {
            textRegKey.Text = regkey;
        }

        IsDone = !string.IsNullOrEmpty(textRegKey.Text);
        StrIncomplete = "Please click the 'Change' button and type in your registration key.";
        groupProcTools.Enabled = IsDone;
    }

    private void butProcCodeTools_Click(object sender, EventArgs e)
    {
        using var formProcTools = new FormProcTools();

        formProcTools.ShowDialog();
    }

    private void butChangeRegKey_Click(object sender, EventArgs e)
    {
        using var formRegistrationKey = new FormRegistrationKey();

        formRegistrationKey.ShowDialog();

        DataValid.SetInvalid(InvalidType.Prefs);

        var regkey = PrefC.GetString(PrefName.RegistrationKey);
        if (regkey.Length == 16)
        {
            textRegKey.Text = regkey.Substring(0, 4) + "-" + regkey.Substring(4, 4) + "-" + regkey.Substring(8, 4) + "-" + regkey.Substring(12, 4);
        }
        else
        {
            textRegKey.Text = regkey;
        }

        IsDone = !string.IsNullOrEmpty(textRegKey.Text);
        groupProcTools.Enabled = IsDone;
    }

    private void butAdvanced_Click(object sender, EventArgs e)
    {
        using var formUpdateSetup = new FormUpdateSetup();

        formUpdateSetup.ShowDialog();

        FillControls();
    }
}