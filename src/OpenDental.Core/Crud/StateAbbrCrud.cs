#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class StateAbbrCrud
{
    public static StateAbbr SelectOne(long stateAbbrNum)
    {
        var command = "SELECT * FROM stateabbr "
                      + "WHERE StateAbbrNum = " + SOut.Long(stateAbbrNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static StateAbbr SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<StateAbbr> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<StateAbbr> TableToList(DataTable table)
    {
        var retVal = new List<StateAbbr>();
        StateAbbr stateAbbr;
        foreach (DataRow row in table.Rows)
        {
            stateAbbr = new StateAbbr();
            stateAbbr.StateAbbrNum = SIn.Long(row["StateAbbrNum"].ToString());
            stateAbbr.Description = SIn.String(row["Description"].ToString());
            stateAbbr.Abbr = SIn.String(row["Abbr"].ToString());
            stateAbbr.MedicaidIDLength = SIn.Int(row["MedicaidIDLength"].ToString());
            retVal.Add(stateAbbr);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<StateAbbr> listStateAbbrs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "StateAbbr";
        var table = new DataTable(tableName);
        table.Columns.Add("StateAbbrNum");
        table.Columns.Add("Description");
        table.Columns.Add("Abbr");
        table.Columns.Add("MedicaidIDLength");
        foreach (var stateAbbr in listStateAbbrs)
            table.Rows.Add(SOut.Long(stateAbbr.StateAbbrNum), stateAbbr.Description, stateAbbr.Abbr, SOut.Int(stateAbbr.MedicaidIDLength));
        return table;
    }

    public static long Insert(StateAbbr stateAbbr)
    {
        return Insert(stateAbbr, false);
    }

    public static long Insert(StateAbbr stateAbbr, bool useExistingPK)
    {
        var command = "INSERT INTO stateabbr (";

        command += "Description,Abbr,MedicaidIDLength) VALUES(";

        command +=
            "'" + SOut.String(stateAbbr.Description) + "',"
            + "'" + SOut.String(stateAbbr.Abbr) + "',"
            + SOut.Int(stateAbbr.MedicaidIDLength) + ")";
        {
            stateAbbr.StateAbbrNum = Db.NonQ(command, true, "StateAbbrNum", "stateAbbr");
        }
        return stateAbbr.StateAbbrNum;
    }

    public static long InsertNoCache(StateAbbr stateAbbr)
    {
        return InsertNoCache(stateAbbr, false);
    }

    public static long InsertNoCache(StateAbbr stateAbbr, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO stateabbr (";
        if (isRandomKeys || useExistingPK) command += "StateAbbrNum,";
        command += "Description,Abbr,MedicaidIDLength) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(stateAbbr.StateAbbrNum) + ",";
        command +=
            "'" + SOut.String(stateAbbr.Description) + "',"
            + "'" + SOut.String(stateAbbr.Abbr) + "',"
            + SOut.Int(stateAbbr.MedicaidIDLength) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            stateAbbr.StateAbbrNum = Db.NonQ(command, true, "StateAbbrNum", "stateAbbr");
        return stateAbbr.StateAbbrNum;
    }

    public static void Update(StateAbbr stateAbbr)
    {
        var command = "UPDATE stateabbr SET "
                      + "Description     = '" + SOut.String(stateAbbr.Description) + "', "
                      + "Abbr            = '" + SOut.String(stateAbbr.Abbr) + "', "
                      + "MedicaidIDLength=  " + SOut.Int(stateAbbr.MedicaidIDLength) + " "
                      + "WHERE StateAbbrNum = " + SOut.Long(stateAbbr.StateAbbrNum);
        Db.NonQ(command);
    }

    public static bool Update(StateAbbr stateAbbr, StateAbbr oldStateAbbr)
    {
        var command = "";
        if (stateAbbr.Description != oldStateAbbr.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(stateAbbr.Description) + "'";
        }

        if (stateAbbr.Abbr != oldStateAbbr.Abbr)
        {
            if (command != "") command += ",";
            command += "Abbr = '" + SOut.String(stateAbbr.Abbr) + "'";
        }

        if (stateAbbr.MedicaidIDLength != oldStateAbbr.MedicaidIDLength)
        {
            if (command != "") command += ",";
            command += "MedicaidIDLength = " + SOut.Int(stateAbbr.MedicaidIDLength) + "";
        }

        if (command == "") return false;
        command = "UPDATE stateabbr SET " + command
                                          + " WHERE StateAbbrNum = " + SOut.Long(stateAbbr.StateAbbrNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(StateAbbr stateAbbr, StateAbbr oldStateAbbr)
    {
        if (stateAbbr.Description != oldStateAbbr.Description) return true;
        if (stateAbbr.Abbr != oldStateAbbr.Abbr) return true;
        if (stateAbbr.MedicaidIDLength != oldStateAbbr.MedicaidIDLength) return true;
        return false;
    }

    public static void Delete(long stateAbbrNum)
    {
        var command = "DELETE FROM stateabbr "
                      + "WHERE StateAbbrNum = " + SOut.Long(stateAbbrNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listStateAbbrNums)
    {
        if (listStateAbbrNums == null || listStateAbbrNums.Count == 0) return;
        var command = "DELETE FROM stateabbr "
                      + "WHERE StateAbbrNum IN(" + string.Join(",", listStateAbbrNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}