using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LanguageForeigns
{
    
    public static long Insert(LanguageForeign languageForeign)
    {
        return LanguageForeignCrud.Insert(languageForeign);
    }

    
    public static void Update(LanguageForeign languageForeign)
    {
        LanguageForeignCrud.Update(languageForeign);
    }

    
    public static void Delete(LanguageForeign languageForeign)
    {
        LanguageForeignCrud.Delete(languageForeign.LanguageForeignNum);
    }

    ///<summary>Only used during export to get a list of all translations for specified culture only.</summary>
    public static List<LanguageForeign> GetListForCurrentCulture()
    {
        var command =
            "SELECT * FROM languageforeign "
            + "WHERE Culture='" + CultureInfo.CurrentCulture.Name + "'";
        return LanguageForeignCrud.SelectMany(command);
    }

    ///<summary>Used in FormTranslation to get all translations for all cultures for one classtype</summary>
    public static List<LanguageForeign> GetListForType(string classType)
    {
        var command =
            "SELECT * FROM languageforeign "
            + "WHERE ClassType='" + SOut.String(classType) + "'";
        return LanguageForeignCrud.SelectMany(command);
    }

    /// <summary>
    ///     Used in FormTranslation to get a single entry for the specified culture.  The culture match must be extact.
    ///     If no translation entries, then it returns null.
    /// </summary>
    public static LanguageForeign GetForCulture(List<LanguageForeign> listLanguageForeignsForType, string english, string cultureName)
    {
        for (var i = 0; i < listLanguageForeignsForType.Count; i++)
        {
            if (english != listLanguageForeignsForType[i].English) continue;
            if (cultureName != listLanguageForeignsForType[i].Culture) continue;
            return listLanguageForeignsForType[i];
        }

        return null;
    }

    /// <summary>
    ///     Used in FormTranslation to get a single entry with the same language as the specified culture, but only for a
    ///     different culture.  For instance, if culture is es-PR (Spanish-PuertoRico), then it will return any spanish
    ///     translation that is NOT from Puerto Rico.  If no other translation entries, then it returns null.
    /// </summary>
    public static LanguageForeign GetOther(List<LanguageForeign> listLanguageForeignsForType, string english, string cultureName)
    {
        for (var i = 0; i < listLanguageForeignsForType.Count; i++)
        {
            if (english != listLanguageForeignsForType[i].English) continue;
            if (cultureName == listLanguageForeignsForType[i].Culture) continue;
            if (cultureName.Substring(0, 2) != listLanguageForeignsForType[i].Culture.Substring(0, 2)) continue;
            return listLanguageForeignsForType[i];
        }

        return null;
    }

    #region Cache Pattern

    ///<summary>Utilizes the NonPkAbs version of CacheDict because it uses a custom Key instead of the PK LanguageForeignNum.</summary>
    private class LanguageForeignCache : CacheDictNonPkAbs<LanguageForeign, string, LanguageForeign>
    {
        protected override List<LanguageForeign> GetCacheFromDb()
        {
            if (CultureInfo.CurrentCulture.Name == "en-US") return new List<LanguageForeign>(); //since DataTable is ignored anyway if on the client, this won't crash.
            //load all translations for the current culture, using other culture of same language if no trans avail.
            var command =
                "SELECT * FROM languageforeign "
                + "WHERE Culture LIKE '" + CultureInfo.CurrentCulture.TwoLetterISOLanguageName + "%' "
                + "ORDER BY Culture";
            return LanguageForeignCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly LanguageForeignCache _languageForeignCache = new();

    public static LanguageForeign GetOne(string classTypeEnglish)
    {
        return _languageForeignCache.GetOne(classTypeEnglish);
    }

    public static bool GetContainsKey(string classTypeEnglish)
    {
        return _languageForeignCache.GetContainsKey(classTypeEnglish);
    }

    /// <summary>
    ///     This will not do anything if the current region is US.
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        if (CultureInfo.CurrentCulture.Name == "en-US") return null;
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _languageForeignCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _languageForeignCache.GetTableFromCache(doRefreshCache);
    }

    #endregion Cache Pattern
}