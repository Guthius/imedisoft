using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Accounts
{
    public static void Insert(Account account)
    {
        AccountCrud.Insert(account);
    }

    public static void Update(Account account, Account accountOld)
    {
        AccountCrud.Update(account, accountOld);
        if (account.Description == accountOld.Description) return; //No need to update splits on attached journal entries.
        //The account was renamed, so update journalentry.Splits.
        var command = @"SELECT je2.*,account.Description
					FROM journalentry 
					INNER JOIN journalentry je2 ON je2.TransactionNum=journalentry.TransactionNum
					INNER JOIN account ON account.AccountNum=je2.AccountNum
					WHERE journalentry.AccountNum=" + SOut.Long(account.AccountNum) + @"
					AND journalentry.DateDisplayed > " + SOut.Date(PrefC.GetDate(PrefName.AccountingLockDate)) + @"
					ORDER BY je2.TransactionNum"; //to group them
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return;
        var listJournalEntries = JournalEntryCrud.TableToList(table);
        for (var i = 0; i < listJournalEntries.Count; i++) listJournalEntries[i].DescriptionAccount = table.Rows[i]["Description"].ToString();
        //Now we will loop through all the journal entries and find the other journal entries that are attached to the same transaction and update
        //those splits.
        var listTransactionNums = listJournalEntries.Select(x => x.TransactionNum).Distinct().ToList();
        for (var t = 0; t < listTransactionNums.Count; t++)
        {
            var listJournalEntriesForTrans = listJournalEntries.FindAll(x => x.TransactionNum == listTransactionNums[t]);
            for (var j = 0; j < listJournalEntriesForTrans.Count; j++)
            {
                if (listJournalEntriesForTrans[j].AccountNum == account.AccountNum) continue;
                if (listJournalEntriesForTrans.Count == 2)
                {
                    //When a transaction only has two splits, the Splits column will simply be the name of the account of the other split.
                    listJournalEntriesForTrans[j].Splits = account.Description;
                    JournalEntries.Update(listJournalEntriesForTrans[j]);
                    continue;
                }

                //When a transaction has three or more splits, the Splits column will be the names of the account and the amount of the other splits.
                //Ex.: 
                //Patient Fee Income 85.00
                //Supplies 110.00
                var updatedSplits = "";
                for (var k = 0; k < listJournalEntriesForTrans.Count; k++)
                {
                    if (listJournalEntriesForTrans[k].JournalEntryNum == listJournalEntriesForTrans[j].JournalEntryNum)
                        //skipping self because we only want the other 2+ in the group
                        continue;
                    var splitAmt = listJournalEntriesForTrans[k].CreditAmt.ToString("n");
                    if (listJournalEntriesForTrans[k].DebitAmt > 0) splitAmt = listJournalEntriesForTrans[k].DebitAmt.ToString("n");
                    updatedSplits += listJournalEntriesForTrans[k].DescriptionAccount + " ";
                    updatedSplits += splitAmt;
                    updatedSplits += "\r\n";
                }

                listJournalEntriesForTrans[j].Splits = updatedSplits;
                JournalEntries.Update(listJournalEntriesForTrans[j]);
            } //for j
        } //for t
    }

    public static string GetDescript(long accountNum)
    {
        var account = GetFirstOrDefault(x => x.AccountNum == accountNum);
        return account == null ? "" : account.Description;
    }

    public static Account GetAccount(long accountNum)
    {
        return GetFirstOrDefault(x => x.AccountNum == accountNum);
    }

    public static void Delete(Account account)
    {
        //check to see if account has any journal entries
        var command = "SELECT COUNT(*) FROM journalentry WHERE AccountNum=" + SOut.Long(account.AccountNum);
        if (Db.GetCount(command) != "0")
            throw new ApplicationException(Lans.g("FormAccountEdit",
                "Not allowed to delete an account with existing journal entries."));
        //Check various preference entries
        command = "SELECT ValueString FROM preference WHERE PrefName='AccountingDepositAccounts'";
        var result = Db.GetCount(command);
        var stringArray = result.Split(',');
        for (var i = 0; i < stringArray.Length; i++)
            if (stringArray[i] == account.AccountNum.ToString())
                throw new ApplicationException(Lans.g("FormAccountEdit", "Account is in use in the setup section."));

        command = "SELECT ValueString FROM preference WHERE PrefName='AccountingIncomeAccount'";
        result = Db.GetCount(command);
        if (result == account.AccountNum.ToString()) throw new ApplicationException(Lans.g("FormAccountEdit", "Account is in use in the setup section."));
        command = "SELECT ValueString FROM preference WHERE PrefName='AccountingCashIncomeAccount'";
        result = Db.GetCount(command);
        if (result == account.AccountNum.ToString()) throw new ApplicationException(Lans.g("FormAccountEdit", "Account is in use in the setup section."));
        //check AccountingAutoPay entries
        var listAccountingAutoPays = AccountingAutoPays.GetDeepCopy();
        for (var i = 0; i < listAccountingAutoPays.Count; i++)
        {
            stringArray = listAccountingAutoPays[i].PickList.Split(',');
            for (var s = 0; s < stringArray.Length; s++)
                if (stringArray[s] == account.AccountNum.ToString())
                    throw new ApplicationException(Lans.g("FormAccountEdit", "Account is in use in the setup section."));
        }

        command = "DELETE FROM account WHERE AccountNum = " + SOut.Long(account.AccountNum);
        Db.NonQ(command);
    }

    public static bool DebitIsPos(AccountType accountType)
    {
        switch (accountType)
        {
            case AccountType.Asset:
            case AccountType.Expense:
                return true;
            case AccountType.Liability:
            case AccountType.Equity: //because liabilities and equity are treated the same
            case AccountType.Income:
                return false;
        }

        return true; //will never happen
    }

    public static double GetBalance(long accountNum, AccountType accountType)
    {
        var command = "SELECT SUM(DebitAmt),SUM(CreditAmt) FROM journalentry "
                      + "WHERE AccountNum=" + SOut.Long(accountNum)
                      + " GROUP BY AccountNum";
        var table = DataCore.GetTable(command);
        double debit = 0;
        double credit = 0;
        if (table.Rows.Count > 0)
        {
            debit = SIn.Double(table.Rows[0][0].ToString());
            credit = SIn.Double(table.Rows[0][1].ToString());
        }

        if (DebitIsPos(accountType)) return debit - credit;

        return credit - debit;
        /*}
        catch {
            Debug.WriteLine(command);
            MessageBox.Show(command);
        }
        return 0;*/
    }

    public static bool DepositsLinked()
    {
        var prefAccountingSoftware = PrefC.GetInt(PrefName.AccountingSoftware);
        if (prefAccountingSoftware == (int) AccountingSoftware.QuickBooks)
        {
            if (PrefC.GetString(PrefName.QuickBooksDepositAccounts) == "") return false;
            if (PrefC.GetString(PrefName.QuickBooksIncomeAccount) == "") return false;
        }
        else if (prefAccountingSoftware == (int) AccountingSoftware.QuickBooksOnline)
        {
            var programQbo = Programs.GetCur(ProgramName.QuickBooksOnline);
            var programPropertyQboDepositAccounts = ProgramProperties.GetPropForProgByDesc(programQbo.ProgramNum, "Deposit Accounts");
            if (programPropertyQboDepositAccounts.PropertyValue == "") return false;
            var programPropertyQboIncomeAccounts = ProgramProperties.GetPropForProgByDesc(programQbo.ProgramNum, "Income Accounts");
            if (programPropertyQboIncomeAccounts.PropertyValue == "") return false;
        }
        else
        {
            if (PrefC.GetString(PrefName.AccountingDepositAccounts) == "") return false;
            if (PrefC.GetLong(PrefName.AccountingIncomeAccount) == 0) return false;
        }

        //might add a few more checks later.
        return true;
    }

    public static bool PaymentsLinked()
    {
        if (AccountingAutoPays.GetCount() == 0) return false;
        if (PrefC.GetLong(PrefName.AccountingCashIncomeAccount) == 0) return false;
        //might add a few more checks later.
        return true;
    }

    public static List<long> GetDepositAccounts()
    {
        var depStr = PrefC.GetString(PrefName.AccountingDepositAccounts);
        var listStrDepositAccounts = depStr.Split(',').ToList();
        var listDepositAccounts = new List<long>();
        for (var i = 0; i < listStrDepositAccounts.Count; i++)
        {
            if (listStrDepositAccounts[i] == "") continue;
            listDepositAccounts.Add(SIn.Long(listStrDepositAccounts[i]));
        }

        return listDepositAccounts;
    }

    public static List<string> GetDepositAccountsQB()
    {
        var depStr = PrefC.GetString(PrefName.QuickBooksDepositAccounts);
        var stringArrayDep = depStr.Split(',');
        var listStrings = new List<string>();
        for (var i = 0; i < stringArrayDep.Length; i++)
        {
            if (stringArrayDep[i] == "") continue;
            listStrings.Add(stringArrayDep[i]);
        }

        return listStrings;
    }

    public static List<string> GetIncomeAccountsQB()
    {
        var incomeStr = PrefC.GetString(PrefName.QuickBooksIncomeAccount);
        var stringArrayIncome = incomeStr.Split(',');
        var listStrings = new List<string>();
        for (var i = 0; i < stringArrayIncome.Length; i++)
        {
            if (stringArrayIncome[i] == "") continue;
            listStrings.Add(stringArrayIncome[i]);
        }

        return listStrings;
    }

    public static decimal GetRE_PreviousYears(DateTime dateAsOf)
    {
        var dateFirstofYear = new DateTime(dateAsOf.Year, 1, 1);
        //this works for both income and expenses, because we are subracting expenses, so signs cancel
        var command = "SELECT SUM(CreditAmt-DebitAmt) "
                      + "FROM account,journalentry "
                      + "WHERE journalentry.AccountNum=account.AccountNum "
                      + "AND DateDisplayed < " + SOut.Date(dateFirstofYear) //all from previous years
                      + " AND (AcctType=" + (int) AccountType.Income + " OR AcctType=" + (int) AccountType.Expense + ")";
        var strBal = Db.GetCount(command);
        var amtBalanceRE = SIn.Decimal(strBal);
        return amtBalanceRE;
    }

    public static DataTable GetFullList(DateTime dateAsOf, bool showInactive)
    {
        var table = new DataTable("Accounts");
        DataRow row;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("type");
        table.Columns.Add("Description");
        table.Columns.Add("balance");
        table.Columns.Add("BankNumber");
        table.Columns.Add("inactive");
        table.Columns.Add("color");
        table.Columns.Add("AccountNum");
        //but we won't actually fill this table with rows until the very end.  It's more useful to use a List<> for now.
        var listRows = new List<DataRow>();
        var dateFirstofYear = new DateTime(dateAsOf.Year, 1, 1);
        //First, get the retained earnings balance
        var amtBalanceRE = GetRE_PreviousYears(dateAsOf);
        //Next, the entire history for the asset, liability, and equity accounts, including Retained Earnings-----------
        var command = "SELECT account.AcctType, account.Description, account.AccountNum,account.IsRetainedEarnings, "
                      + "SUM(DebitAmt) AS SumDebit, SUM(CreditAmt) AS SumCredit, account.BankNumber, account.Inactive, account.AccountColor "
                      + "FROM account "
                      + "LEFT JOIN journalentry ON journalentry.AccountNum=account.AccountNum AND "
                      + "DateDisplayed <= " + SOut.Date(dateAsOf)
                      + " WHERE AcctType<=2 ";
        if (!showInactive) command += "AND Inactive=0 ";
        command += "GROUP BY account.AccountNum, account.AcctType, account.Description, account.BankNumber,"
                   + "account.Inactive, account.AccountColor ORDER BY AcctType, Description";
        var tableRaw = DataCore.GetTable(command);
        AccountType accountType;
        decimal debit;
        decimal credit;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            row = table.NewRow();
            accountType = (AccountType) SIn.Long(tableRaw.Rows[i]["AcctType"].ToString());
            var isRetainedEarnings = SIn.Bool(tableRaw.Rows[i]["IsRetainedEarnings"].ToString());
            row["type"] = Lans.g("enumAccountType", accountType.ToString());
            row["Description"] = tableRaw.Rows[i]["Description"].ToString();
            debit = SIn.Decimal(tableRaw.Rows[i]["SumDebit"].ToString());
            credit = SIn.Decimal(tableRaw.Rows[i]["SumCredit"].ToString());
            if (isRetainedEarnings)
                row["balance"] = (credit - debit + amtBalanceRE).ToString("N");
            else if (DebitIsPos(accountType))
                row["balance"] = (debit - credit).ToString("N");
            else
                row["balance"] = (credit - debit).ToString("N");
            row["BankNumber"] = tableRaw.Rows[i]["BankNumber"].ToString();
            if (tableRaw.Rows[i]["Inactive"].ToString() == "0")
                row["inactive"] = "";
            else
                row["inactive"] = "X";
            row["color"] = tableRaw.Rows[i]["AccountColor"].ToString(); //it will be an unsigned int at this point.
            row["AccountNum"] = tableRaw.Rows[i]["AccountNum"].ToString();
            listRows.Add(row);
        }

        //finally, income and expenses------------------------------------------------------------------------------
        command = "SELECT account.AcctType, account.Description, account.AccountNum, "
                  + "SUM(DebitAmt) AS SumDebit, SUM(CreditAmt) AS SumCredit, account.BankNumber, account.Inactive, account.AccountColor "
                  + "FROM account "
                  + "LEFT JOIN journalentry ON journalentry.AccountNum=account.AccountNum "
                  + "AND DateDisplayed <= " + SOut.Date(dateAsOf)
                  + " AND DateDisplayed >= " + SOut.Date(dateFirstofYear) //only for this year
                  + " WHERE (AcctType=3 OR AcctType=4) ";
        if (!showInactive) command += "AND Inactive=0 ";
        command += "GROUP BY account.AccountNum, account.AcctType, account.Description, account.BankNumber,"
                   + "account.Inactive, account.AccountColor ORDER BY AcctType, Description";
        tableRaw = DataCore.GetTable(command);
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            row = table.NewRow();
            accountType = (AccountType) SIn.Long(tableRaw.Rows[i]["AcctType"].ToString());
            row["type"] = Lans.g("enumAccountType", accountType.ToString());
            row["Description"] = tableRaw.Rows[i]["Description"].ToString();
            debit = SIn.Decimal(tableRaw.Rows[i]["SumDebit"].ToString());
            credit = SIn.Decimal(tableRaw.Rows[i]["SumCredit"].ToString());
            if (DebitIsPos(accountType))
                row["balance"] = (debit - credit).ToString("N");
            else
                row["balance"] = (credit - debit).ToString("N");
            row["BankNumber"] = tableRaw.Rows[i]["BankNumber"].ToString();
            if (tableRaw.Rows[i]["Inactive"].ToString() == "0")
                row["inactive"] = "";
            else
                row["inactive"] = "X";
            row["color"] = tableRaw.Rows[i]["AccountColor"].ToString(); //it will be an unsigned int at this point.
            row["AccountNum"] = tableRaw.Rows[i]["AccountNum"].ToString();
            listRows.Add(row);
        }

        for (var i = 0; i < listRows.Count; i++) table.Rows.Add(listRows[i]);
        return table;
    }

    public static DataTable GetGeneralLedger(DateTime dateStart, DateTime dateEnd)
    {
        var queryString = @"SELECT DATE(" + SOut.Date(new DateTime(dateStart.Year - 1, 12, 31)) + @") DateDisplayed,
				'' Memo,
				'' Splits,
				'' CheckNumber,
				startingbals.SumTotal DebitAmt,
				0 CreditAmt,
				'' Balance,
				startingbals.Description,
				startingbals.AcctType,
				startingbals.AccountNum
				FROM (
					SELECT account.AccountNum,
					account.Description,
					account.AcctType,
					ROUND(SUM(journalentry.DebitAmt-journalentry.CreditAmt),2) SumTotal
					FROM account
					INNER JOIN journalentry ON journalentry.AccountNum=account.AccountNum
					AND journalentry.DateDisplayed < " + SOut.Date(dateStart) + @" 
					AND account.AcctType IN (0,1,2)/*assets,liablities,equity*/
					GROUP BY account.AccountNum
				) startingbals

				UNION ALL
	
				SELECT journalentry.DateDisplayed,
				journalentry.Memo,
				journalentry.Splits,
				journalentry.CheckNumber,
				journalentry.DebitAmt, 
				journalentry.CreditAmt,
				'' Balance,
				account.Description,
				account.AcctType,
				account.AccountNum 
				FROM account
				LEFT JOIN journalentry ON account.AccountNum=journalentry.AccountNum 
					AND journalentry.DateDisplayed >= " + SOut.Date(dateStart) + @" 
					AND journalentry.DateDisplayed <= " + SOut.Date(dateEnd) + @" 
				
				ORDER BY AcctType, Description, DateDisplayed;";
        return DataCore.GetTable(queryString);
    }

    public static DataTable GetAccountTotalByType(DateTime dateAsOf, AccountType accountType)
    {
        var command = "SELECT Description, ";
        if (accountType == AccountType.Asset)
            command += "SUM(ROUND(DebitAmt,3)-ROUND(CreditAmt,3))";
        else //Liability or equity
            command += "SUM(ROUND(CreditAmt,3)-ROUND(DebitAmt,3))";
        command += " AS SumTotal, AcctType, IsRetainedEarnings,Inactive " //Inactive won't show
                   + "FROM account, journalentry "
                   + "WHERE account.AccountNum=journalentry.AccountNum "
                   + "AND DateDisplayed <= " + SOut.Date(dateAsOf) + " "
                   + "AND AcctType=" + SOut.Int((int) accountType) + " "
                   + "GROUP BY account.AccountNum "
                   + "HAVING (SumTotal<>0 OR Inactive=0) " //either a bal or active
                   + "ORDER BY Description, DateDisplayed ";
        var table = DataCore.GetTable(command);
        if (accountType != AccountType.Equity) return table;
        //For equity, get the RE balance from all previous years
        decimal balanceRE = 0;
        if (accountType == AccountType.Equity) balanceRE = GetRE_PreviousYears(dateAsOf);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var isRE = SIn.Bool(table.Rows[i]["IsRetainedEarnings"].ToString());
            if (!isRE) continue; //only one of the equity accounts is the RE
            var amt = SIn.Decimal(table.Rows[i]["SumTotal"].ToString())
                      + balanceRE;
            table.Rows[i]["SumTotal"] = amt;
            break;
        }

        return table;
    }

    public static DataTable GetAccountTotalByType(DateTime dateStart, DateTime dateEnd, AccountType accountType)
    {
        var strSumTotal
            = "";
        if (accountType == AccountType.Expense)
            strSumTotal = "SUM(ROUND(DebitAmt,3)-ROUND(CreditAmt,3))";
        else //Income instead of expense
            strSumTotal = "SUM(ROUND(CreditAmt,3)-ROUND(DebitAmt,3))";
        var command = "SELECT Description, " + strSumTotal + " SumTotal, AcctType "
                      + "FROM account, journalentry "
                      + "WHERE account.AccountNum=journalentry.AccountNum AND DateDisplayed >= " + SOut.Date(dateStart) + " "
                      + "AND DateDisplayed <= " + SOut.Date(dateEnd) + " "
                      + "AND AcctType=" + SOut.Int((int) accountType) + " "
                      + "GROUP BY account.AccountNum "
                      + "ORDER BY Description, DateDisplayed ";
        return DataCore.GetTable(command);
    }

    public static double NetIncomeThisYear(DateTime dateAsOf)
    {
        var dateFirstOfYear = new DateTime(dateAsOf.Year, 1, 1);
        var command = "SELECT SUM(ROUND(CreditAmt,3)), SUM(ROUND(DebitAmt,3)), AcctType "
                      + "FROM journalentry,account "
                      + "WHERE journalentry.AccountNum=account.AccountNum "
                      + "AND DateDisplayed >= " + SOut.Date(dateFirstOfYear)
                      + " AND DateDisplayed <= " + SOut.Date(dateAsOf)
                      + " GROUP BY AcctType";
        var table = DataCore.GetTable(command);
        double retVal = 0;
        for (var i = 0; i < table.Rows.Count; i++)
            if (table.Rows[i][2].ToString() == "3" //income
                || table.Rows[i][2].ToString() == "4") //expense
            {
                retVal += SIn.Double(table.Rows[i][0].ToString()); //add credit
                retVal -= SIn.Double(table.Rows[i][1].ToString()); //subtract debit
                //if it's an expense, we are subtracting (income-expense), but the signs cancel.
            }

        return retVal;
    }

    private class AccountCache : CacheListAbs<Account>
    {
        protected override Account Copy(Account item)
        {
            return item.Clone();
        }

        protected override void FillCacheIfNeeded()
        {
            Accounts.GetTableFromCache(false);
        }

        protected override List<Account> GetCacheFromDb()
        {
            var command = "SELECT * FROM account ORDER BY AcctType,Description";
            return AccountCrud.SelectMany(command);
        }

        protected override DataTable ToDataTable(List<Account> items)
        {
            return AccountCrud.ListToTable(items, "Account");
        }

        protected override List<Account> TableToList(DataTable dataTable)
        {
            return AccountCrud.TableToList(dataTable);
        }

        protected override bool IsInListShort(Account item)
        {
            return !item.Inactive;
        }
    }

    private static readonly AccountCache Cache = new();

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static List<Account> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Account GetFirstOrDefault(Func<Account, bool> funcMatch, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(funcMatch, isShort);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }
}