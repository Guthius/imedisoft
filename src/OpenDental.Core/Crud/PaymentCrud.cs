using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PaymentCrud
{
    public static Payment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Payment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Payment> TableToList(DataTable table)
    {
        var retVal = new List<Payment>();
        foreach (DataRow row in table.Rows)
        {
            var payment = new Payment
            {
                PayNum = SIn.Long(row["PayNum"].ToString()),
                PayType = SIn.Long(row["PayType"].ToString()),
                PayDate = SIn.Date(row["PayDate"].ToString()),
                PayAmt = SIn.Double(row["PayAmt"].ToString()),
                CheckNum = SIn.String(row["CheckNum"].ToString()),
                BankBranch = SIn.String(row["BankBranch"].ToString()),
                PayNote = SIn.String(row["PayNote"].ToString()),
                IsSplit = SIn.Bool(row["IsSplit"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                DateEntry = SIn.Date(row["DateEntry"].ToString()),
                DepositNum = SIn.Long(row["DepositNum"].ToString()),
                Receipt = SIn.String(row["Receipt"].ToString()),
                IsRecurringCC = SIn.Bool(row["IsRecurringCC"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                PaymentSource = (CreditCardSource) SIn.Int(row["PaymentSource"].ToString()),
                ProcessStatus = (ProcessStat) SIn.Int(row["ProcessStatus"].ToString()),
                RecurringChargeDate = SIn.Date(row["RecurringChargeDate"].ToString()),
                ExternalId = SIn.String(row["ExternalId"].ToString()),
                PaymentStatus = (PaymentStatus) SIn.Int(row["PaymentStatus"].ToString()),
                IsCcCompleted = SIn.Bool(row["IsCcCompleted"].ToString()),
                MerchantFee = SIn.Double(row["MerchantFee"].ToString())
            };
            retVal.Add(payment);
        }

        return retVal;
    }

    public static long Insert(Payment payment)
    {
        var command = "INSERT INTO payment (";

        command += "PayType,PayDate,PayAmt,CheckNum,BankBranch,PayNote,IsSplit,PatNum,ClinicNum,DateEntry,DepositNum,Receipt,IsRecurringCC,SecUserNumEntry,PaymentSource,ProcessStatus,RecurringChargeDate,ExternalId,PaymentStatus,IsCcCompleted,MerchantFee) VALUES(";

        command +=
            SOut.Long(payment.PayType) + ","
                                       + SOut.Date(payment.PayDate) + ","
                                       + SOut.Double(payment.PayAmt) + ","
                                       + "'" + SOut.String(payment.CheckNum) + "',"
                                       + "'" + SOut.String(payment.BankBranch) + "',"
                                       + DbHelper.ParamChar + "paramPayNote,"
                                       + SOut.Bool(payment.IsSplit) + ","
                                       + SOut.Long(payment.PatNum) + ","
                                       + SOut.Long(payment.ClinicNum) + ","
                                       + "NOW()" + ","
                                       + SOut.Long(payment.DepositNum) + ","
                                       + DbHelper.ParamChar + "paramReceipt,"
                                       + SOut.Bool(payment.IsRecurringCC) + ","
                                       + SOut.Long(payment.SecUserNumEntry) + ","
                                       //SecDateTEdit can only be set by MySQL
                                       + SOut.Int((int) payment.PaymentSource) + ","
                                       + SOut.Int((int) payment.ProcessStatus) + ","
                                       + SOut.Date(payment.RecurringChargeDate) + ","
                                       + "'" + SOut.String(payment.ExternalId) + "',"
                                       + SOut.Int((int) payment.PaymentStatus) + ","
                                       + SOut.Bool(payment.IsCcCompleted) + ","
                                       + SOut.Double(payment.MerchantFee) + ")";
        if (payment.PayNote == null) payment.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", SOut.StringNote(payment.PayNote));
        if (payment.Receipt == null) payment.Receipt = "";
        var paramReceipt = new OdSqlParameter("paramReceipt", SOut.StringParam(payment.Receipt));
        {
            payment.PayNum = Db.NonQ(command, true, "PayNum", "payment", paramPayNote, paramReceipt);
        }
        return payment.PayNum;
    }

    public static void Update(Payment payment)
    {
        var command = "UPDATE payment SET "
                      + "PayType            =  " + SOut.Long(payment.PayType) + ", "
                      + "PayDate            =  " + SOut.Date(payment.PayDate) + ", "
                      + "PayAmt             =  " + SOut.Double(payment.PayAmt) + ", "
                      + "CheckNum           = '" + SOut.String(payment.CheckNum) + "', "
                      + "BankBranch         = '" + SOut.String(payment.BankBranch) + "', "
                      + "PayNote            =  " + DbHelper.ParamChar + "paramPayNote, "
                      + "IsSplit            =  " + SOut.Bool(payment.IsSplit) + ", "
                      + "PatNum             =  " + SOut.Long(payment.PatNum) + ", "
                      + "ClinicNum          =  " + SOut.Long(payment.ClinicNum) + ", "
                      //DateEntry not allowed to change
                      //DepositNum excluded from update
                      + "Receipt            =  " + DbHelper.ParamChar + "paramReceipt, "
                      + "IsRecurringCC      =  " + SOut.Bool(payment.IsRecurringCC) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateTEdit can only be set by MySQL
                      + "PaymentSource      =  " + SOut.Int((int) payment.PaymentSource) + ", "
                      + "ProcessStatus      =  " + SOut.Int((int) payment.ProcessStatus) + ", "
                      + "RecurringChargeDate=  " + SOut.Date(payment.RecurringChargeDate) + ", "
                      + "ExternalId         = '" + SOut.String(payment.ExternalId) + "', "
                      + "PaymentStatus      =  " + SOut.Int((int) payment.PaymentStatus) + ", "
                      + "IsCcCompleted      =  " + SOut.Bool(payment.IsCcCompleted) + ", "
                      + "MerchantFee        =  " + SOut.Double(payment.MerchantFee) + " "
                      + "WHERE PayNum = " + SOut.Long(payment.PayNum);
        if (payment.PayNote == null) payment.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", SOut.StringNote(payment.PayNote));
        if (payment.Receipt == null) payment.Receipt = "";
        var paramReceipt = new OdSqlParameter("paramReceipt", SOut.StringParam(payment.Receipt));
        Db.NonQ(command, paramPayNote, paramReceipt);
    }

    public static void Update(Payment payment, Payment oldPayment)
    {
        var command = "";
        if (payment.PayType != oldPayment.PayType)
        {
            if (command != "") command += ",";
            command += "PayType = " + SOut.Long(payment.PayType) + "";
        }

        if (payment.PayDate.Date != oldPayment.PayDate.Date)
        {
            if (command != "") command += ",";
            command += "PayDate = " + SOut.Date(payment.PayDate) + "";
        }

        if (payment.PayAmt != oldPayment.PayAmt)
        {
            if (command != "") command += ",";
            command += "PayAmt = " + SOut.Double(payment.PayAmt) + "";
        }

        if (payment.CheckNum != oldPayment.CheckNum)
        {
            if (command != "") command += ",";
            command += "CheckNum = '" + SOut.String(payment.CheckNum) + "'";
        }

        if (payment.BankBranch != oldPayment.BankBranch)
        {
            if (command != "") command += ",";
            command += "BankBranch = '" + SOut.String(payment.BankBranch) + "'";
        }

        if (payment.PayNote != oldPayment.PayNote)
        {
            if (command != "") command += ",";
            command += "PayNote = " + DbHelper.ParamChar + "paramPayNote";
        }

        if (payment.IsSplit != oldPayment.IsSplit)
        {
            if (command != "") command += ",";
            command += "IsSplit = " + SOut.Bool(payment.IsSplit) + "";
        }

        if (payment.PatNum != oldPayment.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(payment.PatNum) + "";
        }

        if (payment.ClinicNum != oldPayment.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(payment.ClinicNum) + "";
        }

        //DateEntry not allowed to change
        //DepositNum excluded from update
        if (payment.Receipt != oldPayment.Receipt)
        {
            if (command != "") command += ",";
            command += "Receipt = " + DbHelper.ParamChar + "paramReceipt";
        }

        if (payment.IsRecurringCC != oldPayment.IsRecurringCC)
        {
            if (command != "") command += ",";
            command += "IsRecurringCC = " + SOut.Bool(payment.IsRecurringCC) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateTEdit can only be set by MySQL
        if (payment.PaymentSource != oldPayment.PaymentSource)
        {
            if (command != "") command += ",";
            command += "PaymentSource = " + SOut.Int((int) payment.PaymentSource) + "";
        }

        if (payment.ProcessStatus != oldPayment.ProcessStatus)
        {
            if (command != "") command += ",";
            command += "ProcessStatus = " + SOut.Int((int) payment.ProcessStatus) + "";
        }

        if (payment.RecurringChargeDate.Date != oldPayment.RecurringChargeDate.Date)
        {
            if (command != "") command += ",";
            command += "RecurringChargeDate = " + SOut.Date(payment.RecurringChargeDate) + "";
        }

        if (payment.ExternalId != oldPayment.ExternalId)
        {
            if (command != "") command += ",";
            command += "ExternalId = '" + SOut.String(payment.ExternalId) + "'";
        }

        if (payment.PaymentStatus != oldPayment.PaymentStatus)
        {
            if (command != "") command += ",";
            command += "PaymentStatus = " + SOut.Int((int) payment.PaymentStatus) + "";
        }

        if (payment.IsCcCompleted != oldPayment.IsCcCompleted)
        {
            if (command != "") command += ",";
            command += "IsCcCompleted = " + SOut.Bool(payment.IsCcCompleted) + "";
        }

        if (payment.MerchantFee != oldPayment.MerchantFee)
        {
            if (command != "") command += ",";
            command += "MerchantFee = " + SOut.Double(payment.MerchantFee) + "";
        }

        if (command == "") return;
        if (payment.PayNote == null) payment.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", SOut.StringNote(payment.PayNote));
        if (payment.Receipt == null) payment.Receipt = "";
        var paramReceipt = new OdSqlParameter("paramReceipt", SOut.StringParam(payment.Receipt));
        command = "UPDATE payment SET " + command
                                        + " WHERE PayNum = " + SOut.Long(payment.PayNum);
        Db.NonQ(command, paramPayNote, paramReceipt);
    }
}