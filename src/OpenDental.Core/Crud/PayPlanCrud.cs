#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PayPlanCrud
{
    public static PayPlan SelectOne(long payPlanNum)
    {
        var command = "SELECT * FROM payplan "
                      + "WHERE PayPlanNum = " + SOut.Long(payPlanNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PayPlan SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayPlan> TableToList(DataTable table)
    {
        var retVal = new List<PayPlan>();
        PayPlan payPlan;
        foreach (DataRow row in table.Rows)
        {
            payPlan = new PayPlan();
            payPlan.PayPlanNum = SIn.Long(row["PayPlanNum"].ToString());
            payPlan.PatNum = SIn.Long(row["PatNum"].ToString());
            payPlan.Guarantor = SIn.Long(row["Guarantor"].ToString());
            payPlan.PayPlanDate = SIn.Date(row["PayPlanDate"].ToString());
            payPlan.APR = SIn.Double(row["APR"].ToString());
            payPlan.Note = SIn.String(row["Note"].ToString());
            payPlan.PlanNum = SIn.Long(row["PlanNum"].ToString());
            payPlan.CompletedAmt = SIn.Double(row["CompletedAmt"].ToString());
            payPlan.InsSubNum = SIn.Long(row["InsSubNum"].ToString());
            payPlan.PaySchedule = (PaymentSchedule) SIn.Int(row["PaySchedule"].ToString());
            payPlan.NumberOfPayments = SIn.Int(row["NumberOfPayments"].ToString());
            payPlan.PayAmt = SIn.Double(row["PayAmt"].ToString());
            payPlan.DownPayment = SIn.Double(row["DownPayment"].ToString());
            payPlan.IsClosed = SIn.Bool(row["IsClosed"].ToString());
            payPlan.Signature = SIn.String(row["Signature"].ToString());
            payPlan.SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString());
            payPlan.PlanCategory = SIn.Long(row["PlanCategory"].ToString());
            payPlan.IsDynamic = SIn.Bool(row["IsDynamic"].ToString());
            payPlan.ChargeFrequency = (PayPlanFrequency) SIn.Int(row["ChargeFrequency"].ToString());
            payPlan.DatePayPlanStart = SIn.Date(row["DatePayPlanStart"].ToString());
            payPlan.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            payPlan.DateInterestStart = SIn.Date(row["DateInterestStart"].ToString());
            payPlan.DynamicPayPlanTPOption = (DynamicPayPlanTPOptions) SIn.Int(row["DynamicPayPlanTPOption"].ToString());
            payPlan.MobileAppDeviceNum = SIn.Long(row["MobileAppDeviceNum"].ToString());
            payPlan.SecurityHash = SIn.String(row["SecurityHash"].ToString());
            payPlan.SheetDefNum = SIn.Long(row["SheetDefNum"].ToString());
            retVal.Add(payPlan);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PayPlan> listPayPlans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayPlan";
        var table = new DataTable(tableName);
        table.Columns.Add("PayPlanNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("Guarantor");
        table.Columns.Add("PayPlanDate");
        table.Columns.Add("APR");
        table.Columns.Add("Note");
        table.Columns.Add("PlanNum");
        table.Columns.Add("CompletedAmt");
        table.Columns.Add("InsSubNum");
        table.Columns.Add("PaySchedule");
        table.Columns.Add("NumberOfPayments");
        table.Columns.Add("PayAmt");
        table.Columns.Add("DownPayment");
        table.Columns.Add("IsClosed");
        table.Columns.Add("Signature");
        table.Columns.Add("SigIsTopaz");
        table.Columns.Add("PlanCategory");
        table.Columns.Add("IsDynamic");
        table.Columns.Add("ChargeFrequency");
        table.Columns.Add("DatePayPlanStart");
        table.Columns.Add("IsLocked");
        table.Columns.Add("DateInterestStart");
        table.Columns.Add("DynamicPayPlanTPOption");
        table.Columns.Add("MobileAppDeviceNum");
        table.Columns.Add("SecurityHash");
        table.Columns.Add("SheetDefNum");
        foreach (var payPlan in listPayPlans)
            table.Rows.Add(SOut.Long(payPlan.PayPlanNum), SOut.Long(payPlan.PatNum), SOut.Long(payPlan.Guarantor), SOut.DateT(payPlan.PayPlanDate, false), SOut.Double(payPlan.APR), payPlan.Note, SOut.Long(payPlan.PlanNum), SOut.Double(payPlan.CompletedAmt), SOut.Long(payPlan.InsSubNum), SOut.Int((int) payPlan.PaySchedule), SOut.Int(payPlan.NumberOfPayments), SOut.Double(payPlan.PayAmt), SOut.Double(payPlan.DownPayment), SOut.Bool(payPlan.IsClosed), payPlan.Signature, SOut.Bool(payPlan.SigIsTopaz), SOut.Long(payPlan.PlanCategory), SOut.Bool(payPlan.IsDynamic), SOut.Int((int) payPlan.ChargeFrequency), SOut.DateT(payPlan.DatePayPlanStart, false), SOut.Bool(payPlan.IsLocked), SOut.DateT(payPlan.DateInterestStart, false), SOut.Int((int) payPlan.DynamicPayPlanTPOption), SOut.Long(payPlan.MobileAppDeviceNum), payPlan.SecurityHash, SOut.Long(payPlan.SheetDefNum));
        return table;
    }

    public static long Insert(PayPlan payPlan)
    {
        return Insert(payPlan, false);
    }

    public static long Insert(PayPlan payPlan, bool useExistingPK)
    {
        var command = "INSERT INTO payplan (";

        command += "PatNum,Guarantor,PayPlanDate,APR,Note,PlanNum,CompletedAmt,InsSubNum,PaySchedule,NumberOfPayments,PayAmt,DownPayment,IsClosed,Signature,SigIsTopaz,PlanCategory,IsDynamic,ChargeFrequency,DatePayPlanStart,IsLocked,DateInterestStart,DynamicPayPlanTPOption,MobileAppDeviceNum,SecurityHash,SheetDefNum) VALUES(";

        command +=
            SOut.Long(payPlan.PatNum) + ","
                                      + SOut.Long(payPlan.Guarantor) + ","
                                      + SOut.Date(payPlan.PayPlanDate) + ","
                                      + SOut.Double(payPlan.APR) + ","
                                      + DbHelper.ParamChar + "paramNote,"
                                      + SOut.Long(payPlan.PlanNum) + ","
                                      + SOut.Double(payPlan.CompletedAmt) + ","
                                      + SOut.Long(payPlan.InsSubNum) + ","
                                      + SOut.Int((int) payPlan.PaySchedule) + ","
                                      + SOut.Int(payPlan.NumberOfPayments) + ","
                                      + SOut.Double(payPlan.PayAmt) + ","
                                      + SOut.Double(payPlan.DownPayment) + ","
                                      + SOut.Bool(payPlan.IsClosed) + ","
                                      + DbHelper.ParamChar + "paramSignature,"
                                      + SOut.Bool(payPlan.SigIsTopaz) + ","
                                      + SOut.Long(payPlan.PlanCategory) + ","
                                      + SOut.Bool(payPlan.IsDynamic) + ","
                                      + SOut.Int((int) payPlan.ChargeFrequency) + ","
                                      + SOut.Date(payPlan.DatePayPlanStart) + ","
                                      + SOut.Bool(payPlan.IsLocked) + ","
                                      + SOut.Date(payPlan.DateInterestStart) + ","
                                      + SOut.Int((int) payPlan.DynamicPayPlanTPOption) + ","
                                      + SOut.Long(payPlan.MobileAppDeviceNum) + ","
                                      + "'" + SOut.String(payPlan.SecurityHash) + "',"
                                      + SOut.Long(payPlan.SheetDefNum) + ")";
        if (payPlan.Note == null) payPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlan.Note));
        if (payPlan.Signature == null) payPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(payPlan.Signature));
        {
            payPlan.PayPlanNum = Db.NonQ(command, true, "PayPlanNum", "payPlan", paramNote, paramSignature);
        }
        return payPlan.PayPlanNum;
    }

    public static void InsertMany(List<PayPlan> listPayPlans)
    {
        InsertMany(listPayPlans, false);
    }

    public static void InsertMany(List<PayPlan> listPayPlans, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPayPlans.Count)
        {
            var payPlan = listPayPlans[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO payplan (");
                if (useExistingPK) sbCommands.Append("PayPlanNum,");
                sbCommands.Append("PatNum,Guarantor,PayPlanDate,APR,Note,PlanNum,CompletedAmt,InsSubNum,PaySchedule,NumberOfPayments,PayAmt,DownPayment,IsClosed,Signature,SigIsTopaz,PlanCategory,IsDynamic,ChargeFrequency,DatePayPlanStart,IsLocked,DateInterestStart,DynamicPayPlanTPOption,MobileAppDeviceNum,SecurityHash,SheetDefNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(payPlan.PayPlanNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(payPlan.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlan.Guarantor));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(payPlan.PayPlanDate));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlan.APR));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payPlan.Note) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlan.PlanNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlan.CompletedAmt));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlan.InsSubNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payPlan.PaySchedule));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(payPlan.NumberOfPayments));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlan.PayAmt));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlan.DownPayment));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payPlan.IsClosed));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payPlan.Signature) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payPlan.SigIsTopaz));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlan.PlanCategory));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payPlan.IsDynamic));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payPlan.ChargeFrequency));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(payPlan.DatePayPlanStart));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payPlan.IsLocked));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(payPlan.DateInterestStart));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payPlan.DynamicPayPlanTPOption));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlan.MobileAppDeviceNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payPlan.SecurityHash) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlan.SheetDefNum));
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
                if (index == listPayPlans.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PayPlan payPlan)
    {
        return InsertNoCache(payPlan, false);
    }

    public static long InsertNoCache(PayPlan payPlan, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payplan (";
        if (isRandomKeys || useExistingPK) command += "PayPlanNum,";
        command += "PatNum,Guarantor,PayPlanDate,APR,Note,PlanNum,CompletedAmt,InsSubNum,PaySchedule,NumberOfPayments,PayAmt,DownPayment,IsClosed,Signature,SigIsTopaz,PlanCategory,IsDynamic,ChargeFrequency,DatePayPlanStart,IsLocked,DateInterestStart,DynamicPayPlanTPOption,MobileAppDeviceNum,SecurityHash,SheetDefNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payPlan.PayPlanNum) + ",";
        command +=
            SOut.Long(payPlan.PatNum) + ","
                                      + SOut.Long(payPlan.Guarantor) + ","
                                      + SOut.Date(payPlan.PayPlanDate) + ","
                                      + SOut.Double(payPlan.APR) + ","
                                      + DbHelper.ParamChar + "paramNote,"
                                      + SOut.Long(payPlan.PlanNum) + ","
                                      + SOut.Double(payPlan.CompletedAmt) + ","
                                      + SOut.Long(payPlan.InsSubNum) + ","
                                      + SOut.Int((int) payPlan.PaySchedule) + ","
                                      + SOut.Int(payPlan.NumberOfPayments) + ","
                                      + SOut.Double(payPlan.PayAmt) + ","
                                      + SOut.Double(payPlan.DownPayment) + ","
                                      + SOut.Bool(payPlan.IsClosed) + ","
                                      + DbHelper.ParamChar + "paramSignature,"
                                      + SOut.Bool(payPlan.SigIsTopaz) + ","
                                      + SOut.Long(payPlan.PlanCategory) + ","
                                      + SOut.Bool(payPlan.IsDynamic) + ","
                                      + SOut.Int((int) payPlan.ChargeFrequency) + ","
                                      + SOut.Date(payPlan.DatePayPlanStart) + ","
                                      + SOut.Bool(payPlan.IsLocked) + ","
                                      + SOut.Date(payPlan.DateInterestStart) + ","
                                      + SOut.Int((int) payPlan.DynamicPayPlanTPOption) + ","
                                      + SOut.Long(payPlan.MobileAppDeviceNum) + ","
                                      + "'" + SOut.String(payPlan.SecurityHash) + "',"
                                      + SOut.Long(payPlan.SheetDefNum) + ")";
        if (payPlan.Note == null) payPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlan.Note));
        if (payPlan.Signature == null) payPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(payPlan.Signature));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote, paramSignature);
        else
            payPlan.PayPlanNum = Db.NonQ(command, true, "PayPlanNum", "payPlan", paramNote, paramSignature);
        return payPlan.PayPlanNum;
    }

    public static void Update(PayPlan payPlan)
    {
        var command = "UPDATE payplan SET "
                      + "PatNum                =  " + SOut.Long(payPlan.PatNum) + ", "
                      + "Guarantor             =  " + SOut.Long(payPlan.Guarantor) + ", "
                      + "PayPlanDate           =  " + SOut.Date(payPlan.PayPlanDate) + ", "
                      + "APR                   =  " + SOut.Double(payPlan.APR) + ", "
                      + "Note                  =  " + DbHelper.ParamChar + "paramNote, "
                      + "PlanNum               =  " + SOut.Long(payPlan.PlanNum) + ", "
                      + "CompletedAmt          =  " + SOut.Double(payPlan.CompletedAmt) + ", "
                      + "InsSubNum             =  " + SOut.Long(payPlan.InsSubNum) + ", "
                      + "PaySchedule           =  " + SOut.Int((int) payPlan.PaySchedule) + ", "
                      + "NumberOfPayments      =  " + SOut.Int(payPlan.NumberOfPayments) + ", "
                      + "PayAmt                =  " + SOut.Double(payPlan.PayAmt) + ", "
                      + "DownPayment           =  " + SOut.Double(payPlan.DownPayment) + ", "
                      + "IsClosed              =  " + SOut.Bool(payPlan.IsClosed) + ", "
                      + "Signature             =  " + DbHelper.ParamChar + "paramSignature, "
                      + "SigIsTopaz            =  " + SOut.Bool(payPlan.SigIsTopaz) + ", "
                      + "PlanCategory          =  " + SOut.Long(payPlan.PlanCategory) + ", "
                      + "IsDynamic             =  " + SOut.Bool(payPlan.IsDynamic) + ", "
                      + "ChargeFrequency       =  " + SOut.Int((int) payPlan.ChargeFrequency) + ", "
                      + "DatePayPlanStart      =  " + SOut.Date(payPlan.DatePayPlanStart) + ", "
                      + "IsLocked              =  " + SOut.Bool(payPlan.IsLocked) + ", "
                      + "DateInterestStart     =  " + SOut.Date(payPlan.DateInterestStart) + ", "
                      + "DynamicPayPlanTPOption=  " + SOut.Int((int) payPlan.DynamicPayPlanTPOption) + ", "
                      + "MobileAppDeviceNum    =  " + SOut.Long(payPlan.MobileAppDeviceNum) + ", "
                      + "SecurityHash          = '" + SOut.String(payPlan.SecurityHash) + "', "
                      + "SheetDefNum           =  " + SOut.Long(payPlan.SheetDefNum) + " "
                      + "WHERE PayPlanNum = " + SOut.Long(payPlan.PayPlanNum);
        if (payPlan.Note == null) payPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlan.Note));
        if (payPlan.Signature == null) payPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(payPlan.Signature));
        Db.NonQ(command, paramNote, paramSignature);
    }

    public static bool Update(PayPlan payPlan, PayPlan oldPayPlan)
    {
        var command = "";
        if (payPlan.PatNum != oldPayPlan.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(payPlan.PatNum) + "";
        }

        if (payPlan.Guarantor != oldPayPlan.Guarantor)
        {
            if (command != "") command += ",";
            command += "Guarantor = " + SOut.Long(payPlan.Guarantor) + "";
        }

        if (payPlan.PayPlanDate.Date != oldPayPlan.PayPlanDate.Date)
        {
            if (command != "") command += ",";
            command += "PayPlanDate = " + SOut.Date(payPlan.PayPlanDate) + "";
        }

        if (payPlan.APR != oldPayPlan.APR)
        {
            if (command != "") command += ",";
            command += "APR = " + SOut.Double(payPlan.APR) + "";
        }

        if (payPlan.Note != oldPayPlan.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (payPlan.PlanNum != oldPayPlan.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(payPlan.PlanNum) + "";
        }

        if (payPlan.CompletedAmt != oldPayPlan.CompletedAmt)
        {
            if (command != "") command += ",";
            command += "CompletedAmt = " + SOut.Double(payPlan.CompletedAmt) + "";
        }

        if (payPlan.InsSubNum != oldPayPlan.InsSubNum)
        {
            if (command != "") command += ",";
            command += "InsSubNum = " + SOut.Long(payPlan.InsSubNum) + "";
        }

        if (payPlan.PaySchedule != oldPayPlan.PaySchedule)
        {
            if (command != "") command += ",";
            command += "PaySchedule = " + SOut.Int((int) payPlan.PaySchedule) + "";
        }

        if (payPlan.NumberOfPayments != oldPayPlan.NumberOfPayments)
        {
            if (command != "") command += ",";
            command += "NumberOfPayments = " + SOut.Int(payPlan.NumberOfPayments) + "";
        }

        if (payPlan.PayAmt != oldPayPlan.PayAmt)
        {
            if (command != "") command += ",";
            command += "PayAmt = " + SOut.Double(payPlan.PayAmt) + "";
        }

        if (payPlan.DownPayment != oldPayPlan.DownPayment)
        {
            if (command != "") command += ",";
            command += "DownPayment = " + SOut.Double(payPlan.DownPayment) + "";
        }

        if (payPlan.IsClosed != oldPayPlan.IsClosed)
        {
            if (command != "") command += ",";
            command += "IsClosed = " + SOut.Bool(payPlan.IsClosed) + "";
        }

        if (payPlan.Signature != oldPayPlan.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (payPlan.SigIsTopaz != oldPayPlan.SigIsTopaz)
        {
            if (command != "") command += ",";
            command += "SigIsTopaz = " + SOut.Bool(payPlan.SigIsTopaz) + "";
        }

        if (payPlan.PlanCategory != oldPayPlan.PlanCategory)
        {
            if (command != "") command += ",";
            command += "PlanCategory = " + SOut.Long(payPlan.PlanCategory) + "";
        }

        if (payPlan.IsDynamic != oldPayPlan.IsDynamic)
        {
            if (command != "") command += ",";
            command += "IsDynamic = " + SOut.Bool(payPlan.IsDynamic) + "";
        }

        if (payPlan.ChargeFrequency != oldPayPlan.ChargeFrequency)
        {
            if (command != "") command += ",";
            command += "ChargeFrequency = " + SOut.Int((int) payPlan.ChargeFrequency) + "";
        }

        if (payPlan.DatePayPlanStart.Date != oldPayPlan.DatePayPlanStart.Date)
        {
            if (command != "") command += ",";
            command += "DatePayPlanStart = " + SOut.Date(payPlan.DatePayPlanStart) + "";
        }

        if (payPlan.IsLocked != oldPayPlan.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(payPlan.IsLocked) + "";
        }

        if (payPlan.DateInterestStart.Date != oldPayPlan.DateInterestStart.Date)
        {
            if (command != "") command += ",";
            command += "DateInterestStart = " + SOut.Date(payPlan.DateInterestStart) + "";
        }

        if (payPlan.DynamicPayPlanTPOption != oldPayPlan.DynamicPayPlanTPOption)
        {
            if (command != "") command += ",";
            command += "DynamicPayPlanTPOption = " + SOut.Int((int) payPlan.DynamicPayPlanTPOption) + "";
        }

        if (payPlan.MobileAppDeviceNum != oldPayPlan.MobileAppDeviceNum)
        {
            if (command != "") command += ",";
            command += "MobileAppDeviceNum = " + SOut.Long(payPlan.MobileAppDeviceNum) + "";
        }

        if (payPlan.SecurityHash != oldPayPlan.SecurityHash)
        {
            if (command != "") command += ",";
            command += "SecurityHash = '" + SOut.String(payPlan.SecurityHash) + "'";
        }

        if (payPlan.SheetDefNum != oldPayPlan.SheetDefNum)
        {
            if (command != "") command += ",";
            command += "SheetDefNum = " + SOut.Long(payPlan.SheetDefNum) + "";
        }

        if (command == "") return false;
        if (payPlan.Note == null) payPlan.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payPlan.Note));
        if (payPlan.Signature == null) payPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(payPlan.Signature));
        command = "UPDATE payplan SET " + command
                                        + " WHERE PayPlanNum = " + SOut.Long(payPlan.PayPlanNum);
        Db.NonQ(command, paramNote, paramSignature);
        return true;
    }

    public static bool UpdateComparison(PayPlan payPlan, PayPlan oldPayPlan)
    {
        if (payPlan.PatNum != oldPayPlan.PatNum) return true;
        if (payPlan.Guarantor != oldPayPlan.Guarantor) return true;
        if (payPlan.PayPlanDate.Date != oldPayPlan.PayPlanDate.Date) return true;
        if (payPlan.APR != oldPayPlan.APR) return true;
        if (payPlan.Note != oldPayPlan.Note) return true;
        if (payPlan.PlanNum != oldPayPlan.PlanNum) return true;
        if (payPlan.CompletedAmt != oldPayPlan.CompletedAmt) return true;
        if (payPlan.InsSubNum != oldPayPlan.InsSubNum) return true;
        if (payPlan.PaySchedule != oldPayPlan.PaySchedule) return true;
        if (payPlan.NumberOfPayments != oldPayPlan.NumberOfPayments) return true;
        if (payPlan.PayAmt != oldPayPlan.PayAmt) return true;
        if (payPlan.DownPayment != oldPayPlan.DownPayment) return true;
        if (payPlan.IsClosed != oldPayPlan.IsClosed) return true;
        if (payPlan.Signature != oldPayPlan.Signature) return true;
        if (payPlan.SigIsTopaz != oldPayPlan.SigIsTopaz) return true;
        if (payPlan.PlanCategory != oldPayPlan.PlanCategory) return true;
        if (payPlan.IsDynamic != oldPayPlan.IsDynamic) return true;
        if (payPlan.ChargeFrequency != oldPayPlan.ChargeFrequency) return true;
        if (payPlan.DatePayPlanStart.Date != oldPayPlan.DatePayPlanStart.Date) return true;
        if (payPlan.IsLocked != oldPayPlan.IsLocked) return true;
        if (payPlan.DateInterestStart.Date != oldPayPlan.DateInterestStart.Date) return true;
        if (payPlan.DynamicPayPlanTPOption != oldPayPlan.DynamicPayPlanTPOption) return true;
        if (payPlan.MobileAppDeviceNum != oldPayPlan.MobileAppDeviceNum) return true;
        if (payPlan.SecurityHash != oldPayPlan.SecurityHash) return true;
        if (payPlan.SheetDefNum != oldPayPlan.SheetDefNum) return true;
        return false;
    }

    public static void Delete(long payPlanNum)
    {
        var command = "DELETE FROM payplan "
                      + "WHERE PayPlanNum = " + SOut.Long(payPlanNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayPlanNums)
    {
        if (listPayPlanNums == null || listPayPlanNums.Count == 0) return;
        var command = "DELETE FROM payplan "
                      + "WHERE PayPlanNum IN(" + string.Join(",", listPayPlanNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}