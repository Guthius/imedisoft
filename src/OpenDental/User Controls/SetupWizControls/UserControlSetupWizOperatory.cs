using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.User_Controls.SetupWizard;

public partial class UserControlSetupWizOperatory : SetupWizControl
{
    private readonly List<Operatory> _operatories = Operatories.GetDeepCopy();
    private int _blink;

    public UserControlSetupWizOperatory()
    {
        InitializeComponent();
    }

    private void UserControlSetupWizOperatory_Load(object sender, EventArgs e)
    {
        FillGrid();

        if (Operatories.GetCount(true) != 0)
        {
            return;
        }
        
        MsgBox.Show("FormSetupWizard", "You have no valid operatories. Please click the Add button to add an operatory.");
        timer1.Start();
    }

    private void FillGrid()
    {
        var needsAttnCol = OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention);
        gridMain.BeginUpdate();
        gridMain.Columns.Clear();
        GridColumn col;
        col = new GridColumn(Lan.g("FormSetupWizard", "OpName"), 110);
        gridMain.Columns.Add(col);
        col = new GridColumn(Lan.g("FormSetupWizard", "Abbrev"), 110);
        gridMain.Columns.Add(col);
        col = new GridColumn(Lan.g("FormSetupWizard", "Clinic"), 110);
        gridMain.Columns.Add(col);
        col = new GridColumn(Lan.g("FormSetupWizard", "ProvDentist"), 110);
        gridMain.Columns.Add(col);
        col = new GridColumn(Lan.g("FormSetupWizard", "ProvHygienist"), 110);
        gridMain.Columns.Add(col);
        col = new GridColumn(Lan.g("FormSetupWizard", "IsHygiene"), 60, HorizontalAlignment.Center);
        gridMain.Columns.Add(col);
        col = new GridColumn(Lan.g("FormSetupWizard", "IsHidden"), 60, HorizontalAlignment.Center);
        gridMain.Columns.Add(col);
        //col = new ODGridColumn("Clinic",120);
        //gridMain.Columns.Add(col);
        gridMain.ListGridRows.Clear();
        GridRow row;
        var IsAllComplete = true;
        if (_operatories.Count == 0)
        {
            IsAllComplete = false;
        }

        foreach (var opCur in _operatories)
        {
            row = new GridRow();
            row.Cells.Add(opCur.OpName);
            if (string.IsNullOrEmpty(opCur.OpName))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(opCur.Abbrev);
            if (string.IsNullOrEmpty(opCur.Abbrev))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            if (true)
            {
                row.Cells.Add(Clinics.GetAbbr(opCur.ClinicNum));
            }

            //not a required field
            row.Cells.Add(Providers.GetAbbr(opCur.ProvDentist));
            //not a required field
            row.Cells.Add(Providers.GetAbbr(opCur.ProvHygienist));
            //not a required field
            row.Cells.Add(opCur.IsHygiene ? "X" : "");
            //not a required field
            row.Cells.Add(opCur.IsHidden ? "X" : "");
            //not a required field
            //row = new ODGridRow();
            //row.Cells.Add(opCur.OpName);
            //if(string.IsNullOrEmpty(opCur.OpName)) {
            //	row.Cells[row.Cells.Count-1].CellColor=needsAttnCol;
            //	IsAllComplete=false;
            //}
            row.Tag = opCur;
            gridMain.ListGridRows.Add(row);
        }

        gridMain.EndUpdate();
        if (IsAllComplete)
        {
            IsDone = true;
        }
        else
        {
            IsDone = false;
        }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (_blink > 5)
        {
            pictureAdd.Visible = true;
            timer1.Stop();
            return;
        }

        pictureAdd.Visible = !pictureAdd.Visible;
        _blink++;
    }

    private void butAdd_Click(object sender, EventArgs e)
    {
        using var FormOE = new FormOperatoryEdit(new Operatory());
        FormOE.IsNew = true;
        var listOld = new List<Operatory>();
        foreach (var op in _operatories)
        {
            listOld.Add(op.Copy());
        }

        FormOE.ListOperatories = _operatories;
        FormOE.ShowDialog();
        if (FormOE.DialogResult == DialogResult.OK)
        {
            Operatories.Sync(_operatories, listOld);
            FillGrid();
            DataValid.SetInvalid(InvalidType.Operatories);
        }
    }

    private void gridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var opCur = (Operatory) gridMain.ListGridRows[e.Row].Tag;
        using var FormOE = new FormOperatoryEdit(opCur);
        var listOld = new List<Operatory>();
        foreach (var op in _operatories)
        {
            listOld.Add(op.Copy());
        }

        FormOE.ListOperatories = _operatories;
        FormOE.ShowDialog();
        if (FormOE.DialogResult == DialogResult.OK)
        {
            Operatories.Sync(_operatories, listOld);
            FillGrid();
            DataValid.SetInvalid(InvalidType.Operatories);
        }
    }

    private void butAdvanced_Click(object sender, EventArgs e)
    {
        using var FormP = new FormOperatories();
        FormP.ShowDialog();
        FillGrid();
    }
}