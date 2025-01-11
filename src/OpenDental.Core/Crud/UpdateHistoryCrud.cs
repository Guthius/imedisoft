#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UpdateHistoryCrud
{
    public static UpdateHistory SelectOne(long updateHistoryNum)
    {
        var command = "SELECT * FROM updatehistory "
                      + "WHERE UpdateHistoryNum = " + SOut.Long(updateHistoryNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static UpdateHistory SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UpdateHistory> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UpdateHistory> TableToList(DataTable table)
    {
        var retVal = new List<UpdateHistory>();
        UpdateHistory updateHistory;
        foreach (DataRow row in table.Rows)
        {
            updateHistory = new UpdateHistory();
            updateHistory.UpdateHistoryNum = SIn.Long(row["UpdateHistoryNum"].ToString());
            updateHistory.DateTimeUpdated = SIn.DateTime(row["DateTimeUpdated"].ToString());
            updateHistory.ProgramVersion = SIn.String(row["ProgramVersion"].ToString());
            updateHistory.Signature = SIn.String(row["Signature"].ToString());
            retVal.Add(updateHistory);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UpdateHistory> listUpdateHistorys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UpdateHistory";
        var table = new DataTable(tableName);
        table.Columns.Add("UpdateHistoryNum");
        table.Columns.Add("DateTimeUpdated");
        table.Columns.Add("ProgramVersion");
        table.Columns.Add("Signature");
        foreach (var updateHistory in listUpdateHistorys)
            table.Rows.Add(SOut.Long(updateHistory.UpdateHistoryNum), SOut.DateT(updateHistory.DateTimeUpdated, false), updateHistory.ProgramVersion, updateHistory.Signature);
        return table;
    }

    public static long Insert(UpdateHistory updateHistory)
    {
        return Insert(updateHistory, false);
    }

    public static long Insert(UpdateHistory updateHistory, bool useExistingPK)
    {
        var command = "INSERT INTO updatehistory (";

        command += "DateTimeUpdated,ProgramVersion,Signature) VALUES(";

        command +=
            DbHelper.Now() + ","
                           + "'" + SOut.String(updateHistory.ProgramVersion) + "',"
                           + DbHelper.ParamChar + "paramSignature)";
        if (updateHistory.Signature == null) updateHistory.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(updateHistory.Signature));
        {
            updateHistory.UpdateHistoryNum = Db.NonQ(command, true, "UpdateHistoryNum", "updateHistory", paramSignature);
        }
        return updateHistory.UpdateHistoryNum;
    }

    public static long InsertNoCache(UpdateHistory updateHistory)
    {
        return InsertNoCache(updateHistory, false);
    }

    public static long InsertNoCache(UpdateHistory updateHistory, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO updatehistory (";
        if (isRandomKeys || useExistingPK) command += "UpdateHistoryNum,";
        command += "DateTimeUpdated,ProgramVersion,Signature) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(updateHistory.UpdateHistoryNum) + ",";
        command +=
            DbHelper.Now() + ","
                           + "'" + SOut.String(updateHistory.ProgramVersion) + "',"
                           + DbHelper.ParamChar + "paramSignature)";
        if (updateHistory.Signature == null) updateHistory.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(updateHistory.Signature));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramSignature);
        else
            updateHistory.UpdateHistoryNum = Db.NonQ(command, true, "UpdateHistoryNum", "updateHistory", paramSignature);
        return updateHistory.UpdateHistoryNum;
    }

    public static void Update(UpdateHistory updateHistory)
    {
        var command = "UPDATE updatehistory SET "
                      //DateTimeUpdated not allowed to change
                      + "ProgramVersion  = '" + SOut.String(updateHistory.ProgramVersion) + "', "
                      + "Signature       =  " + DbHelper.ParamChar + "paramSignature "
                      + "WHERE UpdateHistoryNum = " + SOut.Long(updateHistory.UpdateHistoryNum);
        if (updateHistory.Signature == null) updateHistory.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(updateHistory.Signature));
        Db.NonQ(command, paramSignature);
    }

    public static bool Update(UpdateHistory updateHistory, UpdateHistory oldUpdateHistory)
    {
        var command = "";
        //DateTimeUpdated not allowed to change
        if (updateHistory.ProgramVersion != oldUpdateHistory.ProgramVersion)
        {
            if (command != "") command += ",";
            command += "ProgramVersion = '" + SOut.String(updateHistory.ProgramVersion) + "'";
        }

        if (updateHistory.Signature != oldUpdateHistory.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (command == "") return false;
        if (updateHistory.Signature == null) updateHistory.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(updateHistory.Signature));
        command = "UPDATE updatehistory SET " + command
                                              + " WHERE UpdateHistoryNum = " + SOut.Long(updateHistory.UpdateHistoryNum);
        Db.NonQ(command, paramSignature);
        return true;
    }

    public static bool UpdateComparison(UpdateHistory updateHistory, UpdateHistory oldUpdateHistory)
    {
        //DateTimeUpdated not allowed to change
        if (updateHistory.ProgramVersion != oldUpdateHistory.ProgramVersion) return true;
        if (updateHistory.Signature != oldUpdateHistory.Signature) return true;
        return false;
    }

    public static void Delete(long updateHistoryNum)
    {
        var command = "DELETE FROM updatehistory "
                      + "WHERE UpdateHistoryNum = " + SOut.Long(updateHistoryNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUpdateHistoryNums)
    {
        if (listUpdateHistoryNums == null || listUpdateHistoryNums.Count == 0) return;
        var command = "DELETE FROM updatehistory "
                      + "WHERE UpdateHistoryNum IN(" + string.Join(",", listUpdateHistoryNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}