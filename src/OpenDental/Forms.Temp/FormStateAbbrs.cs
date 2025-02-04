using System;
using System.ComponentModel;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormStateAbbrs : FormODBase
{
    private bool _changed;

    public bool IsSelectionMode { get; set; }
    public StateAbbr SelectedStateAbbr { get; set; }

    public FormStateAbbrs()
    {
        InitializeComponent();
    }

    private void FormStateAbbrs_Load(object sender, EventArgs e)
    {
        if (IsSelectionMode)
        {
            butAdd.Visible = false;
        }

        if (PrefC.GetBool(PrefName.EnforceMedicaidIDLength))
        {
            Width += 100;
        }

        FillGrid();
    }

    private void FormStateAbbrs_Closing(object sender, CancelEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.StateAbbrs);
        }
    }

    private void FillGrid()
    {
        long previousSelectedStateAbbrNum = -1;
        
        var newSelectedIndex = -1;
        if (gridMain.GetSelectedIndex() != -1)
        {
            previousSelectedStateAbbrNum = ((StateAbbr) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag).StateAbbrNum;
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 175));
        gridMain.Columns.Add(new GridColumn("Abbr", 70));

        if (PrefC.GetBool(PrefName.EnforceMedicaidIDLength))
        {
            gridMain.Columns.Add(new GridColumn("Medicaid ID Length", 200));
        }

        gridMain.ListGridRows.Clear();

        var stateAbbrs = StateAbbrs.GetDeepCopy();
        for (var i = 0; i < stateAbbrs.Count; i++)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(stateAbbrs[i].Description);
            gridRow.Cells.Add(stateAbbrs[i].Abbr);

            if (PrefC.GetBool(PrefName.EnforceMedicaidIDLength))
            {
                gridRow.Cells.Add(stateAbbrs[i].MedicaidIDLength == 0 ? "" : stateAbbrs[i].MedicaidIDLength.ToString());
            }

            gridRow.Tag = stateAbbrs[i];

            gridMain.ListGridRows.Add(gridRow);

            if (stateAbbrs[i].StateAbbrNum == previousSelectedStateAbbrNum)
            {
                newSelectedIndex = i;
            }
        }

        gridMain.EndUpdate();
        gridMain.SetSelected(newSelectedIndex);
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "StateAbbrs");

        var stateAbbr = new StateAbbr
        {
            IsNew = true
        };

        using var formStateAbbrEdit = new FormStateAbbrEdit(stateAbbr);

        if (formStateAbbrEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        Cache.Refresh(InvalidType.StateAbbrs);

        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "StateAbbrs");

        if (IsSelectionMode)
        {
            SelectedStateAbbr = (StateAbbr) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
            DialogResult = DialogResult.OK;
            return;
        }

        using var formStateAbbrEdit = new FormStateAbbrEdit((StateAbbr) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag);

        if (formStateAbbrEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        Cache.Refresh(InvalidType.StateAbbrs);

        FillGrid();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            DialogResult = DialogResult.OK;
            return;
        }

        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select a state.");
            return;
        }

        SelectedStateAbbr = (StateAbbr) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;

        DialogResult = DialogResult.OK;
    }
}