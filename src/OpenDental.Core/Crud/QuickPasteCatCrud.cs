#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class QuickPasteCatCrud
{
    public static QuickPasteCat SelectOne(long quickPasteCatNum)
    {
        var command = "SELECT * FROM quickpastecat "
                      + "WHERE QuickPasteCatNum = " + SOut.Long(quickPasteCatNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static QuickPasteCat SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<QuickPasteCat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<QuickPasteCat> TableToList(DataTable table)
    {
        var retVal = new List<QuickPasteCat>();
        QuickPasteCat quickPasteCat;
        foreach (DataRow row in table.Rows)
        {
            quickPasteCat = new QuickPasteCat();
            quickPasteCat.QuickPasteCatNum = SIn.Long(row["QuickPasteCatNum"].ToString());
            quickPasteCat.Description = SIn.String(row["Description"].ToString());
            quickPasteCat.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            quickPasteCat.DefaultForTypes = SIn.String(row["DefaultForTypes"].ToString());
            retVal.Add(quickPasteCat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<QuickPasteCat> listQuickPasteCats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "QuickPasteCat";
        var table = new DataTable(tableName);
        table.Columns.Add("QuickPasteCatNum");
        table.Columns.Add("Description");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("DefaultForTypes");
        foreach (var quickPasteCat in listQuickPasteCats)
            table.Rows.Add(SOut.Long(quickPasteCat.QuickPasteCatNum), quickPasteCat.Description, SOut.Int(quickPasteCat.ItemOrder), quickPasteCat.DefaultForTypes);
        return table;
    }

    public static long Insert(QuickPasteCat quickPasteCat)
    {
        return Insert(quickPasteCat, false);
    }

    public static long Insert(QuickPasteCat quickPasteCat, bool useExistingPK)
    {
        var command = "INSERT INTO quickpastecat (";

        command += "Description,ItemOrder,DefaultForTypes) VALUES(";

        command +=
            "'" + SOut.String(quickPasteCat.Description) + "',"
            + SOut.Int(quickPasteCat.ItemOrder) + ","
            + DbHelper.ParamChar + "paramDefaultForTypes)";
        if (quickPasteCat.DefaultForTypes == null) quickPasteCat.DefaultForTypes = "";
        var paramDefaultForTypes = new OdSqlParameter("paramDefaultForTypes", OdDbType.Text, SOut.StringParam(quickPasteCat.DefaultForTypes));
        {
            quickPasteCat.QuickPasteCatNum = Db.NonQ(command, true, "QuickPasteCatNum", "quickPasteCat", paramDefaultForTypes);
        }
        return quickPasteCat.QuickPasteCatNum;
    }

    public static long InsertNoCache(QuickPasteCat quickPasteCat)
    {
        return InsertNoCache(quickPasteCat, false);
    }

    public static long InsertNoCache(QuickPasteCat quickPasteCat, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO quickpastecat (";
        if (isRandomKeys || useExistingPK) command += "QuickPasteCatNum,";
        command += "Description,ItemOrder,DefaultForTypes) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(quickPasteCat.QuickPasteCatNum) + ",";
        command +=
            "'" + SOut.String(quickPasteCat.Description) + "',"
            + SOut.Int(quickPasteCat.ItemOrder) + ","
            + DbHelper.ParamChar + "paramDefaultForTypes)";
        if (quickPasteCat.DefaultForTypes == null) quickPasteCat.DefaultForTypes = "";
        var paramDefaultForTypes = new OdSqlParameter("paramDefaultForTypes", OdDbType.Text, SOut.StringParam(quickPasteCat.DefaultForTypes));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDefaultForTypes);
        else
            quickPasteCat.QuickPasteCatNum = Db.NonQ(command, true, "QuickPasteCatNum", "quickPasteCat", paramDefaultForTypes);
        return quickPasteCat.QuickPasteCatNum;
    }

    public static void Update(QuickPasteCat quickPasteCat)
    {
        var command = "UPDATE quickpastecat SET "
                      + "Description     = '" + SOut.String(quickPasteCat.Description) + "', "
                      + "ItemOrder       =  " + SOut.Int(quickPasteCat.ItemOrder) + ", "
                      + "DefaultForTypes =  " + DbHelper.ParamChar + "paramDefaultForTypes "
                      + "WHERE QuickPasteCatNum = " + SOut.Long(quickPasteCat.QuickPasteCatNum);
        if (quickPasteCat.DefaultForTypes == null) quickPasteCat.DefaultForTypes = "";
        var paramDefaultForTypes = new OdSqlParameter("paramDefaultForTypes", OdDbType.Text, SOut.StringParam(quickPasteCat.DefaultForTypes));
        Db.NonQ(command, paramDefaultForTypes);
    }

    public static bool Update(QuickPasteCat quickPasteCat, QuickPasteCat oldQuickPasteCat)
    {
        var command = "";
        if (quickPasteCat.Description != oldQuickPasteCat.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(quickPasteCat.Description) + "'";
        }

        if (quickPasteCat.ItemOrder != oldQuickPasteCat.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(quickPasteCat.ItemOrder) + "";
        }

        if (quickPasteCat.DefaultForTypes != oldQuickPasteCat.DefaultForTypes)
        {
            if (command != "") command += ",";
            command += "DefaultForTypes = " + DbHelper.ParamChar + "paramDefaultForTypes";
        }

        if (command == "") return false;
        if (quickPasteCat.DefaultForTypes == null) quickPasteCat.DefaultForTypes = "";
        var paramDefaultForTypes = new OdSqlParameter("paramDefaultForTypes", OdDbType.Text, SOut.StringParam(quickPasteCat.DefaultForTypes));
        command = "UPDATE quickpastecat SET " + command
                                              + " WHERE QuickPasteCatNum = " + SOut.Long(quickPasteCat.QuickPasteCatNum);
        Db.NonQ(command, paramDefaultForTypes);
        return true;
    }

    public static bool UpdateComparison(QuickPasteCat quickPasteCat, QuickPasteCat oldQuickPasteCat)
    {
        if (quickPasteCat.Description != oldQuickPasteCat.Description) return true;
        if (quickPasteCat.ItemOrder != oldQuickPasteCat.ItemOrder) return true;
        if (quickPasteCat.DefaultForTypes != oldQuickPasteCat.DefaultForTypes) return true;
        return false;
    }

    public static void Delete(long quickPasteCatNum)
    {
        var command = "DELETE FROM quickpastecat "
                      + "WHERE QuickPasteCatNum = " + SOut.Long(quickPasteCatNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listQuickPasteCatNums)
    {
        if (listQuickPasteCatNums == null || listQuickPasteCatNums.Count == 0) return;
        var command = "DELETE FROM quickpastecat "
                      + "WHERE QuickPasteCatNum IN(" + string.Join(",", listQuickPasteCatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<QuickPasteCat> listNew, List<QuickPasteCat> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<QuickPasteCat>();
        var listUpdNew = new List<QuickPasteCat>();
        var listUpdDB = new List<QuickPasteCat>();
        var listDel = new List<QuickPasteCat>();
        listNew.Sort((x, y) => { return x.QuickPasteCatNum.CompareTo(y.QuickPasteCatNum); });
        listDB.Sort((x, y) => { return x.QuickPasteCatNum.CompareTo(y.QuickPasteCatNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        QuickPasteCat fieldNew;
        QuickPasteCat fieldDB;
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

            if (fieldNew.QuickPasteCatNum < fieldDB.QuickPasteCatNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.QuickPasteCatNum > fieldDB.QuickPasteCatNum)
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

        DeleteMany(listDel.Select(x => x.QuickPasteCatNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}