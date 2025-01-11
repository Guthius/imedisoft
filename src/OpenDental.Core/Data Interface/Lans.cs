using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>Handles database commands for the language table in the database.</summary>
public class Lans
{
    ///<summary>Converts a string to the current language.</summary>
    public static string g(string classType, string text)
    {
        var retVal = ConvertString(classType, text);
        return retVal;
    }

    ///<summary>Converts a string to the current language.</summary>
    public static string g(object sender, string text)
    {
        var retVal = ConvertString(sender.GetType().Name, text);
        return retVal;
    }

    /// <summary>
    ///     This is where all the action happens.  This method is used by all the others.
    ///     This is always run on the client rather than the server, unless, of course, it's being called from the server.
    ///     If it inserts an item into the db table, it will also add it to the local cache, but will not trigger a refresh on
    ///     both ends.
    /// </summary>
    public static string ConvertString(string classType, string text)
    {
        return text;
    }
    
    public static long Insert(Language language)
    {
        return LanguageCrud.Insert(language);
    }

    /*
    ///<summary>not used to update the english version of text.  Create new instead.</summary>
    public static void UpdateCur(){
        string command="UPDATE language SET "
            +"EnglishComments = '" +POut.PString(Cur.EnglishComments)+"'"
            +",IsObsolete = '"     +POut.PBool  (Cur.IsObsolete)+"'"
            +" WHERE ClassType = BINARY '"+POut.PString(Cur.ClassType)+"'"
            +" AND English = BINARY '"     +POut.PString(Cur.English)+"'";
        NonQ(false);
    }*/

    ///<summary>No need to refresh after this.</summary>
    public static void DeleteItems(string classType, List<string> listEnglish)
    {
        var command = "DELETE FROM language WHERE ClassType='" + SOut.String(classType) + "' AND (";
        for (var i = 0; i < listEnglish.Count; i++)
        {
            if (i > 0) command += "OR ";

            command += "English='" + SOut.String(listEnglish[i]) + "' ";
            _languageCache.RemoveKey(classType + listEnglish[i]);
        }

        command += ")";
        Db.NonQ(command);
    }

    
    public static List<string> GetListCat()
    {
        var command = "SELECT Distinct ClassType FROM language ORDER BY ClassType ";
        var table = DataCore.GetTable(command);
        var listCats = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listCats.Add(SIn.String(table.Rows[i][0].ToString()));

        return listCats;
    }

    ///<summary>Only used in translation tool to get list for one category</summary>
    public static List<Language> GetListForCat(string classType)
    {
        var command = "SELECT * FROM language "
                      + "WHERE ClassType = BINARY '" + SOut.String(classType) + "' ORDER BY English";
        return LanguageCrud.SelectMany(command);
    }

    ///<summary>This had to be added because SilverLight does not allow globally setting the current culture format.</summary>
    public static string GetShortDateTimeFormat()
    {
        if (CultureInfo.CurrentCulture.Name == "en-US")
            //DateTimeFormatInfo formatinfo=(DateTimeFormatInfo)CultureInfo.CurrentCulture.DateTimeFormat.Clone();
            //formatinfo.ShortDatePattern="MM/dd/yyyy";
            //return formatinfo;
            return "MM/dd/yyyy";

        return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
    }

    /// <summary>
    ///     Gets a short time format for displaying in appt and schedule along the sides. Pass in a clone of the current
    ///     culture; it will get altered. Returns a string format.
    /// </summary>
    public static string GetShortTimeFormat(CultureInfo cultureInfo)
    {
        var hFormat = "";
        cultureInfo.DateTimeFormat.AMDesignator = cultureInfo.DateTimeFormat.AMDesignator.ToLower();
        cultureInfo.DateTimeFormat.PMDesignator = cultureInfo.DateTimeFormat.PMDesignator.ToLower();
        var shortTimePattern = cultureInfo.DateTimeFormat.ShortTimePattern;
        if (shortTimePattern.IndexOf("hh") != -1)
            //if hour is 01-12
            hFormat += "hh";
        else if (shortTimePattern.IndexOf("h") != -1)
            //or if hour is 1-12
            hFormat += "h";
        else if (shortTimePattern.IndexOf("HH") != -1)
            //or if hour is 00-23
            hFormat += "HH";
        else
            //hour is 0-23
            hFormat += "H";

        if (shortTimePattern.IndexOf("t") != -1)
            //if there is an am/pm designator
            hFormat += "tt";
        else
            //if no am/pm designator, then use :00
            hFormat += ":00"; //time separator will actually change according to region

        return hFormat;
    }

    /// <summary>
    ///     This is one rare situation where queries can be passed.  But it will always fail for client web and server
    ///     web.
    /// </summary>
    public static void LoadTranslationsFromTextFile(string content)
    {
        Db.NonQ(content);
    }

    #region CachePattern

    ///<summary>Utilizes the NonPkAbs version of CacheDict because it uses a custom Key instead of the PK LanguageNum.</summary>
    private class LanguageCache : CacheDictNonPkAbs<Language, string, Language>
    {
        protected override List<Language> GetCacheFromDb()
        {
            var command = "SELECT * FROM language";
            return LanguageCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly LanguageCache _languageCache = new();

    public static bool DictIsNull()
    {
        return _languageCache.DictIsNull();
    }

    /// <summary>
    ///     Does not do anything if the current region is US.
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
        if (CultureInfo.CurrentCulture.Name == "en-US") return;

        _languageCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _languageCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _languageCache.ClearCache();
    }

    #endregion
}