using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class CreditCardCrud
{
    public static CreditCard SelectOne(long creditCardNum)
    {
        var command = "SELECT * FROM creditcard "
                      + "WHERE CreditCardNum = " + SOut.Long(creditCardNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static CreditCard SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<CreditCard> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CreditCard> TableToList(DataTable table)
    {
        var retVal = new List<CreditCard>();
        foreach (DataRow row in table.Rows)
        {
            var creditCard = new CreditCard
            {
                CreditCardNum = SIn.Long(row["CreditCardNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                XChargeToken = SIn.String(row["XChargeToken"].ToString()),
                CCNumberMasked = SIn.String(row["CCNumberMasked"].ToString()),
                CCExpiration = SIn.Date(row["CCExpiration"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                ChargeAmt = SIn.Double(row["ChargeAmt"].ToString()),
                DateStart = SIn.Date(row["DateStart"].ToString()),
                DateStop = SIn.Date(row["DateStop"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                PayPlanNum = SIn.Long(row["PayPlanNum"].ToString()),
                PayConnectToken = SIn.String(row["PayConnectToken"].ToString()),
                PayConnectTokenExp = SIn.Date(row["PayConnectTokenExp"].ToString()),
                Procedures = SIn.String(row["Procedures"].ToString()),
                CCSource = (CreditCardSource) SIn.Int(row["CCSource"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                ExcludeProcSync = SIn.Bool(row["ExcludeProcSync"].ToString()),
                PaySimpleToken = SIn.String(row["PaySimpleToken"].ToString()),
                ChargeFrequency = SIn.String(row["ChargeFrequency"].ToString()),
                CanChargeWhenNoBal = SIn.Bool(row["CanChargeWhenNoBal"].ToString()),
                PaymentType = SIn.Long(row["PaymentType"].ToString()),
                IsRecurringActive = SIn.Bool(row["IsRecurringActive"].ToString()),
                Nickname = SIn.String(row["Nickname"].ToString())
            };
            retVal.Add(creditCard);
        }

        return retVal;
    }

    public static void Insert(CreditCard creditCard)
    {
        var command = "INSERT INTO creditcard (";

        command += "PatNum,Address,Zip,XChargeToken,CCNumberMasked,CCExpiration,ItemOrder,ChargeAmt,DateStart,DateStop,Note,PayPlanNum,PayConnectToken,PayConnectTokenExp,Procedures,CCSource,ClinicNum,ExcludeProcSync,PaySimpleToken,ChargeFrequency,CanChargeWhenNoBal,PaymentType,IsRecurringActive,Nickname) VALUES(";

        command +=
            SOut.Long(creditCard.PatNum) + ","
                                         + "'" + SOut.String(creditCard.Address) + "',"
                                         + "'" + SOut.String(creditCard.Zip) + "',"
                                         + "'" + SOut.String(creditCard.XChargeToken) + "',"
                                         + "'" + SOut.String(creditCard.CCNumberMasked) + "',"
                                         + SOut.Date(creditCard.CCExpiration) + ","
                                         + SOut.Int(creditCard.ItemOrder) + ","
                                         + SOut.Double(creditCard.ChargeAmt) + ","
                                         + SOut.Date(creditCard.DateStart) + ","
                                         + SOut.Date(creditCard.DateStop) + ","
                                         + "'" + SOut.String(creditCard.Note) + "',"
                                         + SOut.Long(creditCard.PayPlanNum) + ","
                                         + "'" + SOut.String(creditCard.PayConnectToken) + "',"
                                         + SOut.Date(creditCard.PayConnectTokenExp) + ","
                                         + DbHelper.ParamChar + "paramProcedures,"
                                         + SOut.Int((int) creditCard.CCSource) + ","
                                         + SOut.Long(creditCard.ClinicNum) + ","
                                         + SOut.Bool(creditCard.ExcludeProcSync) + ","
                                         + "'" + SOut.String(creditCard.PaySimpleToken) + "',"
                                         + "'" + SOut.String(creditCard.ChargeFrequency) + "',"
                                         + SOut.Bool(creditCard.CanChargeWhenNoBal) + ","
                                         + SOut.Long(creditCard.PaymentType) + ","
                                         + SOut.Bool(creditCard.IsRecurringActive) + ","
                                         + "'" + SOut.String(creditCard.Nickname) + "')";
        if (creditCard.Procedures == null) creditCard.Procedures = "";
        var paramProcedures = new OdSqlParameter("paramProcedures", SOut.StringParam(creditCard.Procedures));
        {
            creditCard.CreditCardNum = Db.NonQ(command, true, "CreditCardNum", "creditCard", paramProcedures);
        }
    }

    public static void Update(CreditCard creditCard)
    {
        var command = "UPDATE creditcard SET "
                      + "PatNum            =  " + SOut.Long(creditCard.PatNum) + ", "
                      + "Address           = '" + SOut.String(creditCard.Address) + "', "
                      + "Zip               = '" + SOut.String(creditCard.Zip) + "', "
                      + "XChargeToken      = '" + SOut.String(creditCard.XChargeToken) + "', "
                      + "CCNumberMasked    = '" + SOut.String(creditCard.CCNumberMasked) + "', "
                      + "CCExpiration      =  " + SOut.Date(creditCard.CCExpiration) + ", "
                      + "ItemOrder         =  " + SOut.Int(creditCard.ItemOrder) + ", "
                      + "ChargeAmt         =  " + SOut.Double(creditCard.ChargeAmt) + ", "
                      + "DateStart         =  " + SOut.Date(creditCard.DateStart) + ", "
                      + "DateStop          =  " + SOut.Date(creditCard.DateStop) + ", "
                      + "Note              = '" + SOut.String(creditCard.Note) + "', "
                      + "PayPlanNum        =  " + SOut.Long(creditCard.PayPlanNum) + ", "
                      + "PayConnectToken   = '" + SOut.String(creditCard.PayConnectToken) + "', "
                      + "PayConnectTokenExp=  " + SOut.Date(creditCard.PayConnectTokenExp) + ", "
                      + "Procedures        =  " + DbHelper.ParamChar + "paramProcedures, "
                      + "CCSource          =  " + SOut.Int((int) creditCard.CCSource) + ", "
                      + "ClinicNum         =  " + SOut.Long(creditCard.ClinicNum) + ", "
                      + "ExcludeProcSync   =  " + SOut.Bool(creditCard.ExcludeProcSync) + ", "
                      + "PaySimpleToken    = '" + SOut.String(creditCard.PaySimpleToken) + "', "
                      + "ChargeFrequency   = '" + SOut.String(creditCard.ChargeFrequency) + "', "
                      + "CanChargeWhenNoBal=  " + SOut.Bool(creditCard.CanChargeWhenNoBal) + ", "
                      + "PaymentType       =  " + SOut.Long(creditCard.PaymentType) + ", "
                      + "IsRecurringActive =  " + SOut.Bool(creditCard.IsRecurringActive) + ", "
                      + "Nickname          = '" + SOut.String(creditCard.Nickname) + "' "
                      + "WHERE CreditCardNum = " + SOut.Long(creditCard.CreditCardNum);
        if (creditCard.Procedures == null) creditCard.Procedures = "";
        var paramProcedures = new OdSqlParameter("paramProcedures", SOut.StringParam(creditCard.Procedures));
        Db.NonQ(command, paramProcedures);
    }
}