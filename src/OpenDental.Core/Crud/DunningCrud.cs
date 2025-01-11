#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DunningCrud
{
    public static Dunning SelectOne(long dunningNum)
    {
        var command = "SELECT * FROM dunning "
                      + "WHERE DunningNum = " + SOut.Long(dunningNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Dunning SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Dunning> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Dunning> TableToList(DataTable table)
    {
        var retVal = new List<Dunning>();
        Dunning dunning;
        foreach (DataRow row in table.Rows)
        {
            dunning = new Dunning();
            dunning.DunningNum = SIn.Long(row["DunningNum"].ToString());
            dunning.DunMessage = SIn.String(row["DunMessage"].ToString());
            dunning.BillingType = SIn.Long(row["BillingType"].ToString());
            dunning.AgeAccount = SIn.Byte(row["AgeAccount"].ToString());
            dunning.InsIsPending = (YN) SIn.Int(row["InsIsPending"].ToString());
            dunning.MessageBold = SIn.String(row["MessageBold"].ToString());
            dunning.EmailSubject = SIn.String(row["EmailSubject"].ToString());
            dunning.EmailBody = SIn.String(row["EmailBody"].ToString());
            dunning.DaysInAdvance = SIn.Int(row["DaysInAdvance"].ToString());
            dunning.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            dunning.IsSuperFamily = SIn.Bool(row["IsSuperFamily"].ToString());
            retVal.Add(dunning);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Dunning> listDunnings, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Dunning";
        var table = new DataTable(tableName);
        table.Columns.Add("DunningNum");
        table.Columns.Add("DunMessage");
        table.Columns.Add("BillingType");
        table.Columns.Add("AgeAccount");
        table.Columns.Add("InsIsPending");
        table.Columns.Add("MessageBold");
        table.Columns.Add("EmailSubject");
        table.Columns.Add("EmailBody");
        table.Columns.Add("DaysInAdvance");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("IsSuperFamily");
        foreach (var dunning in listDunnings)
            table.Rows.Add(SOut.Long(dunning.DunningNum), dunning.DunMessage, SOut.Long(dunning.BillingType), SOut.Byte(dunning.AgeAccount), SOut.Int((int) dunning.InsIsPending), dunning.MessageBold, dunning.EmailSubject, dunning.EmailBody, SOut.Int(dunning.DaysInAdvance), SOut.Long(dunning.ClinicNum), SOut.Bool(dunning.IsSuperFamily));
        return table;
    }

    public static long Insert(Dunning dunning)
    {
        return Insert(dunning, false);
    }

    public static long Insert(Dunning dunning, bool useExistingPK)
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
        var paramDunMessage = new OdSqlParameter("paramDunMessage", OdDbType.Text, SOut.StringParam(dunning.DunMessage));
        if (dunning.MessageBold == null) dunning.MessageBold = "";
        var paramMessageBold = new OdSqlParameter("paramMessageBold", OdDbType.Text, SOut.StringParam(dunning.MessageBold));
        if (dunning.EmailBody == null) dunning.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(dunning.EmailBody));
        {
            dunning.DunningNum = Db.NonQ(command, true, "DunningNum", "dunning", paramDunMessage, paramMessageBold, paramEmailBody);
        }
        return dunning.DunningNum;
    }

    public static long InsertNoCache(Dunning dunning)
    {
        return InsertNoCache(dunning, false);
    }

    public static long InsertNoCache(Dunning dunning, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO dunning (";
        if (isRandomKeys || useExistingPK) command += "DunningNum,";
        command += "DunMessage,BillingType,AgeAccount,InsIsPending,MessageBold,EmailSubject,EmailBody,DaysInAdvance,ClinicNum,IsSuperFamily) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(dunning.DunningNum) + ",";
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
        var paramDunMessage = new OdSqlParameter("paramDunMessage", OdDbType.Text, SOut.StringParam(dunning.DunMessage));
        if (dunning.MessageBold == null) dunning.MessageBold = "";
        var paramMessageBold = new OdSqlParameter("paramMessageBold", OdDbType.Text, SOut.StringParam(dunning.MessageBold));
        if (dunning.EmailBody == null) dunning.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(dunning.EmailBody));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDunMessage, paramMessageBold, paramEmailBody);
        else
            dunning.DunningNum = Db.NonQ(command, true, "DunningNum", "dunning", paramDunMessage, paramMessageBold, paramEmailBody);
        return dunning.DunningNum;
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
        var paramDunMessage = new OdSqlParameter("paramDunMessage", OdDbType.Text, SOut.StringParam(dunning.DunMessage));
        if (dunning.MessageBold == null) dunning.MessageBold = "";
        var paramMessageBold = new OdSqlParameter("paramMessageBold", OdDbType.Text, SOut.StringParam(dunning.MessageBold));
        if (dunning.EmailBody == null) dunning.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(dunning.EmailBody));
        Db.NonQ(command, paramDunMessage, paramMessageBold, paramEmailBody);
    }

    public static bool Update(Dunning dunning, Dunning oldDunning)
    {
        var command = "";
        if (dunning.DunMessage != oldDunning.DunMessage)
        {
            if (command != "") command += ",";
            command += "DunMessage = " + DbHelper.ParamChar + "paramDunMessage";
        }

        if (dunning.BillingType != oldDunning.BillingType)
        {
            if (command != "") command += ",";
            command += "BillingType = " + SOut.Long(dunning.BillingType) + "";
        }

        if (dunning.AgeAccount != oldDunning.AgeAccount)
        {
            if (command != "") command += ",";
            command += "AgeAccount = " + SOut.Byte(dunning.AgeAccount) + "";
        }

        if (dunning.InsIsPending != oldDunning.InsIsPending)
        {
            if (command != "") command += ",";
            command += "InsIsPending = " + SOut.Int((int) dunning.InsIsPending) + "";
        }

        if (dunning.MessageBold != oldDunning.MessageBold)
        {
            if (command != "") command += ",";
            command += "MessageBold = " + DbHelper.ParamChar + "paramMessageBold";
        }

        if (dunning.EmailSubject != oldDunning.EmailSubject)
        {
            if (command != "") command += ",";
            command += "EmailSubject = '" + SOut.String(dunning.EmailSubject) + "'";
        }

        if (dunning.EmailBody != oldDunning.EmailBody)
        {
            if (command != "") command += ",";
            command += "EmailBody = " + DbHelper.ParamChar + "paramEmailBody";
        }

        if (dunning.DaysInAdvance != oldDunning.DaysInAdvance)
        {
            if (command != "") command += ",";
            command += "DaysInAdvance = " + SOut.Int(dunning.DaysInAdvance) + "";
        }

        if (dunning.ClinicNum != oldDunning.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(dunning.ClinicNum) + "";
        }

        if (dunning.IsSuperFamily != oldDunning.IsSuperFamily)
        {
            if (command != "") command += ",";
            command += "IsSuperFamily = " + SOut.Bool(dunning.IsSuperFamily) + "";
        }

        if (command == "") return false;
        if (dunning.DunMessage == null) dunning.DunMessage = "";
        var paramDunMessage = new OdSqlParameter("paramDunMessage", OdDbType.Text, SOut.StringParam(dunning.DunMessage));
        if (dunning.MessageBold == null) dunning.MessageBold = "";
        var paramMessageBold = new OdSqlParameter("paramMessageBold", OdDbType.Text, SOut.StringParam(dunning.MessageBold));
        if (dunning.EmailBody == null) dunning.EmailBody = "";
        var paramEmailBody = new OdSqlParameter("paramEmailBody", OdDbType.Text, SOut.StringParam(dunning.EmailBody));
        command = "UPDATE dunning SET " + command
                                        + " WHERE DunningNum = " + SOut.Long(dunning.DunningNum);
        Db.NonQ(command, paramDunMessage, paramMessageBold, paramEmailBody);
        return true;
    }

    public static bool UpdateComparison(Dunning dunning, Dunning oldDunning)
    {
        if (dunning.DunMessage != oldDunning.DunMessage) return true;
        if (dunning.BillingType != oldDunning.BillingType) return true;
        if (dunning.AgeAccount != oldDunning.AgeAccount) return true;
        if (dunning.InsIsPending != oldDunning.InsIsPending) return true;
        if (dunning.MessageBold != oldDunning.MessageBold) return true;
        if (dunning.EmailSubject != oldDunning.EmailSubject) return true;
        if (dunning.EmailBody != oldDunning.EmailBody) return true;
        if (dunning.DaysInAdvance != oldDunning.DaysInAdvance) return true;
        if (dunning.ClinicNum != oldDunning.ClinicNum) return true;
        if (dunning.IsSuperFamily != oldDunning.IsSuperFamily) return true;
        return false;
    }

    public static void Delete(long dunningNum)
    {
        var command = "DELETE FROM dunning "
                      + "WHERE DunningNum = " + SOut.Long(dunningNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDunningNums)
    {
        if (listDunningNums == null || listDunningNums.Count == 0) return;
        var command = "DELETE FROM dunning "
                      + "WHERE DunningNum IN(" + string.Join(",", listDunningNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}