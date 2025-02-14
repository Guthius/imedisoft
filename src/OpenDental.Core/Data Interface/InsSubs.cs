using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class InsSubs
{
    public static InsSub GetSub(long insSubNum, List<InsSub> listInsSubs)
    {
        if (insSubNum == 0) return new InsSub();
        if (listInsSubs == null) listInsSubs = [];
        //get InsSub from list if provided and exists in list, otherwise from db if exists, otherwise return a new InsSub
        //LastOrDefault to preserve old behavior. No other reason.
        var insSub = listInsSubs.LastOrDefault(x => x.InsSubNum == insSubNum);
        if (insSub == null) insSub = GetOne(insSubNum);
        if (insSub == null) return new InsSub();
        return insSub;
    }

    public static InsSub GetOne(long insSubNum)
    {
        return InsSubCrud.SelectOne(insSubNum);
    }

    public static List<InsSub> GetMany(List<long> listInsSubNums)
    {
        if (listInsSubNums == null || listInsSubNums.Count < 1) return [];
        var command = "SELECT * FROM inssub WHERE InsSubNum IN (" + string.Join(",", listInsSubNums) + ")";
        return InsSubCrud.SelectMany(command);
    }

    public static List<InsSub> GetPatientData(List<Patient> listPatients)
    {
        var family = new Family();
        family.ListPats = listPatients.ToArray();
        return RefreshForFam(family);
    }

    public static List<InsSub> RefreshForFam(Family family)
    {
        //The command is written in a nested fashion in order to be compatible with both MySQL and Oracle.
        var command =
            "SELECT D.* FROM inssub D," +
            "((SELECT A.InsSubNum FROM inssub A WHERE";
        //subscribers in family
        for (var i = 0; i < family.ListPats.Length; i++)
        {
            if (i > 0) command += " OR";
            command += " A.Subscriber=" + family.ListPats[i].PatNum;
        }

        //in union, distinct is implied
        command += ") UNION (SELECT B.InsSubNum FROM inssub B,patplan P WHERE B.InsSubNum=P.InsSubNum AND (";
        for (var i = 0; i < family.ListPats.Length; i++)
        {
            if (i > 0) command += " OR";
            command += " P.PatNum=" + family.ListPats[i].PatNum;
        }

        command += "))) C "
                   + "WHERE D.InsSubNum=C.InsSubNum "
                   + "ORDER BY " + DbHelper.UnionOrderBy("DateEffective");
        return InsSubCrud.SelectMany(command);
    }

    public static List<InsSub> GetListInsSubs(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return [];

        var command = "SELECT * FROM inssub WHERE inssub.Subscriber IN (" + string.Join(",", listPatNums.Select(x => x)) + ")";
        return InsSubCrud.SelectMany(command);
    }

    public static long Insert(InsSub insSub)
    {
        return Insert(insSub, false);
    }

    public static long Insert(InsSub insSub, bool useExistingPK)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        insSub.SecUserNumEntry = Security.CurUser.UserNum;
        insSub.InsSubNum = InsSubCrud.Insert(insSub);
        InsEditPatLogs.MakeLogEntry(insSub, null, InsEditPatLogType.Subscriber);
        return insSub.InsSubNum;
    }

    public static void Update(InsSub insSub)
    {
        InsSubCrud.Update(insSub);
    }

    public static void Delete(long insSubNum)
    {
        try
        {
            ValidateNoKeys(insSubNum, true);
        }
        catch (ApplicationException ex)
        {
            throw new ApplicationException(Lans.g("FormInsPlan", "Not allowed to delete: ") + ex.Message);
        }

        string command;
        DataTable table;
        //Remove from the patplan table just in case it is still there.
        command = "SELECT PatPlanNum FROM patplan WHERE InsSubNum = " + insSubNum;
        table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
            //benefits with this PatPlanNum are also deleted here
            PatPlans.Delete(SIn.Long(table.Rows[i]["PatPlanNum"].ToString()));
        command = "DELETE FROM claimproc WHERE InsSubNum = " + insSubNum; //Will delete all estimates, but nothing else due to ValidateNoKeys()
        Db.NonQ(command);
        InsSubCrud.Delete(insSubNum);
    }

    public static void ValidateNoKeys(long insSubNum, bool isStrict)
    {
        var command = "SELECT 1 FROM claim WHERE InsSubNum=" + insSubNum + " OR InsSubNum2=" + insSubNum + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing claims and so the subscriber cannot be deleted."));
        if (isStrict)
        {
            command = "SELECT 1 FROM claimproc WHERE InsSubNum=" + insSubNum + " AND Status!=" + SOut.Int((int) ClaimProcStatus.Estimate) + " " + DbHelper.LimitAnd(1); //ignore estimates
            if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing claim procedures and so the subscriber cannot be deleted."));
        }

        command = "SELECT 1 FROM etrans WHERE InsSubNum=" + insSubNum + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing etrans entry and so the subscriber cannot be deleted."));
        command = "SELECT 1 FROM payplan WHERE InsSubNum=" + insSubNum + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing insurance linked payment plans and so the subscriber cannot be deleted."));
    }

    public static List<InsSub> GetListForSubscriber(long subscriber)
    {
        var command = "SELECT * FROM inssub WHERE Subscriber=" + subscriber;
        return InsSubCrud.SelectMany(command);
    }

    public static List<InsSub> GetListForPlanNum(long planNum)
    {
        var command = "SELECT * FROM inssub WHERE PlanNum=" + planNum;
        return InsSubCrud.SelectMany(command);
    }

    public static int GetSubscriberCountForPlan(long planNum, bool isExcludedSub)
    {
        var command = "SELECT COUNT(inssub.InsSubNum) "
                      + "FROM inssub "
                      + "WHERE inssub.PlanNum=" + planNum + " ";
        var retVal = SIn.Int(Db.GetCount(command));
        if (isExcludedSub) retVal = Math.Max(retVal - 1, 0);
        return retVal;
    }

    public static List<string> GetSubscribersForPlan(long planNum, long insSubNumExclude)
    {
        var command = "SELECT CONCAT(CONCAT(LName,', '),FName) "
                      + "FROM inssub LEFT JOIN patient ON patient.PatNum=inssub.Subscriber "
                      + "WHERE inssub.PlanNum=" + planNum + " "
                      + "AND inssub.InsSubNum !=" + insSubNumExclude + " "
                      + " ORDER BY LName,FName";
        var table = DataCore.GetTable(command);
        var listSubscriberNames = new List<string>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++) listSubscriberNames.Add(SIn.String(table.Rows[i][0].ToString()));
        return listSubscriberNames;
    }

    public static string GetBenefitNotes(long planNum, long insSubNumExclude)
    {
        var command = "SELECT BenefitNotes FROM inssub WHERE BenefitNotes != '' AND PlanNum=" + planNum + " AND InsSubNum !=" + insSubNumExclude + " " + DbHelper.LimitAnd(1);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return "";
        return SIn.String(table.Rows[0][0].ToString());
    }

    public static long SetAllSubsAssignBen(bool isAssignBen)
    {
        var command = "UPDATE inssub SET AssignBen=" + SOut.Bool(isAssignBen) + " WHERE AssignBen!=" + SOut.Bool(isAssignBen);
        return Db.NonQ(command);
    }

    public static void SynchPlanNumsForNewPlan(InsSub insSub)
    {
        //insbluebook.PlanNum (insbluebook.GroupNum and insbluebook.CarrierNum will be updated in FormInsPlan as needed)
        var command = $@"UPDATE claim
				INNER JOIN insbluebook ON claim.ClaimNum=insbluebook.ClaimNum
				SET insbluebook.PlanNum={insSub.PlanNum}
				WHERE claim.InsSubNum={insSub.InsSubNum} AND claim.PlanNum!={insSub.PlanNum}";
        Db.NonQ(command);
        //claim.PlanNum
        command = "UPDATE claim SET claim.PlanNum=" + insSub.PlanNum + " "
                  + "WHERE claim.InsSubNum=" + insSub.InsSubNum + " AND claim.PlanNum!=" + insSub.PlanNum;
        Db.NonQ(command);
        //claim.PlanNum2
        command = "UPDATE claim SET claim.PlanNum2=" + insSub.PlanNum + " "
                  + "WHERE claim.InsSubNum2=" + insSub.InsSubNum + " AND claim.PlanNum2!=" + insSub.PlanNum;
        Db.NonQ(command);
        //claimproc.PlanNum
        command = "UPDATE claimproc SET claimproc.PlanNum=" + insSub.PlanNum + " "
                  + "WHERE claimproc.InsSubNum=" + insSub.InsSubNum + " AND claimproc.PlanNum!=" + insSub.PlanNum;
        Db.NonQ(command);
        //payplan.PlanNum
        command = "UPDATE payplan SET payplan.PlanNum=" + insSub.PlanNum + " "
                  + "WHERE payplan.InsSubNum=" + insSub.InsSubNum + " AND payplan.PlanNum!=" + insSub.PlanNum;
        Db.NonQ(command);
        //etrans.PlanNum, only used if EtransType.BenefitInquiry270 and BenefitResponse271 and Eligibility_CA.
        command = "UPDATE etrans SET etrans.PlanNum=" + insSub.PlanNum + " "
                  + "WHERE etrans.InsSubNum!=0 AND etrans.InsSubNum=" + insSub.InsSubNum + " AND etrans.PlanNum!=" + insSub.PlanNum;
        Db.NonQ(command);
    }

    public static long MoveSubscribers(long insPlanNumFrom, long insPlanNumTo)
    {
        var listInsSubsFrom = GetListForPlanNum(insPlanNumFrom);
        var listBlockedPatNums = new List<long>();
        //Perform the same validation as when the user manually drops insplans from FormInsPlan using the Drop button.
        for (var i = 0; i < listInsSubsFrom.Count; i++)
        {
            var insSubFrom = listInsSubsFrom[i];
            var listPatPlansFrom = PatPlans.Refresh(insSubFrom.Subscriber);
            for (var j = 0; j < listPatPlansFrom.Count; j++)
            {
                var patPlanFrom = listPatPlansFrom[j];
                //The following comments and logic are copied from the FormInsPlan Drop button...
                //If they have a claim for this ins with today's date, don't let them drop.
                //We already have code in place to delete claimprocs when we drop ins, but the claimprocs attached to claims are protected.
                //The claim clearly needs to be deleted if they are dropping.  We need the user to delete the claim before they drop the plan.
                //We also have code in place to add new claimprocs when they add the correct insurance.
                var listClaims = Claims.Refresh(patPlanFrom.PatNum); //Get all claims for patient.
                for (var k = 0; k < listClaims.Count; k++)
                {
                    if (listClaims[k].PlanNum != insPlanNumFrom) //Make sure the claim is for the insurance plan we are about to change, not any other plans the patient might have.
                        continue;
                    if (listClaims[k].DateService != DateTime.Today) //not today
                        continue;
                    //Patient currently has a claim for the insplan they are trying to drop.
                    if (!listBlockedPatNums.Contains(patPlanFrom.PatNum)) listBlockedPatNums.Add(patPlanFrom.PatNum);
                }
            }
        }

        if (listBlockedPatNums.Count > 0)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < listBlockedPatNums.Count; i++)
            {
                stringBuilder.Append("\r\n");
                var patient = Patients.GetPat(listBlockedPatNums[i]);
                stringBuilder.Append("#" + listBlockedPatNums[i] + " " + patient.GetNameFLFormal());
            }

            throw new ApplicationException(Lans.g("InsSubs", "Before changing the subscribers on the insurance plan being moved from, please delete all of today's claims related to the insurance plan being moved from for the following patients") + ":" + stringBuilder);
        }

        //This loop mimics some of the logic in PatPlans.Delete().
        var insSubMovedCount = 0;
        for (var i = 0; i < listInsSubsFrom.Count; i++)
        {
            var insSub = listInsSubsFrom[i];
            var insSubNumOld = insSub.InsSubNum;
            insSub.InsSubNum = 0; //This will allow us to insert a new record.
            insSub.PlanNum = insPlanNumTo;
            insSub.DateEffective = DateTime.MinValue;
            insSub.BenefitNotes = "";
            insSub.SubscNote = "";
            //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            insSub.SecUserNumEntry = Security.CurUser.UserNum;
            var insSubNumNew = Insert(insSub);
            var command = "SELECT PatNum FROM patplan WHERE InsSubNum=" + insSubNumOld;
            var tablePatsForInsSub = DataCore.GetTable(command);
            if (tablePatsForInsSub.Rows.Count == 0) continue;
            insSubMovedCount++;
            for (var j = 0; j < tablePatsForInsSub.Rows.Count; j++)
            {
                var patNum = SIn.Long(tablePatsForInsSub.Rows[j]["PatNum"].ToString());
                var listPatPlans = PatPlans.Refresh(patNum);
                for (var k = 0; k < listPatPlans.Count; k++)
                {
                    var patPlan = listPatPlans[k];
                    if (patPlan.InsSubNum == insSubNumOld)
                    {
                        command = "DELETE FROM benefit WHERE PatPlanNum=" + patPlan.PatPlanNum; //Delete patient specific benefits (rare).
                        Db.NonQ(command);
                        patPlan.InsSubNum = insSubNumNew;
                        PatPlans.Update(patPlan);
                    }
                }

                //Now that the plan has changed for the current subscriber, recalculate estimates.
                var prefChanged = false;
                //Forcefully set pref false to prevent creating new estimates for all procs (including completed, sent procs)
                if (Prefs.UpdateBool(PrefName.ClaimProcsAllowedToBackdate, false)) prefChanged = true; //We will turn the preference back on for the user after we finish our computations.
                var family = Patients.GetFamily(patNum);
                var patient = family.GetPatient(patNum);
                var listClaimProcs = ClaimProcs.Refresh(patNum);
                var listProcedures = Procedures.GetProcsByStatusForPat(patNum, ProcStat.TP, ProcStat.TPi);
                listPatPlans = PatPlans.Refresh(patNum);
                var listInsSubs = RefreshForFam(family);
                var listInsPlans = InsPlans.RefreshForSubList(listInsSubs);
                var listBenefits = Benefits.Refresh(listPatPlans, listInsSubs);
                Procedures.ComputeEstimatesForAll(patNum, listClaimProcs, listProcedures, listInsPlans, listPatPlans, listBenefits, patient.Age, listInsSubs);
                if (prefChanged) Prefs.UpdateBool(PrefName.ClaimProcsAllowedToBackdate, true); //set back to original value if changed.
            }
        }

        var insPlanFrom = InsPlans.RefreshOne(insPlanNumFrom);
        var insPlanOld = insPlanFrom.Copy();
        insPlanFrom.IsHidden = true;
        InsPlans.Update(insPlanFrom, insPlanOld);
        return insSubMovedCount;
    }

    public static void AssignBlankPlanToInsSub(InsSub insSub)
    {
        //Will get the plan if it exists, or create a new one and insert if it does not.
        var strCarrierUnknownName = "UNKNOWN CARRIER";
        var insPlan = InsPlans.GetByCarrierName(strCarrierUnknownName).FirstOrDefault();
        //If an UNKNOWN CARRIER plan doesn't exist in the database, we will create it
        if (insPlan == null)
        {
            //Will create a new UNKNOWN CARRIER carrier if it doesn't already exist
            var carrier = Carriers.GetByNameAndPhone(strCarrierUnknownName, "");
            insPlan = new InsPlan();
            insPlan.CarrierNum = carrier.CarrierNum;
            //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            insPlan.SecUserNumEntry = Security.CurUser.UserNum;
            InsPlans.Insert(insPlan); //log taken care of in a subfunction.
        }

        insSub.PlanNum = insPlan.PlanNum;
        Update(insSub);
    }

    public static bool ValidatePlanNum(long insSubNum, bool doFixIfInvalid = true, List<InsSub> listInsSubs = null, List<InsPlan> listInsPlans = null)
    {
        var insSub = GetSub(insSubNum, listInsSubs);
        var subscriberNum = insSub.Subscriber;
        var insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlans);
        if (insPlan != null) //Plan exists.  This means the reference from the inssub is intact.
            return true;
        if (!doFixIfInvalid)
            //There is an invalid PlanNum reference.
            //Don't automatically fix the reference if the caller doesn't want it fixed.
            return false;
        try
        {
            //Clear out the invalid planNum from any existing appointments.
            InsPlans.ResetAppointmentInsplanNum(insSub.PlanNum);
            //The inssub points to an invalid plan, attempt to delete sub
            Delete(insSub.InsSubNum);
            SecurityLogs.MakeLogEntry(EnumPermType.InsPlanEditSub, subscriberNum, "Deleted inssub with invalid insplan attached.");
        }
        catch (Exception ex)
        {
            //Create blank insplan and attach to inssub
            AssignBlankPlanToInsSub(insSub);
            SecurityLogs.MakeLogEntry(EnumPermType.InsPlanEditSub, subscriberNum, "Inssub with invalid insplan found, attached blank insplan.");
        }

        //Return false because the plan wasn't valid when entering the method, although it is now valid.
        return false;
    }

    public static bool ValidatePlanNumForList(List<long> listInsSubNums, bool doFixIfInvalid = true)
    {
        var isValid = true;
        for (var i = 0; i < listInsSubNums.Count; i++)
            if (!ValidatePlanNum(listInsSubNums[i], doFixIfInvalid))
                isValid = false;

        return isValid;
    }

    public static string ReplaceInsSub(string message, InsSub insSub, bool isHtmlEmail = false)
    {
        var stringBuilder = new StringBuilder(message);
        if (insSub == null)
        {
            ReplaceTags.ReplaceOneTag(stringBuilder, "[SubscriberID]", "", isHtmlEmail);
            return stringBuilder.ToString();
        }

        ReplaceTags.ReplaceOneTag(stringBuilder, "[SubscriberID]", insSub.SubscriberID, isHtmlEmail);
        return stringBuilder.ToString();
    }
}