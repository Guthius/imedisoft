namespace OpenDentBusiness;

public interface IWebServiceMainHQ
{
    string SmsSend(string officeData);
    string BuildOAuthUrl(string registrationKey, string appName);
    string CreateNewHelpKey(string officeData);
    string CanadaCarrierUpdate(string officeData);
    string InsertPaySimpleACHId(string officeData);
    string GetPaySimpleWebHookUrl();
    string SetSmsPatientPhoneOptIn(string officeData);
    string GetGoogleAccessToken(string officeData);
}