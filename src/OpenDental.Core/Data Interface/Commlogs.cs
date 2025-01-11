using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Commlogs
{
    #region Get Methods

    /// <summary>
    ///     Returns the list of CommItemTypeAutos. Filters out the IsODHQ-only CommItemTypeAuto when the user is not in
    ///     HQ.
    /// </summary>
    public static List<CommItemTypeAuto> GetCommItemTypes()
    {
        var listCommonItemTypeAutos = Enum.GetValues(typeof(CommItemTypeAuto)).Cast<CommItemTypeAuto>().ToList();
        listCommonItemTypeAutos.RemoveAll(GenericTools.IsODHQ); //only remove the HQ commlog type(s) if we are not in HQ
        return listCommonItemTypeAutos;
    }

    #endregion

    ///<summary>Gets all items for the current patient ordered by date.</summary>
    public static List<Commlog> Refresh(long patNum)
    {
        var command =
            "SELECT * FROM commlog"
            + " WHERE PatNum = '" + SOut.Long(patNum) + "'"
            + " ORDER BY CommDateTime";
        return CommlogCrud.SelectMany(command);
    }

    ///<summary>Gets one commlog item from database.</summary>
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

    public static bool Update(Commlog commlog, Commlog commlogOld)
    {
        return CommlogCrud.Update(commlog, commlogOld);
    }

    public static void Delete(Commlog commlog)
    {
        var command = "SELECT COUNT(*) FROM smsfrommobile WHERE CommlogNum=" + SOut.Long(commlog.CommlogNum);
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("CommLogs", "Not allowed to delete a commlog attached to a text message."));

        CommlogCrud.Delete(commlog.CommlogNum);
    }

    ///<summary>Used when printing or emailing recall to make a commlog entry without any display.</summary>
    public static void InsertForRecallOrReactivation(long patNum, CommItemMode commItemMode, int numberOfReminders, long defNumNewStatus, CommItemTypeAuto commonItemTypeAuto = CommItemTypeAuto.RECALL)
    {
        InsertForRecallOrReactivation(patNum, commItemMode, numberOfReminders, defNumNewStatus, CommItemSource.User, Security.CurUser.UserNum //Recall commlog not associated to the Web Sched app.
            , DateTime.Now, commonItemTypeAuto);
    }

    /// <summary>
    ///     Used when printing or emailing recall to make a commlog entry without any display.
    ///     Set commSource to the corresponding entity that is making this recall.  E.g. Web Sched.
    ///     If the commSource is a 3rd party, set it to ProgramLink and make an overload that accepts the ProgramNum.
    /// </summary>
    public static Commlog InsertForRecallOrReactivation(long patNum, CommItemMode commItemMode, int numberOfReminders, long defNumNewStatus, CommItemSource commItemSource, long userNum, DateTime dateTimeNow, CommItemTypeAuto commItemTypeAuto = CommItemTypeAuto.RECALL, string message = "")
    {
        var commType = GetTypeAuto(commItemTypeAuto);
        var commTypeStr = "Reactivation";
        if (commItemTypeAuto == CommItemTypeAuto.RECALL) commTypeStr = "Recall";

        var commlog = GetTodayCommlog(patNum, commItemMode, commItemTypeAuto);
        if (commlog != null) return commlog;

        commlog = new Commlog();
        commlog.PatNum = patNum;
        commlog.CommDateTime = dateTimeNow;
        commlog.CommType = commType;
        commlog.Mode_ = commItemMode;
        commlog.SentOrReceived = CommSentOrReceived.Sent;
        commlog.Note = "";
        if (numberOfReminders == 0)
            commlog.Note = Lans.g("FormRecallList", $"{commTypeStr} reminder.");
        else if (numberOfReminders == 1)
            commlog.Note = Lans.g("FormRecallList", $"Second {commTypeStr} reminder.");
        else if (numberOfReminders == 2)
            commlog.Note = Lans.g("FormRecallList", $"Third {commTypeStr} reminder.");
        else
            commlog.Note = Lans.g("FormRecallList", $"{commTypeStr} reminder:") + " " + (numberOfReminders + 1);

        if (defNumNewStatus == 0)
            commlog.Note += "  " + Lans.g("Commlogs", "Status None");
        else
            commlog.Note += "  " + Defs.GetName(DefCat.RecallUnschedStatus, defNumNewStatus);

        if (!string.IsNullOrWhiteSpace(message)) commlog.Note += "\r\n" + message;

        commlog.UserNum = userNum;
        commlog.CommSource = commItemSource;
        commlog.CommlogNum = Insert(commlog);
        var ehrMeasureEventNew = new EhrMeasureEvent();
        ehrMeasureEventNew.DateTEvent = commlog.CommDateTime;
        ehrMeasureEventNew.EventType = EhrMeasureEventType.ReminderSent;
        ehrMeasureEventNew.PatNum = commlog.PatNum;
        var moreInfo = commlog.Note;
        if (moreInfo.Length > 200) moreInfo = moreInfo.Substring(0, 199) + "...";

        ehrMeasureEventNew.MoreInfo = moreInfo;
        EhrMeasureEvents.Insert(ehrMeasureEventNew);
        return commlog;
    }

    /// <summary>
    ///     Gets an existing Commlog sent today for patNum, _mode, and type.  Returns null if not found or no defs are
    ///     setup for type.
    /// </summary>
    public static Commlog GetTodayCommlog(long patNum, CommItemMode commItemMode, CommItemTypeAuto commItemTypeAuto)
    {
        var commType = GetTypeAuto(commItemTypeAuto);
        string command;
        var dateSQL = "CURDATE()";
        if (ODBuild.IsUnitTest) dateSQL = SOut.Date(DateTime_.Today, true);

        if (commType == 0) return null;

        command = "SELECT * FROM commlog WHERE ";
        command += DbHelper.DtimeToDate("CommDateTime") + " = " + dateSQL;
        command += " AND PatNum=" + SOut.Long(patNum) + " AND CommType=" + SOut.Long(commType)
                   + " AND Mode_=" + SOut.Long((int) commItemMode)
                   + " AND SentOrReceived=1";
        var listCommlogs = CommlogCrud.SelectMany(command).OrderByDescending(x => x.CommDateTime).ToList();
        return listCommlogs.FirstOrDefault();
    }

    /// <Summary>
    ///     Returns a defnum.  If no match, then it returns the first one in the list in that category.
    ///     If there are no defs in the category, 0 is returned.
    /// </Summary>
    public static long GetTypeAuto(CommItemTypeAuto commItemTypeAuto)
    {
        var listDefs = Defs.GetDefsForCategory(DefCat.CommLogTypes);
        var def = listDefs.Find(x => x.ItemValue == commItemTypeAuto.ToString());
        if (def != null) return def.DefNum;

        if (listDefs.Count > 0) return listDefs[0].DefNum;

        return 0;
    }

    public static int GetRecallUndoCount(DateTime date)
    {
        var command = "SELECT COUNT(*) FROM commlog "
                      + "WHERE " + DbHelper.DtimeToDate("CommDateTime") + " = " + SOut.Date(date) + " "
                      + "AND (SELECT ItemValue FROM definition WHERE definition.DefNum=commlog.CommType) ='" + CommItemTypeAuto.RECALL + "'";
        return SIn.Int(DataCore.GetScalar(command));
    }

    public static void RecallUndo(DateTime date)
    {
        var command = "DELETE FROM commlog "
                      + "WHERE " + DbHelper.DtimeToDate("CommDateTime") + " = " + SOut.Date(date) + " "
                      + "AND (SELECT ItemValue FROM definition WHERE definition.DefNum=commlog.CommType) ='" + CommItemTypeAuto.RECALL + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Returns the message used to ask if the user would like to save the appointment/patient note as a commlog when
    ///     deleting an appointment/patient note.  Only returns up to the first 30 characters of the note.
    /// </summary>
    public static string GetDeleteApptCommlogMessage(string noteText, ApptStatus apptStatus)
    {
        var commlogMsgText = "";
        if (noteText == "") return commlogMsgText;

        if (apptStatus == ApptStatus.PtNote || apptStatus == ApptStatus.PtNoteCompleted)
            commlogMsgText = Lans.g("Commlogs", "Save patient note in CommLog?") + "\r\n" + "\r\n";
        else
            commlogMsgText = Lans.g("Commlogs", "Save appointment note in CommLog?") + "\r\n" + "\r\n";

        //Show up to 30 characters of the note because they can get rather large thus pushing the buttons off the screen.
        commlogMsgText += noteText.Substring(0, Math.Min(noteText.Length, 30));
        if (noteText.Length > 30)
            //Append ... to the end of the message so that they know there is more to the note than what is displayed.
            commlogMsgText += "...";

        return commlogMsgText;
    }

    /// <summary>Returns up to the first 30 characters of the note that will be saved into a new Commlog.</summary>
    public static string GetCommlogMessageText(string noteText)
    {
        var commlogMsgText = "";
        commlogMsgText += noteText.Substring(0, Math.Min(noteText.Length, 30));
        if (noteText.Length > 30)
            //Append ... to the end of the message so that they know there is more to the note than what is displayed.
            commlogMsgText += "...";

        return commlogMsgText;
    }

    /// <summary>
    ///     Gets all commlogs for family that contain a DateTimeEnd entry.  Used internally to keep track of how long
    ///     calls lasted.
    /// </summary>
    public static List<Commlog> GetTimedCommlogsForPat(long guarantor)
    {
        var command = "SELECT commlog.* FROM commlog "
                      + "INNER JOIN patient ON commlog.PatNum=patient.PatNum AND patient.Guarantor=" + SOut.Long(guarantor) + " "
                      + "WHERE " + DbHelper.Year("commlog.DateTimeEnd") + ">1";
        return CommlogCrud.SelectMany(command);
    }

    ///<summary>This returns whether the commlog is an automated type that should be shortened in display.</summary>
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

        var isAutomated = commItemSource != CommItemSource.User || commItemTypeAuto == CommItemTypeAuto.FHIR;
        return isAutomated;
    }

    ///<summary>Returns the first line in a note, trimmed to 38 characters if there is no newline.</summary>
    public static string GetNoteFirstLine(string value)
    {
        var index = value.IndexOf(Environment.NewLine);
        if (index != -1 && index <= 38)
            //If there is a newline within bounds
            //Trim at the newline.
            return value.Substring(0, index) + "(...)";

        if (value.Length > 38) return value.Substring(0, 38) + "(...)";

        return value;
    }

    #region Misc Methods

    ///<summary>Returns true if there are any rows that have a Note with char length greater than 65,535</summary>
    public static bool HasAnyLongNotes()
    {
        var command = "SELECT COUNT(*) FROM commlog WHERE CHAR_LENGTH(commlog.Note)>65535";
        return Db.GetCount(command) != "0";
    }

    ///<summary>Gets all referral specific commlogs. If referralNum is 0, returns empty list.</summary>
    public static List<Commlog> GetForReferral(long referralNum)
    {
        if (referralNum == 0)
            //Don't bother going and looking for every commlog where referralNum=0.
            return new List<Commlog>();

        var command = "SELECT * "
                      + "FROM commlog "
                      + "WHERE ReferralNum=" + SOut.Long(referralNum) + " "
                      + "ORDER BY CommDateTime";
        return CommlogCrud.SelectMany(command);
    }

    ///<summary>Deletes all commlogs associated to the referral passed in.</summary>
    public static void DeleteForReferral(long referralNum)
    {
        if (referralNum == 0) return;

        var command = "DELETE FROM commlog WHERE ReferralNum=" + SOut.Long(referralNum);
        Db.NonQ(command);
    }

    #endregion
}

public enum CommItemTypeAuto
{
    ///<summary>0</summary>
    [ShortDescription("APPT")] [Description("Appointent")]
    APPT,

    ///<summary>1</summary>
    [ShortDescription("FIN")] [Description("Financial")]
    FIN,

    ///<summary>2</summary>
    [ShortDescription("RECALL")] [Description("Recall")]
    RECALL,

    ///<summary>3</summary>
    [ShortDescription("MISC")] [Description("Miscellaneous")]
    MISC,

    ///<summary>4</summary>
    [ShortDescription("TEXT")] [Description("Text Communication (E-mail, Sms, etc.)")]
    TEXT,

    ///<summary>5</summary>
    [ShortDescription("ODHQ")] [Description("Open Dental HQ-Generated")] [IsODHQ]
    ODHQ,

    ///<summary>6</summary>
    [ShortDescription("REACT")] [Description("Reactivation")]
    REACT,

    ///<summary>6</summary>
    [ShortDescription("FHIR")] [Description("FHIR API")]
    FHIR
}