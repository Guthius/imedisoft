using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using CodeBase;
using Google;
using Google.Apis.Gmail.v1.Data;
using MimeKit;
using ContentType = System.Net.Mime.ContentType;

namespace OpenDentBusiness.Email;

public static class SendEmail
{
    public const int EmailSendTimeoutMillis = 180000;

    public static void WireEmailUnsecure(BasicEmailAddress address, BasicEmailMessage emailMessage, NameValueCollection nameValueCollectionHeaders, params AlternateView[] arrayAlternateViews)
    {
        try
        {
            WireEmailUnsecure(address, emailMessage, nameValueCollectionHeaders, EmailSendTimeoutMillis, arrayAlternateViews);
        }
        catch (Exception ex)
        {
            if (ex.Message.Split(' ').Contains("451"))
            {
                throw new ODException("Email provider is having temporary send issues. This could be due to high email demand. Please try again later.");
            }

            throw new ODException("Error sending email", ex);
        }
    }

    public static void WireEmailUnsecure(BasicEmailAddress address, BasicEmailMessage emailMessage, NameValueCollection nameValueCollectionHeaders, int emailSendTimeoutMs = EmailSendTimeoutMillis, params AlternateView[] arrayAlternateViews)
    {
        if (address.AuthenticationType.In(BasicOAuthType.Google))
        {
            SendEmailOAuth(address, emailMessage);
        }
        else
        {
            //No SSL or explicit SSL on port 587  
            SmtpClient client = null;
            MailMessage message = null;
            try
            {
                client = new SmtpClient(address.SMTPserver, address.ServerPort);
                //The default credentials are not used by default, according to: 
                //http://msdn2.microsoft.com/en-us/library/system.net.mail.smtpclient.usedefaultcredentials.aspx
                client.Credentials = new NetworkCredential(address.EmailUsername.Trim(), address.EmailPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = address.UseSSL;
                client.Timeout = emailSendTimeoutMs;
                message = BasicEmailMessageToMailMessage(emailMessage, nameValueCollectionHeaders, arrayAlternateViews);
                client.Send(message);
            }
            finally
            {
                //Dispose of the client and messages here. For large customers, sending thousands of emails will start to fail until they restart the
                //app. Freeing memory here can prevent OutOfMemoryExceptions.
                client?.Dispose();
                if (message != null)
                {
                    if (message.Attachments != null)
                    {
                        message.Attachments.ForEach(x => x.Dispose());
                    }

                    message.Dispose();
                }
            }
        }
    }

    public static MailMessage BasicEmailMessageToMailMessage(BasicEmailMessage basicMessage, NameValueCollection nameValueCollectionHeaders, params AlternateView[] arrayAlternateViews)
    {
        var message = new MailMessage();
        message.From = new MailAddress(basicMessage.FromAddress.Trim());
        if (!string.IsNullOrWhiteSpace(basicMessage.ToAddress))
        {
            message.To.Add(basicMessage.ToAddress.Trim());
        }

        if (!string.IsNullOrWhiteSpace(basicMessage.CcAddress))
        {
            message.CC.Add(basicMessage.CcAddress.Trim());
        }

        if (!string.IsNullOrWhiteSpace(basicMessage.BccAddress))
        {
            message.Bcc.Add(basicMessage.BccAddress.Trim());
        }

        message.Subject = basicMessage.Subject;
        if (basicMessage.IsHtml)
        {
            //create alternate view in case browser cannot render html
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(basicMessage.HtmlBody
                , new ContentType("text/html")));
            message.IsBodyHtml = true;
            message.Body = basicMessage.HtmlBody;
            if (!basicMessage.ListHtmlImages.IsNullOrEmpty())
            {
                foreach (var imagePath in basicMessage.ListHtmlImages)
                {
                    var imgAttach = new Attachment(imagePath);
                    imgAttach.ContentId = HttpUtility.UrlEncode(Path.GetFileName(imagePath));
                    imgAttach.ContentDisposition.Inline = true;
                    message.Attachments.Add(imgAttach);
                }
            }
        }
        else
        {
            message.IsBodyHtml = false;
            message.Body = basicMessage.BodyText;
        }

        if (nameValueCollectionHeaders != null)
        {
            message.Headers.Add(nameValueCollectionHeaders); //Needed for Direct Acks to work.
        }

        for (var i = 0; i < arrayAlternateViews.Length; i++)
        {
            //Needed for Direct messages to be interpreted encrypted on the receiver's end.
            message.AlternateViews.Add(arrayAlternateViews[i]);
        }

        if (!basicMessage.ListAttachments.IsNullOrEmpty())
        {
            foreach (var attachment in basicMessage.ListAttachments)
            {
                //@"C:\OpenDentalData\EmailAttachments\1");
                var attach = new Attachment(attachment.FullPath);
                //"canadian.gif";
                attach.Name = attachment.DisplayedFilename;
                message.Attachments.Add(attach);
            }
        }

        return message;
    }

    private static void SendEmailOAuth(BasicEmailAddress address, BasicEmailMessage message)
    {
        if (address.AuthenticationType != BasicOAuthType.Google)
        {
            return;
        }

        using var gService = GoogleApiConnector.CreateGmailService(address);
        try
        {
            var gmailMessage = CreateGmailMsg(message);

            gService.Users.Messages.Send(gmailMessage, address.EmailUsername).Execute();
        }
        catch (GoogleApiException gae)
        {
            throw new GoogleApiException(gae.ServiceName, "Unable to authenticate with Google: " + gae.Message, gae);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error sending email with OAuth authorization: {ex.Message}");
        }
    }

    private static MimeMessage CreateMimeMessage(BasicEmailMessage emailMessage)
    {
        var mimeMessage = new MimeMessage();
        if (!emailMessage.Subject.IsNullOrEmpty())
        {
            mimeMessage.Subject = emailMessage.Subject;
        }

        var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(emailMessage.FromAddress);
        mimeMessage.From.Add(new MailboxAddress(mailMessage.From.DisplayName, mailMessage.From.Address));
        mailMessage.To.Add(emailMessage.ToAddress.Trim());
        mimeMessage.To.AddRange(mailMessage.To.Select(x => new MailboxAddress(x.DisplayName, x.Address)));
        if (!emailMessage.CcAddress.IsNullOrEmpty())
        {
            mailMessage.CC.Add(emailMessage.CcAddress.Trim());
            mimeMessage.Cc.AddRange(mailMessage.CC.Select(x => new MailboxAddress(x.DisplayName, x.Address)));
        }

        if (!emailMessage.BccAddress.IsNullOrEmpty())
        {
            mailMessage.Bcc.Add(emailMessage.BccAddress.Trim());
            mimeMessage.Bcc.AddRange(mailMessage.Bcc.Select(x => new MailboxAddress(x.DisplayName, x.Address)));
        }

        var body = new BodyBuilder();
        if (!emailMessage.ListHtmlImages.IsNullOrEmpty())
        {
            foreach (var imagePath in emailMessage.ListHtmlImages)
            {
                var imgAttach = body.LinkedResources.Add(imagePath);

                imgAttach.ContentId = HttpUtility.UrlEncode(Path.GetFileName(imagePath));
                imgAttach.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            }
        }

        if (emailMessage.ListAttachments != null)
        {
            foreach (var attachmentPath in emailMessage.ListAttachments)
            {
                body.Attachments.Add(attachmentPath.FullPath);
            }
        }

        if (emailMessage.IsHtml)
        {
            body.HtmlBody = emailMessage.HtmlBody;
        }
        else
        {
            body.TextBody = emailMessage.BodyText;
        }

        for (var i = 0; i < body.Attachments.Count; i++)
        {
            var attachment = (MimePart) body.Attachments[i];

            if (attachment.ContentTransferEncoding == ContentEncoding.QuotedPrintable)
            {
                attachment.ContentTransferEncoding = ContentEncoding.Base64;
            }

            attachment.FileName = emailMessage.ListAttachments[i].DisplayedFilename;
        }

        mimeMessage.Body = body.ToMessageBody();
        return mimeMessage;
    }

    private static Message CreateGmailMsg(BasicEmailMessage emailMessage)
    {
        var mimeMessage = CreateMimeMessage(emailMessage);

        using var stream = new MemoryStream();

        mimeMessage.WriteTo(stream);

        stream.Position = 0;

        using var streamReader = new StreamReader(stream);

        var message = new Message();

        var rawString = streamReader.ReadToEnd();
        var raw = Encoding.UTF8.GetBytes(rawString);

        message.Raw = Convert.ToBase64String(raw);
        message.Raw = message.Raw.Replace("+", "-");
        message.Raw = message.Raw.Replace("/", "_");

        return message;
    }
}