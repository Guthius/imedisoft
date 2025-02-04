using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<DiscountPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DiscountPlan> TableToList(DataTable table)
    {
        var retVal = new List<DiscountPlan>();
        foreach (DataRow row in table.Rows)
        {
            var discountPlan = new DiscountPlan
            {
                DiscountPlanNum = SIn.Long(row["DiscountPlanNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString()),
                DefNum = SIn.Long(row["DefNum"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                PlanNote = SIn.String(row["PlanNote"].ToString()),
                ExamFreqLimit = SIn.Int(row["ExamFreqLimit"].ToString()),
                XrayFreqLimit = SIn.Int(row["XrayFreqLimit"].ToString()),
                ProphyFreqLimit = SIn.Int(row["ProphyFreqLimit"].ToString()),
                FluorideFreqLimit = SIn.Int(row["FluorideFreqLimit"].ToString()),
                PerioFreqLimit = SIn.Int(row["PerioFreqLimit"].ToString()),
                LimitedExamFreqLimit = SIn.Int(row["LimitedExamFreqLimit"].ToString()),
                PAFreqLimit = SIn.Int(row["PAFreqLimit"].ToString()),
                AnnualMax = SIn.Double(row["AnnualMax"].ToString())
            };
            retVal.Add(discountPlan);
        }

        return retVal;
    }

    public static void Insert(DiscountPlan discountPlan)
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
        var paramPlanNote = new OdSqlParameter("paramPlanNote", SOut.StringParam(discountPlan.PlanNote));
        {
            discountPlan.DiscountPlanNum = Db.NonQ(command, true, "DiscountPlanNum", "discountPlan", paramPlanNote);
        }
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
        var paramPlanNote = new OdSqlParameter("paramPlanNote", SOut.StringParam(discountPlan.PlanNote));
        Db.NonQ(command, paramPlanNote);
    }

    public static void Delete(long discountPlanNum)
    {
        var command = "DELETE FROM discountplan "
                      + "WHERE DiscountPlanNum = " + SOut.Long(discountPlanNum);
        Db.NonQ(command);
    }
}