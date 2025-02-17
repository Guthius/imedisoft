using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormInsAdj : FormODBase
{
    private readonly ClaimProc _claimProc;
    private readonly ClaimProc _claimProcOld;

    public bool IsNew { get; set; }

    public FormInsAdj(ClaimProc claimProc)
    {
        _claimProc = claimProc;
        _claimProcOld = _claimProc.Copy();

        InitializeComponent();
    }

    private void FormInsAdj_Load(object sender, EventArgs e)
    {
        textDate.Text = _claimProc.ProcDate.ToShortDateString();
        textInsUsed.Text = _claimProc.InsPayAmt.ToString("F");
        textDedUsed.Text = _claimProc.DedApplied.ToString("F");
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (IsNew)
        {
            DialogResult = DialogResult.Cancel;

            return;
        }

        if (!ConfirmOk("Delete?"))
        {
            return;
        }

        ClaimProcs.Delete(_claimProc);

        InsEditPatLogs.MakeLogEntry(null, _claimProc, InsEditPatLogType.Adjustment);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textDate.IsValid() || !textInsUsed.IsValid() || !textDedUsed.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        _claimProc.ProcDate = SIn.Date(textDate.Text);
        _claimProc.InsPayAmt = SIn.Double(textInsUsed.Text);
        _claimProc.DedApplied = SIn.Double(textDedUsed.Text);

        if (IsNew)
        {
            ClaimProcs.Insert(_claimProc);

            InsEditPatLogs.MakeLogEntry(_claimProc, null, InsEditPatLogType.Adjustment);

            DialogResult = DialogResult.OK;
            return;
        }

        ClaimProcs.Update(_claimProc);

        InsEditPatLogs.MakeLogEntry(_claimProc, _claimProcOld, InsEditPatLogType.Adjustment);

        DialogResult = DialogResult.OK;
    }
}