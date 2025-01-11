using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class XWebResponses
{
    public static XWebResponse GetOne(long xWebResponseNum)
    {
        return XWebResponseCrud.SelectOne(xWebResponseNum);
    }

    public static DataTable GetApprovedTransactions(List<long> listClinicNums, DateTime dateFrom, DateTime dateTo)
    {
        #region XWeb query

        var command = "SELECT " + DbHelper.Concat("patient.LName", "', '", "patient.FName") + " Patient,xwebresponse.DateTUpdate,xwebresponse.TransactionID,"
                      + "xwebresponse.MaskedAcctNum,DATE_FORMAT(xwebresponse.AccountExpirationDate,'%m/%y') ExpDate,xwebresponse.Amount,xwebresponse.PaymentNum,xwebresponse.TransactionStatus,"
                      + "(CASE WHEN payment.PayNum IS NULL THEN 0 ELSE 1 END) doesPaymentExist,COALESCE(clinic.Abbr,'Unassigned') Clinic,xwebresponse.PatNum, "
                      + "xwebresponse.XWebResponseNum AS 'ResponseNum',xwebresponse.Alias,1 AS 'isXWeb' "
                      + "FROM xwebresponse "
                      + "INNER JOIN patient ON patient.PatNum=xwebresponse.PatNum "
                      + "LEFT JOIN payment ON payment.PayNum=xwebresponse.PaymentNum "
                      + "LEFT JOIN clinic ON clinic.ClinicNum=xwebresponse.ClinicNum "
                      + "WHERE xwebresponse.TransactionStatus IN("
                      + SOut.Int((int) XWebTransactionStatus.DtgPaymentApproved) + ","
                      + SOut.Int((int) XWebTransactionStatus.HpfCompletePaymentApproved) + ","
                      + SOut.Int((int) XWebTransactionStatus.HpfCompletePaymentApprovedPartial) + ","
                      + SOut.Int((int) XWebTransactionStatus.DtgPaymentReturned) + ","
                      + SOut.Int((int) XWebTransactionStatus.DtgPaymentVoided) + ","
                      + SOut.Int((int) XWebTransactionStatus.EdgeExpressCompletePaymentApproved) + ","
                      + SOut.Int((int) XWebTransactionStatus.EdgeExpressCompletePaymentApprovedPartial) + ") "
                      + "AND xwebresponse.ResponseCode IN("
                      + SOut.Int((int) XWebResponseCodes.Approval) + ","
                      + SOut.Int((int) XWebResponseCodes.PartialApproval) + ") "
                      + "AND xwebresponse.DateTUpdate BETWEEN " + SOut.DateT(dateFrom) + " AND " + SOut.DateT(dateTo.AddDays(1)) + " ";
        if (listClinicNums.Count > 0) command += "AND xwebresponse.ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") ";

        #endregion

        command += "UNION ALL ";

        #region PayConnect

        command += "SELECT " + DbHelper.Concat("patient.LName", "', '", "patient.FName") + " Patient,payconnectresponseweb.DateTimeCompleted,payconnectresponseweb.RefNumber,"
                   + "(CASE WHEN creditcard.CCNumberMasked IS NULL THEN 'CC Not Saved' ELSE creditcard.CCNumberMasked END),"
                   + "(CASE WHEN creditcard.CCExpiration IS NULL THEN '' ELSE DATE_FORMAT(creditcard.CCExpiration,'%m/%y') END),"
                   + "payconnectresponseweb.Amount,payconnectresponseweb.PayNum,payconnectresponseweb.TransType,(CASE WHEN payment.PayNum IS NULL THEN 0 ELSE 1 END) doesPaymentExist,"
                   + "COALESCE(clinic.Abbr,'Unassigned') Clinic,payconnectresponseweb.PatNum,payconnectresponseweb.PayConnectResponseWebNum,payconnectresponseweb.PayToken,0 AS 'isXWeb' "
                   + "FROM payconnectresponseweb "
                   + "INNER JOIN patient ON patient.PatNum=payconnectresponseweb.PatNum "
                   + "LEFT JOIN creditcard ON creditcard.PayConnectToken=payconnectresponseweb.PaymentToken "
                   + "LEFT JOIN payment ON payment.PayNum=payconnectresponseweb.PayNum "
                   + "LEFT JOIN clinic ON clinic.ClinicNum=payment.ClinicNum "
                   + "WHERE payconnectresponseweb.DateTimeCompleted BETWEEN " + SOut.DateT(dateFrom) + " AND " + SOut.DateT(dateTo.AddDays(1)) + " "
                   + "AND payconnectresponseweb.ProcessingStatus='" + PayConnectWebStatus.Completed + "' "
                   + "AND payconnectresponseweb.TransType!='' ";
        if (listClinicNums.Count > 0) command += "AND payment.ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") ";

        command += "ORDER BY DateTUpdate,Patient;";

        #endregion

        return DataCore.GetTable(command);
    }

    public static XWebResponse GetOneByPaymentNum(long paymentNum)
    {
        return XWebResponseCrud.SelectOne("SELECT * FROM xwebresponse WHERE PaymentNum=" + paymentNum);
    }

    public static void Insert(XWebResponse xWebResponse)
    {
        XWebResponseCrud.Insert(xWebResponse);
    }

    public static void Update(XWebResponse xWebResponse)
    {
        XWebResponseCrud.Update(xWebResponse);
    }

    public static string CreateOrderId()
    {
        var attempts = 0;

        while (true)
        {
            attempts++;
            if (attempts >= 1000)
            {
                throw new ODException("Reached 1000 attempts of trying to generate OrderId.");
            }

            var orderId = MiscUtils.CreateRandomNumericString(10);

            var command = $"SELECT COUNT(*) FROM xwebresponse WHERE OrderId='{SOut.String(orderId)}'";
            if (Db.GetCount(command) == "0")
            {
                return orderId;
            }
        }
    }
}