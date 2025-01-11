using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ActiveInstanceCrud
{
    public static ActiveInstance SelectOne(long activeInstanceNum)
    {
        var command = "SELECT * FROM activeinstance "
                      + "WHERE ActiveInstanceNum = " + SOut.Long(activeInstanceNum);
        var list = TableToList(DataCore.GetTable(command));
        return list.Count == 0 ? null : list[0];
    }

    public static ActiveInstance SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list.Count == 0 ? null : list[0];
    }

    public static List<ActiveInstance> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ActiveInstance> TableToList(DataTable table)
    {
        var retVal = new List<ActiveInstance>();
        ActiveInstance activeInstance;
        foreach (DataRow row in table.Rows)
        {
            activeInstance = new ActiveInstance();
            activeInstance.ActiveInstanceNum = SIn.Long(row["ActiveInstanceNum"].ToString());
            activeInstance.ComputerNum = SIn.Long(row["ComputerNum"].ToString());
            activeInstance.UserNum = SIn.Long(row["UserNum"].ToString());
            activeInstance.ProcessId = SIn.Long(row["ProcessId"].ToString());
            activeInstance.DateTimeLastActive = SIn.DateTime(row["DateTimeLastActive"].ToString());
            activeInstance.DateTRecorded = SIn.DateTime(row["DateTRecorded"].ToString());
            activeInstance.ConnectionType = (ConnectionTypes) SIn.Int(row["ConnectionType"].ToString());
            retVal.Add(activeInstance);
        }

        return retVal;
    }

    public static long Insert(ActiveInstance activeInstance)
    {
        var command = "INSERT INTO activeinstance (";

        command += "ComputerNum,UserNum,ProcessId,DateTimeLastActive,DateTRecorded,ConnectionType) VALUES(";

        command +=
            SOut.Long(activeInstance.ComputerNum) + ","
                                                  + SOut.Long(activeInstance.UserNum) + ","
                                                  + SOut.Long(activeInstance.ProcessId) + ","
                                                  + SOut.DateT(activeInstance.DateTimeLastActive) + ","
                                                  + SOut.DateT(activeInstance.DateTRecorded) + ","
                                                  + SOut.Int((int) activeInstance.ConnectionType) + ")";

        activeInstance.ActiveInstanceNum = Db.NonQ(command, true, "ActiveInstanceNum", "activeInstance");
        return activeInstance.ActiveInstanceNum;
    }

    public static void Update(ActiveInstance activeInstance)
    {
        var command = "UPDATE activeinstance SET "
                      + "ComputerNum       =  " + SOut.Long(activeInstance.ComputerNum) + ", "
                      + "UserNum           =  " + SOut.Long(activeInstance.UserNum) + ", "
                      + "ProcessId         =  " + SOut.Long(activeInstance.ProcessId) + ", "
                      + "DateTimeLastActive=  " + SOut.DateT(activeInstance.DateTimeLastActive) + ", "
                      + "DateTRecorded     =  " + SOut.DateT(activeInstance.DateTRecorded) + ", "
                      + "ConnectionType    =  " + SOut.Int((int) activeInstance.ConnectionType) + " "
                      + "WHERE ActiveInstanceNum = " + SOut.Long(activeInstance.ActiveInstanceNum);
        Db.NonQ(command);
    }

    public static void Delete(long activeInstanceNum)
    {
        var command = "DELETE FROM activeinstance "
                      + "WHERE ActiveInstanceNum = " + SOut.Long(activeInstanceNum);
        Db.NonQ(command);
    }
}