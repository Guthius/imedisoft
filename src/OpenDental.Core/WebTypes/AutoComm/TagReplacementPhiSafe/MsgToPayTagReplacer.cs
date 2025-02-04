using System.Text;
using CodeBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;

namespace OpenDentBusiness.AutoComm;

public class MsgToPayTagReplacer : TagReplacer
{
    public const string MonthlyCardTag = "[monthlyCardsOnFile]";
    public const string NamePrefTag = "[namePref]";
    public const string PatnumTag = "[PatNum]";
    public const string CurmonthTag = "[currentMonth]";
    public const string StatementUrlTag = "[StatementURL]";
    public const string StatementShortTag = "[StatementShortURL]";
    public const string MsgToPayTag = "[MsgToPayURL]";
    public const string StatementBalanceTag = "[StatementBalance]";
    public const string StatementInsEstTag = "[StatementInsuranceEst]";
    public const string NameflNoprefTag = "[nameFLnoPref]";
    public const string NamefNoprefTag = "[nameFnoPref]";

    protected override void ReplaceTagsChild(StringBuilder sbTemplate, AutoCommObj autoCommObj, bool isEmail)
    {
        base.ReplaceTagsChild(sbTemplate, autoCommObj, isEmail);

        if (sbTemplate.ToString().Contains(MonthlyCardTag))
        {
            ReplaceOneTag(sbTemplate, MonthlyCardTag, CreditCards.GetMonthlyCardsOnFile(autoCommObj.PatNum), isEmail);
        }

        ReplaceOneTag(sbTemplate, NamePrefTag, autoCommObj.NamePreferred, isEmail);
        ReplaceOneTag(sbTemplate, PatnumTag, autoCommObj.PatNum.ToString(), isEmail);
        ReplaceOneTag(sbTemplate, CurmonthTag, DateTime_.Now.ToString("MMMM"), isEmail);

        var patient = Patients.GetPat(autoCommObj.PatNum);
        var statement = Statements.GetStatement(autoCommObj.StatementNum);
        if (statement != null)
        {
            ReplaceOneTag(sbTemplate, StatementBalanceTag, statement.BalTotal.ToString("0.00"), isEmail);
            ReplaceOneTag(sbTemplate, StatementInsEstTag, statement.InsEst.ToString("0.00"), isEmail);
        }

        if (!isEmail)
        {
            return;
        }

        ReplaceOneTag(sbTemplate, NameflNoprefTag, patient.GetNameFLnoPref(), true);
        ReplaceOneTag(sbTemplate, NamefNoprefTag, patient.FName, true);
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

        var stringBuilder = new StringBuilder();

        stringBuilder.Append(messageTemplate);

        messageTemplate = stringBuilder.ToString();

        return ReplaceTags(messageTemplate, autoCommObj, clinic, isEmailBody: isEmail);
    }
}