#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RecallCrud
{
    public static Recall SelectOne(long recallNum)
    {
        var command = "SELECT * FROM recall "
                      + "WHERE RecallNum = " + SOut.Long(recallNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Recall SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Recall> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Recall> TableToList(DataTable table)
    {
        var retVal = new List<Recall>();
        Recall recall;
        foreach (DataRow row in table.Rows)
        {
            recall = new Recall();
            recall.RecallNum = SIn.Long(row["RecallNum"].ToString());
            recall.PatNum = SIn.Long(row["PatNum"].ToString());
            recall.DateDueCalc = SIn.Date(row["DateDueCalc"].ToString());
            recall.DateDue = SIn.Date(row["DateDue"].ToString());
            recall.DatePrevious = SIn.Date(row["DatePrevious"].ToString());
            recall.RecallInterval = new Interval(SIn.Int(row["RecallInterval"].ToString()));
            recall.RecallStatus = SIn.Long(row["RecallStatus"].ToString());
            recall.Note = SIn.String(row["Note"].ToString());
            recall.IsDisabled = SIn.Bool(row["IsDisabled"].ToString());
            recall.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            recall.RecallTypeNum = SIn.Long(row["RecallTypeNum"].ToString());
            recall.DisableUntilBalance = SIn.Double(row["DisableUntilBalance"].ToString());
            recall.DisableUntilDate = SIn.Date(row["DisableUntilDate"].ToString());
            recall.DateScheduled = SIn.Date(row["DateScheduled"].ToString());
            recall.Priority = (RecallPriority) SIn.Int(row["Priority"].ToString());
            recall.TimePatternOverride = SIn.String(row["TimePatternOverride"].ToString());
            retVal.Add(recall);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Recall> listRecalls, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Recall";
        var table = new DataTable(tableName);
        table.Columns.Add("RecallNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateDueCalc");
        table.Columns.Add("DateDue");
        table.Columns.Add("DatePrevious");
        table.Columns.Add("RecallInterval");
        table.Columns.Add("RecallStatus");
        table.Columns.Add("Note");
        table.Columns.Add("IsDisabled");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("RecallTypeNum");
        table.Columns.Add("DisableUntilBalance");
        table.Columns.Add("DisableUntilDate");
        table.Columns.Add("DateScheduled");
        table.Columns.Add("Priority");
        table.Columns.Add("TimePatternOverride");
        foreach (var recall in listRecalls)
            table.Rows.Add(SOut.Long(recall.RecallNum), SOut.Long(recall.PatNum), SOut.DateT(recall.DateDueCalc, false), SOut.DateT(recall.DateDue, false), SOut.DateT(recall.DatePrevious, false), SOut.Int(recall.RecallInterval.ToInt()), SOut.Long(recall.RecallStatus), recall.Note, SOut.Bool(recall.IsDisabled), SOut.DateT(recall.DateTStamp, false), SOut.Long(recall.RecallTypeNum), SOut.Double(recall.DisableUntilBalance), SOut.DateT(recall.DisableUntilDate, false), SOut.DateT(recall.DateScheduled, false), SOut.Int((int) recall.Priority), recall.TimePatternOverride);
        return table;
    }

    public static long Insert(Recall recall)
    {
        return Insert(recall, false);
    }

    public static long Insert(Recall recall, bool useExistingPK)
    {
        var command = "INSERT INTO recall (";

        command += "PatNum,DateDueCalc,DateDue,DatePrevious,RecallInterval,RecallStatus,Note,IsDisabled,RecallTypeNum,DisableUntilBalance,DisableUntilDate,DateScheduled,Priority,TimePatternOverride) VALUES(";

        command +=
            SOut.Long(recall.PatNum) + ","
                                     + SOut.Date(recall.DateDueCalc) + ","
                                     + SOut.Date(recall.DateDue) + ","
                                     + SOut.Date(recall.DatePrevious) + ","
                                     + SOut.Int(recall.RecallInterval.ToInt()) + ","
                                     + SOut.Long(recall.RecallStatus) + ","
                                     + DbHelper.ParamChar + "paramNote,"
                                     + SOut.Bool(recall.IsDisabled) + ","
                                     //DateTStamp can only be set by MySQL
                                     + SOut.Long(recall.RecallTypeNum) + ","
                                     + SOut.Double(recall.DisableUntilBalance) + ","
                                     + SOut.Date(recall.DisableUntilDate) + ","
                                     + SOut.Date(recall.DateScheduled) + ","
                                     + SOut.Int((int) recall.Priority) + ","
                                     + "'" + SOut.String(recall.TimePatternOverride) + "')";
        if (recall.Note == null) recall.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(recall.Note));
        {
            recall.RecallNum = Db.NonQ(command, true, "RecallNum", "recall", paramNote);
        }
        return recall.RecallNum;
    }

    public static void InsertMany(List<Recall> listRecalls)
    {
        InsertMany(listRecalls, false);
    }

    public static void InsertMany(List<Recall> listRecalls, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listRecalls.Count)
        {
            var recall = listRecalls[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO recall (");
                if (useExistingPK) sbCommands.Append("RecallNum,");
                sbCommands.Append("PatNum,DateDueCalc,DateDue,DatePrevious,RecallInterval,RecallStatus,Note,IsDisabled,RecallTypeNum,DisableUntilBalance,DisableUntilDate,DateScheduled,Priority,TimePatternOverride) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(recall.RecallNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(recall.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(recall.DateDueCalc));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(recall.DateDue));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(recall.DatePrevious));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(recall.RecallInterval.ToInt()));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(recall.RecallStatus));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(recall.Note) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(recall.IsDisabled));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(recall.RecallTypeNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(recall.DisableUntilBalance));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(recall.DisableUntilDate));
            sbRow.Append(",");
            sbRow.Append(SOut.Date(recall.DateScheduled));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) recall.Priority));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(recall.TimePatternOverride) + "'");
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listRecalls.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Recall recall)
    {
        return InsertNoCache(recall, false);
    }

    public static long InsertNoCache(Recall recall, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO recall (";
        if (isRandomKeys || useExistingPK) command += "RecallNum,";
        command += "PatNum,DateDueCalc,DateDue,DatePrevious,RecallInterval,RecallStatus,Note,IsDisabled,RecallTypeNum,DisableUntilBalance,DisableUntilDate,DateScheduled,Priority,TimePatternOverride) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(recall.RecallNum) + ",";
        command +=
            SOut.Long(recall.PatNum) + ","
                                     + SOut.Date(recall.DateDueCalc) + ","
                                     + SOut.Date(recall.DateDue) + ","
                                     + SOut.Date(recall.DatePrevious) + ","
                                     + SOut.Int(recall.RecallInterval.ToInt()) + ","
                                     + SOut.Long(recall.RecallStatus) + ","
                                     + DbHelper.ParamChar + "paramNote,"
                                     + SOut.Bool(recall.IsDisabled) + ","
                                     //DateTStamp can only be set by MySQL
                                     + SOut.Long(recall.RecallTypeNum) + ","
                                     + SOut.Double(recall.DisableUntilBalance) + ","
                                     + SOut.Date(recall.DisableUntilDate) + ","
                                     + SOut.Date(recall.DateScheduled) + ","
                                     + SOut.Int((int) recall.Priority) + ","
                                     + "'" + SOut.String(recall.TimePatternOverride) + "')";
        if (recall.Note == null) recall.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(recall.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            recall.RecallNum = Db.NonQ(command, true, "RecallNum", "recall", paramNote);
        return recall.RecallNum;
    }

    public static void Update(Recall recall)
    {
        var command = "UPDATE recall SET "
                      + "PatNum             =  " + SOut.Long(recall.PatNum) + ", "
                      + "DateDueCalc        =  " + SOut.Date(recall.DateDueCalc) + ", "
                      + "DateDue            =  " + SOut.Date(recall.DateDue) + ", "
                      + "DatePrevious       =  " + SOut.Date(recall.DatePrevious) + ", "
                      + "RecallInterval     =  " + SOut.Int(recall.RecallInterval.ToInt()) + ", "
                      + "RecallStatus       =  " + SOut.Long(recall.RecallStatus) + ", "
                      + "Note               =  " + DbHelper.ParamChar + "paramNote, "
                      + "IsDisabled         =  " + SOut.Bool(recall.IsDisabled) + ", "
                      //DateTStamp can only be set by MySQL
                      + "RecallTypeNum      =  " + SOut.Long(recall.RecallTypeNum) + ", "
                      + "DisableUntilBalance=  " + SOut.Double(recall.DisableUntilBalance) + ", "
                      + "DisableUntilDate   =  " + SOut.Date(recall.DisableUntilDate) + ", "
                      + "DateScheduled      =  " + SOut.Date(recall.DateScheduled) + ", "
                      + "Priority           =  " + SOut.Int((int) recall.Priority) + ", "
                      + "TimePatternOverride= '" + SOut.String(recall.TimePatternOverride) + "' "
                      + "WHERE RecallNum = " + SOut.Long(recall.RecallNum);
        if (recall.Note == null) recall.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(recall.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Recall recall, Recall oldRecall)
    {
        var command = "";
        if (recall.PatNum != oldRecall.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(recall.PatNum) + "";
        }

        if (recall.DateDueCalc.Date != oldRecall.DateDueCalc.Date)
        {
            if (command != "") command += ",";
            command += "DateDueCalc = " + SOut.Date(recall.DateDueCalc) + "";
        }

        if (recall.DateDue.Date != oldRecall.DateDue.Date)
        {
            if (command != "") command += ",";
            command += "DateDue = " + SOut.Date(recall.DateDue) + "";
        }

        if (recall.DatePrevious.Date != oldRecall.DatePrevious.Date)
        {
            if (command != "") command += ",";
            command += "DatePrevious = " + SOut.Date(recall.DatePrevious) + "";
        }

        if (recall.RecallInterval != oldRecall.RecallInterval)
        {
            if (command != "") command += ",";
            command += "RecallInterval = " + SOut.Int(recall.RecallInterval.ToInt()) + "";
        }

        if (recall.RecallStatus != oldRecall.RecallStatus)
        {
            if (command != "") command += ",";
            command += "RecallStatus = " + SOut.Long(recall.RecallStatus) + "";
        }

        if (recall.Note != oldRecall.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (recall.IsDisabled != oldRecall.IsDisabled)
        {
            if (command != "") command += ",";
            command += "IsDisabled = " + SOut.Bool(recall.IsDisabled) + "";
        }

        //DateTStamp can only be set by MySQL
        if (recall.RecallTypeNum != oldRecall.RecallTypeNum)
        {
            if (command != "") command += ",";
            command += "RecallTypeNum = " + SOut.Long(recall.RecallTypeNum) + "";
        }

        if (recall.DisableUntilBalance != oldRecall.DisableUntilBalance)
        {
            if (command != "") command += ",";
            command += "DisableUntilBalance = " + SOut.Double(recall.DisableUntilBalance) + "";
        }

        if (recall.DisableUntilDate.Date != oldRecall.DisableUntilDate.Date)
        {
            if (command != "") command += ",";
            command += "DisableUntilDate = " + SOut.Date(recall.DisableUntilDate) + "";
        }

        if (recall.DateScheduled.Date != oldRecall.DateScheduled.Date)
        {
            if (command != "") command += ",";
            command += "DateScheduled = " + SOut.Date(recall.DateScheduled) + "";
        }

        if (recall.Priority != oldRecall.Priority)
        {
            if (command != "") command += ",";
            command += "Priority = " + SOut.Int((int) recall.Priority) + "";
        }

        if (recall.TimePatternOverride != oldRecall.TimePatternOverride)
        {
            if (command != "") command += ",";
            command += "TimePatternOverride = '" + SOut.String(recall.TimePatternOverride) + "'";
        }

        if (command == "") return false;
        if (recall.Note == null) recall.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(recall.Note));
        command = "UPDATE recall SET " + command
                                       + " WHERE RecallNum = " + SOut.Long(recall.RecallNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(Recall recall, Recall oldRecall)
    {
        if (recall.PatNum != oldRecall.PatNum) return true;
        if (recall.DateDueCalc.Date != oldRecall.DateDueCalc.Date) return true;
        if (recall.DateDue.Date != oldRecall.DateDue.Date) return true;
        if (recall.DatePrevious.Date != oldRecall.DatePrevious.Date) return true;
        if (recall.RecallInterval != oldRecall.RecallInterval) return true;
        if (recall.RecallStatus != oldRecall.RecallStatus) return true;
        if (recall.Note != oldRecall.Note) return true;
        if (recall.IsDisabled != oldRecall.IsDisabled) return true;
        //DateTStamp can only be set by MySQL
        if (recall.RecallTypeNum != oldRecall.RecallTypeNum) return true;
        if (recall.DisableUntilBalance != oldRecall.DisableUntilBalance) return true;
        if (recall.DisableUntilDate.Date != oldRecall.DisableUntilDate.Date) return true;
        if (recall.DateScheduled.Date != oldRecall.DateScheduled.Date) return true;
        if (recall.Priority != oldRecall.Priority) return true;
        if (recall.TimePatternOverride != oldRecall.TimePatternOverride) return true;
        return false;
    }

    public static void Delete(long recallNum)
    {
        var command = "DELETE FROM recall "
                      + "WHERE RecallNum = " + SOut.Long(recallNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRecallNums)
    {
        if (listRecallNums == null || listRecallNums.Count == 0) return;
        var command = "DELETE FROM recall "
                      + "WHERE RecallNum IN(" + string.Join(",", listRecallNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}