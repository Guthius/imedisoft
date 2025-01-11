using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;
using OpenDentBusiness.Eclaims;
using OpenDentBusiness.Misc;

namespace OpenDentBusiness;


public class ClaimProcs
{
    #region Delete

    /// <summary>
    ///     If the claim has transfers and is then edited in any way after the fact, call this method to remove all
    ///     associate transfer procedures.
    /// </summary>
    public static void RemoveSupplementalTransfersForClaims(long claimNum)
    {
        //A claimnum of zero will scan the entire database, we save MT users a lot of time if we kick out before checking MT.
        if (claimNum == 0) return;
        var command = "DELETE FROM claimproc WHERE ClaimNum = " + SOut.Long(claimNum) + " AND IsTransfer!=0";
        Db.NonQ(command);
    }

    #endregion

    public static List<ClaimProc> GetPatientData(long patNum)
    {
        var command =
            "SELECT * FROM claimproc "
            + "WHERE PatNum = '" + SOut.Long(patNum) + "' ORDER BY LineNumber";
        return ClaimProcCrud.SelectMany(command);
    }

    
    public static List<ClaimProc> Refresh(long patNum)
    {
        var command = "SELECT * FROM claimproc WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY LineNumber";
        return DataCore.GetList(command, ClaimProcCrud.RowToObj);
    }

    ///<summary>Gets the ClaimProcs for a list of patients.</summary>
    public static List<ClaimProc> Refresh(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<ClaimProc>();
        var command =
            "SELECT * FROM claimproc "
            + "WHERE PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")";
        return DataCore.GetList(command, ClaimProcCrud.RowToObj);
    }

    /// <summary>
    ///     For a given PayPlan, returns a list of Claimprocs associated to that PayPlan. Pass in claim proc status for
    ///     filtering.
    /// </summary>
    public static List<ClaimProc> GetForPayPlans(List<long> listPayPlanNums, List<ClaimProcStatus> listClaimProcStatuses = null)
    {
        var command = "SELECT claimproc.* "
                      + "FROM claimproc "
                      + "WHERE claimproc.PayPlanNum IN (" + SOut.String(string.Join(",", listPayPlanNums)) + ") ";
        if (listClaimProcStatuses != null && listClaimProcStatuses.Count > 0) command += "AND claimproc.Status IN (" + string.Join(",", listClaimProcStatuses.Select(x => (int) x)) + ") ";
        command += "ORDER BY claimproc.DateCP";
        var listClaimProcs = ClaimProcCrud.SelectMany(command);
        return listClaimProcs;
    }

    ///<summary>Gets a list of ClaimProcs for one claim.</summary>
    public static List<ClaimProc> RefreshForClaim(long claimNum, List<Procedure> listProceduresForClaim = null, List<ClaimProc> listClaimProcs = null)
    {
        List<ClaimProc> listClaimProcsForClaim;
        if (listClaimProcs == null)
            listClaimProcsForClaim = RefreshForClaims(new List<long> {claimNum}).OrderBy(x => x.LineNumber).ToList();
        else
            listClaimProcsForClaim = listClaimProcs.FindAll(x => x.ClaimNum == claimNum).OrderBy(x => x.LineNumber).ToList();
        //In Canada, we must remove any claimprocs which are directly associated to labs, because labs go out on the same line as the attached parent proc.
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA") && listClaimProcsForClaim.Count > 0)
        {
            if (listProceduresForClaim == null) listProceduresForClaim = Procedures.Refresh(listClaimProcsForClaim[0].PatNum);
            for (var i = 0; i < listProceduresForClaim.Count; i++)
            {
                if (listProceduresForClaim[i].ProcNumLab == 0) continue;
                listClaimProcsForClaim.RemoveAll(x => x.ProcNum == listProceduresForClaim[i].ProcNum);
            }
        }

        return listClaimProcsForClaim;
    }

    ///<summary>Inserts a ClaimProc for the passed in procedure.</summary>
    public static void InsertClaimProcForInsHist(Procedure procedure, long planNum, long insSubNum)
    {
        if (procedure == null) return;
        var claimProc = new ClaimProc();
        claimProc.PatNum = procedure.PatNum;
        claimProc.Status = ClaimProcStatus.InsHist;
        claimProc.PlanNum = planNum;
        claimProc.InsSubNum = insSubNum;
        claimProc.ProcDate = procedure.ProcDate;
        claimProc.ProcNum = procedure.ProcNum;
        claimProc.ProvNum = procedure.ProvNum;
        claimProc.ClinicNum = procedure.ClinicNum;
        Insert(claimProc);
    }

    /// <summary>
    ///     Updates the claimprocs with the date passed in and sets the claimproc status to InsHist. Only changes the status if
    ///     the claimproc matches
    ///     the InsSubNum passed in. The ProcDate gets updated for all claimprocs to keep them in sync with the proccedure.
    /// </summary>
    public static void UpdateClaimProcForInsHist(List<ClaimProc> listClaimProcs, DateTime date, long insSubNum)
    {
        if (listClaimProcs == null || listClaimProcs.Count == 0) return;
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].InsSubNum == insSubNum) listClaimProcs[i].Status = ClaimProcStatus.InsHist; //Only change claimproc to InsHist status if it's for the plan with the ins hist limitations
            listClaimProcs[i].ProcDate = date;
        }

        UpdateMany(listClaimProcs);
    }

    ///<summary>Updates the InsHist claimprocs that have planNumOld for this patNum with the planNumNew.</summary>
    public static void UpdatePlanNumForInsHist(long patNum, long planNumOld, long planNumNew)
    {
        var command = "UPDATE claimproc SET PlanNum=" + SOut.Long(planNumNew) + " "
                      + "WHERE PlanNum=" + SOut.Long(planNumOld) + " "
                      + "AND PatNum=" + SOut.Long(patNum) + " "
                      + "AND Status=" + SOut.Long((int) ClaimProcStatus.InsHist);
        Db.NonQ(command);
    }

    public static List<ClaimProc> RefreshForClaims(List<long> listClaimNums)
    {
        if (listClaimNums.Count == 0) return new List<ClaimProc>();
        listClaimNums = listClaimNums.Distinct().ToList();
        var command =
            "SELECT * FROM claimproc "
            + "WHERE ClaimNum IN(" + string.Join(",", listClaimNums) + ")";
        return ClaimProcCrud.SelectMany(command);
    }

    ///<summary>Grabs columns specifically needed for Claims.GetQueueList() for the sake of speed enhancement.</summary>
    public static List<ClaimProcQueued> GetClaimProcQueuedsForClaims(List<long> listClaimNums)
    {
        if (listClaimNums.Count == 0) return new List<ClaimProcQueued>();
        var command = "SELECT ClaimProcNum,ProcNum,ClaimNum" +
                      " FROM claimproc WHERE ClaimNum IN (" + string.Join(",", listClaimNums) + ")";
        var table = DataCore.GetTable(command);
        var listClaimProcQueueds = new List<ClaimProcQueued>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var dataRow = table.Rows[i];
            var claimProcQueued = new ClaimProcQueued();
            claimProcQueued.ClaimProcNum = SIn.Long(dataRow["ClaimProcNum"].ToString());
            claimProcQueued.ProcNum = SIn.Long(dataRow["ProcNum"].ToString());
            claimProcQueued.ClaimNum = SIn.Long(dataRow["ClaimNum"].ToString());
            listClaimProcQueueds.Add(claimProcQueued);
        }

        return listClaimProcQueueds;
    }

    ///<summary>Gets a list of ClaimProcs with status of estimate.</summary>
    public static List<ClaimProc> RefreshForTP(long patNum)
    {
        var command =
            "SELECT * FROM claimproc "
            + "WHERE (Status=" + SOut.Long((int) ClaimProcStatus.Estimate)
            + " OR Status=" + SOut.Long((int) ClaimProcStatus.CapEstimate) + ") "
            + "AND PatNum = " + SOut.Long(patNum);
        return ClaimProcCrud.SelectMany(command);
    }

    ///<summary>Gets a list of ClaimProcs for one proc.</summary>
    public static List<ClaimProc> RefreshForProc(long procNum)
    {
        var command =
            "SELECT * FROM claimproc "
            + "WHERE ProcNum=" + SOut.Long(procNum);
        return ClaimProcCrud.SelectMany(command);
    }

    ///<summary>Gets a list of ClaimProcs for one proc.</summary>
    public static List<ClaimProc> RefreshForProcs(List<long> listProcNums)
    {
        if (listProcNums == null || listProcNums.Count == 0) return new List<ClaimProc>(); //No point going to middle tier.
        //TODO: Use new CRUD function to prevent issue with IN statement when using Oracle.  Derek will provide function.
        var command =
            "SELECT * FROM claimproc "
            + "WHERE ProcNum IN (" + string.Join(",", listProcNums.Select(x => SOut.Long(x))) + ")";
        return ClaimProcCrud.SelectMany(command);
    }

    
    public static long Insert(ClaimProc claimProc)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        claimProc.SecUserNumEntry = Security.CurUser.UserNum;
        if (claimProc.Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental))
            claimProc.DateSuppReceived = DateTime.Today;
        else //In case someone tried to programmatically set the DateSuppReceived when they shouldn't have
            claimProc.DateSuppReceived = DateTime.MinValue;
        claimProc.SecurityHash = HashFields(claimProc);
        return ClaimProcCrud.Insert(claimProc);
    }

    
    public static void Update(ClaimProc claimProc, ClaimProc claimProcOld = null)
    {
        if (claimProc.Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental) && claimProc.DateSuppReceived.Year < 1880 && (claimProcOld == null || claimProc.Status != claimProcOld.Status))
            claimProc.DateSuppReceived = DateTime.Today;
        else if (!claimProc.Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental) && claimProc.DateSuppReceived.Date == DateTime.Today.Date) claimProc.DateSuppReceived = DateTime.MinValue; //db only field used by one customer and this is how they requested it.  PatNum #19191
        if (claimProcOld == null)
        {
            var claimProcDb = GetOneClaimProc(claimProc.ClaimProcNum);
            if (IsClaimProcHashValid(claimProcDb)) //Only rehash claimprocs that are already valid.
                claimProc.SecurityHash = HashFields(claimProc);
            ClaimProcCrud.Update(claimProc);
        }
        else
        {
            if (IsClaimProcHashValid(claimProcOld)) //Only rehash claimprocs that are already valid.
                claimProc.SecurityHash = HashFields(claimProc);
            ClaimProcCrud.Update(claimProc, claimProcOld);
        }
    }

    ///<summary>Updates the DateCP and ClaimPaymentNum for the list of ClaimProcNums passed in.</summary>
    public static void UpdateForClaimPayment(List<long> listClaimProcNums, ClaimPayment claimPayment)
    {
        if (listClaimProcNums.IsNullOrEmpty()) return;
        var command = @$"
				UPDATE claimproc 
				SET DateCP={SOut.Date(claimPayment.CheckDate)},ClaimPaymentNum={SOut.Long(claimPayment.ClaimPaymentNum)}
				WHERE ClaimProcNum IN ({string.Join(",", listClaimProcNums.Select(x => SOut.Long(x)))})";
        Db.NonQ(command);
    }

    
    public static void Delete(ClaimProc claimProc)
    {
        var command = "DELETE FROM claimproc WHERE ClaimProcNum = " + SOut.Long(claimProc.ClaimProcNum);
        Db.NonQ(command);
    }

    ///<summary>Validates and deletes a claimproc. If there are any dependencies, then this will throw an exception.</summary>
    public static void DeleteAfterValidating(ClaimProc claimProc)
    {
        string command;
        //Can't delete claimprocs for procedures that have Supplemental or Pending Supplemental (Not Received and IsOverpay==true) claimprocs created.
        if (claimProc.ClaimNum != 0 && claimProc.Status != ClaimProcStatus.Supplemental)
        {
            command = "SELECT COUNT(*) FROM claimproc WHERE ProcNum=" + SOut.Long(claimProc.ProcNum) + " AND ClaimNum=" + SOut.Long(claimProc.ClaimNum) + " AND Status=" + (int) ClaimProcStatus.Supplemental;
            var supplementalCP = SIn.Long(Db.GetCount(command));
            if (supplementalCP != 0) throw new ApplicationException(Lans.g("ClaimProcs", "Not allowed to delete this procedure until all supplementals for this procedure are deleted first."));
            command = "SELECT COUNT(*) FROM claimproc WHERE ProcNum=" + SOut.Long(claimProc.ProcNum) + " AND ClaimNum=" + SOut.Long(claimProc.ClaimNum) + " AND Status=" + (int) ClaimProcStatus.NotReceived + " AND IsOverPay=1";
            supplementalCP = SIn.Long(Db.GetCount(command));
            if (supplementalCP != 0) throw new ApplicationException(Lans.g("ClaimProcs", "Not allowed to delete this estimate until all pending supplementals for this procedure are zeroed out first."));
        }

        //Can't delete claimprocs for procedures attached to ortho cases.
        if (OrthoProcLinks.IsProcLinked(claimProc.ProcNum))
            throw new ApplicationException(Lans.g("ClaimProcs", "Not allowed to delete claim procedures attached to ortho cases." +
                                                                " The procedure would need to be detached from the ortho case first."));
        //Validate: make sure this is not the last claimproc on the claim.  If cp is not attached to a claim no need to validate.
        if (claimProc.ClaimNum != 0)
        {
            long remainingCP = 0;
            if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
            {
                command = @$"SELECT claimproc.*
						FROM claimproc 
						INNER JOIN procedurelog ON claimproc.ProcNum=procedurelog.ProcNum
						WHERE claimproc.ClaimNum={SOut.Long(claimProc.ClaimNum)} AND claimproc.ClaimProcNum!={SOut.Long(claimProc.ClaimProcNum)} AND claimproc.ProcNum!=0
							AND procedurelog.ProcNumLab=0"; //Ignore labs, only consider parent procedures.
                remainingCP = Db.GetListLong(command).Count;
            }
            else
            {
                command = "SELECT COUNT(*) FROM claimproc WHERE ClaimNum= " + SOut.Long(claimProc.ClaimNum) + " AND ClaimProcNum!= " + SOut.Long(claimProc.ClaimProcNum) + " AND ProcNum!=0";
                remainingCP = SIn.Long(Db.GetCount(command));
            }

            if (remainingCP == 0) throw new ApplicationException(Lans.g("ClaimProcs", "Not allowed to delete the last procedure from a claim.  The entire claim would have to be deleted."));
        }

        //end of validation
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA") && claimProc.ProcNum != 0)
        {
            var listClaimProcsToDelete = new List<ClaimProc>();
            command = $@"SELECT claimproc.* FROM claimproc 
					INNER JOIN procedurelog on claimproc.ProcNum=procedurelog.ProcNum
					WHERE claimproc.ClaimProcNum={SOut.Long(claimProc.ClaimProcNum)} 
					OR (procedurelog.ProcNumLab={SOut.Long(claimProc.ProcNum)} AND claimproc.Status={SOut.Enum(claimProc.Status)} ";
            if (claimProc.Status == ClaimProcStatus.Supplemental) command += $"AND claimproc.DateCP={SOut.DateT(claimProc.DateCP)}"; //Supplemental claimprocs and their labs are made at the same time
            command += $"AND claimproc.ClaimNum={SOut.Long(claimProc.ClaimNum)})";
            listClaimProcsToDelete = DataCore.GetList(command, ClaimProcCrud.RowToObj);
            DeleteMany(listClaimProcsToDelete);
            return;
        }

        command = "DELETE FROM claimproc WHERE ClaimProcNum=" + SOut.Long(claimProc.ClaimProcNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Deletes claimprocs passed in if they are associated to a dropped patplan for the patnum passed in.
    ///     Usually ran before deleting a claim.
    /// </summary>
    public static void DeleteEstimatesForDroppedPatPlan(List<ClaimProc> listClaimProcs)
    {
        if (listClaimProcs == null || listClaimProcs.Count == 0) return;
        //Check to see if the patient still has the patplan associated to the claimprocs.
        var command = @"SELECT claimproc.ClaimProcNum FROM claimproc
				LEFT JOIN patplan ON patplan.InsSubNum=claimproc.InsSubNum AND patplan.PatNum=claimproc.PatNum
				WHERE claimproc.ClaimProcNum IN(" + string.Join(",", listClaimProcs.Select(x => SOut.Long(x.ClaimProcNum))) + @") 
				AND (claimproc.Status IN(" + string.Join(",", new List<int>
        {
            (int) ClaimProcStatus.NotReceived,
            (int) ClaimProcStatus.CapClaim, (int) ClaimProcStatus.Estimate, (int) ClaimProcStatus.CapEstimate
        }.Select(x => SOut.Long(x))) + @") ";
        //Per TaskNum:3000120, Nathan and Allen approved to delete received claimprocs with $0 InsPayAmts
        command += $"OR (claimproc.Status={SOut.Int((int) ClaimProcStatus.Received)} AND claimproc.InsPayAmt=0)) "
                   + "AND patplan.PatPlanNum IS NULL ";
        //Remove claimprocs that are associated to to a dropped patient's patplan before removing the claim.
        DeleteMany(Db.GetListLong(command));
    }

    /// <summary>
    ///     Used when creating a claim to create any missing claimProcs. Also used in FormProcEdit if click button to add
    ///     Estimate.  Inserts it into db. It will still be altered after this to fill in the fields that actually attach it to
    ///     the claim.
    /// </summary>
    public static void CreateEst(ClaimProc claimProc, Procedure procedure, InsPlan insPlan, InsSub insSub, double baseEstAmt = 0, double insEstTotalAmt = 0,
        bool isInsertNeeded = true, bool isPreauth = false)
    {
        claimProc.ProcNum = procedure.ProcNum;
        //claimnum
        claimProc.PatNum = procedure.PatNum;
        claimProc.ProvNum = procedure.ProvNum;
        if (isPreauth)
        {
            claimProc.Status = ClaimProcStatus.Preauth;
        }
        else if (insPlan.PlanType == "c")
        {
            //capitation
            if (procedure.ProcStatus == ProcStat.C) //complete
                claimProc.Status = ClaimProcStatus.CapComplete; //in this case, a copy will be made later.
            else //usually TP status
                claimProc.Status = ClaimProcStatus.CapEstimate;
        }
        else
        {
            claimProc.Status = ClaimProcStatus.Estimate;
        }

        claimProc.PlanNum = insPlan.PlanNum;
        claimProc.InsSubNum = insSub.InsSubNum;
        //Writeoff=0
        claimProc.AllowedOverride = -1;
        claimProc.Percentage = -1;
        claimProc.PercentOverride = -1;
        claimProc.CopayAmt = -1;
        claimProc.NoBillIns = InsPlanPreferences.NoBillIns(ProcedureCodes.GetProcCode(procedure.CodeNum), insPlan);
        claimProc.PaidOtherIns = -1;
        claimProc.BaseEst = baseEstAmt;
        claimProc.DedEst = -1;
        claimProc.DedEstOverride = -1;
        claimProc.InsEstTotal = insEstTotalAmt;
        claimProc.InsEstTotalOverride = -1;
        claimProc.CopayOverride = -1;
        claimProc.PaidOtherInsOverride = -1;
        claimProc.WriteOffEst = -1;
        claimProc.WriteOffEstOverride = -1;
        claimProc.ClinicNum = procedure.ClinicNum;
        claimProc.EstimateNote = "";
        if (!isPreauth)
        {
            //Capitation procedures are not usually attached to a claim.
            //In order for Aging to calculate properly the ProcDate (Date Completed) and DateCP (Payment Date) must be the same.
            //If the following line of code changes, then we need to preserve this existing behavior specifically for CapComplete.
            claimProc.DateCP = procedure.ProcDate;
            claimProc.ProcDate = procedure.ProcDate;
        }

        if (isInsertNeeded) Insert(claimProc);
    }

    /// <summary>
    ///     Creates and inserts supplemental claimprocs for given listClaimProcs.
    ///     Ignores claimProcs that are not recieved and "By Total" claimProcs.
    /// </summary>
    public static List<ClaimProc> CreateSuppClaimProcs(List<ClaimProc> listClaimProcs, bool isReversalClaim = false, bool isOriginalClaim = true)
    {
        var listClaimProcs2 = new List<ClaimProc>();
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            var claimProc = listClaimProcs[i].Copy(); //Don't modify original list.
            if (claimProc.Status != ClaimProcStatus.Received || claimProc.ProcNum == 0) //Is not received or is a "By Total" payment.
                continue; //Mimics FormClaimEdit.MakeSuppPayment(...) validation logic
            if (isReversalClaim)
                //Used for matching logic, see Hx835_Claim.GetPaymentsForClaimProcs(...), is set to 0 after matching.
                //FeeBilled will be negative for reversals. Negate negative to make it positive.
                claimProc.FeeBilled = -claimProc.FeeBilled;
            else if (isOriginalClaim)
                claimProc.FeeBilled = 0;
            else //Correction
                claimProc.FeeBilled = listClaimProcs[i].FeeBilled;
            if (PayPlans.IsClosed(claimProc.PayPlanNum)) claimProc.PayPlanNum = 0; //detatch the claimproc from closed ins pay plan
            claimProc.ClaimPaymentNum = 0; //no payment attached
            //claimprocnum will be overwritten
            claimProc.DedApplied = 0;
            claimProc.InsPayAmt = 0;
            claimProc.InsPayEst = 0;
            claimProc.Remarks = "";
            claimProc.Status = ClaimProcStatus.Supplemental;
            claimProc.WriteOff = 0;
            claimProc.DateCP = DateTime.Today;
            claimProc.DateEntry = DateTime.Today;
            claimProc.DateInsFinalized = DateTime.MinValue;
            Insert(claimProc); //this inserts a copy of the original with the changes as above.
            listClaimProcs2.Add(claimProc);
        }

        return listClaimProcs2;
    }

    /// <summary>
    ///     This compares the two lists and saves all the changes to the database.  It also removes all the items marked
    ///     doDelete.
    /// </summary>
    public static void Synch(ref List<ClaimProc> listClaimProcs, List<ClaimProc> listClaimProcsOld)
    {
        var listClaimProcsDelete = new List<ClaimProc>();
        var listClaimProcsInsert = new List<ClaimProc>();
        var listClaimProcsUpdate = new List<ClaimProc>();
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].DoDelete)
            {
                listClaimProcsDelete.Add(listClaimProcs[i]);
                continue;
            }

            //new procs
            if (i >= listClaimProcsOld.Count)
            {
                listClaimProcsInsert.Add(listClaimProcs[i]);
                continue;
            }

            //changed procs
            if (!listClaimProcs[i].Equals(listClaimProcsOld[i])) listClaimProcsUpdate.Add(listClaimProcs[i]);
        }

        DeleteMany(listClaimProcsDelete);
        var listClaimProcsNewlyInserted = InsertMany(listClaimProcsInsert);
        //There will be a one-to-one mapping from listInsert to listNewlyInserted.
        for (var i = 0; i < listClaimProcsInsert.Count; i++) listClaimProcsInsert[i].ClaimProcNum = listClaimProcsNewlyInserted[i].ClaimProcNum;
        UpdateMany(listClaimProcsUpdate);
        //go backwards to actually remove the deleted items.
        for (var i = listClaimProcs.Count - 1; i >= 0; i--)
            if (listClaimProcs[i].DoDelete)
                listClaimProcs.RemoveAt(i);
    }

    ///<summary>Gets all as total insurance payments for a family.</summary>
    public static List<ClaimProc> GetByTotForPats(List<long> listPatNums)
    {
        var command = "SELECT * from claimproc WHERE PatNum IN(" + string.Join(", ", listPatNums) + ") "
                      + "AND ProcNum=0 AND (InsPayAmt!=0 OR WriteOff!=0) "
                      + "AND Status IN (" + SOut.Int((int) ClaimProcStatus.Received) + "," + SOut.Int((int) ClaimProcStatus.Supplemental) + "," + SOut.Int((int) ClaimProcStatus.CapClaim) + ")";
        return ClaimProcCrud.SelectMany(command);
    }

    ///<summary>Gets all ClaimProc bundles for the given PayPlanNum. Bundles claimprocs by Date and then by ClaimPaymentNum.</summary>
    public static DataTable GetBundlesForPayPlan(long payPlanNum)
    {
        //MAX functions added to preserve behavior in Oracle.  We may use ProcDate instead of DateCP in the future.
        var command = "SELECT claimproc.ClaimNum,MAX(claimpayment.CheckNum) CheckNum,claimproc.DateCP,MAX(claimpayment.CheckAmt) CheckAmt,claimproc.ClaimPaymentNum,MAX(claimpayment.PayType) PayType, claimproc.ProvNum"
                      + ",SUM(claimproc.InsPayAmt) InsPayAmt "
                      + "FROM claimproc "
                      + "LEFT JOIN claimpayment ON claimproc.ClaimPaymentNum=claimpayment.ClaimPaymentNum "
                      + "WHERE PayPlanNum=" + SOut.Long(payPlanNum) + " "
                      + "AND claimproc.Status IN (" + SOut.Long((long) ClaimProcStatus.Received) + "," + SOut.Long((long) ClaimProcStatus.Supplemental) + "," + SOut.Long((long) ClaimProcStatus.CapClaim) + ") "
                      + "GROUP BY claimproc.ClaimNum,claimproc.DateCP,claimproc.ClaimPaymentNum,claimproc.ProvNum "
                      + "ORDER BY claimproc.DateCP";
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     Gets all ClaimProcs contributing to the -InsEst value for a given family. Mimics aging, specifically the
    ///     Regular Claimproc By DateCP region of Ledgers.GetTransQueryString().
    /// </summary>
    public static DataTable GetClaimProcEstimatesForPatients(List<long> listPatNums)
    {
        var command = "SELECT cp.ClaimProcNum, cp.ProcNum, cp.ClaimNum, cp.PatNum, cp.InsPayEst, cp.WriteOff, p.ProcStatus, p.CodeNum, c.DateService "
                      + "FROM claimproc cp "
                      + "LEFT JOIN procedurelog p ON cp.ProcNum = p.ProcNum "
                      + "LEFT JOIN claim c ON cp.ClaimNum = c.ClaimNum "
                      + "WHERE cp.Status = " + SOut.Long((long) ClaimProcStatus.NotReceived) + " "
                      + "AND (cp.ProcNum = 0 OR p.ProcStatus = " + SOut.Long((long) ProcStat.C) + ") "
                      + "AND cp.PatNum IN(" + string.Join(", ", listPatNums) + ") ";
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     When sending or printing a claim, this converts the supplied list into a list of ClaimProcs that need to be
    ///     sent.
    /// </summary>
    public static List<ClaimProc> GetForSendClaim(List<ClaimProc> listClaimProcs, long claimNum)
    {
        //MessageBox.Show(List.Length.ToString());
        var listLabProcNums = new List<long>();
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) listLabProcNums = Procedures.GetCanadianLabFees(listClaimProcs.Select(x => x.ProcNum).Where(x => x != 0).ToList()).Select(x => x.ProcNum).ToList();
        var listClaimProcsRet = new List<ClaimProc>();
        bool includeThis;
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ClaimNum != claimNum) continue;
            if (listClaimProcs[i].ProcNum == 0) continue; //skip payments
            if (CultureInfo.CurrentCulture.Name.EndsWith("CA") //Canada
                && listLabProcNums.Contains(listClaimProcs[i].ProcNum)) //Current claimProc is associated to a lab.
                continue;
            includeThis = true;
            for (var j = 0; j < listClaimProcsRet.Count; j++) //loop through existing claimprocs
                if (listClaimProcsRet[j].ProcNum == listClaimProcs[i].ProcNum)
                    includeThis = false; //skip duplicate procedures

            if (includeThis) listClaimProcsRet.Add(listClaimProcs[i]);
        }

        return listClaimProcsRet;
    }

    ///<summary>Gets claimprocs from the given list which are attached to the given claimNum and are attached to a procedure.</summary>
    public static List<ClaimProc> GetForClaimOverpay(List<ClaimProc> listClaimProcs, long claimNum)
    {
        var listClaimProcsRet = new List<ClaimProc>();
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ClaimNum != claimNum) continue;
            if (listClaimProcs[i].ProcNum == 0) continue; //skip total payments
            listClaimProcsRet.Add(listClaimProcs[i]);
        }

        return listClaimProcsRet;
    }

    /// <summary>
    ///     Sets the fields of an under/overpayment claimproc and then returns the claimproc. If the passed in
    ///     claimProcOverpay is null, creates the claimproc.
    /// </summary>
    public static ClaimProc CreateOverpay(ClaimProc claimProc, double insEstTotalOverride, ClaimProc claimProcOverpay = null)
    {
        if (claimProcOverpay == null) claimProcOverpay = new ClaimProc();
        //claimProcOverpay.ClaimProcNum//Either claimProcOverpay was given and ClaimProcNum is already set, or we created a new claimproc above with ClaimProcNum=0.
        claimProcOverpay.ProcNum = claimProc.ProcNum;
        claimProcOverpay.ClaimNum = claimProc.ClaimNum;
        claimProcOverpay.PatNum = claimProc.PatNum; //Matches claim.PatNum
        claimProcOverpay.ProvNum = claimProc.ProvNum; //Financial impact.
        claimProcOverpay.FeeBilled = 0; //Prevents matching in 835s.
        //InsPayEst is what affects patient portion when in NotReceived status. See ClaimProcs.GetPatPortion() and more importantly ClaimProcs.GetInsPay().
        claimProcOverpay.InsPayEst = insEstTotalOverride;
        claimProcOverpay.DedApplied = 0; //Not applicable.
        claimProcOverpay.Status = ClaimProcStatus.NotReceived;
        claimProcOverpay.InsPayAmt = 0; //Is set later when converting this overpayment/underpayment to Supplemental status.
        claimProcOverpay.Remarks = ""; //Is set outside of this function.
        claimProcOverpay.ClaimPaymentNum = 0; //Overpayments/underpayments will not be attached to claim payments.
        claimProcOverpay.PlanNum = claimProc.PlanNum; //Financial impact.
        claimProcOverpay.DateCP = claimProc.DateCP;
        claimProcOverpay.WriteOff = 0; //Not applicable.
        claimProcOverpay.CodeSent = claimProc.CodeSent;
        claimProcOverpay.AllowedOverride = -1; //Not applicable.
        claimProcOverpay.Percentage = -1; //Not applicable.
        claimProcOverpay.PercentOverride = -1; //Not applicable.
        claimProcOverpay.CopayAmt = -1; //Not applicable.
        claimProcOverpay.NoBillIns = false; //Job 50054. NADG wants this set to false. When true, it hides the financial info for the claimproc in the Procedure Info window.
        claimProcOverpay.PaidOtherIns = -1; //Not applicable.
        claimProcOverpay.BaseEst = insEstTotalOverride;
        claimProcOverpay.CopayOverride = -1; //Not applicable.
        claimProcOverpay.ProcDate = claimProc.ProcDate;
        //claimProcOverpay.DateEntry//CRUD automatically sets this.
        //LineNumber is a one-based index which is used when printing claims to match up line items
        claimProcOverpay.LineNumber = 0; //Leave set to zero to prevent overpayments/underpayments from showing on printed claims.
        claimProcOverpay.DedEst = -1; //Not applicable.
        claimProcOverpay.DedEstOverride = -1; //Not applicable.
        claimProcOverpay.InsEstTotal = insEstTotalOverride;
        claimProcOverpay.InsEstTotalOverride = insEstTotalOverride; //Will be negative for overpayments, positive for underpayments.
        claimProcOverpay.PaidOtherInsOverride = -1; //Not applicable.
        claimProcOverpay.EstimateNote = ""; //Not applicable when attached to a claim. Overpayments/underpayments are always attached to a claim.
        claimProcOverpay.WriteOffEst = -1; //Not applicable.
        claimProcOverpay.WriteOffEstOverride = -1; //Not applicable.
        claimProcOverpay.ClinicNum = claimProc.ClinicNum; //Financial impact.
        claimProcOverpay.InsSubNum = claimProc.InsSubNum; //Financial impact.
        claimProcOverpay.PaymentRow = 0; //Overpayments/underpayments do not come directly from EOBs. 
        //claimProcOverpay.DoDelete//Not a database field.
        claimProcOverpay.PayPlanNum = 0; //Not applicable.
        claimProcOverpay.ClaimPaymentTracking = 0; //Overpayments/underpayments will not be attached to claim payments.
        claimProcOverpay.SecUserNumEntry = Security.CurUser.UserNum;
        //claimProcOverpay.SecDateEntry//CRUD automatically sets this.
        //claimProcOverpay.SecDateTEdit//CRUD automatically sets this.
        claimProcOverpay.DateSuppReceived = DateTime.MinValue;
        claimProcOverpay.DateInsFinalized = DateTime.MinValue;
        claimProcOverpay.IsTransfer = false;
        claimProcOverpay.ClaimAdjReasonCodes = ""; //Overpayments/underpayments do not come from ERAs directly.
        claimProcOverpay.IsOverpay = true;
        return claimProcOverpay;
    }


    ///<summary>Gets all ClaimProcs for the current Procedure. The List must be all ClaimProcs for this patient.</summary>
    public static List<ClaimProc> GetForProc(List<ClaimProc> listClaimProcs, long procNum)
    {
        return listClaimProcs.FindAll(x => x.ProcNum == procNum);
    }

    /// <summary>
    ///     Loops through listClaimProcs for a claimProc associated to the given claimProcNum.
    ///     If not found returns null
    /// </summary>
    public static ClaimProc GetFromList(List<ClaimProc> listClaimProcs, long claimProcNum)
    {
        for (var i = 0; i < listClaimProcs.Count; i++)
            if (listClaimProcs[i].ClaimProcNum == claimProcNum)
                return listClaimProcs[i];

        return null;
    }

    /// <summary>Gets all ClaimProcs for the list of ProcNums, optionally filtered by list of statuses.</summary>
    /// <param name="useDataReader">
    ///     Setting useDataReader to true will cause this to call DataCore.GetList(command,RowToObj) which uses a MySqlDataReader to
    ///     retrieve one row at a time to be converted to ClaimProc objects.  This is to reduce the memory load of the
    ///     TableToList(GetTable(command))
    ///     pattern which causes two copies of the data to be held in memory, one as a DataRow and one as the object.
    /// </param>
    public static List<ClaimProc> GetForProcs(List<long> listProcNums, List<ClaimProcStatus> listClaimProcStatuses = null, bool useDataReader = false)
    {
        if (listProcNums.IsNullOrEmpty()) return new List<ClaimProc>();
        var command = $"SELECT * FROM claimproc WHERE ProcNum IN({string.Join(",", listProcNums)})";
        if (!listClaimProcStatuses.IsNullOrEmpty()) command += $" AND Status IN ({string.Join(",", listClaimProcStatuses.Select(x => SOut.Int((int) x)))})";
        if (useDataReader) return DataCore.GetList(command, ClaimProcCrud.RowToObj);
        return ClaimProcCrud.SelectMany(command);
    }

    ///<summary> </summary>
    public static List<ClaimProc> GetForProcsWithOrdinal(List<long> listProcNums, int ordinal)
    {
        var listClaimProcs = new List<ClaimProc>();
        if (listProcNums == null || listProcNums.Count < 1) return listClaimProcs;
        var command = "SELECT claimproc.* "
                      + "FROM claimproc "
                      + "INNER JOIN patplan ON patplan.InsSubNum=claimproc.InsSubNum "
                      + "AND patplan.PatNum=claimproc.PatNum "
                      + "AND patplan.Ordinal=" + SOut.Int(ordinal) + " "
                      + "WHERE ProcNum IN(" + string.Join(",", listProcNums) + ")";
        return ClaimProcCrud.SelectMany(command);
    }

    ///<summary>Mimics GetForProcsWithOrdinal(...) but for cached information.</summary>
    public static List<ClaimProc> GetForProcsWithOrdinalFromList(List<long> listProcNums, int ordinal, List<PatPlan> listPatPlansAll, List<ClaimProc> listClaimProcsAll)
    {
        if (listProcNums == null || listProcNums.Count < 1) return new List<ClaimProc>();
        return listClaimProcsAll.FindAll(x =>
            listProcNums.Contains(x.ProcNum) && listPatPlansAll.Find(y => y.InsSubNum == x.InsSubNum && y.PatNum == x.PatNum && y.Ordinal == ordinal) != null
        );
    }

    ///<summary> </summary>
    public static ClaimProc GetForProcWithOrdinal(long procNum, int ordinal)
    {
        var command = "SELECT claimproc.* "
                      + "FROM claimproc "
                      + "INNER JOIN patplan ON patplan.InsSubNum=claimproc.InsSubNum "
                      + "AND patplan.PatNum=claimproc.PatNum "
                      + "AND patplan.Ordinal=" + SOut.Int(ordinal) + " "
                      + "WHERE ProcNum = " + SOut.Long(procNum);
        return ClaimProcCrud.SelectOne(command);
    }

    /// <summary>
    ///     Used in TP module to get one estimate. The List must be all ClaimProcs for this patient. If estimate can't be
    ///     found, then return null.  The procedure is always status TP, so there shouldn't be more than one estimate for one
    ///     plan.
    /// </summary>
    public static ClaimProc GetEstimate(List<ClaimProc> listClaimProcs, long procNum, long planNum, long subNum)
    {
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].Status == ClaimProcStatus.Preauth) continue;
            if (listClaimProcs[i].ProcNum == procNum && listClaimProcs[i].PlanNum == planNum && listClaimProcs[i].InsSubNum == subNum) return listClaimProcs[i];
        }

        return null;
    }

    /// <summary>
    ///     Used in Account and in PaySplitEdit. The insurance estimate based on all claimprocs with this procNum, but
    ///     only for those claimprocs that are not received yet. The list can be all ClaimProcs for patient, or just those for
    ///     this procedure.
    /// </summary>
    public static double ProcEstNotReceived(List<ClaimProc> listClaimProcs, long procNum)
    {
        return listClaimProcs.FindAll(x => x.ProcNum == procNum && x.Status == ClaimProcStatus.NotReceived).Select(x => x.InsPayEst).Sum();
    }

    /// <summary>
    ///     Used in PaySplitEdit. The insurance amount paid based on all claimprocs with this procNum. The list can be all
    ///     ClaimProcs for patient, or just those for this procedure.
    /// </summary>
    public static double ProcInsPay(List<ClaimProc> listClaimProcs, long procNum)
    {
        return listClaimProcs.FindAll(x => x.ProcNum == procNum)
            .FindAll(x => !x.Status.In(ClaimProcStatus.Preauth, ClaimProcStatus.CapEstimate, ClaimProcStatus.CapComplete, ClaimProcStatus.Estimate, ClaimProcStatus.InsHist))
            .Select(x => x.InsPayAmt).Sum();
    }

    /// <summary>
    ///     Used in PaySplitEdit. The insurance writeoff based on all claimprocs with this procNum. The list can be all
    ///     ClaimProcs for patient, or just those for this procedure.
    /// </summary>
    public static double ProcWriteoff(List<ClaimProc> listClaimProcs, long procNum)
    {
        return listClaimProcs.FindAll(x => x.ProcNum == procNum)
            .FindAll(x => !x.Status.In(ClaimProcStatus.Preauth, ClaimProcStatus.CapEstimate, ClaimProcStatus.CapComplete, ClaimProcStatus.Estimate, ClaimProcStatus.InsHist))
            .Select(x => x.WriteOff).Sum();
    }

    /// <summary>
    ///     Used in E-claims to get the amount paid by primary. The insurance amount paid by other subNums based on all
    ///     claimprocs with this procNum. The list can be all ClaimProcs for patient, or just those for this procedure.
    /// </summary>
    public static double ProcInsPayPri(List<ClaimProc> listClaimProcs, long procNum, long subNumExclude)
    {
        double retVal = 0;
        for (var i = 0; i < listClaimProcs.Count; i++)
            if (listClaimProcs[i].ProcNum == procNum
                && listClaimProcs[i].InsSubNum != subNumExclude
                && listClaimProcs[i].Status != ClaimProcStatus.Preauth
                && listClaimProcs[i].Status != ClaimProcStatus.CapEstimate
                && listClaimProcs[i].Status != ClaimProcStatus.CapComplete
                && listClaimProcs[i].Status != ClaimProcStatus.InsHist
                && listClaimProcs[i].Status != ClaimProcStatus.Estimate)
                retVal += listClaimProcs[i].InsPayAmt;

        return retVal;
    }

    public static bool IsValidClaimAdj(ClaimProc claimProc, long procNum, long subNumExclude)
    {
        if (claimProc.ProcNum != procNum) return false;
        if (claimProc.InsSubNum == subNumExclude) return false;
        if (claimProc.Status == ClaimProcStatus.CapClaim
            //|| claimProc.Status==ClaimProcStatus.NotReceived //7/9/2013 Was causing paid amounts to show on primary claims when the patient had secondary insurance, because this is the starting status of secondary claimprocs when the New Claim button is pressed.
            || claimProc.Status == ClaimProcStatus.Received
            || claimProc.Status == ClaimProcStatus.Supplemental)
            //Adjustment never attached to proc. Preauth, CapEstimate, CapComplete, and Estimate never paid. 
            return true;

        return false;
    }

    /// <summary>
    ///     Used in E-claims to get the most recent date paid (by primary?). The insurance amount paid by the planNum
    ///     based on all claimprocs with this procNum. The list can be all ClaimProcs for patient, or just those for this
    ///     procedure.
    /// </summary>
    public static DateTime GetDatePaid(List<ClaimProc> listClaimProcs, long procNum, long planNum)
    {
        var date = DateTime.MinValue;
        for (var i = 0; i < listClaimProcs.Count; i++)
            if (listClaimProcs[i].ProcNum == procNum
                && listClaimProcs[i].PlanNum == planNum
                && listClaimProcs[i].Status != ClaimProcStatus.Preauth
                && listClaimProcs[i].Status != ClaimProcStatus.CapEstimate
                && listClaimProcs[i].Status != ClaimProcStatus.CapComplete
                && listClaimProcs[i].Status != ClaimProcStatus.InsHist
                && listClaimProcs[i].Status != ClaimProcStatus.Estimate)
                if (listClaimProcs[i].DateCP > date)
                    date = listClaimProcs[i].DateCP;

        return date;
    }

    /// <summary>
    ///     Used once in Account on the Claim line.  The amount paid on a claim only by total, not including by procedure.
    ///     The list can be all ClaimProcs for patient, or just those for this claim.
    /// </summary>
    public static double ClaimByTotalOnly(ClaimProc[] claimProcArray, long claimNum)
    {
        double retVal = 0;
        for (var i = 0; i < claimProcArray.Length; i++)
            if (claimProcArray[i].ClaimNum == claimNum
                && claimProcArray[i].ProcNum == 0
                && claimProcArray[i].Status != ClaimProcStatus.Preauth)
                retVal += claimProcArray[i].InsPayAmt;

        return retVal;
    }

    /// <summary>
    ///     Used once in Account on the Claim line.  The writeoff amount on a claim only by total, not including by
    ///     procedure.  The list can be all ClaimProcs for patient, or just those for this claim.
    /// </summary>
    public static double ClaimWriteoffByTotalOnly(ClaimProc[] claimProcArray, long claimNum)
    {
        double retVal = 0;
        for (var i = 0; i < claimProcArray.Length; i++)
            if (claimProcArray[i].ClaimNum == claimNum
                && claimProcArray[i].ProcNum == 0
                && claimProcArray[i].Status != ClaimProcStatus.Preauth)
                retVal += claimProcArray[i].WriteOff;

        return retVal;
    }

    /// <summary>
    ///     Returns the sum of all claimproc writeoff amounts for the specified claim.  If there are claimprocs provided
    ///     in the list it will not include those in the sum.
    /// </summary>
    public static double GetClaimWriteOffTotal(long claimNum, long procNum, List<ClaimProc> listClaimProcsExclude)
    {
        var command = "SELECT * FROM claimproc WHERE ClaimNum=" + SOut.Long(claimNum) + " AND ProcNum=" + SOut.Long(procNum) + " AND Status IN(" + (int) ClaimProcStatus.Received + "," + (int) ClaimProcStatus.Supplemental + ")";
        var listClaimProcs = ClaimProcCrud.SelectMany(command);
        decimal writeoffTotal = 0; //decimal used to prevent rounding errors.
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcsExclude.Exists(x => x.ClaimProcNum == listClaimProcs[i].ClaimProcNum)) continue; //Don't sum together the current claimprocs that are being edited.
            writeoffTotal += (decimal) listClaimProcs[i].WriteOff;
        }

        return (double) writeoffTotal;
    }

    /// <summary>
    ///     Used in Adjustments when calculating sales tax.  Returns the sum of all claimproc writeoff and estimate
    ///     amounts for the specified procedure.
    /// </summary>
    public static double GetWriteOffFromList(List<ClaimProc> listClaimProcs, long procNum)
    {
        double writeOff = 0;
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ProcNum != procNum) //Skip claimprocs that don't match
                continue;
            if (listClaimProcs[i].Status == ClaimProcStatus.Estimate)
            {
                if (listClaimProcs[i].WriteOffEstOverride != -1)
                    writeOff += listClaimProcs[i].WriteOffEstOverride;
                else if (listClaimProcs[i].WriteOffEst != -1) writeOff += listClaimProcs[i].WriteOffEst;
            }
            else if ((listClaimProcs[i].Status == ClaimProcStatus.Received || listClaimProcs[i].Status == ClaimProcStatus.NotReceived) && listClaimProcs[i].WriteOff != -1)
            {
                writeOff += listClaimProcs[i].WriteOff;
            }
        }

        return writeOff;
    }

    /// <summary>
    ///     Centralized logic for computing sales tax. Returns whatever value is already there for ODHQ. The isEstimate
    ///     parameter should be set to true when creating sales tax estimates
    /// </summary>
    public static double ComputeSalesTax(Procedure procedure, List<ClaimProc> listClaimProcs, bool isEstimate)
    {
        //This check will stop non-autotaxed procedures from showing sales tax estimates, but will still allow manual sales tax to be applied
        if (isEstimate && !ProcedureCodes.GetProcCode(procedure.CodeNum).IsTaxed) return 0;
        //NOTE: In Job F822, we decided that we would implement the workflow as deducting writeoffs before applying sales tax.
        //This will most likely need to update to account for other adjustments as sales tax gets more use.
        var writeOff = GetWriteOffFromList(listClaimProcs, procedure.ProcNum);
        var taxPercent = PrefC.GetDouble(PrefName.SalesTaxPercentage);
        var taxAmt = Math.Round((procedure.ProcFee - writeOff) * (taxPercent / 100), 2); //Round to two place
        return taxAmt;
    }

    /// <summary>
    ///     Attaches or detaches claimprocs from the specified claimPayment. Updates all claimprocs on a claim with one
    ///     query.  It also updates their DateCP's to match the claimpayment date.
    /// </summary>
    public static void SetForClaimOld(long claimNum, long claimPaymentNum, DateTime date, bool setAttached)
    {
        var command = "UPDATE claimproc SET ClaimPaymentNum = ";
        if (setAttached)
            command += "" + SOut.Long(claimPaymentNum) + " ";
        else
            command += "0 ";
        command += ",DateCP=" + SOut.Date(date) + " "
                   + "WHERE ClaimNum=" + SOut.Long(claimNum) + " AND "
                   + "claimproc.IsTransfer=0 AND "
                   + "InsPayAmt!=0 AND ("
                   + "ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + " OR ClaimPaymentNum=0)";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Attaches claimprocs to the specified claimPayment. Updates all claimprocs on a claim with one query.
    ///     It also updates their DateCP's to match the claimpayment date.  Returns the number of rows that were changed via
    ///     the UPDATE query.
    /// </summary>
    public static long AttachToPayment(long claimNum, long claimPaymentNum, DateTime date, int paymentRow)
    {
        var command = "UPDATE claimproc SET ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + ", "
                      + "DateCP=" + SOut.Date(date) + ", "
                      + "PaymentRow=" + SOut.Int(paymentRow) + ", "
                      + "DateInsFinalized = (CASE DateInsFinalized WHEN '0001-01-01' THEN " + DbHelper.Now() + " ELSE DateInsFinalized END) "
                      + "WHERE ClaimNum=" + SOut.Long(claimNum) + " "
                      + "AND Status IN (" + string.Join(",", GetInsPaidStatuses().Select(x => SOut.Int((int) x))) + ") "
                      + "AND ClaimPaymentNum=0 "
                      + "AND claimproc.IsTransfer=0 ";
        return Db.NonQ(command);
    }

    /// <summary>
    ///     Detaches claimprocs from the specified claimPayment. Updates all claimprocs on a list of claims with one
    ///     query.
    /// </summary>
    public static void DetachFromPayment(List<long> listClaimNums, long claimPaymentNum)
    {
        if (listClaimNums == null || listClaimNums.Count == 0) return;
        var command = "UPDATE claimproc SET "
                      + "DateInsFinalized='0001-01-01' "
                      + "WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + " "
                      + "AND (SELECT SecDateEntry FROM claimpayment WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + ")=" + DbHelper.Curdate() + " "
                      + "AND ClaimNum IN(" + string.Join(",", listClaimNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
        command = "UPDATE claimproc SET ClaimPaymentNum=0, "
                  //+"DateCP="+POut.Date(DateTime.MinValue)+", "
                  + "PaymentRow=0 "
                  + "WHERE ClaimNum IN (" + string.Join(",", listClaimNums.Select(x => SOut.Long(x))) + ") "
                  + "AND ClaimPaymentNum=" + SOut.Long(claimPaymentNum);
        Db.NonQ(command);
    }

    ///<summary>Synchs all claimproc DateCP's attached to the claim payment.  Used when an insurance check's date is changed.</summary>
    public static void SynchDateCP(long claimPaymentNum, DateTime date)
    {
        var command = "UPDATE claimproc SET "
                      + "DateCP=" + SOut.Date(date) + " "
                      + "WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + " "
                      + "AND claimproc.IsTransfer=0 ";
        Db.NonQ(command);
    }

    ///<summary>After entering estimates from a preauth, this routine is called for each proc to override the ins est.</summary>
    public static void SetInsEstTotalOverride(long procNum, long planNum, long insSubNum, double insPayEst, List<ClaimProc> listClaimProcs)
    {
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (procNum != listClaimProcs[i].ProcNum) continue;
            if (planNum != listClaimProcs[i].PlanNum) continue;
            if (insSubNum != listClaimProcs[i].InsSubNum) continue;
            if (listClaimProcs[i].Status != ClaimProcStatus.Estimate) continue;
            listClaimProcs[i].InsEstTotalOverride = insPayEst;
            Update(listClaimProcs[i]);
        }
    }

    /// <summary>
    ///     Calculates the Base estimate, InsEstTotal, and all the other insurance numbers for a single claimproc.  This is not
    ///     done on the fly.
    ///     Use Procedure.GetEst to later retrieve the estimate. This function replaces all of the upper estimating logic that
    ///     was within FormClaimProc.
    ///     BaseEst=((fee or allowedOverride)-Copay) x (percentage or percentOverride).
    ///     The calling class must have already created the claimProc, this function simply updates the BaseEst field of that
    ///     claimproc. pst.Tot not used.
    ///     For Estimate and CapEstimate, all the estimate fields will be recalculated except the overrides.  histList and
    ///     loopList can be null.
    ///     If so, then deductible and annual max will not be recalculated.  histList and loopList may only make sense in TP
    ///     module and claimEdit.
    ///     loopList contains all claimprocs in the current list (TP or claim) that come before this procedure.
    ///     PaidOtherInsTot should only contain sum of InsEstTotal/Override, or paid, depending on the status.
    ///     PaidOtherInsBase also includes actual payments.
    ///     listSubstLinks won't be null because it gets filled in Procedures.ComputeEstimates
    ///     lookupFees can be null, in which case, it will go to the db for fee.  lookupFees passed in will also contain all
    ///     possible alternate codes and medical codes.  In GlobalUpdateWriteoffs, it will be massive, which is why it must be
    ///     a lookup.
    ///     For Canadians, this method doesn't simply calculate insurance related numbers for a single claimproc as it states
    ///     above.
    ///     There is a chance that this function will create new claimprocs for labs associated to the procedure passed in if
    ///     they do not already exist.
    ///     Set doCheckCanadianLabs to false when simply making in memory changes to given cp (like in FormClaimProc).
    ///     Otherwise duplicate lab claimprocs can be created if cp.Status was only changed in memory.
    /// </summary>
    public static void ComputeBaseEst(ClaimProc claimProc, Procedure procedure, InsPlan insPlan, long patPlanNum, List<Benefit> listBenefits, List<ClaimProcHist> listClaimProcHists
        , List<ClaimProcHist> listClaimProcHistsLoop, List<PatPlan> listPatPlans, double paidOtherInsTot, double paidOtherInsBase, int patientAge, double writeOffOtherIns
        , List<InsPlan> listInsPlans, List<InsSub> listInsSubs, List<SubstitutionLink> listSubstitutionLinks, bool useProcDateOnProc, //=false,List<Fee> listFees=null) 
        Lookup<FeeKey2, Fee> lookupFees, BlueBookEstimateData blueBookEstimateData, bool doCheckCanadianLabs = true)
    {
        if (claimProc.Status == ClaimProcStatus.Received && !PrefC.GetBool(PrefName.InsEstRecalcReceived)) return;
        if (listClaimProcHists != null)
            //In case we are recalculating the estimate for a procedure already attached to a claim, we need to make sure the histList does not include
            //the claim proc we are currently recalculating.
            listClaimProcHists = listClaimProcHists.FindAll(x => x.ProcNum != claimProc.ProcNum || x.ClaimNum != claimProc.ClaimNum);
        if (Canadian.IsValidForLabEstimates(insPlan) && procedure.ProcNumLab != 0)
        {
            //This is a lab. Do not allow it to calculate its own estimates.
            //Instead use parents associated claimProc to calcualte estimates.
            var procParent = Procedures.GetOneProc(procedure.ProcNumLab, false);
            var listClaimProcs = RefreshForProc(procParent.ProcNum);
            //Don't want to pass BlueBookEstimateData in here because it won't include data for the procParent.
            //ComputeEstimates() will create its own BlueBookEstimateData
            Procedures.ComputeEstimates(procParent, procParent.PatNum, listClaimProcs, false, listInsPlans, listPatPlans, listBenefits, patientAge, listInsSubs);
            return;
        }

        var procFee = procedure.ProcFeeTotal;
        var toothNum = procedure.ToothNum;
        var codeNum = procedure.CodeNum;
        if (claimProc.Status == ClaimProcStatus.CapClaim
            || claimProc.Status == ClaimProcStatus.CapComplete
            || claimProc.Status == ClaimProcStatus.Preauth
            || claimProc.Status == ClaimProcStatus.Supplemental)
        {
            if (Canadian.IsValidForLabEstimates(insPlan) && claimProc.Status == ClaimProcStatus.Preauth && doCheckCanadianLabs)
            {
                var listProceduresLabFees = Procedures.GetCanadianLabFees(procedure.ProcNum);
                for (var i = 0; i < listProceduresLabFees.Count; i++) CanadianLabBaseEstHelper(claimProc, listProceduresLabFees[i], insPlan, claimProc.InsSubNum, procedure, listBenefits, patPlanNum, listClaimProcHists, listClaimProcHistsLoop, patientAge, useProcDateOnProc);
            }

            return; //never compute estimates for those types listed above.
        }

        if (insPlan.PlanType == "c" //if capitation plan
            && claimProc.Status == ClaimProcStatus.Estimate) //and ordinary estimate
            claimProc.Status = ClaimProcStatus.CapEstimate;
        if (insPlan.PlanType != "c" //if not capitation plan
            && claimProc.Status == ClaimProcStatus.CapEstimate) //and estimate is a capitation estimate
            claimProc.Status = ClaimProcStatus.Estimate;
        //NoBillIns is only calculated when creating the claimproc, even if resetAll is true.
        //If user then changes a procCode, it does not cause an update of all procedures with that code.
        if (claimProc.NoBillIns)
        {
            ZeroOutClaimProc(claimProc);
            //Canadian Lab Fee Estimates-------------------------------------------------------------------------------------------------------------------
            //These will all be 0 because they are based on the parent claimproc's percentage, which just got blanked out.
            if (Canadian.IsValidForLabEstimates(insPlan) && doCheckCanadianLabs)
            {
                var listProceduresLabFees = Procedures.GetCanadianLabFees(procedure.ProcNum);
                for (var i = 0; i < listProceduresLabFees.Count; i++) CanadianLabBaseEstHelper(claimProc, listProceduresLabFees[i], insPlan, claimProc.InsSubNum, procedure, listBenefits, patPlanNum, listClaimProcHists, listClaimProcHistsLoop, patientAge, useProcDateOnProc);
            }

            return;
        }

        claimProc.EstimateNote = "";
        //This function is called every time a ProcFee is changed,
        //so the BaseEst does reflect the new ProcFee.
        //ProcFee----------------------------------------------------------------------------------------------
        claimProc.BaseEst = procFee;
        claimProc.InsEstTotal = procFee;
        //Allowed----------------------------------------------------------------------------------------------
        var allowed = procFee; //could be fee, or could be a little less.  Used further down in paidOtherIns.
        var codeSubstNone = !SubstitutionLinks.HasSubstCodeForPlan(insPlan, codeNum, listSubstitutionLinks); //Left variable name alone when substitution links added.
        if (claimProc.AllowedOverride != -1)
        {
            if (claimProc.AllowedOverride > procFee) claimProc.AllowedOverride = procFee;
            allowed = claimProc.AllowedOverride;
            claimProc.BaseEst = claimProc.AllowedOverride;
            claimProc.InsEstTotal = claimProc.AllowedOverride;
        }
        else if (insPlan.PlanType == "c")
        {
            //capitation estimate.  No allowed fee sched.  No substitute codes.
            allowed = procFee;
            claimProc.BaseEst = procFee;
            claimProc.InsEstTotal = procFee;
        }
        else
        {
            //no point in wasting time calculating this unless it's needed.
            //List<Fee> listFee=lookupFees[new FeeKey2(codeNum,feeSched)].ToList();
            double carrierAllowed;
            if (blueBookEstimateData != null && blueBookEstimateData.IsValidForEstimate(claimProc))
                carrierAllowed = blueBookEstimateData.GetAllowed(procedure, lookupFees, codeSubstNone, listSubstitutionLinks);
            else
                carrierAllowed = InsPlans.GetAllowed(ProcedureCodes.GetProcCode(codeNum).ProcCode, insPlan.FeeSched, insPlan.AllowedFeeSched,
                    codeSubstNone, insPlan.PlanType, toothNum, procedure.ProvNum, procedure.ClinicNum, insPlan.PlanNum, listSubstitutionLinks, lookupFees); //lookupFees can be null
            if (carrierAllowed == -1)
            {
                //Fee not found in feesched
                if (PrefC.GetBool(PrefName.InsOutOfNetworkBlankLikeZero))
                {
                    allowed = 0;
                    claimProc.BaseEst = 0;
                    claimProc.InsEstTotal = 0;
                }
            }
            else
            {
                carrierAllowed = carrierAllowed * procedure.Quantity;
                if (carrierAllowed > procFee)
                {
                    allowed = procFee;
                    claimProc.BaseEst = procFee;
                    claimProc.InsEstTotal = procFee;
                }
                else
                {
                    allowed = carrierAllowed;
                    claimProc.BaseEst = carrierAllowed;
                    claimProc.InsEstTotal = carrierAllowed;
                }
            }
        }

        //Copay----------------------------------------------------------------------------------------------
        var feeSchedCopay = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == insPlan.CopayFeeSched);
        if (insPlan.PlanType == "p" && feeSchedCopay != null && feeSchedCopay.FeeSchedType == FeeScheduleType.FixedBenefit)
        {
            var codeNumFixedBen = codeNum;
            if (!codeSubstNone) //Has substitution code
                codeNumFixedBen = ProcedureCodes.GetSubstituteCodeNum(ProcedureCodes.GetStringProcCode(codeNum), toothNum, insPlan.PlanNum, listSubstitutionLinks);
            Fee feeFixedBenefit = null;
            Fee feePpo = null;
            List<Fee> listFees = null;
            if (lookupFees != null) listFees = lookupFees[new FeeKey2(codeNumFixedBen, feeSchedCopay.FeeSchedNum)].ToList();
            feeFixedBenefit = Fees.GetFee(codeNumFixedBen, feeSchedCopay.FeeSchedNum, procedure.ClinicNum, procedure.ProvNum, listFees);
            if (lookupFees != null) listFees = lookupFees[new FeeKey2(codeNumFixedBen, insPlan.FeeSched)].ToList();
            feePpo = Fees.GetFee(codeNumFixedBen, insPlan.FeeSched, procedure.ClinicNum, procedure.ProvNum, listFees);
            if (feePpo == null)
                //No fee defined for this procedure, use ProcFee because it is assumed to be a 100% coverage with PPO plans
                claimProc.CopayAmt = procFee;
            else
                claimProc.CopayAmt = feePpo.Amount;
            if (feeFixedBenefit != null && CompareDouble.IsGreaterThan(feeFixedBenefit.Amount, 0))
            {
                //If we have a valid feeFixedbenefit and feePPO > procFee or carrierallowed, then we can set the copay to the UCR fee rather than the PPO fee
                if (PrefC.GetBool(PrefName.InsPpoAlwaysUseUcrFee) && CompareDouble.IsGreaterThan(claimProc.CopayAmt, claimProc.BaseEst))
                    //From the manual: "The patient portion is calculated using the following formula: UCR fee - Write-Off - Fixed Benefit amount".
                    //At this point the writeoffs for this claimproc will be 0 so it can be omitted, and the fixed benefit amount is subtracted below, so just change the copay amount to the carrierallowed or the procfee (whatever BaseEst was set to above).
                    claimProc.CopayAmt = claimProc.BaseEst;
                //Deduct the fixed benefit amount from the PPO fee or the procfee/allowed amount (whatever BaseEst was set to above) to determine the copay for this claimproc. When the InsPpoAlwaysUseUcrFee pref is set, the formula will be Copay = UCR fee - Fixed Benefit Amount, otherwise Copay =  PPO fee - Fixed Benefit Amount.
                claimProc.CopayAmt -= feeFixedBenefit.Amount;
            }

            if (feeFixedBenefit == null && !PrefC.GetBool(PrefName.FixedBenefitBlankLikeZero)) claimProc.CopayAmt = -1;
        }
        else
        {
            claimProc.CopayAmt = InsPlans.GetCopay(codeNum, insPlan.FeeSched, insPlan.CopayFeeSched, codeSubstNone, toothNum, procedure.ClinicNum, procedure.ProvNum, insPlan.PlanNum,
                listSubstitutionLinks, lookupFees);
        }

        if (claimProc.CopayAmt != -1) claimProc.CopayAmt = claimProc.CopayAmt * procedure.Quantity;
        if (claimProc.CopayAmt > allowed) //if the copay is greater than the allowed fee calculated above
            claimProc.CopayAmt = allowed; //reduce the copay
        if (claimProc.CopayOverride > allowed) //or if the copay override is greater than the allowed fee calculated above
            claimProc.CopayOverride = allowed; //reduce the override
        if (claimProc.Status == ClaimProcStatus.CapEstimate)
        {
            //this does automate the Writeoff. If user does not want writeoff automated,
            //then they will have to complete the procedure first. (very rare)
            if (claimProc.CopayAmt == -1) claimProc.CopayAmt = 0;
            if (insPlan != null && InsPlans.UsesUcrFeeForExclusions(insPlan.ExclusionFeeRule) && (Benefits.IsExcluded(ProcedureCodes.GetStringProcCode(codeNum), listBenefits, insPlan.PlanNum, patPlanNum)
                                                                                                  || Benefits.GetPercent(ProcedureCodes.GetStringProcCode(codeNum), insPlan.PlanType, insPlan.PlanNum, patPlanNum, listBenefits) == 0))
                claimProc.WriteOffEst = 0; //Never any writeoff for excluded procedures in this case.
            else if (claimProc.CopayOverride != -1) //override the copay
                claimProc.WriteOffEst = claimProc.BaseEst - claimProc.CopayOverride;
            else if (claimProc.CopayAmt != -1) //use the calculated copay
                claimProc.WriteOffEst = claimProc.BaseEst - claimProc.CopayAmt;
            if (claimProc.WriteOffEst < 0) claimProc.WriteOffEst = 0;
            claimProc.WriteOff = claimProc.WriteOffEst;
            claimProc.DedApplied = 0;
            claimProc.DedEst = 0;
            claimProc.Percentage = -1;
            claimProc.PercentOverride = -1;
            claimProc.BaseEst = 0;
            claimProc.InsEstTotal = 0;
            return;
        }

        if (claimProc.CopayOverride != -1)
        {
            //subtract copay if override
            claimProc.BaseEst -= claimProc.CopayOverride;
            claimProc.InsEstTotal -= claimProc.CopayOverride;
        }
        else if (claimProc.CopayAmt != -1)
        {
            //otherwise subtract calculated copay
            claimProc.BaseEst -= claimProc.CopayAmt;
            claimProc.InsEstTotal -= claimProc.CopayAmt;
        }

        //Deductible----------------------------------------------------------------------------------------
        //The code below handles partial usage of available deductible. 
        DateTime dateProc;
        if (useProcDateOnProc)
            dateProc = procedure.ProcDate;
        else if (claimProc.Status == ClaimProcStatus.Estimate)
            dateProc = DateTime.Today;
        else
            dateProc = claimProc.ProcDate;
        if (listClaimProcHistsLoop != null && listClaimProcHists != null)
            claimProc.DedEst = Benefits.GetDeductibleByCode(listBenefits, insPlan.PlanNum, patPlanNum, dateProc, ProcedureCodes.GetStringProcCode(codeNum)
                , listClaimProcHists, listClaimProcHistsLoop, insPlan, claimProc.PatNum);
        if (Benefits.GetPercent(ProcedureCodes.GetProcCode(codeNum).ProcCode, insPlan.PlanType, insPlan.PlanNum, patPlanNum, listBenefits) == 0) //this is binary
            claimProc.DedEst = 0; //Procedure is not covered. Do not apply deductible. This does not take into account percent override.
        if (claimProc.DedEst > claimProc.InsEstTotal) //if the deductible is more than the fee
            claimProc.DedEst = claimProc.InsEstTotal; //reduce the deductible
        if (claimProc.DedEstOverride > claimProc.InsEstTotal) //if the deductible override is more than the fee
            claimProc.DedEstOverride = claimProc.InsEstTotal; //reduce the override.
        if (claimProc.DedEstOverride != -1) //use the override
            claimProc.InsEstTotal -= claimProc.DedEstOverride; //subtract
        else if (claimProc.DedEst != -1) //use the calculated deductible
            claimProc.InsEstTotal -= claimProc.DedEst;
        //Percentage----------------------------------------------------------------------------------------
        if (insPlan.PlanType == "p" && feeSchedCopay != null && feeSchedCopay.FeeSchedType == FeeScheduleType.FixedBenefit)
            claimProc.Percentage = 100;
        else
            claimProc.Percentage = Benefits.GetPercent(ProcedureCodes.GetProcCode(codeNum).ProcCode, insPlan.PlanType, insPlan.PlanNum, patPlanNum, listBenefits); //will never =-1
        if (claimProc.PercentOverride != -1)
        {
            //override, so use PercentOverride
            claimProc.BaseEst = claimProc.BaseEst * claimProc.PercentOverride / 100d;
            claimProc.InsEstTotal = claimProc.InsEstTotal * claimProc.PercentOverride / 100d;
        }
        else if (claimProc.Percentage != -1)
        {
            //use calculated Percentage
            claimProc.BaseEst = claimProc.BaseEst * claimProc.Percentage / 100d;
            claimProc.InsEstTotal = claimProc.InsEstTotal * claimProc.Percentage / 100d;
        }

        //PaidOtherIns----------------------------------------------------------------------------------------
        //double paidOtherInsActual=GetPaidOtherIns(cp,patPlanList,patPlanNum,histList);//can return -1 for primary
        var patPlan = PatPlans.GetFromList(listPatPlans.ToArray(), patPlanNum);
        //if -1, that indicates primary ins, not a proc, or no histlist.  We should not alter it in this case.
        //if(paidOtherInsActual!=-1) {
        //An older restriction was that histList must not be null.  But since this is now straight from db, that's not restriction.
        if (patPlan == null)
        {
            //corruption.  Do nothing.
        }
        else if (patPlan.Ordinal == 1 || claimProc.ProcNum == 0)
        {
            claimProc.PaidOtherIns = 0;
        }
        else
        {
            //if secondary or greater
            //The normal calculation uses the InsEstTotal from the primary ins.
            //But in TP module, if not using max and deduct, then the amount estimated to be paid by primary will be different.
            //It will use the primary BaseEst instead of the primary InsEstTotal.
            //Since the only use of BaseEst here is to handle this alternate viewing in the TP,
            //the secondary BaseEst should use the primary BaseEst when calculating paidOtherIns.
            //The BaseEst will, however, use PaidOtherInsOverride, if user has entered one.
            //This calculation doesn't need to be accurate unless viewing TP,
            //so it's ok to pass in a dummy value, like paidOtherInsTotal.
            //We do InsEstTotal first
            //cp.PaidOtherIns=paidOtherInsActual+paidOtherInsEstTotal;
            claimProc.PaidOtherIns = paidOtherInsTot;
            var paidOtherInsTotTemp = claimProc.PaidOtherIns;
            if (claimProc.PaidOtherInsOverride != -1) //use the override
                paidOtherInsTotTemp = claimProc.PaidOtherInsOverride;
            //example: Fee:200, InsEstT:80, BaseEst:100, PaidOI:110.
            //So... MaxPtP:90.
            //Since InsEstT is not greater than MaxPtoP, no change.
            //Since BaseEst is greater than MaxPtoP, BaseEst changed to 90.
            if (paidOtherInsTotTemp != -1)
            {
                double maxPossibleToPay = 0;
                if (insPlan.CobRule == EnumCobRule.Basic
                    || (insPlan.CobRule == EnumCobRule.SecondaryMedicaid && PatPlans.GetOrdinal(claimProc.InsSubNum, listPatPlans) == 2))
                {
                    maxPossibleToPay = allowed - paidOtherInsTotTemp;
                }
                else if (insPlan.CobRule == EnumCobRule.Standard
                         || (insPlan.CobRule == EnumCobRule.SecondaryMedicaid && PatPlans.GetOrdinal(claimProc.InsSubNum, listPatPlans) > 2))
                {
                    //If COB is SecondaryMedicaid, but the plan is not ordinal 2, default to standard COB.
                    var patPortionTot = procFee - paidOtherInsTotTemp - writeOffOtherIns; //patPortion for InsEstTotal
                    maxPossibleToPay = Math.Min(claimProc.BaseEst, patPortionTot); //The lesser of what insurance would pay if they were primary, and the patient portion.
                }
                else
                {
                    //plan.CobRule==EnumCobRule.CarveOut
                    maxPossibleToPay = claimProc.InsEstTotal - paidOtherInsTotTemp;
                }

                if (maxPossibleToPay < 0) maxPossibleToPay = 0;
                if (claimProc.InsEstTotal > maxPossibleToPay) claimProc.InsEstTotal = maxPossibleToPay; //reduce the estimate
            }

            //Then, we do BaseEst
            var paidOtherInsBaseTemp = paidOtherInsBase; //paidOtherInsActual+paidOtherInsBaseEst;
            if (claimProc.PaidOtherInsOverride != -1) //use the override
                paidOtherInsBaseTemp = claimProc.PaidOtherInsOverride;
            if (paidOtherInsBaseTemp != -1)
            {
                double maxPossibleToPay = 0;
                if (insPlan.CobRule == EnumCobRule.Basic
                    || (insPlan.CobRule == EnumCobRule.SecondaryMedicaid && PatPlans.GetOrdinal(claimProc.InsSubNum, listPatPlans) == 2))
                {
                    maxPossibleToPay = allowed - paidOtherInsBaseTemp;
                }
                else if (insPlan.CobRule == EnumCobRule.Standard
                         || (insPlan.CobRule == EnumCobRule.SecondaryMedicaid && PatPlans.GetOrdinal(claimProc.InsSubNum, listPatPlans) > 2))
                {
                    //If COB is SecondaryMedicaid, but the plan is not ordinal 2, default to standard COB.
                    var patPortionBase = procFee - paidOtherInsBaseTemp - writeOffOtherIns; //patPortion for BaseEst
                    maxPossibleToPay = Math.Min(claimProc.BaseEst, patPortionBase);
                }
                else
                {
                    //plan.CobRule==EnumCobRule.CarveOut
                    maxPossibleToPay = claimProc.BaseEst - paidOtherInsBaseTemp;
                }

                if (maxPossibleToPay < 0) maxPossibleToPay = 0;
                if (claimProc.BaseEst > maxPossibleToPay) claimProc.BaseEst = maxPossibleToPay; //reduce the base est
            }
        }

        //Canadian Lab Fee Estimates-------------------------------------------------------------------------------------------------------------------
        if (Canadian.IsValidForLabEstimates(insPlan) && doCheckCanadianLabs)
        {
            var listProceduresLabFees = Procedures.GetCanadianLabFees(procedure.ProcNum);
            for (var i = 0; i < listProceduresLabFees.Count; i++) CanadianLabBaseEstHelper(claimProc, listProceduresLabFees[i], insPlan, claimProc.InsSubNum, procedure, listBenefits, patPlanNum, listClaimProcHists, listClaimProcHistsLoop, patientAge, useProcDateOnProc);
        }

        //Exclusions---------------------------------------------------------------------------------------
        //We are not going to consider date of proc.  Just simple exclusions
        if (Benefits.IsExcluded(ProcedureCodes.GetStringProcCode(codeNum), listBenefits, insPlan.PlanNum, patPlanNum))
        {
            claimProc.BaseEst = 0;
            claimProc.InsEstTotal = 0;
            if (claimProc.EstimateNote != "") claimProc.EstimateNote += ", ";
            if (PrefC.GetBool(PrefName.InsPlanExclusionsMarkDoNotBillIns)) claimProc.NoBillIns = true;
            claimProc.EstimateNote += Lans.g("ClaimProcs", "Exclusion");
        }

        //base estimate is now done and will not be altered further.  From here out, we are only altering insEstTotal
        //annual max and other limitations--------------------------------------------------------------------------------
        var doZeroWriteoff = false;
        if (listClaimProcHistsLoop != null && listClaimProcHists != null)
        {
            var note = "";
            claimProc.InsEstTotal = Benefits.GetLimitationByCode(listBenefits, insPlan.PlanNum, patPlanNum, dateProc
                , ProcedureCodes.GetStringProcCode(codeNum), listClaimProcHists, listClaimProcHistsLoop, insPlan, claimProc.PatNum, out note, claimProc.InsEstTotal, patientAge
                , claimProc.InsSubNum, claimProc.InsEstTotalOverride, out var limitationMet);
            if ((limitationMet == LimitationTypeMet.PeriodMax || limitationMet == LimitationTypeMet.FamilyPeriodMax)
                && CompareDouble.IsLessThanOrEqualToZero(claimProc.InsEstTotal) && InsPlans.DoZeroOutWriteOffOnAnnualMaxLimitation(insPlan))
                doZeroWriteoff = true;
            else if (limitationMet == LimitationTypeMet.Aging && InsPlans.DoZeroOutWriteOffOnOtherLimitation(insPlan)) doZeroWriteoff = true;
            if (note != "")
            {
                if (claimProc.EstimateNote != "") claimProc.EstimateNote += ", ";
                claimProc.EstimateNote += note;
            }
        }

        //procDate;//was already calculated in the deductible section.
        //Writeoff Estimate------------------------------------------------------------------------------------------
        if (insPlan != null && InsPlans.UsesUcrFeeForExclusions(insPlan.ExclusionFeeRule) && (Benefits.IsExcluded(ProcedureCodes.GetStringProcCode(codeNum), listBenefits, insPlan.PlanNum, patPlanNum)
                                                                                              || Benefits.GetPercent(ProcedureCodes.GetStringProcCode(codeNum), insPlan.PlanType, insPlan.PlanNum, patPlanNum, listBenefits) == 0))
        {
            switch (insPlan.PlanType)
            {
                case "p":
                    claimProc.WriteOffEst = 0;
                    break;
                default: //Category Percent and Flat Copay/Medicaid both get -1
                    claimProc.WriteOffEst = -1;
                    break;
            }
        }
        else if (insPlan.CobRule == EnumCobRule.SecondaryMedicaid && PatPlans.GetOrdinal(claimProc.InsSubNum, listPatPlans) == 2)
        {
            //If a plan is Secondary Medicaid and ordinal 2, any amount that has not been written off by another insurance or paid will be written off.
            //This should cause the patient portion to be 0.
            claimProc.WriteOffEst = procFee - paidOtherInsTot - writeOffOtherIns - claimProc.InsEstTotal;
            if (claimProc.WriteOffEst < 0) claimProc.WriteOffEst = 0;
        }
        else if (insPlan.PlanType == "p" //PPO
                 //and this is a substituted code that doesn't calculate writeoffs
                 && !codeSubstNone && !insPlan.HasPpoSubstWriteoffs
                 && ProcedureCodes.GetSubstituteCodeNum(ProcedureCodes.GetProcCode(codeNum).ProcCode, toothNum, insPlan.PlanNum, listSubstitutionLinks) != codeNum) //there is a substitution for this code
        {
            //Using -1 will cause the estimate to show as blank in the edit claim procedure (FormClaimProc) window.
            //If we used 0, then the 0 would show, which might give the user the impression that we are calculating writeoffs.
            claimProc.WriteOffEst = -1;
        }
        else if (insPlan.PlanType == "p")
        {
            //PPO
            //we can't use the allowed previously calculated, because it might be the allowed of a substituted code.
            //so we will calculate the allowed all over again, but this time, without using a substitution code.
            //AllowedFeeSched and toothNum do not need to be passed in.  codeSubstNone is set to true to not subst.
            var carrierAllowedNoSubst = InsPlans.GetAllowed(ProcedureCodes.GetProcCode(codeNum).ProcCode, insPlan.FeeSched, 0,
                true, "p", "", procedure.ProvNum, procedure.ClinicNum, insPlan.PlanNum, listSubstitutionLinks, lookupFees);
            var allowedNoSubst = procFee;
            if (carrierAllowedNoSubst != -1)
            {
                carrierAllowedNoSubst = carrierAllowedNoSubst * procedure.Quantity;
                if (carrierAllowedNoSubst > procFee)
                    allowedNoSubst = procFee;
                else
                    allowedNoSubst = carrierAllowedNoSubst;
            }

            var normalWriteOff = procFee - allowedNoSubst; //This is what the normal writeoff would be if no other insurance was involved.
            if (normalWriteOff < 0) normalWriteOff = 0;
            var remainingWriteOff = procFee - paidOtherInsTot - writeOffOtherIns; //This is the fee minus whatever other ins has already paid or written off.
            if (remainingWriteOff < 0) remainingWriteOff = 0;
            if ((!PrefC.GetBool(PrefName.InsPPOsecWriteoffs) && paidOtherInsTot > 0) || writeOffOtherIns > 0)
                //This pref solves a conflict between two customers.  One customer paid for a past feature request.
                //They need this new preference because they have a non-PPO as primary and a pseudo-PPO (Medicaid flagged as PPO) as secondary.
                //When the pref is true, then secondary writeoffs are only included if other insurance has no writeoffs already.  This is how we used to calculate secondary writeoffs for everyone.
                //When the pref is false (default), then no secondary writeoff estimates allowed.  Only primary may have writeoffs.  If no other insurance payments/estimates/writeoffs, then the current writeoff is calculated as primary.
                claimProc.WriteOffEst = 0; //The reasoning for this is covered in the manual under Unit Test #1 and COB.
            //We can't go over either number.  We must use the smaller of the two.  If one of them is zero, then the writeoff is zero.
            else if (remainingWriteOff == 0 || normalWriteOff == 0)
                claimProc.WriteOffEst = 0;
            else if (remainingWriteOff <= normalWriteOff)
                claimProc.WriteOffEst = remainingWriteOff;
            else if (claimProc.IsOverpay) //IsOverpay shows that it is either an overPayment or underPayment, so set WriteOffEst=0
                claimProc.WriteOffEst = 0;
            else
                claimProc.WriteOffEst = normalWriteOff;
            if (paidOtherInsTot + writeOffOtherIns + claimProc.InsEstTotal + claimProc.WriteOffEst > procFee)
                //The ins paid and writeoffs from all procedures is greater than the proc fee. Reduce the remaining writeoff.
                claimProc.WriteOffEst = Math.Max(0, procFee - paidOtherInsTot - writeOffOtherIns - claimProc.InsEstTotal);
        }
        //capitation calculation never makes it this far:
        //else if(plan.PlanType=="c") {//capitation
        //	cp.WriteOffEst=cp.WriteOff;//this probably needs to change
        //}
        else
        {
            claimProc.WriteOffEst = -1;
        }

        //Round now to prevent the sum of the InsEstTotal and the Patient Portion from being 1 cent more than the Proc Fee
        claimProc.BaseEst = (double) Math.Round((decimal) claimProc.BaseEst, 2);
        claimProc.InsEstTotal = (double) Math.Round((decimal) claimProc.InsEstTotal, 2);
        //Calculations done, copy over estimates from InsEstTotal into InsPayEst.  
        //This could potentially be limited to claimprocs status Recieved or NotReceived, but there likely is no harm in doing it for all claimprocs.
        if (!CompareDouble.IsEqual(claimProc.InsEstTotalOverride, -1))
            claimProc.InsPayEst = claimProc.InsEstTotalOverride;
        else
            claimProc.InsPayEst = claimProc.InsEstTotal;
        if (doZeroWriteoff && GetEstimatedStatuses().Contains(claimProc.Status)) claimProc.WriteOffEst = 0;
    }

    ///<summary>Determine which date to use for procDate when computing base estimate.</summary>
    private static DateTime GetProcDate(Procedure procedure, ClaimProc claimProc, bool useProcDateOnProc)
    {
        DateTime dateProc;
        if (useProcDateOnProc)
            dateProc = procedure.ProcDate;
        else if (claimProc.Status == ClaimProcStatus.Estimate)
            dateProc = DateTime.Today;
        else
            dateProc = claimProc.ProcDate;
        return dateProc;
    }

    ///<summary>Append a string to the ClaimProc's EstimateNote field.</summary>
    private static void AppendToEstimateNote(ClaimProc claimProc, string note)
    {
        if (string.IsNullOrEmpty(note)) return;
        if (claimProc.EstimateNote != "") claimProc.EstimateNote += ", ";
        claimProc.EstimateNote += note;
    }

    /// <summary>
    ///     Update or create a claimProc for the given Canadian lab procedure (procLab).
    ///     Calculations are based on the percentage from the parent claim procs percentage (procParent).
    ///     Optionally pass in a list of claimprocs that will be manipulated if a new claimproc is created by this method.
    /// </summary>
    private static void CanadianLabBaseEstHelper(ClaimProc claimProcParent, Procedure procedureLab, InsPlan insPlan, long insSubNum,
        Procedure procedureParent, List<Benefit> listBenefits, long patPlanNum, List<ClaimProcHist> listClaimProcHists, List<ClaimProcHist> listClaimProcHistsLoop,
        int patientAge, bool useProcDateOnProc)
    {
        if (listClaimProcHists == null) listClaimProcHists = new List<ClaimProcHist>();
        if (listClaimProcHistsLoop == null) listClaimProcHistsLoop = new List<ClaimProcHist>();
        double percentage = 0;
        if (claimProcParent.Percentage != -1) //Can happen if claimProcParent.NoBillIns==true.
            percentage = claimProcParent.Percentage;
        if (claimProcParent.PercentOverride != -1) percentage = claimProcParent.PercentOverride;
        var note = "";
        var estAmt = procedureLab.ProcFee * percentage / 100d;
        double primaryEst = 0;
        //calculate estimates on lower ordinal claimprocs for the current lab fee claimproc.
        //this ensures that the secondary do not ignore the primary estimate
        var listPatPlans = new List<PatPlan>();
        byte ordinalCur = 0;
        if (patPlanNum != 0)
        {
            listPatPlans = PatPlans.GetPatPlansForPat(claimProcParent.PatNum);
            ordinalCur = (listPatPlans.Find(x => x.PatPlanNum == patPlanNum)
                          ?? PatPlans.GetByPatPlanNum(patPlanNum))
                ?.Ordinal ?? 0; //coalesce to 0 just in case patPlanNum is 0 to avoid a null, if check below that uses this is then skipped
        }

        var listClaimProcs = RefreshForProc(procedureLab.ProcNum); //Get all claimProcs for this lab (0, 1 or 2).
        if (ordinalCur > 1)
            for (var i = 0; i < listClaimProcs.Count; i++)
            {
                int ord = PatPlans.GetByInsSubNum(listPatPlans, listClaimProcs[i].InsSubNum)?.Ordinal ?? 0;
                if (ord == 1) primaryEst += listClaimProcs[i].InsPayEst;
            }

        var baseEst = Math.Max(estAmt - primaryEst, 0);
        var isPreauth = claimProcParent.Status == ClaimProcStatus.Preauth;
        listClaimProcs = listClaimProcs.FindAll(x => x.InsSubNum == claimProcParent.InsSubNum && x.PlanNum == claimProcParent.PlanNum //Same subscriber and insurance.
                                                                                              && x.Status == claimProcParent.Status //Ensure parent proc and lab proc have same status.
                                                                                              && (isPreauth ? x.ClaimNum == claimProcParent.ClaimNum : true)); //Preauths claimProcs can be sent to same insurance many times, claim specific.
        if (listClaimProcs.Count > 0)
        {
            //There exists 1 or 2 estimates for the current lab proc.
            for (var i = 0; i < listClaimProcs.Count; i++)
            {
                //Allow fee billed to update for estimates, or if the claim is unsent
                if (listClaimProcs[i].Status.In(ClaimProcStatus.Estimate, ClaimProcStatus.CapEstimate, ClaimProcStatus.NotReceived)) listClaimProcs[i].FeeBilled = procedureLab.ProcFee;
                listClaimProcs[i].Status = claimProcParent.Status;
                listClaimProcs[i].ClaimNum = claimProcParent.ClaimNum;
                listClaimProcs[i].CodeSent = GetCanadianCodeSent(procedureLab.CodeNum); //Not used when sending a claim.
                if (listClaimProcs[i].Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental)) listClaimProcs[i].DateEntry = claimProcParent.DateEntry;
                if (listClaimProcs[i].NoBillIns)
                {
                    ZeroOutClaimProc(listClaimProcs[i]);
                }
                else
                {
                    listClaimProcs[i].BaseEst = baseEst;
                    listClaimProcs[i].InsEstTotal = baseEst;
                    listClaimProcs[i].Percentage = claimProcParent.Percentage;
                    listClaimProcs[i].PercentOverride = claimProcParent.PercentOverride;
                    listClaimProcs[i].EstimateNote = ""; //Cannot be edited by user, no risk of deleting user input.
                    note = "";
                    listClaimProcs[i].InsEstTotal = Benefits.GetLimitationByCode(listBenefits, insPlan.PlanNum, patPlanNum //Consider annual/lifetime max, benefits, etc
                        , GetProcDate(procedureParent, claimProcParent, useProcDateOnProc), GetCanadianCodeSent(procedureParent.CodeNum), listClaimProcHists
                        , listClaimProcHistsLoop, insPlan, listClaimProcs[i].PatNum, out note, listClaimProcs[i].InsEstTotal, patientAge
                        , listClaimProcs[i].InsSubNum, listClaimProcs[i].InsEstTotalOverride, out _);
                    listClaimProcs[i].InsPayEst = GetInsEstTotal(listClaimProcs[i]);
                    AppendToEstimateNote(listClaimProcs[i], note);
                }

                //next proc needs to know this one is already added, 
                listClaimProcHistsLoop.AddRange(GetHistForProc(new List<ClaimProc> {listClaimProcs[i]}, procedureLab, procedureParent.CodeNum));
                Update(listClaimProcs[i]);
            }

            return;
        }

        //Create a new ClaimProc since we couldn't find one for this Lab.
        var claimProcLab = new ClaimProc();
        var insSub = new InsSub();
        insSub.InsSubNum = insSubNum;
        CreateEst(claimProcLab, procedureLab, insPlan, insSub, baseEst, baseEst);
        claimProcLab.Status = claimProcParent.Status;
        if (claimProcLab.Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental)) claimProcLab.DateEntry = claimProcParent.DateEntry;
        claimProcLab.ClaimNum = claimProcParent.ClaimNum;
        claimProcLab.CodeSent = GetCanadianCodeSent(procedureLab.CodeNum); //Not used when sending a claim.
        claimProcLab.NoBillIns = claimProcParent.NoBillIns;
        if (claimProcLab.NoBillIns)
        {
            ZeroOutClaimProc(claimProcLab);
        }
        else
        {
            claimProcLab.FeeBilled = procedureLab.ProcFee;
            claimProcLab.Percentage = claimProcParent.Percentage;
            claimProcLab.PercentOverride = claimProcParent.PercentOverride;
            claimProcLab.EstimateNote = ""; //Cannot be edited by user, no risk of deleting user input.
            note = "";
            claimProcLab.InsEstTotal = Benefits.GetLimitationByCode(listBenefits, insPlan.PlanNum, patPlanNum
                , GetProcDate(procedureParent, claimProcParent, useProcDateOnProc), GetCanadianCodeSent(procedureParent.CodeNum), listClaimProcHists
                , listClaimProcHistsLoop, insPlan, claimProcLab.PatNum, out note, claimProcLab.InsEstTotal, patientAge
                , claimProcLab.InsSubNum, claimProcLab.InsEstTotalOverride, out _);
            claimProcLab.InsPayEst = GetInsEstTotal(claimProcLab);
            AppendToEstimateNote(claimProcLab, note);
        }

        listClaimProcHistsLoop.AddRange(GetHistForProc(new List<ClaimProc> {claimProcLab}, procedureLab, procedureParent.CodeNum));
        Update(claimProcLab);
    }

    ///<summary>If the given procedure status is in a "completed" state, set the CodeSent, othewise set it to an empty string.</summary>
    private static string GetCanadianCodeSent(long codeNum)
    {
        var code = ProcedureCodes.GetStringProcCode(codeNum);
        if (code.Length > 5) //In Canadian electronic claims, codes can contain letters or numbers and cannot be longer than 5 characters.
            code = code.Substring(0, 5);
        return code;
    }

    /// <summary>
    ///     Updates pertinent lab claimproc statuses for given parentClaimProc.
    ///     Only updates the statuses for claimprocs associated to the same plan as given parentClaimProc.
    ///     Simply returns if given insPlan is not valid for lab estimates.
    /// </summary>
    public static void UpdatePertinentLabStatuses(ClaimProc claimProcParent, InsPlan insPlan)
    {
        if (!Canadian.IsValidForLabEstimates(insPlan)) return;
        var isOnClaim = claimProcParent.Status.In(ClaimProcStatus.Preauth, ClaimProcStatus.NotReceived, ClaimProcStatus.Received, ClaimProcStatus.CapClaim, ClaimProcStatus.Supplemental);
        var listClaimProcsLab = GetAllLabClaimProcsForParentProcNum(claimProcParent.ProcNum);
        listClaimProcsLab.RemoveAll(x => //Mimics CanadianLabBaseEstHelper(...)
            x.InsSubNum != claimProcParent.InsSubNum ||
            x.PlanNum != claimProcParent.PlanNum ||
            (isOnClaim ? x.ClaimNum != claimProcParent.ClaimNum : false)
        );
        for (var i = 0; i < listClaimProcsLab.Count; i++) listClaimProcsLab[i].Status = claimProcParent.Status;
        UpdateMany(listClaimProcsLab);
    }

    ///<summary>Returns all ClaimProcs associated to any lab procedures that may point the the given parentProcNum.</summary>
    public static List<ClaimProc> GetAllLabClaimProcsForParentProcNum(long parentProcNum)
    {
        var command = $@"SELECT claimproc.*
				FROM claimproc
				INNER JOIN procedurelog ON claimproc.ProcNum=procedurelog.ProcNum 
				AND ProcNumLab={SOut.Long(parentProcNum)}";
        return ClaimProcCrud.SelectMany(command);
    }

    /// <summary>
    ///     Typically called when cp.NoBillIns is true.
    ///     Sets various ClaimProc fields to 0 or -1 (indicates blank) where appropriate.
    /// </summary>
    private static void ZeroOutClaimProc(ClaimProc claimProc)
    {
        claimProc.AllowedOverride = -1;
        claimProc.CopayAmt = 0;
        claimProc.CopayOverride = -1;
        claimProc.Percentage = -1;
        claimProc.PercentOverride = -1;
        claimProc.DedEst = -1;
        claimProc.DedEstOverride = -1;
        claimProc.PaidOtherIns = -1;
        claimProc.BaseEst = 0;
        claimProc.InsEstTotal = 0;
        claimProc.InsEstTotalOverride = -1;
        claimProc.WriteOff = 0;
        claimProc.PaidOtherInsOverride = -1;
        claimProc.WriteOffEst = -1;
        claimProc.WriteOffEstOverride = -1;
        claimProc.EstimateNote = "";
    }

    /// <summary>
    ///     Only useful if secondary ins or greater.  For one procedure, it gets the sum of InsEstTotal/Override for other
    ///     insurances with lower ordinals.  Either estimates or actual payments.  Will return 0 if ordinal of this claimproc
    ///     is 1.
    /// </summary>
    public static double GetPaidOtherInsTotal(ClaimProc claimProc, List<PatPlan> listPatPlans)
    {
        if (claimProc.ProcNum == 0) return 0;
        var thisOrdinal = PatPlans.GetOrdinal(claimProc.InsSubNum, listPatPlans);
        if (thisOrdinal == 1) return 0;
        var command = "SELECT InsSubNum,InsEstTotal,InsEstTotalOverride,InsPayAmt,Status FROM claimproc WHERE ProcNum=" + SOut.Long(claimProc.ProcNum);
        var table = DataCore.GetTable(command);
        double retVal = 0;
        long subNum;
        int ordinal;
        double insEstTotal;
        double insEstTotalOverride;
        double insPayAmt;
        ClaimProcStatus status;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            subNum = SIn.Long(table.Rows[i]["InsSubNum"].ToString());
            ordinal = PatPlans.GetOrdinal(subNum, listPatPlans);
            if (ordinal >= thisOrdinal) continue;
            insEstTotal = SIn.Double(table.Rows[i]["InsEstTotal"].ToString());
            insEstTotalOverride = SIn.Double(table.Rows[i]["InsEstTotalOverride"].ToString());
            insPayAmt = SIn.Double(table.Rows[i]["InsPayAmt"].ToString());
            status = (ClaimProcStatus) SIn.Int(table.Rows[i]["Status"].ToString());
            if (status == ClaimProcStatus.Received || status == ClaimProcStatus.Supplemental) retVal += insPayAmt;
            if (status != ClaimProcStatus.Estimate && status != ClaimProcStatus.NotReceived) continue;
            if (insEstTotalOverride != -1)
                retVal += insEstTotalOverride;
            else
                retVal += insEstTotal;
        }

        return retVal;
    }

    /// <summary>
    ///     Only useful if secondary ins or greater.  For one procedure, it gets the sum of BaseEst for other insurances
    ///     with lower ordinals.  Either estimates or actual payments.  Will return 0 if ordinal of this claimproc is 1.
    /// </summary>
    public static double GetPaidOtherInsBaseEst(ClaimProc claimProc, List<PatPlan> listPatPlans)
    {
        if (claimProc.ProcNum == 0) return 0;
        var thisOrdinal = PatPlans.GetOrdinal(claimProc.InsSubNum, listPatPlans);
        if (thisOrdinal == 1) return 0;
        var command = "SELECT InsSubNum,BaseEst,InsPayAmt,Status FROM claimproc WHERE ProcNum=" + SOut.Long(claimProc.ProcNum);
        var table = DataCore.GetTable(command);
        double retVal = 0;
        long subNum;
        int ordinal;
        double baseEst;
        double insPayAmt;
        ClaimProcStatus status;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            subNum = SIn.Long(table.Rows[i]["InsSubNum"].ToString());
            ordinal = PatPlans.GetOrdinal(subNum, listPatPlans);
            if (ordinal >= thisOrdinal) continue;
            baseEst = SIn.Double(table.Rows[i]["BaseEst"].ToString());
            insPayAmt = SIn.Double(table.Rows[i]["InsPayAmt"].ToString());
            status = (ClaimProcStatus) SIn.Int(table.Rows[i]["Status"].ToString());
            if (status == ClaimProcStatus.Received || status == ClaimProcStatus.Supplemental) retVal += insPayAmt;
            if (status == ClaimProcStatus.Estimate || status == ClaimProcStatus.NotReceived) retVal += baseEst;
        }

        return retVal;
    }

    /// <summary>
    ///     Only useful if secondary ins or greater.  For one procedure, it gets the sum of WriteOffEstimates/Override for
    ///     other insurances with lower ordinals.  Either estimates or actual writeoffs.  Will return 0 if ordinal of this
    ///     claimproc is 1.
    /// </summary>
    public static double GetWriteOffOtherIns(ClaimProc claimProcs, List<PatPlan> listPatPlans)
    {
        if (claimProcs.ProcNum == 0) return 0;
        var thisOrdinal = PatPlans.GetOrdinal(claimProcs.InsSubNum, listPatPlans);
        if (thisOrdinal == 1) return 0;
        var command = "SELECT InsSubNum,WriteOffEst,WriteOffEstOverride,WriteOff,Status FROM claimproc WHERE ProcNum=" + SOut.Long(claimProcs.ProcNum);
        var table = DataCore.GetTable(command);
        double retVal = 0;
        long subNum;
        int ordinal;
        double writeOffEst;
        double writeOffEstOverride;
        double writeOff;
        ClaimProcStatus status;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            subNum = SIn.Long(table.Rows[i]["InsSubNum"].ToString());
            ordinal = PatPlans.GetOrdinal(subNum, listPatPlans);
            if (ordinal >= thisOrdinal) continue;
            writeOffEst = SIn.Double(table.Rows[i]["WriteOffEst"].ToString());
            writeOffEstOverride = SIn.Double(table.Rows[i]["WriteOffEstOverride"].ToString());
            writeOff = SIn.Double(table.Rows[i]["WriteOff"].ToString());
            status = (ClaimProcStatus) SIn.Int(table.Rows[i]["Status"].ToString());
            if (status == ClaimProcStatus.Received || status == ClaimProcStatus.Supplemental) retVal += writeOff;
            if (status != ClaimProcStatus.Estimate && status != ClaimProcStatus.NotReceived) continue;
            if (writeOffEstOverride != -1)
                retVal += writeOffEstOverride;
            else if (writeOffEst != -1) retVal += writeOffEst;
        }

        return retVal;
    }

    /////<summary>Only useful if secondary ins or greater.  For one procedure, it gets the sum of WriteOffEstimates/Override for other insurances with lower ordinals.  Either estimates or actual writeoffs.  Will return 0 if ordinal of this claimproc is 1.</summary>
    //public static double GetDeductibleOtherIns(ClaimProc cp,List<PatPlan> patPlanList) {
    //  
    //  if(cp.ProcNum==0) {
    //    return 0;
    //  }
    //  int thisOrdinal=PatPlans.GetOrdinal(cp.InsSubNum,patPlanList);
    //  if(thisOrdinal==1) {
    //    return 0;
    //  }
    //  string command="SELECT InsSubNum,DedEst,DedEstOverride,DedApplied,Status FROM claimproc WHERE ProcNum="+POut.Long(cp.ProcNum);
    //  DataTable table=DataCore.GetTable(command);
    //  double retVal=0;
    //  long subNum;
    //  int ordinal;
    //  double dedEst;
    //  double dedEstOverride;
    //  double dedApplied;
    //  ClaimProcStatus status;
    //  for(int i=0;i<table.Rows.Count;i++) {
    //    subNum=PIn.Long(table.Rows[i]["InsSubNum"].ToString());
    //    ordinal=PatPlans.GetOrdinal(subNum,patPlanList);
    //    if(ordinal >= thisOrdinal) {
    //      continue;
    //    }
    //    dedEst=PIn.Double(table.Rows[i]["DedEst"].ToString());
    //    dedEstOverride=PIn.Double(table.Rows[i]["DedEstOverride"].ToString());
    //    dedApplied=PIn.Double(table.Rows[i]["DedApplied"].ToString());
    //    status=(ClaimProcStatus)PIn.Int(table.Rows[i]["Status"].ToString());
    //    if(status==ClaimProcStatus.Received || status==ClaimProcStatus.Supplemental) {
    //      retVal+=dedApplied;
    //    }
    //    if(status==ClaimProcStatus.Estimate || status==ClaimProcStatus.NotReceived) {
    //      if(dedEstOverride != -1) {
    //        retVal+=dedEst;
    //      }
    //      else if(dedEst !=-1){
    //        retVal+=dedEst;
    //      }
    //    }
    //  }
    //  return retVal;
    //}

    ///<summary>Simply gets insEstTotal or its override if applicable.</summary>
    public static double GetInsEstTotal(ClaimProc claimProc)
    {
        if (claimProc.InsEstTotalOverride != -1) return claimProc.InsEstTotalOverride;
        return claimProc.InsEstTotal;
    }

    ///<summary>Simply gets dedEst or its override if applicable.  Can return 0, but never -1.</summary>
    public static double GetDedEst(ClaimProc claimProc)
    {
        if (claimProc.DedEstOverride != -1) return claimProc.DedEstOverride;

        if (claimProc.DedEst != -1) return claimProc.DedEst;
        return 0;
    }

    ///<summary>Gets either the override or the calculated write-off estimate.  Or zero if neither.</summary>
    public static double GetWriteOffEstimate(ClaimProc claimProc)
    {
        if (claimProc.WriteOffEstOverride != -1) return claimProc.WriteOffEstOverride;

        if (claimProc.WriteOffEst != -1) return claimProc.WriteOffEst;
        return 0;
    }

    public static string GetPercentageDisplay(ClaimProc claimProc)
    {
        if (claimProc.Status == ClaimProcStatus.CapEstimate || claimProc.Status == ClaimProcStatus.CapComplete) return "";
        if (claimProc.PercentOverride != -1) return claimProc.PercentOverride.ToString();

        if (claimProc.Percentage != -1) return claimProc.Percentage.ToString();
        return "";
    }

    public static string GetCopayDisplay(ClaimProc claimProc)
    {
        if (claimProc.CopayOverride != -1) return claimProc.CopayOverride.ToString("f");

        if (claimProc.CopayAmt != -1) return claimProc.CopayAmt.ToString("f");
        return "";
    }

    
    public static string GetWriteOffEstimateDisplay(ClaimProc claimProc)
    {
        if (claimProc.WriteOffEstOverride != -1) return claimProc.WriteOffEstOverride.ToString("f");

        if (claimProc.WriteOffEst != -1) return claimProc.WriteOffEst.ToString("f");
        return "";
    }

    public static string GetEstimateDisplay(ClaimProc claimProc)
    {
        if (claimProc.Status == ClaimProcStatus.CapEstimate || claimProc.Status == ClaimProcStatus.CapComplete) return "";
        if (claimProc.Status == ClaimProcStatus.Estimate)
        {
            if (claimProc.InsEstTotalOverride != -1) return claimProc.InsEstTotalOverride.ToString("f"); //shows even if 0.
            return claimProc.InsEstTotal.ToString("f");
        }

        return claimProc.InsPayEst.ToString("f");
    }

    ///<summary>Returns 0 or -1 if no deduct.</summary>
    public static double GetDeductibleDisplay(ClaimProc claimProc)
    {
        if (claimProc.Status == ClaimProcStatus.CapEstimate || claimProc.Status == ClaimProcStatus.CapComplete) return -1;
        if (claimProc.Status == ClaimProcStatus.Estimate)
        {
            if (claimProc.DedEstOverride != -1) return claimProc.DedEstOverride;
            //else if(cp.DedEst > 0) {
            return claimProc.DedEst; //could be -1
            //}
            //else {
            //	return "";
            //}
        }

        return claimProc.DedApplied;
    }

    ///<summary>Used in TP module.  Gets all estimate notes for this proc.</summary>
    public static string GetEstimateNotes(long procNum, List<ClaimProc> listClaimProcs)
    {
        var retVal = "";
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ProcNum != procNum) continue;
            if (listClaimProcs[i].EstimateNote == "") continue;
            if (retVal != "") retVal += ", ";
            retVal += listClaimProcs[i].EstimateNote;
        }

        return retVal;
    }

    public static double GetTotalWriteOffEstimateDisplay(List<ClaimProc> listClaimProcs, long procNum)
    {
        double retVal = 0;
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ProcNum != procNum) continue;
            if (listClaimProcs[i].WriteOffEstOverride != -1)
                retVal += listClaimProcs[i].WriteOffEstOverride;
            else if (listClaimProcs[i].WriteOffEst != -1) retVal += listClaimProcs[i].WriteOffEst;
        }

        return retVal;
    }

    
    public static List<ClaimProcHist> GetPatientData(long patNum, List<Benefit> listBenefits, List<PatPlan> listPatPlans, List<InsPlan> listInsPlans, List<InsSub> listInsSubs)
    {
        return GetHistList(patNum, listBenefits, listPatPlans, listInsPlans, -1, DateTime.Today, listInsSubs);
    }

    public static List<ClaimProcHist> GetHistList(long patNum, List<Benefit> listBenefits, List<PatPlan> listPatPlans, List<InsPlan> listInsPlans, DateTime dateProc, List<InsSub> listInsSubs)
    {
        return GetHistList(patNum, listBenefits, listPatPlans, listInsPlans, -1, dateProc, listInsSubs);
    }

    /// <summary>
    ///     We pass in the benefit list so that we know whether to include family.  We are getting a simplified list of
    ///     claimprocs.
    ///     History of payments and pending payments.  If the patient has multiple insurance, then this info will be for all of
    ///     their insurance plans.
    ///     It runs a separate query for each plan because that's the only way to handle family history.
    ///     For some plans, the benefits will indicate entire family, but not for other plans.  And the date ranges can be
    ///     different as well.
    ///     When this list is processed later, it is again filtered, but it can't have missing information.  Use
    ///     excludeClaimNum=-1 to not exclude a claim.
    ///     A claim is excluded if editing from inside that claim.
    /// </summary>
    public static List<ClaimProcHist> GetHistList(long patNum, List<Benefit> listBenefits, List<PatPlan> listPatPlans, List<InsPlan> listInsPlans
        , long excludeClaimNum, DateTime dateProc, List<InsSub> listInsSubs)
    {
        var listClaimProcHists = new List<ClaimProcHist>();
        InsSub insSub;
        InsPlan insPlan;
        bool isFam;
        bool isLife;
        DateTime dateStart;
        DataTable table;
        ClaimProcHist claimProcHist;
        var listBenefitsLimitations = new List<Benefit>();
        for (var p = 0; p < listPatPlans.Count; p++)
        {
            //loop through each plan that this patient is covered by
            insSub = InsSubs.GetSub(listPatPlans[p].InsSubNum, listInsSubs);
            //get the plan for the given patPlan
            insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlans);
            if (insPlan == null) continue;
            //test benefits for fam and life
            isFam = false;
            isLife = false;
            for (var i = 0; i < listBenefits.Count; i++)
            {
                if (listBenefits[i].PlanNum == 0 && listBenefits[i].PatPlanNum != listPatPlans[p].PatPlanNum) continue;
                if (listBenefits[i].PatPlanNum == 0 && listBenefits[i].PlanNum != insPlan.PlanNum) continue;

                if (listBenefits[i].TimePeriod == BenefitTimePeriod.Lifetime) isLife = true;
                if (listBenefits[i].CoverageLevel == BenefitCoverageLevel.Family) isFam = true;
                if (listBenefits[i].BenefitType == InsBenefitType.Limitations //BW, Pano/FW, Exam, and Custom category frequency limitations
                    && listBenefits[i].MonetaryAmt == -1
                    && listBenefits[i].Percent == -1
                    && (listBenefits[i].QuantityQualifier == BenefitQuantity.Months
                        || listBenefits[i].QuantityQualifier == BenefitQuantity.Years
                        || listBenefits[i].QuantityQualifier == BenefitQuantity.NumberOfServices))
                    listBenefitsLimitations.Add(listBenefits[i]);
            }

            if (isLife)
                dateStart = new DateTime(1880, 1, 1);
            else
                //unsure what date to use to start.  DateTime.Today?  That might miss procs from late last year when doing secondary claim, InsPaidOther.
                //If we use the proc date, then it will indeed get an accurate history.  And future procedures just don't matter when calculating things.
                dateStart = BenefitLogic.ComputeRenewDate(dateProc, insPlan.MonthRenew);
            var dateRenew = dateStart;
            if (listBenefitsLimitations.Count > 0 && dateProc != DateTime.MinValue)
                //If there are limitation benefits, calculate the dateStart based on the limitation quantity and quantityqualifier.
                //If the limitation dictates the dateStart is prior to the renew date, use that instead.
                for (var i = 0; i < listBenefitsLimitations.Count; i++)
                {
                    var date = dateProc;
                    if (listBenefitsLimitations[i].QuantityQualifier == BenefitQuantity.Months)
                        date = DateTimeOD.CalculateForEndOfMonthOffset(dateProc, listBenefitsLimitations[i].Quantity);
                    else if (listBenefitsLimitations[i].QuantityQualifier == BenefitQuantity.Years)
                        date = DateTimeOD.CalculateForEndOfMonthOffset(dateProc, listBenefitsLimitations[i].Quantity * 12);
                    else if (listBenefitsLimitations[i].QuantityQualifier == BenefitQuantity.NumberOfServices) date = DateTimeOD.CalculateForEndOfMonthOffset(dateProc, 12);
                    if (date < dateStart) dateStart = date;
                }

            var command = "SELECT claimproc.ProcDate,CodeNum,InsPayEst,InsPayAmt,DedApplied,claimproc.PatNum,Status,ClaimNum,claimproc.InsSubNum,claimproc.ProcNum, "
                          + "claimproc.NoBillIns,procedurelog.Surf, procedurelog.ToothRange, procedurelog.ToothNum "
                          + "FROM claimproc "
                          + "LEFT JOIN procedurelog on claimproc.ProcNum=procedurelog.ProcNum " //to get the codenum
                          + "WHERE claimproc.InsSubNum=" + SOut.Long(listPatPlans[p].InsSubNum)
                          + " AND claimproc.ProcDate >= " + SOut.Date(dateStart) //no upper limit on date.
                          + " AND claimproc.Status IN("
                          + SOut.Long((int) ClaimProcStatus.NotReceived) + ","
                          + SOut.Long((int) ClaimProcStatus.Adjustment) + "," //insPayAmt and DedApplied
                          + SOut.Long((int) ClaimProcStatus.Received) + ","
                          + SOut.Long((int) ClaimProcStatus.InsHist) + ","
                          + SOut.Long((int) ClaimProcStatus.Supplemental) + ")";
            if (!isFam)
                //we include patnum because this one query can get results for multiple patients that all have this one subsriber.
                command += " AND claimproc.PatNum=" + SOut.Long(patNum);
            if (excludeClaimNum != -1) command += " AND claimproc.ClaimNum != " + SOut.Long(excludeClaimNum);
            table = DataCore.GetTable(command);
            var listCodeNumsLimitations = ProcedureCodes.GetCodeNumsForAllLimitations(listBenefitsLimitations, insPlan, listPatPlans[p].PatPlanNum);
            for (var i = 0; i < table.Rows.Count; i++)
            {
                var codeNum = SIn.Long(table.Rows[i]["CodeNum"].ToString());
                var claimProcDate = SIn.Date(table.Rows[i]["ProcDate"].ToString());
                if (claimProcDate < dateRenew && !listCodeNumsLimitations.Contains(codeNum)) continue; //If it's a claimproc that's older than the renew date, which means it may be for a frequency, but it doesn't have a frequency codenum, don't make a hist for it.
                claimProcHist = new ClaimProcHist();
                claimProcHist.ProcDate = claimProcDate;
                claimProcHist.StrProcCode = ProcedureCodes.GetStringProcCode(SIn.Long(table.Rows[i]["CodeNum"].ToString()));
                claimProcHist.Status = (ClaimProcStatus) SIn.Long(table.Rows[i]["Status"].ToString());
                if (claimProcHist.Status == ClaimProcStatus.NotReceived)
                    claimProcHist.Amount = SIn.Double(table.Rows[i]["InsPayEst"].ToString());
                else
                    claimProcHist.Amount = SIn.Double(table.Rows[i]["InsPayAmt"].ToString());
                claimProcHist.Deduct = SIn.Double(table.Rows[i]["DedApplied"].ToString());
                claimProcHist.PatNum = SIn.Long(table.Rows[i]["PatNum"].ToString());
                claimProcHist.ClaimNum = SIn.Long(table.Rows[i]["ClaimNum"].ToString());
                claimProcHist.InsSubNum = SIn.Long(table.Rows[i]["InsSubNum"].ToString());
                claimProcHist.ProcNum = SIn.Long(table.Rows[i]["ProcNum"].ToString());
                claimProcHist.PlanNum = insPlan.PlanNum;
                claimProcHist.Surf = SIn.String(table.Rows[i]["Surf"].ToString());
                claimProcHist.ToothRange = SIn.String(table.Rows[i]["ToothRange"].ToString());
                claimProcHist.ToothNum = SIn.String(table.Rows[i]["ToothNum"].ToString());
                claimProcHist.NoBillIns = SIn.Bool(table.Rows[i]["NoBillIns"].ToString());
                listClaimProcHists.Add(claimProcHist);
            }
        }

        return listClaimProcHists;
    }

    /// <summary>
    ///     Used in creation of the loopList.  Used in TP list estimation and in claim creation.  Some of the items in the
    ///     claimProcList passed in will not have been saved to the database yet.  The codeNum can be different than
    ///     Proc.CodeNum.
    /// </summary>
    public static List<ClaimProcHist> GetHistForProc(List<ClaimProc> listClaimProcs, Procedure procedure, long codeNum)
    {
        var listClaimProcHists = new List<ClaimProcHist>();
        ClaimProcHist claimProcHist;
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ProcNum != procedure.ProcNum) continue;
            claimProcHist = new ClaimProcHist();
            claimProcHist.ProcNum = listClaimProcs[i].ProcNum;
            claimProcHist.Amount = GetInsEstTotal(listClaimProcs[i]);
            claimProcHist.ClaimNum = 0;
            if (listClaimProcs[i].DedEstOverride != -1)
                claimProcHist.Deduct = listClaimProcs[i].DedEstOverride;
            else
                claimProcHist.Deduct = listClaimProcs[i].DedEst;
            claimProcHist.PatNum = listClaimProcs[i].PatNum;
            claimProcHist.PlanNum = listClaimProcs[i].PlanNum;
            claimProcHist.InsSubNum = listClaimProcs[i].InsSubNum;
            claimProcHist.ProcDate = DateTime.Today;
            claimProcHist.Status = ClaimProcStatus.Estimate;
            claimProcHist.StrProcCode = ProcedureCodes.GetStringProcCode(codeNum);
            claimProcHist.Surf = procedure.Surf;
            claimProcHist.ToothRange = procedure.ToothRange;
            claimProcHist.ToothNum = procedure.ToothNum;
            claimProcHist.NoBillIns = listClaimProcs[i].NoBillIns;
            listClaimProcHists.Add(claimProcHist);
        }

        return listClaimProcHists;
    }

    ///<summary>Gets a list of ClaimProcs attached to a specific ClaimPayment from the db. Returns an empty list if not found.</summary>
    public static List<ClaimProc> GetClaimProcsForClaimPayment(long claimPaymentNum)
    {
        var command = "SELECT * from claimproc"
                      + " WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum);
        return ClaimProcCrud.SelectMany(command);
    }

    /// <summary>
    ///     Attempts to set ProvNum on each supplied ClaimProc to the ProvNum on the supplied Procedure. Does not update
    ///     database.
    ///     Returns true if all ClaimProcs are set to the ProvNum. Returns false if at least one ClaimProc was not set, due to
    ///     the Procedure being
    ///     attached to a Claim and PrefName.ProcProvChangesClaimProcWithClaim being false.
    /// </summary>
    public static bool TrySetProvFromProc(Procedure procedure, List<ClaimProc> listClaimProcs)
    {
        var retVal = true;
        if (PrefC.GetBool(PrefName.ProcProvChangesClaimProcWithClaim))
        {
            //This will only change providers for claimproc estimates
            var listClaimProcsForProcedure = listClaimProcs.FindAll(x => x.ProcNum == procedure.ProcNum);
            for (var i = 0; i < listClaimProcsForProcedure.Count; i++) listClaimProcsForProcedure[i].ProvNum = procedure.ProvNum;
            return retVal;
        }

        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ProcNum != procedure.ProcNum) continue;
            //Change claimproc provnum only if not attached to Claim.
            if (!listClaimProcs[i].Status.In(ClaimProcStatus.Received, ClaimProcStatus.NotReceived, ClaimProcStatus.CapClaim))
                listClaimProcs[i].ProvNum = procedure.ProvNum;
            else
                retVal = false;
        }

        return retVal;
    }

    ///<summary>Does not make call to db unless necessary.</summary>
    public static void SetProvForProc(Procedure procedure, List<ClaimProc> listClaimProcs)
    {
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            if (listClaimProcs[i].ProcNum != procedure.ProcNum) continue;
            if (listClaimProcs[i].ProvNum == procedure.ProvNum) continue; //no change needed
            listClaimProcs[i].ProvNum = procedure.ProvNum;
            Update(listClaimProcs[i]);
        }
    }

    ///<summary>For moving rows up and down the batch insurance window.</summary>
    public static void SetPaymentRow(long claimNum, long claimPaymentNum, int paymentRow)
    {
        var command = "UPDATE claimproc SET PaymentRow=" + SOut.Int(paymentRow) + " "
                      + "WHERE ClaimNum=" + SOut.Long(claimNum) + " "
                      + "AND ClaimPaymentNum=" + SOut.Long(claimPaymentNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     For moving rows up and down the batch insurance window. For each ClaimNum in listClaimNums, the value at the
    ///     corresponding index in
    ///     listPaymentRows will be set as the PaymentRow. Be sure that listClaimNums and listPaymentRows have the same number
    ///     of items.
    /// </summary>
    public static void SetPaymentRow(List<long> listClaimNums, long claimPaymentNum, List<int> listPaymentRows)
    {
        if (listClaimNums == null || listClaimNums.Count == 0 || listPaymentRows == null || listClaimNums.Count != listPaymentRows.Count) return;
        var command = "UPDATE claimproc SET PaymentRow=(CASE ClaimNum ";
        for (var i = 0; i < listClaimNums.Count; i++) //Expected up to 10; To actual limit, one for each claim in FormClaimPayBatch.GridMain (100s or 1000s)
            command += "WHEN " + SOut.Long(listClaimNums[i]) + " THEN " + SOut.Int(listPaymentRows[i]) + " ";
        command += "END) WHERE ClaimNum IN (" + string.Join(",", listClaimNums.Select(x => SOut.Long(x))) + ") "
                   + "AND ClaimPaymentNum=" + SOut.Long(claimPaymentNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Attaches all claimprocs that have an InsPayAmt entered to the specified ClaimPayment,
    ///     and then returns a list of the claimprocs.  The claimprocs must be currently unattached.
    ///     Used from Edit Claim window (FormClaimEdit) when user is not doing the batch entry.
    ///     To finalize a single claim, set onlyOneClaimNum to the claimNum to finalize.
    /// </summary>
    public static List<ClaimProc> AttachAllOutstandingToPayment(long claimPaymentNum, DateTime dateClaimPayZero, long onlyOneClaimNum = 0)
    {
        //See job #7423.
        //The claimproc.DateCP is essentially the same as the claim.DateReceived.
        //We used to use the claimproc.ProcDate, which is essentially the same as the claim.DateService.
        //Since the service date could be weeks or months in the past, it makes more sense to use the received date, which will be more recent.
        //Additionally, users found using the date of service to be unintuitive.
        //STRONG CAUTION not to use the claimproc.ProcDate here in the future.
        var command = "UPDATE claimproc SET ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + " "
                      + "WHERE ClaimPaymentNum=0 "
                      + "AND claimproc.IsTransfer=0 " //do not attach transfer claimprocs to claim payments
                      + "AND claimproc.Status IN(" + string.Join(",", GetInsPaidStatuses().Select(x => SOut.Int((int) x))) + ") "
                      + "AND (InsPayAmt != 0 ";
        //See job #7517.
        //Always exclude NO PAYMENT claims and $0 claimprocs for batch payments created from the Edit Claim window.
        //Include $0 claimprocs on or after rolling date (if enabled) when finalizing an individual claim.
        if (onlyOneClaimNum != 0 && dateClaimPayZero.Year > 1880) command += "OR DateCP >= " + SOut.Date(dateClaimPayZero);
        command += ") ";
        if (onlyOneClaimNum != 0) //Finalizing individual claim.
            command += "AND ClaimNum=" + SOut.Long(onlyOneClaimNum);
        Db.NonQ(command);
        command = "SELECT * FROM claimproc WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum);
        return ClaimProcCrud.SelectMany(command);
    }

    /// <summary>
    ///     Called when we want to try and update claimProc.DateInsFinalized for claimProcs associated to the given
    ///     claimPaymentNum.
    ///     The filters inside this function mimic AttachAllOutstandingToPayment() above.
    ///     If finalizing a single claim, set onlyOneClaimNum to the claimNum to finalize.
    /// </summary>
    public static void DateInsFinalizedHelper(long claimPaymentNum, DateTime dateClaimPayZero, long onlyOneClaimNum = 0)
    {
        //See job #7423.
        //The claimproc.DateCP is essentially the same as the claim.DateReceived.
        //We used to use the claimproc.ProcDate, which is essentially the same as the claim.DateService.
        //Since the service date could be weeks or months in the past, it makes more sense to use the received date, which will be more recent.
        //Additionally, users found using the date of service to be unintuitive.
        //STRONG CAUTION not to use the claimproc.ProcDate here in the future.
        var command = "UPDATE claimproc SET "
                      + "DateInsFinalized = (CASE DateInsFinalized WHEN '0001-01-01' THEN " + DbHelper.Now() + " ELSE DateInsFinalized END) "
                      + "WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + " "
                      + "AND (claimproc.Status = '1' OR claimproc.Status = '4' OR claimproc.Status='5') " //received or supplemental or capclaim
                      + "AND (InsPayAmt != 0 ";
        //See job #7517.
        //Always exclude NO PAYMENT claims and $0 claimprocs for batch payments created from the Edit Claim window.
        //Include $0 claimprocs on or after rolling date (if enabled) when finalizing an individual claim.
        if (onlyOneClaimNum != 0 && dateClaimPayZero.Year > 1880) command += "OR DateCP >= " + SOut.Date(dateClaimPayZero);
        command += ") ";
        if (onlyOneClaimNum != 0) //Finalizing individual claim.
            command += "AND ClaimNum=" + SOut.Long(onlyOneClaimNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Pass in a cached or potentially "stale" list of claim procs and this method will check the ClaimNum against
    ///     the num stored in the database to make sure they still match.  Returns true if any of the claim procs are not
    ///     pointing to the same claim.
    /// </summary>
    public static bool IsAttachedToDifferentClaim(long procNum, List<ClaimProc> listClaimProcsFromCache)
    {
        var listClaimProcsFromDB = RefreshForProc(procNum);
        for (var i = 0; i < listClaimProcsFromCache.Count; i++)
        for (var j = 0; j < listClaimProcsFromDB.Count; j++)
        {
            if (listClaimProcsFromCache[i].ClaimProcNum != listClaimProcsFromDB[j].ClaimProcNum) continue;
            if (listClaimProcsFromCache[i].ClaimNum != listClaimProcsFromDB[j].ClaimNum) return true;
        }

        return false;
    }

    ///<summary>Gets the ProcNum, Status, and WriteOff for the passed in procedures.</summary>
    public static List<ClaimProc> GetForProcsLimited(List<long> listProcNums, params ClaimProcStatus[] claimProcStatusArray)
    {
        var listClaimProcs = new List<ClaimProc>();
        if (listProcNums == null || listProcNums.Count < 1 || claimProcStatusArray.Count() == 0) return listClaimProcs;
        var command = "SELECT ProcNum,Status,WriteOff FROM claimproc WHERE ProcNum IN(" + string.Join(",", listProcNums) + ") "
                      + "AND Status IN(" + string.Join(",", claimProcStatusArray.Select(x => (int) x)) + ")";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var claimProc = new ClaimProc();
            claimProc.ProcNum = SIn.Long(table.Rows[i]["ProcNum"].ToString());
            claimProc.Status = (ClaimProcStatus) SIn.Int(table.Rows[i]["Status"].ToString());
            claimProc.WriteOff = SIn.Double(table.Rows[i]["WriteOff"].ToString());
            listClaimProcs.Add(claimProc);
        }

        return listClaimProcs;
    }

    public static void DeleteMany(List<ClaimProc> listClaimProcs)
    {
        DeleteMany(listClaimProcs.Select(x => x.ClaimProcNum).ToList());
    }

    public static void DeleteMany(List<long> listClaimProcNums)
    {
        if (listClaimProcNums == null || listClaimProcNums.Count == 0) return;
        var command = "DELETE FROM claimproc WHERE ClaimProcNum IN(" + string.Join(",", listClaimProcNums) + ")";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Returns the salted hash for the claimproc. Will return an empty string if the calling program is unable to use
    ///     CDT.dll.
    /// </summary>
    public static string HashFields(ClaimProc claimProc)
    {
        var unhashedText = claimProc.ClaimNum + ((int) claimProc.Status) + claimProc.InsPayEst.ToString("F2") + claimProc.InsPayAmt.ToString("F2");
        try
        {
            return Class1.CreateSaltedHash(unhashedText);
        }
        catch (Exception ex)
        {
            return ex.GetType().Name;
        }
    }

    /// <summary>
    ///     Validates the hash string in claimproc.SecurityHash. Returns true if it matches the expected hash, otherwise
    ///     false.
    /// </summary>
    public static bool IsClaimProcHashValid(ClaimProc claimProc)
    {
        if (claimProc == null) return true;
        var dateHashStart = SecurityHash.GetHashingDate();
        if (claimProc.SecDateEntry < dateHashStart) //Too old, isn't hashed.
            return true;
        if (claimProc.SecurityHash == HashFields(claimProc)) return true;
        return false;
    }

    ///<summary>Bite-Sized ClaimProc class for speed enhancement</summary>
    public class ClaimProcQueued
    {
        public long ClaimNum;
        public long ClaimProcNum;
        public long ProcNum;
    }

    #region Get Methods

    /// <summary>
    ///     Can be used to calculate the patient portion estimate for a procedure from a list of claimprocs and list of
    ///     adjustments.
    /// </summary>
    public static decimal GetPatPortion(Procedure procedure, List<ClaimProc> listClaimProcs, List<Adjustment> listAdjustments = null,
        bool includeEstimates = true)
    {
        if (procedure == null || listClaimProcs == null) return 0;
        //PPO patient portion calculation is: Office Fee - Write-off - Insurance Payment + Adjustments = Patient Portion
        //The definition of some variables differ based on the claimproc.Status and overrides (if set).
        //Office Fee: Procedure Fee * (Base Units + Unit Quantity)
        //Write-off: WriteOff or WriteOffEst or WriteOffEstOverride
        //Insurance Payment: InsPayAmt or InsPayEst
        //Adjustments: Any account adjustment attached to the proc
        var listClaimProcsFiltered = listClaimProcs.FindAll(x => x.ProcNum == procedure.ProcNum && x.Status != ClaimProcStatus.Preauth);
        var patPort = (decimal) procedure.ProcFeeTotal;

        #region Insurance Payments

        patPort -= GetInsPay(listClaimProcsFiltered, includeEstimates);

        #endregion

        #region Adjustments (optional)

        listAdjustments = listAdjustments ?? new List<Adjustment>();
        patPort += listAdjustments
            .FindAll(x => x.ProcNum == procedure.ProcNum)
            .Sum(x => (decimal) x.AdjAmt);

        #endregion

        return Math.Round(patPort, 2);
    }

    /// <summary>
    ///     Returns the sum of what insurance paid, including write-offs, based off of each claimproc status. Optionally
    ///     ignore estimates.
    /// </summary>
    public static decimal GetInsPay(List<ClaimProc> listClaimProcs, bool includeEstimates = true)
    {
        decimal insPay = 0;
        if (listClaimProcs.IsNullOrEmpty()) return insPay;
        if (includeEstimates)
        {
            var listClaimProcsEst = listClaimProcs.FindAll(x => GetEstimatedStatuses().Contains(x.Status));
            insPay += listClaimProcsEst.Sum(x => (decimal) x.InsPayEst);
            insPay += listClaimProcsEst.Sum(x => (decimal) GetWriteOffEstimate(x));
        }

        var listPaidClaimProcs = listClaimProcs.FindAll(x => GetInsPaidStatuses().Contains(x.Status));
        insPay += listPaidClaimProcs.Sum(x => (decimal) x.InsPayAmt);
        insPay += listPaidClaimProcs.Sum(x => (decimal) x.WriteOff);
        return insPay;
    }

    ///<summary>Returns the patient portion for an entire claim.</summary>
    public static decimal GetPatPortionForClaim(Claim claim)
    {
        var listProcedures = Procedures.GetCompleteForPats(new List<long> {claim.PatNum});
        var listClaimProcs = RefreshForClaim(claim.ClaimNum, listProcedures);
        var listAdjustments = Adjustments.GetForProcs(listProcedures.Select(x => x.ProcNum).ToList());
        decimal totalPatPort = 0;
        //Go through our procs that are attached to the claim and add up the patient portion.
        var listProceduresAttached = listProcedures.FindAll(x => listClaimProcs.Any(y => y.ProcNum == x.ProcNum));
        for (var i = 0; i < listProceduresAttached.Count; i++) totalPatPort += GetPatPortion(listProceduresAttached[i], listClaimProcs, listAdjustments);
        return totalPatPort;
    }

    public static List<ClaimProcStatus> GetEstimatedStatuses()
    {
        var listClaimProcStatuses = new List<ClaimProcStatus>();
        listClaimProcStatuses.Add(ClaimProcStatus.NotReceived);
        listClaimProcStatuses.Add(ClaimProcStatus.Estimate);
        listClaimProcStatuses.Add(ClaimProcStatus.CapEstimate);
        listClaimProcStatuses.Add(ClaimProcStatus.InsHist);
        return listClaimProcStatuses;
    }

    public static List<ClaimProcStatus> GetInsPaidStatuses()
    {
        var listClaimProcStatuses = new List<ClaimProcStatus>();
        listClaimProcStatuses.Add(ClaimProcStatus.Received);
        listClaimProcStatuses.Add(ClaimProcStatus.Supplemental);
        listClaimProcStatuses.Add(ClaimProcStatus.CapComplete);
        listClaimProcStatuses.Add(ClaimProcStatus.CapClaim);
        return listClaimProcStatuses;
    }

    /// <summary>
    ///     Attempts to group up pay as totals on each claim and return at most, 1 pay as total per claim group. This is to
    ///     balance the
    ///     pay as totals so we do not transfer after a transfer has already been performed. Returns a list of PayAsTotals that
    ///     can be transferred.
    ///     Does not currently support capitation claims.  Throws exceptions.
    /// </summary>
    public static List<PayAsTotal> GetOutstandingClaimPayByTotal(List<long> listFamilyPatNums, List<ClaimProc> listClaimsAsTotalForPats = null)
    {
        var listClaimProcsTotal = listClaimsAsTotalForPats ?? GetByTotForPats(listFamilyPatNums)
            .FindAll(x => !x.Status.In(ClaimProcStatus.CapClaim, ClaimProcStatus.CapComplete, ClaimProcStatus.CapEstimate));
        //Loop through all As Total claimprocs and blindly sum ones together that are associated with the same claim.
        //Remove all As Total claimprocs for claims that sum to zero or less.
        var listClaimNums = new List<long>();
        for (var i = 0; i < listClaimProcsTotal.Count; i++)
        {
            var insPayAmtSum = listClaimProcsTotal.Where(x => x.ClaimNum == listClaimProcsTotal[i].ClaimNum).Sum(x => x.InsPayAmt);
            if (insPayAmtSum <= 0) listClaimNums.Add(listClaimProcsTotal[i].ClaimNum);
        }

        listClaimProcsTotal.RemoveAll(x => listClaimNums.Contains(x.ClaimNum));
        if (listClaimProcsTotal.Count == 0) return new List<PayAsTotal>();
        var listPayAsTotalsOutstanding = new List<PayAsTotal>(); //will hold claims pay by total that have not yet been transferred.
        // Need all unique combinations of these keys, each ClaimProc will act as the 'seed' for a group with this combination.
        var listClaimProcsGroups = listClaimProcsTotal.DistinctBy(x => new {x.ClaimNum, x.PatNum, x.ProvNum, x.ClinicNum}).ToList();
        for (var i = 0; i < listClaimProcsGroups.Count; i++)
        {
            // Get a group of all elements that have this ClaimProc's combination of keys.
            var listClaimProcsToSum = listClaimProcsTotal.FindAll(x => x.ClaimNum == listClaimProcsGroups[i].ClaimNum
                                                                       && x.PatNum == listClaimProcsGroups[i].PatNum
                                                                       && x.ProvNum == listClaimProcsGroups[i].ProvNum
                                                                       && x.ClinicNum == listClaimProcsGroups[i].ClinicNum)
                .ToList();
            var summedInsPay = listClaimProcsToSum.Sum(x => x.InsPayAmt);
            if (CompareDouble.IsZero(summedInsPay)) continue; //these claims as total have already been transferred, or didn't have value. Nothing to do, move on to the next.
            //else there is an imbalance that needs to be transferred
            //will not get saved to the DB, just placeholder (mostly for the amounts)
            var payAsTotal = new PayAsTotal();
            payAsTotal.ClaimNum = listClaimProcsGroups[i].ClaimNum;
            payAsTotal.PatNum = listClaimProcsGroups[i].PatNum;
            payAsTotal.ProvNum = listClaimProcsGroups[i].ProvNum;
            payAsTotal.ClinicNum = listClaimProcsGroups[i].ClinicNum;
            payAsTotal.CodeSent = listClaimProcsGroups[i].CodeSent;
            payAsTotal.InsSubNum = listClaimProcsGroups[i].InsSubNum;
            payAsTotal.PlanNum = listClaimProcsGroups[i].PlanNum;
            payAsTotal.PayPlanNum = listClaimProcsGroups[i].PayPlanNum;
            payAsTotal.ProcNum = listClaimProcsGroups[i].ProcNum;
            payAsTotal.ProcDate = listClaimProcsGroups[i].ProcDate;
            payAsTotal.DateEntry = listClaimProcsGroups[i].DateEntry;
            payAsTotal.SummedInsPayAmt = summedInsPay;
            listPayAsTotalsOutstanding.Add(payAsTotal);
        }

        if (listPayAsTotalsOutstanding.Count > listClaimProcsTotal.Count)
            //fail loudly so we don't transfer more splits than what is necessary. 
            throw new ApplicationException("Error encountered while transferring. Please call support.");
        return listPayAsTotalsOutstanding;
    }

    ///<summary>Gets one ClaimProc from db. Returns null if not found.</summary>
    public static ClaimProc GetOneClaimProc(long claimProcNum)
    {
        if (claimProcNum == 0) return null;
        var command = "SELECT * FROM claimproc "
                      + "WHERE ClaimProcNum = '" + SOut.Long(claimProcNum) + "'";
        return ClaimProcCrud.SelectOne(command);
    }

    #endregion

    #region Insert

    ///<summary>Inserts the ClaimProcs passed in. Does set ClaimProcNum.</summary>
    public static List<ClaimProc> InsertMany(List<ClaimProc> listClaimProcs)
    {
        if (listClaimProcs.Count == 0) return new List<ClaimProc>();
        //Not using Crud.InsertMany because we need to set the PrimaryKeys
        for (var i = 0; i < listClaimProcs.Count; i++) Insert(listClaimProcs[i]);
        return listClaimProcs;
    }

    /// <summary>
    ///     Finds all the claim pay by totals for this family and attempts to transfer them to their respective procedures.
    ///     If successful, lists will be inserted into the database. Does not currently support capitation claims.
    ///     Throws exceptions.
    /// </summary>
    public static ClaimTransferResult TransferClaimsAsTotalToProcedures(List<long> listFamilyPatNums, List<ClaimProc> listClaimProcsForPats = null)
    {
        var claimTransferResult = new ClaimTransferResult();
        //Gets all claims as total not yet transferred
        var listPayAsTotals = GetOutstandingClaimPayByTotal(listFamilyPatNums, listClaimProcsForPats);
        if (listPayAsTotals.Count == 0) return claimTransferResult;
        var listProcedures = Procedures.GetCompleteForPats(listFamilyPatNums);
        var listProcNums = listProcedures.Select(x => x.ProcNum).ToList();
        //Purposefully getting claim procs of all statuses (including received).
        var listClaimProcs = GetForProcs(listProcNums);
        for (var i = 0; i < listPayAsTotals.Count; i++) TransferPayAsTotal(listPayAsTotals[i], listClaimProcs, listProcedures, ref claimTransferResult);
        if (!claimTransferResult.IsValid())
        {
            claimTransferResult = null;
            throw new ApplicationException("Transfer returned a value other than 0. Please call support.");
        }

        claimTransferResult.Insert();
        return claimTransferResult;
    }

    private static void TransferPayAsTotal(PayAsTotal payAsTotal, List<ClaimProc> listClaimProcs, List<Procedure> listProcedures, ref ClaimTransferResult claimTransferResult)
    {
        var claimNum = payAsTotal.ClaimNum;
        var insPayAmtToAllocate = Math.Max(payAsTotal.SummedInsPayAmt, 0);
        if (CompareDouble.IsZero(insPayAmtToAllocate)) return;
        var listClaimProcsForClaim = listClaimProcs.FindAll(x => x.ClaimNum == claimNum);
        var listProcNums = listClaimProcsForClaim.Select(x => x.ProcNum).ToList();
        var listClaimProcsForOtherClaims = listClaimProcs.FindAll(x => x.ClaimNum != claimNum
                                                                       && x.Status != ClaimProcStatus.Supplemental //supplementals will be handled separately.
                                                                       && listProcNums.Contains(x.ProcNum));

        #region Create ClaimProcTxfr objects for PayAsTotal and ClaimProcs

        //Blindly make an offsetting supplemental claimproc on the claim for the PayAsTotal passed in.
        claimTransferResult.ListClaimProcTxfrs.Add(new ClaimProcTxfr(payAsTotal));
        //Make a bunch of individual supplemental claimprocs associated with procedures on the claim.
        //Transfer as much value to each procedure as possible.
        //Any leftover value (overpayment) should be blindly applied to the first procedure on the claim (or to unearned if no procedures are present).
        for (var i = 0; i < listClaimProcsForClaim.Count; i++)
        {
            if (CompareDouble.IsZero(insPayAmtToAllocate)) break;
            //There might be multiple PayAsTotal entries on a single claim so look for an existing ClaimProcTxfr objects for this claim and procedure.
            var claimProcTxfr = claimTransferResult.GetForClaimAndProc(claimNum, listClaimProcsForClaim[i].ProcNum);
            if (claimProcTxfr != null)
                //A previous PayAsTotal has created a ClaimProcTxfr already. Do not create a duplicate.
                continue;
            var procedure = listProcedures.Find(x => x.ProcNum == listClaimProcsForClaim[i].ProcNum);
            //Figure out how much value of this procedure has already been covered by other claims.
            double insPayAmtOtherIns = 0;
            for (var j = 0; j < listClaimProcsForOtherClaims.Count; j++)
            {
                if (listClaimProcsForOtherClaims[j].ProcNum != listClaimProcsForClaim[i].ProcNum) continue;
                insPayAmtOtherIns += listClaimProcsForOtherClaims[j].InsPayAmt;
            }

            //It is possible that there was a previous As Total payment for a different claim that has already been considered.
            //The "procedure fee" needs to consider these payments so that we do not create a supplemental claimproc that overpays the procedure.
            insPayAmtOtherIns += claimTransferResult.GetInsPayAmtForOtherClaims(claimNum, listClaimProcsForClaim[i].ProcNum);
            //Figure out how much value of this procedure has already been covered by supplementals.
            double insPayAmtSupplemental = 0;
            for (var j = 0; j < listClaimProcs.Count; j++)
            {
                if (listClaimProcs[j].ProcNum != listClaimProcsForClaim[i].ProcNum) continue;
                if (listClaimProcs[j].Status != ClaimProcStatus.Supplemental) continue;
                insPayAmtSupplemental += listClaimProcs[j].InsPayAmt;
            }

            //Create a new ClaimProcTxfr object for this procedure.
            //This will be used to keep track of how much value has been transferred from the PayAsTotal.
            claimProcTxfr = new ClaimProcTxfr(listClaimProcsForClaim[i], procedure, insPayAmtOtherIns, insPayAmtSupplemental);
            claimTransferResult.ListClaimProcTxfrs.Add(claimProcTxfr);
        }

        #endregion

        //Find all of the procedures related ClaimProcTxfr objects that are associated with this claim.
        //PayAsTotal ClaimProcTxfr objects will be flagged as 'offsets' and should be ignored.
        var listClaimProcTxfrsForProcs = claimTransferResult.ListClaimProcTxfrs.FindAll(x => !x.IsOffset && x.ClaimNum == claimNum);

        #region 1st layer, minimum transfer

        //First pass will be for transferring up to the minimum amount of value into each claimproc.
        //E.g. Some claimprocs will utilize the procedure fee if it is less than the insurance estimate or visa versa.
        for (var i = 0; i < listClaimProcTxfrsForProcs.Count; i++)
        {
            if (CompareDouble.IsZero(insPayAmtToAllocate)) break;
            var insPayAmt = listClaimProcTxfrsForProcs[i].GetInsPayAmtMin(insPayAmtToAllocate);
            listClaimProcTxfrsForProcs[i].ClaimProc.InsPayAmt += insPayAmt;
            insPayAmtToAllocate -= insPayAmt;
        }

        #endregion

        #region 2nd layer, maximum transfer

        if (CompareDouble.IsZero(insPayAmtToAllocate)) return;
        //There is still value to be transferred.
        //Second pass will be for transferring up to the maximum amount of value into each claimproc.
        //E.g. Some claimprocs will utilize the procedure fee if it is more than the insurance estimate or visa versa.
        for (var i = 0; i < listClaimProcTxfrsForProcs.Count; i++)
        {
            if (CompareDouble.IsZero(insPayAmtToAllocate)) break;
            var insPayAmt = listClaimProcTxfrsForProcs[i].GetInsPayAmtMax(insPayAmtToAllocate);
            listClaimProcTxfrsForProcs[i].ClaimProc.InsPayAmt += insPayAmt;
            insPayAmtToAllocate -= insPayAmt;
        }

        #endregion

        #region 3rd layer, leftover transfer

        if (CompareDouble.IsZero(insPayAmtToAllocate)) return;
        //There is STILL value to be transferred (overpayment?).
        //Transfer the rest of the money to the first procedure and let the income transfer manager figure out the rest later.
        if (listClaimProcTxfrsForProcs.Count > 0)
        {
            listClaimProcTxfrsForProcs[0].ClaimProc.InsPayAmt += insPayAmtToAllocate;
            return;
        }

        //Unique issue present from a conversions error that caused some claims to get created with only As Totals and no procedures.
        //This means that we will have not created any procedure supplementals and this list will be empty.
        //Instead of transferring to procedures, we will instead transfer this income to unearned.
        var payment = new Payment();
        payment.PatNum = payAsTotal.PatNum;
        payment.ClinicNum = claimNum;
        payment.PayDate = DateTime.Today;
        payment.PayAmt = insPayAmtToAllocate;
        payment.PayNote = Lans.g("FormIncomeTransferManager", "Transfer from claim with no claim procedures");
        Payments.Insert(payment);
        var paySplit = new PaySplit();
        paySplit.ClinicNum = Clinics.ClinicNum;
        paySplit.DateEntry = DateTime.Today;
        paySplit.DatePay = DateTime.Today;
        paySplit.PatNum = payAsTotal.PatNum;
        paySplit.PayNum = payment.PayNum;
        if (PrefC.GetBool(PrefName.AllowPrepayProvider))
            paySplit.ProvNum = payAsTotal.ProvNum;
        else
            paySplit.ProvNum = 0;
        paySplit.SplitAmt = payment.PayAmt;
        paySplit.UnearnedType = PrefC.GetLong(PrefName.PrepaymentUnearnedType);
        claimTransferResult.ListPaySplitsInserted.Add(paySplit);

        #endregion
    }

    #endregion

    #region Update

    /// <summary>Set all ClaimProcs attached to the claim to have the same clinic num.</summary>
    public static void UpdateClinicNumForClaim(long claimNum, long clinicNum)
    {
        if (claimNum <= 0) return;
        var command = $"UPDATE claimproc SET ClinicNum={SOut.Long(clinicNum)} WHERE ClaimNum={SOut.Long(claimNum)}";
        Db.NonQ(command);
    }

    ///<summary>Updates the ClaimProcs passed in.</summary>
    public static void UpdateMany(List<ClaimProc> listClaimProcsUpdate)
    {
        if (listClaimProcsUpdate.Count == 0) return;
        for (var i = 0; i < listClaimProcsUpdate.Count; i++) Update(listClaimProcsUpdate[i]);
    }

    /// <summary>
    ///     Sets the overrides for primary and/or secondary insurance ClaimProcs when the procedure is linked to an OrthoCase.
    ///     All overrides for OrthoCases are set to zero, save for the InsEstTotalOverride and InsPayEst.
    ///     These fields are set to the procedure's fee multiplied by the percent that insurance covers for the OrthoCase. For
    ///     the OrthoCase's final visit,
    ///     this estimate may be adjusted up or down so that the total of estimates for all procedures linked to the OrthoCase
    ///     is not dissimilar to the total insurance coverage for the OrthoCase once it is complete.
    /// </summary>
    public static void ComputeEstimatesByOrthoCase(Procedure procedure, OrthoProcLink orthoProcLink, OrthoCase orthoCase, OrthoSchedule orthoSchedule
        , bool saveToDb, List<ClaimProc> listClaimProcsAll, List<ClaimProc> listClaimProcsToUpdate, List<PatPlan> listPatPlans
        , List<OrthoProcLink> listOrthoProcLinksForCase)
    {
        //Iterate through each claimproc to update, only process claimProcs that have the correct/desired ProcNum.
        var listClaimProcsToUpdateFiltered = listClaimProcsToUpdate.FindAll(x => x.ProcNum == procedure.ProcNum);
        for (var i = 0; i < listClaimProcsToUpdateFiltered.Count; i++)
        {
            //Reset all insurance-related overrides for the current claimproc.
            ZeroOutClaimProcOverrides(listClaimProcsToUpdateFiltered[i]);
            //Reset all insurance-related overrides for the original copy of the claimproc (from listClaimProcsAll).
            var claimProcCopy = listClaimProcsAll.Find(x => x.ClaimProcNum == listClaimProcsToUpdateFiltered[i].ClaimProcNum);
            ZeroOutClaimProcOverrides(claimProcCopy);
            //If the claim is new/hasn't been created yet, set the procedure's date.
            if (listClaimProcsToUpdateFiltered[i].ClaimNum == 0)
            {
                listClaimProcsToUpdateFiltered[i].ProcDate = procedure.ProcDate;
                claimProcCopy.ProcDate = procedure.ProcDate;
            }

            //Get patient's insurance plan.
            var patPlan = PatPlans.GetFromList(listPatPlans, listClaimProcsToUpdateFiltered[i].InsSubNum);
            //Only primary and secondary insurance claimprocs get a nonzero override.
            if (patPlan == null || patPlan.Ordinal > 2) continue; //Skip claimprocs not linked to primary or secondary insurance plans.
            //Determine which insurance fee estimate to use based on the plan's Ordinal.
            var feeIns = patPlan.Ordinal == 1 ? orthoCase.FeeInsPrimary : orthoCase.FeeInsSecondary;
            //Compute the insurance estimate override based on procedure and ortho case fees.
            var insuranceCoverageRatio = feeIns / orthoCase.Fee;
            var insPayForProc = insuranceCoverageRatio * procedure.ProcFeeTotal;
            //TL;DR: If proc is a visit we may need to adjust the estimate so that estimates for all linked procs end up equalling FeeIns.
            //Long version: If we have completed all the visits, then check to make sure the original amount quoted to the patient is the same
            // as the number we're coming up with. The reason these might differ is because of rounding errors that (might) creep into our math
            // due to running this code - and thus rounding - every time they complete a visit or make an adjustment to the ortho case. So we
            // add up all the completed stuff (bonding, visits, etc.) and compare it's sum to the original price, and if we're off (usually by
            // no more than a cent or 2), we just adjust this final calculation to make sure they all match/add-up.
            if (orthoProcLink.ProcLinkType == OrthoProcType.Visit)
            {
                //Get number of planned visits for the OrthoCase
                var plannedVisitsCount = OrthoSchedules.CalculatePlannedVisitsCount(orthoSchedule.BandingAmount, orthoSchedule.DebondAmount, orthoSchedule.VisitAmount, orthoCase.Fee);
                //Number of completed visits for the OrthoCase
                var completedVisitsCount = listOrthoProcLinksForCase.FindAll(x => x.ProcLinkType == OrthoProcType.Visit).Count;
                //If the number of visits planned for the ortho case have been completed and the sum of claimproc estimate overrides does not
                // equal the adjusted FeeIns (FeeIns minus the estimate that will be applied to the claimproc for the debond proc), then adjust
                // the current claimproc so that all estimates will equal FeeIns.
                if (completedVisitsCount >= plannedVisitsCount)
                {
                    //Calculate the sum of insurance estimate overrides (InsEstTotalOverrides) for all linked procedures (excluding debond):
                    var sumClaimProcEstimates = listClaimProcsAll.FindAll(x => x.InsSubNum == patPlan.InsSubNum && listOrthoProcLinksForCase.Any(y => y.ProcNum == x.ProcNum && y.ProcLinkType != OrthoProcType.Debond)).Sum(x => x.InsEstTotalOverride);
                    //Get debond estimate for next calculation.
                    var insEstimateForDebond = insuranceCoverageRatio * orthoSchedule.DebondAmount;
                    //Do the adjustment
                    insPayForProc = feeIns - (sumClaimProcEstimates + insEstimateForDebond);
                }
            }

            //Round amount since we're dealing with currency here
            insPayForProc = Math.Round(insPayForProc, 2, MidpointRounding.AwayFromZero);
            //Update members/values in memory
            listClaimProcsToUpdateFiltered[i].InsPayEst = insPayForProc;
            listClaimProcsToUpdateFiltered[i].InsEstTotalOverride = insPayForProc;
            claimProcCopy.InsPayEst = insPayForProc;
            claimProcCopy.InsEstTotalOverride = insPayForProc;
        }

        if (saveToDb)
            //Update members/values in db
            UpdateMany(listClaimProcsToUpdate);
    }

    private static void ZeroOutClaimProcOverrides(ClaimProc claimProc)
    {
        claimProc.AllowedOverride = 0;
        claimProc.CopayOverride = 0;
        claimProc.DedEstOverride = 0;
        claimProc.PercentOverride = 0;
        claimProc.PaidOtherInsOverride = 0;
        claimProc.InsEstTotalOverride = 0;
        claimProc.WriteOffEstOverride = 0;
        claimProc.InsPayEst = 0;
    }

    #endregion

    #region Misc Methods

    ///<summary>Creates and returns a claimproc object used for insurance adjustments.  Does not insert into the db.</summary>
    public static ClaimProc CreateInsPlanAdjustment(long patNum, long planNum, long insSubNum)
    {
        var claimProc = new ClaimProc();
        claimProc.PatNum = patNum;
        claimProc.ProcDate = DateTime.Today;
        claimProc.Status = ClaimProcStatus.Adjustment;
        claimProc.PlanNum = planNum;
        claimProc.InsSubNum = insSubNum;
        return claimProc;
    }

    ///<summary>Creates a new claimproc based off of the existing claimproc passed in.</summary>
    public static ClaimProc CreateSuppClaimProcForTransfer(ClaimProc claimProc)
    {
        var claimProcNew = new ClaimProc(); //or set to copy
        claimProcNew.Status = ClaimProcStatus.Supplemental;
        claimProcNew.PatNum = claimProc.PatNum;
        claimProcNew.ProcNum = claimProc.ProcNum; //will be 0 for as total offset, and a procNum when creating supplemental for procedures.
        claimProcNew.ClaimNum = claimProc.ClaimNum;
        claimProcNew.ClinicNum = claimProc.ClinicNum;
        claimProcNew.DateEntry = DateTime.Today;
        claimProcNew.ProcDate = claimProc.ProcDate;
        //Supplemental payments should not be attached to payment plans.
        claimProcNew.PayPlanNum = 0;
        //This causes another line item to be created when set. We do not want these transfers to show on statements so we want it to be minval.
        //newClaimProc.DateCP=DateTime.MinValue;
        claimProcNew.PlanNum = claimProc.PlanNum;
        claimProcNew.ProvNum = claimProc.ProvNum;
        claimProcNew.InsSubNum = claimProc.InsSubNum;
        claimProcNew.CodeSent = claimProc.CodeSent;
        claimProcNew.IsTransfer = true;
        return claimProcNew;
    }

    /// <summary>
    ///     Creates a new claimproc based off of the existing claimproc passed in. Made as offset supplemental for the
    ///     original.
    /// </summary>
    public static ClaimProc CreateSuppClaimProcForTransfer(PayAsTotal payAsTotal)
    {
        var claimProc = new ClaimProc();
        claimProc.Status = ClaimProcStatus.Supplemental;
        claimProc.PatNum = payAsTotal.PatNum;
        claimProc.ProcNum = payAsTotal.ProcNum; //will be 0 for as total offset, and a procNum when creating supplemental for procedures.
        claimProc.ClaimNum = payAsTotal.ClaimNum;
        claimProc.ClinicNum = payAsTotal.ClinicNum;
        claimProc.DateEntry = DateTime.Today;
        claimProc.ProcDate = payAsTotal.ProcDate;
        //Supplemental payments should not be attached to payment plans.
        claimProc.PayPlanNum = 0;
        //This causes another line item to be created when set. We do not want these transfers to show on statements so we want it to be minval.
        //newClaimProc.DateCP=DateTime.MinValue;
        claimProc.PlanNum = payAsTotal.PlanNum;
        claimProc.ProvNum = payAsTotal.ProvNum;
        claimProc.InsSubNum = payAsTotal.InsSubNum;
        claimProc.CodeSent = payAsTotal.CodeSent;
        claimProc.IsTransfer = true;
        //This supplemental claimproc is designed to offset the entire PayAsTotal passed in.
        //Only set InsPayAmt since transferring a WriteOff value doesn't make sense.
        claimProc.InsPayAmt = payAsTotal.SummedInsPayAmt * -1;
        return claimProc;
    }

    /// <summary>
    ///     This method is specifically aimed at fixing claims that were created with no claim procedures, only as totals.
    ///     The fix is to create a dummy procedure and claimproc for each as total on the claim for the matching
    ///     pat/prov/clinic group.
    ///     Returns true if a fix was needed and applied, otherwise false.
    /// </summary>
    public static bool FixClaimsNoProcedures(List<long> listPatNumsFamily)
    {
        var listClaimNumsNoProcedures = new List<long>();
        //Get all ClaimNums for claims that have no claimprocs associated to procedures, regardless of claim status.
        var command = $@"SELECT claimproc.ClaimNum FROM claimproc 
												WHERE claimproc.PatNum IN ({string.Join(",", listPatNumsFamily.Select(x => SOut.Long(x)))}) 
												AND claimproc.ClaimNum > 0
												GROUP BY claimproc.ClaimNum 
												HAVING SUM(claimproc.ProcNum)=0 ";
        listClaimNumsNoProcedures = Db.GetListLong(command);
        if (listClaimNumsNoProcedures.Count == 0) return false;
        //Get all of the claimprocs for the claims that have no procedures.
        var listClaimProcsByClaim = RefreshForClaims(listClaimNumsNoProcedures);
        var listClaimProcs = new List<ClaimProc>();
        //Get the claims from the database that do not have a procedure attached.
        var listClaims = Claims.GetClaimsFromClaimNums(listClaimNumsNoProcedures);
        //Loop through each claim and add a dummy procedure and claim proc for every original as total.
        for (var i = 0; i < listClaims.Count; i++)
        {
            var listClaimProcsAsTotals = listClaimProcsByClaim.FindAll(x => x.ClaimNum == listClaims[i].ClaimNum && !x.IsTransfer); //originals
            listClaimProcs.AddRange(CreateDummyDataForClaimMissingClaimProcs(listClaims[i], listClaimProcsAsTotals));
        }

        if (listClaimProcs.Count > 0)
        {
            InsertMany(listClaimProcs);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Helper method for fixing claims without any claim procedures with proc nums (claims having only as total payments).
    ///     The fix is to create a $0 dummy procedure and a dummy claimproc associated to said procedure.  This method will
    ///     create the dummy procedure
    ///     if it doesn't already exist because the ProcNum is needed to associate with the dummy claimproc that gets created
    ///     as well.
    /// </summary>
    private static List<ClaimProc> CreateDummyDataForClaimMissingClaimProcs(Claim claim, List<ClaimProc> listClaimProcsTotals)
    {
        var listClaimProcs = new List<ClaimProc>();

        #region Create Dummy Procedure Code

        ProcedureCode procedureCodeFix;
        var code = "ZZZFIX";
        procedureCodeFix = ProcedureCodes.GetFirstOrDefault(x => x.ProcCode == code);
        if (procedureCodeFix == null)
        {
            procedureCodeFix = new ProcedureCode();
            procedureCodeFix.ProcCode = code;
            procedureCodeFix.AbbrDesc = code;
            procedureCodeFix.Descript = "ClaimPayAsTotalFix";
            procedureCodeFix.ProcCat = Defs.GetByExactName(DefCat.ProcCodeCats, "Obsolete");
            if (procedureCodeFix.ProcCat == 0) //There is no Obsolete category so just get the first non-hidden one in the cache.
                procedureCodeFix.ProcCat = Defs.GetDefsForCategory(DefCat.ProcCodeCats, true).First().DefNum;
            ProcedureCodes.Insert(procedureCodeFix);
            SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, Lans.g("Procedures", "Income Transfer Manager automatically added Procedure Code:")
                                                             + " " + procedureCodeFix.ProcCode);
            Signalods.SetInvalid(InvalidType.ProcCodes);
            Cache.Refresh(InvalidType.ProcCodes);
        }

        #endregion

        //Group all of the as totals into Pat/Prov/Clinic specific buckets (most accurate transfer ATM).
        //Usually only one claimproc for this claim, but it is possible to have more than one AsTotal on a claim for different provs.
        //The only information that matters from these claimprocs is the Pat/Prov/Clinic and DateCP.  So just use the first entry.
        var listClaimProcsUniquePatProvClinic = listClaimProcsTotals.DistinctBy(x => new {x.PatNum, x.ProvNum, x.ClinicNum}).ToList();
        var logText = Lans.g("Procedures", "Income Transfer Manager automatically added") +
                      $" {procedureCodeFix.ProcCode}, {Lans.g("Procedures", "Fee")}: {0.ToString("F")}, {procedureCodeFix.Descript}";
        //Make a unique dummy procedure and claimproc for each Pat/Prov/Clinic combination to accurately transfer the money. 
        for (var i = 0; i < listClaimProcsUniquePatProvClinic.Count(); i++)
        {
            #region Create Dummy Procedure For Group

            var procedureDummy = new Procedure();
            procedureDummy.PatNum = listClaimProcsUniquePatProvClinic[i].PatNum;
            procedureDummy.ProcFee = 0;
            procedureDummy.ProcStatus = ProcStat.C;
            procedureDummy.ProvNum = listClaimProcsUniquePatProvClinic[i].ProvNum;
            procedureDummy.ClinicNum = listClaimProcsUniquePatProvClinic[i].ClinicNum;
            procedureDummy.CodeNum = procedureCodeFix.CodeNum;
            procedureDummy.ProcDate = listClaimProcsUniquePatProvClinic[i].DateCP;
            procedureDummy.DateComplete = listClaimProcsUniquePatProvClinic[i].DateCP;
            procedureDummy.DateEntryC = listClaimProcsUniquePatProvClinic[i].DateCP;
            Procedures.Insert(procedureDummy); //needs to be inserted here for the claimproc to know what it is attached to
            SecurityLogs.MakeLogEntry(EnumPermType.ProcComplCreate, listClaimProcsUniquePatProvClinic[i].PatNum, logText);

            #endregion

            #region Create Dummy ClaimProc For Dummy Procedure

            var claimProcDummy = new ClaimProc();
            claimProcDummy.ClaimNum = claim.ClaimNum;
            claimProcDummy.ClinicNum = procedureDummy.ClinicNum;
            claimProcDummy.DateCP = listClaimProcsUniquePatProvClinic[i].DateCP;
            claimProcDummy.CodeSent = procedureCodeFix.ProcCode;
            claimProcDummy.InsPayAmt = 0;
            claimProcDummy.InsPayEst = 0;
            claimProcDummy.InsSubNum = claim.InsSubNum;
            claimProcDummy.PatNum = procedureDummy.PatNum;
            claimProcDummy.PlanNum = claim.PlanNum;
            claimProcDummy.ProcDate = procedureDummy.ProcDate;
            claimProcDummy.ProcNum = procedureDummy.ProcNum;
            claimProcDummy.ProvNum = procedureDummy.ProvNum;
            claimProcDummy.Status = ClaimProcStatus.Received;
            claimProcDummy.WriteOffEst = 0;
            claimProcDummy.DateEntry = listClaimProcsUniquePatProvClinic[i].DateCP;
            listClaimProcs.Add(claimProcDummy);

            #endregion
        }

        return listClaimProcs;
    }

    /// <summary>
    ///     Goes through logic to apply an AsTotal payment to specific procedures depending on their current amount of
    ///     allocated money.
    /// </summary>
    public static void ApplyAsTotalPayment(ref ClaimProc[] claimProcArray, double totalPayAmt, List<Procedure> listProcedures)
    {
        if (!PrefC.GetBool(PrefName.ClaimPayByTotalSplitsAuto))
            //preference is set to not create claim payments by total automatically.
            return;
        var listClaimProcsSorted = claimProcArray.ToList();
        listClaimProcsSorted = listClaimProcsSorted.FindAll(x => !x.Status.In(ClaimProcStatus.Supplemental, ClaimProcStatus.Received, ClaimProcStatus.CapComplete))
            .OrderByDescending(x => x.DateCP).ToList();
        var existingAsTotalPayments = listClaimProcsSorted.FindAll(x => x.ProcNum == 0).Sum(x => x.InsPayAmt);
        listClaimProcsSorted.RemoveAll(x => x.ProcNum == 0);
        if (listClaimProcsSorted.IsNullOrEmpty() || totalPayAmt <= existingAsTotalPayments) return;
        var remainingAlotment = totalPayAmt - existingAsTotalPayments; //The user entered value (pool) of cash we are going to fill the claim procs with
        //Shorten the list of Procedures down to only the ones with associated Claim Procs
        var listProceduresAssociatedtoClaims = listProcedures.FindAll(x => listClaimProcsSorted.Select(y => y.ProcNum).ToList().Contains(x.ProcNum));
        var listSupplementalPayments = new List<double>();
        for (var i = 0; i < listClaimProcsSorted.Count; i++)
        {
            //Build the list of supplemental Write offs / ins payments and put them in a list.
            var listClaimProcsSupplemental = claimProcArray.ToList()
                .FindAll(x => x.Status == ClaimProcStatus.Supplemental && x.LineNumber == listClaimProcsSorted[i].LineNumber); //Get attached supplementals
            double previousPayments = 0;
            for (var j = 0; j < listClaimProcsSupplemental.Count; j++) //Sum the attached supplemental payments and writeoffs. 
                previousPayments += listClaimProcsSupplemental[j].InsPayAmt;
            listSupplementalPayments.Add(previousPayments); //Since we are adding to the end of the list, listSupplementalPayments order == sortedClaimProc order
        }

        //Set all of the selected claimProcs to recieved
        for (var i = 0; i < claimProcArray.Length; i++)
            if (claimProcArray[i].Status == ClaimProcStatus.Supplemental)
            {
            }
            else if (claimProcArray[i].Status == ClaimProcStatus.NotReceived)
            {
                claimProcArray[i].Status = ClaimProcStatus.Received;
            }

        //Allocate payments to all Insurance Billed Estimates.
        for (var i = 0; i < listClaimProcsSorted.Count; i++)
        {
            var claimProc = listClaimProcsSorted[i];
            var previousPayments = listSupplementalPayments[i]; //Pull the related supplimental payments total.
            //If the insurance paid + supplement is more than the estimate, the estimate is 0, or the Estimate is less than the supplemental payment, skip.
            if (claimProc.InsPayAmt + previousPayments >= claimProc.InsPayEst || claimProc.InsPayEst == 0 || claimProc.InsPayEst <= previousPayments) continue;
            //Cap the InsPayEst to the amount billed to insurance. Insurance will sometimes include multiple procedures in an estimate i.e, a Pre-Auth was received. 
            var claimProcEst = Math.Min(claimProc.FeeBilled, claimProc.InsPayEst);
            //if the remaining pay total is less than the insurance estimate (minus the supplemental payments), just dump whats left of pay total into the insurance paid.
            if (remainingAlotment - (claimProcEst - previousPayments) <= 0)
            {
                claimProc.InsPayAmt += remainingAlotment;
                return;
            }

            var amountAllocated = claimProcEst - claimProc.InsPayAmt - previousPayments;
            remainingAlotment -= amountAllocated; //pull the amount to be allocated towards insurance paid, from the pay total pool.
            claimProc.InsPayAmt += amountAllocated; //put the amount allocated into the insurace paid pool.
        }

        //Allocate remaining alotment to all remaining procedure fees.
        for (var i = 0; i < listClaimProcsSorted.Count; i++)
        {
            var claimProc = listClaimProcsSorted[i];
            var previousPayments = listSupplementalPayments[i];
            var procFee = listProceduresAssociatedtoClaims.Find(x => x.ProcNum == claimProc.ProcNum).ProcFee; //Get the associated procedure fee
            if (procFee == 0 || claimProc.InsPayAmt + previousPayments >= procFee) continue;
            //if the remaining pay total is less than the procedure fee (minus the supplemental payments), just dump whats left of pay total into the insurance paid.
            if (remainingAlotment - (procFee - claimProc.InsPayAmt - previousPayments) <= 0)
            {
                claimProc.InsPayAmt += remainingAlotment;
                return;
            }

            remainingAlotment -= procFee - claimProc.InsPayAmt - previousPayments;
            claimProc.InsPayAmt += procFee - claimProc.InsPayAmt - previousPayments;
        }

        //If anything is leftover dump the funds into the first claim proc
        if (remainingAlotment >= 0) listClaimProcsSorted[0].InsPayAmt += remainingAlotment;
    }

    ///<summary>Creates InsWriteOffEdit, InsPayEdit, and InsPayCreate logs for a claimproc as needed.</summary>
    public static void CreateAuditTrailEntryForClaimProcPayment(ClaimProc claimProcNew, ClaimProc claimProcOld, bool isInsPayCreate, bool isPaymentFromERA = false)
    {
        if (claimProcNew == null || claimProcOld == null) return;
        //Will be "" if claimProcNew.ProcNum is 0 or resulting CodeNum is 0.
        var strProcCode = ProcedureCodes.GetStringProcCode(Procedures.GetOneProc(claimProcNew.ProcNum, false).CodeNum);
        var strProcLog = "";
        if (!strProcCode.IsNullOrEmpty()) strProcLog = $"amount for procedure {strProcCode}, ";
        var strClaimProcOldWriteOff = claimProcOld.WriteOff.ToString("C");
        var strClaimProcNewWriteOff = claimProcNew.WriteOff.ToString("C");
        var strClaimProcOldInsPayAmt = claimProcOld.InsPayAmt.ToString("C");
        var strClaimProcNewInsPayAmt = claimProcNew.InsPayAmt.ToString("C");
        var strERA = isPaymentFromERA ? " (Payment from ERA)" : "";
        var patNum = claimProcNew.PatNum;
        var permissionInsPay = EnumPermType.InsPayEdit;
        if (isInsPayCreate) permissionInsPay = EnumPermType.InsPayCreate;
        //The write off has changed
        if (claimProcNew.WriteOff != claimProcOld.WriteOff)
            SecurityLogs.MakeLogEntry(EnumPermType.InsWriteOffEdit, patNum, $"Write-off {strProcLog}"
                                                                            + $"changed from {strClaimProcOldWriteOff} to {strClaimProcNewWriteOff}" + strERA);
        //Insurance payment is being created but hasn't changed. The only scenario I can think of where this would be true is $0 payment.
        if (claimProcNew.InsPayAmt == claimProcOld.InsPayAmt && isInsPayCreate)
        {
            if (!strProcLog.IsNullOrEmpty())
                //This prepends to string from above, so strProcLog will be "Insurance payment amount for procedure Dxxxx, "
                strProcLog = $"Insurance payment {strProcLog}" + strERA;
            SecurityLogs.MakeLogEntry(permissionInsPay, patNum, $"{strProcLog}Insurance payment amount {strClaimProcNewInsPayAmt}" + strERA);
        }
        //Insurance payment is new or we are editing an existing one and payment amount has changed.
        else if (claimProcNew.InsPayAmt != claimProcOld.InsPayAmt)
        {
            SecurityLogs.MakeLogEntry(permissionInsPay, patNum, $"Insurance payment {strProcLog}"
                                                                + $"changed from {strClaimProcOldInsPayAmt} to {strClaimProcNewInsPayAmt}" + strERA);
        }
    }

    #endregion
}

/// <summary>
///     During the ClaimProc.ComputeBaseEst() and related sections, this holds historical payment information for one
///     procedure or an adjustment to insurance benefits from patplan.
/// </summary>
public class ClaimProcHist
{
    ///<summary>Insurance paid or est, depending on the status.</summary>
    public double Amount;

    ///<summary>So that we can exclude history from the claim that we are in.</summary>
    public long ClaimNum;

    ///<summary>Deductible paid or est.</summary>
    public double Deduct;

    
    public long InsSubNum;

    ///<summary>Copied from claimproc.NoBillIns</summary>
    public bool NoBillIns;

    ///<summary>Because a list can store info for an entire family.</summary>
    public long PatNum;

    ///<summary>Because a list can store info about multiple plans.</summary>
    public long PlanNum;

    public DateTime ProcDate;

    ///<summary>This is needed to filter out primary histList entries on secondary claims.</summary>
    public long ProcNum;

    ///<summary>Only 4 statuses get used anyway.  This helps us filter the pending items sometimes.</summary>
    public ClaimProcStatus Status;

    public string StrProcCode;

    ///<summary>This value is formatted exactly the same as it is within the database for procedurelog.Surf</summary>
    public string Surf;

    ///<summary>This value is formatted exactly the same as it is within the database for procedurelog.ToothNum</summary>
    public string ToothNum;

    ///<summary>This value is formatted exactly the same as it is within the database for procedurelog.ToothRange</summary>
    public string ToothRange;

    public override string ToString()
    {
        return StrProcCode + " " + Status + " " + Amount + " ded:" + Deduct;
    }
}

/// <summary>
///     Jordan I would like to eliminate this class. It represents PayAsTotal claimprocs. This is used specifically
///     with ClaimProc pay as total transfers where we create a singular PayAsTotal to represent a group of summed pay as
///     totals, which is where the SummedInsPayAmt and SummedInsWriteOff comes from.
/// </summary>
[Serializable]
public class PayAsTotal
{
    //Jordan All of the fields are already present in ClaimProc, so this class is not necessary.
    //The two "Summed" fields also have corresponding fields in ClaimProc.
    //Eventually, all PayAsTotal objects already make their way into the db as a claimproc.
    public long ClaimNum;
    public long ClinicNum;
    public string CodeSent;
    public DateTime DateEntry;
    public long InsSubNum;
    public long PatNum;
    public long PayPlanNum;
    public long PlanNum;
    public DateTime ProcDate;
    public long ProcNum;
    public long ProvNum;
    public double SummedInsPayAmt;

    public PayAsTotal Copy()
    {
        return (PayAsTotal) MemberwiseClone();
    }
}

///<summary>A helper class that stores inserted claimprocs and their corresponding paysplits.</summary>
public class ClaimTransferResult
{
    public List<ClaimProc> ListClaimProcsInserted = new();
    public List<ClaimProcTxfr> ListClaimProcTxfrs = new();
    public List<PaySplit> ListPaySplitsInserted = new();

    public ClaimProcTxfr GetForClaimAndProc(long claimNum, long procNum)
    {
        return ListClaimProcTxfrs.Find(x => !x.IsOffset && x.ClaimNum == claimNum && x.ProcNum == procNum);
    }

    ///<summary>Returns the total InsPayAmt from other claims that are associated with the procedure passed in.</summary>
    public double GetInsPayAmtForOtherClaims(long claimNum, long procNum)
    {
        double insPayAmt = 0;
        for (var i = 0; i < ListClaimProcTxfrs.Count; i++)
        {
            if (ListClaimProcTxfrs[i].ClaimNum == claimNum) continue;
            if (ListClaimProcTxfrs[i].ProcNum != procNum) continue;
            insPayAmt += ListClaimProcTxfrs[i].ClaimProc.InsPayAmt;
        }

        return insPayAmt;
    }

    public bool IsValid()
    {
        var totalInsPayAmt = ListClaimProcTxfrs.Sum(x => x.ClaimProc.InsPayAmt);
        var totalWriteOff = ListClaimProcTxfrs.Sum(x => x.ClaimProc.WriteOff);
        var totalSplitAmt = ListPaySplitsInserted.Sum(x => x.SplitAmt);
        var totalTransfer = totalInsPayAmt + totalWriteOff + totalSplitAmt;
        if (!CompareDouble.IsZero(totalTransfer)) return false;
        return true;
    }

    ///<summary>Inserts the ClaimProcTxfrs and PaySplits into the database.</summary>
    public void Insert()
    {
        ListClaimProcTxfrs.RemoveAll(x => CompareDouble.IsZero(x.ClaimProc.InsPayAmt) && CompareDouble.IsZero(x.ClaimProc.WriteOff));
        var listClaimProcs = ListClaimProcTxfrs.Select(x => x.ClaimProc).ToList();
        ListClaimProcsInserted = ClaimProcs.InsertMany(listClaimProcs);
        PaySplits.InsertMany(ListPaySplitsInserted);
    }
}

public class ClaimProcTxfr
{
	/// <summary>
	///     The maximum value that can be transferred. This is the larger value between the procedure fee and the
	///     insurance estimate.
	/// </summary>
	public double AmountTxfrMax;

	/// <summary>
	///     The minimum value that can be transferred. This is the smaller value between the procedure fee and the
	///     insurance estimate.
	/// </summary>
	public double AmountTxfrMin;

    
    public ClaimProc ClaimProc;

    
    public double InsPayAmtSupplemental;

    
    public bool IsOffset;

    ///<summary>For serialization. Do not use.</summary>
    public ClaimProcTxfr()
    {
    }

    public ClaimProcTxfr(PayAsTotal payAsTotal)
    {
        IsOffset = true;
        ClaimProc = ClaimProcs.CreateSuppClaimProcForTransfer(payAsTotal);
    }

    public ClaimProcTxfr(ClaimProc claimProc, Procedure procedure, double insPayAmtOtherIns, double insPayAmtSupplemental)
    {
        IsOffset = false;
        InsPayAmtSupplemental = insPayAmtSupplemental;
        ClaimProc = ClaimProcs.CreateSuppClaimProcForTransfer(claimProc);
        //InsPayEst and WriteOff logic
        var insPayEst = Math.Max(claimProc.InsPayEst, 0);
        //Subtract how much value has already been paid and written off for this claimproc.
        var amountInsCovered = Math.Max(claimProc.InsPayAmt + claimProc.WriteOff, 0);
        var amountCanTransfer = Math.Max(insPayEst - amountInsCovered, 0);
        //ProcFee logic
        double procFee = 0;
        if (procedure != null) procFee = procedure.ProcFeeTotal - insPayAmtOtherIns - amountInsCovered;
        AmountTxfrMin = Math.Min(amountCanTransfer, procFee);
        AmountTxfrMax = Math.Max(amountCanTransfer, procFee);
    }

    
    public long ClaimNum => ClaimProc.ClaimNum;

    
    public long ProcNum => ClaimProc.ProcNum;

    public double GetInsPayAmtMin(double insPayAmtToAllocate)
    {
        return GetInsPayAmt(AmountTxfrMin, insPayAmtToAllocate);
    }

    public double GetInsPayAmtMax(double insPayAmtToAllocate)
    {
        return GetInsPayAmt(AmountTxfrMax, insPayAmtToAllocate);
    }

    private double GetInsPayAmt(double insPayEst, double insPayAmtToAllocate)
    {
        double insPayAmt = 0;
        var insPayEstRemaining = insPayEst - ClaimProc.InsPayAmt;
        if (insPayEst != 0 && insPayAmtToAllocate != 0)
        {
            //estimated payment exists and there is money to allocate
            var amt = Math.Min(insPayAmtToAllocate, insPayEst) - InsPayAmtSupplemental;
            var amtRemain = Math.Min(insPayEstRemaining, amt);
            insPayAmt = Math.Max(amtRemain, 0);
        }

        return insPayAmt;
    }
}