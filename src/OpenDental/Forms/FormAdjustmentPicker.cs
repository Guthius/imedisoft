using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAdjustmentPicker : FormODBase
{
    private readonly long _patNum;
    private readonly bool _unattachedMode;
    private readonly long _clinicNum;
    private readonly long _provNum;
    private List<Adjustment> _adjustments;
    private List<Adjustment> _adjustmentsFiltered;

    public Adjustment SelectedAdjustment { get; set; }

    public FormAdjustmentPicker(long patNum, bool unattachedMode = false, List<Adjustment> adjustments = null, long clinicNum = -1, long provNum = 0)
    {
        _patNum = patNum;
        _unattachedMode = unattachedMode;
        _adjustments = adjustments;
        _clinicNum = clinicNum;
        _provNum = provNum;

        InitializeComponent();
    }

    private void FormAdjustmentPicker_Load(object sender, EventArgs e)
    {
        if (_unattachedMode)
        {
            checkUnattached.Checked = true;
            checkUnattached.Enabled = false;
        }

        _adjustments ??= Adjustments.Refresh(_patNum).ToList();


        var paySplits = PaySplits.GetForAdjustments(_adjustments.Select(x => x.AdjNum).ToList());

        _adjustments.RemoveAll(x => paySplits.Exists(y => y.AdjNum == x.AdjNum));

        var payPlanAdjNums = PayPlanLinks.GetListForLinkTypeAndFKeys(PayPlanLinkType.Adjustment, _adjustments.Select(x => x.AdjNum).ToList());

        _adjustments.RemoveAll(x => payPlanAdjNums.Contains(x.AdjNum));

        FillGrid();
    }

    private void FillGrid()
    {
        _adjustmentsFiltered = _adjustments;
        if (checkUnattached.Checked)
        {
            _adjustmentsFiltered = _adjustments.FindAll(x => x.ProcNum == 0);
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date", 75));
        gridMain.Columns.Add(new GridColumn("PatNum", 65));
        gridMain.Columns.Add(new GridColumn("Provider", 75));
        gridMain.Columns.Add(new GridColumn("Clinic", 100));
        gridMain.Columns.Add(new GridColumn("Type", 120));
        gridMain.Columns.Add(new GridColumn("Amount", 100));

        if (!_unattachedMode)
        {
            gridMain.Columns.Add(new GridColumn("Has Proc", 0, HorizontalAlignment.Center));
        }

        gridMain.ListGridRows.Clear();

        foreach (var adjustment in _adjustmentsFiltered)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(adjustment.AdjDate.ToShortDateString());
            gridRow.Cells.Add(adjustment.PatNum.ToString());
            gridRow.Cells.Add(Providers.GetAbbr(adjustment.ProvNum));
            gridRow.Cells.Add(Clinics.GetAbbr(adjustment.ClinicNum));
            gridRow.Cells.Add(Defs.GetName(DefCat.AdjTypes, adjustment.AdjType));
            gridRow.Cells.Add(adjustment.AdjAmt.ToString("F"));

            var attachedProc = "";
            if (adjustment.ProcNum != 0)
            {
                attachedProc = "X";
            }

            if (!_unattachedMode)
            {
                gridRow.Cells.Add(attachedProc);
            }

            gridRow.Tag = adjustment;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private bool IsSameProvClinic(Adjustment adjustment)
    {
        if (_provNum == 0 && _clinicNum == -1)
        {
            return true;
        }

        if (adjustment.ProvNum == _provNum && _clinicNum == -1)
        {
            return true;
        }

        return adjustment.ProvNum == _provNum && adjustment.ClinicNum == _clinicNum;
    }

    private void CheckBoxUnattached_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedAdjustment = _adjustmentsFiltered[gridMain.GetSelectedIndex()];

        if (!IsSameProvClinic(SelectedAdjustment))
        {
            if (!Confirm("The selected adjustment's provider and/or clinic do not match the procedure. Would you like to continue?"))
            {
                return;
            }
        }

        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        var index = gridMain.GetSelectedIndex();
        if (index < 0)
        {
            ShowError("You must select an adjustment.");
            return;
        }

        SelectedAdjustment = _adjustmentsFiltered[index];

        if (!IsSameProvClinic(SelectedAdjustment))
        {
            if (!Confirm("The selected adjustment's provider and/or clinic do not match the procedure. Would you like to continue?"))
            {
                return;
            }
        }

        DialogResult = DialogResult.OK;
    }
}