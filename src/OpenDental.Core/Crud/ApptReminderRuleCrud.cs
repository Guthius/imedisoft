using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ApptReminderRuleCrud
{
    public static List<ApptReminderRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApptReminderRule> TableToList(DataTable table)
    {
        var retVal = new List<ApptReminderRule>();
        foreach (DataRow row in table.Rows)
        {
            var apptReminderRule = new ApptReminderRule
            {
                ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString()),
                TypeCur = (ApptReminderType) SIn.Int(row["TypeCur"].ToString()),
                TSPrior = TimeSpan.FromTicks(SIn.Long(row["TSPrior"].ToString())),
                SendOrder = SIn.String(row["SendOrder"].ToString()),
                IsSendAll = SIn.Bool(row["IsSendAll"].ToString()),
                TemplateSMS = SIn.String(row["TemplateSMS"].ToString()),
                TemplateEmailSubject = SIn.String(row["TemplateEmailSubject"].ToString()),
                TemplateEmail = SIn.String(row["TemplateEmail"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                TemplateSMSAggShared = SIn.String(row["TemplateSMSAggShared"].ToString()),
                TemplateSMSAggPerAppt = SIn.String(row["TemplateSMSAggPerAppt"].ToString()),
                TemplateEmailSubjAggShared = SIn.String(row["TemplateEmailSubjAggShared"].ToString()),
                TemplateEmailAggShared = SIn.String(row["TemplateEmailAggShared"].ToString()),
                TemplateEmailAggPerAppt = SIn.String(row["TemplateEmailAggPerAppt"].ToString()),
                DoNotSendWithin = TimeSpan.FromTicks(SIn.Long(row["DoNotSendWithin"].ToString())),
                IsEnabled = SIn.Bool(row["IsEnabled"].ToString()),
                TemplateAutoReply = SIn.String(row["TemplateAutoReply"].ToString()),
                TemplateAutoReplyAgg = SIn.String(row["TemplateAutoReplyAgg"].ToString()),
                TemplateFailureAutoReply = SIn.String(row["TemplateFailureAutoReply"].ToString()),
                IsAutoReplyEnabled = SIn.Bool(row["IsAutoReplyEnabled"].ToString()),
                Language = SIn.String(row["Language"].ToString())
            };
            var emailTemplateType = row["EmailTemplateType"].ToString();
            if (emailTemplateType == "")
                apptReminderRule.EmailTemplateType = 0;
            else
                try
                {
                    apptReminderRule.EmailTemplateType = (EmailType) Enum.Parse(typeof(EmailType), emailTemplateType);
                }
                catch
                {
                    apptReminderRule.EmailTemplateType = 0;
                }

            var aggEmailTemplateType = row["AggEmailTemplateType"].ToString();
            if (aggEmailTemplateType == "")
                apptReminderRule.AggEmailTemplateType = 0;
            else
                try
                {
                    apptReminderRule.AggEmailTemplateType = (EmailType) Enum.Parse(typeof(EmailType), aggEmailTemplateType);
                }
                catch
                {
                    apptReminderRule.AggEmailTemplateType = 0;
                }

            apptReminderRule.TemplateComeInMessage = SIn.String(row["TemplateComeInMessage"].ToString());
            apptReminderRule.IsSendForMinorsBirthday = SIn.Bool(row["IsSendForMinorsBirthday"].ToString());
            apptReminderRule.EmailHostingTemplateNum = SIn.Long(row["EmailHostingTemplateNum"].ToString());
            apptReminderRule.MinorAge = SIn.Int(row["MinorAge"].ToString());
            apptReminderRule.SendMultipleInvites = (SendMultipleInvites) SIn.Int(row["SendMultipleInvites"].ToString());
            apptReminderRule.TimeSpanMultipleInvites = TimeSpan.FromTicks(SIn.Long(row["TimeSpanMultipleInvites"].ToString()));
            retVal.Add(apptReminderRule);
        }

        return retVal;
    }

    public static void Insert(ApptReminderRule apptReminderRule)
    {
        var command = "INSERT INTO apptreminderrule (";

        command += "TypeCur,TSPrior,SendOrder,IsSendAll,TemplateSMS,TemplateEmailSubject,TemplateEmail,ClinicNum,TemplateSMSAggShared,TemplateSMSAggPerAppt,TemplateEmailSubjAggShared,TemplateEmailAggShared,TemplateEmailAggPerAppt,DoNotSendWithin,IsEnabled,TemplateAutoReply,TemplateAutoReplyAgg,TemplateFailureAutoReply,IsAutoReplyEnabled,Language,EmailTemplateType,AggEmailTemplateType,TemplateComeInMessage,IsSendForMinorsBirthday,EmailHostingTemplateNum,MinorAge,SendMultipleInvites,TimeSpanMultipleInvites) VALUES(";

        command +=
            SOut.Int((int) apptReminderRule.TypeCur) + ","
                                                     + "'" + SOut.Long(apptReminderRule.TSPrior.Ticks) + "',"
                                                     + "'" + SOut.String(apptReminderRule.SendOrder) + "',"
                                                     + SOut.Bool(apptReminderRule.IsSendAll) + ","
                                                     + DbHelper.ParamChar + "paramTemplateSMS,"
                                                     + DbHelper.ParamChar + "paramTemplateEmailSubject,"
                                                     + DbHelper.ParamChar + "paramTemplateEmail,"
                                                     + SOut.Long(apptReminderRule.ClinicNum) + ","
                                                     + DbHelper.ParamChar + "paramTemplateSMSAggShared,"
                                                     + DbHelper.ParamChar + "paramTemplateSMSAggPerAppt,"
                                                     + DbHelper.ParamChar + "paramTemplateEmailSubjAggShared,"
                                                     + DbHelper.ParamChar + "paramTemplateEmailAggShared,"
                                                     + DbHelper.ParamChar + "paramTemplateEmailAggPerAppt,"
                                                     + "'" + SOut.Long(apptReminderRule.DoNotSendWithin.Ticks) + "',"
                                                     + SOut.Bool(apptReminderRule.IsEnabled) + ","
                                                     + DbHelper.ParamChar + "paramTemplateAutoReply,"
                                                     + DbHelper.ParamChar + "paramTemplateAutoReplyAgg,"
                                                     + DbHelper.ParamChar + "paramTemplateFailureAutoReply,"
                                                     + SOut.Bool(apptReminderRule.IsAutoReplyEnabled) + ","
                                                     + "'" + SOut.String(apptReminderRule.Language) + "',"
                                                     + "'" + SOut.String(apptReminderRule.EmailTemplateType.ToString()) + "',"
                                                     + "'" + SOut.String(apptReminderRule.AggEmailTemplateType.ToString()) + "',"
                                                     + DbHelper.ParamChar + "paramTemplateComeInMessage,"
                                                     + SOut.Bool(apptReminderRule.IsSendForMinorsBirthday) + ","
                                                     + SOut.Long(apptReminderRule.EmailHostingTemplateNum) + ","
                                                     + SOut.Int(apptReminderRule.MinorAge) + ","
                                                     + SOut.Int((int) apptReminderRule.SendMultipleInvites) + ","
                                                     + "'" + SOut.Long(apptReminderRule.TimeSpanMultipleInvites.Ticks) + "')";
        if (apptReminderRule.TemplateSMS == null) apptReminderRule.TemplateSMS = "";
        var paramTemplateSMS = new OdSqlParameter("paramTemplateSMS", SOut.StringParam(apptReminderRule.TemplateSMS));
        if (apptReminderRule.TemplateEmailSubject == null) apptReminderRule.TemplateEmailSubject = "";
        var paramTemplateEmailSubject = new OdSqlParameter("paramTemplateEmailSubject", SOut.StringParam(apptReminderRule.TemplateEmailSubject));
        if (apptReminderRule.TemplateEmail == null) apptReminderRule.TemplateEmail = "";
        var paramTemplateEmail = new OdSqlParameter("paramTemplateEmail", SOut.StringParam(apptReminderRule.TemplateEmail));
        if (apptReminderRule.TemplateSMSAggShared == null) apptReminderRule.TemplateSMSAggShared = "";
        var paramTemplateSMSAggShared = new OdSqlParameter("paramTemplateSMSAggShared", SOut.StringParam(apptReminderRule.TemplateSMSAggShared));
        if (apptReminderRule.TemplateSMSAggPerAppt == null) apptReminderRule.TemplateSMSAggPerAppt = "";
        var paramTemplateSMSAggPerAppt = new OdSqlParameter("paramTemplateSMSAggPerAppt", SOut.StringParam(apptReminderRule.TemplateSMSAggPerAppt));
        if (apptReminderRule.TemplateEmailSubjAggShared == null) apptReminderRule.TemplateEmailSubjAggShared = "";
        var paramTemplateEmailSubjAggShared = new OdSqlParameter("paramTemplateEmailSubjAggShared", SOut.StringParam(apptReminderRule.TemplateEmailSubjAggShared));
        if (apptReminderRule.TemplateEmailAggShared == null) apptReminderRule.TemplateEmailAggShared = "";
        var paramTemplateEmailAggShared = new OdSqlParameter("paramTemplateEmailAggShared", SOut.StringParam(apptReminderRule.TemplateEmailAggShared));
        if (apptReminderRule.TemplateEmailAggPerAppt == null) apptReminderRule.TemplateEmailAggPerAppt = "";
        var paramTemplateEmailAggPerAppt = new OdSqlParameter("paramTemplateEmailAggPerAppt", SOut.StringParam(apptReminderRule.TemplateEmailAggPerAppt));
        if (apptReminderRule.TemplateAutoReply == null) apptReminderRule.TemplateAutoReply = "";
        var paramTemplateAutoReply = new OdSqlParameter("paramTemplateAutoReply", SOut.StringParam(apptReminderRule.TemplateAutoReply));
        if (apptReminderRule.TemplateAutoReplyAgg == null) apptReminderRule.TemplateAutoReplyAgg = "";
        var paramTemplateAutoReplyAgg = new OdSqlParameter("paramTemplateAutoReplyAgg", SOut.StringParam(apptReminderRule.TemplateAutoReplyAgg));
        if (apptReminderRule.TemplateFailureAutoReply == null) apptReminderRule.TemplateFailureAutoReply = "";
        var paramTemplateFailureAutoReply = new OdSqlParameter("paramTemplateFailureAutoReply", SOut.StringParam(apptReminderRule.TemplateFailureAutoReply));
        if (apptReminderRule.TemplateComeInMessage == null) apptReminderRule.TemplateComeInMessage = "";
        var paramTemplateComeInMessage = new OdSqlParameter("paramTemplateComeInMessage", SOut.StringParam(apptReminderRule.TemplateComeInMessage));
        {
            apptReminderRule.ApptReminderRuleNum = Db.NonQ(command, true, "ApptReminderRuleNum", "apptReminderRule", paramTemplateSMS, paramTemplateEmailSubject, paramTemplateEmail, paramTemplateSMSAggShared, paramTemplateSMSAggPerAppt, paramTemplateEmailSubjAggShared, paramTemplateEmailAggShared, paramTemplateEmailAggPerAppt, paramTemplateAutoReply, paramTemplateAutoReplyAgg, paramTemplateFailureAutoReply, paramTemplateComeInMessage);
        }
    }

    public static bool Update(ApptReminderRule apptReminderRule, ApptReminderRule oldApptReminderRule)
    {
        var command = "";
        if (apptReminderRule.TypeCur != oldApptReminderRule.TypeCur)
        {
            if (command != "") command += ",";
            command += "TypeCur = " + SOut.Int((int) apptReminderRule.TypeCur) + "";
        }

        if (apptReminderRule.TSPrior != oldApptReminderRule.TSPrior)
        {
            if (command != "") command += ",";
            command += "TSPrior = '" + SOut.Long(apptReminderRule.TSPrior.Ticks) + "'";
        }

        if (apptReminderRule.SendOrder != oldApptReminderRule.SendOrder)
        {
            if (command != "") command += ",";
            command += "SendOrder = '" + SOut.String(apptReminderRule.SendOrder) + "'";
        }

        if (apptReminderRule.IsSendAll != oldApptReminderRule.IsSendAll)
        {
            if (command != "") command += ",";
            command += "IsSendAll = " + SOut.Bool(apptReminderRule.IsSendAll) + "";
        }

        if (apptReminderRule.TemplateSMS != oldApptReminderRule.TemplateSMS)
        {
            if (command != "") command += ",";
            command += "TemplateSMS = " + DbHelper.ParamChar + "paramTemplateSMS";
        }

        if (apptReminderRule.TemplateEmailSubject != oldApptReminderRule.TemplateEmailSubject)
        {
            if (command != "") command += ",";
            command += "TemplateEmailSubject = " + DbHelper.ParamChar + "paramTemplateEmailSubject";
        }

        if (apptReminderRule.TemplateEmail != oldApptReminderRule.TemplateEmail)
        {
            if (command != "") command += ",";
            command += "TemplateEmail = " + DbHelper.ParamChar + "paramTemplateEmail";
        }

        if (apptReminderRule.ClinicNum != oldApptReminderRule.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(apptReminderRule.ClinicNum) + "";
        }

        if (apptReminderRule.TemplateSMSAggShared != oldApptReminderRule.TemplateSMSAggShared)
        {
            if (command != "") command += ",";
            command += "TemplateSMSAggShared = " + DbHelper.ParamChar + "paramTemplateSMSAggShared";
        }

        if (apptReminderRule.TemplateSMSAggPerAppt != oldApptReminderRule.TemplateSMSAggPerAppt)
        {
            if (command != "") command += ",";
            command += "TemplateSMSAggPerAppt = " + DbHelper.ParamChar + "paramTemplateSMSAggPerAppt";
        }

        if (apptReminderRule.TemplateEmailSubjAggShared != oldApptReminderRule.TemplateEmailSubjAggShared)
        {
            if (command != "") command += ",";
            command += "TemplateEmailSubjAggShared = " + DbHelper.ParamChar + "paramTemplateEmailSubjAggShared";
        }

        if (apptReminderRule.TemplateEmailAggShared != oldApptReminderRule.TemplateEmailAggShared)
        {
            if (command != "") command += ",";
            command += "TemplateEmailAggShared = " + DbHelper.ParamChar + "paramTemplateEmailAggShared";
        }

        if (apptReminderRule.TemplateEmailAggPerAppt != oldApptReminderRule.TemplateEmailAggPerAppt)
        {
            if (command != "") command += ",";
            command += "TemplateEmailAggPerAppt = " + DbHelper.ParamChar + "paramTemplateEmailAggPerAppt";
        }

        if (apptReminderRule.DoNotSendWithin != oldApptReminderRule.DoNotSendWithin)
        {
            if (command != "") command += ",";
            command += "DoNotSendWithin = '" + SOut.Long(apptReminderRule.DoNotSendWithin.Ticks) + "'";
        }

        if (apptReminderRule.IsEnabled != oldApptReminderRule.IsEnabled)
        {
            if (command != "") command += ",";
            command += "IsEnabled = " + SOut.Bool(apptReminderRule.IsEnabled) + "";
        }

        if (apptReminderRule.TemplateAutoReply != oldApptReminderRule.TemplateAutoReply)
        {
            if (command != "") command += ",";
            command += "TemplateAutoReply = " + DbHelper.ParamChar + "paramTemplateAutoReply";
        }

        if (apptReminderRule.TemplateAutoReplyAgg != oldApptReminderRule.TemplateAutoReplyAgg)
        {
            if (command != "") command += ",";
            command += "TemplateAutoReplyAgg = " + DbHelper.ParamChar + "paramTemplateAutoReplyAgg";
        }

        if (apptReminderRule.TemplateFailureAutoReply != oldApptReminderRule.TemplateFailureAutoReply)
        {
            if (command != "") command += ",";
            command += "TemplateFailureAutoReply = " + DbHelper.ParamChar + "paramTemplateFailureAutoReply";
        }

        if (apptReminderRule.IsAutoReplyEnabled != oldApptReminderRule.IsAutoReplyEnabled)
        {
            if (command != "") command += ",";
            command += "IsAutoReplyEnabled = " + SOut.Bool(apptReminderRule.IsAutoReplyEnabled) + "";
        }

        if (apptReminderRule.Language != oldApptReminderRule.Language)
        {
            if (command != "") command += ",";
            command += "Language = '" + SOut.String(apptReminderRule.Language) + "'";
        }

        if (apptReminderRule.EmailTemplateType != oldApptReminderRule.EmailTemplateType)
        {
            if (command != "") command += ",";
            command += "EmailTemplateType = '" + SOut.String(apptReminderRule.EmailTemplateType.ToString()) + "'";
        }

        if (apptReminderRule.AggEmailTemplateType != oldApptReminderRule.AggEmailTemplateType)
        {
            if (command != "") command += ",";
            command += "AggEmailTemplateType = '" + SOut.String(apptReminderRule.AggEmailTemplateType.ToString()) + "'";
        }

        if (apptReminderRule.TemplateComeInMessage != oldApptReminderRule.TemplateComeInMessage)
        {
            if (command != "") command += ",";
            command += "TemplateComeInMessage = " + DbHelper.ParamChar + "paramTemplateComeInMessage";
        }

        if (apptReminderRule.IsSendForMinorsBirthday != oldApptReminderRule.IsSendForMinorsBirthday)
        {
            if (command != "") command += ",";
            command += "IsSendForMinorsBirthday = " + SOut.Bool(apptReminderRule.IsSendForMinorsBirthday) + "";
        }

        if (apptReminderRule.EmailHostingTemplateNum != oldApptReminderRule.EmailHostingTemplateNum)
        {
            if (command != "") command += ",";
            command += "EmailHostingTemplateNum = " + SOut.Long(apptReminderRule.EmailHostingTemplateNum) + "";
        }

        if (apptReminderRule.MinorAge != oldApptReminderRule.MinorAge)
        {
            if (command != "") command += ",";
            command += "MinorAge = " + SOut.Int(apptReminderRule.MinorAge) + "";
        }

        if (apptReminderRule.SendMultipleInvites != oldApptReminderRule.SendMultipleInvites)
        {
            if (command != "") command += ",";
            command += "SendMultipleInvites = " + SOut.Int((int) apptReminderRule.SendMultipleInvites) + "";
        }

        if (apptReminderRule.TimeSpanMultipleInvites != oldApptReminderRule.TimeSpanMultipleInvites)
        {
            if (command != "") command += ",";
            command += "TimeSpanMultipleInvites = '" + SOut.Long(apptReminderRule.TimeSpanMultipleInvites.Ticks) + "'";
        }

        if (command == "") return false;
        if (apptReminderRule.TemplateSMS == null) apptReminderRule.TemplateSMS = "";
        var paramTemplateSMS = new OdSqlParameter("paramTemplateSMS", SOut.StringParam(apptReminderRule.TemplateSMS));
        if (apptReminderRule.TemplateEmailSubject == null) apptReminderRule.TemplateEmailSubject = "";
        var paramTemplateEmailSubject = new OdSqlParameter("paramTemplateEmailSubject", SOut.StringParam(apptReminderRule.TemplateEmailSubject));
        if (apptReminderRule.TemplateEmail == null) apptReminderRule.TemplateEmail = "";
        var paramTemplateEmail = new OdSqlParameter("paramTemplateEmail", SOut.StringParam(apptReminderRule.TemplateEmail));
        if (apptReminderRule.TemplateSMSAggShared == null) apptReminderRule.TemplateSMSAggShared = "";
        var paramTemplateSMSAggShared = new OdSqlParameter("paramTemplateSMSAggShared", SOut.StringParam(apptReminderRule.TemplateSMSAggShared));
        if (apptReminderRule.TemplateSMSAggPerAppt == null) apptReminderRule.TemplateSMSAggPerAppt = "";
        var paramTemplateSMSAggPerAppt = new OdSqlParameter("paramTemplateSMSAggPerAppt", SOut.StringParam(apptReminderRule.TemplateSMSAggPerAppt));
        if (apptReminderRule.TemplateEmailSubjAggShared == null) apptReminderRule.TemplateEmailSubjAggShared = "";
        var paramTemplateEmailSubjAggShared = new OdSqlParameter("paramTemplateEmailSubjAggShared", SOut.StringParam(apptReminderRule.TemplateEmailSubjAggShared));
        if (apptReminderRule.TemplateEmailAggShared == null) apptReminderRule.TemplateEmailAggShared = "";
        var paramTemplateEmailAggShared = new OdSqlParameter("paramTemplateEmailAggShared", SOut.StringParam(apptReminderRule.TemplateEmailAggShared));
        if (apptReminderRule.TemplateEmailAggPerAppt == null) apptReminderRule.TemplateEmailAggPerAppt = "";
        var paramTemplateEmailAggPerAppt = new OdSqlParameter("paramTemplateEmailAggPerAppt", SOut.StringParam(apptReminderRule.TemplateEmailAggPerAppt));
        if (apptReminderRule.TemplateAutoReply == null) apptReminderRule.TemplateAutoReply = "";
        var paramTemplateAutoReply = new OdSqlParameter("paramTemplateAutoReply", SOut.StringParam(apptReminderRule.TemplateAutoReply));
        if (apptReminderRule.TemplateAutoReplyAgg == null) apptReminderRule.TemplateAutoReplyAgg = "";
        var paramTemplateAutoReplyAgg = new OdSqlParameter("paramTemplateAutoReplyAgg", SOut.StringParam(apptReminderRule.TemplateAutoReplyAgg));
        if (apptReminderRule.TemplateFailureAutoReply == null) apptReminderRule.TemplateFailureAutoReply = "";
        var paramTemplateFailureAutoReply = new OdSqlParameter("paramTemplateFailureAutoReply", SOut.StringParam(apptReminderRule.TemplateFailureAutoReply));
        if (apptReminderRule.TemplateComeInMessage == null) apptReminderRule.TemplateComeInMessage = "";
        var paramTemplateComeInMessage = new OdSqlParameter("paramTemplateComeInMessage", SOut.StringParam(apptReminderRule.TemplateComeInMessage));
        command = "UPDATE apptreminderrule SET " + command
                                                 + " WHERE ApptReminderRuleNum = " + SOut.Long(apptReminderRule.ApptReminderRuleNum);
        Db.NonQ(command, paramTemplateSMS, paramTemplateEmailSubject, paramTemplateEmail, paramTemplateSMSAggShared, paramTemplateSMSAggPerAppt, paramTemplateEmailSubjAggShared, paramTemplateEmailAggShared, paramTemplateEmailAggPerAppt, paramTemplateAutoReply, paramTemplateAutoReplyAgg, paramTemplateFailureAutoReply, paramTemplateComeInMessage);
        return true;
    }

    public static void DeleteMany(List<long> listApptReminderRuleNums)
    {
        if (listApptReminderRuleNums == null || listApptReminderRuleNums.Count == 0) return;
        var command = "DELETE FROM apptreminderrule "
                      + "WHERE ApptReminderRuleNum IN(" + string.Join(",", listApptReminderRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ApptReminderRule> listNew, List<ApptReminderRule> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ApptReminderRule>();
        var listUpdNew = new List<ApptReminderRule>();
        var listUpdDB = new List<ApptReminderRule>();
        var listDel = new List<ApptReminderRule>();
        listNew.Sort((x, y) => { return x.ApptReminderRuleNum.CompareTo(y.ApptReminderRuleNum); });
        listDB.Sort((x, y) => { return x.ApptReminderRuleNum.CompareTo(y.ApptReminderRuleNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            ApptReminderRule fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            ApptReminderRule fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.ApptReminderRuleNum < fieldDB.ApptReminderRuleNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ApptReminderRuleNum > fieldDB.ApptReminderRuleNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.ApptReminderRuleNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}