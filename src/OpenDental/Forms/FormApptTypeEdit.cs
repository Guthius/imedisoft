using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptTypeEdit : FormODBase
{
    private bool _isMouseDown;
    private Point _pointMouseOrigin;
    private Point _pointSliderOrigin;
    private StringBuilder _stringBuilderTime;
    private List<ProcedureCode> _procedureCodes;
    private List<ProcedureCode> _procedureCodesRequired;

    public AppointmentType AppointmentTypeCur;

    public FormApptTypeEdit()
    {
        InitializeComponent();
    }

    private void FormApptTypeEdit_Load(object sender, EventArgs e)
    {
        textName.Text = AppointmentTypeCur.AppointmentTypeName;
        butColor.BackColor = AppointmentTypeCur.AppointmentTypeColor;
        checkIsHidden.Checked = AppointmentTypeCur.IsHidden;

        _procedureCodes = ProcedureCodes.GetFromCommaDelimitedList(AppointmentTypeCur.CodeStr);
        _procedureCodesRequired = ProcedureCodes.GetFromCommaDelimitedList(AppointmentTypeCur.CodeStrRequired);

        _stringBuilderTime = new StringBuilder();

        if (AppointmentTypeCur.Pattern != null)
        {
            _stringBuilderTime = new StringBuilder(Appointments.ConvertPatternFrom5(AppointmentTypeCur.Pattern));
        }

        AppointmentTypeCur.BlockoutTypes ??= "";

        var blockoutTypes = AppointmentTypeCur.BlockoutTypes.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        var blockoutTypeDefNums = blockoutTypes.Select(x => SIn.Long(x, throwExceptions: false)).ToList();
        var blockoutTypeDefs = Defs.GetDefsForCategory(DefCat.BlockoutTypes);
        var blockoutTypesUnrestrictedDefs = blockoutTypeDefs.FindAll(x => !x.ItemValue.Contains(BlockoutType.NoSchedule.GetDescription()));

        listBoxBlockoutTypes.Items.AddList(blockoutTypesUnrestrictedDefs, x => x.ItemName);

        for (var i = 0; i < listBoxBlockoutTypes.Items.Count; i++)
        {
            var def = (Def) listBoxBlockoutTypes.Items.GetObjectAt(i);
            if (blockoutTypeDefNums.Contains(def.DefNum))
            {
                listBoxBlockoutTypes.SelectedIndices.Add(i);
            }
        }

        switch (AppointmentTypeCur.RequiredProcCodesNeeded)
        {
            case EnumRequiredProcCodesNeeded.AtLeastOne:
                radioButtonAtLeastOne.Checked = true;
                break;

            case EnumRequiredProcCodesNeeded.All:
                radioButtonAll.Checked = true;
                break;
        }

        FillTime();
        RefreshListBoxProcCodes();
        RefreshListBoxProcCodesRequired();
    }

    private void ButtonColor_Click(object sender, EventArgs e)
    {
        using var colorDialog = new ColorDialog();

        colorDialog.Color = butColor.BackColor;
        colorDialog.ShowDialog();

        butColor.BackColor = colorDialog.Color;
    }

    private void ButtonColorClear_Click(object sender, EventArgs e)
    {
        butColor.BackColor = Color.FromArgb(0);
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (AppointmentTypeCur.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        var message = AppointmentTypes.CheckInUse(AppointmentTypeCur.AppointmentTypeNum);
        if (!string.IsNullOrWhiteSpace(message))
        {
            ShowError(message);
            return;
        }

        if (!ConfirmOk("Delete Appointment Type?"))
        {
            return;
        }

        AppointmentTypeCur = null;
        DialogResult = DialogResult.OK;
    }

    private void ButtonSlider_MouseUp(object sender, MouseEventArgs e)
    {
        _isMouseDown = false;
    }

    private void ButtonSlider_MouseDown(object sender, MouseEventArgs e)
    {
        _isMouseDown = true;
        _pointMouseOrigin = new Point(e.X + butSlider.Location.X, e.Y + butSlider.Location.Y);
        _pointSliderOrigin = butSlider.Location;
    }

    private void ButtonSlider_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDown)
        {
            return;
        }

        var point = _pointSliderOrigin with {Y = _pointSliderOrigin.Y + e.Y + butSlider.Location.Y - _pointMouseOrigin.Y};
        var step = (int) Math.Round((decimal) (point.Y - tbTime.Location.Y) / 14);
        if (step == _stringBuilderTime.Length)
        {
            return;
        }

        if (step < 1)
        {
            return;
        }

        if (step > tbTime.MaxRows - 1)
        {
            return;
        }

        if (step > _stringBuilderTime.Length)
        {
            _stringBuilderTime.Append('/');
        }

        if (step < _stringBuilderTime.Length)
        {
            _stringBuilderTime.Remove(step, 1);
        }

        FillTime();
    }

    private void FillTime()
    {
        for (var i = 0; i < _stringBuilderTime.Length; i++)
        {
            tbTime.Cell[0, i] = _stringBuilderTime.ToString(i, 1);
            tbTime.BackGColor[0, i] = Color.White;
        }

        for (var i = _stringBuilderTime.Length; i < tbTime.MaxRows; i++)
        {
            tbTime.Cell[0, i] = "";
            tbTime.BackGColor[0, i] = Color.FromName("Control");
        }

        tbTime.Refresh();

        butSlider.Location = new Point(tbTime.Location.X + 2, tbTime.Location.Y + _stringBuilderTime.Length * 14 + 1);

        textTime.Text = _stringBuilderTime.Length > 0 ? (_stringBuilderTime.Length * PrefC.GetInt(PrefName.AppointmentTimeIncrement)).ToString() : "Use procedure time pattern";
    }

    private void TimeBarTime_CellClicked(object sender, CellEventArgs e)
    {
        if (e.Row < _stringBuilderTime.Length)
        {
            if (_stringBuilderTime[e.Row] == '/')
            {
                _stringBuilderTime.Replace('/', 'X', e.Row, 1);
            }
            else
            {
                _stringBuilderTime.Replace(_stringBuilderTime[e.Row], '/', e.Row, 1);
            }
        }

        FillTime();
    }

    private void RefreshListBoxProcCodes()
    {
        listBoxProcCodes.Items.Clear();

        foreach (var procedureCode in _procedureCodes)
        {
            if (procedureCode.CodeNum == 0)
            {
                continue;
            }

            listBoxProcCodes.Items.Add(procedureCode.ProcCode, procedureCode);
        }
    }

    private void RefreshListBoxProcCodesRequired()
    {
        listBoxProcCodesRequired.Items.Clear();

        foreach (var procedureCode in _procedureCodesRequired)
        {
            if (procedureCode.CodeNum == 0)
            {
                continue;
            }

            listBoxProcCodesRequired.Items.Add(procedureCode.ProcCode, procedureCode);
        }

        if (_procedureCodesRequired.Count == 0)
        {
            radioButtonAll.Checked = false;
            radioButtonAtLeastOne.Checked = false;
            radioButtonAll.Enabled = false;
            radioButtonAtLeastOne.Enabled = false;
        }
        else
        {
            radioButtonAll.Enabled = true;
            radioButtonAtLeastOne.Enabled = true;
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes();

        formProcCodes.IsSelectionMode = true;
        formProcCodes.CanAllowMultipleSelections = true;

        if (formProcCodes.ShowDialog() == DialogResult.OK)
        {
            _procedureCodes.AddRange(formProcCodes.ListProcedureCodesSelected.Select(x => x.Copy()).ToList());
        }

        RefreshListBoxProcCodes();
    }

    private void ButtonClear_Click(object sender, EventArgs e)
    {
        _stringBuilderTime.Clear();

        FillTime();
    }

    private void ButtonRemove_Click(object sender, EventArgs e)
    {
        if (listBoxProcCodes.SelectedIndices.Count < 1)
        {
            ShowError("Please select the procedures you wish to remove.");
            return;
        }

        if (!ConfirmOk("Remove selected procedure(s)?"))
        {
            return;
        }

        var procedureCodes = listBoxProcCodes.GetListSelected<ProcedureCode>();

        foreach (var procedureCode in procedureCodes)
        {
            _procedureCodes.Remove(procedureCode);
        }

        RefreshListBoxProcCodes();
    }

    private void ButtonAddRequired_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes();

        formProcCodes.IsSelectionMode = true;
        formProcCodes.CanAllowMultipleSelections = true;

        if (formProcCodes.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _procedureCodesRequired.AddRange(formProcCodes.ListProcedureCodesSelected.Select(x => x.Copy()).ToList());
        if (!radioButtonAtLeastOne.Checked)
        {
            radioButtonAll.Checked = true;
        }

        RefreshListBoxProcCodesRequired();
    }

    private void ButtonRemoveRequired_Click(object sender, EventArgs e)
    {
        if (listBoxProcCodesRequired.SelectedIndices.Count < 1)
        {
            ShowError("Please select the procedures you wish to remove.");
            return;
        }

        if (!ConfirmOk("Remove selected procedure(s)?"))
        {
            return;
        }

        var procedureCodes = listBoxProcCodesRequired.GetListSelected<ProcedureCode>();
        foreach (var procedureCode in procedureCodes)
        {
            _procedureCodesRequired.Remove(procedureCode);
        }

        RefreshListBoxProcCodesRequired();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        AppointmentTypeCur.AppointmentTypeName = textName.Text;

        if (AppointmentTypeCur.AppointmentTypeColor != butColor.BackColor)
        {
            AppointmentTypeCur.AppointmentTypeColor = butColor.BackColor;
            if (AppointmentTypeCur.AppointmentTypeNum != 0 && !AppointmentTypeCur.IsNew && MsgBox.Show(this, MsgBoxButtons.YesNo, "Would you like to update all future appointments of this type to the new color?"))
            {
                Appointments.UpdateFutureApptColorForApptType(AppointmentTypeCur);
            }
        }

        AppointmentTypeCur.IsHidden = checkIsHidden.Checked;
        AppointmentTypeCur.CodeStr = string.Join(",", _procedureCodes.Select(x => x.ProcCode));
        AppointmentTypeCur.CodeStrRequired = string.Join(",", _procedureCodesRequired.Select(x => x.ProcCode));
        AppointmentTypeCur.RequiredProcCodesNeeded = EnumRequiredProcCodesNeeded.None;

        if (radioButtonAtLeastOne.Checked)
        {
            AppointmentTypeCur.RequiredProcCodesNeeded = EnumRequiredProcCodesNeeded.AtLeastOne;
        }

        if (radioButtonAll.Checked)
        {
            AppointmentTypeCur.RequiredProcCodesNeeded = EnumRequiredProcCodesNeeded.All;
        }

        var blockoutTypeDefNums = listBoxBlockoutTypes.GetListSelected<Def>().Select(x => x.DefNum).ToList();
        var blockoutTypes = string.Join(",", blockoutTypeDefNums);

        AppointmentTypeCur.BlockoutTypes = blockoutTypes;
        AppointmentTypeCur.Pattern = _stringBuilderTime.Length > 0 ? Appointments.ConvertPatternTo5(_stringBuilderTime.ToString()) : "";
        AppointmentTypeCur.IsNew = false;

        DialogResult = DialogResult.OK;
    }
}