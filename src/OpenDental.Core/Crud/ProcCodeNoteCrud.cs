using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcCodeNoteCrud
{
    public static List<ProcCodeNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcCodeNote> TableToList(DataTable table)
    {
        var retVal = new List<ProcCodeNote>();
        foreach (DataRow row in table.Rows)
        {
            var procCodeNote = new ProcCodeNote
            {
                ProcCodeNoteNum = SIn.Long(row["ProcCodeNoteNum"].ToString()),
                CodeNum = SIn.Long(row["CodeNum"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                ProcTime = SIn.String(row["ProcTime"].ToString()),
                ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString())
            };
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

    public static void Insert(ProcCodeNote procCodeNote)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(procCodeNote.Note));
        {
            procCodeNote.ProcCodeNoteNum = Db.NonQ(command, true, "ProcCodeNoteNum", "procCodeNote", paramNote);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(procCodeNote.Note));
        Db.NonQ(command, paramNote);
    }
}