using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimTrackingCrud
{
    public static List<ClaimTracking> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimTracking> TableToList(DataTable table)
    {
        var retVal = new List<ClaimTracking>();
        ClaimTracking claimTracking;
        foreach (DataRow row in table.Rows)
        {
            claimTracking = new ClaimTracking();
            claimTracking.ClaimTrackingNum = SIn.Long(row["ClaimTrackingNum"].ToString());
            claimTracking.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            var trackingType = row["TrackingType"].ToString();
            if (trackingType == "")
                claimTracking.TrackingType = 0;
            else
                try
                {
                    claimTracking.TrackingType = (ClaimTrackingType) Enum.Parse(typeof(ClaimTrackingType), trackingType);
                }
                catch
                {
                    claimTracking.TrackingType = 0;
                }

            claimTracking.UserNum = SIn.Long(row["UserNum"].ToString());
            claimTracking.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            claimTracking.Note = SIn.String(row["Note"].ToString());
            claimTracking.TrackingDefNum = SIn.Long(row["TrackingDefNum"].ToString());
            claimTracking.TrackingErrorDefNum = SIn.Long(row["TrackingErrorDefNum"].ToString());
            retVal.Add(claimTracking);
        }

        return retVal;
    }

    public static long Insert(ClaimTracking claimTracking)
    {
        var command = "INSERT INTO claimtracking (";

        command += "ClaimNum,TrackingType,UserNum,Note,TrackingDefNum,TrackingErrorDefNum) VALUES(";

        command +=
            SOut.Long(claimTracking.ClaimNum) + ","
                                              + "'" + SOut.String(claimTracking.TrackingType.ToString()) + "',"
                                              + SOut.Long(claimTracking.UserNum) + ","
                                              //DateTimeEntry can only be set by MySQL
                                              + DbHelper.ParamChar + "paramNote,"
                                              + SOut.Long(claimTracking.TrackingDefNum) + ","
                                              + SOut.Long(claimTracking.TrackingErrorDefNum) + ")";
        if (claimTracking.Note == null) claimTracking.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(claimTracking.Note));
        {
            claimTracking.ClaimTrackingNum = Db.NonQ(command, true, "ClaimTrackingNum", "claimTracking", paramNote);
        }
        return claimTracking.ClaimTrackingNum;
    }

    public static void Update(ClaimTracking claimTracking)
    {
        var command = "UPDATE claimtracking SET "
                      + "ClaimNum           =  " + SOut.Long(claimTracking.ClaimNum) + ", "
                      + "TrackingType       = '" + SOut.String(claimTracking.TrackingType.ToString()) + "', "
                      + "UserNum            =  " + SOut.Long(claimTracking.UserNum) + ", "
                      //DateTimeEntry can only be set by MySQL
                      + "Note               =  " + DbHelper.ParamChar + "paramNote, "
                      + "TrackingDefNum     =  " + SOut.Long(claimTracking.TrackingDefNum) + ", "
                      + "TrackingErrorDefNum=  " + SOut.Long(claimTracking.TrackingErrorDefNum) + " "
                      + "WHERE ClaimTrackingNum = " + SOut.Long(claimTracking.ClaimTrackingNum);
        if (claimTracking.Note == null) claimTracking.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(claimTracking.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(ClaimTracking claimTracking, ClaimTracking oldClaimTracking)
    {
        var command = "";
        if (claimTracking.ClaimNum != oldClaimTracking.ClaimNum)
        {
            if (command != "") command += ",";
            command += "ClaimNum = " + SOut.Long(claimTracking.ClaimNum) + "";
        }

        if (claimTracking.TrackingType != oldClaimTracking.TrackingType)
        {
            if (command != "") command += ",";
            command += "TrackingType = '" + SOut.String(claimTracking.TrackingType.ToString()) + "'";
        }

        if (claimTracking.UserNum != oldClaimTracking.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(claimTracking.UserNum) + "";
        }

        //DateTimeEntry can only be set by MySQL
        if (claimTracking.Note != oldClaimTracking.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (claimTracking.TrackingDefNum != oldClaimTracking.TrackingDefNum)
        {
            if (command != "") command += ",";
            command += "TrackingDefNum = " + SOut.Long(claimTracking.TrackingDefNum) + "";
        }

        if (claimTracking.TrackingErrorDefNum != oldClaimTracking.TrackingErrorDefNum)
        {
            if (command != "") command += ",";
            command += "TrackingErrorDefNum = " + SOut.Long(claimTracking.TrackingErrorDefNum) + "";
        }

        if (command == "") return false;
        if (claimTracking.Note == null) claimTracking.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(claimTracking.Note));
        command = "UPDATE claimtracking SET " + command
                                              + " WHERE ClaimTrackingNum = " + SOut.Long(claimTracking.ClaimTrackingNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static void Delete(long claimTrackingNum)
    {
        var command = "DELETE FROM claimtracking "
                      + "WHERE ClaimTrackingNum = " + SOut.Long(claimTrackingNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listClaimTrackingNums)
    {
        if (listClaimTrackingNums == null || listClaimTrackingNums.Count == 0) return;
        var command = "DELETE FROM claimtracking "
                      + "WHERE ClaimTrackingNum IN(" + string.Join(",", listClaimTrackingNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ClaimTracking> listNew, List<ClaimTracking> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ClaimTracking>();
        var listUpdNew = new List<ClaimTracking>();
        var listUpdDB = new List<ClaimTracking>();
        var listDel = new List<ClaimTracking>();
        listNew.Sort((x, y) => { return x.ClaimTrackingNum.CompareTo(y.ClaimTrackingNum); });
        listDB.Sort((x, y) => { return x.ClaimTrackingNum.CompareTo(y.ClaimTrackingNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ClaimTracking fieldNew;
        ClaimTracking fieldDB;
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

            if (fieldNew.ClaimTrackingNum < fieldDB.ClaimTrackingNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ClaimTrackingNum > fieldDB.ClaimTrackingNum)
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

        DeleteMany(listDel.Select(x => x.ClaimTrackingNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}