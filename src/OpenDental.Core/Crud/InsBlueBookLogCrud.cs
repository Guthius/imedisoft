using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsBlueBookLogCrud
{
    public static InsBlueBookLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsBlueBookLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsBlueBookLog> TableToList(DataTable table)
    {
        var retVal = new List<InsBlueBookLog>();
        InsBlueBookLog insBlueBookLog;
        foreach (DataRow row in table.Rows)
        {
            insBlueBookLog = new InsBlueBookLog();
            insBlueBookLog.InsBlueBookLogNum = SIn.Long(row["InsBlueBookLogNum"].ToString());
            insBlueBookLog.ClaimProcNum = SIn.Long(row["ClaimProcNum"].ToString());
            insBlueBookLog.AllowedFee = SIn.Double(row["AllowedFee"].ToString());
            insBlueBookLog.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            insBlueBookLog.Description = SIn.String(row["Description"].ToString());
            retVal.Add(insBlueBookLog);
        }

        return retVal;
    }

    public static void Insert(InsBlueBookLog insBlueBookLog)
    {
        var command = "INSERT INTO insbluebooklog (";

        command += "ClaimProcNum,AllowedFee,DateTEntry,Description) VALUES(";

        command +=
            SOut.Long(insBlueBookLog.ClaimProcNum) + ","
                                                   + SOut.Double(insBlueBookLog.AllowedFee) + ","
                                                   + "NOW()" + ","
                                                   + DbHelper.ParamChar + "paramDescription)";
        if (insBlueBookLog.Description == null) insBlueBookLog.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(insBlueBookLog.Description));
        {
            insBlueBookLog.InsBlueBookLogNum = Db.NonQ(command, true, "InsBlueBookLogNum", "insBlueBookLog", paramDescription);
        }
    }
}