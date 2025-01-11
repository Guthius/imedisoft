#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DiscountPlanCrud
{
    public static DiscountPlan SelectOne(long discountPlanNum)
    {
        var command = "SELECT * FROM discountplan "
                      + "WHERE DiscountPlanNum = " + SOut.Long(discountPlanNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DiscountPlan SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DiscountPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DiscountPlan> TableToList(DataTable table)
    {
        var retVal = new List<DiscountPlan>();
        DiscountPlan discountPlan;
        foreach (DataRow row in table.Rows)
        {
            discountPlan = new DiscountPlan();
            discountPlan.DiscountPlanNum = SIn.Long(row["DiscountPlanNum"].ToString());
            discountPlan.Description = SIn.String(row["Description"].ToString());
            discountPlan.FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString());
            discountPlan.DefNum = SIn.Long(row["DefNum"].ToString());
            discountPlan.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            discountPlan.PlanNote = SIn.String(row["PlanNote"].ToString());
            discountPlan.ExamFreqLimit = SIn.Int(row["ExamFreqLimit"].ToString());
            discountPlan.XrayFreqLimit = SIn.Int(row["XrayFreqLimit"].ToString());
            discountPlan.ProphyFreqLimit = SIn.Int(row["ProphyFreqLimit"].ToString());
            discountPlan.FluorideFreqLimit = SIn.Int(row["FluorideFreqLimit"].ToString());
            discountPlan.PerioFreqLimit = SIn.Int(row["PerioFreqLimit"].ToString());
            discountPlan.LimitedExamFreqLimit = SIn.Int(row["LimitedExamFreqLimit"].ToString());
            discountPlan.PAFreqLimit = SIn.Int(row["PAFreqLimit"].ToString());
            discountPlan.AnnualMax = SIn.Double(row["AnnualMax"].ToString());
            retVal.Add(discountPlan);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DiscountPlan> listDiscountPlans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DiscountPlan";
        var table = new DataTable(tableName);
        table.Columns.Add("DiscountPlanNum");
        table.Columns.Add("Description");
        table.Columns.Add("FeeSchedNum");
        table.Columns.Add("DefNum");
        table.Columns.Add("IsHidden");
        table.Columns.Add("PlanNote");
        table.Columns.Add("ExamFreqLimit");
        table.Columns.Add("XrayFreqLimit");
        table.Columns.Add("ProphyFreqLimit");
        table.Columns.Add("FluorideFreqLimit");
        table.Columns.Add("PerioFreqLimit");
        table.Columns.Add("LimitedExamFreqLimit");
        table.Columns.Add("PAFreqLimit");
        table.Columns.Add("AnnualMax");
        foreach (var discountPlan in listDiscountPlans)
            table.Rows.Add(SOut.Long(discountPlan.DiscountPlanNum), discountPlan.Description, SOut.Long(discountPlan.FeeSchedNum), SOut.Long(discountPlan.DefNum), SOut.Bool(discountPlan.IsHidden), discountPlan.PlanNote, SOut.Int(discountPlan.ExamFreqLimit), SOut.Int(discountPlan.XrayFreqLimit), SOut.Int(discountPlan.ProphyFreqLimit), SOut.Int(discountPlan.FluorideFreqLimit), SOut.Int(discountPlan.PerioFreqLimit), SOut.Int(discountPlan.LimitedExamFreqLimit), SOut.Int(discountPlan.PAFreqLimit), SOut.Double(discountPlan.AnnualMax));
        return table;
    }

    public static long Insert(DiscountPlan discountPlan)
    {
        return Insert(discountPlan, false);
    }

    public static long Insert(DiscountPlan discountPlan, bool useExistingPK)
    {
        var command = "INSERT INTO discountplan (";

        command += "Description,FeeSchedNum,DefNum,IsHidden,PlanNote,ExamFreqLimit,XrayFreqLimit,ProphyFreqLimit,FluorideFreqLimit,PerioFreqLimit,LimitedExamFreqLimit,PAFreqLimit,AnnualMax) VALUES(";

        command +=
            "'" + SOut.String(discountPlan.Description) + "',"
            + SOut.Long(discountPlan.FeeSchedNum) + ","
            + SOut.Long(discountPlan.DefNum) + ","
            + SOut.Bool(discountPlan.IsHidden) + ","
            + DbHelper.ParamChar + "paramPlanNote,"
            + SOut.Int(discountPlan.ExamFreqLimit) + ","
            + SOut.Int(discountPlan.XrayFreqLimit) + ","
            + SOut.Int(discountPlan.ProphyFreqLimit) + ","
            + SOut.Int(discountPlan.FluorideFreqLimit) + ","
            + SOut.Int(discountPlan.PerioFreqLimit) + ","
            + SOut.Int(discountPlan.LimitedExamFreqLimit) + ","
            + SOut.Int(discountPlan.PAFreqLimit) + ","
            + SOut.Double(discountPlan.AnnualMax) + ")";
        if (discountPlan.PlanNote == null) discountPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(discountPlan.PlanNote));
        {
            discountPlan.DiscountPlanNum = Db.NonQ(command, true, "DiscountPlanNum", "discountPlan", paramPlanNote);
        }
        return discountPlan.DiscountPlanNum;
    }

    public static long InsertNoCache(DiscountPlan discountPlan)
    {
        return InsertNoCache(discountPlan, false);
    }

    public static long InsertNoCache(DiscountPlan discountPlan, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO discountplan (";
        if (isRandomKeys || useExistingPK) command += "DiscountPlanNum,";
        command += "Description,FeeSchedNum,DefNum,IsHidden,PlanNote,ExamFreqLimit,XrayFreqLimit,ProphyFreqLimit,FluorideFreqLimit,PerioFreqLimit,LimitedExamFreqLimit,PAFreqLimit,AnnualMax) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(discountPlan.DiscountPlanNum) + ",";
        command +=
            "'" + SOut.String(discountPlan.Description) + "',"
            + SOut.Long(discountPlan.FeeSchedNum) + ","
            + SOut.Long(discountPlan.DefNum) + ","
            + SOut.Bool(discountPlan.IsHidden) + ","
            + DbHelper.ParamChar + "paramPlanNote,"
            + SOut.Int(discountPlan.ExamFreqLimit) + ","
            + SOut.Int(discountPlan.XrayFreqLimit) + ","
            + SOut.Int(discountPlan.ProphyFreqLimit) + ","
            + SOut.Int(discountPlan.FluorideFreqLimit) + ","
            + SOut.Int(discountPlan.PerioFreqLimit) + ","
            + SOut.Int(discountPlan.LimitedExamFreqLimit) + ","
            + SOut.Int(discountPlan.PAFreqLimit) + ","
            + SOut.Double(discountPlan.AnnualMax) + ")";
        if (discountPlan.PlanNote == null) discountPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(discountPlan.PlanNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPlanNote);
        else
            discountPlan.DiscountPlanNum = Db.NonQ(command, true, "DiscountPlanNum", "discountPlan", paramPlanNote);
        return discountPlan.DiscountPlanNum;
    }

    public static void Update(DiscountPlan discountPlan)
    {
        var command = "UPDATE discountplan SET "
                      + "Description         = '" + SOut.String(discountPlan.Description) + "', "
                      + "FeeSchedNum         =  " + SOut.Long(discountPlan.FeeSchedNum) + ", "
                      + "DefNum              =  " + SOut.Long(discountPlan.DefNum) + ", "
                      + "IsHidden            =  " + SOut.Bool(discountPlan.IsHidden) + ", "
                      + "PlanNote            =  " + DbHelper.ParamChar + "paramPlanNote, "
                      + "ExamFreqLimit       =  " + SOut.Int(discountPlan.ExamFreqLimit) + ", "
                      + "XrayFreqLimit       =  " + SOut.Int(discountPlan.XrayFreqLimit) + ", "
                      + "ProphyFreqLimit     =  " + SOut.Int(discountPlan.ProphyFreqLimit) + ", "
                      + "FluorideFreqLimit   =  " + SOut.Int(discountPlan.FluorideFreqLimit) + ", "
                      + "PerioFreqLimit      =  " + SOut.Int(discountPlan.PerioFreqLimit) + ", "
                      + "LimitedExamFreqLimit=  " + SOut.Int(discountPlan.LimitedExamFreqLimit) + ", "
                      + "PAFreqLimit         =  " + SOut.Int(discountPlan.PAFreqLimit) + ", "
                      + "AnnualMax           =  " + SOut.Double(discountPlan.AnnualMax) + " "
                      + "WHERE DiscountPlanNum = " + SOut.Long(discountPlan.DiscountPlanNum);
        if (discountPlan.PlanNote == null) discountPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(discountPlan.PlanNote));
        Db.NonQ(command, paramPlanNote);
    }

    public static bool Update(DiscountPlan discountPlan, DiscountPlan oldDiscountPlan)
    {
        var command = "";
        if (discountPlan.Description != oldDiscountPlan.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(discountPlan.Description) + "'";
        }

        if (discountPlan.FeeSchedNum != oldDiscountPlan.FeeSchedNum)
        {
            if (command != "") command += ",";
            command += "FeeSchedNum = " + SOut.Long(discountPlan.FeeSchedNum) + "";
        }

        if (discountPlan.DefNum != oldDiscountPlan.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(discountPlan.DefNum) + "";
        }

        if (discountPlan.IsHidden != oldDiscountPlan.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(discountPlan.IsHidden) + "";
        }

        if (discountPlan.PlanNote != oldDiscountPlan.PlanNote)
        {
            if (command != "") command += ",";
            command += "PlanNote = " + DbHelper.ParamChar + "paramPlanNote";
        }

        if (discountPlan.ExamFreqLimit != oldDiscountPlan.ExamFreqLimit)
        {
            if (command != "") command += ",";
            command += "ExamFreqLimit = " + SOut.Int(discountPlan.ExamFreqLimit) + "";
        }

        if (discountPlan.XrayFreqLimit != oldDiscountPlan.XrayFreqLimit)
        {
            if (command != "") command += ",";
            command += "XrayFreqLimit = " + SOut.Int(discountPlan.XrayFreqLimit) + "";
        }

        if (discountPlan.ProphyFreqLimit != oldDiscountPlan.ProphyFreqLimit)
        {
            if (command != "") command += ",";
            command += "ProphyFreqLimit = " + SOut.Int(discountPlan.ProphyFreqLimit) + "";
        }

        if (discountPlan.FluorideFreqLimit != oldDiscountPlan.FluorideFreqLimit)
        {
            if (command != "") command += ",";
            command += "FluorideFreqLimit = " + SOut.Int(discountPlan.FluorideFreqLimit) + "";
        }

        if (discountPlan.PerioFreqLimit != oldDiscountPlan.PerioFreqLimit)
        {
            if (command != "") command += ",";
            command += "PerioFreqLimit = " + SOut.Int(discountPlan.PerioFreqLimit) + "";
        }

        if (discountPlan.LimitedExamFreqLimit != oldDiscountPlan.LimitedExamFreqLimit)
        {
            if (command != "") command += ",";
            command += "LimitedExamFreqLimit = " + SOut.Int(discountPlan.LimitedExamFreqLimit) + "";
        }

        if (discountPlan.PAFreqLimit != oldDiscountPlan.PAFreqLimit)
        {
            if (command != "") command += ",";
            command += "PAFreqLimit = " + SOut.Int(discountPlan.PAFreqLimit) + "";
        }

        if (discountPlan.AnnualMax != oldDiscountPlan.AnnualMax)
        {
            if (command != "") command += ",";
            command += "AnnualMax = " + SOut.Double(discountPlan.AnnualMax) + "";
        }

        if (command == "") return false;
        if (discountPlan.PlanNote == null) discountPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(discountPlan.PlanNote));
        command = "UPDATE discountplan SET " + command
                                             + " WHERE DiscountPlanNum = " + SOut.Long(discountPlan.DiscountPlanNum);
        Db.NonQ(command, paramPlanNote);
        return true;
    }

    public static bool UpdateComparison(DiscountPlan discountPlan, DiscountPlan oldDiscountPlan)
    {
        if (discountPlan.Description != oldDiscountPlan.Description) return true;
        if (discountPlan.FeeSchedNum != oldDiscountPlan.FeeSchedNum) return true;
        if (discountPlan.DefNum != oldDiscountPlan.DefNum) return true;
        if (discountPlan.IsHidden != oldDiscountPlan.IsHidden) return true;
        if (discountPlan.PlanNote != oldDiscountPlan.PlanNote) return true;
        if (discountPlan.ExamFreqLimit != oldDiscountPlan.ExamFreqLimit) return true;
        if (discountPlan.XrayFreqLimit != oldDiscountPlan.XrayFreqLimit) return true;
        if (discountPlan.ProphyFreqLimit != oldDiscountPlan.ProphyFreqLimit) return true;
        if (discountPlan.FluorideFreqLimit != oldDiscountPlan.FluorideFreqLimit) return true;
        if (discountPlan.PerioFreqLimit != oldDiscountPlan.PerioFreqLimit) return true;
        if (discountPlan.LimitedExamFreqLimit != oldDiscountPlan.LimitedExamFreqLimit) return true;
        if (discountPlan.PAFreqLimit != oldDiscountPlan.PAFreqLimit) return true;
        if (discountPlan.AnnualMax != oldDiscountPlan.AnnualMax) return true;
        return false;
    }

    public static void Delete(long discountPlanNum)
    {
        var command = "DELETE FROM discountplan "
                      + "WHERE DiscountPlanNum = " + SOut.Long(discountPlanNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDiscountPlanNums)
    {
        if (listDiscountPlanNums == null || listDiscountPlanNums.Count == 0) return;
        var command = "DELETE FROM discountplan "
                      + "WHERE DiscountPlanNum IN(" + string.Join(",", listDiscountPlanNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}