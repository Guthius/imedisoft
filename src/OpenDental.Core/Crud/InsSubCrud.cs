#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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

    public static InsSub SelectOne(string command)
    {
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
        InsSub insSub;
        foreach (DataRow row in table.Rows)
        {
            insSub = new InsSub();
            insSub.InsSubNum = SIn.Long(row["InsSubNum"].ToString());
            insSub.PlanNum = SIn.Long(row["PlanNum"].ToString());
            insSub.Subscriber = SIn.Long(row["Subscriber"].ToString());
            insSub.DateEffective = SIn.Date(row["DateEffective"].ToString());
            insSub.DateTerm = SIn.Date(row["DateTerm"].ToString());
            insSub.ReleaseInfo = SIn.Bool(row["ReleaseInfo"].ToString());
            insSub.AssignBen = SIn.Bool(row["AssignBen"].ToString());
            insSub.SubscriberID = SIn.String(row["SubscriberID"].ToString());
            insSub.BenefitNotes = SIn.String(row["BenefitNotes"].ToString());
            insSub.SubscNote = SIn.String(row["SubscNote"].ToString());
            insSub.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            insSub.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            insSub.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(insSub);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsSub> listInsSubs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsSub";
        var table = new DataTable(tableName);
        table.Columns.Add("InsSubNum");
        table.Columns.Add("PlanNum");
        table.Columns.Add("Subscriber");
        table.Columns.Add("DateEffective");
        table.Columns.Add("DateTerm");
        table.Columns.Add("ReleaseInfo");
        table.Columns.Add("AssignBen");
        table.Columns.Add("SubscriberID");
        table.Columns.Add("BenefitNotes");
        table.Columns.Add("SubscNote");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var insSub in listInsSubs)
            table.Rows.Add(SOut.Long(insSub.InsSubNum), SOut.Long(insSub.PlanNum), SOut.Long(insSub.Subscriber), SOut.DateT(insSub.DateEffective, false), SOut.DateT(insSub.DateTerm, false), SOut.Bool(insSub.ReleaseInfo), SOut.Bool(insSub.AssignBen), insSub.SubscriberID, insSub.BenefitNotes, insSub.SubscNote, SOut.Long(insSub.SecUserNumEntry), SOut.DateT(insSub.SecDateEntry, false), SOut.DateT(insSub.SecDateTEdit, false));
        return table;
    }

    public static long Insert(InsSub insSub)
    {
        return Insert(insSub, false);
    }

    public static long Insert(InsSub insSub, bool useExistingPK)
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
                                      + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (insSub.BenefitNotes == null) insSub.BenefitNotes = "";
        var paramBenefitNotes = new OdSqlParameter("paramBenefitNotes", OdDbType.Text, SOut.StringParam(insSub.BenefitNotes));
        if (insSub.SubscNote == null) insSub.SubscNote = "";
        var paramSubscNote = new OdSqlParameter("paramSubscNote", OdDbType.Text, SOut.StringParam(insSub.SubscNote));
        {
            insSub.InsSubNum = Db.NonQ(command, true, "InsSubNum", "insSub", paramBenefitNotes, paramSubscNote);
        }
        return insSub.InsSubNum;
    }

    public static long InsertNoCache(InsSub insSub)
    {
        return InsertNoCache(insSub, false);
    }

    public static long InsertNoCache(InsSub insSub, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO inssub (";
        if (isRandomKeys || useExistingPK) command += "InsSubNum,";
        command += "PlanNum,Subscriber,DateEffective,DateTerm,ReleaseInfo,AssignBen,SubscriberID,BenefitNotes,SubscNote,SecUserNumEntry,SecDateEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insSub.InsSubNum) + ",";
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
                                      + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (insSub.BenefitNotes == null) insSub.BenefitNotes = "";
        var paramBenefitNotes = new OdSqlParameter("paramBenefitNotes", OdDbType.Text, SOut.StringParam(insSub.BenefitNotes));
        if (insSub.SubscNote == null) insSub.SubscNote = "";
        var paramSubscNote = new OdSqlParameter("paramSubscNote", OdDbType.Text, SOut.StringParam(insSub.SubscNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramBenefitNotes, paramSubscNote);
        else
            insSub.InsSubNum = Db.NonQ(command, true, "InsSubNum", "insSub", paramBenefitNotes, paramSubscNote);
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
        var paramBenefitNotes = new OdSqlParameter("paramBenefitNotes", OdDbType.Text, SOut.StringParam(insSub.BenefitNotes));
        if (insSub.SubscNote == null) insSub.SubscNote = "";
        var paramSubscNote = new OdSqlParameter("paramSubscNote", OdDbType.Text, SOut.StringParam(insSub.SubscNote));
        Db.NonQ(command, paramBenefitNotes, paramSubscNote);
    }

    public static bool Update(InsSub insSub, InsSub oldInsSub)
    {
        var command = "";
        if (insSub.PlanNum != oldInsSub.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(insSub.PlanNum) + "";
        }

        if (insSub.Subscriber != oldInsSub.Subscriber)
        {
            if (command != "") command += ",";
            command += "Subscriber = " + SOut.Long(insSub.Subscriber) + "";
        }

        if (insSub.DateEffective.Date != oldInsSub.DateEffective.Date)
        {
            if (command != "") command += ",";
            command += "DateEffective = " + SOut.Date(insSub.DateEffective) + "";
        }

        if (insSub.DateTerm.Date != oldInsSub.DateTerm.Date)
        {
            if (command != "") command += ",";
            command += "DateTerm = " + SOut.Date(insSub.DateTerm) + "";
        }

        if (insSub.ReleaseInfo != oldInsSub.ReleaseInfo)
        {
            if (command != "") command += ",";
            command += "ReleaseInfo = " + SOut.Bool(insSub.ReleaseInfo) + "";
        }

        if (insSub.AssignBen != oldInsSub.AssignBen)
        {
            if (command != "") command += ",";
            command += "AssignBen = " + SOut.Bool(insSub.AssignBen) + "";
        }

        if (insSub.SubscriberID != oldInsSub.SubscriberID)
        {
            if (command != "") command += ",";
            command += "SubscriberID = '" + SOut.String(insSub.SubscriberID) + "'";
        }

        if (insSub.BenefitNotes != oldInsSub.BenefitNotes)
        {
            if (command != "") command += ",";
            command += "BenefitNotes = " + DbHelper.ParamChar + "paramBenefitNotes";
        }

        if (insSub.SubscNote != oldInsSub.SubscNote)
        {
            if (command != "") command += ",";
            command += "SubscNote = " + DbHelper.ParamChar + "paramSubscNote";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        if (insSub.BenefitNotes == null) insSub.BenefitNotes = "";
        var paramBenefitNotes = new OdSqlParameter("paramBenefitNotes", OdDbType.Text, SOut.StringParam(insSub.BenefitNotes));
        if (insSub.SubscNote == null) insSub.SubscNote = "";
        var paramSubscNote = new OdSqlParameter("paramSubscNote", OdDbType.Text, SOut.StringParam(insSub.SubscNote));
        command = "UPDATE inssub SET " + command
                                       + " WHERE InsSubNum = " + SOut.Long(insSub.InsSubNum);
        Db.NonQ(command, paramBenefitNotes, paramSubscNote);
        return true;
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

    public static void DeleteMany(List<long> listInsSubNums)
    {
        if (listInsSubNums == null || listInsSubNums.Count == 0) return;
        ClearFkey(listInsSubNums);
        var command = "DELETE FROM inssub "
                      + "WHERE InsSubNum IN(" + string.Join(",", listInsSubNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void ClearFkey(long insSubNum)
    {
        if (insSubNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(insSubNum) + " AND PermType IN (155)";
        Db.NonQ(command);
    }

    public static void ClearFkey(List<long> listInsSubNums)
    {
        if (listInsSubNums == null || listInsSubNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listInsSubNums.FindAll(x => x != 0)) + ") AND PermType IN (155)";
        Db.NonQ(command);
    }
}