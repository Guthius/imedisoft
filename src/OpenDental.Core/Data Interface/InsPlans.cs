using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsPlans
{
    ///<summary>Also fills PlanNum from db.</summary>
    public static long Insert(InsPlan insPlan)
    {
        return Insert(insPlan, false);
    }

    ///<summary>Also fills PlanNum from db.</summary>
    public static long Insert(InsPlan insPlan, bool useExistingPK)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        insPlan.SecUserNumEntry = Security.CurUser.UserNum;
        var insPlanOld = insPlan.Copy();
        var planNum = InsPlanCrud.Insert(insPlan, useExistingPK);
        if (insPlanOld.PlanNum == 0)
            InsEditLogs.MakeLogEntry(insPlan, null, InsEditLogType.InsPlan, insPlan.SecUserNumEntry);
        else
            InsEditLogs.MakeLogEntry(insPlan, insPlanOld, InsEditLogType.InsPlan, insPlan.SecUserNumEntry);
        InsVerifies.Upsert(planNum, VerifyTypes.InsuranceBenefit);
        return planNum;
    }

    ///<summary>Pass in the old InsPlan to avoid querying the db for it.</summary>
    public static void Update(InsPlan insPlan, InsPlan insPlanOld = null)
    {
        if (insPlanOld == null) insPlanOld = RefreshOne(insPlan.PlanNum);
        InsPlanCrud.Update(insPlan, insPlanOld);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        InsEditLogs.MakeLogEntry(insPlan, insPlanOld, InsEditLogType.InsPlan, Security.CurUser.UserNum);
    }

    /// <summary>
    ///     It's fastest if you supply a plan list that contains the plan, but it also works just fine if it can't initally
    ///     locate the plan in the
    ///     list.  You can supply a list of length 0 or null.  If not in the list, retrieves from db.  Returns null if planNum
    ///     is 0 or if it cannot find the insplan from the db.
    /// </summary>
    public static InsPlan GetPlan(long planNum, List<InsPlan> listInsPlans)
    {
        if (planNum == 0) return null;
        if (listInsPlans == null) listInsPlans = new List<InsPlan>();
        //LastOrDefault to preserve old behavior. No other reason.
        var insPlan = listInsPlans.LastOrDefault(x => x.PlanNum == planNum);
        if (insPlan == null) return RefreshOne(planNum);
        return insPlan;
    }

    ///<summary>Gets a list of plans from the database.</summary>
    public static List<InsPlan> GetPlans(List<long> listPlanNums)
    {
        if (listPlanNums == null || listPlanNums.Count == 0) return new List<InsPlan>();

        var command = "SELECT * FROM insplan WHERE PlanNum IN (" + string.Join(",", listPlanNums) + ")";
        return InsPlanCrud.SelectMany(command);
    }

    ///<summary>Gets a list of plans from the database for the API.</summary>
    public static List<InsPlan> GetInsPlansForApi(int limit, int offset, string planType, long carrierNum)
    {
        var command = "SELECT * FROM insplan WHERE SecDateEntry >= " + SOut.DateT(DateTime.MinValue) + " ";
        if (planType != null) command += "AND PlanType='" + SOut.String(planType) + "' ";
        if (carrierNum > 0) command += "AND CarrierNum=" + SOut.Long(carrierNum) + " ";
        command += "ORDER BY PlanNum " //same fixed order each time
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return InsPlanCrud.SelectMany(command);
    }

    /*
    ///<summary>Will return null if no active plan for that ordinal.  Ordinal means primary, secondary, etc.</summary>
    public static InsPlan GetPlanByOrdinal(int patNum,int ordinal) {
        string command="SELECT * FROM insplan WHERE EXISTS "
            +"(SELECT * FROM patplan WHERE insplan.PlanNum=patplan.PlanNum "
            +"AND patplan.PatNum="+POut.PInt(patNum)
            +" AND patplan.Ordinal="+POut.PInt(ordinal);
        //num = '"+planNum+"'";
    }*/

    public static InsPlan[] GetByTrojanID(string trojanID)
    {
        var command = "SELECT * FROM insplan WHERE TrojanID = '" + SOut.String(trojanID) + "'";
        return InsPlanCrud.SelectMany(command).ToArray();
    }

    ///<summary>Only loads one plan from db. Can return null.</summary>
    public static InsPlan RefreshOne(long planNum)
    {
        if (planNum == 0) return null;
        var command = "SELECT * FROM insplan WHERE plannum = '" + SOut.Long(planNum) + "'";
        return InsPlanCrud.SelectOne(command);
    }

    
    public static List<InsPlan> GetPatientData(List<InsSub> listInsSubs)
    {
        return RefreshForSubList(listInsSubs);
    }

    /// <summary>
    ///     Returns true if the InsPlan, or global pref indicate that estimates should zero out write-offs on aging or
    ///     frequency limitations exceeded. False otherwise.
    /// </summary>
    public static bool DoZeroOutWriteOffOnOtherLimitation(InsPlan insPlan)
    {
        if (insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride == YN.Unknown) return PrefC.GetBool(PrefName.InsPlansZeroWriteOffsOnFreqOrAging);
        return insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride == YN.Yes;
    }

    /// <summary>
    ///     Returns true if the InsPlan, or global pref indicate that estimates should zero out write-offs when annual max
    ///     is entirely surpassed. False otherwise.
    /// </summary>
    public static bool DoZeroOutWriteOffOnAnnualMaxLimitation(InsPlan insPlan)
    {
        if (insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride == YN.Unknown) return PrefC.GetBool(PrefName.InsPlansZeroWriteOffsOnAnnualMax);
        return insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride == YN.Yes;
    }

    ///<summary>Gets List of plans based on the subList.  The list won't be in the same order.</summary>
    public static List<InsPlan> RefreshForSubList(List<InsSub> listInsSubs)
    {
        if (listInsSubs == null || listInsSubs.Count == 0) return new List<InsPlan>();
        var command = "SELECT * FROM insplan WHERE PlanNum IN(" + string.Join(",", listInsSubs.Select(x => SOut.Long(x.PlanNum))) + ")";
        return InsPlanCrud.SelectMany(command);
    }

    ///<summary>Tests all fields for equality.</summary>
    public static bool AreEqualValue(InsPlan insPlanA, InsPlan insPlanB)
    {
        if (insPlanA.PlanNum == insPlanB.PlanNum
            && insPlanA.GroupName == insPlanB.GroupName
            && insPlanA.GroupNum == insPlanB.GroupNum
            && insPlanA.PlanNote == insPlanB.PlanNote
            && insPlanA.FeeSched == insPlanB.FeeSched
            && insPlanA.PlanType == insPlanB.PlanType
            && insPlanA.ClaimFormNum == insPlanB.ClaimFormNum
            && insPlanA.UseAltCode == insPlanB.UseAltCode
            && insPlanA.ClaimsUseUCR == insPlanB.ClaimsUseUCR
            && insPlanA.CopayFeeSched == insPlanB.CopayFeeSched
            && insPlanA.EmployerNum == insPlanB.EmployerNum
            && insPlanA.CarrierNum == insPlanB.CarrierNum
            && insPlanA.AllowedFeeSched == insPlanB.AllowedFeeSched
            && insPlanA.ManualFeeSchedNum == insPlanB.ManualFeeSchedNum
            && insPlanA.TrojanID == insPlanB.TrojanID
            && insPlanA.DivisionNo == insPlanB.DivisionNo
            && insPlanA.IsMedical == insPlanB.IsMedical
            && insPlanA.FilingCode == insPlanB.FilingCode
            && insPlanA.DentaideCardSequence == insPlanB.DentaideCardSequence
            && insPlanA.ShowBaseUnits == insPlanB.ShowBaseUnits
            && insPlanA.CodeSubstNone == insPlanB.CodeSubstNone
            && insPlanA.IsHidden == insPlanB.IsHidden
            && insPlanA.MonthRenew == insPlanB.MonthRenew
            && insPlanA.FilingCodeSubtype == insPlanB.FilingCodeSubtype
            && insPlanA.CanadianPlanFlag == insPlanB.CanadianPlanFlag
            && insPlanA.CobRule == insPlanB.CobRule
            && insPlanA.HideFromVerifyList == insPlanB.HideFromVerifyList
            && insPlanA.OrthoType == insPlanB.OrthoType
            && insPlanA.OrthoAutoProcCodeNumOverride == insPlanB.OrthoAutoProcCodeNumOverride
            && insPlanA.OrthoAutoProcFreq == insPlanB.OrthoAutoProcFreq
            && insPlanA.OrthoAutoClaimDaysWait == insPlanB.OrthoAutoClaimDaysWait
            && insPlanA.OrthoAutoFeeBilled == insPlanB.OrthoAutoFeeBilled
            && insPlanA.BillingType == insPlanB.BillingType
            && insPlanA.HasPpoSubstWriteoffs == insPlanB.HasPpoSubstWriteoffs
            && insPlanA.ExclusionFeeRule == insPlanB.ExclusionFeeRule
            && insPlanA.IsBlueBookEnabled == insPlanB.IsBlueBookEnabled
            && insPlanA.InsPlansZeroWriteOffsOnFreqOrAgingOverride == insPlanB.InsPlansZeroWriteOffsOnFreqOrAgingOverride
            && insPlanA.InsPlansZeroWriteOffsOnAnnualMaxOverride == insPlanB.InsPlansZeroWriteOffsOnAnnualMaxOverride
            && insPlanA.PerVisitPatAmount == insPlanB.PerVisitPatAmount
            && insPlanA.PerVisitInsAmount == insPlanB.PerVisitInsAmount)
            //When adding a field here, send a task to Web Enhancements so they can update Insurance Plan Information Fields with changes that trigger
            //a new plan.
            return true;
        return false;
    }

    ///<summary>Gets all insurance plans where the feeSched or copayFeeSched is equal to feeSchedNum</summary>
    public static List<InsPlan> GetForFeeSchedNum(long feeSchedNum)
    {
        var command = "SELECT * FROM insplan WHERE insplan.FeeSched = " + SOut.Long(feeSchedNum) + " OR insplan.CopayFeeSched=" + SOut.Long(feeSchedNum);
        return InsPlanCrud.SelectMany(command);
    }

    /*
    ///<summary>Called from FormInsPlan when applying changes to all identical insurance plans. This updates the synchronized fields for all plans like the specified insPlan.  Current InsPlan must be set to the new values that we want.  BenefitNotes and SubscNote are specific to subscriber and are not changed.  PlanNotes are handled separately in a different function after this one is complete.</summary>
    public static void UpdateForLike(InsPlan like, InsPlan plan) {

        string command= "UPDATE insplan SET "
            +"EmployerNum = '"     +POut.Long   (plan.EmployerNum)+"'"
            +",GroupName = '"      +POut.String(plan.GroupName)+"'"
            +",GroupNum = '"       +POut.String(plan.GroupNum)+"'"
            +",DivisionNo = '"     +POut.String(plan.DivisionNo)+"'"
            +",CarrierNum = '"     +POut.Long   (plan.CarrierNum)+"'"
            +",PlanType = '"       +POut.String(plan.PlanType)+"'"
            +",UseAltCode = '"     +POut.Bool  (plan.UseAltCode)+"'"
            +",IsMedical = '"      +POut.Bool  (plan.IsMedical)+"'"
            +",ClaimsUseUCR = '"   +POut.Bool  (plan.ClaimsUseUCR)+"'"
            +",FeeSched = '"       +POut.Long   (plan.FeeSched)+"'"
            +",CopayFeeSched = '"  +POut.Long   (plan.CopayFeeSched)+"'"
            +",ClaimFormNum = '"   +POut.Long   (plan.ClaimFormNum)+"'"
            +",AllowedFeeSched= '" +POut.Long   (plan.AllowedFeeSched)+"'"
            +",TrojanID = '"       +POut.String(plan.TrojanID)+"'"
            +",FilingCode = '"     +POut.Long   (plan.FilingCode)+"'"
            +",FilingCodeSubtype = '"+POut.Long(plan.FilingCodeSubtype)+"'"
            +",ShowBaseUnits = '"  +POut.Bool  (plan.ShowBaseUnits)+"'"
            //+",DedBeforePerc = '"  +POut.PBool  (plan.DedBeforePerc)+"'"
            +",CodeSubstNone='"    +POut.Bool  (plan.CodeSubstNone)+"'"
            +",IsHidden='"         +POut.Bool  (plan.IsHidden)+"'"
            +",MonthRenew='"       +POut.Int   (plan.MonthRenew)+"'"
            //It is most likely that MonthRenew would be the same for everyone on the same plan.  If we get complaints, we might have to add an option.
            +" WHERE "
            +"EmployerNum = '"        +POut.Long   (like.EmployerNum)+"' "
            +"AND GroupName = '"      +POut.String(like.GroupName)+"' "
            +"AND GroupNum = '"       +POut.String(like.GroupNum)+"' "
            +"AND DivisionNo = '"     +POut.String(like.DivisionNo)+"'"
            +"AND CarrierNum = '"     +POut.Long   (like.CarrierNum)+"' "
            +"AND IsMedical = '"      +POut.Bool  (like.IsMedical)+"'";
        Db.NonQ(command);
    }*/

    /// <summary>
    ///     Gets a description of the specified plan, including carrier name and subscriber.
    ///     It's fastest if you supply a plan list that contains the plan, but it also works just fine if it can't initally
    ///     locate the plan in the list.  You can supply an array of length 0 for both family and listInsPlans.
    /// </summary>
    public static string GetDescript(long planNum, Family family, List<InsPlan> listInsPlans, long insSubNum, List<InsSub> listInsSubs)
    {
        if (planNum == 0) return "";
        var insPlan = GetPlan(planNum, listInsPlans);
        if (insPlan == null || insPlan.PlanNum == 0) return "";
        var insSub = InsSubs.GetSub(insSubNum, listInsSubs);
        if (insSub == null || insSub.InsSubNum == 0) return "";
        var subscriber = family.GetNameInFamFL(insSub.Subscriber);
        if (subscriber == "") //subscriber from another family
            subscriber = Patients.GetLim(insSub.Subscriber).GetNameLF();
        var retStr = "";
        //loop just to get the index of the plan in the family list
        var otherFam = true;
        for (var i = 0; i < listInsPlans.Count; i++)
            if (listInsPlans[i].PlanNum == planNum)
                otherFam = false;
        //retStr += (i+1).ToString()+": ";
        if (otherFam) //retStr=="")
            retStr = "(other fam):";
        var carrier = Carriers.GetCarrier(insPlan.CarrierNum);
        var carrierName = carrier.CarrierName;
        if (carrierName.Length > 20) carrierName = carrierName.Substring(0, 20) + "...";
        retStr += carrierName;
        retStr += " (" + subscriber + ")";
        return retStr;
    }

    ///<summary>Used in Ins lines in Account module and in Family module.</summary>
    public static string GetCarrierName(long planNum, List<InsPlan> listInsPlans)
    {
        var insPlan = GetPlan(planNum, listInsPlans);
        if (insPlan == null) return "";
        var carrier = Carriers.GetCarrier(insPlan.CarrierNum);
        if (carrier.CarrierNum == 0) //if corrupted
            return "";
        return carrier.CarrierName;
    }

    /// <summary>
    ///     Only used once in Claims.cs.  Gets insurance benefits remaining for one benefit year.  Returns actual remaining
    ///     insurance based on ClaimProc data, taking into account inspaid and ins pending.
    ///     Must supply all claimprocs for the patient.  Date used to determine which benefit year to calc.  Usually today's
    ///     date.  The insplan.PlanNum is the plan to get value for.
    ///     claimNumExclude is the ClaimNum to exclude, or enter -1 to include all.  This does not yet handle calculations
    ///     where ortho max is different from regular max.
    ///     Just takes the most general annual max, and subtracts all benefits used from all categories.
    /// </summary>
    public static double GetInsRem(List<ClaimProcHist> listClaimProcHists, DateTime dateAsOf, long planNum, long patPlanNum, long claimNumExclude, List<InsPlan> listInsPlans, List<Benefit> listBenefits, long patNum, long insSubNum)
    {
        var insUsed = GetInsUsedDisplay(listClaimProcHists, dateAsOf, planNum, patPlanNum, claimNumExclude, listInsPlans, listBenefits, patNum, insSubNum);
        var insPlan = GetPlan(planNum, listInsPlans);
        var insPending = GetPendingDisplay(listClaimProcHists, dateAsOf, insPlan, patPlanNum, claimNumExclude, patNum, insSubNum, listBenefits);
        var annualMaxFam = Benefits.GetAnnualMaxDisplay(listBenefits, planNum, patPlanNum, true);
        var annualMaxInd = Benefits.GetAnnualMaxDisplay(listBenefits, planNum, patPlanNum, false);
        var annualMax = annualMaxInd;
        if (annualMaxFam > annualMaxInd) annualMax = annualMaxFam;
        if (annualMax < 0) return 999999;
        if (annualMax - insUsed - insPending < 0) return 0;
        return annualMax - insUsed - insPending;
    }

    /// <summary>
    ///     Only for display purposes rather than for calculations.  Get pending insurance for a given plan for one benefit
    ///     year.
    ///     Include a history list for the patient/family.  dateAsOf used to determine which benefit year to calc.  Usually the
    ///     date of service for a claim.  The planNum is the plan to get value for.
    /// </summary>
    public static double GetPendingDisplay(List<ClaimProcHist> listClaimProcHists, DateTime dateAsOf, InsPlan insPlan, long patPlanNum, long claimNumExclude, long patNum, long insSubNum, List<Benefit> listBenefits)
    {
        //InsPlan curPlan=GetPlan(planNum,PlanList);
        if (insPlan == null) return 0;
        //get the most recent renew date, possibly including today:
        var dateRenew = BenefitLogic.ComputeRenewDate(dateAsOf, insPlan.MonthRenew);
        var dateStop = dateRenew.AddYears(1);
        double retVal = 0;
        //CovCat generalCat=CovCats.GetForEbenCat(EbenefitCategory.General);
        //CovSpan[] covSpanArray=null;
        //if(generalCat!=null) {
        //  covSpanArray=CovSpans.GetForCat(generalCat.CovCatNum);
        //}
        for (var i = 0; i < listClaimProcHists.Count; i++)
        {
            //if(generalCat!=null) {//If there is a general category, then we only consider codes within it.  This is how we exclude ortho.
            //  if(!CovSpans.IsCodeInSpans(histList[i].StrProcCode,covSpanArray)) {//for example, ortho
            //    continue;
            //  }
            //}
            if (Benefits.LimitationExistsNotGeneral(listBenefits, insPlan.PlanNum, patPlanNum, listClaimProcHists[i].StrProcCode)) continue;
            if (listClaimProcHists[i].PlanNum == insPlan.PlanNum
                && listClaimProcHists[i].InsSubNum == insSubNum
                && listClaimProcHists[i].ClaimNum != claimNumExclude
                && listClaimProcHists[i].ProcDate < dateStop
                && listClaimProcHists[i].ProcDate >= dateRenew
                //enum ClaimProcStatus{NotReceived,Received,Preauth,Adjustment,Supplemental}
                && listClaimProcHists[i].Status == ClaimProcStatus.NotReceived
                && listClaimProcHists[i].PatNum == patNum)
                //Status Adjustment has no insPayEst, so can ignore it here.
                retVal += listClaimProcHists[i].Amount;
        }

        return retVal;
    }

    /// <summary>
    ///     Only for display purposes rather than for calculations.  Get insurance benefits used for one benefit year.
    ///     Must supply all relevant hist for the patient.  dateAsOf is used to determine which benefit year to calc.  Usually
    ///     date of service for a claim.  The insplan.PlanNum is the plan to get value for.  claimNumExclude is the ClaimNum to
    ///     exclude, or enter -1 to include all.  It only includes values that apply towards annual max.  So if there is a
    ///     limitation override for a category like ortho or preventive, then completed procedures in those categories will be
    ///     excluded.  The listBenefits passed in might very well have benefits from other insurance plans included.
    /// </summary>
    public static double GetInsUsedDisplay(List<ClaimProcHist> listClaimProcHists, DateTime dateAsOf, long planNum, long patPlanNum, long claimNumExclude, List<InsPlan> listInsPlans, List<Benefit> listBenefits, long patNum, long insSubNum)
    {
        var insPlan = GetPlan(planNum, listInsPlans);
        if (insPlan == null) return 0;
        //get the most recent renew date, possibly including today:
        var dateRenew = BenefitLogic.ComputeRenewDate(dateAsOf, insPlan.MonthRenew);
        var dateStop = dateRenew.AddYears(1);
        double retVal = 0;
        //CovCat generalCat=CovCats.GetForEbenCat(EbenefitCategory.General);
        //CovSpan[] covSpanArray=null;
        //if(generalCat!=null) {
        //  covSpanArray=CovSpans.GetForCat(generalCat.CovCatNum);
        //}
        for (var i = 0; i < listClaimProcHists.Count; i++)
        {
            if (listClaimProcHists[i].PlanNum != planNum
                || listClaimProcHists[i].InsSubNum != insSubNum
                || listClaimProcHists[i].ClaimNum == claimNumExclude
                || listClaimProcHists[i].ProcDate.Date >= dateStop
                || listClaimProcHists[i].ProcDate.Date < dateRenew
                || listClaimProcHists[i].PatNum != patNum)
                continue;
            if (Benefits.LimitationExistsNotGeneral(listBenefits, planNum, patPlanNum, listClaimProcHists[i].StrProcCode)) continue;
            //if(generalCat!=null){//If there is a general category, then we only consider codes within it.  This is how we exclude ortho.
            //	if(histList[i].StrProcCode!="" && !CovSpans.IsCodeInSpans(histList[i].StrProcCode,covSpanArray)){//for example, ortho
            //		continue;
            //	}
            //}
            //enum ClaimProcStatus{NotReceived,Received,Preauth,Adjustment,Supplemental}
            if (listClaimProcHists[i].Status == ClaimProcStatus.Received
                || listClaimProcHists[i].Status == ClaimProcStatus.Adjustment
                || listClaimProcHists[i].Status == ClaimProcStatus.Supplemental)
                retVal += listClaimProcHists[i].Amount;
        }

        return retVal;
    }

    /// <summary>
    ///     Only for display purposes rather than for calculations.  Get insurance deductible used for one benefit year.
    ///     Must supply a history list for the patient/family.  dateAsOf is used to determine which benefit year to calc.
    ///     Usually date of service for a claim.  The planNum is the plan to get value for.  claimNumExclude is the ClaimNum to
    ///     exclude, or enter -1 to include all.  It includes pending deductibles in the result.
    /// </summary>
    public static double GetDedUsedDisplay(List<ClaimProcHist> listClaimProcHists, DateTime dateAsOf, long planNum, long patPlanNum, long claimNumExclude, List<InsPlan> listInsPlans, BenefitCoverageLevel benefitCoverageLevel, long patNum)
    {
        var insPlan = GetPlan(planNum, listInsPlans);
        if (insPlan == null) return 0;
        //get the most recent renew date, possibly including today. Date based on annual max.
        var dateRenew = BenefitLogic.ComputeRenewDate(dateAsOf, insPlan.MonthRenew);
        var dateStop = dateRenew.AddYears(1);
        double retVal = 0;
        for (var i = 0; i < listClaimProcHists.Count; i++)
        {
            if (listClaimProcHists[i].PlanNum != planNum
                || listClaimProcHists[i].ClaimNum == claimNumExclude
                || listClaimProcHists[i].ProcDate >= dateStop
                || listClaimProcHists[i].ProcDate < dateRenew
                //no need to check status, because only the following statuses will be part of histlist:
                //Adjustment,NotReceived,Received,Supplemental
               )
                continue;
            if (benefitCoverageLevel != BenefitCoverageLevel.Family && listClaimProcHists[i].PatNum != patNum) continue; //to exclude histList items from other family members
            retVal += listClaimProcHists[i].Deduct;
        }

        return retVal;
    }

    /// <summary>
    ///     Only for display purposes rather than for calculations.  Get insurance deductible used for one benefit year.
    ///     Must supply a history list for the patient/family. dateAsOf is used to determine which benefit year to calc.
    ///     Usually date of service for a claim.  The planNum is the plan to get value for.  claimNumExclude is the ClaimNum to
    ///     exclude, or enter -1 to include all.  It includes pending deductibles in the result. The ded and dedFam variables
    ///     are the individual and family deductibles respectively. This function assumes that the individual deductible 'ded'
    ///     is always available, but that the family deductible 'dedFam' is optional (set to -1 if not available).
    /// </summary>
    public static double GetDedRemainDisplay(List<ClaimProcHist> listClaimProcHists, DateTime dateAsOf, long planNum, long patPlanNum, long claimNumExclude, List<InsPlan> listInsPlans, long patNum, double ded, double dedFam)
    {
        var insPlan = GetPlan(planNum, listInsPlans);
        if (insPlan == null) return 0;
        //get the most recent renew date, possibly including today. Date based on annual max.
        var renewDate = BenefitLogic.ComputeRenewDate(dateAsOf, insPlan.MonthRenew);
        var stopDate = renewDate.AddYears(1);
        var deductibleRemainderInd = ded;
        var deductibleRemainderFam = dedFam;
        for (var i = 0; i < listClaimProcHists.Count; i++)
        {
            if (listClaimProcHists[i].PlanNum != planNum
                || listClaimProcHists[i].ClaimNum == claimNumExclude
                || listClaimProcHists[i].ProcDate >= stopDate
                || listClaimProcHists[i].ProcDate < renewDate
                //no need to check status, because only the following statuses will be part of histlist:
                //Adjustment,NotReceived,Received,Supplemental
               )
                continue;
            deductibleRemainderFam -= listClaimProcHists[i].Deduct;
            if (listClaimProcHists[i].PatNum == patNum) deductibleRemainderInd -= listClaimProcHists[i].Deduct;
        }

        if (dedFam >= 0) return Math.Max(0, Math.Min(deductibleRemainderInd, deductibleRemainderFam)); //never negative
        return Math.Max(0, deductibleRemainderInd); //never negative
    }

    /*
    ///<summary>Used once from Claims and also in ContrTreat.  Gets insurance deductible remaining for one benefit year which includes the given date.  Must supply all claimprocs for the patient.  Must supply all benefits for patient so that we know if it's a service year or a calendar year.  Date used to determine which benefit year to calc.  Usually today's date.  The insplan.PlanNum is the plan to get value for.  ExcludeClaim is the ClaimNum to exclude, or enter -1 to include all.  The supplied procCode is needed because some deductibles, for instance, do not apply to preventive.</summary>
    public static double GetDedRem(List<ClaimProc> claimProcList,DateTime date,int planNum,int patPlanNum,int excludeClaim,List<InsPlan> PlanList,List<Benefit> benList,string procCode) {
        Meth.NoCheckMiddleTierRole();
        double dedTot=Benefits.GetDeductibleByCode(benList,planNum,patPlanNum,procCode);
        double dedUsed=GetDedUsed(claimProcList,date,planNum,patPlanNum,excludeClaim,PlanList,benList);
        if(dedTot-dedUsed<0){
            return 0;
        }
        return dedTot-dedUsed;
    }*/

    /*
    ///<Summary>Only used in TP to calculate discount for PPO procedure.  Will return -1 if no fee found.</Summary>
    public static double GetPPOAllowed(int codeNum,InsPlan plan){
        //plan has already been tested to not be null and to be a PPO plan.
        double fee=Fees.GetAmount(codeNum,plan.FeeSched);//could be -1
    }*/

    ///<summary>This is used in FormQuery.SubmitQuery to allow display of carrier names.</summary>
    public static Hashtable GetHListAll()
    {
        var table = GetCarrierTable();
        var hashtable = new Hashtable(table.Rows.Count);
        long plannum;
        string carrierName;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            plannum = SIn.Long(table.Rows[i][0].ToString());
            carrierName = SIn.String(table.Rows[i][1].ToString());
            hashtable.Add(plannum, carrierName);
        }

        return hashtable;
    }

    ///<summary>This is used in FormUserQuery to allow display of carrier names. Key is PlanNum, value is carrier name.</summary>
    public static Dictionary<long, string> GetDictPlanCarrier()
    {
        var table = GetCarrierTable();
        var dictionary = new Dictionary<long, string>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var plannum = SIn.Long(table.Rows[i][0].ToString());
            var carrierName = SIn.String(table.Rows[i][1].ToString());
            dictionary.Add(plannum, carrierName);
        }

        return dictionary;
    }

    public static DataTable GetCarrierTable()
    {
        var command = "SELECT insplan.PlanNum,carrier.CarrierName "
                      + "FROM insplan,carrier "
                      + "WHERE insplan.CarrierNum=carrier.CarrierNum";
        return DataCore.GetTable(command);
    }
    /*
    ///<summary>Used by Trojan.  Gets all distinct notes for the planNums supplied.  Includes blank notes.</summary>
    public static string[] GetNotesForPlans(List<long> planNums) {

        if(planNums.Count==0) {//this should never happen, but just in case...
            return new string[0];
        }
        if(planNums.Count==1 && planNums[0]==excludePlanNum){
            return new string[0];
        }
        string s="";
        for(int i=0;i<planNums.Count;i++) {
            if(planNums[i]==excludePlanNum){
                continue;
            }
            if(s!="") {
                s+=" OR";
            }
            s+=" PlanNum="+POut.Long(planNums[i]);
        }
        string command="SELECT DISTINCT PlanNote FROM insplan WHERE"+s;
        DataTable table=DataCore.GetTable(command);
        string[] retVal=new string[table.Rows.Count];
        for(int i=0;i<table.Rows.Count;i++) {
            retVal[i]=PIn.String(table.Rows[i][0].ToString());
        }
        return retVal;
    }

    ///<summary>Used by Trojan.  Sets the PlanNote for multiple plans at once.</summary>
    public static void UpdateNoteForPlans(List<long> planNums,string newNote) {

        if(planNums.Count==0){
            return;
        }
        string s="";
        for(int i=0;i<planNums.Count;i++){
            if(i>0){
                s+=" OR";
            }
            s+=" PlanNum="+POut.Long(planNums[i]);
        }
        string command="UPDATE insplan SET PlanNote='"+POut.String(newNote)+"' "
            +"WHERE"+s;
        Db.NonQ(command);
    }*/

    /*
    ///<summary>Called from FormInsPlan when user wants to view a benefit note for similar plans.  Should never include the current plan that the user is editing.  This function will get one note from the database, not including blank notes.  If no note can be found, then it returns empty string.</summary>
    public static string GetBenefitNotes(List<long> planNums) {

        if(planNums.Count==0){
            return "";
        }
        string s="";
        for(int i=0;i<planNums.Count;i++) {
            if(i>0) {
                s+=" OR";
            }
            s+=" PlanNum="+POut.Long(planNums[i]);
        }
        string command="SELECT BenefitNotes FROM insplan WHERE BenefitNotes != '' AND ("+s+") "+DbHelper.LimitAnd(1);
        DataTable table=DataCore.GetTable(command);
        //string[] retVal=new string[];
        if(table.Rows.Count==0){
            return "";
        }
        return PIn.String(table.Rows[0][0].ToString());
    }*/

    /*
    ///<summary>Gets a list of PlanNums from the database of plans that have identical info as this one. Used to perform updates to benefits, etc.  Note that you have the option to include the current plan in the list.</summary>
    public static List<long> GetPlanNumsOfSamePlans(string employerName,string groupName,string groupNum,
            string divisionNo,string carrierName,bool isMedical,long planNum,bool includePlanNum) {

        string command="SELECT PlanNum FROM insplan "
            +"LEFT JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum "
            +"LEFT JOIN employer ON employer.EmployerNum = insplan.EmployerNum ";
        if(employerName==""){
            command+="WHERE employer.EmpName IS NULL ";
        }
        else{
            command+="WHERE employer.EmpName = '"+POut.String(employerName)+"' ";
        }
        command+="AND insplan.GroupName = '"  +POut.String(groupName)+"' "
            +"AND insplan.GroupNum = '"   +POut.String(groupNum)+"' "
            +"AND insplan.DivisionNo = '" +POut.String(divisionNo)+"' "
            +"AND carrier.CarrierName = '"+POut.String(carrierName)+"' "
            +"AND insplan.IsMedical = '"  +POut.Bool  (isMedical)+"'"
            +"AND insplan.PlanNum != "+POut.Long(planNum);
        DataTable table=DataCore.GetTable(command);
        List<long> retVal=new List<long>();
        //if(includePlanNum){
        //	retVal=new int[table.Rows.Count+1];
        //}
        //else{
        //	retVal=new int[table.Rows.Count];
        //}
        for(int i=0;i<table.Rows.Count;i++) {
            retVal.Add(PIn.Long(table.Rows[i][0].ToString()));
        }
        if(includePlanNum){
            retVal.Add(planNum);
        }
        return retVal;
    }*/

    ///<summary>Used from FormInsPlans to get a big list of many plans, organized by carrier name or by employer.</summary>
    public static DataTable GetBigList(bool byEmployer, string empName, string carrierName, string groupName, string groupNum, string planNum,
        string trojanID, bool showHidden, bool isIncludeAll)
    {
        var table = new DataTable();
        DataRow row;
        table.Columns.Add("Address");
        table.Columns.Add("City");
        table.Columns.Add("CarrierName");
        table.Columns.Add("ElectID");
        table.Columns.Add("EmpName");
        table.Columns.Add("GroupName");
        table.Columns.Add("GroupNum");
        table.Columns.Add("noSendElect");
        table.Columns.Add("Phone");
        table.Columns.Add("PlanNum");
        table.Columns.Add("State");
        table.Columns.Add("subscribers");
        table.Columns.Add("trojanID");
        table.Columns.Add("Zip");
        table.Columns.Add("IsCDA");
        var command = "SELECT carrier.Address,carrier.City,CarrierName,ElectID,EmpName,GroupName,GroupNum,NoSendElect,"
                      + "carrier.Phone,PlanNum,"
                      + "(SELECT COUNT(DISTINCT Subscriber) FROM inssub WHERE insplan.PlanNum=inssub.PlanNum) subscribers," //for Oracle
                      + "carrier.State,TrojanID,carrier.Zip, "
                      //+"(SELECT COUNT(*) FROM employer WHERE insplan.EmployerNum=employer.EmployerNum) haveName "//for Oracle. Could be higher than 1?
                      + "CASE WHEN (EmpName IS NULL) THEN 1 ELSE 0 END as haveName," //for Oracle
                      + "carrier.IsCDA "
                      + "FROM insplan "
                      + "LEFT JOIN employer ON employer.EmployerNum = insplan.EmployerNum "
                      + "LEFT JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum "
                      + "WHERE CarrierName LIKE '%" + SOut.String(carrierName) + "%' ";
        if (empName != "") command += "AND EmpName LIKE '%" + SOut.String(empName) + "%' ";
        if (groupName != "") command += "AND GroupName LIKE '%" + SOut.String(groupName) + "%' ";
        if (groupNum != "") command += "AND GroupNum LIKE '%" + SOut.String(groupNum) + "%' ";
        if (planNum != "") command += "AND PlanNum LIKE '%" + SOut.String(planNum) + "%' ";
        if (trojanID != "") command += "AND TrojanID LIKE '%" + SOut.String(trojanID) + "%' ";
        if (!showHidden) command += "AND insplan.IsHidden=0 ";
        if (!isIncludeAll) command += DbHelper.LimitAnd(200);
        var tableRaw = DataCore.GetTable(command);
        List<DataRow> listDataRows;
        if (byEmployer)
            listDataRows = tableRaw.Select().OrderBy(x => x["haveName"].ToString()).ThenBy(x => x["EmpName"].ToString()).ThenBy(x => x["CarrierName"].ToString()).ToList();
        else //by carrier
            listDataRows = tableRaw.Select().OrderBy(x => x["CarrierName"].ToString()).ToList();
        for (var i = 0; i < listDataRows.Count; i++)
        {
            row = table.NewRow();
            row["Address"] = listDataRows[i]["Address"].ToString();
            row["City"] = listDataRows[i]["City"].ToString();
            row["CarrierName"] = listDataRows[i]["CarrierName"].ToString();
            row["ElectID"] = listDataRows[i]["ElectID"].ToString();
            row["EmpName"] = listDataRows[i]["EmpName"].ToString();
            row["GroupName"] = listDataRows[i]["GroupName"].ToString();
            row["GroupNum"] = listDataRows[i]["GroupNum"].ToString();
            row["noSendElect"] = listDataRows[i]["NoSendElect"].ToString() == "1" ? "X" : "";
            row["Phone"] = listDataRows[i]["Phone"].ToString();
            row["PlanNum"] = listDataRows[i]["PlanNum"].ToString();
            row["State"] = listDataRows[i]["State"].ToString();
            row["subscribers"] = listDataRows[i]["subscribers"].ToString();
            row["TrojanID"] = listDataRows[i]["TrojanID"].ToString();
            row["Zip"] = listDataRows[i]["Zip"].ToString();
            row["IsCDA"] = listDataRows[i]["IsCDA"].ToString();
            table.Rows.Add(row);
        }

        return table;
    }

    ///<summary>Used in FormFeesForIns</summary>
    public static DataTable GetListFeeCheck(string carrierName, string carrierNameNot, long feeSchedWithout, long feeSchedWith,
        FeeScheduleType feeScheduleType, string insPlanType = "none")
    {
        var pFeeSched = "FeeSched";
        if (feeScheduleType == FeeScheduleType.OutNetwork) pFeeSched = "AllowedFeeSched"; //This is the name of a column in the insplan table and cannot be changed to OutNetworkFeeSched
        if (feeScheduleType == FeeScheduleType.CoPay || feeScheduleType == FeeScheduleType.FixedBenefit) pFeeSched = "CopayFeeSched";
        if (feeScheduleType == FeeScheduleType.ManualBlueBook) pFeeSched = "ManualFeeSchedNum";
        var command =
            "SELECT insplan.PlanNum,insplan.GroupName,insplan.GroupNum,insplan.CopayFeeSched,employer.EmpName,carrier.CarrierName,"
            + "insplan.EmployerNum,insplan.CarrierNum,feesched.Description AS FeeSchedName,insplan.PlanType,"
            + "insplan.IsBlueBookEnabled,insplan." + pFeeSched + " feeSched "
            + "FROM insplan "
            + "LEFT JOIN employer ON employer.EmployerNum = insplan.EmployerNum "
            + "LEFT JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum "
            + "LEFT JOIN feesched ON feesched.FeeSchedNum = insplan." + pFeeSched + " "
            + "WHERE carrier.CarrierName LIKE '%" + SOut.String(carrierName) + "%' ";
        if (insPlanType != "none") command += "AND insplan.PlanType = '" + SOut.String(insPlanType) + "' ";
        if (carrierNameNot != "") command += "AND carrier.CarrierName NOT LIKE '%" + SOut.String(carrierNameNot) + "%' ";
        if (feeSchedWithout != 0) command += "AND insplan." + pFeeSched + " !=" + SOut.Long(feeSchedWithout) + " ";
        if (feeSchedWith != 0) command += "AND insplan." + pFeeSched + " =" + SOut.Long(feeSchedWith) + " ";
        command += "ORDER BY carrier.CarrierName,employer.EmpName,insplan.GroupNum";
        return DataCore.GetTable(command);
    }

    ///<summary>Used only in FormFeesForIns. Used to update the passed in list of insurance plans to a new fee schedule</summary>
    public static long ChangeFeeScheds(List<long> listInsPlanNums, long feeSchedNumNew, FeeScheduleType feeScheduleType, bool disableBlueBook, bool enableBlueBook)
    {
        if (listInsPlanNums.IsNullOrEmpty()) return 0; //Count of rows changed.

        if (listInsPlanNums.Count == 0) return 0; // no insurance plans to change
        var command = "UPDATE insplan SET ";
        if (disableBlueBook) //mutually exclusive from enableBlueBook, but not the inverse. They will not both be true
            command += "insplan.IsBlueBookEnabled=FALSE, ";
        if (enableBlueBook) command += "insplan.IsBlueBookEnabled=TRUE, ";
        if (feeScheduleType == FeeScheduleType.Normal)
        {
            command += "insplan.FeeSched =" + SOut.Long(feeSchedNumNew)
                                            + " WHERE insplan.FeeSched !=" + SOut.Long(feeSchedNumNew);
        }
        else if (feeScheduleType == FeeScheduleType.OutNetwork)
        {
            command += "insplan.AllowedFeeSched =" + SOut.Long(feeSchedNumNew)
                                                   + " WHERE insplan.AllowedFeeSched !=" + SOut.Long(feeSchedNumNew);
        }
        else if (feeScheduleType == FeeScheduleType.CoPay || feeScheduleType == FeeScheduleType.FixedBenefit)
        {
            command += "insplan.CopayFeeSched =" + SOut.Long(feeSchedNumNew);
            command += " WHERE insplan.CopayFeeSched !=" + SOut.Long(feeSchedNumNew);
        }
        else if (feeScheduleType == FeeScheduleType.ManualBlueBook)
        {
            command += "insplan.ManualFeeSchedNum =" + SOut.Long(feeSchedNumNew)
                                                     + " WHERE insplan.ManualFeeSchedNum !=" + SOut.Long(feeSchedNumNew);
        }

        command += $" AND insplan.PlanNum IN ({string.Join(",", listInsPlanNums.Select(x => SOut.Long(x)))})";
        if (disableBlueBook) InsBlueBooks.DeleteByPlanNums(listInsPlanNums.ToArray());
        var listInsPlans = GetPlans(listInsPlanNums);
        //log InsPlan's fee schedule update.
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listInsPlans.Count; i++)
            InsEditLogs.MakeLogEntry(SOut.String("FeeSchedNum"),
                Security.CurUser.UserNum,
                SOut.String(listInsPlans[i].FeeSched.ToString()),
                feeSchedNumNew.ToString(),
                InsEditLogType.InsPlan,
                SIn.Long(listInsPlans[i].PlanNum.ToString())
                , 0
                , listInsPlans[i].GroupNum + " - " + listInsPlans[i].GroupName);
        return Db.NonQ(command);
    }

    ///<summary>Used only in FormFeesForIns. Used to update the passed in list of insurance plans to a new insurance plan type</summary>
    public static long ChangeInsPlanTypes(List<long> listInsPlanNums, string newInsPlanType, bool enableBlueBook)
    {
        if (listInsPlanNums.IsNullOrEmpty()) return 0; //Count of rows changed.

        var command = "UPDATE insplan SET PlanType='" + SOut.String(newInsPlanType) + "'";
        command += ", insplan.IsBlueBookEnabled=" + SOut.Bool(enableBlueBook);
        command += " WHERE insplan.PlanNum IN (" + string.Join(",", listInsPlanNums.Select(x => SOut.Long(x))) + ")";
        var listInsPlans = GetPlans(listInsPlanNums);
        //log InsPlan's Insurance Plan Type update.
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listInsPlans.Count; i++)
            InsEditLogs.MakeLogEntry(SOut.String("PlanType"),
                Security.CurUser.UserNum,
                SOut.String(listInsPlans[i].PlanType),
                newInsPlanType,
                InsEditLogType.InsPlan,
                SIn.Long(listInsPlans[i].PlanNum.ToString()),
                0,
                listInsPlans[i].GroupNum + " - " + listInsPlans[i].GroupName);
        return Db.NonQ(command);
    }

    /// <summary>
    ///     Returns the number of fee schedules added.  It doesn't inform the user of how many plans were affected, but there
    ///     will obviously be a
    ///     certain number of plans for every new fee schedule.
    /// </summary>
    public static long GenerateAllowedFeeSchedules()
    {
        //get carrier names for all plans without an allowed fee schedule that are also not hidden.
        var command = "SELECT carrier.CarrierName "
                      + "FROM insplan,carrier "
                      + "WHERE carrier.CarrierNum=insplan.CarrierNum "
                      + "AND insplan.AllowedFeeSched=0 "
                      + "AND insplan.PlanType='' "
                      + "AND insplan.IsHidden='0' "
                      + "GROUP BY carrier.CarrierName";
        var table = DataCore.GetTable(command);
        //loop through all the carrier names
        string carrierName;
        FeeSched feeSched;
        var itemOrder = FeeScheds.GetCount();
        long retVal = 0;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            carrierName = SIn.String(table.Rows[i]["CarrierName"].ToString());
            if (carrierName == "" || carrierName == " ") continue;
            //add a fee schedule if needed
            feeSched = FeeScheds.GetByExactName(carrierName, FeeScheduleType.OutNetwork);
            if (feeSched == null)
            {
                feeSched = new FeeSched();
                feeSched.Description = carrierName;
                feeSched.FeeSchedType = FeeScheduleType.OutNetwork;
                //sched.IsNew=true;
                feeSched.IsGlobal = true;
                feeSched.ItemOrder = itemOrder;
                //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                feeSched.SecUserNumEntry = Security.CurUser.UserNum;
                FeeScheds.Insert(feeSched);
                itemOrder++;
            }

            List<long> listCarrierNums;
            //assign the fee sched to many plans
            //for compatibility with Oracle, get a list of all carrierNums that use the carriername
            command = "SELECT CarrierNum FROM carrier WHERE CarrierName='" + SOut.String(carrierName) + "'";
            listCarrierNums = Db.GetListLong(command);
            if (listCarrierNums.Count == 0) continue; //I don't see how this could happen
            command = "SELECT * FROM insplan "
                      + "WHERE AllowedFeeSched = 0 "
                      + "AND PlanType='' "
                      + "AND IsHidden=0 "
                      + "AND CarrierNum IN (" + string.Join(",", listCarrierNums) + ")";
            var listInsPlans = InsPlanCrud.SelectMany(command);
            command = "UPDATE insplan "
                      + "SET AllowedFeeSched=" + SOut.Long(feeSched.FeeSchedNum) + " "
                      + "WHERE PlanNum IN (" + string.Join(",", listInsPlans.Select(x => x.PlanNum)) + ")";
            retVal += Db.NonQ(command);
            //log updated InsPlan's AllowedFeeSched
            //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            for (var j = 0; j < listInsPlans.Count; j++)
                InsEditLogs.MakeLogEntry("AllowedFeeSched",
                    Security.CurUser.UserNum,
                    "0",
                    SOut.Long(feeSched.FeeSchedNum),
                    InsEditLogType.InsPlan,
                    listInsPlans[j].PlanNum,
                    0,
                    listInsPlans[j].GroupNum + " - " + listInsPlans[j].GroupName);
        }

        return retVal;
    }

    public static int UnusedGetCount()
    {
        var command = "SELECT COUNT(*) FROM insplan WHERE IsHidden=0 "
                      + "AND NOT EXISTS (SELECT * FROM inssub WHERE inssub.PlanNum=insplan.PlanNum)";
        var count = SIn.Int(Db.GetCount(command));
        return count;
    }

    public static void UnusedHideAll()
    {
        var command = "SELECT * FROM insplan "
                      + "WHERE IsHidden=0 "
                      + "AND NOT EXISTS (SELECT * FROM inssub WHERE inssub.PlanNum=insplan.PlanNum)";
        var listInsPlans = InsPlanCrud.SelectMany(command);
        if (listInsPlans.Count == 0) return;
        command = "UPDATE insplan SET IsHidden=1 "
                  + "WHERE PlanNum IN (" + string.Join(",", listInsPlans.Select(x => x.PlanNum)) + ")";
        Db.NonQ(command);
        //log newly hidden InsPlans
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listInsPlans.Count; i++)
            InsEditLogs.MakeLogEntry("IsHidden",
                Security.CurUser.UserNum,
                "0",
                "1",
                InsEditLogType.InsPlan,
                listInsPlans[i].PlanNum,
                0,
                listInsPlans[i].GroupNum + " - " + listInsPlans[i].GroupName);
    }

    /// <summary>
    ///     Returns -1 if no copay feeschedule.  Can return -1 if copay amount is blank.
    ///     Leave lookupFees null to retrieve from db.  If not null, it should contain fees for all possible alternate codes.
    /// </summary>
    public static double GetCopay(long codeNum, long feeSched, long feeSchedCopay, bool isCodeSubstNone, string toothNum, long clinicNum, long provNum,
        long planNum, List<SubstitutionLink> listSubstitutionLinks = null, Lookup<FeeKey2, Fee> lookupFees = null) //allowing null on these in order to not break unit tests
    {
        if (feeSchedCopay == 0) return -1;
        var substCodeNum = codeNum;
        //codeSubstNone, true if the insplan does not allow procedure code downgrade substitution.
        if (!isCodeSubstNone)
            //Plan allows substitution codes.  Get the substitution code if one exists.
            substCodeNum = ProcedureCodes.GetSubstituteCodeNum(ProcedureCodes.GetStringProcCode(codeNum), toothNum, planNum, listSubstitutionLinks); //for posterior composites
        //List<Fee> listFees=lookupFees[new FeeKey2(substCodeNum,copayFeeSched)];//couldn't lookup earlier because we didn't know code.
        List<Fee> listFees = null;
        if (lookupFees != null) listFees = lookupFees[new FeeKey2(substCodeNum, feeSchedCopay)].ToList();
        var retVal = Fees.GetAmount(substCodeNum, feeSchedCopay, clinicNum, provNum, listFees);
        if (retVal == -1)
        {
            //blank co-pay
            if (PrefC.GetBool(PrefName.CoPay_FeeSchedule_BlankLikeZero)) return -1; //will act like zero.  No patient co-pay.

            //The amount from the regular fee schedule
            //In other words, the patient is responsible for procs that are not specified in a managed care fee schedule.
            if (lookupFees != null) listFees = lookupFees[new FeeKey2(substCodeNum, feeSched)].ToList();
            return Fees.GetAmount(substCodeNum, feeSched, clinicNum, provNum, listFees);
        }

        return retVal;
    }

    /// <summary>
    ///     Returns -1 if no allowed feeschedule or fee unknown for this procCode. Otherwise, returns the allowed fee including
    ///     0.
    ///     Can handle a planNum of 0.  Tooth num is used for posterior composites.
    ///     It can be left blank in some situations.  Provider must be supplied in case plan has no assigned fee schedule.
    ///     Then it will use the fee schedule for the provider.
    ///     Leave lookupFees null to retrieve from db.
    /// </summary>
    public static double GetAllowed(string procCodeStr, long feeSched, long feeSchedAllowed, bool isCodeSubstNone, string planType, string toothNum
        , long provNum, long clinicNum, long planNum, List<SubstitutionLink> listSubstitutionLinks = null, Lookup<FeeKey2, Fee> lookupFees = null)
    {
        var codeNum = ProcedureCodes.GetCodeNum(procCodeStr);
        var substCodeNum = codeNum;
        if (!isCodeSubstNone) substCodeNum = ProcedureCodes.GetSubstituteCodeNum(procCodeStr, toothNum, planNum, listSubstitutionLinks); //for posterior composites
        //PPO always returns the PPO fee for the code or substituted code. 
        //Flat copay insurances should only ever pay up to their fee schedule amount, regardless of what the procFee is.
        List<Fee> listFees = null;
        if (planType == "p" || planType == "f")
        {
            if (lookupFees != null) listFees = lookupFees[new FeeKey2(substCodeNum, feeSched)].ToList();
            var allowedSub = Fees.GetAmount(substCodeNum, feeSched, clinicNum, provNum, listFees);
            double allowedNoSub;
            if (codeNum == substCodeNum)
            {
                allowedNoSub = allowedSub;
            }
            else
            {
                if (lookupFees != null) listFees = lookupFees[new FeeKey2(codeNum, feeSched)].ToList();
                allowedNoSub = Fees.GetAmount(codeNum, feeSched, clinicNum, provNum, listFees);
            }

            if (allowedSub == -1 //The fee for the substitution code is blank
                || allowedSub > allowedNoSub) //or the downgrade fee is more expensive than the original fee
                return allowedNoSub; //Use the fee from the original code
            return allowedSub;
        }

        //or, if not PPO, and an allowed fee schedule exists, then we use that.
        if (feeSchedAllowed != 0 && !FeeScheds.GetIsHidden(feeSchedAllowed))
        {
            if (lookupFees != null) listFees = lookupFees[new FeeKey2(substCodeNum, feeSchedAllowed)].ToList();
            return Fees.GetAmount(substCodeNum, feeSchedAllowed, clinicNum, provNum, listFees); //whether post composite or not
        }

        //must be an ordinary fee schedule, so if no substitution code, then no allowed override
        if (codeNum == substCodeNum) return -1;
        //must be posterior composite with an ordinary fee schedule
        //Although it won't happen very often, it's possible that there is no fee schedule assigned to the plan.
        if (feeSched == 0)
        {
            if (provNum == 0)
            {
                //slight corruption
                if (lookupFees != null) listFees = lookupFees[new FeeKey2(substCodeNum, Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv)).FeeSched)].ToList();
                return Fees.GetAmount(substCodeNum, Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv)).FeeSched, clinicNum, provNum, listFees);
            }

            if (lookupFees != null) listFees = lookupFees[new FeeKey2(substCodeNum, Providers.GetProv(provNum).FeeSched)].ToList();
            return Fees.GetAmount(substCodeNum, Providers.GetProv(provNum).FeeSched, clinicNum, provNum, listFees);
        }

        if (lookupFees != null) listFees = lookupFees[new FeeKey2(substCodeNum, feeSched)].ToList();
        return Fees.GetAmount(substCodeNum, feeSched, clinicNum, provNum, listFees);
    }

    public static decimal GetAllowedForProc(Procedure procedure, ClaimProc claimProc, List<InsPlan> listInsPlans, List<SubstitutionLink> listSubstitutionLinks
        , Lookup<FeeKey2, Fee> lookupFees, BlueBookEstimateData blueBookEstimateData = null, Appointment appointment = null)
    {
        //List<Fee> listFees=null) {
        var insPlan = GetPlan(claimProc.PlanNum, listInsPlans);
        decimal carrierAllowedAmount;
        var isCodeSubstNone = !SubstitutionLinks.HasSubstCodeForPlan(insPlan, procedure.CodeNum, listSubstitutionLinks);
        if (blueBookEstimateData != null && blueBookEstimateData.IsValidForEstimate(claimProc))
        {
            carrierAllowedAmount = (decimal) blueBookEstimateData.GetAllowed(procedure, lookupFees, isCodeSubstNone, listSubstitutionLinks);
        }
        else
        {
            var provNum = procedure.ProvNum;
            if (insPlan.PlanType == "p" && appointment != null && PrefC.GetBool(PrefName.EnterpriseHygProcUsePriProvFee) && ProcedureCodes.GetProcCode(procedure.CodeNum).IsHygiene) provNum = appointment.ProvNum; //If the previous conditions are met, we want to pull the fee from the primary provider instead of the hygienist.
            carrierAllowedAmount = (decimal) GetAllowed(ProcedureCodes.GetStringProcCode(procedure.CodeNum), insPlan.FeeSched, insPlan.AllowedFeeSched,
                isCodeSubstNone, insPlan.PlanType, procedure.ToothNum, provNum, procedure.ClinicNum, insPlan.PlanNum, listSubstitutionLinks, lookupFees);
        }

        if (carrierAllowedAmount == -1) return -1;

        if (carrierAllowedAmount > (decimal) procedure.ProcFee) //if the Dr's UCR is lower than the Carrier's PPO allowed.
            return (decimal) procedure.ProcFeeTotal;

        return carrierAllowedAmount * (decimal) procedure.Quantity;
    }

    public static List<InsPlan> GetByInsSubs(List<long> listInsSubNums)
    {
        if (listInsSubNums == null || listInsSubNums.Count < 1) return new List<InsPlan>();
        var command = "SELECT DISTINCT insplan.* FROM insplan,inssub "
                      + "WHERE insplan.PlanNum=inssub.PlanNum "
                      + "AND inssub.InsSubNum IN (" + string.Join(",", listInsSubNums) + ")";
        return InsPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Used when closing the edit plan window to find all patients using this plan and to update all claimProcs for
    ///     each patient.  This keeps estimates correct.
    /// </summary>
    public static void ComputeEstimatesForTrojanPlan(long planNum)
    {
        //string command="SELECT PatNum FROM patplan WHERE PlanNum="+POut.Long(planNum);
        //The left join will get extra info about each plan, namely the PlanNum.  No need for a GROUP BY.  The PlanNum is used to filter.
        var command = @"SELECT PatNum FROM patplan 
					LEFT JOIN inssub ON patplan.InsSubNum=inssub.InsSubNum
					WHERE inssub.PlanNum=" + SOut.Long(planNum);
        var table = DataCore.GetTable(command);
        var listPatNums = new List<long>();
        for (var i = 0; i < table.Rows.Count; i++) listPatNums.Add(SIn.Long(table.Rows[i][0].ToString()));
        ComputeEstimatesForPatNums(listPatNums);
    }

    /// <summary>
    ///     Used when closing the edit plan window to find all patients using this subscriber and to update all claimProcs
    ///     for each patient.  This keeps estimates correct.
    /// </summary>
    public static void ComputeEstimatesForSubscriber(long subscriber)
    {
        var command = "SELECT DISTINCT PatNum FROM patplan,inssub WHERE Subscriber=" + SOut.Long(subscriber) + " AND patplan.InsSubNum=inssub.InsSubNum";
        var listPatNums = Db.GetListLong(command);
        ComputeEstimatesForPatNums(listPatNums);
    }

    /// <summary>
    ///     Computes estimates for all patients passed. Optionally set hasCompletedProcs true to compute estimates for
    ///     completed procedures that are not associated with a claim.
    /// </summary>
    public static void ComputeEstimatesForPatNums(List<long> listPatNums, bool hasCompletedProcs = false)
    {
        listPatNums = listPatNums.Distinct().ToList();
        for (var i = 0; i < listPatNums.Count; i++)
        {
            var patNum = listPatNums[i];
            var family = Patients.GetFamily(patNum);
            var patient = family.GetPatient(patNum);
            var listProcedures = Procedures.Refresh(patNum);
            //Never waste time computing estimates for deleted procedures.
            listProcedures.RemoveAll(x => x.ProcStatus == ProcStat.D);
            //Remove completed procedures for speed purposes unless the calling method explicitly wants to recalculate estimates on completed procedures.
            if (!hasCompletedProcs) listProcedures.RemoveAll(x => x.ProcStatus == ProcStat.C);
            //Make a list of ProcNums that need claimprocs from the database.
            var listProcNums = listProcedures.Select(x => x.ProcNum).ToList();
            //Only get the claim procs associated with the remaining procedures in the list.
            //Mimics ClaimProcs.Refresh(long PatNum) which orders by LineNumber in the query.
            var listClaimProcs = ClaimProcs.RefreshForProcs(listProcNums).OrderBy(x => x.LineNumber).ToList();
            List<ClaimProc> listClaimProcsAll = null;
            if (hasCompletedProcs)
            {
                //Compute estimates for completed procedures that are NOT associated with a claim.
                listClaimProcsAll = new List<ClaimProc>(listClaimProcs);
                var listProcNumsComplete = listProcedures.Where(x => x.ProcStatus == ProcStat.C).Select(x => x.ProcNum).ToList();
                //Ignore claimprocs associated with a claim and a completed procedure.
                //These are historical claimprocs that should not have estimates recalculated.
                //Users are blocked from dropping insurance plans attached to claims that were created today.
                listClaimProcs.RemoveAll(x => x.ClaimNum > 0 && listProcNumsComplete.Contains(x.ProcNum));
                //Figure out which completed procedures still have claimprocs after removing the ones associated with a claim.
                //Canadian users have been noticing an estimate remaining when there is an equivalent received and recomputing the estimate will remove it.
                var listProcNumsPreserve = listClaimProcs.Where(x => listProcNumsComplete.Contains(x.ProcNum)).Select(x => x.ProcNum).ToList();
                var listProcNumsRemove = listProcNumsComplete.Except(listProcNumsPreserve).ToList();
                //Remove completed procedures from the list of procedures that don't have anymore claimprocs at this point.
                listProcedures.RemoveAll(x => listProcNumsRemove.Contains(x.ProcNum));
            }

            var listInsSubs = InsSubs.RefreshForFam(family);
            var listInsPlans = RefreshForSubList(listInsSubs);
            var listPatPlans = PatPlans.Refresh(patNum);
            var listBenefits = Benefits.Refresh(listPatPlans, listInsSubs);
            var listProcedureCodes = new List<ProcedureCode>();
            for (var p = 0; p < listProcedures.Count; p++)
            {
                var procedureCode = ProcedureCodes.GetProcCode(listProcedures[p].CodeNum);
                listProcedureCodes.Add(procedureCode); //duplicates are ok
            }

            var listSubstitutionLinks = SubstitutionLinks.GetAllForPlans(listInsPlans);
            var discountPlanNum = DiscountPlanSubs.GetDiscountPlanNumForPat(patient.PatNum);
            var listFees = Fees.GetListFromObjects(listProcedureCodes, listProcedures.Select(x => x.MedicalCode).ToList(), listProcedures.Select(x => x.ProvNum).ToList(),
                patient.PriProv, patient.SecProv, patient.FeeSched, listInsPlans, listProcedures.Select(x => x.ClinicNum).ToList(), null, //don't need appts to set proc provs
                listSubstitutionLinks, discountPlanNum);
            Procedures.ComputeEstimatesForAll(patNum, listClaimProcs, listProcedures, listInsPlans, listPatPlans, listBenefits, patient.Age, listInsSubs,
                listClaimProcsAll, false, listSubstitutionLinks, listFees);
            Patients.SetHasIns(patNum);
        }
    }

    /// <summary>
    ///     Throws ApplicationException if any dependencies exist and it is not safe to delete the insurance plan.
    ///     This is quite complex because it also must update all claimprocs for all patients affected by the deletion.
    ///     Also deletes patplans, benefits, and claimprocs.
    ///     If canDeleteInsSub is true and there is only one inssub associated to the plan, it will also delete inssubs.
    ///     This should only really happen when an existing plan is being deleted.
    /// </summary>
    public static void Delete(InsPlan insPlan, bool canDeleteInsSub = true, bool doInsertInsEditLogs = true)
    {
        #region Validation

        //Claims
        var command = "SELECT 1 FROM claim WHERE PlanNum=" + SOut.Long(insPlan.PlanNum) + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan with existing claims."));
        //Claimprocs
        command = "SELECT 1 FROM claimproc "
                  + "WHERE PlanNum=" + SOut.Long(insPlan.PlanNum) + " AND Status!=" + SOut.Int((int) ClaimProcStatus.Estimate) + " " //ignore estimates
                  + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan attached to procedures."));
        //Appointments
        command = "SELECT 1 FROM appointment "
                  + "WHERE (InsPlan1=" + SOut.Long(insPlan.PlanNum) + " OR InsPlan2=" + SOut.Long(insPlan.PlanNum) + ") "
                  + "AND AptStatus IN (" + SOut.Int((int) ApptStatus.Complete) + ","
                  + SOut.Int((int) ApptStatus.Broken) + ","
                  + SOut.Int((int) ApptStatus.PtNote) + ","
                  + SOut.Int((int) ApptStatus.PtNoteCompleted) + ") " //We only care about appt statuses that are excluded in Appointments.UpdateInsPlansForPat()
                  + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan attached to appointments."));
        //PayPlans
        command = "SELECT 1 FROM payplan WHERE PlanNum=" + SOut.Long(insPlan.PlanNum) + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan attached to payment plans."));
        //InsSubs
        //we want the InsSubNum if only 1, otherwise only need to know there's more than one.
        command = "SELECT InsSubNum FROM inssub WHERE PlanNum=" + SOut.Long(insPlan.PlanNum) + " " + DbHelper.LimitAnd(2);
        var listInsSubNums = Db.GetListLong(command);
        if (listInsSubNums.Count > 1) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan with more than one subscriber."));

        if (listInsSubNums.Count == 1 && canDeleteInsSub) //if there's only one inssub, delete it.
            InsSubs.Delete(listInsSubNums[0]); //Checks dependencies first;  If none, deletes the inssub, claimprocs, patplans, and recomputes all estimates.

        #endregion Validation

        command = "SELECT * FROM benefit WHERE PlanNum=" + SOut.Long(insPlan.PlanNum);
        var listBenefits = BenefitCrud.SelectMany(command);
        if (listBenefits.Count > 0)
        {
            command = "DELETE FROM benefit WHERE PlanNum=" + SOut.Long(insPlan.PlanNum);
            Db.NonQ(command);
            //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            if (doInsertInsEditLogs)
                for (var i = 0; i < listBenefits.Count; i++)
                    InsEditLogs.MakeLogEntry(null, listBenefits[i], InsEditLogType.Benefit, Security.CurUser.UserNum); //log benefit deletion
        }

        ClearFkey(insPlan.PlanNum); //Zero securitylog FKey column for rows to be deleted.
        command = "DELETE FROM insplan WHERE PlanNum=" + SOut.Long(insPlan.PlanNum);
        Db.NonQ(command);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        if (doInsertInsEditLogs) InsEditLogs.MakeLogEntry(null, insPlan, InsEditLogType.InsPlan, Security.CurUser.UserNum); //log insplan deletion
        InsVerifies.DeleteByFKey(insPlan.PlanNum, VerifyTypes.InsuranceBenefit);
    }

    /// <summary>
    ///     This changes PlanNum in every place in database where it's used.  It also deletes benefits for the old
    ///     planNum.
    /// </summary>
    public static void ChangeReferences(long planNum, InsPlan insPlanToMergeTo)
    {
        var planNumTo = insPlanToMergeTo.PlanNum;
        string command;
        //change all references to the old plan to point to the new plan.
        //appointment.InsPlan1/2
        command = "UPDATE appointment SET InsPlan1=" + SOut.Long(planNumTo) + " WHERE InsPlan1=" + SOut.Long(planNum);
        Db.NonQ(command);
        command = "UPDATE appointment SET InsPlan2=" + SOut.Long(planNumTo) + " WHERE InsPlan2=" + SOut.Long(planNum);
        Db.NonQ(command);
        //benefit.PlanNum -- DELETE unused
        command = "SELECT * FROM benefit WHERE PlanNum=" + SOut.Long(planNum);
        var listBenefits = BenefitCrud.SelectMany(command);
        command = "DELETE FROM benefit WHERE PlanNum=" + SOut.Long(planNum);
        Db.NonQ(command);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listBenefits.Count; i++) InsEditLogs.MakeLogEntry(null, listBenefits[i], InsEditLogType.Benefit, Security.CurUser.UserNum);
        //claim.PlanNum/PlanNum2
        command = "UPDATE claim SET PlanNum=" + SOut.Long(planNumTo) + " WHERE PlanNum=" + SOut.Long(planNum);
        Db.NonQ(command);
        command = "UPDATE claim SET PlanNum2=" + SOut.Long(planNumTo) + " WHERE PlanNum2=" + SOut.Long(planNum);
        Db.NonQ(command);
        //claimproc.PlanNum
        command = "UPDATE claimproc SET PlanNum=" + SOut.Long(planNumTo) + " WHERE PlanNum=" + SOut.Long(planNum);
        Db.NonQ(command);
        //insbluebook.PlanNum
        if (insPlanToMergeTo.PlanType == "" && insPlanToMergeTo.IsBlueBookEnabled)
            command = $@"
				UPDATE insbluebook 
				SET insbluebook.CarrierNum={SOut.Long(insPlanToMergeTo.CarrierNum)},
					insbluebook.PlanNum={SOut.Long(insPlanToMergeTo.PlanNum)},
					insbluebook.GroupNum='{SOut.String(insPlanToMergeTo.GroupNum)}'
				WHERE PlanNum={SOut.Long(planNum)}";
        else
            command = $"DELETE FROM insbluebook WHERE insbluebook.PlanNum={SOut.Long(planNum)}";
        Db.NonQ(command);
        //etrans.PlanNum
        command = "UPDATE etrans SET PlanNum=" + SOut.Long(planNumTo) + " WHERE PlanNum=" + SOut.Long(planNum);
        Db.NonQ(command);
        //inssub.PlanNum
        command = "UPDATE inssub SET PlanNum=" + SOut.Long(planNumTo) + " WHERE PlanNum=" + SOut.Long(planNum);
        Db.NonQ(command);
        //payplan.PlanNum
        command = "UPDATE payplan SET PlanNum=" + SOut.Long(planNumTo) + " WHERE PlanNum=" + SOut.Long(planNum);
        Db.NonQ(command);
        //the old plan should then be deleted.
    }

    ///<summary>Returns the number of plans affected.</summary>
    public static long SetAllPlansToShowUCR()
    {
        var command = "SELECT * FROM insplan WHERE ClaimsUseUCR = 0";
        var listInsPlans = InsPlanCrud.SelectMany(command);
        command = "UPDATE insplan SET ClaimsUseUCR=1";
        Db.NonQ(command);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listInsPlans.Count; i++) //log insplan ClaimsUseUCR change.
            InsEditLogs.MakeLogEntry("ClaimsUseUCR", Security.CurUser.UserNum, "0", "1", InsEditLogType.InsPlan,
                listInsPlans[i].PlanNum, 0, listInsPlans[i].GroupNum + " - " + listInsPlans[i].GroupName);
        return listInsPlans.Count;
    }

    public static List<InsPlan> GetByCarrierName(string carrierName)
    {
        var command = "SELECT * FROM insplan WHERE CarrierNum IN (SELECT CarrierNum FROM carrier WHERE CarrierName='" + SOut.String(carrierName) + "')";
        return InsPlanCrud.SelectMany(command);
    }

    public static List<long> GetPlanNumsByCarrierNum(long carrierNum)
    {
        var command = "SELECT PlanNum FROM insplan WHERE CarrierNum=" + SOut.Long(carrierNum);
        var table = DataCore.GetTable(command);
        var listPlanNums = new List<long>();
        for (var i = 0; i < table.Rows.Count; i++) listPlanNums.Add(SIn.Long(table.Rows[i]["PlanNum"].ToString()));
        return listPlanNums;
    }

    public static List<InsPlan> GetAllByCarrierNum(long carrierNum)
    {
        return GetAllByCarrierNums(new List<long> {carrierNum});
    }

    public static List<InsPlan> GetAllByCarrierNums(List<long> listCarrierNums)
    {
        if (listCarrierNums.IsNullOrEmpty()) return new List<InsPlan>();

        var command = $"SELECT * FROM insplan WHERE CarrierNum IN({string.Join(",", listCarrierNums.Select(x => SOut.Long(x)))})";
        return InsPlanCrud.SelectMany(command);
    }

    public static void UpdateCobRuleForAll(EnumCobRule enumCobRule)
    {
        var command = "SELECT * FROM insplan WHERE CobRule != " + SOut.Int((int) enumCobRule);
        var listInsPlans = InsPlanCrud.SelectMany(command);
        command = "UPDATE insplan SET CobRule=" + SOut.Int((int) enumCobRule);
        Db.NonQ(command);
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listInsPlans.Count; i++)
            InsEditLogs.MakeLogEntry("CobRule", Security.CurUser.UserNum, listInsPlans[i].CobRule.ToString(), SOut.Int((int) enumCobRule),
                InsEditLogType.InsPlan, listInsPlans[i].PlanNum, 0, listInsPlans[i].GroupNum + " - " + listInsPlans[i].GroupName);
    }

    ///<summary>Checks preference and insurance plan settings to determine if the insurance plan uses UCR fees for exclusions.</summary>
    public static bool UsesUcrFeeForExclusions(ExclusionRule exclusionRule)
    {
        if (exclusionRule != ExclusionRule.UseUcrFee)
            if (exclusionRule != ExclusionRule.PracticeDefault || !PrefC.GetBool(PrefName.InsPlanUseUcrFeeForExclusions))
                return false;

        return true;
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching planNum as FKey and are related to InsPlan.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the InsPlan table type.
    /// </summary>
    public static void ClearFkey(long planNum)
    {
        InsPlanCrud.ClearFkey(planNum);
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching planNums as FKey and are related to InsPlan.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the InsPlan table type.
    /// </summary>
    public static void ClearFkey(List<long> listPlanNums)
    {
        InsPlanCrud.ClearFkey(listPlanNums);
    }

    /// <summary>
    ///     Returns the ortho auto proc code override associated to the plan passed in or returns the default codeNum via
    ///     pref.
    /// </summary>
    public static long GetOrthoAutoProc(InsPlan insPlan)
    {
        if (insPlan.OrthoAutoProcCodeNumOverride != 0) return insPlan.OrthoAutoProcCodeNumOverride;

        return PrefC.GetLong(PrefName.OrthoAutoProcCodeNum);
    }

    /// <summary>
    ///     Searches all appointments for the given invalid InsPlanNum. Sets appointment.Insplan1=0 and appointment.Insplan2=0.
    ///     This method assumes the planNum is invalid (does not exist in insplan table).
    /// </summary>
    public static void ResetAppointmentInsplanNum(long planNum)
    {
        var command = $"SELECT * FROM appointment WHERE appointment.InsPlan1={SOut.Long(planNum)} OR appointment.InsPlan2={SOut.Long(planNum)}";
        var listAppointments = AppointmentCrud.SelectMany(command);
        if (listAppointments.Count == 0) return;
        var listAppointmentsNew = new List<Appointment>();
        Appointment appointmentNew;
        //Clear out the planNum from each of the appointments.
        for (var i = 0; i < listAppointments.Count; i++)
        {
            appointmentNew = listAppointments[i].Copy();
            if (appointmentNew.InsPlan1 == planNum) appointmentNew.InsPlan1 = 0;
            if (appointmentNew.InsPlan2 == planNum) appointmentNew.InsPlan2 = 0;
            listAppointmentsNew.Add(appointmentNew);
        }

        //Update the changes
        Appointments.Sync(listAppointmentsNew, listAppointments, 0);
    }
}