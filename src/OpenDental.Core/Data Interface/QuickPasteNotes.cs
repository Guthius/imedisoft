using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class QuickPasteNotes
{
    #region Insert

    
    public static long Insert(QuickPasteNote note)
    {
        return QuickPasteNoteCrud.Insert(note);
    }

    #endregion

    #region Update

    
    public static void Update(QuickPasteNote note)
    {
        QuickPasteNoteCrud.Update(note);
    }

    #endregion

    #region Delete

    
    public static void Delete(QuickPasteNote note)
    {
        var command = "DELETE from quickpastenote WHERE QuickPasteNoteNum = '"
                      + SOut.Long(note.QuickPasteNoteNum) + "'";
        Db.NonQ(command);
    }

    #endregion

    #region Get Methods

    ///<summary>Only used from FormQuickPaste to get all notes for the selected cat.</summary>
    public static QuickPasteNote[] GetForCat(long cat)
    {
        return GetWhere(x => x.QuickPasteCatNum == cat).ToArray();
    }

    ///<summary>Gets all notes for the selected cats passed in.  The notes returned will be ordered by listCats passed in.</summary>
    public static List<QuickPasteNote> GetForCats(List<QuickPasteCat> listCats)
    {
        var listQuickNotes = new List<QuickPasteNote>();
        if (listCats.Count == 0) return listQuickNotes;
        //Add all quick notes to listQuickNotes from the categories passed in.  Preserve the order of the categories by looping one at a time.
        foreach (var cat in listCats) listQuickNotes.AddRange(GetWhere(y => cat.QuickPasteCatNum == y.QuickPasteCatNum));
        return listQuickNotes;
    }

    #endregion

    #region Misc Methods

    /// <summary>
    ///     When saving an abbrev, this makes sure that the abbreviation is not already in use.
    ///     This checks the current cache for duplicates.
    /// </summary>
    public static string AbbrAlreadyInUse(QuickPasteNote note)
    {
        var msgText = "";
        var listQuickPasteCats = QuickPasteCats.GetDeepCopy();
        var listDuplicates = GetWhere(x => note.Abbreviation == x.Abbreviation && note.QuickPasteNoteNum != x.QuickPasteNoteNum).ToList();
        if (listDuplicates.Count <= 0) return msgText;
        msgText = Lans.g("FormQuickPasteNoteEdit", "The abbreviation")
                  + " '" + note.Abbreviation + "' " + Lans.g("FormQuickPasteNoteEdit", "is in use in the categories:") + "\r\n"
                  + string.Join(", ", listQuickPasteCats.Where(x => listDuplicates.Select(z => z.QuickPasteCatNum).Contains(x.QuickPasteCatNum)).Select(x => x.Description))
                  + "\r\n" + Lans.g("FormQuickPasteNoteEdit", "Do you wish to continue?");
        return msgText;
    }

    /// <summary>
    ///     Called on KeyUp from various textBoxes in the program to look for a ?abbrev and attempt to substitute.
    ///     Substitutes the text if found.
    /// </summary>
    public static string Substitute(string text, EnumQuickPasteType type)
    {
        var listQuickPasteCatsForType = QuickPasteCats.GetCategoriesForType(type);
        if (listQuickPasteCatsForType.Count == 0) return text;
        var listQuickPasteNotes = GetForCats(listQuickPasteCatsForType.OrderBy(x => x.ItemOrder).ToList());
        for (var i = 0; i < listQuickPasteNotes.Count; i++)
        {
            if (listQuickPasteNotes[i].Abbreviation == "") continue;
            //We have to replace all $ chars with $$ because Regex.Replace allows "Substitutions" in the replacement parameter.
            //The replacement parameter specifies the string that is to replace each match in input. replacement can consist of any combination of literal
            //text and substitutions. For example, the replacement pattern a*${test}b inserts the string "a*" followed by the substring that is matched by
            //the test capturing group, if any, followed by the string "b". 
            //The * character is not recognized as a metacharacter within a replacement pattern.
            //See https://msdn.microsoft.com/en-us/library/taz3ak2f(v=vs.110).aspx for more information.
            var quicknote = listQuickPasteNotes[i].Note.Replace("$", "$$");
            //Techs were complaining about quick notes replacing text that was pasted into text boxes (e.g. when a URL happens to have ?... that matches a quick note abbr).
            //The easiest way to deal with this is to not allow the regular expression to replace strings that have a non-whitespace character before or after the abbr.
            //The regex of '...(?<!\S)...' is utilizing an optional space via a lookbehind and visa versa with '...(?!\S)...' as a lookahead.
            var pattern = @"(?<spaceBefore>(?<!\S))\?" + Regex.Escape(listQuickPasteNotes[i].Abbreviation) + @"(?<spaceAfter>(?!\S))";
            var replacePattern = "${spaceBefore}" + quicknote + "${spaceAfter}";
            text = Regex.Replace(text, pattern, replacePattern, RegexOptions.None);
        }

        //If we didn't find any matches then return the passed in text
        return text;
    }

    /// <summary>
    ///     This should not be passing in two lists. Consider rewriting to only pass in one list and an identifier to get
    ///     list from DB.
    /// </summary>
    public static bool Sync(List<QuickPasteNote> listNew, List<QuickPasteNote> listOld)
    {
        //Eventually we may want to change this to not be passing in listOld.
        return QuickPasteNoteCrud.Sync(listNew.Select(x => x.Copy()).ToList(), listOld.Select(x => x.Copy()).ToList());
    }

    ///<summary>Gets multiple QuickPasteNotes from the database. Returns empty list if not found.</summary>
    public static List<QuickPasteNote> GetQuickPasteNotesForApi(int limit, int offset, long quickPasteCatNum)
    {
        var command = "SELECT * FROM quickpastenote ";
        if (quickPasteCatNum > -1) command += "WHERE QuickPasteCatNum=" + SOut.Long(quickPasteCatNum) + " ";
        command += "ORDER BY QuickPasteNoteNum " //same fixed order each time
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        var listQuickPasteNotes = QuickPasteNoteCrud.SelectMany(command);
        return listQuickPasteNotes;
    }

    #endregion

    #region CachePattern

    private class QuickPasteNoteCache : CacheListAbs<QuickPasteNote>
    {
        protected override List<QuickPasteNote> GetCacheFromDb()
        {
            var command =
                "SELECT * from quickpastenote "
                + "ORDER BY ItemOrder";
            return QuickPasteNoteCrud.SelectMany(command);
        }

        protected override List<QuickPasteNote> TableToList(DataTable dataTable)
        {
            return QuickPasteNoteCrud.TableToList(dataTable);
        }

        protected override QuickPasteNote Copy(QuickPasteNote item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<QuickPasteNote> items)
        {
            return QuickPasteNoteCrud.ListToTable(items, "QuickPasteNote");
        }

        protected override void FillCacheIfNeeded()
        {
            QuickPasteNotes.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly QuickPasteNoteCache _QuickPasteNoteCache = new();

    public static List<QuickPasteNote> GetDeepCopy(bool isShort = false)
    {
        return _QuickPasteNoteCache.GetDeepCopy(isShort);
    }

    public static List<QuickPasteNote> GetWhere(Predicate<QuickPasteNote> match, bool isShort = false)
    {
        return _QuickPasteNoteCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _QuickPasteNoteCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _QuickPasteNoteCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _QuickPasteNoteCache.ClearCache();
    }

    #endregion
}