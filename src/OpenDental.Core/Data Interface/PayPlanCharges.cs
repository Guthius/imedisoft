using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PayPlanCharges
{
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

    public static List<PayPlanCharge> GetForPayPlan(long payPlanNum)
    {
        var command =
            "SELECT * FROM payplancharge "
            + "WHERE PayPlanNum=" + SOut.Long(payPlanNum)
            + " ORDER BY ChargeDate";
        return PayPlanChargeCrud.SelectMany(command);
    }

    public static List<PayPlanCharge> GetForPayPlans(List<long> listPayPlanNums)
    {
        if (listPayPlanNums == null || listPayPlanNums.Count < 1) return new List<PayPlanCharge>();

        var command =
            "SELECT * FROM payplancharge "
            + "WHERE PayPlanNum IN (" + SOut.String(string.Join(",", listPayPlanNums)) + ") "
            + "ORDER BY ChargeDate";
        return PayPlanChargeCrud.SelectMany(command);
    }

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

    public static List<PayPlanCharge> GetChargesForPayPlanChargeType(List<long> listPayPlanNums, PayPlanChargeType chargeType)
    {
        if (listPayPlanNums.IsNullOrEmpty()) return new List<PayPlanCharge>();

        var command = "SELECT * FROM payplancharge "
                      + "WHERE PayPlanNum IN(" + string.Join(",", listPayPlanNums.Select(x => SOut.Long(x))) + ") "
                      + "AND ChargeType=" + SOut.Int((int) chargeType);
        return PayPlanChargeCrud.SelectMany(command);
    }

    public static List<PayPlanCharge> GetAllProcCreditsForPayPlans(List<long> listPayPlanNums)
    {
        if (listPayPlanNums.Count == 0) return new List<PayPlanCharge>();

        var command = $"SELECT * FROM payplancharge WHERE payplancharge.ChargeType={SOut.Int((int) PayPlanChargeType.Credit)} " +
                      $"AND payplancharge.ProcNum!=0 AND payplancharge.PayPlanNum IN ({string.Join(",", listPayPlanNums)})";
        return PayPlanChargeCrud.SelectMany(command);
    }

    public static List<PayPlanCharge> GetFromProc(long procNum)
    {
        var command = $"SELECT * FROM payplancharge WHERE payplancharge.ProcNum={SOut.Long(procNum)} OR (payplancharge.LinkType=" +
                      $"{SOut.Int((int) PayPlanLinkType.Procedure)} AND payplancharge.FKey={SOut.Long(procNum)})";
        return PayPlanChargeCrud.SelectMany(command);
    }

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

    public static long Insert(PayPlanCharge charge)
    {
        return PayPlanChargeCrud.Insert(charge);
    }

    public static void InsertMany(List<PayPlanCharge> listPayPlanCharges)
    {
        if (listPayPlanCharges.IsNullOrEmpty()) return;

        PayPlanChargeCrud.InsertMany(listPayPlanCharges);
    }

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

    public static void Sync(List<PayPlanCharge> listPayPlanCharges, long payPlanNum)
    {
        var listDB = GetForPayPlan(payPlanNum);
        PayPlanChargeCrud.Sync(listPayPlanCharges, listDB);
    }

    public static void DeleteForProc(long procNum)
    {
        if (procNum == 0) return;
        var listPayPlans = PayPlans.GetAllForCharges(GetFromProc(procNum));
        var command = "DELETE FROM payplancharge WHERE ProcNum=" + SOut.Long(procNum);
        Db.NonQ(command);
        PayPlans.UpdateTreatmentCompletedAmt(listPayPlans);
    }

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
}