using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcNoteCrud
{
    public static List<ProcNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcNote> TableToList(DataTable table)
    {
        var retVal = new List<ProcNote>();
        foreach (DataRow row in table.Rows)
        {
            var procNote = new ProcNote
            {
                ProcNoteNum = SIn.Long(row["ProcNoteNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                EntryDateTime = SIn.DateTime(row["EntryDateTime"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString()),
                Signature = SIn.String(row["Signature"].ToString())
            };
            retVal.Add(procNote);
        }

        return retVal;
    }

    public static void Insert(ProcNote procNote)
    {
        var command = "INSERT INTO procnote (";

        command += "PatNum,ProcNum,EntryDateTime,UserNum,Note,SigIsTopaz,Signature) VALUES(";

        command +=
            SOut.Long(procNote.PatNum) + ","
                                       + SOut.Long(procNote.ProcNum) + ","
                                       + "NOW()" + ","
                                       + SOut.Long(procNote.UserNum) + ","
                                       + DbHelper.ParamChar + "paramNote,"
                                       + SOut.Bool(procNote.SigIsTopaz) + ","
                                       + DbHelper.ParamChar + "paramSignature)";
        if (procNote.Note == null) procNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(procNote.Note));
        if (procNote.Signature == null) procNote.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(procNote.Signature));
        {
            procNote.ProcNoteNum = Db.NonQ(command, true, "ProcNoteNum", "procNote", paramNote, paramSignature);
        }
    }
}