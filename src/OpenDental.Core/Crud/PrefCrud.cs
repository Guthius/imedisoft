#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PrefCrud
{
    public static Pref SelectOne(long prefNum)
    {
        var command = "SELECT * FROM preference "
                      + "WHERE PrefNum = " + SOut.Long(prefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Pref SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Pref> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Pref> TableToList(DataTable table)
    {
        var retVal = new List<Pref>();
        Pref pref;
        foreach (DataRow row in table.Rows)
        {
            pref = new Pref();
            pref.PrefNum = SIn.Long(row["PrefNum"].ToString());
            pref.PrefName = SIn.String(row["PrefName"].ToString());
            pref.ValueString = SIn.String(row["ValueString"].ToString());
            pref.Comments = SIn.String(row["Comments"].ToString());
            retVal.Add(pref);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Pref> listPrefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Pref";
        var table = new DataTable(tableName);
        table.Columns.Add("PrefNum");
        table.Columns.Add("PrefName");
        table.Columns.Add("ValueString");
        table.Columns.Add("Comments");
        foreach (var pref in listPrefs)
            table.Rows.Add(SOut.Long(pref.PrefNum), pref.PrefName, pref.ValueString, pref.Comments);
        return table;
    }

    public static long Insert(Pref pref)
    {
        return Insert(pref, false);
    }

    public static long Insert(Pref pref, bool useExistingPK)
    {
        var command = "INSERT INTO preference (";

        command += "PrefName,ValueString,Comments) VALUES(";

        command +=
            "'" + SOut.String(pref.PrefName) + "',"
            + DbHelper.ParamChar + "paramValueString,"
            + DbHelper.ParamChar + "paramComments)";
        if (pref.ValueString == null) pref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(pref.ValueString));
        if (pref.Comments == null) pref.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(pref.Comments));
        {
            pref.PrefNum = Db.NonQ(command, true, "PrefNum", "pref", paramValueString, paramComments);
        }
        return pref.PrefNum;
    }

    public static long InsertNoCache(Pref pref)
    {
        return InsertNoCache(pref, false);
    }

    public static long InsertNoCache(Pref pref, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO preference (";
        if (isRandomKeys || useExistingPK) command += "PrefNum,";
        command += "PrefName,ValueString,Comments) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(pref.PrefNum) + ",";
        command +=
            "'" + SOut.String(pref.PrefName) + "',"
            + DbHelper.ParamChar + "paramValueString,"
            + DbHelper.ParamChar + "paramComments)";
        if (pref.ValueString == null) pref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(pref.ValueString));
        if (pref.Comments == null) pref.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(pref.Comments));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramValueString, paramComments);
        else
            pref.PrefNum = Db.NonQ(command, true, "PrefNum", "pref", paramValueString, paramComments);
        return pref.PrefNum;
    }

    public static void Update(Pref pref)
    {
        var command = "UPDATE preference SET "
                      + "PrefName   = '" + SOut.String(pref.PrefName) + "', "
                      + "ValueString=  " + DbHelper.ParamChar + "paramValueString, "
                      + "Comments   =  " + DbHelper.ParamChar + "paramComments "
                      + "WHERE PrefNum = " + SOut.Long(pref.PrefNum);
        if (pref.ValueString == null) pref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(pref.ValueString));
        if (pref.Comments == null) pref.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(pref.Comments));
        Db.NonQ(command, paramValueString, paramComments);
    }

    public static bool Update(Pref pref, Pref oldPref)
    {
        var command = "";
        if (pref.PrefName != oldPref.PrefName)
        {
            if (command != "") command += ",";
            command += "PrefName = '" + SOut.String(pref.PrefName) + "'";
        }

        if (pref.ValueString != oldPref.ValueString)
        {
            if (command != "") command += ",";
            command += "ValueString = " + DbHelper.ParamChar + "paramValueString";
        }

        if (pref.Comments != oldPref.Comments)
        {
            if (command != "") command += ",";
            command += "Comments = " + DbHelper.ParamChar + "paramComments";
        }

        if (command == "") return false;
        if (pref.ValueString == null) pref.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(pref.ValueString));
        if (pref.Comments == null) pref.Comments = "";
        var paramComments = new OdSqlParameter("paramComments", OdDbType.Text, SOut.StringParam(pref.Comments));
        command = "UPDATE preference SET " + command
                                           + " WHERE PrefNum = " + SOut.Long(pref.PrefNum);
        Db.NonQ(command, paramValueString, paramComments);
        return true;
    }

    public static bool UpdateComparison(Pref pref, Pref oldPref)
    {
        if (pref.PrefName != oldPref.PrefName) return true;
        if (pref.ValueString != oldPref.ValueString) return true;
        if (pref.Comments != oldPref.Comments) return true;
        return false;
    }

    public static void Delete(long prefNum)
    {
        var command = "DELETE FROM preference "
                      + "WHERE PrefNum = " + SOut.Long(prefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPrefNums)
    {
        if (listPrefNums == null || listPrefNums.Count == 0) return;
        var command = "DELETE FROM preference "
                      + "WHERE PrefNum IN(" + string.Join(",", listPrefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}