using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AutomationCrud
{
    public static List<Automation> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Automation> TableToList(DataTable table)
    {
        var retVal = new List<Automation>();
        foreach (DataRow row in table.Rows)
        {
            var automation = new Automation
            {
                AutomationNum = SIn.Long(row["AutomationNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                Autotrigger = (EnumAutomationTrigger) SIn.Int(row["Autotrigger"].ToString()),
                ProcCodes = SIn.String(row["ProcCodes"].ToString()),
                AutoAction = (AutomationAction) SIn.Int(row["AutoAction"].ToString()),
                SheetDefNum = SIn.Long(row["SheetDefNum"].ToString()),
                CommType = SIn.Long(row["CommType"].ToString()),
                MessageContent = SIn.String(row["MessageContent"].ToString()),
                AptStatus = (ApptStatus) SIn.Int(row["AptStatus"].ToString()),
                AppointmentTypeNum = SIn.Long(row["AppointmentTypeNum"].ToString()),
                PatStatus = (PatientStatus) SIn.Int(row["PatStatus"].ToString())
            };
            retVal.Add(automation);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Automation> listAutomations, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Automation";
        var table = new DataTable(tableName);
        table.Columns.Add("AutomationNum");
        table.Columns.Add("Description");
        table.Columns.Add("Autotrigger");
        table.Columns.Add("ProcCodes");
        table.Columns.Add("AutoAction");
        table.Columns.Add("SheetDefNum");
        table.Columns.Add("CommType");
        table.Columns.Add("MessageContent");
        table.Columns.Add("AptStatus");
        table.Columns.Add("AppointmentTypeNum");
        table.Columns.Add("PatStatus");
        foreach (var automation in listAutomations)
            table.Rows.Add(SOut.Long(automation.AutomationNum), automation.Description, SOut.Int((int) automation.Autotrigger), automation.ProcCodes, SOut.Int((int) automation.AutoAction), SOut.Long(automation.SheetDefNum), SOut.Long(automation.CommType), automation.MessageContent, SOut.Int((int) automation.AptStatus), SOut.Long(automation.AppointmentTypeNum), SOut.Int((int) automation.PatStatus));
        return table;
    }

    public static void Insert(Automation automation)
    {
        var command = "INSERT INTO automation (";

        command += "Description,Autotrigger,ProcCodes,AutoAction,SheetDefNum,CommType,MessageContent,AptStatus,AppointmentTypeNum,PatStatus) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + SOut.Int((int) automation.Autotrigger) + ","
                               + DbHelper.ParamChar + "paramProcCodes,"
                               + SOut.Int((int) automation.AutoAction) + ","
                               + SOut.Long(automation.SheetDefNum) + ","
                               + SOut.Long(automation.CommType) + ","
                               + DbHelper.ParamChar + "paramMessageContent,"
                               + SOut.Int((int) automation.AptStatus) + ","
                               + SOut.Long(automation.AppointmentTypeNum) + ","
                               + SOut.Int((int) automation.PatStatus) + ")";
        if (automation.Description == null) automation.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(automation.Description));
        if (automation.ProcCodes == null) automation.ProcCodes = "";
        var paramProcCodes = new OdSqlParameter("paramProcCodes", SOut.StringParam(automation.ProcCodes));
        if (automation.MessageContent == null) automation.MessageContent = "";
        var paramMessageContent = new OdSqlParameter("paramMessageContent", SOut.StringParam(automation.MessageContent));
        {
            automation.AutomationNum = Db.NonQ(command, true, "AutomationNum", "automation", paramDescription, paramProcCodes, paramMessageContent);
        }
    }

    public static void Update(Automation automation)
    {
        var command = "UPDATE automation SET "
                      + "Description       =  " + DbHelper.ParamChar + "paramDescription, "
                      + "Autotrigger       =  " + SOut.Int((int) automation.Autotrigger) + ", "
                      + "ProcCodes         =  " + DbHelper.ParamChar + "paramProcCodes, "
                      + "AutoAction        =  " + SOut.Int((int) automation.AutoAction) + ", "
                      + "SheetDefNum       =  " + SOut.Long(automation.SheetDefNum) + ", "
                      + "CommType          =  " + SOut.Long(automation.CommType) + ", "
                      + "MessageContent    =  " + DbHelper.ParamChar + "paramMessageContent, "
                      + "AptStatus         =  " + SOut.Int((int) automation.AptStatus) + ", "
                      + "AppointmentTypeNum=  " + SOut.Long(automation.AppointmentTypeNum) + ", "
                      + "PatStatus         =  " + SOut.Int((int) automation.PatStatus) + " "
                      + "WHERE AutomationNum = " + SOut.Long(automation.AutomationNum);
        if (automation.Description == null) automation.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(automation.Description));
        if (automation.ProcCodes == null) automation.ProcCodes = "";
        var paramProcCodes = new OdSqlParameter("paramProcCodes", SOut.StringParam(automation.ProcCodes));
        if (automation.MessageContent == null) automation.MessageContent = "";
        var paramMessageContent = new OdSqlParameter("paramMessageContent", SOut.StringParam(automation.MessageContent));
        Db.NonQ(command, paramDescription, paramProcCodes, paramMessageContent);
    }
}