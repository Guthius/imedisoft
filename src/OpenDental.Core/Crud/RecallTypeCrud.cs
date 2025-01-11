#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RecallTypeCrud
{
    public static RecallType SelectOne(long recallTypeNum)
    {
        var command = "SELECT * FROM recalltype "
                      + "WHERE RecallTypeNum = " + SOut.Long(recallTypeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RecallType SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RecallType> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RecallType> TableToList(DataTable table)
    {
        var retVal = new List<RecallType>();
        RecallType recallType;
        foreach (DataRow row in table.Rows)
        {
            recallType = new RecallType();
            recallType.RecallTypeNum = SIn.Long(row["RecallTypeNum"].ToString());
            recallType.Description = SIn.String(row["Description"].ToString());
            recallType.DefaultInterval = new Interval(SIn.Int(row["DefaultInterval"].ToString()));
            recallType.TimePattern = SIn.String(row["TimePattern"].ToString());
            recallType.Procedures = SIn.String(row["Procedures"].ToString());
            recallType.AppendToSpecial = SIn.Bool(row["AppendToSpecial"].ToString());
            retVal.Add(recallType);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RecallType> listRecallTypes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RecallType";
        var table = new DataTable(tableName);
        table.Columns.Add("RecallTypeNum");
        table.Columns.Add("Description");
        table.Columns.Add("DefaultInterval");
        table.Columns.Add("TimePattern");
        table.Columns.Add("Procedures");
        table.Columns.Add("AppendToSpecial");
        foreach (var recallType in listRecallTypes)
            table.Rows.Add(SOut.Long(recallType.RecallTypeNum), recallType.Description, SOut.Int(recallType.DefaultInterval.ToInt()), recallType.TimePattern, recallType.Procedures, SOut.Bool(recallType.AppendToSpecial));
        return table;
    }

    public static long Insert(RecallType recallType)
    {
        return Insert(recallType, false);
    }

    public static long Insert(RecallType recallType, bool useExistingPK)
    {
        var command = "INSERT INTO recalltype (";

        command += "Description,DefaultInterval,TimePattern,Procedures,AppendToSpecial) VALUES(";

        command +=
            "'" + SOut.String(recallType.Description) + "',"
            + SOut.Int(recallType.DefaultInterval.ToInt()) + ","
            + "'" + SOut.String(recallType.TimePattern) + "',"
            + "'" + SOut.String(recallType.Procedures) + "',"
            + SOut.Bool(recallType.AppendToSpecial) + ")";
        {
            recallType.RecallTypeNum = Db.NonQ(command, true, "RecallTypeNum", "recallType");
        }
        return recallType.RecallTypeNum;
    }

    public static long InsertNoCache(RecallType recallType)
    {
        return InsertNoCache(recallType, false);
    }

    public static long InsertNoCache(RecallType recallType, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO recalltype (";
        if (isRandomKeys || useExistingPK) command += "RecallTypeNum,";
        command += "Description,DefaultInterval,TimePattern,Procedures,AppendToSpecial) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(recallType.RecallTypeNum) + ",";
        command +=
            "'" + SOut.String(recallType.Description) + "',"
            + SOut.Int(recallType.DefaultInterval.ToInt()) + ","
            + "'" + SOut.String(recallType.TimePattern) + "',"
            + "'" + SOut.String(recallType.Procedures) + "',"
            + SOut.Bool(recallType.AppendToSpecial) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            recallType.RecallTypeNum = Db.NonQ(command, true, "RecallTypeNum", "recallType");
        return recallType.RecallTypeNum;
    }

    public static void Update(RecallType recallType)
    {
        var command = "UPDATE recalltype SET "
                      + "Description    = '" + SOut.String(recallType.Description) + "', "
                      + "DefaultInterval=  " + SOut.Int(recallType.DefaultInterval.ToInt()) + ", "
                      + "TimePattern    = '" + SOut.String(recallType.TimePattern) + "', "
                      + "Procedures     = '" + SOut.String(recallType.Procedures) + "', "
                      + "AppendToSpecial=  " + SOut.Bool(recallType.AppendToSpecial) + " "
                      + "WHERE RecallTypeNum = " + SOut.Long(recallType.RecallTypeNum);
        Db.NonQ(command);
    }

    public static bool Update(RecallType recallType, RecallType oldRecallType)
    {
        var command = "";
        if (recallType.Description != oldRecallType.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(recallType.Description) + "'";
        }

        if (recallType.DefaultInterval != oldRecallType.DefaultInterval)
        {
            if (command != "") command += ",";
            command += "DefaultInterval = " + SOut.Int(recallType.DefaultInterval.ToInt()) + "";
        }

        if (recallType.TimePattern != oldRecallType.TimePattern)
        {
            if (command != "") command += ",";
            command += "TimePattern = '" + SOut.String(recallType.TimePattern) + "'";
        }

        if (recallType.Procedures != oldRecallType.Procedures)
        {
            if (command != "") command += ",";
            command += "Procedures = '" + SOut.String(recallType.Procedures) + "'";
        }

        if (recallType.AppendToSpecial != oldRecallType.AppendToSpecial)
        {
            if (command != "") command += ",";
            command += "AppendToSpecial = " + SOut.Bool(recallType.AppendToSpecial) + "";
        }

        if (command == "") return false;
        command = "UPDATE recalltype SET " + command
                                           + " WHERE RecallTypeNum = " + SOut.Long(recallType.RecallTypeNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(RecallType recallType, RecallType oldRecallType)
    {
        if (recallType.Description != oldRecallType.Description) return true;
        if (recallType.DefaultInterval != oldRecallType.DefaultInterval) return true;
        if (recallType.TimePattern != oldRecallType.TimePattern) return true;
        if (recallType.Procedures != oldRecallType.Procedures) return true;
        if (recallType.AppendToSpecial != oldRecallType.AppendToSpecial) return true;
        return false;
    }

    public static void Delete(long recallTypeNum)
    {
        var command = "DELETE FROM recalltype "
                      + "WHERE RecallTypeNum = " + SOut.Long(recallTypeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRecallTypeNums)
    {
        if (listRecallTypeNums == null || listRecallTypeNums.Count == 0) return;
        var command = "DELETE FROM recalltype "
                      + "WHERE RecallTypeNum IN(" + string.Join(",", listRecallTypeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}