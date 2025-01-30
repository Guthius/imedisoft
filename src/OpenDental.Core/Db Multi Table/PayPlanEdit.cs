using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness
{
    public class PayPlanEdit
    {
        public static PayPlanCharge CreateDebitCharge(PayPlan payplan, Family famCur, long provNum, long clinicNum, double principalAmt, double interestAmt, DateTime dateCharge, string note)
        {
            var ppCharge = new PayPlanCharge();
            ppCharge.PayPlanNum = payplan.PayPlanNum;
            //FamCur is the family of the patient, so check to see if the guarantor is in the patient's family. 
            //If the guar and pat are in the same family, then use the patnum. else, use guarantor.
            if (famCur.ListPats.Select(x => x.PatNum).Contains(payplan.Guarantor))
            {
                ppCharge.Guarantor = payplan.PatNum;
            }
            else
            {
                ppCharge.Guarantor = payplan.Guarantor;
            }

            ppCharge.PatNum = payplan.PatNum;
            ppCharge.ChargeDate = dateCharge;
            ppCharge.Interest = interestAmt;
            ppCharge.Principal = principalAmt;
            ppCharge.Note = note;
            ppCharge.ChargeType = PayPlanChargeType.Debit;
            ppCharge.ProvNum = provNum;
            ppCharge.ClinicNum = clinicNum;
            return ppCharge;
        }

        public static PayPlanCharge CreateDebitChargeDynamic(PayPlan payplan, Family famCur, long provNum, long clinicNum, double principalAmt, double interestAmt, DateTime dateCharge, string note, long fKey, PayPlanLinkType linkType)
        {
            var ppCharge = CreateDebitCharge(payplan, famCur, provNum, clinicNum, principalAmt, interestAmt, dateCharge, note);
            ppCharge.FKey = fKey;
            ppCharge.LinkType = linkType;
            return ppCharge;
        }

        public static List<PayPlanCharge> CreatePayPlanAdjustments(double negAdjAmt, List<PayPlanCharge> listPayPlanCharges, double totalNegFutureAdjs)
        {
            var moneyToAllocate = (totalNegFutureAdjs + negAdjAmt) * -1; //the total amount of ALL future adjustments, existing + new. 
            //Get a list of all our charges for this payplan.
            var listRemainingCharges = listPayPlanCharges.FindAll(x => x.ChargeType == PayPlanChargeType.Debit && x.Principal >= 0)
                .OrderByDescending(x => x.ChargeDate).ToList();
            //Get a list of all current adjustments for the payment plan. (Existing before the newly added adjustment).
            var listCurAdjs = listPayPlanCharges.FindAll(x => x.ChargeType == PayPlanChargeType.Debit && x.Principal < 0
                                                                                                      && x.ChargeDate > DateTime_.Today);
            //Remove all the adjustments so we can more easily move money around and redistribute if necessary.
            listPayPlanCharges.RemoveAll(x => x.ChargeType == PayPlanChargeType.Debit && x.Principal < 0 && x.ChargeDate > DateTime_.Today);
            foreach (var charge in listRemainingCharges)
            {
                var chargeAmtRemaining = (charge.Principal); //The amount that can still be adjusted off of this charge, how much is left.

                #region Re-allocate existing adjustments

                var listExistNegAdjsForCharge = listCurAdjs.FindAll(x => x.ChargeDate.Date == charge.ChargeDate.Date)
                    .OrderBy(x => x.ChargeDate).ToList();
                if (listExistNegAdjsForCharge.Count != 0)
                {
                    //There exists adjustments for the currrent charge/debit row.
                    var existingNegAdjTotal = listExistNegAdjsForCharge.Sum(x => x.Principal);
                    if (existingNegAdjTotal * (-1) == chargeAmtRemaining)
                    {
                        //all of these adjustments apply to this charge. Add them all back, no need to keep looping through the adjs.
                        listPayPlanCharges.AddRange(listExistNegAdjsForCharge);
                        moneyToAllocate -= chargeAmtRemaining;
                        continue;
                    }

                    //first loop through the existing adjustments and see what can be applied
                    foreach (var existingAdj in listExistNegAdjsForCharge)
                    {
                        if (chargeAmtRemaining == 0)
                        {
                            //All adjustments have been allocated for this charge.
                            break;
                        }
                        else if (moneyToAllocate < existingAdj.Principal * -1)
                        {
                            //Remaining amount is less then the current adjustment
                            //We won't add this adjustment back in but we will create a new adjustment down below to account for this plus the new adj we are adding.
                            continue;
                        }

                        if (existingAdj.Principal * (-1) <= chargeAmtRemaining)
                        {
                            listPayPlanCharges.Add(existingAdj);
                            chargeAmtRemaining -= existingAdj.Principal * (-1);
                            moneyToAllocate -= existingAdj.Principal * -1;
                        }
                        else
                        {
                            //adjustment is bigger that amt avaialable to be applied. Keep removed, add amount back into the total make a new adjustment later.
                            moneyToAllocate += existingAdj.Principal * -1;
                        }
                        //Leave adjustment gone, only allocate for what we have remaining. 
                    }
                }

                #endregion

                if (CompareDouble.IsEqual(moneyToAllocate, 0))
                {
                    break;
                }

                var ppc = new PayPlanCharge();
                ppc.ChargeDate = charge.ChargeDate;
                ppc.PatNum = charge.PatNum;
                ppc.PayPlanNum = charge.PayPlanNum;
                ppc.Guarantor = charge.Guarantor;
                ppc.ProvNum = charge.ProvNum;
                ppc.ProcNum = charge.ProcNum;
                ppc.Note = "";
                ppc.ChargeType = PayPlanChargeType.Debit;
                if (moneyToAllocate <= chargeAmtRemaining)
                {
                    //Balance is equal to adjustment, apply whole adj amt to charge and be done.
                    //Or adj won't cover the whole remaining charge, apply what we can and be done. 
                    ppc.Principal = moneyToAllocate * -1;
                    listPayPlanCharges.Add(ppc);
                    break; //moneyToAllocate should be 0 here. No need to keep looping. 
                }
                else
                {
                    //Entire principal can be allocated to this adjustment. Move money to the principal for entire charge amount and keep looping. 
                    ppc.Principal = chargeAmtRemaining * -1;
                    moneyToAllocate -= chargeAmtRemaining;
                    listPayPlanCharges.Add(ppc);
                }
            }

            return listPayPlanCharges;
        }

        public static double CalcPeriodRate(double APR, PayPlanFrequency frequency)
        {
            double periodRate;
            if (APR == 0)
            {
                periodRate = 0;
            }
            else
            {
                if (frequency == PayPlanFrequency.Weekly)
                {
                    periodRate = APR / 100 / 52;
                }
                else if (frequency == PayPlanFrequency.EveryOtherWeek)
                {
                    periodRate = APR / 100 / 26;
                }
                else if (frequency == PayPlanFrequency.OrdinalWeekday)
                {
                    periodRate = APR / 100 / 12;
                }
                else if (frequency == PayPlanFrequency.Monthly)
                {
                    periodRate = APR / 100 / 12;
                }
                else
                {
                    //quarterly
                    periodRate = APR / 100 / 4;
                }
            }

            return periodRate;
        }

        public static DateTime CalcNextPeriodDate(DateTime firstDate, int periodNum, PayPlanFrequency frequency)
        {
            var retVal = DateTime_.Today;
            if (frequency == PayPlanFrequency.Weekly)
            {
                retVal = firstDate.AddDays(7 * periodNum);
            }
            else if (frequency == PayPlanFrequency.EveryOtherWeek)
            {
                retVal = firstDate.AddDays(14 * periodNum);
            }
            else if (frequency == PayPlanFrequency.OrdinalWeekday)
            {
                //First/second/etc Mon/Tue/etc of month
                var roughMonth = firstDate.AddMonths(1 * periodNum); //this just gets us into the correct month and year
                var dayOfWeekFirstDate = firstDate.DayOfWeek;
                //find the starting point for the given month: the first day that matches day of week
                var dayOfWeekFirstMonth = (new DateTime(roughMonth.Year, roughMonth.Month, 1)).DayOfWeek;
                if (dayOfWeekFirstMonth == dayOfWeekFirstDate)
                {
                    //1st is the proper day of the week
                    retVal = new DateTime(roughMonth.Year, roughMonth.Month, 1);
                }
                else if (dayOfWeekFirstMonth < dayOfWeekFirstDate)
                {
                    //Example, 1st is a Tues (2), but we need to start on a Thursday (4)
                    retVal = new DateTime(roughMonth.Year, roughMonth.Month, dayOfWeekFirstDate - dayOfWeekFirstMonth + 1); //4-2+1=3.  The 3rd is a Thursday
                }
                else
                {
                    //Example, 1st is a Thursday (4), but we need to start on a Monday (1) 
                    retVal = new DateTime(roughMonth.Year, roughMonth.Month, 7 - (dayOfWeekFirstMonth - dayOfWeekFirstDate) + 1); //7-(4-1)+1=5.  The 5th is a Monday
                }

                var ordinalOfMonth = GetOrdinalOfMonth(firstDate); //for example 3 if it's supposed to be the 3rd Friday of each month
                retVal = retVal.AddDays(7 * (ordinalOfMonth - 1)); //to get to the 3rd Friday, and starting from the 1st Friday, we add 2 weeks.
            }
            else if (frequency == PayPlanFrequency.Monthly)
            {
                retVal = firstDate.AddMonths(1 * periodNum);
            }
            else
            {
                //quarterly
                retVal = firstDate.AddMonths(3 * periodNum);
            }

            return retVal;
        }

        public static int GetOrdinalOfMonth(DateTime date)
        {
            if (date.AddDays(-28).Month == date.Month)
            {
                return 4; //treat a 5 like a 4
            }
            else if (date.AddDays(-21).Month == date.Month)
            {
                //4
                return 4;
            }
            else if (date.AddDays(-14).Month == date.Month)
            {
                return 3;
            }

            if (date.AddDays(-7).Month == date.Month)
            {
                return 2;
            }

            return 1;
        }

        public static DataRow CreateRowForPayPlanChargeDT(DataTable table, PayPlanCharge payPlanCharge, int payPlanChargeOrdinal, bool isDynamic, bool hasExpectedChargesAwaitingCompletion = false)
        {
            var retVal = table.NewRow();
            var chargeDate = payPlanCharge.ChargeDate.ToShortDateString();
            var interestAmt = payPlanCharge.Interest.ToString("f");
            var paymentDue = (payPlanCharge.Principal + payPlanCharge.Interest).ToString("n");
            var descript = "#" + (payPlanChargeOrdinal + 1);
            //payPlanCharge.FKey will be zero if payPlanCharge represents an aggregate row.
            if (isDynamic && payPlanCharge.LinkType == PayPlanLinkType.Procedure && payPlanCharge.FKey != 0)
            {
                var curProc = Procedures.GetOneProc(payPlanCharge.FKey, false);
                //If Procedure has a TP status and the payPlanCharge.ChargeDate is MaxValue, the Dynamic Payment Plan is awaiting completion.
                if (payPlanCharge.ChargeDate == DateTime.MaxValue && (curProc.ProcStatus == ProcStat.TP || curProc.ProcStatus == ProcStat.TPi))
                {
                    chargeDate = "TBD";
                    interestAmt = "TBD";
                    paymentDue = "TBD";
                    descript = "#-";
                }

                var curProcCode = ProcedureCodes.GetProcCodeFromDb(curProc.CodeNum);
                if (curProcCode != null)
                {
                    descript += " " + curProcCode.ProcCode;
                    if (curProcCode.AbbrDesc != "")
                    {
                        descript += " - " + curProcCode.AbbrDesc;
                    }
                }
            }

            if (!payPlanCharge.Note.IsNullOrEmpty())
            {
                descript += " " + payPlanCharge.Note;
                //Don't add a # if it's a recalculated charge because they aren't "true" payplan charges.
                if (payPlanCharge.Note.Trim().ToLower().Contains("recalculated based on"))
                {
                    descript = payPlanCharge.Note;
                }
            }

            retVal["ChargeDate"] = chargeDate; //0 Date
            if (hasExpectedChargesAwaitingCompletion || !isDynamic)
            {
                retVal["Provider"] = Providers.GetAbbr(payPlanCharge.ProvNum); //1 Prov Abbr
            }

            retVal["Description"] = descript; //2 Descript
            if (payPlanCharge.Principal < 0)
            {
                //it's a patient payment plan adjustment (non-dynamic)
                retVal["Principal"] = "";
                retVal["Interest"] = ""; //4 Interest
                retVal["Due"] = ""; //5 Due
                retVal["Payment"] = ""; //6 Payment (filled later)
                if (!isDynamic)
                {
                    retVal["Adjustment"] = payPlanCharge.Principal.ToString("f"); //7
                }
            }
            else
            {
                retVal["Principal"] = payPlanCharge.Principal.ToString("f"); //3 Principal
                retVal["Interest"] = interestAmt; //4 Interest
                retVal["Due"] = paymentDue; //5 Due
                retVal["Payment"] = ""; //6 Payment (filled later)
                if (!isDynamic)
                {
                    retVal["Adjustment"] = 0.ToString("f"); //7
                }
            }

            retVal["Balance"] = ""; //8 Balance (filled later)
            if (payPlanCharge.Principal < 0)
            {
                retVal["Type"] = PayPlanEntryType.adjustment.ToString();
            }
            else
            {
                retVal["Type"] = PayPlanEntryType.charge.ToString();
            }

            return retVal;
        }

        public static DataRow CreateRowForPayPlanListDT(DataTable table, DataRow payPlanList, int payPlanListOrdinal, bool isDynamic)
        {
            var retVal = table.NewRow();
            string descript;
            var note = payPlanList[2].ToString().Trim().ToLower();
            if (note.Contains("downpayment") || note.Contains("down payment"))
            {
                //description
                descript = payPlanList[2].ToString();
            }
            else if (note.Contains("increased interest:"))
            {
                //description
                descript = "#" + (payPlanListOrdinal + 1) + " Increased Interest:";
            }
            else if (SIn.Double(payPlanList[6].ToString()) > 0)
            {
                //payment
                descript = payPlanList[2].ToString();
            }
            else if (payPlanList[2].ToString().StartsWith("#-"))
            {
                descript = payPlanList[2].ToString().Replace("#-", "Planned - ");
            }
            else if (payPlanList[2].ToString().StartsWith("#"))
            {
                //only pay plan charges start with #
                var strBuilder = new StringBuilder(payPlanList[2].ToString());
                StringTools.RegReplace(strBuilder, "^#[0-9]+", "");
                descript = "#" + (payPlanListOrdinal + 1) + strBuilder;
            }
            else
            {
                descript = "#" + (payPlanListOrdinal + 1);
            }

            retVal[0] = payPlanList[0].ToString(); //0 Date
            retVal[1] = payPlanList[1].ToString(); //1 Prov Abbr
            retVal[2] = descript; //2 Descript
            retVal[3] = payPlanList[3].ToString(); //3 Principal
            retVal[4] = payPlanList[4].ToString(); //4 Interest
            retVal[5] = payPlanList[5].ToString(); //5 Due
            retVal[6] = payPlanList[6].ToString(); //6 Payment (filled later)
            if (!isDynamic)
            {
                retVal[7] = payPlanList[7].ToString(); //7 adjustment
            }

            retVal[8] = payPlanList[8].ToString(); //8 Balance (filled later)
            return retVal;
        }

        public static DataRow CreateRowForPayPlanSplitDT(DataTable table, PaySplit payPlanSplit, DataRow rowBundlePayment, bool isDynamic)
        {
            var retVal = table.NewRow();
            var descript = "";
            if (isDynamic)
            {
                descript = "Payment";
            }
            else
            {
                descript = Defs.GetName(DefCat.PaymentTypes, SIn.Long(rowBundlePayment["PayType"].ToString()));
                if (rowBundlePayment["CheckNum"].ToString() != "")
                {
                    descript += " #" + rowBundlePayment["CheckNum"].ToString();
                }

                descript += " " + payPlanSplit.SplitAmt.ToString("c");
                if (SIn.Double(rowBundlePayment["PayAmt"].ToString()) != payPlanSplit.SplitAmt)
                {
                    descript += Lans.g("FormPayPlan", "(split)");
                }
            }

            retVal["ChargeDate"] = payPlanSplit.DatePay.ToShortDateString(); //0 Date
            if (!isDynamic)
            {
                retVal["Provider"] = Providers.GetAbbr(SIn.Long(rowBundlePayment["ProvNum"].ToString())); //1 Prov Abbr
            }

            retVal["Description"] = descript; //2 Descript
            retVal["Principal"] = 0.ToString("f"); //3 Principal
            retVal["Interest"] = 0.ToString("f"); //4 Interest 
            retVal["Due"] = 0.ToString("f"); //5 Due (filled later)
            retVal["Payment"] = payPlanSplit.SplitAmt.ToString("f"); // Payment
            if (!isDynamic)
            {
                retVal["Adjustment"] = 0.ToString("f"); //7 Adjustment
            }

            retVal["Balance"] = (""); //8 Balance (filled later)
            retVal["Type"] = PayPlanEntryType.pay.ToString();
            return retVal;
        }

        public enum PayPlanEntryType
        {
            charge,
            adjustment,
            pay
        }

        public static DataRow CreateRowForClaimProcsDT(DataTable table, DataRow rowBundleClaimProc, bool isDynamic)
        {
            //Either a claimpayment or a bundle of claimprocs with no claimpayment that were on the same date.
            var retVal = table.NewRow();
            var descript = Defs.GetName(DefCat.InsurancePaymentType, SIn.Long(rowBundleClaimProc["PayType"].ToString()));
            if (rowBundleClaimProc["CheckNum"].ToString() != "")
            {
                descript += " #" + rowBundleClaimProc["CheckNum"];
            }

            if (SIn.Long(rowBundleClaimProc["ClaimPaymentNum"].ToString()) == 0)
            {
                descript += "No Finalized Payment";
            }
            else
            {
                var checkAmt = SIn.Double(rowBundleClaimProc["CheckAmt"].ToString());
                descript += " " + checkAmt.ToString("c");
                var insPayAmt = SIn.Double(rowBundleClaimProc["InsPayAmt"].ToString());
                if (checkAmt != insPayAmt)
                {
                    descript += " " + Lans.g("FormPayPlan", "(split)");
                }
            }

            retVal["ChargeDate"] = SIn.DateTime(rowBundleClaimProc["DateCP"].ToString()).ToShortDateString(); //0 Date
            retVal["Provider"] = Providers.GetLName(SIn.Long(rowBundleClaimProc["ProvNum"].ToString())); //1 Prov Abbr
            retVal["Description"] = descript; //2 Descript
            retVal["Principal"] = ""; //3 Principal
            retVal["Interest"] = ""; //4 Interest
            retVal["Due"] = ""; //5 Due
            retVal["Payment"] = SIn.Double(rowBundleClaimProc["InsPayAmt"].ToString()).ToString("n"); // Payment
            if (!isDynamic)
            {
                retVal["Adjustment"] = ""; //7
            }

            retVal["Balance"] = (""); //8 Balance (filled later)
            retVal["Type"] = PayPlanEntryType.pay.ToString();
            return retVal;
        }

        public static int ComparePayPlanRowsDT(DataRow x, DataRow y)
        {
            var dateTimeX = SIn.Date(x["ChargeDate"].ToString());
            var dateTimeY = SIn.Date(y["ChargeDate"].ToString());
            if (dateTimeX.Date != dateTimeY.Date)
            {
                return dateTimeX.CompareTo(dateTimeY); // sort by date
            }

            var xIsRecalc = x["Description"].ToString().ToLower().Contains("recalculated based on");
            var yIsRecalc = y["Description"].ToString().ToLower().Contains("recalculated based on");
            if (xIsRecalc != yIsRecalc)
            {
                return xIsRecalc.CompareTo(yIsRecalc); // recalculated charges to the bottom of the current date.
            }

            if (xIsRecalc && yIsRecalc)
            {
                return SIn.Double(x["Principal"].ToString()).CompareTo(SIn.Double(y["Principal"].ToString())); // sort by principal amounts if both are recalculated charges
            }

            if (x["Type"].ToString() != y["Type"].ToString())
            {
                return x["Type"].ToString().CompareTo(y["Type"].ToString()); //charges first; I.e. "charge".CompareTo("pay") will return charge first
            }

            return x["Description"].ToString().CompareTo(y["Description"].ToString()); //Sort by description. 
        }

        public static int CompareDynamicPayPlanRowsDT(DataRow x, DataRow y)
        {
            var dateTimeX = SIn.Date(x["ChargeDate"].ToString());
            var dateTimeY = SIn.Date(y["ChargeDate"].ToString());
            if (x["chargeDate"].ToString() == "TBD")
            {
                dateTimeX = DateTime.MaxValue;
            }

            if (y["ChargeDate"].ToString() == "TBD")
            {
                dateTimeY = DateTime.MaxValue;
            }

            if (dateTimeX.Date != dateTimeY.Date)
            {
                return dateTimeX.CompareTo(dateTimeY); // sort by date
            }

            if (x["Type"].ToString() != y["Type"].ToString())
            {
                return x["Type"].ToString().CompareTo(y["Type"].ToString()); //charges first; I.e. "charge".CompareTo("pay") will return charge first
            }

            return 0;
        }

        public static int OrderDynamicPayPlanProductionEntries(PayPlanProductionEntry x, PayPlanProductionEntry y)
        {
            //Always place treatment planned procedures last.
            if (x.LinkType == PayPlanLinkType.Procedure && !x.IsProcedureCompleted &&
                (y.LinkType != PayPlanLinkType.Procedure || y.IsProcedureCompleted))
            {
                return 1;
            }
            else if (y.LinkType == PayPlanLinkType.Procedure && !y.IsProcedureCompleted &&
                     (x.LinkType != PayPlanLinkType.Procedure || x.IsProcedureCompleted))
            {
                return -1;
            }

            var dateTimeX = SIn.Date(x.ProductionDate.ToString());
            var dateTimeY = SIn.Date(y.ProductionDate.ToString());
            if (dateTimeX != dateTimeY)
            {
                return dateTimeX.CompareTo(dateTimeY); // sort by date
            }

            //We want to charge out OrthoCases first, then Procedures, then Adjustments.
            //If new production types are ever made, they should have lower priority.
            if (x.LinkType != y.LinkType)
            {
                // sort by production type
                if (x.LinkType <= PayPlanLinkType.OrthoCase && y.LinkType <= PayPlanLinkType.OrthoCase)
                {
                    return y.LinkType.CompareTo(x.LinkType);
                }
                else
                {
                    return x.LinkType.CompareTo(y.LinkType);
                }
            }

            return x.PriKey.CompareTo(y.PriKey); // sort by Primary Key
        }

        public static void AddRowsForDynamicPayPlanDT(DataTable dataTablePayPlan, PayPlan payPlan, List<PayPlanCharge> listPayPlanCharges, List<PaySplit> listPaySplits, bool hasExpectedChargesAwaitingCompletion)
        {
            var listPayPlanChargesMerged = new List<PayPlanCharge>();
            var listPaySplitsMerged = new List<PaySplit>();
            //Treatment planned procedures awaiting completion have a ChargeDate of DateTime.MaxValue.
            var listChargeDates = listPayPlanCharges.Select(x => x.ChargeDate).Distinct().ToList().FindAll(x => x != DateTime.MaxValue);
            for (var i = 0; i < listChargeDates.Count; i++)
            {
                var listPayPlanChargesForDate = listPayPlanCharges.FindAll(x => x.ChargeDate == listChargeDates[i]);
                var principal = listPayPlanChargesForDate.Sum(x => x.Principal);
                var interest = listPayPlanChargesForDate.Sum(x => x.Interest);
                var payplanChargeMerged = new PayPlanCharge();
                payplanChargeMerged.Principal = principal;
                payplanChargeMerged.Interest = interest;
                payplanChargeMerged.ChargeDate = listChargeDates[i];
                if (listPayPlanChargesForDate.Any(x => x.Note == "Down Payment"))
                {
                    payplanChargeMerged.Note = "Down payment";
                }

                listPayPlanChargesMerged.Add(payplanChargeMerged);
            }

            listPayPlanChargesMerged.AddRange(listPayPlanCharges.FindAll(x => x.ChargeDate == DateTime.MaxValue));
            var listPayPlanDataRows = new List<DataRow>();
            for (var i = 0; i < listPayPlanChargesMerged.Count; i++)
            {
                listPayPlanDataRows.Add(CreateRowForPayPlanChargeDT(dataTablePayPlan, listPayPlanChargesMerged[i], i, payPlan.IsDynamic, hasExpectedChargesAwaitingCompletion));
            }

            var listPayNums = listPaySplits.Select(x => x.PayNum).Distinct().ToList();
            for (var i = 0; i < listPayNums.Count; i++)
            {
                var listPaySplitsForPayment = listPaySplits.FindAll(x => x.PayNum == listPayNums[i]);
                var sumSplitAmt = listPaySplitsForPayment.Sum(x => x.SplitAmt);
                var paySplitMerged = new PaySplit();
                paySplitMerged.SplitAmt = sumSplitAmt;
                paySplitMerged.DatePay = listPaySplitsForPayment[0].DatePay;
                listPaySplitsMerged.Add(paySplitMerged);
            }

            //===payplan payments===
            var bundledPayments = PaySplits.GetForPayPlan(payPlan.PayPlanNum);
            for (var i = 0; i < listPaySplitsMerged.Count; i++)
            {
                listPayPlanDataRows.Add(CreateRowForPayPlanSplitDT(dataTablePayPlan, listPaySplitsMerged[i], bundledPayments.Rows[i], payPlan.IsDynamic));
            }

            //Sort rows based on date and type ===============================================================================
            listPayPlanDataRows.Sort(CompareDynamicPayPlanRowsDT);
            //Fill sorted data rows to sortRetVal DataTable ===============================================================================
            for (var i = 0; i < listPayPlanDataRows.Count; i++)
            {
                dataTablePayPlan.Rows.Add(listPayPlanDataRows[i]);
            }
        }

        public static PayPlanCharge CalculatePatPayPlanCloseoutCharge(List<PayPlanCharge> listPayPlanCharges, List<Procedure> listProceduresCredited, PayPlan payPlan, DateTime dateToday)
        {
            var listProcNumsTreatmentPlanned = listProceduresCredited
                .Where(x => x.ProcStatus != ProcStat.C)
                .Select(x => x.ProcNum)
                .ToList();
            var totalPrincipal = listPayPlanCharges.Where(x => x.ChargeType == PayPlanChargeType.Debit).Sum(x => x.Principal);
            var treatmentPlannedProcedureCredits = listPayPlanCharges
                .Where(x => listProcNumsTreatmentPlanned.Contains(x.ProcNum))
                .Sum(x => x.Principal);
            //We can't just use credits for completed procedures here because credits don't have to be attached to anything.
            var otherCredits = Math.Max(0, listPayPlanCharges
                .Where(x => !listProcNumsTreatmentPlanned.Contains(x.ProcNum) && x.ChargeType == PayPlanChargeType.Credit)
                .Sum(x => x.Principal));
            var futureChargesPrincipal = listPayPlanCharges
                .Where(x => x.ChargeType == PayPlanChargeType.Debit && x.ChargeDate > dateToday)
                .Sum(x => x.Principal);
            //Treatment planned procedure credits will be removed upon closing. We want to subtract them from the close out charge,
            //but we don't want to force the total of all charges to be less than the total of other credits.
            var amountToSubtract = Math.Min(treatmentPlannedProcedureCredits, totalPrincipal - otherCredits);
            //amountToSubtract could only be negative if we have a negative credit for a TP'd procedure or more other credits than principal.
            //Both scenarios might be possible but aren't practical, so let's avoid them.
            amountToSubtract = Math.Max(0, amountToSubtract);
            var closeoutChargeAmount = futureChargesPrincipal - amountToSubtract;
            return new PayPlanCharge()
            {
                PayPlanNum = payPlan.PayPlanNum,
                Guarantor = payPlan.PatNum, //the closeout charge should always appear on the patient of the payment plan.
                PatNum = payPlan.PatNum,
                ChargeDate = dateToday,
                Interest = 0,
                Principal = closeoutChargeAmount,
                Note = Lans.g("FormPayPlan", "Close Out Charge"),
                ChargeType = PayPlanChargeType.Debit,
            };
        }

        public static void CreateScheduleCharges(PayPlanTerms terms, PayPlan payPlan, Family fam, long provNum, long clinicNum, List<PayPlanCharge> listPayPlanCharges)
        {
            //Set variables
            const int payPlanChargesCeiling = 2000; //This is the maximum number of payplan charges allowed to be made plus 1.
            var periodRate = CalcPeriodRate(terms.APR, terms.Frequency);
            var principalDecrementingAmt = (decimal) (terms.PrincipalAmount - terms.DownPayment);
            var previousPrincipalDecrementingAmt = principalDecrementingAmt;
            var chargesCount = 0; //Not the same as listPayPlanCharges.Count
            //Add charge for down payment
            if (terms.DownPayment != 0)
            {
                listPayPlanCharges.Add(CreateDebitCharge(payPlan, fam, provNum, clinicNum, terms.DownPayment, 0, DateTime_.Today
                    , Lans.g("PayPlanEdit", "Down payment")));
            }

            //Add charges
            while (CompareDecimal.IsGreaterThanZero(principalDecrementingAmt) && chargesCount < payPlanChargesCeiling)
            {
                //the ceiling prevents infinite loop
                //If principal is increasing, terms are invalid, and we must clear the list of charges and return.
                if (decimal.Compare(principalDecrementingAmt, previousPrincipalDecrementingAmt) > 0)
                {
                    listPayPlanCharges.RemoveAll(x => x.ChargeType == PayPlanChargeType.Debit);
                    terms.AreTermsValid = false;
                    return;
                }

                previousPrincipalDecrementingAmt = principalDecrementingAmt;
                var ppCharge =
                    CreateDebitCharge(payPlan, fam, provNum, clinicNum, 0, 0, CalcNextPeriodDate(terms.DateFirstPayment, chargesCount, terms.Frequency), "");
                if (ppCharge.ChargeDate.Date >= terms.DateInterestStart.Date)
                {
                    ppCharge.Interest = Math.Round(((double) principalDecrementingAmt * periodRate), terms.RoundDec); //2 decimals
                }

                ppCharge.Principal = (double) terms.PeriodPayment - ppCharge.Interest;
                if ((terms.PayCount > 0 && chargesCount == (terms.PayCount - 1)) || (principalDecrementingAmt + (decimal) ppCharge.Interest <= terms.PeriodPayment))
                {
                    ppCharge.Principal = (double) principalDecrementingAmt;
                }

                principalDecrementingAmt -= (decimal) ppCharge.Principal;
                listPayPlanCharges.Add(ppCharge);
                chargesCount++;
            }
        }
        
        public static void RecalculateScheduleCharges(PayPlanTerms terms, PayPlanRecalculationData recalcData)
        {
            recalcData.OverpaidAmt = CreateChargeMatchOverpayment(recalcData);
            //if overpaid amount is being paid towards principal, we subtract from principal and set to zero.
            if (!recalcData.IsPrepay && recalcData.OverpaidAmt > 0)
            {
                recalcData.PrincipalRemaining -= (double) recalcData.OverpaidAmt;
                recalcData.OverpaidAmt = 0;
            }

            recalcData.OverpaidDecrementingAmt = (decimal) recalcData.OverpaidAmt;
            recalcData.PrincipalDecrementingAmt = (decimal) recalcData.PrincipalRemaining;
            const int payPlanChargesCeiling = 2000; //This is the maximum number of payplan charges allowed to be made plus 1. To prevent infinite loop.
            while (CompareDecimal.IsGreaterThanZero(recalcData.PrincipalDecrementingAmt) && recalcData.ListChargesToAdd.Count() < payPlanChargesCeiling)
            {
                var ppCharge = CreateDebitCharge(recalcData.PayPlan, recalcData.Fam, recalcData.ProvNum, recalcData.ClinicNum, 0, 0,
                    CalcNextPeriodDate(recalcData.FirstFutureChargeDate, recalcData.ListChargesToAdd.Count(), terms.Frequency), "");
                if (ppCharge.ChargeDate.Date >= terms.DateInterestStart.Date)
                {
                    ppCharge.Interest = CalcInterestForRecalc(recalcData, terms.RoundDec);
                }

                ppCharge.Principal = CalcPrincipalForRecalc(recalcData, ppCharge.Interest);
                var overPaidAmtApplied = ApplyPrepaymentForRecalc(recalcData, ppCharge, terms);
                //Interest can be zero if there is no APR, the charge is fully prepaid, or charge date is less than interest start date.
                if (ppCharge.Interest == 0)
                {
                    recalcData.ChargesWithInterestCount--; //Decrement so interest gets divided correctly for remaining charges when not recalculating interest.
                }

                HandleLastCharge(recalcData, ppCharge, terms);
                recalcData.OverpaidDecrementingAmt -= overPaidAmtApplied;
                recalcData.PrincipalDecrementingAmt -= overPaidAmtApplied;
                if (recalcData.PrincipalDecrementingAmt < 0)
                {
                    //If overpayment is greater than principal, a negative charge must be made to offset this.
                    ppCharge.Principal = (double) (0 - recalcData.OverpaidDecrementingAmt + recalcData.PrincipalDecrementingAmt);
                    ppCharge.Interest = 0;
                    recalcData.PrincipalDecrementingAmt = 0;
                }
                else
                {
                    recalcData.PrincipalDecrementingAmt -= (decimal) ppCharge.Principal;
                    recalcData.InterestUnpaidDecrementingAmt -= (decimal) ppCharge.Interest;
                }

                recalcData.ListChargesToAdd.Add(ppCharge);
            }

            HandleIncreasedInterest(recalcData, terms);
            //Remove old future charges and add new recalculated ones.
            recalcData.ListPayPlanCharges.RemoveAll(x => recalcData.ListFutureDebits.Contains(x));
            recalcData.ListPayPlanCharges.AddRange(recalcData.ListChargesToAdd);
        }

        private static double CreateChargeMatchOverpayment(PayPlanRecalculationData recalcData)
        {
            var pastSplitsTotal = recalcData.ListPaySplits.Where(x => x.DatePay <= DateTime_.Today).Sum(x => x.SplitAmt);
            var pastDebitsTotal = recalcData.ListPastDebits.Sum(x => x.Principal + x.Interest);
            var overpaidAmount = pastSplitsTotal - pastDebitsTotal;
            if (CompareDecimal.IsGreaterThanZero(overpaidAmount))
            {
                var note = "Recalculated based on pay on principal";
                if (recalcData.IsPrepay)
                {
                    note = "Recalculated based on prepayment";
                }

                recalcData.ListPayPlanCharges.Add(CreateDebitCharge(recalcData.PayPlan, recalcData.Fam, recalcData.ProvNum, recalcData.ClinicNum, overpaidAmount,
                    0, DateTime_.Today, Lans.g("PayPlanEdit", note)));
            }

            return overpaidAmount;
        }

        private static DateTime CalcNextChargeDateForRecalc(PayPlanFrequency frequency, PayPlanRecalculationData recalcData)
        {
            if (recalcData.ListFutureDebits.Count > 0)
            {
                return recalcData.ListFutureDebits.Min(x => x.ChargeDate);
            }
            else
            {
                //If we have no future charges, next charge date will be calculated from last charge on or before today.
                if (recalcData.ListPastDebits.Count > 0)
                {
                    return CalcNextPeriodDate(recalcData.ListPastDebits.Max(x => x.ChargeDate), 1, frequency);
                }
                else
                {
                    return CalcNextPeriodDate(DateTime_.Today, 1, frequency);
                }
            }
        }

        private static decimal CalcPeriodPaymentForRecalc(PayPlanTerms terms, PayPlanRecalculationData recalcData)
        {
            if (terms.PayCount == 0)
            {
                return terms.PeriodPayment;
            }

            var periodExactAmt = recalcData.PrincipalRemaining + recalcData.InterestFutureUnpaidAmt;
            if (recalcData.PaymentRemainingCount > 0)
            {
                if (terms.APR == 0)
                {
                    periodExactAmt = recalcData.PrincipalRemaining / recalcData.PaymentRemainingCount;
                }
                else if (!recalcData.IsRecalculateInterest)
                {
                    //APR applies, but the user wants to keep the old interest.
                    periodExactAmt = (recalcData.PrincipalRemaining + recalcData.InterestFutureUnpaidAmt) / recalcData.PaymentRemainingCount;
                }
                else
                {
                    //APR applies and the user wants to recalculate the interest.
                    periodExactAmt = recalcData.PrincipalRemaining * recalcData.PeriodRate / (1 - Math.Pow(1 + recalcData.PeriodRate, -recalcData.PaymentRemainingCount));
                }
            }

            //Round up to the nearest penny (or international equivalent).  
            //This causes the principal on the last payment to be less than or equal to the other principal amounts.
            return (decimal) (Math.Ceiling(periodExactAmt * Math.Pow(10, terms.RoundDec)) / Math.Pow(10, terms.RoundDec));
        }

        private static double CalcInterestForRecalc(PayPlanRecalculationData recalcData, int roundDec)
        {
            if (recalcData.IsRecalculateInterest)
            {
                //If recalculating interest, calculate as normal
                return Math.Round((double) recalcData.PrincipalDecrementingAmt * recalcData.PeriodRate, roundDec);
            }

            //Otherwise, divide the remaining interest amongst all future payments by payment count or period payment amount
            if (recalcData.PaymentRemainingCount > 0)
            {
                //Fully prepaid charges will have no interest
                return Math.Round(recalcData.InterestFutureUnpaidAmt / recalcData.ChargesWithInterestCount, roundDec);
            }

            return Math.Round(recalcData.InterestFutureUnpaidAmt /
                              ((recalcData.PrincipalRemaining + recalcData.InterestFutureUnpaidAmt) / (double) recalcData.PeriodPaymentAmt), roundDec);
        }

        private static double CalcPrincipalForRecalc(PayPlanRecalculationData recalcData, double interest)
        {
            if (recalcData.PrincipalDecrementingAmt < recalcData.PeriodPaymentAmt - (decimal) interest)
            {
                return (double) recalcData.PrincipalDecrementingAmt;
            }

            return (double) recalcData.PeriodPaymentAmt - interest;
        }

        private static decimal ApplyPrepaymentForRecalc(PayPlanRecalculationData recalcData, PayPlanCharge ppCharge, PayPlanTerms terms)
        {
            decimal retVal = 0;
            if (recalcData.OverpaidDecrementingAmt > 0)
            {
                //Set to zero if full charge is prepaid
                if ((double) recalcData.OverpaidDecrementingAmt >= ppCharge.Principal + ppCharge.Interest)
                {
                    //principal+interest always equals period pay amount.
                    retVal = (decimal) (ppCharge.Principal + ppCharge.Interest);
                    ppCharge.Principal = 0;
                    ppCharge.Interest = 0;
                    ppCharge.Note = Lans.g("PayPlanEdit", "Prepaid");
                }
                else
                {
                    ppCharge.Principal -= (double) recalcData.OverpaidDecrementingAmt;
                    if (recalcData.IsRecalculateInterest && ppCharge.ChargeDate.Date >= terms.DateInterestStart.Date)
                    {
                        ppCharge.Interest =
                            Math.Round(((double) (recalcData.PrincipalDecrementingAmt - recalcData.OverpaidDecrementingAmt) * recalcData.PeriodRate), terms.RoundDec);
                    }

                    retVal = recalcData.OverpaidDecrementingAmt;
                }
            }

            return retVal;
        }

        private static void HandleLastCharge(PayPlanRecalculationData recalcData, PayPlanCharge ppCharge, PayPlanTerms terms)
        {
            //check to see if this charge is the last one on the amortization schedule
            if (recalcData.PaymentRemainingCount > 0 && recalcData.ListChargesToAdd.Count() == recalcData.PaymentRemainingCount - 1
                || recalcData.PrincipalDecrementingAmt + (decimal) ppCharge.Interest <= recalcData.PeriodPaymentAmt)
            {
                ppCharge.Principal = (double) recalcData.PrincipalDecrementingAmt;
                if (!recalcData.IsRecalculateInterest && ppCharge.ChargeDate.Date >= terms.DateInterestStart.Date)
                {
                    ppCharge.Interest = (double) recalcData.InterestUnpaidDecrementingAmt;
                }
            }
        }

        private static void HandleIncreasedInterest(PayPlanRecalculationData recalcData, PayPlanTerms terms)
        {
            if (CompareDouble.IsLessThanZero(recalcData.OverpaidAmt))
            {
                var interest = ((recalcData.PrincipalRemaining + Math.Abs(recalcData.OverpaidAmt)) * recalcData.PeriodRate);
                if (recalcData.ListChargesToAdd.Count != 0 && recalcData.ListChargesToAdd[0].ChargeDate >= terms.DateInterestStart)
                {
                    var increasedInterestAmt = interest - recalcData.ListChargesToAdd[0].Interest;
                    recalcData.ListChargesToAdd[0].Interest = interest;
                    recalcData.ListChargesToAdd[0].Note = Lans.g("PayPlanEdit", "Increased interest") + ": " + increasedInterestAmt.ToString("c");
                }
                else if (recalcData.ListChargesToAdd.Count == 0
                         && recalcData.FirstFutureChargeDate.Date >= terms.DateInterestStart.Date
                         && recalcData.ListPayPlanCharges.FindAll(x => x.ChargeType == PayPlanChargeType.Debit).Count == recalcData.ListPastDebits.Count)
                {
                    recalcData.ListPayPlanCharges.Add(CreateDebitCharge(recalcData.PayPlan, recalcData.Fam, recalcData.ProvNum, recalcData.ClinicNum,
                        0, interest, recalcData.FirstFutureChargeDate, Lans.g("PayPlanEdit", "Increased interest") + ": " + interest.ToString("c")));
                }
            }
        }

        public static List<PayPlanEntry> CreatePayPlanEntriesForAccountCharges(List<AccountEntry> listAccountCharges, Patient patCur, PayPlan payPlan)
        {
            var listAccountEntries = listAccountCharges.FindAll(x => x.PatNum == patCur.PatNum && x.GetType() == typeof(Procedure));
            //Find all of the ProcNums associated to this payment plan.
            var listProcNums = listAccountCharges
                .Where(x => x.PayPlanNum == payPlan.PayPlanNum
                            && x.GetType() == typeof(FauxAccountEntry)
                            && ((FauxAccountEntry) x).AccountEntryProc != null)
                .Select(x => ((FauxAccountEntry) x).AccountEntryProc.ProcNum)
                .ToList();
            var listPayPlanEntries = new List<PayPlanEntry>();
            foreach (var entryCharge in listAccountEntries)
            {
                //Don't make a PayPlanEntry object for procedures that are paid off and have nothing to do with the current payment plan.
                if (entryCharge.AmountEnd == 0 && (listProcNums.IsNullOrEmpty() || !listProcNums.Contains(entryCharge.ProcNum)))
                {
                    continue;
                }

                //Blindly add PrincipalApplied which represents the value that was taken away from the procedure by the payment plan before implicit linking.
                var principalApplied = entryCharge.ListPayPlanPrincipalApplieds.FindAll(x => x.PayPlanNum == payPlan.PayPlanNum).Sum(x => x.PrincipalApplied);
                var remBefore = entryCharge.AmountEnd + principalApplied;
                var procCur = (Procedure) entryCharge.Tag;
                listPayPlanEntries.Add(new PayPlanEntry()
                {
                    ProcNumOrd = procCur.ProcNum,
                    AmtOrd = (double) entryCharge.AmountOriginal,
                    DateOrd = procCur.ProcDate,
                    ProcStatOrd = procCur.ProcStatus,
                    IsChargeOrd = false,
                    DateStr = procCur.ProcDate.ToShortDateString(),
                    PatStr = patCur.FName,
                    StatStr = procCur.ProcStatus.ToString(),
                    ProcStr = ProcedureCodes.GetStringProcCode(procCur.CodeNum),
                    FeeStr = procCur.ProcFeeTotal.ToString("f"),
                    RemBefStr = remBefore.ToString("f"),
                    Proc = procCur,
                    ProvAbbr = Providers.GetAbbr(procCur.ProvNum),
                    //everything else blank
                });
            }

            return listPayPlanEntries;
        }

        public static List<PayPlanEntry> CreatePayPlanEntriesForPayPlanCredits(List<AccountEntry> listAccountCharges, List<PayPlanCharge> listPayPlanCreditsCur)
        {
            var listPayPlanEntries = new List<PayPlanEntry>();
            var listCreditsApplied = new List<PayPlanCharge>();
            foreach (var credCur in listPayPlanCreditsCur)
            {
                var entryCur = listAccountCharges.Where(x => x.PriKey == credCur.ProcNum && x.GetType() == typeof(Procedure)).FirstOrDefault();
                Procedure procCur = null;
                if (entryCur != null)
                {
                    procCur = (Procedure) entryCur.Tag;
                }

                decimal remBefore = 0;
                if (entryCur != null)
                {
                    //Blindly add PrincipalApplied which represents the value that was taken away from the procedure by the payment plan before implicit linking.
                    var principalApplied = entryCur.ListPayPlanPrincipalApplieds.Where(x => x.PayPlanNum == credCur.PayPlanNum).Sum(x => x.PrincipalApplied);
                    remBefore = entryCur.AmountEnd + principalApplied;
                }

                listCreditsApplied.Add(credCur);
                var remAfter = remBefore - listCreditsApplied.Where(x => x.ProcNum == credCur.ProcNum).Sum(x => (decimal) x.Principal);
                var addEntry = new PayPlanEntry();
                addEntry.ProcNumOrd = credCur.ProcNum;
                addEntry.AmtOrd = credCur.Principal;
                addEntry.DateOrd = credCur.ChargeDate;
                addEntry.ProcStatOrd = ProcStat.TP;
                if (procCur != null)
                {
                    addEntry.ProcStatOrd = procCur.ProcStatus;
                }

                addEntry.IsChargeOrd = true;
                if (procCur != null && procCur.ProcStatus == ProcStat.TP)
                {
                    addEntry.CredDateStr = Lans.g("PayPlanEdit", "None");
                }
                else
                {
                    addEntry.CredDateStr = credCur.ChargeDate.ToShortDateString();
                }

                addEntry.AmtStr = credCur.Principal.ToString("f");
                addEntry.RemAftStr = "";
                if (entryCur != null)
                {
                    addEntry.RemAftStr = remAfter.ToString("f");
                }

                addEntry.NoteStr = credCur.Note;
                addEntry.Proc = procCur;
                addEntry.Charge = credCur;
                //specifically to get the provider that did the work for this procedure to see if the charge has the same prov
                addEntry.ProvNum = procCur?.ProvNum ?? 0;
                //everything else blank
                listPayPlanEntries.Add(addEntry);
            }

            return listPayPlanEntries;
        }

        public static PayPlanEntry CreatePayPlanEntryForUnattachedProcs(string patFName)
        {
            var payPlanEntry = new PayPlanEntry();
            //add another procedure row showing all unattached credits if at least one charge with a procnum of 0 exists.
            payPlanEntry.AmtOrd = 0;
            payPlanEntry.DateOrd = DateTime.MinValue;
            payPlanEntry.ProcStatOrd = ProcStat.TP; //for ordering purposes, since we want unattached to always show up last.
            payPlanEntry.IsChargeOrd = false;
            payPlanEntry.ProcNumOrd = 0;
            payPlanEntry.DateStr = Lans.g("PayPlanEdit", "Unattached");
            payPlanEntry.PatStr = patFName;
            payPlanEntry.StatStr = "";
            payPlanEntry.ProcStr = "";
            payPlanEntry.FeeStr = "";
            payPlanEntry.RemBefStr = "";
            payPlanEntry.ProvAbbr = ""; //credit will go to prov on payment plan
            return payPlanEntry;
        }

        public static PayPlanCharge CreateUnattachedCredit(string textDate, long patNum, string textNote, long payPlanNum, double amt)
        {
            var addCharge = new PayPlanCharge()
            {
                ChargeDate = SIn.Date(textDate),
                ChargeType = PayPlanChargeType.Credit,
                Guarantor = patNum, //credits should always appear on the patient of the payment plan.
                Note = SIn.String(textNote),
                PatNum = patNum,
                PayPlanNum = payPlanNum,
                Principal = amt,
                ProcNum = 0,
                //provider/clinic will be set when the amortization schedule is saved. FormPayPlan.SaveData()
                //ClinicNum=0,
                //ProvNum=0,
            };
            return addCharge;
        }

        public static List<PayPlanCharge> CreateOrUpdateChargeForSelectedEntry(PayPlanEntry selectedEntry, List<PayPlanCharge> listPayPlanCreditsCur, double amt, string textNote, string textDate, long patNum, long payPlanNum, PayPlanCharge selectedCharge)
        {
            selectedCharge.Principal = amt;
            selectedCharge.Note = SIn.String(textNote);
            if (selectedEntry.ProcStatOrd == ProcStat.TP && selectedEntry.ProcNumOrd != 0)
            {
                //if it's treatment planned, save the date as maxvalue so it will not show up in the ledger.
                //if it doesn't have a procnum, then we are editing an unattached row.
                selectedCharge.ChargeDate = DateTime.MaxValue;
            }
            else
            {
                selectedCharge.ChargeDate = SIn.Date(textDate);
            }

            if (!selectedEntry.IsChargeOrd)
            {
                //if it's a procedure
                //add a charge for the selected procedure. get info from text boxes.
                selectedCharge.ChargeType = PayPlanChargeType.Credit;
                selectedCharge.Guarantor = patNum; //credits should always appear on the patient of the payment plan.
                selectedCharge.PatNum = patNum;
                selectedCharge.PayPlanNum = payPlanNum;
                selectedCharge.ProcNum = selectedEntry.ProcNumOrd;
                //provider/clinic will be set when the amortization schedule is saved. FormPayPlan.SaveData()
                //ClinicNum=0,
                //ProvNum=0,
                listPayPlanCreditsCur.Add(selectedCharge);
            }

            return listPayPlanCreditsCur;
        }

        public static List<PayPlanCharge> CreateCreditsForAllSelectedEntries(List<PayPlanEntry> listSelectedProcs, List<PayPlanEntry> listPayPlanEntries, DateTime date, long patNum, long payPlanNum, List<PayPlanCharge> listPayPlanCreditsCur)
        {
            //add a charge for every selected procedure for the amount remaining.
            //don't allow adding $0.00 credits.
            foreach (var entryProcCur in listSelectedProcs)
            {
                var listEntriesForProc = listPayPlanEntries
                    .Where(x => x.ProcNumOrd == entryProcCur.ProcNumOrd)
                    .Where(x => x.IsChargeOrd == true).ToList();
                var addCharge = new PayPlanCharge();
                if (entryProcCur.ProcStatOrd == ProcStat.TP)
                {
                    //If tp, maxvalue.
                    addCharge.ChargeDate = DateTime.MaxValue;
                }
                else
                {
                    addCharge.ChargeDate = date;
                }

                addCharge.ChargeType = PayPlanChargeType.Credit;
                addCharge.Guarantor = patNum; //credits should always appear on the patient of the payment plan.
                addCharge.Note = ProcedureCodes.GetStringProcCode(entryProcCur.Proc.CodeNum) + ": " + Procedures.GetDescription(entryProcCur.Proc);
                addCharge.PatNum = patNum;
                addCharge.PayPlanNum = payPlanNum;
                addCharge.Principal = SIn.Double(entryProcCur.RemBefStr);
                if (listEntriesForProc.Count != 0)
                {
                    addCharge.Principal = listEntriesForProc.Min(x => SIn.Double(x.RemAftStr));
                }

                addCharge.ProcNum = entryProcCur.ProcNumOrd;
                //provider/clinic will be set when the amortization schedule is saved. FormPayPlan.SaveData()
                //ClinicNum=0,
                //ProvNum=0,
                if (CompareDouble.IsGreaterThan(addCharge.Principal, 0))
                {
                    listPayPlanCreditsCur.Add(addCharge);
                }
            }

            return listPayPlanCreditsCur;
        }

        public static List<PayPlanCharge> GetListExpectedChargesAwaitingCompletion(List<PayPlanCharge> listChargesInDB, PayPlanTerms terms, Family famCur, List<PayPlanLink> listPayPlanLinks, PayPlan payplan, bool isNextPeriodOnly, bool isForDownPaymentCharge = false, List<PaySplit> listPaySplits = null, List<PayPlanCharge> listExpectedChargesDownPayment = null)
        {
            var listExpectedCharges = new List<PayPlanCharge>();
            //If the payment plan is set to await complete, TP procs will have been omitted. So we need to go and grab the charges for tp procs,
            //and add them to the returned list. Getting completed charges seperately maintains the charge structure for completed production charges.
            //i.e. Interest calculation, and Charge date will be accurate.
            if (terms.DynamicPayPlanTPOption == DynamicPayPlanTPOptions.AwaitComplete)
            {
                //Pretend the terms were set to treat as complete so we can get the charges for TP procs.
                terms.DynamicPayPlanTPOption = DynamicPayPlanTPOptions.TreatAsComplete;
                var listExpectedChargesIncludingTP = GetListExpectedCharges(listChargesInDB, terms, famCur, listPayPlanLinks, payplan, isNextPeriodOnly,
                    isForDownPaymentCharge, listPaySplits: listPaySplits, listExpectedChargesDownPayment: listExpectedChargesDownPayment);
                terms.DynamicPayPlanTPOption = DynamicPayPlanTPOptions.AwaitComplete;
                var payPlanProductionEntries = PayPlanProductionEntry.GetProductionForLinks(listPayPlanLinks);
                //iterate through all of the productions, for treatment planned procs, find their associated charges and change their date due to MaxValue, 
                //then add them to the list of expected charges.
                for (var i = 0; i < payPlanProductionEntries.Count; i++)
                {
                    Procedure proc = null;
                    if (payPlanProductionEntries[i].LinkType == PayPlanLinkType.Procedure)
                    {
                        proc = Procedures.GetOneProc((long) listPayPlanLinks[i].FKey, false);
                    }

                    if (proc != null && proc.ProcStatus != ProcStat.C)
                    {
                        listExpectedChargesIncludingTP.FindAll(x => x.LinkType == PayPlanLinkType.Procedure && x.FKey == proc.ProcNum).ForEach(x => x.ChargeDate = DateTime.MaxValue);
                        listExpectedCharges.AddRange(listExpectedChargesIncludingTP.FindAll(x => x.LinkType == PayPlanLinkType.Procedure && x.FKey == proc.ProcNum));
                    }
                }
            }

            return listExpectedCharges; //returns all expected for the entire life of the payment plan. 
        }

        public static List<PayPlanCharge> GetListExpectedCharges(List<PayPlanCharge> listChargesInDB, PayPlanTerms terms, Family famCur, List<PayPlanLink> listPayPlanLinks, PayPlan payplan, bool isNextPeriodOnly, bool isForDownPaymentCharge = false, List<PaySplit> listPaySplits = null, List<PayPlanCharge> listExpectedChargesDownPayment = null)
        {
            if (listPaySplits == null)
            {
                listPaySplits = PaySplits.GetForPayPlans([payplan.PayPlanNum]);
            }

            var chargesCount = listChargesInDB.Count;
            var periodCount = CalcPeriodsForFrequency(terms.DateFirstPayment.Date, terms.Frequency);
            return GetListExpectedCharges(listChargesInDB, terms, famCur, listPayPlanLinks, listPaySplits, payplan, isNextPeriodOnly, chargesCount, periodCount
                , isForDownPaymentCharge, listExpectedChargesDownPayment);
        }

        public static List<PayPlanCharge> GetListExpectedCharges(List<PayPlanCharge> listChargesInDB, PayPlanTerms terms, Family famCur, List<PayPlanLink> listPayPlanLinks, List<PaySplit> listPaySplits, PayPlan payplan, bool isNextPeriodOnly, int chargesCount, int periodCount, bool isForDownPaymentCharge, List<PayPlanCharge> listExpectedChargesDownPayment = null)
        {
            var listExpectedCharges = new List<PayPlanCharge>();
            //Get production attached to credits attached to the payment plan (what we will be making charges for)
            var listCreditsAndProduction = PayPlanProductionEntry.GetWithAmountRemaining(listPayPlanLinks, listChargesInDB);
            if (listExpectedChargesDownPayment != null)
            {
                //Charges for down payment are created but not in DB yet, so we must subtract these charges from the remaining amounts of production items.
                foreach (var productionEntry in listCreditsAndProduction)
                {
                    productionEntry.AmountRemaining -= (decimal) listExpectedChargesDownPayment
                        .Where(x => x.FKey == productionEntry.PriKey && x.LinkType == productionEntry.LinkType).Sum(x => x.Principal);
                }
            }

            var sumPrincipalNotYetCharged = (decimal) terms.PrincipalAmount - (decimal) listChargesInDB.Sum(x => x.Principal);
            if (terms.DynamicPayPlanTPOption == DynamicPayPlanTPOptions.AwaitComplete && !isForDownPaymentCharge)
            {
                var payPlanProductionEntries = PayPlanProductionEntry.GetProductionForLinks(listPayPlanLinks);
                for (var i = 0; i < payPlanProductionEntries.Count; i++)
                {
                    Procedure proc = null;
                    if (payPlanProductionEntries[i].LinkType == PayPlanLinkType.Procedure)
                    {
                        proc = Procedures.GetOneProc((long) listPayPlanLinks[i].FKey, false);
                    }

                    if (proc != null && proc.ProcStatus != ProcStat.C)
                    {
                        //TP procs don't really have value yet until they're complete
                        sumPrincipalNotYetCharged -= payPlanProductionEntries[i].GetAmountAttached();
                    }
                }
            }

            //If plan has no charges in DB yet, and method isn't being called to create down payment charges,
            //we must subtract the down payment from the principal so it is not considered in interest calculations.
            if (listChargesInDB.Count == 0 && !isForDownPaymentCharge)
            {
                sumPrincipalNotYetCharged -= (decimal) terms.DownPayment;
            }

            //specifically to make a check more accurate where determining if payplan will ever be paid off
            var previousSumPrincipalNotYetCharged = sumPrincipalNotYetCharged;
            var maxPayPlanCharges = 2000; //ceiling of payplan charges should not go beyond 2000
            var listProcNums = listCreditsAndProduction.FindAll(x => x.LinkType == PayPlanLinkType.Procedure)
                .Select(y => y.PriKey)
                .Distinct()
                .ToList();
            var listProcedures = Procedures.GetManyProc(listProcNums, includeNote: false);
            //Iterates over the periods.
            while (sumPrincipalNotYetCharged > 0 && chargesCount < maxPayPlanCharges)
            {
                var periodDate = CalcNextPeriodDate(terms.DateFirstPayment, periodCount, terms.Frequency);
                var interestForPeriod = CalculateInterestAmountForPeriod(sumPrincipalNotYetCharged, terms, periodDate, isNextPeriodOnly, listPaySplits
                    , listChargesInDB, listExpectedCharges);
                var principalForPeriod = CalculatePrincipalAmountForPeriod(sumPrincipalNotYetCharged, terms, interestForPeriod);
                if (CompareDouble.IsEqual(principalForPeriod, 0))
                {
                    break;
                }

                if ((decimal.Compare(sumPrincipalNotYetCharged, previousSumPrincipalNotYetCharged) > 0 && interestForPeriod > 0))
                {
                    //The principal is actually increasing or staying the same with each payment.
                    terms.AreTermsValid = false;
                    listExpectedCharges.Clear();
                    return listExpectedCharges; //return nothing. We don't want to leave charges in this list since we're stopping our calculations.
                }

                previousSumPrincipalNotYetCharged = sumPrincipalNotYetCharged;
                decimal curPayAmount = 0;

                #region Generate Charges For Period

                while (!CompareDecimal.IsEqual(curPayAmount, terms.PeriodPayment) && !CompareDouble.IsZero(principalForPeriod) && chargesCount < maxPayPlanCharges)
                {
                    var chargeCur = new PayPlanCharge();
                    var entry = listCreditsAndProduction.FirstOrDefault(x => CompareDecimal.IsGreaterThanZero(x.AmountRemaining));
                    if (entry == null)
                    {
                        return listExpectedCharges;
                    }

                    Procedure proc = null;
                    if (entry.LinkType == PayPlanLinkType.Procedure)
                    {
                        proc = listProcedures.Find(x => x.ProcNum == entry.PriKey);
                    }

                    //Used for handling 'Await Complete' option in dynamic payment plans. skips over adding a charge for an incomplete proc.
                    if (proc != null && proc.ProcStatus == ProcStat.TP && terms.DynamicPayPlanTPOption == DynamicPayPlanTPOptions.AwaitComplete && !isForDownPaymentCharge)
                    {
                        entry.AmountRemaining = 0;
                        continue;
                    }

                    //compare amount remaining on attached production with the period payment amount.
                    var principalForCharge = Math.Min(principalForPeriod, (double) entry.AmountRemaining);
                    if (curPayAmount > 0)
                    {
                        interestForPeriod = 0; //only apply interest to the first charge for the period. 
                    }

                    chargeCur = CreateDebitChargeDynamic(payplan, famCur, entry.ProvNum, entry.ClinicNum, principalForCharge, interestForPeriod
                        , periodDate, "", entry.PriKey, entry.LinkType);
                    if (listChargesInDB.Exists(x => x.ChargeDate == chargeCur.ChargeDate && !x.Note.Contains("Down Payment")))
                    {
                        principalForPeriod -= chargeCur.Principal;
                        continue;
                    }

                    entry.AmountRemaining -= (decimal) principalForCharge;
                    sumPrincipalNotYetCharged -= (decimal) chargeCur.Principal;
                    principalForPeriod -= chargeCur.Principal;
                    curPayAmount += (decimal) chargeCur.Principal;
                    listExpectedCharges.Add(chargeCur);
                    chargesCount++;
                }

                #endregion

                periodCount++;
                if (isNextPeriodOnly && listExpectedCharges.Count > 0)
                {
                    return listExpectedCharges; //only returns charges made for this next period. 
                }
            }

            return listExpectedCharges; //returns all expected for the entire life of the payment plan. 
        }

        public static int CalcPeriodsForFrequency(DateTime dateFirstCharge, PayPlanFrequency frequency)
        {
            var period = 0;
            if (dateFirstCharge == DateTime.MinValue)
            {
                return period;
            }

            if (frequency == PayPlanFrequency.Weekly)
            {
                while (DateTime_.Today > dateFirstCharge.AddDays(7 * period))
                {
                    period++;
                }
            }
            else if (frequency == PayPlanFrequency.EveryOtherWeek)
            {
                while (DateTime_.Today > dateFirstCharge.AddDays(14 * period))
                {
                    period++;
                }
            }
            else if (frequency == PayPlanFrequency.OrdinalWeekday)
            {
                while (DateTime_.Today > dateFirstCharge)
                {
                    var roughMonth = dateFirstCharge.AddMonths(1);
                    var dayOfWeekFirstDate = dateFirstCharge.DayOfWeek;
                    var dayOfWeekFirstMonth = new DateTime(roughMonth.Year, roughMonth.Month, 1).DayOfWeek;
                    if (dayOfWeekFirstMonth == dayOfWeekFirstDate)
                    {
                        dateFirstCharge = new DateTime(roughMonth.Year, roughMonth.Month, 1);
                    }
                    else if (dayOfWeekFirstMonth < dayOfWeekFirstDate)
                    {
                        dateFirstCharge = new DateTime(roughMonth.Year, roughMonth.Month, dayOfWeekFirstDate - dayOfWeekFirstMonth + 1);
                    }
                    else
                    {
                        dateFirstCharge = new DateTime(roughMonth.Year, roughMonth.Month, 7 - (dayOfWeekFirstMonth - dayOfWeekFirstDate) + 1);
                    }

                    var ordinalOfMonth = GetOrdinalOfMonth(dateFirstCharge);
                    dateFirstCharge = dateFirstCharge.AddDays(7 * (ordinalOfMonth - 1));
                    period++;
                }
            }
            else if (frequency == PayPlanFrequency.Monthly)
            {
                while (DateTime_.Today > dateFirstCharge.AddMonths(1 * period))
                {
                    period++;
                }
            }
            else if (frequency == PayPlanFrequency.Quarterly)
            {
                while (DateTime_.Today > dateFirstCharge.AddMonths(3 * period))
                {
                    period++;
                }
            }

            return period;
        }

        public static void IssueChargesDueForDynamicPaymentPlans(List<PayPlan> listDynamicPayPlans, LogWriter log, bool isOpenDentalService = false)
        {
            Signalods.SetInvalid(InvalidType.Prefs);
            try
            {
                log.WriteLine("Running payment plan logic.", LogLevel.Verbose);
                var listPayPlanLinksAll = PayPlanLinks.GetForPayPlans(listDynamicPayPlans.Select(x => x.PayPlanNum).ToList());
                var dictPayPlanCharges = PayPlanCharges.GetForPayPlans(listDynamicPayPlans.Select(x => x.PayPlanNum).ToList())
                    .GroupBy(x => x.PayPlanNum)
                    .ToDictionary(x => x.Key, x => x.OrderBy(y => y.ChargeDate).ToList());
                var dictPaySplits = PaySplits.GetForPayPlans(listDynamicPayPlans.Select(x => x.PayPlanNum).ToList())
                    .GroupBy(x => x.PayPlanNum)
                    .ToDictionary(x => x.Key, x => x.ToList());
                var dictProdEntrys = PayPlanProductionEntry.GetProductionForLinks(listPayPlanLinksAll)
                    .GroupBy(x => x.PayPlanNum)
                    .ToDictionary(x => x.Key, x => x.ToList());
                //Make a list of distinct pat nums for each payment plan.
                var listPatNums = listDynamicPayPlans.Select(x => x.PatNum).Distinct().ToList();
                //Get every family for each pat num found.
                var listFamilies = Patients.GetFamilies(listPatNums);
                //Make a dictionary out of the distinct pat nums for each payment plan and find their corresponding family entity.
                var dictFamilies = new Dictionary<long, Family>();
                foreach (var patNum in listPatNums)
                {
                    dictFamilies[patNum] = listFamilies.First(x => x.ListPats.Any(y => y.PatNum == patNum));
                }

                log.WriteLine($"Payment plans found: {listDynamicPayPlans.Count}", LogLevel.Verbose);
                //Create any necessary pay plan charges for dynamic payment plans.
                foreach (var payplan in listDynamicPayPlans)
                {
                    if (!dictPayPlanCharges.TryGetValue(payplan.PayPlanNum, out var listPayPlanChargesInDb))
                    {
                        listPayPlanChargesInDb = [];
                    }

                    if (!dictPaySplits.TryGetValue(payplan.PayPlanNum, out var listPaySplits))
                    {
                        listPaySplits = [];
                    }

                    //Continue issuing charges for today.
                    var listLinksForPayPlan = listPayPlanLinksAll.FindAll(x => x.PayPlanNum == payplan.PayPlanNum);
                    var terms = GetPayPlanTerms(payplan, listLinksForPayPlan);
                    //get the expected period that this charge would be for.
                    var periodCount = CalcPeriodsForFrequency(terms.DateFirstPayment.Date, terms.Frequency);
                    var nextExpectedDate = CalcNextPeriodDate(payplan.DatePayPlanStart, periodCount, payplan.ChargeFrequency);
                    var listPayPlanChargesExpected = listPayPlanChargesInDb.FindAll(x => x.ChargeDate == nextExpectedDate && !x.Note.Contains("Down Payment"));
                    //Check to see if a charge has already been made for today.
                    if (nextExpectedDate == DateTime_.Today && !listPayPlanChargesExpected.IsNullOrEmpty())
                    {
                        //Any prepayments should be automatically applied to charges that are due today.
                        ApplyPrepaymentsToCharges(payplan, listPayPlanChargesExpected, listPaySplits, log);
                        continue;
                    }

                    if (nextExpectedDate.Date > DateTime_.Today)
                    {
                        continue; //it is not yet time to add another charge. Skip the other logic.
                    }

                    //Filter out on charge date so we don't insert future charges. We need to get ALL expected charges here just in case the service was
                    //down or a payment got missed from the run time. Logic should add the older charge to the database as well as the newer that is now due.
                    //Should have been skipped above, though the logic within GetListExpectedCharges() is more accurate.
                    var listPayPlanChargesNew = new List<PayPlanCharge>();
                    List<PayPlanProductionEntry> listProdEntryForPlan;
                    if (!dictProdEntrys.TryGetValue(payplan.PayPlanNum, out listProdEntryForPlan))
                    {
                        listProdEntryForPlan = [];
                    }

                    var principalActual = PayPlanProductionEntry.GetDynamicPayPlanCompletedAmount(payplan, listProdEntryForPlan);
                    var payPeriodsMissable = 100; //This should be way more than sufficent to catch up on charges. Mostly put in for safety kickout. 
                    while (listPayPlanChargesInDb.Sum(x => x.Principal) < principalActual
                           && nextExpectedDate.Date <= DateTime_.Today
                           && payPeriodsMissable > 0)
                    {
                        var listChargesThisPeriod = GetListExpectedCharges(listPayPlanChargesInDb, terms, dictFamilies[payplan.PatNum]
                            , listLinksForPayPlan, payplan, true, listPaySplits: listPaySplits);
                        if (listChargesThisPeriod.IsNullOrEmpty())
                        {
                            break;
                        }

                        listPayPlanChargesNew.AddRange(listChargesThisPeriod);
                        listPayPlanChargesInDb.AddRange(listChargesThisPeriod);
                        periodCount++;
                        nextExpectedDate = CalcNextPeriodDate(payplan.DatePayPlanStart, periodCount, payplan.ChargeFrequency);
                        payPeriodsMissable--;
                    }

                    log.WriteLine($"Expected PayPlanCharges to be inserted for PayPlanNum #{payplan.PayPlanNum}: {listPayPlanChargesNew.Count}", LogLevel.Verbose);
                    //Loop through each expected charge and insert them into the database one at a time so that the primary keys in memory are set correctly.
                    foreach (var payPlanCharge in listPayPlanChargesNew)
                    {
                        PayPlanCharges.Insert(payPlanCharge);
                    }

                    ApplyPrepaymentsToCharges(payplan, listPayPlanChargesNew, listPaySplits, log);
                }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex.Message, LogLevel.Error);
            }
            finally
            {
                if (isOpenDentalService)
                {
                    //This instance of Open Dental service is no longer running the dynamic payment plan logic.  Update the database to indicate this fact.
                    Prefs.UpdateDateT(PrefName.DynamicPayPlanLastDateTime, MiscData.GetNowDateTime());
                    Prefs.UpdateDateT(PrefName.DynamicPayPlanStartDateTime, DateTime.MinValue);
                    Signalods.SetInvalid(InvalidType.Prefs);
                }
            }
        }

        private static void ApplyPrepaymentsToCharges(PayPlan payplan, List<PayPlanCharge> listPayPlanCharges, List<PaySplit> listPaySplits, LogWriter log)
        {
            var prepaymentAmount = GetDynamicPayPlanPrepaymentAmount(listPaySplits);
            if (CompareDouble.IsLessThanOrEqualToZero(prepaymentAmount))
            {
                return;
            }

            log.WriteLine($"Hidden unearned prepayments detected for PayPlanNum #{payplan.PayPlanNum}. Applying up to {prepaymentAmount.ToString("c")} to any new charges.", LogLevel.Verbose);
            ApplyPrepaymentToDynamicPaymentPlan(payplan.Guarantor, prepaymentAmount, listPayPlanCharges, listPaySplits); //Guarantors on payment plans are the ones that make the payments.
        }

        public static double GetDynamicPayPlanPrepaymentAmount(List<PaySplit> listPaySplits)
        {
            var paymentPlanHiddenUnearnedPrepaymentNum = PrefC.GetLong(PrefName.DynamicPayPlanPrepaymentUnearnedType);
            return listPaySplits.FindAll(x => x.UnearnedType == paymentPlanHiddenUnearnedPrepaymentNum).Sum(x => x.SplitAmt);
        }

        public static void ApplyPrepaymentToDynamicPaymentPlan(long patNum, double hiddenUnearnedTotal, List<PayPlanCharge> listPayPlanChargesNow, List<PaySplit> listPaySplits, long payPlanNum = 0, bool isClosing = false)
        {
            if (listPayPlanChargesNow.IsNullOrEmpty() && CompareDouble.IsZero(hiddenUnearnedTotal))
            {
                return;
            }

            var listTransferSplits = new List<PaySplit>();
            var PrepayUnearnedType = PrefC.GetLong(PrefName.DynamicPayPlanPrepaymentUnearnedType);
            for (var i = 0; i < listPayPlanChargesNow.Count; i++)
            {
                if (CompareDouble.IsLessThanOrEqualToZero(hiddenUnearnedTotal))
                {
                    break;
                }

                var charge = listPayPlanChargesNow[i];
                //Calculate the remainder value of the charge. This allows future dated charges (that have an opportunity to have payments applied to them), to be passed into this method.
                var chargeInterestValue = charge.Interest;
                var chargePrincipalValue = charge.Principal;
                //Sum up all the payments on inserted charges broken up by debit type.
                var principalPaid = listPaySplits.FindAll(x => x.PayPlanChargeNum != 0 && x.PayPlanChargeNum == charge.PayPlanChargeNum && x.PayPlanDebitType == PayPlanDebitTypes.Principal).Sum(x => x.SplitAmt);
                var interestPaid = listPaySplits.FindAll(x => x.PayPlanChargeNum != 0 && x.PayPlanChargeNum == charge.PayPlanChargeNum && x.PayPlanDebitType == PayPlanDebitTypes.Interest).Sum(x => x.SplitAmt);
                var unknownPaid = listPaySplits.FindAll(x => x.PayPlanChargeNum != 0 && x.PayPlanChargeNum == charge.PayPlanChargeNum && x.PayPlanDebitType == PayPlanDebitTypes.Unknown && x.UnearnedType == 0).Sum(x => x.SplitAmt);
                chargeInterestValue -= Math.Min(chargeInterestValue, interestPaid);
                if (!CompareDouble.IsZero(chargeInterestValue))
                {
                    var unknownValueApplied = Math.Min(chargeInterestValue, unknownPaid);
                    unknownPaid -= unknownValueApplied;
                    chargeInterestValue -= unknownValueApplied;
                }

                chargePrincipalValue -= Math.Min(chargePrincipalValue, principalPaid);
                if (!CompareDouble.IsZero(chargePrincipalValue))
                {
                    var unknownValueApplied = Math.Min(chargePrincipalValue, unknownPaid);
                    unknownPaid -= unknownValueApplied;
                    chargePrincipalValue -= unknownValueApplied;
                }

                //if there is no value remaining on this charge, kick rocks kid.
                if (CompareDouble.IsLessThanOrEqualToZero(chargeInterestValue) && CompareDouble.IsLessThanOrEqualToZero(chargePrincipalValue))
                {
                    continue;
                }

                var splitInterestAmt = Math.Min(hiddenUnearnedTotal, chargeInterestValue);
                hiddenUnearnedTotal -= splitInterestAmt;
                var splitPrincipalAmt = Math.Min(hiddenUnearnedTotal, chargePrincipalValue);
                hiddenUnearnedTotal -= splitPrincipalAmt;
                //Make a positive interest, and positive princial split linked to the charge.
                if (!CompareDouble.IsZero(splitInterestAmt))
                {
                    listTransferSplits.AddRange(CreateUnearnedTransfer(splitInterestAmt, charge, PayPlanDebitTypes.Interest, PrepayUnearnedType));
                }

                if (!CompareDouble.IsZero(splitPrincipalAmt))
                {
                    listTransferSplits.AddRange(CreateUnearnedTransfer(splitPrincipalAmt, charge, PayPlanDebitTypes.Principal, PrepayUnearnedType));
                }
            }

            //If there are no transfer splits to even make and we aren't closing the plan, return.
            //Note: if we are closing the plan, continue through this logic so that the remainder of hiddenUnearnedTotal (that isn't applied to a passed in charge) can be removed from
            //the passed in pay plan's hidden unearned bucket, and moved to regular unearned.
            if (listTransferSplits.IsNullOrEmpty() && !isClosing)
            {
                return;
            }

            if (!CompareDouble.IsZero(listTransferSplits.Sum(x => x.SplitAmt)))
            {
                throw new ApplicationException("Applying prepayments to payment plan tried to create a non-zero amount income transfer.");
            }

            var transferPayment = new Payment();
            transferPayment.DateEntry = DateTime_.Today;
            transferPayment.PayDate = DateTime_.Today;
            transferPayment.PaymentSource = CreditCardSource.None;
            transferPayment.ProcessStatus = ProcessStat.OfficeProcessed;
            transferPayment.PayType = 0;
            transferPayment.PatNum = patNum;
            transferPayment.PayAmt = 0; //Income transfers are always $0.
            transferPayment.PayNum = Payments.Insert(transferPayment, listTransferSplits);
            var logText = Payments.GetSecuritylogEntryText(transferPayment, transferPayment, isNew: true) + ", " + Lans.g("PayPlanEdit", "from the payment plan prepayment processing service.");
            if (transferPayment.PayNum > 0)
            {
                SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, transferPayment.PatNum, logText);
            }

            if (CompareDouble.IsGreaterThan(hiddenUnearnedTotal, 0) && payPlanNum != 0)
            {
                var notUsed = new PaymentEdit.IncomeTransferData();
                listTransferSplits = [];
                listTransferSplits.AddRange(PaymentEdit.CreateUnearnedTransfer((decimal) hiddenUnearnedTotal, patNum, 0, 0, ref notUsed, unearnedType: PrepayUnearnedType, payPlanNum: payPlanNum, isMovingFromHiddenUnearnedToUnearned: true));
                transferPayment.PayNum = Payments.Insert(transferPayment, listTransferSplits);
                SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, transferPayment.PatNum, logText);
            }
        }

        public static double CalculatePrincipalRemaining(double principalAmount, List<PaySplit> listPaySplitsForPayPlan, List<PayPlanCharge> listChargesInDB, List<PayPlanCharge> listExpectedCharges, bool isForNextPeriodOnly)
        {
            var listSplitsExcludeUnearned = listPaySplitsForPayPlan.FindAll(x => x.UnearnedType == 0);
            var listPayPlanCharges = new List<PayPlanCharge>(listChargesInDB);
            listPayPlanCharges.AddRange(listExpectedCharges);
            var buckets = GetDynamicPaymentPlanRecalculationBuckets(listPayPlanCharges, listSplitsExcludeUnearned, isOverpaidInterestPrincipal: true);
            //Principle expected to be charged out
            var principalNotYetCharged = principalAmount - (listPayPlanCharges.Sum(x => x.Principal));
            //If the user has selected to apply excess interest to principal
            //if payment on principal is greater than principal that is / will be due, no interest should be charged.
            var totalAgainstPrincipal = Math.Max(0, buckets.PrincipalSplitAmtRem + buckets.UnknownSplitAmtRem + buckets.InterestSplitAmtRem);
            double unpaidPrincipal = 0;
            if (isForNextPeriodOnly)
            {
                unpaidPrincipal = buckets.PrincipalChargedRem;
            }

            var principalRemaining = principalNotYetCharged + unpaidPrincipal - totalAgainstPrincipal;
            return Math.Max(0, principalRemaining);
        }

        private static DynamicPaymentPlanRecalculationBuckets GetDynamicPaymentPlanRecalculationBuckets(List<PayPlanCharge> listPayPlanCharges, List<PaySplit> listPaySplits, bool isOverpaidInterestPrincipal = false)
        {
            /* When calculating interest remaining, we want to consider the account in its current state at this exact moment in time.
             * To do this, we will need to partition paysplits into 3 buckets. Principal, Interest, and unknown.
             * Paysplits for principal, are exclusive to principal, and cannot be applied to interest.
             * Similarly, interest will be applied to interest first, but then can be applied to principal.
             * First, if transfers exist in the unkown category, the negative splits will be pulled from the unknown bucket, then if there is any remaining negative
             * value, we will remove from the principal bucket, and the interest bucket last. Negative Principal/Interest only gets removed from the positive Principal/Interest buckets.
             * Next, we pull the charged interest from the interest bucket, then charged principal from the principal bucket.
             * Finally, if there is anything left charged (either interest or principal), pull interest charged from unknown bucket first, then principal.
             * Any left over value in the unknown, or principal buckets can be applied to principal not yet charged. */
            var retVal = new DynamicPaymentPlanRecalculationBuckets();
            listPaySplits.RemoveAll(x => x.UnearnedType > 0); //Unearned and hidden unearned should never be considered as part of the payment plan.
            if (listPayPlanCharges.IsNullOrEmpty() || listPaySplits.IsNullOrEmpty())
            {
                if (!listPayPlanCharges.IsNullOrEmpty())
                {
                    retVal.InterestChargedRem = listPayPlanCharges.Sum(x => x.Interest);
                    retVal.PrincipalChargedRem = listPayPlanCharges.Sum(x => x.Principal);
                }

                return retVal;
            }

            //Payment Splits-----------------------------------------------------------------------------------------------------------------------------------
            //The principle bucket will be the sum of all paysplits flagged for principal
            retVal.PrincipalSplitAmtRem = listPaySplits.Where(x => x.PayPlanDebitType == PayPlanDebitTypes.Principal && x.SplitAmt >= 0).Sum(x => x.SplitAmt);
            //transferPrincipalBucket is a positive value that represents negative splits.
            var transferPrincipalBucket = listPaySplits.Where(x => x.PayPlanDebitType == PayPlanDebitTypes.Principal && x.SplitAmt < 0).Sum(x => -x.SplitAmt);
            //The interest bucket will be the sum of all paysplits flagged for interest
            retVal.InterestSplitAmtRem = listPaySplits.Where(x => x.PayPlanDebitType == PayPlanDebitTypes.Interest && x.SplitAmt >= 0).Sum(x => x.SplitAmt);
            //transferInterestBucket is a positive value that represents negative splits.
            var transferInterestBucket = listPaySplits.Where(x => x.PayPlanDebitType == PayPlanDebitTypes.Interest && x.SplitAmt < 0).Sum(x => -x.SplitAmt);
            //The unknown bucket will be the sum of all paysplits flagged as unknown (legacy splits)
            retVal.UnknownSplitAmtRem = listPaySplits.Where(x => x.PayPlanDebitType == PayPlanDebitTypes.Unknown && x.SplitAmt >= 0).Sum(x => x.SplitAmt);
            //transferUknownBucket is a positive value that represents negative splits.
            var transferUnknownBucket = listPaySplits.Where(x => x.PayPlanDebitType == PayPlanDebitTypes.Unknown && x.SplitAmt < 0).Sum(x => -x.SplitAmt);
            //Charges------------------------------------------------------------------------------------------------------------------------------------------
            retVal.InterestChargedRem = listPayPlanCharges.Sum(x => x.Interest);
            retVal.PrincipalChargedRem = listPayPlanCharges.Sum(x => x.Principal);

            #region Negative splits

            //Apply negative principal first
            retVal.PrincipalSplitAmtRem -= Math.Min(transferPrincipalBucket, retVal.PrincipalSplitAmtRem);
            //Next apply negative interest
            retVal.InterestSplitAmtRem -= Math.Min(transferInterestBucket, retVal.InterestSplitAmtRem);
            //Lastly apply negative unknown
            //transferUknownBucket is a positive value that represents negative splits.
            var negativeUnknownSplitsApplyingToUnknownSplits = Math.Min(transferUnknownBucket, retVal.UnknownSplitAmtRem);
            transferUnknownBucket -= negativeUnknownSplitsApplyingToUnknownSplits; //Apply unknown positive splits to unknown negative splits.
            retVal.UnknownSplitAmtRem -= negativeUnknownSplitsApplyingToUnknownSplits; //Remove value applied from the Unknown bucket.
            //Take any excess principal and apply the negative unknown to it.
            if (!CompareDouble.IsZero(transferUnknownBucket))
            {
                var negativeUnknownSplitsApplyingToPrincipalSplits = Math.Min(transferUnknownBucket, retVal.PrincipalSplitAmtRem);
                retVal.PrincipalSplitAmtRem -= negativeUnknownSplitsApplyingToPrincipalSplits;
                transferUnknownBucket -= negativeUnknownSplitsApplyingToPrincipalSplits;
            }

            //Take any excess interest and apply the negative unknown to it.
            if (!CompareDouble.IsZero(transferUnknownBucket))
            {
                var negativeUnknownSplitsApplyingToInterestSplits = Math.Min(transferUnknownBucket, retVal.InterestSplitAmtRem);
                retVal.InterestSplitAmtRem -= negativeUnknownSplitsApplyingToInterestSplits;
                transferUnknownBucket -= negativeUnknownSplitsApplyingToInterestSplits;
            }

            if (transferUnknownBucket > 0)
            {
                //We never want to do anything in this state, and it really shouldn't be possible since this would mean the entire plan has transfer splits far exceeding their positive counterpart.
            }

            #endregion

            #region Positive splits

            //Apply positive interest first
            var interestSplitsApplyingToInterestCharged = Math.Min(retVal.InterestSplitAmtRem, retVal.InterestChargedRem);
            retVal.InterestChargedRem -= interestSplitsApplyingToInterestCharged;
            retVal.InterestSplitAmtRem -= interestSplitsApplyingToInterestCharged;
            if (retVal.InterestChargedRem != 0)
            {
                var unknownApplyingToInterest = Math.Min(retVal.UnknownSplitAmtRem, retVal.InterestChargedRem);
                retVal.UnknownSplitAmtRem -= unknownApplyingToInterest;
                retVal.InterestChargedRem -= unknownApplyingToInterest;
            }

            //Conditionally treat overpaid interest as if it is money that can be applied towards principal.
            if (isOverpaidInterestPrincipal)
            {
                retVal.PrincipalSplitAmtRem += retVal.InterestSplitAmtRem;
                retVal.InterestSplitAmtRem = 0;
            }

            //Next apply positive principal
            var principalSplitsApplyingToPrincipalCharged = Math.Min(retVal.PrincipalSplitAmtRem, retVal.PrincipalChargedRem);
            retVal.PrincipalChargedRem -= principalSplitsApplyingToPrincipalCharged;
            retVal.PrincipalSplitAmtRem -= principalSplitsApplyingToPrincipalCharged;
            if (retVal.PrincipalChargedRem != 0)
            {
                var unknwonApplyingToPrincipal = Math.Min(retVal.UnknownSplitAmtRem, retVal.PrincipalChargedRem);
                retVal.UnknownSplitAmtRem -= unknwonApplyingToPrincipal;
                retVal.PrincipalChargedRem -= unknwonApplyingToPrincipal;
            }

            #endregion

            return retVal;
        }

        public static DynamicPaymentPlanModuleData GetCloseoutChargesForDynamicPaymentPlan(DynamicPaymentPlanModuleData dynamicPaymentPlanModule)
        {
            //make payment plan charges for the remaining amount of each piece of production
            var listChargesClosing = new List<PayPlanCharge>();
            var listProcNumsForChargesInDb = dynamicPaymentPlanModule.ListPayPlanChargesDb.FindAll(x => x.LinkType == PayPlanLinkType.Procedure).Select(x => x.FKey).ToList();
            var listAdjNumsForChargesInDb = dynamicPaymentPlanModule.ListPayPlanChargesDb.FindAll(x => x.LinkType == PayPlanLinkType.Adjustment).Select(x => x.FKey).ToList();
            var listProductionEntries = PayPlanProductionEntry.GetWithAmountRemaining(dynamicPaymentPlanModule.ListPayPlanLinks, dynamicPaymentPlanModule.ListPayPlanChargesDb);
            foreach (var entry in listProductionEntries)
            {
                var isDetatchedFromPlan = false;
                //These links never had actual charges made for them. Remove them so they can be attached to a new payment plan in the future.
                if (entry.LinkType == PayPlanLinkType.Adjustment && !listAdjNumsForChargesInDb.Contains(entry.PriKey))
                {
                    dynamicPaymentPlanModule.ListPayPlanLinks.RemoveAll(x => x.LinkType == PayPlanLinkType.Adjustment && x.FKey == entry.PriKey); //Sync will delete when saving.
                    dynamicPaymentPlanModule.ListPayPlanProductionEntries.RemoveAll(x => x.LinkType == PayPlanLinkType.Adjustment && x.PriKey == entry.PriKey);
                    isDetatchedFromPlan = true;
                }

                if (entry.LinkType == PayPlanLinkType.Procedure && !listProcNumsForChargesInDb.Contains(entry.PriKey))
                {
                    dynamicPaymentPlanModule.ListPayPlanLinks.RemoveAll(x => x.LinkType == PayPlanLinkType.Procedure && x.FKey == entry.PriKey); //Sync will delete when saving.
                    dynamicPaymentPlanModule.ListPayPlanProductionEntries.RemoveAll(x => x.LinkType == PayPlanLinkType.Procedure && x.PriKey == entry.PriKey);
                    isDetatchedFromPlan = true;
                }

                if (isDetatchedFromPlan)
                {
                    continue; //this was a link that never ended up getting charged and saved in the database. Skip it. It was just deleted above. 
                }

                //make a charge for the amount that has yet to be put on the plan
                var charge = CreateDebitChargeDynamic(dynamicPaymentPlanModule.PayPlan, dynamicPaymentPlanModule.Family, entry.ProvNum, entry.ClinicNum, (double) entry.AmountRemaining, 0
                    , DateTime_.Today, Lans.g("PayPlanEdit", "Close Out Charge"), entry.PriKey, entry.LinkType);
                listChargesClosing.Add(charge);
            }

            dynamicPaymentPlanModule.ListCloseOutPayPlanCharges.AddRange(listChargesClosing);
            return dynamicPaymentPlanModule;
        }
        
        public static void CreateTransferForDynamicPaymentPlans(List<PayPlanRecalculationData> listRecalcData, bool isPayOnPrincipal)
        {
            for (var i = 0; i < listRecalcData.Count; i++)
            {
                CreateTransferForDynamicPaymentPlan(listRecalcData[i], isPayOnPrincipal);
            }
        }

        private static void CreateTransferForDynamicPaymentPlan(PayPlanRecalculationData recalcData, bool isPayOnPrincipal)
        {
            var listPaySplitsToInsert = new List<PaySplit>();
            var prepayUnearnedType = PrefC.GetLong(PrefName.DynamicPayPlanPrepaymentUnearnedType);
            var payPlan = recalcData.PayPlan;
            //Separate all of the charges up and keep track of the splits that are associated to said charges.
            var listPayPlanChargeSplits = recalcData.ListPayPlanCharges.Select(x => new PayPlanChargeSplits()
            {
                ListPaySplits = recalcData.ListPaySplits.FindAll(y => y.PayPlanChargeNum == x.PayPlanChargeNum),
                ListPaySplitsToTransfer = [],
                PayPlanChargeCur = x,
            }).ToList();

            #region Balance the Plan

            BalancePayPlanChargeSplits(ref listPayPlanChargeSplits, unearnedType: prepayUnearnedType);
            //Add any new splits that are required to balance out the payment plan to the list of splits to insert.
            listPaySplitsToInsert.AddRange(listPayPlanChargeSplits.SelectMany(x => x.ListPaySplitsToTransfer));

            #endregion

            #region Create and Apply Principal Charges and Payments

            //Apply overpayments to principal
            var listPrincipalChargeSplits = new List<PayPlanChargeSplits>();
            if (isPayOnPrincipal)
            {
                listPrincipalChargeSplits = CreatePrincipalChargeSplitsForAmount(recalcData.GetOverpaidAmount(), DateTime.Today, prepayUnearnedType, ref recalcData.ListProductionEntry, payPlan, recalcData.Fam);
                listPayPlanChargeSplits.AddRange(listPrincipalChargeSplits);
            }

            #endregion

            #region Payment Plan Overpaid

            var listChargesToConsider = listPayPlanChargeSplits.Select(x => x.PayPlanChargeCur).ToList();
            var listSplitsToConsider = listPayPlanChargeSplits.SelectMany(x => x.ListPaySplitsAll).ToList();
            //This will get us all future charges
            var listPayPlanChargesExpected = GetListExpectedCharges(listChargesToConsider, recalcData.Terms, recalcData.Fam, recalcData.ListPayPlanLinks, recalcData.PayPlan, false, listPaySplits: listSplitsToConsider);
            var listPlanOverpaidSplits = GetSplitsToTransferOverpaymentToUnearned(listChargesToConsider, listPayPlanChargesExpected, listSplitsToConsider, prepayUnearnedType, payPlan);
            //Add any overpaid payment splits that move value to regular unearned to the list of splits to insert.
            listPaySplitsToInsert.AddRange(listPlanOverpaidSplits);

            #endregion

            #region Insert Principal Charges and Transfer Payment

            //Create Principal Charges and Synchronize Payment Splits
            if (!listPrincipalChargeSplits.IsNullOrEmpty())
            {
                for (var i = 0; i < listPrincipalChargeSplits.Count; i++)
                {
                    PayPlanCharges.Insert(listPrincipalChargeSplits[i].PayPlanChargeCur);
                    listPrincipalChargeSplits[i].ListPaySplitsToTransfer.Where(x => x.UnearnedType == 0)
                        .ForEach(x => x.PayPlanChargeNum = listPrincipalChargeSplits[i].PayPlanChargeCur.PayPlanChargeNum);
                    listPaySplitsToInsert.AddRange(listPrincipalChargeSplits[i].ListPaySplitsToTransfer);
                }
            }

            listPaySplitsToInsert.RemoveAll(x => CompareDouble.IsZero(x.SplitAmt));
            //Create Transfer Payments
            if (listPaySplitsToInsert.Count > 0)
            {
                if (!CompareDouble.IsZero(listPaySplitsToInsert.Sum(x => x.SplitAmt)))
                {
                    throw new ApplicationException("Balancing overpaid interest for payment plan tried to create a non-zero amount income transfer.");
                }

                var transferPayment = new Payment();
                transferPayment.DateEntry = DateTime_.Today;
                transferPayment.PaymentSource = CreditCardSource.None;
                transferPayment.ProcessStatus = ProcessStat.OfficeProcessed;
                transferPayment.PayType = 0; //Income transfers are required to not have a payment type.
                transferPayment.PayAmt = 0; //Income transfers are required to be $0.
                transferPayment.PayDate = DateTime_.Today;
                transferPayment.PatNum = payPlan.PatNum;
                //Explicitly set ClinicNum=0, since a pat's ClinicNum will remain set if the user enabled clinics, assigned patients to clinics, and then
                //disabled clinics because we use the ClinicNum to determine which PayConnect or XCharge/XWeb credentials to use for payments.
                if (true)
                {
                    //if clinics aren't enabled default to 0
                    transferPayment.ClinicNum = Clinics.ClinicNum;
                    if ((PayClinicSetting) PrefC.GetInt(PrefName.PaymentClinicSetting) == PayClinicSetting.PatientDefaultClinic)
                    {
                        transferPayment.ClinicNum = recalcData.Guar.ClinicNum;
                    }
                    else if ((PayClinicSetting) PrefC.GetInt(PrefName.PaymentClinicSetting) == PayClinicSetting.SelectedExceptHQ)
                    {
                        transferPayment.ClinicNum = (Clinics.ClinicNum == 0 ? recalcData.Guar.ClinicNum : Clinics.ClinicNum);
                    }
                }

                Payments.Insert(transferPayment, listPaySplitsToInsert);
                var logText = Payments.GetSecuritylogEntryText(transferPayment, transferPayment, isNew: true) + ", " + Lans.g("PayPlanEdit", "from Payment Plan Balancer.");
                SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, transferPayment.PatNum, logText);
            }

            #endregion
        }

        private static void BalancePayPlanChargeSplits(ref List<PayPlanChargeSplits> listDppChargeSplits, long unearnedType = 0)
        {
            //Get a list of all of the charges that are overpaid.
            var listDppChargeSplitsOverpaid = listDppChargeSplits.FindAll(x => x.IsOverpaid);
            //Create transfers for the splits associated to each charge that has been overpaid.
            for (var i = 0; i < listDppChargeSplitsOverpaid.Count; i++)
            {
                var chargeSplitsPairTakingFrom = listDppChargeSplitsOverpaid[i];
                var bucket = GetDynamicPaymentPlanRecalculationBuckets(ListTools.FromSingle(chargeSplitsPairTakingFrom.PayPlanChargeCur), chargeSplitsPairTakingFrom.ListPaySplits);
                BalanceOverpaidChargeForDynamicPaymentPlan(bucket.InterestSplitAmtRem, PayPlanDebitTypes.Interest, ref listDppChargeSplits, chargeSplitsPairTakingFrom, unearnedType);
                BalanceOverpaidChargeForDynamicPaymentPlan(bucket.PrincipalSplitAmtRem, PayPlanDebitTypes.Principal, ref listDppChargeSplits, chargeSplitsPairTakingFrom, unearnedType);
                BalanceOverpaidChargeForDynamicPaymentPlan(bucket.UnknownSplitAmtRem, PayPlanDebitTypes.Unknown, ref listDppChargeSplits, chargeSplitsPairTakingFrom, unearnedType);
            }
        }

        private static void BalanceOverpaidChargeForDynamicPaymentPlan(double overpaidAmount, PayPlanDebitTypes payPlanDebitType, ref List<PayPlanChargeSplits> listDppChargeSplits, PayPlanChargeSplits chargeSplitsPairTakingFrom, long unearnedType = 0)
        {
            while (CompareDouble.IsGreaterThan(overpaidAmount, 0))
            {
                //Get a PayPlanCharge for the overpaid amount. Could be pre-existing, or a new charge.
                var chargeSplitsGivingTo = listDppChargeSplits.FirstOrDefault(x => x.IsUnderpaid); //TODO Change is deficit
                //There are no more charges that want money in either interest or principal. Therefore, there is nothing left to balance, return out of this method.
                if (chargeSplitsGivingTo == null)
                {
                    //The current value of the payment plan must be overpaid at this point.
                    //Calling methods can optionally suggest moving the remaining overpaidAmount directly to unearned.
                    if (unearnedType > 0)
                    {
                        var listPaySplitsForPrepay = CreateUnearnedTransfer(-overpaidAmount, chargeSplitsPairTakingFrom.PayPlanChargeCur, payPlanDebitType, unearnedType);
                        chargeSplitsPairTakingFrom.ListPaySplitsToTransfer.AddRange(listPaySplitsForPrepay);
                    }

                    return;
                }

                var bucketsForCharge = GetDynamicPaymentPlanRecalculationBuckets(ListTools.FromSingle(chargeSplitsGivingTo.PayPlanChargeCur), chargeSplitsGivingTo.ListPaySplitsAll);
                //GetChargeWithDeficit() always returns the first (oldest) charge in the list that has a deficit (FIFO).
                //Suggest explicit payment splits that will transfer overpayment to the charge with a deficit and will "fill up" interest first and then principal last.
                //Suggested transfers will never overpay the charge with a deficit. It is important to manipulate overpaidAmount correctly if additional transfers are needed.
                //Interest========================================================================================================================================
                var splitAmount = Math.Min(overpaidAmount, bucketsForCharge.InterestChargedRem);
                //Create negative split
                var paySplitNegative = CreateTransferSplitForDynamicPaymentPlanCharge(-splitAmount, chargeSplitsPairTakingFrom.PayPlanChargeCur, payPlanDebitType);
                chargeSplitsPairTakingFrom.ListPaySplitsToTransfer.Add(paySplitNegative);
                //Create positive split
                var paySplitPositive = CreateTransferSplitForDynamicPaymentPlanCharge(splitAmount, chargeSplitsGivingTo.PayPlanChargeCur, PayPlanDebitTypes.Interest);
                chargeSplitsGivingTo.ListPaySplitsToTransfer.Add(paySplitPositive);
                overpaidAmount -= splitAmount;
                if (CompareDouble.IsLessThanOrEqualToZero(overpaidAmount))
                {
                    return;
                }

                //Principal=======================================================================================================================================
                splitAmount = Math.Min(overpaidAmount, bucketsForCharge.PrincipalChargedRem);
                //Create negative split
                paySplitNegative = CreateTransferSplitForDynamicPaymentPlanCharge(-splitAmount, chargeSplitsPairTakingFrom.PayPlanChargeCur, payPlanDebitType);
                chargeSplitsPairTakingFrom.ListPaySplitsToTransfer.Add(paySplitNegative);
                //Create positive split
                paySplitPositive = CreateTransferSplitForDynamicPaymentPlanCharge(splitAmount, chargeSplitsGivingTo.PayPlanChargeCur, PayPlanDebitTypes.Principal);
                chargeSplitsGivingTo.ListPaySplitsToTransfer.Add(paySplitPositive);
                overpaidAmount -= splitAmount;
            }

            return;
        }

        private static List<PayPlanChargeSplits> CreatePrincipalChargeSplitsForAmount(double amountRequesting, DateTime chargeDate, long unearnedType, ref List<PayPlanProductionEntry> listProdEntries, PayPlan payPlan, Family fam)
        {
            var listPayOnPrincDppChargeSplits = new List<PayPlanChargeSplits>();
            //Create as many principal payment plan charges as possible, up to amountRequesting.
            var listPayPlanChargesNew = CreateChargesForPrincipalPayment(amountRequesting, ref listProdEntries, chargeDate, payPlan, fam);
            //Loop through all charges suggested and blindly make a DynamicPaymentPlanRecalculationChargeSplits object for each one and return 
            for (var i = 0; i < listPayPlanChargesNew.Count; i++)
            {
                //If no more charges can be created, the plan has maxed out its value, and has been entirely overpaid. We shouldn't reach this point.
                var chargeGivingTo = listPayPlanChargesNew[i];
                var chargeSplitsGivingTo = new PayPlanChargeSplits();
                chargeSplitsGivingTo.ListPaySplits = [];
                chargeSplitsGivingTo.ListPaySplitsToTransfer = [];
                chargeSplitsGivingTo.PayPlanChargeCur = chargeGivingTo;
                var splitAmt = Math.Min(amountRequesting, chargeGivingTo.Principal);
                amountRequesting -= splitAmt;
                chargeSplitsGivingTo.ListPaySplitsToTransfer.AddRange(CreateUnearnedTransfer(splitAmt, chargeGivingTo, PayPlanDebitTypes.Principal, unearnedType));
                listPayOnPrincDppChargeSplits.Add(chargeSplitsGivingTo);
            }

            return listPayOnPrincDppChargeSplits;
        }

        private static List<PayPlanCharge> CreateChargesForPrincipalPayment(double amountToCharge, ref List<PayPlanProductionEntry> listProdEntries, DateTime chargeDate, PayPlan payPlan, Family fam)
        {
            var listPayPlanCharges = new List<PayPlanCharge>();
            if (CompareDouble.IsLessThanOrEqualToZero(amountToCharge))
            {
                return listPayPlanCharges;
            }

            //Check to see if there are any production entries with an amount remaining that has yet to be charged out.
            var listEntries = listProdEntries.FindAll(x => CompareDecimal.IsGreaterThanZero(x.AmountRemaining));
            for (var i = 0; i < listEntries.Count; i++)
            {
                if (CompareDouble.IsLessThanOrEqualToZero(amountToCharge))
                {
                    break;
                }

                var principal = Math.Min(amountToCharge, (double) listEntries[i].AmountRemaining);
                //There is at least one outstanding production entry that can have a principal charge created for it.
                //If we have already created a payment plan charge for this outstanding production entry before, simply apply as much amountToTransfer to it as possible.
                var payPlanCharge = GetPayPlanChargeForEntry(listEntries[i], payPlan, principal, chargeDate, fam);
                payPlanCharge.Note = Lans.g("PayPlanEdit", "(Charge created for pay on principal)");
                listEntries[i].AmountRemaining -= (decimal) principal;
                amountToCharge -= principal;
                listPayPlanCharges.Add(payPlanCharge);
            }

            return listPayPlanCharges;
        }

        private static List<PaySplit> GetSplitsToTransferOverpaymentToUnearned(List<PayPlanCharge> listCharges, List<PayPlanCharge> listChargesExpected, List<PaySplit> listSplits, long prepayUnearnedType, PayPlan payPlan)
        {
            var listSplitsToTransfer = new List<PaySplit>();
            var totalCharges = listCharges.Sum(x => x.Principal + x.Interest) + listChargesExpected.Sum(x => x.Principal + x.Interest);
            var totalSplits = listSplits.Sum(x => x.SplitAmt);
            if (totalSplits > totalCharges)
            {
                var amountOverpaid = totalSplits - totalCharges;
                //This is the amount that is currently in hidden unearned, before we have moved in any suggested hidden unearned.
                var amountInHiddenUnearned = listSplits.FindAll(x => x.UnearnedType == prepayUnearnedType).Sum(x => x.SplitAmt);
                var amountToTransfer = Math.Min(amountOverpaid, amountInHiddenUnearned);
                if (CompareDecimal.IsGreaterThanZero(amountToTransfer))
                {
                    var notUsed = new PaymentEdit.IncomeTransferData();
                    listSplitsToTransfer.AddRange(PaymentEdit.CreateUnearnedTransfer((decimal) amountToTransfer, payPlan.Guarantor, 0, 0, ref notUsed, unearnedType: prepayUnearnedType, payPlanNum: payPlan.PayPlanNum, isMovingFromHiddenUnearnedToUnearned: true));
                }
            }

            return listSplitsToTransfer;
        }

        public static List<PayPlanRecalculationData> GetRecalculationDataForDynamicPaymentPlans(Patient pat, List<PaySplit> listSplitsCur)
        {
            var listRecalcData = new List<PayPlanRecalculationData>();
            if (listSplitsCur.All(x => x.PayPlanNum == 0))
            {
                return listRecalcData;
            }

            var listPayPlanNums = listSplitsCur.Where(x => x.PayPlanNum != 0).Select(x => x.PayPlanNum).Distinct().ToList();
            var listPayPlans = PayPlans.GetMany(listPayPlanNums.ToArray());
            var listPayPlanCharges = PayPlanCharges.GetForPayPlans(listPayPlanNums).FindAll(x => x.ChargeDate.Date <= DateTime_.Today);
            var listPaySplits = GetPaymentPlanSplitsWithPaymentOverride(listSplitsCur);
            var listPayPlanLinks = PayPlanLinks.GetForPayPlans(listPayPlanNums);
            var listPayPlanProductionEntries = PayPlanProductionEntry.GetWithAmountRemaining(listPayPlanLinks, listPayPlanCharges);
            for (var i = 0; i < listPayPlans.Count; i++)
            {
                var payPlan = listPayPlans[i];
                var family = Patients.GetFamily(payPlan.PatNum);
                var listPayPlanLinksForPlan = listPayPlanLinks.FindAll(x => x.PayPlanNum == payPlan.PayPlanNum);
                var terms = GetPayPlanTerms(payPlan, listPayPlanLinksForPlan);
                var listPayPlanChargesForPlan = listPayPlanCharges.FindAll(x => x.PayPlanNum == payPlan.PayPlanNum);
                var listPaySplitsForPlan = listPaySplits.FindAll(x => x.PayPlanNum == payPlan.PayPlanNum);
                var listPayPlanProductionEntriesForPlan = listPayPlanProductionEntries.FindAll(x => x.PayPlanNum == payPlan.PayPlanNum);
                var areChargesOverpaid = IsDynamicPaymentPlanInterestOrPrincipalOverpaid(listPayPlanChargesForPlan, listPaySplitsForPlan.FindAll(x => x.UnearnedType == 0));
                if (areChargesOverpaid)
                {
                    if (payPlan.DynamicPayPlanTPOption == DynamicPayPlanTPOptions.AwaitComplete)
                    {
                        listPayPlanProductionEntriesForPlan.RemoveAll(x => x.LinkType == PayPlanLinkType.Procedure && !x.IsProcedureCompleted);
                    }

                    listRecalcData.Add(PayPlanRecalculationData.CreateRecalculationData(terms, payPlan, family, 0, 0, listPayPlanChargesForPlan, listPaySplitsForPlan, false, false, listPayPlanLinks: listPayPlanLinksForPlan, listProductionEntry: listPayPlanProductionEntriesForPlan));
                }
            }

            return listRecalcData;
        }

        public static bool AreAnyPayPlansOverpaid(List<PaySplit> listSplitsCur)
        {
            if (listSplitsCur.IsNullOrEmpty())
            {
                return false;
            }

            var listPayPlanNums = listSplitsCur.Where(x => x.PayPlanNum != 0).Select(x => x.PayPlanNum).Distinct().ToList();
            var listPayPlanCharges = PayPlanCharges.GetForPayPlans(listPayPlanNums);
            var listPaySplits = GetPaymentPlanSplitsWithPaymentOverride(listSplitsCur);
            for (var i = 0; i < listPayPlanNums.Count; i++)
            {
                var totalPaySplitsForPlan = listPaySplits.Where(x => x.PayPlanNum == listPayPlanNums[i] && x.UnearnedType == 0).Sum(x => x.SplitAmt);
                var totalChargesForPlan = listPayPlanCharges.Where(x => x.PayPlanNum == listPayPlanNums[i]).Sum(x => x.Interest + x.Principal);
                if (CompareDouble.IsGreaterThan(totalPaySplitsForPlan, totalChargesForPlan))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDynamicPaymentPlanInterestOrPrincipalOverpaid(List<PayPlanCharge> listPayPlanCharges, List<PaySplit> listPaySplits)
        {
            var buckets = GetDynamicPaymentPlanRecalculationBuckets(listPayPlanCharges, listPaySplits);
            return CompareDouble.IsGreaterThan(buckets.InterestSplitAmtRem, 0) || CompareDouble.IsGreaterThan(buckets.PrincipalSplitAmtRem, 0) || CompareDouble.IsGreaterThan(buckets.UnknownSplitAmtRem, 0);
        }

        public static bool IsDynamicPaymentPlanInterestOrPrincipalUnderpaid(List<PayPlanCharge> listPayPlanCharges, List<PaySplit> listPaySplits)
        {
            var buckets = GetDynamicPaymentPlanRecalculationBuckets(listPayPlanCharges, listPaySplits);
            return CompareDouble.IsGreaterThan(buckets.InterestChargedRem, 0) || CompareDouble.IsGreaterThan(buckets.PrincipalChargedRem, 0);
        }

        private static List<PaySplit> GetPaymentPlanSplitsWithPaymentOverride(List<PaySplit> listSplitsCur)
        {
            if (listSplitsCur.IsNullOrEmpty())
            {
                return [];
            }

            var payNum = listSplitsCur.First().PayNum;
            //Figure out which payment plans are important.
            var listPayPlanNums = listSplitsCur.Where(x => x.PayPlanNum != 0).Select(x => x.PayPlanNum).Distinct().ToList();
            //Get all splits for said payment plans from the database.
            var listPaySplits = PaySplits.GetForPayPlans(listPayPlanNums);
            //Remove all splits associated to the payment passed in.
            listPaySplits.RemoveAll(x => x.PayNum == payNum);
            //Add all of the splits passed in that are now posing as the official splits for said payment passed in.
            listPaySplits.AddRange(listSplitsCur.FindAll(x => x.PayPlanNum != 0));
            return listPaySplits;
        }

        private static List<PaySplit> CreateUnearnedTransfer(double splitAmount, PayPlanCharge charge, PayPlanDebitTypes debitType, long unearnedType)
        {
            var patNumResponsible = charge.PatNum;
            long procNum = 0;
            long adjNum = 0;
            if (charge.LinkType == PayPlanLinkType.Procedure && debitType != PayPlanDebitTypes.Interest)
            {
                procNum = charge.FKey;
            }
            else if (charge.LinkType == PayPlanLinkType.Adjustment && debitType != PayPlanDebitTypes.Interest)
            {
                adjNum = charge.FKey;
            }

            if (PrefC.GetEnum<PayPlanVersions>(PrefName.PayPlansVersion) != PayPlanVersions.NoCharges)
            {
                patNumResponsible = charge.Guarantor;
            }

            var notUsed = new PaymentEdit.IncomeTransferData();
            var listPaySplit = PaymentEdit.CreateUnearnedTransfer(
                (decimal) splitAmount,
                patNumResponsible,
                charge.ProvNum,
                charge.ClinicNum,
                ref notUsed,
                procNum: procNum,
                adjNum: adjNum,
                unearnedType: unearnedType,
                payPlanNum: charge.PayPlanNum,
                payPlanChargeNum: charge.PayPlanChargeNum,
                payPlanDebitType: debitType,
                isForDynamicPaymentPlanBalancing: true);
            return listPaySplit;
        }

        private static PaySplit CreateTransferSplitForDynamicPaymentPlanCharge(double splitAmount, PayPlanCharge charge, PayPlanDebitTypes debitType)
        {
            var fx = new FauxAccountEntry(charge, true);
            fx.PayPlanDebitType = debitType;
            return PaymentEdit.CreatePaySplitHelper(fx, splitAmount, DateTime_.Today, isNew: true);
        }

        public static PayPlanCharge GetPayPlanChargeForEntry(PayPlanProductionEntry payPlanProductionEntry, PayPlan payPlan, double principal, DateTime chargeDate, Family famCur)
        {
            var charge = new PayPlanCharge();
            charge.ChargeDate = chargeDate;
            charge.ChargeType = PayPlanChargeType.Debit;
            charge.LinkType = payPlanProductionEntry.LinkType;
            charge.FKey = payPlanProductionEntry.PriKey;
            charge.ProcNum = 0; //ProcNum should be 0 when PayPlanChargeType is ChargeDue
            charge.PatNum = payPlanProductionEntry.PatNum;
            charge.ProvNum = payPlanProductionEntry.ProvNum;
            charge.ClinicNum = payPlanProductionEntry.ClinicNum;
            //FamCur is the family of the patient, so check to see if the guarantor is in the patient's family. 
            //If the guar and pat are in the same family, then use the patnum. Otherwise, use guarantor.
            if (famCur.ListPats.Select(x => x.PatNum).Contains(payPlan.Guarantor))
            {
                charge.Guarantor = payPlan.PatNum;
            }
            else
            {
                charge.Guarantor = payPlan.Guarantor;
            }

            charge.PayPlanNum = payPlan.PayPlanNum;
            charge.IsOffset = false;
            charge.Interest = 0;
            charge.IsNew = true;
            charge.Principal = principal;
            return charge;
        }

        public static decimal CalculatePeriodPayment(double apr, PayPlanFrequency frequency, decimal periodPayment, int payCount, int roundDec, double principalAmount, double downPayment)
        {
            var periodRate = CalcPeriodRate(apr, frequency);
            return CalculatePeriodPayment(payCount, apr, periodPayment, roundDec, principalAmount - downPayment, periodRate);
        }

        public static decimal CalculatePeriodPayment(int payCount, double apr, decimal periodPayment, int roundDec, double principalAmount, double periodRate)
        {
            decimal periodPaymentAmt = 0;
            if (payCount == 0)
            {
                periodPaymentAmt = periodPayment;
            }
            else
            {
                //will need to save what this initial periodPaymentAmt is and use it for the future
                double periodExactAmt = 0;
                if (apr == 0)
                {
                    periodExactAmt = principalAmount / payCount;
                }
                else
                {
                    periodExactAmt = principalAmount * periodRate / (1 - Math.Pow(1 + periodRate, -payCount));
                }

                //Round up to the nearest penny (or international equivalent).
                //This causes the principal on the last payment to be less than or equal to the other principal amounts.
                periodPaymentAmt = (decimal) (Math.Ceiling(periodExactAmt * Math.Pow(10, roundDec)) / Math.Pow(10, roundDec));
            }

            return periodPaymentAmt;
        }

        private static double CalculatePrincipalAmountForPeriod(decimal sumPrincipalNotYetCharged, PayPlanTerms terms, double interest)
        {
            //sumPrincipalNotCharged is the sum of the principal that is expected to be charged out in the future
            double principal;
            if (sumPrincipalNotYetCharged < (terms.PeriodPayment - (decimal) interest))
            {
                principal = (double) sumPrincipalNotYetCharged;
            }
            else
            {
                principal = (double) terms.PeriodPayment - interest;
            }

            if (sumPrincipalNotYetCharged + (decimal) interest <= terms.PeriodPayment)
            {
                principal = (double) sumPrincipalNotYetCharged; //all remaining principal.
            }

            return principal;
        }

        private static double CalculateInterestAmountForPeriod(decimal sumPrincipalNotYetCharged, PayPlanTerms terms, DateTime periodDate, bool isNextPeriodOnly, List<PaySplit> listPaySplits, List<PayPlanCharge> listChargesInDB, List<PayPlanCharge> listExpectedCharges)
        {
            double interest = 0;
            if (periodDate.Date >= terms.DateInterestStart.Date)
            {
                var periodRate = CalcPeriodRate(terms.APR, terms.Frequency);
                var sumChargesAllPrincipal = listChargesInDB.Sum(x => x.Principal) + listExpectedCharges.Sum(x => x.Principal);
                //Figure out the total principal value for the plan by adding up the principal on the charges in the database, the 'future' charges that 
                //have been calculated, and the amount of principal that has yet to be considered (via the calling method).
                var principalAmount = sumChargesAllPrincipal + (double) sumPrincipalNotYetCharged;
                var principalAmountUsedForCalculation = CalculatePrincipalRemaining(principalAmount, listPaySplits, listChargesInDB
                    , listExpectedCharges, isNextPeriodOnly);
                interest = Math.Round((principalAmountUsedForCalculation * periodRate), terms.RoundDec);
            }

            return interest;
        }

        public static PaymentSchedule GetPayScheduleFromFrequency(PayPlanFrequency frequency)
        {
            switch (frequency)
            {
                case PayPlanFrequency.Weekly:
                    return PaymentSchedule.Weekly;
                case PayPlanFrequency.EveryOtherWeek:
                    return PaymentSchedule.BiWeekly;
                case PayPlanFrequency.OrdinalWeekday:
                    return PaymentSchedule.MonthlyDayOfWeek;
                case PayPlanFrequency.Monthly:
                    return PaymentSchedule.Monthly;
                case PayPlanFrequency.Quarterly:
                    return PaymentSchedule.Quarterly;
                default:
                    return PaymentSchedule.Monthly; //most common
            }
        }

        public static PayPlanTerms GetPayPlanTerms(PayPlan payPlan, List<PayPlanLink> listLinksForPayPlan)
        {
            var terms = new PayPlanTerms();
            terms.APR = payPlan.APR;
            terms.DateInterestStart = payPlan.DateInterestStart;
            terms.DateAgreement = payPlan.PayPlanDate;
            terms.DateFirstPayment = payPlan.DatePayPlanStart;
            terms.DownPayment = payPlan.DownPayment;
            terms.Frequency = payPlan.ChargeFrequency;
            terms.PaySchedule = payPlan.PaySchedule;
            //payCount is excluded because it does not apply to dynamic payment plans
            terms.PeriodPayment = (decimal) payPlan.PayAmt;
            terms.DynamicPayPlanTPOption = payPlan.DynamicPayPlanTPOption;
            terms.PrincipalAmount = (double) PayPlanProductionEntry.GetProductionForLinks(listLinksForPayPlan)
                .Sum(x => x.AmountOverride == 0 ? x.AmountOriginal : x.AmountOverride);
            terms.RoundDec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits;
            return terms;
        }

        public static string ValidateDynamicPaymentPlanTerms(PayPlanTerms payPlanTerms, bool isNew, bool isLocked, bool doCheckApr, int linkedProdCount)
        {
            var sb = new StringBuilder("");
            //User has to enter a payment amount when creating a payplan with no linked prods
            if (linkedProdCount == 0 && payPlanTerms.PeriodPayment == 0 && payPlanTerms.PayCount == 0)
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Please enter a payment amount."));
            }
            //User can enter either when creating a payplan with linked prods
            else if (linkedProdCount != 0 && payPlanTerms.PeriodPayment == 0 && payPlanTerms.PayCount == 0)
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Please enter a term or payment amount first."));
            }

            if (payPlanTerms.DateAgreement > DateTime_.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed))
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Payment plan date cannot be set for the future."));
            }

            if (linkedProdCount != 0 && payPlanTerms.PayCount == 0 && payPlanTerms.PeriodPayment == 0)
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Payment cannot be 0."));
            }

            if (payPlanTerms.PayCount != 0 && payPlanTerms.PeriodPayment != 0)
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Please choose either Number of Payments or Payment Amt."));
            }

            if (linkedProdCount != 0 && payPlanTerms.PeriodPayment == 0 && payPlanTerms.PayCount < 1)
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Term cannot be less than 1."));
            }

            if (payPlanTerms.PrincipalAmount - payPlanTerms.DownPayment < 0)
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Down payment must be less than or equal to total amount."));
            }

            if (doCheckApr && !CompareDouble.IsZero(payPlanTerms.APR) && PrefC.GetBool(PrefName.PayPlanRequireLockForAPR) && !isLocked)
            {
                sb.AppendLine(Lans.g("FormPayPlanDynamic", "Payment plans with APR must be locked. Remove the APR or check the box for Permanent Lock."));
            }

            Action<string> actionNotAuthorized = (msg) => { sb.AppendLine(Lans.g("FormPayPlanDynamic", msg)); };
            Security.IsGlobalDateLock(EnumPermType.PayPlanEdit, payPlanTerms.DateAgreement, suppressMsgBox: true, actionNotAuthorized: actionNotAuthorized);
            return sb.ToString();
        }

        public static PayPlan ApplyPayPlanTermsToDynamicPaymentPlan(PayPlan payPlan, PayPlanTerms payPlanTerms, long defNumPlanCategory)
        {
            var retVal = payPlan;
            var family = Patients.GetFamily(payPlan.PatNum);
            retVal.PayPlanDate = payPlanTerms.DateAgreement;
            retVal.IsDynamic = true;
            retVal.Note = payPlanTerms.Note;
            retVal.ChargeFrequency = payPlanTerms.Frequency;
            retVal.NumberOfPayments = 0; //This should never be set. We only store the payment amount for dynamic payplans. 
            retVal.PayAmt = (double) payPlanTerms.PeriodPayment;
            retVal.APR = payPlanTerms.APR;
            retVal.DownPayment = payPlanTerms.DownPayment;
            retVal.PaySchedule = GetPayScheduleFromFrequency(payPlanTerms.Frequency);
            retVal.DatePayPlanStart = payPlanTerms.DateFirstPayment;
            retVal.DateInterestStart = payPlanTerms.DateInterestStart;
            retVal.DynamicPayPlanTPOption = payPlanTerms.DynamicPayPlanTPOption;
            retVal.PlanCategory = defNumPlanCategory;
            retVal.SheetDefNum = payPlanTerms.SheetDefNum;
            return retVal;
        }

        public static DynamicPaymentPlanModuleData SaveDynamicPaymentPlanAndTerms(PayPlanTerms payPlanTerms, DynamicPaymentPlanModuleData dynamicPaymentPlanModuleData, bool isLocking, long defNumPlanCategory)
        {
            //Apply the passed in terms to the payment plan.
            dynamicPaymentPlanModuleData.PayPlan = ApplyPayPlanTermsToDynamicPaymentPlan(dynamicPaymentPlanModuleData.PayPlan, payPlanTerms, defNumPlanCategory);
            //Store the changes. Issue out any additional charges needed for down payments.
            var payPlan = dynamicPaymentPlanModuleData.PayPlan;
            var family = Patients.GetFamily(dynamicPaymentPlanModuleData.PayPlan.PatNum);
            var listPayPlanChargesDB = dynamicPaymentPlanModuleData.ListPayPlanChargesDb;
            var listPayPlanProductionEntries = dynamicPaymentPlanModuleData.ListPayPlanProductionEntries;
            var listPayPlanLink = dynamicPaymentPlanModuleData.ListPayPlanLinks;
            payPlan.CompletedAmt = PayPlanProductionEntry.GetDynamicPayPlanCompletedAmount(payPlan, listPayPlanProductionEntries);
            var listPayPlanChargesDownPayment = new List<PayPlanCharge>();
            if (payPlan.IsNew)
            {
                listPayPlanChargesDownPayment = GetDownPaymentCharges(payPlan, payPlanTerms, listPayPlanLink);
            }

            if (payPlan.IsNew && payPlanTerms.DateFirstPayment.Date <= DateTime_.Today)
            {
                //immediately call the code to run the "service" on this payment plan in case they created a plan who's first charge is today or in the past. 
                var listPayPlanCharges = GetListExpectedCharges(listPayPlanChargesDB, payPlanTerms, family, listPayPlanLink, payPlan, false
                        , listPaySplits: dynamicPaymentPlanModuleData.ListPaySplits, listExpectedChargesDownPayment: listPayPlanChargesDownPayment)
                    .FindAll(x => x.ChargeDate <= DateTime_.Today);
                if (listPayPlanCharges.Count > 0)
                {
                    PayPlanCharges.InsertMany(listPayPlanCharges);
                    listPayPlanChargesDB.AddRange(listPayPlanCharges);
                }
            }
            else if (listPayPlanChargesDownPayment.Count > 0)
            {
                listPayPlanChargesDB.AddRange(listPayPlanChargesDownPayment);
            }

            if (!payPlan.IsLocked && isLocking)
            {
                for (var i = 0; i < listPayPlanProductionEntries.Count; i++)
                {
                    //find matching credit.
                    var payPlanLinkCredit = listPayPlanLink.FirstOrDefault(x => x.FKey == listPayPlanProductionEntries[i].PriKey && x.LinkType == listPayPlanProductionEntries[i].LinkType);
                    if (CompareDouble.IsEqual(payPlanLinkCredit.AmountOverride, 0))
                    {
                        //credit has not been set, set amount override so production never changes.
                        payPlanLinkCredit.AmountOverride = (double) dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].AmountOriginal;
                    }
                }

                payPlan.IsLocked = true;
            }

            payPlan.IsNew = false;
            PayPlans.Update(payPlan); //always saved to db before opening this form
            PayPlanLinks.Sync(listPayPlanLink, payPlan.PayPlanNum);
            dynamicPaymentPlanModuleData.PayPlan = payPlan;
            dynamicPaymentPlanModuleData.ListPayPlanChargesDb = listPayPlanChargesDB;
            dynamicPaymentPlanModuleData.ListPayPlanProductionEntries = listPayPlanProductionEntries;
            dynamicPaymentPlanModuleData.ListPayPlanLinks = listPayPlanLink;
            return dynamicPaymentPlanModuleData;
        }

        public static List<PayPlanCharge> GetDownPaymentCharges(PayPlan payPlan, PayPlanTerms payPlanTerms, List<PayPlanLink> listPayPlanLinks)
        {
            var family = Patients.GetFamily(payPlan.PatNum);
            var listPayPlanChargesDownPayment = new List<PayPlanCharge>();
            if (!CompareDouble.IsZero(payPlanTerms.DownPayment))
            {
                //insert down payment(s) if none exist. Get a new copy from DB so we can guarantee no one else has made this payment plan yet. 
                if (PayPlanCharges.GetForPayPlan(payPlan.PayPlanNum).Count == 0)
                {
                    var listPayPlanChargesDownPayments = PayPlanCharges.GetForDownPayment(payPlanTerms, family, listPayPlanLinks, payPlan);
                    for (var i = 0; i < listPayPlanChargesDownPayments.Count; i++)
                    {
                        PayPlanCharges.Insert(listPayPlanChargesDownPayments[i]);
                        listPayPlanChargesDownPayment.Add(listPayPlanChargesDownPayments[i]);
                    }
                }
            }

            return listPayPlanChargesDownPayment;
        }

        public static DynamicPaymentPlanModuleData CloseOutDynamicPaymentPlan(PayPlanTerms payPlanTerms, DynamicPaymentPlanModuleData dynamicPaymentPlanModuleData, bool isLocked, long categoryDef)
        {
            //make payment plan charges for the remaining amount of each piece of production
            var family = Patients.GetFamily(dynamicPaymentPlanModuleData.PayPlan.PatNum);
            dynamicPaymentPlanModuleData = GetCloseoutChargesForDynamicPaymentPlan(dynamicPaymentPlanModuleData);
            var listPaySplits = PaySplits.GetForPayPlans(ListTools.FromSingle(dynamicPaymentPlanModuleData.PayPlan.PayPlanNum));
            var hiddenUnearnedTotal = GetDynamicPayPlanPrepaymentAmount(listPaySplits);
            ApplyPrepaymentToDynamicPaymentPlan(
                dynamicPaymentPlanModuleData.PayPlan.PatNum,
                hiddenUnearnedTotal, dynamicPaymentPlanModuleData.ListCloseOutPayPlanCharges,
                listPaySplits,
                payPlanNum: dynamicPaymentPlanModuleData.PayPlan.PayPlanNum,
                isClosing: true);
            dynamicPaymentPlanModuleData.PayPlan.IsClosed = true;
            dynamicPaymentPlanModuleData.PayPlan = ApplyPayPlanTermsToDynamicPaymentPlan(dynamicPaymentPlanModuleData.PayPlan, payPlanTerms, categoryDef);
            dynamicPaymentPlanModuleData = SaveDynamicPaymentPlanAndTerms(
                payPlanTerms,
                dynamicPaymentPlanModuleData,
                isLocked,
                categoryDef);
            //When sign & print is clicked (saves the plan) on new plan to let users know it has been saved and some thing can no longer be edited.
            //Refresh local class wide variables.
            PayPlanCharges.InsertMany(dynamicPaymentPlanModuleData.ListCloseOutPayPlanCharges);
            //Update all future dated charges to be due today.
            for (var i = 0; i < dynamicPaymentPlanModuleData.ListPayPlanChargesDb.Count; i++)
            {
                if (dynamicPaymentPlanModuleData.ListPayPlanChargesDb[i].ChargeDate <= DateTime.Today)
                {
                    continue;
                }

                dynamicPaymentPlanModuleData.ListPayPlanChargesDb[i].ChargeDate = DateTime.Today;
                PayPlanCharges.Update(dynamicPaymentPlanModuleData.ListPayPlanChargesDb[i]);
            }

            CreditCards.RemoveRecurringCharges(dynamicPaymentPlanModuleData.PayPlan.PayPlanNum);
            return dynamicPaymentPlanModuleData;
        }

        public static DynamicPaymentPlanModuleData AddProductionToDynamicPaymentPlan(List<AccountEntry> listAccountEntries, DynamicPaymentPlanModuleData dynamicPaymentPlanModuleData)
        {
            var listProductionEntriesAdded = new List<PayPlanProductionEntry>();
            //Add production selected procedures/adjustments. Adding to _listPayPlanLinks also prevents GetAccountEntriesUnattached() from adding currently linked procedures/adjustments.
            for (var i = 0; i < listAccountEntries.Count; i++)
            {
                if (listAccountEntries[i].GetType() == typeof(Adjustment))
                {
                    var adjustmentAdding = (Adjustment) listAccountEntries[i].Tag;
                    var payPlanLinkCreditAdding = new PayPlanLink();
                    payPlanLinkCreditAdding.PayPlanNum = dynamicPaymentPlanModuleData.PayPlan.PayPlanNum;
                    payPlanLinkCreditAdding.LinkType = PayPlanLinkType.Adjustment;
                    payPlanLinkCreditAdding.FKey = adjustmentAdding.AdjNum;
                    listProductionEntriesAdded.Add(new PayPlanProductionEntry(adjustmentAdding, payPlanLinkCreditAdding, null, amountOriginal: listAccountEntries[i].AmountEnd));
                }
                else if (listAccountEntries[i].GetType() == typeof(Procedure))
                {
                    var procedure = (Procedure) listAccountEntries[i].Tag;
                    var payPlanLinkCreditAdding = new PayPlanLink();
                    payPlanLinkCreditAdding.PayPlanNum = dynamicPaymentPlanModuleData.PayPlan.PayPlanNum;
                    payPlanLinkCreditAdding.LinkType = PayPlanLinkType.Procedure;
                    payPlanLinkCreditAdding.FKey = procedure.ProcNum;
                    listProductionEntriesAdded.Add(new PayPlanProductionEntry(procedure, payPlanLinkCreditAdding, null, null, null, amountOriginal: listAccountEntries[i].AmountEnd));
                }
            }

            dynamicPaymentPlanModuleData.ListPayPlanLinks.AddRange(listProductionEntriesAdded.Select(x => x.LinkedCredit).ToList());
            dynamicPaymentPlanModuleData.ListPayPlanProductionEntries.AddRange(listProductionEntriesAdded);
            dynamicPaymentPlanModuleData.ListPayPlanProductionEntries.Sort(OrderDynamicPayPlanProductionEntries);
            return dynamicPaymentPlanModuleData;
        }

        public static DynamicPaymentPlanModuleData RemoveProductionFromDynamicPaymentPlan(List<PayPlanProductionEntry> listPayPlanProductionEntriesToRemove, DynamicPaymentPlanModuleData dynamicPaymentPlanModuleData)
        {
            //The return value needs to be items from listPayPlanProductionEntriesToRemove.
            var listPayPlanLinkTypes = listPayPlanProductionEntriesToRemove.Select(x => x.LinkType).ToList();
            var listPayPlanChargesForEntry = new List<PayPlanCharge>();
            for (var i = 0; i < listPayPlanLinkTypes.Count; i++)
            {
                var listPayPlanProductionEntriesFiltered = listPayPlanProductionEntriesToRemove.FindAll(x => x.LinkType == listPayPlanLinkTypes[i]);
                listPayPlanChargesForEntry.AddRange(PayPlanCharges.GetForLinkTypeAndFKeys(listPayPlanLinkTypes[i], listPayPlanProductionEntriesFiltered.Select(x => x.PriKey).ToArray()));
            }

            for (var i = 0; i < listPayPlanProductionEntriesToRemove.Count; i++)
            {
                //check to see if payments exist for the entry and disallow removing while payments exist.
                switch (listPayPlanProductionEntriesToRemove[i].LinkType)
                {
                    case PayPlanLinkType.Adjustment:
                        if (dynamicPaymentPlanModuleData.ListPaySplitsForCharges.Any(x => x.AdjNum == listPayPlanProductionEntriesToRemove[i].PriKey))
                        {
                            continue; //patient payments are attached, not valid for deletion.
                        }

                        break;
                    case PayPlanLinkType.Procedure:
                        if (dynamicPaymentPlanModuleData.ListPaySplitsForCharges.Any(x => x.ProcNum == listPayPlanProductionEntriesToRemove[i].PriKey))
                        {
                            continue; //patient payments are attached, not valid for deletion.
                        }

                        break;
                    default:
                        continue; //Unknown link type, play it safe and do not delete the charges.
                }

                var listPayPlanChargeNumsDeleting = listPayPlanChargesForEntry.Select(x => x.PayPlanChargeNum).ToList();
                PayPlanCharges.DeleteMany(listPayPlanChargeNumsDeleting);
                PayPlanLinks.Delete(listPayPlanProductionEntriesToRemove[i].LinkedCredit.PayPlanLinkNum);
                dynamicPaymentPlanModuleData.ListPayPlanLinks.Remove(listPayPlanProductionEntriesToRemove[i].LinkedCredit);
                dynamicPaymentPlanModuleData.ListPayPlanProductionEntries.Remove(listPayPlanProductionEntriesToRemove[i]);
                dynamicPaymentPlanModuleData.ListPayPlanChargesDb = PayPlanCharges.GetForPayPlan(dynamicPaymentPlanModuleData.PayPlan.PayPlanNum).OrderBy(x => x.ChargeDate).ToList();
            }

            return dynamicPaymentPlanModuleData;
        }

        public static List<AccountEntry> GetAccountEntriesUnattachedToDynamicPaymentPlan(DynamicPaymentPlanModuleData dynamicPaymentPlanModuleData)
        {
            var patient = dynamicPaymentPlanModuleData.Patient;
            var family = dynamicPaymentPlanModuleData.Family;
            //Gather account information as if an income transfer is about to be made so that explicit linking is the only type of linking performed.
            //Unattached positive adjustments can be associated with dynamic payment plans so turn off the offsetting adjustments logic so their values are not manipulated.
            var constructResults = PaymentEdit.ConstructAndLinkChargeCredits(patient.PatNum, listPatNums: family.GetPatNums(),
                isIncomeTxfr: true, doIncludeTreatmentPlanned: true, hasOffsettingAdjustmets: false);
            var listTypesProd = new List<Type>() {typeof(Adjustment), typeof(Procedure)};

            #region Invalid Procedures

            var listAccountEntriesAll = constructResults.ListAccountEntries;
            var listProcNums = listAccountEntriesAll.Where(x => x.GetType() == typeof(Procedure)).Select(x => x.PriKey).ToList();
            var listPayPlanChargesCreditsForProcs = PayPlanCharges.GetPatientPayPlanCreditsForProcs(listProcNums);
            var listPayPlanLinksForProcs = PayPlanLinks.GetForFKeysAndLinkType(listProcNums, PayPlanLinkType.Procedure);
            //Already attached to this dynamic payment plan.
            var listInvalidProcNums = dynamicPaymentPlanModuleData.ListPayPlanLinks
                .FindAll(x => x.LinkType == PayPlanLinkType.Procedure)
                .Select(x => x.FKey)
                .ToList();
            //Already attached to another patient payment plan.
            listInvalidProcNums.AddRange(listPayPlanChargesCreditsForProcs.Select(x => x.ProcNum));
            //Already attached to another dynamic payment plan.
            listInvalidProcNums.AddRange(listPayPlanLinksForProcs.Select(x => x.FKey));

            #endregion

            #region Invalid Adjustments

            var listAdjNums = listAccountEntriesAll
                .Where(x => x.GetType() == typeof(Adjustment))
                .Select(x => x.PriKey)
                .ToList();
            var listPayPlanLinksForAdjs = PayPlanLinks.GetForFKeysAndLinkType(listAdjNums, PayPlanLinkType.Adjustment);
            //Already attached to this dynamic payment plan.
            var listInvalidAdjNums = dynamicPaymentPlanModuleData.ListPayPlanLinks
                .FindAll(x => x.LinkType == PayPlanLinkType.Adjustment)
                .Select(x => x.FKey)
                .ToList();
            //Adjustments cannot be attached to patient payment plans so only get ones already attached to another dynamic payment plan.
            listInvalidAdjNums.AddRange(listPayPlanLinksForAdjs.Select(x => x.FKey));

            #endregion

            var listAccountEntriesUnattached = new List<AccountEntry>();
            for (var i = 0; i < listAccountEntriesAll.Count; i++)
            {
                if (listAccountEntriesAll[i].PatNum != patient.PatNum || listAccountEntriesAll[i].AmountEnd == 0 || !listTypesProd.Contains(listAccountEntriesAll[i].GetType()))
                {
                    continue;
                }

                if (listAccountEntriesAll[i].GetType() == typeof(Adjustment) && listInvalidAdjNums.Contains(listAccountEntriesAll[i].PriKey))
                {
                    continue;
                }
                else if (listAccountEntriesAll[i].GetType() == typeof(Procedure) && listInvalidProcNums.Contains(listAccountEntriesAll[i].PriKey))
                {
                    continue;
                }

                // Filter out negative, unallocated adjustments.
                if (listAccountEntriesAll[i].GetType() == typeof(Adjustment) && listAccountEntriesAll[i].ProcNum == 0 && listAccountEntriesAll[i].AmountOriginal < 0)
                {
                    continue;
                }

                //for treatment planned procedured, need to add in the estimated tax amount. If procedure is complete, tax adjustment is already being considered
                if (listAccountEntriesAll[i].GetType() == typeof(Procedure) && ((Procedure) listAccountEntriesAll[i].Tag).ProcStatus == ProcStat.TP)
                {
                    listAccountEntriesAll[i].AmountEnd += (decimal) ((Procedure) listAccountEntriesAll[i].Tag).TaxAmt;
                }

                listAccountEntriesUnattached.Add(listAccountEntriesAll[i]);
            }

            return listAccountEntriesUnattached;
        }

        public static List<PayPlanCharge> GetPayPlanChargesForDynamicPaymentPlanSchedule(PayPlan payPlan, PayPlanTerms payPlanTerms, List<PayPlanCharge> listPayPlanChargeDB, List<PayPlanLink> listPayPlanLinks, List<PaySplit> listPaySplits)
        {
            var patient = Patients.GetPat(payPlan.PatNum);
            var family = Patients.GetFamily(patient.PatNum);
            var listPayPlanChargesExpected = new List<PayPlanCharge>();
            listPayPlanChargesExpected.AddRange(listPayPlanChargeDB);
            if (payPlan.IsNew && payPlanTerms.DownPayment != 0)
            {
                //they have a down payment (possibly broken into multiple charges for differing providers) that has not yet been put into the database.
                var listPayPlanChargesForDownPayment = PayPlanCharges.GetForDownPayment(payPlanTerms, family, listPayPlanLinks, payPlan);
                listPayPlanChargesExpected.AddRange(listPayPlanChargesForDownPayment);
                listPayPlanChargesExpected.AddRange(GetListExpectedCharges(listPayPlanChargeDB, payPlanTerms, family, listPayPlanLinks, payPlan, false
                    , listPaySplits: listPaySplits, listExpectedChargesDownPayment: listPayPlanChargesForDownPayment));
            }
            else
            {
                listPayPlanChargesExpected.AddRange(GetListExpectedCharges(listPayPlanChargeDB, payPlanTerms, family, listPayPlanLinks, payPlan, false
                    , listPaySplits: listPaySplits));
            }

            return listPayPlanChargesExpected;
        }

        public static DynamicPaymentPlanModuleData GetDynamicPaymentPlanModuleData(PayPlan payPlan)
        {
            var retVal = new DynamicPaymentPlanModuleData();
            retVal.PayPlan = payPlan;
            retVal.Patient = Patients.GetPat(payPlan.PatNum);
            retVal.Family = Patients.GetFamily(retVal.Patient.PatNum);
            retVal.AmountPaid = PayPlans.GetAmtPaid(retVal.PayPlan);
            if (payPlan.PayPlanNum > 0)
            {
                retVal.ListPayPlanChargesDb = PayPlanCharges.GetForPayPlan(retVal.PayPlan.PayPlanNum).OrderBy(x => x.ChargeDate).ThenBy(x => x.PayPlanChargeNum).ToList();
                retVal.ListPayPlanLinks = PayPlanLinks.GetListForPayplan(retVal.PayPlan.PayPlanNum);
                retVal.ListPayPlanProductionEntries = PayPlanProductionEntry.GetProductionForLinks(retVal.ListPayPlanLinks);
                retVal.ListPayPlanProductionEntries.Sort(OrderDynamicPayPlanProductionEntries);
                retVal.ListPaySplits = PaySplits.GetForPayPlans([retVal.PayPlan.PayPlanNum]);
                retVal.ListPaySplitsForCharges = retVal.ListPaySplits.Where(x => retVal.ListPayPlanChargesDb.Select(y => y.PayPlanChargeNum).ToList().Contains(x.PayPlanChargeNum)).ToList();
                var payPlanTerms = GetPayPlanTerms(retVal.PayPlan, retVal.ListPayPlanLinks);
                retVal.ListPayPlanChargesExpected = GetPayPlanChargesForDynamicPaymentPlanSchedule(retVal.PayPlan, payPlanTerms, retVal.ListPayPlanChargesDb, retVal.ListPayPlanLinks, retVal.ListPaySplits);
            }

            return retVal;
        }

        public class PayPlanEntry
        {
            public long ProcNumOrd;
            public DateTime DateOrd;
            public double AmtOrd;
            public bool IsChargeOrd;
            public ProcStat ProcStatOrd;
            public string DateStr = "";
            public string PatStr = "";
            public string StatStr = "";
            public string ProcStr = "";
            public string FeeStr = "";
            public string RemBefStr = "";
            public string CredDateStr = "";
            public string AmtStr = "";
            public string RemAftStr = "";
            public string NoteStr = "";
            public string ProvAbbr = "";
            public Procedure Proc;
            public PayPlanCharge Charge;
            public long ProvNum;
        }

        private class DynamicPaymentPlanRecalculationBuckets
        {
            public double InterestSplitAmtRem;
            public double PrincipalSplitAmtRem;
            public double UnknownSplitAmtRem;
            public double InterestChargedRem;
            public double PrincipalChargedRem;
        }

        private class PayPlanChargeSplits
        {
            public PayPlanCharge PayPlanChargeCur;
            public List<PaySplit> ListPaySplits = [];
            public List<PaySplit> ListPaySplitsToTransfer = [];
            
            public List<PaySplit> ListPaySplitsAll
            {
                get
                {
                    var listPaySplitsAll = new List<PaySplit>(ListPaySplits);
                    listPaySplitsAll.AddRange(ListPaySplitsToTransfer);
                    return listPaySplitsAll;
                }
            }

            public bool IsOverpaid
            {
                get { return (ListPaySplitsAll.Where(x => x.UnearnedType == 0).Sum(x => x.SplitAmt) > PayPlanChargeCur.Principal + PayPlanChargeCur.Interest); }
            }
            public bool IsUnderpaid
            {
                get { return IsDynamicPaymentPlanInterestOrPrincipalUnderpaid(ListTools.FromSingle(PayPlanChargeCur), ListPaySplitsAll.FindAll(x => x.UnearnedType == 0)); }
            }
        }

        public class PayPlanRecalculationData
        {
            public PayPlan PayPlan = new();
            public PayPlanTerms Terms = new();
            public Family Fam = new();
            public Patient Guar;
            public long ProvNum;
            public long ClinicNum;

            public List<PayPlanCharge> ListPayPlanCharges = [];
            public List<PayPlanCharge> ListPastDebits = [];
            public List<PayPlanCharge> ListFutureDebits = [];
            public List<PayPlanCharge> ListChargesToAdd = [];
            public List<PayPlanLink> ListPayPlanLinks = [];
            public List<PayPlanProductionEntry> ListProductionEntry = [];
            public int PaymentRemainingCount;
            public List<PaySplit> ListPaySplits = [];
            public bool IsPrepay;
            public bool IsRecalculateInterest;
            public double PrincipalRemaining;
            public double InterestFutureUnpaidAmt;
            public double PeriodRate;
            public DateTime FirstFutureChargeDate;
            public double OverpaidAmt;
            public decimal PeriodPaymentAmt;
            public decimal PrincipalDecrementingAmt;
            public decimal InterestUnpaidDecrementingAmt;
            public decimal OverpaidDecrementingAmt;
            public int ChargesWithInterestCount;

            public double GetOverpaidAmount()
            {
                var pastSplitsTotal = ListPaySplits.Sum(x => x.SplitAmt);
                var pastDebitsTotal = ListPastDebits.Sum(x => x.Principal + x.Interest);
                return Math.Max(pastSplitsTotal - pastDebitsTotal, 0);
            }

            public static PayPlanRecalculationData CreateRecalculationData(PayPlanTerms terms, PayPlan payPlan, Family fam, long provNum, long clinicNum, List<PayPlanCharge> listPayPlanCharges, List<PaySplit> listPaySplits, bool isPrepay, bool isRecalculateInterest, List<PayPlanLink> listPayPlanLinks = null, List<PayPlanProductionEntry> listProductionEntry = null)
            {
                var recalcData = new PayPlanRecalculationData();
                recalcData.Terms = terms;
                recalcData.PayPlan = payPlan;
                recalcData.Guar = Patients.GetPat(payPlan.Guarantor);
                recalcData.Fam = fam;
                recalcData.ProvNum = provNum;
                recalcData.ClinicNum = clinicNum;
                recalcData.ListPayPlanCharges = listPayPlanCharges;
                recalcData.ListPastDebits = listPayPlanCharges.FindAll(x => x.ChargeType == PayPlanChargeType.Debit && x.ChargeDate <= DateTime_.Today);
                recalcData.ListFutureDebits = listPayPlanCharges.FindAll(x => x.ChargeType == PayPlanChargeType.Debit && x.ChargeDate > DateTime_.Today);
                recalcData.ListChargesToAdd = [];
                recalcData.PaymentRemainingCount = terms.PayCount - recalcData.ListPastDebits
                    .Where(x => !x.Note.Trim().ToLower().Contains("recalculated based on")
                                && (!x.Note.Trim().ToLower().Contains("downpayment") || !x.Note.Trim().ToLower().Contains("down payment")))
                    .Count();
                recalcData.ChargesWithInterestCount = recalcData.PaymentRemainingCount;
                recalcData.ListPaySplits = listPaySplits;
                recalcData.IsPrepay = isPrepay;
                recalcData.IsRecalculateInterest = isRecalculateInterest;
                recalcData.InterestFutureUnpaidAmt = recalcData.ListFutureDebits.Sum(x => x.Interest);
                recalcData.InterestUnpaidDecrementingAmt = (decimal) recalcData.InterestFutureUnpaidAmt;
                recalcData.PeriodRate = CalcPeriodRate(terms.APR, terms.Frequency);
                recalcData.FirstFutureChargeDate = CalcNextChargeDateForRecalc(terms.Frequency, recalcData);
                recalcData.PrincipalRemaining = terms.PrincipalAmount - recalcData.ListPastDebits.Sum(x => x.Principal);
                recalcData.PeriodPaymentAmt = CalcPeriodPaymentForRecalc(terms, recalcData);
                if (listPayPlanLinks != null)
                {
                    recalcData.ListPayPlanLinks = listPayPlanLinks;
                }

                if (listProductionEntry != null)
                {
                    recalcData.ListProductionEntry = listProductionEntry;
                }

                //Below properties are set in PayPlanEdit.RecalculateScheduleCharges() after matching overpayment charge is created.
                //recalcData.OverPaidAmt;
                //recalcData.PrincipalDecrementingAmt;
                //recalcData.OverPaidDecrementingAmt;
                return recalcData;
            }
        }
    }

    public class PayPlanTerms
    {
        public double PrincipalAmount;
        public double APR;
        public DateTime DateInterestStart;
        public decimal PeriodPayment;
        public int PayCount;
        public DateTime DateFirstPayment;
        public int RoundDec;
        public PayPlanFrequency Frequency;
        public bool AreTermsValid = true;
        public DateTime DateAgreement;
        public double DownPayment;
        public PaymentSchedule PaySchedule;
        public DynamicPayPlanTPOptions DynamicPayPlanTPOption;
        public string Note;
        public long SheetDefNum;
    }

    public class DynamicPaymentPlanModuleData
    {
        public Patient Patient;
        public Family Family;
        public double AmountPaid;
        public double TotalInterest;
        public PayPlan PayPlan;
        public List<PayPlanCharge> ListPayPlanChargesDb = [];
        public readonly List<PayPlanCharge> ListCloseOutPayPlanCharges = [];
        public List<PayPlanCharge> ListPayPlanChargesExpected = [];
        public List<PayPlanLink> ListPayPlanLinks = [];
        public List<PayPlanProductionEntry> ListPayPlanProductionEntries = [];
        public List<PaySplit> ListPaySplits = [];
        public List<PaySplit> ListPaySplitsForCharges = [];
    }
}