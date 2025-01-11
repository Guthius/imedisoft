using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AlertSubCrud
{
    public static List<AlertSub> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AlertSub> TableToList(DataTable table)
    {
        var retVal = new List<AlertSub>();
        AlertSub alertSub;
        foreach (DataRow row in table.Rows)
        {
            alertSub = new AlertSub();
            alertSub.AlertSubNum = SIn.Long(row["AlertSubNum"].ToString());
            alertSub.UserNum = SIn.Long(row["UserNum"].ToString());
            alertSub.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            alertSub.Type = (AlertType) SIn.Int(row["Type"].ToString());
            alertSub.AlertCategoryNum = SIn.Long(row["AlertCategoryNum"].ToString());
            retVal.Add(alertSub);
        }

        return retVal;
    }

    public static long Insert(AlertSub alertSub)
    {
        var command = "INSERT INTO alertsub (";

        command += "UserNum,ClinicNum,Type,AlertCategoryNum) VALUES(";

        command +=
            SOut.Long(alertSub.UserNum) + ","
                                        + SOut.Long(alertSub.ClinicNum) + ","
                                        + SOut.Int((int) alertSub.Type) + ","
                                        + SOut.Long(alertSub.AlertCategoryNum) + ")";


        alertSub.AlertSubNum = Db.NonQ(command, true, "AlertSubNum", "alertSub");

        return alertSub.AlertSubNum;
    }

    public static bool Update(AlertSub alertSub, AlertSub oldAlertSub)
    {
        var command = "";
        if (alertSub.UserNum != oldAlertSub.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(alertSub.UserNum) + "";
        }

        if (alertSub.ClinicNum != oldAlertSub.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(alertSub.ClinicNum) + "";
        }

        if (alertSub.Type != oldAlertSub.Type)
        {
            if (command != "") command += ",";
            command += "Type = " + SOut.Int((int) alertSub.Type) + "";
        }

        if (alertSub.AlertCategoryNum != oldAlertSub.AlertCategoryNum)
        {
            if (command != "") command += ",";
            command += "AlertCategoryNum = " + SOut.Long(alertSub.AlertCategoryNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE alertsub SET " + command
                                         + " WHERE AlertSubNum = " + SOut.Long(alertSub.AlertSubNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listAlertSubNums)
    {
        if (listAlertSubNums == null || listAlertSubNums.Count == 0) return;
        var command = "DELETE FROM alertsub "
                      + "WHERE AlertSubNum IN(" + string.Join(",", listAlertSubNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<AlertSub> listNew, List<AlertSub> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<AlertSub>();
        var listUpdNew = new List<AlertSub>();
        var listUpdDB = new List<AlertSub>();
        var listDel = new List<AlertSub>();
        listNew.Sort((x, y) => { return x.AlertSubNum.CompareTo(y.AlertSubNum); });
        listDB.Sort((x, y) => { return x.AlertSubNum.CompareTo(y.AlertSubNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        AlertSub fieldNew;
        AlertSub fieldDB;
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

            if (fieldNew.AlertSubNum < fieldDB.AlertSubNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.AlertSubNum > fieldDB.AlertSubNum)
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

        DeleteMany(listDel.Select(x => x.AlertSubNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}