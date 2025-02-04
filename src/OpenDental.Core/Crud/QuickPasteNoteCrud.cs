using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class QuickPasteNoteCrud
{
    public static List<QuickPasteNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<QuickPasteNote> TableToList(DataTable table)
    {
        var retVal = new List<QuickPasteNote>();
        foreach (DataRow row in table.Rows)
        {
            var quickPasteNote = new QuickPasteNote
            {
                QuickPasteNoteNum = SIn.Long(row["QuickPasteNoteNum"].ToString()),
                QuickPasteCatNum = SIn.Long(row["QuickPasteCatNum"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                Abbreviation = SIn.String(row["Abbreviation"].ToString())
            };
            retVal.Add(quickPasteNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<QuickPasteNote> listQuickPasteNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "QuickPasteNote";
        var table = new DataTable(tableName);
        table.Columns.Add("QuickPasteNoteNum");
        table.Columns.Add("QuickPasteCatNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Note");
        table.Columns.Add("Abbreviation");
        foreach (var quickPasteNote in listQuickPasteNotes)
            table.Rows.Add(SOut.Long(quickPasteNote.QuickPasteNoteNum), SOut.Long(quickPasteNote.QuickPasteCatNum), SOut.Int(quickPasteNote.ItemOrder), quickPasteNote.Note, quickPasteNote.Abbreviation);
        return table;
    }

    public static void Insert(QuickPasteNote quickPasteNote)
    {
        var command = "INSERT INTO quickpastenote (";

        command += "QuickPasteCatNum,ItemOrder,Note,Abbreviation) VALUES(";

        command +=
            SOut.Long(quickPasteNote.QuickPasteCatNum) + ","
                                                       + SOut.Int(quickPasteNote.ItemOrder) + ","
                                                       + DbHelper.ParamChar + "paramNote,"
                                                       + "'" + SOut.String(quickPasteNote.Abbreviation) + "')";
        if (quickPasteNote.Note == null) quickPasteNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(quickPasteNote.Note));
        {
            quickPasteNote.QuickPasteNoteNum = Db.NonQ(command, true, "QuickPasteNoteNum", "quickPasteNote", paramNote);
        }
    }

    public static bool Update(QuickPasteNote quickPasteNote, QuickPasteNote oldQuickPasteNote)
    {
        var command = "";
        if (quickPasteNote.QuickPasteCatNum != oldQuickPasteNote.QuickPasteCatNum)
        {
            if (command != "") command += ",";
            command += "QuickPasteCatNum = " + SOut.Long(quickPasteNote.QuickPasteCatNum) + "";
        }

        if (quickPasteNote.ItemOrder != oldQuickPasteNote.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(quickPasteNote.ItemOrder) + "";
        }

        if (quickPasteNote.Note != oldQuickPasteNote.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (quickPasteNote.Abbreviation != oldQuickPasteNote.Abbreviation)
        {
            if (command != "") command += ",";
            command += "Abbreviation = '" + SOut.String(quickPasteNote.Abbreviation) + "'";
        }

        if (command == "") return false;
        if (quickPasteNote.Note == null) quickPasteNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(quickPasteNote.Note));
        command = "UPDATE quickpastenote SET " + command
                                               + " WHERE QuickPasteNoteNum = " + SOut.Long(quickPasteNote.QuickPasteNoteNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static void DeleteMany(List<long> listQuickPasteNoteNums)
    {
        if (listQuickPasteNoteNums == null || listQuickPasteNoteNums.Count == 0) return;
        var command = "DELETE FROM quickpastenote "
                      + "WHERE QuickPasteNoteNum IN(" + string.Join(",", listQuickPasteNoteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<QuickPasteNote> listNew, List<QuickPasteNote> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<QuickPasteNote>();
        var listUpdNew = new List<QuickPasteNote>();
        var listUpdDB = new List<QuickPasteNote>();
        var listDel = new List<QuickPasteNote>();
        listNew.Sort((x, y) => { return x.QuickPasteNoteNum.CompareTo(y.QuickPasteNoteNum); });
        listDB.Sort((x, y) => { return x.QuickPasteNoteNum.CompareTo(y.QuickPasteNoteNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            QuickPasteNote fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            QuickPasteNote fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.QuickPasteNoteNum < fieldDB.QuickPasteNoteNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.QuickPasteNoteNum > fieldDB.QuickPasteNoteNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.QuickPasteNoteNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}