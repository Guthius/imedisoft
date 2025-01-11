#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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

    public static List<XWebResponse> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<XWebResponse> TableToList(DataTable table)
    {
        var retVal = new List<XWebResponse>();
        XWebResponse xWebResponse;
        foreach (DataRow row in table.Rows)
        {
            xWebResponse = new XWebResponse();
            xWebResponse.XWebResponseNum = SIn.Long(row["XWebResponseNum"].ToString());
            xWebResponse.PatNum = SIn.Long(row["PatNum"].ToString());
            xWebResponse.ProvNum = SIn.Long(row["ProvNum"].ToString());
            xWebResponse.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            xWebResponse.PaymentNum = SIn.Long(row["PaymentNum"].ToString());
            xWebResponse.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            xWebResponse.DateTUpdate = SIn.DateTime(row["DateTUpdate"].ToString());
            xWebResponse.TransactionStatus = (XWebTransactionStatus) SIn.Int(row["TransactionStatus"].ToString());
            xWebResponse.ResponseCode = SIn.Int(row["ResponseCode"].ToString());
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

    public static DataTable ListToTable(List<XWebResponse> listXWebResponses, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "XWebResponse";
        var table = new DataTable(tableName);
        table.Columns.Add("XWebResponseNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("PaymentNum");
        table.Columns.Add("DateTEntry");
        table.Columns.Add("DateTUpdate");
        table.Columns.Add("TransactionStatus");
        table.Columns.Add("ResponseCode");
        table.Columns.Add("XWebResponseCode");
        table.Columns.Add("ResponseDescription");
        table.Columns.Add("OTK");
        table.Columns.Add("HpfUrl");
        table.Columns.Add("HpfExpiration");
        table.Columns.Add("TransactionID");
        table.Columns.Add("TransactionType");
        table.Columns.Add("Alias");
        table.Columns.Add("CardType");
        table.Columns.Add("CardBrand");
        table.Columns.Add("CardBrandShort");
        table.Columns.Add("MaskedAcctNum");
        table.Columns.Add("Amount");
        table.Columns.Add("ApprovalCode");
        table.Columns.Add("CardCodeResponse");
        table.Columns.Add("ReceiptID");
        table.Columns.Add("ExpDate");
        table.Columns.Add("EntryMethod");
        table.Columns.Add("ProcessorResponse");
        table.Columns.Add("BatchNum");
        table.Columns.Add("BatchAmount");
        table.Columns.Add("AccountExpirationDate");
        table.Columns.Add("DebugError");
        table.Columns.Add("PayNote");
        table.Columns.Add("CCSource");
        table.Columns.Add("OrderId");
        table.Columns.Add("EmailResponse");
        table.Columns.Add("LogGuid");
        foreach (var xWebResponse in listXWebResponses)
            table.Rows.Add(SOut.Long(xWebResponse.XWebResponseNum), SOut.Long(xWebResponse.PatNum), SOut.Long(xWebResponse.ProvNum), SOut.Long(xWebResponse.ClinicNum), SOut.Long(xWebResponse.PaymentNum), SOut.DateT(xWebResponse.DateTEntry, false), SOut.DateT(xWebResponse.DateTUpdate, false), SOut.Int((int) xWebResponse.TransactionStatus), SOut.Int(xWebResponse.ResponseCode), SOut.Int((int) xWebResponse.XWebResponseCode), xWebResponse.ResponseDescription, xWebResponse.OTK, xWebResponse.HpfUrl, SOut.DateT(xWebResponse.HpfExpiration, false), xWebResponse.TransactionID, xWebResponse.TransactionType, xWebResponse.Alias, xWebResponse.CardType, xWebResponse.CardBrand, xWebResponse.CardBrandShort, xWebResponse.MaskedAcctNum, SOut.Double(xWebResponse.Amount), xWebResponse.ApprovalCode, xWebResponse.CardCodeResponse, SOut.Int(xWebResponse.ReceiptID), xWebResponse.ExpDate, xWebResponse.EntryMethod, xWebResponse.ProcessorResponse, SOut.Int(xWebResponse.BatchNum), SOut.Double(xWebResponse.BatchAmount), SOut.DateT(xWebResponse.AccountExpirationDate, false), xWebResponse.DebugError, xWebResponse.PayNote, SOut.Int((int) xWebResponse.CCSource), xWebResponse.OrderId, xWebResponse.EmailResponse, xWebResponse.LogGuid);
        return table;
    }

    public static long Insert(XWebResponse xWebResponse)
    {
        return Insert(xWebResponse, false);
    }

    public static long Insert(XWebResponse xWebResponse, bool useExistingPK)
    {
        var command = "INSERT INTO xwebresponse (";

        command += "PatNum,ProvNum,ClinicNum,PaymentNum,DateTEntry,DateTUpdate,TransactionStatus,ResponseCode,XWebResponseCode,ResponseDescription,OTK,HpfUrl,HpfExpiration,TransactionID,TransactionType,Alias,CardType,CardBrand,CardBrandShort,MaskedAcctNum,Amount,ApprovalCode,CardCodeResponse,ReceiptID,ExpDate,EntryMethod,ProcessorResponse,BatchNum,BatchAmount,AccountExpirationDate,DebugError,PayNote,CCSource,OrderId,EmailResponse,LogGuid) VALUES(";

        command +=
            SOut.Long(xWebResponse.PatNum) + ","
                                           + SOut.Long(xWebResponse.ProvNum) + ","
                                           + SOut.Long(xWebResponse.ClinicNum) + ","
                                           + SOut.Long(xWebResponse.PaymentNum) + ","
                                           + DbHelper.Now() + ","
                                           + SOut.DateT(xWebResponse.DateTUpdate) + ","
                                           + SOut.Int((int) xWebResponse.TransactionStatus) + ","
                                           + SOut.Int(xWebResponse.ResponseCode) + ","
                                           + "'" + SOut.String(xWebResponse.XWebResponseCode.ToString()) + "',"
                                           + "'" + SOut.String(xWebResponse.ResponseDescription) + "',"
                                           + "'" + SOut.String(xWebResponse.OTK) + "',"
                                           + DbHelper.ParamChar + "paramHpfUrl,"
                                           + SOut.DateT(xWebResponse.HpfExpiration) + ","
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
        var paramHpfUrl = new OdSqlParameter("paramHpfUrl", OdDbType.Text, SOut.StringParam(xWebResponse.HpfUrl));
        if (xWebResponse.DebugError == null) xWebResponse.DebugError = "";
        var paramDebugError = new OdSqlParameter("paramDebugError", OdDbType.Text, SOut.StringParam(xWebResponse.DebugError));
        if (xWebResponse.PayNote == null) xWebResponse.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringParam(xWebResponse.PayNote));
        {
            xWebResponse.XWebResponseNum = Db.NonQ(command, true, "XWebResponseNum", "xWebResponse", paramHpfUrl, paramDebugError, paramPayNote);
        }
        return xWebResponse.XWebResponseNum;
    }

    public static long InsertNoCache(XWebResponse xWebResponse)
    {
        return InsertNoCache(xWebResponse, false);
    }

    public static long InsertNoCache(XWebResponse xWebResponse, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO xwebresponse (";
        if (isRandomKeys || useExistingPK) command += "XWebResponseNum,";
        command += "PatNum,ProvNum,ClinicNum,PaymentNum,DateTEntry,DateTUpdate,TransactionStatus,ResponseCode,XWebResponseCode,ResponseDescription,OTK,HpfUrl,HpfExpiration,TransactionID,TransactionType,Alias,CardType,CardBrand,CardBrandShort,MaskedAcctNum,Amount,ApprovalCode,CardCodeResponse,ReceiptID,ExpDate,EntryMethod,ProcessorResponse,BatchNum,BatchAmount,AccountExpirationDate,DebugError,PayNote,CCSource,OrderId,EmailResponse,LogGuid) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(xWebResponse.XWebResponseNum) + ",";
        command +=
            SOut.Long(xWebResponse.PatNum) + ","
                                           + SOut.Long(xWebResponse.ProvNum) + ","
                                           + SOut.Long(xWebResponse.ClinicNum) + ","
                                           + SOut.Long(xWebResponse.PaymentNum) + ","
                                           + DbHelper.Now() + ","
                                           + SOut.DateT(xWebResponse.DateTUpdate) + ","
                                           + SOut.Int((int) xWebResponse.TransactionStatus) + ","
                                           + SOut.Int(xWebResponse.ResponseCode) + ","
                                           + "'" + SOut.String(xWebResponse.XWebResponseCode.ToString()) + "',"
                                           + "'" + SOut.String(xWebResponse.ResponseDescription) + "',"
                                           + "'" + SOut.String(xWebResponse.OTK) + "',"
                                           + DbHelper.ParamChar + "paramHpfUrl,"
                                           + SOut.DateT(xWebResponse.HpfExpiration) + ","
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
        var paramHpfUrl = new OdSqlParameter("paramHpfUrl", OdDbType.Text, SOut.StringParam(xWebResponse.HpfUrl));
        if (xWebResponse.DebugError == null) xWebResponse.DebugError = "";
        var paramDebugError = new OdSqlParameter("paramDebugError", OdDbType.Text, SOut.StringParam(xWebResponse.DebugError));
        if (xWebResponse.PayNote == null) xWebResponse.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringParam(xWebResponse.PayNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramHpfUrl, paramDebugError, paramPayNote);
        else
            xWebResponse.XWebResponseNum = Db.NonQ(command, true, "XWebResponseNum", "xWebResponse", paramHpfUrl, paramDebugError, paramPayNote);
        return xWebResponse.XWebResponseNum;
    }

    public static void Update(XWebResponse xWebResponse)
    {
        var command = "UPDATE xwebresponse SET "
                      + "PatNum               =  " + SOut.Long(xWebResponse.PatNum) + ", "
                      + "ProvNum              =  " + SOut.Long(xWebResponse.ProvNum) + ", "
                      + "ClinicNum            =  " + SOut.Long(xWebResponse.ClinicNum) + ", "
                      + "PaymentNum           =  " + SOut.Long(xWebResponse.PaymentNum) + ", "
                      //DateTEntry not allowed to change
                      + "DateTUpdate          =  " + SOut.DateT(xWebResponse.DateTUpdate) + ", "
                      + "TransactionStatus    =  " + SOut.Int((int) xWebResponse.TransactionStatus) + ", "
                      + "ResponseCode         =  " + SOut.Int(xWebResponse.ResponseCode) + ", "
                      + "XWebResponseCode     = '" + SOut.String(xWebResponse.XWebResponseCode.ToString()) + "', "
                      + "ResponseDescription  = '" + SOut.String(xWebResponse.ResponseDescription) + "', "
                      + "OTK                  = '" + SOut.String(xWebResponse.OTK) + "', "
                      + "HpfUrl               =  " + DbHelper.ParamChar + "paramHpfUrl, "
                      + "HpfExpiration        =  " + SOut.DateT(xWebResponse.HpfExpiration) + ", "
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
        var paramHpfUrl = new OdSqlParameter("paramHpfUrl", OdDbType.Text, SOut.StringParam(xWebResponse.HpfUrl));
        if (xWebResponse.DebugError == null) xWebResponse.DebugError = "";
        var paramDebugError = new OdSqlParameter("paramDebugError", OdDbType.Text, SOut.StringParam(xWebResponse.DebugError));
        if (xWebResponse.PayNote == null) xWebResponse.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringParam(xWebResponse.PayNote));
        Db.NonQ(command, paramHpfUrl, paramDebugError, paramPayNote);
    }

    public static bool Update(XWebResponse xWebResponse, XWebResponse oldXWebResponse)
    {
        var command = "";
        if (xWebResponse.PatNum != oldXWebResponse.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(xWebResponse.PatNum) + "";
        }

        if (xWebResponse.ProvNum != oldXWebResponse.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(xWebResponse.ProvNum) + "";
        }

        if (xWebResponse.ClinicNum != oldXWebResponse.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(xWebResponse.ClinicNum) + "";
        }

        if (xWebResponse.PaymentNum != oldXWebResponse.PaymentNum)
        {
            if (command != "") command += ",";
            command += "PaymentNum = " + SOut.Long(xWebResponse.PaymentNum) + "";
        }

        //DateTEntry not allowed to change
        if (xWebResponse.DateTUpdate != oldXWebResponse.DateTUpdate)
        {
            if (command != "") command += ",";
            command += "DateTUpdate = " + SOut.DateT(xWebResponse.DateTUpdate) + "";
        }

        if (xWebResponse.TransactionStatus != oldXWebResponse.TransactionStatus)
        {
            if (command != "") command += ",";
            command += "TransactionStatus = " + SOut.Int((int) xWebResponse.TransactionStatus) + "";
        }

        if (xWebResponse.ResponseCode != oldXWebResponse.ResponseCode)
        {
            if (command != "") command += ",";
            command += "ResponseCode = " + SOut.Int(xWebResponse.ResponseCode) + "";
        }

        if (xWebResponse.XWebResponseCode != oldXWebResponse.XWebResponseCode)
        {
            if (command != "") command += ",";
            command += "XWebResponseCode = '" + SOut.String(xWebResponse.XWebResponseCode.ToString()) + "'";
        }

        if (xWebResponse.ResponseDescription != oldXWebResponse.ResponseDescription)
        {
            if (command != "") command += ",";
            command += "ResponseDescription = '" + SOut.String(xWebResponse.ResponseDescription) + "'";
        }

        if (xWebResponse.OTK != oldXWebResponse.OTK)
        {
            if (command != "") command += ",";
            command += "OTK = '" + SOut.String(xWebResponse.OTK) + "'";
        }

        if (xWebResponse.HpfUrl != oldXWebResponse.HpfUrl)
        {
            if (command != "") command += ",";
            command += "HpfUrl = " + DbHelper.ParamChar + "paramHpfUrl";
        }

        if (xWebResponse.HpfExpiration != oldXWebResponse.HpfExpiration)
        {
            if (command != "") command += ",";
            command += "HpfExpiration = " + SOut.DateT(xWebResponse.HpfExpiration) + "";
        }

        if (xWebResponse.TransactionID != oldXWebResponse.TransactionID)
        {
            if (command != "") command += ",";
            command += "TransactionID = '" + SOut.String(xWebResponse.TransactionID) + "'";
        }

        if (xWebResponse.TransactionType != oldXWebResponse.TransactionType)
        {
            if (command != "") command += ",";
            command += "TransactionType = '" + SOut.String(xWebResponse.TransactionType) + "'";
        }

        if (xWebResponse.Alias != oldXWebResponse.Alias)
        {
            if (command != "") command += ",";
            command += "Alias = '" + SOut.String(xWebResponse.Alias) + "'";
        }

        if (xWebResponse.CardType != oldXWebResponse.CardType)
        {
            if (command != "") command += ",";
            command += "CardType = '" + SOut.String(xWebResponse.CardType) + "'";
        }

        if (xWebResponse.CardBrand != oldXWebResponse.CardBrand)
        {
            if (command != "") command += ",";
            command += "CardBrand = '" + SOut.String(xWebResponse.CardBrand) + "'";
        }

        if (xWebResponse.CardBrandShort != oldXWebResponse.CardBrandShort)
        {
            if (command != "") command += ",";
            command += "CardBrandShort = '" + SOut.String(xWebResponse.CardBrandShort) + "'";
        }

        if (xWebResponse.MaskedAcctNum != oldXWebResponse.MaskedAcctNum)
        {
            if (command != "") command += ",";
            command += "MaskedAcctNum = '" + SOut.String(xWebResponse.MaskedAcctNum) + "'";
        }

        if (xWebResponse.Amount != oldXWebResponse.Amount)
        {
            if (command != "") command += ",";
            command += "Amount = " + SOut.Double(xWebResponse.Amount) + "";
        }

        if (xWebResponse.ApprovalCode != oldXWebResponse.ApprovalCode)
        {
            if (command != "") command += ",";
            command += "ApprovalCode = '" + SOut.String(xWebResponse.ApprovalCode) + "'";
        }

        if (xWebResponse.CardCodeResponse != oldXWebResponse.CardCodeResponse)
        {
            if (command != "") command += ",";
            command += "CardCodeResponse = '" + SOut.String(xWebResponse.CardCodeResponse) + "'";
        }

        if (xWebResponse.ReceiptID != oldXWebResponse.ReceiptID)
        {
            if (command != "") command += ",";
            command += "ReceiptID = " + SOut.Int(xWebResponse.ReceiptID) + "";
        }

        if (xWebResponse.ExpDate != oldXWebResponse.ExpDate)
        {
            if (command != "") command += ",";
            command += "ExpDate = '" + SOut.String(xWebResponse.ExpDate) + "'";
        }

        if (xWebResponse.EntryMethod != oldXWebResponse.EntryMethod)
        {
            if (command != "") command += ",";
            command += "EntryMethod = '" + SOut.String(xWebResponse.EntryMethod) + "'";
        }

        if (xWebResponse.ProcessorResponse != oldXWebResponse.ProcessorResponse)
        {
            if (command != "") command += ",";
            command += "ProcessorResponse = '" + SOut.String(xWebResponse.ProcessorResponse) + "'";
        }

        if (xWebResponse.BatchNum != oldXWebResponse.BatchNum)
        {
            if (command != "") command += ",";
            command += "BatchNum = " + SOut.Int(xWebResponse.BatchNum) + "";
        }

        if (xWebResponse.BatchAmount != oldXWebResponse.BatchAmount)
        {
            if (command != "") command += ",";
            command += "BatchAmount = " + SOut.Double(xWebResponse.BatchAmount) + "";
        }

        if (xWebResponse.AccountExpirationDate.Date != oldXWebResponse.AccountExpirationDate.Date)
        {
            if (command != "") command += ",";
            command += "AccountExpirationDate = " + SOut.Date(xWebResponse.AccountExpirationDate) + "";
        }

        if (xWebResponse.DebugError != oldXWebResponse.DebugError)
        {
            if (command != "") command += ",";
            command += "DebugError = " + DbHelper.ParamChar + "paramDebugError";
        }

        if (xWebResponse.PayNote != oldXWebResponse.PayNote)
        {
            if (command != "") command += ",";
            command += "PayNote = " + DbHelper.ParamChar + "paramPayNote";
        }

        if (xWebResponse.CCSource != oldXWebResponse.CCSource)
        {
            if (command != "") command += ",";
            command += "CCSource = " + SOut.Int((int) xWebResponse.CCSource) + "";
        }

        if (xWebResponse.OrderId != oldXWebResponse.OrderId)
        {
            if (command != "") command += ",";
            command += "OrderId = '" + SOut.String(xWebResponse.OrderId) + "'";
        }

        if (xWebResponse.EmailResponse != oldXWebResponse.EmailResponse)
        {
            if (command != "") command += ",";
            command += "EmailResponse = '" + SOut.String(xWebResponse.EmailResponse) + "'";
        }

        if (xWebResponse.LogGuid != oldXWebResponse.LogGuid)
        {
            if (command != "") command += ",";
            command += "LogGuid = '" + SOut.String(xWebResponse.LogGuid) + "'";
        }

        if (command == "") return false;
        if (xWebResponse.HpfUrl == null) xWebResponse.HpfUrl = "";
        var paramHpfUrl = new OdSqlParameter("paramHpfUrl", OdDbType.Text, SOut.StringParam(xWebResponse.HpfUrl));
        if (xWebResponse.DebugError == null) xWebResponse.DebugError = "";
        var paramDebugError = new OdSqlParameter("paramDebugError", OdDbType.Text, SOut.StringParam(xWebResponse.DebugError));
        if (xWebResponse.PayNote == null) xWebResponse.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringParam(xWebResponse.PayNote));
        command = "UPDATE xwebresponse SET " + command
                                             + " WHERE XWebResponseNum = " + SOut.Long(xWebResponse.XWebResponseNum);
        Db.NonQ(command, paramHpfUrl, paramDebugError, paramPayNote);
        return true;
    }

    public static bool UpdateComparison(XWebResponse xWebResponse, XWebResponse oldXWebResponse)
    {
        if (xWebResponse.PatNum != oldXWebResponse.PatNum) return true;
        if (xWebResponse.ProvNum != oldXWebResponse.ProvNum) return true;
        if (xWebResponse.ClinicNum != oldXWebResponse.ClinicNum) return true;
        if (xWebResponse.PaymentNum != oldXWebResponse.PaymentNum) return true;
        //DateTEntry not allowed to change
        if (xWebResponse.DateTUpdate != oldXWebResponse.DateTUpdate) return true;
        if (xWebResponse.TransactionStatus != oldXWebResponse.TransactionStatus) return true;
        if (xWebResponse.ResponseCode != oldXWebResponse.ResponseCode) return true;
        if (xWebResponse.XWebResponseCode != oldXWebResponse.XWebResponseCode) return true;
        if (xWebResponse.ResponseDescription != oldXWebResponse.ResponseDescription) return true;
        if (xWebResponse.OTK != oldXWebResponse.OTK) return true;
        if (xWebResponse.HpfUrl != oldXWebResponse.HpfUrl) return true;
        if (xWebResponse.HpfExpiration != oldXWebResponse.HpfExpiration) return true;
        if (xWebResponse.TransactionID != oldXWebResponse.TransactionID) return true;
        if (xWebResponse.TransactionType != oldXWebResponse.TransactionType) return true;
        if (xWebResponse.Alias != oldXWebResponse.Alias) return true;
        if (xWebResponse.CardType != oldXWebResponse.CardType) return true;
        if (xWebResponse.CardBrand != oldXWebResponse.CardBrand) return true;
        if (xWebResponse.CardBrandShort != oldXWebResponse.CardBrandShort) return true;
        if (xWebResponse.MaskedAcctNum != oldXWebResponse.MaskedAcctNum) return true;
        if (xWebResponse.Amount != oldXWebResponse.Amount) return true;
        if (xWebResponse.ApprovalCode != oldXWebResponse.ApprovalCode) return true;
        if (xWebResponse.CardCodeResponse != oldXWebResponse.CardCodeResponse) return true;
        if (xWebResponse.ReceiptID != oldXWebResponse.ReceiptID) return true;
        if (xWebResponse.ExpDate != oldXWebResponse.ExpDate) return true;
        if (xWebResponse.EntryMethod != oldXWebResponse.EntryMethod) return true;
        if (xWebResponse.ProcessorResponse != oldXWebResponse.ProcessorResponse) return true;
        if (xWebResponse.BatchNum != oldXWebResponse.BatchNum) return true;
        if (xWebResponse.BatchAmount != oldXWebResponse.BatchAmount) return true;
        if (xWebResponse.AccountExpirationDate.Date != oldXWebResponse.AccountExpirationDate.Date) return true;
        if (xWebResponse.DebugError != oldXWebResponse.DebugError) return true;
        if (xWebResponse.PayNote != oldXWebResponse.PayNote) return true;
        if (xWebResponse.CCSource != oldXWebResponse.CCSource) return true;
        if (xWebResponse.OrderId != oldXWebResponse.OrderId) return true;
        if (xWebResponse.EmailResponse != oldXWebResponse.EmailResponse) return true;
        if (xWebResponse.LogGuid != oldXWebResponse.LogGuid) return true;
        return false;
    }

    public static void Delete(long xWebResponseNum)
    {
        var command = "DELETE FROM xwebresponse "
                      + "WHERE XWebResponseNum = " + SOut.Long(xWebResponseNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listXWebResponseNums)
    {
        if (listXWebResponseNums == null || listXWebResponseNums.Count == 0) return;
        var command = "DELETE FROM xwebresponse "
                      + "WHERE XWebResponseNum IN(" + string.Join(",", listXWebResponseNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}