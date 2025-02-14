using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class JournalEntries
{
    public static List<JournalEntry> GetForTrans(long transactionNum)
    {
        return JournalEntryCrud.SelectMany("SELECT * FROM journalentry WHERE TransactionNum = " + transactionNum);
    }

    public static List<JournalEntry> GetForAccount(Account account, DateTime dateFrom, DateTime dateTo)
    {
        var journalEntries = new List<JournalEntry>();

        var from = new DateTime(dateFrom.Year, 1, 1);
        var to = new DateTime(dateTo.Year, 1, 1);

        var commandText =
            "SELECT SUM(ROUND(CreditAmt,3)) SumCredit, " +
            "SUM(ROUND(DebitAmt,3)) SumDebit " +
            "FROM journalentry " +
            "WHERE AccountNum='" + account.AccountNum + "' " +
            "AND DateDisplayed < " + SOut.Date(dateFrom);

        if (account.AcctType is AccountType.Income or AccountType.Expense)
        {
            commandText += " AND DateDisplayed >= " + SOut.Date(from);
        }

        var dataTable = DataCore.GetTable(commandText);

        var credit = SIn.Double(dataTable.Rows[0]["SumCredit"].ToString());
        var debit = SIn.Double(dataTable.Rows[0]["SumDebit"].ToString());

        double startBalance;
        if (Accounts.DebitIsPos(account.AcctType))
        {
            startBalance = debit - credit;
        }
        else
        {
            startBalance = credit - debit;
        }

        double retainedEarningsBalance = 0;

        if (account.IsRetainedEarnings)
        {
            retainedEarningsBalance = SIn.Double(Db.GetCount(
                "SELECT SUM(ROUND(CreditAmt,3))-SUM(ROUND(DebitAmt,3)) " +
                "FROM journalentry,account " +
                "WHERE journalentry.AccountNum=account.AccountNum " +
                "AND (account.AcctType='" + (int) AccountType.Income + "' " +
                "OR account.AcctType='" + (int) AccountType.Expense + "') " +
                "AND DateDisplayed < " + SOut.Date(from)));
        }

        startBalance += retainedEarningsBalance;

        var journalEntry = new JournalEntry
        {
            CheckNumber = "",
            Memo = "(starting balance)",
            Splits = ""
        };

        if (dateFrom.Year > 1880)
        {
            journalEntry.DateDisplayed = dateFrom.AddDays(-1);
        }

        if (Accounts.DebitIsPos(account.AcctType))
        {
            if (startBalance >= 0)
            {
                journalEntry.DebitAmt = startBalance;
            }
            else
            {
                journalEntry.CreditAmt = -startBalance;
            }
        }
        else
        {
            if (startBalance >= 0)
            {
                journalEntry.CreditAmt = startBalance;
            }
            else
            {
                journalEntry.DebitAmt = -startBalance;
            }
        }

        journalEntries.Add(journalEntry);

        if (account.IsRetainedEarnings)
        {
            dataTable = DataCore.GetTable(
                "SELECT SUM(ROUND(CreditAmt, 3)) - SUM(ROUND(DebitAmt, 3)) AS Amount, " +
                "YEAR(journalentry.DateDisplayed) AS yearDis " +
                "FROM journalentry, account " +
                "WHERE journalentry.AccountNum=account.AccountNum " +
                "AND (account.AcctType = '" + (int) AccountType.Income + "' " +
                "OR account.AcctType = '" + (int) AccountType.Expense + "') " +
                "AND DateDisplayed < " + SOut.Date(to) + " " +
                "AND DateDisplayed >= " + SOut.Date(from) + " " +
                "GROUP BY yearDis");

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var year = SIn.Int(dataTable.Rows[i]["yearDis"].ToString());

                journalEntry = new JournalEntry
                {
                    CheckNumber = "",
                    Splits = "",
                    DateDisplayed = new DateTime(year, 12, 31)
                };

                var amount = SIn.Double(dataTable.Rows[i]["Amount"].ToString());
                if (amount > 0)
                {
                    journalEntry.CreditAmt = amount;
                }
                else
                {
                    journalEntry.DebitAmt = -amount;
                }

                journalEntry.Memo = "(auto)";

                journalEntries.Add(journalEntry);
            }
        }

        if (account.AcctType is AccountType.Income or AccountType.Expense)
        {
            dataTable = DataCore.GetTable(
                "SELECT SUM(ROUND(CreditAmt,3))-SUM(ROUND(DebitAmt,3)) AS Amount, " +
                "YEAR(DateDisplayed) AS yearDis " +
                "FROM journalentry " +
                "WHERE AccountNum='" + account.AccountNum + "' " +
                "AND DateDisplayed < " + SOut.Date(to) + " " +
                "AND DateDisplayed >= " + SOut.Date(from) + " " +
                "GROUP BY yearDis");

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var year = SIn.Int(dataTable.Rows[i]["yearDis"].ToString());

                journalEntry = new JournalEntry
                {
                    CheckNumber = "",
                    Splits = "",
                    DateDisplayed = new DateTime(year, 12, 31)
                };

                var amount = SIn.Double(dataTable.Rows[i]["Amount"].ToString());
                if (amount > 0)
                {
                    journalEntry.DebitAmt = amount;
                }
                else
                {
                    journalEntry.CreditAmt = -amount;
                }

                journalEntry.Memo = "(auto)";

                journalEntries.Add(journalEntry);
            }
        }

        journalEntries.AddRange(JournalEntryCrud.SelectMany(
            "SELECT * FROM journalentry " +
            "WHERE AccountNum = '" + account.AccountNum + "' " +
            "AND DateDisplayed >= " + SOut.Date(dateFrom) + " " +
            "AND DateDisplayed <= " + SOut.Date(dateTo) + " " +
            "ORDER BY DateDisplayed"));

        return journalEntries
            .OrderBy(x => x.DateDisplayed)
            .ThenByDescending(x => x.AccountNum)
            .ToList();
    }

    public static List<JournalEntry> GetForReconcile(long accountNum, bool includeUncleared, long reconcileNum)
    {
        var commandText = "SELECT * FROM journalentry WHERE AccountNum = " + accountNum + " AND (ReconcileNum = " + reconcileNum;

        if (includeUncleared)
        {
            commandText += " OR ReconcileNum = 0)";
        }
        else
        {
            commandText += ")";
        }

        commandText += " ORDER BY DateDisplayed";

        return JournalEntryCrud.SelectMany(commandText);
    }

    public static void Insert(JournalEntry journalEntry)
    {
        journalEntry.SecUserNumEntry = Security.CurUser.UserNum;
        journalEntry.SecUserNumEdit = Security.CurUser.UserNum;

        if (journalEntry.DebitAmt < 0 || journalEntry.CreditAmt < 0)
        {
            throw new ApplicationException("Error. Credit and debit must both be positive.");
        }

        JournalEntryCrud.Insert(journalEntry);
    }

    public static void Update(JournalEntry journalEntry)
    {
        journalEntry.SecUserNumEdit = Security.CurUser.UserNum;

        if (journalEntry.DebitAmt < 0 || journalEntry.CreditAmt < 0)
        {
            throw new ApplicationException("Error. Credit and debit must both be positive.");
        }

        JournalEntryCrud.Update(journalEntry);
    }

    public static void Delete(JournalEntry journalEntry)
    {
        Db.NonQ("DELETE FROM journalentry WHERE JournalEntryNum = " + journalEntry.JournalEntryNum);
    }

    public static void UpdateList(List<JournalEntry> journalEntriesOld, List<JournalEntry> journalEntriesNew)
    {
        foreach (var journalEntry in journalEntriesNew)
        {
            if (journalEntry.DebitAmt < 0 || journalEntry.CreditAmt < 0)
            {
                throw new ApplicationException("Error. Credit and debit must both be positive.");
            }
        }

        foreach (var journalEntry in journalEntriesOld)
        {
            JournalEntry journalEntryNew = null;
            foreach (var newJournalEntry in journalEntriesNew)
            {
                if (newJournalEntry is null || newJournalEntry.JournalEntryNum == 0)
                {
                    continue;
                }
                
                if (journalEntry.JournalEntryNum != newJournalEntry.JournalEntryNum)
                {
                    continue;
                }

                journalEntryNew = newJournalEntry;
                break;
            }

            if (journalEntryNew is null)
            {
                Delete(journalEntry);

                continue;
            }

            if (journalEntryNew.AccountNum != journalEntry.AccountNum ||
                journalEntryNew.DateDisplayed != journalEntry.DateDisplayed ||
                journalEntryNew.DebitAmt != journalEntry.DebitAmt ||
                journalEntryNew.CreditAmt != journalEntry.CreditAmt ||
                journalEntryNew.Memo != journalEntry.Memo ||
                journalEntryNew.Splits != journalEntry.Splits ||
                journalEntryNew.CheckNumber != journalEntry.CheckNumber)
            {
                Update(journalEntryNew);
            }
        }

        foreach (var journalEntry in journalEntriesNew)
        {
            if (journalEntry is not {JournalEntryNum: 0})
            {
                continue;
            }

            Insert(journalEntry);
        }
    }

    public static bool AttachedToReconcile(List<JournalEntry> journalEntries)
    {
        foreach (var journalEntry in journalEntries)
        {
            if (journalEntry.ReconcileNum != 0)
            {
                return true;
            }
        }

        return false;
    }

    public static DateTime GetReconcileDate(List<JournalEntry> journalEntries)
    {
        foreach (var journalEntry in journalEntries)
        {
            if (journalEntry.ReconcileNum != 0)
            {
                return Reconciles.GetOne(journalEntry.ReconcileNum).DateReconcile;
            }
        }

        return DateTime.MinValue;
    }

    public static void SaveList(List<JournalEntry> journalEntries, long reconcileNum)
    {
        var commandText = "UPDATE journalentry SET ReconcileNum = 0 WHERE";

        var str = "";
        foreach (var journalEntry in journalEntries)
        {
            if (journalEntry.ReconcileNum != 0)
            {
                continue;
            }

            if (str != "")
            {
                str += " OR";
            }

            str += " JournalEntryNum=" + journalEntry.JournalEntryNum;
        }

        if (str != "")
        {
            commandText += str;

            Db.NonQ(commandText);
        }

        commandText = "UPDATE journalentry SET ReconcileNum = " + reconcileNum + " WHERE";

        str = "";
        foreach (var journalEntry in journalEntries)
        {
            if (journalEntry.ReconcileNum != reconcileNum)
            {
                continue;
            }

            if (str != "")
            {
                str += " OR";
            }

            str += " JournalEntryNum=" + journalEntry.JournalEntryNum;
        }

        if (str == "")
        {
            return;
        }

        commandText += str;

        Db.NonQ(commandText);
    }

    public static bool IsInUse(long accountNum)
    {
        var count = Db.GetCount("SELECT COUNT(*) FROM journalentry WHERE AccountNum = " + accountNum);

        return count != "0";
    }
}