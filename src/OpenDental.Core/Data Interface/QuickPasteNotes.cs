using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class QuickPasteNotes
{
    public static List<QuickPasteNote> GetForCats(List<QuickPasteCat> listCats)
    {
        var listQuickNotes = new List<QuickPasteNote>();
        if (listCats.Count == 0) return listQuickNotes;
        //Add all quick notes to listQuickNotes from the categories passed in.  Preserve the order of the categories by looping one at a time.
        foreach (var cat in listCats) listQuickNotes.AddRange(GetWhere(y => cat.QuickPasteCatNum == y.QuickPasteCatNum));
        return listQuickNotes;
    }

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

        return text;
    }

    public static bool Sync(List<QuickPasteNote> listNew, List<QuickPasteNote> listOld)
    {
        return QuickPasteNoteCrud.Sync(listNew.Select(x => x.Copy()).ToList(), listOld.Select(x => x.Copy()).ToList());
    }

    private class QuickPasteNoteCache : CacheListAbs<QuickPasteNote>
    {
        protected override List<QuickPasteNote> GetCacheFromDb()
        {
            return QuickPasteNoteCrud.SelectMany("SELECT * from quickpastenote ORDER BY ItemOrder");
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

    private static readonly QuickPasteNoteCache Cache = new();

    public static List<QuickPasteNote> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<QuickPasteNote> GetWhere(Predicate<QuickPasteNote> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
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