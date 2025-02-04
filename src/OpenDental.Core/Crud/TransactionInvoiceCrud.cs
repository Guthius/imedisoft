using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TransactionInvoiceCrud
{
    public static TransactionInvoice SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TransactionInvoice> TableToList(DataTable table)
    {
        var retVal = new List<TransactionInvoice>();
        foreach (DataRow row in table.Rows)
        {
            var transactionInvoice = new TransactionInvoice
            {
                TransactionInvoiceNum = SIn.Long(row["TransactionInvoiceNum"].ToString()),
                FileName = SIn.String(row["FileName"].ToString()),
                InvoiceData = SIn.String(row["InvoiceData"].ToString()),
                FilePath = SIn.String(row["FilePath"].ToString())
            };
            retVal.Add(transactionInvoice);
        }

        return retVal;
    }

    public static void Insert(TransactionInvoice transactionInvoice)
    {
        var command = "INSERT INTO transactioninvoice (";

        command += "FileName,InvoiceData,FilePath) VALUES(";

        command +=
            "'" + SOut.String(transactionInvoice.FileName) + "',"
            + DbHelper.ParamChar + "paramInvoiceData,"
            + "'" + SOut.String(transactionInvoice.FilePath) + "')";
        if (transactionInvoice.InvoiceData == null) transactionInvoice.InvoiceData = "";
        var paramInvoiceData = new OdSqlParameter("paramInvoiceData", SOut.StringParam(transactionInvoice.InvoiceData));
        {
            transactionInvoice.TransactionInvoiceNum = Db.NonQ(command, true, "TransactionInvoiceNum", "transactionInvoice", paramInvoiceData);
        }
    }

    public static void Delete(long transactionInvoiceNum)
    {
        var command = "DELETE FROM transactioninvoice "
                      + "WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoiceNum);
        Db.NonQ(command);
    }
}