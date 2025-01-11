#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FeeSchedCrud
{
    public static FeeSched SelectOne(long feeSchedNum)
    {
        var command = "SELECT * FROM feesched "
                      + "WHERE FeeSchedNum = " + SOut.Long(feeSchedNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static FeeSched SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FeeSched> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FeeSched> TableToList(DataTable table)
    {
        var retVal = new List<FeeSched>();
        FeeSched feeSched;
        foreach (DataRow row in table.Rows)
        {
            feeSched = new FeeSched();
            feeSched.FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString());
            feeSched.Description = SIn.String(row["Description"].ToString());
            feeSched.FeeSchedType = (FeeScheduleType) SIn.Int(row["FeeSchedType"].ToString());
            feeSched.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            feeSched.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            feeSched.IsGlobal = SIn.Bool(row["IsGlobal"].ToString());
            feeSched.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            feeSched.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            feeSched.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(feeSched);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FeeSched> listFeeScheds, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FeeSched";
        var table = new DataTable(tableName);
        table.Columns.Add("FeeSchedNum");
        table.Columns.Add("Description");
        table.Columns.Add("FeeSchedType");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        table.Columns.Add("IsGlobal");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var feeSched in listFeeScheds)
            table.Rows.Add(SOut.Long(feeSched.FeeSchedNum), feeSched.Description, SOut.Int((int) feeSched.FeeSchedType), SOut.Int(feeSched.ItemOrder), SOut.Bool(feeSched.IsHidden), SOut.Bool(feeSched.IsGlobal), SOut.Long(feeSched.SecUserNumEntry), SOut.DateT(feeSched.SecDateEntry, false), SOut.DateT(feeSched.SecDateTEdit, false));
        return table;
    }

    public static long Insert(FeeSched feeSched)
    {
        return Insert(feeSched, false);
    }

    public static long Insert(FeeSched feeSched, bool useExistingPK)
    {
        var command = "INSERT INTO feesched (";

        command += "Description,FeeSchedType,ItemOrder,IsHidden,IsGlobal,SecUserNumEntry,SecDateEntry) VALUES(";

        command +=
            "'" + SOut.String(feeSched.Description) + "',"
            + SOut.Int((int) feeSched.FeeSchedType) + ","
            + SOut.Int(feeSched.ItemOrder) + ","
            + SOut.Bool(feeSched.IsHidden) + ","
            + SOut.Bool(feeSched.IsGlobal) + ","
            + SOut.Long(feeSched.SecUserNumEntry) + ","
            + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL

        feeSched.FeeSchedNum = Db.NonQ(command, true, "FeeSchedNum", "feeSched");
        return feeSched.FeeSchedNum;
    }

    public static long InsertNoCache(FeeSched feeSched)
    {
        return InsertNoCache(feeSched, false);
    }

    public static long InsertNoCache(FeeSched feeSched, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO feesched (";
        if (isRandomKeys || useExistingPK) command += "FeeSchedNum,";
        command += "Description,FeeSchedType,ItemOrder,IsHidden,IsGlobal,SecUserNumEntry,SecDateEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(feeSched.FeeSchedNum) + ",";
        command +=
            "'" + SOut.String(feeSched.Description) + "',"
            + SOut.Int((int) feeSched.FeeSchedType) + ","
            + SOut.Int(feeSched.ItemOrder) + ","
            + SOut.Bool(feeSched.IsHidden) + ","
            + SOut.Bool(feeSched.IsGlobal) + ","
            + SOut.Long(feeSched.SecUserNumEntry) + ","
            + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            feeSched.FeeSchedNum = Db.NonQ(command, true, "FeeSchedNum", "feeSched");
        return feeSched.FeeSchedNum;
    }

    public static void Update(FeeSched feeSched)
    {
        var command = "UPDATE feesched SET "
                      + "Description    = '" + SOut.String(feeSched.Description) + "', "
                      + "FeeSchedType   =  " + SOut.Int((int) feeSched.FeeSchedType) + ", "
                      + "ItemOrder      =  " + SOut.Int(feeSched.ItemOrder) + ", "
                      + "IsHidden       =  " + SOut.Bool(feeSched.IsHidden) + ", "
                      + "IsGlobal       =  " + SOut.Bool(feeSched.IsGlobal) + " "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE FeeSchedNum = " + SOut.Long(feeSched.FeeSchedNum);
        Db.NonQ(command);
    }

    public static bool Update(FeeSched feeSched, FeeSched oldFeeSched)
    {
        var command = "";
        if (feeSched.Description != oldFeeSched.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(feeSched.Description) + "'";
        }

        if (feeSched.FeeSchedType != oldFeeSched.FeeSchedType)
        {
            if (command != "") command += ",";
            command += "FeeSchedType = " + SOut.Int((int) feeSched.FeeSchedType) + "";
        }

        if (feeSched.ItemOrder != oldFeeSched.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(feeSched.ItemOrder) + "";
        }

        if (feeSched.IsHidden != oldFeeSched.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(feeSched.IsHidden) + "";
        }

        if (feeSched.IsGlobal != oldFeeSched.IsGlobal)
        {
            if (command != "") command += ",";
            command += "IsGlobal = " + SOut.Bool(feeSched.IsGlobal) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE feesched SET " + command
                                         + " WHERE FeeSchedNum = " + SOut.Long(feeSched.FeeSchedNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(FeeSched feeSched, FeeSched oldFeeSched)
    {
        if (feeSched.Description != oldFeeSched.Description) return true;
        if (feeSched.FeeSchedType != oldFeeSched.FeeSchedType) return true;
        if (feeSched.ItemOrder != oldFeeSched.ItemOrder) return true;
        if (feeSched.IsHidden != oldFeeSched.IsHidden) return true;
        if (feeSched.IsGlobal != oldFeeSched.IsGlobal) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long feeSchedNum)
    {
        var command = "DELETE FROM feesched "
                      + "WHERE FeeSchedNum = " + SOut.Long(feeSchedNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFeeSchedNums)
    {
        if (listFeeSchedNums == null || listFeeSchedNums.Count == 0) return;
        var command = "DELETE FROM feesched "
                      + "WHERE FeeSchedNum IN(" + string.Join(",", listFeeSchedNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<FeeSched> listNew, List<FeeSched> listDB, long userNum)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<FeeSched>();
        var listUpdNew = new List<FeeSched>();
        var listUpdDB = new List<FeeSched>();
        var listDel = new List<FeeSched>();
        listNew.Sort((x, y) => { return x.FeeSchedNum.CompareTo(y.FeeSchedNum); });
        listDB.Sort((x, y) => { return x.FeeSchedNum.CompareTo(y.FeeSchedNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        FeeSched fieldNew;
        FeeSched fieldDB;
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

            if (fieldNew.FeeSchedNum < fieldDB.FeeSchedNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.FeeSchedNum > fieldDB.FeeSchedNum)
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
        for (var i = 0; i < listIns.Count; i++)
        {
            listIns[i].SecUserNumEntry = userNum;
            Insert(listIns[i]);
        }

        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.FeeSchedNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}