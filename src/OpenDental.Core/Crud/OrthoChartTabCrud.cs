#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoChartTabCrud
{
    public static OrthoChartTab SelectOne(long orthoChartTabNum)
    {
        var command = "SELECT * FROM orthocharttab "
                      + "WHERE OrthoChartTabNum = " + SOut.Long(orthoChartTabNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoChartTab SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoChartTab> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoChartTab> TableToList(DataTable table)
    {
        var retVal = new List<OrthoChartTab>();
        OrthoChartTab orthoChartTab;
        foreach (DataRow row in table.Rows)
        {
            orthoChartTab = new OrthoChartTab();
            orthoChartTab.OrthoChartTabNum = SIn.Long(row["OrthoChartTabNum"].ToString());
            orthoChartTab.TabName = SIn.String(row["TabName"].ToString());
            orthoChartTab.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            orthoChartTab.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            retVal.Add(orthoChartTab);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoChartTab> listOrthoChartTabs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoChartTab";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoChartTabNum");
        table.Columns.Add("TabName");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        foreach (var orthoChartTab in listOrthoChartTabs)
            table.Rows.Add(SOut.Long(orthoChartTab.OrthoChartTabNum), orthoChartTab.TabName, SOut.Int(orthoChartTab.ItemOrder), SOut.Bool(orthoChartTab.IsHidden));
        return table;
    }

    public static long Insert(OrthoChartTab orthoChartTab)
    {
        return Insert(orthoChartTab, false);
    }

    public static long Insert(OrthoChartTab orthoChartTab, bool useExistingPK)
    {
        var command = "INSERT INTO orthocharttab (";

        command += "TabName,ItemOrder,IsHidden) VALUES(";

        command +=
            "'" + SOut.String(orthoChartTab.TabName) + "',"
            + SOut.Int(orthoChartTab.ItemOrder) + ","
            + SOut.Bool(orthoChartTab.IsHidden) + ")";
        {
            orthoChartTab.OrthoChartTabNum = Db.NonQ(command, true, "OrthoChartTabNum", "orthoChartTab");
        }
        return orthoChartTab.OrthoChartTabNum;
    }

    public static long InsertNoCache(OrthoChartTab orthoChartTab)
    {
        return InsertNoCache(orthoChartTab, false);
    }

    public static long InsertNoCache(OrthoChartTab orthoChartTab, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthocharttab (";
        if (isRandomKeys || useExistingPK) command += "OrthoChartTabNum,";
        command += "TabName,ItemOrder,IsHidden) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoChartTab.OrthoChartTabNum) + ",";
        command +=
            "'" + SOut.String(orthoChartTab.TabName) + "',"
            + SOut.Int(orthoChartTab.ItemOrder) + ","
            + SOut.Bool(orthoChartTab.IsHidden) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoChartTab.OrthoChartTabNum = Db.NonQ(command, true, "OrthoChartTabNum", "orthoChartTab");
        return orthoChartTab.OrthoChartTabNum;
    }

    public static void Update(OrthoChartTab orthoChartTab)
    {
        var command = "UPDATE orthocharttab SET "
                      + "TabName         = '" + SOut.String(orthoChartTab.TabName) + "', "
                      + "ItemOrder       =  " + SOut.Int(orthoChartTab.ItemOrder) + ", "
                      + "IsHidden        =  " + SOut.Bool(orthoChartTab.IsHidden) + " "
                      + "WHERE OrthoChartTabNum = " + SOut.Long(orthoChartTab.OrthoChartTabNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoChartTab orthoChartTab, OrthoChartTab oldOrthoChartTab)
    {
        var command = "";
        if (orthoChartTab.TabName != oldOrthoChartTab.TabName)
        {
            if (command != "") command += ",";
            command += "TabName = '" + SOut.String(orthoChartTab.TabName) + "'";
        }

        if (orthoChartTab.ItemOrder != oldOrthoChartTab.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(orthoChartTab.ItemOrder) + "";
        }

        if (orthoChartTab.IsHidden != oldOrthoChartTab.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(orthoChartTab.IsHidden) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthocharttab SET " + command
                                              + " WHERE OrthoChartTabNum = " + SOut.Long(orthoChartTab.OrthoChartTabNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoChartTab orthoChartTab, OrthoChartTab oldOrthoChartTab)
    {
        if (orthoChartTab.TabName != oldOrthoChartTab.TabName) return true;
        if (orthoChartTab.ItemOrder != oldOrthoChartTab.ItemOrder) return true;
        if (orthoChartTab.IsHidden != oldOrthoChartTab.IsHidden) return true;
        return false;
    }

    public static void Delete(long orthoChartTabNum)
    {
        var command = "DELETE FROM orthocharttab "
                      + "WHERE OrthoChartTabNum = " + SOut.Long(orthoChartTabNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoChartTabNums)
    {
        if (listOrthoChartTabNums == null || listOrthoChartTabNums.Count == 0) return;
        var command = "DELETE FROM orthocharttab "
                      + "WHERE OrthoChartTabNum IN(" + string.Join(",", listOrthoChartTabNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<OrthoChartTab> listNew, List<OrthoChartTab> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<OrthoChartTab>();
        var listUpdNew = new List<OrthoChartTab>();
        var listUpdDB = new List<OrthoChartTab>();
        var listDel = new List<OrthoChartTab>();
        listNew.Sort((x, y) => { return x.OrthoChartTabNum.CompareTo(y.OrthoChartTabNum); });
        listDB.Sort((x, y) => { return x.OrthoChartTabNum.CompareTo(y.OrthoChartTabNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        OrthoChartTab fieldNew;
        OrthoChartTab fieldDB;
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

            if (fieldNew.OrthoChartTabNum < fieldDB.OrthoChartTabNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.OrthoChartTabNum > fieldDB.OrthoChartTabNum)
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

        DeleteMany(listDel.Select(x => x.OrthoChartTabNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}