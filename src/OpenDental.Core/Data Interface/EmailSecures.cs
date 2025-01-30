using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class EmailSecures
{
    private static readonly char[] _arrEmailAddressDelimiters = [';', ','];

    public static bool IsSecureEmailReleased()
    {
        var listClinicNums = Clinics.GetDeepCopy().Select(x => x.Id).ToList();
        listClinicNums.Add(0);
        return listClinicNums.Any(x => LimitedBetaFeatures.IsAllowed(EServiceFeatureInfoEnum.SecureEmail, x));
    }

    public static EmailSecure GetByEmailMessageNum(long emailMessageNum)
    {
        if (emailMessageNum <= 0) return null;

        var command = "SELECT * FROM emailsecure WHERE emailsecure.EmailMessageNum = " + SOut.Long(emailMessageNum) + " ";
        return EmailSecureCrud.SelectOne(command);
    }
    
    public static void Insert(EmailSecure emailSecure)
    {
        EmailSecureCrud.Insert(emailSecure);
    }

    public static void InsertMessageThenSend(EmailMessage emailMessage, EmailAddress EmailAddressSender, string toAddress, long clinicNum, EmailMessage emailMessageReplyingTo = null, Patient patient = null)
    {
        //SendSecureEmail() operates off the assumption that the EmailMessage is already in the database. If we have no PriKey yet then insert this email.
        if (emailMessage.EmailMessageNum == 0) EmailMessages.Insert(emailMessage);
        SendSecureEmail(emailMessage, EmailAddressSender, toAddress, clinicNum, emailMessageReplyingTo, patient);
    }
    
    public static void SendSecureEmail(EmailMessage emailMessageDb, EmailAddress emailAddressSender, string stringToAddresses, long clinicNum, EmailMessage emailMessageReplyingTo = null, Patient patient = null)
    {
        //Work with a copy of messageDb.
        //Otherwise, changes would persist in calling method after an exception is thrown, and user may change sending method.
        var emailMessageDbCopy = emailMessageDb.Copy();
        var iAccountApi = EmailHostingTemplates.GetAccountApi(clinicNum);
        var listEmailAddressResourcesTo = ToEmailAddressResources(stringToAddresses);
        emailMessageDbCopy.MsgType = EmailMessageSource.Hosting;
        var emailResource = ToEmailResource(emailMessageDbCopy, emailAddressSender);
        emailResource.ListAttachments = UploadSecureAttachments(iAccountApi, emailMessageDbCopy.Attachments);
        var emailChainFk = GetEmailChainFkFromEmailMessage(emailMessageReplyingTo);
        if (emailMessageReplyingTo != null && EmailMessages.IsSecureEmail(emailMessageReplyingTo.SentOrReceived) && emailChainFk == 0) throw new ODException("The Secure Email you are replying to could not be found.");
        //If emailChainFk is not zero, we are replying on an existing secure email chain.
        EmailSecure emailSecure;
        if (emailChainFk != 0)
        {
            emailSecure = SendReplySecureEmail(iAccountApi, emailResource, emailChainFk, emailMessageDbCopy, clinicNum);
        }
        else
        {
            //Otherwise, we send a message on a new secure email chain. This happens if not replying or replying to a regular email.
            var notificationSummary = GetNotificationSummary(patient, emailMessageDbCopy);
            emailSecure = SendNewSecureEmail(iAccountApi, emailResource, listEmailAddressResourcesTo, emailMessageDbCopy, clinicNum, notificationSummary);
        }

        Insert(emailSecure);
    }

    public static long GetEmailChainFkFromEmailMessage(EmailMessage emailMessage)
    {
        if (emailMessage == null) return 0;
        var emailSecure = GetByEmailMessageNum(emailMessage.EmailMessageNum); //Returns null if not found.
        if (emailSecure == null) return 0;
        return emailSecure.EmailChainFK;
    }

    private static string GetNotificationSummary(Patient patient, EmailMessage emailMessageDb)
    {
        var notificationSummary = "";
        if (patient == null) patient = EmailMessages.GetPatient(emailMessageDb);
        if (patient != null) notificationSummary = patient.GetNameFirstOrPreferred();
        return notificationSummary;
    }

    private static List<EmailAddressResource> ToEmailAddressResources(string strEmailAddress)
    {
        if (strEmailAddress == null) strEmailAddress = "";
        var listStrEmailAddresses = strEmailAddress.Split(_arrEmailAddressDelimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
        var listEmailAddressResourcesRet = new List<EmailAddressResource>();
        for (var i = 0; i < listStrEmailAddresses.Count; i++)
        {
            var emailAddressDecoded = EmailMessages.ProcessInlineEncodedText(listStrEmailAddresses[i].Trim().ToLower());
            var mailAddress = EmailAddresses.GetValidMailAddress(emailAddressDecoded);
            if (mailAddress == null) throw new ArgumentException(Lans.g("EmailSecure", "Invalid email address: ") + listStrEmailAddresses[i]);
            var emailAddressResource = new EmailAddressResource();
            emailAddressResource.Address = mailAddress.Address;
            emailAddressResource.Alias = mailAddress.DisplayName;
            listEmailAddressResourcesRet.Add(emailAddressResource);
        }

        return listEmailAddressResourcesRet;
    }

    private static EmailResource ToEmailResource(EmailMessage emailMessage, EmailAddress emailAddressSender)
    {
        var emailAddressResource = new EmailAddressResource();
        emailAddressResource.Address = emailAddressSender.GetFrom();
        var emailResource = new EmailResource();
        emailResource.FromAddress = emailAddressResource;
        emailResource.Subject = emailMessage.Subject;
        emailResource.DateTimeEmail = DateTime.Now;
        emailResource.ExternalTag = new ExternalTag();
        emailResource.ListAttachments = new List<AttachmentResource>();
        if (emailMessage.HtmlType.In(EmailType.Html, EmailType.RawHtml))
        {
            emailResource.BodyHtml = EmailMessages.EmbedImages(emailMessage.HtmlText, emailMessage.AreImagesDownloaded);
            return emailResource;
        }

        emailResource.BodyHtml = emailMessage.BodyText;
        return emailResource;
    }

    private static EmailSecure SendNewSecureEmail(IAccountApi iAccountApi, EmailResource emailResource, List<EmailAddressResource> listEmailAddressResources, EmailMessage emailMessageDb, long clinicNum, string notificationSummary)
    {
        var funcSend = () =>
        {
            var sendNewEmailRequest = new SendNewEmailRequest();
            sendNewEmailRequest.EmailToSend = emailResource;
            sendNewEmailRequest.ListEmailAddresses = listEmailAddressResources;
            sendNewEmailRequest.DoSendNotificationsAsOwner = !ClinicPrefs.GetBool(PrefName.EmailHostingUseNoReply, clinicNum);
            sendNewEmailRequest.NotificationSummary = notificationSummary;
            var sendNewEmailResponse = iAccountApi.SendNewEmail(sendNewEmailRequest);
            return ToEmailSecureSent(emailMessageDb, clinicNum, sendNewEmailResponse.EmailChainNum, sendNewEmailResponse.EmailNum);
        };
        return SendViaApi(emailMessageDb, funcSend);
    }

    private static EmailSecure SendReplySecureEmail(IAccountApi iAccountApi, EmailResource emailResource, long emailChainFk, EmailMessage emailMessageDb, long clinicNum)
    {
        var funcSend = () =>
        {
            var sendReplyRequest = new SendReplyRequest();
            sendReplyRequest.EmailToSend = emailResource;
            sendReplyRequest.EmailChainNum = emailChainFk;
            sendReplyRequest.DoSendNotificationsAsOwner = !ClinicPrefs.GetBool(PrefName.EmailHostingUseNoReply, clinicNum);
            //Replying to an existing EmailChain
            var sendReplyResponse = iAccountApi.SendReply(sendReplyRequest);
            return ToEmailSecureSent(emailMessageDb, clinicNum, emailChainFk, sendReplyResponse.EmailNum);
        };
        return SendViaApi(emailMessageDb, funcSend);
    }

    private static EmailSecure SendViaApi(EmailMessage emailMessage, Func<EmailSecure> send)
    {
        try
        {
            emailMessage.SentOrReceived = EmailSentOrReceived.SecureEmailSent; //Set before send, so EmailMessage.FailReason is more informative on failure.
            var emailSecure = send();
            return emailSecure;
        }
        catch (Exception ex)
        {
            EmailMessages.SetFailed(emailMessage, ex);
            throw;
        }
        finally
        {
            EmailMessages.Update(emailMessage);
        }
    }

    private static EmailSecure ToEmailSecureSent(EmailMessage emailMessageDb, long clinicNum, long emailChainNum, long emailNum)
    {
        var emailSecure = new EmailSecure();
        emailSecure.PatNum = emailMessageDb.PatNum;
        emailSecure.ClinicNum = clinicNum;
        emailSecure.EmailChainFK = emailChainNum;
        emailSecure.EmailFK = emailNum;
        emailSecure.EmailMessageNum = emailMessageDb.EmailMessageNum;
        return emailSecure;
    }

    private static List<AttachmentResource> UploadSecureAttachments(IAccountApi iAccountApi, List<EmailAttach> listEmailAttaches)
    {
        var listAttachmentResources = new List<AttachmentResource>();
        var listAttachmentFiles = new List<AttachmentFile>();

        #region Load data into memory

        //Load all attachment data into memory in parallel.
        var listActionsReadFile = listEmailAttaches.Select(x => new Action(() =>
        {
            //Get file info for the attachment.  May download from Cloud (i.e. Dropbox, etc).
            var basicEmailAttachment = EmailMessages.GetListAttachmentsAndDownload(x).First();
            var displayFileName = Path.GetFileNameWithoutExtension(basicEmailAttachment.DisplayedFilename);
            var extension = Path.GetExtension(basicEmailAttachment.DisplayedFilename);
            var byteArray = File.ReadAllBytes(basicEmailAttachment.FullPath);
            var bytesBase64 = Convert.ToBase64String(byteArray);
            var attachmentFile = new AttachmentFile();
            attachmentFile.DisplayFileName = displayFileName;
            attachmentFile.Extension = extension;
            attachmentFile.BytesBase64 = bytesBase64;
            listAttachmentFiles.Add(attachmentFile);
        })).ToList();
        //Read all files
        RunWebCalls(listActionsReadFile);

        #endregion

        #region Upload data to EmailHosting

        //Upload attachment data in parallel.
        var listActionsUpload = listAttachmentFiles.Select(x => new Action(() =>
        {
            var uploadS3ObjectRequest = new UploadS3ObjectRequest();
            uploadS3ObjectRequest.FileName = x.DisplayFileName;
            uploadS3ObjectRequest.Extension = x.Extension;
            uploadS3ObjectRequest.ObjectPurpose = S3ObjectPurpose.SecureEmailAttachment;
            uploadS3ObjectRequest.ObjectType = S3ObjectType.File;
            uploadS3ObjectRequest.ObjectBytesBase64 = x.BytesBase64;
            var uploadS3ObjectResponse = iAccountApi.UploadS3Object(uploadS3ObjectRequest);
            var attachmentResource = new AttachmentResource();
            attachmentResource.DisplayedFileName = x.DisplayFileName;
            attachmentResource.Extension = x.Extension;
            attachmentResource.GUID = uploadS3ObjectResponse.S3ObjectGuid;
            listAttachmentResources.Add(attachmentResource);
        })).ToList();
        RunWebCalls(listActionsUpload);

        #endregion

        return listAttachmentResources;
    }

    private static void RunWebCalls(List<Action> listActions)
    {
        //Run in parallel, because we are making web calls, and there is no reason to not do this in parallel
        //ODThread.RunParallel(listActions);
        //This is tremendously inefficient to run these web calls in serial, resulting in a degraded user experience.
        for (var i = 0; i < listActions.Count; i++) listActions[i].Invoke();
    }

    private class AttachmentFile
    {
        public string BytesBase64;
        public string DisplayFileName;
        public string Extension;
    }
}