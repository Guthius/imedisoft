#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProviderClinicLinkCrud
{
    public static ProviderClinicLink SelectOne(long providerClinicLinkNum)
    {
        var command = "SELECT * FROM providercliniclink "
                      + "WHERE ProviderClinicLinkNum = " + SOut.Long(providerClinicLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProviderClinicLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProviderClinicLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProviderClinicLink> TableToList(DataTable table)
    {
        var retVal = new List<ProviderClinicLink>();
        ProviderClinicLink providerClinicLink;
        foreach (DataRow row in table.Rows)
        {
            providerClinicLink = new ProviderClinicLink();
            providerClinicLink.ProviderClinicLinkNum = SIn.Long(row["ProviderClinicLinkNum"].ToString());
            providerClinicLink.ProvNum = SIn.Long(row["ProvNum"].ToString());
            providerClinicLink.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            retVal.Add(providerClinicLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProviderClinicLink> listProviderClinicLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProviderClinicLink";
        var table = new DataTable(tableName);
        table.Columns.Add("ProviderClinicLinkNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ClinicNum");
        foreach (var providerClinicLink in listProviderClinicLinks)
            table.Rows.Add(SOut.Long(providerClinicLink.ProviderClinicLinkNum), SOut.Long(providerClinicLink.ProvNum), SOut.Long(providerClinicLink.ClinicNum));
        return table;
    }

    public static long Insert(ProviderClinicLink providerClinicLink)
    {
        return Insert(providerClinicLink, false);
    }

    public static long Insert(ProviderClinicLink providerClinicLink, bool useExistingPK)
    {
        var command = "INSERT INTO providercliniclink (";

        command += "ProvNum,ClinicNum) VALUES(";

        command +=
            SOut.Long(providerClinicLink.ProvNum) + ","
                                                  + SOut.Long(providerClinicLink.ClinicNum) + ")";
        {
            providerClinicLink.ProviderClinicLinkNum = Db.NonQ(command, true, "ProviderClinicLinkNum", "providerClinicLink");
        }
        return providerClinicLink.ProviderClinicLinkNum;
    }

    public static long InsertNoCache(ProviderClinicLink providerClinicLink)
    {
        return InsertNoCache(providerClinicLink, false);
    }

    public static long InsertNoCache(ProviderClinicLink providerClinicLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO providercliniclink (";
        if (isRandomKeys || useExistingPK) command += "ProviderClinicLinkNum,";
        command += "ProvNum,ClinicNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(providerClinicLink.ProviderClinicLinkNum) + ",";
        command +=
            SOut.Long(providerClinicLink.ProvNum) + ","
                                                  + SOut.Long(providerClinicLink.ClinicNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            providerClinicLink.ProviderClinicLinkNum = Db.NonQ(command, true, "ProviderClinicLinkNum", "providerClinicLink");
        return providerClinicLink.ProviderClinicLinkNum;
    }

    public static void Update(ProviderClinicLink providerClinicLink)
    {
        var command = "UPDATE providercliniclink SET "
                      + "ProvNum              =  " + SOut.Long(providerClinicLink.ProvNum) + ", "
                      + "ClinicNum            =  " + SOut.Long(providerClinicLink.ClinicNum) + " "
                      + "WHERE ProviderClinicLinkNum = " + SOut.Long(providerClinicLink.ProviderClinicLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(ProviderClinicLink providerClinicLink, ProviderClinicLink oldProviderClinicLink)
    {
        var command = "";
        if (providerClinicLink.ProvNum != oldProviderClinicLink.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(providerClinicLink.ProvNum) + "";
        }

        if (providerClinicLink.ClinicNum != oldProviderClinicLink.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(providerClinicLink.ClinicNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE providercliniclink SET " + command
                                                   + " WHERE ProviderClinicLinkNum = " + SOut.Long(providerClinicLink.ProviderClinicLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProviderClinicLink providerClinicLink, ProviderClinicLink oldProviderClinicLink)
    {
        if (providerClinicLink.ProvNum != oldProviderClinicLink.ProvNum) return true;
        if (providerClinicLink.ClinicNum != oldProviderClinicLink.ClinicNum) return true;
        return false;
    }

    public static void Delete(long providerClinicLinkNum)
    {
        var command = "DELETE FROM providercliniclink "
                      + "WHERE ProviderClinicLinkNum = " + SOut.Long(providerClinicLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProviderClinicLinkNums)
    {
        if (listProviderClinicLinkNums == null || listProviderClinicLinkNums.Count == 0) return;
        var command = "DELETE FROM providercliniclink "
                      + "WHERE ProviderClinicLinkNum IN(" + string.Join(",", listProviderClinicLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ProviderClinicLink> listNew, List<ProviderClinicLink> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ProviderClinicLink>();
        var listUpdNew = new List<ProviderClinicLink>();
        var listUpdDB = new List<ProviderClinicLink>();
        var listDel = new List<ProviderClinicLink>();
        listNew.Sort((x, y) => { return x.ProviderClinicLinkNum.CompareTo(y.ProviderClinicLinkNum); });
        listDB.Sort((x, y) => { return x.ProviderClinicLinkNum.CompareTo(y.ProviderClinicLinkNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ProviderClinicLink fieldNew;
        ProviderClinicLink fieldDB;
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

            if (fieldNew.ProviderClinicLinkNum < fieldDB.ProviderClinicLinkNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ProviderClinicLinkNum > fieldDB.ProviderClinicLinkNum)
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

        DeleteMany(listDel.Select(x => x.ProviderClinicLinkNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}