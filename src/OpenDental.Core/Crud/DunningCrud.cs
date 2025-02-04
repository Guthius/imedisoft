using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DunningCrud
{
    public static List<Dunning> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Dunning> TableToList(DataTable table)
    {
        var retVal = new List<Dunning>();
        foreach (DataRow row in table.Rows)
        {
            var dunning = new Dunning
            {
                DunningNum = SIn.Long(row["DunningNum"].ToString()),
                DunMessage = SIn.String(row["DunMessage"].ToString()),
                BillingType = SIn.Long(row["BillingType"].ToString()),
                AgeAccount = SIn.Byte(row["AgeAccount"].ToString()),
                InsIsPending = (YN) SIn.Int(row["InsIsPending"].ToString()),
                MessageBold = SIn.String(row["MessageBold"].ToString()),
                EmailSubject = SIn.String(row["EmailSubject"].ToString()),
                EmailBody = SIn.String(row["EmailBody"].ToString()),
                DaysInAdvance = SIn.Int(row["DaysInAdvance"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                IsSuperFamily = SIn.Bool(row["IsSuperFamily"].ToString())
            };
            retVal.Add(dunning);
        }

        return retVal;
    }

    public static void Insert(Dunning dunning)
    {
        var command = "INSERT INTO dunning (";

        command += "DunMessage,BillingType,AgeAccount,InsIsPending,MessageBold,EmailSubject,EmailBody,DaysInAdvance,ClinicNum,IsSuperFamily) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDunMessage,"
                               + SOut.Long(dunning.BillingType) + ","
                               + SOut.Byte(dunning.AgeAccount) + ","
                               + SOut.Int((int) dunning.InsIsPending) + ","
                               + DbHelper.ParamChar + "paramMessageBold,"
                               + "'" + SOut.String(dunning.EmailSubject) + "',"
                               + DbHelper.ParamChar + "paramEmailBody,"
                               + SOut.Int(dunning.DaysInAdvance) + ","
                               + SOut.Long(dunning.ClinicNum) + ","
                               + SOut.Bool(dunning.IsSuperFamily) + ")";
        if (dunning.DunMessage == null) dunning.DunMessage = "";
        var paramDunMessage = new OdSqlParameter("paramDunMessage", SOut.StringParam(dunning.DunMessage));
        if (dunning.MessageBold == null) dunning.MessageBold = "";
        var paramMessageBold = new OdSqlParameter("paramMessageBold", SOut.StringParam(dunning.MessageBold));
        if (dunning.EmailBody == null) dunning.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", SOut.StringParam(dunning.EmailBody));
        {
            dunning.DunningNum = Db.NonQ(command, true, "DunningNum", "dunning", paramDunMessage, paramMessageBold, paramEmailBody);
        }
    }

    public static void Update(Dunning dunning)
    {
        var command = "UPDATE dunning SET "
                      + "DunMessage   =  " + DbHelper.ParamChar + "paramDunMessage, "
                      + "BillingType  =  " + SOut.Long(dunning.BillingType) + ", "
                      + "AgeAccount   =  " + SOut.Byte(dunning.AgeAccount) + ", "
                      + "InsIsPending =  " + SOut.Int((int) dunning.InsIsPending) + ", "
                      + "MessageBold  =  " + DbHelper.ParamChar + "paramMessageBold, "
                      + "EmailSubject = '" + SOut.String(dunning.EmailSubject) + "', "
                      + "EmailBody    =  " + DbHelper.ParamChar + "paramEmailBody, "
                      + "DaysInAdvance=  " + SOut.Int(dunning.DaysInAdvance) + ", "
                      + "ClinicNum    =  " + SOut.Long(dunning.ClinicNum) + ", "
                      + "IsSuperFamily=  " + SOut.Bool(dunning.IsSuperFamily) + " "
                      + "WHERE DunningNum = " + SOut.Long(dunning.DunningNum);
        if (dunning.DunMessage == null) dunning.DunMessage = "";
        var paramDunMessage = new OdSqlParameter("paramDunMessage", SOut.StringParam(dunning.DunMessage));
        if (dunning.MessageBold == null) dunning.MessageBold = "";
        var paramMessageBold = new OdSqlParameter("paramMessageBold", SOut.StringParam(dunning.MessageBold));
        if (dunning.EmailBody == null) dunning.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", SOut.StringParam(dunning.EmailBody));
        Db.NonQ(command, paramDunMessage, paramMessageBold, paramEmailBody);
    }
}