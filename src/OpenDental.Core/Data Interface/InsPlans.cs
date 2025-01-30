using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class InsPlans
{
    public static void Insert(InsPlan insPlan)
    {
        insPlan.SecUserNumEntry = Security.CurUser.UserNum;

        var insPlanOld = insPlan.Copy();
        var planNum = InsPlanCrud.Insert(insPlan);

        InsEditLogs.MakeLogEntry(insPlan, insPlanOld.PlanNum == 0 ? null : insPlanOld, InsEditLogType.InsPlan, insPlan.SecUserNumEntry);
        InsVerifies.Upsert(planNum, VerifyTypes.InsuranceBenefit);
    }

    public static void Update(InsPlan insPlan, InsPlan insPlanOld = null)
    {
        insPlanOld ??= RefreshOne(insPlan.PlanNum);

        InsPlanCrud.Update(insPlan, insPlanOld);
        InsEditLogs.MakeLogEntry(insPlan, insPlanOld, InsEditLogType.InsPlan, Security.CurUser.UserNum);
    }

    public static InsPlan GetPlan(long planNum, List<InsPlan> insPlans)
    {
        if (planNum == 0)
        {
            return null;
        }

        insPlans ??= [];

        var insPlan = insPlans.LastOrDefault(x => x.PlanNum == planNum);

        return insPlan ?? RefreshOne(planNum);
    }

    public static List<InsPlan> GetPlans(List<long> planNums)
    {
        if (planNums == null || planNums.Count == 0)
        {
            return [];
        }

        return InsPlanCrud.SelectMany("SELECT * FROM insplan WHERE PlanNum IN (" + string.Join(",", planNums) + ")");
    }

    public static InsPlan[] GetByTrojanId(string trojanId)
    {
        return InsPlanCrud.SelectMany("SELECT * FROM insplan WHERE TrojanID = '" + SOut.String(trojanId) + "'").ToArray();
    }

    public static InsPlan RefreshOne(long planNum)
    {
        return planNum == 0 ? null : InsPlanCrud.SelectOne("SELECT * FROM insplan WHERE plannum = " + planNum);
    }

    public static List<InsPlan> GetPatientData(List<InsSub> listInsSubs)
    {
        return RefreshForSubList(listInsSubs);
    }

    public static bool DoZeroOutWriteOffOnOtherLimitation(InsPlan insPlan)
    {
        if (insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride == YN.Unknown)
        {
            return PrefC.GetBool(PrefName.InsPlansZeroWriteOffsOnFreqOrAging);
        }

        return insPlan.InsPlansZeroWriteOffsOnFreqOrAgingOverride == YN.Yes;
    }

    public static bool DoZeroOutWriteOffOnAnnualMaxLimitation(InsPlan insPlan)
    {
        if (insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride == YN.Unknown)
        {
            return PrefC.GetBool(PrefName.InsPlansZeroWriteOffsOnAnnualMax);
        }

        return insPlan.InsPlansZeroWriteOffsOnAnnualMaxOverride == YN.Yes;
    }

    public static List<InsPlan> RefreshForSubList(List<InsSub> insSubs)
    {
        if (insSubs == null || insSubs.Count == 0)
        {
            return [];
        }

        return InsPlanCrud.SelectMany("SELECT * FROM insplan WHERE PlanNum IN(" + string.Join(",", insSubs.Select(x => x.PlanNum)) + ")");
    }

    public static bool AreEqualValue(InsPlan insPlanA, InsPlan insPlanB)
    {
        return insPlanA.PlanNum == insPlanB.PlanNum &&
               insPlanA.GroupName == insPlanB.GroupName &&
               insPlanA.GroupNum == insPlanB.GroupNum &&
               insPlanA.PlanNote == insPlanB.PlanNote &&
               insPlanA.FeeSched == insPlanB.FeeSched &&
               insPlanA.PlanType == insPlanB.PlanType &&
               insPlanA.ClaimFormNum == insPlanB.ClaimFormNum &&
               insPlanA.UseAltCode == insPlanB.UseAltCode &&
               insPlanA.ClaimsUseUCR == insPlanB.ClaimsUseUCR &&
               insPlanA.CopayFeeSched == insPlanB.CopayFeeSched &&
               insPlanA.EmployerNum == insPlanB.EmployerNum &&
               insPlanA.CarrierNum == insPlanB.CarrierNum &&
               insPlanA.AllowedFeeSched == insPlanB.AllowedFeeSched &&
               insPlanA.ManualFeeSchedNum == insPlanB.ManualFeeSchedNum &&
               insPlanA.TrojanID == insPlanB.TrojanID &&
               insPlanA.DivisionNo == insPlanB.DivisionNo &&
               insPlanA.IsMedical == insPlanB.IsMedical &&
               insPlanA.FilingCode == insPlanB.FilingCode &&
               insPlanA.DentaideCardSequence == insPlanB.DentaideCardSequence &&
               insPlanA.ShowBaseUnits == insPlanB.ShowBaseUnits &&
               insPlanA.CodeSubstNone == insPlanB.CodeSubstNone &&
               insPlanA.IsHidden == insPlanB.IsHidden &&
               insPlanA.MonthRenew == insPlanB.MonthRenew &&
               insPlanA.FilingCodeSubtype == insPlanB.FilingCodeSubtype &&
               insPlanA.CanadianPlanFlag == insPlanB.CanadianPlanFlag &&
               insPlanA.CobRule == insPlanB.CobRule &&
               insPlanA.HideFromVerifyList == insPlanB.HideFromVerifyList &&
               insPlanA.OrthoType == insPlanB.OrthoType &&
               insPlanA.OrthoAutoProcCodeNumOverride == insPlanB.OrthoAutoProcCodeNumOverride &&
               insPlanA.OrthoAutoProcFreq == insPlanB.OrthoAutoProcFreq &&
               insPlanA.OrthoAutoClaimDaysWait == insPlanB.OrthoAutoClaimDaysWait &&
               insPlanA.OrthoAutoFeeBilled == insPlanB.OrthoAutoFeeBilled &&
               insPlanA.BillingType == insPlanB.BillingType &&
               insPlanA.HasPpoSubstWriteoffs == insPlanB.HasPpoSubstWriteoffs &&
               insPlanA.ExclusionFeeRule == insPlanB.ExclusionFeeRule &&
               insPlanA.IsBlueBookEnabled == insPlanB.IsBlueBookEnabled &&
               insPlanA.InsPlansZeroWriteOffsOnFreqOrAgingOverride == insPlanB.InsPlansZeroWriteOffsOnFreqOrAgingOverride &&
               insPlanA.InsPlansZeroWriteOffsOnAnnualMaxOverride == insPlanB.InsPlansZeroWriteOffsOnAnnualMaxOverride &&
               insPlanA.PerVisitPatAmount == insPlanB.PerVisitPatAmount &&
               insPlanA.PerVisitInsAmount == insPlanB.PerVisitInsAmount;
    }

    public static List<InsPlan> GetForFeeSchedNum(long feeSchedNum)
    {
        return InsPlanCrud.SelectMany("SELECT * FROM insplan WHERE insplan.FeeSched = " + feeSchedNum + " OR insplan.CopayFeeSched=" + feeSchedNum);
    }

    public static string GetDescript(long planNum, Family family, List<InsPlan> insPlans, long insSubNum, List<InsSub> listInsSubs)
    {
        if (planNum == 0)
        {
            return string.Empty;
        }

        var insPlan = GetPlan(planNum, insPlans);
        if (insPlan == null || insPlan.PlanNum == 0)
        {
            return string.Empty;
        }

        var insSub = InsSubs.GetSub(insSubNum, listInsSubs);
        if (insSub == null || insSub.InsSubNum == 0)
        {
            return string.Empty;
        }

        var subscriber = family.GetNameInFamFL(insSub.Subscriber);
        if (subscriber == "")
        {
            subscriber = Patients.GetLim(insSub.Subscriber).GetNameLF();
        }

        var result = "";

        var otherFam = true;
        foreach (var plan in insPlans)
        {
            if (plan.PlanNum == planNum)
            {
                otherFam = false;
            }
        }

        if (otherFam)
        {
            result = "(other fam):";
        }

        var carrier = Carriers.GetCarrier(insPlan.CarrierNum);

        var carrierName = carrier.CarrierName;
        if (carrierName.Length > 20)
        {
            carrierName = carrierName.Substring(0, 20) + "...";
        }

        result += carrierName;
        result += " (" + subscriber + ")";

        return result;
    }

    public static string GetCarrierName(long planNum, List<InsPlan> insPlans)
    {
        var insPlan = GetPlan(planNum, insPlans);
        if (insPlan == null)
        {
            return string.Empty;
        }

        var carrier = Carriers.GetCarrier(insPlan.CarrierNum);

        return carrier.CarrierNum == 0 ? "" : carrier.CarrierName;
    }

    public static double GetPendingDisplay(List<ClaimProcHist> claimProcHists, DateTime dateAsOf, InsPlan insPlan, long patPlanNum, long claimNumExclude, long patNum, long insSubNum, List<Benefit> listBenefits)
    {
        if (insPlan == null)
        {
            return 0;
        }

        var dateRenew = BenefitLogic.ComputeRenewDate(dateAsOf, insPlan.MonthRenew);
        var dateStop = dateRenew.AddYears(1);

        double result = 0;

        foreach (var claimProcHist in claimProcHists)
        {
            if (Benefits.LimitationExistsNotGeneral(listBenefits, insPlan.PlanNum, patPlanNum, claimProcHist.StrProcCode))
            {
                continue;
            }

            if (claimProcHist.PlanNum == insPlan.PlanNum &&
                claimProcHist.InsSubNum == insSubNum &&
                claimProcHist.ClaimNum != claimNumExclude &&
                claimProcHist.ProcDate < dateStop &&
                claimProcHist.ProcDate >= dateRenew &&
                claimProcHist.Status == ClaimProcStatus.NotReceived &&
                claimProcHist.PatNum == patNum)
            {
                result += claimProcHist.Amount;
            }
        }

        return result;
    }

    public static double GetInsUsedDisplay(List<ClaimProcHist> claimProcHists, DateTime dateAsOf, long planNum, long patPlanNum, long claimNumExclude, List<InsPlan> listInsPlans, List<Benefit> listBenefits, long patNum, long insSubNum)
    {
        var insPlan = GetPlan(planNum, listInsPlans);
        if (insPlan == null)
        {
            return 0;
        }

        var dateRenew = BenefitLogic.ComputeRenewDate(dateAsOf, insPlan.MonthRenew);
        var dateStop = dateRenew.AddYears(1);

        double result = 0;

        foreach (var claimProcHist in claimProcHists)
        {
            if (claimProcHist.PlanNum != planNum ||
                claimProcHist.InsSubNum != insSubNum ||
                claimProcHist.ClaimNum == claimNumExclude ||
                claimProcHist.ProcDate.Date >= dateStop ||
                claimProcHist.ProcDate.Date < dateRenew ||
                claimProcHist.PatNum != patNum)
            {
                continue;
            }

            if (Benefits.LimitationExistsNotGeneral(listBenefits, planNum, patPlanNum, claimProcHist.StrProcCode))
            {
                continue;
            }

            if (claimProcHist.Status == ClaimProcStatus.Received ||
                claimProcHist.Status == ClaimProcStatus.Adjustment ||
                claimProcHist.Status == ClaimProcStatus.Supplemental)
            {
                result += claimProcHist.Amount;
            }
        }

        return result;
    }

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

    public static DataTable GetBigList(bool byEmployer, string empName, string carrierName, string groupName, string groupNum, string planNum, string trojanID, bool showHidden, bool isIncludeAll)
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

    public static DataTable GetListFeeCheck(string carrierName, string carrierNameNot, long feeSchedWithout, long feeSchedWith, FeeScheduleType feeScheduleType, string insPlanType = "none")
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
        if (feeSchedWithout != 0) command += "AND insplan." + pFeeSched + " !=" + feeSchedWithout + " ";
        if (feeSchedWith != 0) command += "AND insplan." + pFeeSched + " =" + feeSchedWith + " ";
        command += "ORDER BY carrier.CarrierName,employer.EmpName,insplan.GroupNum";
        return DataCore.GetTable(command);
    }

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
            command += "insplan.FeeSched =" + feeSchedNumNew
                                            + " WHERE insplan.FeeSched !=" + feeSchedNumNew;
        }
        else if (feeScheduleType == FeeScheduleType.OutNetwork)
        {
            command += "insplan.AllowedFeeSched =" + feeSchedNumNew
                                                   + " WHERE insplan.AllowedFeeSched !=" + feeSchedNumNew;
        }
        else if (feeScheduleType == FeeScheduleType.CoPay || feeScheduleType == FeeScheduleType.FixedBenefit)
        {
            command += "insplan.CopayFeeSched =" + feeSchedNumNew;
            command += " WHERE insplan.CopayFeeSched !=" + feeSchedNumNew;
        }
        else if (feeScheduleType == FeeScheduleType.ManualBlueBook)
        {
            command += "insplan.ManualFeeSchedNum =" + feeSchedNumNew
                                                     + " WHERE insplan.ManualFeeSchedNum !=" + feeSchedNumNew;
        }

        command += $" AND insplan.PlanNum IN ({string.Join(",", listInsPlanNums.Select(x => x))})";
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

    public static void ChangeInsPlanTypes(List<long> listInsPlanNums, string newInsPlanType, bool enableBlueBook)
    {
        if (listInsPlanNums.IsNullOrEmpty()) return;

        var command = "UPDATE insplan SET PlanType='" + SOut.String(newInsPlanType) + "'";
        command += ", insplan.IsBlueBookEnabled=" + SOut.Bool(enableBlueBook);
        command += " WHERE insplan.PlanNum IN (" + string.Join(",", listInsPlanNums.Select(x => x)) + ")";
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
        Db.NonQ(command);
    }

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
                      + "SET AllowedFeeSched=" + feeSched.FeeSchedNum + " "
                      + "WHERE PlanNum IN (" + string.Join(",", listInsPlans.Select(x => x.PlanNum)) + ")";
            retVal += Db.NonQ(command);
            //log updated InsPlan's AllowedFeeSched
            //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            for (var j = 0; j < listInsPlans.Count; j++)
                InsEditLogs.MakeLogEntry("AllowedFeeSched",
                    Security.CurUser.UserNum,
                    "0",
                    feeSched.FeeSchedNum.ToString(),
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

    public static double GetCopay(long codeNum, long feeSched, long feeSchedCopay, bool isCodeSubstNone, string toothNum, long clinicNum, long provNum, long planNum, List<SubstitutionLink> listSubstitutionLinks = null, Lookup<FeeKey2, Fee> lookupFees = null)
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

    public static double GetAllowed(string procCodeStr, long feeSched, long feeSchedAllowed, bool isCodeSubstNone, string planType, string toothNum, long provNum, long clinicNum, long planNum, List<SubstitutionLink> listSubstitutionLinks = null, Lookup<FeeKey2, Fee> lookupFees = null)
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

    public static decimal GetAllowedForProc(Procedure procedure, ClaimProc claimProc, List<InsPlan> listInsPlans, List<SubstitutionLink> listSubstitutionLinks, Lookup<FeeKey2, Fee> lookupFees, BlueBookEstimateData blueBookEstimateData = null, Appointment appointment = null)
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
        if (listInsSubNums == null || listInsSubNums.Count < 1) return [];
        var command = "SELECT DISTINCT insplan.* FROM insplan,inssub "
                      + "WHERE insplan.PlanNum=inssub.PlanNum "
                      + "AND inssub.InsSubNum IN (" + string.Join(",", listInsSubNums) + ")";
        return InsPlanCrud.SelectMany(command);
    }

    public static void ComputeEstimatesForTrojanPlan(long planNum)
    {
        //string command="SELECT PatNum FROM patplan WHERE PlanNum="+POut.Long(planNum);
        //The left join will get extra info about each plan, namely the PlanNum.  No need for a GROUP BY.  The PlanNum is used to filter.
        var command = @"SELECT PatNum FROM patplan 
					LEFT JOIN inssub ON patplan.InsSubNum=inssub.InsSubNum
					WHERE inssub.PlanNum=" + planNum;
        var table = DataCore.GetTable(command);
        var listPatNums = new List<long>();
        for (var i = 0; i < table.Rows.Count; i++) listPatNums.Add(SIn.Long(table.Rows[i][0].ToString()));
        ComputeEstimatesForPatNums(listPatNums);
    }

    public static void ComputeEstimatesForSubscriber(long subscriber)
    {
        var command = "SELECT DISTINCT PatNum FROM patplan,inssub WHERE Subscriber=" + subscriber + " AND patplan.InsSubNum=inssub.InsSubNum";
        var listPatNums = Db.GetListLong(command);
        ComputeEstimatesForPatNums(listPatNums);
    }

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

    public static void Delete(InsPlan insPlan, bool canDeleteInsSub = true, bool insertInsEditLogs = true)
    {
        #region Validation

        //Claims
        var command = "SELECT 1 FROM claim WHERE PlanNum=" + insPlan.PlanNum + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan with existing claims."));
        //Claimprocs
        command = "SELECT 1 FROM claimproc "
                  + "WHERE PlanNum=" + insPlan.PlanNum + " AND Status!=" + SOut.Int((int) ClaimProcStatus.Estimate) + " " //ignore estimates
                  + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan attached to procedures."));
        //Appointments
        command = "SELECT 1 FROM appointment "
                  + "WHERE (InsPlan1=" + insPlan.PlanNum + " OR InsPlan2=" + insPlan.PlanNum + ") "
                  + "AND AptStatus IN (" + SOut.Int((int) ApptStatus.Complete) + ","
                  + SOut.Int((int) ApptStatus.Broken) + ","
                  + SOut.Int((int) ApptStatus.PtNote) + ","
                  + SOut.Int((int) ApptStatus.PtNoteCompleted) + ") " //We only care about appt statuses that are excluded in Appointments.UpdateInsPlansForPat()
                  + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan attached to appointments."));
        //PayPlans
        command = "SELECT 1 FROM payplan WHERE PlanNum=" + insPlan.PlanNum + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan attached to payment plans."));
        //InsSubs
        //we want the InsSubNum if only 1, otherwise only need to know there's more than one.
        command = "SELECT InsSubNum FROM inssub WHERE PlanNum=" + insPlan.PlanNum + " " + DbHelper.LimitAnd(2);
        var listInsSubNums = Db.GetListLong(command);
        if (listInsSubNums.Count > 1) throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete a plan with more than one subscriber."));

        if (listInsSubNums.Count == 1 && canDeleteInsSub) //if there's only one inssub, delete it.
            InsSubs.Delete(listInsSubNums[0]); //Checks dependencies first;  If none, deletes the inssub, claimprocs, patplans, and recomputes all estimates.

        #endregion Validation
        
        var benefits = BenefitCrud.SelectMany("SELECT * FROM benefit WHERE PlanNum=" + insPlan.PlanNum);
        if (benefits.Count > 0)
        {
            Db.NonQ("DELETE FROM benefit WHERE PlanNum=" + insPlan.PlanNum);
            
            if (insertInsEditLogs)
            {
                foreach (var benefit in benefits)
                {
                    InsEditLogs.MakeLogEntry(null, benefit, InsEditLogType.Benefit, Security.CurUser.UserNum);
                }
            }
        }

        ClearFkey(insPlan.PlanNum);
        
        Db.NonQ("DELETE FROM insplan WHERE PlanNum=" + insPlan.PlanNum);
        
        if (insertInsEditLogs)
        {
            InsEditLogs.MakeLogEntry(null, insPlan, InsEditLogType.InsPlan, Security.CurUser.UserNum);
        }
        
        InsVerifies.DeleteByFKey(insPlan.PlanNum, VerifyTypes.InsuranceBenefit);
    }

    public static void ChangeReferences(long planNum, InsPlan insPlanToMergeTo)
    {
        var planNumTo = insPlanToMergeTo.PlanNum;

        Db.NonQ("UPDATE appointment SET InsPlan1=" + planNumTo + " WHERE InsPlan1=" + planNum);
        Db.NonQ("UPDATE appointment SET InsPlan2=" + planNumTo + " WHERE InsPlan2=" + planNum);

        var benefits = BenefitCrud.SelectMany("SELECT * FROM benefit WHERE PlanNum=" + planNum);
        
        Db.NonQ("DELETE FROM benefit WHERE PlanNum=" + planNum);

        foreach (var benefit in benefits)
        {
            InsEditLogs.MakeLogEntry(null, benefit, InsEditLogType.Benefit, Security.CurUser.UserNum);
        }

        Db.NonQ("UPDATE claim SET PlanNum=" + planNumTo + " WHERE PlanNum=" + planNum);
        Db.NonQ("UPDATE claim SET PlanNum2=" + planNumTo + " WHERE PlanNum2=" + planNum);
        Db.NonQ("UPDATE claimproc SET PlanNum=" + planNumTo + " WHERE PlanNum=" + planNum);

        string commandText;
        if (insPlanToMergeTo.PlanType == "" && insPlanToMergeTo.IsBlueBookEnabled)
        {
            commandText =
                $"""
                 UPDATE insbluebook 
                 SET insbluebook.CarrierNum={insPlanToMergeTo.CarrierNum},
                 	insbluebook.PlanNum={insPlanToMergeTo.PlanNum},
                 	insbluebook.GroupNum='{SOut.String(insPlanToMergeTo.GroupNum)}'
                 WHERE PlanNum={planNum}
                 """;
        }
        else
        {
            commandText = $"DELETE FROM insbluebook WHERE insbluebook.PlanNum={planNum}";
        }

        Db.NonQ(commandText);

        Db.NonQ("UPDATE etrans SET PlanNum=" + planNumTo + " WHERE PlanNum=" + planNum);
        Db.NonQ("UPDATE inssub SET PlanNum=" + planNumTo + " WHERE PlanNum=" + planNum);
        Db.NonQ("UPDATE payplan SET PlanNum=" + planNumTo + " WHERE PlanNum=" + planNum);
    }

    public static long SetAllPlansToShowUcr()
    {
        var insPlans = InsPlanCrud.SelectMany("SELECT * FROM insplan WHERE ClaimsUseUCR = 0");

        Db.NonQ("UPDATE insplan SET ClaimsUseUCR=1");

        foreach (var insPlan in insPlans)
        {
            InsEditLogs.MakeLogEntry("ClaimsUseUCR",
                Security.CurUser.UserNum, "0", "1", InsEditLogType.InsPlan,
                insPlan.PlanNum, 0,
                insPlan.GroupNum + " - " + insPlan.GroupName);
        }

        return insPlans.Count;
    }

    public static List<InsPlan> GetByCarrierName(string carrierName)
    {
        return InsPlanCrud.SelectMany("SELECT * FROM insplan WHERE CarrierNum IN (SELECT CarrierNum FROM carrier WHERE CarrierName='" + SOut.String(carrierName) + "')");
    }

    public static List<InsPlan> GetAllByCarrierNum(long carrierNum)
    {
        return GetAllByCarrierNums([carrierNum]);
    }

    public static List<InsPlan> GetAllByCarrierNums(List<long> carrierNums)
    {
        return carrierNums.IsNullOrEmpty() ? [] : InsPlanCrud.SelectMany($"SELECT * FROM insplan WHERE CarrierNum IN({string.Join(",", carrierNums)})");
    }

    public static void UpdateCobRuleForAll(EnumCobRule enumCobRule)
    {
        var insPlans = InsPlanCrud.SelectMany("SELECT * FROM insplan WHERE CobRule != " + (int) enumCobRule);

        Db.NonQ("UPDATE insplan SET CobRule=" + (int) enumCobRule);

        foreach (var insPlan in insPlans)
        {
            InsEditLogs.MakeLogEntry("CobRule",
                Security.CurUser.UserNum,
                insPlan.CobRule.ToString(), SOut.Int((int) enumCobRule), InsEditLogType.InsPlan,
                insPlan.PlanNum, 0,
                insPlan.GroupNum + " - " + insPlan.GroupName);
        }
    }

    public static bool UsesUcrFeeForExclusions(ExclusionRule exclusionRule)
    {
        if (exclusionRule == ExclusionRule.UseUcrFee)
        {
            return true;
        }

        return exclusionRule == ExclusionRule.PracticeDefault && PrefC.GetBool(PrefName.InsPlanUseUcrFeeForExclusions);
    }

    public static void ClearFkey(long planNum)
    {
        InsPlanCrud.ClearFkey(planNum);
    }

    public static long GetOrthoAutoProc(InsPlan insPlan)
    {
        return insPlan.OrthoAutoProcCodeNumOverride != 0 ? insPlan.OrthoAutoProcCodeNumOverride : PrefC.GetLong(PrefName.OrthoAutoProcCodeNum);
    }

    public static void ResetAppointmentInsplanNum(long planNum)
    {
        var appointments = AppointmentCrud.SelectMany($"SELECT * FROM appointment WHERE appointment.InsPlan1={planNum} OR appointment.InsPlan2={planNum}");
        if (appointments.Count == 0)
        {
            return;
        }

        var appointmentsNew = new List<Appointment>();
        foreach (var appointment in appointments)
        {
            var appointmentNew = appointment.Copy();

            if (appointmentNew.InsPlan1 == planNum)
            {
                appointmentNew.InsPlan1 = 0;
            }

            if (appointmentNew.InsPlan2 == planNum)
            {
                appointmentNew.InsPlan2 = 0;
            }

            appointmentsNew.Add(appointmentNew);
        }

        Appointments.Sync(appointmentsNew, appointments);
    }
}