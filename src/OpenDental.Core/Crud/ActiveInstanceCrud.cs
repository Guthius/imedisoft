using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ActiveInstanceCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var activeInstance = new ActiveInstance
            {
                ActiveInstanceNum = SIn.Long(row["ActiveInstanceNum"].ToString()),
                ComputerNum = SIn.Long(row["ComputerNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                ProcessId = SIn.Long(row["ProcessId"].ToString()),
                DateTimeLastActive = SIn.DateTime(row["DateTimeLastActive"].ToString()),
                DateTRecorded = SIn.DateTime(row["DateTRecorded"].ToString()),
                ConnectionType = (ConnectionTypes) SIn.Int(row["ConnectionType"].ToString())
            };
            retVal.Add(activeInstance);
        }

        return retVal;
    }

    public static void Insert(ActiveInstance activeInstance)
    {
        var command = "INSERT INTO activeinstance (";

        command += "ComputerNum,UserNum,ProcessId,DateTimeLastActive,DateTRecorded,ConnectionType) VALUES(";

        command +=
            SOut.Long(activeInstance.ComputerNum) + ","
                                                  + SOut.Long(activeInstance.UserNum) + ","
                                                  + SOut.Long(activeInstance.ProcessId) + ","
                                                  + SOut.DateTime(activeInstance.DateTimeLastActive) + ","
                                                  + SOut.DateTime(activeInstance.DateTRecorded) + ","
                                                  + SOut.Int((int) activeInstance.ConnectionType) + ")";

        activeInstance.ActiveInstanceNum = Db.NonQ(command, true, "ActiveInstanceNum", "activeInstance");
    }

    public static void Update(ActiveInstance activeInstance)
    {
        var command = "UPDATE activeinstance SET "
                      + "ComputerNum       =  " + SOut.Long(activeInstance.ComputerNum) + ", "
                      + "UserNum           =  " + SOut.Long(activeInstance.UserNum) + ", "
                      + "ProcessId         =  " + SOut.Long(activeInstance.ProcessId) + ", "
                      + "DateTimeLastActive=  " + SOut.DateTime(activeInstance.DateTimeLastActive) + ", "
                      + "DateTRecorded     =  " + SOut.DateTime(activeInstance.DateTRecorded) + ", "
                      + "ConnectionType    =  " + SOut.Int((int) activeInstance.ConnectionType) + " "
                      + "WHERE ActiveInstanceNum = " + SOut.Long(activeInstance.ActiveInstanceNum);
        Db.NonQ(command);
    }
}