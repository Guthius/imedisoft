#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoScheduleCrud
{
    public static OrthoSchedule SelectOne(long orthoScheduleNum)
    {
        var command = "SELECT * FROM orthoschedule "
                      + "WHERE OrthoScheduleNum = " + SOut.Long(orthoScheduleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoSchedule SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoSchedule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoSchedule> TableToList(DataTable table)
    {
        var retVal = new List<OrthoSchedule>();
        OrthoSchedule orthoSchedule;
        foreach (DataRow row in table.Rows)
        {
            orthoSchedule = new OrthoSchedule();
            orthoSchedule.OrthoScheduleNum = SIn.Long(row["OrthoScheduleNum"].ToString());
            orthoSchedule.BandingDateOverride = SIn.Date(row["BandingDateOverride"].ToString());
            orthoSchedule.DebondDateOverride = SIn.Date(row["DebondDateOverride"].ToString());
            orthoSchedule.BandingAmount = SIn.Double(row["BandingAmount"].ToString());
            orthoSchedule.VisitAmount = SIn.Double(row["VisitAmount"].ToString());
            orthoSchedule.DebondAmount = SIn.Double(row["DebondAmount"].ToString());
            orthoSchedule.IsActive = SIn.Bool(row["IsActive"].ToString());
            orthoSchedule.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(orthoSchedule);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoSchedule> listOrthoSchedules, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoSchedule";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoScheduleNum");
        table.Columns.Add("BandingDateOverride");
        table.Columns.Add("DebondDateOverride");
        table.Columns.Add("BandingAmount");
        table.Columns.Add("VisitAmount");
        table.Columns.Add("DebondAmount");
        table.Columns.Add("IsActive");
        table.Columns.Add("SecDateTEdit");
        foreach (var orthoSchedule in listOrthoSchedules)
            table.Rows.Add(SOut.Long(orthoSchedule.OrthoScheduleNum), SOut.DateT(orthoSchedule.BandingDateOverride, false), SOut.DateT(orthoSchedule.DebondDateOverride, false), SOut.Double(orthoSchedule.BandingAmount), SOut.Double(orthoSchedule.VisitAmount), SOut.Double(orthoSchedule.DebondAmount), SOut.Bool(orthoSchedule.IsActive), SOut.DateT(orthoSchedule.SecDateTEdit, false));
        return table;
    }

    public static long Insert(OrthoSchedule orthoSchedule)
    {
        return Insert(orthoSchedule, false);
    }

    public static long Insert(OrthoSchedule orthoSchedule, bool useExistingPK)
    {
        var command = "INSERT INTO orthoschedule (";

        command += "BandingDateOverride,DebondDateOverride,BandingAmount,VisitAmount,DebondAmount,IsActive) VALUES(";

        command +=
            SOut.Date(orthoSchedule.BandingDateOverride) + ","
                                                         + SOut.Date(orthoSchedule.DebondDateOverride) + ","
                                                         + SOut.Double(orthoSchedule.BandingAmount) + ","
                                                         + SOut.Double(orthoSchedule.VisitAmount) + ","
                                                         + SOut.Double(orthoSchedule.DebondAmount) + ","
                                                         + SOut.Bool(orthoSchedule.IsActive) + ")";
        //SecDateTEdit can only be set by MySQL

        orthoSchedule.OrthoScheduleNum = Db.NonQ(command, true, "OrthoScheduleNum", "orthoSchedule");
        return orthoSchedule.OrthoScheduleNum;
    }

    public static long InsertNoCache(OrthoSchedule orthoSchedule)
    {
        return InsertNoCache(orthoSchedule, false);
    }

    public static long InsertNoCache(OrthoSchedule orthoSchedule, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthoschedule (";
        if (isRandomKeys || useExistingPK) command += "OrthoScheduleNum,";
        command += "BandingDateOverride,DebondDateOverride,BandingAmount,VisitAmount,DebondAmount,IsActive) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoSchedule.OrthoScheduleNum) + ",";
        command +=
            SOut.Date(orthoSchedule.BandingDateOverride) + ","
                                                         + SOut.Date(orthoSchedule.DebondDateOverride) + ","
                                                         + SOut.Double(orthoSchedule.BandingAmount) + ","
                                                         + SOut.Double(orthoSchedule.VisitAmount) + ","
                                                         + SOut.Double(orthoSchedule.DebondAmount) + ","
                                                         + SOut.Bool(orthoSchedule.IsActive) + ")";
        //SecDateTEdit can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoSchedule.OrthoScheduleNum = Db.NonQ(command, true, "OrthoScheduleNum", "orthoSchedule");
        return orthoSchedule.OrthoScheduleNum;
    }

    public static void Update(OrthoSchedule orthoSchedule)
    {
        var command = "UPDATE orthoschedule SET "
                      + "BandingDateOverride=  " + SOut.Date(orthoSchedule.BandingDateOverride) + ", "
                      + "DebondDateOverride =  " + SOut.Date(orthoSchedule.DebondDateOverride) + ", "
                      + "BandingAmount      =  " + SOut.Double(orthoSchedule.BandingAmount) + ", "
                      + "VisitAmount        =  " + SOut.Double(orthoSchedule.VisitAmount) + ", "
                      + "DebondAmount       =  " + SOut.Double(orthoSchedule.DebondAmount) + ", "
                      + "IsActive           =  " + SOut.Bool(orthoSchedule.IsActive) + " "
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE OrthoScheduleNum = " + SOut.Long(orthoSchedule.OrthoScheduleNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoSchedule orthoSchedule, OrthoSchedule oldOrthoSchedule)
    {
        var command = "";
        if (orthoSchedule.BandingDateOverride.Date != oldOrthoSchedule.BandingDateOverride.Date)
        {
            if (command != "") command += ",";
            command += "BandingDateOverride = " + SOut.Date(orthoSchedule.BandingDateOverride) + "";
        }

        if (orthoSchedule.DebondDateOverride.Date != oldOrthoSchedule.DebondDateOverride.Date)
        {
            if (command != "") command += ",";
            command += "DebondDateOverride = " + SOut.Date(orthoSchedule.DebondDateOverride) + "";
        }

        if (orthoSchedule.BandingAmount != oldOrthoSchedule.BandingAmount)
        {
            if (command != "") command += ",";
            command += "BandingAmount = " + SOut.Double(orthoSchedule.BandingAmount) + "";
        }

        if (orthoSchedule.VisitAmount != oldOrthoSchedule.VisitAmount)
        {
            if (command != "") command += ",";
            command += "VisitAmount = " + SOut.Double(orthoSchedule.VisitAmount) + "";
        }

        if (orthoSchedule.DebondAmount != oldOrthoSchedule.DebondAmount)
        {
            if (command != "") command += ",";
            command += "DebondAmount = " + SOut.Double(orthoSchedule.DebondAmount) + "";
        }

        if (orthoSchedule.IsActive != oldOrthoSchedule.IsActive)
        {
            if (command != "") command += ",";
            command += "IsActive = " + SOut.Bool(orthoSchedule.IsActive) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE orthoschedule SET " + command
                                              + " WHERE OrthoScheduleNum = " + SOut.Long(orthoSchedule.OrthoScheduleNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoSchedule orthoSchedule, OrthoSchedule oldOrthoSchedule)
    {
        if (orthoSchedule.BandingDateOverride.Date != oldOrthoSchedule.BandingDateOverride.Date) return true;
        if (orthoSchedule.DebondDateOverride.Date != oldOrthoSchedule.DebondDateOverride.Date) return true;
        if (orthoSchedule.BandingAmount != oldOrthoSchedule.BandingAmount) return true;
        if (orthoSchedule.VisitAmount != oldOrthoSchedule.VisitAmount) return true;
        if (orthoSchedule.DebondAmount != oldOrthoSchedule.DebondAmount) return true;
        if (orthoSchedule.IsActive != oldOrthoSchedule.IsActive) return true;
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long orthoScheduleNum)
    {
        var command = "DELETE FROM orthoschedule "
                      + "WHERE OrthoScheduleNum = " + SOut.Long(orthoScheduleNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoScheduleNums)
    {
        if (listOrthoScheduleNums == null || listOrthoScheduleNums.Count == 0) return;
        var command = "DELETE FROM orthoschedule "
                      + "WHERE OrthoScheduleNum IN(" + string.Join(",", listOrthoScheduleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}