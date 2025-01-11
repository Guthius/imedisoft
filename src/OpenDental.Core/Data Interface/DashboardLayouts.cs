using System.Collections.Generic;
using System.Linq;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class DashboardLayouts
{
    public static List<DashboardLayout> GetDashboardLayout(string dashboardGroupName = "")
    {
        var command = "SELECT * FROM dashboardlayout";
        var listDashboardLayouts = DashboardLayoutCrud.SelectMany(command);
        if (!string.IsNullOrEmpty(dashboardGroupName))
            //Limit to a single group.
            listDashboardLayouts = listDashboardLayouts.FindAll(x => x.DashboardGroupName.ToLower() == dashboardGroupName.ToLower());

        //Fill the non-db Cells field.
        var listDashboardCells = DashboardCells.GetAll();
        for (var i = 0; i < listDashboardLayouts.Count; i++) listDashboardLayouts[i].Cells = listDashboardCells.FindAll(x => x.DashboardLayoutNum == listDashboardLayouts[i].DashboardLayoutNum);

        return listDashboardLayouts;
    }

    ///<summary>Inserts the given dashboard layouts and cells into the database.</summary>
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

    /*
Only pull out the methods below as you need them.  Otherwise, leave them commented out.


public static List<DashboardLayout> Refresh(long patNum){

string command="SELECT * FROM dashboardlayout WHERE PatNum = "+POut.Long(patNum);
return Crud.DashboardLayoutCrud.SelectMany(command);
}

///<summary>Gets one DashboardLayout from the db.</summary>
public static DashboardLayout GetOne(long dashboardLayoutNum){

return Crud.DashboardLayoutCrud.SelectOne(dashboardLayoutNum);
}


public static long Insert(DashboardLayout dashboardLayout){

return Crud.DashboardLayoutCrud.Insert(dashboardLayout);
}


public static void Update(DashboardLayout dashboardLayout){

Crud.DashboardLayoutCrud.Update(dashboardLayout);
}


public static void Delete(long dashboardLayoutNum) {

Crud.DashboardLayoutCrud.Delete(dashboardLayoutNum);
}




*/
}