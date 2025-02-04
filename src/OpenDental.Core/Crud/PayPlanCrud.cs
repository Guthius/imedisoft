using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<PayPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayPlan> TableToList(DataTable table)
    {
        var retVal = new List<PayPlan>();
        foreach (DataRow row in table.Rows)
        {
            var payPlan = new PayPlan
            {
                PayPlanNum = SIn.Long(row["PayPlanNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                Guarantor = SIn.Long(row["Guarantor"].ToString()),
                PayPlanDate = SIn.Date(row["PayPlanDate"].ToString()),
                APR = SIn.Double(row["APR"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                PlanNum = SIn.Long(row["PlanNum"].ToString()),
                CompletedAmt = SIn.Double(row["CompletedAmt"].ToString()),
                InsSubNum = SIn.Long(row["InsSubNum"].ToString()),
                PaySchedule = (PaymentSchedule) SIn.Int(row["PaySchedule"].ToString()),
                NumberOfPayments = SIn.Int(row["NumberOfPayments"].ToString()),
                PayAmt = SIn.Double(row["PayAmt"].ToString()),
                DownPayment = SIn.Double(row["DownPayment"].ToString()),
                IsClosed = SIn.Bool(row["IsClosed"].ToString()),
                Signature = SIn.String(row["Signature"].ToString()),
                SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString()),
                PlanCategory = SIn.Long(row["PlanCategory"].ToString()),
                IsDynamic = SIn.Bool(row["IsDynamic"].ToString()),
                ChargeFrequency = (PayPlanFrequency) SIn.Int(row["ChargeFrequency"].ToString()),
                DatePayPlanStart = SIn.Date(row["DatePayPlanStart"].ToString()),
                IsLocked = SIn.Bool(row["IsLocked"].ToString()),
                DateInterestStart = SIn.Date(row["DateInterestStart"].ToString()),
                DynamicPayPlanTPOption = (DynamicPayPlanTPOptions) SIn.Int(row["DynamicPayPlanTPOption"].ToString()),
                MobileAppDeviceNum = SIn.Long(row["MobileAppDeviceNum"].ToString()),
                SecurityHash = SIn.String(row["SecurityHash"].ToString()),
                SheetDefNum = SIn.Long(row["SheetDefNum"].ToString())
            };
            retVal.Add(payPlan);
        }

        return retVal;
    }

    public static long Insert(PayPlan payPlan)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(payPlan.Note));
        if (payPlan.Signature == null) payPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(payPlan.Signature));
        {
            payPlan.PayPlanNum = Db.NonQ(command, true, "PayPlanNum", "payPlan", paramNote, paramSignature);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(payPlan.Note));
        if (payPlan.Signature == null) payPlan.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(payPlan.Signature));
        Db.NonQ(command, paramNote, paramSignature);
    }
}