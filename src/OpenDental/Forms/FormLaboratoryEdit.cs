using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormLaboratoryEdit : FormODBase
{
    private readonly Laboratory _laboratory;
    private List<LabTurnaround> _labTurnarounds;
    private List<SheetDef> _sheetDefs;

    public FormLaboratoryEdit(Laboratory laboratory)
    {
        _laboratory = laboratory;

        InitializeComponent();
    }

    private void FormLaboratoryEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = _laboratory.Description;
        textPhone.Text = _laboratory.Phone;
        textWirelessPhone.Text = _laboratory.WirelessPhone;
        textAddress.Text = _laboratory.Address;
        textCity.Text = _laboratory.City;
        textState.Text = _laboratory.State;
        textZip.Text = _laboratory.Zip;
        textEmail.Text = _laboratory.Email;
        textNotes.Text = _laboratory.Notes;

        _labTurnarounds = LabTurnarounds.GetForLab(_laboratory.LaboratoryNum);

        comboSlip.Items.Add("Default");
        comboSlip.SelectedIndex = 0;

        _sheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.LabSlip);
        for (var i = 0; i < _sheetDefs.Count; i++)
        {
            comboSlip.Items.Add(_sheetDefs[i].Description);
            if (_laboratory.Slip == _sheetDefs[i].SheetDefNum)
            {
                comboSlip.SelectedIndex = i + 1;
            }
        }

        checkIsHidden.Checked = _laboratory.IsHidden;

        FillGrid();
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn(Lan.g("TableLabTurnaround", "Service Description"), 300));
        gridMain.Columns.Add(new GridColumn(Lan.g("TableLabTurnaround", "Days Published"), 120));
        gridMain.Columns.Add(new GridColumn(Lan.g("TableLabTurnaround", "Actual Days"), 120));

        gridMain.ListGridRows.Clear();

        foreach (var labTurnaround in _labTurnarounds)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(labTurnaround.Description);
            gridRow.Cells.Add(labTurnaround.DaysPublished == 0 ? "" : labTurnaround.DaysPublished.ToString());
            gridRow.Cells.Add(labTurnaround.DaysActual.ToString());

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var labTurnaround = new LabTurnaround();

        using var formLabTurnaroundEdit = new FormLabTurnaroundEdit(labTurnaround);

        if (formLabTurnaroundEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _labTurnarounds.Add(labTurnaround);

        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formLabTurnaroundEdit = new FormLabTurnaroundEdit(_labTurnarounds[e.Row]);

        if (formLabTurnaroundEdit.ShowDialog() == DialogResult.OK)
        {
            FillGrid();
        }
    }

    private void ButtonDeleteTurnaround_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");

            return;
        }

        _labTurnarounds.RemoveAt(gridMain.GetSelectedIndex());

        FillGrid();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_laboratory.LaboratoryNum == 0)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (!ConfirmOk("Delete this entire Laboratory?"))
        {
            return;
        }

        try
        {
            Laboratories.Delete(_laboratory.LaboratoryNum);

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescription.Text == "")
        {
            ShowError("Description cannot be blank.");
            return;
        }

        _laboratory.Description = textDescription.Text;
        _laboratory.Phone = textPhone.Text;
        _laboratory.WirelessPhone = textWirelessPhone.Text;
        _laboratory.Address = textAddress.Text;
        _laboratory.City = textCity.Text;
        _laboratory.State = textState.Text;
        _laboratory.Zip = textZip.Text;
        _laboratory.Email = textEmail.Text;
        _laboratory.Notes = textNotes.Text;
        _laboratory.Slip = 0;
        _laboratory.IsHidden = checkIsHidden.Checked;

        if (comboSlip.SelectedIndex > 0)
        {
            _laboratory.Slip = _sheetDefs[comboSlip.SelectedIndex - 1].SheetDefNum;
        }

        if (_laboratory.LaboratoryNum == 0)
        {
            Laboratories.Insert(_laboratory);
        }
        else
        {
            Laboratories.Update(_laboratory);
        }

        LabTurnarounds.SetForLab(_laboratory.LaboratoryNum, _labTurnarounds);

        DialogResult = DialogResult.OK;
    }
}