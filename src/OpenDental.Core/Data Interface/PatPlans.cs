using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PatPlans
{
    public static List<PatPlan> GetPatientData(long patNum)
    {
        return Refresh(patNum);
    }

    public static List<PatPlan> Refresh(long patNum)
    {
        return PatPlanCrud.SelectMany("SELECT * from patplan WHERE PatNum = " + patNum + " ORDER BY Ordinal");
    }

    public static void Update(PatPlan patPlan)
    {
        PatPlanCrud.Update(patPlan);
    }

    public static void Update(PatPlan patPlanNew, PatPlan patPlanOld)
    {
        if (patPlanOld is null)
        {
            Update(patPlanNew);
            
            return;
        }

        PatPlanCrud.Update(patPlanNew, patPlanOld);
    }

    public static long Insert(PatPlan patPlan)
    {
        var patPlanNum = PatPlanCrud.Insert(patPlan);
        
        InsVerifies.Upsert(patPlanNum, VerifyTypes.PatientEnrollment);
        InsEditPatLogs.MakeLogEntry(patPlan, null, InsEditPatLogType.PatPlan);
        
        return patPlanNum;
    }

    public static long GetInsSubNum(List<PatPlan> patPlans, int ordinal)
    {
        foreach (var patPlan in patPlans)
        {
            if (patPlan.Ordinal == ordinal)
            {
                return patPlan.InsSubNum;
            }
        }

        return 0;
    }

    public static PatPlan GetByPatPlanNum(long patPlanNum)
    {
        return PatPlanCrud.SelectOne(patPlanNum);
    }

    public static PatPlan GetByInsSubNum(List<PatPlan> patPlans, long insSubNum)
    {
        foreach (var patPlan in patPlans)
        {
            if (patPlan.InsSubNum == insSubNum)
            {
                return patPlan;
            }
        }

        return null;
    }

    public static Relat GetRelat(List<PatPlan> patPlans, int ordinal)
    {
        foreach (var patPlan in patPlans)
        {
            if (patPlan.Ordinal == ordinal)
            {
                return patPlan.Relationship;
            }
        }

        return Relat.Self;
    }

    public static string GetPatID(long subNum, List<PatPlan> patPlans)
    {
        for (var p = 0; p < patPlans.Count; p++)
            if (patPlans[p].InsSubNum == subNum)
                return patPlans[p].PatID;

        return "";
    }

    public static int GetOrdinal(long subNum, List<PatPlan> patPlans)
    {
        for (var p = 0; p < patPlans.Count; p++)
            if (patPlans[p].InsSubNum == subNum)
                return patPlans[p].Ordinal;

        return 0;
    }

    public static int GetOrdinal(PriSecMed priSecMed, List<PatPlan> patPlanList, List<InsPlan> planList, List<InsSub> subList)
    {
        var dentalOrdinal = 0;
        var listPatPlanOrdered = patPlanList.OrderBy(x => x.Ordinal).ToList();
        for (var i = 0; i < listPatPlanOrdered.Count; i++)
        {
            var sub = InsSubs.GetSub(listPatPlanOrdered[i].InsSubNum, subList);
            var plan = InsPlans.GetPlan(sub.PlanNum, planList);
            if (plan.IsMedical)
            {
                if (priSecMed == PriSecMed.Medical) return listPatPlanOrdered[i].Ordinal;
            }
            else
            {
                //dental
                dentalOrdinal++;
                if (dentalOrdinal == 1)
                {
                    if (priSecMed == PriSecMed.Primary) return listPatPlanOrdered[i].Ordinal;
                }
                else if (dentalOrdinal == 2)
                {
                    if (priSecMed == PriSecMed.Secondary) return listPatPlanOrdered[i].Ordinal;
                }
                else if (dentalOrdinal == 3)
                {
                    if (priSecMed == PriSecMed.Tertiary) return listPatPlanOrdered[i].Ordinal;
                }
            }
        }

        return 0;
    }

    public static PatPlan GetFromList(List<PatPlan> patPlans, long subNum)
    {
        for (var p = 0; p < patPlans.Count; p++)
            if (patPlans[p].InsSubNum == subNum)
                return patPlans[p];

        return null;
    }

    public static DateTime GetOrthoNextClaimDate(DateTime currentOrthoClaimDate, DateTime dateFirstOrthoProc, OrthoAutoProcFrequency freq, int monthsTreat)
    {
        //No remotingrole check needed; no call to db.
        var claimDate = currentOrthoClaimDate;
        switch (freq)
        {
            case OrthoAutoProcFrequency.Monthly:
                claimDate = currentOrthoClaimDate.AddMonths(1);
                break;
            case OrthoAutoProcFrequency.Quarterly:
                claimDate = currentOrthoClaimDate.AddMonths(3);
                break;
            case OrthoAutoProcFrequency.SemiAnnual:
                claimDate = currentOrthoClaimDate.AddMonths(6);
                break;
            case OrthoAutoProcFrequency.Annual:
                claimDate = currentOrthoClaimDate.AddYears(1);
                break;
        }

        //If we are passed the total allotted treatment time for this patient, there should be no NextClaimDate.
        if (claimDate > dateFirstOrthoProc.AddMonths(monthsTreat)) claimDate = DateTime.MinValue; //the previous claim send was their last claim.
        return claimDate;
    }

    public static void IncrementOrthoNextClaimDates(PatPlan patPlan, InsPlan insPlan, int monthsTreat, PatientNote patNoteCur)
    {
        var dateFirstOrthoProc = Procedures.GetFirstOrthoProcDate(patNoteCur);
        patPlan.OrthoAutoNextClaimDate = GetOrthoNextClaimDate(patPlan.OrthoAutoNextClaimDate, dateFirstOrthoProc, insPlan.OrthoAutoProcFreq, monthsTreat);
        Update(patPlan);
    }

    public static int SetOrdinal(long patPlanNum, int newOrdinal)
    {
        var command = "SELECT PatNum FROM patplan WHERE PatPlanNum=" + patPlanNum;
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return 1;
        var patNum = SIn.Long(table.Rows[0][0].ToString());
        var patPlans = Refresh(patNum);
        //int oldOrdinal=GetFromList(patPlans,patPlanNum).Ordinal;
        if (newOrdinal > patPlans.Count) newOrdinal = patPlans.Count;
        if (newOrdinal < 1) newOrdinal = 1;
        var curOrdinal = 1;
        for (var i = 0; i < patPlans.Count; i++)
        {
            //Loop through each patPlan.
            if (patPlans[i].PatPlanNum == patPlanNum) continue; //the one we are setting will be handled later
            if (curOrdinal == newOrdinal) curOrdinal++; //skip the newOrdinal when setting the sequence for the others.
            //Create InsEditPatLog for this plan to note the ordinal was changed.
            var patPlanCur = patPlans[i].Copy();
            patPlanCur.Ordinal = (byte) curOrdinal;
            InsEditPatLogs.MakeLogEntry(patPlanCur, patPlans[i], InsEditPatLogType.PatPlan);
            command = "UPDATE patplan SET Ordinal=" + curOrdinal
                                                    + " WHERE PatPlanNum=" + patPlans[i].PatPlanNum;
            Db.NonQ(command);
            curOrdinal++;
        }

        command = "UPDATE patplan SET Ordinal=" + newOrdinal
                                                + " WHERE PatPlanNum=" + patPlanNum;
        Db.NonQ(command);
//Cameron_ Possibly create outbound ADT message to update insurance info
        return newOrdinal;
    }

    public static PatPlan GetFromList(PatPlan[] patPlans, long patPlanNum)
    {
        for (var i = 0; i < patPlans.Length; i++)
            if (patPlans[i].PatPlanNum == patPlanNum)
                return patPlans[i].Copy();

        return null;
    }

    public static long GetPatPlanNum(long subNum, List<PatPlan> patPlanList)
    {
        for (var i = 0; i < patPlanList.Count; i++)
            if (patPlanList[i].InsSubNum == subNum)
                return patPlanList[i].PatPlanNum;

        return 0;
    }

    public static PatPlan[] GetByPlanNum(long planNum)
    {
        //string command="SELECT * FROM patplan WHERE PlanNum='"+POut.Long(planNum)+"'";
        //The left join will get extra info about each plan, namely the PlanNum.  No need for a GROUP BY.  The PlanNum is used to filter.
        var command = @"SELECT * FROM patplan 
				LEFT JOIN inssub ON patplan.InsSubNum=inssub.InsSubNum
				WHERE inssub.PlanNum=" + planNum;
        return PatPlanCrud.SelectMany(command).ToArray();
    }

    public static int GetCountBySubNum(long insSubNum)
    {
        return SIn.Int(Db.GetCount("SELECT COUNT(*) FROM patplan WHERE InsSubNum = " + insSubNum));
    }

    public static int GetCountForPatAndInsSub(long insSubNum, long patNum)
    {
        return SIn.Int(Db.GetCount("SELECT COUNT(*) FROM patplan WHERE InsSubNum = " + insSubNum + " AND PatNum = " + patNum));
    }

    public static List<long> GetPatNumsByInsFilingCodes(List<long> listInsFilingCodeNums)
    {
        if (listInsFilingCodeNums.IsNullOrEmpty()) return [];

        var command = $@"SELECT PatNum FROM patplan
				INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum
				INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum
				WHERE insplan.FilingCode IN ({string.Join(",", listInsFilingCodeNums)})";
        return Db.GetListLong(command);
    }

    public static PatPlan GetPatPlan(long patNum, int ordinal)
    {
        var command = "SELECT * FROM patplan WHERE PatNum=" + patNum
                                                            + " AND Ordinal=" + ordinal;
        return PatPlanCrud.SelectOne(command);
    }

    public static List<PatPlan> GetPatPlans(List<long> listPatPlanNums)
    {
        var command = "SELECT * FROM patplan WHERE PatPlanNum IN (" + string.Join(",", listPatPlanNums) + ")";
        return PatPlanCrud.SelectMany(command);
    }

    public static List<PatPlan> GetPatPlansForPat(long patNum)
    {
        if (patNum == 0) return [];

        var command = "SELECT * FROM patplan WHERE PatNum=" + patNum;
        return PatPlanCrud.SelectMany(command);
    }

    public static List<PatPlan> GetPatPlansForPats(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count < 1) return [];
        var command = "SELECT * FROM patplan WHERE PatNum IN (" + string.Join(",", listPatNums.Select(x => x)) + ")"
                      + " ORDER BY PatNum,Ordinal";
        return PatPlanCrud.SelectMany(command);
    }

    public static void Delete(long patPlanNum)
    {
        var command = "SELECT PatNum FROM patplan WHERE PatPlanNum=" + patPlanNum;
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return;
        var patNum = SIn.Long(table.Rows[0][0].ToString());
        var patPlans = Refresh(patNum);
        var doDecrement = false;
        for (var i = 0; i < patPlans.Count; i++)
        {
            if (doDecrement)
            {
                //patPlan has already been deleted, so decrement the rest.
                //Create InsEditPatLog for this plan to note the ordinal was changed.
                var patPlanCur = patPlans[i].Copy();
                patPlanCur.Ordinal = (byte) (patPlanCur.Ordinal - 1);
                InsEditPatLogs.MakeLogEntry(patPlanCur, patPlans[i], InsEditPatLogType.PatPlan);
                command = "UPDATE patplan SET Ordinal=" + (patPlans[i].Ordinal - 1)
                                                        + " WHERE PatPlanNum=" + patPlans[i].PatPlanNum;
                Db.NonQ(command);
                continue;
            }

            if (patPlans[i].PatPlanNum == patPlanNum)
            {
                RemoveAssignedUser(patPlans[i]);
                command = "DELETE FROM patplan WHERE PatPlanNum=" + patPlanNum;
                Db.NonQ(command);
                command = "DELETE FROM benefit WHERE PatPlanNum=" + patPlanNum;
                Db.NonQ(command);
                doDecrement = true;
                InsVerifies.DeleteByFKey(patPlanNum, VerifyTypes.PatientEnrollment);
            }
        }

        //Include completed procedures when computing estimates so that they are removed.
        InsPlans.ComputeEstimatesForPatNums([patNum], true);
//Cameron_ Possibly create outbound ADT message to update insurance info
    }

    private static void RemoveAssignedUser(PatPlan patPlanCur)
    {
        //Get the insurance verified assigned to the PatPlan.
        var insVerifyForPatPlan = InsVerifies.GetOneByFKey(patPlanCur.PatPlanNum, VerifyTypes.PatientEnrollment);
        if (insVerifyForPatPlan != null && insVerifyForPatPlan.UserNum > 0)
        {
            //Get the insplan associated to the PatPlan.
            InsSub inssub = null;
            if (patPlanCur != null) inssub = InsSubs.GetOne(patPlanCur.InsSubNum);
            InsPlan insPlan = null;
            if (inssub != null) insPlan = InsPlans.RefreshOne(inssub.PlanNum);
            if (insPlan != null)
            {
                //Get the insVerify for the insplan associated to the patplan we are about to delete.
                var insVerifyForInsPlan = InsVerifies.GetOneByFKey(insPlan.PlanNum, VerifyTypes.InsuranceBenefit);
                //Only unassign the user for the insplan if it matches the user for the patplan being dropped
                if (insVerifyForInsPlan != null && insVerifyForInsPlan.UserNum == insVerifyForPatPlan.UserNum)
                {
                    //Remove user and set DateLastVerified to MinValue.
                    insVerifyForInsPlan.UserNum = 0;
                    insVerifyForInsPlan.DateLastAssigned = DateTime.MinValue;
                    InsVerifies.Update(insVerifyForInsPlan);
                }
            }
        }
    }

    public static void DeleteNonContiguous(long patPlanNum)
    {
        Db.NonQ("DELETE FROM patplan WHERE PatPlanNum = " + patPlanNum);
        Db.NonQ("DELETE FROM benefit WHERE PatPlanNum = " + patPlanNum);
        
        InsVerifies.DeleteByFKey(patPlanNum, VerifyTypes.PatientEnrollment);
    }

    public static List<PatPlan> GetListByInsSubNums(List<long> insSubNums)
    {
        if (insSubNums.IsNullOrEmpty()) return [];

        return PatPlanCrud.SelectMany("SELECT * FROM patplan WHERE InsSubNum IN (" + string.Join(",", insSubNums) + ")");
    }

    public static DataTable GetOutstandingOrtho()
    {
        var orthoMonthsTreat = PrefC.GetByte(PrefName.OrthoDefaultMonthsTreat);
        var orthoDefaultAutoCodeNum = PrefC.GetLong(PrefName.OrthoAutoProcCodeNum);
        var listOrthoBandingCodeNums = ProcedureCodes.GetOrthoBandingCodeNums();
        var command = @"
				SELECT CONCAT(patient.LName,', ', patient.FName) Patient, 
					patient.PatNum,
					carrier.CarrierName,
					patplan.OrthoAutoNextClaimDate, 
					IF(patientnote.OrthoMonthsTreatOverride=-1,
						" + orthoMonthsTreat + @",
						patientnote.OrthoMonthsTreatOverride) MonthsTreat,
					patclaims.LastSent, 
					patclaims.NumSent,
					IF(insplan.OrthoAutoProcCodeNumOverride = 0, 
						" + orthoDefaultAutoCodeNum + @",
						insplan.OrthoAutoProcCodeNumOverride) AS AutoCodeNum,
					banding.DateBanding,
					banding.ProvNum,
					banding.ClinicNum,
					patplan.PatPlanNum,
					inssub.InsSubNum,
					insplan.PlanNum
				FROM patplan 
				INNER JOIN inssub ON inssub.InsSubNum = patplan.InsSubNum
				INNER JOIN insplan ON insplan.PlanNum = inssub.PlanNum
					AND insplan.OrthoType = " + SOut.Int((int) OrthoClaimType.InitialPlusPeriodic) + @"
				INNER JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum
				INNER JOIN patient ON patient.PatNum = patplan.PatNum
				LEFT JOIN patientnote ON patientnote.PatNum = patient.PatNum
				LEFT JOIN (
					SELECT MAX(claim.DateSent) LastSent, COUNT(claim.ClaimNum) NumSent, claim.PatNum, claim.InsSubNum, procedurelog.CodeNum
					FROM claim
					INNER JOIN claimproc ON claimproc.ClaimNum = claim.ClaimNum
					INNER JOIN insplan ON claim.PlanNum = insplan.PlanNum
					INNER JOIN procedurelog ON procedurelog.ProcNum = claimproc.ProcNum
						AND procedurelog.CodeNum = 
							IF(insplan.OrthoAutoProcCodeNumOverride = 0, 
							" + orthoDefaultAutoCodeNum + @",
							insplan.OrthoAutoProcCodeNumOverride)
					WHERE claim.ClaimStatus IN ('S','R')
					GROUP BY claim.PatNum, claim.InsSubNum
				)patclaims ON patclaims.PatNum = patplan.PatNum 
					AND patclaims.InsSubNum = patplan.InsSubNum
				LEFT JOIN (
					SELECT procedurelog.PatNum, MIN(procedurelog.ProcDate) AS DateBanding, procedurelog.ProvNum, procedurelog.ClinicNum
					FROM procedurelog ";
        if (listOrthoBandingCodeNums.Count == 0) //The first rendition of ortho auto codes looked for hard coded D codes.
            command += @"INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum
					AND(procedurecode.ProcCode LIKE 'D8080%' OR procedurecode.ProcCode LIKE 'D8090%') ";
        command += @"WHERE procedurelog.ProcStatus = " + SOut.Int((int) ProcStat.C) + " ";
        if (listOrthoBandingCodeNums.Count > 0) command += @"AND procedurelog.CodeNum IN ( " + string.Join(",", listOrthoBandingCodeNums) + @") ";
        command += @"	GROUP BY procedurelog.PatNum
				)banding ON banding.PatNum = patplan.PatNum
				WHERE (patplan.OrthoAutoNextClaimDate > " + SOut.Date(new DateTime(1880, 1, 1)) + " AND patplan.OrthoAutoNextClaimDate <= " + "CURDATE()" + @")
				AND patplan.Ordinal IN (1,2)
				ORDER BY patient.LName,patient.FName,patient.PatNum ";
        //TODO: Consider the edge case where an office falls behind and the patient really needs to create multiple claims.
        //E.g. NextClaimDate = 11/01/2016, today is 12/16/2016, this query would only show the Nov. row, but needs to also show a row for 12/01/2016.
        return DataCore.GetTable(command);
    }

    public static bool IsPatPlanListValid(List<PatPlan> patPlans, bool doFixIfInvalid = true, List<InsSub> listInsSubs = null, List<InsPlan> listInsPlans = null)
    {
        foreach (var patPlan in patPlans)
        {
            if (!InsSubs.ValidatePlanNum(patPlan.InsSubNum, doFixIfInvalid, listInsSubs, listInsPlans))
            {
                return false;
            }
        }

        return true;
    }
}

public enum PriSecMed
{
    ///<summary>Lowest dental ordinal.</summary>
    Primary,

    ///<summary>Second lowest dental ordinal</summary>
    Secondary,

    ///<summary>Third lowest dental ordinal</summary>
    Tertiary,

    ///<summary>Lowest medical ordinal</summary>
    Medical
}