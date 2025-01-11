#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsBlueBookCrud
{
    public static InsBlueBook SelectOne(long insBlueBookNum)
    {
        var command = "SELECT * FROM insbluebook "
                      + "WHERE InsBlueBookNum = " + SOut.Long(insBlueBookNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsBlueBook SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsBlueBook> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsBlueBook> TableToList(DataTable table)
    {
        var retVal = new List<InsBlueBook>();
        InsBlueBook insBlueBook;
        foreach (DataRow row in table.Rows)
        {
            insBlueBook = new InsBlueBook();
            insBlueBook.InsBlueBookNum = SIn.Long(row["InsBlueBookNum"].ToString());
            insBlueBook.ProcCodeNum = SIn.Long(row["ProcCodeNum"].ToString());
            insBlueBook.CarrierNum = SIn.Long(row["CarrierNum"].ToString());
            insBlueBook.PlanNum = SIn.Long(row["PlanNum"].ToString());
            insBlueBook.GroupNum = SIn.String(row["GroupNum"].ToString());
            insBlueBook.InsPayAmt = SIn.Double(row["InsPayAmt"].ToString());
            insBlueBook.AllowedOverride = SIn.Double(row["AllowedOverride"].ToString());
            insBlueBook.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            insBlueBook.ProcNum = SIn.Long(row["ProcNum"].ToString());
            insBlueBook.ProcDate = SIn.Date(row["ProcDate"].ToString());
            insBlueBook.ClaimType = SIn.String(row["ClaimType"].ToString());
            insBlueBook.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            retVal.Add(insBlueBook);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsBlueBook> listInsBlueBooks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsBlueBook";
        var table = new DataTable(tableName);
        table.Columns.Add("InsBlueBookNum");
        table.Columns.Add("ProcCodeNum");
        table.Columns.Add("CarrierNum");
        table.Columns.Add("PlanNum");
        table.Columns.Add("GroupNum");
        table.Columns.Add("InsPayAmt");
        table.Columns.Add("AllowedOverride");
        table.Columns.Add("DateTEntry");
        table.Columns.Add("ProcNum");
        table.Columns.Add("ProcDate");
        table.Columns.Add("ClaimType");
        table.Columns.Add("ClaimNum");
        foreach (var insBlueBook in listInsBlueBooks)
            table.Rows.Add(SOut.Long(insBlueBook.InsBlueBookNum), SOut.Long(insBlueBook.ProcCodeNum), SOut.Long(insBlueBook.CarrierNum), SOut.Long(insBlueBook.PlanNum), insBlueBook.GroupNum, SOut.Double(insBlueBook.InsPayAmt), SOut.Double(insBlueBook.AllowedOverride), SOut.DateT(insBlueBook.DateTEntry, false), SOut.Long(insBlueBook.ProcNum), SOut.DateT(insBlueBook.ProcDate, false), insBlueBook.ClaimType, SOut.Long(insBlueBook.ClaimNum));
        return table;
    }

    public static long Insert(InsBlueBook insBlueBook)
    {
        return Insert(insBlueBook, false);
    }

    public static long Insert(InsBlueBook insBlueBook, bool useExistingPK)
    {
        var command = "INSERT INTO insbluebook (";

        command += "ProcCodeNum,CarrierNum,PlanNum,GroupNum,InsPayAmt,AllowedOverride,DateTEntry,ProcNum,ProcDate,ClaimType,ClaimNum) VALUES(";

        command +=
            SOut.Long(insBlueBook.ProcCodeNum) + ","
                                               + SOut.Long(insBlueBook.CarrierNum) + ","
                                               + SOut.Long(insBlueBook.PlanNum) + ","
                                               + "'" + SOut.String(insBlueBook.GroupNum) + "',"
                                               + SOut.Double(insBlueBook.InsPayAmt) + ","
                                               + SOut.Double(insBlueBook.AllowedOverride) + ","
                                               + DbHelper.Now() + ","
                                               + SOut.Long(insBlueBook.ProcNum) + ","
                                               + SOut.Date(insBlueBook.ProcDate) + ","
                                               + "'" + SOut.String(insBlueBook.ClaimType) + "',"
                                               + SOut.Long(insBlueBook.ClaimNum) + ")";
        {
            insBlueBook.InsBlueBookNum = Db.NonQ(command, true, "InsBlueBookNum", "insBlueBook");
        }
        return insBlueBook.InsBlueBookNum;
    }

    public static long InsertNoCache(InsBlueBook insBlueBook)
    {
        return InsertNoCache(insBlueBook, false);
    }

    public static long InsertNoCache(InsBlueBook insBlueBook, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insbluebook (";
        if (isRandomKeys || useExistingPK) command += "InsBlueBookNum,";
        command += "ProcCodeNum,CarrierNum,PlanNum,GroupNum,InsPayAmt,AllowedOverride,DateTEntry,ProcNum,ProcDate,ClaimType,ClaimNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insBlueBook.InsBlueBookNum) + ",";
        command +=
            SOut.Long(insBlueBook.ProcCodeNum) + ","
                                               + SOut.Long(insBlueBook.CarrierNum) + ","
                                               + SOut.Long(insBlueBook.PlanNum) + ","
                                               + "'" + SOut.String(insBlueBook.GroupNum) + "',"
                                               + SOut.Double(insBlueBook.InsPayAmt) + ","
                                               + SOut.Double(insBlueBook.AllowedOverride) + ","
                                               + DbHelper.Now() + ","
                                               + SOut.Long(insBlueBook.ProcNum) + ","
                                               + SOut.Date(insBlueBook.ProcDate) + ","
                                               + "'" + SOut.String(insBlueBook.ClaimType) + "',"
                                               + SOut.Long(insBlueBook.ClaimNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            insBlueBook.InsBlueBookNum = Db.NonQ(command, true, "InsBlueBookNum", "insBlueBook");
        return insBlueBook.InsBlueBookNum;
    }

    public static void Update(InsBlueBook insBlueBook)
    {
        var command = "UPDATE insbluebook SET "
                      + "ProcCodeNum    =  " + SOut.Long(insBlueBook.ProcCodeNum) + ", "
                      + "CarrierNum     =  " + SOut.Long(insBlueBook.CarrierNum) + ", "
                      + "PlanNum        =  " + SOut.Long(insBlueBook.PlanNum) + ", "
                      + "GroupNum       = '" + SOut.String(insBlueBook.GroupNum) + "', "
                      + "InsPayAmt      =  " + SOut.Double(insBlueBook.InsPayAmt) + ", "
                      + "AllowedOverride=  " + SOut.Double(insBlueBook.AllowedOverride) + ", "
                      //DateTEntry not allowed to change
                      + "ProcNum        =  " + SOut.Long(insBlueBook.ProcNum) + ", "
                      + "ProcDate       =  " + SOut.Date(insBlueBook.ProcDate) + ", "
                      + "ClaimType      = '" + SOut.String(insBlueBook.ClaimType) + "', "
                      + "ClaimNum       =  " + SOut.Long(insBlueBook.ClaimNum) + " "
                      + "WHERE InsBlueBookNum = " + SOut.Long(insBlueBook.InsBlueBookNum);
        Db.NonQ(command);
    }

    public static bool Update(InsBlueBook insBlueBook, InsBlueBook oldInsBlueBook)
    {
        var command = "";
        if (insBlueBook.ProcCodeNum != oldInsBlueBook.ProcCodeNum)
        {
            if (command != "") command += ",";
            command += "ProcCodeNum = " + SOut.Long(insBlueBook.ProcCodeNum) + "";
        }

        if (insBlueBook.CarrierNum != oldInsBlueBook.CarrierNum)
        {
            if (command != "") command += ",";
            command += "CarrierNum = " + SOut.Long(insBlueBook.CarrierNum) + "";
        }

        if (insBlueBook.PlanNum != oldInsBlueBook.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(insBlueBook.PlanNum) + "";
        }

        if (insBlueBook.GroupNum != oldInsBlueBook.GroupNum)
        {
            if (command != "") command += ",";
            command += "GroupNum = '" + SOut.String(insBlueBook.GroupNum) + "'";
        }

        if (insBlueBook.InsPayAmt != oldInsBlueBook.InsPayAmt)
        {
            if (command != "") command += ",";
            command += "InsPayAmt = " + SOut.Double(insBlueBook.InsPayAmt) + "";
        }

        if (insBlueBook.AllowedOverride != oldInsBlueBook.AllowedOverride)
        {
            if (command != "") command += ",";
            command += "AllowedOverride = " + SOut.Double(insBlueBook.AllowedOverride) + "";
        }

        //DateTEntry not allowed to change
        if (insBlueBook.ProcNum != oldInsBlueBook.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(insBlueBook.ProcNum) + "";
        }

        if (insBlueBook.ProcDate.Date != oldInsBlueBook.ProcDate.Date)
        {
            if (command != "") command += ",";
            command += "ProcDate = " + SOut.Date(insBlueBook.ProcDate) + "";
        }

        if (insBlueBook.ClaimType != oldInsBlueBook.ClaimType)
        {
            if (command != "") command += ",";
            command += "ClaimType = '" + SOut.String(insBlueBook.ClaimType) + "'";
        }

        if (insBlueBook.ClaimNum != oldInsBlueBook.ClaimNum)
        {
            if (command != "") command += ",";
            command += "ClaimNum = " + SOut.Long(insBlueBook.ClaimNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE insbluebook SET " + command
                                            + " WHERE InsBlueBookNum = " + SOut.Long(insBlueBook.InsBlueBookNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(InsBlueBook insBlueBook, InsBlueBook oldInsBlueBook)
    {
        if (insBlueBook.ProcCodeNum != oldInsBlueBook.ProcCodeNum) return true;
        if (insBlueBook.CarrierNum != oldInsBlueBook.CarrierNum) return true;
        if (insBlueBook.PlanNum != oldInsBlueBook.PlanNum) return true;
        if (insBlueBook.GroupNum != oldInsBlueBook.GroupNum) return true;
        if (insBlueBook.InsPayAmt != oldInsBlueBook.InsPayAmt) return true;
        if (insBlueBook.AllowedOverride != oldInsBlueBook.AllowedOverride) return true;
        //DateTEntry not allowed to change
        if (insBlueBook.ProcNum != oldInsBlueBook.ProcNum) return true;
        if (insBlueBook.ProcDate.Date != oldInsBlueBook.ProcDate.Date) return true;
        if (insBlueBook.ClaimType != oldInsBlueBook.ClaimType) return true;
        if (insBlueBook.ClaimNum != oldInsBlueBook.ClaimNum) return true;
        return false;
    }

    public static void Delete(long insBlueBookNum)
    {
        var command = "DELETE FROM insbluebook "
                      + "WHERE InsBlueBookNum = " + SOut.Long(insBlueBookNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsBlueBookNums)
    {
        if (listInsBlueBookNums == null || listInsBlueBookNums.Count == 0) return;
        var command = "DELETE FROM insbluebook "
                      + "WHERE InsBlueBookNum IN(" + string.Join(",", listInsBlueBookNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<InsBlueBook> listNew, List<InsBlueBook> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<InsBlueBook>();
        var listUpdNew = new List<InsBlueBook>();
        var listUpdDB = new List<InsBlueBook>();
        var listDel = new List<InsBlueBook>();
        listNew.Sort((x, y) => { return x.InsBlueBookNum.CompareTo(y.InsBlueBookNum); });
        listDB.Sort((x, y) => { return x.InsBlueBookNum.CompareTo(y.InsBlueBookNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        InsBlueBook fieldNew;
        InsBlueBook fieldDB;
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

            if (fieldNew.InsBlueBookNum < fieldDB.InsBlueBookNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.InsBlueBookNum > fieldDB.InsBlueBookNum)
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

        DeleteMany(listDel.Select(x => x.InsBlueBookNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}