using System.Collections.Generic;

namespace OpenDentBusiness.Email;

public class BasicEmailMessage
{
    public string BccAddress;
    public string BodyText;
    public string CcAddress;
    public string FromAddress;
    public bool IsHtml;
    public string Subject;
    public string ToAddress;
    public List<BasicEmailAttachment> ListAttachments;
    public List<string> ListHtmlImages;
    public string HtmlBody;
}