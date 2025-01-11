using System.Collections.Generic;
using System.Text;
using CodeBase;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness.AutoComm;

public class MsgToPayTagReplacer : TagReplacer
{
    public const string MONTHLY_CARD_TAG = "[monthlyCardsOnFile]";
    public const string NAME_PREF_TAG = "[namePref]";
    public const string PATNUM_TAG = "[PatNum]";
    public const string CURMONTH_TAG = "[currentMonth]";
    public const string STATEMENT_URL_TAG = "[StatementURL]";
    public const string STATEMENT_SHORT_TAG = "[StatementShortURL]";
    public const string MSG_TO_PAY_TAG = "[MsgToPayURL]";
    public const string STATEMENT_BALANCE_TAG = "[StatementBalance]";
    public const string STATEMENT_INS_EST_TAG = "[StatementInsuranceEst]";
    public const string NAMEFL_NOPREF_TAG = "[nameFLnoPref]";
    public const string NAMEF_NOPREF_TAG = "[nameFnoPref]";

    protected override void ReplaceTagsChild(StringBuilder sbTemplate, AutoCommObj autoCommObj, bool isEmail)
    {
        base.ReplaceTagsChild(sbTemplate, autoCommObj, isEmail);
            
        if (sbTemplate.ToString().Contains(MONTHLY_CARD_TAG))
        {
            ReplaceOneTag(sbTemplate, MONTHLY_CARD_TAG, CreditCards.GetMonthlyCardsOnFile(autoCommObj.PatNum), isEmail);
        }

        ReplaceOneTag(sbTemplate, NAME_PREF_TAG, autoCommObj.NamePreferred, isEmail);
        ReplaceOneTag(sbTemplate, PATNUM_TAG, autoCommObj.PatNum.ToString(), isEmail);
        ReplaceOneTag(sbTemplate, CURMONTH_TAG, DateTime_.Now.ToString("MMMM"), isEmail);
            
        var patient = Patients.GetPat(autoCommObj.PatNum);
        var statement = Statements.GetStatement(autoCommObj.StatementNum); //Retrieve statement from DB to update with a ShortGuid from WSHQ and replace balance totals
        if (statement != null)
        {
            ReplaceOneTag(sbTemplate, STATEMENT_BALANCE_TAG, statement.BalTotal.ToString("0.00"), isEmail);
            ReplaceOneTag(sbTemplate, STATEMENT_INS_EST_TAG, statement.InsEst.ToString("0.00"), isEmail);
        }

        if (sbTemplate.ToString().ToLower().Contains(STATEMENT_URL_TAG.ToLower()) //This tag triggers a call to WSHQ to generate a ShortGuid for the statement
            || sbTemplate.ToString().ToLower().Contains(STATEMENT_SHORT_TAG.ToLower()) //This tag triggers a call to WSHQ to generate a ShortGuid for the statement
            || sbTemplate.ToString().Contains(MSG_TO_PAY_TAG)) //This tag triggers a call to WSHQ to generate a ShortGuid for the statement which will then also be used for MsgToPay redirection.
        {
            if (statement != null)
            {
                //This goes to WSHQ to generate a ShortGuidLookup at HQ and URLs for the statement. Statements are updated in the db in this method.
                Statements.AssignURLsIfNecessary(statement, patient);
                    
                ReplaceOneTag(sbTemplate, STATEMENT_URL_TAG, statement.StatementURL, isEmail);
                ReplaceOneTag(sbTemplate, STATEMENT_SHORT_TAG, statement.StatementShortURL, isEmail);
                if (!string.IsNullOrWhiteSpace(statement.ShortGUID) && sbTemplate.ToString().Contains(MSG_TO_PAY_TAG))
                {
                    var listPayloadItems = new List<PayloadItem>
                    {
                        new(statement.ShortGUID, "ShortGuid"),
                    };
                        
                    // Explicitly chose to make this WSHQ call a seperate call from AssignURLs because in case of backwards compatability issues with the WebMethod
                    var payload = PayloadHelper.CreatePayload(PayloadHelper.CreatePayloadContent(listPayloadItems), eServiceCode.PaymentPortalApi);
                    var msgToPayUrl = WebSerializer.DeserializePrimitive<string>(WebServiceMainHQProxy.GetWebServiceMainHQInstance().GetMsgToPayShortUrl(payload));
                    ReplaceOneTag(sbTemplate, MSG_TO_PAY_TAG, msgToPayUrl, isEmail);
                }
            }
        }

        if (!isEmail)
        {
            return;
        }
            
        // Tags that existed for ReplaceVarsForEmail only
        ReplaceOneTag(sbTemplate, NAMEFL_NOPREF_TAG, patient.GetNameFLnoPref(), true);
        ReplaceOneTag(sbTemplate, NAMEF_NOPREF_TAG, patient.FName, true);
    }

    public string ReplaceTagsForStatement(string messageTemplate, Patient patient, Statement statement, ClinicDto clinic = null, bool isEmail = false)
    {
        var autoCommObj = new AutoCommObj
        {
            PatNum = patient.PatNum,
            NameF = patient.FName,
            NamePreferredOrFirst = patient.GetNameFirstOrPreferred(),
            NamePreferred = patient.Preferred,
            ProvNum = patient.PriProv,
            StatementNum = statement.StatementNum
        };
            
        clinic ??= Clinics.GetClinic(patient.ClinicNum) ?? Clinics.GetPracticeAsClinicZero();
            
        var sb = new StringBuilder();
        sb.Append(messageTemplate);
        messageTemplate = sb.ToString();
        return ReplaceTags(messageTemplate, autoCommObj, clinic, isEmailBody: isEmail);
    }
}