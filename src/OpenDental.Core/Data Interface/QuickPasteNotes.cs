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
    public static List<QuickPasteNote> GetForCats(List<QuickPasteCat> quickPasteCats)
    {
        if (quickPasteCats.Count == 0)
        {
            return [];
        }

        var quickPasteNotes = new List<QuickPasteNote>();
        foreach (var quickPasteCat in quickPasteCats)
        {
            quickPasteNotes.AddRange(GetWhere(y => quickPasteCat.QuickPasteCatNum == y.QuickPasteCatNum));
        }

        return quickPasteNotes;
    }

    public static string AbbrAlreadyInUse(QuickPasteNote note)
    {
        var quickPasteCats = QuickPasteCats.GetDeepCopy();

        var duplicates = GetWhere(x => note.Abbreviation == x.Abbreviation && note.QuickPasteNoteNum != x.QuickPasteNoteNum).ToList();
        if (duplicates.Count <= 0)
        {
            return string.Empty;
        }

        var descriptions = quickPasteCats.Where(x => duplicates.Select(z => z.QuickPasteCatNum).Contains(x.QuickPasteCatNum)).Select(x => x.Description);

        return
            "The abbreviation '" + note.Abbreviation + "' is in use in the categories:\r\n" +
            string.Join(", ", descriptions) + "\r\n" +
            "Do you wish to continue?";
    }

    public static string Substitute(string input, EnumQuickPasteType type)
    {
        var quickPasteCatsForType = QuickPasteCats.GetCategoriesForType(type);
        if (quickPasteCatsForType.Count == 0)
        {
            return input;
        }
        
        var quickPasteNotes = GetForCats(quickPasteCatsForType.OrderBy(x => x.ItemOrder).ToList());
        foreach (var quickPasteNote in quickPasteNotes)
        {
            if (quickPasteNote.Abbreviation == "")
            {
                continue;
            }

            var note = quickPasteNote.Note.Replace("$", "$$");
            var pattern = @"(?<spaceBefore>(?<!\S))\?" + Regex.Escape(quickPasteNote.Abbreviation) + @"(?<spaceAfter>(?!\S))";
            var replacement = "${spaceBefore}" + note + "${spaceAfter}";
            
            input = Regex.Replace(input, pattern, replacement, RegexOptions.None);
        }

        return input;
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
            GetTableFromCache(false);
        }
    }

    private static readonly QuickPasteNoteCache Cache = new();

    public static List<QuickPasteNote> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<QuickPasteNote> GetWhere(Predicate<QuickPasteNote> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}