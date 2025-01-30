using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAudit : FormODBase
{
    private long _patNum;
    private readonly List<string> _permissions;
    private bool _hasHeadingPrinted;
    private int _pagesPrinted;
    private int _headingPrintH;
    private List<Userod> _users;

    public long CurPatNum { get; set; }

    public FormAudit()
    {
        InitializeComponent();

        var permissionTypes = (EnumPermType[]) Enum.GetValues(typeof(EnumPermType));

        _permissions = [];

        for (var i = 1; i < permissionTypes.Length; i++)
        {
            if (GroupPermissions.HasAuditTrail(permissionTypes[i]))
            {
                _permissions.Add(permissionTypes[i].ToString());
            }
        }

        _permissions.Sort();
        _permissions.RemoveAll(x => x == "TextMessageView");
        _permissions.Insert(0, EnumPermType.None.ToString());

        comboLogSource.IncludeAll = true;

        var sourcesAlphabetic = Enum
            .GetValues(typeof(LogSources))
            .Cast<LogSources>()
            .OrderByDescending(x => x == LogSources.None)
            .ThenBy(x => x.GetDescription())
            .ToList();

        comboLogSource.Items.AddListEnum(sourcesAlphabetic);
    }

    private void FormAudit_Load(object sender, EventArgs e)
    {
        textDateFrom.Text = DateTime.Today.AddDays(-10).ToShortDateString();
        textDateTo.Text = DateTime.Today.ToShortDateString();

        for (var i = 0; i < _permissions.Count; i++)
        {
            comboPermission.Items.Add(i == 0 ? "All" : _permissions[i]);
        }

        comboPermission.SelectedIndex = 0;
        comboLogSource.IsAllSelected = true;

        _users = Userods.GetDeepCopy();

        comboUser.Items.Add("All");
        comboUser.Items.Add("None");
        comboUser.SelectedIndex = 0;

        foreach (var user in _users)
        {
            comboUser.Items.Add(user.UserName);
        }

        _patNum = CurPatNum;

        textPatient.Text = _patNum == 0 ? "" : Patients.GetLim(_patNum).GetNameLF();
        textRows.Text = PrefC.GetString(PrefName.AuditTrailEntriesDisplayed);

        FillGrid();
    }

    private void ComboBoxUser_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ComboBoxPermission_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ComboBoxLogSource_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ButtonCurrent_Click(object sender, EventArgs e)
    {
        _patNum = CurPatNum;

        textPatient.Text = _patNum == 0 ? "" : Patients.GetLim(_patNum).GetNameLF();

        FillGrid();
    }

    private void ButtonFind_Click(object sender, EventArgs e)
    {
        var frmPatientSelect = new FrmPatientSelect();

        frmPatientSelect.ShowDialog();

        if (frmPatientSelect.IsDialogCancel)
        {
            return;
        }

        _patNum = frmPatientSelect.PatNumSelected;

        textPatient.Text = Patients.GetLim(_patNum).GetNameLF();

        FillGrid();
    }

    private void ButtonAll_Click(object sender, EventArgs e)
    {
        _patNum = 0;

        textPatient.Text = "";

        FillGrid();
    }

    private void FillGrid()
    {
        if (!textRows.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        var userNum = comboUser.SelectedIndex switch
        {
            0 => -1,
            > 1 => _users[comboUser.SelectedIndex - 2].UserNum,
            _ => 0
        };

        var datePreviousFrom = SIn.Date(textDateEditedFrom.Text);
        var datePreviousTo = DateTime.Today;
        if (textDateEditedTo.Text != "")
        {
            datePreviousTo = SIn.Date(textDateEditedTo.Text);
        }

        var logSource = -1;
        if (!comboLogSource.IsAllSelected)
        {
            logSource = (int) comboLogSource.GetSelected<LogSources>();
        }

        SecurityLog[] securityLogs;
        try
        {
            securityLogs = comboPermission.SelectedIndex == 0
                ? SecurityLogs.Refresh(SIn.Date(textDateFrom.Text), SIn.Date(textDateTo.Text), EnumPermType.None, _patNum, datePreviousFrom, datePreviousTo, SIn.Int(textRows.Text), userNum, logSource)
                : SecurityLogs.Refresh(SIn.Date(textDateFrom.Text), SIn.Date(textDateTo.Text), (EnumPermType) Enum.Parse(typeof(EnumPermType), comboPermission.SelectedItem.ToString()), _patNum, datePreviousFrom, datePreviousTo, SIn.Int(textRows.Text), userNum, logSource);
        }
        catch (Exception ex)
        {
            ShowException(ex, "There was a problem refreshing the Audit Trail with the current filters.");
            securityLogs = [];
        }

        grid.BeginUpdate();

        grid.Columns.Clear();
        grid.Columns.Add(new GridColumn("Date", 70, GridSortingStrategy.DateParse));
        grid.Columns.Add(new GridColumn("Time", 60, GridSortingStrategy.DateParse));
        grid.Columns.Add(new GridColumn("Patient", 100));
        grid.Columns.Add(new GridColumn("User", 70));
        grid.Columns.Add(new GridColumn("Permission", 190));
        grid.Columns.Add(new GridColumn("Computer", 70));
        grid.Columns.Add(new GridColumn("Log Text", 279));
        grid.Columns.Add(new GridColumn("Log Source", 140));
        grid.Columns.Add(new GridColumn("Last Edit", 100));

        grid.ListGridRows.Clear();

        foreach (var securityLog in securityLogs)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(securityLog.LogDateTime.ToShortDateString());
            gridRow.Cells.Add(securityLog.LogDateTime.ToShortTimeString());
            gridRow.Cells.Add(securityLog.PatientName);
            gridRow.Cells.Add(Userods.GetUser(securityLog.UserNum)?.UserName ?? "Unknown(" + SOut.Long(securityLog.UserNum) + ")");

            switch (securityLog.PermType)
            {
                case EnumPermType.ChartModule:
                    gridRow.Cells.Add("ChartModuleViewed");
                    break;

                case EnumPermType.FamilyModule:
                    gridRow.Cells.Add("FamilyModuleViewed");
                    break;

                case EnumPermType.AccountModule:
                    gridRow.Cells.Add("AccountModuleViewed");
                    break;

                case EnumPermType.ImagingModule:
                    gridRow.Cells.Add("ImagesModuleViewed");
                    break;

                case EnumPermType.TPModule:
                    gridRow.Cells.Add("TreatmentPlanModuleViewed");
                    break;

                default:
                    gridRow.Cells.Add(securityLog.PermType.ToString());
                    break;
            }

            gridRow.Cells.Add(securityLog.CompName);

            var text = securityLog.LogText;
            if (securityLog.PermType != EnumPermType.UserQuery)
            {
                gridRow.Cells.Add(text);
            }
            else
            {
                gridRow.Cells.Add(StringTools.Truncate(text, 200, hasElipsis: true));
                gridRow.Tag = (Action) (() =>
                {
                    var msgBoxCopyPaste = new MsgBoxCopyPaste(text);

                    msgBoxCopyPaste.NormalizeContent();
                    msgBoxCopyPaste.Show();
                });
            }

            var source = securityLog.LogSource.ToString();
            if (source == "None")
            {
                source = "";
            }

            gridRow.Cells.Add(source);
            gridRow.Cells.Add(securityLog.DateTPrevious.Year < 1880 ? "" : securityLog.DateTPrevious.ToString(CultureInfo.InvariantCulture));

            if (securityLog.LogHash != SecurityLogHashes.GetHashString(securityLog))
            {
                gridRow.ColorText = Color.Red;
            }

            grid.ListGridRows.Add(gridRow);
        }

        grid.EndUpdate();
        grid.ScrollToEnd();
    }

    private void Grid_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        (grid.ListGridRows[e.Row].Tag as Action)?.Invoke();
    }

    private void ButtonRefresh_Click(object sender, EventArgs e)
    {
        if (textDateFrom.Text == "" || textDateTo.Text == "" || !textDateFrom.IsValid() || !textDateTo.IsValid() || !textRows.IsValid() || !textDateEditedFrom.IsValid() || !textDateEditedTo.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        FillGrid();
    }

    private void ButtonPrint_Click(object sender, EventArgs e)
    {
        _pagesPrinted = 0;
        _hasHeadingPrinted = false;

        PrinterL.TryPrintOrDebugClassicPreview(PrintPage, "Audit trail printed", printoutOrientation: PrintoutOrientation.Landscape);
    }

    private void PrintPage(object sender, PrintPageEventArgs e)
    {
        using var headingFont = new Font("Arial", 13, FontStyle.Bold);
        using var subHeadingFont = new Font("Arial", 10, FontStyle.Bold);

        var y = e.MarginBounds.Top;
        var cx = e.MarginBounds.X + e.MarginBounds.Width / 2;

        if (!_hasHeadingPrinted)
        {
            var text = "Audit Trail";
            e.Graphics.DrawString(text, headingFont, Brushes.Black, cx - e.Graphics.MeasureString(text, headingFont).Width / 2, y);
            y += (int) e.Graphics.MeasureString(text, headingFont).Height;

            text = textDateFrom.Text + " to " + textDateTo.Text;
            e.Graphics.DrawString(text, subHeadingFont, Brushes.Black, cx - e.Graphics.MeasureString(text, subHeadingFont).Width / 2, y);
            y += 20;

            _hasHeadingPrinted = true;
            _headingPrintH = y;
        }

        y = grid.PrintPage(e.Graphics, _pagesPrinted, e.MarginBounds, _headingPrintH);

        _pagesPrinted++;
        if (y == -1)
        {
            e.HasMorePages = true;
        }
        else
        {
            e.HasMorePages = false;

            _pagesPrinted = 0;
        }
    }
}