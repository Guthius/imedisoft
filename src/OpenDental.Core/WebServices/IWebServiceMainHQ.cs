using System.Collections.Generic;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness
{
    public interface IWebServiceMainHQ
    {
        string Url { get; set; }

        List<long> GetEServiceClinicsAllowed(List<long> listClinicNums, eServiceCode eService);
        string SetEConnectorType(string officeData, bool isListening);
        string EServiceSetup(string officeData);
        string EnableAdditionalFeatures(string officeData);
        string SmsSend(string officeData);
        string BuildOAuthUrl(string registrationKey, string appName);
        string GetDropboxAccessToken(string accessCode);
        string GetFHIRAPIKeysForOffice(string officeData);
        string AssignFHIRAPIKey(string officeData);
        string UpdateFHIRKeyStatus(string officeData);
        string SubmitUnhandledException(string officeData);
        string GenerateShortGUIDs(string officeData);
        string CreateNewHelpKey(string officeData);
        string CanadaCarrierUpdate(string officeData);
        string InsertPaySimpleACHId(string officeData);
        string GetPaySimpleWebHookUrl();
        string CustomerUpdateCommitted(string officeData);
        string SetSupplementalBackupStatus(string officeData);
        string SetSmsPatientPhoneOptIn(string officeData);
        string GetGoogleAccessToken(string officeData);
        string EmailHostingSignup(string officeData);
        string GetMobileSettings(string officeData);
        string GetMobileSettings2FA(string officeData);
        string UpsertMobileSettings(string officeData);
        string GetAdvertisingPostcardsAccounts(string officeData);
        string GetPostcardManiaSSO(string officeData);
        string ManageAdvertisingPostcardsAccount(string officeData);
        string UploadPostcardManiaPatientList(string officeData);
        string GetMsgToPayShortUrl(string officeData);
    }
}