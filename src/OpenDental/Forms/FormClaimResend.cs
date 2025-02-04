using System;
using System.Windows.Forms;

namespace OpenDental.Forms;

public partial class FormClaimResend : FormODBase
{
    public FormClaimResend()
    {
        InitializeComponent();
    }

    public bool IsClaimReplacement()
    {
        return radioClaimReplacement.Checked;
    }

    private void RadioButtonClaimOriginal_Click(object sender, EventArgs e)
    {
        radioClaimOriginal.Checked = true;
        radioClaimReplacement.Checked = false;
    }

    private void RadioButtonClaimReplacement_Click(object sender, EventArgs e)
    {
        radioClaimOriginal.Checked = false;
        radioClaimReplacement.Checked = true;
    }

    private void ButtonSend_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }
}