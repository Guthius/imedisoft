using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AlertCategoryLinkCrud
{
    public static List<AlertCategoryLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AlertCategoryLink> TableToList(DataTable table)
    {
        var retVal = new List<AlertCategoryLink>();
        AlertCategoryLink alertCategoryLink;
        foreach (DataRow row in table.Rows)
        {
            alertCategoryLink = new AlertCategoryLink();
            alertCategoryLink.AlertCategoryLinkNum = SIn.Long(row["AlertCategoryLinkNum"].ToString());
            alertCategoryLink.AlertCategoryNum = SIn.Long(row["AlertCategoryNum"].ToString());
            alertCategoryLink.AlertType = (AlertType) SIn.Int(row["AlertType"].ToString());
            retVal.Add(alertCategoryLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AlertCategoryLink> listAlertCategoryLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AlertCategoryLink";
        var table = new DataTable(tableName);
        table.Columns.Add("AlertCategoryLinkNum");
        table.Columns.Add("AlertCategoryNum");
        table.Columns.Add("AlertType");
        foreach (var alertCategoryLink in listAlertCategoryLinks)
            table.Rows.Add(SOut.Long(alertCategoryLink.AlertCategoryLinkNum), SOut.Long(alertCategoryLink.AlertCategoryNum), SOut.Int((int) alertCategoryLink.AlertType));
        return table;
    }

    public static long Insert(AlertCategoryLink alertCategoryLink)
    {
        var command = "INSERT INTO alertcategorylink (";

        command += "AlertCategoryNum,AlertType) VALUES(";

        command +=
            SOut.Long(alertCategoryLink.AlertCategoryNum) + ","
                                                          + SOut.Int((int) alertCategoryLink.AlertType) + ")";
        {
            alertCategoryLink.AlertCategoryLinkNum = Db.NonQ(command, true, "AlertCategoryLinkNum", "alertCategoryLink");
        }
        return alertCategoryLink.AlertCategoryLinkNum;
    }

    public static bool Update(AlertCategoryLink alertCategoryLink, AlertCategoryLink oldAlertCategoryLink)
    {
        var command = "";
        if (alertCategoryLink.AlertCategoryNum != oldAlertCategoryLink.AlertCategoryNum)
        {
            if (command != "") command += ",";
            command += "AlertCategoryNum = " + SOut.Long(alertCategoryLink.AlertCategoryNum) + "";
        }

        if (alertCategoryLink.AlertType != oldAlertCategoryLink.AlertType)
        {
            if (command != "") command += ",";
            command += "AlertType = " + SOut.Int((int) alertCategoryLink.AlertType) + "";
        }

        if (command == "") return false;
        command = "UPDATE alertcategorylink SET " + command
                                                  + " WHERE AlertCategoryLinkNum = " + SOut.Long(alertCategoryLink.AlertCategoryLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listAlertCategoryLinkNums)
    {
        if (listAlertCategoryLinkNums == null || listAlertCategoryLinkNums.Count == 0) return;
        var command = "DELETE FROM alertcategorylink "
                      + "WHERE AlertCategoryLinkNum IN(" + string.Join(",", listAlertCategoryLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<AlertCategoryLink> listNew, List<AlertCategoryLink> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<AlertCategoryLink>();
        var listUpdNew = new List<AlertCategoryLink>();
        var listUpdDB = new List<AlertCategoryLink>();
        var listDel = new List<AlertCategoryLink>();
        listNew.Sort((x, y) => { return x.AlertCategoryLinkNum.CompareTo(y.AlertCategoryLinkNum); });
        listDB.Sort((x, y) => { return x.AlertCategoryLinkNum.CompareTo(y.AlertCategoryLinkNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        AlertCategoryLink fieldNew;
        AlertCategoryLink fieldDB;
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

            if (fieldNew.AlertCategoryLinkNum < fieldDB.AlertCategoryLinkNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.AlertCategoryLinkNum > fieldDB.AlertCategoryLinkNum)
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

        DeleteMany(listDel.Select(x => x.AlertCategoryLinkNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}