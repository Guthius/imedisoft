using System;
using System.Windows.Forms;

namespace OpenDental.Forms;

public partial class FormSheetImportEnumPicker : FormODBase
{
    public bool ShowClearButton;

    public FormSheetImportEnumPicker(string prompt)
    {
        InitializeComponent();

        labelPrompt.Text = prompt;
    }

    private void FormSheetImportEnumPicker_Load(object sender, EventArgs e)
    {
        if (!ShowClearButton)
        {
            butClear.Visible = false;
        }
    }

    private void ButtonClear_Click(object sender, EventArgs e)
    {
        listResult.SelectedIndex = -1;
        DialogResult = DialogResult.OK;
    }

    private void ListBoxResult_DoubleClick(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }
}