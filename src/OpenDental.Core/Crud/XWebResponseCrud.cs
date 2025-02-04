using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class XWebResponseCrud
{
    public static XWebResponse SelectOne(long xWebResponseNum)
    {
        var command = "SELECT * FROM xwebresponse "
                      + "WHERE XWebResponseNum = " + SOut.Long(xWebResponseNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static XWebResponse SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<XWebResponse> TableToList(DataTable table)
    {
        var retVal = new List<XWebResponse>();
        foreach (DataRow row in table.Rows)
        {
            var xWebResponse = new XWebResponse
            {
                XWebResponseNum = SIn.Long(row["XWebResponseNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                PaymentNum = SIn.Long(row["PaymentNum"].ToString()),
                DateTEntry = SIn.DateTime(row["DateTEntry"].ToString()),
                DateTUpdate = SIn.DateTime(row["DateTUpdate"].ToString()),
                TransactionStatus = (XWebTransactionStatus) SIn.Int(row["TransactionStatus"].ToString()),
                ResponseCode = SIn.Int(row["ResponseCode"].ToString())
            };
            var xWebResponseCode = row["XWebResponseCode"].ToString();
            if (xWebResponseCode == "")
                xWebResponse.XWebResponseCode = 0;
            else
                try
                {
                    xWebResponse.XWebResponseCode = (XWebResponseCodes) Enum.Parse(typeof(XWebResponseCodes), xWebResponseCode);
                }
                catch
                {
                    xWebResponse.XWebResponseCode = 0;
                }

            xWebResponse.ResponseDescription = SIn.String(row["ResponseDescription"].ToString());
            xWebResponse.OTK = SIn.String(row["OTK"].ToString());
            xWebResponse.HpfUrl = SIn.String(row["HpfUrl"].ToString());
            xWebResponse.HpfExpiration = SIn.DateTime(row["HpfExpiration"].ToString());
            xWebResponse.TransactionID = SIn.String(row["TransactionID"].ToString());
            xWebResponse.TransactionType = SIn.String(row["TransactionType"].ToString());
            xWebResponse.Alias = SIn.String(row["Alias"].ToString());
            xWebResponse.CardType = SIn.String(row["CardType"].ToString());
            xWebResponse.CardBrand = SIn.String(row["CardBrand"].ToString());
            xWebResponse.CardBrandShort = SIn.String(row["CardBrandShort"].ToString());
            xWebResponse.MaskedAcctNum = SIn.String(row["MaskedAcctNum"].ToString());
            xWebResponse.Amount = SIn.Double(row["Amount"].ToString());
            xWebResponse.ApprovalCode = SIn.String(row["ApprovalCode"].ToString());
            xWebResponse.CardCodeResponse = SIn.String(row["CardCodeResponse"].ToString());
            xWebResponse.ReceiptID = SIn.Int(row["ReceiptID"].ToString());
            xWebResponse.ExpDate = SIn.String(row["ExpDate"].ToString());
            xWebResponse.EntryMethod = SIn.String(row["EntryMethod"].ToString());
            xWebResponse.ProcessorResponse = SIn.String(row["ProcessorResponse"].ToString());
            xWebResponse.BatchNum = SIn.Int(row["BatchNum"].ToString());
            xWebResponse.BatchAmount = SIn.Double(row["BatchAmount"].ToString());
            xWebResponse.AccountExpirationDate = SIn.Date(row["AccountExpirationDate"].ToString());
            xWebResponse.DebugError = SIn.String(row["DebugError"].ToString());
            xWebResponse.PayNote = SIn.String(row["PayNote"].ToString());
            xWebResponse.CCSource = (CreditCardSource) SIn.Int(row["CCSource"].ToString());
            xWebResponse.OrderId = SIn.String(row["OrderId"].ToString());
            xWebResponse.EmailResponse = SIn.String(row["EmailResponse"].ToString());
            xWebResponse.LogGuid = SIn.String(row["LogGuid"].ToString());
            retVal.Add(xWebResponse);
        }

        return retVal;
    }

    public static void Insert(XWebResponse xWebResponse)
    {
        var command = "INSERT INTO xwebresponse (";

        command += "PatNum,ProvNum,ClinicNum,PaymentNum,DateTEntry,DateTUpdate,TransactionStatus,ResponseCode,XWebResponseCode,ResponseDescription,OTK,HpfUrl,HpfExpiration,TransactionID,TransactionType,Alias,CardType,CardBrand,CardBrandShort,MaskedAcctNum,Amount,ApprovalCode,CardCodeResponse,ReceiptID,ExpDate,EntryMethod,ProcessorResponse,BatchNum,BatchAmount,AccountExpirationDate,DebugError,PayNote,CCSource,OrderId,EmailResponse,LogGuid) VALUES(";

        command +=
            SOut.Long(xWebResponse.PatNum) + ","
                                           + SOut.Long(xWebResponse.ProvNum) + ","
                                           + SOut.Long(xWebResponse.ClinicNum) + ","
                                           + SOut.Long(xWebResponse.PaymentNum) + ","
                                           + "NOW()" + ","
                                           + SOut.DateTime(xWebResponse.DateTUpdate) + ","
                                           + SOut.Int((int) xWebResponse.TransactionStatus) + ","
                                           + SOut.Int(xWebResponse.ResponseCode) + ","
                                           + "'" + SOut.String(xWebResponse.XWebResponseCode.ToString()) + "',"
                                           + "'" + SOut.String(xWebResponse.ResponseDescription) + "',"
                                           + "'" + SOut.String(xWebResponse.OTK) + "',"
                                           + DbHelper.ParamChar + "paramHpfUrl,"
                                           + SOut.DateTime(xWebResponse.HpfExpiration) + ","
                                           + "'" + SOut.String(xWebResponse.TransactionID) + "',"
                                           + "'" + SOut.String(xWebResponse.TransactionType) + "',"
                                           + "'" + SOut.String(xWebResponse.Alias) + "',"
                                           + "'" + SOut.String(xWebResponse.CardType) + "',"
                                           + "'" + SOut.String(xWebResponse.CardBrand) + "',"
                                           + "'" + SOut.String(xWebResponse.CardBrandShort) + "',"
                                           + "'" + SOut.String(xWebResponse.MaskedAcctNum) + "',"
                                           + SOut.Double(xWebResponse.Amount) + ","
                                           + "'" + SOut.String(xWebResponse.ApprovalCode) + "',"
                                           + "'" + SOut.String(xWebResponse.CardCodeResponse) + "',"
                                           + SOut.Int(xWebResponse.ReceiptID) + ","
                                           + "'" + SOut.String(xWebResponse.ExpDate) + "',"
                                           + "'" + SOut.String(xWebResponse.EntryMethod) + "',"
                                           + "'" + SOut.String(xWebResponse.ProcessorResponse) + "',"
                                           + SOut.Int(xWebResponse.BatchNum) + ","
                                           + SOut.Double(xWebResponse.BatchAmount) + ","
                                           + SOut.Date(xWebResponse.AccountExpirationDate) + ","
                                           + DbHelper.ParamChar + "paramDebugError,"
                                           + DbHelper.ParamChar + "paramPayNote,"
                                           + SOut.Int((int) xWebResponse.CCSource) + ","
                                           + "'" + SOut.String(xWebResponse.OrderId) + "',"
                                           + "'" + SOut.String(xWebResponse.EmailResponse) + "',"
                                           + "'" + SOut.String(xWebResponse.LogGuid) + "')";
        if (xWebResponse.HpfUrl == null) xWebResponse.HpfUrl = "";
        var paramHpfUrl = new OdSqlParameter("paramHpfUrl", SOut.StringParam(xWebResponse.HpfUrl));
        if (xWebResponse.DebugError == null) xWebResponse.DebugError = "";
        var paramDebugError = new OdSqlParameter("paramDebugError", SOut.StringParam(xWebResponse.DebugError));
        if (xWebResponse.PayNote == null) xWebResponse.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", SOut.StringParam(xWebResponse.PayNote));
        {
            xWebResponse.XWebResponseNum = Db.NonQ(command, true, "XWebResponseNum", "xWebResponse", paramHpfUrl, paramDebugError, paramPayNote);
        }
    }

    public static void Update(XWebResponse xWebResponse)
    {
        var command = "UPDATE xwebresponse SET "
                      + "PatNum               =  " + SOut.Long(xWebResponse.PatNum) + ", "
                      + "ProvNum              =  " + SOut.Long(xWebResponse.ProvNum) + ", "
                      + "ClinicNum            =  " + SOut.Long(xWebResponse.ClinicNum) + ", "
                      + "PaymentNum           =  " + SOut.Long(xWebResponse.PaymentNum) + ", "
                      //DateTEntry not allowed to change
                      + "DateTUpdate          =  " + SOut.DateTime(xWebResponse.DateTUpdate) + ", "
                      + "TransactionStatus    =  " + SOut.Int((int) xWebResponse.TransactionStatus) + ", "
                      + "ResponseCode         =  " + SOut.Int(xWebResponse.ResponseCode) + ", "
                      + "XWebResponseCode     = '" + SOut.String(xWebResponse.XWebResponseCode.ToString()) + "', "
                      + "ResponseDescription  = '" + SOut.String(xWebResponse.ResponseDescription) + "', "
                      + "OTK                  = '" + SOut.String(xWebResponse.OTK) + "', "
                      + "HpfUrl               =  " + DbHelper.ParamChar + "paramHpfUrl, "
                      + "HpfExpiration        =  " + SOut.DateTime(xWebResponse.HpfExpiration) + ", "
                      + "TransactionID        = '" + SOut.String(xWebResponse.TransactionID) + "', "
                      + "TransactionType      = '" + SOut.String(xWebResponse.TransactionType) + "', "
                      + "Alias                = '" + SOut.String(xWebResponse.Alias) + "', "
                      + "CardType             = '" + SOut.String(xWebResponse.CardType) + "', "
                      + "CardBrand            = '" + SOut.String(xWebResponse.CardBrand) + "', "
                      + "CardBrandShort       = '" + SOut.String(xWebResponse.CardBrandShort) + "', "
                      + "MaskedAcctNum        = '" + SOut.String(xWebResponse.MaskedAcctNum) + "', "
                      + "Amount               =  " + SOut.Double(xWebResponse.Amount) + ", "
                      + "ApprovalCode         = '" + SOut.String(xWebResponse.ApprovalCode) + "', "
                      + "CardCodeResponse     = '" + SOut.String(xWebResponse.CardCodeResponse) + "', "
                      + "ReceiptID            =  " + SOut.Int(xWebResponse.ReceiptID) + ", "
                      + "ExpDate              = '" + SOut.String(xWebResponse.ExpDate) + "', "
                      + "EntryMethod          = '" + SOut.String(xWebResponse.EntryMethod) + "', "
                      + "ProcessorResponse    = '" + SOut.String(xWebResponse.ProcessorResponse) + "', "
                      + "BatchNum             =  " + SOut.Int(xWebResponse.BatchNum) + ", "
                      + "BatchAmount          =  " + SOut.Double(xWebResponse.BatchAmount) + ", "
                      + "AccountExpirationDate=  " + SOut.Date(xWebResponse.AccountExpirationDate) + ", "
                      + "DebugError           =  " + DbHelper.ParamChar + "paramDebugError, "
                      + "PayNote              =  " + DbHelper.ParamChar + "paramPayNote, "
                      + "CCSource             =  " + SOut.Int((int) xWebResponse.CCSource) + ", "
                      + "OrderId              = '" + SOut.String(xWebResponse.OrderId) + "', "
                      + "EmailResponse        = '" + SOut.String(xWebResponse.EmailResponse) + "', "
                      + "LogGuid              = '" + SOut.String(xWebResponse.LogGuid) + "' "
                      + "WHERE XWebResponseNum = " + SOut.Long(xWebResponse.XWebResponseNum);
        if (xWebResponse.HpfUrl == null) xWebResponse.HpfUrl = "";
        var paramHpfUrl = new OdSqlParameter("paramHpfUrl", SOut.StringParam(xWebResponse.HpfUrl));
        if (xWebResponse.DebugError == null) xWebResponse.DebugError = "";
        var paramDebugError = new OdSqlParameter("paramDebugError", SOut.StringParam(xWebResponse.DebugError));
        if (xWebResponse.PayNote == null) xWebResponse.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", SOut.StringParam(xWebResponse.PayNote));
        Db.NonQ(command, paramHpfUrl, paramDebugError, paramPayNote);
    }
}