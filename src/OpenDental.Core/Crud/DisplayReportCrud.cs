#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DisplayReportCrud
{
    public static DisplayReport SelectOne(long displayReportNum)
    {
        var command = "SELECT * FROM displayreport "
                      + "WHERE DisplayReportNum = " + SOut.Long(displayReportNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DisplayReport SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DisplayReport> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DisplayReport> TableToList(DataTable table)
    {
        var retVal = new List<DisplayReport>();
        DisplayReport displayReport;
        foreach (DataRow row in table.Rows)
        {
            displayReport = new DisplayReport();
            displayReport.DisplayReportNum = SIn.Long(row["DisplayReportNum"].ToString());
            displayReport.InternalName = SIn.String(row["InternalName"].ToString());
            displayReport.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            displayReport.Description = SIn.String(row["Description"].ToString());
            displayReport.Category = (DisplayReportCategory) SIn.Int(row["Category"].ToString());
            displayReport.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            displayReport.IsVisibleInSubMenu = SIn.Bool(row["IsVisibleInSubMenu"].ToString());
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

    public static long Insert(DisplayReport displayReport)
    {
        return Insert(displayReport, false);
    }

    public static long Insert(DisplayReport displayReport, bool useExistingPK)
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
        return displayReport.DisplayReportNum;
    }

    public static long InsertNoCache(DisplayReport displayReport)
    {
        return InsertNoCache(displayReport, false);
    }

    public static long InsertNoCache(DisplayReport displayReport, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO displayreport (";
        if (isRandomKeys || useExistingPK) command += "DisplayReportNum,";
        command += "InternalName,ItemOrder,Description,Category,IsHidden,IsVisibleInSubMenu) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(displayReport.DisplayReportNum) + ",";
        command +=
            "'" + SOut.String(displayReport.InternalName) + "',"
            + SOut.Int(displayReport.ItemOrder) + ","
            + "'" + SOut.String(displayReport.Description) + "',"
            + SOut.Int((int) displayReport.Category) + ","
            + SOut.Bool(displayReport.IsHidden) + ","
            + SOut.Bool(displayReport.IsVisibleInSubMenu) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            displayReport.DisplayReportNum = Db.NonQ(command, true, "DisplayReportNum", "displayReport");
        return displayReport.DisplayReportNum;
    }

    public static void Update(DisplayReport displayReport)
    {
        var command = "UPDATE displayreport SET "
                      + "InternalName      = '" + SOut.String(displayReport.InternalName) + "', "
                      + "ItemOrder         =  " + SOut.Int(displayReport.ItemOrder) + ", "
                      + "Description       = '" + SOut.String(displayReport.Description) + "', "
                      + "Category          =  " + SOut.Int((int) displayReport.Category) + ", "
                      + "IsHidden          =  " + SOut.Bool(displayReport.IsHidden) + ", "
                      + "IsVisibleInSubMenu=  " + SOut.Bool(displayReport.IsVisibleInSubMenu) + " "
                      + "WHERE DisplayReportNum = " + SOut.Long(displayReport.DisplayReportNum);
        Db.NonQ(command);
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

    public static bool UpdateComparison(DisplayReport displayReport, DisplayReport oldDisplayReport)
    {
        if (displayReport.InternalName != oldDisplayReport.InternalName) return true;
        if (displayReport.ItemOrder != oldDisplayReport.ItemOrder) return true;
        if (displayReport.Description != oldDisplayReport.Description) return true;
        if (displayReport.Category != oldDisplayReport.Category) return true;
        if (displayReport.IsHidden != oldDisplayReport.IsHidden) return true;
        if (displayReport.IsVisibleInSubMenu != oldDisplayReport.IsVisibleInSubMenu) return true;
        return false;
    }

    public static void Delete(long displayReportNum)
    {
        var command = "DELETE FROM displayreport "
                      + "WHERE DisplayReportNum = " + SOut.Long(displayReportNum);
        Db.NonQ(command);
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
        DisplayReport fieldNew;
        DisplayReport fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
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