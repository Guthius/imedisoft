#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsPlanCrud
{
    public static InsPlan SelectOne(long planNum)
    {
        var command = "SELECT * FROM insplan "
                      + "WHERE PlanNum = " + SOut.Long(planNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        InsPlan insPlan;
        foreach (DataRow row in table.Rows)
        {
            insPlan = new InsPlan();
            insPlan.PlanNum = SIn.Long(row["PlanNum"].ToString());
            insPlan.GroupName = SIn.String(row["GroupName"].ToString());
            insPlan.GroupNum = SIn.String(row["GroupNum"].ToString());
            insPlan.PlanNote = SIn.String(row["PlanNote"].ToString());
            insPlan.FeeSched = SIn.Long(row["FeeSched"].ToString());
            insPlan.PlanType = SIn.String(row["PlanType"].ToString());
            insPlan.ClaimFormNum = SIn.Long(row["ClaimFormNum"].ToString());
            insPlan.UseAltCode = SIn.Bool(row["UseAltCode"].ToString());
            insPlan.ClaimsUseUCR = SIn.Bool(row["ClaimsUseUCR"].ToString());
            insPlan.CopayFeeSched = SIn.Long(row["CopayFeeSched"].ToString());
            insPlan.EmployerNum = SIn.Long(row["EmployerNum"].ToString());
            insPlan.CarrierNum = SIn.Long(row["CarrierNum"].ToString());
            insPlan.AllowedFeeSched = SIn.Long(row["AllowedFeeSched"].ToString());
            insPlan.TrojanID = SIn.String(row["TrojanID"].ToString());
            insPlan.DivisionNo = SIn.String(row["DivisionNo"].ToString());
            insPlan.IsMedical = SIn.Bool(row["IsMedical"].ToString());
            insPlan.FilingCode = SIn.Long(row["FilingCode"].ToString());
            insPlan.DentaideCardSequence = SIn.Byte(row["DentaideCardSequence"].ToString());
            insPlan.ShowBaseUnits = SIn.Bool(row["ShowBaseUnits"].ToString());
            insPlan.CodeSubstNone = SIn.Bool(row["CodeSubstNone"].ToString());
            insPlan.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            insPlan.MonthRenew = SIn.Byte(row["MonthRenew"].ToString());
            insPlan.FilingCodeSubtype = SIn.Long(row["FilingCodeSubtype"].ToString());
            insPlan.CanadianPlanFlag = SIn.String(row["CanadianPlanFlag"].ToString());
            insPlan.CanadianDiagnosticCode = SIn.String(row["CanadianDiagnosticCode"].ToString());
            insPlan.CanadianInstitutionCode = SIn.String(row["CanadianInstitutionCode"].ToString());
            insPlan.RxBIN = SIn.String(row["RxBIN"].ToString());
            insPlan.CobRule = (EnumCobRule) SIn.Int(row["CobRule"].ToString());
            insPlan.SopCode = SIn.String(row["SopCode"].ToString());
            insPlan.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            insPlan.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            insPlan.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            insPlan.HideFromVerifyList = SIn.Bool(row["HideFromVerifyList"].ToString());
            insPlan.OrthoType = (OrthoClaimType) SIn.Int(row["OrthoType"].ToString());
            insPlan.OrthoAutoProcFreq = (OrthoAutoProcFrequency) SIn.Int(row["OrthoAutoProcFreq"].ToString());
            insPlan.OrthoAutoProcCodeNumOverride = SIn.Long(row["OrthoAutoProcCodeNumOverride"].ToString());
            insPlan.OrthoAutoFeeBilled = SIn.Double(row["OrthoAutoFeeBilled"].ToString());
            insPlan.OrthoAutoClaimDaysWait = SIn.Int(row["OrthoAutoClaimDaysWait"].ToString());
            insPlan.BillingType = SIn.Long(row["BillingType"].ToString());
            insPlan.HasPpoSubstWriteoffs = SIn.Bool(row["HasPpoSubstWriteoffs"].ToString());
            insPlan.ExclusionFeeRule = (ExclusionRule) SIn.Int(row["ExclusionFeeRule"].ToString());
            insPlan.ManualFeeSchedNum = SIn.Long(row["ManualFeeSchedNum"].ToString());
            insPlan.IsBlueBookEnabled = SIn.Bool(row["IsBlueBookEnabled"].ToString());
            insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride = (YN) SIn.Int(row["InsPlansZeroWriteOffsOnAnnualMaxOverride"].ToString());
            insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride = (YN) SIn.Int(row["InsPlansZeroWriteOffsOnFreqOrAgingOverride"].ToString());
            insPlan.PerVisitPatAmount = SIn.Double(row["PerVisitPatAmount"].ToString());
            insPlan.PerVisitInsAmount = SIn.Double(row["PerVisitInsAmount"].ToString());
            retVal.Add(insPlan);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsPlan> listInsPlans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsPlan";
        var table = new DataTable(tableName);
        table.Columns.Add("PlanNum");
        table.Columns.Add("GroupName");
        table.Columns.Add("GroupNum");
        table.Columns.Add("PlanNote");
        table.Columns.Add("FeeSched");
        table.Columns.Add("PlanType");
        table.Columns.Add("ClaimFormNum");
        table.Columns.Add("UseAltCode");
        table.Columns.Add("ClaimsUseUCR");
        table.Columns.Add("CopayFeeSched");
        table.Columns.Add("EmployerNum");
        table.Columns.Add("CarrierNum");
        table.Columns.Add("AllowedFeeSched");
        table.Columns.Add("TrojanID");
        table.Columns.Add("DivisionNo");
        table.Columns.Add("IsMedical");
        table.Columns.Add("FilingCode");
        table.Columns.Add("DentaideCardSequence");
        table.Columns.Add("ShowBaseUnits");
        table.Columns.Add("CodeSubstNone");
        table.Columns.Add("IsHidden");
        table.Columns.Add("MonthRenew");
        table.Columns.Add("FilingCodeSubtype");
        table.Columns.Add("CanadianPlanFlag");
        table.Columns.Add("CanadianDiagnosticCode");
        table.Columns.Add("CanadianInstitutionCode");
        table.Columns.Add("RxBIN");
        table.Columns.Add("CobRule");
        table.Columns.Add("SopCode");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("HideFromVerifyList");
        table.Columns.Add("OrthoType");
        table.Columns.Add("OrthoAutoProcFreq");
        table.Columns.Add("OrthoAutoProcCodeNumOverride");
        table.Columns.Add("OrthoAutoFeeBilled");
        table.Columns.Add("OrthoAutoClaimDaysWait");
        table.Columns.Add("BillingType");
        table.Columns.Add("HasPpoSubstWriteoffs");
        table.Columns.Add("ExclusionFeeRule");
        table.Columns.Add("ManualFeeSchedNum");
        table.Columns.Add("IsBlueBookEnabled");
        table.Columns.Add("InsPlansZeroWriteOffsOnAnnualMaxOverride");
        table.Columns.Add("InsPlansZeroWriteOffsOnFreqOrAgingOverride");
        table.Columns.Add("PerVisitPatAmount");
        table.Columns.Add("PerVisitInsAmount");
        foreach (var insPlan in listInsPlans)
            table.Rows.Add(SOut.Long(insPlan.PlanNum), insPlan.GroupName, insPlan.GroupNum, insPlan.PlanNote, SOut.Long(insPlan.FeeSched), insPlan.PlanType, SOut.Long(insPlan.ClaimFormNum), SOut.Bool(insPlan.UseAltCode), SOut.Bool(insPlan.ClaimsUseUCR), SOut.Long(insPlan.CopayFeeSched), SOut.Long(insPlan.EmployerNum), SOut.Long(insPlan.CarrierNum), SOut.Long(insPlan.AllowedFeeSched), insPlan.TrojanID, insPlan.DivisionNo, SOut.Bool(insPlan.IsMedical), SOut.Long(insPlan.FilingCode), SOut.Byte(insPlan.DentaideCardSequence), SOut.Bool(insPlan.ShowBaseUnits), SOut.Bool(insPlan.CodeSubstNone), SOut.Bool(insPlan.IsHidden), SOut.Byte(insPlan.MonthRenew), SOut.Long(insPlan.FilingCodeSubtype), insPlan.CanadianPlanFlag, insPlan.CanadianDiagnosticCode, insPlan.CanadianInstitutionCode, insPlan.RxBIN, SOut.Int((int) insPlan.CobRule), insPlan.SopCode, SOut.Long(insPlan.SecUserNumEntry), SOut.DateT(insPlan.SecDateEntry, false), SOut.DateT(insPlan.SecDateTEdit, false), SOut.Bool(insPlan.HideFromVerifyList), SOut.Int((int) insPlan.OrthoType), SOut.Int((int) insPlan.OrthoAutoProcFreq), SOut.Long(insPlan.OrthoAutoProcCodeNumOverride), SOut.Double(insPlan.OrthoAutoFeeBilled), SOut.Int(insPlan.OrthoAutoClaimDaysWait), SOut.Long(insPlan.BillingType), SOut.Bool(insPlan.HasPpoSubstWriteoffs), SOut.Int((int) insPlan.ExclusionFeeRule), SOut.Long(insPlan.ManualFeeSchedNum), SOut.Bool(insPlan.IsBlueBookEnabled), SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride), SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride), SOut.Double(insPlan.PerVisitPatAmount), SOut.Double(insPlan.PerVisitInsAmount));
        return table;
    }

    public static long Insert(InsPlan insPlan)
    {
        return Insert(insPlan, false);
    }

    public static long Insert(InsPlan insPlan, bool useExistingPK)
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
            + DbHelper.Now() + ","
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
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(insPlan.PlanNote));
        {
            insPlan.PlanNum = Db.NonQ(command, true, "PlanNum", "insPlan", paramPlanNote);
        }
        return insPlan.PlanNum;
    }

    public static long InsertNoCache(InsPlan insPlan)
    {
        return InsertNoCache(insPlan, false);
    }

    public static long InsertNoCache(InsPlan insPlan, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insplan (";
        if (isRandomKeys || useExistingPK) command += "PlanNum,";
        command += "GroupName,GroupNum,PlanNote,FeeSched,PlanType,ClaimFormNum,UseAltCode,ClaimsUseUCR,CopayFeeSched,EmployerNum,CarrierNum,AllowedFeeSched,TrojanID,DivisionNo,IsMedical,FilingCode,DentaideCardSequence,ShowBaseUnits,CodeSubstNone,IsHidden,MonthRenew,FilingCodeSubtype,CanadianPlanFlag,CanadianDiagnosticCode,CanadianInstitutionCode,RxBIN,CobRule,SopCode,SecUserNumEntry,SecDateEntry,HideFromVerifyList,OrthoType,OrthoAutoProcFreq,OrthoAutoProcCodeNumOverride,OrthoAutoFeeBilled,OrthoAutoClaimDaysWait,BillingType,HasPpoSubstWriteoffs,ExclusionFeeRule,ManualFeeSchedNum,IsBlueBookEnabled,InsPlansZeroWriteOffsOnAnnualMaxOverride,InsPlansZeroWriteOffsOnFreqOrAgingOverride,PerVisitPatAmount,PerVisitInsAmount) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insPlan.PlanNum) + ",";
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
            + DbHelper.Now() + ","
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
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(insPlan.PlanNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPlanNote);
        else
            insPlan.PlanNum = Db.NonQ(command, true, "PlanNum", "insPlan", paramPlanNote);
        return insPlan.PlanNum;
    }

    public static void Update(InsPlan insPlan)
    {
        var command = "UPDATE insplan SET "
                      + "GroupName                                 = '" + SOut.String(insPlan.GroupName) + "', "
                      + "GroupNum                                  = '" + SOut.String(insPlan.GroupNum) + "', "
                      + "PlanNote                                  =  " + DbHelper.ParamChar + "paramPlanNote, "
                      + "FeeSched                                  =  " + SOut.Long(insPlan.FeeSched) + ", "
                      + "PlanType                                  = '" + SOut.String(insPlan.PlanType) + "', "
                      + "ClaimFormNum                              =  " + SOut.Long(insPlan.ClaimFormNum) + ", "
                      + "UseAltCode                                =  " + SOut.Bool(insPlan.UseAltCode) + ", "
                      + "ClaimsUseUCR                              =  " + SOut.Bool(insPlan.ClaimsUseUCR) + ", "
                      + "CopayFeeSched                             =  " + SOut.Long(insPlan.CopayFeeSched) + ", "
                      + "EmployerNum                               =  " + SOut.Long(insPlan.EmployerNum) + ", "
                      + "CarrierNum                                =  " + SOut.Long(insPlan.CarrierNum) + ", "
                      + "AllowedFeeSched                           =  " + SOut.Long(insPlan.AllowedFeeSched) + ", "
                      + "TrojanID                                  = '" + SOut.String(insPlan.TrojanID) + "', "
                      + "DivisionNo                                = '" + SOut.String(insPlan.DivisionNo) + "', "
                      + "IsMedical                                 =  " + SOut.Bool(insPlan.IsMedical) + ", "
                      + "FilingCode                                =  " + SOut.Long(insPlan.FilingCode) + ", "
                      + "DentaideCardSequence                      =  " + SOut.Byte(insPlan.DentaideCardSequence) + ", "
                      + "ShowBaseUnits                             =  " + SOut.Bool(insPlan.ShowBaseUnits) + ", "
                      + "CodeSubstNone                             =  " + SOut.Bool(insPlan.CodeSubstNone) + ", "
                      + "IsHidden                                  =  " + SOut.Bool(insPlan.IsHidden) + ", "
                      + "MonthRenew                                =  " + SOut.Byte(insPlan.MonthRenew) + ", "
                      + "FilingCodeSubtype                         =  " + SOut.Long(insPlan.FilingCodeSubtype) + ", "
                      + "CanadianPlanFlag                          = '" + SOut.String(insPlan.CanadianPlanFlag) + "', "
                      + "CanadianDiagnosticCode                    = '" + SOut.String(insPlan.CanadianDiagnosticCode) + "', "
                      + "CanadianInstitutionCode                   = '" + SOut.String(insPlan.CanadianInstitutionCode) + "', "
                      + "RxBIN                                     = '" + SOut.String(insPlan.RxBIN) + "', "
                      + "CobRule                                   =  " + SOut.Int((int) insPlan.CobRule) + ", "
                      + "SopCode                                   = '" + SOut.String(insPlan.SopCode) + "', "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "HideFromVerifyList                        =  " + SOut.Bool(insPlan.HideFromVerifyList) + ", "
                      + "OrthoType                                 =  " + SOut.Int((int) insPlan.OrthoType) + ", "
                      + "OrthoAutoProcFreq                         =  " + SOut.Int((int) insPlan.OrthoAutoProcFreq) + ", "
                      + "OrthoAutoProcCodeNumOverride              =  " + SOut.Long(insPlan.OrthoAutoProcCodeNumOverride) + ", "
                      + "OrthoAutoFeeBilled                        =  " + SOut.Double(insPlan.OrthoAutoFeeBilled) + ", "
                      + "OrthoAutoClaimDaysWait                    =  " + SOut.Int(insPlan.OrthoAutoClaimDaysWait) + ", "
                      + "BillingType                               =  " + SOut.Long(insPlan.BillingType) + ", "
                      + "HasPpoSubstWriteoffs                      =  " + SOut.Bool(insPlan.HasPpoSubstWriteoffs) + ", "
                      + "ExclusionFeeRule                          =  " + SOut.Int((int) insPlan.ExclusionFeeRule) + ", "
                      + "ManualFeeSchedNum                         =  " + SOut.Long(insPlan.ManualFeeSchedNum) + ", "
                      + "IsBlueBookEnabled                         =  " + SOut.Bool(insPlan.IsBlueBookEnabled) + ", "
                      + "InsPlansZeroWriteOffsOnAnnualMaxOverride  =  " + SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride) + ", "
                      + "InsPlansZeroWriteOffsOnFreqOrAgingOverride=  " + SOut.Int((int) insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride) + ", "
                      + "PerVisitPatAmount                         =  " + SOut.Double(insPlan.PerVisitPatAmount) + ", "
                      + "PerVisitInsAmount                         =  " + SOut.Double(insPlan.PerVisitInsAmount) + " "
                      + "WHERE PlanNum = " + SOut.Long(insPlan.PlanNum);
        if (insPlan.PlanNote == null) insPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(insPlan.PlanNote));
        Db.NonQ(command, paramPlanNote);
    }

    public static bool Update(InsPlan insPlan, InsPlan oldInsPlan)
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

        if (command == "") return false;
        if (insPlan.PlanNote == null) insPlan.PlanNote = "";
        var paramPlanNote = new OdSqlParameter("paramPlanNote", OdDbType.Text, SOut.StringParam(insPlan.PlanNote));
        command = "UPDATE insplan SET " + command
                                        + " WHERE PlanNum = " + SOut.Long(insPlan.PlanNum);
        Db.NonQ(command, paramPlanNote);
        return true;
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

    public static void Delete(long planNum)
    {
        ClearFkey(planNum);
        var command = "DELETE FROM insplan "
                      + "WHERE PlanNum = " + SOut.Long(planNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPlanNums)
    {
        if (listPlanNums == null || listPlanNums.Count == 0) return;
        ClearFkey(listPlanNums);
        var command = "DELETE FROM insplan "
                      + "WHERE PlanNum IN(" + string.Join(",", listPlanNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void ClearFkey(long planNum)
    {
        if (planNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(planNum) + " AND PermType IN (65)";
        Db.NonQ(command);
    }

    public static void ClearFkey(List<long> listPlanNums)
    {
        if (listPlanNums == null || listPlanNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listPlanNums.FindAll(x => x != 0)) + ") AND PermType IN (65)";
        Db.NonQ(command);
    }
}