using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoChartTabLinkCrud
{
    public static List<OrthoChartTabLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoChartTabLink> TableToList(DataTable table)
    {
        var retVal = new List<OrthoChartTabLink>();
        foreach (DataRow row in table.Rows)
        {
            var orthoChartTabLink = new OrthoChartTabLink
            {
                OrthoChartTabLinkNum = SIn.Long(row["OrthoChartTabLinkNum"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                OrthoChartTabNum = SIn.Long(row["OrthoChartTabNum"].ToString()),
                DisplayFieldNum = SIn.Long(row["DisplayFieldNum"].ToString()),
                ColumnWidthOverride = SIn.Int(row["ColumnWidthOverride"].ToString())
            };
            retVal.Add(orthoChartTabLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoChartTabLink> listOrthoChartTabLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoChartTabLink";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoChartTabLinkNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("OrthoChartTabNum");
        table.Columns.Add("DisplayFieldNum");
        table.Columns.Add("ColumnWidthOverride");
        foreach (var orthoChartTabLink in listOrthoChartTabLinks)
            table.Rows.Add(SOut.Long(orthoChartTabLink.OrthoChartTabLinkNum), SOut.Int(orthoChartTabLink.ItemOrder), SOut.Long(orthoChartTabLink.OrthoChartTabNum), SOut.Long(orthoChartTabLink.DisplayFieldNum), SOut.Int(orthoChartTabLink.ColumnWidthOverride));
        return table;
    }

    public static void Insert(OrthoChartTabLink orthoChartTabLink)
    {
        var command = "INSERT INTO orthocharttablink (";

        command += "ItemOrder,OrthoChartTabNum,DisplayFieldNum,ColumnWidthOverride) VALUES(";

        command +=
            SOut.Int(orthoChartTabLink.ItemOrder) + ","
                                                  + SOut.Long(orthoChartTabLink.OrthoChartTabNum) + ","
                                                  + SOut.Long(orthoChartTabLink.DisplayFieldNum) + ","
                                                  + SOut.Int(orthoChartTabLink.ColumnWidthOverride) + ")";
        {
            orthoChartTabLink.OrthoChartTabLinkNum = Db.NonQ(command, true, "OrthoChartTabLinkNum", "orthoChartTabLink");
        }
    }

    public static bool Update(OrthoChartTabLink orthoChartTabLink, OrthoChartTabLink oldOrthoChartTabLink)
    {
        var command = "";
        if (orthoChartTabLink.ItemOrder != oldOrthoChartTabLink.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(orthoChartTabLink.ItemOrder) + "";
        }

        if (orthoChartTabLink.OrthoChartTabNum != oldOrthoChartTabLink.OrthoChartTabNum)
        {
            if (command != "") command += ",";
            command += "OrthoChartTabNum = " + SOut.Long(orthoChartTabLink.OrthoChartTabNum) + "";
        }

        if (orthoChartTabLink.DisplayFieldNum != oldOrthoChartTabLink.DisplayFieldNum)
        {
            if (command != "") command += ",";
            command += "DisplayFieldNum = " + SOut.Long(orthoChartTabLink.DisplayFieldNum) + "";
        }

        if (orthoChartTabLink.ColumnWidthOverride != oldOrthoChartTabLink.ColumnWidthOverride)
        {
            if (command != "") command += ",";
            command += "ColumnWidthOverride = " + SOut.Int(orthoChartTabLink.ColumnWidthOverride) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthocharttablink SET " + command
                                                  + " WHERE OrthoChartTabLinkNum = " + SOut.Long(orthoChartTabLink.OrthoChartTabLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listOrthoChartTabLinkNums)
    {
        if (listOrthoChartTabLinkNums == null || listOrthoChartTabLinkNums.Count == 0) return;
        var command = "DELETE FROM orthocharttablink "
                      + "WHERE OrthoChartTabLinkNum IN(" + string.Join(",", listOrthoChartTabLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<OrthoChartTabLink> listNew, List<OrthoChartTabLink> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<OrthoChartTabLink>();
        var listUpdNew = new List<OrthoChartTabLink>();
        var listUpdDB = new List<OrthoChartTabLink>();
        var listDel = new List<OrthoChartTabLink>();
        listNew.Sort((x, y) => { return x.OrthoChartTabLinkNum.CompareTo(y.OrthoChartTabLinkNum); });
        listDB.Sort((x, y) => { return x.OrthoChartTabLinkNum.CompareTo(y.OrthoChartTabLinkNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            OrthoChartTabLink fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            OrthoChartTabLink fieldDB = null;
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

            if (fieldNew.OrthoChartTabLinkNum < fieldDB.OrthoChartTabLinkNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.OrthoChartTabLinkNum > fieldDB.OrthoChartTabLinkNum)
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

        DeleteMany(listDel.Select(x => x.OrthoChartTabLinkNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}