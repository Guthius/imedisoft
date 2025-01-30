using System;

namespace OpenDental.User_Controls.SetupWizard;

public partial class UserControlSetupWizIntro : SetupWizControl
{
    public UserControlSetupWizIntro(string name, string descript)
    {
        InitializeComponent();

        labelTitle.Text += " " + name + "...";
        labelDesc.Text = descript;
        labelDesc.Text += "\r\n\r\nIf you do not want to set up your " + name + " at this time, click 'Skip' below.";
    }

    private void UserControlSetupWizIntro_Load(object sender, EventArgs e)
    {
        IsDone = true;
    }
}