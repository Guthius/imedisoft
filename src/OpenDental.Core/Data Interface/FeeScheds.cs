using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class FeeScheds
{
    
    public static long Insert(FeeSched feeSched)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        feeSched.SecUserNumEntry = Security.CurUser.UserNum;
        return FeeSchedCrud.Insert(feeSched);
    }

    
    public static void Update(FeeSched feeSched)
    {
        FeeSchedCrud.Update(feeSched);
    }

    ///<summary>Inserts, updates, or deletes database rows to match supplied list.</summary>
    public static bool Sync(List<FeeSched> listFeeSchedsNew, List<FeeSched> listFeeSchedsOld)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        return FeeSchedCrud.Sync(listFeeSchedsNew, listFeeSchedsOld, Security.CurUser.UserNum);
    }

    ///<summary>Returns the description of the fee schedule.  Appends (hidden) if the fee schedule has been hidden.</summary>
    public static string GetDescription(long feeSchedNum)
    {
        var feeSchedDesc = "";
        var feeSched = GetFirstOrDefault(x => x.FeeSchedNum == feeSchedNum);
        if (feeSched != null) feeSchedDesc = feeSched.Description + (feeSched.IsHidden ? " (" + Lans.g("FeeScheds", "hidden") + ")" : "");
        return feeSchedDesc;
    }

    ///<summary>Returns whether the FeeSched is hidden.  Defaults to true if not found.</summary>
    public static bool GetIsHidden(long feeSchedNum)
    {
        var feeSched = GetFirstOrDefault(x => x.FeeSchedNum == feeSchedNum);
        return feeSched == null ? true : feeSched.IsHidden;
    }

    ///<summary>Returns whether the FeeSched has IsGlobal set to true.  Defaults to false if not found.</summary>
    public static bool IsGlobal(long feeSchedNum)
    {
        var feeSched = GetFirstOrDefault(x => x.FeeSchedNum == feeSchedNum);
        return feeSched == null ? false : feeSched.IsGlobal;
    }

    ///<summary>Will return null if exact name not found.</summary>
    public static FeeSched GetByExactName(string description)
    {
        return GetFirstOrDefault(x => x.Description == description);
    }

    ///<summary>Will return null if exact name not found.</summary>
    public static FeeSched GetByExactName(string description, FeeScheduleType feeScheduleType)
    {
        return GetFirstOrDefault(x => x.FeeSchedType == feeScheduleType && x.Description == description);
    }

    ///<summary>Used to find FeeScheds of a certain type from within a given list.</summary>
    public static List<FeeSched> GetListForType(FeeScheduleType feeScheduleType, bool includeHidden, List<FeeSched> listFeeScheds = null)
    {
        listFeeScheds = listFeeScheds ?? GetDeepCopy();
        var listFeeSchedsRet = new List<FeeSched>();
        for (var i = 0; i < listFeeScheds.Count; i++)
        {
            if (!includeHidden && listFeeScheds[i].IsHidden) continue;
            if (listFeeScheds[i].FeeSchedType == feeScheduleType) listFeeSchedsRet.Add(listFeeScheds[i]);
        }

        return listFeeSchedsRet;
    }

    /// <summary>
    ///     Deletes FeeScheds that are hidden and not attached to any insurance plans.  Returns the number of deleted fee
    ///     scheds.
    /// </summary>
    public static long CleanupAllowedScheds()
    {
        long countDeleted;
        //Detach allowed FeeSchedules from any hidden InsPlans.
        var command = "UPDATE insplan "
                      + "SET AllowedFeeSched=0 "
                      + "WHERE IsHidden=1";
        Db.NonQ(command);
        //Delete unattached FeeSchedules.
        command = "DELETE FROM feesched "
                  + "WHERE FeeSchedNum NOT IN (SELECT AllowedFeeSched FROM insplan) "
                  + "AND FeeSchedType=" + SOut.Int((int) FeeScheduleType.OutNetwork);
        countDeleted = Db.NonQ(command);
        //Delete all orphaned fees.
        command = "SELECT FeeNum FROM fee "
                  + "WHERE FeeSched NOT IN (SELECT FeeSchedNum FROM feesched)";
        var listFeeNums = Db.GetListLong(command);
        Fees.DeleteMany(listFeeNums);
        return countDeleted;
    }

    /// <summary>
    ///     Hides FeeScheds that are not hidden and not in use by anything. Returns the number of fee scheds that were
    ///     hidden.
    /// </summary>
    public static long HideUnusedScheds()
    {
        ODEvent.Fire(ODEventType.HideUnusedFeeSchedules, Lans.g("FormFeeScheds", "Finding unused fee schedules..."));
        var command = @"SELECT feesched.FeeSchedNum 
				FROM feesched
				WHERE feesched.IsHidden=0
				AND NOT EXISTS (SELECT FeeSched FROM insplan WHERE insplan.FeeSched = feesched.FeeSchedNum)
				AND NOT EXISTS (SELECT FeeSched FROM provider WHERE provider.FeeSched = feesched.FeeSchedNum)
				AND NOT EXISTS (SELECT AllowedFeeSched FROM insplan WHERE insplan.AllowedFeeSched = feesched.FeeSchedNum)
				AND NOT EXISTS (SELECT CopayFeeSched FROM insplan WHERE insplan.CopayFeeSched = feesched.FeeSchedNum)
				AND NOT EXISTS (SELECT ManualFeeSchedNum FROM insplan WHERE insplan.ManualFeeSchedNum = feesched.FeeSchedNum)
				AND NOT EXISTS (SELECT FeeSchedNum FROM discountplan WHERE discountplan.FeeSchedNum = feesched.FeeSchedNum)
				AND NOT EXISTS (SELECT FeeSched FROM patient WHERE patient.FeeSched = feesched.FeeSchedNum)";
        var listFeeSchedNums = Db.GetListLong(command);
        if (listFeeSchedNums.Count == 0) return 0;
        ODEvent.Fire(ODEventType.HideUnusedFeeSchedules, Lans.g("FormFeeScheds", "Hiding unused fee schedules..."));
        command = "UPDATE feesched SET IsHidden=1 WHERE FeeSchedNum IN(" + string.Join(",", listFeeSchedNums.Select(x => SOut.Long(x))) + ")";
        var rowsChanged = Db.NonQ(command);
        return rowsChanged;
    }

    private class FamProc
    {
        public long GuarNum;
        public List<PatProc> ListPatProcs = new();
    }

    private class PatProc
    {
        public int Age;
        public List<Procedure> ListProcs = new();
        public long PatNum;
    }

    #region Get Methods

    /// <summary>
    ///     Gets the fee sched from the first insplan, the patient, or the provider in that order.  Uses provNumProc if>0,
    ///     otherwise pat.PriProv.
    ///     Either returns a fee schedule (fk to definition.DefNum) or 0.
    /// </summary>
    public static long GetFeeSched(Patient patient, List<InsPlan> listInsPlans, List<PatPlan> listPatPlans, List<InsSub> listInsSubs, long provNumProc)
    {
        //there's not really a good place to put this function, so it's here.
        long priPlanFeeSched = 0;
        var patPlanPri = listPatPlans.FirstOrDefault(x => x.Ordinal == 1);
        if (patPlanPri != null)
        {
            var planNum = InsSubs.GetSub(patPlanPri.InsSubNum, listInsSubs).PlanNum;
            var insPlan = InsPlans.GetPlan(planNum, listInsPlans);
            if (insPlan != null) priPlanFeeSched = insPlan.FeeSched;
        }

        return GetFeeSched(priPlanFeeSched, patient.FeeSched, provNumProc != 0 ? provNumProc : patient.PriProv); //use provNumProc, but if 0 then default to pat.PriProv
    }

    /// <summary>
    ///     A simpler version of the same function above.  The required numbers can be obtained in a fairly simple query.
    ///     Might return a 0 if the primary provider does not have a fee schedule set.
    /// </summary>
    public static long GetFeeSched(long priPlanFeeSched, long patFeeSched, long provNumProc)
    {
        var provFeeSched = (Providers.GetFirstOrDefault(x => x.ProvNum == provNumProc) ?? new Provider()).FeeSched; //defaults to 0
        return new[] {priPlanFeeSched, patFeeSched, provFeeSched}.FirstOrDefault(x => x > 0); //defaults to 0 if all fee scheds are 0
    }

    /// <summary>
    ///     Gets the fee schedule from the primary MEDICAL insurance plan,
    ///     the first insurance plan, the patient, or the provider in that order.
    /// </summary>
    public static long GetMedFeeSched(Patient patient, List<InsPlan> listInsPlans, List<PatPlan> listPatPlans, List<InsSub> listInsSubs, long provNumProc)
    {
        if (PatPlans.GetInsSubNum(listPatPlans, 1) != 0)
        {
            //Pick the medinsplan with the ordinal closest to zero
            var planOrdinal = 10; //This is a hack, but I doubt anyone would have more than 10 plans
            var hasMedIns = false; //Keep track of whether we found a medical insurance plan, if not use dental insurance fee schedule.
            InsSub insSub;
            for (var i = 0; i < listPatPlans.Count; i++)
            {
                insSub = InsSubs.GetSub(listPatPlans[i].InsSubNum, listInsSubs);
                if (listPatPlans[i].Ordinal < planOrdinal && InsPlans.GetPlan(insSub.PlanNum, listInsPlans).IsMedical)
                {
                    planOrdinal = listPatPlans[i].Ordinal;
                    hasMedIns = true;
                }
            }

            if (!hasMedIns) //If this patient doesn't have medical insurance (under ordinal 10)
                return GetFeeSched(patient, listInsPlans, listPatPlans, listInsSubs, provNumProc); //Use dental insurance fee schedule
            insSub = InsSubs.GetSub(PatPlans.GetInsSubNum(listPatPlans, planOrdinal), listInsSubs);
            var insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlans);
            if (insPlan != null) return insPlan.FeeSched;
        }

        if (patient.FeeSched != 0) return patient.FeeSched;
        if (patient.PriProv == 0) return Providers.GetFirst(true).FeeSched;
        var providerFirst = Providers.GetFirst(); //Used in order to preserve old behavior...  If this fails, then old code would have failed.
        var provider = Providers.GetFirstOrDefault(x => x.ProvNum == patient.PriProv) ?? providerFirst;
        return provider.FeeSched;
    }

    ///<summary>Gets one FeeSched object from the database using the primary key. Returns null if not found.</summary>
    public static FeeSched GetOneFeeSched(long feeSchedNum)
    {
        return FeeSchedCrud.SelectOne(feeSchedNum);
    }

    #endregion

    #region Misc Methods

    /// <summary>
    ///     Copies one fee schedule to one or more fee schedules.  clinicNumFrom, provNumFrom, and toProvNum can be zero.
    ///     Set listClinicNumsTo to copy to multiple clinic overrides.  If this list is null or empty, clinicNum 0 will be
    ///     used.
    /// </summary>
    public static void CopyFeeSchedule(FeeSched feeSchedFrom, long clinicNumFrom, long provNumFrom, FeeSched feeSchedTo, List<long> listClinicNumsTo, long provNumTo, DateTime dateEffectiveOld = new(), DateTime dateEffectiveNew = new())
    {
        if (listClinicNumsTo == null) listClinicNumsTo = new List<long>();
        if (listClinicNumsTo.Count == 0) listClinicNumsTo.Add(0);
        //Store a local copy of the fees from the old FeeSched
        var listFeesLocalCopy = Fees.GetListExact(feeSchedTo.FeeSchedNum, listClinicNumsTo, provNumTo, dateEffectiveNew);
        //Delete all fees that exactly match setting in "To" combo selections.
        for (var i = 0; i < listClinicNumsTo.Count; i++) Fees.DeleteFees(feeSchedTo.FeeSchedNum, listClinicNumsTo[i], provNumTo, dateEffectiveNew);
        //Copy:
        var listFeesNew = Fees.GetListExact(feeSchedFrom.FeeSchedNum, clinicNumFrom, provNumFrom, dateEffectiveOld);
        var blockValue = 0;
        var blockMax = listFeesNew.Count * listClinicNumsTo.Count;
        var listActions = new List<Action>();
        for (var i = 0; i < listClinicNumsTo.Count; i++)
        {
            var securityLogText = "";
            if (listFeesNew.IsNullOrEmpty())
            {
                securityLogText = "Fee Schedule \"" + feeSchedFrom.Description + "\" copied to Fee Schedule \"" + feeSchedTo.Description + "\".\r\n";
                securityLogText += "  Note: Fee Schedule \"" + feeSchedFrom.Description + "\" was empty and has overwritten Fee Schedule \"" + feeSchedTo.Description + "\".";
                SecurityLogs.MakeLogEntry(EnumPermType.FeeSchedEdit, 0, securityLogText);
            }

            for (var j = 0; j < listFeesNew.Count; j++)
            {
                var isReplacementFee = false;
                var feeNew = listFeesNew[j].Copy();
                feeNew.FeeNum = 0;
                feeNew.ProvNum = provNumTo;
                feeNew.ClinicNum = listClinicNumsTo[i];
                feeNew.FeeSched = feeSchedTo.FeeSchedNum;
                var dateEffective = dateEffectiveNew;
                if (dateEffective == DateTime.MinValue) dateEffective = dateEffectiveOld; //Keep the old date effective if the new one isn't set
                feeNew.DateEffective = dateEffective;
                Fees.Insert(feeNew);
                //Check to see if this replaced an old fee with the same fee details
                var feeOld = listFeesLocalCopy.Where(x => x.ProvNum == feeNew.ProvNum)
                    .Where(x => x.ClinicNum == feeNew.ClinicNum)
                    .Where(x => x.CodeNum == feeNew.CodeNum)
                    .Where(x => x.FeeSched == feeNew.FeeSched)
                    .Where(x => x.DateEffective == feeNew.DateEffective)
                    .FirstOrDefault();
                if (feeOld != null) isReplacementFee = true;
                var procedureCode = ProcedureCodes.GetProcCode(listFeesNew[j].CodeNum);
                securityLogText = "Fee Schedule \"" + feeSchedFrom.Description + "\" copied to Fee Schedule \"" + feeSchedTo.Description + "\", ";
                if (listClinicNumsTo[i] != 0) securityLogText += "To Clinic \"" + Clinics.GetDesc(listClinicNumsTo[i]) + "\", ";
                securityLogText += "Proc Code \"" + procedureCode.ProcCode + "\", Fee \"" + listFeesNew[j].Amount + "\", ";
                if (isReplacementFee) securityLogText += "Replacing Previous Fee \"" + feeOld.Amount + "\"";
                SecurityLogs.MakeLogEntry(EnumPermType.FeeSchedEdit, 0, securityLogText);
                ODEvent.Fire(ODEventType.ProgressBar,
                    new ProgressBarHelper(Lans.g("FormFeeSchedTools", "Copying fees, please wait") + "...", blockValue: blockValue, blockMax: blockMax,
                        progressStyle: ProgBarStyle.Blocks));
                blockValue++;
            }
        }
    }

    /// <summary>
    ///     Replaces ImportCanadaFeeSchedule.  Imports a canadian fee schedule. Called only in FormFeeSchedTools, located here
    ///     to allow unit testing.
    ///     Fires FeeSchedEvents for a progress bar.
    /// </summary>
    public static List<Fee> ImportCanadaFeeSchedule2(FeeSched feeSched, string feeData, long clinicNum, long provNum, out int numImported, out int numSkipped, DateTime dateEffective = new())
    {
        var listStringsFeeLines = feeData.Split('\n').ToList();
        numImported = 0;
        numSkipped = 0;
        var listFees = Fees.GetListExact(feeSched.FeeSchedNum, clinicNum, provNum, dateEffective);
        var listFeesImported = new List<Fee>(listFees);
        for (var i = 0; i < listStringsFeeLines.Count; i++)
        {
            var listStringFields = listStringsFeeLines[i].Split('\t').ToList();
            if (listStringFields.Count <= 1) // && fields[1]!=""){//we no longer skip blank fees
                continue;
            var procCode = listStringFields[0];
            if (ProcedureCodes.IsValidCode(procCode))
            {
                var codeNum = ProcedureCodes.GetCodeNum(procCode);
                var fee = Fees.GetFee(codeNum, feeSched.FeeSchedNum, clinicNum, provNum, listFees, dateEffective); //gets best match
                var feeOldStr = "";
                var datePrevious = DateTime.MinValue;
                if (fee != null)
                {
                    feeOldStr = "Old Fee: " + fee.Amount.ToString("c") + ", ";
                    datePrevious = fee.SecDateTEdit;
                }

                if (listStringFields[1] == "")
                {
                    //an empty entry will delete an existing fee, but not insert a blank override
                    if (fee == null)
                    {
                        //nothing to do
                    }
                    else
                    {
                        //doesn't matter if the existing fee is an override or not.
                        Fees.Delete(fee);
                        listFeesImported.Remove(fee);
                        SecurityLogs.MakeLogEntry(EnumPermType.ProcFeeEdit, 0, "Procedure: " + listStringFields[0] + ", "
                                                                               + feeOldStr
                                                                               //+", Deleted Fee: "+fee.Amount.ToString("c")+", "
                                                                               + "Fee Schedule: " + GetDescription(feeSched.FeeSchedNum) + ". "
                                                                               + "Fee deleted using the Import Canada button in the Fee Tools window.", codeNum,
                            DateTime.MinValue);
                        SecurityLogs.MakeLogEntry(EnumPermType.LogFeeEdit, 0, "Fee deleted", fee.FeeNum, datePrevious);
                    }
                }
                else
                {
                    //value found in text file
                    if (fee == null)
                    {
                        //no current fee
                        fee = new Fee();
                        fee.Amount = SIn.Double(listStringFields[1], true); //The fees are always in the format "1.00" so we need to parse accordingly.
                        fee.FeeSched = feeSched.FeeSchedNum;
                        fee.CodeNum = codeNum;
                        fee.ClinicNum = clinicNum;
                        fee.ProvNum = provNum;
                        fee.DateEffective = dateEffective;
                        Fees.Insert(fee);
                        listFeesImported.Add(fee);
                    }
                    else
                    {
                        fee.Amount = SIn.Double(listStringFields[1], true);
                        Fees.Update(fee);
                    }

                    SecurityLogs.MakeLogEntry(EnumPermType.ProcFeeEdit, 0, "Procedure: " + listStringFields[0] + ", "
                                                                           + feeOldStr
                                                                           + ", New Fee: " + fee.Amount.ToString("c") + ", "
                                                                           + "Fee Schedule: " + GetDescription(feeSched.FeeSchedNum) + ". "
                                                                           + "Fee changed using the Import Canada button in the Fee Tools window.", codeNum,
                        DateTime.MinValue);
                    SecurityLogs.MakeLogEntry(EnumPermType.LogFeeEdit, 0, "Fee changed", fee.FeeNum, datePrevious);
                }

                numImported++;
            }
            else
            {
                numSkipped++;
            }

            ODEvent.Fire(ODEventType.FeeSched,
                new ProgressBarHelper(Lans.g("FeeScheds", "Processing fees, please wait") + "...", "", numImported + numSkipped, listStringsFeeLines.Count,
                    ProgBarStyle.Continuous));
        }

        return listFeesImported;
    }

    ///<summary>Exports a fee schedule.  Called only in FormFeeSchedTools. Fires FeeSchedEvents for a progress bar.</summary>
    public static void ExportFeeSchedule(long feeSchedNum, long clinicNum, long provNum, string fileName, DateTime dateEffective = new())
    {
        //CreateText will overwrite any content if the file already exists.
        using var streamWriter = File.CreateText(fileName);
        //Get every single procedure code from the cache which will already be ordered by ProcCat and then ProcCode.
        //Even if the code does not have a fee, include it in the export because that will trigger a 'deletion' when importing over other schedules.
        var rowNum = 0;
        var listProcedureCodes = ProcedureCodes.GetListDeep();
        var listFees = Fees.GetListForScheds(feeSchedNum, clinicNum, provNum, dateEffective: dateEffective); //gets best matches
        for (var i = 0; i < listProcedureCodes.Count; i++)
        {
            //Get the best matching fee (not exact match) for the current selections. 
            var fee = Fees.GetFee(listProcedureCodes[i].CodeNum, feeSchedNum, clinicNum, provNum, listFees, dateEffective);
            streamWriter.Write(listProcedureCodes[i].ProcCode + "\t");
            if (fee != null && fee.Amount != -1) streamWriter.Write(fee.Amount.ToString("n"));
            streamWriter.Write("\t");
            streamWriter.Write(listProcedureCodes[i].AbbrDesc + "\t");
            streamWriter.WriteLine(listProcedureCodes[i].Descript);
            var percent = rowNum * 1.0 / listProcedureCodes.Count * 100;
            ODEvent.Fire(ODEventType.ProgressBar, new ProgressBarHelper(
                "Exporting fees, please wait...", percent.ToString(), (int) percent, progressStyle: ProgBarStyle.Blocks));
            rowNum++;
        }
    }

    ///<summary>Used for moving feesched items to a new location within the feesched list.</summary>
    public static void RepositionFeeSched(FeeSched feeSched, int newItemOrder)
    {
        string command;
        //change specific row in question.
        command = "UPDATE feesched SET ItemOrder=" + SOut.Int(newItemOrder) + " WHERE FeeSchedNum=" + SOut.Long(feeSched.FeeSchedNum);
        Db.NonQ(command);
        //decrement items below old pos to close the gap, except the one we're moving
        command = "UPDATE feesched SET ItemOrder=ItemOrder-1 WHERE ItemOrder >" + SOut.Int(feeSched.ItemOrder)
                                                                                + " AND FeeSchedNum !=" + SOut.Long(feeSched.FeeSchedNum);
        Db.NonQ(command);
        //increment items (move down) at or below new pos, except the one we're moving
        command = "UPDATE feesched SET ItemOrder=ItemOrder+1 WHERE ItemOrder >= " + SOut.Int(newItemOrder)
                                                                                  + " AND FeeSchedNum !=" + SOut.Long(feeSched.FeeSchedNum);
        Db.NonQ(command);
    }

    ///<summary>Used for sorting feesched based on FeeSchedType followed by Description.</summary>
    public static void SortFeeSched()
    {
        //Jordan Bad pattern
        var command = @"UPDATE feesched,(SELECT @neworder:=-1) a,
				(SELECT FeeSchedNum,(@neworder := @neworder+1) AS NewOrderCol
				FROM feesched
				ORDER BY FeeSchedType,Description) AS feesched2
				SET feesched.ItemOrder=feesched2.NewOrderCol
				WHERE feesched.FeeSchedNum=feesched2.FeeSchedNum";
        Db.NonQ(command);
    }

    ///<summary>Used for checking to make sure that the feesched ItemOrder column is in sequential order.</summary>
    public static void CorrectFeeSchedOrder()
    {
        //Jordan Bad pattern
        var command = @"UPDATE feesched,(SELECT @neworder:=-1) a,
				(SELECT FeeSchedNum,(@neworder := @neworder+1) AS NewOrderCol
				FROM feesched
				ORDER BY ItemOrder) AS feesched2
				SET feesched.ItemOrder=feesched2.NewOrderCol
				WHERE feesched.FeeSchedNum=feesched2.FeeSchedNum";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Updates write-off estimated for claimprocs for the passed in clinics. Called only in FormFeeSchedTools, located
    ///     here to allow unit
    ///     testing. Requires an ODProgressExtended to display UI updates.  If clinics are enabled and the user is not clinic
    ///     restricted and chooses to run
    ///     for all clinics, set doUpdatePrevClinicPref to true so that the ClinicNums will be stored in the preference table
    ///     as they are finished to allow
    ///     for pausing/resuming the process.
    /// </summary>
    public static long GlobalUpdateWriteoffs(List<long> listClinicNumsWriteoff, ODProgressExtended progressExtended, bool doUpdatePrevClinicPref = false)
    {
        long totalWriteoffsUpdated = 0;
        var listFeesHQ = Fees.GetByClinicNum(0); //All HQ fees
        Dictionary<long, List<Procedure>> dictPatProcs;
        List<FamProc> listFamProcs;
        Dictionary<long, List<ClaimProc>> dictClaimProcs;
        List<Fee> listFeesHQandClinic;
        Lookup<FeeKey2, Fee> lookupFeesByCodeAndSched;
        List<InsSub> listInsSubs;
        List<InsPlan> listInsPlans;
        List<PatPlan> listPatPlans;
        List<Benefit> listBenefits;
        List<Action> listActions;
        //Get all objects needed to check if procedures are linked to an orthocase here to avoid querying in loops.
        var listOrthoProcLinksAll = new List<OrthoProcLink>();
        var dictOrthoProcLinksAll = new Dictionary<long, OrthoProcLink>();
        var dictOrthoCases = new Dictionary<long, OrthoCase>();
        var dictOrthoSchedules = new Dictionary<long, OrthoSchedule>();
        OrthoCases.GetDataForAllProcLinks(ref listOrthoProcLinksAll, ref dictOrthoProcLinksAll, ref dictOrthoCases, ref dictOrthoSchedules);
        OrthoProcLink orthoProcLink = null;
        OrthoCase orthoCase = null;
        OrthoSchedule orthoSchedule = null;
        List<OrthoProcLink> listOrthoProcLinksForOrthoCase = null;
        for (var i = 0; i < listClinicNumsWriteoff.Count; i++)
        {
            progressExtended.Fire(ODEventType.FeeSched, new ProgressBarHelper(Clinics.GetAbbr(listClinicNumsWriteoff[i]), "0%", 0, 100, ProgBarStyle.Blocks, "WriteoffProgress"));
            long rowCurIndex = 0; //reset for each clinic.
            var lockObj = new object(); //used to lock rowCurIndex so the threads will correctly increment the count
            progressExtended.Fire(ODEventType.FeeSched, new ProgressBarHelper(Lans.g("FeeSchedEvent", "Getting list to update writeoffs..."),
                progressBarEventType: ProgBarEventType.TextMsg));
            listFeesHQandClinic = listFeesHQ;
            if (true && listClinicNumsWriteoff[i] > 0) //listFeesHQ is already the fees for ClinicNum 0, only add to list if > 0
                listFeesHQandClinic.AddRange(Fees.GetByClinicNum(listClinicNumsWriteoff[i])); //could be empty for some clinics that don't use overrides
            lookupFeesByCodeAndSched = (Lookup<FeeKey2, Fee>) listFeesHQandClinic.ToLookup(x => new FeeKey2(x.CodeNum, x.FeeSched));
            List<Procedure> listProceduresTp;
            if (true)
                listProceduresTp = Procedures.GetAllTp(listClinicNumsWriteoff[i]);
            else //clinics not enabled
                listProceduresTp = Procedures.GetAllTp();
            dictPatProcs = listProceduresTp
                .GroupBy(x => x.PatNum)
                .ToDictionary(x => x.Key, x => Procedures.SortListByTreatPlanPriority(x.ToList()));

            #region Has Paused or Cancelled

            while (true)
            {
                if (!progressExtended.IsPaused) break;
                progressExtended.AllowResume();
                if (progressExtended.IsCanceled) break;
            }

            if (progressExtended.IsCanceled) break;

            #endregion Has Paused or Cancelled

            if (dictPatProcs.Count == 0) continue;
            var procCount = dictPatProcs.Sum(x => x.Value.Count);
            listFamProcs = Patients.GetFamilies(dictPatProcs.Keys.ToList()).Where(x => x.Guarantor != null)
                .Select(x => new FamProc
                {
                    GuarNum = x.Guarantor.PatNum,
                    ListPatProcs = x.ListPats.Select(y => new PatProc
                    {
                        PatNum = y.PatNum,
                        Age = y.Age,
                        ListProcs = dictPatProcs.TryGetValue(y.PatNum, out var listProcsCurr) ? listProcsCurr : new List<Procedure>()
                    }).ToList()
                }).ToList();
            listPatPlans = PatPlans.GetPatPlansForPats(dictPatProcs.Keys.ToList());
            listInsSubs = InsSubs.GetListInsSubs(dictPatProcs.Keys.ToList());
            var listInsSubNums = listInsSubs.Select(x => x.InsSubNum).ToList();
            listInsSubs.AddRange(InsSubs.GetMany(listPatPlans.Select(x => x.InsSubNum).Distinct().Where(x => !listInsSubNums.Contains(x)).ToList()));
            listInsSubs = listInsSubs.DistinctBy(x => x.InsSubNum).ToList();
            listInsPlans = InsPlans.RefreshForSubList(listInsSubs);
            listBenefits = Benefits.GetAllForPatPlans(listPatPlans, listInsSubs);

            #region Has Paused or Cancelled

            while (true)
            {
                if (!progressExtended.IsPaused) break;
                progressExtended.AllowResume();
                if (progressExtended.IsCanceled) break;
            }

            if (progressExtended.IsCanceled) break;

            #endregion Has Paused or Cancelled

            //dictionary of key=PatNum, value=list of claimprocs, i.e. a dictionary linking each PatNum to a list of claimprocs for the given procs
            dictClaimProcs = ClaimProcs.GetForProcs(dictPatProcs.SelectMany(x => x.Value.Select(y => y.ProcNum)).ToList(), useDataReader: true)
                .GroupBy(x => x.PatNum)
                .ToDictionary(x => x.Key, x => x.ToList());

            #region Has Paused or Cancelled

            while (true)
            {
                if (!progressExtended.IsPaused) break;
                progressExtended.AllowResume();
                if (progressExtended.IsCanceled) break;
            }

            if (progressExtended.IsCanceled) break;

            #endregion Has Paused or Cancelled

            progressExtended.Fire(ODEventType.FeeSched, new ProgressBarHelper(Lans.g("FeeSchedEvent", "Updating write-off estimates for patients..."),
                progressBarEventType: ProgBarEventType.TextMsg));
            listActions = listFamProcs.Select(x => new Action(() =>
            {
                #region Has Cancelled

                if (progressExtended.IsCanceled) return;

                #endregion Has Cancelled

                var listPatNums = x.ListPatProcs.Select(y => y.PatNum).ToList();
                var listDiscountPlanSubs = DiscountPlanSubs.GetSubsForPats(listPatNums);
                var listInsSubNumsPatPlanCur = listPatPlans.Where(y => listPatNums.Contains(y.PatNum)).Select(y => y.InsSubNum).ToList();
                var listInsSubsCur = listInsSubs.FindAll(y => listPatNums.Contains(y.Subscriber) || listInsSubNumsPatPlanCur.Contains(y.InsSubNum));
                var listInsSubPlanNumsCur = listInsSubsCur.Select(y => y.PlanNum).ToList();
                var listInsPlansCur = listInsPlans.FindAll(y => listInsSubPlanNumsCur.Contains(y.PlanNum));
                var listSubstitutionLinks = SubstitutionLinks.GetAllForPlans(listInsPlansCur);
                List<PatPlan> listPatPlansCur;
                List<Benefit> listBenefitsCur;
                var listPatProcs = x.ListPatProcs;
                for (var j = 0; j < listPatProcs.Count; j++)
                {
                    //foreach patient in the family
                    if (listPatProcs[j].ListProcs.IsNullOrEmpty()) continue;
                    DiscountPlanSubs.UpdateAssociatedDiscountPlanAmts(listDiscountPlanSubs.FindAll(x => x.PatNum == listPatProcs[j].PatNum));
                    listPatPlansCur = listPatPlans.FindAll(y => y.PatNum == listPatProcs[j].PatNum);
                    var listInsPlanNumsCur = listInsPlansCur.Select(y => y.PlanNum).ToList();
                    var listPatPlanNumsCur = listPatPlansCur.Select(y => y.PatPlanNum).ToList();
                    listBenefitsCur = listBenefits
                        .FindAll(y => listInsPlanNumsCur.Contains(y.PlanNum) || listPatPlanNumsCur.Contains(y.PatPlanNum));
                    listBenefitsCur.Sort();
                    if (!dictClaimProcs.TryGetValue(listPatProcs[j].PatNum, out var listClaimProcsCur)) listClaimProcsCur = new List<ClaimProc>();
                    var blueBookEstimateData = new BlueBookEstimateData(listInsPlansCur, listInsSubsCur, listPatPlansCur, listPatProcs[j].ListProcs, listSubstitutionLinks);
                    var listProcedures = listPatProcs[j].ListProcs;
                    for (var k = 0; k < listProcedures.Count; k++)
                    {
                        //foreach proc for this patient
                        OrthoCases.FillOrthoCaseObjectsForProc(listProcedures[k].ProcNum, ref orthoProcLink, ref orthoCase, ref orthoSchedule
                            , ref listOrthoProcLinksForOrthoCase, dictOrthoProcLinksAll, dictOrthoCases, dictOrthoSchedules, listOrthoProcLinksAll);
                        Procedures.ComputeEstimates(listProcedures[k], listPatProcs[j].PatNum, ref listClaimProcsCur, false, listInsPlansCur, listPatPlansCur, listBenefitsCur,
                            null, null, true, listPatProcs[j].Age, listInsSubsCur, listSubstLinks: listSubstitutionLinks, lookupFees: lookupFeesByCodeAndSched,
                            orthoProcLink: orthoProcLink, orthoCase: orthoCase, orthoSchedule: orthoSchedule, listOrthoProcLinksForOrthoCase: listOrthoProcLinksForOrthoCase,
                            blueBookEstimateData: blueBookEstimateData);
                        double percentage = 0;
                        lock (lockObj)
                        {
                            percentage = Math.Ceiling((double) ++rowCurIndex / procCount * 100);
                        }

                        progressExtended.Fire(ODEventType.FeeSched,
                            new ProgressBarHelper(Clinics.GetAbbr(listClinicNumsWriteoff[i]), (int) percentage + "%", (int) percentage, 100, ProgBarStyle.Blocks, "WriteoffProgress"));
                    }
                }
            })).ToList();
            ODThread.RunParallel(listActions, TimeSpan.FromHours(3),
                onException: ex =>
                {
                    //Notify the user what went wrong via the text box.
                    progressExtended.Fire(ODEventType.FeeSched, new ProgressBarHelper("Error updating writeoffs: " + ex.Message,
                        progressBarEventType: ProgBarEventType.TextMsg));
                }
            );
            if (listClinicNumsWriteoff.Count > 1) //only show if more than one clinic
                progressExtended.Fire(ODEventType.FeeSched,
                    new ProgressBarHelper(rowCurIndex + " " + Lans.g("FeeSchedTools", "procedures processed from") + " " + Clinics.GetAbbr(listClinicNumsWriteoff[i]),
                        progressBarEventType: ProgBarEventType.TextMsg));
            totalWriteoffsUpdated += rowCurIndex;
            if (doUpdatePrevClinicPref && rowCurIndex == procCount)
            {
                //if storing previously completed clinic and we actually completed this clinic's procs, update the pref
                if (listClinicNumsWriteoff.Last() == listClinicNumsWriteoff[i])
                    //if this is the last clinic in the list, clear the last clinic pref so the next time it will run for all clinics
                    Prefs.UpdateString(PrefName.GlobalUpdateWriteOffLastClinicCompleted, "");
                else
                    Prefs.UpdateString(PrefName.GlobalUpdateWriteOffLastClinicCompleted, SOut.Long(listClinicNumsWriteoff[i]));
                Signalods.SetInvalid(InvalidType.Prefs);
            }

            #region Has Cancelled

            if (progressExtended.IsCanceled) break;

            #endregion Has Cancelled
        }

        progressExtended.OnProgressDone();
        progressExtended.Fire(ODEventType.FeeSched, new ProgressBarHelper("Writeoffs updated. " + totalWriteoffsUpdated + " procedures processed.\r\nDone.",
            progressBarEventType: ProgBarEventType.TextMsg));
        return totalWriteoffsUpdated;
    }

    #endregion

    #region CachePattern

    private class FeeSchedCache : CacheListAbs<FeeSched>
    {
        protected override List<FeeSched> GetCacheFromDb()
        {
            var command = "SELECT * FROM feesched ORDER BY ItemOrder";
            return FeeSchedCrud.SelectMany(command);
        }

        protected override List<FeeSched> TableToList(DataTable dataTable)
        {
            return FeeSchedCrud.TableToList(dataTable);
        }

        protected override FeeSched Copy(FeeSched item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<FeeSched> items)
        {
            return FeeSchedCrud.ListToTable(items, "FeeSched");
        }

        protected override void FillCacheIfNeeded()
        {
            FeeScheds.GetTableFromCache(false);
        }

        protected override bool IsInListShort(FeeSched item)
        {
            return !item.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly FeeSchedCache _feeSchedCache = new();

    public static int GetCount(bool isShort = false)
    {
        return _feeSchedCache.GetCount(isShort);
    }

    public static List<FeeSched> GetDeepCopy(bool isShort = false)
    {
        return _feeSchedCache.GetDeepCopy(isShort);
    }

    public static FeeSched GetFirst(bool isShort = true)
    {
        return _feeSchedCache.GetFirst(isShort);
    }

    public static FeeSched GetFirst(Func<FeeSched, bool> match, bool isShort = true)
    {
        return _feeSchedCache.GetFirst(match, isShort);
    }

    public static FeeSched GetFirstOrDefault(Func<FeeSched, bool> match, bool isShort = false)
    {
        return _feeSchedCache.GetFirstOrDefault(match, isShort);
    }

    public static List<FeeSched> GetWhere(Predicate<FeeSched> match, bool isShort = false)
    {
        return _feeSchedCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable dataTable)
    {
        _feeSchedCache.FillCacheFromTable(dataTable);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _feeSchedCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _feeSchedCache.ClearCache();
    }

    #endregion Cache Pattern
}