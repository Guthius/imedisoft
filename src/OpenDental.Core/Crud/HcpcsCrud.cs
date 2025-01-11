#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HcpcsCrud
{
    public static Hcpcs SelectOne(long hcpcsNum)
    {
        var command = "SELECT * FROM hcpcs "
                      + "WHERE HcpcsNum = " + SOut.Long(hcpcsNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Hcpcs SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Hcpcs> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Hcpcs> TableToList(DataTable table)
    {
        var retVal = new List<Hcpcs>();
        Hcpcs hcpcs;
        foreach (DataRow row in table.Rows)
        {
            hcpcs = new Hcpcs();
            hcpcs.HcpcsNum = SIn.Long(row["HcpcsNum"].ToString());
            hcpcs.HcpcsCode = SIn.String(row["HcpcsCode"].ToString());
            hcpcs.DescriptionShort = SIn.String(row["DescriptionShort"].ToString());
            retVal.Add(hcpcs);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Hcpcs> listHcpcss, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Hcpcs";
        var table = new DataTable(tableName);
        table.Columns.Add("HcpcsNum");
        table.Columns.Add("HcpcsCode");
        table.Columns.Add("DescriptionShort");
        foreach (var hcpcs in listHcpcss)
            table.Rows.Add(SOut.Long(hcpcs.HcpcsNum), hcpcs.HcpcsCode, hcpcs.DescriptionShort);
        return table;
    }

    public static long Insert(Hcpcs hcpcs)
    {
        return Insert(hcpcs, false);
    }

    public static long Insert(Hcpcs hcpcs, bool useExistingPK)
    {
        var command = "INSERT INTO hcpcs (";

        command += "HcpcsCode,DescriptionShort) VALUES(";

        command +=
            "'" + SOut.String(hcpcs.HcpcsCode) + "',"
            + "'" + SOut.String(hcpcs.DescriptionShort) + "')";
        {
            hcpcs.HcpcsNum = Db.NonQ(command, true, "HcpcsNum", "hcpcs");
        }
        return hcpcs.HcpcsNum;
    }

    public static long InsertNoCache(Hcpcs hcpcs)
    {
        return InsertNoCache(hcpcs, false);
    }

    public static long InsertNoCache(Hcpcs hcpcs, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hcpcs (";
        if (isRandomKeys || useExistingPK) command += "HcpcsNum,";
        command += "HcpcsCode,DescriptionShort) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hcpcs.HcpcsNum) + ",";
        command +=
            "'" + SOut.String(hcpcs.HcpcsCode) + "',"
            + "'" + SOut.String(hcpcs.DescriptionShort) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            hcpcs.HcpcsNum = Db.NonQ(command, true, "HcpcsNum", "hcpcs");
        return hcpcs.HcpcsNum;
    }

    public static void Update(Hcpcs hcpcs)
    {
        var command = "UPDATE hcpcs SET "
                      + "HcpcsCode       = '" + SOut.String(hcpcs.HcpcsCode) + "', "
                      + "DescriptionShort= '" + SOut.String(hcpcs.DescriptionShort) + "' "
                      + "WHERE HcpcsNum = " + SOut.Long(hcpcs.HcpcsNum);
        Db.NonQ(command);
    }

    public static bool Update(Hcpcs hcpcs, Hcpcs oldHcpcs)
    {
        var command = "";
        if (hcpcs.HcpcsCode != oldHcpcs.HcpcsCode)
        {
            if (command != "") command += ",";
            command += "HcpcsCode = '" + SOut.String(hcpcs.HcpcsCode) + "'";
        }

        if (hcpcs.DescriptionShort != oldHcpcs.DescriptionShort)
        {
            if (command != "") command += ",";
            command += "DescriptionShort = '" + SOut.String(hcpcs.DescriptionShort) + "'";
        }

        if (command == "") return false;
        command = "UPDATE hcpcs SET " + command
                                      + " WHERE HcpcsNum = " + SOut.Long(hcpcs.HcpcsNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Hcpcs hcpcs, Hcpcs oldHcpcs)
    {
        if (hcpcs.HcpcsCode != oldHcpcs.HcpcsCode) return true;
        if (hcpcs.DescriptionShort != oldHcpcs.DescriptionShort) return true;
        return false;
    }

    public static void Delete(long hcpcsNum)
    {
        var command = "DELETE FROM hcpcs "
                      + "WHERE HcpcsNum = " + SOut.Long(hcpcsNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHcpcsNums)
    {
        if (listHcpcsNums == null || listHcpcsNums.Count == 0) return;
        var command = "DELETE FROM hcpcs "
                      + "WHERE HcpcsNum IN(" + string.Join(",", listHcpcsNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}