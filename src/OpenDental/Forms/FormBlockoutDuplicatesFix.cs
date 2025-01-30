using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormBlockoutDuplicatesFix : FormODBase
{
    public FormBlockoutDuplicatesFix()
    {
        InitializeComponent();
    }

    private void FormBlockoutDuplicatesFix_Load(object sender, EventArgs e)
    {
        FillLabels();

        Cursor = Cursors.Default;
    }

    private void FillLabels()
    {
        labelCount.Text = Schedules.GetDuplicateBlockoutCount().ToString();
        labelInstructions.Text = labelCount.Text == "0" ? "" : "Click the Clear button to fix the duplicates.";
    }

    private void ButtonClear_Click(object sender, EventArgs e)
    {
        if (labelCount.Text == "0")
        {
            ShowError("There are no duplicates to clear.");
            return;
        }

        if (!ConfirmOk("Clear all duplicates?"))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;

        Schedules.ClearDuplicates();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Clear duplicate blockouts.");

        Cursor = Cursors.Default;

        ShowInfo("Done.");

        FillLabels();
    }
}