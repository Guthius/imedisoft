using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsPlanCrud
{
    public static InsPlan SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsPlan> TableToList(DataTable table)
    {
        var retVal = new List<InsPlan>();
        foreach (DataRow row in table.Rows)
        {
            var insPlan = new InsPlan
            {
                PlanNum = SIn.Long(row["PlanNum"].ToString()),
                GroupName = SIn.String(row["GroupName"].ToString()),
                GroupNum = SIn.String(row["GroupNum"].ToString()),
                PlanNote = SIn.String(row["PlanNote"].ToString()),
                FeeSched = SIn.Long(row["FeeSched"].ToString()),
                PlanType = SIn.String(row["PlanType"].ToString()),
                ClaimFormNum = SIn.Long(row["ClaimFormNum"].ToString()),
                UseAltCode = SIn.Bool(row["UseAltCode"].ToString()),
                ClaimsUseUCR = SIn.Bool(row["ClaimsUseUCR"].ToString()),
                CopayFeeSched = SIn.Long(row["CopayFeeSched"].ToString()),
                EmployerNum = SIn.Long(row["EmployerNum"].ToString()),
                CarrierNum = SIn.Long(row["CarrierNum"].ToString()),
                AllowedFeeSched = SIn.Long(row["AllowedFeeSched"].ToString()),
                TrojanID = SIn.String(row["TrojanID"].ToString()),
                DivisionNo = SIn.String(row["DivisionNo"].ToString()),
                IsMedical = SIn.Bool(row["IsMedical"].ToString()),
                FilingCode = SIn.Long(row["FilingCode"].ToString()),
                DentaideCardSequence = SIn.Byte(row["DentaideCardSequence"].ToString()),
                ShowBaseUnits = SIn.Bool(row["ShowBaseUnits"].ToString()),
                CodeSubstNone = SIn.Bool(row["CodeSubstNone"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                MonthRenew = SIn.Byte(row["MonthRenew"].ToString()),
                FilingCodeSubtype = SIn.Long(row["FilingCodeSubtype"].ToString()),
                CanadianPlanFlag = SIn.String(row["CanadianPlanFlag"].ToString()),
                CanadianDiagnosticCode = SIn.String(row["CanadianDiagnosticCode"].ToString()),
                CanadianInstitutionCode = SIn.String(row["CanadianInstitutionCode"].ToString()),
                RxBIN = SIn.String(row["RxBIN"].ToString()),
                CobRule = (EnumCobRule) SIn.Int(row["CobRule"].ToString()),
                SopCode = SIn.String(row["SopCode"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                HideFromVerifyList = SIn.Bool(row["HideFromVerifyList"].ToString()),
                OrthoType = (OrthoClaimType) SIn.Int(row["OrthoType"].ToString()),
                OrthoAutoProcFreq = (OrthoAutoProcFrequency) SIn.Int(row["OrthoAutoProcFreq"].ToString()),
                OrthoAutoProcCodeNumOverride = SIn.Long(row["OrthoAutoProcCodeNumOverride"].ToString()),
                OrthoAutoFeeBilled = SIn.Double(row["OrthoAutoFeeBilled"].ToString()),
                OrthoAutoClaimDaysWait = SIn.Int(row["OrthoAutoClaimDaysWait"].ToString()),
                BillingType = SIn.Long(row["BillingType"].ToString()),
                HasPpoSubstWriteoffs = SIn.Bool(row["HasPpoSubstWriteoffs"].ToString()),
                ExclusionFeeRule = (ExclusionRule) SIn.Int(row["ExclusionFeeRule"].ToString()),
                ManualFeeSchedNum = SIn.Long(row["ManualFeeSchedNum"].ToString()),
                IsBlueBookEnabled = SIn.Bool(row["IsBlueBookEnabled"].ToString()),
                InsPlansZeroWriteOffsOnAnnualMaxOverride = (YN) SIn.Int(row["InsPlansZeroWriteOffsOnAnnualMaxOverride"].ToString()),
                InsPlansZeroWriteOffsOnFreqOrAgingOverride = (YN) SIn.Int(row["InsPlansZeroWriteOffsOnFreqOrAgingOverride"].ToString()),
                PerVisitPatAmount = SIn.Double(row["PerVisitPatAmount"].ToString()),
                PerVisitInsAmount = SIn.Double(row["PerVisitInsAmount"].ToString())
            };
            retVal.Add(insPlan);
        }

        return retVal;
    }

    public static long Insert(InsPlan insPlan)
    {
        var command = "INSERT INTO insplan (";

        command += "GroupName,GroupNum,PlanNote,FeeSched,PlanType,ClaimFormNum,UseAltCode,ClaimsUseUCR,CopayFeeSched,EmployerNum,CarrierNum,AllowedFeeSched,TrojanID,DivisionNo,IsMedical,FilingCode,DentaideCardSequence,ShowBaseUnits,CodeSubstNone,IsHidden,MonthRenew,FilingCodeSubtype,CanadianPlanFlag,CanadianDiagnosticCode,CanadianInstitutionCode,RxBIN,CobRule,SopCode,SecUserNumEntry,SecDateEntry,HideFromVerifyList,OrthoType,OrthoAutoProcFreq,OrthoAutoProcCodeNumOverride,OrthoAutoFeeBilled,OrthoAutoClaimDaysWait,BillingType,HasPpoSubstWriteoffs,ExclusionFeeRule,ManualFeeSchedNum,IsBlueBookEnabled,InsPlansZeroWriteOffsOnAnnualMaxOverride,InsPlansZeroWriteOffsOnFreqOrAgingOverride,PerVisitPatAmount,PerVisitInsAmount) VALUES(";

        command +=
            "'" + SOut.String(insPlan.GroupName) + "',"
            + "'" + SOut.String(insPlan.GroupNum) + "',"
            + DbHelper.ParamChar + "paramPlanNote,"
            + SOut.Long(insPlan.FeeSched) + ","
            + "'" + SOut.String(insPlan.PlanType) + "',"
            + SOut.Long(insPlan.ClaimFormNum) + ","
            + SOut.Bool(insPlan.UseAltCode) + ","
            + SOut.Bool(insPlan.ClaimsUseUCR) + ","
            + SOut.Long(insPlan.CopayFeeSched) + ","
            + SOut.Long(insPlan.EmployerNum) + ","
            + SOut.Long(insPlan.CarrierNum) + ","
            + SOut.Long(insPlan.AllowedFeeSched) + ","
            + "'" + SOut.String(insPlan.TrojanID) + "',"
            + "'" + SOut.String(insPlan.DivisionNo) + "',"
            + SOut.Bool(insPlan.IsMedical) + ","
            + SOut.Long(insPlan.FilingCode) + ","
            + SOut.Byte(insPlan.DentaideCardSequence) + ","
            + SOut.Bool(insPlan.ShowBaseUnits) + ","
            + SOut.Bool(insPlan.CodeSubstNone) + ","
            + SOut.Bool(insPlan.IsHidden) + ","
            + SOut.Byte(insPlan.MonthRenew) + ","
            + SOut.Long(insPlan.FilingCodeSubtype) + ","
            + "'" + SOut.String(insPlan.CanadianPlanFlag) + "',"
            + "'" + SOut.String(insPlan.CanadianDiagnosticCode) + "',"
            + "'" + SOut.String(insPlan.CanadianInstitutionCode) + "',"
            + "'" + SOut.String(insPlan.RxBIN) + "',"
            + SOut.Int((int) insPlan.CobRule) + ","
            + "'" + SOut.String(insPlan.SopCode) + "',"
            + SOut.Long(insPlan.SecUserNumEntry) + ","
            + "NOW()" + ","
            //SecDateTEdit can only be set by MySQL
            + SOut.Bool(insPlan.HideFromVerifyList) + ","
            + SOut.Int((int) insPlan.OrthoType) + ","
            + SOut.Int((int) insPlan.OrthoAutoProcFreq) + ","
            + SOut.Long(insPlan.OrthoAutoProcCodeNumOverride) + ","
            + SOut.Double(insPlan.OrthoAutoFeeBilled) + ","
            + SOut.Int(insPlan.OrthoAutoClaimDaysWait) + ","
            + SOut.Long(insPlan.BillingType) + ","
            + SOut.Bool(insPlan.HasPpoSubstWriteoffs) + ","
            + SOut.Int((int) insPlan.ExclusionFeeRule) + ","
            + SOut.Long(insPlan.ManualFeeSchedNum) + ","
            + SOut.Bool(insPlan.IsBlueBookEnabled) + ","
            + SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride) + ","
            + SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride) + ","
            + SOut.Double(insPlan.PerVisitPatAmount) + ","
            + SOut.Double(insPlan.PerVisitInsAmount) + ")";
        if (insPlan.PlanNote == null) insPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", SOut.StringParam(insPlan.PlanNote));
        {
            insPlan.PlanNum = Db.NonQ(command, true, "PlanNum", "insPlan", paramPlanNote);
        }
        return insPlan.PlanNum;
    }

    public static void Update(InsPlan insPlan, InsPlan oldInsPlan)
    {
        var command = "";
        if (insPlan.GroupName != oldInsPlan.GroupName)
        {
            if (command != "") command += ",";
            command += "GroupName = '" + SOut.String(insPlan.GroupName) + "'";
        }

        if (insPlan.GroupNum != oldInsPlan.GroupNum)
        {
            if (command != "") command += ",";
            command += "GroupNum = '" + SOut.String(insPlan.GroupNum) + "'";
        }

        if (insPlan.PlanNote != oldInsPlan.PlanNote)
        {
            if (command != "") command += ",";
            command += "PlanNote = " + DbHelper.ParamChar + "paramPlanNote";
        }

        if (insPlan.FeeSched != oldInsPlan.FeeSched)
        {
            if (command != "") command += ",";
            command += "FeeSched = " + SOut.Long(insPlan.FeeSched) + "";
        }

        if (insPlan.PlanType != oldInsPlan.PlanType)
        {
            if (command != "") command += ",";
            command += "PlanType = '" + SOut.String(insPlan.PlanType) + "'";
        }

        if (insPlan.ClaimFormNum != oldInsPlan.ClaimFormNum)
        {
            if (command != "") command += ",";
            command += "ClaimFormNum = " + SOut.Long(insPlan.ClaimFormNum) + "";
        }

        if (insPlan.UseAltCode != oldInsPlan.UseAltCode)
        {
            if (command != "") command += ",";
            command += "UseAltCode = " + SOut.Bool(insPlan.UseAltCode) + "";
        }

        if (insPlan.ClaimsUseUCR != oldInsPlan.ClaimsUseUCR)
        {
            if (command != "") command += ",";
            command += "ClaimsUseUCR = " + SOut.Bool(insPlan.ClaimsUseUCR) + "";
        }

        if (insPlan.CopayFeeSched != oldInsPlan.CopayFeeSched)
        {
            if (command != "") command += ",";
            command += "CopayFeeSched = " + SOut.Long(insPlan.CopayFeeSched) + "";
        }

        if (insPlan.EmployerNum != oldInsPlan.EmployerNum)
        {
            if (command != "") command += ",";
            command += "EmployerNum = " + SOut.Long(insPlan.EmployerNum) + "";
        }

        if (insPlan.CarrierNum != oldInsPlan.CarrierNum)
        {
            if (command != "") command += ",";
            command += "CarrierNum = " + SOut.Long(insPlan.CarrierNum) + "";
        }

        if (insPlan.AllowedFeeSched != oldInsPlan.AllowedFeeSched)
        {
            if (command != "") command += ",";
            command += "AllowedFeeSched = " + SOut.Long(insPlan.AllowedFeeSched) + "";
        }

        if (insPlan.TrojanID != oldInsPlan.TrojanID)
        {
            if (command != "") command += ",";
            command += "TrojanID = '" + SOut.String(insPlan.TrojanID) + "'";
        }

        if (insPlan.DivisionNo != oldInsPlan.DivisionNo)
        {
            if (command != "") command += ",";
            command += "DivisionNo = '" + SOut.String(insPlan.DivisionNo) + "'";
        }

        if (insPlan.IsMedical != oldInsPlan.IsMedical)
        {
            if (command != "") command += ",";
            command += "IsMedical = " + SOut.Bool(insPlan.IsMedical) + "";
        }

        if (insPlan.FilingCode != oldInsPlan.FilingCode)
        {
            if (command != "") command += ",";
            command += "FilingCode = " + SOut.Long(insPlan.FilingCode) + "";
        }

        if (insPlan.DentaideCardSequence != oldInsPlan.DentaideCardSequence)
        {
            if (command != "") command += ",";
            command += "DentaideCardSequence = " + SOut.Byte(insPlan.DentaideCardSequence) + "";
        }

        if (insPlan.ShowBaseUnits != oldInsPlan.ShowBaseUnits)
        {
            if (command != "") command += ",";
            command += "ShowBaseUnits = " + SOut.Bool(insPlan.ShowBaseUnits) + "";
        }

        if (insPlan.CodeSubstNone != oldInsPlan.CodeSubstNone)
        {
            if (command != "") command += ",";
            command += "CodeSubstNone = " + SOut.Bool(insPlan.CodeSubstNone) + "";
        }

        if (insPlan.IsHidden != oldInsPlan.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(insPlan.IsHidden) + "";
        }

        if (insPlan.MonthRenew != oldInsPlan.MonthRenew)
        {
            if (command != "") command += ",";
            command += "MonthRenew = " + SOut.Byte(insPlan.MonthRenew) + "";
        }

        if (insPlan.FilingCodeSubtype != oldInsPlan.FilingCodeSubtype)
        {
            if (command != "") command += ",";
            command += "FilingCodeSubtype = " + SOut.Long(insPlan.FilingCodeSubtype) + "";
        }

        if (insPlan.CanadianPlanFlag != oldInsPlan.CanadianPlanFlag)
        {
            if (command != "") command += ",";
            command += "CanadianPlanFlag = '" + SOut.String(insPlan.CanadianPlanFlag) + "'";
        }

        if (insPlan.CanadianDiagnosticCode != oldInsPlan.CanadianDiagnosticCode)
        {
            if (command != "") command += ",";
            command += "CanadianDiagnosticCode = '" + SOut.String(insPlan.CanadianDiagnosticCode) + "'";
        }

        if (insPlan.CanadianInstitutionCode != oldInsPlan.CanadianInstitutionCode)
        {
            if (command != "") command += ",";
            command += "CanadianInstitutionCode = '" + SOut.String(insPlan.CanadianInstitutionCode) + "'";
        }

        if (insPlan.RxBIN != oldInsPlan.RxBIN)
        {
            if (command != "") command += ",";
            command += "RxBIN = '" + SOut.String(insPlan.RxBIN) + "'";
        }

        if (insPlan.CobRule != oldInsPlan.CobRule)
        {
            if (command != "") command += ",";
            command += "CobRule = " + SOut.Int((int) insPlan.CobRule) + "";
        }

        if (insPlan.SopCode != oldInsPlan.SopCode)
        {
            if (command != "") command += ",";
            command += "SopCode = '" + SOut.String(insPlan.SopCode) + "'";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (insPlan.HideFromVerifyList != oldInsPlan.HideFromVerifyList)
        {
            if (command != "") command += ",";
            command += "HideFromVerifyList = " + SOut.Bool(insPlan.HideFromVerifyList) + "";
        }

        if (insPlan.OrthoType != oldInsPlan.OrthoType)
        {
            if (command != "") command += ",";
            command += "OrthoType = " + SOut.Int((int) insPlan.OrthoType) + "";
        }

        if (insPlan.OrthoAutoProcFreq != oldInsPlan.OrthoAutoProcFreq)
        {
            if (command != "") command += ",";
            command += "OrthoAutoProcFreq = " + SOut.Int((int) insPlan.OrthoAutoProcFreq) + "";
        }

        if (insPlan.OrthoAutoProcCodeNumOverride != oldInsPlan.OrthoAutoProcCodeNumOverride)
        {
            if (command != "") command += ",";
            command += "OrthoAutoProcCodeNumOverride = " + SOut.Long(insPlan.OrthoAutoProcCodeNumOverride) + "";
        }

        if (insPlan.OrthoAutoFeeBilled != oldInsPlan.OrthoAutoFeeBilled)
        {
            if (command != "") command += ",";
            command += "OrthoAutoFeeBilled = " + SOut.Double(insPlan.OrthoAutoFeeBilled) + "";
        }

        if (insPlan.OrthoAutoClaimDaysWait != oldInsPlan.OrthoAutoClaimDaysWait)
        {
            if (command != "") command += ",";
            command += "OrthoAutoClaimDaysWait = " + SOut.Int(insPlan.OrthoAutoClaimDaysWait) + "";
        }

        if (insPlan.BillingType != oldInsPlan.BillingType)
        {
            if (command != "") command += ",";
            command += "BillingType = " + SOut.Long(insPlan.BillingType) + "";
        }

        if (insPlan.HasPpoSubstWriteoffs != oldInsPlan.HasPpoSubstWriteoffs)
        {
            if (command != "") command += ",";
            command += "HasPpoSubstWriteoffs = " + SOut.Bool(insPlan.HasPpoSubstWriteoffs) + "";
        }

        if (insPlan.ExclusionFeeRule != oldInsPlan.ExclusionFeeRule)
        {
            if (command != "") command += ",";
            command += "ExclusionFeeRule = " + SOut.Int((int) insPlan.ExclusionFeeRule) + "";
        }

        if (insPlan.ManualFeeSchedNum != oldInsPlan.ManualFeeSchedNum)
        {
            if (command != "") command += ",";
            command += "ManualFeeSchedNum = " + SOut.Long(insPlan.ManualFeeSchedNum) + "";
        }

        if (insPlan.IsBlueBookEnabled != oldInsPlan.IsBlueBookEnabled)
        {
            if (command != "") command += ",";
            command += "IsBlueBookEnabled = " + SOut.Bool(insPlan.IsBlueBookEnabled) + "";
        }

        if (insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride != oldInsPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride)
        {
            if (command != "") command += ",";
            command += "InsPlansZeroWriteOffsOnAnnualMaxOverride = " + SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride) + "";
        }

        if (insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride != oldInsPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride)
        {
            if (command != "") command += ",";
            command += "InsPlansZeroWriteOffsOnFreqOrAgingOverride = " + SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride) + "";
        }

        if (insPlan.PerVisitPatAmount != oldInsPlan.PerVisitPatAmount)
        {
            if (command != "") command += ",";
            command += "PerVisitPatAmount = " + SOut.Double(insPlan.PerVisitPatAmount) + "";
        }

        if (insPlan.PerVisitInsAmount != oldInsPlan.PerVisitInsAmount)
        {
            if (command != "") command += ",";
            command += "PerVisitInsAmount = " + SOut.Double(insPlan.PerVisitInsAmount) + "";
        }

        if (command == "") return;
        if (insPlan.PlanNote == null) insPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", SOut.StringParam(insPlan.PlanNote));
        command = "UPDATE insplan SET " + command
                                        + " WHERE PlanNum = " + SOut.Long(insPlan.PlanNum);
        Db.NonQ(command, paramPlanNote);
    }

    public static bool UpdateComparison(InsPlan insPlan, InsPlan oldInsPlan)
    {
        if (insPlan.GroupName != oldInsPlan.GroupName) return true;
        if (insPlan.GroupNum != oldInsPlan.GroupNum) return true;
        if (insPlan.PlanNote != oldInsPlan.PlanNote) return true;
        if (insPlan.FeeSched != oldInsPlan.FeeSched) return true;
        if (insPlan.PlanType != oldInsPlan.PlanType) return true;
        if (insPlan.ClaimFormNum != oldInsPlan.ClaimFormNum) return true;
        if (insPlan.UseAltCode != oldInsPlan.UseAltCode) return true;
        if (insPlan.ClaimsUseUCR != oldInsPlan.ClaimsUseUCR) return true;
        if (insPlan.CopayFeeSched != oldInsPlan.CopayFeeSched) return true;
        if (insPlan.EmployerNum != oldInsPlan.EmployerNum) return true;
        if (insPlan.CarrierNum != oldInsPlan.CarrierNum) return true;
        if (insPlan.AllowedFeeSched != oldInsPlan.AllowedFeeSched) return true;
        if (insPlan.TrojanID != oldInsPlan.TrojanID) return true;
        if (insPlan.DivisionNo != oldInsPlan.DivisionNo) return true;
        if (insPlan.IsMedical != oldInsPlan.IsMedical) return true;
        if (insPlan.FilingCode != oldInsPlan.FilingCode) return true;
        if (insPlan.DentaideCardSequence != oldInsPlan.DentaideCardSequence) return true;
        if (insPlan.ShowBaseUnits != oldInsPlan.ShowBaseUnits) return true;
        if (insPlan.CodeSubstNone != oldInsPlan.CodeSubstNone) return true;
        if (insPlan.IsHidden != oldInsPlan.IsHidden) return true;
        if (insPlan.MonthRenew != oldInsPlan.MonthRenew) return true;
        if (insPlan.FilingCodeSubtype != oldInsPlan.FilingCodeSubtype) return true;
        if (insPlan.CanadianPlanFlag != oldInsPlan.CanadianPlanFlag) return true;
        if (insPlan.CanadianDiagnosticCode != oldInsPlan.CanadianDiagnosticCode) return true;
        if (insPlan.CanadianInstitutionCode != oldInsPlan.CanadianInstitutionCode) return true;
        if (insPlan.RxBIN != oldInsPlan.RxBIN) return true;
        if (insPlan.CobRule != oldInsPlan.CobRule) return true;
        if (insPlan.SopCode != oldInsPlan.SopCode) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (insPlan.HideFromVerifyList != oldInsPlan.HideFromVerifyList) return true;
        if (insPlan.OrthoType != oldInsPlan.OrthoType) return true;
        if (insPlan.OrthoAutoProcFreq != oldInsPlan.OrthoAutoProcFreq) return true;
        if (insPlan.OrthoAutoProcCodeNumOverride != oldInsPlan.OrthoAutoProcCodeNumOverride) return true;
        if (insPlan.OrthoAutoFeeBilled != oldInsPlan.OrthoAutoFeeBilled) return true;
        if (insPlan.OrthoAutoClaimDaysWait != oldInsPlan.OrthoAutoClaimDaysWait) return true;
        if (insPlan.BillingType != oldInsPlan.BillingType) return true;
        if (insPlan.HasPpoSubstWriteoffs != oldInsPlan.HasPpoSubstWriteoffs) return true;
        if (insPlan.ExclusionFeeRule != oldInsPlan.ExclusionFeeRule) return true;
        if (insPlan.ManualFeeSchedNum != oldInsPlan.ManualFeeSchedNum) return true;
        if (insPlan.IsBlueBookEnabled != oldInsPlan.IsBlueBookEnabled) return true;
        if (insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride != oldInsPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride) return true;
        if (insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride != oldInsPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride) return true;
        if (insPlan.PerVisitPatAmount != oldInsPlan.PerVisitPatAmount) return true;
        if (insPlan.PerVisitInsAmount != oldInsPlan.PerVisitInsAmount) return true;
        return false;
    }

    public static void ClearFkey(long planNum)
    {
        if (planNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(planNum) + " AND PermType IN (65)";
        Db.NonQ(command);
    }
}