#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcNoteCrud
{
    public static ProcNote SelectOne(long procNoteNum)
    {
        var command = "SELECT * FROM procnote "
                      + "WHERE ProcNoteNum = " + SOut.Long(procNoteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcNote SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcNote> TableToList(DataTable table)
    {
        var retVal = new List<ProcNote>();
        ProcNote procNote;
        foreach (DataRow row in table.Rows)
        {
            procNote = new ProcNote();
            procNote.ProcNoteNum = SIn.Long(row["ProcNoteNum"].ToString());
            procNote.PatNum = SIn.Long(row["PatNum"].ToString());
            procNote.ProcNum = SIn.Long(row["ProcNum"].ToString());
            procNote.EntryDateTime = SIn.DateTime(row["EntryDateTime"].ToString());
            procNote.UserNum = SIn.Long(row["UserNum"].ToString());
            procNote.Note = SIn.String(row["Note"].ToString());
            procNote.SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString());
            procNote.Signature = SIn.String(row["Signature"].ToString());
            retVal.Add(procNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcNote> listProcNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcNote";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcNoteNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProcNum");
        table.Columns.Add("EntryDateTime");
        table.Columns.Add("UserNum");
        table.Columns.Add("Note");
        table.Columns.Add("SigIsTopaz");
        table.Columns.Add("Signature");
        foreach (var procNote in listProcNotes)
            table.Rows.Add(SOut.Long(procNote.ProcNoteNum), SOut.Long(procNote.PatNum), SOut.Long(procNote.ProcNum), SOut.DateT(procNote.EntryDateTime, false), SOut.Long(procNote.UserNum), procNote.Note, SOut.Bool(procNote.SigIsTopaz), procNote.Signature);
        return table;
    }

    public static long Insert(ProcNote procNote)
    {
        return Insert(procNote, false);
    }

    public static long Insert(ProcNote procNote, bool useExistingPK)
    {
        var command = "INSERT INTO procnote (";

        command += "PatNum,ProcNum,EntryDateTime,UserNum,Note,SigIsTopaz,Signature) VALUES(";

        command +=
            SOut.Long(procNote.PatNum) + ","
                                       + SOut.Long(procNote.ProcNum) + ","
                                       + DbHelper.Now() + ","
                                       + SOut.Long(procNote.UserNum) + ","
                                       + DbHelper.ParamChar + "paramNote,"
                                       + SOut.Bool(procNote.SigIsTopaz) + ","
                                       + DbHelper.ParamChar + "paramSignature)";
        if (procNote.Note == null) procNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(procNote.Note));
        if (procNote.Signature == null) procNote.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(procNote.Signature));
        {
            procNote.ProcNoteNum = Db.NonQ(command, true, "ProcNoteNum", "procNote", paramNote, paramSignature);
        }
        return procNote.ProcNoteNum;
    }

    public static long InsertNoCache(ProcNote procNote)
    {
        return InsertNoCache(procNote, false);
    }

    public static long InsertNoCache(ProcNote procNote, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procnote (";
        if (isRandomKeys || useExistingPK) command += "ProcNoteNum,";
        command += "PatNum,ProcNum,EntryDateTime,UserNum,Note,SigIsTopaz,Signature) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procNote.ProcNoteNum) + ",";
        command +=
            SOut.Long(procNote.PatNum) + ","
                                       + SOut.Long(procNote.ProcNum) + ","
                                       + DbHelper.Now() + ","
                                       + SOut.Long(procNote.UserNum) + ","
                                       + DbHelper.ParamChar + "paramNote,"
                                       + SOut.Bool(procNote.SigIsTopaz) + ","
                                       + DbHelper.ParamChar + "paramSignature)";
        if (procNote.Note == null) procNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(procNote.Note));
        if (procNote.Signature == null) procNote.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(procNote.Signature));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote, paramSignature);
        else
            procNote.ProcNoteNum = Db.NonQ(command, true, "ProcNoteNum", "procNote", paramNote, paramSignature);
        return procNote.ProcNoteNum;
    }

    public static void Update(ProcNote procNote)
    {
        var command = "UPDATE procnote SET "
                      + "PatNum       =  " + SOut.Long(procNote.PatNum) + ", "
                      + "ProcNum      =  " + SOut.Long(procNote.ProcNum) + ", "
                      //EntryDateTime not allowed to change
                      + "UserNum      =  " + SOut.Long(procNote.UserNum) + ", "
                      + "Note         =  " + DbHelper.ParamChar + "paramNote, "
                      + "SigIsTopaz   =  " + SOut.Bool(procNote.SigIsTopaz) + ", "
                      + "Signature    =  " + DbHelper.ParamChar + "paramSignature "
                      + "WHERE ProcNoteNum = " + SOut.Long(procNote.ProcNoteNum);
        if (procNote.Note == null) procNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(procNote.Note));
        if (procNote.Signature == null) procNote.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(procNote.Signature));
        Db.NonQ(command, paramNote, paramSignature);
    }

    public static bool Update(ProcNote procNote, ProcNote oldProcNote)
    {
        var command = "";
        if (procNote.PatNum != oldProcNote.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(procNote.PatNum) + "";
        }

        if (procNote.ProcNum != oldProcNote.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(procNote.ProcNum) + "";
        }

        //EntryDateTime not allowed to change
        if (procNote.UserNum != oldProcNote.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(procNote.UserNum) + "";
        }

        if (procNote.Note != oldProcNote.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (procNote.SigIsTopaz != oldProcNote.SigIsTopaz)
        {
            if (command != "") command += ",";
            command += "SigIsTopaz = " + SOut.Bool(procNote.SigIsTopaz) + "";
        }

        if (procNote.Signature != oldProcNote.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (command == "") return false;
        if (procNote.Note == null) procNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(procNote.Note));
        if (procNote.Signature == null) procNote.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(procNote.Signature));
        command = "UPDATE procnote SET " + command
                                         + " WHERE ProcNoteNum = " + SOut.Long(procNote.ProcNoteNum);
        Db.NonQ(command, paramNote, paramSignature);
        return true;
    }

    public static bool UpdateComparison(ProcNote procNote, ProcNote oldProcNote)
    {
        if (procNote.PatNum != oldProcNote.PatNum) return true;
        if (procNote.ProcNum != oldProcNote.ProcNum) return true;
        //EntryDateTime not allowed to change
        if (procNote.UserNum != oldProcNote.UserNum) return true;
        if (procNote.Note != oldProcNote.Note) return true;
        if (procNote.SigIsTopaz != oldProcNote.SigIsTopaz) return true;
        if (procNote.Signature != oldProcNote.Signature) return true;
        return false;
    }

    public static void Delete(long procNoteNum)
    {
        var command = "DELETE FROM procnote "
                      + "WHERE ProcNoteNum = " + SOut.Long(procNoteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcNoteNums)
    {
        if (listProcNoteNums == null || listProcNoteNums.Count == 0) return;
        var command = "DELETE FROM procnote "
                      + "WHERE ProcNoteNum IN(" + string.Join(",", listProcNoteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}