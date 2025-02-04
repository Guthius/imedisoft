using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class RefAttachCrud
{
    public static List<RefAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RefAttach> TableToList(DataTable table)
    {
        var retVal = new List<RefAttach>();
        foreach (DataRow row in table.Rows)
        {
            var refAttach = new RefAttach
            {
                RefAttachNum = SIn.Long(row["RefAttachNum"].ToString()),
                ReferralNum = SIn.Long(row["ReferralNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                RefDate = SIn.Date(row["RefDate"].ToString()),
                RefType = (ReferralType) SIn.Int(row["RefType"].ToString()),
                RefToStatus = (ReferralToStatus) SIn.Int(row["RefToStatus"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                IsTransitionOfCare = SIn.Bool(row["IsTransitionOfCare"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                DateProcComplete = SIn.Date(row["DateProcComplete"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString())
            };
            retVal.Add(refAttach);
        }

        return retVal;
    }

    public static void Insert(RefAttach refAttach)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(refAttach.Note));
        {
            refAttach.RefAttachNum = Db.NonQ(command, true, "RefAttachNum", "refAttach", paramNote);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(refAttach.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Update(RefAttach refAttach, RefAttach oldRefAttach)
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
        if (command == "") return;
        if (refAttach.Note == null) refAttach.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(refAttach.Note));
        command = "UPDATE refattach SET " + command
                                          + " WHERE RefAttachNum = " + SOut.Long(refAttach.RefAttachNum);
        Db.NonQ(command, paramNote);
    }
}