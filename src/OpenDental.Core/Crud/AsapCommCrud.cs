using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AsapCommCrud
{
    public static List<AsapComm> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AsapComm> TableToList(DataTable table)
    {
        var retVal = new List<AsapComm>();
        AsapComm asapComm;
        foreach (DataRow row in table.Rows)
        {
            asapComm = new AsapComm();
            asapComm.AsapCommNum = SIn.Long(row["AsapCommNum"].ToString());
            asapComm.FKey = SIn.Long(row["FKey"].ToString());
            asapComm.FKeyType = (AsapCommFKeyType) SIn.Int(row["FKeyType"].ToString());
            asapComm.ScheduleNum = SIn.Long(row["ScheduleNum"].ToString());
            asapComm.PatNum = SIn.Long(row["PatNum"].ToString());
            asapComm.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            asapComm.ShortGUID = SIn.String(row["ShortGUID"].ToString());
            asapComm.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            asapComm.DateTimeExpire = SIn.DateTime(row["DateTimeExpire"].ToString());
            asapComm.DateTimeSmsScheduled = SIn.DateTime(row["DateTimeSmsScheduled"].ToString());
            asapComm.SmsSendStatus = (AutoCommStatus) SIn.Int(row["SmsSendStatus"].ToString());
            asapComm.EmailSendStatus = (AutoCommStatus) SIn.Int(row["EmailSendStatus"].ToString());
            asapComm.DateTimeSmsSent = SIn.DateTime(row["DateTimeSmsSent"].ToString());
            asapComm.DateTimeEmailSent = SIn.DateTime(row["DateTimeEmailSent"].ToString());
            asapComm.EmailMessageNum = SIn.Long(row["EmailMessageNum"].ToString());
            asapComm.ResponseStatus = (AsapRSVPStatus) SIn.Int(row["ResponseStatus"].ToString());
            asapComm.DateTimeOrig = SIn.DateTime(row["DateTimeOrig"].ToString());
            asapComm.TemplateText = SIn.String(row["TemplateText"].ToString());
            asapComm.TemplateEmail = SIn.String(row["TemplateEmail"].ToString());
            asapComm.TemplateEmailSubj = SIn.String(row["TemplateEmailSubj"].ToString());
            asapComm.Note = SIn.String(row["Note"].ToString());
            asapComm.GuidMessageToMobile = SIn.String(row["GuidMessageToMobile"].ToString());
            var emailTemplateType = row["EmailTemplateType"].ToString();
            if (emailTemplateType == "")
                asapComm.EmailTemplateType = 0;
            else
                try
                {
                    asapComm.EmailTemplateType = (EmailType) Enum.Parse(typeof(EmailType), emailTemplateType);
                }
                catch
                {
                    asapComm.EmailTemplateType = 0;
                }

            retVal.Add(asapComm);
        }

        return retVal;
    }

    public static long Insert(AsapComm asapComm)
    {
        var command = "INSERT INTO asapcomm (";

        command += "FKey,FKeyType,ScheduleNum,PatNum,ClinicNum,ShortGUID,DateTimeEntry,DateTimeExpire,DateTimeSmsScheduled,SmsSendStatus,EmailSendStatus,DateTimeSmsSent,DateTimeEmailSent,EmailMessageNum,ResponseStatus,DateTimeOrig,TemplateText,TemplateEmail,TemplateEmailSubj,Note,GuidMessageToMobile,EmailTemplateType) VALUES(";

        command +=
            SOut.Long(asapComm.FKey) + ","
                                     + SOut.Int((int) asapComm.FKeyType) + ","
                                     + SOut.Long(asapComm.ScheduleNum) + ","
                                     + SOut.Long(asapComm.PatNum) + ","
                                     + SOut.Long(asapComm.ClinicNum) + ","
                                     + "'" + SOut.String(asapComm.ShortGUID) + "',"
                                     + DbHelper.Now() + ","
                                     + SOut.DateT(asapComm.DateTimeExpire) + ","
                                     + SOut.DateT(asapComm.DateTimeSmsScheduled) + ","
                                     + SOut.Int((int) asapComm.SmsSendStatus) + ","
                                     + SOut.Int((int) asapComm.EmailSendStatus) + ","
                                     + SOut.DateT(asapComm.DateTimeSmsSent) + ","
                                     + SOut.DateT(asapComm.DateTimeEmailSent) + ","
                                     + SOut.Long(asapComm.EmailMessageNum) + ","
                                     + SOut.Int((int) asapComm.ResponseStatus) + ","
                                     + SOut.DateT(asapComm.DateTimeOrig) + ","
                                     + DbHelper.ParamChar + "paramTemplateText,"
                                     + DbHelper.ParamChar + "paramTemplateEmail,"
                                     + "'" + SOut.String(asapComm.TemplateEmailSubj) + "',"
                                     + DbHelper.ParamChar + "paramNote,"
                                     + DbHelper.ParamChar + "paramGuidMessageToMobile,"
                                     + "'" + SOut.String(asapComm.EmailTemplateType.ToString()) + "')";
        if (asapComm.TemplateText == null) asapComm.TemplateText = "";
        var paramTemplateText = new OdSqlParameter("paramTemplateText", OdDbType.Text, SOut.StringParam(asapComm.TemplateText));
        if (asapComm.TemplateEmail == null) asapComm.TemplateEmail = "";
        var paramTemplateEmail = new OdSqlParameter("paramTemplateEmail", OdDbType.Text, SOut.StringParam(asapComm.TemplateEmail));
        if (asapComm.Note == null) asapComm.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(asapComm.Note));
        if (asapComm.GuidMessageToMobile == null) asapComm.GuidMessageToMobile = "";
        var paramGuidMessageToMobile = new OdSqlParameter("paramGuidMessageToMobile", OdDbType.Text, SOut.StringParam(asapComm.GuidMessageToMobile));
        {
            asapComm.AsapCommNum = Db.NonQ(command, true, "AsapCommNum", "asapComm", paramTemplateText, paramTemplateEmail, paramNote, paramGuidMessageToMobile);
        }
        return asapComm.AsapCommNum;
    }

    public static void InsertMany(List<AsapComm> listAsapComms)
    {
        InsertMany(listAsapComms, false);
    }

    public static void InsertMany(List<AsapComm> listAsapComms, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listAsapComms.Count)
        {
            var asapComm = listAsapComms[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO asapcomm (");
                if (useExistingPK) sbCommands.Append("AsapCommNum,");
                sbCommands.Append("FKey,FKeyType,ScheduleNum,PatNum,ClinicNum,ShortGUID,DateTimeEntry,DateTimeExpire,DateTimeSmsScheduled,SmsSendStatus,EmailSendStatus,DateTimeSmsSent,DateTimeEmailSent,EmailMessageNum,ResponseStatus,DateTimeOrig,TemplateText,TemplateEmail,TemplateEmailSubj,Note,GuidMessageToMobile,EmailTemplateType) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(asapComm.AsapCommNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(asapComm.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) asapComm.FKeyType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(asapComm.ScheduleNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(asapComm.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(asapComm.ClinicNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(asapComm.ShortGUID) + "'");
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(asapComm.DateTimeExpire));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(asapComm.DateTimeSmsScheduled));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) asapComm.SmsSendStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) asapComm.EmailSendStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(asapComm.DateTimeSmsSent));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(asapComm.DateTimeEmailSent));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(asapComm.EmailMessageNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) asapComm.ResponseStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(asapComm.DateTimeOrig));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(asapComm.TemplateText) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(asapComm.TemplateEmail) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(asapComm.TemplateEmailSubj) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(asapComm.Note) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(asapComm.GuidMessageToMobile) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(asapComm.EmailTemplateType.ToString()) + "'");
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
                if (index == listAsapComms.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}