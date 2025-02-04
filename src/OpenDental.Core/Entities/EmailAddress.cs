using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EmailAddress : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EmailAddressNum;

    public string SMTPserver;
    public string EmailUsername;
    public string EmailPassword;
    public int ServerPort;
    public bool UseSSL;

    ///<summary>The email address of the sender as it should appear to the recipient.</summary>
    public string SenderAddress;

    ///<summary>For example pop.gmail.com</summary>
    public string Pop3ServerIncoming;

    ///<summary>Usually 110, sometimes 995.</summary>
    public int ServerPortIncoming;

    ///<summary>FK to userod.UserNum.  Associates a user with this email address.  A user may only have one email address associated with them.
    ///Can be 0 if no user is associated with this email address.</summary>
    public long UserNum;

    ///<summary>Webmail ProvNum.  Just makes it easier to know what email address the user picked in the inbox. Not a DB column.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public long WebmailProvNum;

    ///<summary>Needed for OAuth.</summary>
    public string AccessToken;

    ///<summary>Needed for OAuth.</summary>
    public string RefreshToken;

    ///<summary>When true, this will allow the user to download emails to their inbox.</summary>
    public bool DownloadInbox;

    ///<summary>Allows gmail users to specify search parameters</summary>
    public string QueryString;

    ///<summary>Enum:OAuthType None=0,Google=1,Microsoft=2.  Indicates which OAuth type to use for the email address.</summary>
    public OAuthType AuthenticationType;

    public bool IsImplicitSsl => ServerPort == 465;
    
    public EmailAddress Clone()
    {
        return (EmailAddress) MemberwiseClone();
    }

    public string GetFrom()
    {
        return string.IsNullOrEmpty(SenderAddress) ? EmailUsername : SenderAddress;
    }
}

public enum OAuthType
{
    None,
    Google
}