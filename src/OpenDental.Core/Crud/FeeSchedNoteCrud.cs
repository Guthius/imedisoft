using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class FeeSchedNoteCrud
{
    public static List<FeeSchedNote> TableToList(DataTable table)
    {
        var retVal = new List<FeeSchedNote>();
        foreach (DataRow row in table.Rows)
        {
            var feeSchedNote = new FeeSchedNote
            {
                FeeSchedNoteNum = SIn.Long(row["FeeSchedNoteNum"].ToString()),
                FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString()),
                ClinicNums = SIn.String(row["ClinicNums"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                DateEntry = SIn.Date(row["DateEntry"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(feeSchedNote);
        }

        return retVal;
    }

    public static void Insert(FeeSchedNote feeSchedNote)
    {
        var command = "INSERT INTO feeschednote (";

        command += "FeeSchedNum,ClinicNums,Note,DateEntry,SecUserNumEntry,SecDateEntry) VALUES(";

        command +=
            SOut.Long(feeSchedNote.FeeSchedNum) + ","
                                                + DbHelper.ParamChar + "paramClinicNums,"
                                                + DbHelper.ParamChar + "paramNote,"
                                                + SOut.Date(feeSchedNote.DateEntry) + ","
                                                + SOut.Long(feeSchedNote.SecUserNumEntry) + ","
                                                + "NOW()" + ")";
        //SecDateTEdit can only be set by MySQL
        if (feeSchedNote.ClinicNums == null) feeSchedNote.ClinicNums = "";
        var paramClinicNums = new OdSqlParameter("paramClinicNums", SOut.StringParam(feeSchedNote.ClinicNums));
        if (feeSchedNote.Note == null) feeSchedNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(feeSchedNote.Note));
        {
            feeSchedNote.FeeSchedNoteNum = Db.NonQ(command, true, "FeeSchedNoteNum", "feeSchedNote", paramClinicNums, paramNote);
        }
    }

    public static void Update(FeeSchedNote feeSchedNote)
    {
        var command = "UPDATE feeschednote SET "
                      + "FeeSchedNum    =  " + SOut.Long(feeSchedNote.FeeSchedNum) + ", "
                      + "ClinicNums     =  " + DbHelper.ParamChar + "paramClinicNums, "
                      + "Note           =  " + DbHelper.ParamChar + "paramNote, "
                      + "DateEntry      =  " + SOut.Date(feeSchedNote.DateEntry) + " "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE FeeSchedNoteNum = " + SOut.Long(feeSchedNote.FeeSchedNoteNum);
        if (feeSchedNote.ClinicNums == null) feeSchedNote.ClinicNums = "";
        var paramClinicNums = new OdSqlParameter("paramClinicNums", SOut.StringParam(feeSchedNote.ClinicNums));
        if (feeSchedNote.Note == null) feeSchedNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(feeSchedNote.Note));
        Db.NonQ(command, paramClinicNums, paramNote);
    }

    public static void Delete(long feeSchedNoteNum)
    {
        var command = "DELETE FROM feeschednote "
                      + "WHERE FeeSchedNoteNum = " + SOut.Long(feeSchedNoteNum);
        Db.NonQ(command);
    }
}