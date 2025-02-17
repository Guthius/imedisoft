using System;
using System.Windows.Forms;

namespace OpenDental.Forms;

public partial class FormInsBenefitNotes : FormODBase
{
    public string BenefitNotes { get; set; } = string.Empty;

    public FormInsBenefitNotes()
    {
        InitializeComponent();
    }

    private void FormInsBenefitNotes_Load(object sender, EventArgs e)
    {
        textBenefitNotes.Text = BenefitNotes;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        BenefitNotes = textBenefitNotes.Text;

        DialogResult = DialogResult.OK;
    }
}