#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ICD9Crud
{
    public static ICD9 SelectOne(long iCD9Num)
    {
        var command = "SELECT * FROM icd9 "
                      + "WHERE ICD9Num = " + SOut.Long(iCD9Num);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ICD9 SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ICD9> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ICD9> TableToList(DataTable table)
    {
        var retVal = new List<ICD9>();
        ICD9 iCD9;
        foreach (DataRow row in table.Rows)
        {
            iCD9 = new ICD9();
            iCD9.ICD9Num = SIn.Long(row["ICD9Num"].ToString());
            iCD9.ICD9Code = SIn.String(row["ICD9Code"].ToString());
            iCD9.Description = SIn.String(row["Description"].ToString());
            iCD9.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            retVal.Add(iCD9);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ICD9> listICD9s, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ICD9";
        var table = new DataTable(tableName);
        table.Columns.Add("ICD9Num");
        table.Columns.Add("ICD9Code");
        table.Columns.Add("Description");
        table.Columns.Add("DateTStamp");
        foreach (var iCD9 in listICD9s)
            table.Rows.Add(SOut.Long(iCD9.ICD9Num), iCD9.ICD9Code, iCD9.Description, SOut.DateT(iCD9.DateTStamp, false));
        return table;
    }

    public static long Insert(ICD9 iCD9)
    {
        return Insert(iCD9, false);
    }

    public static long Insert(ICD9 iCD9, bool useExistingPK)
    {
        var command = "INSERT INTO icd9 (";

        command += "ICD9Code,Description) VALUES(";

        command +=
            "'" + SOut.String(iCD9.ICD9Code) + "',"
            + "'" + SOut.String(iCD9.Description) + "')";
        //DateTStamp can only be set by MySQL

        iCD9.ICD9Num = Db.NonQ(command, true, "ICD9Num", "iCD9");
        return iCD9.ICD9Num;
    }

    public static long InsertNoCache(ICD9 iCD9)
    {
        return InsertNoCache(iCD9, false);
    }

    public static long InsertNoCache(ICD9 iCD9, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO icd9 (";
        if (isRandomKeys || useExistingPK) command += "ICD9Num,";
        command += "ICD9Code,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(iCD9.ICD9Num) + ",";
        command +=
            "'" + SOut.String(iCD9.ICD9Code) + "',"
            + "'" + SOut.String(iCD9.Description) + "')";
        //DateTStamp can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            iCD9.ICD9Num = Db.NonQ(command, true, "ICD9Num", "iCD9");
        return iCD9.ICD9Num;
    }

    public static void Update(ICD9 iCD9)
    {
        var command = "UPDATE icd9 SET "
                      + "ICD9Code   = '" + SOut.String(iCD9.ICD9Code) + "', "
                      + "Description= '" + SOut.String(iCD9.Description) + "' "
                      //DateTStamp can only be set by MySQL
                      + "WHERE ICD9Num = " + SOut.Long(iCD9.ICD9Num);
        Db.NonQ(command);
    }

    public static bool Update(ICD9 iCD9, ICD9 oldICD9)
    {
        var command = "";
        if (iCD9.ICD9Code != oldICD9.ICD9Code)
        {
            if (command != "") command += ",";
            command += "ICD9Code = '" + SOut.String(iCD9.ICD9Code) + "'";
        }

        if (iCD9.Description != oldICD9.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(iCD9.Description) + "'";
        }

        //DateTStamp can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE icd9 SET " + command
                                     + " WHERE ICD9Num = " + SOut.Long(iCD9.ICD9Num);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ICD9 iCD9, ICD9 oldICD9)
    {
        if (iCD9.ICD9Code != oldICD9.ICD9Code) return true;
        if (iCD9.Description != oldICD9.Description) return true;
        //DateTStamp can only be set by MySQL
        return false;
    }

    public static void Delete(long iCD9Num)
    {
        var command = "DELETE FROM icd9 "
                      + "WHERE ICD9Num = " + SOut.Long(iCD9Num);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listICD9Nums)
    {
        if (listICD9Nums == null || listICD9Nums.Count == 0) return;
        var command = "DELETE FROM icd9 "
                      + "WHERE ICD9Num IN(" + string.Join(",", listICD9Nums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}