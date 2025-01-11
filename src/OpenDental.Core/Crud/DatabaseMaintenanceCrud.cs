#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DatabaseMaintenanceCrud
{
    public static DatabaseMaintenance SelectOne(long databaseMaintenanceNum)
    {
        var command = "SELECT * FROM databasemaintenance "
                      + "WHERE DatabaseMaintenanceNum = " + SOut.Long(databaseMaintenanceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DatabaseMaintenance SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DatabaseMaintenance> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DatabaseMaintenance> TableToList(DataTable table)
    {
        var retVal = new List<DatabaseMaintenance>();
        DatabaseMaintenance databaseMaintenance;
        foreach (DataRow row in table.Rows)
        {
            databaseMaintenance = new DatabaseMaintenance();
            databaseMaintenance.DatabaseMaintenanceNum = SIn.Long(row["DatabaseMaintenanceNum"].ToString());
            databaseMaintenance.MethodName = SIn.String(row["MethodName"].ToString());
            databaseMaintenance.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            databaseMaintenance.IsOld = SIn.Bool(row["IsOld"].ToString());
            databaseMaintenance.DateLastRun = SIn.DateTime(row["DateLastRun"].ToString());
            retVal.Add(databaseMaintenance);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DatabaseMaintenance> listDatabaseMaintenances, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DatabaseMaintenance";
        var table = new DataTable(tableName);
        table.Columns.Add("DatabaseMaintenanceNum");
        table.Columns.Add("MethodName");
        table.Columns.Add("IsHidden");
        table.Columns.Add("IsOld");
        table.Columns.Add("DateLastRun");
        foreach (var databaseMaintenance in listDatabaseMaintenances)
            table.Rows.Add(SOut.Long(databaseMaintenance.DatabaseMaintenanceNum), databaseMaintenance.MethodName, SOut.Bool(databaseMaintenance.IsHidden), SOut.Bool(databaseMaintenance.IsOld), SOut.DateT(databaseMaintenance.DateLastRun, false));
        return table;
    }

    public static long Insert(DatabaseMaintenance databaseMaintenance)
    {
        return Insert(databaseMaintenance, false);
    }

    public static long Insert(DatabaseMaintenance databaseMaintenance, bool useExistingPK)
    {
        var command = "INSERT INTO databasemaintenance (";

        command += "MethodName,IsHidden,IsOld,DateLastRun) VALUES(";

        command +=
            "'" + SOut.String(databaseMaintenance.MethodName) + "',"
            + SOut.Bool(databaseMaintenance.IsHidden) + ","
            + SOut.Bool(databaseMaintenance.IsOld) + ","
            + SOut.DateT(databaseMaintenance.DateLastRun) + ")";
        {
            databaseMaintenance.DatabaseMaintenanceNum = Db.NonQ(command, true, "DatabaseMaintenanceNum", "databaseMaintenance");
        }
        return databaseMaintenance.DatabaseMaintenanceNum;
    }

    public static long InsertNoCache(DatabaseMaintenance databaseMaintenance)
    {
        return InsertNoCache(databaseMaintenance, false);
    }

    public static long InsertNoCache(DatabaseMaintenance databaseMaintenance, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO databasemaintenance (";
        if (isRandomKeys || useExistingPK) command += "DatabaseMaintenanceNum,";
        command += "MethodName,IsHidden,IsOld,DateLastRun) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(databaseMaintenance.DatabaseMaintenanceNum) + ",";
        command +=
            "'" + SOut.String(databaseMaintenance.MethodName) + "',"
            + SOut.Bool(databaseMaintenance.IsHidden) + ","
            + SOut.Bool(databaseMaintenance.IsOld) + ","
            + SOut.DateT(databaseMaintenance.DateLastRun) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            databaseMaintenance.DatabaseMaintenanceNum = Db.NonQ(command, true, "DatabaseMaintenanceNum", "databaseMaintenance");
        return databaseMaintenance.DatabaseMaintenanceNum;
    }

    public static void Update(DatabaseMaintenance databaseMaintenance)
    {
        var command = "UPDATE databasemaintenance SET "
                      + "MethodName            = '" + SOut.String(databaseMaintenance.MethodName) + "', "
                      + "IsHidden              =  " + SOut.Bool(databaseMaintenance.IsHidden) + ", "
                      + "IsOld                 =  " + SOut.Bool(databaseMaintenance.IsOld) + ", "
                      + "DateLastRun           =  " + SOut.DateT(databaseMaintenance.DateLastRun) + " "
                      + "WHERE DatabaseMaintenanceNum = " + SOut.Long(databaseMaintenance.DatabaseMaintenanceNum);
        Db.NonQ(command);
    }

    public static bool Update(DatabaseMaintenance databaseMaintenance, DatabaseMaintenance oldDatabaseMaintenance)
    {
        var command = "";
        if (databaseMaintenance.MethodName != oldDatabaseMaintenance.MethodName)
        {
            if (command != "") command += ",";
            command += "MethodName = '" + SOut.String(databaseMaintenance.MethodName) + "'";
        }

        if (databaseMaintenance.IsHidden != oldDatabaseMaintenance.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(databaseMaintenance.IsHidden) + "";
        }

        if (databaseMaintenance.IsOld != oldDatabaseMaintenance.IsOld)
        {
            if (command != "") command += ",";
            command += "IsOld = " + SOut.Bool(databaseMaintenance.IsOld) + "";
        }

        if (databaseMaintenance.DateLastRun != oldDatabaseMaintenance.DateLastRun)
        {
            if (command != "") command += ",";
            command += "DateLastRun = " + SOut.DateT(databaseMaintenance.DateLastRun) + "";
        }

        if (command == "") return false;
        command = "UPDATE databasemaintenance SET " + command
                                                    + " WHERE DatabaseMaintenanceNum = " + SOut.Long(databaseMaintenance.DatabaseMaintenanceNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DatabaseMaintenance databaseMaintenance, DatabaseMaintenance oldDatabaseMaintenance)
    {
        if (databaseMaintenance.MethodName != oldDatabaseMaintenance.MethodName) return true;
        if (databaseMaintenance.IsHidden != oldDatabaseMaintenance.IsHidden) return true;
        if (databaseMaintenance.IsOld != oldDatabaseMaintenance.IsOld) return true;
        if (databaseMaintenance.DateLastRun != oldDatabaseMaintenance.DateLastRun) return true;
        return false;
    }

    public static void Delete(long databaseMaintenanceNum)
    {
        var command = "DELETE FROM databasemaintenance "
                      + "WHERE DatabaseMaintenanceNum = " + SOut.Long(databaseMaintenanceNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDatabaseMaintenanceNums)
    {
        if (listDatabaseMaintenanceNums == null || listDatabaseMaintenanceNums.Count == 0) return;
        var command = "DELETE FROM databasemaintenance "
                      + "WHERE DatabaseMaintenanceNum IN(" + string.Join(",", listDatabaseMaintenanceNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}