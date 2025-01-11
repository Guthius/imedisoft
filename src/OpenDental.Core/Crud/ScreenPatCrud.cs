#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ScreenPatCrud
{
    public static ScreenPat SelectOne(long screenPatNum)
    {
        var command = "SELECT * FROM screenpat "
                      + "WHERE ScreenPatNum = " + SOut.Long(screenPatNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ScreenPat SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ScreenPat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ScreenPat> TableToList(DataTable table)
    {
        var retVal = new List<ScreenPat>();
        ScreenPat screenPat;
        foreach (DataRow row in table.Rows)
        {
            screenPat = new ScreenPat();
            screenPat.ScreenPatNum = SIn.Long(row["ScreenPatNum"].ToString());
            screenPat.PatNum = SIn.Long(row["PatNum"].ToString());
            screenPat.ScreenGroupNum = SIn.Long(row["ScreenGroupNum"].ToString());
            screenPat.SheetNum = SIn.Long(row["SheetNum"].ToString());
            screenPat.PatScreenPerm = (PatScreenPerm) SIn.Int(row["PatScreenPerm"].ToString());
            retVal.Add(screenPat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ScreenPat> listScreenPats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ScreenPat";
        var table = new DataTable(tableName);
        table.Columns.Add("ScreenPatNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ScreenGroupNum");
        table.Columns.Add("SheetNum");
        table.Columns.Add("PatScreenPerm");
        foreach (var screenPat in listScreenPats)
            table.Rows.Add(SOut.Long(screenPat.ScreenPatNum), SOut.Long(screenPat.PatNum), SOut.Long(screenPat.ScreenGroupNum), SOut.Long(screenPat.SheetNum), SOut.Int((int) screenPat.PatScreenPerm));
        return table;
    }

    public static long Insert(ScreenPat screenPat)
    {
        return Insert(screenPat, false);
    }

    public static long Insert(ScreenPat screenPat, bool useExistingPK)
    {
        var command = "INSERT INTO screenpat (";

        command += "PatNum,ScreenGroupNum,SheetNum,PatScreenPerm) VALUES(";

        command +=
            SOut.Long(screenPat.PatNum) + ","
                                        + SOut.Long(screenPat.ScreenGroupNum) + ","
                                        + SOut.Long(screenPat.SheetNum) + ","
                                        + SOut.Int((int) screenPat.PatScreenPerm) + ")";
        {
            screenPat.ScreenPatNum = Db.NonQ(command, true, "ScreenPatNum", "screenPat");
        }
        return screenPat.ScreenPatNum;
    }

    public static long InsertNoCache(ScreenPat screenPat)
    {
        return InsertNoCache(screenPat, false);
    }

    public static long InsertNoCache(ScreenPat screenPat, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO screenpat (";
        if (isRandomKeys || useExistingPK) command += "ScreenPatNum,";
        command += "PatNum,ScreenGroupNum,SheetNum,PatScreenPerm) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(screenPat.ScreenPatNum) + ",";
        command +=
            SOut.Long(screenPat.PatNum) + ","
                                        + SOut.Long(screenPat.ScreenGroupNum) + ","
                                        + SOut.Long(screenPat.SheetNum) + ","
                                        + SOut.Int((int) screenPat.PatScreenPerm) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            screenPat.ScreenPatNum = Db.NonQ(command, true, "ScreenPatNum", "screenPat");
        return screenPat.ScreenPatNum;
    }

    public static void Update(ScreenPat screenPat)
    {
        var command = "UPDATE screenpat SET "
                      + "PatNum        =  " + SOut.Long(screenPat.PatNum) + ", "
                      + "ScreenGroupNum=  " + SOut.Long(screenPat.ScreenGroupNum) + ", "
                      + "SheetNum      =  " + SOut.Long(screenPat.SheetNum) + ", "
                      + "PatScreenPerm =  " + SOut.Int((int) screenPat.PatScreenPerm) + " "
                      + "WHERE ScreenPatNum = " + SOut.Long(screenPat.ScreenPatNum);
        Db.NonQ(command);
    }

    public static bool Update(ScreenPat screenPat, ScreenPat oldScreenPat)
    {
        var command = "";
        if (screenPat.PatNum != oldScreenPat.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(screenPat.PatNum) + "";
        }

        if (screenPat.ScreenGroupNum != oldScreenPat.ScreenGroupNum)
        {
            if (command != "") command += ",";
            command += "ScreenGroupNum = " + SOut.Long(screenPat.ScreenGroupNum) + "";
        }

        if (screenPat.SheetNum != oldScreenPat.SheetNum)
        {
            if (command != "") command += ",";
            command += "SheetNum = " + SOut.Long(screenPat.SheetNum) + "";
        }

        if (screenPat.PatScreenPerm != oldScreenPat.PatScreenPerm)
        {
            if (command != "") command += ",";
            command += "PatScreenPerm = " + SOut.Int((int) screenPat.PatScreenPerm) + "";
        }

        if (command == "") return false;
        command = "UPDATE screenpat SET " + command
                                          + " WHERE ScreenPatNum = " + SOut.Long(screenPat.ScreenPatNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ScreenPat screenPat, ScreenPat oldScreenPat)
    {
        if (screenPat.PatNum != oldScreenPat.PatNum) return true;
        if (screenPat.ScreenGroupNum != oldScreenPat.ScreenGroupNum) return true;
        if (screenPat.SheetNum != oldScreenPat.SheetNum) return true;
        if (screenPat.PatScreenPerm != oldScreenPat.PatScreenPerm) return true;
        return false;
    }

    public static void Delete(long screenPatNum)
    {
        var command = "DELETE FROM screenpat "
                      + "WHERE ScreenPatNum = " + SOut.Long(screenPatNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listScreenPatNums)
    {
        if (listScreenPatNums == null || listScreenPatNums.Count == 0) return;
        var command = "DELETE FROM screenpat "
                      + "WHERE ScreenPatNum IN(" + string.Join(",", listScreenPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ScreenPat> listNew, List<ScreenPat> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ScreenPat>();
        var listUpdNew = new List<ScreenPat>();
        var listUpdDB = new List<ScreenPat>();
        var listDel = new List<ScreenPat>();
        listNew.Sort((x, y) => { return x.ScreenPatNum.CompareTo(y.ScreenPatNum); });
        listDB.Sort((x, y) => { return x.ScreenPatNum.CompareTo(y.ScreenPatNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ScreenPat fieldNew;
        ScreenPat fieldDB;
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

            if (fieldNew.ScreenPatNum < fieldDB.ScreenPatNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ScreenPatNum > fieldDB.ScreenPatNum)
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

        DeleteMany(listDel.Select(x => x.ScreenPatNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}