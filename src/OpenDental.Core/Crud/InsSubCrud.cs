using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsSubCrud
{
    public static InsSub SelectOne(long insSubNum)
    {
        var command = "SELECT * FROM inssub "
                      + "WHERE InsSubNum = " + SOut.Long(insSubNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsSub> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsSub> TableToList(DataTable table)
    {
        var retVal = new List<InsSub>();
        foreach (DataRow row in table.Rows)
        {
            var insSub = new InsSub
            {
                InsSubNum = SIn.Long(row["InsSubNum"].ToString()),
                PlanNum = SIn.Long(row["PlanNum"].ToString()),
                Subscriber = SIn.Long(row["Subscriber"].ToString()),
                DateEffective = SIn.Date(row["DateEffective"].ToString()),
                DateTerm = SIn.Date(row["DateTerm"].ToString()),
                ReleaseInfo = SIn.Bool(row["ReleaseInfo"].ToString()),
                AssignBen = SIn.Bool(row["AssignBen"].ToString()),
                SubscriberID = SIn.String(row["SubscriberID"].ToString()),
                BenefitNotes = SIn.String(row["BenefitNotes"].ToString()),
                SubscNote = SIn.String(row["SubscNote"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(insSub);
        }

        return retVal;
    }

    public static long Insert(InsSub insSub)
    {
        var command = "INSERT INTO inssub (";

        command += "PlanNum,Subscriber,DateEffective,DateTerm,ReleaseInfo,AssignBen,SubscriberID,BenefitNotes,SubscNote,SecUserNumEntry,SecDateEntry) VALUES(";

        command +=
            SOut.Long(insSub.PlanNum) + ","
                                      + SOut.Long(insSub.Subscriber) + ","
                                      + SOut.Date(insSub.DateEffective) + ","
                                      + SOut.Date(insSub.DateTerm) + ","
                                      + SOut.Bool(insSub.ReleaseInfo) + ","
                                      + SOut.Bool(insSub.AssignBen) + ","
                                      + "'" + SOut.String(insSub.SubscriberID) + "',"
                                      + DbHelper.ParamChar + "paramBenefitNotes,"
                                      + DbHelper.ParamChar + "paramSubscNote,"
                                      + SOut.Long(insSub.SecUserNumEntry) + ","
                                      + "NOW()" + ")";
        //SecDateTEdit can only be set by MySQL
        if (insSub.BenefitNotes == null) insSub.BenefitNotes = "";
        var paramBenefitNotes = new OdSqlParameter("paramBenefitNotes", SOut.StringParam(insSub.BenefitNotes));
        if (insSub.SubscNote == null) insSub.SubscNote = "";
        var paramSubscNote = new OdSqlParameter("paramSubscNote", SOut.StringParam(insSub.SubscNote));
        {
            insSub.InsSubNum = Db.NonQ(command, true, "InsSubNum", "insSub", paramBenefitNotes, paramSubscNote);
        }
        return insSub.InsSubNum;
    }

    public static void Update(InsSub insSub)
    {
        var command = "UPDATE inssub SET "
                      + "PlanNum        =  " + SOut.Long(insSub.PlanNum) + ", "
                      + "Subscriber     =  " + SOut.Long(insSub.Subscriber) + ", "
                      + "DateEffective  =  " + SOut.Date(insSub.DateEffective) + ", "
                      + "DateTerm       =  " + SOut.Date(insSub.DateTerm) + ", "
                      + "ReleaseInfo    =  " + SOut.Bool(insSub.ReleaseInfo) + ", "
                      + "AssignBen      =  " + SOut.Bool(insSub.AssignBen) + ", "
                      + "SubscriberID   = '" + SOut.String(insSub.SubscriberID) + "', "
                      + "BenefitNotes   =  " + DbHelper.ParamChar + "paramBenefitNotes, "
                      + "SubscNote      =  " + DbHelper.ParamChar + "paramSubscNote "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE InsSubNum = " + SOut.Long(insSub.InsSubNum);
        if (insSub.BenefitNotes == null) insSub.BenefitNotes = "";
        var paramBenefitNotes = new OdSqlParameter("paramBenefitNotes", SOut.StringParam(insSub.BenefitNotes));
        if (insSub.SubscNote == null) insSub.SubscNote = "";
        var paramSubscNote = new OdSqlParameter("paramSubscNote", SOut.StringParam(insSub.SubscNote));
        Db.NonQ(command, paramBenefitNotes, paramSubscNote);
    }

    public static bool UpdateComparison(InsSub insSub, InsSub oldInsSub)
    {
        if (insSub.PlanNum != oldInsSub.PlanNum) return true;
        if (insSub.Subscriber != oldInsSub.Subscriber) return true;
        if (insSub.DateEffective.Date != oldInsSub.DateEffective.Date) return true;
        if (insSub.DateTerm.Date != oldInsSub.DateTerm.Date) return true;
        if (insSub.ReleaseInfo != oldInsSub.ReleaseInfo) return true;
        if (insSub.AssignBen != oldInsSub.AssignBen) return true;
        if (insSub.SubscriberID != oldInsSub.SubscriberID) return true;
        if (insSub.BenefitNotes != oldInsSub.BenefitNotes) return true;
        if (insSub.SubscNote != oldInsSub.SubscNote) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long insSubNum)
    {
        ClearFkey(insSubNum);
        var command = "DELETE FROM inssub "
                      + "WHERE InsSubNum = " + SOut.Long(insSubNum);
        Db.NonQ(command);
    }

    public static void ClearFkey(long insSubNum)
    {
        if (insSubNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(insSubNum) + " AND PermType IN (155)";
        Db.NonQ(command);
    }
}