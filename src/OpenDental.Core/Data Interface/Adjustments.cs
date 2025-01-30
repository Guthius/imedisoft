using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class Adjustments
{
    public static void Update(Adjustment adjustment)
    {
        AdjustmentCrud.Update(adjustment);
    }

    public class ChargeUndoData
    {
        public int CountDeletedAdjustments;
        public List<long> ListSkippedPatNums = [];
    }

    public static List<Adjustment> GetMany(List<long> adjNums)
    {
        return adjNums.IsNullOrEmpty() ? [] : AdjustmentCrud.SelectMany($"SELECT * FROM adjustment WHERE adjustment.AdjNum IN ({string.Join(",", adjNums)})");
    }

    public static Adjustment[] Refresh(long patNum)
    {
        return AdjustmentCrud
            .SelectMany("SELECT * FROM adjustment WHERE PatNum = " + patNum + " ORDER BY AdjDate")
            .ToArray();
    }

    public static List<Adjustment> GetPatientData(long patNum)
    {
        return AdjustmentCrud.SelectMany("SELECT * FROM adjustment WHERE PatNum = " + patNum + " ORDER BY AdjDate");
    }

    public static Adjustment GetOne(long adjNum)
    {
        return AdjustmentCrud.SelectOne(adjNum);
    }

    public static double GetAmtAllocated(long adjNum, long payNumExcluded, List<PaySplit> paySplits = null)
    {
        if (paySplits != null)
        {
            return paySplits.FindAll(x => x.PayNum != payNumExcluded).Sum(x => x.SplitAmt);
        }

        var command = "SELECT SUM(SplitAmt) FROM paysplit WHERE AdjNum=" + adjNum;
        if (payNumExcluded != 0)
        {
            command += " AND PayNum!=" + payNumExcluded;
        }

        return SIn.Double(DataCore.GetScalar(command));
    }

    public static List<Adjustment> GetAdjustForPats(List<long> patNums)
    {
        return AdjustmentCrud.SelectMany("SELECT * FROM adjustment WHERE PatNum IN (" + string.Join(", ", patNums) + ")");
    }

    public static List<Adjustment> GetForProc(long procNum, IEnumerable<Adjustment> adjustments)
    {
        return adjustments.Where(adjustment => adjustment.ProcNum == procNum).ToList();
    }

    public static List<Adjustment> GetListForProc(long procNum)
    {
        return AdjustmentCrud.SelectMany("SELECT * FROM adjustment WHERE ProcNum=" + procNum);
    }

    public static List<Adjustment> GetForProcs(List<long> procNums)
    {
        if (procNums == null || procNums.Count < 1)
        {
            return [];
        }

        return AdjustmentCrud.SelectMany("SELECT * FROM adjustment WHERE ProcNum IN(" + string.Join(",", procNums) + ")");
    }

    public static double GetTotForProc(long procNum, bool canIncludeTax = true)
    {
        var procNums = new List<long> {procNum};

        return GetTotForProcs(procNums, canIncludeTax);
    }

    public static double GetTotForProcs(List<long> procNums, bool canIncludeTax = true)
    {
        if (procNums.IsNullOrEmpty())
        {
            return 0;
        }

        return SIn.Double(DataCore.GetScalar("SELECT SUM(AdjAmt) FROM adjustment WHERE ProcNum IN(" + string.Join(",", procNums) + ")"));
    }

    public static List<Adjustment> GetAdjustForPatByType(long patNum, long adjType)
    {
        return AdjustmentCrud.SelectMany("SELECT * FROM adjustment WHERE PatNum=" + patNum + " AND AdjType=" + adjType);
    }

    public static List<Adjustment> GetAdjustForPatsByType(List<long> patNums, long adjType, DateTime dateAdjMax)
    {
        if (patNums == null || patNums.Count == 0)
        {
            return [];
        }

        var queryBrokenApts =
            "SELECT * FROM adjustment " +
            "WHERE PatNum IN (" + string.Join(",", patNums) + ") " +
            "AND AdjType=" + adjType + " " +
            "AND " + DbHelper.DateTConditionColumn("AdjDate", ConditionOperator.LessThan, dateAdjMax);

        return AdjustmentCrud.SelectMany(queryBrokenApts);
    }

    public static double GetTotForProc(long procNum, Adjustment[] adjustments, long excludedNum = 0)
    {
        return adjustments
            .Where(adjustment => adjustment.AdjNum != excludedNum)
            .Where(adjustment => adjustment.ProcNum == procNum)
            .Select(adjustment => adjustment.AdjAmt)
            .Sum();
    }

    public static List<Adjustment> GetForDateRange(DateTime dateStart, DateTime dateEnd, List<long> patNums = null, long adjType = -1, bool useProcDate = false)
    {
        if (dateEnd < dateStart) return [];
        
        var dateColumn = "AdjDate";
        if (useProcDate)
        {
            dateColumn = "ProcDate";
        }
        
        var command = $"SELECT * FROM adjustment WHERE {DbHelper.BetweenDates(dateColumn, dateStart, dateEnd)} ";
        if (!patNums.IsNullOrEmpty())
        {
            command += $"AND PatNum IN ({string.Join(",", patNums)}) ";
        }
        
        if (adjType != -1)
        {
            command += $"AND AdjType={adjType} ";
        }
        
        return AdjustmentCrud.SelectMany(command);
    }

    public static double GetTotForPatByType(long patNum, long defNumAdjustmentType, DateTime dateStart, DateTime dateEnd, long procNumExclude = 0)
    {
        double total = 0;
        var listHistAdjustment = GetForDateRange(dateStart, dateEnd, ListTools.FromSingle(patNum), defNumAdjustmentType, true);
        var listAdjustmentsToSum = listHistAdjustment.FindAll(x => x.AdjAmt < 0 && x.ProcNum > 0 && x.ProcNum != procNumExclude);
        total = Math.Abs(listAdjustmentsToSum.Sum(x => x.AdjAmt));
        return total;
    }

    public static List<double> GetAnnualTotalsForPatByDiscountPlan(long patNum, DateTime dateStart, DateTime dateStop, DiscountPlan discountPlan, DateTime dateProcMax, List<Adjustment> listAdjustments = null)
    {
        var listAnnualRunningTotals = new List<double>();
        if (dateProcMax.Year < 1880 && listAdjustments.IsNullOrEmpty()) return listAnnualRunningTotals;
        var startDate = DiscountPlanSubs.GetAnnualMaxDateEffective(dateStart);
        var endDate = DiscountPlanSubs.GetAnnualMaxDateTerm(dateStop);
        return GetAnnualTotalsForPatByDiscountPlanHelper(patNum, discountPlan, dateProcMax, listAdjustments, startDate, endDate);
    }

    public static List<double> GetAnnualTotalsForPatByDiscountPlanHelper(long patNum, DiscountPlan discountPlan, DateTime maxProcDate, List<Adjustment> listAdjustments, DateTime startDate, DateTime endDate)
    {
        var dateUpperBound = endDate; //Tracks the end of the discount plan.
        if (listAdjustments == null) listAdjustments = GetForDateRange(startDate, dateUpperBound, ListTools.FromSingle(patNum));
        var listAnnualRunningTotals = new List<double>();
        if (dateUpperBound == DateTime.MaxValue) dateUpperBound = maxProcDate;
        if (dateUpperBound < startDate) return listAnnualRunningTotals;
        var annualSegments = GetDifferenceNumberOfYears(startDate, dateUpperBound);
        for (var i = 0; i < annualSegments; i++) listAnnualRunningTotals.Add(0);
        for (var i = 0; i < listAdjustments.Count; i++)
        {
            var index = GetAnnualMaxSegmentIndex(startDate, endDate, listAdjustments[i].ProcDate); //Use original endDate
            if (listAdjustments[i].PatNum != patNum
                || listAdjustments[i].AdjType != discountPlan.DefNum
                || listAdjustments[i].AdjAmt > 0
                || index == -1
                || index >= listAnnualRunningTotals.Count)
                continue;
            listAnnualRunningTotals[index] += Math.Abs(listAdjustments[i].AdjAmt);
        }

        return listAnnualRunningTotals;
    }

    public static int GetDifferenceNumberOfYears(DateTime startDate, DateTime endDate)
    {
        var years = 0;

        var current = startDate;

        while (current <= endDate)
        {
            years++;

            current = current.AddYears(1);
        }

        return years;
    }

    public static int GetAnnualMaxSegmentIndex(DateTime dateStart, DateTime dateStop, DateTime dateIndex)
    {
        if (dateIndex == DateTime.MinValue) return -1;
        var startDate = DiscountPlanSubs.GetAnnualMaxDateEffective(dateStart);
        var endDate = DiscountPlanSubs.GetAnnualMaxDateTerm(dateStop);
        if (dateIndex < startDate || dateIndex > endDate) return -1;
        var index = GetDifferenceNumberOfYears(startDate, dateIndex);
        if (index > 0) index -= 1; //Index is 0 based.
        return index;
    }

    public static long Insert(Adjustment adjustment)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        adjustment.SecUserNumEntry = Security.CurUser.UserNum;
        var adjNum = AdjustmentCrud.Insert(adjustment);
        return adjNum;
    }

    public static void CreateAdjustmentForDiscount(Procedure procedure)
    {
        var adjustmentDiscount = new Adjustment();
        adjustmentDiscount.DateEntry = DateTime.Today;
        adjustmentDiscount.AdjDate = DateTime.Today;
        adjustmentDiscount.ProcDate = procedure.ProcDate;
        adjustmentDiscount.ProvNum = procedure.ProvNum;
        adjustmentDiscount.PatNum = procedure.PatNum;
        adjustmentDiscount.AdjType = PrefC.GetLong(PrefName.TreatPlanDiscountAdjustmentType);
        adjustmentDiscount.ClinicNum = procedure.ClinicNum;
        adjustmentDiscount.AdjAmt = -procedure.Discount; //Discount must be negative here.
        adjustmentDiscount.ProcNum = procedure.ProcNum;
        Insert(adjustmentDiscount);
        TsiTransLogs.CheckAndInsertLogsIfAdjTypeExcluded(adjustmentDiscount);
    }

    public static double CreateAdjustmentForDiscountPlan(Procedure procedure)
    {
        var discountPlanSub = DiscountPlanSubs.GetSubForPat(procedure.PatNum);
        if (discountPlanSub == null) return 0;
        var discountPlan = DiscountPlans.GetPlan(discountPlanSub.DiscountPlanNum);
        if (discountPlan == null) //Patient doesn't have a discountPlan
            return 0;
        var adjAmt = Procedures.GetDiscountAmountForDiscountPlanAndValidate(procedure, discountPlanSub, discountPlan);
        if (adjAmt <= 0) return 0;
        var adjustmentDiscountPlan = new Adjustment();
        adjustmentDiscountPlan.DateEntry = DateTime.Today;
        adjustmentDiscountPlan.AdjDate = DateTime.Today;
        adjustmentDiscountPlan.ProcDate = procedure.ProcDate;
        adjustmentDiscountPlan.ProvNum = procedure.ProvNum;
        adjustmentDiscountPlan.PatNum = procedure.PatNum;
        adjustmentDiscountPlan.AdjType = discountPlan.DefNum;
        adjustmentDiscountPlan.ClinicNum = procedure.ClinicNum;
        adjustmentDiscountPlan.AdjAmt = -adjAmt;
        adjustmentDiscountPlan.ProcNum = procedure.ProcNum;
        Insert(adjustmentDiscountPlan);
        TsiTransLogs.CheckAndInsertLogsIfAdjTypeExcluded(adjustmentDiscountPlan);
        SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentCreate, procedure.PatNum, "Adjustment made for discount plan: " + adjustmentDiscountPlan.AdjAmt.ToString("f"));
        Procedures.UpdateDiscountPlanAmt(procedure.ProcNum, adjAmt);
        return adjAmt;
    }

    public static void CreateAdjustmentForSalesTax(Procedure procedure, bool canSkipIsTaxed = false)
    {
        if (!canSkipIsTaxed && !ProcedureCodes.GetProcCode(procedure.CodeNum).IsTaxed) return;
        var listClaimProcs = ClaimProcs.RefreshForProc(procedure.ProcNum);
        long provNum;
        //Always use the default global provider if set.
        var salesTaxDefaultProvNum = PrefC.GetLong(PrefName.SalesTaxDefaultProvider);
        if (salesTaxDefaultProvNum != 0)
        {
            //Note: To have clinics override the SalesTaxDefaultProvider, add clinic code here.
            //This can be done in a future job.
            provNum = salesTaxDefaultProvNum;
        }
        else
        {
            provNum = PrefC.GetLong(PrefName.PracticeDefaultProv);
            var clinic = Clinics.GetClinic(procedure.ClinicNum);
            if (procedure.ClinicNum != 0 && clinic.DefaultProviderId.HasValue) provNum = clinic.DefaultProviderId.Value;
        }

        var adjustmentSalesTax = new Adjustment();
        adjustmentSalesTax.AdjDate = DateTime.Today;
        adjustmentSalesTax.ProcDate = procedure.ProcDate;
        adjustmentSalesTax.PatNum = procedure.PatNum;
        adjustmentSalesTax.ClinicNum = procedure.ClinicNum;
        adjustmentSalesTax.AdjAmt = ClaimProcs.ComputeSalesTax(procedure, listClaimProcs, false);
        adjustmentSalesTax.AdjType = PrefC.GetLong(PrefName.SalesTaxAdjustmentType);
        adjustmentSalesTax.ProcNum = procedure.ProcNum;
        adjustmentSalesTax.ProvNum = provNum;
        Insert(adjustmentSalesTax);
        TsiTransLogs.CheckAndInsertLogsIfAdjTypeExcluded(adjustmentSalesTax);
        SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentCreate, procedure.PatNum, "Adjustment made for sales tax: " + adjustmentSalesTax.AdjAmt);
    }

    public static long CreateLateChargeAdjustment(double lateChargeAmount, DateTime dateCharge, long provNum, long patNum, DateTime dateStatementSent, DateTime dateMaxUpdateStmtProd, List<long> listProcNums, List<long> listAdjNums, List<long> listPayPlanChargeNums)
    {
        var adjustmentLateCharge = new Adjustment();
        adjustmentLateCharge.AdjDate = dateCharge;
        adjustmentLateCharge.PatNum = patNum;
        adjustmentLateCharge.AdjAmt = lateChargeAmount;
        adjustmentLateCharge.AdjType = PrefC.GetLong(PrefName.LateChargeAdjustmentType);
        adjustmentLateCharge.ProvNum = provNum;
        adjustmentLateCharge.AdjNote = Lans.g("Adjustment", "Late charge for statement sent on") + $" {dateStatementSent.ToShortDateString()}.";
        TsiTransLogs.CheckAndInsertLogsIfAdjTypeExcluded(adjustmentLateCharge);
        SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentCreate, patNum,
            $"Adjustment for late charge for statement sent on {dateStatementSent.ToShortDateString()}, Amount: {adjustmentLateCharge.AdjAmt.ToString("c")}");
        var adjNum = Insert(adjustmentLateCharge);
        StatementProds.UpdateLateChargeAdjNumForMany(adjNum, listProcNums, listAdjNums, listPayPlanChargeNums, dateMaxUpdateStmtProd);
        return adjNum;
    }

    public static void Delete(Adjustment adjustment)
    {
        AdjustmentCrud.Delete(adjustment.AdjNum);
        PaySplits.UnlinkForAdjust(adjustment);
        StatementProds.UpdateLateChargeAdjNumForMany(0, adjustment.AdjNum);
    }

    public static void DeleteForProcedure(long procNum)
    {
        //Create log for each adjustment that is going to be deleted.
        var command = "SELECT * FROM adjustment WHERE ProcNum = " + procNum; //query for all adjustments of a procedure 
        var listAdjustments = AdjustmentCrud.SelectMany(command);
        var listAdjNums = new List<long>();
        for (var i = 0; i < listAdjustments.Count; i++)
        {
            //loops through the rows
            listAdjNums.Add(listAdjustments[i].AdjNum);
            SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentEdit, listAdjustments[i].PatNum, //and creates audit trail entry for every row to be deleted
                "Delete adjustment for patient: "
                + Patients.GetLim(listAdjustments[i].PatNum).GetNameLF() + ", "
                + listAdjustments[i].AdjAmt.ToString("c"), 0, listAdjustments[i].SecDateTEdit);
        }

        //Delete each adjustment for the procedure.
        command = "DELETE FROM adjustment WHERE ProcNum = " + procNum;
        Db.NonQ(command);
        //Late charge adjustments aren't normally attached to procedures, but it is possible for users to attach a procedure to them after they are
        //made, so we must update any StatementProds that might be associated to the deleted adjustment.
        StatementProds.UpdateLateChargeAdjNumForMany(0, listAdjNums.ToArray());
    }

    public static void AddSalesTaxIfNoneExists(Procedure procedure)
    {
        //Don't apply if there's an ortho proc link
        if (OrthoProcLinks.GetByProcNum(procedure.ProcNum) != null) return;
        //Check if there's already an existing sales tax adjustment
        var salesTaxAdjType = PrefC.GetLong(PrefName.SalesTaxAdjustmentType);
        if (GetListForProc(procedure.ProcNum).Any(x => x.AdjType == salesTaxAdjType)) return;
        //Add sales tax.
        CreateAdjustmentForSalesTax(procedure);
    }

    public static ChargeUndoData UndoFinanceOrBillingCharges(DateTime dateUndo, bool isBillingCharges)
    {
        var adjTypeStr = "Finance";
        var adjTypeDefNum = PrefC.GetLong(PrefName.FinanceChargeAdjustmentType);
        if (isBillingCharges)
        {
            adjTypeStr = "Billing";
            adjTypeDefNum = PrefC.GetLong(PrefName.BillingChargeAdjustmentType);
        }

        var command = "SELECT adjustment.AdjNum,adjustment.AdjAmt,patient.PatNum"
                      + ",patient.LName,patient.FName,patient.Preferred,patient.MiddleI,adjustment.SecDateTEdit"
                      + ",(CASE WHEN paysplit.SplitNum IS NULL THEN 0 ELSE 1 END) AS 'HasPaySplits' "
                      + ",(CASE WHEN payplanlink.PayPlanLinkNum IS NULL THEN 0 ELSE 1 END) AS 'HasPayPlan' "
                      + "FROM adjustment "
                      + "INNER JOIN patient ON patient.PatNum=adjustment.PatNum "
                      + "LEFT JOIN paysplit ON adjustment.AdjNum=paysplit.AdjNum "
                      + $"LEFT JOIN payplanlink ON adjustment.AdjNum=payplanlink.FKey AND payplanlink.LinkType={SOut.Enum(PayPlanLinkType.Adjustment)} "
                      + "WHERE AdjDate=" + SOut.Date(dateUndo) + " "
                      + "AND AdjType=" + adjTypeDefNum + " "
                      + "GROUP BY adjustment.AdjNum";
        var table = DataCore.GetTable(command);
        var chargeUndoData = new ChargeUndoData();
        for (var i = table.Rows.Count - 1; i >= 0; i--)
        {
            var dataRow = table.Rows[i];
            //Any rows for adjustments that have pay splits attached will not be deleted. Add them to the skipped list and remove them from the table.
            if (SIn.Bool(dataRow["HasPaySplits"].ToString()) == false && SIn.Bool(dataRow["HasPayPlan"].ToString()) == false) continue;
            chargeUndoData.ListSkippedPatNums.Add(SIn.Long(dataRow["PatNum"].ToString()));
            table.Rows.RemoveAt(i);
        }

        var listAdjNumsToDelete = new List<long>();
        var listActions = new List<Action>();
        var loopCount = 0;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            //loops through the remaining rows and creates an audit trail entry for every row to be deleted
            var rowCur = table.Rows[i];
            listAdjNumsToDelete.Add(SIn.Long(rowCur["AdjNum"].ToString()));
            var actionCreateAuditTrailEntry = () =>
            {
                SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentEdit, SIn.Long(rowCur["PatNum"].ToString()), "Delete adjustment for patient, undo " + adjTypeStr.ToLower() + " charges: " + Patients.GetNameLF(rowCur["LName"].ToString(), rowCur["FName"].ToString(), rowCur["Preferred"].ToString(), rowCur["MiddleI"].ToString()) + ", " + SIn.Double(rowCur["AdjAmt"].ToString()).ToString("c"), 0, SIn.DateTime(rowCur["SecDateTEdit"].ToString()));
                if (++loopCount % 5 == 0) 
                {
                    ODEvent.Fire(ODEventType.ProgressBar, "Creating log entries for " + adjTypeStr.ToLower() + " charges " + loopCount + " out of " + table.Rows.Count);
                }
            };
            listActions.Add(actionCreateAuditTrailEntry);
        }

        ODThread.RunParallel(listActions, TimeSpan.FromMinutes(2));
        
        ODEvent.Fire(ODEventType.ProgressBar, "Deleting " + table.Rows.Count + " " + adjTypeStr.ToLower() + " charge adjustments...");
        
        AdjustmentCrud.DeleteMany(listAdjNumsToDelete);
        
        StatementProds.UpdateLateChargeAdjNumForMany(0, listAdjNumsToDelete.ToArray());
        chargeUndoData.CountDeletedAdjustments = listAdjNumsToDelete.Count;
        
        return chargeUndoData;
    }

    public static ChargeUndoData UndoLateCharges(DateTime dateUndo)
    {
        var command =
            $"""
             SELECT adjustment.AdjNum,adjustment.AdjAmt,adjustment.PatNum,(CASE WHEN paysplit.SplitNum IS NULL THEN 0 ELSE 1 END) AS 'HasPaySplits', 
             (CASE WHEN payplanlink.PayPlanLinkNum IS NULL THEN 0 ELSE 1 END) AS 'HasPayPlan'
             FROM adjustment
             INNER JOIN statementprod ON adjustment.AdjNum=statementprod.LateChargeAdjNum
             LEFT JOIN paysplit ON adjustment.AdjNum=paysplit.AdjNum
             LEFT JOIN payplanlink ON adjustment.AdjNum=payplanlink.FKey AND payplanlink.LinkType={SOut.Enum(PayPlanLinkType.Adjustment)}
             WHERE adjustment.AdjDate={SOut.Date(dateUndo)} 
             GROUP BY adjustment.AdjNum
             """;

        var table = DataCore.GetTable(command);
        var chargeUndoDataLate = new ChargeUndoData();
        var listAdjNumsDeleted = new List<long>();

        for (var i = 0; i < table.Rows.Count; i++)
        {
            var dataRow = table.Rows[i];
            if (SIn.Bool(dataRow["HasPaySplits"].ToString()) || SIn.Bool(dataRow["HasPayPlan"].ToString()))
            {
                chargeUndoDataLate.ListSkippedPatNums.Add(SIn.Long(dataRow["PatNum"].ToString()));
            }
            else
            {
                listAdjNumsDeleted.Add(SIn.Long(dataRow["AdjNum"].ToString()));
                SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentEdit, SIn.Long(dataRow["PatNum"].ToString()), $"Late charges dated {dateUndo.ToShortDateString()} undone, Adjustment deleted, Amount: " + $"{SIn.Decimal(dataRow["AdjAmt"].ToString()):c}");
                AdjustmentCrud.Delete(SIn.Long(dataRow["AdjNum"].ToString()));
                StatementProds.UpdateLateChargeAdjNumForMany(0, SIn.Long(dataRow["AdjNum"].ToString()));
            }
        }

        chargeUndoDataLate.CountDeletedAdjustments = listAdjNumsDeleted.Count;
        return chargeUndoDataLate;
    }

    public static string GetQueryAdjustmentsForAppointments(DateTime dateStart, DateTime dateEnd, List<long> operatoryNums, bool doGetSum)
    {
        if (operatoryNums.IsNullOrEmpty())
        {
            return
                "SELECT " + (doGetSum ? "SUM(adjustment.AdjAmt) " : "* ") +
                "FROM adjustment " +
                "WHERE AdjDate BETWEEN " + SOut.Date(dateStart) + " " +
                "AND " + SOut.Date(dateEnd) + " ";
        }

        return
            "SELECT " + (doGetSum ? "SUM(adjustment.AdjAmt) " : "* ") +
            "FROM adjustment WHERE AdjDate BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " " +
            "AND PatNum IN(" +
            "SELECT PatNum FROM appointment " +
            "WHERE AptDateTime BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd.AddDays(1)) +
            "AND AptStatus IN (" +
            (int) ApptStatus.Scheduled + ", " +
            (int) ApptStatus.Complete + ", " +
            (int) ApptStatus.Broken + ", " +
            (int) ApptStatus.PtNote + ", " +
            (int) ApptStatus.PtNoteCompleted + ")" + " " +
            "AND Op IN (" + string.Join(",", operatoryNums) + ")) ";
    }

    public static List<Adjustment> CreateNegativeAdjustmentsForRefund(Payment paymentExisting)
    {
        var existingPaySplits = PaySplits.GetForPayment(paymentExisting.PayNum);
        var def = Defs.GetDef(DefCat.AdjTypes, PrefC.GetLong(PrefName.RefundAdjustmentType));
        var adjustmentsAdded = new List<Adjustment>();

        foreach (var paySplit in existingPaySplits)
        {
            if (paySplit.IsUnallocated || paySplit.PayPlanNum > 0 || paySplit.PayPlanChargeNum > 0 || paySplit.UnearnedType > 0)
            {
                continue;
            }

            adjustmentsAdded.Add(new Adjustment
            {
                IsNew = true,
                DateEntry = DateTime.Today,
                AdjDate = DateTime.Today,
                ProcNum = paySplit.ProcNum,
                AdjAmt = -paySplit.SplitAmt,
                PatNum = paySplit.PatNum,
                ProvNum = paySplit.ProvNum,
                ClinicNum = paySplit.ClinicNum,
                AdjType = def.DefNum
            });
        }

        return adjustmentsAdded;
    }
}