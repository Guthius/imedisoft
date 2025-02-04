using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<Recall> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Recall> TableToList(DataTable table)
    {
        var retVal = new List<Recall>();
        foreach (DataRow row in table.Rows)
        {
            var recall = new Recall
            {
                RecallNum = SIn.Long(row["RecallNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateDueCalc = SIn.Date(row["DateDueCalc"].ToString()),
                DateDue = SIn.Date(row["DateDue"].ToString()),
                DatePrevious = SIn.Date(row["DatePrevious"].ToString()),
                RecallInterval = new Interval(SIn.Int(row["RecallInterval"].ToString())),
                RecallStatus = SIn.Long(row["RecallStatus"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                IsDisabled = SIn.Bool(row["IsDisabled"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                RecallTypeNum = SIn.Long(row["RecallTypeNum"].ToString()),
                DisableUntilBalance = SIn.Double(row["DisableUntilBalance"].ToString()),
                DisableUntilDate = SIn.Date(row["DisableUntilDate"].ToString()),
                DateScheduled = SIn.Date(row["DateScheduled"].ToString()),
                Priority = (RecallPriority) SIn.Int(row["Priority"].ToString()),
                TimePatternOverride = SIn.String(row["TimePatternOverride"].ToString())
            };
            retVal.Add(recall);
        }

        return retVal;
    }

    public static void Insert(Recall recall)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(recall.Note));
        {
            recall.RecallNum = Db.NonQ(command, true, "RecallNum", "recall", paramNote);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(recall.Note));
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(recall.Note));
        command = "UPDATE recall SET " + command
                                       + " WHERE RecallNum = " + SOut.Long(recall.RecallNum);
        Db.NonQ(command, paramNote);
        return true;
    }
}