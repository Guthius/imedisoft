#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class XChargeTransactionCrud
{
    public static XChargeTransaction SelectOne(long xChargeTransactionNum)
    {
        var command = "SELECT * FROM xchargetransaction "
                      + "WHERE XChargeTransactionNum = " + SOut.Long(xChargeTransactionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static XChargeTransaction SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<XChargeTransaction> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<XChargeTransaction> TableToList(DataTable table)
    {
        var retVal = new List<XChargeTransaction>();
        XChargeTransaction xChargeTransaction;
        foreach (DataRow row in table.Rows)
        {
            xChargeTransaction = new XChargeTransaction();
            xChargeTransaction.XChargeTransactionNum = SIn.Long(row["XChargeTransactionNum"].ToString());
            xChargeTransaction.TransType = SIn.String(row["TransType"].ToString());
            xChargeTransaction.Amount = SIn.Double(row["Amount"].ToString());
            xChargeTransaction.CCEntry = SIn.String(row["CCEntry"].ToString());
            xChargeTransaction.PatNum = SIn.Long(row["PatNum"].ToString());
            xChargeTransaction.Result = SIn.String(row["Result"].ToString());
            xChargeTransaction.ClerkID = SIn.String(row["ClerkID"].ToString());
            xChargeTransaction.ResultCode = SIn.String(row["ResultCode"].ToString());
            xChargeTransaction.Expiration = SIn.String(row["Expiration"].ToString());
            xChargeTransaction.CCType = SIn.String(row["CCType"].ToString());
            xChargeTransaction.CreditCardNum = SIn.String(row["CreditCardNum"].ToString());
            xChargeTransaction.BatchNum = SIn.String(row["BatchNum"].ToString());
            xChargeTransaction.ItemNum = SIn.String(row["ItemNum"].ToString());
            xChargeTransaction.ApprCode = SIn.String(row["ApprCode"].ToString());
            xChargeTransaction.TransactionDateTime = SIn.DateTime(row["TransactionDateTime"].ToString());
            xChargeTransaction.BatchTotal = SIn.Double(row["BatchTotal"].ToString());
            retVal.Add(xChargeTransaction);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<XChargeTransaction> listXChargeTransactions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "XChargeTransaction";
        var table = new DataTable(tableName);
        table.Columns.Add("XChargeTransactionNum");
        table.Columns.Add("TransType");
        table.Columns.Add("Amount");
        table.Columns.Add("CCEntry");
        table.Columns.Add("PatNum");
        table.Columns.Add("Result");
        table.Columns.Add("ClerkID");
        table.Columns.Add("ResultCode");
        table.Columns.Add("Expiration");
        table.Columns.Add("CCType");
        table.Columns.Add("CreditCardNum");
        table.Columns.Add("BatchNum");
        table.Columns.Add("ItemNum");
        table.Columns.Add("ApprCode");
        table.Columns.Add("TransactionDateTime");
        table.Columns.Add("BatchTotal");
        foreach (var xChargeTransaction in listXChargeTransactions)
            table.Rows.Add(SOut.Long(xChargeTransaction.XChargeTransactionNum), xChargeTransaction.TransType, SOut.Double(xChargeTransaction.Amount), xChargeTransaction.CCEntry, SOut.Long(xChargeTransaction.PatNum), xChargeTransaction.Result, xChargeTransaction.ClerkID, xChargeTransaction.ResultCode, xChargeTransaction.Expiration, xChargeTransaction.CCType, xChargeTransaction.CreditCardNum, xChargeTransaction.BatchNum, xChargeTransaction.ItemNum, xChargeTransaction.ApprCode, SOut.DateT(xChargeTransaction.TransactionDateTime, false), SOut.Double(xChargeTransaction.BatchTotal));
        return table;
    }

    public static long Insert(XChargeTransaction xChargeTransaction)
    {
        return Insert(xChargeTransaction, false);
    }

    public static long Insert(XChargeTransaction xChargeTransaction, bool useExistingPK)
    {
        var command = "INSERT INTO xchargetransaction (";

        command += "TransType,Amount,CCEntry,PatNum,Result,ClerkID,ResultCode,Expiration,CCType,CreditCardNum,BatchNum,ItemNum,ApprCode,TransactionDateTime,BatchTotal) VALUES(";

        command +=
            "'" + SOut.String(xChargeTransaction.TransType) + "',"
            + SOut.Double(xChargeTransaction.Amount) + ","
            + "'" + SOut.String(xChargeTransaction.CCEntry) + "',"
            + SOut.Long(xChargeTransaction.PatNum) + ","
            + "'" + SOut.String(xChargeTransaction.Result) + "',"
            + "'" + SOut.String(xChargeTransaction.ClerkID) + "',"
            + "'" + SOut.String(xChargeTransaction.ResultCode) + "',"
            + "'" + SOut.String(xChargeTransaction.Expiration) + "',"
            + "'" + SOut.String(xChargeTransaction.CCType) + "',"
            + "'" + SOut.String(xChargeTransaction.CreditCardNum) + "',"
            + "'" + SOut.String(xChargeTransaction.BatchNum) + "',"
            + "'" + SOut.String(xChargeTransaction.ItemNum) + "',"
            + "'" + SOut.String(xChargeTransaction.ApprCode) + "',"
            + SOut.DateT(xChargeTransaction.TransactionDateTime) + ","
            + SOut.Double(xChargeTransaction.BatchTotal) + ")";
        {
            xChargeTransaction.XChargeTransactionNum = Db.NonQ(command, true, "XChargeTransactionNum", "xChargeTransaction");
        }
        return xChargeTransaction.XChargeTransactionNum;
    }

    public static long InsertNoCache(XChargeTransaction xChargeTransaction)
    {
        return InsertNoCache(xChargeTransaction, false);
    }

    public static long InsertNoCache(XChargeTransaction xChargeTransaction, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO xchargetransaction (";
        if (isRandomKeys || useExistingPK) command += "XChargeTransactionNum,";
        command += "TransType,Amount,CCEntry,PatNum,Result,ClerkID,ResultCode,Expiration,CCType,CreditCardNum,BatchNum,ItemNum,ApprCode,TransactionDateTime,BatchTotal) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(xChargeTransaction.XChargeTransactionNum) + ",";
        command +=
            "'" + SOut.String(xChargeTransaction.TransType) + "',"
            + SOut.Double(xChargeTransaction.Amount) + ","
            + "'" + SOut.String(xChargeTransaction.CCEntry) + "',"
            + SOut.Long(xChargeTransaction.PatNum) + ","
            + "'" + SOut.String(xChargeTransaction.Result) + "',"
            + "'" + SOut.String(xChargeTransaction.ClerkID) + "',"
            + "'" + SOut.String(xChargeTransaction.ResultCode) + "',"
            + "'" + SOut.String(xChargeTransaction.Expiration) + "',"
            + "'" + SOut.String(xChargeTransaction.CCType) + "',"
            + "'" + SOut.String(xChargeTransaction.CreditCardNum) + "',"
            + "'" + SOut.String(xChargeTransaction.BatchNum) + "',"
            + "'" + SOut.String(xChargeTransaction.ItemNum) + "',"
            + "'" + SOut.String(xChargeTransaction.ApprCode) + "',"
            + SOut.DateT(xChargeTransaction.TransactionDateTime) + ","
            + SOut.Double(xChargeTransaction.BatchTotal) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            xChargeTransaction.XChargeTransactionNum = Db.NonQ(command, true, "XChargeTransactionNum", "xChargeTransaction");
        return xChargeTransaction.XChargeTransactionNum;
    }

    public static void Update(XChargeTransaction xChargeTransaction)
    {
        var command = "UPDATE xchargetransaction SET "
                      + "TransType            = '" + SOut.String(xChargeTransaction.TransType) + "', "
                      + "Amount               =  " + SOut.Double(xChargeTransaction.Amount) + ", "
                      + "CCEntry              = '" + SOut.String(xChargeTransaction.CCEntry) + "', "
                      + "PatNum               =  " + SOut.Long(xChargeTransaction.PatNum) + ", "
                      + "Result               = '" + SOut.String(xChargeTransaction.Result) + "', "
                      + "ClerkID              = '" + SOut.String(xChargeTransaction.ClerkID) + "', "
                      + "ResultCode           = '" + SOut.String(xChargeTransaction.ResultCode) + "', "
                      + "Expiration           = '" + SOut.String(xChargeTransaction.Expiration) + "', "
                      + "CCType               = '" + SOut.String(xChargeTransaction.CCType) + "', "
                      + "CreditCardNum        = '" + SOut.String(xChargeTransaction.CreditCardNum) + "', "
                      + "BatchNum             = '" + SOut.String(xChargeTransaction.BatchNum) + "', "
                      + "ItemNum              = '" + SOut.String(xChargeTransaction.ItemNum) + "', "
                      + "ApprCode             = '" + SOut.String(xChargeTransaction.ApprCode) + "', "
                      + "TransactionDateTime  =  " + SOut.DateT(xChargeTransaction.TransactionDateTime) + ", "
                      + "BatchTotal           =  " + SOut.Double(xChargeTransaction.BatchTotal) + " "
                      + "WHERE XChargeTransactionNum = " + SOut.Long(xChargeTransaction.XChargeTransactionNum);
        Db.NonQ(command);
    }

    public static bool Update(XChargeTransaction xChargeTransaction, XChargeTransaction oldXChargeTransaction)
    {
        var command = "";
        if (xChargeTransaction.TransType != oldXChargeTransaction.TransType)
        {
            if (command != "") command += ",";
            command += "TransType = '" + SOut.String(xChargeTransaction.TransType) + "'";
        }

        if (xChargeTransaction.Amount != oldXChargeTransaction.Amount)
        {
            if (command != "") command += ",";
            command += "Amount = " + SOut.Double(xChargeTransaction.Amount) + "";
        }

        if (xChargeTransaction.CCEntry != oldXChargeTransaction.CCEntry)
        {
            if (command != "") command += ",";
            command += "CCEntry = '" + SOut.String(xChargeTransaction.CCEntry) + "'";
        }

        if (xChargeTransaction.PatNum != oldXChargeTransaction.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(xChargeTransaction.PatNum) + "";
        }

        if (xChargeTransaction.Result != oldXChargeTransaction.Result)
        {
            if (command != "") command += ",";
            command += "Result = '" + SOut.String(xChargeTransaction.Result) + "'";
        }

        if (xChargeTransaction.ClerkID != oldXChargeTransaction.ClerkID)
        {
            if (command != "") command += ",";
            command += "ClerkID = '" + SOut.String(xChargeTransaction.ClerkID) + "'";
        }

        if (xChargeTransaction.ResultCode != oldXChargeTransaction.ResultCode)
        {
            if (command != "") command += ",";
            command += "ResultCode = '" + SOut.String(xChargeTransaction.ResultCode) + "'";
        }

        if (xChargeTransaction.Expiration != oldXChargeTransaction.Expiration)
        {
            if (command != "") command += ",";
            command += "Expiration = '" + SOut.String(xChargeTransaction.Expiration) + "'";
        }

        if (xChargeTransaction.CCType != oldXChargeTransaction.CCType)
        {
            if (command != "") command += ",";
            command += "CCType = '" + SOut.String(xChargeTransaction.CCType) + "'";
        }

        if (xChargeTransaction.CreditCardNum != oldXChargeTransaction.CreditCardNum)
        {
            if (command != "") command += ",";
            command += "CreditCardNum = '" + SOut.String(xChargeTransaction.CreditCardNum) + "'";
        }

        if (xChargeTransaction.BatchNum != oldXChargeTransaction.BatchNum)
        {
            if (command != "") command += ",";
            command += "BatchNum = '" + SOut.String(xChargeTransaction.BatchNum) + "'";
        }

        if (xChargeTransaction.ItemNum != oldXChargeTransaction.ItemNum)
        {
            if (command != "") command += ",";
            command += "ItemNum = '" + SOut.String(xChargeTransaction.ItemNum) + "'";
        }

        if (xChargeTransaction.ApprCode != oldXChargeTransaction.ApprCode)
        {
            if (command != "") command += ",";
            command += "ApprCode = '" + SOut.String(xChargeTransaction.ApprCode) + "'";
        }

        if (xChargeTransaction.TransactionDateTime != oldXChargeTransaction.TransactionDateTime)
        {
            if (command != "") command += ",";
            command += "TransactionDateTime = " + SOut.DateT(xChargeTransaction.TransactionDateTime) + "";
        }

        if (xChargeTransaction.BatchTotal != oldXChargeTransaction.BatchTotal)
        {
            if (command != "") command += ",";
            command += "BatchTotal = " + SOut.Double(xChargeTransaction.BatchTotal) + "";
        }

        if (command == "") return false;
        command = "UPDATE xchargetransaction SET " + command
                                                   + " WHERE XChargeTransactionNum = " + SOut.Long(xChargeTransaction.XChargeTransactionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(XChargeTransaction xChargeTransaction, XChargeTransaction oldXChargeTransaction)
    {
        if (xChargeTransaction.TransType != oldXChargeTransaction.TransType) return true;
        if (xChargeTransaction.Amount != oldXChargeTransaction.Amount) return true;
        if (xChargeTransaction.CCEntry != oldXChargeTransaction.CCEntry) return true;
        if (xChargeTransaction.PatNum != oldXChargeTransaction.PatNum) return true;
        if (xChargeTransaction.Result != oldXChargeTransaction.Result) return true;
        if (xChargeTransaction.ClerkID != oldXChargeTransaction.ClerkID) return true;
        if (xChargeTransaction.ResultCode != oldXChargeTransaction.ResultCode) return true;
        if (xChargeTransaction.Expiration != oldXChargeTransaction.Expiration) return true;
        if (xChargeTransaction.CCType != oldXChargeTransaction.CCType) return true;
        if (xChargeTransaction.CreditCardNum != oldXChargeTransaction.CreditCardNum) return true;
        if (xChargeTransaction.BatchNum != oldXChargeTransaction.BatchNum) return true;
        if (xChargeTransaction.ItemNum != oldXChargeTransaction.ItemNum) return true;
        if (xChargeTransaction.ApprCode != oldXChargeTransaction.ApprCode) return true;
        if (xChargeTransaction.TransactionDateTime != oldXChargeTransaction.TransactionDateTime) return true;
        if (xChargeTransaction.BatchTotal != oldXChargeTransaction.BatchTotal) return true;
        return false;
    }

    public static void Delete(long xChargeTransactionNum)
    {
        var command = "DELETE FROM xchargetransaction "
                      + "WHERE XChargeTransactionNum = " + SOut.Long(xChargeTransactionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listXChargeTransactionNums)
    {
        if (listXChargeTransactionNums == null || listXChargeTransactionNums.Count == 0) return;
        var command = "DELETE FROM xchargetransaction "
                      + "WHERE XChargeTransactionNum IN(" + string.Join(",", listXChargeTransactionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}