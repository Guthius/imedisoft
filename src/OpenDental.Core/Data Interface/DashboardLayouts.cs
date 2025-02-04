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

    public static void SetDashboardLayout(List<DashboardLayout> dashboardLayouts, string dashboardGroupName)
    {
        var dashboardLayoutsDbAll = GetDashboardLayout();
        var dashboardLayoutsDbGroup = dashboardLayoutsDbAll.FindAll(x => string.Equals(x.DashboardGroupName, dashboardGroupName, StringComparison.CurrentCultureIgnoreCase));

        var dashboardCells = dashboardLayoutsDbGroup.SelectMany(x => x.Cells).ToList();
        foreach (var dashboardCell in dashboardCells)
        {
            DashboardCellCrud.Delete(dashboardCell.DashboardCellNum);
        }

        foreach (var dashboardLayout in dashboardLayoutsDbGroup)
        {
            DashboardLayoutCrud.Delete(dashboardLayout.DashboardLayoutNum);
        }

        var dashboardCellsDb = DashboardCells.GetAll();
        foreach (var dashboardLayout in dashboardLayouts)
        {
            dashboardLayout.DashboardGroupName = dashboardGroupName;
            dashboardLayoutsDbAll
                .FindAll(x => x.DashboardLayoutNum == dashboardLayout.DashboardLayoutNum)
                .ForEach(x => DashboardLayoutCrud.Delete(x.DashboardLayoutNum));
            
            dashboardCellsDb
                .FindAll(x => x.DashboardLayoutNum == dashboardLayout.DashboardLayoutNum)
                .ForEach(x => DashboardCellCrud.Delete(x.DashboardCellNum));
            
            var layoutNum = DashboardLayoutCrud.Insert(dashboardLayout);
            
            dashboardLayout.Cells.ForEach(x =>
            {
                x.DashboardLayoutNum = layoutNum;
                
                DashboardCellCrud.Insert(x);
            });
        }
    }
}