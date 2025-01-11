using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class RxDefs
{
    
    public static RxDef[] Refresh()
    {
        var command = "SELECT * FROM rxdef ORDER BY Drug";
        return RxDefCrud.SelectMany(command).ToArray();
    }

    public static RxDef GetOne(long rxDefNum)
    {
        return RxDefCrud.SelectOne(rxDefNum);
    }

    
    public static void Update(RxDef rxDef)
    {
        RxDefCrud.Update(rxDef);
    }

    
    public static long Insert(RxDef rxDef)
    {
        return RxDefCrud.Insert(rxDef);
    }

    
    public static List<RxDef> TableToList(DataTable table)
    {
        return RxDefCrud.TableToList(table);
    }

    ///<summary>Also deletes all RxAlerts that were attached.</summary>
    public static void Delete(RxDef rxDef)
    {
        var command = "DELETE FROM rxalert WHERE RxDefNum=" + SOut.Long(rxDef.RxDefNum);
        Db.NonQ(command);
        command = "DELETE FROM rxdef WHERE RxDefNum = " + SOut.Long(rxDef.RxDefNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Used to combine prescriptions by first adjusting the FKs on any necessary tables, then removing the
    ///     prescription from the rxDef table.
    /// </summary>
    public static void Combine(List<long> listRxDefNums, long rxDefNumPicked)
    {
        if (listRxDefNums.Count <= 1) return; //nothing to do

        for (var i = 0; i < listRxDefNums.Count; i++)
        {
            if (listRxDefNums[i] == rxDefNumPicked) continue;
            var command = "UPDATE rxalert SET RxDefNum=" + SOut.Long(rxDefNumPicked) + " "
                          + "WHERE RxDefNum=" + SOut.Long(listRxDefNums[i]);
            Db.NonQ(command);
            command = "DELETE FROM rxdef "
                      + "WHERE RxDefNum=" + SOut.Long(listRxDefNums[i]);
            Db.NonQ(command);
        }
    }
}