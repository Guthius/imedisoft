using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsSubs
{
	/// <summary>
	///     It's fastest if you supply a sub list that contains the sub, but it also works just fine if it can't initally
	///     locate the sub in the list.  You can supply an empty list.  If still not found, returns a new InsSub. The reason
	///     for the new InsSub is because it is common to immediately get an insplan using inssub.InsSubNum.  And, of course,
	///     that would fail if inssub was null.
	/// </summary>
	public static InsSub GetSub(long insSubNum, List<InsSub> listInsSubs)
    {
        if (insSubNum == 0) return new InsSub();
        if (listInsSubs == null) listInsSubs = new List<InsSub>();
        //get InsSub from list if provided and exists in list, otherwise from db if exists, otherwise return a new InsSub
        //LastOrDefault to preserve old behavior. No other reason.
        var insSub = listInsSubs.LastOrDefault(x => x.InsSubNum == insSubNum);
        if (insSub == null) insSub = GetOne(insSubNum);
        if (insSub == null) return new InsSub();
        return insSub;
    }

    ///<summary>Gets one InsSub from the db.</summary>
    public static InsSub GetOne(long insSubNum)
    {
        return InsSubCrud.SelectOne(insSubNum);
    }


    ///<summary>Gets a list of InsSubs from the db.</summary>
    public static List<InsSub> GetMany(List<long> listInsSubNums)
    {
        if (listInsSubNums == null || listInsSubNums.Count < 1) return new List<InsSub>();
        var command = "SELECT * FROM inssub WHERE InsSubNum IN (" + string.Join(",", listInsSubNums) + ")";
        return InsSubCrud.SelectMany(command);
    }

    ///<summary>Returns a list of InsSubs based on the list of insfilingcode.InsFilingCodeNums passed in.</summary>
    public static List<InsSub> GetManyByInsFilingCodes(List<long> listInsFilingCodeNums)
    {
        if (listInsFilingCodeNums.IsNullOrEmpty()) return new List<InsSub>();

        var command = $@"SELECT * FROM inssub
				INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum
				WHERE insplan.FilingCode IN ({string.Join(",", listInsFilingCodeNums)})";
        return InsSubCrud.SelectMany(command);
    }

    
    public static List<InsSub> GetPatientData(List<Patient> listPatients)
    {
        var family = new Family();
        family.ListPats = listPatients.ToArray();
        return RefreshForFam(family);
    }

    /// <summary>
    ///     Gets new List for the specified family.  The only insSubs it misses are for claims with no current coverage.
    ///     These are handled as needed.
    /// </summary>
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
            command += " A.Subscriber=" + SOut.Long(family.ListPats[i].PatNum);
        }

        //in union, distinct is implied
        command += ") UNION (SELECT B.InsSubNum FROM inssub B,patplan P WHERE B.InsSubNum=P.InsSubNum AND (";
        for (var i = 0; i < family.ListPats.Length; i++)
        {
            if (i > 0) command += " OR";
            command += " P.PatNum=" + SOut.Long(family.ListPats[i].PatNum);
        }

        command += "))) C "
                   + "WHERE D.InsSubNum=C.InsSubNum "
                   + "ORDER BY " + DbHelper.UnionOrderBy("DateEffective");
        return InsSubCrud.SelectMany(command);
    }

    ///<summary>Gets a list of InsSubs where the subscribers are in the passed-in list of patnums.</summary>
    public static List<InsSub> GetListInsSubs(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<InsSub>();

        var command = "SELECT * FROM inssub WHERE inssub.Subscriber IN (" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")";
        return InsSubCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all of the families and their corresponding InsSubs for all families passed in (saves calling RefreshForFam()
    ///     one by one).
    ///     Returns a dictionary of key: family and all of their corresponding value: InsSubs
    /// </summary>
    public static Dictionary<Family, List<InsSub>> GetDictInsSubsForFams(List<Family> listFamilies)
    {
        var dictFamilyInsSubs = new Dictionary<Family, List<InsSub>>();
        if (listFamilies == null || listFamilies.Count < 1) return dictFamilyInsSubs;
        var listPatNums = listFamilies.SelectMany(x => x.ListPats).Select(x => x.PatNum).ToList();
        if (listPatNums == null || listPatNums.Count < 1) return dictFamilyInsSubs;
        //The command is written ina nested fashion in order to be compatible with both MySQL and Oracle.
        var command = "SELECT D.*,C.OnBehalfOf "
                      + "FROM inssub D,((SELECT A.InsSubNum,A.Subscriber AS OnBehalfOf "
                      + "FROM inssub A "
                      + "WHERE A.Subscriber IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")"
                      //in union, distinct is implied
                      + ") UNION (SELECT B.InsSubNum,P.PatNum AS OnBehalfOf "
                      + "FROM inssub B,patplan P "
                      + "WHERE B.InsSubNum=P.InsSubNum AND P.PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + "))"
                      + ") C "
                      + "WHERE D.InsSubNum=C.InsSubNum "
                      + "ORDER BY " + DbHelper.UnionOrderBy("DateEffective");
        var table = DataCore.GetTable(command);
        for (var i = 0; i < listFamilies.Count; i++)
        {
            var listPatNumsFam = listFamilies[i].ListPats.Select(x => x.PatNum).ToList();
            var listInsSubs = new List<InsSub>();
            var listDataRows = table.Select().Where(x => listPatNumsFam.Exists(y => y == SIn.Long(x["OnBehalfOf"].ToString()))).ToList();
            var tableFamilyInsSubs = table.Clone();
            for (var j = 0; j < listDataRows.Count; j++) tableFamilyInsSubs.ImportRow(listDataRows[j]);
            dictFamilyInsSubs[listFamilies[i]] = InsSubCrud.TableToList(tableFamilyInsSubs);
        }

        return dictFamilyInsSubs;
    }

    
    public static long Insert(InsSub insSub)
    {
        return Insert(insSub, false);
    }

    
    public static long Insert(InsSub insSub, bool useExistingPK)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        insSub.SecUserNumEntry = Security.CurUser.UserNum;
        insSub.InsSubNum = InsSubCrud.Insert(insSub, useExistingPK);
        InsEditPatLogs.MakeLogEntry(insSub, null, InsEditPatLogType.Subscriber);
        return insSub.InsSubNum;
    }

    
    public static void Update(InsSub insSub)
    {
        InsSubCrud.Update(insSub);
    }

    ///<summary>Throws exception if dependencies.  Also deletes PatPlans tied to the InsSub.</summary>
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
        command = "SELECT PatPlanNum FROM patplan WHERE InsSubNum = " + SOut.Long(insSubNum);
        table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
            //benefits with this PatPlanNum are also deleted here
            PatPlans.Delete(SIn.Long(table.Rows[i]["PatPlanNum"].ToString()));
        command = "DELETE FROM claimproc WHERE InsSubNum = " + SOut.Long(insSubNum); //Will delete all estimates, but nothing else due to ValidateNoKeys()
        Db.NonQ(command);
        InsSubCrud.Delete(insSubNum);
    }

    ///<summary>Returns true if any PatPlans exist for this InsSub.</summary>
    public static bool ExistPatPlans(long insSubNum)
    {
        var command = "SELECT COUNT(PatPlanNum) FROM patplan WHERE InsSubNum = " + SOut.Long(insSubNum);
        var patPlansExist = Db.GetCount(command) != "0";
        return patPlansExist;
    }

    /// <summary>Will throw an exception if this InsSub is being used anywhere. Set strict true to test against every check.</summary>
    public static void ValidateNoKeys(long insSubNum, bool isStrict)
    {
        var command = "SELECT 1 FROM claim WHERE InsSubNum=" + SOut.Long(insSubNum) + " OR InsSubNum2=" + SOut.Long(insSubNum) + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing claims and so the subscriber cannot be deleted."));
        if (isStrict)
        {
            command = "SELECT 1 FROM claimproc WHERE InsSubNum=" + SOut.Long(insSubNum) + " AND Status!=" + SOut.Int((int) ClaimProcStatus.Estimate) + " " + DbHelper.LimitAnd(1); //ignore estimates
            if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing claim procedures and so the subscriber cannot be deleted."));
        }

        command = "SELECT 1 FROM etrans WHERE InsSubNum=" + SOut.Long(insSubNum) + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing etrans entry and so the subscriber cannot be deleted."));
        command = "SELECT 1 FROM payplan WHERE InsSubNum=" + SOut.Long(insSubNum) + " " + DbHelper.LimitAnd(1);
        if (!string.IsNullOrEmpty(DataCore.GetScalar(command))) throw new ApplicationException(Lans.g("FormInsPlan", "Subscriber has existing insurance linked payment plans and so the subscriber cannot be deleted."));
    }

    /* jsalmon (11/15/2013) Depricated because inssubs should not be blindly deleted.
    ///<summary>A quick delete that is only used when cancelling out of a new edit window.</summary>
    public static void Delete(long insSubNum) {

        Crud.InsSubCrud.Delete(insSubNum);
    }
     */

    ///<summary>Gets a list of InsSubs directly from the database. Used in ODApi.</summary>
    public static List<InsSub> GetInsSubsForApi(int limit, int offset, long planNum, long patNum, DateTime secDateTEdit)
    {
        var command = "SELECT * FROM inssub WHERE SecDateTEdit >= " + SOut.DateT(secDateTEdit) + " ";
        if (patNum > 0) command += "AND Subscriber=" + SOut.Long(patNum) + " ";
        if (planNum > 0) command += "AND PlanNum=" + SOut.Long(planNum) + " ";
        command += "ORDER BY inssubnum " //Ensure order for limit and offset.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return InsSubCrud.SelectMany(command);
    }

    ///<summary>Used in FormInsSelectSubscr to get a list of insplans for one subscriber directly from the database.</summary>
    public static List<InsSub> GetListForSubscriber(long subscriber)
    {
        var command = "SELECT * FROM inssub WHERE Subscriber=" + SOut.Long(subscriber);
        return InsSubCrud.SelectMany(command);
    }

    public static List<InsSub> GetListForPlanNum(long planNum)
    {
        var command = "SELECT * FROM inssub WHERE PlanNum=" + SOut.Long(planNum);
        return InsSubCrud.SelectMany(command);
    }

    /// <summary>
    ///     Only used once.  Gets a count of subscribers from the database that have the specified plan. Used to display
    ///     in the insplan window.  The returned count never includes the inssub that we're viewing.
    /// </summary>
    public static int GetSubscriberCountForPlan(long planNum, bool isExcludedSub)
    {
        var command = "SELECT COUNT(inssub.InsSubNum) "
                      + "FROM inssub "
                      + "WHERE inssub.PlanNum=" + SOut.Long(planNum) + " ";
        var retVal = SIn.Int(Db.GetCount(command));
        if (isExcludedSub) retVal = Math.Max(retVal - 1, 0);
        return retVal;
    }

    /// <summary>
    ///     Only used when there are more than 10,000 subscribers.  Gets a list of subscriber names from the database that
    ///     have the specified plan. Used to display in the insplan window.  The returned list never includes the inssub that
    ///     we're viewing.
    /// </summary>
    public static List<string> GetSubscribersForPlan(long planNum, long insSubNumExclude)
    {
        var command = "SELECT CONCAT(CONCAT(LName,', '),FName) "
                      + "FROM inssub LEFT JOIN patient ON patient.PatNum=inssub.Subscriber "
                      + "WHERE inssub.PlanNum=" + SOut.Long(planNum) + " "
                      + "AND inssub.InsSubNum !=" + SOut.Long(insSubNumExclude) + " "
                      + " ORDER BY LName,FName";
        var table = DataCore.GetTable(command);
        var listSubscriberNames = new List<string>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++) listSubscriberNames.Add(SIn.String(table.Rows[i][0].ToString()));
        return listSubscriberNames;
    }

    /// <summary>
    ///     Called from FormInsPlan when user wants to view a benefit note for other subscribers on a plan.  Should never
    ///     include the current subscriber that the user is editing.  This function will get one note from the database, not
    ///     including blank notes.  If no note can be found, then it returns empty string.
    /// </summary>
    public static string GetBenefitNotes(long planNum, long insSubNumExclude)
    {
        var command = "SELECT BenefitNotes FROM inssub WHERE BenefitNotes != '' AND PlanNum=" + SOut.Long(planNum) + " AND InsSubNum !=" + SOut.Long(insSubNumExclude) + " " + DbHelper.LimitAnd(1);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return "";
        return SIn.String(table.Rows[0][0].ToString());
    }

    ///<summary>Sets all subs to the value passed in. Returns the number of subs affected.</summary>
    public static long SetAllSubsAssignBen(bool isAssignBen)
    {
        var command = "UPDATE inssub SET AssignBen=" + SOut.Bool(isAssignBen) + " WHERE AssignBen!=" + SOut.Bool(isAssignBen);
        return Db.NonQ(command);
    }

    /// <summary>
    ///     This will assign all PlanNums to new value when Create New Plan If Needed is selected and there are multiple
    ///     subscribers to a plan and an inssub object has been updated to point at a new PlanNum.  The PlanNum values need to
    ///     be reflected in the claim, claimproc, payplan, and etrans tables, since those all both store inssub.InsSubNum and
    ///     insplan.PlanNum.
    /// </summary>
    public static void SynchPlanNumsForNewPlan(InsSub insSub)
    {
        //insbluebook.PlanNum (insbluebook.GroupNum and insbluebook.CarrierNum will be updated in FormInsPlan as needed)
        var command = $@"UPDATE claim
				INNER JOIN insbluebook ON claim.ClaimNum=insbluebook.ClaimNum
				SET insbluebook.PlanNum={SOut.Long(insSub.PlanNum)}
				WHERE claim.InsSubNum={SOut.Long(insSub.InsSubNum)} AND claim.PlanNum!={SOut.Long(insSub.PlanNum)}";
        Db.NonQ(command);
        //claim.PlanNum
        command = "UPDATE claim SET claim.PlanNum=" + SOut.Long(insSub.PlanNum) + " "
                  + "WHERE claim.InsSubNum=" + SOut.Long(insSub.InsSubNum) + " AND claim.PlanNum!=" + SOut.Long(insSub.PlanNum);
        Db.NonQ(command);
        //claim.PlanNum2
        command = "UPDATE claim SET claim.PlanNum2=" + SOut.Long(insSub.PlanNum) + " "
                  + "WHERE claim.InsSubNum2=" + SOut.Long(insSub.InsSubNum) + " AND claim.PlanNum2!=" + SOut.Long(insSub.PlanNum);
        Db.NonQ(command);
        //claimproc.PlanNum
        command = "UPDATE claimproc SET claimproc.PlanNum=" + SOut.Long(insSub.PlanNum) + " "
                  + "WHERE claimproc.InsSubNum=" + SOut.Long(insSub.InsSubNum) + " AND claimproc.PlanNum!=" + SOut.Long(insSub.PlanNum);
        Db.NonQ(command);
        //payplan.PlanNum
        command = "UPDATE payplan SET payplan.PlanNum=" + SOut.Long(insSub.PlanNum) + " "
                  + "WHERE payplan.InsSubNum=" + SOut.Long(insSub.InsSubNum) + " AND payplan.PlanNum!=" + SOut.Long(insSub.PlanNum);
        Db.NonQ(command);
        //etrans.PlanNum, only used if EtransType.BenefitInquiry270 and BenefitResponse271 and Eligibility_CA.
        command = "UPDATE etrans SET etrans.PlanNum=" + SOut.Long(insSub.PlanNum) + " "
                  + "WHERE etrans.InsSubNum!=0 AND etrans.InsSubNum=" + SOut.Long(insSub.InsSubNum) + " AND etrans.PlanNum!=" + SOut.Long(insSub.PlanNum);
        Db.NonQ(command);
    }

    ///<summary>Returns the number of subscribers moved.</summary>
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
            var command = "SELECT PatNum FROM patplan WHERE InsSubNum=" + SOut.Long(insSubNumOld);
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
                        command = "DELETE FROM benefit WHERE PatPlanNum=" + SOut.Long(patPlan.PatPlanNum); //Delete patient specific benefits (rare).
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

    ///<summary>This will replace the currently attached InsPlan with an "UNKNOWN CARRIER" InsPlan.</summary>
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

    /// <summary>
    ///     Returns true if the inssub has valid fkey references and no changes were needed.
    ///     Returns false if changes were needed. doFixIfInvalid dictates if changes were actually made, separate from the
    ///     return value.
    ///     If doFixIfInvalid is true, we attempt to delete an inssub with an invalid PlanNum.
    ///     If unable to delete, we set the PlanNum to a new insplan associated to a carrier with the CarrierName of "UNKNOWN
    ///     CARRIER" (this matches DBM logic)
    /// </summary>
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

    /// <summary>
    ///     Validates the inssub of each inssubnum passed in.  Will delete the inssub/create and attach a blank plan if
    ///     needed.
    /// </summary>
    public static bool ValidatePlanNumForList(List<long> listInsSubNums, bool doFixIfInvalid = true)
    {
        var isValid = true;
        for (var i = 0; i < listInsSubNums.Count; i++)
            if (!ValidatePlanNum(listInsSubNums[i], doFixIfInvalid))
                isValid = false;

        return isValid;
    }

    /// <summary>
    ///     Replace the tag "[SubscriberID]" with the insurance subcriber id (found on InsSub) for the patient. If there
    ///     isn't one, replaces the [SubscriberID] tag with empty string and returns the message.
    /// </summary>
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

    #region Misc Methods

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching insSubNum as FKey and are related to InsSub.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the InsSub table type.
    /// </summary>
    public static void ClearFkey(long insSubNum)
    {
        InsSubCrud.ClearFkey(insSubNum);
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching insSubNums as FKey and are related to InsSub.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the InsSub table type.
    /// </summary>
    public static void ClearFkey(List<long> listInsSubNums)
    {
        InsSubCrud.ClearFkey(listInsSubNums);
    }

    #endregion
}