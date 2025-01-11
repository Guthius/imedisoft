using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PayPlanCharges
{
    #region Get Methods

    
    public static List<PayPlanCharge> GetForDownPayment(PayPlanTerms terms, Family family, List<PayPlanLink> listPayPlanLinks, PayPlan payplan)
    {
        //Create a temporary variable to keep track of the original PeriodPayment.
        var periodPaymentTemp = terms.PeriodPayment;
        var aprTemp = terms.APR;
        //Set the PeriodPayment to the current DownPayment so that the full amount of the down payment gets generated.
        //E.g. there are several procedures attached to the payment plan and the down payment only covers one and a half (partial proc).
        terms.PeriodPayment = (decimal) terms.DownPayment;
        terms.APR = 0; //downpayments should pay on principal only
        var downPaymentChargeDate = DateTime.Today; //The chargeDate for the downpayment.
        if (terms.DateFirstPayment < downPaymentChargeDate) //If Date of First Payment was backdated, we need to use that date for the Down Payment.
            downPaymentChargeDate = terms.DateFirstPayment;
        var listDownPayments = PayPlanEdit.GetListExpectedCharges(new List<PayPlanCharge>(), terms, family, listPayPlanLinks, payplan, true
            , true, new List<PaySplit>());
        listDownPayments.ForEach(x =>
        {
            x.Note = "Down Payment";
            x.ChargeDate = downPaymentChargeDate;
            x.Interest = 0;
        });
        //Put the PeriodPayment back to the way it was upon entry.
        terms.PeriodPayment = periodPaymentTemp;
        terms.APR = aprTemp;
        return listDownPayments;
    }

    ///<summary>Gets all payplancharges for a specific payment plan.</summary>
    public static List<PayPlanCharge> GetForPayPlan(long payPlanNum)
    {
        var command =
            "SELECT * FROM payplancharge "
            + "WHERE PayPlanNum=" + SOut.Long(payPlanNum)
            + " ORDER BY ChargeDate";
        return PayPlanChargeCrud.SelectMany(command);
    }

    ///<summary>Gets all payplancharges for a specific payment plan for the API.</summary>
    public static List<PayPlanCharge> GetPayPlanChargesForApi(int limit, int offset, long payPlanNum)
    {
        var command =
            "SELECT * FROM payplancharge "
            + "WHERE PayPlanNum=" + SOut.Long(payPlanNum) + " "
            + "ORDER BY PayPlanChargeNum "
            + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return PayPlanChargeCrud.SelectMany(command);
    }

    ///<summary>Returns a list of payplancharges associated to the passed in payplannums.  Will return a blank list if none.</summary>
    public static List<PayPlanCharge> GetForPayPlans(List<long> listPayPlanNums)
    {
        if (listPayPlanNums == null || listPayPlanNums.Count < 1) return new List<PayPlanCharge>();

        var command =
            "SELECT * FROM payplancharge "
            + "WHERE PayPlanNum IN (" + SOut.String(string.Join(",", listPayPlanNums)) + ") "
            + "ORDER BY ChargeDate";
        return PayPlanChargeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all payplan charges for the payplans passed in where the specified patient is the Guarantor.  Based on today's
    ///     date.
    ///     Will return both credits and debits.  Does not return insurance payment plan charges.
    /// </summary>
    public static List<PayPlanCharge> GetDueForPayPlan(PayPlan payPlan, long patNum)
    {
        return GetDueForPayPlans(new List<PayPlan> {payPlan}, patNum);
    }

    /// <summary>
    ///     Gets all payplan charges for the payplans passed in where the specified patient is the Guarantor.  Based on today's
    ///     date.
    ///     Will return both credits and debits.  Does not return insurance payment plan charges.
    /// </summary>
    public static List<PayPlanCharge> GetDueForPayPlans(List<PayPlan> listPayPlans, long patNum)
    {
        if (listPayPlans.Count < 1) return new List<PayPlanCharge>();
        var command = "SELECT payplancharge.* FROM payplan "
                      + "INNER JOIN payplancharge ON payplancharge.PayPlanNum = payplan.PayPlanNum "
                      + "AND payplancharge.ChargeDate <= " + DbHelper.Curdate() + " "
                      + "WHERE payplan.Guarantor=" + SOut.Long(patNum) + " "
                      + "AND payplan.PayPlanNum IN(" + string.Join(", ", listPayPlans.Select(x => x.PayPlanNum).ToList()) + ") "
                      + "AND payplan.PlanNum = 0 "; //do not return insurance payment plan charges.
        return PayPlanChargeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all payplan charges for the payplans passed in where the any of the patients in the list are the Guarantor or
    ///     the patient on the
    ///     payment plan.  Will return both credits and debits.  Does not return insurance payment plan charges.
    /// </summary>
    public static List<PayPlanCharge> GetForPayPlans(List<long> listPayPlans, List<long> listPatNums)
    {
        if (listPayPlans.IsNullOrEmpty() || listPatNums.IsNullOrEmpty()) return new List<PayPlanCharge>();

        var command = "SELECT payplancharge.* FROM payplan "
                      + "INNER JOIN payplancharge ON payplancharge.PayPlanNum = payplan.PayPlanNum "
                      + "WHERE (payplan.PatNum IN(" + string.Join(",", listPatNums) + ") OR payplan.Guarantor IN(" + string.Join(",", listPatNums) + ")) "
                      + "AND payplan.PayPlanNum IN(" + string.Join(", ", listPayPlans) + ") "
                      + "AND payplan.PlanNum = 0 "; //do not return insurance payment plan charges.
        return PayPlanChargeCrud.SelectMany(command);
    }

    
    public static List<PayPlanCharge> GetChargesForPayPlanChargeType(long payPlanNum, PayPlanChargeType chargeType)
    {
        return GetChargesForPayPlanChargeType(new List<long> {payPlanNum}, chargeType);
    }

    
    public static List<PayPlanCharge> GetChargesForPayPlanChargeType(List<long> listPayPlanNums, PayPlanChargeType chargeType)
    {
        if (listPayPlanNums.IsNullOrEmpty()) return new List<PayPlanCharge>();

        var command = "SELECT * FROM payplancharge "
                      + "WHERE PayPlanNum IN(" + string.Join(",", listPayPlanNums.Select(x => SOut.Long(x))) + ") "
                      + "AND ChargeType=" + SOut.Int((int) chargeType);
        return PayPlanChargeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all charges of the credit type that don't have a procnum of 0 and belong to any of the pats in
    ///     listPatNums
    /// </summary>
    public static List<PayPlanCharge> GetAllProcCreditsForPats(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<PayPlanCharge>();

        var command = $"SELECT * FROM payplancharge WHERE payplancharge.ChargeType={SOut.Int((int) PayPlanChargeType.Credit)} " +
                      $"AND payplancharge.ProcNum!=0 AND payplancharge.PatNum IN ({string.Join(",", listPatNums)})";
        return PayPlanChargeCrud.SelectMany(command);
    }

    ///<summary>Gets all credit charges for procedures that belong to any of the payplans in listPayPlanNums</summary>
    public static List<PayPlanCharge> GetAllProcCreditsForPayPlans(List<long> listPayPlanNums)
    {
        if (listPayPlanNums.Count == 0) return new List<PayPlanCharge>();

        var command = $"SELECT * FROM payplancharge WHERE payplancharge.ChargeType={SOut.Int((int) PayPlanChargeType.Credit)} " +
                      $"AND payplancharge.ProcNum!=0 AND payplancharge.PayPlanNum IN ({string.Join(",", listPayPlanNums)})";
        return PayPlanChargeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Takes a procNum and returns a list of all payment plan charges associated to the procedure.
    ///     Returns an empty list if there are none.
    /// </summary>
    public static List<PayPlanCharge> GetFromProc(long procNum)
    {
        var command = $"SELECT * FROM payplancharge WHERE payplancharge.ProcNum={SOut.Long(procNum)} OR (payplancharge.LinkType=" +
                      $"{SOut.Int((int) PayPlanLinkType.Procedure)} AND payplancharge.FKey={SOut.Long(procNum)})";
        return PayPlanChargeCrud.SelectMany(command);
    }

    ///<summary>Gets a list of all payment plan charges of type Credit associated to the procedures for patient payment plans.</summary>
    public static List<PayPlanCharge> GetPatientPayPlanCreditsForProcs(List<long> listProcNums)
    {
        if (listProcNums.Count == 0) return new List<PayPlanCharge>();

        var command = $"SELECT * FROM payplancharge WHERE payplancharge.ProcNum IN({string.Join(",", listProcNums.Select(x => SOut.Long(x)))})" +
                      $" AND payplancharge.ChargeType={SOut.Int((int) PayPlanChargeType.Credit)}";
        return PayPlanChargeCrud.SelectMany(command);
    }

    
    public static PayPlanCharge GetOne(long payPlanChargeNum)
    {
        var command =
            "SELECT * FROM payplancharge "
            + "WHERE PayPlanChargeNum=" + SOut.Long(payPlanChargeNum);
        return PayPlanChargeCrud.SelectOne(command);
    }

    public static List<PayPlanCharge> GetMany(List<long> listPayPlanChargeNums)
    {
        if (listPayPlanChargeNums.IsNullOrEmpty()) return new List<PayPlanCharge>();

        var command =
            "SELECT * FROM payplancharge "
            + "WHERE PayPlanChargeNum IN (" + string.Join(",", listPayPlanChargeNums.Select(x => SOut.Long(x))) + ")";
        return PayPlanChargeCrud.SelectMany(command);
    }

    ///<summary>Gets a list of charges for the passed in fkey and link type (i.e. adjustment, procedure...)</summary>
    public static List<PayPlanCharge> GetForLinkTypeAndFKeys(PayPlanLinkType linkType, params long[] arrayFKeys)
    {
        if (arrayFKeys.IsNullOrEmpty()) return new List<PayPlanCharge>();

        var command = $"SELECT * FROM payplancharge " +
                      $"WHERE payplancharge.FKey IN({string.Join(",", arrayFKeys.Select(x => SOut.Long(x)))}) " +
                      $"AND payplancharge.LinkType={SOut.Int((int) linkType)}";
        return PayPlanChargeCrud.SelectMany(command);
    }

    public static List<PayPlanCharge> GetForProcs(List<long> listProcNums)
    {
        if (listProcNums.IsNullOrEmpty()) return new List<PayPlanCharge>();

        var command = $"SELECT * FROM payplancharge WHERE payplancharge.ProcNum IN ({string.Join(",", listProcNums)}) ";
        return PayPlanChargeCrud.SelectMany(command);
    }

    #endregion

    #region Insert

    
    public static long Insert(PayPlanCharge charge)
    {
        return PayPlanChargeCrud.Insert(charge);
    }

    
    public static void InsertMany(List<PayPlanCharge> listPayPlanCharges)
    {
        if (listPayPlanCharges.IsNullOrEmpty()) return;

        PayPlanChargeCrud.InsertMany(listPayPlanCharges);
    }

    #endregion

    #region Update

    /// <summary>
    ///     Takes a procNum and updates all of the dates of the payment plan charge credits associated to it.
    ///     If a completed procedure is passed in, it will update all of the payment plan charges associated to it to the
    ///     ProcDate.
    ///     If a non-complete procedure is passed in, it will update the charges associated to MaxValue.
    ///     Does nothing if there are no charges attached to the passed-in procedure.
    /// </summary>
    public static void UpdateAttachedPayPlanCharges(Procedure proc)
    {
        #region PayPlanCharge.ChargeDate

        var listCharges = GetFromProc(proc.ProcNum);
        var listPayPlans = PayPlans.GetAllForCharges(listCharges);
        foreach (var chargeCur in listCharges)
        {
            var planForCharge = listPayPlans.FirstOrDefault(x => x.PayPlanNum == chargeCur.PayPlanNum);
            if (planForCharge.IsDynamic) //Dynamic payment plan charges only get issued when they're due, thus should not have their dates changed.
                continue;
            chargeCur.ChargeDate = DateTime.MaxValue;
            if (proc.ProcStatus == ProcStat.C) chargeCur.ChargeDate = proc.ProcDate;
            Update(chargeCur); //one update statement for each payplancharge.
        }

        #endregion

        #region PayPlan.CompletedAmt

        //The list of payment plans is guaranteed to have every patient payment plan that is associated to the procedure at this point.
        //However, it is not guaranteed to have every dynamic payment plan associated to the procedure (only debits are stored in the payplancharge table).
        PayPlans.UpdateTreatmentCompletedAmt(listPayPlans);
        //Refresh the list of payment plans so that every dynamic payment plan associated to the procedure is present.
        var listPayPlanLinks = PayPlanLinks.GetForFKeyAndLinkType(proc.ProcNum, PayPlanLinkType.Procedure);
        var listPayPlanNums = listPayPlanLinks.Select(x => x.PayPlanNum).ToArray();
        listPayPlans = PayPlans.GetMany(listPayPlanNums);
        PayPlans.UpdateTreatmentCompletedAmtsDynamicPaymentPlan(listPayPlans);

        #endregion
    }

    /// <summary>
    ///     Takes an insurance payplan and updates all payplancharge credits associated to it to match the completed
    ///     amount on the payplan. Every insurance payplan should only have 1 PayPlanCharge of type Credit. The payplan passed
    ///     in should have the correct CompletedAmt.
    /// </summary>
    public static void UpdateInsPlanPayPlanCharges(PayPlan payplan)
    {
        if (payplan == null || payplan.PayPlanNum == 0 || payplan.InsSubNum == 0) return;

        var command = $"UPDATE payplancharge SET Principal={SOut.Double(payplan.CompletedAmt)} " +
                      $"WHERE PayPlanNum={SOut.Long(payplan.PayPlanNum)} AND ChargeType={SOut.Enum(PayPlanChargeType.Credit)}";
        Db.NonQ(command);
    }

    
    public static void Update(PayPlanCharge charge)
    {
        PayPlanChargeCrud.Update(charge);
    }

    
    public static void Update(PayPlanCharge payPlanCharge, PayPlanCharge payPlanChargeOld)
    {
        PayPlanChargeCrud.Update(payPlanCharge, payPlanChargeOld);
    }

    ///<summary>Inserts, updates, or deletes database rows to match supplied list.  Must always pass in payPlanNum.</summary>
    public static void Sync(List<PayPlanCharge> listPayPlanCharges, long payPlanNum)
    {
        var listDB = GetForPayPlan(payPlanNum);
        PayPlanChargeCrud.Sync(listPayPlanCharges, listDB);
    }

    #endregion

    #region Delete

    /// <summary>
    ///     Will delete all PayPlanCharges associated to the passed-in procNum from the database.  Does nothing if the
    ///     procNum = 0.
    /// </summary>
    public static void DeleteForProc(long procNum)
    {
        if (procNum == 0) return;
        var listPayPlans = PayPlans.GetAllForCharges(GetFromProc(procNum));
        var command = "DELETE FROM payplancharge WHERE ProcNum=" + SOut.Long(procNum);
        Db.NonQ(command);
        PayPlans.UpdateTreatmentCompletedAmt(listPayPlans);
    }

    /// <summary>
    ///     Returns a list of payment plan charges that are not safe to delete (either they are credits, or are charges
    ///     with payments attached). If doDelete is true, all debits passed in will be deleted if they are safe to be deleted.
    ///     Calling methods should use the list of charges returned to know which ones were not deleted.
    /// </summary>
    public static List<PayPlanCharge> DeleteDebitsWithoutPayments(List<PayPlanCharge> listCharges, bool doDelete = true)
    {
        var listPayPlanChargesNotDeleted = new List<PayPlanCharge>();
        if (listCharges.IsNullOrEmpty()) return listPayPlanChargesNotDeleted;
        //Do not allow deleting payment plan charges with payments attached.
        var listPayPlanChargeNumsPreserve = PaySplits.GetForPayPlanCharges(listCharges.Where(x => x.PayPlanChargeNum != 0).Select(x => x.PayPlanChargeNum).ToList())
            .Select(x => x.PayPlanChargeNum)
            .Distinct()
            .ToList();
        //Do not allow deleting debits.
        var lisPayPlanChargesCredits = listCharges.FindAll(x => x.ChargeType == PayPlanChargeType.Credit);
        listPayPlanChargeNumsPreserve.AddRange(lisPayPlanChargesCredits.Select(x => x.PayPlanChargeNum));
        //Block deleting down payments.
        listPayPlanChargeNumsPreserve.AddRange(listCharges.FindAll(x => x.Note.ToLower().Contains("down payment")).Select(x => x.PayPlanChargeNum));
        //Actually delete the charges from the database if calling method requests it.
        if (doDelete)
        {
            var listPayPlanChargesToDelete = listCharges.FindAll(x => !listPayPlanChargeNumsPreserve.Contains(x.PayPlanChargeNum));
            DeleteMany(listPayPlanChargesToDelete.Select(x => x.PayPlanChargeNum).ToList());
        }

        //Return the list of charges that were not deleted.
        return listCharges.FindAll(x => listPayPlanChargeNumsPreserve.Contains(x.PayPlanChargeNum));
    }

    
    public static void Delete(PayPlanCharge charge)
    {
        var command = "DELETE from payplancharge WHERE PayPlanChargeNum = '"
                      + SOut.Long(charge.PayPlanChargeNum) + "'";
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listCharges)
    {
        if (listCharges.IsNullOrEmpty()) return;

        var command = $"DELETE from payplancharge WHERE PayPlanChargeNum IN ({string.Join(",", listCharges.Select(x => SOut.Long(x)))})";
        Db.NonQ(command);
    }

    #endregion
}