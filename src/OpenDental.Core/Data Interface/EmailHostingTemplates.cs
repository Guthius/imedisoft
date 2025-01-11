using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EmailHostingTemplates
{
    private const string MASS_EMAIL_LOG_DIR = "MassEmail";

    public static IAccountApi GetAccountApi(long clinicNum)
    {
        var guid = ClinicPrefs.GetPrefValue(PrefName.MassEmailGuid, clinicNum);
        var secret = ClinicPrefs.GetPrefValue(PrefName.MassEmailSecret, clinicNum);
        if (ODBuild.IsDebug()) return AccountApiMock.Get(clinicNum, guid, secret);
        var emailHostingEndpoint = PrefC.GetString(PrefName.EmailHostingEndpoint);
        return new AccountApi(guid, secret, emailHostingEndpoint);
    }

    public static void SyncWithHq()
    {
        var listHostingTemplatesAll = Refresh();
        var listClinicNums = new List<long> {0};
        listClinicNums.AddRange(Clinics.GetDeepCopy().Select(x => x.Id));
        foreach (var t in listClinicNums)
        {
            try
            {
                SyncWithHq(t, listHostingTemplatesAll.FindAll(x => x.ClinicNum == t));
            }
            catch (Exception ex)
            {
                Logger.WriteLine(Lans.g("MassEmail", "Unable to sync MassEmail templates for clinicNum:") + " " + t + ".\n" + MiscUtils.GetExceptionText(ex)
                    , ODFileUtils.CombinePaths(MASS_EMAIL_LOG_DIR, t.ToString()));
            }
        }
    }

    private static void SyncWithHq(long clinicNum, List<EmailHostingTemplate> listEmailHostingTemplatesDatabase)
    {
        if (!Clinics.IsSecureEmailEnabled(clinicNum) && !Clinics.IsMassEmailEnabled(clinicNum)) return;
        var api = GetAccountApi(clinicNum);

        #region Sync Email Signatures

        var plainTextSignature = GetSignature(clinicNum, false);
        var htmlSignature = GetSignature(clinicNum, true);
        var accountDescription = Clinics.GetAbbr(clinicNum);
        var updateSignatureRequest = new UpdateSignatureRequest();
        updateSignatureRequest.SignatureHtml = htmlSignature;
        updateSignatureRequest.SignaturePlainText = plainTextSignature;
        updateSignatureRequest.AccountDescription = accountDescription;
        var updateSignatureResponse = api.UpdateSignature(updateSignatureRequest);
        //Update the cache in case we just uploaded default/generic signatures.
        if (clinicNum == 0)
        {
            Prefs.UpdateString(PrefName.EmailHostingSignaturePlainText, plainTextSignature);
            Prefs.UpdateString(PrefName.EmailHostingSignatureHtml, htmlSignature);
        }
        else
        {
            ClinicPrefs.Upsert(PrefName.EmailHostingSignaturePlainText, clinicNum, plainTextSignature);
            ClinicPrefs.Upsert(PrefName.EmailHostingSignatureHtml, clinicNum, htmlSignature);
        }

        #endregion

        if (!Clinics.IsMassEmailEnabled(clinicNum)) return;
        //todo, check if credentials were available, otherwise skip.
        var logSubDir = ODFileUtils.CombinePaths("EmailHostingTemplates", clinicNum.ToString());

        #region Update Database Templates to match API (exists in database, not in API)

        listEmailHostingTemplatesDatabase.RemoveAll(x => x.ClinicNum != clinicNum);

        #region Remove Birthday templates that do not have an Appointment Reminder Rule

        var listApptReminderRulesBirthday = ApptReminderRules.GetForTypes(ApptReminderType.Birthday).Where(x => x.ClinicNum == clinicNum).ToList();
        var listEmailHostingTemplatesBirthday = listEmailHostingTemplatesDatabase.FindAll(x => x.TemplateType == PromotionType.Birthday);
        var listEmailHostingTemplateNumsNoRules = listEmailHostingTemplatesBirthday
            .FindAll(x => !listApptReminderRulesBirthday.Select(y => y.EmailHostingTemplateNum).Contains(x.EmailHostingTemplateNum))
            .Select(z => z.EmailHostingTemplateNum)
            .ToList();
        listEmailHostingTemplatesDatabase.RemoveAll(x => listEmailHostingTemplateNumsNoRules.Contains(x.EmailHostingTemplateNum)); //Remove orphaned templates
        for (var i = 0; i < listEmailHostingTemplateNumsNoRules.Count; i++) Delete(listEmailHostingTemplateNumsNoRules[i]);

        #endregion

        #endregion

        #region Get API templates

        var getAllTemplatesByAccountRequest = new GetAllTemplatesByAccountRequest();
        var getAllTemplatesByAccountResponse = api.GetAllTemplatesByAccount(getAllTemplatesByAccountRequest);
        var listEmailHostingTemplateNumsToRemove = new List<long>();
        listEmailHostingTemplateNumsToRemove.AddRange(listEmailHostingTemplateNumsNoRules); //if any templates deleted above we'll also want to remove their API info
        var listDictKeys = getAllTemplatesByAccountResponse.DictionaryTemplates.Keys.ToList();
        for (var i = 0; i < listDictKeys.Count; i++)
            if (!listEmailHostingTemplatesDatabase.Any(x => x.TemplateId == listDictKeys[i]))
                //no database template exists for this api template. Remove it. 
                listEmailHostingTemplateNumsToRemove.Add(listDictKeys[i]);

        #endregion

        #region Re-create templates that have previously existed in the API but are now missing for some reason

        for (var i = 0; i < listEmailHostingTemplatesDatabase.Count; i++)
        {
            var existsApi = getAllTemplatesByAccountResponse.DictionaryTemplates.TryGetValue(listEmailHostingTemplatesDatabase[i].TemplateId, out var templateApi);
            var htmlBody = GetHtmlBody(listEmailHostingTemplatesDatabase[i]);
            Template template = null;
            if (existsApi)
            {
                if (templateApi.TemplateSubject != listEmailHostingTemplatesDatabase[i].Subject
                    || templateApi.TemplateBodyPlainText != listEmailHostingTemplatesDatabase[i].BodyPlainText
                    || templateApi.TemplateBodyHtml != htmlBody)
                {
                    //The template exists at the api but is different then the database. Always assume the database is correct.
                    //This may happen because we had a bug where templates were syncing to the EmailHosting API as wiki html.
                    template = new Template();
                    template.TemplateName = listEmailHostingTemplatesDatabase[i].TemplateName;
                    template.TemplateSubject = listEmailHostingTemplatesDatabase[i].Subject;
                    template.TemplateBodyPlainText = listEmailHostingTemplatesDatabase[i].BodyPlainText;
                    template.TemplateBodyHtml = htmlBody;
                    var updateTemplateRequest = new UpdateTemplateRequest();
                    updateTemplateRequest.TemplateNum = listEmailHostingTemplatesDatabase[i].TemplateId;
                    updateTemplateRequest.Template = template;
                    var response = api.UpdateTemplate(updateTemplateRequest);
                }

                continue;
            }

            //template exists in database, but no api template exists. It must have been deleted from the api somehow on accident, or never made it in there.
            template = new Template();
            template.TemplateName = listEmailHostingTemplatesDatabase[i].TemplateName;
            template.TemplateBodyHtml = htmlBody;
            template.TemplateBodyPlainText = listEmailHostingTemplatesDatabase[i].BodyPlainText;
            template.TemplateSubject = listEmailHostingTemplatesDatabase[i].Subject;
            var createTemplateRequest = new CreateTemplateRequest();
            createTemplateRequest.Template = template;
            var createTemplateResponse = api.CreateTemplate(createTemplateRequest);
            if (createTemplateResponse.TemplateNum == 0)
            {
                Logger.WriteError(Lans.g("EmailHostingTemplates", "Upload failed for EmailHostingTemplateNum:") + " " + listEmailHostingTemplatesDatabase[i].TemplateName, logSubDir);
                continue;
            }

            listEmailHostingTemplatesDatabase[i].TemplateId = createTemplateResponse.TemplateNum;
            Update(listEmailHostingTemplatesDatabase[i]);
        }

        #endregion

        #region Remove templates that exist in API but not in the Database

        for (var i = 0; i < listEmailHostingTemplateNumsToRemove.Count; i++)
        {
            var deleteTemplateRequest = new DeleteTemplateRequest();
            deleteTemplateRequest.TemplateNum = listEmailHostingTemplateNumsToRemove[i];
            try
            {
                api.DeleteTemplate(deleteTemplateRequest);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }

    private static string GetHtmlBody(EmailHostingTemplate emailHostingTemplate)
    {
        if (emailHostingTemplate.EmailTemplateType == EmailType.Html) return MarkupEdit.TranslateToXhtml(emailHostingTemplate.BodyHTML, true, false, true);
        return emailHostingTemplate.BodyHTML;
    }

    public static string GetSignature(long clinicNum, bool isHtml)
    {
        string signature = null;
        //Gets the practice preference if not found for this clinic.
        if (isHtml)
            signature = ClinicPrefs.GetPrefValue(PrefName.EmailHostingSignatureHtml, clinicNum);
        else
            signature = ClinicPrefs.GetPrefValue(PrefName.EmailHostingSignaturePlainText, clinicNum);
        if (signature.IsNullOrEmpty()) signature = GetGenericSignature(clinicNum, isHtml);
        return signature;
    }

    private static string GetGenericSignature(long clinicNum, bool isHtml)
    {
        var clinic = Clinics.GetClinic(clinicNum);
        if (clinic is null) clinic = Clinics.GetPracticeAsClinicZero(); //Get the default/HQ clinic
        var description = GetSignatureField(clinic.Description);
        var city = GetSignatureField(clinic.City, ",");
        var state = GetSignatureField(clinic.State, "");
        var zip = GetSignatureField(clinic.Zip, "");
        var cityStateZip = GetSignatureField(city + " " + state + " " + zip);
        var phoneWithFormat = TelephoneNumbers.ReFormat(clinic.PhoneNumber);
        var phone = "";
        if (!string.IsNullOrWhiteSpace(phoneWithFormat)) phone = "PH: " + GetSignatureField(phoneWithFormat, "");
        if (isHtml)
        {
            if (!string.IsNullOrWhiteSpace(description)) description = "<b>" + description + "</b>";
            if (!string.IsNullOrWhiteSpace(phoneWithFormat)) phone = "PH: <i>" + GetSignatureField(phoneWithFormat, "") + "</i>";
        }

        var signature = description + GetSignatureField(clinic.AddressLine1) + GetSignatureField(clinic.AddressLine2) + cityStateZip + phone;
        if (isHtml) signature = signature.Replace("\r\n", "<br/>");
        return signature;
    }

    private static string GetSignatureField(string field, string separator = "\r\n")
    {
        if (string.IsNullOrWhiteSpace(field)) return "";
        return field + separator;
    }

    public static List<EmailHostingTemplate> Refresh()
    {
        var command = "SELECT * FROM emailhostingtemplate";
        return EmailHostingTemplateCrud.SelectMany(command);
    }

    public static List<EmailHostingTemplate> GetMany(List<long> listEmailHostingTemplateNums)
    {
        if (listEmailHostingTemplateNums.IsNullOrEmpty()) return new List<EmailHostingTemplate>();

        var command = "SELECT * FROM emailhostingtemplate WHERE EmailHostingTemplateNum IN (" +
                      string.Join(",", listEmailHostingTemplateNums.Select(x => SOut.Long(x)).Distinct()) + ")";
        return EmailHostingTemplateCrud.SelectMany(command);
    }

    public static long Insert(EmailHostingTemplate emailHostingTemplate)
    {
        return EmailHostingTemplateCrud.Insert(emailHostingTemplate);
    }

    public static EmailHostingTemplate CreateDefaultTemplate(long clinicNum, PromotionType promotionType)
    {
        var emailHostingTemplate = new EmailHostingTemplate();
        emailHostingTemplate.ClinicNum = clinicNum;
        emailHostingTemplate.Subject = "Happy Birthday";
        emailHostingTemplate.BodyPlainText = "Wishing you a happy and healthy Birthday! Hope your day is full of smiles and memorable moments. " +
                                             "From your friends at [{[{ OfficeName }]}]";
        emailHostingTemplate.BodyHTML = "";
        emailHostingTemplate.EmailTemplateType = EmailType.Regular;
        emailHostingTemplate.TemplateName = "Automated Birthday Message";
        emailHostingTemplate.TemplateType = promotionType;
        return emailHostingTemplate;
    }

    public static void Update(EmailHostingTemplate emailHostingTemplate)
    {
        EmailHostingTemplateCrud.Update(emailHostingTemplate);
    }

    public static void Delete(long emailHostingTemplateNum)
    {
        EmailHostingTemplateCrud.Delete(emailHostingTemplateNum);
    }

    public static List<string> GetListReplacements(string subjectOrBody)
    {
        if (string.IsNullOrWhiteSpace(subjectOrBody)) return new List<string>();
        var retVal = new List<string>();
        var matchCollection = Regex.Matches(subjectOrBody, @"\[{\[{\s?([A-Za-z0-9]*)\s?}\]}\]");
        for (var i = 0; i < matchCollection.Count; i++) retVal.Add(matchCollection[i].Groups[1].Value.Trim());
        return retVal;
    }
}