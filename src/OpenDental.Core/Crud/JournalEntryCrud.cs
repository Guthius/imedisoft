#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class JournalEntryCrud
{
    public static JournalEntry SelectOne(long journalEntryNum)
    {
        var command = "SELECT * FROM journalentry "
                      + "WHERE JournalEntryNum = " + SOut.Long(journalEntryNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static JournalEntry SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<JournalEntry> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<JournalEntry> TableToList(DataTable table)
    {
        var retVal = new List<JournalEntry>();
        JournalEntry journalEntry;
        foreach (DataRow row in table.Rows)
        {
            journalEntry = new JournalEntry();
            journalEntry.JournalEntryNum = SIn.Long(row["JournalEntryNum"].ToString());
            journalEntry.TransactionNum = SIn.Long(row["TransactionNum"].ToString());
            journalEntry.AccountNum = SIn.Long(row["AccountNum"].ToString());
            journalEntry.DateDisplayed = SIn.Date(row["DateDisplayed"].ToString());
            journalEntry.DebitAmt = SIn.Double(row["DebitAmt"].ToString());
            journalEntry.CreditAmt = SIn.Double(row["CreditAmt"].ToString());
            journalEntry.Memo = SIn.String(row["Memo"].ToString());
            journalEntry.Splits = SIn.String(row["Splits"].ToString());
            journalEntry.CheckNumber = SIn.String(row["CheckNumber"].ToString());
            journalEntry.ReconcileNum = SIn.Long(row["ReconcileNum"].ToString());
            journalEntry.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            journalEntry.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            journalEntry.SecUserNumEdit = SIn.Long(row["SecUserNumEdit"].ToString());
            journalEntry.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(journalEntry);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<JournalEntry> listJournalEntrys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "JournalEntry";
        var table = new DataTable(tableName);
        table.Columns.Add("JournalEntryNum");
        table.Columns.Add("TransactionNum");
        table.Columns.Add("AccountNum");
        table.Columns.Add("DateDisplayed");
        table.Columns.Add("DebitAmt");
        table.Columns.Add("CreditAmt");
        table.Columns.Add("Memo");
        table.Columns.Add("Splits");
        table.Columns.Add("CheckNumber");
        table.Columns.Add("ReconcileNum");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecUserNumEdit");
        table.Columns.Add("SecDateTEdit");
        foreach (var journalEntry in listJournalEntrys)
            table.Rows.Add(SOut.Long(journalEntry.JournalEntryNum), SOut.Long(journalEntry.TransactionNum), SOut.Long(journalEntry.AccountNum), SOut.DateT(journalEntry.DateDisplayed, false), SOut.Double(journalEntry.DebitAmt), SOut.Double(journalEntry.CreditAmt), journalEntry.Memo, journalEntry.Splits, journalEntry.CheckNumber, SOut.Long(journalEntry.ReconcileNum), SOut.Long(journalEntry.SecUserNumEntry), SOut.DateT(journalEntry.SecDateTEntry, false), SOut.Long(journalEntry.SecUserNumEdit), SOut.DateT(journalEntry.SecDateTEdit, false));
        return table;
    }

    public static long Insert(JournalEntry journalEntry)
    {
        return Insert(journalEntry, false);
    }

    public static long Insert(JournalEntry journalEntry, bool useExistingPK)
    {
        var command = "INSERT INTO journalentry (";

        command += "TransactionNum,AccountNum,DateDisplayed,DebitAmt,CreditAmt,Memo,Splits,CheckNumber,ReconcileNum,SecUserNumEntry,SecDateTEntry,SecUserNumEdit) VALUES(";

        command +=
            SOut.Long(journalEntry.TransactionNum) + ","
                                                   + SOut.Long(journalEntry.AccountNum) + ","
                                                   + SOut.Date(journalEntry.DateDisplayed) + ","
                                                   + SOut.Double(journalEntry.DebitAmt) + ","
                                                   + SOut.Double(journalEntry.CreditAmt) + ","
                                                   + DbHelper.ParamChar + "paramMemo,"
                                                   + DbHelper.ParamChar + "paramSplits,"
                                                   + "'" + SOut.String(journalEntry.CheckNumber) + "',"
                                                   + SOut.Long(journalEntry.ReconcileNum) + ","
                                                   + SOut.Long(journalEntry.SecUserNumEntry) + ","
                                                   + DbHelper.Now() + ","
                                                   + SOut.Long(journalEntry.SecUserNumEdit) + ")";
        //SecDateTEdit can only be set by MySQL
        if (journalEntry.Memo == null) journalEntry.Memo = "";
        var paramMemo = new OdSqlParameter("paramMemo", OdDbType.Text, SOut.StringParam(journalEntry.Memo));
        if (journalEntry.Splits == null) journalEntry.Splits = "";
        var paramSplits = new OdSqlParameter("paramSplits", OdDbType.Text, SOut.StringParam(journalEntry.Splits));
        {
            journalEntry.JournalEntryNum = Db.NonQ(command, true, "JournalEntryNum", "journalEntry", paramMemo, paramSplits);
        }
        return journalEntry.JournalEntryNum;
    }

    public static long InsertNoCache(JournalEntry journalEntry)
    {
        return InsertNoCache(journalEntry, false);
    }

    public static long InsertNoCache(JournalEntry journalEntry, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO journalentry (";
        if (isRandomKeys || useExistingPK) command += "JournalEntryNum,";
        command += "TransactionNum,AccountNum,DateDisplayed,DebitAmt,CreditAmt,Memo,Splits,CheckNumber,ReconcileNum,SecUserNumEntry,SecDateTEntry,SecUserNumEdit) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(journalEntry.JournalEntryNum) + ",";
        command +=
            SOut.Long(journalEntry.TransactionNum) + ","
                                                   + SOut.Long(journalEntry.AccountNum) + ","
                                                   + SOut.Date(journalEntry.DateDisplayed) + ","
                                                   + SOut.Double(journalEntry.DebitAmt) + ","
                                                   + SOut.Double(journalEntry.CreditAmt) + ","
                                                   + DbHelper.ParamChar + "paramMemo,"
                                                   + DbHelper.ParamChar + "paramSplits,"
                                                   + "'" + SOut.String(journalEntry.CheckNumber) + "',"
                                                   + SOut.Long(journalEntry.ReconcileNum) + ","
                                                   + SOut.Long(journalEntry.SecUserNumEntry) + ","
                                                   + DbHelper.Now() + ","
                                                   + SOut.Long(journalEntry.SecUserNumEdit) + ")";
        //SecDateTEdit can only be set by MySQL
        if (journalEntry.Memo == null) journalEntry.Memo = "";
        var paramMemo = new OdSqlParameter("paramMemo", OdDbType.Text, SOut.StringParam(journalEntry.Memo));
        if (journalEntry.Splits == null) journalEntry.Splits = "";
        var paramSplits = new OdSqlParameter("paramSplits", OdDbType.Text, SOut.StringParam(journalEntry.Splits));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMemo, paramSplits);
        else
            journalEntry.JournalEntryNum = Db.NonQ(command, true, "JournalEntryNum", "journalEntry", paramMemo, paramSplits);
        return journalEntry.JournalEntryNum;
    }

    public static void Update(JournalEntry journalEntry)
    {
        var command = "UPDATE journalentry SET "
                      + "TransactionNum =  " + SOut.Long(journalEntry.TransactionNum) + ", "
                      + "AccountNum     =  " + SOut.Long(journalEntry.AccountNum) + ", "
                      + "DateDisplayed  =  " + SOut.Date(journalEntry.DateDisplayed) + ", "
                      + "DebitAmt       =  " + SOut.Double(journalEntry.DebitAmt) + ", "
                      + "CreditAmt      =  " + SOut.Double(journalEntry.CreditAmt) + ", "
                      + "Memo           =  " + DbHelper.ParamChar + "paramMemo, "
                      + "Splits         =  " + DbHelper.ParamChar + "paramSplits, "
                      + "CheckNumber    = '" + SOut.String(journalEntry.CheckNumber) + "', "
                      + "ReconcileNum   =  " + SOut.Long(journalEntry.ReconcileNum) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateTEntry not allowed to change
                      + "SecUserNumEdit =  " + SOut.Long(journalEntry.SecUserNumEdit) + " "
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE JournalEntryNum = " + SOut.Long(journalEntry.JournalEntryNum);
        if (journalEntry.Memo == null) journalEntry.Memo = "";
        var paramMemo = new OdSqlParameter("paramMemo", OdDbType.Text, SOut.StringParam(journalEntry.Memo));
        if (journalEntry.Splits == null) journalEntry.Splits = "";
        var paramSplits = new OdSqlParameter("paramSplits", OdDbType.Text, SOut.StringParam(journalEntry.Splits));
        Db.NonQ(command, paramMemo, paramSplits);
    }

    public static bool Update(JournalEntry journalEntry, JournalEntry oldJournalEntry)
    {
        var command = "";
        if (journalEntry.TransactionNum != oldJournalEntry.TransactionNum)
        {
            if (command != "") command += ",";
            command += "TransactionNum = " + SOut.Long(journalEntry.TransactionNum) + "";
        }

        if (journalEntry.AccountNum != oldJournalEntry.AccountNum)
        {
            if (command != "") command += ",";
            command += "AccountNum = " + SOut.Long(journalEntry.AccountNum) + "";
        }

        if (journalEntry.DateDisplayed.Date != oldJournalEntry.DateDisplayed.Date)
        {
            if (command != "") command += ",";
            command += "DateDisplayed = " + SOut.Date(journalEntry.DateDisplayed) + "";
        }

        if (journalEntry.DebitAmt != oldJournalEntry.DebitAmt)
        {
            if (command != "") command += ",";
            command += "DebitAmt = " + SOut.Double(journalEntry.DebitAmt) + "";
        }

        if (journalEntry.CreditAmt != oldJournalEntry.CreditAmt)
        {
            if (command != "") command += ",";
            command += "CreditAmt = " + SOut.Double(journalEntry.CreditAmt) + "";
        }

        if (journalEntry.Memo != oldJournalEntry.Memo)
        {
            if (command != "") command += ",";
            command += "Memo = " + DbHelper.ParamChar + "paramMemo";
        }

        if (journalEntry.Splits != oldJournalEntry.Splits)
        {
            if (command != "") command += ",";
            command += "Splits = " + DbHelper.ParamChar + "paramSplits";
        }

        if (journalEntry.CheckNumber != oldJournalEntry.CheckNumber)
        {
            if (command != "") command += ",";
            command += "CheckNumber = '" + SOut.String(journalEntry.CheckNumber) + "'";
        }

        if (journalEntry.ReconcileNum != oldJournalEntry.ReconcileNum)
        {
            if (command != "") command += ",";
            command += "ReconcileNum = " + SOut.Long(journalEntry.ReconcileNum) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateTEntry not allowed to change
        if (journalEntry.SecUserNumEdit != oldJournalEntry.SecUserNumEdit)
        {
            if (command != "") command += ",";
            command += "SecUserNumEdit = " + SOut.Long(journalEntry.SecUserNumEdit) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        if (journalEntry.Memo == null) journalEntry.Memo = "";
        var paramMemo = new OdSqlParameter("paramMemo", OdDbType.Text, SOut.StringParam(journalEntry.Memo));
        if (journalEntry.Splits == null) journalEntry.Splits = "";
        var paramSplits = new OdSqlParameter("paramSplits", OdDbType.Text, SOut.StringParam(journalEntry.Splits));
        command = "UPDATE journalentry SET " + command
                                             + " WHERE JournalEntryNum = " + SOut.Long(journalEntry.JournalEntryNum);
        Db.NonQ(command, paramMemo, paramSplits);
        return true;
    }

    public static bool UpdateComparison(JournalEntry journalEntry, JournalEntry oldJournalEntry)
    {
        if (journalEntry.TransactionNum != oldJournalEntry.TransactionNum) return true;
        if (journalEntry.AccountNum != oldJournalEntry.AccountNum) return true;
        if (journalEntry.DateDisplayed.Date != oldJournalEntry.DateDisplayed.Date) return true;
        if (journalEntry.DebitAmt != oldJournalEntry.DebitAmt) return true;
        if (journalEntry.CreditAmt != oldJournalEntry.CreditAmt) return true;
        if (journalEntry.Memo != oldJournalEntry.Memo) return true;
        if (journalEntry.Splits != oldJournalEntry.Splits) return true;
        if (journalEntry.CheckNumber != oldJournalEntry.CheckNumber) return true;
        if (journalEntry.ReconcileNum != oldJournalEntry.ReconcileNum) return true;
        //SecUserNumEntry excluded from update
        //SecDateTEntry not allowed to change
        if (journalEntry.SecUserNumEdit != oldJournalEntry.SecUserNumEdit) return true;
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long journalEntryNum)
    {
        var command = "DELETE FROM journalentry "
                      + "WHERE JournalEntryNum = " + SOut.Long(journalEntryNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listJournalEntryNums)
    {
        if (listJournalEntryNums == null || listJournalEntryNums.Count == 0) return;
        var command = "DELETE FROM journalentry "
                      + "WHERE JournalEntryNum IN(" + string.Join(",", listJournalEntryNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}