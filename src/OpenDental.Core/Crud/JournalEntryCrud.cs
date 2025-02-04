using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class JournalEntryCrud
{
    public static List<JournalEntry> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<JournalEntry> TableToList(DataTable table)
    {
        var retVal = new List<JournalEntry>();
        foreach (DataRow row in table.Rows)
        {
            var journalEntry = new JournalEntry
            {
                JournalEntryNum = SIn.Long(row["JournalEntryNum"].ToString()),
                TransactionNum = SIn.Long(row["TransactionNum"].ToString()),
                AccountNum = SIn.Long(row["AccountNum"].ToString()),
                DateDisplayed = SIn.Date(row["DateDisplayed"].ToString()),
                DebitAmt = SIn.Double(row["DebitAmt"].ToString()),
                CreditAmt = SIn.Double(row["CreditAmt"].ToString()),
                Memo = SIn.String(row["Memo"].ToString()),
                Splits = SIn.String(row["Splits"].ToString()),
                CheckNumber = SIn.String(row["CheckNumber"].ToString()),
                ReconcileNum = SIn.Long(row["ReconcileNum"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecUserNumEdit = SIn.Long(row["SecUserNumEdit"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(journalEntry);
        }

        return retVal;
    }

    public static void Insert(JournalEntry journalEntry)
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
                                                   + "NOW()" + ","
                                                   + SOut.Long(journalEntry.SecUserNumEdit) + ")";
        //SecDateTEdit can only be set by MySQL
        if (journalEntry.Memo == null) journalEntry.Memo = "";
        var paramMemo = new OdSqlParameter("paramMemo", SOut.StringParam(journalEntry.Memo));
        if (journalEntry.Splits == null) journalEntry.Splits = "";
        var paramSplits = new OdSqlParameter("paramSplits", SOut.StringParam(journalEntry.Splits));
        {
            journalEntry.JournalEntryNum = Db.NonQ(command, true, "JournalEntryNum", "journalEntry", paramMemo, paramSplits);
        }
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
        var paramMemo = new OdSqlParameter("paramMemo", SOut.StringParam(journalEntry.Memo));
        if (journalEntry.Splits == null) journalEntry.Splits = "";
        var paramSplits = new OdSqlParameter("paramSplits", SOut.StringParam(journalEntry.Splits));
        Db.NonQ(command, paramMemo, paramSplits);
    }
}