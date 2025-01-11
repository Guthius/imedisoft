#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.PayConnectService;

#endregion

namespace OpenDentBusiness.Crud;

public class PayConnectResponseWebCrud
{
    public static PayConnectResponseWeb SelectOne(long payConnectResponseWebNum)
    {
        var command = "SELECT * FROM payconnectresponseweb "
                      + "WHERE PayConnectResponseWebNum = " + SOut.Long(payConnectResponseWebNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PayConnectResponseWeb SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayConnectResponseWeb> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayConnectResponseWeb> TableToList(DataTable table)
    {
        var retVal = new List<PayConnectResponseWeb>();
        PayConnectResponseWeb payConnectResponseWeb;
        foreach (DataRow row in table.Rows)
        {
            payConnectResponseWeb = new PayConnectResponseWeb();
            payConnectResponseWeb.PayConnectResponseWebNum = SIn.Long(row["PayConnectResponseWebNum"].ToString());
            payConnectResponseWeb.PatNum = SIn.Long(row["PatNum"].ToString());
            payConnectResponseWeb.PayNum = SIn.Long(row["PayNum"].ToString());
            payConnectResponseWeb.CCSource = (CreditCardSource) SIn.Int(row["CCSource"].ToString());
            payConnectResponseWeb.Amount = SIn.Double(row["Amount"].ToString());
            payConnectResponseWeb.PayNote = SIn.String(row["PayNote"].ToString());
            payConnectResponseWeb.AccountToken = SIn.String(row["AccountToken"].ToString());
            payConnectResponseWeb.PayToken = SIn.String(row["PayToken"].ToString());
            var processingStatus = row["ProcessingStatus"].ToString();
            if (processingStatus == "")
                payConnectResponseWeb.ProcessingStatus = 0;
            else
                try
                {
                    payConnectResponseWeb.ProcessingStatus = (PayConnectWebStatus) Enum.Parse(typeof(PayConnectWebStatus), processingStatus);
                }
                catch
                {
                    payConnectResponseWeb.ProcessingStatus = 0;
                }

            payConnectResponseWeb.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            payConnectResponseWeb.DateTimePending = SIn.DateTime(row["DateTimePending"].ToString());
            payConnectResponseWeb.DateTimeCompleted = SIn.DateTime(row["DateTimeCompleted"].ToString());
            payConnectResponseWeb.DateTimeExpired = SIn.DateTime(row["DateTimeExpired"].ToString());
            payConnectResponseWeb.DateTimeLastError = SIn.DateTime(row["DateTimeLastError"].ToString());
            payConnectResponseWeb.LastResponseStr = SIn.String(row["LastResponseStr"].ToString());
            payConnectResponseWeb.IsTokenSaved = SIn.Bool(row["IsTokenSaved"].ToString());
            payConnectResponseWeb.PaymentToken = SIn.String(row["PaymentToken"].ToString());
            payConnectResponseWeb.ExpDateToken = SIn.String(row["ExpDateToken"].ToString());
            payConnectResponseWeb.RefNumber = SIn.String(row["RefNumber"].ToString());
            var transType = row["TransType"].ToString();
            if (transType == "")
                payConnectResponseWeb.TransType = 0;
            else
                try
                {
                    payConnectResponseWeb.TransType = (transType) Enum.Parse(typeof(transType), transType);
                }
                catch
                {
                    payConnectResponseWeb.TransType = 0;
                }

            payConnectResponseWeb.EmailResponse = SIn.String(row["EmailResponse"].ToString());
            payConnectResponseWeb.LogGuid = SIn.String(row["LogGuid"].ToString());
            retVal.Add(payConnectResponseWeb);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PayConnectResponseWeb> listPayConnectResponseWebs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayConnectResponseWeb";
        var table = new DataTable(tableName);
        table.Columns.Add("PayConnectResponseWebNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("PayNum");
        table.Columns.Add("CCSource");
        table.Columns.Add("Amount");
        table.Columns.Add("PayNote");
        table.Columns.Add("AccountToken");
        table.Columns.Add("PayToken");
        table.Columns.Add("ProcessingStatus");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("DateTimePending");
        table.Columns.Add("DateTimeCompleted");
        table.Columns.Add("DateTimeExpired");
        table.Columns.Add("DateTimeLastError");
        table.Columns.Add("LastResponseStr");
        table.Columns.Add("IsTokenSaved");
        table.Columns.Add("PaymentToken");
        table.Columns.Add("ExpDateToken");
        table.Columns.Add("RefNumber");
        table.Columns.Add("TransType");
        table.Columns.Add("EmailResponse");
        table.Columns.Add("LogGuid");
        foreach (var payConnectResponseWeb in listPayConnectResponseWebs)
            table.Rows.Add(SOut.Long(payConnectResponseWeb.PayConnectResponseWebNum), SOut.Long(payConnectResponseWeb.PatNum), SOut.Long(payConnectResponseWeb.PayNum), SOut.Int((int) payConnectResponseWeb.CCSource), SOut.Double(payConnectResponseWeb.Amount), payConnectResponseWeb.PayNote, payConnectResponseWeb.AccountToken, payConnectResponseWeb.PayToken, SOut.Int((int) payConnectResponseWeb.ProcessingStatus), SOut.DateT(payConnectResponseWeb.DateTimeEntry, false), SOut.DateT(payConnectResponseWeb.DateTimePending, false), SOut.DateT(payConnectResponseWeb.DateTimeCompleted, false), SOut.DateT(payConnectResponseWeb.DateTimeExpired, false), SOut.DateT(payConnectResponseWeb.DateTimeLastError, false), payConnectResponseWeb.LastResponseStr, SOut.Bool(payConnectResponseWeb.IsTokenSaved), payConnectResponseWeb.PaymentToken, payConnectResponseWeb.ExpDateToken, payConnectResponseWeb.RefNumber, SOut.Int((int) payConnectResponseWeb.TransType), payConnectResponseWeb.EmailResponse, payConnectResponseWeb.LogGuid);
        return table;
    }

    public static long Insert(PayConnectResponseWeb payConnectResponseWeb)
    {
        return Insert(payConnectResponseWeb, false);
    }

    public static long Insert(PayConnectResponseWeb payConnectResponseWeb, bool useExistingPK)
    {
        var command = "INSERT INTO payconnectresponseweb (";

        command += "PatNum,PayNum,CCSource,Amount,PayNote,AccountToken,PayToken,ProcessingStatus,DateTimeEntry,DateTimePending,DateTimeCompleted,DateTimeExpired,DateTimeLastError,LastResponseStr,IsTokenSaved,PaymentToken,ExpDateToken,RefNumber,TransType,EmailResponse,LogGuid) VALUES(";

        command +=
            SOut.Long(payConnectResponseWeb.PatNum) + ","
                                                    + SOut.Long(payConnectResponseWeb.PayNum) + ","
                                                    + SOut.Int((int) payConnectResponseWeb.CCSource) + ","
                                                    + SOut.Double(payConnectResponseWeb.Amount) + ","
                                                    + "'" + SOut.String(payConnectResponseWeb.PayNote) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.AccountToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.PayToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.ProcessingStatus.ToString()) + "',"
                                                    + DbHelper.Now() + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimePending) + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimeCompleted) + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimeExpired) + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimeLastError) + ","
                                                    + DbHelper.ParamChar + "paramLastResponseStr,"
                                                    + SOut.Bool(payConnectResponseWeb.IsTokenSaved) + ","
                                                    + "'" + SOut.String(payConnectResponseWeb.PaymentToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.ExpDateToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.RefNumber) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.TransType.ToString()) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.EmailResponse) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.LogGuid) + "')";
        if (payConnectResponseWeb.LastResponseStr == null) payConnectResponseWeb.LastResponseStr = "";
        var paramLastResponseStr = new OdSqlParameter("paramLastResponseStr", OdDbType.Text, SOut.StringParam(payConnectResponseWeb.LastResponseStr));
        {
            payConnectResponseWeb.PayConnectResponseWebNum = Db.NonQ(command, true, "PayConnectResponseWebNum", "payConnectResponseWeb", paramLastResponseStr);
        }
        return payConnectResponseWeb.PayConnectResponseWebNum;
    }

    public static long InsertNoCache(PayConnectResponseWeb payConnectResponseWeb)
    {
        return InsertNoCache(payConnectResponseWeb, false);
    }

    public static long InsertNoCache(PayConnectResponseWeb payConnectResponseWeb, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payconnectresponseweb (";
        if (isRandomKeys || useExistingPK) command += "PayConnectResponseWebNum,";
        command += "PatNum,PayNum,CCSource,Amount,PayNote,AccountToken,PayToken,ProcessingStatus,DateTimeEntry,DateTimePending,DateTimeCompleted,DateTimeExpired,DateTimeLastError,LastResponseStr,IsTokenSaved,PaymentToken,ExpDateToken,RefNumber,TransType,EmailResponse,LogGuid) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payConnectResponseWeb.PayConnectResponseWebNum) + ",";
        command +=
            SOut.Long(payConnectResponseWeb.PatNum) + ","
                                                    + SOut.Long(payConnectResponseWeb.PayNum) + ","
                                                    + SOut.Int((int) payConnectResponseWeb.CCSource) + ","
                                                    + SOut.Double(payConnectResponseWeb.Amount) + ","
                                                    + "'" + SOut.String(payConnectResponseWeb.PayNote) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.AccountToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.PayToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.ProcessingStatus.ToString()) + "',"
                                                    + DbHelper.Now() + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimePending) + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimeCompleted) + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimeExpired) + ","
                                                    + SOut.DateT(payConnectResponseWeb.DateTimeLastError) + ","
                                                    + DbHelper.ParamChar + "paramLastResponseStr,"
                                                    + SOut.Bool(payConnectResponseWeb.IsTokenSaved) + ","
                                                    + "'" + SOut.String(payConnectResponseWeb.PaymentToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.ExpDateToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.RefNumber) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.TransType.ToString()) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.EmailResponse) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.LogGuid) + "')";
        if (payConnectResponseWeb.LastResponseStr == null) payConnectResponseWeb.LastResponseStr = "";
        var paramLastResponseStr = new OdSqlParameter("paramLastResponseStr", OdDbType.Text, SOut.StringParam(payConnectResponseWeb.LastResponseStr));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramLastResponseStr);
        else
            payConnectResponseWeb.PayConnectResponseWebNum = Db.NonQ(command, true, "PayConnectResponseWebNum", "payConnectResponseWeb", paramLastResponseStr);
        return payConnectResponseWeb.PayConnectResponseWebNum;
    }

    public static void Update(PayConnectResponseWeb payConnectResponseWeb)
    {
        var command = "UPDATE payconnectresponseweb SET "
                      + "PatNum                  =  " + SOut.Long(payConnectResponseWeb.PatNum) + ", "
                      + "PayNum                  =  " + SOut.Long(payConnectResponseWeb.PayNum) + ", "
                      + "CCSource                =  " + SOut.Int((int) payConnectResponseWeb.CCSource) + ", "
                      + "Amount                  =  " + SOut.Double(payConnectResponseWeb.Amount) + ", "
                      + "PayNote                 = '" + SOut.String(payConnectResponseWeb.PayNote) + "', "
                      + "AccountToken            = '" + SOut.String(payConnectResponseWeb.AccountToken) + "', "
                      + "PayToken                = '" + SOut.String(payConnectResponseWeb.PayToken) + "', "
                      + "ProcessingStatus        = '" + SOut.String(payConnectResponseWeb.ProcessingStatus.ToString()) + "', "
                      //DateTimeEntry not allowed to change
                      + "DateTimePending         =  " + SOut.DateT(payConnectResponseWeb.DateTimePending) + ", "
                      + "DateTimeCompleted       =  " + SOut.DateT(payConnectResponseWeb.DateTimeCompleted) + ", "
                      + "DateTimeExpired         =  " + SOut.DateT(payConnectResponseWeb.DateTimeExpired) + ", "
                      + "DateTimeLastError       =  " + SOut.DateT(payConnectResponseWeb.DateTimeLastError) + ", "
                      + "LastResponseStr         =  " + DbHelper.ParamChar + "paramLastResponseStr, "
                      + "IsTokenSaved            =  " + SOut.Bool(payConnectResponseWeb.IsTokenSaved) + ", "
                      + "PaymentToken            = '" + SOut.String(payConnectResponseWeb.PaymentToken) + "', "
                      + "ExpDateToken            = '" + SOut.String(payConnectResponseWeb.ExpDateToken) + "', "
                      + "RefNumber               = '" + SOut.String(payConnectResponseWeb.RefNumber) + "', "
                      + "TransType               = '" + SOut.String(payConnectResponseWeb.TransType.ToString()) + "', "
                      + "EmailResponse           = '" + SOut.String(payConnectResponseWeb.EmailResponse) + "', "
                      + "LogGuid                 = '" + SOut.String(payConnectResponseWeb.LogGuid) + "' "
                      + "WHERE PayConnectResponseWebNum = " + SOut.Long(payConnectResponseWeb.PayConnectResponseWebNum);
        if (payConnectResponseWeb.LastResponseStr == null) payConnectResponseWeb.LastResponseStr = "";
        var paramLastResponseStr = new OdSqlParameter("paramLastResponseStr", OdDbType.Text, SOut.StringParam(payConnectResponseWeb.LastResponseStr));
        Db.NonQ(command, paramLastResponseStr);
    }

    public static bool Update(PayConnectResponseWeb payConnectResponseWeb, PayConnectResponseWeb oldPayConnectResponseWeb)
    {
        var command = "";
        if (payConnectResponseWeb.PatNum != oldPayConnectResponseWeb.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(payConnectResponseWeb.PatNum) + "";
        }

        if (payConnectResponseWeb.PayNum != oldPayConnectResponseWeb.PayNum)
        {
            if (command != "") command += ",";
            command += "PayNum = " + SOut.Long(payConnectResponseWeb.PayNum) + "";
        }

        if (payConnectResponseWeb.CCSource != oldPayConnectResponseWeb.CCSource)
        {
            if (command != "") command += ",";
            command += "CCSource = " + SOut.Int((int) payConnectResponseWeb.CCSource) + "";
        }

        if (payConnectResponseWeb.Amount != oldPayConnectResponseWeb.Amount)
        {
            if (command != "") command += ",";
            command += "Amount = " + SOut.Double(payConnectResponseWeb.Amount) + "";
        }

        if (payConnectResponseWeb.PayNote != oldPayConnectResponseWeb.PayNote)
        {
            if (command != "") command += ",";
            command += "PayNote = '" + SOut.String(payConnectResponseWeb.PayNote) + "'";
        }

        if (payConnectResponseWeb.AccountToken != oldPayConnectResponseWeb.AccountToken)
        {
            if (command != "") command += ",";
            command += "AccountToken = '" + SOut.String(payConnectResponseWeb.AccountToken) + "'";
        }

        if (payConnectResponseWeb.PayToken != oldPayConnectResponseWeb.PayToken)
        {
            if (command != "") command += ",";
            command += "PayToken = '" + SOut.String(payConnectResponseWeb.PayToken) + "'";
        }

        if (payConnectResponseWeb.ProcessingStatus != oldPayConnectResponseWeb.ProcessingStatus)
        {
            if (command != "") command += ",";
            command += "ProcessingStatus = '" + SOut.String(payConnectResponseWeb.ProcessingStatus.ToString()) + "'";
        }

        //DateTimeEntry not allowed to change
        if (payConnectResponseWeb.DateTimePending != oldPayConnectResponseWeb.DateTimePending)
        {
            if (command != "") command += ",";
            command += "DateTimePending = " + SOut.DateT(payConnectResponseWeb.DateTimePending) + "";
        }

        if (payConnectResponseWeb.DateTimeCompleted != oldPayConnectResponseWeb.DateTimeCompleted)
        {
            if (command != "") command += ",";
            command += "DateTimeCompleted = " + SOut.DateT(payConnectResponseWeb.DateTimeCompleted) + "";
        }

        if (payConnectResponseWeb.DateTimeExpired != oldPayConnectResponseWeb.DateTimeExpired)
        {
            if (command != "") command += ",";
            command += "DateTimeExpired = " + SOut.DateT(payConnectResponseWeb.DateTimeExpired) + "";
        }

        if (payConnectResponseWeb.DateTimeLastError != oldPayConnectResponseWeb.DateTimeLastError)
        {
            if (command != "") command += ",";
            command += "DateTimeLastError = " + SOut.DateT(payConnectResponseWeb.DateTimeLastError) + "";
        }

        if (payConnectResponseWeb.LastResponseStr != oldPayConnectResponseWeb.LastResponseStr)
        {
            if (command != "") command += ",";
            command += "LastResponseStr = " + DbHelper.ParamChar + "paramLastResponseStr";
        }

        if (payConnectResponseWeb.IsTokenSaved != oldPayConnectResponseWeb.IsTokenSaved)
        {
            if (command != "") command += ",";
            command += "IsTokenSaved = " + SOut.Bool(payConnectResponseWeb.IsTokenSaved) + "";
        }

        if (payConnectResponseWeb.PaymentToken != oldPayConnectResponseWeb.PaymentToken)
        {
            if (command != "") command += ",";
            command += "PaymentToken = '" + SOut.String(payConnectResponseWeb.PaymentToken) + "'";
        }

        if (payConnectResponseWeb.ExpDateToken != oldPayConnectResponseWeb.ExpDateToken)
        {
            if (command != "") command += ",";
            command += "ExpDateToken = '" + SOut.String(payConnectResponseWeb.ExpDateToken) + "'";
        }

        if (payConnectResponseWeb.RefNumber != oldPayConnectResponseWeb.RefNumber)
        {
            if (command != "") command += ",";
            command += "RefNumber = '" + SOut.String(payConnectResponseWeb.RefNumber) + "'";
        }

        if (payConnectResponseWeb.TransType != oldPayConnectResponseWeb.TransType)
        {
            if (command != "") command += ",";
            command += "TransType = '" + SOut.String(payConnectResponseWeb.TransType.ToString()) + "'";
        }

        if (payConnectResponseWeb.EmailResponse != oldPayConnectResponseWeb.EmailResponse)
        {
            if (command != "") command += ",";
            command += "EmailResponse = '" + SOut.String(payConnectResponseWeb.EmailResponse) + "'";
        }

        if (payConnectResponseWeb.LogGuid != oldPayConnectResponseWeb.LogGuid)
        {
            if (command != "") command += ",";
            command += "LogGuid = '" + SOut.String(payConnectResponseWeb.LogGuid) + "'";
        }

        if (command == "") return false;
        if (payConnectResponseWeb.LastResponseStr == null) payConnectResponseWeb.LastResponseStr = "";
        var paramLastResponseStr = new OdSqlParameter("paramLastResponseStr", OdDbType.Text, SOut.StringParam(payConnectResponseWeb.LastResponseStr));
        command = "UPDATE payconnectresponseweb SET " + command
                                                      + " WHERE PayConnectResponseWebNum = " + SOut.Long(payConnectResponseWeb.PayConnectResponseWebNum);
        Db.NonQ(command, paramLastResponseStr);
        return true;
    }

    public static bool UpdateComparison(PayConnectResponseWeb payConnectResponseWeb, PayConnectResponseWeb oldPayConnectResponseWeb)
    {
        if (payConnectResponseWeb.PatNum != oldPayConnectResponseWeb.PatNum) return true;
        if (payConnectResponseWeb.PayNum != oldPayConnectResponseWeb.PayNum) return true;
        if (payConnectResponseWeb.CCSource != oldPayConnectResponseWeb.CCSource) return true;
        if (payConnectResponseWeb.Amount != oldPayConnectResponseWeb.Amount) return true;
        if (payConnectResponseWeb.PayNote != oldPayConnectResponseWeb.PayNote) return true;
        if (payConnectResponseWeb.AccountToken != oldPayConnectResponseWeb.AccountToken) return true;
        if (payConnectResponseWeb.PayToken != oldPayConnectResponseWeb.PayToken) return true;
        if (payConnectResponseWeb.ProcessingStatus != oldPayConnectResponseWeb.ProcessingStatus) return true;
        //DateTimeEntry not allowed to change
        if (payConnectResponseWeb.DateTimePending != oldPayConnectResponseWeb.DateTimePending) return true;
        if (payConnectResponseWeb.DateTimeCompleted != oldPayConnectResponseWeb.DateTimeCompleted) return true;
        if (payConnectResponseWeb.DateTimeExpired != oldPayConnectResponseWeb.DateTimeExpired) return true;
        if (payConnectResponseWeb.DateTimeLastError != oldPayConnectResponseWeb.DateTimeLastError) return true;
        if (payConnectResponseWeb.LastResponseStr != oldPayConnectResponseWeb.LastResponseStr) return true;
        if (payConnectResponseWeb.IsTokenSaved != oldPayConnectResponseWeb.IsTokenSaved) return true;
        if (payConnectResponseWeb.PaymentToken != oldPayConnectResponseWeb.PaymentToken) return true;
        if (payConnectResponseWeb.ExpDateToken != oldPayConnectResponseWeb.ExpDateToken) return true;
        if (payConnectResponseWeb.RefNumber != oldPayConnectResponseWeb.RefNumber) return true;
        if (payConnectResponseWeb.TransType != oldPayConnectResponseWeb.TransType) return true;
        if (payConnectResponseWeb.EmailResponse != oldPayConnectResponseWeb.EmailResponse) return true;
        if (payConnectResponseWeb.LogGuid != oldPayConnectResponseWeb.LogGuid) return true;
        return false;
    }

    public static void Delete(long payConnectResponseWebNum)
    {
        var command = "DELETE FROM payconnectresponseweb "
                      + "WHERE PayConnectResponseWebNum = " + SOut.Long(payConnectResponseWebNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayConnectResponseWebNums)
    {
        if (listPayConnectResponseWebNums == null || listPayConnectResponseWebNums.Count == 0) return;
        var command = "DELETE FROM payconnectresponseweb "
                      + "WHERE PayConnectResponseWebNum IN(" + string.Join(",", listPayConnectResponseWebNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}