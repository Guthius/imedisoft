using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptTypes : FormODBase
{
    private List<AppointmentType> _appointmentTypes = [];
    private List<AppointmentType> _appointmentTypesOld;
    private bool _changed;

    public bool IsNoneAllowed { get; set; }
    public bool IsSelectionMode { get; set; }
    public bool AllowMultipleSelections { get; set; }
    public AppointmentType SelectedAppointmentType { get; set; }
    public List<AppointmentType> SelectedAppointmentTypes { get; set; } = [];

    public FormApptTypes()
    {
        InitializeComponent();
    }

    private void FormApptTypes_Load(object sender, EventArgs e)
    {
        if (IsSelectionMode)
        {
            butAdd.Visible = false;
            checkWarn.Visible = false;
            checkPrompt.Visible = false;

            if (AllowMultipleSelections)
            {
                Text = "Select Appointment Types";
                gridMain.SelectionMode = GridSelectionMode.MultiExtended;
            }
            else
            {
                Text = "Select Appointment Type";
            }

            gridMain.Location = new Point(8, 6);
            gridMain.Size = new Size(320, 447);
        }
        else
        {
            butOK.Visible = false;
        }

        checkPrompt.Checked = PrefC.GetBool(PrefName.AppointmentTypeShowPrompt);
        checkWarn.Checked = PrefC.GetBool(PrefName.AppointmentTypeShowWarning);

        _appointmentTypes = AppointmentTypes.GetDeepCopy(IsSelectionMode);
        _appointmentTypesOld = AppointmentTypes.GetDeepCopy();

        FillMain();

        if (!IsSelectionMode)
        {
            return;
        }

        if (SelectedAppointmentType is not null)
        {
            SelectedAppointmentTypes.Add(SelectedAppointmentType);
        }

        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            if ((AppointmentType) gridMain.ListGridRows[i].Tag is not null && SelectedAppointmentTypes.Any(x => x.AppointmentTypeNum == ((AppointmentType) gridMain.ListGridRows[i].Tag).AppointmentTypeNum))
            {
                gridMain.SetSelected(i);
            }
        }
    }

    private void FillMain()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Name", 200));
        gridMain.Columns.Add(new GridColumn("Color", 50, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Hidden", 60, HorizontalAlignment.Center) {IsWidthDynamic = true});

        gridMain.ListGridRows.Clear();

        _appointmentTypes.Sort(AppointmentTypes.SortItemOrder);

        GridRow gridRow;

        foreach (var appointmentType in _appointmentTypes)
        {
            gridRow = new GridRow();

            gridRow.Cells.Add(appointmentType.AppointmentTypeName);
            gridRow.Cells.Add("");
            gridRow.Cells.Add(appointmentType.IsHidden ? "X" : "");
            gridRow.Cells[1].ColorBackG = appointmentType.AppointmentTypeColor;
            gridRow.Tag = appointmentType;

            gridMain.ListGridRows.Add(gridRow);
        }

        if (IsNoneAllowed)
        {
            gridRow = new GridRow();
            gridRow.Cells.Add("None");
            gridRow.Cells.Add("");
            gridRow.Cells.Add("");
            gridRow.Tag = null;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (IsSelectionMode)
        {
            SelectedAppointmentTypes = gridMain.SelectedTags<AppointmentType>();
            SelectedAppointmentType = SelectedAppointmentTypes.FirstOrDefault();
            DialogResult = DialogResult.OK;
        }
        else
        {
            using var formApptTypeEdit = new FormApptTypeEdit();

            formApptTypeEdit.AppointmentTypeCur = _appointmentTypes[e.Row];

            if (formApptTypeEdit.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (formApptTypeEdit.AppointmentTypeCur == null)
            {
                _appointmentTypes.RemoveAt(e.Row);
            }
            else
            {
                _appointmentTypes[e.Row] = formApptTypeEdit.AppointmentTypeCur;
            }

            _changed = true;

            FillMain();
        }
    }

    private void CheckBoxPrompt_CheckedChanged(object sender, EventArgs e)
    {
        _changed = true;
    }

    private void CheckBoxWarn_CheckedChanged(object sender, EventArgs e)
    {
        _changed = true;
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var appointmentType = new AppointmentType
        {
            ItemOrder = _appointmentTypes.Count,
            IsNew = true,
            AppointmentTypeColor = Color.FromArgb(0)
        };

        using var formApptTypeEdit = new FormApptTypeEdit();

        formApptTypeEdit.AppointmentTypeCur = appointmentType;

        if (formApptTypeEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _appointmentTypes.Add(formApptTypeEdit.AppointmentTypeCur);

        _changed = true;

        FillMain();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        SelectedAppointmentTypes = gridMain.SelectedTags<AppointmentType>();
        SelectedAppointmentType = SelectedAppointmentTypes.FirstOrDefault();

        DialogResult = DialogResult.OK;
    }

    private void FormApptTypes_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (IsSelectionMode)
        {
            return;
        }

        if (_changed)
        {
            Prefs.UpdateBool(PrefName.AppointmentTypeShowPrompt, checkPrompt.Checked);
            Prefs.UpdateBool(PrefName.AppointmentTypeShowWarning, checkWarn.Checked);

            for (var i = 0; i < _appointmentTypes.Count; i++)
            {
                _appointmentTypes[i].ItemOrder = i;
            }

            AppointmentTypes.Sync(_appointmentTypes, _appointmentTypesOld);

            var appointmentTypesOverlap = _appointmentTypesOld.FindAll(x => _appointmentTypes.Exists(y => x.AppointmentTypeNum == y.AppointmentTypeNum));

            foreach (var t in appointmentTypesOverlap)
            {
                var appointmentType = _appointmentTypes.Find(x => x.AppointmentTypeNum == t.AppointmentTypeNum);

                var message = "";
                if (appointmentType.BlockoutTypes != t.BlockoutTypes)
                {
                    message += "BlockoutTypes changed from '" + t.BlockoutTypes + "' to '" + appointmentType.BlockoutTypes + "'\r\n";
                }

                if (appointmentType.CodeStr != t.CodeStr)
                {
                    message += "CodeStr changed from '" + t.CodeStr + "' to '" + appointmentType.CodeStr + "'\r\n";
                }

                if (appointmentType.CodeStrRequired != t.CodeStrRequired)
                {
                    message += "CodeStrRequired changed from '" + t.CodeStrRequired + "' to '" + appointmentType.CodeStrRequired + "'\r\n";
                }

                if (!string.IsNullOrEmpty(message))
                {
                    SecurityLogs.MakeLogEntry(EnumPermType.AppointmentTypeEdit, 0, "Appointment Type \"" + appointmentType.AppointmentTypeName + "\" edited.\r\n" + message.Trim());
                }
            }

            DataValid.SetInvalid(InvalidType.AppointmentTypes);
            DataValid.SetInvalid(InvalidType.Prefs);
        }

        DialogResult = DialogResult.OK;
    }
}