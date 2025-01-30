using System;
using System.Windows.Forms;

namespace OpenDental.User_Controls.SetupWizard;

public partial class SetupWizControl : UserControl
{
    public bool IsDone = false;

    public string StrIncomplete = "Please fill in the missing fields first.";

    public delegate bool ControlValidated(object sender, EventArgs e);

    public ControlValidated OnControlValidated;

    public EventHandler OnControlDone;
}