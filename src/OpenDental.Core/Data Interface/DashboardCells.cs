using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

/// <summary>
///     Insert, Update, Delete are all managed by DashboardLayouts. The 2 classes are tightly coupled and should not
///     be modified separately.
/// </summary>
public class DashboardCells
{
    
    public static List<DashboardCell> GetAll()
    {
        var command = "SELECT * FROM dashboardcell";
        return DashboardCellCrud.SelectMany(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static long Insert(DashboardCell dashboardCell) {

        return Crud.DashboardCellCrud.Insert(dashboardCell);
    }

    
    public static void Delete(long dashboardCellNum) {

        Crud.DashboardCellCrud.Delete(dashboardCellNum);
    }

    ///<summary>Gets one DashboardCell from the db.</summary>
    public static DashboardCell GetOne(long dashboardCellNum){

        return Crud.DashboardCellCrud.SelectOne(dashboardCellNum);
    }


    
    public static void Update(DashboardCell dashboardCell){

        Crud.DashboardCellCrud.Update(dashboardCell);
    }






    */
}