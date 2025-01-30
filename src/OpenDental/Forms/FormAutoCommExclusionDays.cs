using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormeConfimationExclusionDays : FormODBase
{
    private readonly ClinicPrefHelper _clinicPrefHelper = new(PrefName.EConfirmExcludeDays, PrefName.EConfirmExcludeDaysUseHQ);
    private long _clinicNum;
    private List<AutoCommExcludeDate> _excludeDates = [];
    private AutoCommExcludeDate.AutoCommExcludeDays _excludeDays;
    private bool _useHqSettings;

    public FormeConfimationExclusionDays(long clinicNum)
    {
        InitializeComponent();

        _clinicNum = clinicNum;
    }

    private void FormAutoCommExclusionDays_Load(object sender, EventArgs e)
    {
        comboBoxClinicPicker.ClinicNumSelected = _clinicNum;

        Reload();
    }

    private void Reload()
    {
        _excludeDates = checkShowPastDates.Checked ? AutoCommExcludeDates.Refresh(_clinicNum) : AutoCommExcludeDates.GetFutureForClinic(_clinicNum);

        var val = _clinicPrefHelper.GetStringVal(PrefName.EConfirmExcludeDays, _clinicNum);
        if (val == AutoCommExcludeDate.AutoCommExcludeDays.None.ToString())
        {
            _excludeDays = 0;
        }
        else
        {
            _excludeDays = (AutoCommExcludeDate.AutoCommExcludeDays) byte.Parse(_clinicPrefHelper.GetStringVal(PrefName.EConfirmExcludeDays, _clinicNum));
        }

        if (_clinicNum > 0)
        {
            _useHqSettings = _clinicPrefHelper.GetBoolVal(PrefName.EConfirmExcludeDaysUseHQ, _clinicNum);
            checkUseHQ.Checked = _useHqSettings;
            checkUseHQ.Visible = true;
        }
        else
        {
            checkUseHQ.Checked = false;
            checkUseHQ.Visible = false;
        }

        listBoxExclusionDates.Items.Clear();
        listBoxExclusionDates.Items.AddList(_excludeDates, x => x.DateExclude.ToShortDateString());

        listBoxExclusionDays.Items.Clear();
        listBoxExclusionDays.Items.AddEnums<AutoCommExcludeDate.AutoCommExcludeDays>();
        listBoxExclusionDays.Items.RemoveAt(0);

        foreach (AutoCommExcludeDate.AutoCommExcludeDays day in Enum.GetValues(typeof(AutoCommExcludeDate.AutoCommExcludeDays)))
        {
            if (day != AutoCommExcludeDate.AutoCommExcludeDays.None && _excludeDays.HasFlag(day))
            {
                listBoxExclusionDays.SetSelectedEnum(day);
            }
        }
    }

    private void ComboBoxClinicPicker_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (!ExcludeDaysSelectionOk())
        {
            comboBoxClinicPicker.ClinicNumSelected = _clinicNum;
            return;
        }

        Save();

        _clinicNum = comboBoxClinicPicker.ClinicNumSelected;

        Reload();
    }

    private void CheckBoxShowPastDates_CheckedChanged(object sender, EventArgs e)
    {
        Reload();
    }

    private void CheckBoxUseHQ_CheckedChanged(object sender, EventArgs e)
    {
        if (checkUseHQ.Checked && _clinicNum > 0)
        {
            listBoxExclusionDates.Enabled = false;
            listBoxExclusionDays.Enabled = false;
            butAdd.Enabled = false;
            butDelete.Enabled = false;
            labelExclusionDays.Enabled = false;
            labelExclusionDates.Enabled = false;
            labelUseDefaultMessage.Visible = true;
        }
        else
        {
            listBoxExclusionDates.Enabled = true;
            listBoxExclusionDays.Enabled = true;
            butAdd.Enabled = true;
            butDelete.Enabled = true;
            labelExclusionDays.Enabled = true;
            labelExclusionDates.Enabled = true;
            labelUseDefaultMessage.Visible = false;
        }
    }

    private void Save()
    {
        var excludeDays = AutoCommExcludeDate.AutoCommExcludeDays.None;

        listBoxExclusionDays.GetListSelected<AutoCommExcludeDate.AutoCommExcludeDays>().ForEach(day => excludeDays |= day);

        if (checkUseHQ.Checked && _clinicNum > 0)
        {
            _clinicPrefHelper.ValChangedByUser(PrefName.EConfirmExcludeDaysUseHQ, _clinicNum, SOut.Bool(true));
        }
        else if (_clinicNum > 0)
        {
            _clinicPrefHelper.ValChangedByUser(PrefName.EConfirmExcludeDaysUseHQ, _clinicNum, SOut.Bool(false));
        }

        _clinicPrefHelper.ValChangedByUser(PrefName.EConfirmExcludeDays, _clinicNum, ((int) (byte) excludeDays).ToString());
    }

    private bool ExcludeDaysSelectionOk()
    {
        if (listBoxExclusionDays.SelectedIndices.Count != 7)
        {
            return true;
        }

        ShowError("Cannot block all days of the week. To block eConfirmations entirely, disable the eConfirmation rule.");

        return false;
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        Save();

        using var formDatePicker = new FormCalendar();

        formDatePicker.DateSelected = DateTime.Now;
        formDatePicker.MinDate = DateTime.Now;

        if (formDatePicker.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var autoCommExcludeDate = new AutoCommExcludeDate
        {
            DateExclude = formDatePicker.DateSelected,
            ClinicNum = _clinicNum
        };

        if (_excludeDates.Any(x => x.DateExclude == autoCommExcludeDate.DateExclude && x.ClinicNum == autoCommExcludeDate.ClinicNum))
        {
            return;
        }

        AutoCommExcludeDates.Insert(autoCommExcludeDate);

        Reload();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        Save();

        var selectedDate = listBoxExclusionDates.GetSelected<AutoCommExcludeDate>();
        if (selectedDate is null)
        {
            ShowError("Please selete a date to delete.");
            return;
        }

        AutoCommExcludeDates.Delete(selectedDate.AutoCommExcludeDateNum);

        Reload();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!ExcludeDaysSelectionOk())
        {
            return;
        }

        Save();

        _clinicPrefHelper.SyncAllPrefs();

        DialogResult = DialogResult.OK;
    }
}