using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AlertReads
{
    public static List<AlertRead> RefreshForAlertNums(long patNum, List<long> alertItemNums)
    {
        if (alertItemNums == null || alertItemNums.Count == 0)
        {
            return [];
        }
        
        return AlertReadCrud.SelectMany("SELECT * FROM alertread WHERE UserNum = " + patNum + " AND  AlertItemNum IN (" + string.Join(", ", alertItemNums) + ")");
    }

    public static void Insert(AlertRead alertRead)
    {
        AlertReadCrud.Insert(alertRead);
    }

    public static void DeleteForAlertItems(List<long> alertItemNums)
    {
        if (alertItemNums == null || alertItemNums.Count == 0)
        {
            return;
        }
        
        Db.NonQ("DELETE FROM alertread WHERE AlertItemNum IN (" + string.Join(", ", alertItemNums) + ")");
    }
}