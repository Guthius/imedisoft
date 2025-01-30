using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormClaimProcBlueBookLog : FormODBase
{
    private List<InsBlueBookLog> _insBlueBookLogs;

    public FormClaimProcBlueBookLog(List<InsBlueBookLog> insBlueBookLogs)
    {
        InitializeComponent();

        _insBlueBookLogs = insBlueBookLogs;
    }

    private void FormClaimProcBlueBookLog_Load(object sender, EventArgs e)
    {
        _insBlueBookLogs = _insBlueBookLogs.OrderBy(x => x.DateTEntry).ToList();

        if (PrefC.GetEnum<AllowedFeeSchedsAutomate>(PrefName.AllowedFeeSchedsAutomate) == AllowedFeeSchedsAutomate.BlueBook)
        {
            labelBlueBookOff.Visible = false;
        }

        FillGrid();
    }

    private void FillGrid()
    {
        gridInsBlueBookLog.BeginUpdate();

        gridInsBlueBookLog.Columns.Clear();
        gridInsBlueBookLog.Columns.Add(new GridColumn("Date", 70, HorizontalAlignment.Center, GridSortingStrategy.DateParse));
        gridInsBlueBookLog.Columns.Add(new GridColumn("Time", 60, HorizontalAlignment.Center, GridSortingStrategy.DateParse));
        gridInsBlueBookLog.Columns.Add(new GridColumn("Insurance\r\nEstimate", 70, HorizontalAlignment.Right));
        gridInsBlueBookLog.Columns.Add(new GridColumn("Description", 0) {IsWidthDynamic = true});

        gridInsBlueBookLog.ListGridRows.Clear();

        foreach (var insBlueBookLog in _insBlueBookLogs)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(insBlueBookLog.DateTEntry.ToShortDateString());
            gridRow.Cells.Add(insBlueBookLog.DateTEntry.ToShortTimeString());
            gridRow.Cells.Add(insBlueBookLog.AllowedFee.ToString("f"));
            gridRow.Cells.Add(insBlueBookLog.Description);
            gridRow.Tag = insBlueBookLog;

            gridInsBlueBookLog.ListGridRows.Add(gridRow);
        }

        gridInsBlueBookLog.EndUpdate();
    }
}