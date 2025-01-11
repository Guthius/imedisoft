#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TreatPlanAttachCrud
{
    public static TreatPlanAttach SelectOne(long treatPlanAttachNum)
    {
        var command = "SELECT * FROM treatplanattach "
                      + "WHERE TreatPlanAttachNum = " + SOut.Long(treatPlanAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TreatPlanAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TreatPlanAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TreatPlanAttach> TableToList(DataTable table)
    {
        var retVal = new List<TreatPlanAttach>();
        TreatPlanAttach treatPlanAttach;
        foreach (DataRow row in table.Rows)
        {
            treatPlanAttach = new TreatPlanAttach();
            treatPlanAttach.TreatPlanAttachNum = SIn.Long(row["TreatPlanAttachNum"].ToString());
            treatPlanAttach.TreatPlanNum = SIn.Long(row["TreatPlanNum"].ToString());
            treatPlanAttach.ProcNum = SIn.Long(row["ProcNum"].ToString());
            treatPlanAttach.Priority = SIn.Long(row["Priority"].ToString());
            retVal.Add(treatPlanAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TreatPlanAttach> listTreatPlanAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TreatPlanAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("TreatPlanAttachNum");
        table.Columns.Add("TreatPlanNum");
        table.Columns.Add("ProcNum");
        table.Columns.Add("Priority");
        foreach (var treatPlanAttach in listTreatPlanAttachs)
            table.Rows.Add(SOut.Long(treatPlanAttach.TreatPlanAttachNum), SOut.Long(treatPlanAttach.TreatPlanNum), SOut.Long(treatPlanAttach.ProcNum), SOut.Long(treatPlanAttach.Priority));
        return table;
    }

    public static long Insert(TreatPlanAttach treatPlanAttach)
    {
        return Insert(treatPlanAttach, false);
    }

    public static long Insert(TreatPlanAttach treatPlanAttach, bool useExistingPK)
    {
        var command = "INSERT INTO treatplanattach (";

        command += "TreatPlanNum,ProcNum,Priority) VALUES(";

        command +=
            SOut.Long(treatPlanAttach.TreatPlanNum) + ","
                                                    + SOut.Long(treatPlanAttach.ProcNum) + ","
                                                    + SOut.Long(treatPlanAttach.Priority) + ")";
        {
            treatPlanAttach.TreatPlanAttachNum = Db.NonQ(command, true, "TreatPlanAttachNum", "treatPlanAttach");
        }
        return treatPlanAttach.TreatPlanAttachNum;
    }

    public static long InsertNoCache(TreatPlanAttach treatPlanAttach)
    {
        return InsertNoCache(treatPlanAttach, false);
    }

    public static long InsertNoCache(TreatPlanAttach treatPlanAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO treatplanattach (";
        if (isRandomKeys || useExistingPK) command += "TreatPlanAttachNum,";
        command += "TreatPlanNum,ProcNum,Priority) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(treatPlanAttach.TreatPlanAttachNum) + ",";
        command +=
            SOut.Long(treatPlanAttach.TreatPlanNum) + ","
                                                    + SOut.Long(treatPlanAttach.ProcNum) + ","
                                                    + SOut.Long(treatPlanAttach.Priority) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            treatPlanAttach.TreatPlanAttachNum = Db.NonQ(command, true, "TreatPlanAttachNum", "treatPlanAttach");
        return treatPlanAttach.TreatPlanAttachNum;
    }

    public static void Update(TreatPlanAttach treatPlanAttach)
    {
        var command = "UPDATE treatplanattach SET "
                      + "TreatPlanNum      =  " + SOut.Long(treatPlanAttach.TreatPlanNum) + ", "
                      + "ProcNum           =  " + SOut.Long(treatPlanAttach.ProcNum) + ", "
                      + "Priority          =  " + SOut.Long(treatPlanAttach.Priority) + " "
                      + "WHERE TreatPlanAttachNum = " + SOut.Long(treatPlanAttach.TreatPlanAttachNum);
        Db.NonQ(command);
    }

    public static bool Update(TreatPlanAttach treatPlanAttach, TreatPlanAttach oldTreatPlanAttach)
    {
        var command = "";
        if (treatPlanAttach.TreatPlanNum != oldTreatPlanAttach.TreatPlanNum)
        {
            if (command != "") command += ",";
            command += "TreatPlanNum = " + SOut.Long(treatPlanAttach.TreatPlanNum) + "";
        }

        if (treatPlanAttach.ProcNum != oldTreatPlanAttach.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(treatPlanAttach.ProcNum) + "";
        }

        if (treatPlanAttach.Priority != oldTreatPlanAttach.Priority)
        {
            if (command != "") command += ",";
            command += "Priority = " + SOut.Long(treatPlanAttach.Priority) + "";
        }

        if (command == "") return false;
        command = "UPDATE treatplanattach SET " + command
                                                + " WHERE TreatPlanAttachNum = " + SOut.Long(treatPlanAttach.TreatPlanAttachNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TreatPlanAttach treatPlanAttach, TreatPlanAttach oldTreatPlanAttach)
    {
        if (treatPlanAttach.TreatPlanNum != oldTreatPlanAttach.TreatPlanNum) return true;
        if (treatPlanAttach.ProcNum != oldTreatPlanAttach.ProcNum) return true;
        if (treatPlanAttach.Priority != oldTreatPlanAttach.Priority) return true;
        return false;
    }

    public static void Delete(long treatPlanAttachNum)
    {
        var command = "DELETE FROM treatplanattach "
                      + "WHERE TreatPlanAttachNum = " + SOut.Long(treatPlanAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTreatPlanAttachNums)
    {
        if (listTreatPlanAttachNums == null || listTreatPlanAttachNums.Count == 0) return;
        var command = "DELETE FROM treatplanattach "
                      + "WHERE TreatPlanAttachNum IN(" + string.Join(",", listTreatPlanAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<TreatPlanAttach> listNew, List<TreatPlanAttach> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<TreatPlanAttach>();
        var listUpdNew = new List<TreatPlanAttach>();
        var listUpdDB = new List<TreatPlanAttach>();
        var listDel = new List<TreatPlanAttach>();
        listNew.Sort((x, y) => { return x.TreatPlanAttachNum.CompareTo(y.TreatPlanAttachNum); });
        listDB.Sort((x, y) => { return x.TreatPlanAttachNum.CompareTo(y.TreatPlanAttachNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        TreatPlanAttach fieldNew;
        TreatPlanAttach fieldDB;
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

            if (fieldNew.TreatPlanAttachNum < fieldDB.TreatPlanAttachNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.TreatPlanAttachNum > fieldDB.TreatPlanAttachNum)
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

        DeleteMany(listDel.Select(x => x.TreatPlanAttachNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}