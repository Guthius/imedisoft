using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDentBusiness.PayConnectService;

namespace Imedisoft.Core.Crud;

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

    public static List<PayConnectResponseWeb> TableToList(DataTable table)
    {
        var retVal = new List<PayConnectResponseWeb>();
        foreach (DataRow row in table.Rows)
        {
            var payConnectResponseWeb = new PayConnectResponseWeb
            {
                PayConnectResponseWebNum = SIn.Long(row["PayConnectResponseWebNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                PayNum = SIn.Long(row["PayNum"].ToString()),
                CCSource = (CreditCardSource) SIn.Int(row["CCSource"].ToString()),
                Amount = SIn.Double(row["Amount"].ToString()),
                PayNote = SIn.String(row["PayNote"].ToString()),
                AccountToken = SIn.String(row["AccountToken"].ToString()),
                PayToken = SIn.String(row["PayToken"].ToString())
            };
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

    public static void Insert(PayConnectResponseWeb payConnectResponseWeb)
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
                                                    + "NOW()" + ","
                                                    + SOut.DateTime(payConnectResponseWeb.DateTimePending) + ","
                                                    + SOut.DateTime(payConnectResponseWeb.DateTimeCompleted) + ","
                                                    + SOut.DateTime(payConnectResponseWeb.DateTimeExpired) + ","
                                                    + SOut.DateTime(payConnectResponseWeb.DateTimeLastError) + ","
                                                    + DbHelper.ParamChar + "paramLastResponseStr,"
                                                    + SOut.Bool(payConnectResponseWeb.IsTokenSaved) + ","
                                                    + "'" + SOut.String(payConnectResponseWeb.PaymentToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.ExpDateToken) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.RefNumber) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.TransType.ToString()) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.EmailResponse) + "',"
                                                    + "'" + SOut.String(payConnectResponseWeb.LogGuid) + "')";
        if (payConnectResponseWeb.LastResponseStr == null) payConnectResponseWeb.LastResponseStr = "";
        var paramLastResponseStr = new OdSqlParameter("paramLastResponseStr", SOut.StringParam(payConnectResponseWeb.LastResponseStr));
        {
            payConnectResponseWeb.PayConnectResponseWebNum = Db.NonQ(command, true, "PayConnectResponseWebNum", "payConnectResponseWeb", paramLastResponseStr);
        }
    }
}