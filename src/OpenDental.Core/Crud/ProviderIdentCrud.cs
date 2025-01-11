#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProviderIdentCrud
{
    public static ProviderIdent SelectOne(long providerIdentNum)
    {
        var command = "SELECT * FROM providerident "
                      + "WHERE ProviderIdentNum = " + SOut.Long(providerIdentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProviderIdent SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProviderIdent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProviderIdent> TableToList(DataTable table)
    {
        var retVal = new List<ProviderIdent>();
        ProviderIdent providerIdent;
        foreach (DataRow row in table.Rows)
        {
            providerIdent = new ProviderIdent();
            providerIdent.ProviderIdentNum = SIn.Long(row["ProviderIdentNum"].ToString());
            providerIdent.ProvNum = SIn.Long(row["ProvNum"].ToString());
            providerIdent.PayorID = SIn.String(row["PayorID"].ToString());
            providerIdent.SuppIDType = (ProviderSupplementalID) SIn.Int(row["SuppIDType"].ToString());
            providerIdent.IDNumber = SIn.String(row["IDNumber"].ToString());
            retVal.Add(providerIdent);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProviderIdent> listProviderIdents, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProviderIdent";
        var table = new DataTable(tableName);
        table.Columns.Add("ProviderIdentNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("PayorID");
        table.Columns.Add("SuppIDType");
        table.Columns.Add("IDNumber");
        foreach (var providerIdent in listProviderIdents)
            table.Rows.Add(SOut.Long(providerIdent.ProviderIdentNum), SOut.Long(providerIdent.ProvNum), providerIdent.PayorID, SOut.Int((int) providerIdent.SuppIDType), providerIdent.IDNumber);
        return table;
    }

    public static long Insert(ProviderIdent providerIdent)
    {
        return Insert(providerIdent, false);
    }

    public static long Insert(ProviderIdent providerIdent, bool useExistingPK)
    {
        var command = "INSERT INTO providerident (";

        command += "ProvNum,PayorID,SuppIDType,IDNumber) VALUES(";

        command +=
            SOut.Long(providerIdent.ProvNum) + ","
                                             + "'" + SOut.String(providerIdent.PayorID) + "',"
                                             + SOut.Int((int) providerIdent.SuppIDType) + ","
                                             + "'" + SOut.String(providerIdent.IDNumber) + "')";
        {
            providerIdent.ProviderIdentNum = Db.NonQ(command, true, "ProviderIdentNum", "providerIdent");
        }
        return providerIdent.ProviderIdentNum;
    }

    public static long InsertNoCache(ProviderIdent providerIdent)
    {
        return InsertNoCache(providerIdent, false);
    }

    public static long InsertNoCache(ProviderIdent providerIdent, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO providerident (";
        if (isRandomKeys || useExistingPK) command += "ProviderIdentNum,";
        command += "ProvNum,PayorID,SuppIDType,IDNumber) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(providerIdent.ProviderIdentNum) + ",";
        command +=
            SOut.Long(providerIdent.ProvNum) + ","
                                             + "'" + SOut.String(providerIdent.PayorID) + "',"
                                             + SOut.Int((int) providerIdent.SuppIDType) + ","
                                             + "'" + SOut.String(providerIdent.IDNumber) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            providerIdent.ProviderIdentNum = Db.NonQ(command, true, "ProviderIdentNum", "providerIdent");
        return providerIdent.ProviderIdentNum;
    }

    public static void Update(ProviderIdent providerIdent)
    {
        var command = "UPDATE providerident SET "
                      + "ProvNum         =  " + SOut.Long(providerIdent.ProvNum) + ", "
                      + "PayorID         = '" + SOut.String(providerIdent.PayorID) + "', "
                      + "SuppIDType      =  " + SOut.Int((int) providerIdent.SuppIDType) + ", "
                      + "IDNumber        = '" + SOut.String(providerIdent.IDNumber) + "' "
                      + "WHERE ProviderIdentNum = " + SOut.Long(providerIdent.ProviderIdentNum);
        Db.NonQ(command);
    }

    public static bool Update(ProviderIdent providerIdent, ProviderIdent oldProviderIdent)
    {
        var command = "";
        if (providerIdent.ProvNum != oldProviderIdent.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(providerIdent.ProvNum) + "";
        }

        if (providerIdent.PayorID != oldProviderIdent.PayorID)
        {
            if (command != "") command += ",";
            command += "PayorID = '" + SOut.String(providerIdent.PayorID) + "'";
        }

        if (providerIdent.SuppIDType != oldProviderIdent.SuppIDType)
        {
            if (command != "") command += ",";
            command += "SuppIDType = " + SOut.Int((int) providerIdent.SuppIDType) + "";
        }

        if (providerIdent.IDNumber != oldProviderIdent.IDNumber)
        {
            if (command != "") command += ",";
            command += "IDNumber = '" + SOut.String(providerIdent.IDNumber) + "'";
        }

        if (command == "") return false;
        command = "UPDATE providerident SET " + command
                                              + " WHERE ProviderIdentNum = " + SOut.Long(providerIdent.ProviderIdentNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProviderIdent providerIdent, ProviderIdent oldProviderIdent)
    {
        if (providerIdent.ProvNum != oldProviderIdent.ProvNum) return true;
        if (providerIdent.PayorID != oldProviderIdent.PayorID) return true;
        if (providerIdent.SuppIDType != oldProviderIdent.SuppIDType) return true;
        if (providerIdent.IDNumber != oldProviderIdent.IDNumber) return true;
        return false;
    }

    public static void Delete(long providerIdentNum)
    {
        var command = "DELETE FROM providerident "
                      + "WHERE ProviderIdentNum = " + SOut.Long(providerIdentNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProviderIdentNums)
    {
        if (listProviderIdentNums == null || listProviderIdentNums.Count == 0) return;
        var command = "DELETE FROM providerident "
                      + "WHERE ProviderIdentNum IN(" + string.Join(",", listProviderIdentNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}