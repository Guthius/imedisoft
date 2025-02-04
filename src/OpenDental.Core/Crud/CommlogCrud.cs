using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class CommlogCrud
{
    public static Commlog SelectOne(long commlogNum)
    {
        var command = "SELECT * FROM commlog "
                      + "WHERE CommlogNum = " + SOut.Long(commlogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Commlog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Commlog> TableToList(DataTable table)
    {
        var retVal = new List<Commlog>();
        foreach (DataRow row in table.Rows)
        {
            var commlog = new Commlog
            {
                CommlogNum = SIn.Long(row["CommlogNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                CommDateTime = SIn.DateTime(row["CommDateTime"].ToString()),
                CommType = SIn.Long(row["CommType"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                Mode_ = (CommItemMode) SIn.Int(row["Mode_"].ToString()),
                SentOrReceived = (CommSentOrReceived) SIn.Int(row["SentOrReceived"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                Signature = SIn.String(row["Signature"].ToString()),
                SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                DateTimeEnd = SIn.DateTime(row["DateTimeEnd"].ToString()),
                CommSource = (CommItemSource) SIn.Int(row["CommSource"].ToString()),
                ProgramNum = SIn.Long(row["ProgramNum"].ToString()),
                DateTEntry = SIn.DateTime(row["DateTEntry"].ToString()),
                ReferralNum = SIn.Long(row["ReferralNum"].ToString()),
                CommReferralBehavior = (EnumCommReferralBehavior) SIn.Int(row["CommReferralBehavior"].ToString())
            };
            retVal.Add(commlog);
        }

        return retVal;
    }

    public static long Insert(Commlog commlog)
    {
        var command = "INSERT INTO commlog (";

        command += "PatNum,CommDateTime,CommType,Note,Mode_,SentOrReceived,UserNum,Signature,SigIsTopaz,DateTimeEnd,CommSource,ProgramNum,DateTEntry,ReferralNum,CommReferralBehavior) VALUES(";

        command +=
            SOut.Long(commlog.PatNum) + ","
                                      + SOut.DateTime(commlog.CommDateTime) + ","
                                      + SOut.Long(commlog.CommType) + ","
                                      + DbHelper.ParamChar + "paramNote,"
                                      + SOut.Int((int) commlog.Mode_) + ","
                                      + SOut.Int((int) commlog.SentOrReceived) + ","
                                      + SOut.Long(commlog.UserNum) + ","
                                      + DbHelper.ParamChar + "paramSignature,"
                                      + SOut.Bool(commlog.SigIsTopaz) + ","
                                      //DateTStamp can only be set by MySQL
                                      + SOut.DateTime(commlog.DateTimeEnd) + ","
                                      + SOut.Int((int) commlog.CommSource) + ","
                                      + SOut.Long(commlog.ProgramNum) + ","
                                      + "NOW()" + ","
                                      + SOut.Long(commlog.ReferralNum) + ","
                                      + SOut.Int((int) commlog.CommReferralBehavior) + ")";
        if (commlog.Note == null) commlog.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(commlog.Note));
        if (commlog.Signature == null) commlog.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(commlog.Signature));
        {
            commlog.CommlogNum = Db.NonQ(command, true, "CommlogNum", "commlog", paramNote, paramSignature);
        }
        return commlog.CommlogNum;
    }

    public static void Update(Commlog commlog)
    {
        var command = "UPDATE commlog SET "
                      + "PatNum              =  " + SOut.Long(commlog.PatNum) + ", "
                      + "CommDateTime        =  " + SOut.DateTime(commlog.CommDateTime) + ", "
                      + "CommType            =  " + SOut.Long(commlog.CommType) + ", "
                      + "Note                =  " + DbHelper.ParamChar + "paramNote, "
                      + "Mode_               =  " + SOut.Int((int) commlog.Mode_) + ", "
                      + "SentOrReceived      =  " + SOut.Int((int) commlog.SentOrReceived) + ", "
                      + "UserNum             =  " + SOut.Long(commlog.UserNum) + ", "
                      + "Signature           =  " + DbHelper.ParamChar + "paramSignature, "
                      + "SigIsTopaz          =  " + SOut.Bool(commlog.SigIsTopaz) + ", "
                      //DateTStamp can only be set by MySQL
                      + "DateTimeEnd         =  " + SOut.DateTime(commlog.DateTimeEnd) + ", "
                      + "CommSource          =  " + SOut.Int((int) commlog.CommSource) + ", "
                      + "ProgramNum          =  " + SOut.Long(commlog.ProgramNum) + ", "
                      //DateTEntry not allowed to change
                      + "ReferralNum         =  " + SOut.Long(commlog.ReferralNum) + ", "
                      + "CommReferralBehavior=  " + SOut.Int((int) commlog.CommReferralBehavior) + " "
                      + "WHERE CommlogNum = " + SOut.Long(commlog.CommlogNum);
        if (commlog.Note == null) commlog.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(commlog.Note));
        if (commlog.Signature == null) commlog.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(commlog.Signature));
        Db.NonQ(command, paramNote, paramSignature);
    }

    public static void Update(Commlog commlog, Commlog oldCommlog)
    {
        var command = "";
        if (commlog.PatNum != oldCommlog.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(commlog.PatNum) + "";
        }

        if (commlog.CommDateTime != oldCommlog.CommDateTime)
        {
            if (command != "") command += ",";
            command += "CommDateTime = " + SOut.DateTime(commlog.CommDateTime) + "";
        }

        if (commlog.CommType != oldCommlog.CommType)
        {
            if (command != "") command += ",";
            command += "CommType = " + SOut.Long(commlog.CommType) + "";
        }

        if (commlog.Note != oldCommlog.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (commlog.Mode_ != oldCommlog.Mode_)
        {
            if (command != "") command += ",";
            command += "Mode_ = " + SOut.Int((int) commlog.Mode_) + "";
        }

        if (commlog.SentOrReceived != oldCommlog.SentOrReceived)
        {
            if (command != "") command += ",";
            command += "SentOrReceived = " + SOut.Int((int) commlog.SentOrReceived) + "";
        }

        if (commlog.UserNum != oldCommlog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(commlog.UserNum) + "";
        }

        if (commlog.Signature != oldCommlog.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (commlog.SigIsTopaz != oldCommlog.SigIsTopaz)
        {
            if (command != "") command += ",";
            command += "SigIsTopaz = " + SOut.Bool(commlog.SigIsTopaz) + "";
        }

        //DateTStamp can only be set by MySQL
        if (commlog.DateTimeEnd != oldCommlog.DateTimeEnd)
        {
            if (command != "") command += ",";
            command += "DateTimeEnd = " + SOut.DateTime(commlog.DateTimeEnd) + "";
        }

        if (commlog.CommSource != oldCommlog.CommSource)
        {
            if (command != "") command += ",";
            command += "CommSource = " + SOut.Int((int) commlog.CommSource) + "";
        }

        if (commlog.ProgramNum != oldCommlog.ProgramNum)
        {
            if (command != "") command += ",";
            command += "ProgramNum = " + SOut.Long(commlog.ProgramNum) + "";
        }

        //DateTEntry not allowed to change
        if (commlog.ReferralNum != oldCommlog.ReferralNum)
        {
            if (command != "") command += ",";
            command += "ReferralNum = " + SOut.Long(commlog.ReferralNum) + "";
        }

        if (commlog.CommReferralBehavior != oldCommlog.CommReferralBehavior)
        {
            if (command != "") command += ",";
            command += "CommReferralBehavior = " + SOut.Int((int) commlog.CommReferralBehavior) + "";
        }

        if (command == "") return;
        if (commlog.Note == null) commlog.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(commlog.Note));
        if (commlog.Signature == null) commlog.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(commlog.Signature));
        command = "UPDATE commlog SET " + command
                                        + " WHERE CommlogNum = " + SOut.Long(commlog.CommlogNum);
        Db.NonQ(command, paramNote, paramSignature);
    }

    public static void Delete(long commlogNum)
    {
        var command = "DELETE FROM commlog "
                      + "WHERE CommlogNum = " + SOut.Long(commlogNum);
        Db.NonQ(command);
    }
}