#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FeeSchedNoteCrud
{
    public static FeeSchedNote SelectOne(long feeSchedNoteNum)
    {
        var command = "SELECT * FROM feeschednote "
                      + "WHERE FeeSchedNoteNum = " + SOut.Long(feeSchedNoteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static FeeSchedNote SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FeeSchedNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FeeSchedNote> TableToList(DataTable table)
    {
        var retVal = new List<FeeSchedNote>();
        FeeSchedNote feeSchedNote;
        foreach (DataRow row in table.Rows)
        {
            feeSchedNote = new FeeSchedNote();
            feeSchedNote.FeeSchedNoteNum = SIn.Long(row["FeeSchedNoteNum"].ToString());
            feeSchedNote.FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString());
            feeSchedNote.ClinicNums = SIn.String(row["ClinicNums"].ToString());
            feeSchedNote.Note = SIn.String(row["Note"].ToString());
            feeSchedNote.DateEntry = SIn.Date(row["DateEntry"].ToString());
            feeSchedNote.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            feeSchedNote.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            feeSchedNote.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(feeSchedNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FeeSchedNote> listFeeSchedNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FeeSchedNote";
        var table = new DataTable(tableName);
        table.Columns.Add("FeeSchedNoteNum");
        table.Columns.Add("FeeSchedNum");
        table.Columns.Add("ClinicNums");
        table.Columns.Add("Note");
        table.Columns.Add("DateEntry");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var feeSchedNote in listFeeSchedNotes)
            table.Rows.Add(SOut.Long(feeSchedNote.FeeSchedNoteNum), SOut.Long(feeSchedNote.FeeSchedNum), feeSchedNote.ClinicNums, feeSchedNote.Note, SOut.DateT(feeSchedNote.DateEntry, false), SOut.Long(feeSchedNote.SecUserNumEntry), SOut.DateT(feeSchedNote.SecDateEntry, false), SOut.DateT(feeSchedNote.SecDateTEdit, false));
        return table;
    }

    public static long Insert(FeeSchedNote feeSchedNote)
    {
        return Insert(feeSchedNote, false);
    }

    public static long Insert(FeeSchedNote feeSchedNote, bool useExistingPK)
    {
        var command = "INSERT INTO feeschednote (";

        command += "FeeSchedNum,ClinicNums,Note,DateEntry,SecUserNumEntry,SecDateEntry) VALUES(";

        command +=
            SOut.Long(feeSchedNote.FeeSchedNum) + ","
                                                + DbHelper.ParamChar + "paramClinicNums,"
                                                + DbHelper.ParamChar + "paramNote,"
                                                + SOut.Date(feeSchedNote.DateEntry) + ","
                                                + SOut.Long(feeSchedNote.SecUserNumEntry) + ","
                                                + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (feeSchedNote.ClinicNums == null) feeSchedNote.ClinicNums = "";
        var paramClinicNums = new OdSqlParameter("paramClinicNums", OdDbType.Text, SOut.StringParam(feeSchedNote.ClinicNums));
        if (feeSchedNote.Note == null) feeSchedNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(feeSchedNote.Note));
        {
            feeSchedNote.FeeSchedNoteNum = Db.NonQ(command, true, "FeeSchedNoteNum", "feeSchedNote", paramClinicNums, paramNote);
        }
        return feeSchedNote.FeeSchedNoteNum;
    }

    public static long InsertNoCache(FeeSchedNote feeSchedNote)
    {
        return InsertNoCache(feeSchedNote, false);
    }

    public static long InsertNoCache(FeeSchedNote feeSchedNote, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO feeschednote (";
        if (isRandomKeys || useExistingPK) command += "FeeSchedNoteNum,";
        command += "FeeSchedNum,ClinicNums,Note,DateEntry,SecUserNumEntry,SecDateEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(feeSchedNote.FeeSchedNoteNum) + ",";
        command +=
            SOut.Long(feeSchedNote.FeeSchedNum) + ","
                                                + DbHelper.ParamChar + "paramClinicNums,"
                                                + DbHelper.ParamChar + "paramNote,"
                                                + SOut.Date(feeSchedNote.DateEntry) + ","
                                                + SOut.Long(feeSchedNote.SecUserNumEntry) + ","
                                                + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (feeSchedNote.ClinicNums == null) feeSchedNote.ClinicNums = "";
        var paramClinicNums = new OdSqlParameter("paramClinicNums", OdDbType.Text, SOut.StringParam(feeSchedNote.ClinicNums));
        if (feeSchedNote.Note == null) feeSchedNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(feeSchedNote.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramClinicNums, paramNote);
        else
            feeSchedNote.FeeSchedNoteNum = Db.NonQ(command, true, "FeeSchedNoteNum", "feeSchedNote", paramClinicNums, paramNote);
        return feeSchedNote.FeeSchedNoteNum;
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
        var paramClinicNums = new OdSqlParameter("paramClinicNums", OdDbType.Text, SOut.StringParam(feeSchedNote.ClinicNums));
        if (feeSchedNote.Note == null) feeSchedNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(feeSchedNote.Note));
        Db.NonQ(command, paramClinicNums, paramNote);
    }

    public static bool Update(FeeSchedNote feeSchedNote, FeeSchedNote oldFeeSchedNote)
    {
        var command = "";
        if (feeSchedNote.FeeSchedNum != oldFeeSchedNote.FeeSchedNum)
        {
            if (command != "") command += ",";
            command += "FeeSchedNum = " + SOut.Long(feeSchedNote.FeeSchedNum) + "";
        }

        if (feeSchedNote.ClinicNums != oldFeeSchedNote.ClinicNums)
        {
            if (command != "") command += ",";
            command += "ClinicNums = " + DbHelper.ParamChar + "paramClinicNums";
        }

        if (feeSchedNote.Note != oldFeeSchedNote.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (feeSchedNote.DateEntry.Date != oldFeeSchedNote.DateEntry.Date)
        {
            if (command != "") command += ",";
            command += "DateEntry = " + SOut.Date(feeSchedNote.DateEntry) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        if (feeSchedNote.ClinicNums == null) feeSchedNote.ClinicNums = "";
        var paramClinicNums = new OdSqlParameter("paramClinicNums", OdDbType.Text, SOut.StringParam(feeSchedNote.ClinicNums));
        if (feeSchedNote.Note == null) feeSchedNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(feeSchedNote.Note));
        command = "UPDATE feeschednote SET " + command
                                             + " WHERE FeeSchedNoteNum = " + SOut.Long(feeSchedNote.FeeSchedNoteNum);
        Db.NonQ(command, paramClinicNums, paramNote);
        return true;
    }

    public static bool UpdateComparison(FeeSchedNote feeSchedNote, FeeSchedNote oldFeeSchedNote)
    {
        if (feeSchedNote.FeeSchedNum != oldFeeSchedNote.FeeSchedNum) return true;
        if (feeSchedNote.ClinicNums != oldFeeSchedNote.ClinicNums) return true;
        if (feeSchedNote.Note != oldFeeSchedNote.Note) return true;
        if (feeSchedNote.DateEntry.Date != oldFeeSchedNote.DateEntry.Date) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long feeSchedNoteNum)
    {
        var command = "DELETE FROM feeschednote "
                      + "WHERE FeeSchedNoteNum = " + SOut.Long(feeSchedNoteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFeeSchedNoteNums)
    {
        if (listFeeSchedNoteNums == null || listFeeSchedNoteNums.Count == 0) return;
        var command = "DELETE FROM feeschednote "
                      + "WHERE FeeSchedNoteNum IN(" + string.Join(",", listFeeSchedNoteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}