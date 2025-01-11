#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrProvKeyCrud
{
    public static EhrProvKey SelectOne(long ehrProvKeyNum)
    {
        var command = "SELECT * FROM ehrprovkey "
                      + "WHERE EhrProvKeyNum = " + SOut.Long(ehrProvKeyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrProvKey SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrProvKey> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrProvKey> TableToList(DataTable table)
    {
        var retVal = new List<EhrProvKey>();
        EhrProvKey ehrProvKey;
        foreach (DataRow row in table.Rows)
        {
            ehrProvKey = new EhrProvKey();
            ehrProvKey.EhrProvKeyNum = SIn.Long(row["EhrProvKeyNum"].ToString());
            ehrProvKey.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrProvKey.LName = SIn.String(row["LName"].ToString());
            ehrProvKey.FName = SIn.String(row["FName"].ToString());
            ehrProvKey.ProvKey = SIn.String(row["ProvKey"].ToString());
            ehrProvKey.FullTimeEquiv = SIn.Float(row["FullTimeEquiv"].ToString());
            ehrProvKey.Notes = SIn.String(row["Notes"].ToString());
            ehrProvKey.YearValue = SIn.Int(row["YearValue"].ToString());
            retVal.Add(ehrProvKey);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrProvKey> listEhrProvKeys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrProvKey";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrProvKeyNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("LName");
        table.Columns.Add("FName");
        table.Columns.Add("ProvKey");
        table.Columns.Add("FullTimeEquiv");
        table.Columns.Add("Notes");
        table.Columns.Add("YearValue");
        foreach (var ehrProvKey in listEhrProvKeys)
            table.Rows.Add(SOut.Long(ehrProvKey.EhrProvKeyNum), SOut.Long(ehrProvKey.PatNum), ehrProvKey.LName, ehrProvKey.FName, ehrProvKey.ProvKey, SOut.Float(ehrProvKey.FullTimeEquiv), ehrProvKey.Notes, SOut.Int(ehrProvKey.YearValue));
        return table;
    }

    public static long Insert(EhrProvKey ehrProvKey)
    {
        return Insert(ehrProvKey, false);
    }

    public static long Insert(EhrProvKey ehrProvKey, bool useExistingPK)
    {
        var command = "INSERT INTO ehrprovkey (";

        command += "PatNum,LName,FName,ProvKey,FullTimeEquiv,Notes,YearValue) VALUES(";

        command +=
            SOut.Long(ehrProvKey.PatNum) + ","
                                         + "'" + SOut.String(ehrProvKey.LName) + "',"
                                         + "'" + SOut.String(ehrProvKey.FName) + "',"
                                         + "'" + SOut.String(ehrProvKey.ProvKey) + "',"
                                         + SOut.Float(ehrProvKey.FullTimeEquiv) + ","
                                         + DbHelper.ParamChar + "paramNotes,"
                                         + SOut.Int(ehrProvKey.YearValue) + ")";
        if (ehrProvKey.Notes == null) ehrProvKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrProvKey.Notes));
        {
            ehrProvKey.EhrProvKeyNum = Db.NonQ(command, true, "EhrProvKeyNum", "ehrProvKey", paramNotes);
        }
        return ehrProvKey.EhrProvKeyNum;
    }

    public static long InsertNoCache(EhrProvKey ehrProvKey)
    {
        return InsertNoCache(ehrProvKey, false);
    }

    public static long InsertNoCache(EhrProvKey ehrProvKey, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrprovkey (";
        if (isRandomKeys || useExistingPK) command += "EhrProvKeyNum,";
        command += "PatNum,LName,FName,ProvKey,FullTimeEquiv,Notes,YearValue) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrProvKey.EhrProvKeyNum) + ",";
        command +=
            SOut.Long(ehrProvKey.PatNum) + ","
                                         + "'" + SOut.String(ehrProvKey.LName) + "',"
                                         + "'" + SOut.String(ehrProvKey.FName) + "',"
                                         + "'" + SOut.String(ehrProvKey.ProvKey) + "',"
                                         + SOut.Float(ehrProvKey.FullTimeEquiv) + ","
                                         + DbHelper.ParamChar + "paramNotes,"
                                         + SOut.Int(ehrProvKey.YearValue) + ")";
        if (ehrProvKey.Notes == null) ehrProvKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrProvKey.Notes));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNotes);
        else
            ehrProvKey.EhrProvKeyNum = Db.NonQ(command, true, "EhrProvKeyNum", "ehrProvKey", paramNotes);
        return ehrProvKey.EhrProvKeyNum;
    }

    public static void Update(EhrProvKey ehrProvKey)
    {
        var command = "UPDATE ehrprovkey SET "
                      + "PatNum       =  " + SOut.Long(ehrProvKey.PatNum) + ", "
                      + "LName        = '" + SOut.String(ehrProvKey.LName) + "', "
                      + "FName        = '" + SOut.String(ehrProvKey.FName) + "', "
                      + "ProvKey      = '" + SOut.String(ehrProvKey.ProvKey) + "', "
                      + "FullTimeEquiv=  " + SOut.Float(ehrProvKey.FullTimeEquiv) + ", "
                      + "Notes        =  " + DbHelper.ParamChar + "paramNotes, "
                      + "YearValue    =  " + SOut.Int(ehrProvKey.YearValue) + " "
                      + "WHERE EhrProvKeyNum = " + SOut.Long(ehrProvKey.EhrProvKeyNum);
        if (ehrProvKey.Notes == null) ehrProvKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrProvKey.Notes));
        Db.NonQ(command, paramNotes);
    }

    public static bool Update(EhrProvKey ehrProvKey, EhrProvKey oldEhrProvKey)
    {
        var command = "";
        if (ehrProvKey.PatNum != oldEhrProvKey.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrProvKey.PatNum) + "";
        }

        if (ehrProvKey.LName != oldEhrProvKey.LName)
        {
            if (command != "") command += ",";
            command += "LName = '" + SOut.String(ehrProvKey.LName) + "'";
        }

        if (ehrProvKey.FName != oldEhrProvKey.FName)
        {
            if (command != "") command += ",";
            command += "FName = '" + SOut.String(ehrProvKey.FName) + "'";
        }

        if (ehrProvKey.ProvKey != oldEhrProvKey.ProvKey)
        {
            if (command != "") command += ",";
            command += "ProvKey = '" + SOut.String(ehrProvKey.ProvKey) + "'";
        }

        if (ehrProvKey.FullTimeEquiv != oldEhrProvKey.FullTimeEquiv)
        {
            if (command != "") command += ",";
            command += "FullTimeEquiv = " + SOut.Float(ehrProvKey.FullTimeEquiv) + "";
        }

        if (ehrProvKey.Notes != oldEhrProvKey.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = " + DbHelper.ParamChar + "paramNotes";
        }

        if (ehrProvKey.YearValue != oldEhrProvKey.YearValue)
        {
            if (command != "") command += ",";
            command += "YearValue = " + SOut.Int(ehrProvKey.YearValue) + "";
        }

        if (command == "") return false;
        if (ehrProvKey.Notes == null) ehrProvKey.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(ehrProvKey.Notes));
        command = "UPDATE ehrprovkey SET " + command
                                           + " WHERE EhrProvKeyNum = " + SOut.Long(ehrProvKey.EhrProvKeyNum);
        Db.NonQ(command, paramNotes);
        return true;
    }

    public static bool UpdateComparison(EhrProvKey ehrProvKey, EhrProvKey oldEhrProvKey)
    {
        if (ehrProvKey.PatNum != oldEhrProvKey.PatNum) return true;
        if (ehrProvKey.LName != oldEhrProvKey.LName) return true;
        if (ehrProvKey.FName != oldEhrProvKey.FName) return true;
        if (ehrProvKey.ProvKey != oldEhrProvKey.ProvKey) return true;
        if (ehrProvKey.FullTimeEquiv != oldEhrProvKey.FullTimeEquiv) return true;
        if (ehrProvKey.Notes != oldEhrProvKey.Notes) return true;
        if (ehrProvKey.YearValue != oldEhrProvKey.YearValue) return true;
        return false;
    }

    public static void Delete(long ehrProvKeyNum)
    {
        var command = "DELETE FROM ehrprovkey "
                      + "WHERE EhrProvKeyNum = " + SOut.Long(ehrProvKeyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrProvKeyNums)
    {
        if (listEhrProvKeyNums == null || listEhrProvKeyNums.Count == 0) return;
        var command = "DELETE FROM ehrprovkey "
                      + "WHERE EhrProvKeyNum IN(" + string.Join(",", listEhrProvKeyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}