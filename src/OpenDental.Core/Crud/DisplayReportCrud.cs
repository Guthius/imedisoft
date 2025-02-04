using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DisplayReportCrud
{
    public static List<DisplayReport> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DisplayReport> TableToList(DataTable table)
    {
        var retVal = new List<DisplayReport>();
        foreach (DataRow row in table.Rows)
        {
            var displayReport = new DisplayReport
            {
                DisplayReportNum = SIn.Long(row["DisplayReportNum"].ToString()),
                InternalName = SIn.String(row["InternalName"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                Category = (DisplayReportCategory) SIn.Int(row["Category"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                IsVisibleInSubMenu = SIn.Bool(row["IsVisibleInSubMenu"].ToString())
            };
            retVal.Add(displayReport);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DisplayReport> listDisplayReports, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DisplayReport";
        var table = new DataTable(tableName);
        table.Columns.Add("DisplayReportNum");
        table.Columns.Add("InternalName");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Description");
        table.Columns.Add("Category");
        table.Columns.Add("IsHidden");
        table.Columns.Add("IsVisibleInSubMenu");
        foreach (var displayReport in listDisplayReports)
            table.Rows.Add(SOut.Long(displayReport.DisplayReportNum), displayReport.InternalName, SOut.Int(displayReport.ItemOrder), displayReport.Description, SOut.Int((int) displayReport.Category), SOut.Bool(displayReport.IsHidden), SOut.Bool(displayReport.IsVisibleInSubMenu));
        return table;
    }

    public static void Insert(DisplayReport displayReport)
    {
        var command = "INSERT INTO displayreport (";

        command += "InternalName,ItemOrder,Description,Category,IsHidden,IsVisibleInSubMenu) VALUES(";

        command +=
            "'" + SOut.String(displayReport.InternalName) + "',"
            + SOut.Int(displayReport.ItemOrder) + ","
            + "'" + SOut.String(displayReport.Description) + "',"
            + SOut.Int((int) displayReport.Category) + ","
            + SOut.Bool(displayReport.IsHidden) + ","
            + SOut.Bool(displayReport.IsVisibleInSubMenu) + ")";
        {
            displayReport.DisplayReportNum = Db.NonQ(command, true, "DisplayReportNum", "displayReport");
        }
    }

    public static bool Update(DisplayReport displayReport, DisplayReport oldDisplayReport)
    {
        var command = "";
        if (displayReport.InternalName != oldDisplayReport.InternalName)
        {
            if (command != "") command += ",";
            command += "InternalName = '" + SOut.String(displayReport.InternalName) + "'";
        }

        if (displayReport.ItemOrder != oldDisplayReport.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(displayReport.ItemOrder) + "";
        }

        if (displayReport.Description != oldDisplayReport.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(displayReport.Description) + "'";
        }

        if (displayReport.Category != oldDisplayReport.Category)
        {
            if (command != "") command += ",";
            command += "Category = " + SOut.Int((int) displayReport.Category) + "";
        }

        if (displayReport.IsHidden != oldDisplayReport.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(displayReport.IsHidden) + "";
        }

        if (displayReport.IsVisibleInSubMenu != oldDisplayReport.IsVisibleInSubMenu)
        {
            if (command != "") command += ",";
            command += "IsVisibleInSubMenu = " + SOut.Bool(displayReport.IsVisibleInSubMenu) + "";
        }

        if (command == "") return false;
        command = "UPDATE displayreport SET " + command
                                              + " WHERE DisplayReportNum = " + SOut.Long(displayReport.DisplayReportNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listDisplayReportNums)
    {
        if (listDisplayReportNums == null || listDisplayReportNums.Count == 0) return;
        var command = "DELETE FROM displayreport "
                      + "WHERE DisplayReportNum IN(" + string.Join(",", listDisplayReportNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<DisplayReport> listNew, List<DisplayReport> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<DisplayReport>();
        var listUpdNew = new List<DisplayReport>();
        var listUpdDB = new List<DisplayReport>();
        var listDel = new List<DisplayReport>();
        listNew.Sort((x, y) => { return x.DisplayReportNum.CompareTo(y.DisplayReportNum); });
        listDB.Sort((x, y) => { return x.DisplayReportNum.CompareTo(y.DisplayReportNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            DisplayReport fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            DisplayReport fieldDB = null;
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

            if (fieldNew.DisplayReportNum < fieldDB.DisplayReportNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.DisplayReportNum > fieldDB.DisplayReportNum)
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

        DeleteMany(listDel.Select(x => x.DisplayReportNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}