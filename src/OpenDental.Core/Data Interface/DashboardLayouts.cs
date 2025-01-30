using System;
using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class DashboardLayouts
{
    public static List<DashboardLayout> GetDashboardLayout(string dashboardGroupName = "")
    {
        var dashboardLayouts = DashboardLayoutCrud.SelectMany("SELECT * FROM dashboardlayout");
        if (!string.IsNullOrEmpty(dashboardGroupName))
        {
            dashboardLayouts = dashboardLayouts.FindAll(x => string.Equals(x.DashboardGroupName, dashboardGroupName, StringComparison.CurrentCultureIgnoreCase));
        }
        
        var dashboardCells = DashboardCells.GetAll();
        foreach (var dashboardLayout in dashboardLayouts)
        {
            dashboardLayout.Cells = dashboardCells.FindAll(x => x.DashboardLayoutNum == dashboardLayout.DashboardLayoutNum);
        }

        return dashboardLayouts;
    }

    public static void SetDashboardLayout(List<DashboardLayout> listDashboardLayouts, string dashboardGroupName)
    {
        //Get all old layouts.
        var listDashboardLayoutsDbAll = GetDashboardLayout();
        //Get all old layouts for this group.
        var listDashboardLayoutsDbGroup = listDashboardLayoutsDbAll.FindAll(x => x.DashboardGroupName.ToLower() == dashboardGroupName.ToLower());
        //Delete all cells from old dashboard group.
        var listDashboardCells = listDashboardLayoutsDbGroup.SelectMany(x => x.Cells).ToList();
        for (var i = 0; i < listDashboardCells.Count; i++) DashboardCellCrud.Delete(listDashboardCells[i].DashboardCellNum);

        //Delete all layouts from old dashboard group.
        for (var i = 0; i < listDashboardLayoutsDbGroup.Count; i++) DashboardLayoutCrud.Delete(listDashboardLayoutsDbGroup[i].DashboardLayoutNum);

        var listDashboardCellsDb = DashboardCells.GetAll();
        for (var i = 0; i < listDashboardLayouts.Count; i++)
        {
            listDashboardLayouts[i].DashboardGroupName = dashboardGroupName;
            //Delete old tab if it exists.
            listDashboardLayoutsDbAll
                .FindAll(x => x.DashboardLayoutNum == listDashboardLayouts[i].DashboardLayoutNum)
                .ForEach(x => DashboardLayoutCrud.Delete(x.DashboardLayoutNum));
            //Delete old cells which belonged to this tab if they exist.
            listDashboardCellsDb
                .FindAll(x => x.DashboardLayoutNum == listDashboardLayouts[i].DashboardLayoutNum)
                .ForEach(x => DashboardCellCrud.Delete(x.DashboardCellNum));
            //Insert new tab.
            var layoutNumNew = DashboardLayoutCrud.Insert(listDashboardLayouts[i]);
            //Insert link cells to new tab and insert.
            listDashboardLayouts[i].Cells.ForEach(x =>
            {
                x.DashboardLayoutNum = layoutNumNew;
                DashboardCellCrud.Insert(x);
            });
        }
    }
}