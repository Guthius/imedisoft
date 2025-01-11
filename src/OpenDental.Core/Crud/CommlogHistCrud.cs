using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        CommlogHist commlogHist;
        foreach (DataRow row in table.Rows)
        {
            commlogHist = new CommlogHist();
            commlogHist.CommlogHistNum = SIn.Long(row["CommlogHistNum"].ToString());
            commlogHist.CustomerNumberRaw = SIn.String(row["CustomerNumberRaw"].ToString());
            commlogHist.HistSource = (CommlogHistSource) SIn.Int(row["HistSource"].ToString());
            commlogHist.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            commlogHist.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            commlogHist.CommlogNum = SIn.Long(row["CommlogNum"].ToString());
            commlogHist.PatNum = SIn.Long(row["PatNum"].ToString());
            commlogHist.CommDateTime = SIn.DateTime(row["CommDateTime"].ToString());
            commlogHist.CommType = SIn.Long(row["CommType"].ToString());
            commlogHist.Note = SIn.String(row["Note"].ToString());
            commlogHist.Mode_ = (CommItemMode) SIn.Int(row["Mode_"].ToString());
            commlogHist.SentOrReceived = (CommSentOrReceived) SIn.Int(row["SentOrReceived"].ToString());
            commlogHist.UserNum = SIn.Long(row["UserNum"].ToString());
            commlogHist.Signature = SIn.String(row["Signature"].ToString());
            commlogHist.SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString());
            commlogHist.DateTimeEnd = SIn.DateTime(row["DateTimeEnd"].ToString());
            commlogHist.CommSource = (CommItemSource) SIn.Int(row["CommSource"].ToString());
            commlogHist.ProgramNum = SIn.Long(row["ProgramNum"].ToString());
            commlogHist.ReferralNum = SIn.Long(row["ReferralNum"].ToString());
            commlogHist.CommReferralBehavior = (EnumCommReferralBehavior) SIn.Int(row["CommReferralBehavior"].ToString());
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
            + DbHelper.Now() + ","
            + SOut.Long(commlogHist.CommlogNum) + ","
            + SOut.Long(commlogHist.PatNum) + ","
            + SOut.DateT(commlogHist.CommDateTime) + ","
            + SOut.Long(commlogHist.CommType) + ","
            + DbHelper.ParamChar + "paramNote,"
            + SOut.Int((int) commlogHist.Mode_) + ","
            + SOut.Int((int) commlogHist.SentOrReceived) + ","
            + SOut.Long(commlogHist.UserNum) + ","
            + DbHelper.ParamChar + "paramSignature,"
            + SOut.Bool(commlogHist.SigIsTopaz) + ","
            + SOut.DateT(commlogHist.DateTimeEnd) + ","
            + SOut.Int((int) commlogHist.CommSource) + ","
            + SOut.Long(commlogHist.ProgramNum) + ","
            + SOut.Long(commlogHist.ReferralNum) + ","
            + SOut.Int((int) commlogHist.CommReferralBehavior) + ")";
        if (commlogHist.Note == null) commlogHist.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(commlogHist.Note));
        if (commlogHist.Signature == null) commlogHist.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(commlogHist.Signature));
        {
            commlogHist.CommlogHistNum = Db.NonQ(command, true, "CommlogHistNum", "commlogHist", paramNote, paramSignature);
        }
    }
}