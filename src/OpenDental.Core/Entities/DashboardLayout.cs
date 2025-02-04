using System.Collections.Generic;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class DashboardLayout : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DashboardLayoutNum;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;

    ///<summary>FK to usergroup.UserGroupNum.</summary>
    public long UserGroupNum;

    ///<summary>Text shown in the tab header.</summary>
    public string DashboardTabName;

    ///<summary>Orders the tabs in the tab control. 0 based.</summary>
    public int DashboardTabOrder;

    ///<summary>Number of rows for this DashboardLayout. Min value of 1.</summary>
    public int DashboardRows;

    ///<summary>Number of columns for this DashboardLayout. Min value of 1.</summary>
    public int DashboardColumns;

    ///<summary>Groups multiple DashboardLayout(s) together.</summary>
    public string DashboardGroupName;

    [CrudColumn(IsNotDbColumn = true)]
    public List<DashboardCell> Cells = [];
}