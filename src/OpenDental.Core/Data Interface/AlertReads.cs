using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AlertReads
{
    public static List<AlertRead> RefreshForAlertNums(long patNum, List<long> listAlertItemNums)
    {
        if (listAlertItemNums == null || listAlertItemNums.Count == 0) return new List<AlertRead>();
        var command = "SELECT * FROM alertread WHERE UserNum = " + SOut.Long(patNum) + " ";
        command += "AND  AlertItemNum IN (" + string.Join(",", listAlertItemNums) + ")";
        return AlertReadCrud.SelectMany(command);
    }

    public static void Insert(AlertRead alertRead)
    {
        AlertReadCrud.Insert(alertRead);
    }

    public static void DeleteForAlertItems(List<long> listAlertItemNums)
    {
        if (listAlertItemNums == null || listAlertItemNums.Count == 0) return;
        var command = "DELETE FROM alertread "
                      + "WHERE AlertItemNum IN (" + string.Join(",", listAlertItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}