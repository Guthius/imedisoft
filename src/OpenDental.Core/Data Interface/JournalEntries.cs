using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class JournalEntries
{
    ///<summary>Used when displaying the splits for a transaction.</summary>
    public static List<JournalEntry> GetForTrans(long transactionNum)
    {
        var command =
            "SELECT * FROM journalentry "
            + "WHERE TransactionNum=" + SOut.Long(transactionNum);
        return JournalEntryCrud.SelectMany(command);
    }

    /// <summary>
    ///     Used to display a list of entries for one account. Even though we're passing in a dateFrom, we always get full
    ///     list for assets, liabilities, and equity in order to get the running total, even if we don't show those rows.
    /// </summary>
    public static List<JournalEntry> GetForAccount(Account account, DateTime dateFrom, DateTime dateTo)
    {
        string command;
        var listJournalEntries = new List<JournalEntry>();

        #region StartingBalanceRow

        //Get history balance and create a starting balance row.
        var dateFirstOfYearFrom = new DateTime(dateFrom.Year, 1, 1);
        var dateFirstOfYearTo = new DateTime(dateTo.Year, 1, 1);
        command = "SELECT SUM(ROUND(CreditAmt,3)) SumCredit, "
                  + "SUM(ROUND(DebitAmt,3)) SumDebit "
                  + "FROM journalentry "
                  + "WHERE AccountNum='" + SOut.Long(account.AccountNum) + "' "
                  + "AND DateDisplayed < " + SOut.Date(dateFrom);
        if (account.AcctType.In(AccountType.Income, AccountType.Expense))
            //For Income and Expense, if their dateFrom is not 1/1, then we might need a starting balance for part of a year.
            //Usually, with a 1/1 start date, this will result in no range, and a $0 starting row that doesn't show.
            command += " AND DateDisplayed >= " + SOut.Date(dateFirstOfYearFrom);
        var table = DataCore.GetTable(command); //always exactly one row
        var credit = SIn.Double(table.Rows[0]["SumCredit"].ToString());
        var debit = SIn.Double(table.Rows[0]["SumDebit"].ToString());
        double balStart = 0;
        if (Accounts.DebitIsPos(account.AcctType))
            balStart = debit - credit;
        else
            balStart = credit - debit;
        double amtBalRE = 0;
        if (account.IsRetainedEarnings)
        {
            //Now we need an entry that's the sum all previous RE entries
            command = "SELECT SUM(ROUND(CreditAmt,3))-SUM(ROUND(DebitAmt,3)) "
                      + "FROM journalentry,account "
                      + "WHERE journalentry.AccountNum=account.AccountNum "
                      + "AND (account.AcctType='" + SOut.Enum(AccountType.Income) + "' "
                      + "OR account.AcctType='" + SOut.Enum(AccountType.Expense) + "') "
                      + "AND DateDisplayed < " + SOut.Date(dateFirstOfYearFrom);
            amtBalRE = SIn.Double(Db.GetCount(command)); //always a single cell
        }

        balStart += amtBalRE; //no change for non-RE
        var journalEntry = new JournalEntry();
        journalEntry.CheckNumber = "";
        if (dateFrom.Year > 1880) journalEntry.DateDisplayed = dateFrom.AddDays(-1);
        journalEntry.Memo = Lans.g("FormJournal", "(starting balance)");
        journalEntry.Splits = "";
        if (Accounts.DebitIsPos(account.AcctType))
        {
            if (balStart >= 0)
                journalEntry.DebitAmt = balStart;
            else
                journalEntry.CreditAmt = -balStart;
        }
        else
        {
            if (balStart >= 0)
                journalEntry.CreditAmt = balStart;
            else
                journalEntry.DebitAmt = -balStart;
        }

        //The debit or credit will be used later to arrive at a starting bal to show
        listJournalEntries.Add(journalEntry);

        #endregion StartingBalanceRow

        #region RetainedEarningsAutoEntries

        if (account.IsRetainedEarnings)
        {
            //For Retained Earnings, add the auto entries for each year
            //Only show the ones in our date range.
            //RE entries prior to our date range are already included in starting bal.
            //This will normally return no rows, unless date span is greater than one year.
            //dateFrom could be empty, so 1/1/1
            command = "SELECT SUM(ROUND(CreditAmt,3))-SUM(ROUND(DebitAmt,3)) AS Amount, "
                      + "YEAR(journalentry.DateDisplayed) AS yearDis "
                      + "FROM journalentry,account "
                      + "WHERE journalentry.AccountNum=account.AccountNum "
                      + "AND (account.AcctType='" + SOut.Enum(AccountType.Income) + "' "
                      + "OR account.AcctType='" + SOut.Enum(AccountType.Expense) + "') "
                      + "AND DateDisplayed < " + SOut.Date(dateFirstOfYearTo) + " "
                      + "AND DateDisplayed >= " + SOut.Date(dateFirstOfYearFrom) + " "
                      + "GROUP BY yearDis";
            table = DataCore.GetTable(command);
            for (var i = 0; i < table.Rows.Count; i++)
            {
                journalEntry = new JournalEntry();
                journalEntry.CheckNumber = "";
                journalEntry.Splits = "";
                var year = SIn.Int(table.Rows[i]["yearDis"].ToString());
                journalEntry.DateDisplayed = new DateTime(year, 12, 31);
                var amount = SIn.Double(table.Rows[i]["Amount"].ToString());
                if (amount > 0)
                    journalEntry.CreditAmt = amount;
                else
                    journalEntry.DebitAmt = -amount;
                journalEntry.Memo = Lans.g("FormJournal", "(auto)");
                listJournalEntries.Add(journalEntry);
            }
        }

        #endregion RetainedEarningsAutoEntries

        #region ExpenseIncomeAutoEntries

        //For income and expense accounts, if our range showing crosses any annual boundaries,
        //then we need to have an auto entry at each of those points to zero out the running balance
        if (account.AcctType.In(AccountType.Income, AccountType.Expense))
        {
            command = "SELECT SUM(ROUND(CreditAmt,3))-SUM(ROUND(DebitAmt,3)) AS Amount, "
                      + "YEAR(DateDisplayed) AS yearDis "
                      + "FROM journalentry "
                      + "WHERE AccountNum='" + SOut.Long(account.AccountNum) + "' "
                      + "AND DateDisplayed < " + SOut.Date(dateFirstOfYearTo) + " "
                      + "AND DateDisplayed >= " + SOut.Date(dateFirstOfYearFrom) + " "
                      + "GROUP BY yearDis";
            table = DataCore.GetTable(command);
            for (var i = 0; i < table.Rows.Count; i++)
            {
                journalEntry = new JournalEntry();
                journalEntry.CheckNumber = "";
                journalEntry.Splits = "";
                var year = SIn.Int(table.Rows[i]["yearDis"].ToString());
                journalEntry.DateDisplayed = new DateTime(year, 12, 31);
                var amount = SIn.Double(table.Rows[i]["Amount"].ToString());
                //this math is the same for both types because the query got credits as pos.
                if (amount > 0)
                    journalEntry.DebitAmt = amount;
                else
                    journalEntry.CreditAmt = -amount;
                journalEntry.Memo = Lans.g("FormJournal", "(auto)");
                listJournalEntries.Add(journalEntry);
            }
        }

        #endregion ExpenseIncomeAutoEntries

        command =
            "SELECT * FROM journalentry "
            + "WHERE AccountNum='" + SOut.Long(account.AccountNum) + "' "
            + "AND DateDisplayed >= " + SOut.Date(dateFrom) + " "
            + "AND DateDisplayed <= " + SOut.Date(dateTo) + " "
            + "ORDER BY DateDisplayed";
        listJournalEntries.AddRange(JournalEntryCrud.SelectMany(command));
        listJournalEntries = listJournalEntries.OrderBy(x => x.DateDisplayed)
            .ThenByDescending(x => x.AccountNum).ToList(); //this makes the auto entry come after other entries on that date
        return listJournalEntries;
    }

    ///<summary>Used in reconcile window.</summary>
    public static List<JournalEntry> GetForReconcile(long accountNum, bool includeUncleared, long reconcileNum)
    {
        var command =
            "SELECT * FROM journalentry "
            + "WHERE AccountNum=" + SOut.Long(accountNum)
            + " AND (ReconcileNum=" + SOut.Long(reconcileNum);
        if (includeUncleared)
            command += " OR ReconcileNum=0)";
        else
            command += ")";
        command += " ORDER BY DateDisplayed";
        return JournalEntryCrud.SelectMany(command);
    }

    
    public static long Insert(JournalEntry journalEntry)
    {
        journalEntry.SecUserNumEntry = Security.CurUser.UserNum; //Before middle tier check to catch user at workstation
        journalEntry.SecUserNumEdit = Security.CurUser.UserNum;

        if (journalEntry.DebitAmt < 0 || journalEntry.CreditAmt < 0) throw new ApplicationException(Lans.g("JournalEntries", "Error. Credit and debit must both be positive."));
        return JournalEntryCrud.Insert(journalEntry);
    }

    
    public static void Update(JournalEntry journalEntry)
    {
        journalEntry.SecUserNumEdit = Security.CurUser.UserNum; //Before middle tier check to catch user at workstation

        if (journalEntry.DebitAmt < 0 || journalEntry.CreditAmt < 0) throw new ApplicationException(Lans.g("JournalEntries", "Error. Credit and debit must both be positive."));
        JournalEntryCrud.Update(journalEntry);
    }

    
    public static void Delete(JournalEntry journalEntry)
    {
        //This method is only used once in synch below.  Validation needs to be done, but doing it inside the loop would be dangerous.
        //So validation is done in the UI as follows:
        //1. Deleting an entire transaction is validated in business layer.
        //2. When editing a transaction attached to reconcile, simple view is blocked.
        //3. Double clicking on grid lets you change JEs not attached to reconcile.
        //4. Double clicking on grid lets you change notes even if attached to reconcile.
        var command = "DELETE FROM journalentry WHERE JournalEntryNum = " + SOut.Long(journalEntry.JournalEntryNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Used in FormTransactionEdit to synch database with changes user made to the journalEntry list for a
    ///     transaction.  Must supply an old list for comparison.  Only the differences are saved.  Surround with try/catch,
    ///     because it will thrown an exception if any entries are negative.
    /// </summary>
    public static void UpdateList(List<JournalEntry> listJournalEntriesOld, List<JournalEntry> listJournalEntriesNew)
    {
        for (var i = 0; i < listJournalEntriesNew.Count; i++)
            if (listJournalEntriesNew[i].DebitAmt < 0 || listJournalEntriesNew[i].CreditAmt < 0)
                throw new ApplicationException(Lans.g("JournalEntries", "Error. Credit and debit must both be positive."));

        JournalEntry journalEntryNew;
        for (var i = 0; i < listJournalEntriesOld.Count; i++)
        {
            //loop through the old list
            journalEntryNew = null;
            for (var j = 0; j < listJournalEntriesNew.Count; j++)
            {
                if (listJournalEntriesNew[j] == null || listJournalEntriesNew[j].JournalEntryNum == 0) continue;
                if (listJournalEntriesOld[i].JournalEntryNum == listJournalEntriesNew[j].JournalEntryNum)
                {
                    journalEntryNew = listJournalEntriesNew[j];
                    break;
                }
            }

            if (journalEntryNew == null)
            {
                //journalentry with matching journalEntryNum was not found, so it must have been deleted
                Delete(listJournalEntriesOld[i]);
                continue;
            }

            //journalentry was found with matching journalEntryNum, so check for changes
            if (journalEntryNew.AccountNum != listJournalEntriesOld[i].AccountNum
                || journalEntryNew.DateDisplayed != listJournalEntriesOld[i].DateDisplayed
                || journalEntryNew.DebitAmt != listJournalEntriesOld[i].DebitAmt
                || journalEntryNew.CreditAmt != listJournalEntriesOld[i].CreditAmt
                || journalEntryNew.Memo != listJournalEntriesOld[i].Memo
                || journalEntryNew.Splits != listJournalEntriesOld[i].Splits
                || journalEntryNew.CheckNumber != listJournalEntriesOld[i].CheckNumber)
                Update(journalEntryNew);
        }

        for (var i = 0; i < listJournalEntriesNew.Count; i++)
        {
            //loop through the new list
            if (listJournalEntriesNew[i] == null) continue;
            if (listJournalEntriesNew[i].JournalEntryNum != 0) continue;
            //entry with journalEntryNum=0, so it's new
            Insert(listJournalEntriesNew[i]);
        }
    }

    ///<summary>Called from FormTransactionEdit.</summary>
    public static bool AttachedToReconcile(List<JournalEntry> listJournalEntries)
    {
        for (var i = 0; i < listJournalEntries.Count; i++)
            if (listJournalEntries[i].ReconcileNum != 0)
                return true;

        return false;
    }

    ///<summary>Called from FormTransactionEdit.</summary>
    public static DateTime GetReconcileDate(List<JournalEntry> listJournalEntries)
    {
        for (var i = 0; i < listJournalEntries.Count; i++)
            if (listJournalEntries[i].ReconcileNum != 0)
                return Reconciles.GetOne(listJournalEntries[i].ReconcileNum).DateReconcile;

        return DateTime.MinValue;
    }

    ///<summary>Called once from FormReconcileEdit when closing.  Saves the reconcileNum for every item in the list.</summary>
    public static void SaveList(List<JournalEntry> listJournalEntries, long reconcileNum)
    {
        var command = "UPDATE journalentry SET ReconcileNum=0 WHERE";
        var str = "";
        for (var i = 0; i < listJournalEntries.Count; i++)
        {
            if (listJournalEntries[i].ReconcileNum != 0) continue;
            if (str != "") str += " OR";
            str += " JournalEntryNum=" + SOut.Long(listJournalEntries[i].JournalEntryNum);
        }

        if (str != "")
        {
            command += str;
            Db.NonQ(command);
        }

        command = "UPDATE journalentry SET ReconcileNum=" + SOut.Long(reconcileNum) + " WHERE";
        str = "";
        for (var i = 0; i < listJournalEntries.Count; i++)
        {
            if (listJournalEntries[i].ReconcileNum != reconcileNum) continue;
            if (str != "") str += " OR";
            str += " JournalEntryNum=" + SOut.Long(listJournalEntries[i].JournalEntryNum);
        }

        if (str != "")
        {
            command += str;
            Db.NonQ(command);
        }
    }

    ///<Summary>Returns true if the account was used in any journal entry.</Summary>
    public static bool IsInUse(long accountNum)
    {
        var command = "SELECT COUNT(*) FROM journalentry "
                      + "WHERE AccountNum=" + SOut.Long(accountNum);
        var count = Db.GetCount(command);
        if (count == "0") return false;
        return true;
    }
}