#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TransactionInvoiceCrud
{
    public static TransactionInvoice SelectOne(long transactionInvoiceNum)
    {
        var command = "SELECT * FROM transactioninvoice "
                      + "WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoiceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TransactionInvoice SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TransactionInvoice> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TransactionInvoice> TableToList(DataTable table)
    {
        var retVal = new List<TransactionInvoice>();
        TransactionInvoice transactionInvoice;
        foreach (DataRow row in table.Rows)
        {
            transactionInvoice = new TransactionInvoice();
            transactionInvoice.TransactionInvoiceNum = SIn.Long(row["TransactionInvoiceNum"].ToString());
            transactionInvoice.FileName = SIn.String(row["FileName"].ToString());
            transactionInvoice.InvoiceData = SIn.String(row["InvoiceData"].ToString());
            transactionInvoice.FilePath = SIn.String(row["FilePath"].ToString());
            retVal.Add(transactionInvoice);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TransactionInvoice> listTransactionInvoices, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TransactionInvoice";
        var table = new DataTable(tableName);
        table.Columns.Add("TransactionInvoiceNum");
        table.Columns.Add("FileName");
        table.Columns.Add("InvoiceData");
        table.Columns.Add("FilePath");
        foreach (var transactionInvoice in listTransactionInvoices)
            table.Rows.Add(SOut.Long(transactionInvoice.TransactionInvoiceNum), transactionInvoice.FileName, transactionInvoice.InvoiceData, transactionInvoice.FilePath);
        return table;
    }

    public static long Insert(TransactionInvoice transactionInvoice)
    {
        return Insert(transactionInvoice, false);
    }

    public static long Insert(TransactionInvoice transactionInvoice, bool useExistingPK)
    {
        var command = "INSERT INTO transactioninvoice (";

        command += "FileName,InvoiceData,FilePath) VALUES(";

        command +=
            "'" + SOut.String(transactionInvoice.FileName) + "',"
            + DbHelper.ParamChar + "paramInvoiceData,"
            + "'" + SOut.String(transactionInvoice.FilePath) + "')";
        if (transactionInvoice.InvoiceData == null) transactionInvoice.InvoiceData = "";
        var paramInvoiceData = new OdSqlParameter("paramInvoiceData", OdDbType.Text, SOut.StringParam(transactionInvoice.InvoiceData));
        {
            transactionInvoice.TransactionInvoiceNum = Db.NonQ(command, true, "TransactionInvoiceNum", "transactionInvoice", paramInvoiceData);
        }
        return transactionInvoice.TransactionInvoiceNum;
    }

    public static long InsertNoCache(TransactionInvoice transactionInvoice)
    {
        return InsertNoCache(transactionInvoice, false);
    }

    public static long InsertNoCache(TransactionInvoice transactionInvoice, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO transactioninvoice (";
        if (isRandomKeys || useExistingPK) command += "TransactionInvoiceNum,";
        command += "FileName,InvoiceData,FilePath) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(transactionInvoice.TransactionInvoiceNum) + ",";
        command +=
            "'" + SOut.String(transactionInvoice.FileName) + "',"
            + DbHelper.ParamChar + "paramInvoiceData,"
            + "'" + SOut.String(transactionInvoice.FilePath) + "')";
        if (transactionInvoice.InvoiceData == null) transactionInvoice.InvoiceData = "";
        var paramInvoiceData = new OdSqlParameter("paramInvoiceData", OdDbType.Text, SOut.StringParam(transactionInvoice.InvoiceData));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramInvoiceData);
        else
            transactionInvoice.TransactionInvoiceNum = Db.NonQ(command, true, "TransactionInvoiceNum", "transactionInvoice", paramInvoiceData);
        return transactionInvoice.TransactionInvoiceNum;
    }

    public static void Update(TransactionInvoice transactionInvoice)
    {
        var command = "UPDATE transactioninvoice SET "
                      + "FileName             = '" + SOut.String(transactionInvoice.FileName) + "', "
                      + "InvoiceData          =  " + DbHelper.ParamChar + "paramInvoiceData, "
                      + "FilePath             = '" + SOut.String(transactionInvoice.FilePath) + "' "
                      + "WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoice.TransactionInvoiceNum);
        if (transactionInvoice.InvoiceData == null) transactionInvoice.InvoiceData = "";
        var paramInvoiceData = new OdSqlParameter("paramInvoiceData", OdDbType.Text, SOut.StringParam(transactionInvoice.InvoiceData));
        Db.NonQ(command, paramInvoiceData);
    }

    public static bool Update(TransactionInvoice transactionInvoice, TransactionInvoice oldTransactionInvoice)
    {
        var command = "";
        if (transactionInvoice.FileName != oldTransactionInvoice.FileName)
        {
            if (command != "") command += ",";
            command += "FileName = '" + SOut.String(transactionInvoice.FileName) + "'";
        }

        if (transactionInvoice.InvoiceData != oldTransactionInvoice.InvoiceData)
        {
            if (command != "") command += ",";
            command += "InvoiceData = " + DbHelper.ParamChar + "paramInvoiceData";
        }

        if (transactionInvoice.FilePath != oldTransactionInvoice.FilePath)
        {
            if (command != "") command += ",";
            command += "FilePath = '" + SOut.String(transactionInvoice.FilePath) + "'";
        }

        if (command == "") return false;
        if (transactionInvoice.InvoiceData == null) transactionInvoice.InvoiceData = "";
        var paramInvoiceData = new OdSqlParameter("paramInvoiceData", OdDbType.Text, SOut.StringParam(transactionInvoice.InvoiceData));
        command = "UPDATE transactioninvoice SET " + command
                                                   + " WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoice.TransactionInvoiceNum);
        Db.NonQ(command, paramInvoiceData);
        return true;
    }

    public static bool UpdateComparison(TransactionInvoice transactionInvoice, TransactionInvoice oldTransactionInvoice)
    {
        if (transactionInvoice.FileName != oldTransactionInvoice.FileName) return true;
        if (transactionInvoice.InvoiceData != oldTransactionInvoice.InvoiceData) return true;
        if (transactionInvoice.FilePath != oldTransactionInvoice.FilePath) return true;
        return false;
    }

    public static void Delete(long transactionInvoiceNum)
    {
        var command = "DELETE FROM transactioninvoice "
                      + "WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoiceNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTransactionInvoiceNums)
    {
        if (listTransactionInvoiceNums == null || listTransactionInvoiceNums.Count == 0) return;
        var command = "DELETE FROM transactioninvoice "
                      + "WHERE TransactionInvoiceNum IN(" + string.Join(",", listTransactionInvoiceNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}