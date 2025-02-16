using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormPayPlanSelect : FormODBase
{
    private readonly List<PayPlan> _validPayPlans;
    private readonly bool _includeNone;

    private List<PayPlanCharge> _payPlanCharges;

    public long SelectedPayPlanNum { get; set; }

    public FormPayPlanSelect(List<PayPlan> validPayPlans, bool includeNone = false)
    {
        _validPayPlans = validPayPlans;
        _includeNone = includeNone;

        InitializeComponent();
    }

    private void FormPayPlanSelect_Load(object sender, EventArgs e)
    {
        if (_includeNone)
        {
            Text = "Attach to payment plan?";
            labelExpl.Visible = true;
            butNone.Visible = true;
        }

        _payPlanCharges = PayPlanCharges.GetForPayPlans(_validPayPlans.Select(x => x.PayPlanNum).ToList());

        FillGrid();

        gridMain.SetSelected(0);
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date", 70));
        gridMain.Columns.Add(new GridColumn("Patient", 100));
        gridMain.Columns.Add(new GridColumn("Category", 80));
        gridMain.Columns.Add(new GridColumn("Total Cost", 80));
        gridMain.Columns.Add(new GridColumn("Balance", 80));
        gridMain.Columns.Add(new GridColumn("Due Now", 80));
        gridMain.Columns.Add(new GridColumn("Active", 80, HorizontalAlignment.Center));

        var payPlanNums = _validPayPlans.Select(x => x.PayPlanNum).ToList();
        var paySplits = PaySplits.GetForPayPlans(payPlanNums);
        var patients = Patients.GetLimForPats(_validPayPlans.Select(x => x.PatNum).ToList());

        gridMain.ListGridRows.Clear();

        foreach (var payPlan in _validPayPlans)
        {
            if (checkShowActiveOnly.Checked && payPlan.IsClosed)
            {
                continue;
            }

            var patient = patients.FirstOrDefault(x => x.PatNum == payPlan.PatNum);

            var gridRow = new GridRow();

            gridRow.Cells.Add(payPlan.PayPlanDate.ToShortDateString());
            gridRow.Cells.Add(patient?.LName + ", " + patient?.FName);
            gridRow.Cells.Add(payPlan.PlanCategory == 0 ? "None" : Defs.GetDef(DefCat.PayPlanCategories, payPlan.PlanCategory).ItemName);
            gridRow.Cells.Add(PayPlans.GetTotalCost(payPlan.PayPlanNum, _payPlanCharges).ToString("F"));
            gridRow.Cells.Add(PayPlans.GetBalance(payPlan.PayPlanNum, _payPlanCharges, paySplits).ToString("F"));
            gridRow.Cells.Add(PayPlans.GetDueNow(payPlan.PayPlanNum, _payPlanCharges, paySplits).ToString("F"));
            gridRow.Cells.Add(payPlan.IsClosed ? "" : "X");
            gridRow.Tag = payPlan.PayPlanNum;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            return;
        }

        SelectedPayPlanNum = (long) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;

        DialogResult = DialogResult.OK;
    }

    private void ButtonNone_Click(object sender, EventArgs e)
    {
        SelectedPayPlanNum = 0;

        DialogResult = DialogResult.OK;
    }

    private void GridMain_KeyDown(object sender, KeyEventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1 || e.KeyCode != Keys.Enter)
        {
            return;
        }

        SelectedPayPlanNum = (long) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;

        DialogResult = DialogResult.OK;
    }

    private void checkShowActiveOnly_MouseClick(object sender, MouseEventArgs e)
    {
        FillGrid();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select a payment plan first.");
            return;
        }

        SelectedPayPlanNum = (long) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;

        DialogResult = DialogResult.OK;
    }
}