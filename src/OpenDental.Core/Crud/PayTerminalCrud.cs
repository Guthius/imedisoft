#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PayTerminalCrud
{
    public static PayTerminal SelectOne(long payTerminalNum)
    {
        var command = "SELECT * FROM payterminal "
                      + "WHERE PayTerminalNum = " + SOut.Long(payTerminalNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PayTerminal SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayTerminal> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayTerminal> TableToList(DataTable table)
    {
        var retVal = new List<PayTerminal>();
        PayTerminal payTerminal;
        foreach (DataRow row in table.Rows)
        {
            payTerminal = new PayTerminal();
            payTerminal.PayTerminalNum = SIn.Long(row["PayTerminalNum"].ToString());
            payTerminal.Name = SIn.String(row["Name"].ToString());
            payTerminal.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            payTerminal.TerminalID = SIn.String(row["TerminalID"].ToString());
            retVal.Add(payTerminal);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PayTerminal> listPayTerminals, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayTerminal";
        var table = new DataTable(tableName);
        table.Columns.Add("PayTerminalNum");
        table.Columns.Add("Name");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("TerminalID");
        foreach (var payTerminal in listPayTerminals)
            table.Rows.Add(SOut.Long(payTerminal.PayTerminalNum), payTerminal.Name, SOut.Long(payTerminal.ClinicNum), payTerminal.TerminalID);
        return table;
    }

    public static long Insert(PayTerminal payTerminal)
    {
        return Insert(payTerminal, false);
    }

    public static long Insert(PayTerminal payTerminal, bool useExistingPK)
    {
        var command = "INSERT INTO payterminal (";

        command += "Name,ClinicNum,TerminalID) VALUES(";

        command +=
            "'" + SOut.String(payTerminal.Name) + "',"
            + SOut.Long(payTerminal.ClinicNum) + ","
            + "'" + SOut.String(payTerminal.TerminalID) + "')";
        {
            payTerminal.PayTerminalNum = Db.NonQ(command, true, "PayTerminalNum", "payTerminal");
        }
        return payTerminal.PayTerminalNum;
    }

    public static long InsertNoCache(PayTerminal payTerminal)
    {
        return InsertNoCache(payTerminal, false);
    }

    public static long InsertNoCache(PayTerminal payTerminal, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payterminal (";
        if (isRandomKeys || useExistingPK) command += "PayTerminalNum,";
        command += "Name,ClinicNum,TerminalID) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payTerminal.PayTerminalNum) + ",";
        command +=
            "'" + SOut.String(payTerminal.Name) + "',"
            + SOut.Long(payTerminal.ClinicNum) + ","
            + "'" + SOut.String(payTerminal.TerminalID) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            payTerminal.PayTerminalNum = Db.NonQ(command, true, "PayTerminalNum", "payTerminal");
        return payTerminal.PayTerminalNum;
    }

    public static void Update(PayTerminal payTerminal)
    {
        var command = "UPDATE payterminal SET "
                      + "Name          = '" + SOut.String(payTerminal.Name) + "', "
                      + "ClinicNum     =  " + SOut.Long(payTerminal.ClinicNum) + ", "
                      + "TerminalID    = '" + SOut.String(payTerminal.TerminalID) + "' "
                      + "WHERE PayTerminalNum = " + SOut.Long(payTerminal.PayTerminalNum);
        Db.NonQ(command);
    }

    public static bool Update(PayTerminal payTerminal, PayTerminal oldPayTerminal)
    {
        var command = "";
        if (payTerminal.Name != oldPayTerminal.Name)
        {
            if (command != "") command += ",";
            command += "Name = '" + SOut.String(payTerminal.Name) + "'";
        }

        if (payTerminal.ClinicNum != oldPayTerminal.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(payTerminal.ClinicNum) + "";
        }

        if (payTerminal.TerminalID != oldPayTerminal.TerminalID)
        {
            if (command != "") command += ",";
            command += "TerminalID = '" + SOut.String(payTerminal.TerminalID) + "'";
        }

        if (command == "") return false;
        command = "UPDATE payterminal SET " + command
                                            + " WHERE PayTerminalNum = " + SOut.Long(payTerminal.PayTerminalNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PayTerminal payTerminal, PayTerminal oldPayTerminal)
    {
        if (payTerminal.Name != oldPayTerminal.Name) return true;
        if (payTerminal.ClinicNum != oldPayTerminal.ClinicNum) return true;
        if (payTerminal.TerminalID != oldPayTerminal.TerminalID) return true;
        return false;
    }

    public static void Delete(long payTerminalNum)
    {
        var command = "DELETE FROM payterminal "
                      + "WHERE PayTerminalNum = " + SOut.Long(payTerminalNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayTerminalNums)
    {
        if (listPayTerminalNums == null || listPayTerminalNums.Count == 0) return;
        var command = "DELETE FROM payterminal "
                      + "WHERE PayTerminalNum IN(" + string.Join(",", listPayTerminalNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}