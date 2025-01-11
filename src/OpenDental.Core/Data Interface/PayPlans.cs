using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CDT;
using CodeBase;
using DataConnectionBase;
using ODCrypt;
using OpenDentBusiness.Crud;
using OpenDentBusiness.Misc;
using OpenDentBusiness.UI;

namespace OpenDentBusiness;


public class PayPlans
{
    #region Delete

    /// <summary>
    ///     Called from FormPayPlan.  Also deletes all attached payplancharges.  Throws exception if there are any
    ///     paysplits attached.
    /// </summary>
    public static void Delete(PayPlan plan)
    {
        string command;
        if (plan.PlanNum == 0 || plan.IsDynamic)
        {
            //Patient payment plan
            command = "SELECT COUNT(*) FROM paysplit WHERE PayPlanNum=" + SOut.Long(plan.PayPlanNum);
            if (Db.GetCount(command) != "0")
                throw new ApplicationException
                    (Lans.g("PayPlans", "You cannot delete a payment plan with patient payments attached.  Unattach the payments first."));
        }
        else
        {
            //Insurance payment plan
            command = "SELECT COUNT(*) FROM claimproc WHERE PayPlanNum=" + SOut.Long(plan.PayPlanNum) + " AND claimproc.Status IN ("
                      + SOut.Int((int) ClaimProcStatus.Received) + "," + SOut.Int((int) ClaimProcStatus.Supplemental) + ")";
            if (Db.GetCount(command) != "0")
                throw new ApplicationException
                    (Lans.g("PayPlans", "You cannot delete a payment plan with insurance payments attached.  Unattach the payments first."));
            //if there are any unreceived items, detach them here, then proceed deleting
            var listClaimProcs = ClaimProcs.GetForPayPlans(new List<long> {plan.PayPlanNum});
            foreach (var claimProc in listClaimProcs)
            {
                claimProc.PayPlanNum = 0;
                ClaimProcs.Update(claimProc);
            }
        }

        command = "DELETE FROM payplancharge WHERE PayPlanNum=" + SOut.Long(plan.PayPlanNum);
        Db.NonQ(command);
        command = $"DELETE FROM payplanlink WHERE PayPlanNum={SOut.Long(plan.PayPlanNum)}";
        Db.NonQ(command);
        command = "DELETE FROM payplan WHERE PayPlanNum =" + SOut.Long(plan.PayPlanNum);
        Db.NonQ(command);
        command = $"DELETE FROM orthoplanlink WHERE orthoplanlink.FKey={SOut.Long(plan.PayPlanNum)} " +
                  $"AND orthoplanlink.LinkType IN ({SOut.Enum(OrthoPlanLinkType.PatPayPlan)}," +
                  $"{SOut.Enum(OrthoPlanLinkType.InsPayPlan)})";
        Db.NonQ(command);
        CreditCards.RemoveRecurringCharges(plan.PayPlanNum);
    }

    #endregion

    #region Get Methods

    public static List<PayPlan> GetAllWithMobileAppDeviceNum(long mobileAppDeviceNum)
    {
        var command = "SELECT * FROM payplan WHERE MobileAppDeviceNum=" + SOut.Long(mobileAppDeviceNum) + ";";
        return PayPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets a list of all payplans for a given patient, whether they are the patient or the guarantor.  This is only
    ///     used in one place, when deleting a patient to check dependencies.
    /// </summary>
    public static int GetDependencyCount(long patNum)
    {
        var command = "SELECT COUNT(*) FROM payplan"
                      + " WHERE PatNum = " + SOut.Long(patNum)
                      + " OR Guarantor = " + SOut.Long(patNum);
        return SIn.Int(DataCore.GetScalar(command));
    }

    public static PayPlan GetOne(long payPlanNum)
    {
        return PayPlanCrud.SelectOne(payPlanNum);
    }

    public static List<PayPlan> GetMany(params long[] arrayPayPlanNums)
    {
        if (arrayPayPlanNums.IsNullOrEmpty()) return new List<PayPlan>();

        var command = $"SELECT * FROM payplan WHERE PayPlanNum IN ({string.Join(",", arrayPayPlanNums.Select(x => SOut.Long(x)))})";
        return PayPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of payment plans where the patient of the payment plan matches ANY in listPatNums OR the guarantor
    ///     matches patNum.
    ///     patNum will typically be the current patient.
    /// </summary>
    public static List<PayPlan> GetForPats(List<long> listPatNums, long guarantor)
    {
        //We have to check for guarantor separately in case the payment plan belongs to a patient in another family.
        var command = "SELECT * FROM payplan WHERE Guarantor=" + SOut.Long(guarantor);
        if (!listPatNums.IsNullOrEmpty()) command += " OR PatNum IN(" + string.Join(",", listPatNums) + ")";
        return PayPlanCrud.SelectMany(command);
    }

    ///<summary>Gets All patient pay plans for each patient in listPatNums.</summary>
    public static List<PayPlan> GetAllPatPayPlansForPats(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<PayPlan>();

        var command = $"SELECT * FROM payplan WHERE payplan.PatNum IN({string.Join(",", listPatNums)}) AND payplan.InsSubNum=0";
        return PayPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all payment plans that this patient is associated to.
    ///     Will return payment plans that this pat is the patient or guarantor of.
    /// </summary>
    public static List<PayPlan> GetForPatNum(long patNum)
    {
        var command = "SELECT * FROM payplan "
                      + "WHERE PatNum = " + SOut.Long(patNum) + " "
                      + "OR Guarantor = " + SOut.Long(patNum);
        return PayPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of overcharged payplans from the listPayPlanNums. Only necessary for Dynamic Payment Plans.
    ///     Returns an empty list if none are overcharged.
    /// </summary>
    public static List<PayPlan> GetOverChargedPayPlans(List<long> listPayPlanNums)
    {
        #region Get Data

        var listPayPlanLinksAll = PayPlanLinks.GetForPayPlans(listPayPlanNums);
        var listProcedureLinkFKeys = listPayPlanLinksAll.Where(x => x.LinkType == PayPlanLinkType.Procedure).Select(x => x.FKey).ToList();
        var listAdjustmentLinkFKeys = listPayPlanLinksAll.Where(x => x.LinkType == PayPlanLinkType.Adjustment).Select(x => x.FKey).ToList();
        var listPayPlanCharges = PayPlanCharges.GetForPayPlans(listPayPlanNums);
        var listProcsAttachedToPayPlan = Procedures.GetManyProc(listProcedureLinkFKeys, false);
        var listAdjsAttachedToPayPlan = Adjustments.GetMany(listAdjustmentLinkFKeys);
        var listClaimProcsForProcs = ClaimProcs.GetForProcs(listProcedureLinkFKeys);
        var listAdjForProcs = Adjustments.GetForProcs(listProcedureLinkFKeys);
        var listSplitsForProcs = PaySplits.GetPaySplitsFromProcs(listProcedureLinkFKeys);
        var listSplitsForAdjustments = PaySplits.GetForAdjustments(listAdjustmentLinkFKeys);
        var listPayPlans = GetMany(listPayPlanNums.ToArray());

        #endregion Get Data

        var listPayPlansOvercharged = new List<long>();
        foreach (var payPlan in listPayPlans)
        {
            if (payPlan.DatePayPlanStart.Date > DateTime.Now.Date) continue;
            var listLinksForPayPlan = listPayPlanLinksAll.FindAll(x => x.PayPlanNum == payPlan.PayPlanNum);

            #region Sum Linked Production

            double amountOvercharged = 0;
            foreach (var payPlanLink in listLinksForPayPlan)
            {
                if (payPlanLink.LinkType == PayPlanLinkType.Procedure)
                {
                    var proc = listProcsAttachedToPayPlan.FirstOrDefault(x => x.ProcNum == payPlanLink.FKey);
                    if (proc != null)
                    {
                        var listExplicitAdjs = listAdjForProcs.FindAll(x => x.ProcNum == proc.ProcNum
                                                                            && x.PatNum == proc.PatNum
                                                                            && x.ProvNum == proc.ProvNum
                                                                            && x.ClinicNum == proc.ClinicNum);
                        if (payPlanLink.AmountOverride != 0)
                        {
                            amountOvercharged += payPlanLink.AmountOverride;
                        }
                        else
                        {
                            amountOvercharged += proc.ProcFee * Math.Max(1, proc.BaseUnits + proc.UnitQty);
                            if (!listExplicitAdjs.IsNullOrEmpty()) amountOvercharged += listExplicitAdjs.Sum(x => x.AdjAmt);
                            var listClaimProcsForProc = listClaimProcsForProcs.FindAll(x => x.ProcNum == proc.ProcNum);
                            double sumIns = 0;
                            if (!listClaimProcsForProc.IsNullOrEmpty())
                            {
                                var listClaimProcStatForInsPaid = ClaimProcs.GetInsPaidStatuses().Select(x => (int) x).ToList();
                                var listClaimProcStatForInsEst = ClaimProcs.GetEstimatedStatuses().Select(x => (int) x).ToList();
                                for (var i = 0; i < listClaimProcsForProc.Count(); i++)
                                    if (listClaimProcStatForInsPaid.Contains((int) listClaimProcsForProc[i].Status))
                                    {
                                        sumIns += listClaimProcsForProc[i].InsPayAmt + listClaimProcsForProc[i].WriteOff;
                                    }
                                    else if (listClaimProcStatForInsEst.Contains((int) listClaimProcsForProc[i].Status))
                                    {
                                        sumIns += listClaimProcsForProc[i].InsPayEst;
                                        if (listClaimProcsForProc[i].WriteOffEstOverride != -1)
                                            sumIns += listClaimProcsForProc[i].WriteOffEstOverride;
                                        else if (listClaimProcsForProc[i].WriteOffEst != -1) sumIns += listClaimProcsForProc[i].WriteOffEst;
                                    }
                            }

                            amountOvercharged -= sumIns;
                            amountOvercharged -= listSplitsForProcs.FindAll(x => x.ProcNum == payPlanLink.FKey && x.PayPlanNum == 0 && x.PayPlanChargeNum == 0).Sum(x => x.SplitAmt); // Outside Procedure PaySplits
                        }
                    }
                }
                else if (payPlanLink.LinkType == PayPlanLinkType.Adjustment)
                {
                    var adj = listAdjsAttachedToPayPlan.FirstOrDefault(x => x.AdjNum == payPlanLink.FKey);
                    if (adj != null)
                    {
                        if (payPlanLink.AmountOverride != 0)
                        {
                            amountOvercharged += payPlanLink.AmountOverride;
                        }
                        else
                        {
                            amountOvercharged += adj.AdjAmt;
                            amountOvercharged -= listSplitsForAdjustments.FindAll(x => x.AdjNum == payPlanLink.FKey && x.PayPlanNum == 0 && x.PayPlanChargeNum == 0).Sum(x => x.SplitAmt); // Outside Adjustment PaySplits
                        }
                    }
                }

                amountOvercharged -= listPayPlanCharges.FindAll(x => x.ChargeType == (int) PayPlanChargeType.Debit && x.FKey == payPlanLink.FKey && x.LinkType == payPlanLink.LinkType).Sum(x => x.Principal); // Debit PayPlanCharges
            }

            #endregion Sum Linked Production

            amountOvercharged = Math.Abs(Math.Min(Math.Round(amountOvercharged, 2), 0));
            if (CompareDecimal.IsGreaterThanZero(amountOvercharged)) listPayPlansOvercharged.Add(payPlan.PayPlanNum);
        }

        return GetMany(listPayPlansOvercharged.ToArray()).FindAll(x => x.IsDynamic);
    }

    ///<summary>Determines if there are any valid plans with that patient as the guarantor.</summary>
    public static List<PayPlan> GetValidPlansNoIns(long guarNum)
    {
        var command = "SELECT * FROM payplan"
                      + " WHERE Guarantor = " + SOut.Long(guarNum)
                      + " AND PlanNum = 0"
                      + " AND IsClosed = 0"
                      + " ORDER BY payplandate";
        return PayPlanCrud.SelectMany(command);
    }

    public static List<PayPlan> GetAllOpenInsPayPlans()
    {
        var command = "SELECT * FROM payplan WHERE payplan.PlanNum != 0 AND IsClosed = 0";
        return PayPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all insurance payplans that aren't fully paid for patients associated to the claims passed in.
    ///     Only returns payplans that have no claimprocs linked to them, or those that have claimprocs linked to them
    ///     that are also linked to one of the claims passed in.
    /// </summary>
    public static List<PayPlan> GetAllValidInsPayPlansForClaims(List<Claim> listClaims)
    {
        if (listClaims.IsNullOrEmpty()) return new List<PayPlan>();

        var command = "SELECT payplan.* FROM payplan "
                      + "LEFT JOIN claimproc ON claimproc.PayPlanNum=payplan.PayPlanNum	"
                      //Only ins payplans
                      + "WHERE payplan.PlanNum!=0 "
                      //Only ones for patients from the list of claims.
                      + $"AND payplan.PatNum IN ({string.Join(",", listClaims.Select(x => SOut.Long(x.PatNum)))}) "
                      //Only ones with no claimprocs attached or only claimprocs from the list of claims.
                      + $"AND (claimproc.ClaimNum IS NULL OR claimproc.ClaimNum IN ({string.Join(",", listClaims.Select(x => SOut.Long(x.ClaimNum)))})) "
                      + "GROUP BY payplan.PayPlanNum "
                      //Only ones that are not fully paid off.
                      + "HAVING payplan.CompletedAmt>SUM(COALESCE(claimproc.InsPayAmt,0)) "
                      + "ORDER BY payplan.PayPlanDate";
        return PayPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Get all payment plans for this patient with the insurance plan identified by PlanNum and InsSubNum attached
    ///     (marked used for tracking expected insurance payments) that have not been paid in full.  Only returns plans with no
    ///     claimprocs currently attached or claimprocs from the claim identified by the claimNum sent in attached.  If
    ///     claimNum is 0 all payment plans with planNum, insSubNum, and patNum not paid in full will be returned.
    /// </summary>
    public static List<PayPlan> GetValidInsPayPlans(long patNum, long planNum, long insSubNum, long claimNum)
    {
        var command = "";
        command += "SELECT payplan.*,MAX(claimproc.ClaimNum) ClaimNum";
        command += " FROM payplan"
                   + " LEFT JOIN claimproc ON claimproc.PayPlanNum=payplan.PayPlanNum"
                   + " WHERE payplan.PatNum=" + SOut.Long(patNum)
                   + " AND payplan.PlanNum=" + SOut.Long(planNum)
                   + " AND payplan.InsSubNum=" + SOut.Long(insSubNum);
        if (claimNum > 0) command += " AND (claimproc.ClaimNum IS NULL OR claimproc.ClaimNum=" + SOut.Long(claimNum) + ")"; //payplans with no claimprocs attached or only claimprocs from the same claim
        command += " GROUP BY payplan.PayPlanNum";
        command += " HAVING payplan.CompletedAmt>SUM(COALESCE(claimproc.InsPayAmt,0))"; //has not been paid in full yet
        if (claimNum == 0) //if current claimproc is not attached to a claim, do not return payplans with claimprocs from existing claims already attached
            command += " AND (MAX(claimproc.ClaimNum) IS NULL OR MAX(claimproc.ClaimNum)=0)";
        command += " ORDER BY payplan.PayPlanDate";
        var payPlansWithClaimNum = DataCore.GetTable(command);
        var retval = new List<PayPlan>();
        for (var i = 0; i < payPlansWithClaimNum.Rows.Count; i++)
        {
            var planCur = new PayPlan();
            planCur.PayPlanNum = SIn.Long(payPlansWithClaimNum.Rows[i]["PayPlanNum"].ToString());
            planCur.PatNum = SIn.Long(payPlansWithClaimNum.Rows[i]["PatNum"].ToString());
            planCur.Guarantor = SIn.Long(payPlansWithClaimNum.Rows[i]["Guarantor"].ToString());
            planCur.PayPlanDate = SIn.Date(payPlansWithClaimNum.Rows[i]["PayPlanDate"].ToString());
            planCur.APR = SIn.Double(payPlansWithClaimNum.Rows[i]["APR"].ToString());
            planCur.Note = payPlansWithClaimNum.Rows[i]["Note"].ToString();
            planCur.PlanNum = SIn.Long(payPlansWithClaimNum.Rows[i]["PlanNum"].ToString());
            planCur.CompletedAmt = SIn.Double(payPlansWithClaimNum.Rows[i]["CompletedAmt"].ToString());
            planCur.InsSubNum = SIn.Long(payPlansWithClaimNum.Rows[i]["InsSubNum"].ToString());
            if (claimNum > 0 && payPlansWithClaimNum.Rows[i]["ClaimNum"].ToString() == claimNum.ToString())
            {
                //if a payplan exists with claimprocs from the same claim as the current claimproc attached, always only return that one payplan
                //claimprocs from one claim are not allowed to be attached to different payplans
                retval.Clear();
                retval.Add(planCur);
                break;
            }

            retval.Add(planCur);
        }

        return retval;
    }

    /// <summary>
    ///     Executes a LINQ statement that returns the total amount of tx that is both completed and planned for the passed in
    ///     payment plan.
    ///     Only used for payplans v2.  Different from the TxCompletedAmt, which looks ONLY at PayPlanCharge credits that have
    ///     already occurred.
    ///     Does not update or make any calls to the database, as TxTotalAmt is not a db column.
    /// </summary>
    public static double GetTxTotalAmt(List<PayPlanCharge> listCharges)
    {
        if (listCharges.IsNullOrEmpty()) return 0;
        return listCharges.Where(x => x.ChargeType == PayPlanChargeType.Credit)
            .Sum(x => x.Principal);
    }

    /// <summary>
    ///     Gets info directly from database. Used from PayPlan and Account windows to get the amount paid so far on one
    ///     payment plan.
    /// </summary>
    public static double GetAmtPaid(PayPlan payPlan)
    {
        string command;
        if (payPlan.PlanNum == 0) //Patient payment plan
            command = "SELECT SUM(paysplit.SplitAmt) FROM paysplit "
                      + "WHERE paysplit.PayPlanNum = " + SOut.Long(payPlan.PayPlanNum) + " "
                      + "GROUP BY paysplit.PayPlanNum";
        else //Insurance payment plan
            command = "SELECT SUM(claimproc.InsPayAmt) "
                      + "FROM claimproc "
                      + "WHERE claimproc.Status IN(" + SOut.Int((int) ClaimProcStatus.Received) + "," + SOut.Int((int) ClaimProcStatus.Supplemental) + ","
                      + SOut.Int((int) ClaimProcStatus.CapClaim) + ") "
                      + "AND claimproc.PayPlanNum=" + SOut.Long(payPlan.PayPlanNum);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return 0;
        return SIn.Double(table.Rows[0][0].ToString());
    }

    /// <summary>
    ///     Used from FormPayPlan and the Account to get the accumulated amount due for a payment plan based on today's date.
    ///     Includes interest, but does not include payments made so far.  The chargelist must include all charges for this
    ///     payplan,
    ///     but it can include more as well.
    /// </summary>
    public static double GetAccumDue(long payPlanNum, List<PayPlanCharge> chargeList)
    {
        double retVal = 0;
        for (var i = 0; i < chargeList.Count; i++)
        {
            if (chargeList[i].PayPlanNum != payPlanNum) continue;
            if (chargeList[i].ChargeDate > DateTime.Today) //not due yet
                continue;
            if (chargeList[i].ChargeType != PayPlanChargeType.Debit) //for v1, debits(0) are the only ChargeType.
                continue;
            retVal += chargeList[i].Principal + chargeList[i].Interest;
        }

        return retVal;
    }

    /// <summary>
    ///     Gets the amount due now of the passed in payment plan num.
    ///     Optionally pass in the list of PayPlanCharges and list of PaySplits to avoid unneccesary database calls.
    ///     Will filter out paysplits and charges associated to different payplans as well as payplan charges that are for the
    ///     future or have a charge type of debit.
    /// </summary>
    public static double GetDueNow(long payPlanNum, List<PayPlanCharge> listPayPlanCharges = null, List<PaySplit> listPaySplits = null)
    {
        double amtDue = 0;
        if (listPayPlanCharges == null) listPayPlanCharges = PayPlanCharges.GetForPayPlan(payPlanNum);
        if (listPaySplits == null) listPaySplits = PaySplits.GetFromBundled(PaySplits.GetForPayPlan(payPlanNum));
        foreach (var chargeCur in listPayPlanCharges)
            if (chargeCur.PayPlanNum == payPlanNum
                && chargeCur.ChargeType == PayPlanChargeType.Debit
                && chargeCur.ChargeDate <= DateTime.Today)
                amtDue += chargeCur.Principal + chargeCur.Interest;

        foreach (var splitCur in listPaySplits)
            if (splitCur.PayPlanNum == payPlanNum)
                amtDue -= splitCur.SplitAmt;

        return amtDue;
    }

    /// <summary>
    ///     Gets the current balance of the passed in payment plan num.
    ///     Performs the same calculation as the "balance" column in the payment plans grid in ContrAccount.
    ///     Optionally pass in the list of PayPlanCharges and list of PaySplits to avoid unneccesary database calls.
    ///     Will filter out paysplits and charges associated to different payplans as well as payplan charges that are for the
    ///     future or have a charge type of debit.
    /// </summary>
    public static double GetBalance(long payPlanNum, List<PayPlanCharge> listPayPlanCharges = null, List<PaySplit> listPaySplits = null)
    {
        double amtBal = 0;
        if (listPayPlanCharges == null) listPayPlanCharges = PayPlanCharges.GetForPayPlan(payPlanNum);
        if (listPaySplits == null) listPaySplits = PaySplits.GetFromBundled(PaySplits.GetForPayPlan(payPlanNum));
        foreach (var chargeCur in listPayPlanCharges)
            if (chargeCur.PayPlanNum == payPlanNum
                && chargeCur.ChargeType == PayPlanChargeType.Debit)
            {
                amtBal += chargeCur.Principal;
                if (chargeCur.ChargeDate <= DateTime.Today) amtBal += chargeCur.Interest;
            }

        foreach (var splitCur in listPaySplits)
            if (splitCur.PayPlanNum == payPlanNum)
                amtBal -= splitCur.SplitAmt;

        return amtBal;
    }

    /// <summary>
    ///     Gets the total cost now of the passed in payment plan num.
    ///     Optionally pass in the list of PayPlanCharges to avoid unneccesary database calls.
    ///     Will filter out charges associated to different payplans as well as payplan charges that are for the future or have
    ///     a charge type of debit.
    /// </summary>
    public static double GetTotalCost(long payPlanNum, List<PayPlanCharge> listPayPlanCharges = null)
    {
        double amtTotal = 0;
        List<PayPlanCharge> listPayPlanChargesForPlan;
        if (listPayPlanCharges == null)
            listPayPlanChargesForPlan = PayPlanCharges.GetForPayPlan(payPlanNum);
        else
            listPayPlanChargesForPlan = listPayPlanCharges.Where(x => x.PayPlanNum == payPlanNum).Select(x => x.Copy()).ToList();
        var payPlan = GetOne(payPlanNum);
        amtTotal += listPayPlanChargesForPlan.Where(x => x.ChargeType == PayPlanChargeType.Debit) //Only consider existing charges due
            .Sum(x => x.Principal + x.Interest); //Add up everything that has been charged (interest included) to amtTotal.
        if (payPlan.IsDynamic)
        {
            //If this payplan is dynamic add the amount of expected charges.
            var family = Patients.GetFamily(payPlan.PatNum);
            var listPayPlanLinks = PayPlanLinks.GetListForPayplan(payPlanNum); //Get all PayPlanLinks for this dynamic pay plan.
            var payPlanTerms = PayPlanEdit.GetPayPlanTerms(payPlan, listPayPlanLinks); //Get the terms for this dynamic pay plan.
            var listPayPlanChargesExpected = PayPlanEdit.GetListExpectedCharges
                (listPayPlanChargesForPlan, payPlanTerms, family, listPayPlanLinks, payPlan, false); //Calculate & collect every future payment
            amtTotal += listPayPlanChargesExpected.Sum(x => x.Principal + x.Interest); //Add the amount of each future payment (interest included) to amtTotal
        }

        return amtTotal;
    }

    public static List<PayPlan> GetAllForCharges(List<PayPlanCharge> listCharge)
    {
        if (listCharge.Count == 0) return new List<PayPlan>();
        var command = "SELECT * FROM payplan "
                      + "WHERE PayPlanNum IN (" + string.Join(",", listCharge.Select(x => x.PayPlanNum)) + ")";
        return PayPlanCrud.SelectMany(command);
    }

    /// <summary>
    ///     Used from Account window to get the amount paid so far on one payment plan.
    ///     Must pass in the total amount paid and the returned value will not be more than this.
    ///     The chargelist must include all charges for this payplan, but it can include more as well.
    ///     It will loop sequentially through the charges to get just the principal portion.
    /// </summary>
    public static double GetPrincPaid(double amtPaid, long payPlanNum, List<PayPlanCharge> chargeList)
    {
        //amtPaid gets reduced to 0 throughout this loop.
        double retVal = 0;
        for (var i = 0; i < chargeList.Count; i++)
        {
            if (chargeList[i].PayPlanNum != payPlanNum) continue;
            if (chargeList[i].ChargeType != PayPlanChargeType.Debit) //for v1, debits(0/ChargeDue) are the only ChargeType.
                continue;
            //For this charge, first apply payment to interest
            if (amtPaid > chargeList[i].Interest)
            {
                amtPaid -= chargeList[i].Interest;
            }
            else
            {
                //interest will eat up the remainder of the payment
                amtPaid = 0;
                break;
            }

            //Then, apply payment to principal
            if (amtPaid > chargeList[i].Principal)
            {
                retVal += chargeList[i].Principal;
                amtPaid -= chargeList[i].Principal;
            }
            else
            {
                //principal will eat up the remainder of the payment
                retVal += amtPaid;
                amtPaid = 0;
                break;
            }
        }

        return retVal;
    }

    /// <summary>
    ///     Used from Account and ComputeBal to get the total amount of the original principal for one payment plan.
    ///     Does not include any interest. The chargelist must include all charges for this payplan, but it can include more as
    ///     well.
    /// </summary>
    public static double GetTotalPrinc(long payPlanNum, List<PayPlanCharge> chargeList)
    {
        double retVal = 0;
        for (var i = 0; i < chargeList.Count; i++)
        {
            if (chargeList[i].PayPlanNum != payPlanNum) continue;
            if (chargeList[i].ChargeType != PayPlanChargeType.Debit) //for v1, debits(0/ChargeDue) are the only ChargeType.
                continue;
            retVal += chargeList[i].Principal;
        }

        return retVal;
    }

    /// <summary>
    ///     Gets the hashstring from the provided string that is typically generated from GetStringForSignatureHash().
    ///     This is done seperate of building the string so that new line replacements can be done when validating signatures
    ///     before hashing.
    /// </summary>
    public static string GetHashStringForSignature(string str)
    {
        return Encoding.ASCII.GetString(MD5.Hash(Encoding.UTF8.GetBytes(str)));
    }

    ///<summary>Get all open, dynamic payment plans.</summary>
    public static List<PayPlan> GetDynamic()
    {
        var command = "SELECT * FROM payplan WHERE payplan.IsDynamic=1 AND payplan.IsClosed=0";
        return PayPlanCrud.SelectMany(command);
    }

    #endregion

    #region Insert

    
    public static long Insert(PayPlan payPlan)
    {
        payPlan.SecurityHash = HashFields(payPlan);
        return PayPlanCrud.Insert(payPlan);
    }

    public static void InsertMany(List<PayPlan> listPayPlans)
    {
        if (listPayPlans.IsNullOrEmpty()) return;

        for (var i = 0; i < listPayPlans.Count; i++) listPayPlans[i].SecurityHash = HashFields(listPayPlans[i]);
        PayPlanCrud.InsertMany(listPayPlans);
    }

    #endregion

    #region Update

    /// <summary>
    ///     Updates the TreatmentCompletedAmt field of the passed in payplans in the database.
    ///     Used when a procedure attached to a payment plan charge is set complete or deleted.
    ///     The treatment completed amount only takes into account payplancharge credits that have already occurred
    ///     (no charges attached to TP'd procs).
    /// </summary>
    public static void UpdateTreatmentCompletedAmt(List<PayPlan> listPayPlans)
    {
        foreach (var payPlanCur in listPayPlans)
        {
            double completedAmt = 0;
            var listCharges = PayPlanCharges.GetForPayPlan(payPlanCur.PayPlanNum);
            completedAmt = listCharges.Where(x => x.ChargeType == PayPlanChargeType.Credit)
                .Where(x => x.ChargeDate.Date <= DateTime.Today.Date)
                .Select(x => x.Principal)
                .Sum();
            payPlanCur.CompletedAmt = completedAmt;
            Update(payPlanCur);
        }
    }

    /// <summary>
    ///     Updates the TreatmentCompletedAmt field of the passed in payplans in the database.
    ///     Used when a procedure attached to a payment plan charge is set complete or deleted.
    ///     The treatment completed amount only takes into account payplancharge credits that have already occurred
    ///     (no charges attached to TP'd procs).
    /// </summary>
    public static void UpdateTreatmentCompletedAmtsDynamicPaymentPlan(List<PayPlan> listPayPlans)
    {
        var listPayPlansUnique = listPayPlans.FindAll(x => x.IsDynamic).DistinctBy(x => x.PayPlanNum).ToList();
        var listPayPlanLinks = PayPlanLinks.GetForPayPlans(listPayPlans.Select(x => x.PayPlanNum).ToList());
        foreach (var payPlanCur in listPayPlansUnique)
        {
            var listPayPlanLinksForPlan = listPayPlanLinks.FindAll(x => x.PayPlanNum == payPlanCur.PayPlanNum);
            var payPlanProductionEntry = PayPlanProductionEntry.GetProductionForLinks(listPayPlanLinksForPlan);
            double completedAmt = 0;
            completedAmt = PayPlanProductionEntry.GetDynamicPayPlanCompletedAmount(payPlanCur, payPlanProductionEntry);
            payPlanCur.CompletedAmt = completedAmt;
            Update(payPlanCur);
        }
    }

    
    public static void Update(PayPlan payPlan)
    {
        var payPlanOld = GetOne(payPlan.PayPlanNum);
        if (IsPayPlanHashValid(payPlanOld)) payPlan.SecurityHash = HashFields(payPlan);
        PayPlanCrud.Update(payPlan);
    }

    #endregion

    #region Misc Methods

    ///<summary>Gets the key data string to be hashed for payment plans.</summary>
    public static string GetKeyDataStringForSignature(string APR, string NumberOfPayments, string paymentAmt, string freqOfPayments, string patName, string guarName, string sheetDefNum)
    {
        var strb = new StringBuilder();
        strb.Append(APR);
        strb.Append(NumberOfPayments);
        strb.Append(paymentAmt);
        strb.Append(freqOfPayments);
        strb.Append(patName);
        strb.Append(guarName);
        if (sheetDefNum != "0") strb.Append(sheetDefNum);
        return strb.ToString();
    }

    public static string GetTermsAndConditionsString(PayPlan plan, Patient pat, Patient guar, bool isHtmlEmail = false)
    {
        //replacement text fields
        var sb = new StringBuilder(PrefC.GetString(PrefName.PayPlanTermsAndConditions));
        var frequency = "";
        if (plan.IsDynamic)
        {
            //If the payment plan is dynamic, it uses the PayPlanFrequency enum.
            frequency = plan.ChargeFrequency.GetDescription().ToLower();
            if (plan.ChargeFrequency == PayPlanFrequency.OrdinalWeekday) frequency = "on a specific day of each month";
        }
        else
        {
            //If the payment plan is not dynamic, it uses the PaymentSchedule enum.
            frequency = plan.PaySchedule.GetDescription().ToLower();
            if (plan.PaySchedule == PaymentSchedule.MonthlyDayOfWeek) frequency = "on a specific day of each month";
        }

        ReplaceTags.ReplaceOneTag(sb, "[APR]", plan.APR.ToString(), isHtmlEmail);
        //ToString("C") formats such that 5 becomes "$5.00". We append an extra $ to escape "$" during a regex replacement, or else nonsense could happen (it'll look for grouping).
        var strPayAmt = plan.PayAmt.ToString("C");
        strPayAmt = strPayAmt.Replace("$", "$$");
        ReplaceTags.ReplaceOneTag(sb, "[PaymentAmt]", strPayAmt, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(sb, "[NumOfPayments]", plan.NumberOfPayments.ToString(), isHtmlEmail);
        frequency = Lans.g("PaymentPlanTermsAndCondtions", frequency);
        ReplaceTags.ReplaceOneTag(sb, "[ChargeFrequency]", frequency, isHtmlEmail);
        return sb.ToString();
    }

    ///<summary>Returns true if the patient passed in has any outstanding non-ins payment plans with them as the guarantor.</summary>
    public static bool HasOutstandingPayPlansNoIns(long guarNum)
    {
        var command = "SELECT SUM(paysplit.SplitAmt) FROM paysplit "
                      + "INNER JOIN payplan ON paysplit.PayPlanNum=payplan.PayPlanNum "
                      + "WHERE payplan.PlanNum=0 "
                      + "AND payplan.Guarantor=" + SOut.Long(guarNum);
        var amtPaid = SIn.Double(DataCore.GetScalar(command));
        command = "SELECT SUM(payplancharge.Principal+payplancharge.Interest) FROM payplancharge "
                  + "INNER JOIN payplan ON payplancharge.PayPlanNum=payplan.PayPlanNum "
                  + "WHERE payplancharge.ChargeType=" + SOut.Int((int) PayPlanChargeType.Debit) + " AND payplan.PlanNum=0 "
                  + "AND payplan.Guarantor=" + SOut.Long(guarNum);
        var totalCost = SIn.Double(DataCore.GetScalar(command));
        if (totalCost - amtPaid < .01) return false;
        return true;
    }

    /// <summary>Gets info directly from database. Used when adding a payment.</summary>
    public static bool PlanIsPaidOff(long payPlanNum)
    {
        var command = "SELECT SUM(paysplit.SplitAmt) FROM paysplit "
                      + "WHERE PayPlanNum = " + SOut.Long(payPlanNum); // +"' "
        //+" GROUP BY paysplit.PayPlanNum";
        var amtPaid = SIn.Double(DataCore.GetScalar(command));
        command = "SELECT SUM(Principal+Interest) FROM payplancharge "
                  + "WHERE ChargeType=" + SOut.Int((int) PayPlanChargeType.Debit) + " AND PayPlanNum=" + SOut.Long(payPlanNum);
        var totalCost = SIn.Double(DataCore.GetScalar(command));
        if (totalCost - amtPaid < .01) return true;
        return false;
    }

    public static List<long> GetDynamicPayPlanNumsWithTP(List<long> listPayPlanNums = null)
    {
        var command = "";
        command = "SELECT DISTINCT payplannum FROM payplanlink JOIN procedurelog ON payplanlink.fkey=procedurelog.procnum WHERE procedurelog.procstatus=" + SOut.Enum(ProcStat.TP) + " ";
        if (listPayPlanNums != null && listPayPlanNums.Count != 0)
        {
            var dynamicPayPlanNums = string.Join(",", listPayPlanNums);
            command += "AND payplanlink.payplannum IN " + "(" + dynamicPayPlanNums + ")" + " ";
        }

        command += "AND payplanlink.linktype=" + SOut.Enum(PayPlanLinkType.Procedure);
        return Db.GetListLong(command);
    }

    /// <summary>
    ///     Automatically closes all payment plans that have no future charges and that are paid off.
    ///     Returns the number of payment plans that were closed.
    /// </summary>
    public static long AutoClose(bool canIncludeOldPaymentPlans = false, bool canIncludeInsPaymentPlans = false)
    {
        var command = "";
        DataTable table;
        command = "SELECT payplan.PayPlanNum,SUM(payplancharge.Principal) AS Princ,SUM(payplancharge.Interest) AS Interest,"
                  + "COALESCE(ps.TotPayments,0) AS TotPay,COALESCE(cp.InsPayments,0) AS InsPay,"
                  + "MAX(payplancharge.ChargeDate) AS LastDate "
                  + "FROM payplan "
                  + "LEFT JOIN payplancharge ON payplancharge.PayPlanNum=payplan.PayPlanNum "
                  + "AND payplancharge.ChargeType=" + SOut.Int((int) PayPlanChargeType.Debit) + " "
                  + "LEFT JOIN ("
                  + "SELECT paysplit.PayPlanNum, SUM(paysplit.SplitAmt) AS TotPayments "
                  + "FROM paysplit "
                  + "GROUP BY paysplit.PayPlanNum "
                  + ")ps ON ps.PayPlanNum = payplan.PayPlanNum "
                  + "LEFT JOIN ( "
                  + "SELECT claimproc.PayPlanNum, SUM(claimproc.InsPayAmt) AS InsPayments "
                  + "FROM claimproc "
                  + "GROUP BY claimproc.PayPlanNum "
                  + ")cp ON cp.PayPlanNum = payplan.PayPlanNum "
                  + "WHERE payplan.IsClosed = 0 "
                  + "GROUP BY payplan.PayPlanNum "
                  + "HAVING Princ+Interest <= (TotPay + InsPay) AND LastDate <=" + DbHelper.Curdate();
        table = DataCore.GetTable(command);
        var arrayPayPlanNums = table.AsEnumerable().Select(x => (long) x["PayPlanNum"]).ToArray();
        var listPayPlans = GetMany(arrayPayPlanNums);
        var listPayPlanNumWithTP = new List<long>();
        listPayPlanNumWithTP = GetDynamicPayPlanNumsWithTP(listPayPlans.Where(x => x.IsDynamic).Select(x => x.PayPlanNum).ToList());
        var count = 0;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var payPlanNum = SIn.Long(table.Rows[i]["PayPlanNum"].ToString());
            var payPlan = listPayPlans.Find(x => x.PayPlanNum == payPlanNum);
            if (payPlan == null) continue;
            if (payPlan.IsDynamic)
            {
                if (payPlan.DynamicPayPlanTPOption == DynamicPayPlanTPOptions.AwaitComplete && listPayPlanNumWithTP.Contains(payPlan.PayPlanNum)) continue;
                var totalPaidAmt = 0.00;
                var dynamicPaymentPlanModuleData = PayPlanEdit.GetDynamicPaymentPlanModuleData(payPlan);
                var payPlanTerms = PayPlanEdit.GetPayPlanTerms(payPlan, dynamicPaymentPlanModuleData.ListPayPlanLinks);
                var listPayPlanChargesExpected = PayPlanEdit.GetPayPlanChargesForDynamicPaymentPlanSchedule(payPlan, payPlanTerms,
                    dynamicPaymentPlanModuleData.ListPayPlanChargesDb, dynamicPaymentPlanModuleData.ListPayPlanLinks, dynamicPaymentPlanModuleData.ListPaySplits);
                totalPaidAmt += SIn.Double(table.Rows[i]["TotPay"].ToString());
                totalPaidAmt += SIn.Double(table.Rows[i]["InsPay"].ToString());
                var amountToPay = 0.00;
                for (var k = 0; k < listPayPlanChargesExpected.Count; k++)
                {
                    if (listPayPlanChargesExpected[k].ChargeType == PayPlanChargeType.Credit)
                    {
                        amountToPay -= listPayPlanChargesExpected[k].Principal + listPayPlanChargesExpected[k].Interest;
                        continue;
                    }

                    amountToPay += listPayPlanChargesExpected[k].Principal + listPayPlanChargesExpected[k].Interest;
                }

                if (totalPaidAmt < amountToPay) continue;
                //"isLocked" here is passed into "isLocking" further down. We are not locking here because the user has not way to lock the dynamic payment plan from this UI.
                PayPlanEdit.CloseOutDynamicPaymentPlan(payPlanTerms, dynamicPaymentPlanModuleData, false, dynamicPaymentPlanModuleData.PayPlan.PlanCategory);
                SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum, Lans.g("PayPlans", "Payment Plan closed using Close Payment Plan tool."));
                count++;
            }
            else
            {
                if (payPlan.InsSubNum > 0 && !canIncludeInsPaymentPlans) continue;
                if (payPlan.InsSubNum == 0 && !canIncludeOldPaymentPlans) continue;
                payPlan.IsClosed = true;
                if (payPlan.InsSubNum > 0)
                {
                    //Manually closing ins payment plan will set the CompletedAmt to the total InsPay.
                    payPlan.CompletedAmt = SIn.Double(table.Rows[i]["InsPay"].ToString());
                    PayPlanCharges.UpdateInsPlanPayPlanCharges(payPlan);
                }

                Update(payPlan);
                var logMessage = Lans.g("PayPlans", "Patient Payment Plan closed using Close Payment Plan tool.");
                if (payPlan.InsSubNum > 0) logMessage = Lans.g("PayPlans", "Insurance Payment Plan closed using Close Payment Plan tool.");
                SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum, logMessage);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    ///     Returns the salted hash for the payplan. Will return an empty string if the calling program is unable to use
    ///     CDT.dll.
    /// </summary>
    public static string HashFields(PayPlan payPlan)
    {
        var unhashedText = payPlan.Guarantor + payPlan.PayAmt.ToString("f2") + payPlan.IsClosed + payPlan.IsLocked;
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
    ///     Validates the hash string in payplan.SecurityHash. Returns true if it matches the expected hash, otherwise
    ///     false.
    /// </summary>
    public static bool IsPayPlanHashValid(PayPlan payPlan)
    {
        if (payPlan == null) return true;
        if (payPlan.SecurityHash == null) //When a payplan is first created through middle tier and not yet refreshed from db, this can be null and should not show a warning triangle.
            return true;
        var dateHashStart = SecurityHash.GetHashingDate();
        if (payPlan.PayPlanDate < dateHashStart) //old
            return true;
        if (payPlan.SecurityHash == HashFields(payPlan)) //Hash is not what it should be
            return true;
        return false;
    }

    /// <summary>Returns true if there is a PayPlan attached to the payPlanNum and it is open, false otherwise</summary>
    public static bool IsClosed(long payPlanNum)
    {
        if (payPlanNum == 0) return false;
        var payPlan = GetOne(payPlanNum);
        if (payPlan == null || payPlan.IsClosed == false) return false;
        return true;
    }

    public static string GetChangeLog(List<string> listChanges)
    {
        var log = "";
        for (var i = 0; i < listChanges.Count; i++)
        {
            if (i > 0) log += ", ";
            if (i == listChanges.Count - 1 && listChanges.Count != 1) log += "and ";
            log += listChanges[i];
        }

        log += " changed.";
        return log;
    }

    #endregion

    #region Xam Methods

    /// <summary>
    ///     Gets the hash string for generating signatures. Used for Xamarin/web apps. Works with regular and dynamic pay
    ///     plans.
    /// </summary>
    public static string GetKeyDataForSignature(PayPlan payPlan)
    {
        //Dynamic payment plan key data is built differently than regular payment plan key data
        var pat = Patients.GetLim(payPlan.PatNum);
        var guar = Patients.GetLim(payPlan.Guarantor);
        var keyDataStr = GetKeyDataStringForSignature(
            payPlan.APR.ToString(),
            payPlan.NumberOfPayments.ToString(),
            payPlan.PayAmt.ToString("f"),
            payPlan.ChargeFrequency.GetDescription(),
            pat.GetNameFirstOrPrefL(),
            guar.GetNameFirstOrPrefL(),
            payPlan.SheetDefNum.ToString());
        return GetHashStringForSignature(keyDataStr);
    }

    ///<summary>Creates a new sheet from a given Pay Plan. Works for both regular or dynamic payment plans.</summary>
    public static Sheet PayPlanToSheet(PayPlan payPlan)
    {
        var listPayPlanCharges = PayPlanCharges.GetForPayPlan(payPlan.PayPlanNum).OrderBy(x => x.ChargeDate).ToList();
        double totalPrincipal = 0;
        double totalInterest = 0;
        var countDebits = 0;
        for (var i = 0; i < listPayPlanCharges.Count; i++)
        {
            if (listPayPlanCharges[i].ChargeType == PayPlanChargeType.Credit) continue; //don't include production when calculating the total loan cost, but do include adjustments
            countDebits++;
            if (listPayPlanCharges[i].ChargeType == PayPlanChargeType.Debit && listPayPlanCharges[i].Principal >= 0)
            {
                //Not an adjustment
                totalPrincipal += listPayPlanCharges[i].Principal;
                totalInterest += listPayPlanCharges[i].Interest;
            }
        }

        var totalNegAdjAmt = listPayPlanCharges.FindAll(x => x.ChargeType == PayPlanChargeType.Debit && x.Principal < 0).Sum(x => x.Principal);
        var totPrincIntAdj = totalPrincipal + totalInterest + totalNegAdjAmt;
        var sheetDef = SheetDefs.GetSheetDef(payPlan.SheetDefNum, false);
        if (sheetDef == null) sheetDef = SheetDefs.GetInternalOrCustom(SheetInternalType.PaymentPlan);
        var sheetPP = SheetUtil.CreateSheet(sheetDef, payPlan.PatNum);
        sheetPP.Parameters.Add(new SheetParameter(true, "payplan") {ParamValue = payPlan});
        //The math for this comes from FormPayPlanDynamic
        if (payPlan.IsDynamic)
        {
            var listPayPlanLinks = PayPlanLinks.GetListForPayplan(payPlan.PayPlanNum);
            var listPayPlanProductionEntries = PayPlanProductionEntry.GetProductionForLinks(listPayPlanLinks);
            listPayPlanProductionEntries.Sort(PayPlanEdit.OrderDynamicPayPlanProductionEntries);
            var famCur = Patients.GetFamily(payPlan.PatNum);
            var principal = listPayPlanProductionEntries.Sum(x => x.AmountOverride == 0 ? x.AmountOriginal : x.AmountOverride);
            var payPlanTerms = new PayPlanTerms();
            payPlanTerms.APR = payPlan.APR;
            payPlanTerms.DateFirstPayment = payPlan.DatePayPlanStart;
            payPlanTerms.Frequency = payPlan.ChargeFrequency; //verify this is just based on the ui, not the db.
            payPlanTerms.DynamicPayPlanTPOption = payPlan.DynamicPayPlanTPOption;
            payPlanTerms.DateInterestStart = payPlan.DateInterestStart; //Will be DateTime.MinDate if field is blank.
            payPlanTerms.PayCount = 0; //Not used in PayPlanEdit.GetListExpectedCharges
            payPlanTerms.PeriodPayment = (decimal) payPlan.PayAmt;
            payPlanTerms.PrincipalAmount = (double) principal;
            payPlanTerms.RoundDec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits;
            payPlanTerms.DateAgreement = payPlan.PayPlanDate;
            payPlanTerms.DownPayment = payPlan.DownPayment;
            payPlanTerms.PaySchedule = PayPlanEdit.GetPayScheduleFromFrequency(payPlanTerms.Frequency);
            //now that terms are set, we need to potentially calculate the periodpayment amount since we only store that and not the payCount
            if (payPlanTerms.PayCount != 0)
            {
                payPlanTerms.PeriodPayment = PayPlanEdit.CalculatePeriodPayment(payPlanTerms.APR, payPlanTerms.Frequency, payPlanTerms.PeriodPayment, payPlanTerms.PayCount, payPlanTerms.RoundDec
                    , payPlanTerms.PrincipalAmount, payPlanTerms.DownPayment);
                payPlanTerms.PayCount = 0;
            }

            var listChargesExpected = PayPlanEdit.GetListExpectedCharges(listPayPlanCharges, payPlanTerms, famCur, listPayPlanLinks, payPlan, false);
            for (var i = 0; i < listPayPlanCharges.Count; i++) totalInterest += listPayPlanCharges[i].Interest;
            for (var i = 0; i < listChargesExpected.Count; i++) //combine with list expected.
                totalInterest += listChargesExpected[i].Interest;
            sheetPP.Parameters.Add(new SheetParameter(true, "Principal") {ParamValue = principal.ToString("n")});
            sheetPP.Parameters.Add(new SheetParameter(true, "totalFinanceCharge") {ParamValue = totalInterest});
            sheetPP.Parameters.Add(new SheetParameter(true, "totalCostOfLoan") {ParamValue = (principal + (decimal) totalInterest).ToString("n")});
        }
        else
        {
            //The math for this comes from FormPayPlan
            sheetPP.Parameters.Add(new SheetParameter(true, "Principal") {ParamValue = totalPrincipal.ToString("n")});
            sheetPP.Parameters.Add(new SheetParameter(true, "totalFinanceCharge") {ParamValue = totalInterest});
            sheetPP.Parameters.Add(new SheetParameter(true, "totalCostOfLoan") {ParamValue = totPrincIntAdj.ToString("n")});
        }

        SheetFiller.FillFields(sheetPP);
        return sheetPP;
    }

    ///<summary>Sets every PayPlan MobileAppDeviceNum to 0 if it matches the passed in mobileAppDeviceNum.</summary>
    public static void RemoveMobileAppDeviceNum(long mobileAppDeviceNum)
    {
        var command = $@"
				UPDATE payplan
				SET MobileAppDeviceNum=0
				WHERE MobileAppDeviceNum={mobileAppDeviceNum}";
        Db.NonQ(command);
    }

    public static bool TrySignPaymentPlan(PayPlan payPlan, string signaturePatient, out string error)
    {
        if (!TryValidateSignatures(payPlan, signaturePatient, out var patientSignature, out error)) return false;
        UpdatePaymentPlanSignatures(payPlan, patientSignature);
        return true;
    }

    /// <summary>
    ///     Returns true if given payPlan and signatures are valid for DB, provides decrypted signatures when true.
    ///     Otherwise returns false and sets out error.
    /// </summary>
    public static bool TryValidateSignatures(PayPlan payPlan, string signaturePatient, out string patientSignature, out string error)
    {
        error = null;
        patientSignature = null;
        if (payPlan == null) error = "This Payment Plan no longer exists. Please select and sign a new Payment Plan and try again.";
        var keyData = GetKeyDataForSignature(payPlan);
        var hash = MD5.Hash(Encoding.UTF8.GetBytes(keyData));
        //331 and 79 are the width and height of the signature box in FormTPsign.cs
        patientSignature = SigBox.EncryptSigString(hash, TreatPlans.GetScaledSignature(signaturePatient));
        if (patientSignature.IsNullOrEmpty()) error = "Error occurred when encrypting the patient signature.";
        return error.IsNullOrEmpty();
    }

    ///<summary>Used to set or clear out the mobile app device the payment plan is being added or removed from.</summary>
    public static void UpdateMobileAppDeviceNum(PayPlan payPlan, long mobileAppDeviceNum)
    {
        payPlan.MobileAppDeviceNum = mobileAppDeviceNum;
        Update(payPlan);
    }

    ///<summary>Updates the given payPlans signatures in the DB.</summary>
    public static void UpdatePaymentPlanSignatures(PayPlan payPlan, string patientSignature)
    {
        payPlan.Signature = patientSignature;
        Update(payPlan);
    }

    #endregion Xam Methods
}