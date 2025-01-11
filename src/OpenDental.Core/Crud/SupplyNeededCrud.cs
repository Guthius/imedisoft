#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SupplyNeededCrud
{
    public static SupplyNeeded SelectOne(long supplyNeededNum)
    {
        var command = "SELECT * FROM supplyneeded "
                      + "WHERE SupplyNeededNum = " + SOut.Long(supplyNeededNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SupplyNeeded SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SupplyNeeded> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SupplyNeeded> TableToList(DataTable table)
    {
        var retVal = new List<SupplyNeeded>();
        SupplyNeeded supplyNeeded;
        foreach (DataRow row in table.Rows)
        {
            supplyNeeded = new SupplyNeeded();
            supplyNeeded.SupplyNeededNum = SIn.Long(row["SupplyNeededNum"].ToString());
            supplyNeeded.Description = SIn.String(row["Description"].ToString());
            supplyNeeded.DateAdded = SIn.Date(row["DateAdded"].ToString());
            retVal.Add(supplyNeeded);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SupplyNeeded> listSupplyNeededs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SupplyNeeded";
        var table = new DataTable(tableName);
        table.Columns.Add("SupplyNeededNum");
        table.Columns.Add("Description");
        table.Columns.Add("DateAdded");
        foreach (var supplyNeeded in listSupplyNeededs)
            table.Rows.Add(SOut.Long(supplyNeeded.SupplyNeededNum), supplyNeeded.Description, SOut.DateT(supplyNeeded.DateAdded, false));
        return table;
    }

    public static long Insert(SupplyNeeded supplyNeeded)
    {
        return Insert(supplyNeeded, false);
    }

    public static long Insert(SupplyNeeded supplyNeeded, bool useExistingPK)
    {
        var command = "INSERT INTO supplyneeded (";

        command += "Description,DateAdded) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + SOut.Date(supplyNeeded.DateAdded) + ")";
        if (supplyNeeded.Description == null) supplyNeeded.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(supplyNeeded.Description));
        {
            supplyNeeded.SupplyNeededNum = Db.NonQ(command, true, "SupplyNeededNum", "supplyNeeded", paramDescription);
        }
        return supplyNeeded.SupplyNeededNum;
    }

    public static long InsertNoCache(SupplyNeeded supplyNeeded)
    {
        return InsertNoCache(supplyNeeded, false);
    }

    public static long InsertNoCache(SupplyNeeded supplyNeeded, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO supplyneeded (";
        if (isRandomKeys || useExistingPK) command += "SupplyNeededNum,";
        command += "Description,DateAdded) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(supplyNeeded.SupplyNeededNum) + ",";
        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + SOut.Date(supplyNeeded.DateAdded) + ")";
        if (supplyNeeded.Description == null) supplyNeeded.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(supplyNeeded.Description));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription);
        else
            supplyNeeded.SupplyNeededNum = Db.NonQ(command, true, "SupplyNeededNum", "supplyNeeded", paramDescription);
        return supplyNeeded.SupplyNeededNum;
    }

    public static void Update(SupplyNeeded supplyNeeded)
    {
        var command = "UPDATE supplyneeded SET "
                      + "Description    =  " + DbHelper.ParamChar + "paramDescription, "
                      + "DateAdded      =  " + SOut.Date(supplyNeeded.DateAdded) + " "
                      + "WHERE SupplyNeededNum = " + SOut.Long(supplyNeeded.SupplyNeededNum);
        if (supplyNeeded.Description == null) supplyNeeded.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(supplyNeeded.Description));
        Db.NonQ(command, paramDescription);
    }

    public static bool Update(SupplyNeeded supplyNeeded, SupplyNeeded oldSupplyNeeded)
    {
        var command = "";
        if (supplyNeeded.Description != oldSupplyNeeded.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (supplyNeeded.DateAdded.Date != oldSupplyNeeded.DateAdded.Date)
        {
            if (command != "") command += ",";
            command += "DateAdded = " + SOut.Date(supplyNeeded.DateAdded) + "";
        }

        if (command == "") return false;
        if (supplyNeeded.Description == null) supplyNeeded.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(supplyNeeded.Description));
        command = "UPDATE supplyneeded SET " + command
                                             + " WHERE SupplyNeededNum = " + SOut.Long(supplyNeeded.SupplyNeededNum);
        Db.NonQ(command, paramDescription);
        return true;
    }

    public static bool UpdateComparison(SupplyNeeded supplyNeeded, SupplyNeeded oldSupplyNeeded)
    {
        if (supplyNeeded.Description != oldSupplyNeeded.Description) return true;
        if (supplyNeeded.DateAdded.Date != oldSupplyNeeded.DateAdded.Date) return true;
        return false;
    }

    public static void Delete(long supplyNeededNum)
    {
        var command = "DELETE FROM supplyneeded "
                      + "WHERE SupplyNeededNum = " + SOut.Long(supplyNeededNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSupplyNeededNums)
    {
        if (listSupplyNeededNums == null || listSupplyNeededNums.Count == 0) return;
        var command = "DELETE FROM supplyneeded "
                      + "WHERE SupplyNeededNum IN(" + string.Join(",", listSupplyNeededNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}