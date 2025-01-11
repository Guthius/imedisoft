using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace CodeBase {
	public class EmailUtils {

		public static void SendEmail(List<string> toAddresses,string subject,string emailBody, string fromAddress,string fromPassword,string fromDisplayName,
			string fromSMTPserver,int fromServerPort,bool enableSsl=false,bool isBodyHtml=true,List<string> ccAddresses=null,List<string> bccAddresses=null,List<AlternateView> alternateViews=null)
		{
			//We don't want these to be null but we can't default to new lists.
			ccAddresses=ccAddresses??new List<string>();
			bccAddresses=bccAddresses??new List<string>();
			alternateViews=alternateViews??new List<AlternateView>();
			SmtpClient client=new SmtpClient(fromSMTPserver,fromServerPort);
			client.Credentials=new NetworkCredential(fromAddress,fromPassword);
			client.DeliveryMethod=SmtpDeliveryMethod.Network;
			client.EnableSsl=enableSsl;
			client.Timeout=180000;//3 minutes
			MailMessage message=new MailMessage();
			foreach(string cc in ccAddresses) {
				message.CC.Add(cc.Trim());
			}
			foreach(string bcc in bccAddresses) {
				message.Bcc.Add(bcc.Trim());
			}
			foreach(string toAddress in toAddresses) {
				message.To.Add(toAddress.Trim());
			}
			message.From=new MailAddress(fromAddress.Trim(),fromDisplayName);
			message.Subject=subject;
			message.Body=emailBody;
			foreach(AlternateView alternate in alternateViews) {
				message.AlternateViews.Add(alternate);
			}
			message.IsBodyHtml=isBodyHtml;
			client.Send(message);
		}
	}
}
