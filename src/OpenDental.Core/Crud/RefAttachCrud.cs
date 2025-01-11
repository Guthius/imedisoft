#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RefAttachCrud
{
    public static RefAttach SelectOne(long refAttachNum)
    {
        var command = "SELECT * FROM refattach "
                      + "WHERE RefAttachNum = " + SOut.Long(refAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RefAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RefAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RefAttach> TableToList(DataTable table)
    {
        var retVal = new List<RefAttach>();
        RefAttach refAttach;
        foreach (DataRow row in table.Rows)
        {
            refAttach = new RefAttach();
            refAttach.RefAttachNum = SIn.Long(row["RefAttachNum"].ToString());
            refAttach.ReferralNum = SIn.Long(row["ReferralNum"].ToString());
            refAttach.PatNum = SIn.Long(row["PatNum"].ToString());
            refAttach.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            refAttach.RefDate = SIn.Date(row["RefDate"].ToString());
            refAttach.RefType = (ReferralType) SIn.Int(row["RefType"].ToString());
            refAttach.RefToStatus = (ReferralToStatus) SIn.Int(row["RefToStatus"].ToString());
            refAttach.Note = SIn.String(row["Note"].ToString());
            refAttach.IsTransitionOfCare = SIn.Bool(row["IsTransitionOfCare"].ToString());
            refAttach.ProcNum = SIn.Long(row["ProcNum"].ToString());
            refAttach.DateProcComplete = SIn.Date(row["DateProcComplete"].ToString());
            refAttach.ProvNum = SIn.Long(row["ProvNum"].ToString());
            refAttach.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            retVal.Add(refAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RefAttach> listRefAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RefAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("RefAttachNum");
        table.Columns.Add("ReferralNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("RefDate");
        table.Columns.Add("RefType");
        table.Columns.Add("RefToStatus");
        table.Columns.Add("Note");
        table.Columns.Add("IsTransitionOfCare");
        table.Columns.Add("ProcNum");
        table.Columns.Add("DateProcComplete");
        table.Columns.Add("ProvNum");
        table.Columns.Add("DateTStamp");
        foreach (var refAttach in listRefAttachs)
            table.Rows.Add(SOut.Long(refAttach.RefAttachNum), SOut.Long(refAttach.ReferralNum), SOut.Long(refAttach.PatNum), SOut.Int(refAttach.ItemOrder), SOut.DateT(refAttach.RefDate, false), SOut.Int((int) refAttach.RefType), SOut.Int((int) refAttach.RefToStatus), refAttach.Note, SOut.Bool(refAttach.IsTransitionOfCare), SOut.Long(refAttach.ProcNum), SOut.DateT(refAttach.DateProcComplete, false), SOut.Long(refAttach.ProvNum), SOut.DateT(refAttach.DateTStamp, false));
        return table;
    }

    public static long Insert(RefAttach refAttach)
    {
        return Insert(refAttach, false);
    }

    public static long Insert(RefAttach refAttach, bool useExistingPK)
    {
        var command = "INSERT INTO refattach (";

        command += "ReferralNum,PatNum,ItemOrder,RefDate,RefType,RefToStatus,Note,IsTransitionOfCare,ProcNum,DateProcComplete,ProvNum) VALUES(";

        command +=
            SOut.Long(refAttach.ReferralNum) + ","
                                             + SOut.Long(refAttach.PatNum) + ","
                                             + SOut.Int(refAttach.ItemOrder) + ","
                                             + SOut.Date(refAttach.RefDate) + ","
                                             + SOut.Int((int) refAttach.RefType) + ","
                                             + SOut.Int((int) refAttach.RefToStatus) + ","
                                             + DbHelper.ParamChar + "paramNote,"
                                             + SOut.Bool(refAttach.IsTransitionOfCare) + ","
                                             + SOut.Long(refAttach.ProcNum) + ","
                                             + SOut.Date(refAttach.DateProcComplete) + ","
                                             + SOut.Long(refAttach.ProvNum) + ")";
        //DateTStamp can only be set by MySQL
        if (refAttach.Note == null) refAttach.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(refAttach.Note));
        {
            refAttach.RefAttachNum = Db.NonQ(command, true, "RefAttachNum", "refAttach", paramNote);
        }
        return refAttach.RefAttachNum;
    }

    public static long InsertNoCache(RefAttach refAttach)
    {
        return InsertNoCache(refAttach, false);
    }

    public static long InsertNoCache(RefAttach refAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO refattach (";
        if (isRandomKeys || useExistingPK) command += "RefAttachNum,";
        command += "ReferralNum,PatNum,ItemOrder,RefDate,RefType,RefToStatus,Note,IsTransitionOfCare,ProcNum,DateProcComplete,ProvNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(refAttach.RefAttachNum) + ",";
        command +=
            SOut.Long(refAttach.ReferralNum) + ","
                                             + SOut.Long(refAttach.PatNum) + ","
                                             + SOut.Int(refAttach.ItemOrder) + ","
                                             + SOut.Date(refAttach.RefDate) + ","
                                             + SOut.Int((int) refAttach.RefType) + ","
                                             + SOut.Int((int) refAttach.RefToStatus) + ","
                                             + DbHelper.ParamChar + "paramNote,"
                                             + SOut.Bool(refAttach.IsTransitionOfCare) + ","
                                             + SOut.Long(refAttach.ProcNum) + ","
                                             + SOut.Date(refAttach.DateProcComplete) + ","
                                             + SOut.Long(refAttach.ProvNum) + ")";
        //DateTStamp can only be set by MySQL
        if (refAttach.Note == null) refAttach.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(refAttach.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            refAttach.RefAttachNum = Db.NonQ(command, true, "RefAttachNum", "refAttach", paramNote);
        return refAttach.RefAttachNum;
    }

    public static void Update(RefAttach refAttach)
    {
        var command = "UPDATE refattach SET "
                      + "ReferralNum       =  " + SOut.Long(refAttach.ReferralNum) + ", "
                      + "PatNum            =  " + SOut.Long(refAttach.PatNum) + ", "
                      + "ItemOrder         =  " + SOut.Int(refAttach.ItemOrder) + ", "
                      + "RefDate           =  " + SOut.Date(refAttach.RefDate) + ", "
                      + "RefType           =  " + SOut.Int((int) refAttach.RefType) + ", "
                      + "RefToStatus       =  " + SOut.Int((int) refAttach.RefToStatus) + ", "
                      + "Note              =  " + DbHelper.ParamChar + "paramNote, "
                      + "IsTransitionOfCare=  " + SOut.Bool(refAttach.IsTransitionOfCare) + ", "
                      + "ProcNum           =  " + SOut.Long(refAttach.ProcNum) + ", "
                      + "DateProcComplete  =  " + SOut.Date(refAttach.DateProcComplete) + ", "
                      + "ProvNum           =  " + SOut.Long(refAttach.ProvNum) + " "
                      //DateTStamp can only be set by MySQL
                      + "WHERE RefAttachNum = " + SOut.Long(refAttach.RefAttachNum);
        if (refAttach.Note == null) refAttach.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(refAttach.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(RefAttach refAttach, RefAttach oldRefAttach)
    {
        var command = "";
        if (refAttach.ReferralNum != oldRefAttach.ReferralNum)
        {
            if (command != "") command += ",";
            command += "ReferralNum = " + SOut.Long(refAttach.ReferralNum) + "";
        }

        if (refAttach.PatNum != oldRefAttach.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(refAttach.PatNum) + "";
        }

        if (refAttach.ItemOrder != oldRefAttach.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(refAttach.ItemOrder) + "";
        }

        if (refAttach.RefDate.Date != oldRefAttach.RefDate.Date)
        {
            if (command != "") command += ",";
            command += "RefDate = " + SOut.Date(refAttach.RefDate) + "";
        }

        if (refAttach.RefType != oldRefAttach.RefType)
        {
            if (command != "") command += ",";
            command += "RefType = " + SOut.Int((int) refAttach.RefType) + "";
        }

        if (refAttach.RefToStatus != oldRefAttach.RefToStatus)
        {
            if (command != "") command += ",";
            command += "RefToStatus = " + SOut.Int((int) refAttach.RefToStatus) + "";
        }

        if (refAttach.Note != oldRefAttach.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (refAttach.IsTransitionOfCare != oldRefAttach.IsTransitionOfCare)
        {
            if (command != "") command += ",";
            command += "IsTransitionOfCare = " + SOut.Bool(refAttach.IsTransitionOfCare) + "";
        }

        if (refAttach.ProcNum != oldRefAttach.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(refAttach.ProcNum) + "";
        }

        if (refAttach.DateProcComplete.Date != oldRefAttach.DateProcComplete.Date)
        {
            if (command != "") command += ",";
            command += "DateProcComplete = " + SOut.Date(refAttach.DateProcComplete) + "";
        }

        if (refAttach.ProvNum != oldRefAttach.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(refAttach.ProvNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (command == "") return false;
        if (refAttach.Note == null) refAttach.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(refAttach.Note));
        command = "UPDATE refattach SET " + command
                                          + " WHERE RefAttachNum = " + SOut.Long(refAttach.RefAttachNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(RefAttach refAttach, RefAttach oldRefAttach)
    {
        if (refAttach.ReferralNum != oldRefAttach.ReferralNum) return true;
        if (refAttach.PatNum != oldRefAttach.PatNum) return true;
        if (refAttach.ItemOrder != oldRefAttach.ItemOrder) return true;
        if (refAttach.RefDate.Date != oldRefAttach.RefDate.Date) return true;
        if (refAttach.RefType != oldRefAttach.RefType) return true;
        if (refAttach.RefToStatus != oldRefAttach.RefToStatus) return true;
        if (refAttach.Note != oldRefAttach.Note) return true;
        if (refAttach.IsTransitionOfCare != oldRefAttach.IsTransitionOfCare) return true;
        if (refAttach.ProcNum != oldRefAttach.ProcNum) return true;
        if (refAttach.DateProcComplete.Date != oldRefAttach.DateProcComplete.Date) return true;
        if (refAttach.ProvNum != oldRefAttach.ProvNum) return true;
        //DateTStamp can only be set by MySQL
        return false;
    }

    public static void Delete(long refAttachNum)
    {
        var command = "DELETE FROM refattach "
                      + "WHERE RefAttachNum = " + SOut.Long(refAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRefAttachNums)
    {
        if (listRefAttachNums == null || listRefAttachNums.Count == 0) return;
        var command = "DELETE FROM refattach "
                      + "WHERE RefAttachNum IN(" + string.Join(",", listRefAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}