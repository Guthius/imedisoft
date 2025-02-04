using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAllergySetup : FormODBase
{
    private List<AllergyDef> _allergyDefs;

    public bool IsSelectionMode { get; set; }
    public long SelectedAllergyDefNum { get; set; }

    public FormAllergySetup()
    {
        InitializeComponent();
    }

    private void FormAllergySetup_Load(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            butOK.Visible = false;
        }

        FillGrid();
    }

    private void FillGrid()
    {
        _allergyDefs = AllergyDefs.GetAll(checkShowHidden.Checked);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 160));
        gridMain.Columns.Add(new GridColumn("Hidden", 60));

        gridMain.ListGridRows.Clear();

        foreach (var allergyDef in _allergyDefs)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(allergyDef.Description);
            gridRow.Cells.Add(allergyDef.IsHidden ? "X" : "");

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void CheckBoxShowHidden_CheckedChanged(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (IsSelectionMode)
        {
            SelectedAllergyDefNum = _allergyDefs[e.Row].AllergyDefNum;

            DialogResult = DialogResult.OK;
        }
        else
        {
            using var formAllergyDefEdit = new FormAllergyDefEdit(_allergyDefs[gridMain.GetSelectedIndex()]);

            formAllergyDefEdit.ShowDialog();

            FillGrid();
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.AllergyDefEdit))
        {
            return;
        }

        var allergyDef = new AllergyDef
        {
            IsNew = true
        };

        using var formAllergyDefEdit = new FormAllergyDefEdit(allergyDef);

        formAllergyDefEdit.ShowDialog();

        FillGrid();
    }

    private void ButtonMerge_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.AllergyMerge))
        {
            return;
        }

        if (gridMain.SelectedGridRows.Count != 2)
        {
            ShowError("Select two allergies.");
            return;
        }

        if (!ConfirmOk("Combine the two selected allergies? This cannot be reversed."))
        {
            return;
        }

        var allergyDefs = new List<AllergyDef>
        {
            _allergyDefs[gridMain.SelectedIndices[0]],
            _allergyDefs[gridMain.SelectedIndices[1]]
        };

        try
        {
            AllergyDefs.Combine(allergyDefNumKeep: allergyDefs[0].AllergyDefNum, allergyDefNumCombine: allergyDefs[1].AllergyDefNum);
        }
        catch (ApplicationException ex)
        {
            ShowError(ex.Message);
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.AllergyMerge, 0, "Allergy with name " + allergyDefs[1].Description + " was merged with " + allergyDefs[0].Description);

        FillGrid();

        for (var i = 0; i < _allergyDefs.Count; i++)
        {
            if (_allergyDefs[i].AllergyDefNum == allergyDefs[0].AllergyDefNum)
            {
                gridMain.SetSelected(i);
            }
        }
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Select at least one allergy.");
            return;
        }

        if (gridMain.SelectedGridRows.Count > 1)
        {
            ShowError("Only select one allergy.");
            return;
        }

        SelectedAllergyDefNum = _allergyDefs[gridMain.GetSelectedIndex()].AllergyDefNum;

        DialogResult = DialogResult.OK;
    }
}