#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrQuarterlyKeyCrud
{
    public static EhrQuarterlyKey SelectOne(long ehrQuarterlyKeyNum)
    {
        var command = "SELECT * FROM ehrquarterlykey "
                      + "WHERE EhrQuarterlyKeyNum = " + SOut.Long(ehrQuarterlyKeyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrQuarterlyKey SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrQuarterlyKey> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrQuarterlyKey> TableToList(DataTable table)
    {
        var retVal = new List<EhrQuarterlyKey>();
        EhrQuarterlyKey ehrQuarterlyKey;
        foreach (DataRow row in table.Rows)
        {
            ehrQuarterlyKey = new EhrQuarterlyKey();
            ehrQuarterlyKey.EhrQuarterlyKeyNum = SIn.Long(row["EhrQuarterlyKeyNum"].ToString());
            ehrQuarterlyKey.YearValue = SIn.Int(row["YearValue"].ToString());
            ehrQuarterlyKey.QuarterValue = SIn.Int(row["QuarterValue"].ToString());
            ehrQuarterlyKey.PracticeName = SIn.String(row["PracticeName"].ToString());
            ehrQuarterlyKey.KeyValue = SIn.String(row["KeyValue"].ToString());
            ehrQuarterlyKey.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrQuarterlyKey.Notes = SIn.String(row["Notes"].ToString());
            retVal.Add(ehrQuarterlyKey);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrQuarterlyKey> listEhrQuarterlyKeys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrQuarterlyKey";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrQuarterlyKeyNum");
        table.Columns.Add("YearValue");
        table.Columns.Add("QuarterValue");
        table.Columns.Add("PracticeName");
        table.Columns.Add("KeyValue");
        table.Columns.Add("PatNum");
        table.Columns.Add("Notes");
        foreach (var ehrQuarterlyKey in listEhrQuarterlyKeys)
            table.Rows.Add(SOut.Long(ehrQuarterlyKey.EhrQuarterlyKeyNum), SOut.Int(ehrQuarterlyKey.YearValue), SOut.Int(ehrQuarterlyKey.QuarterValue), ehrQuarterlyKey.PracticeName, ehrQuarterlyKey.KeyValue, SOut.Long(ehrQuarterlyKey.PatNum), ehrQuarterlyKey.Notes);
        return table;
    }

    public static long Insert(EhrQuarterlyKey ehrQuarterlyKey)
    {
        return Insert(ehrQuarterlyKey, false);
    }

    public static long Insert(EhrQuarterlyKey ehrQuarterlyKey, bool useExistingPK)
    {
        var command = "INSERT INTO ehrquarterlykey (";

        command += "YearValue,QuarterValue,PracticeName,KeyValue,PatNum,Notes) VALUES(";

        command +=
            SOut.Int(ehrQuarterlyKey.YearValue) + ","
                                                + SOut.Int(ehrQuarterlyKey.QuarterValue) + ","
                                                + "'" + SOut.String(ehrQuarterlyKey.PracticeName) + "',"
                                                + "'" + SOut.String(ehrQuarterlyKey.KeyValue) + "',"
                                                + SOut.Long(ehrQuarterlyKey.PatNum) + ","
                                                + DbHelper.ParamChar + "paramNotes)";
        if (ehrQuarterlyKey.Notes == null) ehrQuarterlyKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrQuarterlyKey.Notes));
        {
            ehrQuarterlyKey.EhrQuarterlyKeyNum = Db.NonQ(command, true, "EhrQuarterlyKeyNum", "ehrQuarterlyKey", paramNotes);
        }
        return ehrQuarterlyKey.EhrQuarterlyKeyNum;
    }

    public static long InsertNoCache(EhrQuarterlyKey ehrQuarterlyKey)
    {
        return InsertNoCache(ehrQuarterlyKey, false);
    }

    public static long InsertNoCache(EhrQuarterlyKey ehrQuarterlyKey, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrquarterlykey (";
        if (isRandomKeys || useExistingPK) command += "EhrQuarterlyKeyNum,";
        command += "YearValue,QuarterValue,PracticeName,KeyValue,PatNum,Notes) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrQuarterlyKey.EhrQuarterlyKeyNum) + ",";
        command +=
            SOut.Int(ehrQuarterlyKey.YearValue) + ","
                                                + SOut.Int(ehrQuarterlyKey.QuarterValue) + ","
                                                + "'" + SOut.String(ehrQuarterlyKey.PracticeName) + "',"
                                                + "'" + SOut.String(ehrQuarterlyKey.KeyValue) + "',"
                                                + SOut.Long(ehrQuarterlyKey.PatNum) + ","
                                                + DbHelper.ParamChar + "paramNotes)";
        if (ehrQuarterlyKey.Notes == null) ehrQuarterlyKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrQuarterlyKey.Notes));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNotes);
        else
            ehrQuarterlyKey.EhrQuarterlyKeyNum = Db.NonQ(command, true, "EhrQuarterlyKeyNum", "ehrQuarterlyKey", paramNotes);
        return ehrQuarterlyKey.EhrQuarterlyKeyNum;
    }

    public static void Update(EhrQuarterlyKey ehrQuarterlyKey)
    {
        var command = "UPDATE ehrquarterlykey SET "
                      + "YearValue         =  " + SOut.Int(ehrQuarterlyKey.YearValue) + ", "
                      + "QuarterValue      =  " + SOut.Int(ehrQuarterlyKey.QuarterValue) + ", "
                      + "PracticeName      = '" + SOut.String(ehrQuarterlyKey.PracticeName) + "', "
                      + "KeyValue          = '" + SOut.String(ehrQuarterlyKey.KeyValue) + "', "
                      + "PatNum            =  " + SOut.Long(ehrQuarterlyKey.PatNum) + ", "
                      + "Notes             =  " + DbHelper.ParamChar + "paramNotes "
                      + "WHERE EhrQuarterlyKeyNum = " + SOut.Long(ehrQuarterlyKey.EhrQuarterlyKeyNum);
        if (ehrQuarterlyKey.Notes == null) ehrQuarterlyKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrQuarterlyKey.Notes));
        Db.NonQ(command, paramNotes);
    }

    public static bool Update(EhrQuarterlyKey ehrQuarterlyKey, EhrQuarterlyKey oldEhrQuarterlyKey)
    {
        var command = "";
        if (ehrQuarterlyKey.YearValue != oldEhrQuarterlyKey.YearValue)
        {
            if (command != "") command += ",";
            command += "YearValue = " + SOut.Int(ehrQuarterlyKey.YearValue) + "";
        }

        if (ehrQuarterlyKey.QuarterValue != oldEhrQuarterlyKey.QuarterValue)
        {
            if (command != "") command += ",";
            command += "QuarterValue = " + SOut.Int(ehrQuarterlyKey.QuarterValue) + "";
        }

        if (ehrQuarterlyKey.PracticeName != oldEhrQuarterlyKey.PracticeName)
        {
            if (command != "") command += ",";
            command += "PracticeName = '" + SOut.String(ehrQuarterlyKey.PracticeName) + "'";
        }

        if (ehrQuarterlyKey.KeyValue != oldEhrQuarterlyKey.KeyValue)
        {
            if (command != "") command += ",";
            command += "KeyValue = '" + SOut.String(ehrQuarterlyKey.KeyValue) + "'";
        }

        if (ehrQuarterlyKey.PatNum != oldEhrQuarterlyKey.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrQuarterlyKey.PatNum) + "";
        }

        if (ehrQuarterlyKey.Notes != oldEhrQuarterlyKey.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = " + DbHelper.ParamChar + "paramNotes";
        }

        if (command == "") return false;
        if (ehrQuarterlyKey.Notes == null) ehrQuarterlyKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrQuarterlyKey.Notes));
        command = "UPDATE ehrquarterlykey SET " + command
                                                + " WHERE EhrQuarterlyKeyNum = " + SOut.Long(ehrQuarterlyKey.EhrQuarterlyKeyNum);
        Db.NonQ(command, paramNotes);
        return true;
    }

    public static bool UpdateComparison(EhrQuarterlyKey ehrQuarterlyKey, EhrQuarterlyKey oldEhrQuarterlyKey)
    {
        if (ehrQuarterlyKey.YearValue != oldEhrQuarterlyKey.YearValue) return true;
        if (ehrQuarterlyKey.QuarterValue != oldEhrQuarterlyKey.QuarterValue) return true;
        if (ehrQuarterlyKey.PracticeName != oldEhrQuarterlyKey.PracticeName) return true;
        if (ehrQuarterlyKey.KeyValue != oldEhrQuarterlyKey.KeyValue) return true;
        if (ehrQuarterlyKey.PatNum != oldEhrQuarterlyKey.PatNum) return true;
        if (ehrQuarterlyKey.Notes != oldEhrQuarterlyKey.Notes) return true;
        return false;
    }

    public static void Delete(long ehrQuarterlyKeyNum)
    {
        var command = "DELETE FROM ehrquarterlykey "
                      + "WHERE EhrQuarterlyKeyNum = " + SOut.Long(ehrQuarterlyKeyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrQuarterlyKeyNums)
    {
        if (listEhrQuarterlyKeyNums == null || listEhrQuarterlyKeyNums.Count == 0) return;
        var command = "DELETE FROM ehrquarterlykey "
                      + "WHERE EhrQuarterlyKeyNum IN(" + string.Join(",", listEhrQuarterlyKeyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}