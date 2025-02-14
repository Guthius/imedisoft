using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.WebTypes.Shared.XWeb;

namespace OpenDentBusiness;

public class Payments
{
    public static Payment GetPayment(long payNum)
    {
        var command =
            "SELECT * from payment"
            + " WHERE PayNum = '" + payNum + "'";
        return PaymentCrud.SelectOne(command);
    }

    public static List<Payment> GetPayments(List<long> listPayNums)
    {
        if (listPayNums.IsNullOrEmpty()) return [];

        var command = $"SELECT * FROM payment WHERE PayNum IN({string.Join(",", listPayNums.Select(x => x))})";
        return PaymentCrud.SelectMany(command);
    }

    public static List<Payment> GetTransfers(List<long> listPatNums)
    {
        var command = "SELECT * FROM payment WHERE PayType=0";
        if (!listPatNums.IsNullOrEmpty()) command += $" AND PatNum IN({string.Join(",", listPatNums.Select(x => x))})";
        return PaymentCrud.SelectMany(command);
    }
    
    public static List<long> GetPayNumsForTransfers(bool isPayTypeIgnored, params long[] arrayPatNums)
    {
        var command = "SELECT payment.PayNum FROM payment ";
        if (isPayTypeIgnored)
            //Ignore the payment.PayType value and instead join up with the paysplit table to consider their values to determine which payments are 'income transfers'.
            command += "INNER JOIN paysplit ON payment.PayNum=paysplit.PayNum ";
        else
            //Only consider payments that have the "None (Income Transfer)" payment type check box checked as income transfers.
            command += "WHERE payment.PayType=0 ";
        //Conditionally filter the list of payments by the array of PatNums passed in.
        if (!arrayPatNums.IsNullOrEmpty()) command += $"AND payment.PatNum IN({string.Join(",", arrayPatNums.Select(x => x))}) ";
        if (isPayTypeIgnored)
            //Treat all payments that have payment splits that sum up to $0 as a income transfers.
            command += "GROUP BY payment.PayNum "
                       + "HAVING SUM(paysplit.SplitAmt) = 0 ";
        return Db.GetListLong(command);
    }

    public static List<Payment> GetForDeposit(long depositNum)
    {
        var command =
            "SELECT * FROM payment "
            + "WHERE DepositNum = " + depositNum + " "
            //Order by the date on the payment, and then the incremental order of the creation of each payment (doesn't affect random primary keys).
            //It was an internal complaint that checks on the same date show up in a 'random' order.
            //The real fix for this issue would be to add a time column and order by it by that instead of the PK.
            + "ORDER BY PayDate,PayNum"; //Not usual pattern to order by PK
        return PaymentCrud.SelectMany(command);
    }

    public static List<Payment> GetForDeposit(DateTime dateStart, long clinicNum, List<long> payTypes)
    {
        var command =
            "SELECT * FROM payment "
            + "WHERE DepositNum = 0 "
            + "AND PayDate >= " + SOut.Date(dateStart) + " ";
        if (clinicNum != 0) command += "AND ClinicNum=" + clinicNum;
        for (var i = 0; i < payTypes.Count; i++)
        {
            if (i == 0)
                command += " AND (";
            else
                command += " OR ";
            command += "PayType=" + payTypes[i];
            if (i == payTypes.Count - 1) command += ")";
        }

        //Order by the date on the payment, and then the incremental order of the creation of each payment (doesn't affect random primary keys).
        //It was an internal complaint that checks on the same date show up in a 'random' order.
        //The real fix for this issue would be to add a time column and order by it by that instead of the PK.
        command += " ORDER BY PayDate,PayNum"; //Not usual pattern to order by PK
        object[] parameters = [command, payTypes];
        command = (string) parameters[0];
        return PaymentCrud.SelectMany(command);
    }

    public static List<Payment> GetPaymentsUsingFilters(List<long> clinicNums, DateTime startDate, DateTime endDate, List<ProcessStat> listProcessStatus, List<CreditCardSource> listCreditCardSources)
    {
        var command = $@"SELECT * FROM payment WHERE PayDate BETWEEN {SOut.Date(startDate)} AND {SOut.Date(endDate)}";
        if (listProcessStatus.Count > 0) command += $" AND ProcessStatus IN ({string.Join(",", listProcessStatus.Select(x => SOut.Int((int) x)))})";
        if (listCreditCardSources.Count > 0) command += $" AND PaymentSource IN ({string.Join(",", listCreditCardSources.Select(x => SOut.Int((int) x)))})";
        if (clinicNums.Count > 0) command += $" AND payment.ClinicNum IN ({string.Join(",", clinicNums)})";
        return PaymentCrud.SelectMany(command);
    }

    public static Payment GetFromList(long payNum, List<Payment> List)
    {
        for (var i = 0; i < List.Count; i++)
            if (List[i].PayNum == payNum)
                return List[i];

        return null; //should never happen
    }

    public static int GetCountAttachedToDeposit(List<long> listPayNums, long ignoreDepositNum)
    {
        if (listPayNums.Count == 0) return 0;
        var command = "";
        command = "SELECT COUNT(*) FROM payment WHERE PayNum IN(" + string.Join(",", listPayNums) + ") AND DepositNum!=0";
        if (ignoreDepositNum != 0) command += " AND DepositNum!=" + ignoreDepositNum;
        return SIn.Int(Db.GetCount(command));
    }

    public static long Insert(Payment pay)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        pay.SecUserNumEntry = Security.CurUser.UserNum;
        return PaymentCrud.Insert(pay);
    }

    public static long Insert(Payment pay, List<PaySplit> listPaySplits)
    {
        if (listPaySplits.IsNullOrEmpty()) return 0; //Never insert a payment without any payment splits.

        Insert(pay); //The CRUD will set pay.PayNum accordingly.
        PaySplits.InsertMany(pay.PayNum, listPaySplits);
        return pay.PayNum;
    }

    public static long InsertFromXWeb(long patNum, long clinicNum, double amount, string payNote, string receipt, CreditCardSource ccSource, string logGuid = "")
    {
        WebPaymentProperties xwebProperties;
        ProgramProperties.GetXWebCreds(clinicNum, out xwebProperties);
        var payment = new Payment
        {
            ClinicNum = clinicNum,
            IsRecurringCC = false,
            IsSplit = false,
            PatNum = patNum,
            PayAmt = amount,
            PayDate = DateTime.Now,
            PaymentSource = ccSource,
            PayType = xwebProperties.PaymentTypeDefNum,
            ProcessStatus = ProcessStat.OnlinePending,
            Receipt = receipt,
            PayNote = payNote
        };
        if (PrefC.GetBool(PrefName.OnlinePaymentsMarkAsProcessed)) payment.ProcessStatus = ProcessStat.OnlineProcessed;
        var patient = Patients.GetPat(patNum);
        var retVal = ProcessPaymentForWeb(payment, patient, amount);
        var ccSourceString = "XWeb";
        if (ccSource.In(CreditCardSource.EdgeExpressPaymentPortal, CreditCardSource.EdgeExpressPaymentPortalGuest, CreditCardSource.EdgeExpressCNP, CreditCardSource.EdgeExpressRCM)) ccSourceString = "EdgeExpress";
        var logSource = LogSources.None;
        if (CreditCards.GetCreditCardSourcesForOnlinePayments().Contains(ccSource)) logSource = LogSources.PaymentPortal;
        SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, patNum, ccSourceString + " " + Lans.g("Payments.InsertFromXWeb", "payment by") + " " + Patients.GetLim(patNum).GetNameLF() + ", " + amount.ToString("c"), logSource);
        return retVal;
    }

    public static Payment InsertReturnXWebPayment(Payment payment, string payNote, double payAmt, ProcessStat processStat = ProcessStat.OfficeProcessed)
    {
        var paymentReturn = payment.Clone();
        var isPartial = false;
        if (payAmt != 0)
        {
            isPartial = !CompareDouble.IsEqual(Math.Abs(paymentReturn.PayAmt), Math.Abs(payAmt));
            paymentReturn.PayAmt = payAmt;
        }

        if (paymentReturn.PayAmt > 0) //User passed us original amount, negate. Voids are always negative.
            paymentReturn.PayAmt *= -1; //The negated amount of the original payment
        paymentReturn.Receipt = ""; //no receipt returned
        paymentReturn.PayNote = payNote;
        if (paymentReturn.PayNote != "") paymentReturn.PayNote += "\r\n";
        paymentReturn.PaymentSource = CreditCardSource.XWeb;
        paymentReturn.ProcessStatus = processStat;
        paymentReturn.IsCcCompleted = true;
        paymentReturn.PayDate = DateTime.Now;
        paymentReturn.PayNum = Insert(paymentReturn);
        var listPaySplits = PaySplits.GetForPayment(payment.PayNum);
        var listClonedPaySplits = new List<PaySplit>();
        var payAmtAllocateRemaining = Math.Abs(payAmt);
        foreach (var paySplit in listPaySplits)
        {
            var paySplitCopy = paySplit.Copy();
            paySplitCopy.SplitAmt *= -1;
            if (isPartial)
            {
                if (CompareDouble.IsLessThanOrEqualToZero(payAmtAllocateRemaining)) break;
                var amtToAllocate = Math.Min(Math.Abs(paySplitCopy.SplitAmt), Math.Abs(payAmtAllocateRemaining));
                paySplitCopy.SplitAmt = amtToAllocate * -1;
                payAmtAllocateRemaining -= amtToAllocate;
            }

            paySplitCopy.PayNum = paymentReturn.PayNum;
            paySplitCopy.DatePay = paymentReturn.PayDate;
            listClonedPaySplits.Add(paySplitCopy);
        }

        PaySplits.InsertMany(listClonedPaySplits);
        return paymentReturn;
    }

    public static Payment InsertVoidPayment(Payment payment, List<PaySplit> listPaySplits, string receipt, string payNote, CreditCardSource creditCardSource, ProcessStat processStat = ProcessStat.OfficeProcessed, double payAmt = 0)
    {
        var paymentVoid = payment.Clone();
        if (payAmt != 0) paymentVoid.PayAmt = payAmt;
        if (paymentVoid.PayAmt > 0) //User passed us original amount, negate. Voids are always negative.
            paymentVoid.PayAmt *= -1; //The negated amount of the original payment
        paymentVoid.Receipt = receipt;
        paymentVoid.PayNote = payNote;
        if (paymentVoid.PayNote != "") paymentVoid.PayNote += "\r\n";
        paymentVoid.PaymentSource = creditCardSource;
        paymentVoid.ProcessStatus = processStat;
        paymentVoid.IsCcCompleted = true;
        paymentVoid.PayNum = Insert(paymentVoid);
        var listClonedPaySplits = new List<PaySplit>();
        foreach (var paySplit in listPaySplits)
        {
            var paySplitCopy = paySplit.Copy();
            paySplitCopy.SplitAmt *= -1;
            paySplitCopy.PayNum = paymentVoid.PayNum;
            paySplitCopy.DatePay = paymentVoid.PayDate;
            listClonedPaySplits.Add(paySplitCopy);
        }

        PaySplits.InsertMany(listClonedPaySplits);
        return paymentVoid;
    }

    public static Payment MakeNegativePaymentsRefund(Payment paymentExisting)
    {
        #region Make Payment

        var listPaySplitsExisting = PaySplits.GetForPayment(paymentExisting.PayNum);
        var paymentRefund = new Payment();
        listPaySplitsExisting = listPaySplitsExisting.FindAll(x => x.PayNum == paymentExisting.PayNum);
        //Create the refund payment with negative amount
        //Give the paytime of the original, the user will be able to select a different type on the payment window if they like
        paymentRefund.PayType = paymentExisting.PayType;
        paymentRefund.PayDate = DateTime.Today;
        paymentRefund.PayAmt = -paymentExisting.PayAmt;
        paymentRefund.IsSplit = paymentExisting.IsSplit;
        paymentRefund.PatNum = paymentExisting.PatNum;
        paymentRefund.ClinicNum = paymentExisting.ClinicNum;
        paymentRefund.DateEntry = DateTime.Today;

        #endregion

        #region Make Pay Splits

        //Create a negative payment split for each paysplit attached to the existing payment.
        var listPaySplitsRefund = new List<PaySplit>();
        for (var i = 0; i < listPaySplitsExisting.Count; i++)
        {
            var paySplit = listPaySplitsExisting[i].Copy();
            paySplit.SplitNum = 0;
            paySplit.PayNum = paymentRefund.PayNum;
            paySplit.DatePay = paymentRefund.PayDate;
            paySplit.DateEntry = paymentRefund.DateEntry;
            paySplit.SplitAmt = -listPaySplitsExisting[i].SplitAmt;
            listPaySplitsRefund.Add(paySplit);
        }

        #endregion

        Insert(paymentRefund, listPaySplitsRefund);
        return paymentRefund;
    }
    
    public static void Update(Payment pay, bool excludeDepositNum)
    {
        if (!PrefC.GetBool(PrefName.AccountAllowFutureDebits) && !PrefC.GetBool(PrefName.FutureTransDatesAllowed) && pay.PayDate.Date > DateTime.Today.Date) throw new ApplicationException(Lans.g("Payments", "Payment Date must not be a future date."));
        if (pay.PayDate.Year < 1880) throw new ApplicationException(Lans.g("Payments", "Invalid Payment Date"));
        //the functionality below needs to be taken care of before calling the function:
        /*string command="SELECT DepositNum,PayAmt FROM payment "
                +"WHERE PayNum="+POut.PInt(PayNum);
        DataConnection dcon=new DataConnection();
        DataTable table=DataCore.GetTable(command);
        if(table.Rows.Count==0) {
            return;
        }
        if(table.Rows[0][0].ToString()!="0"//if payment is already attached to a deposit
                && PIn.PDouble(table.Rows[0][1].ToString())!=PayAmt) {//and PayAmt changes
            throw new ApplicationException(Lans.g("Payments","Not allowed to change the amount on payments attached to deposits."));
        }*/
        PaymentCrud.Update(pay);
        if (!excludeDepositNum)
        {
            var command = "UPDATE payment SET DepositNum=" + pay.DepositNum + " WHERE PayNum = " + pay.PayNum;
            Db.NonQ(command);
        }
    }

    public static void Update(Payment payNew, Payment payOld)
    {
        PaymentCrud.Update(payNew, payOld);
    }
    
    public static void Delete(Payment pay)
    {
        Delete(pay.PayNum);
    }
    
    public static void Delete(long payNum)
    {
        var command = "SELECT DepositNum,PayAmt FROM payment WHERE PayNum=" + payNum;
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return;
        if (table.Rows[0]["DepositNum"].ToString() != "0" //if payment is already attached to a deposit
            && SIn.Double(table.Rows[0]["PayAmt"].ToString()) != 0) //and it's not new
            throw new ApplicationException(Lans.g("Payments", "Not allowed to delete a payment attached to a deposit."));
        command = "DELETE from payment WHERE PayNum = " + payNum;
        Db.NonQ(command);
        //this needs to be improved to handle EstBal
        command = "DELETE from paysplit WHERE PayNum = " + payNum;
        Db.NonQ(command);
        command = "UPDATE recurringcharge SET PayNum=0 WHERE PayNum=" + payNum;
        Db.NonQ(command);
    }
    
    public static bool AllocationRequired(double payAmt, long patNum)
    {
        var command = "SELECT EstBalance FROM patient "
                      + "WHERE PatNum = " + patNum;
        var table = DataCore.GetTable(command);
        double estBal = 0;
        if (table.Rows.Count > 0) estBal = SIn.Double(table.Rows[0][0].ToString());
        if (!PrefC.GetBool(PrefName.BalancesDontSubtractIns))
        {
            command = @"SELECT SUM(InsPayEst)+SUM(Writeoff) 
					FROM claimproc
					WHERE PatNum=" + patNum + " "
                      + "AND Status=0"; //NotReceived
            table = DataCore.GetTable(command);
            if (table.Rows.Count > 0) estBal -= SIn.Double(table.Rows[0][0].ToString());
        }

        if (payAmt > estBal) return true;
        return false;
    }

    public static List<PaySplit> Allocate(Payment pay)
    {
        //double amtTot,int patNum,Payment payNum){

        var command =
            "SELECT Guarantor FROM patient "
            + "WHERE PatNum = " + pay.PatNum;
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return [];
        command =
            "SELECT patient.PatNum,EstBalance,PriProv,SUM(InsPayEst)+SUM(Writeoff) insEst_ "
            + "FROM patient "
            + "LEFT JOIN claimproc ON patient.PatNum=claimproc.PatNum "
            + "AND Status=0 " //NotReceived
            + "WHERE Guarantor = " + table.Rows[0][0] + " "
            + "GROUP BY  patient.PatNum,EstBalance,PriProv";
        //+" ORDER BY PatNum!="+POut.PInt(pay.PatNum);//puts current patient in position 0 //Oracle does not allow
        table = DataCore.GetTable(command);
        var pats = new List<Patient>();
        Patient pat;
        //first, put the current patient at position 0.
        for (var i = 0; i < table.Rows.Count; i++)
            if (table.Rows[i]["PatNum"].ToString() == pay.PatNum.ToString())
            {
                pat = new Patient();
                pat.PatNum = SIn.Long(table.Rows[i][0].ToString());
                pat.EstBalance = SIn.Double(table.Rows[i][1].ToString());
                if (!PrefC.GetBool(PrefName.BalancesDontSubtractIns)) pat.EstBalance -= SIn.Double(table.Rows[i]["insEst_"].ToString());
                pat.PriProv = SIn.Long(table.Rows[i][2].ToString());
                pats.Add(pat.Copy());
            }

        //then, do all the rest of the patients.
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (table.Rows[i]["PatNum"].ToString() == pay.PatNum.ToString()) continue;
            pat = new Patient();
            pat.PatNum = SIn.Long(table.Rows[i][0].ToString());
            pat.EstBalance = SIn.Double(table.Rows[i][1].ToString());
            if (!PrefC.GetBool(PrefName.BalancesDontSubtractIns)) pat.EstBalance -= SIn.Double(table.Rows[i]["insEst_"].ToString());
            pat.PriProv = SIn.Long(table.Rows[i][2].ToString());
            pats.Add(pat.Copy());
        }

        //first calculate all the amounts
        var amtRemain = pay.PayAmt; //start off with the full amount
        var amtSplits = new double[pats.Count];
        //loop through each family member, starting with current
        for (var i = 0; i < pats.Count; i++)
        {
            if (pats[i].EstBalance == 0 || pats[i].EstBalance < 0) continue; //don't apply paysplits to anyone with a negative balance
            if (amtRemain < pats[i].EstBalance)
            {
                //entire remainder can be allocated to this patient
                amtSplits[i] = amtRemain;
                amtRemain = 0;
                break;
            } //amount remaining is more than or equal to the estBal for this family member

            amtSplits[i] = pats[i].EstBalance;
            amtRemain -= pats[i].EstBalance;
        }

        //add any remainder to the split for this patient
        amtSplits[0] += amtRemain;
        //now create a split for each non-zero amount
        PaySplit PaySplitCur;
        var retVal = new List<PaySplit>();
        for (var i = 0; i < pats.Count; i++)
        {
            if (amtSplits[i] == 0) continue;
            PaySplitCur = new PaySplit();
            PaySplitCur.PatNum = pats[i].PatNum;
            PaySplitCur.PayNum = pay.PayNum;
            PaySplitCur.DatePay = pay.PayDate;
            PaySplitCur.ClinicNum = pay.ClinicNum;
            PaySplitCur.ProvNum = Patients.GetProvNum(pats[i]);
            PaySplitCur.SplitAmt = Math.Round(amtSplits[i], CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits);
            //PaySplitCur.InsertOrUpdate(true);
            retVal.Add(PaySplitCur);
        }

        //finally, adjust each EstBalance, but no need to do current patient
        //This no longer works here.  Must do it when closing payment window somehow
        /*for(int i=1;i<pats.Length;i++){
            if(amtSplits[i]==0){
                continue;
            }
            command="UPDATE patient SET EstBalance=EstBalance-"+POut.PDouble(amtSplits[i])
                +" WHERE PatNum="+POut.PInt(pats[i].PatNum);
            Db.NonQ(command);
        }*/
        return retVal;
    }

    public static bool ValidateLinkedEntries(double oldAmt, double newAmt, long payNum, long newAcct)
    {
        if (!Accounts.PaymentsLinked()) return false; //user has not even set up accounting links, so no need to check any of this.
        var amtChanged = false;
        if (oldAmt != newAmt) amtChanged = true;
        var trans = Transactions.GetAttachedToPayment(payNum); //this gives us the oldAcctNum
        if (trans == null && (newAcct == 0 || newAcct == -1)) //if there was no previous link, and there is no attempt to create a link
            return false; //no synch needed
        if (trans == null) //no previous link, but user is trying to create one. newAcct>0.
            return true; //new transaction will be required
        if (newAcct == 0 && Transactions.IsAttachedToLockedReconcile(trans)) //trying to change payment type to one without deposit account
            throw new ApplicationException(Lans.g("Transactions", "Not allowed to change a transaction that is attached to a locked accounting reconcile."));
        //at this point, we have established that there is a previous transaction.
        //If payment is attached to a transaction which is more than 48 hours old, then not allowed to change.
        if (amtChanged && trans.DateTimeEntry < MiscData.GetNowDateTime().AddDays(-2)) throw new ApplicationException(Lans.g("Payments", "Not allowed to change amount that is more than 48 hours old.  This payment is already attached to an accounting transaction.  You will need to detach it from within the accounting section of the program."));
        if (amtChanged && Transactions.IsReconciled(trans)) throw new ApplicationException(Lans.g("Payments", "Not allowed to change amount.  This payment is attached to an accounting transaction that has been reconciled.  You will need to detach it from within the accounting section of the program."));
        var jeL = JournalEntries.GetForTrans(trans.TransactionNum);
        long oldAcct = 0;
        JournalEntry jeDebit = null;
        JournalEntry jeCredit = null;
        var absOld = oldAmt; //the absolute value of the old amount
        if (oldAmt < 0) absOld = -oldAmt;
        for (var i = 0; i < jeL.Count; i++)
        {
            //we make sure down below that this count is exactly 2.
            if (Accounts.GetAccount(jeL[i].AccountNum).AcctType == AccountType.Asset) oldAcct = jeL[i].AccountNum;
            if (jeL[i].DebitAmt == absOld) jeDebit = jeL[i];
            //old credit entry
            if (jeL[i].CreditAmt == absOld) jeCredit = jeL[i];
        }

        if (jeCredit == null || jeDebit == null) throw new ApplicationException(Lans.g("Payments", "Not able to automatically make changes in the accounting section to match the change made here.  You will need to detach it from within the accounting section."));
        if (oldAcct == 0) //something must have gone wrong.  But this should never happen
            throw new ApplicationException(Lans.g("Payments", "Could not locate linked transaction.  You will need to detach it manually from within the accounting section of the program."));
        if (newAcct == 0) //detaching it from a linked transaction.
            //We will delete the transaction
            return true;
        var acctChanged = false;
        if (newAcct != -1 && oldAcct != newAcct) acctChanged = true; //changing linked acctNum
        if (!amtChanged && !acctChanged) return false; //no changes being made to amount or account, so no synch required.
        if (jeL.Count != 2) throw new ApplicationException(Lans.g("Payments", "Not able to automatically change the amount in the accounting section to match the change made here.  You will need to detach it from within the accounting section."));
        //Amount or account changed on an existing linked transaction.
        return true;
    }

    public static void AlterLinkedEntries(double oldAmt, double newAmt, long payNum, long newAcct, DateTime payDate, string patName)
    {
        if (!Accounts.PaymentsLinked()) return; //user has not even set up accounting links.
        var amtChanged = false;
        if (oldAmt != newAmt) amtChanged = true;
        var trans = Transactions.GetAttachedToPayment(payNum); //this gives us the oldAcctNum
        var absNew = newAmt; //absolute value of the new amount
        if (newAmt < 0) absNew = -newAmt;
        //if(trans==null && (newAcct==0 || newAcct==-1)) {//then this method will not even be called
        if (trans == null)
        {
            //no previous link, but user is trying to create one.
            //this is the only case where a new trans is required.
            trans = new Transaction();
            trans.PayNum = payNum;
            trans.UserNum = Security.CurUser.UserNum;
            Transactions.Insert(trans); //sets entry date
            //first the deposit entry
            var je = new JournalEntry();
            je.AccountNum = newAcct; //DepositAccounts[comboDepositAccount.SelectedIndex];
            je.CheckNumber = Lans.g("Payments", "DEP");
            je.DateDisplayed = payDate; //it would be nice to add security here.
            if (absNew == newAmt) //amount is positive
                je.DebitAmt = newAmt;
            else
                je.CreditAmt = absNew;
            je.Memo = Lans.g("Payments", "Payment -") + " " + patName;
            je.Splits = Accounts.GetDescript(PrefC.GetLong(PrefName.AccountingCashIncomeAccount));
            je.TransactionNum = trans.TransactionNum;
            JournalEntries.Insert(je);
            //then, the income entry
            je = new JournalEntry();
            je.AccountNum = PrefC.GetLong(PrefName.AccountingCashIncomeAccount);
            je.CheckNumber = Lans.g("Payments", "DEP");
            je.DateDisplayed = payDate; //it would be nice to add security here.
            if (absNew == newAmt) //amount is positive
                je.CreditAmt = newAmt;
            else
                je.DebitAmt = absNew;
            je.Memo = Lans.g("Payments", "Payment -") + " " + patName;
            je.Splits = Accounts.GetDescript(newAcct);
            je.TransactionNum = trans.TransactionNum;
            JournalEntries.Insert(je);
            return;
        }

        //at this point, we have established that there is a previous transaction.
        var jeL = JournalEntries.GetForTrans(trans.TransactionNum);
        long oldAcct = 0;
        JournalEntry jeDebit = null;
        JournalEntry jeCredit = null;
        var signChanged = false;
        var absOld = oldAmt; //the absolute value of the old amount
        if (oldAmt < 0) absOld = -oldAmt;
        if (oldAmt < 0 && newAmt > 0) signChanged = true;
        if (oldAmt > 0 && newAmt < 0) signChanged = true;
        for (var i = 0; i < 2; i++)
        {
            if (Accounts.GetAccount(jeL[i].AccountNum).AcctType == AccountType.Asset) oldAcct = jeL[i].AccountNum;
            if (jeL[i].DebitAmt == absOld) jeDebit = jeL[i];
            //old credit entry
            if (jeL[i].CreditAmt == absOld) jeCredit = jeL[i];
        }

        //Already validated that both je's are not null, and that oldAcct is not 0.
        if (newAcct == 0)
        {
            //detaching it from a linked transaction. We will delete the transaction
            //we don't care about the amount
            Transactions.Delete(trans); //we need to make sure this doesn't throw any exceptions by carefully checking all
            //possibilities in the validation routine above.
            return;
        }

        //Either the amount or the account changed on an existing linked transaction.
        var acctChanged = false;
        if (newAcct != -1 && oldAcct != newAcct) acctChanged = true; //changing linked acctNum
        if (amtChanged)
        {
            if (signChanged)
            {
                jeDebit.DebitAmt = 0;
                jeDebit.CreditAmt = absNew;
                jeCredit.DebitAmt = absNew;
                jeCredit.CreditAmt = 0;
            }
            else
            {
                jeDebit.DebitAmt = absNew;
                jeCredit.CreditAmt = absNew;
            }
        }

        if (acctChanged)
        {
            if (jeDebit.AccountNum == oldAcct) jeDebit.AccountNum = newAcct;
            if (jeCredit.AccountNum == oldAcct) jeCredit.AccountNum = newAcct;
        }

        JournalEntries.Update(jeDebit);
        JournalEntries.Update(jeCredit);
    }

    private static PaySplit CreateSinglePaySplitForWeb(Payment odbPayment, Patient odbPatient, double paymentAmount)
    {
        var paySplit = new PaySplit();
        paySplit.PatNum = odbPatient.PatNum;
        paySplit.PayNum = odbPayment.PayNum;
        paySplit.DatePay = odbPayment.PayDate;
        paySplit.ClinicNum = odbPayment.ClinicNum;
        paySplit.SplitAmt = paymentAmount;
        var rigorousAccounting = PrefC.GetEnum<RigorousAccounting>(PrefName.RigorousAccounting);
        if (rigorousAccounting == RigorousAccounting.DontEnforce)
        {
            paySplit.ProvNum = Patients.GetProvNum(odbPatient);
        }
        else
        {
            paySplit.ProvNum = 0;
            paySplit.UnearnedType = PrefC.GetLong(PrefName.PrepaymentUnearnedType); //Use default unallocated type
        }

        return paySplit;
    }

    public static string GetSecuritylogEntryText(Payment paymentNew, Payment paymentOld, bool isNew, List<Def> listPayTypes = null)
    {
        string secLogText;
        if (listPayTypes == null) listPayTypes = Defs.GetDefsForCategory(DefCat.PaymentTypes);
        var clinicAbbrNew = Clinics.GetAbbr(paymentNew.ClinicNum);
        if (isNew)
        {
            secLogText = $"Payment created for {Patients.GetLim(paymentNew.PatNum).GetNameLF()} with payment type '" +
                         GetPaymentTypeDesc(paymentNew, listPayTypes) + "'";
            if (!string.IsNullOrEmpty(clinicAbbrNew)) secLogText += $", clinic '{clinicAbbrNew}'";
            secLogText += $", amount '{paymentNew.PayAmt.ToString("c")}'";
        }
        else
        {
            secLogText = $"Payment edited for {Patients.GetLim(paymentNew.PatNum).GetNameLF()}";
            secLogText += SecurityLogEntryTextHelper(paymentNew.PayAmt.ToString("c"), paymentOld.PayAmt.ToString("c"), "amount");
            secLogText += SecurityLogEntryTextHelper(clinicAbbrNew, Clinics.GetAbbr(paymentOld.ClinicNum), "clinic");
            secLogText += SecurityLogEntryTextHelper(paymentNew.PayDate.ToShortDateString(), paymentOld.PayDate.ToShortDateString(), "payment date");
            secLogText += SecurityLogEntryTextHelper(GetPaymentTypeDesc(paymentNew, listPayTypes), GetPaymentTypeDesc(paymentOld, listPayTypes), "payment type");
        }

        return secLogText;
    }

    public static string GetPaymentTypeDesc(Payment payment, List<Def> listPayTypes = null)
    {
        if (listPayTypes == null) listPayTypes = Defs.GetDefsForCategory(DefCat.PaymentTypes);
        return payment.PayType == 0 ? "Income Transfer" : Defs.GetName(DefCat.PaymentTypes, payment.PayType, listPayTypes);
    }

    private static string SecurityLogEntryTextHelper(string newVal, string oldVal, string textInLog)
    {
        return newVal != oldVal ? $"\r\n {textInLog} changed from '{oldVal}' to '{newVal}'" : "";
    }

    public static void CreateTransferForTpProcs(Procedure procOriginal, List<PaySplit> listSplitsForProc, Procedure procAttaching = null, double transferAmountOverride = 0)
    {
        if (listSplitsForProc.IsNullOrEmpty() || listSplitsForProc.Sum(x => x.SplitAmt) == 0) return;
        //Remove all TP that are associated to DPP/PP
        listSplitsForProc.RemoveAll(x => x.PayPlanChargeNum != 0);
        if (listSplitsForProc.IsNullOrEmpty()) return;
        procAttaching = procAttaching ?? procOriginal;
        var transferPayment = new Payment();
        transferPayment.PayDate = DateTime.Today;
        transferPayment.ClinicNum = procOriginal.ClinicNum;
        transferPayment.PayNote = "Automatic transfer from treatment planned procedure prepayment.";
        transferPayment.PatNum = procAttaching.PatNum; //ultimately where the payment ends up.
        transferPayment.PayType = 0;
        Insert(transferPayment);
        foreach (var prepaySplit in listSplitsForProc)
        {
            //make negative split to remove the 'tp prepayment'
            var negSplitForTxfr = new PaySplit
            {
                ClinicNum = procOriginal.ClinicNum,
                DatePay = DateTime.Today,
                ProcNum = 0, //either the procedure is being set complete, or pref for Non-Refundable TP prepay is set and transferring to procAttaching.
                //If non-refundable the procedure needs to be disassociated as well as we will not have a way to determine when procOriginal eventually
                //gets set complete and unearned cannot exist with a completed procedure attached. 
                PatNum = procOriginal.PatNum,
                PayNum = transferPayment.PayNum,
                SplitAmt = (transferAmountOverride == 0 ? prepaySplit.SplitAmt : transferAmountOverride) * -1,
                UnearnedType = prepaySplit.UnearnedType,
                ProcDate = procOriginal.ProcDate,
                ProvNum = prepaySplit.ProvNum
            };
            PaySplits.Insert(negSplitForTxfr);
            //Update original pre-payment split to disassociate the procedure now that the procedure is complete (Splits cannot have unaerned and C proc)
            prepaySplit.ProcNum = 0;
            PaySplits.Update(prepaySplit);
            var positiveSplit = new PaySplit
            {
                ClinicNum = procAttaching.ClinicNum,
                DatePay = DateTime.Today,
                ProcNum = procAttaching.ProcNum,
                PatNum = procAttaching.PatNum,
                PayNum = transferPayment.PayNum,
                SplitAmt = transferAmountOverride == 0 ? prepaySplit.SplitAmt : transferAmountOverride,
                ProcDate = procAttaching.ProcDate,
                ProvNum = procAttaching.ProvNum,
                UnearnedType = 0 //necessary for when broken appointments do not get a procedure created for them to transfer to. 
            };
            PaySplits.Insert(positiveSplit);
        }

        SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, transferPayment.PatNum, "Automatic transfer of funds for treatment plan procedure pre-payments.");
    }

    public static long ProcessPaymentForWeb(Payment odbPayment, Patient odbPatient, double payAmt, bool isPatientPreferred = false, bool isPrepayment = false, List<AccountEntry> listAccountEntries = null, long payPlanNum = 0)
    {
        var autoSplitData = PaymentEdit.AutoSplitForPayment(odbPayment.PatNum, odbPayment, isPatPrefer: isPatientPreferred, listAccountEntriesPayFirst: listAccountEntries, payPlanNum: payPlanNum);
        odbPayment.PayAmt = payAmt; //AutoSplitForPayment empties PayAmt - Set it back to what it should be.
        //Zero dollar splits are not valid. Remove.
        autoSplitData.ListPaySplitsSuggested.RemoveAll(x => CompareDouble.IsZero(x.SplitAmt));
        //All payments must have one split
        if (autoSplitData.ListPaySplitsSuggested.Count == 0)
        {
            var odbPaySplit = CreateSinglePaySplitForWeb(odbPayment, odbPatient, odbPayment.PayAmt);
            autoSplitData.ListPaySplitsSuggested.Add(odbPaySplit);
        }

        //If there is extra or negative money leftover when making splits, put it into one last split.
        var payDifference = odbPayment.PayAmt - autoSplitData.ListPaySplitsSuggested.Sum(x => x.SplitAmt);
        if (payDifference != 0)
        {
            var odbPaySplit = CreateSinglePaySplitForWeb(odbPayment, odbPatient, payDifference);
            autoSplitData.ListPaySplitsSuggested.Add(odbPaySplit);
        }

        //If the payment is a Prepayment, clear all PaySplits and create one for the total. Mimics FormPayment.butPrePay_Click().
        if (isPrepayment)
        {
            autoSplitData.ListPaySplitsSuggested.Clear();
            var odbPaySplit = CreateSinglePaySplitForWeb(odbPayment, odbPatient, odbPayment.PayAmt);
            if (payPlanNum > 0)
            {
                var payPlan = PayPlans.GetOne(payPlanNum);
                if (payPlan.IsDynamic)
                {
                    odbPaySplit.PayPlanNum = payPlanNum;
                    odbPaySplit.UnearnedType = PrefC.GetLong(PrefName.DynamicPayPlanPrepaymentUnearnedType);
                }
            }

            autoSplitData.ListPaySplitsSuggested.Add(odbPaySplit);
        }

        var ret = Insert(odbPayment, autoSplitData.ListPaySplitsSuggested);
        //Compute Aging
        var odbFamily = Patients.GetFamily(odbPatient.PatNum);
        Ledgers.ComputeAgingForPaysplitsAllocatedToDiffPats(odbPatient.PatNum, autoSplitData.ListPaySplitsSuggested);
        Ledgers.ComputeAging(odbFamily.Guarantor.PatNum, DateTime.Now);
        Signalods.SetInvalid(InvalidType.BillingList);
        return ret;
    }
}