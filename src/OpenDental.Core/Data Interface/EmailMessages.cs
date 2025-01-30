using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using CodeBase;
using DataConnectionBase;
using Google;
using Health.Direct.Agent;
using Health.Direct.Common.Certificates;
using Health.Direct.Common.Domains;
using Health.Direct.Common.Mail;
using Health.Direct.Common.Mail.Notifications;
using Health.Direct.ResolverPlugins;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using MimeKit;
using OpenDentBusiness.Email;
using OpenDentBusiness.FileIO;
using GmailApi = Google.Apis.Gmail.v1;
using Header = Health.Direct.Common.Mime.Header;
using MimeEntity = Health.Direct.Common.Mime.MimeEntity;

namespace OpenDentBusiness;

public class EmailMessages
{
    [ThreadStatic]
    private static DirectAgent _directAgent;

    public static bool IsHtmlEmail(EmailType emailType)
    {
        return emailType is EmailType.Html or EmailType.RawHtml;
    }

    private delegate string ReplaceImgSrc(string valueOriginal, string imgName, string localFilePath);

    public static EmailMessage GetOne(long emailMessageNum)
    {
        var command = "SELECT * FROM emailmessage WHERE EmailMessageNum = " + SOut.Long(emailMessageNum);
        var emailMessage = EmailMessageCrud.SelectOne(emailMessageNum);
        if (emailMessage != null)
        {
            command = "SELECT * FROM emailattach WHERE EmailMessageNum = " + SOut.Long(emailMessageNum);
            emailMessage.Attachments = EmailAttachCrud.SelectMany(command);
        }

        return emailMessage;
    }

    public static List<EmailMessage> GetMailboxForAddress(EmailAddress emailAddress, DateTime dateFrom, DateTime dateTo, params MailboxType[] mailboxTypeArray)
    {
        //Use Reflection to get all the fields of EmailMessage to construct the query.  We do this instead of SELECT * because we want to limit the
        //amount of data loaded into memory from the BodyText and RawEmailIn columns.  Using Reflection allows us to add fields to the EmailMessage 
        //class and not need to return to this method and manually alter it.
        var command = "SELECT " + string.Join(", ", typeof(EmailMessage).GetFields()
                                    .Where(field =>
                                    {
                                        //Copied from CrudGenHelper.IsNotDbColumn()
                                        var objectArray = field.GetCustomAttributes(typeof(CrudColumnAttribute), true);
                                        if (objectArray.Length == 0) return true;

                                        return !((CrudColumnAttribute) objectArray[0]).IsNotDbColumn;
                                    })
                                    .Select(field =>
                                    {
                                        switch (field.Name)
                                        {
                                            case nameof(EmailMessage.BodyText):
                                                //We only pull the first 50 characters of the bodytext for preview purposes. After double-clicking an email in the inbox to view it, 
                                                //then the entire email contents are read from the database.
                                                return "SUBSTR(" + field.Name + ",1,50) " + field.Name;
                                            case nameof(EmailMessage.RawEmailIn):
                                                //We also do not pull the RawEmailIn, because it is not necessary for the inbox.
                                                return "'' " + field.Name;
                                            default:
                                                return field.Name;
                                        }
                                    }))
                                + " FROM emailmessage "
                                + "WHERE MsgDateTime BETWEEN	" + SOut.Date(dateFrom) + " AND " + SOut.Date(dateTo.AddDays(1)) + " AND ( "; //Cannot use DATE(MsgDateTime), because the index on MsgDateTime would not work.
        var strSentReceived = "";
        if (emailAddress.WebmailProvNum == 0)
        {
            //emailmessages
            //must match one of these EmailSentOrReceived statuses
            if (mailboxTypeArray.Contains(MailboxType.Inbox))
            {
                var emailPlatformExcludeWebMail = EmailPlatform.All & ~EmailPlatform.WebMail;
                var listEmailSentOrReceivedsReceivedTypes = GetUnreadTypes(emailPlatformExcludeWebMail).Concat(GetReadTypes(emailPlatformExcludeWebMail)).ToList();
                var receivedTypesStr = string.Join(",", listEmailSentOrReceivedsReceivedTypes.Select(x => SOut.Int((int) x)));
                strSentReceived += " (SentOrReceived IN (" + receivedTypesStr + ") AND RecipientAddress = '" + SOut.String(emailAddress.EmailUsername.Trim()) + "') ";
            }

            if (mailboxTypeArray.Contains(MailboxType.Sent))
            {
                if (strSentReceived != "") strSentReceived += " OR ";

                var emailTypesExcludeAck = EmailPlatform.All & ~EmailPlatform.Ack;
                var listSentTypes = GetSentTypes(emailTypesExcludeAck, true); //Do include webmail, but not Acks
                var sentTypesStr = string.Join(",", listSentTypes.Select(x => SOut.Int((int) x)));
                strSentReceived += " (SentOrReceived IN (" + sentTypesStr + ") "
                                   + "AND (FromAddress LIKE '%" + SOut.String(GetAddressSimple(emailAddress.EmailUsername).Trim()) + "%'";
                if (!string.IsNullOrEmpty(emailAddress.SenderAddress)
                    && GetAddressSimple(emailAddress.SenderAddress).Trim() != GetAddressSimple(emailAddress.EmailUsername))
                    strSentReceived += " OR FromAddress LIKE '%" + SOut.String(GetAddressSimple(emailAddress.SenderAddress).Trim()) + "%'";

                strSentReceived += ")) ";
            }
        }
        else
        {
            //webmail messages for matching provnum
            var listEmailSentOrReceiveds = new List<EmailSentOrReceived>();
            //must match one of these EmailSentOrReceived statuses
            if (mailboxTypeArray.Contains(MailboxType.Inbox))
            {
                listEmailSentOrReceiveds.AddRange(GetReadTypes(EmailPlatform.WebMail));
                listEmailSentOrReceiveds.AddRange(GetUnreadTypes(EmailPlatform.WebMail));
            }

            if (mailboxTypeArray.Contains(MailboxType.Sent)) listEmailSentOrReceiveds.AddRange(GetSentTypes(EmailPlatform.WebMail));

            if (listEmailSentOrReceiveds.Count > 0)
                strSentReceived += "ProvNumWebMail=" + SOut.Long(emailAddress.WebmailProvNum)
                                                     + " AND SentOrReceived IN (" + string.Join(",", listEmailSentOrReceiveds.Select(x => SOut.Int((int) x))) + ") ";
        }

        command += strSentReceived;
        command += ") ORDER BY MsgDateTime";
        var listEmailMessagesRet = EmailMessageCrud.SelectMany(command);
        var listEmailAttaches = EmailAttaches.GetForEmails(listEmailMessagesRet.Select(x => x.EmailMessageNum).ToList());
        var dictionaryEmailAttaches = new Dictionary<long, List<EmailAttach>>();
        for (var i = 0; i < listEmailAttaches.Count; i++)
        {
            if (!dictionaryEmailAttaches.ContainsKey(listEmailAttaches[i].EmailMessageNum)) dictionaryEmailAttaches[listEmailAttaches[i].EmailMessageNum] = new List<EmailAttach>();

            dictionaryEmailAttaches[listEmailAttaches[i].EmailMessageNum].Add(listEmailAttaches[i]);
        }

        for (var i = 0; i < listEmailMessagesRet.Count; i++)
            if (dictionaryEmailAttaches.ContainsKey(listEmailMessagesRet[i].EmailMessageNum))
                listEmailMessagesRet[i].Attachments = dictionaryEmailAttaches[listEmailMessagesRet[i].EmailMessageNum];

        return listEmailMessagesRet;
    }

    public static List<string> GetHistoricalEmailAddresses(EmailAddress emailAddress)
    {
        var fromAddress = SOut.String(GetAddressSimple(emailAddress.EmailUsername).Trim());
        var recipientAddress = SOut.String(emailAddress.EmailUsername.Trim());
        var fromAddressSender = SOut.String(GetAddressSimple(emailAddress.SenderAddress).Trim());
        var recipientAddressSender = SOut.String(emailAddress.SenderAddress.Trim());
        var emailPlatformExcludeWebMail = EmailPlatform.All & ~EmailPlatform.WebMail;
        var listEmailSentOrReceivedsReceivedTypes = GetReadTypes(emailPlatformExcludeWebMail).Concat(GetUnreadTypes(emailPlatformExcludeWebMail)).ToList();
        var receivedTypesStr = string.Join(",", listEmailSentOrReceivedsReceivedTypes.Select(x => SOut.Int((int) x)));
        var emailPlatformExcludeAck = EmailPlatform.All & ~EmailPlatform.Ack;
        var listEmailSentOrReceivedsSentTypes = GetSentTypes(emailPlatformExcludeAck);
        var sentTypesStr = string.Join(",", listEmailSentOrReceivedsSentTypes.Select(x => SOut.Int((int) x)));
        var command = @"SELECT 
				(CASE
					WHEN address.ToAddress IN ('" + fromAddress + @"', '" + fromAddressSender + @"') THEN '' 
					ELSE address.ToAddress 
				END) ToAddress,
				(CASE
					WHEN address.FromAddress IN ('" + fromAddress + @"', '" + fromAddressSender + @"') THEN '' 
					ELSE address.FromAddress 
				END) FromAddress,
				(CASE
					WHEN address.RecipientAddress IN ('" + recipientAddress + @"', '" + recipientAddressSender + @"') THEN '' 
					ELSE address.RecipientAddress 
				END) RecipientAddress,
				address.CcAddress,
				address.BccAddress
				FROM (	
					SELECT DISTINCT
					LEFT(emailmessage.ToAddress,500) ToAddress,
					LEFT(emailmessage.FromAddress,500) FromAddress,
					LEFT(emailmessage.RecipientAddress,500) RecipientAddress,
					LEFT(emailmessage.CcAddress,500) CcAddress,
					LEFT(emailmessage.BccAddress,500) BccAddress
					FROM emailmessage
					WHERE (SentOrReceived IN (" + receivedTypesStr + ") AND (RecipientAddress LIKE '%" + recipientAddress + "%' OR RecipientAddress LIKE '%" + recipientAddressSender + "%')) "
                      + "OR (SentOrReceived IN(" + sentTypesStr + ") AND (FromAddress LIKE '%" + fromAddress + @"%' OR FromAddress LIKE '%" + fromAddressSender + @"%')) 
			) address";
        var table = DataCore.GetTable(command);
        var listEmailMessages = new List<EmailMessage>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var emailMessage = new EmailMessage();
            emailMessage.ToAddress = SIn.String(table.Rows[i]["ToAddress"].ToString());
            emailMessage.FromAddress = SIn.String(table.Rows[i]["FromAddress"].ToString());
            emailMessage.RecipientAddress = SIn.String(table.Rows[i]["RecipientAddress"].ToString());
            emailMessage.CcAddress = SIn.String(table.Rows[i]["CcAddress"].ToString());
            emailMessage.BccAddress = SIn.String(table.Rows[i]["BccAddress"].ToString());
            listEmailMessages.Add(emailMessage);
        }

        return GetAddressesFromMessages(listEmailMessages);
    }

    public static List<string> GetAddressesFromMessages(List<EmailMessage> listEmailMessages)
    {
        var listStrEmailAddresses = new List<string>();
        for (var i = 0; i < listEmailMessages.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(listEmailMessages[i].ToAddress)) listStrEmailAddresses.Add(listEmailMessages[i].ToAddress.Trim());

            if (!string.IsNullOrWhiteSpace(listEmailMessages[i].FromAddress)) listStrEmailAddresses.Add(listEmailMessages[i].FromAddress.Trim());

            if (!string.IsNullOrWhiteSpace(listEmailMessages[i].BccAddress)) listStrEmailAddresses.Add(listEmailMessages[i].BccAddress.Trim());

            if (!string.IsNullOrWhiteSpace(listEmailMessages[i].CcAddress)) listStrEmailAddresses.Add(listEmailMessages[i].CcAddress.Trim());

            if (!string.IsNullOrWhiteSpace(listEmailMessages[i].RecipientAddress)) listStrEmailAddresses.Add(listEmailMessages[i].RecipientAddress.Trim());
        }

        return listStrEmailAddresses;
    }

    public static List<EmailMessage> GetBySearch(long searchPatNum, string searchEmail, DateTime dateFrom, DateTime dateTo, string searchBody, bool hasAttach)
    {
        var command = "SELECT * FROM emailmessage "
                      + "WHERE TRUE ";
        if (searchPatNum != 0) command += "AND PatNum=" + SOut.Long(searchPatNum) + " ";

        if (searchEmail != "")
            command += "AND (FromAddress LIKE '%" + SOut.String(searchEmail) + "%' "
                       + "OR ToAddress LIKE '%" + SOut.String(searchEmail) + "%' "
                       + "OR RecipientAddress LIKE '%" + SOut.String(searchEmail) + "%' "
                       + "OR CcAddress LIKE '%" + SOut.String(searchEmail) + "%' "
                       + "OR BccAddress LIKE '%" + SOut.String(searchEmail) + "%') ";

        if (dateFrom != DateTime.MinValue) command += "AND DATE(MsgDateTime)>=" + SOut.Date(dateFrom) + " ";

        if (dateTo != DateTime.MinValue) command += "AND DATE(MsgDateTime)<=" + SOut.Date(dateTo) + " ";

        if (searchBody != "")
            //this should never be blank
            command += "AND (Subject LIKE '%" + SOut.String(searchBody) + "%' OR BodyText LIKE '%" + SOut.String(searchBody) + "%')";

        var listEmailMessagesRet = EmailMessageCrud.SelectMany(command);
        for (var i = 0; i < listEmailMessagesRet.Count; i++)
        {
            command = "SELECT * FROM emailattach WHERE EmailMessageNum=" + SOut.Long(listEmailMessagesRet[i].EmailMessageNum);
            listEmailMessagesRet[i].Attachments = EmailAttachCrud.SelectMany(command);
        }

        if (hasAttach) listEmailMessagesRet = listEmailMessagesRet.FindAll(x => x.Attachments.Count > 0);

        return listEmailMessagesRet;
    }

    public static List<EmailMessage> GetWebMailForPat(long patNum)
    {
        var listPatNums = Patients.GetPatNumsForPhi(patNum); //Guaranteed to have at least one value (the patNum passed in).
        var listEmailSentOrReceivedsWebMailTypes = GetUnreadTypes(EmailPlatform.WebMail)
            .Concat(GetReadTypes(EmailPlatform.WebMail))
            .Concat(GetSentTypes(EmailPlatform.WebMail)).ToList();
        var webMailTypesStr = string.Join(",", listEmailSentOrReceivedsWebMailTypes.Select(x => SOut.Int((int) x)));
        var command = "SELECT * FROM emailmessage "
                      + "WHERE PatNumSubj IN(" + string.Join(",", listPatNums) + ") "
                      + "AND SentOrReceived IN (" + webMailTypesStr + ") "
                      + "ORDER BY MsgDateTime DESC";
        return EmailMessageCrud.SelectMany(command);
    }

    public static void Update(EmailMessage emailMessage, EmailMessage emailMessageOld = null, bool isAttachmentSyncNeeded = true)
    {
        if (emailMessageOld == null)
            EmailMessageCrud.Update(emailMessage);
        else
            EmailMessageCrud.Update(emailMessage, emailMessageOld);

        if (isAttachmentSyncNeeded)
        {
            for (var i = 0; i < emailMessage.Attachments.Count; i++) emailMessage.Attachments[i].EmailMessageNum = emailMessage.EmailMessageNum; //update all of the emailmessagenums for the attachments.

            ;
            EmailAttaches.Sync(emailMessage.EmailMessageNum, emailMessage.Attachments);
        }
    }

    public static EmailSentOrReceived UpdateSentOrReceivedRead(EmailMessage emailMessage)
    {
        var emailSentOrReceived = emailMessage.SentOrReceived;
        if (emailMessage.SentOrReceived == EmailSentOrReceived.Received)
            emailSentOrReceived = EmailSentOrReceived.Read;
        else if (emailMessage.SentOrReceived == EmailSentOrReceived.WebMailReceived)
            emailSentOrReceived = EmailSentOrReceived.WebMailRecdRead;
        else if (emailMessage.SentOrReceived == EmailSentOrReceived.ReceivedDirect)
            emailSentOrReceived = EmailSentOrReceived.ReadDirect;
        else if (emailMessage.SentOrReceived == EmailSentOrReceived.SecureEmailReceivedUnread) emailSentOrReceived = EmailSentOrReceived.SecureEmailReceivedRead;

        if (emailSentOrReceived == emailMessage.SentOrReceived) return emailSentOrReceived; //Nothing to do.

        var command = "UPDATE emailmessage SET SentOrReceived=" + SOut.Int((int) emailSentOrReceived) + " WHERE EmailMessageNum=" + SOut.Long(emailMessage.EmailMessageNum);
        Db.NonQ(command);
        return emailSentOrReceived;
    }

    public static EmailSentOrReceived UpdateSentOrReceivedUnread(EmailMessage emailMessage)
    {
        var emailSentOrReceived = emailMessage.SentOrReceived;
        if (emailMessage.SentOrReceived == EmailSentOrReceived.Read)
            emailSentOrReceived = EmailSentOrReceived.Received;
        else if (emailMessage.SentOrReceived == EmailSentOrReceived.WebMailRecdRead)
            emailSentOrReceived = EmailSentOrReceived.WebMailReceived;
        else if (emailMessage.SentOrReceived == EmailSentOrReceived.ReadDirect)
            emailSentOrReceived = EmailSentOrReceived.ReceivedDirect;
        else if (emailMessage.SentOrReceived == EmailSentOrReceived.SecureEmailReceivedRead) emailSentOrReceived = EmailSentOrReceived.SecureEmailReceivedUnread;

        if (emailSentOrReceived == emailMessage.SentOrReceived) return emailSentOrReceived; //Nothing to do.

        var command = "UPDATE emailmessage SET SentOrReceived=" + SOut.Int((int) emailSentOrReceived) + " WHERE EmailMessageNum=" + SOut.Long(emailMessage.EmailMessageNum);
        Db.NonQ(command);
        return emailSentOrReceived;
    }

    public static void UpdatePatNum(EmailMessage emailMessage)
    {
        var command = "UPDATE emailmessage SET PatNum=" + SOut.Long(emailMessage.PatNum) + " WHERE EmailMessageNum=" + SOut.Long(emailMessage.EmailMessageNum);
        Db.NonQ(command);
    }
    
    public static void Insert(EmailMessage emailMessage)
    {
        EmailMessageCrud.Insert(emailMessage);
        //now, insert all the attaches.
        if (emailMessage.Attachments == null) return;

        for (var i = 0; i < emailMessage.Attachments.Count; i++) emailMessage.Attachments[i].EmailMessageNum = emailMessage.EmailMessageNum;

        EmailAttaches.InsertMany(emailMessage.Attachments);
    }
    
    public static void Delete(EmailMessage emailMessage)
    {
        if (emailMessage.EmailMessageNum == 0) return; //this prevents deletion of all commlog entries if something goes wrong.

        var command = "DELETE FROM emailmessage WHERE EmailMessageNum=" + SOut.Long(emailMessage.EmailMessageNum);
        Db.NonQ(command);
    }

    public static Patient GetPatient(EmailMessage emailMessage)
    {
        long patNum = 0;
        if (emailMessage.PatNumSubj != 0)
            patNum = emailMessage.PatNumSubj;
        else if (emailMessage.PatNum != 0) patNum = emailMessage.PatNum;

        Patient pat = null;
        if (patNum != 0) pat = Patients.GetPat(patNum);

        return pat;
    }

    public static void SendEmail(EmailMessage emailMessage, EmailAddress emailAddressSender, X509Certificate2 x509Certificate2Private = null, bool useDirect = false)
    {
        //Always insert except when error sending direct message.
        var doInsertEmailMessage = true;
        try
        {
            if (useDirect)
            {
                var errorMsg = SendEmailDirect(emailMessage, emailAddressSender);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    doInsertEmailMessage = false;
                    throw new Exception(errorMsg);
                }
            }
            else
            {
                //unsecure
                if (x509Certificate2Private == null)
                    SendEmailUnsecure(emailMessage, emailAddressSender, null);
                else
                    //unsecure w sig
                    SendEmailUnsecureWithSig(emailMessage, emailAddressSender, x509Certificate2Private);
            }
        }
        catch (Exception ex)
        {
            SetFailed(emailMessage, ex);
            throw;
        }
        finally
        {
            if (doInsertEmailMessage)
            {
                if (emailMessage.EmailMessageNum == 0)
                    Insert(emailMessage);
                else
                    Update(emailMessage);
            }
        }
    }

    public static void SetFailed(EmailMessage emailMessage, Exception ex)
    {
        emailMessage.FailReason = emailMessage.SentOrReceived.GetDescription() + " failed with error: " + MiscUtils.GetExceptionText(ex);
        emailMessage.SentOrReceived = EmailSentOrReceived.SendFailed;
    }

    private static string SendEmailDirect(EmailMessage emailMessage, EmailAddress emailAddressFrom)
    {
        emailMessage.FromAddress = emailAddressFrom.EmailUsername.Trim(); //Cannot be emailAddressFrom.SenderAddress, or else will not find the correct encryption certificate.  Used in ConvertEmailMessageToMessage().
        //Start by converting the emailMessage to an unencrypted message using the Direct libraries. The email must be in this form to carry out encryption.
        var msgUnencrypted = ConvertEmailMessageToMessage(emailMessage, true);
        var msgEnvelopeUnencrypted = new MessageEnvelope(msgUnencrypted);
        var outMsgUnencrypted = new OutgoingMessage(msgEnvelopeUnencrypted);
        var strErrors = SendEmailDirect(outMsgUnencrypted, emailAddressFrom);
        return strErrors;
    }

    private static string SendEmailDirect(OutgoingMessage outgoingMessageUnencrypted, EmailAddress emailAddressFrom)
    {
        var strErrors = "";
        var strSenderAddress = emailAddressFrom.EmailUsername.Trim(); //Cannot be emailAddressFrom.SenderAddress, or else will not find the right encryption certificate.
        //Locate or discover public certificates for each receiver for encryption purposes.
        for (var i = 0; i < outgoingMessageUnencrypted.Recipients.Count; i++)
        {
            var receiveAddress = outgoingMessageUnencrypted.Recipients[i].Address.Trim();
            var listX509Certificate2sValid = new List<X509Certificate2>();
            var listX509Certificate2sInvalid = new List<X509Certificate2>();
            try
            {
                TryAddTrustDirect(receiveAddress, listX509Certificate2sValid, listX509Certificate2sInvalid);
            }
            catch (Exception ex)
            {
                if (strErrors != "") strErrors += "\r\n";

                strErrors += ex.Message;
            }

            //For Direct addresses, only use the current listX509Certificate2sValid for sending.  Otherwise, use all certs in pub cert store.
            var untrustedCount = -1;
            if (listX509Certificate2sValid.Count > 0 || listX509Certificate2sInvalid.Count > 0)
            {
                //The receiveAddress has a hosted cert, therefore is Direct address.
                if (listX509Certificate2sValid.Count == 0) untrustedCount = listX509Certificate2sInvalid.Count;
            }
            else
            {
                //Standard encrypted email.
                untrustedCount = GetReceiverUntrustedCount(receiveAddress);
            }

            if (untrustedCount >= 0)
            {
                if (strErrors != "") strErrors += "\r\n";

                strErrors += Lans.g("EmailMessages", "No active certificates discovered for recipient: ") + " " + receiveAddress;
                if (untrustedCount > 0) strErrors += "\r\n" + Lans.g("EmailMessages", "Inactive certificates discovered") + ": " + untrustedCount;
            }
        }

        if (strErrors != "") return strErrors; //Most likely could not find the public certificate for the receiver.  In any case, cannot continue.

        var listAddresses = new List<string>();
        listAddresses.Add(strSenderAddress);
        for (var i = 0; i < outgoingMessageUnencrypted.Recipients.Count; i++) listAddresses.Add(outgoingMessageUnencrypted.Recipients[i].Address.Trim());

        OutgoingMessage outgoingMessageEncrypted = null;
        try
        {
            var directAgent = GetDirectAgentForEmailAddress(listAddresses.ToArray());
            outgoingMessageEncrypted = directAgent.ProcessOutgoing(outgoingMessageUnencrypted); //This is where encryption, signing, and trust verification occurs.
        }
        catch (Exception ex)
        {
            if (strErrors != "") strErrors += "\r\n";

            strErrors += ex.Message;
            return strErrors; //Cannot recover from an encryption error.
        }

        outgoingMessageEncrypted.Message.SubjectValue = "Encrypted Message"; //Prevents a warning in the transport testing tool (TTT). http://tools.ietf.org/html/rfc5322#section-3.6.5
        var emailMessageEncrypted = ConvertMessageToEmailMessage(outgoingMessageEncrypted.Message, false, true); //No point in saving the encrypted attachment, because nobody can read it and it will bloat the OpenDentImages folder.
        var nameValueCollectionHeaders = new NameValueCollection();
        for (var i = 0; i < outgoingMessageEncrypted.Message.Headers.Count; i++) nameValueCollectionHeaders.Add(outgoingMessageEncrypted.Message.Headers[i].Name, outgoingMessageEncrypted.Message.Headers[i].ValueRaw);

        var byteArrayEncryptedBody = Encoding.UTF8.GetBytes(outgoingMessageEncrypted.Message.Body.Text); //The bytes of the encrypted and base 64 encoded body string.  No need to call Tidy() here because this body text will be in base64.
        var memoryStream = new MemoryStream(byteArrayEncryptedBody);
        memoryStream.Position = 0;
        //The memory stream for the alternate view must be mime (not an entire email), based on AlternateView use example http://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage.alternateviews.aspx
        var alternateView = new AlternateView(memoryStream, outgoingMessageEncrypted.Message.ContentType); //Causes the receiver to recognize this email as an encrypted email.
        alternateView.TransferEncoding = TransferEncoding.SevenBit;
        if (emailAddressFrom.ServerPort == 465)
        {
            //Implicit SSL
            //See comments inside SendEmailUnsecure() regarding why this does not work.
            if (strErrors != "") strErrors += "\r\n";

            strErrors += Lans.g("EmailMessages", "Direct messages cannot be sent over implicit SSL.");
        }
        else
        {
            SendEmailUnsecure(emailMessageEncrypted, emailAddressFrom, nameValueCollectionHeaders, alternateViewArray: alternateView); //Not really unsecure in this spot, because the message is already encrypted.
        }

        memoryStream.Dispose();
        return strErrors;
    }

    private static void SendAckDirect(IncomingMessage incomingMessage, EmailAddress emailAddressFrom, long patNum)
    {
        //The CreateAcks() function handles the case where the incoming message is an MDN, in which case we do not reply with anything.
        //The CreateAcks() function also takes care of figuring out where to send the MDN, because the rules are complicated.
        //According to http://wiki.directproject.org/Applicability+Statement+for+Secure+Health+Transport+Working+Version#x3.0%20Message%20Disposition%20Notification,
        //The MDN must be sent to the first available of: Disposition-Notification-To header, MAIL FROM SMTP command, Sender header, From header.
        var notificationType = MDNStandard.NotificationType.Failed;
        notificationType = MDNStandard.NotificationType.Processed;
        var listNotificationMessages =
            incomingMessage.CreateAcks("OpenDental " + Assembly.GetExecutingAssembly().GetName().Version, "", notificationType).ToList();
        if (listNotificationMessages == null) return;

        var strErrorsAll = "";
        for (var i = 0; i < listNotificationMessages.Count(); i++)
        {
            var strErrors = "";
            try
            {
                //According to RFC3798, section 3 - Format of a Message Disposition Notification http://tools.ietf.org/html/rfc3798#page-3
                //A message disposition notification is a MIME message with a top-level
                //content-type of multipart/report (defined in [RFC-REPORT]).  When
                //multipart/report content is used to transmit an MDN:
                //(a)  The report-type parameter of the multipart/report content is "disposition-notification".
                //(b)  The first component of the multipart/report contains a human-readable explanation of the MDN, as described in [RFC-REPORT].
                //(c)  The second component of the multipart/report is of content-type message/disposition-notification, described in section 3.1 of this document.
                //(d)  If the original message or a portion of the message is to be returned to the sender, it appears as the third component of the multipart/report.
                //     The decision of whether or not to return the message or part of the message is up to the MUA generating the MDN.  However, in the case of 
                //     encrypted messages requesting MDNs, encrypted message text MUST be returned, if it is returned at all, only in its original encrypted form.
                var outgoingMessageDirect = new OutgoingMessage(listNotificationMessages[i]);
                if (listNotificationMessages[i].ToValue.Trim().ToLower() == listNotificationMessages[i].FromValue.Trim().ToLower()) continue; //Do not send an ack to self.

                var emailMessage = ConvertMessageToEmailMessage(outgoingMessageDirect.Message, false, true);
                emailMessage.PatNum = patNum;
                //First save the ack message to the database in case their is a failure sending the email. This way we can remember to try and send it again later, based on SentOrReceived.
                emailMessage.SentOrReceived = GetUnsentTypes(EmailPlatform.Ack).First();
                var memoryStream = new MemoryStream();
                listNotificationMessages[i].Save(memoryStream);
                var byteArrayMdnMessage = memoryStream.ToArray();
                emailMessage.BodyText = Encoding.UTF8.GetString(byteArrayMdnMessage);
                memoryStream.Dispose();
                Insert(emailMessage);
            }
            catch (Exception ex)
            {
                strErrors = ex.Message;
            }

            if (strErrorsAll != "") strErrorsAll += "\r\n";

            strErrorsAll += strErrors;
        }

        try
        {
            SendOldestUnsentAck(emailAddressFrom); //Send the ack(s) we created above.
        }
        catch
        {
            //Not critical to send the acks here, because they will be sent later if they failed now.
        }
    }

    public static void SendOldestUnsentAck(EmailAddress emailAddressFrom)
    {
        string command;
        var emailSentOrReceivedAckSent = GetSentTypes(EmailPlatform.Ack).First();
        //Get the time that the last Direct Ack was sent for the From address.
        command = DbHelper.LimitOrderBy(
            "SELECT MsgDateTime FROM emailmessage "
            + "WHERE FromAddress='" + SOut.String(emailAddressFrom.EmailUsername.Trim()) + "' AND SentOrReceived=" + SOut.Long((int) emailSentOrReceivedAckSent) + " "
            + "ORDER BY MsgDateTime DESC",
            1);
        var dateTimeLastAck = SIn.DateTime(DataCore.GetScalar(command)); //dateTimeLastAck will be 0001-01-01 if there is not yet any sent Acks.
        if ((DateTime.Now - dateTimeLastAck).TotalSeconds < 60)
            //Our last Ack sent was less than 15 seconds ago.  Abort sending Acks right now.
            return;

        //Get the oldest Ack for the From address which has not been sent yet.
        var listEmailSentOrReceivedsAckNotSent = GetUnsentTypes(EmailPlatform.Ack);
        var ackNotSentStr = string.Join(",", listEmailSentOrReceivedsAckNotSent.Select(x => SOut.Int((int) x)));
        command = DbHelper.LimitOrderBy(
            "SELECT * FROM emailmessage "
            + "WHERE FromAddress='" + SOut.String(emailAddressFrom.EmailUsername.Trim()) + "' AND SentOrReceived IN (" + ackNotSentStr + ") "
            + "ORDER BY EmailMessageNum", //The oldest Ack is the one that was recorded first.  EmailMessageNum is better than using MsgDateTime, because MsgDateTime is only accurate down to the second.
            1);
        var listEmailMessagesAckNotSent = EmailMessageCrud.SelectMany(command);
        if (listEmailMessagesAckNotSent.Count < 1) return; //No Acks to send.

        var emailMessageAck = listEmailMessagesAckNotSent[0];
        var strRawEmailAck = emailMessageAck.BodyText; //Not really body text.  The entire raw Ack is saved here, and we use it to reconstruct the Ack email completely.
        var messageEnvelopeMdn = new MessageEnvelope(strRawEmailAck);
        var outgoingMessageDirect = new OutgoingMessage(messageEnvelopeMdn);
        var strErrors = "";
        try
        {
            strErrors = SendEmailDirect(outgoingMessageDirect, emailAddressFrom); //Encryption is performed in this step. Throws an exception if unable to send (i.e. when internet down).
        }
        catch
        {
            return;
        }

        if (strErrors == "")
        {
            emailMessageAck.SentOrReceived = EmailSentOrReceived.AckDirectProcessed;
            emailMessageAck.MsgDateTime = DateTime_.Now; //Update the time, otherwise the throttle will not work properly.
            Update(emailMessageAck);
        }
    }

    private static void SendDirectUnsecure(OutgoingMessage outgoingMessage, EmailAddress emailAddress, long patNum)
    {
        //When batch email operations are performed, we sometimes do this check further up in the UI.  This check is here to as a catch-all.
        //Security.CurUser will be null if this is called from a third party application (like Patient Portal).  We want to continue if that is the case.
        if (Security.CurUser != null && !Security.IsAuthorized(EnumPermType.EmailSend, true))
            //we need to suppress the message
            return;

        if (emailAddress.IsImplicitSsl)
            //The poor Content-Type header treatment by the System.Web.Mail.MailMessage class is the reason why both encrypted messages (Direct) and also signed unencrypted messages do not work though implicit SSL.
            //The System.Web.Mail.MailMessage class only understands plain text and html messages.
            //For a signed unencrypted message, the Content-Type header in the msgOut is "Content-Type: multipart/signed; boundary=PartA; protocol="application/pkcs7-signature"; micalg=sha1"
            //If the Content-Type header is added to the System.Web.Mail.MailMessage.Headers,
            //the Content-Type is modified to the following by C# when sending: "Content-Type: text/plain; boundary=PartA; protocol="application/pkcs7-signature"; micalg=sha1"
            throw new Exception(Lans.g("EmailMessages", "Cannot send this type of message over implicit SSL."));

        SmtpClient smtpClient = null;
        MailMessage mailMessage = null;
        MemoryStream memoryStreamEmailContent = null;
        AlternateView alternateView = null;
        try
        {
            smtpClient = new SmtpClient(emailAddress.SMTPserver, emailAddress.ServerPort);
            //The default credentials are not used by default, according to: 
            //http://msdn2.microsoft.com/en-us/library/system.net.mail.smtpclient.usedefaultcredentials.aspx
            smtpClient.Credentials = new NetworkCredential(emailAddress.EmailUsername.Trim(), MiscUtils.Decrypt(emailAddress.EmailPassword));
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = emailAddress.UseSSL;
            smtpClient.Timeout = 180000; //3 minutes
            mailMessage = new MailMessage();
            var contentType = "text/plain"; //This is the default value that C# would use if we did not specify a Content-Type.  However we need to specify the Content-Type for the AlternateView.
            for (var i = 0; i < outgoingMessage.Message.Headers.Count; i++)
            {
                //This copies all headers, including but not limited to: From/To/Subject/Date/MessageID/etc...
                var name = outgoingMessage.Message.Headers[i].Name;
                var val = outgoingMessage.Message.Headers[i].ValueRaw;
                switch (name.ToUpper())
                {
                    case "BCC":
                        mailMessage.Bcc.Add(val.Trim());
                        break;
                    case "CC":
                        mailMessage.CC.Add(val.Trim());
                        break;
                    case "CONTENT-TYPE":
                        contentType = val;
                        break;
                    case "FROM":
                        mailMessage.From = new MailAddress(val.Trim());
                        break;
                    case "PRIORITY":
                        mailMessage.Priority = MailPriority.Normal;
                        if (val.ToLower() == "high")
                            mailMessage.Priority = MailPriority.High;
                        else if (val.ToLower() == "low") mailMessage.Priority = MailPriority.Low;

                        break;
                    case "REPLY-TO":
                        mailMessage.ReplyTo = new MailAddress(val.Trim());
                        break;
                    case "REPLY-TO-LIST":
                        var stringArray = val.Split(',');
                        for (var j = 0; j < stringArray.Length; j++) mailMessage.ReplyToList.Add(stringArray[j].Trim());

                        break;
                    case "SENDER":
                        mailMessage.Sender = new MailAddress(val.Trim());
                        break;
                    case "SUBJECT":
                        mailMessage.Subject = SubjectTidy(val);
                        break;
                    case "TO":
                        mailMessage.To.Add(val.Trim());
                        break;
                    default: //Other headers, such as MessageID, which is needed for Direct messaging, but is not part of the standard MailMessage object.
                        mailMessage.Headers.Add(name, val); //Add to header verbatim.
                        break;
                }
            }

            //Using an AlternateView is the only way to specify a custom Content-Type.  Both encrypted email and signed email messages have special Content-Types.  Necessary for Direct messaging.
            var byteArrayContent = Encoding.UTF8.GetBytes(outgoingMessage.Message.Body.Text); //This includes the body and all attachments.  Should have already been formatted properly by the Direct library.
            memoryStreamEmailContent = new MemoryStream(byteArrayContent);
            memoryStreamEmailContent.Position = 0;
            alternateView = new AlternateView(memoryStreamEmailContent, contentType);
            alternateView.TransferEncoding = TransferEncoding.SevenBit; //Default is base64, but 7bit is much easier to read/debug.
            mailMessage.AlternateViews.Add(alternateView);
            smtpClient.Send(mailMessage);
            memoryStreamEmailContent.Dispose();
            SecurityLogs.MakeLogEntry(EnumPermType.EmailSend, patNum, "Email Sent");
        }
        finally
        {
            //Dispose of the client and messages here. For large customers, sending thousands of emails will start to fail until they restart the
            //app. Freeing memory here can prevent OutOfMemoryExceptions.
            smtpClient?.Dispose();
            mailMessage?.Dispose();
            memoryStreamEmailContent?.Dispose();
            alternateView?.Dispose();
        }
    }

    private static void SendEmailUnsecure(EmailMessage emailMessage, EmailAddress emailAddress, NameValueCollection nameValueCollectionHeaders, bool hasRetried = false, params AlternateView[] alternateViewArray)
    {
        //When batch email operations are performed, we sometimes do this check further up in the UI.  This check is here to as a catch-all.
        if (!Security.IsAuthorized(EnumPermType.EmailSend, true))
            //we need to suppress the message
            return;

        //Verify that we can connect to Google using OAuth before moving on as we can't refresh tokens from OpenDentalEmail Processor.
        if (emailAddress.AuthenticationType == OAuthType.Google)
        {
            if (emailAddress.RefreshToken.IsNullOrEmpty()) throw new ODException(emailAddress.EmailUsername + " needs to be re-authenticated. Please sign out and back in to continue.");

            using var gmailService = GoogleApiConnector.CreateGmailService(ODEmailAddressToBasic(emailAddress));
            try
            {
                //This call to Google is only to ensure that we have a valid OAuth token.  
                //There is no connect or authorize, so this is the best/smallest request we can do.
                //We have to ensure we have an OAuth token right here because we will send the email in SendEmail.WireEmailUnsecure,
                // which doesn't have a DB context and wouldn't be able to refresh the token.
                gmailService.Users.GetProfile(emailAddress.EmailUsername).Execute();
            }
            catch (GoogleApiException gae)
            {
                if (hasRetried) throw;

                if (gae.HttpStatusCode != HttpStatusCode.Unauthorized) throw;

                RefreshGmailToken(emailAddress);
                //Try one more time after refreshing
                SendEmailUnsecure(emailMessage, emailAddress, nameValueCollectionHeaders, true, alternateViewArray);
                return;
            }
        }

        //Always send the email through this centralized method.  We cannot assume we have a database context inside SendEmail.
        Email.SendEmail.WireEmailUnsecure(ODEmailAddressToBasic(emailAddress), ODEmailMessageToBasic(emailMessage), nameValueCollectionHeaders, alternateViewArray);
        emailMessage.UserNum = Security.CurUser.UserNum;
        SecurityLogs.MakeLogEntry(EnumPermType.EmailSend, emailMessage.PatNum, "Email Sent");
    }

    private static void SendEmailUnsecureWithSig(EmailMessage emailMessage, EmailAddress emailAddressFrom, X509Certificate2 x509Certificate2Private)
    {
        if (emailAddressFrom.IsImplicitSsl) throw new Exception(Lans.g("EmailMessages", "Digitally signed messages cannot be sent over implicit SSL.")); //See detailed comments in the private version of SendEmailUnsecure().

        emailMessage.UserNum = Security.CurUser.UserNum;
        emailMessage.FromAddress = emailAddressFrom.EmailUsername.Trim(); //Cannot be emailAddressFrom.SenderAddress, or else will not find the correct signing certificate.  Used in ConvertEmailMessageToMessage().
        var message = ConvertEmailMessageToMessage(emailMessage, true);
        var messageEnvelope = new MessageEnvelope(message);
        var messageOut = new OutgoingMessage(messageEnvelope);
        var directAgent = GetDirectAgentForEmailAddress(emailMessage.FromAddress);
        try
        {
            var signedEntity = directAgent.Cryptographer.Sign(messageOut.Message, x509Certificate2Private); //Compute the signature digest.  A hash of the certificate against the raw email content.
            messageOut.Message.UpdateBody(signedEntity); //Modify the relevant message headers as well as the entire message body to include the signature digest.
        }
        catch (Exception ex)
        {
            throw new ApplicationException(Lans.g("EmailMessages", "Failed to sign outgoing email message, probably due to permissions: ") + ex.Message);
        }

        SendDirectUnsecure(messageOut, emailAddressFrom, emailMessage.PatNum);
    }

    private static bool IsEmailFromInbox<TEmailAddress>(string emailUsername, List<TEmailAddress> listTEmailAddressesTo, List<TEmailAddress> listTEmailAddressesFrom, List<TEmailAddress> listTEmailAddressesCc, List<TEmailAddress> listTEmailAddressesBcc) where TEmailAddress : class
    {
        if (emailUsername == null) emailUsername = "";

        emailUsername = emailUsername.Trim().ToLower();
        if (listTEmailAddressesTo == null) listTEmailAddressesTo = new List<TEmailAddress>();

        if (listTEmailAddressesFrom == null) listTEmailAddressesFrom = new List<TEmailAddress>();

        if (listTEmailAddressesCc == null) listTEmailAddressesCc = new List<TEmailAddress>();

        if (listTEmailAddressesBcc == null) listTEmailAddressesBcc = new List<TEmailAddress>();

        var isEmailFromInbox = true;
        if (!string.Join(",", listTEmailAddressesFrom).Contains(emailUsername)) return isEmailFromInbox;

        //The email Recipient and email From addresses are the same.
        if (string.Join(",", listTEmailAddressesTo).ToLower().Contains(emailUsername) ||
            string.Join(",", listTEmailAddressesCc).ToLower().Contains(emailUsername) ||
            string.Join(",", listTEmailAddressesBcc).ToLower().Contains(emailUsername))
            //The email Recipient and email To or CC or BCC addresses are the same.  We have verified that a user can send an email to themself using 
            //only CC or BCC.
            //Download this message because it was clearly sent from the user to themself.
            return isEmailFromInbox;

        //Gmail will report sent email as if it is part of the Inbox. These emails will have the From address as the Recipient address, but the To 
        //address will be a different address.
        isEmailFromInbox = false;
        return isEmailFromInbox;
    }

    private static int RetrieveFromInboxOAuth(EmailAddress emailAddressInbox, bool hasRetried = false)
    {
        if (emailAddressInbox.RefreshToken.IsNullOrEmpty()) throw new ODException(emailAddressInbox.EmailUsername + " needs to be re-authenticated. Please sign out and back in to continue.");

        if (emailAddressInbox.AuthenticationType == OAuthType.Google) return RetrieveFromGmailInbox(emailAddressInbox, hasRetried);

        return 0;
    }

    private static int RetrieveFromGmailInbox(EmailAddress emailAddressInbox, bool hasRetried = false)
    {
        //Get all the IDs in the users inbox (this is paginated so we have to continuously receive IDs until we don't receive a 'next page' token)
        var countNewEmails = 0;
        using var gmailService = GoogleApiConnector.CreateGmailService(ODEmailAddressToBasic(emailAddressInbox));
        var listMessages = new List<global::Google.Apis.Gmail.v1.Data.Message>();
        var listEmailMessageUids = EmailMessageUids.GetMsgIdsRecipientAddress(emailAddressInbox.EmailUsername).Select(x => x.TrimStart("GmailId".ToCharArray())).ToList();
        //This example is from: https://developers.google.com/gmail/api/v1/reference/users/messages/list
        var request = gmailService.Users.Messages.List(emailAddressInbox.EmailUsername);
        //Ask for as many email message IDs as possible so that we ask the API as few times as possible.
        request.MaxResults = 500; //Maximum number of messages to return. This field defaults to 100. The maximum allowed value for this field is 500.
        //Open Dental has no need to download messages within SPAM and TRASH.
        request.IncludeSpamTrash = false;
        request.Q = emailAddressInbox.QueryString;
        while (true)
        {
            try
            {
                var listMessagesResponse = request.Execute();
                if (listMessagesResponse.Messages != null) listMessages.AddRange(listMessagesResponse.Messages);

                request.PageToken = listMessagesResponse.NextPageToken;
            }
            catch (GoogleApiException gae)
            {
                if (hasRetried) throw;

                if (gae.HttpStatusCode != HttpStatusCode.Unauthorized) throw;

                RefreshGmailToken(emailAddressInbox);
                return RetrieveFromInboxOAuth(emailAddressInbox, true);
            }

            if (request.PageToken.IsNullOrEmpty()) break;
        }

        //Filter out messages that have already been received
        listMessages = listMessages.Where(x => !listEmailMessageUids.Contains(x.Id)).ToList();
        //After receiving all of the ID's in the inbox, perform a GET for each email message
        for (var i = 0; i < listMessages.Count; i++)
        {
            var emailRequest = gmailService.Users.Messages.Get(emailAddressInbox.EmailUsername, listMessages[i].Id);
            emailRequest.Format = GmailApi.UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
            global::Google.Apis.Gmail.v1.Data.Message messageResponse = null;
            try
            {
                messageResponse = emailRequest.Execute();
            }
            catch (ThreadAbortException)
            {
                //This can happen if the application is exiting. We need to leave right away so the program does not lock up.
                //Otherwise, this loop could continue for a while if there are a lot of messages to download.
                throw;
            }
            catch (Exception ex)
            {
                //If one particular email fails to download, then skip it for now and move on to the next email.
                continue;
            }

            var emailMessageUid = new EmailMessageUid
            {
                MsgId = listMessages[i].Id, //Interchangeable with response.id
                RecipientAddress = emailAddressInbox.EmailUsername.Trim()
            };
            try
            {
                //What we receive from Gmail is a Base64 File/URL safe string, but we need this to be just Base64 (replace - and _ with + and / respectively)
                messageResponse.Raw = Regex.Replace(messageResponse.Raw, "-", "+");
                messageResponse.Raw = Regex.Replace(messageResponse.Raw, "_", "/");
                var byteArrayResponse = Convert.FromBase64String(messageResponse.Raw);
                using var memoryStream = new MemoryStream(byteArrayResponse);
                var mimeMessage = MimeMessage.Load(memoryStream);
                if (IsEmailFromInbox(emailAddressInbox.EmailUsername, mimeMessage.To.ToList(), mimeMessage.From.ToList(), mimeMessage.Cc.ToList(), mimeMessage.Bcc.ToList()))
                {
                    //Convert MIME to our Email format and store the UID in the database
                    var recd = ProcessRawEmailMessageIn(mimeMessage.ToString(), 0, emailAddressInbox, true);
                    if (emailMessageUid.RecipientAddress != recd.RecipientAddress) emailMessageUid.RecipientAddress = recd.RecipientAddress;

                    countNewEmails++;
                }
            }
            catch (ThreadAbortException)
            {
                //This can happen if the application is exiting. We need to leave right away so the program does not lock up.
                //Otherwise, this loop could continue for a while if there are a lot of messages to download.
                throw;
            }
            catch (Exception ex)
            {
                //Something went wrong processing this email. Still insert uid so we don't download it again.
            }

            EmailMessageUids.Insert(emailMessageUid);
        }

        return countNewEmails;
    }

    public static IncomingMessage RawEmailToIncomingMessage(string strRawEmailIn, EmailAddress emailAddressInbox)
    {
        IncomingMessage incomingMessage = null;
        var lastErrorMsg = "";

        #region Scrub Boundaries

        //Find all of the boundaries within the raw email and force them to be unique
        //Some third parties will format their boundaries in a way that that does not parse correctly. E.g.:
        //boundary #1 = D775FB8094F7C52EF0C994F5B1152B71
        //boundary #2 = D775FB8094F7C52EF0C994F5B1152B712
        //boundary #3 = D775FB8094F7C52EF0C994F5B1152B713
        //etc.
        var listBoundaries = new List<string>();
        var matchCollection = Regex.Matches(strRawEmailIn, @"boundary=""(.*)""");
        for (var i = 0; i < matchCollection.Count; i++)
        {
            if (!matchCollection[i].Success || matchCollection[i].Groups.Count < 2) continue;

            listBoundaries.Add(matchCollection[i].Groups[1].Value);
        }

        var hasValidBoundaries = true;
        for (var i = 0; i < listBoundaries.Count; i++)
            if (listBoundaries.Exists(x => x != listBoundaries[i] && x.StartsWith(listBoundaries[i])))
            {
                hasValidBoundaries = false;
                break;
            }

        //Replace all of the boundaries if any show up more than 4 times which indicates that they are not unique enough and need to be replaced.
        if (!hasValidBoundaries)
            for (var i = 0; i < listBoundaries.Count; i++)
            {
                //Replace this boundary within the raw email string with a better formatted boundary.
                //There are three explicit ways to utilize the boundary and we will replace each one with:
                //#1:  boundary="[uniqueBoundaryID]"
                //#2:  --[uniqueBoundaryID]
                //#3:  --[uniqueBoundaryID]--
                /******************************************************************************************
                    Boundary syntax via https://www.w3.org/Protocols/rfc1341/7_2_Multipart.html is as follows:
                    boundary := 0*69<bchars> bcharsnospace
                    bchars := bcharsnospace / " "
                    bcharsnospace := DIGIT / ALPHA / "'" / "(" / ")" /
                                                    "+" / "_" / "," / "-" / "." /
                                                    "/" / ":" / "=" / "?"
                *******************************************************************************************/
                //Generate a GUID and use ToString("N") which returns 32 hexadecimal digits with no formatting.
                //https://docs.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=netframework-4.5.2
                var boundaryNew = Guid.NewGuid().ToString("N");
                strRawEmailIn = Regex.Replace(strRawEmailIn, "boundary=\"" + listBoundaries[i] + "\"", "boundary=\"" + boundaryNew + "\"");
                strRawEmailIn = Regex.Replace(strRawEmailIn, "\r\n--" + listBoundaries[i] + "\r\n", "\r\n--" + boundaryNew + "\r\n");
                strRawEmailIn = Regex.Replace(strRawEmailIn, "\r\n--" + listBoundaries[i] + "--", "\r\n--" + boundaryNew + "--");
            }

        #endregion

        for (var i = 0; i < 5; i++)
            //We will exit if unknown error or if previous error was the same as current error.
            try
            {
                incomingMessage = new IncomingMessage(strRawEmailIn); //Used to parse all email (encrypted or not).
                break;
            }
            catch (Exception ex)
            {
                if (ex.Message == lastErrorMsg)
                    //Our last attempt to fix the issue failed.
                    throw new ApplicationException("Failed to parse raw email message.\r\n" + ex.Message);

                if (ex.Message == "Error=MissingHeaderValue")
                {
                    //The "Welcome to Email" message from GoDaddy has a blank CC field which causes the IncomingMessage() constructor to throw an exception.
                    //The TO header can be blank because it is not required, since the user could put all destination addresses in either CC or BCC alone.  We tested this.
                    strRawEmailIn = Regex.Replace(strRawEmailIn, @"TO:[ \t]*\r\n", "", RegexOptions.IgnoreCase); //Remove the TO header if it is any number of spaces or tabs followed by exactly one newline.
                    strRawEmailIn = Regex.Replace(strRawEmailIn, @"BCC:[ \t]*\r\n", "", RegexOptions.IgnoreCase); //BCC before CC, since CC is partial match of BCC
                    strRawEmailIn = Regex.Replace(strRawEmailIn, @"CC:[ \t]*\r\n", "", RegexOptions.IgnoreCase); //Remove the CC header if it is any number of spaces or tabs followed by exactly one newline.
                }
                else if (ex.Message == "An invalid character was found in the mail header: ';'.")
                {
                    //When all recipients are in the bcc field, some clients (gmail) inputs "undisclosed-recipients:;" into the TO field, which causes an error to be thrown.
                    strRawEmailIn = Regex.Replace(strRawEmailIn, @"undisclosed[ -]*recipients:[\t ]*;", "", RegexOptions.IgnoreCase); //Remove "undisclosed-recipients".
                }
                else if (ex.Message == "Error=NoRecipients")
                {
                    //When all recipients are in the bcc field, some clients (Apple mail) remove all address fields (To, cc, bcc) from the header, which causes an error to be thrown.
                    //the code below attempts to add a bcc field with the user's email into the header (seems to work for emails coming from Apple mail)
                    var lengthEmail = strRawEmailIn.Length;
                    string username;
                    if (emailAddressInbox == null)
                        username = "";
                    else if (emailAddressInbox.EmailUsername == null)
                        username = "Failed to match email address";
                    else
                        username = emailAddressInbox.EmailUsername;

                    strRawEmailIn = Regex.Replace(strRawEmailIn, @"Subject: ",
                        "Bcc: " + username + "\r\nSubject: ", RegexOptions.IgnoreCase);
                    if (strRawEmailIn.Length == lengthEmail)
                        //If the email didn't have a subject, try again with 'From'.
                        strRawEmailIn = Regex.Replace(strRawEmailIn, @"From: ",
                            "Bcc: " + username + "\r\nFrom: ", RegexOptions.IgnoreCase);
                }
                else
                {
                    throw new ApplicationException("Failed to parse raw email message.\r\n" + ex.Message);
                }

                lastErrorMsg = ex.Message;
            }

        return incomingMessage;
    }

    private static IncomingMessage DecryptIncomingMessage(IncomingMessage incomingMessage)
    {
        var directAgent = GetDirectAgentForEmailAddress(incomingMessage.Message.ToValue.Trim());
        //throw new ApplicationException("test decryption failure");
        return directAgent.ProcessIncoming(incomingMessage); //Decrypts and valudates trust.  Also removes the signature from the decrypted attachments and moves them into incomingMessage.Signatures.
    }

    public static EmailMessage ProcessRawEmailMessageIn(string strRawEmail, long emailMessageNum, EmailAddress emailAddressReceiver, bool isAck, EmailSentOrReceived emailSentOrReceivedUnencrypted = EmailSentOrReceived.Received)
    {
        var incomingMessage = RawEmailToIncomingMessage(strRawEmail, emailAddressReceiver);
        var isEncrypted = IsMimeEntityEncrypted(incomingMessage.Message);
        EmailMessage emailMessage = null;
        if (isEncrypted)
        {
            emailMessage = ConvertMessageToEmailMessage(incomingMessage.Message, false, false); //Exclude attachments until we decrypt.
            emailMessage.RawEmailIn = strRawEmail; //The raw encrypted email, including the message, the attachments, and the signature.  The body of the encrypted email is just a base64 string until decrypted.
            emailMessage.EmailMessageNum = emailMessageNum;
            emailMessage.SentOrReceived = EmailSentOrReceived.ReceivedEncrypted;
            emailMessage.RecipientAddress = emailAddressReceiver.EmailUsername.Trim();
            //The entire contents of the email are saved in the emailMessage.BodyText field, so that if decryption fails, the email will still be saved to the db for decryption later if possible.
            emailMessage.BodyText = strRawEmail;
            try
            {
                incomingMessage = DecryptIncomingMessage(incomingMessage);
                emailMessage = ConvertMessageToEmailMessage(incomingMessage.Message, true, false); //If the message was wrapped, then the To, From, Subject and Date can change after decyption. We also need to create the attachments for the decrypted message.
                emailMessage.RawEmailIn = strRawEmail; //The raw encrypted email, including the message, the attachments, and the signature.  The body of the encrypted email is just a base64 string until decrypted.
                emailMessage.EmailMessageNum = emailMessageNum;
                emailMessage.SentOrReceived = EmailSentOrReceived.ReceivedDirect;
                emailMessage.RecipientAddress = emailAddressReceiver.EmailUsername.Trim();
                if (incomingMessage.HasSenderSignatures)
                    for (var i = 0; i < incomingMessage.SenderSignatures.Count; i++)
                    {
                        var emailAttach = EmailAttaches.CreateAttach("smime.p7s", "", incomingMessage.SenderSignatures[i].Certificate.GetRawCertData(), false);
                        emailMessage.Attachments.Add(emailAttach);
                    }
            }
            catch (Exception)
            {
                //SentOrReceived will be ReceivedEncrypted, indicating to the calling code that decryption failed.
                //The decryption step may have failed due to an untrusted sender, in which case the decrypting actually took place and the signature was extracted.
                //We add the signature to the email message so it will show up next to the email message in the inbox and make it easier for the user to add trust for the sender.
                if (incomingMessage.HasSenderSignatures)
                    for (var i = 0; i < incomingMessage.SenderSignatures.Count; i++)
                    {
                        var emailAttach = EmailAttaches.CreateAttach("smime.p7s", "", incomingMessage.SenderSignatures[i].Certificate.GetRawCertData(), false);
                        emailMessage.Attachments.Add(emailAttach);
                    }

                if (emailMessageNum == 0)
                {
                    Insert(emailMessage);
                    return emailMessage; //If the message was just downloaded, then this function was called from the inbox, simply return the inserted email without an exception (it can be decypted later manually by the user).
                }

                //Do not update if emailMessageNum<>0, because nothing changed (was encrypted and still is).
                throw; //Throw an exception if trying to decrypt an email that was already in the database, so the user can see the error message in the UI.
            }
        }
        else
        {
            //Unencrypted
            //First check to see if attachments have already been digested for this email.
            var listEmailAttaches = EmailAttaches.GetForEmail(emailMessageNum); //will return an empty list if emailmessagenum == 0
            var parseAttachments = true; //Always parse attachments from the strRawEmail unless we've already parsed them before.
            if (listEmailAttaches.Count > 0)
                //Attachments have already been parsed so do not waste time re-parsing.
                //Re-parsing attachments would be very bad because there is a good chance that strRawEmail has cleared out the body portion of attachments.
                //The actual attachments will be affected (erased) if these attachments are re-parsed due to the body portions being blank.
                parseAttachments = false;

            emailMessage = ConvertMessageToEmailMessage(incomingMessage.Message, parseAttachments, false);
            emailMessage.RawEmailIn = strRawEmail;
            //Set the Attachments on emailMessage if the attachments weren't parsed from strRawEmail.
            if (!parseAttachments)
                //Calling EmailMessages.Update() will delete all email attachments and sync them with the current list of attachments even if no changes.
                //Therefore, we need to make sure to have the Attachments variable set to the "old" (current really) list of attachments.
                emailMessage.Attachments = listEmailAttaches;

            //Only try and trim the fat from the RawEmailIn column if attachments are present.
            if (GetAttachmentMimeParts(incomingMessage.Message, 1).Count == 1)
                //At this point we know that the attachments have been successfully extracted from the raw message (now or some time in the past).
                //Try to remove the attachment text from the raw email as to save space in the database.
                try
                {
                    emailMessage.RawEmailIn = DissolveAttachmentsFromIncomingMessage(incomingMessage);
                }
                catch
                {
                    //Something went wrong so keep the "bloat" in the database because it is the safest option.
                }

            //No attachments present.
            //No need to try and annul attachment body text from strRawEmail because it doesn't have any attachments.
            emailMessage.EmailMessageNum = emailMessageNum;
            emailMessage.SentOrReceived = emailSentOrReceivedUnencrypted;
            emailMessage.RecipientAddress = emailAddressReceiver.EmailUsername.Trim();
        }
        
        if (emailMessage.PatNum == 0)
        {
            //If a patient match was not already found, try to locate patient based on the email address sent from.
            var emailFromAddress = GetAddressSimple(emailMessage.FromAddress);
            var listPatientsMatched = Patients.GetPatsByEmailAddress(emailFromAddress);
            if (listPatientsMatched.Count == 1)
                //If multiple matches, then we do not want to mislead the user by assigning a patient.
                emailMessage.PatNum = listPatientsMatched[0].PatNum;
        }

        if (emailMessageNum == 0)
            Insert(emailMessage); //Also inserts all of the attachments in emailMessage.Attachments after setting each attachment EmailMessageNum properly.
        else
            Update(emailMessage);
        
        if (isEncrypted && isAck)
            //Send a Message Disposition Notification (MDN) message to the sender, as required by the Direct messaging specifications.
            //The MDN will be attached to the same patient as the incoming message.
            SendAckDirect(incomingMessage, emailAddressReceiver, emailMessage.PatNum);

        return emailMessage;
    }

    public static List<List<MimeEntity>> GetMimePartsForMimeTypes(string strRawEmailIn, EmailAddress emailAddressInbox, params string[] stringArrayMimeContentTypes)
    {
        IncomingMessage incomingMessage = null;
        List<MimeEntity> listMimeEntityLeafNodes = null;
        try
        {
            incomingMessage = RawEmailToIncomingMessage(strRawEmailIn, emailAddressInbox);
            if (IsMimeEntityEncrypted(incomingMessage.Message)) incomingMessage = DecryptIncomingMessage(incomingMessage);

            listMimeEntityLeafNodes = GetMimeLeafNodes(incomingMessage.Message);
            //If we were unable to read the mime parts, we will treat it as none found.
            listMimeEntityLeafNodes = listMimeEntityLeafNodes ?? new List<MimeEntity>();
        }
        catch
        {
            //Since we could not read the message, we cannot read the mime parts.  Therefore, none found.
            listMimeEntityLeafNodes = new List<MimeEntity>();
        }

        var listListMimeEntitiesRet = new List<List<MimeEntity>>();
        for (var i = 0; i < stringArrayMimeContentTypes.Length; i++)
        {
            var mimeContentType = stringArrayMimeContentTypes[i];
            var listMimeEntityParts = new List<MimeEntity>();
            for (var j = 0; j < listMimeEntityLeafNodes.Count; j++)
                if (listMimeEntityLeafNodes[j].ContentType.Contains(mimeContentType))
                    listMimeEntityParts.Add(listMimeEntityLeafNodes[j]);

            listListMimeEntitiesRet.Add(listMimeEntityParts);
        }

        return listListMimeEntitiesRet;
    }

    public static string GetMimeImageFileName(MimeEntity mimeEntityForImage)
    {
        string getFileNameFromField(string field)
        {
            var nameIndexStart = field.ToLower().IndexOf("name=");
            if (nameIndexStart >= 0)
            {
                nameIndexStart += 5;
            }
            else
            {
                nameIndexStart = field.ToLower().IndexOf("filename=");
                if (nameIndexStart >= 0) nameIndexStart += 9;
            }

            if (nameIndexStart < 0) return null;

            var nameIndexEnd = field.IndexOf(';', nameIndexStart + 1);
            var fileName = "";
            if (nameIndexEnd >= 0)
                fileName = field.Substring(nameIndexStart, nameIndexEnd - nameIndexStart + 1);
            else
                fileName = field.Substring(nameIndexStart);

            return fileName.Replace("\"", "").TrimEnd(';');
        }

        var filename = getFileNameFromField(mimeEntityForImage.ContentType);
        if (string.IsNullOrEmpty(filename))
            //Gmail sometimes puts the file name in the ContentDisposition field.
            filename = getFileNameFromField(mimeEntityForImage.ContentDisposition);

        return filename;
    }

    public static string GetMimeImageContentId(MimeEntity mimeEntityForImage)
    {
        if (!mimeEntityForImage.Headers.Contains("Content-ID")) return "";

        return mimeEntityForImage.Headers["Content-ID"].Value.Replace("<", "").Replace(">", "");
    }

    public static void SaveMimeImageToFile(MimeEntity mimeEntityForImage, string directoryPath, string sourceFileName)
    {
        if (!IsMimeEntityBase64(mimeEntityForImage)) return;

        try
        {
            var fileName = GetMimeImageFileName(mimeEntityForImage);
            var fileExt = Path.GetExtension(fileName);
            var filePath = ODFileUtils.CombinePaths(directoryPath, fileName);
            MemoryStream memoryStream = null;
            Bitmap bitmap = null;
            try
            {
                //Access the bitmap via passed in actualFilePath and actualFileName from EmailAttach obj, since we strip out embedded images from emails
                //and save them separately as attachments.
                bitmap = new Bitmap(Path.Combine(EmailAttaches.GetAttachPath(), sourceFileName));
            }
            catch (Exception ex)
            {
                //Something went wrong fetching image from file. Attempt to get from mimeEntityForImage, in case we didn't extract it during download.
                if (!IsMimeEntityBase64(mimeEntityForImage)) return;

                var byteArrayForImage = Convert.FromBase64String(mimeEntityForImage.Body.Text);
                memoryStream = new MemoryStream(byteArrayForImage);
                bitmap = new Bitmap(memoryStream);
            }

            var imageFormat = ImageFormat.Jpeg;
            switch (fileExt.ToLower())
            {
                case ".bmp":
                    imageFormat = ImageFormat.Bmp;
                    break;
                case ".emf":
                    imageFormat = ImageFormat.Emf;
                    break;
                case ".exif":
                    imageFormat = ImageFormat.Exif;
                    break;
                case ".gif":
                    imageFormat = ImageFormat.Gif;
                    break;
                case ".ico":
                    imageFormat = ImageFormat.Icon;
                    break;
                case ".jpg":
                    imageFormat = ImageFormat.Jpeg;
                    break;
                case ".jpeg":
                    imageFormat = ImageFormat.Jpeg;
                    break;
                case ".png":
                    imageFormat = ImageFormat.Png;
                    break;
                case ".tif":
                    imageFormat = ImageFormat.Tiff;
                    break;
                case ".tiff":
                    imageFormat = ImageFormat.Tiff;
                    break;
                case ".wmf":
                    imageFormat = ImageFormat.Wmf;
                    break;
            }

            bitmap.Save(filePath, imageFormat);
            bitmap.Dispose();
            memoryStream?.Dispose();
            return;
        }
        catch
        {
        }
    }

    public static void RefreshCertStoreExternal(EmailAddress emailAddressLocal)
    {
        var strSenderAddress = emailAddressLocal.EmailUsername.Trim(); //Cannot be emailAddressFrom.SenderAddress, or else will not find the right encryption certificate.
        try
        {
            GetDirectAgentForEmailAddress(strSenderAddress); //This line is where the refresh occurs.
        }
        catch (Exception ex)
        {
            //Likely a permission issue
        }
    }

    public static void RefreshGmailToken(EmailAddress emailAddress)
    {
        var dbToken = EmailAddresses.GetOneFromDb(emailAddress.EmailAddressNum).AccessToken;
        if (!dbToken.IsNullOrEmpty() && dbToken != emailAddress.AccessToken)
        {
            emailAddress.AccessToken = dbToken; //This means that another service has already updated the token in the db, so use that one
            return;
        }

        emailAddress.AccessToken = Google.MakeRefreshAccessTokenRequest(emailAddress.RefreshToken);
        EmailAddresses.Update(emailAddress);
        EmailAddresses.RefreshCache();
        Signalods.SetInvalid(InvalidType.Email);
    }

    public static BasicEmailAddress ODEmailAddressToBasic(EmailAddress emailAddressOd)
    {
        var basicEmailAddress = new BasicEmailAddress();
        basicEmailAddress.EmailPassword = MiscUtils.Decrypt(emailAddressOd.EmailPassword, true);
        basicEmailAddress.EmailUsername = emailAddressOd.EmailUsername;
        basicEmailAddress.ServerPort = emailAddressOd.ServerPort;
        basicEmailAddress.SMTPserver = emailAddressOd.SMTPserver;
        basicEmailAddress.UseSSL = emailAddressOd.UseSSL;
        basicEmailAddress.AccessToken = emailAddressOd.AccessToken;
        basicEmailAddress.RefreshToken = emailAddressOd.RefreshToken;
        basicEmailAddress.AuthenticationType = (BasicOAuthType) emailAddressOd.AuthenticationType;
        return basicEmailAddress;
    }

    public static BasicEmailMessage ODEmailMessageToBasic(EmailMessage emailMessageOd)
    {
        var basicEmailMessage = new BasicEmailMessage();
        basicEmailMessage.BccAddress = emailMessageOd.BccAddress;
        basicEmailMessage.CcAddress = emailMessageOd.CcAddress;
        basicEmailMessage.FromAddress = emailMessageOd.FromAddress;
        basicEmailMessage.ToAddress = emailMessageOd.ToAddress;
        //Tidy subject
        basicEmailMessage.Subject = SubjectTidy(emailMessageOd.Subject);
        basicEmailMessage.IsHtml = IsHtmlEmail(emailMessageOd.HtmlType);
        if (basicEmailMessage.IsHtml)
            //If it is HTML, tidy the body, find images within the email, download them, and provide a list
            basicEmailMessage.HtmlBody = FindAndReplaceImageTagsWithAttachedImage(BodyTidy(emailMessageOd.HtmlText), emailMessageOd.AreImagesDownloaded,
                out basicEmailMessage.ListHtmlImages);
        else
            //Normal email. Tidy it.
            basicEmailMessage.BodyText = BodyTidy(emailMessageOd.BodyText);

        //Gets a list of attachments and downloads them locally.
        basicEmailMessage.ListAttachments = GetListAttachmentsAndDownload(emailMessageOd.Attachments.ToArray());
        return basicEmailMessage;
    }

    public static List<BasicEmailAttachment> GetListAttachmentsAndDownload(params EmailAttach[] emailAttachArray)
    {
        var listBasicEmailAttachmentsFilePaths = new List<BasicEmailAttachment>();
        if (emailAttachArray.IsNullOrEmpty()) return listBasicEmailAttachmentsFilePaths;

        var attachPath = EmailAttaches.GetAttachPath();
        for (var i = 0; i < emailAttachArray.Count(); i++)
        {
            var attachFullPath = ODFileUtils.CombinePaths(attachPath, emailAttachArray[i].ActualFileName);
            var basicEmailAttachment = new BasicEmailAttachment(attachFullPath, emailAttachArray[i].DisplayedFileName);
            listBasicEmailAttachmentsFilePaths.Add(basicEmailAttachment);
        }

        return listBasicEmailAttachmentsFilePaths;
    }

    public static string BodyTidy(string str)
    {
        //This function assumes the worst case, which is a string that has all 3 types of newlines: \r, \n and \r\n
        //We will first convert \r\n and \r into \n so that all our line endings are the same. Then replace \n with \r\n to make the newlines proper.
        var retVal = str.Replace("\r\n", "\n"); //We must replace the two character newline first so that our following replacements do not create extra newlines.
        retVal = retVal.Replace("\r", "\n"); //After this step, all newlines are in the form \n.
        retVal = retVal.Replace("\n", "\r\n"); //After this step, all newlines will be in form \r\n.
        return retVal;
    }

    public static string InsertAutograph(string bodyText, EmailAutograph emailAutograph)
    {
        if (emailAutograph == null) return bodyText;

        if (string.IsNullOrEmpty(emailAutograph.AutographText)) return bodyText;

        if (bodyText.TrimEnd().ToLower().EndsWith(emailAutograph.AutographText.ToLower().Trim())) return bodyText;

        bodyText += "\r\n\r\n" + emailAutograph.AutographText;
        return bodyText;
    }

    public static void CreateCertificateStoresIfNeeded()
    {
        SystemX509Store.OpenAnchorEdit().Dispose(); //Create the NHINDAnchor certificate store if it does not already exist on the local machine.
        SystemX509Store.OpenExternalEdit().Dispose(); //Create the NHINDExternal certificate store if it does not already exist on the local machine.
        SystemX509Store.OpenPrivateEdit().Dispose(); //Create the NHINDPrivate certificate store if it does not already exist on the local machine.
    }

    private static string DissolveAttachmentsFromIncomingMessage(IncomingMessage incomingMessage)
    {
        var rawEmail = incomingMessage.SerializeMessage(); //The original raw message, unaltered.
        var listAttachments = GetAttachmentMimeParts(incomingMessage.Message);
        for (var i = 0; i < listAttachments.Count; i++)
        {
            //Clear the body text of each raw attachment body.
            if (listAttachments[i].Body.Text.Length == 0) continue; //Body is already empty.  Nothing to do.

            var rawAttachment = listAttachments[i].ToString(); //This includes the mime headers as well as the body text.
            var attachIndex = rawEmail.IndexOf(rawAttachment); //Uniquely locate the mime text in the raw email (will be unique because of header timestamps)
            if (attachIndex < 0)
                //Mime part not found in raw email?  Should be impossible.
                continue; //We do not want to crash for any reason when running DBM tools.

            //Now find the start index of the attachment body from where the attachment starts.
            var bodyIndex = rawEmail.IndexOf(listAttachments[i].Body.Text, attachIndex);
            if (bodyIndex > attachIndex + rawAttachment.Length - 1)
                //The body text match located was beyond the attachment boundary.  Should be impossible.
                continue; //We do not want to crash for any reason when running DBM tools.

            rawEmail = rawEmail.Remove(bodyIndex, listAttachments[i].Body.Text.Length);
        }

        return rawEmail;
    }

    public static string FindAndReplaceImageTagsWithAttachedImage(string localHtml, bool areImagesDownloaded, out List<string> listLocalImagePaths)
    {
        return FindAndReplaceImageTags(localHtml, areImagesDownloaded, ReplaceSrcWithCid, out listLocalImagePaths);
    }

    private static string ReplaceSrcWithCid(string value, string imgName, string localFilePath)
    {
        return Regex.Replace(value, @"src\s*=\s*""(.*?)""", "src=\"cid:" + imgName + "\"");
    }

    private static string ReplaceSrcWithEmbedded(string value, string imgName, string localFilePath)
    {
        //We can go directly to the local file space, because the calling method already performed appropriate false() check.
        if (!File.Exists(localFilePath)) return value; //Most likely an image hosted on the internet.

        var extension = Path.GetExtension(localFilePath);
        var byteArray = File.ReadAllBytes(localFilePath);
        var bytesBase64 = Convert.ToBase64String(byteArray);
        var replacement = "src=\"data:image/" + extension + ";base64," + bytesBase64 + "\"";
        return Regex.Replace(value, @"src\s*=\s*""(.*?)""", replacement);
    }

    public static string EmbedImages(string localHtml, bool areImagesDownloaded)
    {
        return FindAndReplaceImageTags(localHtml, areImagesDownloaded, ReplaceSrcWithEmbedded, out _);
    }

    private static string FindAndReplaceImageTags(string localHtml, bool areImagesDownloaded, ReplaceImgSrc replaceImgSrc, out List<string> listLocalImagePaths)
    {
        listLocalImagePaths = new List<string>();
        var matchCollection = Regex.Matches(localHtml, @"<img\s+.*?src\s*=\s*""(.*?)""");
        for (var i = 0; i < matchCollection.Count; i++)
        {
            //MarkupEdit.TranslateToXhtml(...) changes "&"  to "&amp;", we need to change it back before we set the image path. 
            var imagePath = matchCollection[i].Result("$1").Replace("&amp;", "&");
            var imgName = Path.GetFileName(imagePath);
            var imageDir = ImageStore.GetEmailImagePath();
            var imagePathLocal = Path.Combine(imageDir, imgName);
            imgName = HttpUtility.UrlEncode(imgName); //File names with spaces won't show as embedded image without doing this.
            if (!File.Exists(imagePathLocal) && File.Exists(imagePath))
            {
                //File is not in OpenDentImages folder, but is elsewhere locally, so copy it there.
                File.Copy(imagePath, imagePathLocal);
            }
            else if (!File.Exists(imagePathLocal) && !File.Exists(imagePath))
            {
                //File not found.  Leave the <img src="filename"></img> alone.  This will either be an internet hosted image or a broken image link.
                continue;
            }

            listLocalImagePaths.Add(imagePathLocal);
            //Replace the src attribute in the img tag to point to the attachment content id but preserve all other attributes (width, height, etc).
            var imgSrcCidAttachment = replaceImgSrc(matchCollection[i].Value, imgName, imagePathLocal);
            localHtml = localHtml.Replace(matchCollection[i].Value, imgSrcCidAttachment);
        }

        return localHtml;
    }

    public static string FindAndReplacePostalAddressTag(string emailBody, long clinicNum)
    {
        var disclaimerWithAddress = GetEmailDisclaimer(clinicNum);
        if (string.IsNullOrEmpty(disclaimerWithAddress)) return emailBody;

        return emailBody + "\r\n\r\n\r\n" + disclaimerWithAddress;
    }

    private static List<MimeEntity> GetAttachmentMimeParts(Message message, int limitCount = 0)
    {
        var listMimeEntitesAttachments = new List<MimeEntity>();
        var listMimeEntitesParts = new List<MimeEntity>();
        if (message.IsMultiPart) listMimeEntitesParts.AddRange(message.GetParts());

        //Traverse all branches of the mime tree to locate all attachment mime parts.
        while (true)
        {
            if (listMimeEntitesParts.Count == 0) break;

            var mimeEntity = listMimeEntitesParts[0];
            listMimeEntitesParts.RemoveAt(0);
            //An email attachment. Leaf node. https://www.ietf.org/rfc/rfc2183.txt we treat both 'attachment' and 'inline' the same. No other options.
            if (mimeEntity.ContentDisposition != null)
            {
                if (mimeEntity.HasBody)
                {
                    //Clear out the body or content of the attachment as to reduce the amount of space we take up in the database.
                    //This is safe to do at this point because we have already extracted the attachments and they are stored in the AtoZ folder or db already.
                    //Since the Text of the body for the mimeEntity is protected, we need to replace the current mime body with a new mime body.
                    listMimeEntitesAttachments.Add(mimeEntity);
                    if (limitCount > 0 && listMimeEntitesAttachments.Count == limitCount) return listMimeEntitesAttachments;
                }

                continue;
            }

            if (mimeEntity.IsMultiPart)
                //Branch node.
                listMimeEntitesParts.AddRange(mimeEntity.GetParts()); //Push children mime parts to stack to be examined in a later pass through the loop.
        }

        return listMimeEntitesAttachments;
    }

    public static string GetEmailDisclaimer(long clinicNum)
    {
        if (!PrefC.GetBool(PrefName.EmailDisclaimerIsOn)) return "";

        var disclaimer = PrefC.GetString(PrefName.EmailDisclaimerTemplate);
        if (string.IsNullOrEmpty(disclaimer)) return "";

        var postalAddress = PrefC.GetString(PrefName.PracticeTitle) + "\r\n" + Patients.GetAddressFull(
            PrefC.GetString(PrefName.PracticeAddress),
            PrefC.GetString(PrefName.PracticeAddress2),
            PrefC.GetString(PrefName.PracticeCity),
            PrefC.GetString(PrefName.PracticeST),
            PrefC.GetString(PrefName.PracticeZip));
        if (true)
        {
            var clinic = Clinics.GetClinic(clinicNum);
            if (clinic != null)
            {
                var clinicPostalAddress = Patients.GetAddressFull(clinic.AddressLine1, clinic.AddressLine2, clinic.City, clinic.State, clinic.Zip);
                if (!string.IsNullOrWhiteSpace(clinicPostalAddress.Replace(" ", "").Replace("\r\n", "").Replace(",", ""))) postalAddress = clinic.Description + "\r\n" + clinicPostalAddress;
            }
        }

        var StringBuilder = new StringBuilder(disclaimer);
        //RegReplace is case insensitive by default.
        StringTools.RegReplace(StringBuilder, "\\[PostalAddress]", postalAddress);
        return StringBuilder.ToString();
    }

    private static DirectAgent GetDirectAgentForEmailAddress(params string[] stringArrayEmailAddresses)
    {
        var listDomains = new List<string>();
        for (var i = 0; i < stringArrayEmailAddresses.Length; i++) listDomains.Add(GetDomainForAddress(stringArrayEmailAddresses[i]));

        var staticDomainResolverRes = new StaticDomainResolver(listDomains.ToArray());
        ICertificateResolver iCertificateResolverResPriv = new EmailPrivateResolver();
        ICertificateResolver iCertificateResolverResPub = new EmailPublicResolver();
        var systemX509StoreAnchor = SystemX509Store.OpenAnchor();
        var x509Certificate2CollectionAnchor = systemX509StoreAnchor.GetAllCertificates();
        systemX509StoreAnchor.Dispose();
        var TrustAnchorResolverRes = new TrustAnchorResolver(x509Certificate2CollectionAnchor);
        _directAgent = new DirectAgent(staticDomainResolverRes, iCertificateResolverResPriv, iCertificateResolverResPub, TrustAnchorResolverRes);
        _directAgent.EncryptMessages = true;
        //The Transport Testing Tool (TTT) complained when we sent a message that was not wrapped.
        //Specifically, the tool looks for the headers Orig-Date and Message-Id after the message is decrypted.
        //See http://tools.ietf.org/html/rfc5322#section-3.6.1 and http://tools.ietf.org/html/rfc5322#section-3.6.4 for details about these two header fields.
        _directAgent.WrapMessages = true;
        return _directAgent;
    }

    public static int GetReceiverUntrustedCount(string strAddressTest)
    {
        var emailPublicResolver = new EmailPublicResolver();
        var listX509Certificate2sValid = new List<X509Certificate2>();
        var listX509Certificate2sInvalid = new List<X509Certificate2>();
        emailPublicResolver.GetCertificates(strAddressTest, listX509Certificate2sValid, listX509Certificate2sInvalid);
        if (listX509Certificate2sValid.Count > 0) return -1;

        return listX509Certificate2sInvalid.Count;
    }

    public static bool IsSenderTrusted(string strAddressTest)
    {
        if (strAddressTest.Trim() == "") return false;

        if (_directAgent == null) GetDirectAgentForEmailAddress(strAddressTest);

        var systemX509StoreAnchor = SystemX509Store.OpenAnchor();
        //Look for domain level and address level trust certificates (anchors).
        var mailAddress = new MailAddress(strAddressTest);
        var x509Certificate2CollectionPriv = new EmailPrivateResolver().GetCertificates(mailAddress);
        var x509Certificate2CollectionAnchor = systemX509StoreAnchor.GetAllCertificates();
        var isTrusted = false;
        for (var i = 0; i < x509Certificate2CollectionPriv.Count; i++)
            if (_directAgent.TrustModel.CertChainValidator.IsTrustedCertificate(x509Certificate2CollectionPriv[i], x509Certificate2CollectionAnchor))
            {
                isTrusted = true;
                break;
            }

        systemX509StoreAnchor.Dispose();
        return isTrusted;
    }

    public static string SubjectTidy(string str)
    {
        var retVal = str.Replace("\r\n", " ");
        retVal = retVal.Replace("\r", " ");
        retVal = retVal.Replace("\n", " ");
        return retVal;
    }

    public static bool TryAddTrustDirect(string strAddressTest, List<X509Certificate2> listX509Certificate2sValidDirect = null, List<X509Certificate2> listX509Certificate2sInvalidDirect = null)
    {
        if (strAddressTest.Trim() == "") return false;

        if (listX509Certificate2sValidDirect == null) listX509Certificate2sValidDirect = new List<X509Certificate2>();

        if (listX509Certificate2sInvalidDirect == null) listX509Certificate2sInvalidDirect = new List<X509Certificate2>();

        try
        {
            FindPublicCertForAddress(strAddressTest, listX509Certificate2sValidDirect, listX509Certificate2sInvalidDirect);
            var emailPublicResolver = new EmailPublicResolver();
            var listX509Certificate2sValid = new List<X509Certificate2>();
            var listX509Certificate2sInvalid = new List<X509Certificate2>();
            emailPublicResolver.GetCertificates(strAddressTest, listX509Certificate2sValid, listX509Certificate2sInvalid);
            if (listX509Certificate2sValid.Count > 0 || listX509Certificate2sInvalid.Count > 0)
            {
                var systemX509StoreAnchors = SystemX509Store.OpenAnchorEdit(); //Open for read and write.  Corresponds to NHINDAnchors/Certificates.
                systemX509StoreAnchors.Add(listX509Certificate2sValid);
                systemX509StoreAnchors.Add(listX509Certificate2sInvalid);
                systemX509StoreAnchors.Dispose();
            }

            GetDirectAgentForEmailAddress(strAddressTest); //Force the cert stores to be refreshed within our DirectAgent instance.
            return true;
        }
        catch (Exception ex)
        {
            //Likely a network issue (FindPublicCertForAddress) or a permissions issue opening the anchors store.
            return false;
        }
    }

    public static X509Certificate2 GetEmailSignatureFromSmimeP7sFile(string smimeP7sFilePath)
    {
        X509Certificate2 x509Certificate2Signed2 = null;
        try
        {
            var x509Certificate2Signed1 = X509Certificate.CreateFromSignedFile(smimeP7sFilePath); //This is a public encryption key.
            x509Certificate2Signed2 = new X509Certificate2(x509Certificate2Signed1);
        }
        catch (Exception ex)
        {
            throw new Exception(Lans.g("EmailMessages", "Failed to load signature file") + ". " + ex.Message);
        }

        return x509Certificate2Signed2;
    }

    public static X509Certificate2 GetCertFromPrivateStore(string emailAddress)
    {
        //Look for domain level and address level trust certificates.
        MailAddress mailAddress = null;
        try
        {
            mailAddress = new MailAddress(emailAddress);
        }
        catch (Exception ex)
        {
            //This can happen if emailAddress is not formatted according to the email standard.
            return null;
        }

        X509Certificate2Collection x509Certificate2CollectionPriv = null;
        try
        {
            x509Certificate2CollectionPriv = new EmailPrivateResolver().GetCertificates(mailAddress);
        }
        catch (Exception ex)
        {
        }

        if (x509Certificate2CollectionPriv == null || x509Certificate2CollectionPriv.Count == 0) return null;

        return x509Certificate2CollectionPriv[0];
    }

    public static void TryAddTrustForSignature(X509Certificate2 x509Certificate2Signed)
    {
        try
        {
            var storePublicCerts = SystemX509Store.OpenExternalEdit(); //Open for read and write.  Corresponds to NHINDExternal/Certificates.
            storePublicCerts.Add(x509Certificate2Signed); //Write the pubic encryption certificate to the Windows certificate store.
        }
        catch (Exception ex)
        {
            throw new Exception(Lans.g("EmailMessages", "Failed to save signature to encryption certificate store") + ". " + ex.Message);
        }

        try
        {
            var systemX509StoreAnchors = SystemX509Store.OpenAnchorEdit(); //Open for read and write.  Corresponds to NHINDAnchors/Certificates.
            systemX509StoreAnchors.Add(x509Certificate2Signed); //Adds to NHINDAnchors/Certificates within the windows certificate store manager (mmc).
        }
        catch (Exception ex)
        {
            throw new Exception(Lans.g("EmailMessages", "Failed to save signature to trust certificate store") + ". " + ex.Message);
        }
    }

    public static string GetAddressSimple(string emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress)) return "";

        if (!emailAddress.Contains("<")) return emailAddress.Trim();

        if (!emailAddress.Contains(">")) return emailAddress.Trim();

        var startIndex = emailAddress.IndexOf("<") + 1;
        var endIndex = emailAddress.IndexOf(">") - 1;
        return emailAddress.Substring(startIndex, endIndex - startIndex + 1).Trim();
    }

    private static string GetDomainForAddress(string emailAddress)
    {
        emailAddress = GetAddressSimple(emailAddress);
        if (emailAddress.Contains("@")) return emailAddress.Substring(emailAddress.IndexOf("@") + 1); //For example, if ToAddress is ehr@opendental.com, then this will be opendental.com

        return emailAddress;
    }

    private static void FindPublicCertForAddress(string strAddressTest, List<X509Certificate2> listX509Certificate2sValid, List<X509Certificate2> listX509Certificate2sInvalid)
    {
        listX509Certificate2sValid.Clear();
        listX509Certificate2sInvalid.Clear();
        //It may be useful in the future to attempt communicating with a secondary DNS server if the primary DNS is not available.
        //const string strDnsServer = "184.73.237.102";//Amazon - This is the DNS server used within the Direct resolverPlugins test project. Appears to have worked the best for them, compared to the others listed below, but was not accessible.
        //const string strDnsServer = "10.110.22.16";//This address was tried in the Direct resolverPlugins test project and is commented out, implying that it might not be the best DNS server to use.
        //const string strDnsServer = "207.170.210.162";//This address was tried in the Direct resolverPlugins test project and is commented out, implying that it might not be the best DNS server to use.
        const string strGlobalDnsServer = "8.8.8.8"; //Google - This address was tried in the Direct resolverPlugins test project and is commented out, implying that it might not be the best DNS server to use.
        var ipAddressGlobalDnsServer = IPAddress.Parse(strGlobalDnsServer);
        var mailAddressQuery = new MailAddress(strAddressTest);
        //Attempt to discover the certificate via DNS.
        DnsQueryForCert(ipAddressGlobalDnsServer, mailAddressQuery, listX509Certificate2sValid, listX509Certificate2sInvalid);
        //Always look in LDAP even if we found some certificates in DNS.  This is required for Direct Module H.1 to work for stage 3.
        ICertificateResolver certResolverInternetLdap = new LdapCertResolver(ipAddressGlobalDnsServer, TimeSpan.FromMinutes(3));
        var x509Certificate2Collection = certResolverInternetLdap.GetCertificates(mailAddressQuery); //Can return null.
        if (x509Certificate2Collection != null)
            for (var i = 0; i < x509Certificate2Collection.Count; i++)
            {
                if (!EmailNameResolver.IsCertValid(x509Certificate2Collection[i]))
                {
                    //If the certificate is not yet valid or is expired, then discard.
                    listX509Certificate2sInvalid.Add(x509Certificate2Collection[i]);
                    continue;
                }

                listX509Certificate2sValid.Add(x509Certificate2Collection[i]);
            }

        //If any certificates were discovered via DNS or LDAP, save them locally for later reference.
        EmailPublicResolver emailPublicResolver = null;
        if (listX509Certificate2sValid.Count > 0 || listX509Certificate2sInvalid.Count > 0)
        {
            emailPublicResolver = new EmailPublicResolver(false); //Open for read/write.  Requires more permission.  Users do not usually have write permission.
            //At least one certificate was hosted in DNS or LDAP, which means that the emailAddress is a Direct address,
            //not a regular encrypted email address.				
            var x509Certificate2CollectionPubOld = emailPublicResolver.GetCertificatesForAddress(strAddressTest); //Address specific (excludes domain level certificates).
            if (x509Certificate2CollectionPubOld != null && x509Certificate2CollectionPubOld.Count > 0)
            {
                //Remove other certs which are specifically for the address being queried, to make stores match DNS and LDAP results.
                emailPublicResolver.Store.Remove(x509Certificate2CollectionPubOld);
                var systemX509StoreAnchors = SystemX509Store.OpenAnchorEdit();
                systemX509StoreAnchors.Remove(x509Certificate2CollectionPubOld);
                systemX509StoreAnchors.Dispose();
            }

            emailPublicResolver.Store.Add(listX509Certificate2sValid); //Write the discovered certificates to the Windows certificate store for future reference.
            emailPublicResolver.Store.Add(listX509Certificate2sInvalid); //Write the discovered certificates to the Windows certificate store for future reference.
            return;
        }

        if (emailPublicResolver == null) emailPublicResolver = new EmailPublicResolver(); //Open for read only.  Nearly all users have read-only permission.

        //No certificates discovered in DNS or LDAP.  Either the address is not a Direct address or the servers are down.
        //Treat the address as a standard encrypted email address and get the existing certificates from the store.
        emailPublicResolver.GetCertificates(strAddressTest, listX509Certificate2sValid, listX509Certificate2sInvalid);
    }

    private static void DnsQueryForCert(IPAddress ipAddressDnsServer, MailAddress emailAddress, List<X509Certificate2> listX509Certificate2sDiscoveredActive, List<X509Certificate2> listX509Certificate2sDiscoveredInactive)
    {
        ICertificateResolver certResolverInternetDns =
            new DnsCertResolver(ipAddressDnsServer);
        var x509Certificate2Collection = certResolverInternetDns.GetCertificates(emailAddress); //Can return null.
        if (x509Certificate2Collection == null) return;

        //Certificates found via DNS.  Remove any invalid or expired certificates.
        for (var i = 0; i < x509Certificate2Collection.Count; i++)
        {
            if (DateTime.Now < x509Certificate2Collection[i].NotBefore || DateTime.Now > x509Certificate2Collection[i].NotAfter)
            {
                //If the certificate is not yet valid or is expired, then discard so we can possibly discover a better certificate below.
                listX509Certificate2sDiscoveredInactive.Add(x509Certificate2Collection[i]);
                continue;
            }

            listX509Certificate2sDiscoveredActive.Add(x509Certificate2Collection[i]);
        }
    }

    private static List<MimeEntity> GetMimeLeafNodes(Message message)
    {
        //Think of the mime structure as a tree.
        var listMimeEntitiesLeafNodes = new List<MimeEntity>();
        MimeEntity mimeEntity = null;
        try
        {
            mimeEntity = message.ExtractMimeEntity();
        }
        catch
        {
            return null;
        }

        //If GetParts() is called when IsMultiPart is false, then an exception will be thrown by the Direct library.
        if (!message.IsMultiPart)
        {
            //Single body part.
            listMimeEntitiesLeafNodes.Add(mimeEntity);
            return listMimeEntitiesLeafNodes;
        }

        var listMimeEntitiesMultiPart = new List<MimeEntity>();
        listMimeEntitiesMultiPart.Add(mimeEntity);
        while (true)
        {
            if (listMimeEntitiesMultiPart.Count == 0) break;

            var listMimeEntities = listMimeEntitiesMultiPart[0].GetParts().ToList();
            for (var i = 0; i < listMimeEntities.Count(); i++)
            {
                if (listMimeEntities[i].IsMultiPart)
                {
                    listMimeEntitiesMultiPart.Add(listMimeEntities[i]);
                    continue;
                }

                listMimeEntitiesLeafNodes.Add(listMimeEntities[i]);
            }

            listMimeEntitiesMultiPart.RemoveAt(0);
        }

        return listMimeEntitiesLeafNodes;
    }

    private static EmailMessage ConvertMessageToEmailMessage(Message message, bool hasAttachments, bool isOutbound)
    {
        var emailMessage = new EmailMessage();
        emailMessage.FromAddress = ProcessInlineEncodedText(message.FromValue.Trim());
        if (message.DateValue != null)
        {
            //Is null when sending, but should not be null when receiving.
            //The received email message date must be in a very specific format and must match the RFC822 standard.  Is a required field for RFC822.  http://tools.ietf.org/html/rfc822
            //We show the datetime that the email landed onto the email server instead of the datetime that the email was downloaded.
            //Examples: "3 Dec 2013 17:10:37 -0800", "10 Dec 2013 17:10:37 -0800", "Tue, 5 Nov 2013 17:10:37 +0000 (UTC)", "Tue, 12 Nov 2013 17:10:37 +0000 (UTC)"
            if (message.DateValue.EndsWith("GMT"))
            {
                //Examples: Tue, 09 Sep 2014 23:16:36 GMT
                emailMessage.MsgDateTime = DateTime.Parse(message.DateValue);
            }
            else
            {
                //Different email providers send the Date in formats that don't exactly match the RFC standard:  tools.ietf.org/html/rfc2822#section-3.3
                //This regular expression was created based off of all of the different types of date formats that have been officially whitnessed.
                //It is not based off of the RFC 2822 standard, as one would want to do.
                //A new unit test should be added for any scenarios where a 'valid' message.DateValue cannot parse correctly.
                var datePattern = @"^\s*(\S+,)?\s*(\d{1,2})\s*(\S+)\s*(\d{4,5})\s+(\d{1,2}):(\d{1,2})(:\d{1,2})?\s*([\+\-]\d+:?\d*)?(\s*\S+)?\s*$";
                var match = Regex.Match(message.DateValue, datePattern);
                if (!match.Success) throw new ApplicationException("DateValue was not recognized as a valid DateTime: " + message.DateValue);

                var dayOfWeekName = match.Result("$1"); //Mon, Tue, Wed, etc.
                var dayOfMonthNum = match.Result("$2");
                var monthName = match.Result("$3");
                var yearNum = match.Result("$4");
                var hourNum = match.Result("$5");
                var minuteNum = match.Result("$6");
                var secondNum = match.Result("$7");
                var utcOffset = match.Result("$8");
                var timeZoneAbbr = match.Result("$9"); //ex UTC, GMT, CST, MDT, etc.
                var dateFormat = "d MMM yyyy HH:mm";
                var dateValueConverted = dayOfMonthNum + " " + monthName + " " + yearNum + " " + hourNum.PadLeft(2, '0') + ":" + minuteNum.PadLeft(2, '0');
                if (!string.IsNullOrWhiteSpace(secondNum))
                {
                    dateFormat += ":ss";
                    dateValueConverted += ":" + secondNum.TrimStart(':').PadLeft(2, '0');
                }

                if (!string.IsNullOrWhiteSpace(utcOffset))
                {
                    dateFormat += " zzz";
                    dateValueConverted += " " + utcOffset;
                }

                if (!DateTime.TryParseExact(dateValueConverted, dateFormat, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None,
                        out emailMessage.MsgDateTime))
                    throw new ApplicationException("DateValue was not recognized as a valid DateTime.\r\n"
                                                   + "message.DateValue: " + message.DateValue + "\r\n"
                                                   + "dateValueConverted: " + dateValueConverted);
            }
        }
        else
        {
            //Sending the email.
            emailMessage.MsgDateTime = DateTime_.Now;
        }

        emailMessage.Subject = SubjectTidy(ProcessInlineEncodedText(message.SubjectValue));
        emailMessage.ToAddress = ProcessInlineEncodedText(SOut.String(message.ToValue).Trim()).Replace(@"\", ""); //ToValue can be null if recipients were CC or BCC only.
        emailMessage.CcAddress = ProcessInlineEncodedText(SOut.String(message.CcValue).Trim()).Replace(@"\", "");
        emailMessage.BccAddress = ProcessInlineEncodedText(SOut.String(message.BccValue).Trim()).Replace(@"\", "");
        //Think of the mime structure as a tree.
        //We want to treat one part and multi-part emails the same way below, so we make our own list of leaf node mime parts (mime parts which have no children, also know as single part).
        var listMimeEntitiesLeafNodes = GetMimeLeafNodes(message);
        if (listMimeEntitiesLeafNodes == null)
        {
            emailMessage.BodyText = ProcessMimeTextPart(message);
            return emailMessage;
        }

        var listMimeEntitiesBodyTextParts = new List<MimeEntity>();
        var listMimeEntitiesAttachParts = new List<MimeEntity>();
        for (var i = 0; i < listMimeEntitiesLeafNodes.Count; i++)
        {
            var mimeEntityPart = listMimeEntitiesLeafNodes[i];
            if (mimeEntityPart.ContentDisposition == null)
            {
                //Not an email attachment.  Treat as body text.
                listMimeEntitiesBodyTextParts.Add(mimeEntityPart);
                continue;
            }

            //An email attachment. Leaf node. https://www.ietf.org/rfc/rfc2183.txt we treat both 'attachment' and 'inline' the same. No other options.
            listMimeEntitiesAttachParts.Add(mimeEntityPart);
        }

        var strTextPartBoundary = "";
        if (listMimeEntitiesBodyTextParts.Count > 1) strTextPartBoundary = message.ParsedContentType.Boundary;

        var stringBuilderBodyText = new StringBuilder("");
        if (isOutbound)
        {
            for (var i = 0; i < listMimeEntitiesBodyTextParts.Count; i++)
            {
                if (strTextPartBoundary != "")
                {
                    //For outgoing Direct Ack messages.
                    stringBuilderBodyText.Append("\r\n--" + strTextPartBoundary + "\r\n");
                    stringBuilderBodyText.Append(listMimeEntitiesBodyTextParts[i]); //Includes not only the body text, but also content type and content disposition.
                    continue;
                }

                stringBuilderBodyText.Append(ProcessMimeTextPart(listMimeEntitiesBodyTextParts[i]));
            }

            if (strTextPartBoundary != "") stringBuilderBodyText.Append("\r\n--" + strTextPartBoundary + "--\r\n");
        }
        else
        {
            //All plain text body parts will show to the user in the chart module progress notes.
            for (var i = 0; i < listMimeEntitiesBodyTextParts.Count; i++)
                if (IsMimeEntityTextPlain(listMimeEntitiesBodyTextParts[i]))
                    stringBuilderBodyText.Append(ProcessMimeTextPart(listMimeEntitiesBodyTextParts[i]));
        }

        emailMessage.BodyText = stringBuilderBodyText.ToString();
        emailMessage.Attachments = new List<EmailAttach>();
        if (!hasAttachments) return emailMessage;

        //If an encrypted attachment is present (smime.p7m), then ensure the message content type correctly indicates an encrypted message.
        for (var i = 0; i < listMimeEntitiesAttachParts.Count; i++)
        {
            if (listMimeEntitiesAttachParts[i].ParsedContentType.Name == null) continue;

            if (listMimeEntitiesAttachParts[i].ParsedContentType.Name.ToLower() != "smime.p7m") continue;

            //encrypted attachment
            message.ContentType = "application/pkcs7-mime; name=smime.p7m; boundary=" + strTextPartBoundary + ";";
            break;
        }

        try
        {
            for (var i = 0; i < listMimeEntitiesAttachParts.Count; i++)
            {
                byte[] byteArrayData = null;
                try
                {
                    if (IsMimeEntityBase64(listMimeEntitiesAttachParts[i])) byteArrayData = Convert.FromBase64String(listMimeEntitiesAttachParts[i].Body.Text);
                }
                catch
                {
                }

                if (byteArrayData == null)
                    //Plain attachment.
                    byteArrayData = Encoding.UTF8.GetBytes(listMimeEntitiesAttachParts[i].Body.Text);

                var displayFileName = listMimeEntitiesAttachParts[i].ParsedContentType.Name;
                //If the name directive was not set, check for the filename directive.
                //The filename is always optional and must not be used blindly by the application: path information should be stripped, and conversion to 
                //the server file system rules should be done. This parameter provides mostly indicative information. When used in combination with 
                //Content-Disposition: attachment, it is used as the default filename for an eventual "Save As" dialog presented to the user.
                if (string.IsNullOrEmpty(displayFileName)
                    && !string.IsNullOrWhiteSpace(listMimeEntitiesAttachParts[i].ContentDisposition)
                    && listMimeEntitiesAttachParts[i].ContentDisposition.Contains("filename"))
                {
                    //E.g. Content-Disposition: attachment; filename="cool.html" should suggest saving under the "cool.html" filename (by default).
                    var match = Regex.Match(listMimeEntitiesAttachParts[i].ContentDisposition, @"filename[\t ]*=[\t ]*""(.*)""");
                    if (match.Success && match.Groups != null && match.Groups.Count > 1) displayFileName = ODFileUtils.CleanFileName(match.Groups[1].Value);
                }

                var emailAttach = EmailAttaches.CreateAttach(displayFileName, "", byteArrayData, isOutbound);
                emailMessage.Attachments.Add(emailAttach); //The attachment EmailMessageNum is set when the emailMessage is inserted/updated below.
            }
        }
        catch (Exception)
        {
            //Failed to extract all attachments from the email message.  Cleanup the attachments which were successfully extracted.
            for (var i = 0; i < emailMessage.Attachments.Count; i++)
            {
                var attachFilePath = Path.Combine(EmailAttaches.GetAttachPath(), emailMessage.Attachments[i].ActualFileName);
                if (!File.Exists(attachFilePath)) continue;

                try
                {
                    File.Delete(attachFilePath);
                }
                catch
                {
                    //Probably nothing else we can do.  At least continue to the remaining attachments to try deleting them as well.
                }
            }

            throw;
        }

        return emailMessage;
    }

    private static Message ConvertEmailMessageToMessage(EmailMessage emailMessage, bool hasAttachments)
    {
        //We need to use emailAddressFrom.Username instead of emailAddressFrom.SenderAddress, because of how strict encryption is for matching the name to the certificate.
        var message = new Message();
        if (!string.IsNullOrWhiteSpace(emailMessage.ToAddress)) message.To = new Header("To", emailMessage.ToAddress.Trim());

        message.From = new Header("From", emailMessage.FromAddress.Trim());
        //message.Body is set below.
        message.ContentType = "text/plain"; //Setting the default content type helps with signing.
        if (!string.IsNullOrWhiteSpace(emailMessage.CcAddress)) message.CcValue = emailMessage.CcAddress.Trim(); //constructor does not accept cc and bcc values

        if (!string.IsNullOrWhiteSpace(emailMessage.BccAddress)) message.BccValue = emailMessage.BccAddress.Trim();

        var subject = SubjectTidy(emailMessage.Subject);
        if (subject != "")
        {
            var headerSubject = new Header("Subject", subject);
            message.Headers.Add(headerSubject);
        }

        //The Transport Testing Tool (TTT) complained when we sent a message that was not wrapped.
        //It appears that wrapped messages are preferred when sending a message, although support for incoming wrapped messages is optional (unwrapped is required).  We support both unwrapped and wrapped.
        //Specifically, the tool looks for the headers Orig-Date and Message-Id after the message is decrypted, so we need to include these two headers before encrypting an outgoing email.
        //The message date must be in a very specific format and must match the RFC822 standard.  Is a required field for RFC822.  http://tools.ietf.org/html/rfc822
        var strOrigDate = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss zzz"); //Example: "Tue, 12 Nov 2013 17:10:37 +08:00", which has an extra colon in the Zulu offset.
        strOrigDate = strOrigDate.Remove(strOrigDate.LastIndexOf(':'), 1); //Remove the colon from the Zulu offset, as required by the RFC 822 message format.
        message.Date = new Header("Date", strOrigDate); //http://tools.ietf.org/html/rfc5322#section-3.6.1
        message.AssignMessageID(); //http://tools.ietf.org/html/rfc5322#section-3.6.4
        var strBoundry = "";
        var listMimeEntitiesPart = new List<MimeEntity>();
        var bodyText = BodyTidy(emailMessage.BodyText);
        if (bodyText.Trim().Length > 4 && bodyText.Trim().StartsWith("--") && bodyText.Trim().EndsWith("--"))
        {
            //The body text is multi-part.
            strBoundry = bodyText.Trim().Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None)[0];
            var listBodyTextParts = bodyText.Trim().TrimEnd('-').Split(new[] {strBoundry}, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var i = 0; i < listBodyTextParts.Count; i++)
            {
                var mimeEntityBodyText = new MimeEntity(listBodyTextParts[i]);
                mimeEntityBodyText.ContentType = "text/plain;";
                listMimeEntitiesPart.Add(mimeEntityBodyText);
            }
        }
        else
        {
            var mimeEntityBodyText = new MimeEntity(bodyText);
            mimeEntityBodyText.ContentType = "text/plain;";
            listMimeEntitiesPart.Add(mimeEntityBodyText);
        }

        if (hasAttachments && emailMessage.Attachments != null && emailMessage.Attachments.Count > 0)
        {
            var strAttachPath = EmailAttaches.GetAttachPath();
            for (var i = 0; i < emailMessage.Attachments.Count; i++)
            {
                var strAttachFile = Path.Combine(strAttachPath, emailMessage.Attachments[i].ActualFileName);
                //We always attach with base64 encoding, so that we do not have to worry about violating the RFC822 email format with binary characters or invalid newlines.
                var mimeEntityAttach = new MimeEntity(Convert.ToBase64String(File.ReadAllBytes(strAttachFile)));
                mimeEntityAttach.ContentTransferEncoding = "base64";
                mimeEntityAttach.ContentDisposition = "attachment; filename=\"" + emailMessage.Attachments[i].DisplayedFileName + "\"";
                mimeEntityAttach.ContentType = Mime.GetMimeTypeForEmail(strAttachFile) + "; name=\"" + emailMessage.Attachments[i].DisplayedFileName + "\"";
                listMimeEntitiesPart.Add(mimeEntityAttach);
            }
        }

        if (strBoundry == "") strBoundry = MiscUtils.CreateRandomAlphaNumericString(32);

        if (listMimeEntitiesPart.Count == 1)
        {
            //Single body part
            message.Body = listMimeEntitiesPart[0].Body;
            return message;
        }

        if (listMimeEntitiesPart.Count > 1)
            //multiple body parts
            message.SetParts(listMimeEntitiesPart, "multipart/mixed; boundary=" + strBoundry + ";");

        return message;
    }

    public static string ProcessInlineEncodedText(string text)
    {
        //str must be in "=?bodycharset?[B,Q,iso-8859-1,etc]?input?=" format for Attachment to properly decode non-ascii chars.  This is the case for 
        //the email subject line, and to/from/cc/bcc addresses but not for the body, which is why we decode the body differently.  
        //Ex. =?UTF-8?B?RndkOiDCoiDDhiAxMjM0NSDDpiDDvyBzb21lIGFzY2lpIGNoYXJzIMOCIMOD?= decodes to "Fwd: � � 12345 � � some ascii chars � �"
        //=?UTF-8?Q?nu=C2=A4=20=C3=82=20=C3=80=20=C2=A2?= decodes to "nu� � � �"
        MatchEvaluator matchEvaluator = match =>
        {
            var charsetStr = match.Result("$1");
            var encodingStr = match.Result("$2");
            var encodedTextStr = match.Result("$3");
            Encoding encoding;
            if (charsetStr.ToLower() == "cp1252")
                encoding = Encoding.GetEncoding("Windows-1252");
            else
                encoding = Encoding.GetEncoding(charsetStr);

            if (encodingStr.ToUpper() == "B")
            {
                //Treat the encodedTextStr as BASE64
                var byteArray = Convert.FromBase64String(encodedTextStr);
                return encoding.GetString(byteArray);
            }

            //Q is the only other option which is similar to "Quoted-Pritable" which is designed to allow text containingly mostly ASCII.
            //Send the encoded text through DecodeBodyText() since any 8-bit value may be represented by a "=" followed by two hexadecimal digits.
            //However, The 8-bit hexadecimal value 20 (e.g., IS0-8859-1 SPACE) may be represented as "_" (underscore, ASCII 95.).
            //This means that we need to always replace '_' with ' ' prior to decoding it.  https://tools.ietf.org/html/rfc1342
            var encodedTextStrScrubbed = encodedTextStr.Replace('_', ' ');
            return DecodeBodyText("=", encodedTextStrScrubbed, encoding);
        };
        if (text == null) text = "";

        return Regex.Replace(text, @"=\?([^?]+)\?([^?]+)\?([^?]+)\?=", matchEvaluator);
    }

    public static string ProcessMimeTextPart(MimeEntity mimeEntity)
    {
        var strBodyText = mimeEntity.Body.Text;
        //Convert clear text mime parts which are base64 encoded into utf8 to make the text readable (plain text, html, xml, etc...)
        //This includes messages which were received as encryped and which were successfully decrypted.
        var encoding = Encoding.GetEncoding("utf-8");
        ODException.SwallowAnyException(() => encoding = GetMimeEncoding(mimeEntity));
        if (!IsMimeEntityEncrypted(mimeEntity) && IsMimeEntityText(mimeEntity) && IsMimeEntityBase64(mimeEntity))
        {
            var byteArrayBody = Convert.FromBase64String(mimeEntity.Body.Text);
            strBodyText = encoding.GetString(byteArrayBody);
        }

        //Official documentation regarding text wrapping.  http://www.ietf.org/rfc/rfc2646.txt
        //Both text and html bodies appear to be commonly wrapped at 75 characters with an extra '=' character added to the end of wrapped lines.
        //We have seen email wrapped at 75 characters from a number of sources, including GoDaddy and Comodo.
        //However, lines may be wrapped at any number of characters, so we cannot rely on the length of the line.
        //Instead we rely on the presence of a "soft line break" (a special character SP followed by CRLF).
        //I hard line break is a CRLF which is not preceded by the SP character.
        //The SP character can be any character, and from what we have seen, is usually the '=' character.
        var sp = "="; //Soft line break indicator character.
        var listMimeBodyLines = strBodyText.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).ToList();
        var stringBuilderBodyText = new StringBuilder();
        for (var i = 0; i < listMimeBodyLines.Count; i++)
        {
            if (listMimeBodyLines[i].EndsWith(sp))
            {
                //Soft line break.  The line ends with SP CRLF
                //The current line is wrapped.  Remove the trailing soft line break indicator character and also remove the new line.
                //The CRLF was already removed when splitting, so we only need to remove the soft line break indicator at the end.
                stringBuilderBodyText.Append(listMimeBodyLines[i].Substring(0, listMimeBodyLines[i].Length - 1));
                continue;
            }

            //Hard line break.
            //The current line is not wrapped.  Do not modify this line.  Also ensure that the CRLF is placed back into the output.
            stringBuilderBodyText.Append(listMimeBodyLines[i]);
            if (i < listMimeBodyLines.Count) stringBuilderBodyText.AppendLine();
        }

        //Soft line breaks have now been removed from the message.
        return DecodeBodyText(sp, stringBuilderBodyText.ToString(), encoding);
    }

    public static string DecodeBodyText(string sp, string strBodyTextUnwrapped, Encoding encoding)
    {
        var listBodyEncodeds = strBodyTextUnwrapped.Split(new[] {sp}, StringSplitOptions.None).ToList();
        var listBytes = new List<byte>();
        if (listBodyEncodeds.Count == 0) return encoding.GetString(listBytes.ToArray());

        listBytes.AddRange(encoding.GetBytes(listBodyEncodeds[0]));
        //In the remaining message, the same special character is used to precede encoded characters.
        //For example, "=3D" needs to be converted to an '=' character, because 3D in hexadecimal is the '=' character.
        //Another example, "=20" would be converted to a ' ' character.
        for (var i = 1; i < listBodyEncodeds.Count; i++)
        {
            if (Regex.IsMatch(listBodyEncodeds[i], "^[0-9A-F]{2}.*"))
            {
                //Starts with a 2 digit hexadecimal number.
                var hexStr = listBodyEncodeds[i].Substring(0, 2);
                listBytes.Add(Convert.ToByte(hexStr, 16)); //Format provider of 16 means convert from base 16.
                listBytes.AddRange(encoding.GetBytes(listBodyEncodeds[i].Substring(2)));
                continue;
            }

            //This loop can, and will, remove more than just "=3D", or similiar encoded characters. It will also remove "=" from necessary html code 
            //such as alt and src. Appending sp here allows it to be put back into the code when sp was not followed by two hex characters.
            listBytes.AddRange(encoding.GetBytes(sp));
            listBytes.AddRange(encoding.GetBytes(listBodyEncodeds[i]));
        }

        return encoding.GetString(listBytes.ToArray());
    }

    private static Encoding GetMimeEncoding(MimeEntity mimeEntity)
    {
        if (mimeEntity.ContentType == null)
            //The body of a message is simply lines of US-ASCII characters.
            //Default to UTF-8, which can handle US-ASCII characters, because it can interpret more characters just in case the standard is not followed.
            return Encoding.UTF8;

        //However, mime entities can specify other types of encoding via the content type.
        //We cannot use mimeEntity.ParsedContentType.CharSet because it fails if there are spaces around the equal sign of the charset statement.
        var contentType = mimeEntity.ContentType.ToLower();
        var charsetIndex = contentType.IndexOf("charset");
        if (charsetIndex < 0) throw new ApplicationException("Mime encoding not specified.");

        charsetIndex = contentType.IndexOf("=", charsetIndex + 7); //Find the '=' after the "charset" string (ignoring spaces).
        if (charsetIndex < 0) throw new ApplicationException("Mime encoding incorrectly specified.");

        charsetIndex++; //Skip '='
        while (charsetIndex < contentType.Length && char.IsWhiteSpace(contentType[charsetIndex]))
            //Skip white space after '='
            charsetIndex++;

        var encName = "";
        while (true)
        {
            if (charsetIndex >= contentType.Length) break;

            if (contentType[charsetIndex] == ';') break;

            if (char.IsWhiteSpace(contentType[charsetIndex])) break;

            encName += contentType[charsetIndex];
            charsetIndex++;
        }

        encName = encName.Replace("\"", ""); //Remove double-quotes
        Encoding encoding = null;
        try
        {
            encoding = Encoding.GetEncoding(encName);
        }
        catch
        {
            if (encName.ToLower().StartsWith("cp"))
            {
                var codePage = encName.Substring(2);
                encoding = Encoding.GetEncoding(int.Parse(codePage));
            }
        }

        return encoding;
    }

    private static bool IsMimeEntityEncrypted(MimeEntity mimeEntity)
    {
        if (mimeEntity.ContentType == null) return false;

        if (!mimeEntity.ContentType.ToLower().Contains("application/pkcs7-mime")) return false;

        return true; //The email MIME/body is encrypted (known as S/MIME).  Treated as an Encrypted/Direct message.
    }

    private static bool IsMimeEntityBase64(MimeEntity mimeEntity)
    {
        if (mimeEntity.ContentTransferEncoding == null) return false;

        if (!mimeEntity.ContentTransferEncoding.ToLower().Contains("base64")) return false;

        return true;
    }

    private static bool IsMimeEntityText(MimeEntity mimeEntity)
    {
        if (mimeEntity.ContentType == null) return false;

        if (!mimeEntity.ContentType.ToLower().Contains("text/")) return false;

        return true;
    }

    private static bool IsMimeEntityTextPlain(MimeEntity mimeEntity)
    {
        if (mimeEntity.ContentType == null) return false;

        if (!mimeEntity.ContentType.ToLower().Contains("text/plain")) return false;

        return true;
    }

    public static string GetEmailSentOrReceivedDescript(EmailSentOrReceived sentOrReceived)
    {
        if (IsRegularEmail(sentOrReceived)) return Lans.g("EmailMessages", "Regular Email");

        if (IsEncryptedEmail(sentOrReceived)) return Lans.g("EmailMessages", "Encrypted Email");

        if (IsSecureWebMail(sentOrReceived)) return Lans.g("EmailMessages", "Secure Web Mail");

        if (IsSecureEmail(sentOrReceived)) return Lans.g("EmailMessages", "Secure Email");

        if (IsUnsent(sentOrReceived)) return Lans.g("EmailMessages", "Unsent");

        return "";
    }

    public static bool IsRegularEmail(EmailSentOrReceived emailSentOrReceived)
    {
        return IsEmailType(emailSentOrReceived, EmailPlatform.Unsecure);
    }

    public static bool IsEncryptedEmail(EmailSentOrReceived emailSentOrReceived)
    {
        return IsEmailType(emailSentOrReceived, EmailPlatform.Direct | EmailPlatform.Ack) || GetUnsentTypes(EmailPlatform.Ack).Contains(emailSentOrReceived);
    }

    public static bool IsSecureWebMail(EmailSentOrReceived emailSentOrReceived)
    {
        return IsEmailType(emailSentOrReceived, EmailPlatform.WebMail);
    }

    public static bool IsSecureEmail(EmailSentOrReceived emailSentOrReceived)
    {
        return IsEmailType(emailSentOrReceived, EmailPlatform.Secure);
    }

    private static bool IsEmailType(EmailSentOrReceived emailSentOrReceived, EmailPlatform emailPlatform)
    {
        var listEmailSentOrReceivedsSecureTypes = GetUnreadTypes(emailPlatform)
            .Concat(GetReadTypes(emailPlatform))
            .Concat(GetSentTypes(emailPlatform))
            .ToList();
        return listEmailSentOrReceivedsSecureTypes.Contains(emailSentOrReceived);
    }

    public static bool IsUnsent(EmailSentOrReceived emailSentOrReceived)
    {
        var listEmailSentOrReceivedsReceivedTypes = GetUnsentTypes(EmailPlatform.All);
        return listEmailSentOrReceivedsReceivedTypes.Contains(emailSentOrReceived);
    }

    public static bool IsReceived(EmailSentOrReceived emailSentOrReceived)
    {
        var listEmailSentOrReceivedsReceivedTypes = GetUnreadTypes().Concat(GetReadTypes()).ToList();
        return listEmailSentOrReceivedsReceivedTypes.Contains(emailSentOrReceived);
    }

    public static bool IsUnread(EmailSentOrReceived emailSentOrReceived)
    {
        var listEmailSentOrReceivedsUnreadTypes = GetUnreadTypes();
        return listEmailSentOrReceivedsUnreadTypes.Contains(emailSentOrReceived);
    }

    public static bool IsSent(EmailSentOrReceived emailSentOrReceived)
    {
        var listEmailSentOrReceivedsSentTypes = GetSentTypes();
        return listEmailSentOrReceivedsSentTypes.Contains(emailSentOrReceived);
    }

    public static List<EmailSentOrReceived> GetUnsentTypes(EmailPlatform emailPlatform)
    {
        var listEmailSentOrReceivedsUnsent = new List<EmailSentOrReceived>();
        if (emailPlatform == EmailPlatform.Ack)
        {
            listEmailSentOrReceivedsUnsent.Add(EmailSentOrReceived.AckDirectNotSent);
            return listEmailSentOrReceivedsUnsent;
        }

        listEmailSentOrReceivedsUnsent.Add(EmailSentOrReceived.Neither);
        return listEmailSentOrReceivedsUnsent;
    }

    public static List<EmailSentOrReceived> GetUnreadTypes(EmailPlatform emailPlatform = EmailPlatform.All)
    {
        var listEmailSentOrReceivedsUnread = new List<EmailSentOrReceived>();
        if (emailPlatform.HasFlag(EmailPlatform.Unsecure)) listEmailSentOrReceivedsUnread.Add(EmailSentOrReceived.Received);

        if (emailPlatform.HasFlag(EmailPlatform.Direct))
        {
            listEmailSentOrReceivedsUnread.Add(EmailSentOrReceived.ReceivedEncrypted);
            listEmailSentOrReceivedsUnread.Add(EmailSentOrReceived.ReceivedDirect);
        }

        if (emailPlatform.HasFlag(EmailPlatform.WebMail)) listEmailSentOrReceivedsUnread.Add(EmailSentOrReceived.WebMailReceived);

        if (emailPlatform.HasFlag(EmailPlatform.Secure)) listEmailSentOrReceivedsUnread.Add(EmailSentOrReceived.SecureEmailReceivedUnread);

        return listEmailSentOrReceivedsUnread;
    }

    public static List<EmailSentOrReceived> GetReadTypes(EmailPlatform emailPlatform = EmailPlatform.All)
    {
        var listEmailSentOrReceivedsRead = new List<EmailSentOrReceived>();
        if (emailPlatform.HasFlag(EmailPlatform.Unsecure)) listEmailSentOrReceivedsRead.Add(EmailSentOrReceived.Read);

        if (emailPlatform.HasFlag(EmailPlatform.Direct)) listEmailSentOrReceivedsRead.Add(EmailSentOrReceived.ReadDirect);

        if (emailPlatform.HasFlag(EmailPlatform.WebMail)) listEmailSentOrReceivedsRead.Add(EmailSentOrReceived.WebMailRecdRead);

        if (emailPlatform.HasFlag(EmailPlatform.Secure)) listEmailSentOrReceivedsRead.Add(EmailSentOrReceived.SecureEmailReceivedRead);

        return listEmailSentOrReceivedsRead;
    }

    public static List<EmailSentOrReceived> GetSentTypes(EmailPlatform emailPlatform = EmailPlatform.All, bool doIncludeFails = false)
    {
        var listEmailSentOrReceivedsSent = new List<EmailSentOrReceived>();
        if (emailPlatform.HasFlag(EmailPlatform.Unsecure)) listEmailSentOrReceivedsSent.Add(EmailSentOrReceived.Sent);

        if (emailPlatform.HasFlag(EmailPlatform.Direct)) listEmailSentOrReceivedsSent.Add(EmailSentOrReceived.SentDirect);

        if (emailPlatform.HasFlag(EmailPlatform.WebMail))
        {
            listEmailSentOrReceivedsSent.Add(EmailSentOrReceived.WebMailSent);
            listEmailSentOrReceivedsSent.Add(EmailSentOrReceived.WebMailSentRead);
        }

        if (emailPlatform.HasFlag(EmailPlatform.Secure)) listEmailSentOrReceivedsSent.Add(EmailSentOrReceived.SecureEmailSent);

        if (emailPlatform.HasFlag(EmailPlatform.Ack)) listEmailSentOrReceivedsSent.Add(EmailSentOrReceived.AckDirectProcessed);

        if (doIncludeFails) listEmailSentOrReceivedsSent.Add(EmailSentOrReceived.SendFailed);

        return listEmailSentOrReceivedsSent;
    }
    
    public static EmailMessage CreateReply(EmailMessage emailMessageReceived, EmailAddress emailAddress, bool isReplyAll = false)
    {
        var emailMessageReply = new EmailMessage();
        emailMessageReply.PatNum = emailMessageReceived.PatNum;
        var isSecureEmail = IsSecureEmail(emailMessageReceived.SentOrReceived);
        FillEmailAddressesForReply(emailMessageReply, emailMessageReceived, emailAddress, isReplyAll);
        if (!isSecureEmail && isReplyAll) FillCCAddressesForReply(emailMessageReply, emailMessageReceived, emailAddress);

        var subject = ProcessInlineEncodedText(emailMessageReceived.Subject);
        if (subject.Trim().Length >= 3 && subject.Trim().Substring(0, 3).ToLower() == "re:")
            //already contains a "re:"
            emailMessageReply.Subject = subject;
        else
            //doesn't contain a "re:"
            emailMessageReply.Subject = "RE: " + subject;

        emailMessageReply = SetForwardOrReplyBody(emailMessageReply, emailMessageReceived, emailAddress);
        emailMessageReply.MsgType = EmailMessageSource.Reply;
        return emailMessageReply;
    }

    public static void FillEmailAddressesForReply(EmailMessage emailMessageReply, EmailMessage emailMessageReceived, EmailAddress emailAddressSender, bool isReplyAll)
    {
        emailMessageReply.ToAddress = ProcessInlineEncodedText(emailMessageReceived.FromAddress);
        if (!isReplyAll)
        {
            emailMessageReply.FromAddress = ProcessInlineEncodedText(emailMessageReceived.RecipientAddress);
            return;
        }

        var listStringEmails = emailMessageReceived.ToAddress.Split(',').ToList();
        for (var i = 0; i < listStringEmails.Count; i++)
        {
            listStringEmails[i] = ProcessInlineEncodedText(listStringEmails[i]).Trim(); //Decode any UTF-8 or otherwise
            if (listStringEmails[i].ToLower().Contains(emailAddressSender.EmailUsername.ToLower())) continue;

            if (!emailAddressSender.SenderAddress.IsNullOrEmpty() && listStringEmails[i].ToLower().Contains(emailAddressSender.SenderAddress.ToLower())) continue;

            //Since we are replying, remove our current email from list
            emailMessageReply.ToAddress += "," + listStringEmails[i];
        }

        emailMessageReply.FromAddress = ProcessInlineEncodedText(emailMessageReceived.RecipientAddress);
    }

    public static void FillCCAddressesForReply(EmailMessage emailMessageReply, EmailMessage emailMessageReceived, EmailAddress emailAddressSender)
    {
        if (emailMessageReceived.CcAddress.IsNullOrEmpty()) return;

        emailMessageReply.CcAddress = "";
        //email@od.com,email2@od.com,...
        //Since we are replying, remove our current email from list. Also decode any UTF-8 or otherwise
        var temp = emailMessageReceived.CcAddress.Split(',')
            .Select(x => ProcessInlineEncodedText(x).Trim())
            .ToList()
            .FindAll(x => !x.ToLower().Contains(emailAddressSender.EmailUsername.ToLower())
                          || (!emailAddressSender.SenderAddress.IsNullOrEmpty() && !x.ToLower().Contains(emailAddressSender.SenderAddress.ToLower())));
        //Loop through the email addresses, combining them into a comma separated list string.
        for (var i = 0; i < temp.Count; i++)
        {
            //First address
            if (i == 0)
            {
                emailMessageReply.CcAddress += temp[i];
                continue;
            }

            //All other addresses
            emailMessageReply.CcAddress += "," + temp[i];
        }
    }

    public static EmailMessage CreateForward(EmailMessage emailMessageReceived, EmailAddress emailAddress)
    {
        var emailMessageForward = new EmailMessage();
        emailMessageForward.PatNum = emailMessageReceived.PatNum;
        emailMessageForward.FromAddress = emailAddress.EmailUsername; //We cannot use emailAddress.SenderAddress here in case the user wants to send a Direct message.
        var subject = ProcessInlineEncodedText(emailMessageReceived.Subject);
        if (subject.Trim().ToLower().StartsWith("fwd:"))
            //already contains a "fwd:"
            emailMessageForward.Subject = subject;
        else
            //doesn't contain a "fwd:"
            emailMessageForward.Subject = "FWD: " + subject;

        emailMessageForward = SetForwardOrReplyBody(emailMessageForward, emailMessageReceived, emailAddress);
        emailMessageForward.MsgType = EmailMessageSource.Forward;
        return emailMessageForward;
    }

    private static EmailMessage SetForwardOrReplyBody(EmailMessage emailMessage, EmailMessage emailMessageReceived, EmailAddress emailAddress)
    {
        var bodyTextHeader = "\r\n\r\n\r\nOn " + emailMessageReceived.MsgDateTime + " " + ProcessInlineEncodedText(emailMessageReceived.FromAddress) + " sent:\r\n";
        string bodyText;
        var listMimeEntitiesPart =
            GetMimePartsForMimeTypes(emailMessageReceived.RawEmailIn, emailAddress, "text/html", "text/plain", "image/", "application/octet-stream");
        var listMimeEntitiesPartHtml = listMimeEntitiesPart[0]; //If RawEmailIn is blank, then this list will also be blank (ex Secure Web Mail messages).
        var listMimeEntitiesPartText = listMimeEntitiesPart[1]; //If RawEmailIn is blank, then this list will also be blank (ex Secure Web Mail messages).
        var listMimeEntriesPartImages = new List<MimeEntity>();
        listMimeEntriesPartImages.AddRange(listMimeEntitiesPart[2]); //If RawEmailIn is blank, then this list will also be blank (ex Secure Web Mail messages).
        listMimeEntriesPartImages.AddRange(listMimeEntitiesPart[3]); //If RawEmailIn is blank, then this list will also be blank (ex Secure Web Mail messages).
        if (listMimeEntriesPartImages.Count > 0)
        {
            //Email has images that need to be displayed
            bodyTextHeader = bodyTextHeader.Replace("<", "&<"); //& is our internal escape character for html used in MarkupEdit.TranslateToXhtml()
            bodyTextHeader = bodyTextHeader.Replace(">", "&>"); //& is our internal escape character for html used in MarkupEdit.TranslateToXhtml()
            if (listMimeEntitiesPartText.Count > 0)
                bodyText = ProcessMimeTextPart(listMimeEntitiesPartText[0]);
            else
                bodyText = emailMessageReceived.BodyText;

            bodyText = bodyTextHeader + bodyText;
            for (var i = 0; i < listMimeEntriesPartImages.Count; i++)
            {
                //Similar logic to EmailPreviewControl.ParseAndSaveAttachments()
                var fileName = GetMimeImageFileName(listMimeEntriesPartImages[i]);
                if (!ImageStore.HasImageExtension(fileName))
                    //Check file format against known image format extensions.
                    continue; //Don't show any that are not a known image format

                bodyText = bodyText.Replace($"[cid:{fileName}]", $"[[img:{fileName}]]"); //Change inline image to image link.
            }

            var rawEmailText = "";
            try
            {
                rawEmailText = MarkupEdit.TranslateToXhtml(bodyText, isEmail: true);
            }
            catch (Exception e)
            {
            }

            bodyText = rawEmailText;
            emailMessage.AreImagesDownloaded = true;
            emailMessage.Attachments = emailMessageReceived.Attachments;
            emailMessage.RawEmailIn = bodyText;
            emailMessage.HtmlType = EmailType.RawHtml;
        }
        else
        {
            if (listMimeEntitiesPartHtml.Count > 0)
                //Html body found.
                bodyText = HttpUtility.HtmlDecode(Regex.Replace(ProcessMimeTextPart(listMimeEntitiesPartHtml[0]), @"<(.|\n)*?>", "")); //remove most html tags.
            else if (listMimeEntitiesPartText.Count > 0)
                //No html body found, however one specific mime part is for viewing in text only.
                bodyText = HttpUtility.HtmlDecode(ProcessMimeTextPart(listMimeEntitiesPartText[0]));
            else
                //No html body found and no text body found.  Last resort.  Show all mime parts which are not attachments (ugly).
                bodyText = emailMessageReceived.BodyText; //This version of the body text includes all non-attachment mime parts.

            bodyText = bodyText.Trim().Replace("\r\n", ".!(-&>"); //replace with a string that would very very seldom happen.
            bodyText = bodyText.Trim().Replace("\n", ".!(-&>");
            bodyText = bodyText.Trim().Replace("\r", ".!(-&>");
            bodyText = bodyText.Trim().Replace(".!(-&>", "\r\n>");
            bodyText = bodyTextHeader + ">" + bodyText;
        }

        emailMessage.BodyText = bodyText;
        return emailMessage;
    }

    public static void PrepHtmlEmail(EmailMessage emailMessage)
    {
        if (emailMessage.HtmlType != EmailType.RawHtml)
            if (!MarkupEdit.ContainsOdHtmlTags(emailMessage.BodyText))
            {
                //OD will not allow the user to save the email edit window if they have included a '<' or '>' without prefixing '&'.
                emailMessage.BodyText = emailMessage.BodyText.Replace("&<", "<");
                emailMessage.BodyText = emailMessage.BodyText.Replace("&>", ">");
                emailMessage.HtmlType = EmailType.Regular;
                return;
            }

        if (emailMessage.HtmlType == EmailType.RawHtml)
        {
            emailMessage.HtmlText = emailMessage.BodyText;
            emailMessage.AreImagesDownloaded = true;
            return;
        }

        emailMessage.HtmlText = MarkupEdit.TranslateToXhtml(emailMessage.BodyText, true);
        emailMessage.HtmlType = EmailType.Html;
        emailMessage.AreImagesDownloaded = true;
    }

    public static EmailMessage CreateEmailMessageForStatement(Statement statement, Patient patient, string patFolder, Document document = null)
    {
        if (document == null) document = Documents.GetByNum(statement.DocNum);

        var attachPath = EmailAttaches.GetAttachPath();
        var random = new Random();
        var fileName = DateTime.Now.ToString("yyyyMMdd") + DateTime.Now.TimeOfDay.Ticks + random.Next(1000) + ".pdf";
        var filePathAndName = Path.Combine(attachPath, fileName);
        File.Copy(ImageStore.GetFilePath(document, patFolder), filePathAndName);
        var emailMessage = Statements.GetEmailMessageForStatement(statement, patient);
        var emailAttach = new EmailAttach();
        emailAttach.DisplayedFileName = "Statement.pdf";
        emailAttach.ActualFileName = fileName;
        emailMessage.Attachments.Add(emailAttach);
        return emailMessage;
    }

    public static void SendTestUnsecure(string subjectAndBody, string attachName, string attachContents)
    {
        SendTestUnsecure(subjectAndBody, attachName, attachContents, "", "");
    }

    public static void SendTestUnsecure(string subjectAndBody, string attachName1, string attachContents1, string attachName2, string attachContents2)
    {
        var strTo = PrefC.GetString(PrefName.EHREmailToAddress);
        if (strTo == "") throw new ApplicationException("This feature cannot be used except in a test environment because email is not secure.");

        var emailAddressFrom = EmailAddresses.GetByClinic(0);
        var emailMessage = new EmailMessage();
        emailMessage.FromAddress = emailAddressFrom.EmailUsername.Trim();
        emailMessage.ToAddress = strTo.Trim();
        emailMessage.Subject = subjectAndBody;
        emailMessage.BodyText = subjectAndBody;
        if (attachName1 != "")
        {
            var emailAttach = EmailAttaches.CreateAttach(attachName1, Encoding.UTF8.GetBytes(attachContents1));
            emailMessage.Attachments.Add(emailAttach);
        }

        if (attachName2 != "")
        {
            var emailAttach = EmailAttaches.CreateAttach(attachName2, Encoding.UTF8.GetBytes(attachContents2));
            emailMessage.Attachments.Add(emailAttach);
        }

        emailMessage.SentOrReceived = EmailSentOrReceived.Sent;
        emailMessage.MsgDateTime = DateTime_.Now;
        emailMessage.MsgType = EmailMessageSource.EHR;
        SendEmail(emailMessage, emailAddressFrom);
    }
}

public class EmailPublicResolver(bool isReadOnly = true) : EmailNameResolver(isReadOnly ? SystemX509Store.OpenExternal() : SystemX509Store.OpenExternalEdit());

public class EmailPrivateResolver(bool isReadOnly = true) : EmailNameResolver(isReadOnly ? SystemX509Store.OpenPrivate() : SystemX509Store.OpenPrivateEdit());

public class EmailNameResolver(CertificateStore certificateStore) : ICertificateResolver
{
    public CertificateStore Store = certificateStore;

    public X509Certificate2Collection GetCertificates(MailAddress address)
    {
        var listX509Certificate2sValid = new List<X509Certificate2>();
        var listX509Certificate2sInvalid = new List<X509Certificate2>();
        GetCertificates(address.Address, listX509Certificate2sValid, listX509Certificate2sInvalid);
        if (listX509Certificate2sValid.Count > 0 || listX509Certificate2sInvalid.Count > 0) return new X509Certificate2Collection(listX509Certificate2sValid.ToArray());

        return null;
    }

    public X509Certificate2Collection GetCertificatesForDomain(string domain)
    {
        var listX509Certificate2sValid = new List<X509Certificate2>();
        var listX509Certificate2sInvalid = new List<X509Certificate2>();
        GetCertificatesForDomain(domain, listX509Certificate2sValid, listX509Certificate2sInvalid);
        return new X509Certificate2Collection(listX509Certificate2sValid.ToArray());
    }

    public event Action<ICertificateResolver, Exception> Error;

    ~EmailNameResolver()
    {
        if (Store != null)
        {
            Store.Dispose();
            Store = null;
        }
    }

    public void GetCertificates(string addressOrDomain, List<X509Certificate2> listX509Certificate2sValid, List<X509Certificate2> listlistX509Certificate2sInvalid)
    {
        if (addressOrDomain.Contains("@"))
        {
            GetCertificatesForAddress(addressOrDomain, listX509Certificate2sValid, listlistX509Certificate2sInvalid);
            if (listX509Certificate2sValid.Count > 0)
                //If we found at least one address specific certificate, then we ignore domain level certificates.
                //This will be true even if we only find invalid certificates, because we assume the intent of the setup was
                //to have an address specific certificate and there is currently something wrong which the user might correct.
                //We do not want to temporarily send a domain level certificate if there is a minor issue that can be easily corrected
                //for any invalid address specific certificates found.
                return;
        }

        var domain = addressOrDomain;
        if (addressOrDomain.Contains("@"))
        {
            var mailAddress = new MailAddress(addressOrDomain);
            domain = mailAddress.Host;
        }

        GetCertificatesForDomain(domain, listX509Certificate2sValid, listlistX509Certificate2sInvalid);
    }

    public X509Certificate2Collection GetCertificatesForAddress(string emailAddress)
    {
        var listX509Certificate2sValid = new List<X509Certificate2>();
        var listX509Certificate2sInvalid = new List<X509Certificate2>();
        GetCertificatesForAddress(emailAddress, listX509Certificate2sValid, listX509Certificate2sInvalid);
        return new X509Certificate2Collection(listX509Certificate2sValid.ToArray());
    }

    public void GetCertificatesForAddress(string emailAddress, List<X509Certificate2> listX509Certificate2sValid, List<X509Certificate2> listX509Certificate2sInvalid)
    {
        var x509Certificate2Collection = Store.GetAllCertificates();
        for (var i = 0; i < x509Certificate2Collection.Count; i++)
        {
            var subjectName = GetCertSubjectName(x509Certificate2Collection[i]);
            if (subjectName != "" && subjectName.ToLower() != emailAddress.ToLower()) continue;

            if (GetCertRfc822Name(x509Certificate2Collection[i]).ToLower() != emailAddress.ToLower()) continue;

            if (IsCertValid(x509Certificate2Collection[i]))
            {
                listX509Certificate2sValid.Add(x509Certificate2Collection[i]);
                continue;
            }

            listX509Certificate2sInvalid.Add(x509Certificate2Collection[i]);
        }
    }

    public void GetCertificatesForDomain(string domain, List<X509Certificate2> listX509Certificate2sValid, List<X509Certificate2> listX509Certificate2sInvalid)
    {
        var x509Certificate2Collection = Store.GetAllCertificates();
        for (var i = 0; i < x509Certificate2Collection.Count; i++)
        {
            var subjectName = GetCertSubjectName(x509Certificate2Collection[i]);
            if (subjectName != "" && subjectName.ToLower() != domain.ToLower()) continue;

            if (GetCertDnsName(x509Certificate2Collection[i]).ToLower() != domain.ToLower()) continue;

            if (IsCertValid(x509Certificate2Collection[i]))
            {
                listX509Certificate2sValid.Add(x509Certificate2Collection[i]);
                continue;
            }

            listX509Certificate2sInvalid.Add(x509Certificate2Collection[i]);
        }
    }

    public static string GetCertSubjectName(X509Certificate2 x509Certificate2)
    {
        var listSubjectNames = x509Certificate2.SubjectName.Name.Split(',').ToList();
        for (var i = 0; i < listSubjectNames.Count; i++)
        {
            var typeAndName = listSubjectNames[i].Trim();
            if (typeAndName.ToUpper().StartsWith("E="))
            {
                var name = typeAndName.Substring(2);
                return name;
            }
        }

        return "";
    }

    public static string GetCertRfc822Name(X509Certificate2 x509Certificate2)
    {
        for (var i = 0; i < x509Certificate2.Extensions.Count; i++)
        {
            if (x509Certificate2.Extensions[i].Oid.FriendlyName.ToLower() != "subject alternative name") continue;

            var asnEncodedData = new AsnEncodedData(x509Certificate2.Extensions[i].Oid, x509Certificate2.Extensions[i].RawData);
            var san = asnEncodedData.Format(true);
            var listStringSanFields = san.Replace("\r", "").Split('\n').ToList();
            for (var j = 0; j < listStringSanFields.Count; j++)
            {
                var fieldName = "rfc822 name=";
                if (listStringSanFields[j].ToLower().StartsWith(fieldName)) return listStringSanFields[j].Substring(fieldName.Length);
            }
        }

        return "";
    }

    public static string GetCertDnsName(X509Certificate2 x509Certificate2)
    {
        for (var i = 0; i < x509Certificate2.Extensions.Count; i++)
        {
            if (x509Certificate2.Extensions[i].Oid.FriendlyName.ToLower() != "subject alternative name") continue;

            var asnEncodedData = new AsnEncodedData(x509Certificate2.Extensions[i].Oid, x509Certificate2.Extensions[i].RawData);
            var san = asnEncodedData.Format(true);
            var listStringSanFields = san.Replace("\r", "").Split('\n').ToList();
            for (var j = 0; j < listStringSanFields.Count; j++)
            {
                var fieldName = "dns name=";
                if (listStringSanFields[j].ToLower().StartsWith(fieldName)) return listStringSanFields[j].Substring(fieldName.Length);
            }
        }

        return "";
    }

    public static bool IsCertValid(X509Certificate2 x509Certificate2)
    {
        //This code mimics Health.Direct.Agent.TrustChainValidator.IsTrustedCertificate().
        var x509ChainBuilder = new X509Chain();
        x509ChainBuilder.ChainPolicy = new X509ChainPolicy();
        x509ChainBuilder.Build(x509Certificate2);
        for (var i = 0; i < x509ChainBuilder.ChainElements.Count; i++)
            if (x509ChainBuilder.ChainElements[i].ChainElementStatus.Any(s => (s.Status & TrustChainValidator.DefaultProblemFlags) != 0))
                return false;

        return true;
    }
}