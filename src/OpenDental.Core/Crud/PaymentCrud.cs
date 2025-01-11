#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PaymentCrud
{
    public static Payment SelectOne(long payNum)
    {
        var command = "SELECT * FROM payment "
                      + "WHERE PayNum = " + SOut.Long(payNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        Payment payment;
        foreach (DataRow row in table.Rows)
        {
            payment = new Payment();
            payment.PayNum = SIn.Long(row["PayNum"].ToString());
            payment.PayType = SIn.Long(row["PayType"].ToString());
            payment.PayDate = SIn.Date(row["PayDate"].ToString());
            payment.PayAmt = SIn.Double(row["PayAmt"].ToString());
            payment.CheckNum = SIn.String(row["CheckNum"].ToString());
            payment.BankBranch = SIn.String(row["BankBranch"].ToString());
            payment.PayNote = SIn.String(row["PayNote"].ToString());
            payment.IsSplit = SIn.Bool(row["IsSplit"].ToString());
            payment.PatNum = SIn.Long(row["PatNum"].ToString());
            payment.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            payment.DateEntry = SIn.Date(row["DateEntry"].ToString());
            payment.DepositNum = SIn.Long(row["DepositNum"].ToString());
            payment.Receipt = SIn.String(row["Receipt"].ToString());
            payment.IsRecurringCC = SIn.Bool(row["IsRecurringCC"].ToString());
            payment.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            payment.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            payment.PaymentSource = (CreditCardSource) SIn.Int(row["PaymentSource"].ToString());
            payment.ProcessStatus = (ProcessStat) SIn.Int(row["ProcessStatus"].ToString());
            payment.RecurringChargeDate = SIn.Date(row["RecurringChargeDate"].ToString());
            payment.ExternalId = SIn.String(row["ExternalId"].ToString());
            payment.PaymentStatus = (PaymentStatus) SIn.Int(row["PaymentStatus"].ToString());
            payment.IsCcCompleted = SIn.Bool(row["IsCcCompleted"].ToString());
            payment.MerchantFee = SIn.Double(row["MerchantFee"].ToString());
            retVal.Add(payment);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Payment> listPayments, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Payment";
        var table = new DataTable(tableName);
        table.Columns.Add("PayNum");
        table.Columns.Add("PayType");
        table.Columns.Add("PayDate");
        table.Columns.Add("PayAmt");
        table.Columns.Add("CheckNum");
        table.Columns.Add("BankBranch");
        table.Columns.Add("PayNote");
        table.Columns.Add("IsSplit");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("DateEntry");
        table.Columns.Add("DepositNum");
        table.Columns.Add("Receipt");
        table.Columns.Add("IsRecurringCC");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("PaymentSource");
        table.Columns.Add("ProcessStatus");
        table.Columns.Add("RecurringChargeDate");
        table.Columns.Add("ExternalId");
        table.Columns.Add("PaymentStatus");
        table.Columns.Add("IsCcCompleted");
        table.Columns.Add("MerchantFee");
        foreach (var payment in listPayments)
            table.Rows.Add(SOut.Long(payment.PayNum), SOut.Long(payment.PayType), SOut.DateT(payment.PayDate, false), SOut.Double(payment.PayAmt), payment.CheckNum, payment.BankBranch, payment.PayNote, SOut.Bool(payment.IsSplit), SOut.Long(payment.PatNum), SOut.Long(payment.ClinicNum), SOut.DateT(payment.DateEntry, false), SOut.Long(payment.DepositNum), payment.Receipt, SOut.Bool(payment.IsRecurringCC), SOut.Long(payment.SecUserNumEntry), SOut.DateT(payment.SecDateTEdit, false), SOut.Int((int) payment.PaymentSource), SOut.Int((int) payment.ProcessStatus), SOut.DateT(payment.RecurringChargeDate, false), payment.ExternalId, SOut.Int((int) payment.PaymentStatus), SOut.Bool(payment.IsCcCompleted), SOut.Double(payment.MerchantFee));
        return table;
    }

    public static long Insert(Payment payment)
    {
        return Insert(payment, false);
    }

    public static long Insert(Payment payment, bool useExistingPK)
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
                                       + DbHelper.Now() + ","
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
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringNote(payment.PayNote));
        if (payment.Receipt == null) payment.Receipt = "";
        var paramReceipt = new OdSqlParameter("paramReceipt", OdDbType.Text, SOut.StringParam(payment.Receipt));
        {
            payment.PayNum = Db.NonQ(command, true, "PayNum", "payment", paramPayNote, paramReceipt);
        }
        return payment.PayNum;
    }

    public static void InsertMany(List<Payment> listPayments)
    {
        InsertMany(listPayments, false);
    }

    public static void InsertMany(List<Payment> listPayments, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPayments.Count)
        {
            var payment = listPayments[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO payment (");
                if (useExistingPK) sbCommands.Append("PayNum,");
                sbCommands.Append("PayType,PayDate,PayAmt,CheckNum,BankBranch,PayNote,IsSplit,PatNum,ClinicNum,DateEntry,DepositNum,Receipt,IsRecurringCC,SecUserNumEntry,PaymentSource,ProcessStatus,RecurringChargeDate,ExternalId,PaymentStatus,IsCcCompleted,MerchantFee) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(payment.PayNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(payment.PayType));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(payment.PayDate));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payment.PayAmt));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payment.CheckNum) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payment.BankBranch) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payment.PayNote) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payment.IsSplit));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payment.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payment.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payment.DepositNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payment.Receipt) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payment.IsRecurringCC));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payment.SecUserNumEntry));
            sbRow.Append(",");
            //SecDateTEdit can only be set by MySQL
            sbRow.Append(SOut.Int((int) payment.PaymentSource));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payment.ProcessStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(payment.RecurringChargeDate));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payment.ExternalId) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payment.PaymentStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payment.IsCcCompleted));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payment.MerchantFee));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listPayments.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Payment payment)
    {
        return InsertNoCache(payment, false);
    }

    public static long InsertNoCache(Payment payment, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payment (";
        if (isRandomKeys || useExistingPK) command += "PayNum,";
        command += "PayType,PayDate,PayAmt,CheckNum,BankBranch,PayNote,IsSplit,PatNum,ClinicNum,DateEntry,DepositNum,Receipt,IsRecurringCC,SecUserNumEntry,PaymentSource,ProcessStatus,RecurringChargeDate,ExternalId,PaymentStatus,IsCcCompleted,MerchantFee) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payment.PayNum) + ",";
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
                                       + DbHelper.Now() + ","
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
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringNote(payment.PayNote));
        if (payment.Receipt == null) payment.Receipt = "";
        var paramReceipt = new OdSqlParameter("paramReceipt", OdDbType.Text, SOut.StringParam(payment.Receipt));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPayNote, paramReceipt);
        else
            payment.PayNum = Db.NonQ(command, true, "PayNum", "payment", paramPayNote, paramReceipt);
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
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringNote(payment.PayNote));
        if (payment.Receipt == null) payment.Receipt = "";
        var paramReceipt = new OdSqlParameter("paramReceipt", OdDbType.Text, SOut.StringParam(payment.Receipt));
        Db.NonQ(command, paramPayNote, paramReceipt);
    }

    public static bool Update(Payment payment, Payment oldPayment)
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

        if (command == "") return false;
        if (payment.PayNote == null) payment.PayNote = "";
        var paramPayNote = new OdSqlParameter("paramPayNote", OdDbType.Text, SOut.StringNote(payment.PayNote));
        if (payment.Receipt == null) payment.Receipt = "";
        var paramReceipt = new OdSqlParameter("paramReceipt", OdDbType.Text, SOut.StringParam(payment.Receipt));
        command = "UPDATE payment SET " + command
                                        + " WHERE PayNum = " + SOut.Long(payment.PayNum);
        Db.NonQ(command, paramPayNote, paramReceipt);
        return true;
    }

    public static bool UpdateComparison(Payment payment, Payment oldPayment)
    {
        if (payment.PayType != oldPayment.PayType) return true;
        if (payment.PayDate.Date != oldPayment.PayDate.Date) return true;
        if (payment.PayAmt != oldPayment.PayAmt) return true;
        if (payment.CheckNum != oldPayment.CheckNum) return true;
        if (payment.BankBranch != oldPayment.BankBranch) return true;
        if (payment.PayNote != oldPayment.PayNote) return true;
        if (payment.IsSplit != oldPayment.IsSplit) return true;
        if (payment.PatNum != oldPayment.PatNum) return true;
        if (payment.ClinicNum != oldPayment.ClinicNum) return true;
        //DateEntry not allowed to change
        //DepositNum excluded from update
        if (payment.Receipt != oldPayment.Receipt) return true;
        if (payment.IsRecurringCC != oldPayment.IsRecurringCC) return true;
        //SecUserNumEntry excluded from update
        //SecDateTEdit can only be set by MySQL
        if (payment.PaymentSource != oldPayment.PaymentSource) return true;
        if (payment.ProcessStatus != oldPayment.ProcessStatus) return true;
        if (payment.RecurringChargeDate.Date != oldPayment.RecurringChargeDate.Date) return true;
        if (payment.ExternalId != oldPayment.ExternalId) return true;
        if (payment.PaymentStatus != oldPayment.PaymentStatus) return true;
        if (payment.IsCcCompleted != oldPayment.IsCcCompleted) return true;
        if (payment.MerchantFee != oldPayment.MerchantFee) return true;
        return false;
    }

    public static void Delete(long payNum)
    {
        var command = "DELETE FROM payment "
                      + "WHERE PayNum = " + SOut.Long(payNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayNums)
    {
        if (listPayNums == null || listPayNums.Count == 0) return;
        var command = "DELETE FROM payment "
                      + "WHERE PayNum IN(" + string.Join(",", listPayNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}