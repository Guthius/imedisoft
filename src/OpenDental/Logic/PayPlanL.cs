using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public class PayPlanL
{
    public static int ComparePayPlanRows(GridRow x, GridRow y)
    {
        var dateTimeX = DateTime.Parse(x.Cells[0].Text);
        var dateTimeY = DateTime.Parse(y.Cells[0].Text);
            
        if (dateTimeX < dateTimeY)
        {
            return -1;
        }

        if (dateTimeX > dateTimeY)
        {
            return 1;
        }

        if (x.Cells[2].Text.Trim().ToLower().Contains("recalculated based on") && !y.Cells[2].Text.Trim().ToLower().Contains("recalculated based on"))
        {
            return 1;
        }

        if (!x.Cells[2].Text.Trim().ToLower().Contains("recalculated based on") && y.Cells[2].Text.Trim().ToLower().Contains("recalculated based on"))
        {
            return -1;
        }

        if (x.Cells[2].Text.Trim().ToLower().Contains("recalculated based on") && y.Cells[2].Text.Trim().ToLower().Contains("recalculated based on"))
        {
            if (SIn.Double(x.Cells[3].Text) < SIn.Double(y.Cells[3].Text))
            {
                return 1;
            }

            return -1;
        }

        if (x.Tag.GetType() == typeof(PayPlanCharge))
        {
            if (y.Tag.GetType() == typeof(PaySplit) || y.Tag.GetType() == typeof(DataRow))
            {
                return -1;
            }

            if (string.IsNullOrEmpty(x.Cells[7].Text) && !string.IsNullOrEmpty(y.Cells[7].Text))
            {
                return -1;
            }

            if (!string.IsNullOrEmpty(x.Cells[7].Text) && string.IsNullOrEmpty(y.Cells[7].Text))
            {
                return 1;
            }
        }
        else
        {
            if (y.Tag.GetType() == typeof(PayPlanCharge))
            {
                return 1;
            }
        }

        return x.Cells[2].Text.CompareTo(y.Cells[2].Text);
    }

    public static int CompareMergedPayPlanRows(GridRow x, GridRow y)
    {
        var dateTimeX = DateTime.Parse(x.Cells[0].Text);
        var dateTimeY = DateTime.Parse(y.Cells[0].Text);
        var dynamicPayPlanRowDataX = (DynamicPayPlanRowData) x.Tag;
        var dynamicPayPlanRowDataY = (DynamicPayPlanRowData) y.Tag;
        if (dateTimeX < dateTimeY)
        {
            return -1;
        }
        else if (dateTimeX > dateTimeY)
        {
            return 1;
        }

        //Show charges before Payment on the same date.
        if (dynamicPayPlanRowDataX.IsChargeRow())
        {
            //x is charge (Type.Equals doesn't seem to work in sorters for some reason)
            if (dynamicPayPlanRowDataY.IsPaymentRow())
            {
                //y is credit, x goes first
                return -1;
            }
        }
        else
        {
            //x is credit
            if (dynamicPayPlanRowDataY.IsChargeRow())
            {
                //y is charge
                return 1;
            }
            //x and y are both Payments
        }

        return 0;
    }

    public static GridRow CreateRowForPayPlanCharge(PayPlanCharge payPlanCharge, int payPlanChargeOrdinal, bool isDynamic = false)
    {
        var descript = "#" + payPlanChargeOrdinal;
        if (isDynamic && payPlanCharge.LinkType == PayPlanLinkType.Procedure)
        {
            var procedure = Procedures.GetOneProc(payPlanCharge.FKey, false);
            if (procedure != null)
            {
                if (payPlanCharge.ChargeDate == DateTime.MaxValue && (procedure.ProcStatus == ProcStat.TP || procedure.ProcStatus == ProcStat.TPi))
                {
                    descript = "";
                }

                var procCode = ProcedureCodes.GetProcCodeFromDb(procedure.CodeNum);
                if (procCode != null)
                {
                    descript += " " + procCode.ProcCode;
                }

                if (procCode.AbbrDesc != "")
                {
                    descript += " - " + procCode.AbbrDesc;
                }
            }
        }

        if (isDynamic && payPlanCharge.LinkType == PayPlanLinkType.Adjustment)
        {
            descript += " - " + Lan.g("Payment Plan", "Adjustment");
        }

        if (payPlanCharge.Note != "")
        {
            descript += " " + payPlanCharge.Note;
            //Don't add a # if it's a recalculated charge because they aren't "true" payplan charges.
            if (payPlanCharge.Note.Trim().ToLower().Contains("recalculated based on"))
            {
                descript = payPlanCharge.Note;
            }
        }

        var row = new GridRow(); //Charge row
        row.Cells.Add(payPlanCharge.ChargeDate.ToShortDateString()); //0 Date
        row.Cells.Add(Providers.GetAbbr(payPlanCharge.ProvNum)); //1 Prov Abbr
        row.Cells.Add(descript); //2 Descript
        if (payPlanCharge.Principal < 0 && payPlanCharge.IsOffset)
        {
            //Offsetting Debits
            row.Cells.Add(payPlanCharge.Principal.ToString("n")); //principal
            row.Cells.Add(""); //interest
            row.Cells.Add(""); //due
            row.Cells.Add(""); //payment
        }
        else if (payPlanCharge.Principal < 0)
        {
            //adjustment
            row.Cells.Add(""); //principal
            row.Cells.Add(""); //interest
            row.Cells.Add(""); //due
            row.Cells.Add(""); //payment
            if (!isDynamic)
            {
                row.ColorText = Defs.GetDefByExactName(DefCat.AccountColors, "Adjustment").ItemColor;
                row.Cells.Add(payPlanCharge.Principal.ToString("n")); //adjustment
            }
        }
        else
        {
            //regular charge
            row.Cells.Add(payPlanCharge.Principal.ToString("n")); //3 Principal
            row.Cells.Add(payPlanCharge.Interest.ToString("n")); //4 Interest
            row.Cells.Add((payPlanCharge.Principal + payPlanCharge.Interest).ToString("n")); //5 Due
            row.Cells.Add(""); //6 Payment
            if (!isDynamic)
            {
                //Dynamic payment plans do not have pay plan adjustments.
                row.Cells.Add(""); //7 Adjustment
            }
        }

        row.Cells.Add(""); //8 Balance (filled later)
        if (isDynamic && payPlanCharge.PayPlanChargeNum == 0)
        {
            row.ColorText = Color.Gray; //it isn't an actual charge yet, it hasn't come due and been inserted into the database. 
        }

        row.Tag = payPlanCharge;
        return row;
    }

    public static GridRow CreateRowForPatientPayPlanSplit(DataRow dataRowBundlePayment, PaySplit paySplit)
    {
        var descript = Defs.GetName(DefCat.PaymentTypes, SIn.Long(dataRowBundlePayment["PayType"].ToString()));
        if (dataRowBundlePayment["CheckNum"].ToString() != "")
        {
            descript += " #" + dataRowBundlePayment["CheckNum"];
        }

        descript += " " + paySplit.SplitAmt.ToString("c");
        if (SIn.Double(dataRowBundlePayment["PayAmt"].ToString()) != paySplit.SplitAmt)
        {
            descript += Lans.g("PayPlanL", "(split)");
        }

        var row = new GridRow();
        row.Cells.Add(paySplit.DatePay.ToShortDateString()); //0 Date
        row.Cells.Add(Providers.GetAbbr(SIn.Long(dataRowBundlePayment["ProvNum"].ToString()))); //1 Prov Abbr
        row.Cells.Add(descript); //2 Descript
        row.Cells.Add(""); //3 Principal
        row.Cells.Add(""); //4 Interest
        row.Cells.Add(""); //5 Due
        row.Cells.Add(paySplit.SplitAmt.ToString("n")); //6 Payment
        row.Cells.Add(""); //7 Adjustment - Does not exist for dynamic payment plans
        row.Cells.Add(""); //8 Balance (filled later)
        row.Tag = paySplit;
        row.ColorText = Defs.GetDefByExactName(DefCat.AccountColors, "Payment").ItemColor;
        return row;
    }

    public static GridRow CreateRowForClaimProcs(DataRow dataRowBundleClaimProc, bool isDynamic = false)
    {
        //Either a claimpayment or a bundle of claimprocs with no claimpayment that were on the same date.
        var descript = Defs.GetName(DefCat.InsurancePaymentType, SIn.Long(dataRowBundleClaimProc["PayType"].ToString()));
        if (dataRowBundleClaimProc["CheckNum"].ToString() != "")
        {
            descript += " #" + dataRowBundleClaimProc["CheckNum"];
        }

        if (SIn.Long(dataRowBundleClaimProc["ClaimPaymentNum"].ToString()) == 0)
        {
            descript += "No Finalized Payment";
        }
        else
        {
            var checkAmt = SIn.Double(dataRowBundleClaimProc["CheckAmt"].ToString());
            descript += " " + checkAmt.ToString("c");
            var insPayAmt = SIn.Double(dataRowBundleClaimProc["InsPayAmt"].ToString());
            if (checkAmt != insPayAmt)
            {
                descript += " " + Lans.g("PayPlanL", "(split)");
            }
        }

        var row = new GridRow();
        row.Cells.Add(SIn.DateTime(dataRowBundleClaimProc["DateCP"].ToString()).ToShortDateString()); //0 Date
        row.Cells.Add(Providers.GetLName(SIn.Long(dataRowBundleClaimProc["ProvNum"].ToString()))); //1 Prov Abbr
        row.Cells.Add(descript); //2 Descript
        row.Cells.Add(""); //3 Principal
        row.Cells.Add(""); //4 Interest
        row.Cells.Add(""); //5 Due
        row.Cells.Add(SIn.Double(dataRowBundleClaimProc["InsPayAmt"].ToString()).ToString("n")); //6 Payment
        if (!isDynamic)
        {
            row.Cells.Add(""); //7 Adjustment
        }

        row.Cells.Add(""); //8 Balance (filled later)
        row.Tag = dataRowBundleClaimProc;
        row.ColorText = Defs.GetDefByExactName(DefCat.AccountColors, "Insurance Payment").ItemColor;
        return row;
    }

    public static void MakeSecLogEntries(PayPlan payPlan, PayPlan payPlanOld, bool hasSignatureChanged, bool isSigOldValid, bool isSigBlank, bool isSigValid, bool isPrinting = false)
    {
        //logs creating, closing out, deleting, and signing of payment plan.
        //deleted logs are in butDelete_click since that method doesn't call SaveData.
        if (isPrinting)
        {
            // Don't make log entry if the print button was clicked.
            return;
        }

        var planType = "Patient ";
        if (payPlan.IsDynamic)
        {
            planType = "";
        }

        //new
        if (payPlanOld.IsNew)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan created.", payPlanOld.PayPlanNum, DateTime.MinValue);
            return;
        }

        //closed
        if (!payPlan.IsClosed && payPlanOld.IsClosed)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan reopened.", payPlanOld.PayPlanNum, DateTime.MinValue);
        }

        if (hasSignatureChanged)
        {
            //signed
            if (!isSigOldValid && !isSigBlank && isSigValid)
            {
                SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                    (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan signed.", payPlanOld.PayPlanNum, DateTime.MinValue);
            }

            //sig invalidated
            if (isSigOldValid && (!isSigValid || isSigBlank))
            {
                SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                    (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan signature invalidated.", payPlanOld.PayPlanNum, DateTime.MinValue);
            }
        }

        //guarantor changed
        if (payPlanOld.Guarantor != payPlan.Guarantor)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan guarantor changed from "
                                                                 + Patients.GetNameLF(payPlanOld.Guarantor) + " to " + Patients.GetNameLF(payPlan.Guarantor) + ".", payPlanOld.PayPlanNum, DateTime.MinValue);
        }

        //Completed Amt Changed
        if (payPlanOld.CompletedAmt != payPlan.CompletedAmt)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan completed amount changed.", payPlanOld.PayPlanNum, DateTime.MinValue);
        }

        //Ins Plan Changed
        if (payPlanOld.PlanNum != payPlan.PlanNum)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan ins plan changed.", payPlanOld.PayPlanNum, DateTime.MinValue);
        }

        //Note Changed
        if (payPlanOld.Note != payPlan.Note)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan note changed.", payPlanOld.PayPlanNum, DateTime.MinValue);
        }

        //closed
        if (payPlan.IsClosed && !payPlanOld.IsClosed)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.PayPlanEdit, payPlan.PatNum,
                (payPlan.PlanNum == 0 ? planType : "Insurance ") + "Payment Plan closed.", payPlanOld.PayPlanNum, DateTime.MinValue);
        }
    }
        
    public static List<GridRow> CreateRowsForDynamicPayPlanCharges(List<PayPlanCharge> listPayPlanCharges, DataTable tableBundledPayments, bool ungrouped)
    {
        var listGridRowsMerged = new List<GridRow>();
        var listPaySplits = PaySplits.GetFromBundled(tableBundledPayments);
        if (ungrouped)
        {
            var numCharges = 0;
            var datePrevCharge = DateTime.MinValue;
            var listProcNums = listPayPlanCharges.Where(x => x.LinkType == PayPlanLinkType.Procedure).Select(x => x.FKey).ToList();
            var listProcedures = Procedures.GetManyProc(listProcNums, false);
            var listCodeNums = listProcedures.Select(x => x.CodeNum).ToList();
            var listProcedureCodes = ProcedureCodes.GetCodesForCodeNums(listCodeNums);
            for (var i = 0; i < listPayPlanCharges.Count; i++)
            {
                if (listPayPlanCharges[i].ChargeDate != datePrevCharge)
                {
                    numCharges++;
                }

                var descript = "#" + numCharges;
                if (listPayPlanCharges[i].LinkType == PayPlanLinkType.Procedure)
                {
                    var procedure = listProcedures.FirstOrDefault(x => x.ProcNum == listPayPlanCharges[i].FKey);
                    if (procedure != null)
                    {
                        if (listPayPlanCharges[i].ChargeDate == DateTime.MaxValue && (procedure.ProcStatus == ProcStat.TP || procedure.ProcStatus == ProcStat.TPi))
                        {
                            descript = "";
                        }

                        var procedureCode = listProcedureCodes.FirstOrDefault(x => x.CodeNum == procedure.CodeNum);
                        if (procedureCode != null)
                        {
                            descript += " " + procedureCode.ProcCode;
                        }

                        if (procedureCode.AbbrDesc != "")
                        {
                            descript += " - " + procedureCode.AbbrDesc;
                        }
                    }
                }

                //I think this if statement can be removed.
                if (listPayPlanCharges[i].LinkType == PayPlanLinkType.Adjustment)
                {
                    descript += " - " + Lan.g("Payment Plan", "Adjustment");
                }

                if (listPayPlanCharges[i].Note != "")
                {
                    descript += " " + listPayPlanCharges[i].Note;
                }

                var row = new GridRow();
                row.Cells.Add(listPayPlanCharges[i].ChargeDate.ToShortDateString());
                row.Cells.Add(descript);
                row.Cells.Add(listPayPlanCharges[i].Principal.ToString("n"));
                row.Cells.Add(listPayPlanCharges[i].Interest.ToString("n"));
                row.Cells.Add((listPayPlanCharges[i].Principal + listPayPlanCharges[i].Interest).ToString("n"));
                row.Cells.Add("");
                row.Cells.Add("");
                if (listPayPlanCharges[i].PayPlanChargeNum == 0)
                {
                    row.ColorText = Color.Gray; //it isn't an actual charge yet, it hasn't come due and been inserted into the database. 
                }

                row.Tag = new DynamicPayPlanRowData
                {
                    ListPayPlanCharges = [listPayPlanCharges[i]],
                    IsDownPayment = descript.Contains("Down Payment")
                };
                listGridRowsMerged.Add(row);
                datePrevCharge = listPayPlanCharges[i].ChargeDate;
            }

            for (var i = 0; i < listPaySplits.Count; i++)
            {
                var descript = Defs.GetName(DefCat.PaymentTypes, SIn.Long(tableBundledPayments.Rows[i]["PayType"].ToString()));
                if (tableBundledPayments.Rows[i]["CheckNum"].ToString() != "")
                {
                    descript += " #" + tableBundledPayments.Rows[i]["CheckNum"];
                }

                descript += " " + listPaySplits[i].SplitAmt.ToString("c");
                if (SIn.Double(tableBundledPayments.Rows[i]["PayAmt"].ToString()) != listPaySplits[i].SplitAmt)
                {
                    descript += Lans.g("PayPlanL", "(split)");
                }

                var row = new GridRow();
                row.Cells.Add(listPaySplits[i].DatePay.ToShortDateString()); //1 Date
                row.Cells.Add(descript); //2 Description
                row.Cells.Add(""); //3 Principal
                row.Cells.Add(""); //4 Interest
                row.Cells.Add(""); //5 Due
                row.Cells.Add(listPaySplits[i].SplitAmt.ToString("n")); //6 Payment
                row.Cells.Add(""); //7 Balance (filled later)
                row.ColorText = Defs.GetDefByExactName(DefCat.AccountColors, "Payment").ItemColor;
                row.Tag = new DynamicPayPlanRowData
                {
                    PayNum = listPaySplits[i].PayNum
                };
                listGridRowsMerged.Add(row);
            }

            listGridRowsMerged.Sort(CompareMergedPayPlanRows);
            return listGridRowsMerged;
        }

        //Charge rows grouped by ChargeDate.
        var listDateTimesCharges = listPayPlanCharges.Select(x => x.ChargeDate).OrderBy(x => x).Distinct().ToList();
        for (var i = 0; i < listDateTimesCharges.Count; i++)
        {
            var listPayPlanChargesForDate = listPayPlanCharges.FindAll(x => x.ChargeDate == listDateTimesCharges[i]);
            var principal = listPayPlanChargesForDate.Sum(x => x.Principal);
            var interest = listPayPlanChargesForDate.Sum(x => x.Interest);
            var due = principal + interest;
            var descript = "#" + (i + 1);
            if (listPayPlanChargesForDate.Any(x => x.Note == "Down Payment"))
            {
                descript += " Down Payment";
            }

            var row = new GridRow();
            row.Cells.Add(listDateTimesCharges[i].ToShortDateString());
            row.Cells.Add(descript);
            row.Cells.Add(principal.ToString("n"));
            row.Cells.Add(interest.ToString("n"));
            row.Cells.Add(due.ToString("n"));
            row.Cells.Add("");
            row.Cells.Add(""); //6 Balance (filled later)
            if (listPayPlanChargesForDate.Any(x => x.PayPlanChargeNum == 0))
            {
                row.ColorText = Color.Gray; //it isn't an actual charge yet, it hasn't come due and been inserted into the database 
            }

            row.Tag = new DynamicPayPlanRowData
            {
                ListPayPlanCharges = listPayPlanChargesForDate,
                IsDownPayment = descript.Contains("Down Payment")
            };
            listGridRowsMerged.Add(row);
        }

        var listPayNums = listPaySplits.Select(x => x.PayNum).Distinct().ToList();
        for (var i = 0; i < listPayNums.Count; i++)
        {
            var listPaySplitsForPayment = listPaySplits.FindAll(x => x.PayNum == listPayNums[i]);
            var datePay = listPaySplitsForPayment[0].DatePay.ToShortDateString();
            var sumSplitAmt = listPaySplitsForPayment.Sum(x => x.SplitAmt);
            var row = new GridRow();
            row.Cells.Add(datePay); //0 Date
            row.Cells.Add("Payment"); //1 Description
            row.Cells.Add(""); //2 Principal
            row.Cells.Add(""); //3 Interest
            row.Cells.Add(""); //4 Due
            row.Cells.Add(sumSplitAmt.ToString("n")); //5 Payment
            row.Cells.Add(""); //6 Balance (filled later)
            row.ColorText = Defs.GetDefByExactName(DefCat.AccountColors, "Payment").ItemColor;
            row.Tag = new DynamicPayPlanRowData
            {
                PayNum = listPayNums[i]
            };
            listGridRowsMerged.Add(row);
        }

        listGridRowsMerged.Sort(CompareMergedPayPlanRows);
        return listGridRowsMerged;
    }
        
    public static List<GridRow> CreateGridRowsForProductionTab(DynamicPaymentPlanModuleData dynamicPaymentPlanModuleData, bool showAttachedProductionAndIncome)
    {
        var listPayPlanCharges = ListTools.DeepCopy<PayPlanCharge, PayPlanCharge>(dynamicPaymentPlanModuleData.ListPayPlanChargesExpected).OrderBy(x => x.ChargeDate).ToList();
        var listDescriptions = new List<string>();
        var numCharges = 0;
        var datePrevCharge = DateTime.MinValue;
            
        for (var i = 0; i < listPayPlanCharges.Count; i++)
        {
            if (listPayPlanCharges[i].ChargeDate != datePrevCharge)
            {
                numCharges++;
            }

            var descript = "#" + numCharges + ": Charge";
            if (listPayPlanCharges[i].Note == "Down Payment")
            {
                descript += " Down Payment";
            }

            listDescriptions.Add(descript);
            datePrevCharge = listPayPlanCharges[i].ChargeDate;
        }

        listPayPlanCharges.Reverse(); //Reversing the order so that we can remove elements from the list once they are found down below. 
        listDescriptions.Reverse();
        var listGridRows = new List<GridRow>();
        var tableBundledPayments = PaySplits.GetForPayPlan(dynamicPaymentPlanModuleData.PayPlan.PayPlanNum);
        var listPaySplits = PaySplits.GetFromBundled(tableBundledPayments);
        listPaySplits.Reverse(); //Same reason why we are reversing listPayPlanCharges.
        for (var i = 0; i < dynamicPaymentPlanModuleData.ListPayPlanProductionEntries.Count; i++)
        {
            var row = new GridRow();
            //If the pref to use Date Production as Date Showing is true, this column would be duplicate data
            if (!PrefC.GetBool(PrefName.PayPlanItemDateShowProc))
            {
                var dateShowing = dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].CreditDate;
                if (dateShowing == DateTime.MinValue)
                {
                    //credit was just added 
                    dateShowing = DateTime.Today; //Date Showing
                }

                row.Cells.Add(dateShowing.ToShortDateString()); //Date Showing
            }

            row.Cells.Add(dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].ProductionDate.ToShortDateString()); //Date Production
            row.Cells.Add(Providers.GetAbbr(dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].ProvNum)); //Provider
            if (true)
            {
                row.Cells.Add(Clinics.GetAbbr(dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].ClinicNum)); //Clinic
            }

            row.Cells.Add(dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].Description); //Description
            row.Cells.Add(dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].AmountOriginal.ToString("f")); //Amount
            if (dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].AmountOverride == 0)
            {
                row.Cells.Add(""); //if no override was entered cell should be blank. Override
            }
            else
            {
                row.Cells.Add(dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].AmountOverride.ToString("f")); //Override
            }

            row.Tag = dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i];
            listGridRows.Add(row);
            if (!showAttachedProductionAndIncome)
            {
                continue;
            }

            for (var j = listPayPlanCharges.Count - 1; j >= 0; j--)
            {
                //This is why we are reversing the order of listPayPlanCharges. 
                if (listPayPlanCharges[j].LinkType != dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].LinkType ||
                    listPayPlanCharges[j].FKey != dynamicPaymentPlanModuleData.ListPayPlanProductionEntries[i].PriKey)
                {
                    continue;
                }

                row = new GridRow();
                if (!PrefC.GetBool(PrefName.PayPlanItemDateShowProc))
                {
                    row.Cells.Add(""); //Date Showing
                }

                row.Cells.Add(listPayPlanCharges[j].ChargeDate.ToShortDateString()); //'Date' if PayPlanItemDateShowProc is true, otherwise 'Date Production'
                row.Cells.Add(""); //Provider
                if (true)
                {
                    row.Cells.Add(""); //Clinic
                }

                row.Cells.Add(listDescriptions[j]); //Description
                row.Cells.Add((listPayPlanCharges[j].Principal + listPayPlanCharges[j].Interest).ToString("n")); //Amount
                row.Cells.Add(""); //Amount Override
                row.ColorText = Defs.GetDefByExactName(DefCat.AccountColors, "PayPlan").ItemColor; //There isn't an ItemName for "Charge". Using "PayPlan" instead.
                row.Tag = new DynamicPayPlanRowData
                {
                    ListPayPlanCharges = listPayPlanCharges
                };
                listGridRows.Add(row);
                for (var k = listPaySplits.Count - 1; k >= 0; k--)
                {
                    //This is why we are reversing the order of listPaySplits.
                    if (listPayPlanCharges[j].PayPlanChargeNum == 0)
                    {
                        break; //Future payment plan charges will never have payments associated with them.
                    }

                    if (listPaySplits[k].PayPlanChargeNum != listPayPlanCharges[j].PayPlanChargeNum)
                    {
                        continue;
                    }

                    var descript = "Payment";
                    if (listPaySplits[k].PayPlanDebitType != PayPlanDebitTypes.Unknown)
                    {
                        //Not a legacy split or a current prepayment.
                        descript += " (" + listPaySplits[k].PayPlanDebitType + ")";
                    }

                    row = new GridRow();
                    if (!PrefC.GetBool(PrefName.PayPlanItemDateShowProc))
                    {
                        row.Cells.Add(""); //Date Showing
                    }

                    row.Cells.Add(listPaySplits[k].DatePay.ToShortDateString()); //'Date' if PayPlanItemDateShowProc is true, otherwise 'Date Production'
                    row.Cells.Add(""); //Provider
                    if (true)
                    {
                        row.Cells.Add(""); //Clinic
                    }

                    row.Cells.Add(descript); //Description
                    row.Cells.Add(listPaySplits[k].SplitAmt.ToString("n")); //Amount
                    row.Cells.Add(""); //Amount Override
                    row.ColorText = Defs.GetDefByExactName(DefCat.AccountColors, "Payment").ItemColor;
                    row.Tag = new DynamicPayPlanRowData
                    {
                        PayNum = listPaySplits[k].PayNum
                    };
                    listGridRows.Add(row);
                    listPaySplits.RemoveAt(k);
                }

                listPayPlanCharges.RemoveAt(j);
                listDescriptions.RemoveAt(j);
            }
        }

        return listGridRows;
    }
}

public class DynamicPayPlanRowData
{
    public List<long> ListPayPlanChargeNums
    {
        get => ListPayPlanCharges.Select(x => x.PayPlanChargeNum).ToList();
    }

    public long PayNum;
    public bool IsDownPayment;
    public List<PayPlanCharge> ListPayPlanCharges = [];

    public bool IsChargeRow()
    {
        return ListPayPlanCharges.Count > 0;
    }

    public bool IsPaymentRow()
    {
        return PayNum != 0;
    }
}