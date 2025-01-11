#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReqNeededCrud
{
    public static ReqNeeded SelectOne(long reqNeededNum)
    {
        var command = "SELECT * FROM reqneeded "
                      + "WHERE ReqNeededNum = " + SOut.Long(reqNeededNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ReqNeeded SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ReqNeeded> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ReqNeeded> TableToList(DataTable table)
    {
        var retVal = new List<ReqNeeded>();
        ReqNeeded reqNeeded;
        foreach (DataRow row in table.Rows)
        {
            reqNeeded = new ReqNeeded();
            reqNeeded.ReqNeededNum = SIn.Long(row["ReqNeededNum"].ToString());
            reqNeeded.Descript = SIn.String(row["Descript"].ToString());
            reqNeeded.SchoolCourseNum = SIn.Long(row["SchoolCourseNum"].ToString());
            reqNeeded.SchoolClassNum = SIn.Long(row["SchoolClassNum"].ToString());
            retVal.Add(reqNeeded);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ReqNeeded> listReqNeededs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ReqNeeded";
        var table = new DataTable(tableName);
        table.Columns.Add("ReqNeededNum");
        table.Columns.Add("Descript");
        table.Columns.Add("SchoolCourseNum");
        table.Columns.Add("SchoolClassNum");
        foreach (var reqNeeded in listReqNeededs)
            table.Rows.Add(SOut.Long(reqNeeded.ReqNeededNum), reqNeeded.Descript, SOut.Long(reqNeeded.SchoolCourseNum), SOut.Long(reqNeeded.SchoolClassNum));
        return table;
    }

    public static long Insert(ReqNeeded reqNeeded)
    {
        return Insert(reqNeeded, false);
    }

    public static long Insert(ReqNeeded reqNeeded, bool useExistingPK)
    {
        var command = "INSERT INTO reqneeded (";

        command += "Descript,SchoolCourseNum,SchoolClassNum) VALUES(";

        command +=
            "'" + SOut.String(reqNeeded.Descript) + "',"
            + SOut.Long(reqNeeded.SchoolCourseNum) + ","
            + SOut.Long(reqNeeded.SchoolClassNum) + ")";
        {
            reqNeeded.ReqNeededNum = Db.NonQ(command, true, "ReqNeededNum", "reqNeeded");
        }
        return reqNeeded.ReqNeededNum;
    }

    public static long InsertNoCache(ReqNeeded reqNeeded)
    {
        return InsertNoCache(reqNeeded, false);
    }

    public static long InsertNoCache(ReqNeeded reqNeeded, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO reqneeded (";
        if (isRandomKeys || useExistingPK) command += "ReqNeededNum,";
        command += "Descript,SchoolCourseNum,SchoolClassNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(reqNeeded.ReqNeededNum) + ",";
        command +=
            "'" + SOut.String(reqNeeded.Descript) + "',"
            + SOut.Long(reqNeeded.SchoolCourseNum) + ","
            + SOut.Long(reqNeeded.SchoolClassNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            reqNeeded.ReqNeededNum = Db.NonQ(command, true, "ReqNeededNum", "reqNeeded");
        return reqNeeded.ReqNeededNum;
    }

    public static void Update(ReqNeeded reqNeeded)
    {
        var command = "UPDATE reqneeded SET "
                      + "Descript       = '" + SOut.String(reqNeeded.Descript) + "', "
                      + "SchoolCourseNum=  " + SOut.Long(reqNeeded.SchoolCourseNum) + ", "
                      + "SchoolClassNum =  " + SOut.Long(reqNeeded.SchoolClassNum) + " "
                      + "WHERE ReqNeededNum = " + SOut.Long(reqNeeded.ReqNeededNum);
        Db.NonQ(command);
    }

    public static bool Update(ReqNeeded reqNeeded, ReqNeeded oldReqNeeded)
    {
        var command = "";
        if (reqNeeded.Descript != oldReqNeeded.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(reqNeeded.Descript) + "'";
        }

        if (reqNeeded.SchoolCourseNum != oldReqNeeded.SchoolCourseNum)
        {
            if (command != "") command += ",";
            command += "SchoolCourseNum = " + SOut.Long(reqNeeded.SchoolCourseNum) + "";
        }

        if (reqNeeded.SchoolClassNum != oldReqNeeded.SchoolClassNum)
        {
            if (command != "") command += ",";
            command += "SchoolClassNum = " + SOut.Long(reqNeeded.SchoolClassNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE reqneeded SET " + command
                                          + " WHERE ReqNeededNum = " + SOut.Long(reqNeeded.ReqNeededNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ReqNeeded reqNeeded, ReqNeeded oldReqNeeded)
    {
        if (reqNeeded.Descript != oldReqNeeded.Descript) return true;
        if (reqNeeded.SchoolCourseNum != oldReqNeeded.SchoolCourseNum) return true;
        if (reqNeeded.SchoolClassNum != oldReqNeeded.SchoolClassNum) return true;
        return false;
    }

    public static void Delete(long reqNeededNum)
    {
        var command = "DELETE FROM reqneeded "
                      + "WHERE ReqNeededNum = " + SOut.Long(reqNeededNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReqNeededNums)
    {
        if (listReqNeededNums == null || listReqNeededNums.Count == 0) return;
        var command = "DELETE FROM reqneeded "
                      + "WHERE ReqNeededNum IN(" + string.Join(",", listReqNeededNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ReqNeeded> listNew, List<ReqNeeded> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ReqNeeded>();
        var listUpdNew = new List<ReqNeeded>();
        var listUpdDB = new List<ReqNeeded>();
        var listDel = new List<ReqNeeded>();
        listNew.Sort((x, y) => { return x.ReqNeededNum.CompareTo(y.ReqNeededNum); });
        listDB.Sort((x, y) => { return x.ReqNeededNum.CompareTo(y.ReqNeededNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ReqNeeded fieldNew;
        ReqNeeded fieldDB;
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

            if (fieldNew.ReqNeededNum < fieldDB.ReqNeededNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ReqNeededNum > fieldDB.ReqNeededNum)
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

        DeleteMany(listDel.Select(x => x.ReqNeededNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}