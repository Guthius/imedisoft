using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class FieldDefLinkCrud
{
    public static List<FieldDefLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FieldDefLink> TableToList(DataTable table)
    {
        var retVal = new List<FieldDefLink>();
        foreach (DataRow row in table.Rows)
        {
            var fieldDefLink = new FieldDefLink
            {
                FieldDefLinkNum = SIn.Long(row["FieldDefLinkNum"].ToString()),
                FieldDefNum = SIn.Long(row["FieldDefNum"].ToString()),
                FieldDefType = (FieldDefTypes) SIn.Int(row["FieldDefType"].ToString()),
                FieldLocation = (FieldLocations) SIn.Int(row["FieldLocation"].ToString())
            };
            retVal.Add(fieldDefLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FieldDefLink> listFieldDefLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FieldDefLink";
        var table = new DataTable(tableName);
        table.Columns.Add("FieldDefLinkNum");
        table.Columns.Add("FieldDefNum");
        table.Columns.Add("FieldDefType");
        table.Columns.Add("FieldLocation");
        foreach (var fieldDefLink in listFieldDefLinks)
            table.Rows.Add(SOut.Long(fieldDefLink.FieldDefLinkNum), SOut.Long(fieldDefLink.FieldDefNum), SOut.Int((int) fieldDefLink.FieldDefType), SOut.Int((int) fieldDefLink.FieldLocation));
        return table;
    }

    public static void Insert(FieldDefLink fieldDefLink)
    {
        var command = "INSERT INTO fielddeflink (";

        command += "FieldDefNum,FieldDefType,FieldLocation) VALUES(";

        command +=
            SOut.Long(fieldDefLink.FieldDefNum) + ","
                                                + SOut.Int((int) fieldDefLink.FieldDefType) + ","
                                                + SOut.Int((int) fieldDefLink.FieldLocation) + ")";
        {
            fieldDefLink.FieldDefLinkNum = Db.NonQ(command, true, "FieldDefLinkNum", "fieldDefLink");
        }
    }

    public static bool Update(FieldDefLink fieldDefLink, FieldDefLink oldFieldDefLink)
    {
        var command = "";
        if (fieldDefLink.FieldDefNum != oldFieldDefLink.FieldDefNum)
        {
            if (command != "") command += ",";
            command += "FieldDefNum = " + SOut.Long(fieldDefLink.FieldDefNum) + "";
        }

        if (fieldDefLink.FieldDefType != oldFieldDefLink.FieldDefType)
        {
            if (command != "") command += ",";
            command += "FieldDefType = " + SOut.Int((int) fieldDefLink.FieldDefType) + "";
        }

        if (fieldDefLink.FieldLocation != oldFieldDefLink.FieldLocation)
        {
            if (command != "") command += ",";
            command += "FieldLocation = " + SOut.Int((int) fieldDefLink.FieldLocation) + "";
        }

        if (command == "") return false;
        command = "UPDATE fielddeflink SET " + command
                                             + " WHERE FieldDefLinkNum = " + SOut.Long(fieldDefLink.FieldDefLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listFieldDefLinkNums)
    {
        if (listFieldDefLinkNums == null || listFieldDefLinkNums.Count == 0) return;
        var command = "DELETE FROM fielddeflink "
                      + "WHERE FieldDefLinkNum IN(" + string.Join(",", listFieldDefLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<FieldDefLink> listNew, List<FieldDefLink> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<FieldDefLink>();
        var listUpdNew = new List<FieldDefLink>();
        var listUpdDB = new List<FieldDefLink>();
        var listDel = new List<FieldDefLink>();
        listNew.Sort((x, y) => { return x.FieldDefLinkNum.CompareTo(y.FieldDefLinkNum); });
        listDB.Sort((x, y) => { return x.FieldDefLinkNum.CompareTo(y.FieldDefLinkNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            FieldDefLink fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            FieldDefLink fieldDB = null;
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

            if (fieldNew.FieldDefLinkNum < fieldDB.FieldDefLinkNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.FieldDefLinkNum > fieldDB.FieldDefLinkNum)
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

        DeleteMany(listDel.Select(x => x.FieldDefLinkNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}