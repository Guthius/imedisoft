#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsPlanPreferenceCrud
{
    public static InsPlanPreference SelectOne(long insPlanPrefNum)
    {
        var command = "SELECT * FROM insplanpreference "
                      + "WHERE InsPlanPrefNum = " + SOut.Long(insPlanPrefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsPlanPreference SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsPlanPreference> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsPlanPreference> TableToList(DataTable table)
    {
        var retVal = new List<InsPlanPreference>();
        InsPlanPreference insPlanPreference;
        foreach (DataRow row in table.Rows)
        {
            insPlanPreference = new InsPlanPreference();
            insPlanPreference.InsPlanPrefNum = SIn.Long(row["InsPlanPrefNum"].ToString());
            insPlanPreference.PlanNum = SIn.Long(row["PlanNum"].ToString());
            insPlanPreference.FKey = SIn.Long(row["FKey"].ToString());
            insPlanPreference.FKeyType = (InsPlanPrefFKeyType) SIn.Int(row["FKeyType"].ToString());
            insPlanPreference.ValueString = SIn.String(row["ValueString"].ToString());
            retVal.Add(insPlanPreference);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsPlanPreference> listInsPlanPreferences, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsPlanPreference";
        var table = new DataTable(tableName);
        table.Columns.Add("InsPlanPrefNum");
        table.Columns.Add("PlanNum");
        table.Columns.Add("FKey");
        table.Columns.Add("FKeyType");
        table.Columns.Add("ValueString");
        foreach (var insPlanPreference in listInsPlanPreferences)
            table.Rows.Add(SOut.Long(insPlanPreference.InsPlanPrefNum), SOut.Long(insPlanPreference.PlanNum), SOut.Long(insPlanPreference.FKey), SOut.Int((int) insPlanPreference.FKeyType), insPlanPreference.ValueString);
        return table;
    }

    public static long Insert(InsPlanPreference insPlanPreference)
    {
        return Insert(insPlanPreference, false);
    }

    public static long Insert(InsPlanPreference insPlanPreference, bool useExistingPK)
    {
        var command = "INSERT INTO insplanpreference (";

        command += "PlanNum,FKey,FKeyType,ValueString) VALUES(";

        command +=
            SOut.Long(insPlanPreference.PlanNum) + ","
                                                 + SOut.Long(insPlanPreference.FKey) + ","
                                                 + SOut.Int((int) insPlanPreference.FKeyType) + ","
                                                 + DbHelper.ParamChar + "paramValueString)";
        if (insPlanPreference.ValueString == null) insPlanPreference.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(insPlanPreference.ValueString));
        {
            insPlanPreference.InsPlanPrefNum = Db.NonQ(command, true, "InsPlanPrefNum", "insPlanPreference", paramValueString);
        }
        return insPlanPreference.InsPlanPrefNum;
    }

    public static long InsertNoCache(InsPlanPreference insPlanPreference)
    {
        return InsertNoCache(insPlanPreference, false);
    }

    public static long InsertNoCache(InsPlanPreference insPlanPreference, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insplanpreference (";
        if (isRandomKeys || useExistingPK) command += "InsPlanPrefNum,";
        command += "PlanNum,FKey,FKeyType,ValueString) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insPlanPreference.InsPlanPrefNum) + ",";
        command +=
            SOut.Long(insPlanPreference.PlanNum) + ","
                                                 + SOut.Long(insPlanPreference.FKey) + ","
                                                 + SOut.Int((int) insPlanPreference.FKeyType) + ","
                                                 + DbHelper.ParamChar + "paramValueString)";
        if (insPlanPreference.ValueString == null) insPlanPreference.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(insPlanPreference.ValueString));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramValueString);
        else
            insPlanPreference.InsPlanPrefNum = Db.NonQ(command, true, "InsPlanPrefNum", "insPlanPreference", paramValueString);
        return insPlanPreference.InsPlanPrefNum;
    }

    public static void Update(InsPlanPreference insPlanPreference)
    {
        var command = "UPDATE insplanpreference SET "
                      + "PlanNum       =  " + SOut.Long(insPlanPreference.PlanNum) + ", "
                      + "FKey          =  " + SOut.Long(insPlanPreference.FKey) + ", "
                      + "FKeyType      =  " + SOut.Int((int) insPlanPreference.FKeyType) + ", "
                      + "ValueString   =  " + DbHelper.ParamChar + "paramValueString "
                      + "WHERE InsPlanPrefNum = " + SOut.Long(insPlanPreference.InsPlanPrefNum);
        if (insPlanPreference.ValueString == null) insPlanPreference.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(insPlanPreference.ValueString));
        Db.NonQ(command, paramValueString);
    }

    public static bool Update(InsPlanPreference insPlanPreference, InsPlanPreference oldInsPlanPreference)
    {
        var command = "";
        if (insPlanPreference.PlanNum != oldInsPlanPreference.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(insPlanPreference.PlanNum) + "";
        }

        if (insPlanPreference.FKey != oldInsPlanPreference.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(insPlanPreference.FKey) + "";
        }

        if (insPlanPreference.FKeyType != oldInsPlanPreference.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = " + SOut.Int((int) insPlanPreference.FKeyType) + "";
        }

        if (insPlanPreference.ValueString != oldInsPlanPreference.ValueString)
        {
            if (command != "") command += ",";
            command += "ValueString = " + DbHelper.ParamChar + "paramValueString";
        }

        if (command == "") return false;
        if (insPlanPreference.ValueString == null) insPlanPreference.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(insPlanPreference.ValueString));
        command = "UPDATE insplanpreference SET " + command
                                                  + " WHERE InsPlanPrefNum = " + SOut.Long(insPlanPreference.InsPlanPrefNum);
        Db.NonQ(command, paramValueString);
        return true;
    }

    public static bool UpdateComparison(InsPlanPreference insPlanPreference, InsPlanPreference oldInsPlanPreference)
    {
        if (insPlanPreference.PlanNum != oldInsPlanPreference.PlanNum) return true;
        if (insPlanPreference.FKey != oldInsPlanPreference.FKey) return true;
        if (insPlanPreference.FKeyType != oldInsPlanPreference.FKeyType) return true;
        if (insPlanPreference.ValueString != oldInsPlanPreference.ValueString) return true;
        return false;
    }

    public static void Delete(long insPlanPrefNum)
    {
        var command = "DELETE FROM insplanpreference "
                      + "WHERE InsPlanPrefNum = " + SOut.Long(insPlanPrefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsPlanPrefNums)
    {
        if (listInsPlanPrefNums == null || listInsPlanPrefNums.Count == 0) return;
        var command = "DELETE FROM insplanpreference "
                      + "WHERE InsPlanPrefNum IN(" + string.Join(",", listInsPlanPrefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}