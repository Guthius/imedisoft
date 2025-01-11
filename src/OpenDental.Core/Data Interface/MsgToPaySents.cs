using System;
using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MsgToPaySents
{
    public static long Insert(MsgToPaySent msgToPaySent)
    {
        return MsgToPaySentCrud.Insert(msgToPaySent);
    }

    /// <summary>
    ///     Creates and Inserts a MsgToPaySent and attaches a StmtLink. EmailType not used for Texts. This table is linked to
    ///     AutoComm and is only used for appointment-related messages. Inserts into this table
    ///     do not require a corresponding Text or Email to be sent as the AutoCommProcessor handles the creation and sending
    ///     of those message. Regular M2Ps are sent in real time and are not inserted into this table.
    ///     AutoComm treats M2Ps inserted with a Source of Manual as an outbound queue to handle.
    /// </summary>
    public static MsgToPaySent CreateFromPat(Patient patient, CommType commType, string message, Appointment appt, string subject = "", EmailType emailType = EmailType.Regular, long statementNum = 0)
    {
        var msgToPaySent = new MsgToPaySent();
        msgToPaySent.PatNum = patient.PatNum;
        msgToPaySent.ClinicNum = patient.ClinicNum;
        msgToPaySent.StatementNum = statementNum;
        msgToPaySent.SendStatus = AutoCommStatus.SendNotAttempted;
        msgToPaySent.Source = MsgToPaySource.Manual;
        msgToPaySent.Message = message;
        msgToPaySent.MessageType = commType;
        msgToPaySent.Subject = subject;
        msgToPaySent.EmailType = emailType;
        msgToPaySent.ApptNum = appt.AptNum;
        msgToPaySent.ApptDateTime = appt.AptDateTime;
        msgToPaySent.StatementNum = Insert(msgToPaySent);
        //Bind the Statement to the MsgToPaySent
        StmtLinks.AttachMsgToPayToStatement(statementNum, ListTools.FromSingle(msgToPaySent.MsgToPaySentNum));
        return msgToPaySent;
    }

    public static bool Update(MsgToPaySent msgToPaySent, MsgToPaySent msgToPaySentOld = null)
    {
        if (msgToPaySentOld == null)
        {
            MsgToPaySentCrud.Update(msgToPaySent);
            return true;
        }

        return MsgToPaySentCrud.Update(msgToPaySent, msgToPaySentOld);
    }

    ///<summay>Gets and returns all unsent (SendNotAttempted) MsgToPaySents from the database.</summay>
    public static List<MsgToPaySent> GetAllUnsent(long clinicNum, DateTime dateNow, CommType commType = CommType.Invalid)
    {
        //SendNotAttempted means this has been queued to be sent
        var command = $"SELECT * FROM msgtopaysent WHERE Source={SOut.Enum(MsgToPaySource.Manual)} AND ((DateTimeSent < '1880-01-01' AND SendStatus={SOut.Enum(AutoCommStatus.SendNotAttempted)}) "
                      + $"OR (SendStatus={SOut.Enum(AutoCommStatus.SendFailed)} AND DATE(DateTimeSendFailed)<DATE({SOut.Date(dateNow, true)})))"; //Or the attempted send failed and its been at least a day.
        if (clinicNum >= 0)
            //Query clinicNum if needed
            command += $" AND ClinicNum={SOut.Long(clinicNum)}";
        if (commType != CommType.Invalid) command += $" AND MessageType={SOut.Enum(commType)}";
        return MsgToPaySentCrud.SelectMany(command);
    }
}

/// <summary>
///     Backing class for PrefName.PaymentPortalMsgToPayEmailMessageTemplate and
///     PrefName.PaymentPortalMsgToPayEmailMessageTemplateAppt. Holds the email template and type (html, raw html, etc,
///     etc)
/// </summary>
public class MsgToPayEmailTemplate
{
    public EmailType EmailType;
    public string Template;
}