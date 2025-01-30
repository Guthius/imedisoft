using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class LanguagePats
{
    public static string TranslateEFormField(long eFormFieldDefNum, string langDisplay, string defaultText, string langIso3 = "")
    {
        var threeLetterISO = langIso3;
        if (langDisplay != "") threeLetterISO = GetLang3LetterFromDisplay(langDisplay);
        if (threeLetterISO.IsNullOrEmpty()) return defaultText;
        var languagePat = GetFirstOrDefault(x => x.EFormFieldDefNum == eFormFieldDefNum && x.Language == threeLetterISO);
        if (languagePat == null) return defaultText;
        return languagePat.Translation;
    }

    public static bool SaveTranslationEFormField(long eFormFieldDefNum, string langDisplay, string foreignText)
    {
        var threeLetterISO = GetLang3LetterFromDisplay(langDisplay);
        if (threeLetterISO.IsNullOrEmpty()) return false;
        var languagePat = GetFirstOrDefault(x => x.EFormFieldDefNum == eFormFieldDefNum && x.Language == threeLetterISO);
        if (foreignText == "")
        {
            if (languagePat == null) return false;
            Delete(languagePat.LanguagePatNum);
            return true;
        }

        if (languagePat == null)
        {
            languagePat = new LanguagePat();
            languagePat.EFormFieldDefNum = eFormFieldDefNum;
            languagePat.Language = threeLetterISO;
            languagePat.Translation = foreignText;
            LanguagePatCrud.Insert(languagePat);
            return true;
        }

        if (languagePat.Translation != foreignText)
        {
            languagePat.Translation = foreignText;
            LanguagePatCrud.Update(languagePat);
            return true;
        }

        return false;
    }

    public static List<string> GetLanguagesForCombo()
    {
        var listLangsFromPref = PrefC.GetString(PrefName.LanguagesUsedByPatients)
            .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList()
            .FindAll(x => x != Patients.LanguageDeclinedToSpecify);
        //Example: "Declined to Specify,spa,fra,Tahitian"
        //Would result at this point in three items in the list: spa,fra,Tahitian
        var listRet = new List<string>();
        for (var i = 0; i < listLangsFromPref.Count; i++)
        {
            var cultureInfo = MiscUtils.GetCultureFromThreeLetter(listLangsFromPref[i]); //converts 3 char to full name
            if (cultureInfo == null)
            {
                listRet.Add(listLangsFromPref[i]); //Example Tahitian
                continue;
            }

            listRet.Add(SOut.String(cultureInfo.DisplayName)); //long version, like Spanish
        }

        return listRet;
    }

    public static string GetLang3LetterFromDisplay(string languageDisplay)
    {
        var listLangsFromPref = PrefC.GetString(PrefName.LanguagesUsedByPatients)
            .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList()
            .FindAll(x => x != Patients.LanguageDeclinedToSpecify);
        //Example: "Declined to Specify,spa,fra,Tahitian"
        //Would result at this point in three items in the list: spa,fra,Tahitian
        for (var i = 0; i < listLangsFromPref.Count; i++)
        {
            var cultureInfo = MiscUtils.GetCultureFromThreeLetter(listLangsFromPref[i]);
            if (cultureInfo == null)
            {
                if (listLangsFromPref[i] == languageDisplay) return listLangsFromPref[i]; //Example Tahitian
                continue;
            }

            if (cultureInfo.DisplayName == languageDisplay) return listLangsFromPref[i];
        }

        return null; //language wasn't found within LanguagesUsedByPatients pref.
    }

    public static bool SyncRadioButtonTranslations(EFormField eFormField)
    {
        if (eFormField.FieldType != EnumEFormFieldType.RadioButtons) return false;
        var listDisplayLanguages = GetLanguagesForCombo(); //get all languages set up in pref.
        var listVisOrig = eFormField.PickListVis.Split('|').ToList();
        var isChangedLanCache = false;
        for (var i = 0; i < listDisplayLanguages.Count; i++)
        {
            //iterate through each language.
            var strTranslations = TranslateEFormField(eFormField.EFormFieldDefNum, listDisplayLanguages[i], ""); //empty string will indicate no translation exists yet.
            if (strTranslations == "") //nothing to change for a language that hasn't been translated yet.
                continue;
            var listTranslations = strTranslations.Split('|').ToList(); //Ex: [label|button1|button2]
            var listLangOrig = new List<string>(); //only stores the language translations for the buttons, doesn't include the ValueLabel.
            for (var j = 1; j < listTranslations.Count; j++) //add the translated button labels to listLangOrig.
                listLangOrig.Add(listTranslations[j]);
            if (listLangOrig.Count == listVisOrig.Count) continue; //no need to sync, number of elements already match
            var strTranslationsNew = listTranslations[0] + "|"; //add the translated ValueLabel back to a string
            for (var k = 0; k < listVisOrig.Count; k++)
            {
                //iterate through listVisOrig to sync listLangOrig
                if (k > 0) strTranslationsNew += "|";
                if (k < listLangOrig.Count) //if listVisOrig has a greater count, the resulting string will have extra pipes. ex: "Label|Button1|Button2|||"
                    strTranslationsNew += listLangOrig[k];
            }

            isChangedLanCache |= SaveTranslationEFormField(eFormField.EFormFieldDefNum, listDisplayLanguages[i], strTranslationsNew);
        }

        if (isChangedLanCache) RefreshCache();
        return isChangedLanCache;
    }

    private class LanguagePatCache : CacheListAbs<LanguagePat>
    {
        protected override List<LanguagePat> GetCacheFromDb()
        {
            var command = "SELECT * FROM languagepat";
            return LanguagePatCrud.SelectMany(command);
        }

        protected override List<LanguagePat> TableToList(DataTable dataTable)
        {
            return LanguagePatCrud.TableToList(dataTable);
        }

        protected override LanguagePat Copy(LanguagePat item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<LanguagePat> items)
        {
            return LanguagePatCrud.ListToTable(items, "LanguagePat");
        }

        protected override void FillCacheIfNeeded()
        {
            LanguagePats.GetTableFromCache(false);
        }
    }

    private static readonly LanguagePatCache Cache = new();

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static LanguagePat GetFirstOrDefault(Func<LanguagePat, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static string GetPrefTranslation(PrefName prefName, string language, bool defaultToBlank = false)
    {
        var languagePat = GetFirstOrDefault(x => x.Language == language && x.PrefName == prefName.ToString());
        if (languagePat == null || string.IsNullOrWhiteSpace(languagePat.Translation))
        {
            if (defaultToBlank && language != PrefC.GetLanguageAndRegion().ThreeLetterISOLanguageName) return "";
            return PrefC.GetString(prefName); //If default language, use pref value.
        }

        //Use translation from LanguagePat if it exists.
        return languagePat.Translation;
    }

    public static LanguagePat GetPrefTranslationFromDb(PrefName prefName, string language)
    {
        return GetListPrefTranslationsFromDb(ListTools.FromSingle(prefName.ToString()), ListTools.FromSingle(language)).FirstOrDefault();
    }

    public static List<LanguagePat> GetListPrefTranslationsFromDb(List<string> listStrPrefNames, List<string> listLanguages)
    {
        if (listStrPrefNames.IsNullOrEmpty() || listLanguages.IsNullOrEmpty()) return new List<LanguagePat>();

        var command = "SELECT * FROM languagepat "
                      + "WHERE PrefName IN (" + string.Join(",", listStrPrefNames.Select(x => $"'{SOut.String(x)}'")) + ") "
                      + "AND Language IN (" + string.Join(",", listLanguages.Select(x => $"'{SOut.String(x)}'")) + ") ";
        return LanguagePatCrud.SelectMany(command);
    }

    public static void Insert(LanguagePat languagePat)
    {
        LanguagePatCrud.Insert(languagePat);
    }

    public static void UpsertPrefTranslation(PrefName prefName, string language, string translation)
    {
        if (language == PrefC.GetLanguageAndRegion().ThreeLetterISOLanguageName)
        {
            Prefs.UpdateString(prefName, translation); //If default language, update pref value directly.
            return;
        }

        var languagePatNew = GetPrefTranslationFromDb(prefName, language);
        if (languagePatNew == null)
        {
            languagePatNew = new LanguagePat();
            languagePatNew.Language = language;
            languagePatNew.PrefName = prefName.ToString();
            languagePatNew.Translation = translation;
            Insert(languagePatNew);
            return;
        }

        var languagePatOld = languagePatNew.Copy();
        languagePatNew.Translation = translation;
        LanguagePatCrud.Update(languagePatNew, languagePatOld);
    }

    public static void DeleteForEFormFieldDef(long eFormFieldDefNum)
    {
        var command = "DELETE FROM languagepat WHERE EFormFieldDefNum=" + SOut.Long(eFormFieldDefNum);
        var count = Db.NonQ(command);
    }

    public static void Delete(long languagePatNum)
    {
        var command = "DELETE FROM languagepat WHERE LanguagePatNum=" + SOut.Long(languagePatNum);
        var count = Db.NonQ(command);
    }
}