using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using OpenDental.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormOperatories : FormODBase
{
    private List<Operatory> _operatories;
    private List<Operatory> _operatoriesOld;

    ///<summary>List of conflict appointments to show the user.
    ///Only used for the combine operatories tool</summary>
    public List<Appointment> ListAppointmentsConflicting = [];

    ///<summary>This reference is passed in because it's needed for the "Update Provs on Future Appts" tool.</summary>
    public ControlAppt ControlApptRef;

    public FormOperatories()
    {
        InitializeComponent();
    }

    private void FormOperatories_Load(object sender, EventArgs e)
    {
        RefreshList();
    }

    private void RefreshList()
    {
        Cache.Refresh(InvalidType.Operatories);

        _operatories = Operatories.GetDeepCopy();
        _operatoriesOld = _operatories.Select(x => x.Copy()).ToList();

        FillGrid();
    }

    private void FillGrid()
    {
        var selectedOpNums = gridMain.SelectedTags<Operatory>().Select(x => x.OperatoryNum).ToList();

        var scrollValue = gridMain.ScrollValue;

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Op Name", 180));
        gridMain.Columns.Add(new GridColumn("Abbrev", 70));
        gridMain.Columns.Add(new GridColumn("IsHidden", 64, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Clinic", 85));
        gridMain.Columns.Add(new GridColumn("Provider", 70));
        gridMain.Columns.Add(new GridColumn("Hygienist", 70));
        gridMain.Columns.Add(new GridColumn("IsHygiene", 64, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("IsWebSched", 74, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("IsNewPat", 75, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("IsExistPat", 75, HorizontalAlignment.Center) {IsWidthDynamic = true});
        gridMain.ListGridRows.Clear();

        foreach (var operatory in _operatories)
        {
            if (!comboClinic.IsAllSelected && operatory.ClinicNum != comboClinic.ClinicNumSelected)
            {
                continue;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(operatory.OpName);
            gridRow.Cells.Add(operatory.Abbrev);
            gridRow.Cells.Add(operatory.IsHidden ? "X" : "");
            gridRow.Cells.Add(Clinics.GetAbbr(operatory.ClinicNum));
            gridRow.Cells.Add(Providers.GetAbbr(operatory.ProvDentist));
            gridRow.Cells.Add(Providers.GetAbbr(operatory.ProvHygienist));
            gridRow.Cells.Add(operatory.IsHygiene ? "X" : "");
            gridRow.Cells.Add(operatory.IsWebSched ? "X" : "");
            gridRow.Cells.Add("");
            gridRow.Cells.Add("");
            gridRow.Tag = operatory;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();

        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            var operatory = (Operatory) gridMain.ListGridRows[i].Tag;
            if (selectedOpNums.Contains(operatory.OperatoryNum))
            {
                gridMain.SetSelected(i);
            }
        }

        gridMain.ScrollValue = scrollValue;
    }

    private void gridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formOperatoryEdit = new FormOperatoryEdit((Operatory) gridMain.ListGridRows[e.Row].Tag);
        formOperatoryEdit.ListOperatories = _operatories;
        formOperatoryEdit.ControlApptRef = ControlApptRef;
        formOperatoryEdit.ShowDialog();
        FillGrid();
    }

    private void comboClinic_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void butAdd_Click(object sender, EventArgs e)
    {
        var operatory = new Operatory
        {
            IsNew = true,
            ItemOrder = gridMain.SelectedIndices.Length > 0 ? gridMain.SelectedIndices[0] : _operatories.Count
        };

        if (!comboClinic.IsAllSelected && !comboClinic.IsNothingSelected)
        {
            operatory.ClinicNum = comboClinic.ClinicNumSelected;
            operatory.IsInHQView = false;
        }

        using var formOperatoryEdit = new FormOperatoryEdit(operatory);

        formOperatoryEdit.ListOperatories = _operatories;
        formOperatoryEdit.IsNew = true;

        if (formOperatoryEdit.ShowDialog() == DialogResult.Cancel)
        {
            return;
        }

        FillGrid();
    }

    private void butCombine_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        if (gridMain.SelectedIndices.Length < 2)
        {
            ShowError("Please select multiple items first while holding down the control key.");
            return;
        }

        if (!ConfirmOk(
                "Combine all selected operatories into a single operatory?\r\n\r\n" +
                "This will affect all appointments set in these operatories and could take a while to run. " +
                "The next window will let you select which operatory to keep when combining."))
        {
            return;
        }
        
        var hasNewOp = gridMain.SelectedIndices.ToList().Exists(x => ((Operatory) gridMain.ListGridRows[x].Tag).IsNew);
        if (hasNewOp)
        {
            ReorderAndSync();
            
            DataValid.SetInvalid(InvalidType.Operatories);
            
            RefreshList();
        }

        var selectedOpNums = gridMain.SelectedIndices.Select(index => ((Operatory) gridMain.ListGridRows[index].Tag).OperatoryNum).ToList();

        using var formOperatoryPick = new FormOperatoryPick(_operatories.FindAll(x => selectedOpNums.Contains(x.OperatoryNum)));
        formOperatoryPick.ShowDialog();
        if (formOperatoryPick.DialogResult != DialogResult.OK)
        {
            return;
        }

        var opNumMaster = formOperatoryPick.SelectedOperatoryNum;

        var listAppointmentsToMerge = Operatories.MergeApptCheck(opNumMaster, selectedOpNums.FindAll(x => x != opNumMaster));
        if (!PrefC.GetBool(PrefName.ApptsAllowOverlap))
        {
            ListAppointmentsConflicting = listAppointmentsToMerge.FindAll(x => x.Op != opNumMaster && Operatories.HasConflict(x, listAppointmentsToMerge));
            if (ListAppointmentsConflicting.Count > 0)
            {
                if (!ConfirmOk("Cannot merge operatories due to appointment conflicts.\r\n\r\n" +
                               "These conflicts need to be resolved before combining can occur.\r\n" +
                               "Click OK to view the conflicting appointments."))
                {
                    ListAppointmentsConflicting.Clear();
                    return;
                }

                Close();

                return;
            }
        }

        var apptCount = listAppointmentsToMerge.FindAll(x => x.Op != opNumMaster).Count;
        if (apptCount > 0)
        {
            var selectedOpName = _operatories.First(x => x.OperatoryNum == opNumMaster).Abbrev;

            if (!Confirm("Would you like to move " + apptCount + " appointments from their current operatories to " + selectedOpName + "?\r\n\r\n" +
                         "You cannot undo this!"))
            {
                return;
            }
        }

        if (!hasNewOp)
        {
            ReorderAndSync();

            DataValid.SetInvalid(InvalidType.Operatories);
        }

        try
        {
            Operatories.MergeOperatoriesIntoMaster(opNumMaster, selectedOpNums, listAppointmentsToMerge);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return;
        }

        ShowInfo(
            "The following operatories and all of their appointments were merged into the " +
            _operatories.FirstOrDefault(x => x.OperatoryNum == opNumMaster).Abbrev + " operatory:\r\n" +
            string.Join(", ", _operatories.FindAll(x => x.OperatoryNum != opNumMaster && selectedOpNums.Contains(x.OperatoryNum)).Select(x => x.Abbrev)));

        RefreshList();
        FillGrid();
    }

    private void butUp_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length == 0)
        {
            ShowError("You must first select a row.");
            return;
        }

        var selectedIndex = gridMain.GetSelectedIndex();
        if (selectedIndex == 0)
        {
            return;
        }

        var operatorySelected = (Operatory) gridMain.ListGridRows[selectedIndex].Tag;
        var operatoryAboveSelected = (Operatory) gridMain.ListGridRows[selectedIndex - 1].Tag;
        if (!CanReorderOps(operatorySelected, operatoryAboveSelected))
        {
            return;
        }

        (operatorySelected.ItemOrder, operatoryAboveSelected.ItemOrder) = (operatoryAboveSelected.ItemOrder, operatorySelected.ItemOrder);

        _operatories = _operatories.OrderBy(x => x.ItemOrder).ToList();

        SwapGridMainLocations(selectedIndex, selectedIndex - 1);

        gridMain.SetSelected(selectedIndex - 1);
    }

    private void butDown_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length == 0)
        {
            ShowError("You must first select a row.");
            return;
        }

        var selectedIndex = gridMain.GetSelectedIndex();
        if (selectedIndex == gridMain.ListGridRows.Count - 1)
        {
            return;
        }

        var operatorySelected = (Operatory) gridMain.ListGridRows[selectedIndex].Tag;
        var operatoryBelowSelected = (Operatory) gridMain.ListGridRows[selectedIndex + 1].Tag;
        if (!CanReorderOps(operatorySelected, operatoryBelowSelected))
        {
            return;
        }

        (operatorySelected.ItemOrder, operatoryBelowSelected.ItemOrder) = (operatoryBelowSelected.ItemOrder, operatorySelected.ItemOrder);

        _operatories = _operatories.OrderBy(x => x.ItemOrder).ToList();

        SwapGridMainLocations(selectedIndex, selectedIndex + 1);

        gridMain.SetSelected(selectedIndex + 1);
    }

    private bool CanReorderOps(Operatory operatory1, Operatory operatory2)
    {
        if (comboClinic.IsAllSelected)
        {
            return true;
        }

        if (!operatory1.IsInHQView && !operatory2.IsInHQView)
        {
            return true;
        }

        ShowError(
            "You cannot change the order of the Operatories '" + operatory1.Abbrev + "' and '" + operatory2.Abbrev + "' " +
            "with Clinic '" + comboClinic.GetSelectedAbbr() + "' selected because it is also a member of a Headquarters Appointment View. " +
            "You must set your clinic selection to 'All' to reorder these operatories.");

        return false;
    }

    private void SwapGridMainLocations(int indxMoveFrom, int indxMoveTo)
    {
        gridMain.BeginUpdate();

        var dataRow = gridMain.ListGridRows[indxMoveFrom];

        gridMain.ListGridRows.RemoveAt(indxMoveFrom);
        gridMain.ListGridRows.Insert(indxMoveTo, dataRow);
        gridMain.EndUpdate();
    }

    private void ReorderAndSync()
    {
        for (var i = 0; i < _operatories.Count; i++)
        {
            _operatories[i].ItemOrder = i;
        }

        Operatories.Sync(_operatories, _operatoriesOld);
    }

    private void FormOperatories_Closing(object sender, CancelEventArgs e)
    {
        ReorderAndSync();

        DataValid.SetInvalid(InvalidType.Operatories);
    }
}