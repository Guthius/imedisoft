using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class LanguageForeigns
{
    public static void Insert(LanguageForeign languageForeign)
    {
        LanguageForeignCrud.Insert(languageForeign);
    }

    public static void Update(LanguageForeign languageForeign)
    {
        LanguageForeignCrud.Update(languageForeign);
    }

    public static void Delete(LanguageForeign languageForeign)
    {
        LanguageForeignCrud.Delete(languageForeign.LanguageForeignNum);
    }

    public static List<LanguageForeign> GetListForCurrentCulture()
    {
        return LanguageForeignCrud.SelectMany("SELECT * FROM languageforeign WHERE Culture='" + CultureInfo.CurrentCulture.Name + "'");
    }

    public static List<LanguageForeign> GetListForType(string classType)
    {
        return LanguageForeignCrud.SelectMany("SELECT * FROM languageforeign WHERE ClassType='" + SOut.String(classType) + "'");
    }

    public static LanguageForeign GetForCulture(List<LanguageForeign> languageForeignsForType, string english, string cultureName)
    {
        foreach (var languageForeign in languageForeignsForType)
        {
            if (english != languageForeign.English)
            {
                continue;
            }

            if (cultureName != languageForeign.Culture)
            {
                continue;
            }

            return languageForeign;
        }

        return null;
    }

    public static LanguageForeign GetOther(List<LanguageForeign> languageForeignsForType, string english, string cultureName)
    {
        foreach (var languageForeign in languageForeignsForType)
        {
            if (english != languageForeign.English)
            {
                continue;
            }

            if (cultureName == languageForeign.Culture)
            {
                continue;
            }

            if (cultureName.Substring(0, 2) != languageForeign.Culture.Substring(0, 2))
            {
                continue;
            }

            return languageForeign;
        }

        return null;
    }

    private class LanguageForeignCache : CacheDictNonPkAbs<LanguageForeign, string, LanguageForeign>
    {
        protected override List<LanguageForeign> GetCacheFromDb()
        {
            if (CultureInfo.CurrentCulture.Name == "en-US")
            {
                return [];
            }

            return LanguageForeignCrud.SelectMany(
                "SELECT * FROM languageforeign " +
                "WHERE Culture LIKE '" + CultureInfo.CurrentCulture.TwoLetterISOLanguageName + "%' " +
                "ORDER BY Culture");
        }

        protected override List<LanguageForeign> TableToList(DataTable dataTable)
        {
            return LanguageForeignCrud.TableToList(dataTable);
        }

        protected override LanguageForeign Copy(LanguageForeign item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<string, LanguageForeign> dict)
        {
            return LanguageForeignCrud.ListToTable(dict.Values.ToList(), "LanguageForeign");
        }

        protected override void FillCacheIfNeeded()
        {
            LanguageForeigns.GetTableFromCache(false);
        }

        protected override string GetDictKey(LanguageForeign item)
        {
            return item.ClassType + item.English;
        }

        protected override LanguageForeign GetDictValue(LanguageForeign item)
        {
            return item;
        }

        protected override LanguageForeign CopyValue(LanguageForeign languageForeign)
        {
            return languageForeign.Copy();
        }

        protected override DataTable ToDataTable(List<LanguageForeign> items)
        {
            return LanguageForeignCrud.ListToTable(items);
        }
    }

    private static readonly LanguageForeignCache Cache = new();

    public static void RefreshCache()
    {
        if (CultureInfo.CurrentCulture.Name == "en-US")
        {
            return;
        }

        GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool doRefreshCache)
    {
        Cache.GetTableFromCache(doRefreshCache);
    }
}