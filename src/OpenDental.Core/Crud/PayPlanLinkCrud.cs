#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PayPlanLinkCrud
{
    public static PayPlanLink SelectOne(long payPlanLinkNum)
    {
        var command = "SELECT * FROM payplanlink "
                      + "WHERE PayPlanLinkNum = " + SOut.Long(payPlanLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PayPlanLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayPlanLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayPlanLink> TableToList(DataTable table)
    {
        var retVal = new List<PayPlanLink>();
        PayPlanLink payPlanLink;
        foreach (DataRow row in table.Rows)
        {
            payPlanLink = new PayPlanLink();
            payPlanLink.PayPlanLinkNum = SIn.Long(row["PayPlanLinkNum"].ToString());
            payPlanLink.PayPlanNum = SIn.Long(row["PayPlanNum"].ToString());
            payPlanLink.LinkType = (PayPlanLinkType) SIn.Int(row["LinkType"].ToString());
            payPlanLink.FKey = SIn.Long(row["FKey"].ToString());
            payPlanLink.AmountOverride = SIn.Double(row["AmountOverride"].ToString());
            payPlanLink.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            retVal.Add(payPlanLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PayPlanLink> listPayPlanLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayPlanLink";
        var table = new DataTable(tableName);
        table.Columns.Add("PayPlanLinkNum");
        table.Columns.Add("PayPlanNum");
        table.Columns.Add("LinkType");
        table.Columns.Add("FKey");
        table.Columns.Add("AmountOverride");
        table.Columns.Add("SecDateTEntry");
        foreach (var payPlanLink in listPayPlanLinks)
            table.Rows.Add(SOut.Long(payPlanLink.PayPlanLinkNum), SOut.Long(payPlanLink.PayPlanNum), SOut.Int((int) payPlanLink.LinkType), SOut.Long(payPlanLink.FKey), SOut.Double(payPlanLink.AmountOverride), SOut.DateT(payPlanLink.SecDateTEntry, false));
        return table;
    }

    public static long Insert(PayPlanLink payPlanLink)
    {
        return Insert(payPlanLink, false);
    }

    public static long Insert(PayPlanLink payPlanLink, bool useExistingPK)
    {
        var command = "INSERT INTO payplanlink (";

        command += "PayPlanNum,LinkType,FKey,AmountOverride,SecDateTEntry) VALUES(";

        command +=
            SOut.Long(payPlanLink.PayPlanNum) + ","
                                              + SOut.Int((int) payPlanLink.LinkType) + ","
                                              + SOut.Long(payPlanLink.FKey) + ","
                                              + SOut.Double(payPlanLink.AmountOverride) + ","
                                              + DbHelper.Now() + ")";
        {
            payPlanLink.PayPlanLinkNum = Db.NonQ(command, true, "PayPlanLinkNum", "payPlanLink");
        }
        return payPlanLink.PayPlanLinkNum;
    }

    public static long InsertNoCache(PayPlanLink payPlanLink)
    {
        return InsertNoCache(payPlanLink, false);
    }

    public static long InsertNoCache(PayPlanLink payPlanLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payplanlink (";
        if (isRandomKeys || useExistingPK) command += "PayPlanLinkNum,";
        command += "PayPlanNum,LinkType,FKey,AmountOverride,SecDateTEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payPlanLink.PayPlanLinkNum) + ",";
        command +=
            SOut.Long(payPlanLink.PayPlanNum) + ","
                                              + SOut.Int((int) payPlanLink.LinkType) + ","
                                              + SOut.Long(payPlanLink.FKey) + ","
                                              + SOut.Double(payPlanLink.AmountOverride) + ","
                                              + DbHelper.Now() + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            payPlanLink.PayPlanLinkNum = Db.NonQ(command, true, "PayPlanLinkNum", "payPlanLink");
        return payPlanLink.PayPlanLinkNum;
    }

    public static void Update(PayPlanLink payPlanLink)
    {
        var command = "UPDATE payplanlink SET "
                      + "PayPlanNum    =  " + SOut.Long(payPlanLink.PayPlanNum) + ", "
                      + "LinkType      =  " + SOut.Int((int) payPlanLink.LinkType) + ", "
                      + "FKey          =  " + SOut.Long(payPlanLink.FKey) + ", "
                      + "AmountOverride=  " + SOut.Double(payPlanLink.AmountOverride) + " "
                      //SecDateTEntry not allowed to change
                      + "WHERE PayPlanLinkNum = " + SOut.Long(payPlanLink.PayPlanLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(PayPlanLink payPlanLink, PayPlanLink oldPayPlanLink)
    {
        var command = "";
        if (payPlanLink.PayPlanNum != oldPayPlanLink.PayPlanNum)
        {
            if (command != "") command += ",";
            command += "PayPlanNum = " + SOut.Long(payPlanLink.PayPlanNum) + "";
        }

        if (payPlanLink.LinkType != oldPayPlanLink.LinkType)
        {
            if (command != "") command += ",";
            command += "LinkType = " + SOut.Int((int) payPlanLink.LinkType) + "";
        }

        if (payPlanLink.FKey != oldPayPlanLink.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(payPlanLink.FKey) + "";
        }

        if (payPlanLink.AmountOverride != oldPayPlanLink.AmountOverride)
        {
            if (command != "") command += ",";
            command += "AmountOverride = " + SOut.Double(payPlanLink.AmountOverride) + "";
        }

        //SecDateTEntry not allowed to change
        if (command == "") return false;
        command = "UPDATE payplanlink SET " + command
                                            + " WHERE PayPlanLinkNum = " + SOut.Long(payPlanLink.PayPlanLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PayPlanLink payPlanLink, PayPlanLink oldPayPlanLink)
    {
        if (payPlanLink.PayPlanNum != oldPayPlanLink.PayPlanNum) return true;
        if (payPlanLink.LinkType != oldPayPlanLink.LinkType) return true;
        if (payPlanLink.FKey != oldPayPlanLink.FKey) return true;
        if (payPlanLink.AmountOverride != oldPayPlanLink.AmountOverride) return true;
        //SecDateTEntry not allowed to change
        return false;
    }

    public static void Delete(long payPlanLinkNum)
    {
        var command = "DELETE FROM payplanlink "
                      + "WHERE PayPlanLinkNum = " + SOut.Long(payPlanLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayPlanLinkNums)
    {
        if (listPayPlanLinkNums == null || listPayPlanLinkNums.Count == 0) return;
        var command = "DELETE FROM payplanlink "
                      + "WHERE PayPlanLinkNum IN(" + string.Join(",", listPayPlanLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<PayPlanLink> listNew, List<PayPlanLink> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<PayPlanLink>();
        var listUpdNew = new List<PayPlanLink>();
        var listUpdDB = new List<PayPlanLink>();
        var listDel = new List<PayPlanLink>();
        listNew.Sort((x, y) => { return x.PayPlanLinkNum.CompareTo(y.PayPlanLinkNum); });
        listDB.Sort((x, y) => { return x.PayPlanLinkNum.CompareTo(y.PayPlanLinkNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        PayPlanLink fieldNew;
        PayPlanLink fieldDB;
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

            if (fieldNew.PayPlanLinkNum < fieldDB.PayPlanLinkNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.PayPlanLinkNum > fieldDB.PayPlanLinkNum)
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

        DeleteMany(listDel.Select(x => x.PayPlanLinkNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}