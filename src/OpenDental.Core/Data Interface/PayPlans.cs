using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using ODCrypt;
using OpenDentBusiness.Misc;

namespace OpenDentBusiness;

public class PayPlans
{
    public static void Delete(PayPlan plan)
    {
        string command;
        if (plan.PlanNum == 0 || plan.IsDynamic)
        {
            //Patient payment plan
            command = "SELECT COUNT(*) FROM paysplit WHERE PayPlanNum=" + plan.PayPlanNum;
            if (Db.GetCount(command) != "0")
                throw new ApplicationException
                    (Lans.g("PayPlans", "You cannot delete a payment plan with patient payments attached.  Unattach the payments first."));
        }
        else
        {
            //Insurance payment plan
            command = "SELECT COUNT(*) FROM claimproc WHERE PayPlanNum=" + plan.PayPlanNum + " AND claimproc.Status IN ("
                      + SOut.Int((int) ClaimProcStatus.Received) + "," + SOut.Int((int) ClaimProcStatus.Supplemental) + ")";
            if (Db.GetCount(command) != "0")
                throw new ApplicationException
                    (Lans.g("PayPlans", "You cannot delete a payment plan with insurance payments attached.  Unattach the payments first."));
            //if there are any unreceived items, detach them here, then proceed deleting
            var listClaimProcs = ClaimProcs.GetForPayPlans([plan.PayPlanNum]);
            foreach (var claimProc in listClaimProcs)
            {
                claimProc.PayPlanNum = 0;
                ClaimProcs.Update(claimProc);
            }
        }

        command = "DELETE FROM payplancharge WHERE PayPlanNum=" + plan.PayPlanNum;
        Db.NonQ(command);
        command = $"DELETE FROM payplanlink WHERE PayPlanNum={plan.PayPlanNum}";
        Db.NonQ(command);
        command = "DELETE FROM payplan WHERE PayPlanNum =" + plan.PayPlanNum;
        Db.NonQ(command);
        command = $"DELETE FROM orthoplanlink WHERE orthoplanlink.FKey={plan.PayPlanNum} " +
                  $"AND orthoplanlink.LinkType IN ({SOut.Enum(OrthoPlanLinkType.PatPayPlan)}," +
                  $"{SOut.Enum(OrthoPlanLinkType.InsPayPlan)})";
        Db.NonQ(command);
        CreditCards.RemoveRecurringCharges(plan.PayPlanNum);
    }

    public static int GetDependencyCount(long patNum)
    {
        var command = "SELECT COUNT(*) FROM payplan"
                      + " WHERE PatNum = " + patNum
                      + " OR Guarantor = " + patNum;
        return SIn.Int(DataCore.GetScalar(command));
    }

    public static PayPlan GetOne(long payPlanNum)
    {
        return PayPlanCrud.SelectOne(payPlanNum);
    }

    public static List<PayPlan> GetMany(params long[] arrayPayPlanNums)
    {
        if (arrayPayPlanNums.IsNullOrEmpty()) return [];

        var command = $"SELECT * FROM payplan WHERE PayPlanNum IN ({string.Join(",", arrayPayPlanNums.Select(x => x))})";
        return PayPlanCrud.SelectMany(command);
    }

    public static List<PayPlan> GetForPats(List<long> listPatNums, long guarantor)
    {
        //We have to check for guarantor separately in case the payment plan belongs to a patient in another family.
        var command = "SELECT * FROM payplan WHERE Guarantor=" + guarantor;
        if (!listPatNums.IsNullOrEmpty()) command += " OR PatNum IN(" + string.Join(",", listPatNums) + ")";
        return PayPlanCrud.SelectMany(command);
    }

    public static List<PayPlan> GetAllPatPayPlansForPats(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return [];

        var command = $"SELECT * FROM payplan WHERE payplan.PatNum IN({string.Join(",", listPatNums)}) AND payplan.InsSubNum=0";
        return PayPlanCrud.SelectMany(command);
    }

    public static List<PayPlan> GetForPatNum(long patNum)
    {
        var command = "SELECT * FROM payplan "
                      + "WHERE PatNum = " + patNum + " "
                      + "OR Guarantor = " + patNum;
        return PayPlanCrud.SelectMany(command);
    }

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

    public static List<PayPlan> GetValidPlansNoIns(long guarNum)
    {
        var command = "SELECT * FROM payplan"
                      + " WHERE Guarantor = " + guarNum
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

    public static List<PayPlan> GetAllValidInsPayPlansForClaims(List<Claim> listClaims)
    {
        if (listClaims.IsNullOrEmpty()) return [];

        var command = "SELECT payplan.* FROM payplan "
                      + "LEFT JOIN claimproc ON claimproc.PayPlanNum=payplan.PayPlanNum	"
                      //Only ins payplans
                      + "WHERE payplan.PlanNum!=0 "
                      //Only ones for patients from the list of claims.
                      + $"AND payplan.PatNum IN ({string.Join(",", listClaims.Select(x => x.PatNum))}) "
                      //Only ones with no claimprocs attached or only claimprocs from the list of claims.
                      + $"AND (claimproc.ClaimNum IS NULL OR claimproc.ClaimNum IN ({string.Join(",", listClaims.Select(x => x.ClaimNum))})) "
                      + "GROUP BY payplan.PayPlanNum "
                      //Only ones that are not fully paid off.
                      + "HAVING payplan.CompletedAmt>SUM(COALESCE(claimproc.InsPayAmt,0)) "
                      + "ORDER BY payplan.PayPlanDate";
        return PayPlanCrud.SelectMany(command);
    }

    public static List<PayPlan> GetValidInsPayPlans(long patNum, long planNum, long insSubNum, long claimNum)
    {
        var command = "";
        command += "SELECT payplan.*,MAX(claimproc.ClaimNum) ClaimNum";
        command += " FROM payplan"
                   + " LEFT JOIN claimproc ON claimproc.PayPlanNum=payplan.PayPlanNum"
                   + " WHERE payplan.PatNum=" + patNum
                   + " AND payplan.PlanNum=" + planNum
                   + " AND payplan.InsSubNum=" + insSubNum;
        if (claimNum > 0) command += " AND (claimproc.ClaimNum IS NULL OR claimproc.ClaimNum=" + claimNum + ")"; //payplans with no claimprocs attached or only claimprocs from the same claim
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

    public static double GetTxTotalAmt(List<PayPlanCharge> listCharges)
    {
        if (listCharges.IsNullOrEmpty()) return 0;
        return listCharges.Where(x => x.ChargeType == PayPlanChargeType.Credit)
            .Sum(x => x.Principal);
    }

    public static double GetAmtPaid(PayPlan payPlan)
    {
        string command;
        if (payPlan.PlanNum == 0) //Patient payment plan
            command = "SELECT SUM(paysplit.SplitAmt) FROM paysplit "
                      + "WHERE paysplit.PayPlanNum = " + payPlan.PayPlanNum + " "
                      + "GROUP BY paysplit.PayPlanNum";
        else //Insurance payment plan
            command = "SELECT SUM(claimproc.InsPayAmt) "
                      + "FROM claimproc "
                      + "WHERE claimproc.Status IN(" + SOut.Int((int) ClaimProcStatus.Received) + "," + SOut.Int((int) ClaimProcStatus.Supplemental) + ","
                      + SOut.Int((int) ClaimProcStatus.CapClaim) + ") "
                      + "AND claimproc.PayPlanNum=" + payPlan.PayPlanNum;
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return 0;
        return SIn.Double(table.Rows[0][0].ToString());
    }

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
        if (listCharge.Count == 0) return [];
        var command = "SELECT * FROM payplan "
                      + "WHERE PayPlanNum IN (" + string.Join(",", listCharge.Select(x => x.PayPlanNum)) + ")";
        return PayPlanCrud.SelectMany(command);
    }
    
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

    public static string GetHashStringForSignature(string str)
    {
        return Encoding.ASCII.GetString(MD5.Hash(Encoding.UTF8.GetBytes(str)));
    }

    public static long Insert(PayPlan payPlan)
    {
        payPlan.SecurityHash = HashFields(payPlan);
        return PayPlanCrud.Insert(payPlan);
    }

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
    
    public static string GetKeyDataStringForSignature(string APR, string numberOfPayments, string paymentAmt, string freqOfPayments, string patName, string guarName, string sheetDefNum)
    {
        var strb = new StringBuilder();
        strb.Append(APR);
        strb.Append(numberOfPayments);
        strb.Append(paymentAmt);
        strb.Append(freqOfPayments);
        strb.Append(patName);
        strb.Append(guarName);
        if (sheetDefNum != "0") strb.Append(sheetDefNum);
        return strb.ToString();
    }

    public static string GetTermsAndConditionsString(PayPlan plan, bool isHtmlEmail = false)
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
                  + "HAVING Princ+Interest <= (TotPay + InsPay) AND LastDate <=" + "CURDATE()";
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
}