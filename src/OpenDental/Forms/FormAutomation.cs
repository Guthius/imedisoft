using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutomation : FormODBase
{
    private bool _changed;
    private List<Automation> _automations;

    public FormAutomation()
    {
        InitializeComponent();
    }

    private void FormAutomation_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FormAutomation_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Automation);
        }
    }

    private void FillGrid()
    {
        Automations.RefreshCache();

        _automations = Automations.GetDeepCopy();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 200));
        gridMain.Columns.Add(new GridColumn("Trigger", 150));
        gridMain.Columns.Add(new GridColumn("Action", 150));
        gridMain.Columns.Add(new GridColumn("Details", 200));

        gridMain.ListGridRows.Clear();

        foreach (var automation in _automations)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(automation.Description);
            gridRow.Cells.Add(automation.Autotrigger == EnumAutomationTrigger.ProcedureComplete ? automation.ProcCodes : automation.Autotrigger.ToString());
            gridRow.Cells.Add(automation.AutoAction.ToString());

            var detail = automation.AutoAction switch
            {
                AutomationAction.CreateCommlog => Defs.GetName(DefCat.CommLogTypes, automation.CommType) + ".  " + automation.MessageContent,
                AutomationAction.PrintPatientLetter or AutomationAction.PrintReferralLetter => SheetDefs.GetDescription(automation.SheetDefNum),
                AutomationAction.ChangePatStatus => automation.PatStatus.GetDescription(),
                _ => ""
            };

            gridRow.Cells.Add(detail);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formAutomationEdit = new FormAutomationEdit(_automations[e.Row]);

        formAutomationEdit.ShowDialog();

        FillGrid();

        _changed = true;
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var automation = new Automation();

        Automations.Insert(automation);

        using var formAutomationEdit = new FormAutomationEdit(automation);

        if (formAutomationEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();

        _changed = true;
    }
}