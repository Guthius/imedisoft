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
        PayPlanTemplate payPlanTemplate;
        foreach (DataRow row in table.Rows)
        {
            payPlanTemplate = new PayPlanTemplate();
            payPlanTemplate.PayPlanTemplateNum = SIn.Long(row["PayPlanTemplateNum"].ToString());
            payPlanTemplate.PayPlanTemplateName = SIn.String(row["PayPlanTemplateName"].ToString());
            payPlanTemplate.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            payPlanTemplate.APR = SIn.Double(row["APR"].ToString());
            payPlanTemplate.InterestDelay = SIn.Int(row["InterestDelay"].ToString());
            payPlanTemplate.PayAmt = SIn.Double(row["PayAmt"].ToString());
            payPlanTemplate.NumberOfPayments = SIn.Int(row["NumberOfPayments"].ToString());
            payPlanTemplate.ChargeFrequency = (PayPlanFrequency) SIn.Int(row["ChargeFrequency"].ToString());
            payPlanTemplate.DownPayment = SIn.Double(row["DownPayment"].ToString());
            payPlanTemplate.DynamicPayPlanTPOption = (DynamicPayPlanTPOptions) SIn.Int(row["DynamicPayPlanTPOption"].ToString());
            payPlanTemplate.Note = SIn.String(row["Note"].ToString());
            payPlanTemplate.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            payPlanTemplate.SheetDefNum = SIn.Long(row["SheetDefNum"].ToString());
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