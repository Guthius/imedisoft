using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Features.Providers.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormApptSearchAdvanced : FormODBase
{
    private readonly Appointment _appointment;
    private List<long> _provNums = [];
    private string _beforeTime;
    private string _afterTime;
    private DateTime _dateAfter;
    private List<ScheduleOpening> _scheduleOpenings = [];

    public FormApptSearchAdvanced(long apptNum)
    {
        InitializeComponent();

        _appointment = Appointments.GetOneApt(apptNum);
        if (_appointment is not null)
        {
            return;
        }

        ShowError("Invalid appointment on the Pinboard.");

        DialogResult = DialogResult.Abort;
    }

    private void FormApptSearchAdvanced_Load(object sender, EventArgs e)
    {
        dateSearchFrom.Text = _dateAfter != DateTime.MinValue.Date ? _dateAfter.ToShortDateString() : DateTime.Today.AddDays(1).ToShortDateString();
        dateSearchTo.Text = SIn.Date(dateSearchFrom.Text).AddYears(2).AddDays(1).ToShortDateString();

        textBefore.Text = _beforeTime;
        textAfter.Text = _afterTime;

        FillClinics();
        FillApptViews();
        FillBlockouts();
        FillProviders(GetProvidersForSelectedClinic(), _provNums);
    }

    internal void SetSearchArgs(List<long> provNums, string beforeTime, string afterTime, DateTime dateAfter)
    {
        _provNums = provNums;
        _beforeTime = beforeTime;
        _afterTime = afterTime;
        _dateAfter = dateAfter.Date;
    }

    private void FillProviders(List<ProviderDto> providersForClinic, List<long> provNumsToSelect = null)
    {
        if (provNumsToSelect == null || provNumsToSelect.Count == 0)
        {
            provNumsToSelect = [];
        }

        comboBoxMultiProv.Items.Clear();
        comboBoxMultiProv.Items.AddProvNone();
        comboBoxMultiProv.Items.AddProvsFull(providersForClinic);

        for (var i = 0; i < providersForClinic.Count; i++)
        {
            if (provNumsToSelect.Contains(providersForClinic[i].Id))
            {
                comboBoxMultiProv.SetSelected(i + 1, true);
            }
        }

        if (comboBoxMultiProv.GetListSelected<ProviderDto>().Count == 0)
        {
            comboBoxMultiProv.SetSelected(0, true);
        }
    }

    private List<ProviderDto> GetProvidersForSelectedClinic(ProvMode provMode = ProvMode.All)
    {
        var providers = Providers.GetProvsForClinic(comboBoxClinic.ClinicNumSelected);
        switch (provMode)
        {
            case ProvMode.Dent:
                providers.RemoveAll(x => x.IsSecondary);
                break;

            case ProvMode.Hyg:
                providers.RemoveAll(x => !x.IsSecondary);
                break;

            case ProvMode.All:
            default:
                break;
        }

        return providers;
    }

    private void FillClinics()
    {
        if (comboBoxClinic.IsNothingSelected)
        {
            comboBoxClinic.ClinicNumSelected = 0;
        }

        if (!comboBoxClinic.IsUnassignedSelected)
        {
            return;
        }

        comboApptView.Visible = true;
        labelAptViews.Visible = true;
    }

    private void FillApptViews()
    {
        if (comboBoxClinic.ClinicNumSelected > 0)
        {
            return;
        }

        var apptViews = ApptViews.GetForClinic(comboBoxClinic.ClinicNumSelected);

        comboApptView.Items.Clear();

        foreach (var apptView in apptViews)
        {
            comboApptView.Items.Add(apptView.Description, apptView);
        }

        if (apptViews.Count != 0)
        {
            comboApptView.SelectedIndex = 0;
        }
    }

    private void FillBlockouts()
    {
        comboBlockout.Items.Clear();
        comboBlockout.Items.AddDefNone();

        var blockoutTypeDefs = Defs.GetDefsForCategory(DefCat.BlockoutTypes, isShort: true);

        foreach (var def in blockoutTypeDefs)
        {
            if (def.ItemValue.Contains(BlockoutType.NoSchedule.GetDescription()))
            {
                continue;
            }

            comboBlockout.Items.Add(def.ItemName, def);
        }

        comboBlockout.SelectedIndex = 0;
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Day", 85));
        gridMain.Columns.Add(new GridColumn("Date", 85, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Time", 85, HorizontalAlignment.Center));

        gridMain.ListGridRows.Clear();

        foreach (var scheduleOpening in _scheduleOpenings)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(scheduleOpening.DateTimeAvail.DayOfWeek.ToString());
            gridRow.Cells.Add(scheduleOpening.DateTimeAvail.Date.ToShortDateString());
            gridRow.Cells.Add(scheduleOpening.DateTimeAvail.ToShortTimeString());
            gridRow.Tag = scheduleOpening;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void DoSearch()
    {
        Cursor = Cursors.WaitCursor;

        var from = dateSearchFrom.Value.Date.AddDays(-1);
        var to = dateSearchTo.Value.Date.AddDays(1);

        _scheduleOpenings.Clear();

        if (from.Year < 1880 || to.Year < 1880)
        {
            Cursor = Cursors.Default;

            ShowError("Invalid date selection.");
            return;
        }

        var beforeTime = new TimeSpan(0);
        if (textBefore.Text != "")
        {
            try
            {
                beforeTime = GetBeforeAfterTime(textBefore.Text, radioBeforePM.Checked);
            }
            catch
            {
                Cursor = Cursors.Default;

                ShowError("Invalid 'Starting before' time.");
                return;
            }
        }

        var afterTime = new TimeSpan(0);
        if (textAfter.Text != "")
        {
            try
            {
                afterTime = GetBeforeAfterTime(textAfter.Text, radioAfterPM.Checked);
            }
            catch
            {
                Cursor = Cursors.Default;

                ShowError("Invalid 'Starting after' time.");
                return;
            }
        }

        if (PrefC.GetInt(PrefName.AppointmentSearchBehavior) == 0 && comboBoxMultiProv.GetSelectedProvNums().Contains(0))
        {
            Cursor = Cursors.Default;

            ShowError("Please pick a provider.");
            return;
        }

        if (comboBoxMultiProv.GetSelectedProvNums().Contains(0) && comboBlockout.GetSelectedDefNum() == 0)
        {
            Cursor = Cursors.Default;

            ShowError("Please pick a provider and/or a blockout type.");
            return;
        }

        List<long> opNums;
        List<long> clinicNums = [];
        List<long> provNums = [];

        long blockoutType = 0;
        if (comboBlockout.GetSelectedDefNum() != 0)
        {
            blockoutType = comboBlockout.GetSelectedDefNum();
            provNums.Add(0);
        }

        if (!comboBoxMultiProv.GetSelectedProvNums().Contains(0))
        {
            var selectedProvNums = comboBoxMultiProv.GetSelectedProvNums();
            foreach (var provNum in selectedProvNums)
            {
                provNums.Add(provNum);
            }
        }

        if (comboBoxClinic.ClinicNumSelected == 0)
        {
            var apptViewNum = comboApptView.GetSelected<ApptView>().ApptViewNum;

            var opsForView = ApptViewItems.GetOpsForView(apptViewNum);
            var operatories = Operatories.GetOperatories(opsForView, isShort: true);

            clinicNums = operatories.Select(x => x.ClinicNum).Distinct().ToList();
            opNums = operatories.Select(x => x.OperatoryNum).ToList();
        }
        else
        {
            clinicNums.Add(comboBoxClinic.ClinicNumSelected);
            opNums = Operatories.GetOpsForClinic(comboBoxClinic.ClinicNumSelected).Select(x => x.OperatoryNum).ToList();
        }

        if (blockoutType != 0 && provNums.Max() > 0)
        {
            _scheduleOpenings.AddRange(ApptSearch.GetSearchResultsForBlockoutAndProvider(provNums, _appointment.AptNum,
                from, to, opNums, clinicNums, beforeTime, afterTime, [blockoutType], 15));
        }
        else
        {
            _scheduleOpenings = ApptSearch.GetSearchResults(_appointment.AptNum,
                from, to, provNums, opNums, clinicNums, beforeTime, afterTime, [blockoutType], resultCount: 15);
        }

        Cursor = Cursors.Default;

        FillGrid();
    }

    private static TimeSpan GetBeforeAfterTime(string timeText, bool isAfterPm)
    {
        var tokens = timeText.Split([':'], StringSplitOptions.RemoveEmptyEntries);

        var hour = "0";
        if (tokens.Length > 0)
        {
            hour = tokens[0];
        }

        var minute = "0";
        if (tokens.Length > 1)
        {
            minute = tokens[1];
        }

        var timeSpan = TimeSpan.FromHours(SIn.Double(hour)) + TimeSpan.FromMinutes(SIn.Double(minute));
        if (isAfterPm && timeSpan.Hours < 12)
        {
            timeSpan += TimeSpan.FromHours(12);
        }

        return timeSpan;
    }

    private void ButtonProviders_Click(object sender, EventArgs e)
    {
        var providers = comboBoxMultiProv.Items.GetAll<ProviderDto>();

        using var formProvidersMultiPick = new FormProvidersMultiPick(providers);

        formProvidersMultiPick.SelectedProviders = comboBoxMultiProv.GetListSelected<ProviderDto>();

        if (formProvidersMultiPick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        List<long> provNums = [];

        foreach (var provider in formProvidersMultiPick.SelectedProviders)
        {
            provNums.Add(provider.Id);
        }

        FillProviders(GetProvidersForSelectedClinic(), provNums);
    }

    private void comboBoxMultiProv_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (comboBoxMultiProv.GetSelectedProvNums().Contains(0))
        {
            comboBoxMultiProv.SetSelected(0, true);
        }
    }

    private void ComboBoxClinic_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (comboBoxClinic.ClinicNumSelected == 0)
        {
            comboApptView.Visible = true;
            labelAptViews.Visible = true;

            FillApptViews();
        }
        else
        {
            comboApptView.Visible = false;
            labelAptViews.Visible = false;
        }

        var provNums = comboBoxMultiProv.GetSelectedProvNums().FindAll(x => x != 0);

        FillProviders(GetProvidersForSelectedClinic(), provNums);
    }

    private void GridMain_CellClick(object sender, ODGridClickEventArgs e)
    {
        var rowDate = ((ScheduleOpening) gridMain.ListGridRows[e.Row].Tag).DateTimeAvail;

        GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, dateSelected: rowDate.Date, selectedAptNum: _appointment.AptNum);
    }

    private void ButtonMore_Click(object sender, EventArgs e)
    {
        if (_scheduleOpenings.Count < 1)
        {
            return;
        }

        dateSearchFrom.Text = _scheduleOpenings[_scheduleOpenings.Count - 1].DateTimeAvail.ToShortDateString();

        DoSearch();
    }

    private void ButtonProvDentist_Click(object sender, EventArgs e)
    {
        var providers = GetProvidersForSelectedClinic(ProvMode.Dent);

        FillProviders(providers, providers.Select(x => x.Id).ToList());

        DoSearch();
    }

    private void ButtonProvHygenist_Click(object sender, EventArgs e)
    {
        var providers = GetProvidersForSelectedClinic(ProvMode.Hyg);

        FillProviders(providers, providers.Select(x => x.Id).ToList());

        DoSearch();
    }

    private void ButtonSearch_Click(object sender, EventArgs e)
    {
        if (_appointment.AptNum <= 0)
        {
            ShowError("Invalid appointments on pinboard.");
            return;
        }

        DoSearch();

        butMore.Enabled = true;
    }

    private enum ProvMode
    {
        All,
        Dent,
        Hyg
    }
}