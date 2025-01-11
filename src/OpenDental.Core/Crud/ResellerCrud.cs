#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ResellerCrud
{
    public static Reseller SelectOne(long resellerNum)
    {
        var command = "SELECT * FROM reseller "
                      + "WHERE ResellerNum = " + SOut.Long(resellerNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Reseller SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Reseller> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Reseller> TableToList(DataTable table)
    {
        var retVal = new List<Reseller>();
        Reseller reseller;
        foreach (DataRow row in table.Rows)
        {
            reseller = new Reseller();
            reseller.ResellerNum = SIn.Long(row["ResellerNum"].ToString());
            reseller.PatNum = SIn.Long(row["PatNum"].ToString());
            reseller.UserName = SIn.String(row["UserName"].ToString());
            reseller.ResellerPassword = SIn.String(row["ResellerPassword"].ToString());
            reseller.BillingType = SIn.Long(row["BillingType"].ToString());
            reseller.VotesAllotted = SIn.Int(row["VotesAllotted"].ToString());
            reseller.Note = SIn.String(row["Note"].ToString());
            reseller.AllowSignupPortal = SIn.Bool(row["AllowSignupPortal"].ToString());
            retVal.Add(reseller);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Reseller> listResellers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Reseller";
        var table = new DataTable(tableName);
        table.Columns.Add("ResellerNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("UserName");
        table.Columns.Add("ResellerPassword");
        table.Columns.Add("BillingType");
        table.Columns.Add("VotesAllotted");
        table.Columns.Add("Note");
        table.Columns.Add("AllowSignupPortal");
        foreach (var reseller in listResellers)
            table.Rows.Add(SOut.Long(reseller.ResellerNum), SOut.Long(reseller.PatNum), reseller.UserName, reseller.ResellerPassword, SOut.Long(reseller.BillingType), SOut.Int(reseller.VotesAllotted), reseller.Note, SOut.Bool(reseller.AllowSignupPortal));
        return table;
    }

    public static long Insert(Reseller reseller)
    {
        return Insert(reseller, false);
    }

    public static long Insert(Reseller reseller, bool useExistingPK)
    {
        var command = "INSERT INTO reseller (";

        command += "PatNum,UserName,ResellerPassword,BillingType,VotesAllotted,Note,AllowSignupPortal) VALUES(";

        command +=
            SOut.Long(reseller.PatNum) + ","
                                       + "'" + SOut.String(reseller.UserName) + "',"
                                       + "'" + SOut.String(reseller.ResellerPassword) + "',"
                                       + SOut.Long(reseller.BillingType) + ","
                                       + SOut.Int(reseller.VotesAllotted) + ","
                                       + "'" + SOut.String(reseller.Note) + "',"
                                       + SOut.Bool(reseller.AllowSignupPortal) + ")";
        {
            reseller.ResellerNum = Db.NonQ(command, true, "ResellerNum", "reseller");
        }
        return reseller.ResellerNum;
    }

    public static long InsertNoCache(Reseller reseller)
    {
        return InsertNoCache(reseller, false);
    }

    public static long InsertNoCache(Reseller reseller, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO reseller (";
        if (isRandomKeys || useExistingPK) command += "ResellerNum,";
        command += "PatNum,UserName,ResellerPassword,BillingType,VotesAllotted,Note,AllowSignupPortal) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(reseller.ResellerNum) + ",";
        command +=
            SOut.Long(reseller.PatNum) + ","
                                       + "'" + SOut.String(reseller.UserName) + "',"
                                       + "'" + SOut.String(reseller.ResellerPassword) + "',"
                                       + SOut.Long(reseller.BillingType) + ","
                                       + SOut.Int(reseller.VotesAllotted) + ","
                                       + "'" + SOut.String(reseller.Note) + "',"
                                       + SOut.Bool(reseller.AllowSignupPortal) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            reseller.ResellerNum = Db.NonQ(command, true, "ResellerNum", "reseller");
        return reseller.ResellerNum;
    }

    public static void Update(Reseller reseller)
    {
        var command = "UPDATE reseller SET "
                      + "PatNum           =  " + SOut.Long(reseller.PatNum) + ", "
                      + "UserName         = '" + SOut.String(reseller.UserName) + "', "
                      + "ResellerPassword = '" + SOut.String(reseller.ResellerPassword) + "', "
                      + "BillingType      =  " + SOut.Long(reseller.BillingType) + ", "
                      + "VotesAllotted    =  " + SOut.Int(reseller.VotesAllotted) + ", "
                      + "Note             = '" + SOut.String(reseller.Note) + "', "
                      + "AllowSignupPortal=  " + SOut.Bool(reseller.AllowSignupPortal) + " "
                      + "WHERE ResellerNum = " + SOut.Long(reseller.ResellerNum);
        Db.NonQ(command);
    }

    public static bool Update(Reseller reseller, Reseller oldReseller)
    {
        var command = "";
        if (reseller.PatNum != oldReseller.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(reseller.PatNum) + "";
        }

        if (reseller.UserName != oldReseller.UserName)
        {
            if (command != "") command += ",";
            command += "UserName = '" + SOut.String(reseller.UserName) + "'";
        }

        if (reseller.ResellerPassword != oldReseller.ResellerPassword)
        {
            if (command != "") command += ",";
            command += "ResellerPassword = '" + SOut.String(reseller.ResellerPassword) + "'";
        }

        if (reseller.BillingType != oldReseller.BillingType)
        {
            if (command != "") command += ",";
            command += "BillingType = " + SOut.Long(reseller.BillingType) + "";
        }

        if (reseller.VotesAllotted != oldReseller.VotesAllotted)
        {
            if (command != "") command += ",";
            command += "VotesAllotted = " + SOut.Int(reseller.VotesAllotted) + "";
        }

        if (reseller.Note != oldReseller.Note)
        {
            if (command != "") command += ",";
            command += "Note = '" + SOut.String(reseller.Note) + "'";
        }

        if (reseller.AllowSignupPortal != oldReseller.AllowSignupPortal)
        {
            if (command != "") command += ",";
            command += "AllowSignupPortal = " + SOut.Bool(reseller.AllowSignupPortal) + "";
        }

        if (command == "") return false;
        command = "UPDATE reseller SET " + command
                                         + " WHERE ResellerNum = " + SOut.Long(reseller.ResellerNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Reseller reseller, Reseller oldReseller)
    {
        if (reseller.PatNum != oldReseller.PatNum) return true;
        if (reseller.UserName != oldReseller.UserName) return true;
        if (reseller.ResellerPassword != oldReseller.ResellerPassword) return true;
        if (reseller.BillingType != oldReseller.BillingType) return true;
        if (reseller.VotesAllotted != oldReseller.VotesAllotted) return true;
        if (reseller.Note != oldReseller.Note) return true;
        if (reseller.AllowSignupPortal != oldReseller.AllowSignupPortal) return true;
        return false;
    }

    public static void Delete(long resellerNum)
    {
        var command = "DELETE FROM reseller "
                      + "WHERE ResellerNum = " + SOut.Long(resellerNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listResellerNums)
    {
        if (listResellerNums == null || listResellerNums.Count == 0) return;
        var command = "DELETE FROM reseller "
                      + "WHERE ResellerNum IN(" + string.Join(",", listResellerNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}