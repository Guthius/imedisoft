using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class BenefitCrud
{
    public static List<Benefit> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Benefit> TableToList(DataTable table)
    {
        var retVal = new List<Benefit>();
        Benefit benefit;
        foreach (DataRow row in table.Rows)
        {
            benefit = new Benefit();
            benefit.BenefitNum = SIn.Long(row["BenefitNum"].ToString());
            benefit.PlanNum = SIn.Long(row["PlanNum"].ToString());
            benefit.PatPlanNum = SIn.Long(row["PatPlanNum"].ToString());
            benefit.CovCatNum = SIn.Long(row["CovCatNum"].ToString());
            benefit.BenefitType = (InsBenefitType) SIn.Int(row["BenefitType"].ToString());
            benefit.Percent = SIn.Int(row["Percent"].ToString());
            benefit.MonetaryAmt = SIn.Double(row["MonetaryAmt"].ToString());
            benefit.TimePeriod = (BenefitTimePeriod) SIn.Int(row["TimePeriod"].ToString());
            benefit.QuantityQualifier = (BenefitQuantity) SIn.Int(row["QuantityQualifier"].ToString());
            benefit.Quantity = SIn.Byte(row["Quantity"].ToString());
            benefit.CodeNum = SIn.Long(row["CodeNum"].ToString());
            benefit.CoverageLevel = (BenefitCoverageLevel) SIn.Int(row["CoverageLevel"].ToString());
            benefit.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            benefit.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            benefit.CodeGroupNum = SIn.Long(row["CodeGroupNum"].ToString());
            benefit.TreatArea = (TreatmentArea) SIn.Int(row["TreatArea"].ToString());
            retVal.Add(benefit);
        }

        return retVal;
    }

    public static long Insert(Benefit benefit)
    {
        var command = "INSERT INTO benefit (";

        command += "PlanNum,PatPlanNum,CovCatNum,BenefitType,Percent,MonetaryAmt,TimePeriod,QuantityQualifier,Quantity,CodeNum,CoverageLevel,SecDateTEntry,CodeGroupNum,TreatArea) VALUES(";

        command +=
            SOut.Long(benefit.PlanNum) + ","
                                       + SOut.Long(benefit.PatPlanNum) + ","
                                       + SOut.Long(benefit.CovCatNum) + ","
                                       + SOut.Int((int) benefit.BenefitType) + ","
                                       + SOut.Int(benefit.Percent) + ","
                                       + SOut.Double(benefit.MonetaryAmt) + ","
                                       + SOut.Int((int) benefit.TimePeriod) + ","
                                       + SOut.Int((int) benefit.QuantityQualifier) + ","
                                       + SOut.Byte(benefit.Quantity) + ","
                                       + SOut.Long(benefit.CodeNum) + ","
                                       + SOut.Int((int) benefit.CoverageLevel) + ","
                                       + DbHelper.Now() + ","
                                       //SecDateTEdit can only be set by MySQL
                                       + SOut.Long(benefit.CodeGroupNum) + ","
                                       + SOut.Int((int) benefit.TreatArea) + ")";
        {
            benefit.BenefitNum = Db.NonQ(command, true, "BenefitNum", "benefit");
        }
        return benefit.BenefitNum;
    }

    public static void Update(Benefit benefit, Benefit oldBenefit)
    {
        var command = "";
        if (benefit.PlanNum != oldBenefit.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(benefit.PlanNum) + "";
        }

        if (benefit.PatPlanNum != oldBenefit.PatPlanNum)
        {
            if (command != "") command += ",";
            command += "PatPlanNum = " + SOut.Long(benefit.PatPlanNum) + "";
        }

        if (benefit.CovCatNum != oldBenefit.CovCatNum)
        {
            if (command != "") command += ",";
            command += "CovCatNum = " + SOut.Long(benefit.CovCatNum) + "";
        }

        if (benefit.BenefitType != oldBenefit.BenefitType)
        {
            if (command != "") command += ",";
            command += "BenefitType = " + SOut.Int((int) benefit.BenefitType) + "";
        }

        if (benefit.Percent != oldBenefit.Percent)
        {
            if (command != "") command += ",";
            command += "Percent = " + SOut.Int(benefit.Percent) + "";
        }

        if (benefit.MonetaryAmt != oldBenefit.MonetaryAmt)
        {
            if (command != "") command += ",";
            command += "MonetaryAmt = " + SOut.Double(benefit.MonetaryAmt) + "";
        }

        if (benefit.TimePeriod != oldBenefit.TimePeriod)
        {
            if (command != "") command += ",";
            command += "TimePeriod = " + SOut.Int((int) benefit.TimePeriod) + "";
        }

        if (benefit.QuantityQualifier != oldBenefit.QuantityQualifier)
        {
            if (command != "") command += ",";
            command += "QuantityQualifier = " + SOut.Int((int) benefit.QuantityQualifier) + "";
        }

        if (benefit.Quantity != oldBenefit.Quantity)
        {
            if (command != "") command += ",";
            command += "Quantity = " + SOut.Byte(benefit.Quantity) + "";
        }

        if (benefit.CodeNum != oldBenefit.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(benefit.CodeNum) + "";
        }

        if (benefit.CoverageLevel != oldBenefit.CoverageLevel)
        {
            if (command != "") command += ",";
            command += "CoverageLevel = " + SOut.Int((int) benefit.CoverageLevel) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (benefit.CodeGroupNum != oldBenefit.CodeGroupNum)
        {
            if (command != "") command += ",";
            command += "CodeGroupNum = " + SOut.Long(benefit.CodeGroupNum) + "";
        }

        if (benefit.TreatArea != oldBenefit.TreatArea)
        {
            if (command != "") command += ",";
            command += "TreatArea = " + SOut.Int((int) benefit.TreatArea) + "";
        }

        if (command == "") return;
        command = "UPDATE benefit SET " + command
                                        + " WHERE BenefitNum = " + SOut.Long(benefit.BenefitNum);
        Db.NonQ(command);
    }
}