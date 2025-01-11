#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcCodeNoteCrud
{
    public static ProcCodeNote SelectOne(long procCodeNoteNum)
    {
        var command = "SELECT * FROM proccodenote "
                      + "WHERE ProcCodeNoteNum = " + SOut.Long(procCodeNoteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcCodeNote SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcCodeNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcCodeNote> TableToList(DataTable table)
    {
        var retVal = new List<ProcCodeNote>();
        ProcCodeNote procCodeNote;
        foreach (DataRow row in table.Rows)
        {
            procCodeNote = new ProcCodeNote();
            procCodeNote.ProcCodeNoteNum = SIn.Long(row["ProcCodeNoteNum"].ToString());
            procCodeNote.CodeNum = SIn.Long(row["CodeNum"].ToString());
            procCodeNote.ProvNum = SIn.Long(row["ProvNum"].ToString());
            procCodeNote.Note = SIn.String(row["Note"].ToString());
            procCodeNote.ProcTime = SIn.String(row["ProcTime"].ToString());
            procCodeNote.ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString());
            retVal.Add(procCodeNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcCodeNote> listProcCodeNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcCodeNote";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcCodeNoteNum");
        table.Columns.Add("CodeNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("Note");
        table.Columns.Add("ProcTime");
        table.Columns.Add("ProcStatus");
        foreach (var procCodeNote in listProcCodeNotes)
            table.Rows.Add(SOut.Long(procCodeNote.ProcCodeNoteNum), SOut.Long(procCodeNote.CodeNum), SOut.Long(procCodeNote.ProvNum), procCodeNote.Note, procCodeNote.ProcTime, SOut.Int((int) procCodeNote.ProcStatus));
        return table;
    }

    public static long Insert(ProcCodeNote procCodeNote)
    {
        return Insert(procCodeNote, false);
    }

    public static long Insert(ProcCodeNote procCodeNote, bool useExistingPK)
    {
        var command = "INSERT INTO proccodenote (";

        command += "CodeNum,ProvNum,Note,ProcTime,ProcStatus) VALUES(";

        command +=
            SOut.Long(procCodeNote.CodeNum) + ","
                                            + SOut.Long(procCodeNote.ProvNum) + ","
                                            + DbHelper.ParamChar + "paramNote,"
                                            + "'" + SOut.String(procCodeNote.ProcTime) + "',"
                                            + SOut.Int((int) procCodeNote.ProcStatus) + ")";
        if (procCodeNote.Note == null) procCodeNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(procCodeNote.Note));
        {
            procCodeNote.ProcCodeNoteNum = Db.NonQ(command, true, "ProcCodeNoteNum", "procCodeNote", paramNote);
        }
        return procCodeNote.ProcCodeNoteNum;
    }

    public static long InsertNoCache(ProcCodeNote procCodeNote)
    {
        return InsertNoCache(procCodeNote, false);
    }

    public static long InsertNoCache(ProcCodeNote procCodeNote, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO proccodenote (";
        if (isRandomKeys || useExistingPK) command += "ProcCodeNoteNum,";
        command += "CodeNum,ProvNum,Note,ProcTime,ProcStatus) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procCodeNote.ProcCodeNoteNum) + ",";
        command +=
            SOut.Long(procCodeNote.CodeNum) + ","
                                            + SOut.Long(procCodeNote.ProvNum) + ","
                                            + DbHelper.ParamChar + "paramNote,"
                                            + "'" + SOut.String(procCodeNote.ProcTime) + "',"
                                            + SOut.Int((int) procCodeNote.ProcStatus) + ")";
        if (procCodeNote.Note == null) procCodeNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(procCodeNote.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            procCodeNote.ProcCodeNoteNum = Db.NonQ(command, true, "ProcCodeNoteNum", "procCodeNote", paramNote);
        return procCodeNote.ProcCodeNoteNum;
    }

    public static void Update(ProcCodeNote procCodeNote)
    {
        var command = "UPDATE proccodenote SET "
                      + "CodeNum        =  " + SOut.Long(procCodeNote.CodeNum) + ", "
                      + "ProvNum        =  " + SOut.Long(procCodeNote.ProvNum) + ", "
                      + "Note           =  " + DbHelper.ParamChar + "paramNote, "
                      + "ProcTime       = '" + SOut.String(procCodeNote.ProcTime) + "', "
                      + "ProcStatus     =  " + SOut.Int((int) procCodeNote.ProcStatus) + " "
                      + "WHERE ProcCodeNoteNum = " + SOut.Long(procCodeNote.ProcCodeNoteNum);
        if (procCodeNote.Note == null) procCodeNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(procCodeNote.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(ProcCodeNote procCodeNote, ProcCodeNote oldProcCodeNote)
    {
        var command = "";
        if (procCodeNote.CodeNum != oldProcCodeNote.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(procCodeNote.CodeNum) + "";
        }

        if (procCodeNote.ProvNum != oldProcCodeNote.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(procCodeNote.ProvNum) + "";
        }

        if (procCodeNote.Note != oldProcCodeNote.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (procCodeNote.ProcTime != oldProcCodeNote.ProcTime)
        {
            if (command != "") command += ",";
            command += "ProcTime = '" + SOut.String(procCodeNote.ProcTime) + "'";
        }

        if (procCodeNote.ProcStatus != oldProcCodeNote.ProcStatus)
        {
            if (command != "") command += ",";
            command += "ProcStatus = " + SOut.Int((int) procCodeNote.ProcStatus) + "";
        }

        if (command == "") return false;
        if (procCodeNote.Note == null) procCodeNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(procCodeNote.Note));
        command = "UPDATE proccodenote SET " + command
                                             + " WHERE ProcCodeNoteNum = " + SOut.Long(procCodeNote.ProcCodeNoteNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(ProcCodeNote procCodeNote, ProcCodeNote oldProcCodeNote)
    {
        if (procCodeNote.CodeNum != oldProcCodeNote.CodeNum) return true;
        if (procCodeNote.ProvNum != oldProcCodeNote.ProvNum) return true;
        if (procCodeNote.Note != oldProcCodeNote.Note) return true;
        if (procCodeNote.ProcTime != oldProcCodeNote.ProcTime) return true;
        if (procCodeNote.ProcStatus != oldProcCodeNote.ProcStatus) return true;
        return false;
    }

    public static void Delete(long procCodeNoteNum)
    {
        var command = "DELETE FROM proccodenote "
                      + "WHERE ProcCodeNoteNum = " + SOut.Long(procCodeNoteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcCodeNoteNums)
    {
        if (listProcCodeNoteNums == null || listProcCodeNoteNums.Count == 0) return;
        var command = "DELETE FROM proccodenote "
                      + "WHERE ProcCodeNoteNum IN(" + string.Join(",", listProcCodeNoteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}