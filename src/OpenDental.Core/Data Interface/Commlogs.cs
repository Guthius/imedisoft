using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Commlogs
{
    public static List<CommItemTypeAuto> GetCommItemTypes()
    {
        return Enum.GetValues(typeof(CommItemTypeAuto)).Cast<CommItemTypeAuto>().ToList();
    }

    public static List<Commlog> Refresh(long patNum)
    {
        return CommlogCrud.SelectMany("SELECT * FROM commlog WHERE PatNum = " + patNum + " ORDER BY CommDateTime");
    }

    public static Commlog GetOne(long commlogNum)
    {
        return CommlogCrud.SelectOne(commlogNum);
    }

    public static long Insert(Commlog commlog)
    {
        return CommlogCrud.Insert(commlog);
    }

    public static void Update(Commlog commlog)
    {
        CommlogCrud.Update(commlog);
    }

    public static void Update(Commlog commlog, Commlog commlogOld)
    {
        CommlogCrud.Update(commlog, commlogOld);
    }

    public static void Delete(Commlog commlog)
    {
        var command = "SELECT COUNT(*) FROM smsfrommobile WHERE CommlogNum=" + commlog.CommlogNum;
        if (Db.GetCount(command) != "0")
        {
            throw new Exception("Not allowed to delete a commlog attached to a text message.");
        }

        CommlogCrud.Delete(commlog.CommlogNum);
    }

    public static void InsertForRecallOrReactivation(long patNum, CommItemMode commItemMode, int numberOfReminders, long defNumNewStatus, CommItemTypeAuto commonItemTypeAuto = CommItemTypeAuto.RECALL)
    {
        InsertForRecallOrReactivation(patNum, commItemMode, numberOfReminders, defNumNewStatus, CommItemSource.User, Security.CurUser.UserNum, DateTime.Now, commonItemTypeAuto);
    }

    public static Commlog InsertForRecallOrReactivation(long patNum, CommItemMode commItemMode, int numberOfReminders, long defNumNewStatus, CommItemSource commItemSource, long userNum, DateTime dateTimeNow, CommItemTypeAuto commItemTypeAuto = CommItemTypeAuto.RECALL, string message = "")
    {
        var commType = GetTypeAuto(commItemTypeAuto);
        var commTypeStr = "Reactivation";
        if (commItemTypeAuto == CommItemTypeAuto.RECALL)
        {
            commTypeStr = "Recall";
        }

        var commlog = GetTodayCommlog(patNum, commItemMode, commItemTypeAuto);
        if (commlog != null)
        {
            return commlog;
        }

        commlog = new Commlog
        {
            PatNum = patNum,
            CommDateTime = dateTimeNow,
            CommType = commType,
            Mode_ = commItemMode,
            SentOrReceived = CommSentOrReceived.Sent,
            Note = ""
        };

        commlog.Note = numberOfReminders switch
        {
            0 => $"{commTypeStr} reminder.",
            1 => $"Second {commTypeStr} reminder.",
            2 => $"Third {commTypeStr} reminder.",
            _ => $"{commTypeStr} reminder: " + (numberOfReminders + 1)
        };

        if (defNumNewStatus == 0)
        {
            commlog.Note += "  Status None";
        }
        else
        {
            commlog.Note += "  " + Defs.GetName(DefCat.RecallUnschedStatus, defNumNewStatus);
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            commlog.Note += "\r\n" + message;
        }

        commlog.UserNum = userNum;
        commlog.CommSource = commItemSource;
        commlog.CommlogNum = Insert(commlog);
        
        return commlog;
    }

    public static Commlog GetTodayCommlog(long patNum, CommItemMode commItemMode, CommItemTypeAuto commItemTypeAuto)
    {
        var commType = GetTypeAuto(commItemTypeAuto);
        if (commType == 0)
        {
            return null;
        }

        return CommlogCrud
            .SelectMany(
                "SELECT * FROM commlog " +
                "WHERE DATE(CommDateTime) = CURDATE() " +
                "AND PatNum=" + patNum + " " +
                "AND CommType=" + commType + " " +
                "AND Mode_=" + (int) commItemMode + " " +
                "AND SentOrReceived=1")
            .OrderByDescending(x => x.CommDateTime)
            .FirstOrDefault();
    }

    public static long GetTypeAuto(CommItemTypeAuto commItemTypeAuto)
    {
        var defs = Defs.GetDefsForCategory(DefCat.CommLogTypes);

        var def = defs.Find(x => x.ItemValue == commItemTypeAuto.ToString());
        if (def is not null)
        {
            return def.DefNum;
        }

        return defs.Count > 0 ? defs[0].DefNum : 0;
    }

    public static int GetRecallUndoCount(DateTime date)
    {
        return SIn.Int(DataCore.GetScalar(
            "SELECT COUNT(*) FROM commlog " +
            "WHERE DATE(CommDateTime) = " + SOut.Date(date) + " " +
            "AND (SELECT ItemValue FROM definition WHERE definition.DefNum = commlog.CommType) = '" + CommItemTypeAuto.RECALL + "'"));
    }

    public static void RecallUndo(DateTime date)
    {
        Db.NonQ(
            "DELETE FROM commlog " +
            "WHERE DATE(CommDateTime) = " + SOut.Date(date) + " " +
            "AND (SELECT ItemValue FROM definition WHERE definition.DefNum = commlog.CommType) = '" + CommItemTypeAuto.RECALL + "'");
    }

    public static string GetDeleteApptCommlogMessage(string noteText, ApptStatus apptStatus)
    {
        var commlogMsgText = "";
        if (noteText == "")
        {
            return commlogMsgText;
        }

        commlogMsgText = apptStatus is ApptStatus.PtNote or ApptStatus.PtNoteCompleted
            ? "Save patient note in CommLog?\r\n\r\n"
            : "Save appointment note in CommLog?\r\n\r\n";

        commlogMsgText += noteText.Substring(0, Math.Min(noteText.Length, 30));
        if (noteText.Length > 30)
        {
            commlogMsgText += "...";
        }

        return commlogMsgText;
    }

    public static bool IsAutomated(string commType, CommItemSource commItemSource)
    {
        CommItemTypeAuto commItemTypeAuto;
        try
        {
            commItemTypeAuto = (CommItemTypeAuto) Enum.Parse(typeof(CommItemTypeAuto), commType, true);
        }
        catch
        {
            return false;
        }

        return commItemSource != CommItemSource.User || commItemTypeAuto == CommItemTypeAuto.FHIR;
    }

    public static string GetNoteFirstLine(string value)
    {
        var index = value.IndexOf(Environment.NewLine, StringComparison.Ordinal);
        if (index != -1 && index <= 38)
        {
            return value.Substring(0, index) + "(...)";
        }

        if (value.Length > 38)
        {
            return value.Substring(0, 38) + "(...)";
        }

        return value;
    }

    public static List<Commlog> GetForReferral(long referralNum)
    {
        if (referralNum == 0)
        {
            return [];
        }

        return CommlogCrud.SelectMany(
            "SELECT * FROM commlog " +
            "WHERE ReferralNum=" + referralNum + " " +
            "ORDER BY CommDateTime");
    }

    public static void DeleteForReferral(long referralNum)
    {
        if (referralNum == 0)
        {
            return;
        }

        Db.NonQ("DELETE FROM commlog WHERE ReferralNum=" + referralNum);
    }
}

public enum CommItemTypeAuto
{
    [ShortDescription("APPT")] [Description("Appointent")]
    APPT = 0,

    [ShortDescription("FIN")] [Description("Financial")]
    FIN = 1,

    [ShortDescription("RECALL")] [Description("Recall")]
    RECALL = 2,

    [ShortDescription("MISC")] [Description("Miscellaneous")]
    MISC = 3,

    [ShortDescription("TEXT")] [Description("Text Communication (E-mail, Sms, etc.)")]
    TEXT = 4,

    [ShortDescription("REACT")] [Description("Reactivation")]
    REACT = 6,

    [ShortDescription("FHIR")] [Description("FHIR API")]
    FHIR = 7
}