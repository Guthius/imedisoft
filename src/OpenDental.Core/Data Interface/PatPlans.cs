using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PatPlans
{
    
    public static List<PatPlan> GetPatientData(long patNum)
    {
        return Refresh(patNum);
    }

    ///<summary>Gets a list of all patplans for a given patient</summary>
    public static List<PatPlan> Refresh(long patNum)
    {
        var command = "SELECT * from patplan"
                      + " WHERE PatNum = " + patNum
                      + " ORDER BY Ordinal";
        return PatPlanCrud.SelectMany(command);
    }

    
    public static void Update(PatPlan patPlan)
    {
        //ordinal was already set using SetOrdinal, but it's harmless to set it again.
        PatPlanCrud.Update(patPlan);
    }

    public static void Update(PatPlan patPlanNew, PatPlan patPlanOld)
    {
        if (patPlanOld == null)
        {
            Update(patPlanNew);
            return;
        }

        PatPlanCrud.Update(patPlanNew, patPlanOld);
    }

    
    public static long Insert(PatPlan patPlan)
    {
        //Cameron_ Possibly create outbound ADT message to update insurance info
        var patPlanNum = PatPlanCrud.Insert(patPlan);
        //Upsert an InsVerify for the patplan to ensure that the patplan can be verified.
        InsVerifies.Upsert(patPlanNum, VerifyTypes.PatientEnrollment);
        InsEditPatLogs.MakeLogEntry(patPlan, null, InsEditPatLogType.PatPlan);
        return patPlanNum;
    }

    /*
    ///<summary>Supply a PatPlan list.  This function loops through the list and returns the plan num of the specified ordinal.  If ordinal not valid, then it returns 0.  The main purpose of this function is so we don't have to check the length of the list.</summary>
    public static long GetPlanNum(List<PatPlan> list,int ordinal) {
        Meth.NoCheckMiddleTierRole();
        for(int i=0;i<list.Count;i++){
            if(list[i].Ordinal==ordinal){
                return list[i].PlanNum;
            }
        }
        return 0;
    }*/

    /// <summary>
    ///     Supply a PatPlan list.  This function loops through the list and returns the insSubNum of the specified
    ///     ordinal.  If ordinal not valid, then it returns 0.  The main purpose of this function is so we don't have to check
    ///     the length of the list.
    /// </summary>
    public static long GetInsSubNum(List<PatPlan> list, int ordinal)
    {
        for (var i = 0; i < list.Count; i++)
            if (list[i].Ordinal == ordinal)
                return list[i].InsSubNum;

        return 0;
    }

    public static PatPlan GetByPatPlanNum(long patPlanNum)
    {
        return PatPlanCrud.SelectOne(patPlanNum);
    }

    public static PatPlan GetByInsSubNum(List<PatPlan> listPatPlans, long insSubNum)
    {
        for (var p = 0; p < listPatPlans.Count; p++)
            if (listPatPlans[p].InsSubNum == insSubNum)
                return listPatPlans[p];

        return null;
    }

    /// <summary>
    ///     Supply a PatPlan list.  This function loops through the list and returns the relationship of the specified
    ///     ordinal.  If ordinal not valid, then it returns self (0).
    /// </summary>
    public static Relat GetRelat(List<PatPlan> list, int ordinal)
    {
        for (var i = 0; i < list.Count; i++)
            if (list[i].Ordinal == ordinal)
                return list[i].Relationship;

        return Relat.Self;
    }

    public static string GetPatID(long subNum, List<PatPlan> patPlans)
    {
        for (var p = 0; p < patPlans.Count; p++)
            if (patPlans[p].InsSubNum == subNum)
                return patPlans[p].PatID;

        return "";
    }

    /// <summary>
    ///     Since there can be multiple patplans for an InsSubNum, you should pass in ONLY patplans for the patient.
    ///     Will return 1 for primary insurance, etc.  Will return 0 if planNum not found in the list.
    /// </summary>
    public static int GetOrdinal(long subNum, List<PatPlan> patPlans)
    {
        for (var p = 0; p < patPlans.Count; p++)
            if (patPlans[p].InsSubNum == subNum)
                return patPlans[p].Ordinal;

        return 0;
    }

    /// <summary>
    ///     Returns the ordinal (1-based) for the patplan matching the given PriSecMed. Returns 0 if no match.
    ///     You must pass ALL plans for the patient into this method.
    /// </summary>
    public static int GetOrdinal(PriSecMed priSecMed, List<PatPlan> PatPlanList, List<InsPlan> planList, List<InsSub> subList)
    {
        var dentalOrdinal = 0;
        var listPatPlanOrdered = PatPlanList.OrderBy(x => x.Ordinal).ToList();
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

    ///<summary>Will return null if subNum not found in the list.</summary>
    public static PatPlan GetFromList(List<PatPlan> patPlans, long subNum)
    {
        for (var p = 0; p < patPlans.Count; p++)
            if (patPlans[p].InsSubNum == subNum)
                return patPlans[p];

        return null;
    }

    public static DateTime GetOrthoNextClaimDate(DateTime currentOrthoClaimDate, DateTime dateFirstOrthoProc
        , OrthoAutoProcFrequency freq, int monthsTreat)
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

    /// <summary>
    ///     Sets the ordinal of the specified patPlan.  Rearranges the other patplans for the patient to keep the ordinal
    ///     sequence contiguous.  Estimates must be recomputed after this.  FormInsPlan currently updates estimates every time
    ///     it closes.  Only used in one place.  Returns the new ordinal.
    /// </summary>
    public static int SetOrdinal(long patPlanNum, int newOrdinal)
    {
        var command = "SELECT PatNum FROM patplan WHERE PatPlanNum=" + SOut.Long(patPlanNum);
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
            command = "UPDATE patplan SET Ordinal=" + SOut.Long(curOrdinal)
                                                    + " WHERE PatPlanNum=" + SOut.Long(patPlans[i].PatPlanNum);
            Db.NonQ(command);
            curOrdinal++;
        }

        command = "UPDATE patplan SET Ordinal=" + SOut.Long(newOrdinal)
                                                + " WHERE PatPlanNum=" + SOut.Long(patPlanNum);
        Db.NonQ(command);
//Cameron_ Possibly create outbound ADT message to update insurance info
        return newOrdinal;
    }

    ///<summary>Loops through the supplied list to find the one patplan needed.</summary>
    public static PatPlan GetFromList(PatPlan[] patPlans, long patPlanNum)
    {
        for (var i = 0; i < patPlans.Length; i++)
            if (patPlans[i].PatPlanNum == patPlanNum)
                return patPlans[i].Copy();

        return null;
    }

    /// <summary>
    ///     Loops through the supplied list to find the one patplanNum needed based on the planNum.  Returns 0 if patient
    ///     is not currently covered by the planNum supplied.
    /// </summary>
    public static long GetPatPlanNum(long subNum, List<PatPlan> patPlanList)
    {
        for (var i = 0; i < patPlanList.Count; i++)
            if (patPlanList[i].InsSubNum == subNum)
                return patPlanList[i].PatPlanNum;

        return 0;
    }

    ///<summary>Gets multiple PatPlans from database. Returns null if not found.</summary>
    public static List<PatPlan> GetPatPlansForApi(int limit, int offset, long patNum, long insSubNum)
    {
        var command = "SELECT * FROM patplan WHERE SecDateTEdit>=" + SOut.DateT(DateTime.MinValue) + " ";
        if (patNum > -1) command += "AND PatNum=" + SOut.Long(patNum) + " ";
        if (insSubNum > -1) command += "AND InsSubNum=" + SOut.Long(insSubNum) + " ";
        command += "ORDER BY PatPlanNum " //same fixed order each time
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return PatPlanCrud.SelectMany(command);
    }

    /*Deprecated
    ///<summary>Gets one patPlanNum directly from database.  Only used once in FormClaimProc.</summary>
    public static long GetPatPlanNum(long patNum,long planNum) {

        string command="SELECT PatPlanNum FROM patplan WHERE PatNum="+POut.Long(patNum)+" AND PlanNum="+POut.Long(planNum);
        return PIn.Long(DataCore.GetScalar(command));
    }*/

    ///<summary>Gets directly from database.  Used by Trojan.</summary>
    public static PatPlan[] GetByPlanNum(long planNum)
    {
        //string command="SELECT * FROM patplan WHERE PlanNum='"+POut.Long(planNum)+"'";
        //The left join will get extra info about each plan, namely the PlanNum.  No need for a GROUP BY.  The PlanNum is used to filter.
        var command = @"SELECT * FROM patplan 
				LEFT JOIN inssub ON patplan.InsSubNum=inssub.InsSubNum
				WHERE inssub.PlanNum=" + SOut.Long(planNum);
        return PatPlanCrud.SelectMany(command).ToArray();
    }

    
    public static int GetCountBySubNum(long insSubNum)
    {
        var command = "SELECT COUNT(*) FROM patplan WHERE InsSubNum='" + SOut.Long(insSubNum) + "'";
        return SIn.Int(Db.GetCount(command));
    }

    public static int GetCountForPatAndInsSub(long insSubNum, long patNum)
    {
        var command = "SELECT COUNT(*) FROM patplan WHERE InsSubNum='" + SOut.Long(insSubNum) + "' "
                      + "AND PatNum='" + SOut.Long(patNum) + "'";
        return SIn.Int(Db.GetCount(command));
    }

    ///<summary>Returns a list of PatNums based on the list of insfilingcode.InsFilingCodeNums passed in.</summary>
    public static List<long> GetPatNumsByInsFilingCodes(List<long> listInsFilingCodeNums)
    {
        if (listInsFilingCodeNums.IsNullOrEmpty()) return new List<long>();

        var command = $@"SELECT PatNum FROM patplan
				INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum
				INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum
				WHERE insplan.FilingCode IN ({string.Join(",", listInsFilingCodeNums)})";
        return Db.GetListLong(command);
    }

    ///<summary>Will return null if none exists.</summary>
    public static PatPlan GetPatPlan(long patNum, int ordinal)
    {
        var command = "SELECT * FROM patplan WHERE PatNum=" + SOut.Long(patNum)
                                                            + " AND Ordinal=" + SOut.Long(ordinal);
        return PatPlanCrud.SelectOne(command);
    }

    ///<summary>Will return an empty list if none exists.</summary>
    public static List<PatPlan> GetPatPlans(List<long> listPatPlanNums)
    {
        var command = "SELECT * FROM patplan WHERE PatPlanNum IN (" + string.Join(",", listPatPlanNums) + ")";
        return PatPlanCrud.SelectMany(command);
    }

    public static List<PatPlan> GetPatPlansForPat(long patNum)
    {
        if (patNum == 0) return new List<PatPlan>();

        var command = "SELECT * FROM patplan WHERE PatNum=" + SOut.Long(patNum);
        return PatPlanCrud.SelectMany(command);
    }

    public static List<PatPlan> GetPatPlansForPats(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count < 1) return new List<PatPlan>();
        var command = "SELECT * FROM patplan WHERE PatNum IN (" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")"
                      + " ORDER BY PatNum,Ordinal";
        return PatPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Deletes the patplan with the specified patPlanNum.  Rearranges the other patplans for the patient to keep the
    ///     ordinal sequence contiguous.  Then, recomputes all estimates for this patient because their coverage is now
    ///     different.  Also sets patient.HasIns to the correct value.
    /// </summary>
    public static void Delete(long patPlanNum)
    {
        var command = "SELECT PatNum FROM patplan WHERE PatPlanNum=" + SOut.Long(patPlanNum);
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
                command = "UPDATE patplan SET Ordinal=" + SOut.Long(patPlans[i].Ordinal - 1)
                                                        + " WHERE PatPlanNum=" + SOut.Long(patPlans[i].PatPlanNum);
                Db.NonQ(command);
                continue;
            }

            if (patPlans[i].PatPlanNum == patPlanNum)
            {
                RemoveAssignedUser(patPlans[i]);
                command = "DELETE FROM patplan WHERE PatPlanNum=" + SOut.Long(patPlanNum);
                Db.NonQ(command);
                command = "DELETE FROM benefit WHERE PatPlanNum=" + SOut.Long(patPlanNum);
                Db.NonQ(command);
                doDecrement = true;
                InsVerifies.DeleteByFKey(patPlanNum, VerifyTypes.PatientEnrollment);
            }
        }

        //Include completed procedures when computing estimates so that they are removed.
        InsPlans.ComputeEstimatesForPatNums(new List<long> {patNum}, true);
//Cameron_ Possibly create outbound ADT message to update insurance info
    }

    /// <summary>
    ///     Removes the assigned user from the InsVerify of the InsPlan that is associated to the PatPlan passed in.
    ///     Will only unassign if the user assigned to the patplan matches the user assigned to the insplan. Used when a plan
    ///     gets deleted.
    /// </summary>
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

    /// <summary>
    ///     Deletes the patplan and benefits with the specified patPlanNum.  Does not rearrange the other patplans for the
    ///     patient.  A patplan must be inserted after this function is called to take the place of the patplan being deleted.
    /// </summary>
    public static void DeleteNonContiguous(long patPlanNum)
    {
        var command = "DELETE FROM patplan WHERE PatPlanNum=" + SOut.Long(patPlanNum);
        Db.NonQ(command);
        command = "DELETE FROM benefit WHERE PatPlanNum=" + SOut.Long(patPlanNum);
        Db.NonQ(command);
        InsVerifies.DeleteByFKey(patPlanNum, VerifyTypes.PatientEnrollment);
    }

    ///<summary>There can be multiple PatPlans returned for a single InsSubNum.</summary>
    public static List<PatPlan> GetListByInsSubNums(List<long> listInsSubNums)
    {
        if (listInsSubNums.IsNullOrEmpty()) return new List<PatPlan>();

        var command = "SELECT * FROM patplan WHERE InsSubNum IN(" + string.Join(",", listInsSubNums) + ")";
        return PatPlanCrud.SelectMany(command);
    }

    ///<summary>Gets all patplans with DateNextClaims that are today or in the past.</summary>
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
				WHERE (patplan.OrthoAutoNextClaimDate > " + SOut.Date(new DateTime(1880, 1, 1)) + " AND patplan.OrthoAutoNextClaimDate <= " + DbHelper.Curdate() + @")
				AND patplan.Ordinal IN (1,2)
				ORDER BY patient.LName,patient.FName,patient.PatNum ";
        //TODO: Consider the edge case where an office falls behind and the patient really needs to create multiple claims.
        //E.g. NextClaimDate = 11/01/2016, today is 12/16/2016, this query would only show the Nov. row, but needs to also show a row for 12/01/2016.
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     Checks all attached inssubs to make sure they have valid insplans. returns true if list is valid. If it
    ///     returns false, it fixed the db, and a new list will be needed.
    /// </summary>
    public static bool IsPatPlanListValid(List<PatPlan> listPatPlan, bool doFixIfInvalid = true, List<InsSub> listInsSubs = null
        , List<InsPlan> listInsPlans = null)
    {
        var isValid = true;
        for (var i = 0; i < listPatPlan.Count; i++)
            if (!InsSubs.ValidatePlanNum(listPatPlan[i].InsSubNum, doFixIfInvalid, listInsSubs, listInsPlans))
                isValid = false;

        return isValid;
    }
}

/// <summary>This is only used in the GetOrdinal method above.</summary>
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