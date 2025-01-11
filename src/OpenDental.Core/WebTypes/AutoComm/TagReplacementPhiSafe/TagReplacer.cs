using System.Text;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;

namespace OpenDentBusiness.AutoComm;

public class TagReplacer
{
    protected virtual void ReplaceTagsChild(StringBuilder retVal, AutoCommObj autoCommObj, bool isEmail)
    {
    }

    protected static void ReplaceOneTag(StringBuilder template, string tagToReplace, string replaceValue, bool isEmailBody)
    {
        OpenDentBusiness.ReplaceTags.ReplaceOneTag(template, tagToReplace, replaceValue, isEmailBody);
    }

    public virtual string ReplaceTags(string template, AutoCommObj autoCommObj, ClinicDto clinic, bool isEmailBody)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.Append(template);

        ReplaceOneTag(stringBuilder, "[NameF]", autoCommObj.NameF, isEmailBody);
        ReplaceOneTag(stringBuilder, "[NameFL]", autoCommObj.NameF, isEmailBody);
        ReplaceOneTag(stringBuilder, "[NamePreferredOrFirst]", autoCommObj.NamePreferredOrFirst, isEmailBody);
        ReplaceOneTag(stringBuilder, "[ClinicName]", Clinics.GetOfficeName(clinic), isEmailBody);
        ReplaceOneTag(stringBuilder, "[ClinicPhone]", Clinics.GetOfficePhone(clinic), isEmailBody);
        ReplaceOneTag(stringBuilder, "[OfficePhone]", Clinics.GetOfficePhone(clinic), isEmailBody);
        ReplaceOneTag(stringBuilder, "[OfficeName]", Clinics.GetOfficeName(clinic), isEmailBody);
        ReplaceOneTag(stringBuilder, "[PracticeName]", clinic.Description, isEmailBody);
        ReplaceOneTag(stringBuilder, "[PracticePhone]", Clinics.GetOfficePhone(null), isEmailBody);
        ReplaceOneTag(stringBuilder, "[ProvName]", Providers.GetFormalName(autoCommObj.ProvNum), isEmailBody);
        ReplaceOneTag(stringBuilder, "[ProvAbbr]", Providers.GetAbbr(autoCommObj.ProvNum), isEmailBody);
        ReplaceOneTag(stringBuilder, "[EmailDisclaimer]", EmailMessages.GetEmailDisclaimer(clinic.Id), isEmailBody);

        ReplaceTagsChild(stringBuilder, autoCommObj, isEmailBody);

        return stringBuilder.ToString();
    }
}