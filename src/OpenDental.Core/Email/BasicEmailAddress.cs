namespace OpenDentBusiness.Email;

public class BasicEmailAddress
{
    public string SMTPserver;
    public string EmailUsername;
    public string EmailPassword;
    public int ServerPort;
    public bool UseSSL;
    public string AccessToken;
    public BasicOAuthType AuthenticationType;
}

public enum BasicOAuthType
{
    None,
    Google
}