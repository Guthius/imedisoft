#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class Icd10Crud
{
    public static Icd10 SelectOne(long icd10Num)
    {
        var command = "SELECT * FROM icd10 "
                      + "WHERE Icd10Num = " + SOut.Long(icd10Num);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Icd10 SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Icd10> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Icd10> TableToList(DataTable table)
    {
        var retVal = new List<Icd10>();
        Icd10 icd10;
        foreach (DataRow row in table.Rows)
        {
            icd10 = new Icd10();
            icd10.Icd10Num = SIn.Long(row["Icd10Num"].ToString());
            icd10.Icd10Code = SIn.String(row["Icd10Code"].ToString());
            icd10.Description = SIn.String(row["Description"].ToString());
            icd10.IsCode = SIn.String(row["IsCode"].ToString());
            retVal.Add(icd10);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Icd10> listIcd10s, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Icd10";
        var table = new DataTable(tableName);
        table.Columns.Add("Icd10Num");
        table.Columns.Add("Icd10Code");
        table.Columns.Add("Description");
        table.Columns.Add("IsCode");
        foreach (var icd10 in listIcd10s)
            table.Rows.Add(SOut.Long(icd10.Icd10Num), icd10.Icd10Code, icd10.Description, icd10.IsCode);
        return table;
    }

    public static long Insert(Icd10 icd10)
    {
        return Insert(icd10, false);
    }

    public static long Insert(Icd10 icd10, bool useExistingPK)
    {
        var command = "INSERT INTO icd10 (";

        command += "Icd10Code,Description,IsCode) VALUES(";

        command +=
            "'" + SOut.String(icd10.Icd10Code) + "',"
            + "'" + SOut.String(icd10.Description) + "',"
            + "'" + SOut.String(icd10.IsCode) + "')";
        {
            icd10.Icd10Num = Db.NonQ(command, true, "Icd10Num", "icd10");
        }
        return icd10.Icd10Num;
    }

    public static long InsertNoCache(Icd10 icd10)
    {
        return InsertNoCache(icd10, false);
    }

    public static long InsertNoCache(Icd10 icd10, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO icd10 (";
        if (isRandomKeys || useExistingPK) command += "Icd10Num,";
        command += "Icd10Code,Description,IsCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(icd10.Icd10Num) + ",";
        command +=
            "'" + SOut.String(icd10.Icd10Code) + "',"
            + "'" + SOut.String(icd10.Description) + "',"
            + "'" + SOut.String(icd10.IsCode) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            icd10.Icd10Num = Db.NonQ(command, true, "Icd10Num", "icd10");
        return icd10.Icd10Num;
    }

    public static void Update(Icd10 icd10)
    {
        var command = "UPDATE icd10 SET "
                      + "Icd10Code  = '" + SOut.String(icd10.Icd10Code) + "', "
                      + "Description= '" + SOut.String(icd10.Description) + "', "
                      + "IsCode     = '" + SOut.String(icd10.IsCode) + "' "
                      + "WHERE Icd10Num = " + SOut.Long(icd10.Icd10Num);
        Db.NonQ(command);
    }

    public static bool Update(Icd10 icd10, Icd10 oldIcd10)
    {
        var command = "";
        if (icd10.Icd10Code != oldIcd10.Icd10Code)
        {
            if (command != "") command += ",";
            command += "Icd10Code = '" + SOut.String(icd10.Icd10Code) + "'";
        }

        if (icd10.Description != oldIcd10.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(icd10.Description) + "'";
        }

        if (icd10.IsCode != oldIcd10.IsCode)
        {
            if (command != "") command += ",";
            command += "IsCode = '" + SOut.String(icd10.IsCode) + "'";
        }

        if (command == "") return false;
        command = "UPDATE icd10 SET " + command
                                      + " WHERE Icd10Num = " + SOut.Long(icd10.Icd10Num);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Icd10 icd10, Icd10 oldIcd10)
    {
        if (icd10.Icd10Code != oldIcd10.Icd10Code) return true;
        if (icd10.Description != oldIcd10.Description) return true;
        if (icd10.IsCode != oldIcd10.IsCode) return true;
        return false;
    }

    public static void Delete(long icd10Num)
    {
        var command = "DELETE FROM icd10 "
                      + "WHERE Icd10Num = " + SOut.Long(icd10Num);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listIcd10Nums)
    {
        if (listIcd10Nums == null || listIcd10Nums.Count == 0) return;
        var command = "DELETE FROM icd10 "
                      + "WHERE Icd10Num IN(" + string.Join(",", listIcd10Nums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}