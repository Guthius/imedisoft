using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormPayorTypeEdit : FormODBase
{
    private readonly PayorType _payorType;
    private readonly List<Sop> _sops;
    private int _selectedIndex;

    public FormPayorTypeEdit(PayorType payorType)
    {
        _payorType = payorType;
        _sops = Sops.GetDeepCopy();

        InitializeComponent();
    }

    private void FormPayorTypeEdit_Load(object sender, EventArgs e)
    {
        _selectedIndex = -1;

        for (var i = 0; i < _sops.Count; i++)
        {
            comboSopCode.Items.Add(_sops[i].SopCode + " - " + _sops[i].Description);

            if (_payorType.SopCode == _sops[i].SopCode)
            {
                comboSopCode.SelectedIndex = i;
            }
        }

        _selectedIndex = comboSopCode.SelectedIndex;

        textDate.Text = _payorType.DateStart.ToShortDateString();
        textNote.Text = _payorType.Note;
    }

    private void ComboBoxSopCode_SelectionChangeCommitted(object sender, EventArgs e)
    {
        _selectedIndex = comboSopCode.SelectedIndex;

        comboSopCode.Items.Clear();

        foreach (var sop in _sops)
        {
            comboSopCode.Items.Add(sop.SopCode + " - " + sop.Description);
        }

        comboSopCode.SelectedIndex = _selectedIndex;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_payorType.PayorTypeNum == 0)
        {
            DialogResult = DialogResult.Cancel;

            return;
        }

        if (!ConfirmOk("Delete entry?"))
        {
            return;
        }

        PayorTypes.Delete(_payorType.PayorTypeNum);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textDate.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        if (textDate.Text == "")
        {
            ShowError("Please enter a date.");
            return;
        }

        if (comboSopCode.SelectedIndex == -1)
        {
            ShowError("Please select an Sop Code.");
            return;
        }

        var payorTypes = PayorTypes.GetPatientData(_payorType.PatNum);
        foreach (var payorType in payorTypes)
        {
            if (payorType.PayorTypeNum == _payorType.PayorTypeNum)
            {
                continue;
            }

            if (payorType.DateStart != SIn.Date(textDate.Text))
            {
                continue;
            }

            ShowError(
                "There is already a payor type with the selected start date. " +
                "Either change the date of this payor type or edit the existing payor type with this date.");

            return;
        }

        _payorType.SopCode = _sops[comboSopCode.SelectedIndex].SopCode;
        _payorType.Note = textNote.Text;
        _payorType.DateStart = SIn.Date(textDate.Text);

        if (_payorType.PayorTypeNum == 0)
        {
            PayorTypes.Insert(_payorType);
        }
        else
        {
            PayorTypes.Update(_payorType);
        }

        DialogResult = DialogResult.OK;
    }
}