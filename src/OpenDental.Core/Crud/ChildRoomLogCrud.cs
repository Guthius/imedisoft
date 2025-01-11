using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ChildRoomLogCrud
{
    public static ChildRoomLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ChildRoomLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ChildRoomLog> TableToList(DataTable table)
    {
        var retVal = new List<ChildRoomLog>();
        ChildRoomLog childRoomLog;
        foreach (DataRow row in table.Rows)
        {
            childRoomLog = new ChildRoomLog();
            childRoomLog.ChildRoomLogNum = SIn.Long(row["ChildRoomLogNum"].ToString());
            childRoomLog.DateTEntered = SIn.DateTime(row["DateTEntered"].ToString());
            childRoomLog.DateTDisplayed = SIn.DateTime(row["DateTDisplayed"].ToString());
            childRoomLog.ChildNum = SIn.Long(row["ChildNum"].ToString());
            childRoomLog.EmployeeNum = SIn.Long(row["EmployeeNum"].ToString());
            childRoomLog.IsComing = SIn.Bool(row["IsComing"].ToString());
            childRoomLog.ChildRoomNum = SIn.Long(row["ChildRoomNum"].ToString());
            childRoomLog.RatioChange = SIn.Double(row["RatioChange"].ToString());
            retVal.Add(childRoomLog);
        }

        return retVal;
    }

    public static long Insert(ChildRoomLog childRoomLog)
    {
        var command = "INSERT INTO childroomlog (";

        command += "DateTEntered,DateTDisplayed,ChildNum,EmployeeNum,IsComing,ChildRoomNum,RatioChange) VALUES(";

        command +=
            SOut.DateT(childRoomLog.DateTEntered) + ","
                                                  + SOut.DateT(childRoomLog.DateTDisplayed) + ","
                                                  + SOut.Long(childRoomLog.ChildNum) + ","
                                                  + SOut.Long(childRoomLog.EmployeeNum) + ","
                                                  + SOut.Bool(childRoomLog.IsComing) + ","
                                                  + SOut.Long(childRoomLog.ChildRoomNum) + ","
                                                  + SOut.Double(childRoomLog.RatioChange) + ")";
        {
            childRoomLog.ChildRoomLogNum = Db.NonQ(command, true, "ChildRoomLogNum", "childRoomLog");
        }
        return childRoomLog.ChildRoomLogNum;
    }

    public static void Update(ChildRoomLog childRoomLog)
    {
        var command = "UPDATE childroomlog SET "
                      + "DateTEntered   =  " + SOut.DateT(childRoomLog.DateTEntered) + ", "
                      + "DateTDisplayed =  " + SOut.DateT(childRoomLog.DateTDisplayed) + ", "
                      + "ChildNum       =  " + SOut.Long(childRoomLog.ChildNum) + ", "
                      + "EmployeeNum    =  " + SOut.Long(childRoomLog.EmployeeNum) + ", "
                      + "IsComing       =  " + SOut.Bool(childRoomLog.IsComing) + ", "
                      + "ChildRoomNum   =  " + SOut.Long(childRoomLog.ChildRoomNum) + ", "
                      + "RatioChange    =  " + SOut.Double(childRoomLog.RatioChange) + " "
                      + "WHERE ChildRoomLogNum = " + SOut.Long(childRoomLog.ChildRoomLogNum);
        Db.NonQ(command);
    }

    public static void Delete(long childRoomLogNum)
    {
        var command = "DELETE FROM childroomlog "
                      + "WHERE ChildRoomLogNum = " + SOut.Long(childRoomLogNum);
        Db.NonQ(command);
    }
}