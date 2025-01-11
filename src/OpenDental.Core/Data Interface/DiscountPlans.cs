using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DiscountPlans
{
    #region Methods - Insert

    
    public static long Insert(DiscountPlan discountPlan)
    {
        return DiscountPlanCrud.Insert(discountPlan);
    }

    #endregion

    #region Methods - Get

    
    public static List<DiscountPlan> GetAll(bool includeHidden)
    {
        var command = "SELECT * FROM discountplan";
        if (!includeHidden) command += " WHERE IsHidden=0";
        return DiscountPlanCrud.SelectMany(command);
    }

    ///<summary>Returns a list of DiscountPlans for a list of passed in DiscountPlanNums.</summary>
    public static List<DiscountPlan> GetDiscountPlansByPlanNum(List<long> listDiscountPlanNums)
    {
        if (listDiscountPlanNums.Count == 0) return new List<DiscountPlan>();

        var command = "SELECT * "
                      + "FROM discountplan "
                      + "WHERE discountplan.DiscountPlanNum IN (" + string.Join(",", listDiscountPlanNums) + ")";
        return DiscountPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of discountplans for a list of passed in patnums. There is no guarantee that a patnum will have
    ///     a discount plan.
    /// </summary>
    public static List<DiscountPlan> GetForPats(List<long> listPatNums)
    {
        if (listPatNums.Count < 1) return new List<DiscountPlan>();

        var command = "SELECT discountplan.* "
                      + "FROM discountplan "
                      + "INNER JOIN discountplansub ON discountplansub.DiscountPlanNum=discountplan.DiscountPlanNum "
                      + "WHERE discountplansub.PatNum IN (" + string.Join(",", listPatNums) + ")";
        return DiscountPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Takes in a list of patnums and returns a dictionary of PatNum to DiscountPlan.FeeSchedNum pairs. Value is 0 if
    ///     no discount plan exists
    /// </summary>
    public static Dictionary<long, long> GetFeeSchedNumsByPatNums(List<long> listPatNums)
    {
        if (listPatNums.IsNullOrEmpty()) return new Dictionary<long, long>();

        var command = "SELECT patient.PatNum,discountplan.FeeSchedNum "
                      + "FROM patient "
                      + "INNER JOIN discountplansub ON discountplansub.PatNum=patient.PatNum "
                      + "INNER JOIN discountplan ON discountplan.DiscountPlanNum=discountplansub.DiscountPlanNum "
                      + "WHERE patient.PatNum IN (" + string.Join(",", listPatNums) + ")";
        return DataCore.GetTable(command).Select()
            .ToDictionary(x => SIn.Long(x["PatNum"].ToString()), x => SIn.Long(x["FeeSchedNum"].ToString()));
    }

    ///<summary>Returns an empty list if planNum is 0.</summary>
    public static List<string> GetPatNamesForPlan(long discountPlanNum)
    {
        if (discountPlanNum == 0) return new List<string>();

        var command = "SELECT patient.LName,patient.FName "
                      + "FROM discountplansub "
                      + "LEFT JOIN patient ON discountplansub.PatNum=patient.PatNum "
                      + "WHERE discountplansub.DiscountPlanNum=" + SOut.Long(discountPlanNum) + " "
                      + "AND patient.PatStatus NOT IN (" + SOut.Int((int) PatientStatus.Deleted) + "," + SOut.Int((int) PatientStatus.Deceased) + ") ";
        //No Preferred or MiddleI needed because this logic needs to match FormInsPlan.
        return DataCore.GetTable(command).Select().Select(x => Patients.GetNameLFnoPref(x["LName"].ToString(), x["FName"].ToString(), "")).ToList();
    }

    public static int GetPatCountForPlan(long discountPlanNum)
    {
        if (discountPlanNum == 0) return 0;

        var command = "SELECT COUNT(discountplansub.PatNum) "
                      + "FROM discountplansub "
                      + "LEFT JOIN patient ON discountplansub.PatNum=patient.PatNum "
                      + "WHERE discountplansub.DiscountPlanNum=" + SOut.Long(discountPlanNum) + " "
                      + "AND patient.PatStatus NOT IN (" + SOut.Int((int) PatientStatus.Deleted) + "," + SOut.Int((int) PatientStatus.Deceased) + ")";
        return SIn.Int(Db.GetCount(command));
    }


    public class CountPerPlan
    {
        public int Count;
        public long DiscountPlanNum;
    }

    /// <summary>
    ///     Returns a list of the count of patients for the DiscountPlanNum.
    ///     Returns an empty list if the list of plan nums is empty.
    /// </summary>
    public static List<CountPerPlan> GetPatCountsForPlans(List<long> listPlanNums)
    {
        var listCountPerPlan = new List<CountPerPlan>();
        if (listPlanNums.Count == 0) return listCountPerPlan;
        var command = "SELECT discountplansub.DiscountPlanNum,COUNT(discountplansub.PatNum) PatCount "
                      + "FROM discountplansub "
                      + "LEFT JOIN patient ON discountplansub.PatNum=patient.PatNum "
                      + "WHERE discountplansub.DiscountPlanNum IN (" + string.Join(",", listPlanNums) + ") "
                      + "AND patient.PatStatus NOT IN (" + SOut.Int((int) PatientStatus.Deleted) + "," + SOut.Int((int) PatientStatus.Deceased) + ") "
                      + "GROUP BY discountplansub.DiscountPlanNum";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var countPerPlan = new CountPerPlan();
            countPerPlan.DiscountPlanNum = SIn.Long(table.Rows[i]["DiscountPlanNum"].ToString());
            countPerPlan.Count = SIn.Int(table.Rows[i]["PatCount"].ToString());
            listCountPerPlan.Add(countPerPlan);
        }

        return listCountPerPlan;
    }

    /// <summary>
    ///     Returns a list of ProcedureCodes that are stored in PrefName.DiscountPlan[x]Code.
    ///     The pref must store a comma delimited list of D-Codes.
    /// </summary>
    private static List<ProcedureCode> GetProcedureCodesByPref(PrefName prefName)
    {
        var listCodeNum = ProcedureCodes.GetCodeNumsForPref(prefName);
        return ProcedureCodes.GetCodesForCodeNums(listCodeNum);
    }

    ///<summary>Gets one DiscountPlan from the db.</summary>
    public static DiscountPlan GetPlan(long discountPlanNum)
    {
        return DiscountPlanCrud.SelectOne(discountPlanNum);
    }

    ///<summary>Gets a list of all DiscountPlans from the database.</summary>
    public static List<DiscountPlan> GetPlans()
    {
        var command = "SELECT * FROM discountplan";
        return DiscountPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a DiscountPlanProc object for every procedure passed in. It is assumed that all procedures are for the same
    ///     patient. Passing in an empty list of Adjustments will assume
    ///     no prior adjustments of the discountPlan.DefNum had been applied.
    /// </summary>
    public static List<DiscountPlanProc> GetDiscountPlanProc(List<Procedure> listProcedures, DiscountPlanSub discountPlanSub = null, DiscountPlan discountPlan = null, List<Adjustment> listAdjustments = null)
    {
        if (listProcedures.IsNullOrEmpty() || discountPlanSub == null || discountPlan == null) return new List<DiscountPlanProc>();

        var listDiscountPlanProcs = new List<DiscountPlanProc>();
        var listProceduresHist = new List<Procedure>();
        var listAnnualTots = Adjustments.GetAnnualTotalsForPatByDiscountPlan(discountPlanSub.PatNum, discountPlanSub.DateEffective, discountPlanSub.DateTerm, discountPlan, listProcedures.Max(x => x.ProcDate), listAdjustments);
        for (var i = 0; i < listProcedures.Count; i++)
        {
            var annualBucketIndex = Adjustments.GetAnnualMaxSegmentIndex(discountPlanSub.DateEffective, discountPlanSub.DateTerm, listProcedures[i].ProcDate);
            double discountPlanAmt = 0;
            if (annualBucketIndex != -1 && listAnnualTots.Count > annualBucketIndex)
            {
                discountPlanAmt = Procedures.GetDiscountAmountForDiscountPlanAndValidate(listProcedures[i], discountPlanSub, discountPlan, listAnnualTots[annualBucketIndex], listProceduresHist);
                listAnnualTots[annualBucketIndex] += discountPlanAmt;
            }

            listDiscountPlanProcs.Add(new DiscountPlanProc
            {
                DiscountPlanAmt = discountPlanAmt,
                doesExceedAnnualMax = Procedures.ExceedsAnnualMax,
                doesExceedFreqLimit = Procedures.ExceedsFreqLimitation,
                ProcNum = listProcedures[i].ProcNum
            });
            listProceduresHist.Add(listProcedures[i]);
        }

        return listDiscountPlanProcs;
    }

    /// <summary>
    ///     Returns a DiscountPlanProc object for every procedure passed in. It is assumed that all procedures are for the same
    ///     patient. Passing in an empty list of Adjustments will assume
    ///     no prior adjustments of the discountPlan.DefNum had been applied.
    /// </summary>
    public static List<DiscountPlanProc> GetDiscountPlanProcEstimate(List<Procedure> listProcedures, long patNum, DateTime dateEffective, DateTime dateTerm, DiscountPlan discountPlan = null, List<Adjustment> listAdjustments = null)
    {
        if (listProcedures.IsNullOrEmpty() || discountPlan == null) return new List<DiscountPlanProc>();

        var listDiscountPlanProcs = new List<DiscountPlanProc>();
        var listProceduresHist = new List<Procedure>();
        var listAnnualTots = Adjustments.GetAnnualTotalsForPatByDiscountPlan(patNum, dateEffective, dateTerm, discountPlan, listProcedures.Max(x => x.ProcDate), listAdjustments);
        for (var i = 0; i < listProcedures.Count; i++)
        {
            var annualBucketIndex = Adjustments.GetAnnualMaxSegmentIndex(dateEffective, dateTerm, listProcedures[i].ProcDate);
            double discountPlanAmt = 0;
            if (annualBucketIndex != -1 && listAnnualTots.Count > annualBucketIndex)
            {
                discountPlanAmt = Procedures.GetDiscountAmountForDiscountPlanEstimate(listProcedures[i], patNum, dateEffective, dateTerm, discountPlan, listAnnualTots[annualBucketIndex], listProceduresHist);
                listAnnualTots[annualBucketIndex] += discountPlanAmt;
            }

            listDiscountPlanProcs.Add(new DiscountPlanProc
            {
                DiscountPlanAmt = discountPlanAmt,
                doesExceedAnnualMax = Procedures.ExceedsAnnualMax,
                doesExceedFreqLimit = Procedures.ExceedsFreqLimitation,
                ProcNum = listProcedures[i].ProcNum
            });
            listProceduresHist.Add(listProcedures[i]);
        }

        return listDiscountPlanProcs;
    }

    #endregion

    #region Methods - Update

    
    public static void Update(DiscountPlan discountPlan)
    {
        DiscountPlanCrud.Update(discountPlan);
    }

    /// <summary>
    ///     Changes the DiscountPlanNum of all discountplansub that have _planFrom.DiscountPlanNum to
    ///     _planInto.DiscountPlanNum
    /// </summary>
    public static void MergeTwoPlans(DiscountPlan discountPlanInto, DiscountPlan discountPlanFrom)
    {
        var command = "UPDATE discountplansub SET DiscountPlanNum=" + SOut.Long(discountPlanInto.DiscountPlanNum)
                                                                    + " WHERE DiscountPlanNum=" + SOut.Long(discountPlanFrom.DiscountPlanNum);
        Db.NonQ(command);
        //Delete the discount plan from the database.
        DiscountPlanCrud.Delete(discountPlanFrom.DiscountPlanNum);
    }

    #endregion

    #region Methods - Misc

    /// <summary>
    ///     Checks for frequency conflicts with the passed-in list of procedures.
    ///     Returns empty string if there are no conflicts, new line delimited list of proc codes if there are ANY frequency
    ///     limitations exceeded.
    ///     Optionally add procedures to the list of historic procedures. Throws exceptions. listProcs should not contain any
    ///     completed procs,
    ///     as those get pulled from the db.
    /// </summary>
    public static string CheckDiscountFrequencyAndValidateDiscountPlanSub(List<Procedure> listProcedures, long patNum, DateTime aptDateTime, DiscountPlanSub discountPlanSub = null,
        List<Procedure> listProceduresAddHist = null)
    {
        //Note: If the passed in list contains procs in categories that have already exceeded limits, then this method will
        //return 0;
        var patient = Patients.GetPat(patNum);
        if (patient == null) throw new ArgumentException("Patient not found in database.", nameof(patNum));
        if (aptDateTime == null) throw new ArgumentException("Appointment Date not present.", nameof(aptDateTime));
        if (discountPlanSub == null) discountPlanSub = DiscountPlanSubs.GetSubForPat(patient.PatNum);
        //if this patient is a subscriber to the discount plan.
        if (discountPlanSub == null) return "";
        var discountPlan = GetPlan(discountPlanSub.DiscountPlanNum);
        if (discountPlan == null) return "";
        return CheckDiscountFrequency(listProcedures, patient.PatNum, aptDateTime, discountPlanSub.DateEffective, discountPlanSub.DateTerm, discountPlan, listProceduresAddHist);
    }

    /// <summary>
    ///     Checks for frequency conflicts with the passed-in list of procedures.
    ///     Returns empty string if there are no conflicts, new line delimited list of proc codes if there are ANY frequency
    ///     limitations exceeded.
    ///     Optionally add procedures to the list of historic procedures. Throws exceptions. listProcs should not contain any
    ///     completed procs,
    ///     as those get pulled from the db.
    /// </summary>
    public static string CheckDiscountFrequency(List<Procedure> listProcedures, long patNum, DateTime aptDateTime, DateTime dateEffective, DateTime dateTerm, DiscountPlan discountPlan,
        List<Procedure> listProceduresAddHist = null)
    {
        //if aptDateTime out of Discount plan range, bounce outta this bad boyo with no conflict/
        if (aptDateTime < dateEffective || (dateTerm.Year >= 1880 && aptDateTime > dateTerm)) return "";
        if (listProcedures.IsNullOrEmpty()) return "";
        //get completed procs for date range of discount plan
        var termDate = dateTerm;
        if (termDate == DateTime.MinValue) termDate = DateTime.MaxValue;
        var listProceduresSorted = Procedures.SortListByTreatPlanPriority(listProcedures).ToList();
        listProceduresSorted.RemoveAll(x => x.ProcStatus == ProcStat.C);
        listProceduresSorted.InsertRange(0, listProcedures.FindAll(x => x.ProcStatus == ProcStat.C));
        var listProceduresHist = Procedures.GetCompletedForDateRange(dateEffective, termDate, listPatNums: ListTools.FromSingle(patNum));
        if (listProceduresAddHist != null) listProceduresHist.AddRange(listProceduresAddHist);
        listProceduresHist.RemoveAll(x => listProcedures.Select(y => y.ProcNum).ToList().Contains(x.ProcNum));
        //get multiple lists of ProcCodes for each catagory of frequency limit dcodes
        var listProcedureCodesExam = GetProcedureCodesByPref(PrefName.DiscountPlanExamCodes);
        var listProcedureCodesXray = GetProcedureCodesByPref(PrefName.DiscountPlanXrayCodes);
        var listProcedureCodesProphy = GetProcedureCodesByPref(PrefName.DiscountPlanProphyCodes);
        var listProcedureCodesFluoride = GetProcedureCodesByPref(PrefName.DiscountPlanFluorideCodes);
        var listProcedureCodesPerio = GetProcedureCodesByPref(PrefName.DiscountPlanPerioCodes);
        var listProcedureCodesLimited = GetProcedureCodesByPref(PrefName.DiscountPlanLimitedCodes);
        var listPACodes = GetProcedureCodesByPref(PrefName.DiscountPlanPACodes);
        var listCodeNumsAll = new List<long>();
        //Create a master list to reference the ProcCodes of the Procedurelogs against
        listCodeNumsAll.AddRange(listProcedureCodesExam.Select(x => x.CodeNum).ToList());
        listCodeNumsAll.AddRange(listProcedureCodesXray.Select(x => x.CodeNum).ToList());
        listCodeNumsAll.AddRange(listProcedureCodesProphy.Select(x => x.CodeNum).ToList());
        listCodeNumsAll.AddRange(listProcedureCodesFluoride.Select(x => x.CodeNum).ToList());
        listCodeNumsAll.AddRange(listProcedureCodesPerio.Select(x => x.CodeNum).ToList());
        listCodeNumsAll.AddRange(listProcedureCodesLimited.Select(x => x.CodeNum).ToList());
        listCodeNumsAll.AddRange(listPACodes.Select(x => x.CodeNum).ToList());
        var frequencyConflicts = "";
        //Iterates over the passed in list of procedures, and checks if adding them will exceed frequency limitations.
        for (var i = 0; i < listProceduresSorted.Count; i++)
        {
            if (!listCodeNumsAll.Contains(listProceduresSorted[i].CodeNum)) continue;
            var frequencyConflict = HasMetDiscountFrequencyLimitation(listProceduresSorted[i], discountPlan, dateEffective, dateTerm, listProceduresHist, listProcedureCodesExam, listProcedureCodesXray, listProcedureCodesProphy,
                listProcedureCodesFluoride, listProcedureCodesPerio, listProcedureCodesLimited, listPACodes);
            if (!frequencyConflict.IsNullOrEmpty()) frequencyConflicts += frequencyConflict + "\r\n";
            listProceduresHist.Add(listProceduresSorted[i]);
        }

        //for each list, if the number of procedurelogs+1 > frequency limit for that catagory bounce with a conflict that includes the Proccode
        return frequencyConflicts;
    }

    ///<summary>Returns an empty string if there is no limitation met for the given proc, otherwise the CodeNum is returned.</summary>
    public static string HasMetDiscountFrequencyLimitation(Procedure procedure, DiscountPlan discountPlan, DateTime dateEffective, DateTime dateTerm,
        List<Procedure> listProceduresHist, List<ProcedureCode> listExamCodes, List<ProcedureCode> listXrayCodes, List<ProcedureCode> listProphyCodes,
        List<ProcedureCode> listFluorideCodes, List<ProcedureCode> listPerioCodes, List<ProcedureCode> listLimitedExamCodes, List<ProcedureCode> listPACodes)
    {
        if (HasMetFrequencyLimitForCategory(procedure, listProceduresHist, listExamCodes, discountPlan.ExamFreqLimit, dateEffective, dateTerm)
            || HasMetFrequencyLimitForCategory(procedure, listProceduresHist, listXrayCodes, discountPlan.XrayFreqLimit, dateEffective, dateTerm)
            || HasMetFrequencyLimitForCategory(procedure, listProceduresHist, listProphyCodes, discountPlan.ProphyFreqLimit, dateEffective, dateTerm)
            || HasMetFrequencyLimitForCategory(procedure, listProceduresHist, listFluorideCodes, discountPlan.FluorideFreqLimit, dateEffective, dateTerm)
            || HasMetFrequencyLimitForCategory(procedure, listProceduresHist, listPerioCodes, discountPlan.PerioFreqLimit, dateEffective, dateTerm)
            || HasMetFrequencyLimitForCategory(procedure, listProceduresHist, listLimitedExamCodes, discountPlan.LimitedExamFreqLimit, dateEffective, dateTerm)
            || HasMetFrequencyLimitForCategory(procedure, listProceduresHist, listPACodes, discountPlan.PAFreqLimit, dateEffective, dateTerm)
           )
            return ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode;
        return "";
    }

    /// <summary>
    ///     Returns false when procCode is not in listCodesForCategory, or when procCode would not exceed the
    ///     frequencyLimit for the passed in listCodesForCategory. Returns true otherwise
    /// </summary>
    public static bool HasMetFrequencyLimitForCategory(Procedure procedure, List<Procedure> listProceduresHist, List<ProcedureCode> listCodesForCategory, int frequencyLimit, DateTime dateEffective, DateTime dateTerm)
    {
        if (frequencyLimit == -1 || !listCodesForCategory.Any(x => x.CodeNum == procedure.CodeNum)) return false;
        dateEffective = DiscountPlanSubs.GetAnnualMaxDateEffective(dateEffective);
        dateTerm = DiscountPlanSubs.GetAnnualMaxDateTerm(dateTerm);
        if (procedure.ProcDate < dateEffective || procedure.ProcDate > dateTerm) return false;
        var dateEffectiveFinal = DiscountPlanSubs.GetDateEffectiveForAnnualDateRangeSegment(procedure.ProcDate, dateEffective, dateTerm);
        var dateTermFinal = DiscountPlanSubs.GetDateTermForAnnualDateRangeSegment(procedure.ProcDate, dateEffective, dateTerm);
        var count = listProceduresHist.Count(x => listCodesForCategory.Select(y => y.CodeNum).Contains(x.CodeNum) && x.ProcDate >= dateEffectiveFinal && x.ProcDate <= dateTermFinal);
        return count >= frequencyLimit;
    }

    #endregion
}

/// <summary>
///     Helper class that holds information regarding a specific procedure and its relation to discount plan related
///     things.
/// </summary>
[Serializable]
public class DiscountPlanProc
{
	/// <summary>
	///     The discounted amount for the procedure that will be turned into an adjustment once the procedure is
	///     completed.
	/// </summary>
	public double DiscountPlanAmt;

	/// <summary>
	///     Set to true when the total sum of this procedure and previously tallied procedures exceeds the annual max.
	///     Otherwise false.
	/// </summary>
	public bool doesExceedAnnualMax;

	/// <summary>
	///     Set to true when the amount of previously tallied procedures have already met the frequency limitation for
	///     this procedure's code. Otherwise false.
	/// </summary>
	public bool doesExceedFreqLimit;

    public long ProcNum;

    ///<summary>For serialization purposes.</summary>
    public DiscountPlanProc()
    {
    }

    ///<summary>Only sets the ProcNum and DiscountPlanAmt fields.</summary>
    public DiscountPlanProc(Procedure procedure)
    {
        ProcNum = procedure.ProcNum;
        DiscountPlanAmt = procedure.DiscountPlanAmt;
    }
}