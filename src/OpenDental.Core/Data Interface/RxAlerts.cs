using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class RxAlerts
{
    ///<summary>Gets a list of all RxAlerts for one RxDef.</summary>
    public static List<RxAlert> Refresh(long rxDefNum)
    {
        var command = "SELECT * FROM rxalert WHERE RxDefNum=" + SOut.Long(rxDefNum);
        return RxAlertCrud.SelectMany(command);
    }

    
    public static List<RxAlert> TableToList(DataTable table)
    {
        return RxAlertCrud.TableToList(table);
    }

    
    public static void Update(RxAlert rxAlert)
    {
        RxAlertCrud.Update(rxAlert);
    }

    
    public static long Insert(RxAlert rxAlert)
    {
        return RxAlertCrud.Insert(rxAlert);
    }

    
    public static void Delete(RxAlert rxAlert)
    {
        var command = "DELETE FROM rxalert WHERE RxAlertNum =" + SOut.Long(rxAlert.RxAlertNum);
        Db.NonQ(command);
    }
}