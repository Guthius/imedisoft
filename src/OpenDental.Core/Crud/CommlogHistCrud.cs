using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class CommlogHistCrud
{
    public static List<CommlogHist> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CommlogHist> TableToList(DataTable table)
    {
        var retVal = new List<CommlogHist>();
        foreach (DataRow row in table.Rows)
        {
            var commlogHist = new CommlogHist
            {
                CommlogHistNum = SIn.Long(row["CommlogHistNum"].ToString()),
                CustomerNumberRaw = SIn.String(row["CustomerNumberRaw"].ToString()),
                HistSource = (CommlogHistSource) SIn.Int(row["HistSource"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                DateTEntry = SIn.DateTime(row["DateTEntry"].ToString()),
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
                DateTimeEnd = SIn.DateTime(row["DateTimeEnd"].ToString()),
                CommSource = (CommItemSource) SIn.Int(row["CommSource"].ToString()),
                ProgramNum = SIn.Long(row["ProgramNum"].ToString()),
                ReferralNum = SIn.Long(row["ReferralNum"].ToString()),
                CommReferralBehavior = (EnumCommReferralBehavior) SIn.Int(row["CommReferralBehavior"].ToString())
            };
            retVal.Add(commlogHist);
        }

        return retVal;
    }

    public static void Insert(CommlogHist commlogHist)
    {
        var command = "INSERT INTO commloghist (";

        command += "CustomerNumberRaw,HistSource,DateTEntry,CommlogNum,PatNum,CommDateTime,CommType,Note,Mode_,SentOrReceived,UserNum,Signature,SigIsTopaz,DateTimeEnd,CommSource,ProgramNum,ReferralNum,CommReferralBehavior) VALUES(";

        command +=
            "'" + SOut.String(commlogHist.CustomerNumberRaw) + "',"
            + SOut.Int((int) commlogHist.HistSource) + ","
            //DateTStamp can only be set by MySQL
            + "NOW()" + ","
            + SOut.Long(commlogHist.CommlogNum) + ","
            + SOut.Long(commlogHist.PatNum) + ","
            + SOut.DateTime(commlogHist.CommDateTime) + ","
            + SOut.Long(commlogHist.CommType) + ","
            + DbHelper.ParamChar + "paramNote,"
            + SOut.Int((int) commlogHist.Mode_) + ","
            + SOut.Int((int) commlogHist.SentOrReceived) + ","
            + SOut.Long(commlogHist.UserNum) + ","
            + DbHelper.ParamChar + "paramSignature,"
            + SOut.Bool(commlogHist.SigIsTopaz) + ","
            + SOut.DateTime(commlogHist.DateTimeEnd) + ","
            + SOut.Int((int) commlogHist.CommSource) + ","
            + SOut.Long(commlogHist.ProgramNum) + ","
            + SOut.Long(commlogHist.ReferralNum) + ","
            + SOut.Int((int) commlogHist.CommReferralBehavior) + ")";
        if (commlogHist.Note == null) commlogHist.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(commlogHist.Note));
        if (commlogHist.Signature == null) commlogHist.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(commlogHist.Signature));
        {
            commlogHist.CommlogHistNum = Db.NonQ(command, true, "CommlogHistNum", "commlogHist", paramNote, paramSignature);
        }
    }
}