using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PayPlanTemplateCrud
{
    public static List<PayPlanTemplate> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayPlanTemplate> TableToList(DataTable table)
    {
        var retVal = new List<PayPlanTemplate>();
        foreach (DataRow row in table.Rows)
        {
            var payPlanTemplate = new PayPlanTemplate
            {
                PayPlanTemplateNum = SIn.Long(row["PayPlanTemplateNum"].ToString()),
                PayPlanTemplateName = SIn.String(row["PayPlanTemplateName"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                APR = SIn.Double(row["APR"].ToString()),
                InterestDelay = SIn.Int(row["InterestDelay"].ToString()),
                PayAmt = SIn.Double(row["PayAmt"].ToString()),
                NumberOfPayments = SIn.Int(row["NumberOfPayments"].ToString()),
                ChargeFrequency = (PayPlanFrequency) SIn.Int(row["ChargeFrequency"].ToString()),
                DownPayment = SIn.Double(row["DownPayment"].ToString()),
                DynamicPayPlanTPOption = (DynamicPayPlanTPOptions) SIn.Int(row["DynamicPayPlanTPOption"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                SheetDefNum = SIn.Long(row["SheetDefNum"].ToString())
            };
            retVal.Add(payPlanTemplate);
        }

        return retVal;
    }

    public static void Insert(PayPlanTemplate payPlanTemplate)
    {
        var command = "INSERT INTO payplantemplate (";

        command += "PayPlanTemplateName,ClinicNum,APR,InterestDelay,PayAmt,NumberOfPayments,ChargeFrequency,DownPayment,DynamicPayPlanTPOption,Note,IsHidden,SheetDefNum) VALUES(";

        command +=
            "'" + SOut.String(payPlanTemplate.PayPlanTemplateName) + "',"
            + SOut.Long(payPlanTemplate.ClinicNum) + ","
            + SOut.Double(payPlanTemplate.APR) + ","
            + SOut.Int(payPlanTemplate.InterestDelay) + ","
            + SOut.Double(payPlanTemplate.PayAmt) + ","
            + SOut.Int(payPlanTemplate.NumberOfPayments) + ","
            + SOut.Int((int) payPlanTemplate.ChargeFrequency) + ","
            + SOut.Double(payPlanTemplate.DownPayment) + ","
            + SOut.Int((int) payPlanTemplate.DynamicPayPlanTPOption) + ","
            + "'" + SOut.String(payPlanTemplate.Note) + "',"
            + SOut.Bool(payPlanTemplate.IsHidden) + ","
            + SOut.Long(payPlanTemplate.SheetDefNum) + ")";
        {
            payPlanTemplate.PayPlanTemplateNum = Db.NonQ(command, true, "PayPlanTemplateNum", "payPlanTemplate");
        }
    }

    public static void Update(PayPlanTemplate payPlanTemplate)
    {
        var command = "UPDATE payplantemplate SET "
                      + "PayPlanTemplateName   = '" + SOut.String(payPlanTemplate.PayPlanTemplateName) + "', "
                      + "ClinicNum             =  " + SOut.Long(payPlanTemplate.ClinicNum) + ", "
                      + "APR                   =  " + SOut.Double(payPlanTemplate.APR) + ", "
                      + "InterestDelay         =  " + SOut.Int(payPlanTemplate.InterestDelay) + ", "
                      + "PayAmt                =  " + SOut.Double(payPlanTemplate.PayAmt) + ", "
                      + "NumberOfPayments      =  " + SOut.Int(payPlanTemplate.NumberOfPayments) + ", "
                      + "ChargeFrequency       =  " + SOut.Int((int) payPlanTemplate.ChargeFrequency) + ", "
                      + "DownPayment           =  " + SOut.Double(payPlanTemplate.DownPayment) + ", "
                      + "DynamicPayPlanTPOption=  " + SOut.Int((int) payPlanTemplate.DynamicPayPlanTPOption) + ", "
                      + "Note                  = '" + SOut.String(payPlanTemplate.Note) + "', "
                      + "IsHidden              =  " + SOut.Bool(payPlanTemplate.IsHidden) + ", "
                      + "SheetDefNum           =  " + SOut.Long(payPlanTemplate.SheetDefNum) + " "
                      + "WHERE PayPlanTemplateNum = " + SOut.Long(payPlanTemplate.PayPlanTemplateNum);
        Db.NonQ(command);
    }
}