#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PayPlanTemplateCrud
{
    public static PayPlanTemplate SelectOne(long payPlanTemplateNum)
    {
        var command = "SELECT * FROM payplantemplate "
                      + "WHERE PayPlanTemplateNum = " + SOut.Long(payPlanTemplateNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PayPlanTemplate SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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

    public static DataTable ListToTable(List<PayPlanTemplate> listPayPlanTemplates, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayPlanTemplate";
        var table = new DataTable(tableName);
        table.Columns.Add("PayPlanTemplateNum");
        table.Columns.Add("PayPlanTemplateName");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("APR");
        table.Columns.Add("InterestDelay");
        table.Columns.Add("PayAmt");
        table.Columns.Add("NumberOfPayments");
        table.Columns.Add("ChargeFrequency");
        table.Columns.Add("DownPayment");
        table.Columns.Add("DynamicPayPlanTPOption");
        table.Columns.Add("Note");
        table.Columns.Add("IsHidden");
        table.Columns.Add("SheetDefNum");
        foreach (var payPlanTemplate in listPayPlanTemplates)
            table.Rows.Add(SOut.Long(payPlanTemplate.PayPlanTemplateNum), payPlanTemplate.PayPlanTemplateName, SOut.Long(payPlanTemplate.ClinicNum), SOut.Double(payPlanTemplate.APR), SOut.Int(payPlanTemplate.InterestDelay), SOut.Double(payPlanTemplate.PayAmt), SOut.Int(payPlanTemplate.NumberOfPayments), SOut.Int((int) payPlanTemplate.ChargeFrequency), SOut.Double(payPlanTemplate.DownPayment), SOut.Int((int) payPlanTemplate.DynamicPayPlanTPOption), payPlanTemplate.Note, SOut.Bool(payPlanTemplate.IsHidden), SOut.Long(payPlanTemplate.SheetDefNum));
        return table;
    }

    public static long Insert(PayPlanTemplate payPlanTemplate)
    {
        return Insert(payPlanTemplate, false);
    }

    public static long Insert(PayPlanTemplate payPlanTemplate, bool useExistingPK)
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
        return payPlanTemplate.PayPlanTemplateNum;
    }

    public static void InsertMany(List<PayPlanTemplate> listPayPlanTemplates)
    {
        InsertMany(listPayPlanTemplates, false);
    }

    public static void InsertMany(List<PayPlanTemplate> listPayPlanTemplates, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPayPlanTemplates.Count)
        {
            var payPlanTemplate = listPayPlanTemplates[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO payplantemplate (");
                if (useExistingPK) sbCommands.Append("PayPlanTemplateNum,");
                sbCommands.Append("PayPlanTemplateName,ClinicNum,APR,InterestDelay,PayAmt,NumberOfPayments,ChargeFrequency,DownPayment,DynamicPayPlanTPOption,Note,IsHidden,SheetDefNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(payPlanTemplate.PayPlanTemplateNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(payPlanTemplate.PayPlanTemplateName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanTemplate.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlanTemplate.APR));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(payPlanTemplate.InterestDelay));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlanTemplate.PayAmt));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(payPlanTemplate.NumberOfPayments));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payPlanTemplate.ChargeFrequency));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(payPlanTemplate.DownPayment));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) payPlanTemplate.DynamicPayPlanTPOption));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(payPlanTemplate.Note) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(payPlanTemplate.IsHidden));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(payPlanTemplate.SheetDefNum));
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
                if (index == listPayPlanTemplates.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PayPlanTemplate payPlanTemplate)
    {
        return InsertNoCache(payPlanTemplate, false);
    }

    public static long InsertNoCache(PayPlanTemplate payPlanTemplate, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payplantemplate (";
        if (isRandomKeys || useExistingPK) command += "PayPlanTemplateNum,";
        command += "PayPlanTemplateName,ClinicNum,APR,InterestDelay,PayAmt,NumberOfPayments,ChargeFrequency,DownPayment,DynamicPayPlanTPOption,Note,IsHidden,SheetDefNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payPlanTemplate.PayPlanTemplateNum) + ",";
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
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            payPlanTemplate.PayPlanTemplateNum = Db.NonQ(command, true, "PayPlanTemplateNum", "payPlanTemplate");
        return payPlanTemplate.PayPlanTemplateNum;
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

    public static bool Update(PayPlanTemplate payPlanTemplate, PayPlanTemplate oldPayPlanTemplate)
    {
        var command = "";
        if (payPlanTemplate.PayPlanTemplateName != oldPayPlanTemplate.PayPlanTemplateName)
        {
            if (command != "") command += ",";
            command += "PayPlanTemplateName = '" + SOut.String(payPlanTemplate.PayPlanTemplateName) + "'";
        }

        if (payPlanTemplate.ClinicNum != oldPayPlanTemplate.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(payPlanTemplate.ClinicNum) + "";
        }

        if (payPlanTemplate.APR != oldPayPlanTemplate.APR)
        {
            if (command != "") command += ",";
            command += "APR = " + SOut.Double(payPlanTemplate.APR) + "";
        }

        if (payPlanTemplate.InterestDelay != oldPayPlanTemplate.InterestDelay)
        {
            if (command != "") command += ",";
            command += "InterestDelay = " + SOut.Int(payPlanTemplate.InterestDelay) + "";
        }

        if (payPlanTemplate.PayAmt != oldPayPlanTemplate.PayAmt)
        {
            if (command != "") command += ",";
            command += "PayAmt = " + SOut.Double(payPlanTemplate.PayAmt) + "";
        }

        if (payPlanTemplate.NumberOfPayments != oldPayPlanTemplate.NumberOfPayments)
        {
            if (command != "") command += ",";
            command += "NumberOfPayments = " + SOut.Int(payPlanTemplate.NumberOfPayments) + "";
        }

        if (payPlanTemplate.ChargeFrequency != oldPayPlanTemplate.ChargeFrequency)
        {
            if (command != "") command += ",";
            command += "ChargeFrequency = " + SOut.Int((int) payPlanTemplate.ChargeFrequency) + "";
        }

        if (payPlanTemplate.DownPayment != oldPayPlanTemplate.DownPayment)
        {
            if (command != "") command += ",";
            command += "DownPayment = " + SOut.Double(payPlanTemplate.DownPayment) + "";
        }

        if (payPlanTemplate.DynamicPayPlanTPOption != oldPayPlanTemplate.DynamicPayPlanTPOption)
        {
            if (command != "") command += ",";
            command += "DynamicPayPlanTPOption = " + SOut.Int((int) payPlanTemplate.DynamicPayPlanTPOption) + "";
        }

        if (payPlanTemplate.Note != oldPayPlanTemplate.Note)
        {
            if (command != "") command += ",";
            command += "Note = '" + SOut.String(payPlanTemplate.Note) + "'";
        }

        if (payPlanTemplate.IsHidden != oldPayPlanTemplate.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(payPlanTemplate.IsHidden) + "";
        }

        if (payPlanTemplate.SheetDefNum != oldPayPlanTemplate.SheetDefNum)
        {
            if (command != "") command += ",";
            command += "SheetDefNum = " + SOut.Long(payPlanTemplate.SheetDefNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE payplantemplate SET " + command
                                                + " WHERE PayPlanTemplateNum = " + SOut.Long(payPlanTemplate.PayPlanTemplateNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PayPlanTemplate payPlanTemplate, PayPlanTemplate oldPayPlanTemplate)
    {
        if (payPlanTemplate.PayPlanTemplateName != oldPayPlanTemplate.PayPlanTemplateName) return true;
        if (payPlanTemplate.ClinicNum != oldPayPlanTemplate.ClinicNum) return true;
        if (payPlanTemplate.APR != oldPayPlanTemplate.APR) return true;
        if (payPlanTemplate.InterestDelay != oldPayPlanTemplate.InterestDelay) return true;
        if (payPlanTemplate.PayAmt != oldPayPlanTemplate.PayAmt) return true;
        if (payPlanTemplate.NumberOfPayments != oldPayPlanTemplate.NumberOfPayments) return true;
        if (payPlanTemplate.ChargeFrequency != oldPayPlanTemplate.ChargeFrequency) return true;
        if (payPlanTemplate.DownPayment != oldPayPlanTemplate.DownPayment) return true;
        if (payPlanTemplate.DynamicPayPlanTPOption != oldPayPlanTemplate.DynamicPayPlanTPOption) return true;
        if (payPlanTemplate.Note != oldPayPlanTemplate.Note) return true;
        if (payPlanTemplate.IsHidden != oldPayPlanTemplate.IsHidden) return true;
        if (payPlanTemplate.SheetDefNum != oldPayPlanTemplate.SheetDefNum) return true;
        return false;
    }

    public static void Delete(long payPlanTemplateNum)
    {
        var command = "DELETE FROM payplantemplate "
                      + "WHERE PayPlanTemplateNum = " + SOut.Long(payPlanTemplateNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayPlanTemplateNums)
    {
        if (listPayPlanTemplateNums == null || listPayPlanTemplateNums.Count == 0) return;
        var command = "DELETE FROM payplantemplate "
                      + "WHERE PayPlanTemplateNum IN(" + string.Join(",", listPayPlanTemplateNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}