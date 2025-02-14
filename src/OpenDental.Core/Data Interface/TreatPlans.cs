using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using ODCrypt;

namespace OpenDentBusiness;

public class TreatPlans
{
    public static List<TreatPlan> Refresh(long patNum)
    {
        return TreatPlanCrud.SelectMany("SELECT * FROM treatplan WHERE PatNum = " + patNum + " AND TPStatus = 0 ORDER BY DateTP");
    }

    public static List<TreatPlan> GetAllForPat(long patNum)
    {
        return TreatPlanCrud.SelectMany("SELECT * FROM treatplan WHERE PatNum = " + patNum);
    }

    public static List<TreatPlan> GetAllCurrentForPat(long patNum)
    {
        return GetAllForPat(patNum)
            .Where(x => x.TPStatus != TreatPlanStatus.Saved)
            .OrderBy(x => x.TPStatus != TreatPlanStatus.Active)
            .ThenBy(x => x.DateTP)
            .ToList();
    }

    public static TreatPlan GetActiveForPat(long patNum)
    {
        return TreatPlanCrud.SelectOne("SELECT * FROM treatplan WHERE PatNum = " + patNum + " AND TPStatus = " + (int) TreatPlanStatus.Active);
    }

    public static void Update(TreatPlan treatPlan)
    {
        TreatPlanCrud.Update(treatPlan);
    }

    public static void Update(TreatPlan treatPlan, TreatPlan treatPlanOld)
    {
        TreatPlanCrud.Update(treatPlan, treatPlanOld);
    }

    public static long Insert(TreatPlan treatPlan)
    {
        treatPlan.SecUserNumEntry = Security.CurUser.UserNum;

        return TreatPlanCrud.Insert(treatPlan);
    }

    public static void Delete(TreatPlan treatPlan)
    {
        var dataTable = DataCore.GetTable("SELECT * FROM proctp WHERE TreatPlanNum = " + treatPlan.TreatPlanNum);

        if (dataTable.Rows.Count > 0)
        {
            throw new ApplicationException("Cannot delete treatment plan because it has ProcTP's attached");
        }

        Db.NonQ("DELETE from treatplan WHERE TreatPlanNum = " + treatPlan.TreatPlanNum);
    }

    public static long CreateArchivedTreatPlan(TreatPlan treatPlan, Patient patient, List<ProcTP> selectedProcTps, List<TreatPlanAttach> listTreatPlanAttaches)
    {
        var retVal = Insert(treatPlan);
        
        Procedure procedure;
        
        var itemNo = 0;
        
        foreach (var procTp in selectedProcTps)
        {
            if (procTp is null)
            {
                continue;
            }
            
            procedure = Procedures.GetOneProc(procTp.ProcNumOrig, true);
            
            var treatPlanAttach = listTreatPlanAttaches.FirstOrDefault(x => x.ProcNum == procedure.ProcNum);
            var procedureCode = ProcedureCodes.GetProcCode(procedure.CodeNum);
            
            var proc = new ProcTP
            {
                TreatPlanNum = treatPlan.TreatPlanNum,
                PatNum = patient.PatNum,
                ProcNumOrig = procedure.ProcNum,
                ItemOrder = itemNo,
                Priority = treatPlanAttach?.Priority ?? 0,
                ToothNumTP = Tooth.Display(procedure.ToothNum),
                Surf = procedureCode.TreatArea switch
                {
                    TreatmentArea.Surf => Tooth.SurfTidyFromDbToDisplay(procedure.Surf, procedure.ToothNum),
                    TreatmentArea.Sextant => Tooth.GetSextant(procedure.Surf, (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers)),
                    _ => procedure.Surf
                },
                ProcCode = ProcedureCodes.GetStringProcCode(procedure.CodeNum),
                Descript = procTp.Descript,
                FeeAmt = procTp.FeeAmt,
                PriInsAmt = procTp.PriInsAmt,
                SecInsAmt = procTp.SecInsAmt,
                Discount = procTp.Discount,
                PatAmt = procTp.PatAmt,
                Prognosis = procTp.Prognosis,
                Dx = procTp.Dx,
                ProcAbbr = procTp.ProcAbbr,
                FeeAllowed = procTp.FeeAllowed,
                TaxAmt = procTp.TaxAmt,
                ProvNum = procTp.ProvNum,
                DateTP = procTp.DateTP,
                ClinicNum = procTp.ClinicNum,
                CatPercUCR = procTp.CatPercUCR
            };
            
            ProcTPs.InsertOrUpdate(proc, true);
            
            itemNo++;
        }

        return retVal;
    }

    public static string GetKeyDataForSignatureSaving(TreatPlan treatPlan, List<ProcTP> listProcTPs)
    {
        var keyData = GetKeyDataForSignatureHash(treatPlan, listProcTPs);
        
        return GetHashStringForSignature(keyData);
    }

    public static string GetKeyDataForSignatureHash(TreatPlan treatPlan, List<ProcTP> procTps)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(treatPlan.Note);
        stringBuilder.Append(treatPlan.DateTP.ToString("yyyyMMdd"));
        stringBuilder.Append(treatPlan.SignatureText);
        stringBuilder.Append(treatPlan.SignaturePracticeText);
        
        foreach (var procTp in procTps)
        {
            stringBuilder.Append(procTp.Descript);
            stringBuilder.Append(procTp.PatAmt.ToString("F2"));
        }

        return stringBuilder.ToString();
    }

    public static string GetHashStringForSignature(string str)
    {
        return Encoding.ASCII.GetString(MD5.Hash(Encoding.UTF8.GetBytes(str)));
    }

    public static void AuditPlans(long patNum, TreatPlanType treatPlanType)
    {
        #region Pseudo Code

        //Get all treatplans for the patient
        //Find active TP if it already exists
        //If more than one active TP, update all but the first to Inactive
        //Find unassigned TP if it already exists
        //Get all treatplanattaches for the treatplans
        //Get all TP and TPi procs for the patient
        //Get list of procs for the active plan, i.e. TPA exists linking to active plan or ProcStatus is TP or attached to sched/planned appt
        //Get list of inactive procs, i.e. ProcStatus is TPi and AptNum is 0 and PlannedAptNum is 0 and no TPA exists linking it to the active plan
        //Create an active plan if one doesn't exist and there are procs that need to be attached to it
        //Create an unassigned plan if one doesn't exist and there are unassigned TPi procs
        //For each proc that should be attached to the active plan
        //  update status from TPi to TP
        //  if TPA exists linking to active plan, update priority to TPA priority
        //  delete any TPA that links the proc to the unassigned plan
        //  if TPA linking the proc to the active plan does not exist, insert one with TPA priority set to the proc priority
        //For each proc that is not attached to the active plan (ProcStatus is TPi)
        //  set proc priority to 0
        //  if TPA does not exist, insert one linking the proc to the unassigned plan with TPA priority 0
        //  if multiple TPAs exist with one linking the proc to the unassigned plan, delete the TPA linking to the unassigned plan
        //Foreach TPA
        //  if TPA links proc to the unassigned plan and TPA exists linking the proc to any other plan, delete the link to the unassigned plan
        //If an unassigned plan exists and there are no TPAs pointing to it, delete the unassigned plan

        #endregion Pseudo Code

        #region Variables

        var listTreatPlans = GetAllForPat(patNum); //All treatplans for the pat. [([Includes Saved Plans]}};
        var treatPlanActive = listTreatPlans.FirstOrDefault(x => x.TPStatus == TreatPlanStatus.Active); //can be null
        var treatPlanUnassigned = listTreatPlans.FirstOrDefault(x => x.TPStatus == TreatPlanStatus.Inactive && x.Heading == Lans.g("TreatPlans", "Unassigned")); //can be null
        var listTreatPlanAttaches = TreatPlanAttaches.GetAllForTPs(listTreatPlans.Select(x => x.TreatPlanNum).ToList());
        var listProceduresTpTpi = Procedures.GetProcsByStatusForPat(patNum, ProcStat.TP, ProcStat.TPi); //All TP and TPi procs for the pat.
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            //Previously we have had Canadian users add labs to TPi parent procs, this is rare and odd.
            //When this happens we must ensure that there are matching TPAs for the labs.
            var listTPAProcNums = listTreatPlanAttaches.Select(x => x.ProcNum).ToList();
            for (var i = 0; i < listProceduresTpTpi.Count(); i++)
            {
                var procedure = listProceduresTpTpi[i];
                if (procedure.ProcNumLab != 0 && //proc is a Canadian lab
                    !listTPAProcNums.Contains(procedure.ProcNum) && //Lab does not have a TreatPlanAttach
                    listTPAProcNums.Contains(procedure.ProcNumLab)) //Parent proc has a TreatPlanAttach
                {
                    var treatPlanAttach = new TreatPlanAttach();
                    treatPlanAttach.ProcNum = procedure.ProcNum;
                    treatPlanAttach.TreatPlanNum = listTreatPlanAttaches.First(x => x.ProcNum == procedure.ProcNumLab).TreatPlanNum;
                    listTreatPlanAttaches.Add(treatPlanAttach);
                }
            }
        }

        var listProceduresForActive = new List<Procedure>(); //All procs that should be linked to the active plan (can be linked to inactive plans as well)
        var listProceduresForInactive = new List<Procedure>(); //All procs that should not be linked to the active plan (linked to inactive or unnasigned)
        var arrayProcNumsTpa = listTreatPlanAttaches.Select(x => x.ProcNum).ToArray(); //All procnums from listTPAs, makes it easier to see if a TPA exists for a proc
        var discountPlanSub = DiscountPlanSubs.GetSubForPat(patNum);
        var discountPlan = DiscountPlans.GetForPats([patNum]).FirstOrDefault();

        #endregion Variables

        #region Fill Proc Lists and Create Active and Unassigned Plans

        for (var i = 0; i < listProceduresTpTpi.Count; i++)
            //puts each procedure in listProcsForActive or listProcsForInactive
            if (listProceduresTpTpi[i].ProcStatus == ProcStat.TP //all TP procs should be linked to active plan
                || listProceduresTpTpi[i].AptNum > 0 //all procs attached to an appt should be linked to active plan
                || listProceduresTpTpi[i].PlannedAptNum > 0 //all procs attached to a planned appt should be linked to active plan
                || (treatPlanActive != null //if active plan exists and proc is linked to it, add to list
                    && listTreatPlanAttaches.Any(x => x.ProcNum == listProceduresTpTpi[i].ProcNum && x.TreatPlanNum == treatPlanActive.TreatPlanNum)))
                listProceduresForActive.Add(listProceduresTpTpi[i]);
            else
                //TPi status, AptNum=0, PlannedAptNum=0, and not attached to active plan
                listProceduresForInactive.Add(listProceduresTpTpi[i]);

        //Create active plan if needed
        if (treatPlanActive == null && listProceduresForActive.Count > 0)
        {
            treatPlanActive = new TreatPlan();
            treatPlanActive.Heading = Lans.g("TreatPlans", "Active Treatment Plan");
            treatPlanActive.Note = PrefC.GetString(PrefName.TreatmentPlanNote);
            treatPlanActive.TPStatus = TreatPlanStatus.Active;
            treatPlanActive.PatNum = patNum;
            //treatPlanActive.UserNumPresenter=userNum;
            //treatPlanActive.Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            treatPlanActive.SecUserNumEntry = Security.CurUser.UserNum;
            treatPlanActive.TPType = treatPlanType;
            Insert(treatPlanActive);
            listTreatPlans.Add(treatPlanActive);
        }

        //Update extra active plans to Inactive status, should only ever be one Active status plan
        //All TP procs are linked to the active plan, so proc statuses won't have to change to TPi for procs attached to an "extra" active plan
        var listTreatPlansExtra = listTreatPlans.FindAll(x => x.TPStatus == TreatPlanStatus.Active && treatPlanActive != null
                                                                                                   && x.TreatPlanNum != treatPlanActive.TreatPlanNum);
        for (var i = 0; i < listTreatPlansExtra.Count; i++)
        {
            listTreatPlansExtra[i].TPStatus = TreatPlanStatus.Inactive;
            Update(listTreatPlansExtra[i]);
        }

        //Create unassigned plan if needed
        if (treatPlanUnassigned == null && listProceduresForInactive.Any(x => !arrayProcNumsTpa.Contains(x.ProcNum)))
        {
            treatPlanUnassigned = new TreatPlan();
            treatPlanUnassigned.Heading = Lans.g("TreatPlans", "Unassigned");
            treatPlanUnassigned.Note = PrefC.GetString(PrefName.TreatmentPlanNote);
            treatPlanUnassigned.TPStatus = TreatPlanStatus.Inactive;
            treatPlanUnassigned.PatNum = patNum;
            //treatPlanUnassigned.UserNumPresenter=userNum;
            //treatPlanUnassigned.Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            treatPlanUnassigned.SecUserNumEntry = Security.CurUser.UserNum;
            treatPlanUnassigned.TPType = treatPlanType;
            Insert(treatPlanUnassigned);
            listTreatPlans.Add(treatPlanUnassigned);
        }

        var listDiscountPlanProcs = new List<DiscountPlanProc>();
        if (discountPlanSub != null && !listProceduresForActive.IsNullOrEmpty()) listDiscountPlanProcs = DiscountPlans.GetDiscountPlanProc(listProceduresForActive, discountPlanSub, discountPlan);

        #endregion Fill Proc Lists and Create Active and Unassigned Plans

        #region Procs for Active Plan

        //Update proc status to TP (from TPi) for all procs that should be linked to the active plan.
        //For procs with an existing TPA linking it to the active plan, update proc priority to the TPA priority.
        //Remove any TPAs linking the proc to the unassigned plan.
        //Create TPAs linking the proc to the active plan if needed with TPA priority set to the proc priority.
        for (var i = 0; i < listProceduresForActive.Count(); i++)
        {
            var procedureOld = listProceduresForActive[i].Copy();
            listProceduresForActive[i].ProcStatus = ProcStat.TP;
            //checking the array of ProcNums for an existing TPA is fast, so check the list first
            if (arrayProcNumsTpa.Contains(listProceduresForActive[i].ProcNum))
            {
                if (treatPlanUnassigned != null)
                    //remove any TPAs linking the proc to the unassigned plan
                    listTreatPlanAttaches.RemoveAll(x => x.ProcNum == listProceduresForActive[i].ProcNum && x.TreatPlanNum == treatPlanUnassigned.TreatPlanNum);

                var treatPlanAttachActivePlan = listTreatPlanAttaches.FirstOrDefault(x => x.ProcNum == listProceduresForActive[i].ProcNum && x.TreatPlanNum == treatPlanActive.TreatPlanNum);
                if (treatPlanAttachActivePlan == null)
                {
                    //no TPA linking the proc to the active plan, create one with priority equal to the proc priority
                    var treatPlanAttach = new TreatPlanAttach();
                    treatPlanAttach.ProcNum = listProceduresForActive[i].ProcNum;
                    treatPlanAttach.TreatPlanNum = treatPlanActive.TreatPlanNum;
                    treatPlanAttach.Priority = listProceduresForActive[i].Priority;
                    listTreatPlanAttaches.Add(treatPlanAttach);
                }
                else
                {
                    //TPA linking this proc to the active plan exists, update proc priority to equal TPA priority
                    listProceduresForActive[i].Priority = treatPlanAttachActivePlan.Priority;
                }
            }
            else
            {
                //no TPAs exist for this proc, add one linking the proc to the active plan and set the TPA priority equal to the proc priority
                var treatPlanAttach = new TreatPlanAttach();
                treatPlanAttach.ProcNum = listProceduresForActive[i].ProcNum;
                treatPlanAttach.TreatPlanNum = treatPlanActive.TreatPlanNum;
                treatPlanAttach.Priority = listProceduresForActive[i].Priority;
                listTreatPlanAttaches.Add(treatPlanAttach);
            }

            if (discountPlanSub != null)
                listProceduresForActive[i].DiscountPlanAmt = listDiscountPlanProcs.First(x => x.ProcNum == listProceduresForActive[i].ProcNum).DiscountPlanAmt;
            else
                listProceduresForActive[i].DiscountPlanAmt = 0;

            Procedures.Update(listProceduresForActive[i], procedureOld); //We want to suppress AvaTax errors here or we could end up with a bunch
        }

        #endregion Procs for Active Plan

        #region Procs for Inactive and Unassigned Plans

        //Update proc priority to 0 for all inactive procs.
        //If no TPA exists for the proc, create a TPA with priority 0 linking the proc to the unassigned plan.
        for (var i = 0; i < listProceduresForInactive.Count; i++)
        {
            var procedureOld = listProceduresForInactive[i].Copy();
            listProceduresForInactive[i].Priority = 0;
            Procedures.Update(listProceduresForInactive[i], procedureOld);
            if (treatPlanUnassigned != null && !arrayProcNumsTpa.Contains(listProceduresForInactive[i].ProcNum))
            {
                //no TPAs for this proc, add a new one to the list linking proc to the unassigned plan
                var treatPlanAttach = new TreatPlanAttach();
                treatPlanAttach.TreatPlanNum = treatPlanUnassigned.TreatPlanNum;
                treatPlanAttach.ProcNum = listProceduresForInactive[i].ProcNum;
                treatPlanAttach.Priority = 0;
                listTreatPlanAttaches.Add(treatPlanAttach);
            }
        }

        #endregion Procs for Inactive and Unassigned Plans

        #region Sync and Clean-Up TreatPlanAttach List

        //Remove any TPAs if the proc isn't in listProcsTpTpi, status could've changed or possibly proc is for a different pat.
        listTreatPlanAttaches.RemoveAll(x => !listProceduresTpTpi.Select(y => y.ProcNum).Contains(x.ProcNum));
        if (treatPlanUnassigned != null)
            //if an unassigned plan exists
            //Remove any TPAs from the list that link a proc to the unassigned plan if there is a TPA that links the proc to any other plan
            listTreatPlanAttaches.RemoveAll(x => x.TreatPlanNum == treatPlanUnassigned.TreatPlanNum
                                                 && listTreatPlanAttaches.Any(y => y.ProcNum == x.ProcNum && y.TreatPlanNum != treatPlanUnassigned.TreatPlanNum));

        listTreatPlans.ForEach(x => TreatPlanAttaches.Sync(listTreatPlanAttaches.FindAll(y => y.TreatPlanNum == x.TreatPlanNum), x.TreatPlanNum));
        if (treatPlanUnassigned != null)
        {
            //Must happen after Sync. Delete the unassigned plan if it exists and there are no TPAs pointing to it.
            listTreatPlanAttaches = TreatPlanAttaches.GetAllForTreatPlan(treatPlanUnassigned.TreatPlanNum); //from DB.
            if (listTreatPlanAttaches.Count == 0)
                //nothing attached to unassigned anymore
                TreatPlanCrud.Delete(treatPlanUnassigned.TreatPlanNum);
        }

        #endregion Sync and Clean-Up TreatPlanAttach List
    }

    public static void SyncTreatPlanStatusWithProcs(TreatPlan TreatPlan, bool isMarkingActive, List<TreatPlanAttach> listTreatPlanAttaches, List<TreatPlanAttach> listTreatPlanAttachesAll, List<Procedure> listProceduresTpProcs)
    {
        //get all TPAttaches for this TP where there is either a procedure with a TPAttach linking it to this TP
        //or, if this TP is active, a procedure linked to an appt by AptNum or PlannedAptNun
        var listTreatPlanAttachesNew = listTreatPlanAttaches.FindAll(x => listProceduresTpProcs.Any(y => x.ProcNum == y.ProcNum));
        listProceduresTpProcs.FindAll(x => !listTreatPlanAttachesNew.Any(y => x.ProcNum == y.ProcNum))
            .ForEach(x => listTreatPlanAttachesNew.Add(new TreatPlanAttach {TreatPlanNum = TreatPlan.TreatPlanNum, ProcNum = x.ProcNum, Priority = 0}));
        TreatPlanAttaches.Sync(listTreatPlanAttachesNew, TreatPlan.TreatPlanNum);
        if (isMarkingActive) SetOtherActiveTPsToInactive(TreatPlan);

        if (TreatPlan.TPStatus != TreatPlanStatus.Active) return;

        //we have to this whether we just made this the active or it was already active, otherwise any procs we move off of the active plan will
        //retain the TP status and AuditPlans will throw them back on this TP.
        //Changing the status to TPi of any procs that are not on this plan prevents that from happening.
        var listProcNumsActive = listTreatPlanAttachesNew.Select(x => x.ProcNum).ToList();
        var listTreatPlanAttachesInactive = listTreatPlanAttachesAll.FindAll(x => !listProcNumsActive.Contains(x.ProcNum));
        Procedures.SetTPActive(TreatPlan.PatNum, listTreatPlanAttachesNew.Select(x => x.ProcNum).ToList());
        for (var i = 0; i < listTreatPlanAttachesNew.Count; i++) ProcMultiVisits.UpdateGroupForProc(listTreatPlanAttachesNew[i].ProcNum, ProcStat.TP);

        for (var i = 0; i < listTreatPlanAttachesInactive.Count; i++) ProcMultiVisits.UpdateGroupForProc(listTreatPlanAttachesInactive[i].ProcNum, ProcStat.TPi);
    }

    public static void SetPriorityForProcs(TreatPlan treatPlan, long priorityDefNum, List<long> procNums, int treatPlanCount, bool suppressSecurityMessage = false)
    {
        if (treatPlanCount > 0 && treatPlan.TPStatus is TreatPlanStatus.Active or TreatPlanStatus.Inactive)
        {
            TreatPlanAttaches.SetPriorityForTreatPlanProcs(priorityDefNum, treatPlan.TreatPlanNum, procNums);
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.TreatPlanEdit, treatPlan.DateTP, suppressSecurityMessage))
        {
            return;
        }

        ProcTPs.SetPriorityForTreatPlanProcs(priorityDefNum, treatPlan.TreatPlanNum, procNums);
    }

    public static TreatPlan GetUnassigned(long patNum)
    {
        var commandText =
            "SELECT * FROM treatplan " +
            "WHERE PatNum = " + patNum + " " +
            "AND TPStatus = " + (int) TreatPlanStatus.Inactive + " " +
            "AND Heading = '" + SOut.String("Unassigned") + "'";

        return TreatPlanCrud.SelectOne(commandText) ?? new TreatPlan();
    }

    public static void SetOtherActiveTPsToInactive(TreatPlan treatPlan)
    {
        var activeTreatPlans = TreatPlanCrud.SelectMany(
            "SELECT * FROM treatplan " +
            "WHERE PatNum = " + treatPlan.PatNum + " " +
            "AND TPStatus = " + (int) TreatPlanStatus.Active + " " +
            "AND TreatPlanNum != " + treatPlan.TreatPlanNum);

        foreach (var otherTreatPlan in activeTreatPlans)
        {
            if (otherTreatPlan.Heading == "Active Treatment Plan")
            {
                otherTreatPlan.Heading = "Inactive Treatment Plan";
            }

            otherTreatPlan.TPStatus = TreatPlanStatus.Inactive;

            Update(otherTreatPlan);
        }
    }

    public static List<long> GetNumsByNote(string noteOld)
    {
        noteOld = noteOld.Replace("\r", "");

        return Db.GetListLong(
            "SELECT TreatPlanNum FROM treatplan " +
            "WHERE REPLACE(Note, '\\r', '') = '" + SOut.String(noteOld) + "' " +
            "AND TPStatus IN (" + (int) TreatPlanStatus.Active + ", " + (int) TreatPlanStatus.Inactive + ")");
    }

    public static void UpdateNotes(string noteNew, List<long> listTreatPlanNums)
    {
        if (listTreatPlanNums == null || listTreatPlanNums.Count == 0) return;

        Db.NonQ("UPDATE treatplan SET Note = '" + SOut.String(noteNew) + "' WHERE TreatPlanNum IN (" + string.Join(", ", listTreatPlanNums) + ")");
    }

    public static List<TreatPlan> GetFromProcTPs(List<ProcTP> procTps)
    {
        if (procTps.Count == 0)
        {
            return [];
        }

        var treatPlans = TreatPlanCrud.SelectMany(
            "SELECT * FROM treatplan " +
            "WHERE TreatPlanNum IN (" + string.Join(", ", procTps.Select(x => x.TreatPlanNum)) + ")");

        foreach (var treatPlan in treatPlans)
        {
            treatPlan.ListProcTPs = procTps.Where(x => x.TreatPlanNum == treatPlan.TreatPlanNum).ToList();
        }

        return treatPlans;
    }

    public static List<TreatPlan> GetAllSavedLim(DateTime dateStart, DateTime dateEnd)
    {
        var dataTable = DataCore.GetTable(
            "SELECT TreatPlanNum, PatNum, DateTP, SecUserNumEntry, UserNumPresenter " +
            "FROM treatplan " +
            "WHERE treatplan.TPStatus = " + (int) TreatPlanStatus.Saved + " " +
            "AND DateTP >= " + SOut.Date(dateStart) + " " +
            "AND DateTP <= " + SOut.Date(dateEnd));

        var treatPlans = new List<TreatPlan>();

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            treatPlans.Add(new TreatPlan
            {
                TreatPlanNum = SIn.Long(dataTable.Rows[i]["TreatPlanNum"].ToString()),
                PatNum = SIn.Long(dataTable.Rows[i]["PatNum"].ToString()),
                DateTP = SIn.Date(dataTable.Rows[i]["DateTP"].ToString()),
                SecUserNumEntry = SIn.Long(dataTable.Rows[i]["SecUserNumEntry"].ToString()),
                UserNumPresenter = SIn.Long(dataTable.Rows[i]["UserNumPresenter"].ToString())
            });
        }

        return treatPlans;
    }

    public static void UpdateTreatmentPlanType(Patient patient)
    {
        var treatPlans = GetAllForPat(patient.PatNum);

        treatPlans.RemoveAll(x => x.TPStatus == TreatPlanStatus.Saved);

        var treatPlanType = TreatPlanType.Insurance;
        if (DiscountPlanSubs.HasDiscountPlan(patient.PatNum))
        {
            treatPlanType = TreatPlanType.Discount;
        }

        foreach (var treatPlan in treatPlans)
        {
            if (treatPlan.TPType == treatPlanType)
            {
                continue;
            }

            treatPlan.TPType = treatPlanType;

            Update(treatPlan);
        }
    }
}