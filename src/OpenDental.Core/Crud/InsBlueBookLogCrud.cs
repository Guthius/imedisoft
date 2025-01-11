#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsBlueBookLogCrud
{
    public static InsBlueBookLog SelectOne(long insBlueBookLogNum)
    {
        var command = "SELECT * FROM insbluebooklog "
                      + "WHERE InsBlueBookLogNum = " + SOut.Long(insBlueBookLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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

    public static DataTable ListToTable(List<InsBlueBookLog> listInsBlueBookLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsBlueBookLog";
        var table = new DataTable(tableName);
        table.Columns.Add("InsBlueBookLogNum");
        table.Columns.Add("ClaimProcNum");
        table.Columns.Add("AllowedFee");
        table.Columns.Add("DateTEntry");
        table.Columns.Add("Description");
        foreach (var insBlueBookLog in listInsBlueBookLogs)
            table.Rows.Add(SOut.Long(insBlueBookLog.InsBlueBookLogNum), SOut.Long(insBlueBookLog.ClaimProcNum), SOut.Double(insBlueBookLog.AllowedFee), SOut.DateT(insBlueBookLog.DateTEntry, false), insBlueBookLog.Description);
        return table;
    }

    public static long Insert(InsBlueBookLog insBlueBookLog)
    {
        return Insert(insBlueBookLog, false);
    }

    public static long Insert(InsBlueBookLog insBlueBookLog, bool useExistingPK)
    {
        var command = "INSERT INTO insbluebooklog (";

        command += "ClaimProcNum,AllowedFee,DateTEntry,Description) VALUES(";

        command +=
            SOut.Long(insBlueBookLog.ClaimProcNum) + ","
                                                   + SOut.Double(insBlueBookLog.AllowedFee) + ","
                                                   + DbHelper.Now() + ","
                                                   + DbHelper.ParamChar + "paramDescription)";
        if (insBlueBookLog.Description == null) insBlueBookLog.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(insBlueBookLog.Description));
        {
            insBlueBookLog.InsBlueBookLogNum = Db.NonQ(command, true, "InsBlueBookLogNum", "insBlueBookLog", paramDescription);
        }
        return insBlueBookLog.InsBlueBookLogNum;
    }

    public static long InsertNoCache(InsBlueBookLog insBlueBookLog)
    {
        return InsertNoCache(insBlueBookLog, false);
    }

    public static long InsertNoCache(InsBlueBookLog insBlueBookLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insbluebooklog (";
        if (isRandomKeys || useExistingPK) command += "InsBlueBookLogNum,";
        command += "ClaimProcNum,AllowedFee,DateTEntry,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insBlueBookLog.InsBlueBookLogNum) + ",";
        command +=
            SOut.Long(insBlueBookLog.ClaimProcNum) + ","
                                                   + SOut.Double(insBlueBookLog.AllowedFee) + ","
                                                   + DbHelper.Now() + ","
                                                   + DbHelper.ParamChar + "paramDescription)";
        if (insBlueBookLog.Description == null) insBlueBookLog.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(insBlueBookLog.Description));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription);
        else
            insBlueBookLog.InsBlueBookLogNum = Db.NonQ(command, true, "InsBlueBookLogNum", "insBlueBookLog", paramDescription);
        return insBlueBookLog.InsBlueBookLogNum;
    }

    public static void Update(InsBlueBookLog insBlueBookLog)
    {
        var command = "UPDATE insbluebooklog SET "
                      + "ClaimProcNum     =  " + SOut.Long(insBlueBookLog.ClaimProcNum) + ", "
                      + "AllowedFee       =  " + SOut.Double(insBlueBookLog.AllowedFee) + ", "
                      //DateTEntry not allowed to change
                      + "Description      =  " + DbHelper.ParamChar + "paramDescription "
                      + "WHERE InsBlueBookLogNum = " + SOut.Long(insBlueBookLog.InsBlueBookLogNum);
        if (insBlueBookLog.Description == null) insBlueBookLog.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(insBlueBookLog.Description));
        Db.NonQ(command, paramDescription);
    }

    public static bool Update(InsBlueBookLog insBlueBookLog, InsBlueBookLog oldInsBlueBookLog)
    {
        var command = "";
        if (insBlueBookLog.ClaimProcNum != oldInsBlueBookLog.ClaimProcNum)
        {
            if (command != "") command += ",";
            command += "ClaimProcNum = " + SOut.Long(insBlueBookLog.ClaimProcNum) + "";
        }

        if (insBlueBookLog.AllowedFee != oldInsBlueBookLog.AllowedFee)
        {
            if (command != "") command += ",";
            command += "AllowedFee = " + SOut.Double(insBlueBookLog.AllowedFee) + "";
        }

        //DateTEntry not allowed to change
        if (insBlueBookLog.Description != oldInsBlueBookLog.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (command == "") return false;
        if (insBlueBookLog.Description == null) insBlueBookLog.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(insBlueBookLog.Description));
        command = "UPDATE insbluebooklog SET " + command
                                               + " WHERE InsBlueBookLogNum = " + SOut.Long(insBlueBookLog.InsBlueBookLogNum);
        Db.NonQ(command, paramDescription);
        return true;
    }

    public static bool UpdateComparison(InsBlueBookLog insBlueBookLog, InsBlueBookLog oldInsBlueBookLog)
    {
        if (insBlueBookLog.ClaimProcNum != oldInsBlueBookLog.ClaimProcNum) return true;
        if (insBlueBookLog.AllowedFee != oldInsBlueBookLog.AllowedFee) return true;
        //DateTEntry not allowed to change
        if (insBlueBookLog.Description != oldInsBlueBookLog.Description) return true;
        return false;
    }

    public static void Delete(long insBlueBookLogNum)
    {
        var command = "DELETE FROM insbluebooklog "
                      + "WHERE InsBlueBookLogNum = " + SOut.Long(insBlueBookLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsBlueBookLogNums)
    {
        if (listInsBlueBookLogNums == null || listInsBlueBookLogNums.Count == 0) return;
        var command = "DELETE FROM insbluebooklog "
                      + "WHERE InsBlueBookLogNum IN(" + string.Join(",", listInsBlueBookLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}