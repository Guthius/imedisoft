using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Lans
{
    public static string g(string classType, string text)
    {
        return text;
    }

    public static string g(string text)
    {
        return text;
    }

    public static string GetShortDateTimeFormat()
    {
        return CultureInfo.CurrentCulture.Name == "en-US" ? "MM/dd/yyyy" : CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
    }

    public static string GetShortTimeFormat(CultureInfo cultureInfo)
    {
        var format = "";

        cultureInfo.DateTimeFormat.AMDesignator = cultureInfo.DateTimeFormat.AMDesignator.ToLower();
        cultureInfo.DateTimeFormat.PMDesignator = cultureInfo.DateTimeFormat.PMDesignator.ToLower();

        var shortTimePattern = cultureInfo.DateTimeFormat.ShortTimePattern;
        if (shortTimePattern.Contains("hh"))
            format += "hh";
        else if (shortTimePattern.Contains("h"))
            format += "h";
        else if (shortTimePattern.Contains("HH"))
            format += "HH";
        else
            format += "H";

        if (shortTimePattern.Contains("t"))
            format += "tt";
        else
            format += ":00";

        return format;
    }

    private class LanguageCache : CacheDictNonPkAbs<Language, string, Language>
    {
        protected override List<Language> GetCacheFromDb()
        {
            return LanguageCrud.SelectMany("SELECT * FROM language");
        }

        protected override List<Language> TableToList(DataTable dataTable)
        {
            return LanguageCrud.TableToList(dataTable);
        }

        protected override Language Copy(Language item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<string, Language> dict)
        {
            return LanguageCrud.ListToTable(dict.Values.ToList(), "Language");
        }

        protected override void FillCacheIfNeeded()
        {
            Lans.GetTableFromCache(false);
        }

        protected override string GetDictKey(Language item)
        {
            return item.ClassType + item.English;
        }

        protected override Language GetDictValue(Language item)
        {
            return item;
        }

        protected override Language CopyValue(Language language)
        {
            return language.Copy();
        }

        protected override DataTable ToDataTable(List<Language> items)
        {
            return LanguageCrud.ListToTable(items);
        }
    }

    private static readonly LanguageCache Cache = new();

    public static DataTable RefreshCache()
    {
        return CultureInfo.CurrentCulture.Name == "en-US" ? null : GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}