using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptRules : FormODBase
{
    private bool _changed;
    private List<AppointmentRule> _appointmentRules;

    public FormApptRules()
    {
        InitializeComponent();
    }

    private void FormApptRules_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FormPayPeriods_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Views);
        }
    }

    private void FillGrid()
    {
        AppointmentRules.RefreshCache();

        _appointmentRules = AppointmentRules.GetDeepCopy();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 200));
        gridMain.Columns.Add(new GridColumn("Start Code", 100));
        gridMain.Columns.Add(new GridColumn("End Code", 100));
        gridMain.Columns.Add(new GridColumn("Enabled", 50, HorizontalAlignment.Center));
        gridMain.ListGridRows.Clear();

        foreach (var appointmentRule in _appointmentRules)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(appointmentRule.RuleDesc);
            gridRow.Cells.Add(appointmentRule.CodeStart);
            gridRow.Cells.Add(appointmentRule.CodeEnd);
            gridRow.Cells.Add(appointmentRule.IsEnabled ? "X" : "");

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var frmApptRuleEdit = new FrmApptRuleEdit(_appointmentRules[e.Row]);

        frmApptRuleEdit.ShowDialog();

        FillGrid();

        _changed = true;
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var appointmentRule = new AppointmentRule
        {
            IsEnabled = true
        };

        var frmApptRuleEdit = new FrmApptRuleEdit(appointmentRule)
        {
            IsNew = true
        };

        frmApptRuleEdit.ShowDialog();
        if (!frmApptRuleEdit.IsDialogOK)
        {
            return;
        }

        FillGrid();

        _changed = true;
    }
}