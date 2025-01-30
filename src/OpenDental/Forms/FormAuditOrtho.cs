using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAuditOrtho : FormODBase
{
    public SortedDictionary<DateTime, List<SecurityLog>> DictDateOrthoLogs = new();

    public readonly List<SecurityLog> ListSecurityLogs;

    public FormAuditOrtho()
    {
        InitializeComponent();

        ListSecurityLogs = [];
    }

    private void FormAuditOrtho_Load(object sender, EventArgs e)
    {
        FillGridDates();
        FillGridMain();
    }

    private void FillGridDates()
    {
        gridHist.BeginUpdate();

        gridHist.Columns.Clear();
        gridHist.Columns.Add(new GridColumn("Date", 70));
        gridHist.Columns.Add(new GridColumn("Entries", 50, HorizontalAlignment.Center));

        gridHist.ListGridRows.Clear();
        foreach (var pair in DictDateOrthoLogs)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(pair.Key.ToShortDateString());
            gridRow.Cells.Add(pair.Value.Count.ToString());
            gridRow.Tag = pair.Key;

            gridHist.ListGridRows.Add(gridRow);
        }

        gridHist.EndUpdate();
        gridHist.ScrollToEnd();
        gridHist.SetAll(true);
    }

    private void FillGridMain()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date Time", 120, GridSortingStrategy.DateParse));
        gridMain.Columns.Add(new GridColumn("User", 70));
        gridMain.Columns.Add(new GridColumn("Permission", 110));
        gridMain.Columns.Add(new GridColumn("Log Text", 569));

        gridMain.ListGridRows.Clear();

        List<SecurityLog> securityLogs = [];
        foreach (var iDate in gridHist.SelectedIndices)
        {
            var dateRow = (DateTime) gridHist.ListGridRows[iDate].Tag;
            if (!DictDateOrthoLogs.TryGetValue(dateRow, out var listSecurityLogsForDate))
            {
                continue;
            }

            securityLogs.AddRange(listSecurityLogsForDate);
        }

        securityLogs = securityLogs.OrderBy(x => x.LogDateTime).ToList();
        securityLogs.AddRange(ListSecurityLogs.OrderBy(x => x.LogDateTime));

        foreach (var securityLog in securityLogs)
        {
            var user = Userods.GetUser(securityLog.UserNum);

            var gridRow = new GridRow();

            gridRow.Cells.Add(securityLog.LogDateTime.ToShortDateString() + " " + securityLog.LogDateTime.ToShortTimeString());
            gridRow.Cells.Add(user == null ? "unknown" : user.UserName);
            gridRow.Cells.Add(securityLog.PermType.ToString());
            gridRow.Cells.Add(securityLog.LogText);
            gridRow.Tag = securityLog;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        gridMain.ScrollToEnd();
    }

    private void GridHist_CellClick(object sender, ODGridClickEventArgs e)
    {
        FillGridMain();
    }
}