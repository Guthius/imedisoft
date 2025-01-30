using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormActivityLog : FormODBase
{
    private List<string> _actionDescriptions;
    private List<EServiceLog> _eServiceLogs = [];

    public FormActivityLog()
    {
        InitializeComponent();
    }

    private void FormActivityLog_Load(object sender, EventArgs e)
    {
        comboBoxClinicMulti.IsAllSelected = true;

        var firstDayOfTheMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        datePicker.SetDateTimeFrom(firstDayOfTheMonth);
        datePicker.SetDateTimeTo(firstDayOfTheMonth.AddMonths(1));

        checkDistinctLogGuid.Checked = false;

        _actionDescriptions = [];

        var eserviceTypes = Enum.GetValues(typeof(eServiceType)).Cast<eServiceType>().OrderByDescending(x => x == eServiceType.Unknown).ThenBy(x => x.GetDescription(useShortVersionIfAvailable: true)).ToList();
        foreach (var eServiceType in eserviceTypes)
        {
            comboBoxTypes.Items.Add(eServiceType.GetDescription(useShortVersionIfAvailable: true), eServiceType);
        }

        comboBoxTypes.SelectedIndex = 0;

        var eServiceActions = EServiceLogs.GetEServiceActions(eServiceType.Unknown);
        foreach (var eServiceAction in eServiceActions)
        {
            _actionDescriptions.Add(eServiceAction.GetDescription());
        }

        _actionDescriptions.Sort();
        _actionDescriptions.Insert(0, "All");

        comboBoxActions.Items.AddList(_actionDescriptions, x => x);
        comboBoxActions.SelectedIndex = 0;

        comboBoxClinicMulti.ClinicNumSelected = Clinics.ClinicNum;
    }

    private void FillGrid()
    {
        var eServiceLogs = _eServiceLogs;

        if (textPatNum.Text != "" && SIn.Long(textPatNum.Text) > -1)
        {
            eServiceLogs = eServiceLogs.Where(x => x.PatNum.ToString() == textPatNum.Text).ToList();
        }

        if (comboBoxTypes.SelectedIndex != -1 && comboBoxTypes.GetSelected<eServiceType>() != eServiceType.Unknown)
        {
            eServiceLogs = eServiceLogs.Where(x => x.EServiceType == comboBoxTypes.GetSelected<eServiceType>()).ToList();
        }

        if (comboBoxActions.SelectedIndex != -1 && _actionDescriptions[comboBoxActions.SelectedIndex] != "All")
        {
            eServiceLogs = eServiceLogs.Where(x => x.EServiceAction.GetDescription() == comboBoxActions.GetSelected<string>()).ToList();
        }

        if (textLogGuid.Text != "")
        {
            eServiceLogs = eServiceLogs.Where(x => x.LogGuid.Contains(textLogGuid.Text)).ToList();
        }

        if (checkDistinctLogGuid.Checked)
        {
            eServiceLogs = eServiceLogs.GroupBy(x => x.LogGuid).Select(x => x.OrderByDescending(y => y.LogDateTime).ThenByDescending(y => y.EServiceLogNum).First()).ToList();
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("eService Type", 150));
        gridMain.Columns.Add(new GridColumn("eService Action", 250));
        gridMain.Columns.Add(new GridColumn("FKeyType", 100));
        gridMain.Columns.Add(new GridColumn("FKey", 50));
        gridMain.Columns.Add(new GridColumn("Log DateTime", 100));
        gridMain.Columns.Add(new GridColumn("PatNum", 50));
        gridMain.Columns.Add(new GridColumn("Clinic Abbr", 100));
        gridMain.Columns.Add(new GridColumn("Log GUID", 100));
        gridMain.Columns.Add(new GridColumn("Note", 100));

        gridMain.ListGridRows.Clear();

        foreach (var eServiceLog in eServiceLogs)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(eServiceLog.EServiceType.GetDescription());
            gridRow.Cells.Add(eServiceLog.EServiceAction.GetDescription());
            gridRow.Cells.Add(eServiceLog.KeyType.ToString());
            gridRow.Cells.Add(eServiceLog.FKey.ToString());
            gridRow.Cells.Add(eServiceLog.LogDateTime.ToString(CultureInfo.InvariantCulture));
            gridRow.Cells.Add(eServiceLog.PatNum.ToString());
            gridRow.Cells.Add(eServiceLog.ClinicNum == 0 ? "HQ" : Clinics.GetClinic(eServiceLog.ClinicNum).Abbr);
            gridRow.Cells.Add(eServiceLog.LogGuid);
            gridRow.Cells.Add(eServiceLog.Note);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();

        labelRows.Text = $"Row Count: {eServiceLogs.Count}";

        gridMain.ScrollToEnd();
    }

    private void ButtonRefresh_Click(object sender, EventArgs e)
    {
        _eServiceLogs = EServiceLogs.GetEServiceLog(comboBoxClinicMulti.ClinicNumSelected, datePicker.GetDateTimeFrom(), datePicker.GetDateTimeTo());

        FillGrid();
    }

    private void ComboBoxActions_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ComboBoxTypes_SelectionChangeCommitted(object sender, EventArgs e)
    {
        comboBoxActions.Items.Clear();

        _actionDescriptions = [];

        var eServiceTypeSelected = comboBoxTypes.GetSelected<eServiceType>();
        var eServiceActions = EServiceLogs.GetEServiceActions(eServiceTypeSelected);

        foreach (var eServiceAction in eServiceActions)
        {
            _actionDescriptions.Add(eServiceAction.GetDescription());
        }

        _actionDescriptions.Sort();
        _actionDescriptions.Insert(0, "All");

        comboBoxActions.Items.AddList(_actionDescriptions, x => x);
        comboBoxActions.SelectedIndex = 0;

        FillGrid();
    }

    private void Textbox_TextChanged(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void CheckBoxDistinctLogGuid_CheckedChanged(object sender, EventArgs e)
    {
        FillGrid();
    }
}