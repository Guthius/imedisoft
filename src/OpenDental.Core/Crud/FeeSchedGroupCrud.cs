#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FeeSchedGroupCrud
{
    public static FeeSchedGroup SelectOne(long feeSchedGroupNum)
    {
        var command = "SELECT * FROM feeschedgroup "
                      + "WHERE FeeSchedGroupNum = " + SOut.Long(feeSchedGroupNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static FeeSchedGroup SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FeeSchedGroup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FeeSchedGroup> TableToList(DataTable table)
    {
        var retVal = new List<FeeSchedGroup>();
        FeeSchedGroup feeSchedGroup;
        foreach (DataRow row in table.Rows)
        {
            feeSchedGroup = new FeeSchedGroup();
            feeSchedGroup.FeeSchedGroupNum = SIn.Long(row["FeeSchedGroupNum"].ToString());
            feeSchedGroup.Description = SIn.String(row["Description"].ToString());
            feeSchedGroup.FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString());
            feeSchedGroup.ClinicNums = SIn.String(row["ClinicNums"].ToString());
            retVal.Add(feeSchedGroup);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FeeSchedGroup> listFeeSchedGroups, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FeeSchedGroup";
        var table = new DataTable(tableName);
        table.Columns.Add("FeeSchedGroupNum");
        table.Columns.Add("Description");
        table.Columns.Add("FeeSchedNum");
        table.Columns.Add("ClinicNums");
        foreach (var feeSchedGroup in listFeeSchedGroups)
            table.Rows.Add(SOut.Long(feeSchedGroup.FeeSchedGroupNum), feeSchedGroup.Description, SOut.Long(feeSchedGroup.FeeSchedNum), feeSchedGroup.ClinicNums);
        return table;
    }

    public static long Insert(FeeSchedGroup feeSchedGroup)
    {
        return Insert(feeSchedGroup, false);
    }

    public static long Insert(FeeSchedGroup feeSchedGroup, bool useExistingPK)
    {
        var command = "INSERT INTO feeschedgroup (";

        command += "Description,FeeSchedNum,ClinicNums) VALUES(";

        command +=
            "'" + SOut.String(feeSchedGroup.Description) + "',"
            + SOut.Long(feeSchedGroup.FeeSchedNum) + ","
            + "'" + SOut.String(feeSchedGroup.ClinicNums) + "')";
        {
            feeSchedGroup.FeeSchedGroupNum = Db.NonQ(command, true, "FeeSchedGroupNum", "feeSchedGroup");
        }
        return feeSchedGroup.FeeSchedGroupNum;
    }

    public static long InsertNoCache(FeeSchedGroup feeSchedGroup)
    {
        return InsertNoCache(feeSchedGroup, false);
    }

    public static long InsertNoCache(FeeSchedGroup feeSchedGroup, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO feeschedgroup (";
        if (isRandomKeys || useExistingPK) command += "FeeSchedGroupNum,";
        command += "Description,FeeSchedNum,ClinicNums) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(feeSchedGroup.FeeSchedGroupNum) + ",";
        command +=
            "'" + SOut.String(feeSchedGroup.Description) + "',"
            + SOut.Long(feeSchedGroup.FeeSchedNum) + ","
            + "'" + SOut.String(feeSchedGroup.ClinicNums) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            feeSchedGroup.FeeSchedGroupNum = Db.NonQ(command, true, "FeeSchedGroupNum", "feeSchedGroup");
        return feeSchedGroup.FeeSchedGroupNum;
    }

    public static void Update(FeeSchedGroup feeSchedGroup)
    {
        var command = "UPDATE feeschedgroup SET "
                      + "Description     = '" + SOut.String(feeSchedGroup.Description) + "', "
                      + "FeeSchedNum     =  " + SOut.Long(feeSchedGroup.FeeSchedNum) + ", "
                      + "ClinicNums      = '" + SOut.String(feeSchedGroup.ClinicNums) + "' "
                      + "WHERE FeeSchedGroupNum = " + SOut.Long(feeSchedGroup.FeeSchedGroupNum);
        Db.NonQ(command);
    }

    public static bool Update(FeeSchedGroup feeSchedGroup, FeeSchedGroup oldFeeSchedGroup)
    {
        var command = "";
        if (feeSchedGroup.Description != oldFeeSchedGroup.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(feeSchedGroup.Description) + "'";
        }

        if (feeSchedGroup.FeeSchedNum != oldFeeSchedGroup.FeeSchedNum)
        {
            if (command != "") command += ",";
            command += "FeeSchedNum = " + SOut.Long(feeSchedGroup.FeeSchedNum) + "";
        }

        if (feeSchedGroup.ClinicNums != oldFeeSchedGroup.ClinicNums)
        {
            if (command != "") command += ",";
            command += "ClinicNums = '" + SOut.String(feeSchedGroup.ClinicNums) + "'";
        }

        if (command == "") return false;
        command = "UPDATE feeschedgroup SET " + command
                                              + " WHERE FeeSchedGroupNum = " + SOut.Long(feeSchedGroup.FeeSchedGroupNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(FeeSchedGroup feeSchedGroup, FeeSchedGroup oldFeeSchedGroup)
    {
        if (feeSchedGroup.Description != oldFeeSchedGroup.Description) return true;
        if (feeSchedGroup.FeeSchedNum != oldFeeSchedGroup.FeeSchedNum) return true;
        if (feeSchedGroup.ClinicNums != oldFeeSchedGroup.ClinicNums) return true;
        return false;
    }

    public static void Delete(long feeSchedGroupNum)
    {
        var command = "DELETE FROM feeschedgroup "
                      + "WHERE FeeSchedGroupNum = " + SOut.Long(feeSchedGroupNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFeeSchedGroupNums)
    {
        if (listFeeSchedGroupNums == null || listFeeSchedGroupNums.Count == 0) return;
        var command = "DELETE FROM feeschedgroup "
                      + "WHERE FeeSchedGroupNum IN(" + string.Join(",", listFeeSchedGroupNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}