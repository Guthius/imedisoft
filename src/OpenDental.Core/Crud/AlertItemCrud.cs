using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AlertItemCrud
{
    public static List<AlertItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AlertItem> TableToList(DataTable table)
    {
        var retVal = new List<AlertItem>();
        AlertItem alertItem;
        foreach (DataRow row in table.Rows)
        {
            alertItem = new AlertItem();
            alertItem.AlertItemNum = SIn.Long(row["AlertItemNum"].ToString());
            alertItem.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            alertItem.Description = SIn.String(row["Description"].ToString());
            alertItem.Type = (AlertType) SIn.Int(row["Type"].ToString());
            alertItem.Severity = (SeverityType) SIn.Int(row["Severity"].ToString());
            alertItem.Actions = (ActionType) SIn.Int(row["Actions"].ToString());
            alertItem.FormToOpen = (FormType) SIn.Int(row["FormToOpen"].ToString());
            alertItem.FKey = SIn.Long(row["FKey"].ToString());
            alertItem.ItemValue = SIn.String(row["ItemValue"].ToString());
            alertItem.UserNum = SIn.Long(row["UserNum"].ToString());
            alertItem.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            retVal.Add(alertItem);
        }

        return retVal;
    }

    public static long Insert(AlertItem alertItem)
    {
        var command = "INSERT INTO alertitem (";

        command += "ClinicNum,Description,Type,Severity,Actions,FormToOpen,FKey,ItemValue,UserNum,SecDateTEntry) VALUES(";

        command +=
            SOut.Long(alertItem.ClinicNum) + ","
                                           + "'" + SOut.String(alertItem.Description) + "',"
                                           + SOut.Int((int) alertItem.Type) + ","
                                           + SOut.Int((int) alertItem.Severity) + ","
                                           + SOut.Int((int) alertItem.Actions) + ","
                                           + SOut.Int((int) alertItem.FormToOpen) + ","
                                           + SOut.Long(alertItem.FKey) + ","
                                           + "'" + SOut.String(alertItem.ItemValue) + "',"
                                           + SOut.Long(alertItem.UserNum) + ","
                                           + DbHelper.Now() + ")";
        {
            alertItem.AlertItemNum = Db.NonQ(command, true, "AlertItemNum", "alertItem");
        }
        return alertItem.AlertItemNum;
    }

    public static void Update(AlertItem alertItem)
    {
        var command = "UPDATE alertitem SET "
                      + "ClinicNum    =  " + SOut.Long(alertItem.ClinicNum) + ", "
                      + "Description  = '" + SOut.String(alertItem.Description) + "', "
                      + "Type         =  " + SOut.Int((int) alertItem.Type) + ", "
                      + "Severity     =  " + SOut.Int((int) alertItem.Severity) + ", "
                      + "Actions      =  " + SOut.Int((int) alertItem.Actions) + ", "
                      + "FormToOpen   =  " + SOut.Int((int) alertItem.FormToOpen) + ", "
                      + "FKey         =  " + SOut.Long(alertItem.FKey) + ", "
                      + "ItemValue    = '" + SOut.String(alertItem.ItemValue) + "', "
                      + "UserNum      =  " + SOut.Long(alertItem.UserNum) + " "
                      //SecDateTEntry not allowed to change
                      + "WHERE AlertItemNum = " + SOut.Long(alertItem.AlertItemNum);
        Db.NonQ(command);
    }

    public static bool Update(AlertItem alertItem, AlertItem oldAlertItem)
    {
        var command = "";
        if (alertItem.ClinicNum != oldAlertItem.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(alertItem.ClinicNum) + "";
        }

        if (alertItem.Description != oldAlertItem.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(alertItem.Description) + "'";
        }

        if (alertItem.Type != oldAlertItem.Type)
        {
            if (command != "") command += ",";
            command += "Type = " + SOut.Int((int) alertItem.Type) + "";
        }

        if (alertItem.Severity != oldAlertItem.Severity)
        {
            if (command != "") command += ",";
            command += "Severity = " + SOut.Int((int) alertItem.Severity) + "";
        }

        if (alertItem.Actions != oldAlertItem.Actions)
        {
            if (command != "") command += ",";
            command += "Actions = " + SOut.Int((int) alertItem.Actions) + "";
        }

        if (alertItem.FormToOpen != oldAlertItem.FormToOpen)
        {
            if (command != "") command += ",";
            command += "FormToOpen = " + SOut.Int((int) alertItem.FormToOpen) + "";
        }

        if (alertItem.FKey != oldAlertItem.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(alertItem.FKey) + "";
        }

        if (alertItem.ItemValue != oldAlertItem.ItemValue)
        {
            if (command != "") command += ",";
            command += "ItemValue = '" + SOut.String(alertItem.ItemValue) + "'";
        }

        if (alertItem.UserNum != oldAlertItem.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(alertItem.UserNum) + "";
        }

        //SecDateTEntry not allowed to change
        if (command == "") return false;
        command = "UPDATE alertitem SET " + command
                                          + " WHERE AlertItemNum = " + SOut.Long(alertItem.AlertItemNum);
        Db.NonQ(command);
        return true;
    }

    public static void DeleteMany(List<long> listAlertItemNums)
    {
        if (listAlertItemNums == null || listAlertItemNums.Count == 0) return;
        var command = "DELETE FROM alertitem "
                      + "WHERE AlertItemNum IN(" + string.Join(",", listAlertItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<AlertItem> listNew, List<AlertItem> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<AlertItem>();
        var listUpdNew = new List<AlertItem>();
        var listUpdDB = new List<AlertItem>();
        var listDel = new List<AlertItem>();
        listNew.Sort((x, y) => { return x.AlertItemNum.CompareTo(y.AlertItemNum); });
        listDB.Sort((x, y) => { return x.AlertItemNum.CompareTo(y.AlertItemNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        AlertItem fieldNew;
        AlertItem fieldDB;
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

            if (fieldNew.AlertItemNum < fieldDB.AlertItemNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.AlertItemNum > fieldDB.AlertItemNum)
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

        DeleteMany(listDel.Select(x => x.AlertItemNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}