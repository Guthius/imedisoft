using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class DashboardCells
{
    public static List<DashboardCell> GetAll()
    {
        return DashboardCellCrud.SelectMany("SELECT * FROM dashboardcell");
    }
}