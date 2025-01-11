#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PerioExamCrud
{
    public static PerioExam SelectOne(long perioExamNum)
    {
        var command = "SELECT * FROM perioexam "
                      + "WHERE PerioExamNum = " + SOut.Long(perioExamNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PerioExam SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PerioExam> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PerioExam> TableToList(DataTable table)
    {
        var retVal = new List<PerioExam>();
        PerioExam perioExam;
        foreach (DataRow row in table.Rows)
        {
            perioExam = new PerioExam();
            perioExam.PerioExamNum = SIn.Long(row["PerioExamNum"].ToString());
            perioExam.PatNum = SIn.Long(row["PatNum"].ToString());
            perioExam.ExamDate = SIn.Date(row["ExamDate"].ToString());
            perioExam.ProvNum = SIn.Long(row["ProvNum"].ToString());
            perioExam.DateTMeasureEdit = SIn.DateTime(row["DateTMeasureEdit"].ToString());
            perioExam.Note = SIn.String(row["Note"].ToString());
            retVal.Add(perioExam);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PerioExam> listPerioExams, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PerioExam";
        var table = new DataTable(tableName);
        table.Columns.Add("PerioExamNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ExamDate");
        table.Columns.Add("ProvNum");
        table.Columns.Add("DateTMeasureEdit");
        table.Columns.Add("Note");
        foreach (var perioExam in listPerioExams)
            table.Rows.Add(SOut.Long(perioExam.PerioExamNum), SOut.Long(perioExam.PatNum), SOut.DateT(perioExam.ExamDate, false), SOut.Long(perioExam.ProvNum), SOut.DateT(perioExam.DateTMeasureEdit, false), perioExam.Note);
        return table;
    }

    public static long Insert(PerioExam perioExam)
    {
        return Insert(perioExam, false);
    }

    public static long Insert(PerioExam perioExam, bool useExistingPK)
    {
        var command = "INSERT INTO perioexam (";

        command += "PatNum,ExamDate,ProvNum,DateTMeasureEdit,Note) VALUES(";

        command +=
            SOut.Long(perioExam.PatNum) + ","
                                        + SOut.Date(perioExam.ExamDate) + ","
                                        + SOut.Long(perioExam.ProvNum) + ","
                                        + DbHelper.Now() + ","
                                        + DbHelper.ParamChar + "paramNote)";
        if (perioExam.Note == null) perioExam.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(perioExam.Note));
        {
            perioExam.PerioExamNum = Db.NonQ(command, true, "PerioExamNum", "perioExam", paramNote);
        }
        return perioExam.PerioExamNum;
    }

    public static long InsertNoCache(PerioExam perioExam)
    {
        return InsertNoCache(perioExam, false);
    }

    public static long InsertNoCache(PerioExam perioExam, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO perioexam (";
        if (isRandomKeys || useExistingPK) command += "PerioExamNum,";
        command += "PatNum,ExamDate,ProvNum,DateTMeasureEdit,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(perioExam.PerioExamNum) + ",";
        command +=
            SOut.Long(perioExam.PatNum) + ","
                                        + SOut.Date(perioExam.ExamDate) + ","
                                        + SOut.Long(perioExam.ProvNum) + ","
                                        + DbHelper.Now() + ","
                                        + DbHelper.ParamChar + "paramNote)";
        if (perioExam.Note == null) perioExam.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(perioExam.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            perioExam.PerioExamNum = Db.NonQ(command, true, "PerioExamNum", "perioExam", paramNote);
        return perioExam.PerioExamNum;
    }

    public static void Update(PerioExam perioExam)
    {
        var command = "UPDATE perioexam SET "
                      + "PatNum          =  " + SOut.Long(perioExam.PatNum) + ", "
                      + "ExamDate        =  " + SOut.Date(perioExam.ExamDate) + ", "
                      + "ProvNum         =  " + SOut.Long(perioExam.ProvNum) + ", "
                      + "DateTMeasureEdit=  " + SOut.DateT(perioExam.DateTMeasureEdit) + ", "
                      + "Note            =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE PerioExamNum = " + SOut.Long(perioExam.PerioExamNum);
        if (perioExam.Note == null) perioExam.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(perioExam.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(PerioExam perioExam, PerioExam oldPerioExam)
    {
        var command = "";
        if (perioExam.PatNum != oldPerioExam.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(perioExam.PatNum) + "";
        }

        if (perioExam.ExamDate.Date != oldPerioExam.ExamDate.Date)
        {
            if (command != "") command += ",";
            command += "ExamDate = " + SOut.Date(perioExam.ExamDate) + "";
        }

        if (perioExam.ProvNum != oldPerioExam.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(perioExam.ProvNum) + "";
        }

        if (perioExam.DateTMeasureEdit != oldPerioExam.DateTMeasureEdit)
        {
            if (command != "") command += ",";
            command += "DateTMeasureEdit = " + SOut.DateT(perioExam.DateTMeasureEdit) + "";
        }

        if (perioExam.Note != oldPerioExam.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (perioExam.Note == null) perioExam.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(perioExam.Note));
        command = "UPDATE perioexam SET " + command
                                          + " WHERE PerioExamNum = " + SOut.Long(perioExam.PerioExamNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(PerioExam perioExam, PerioExam oldPerioExam)
    {
        if (perioExam.PatNum != oldPerioExam.PatNum) return true;
        if (perioExam.ExamDate.Date != oldPerioExam.ExamDate.Date) return true;
        if (perioExam.ProvNum != oldPerioExam.ProvNum) return true;
        if (perioExam.DateTMeasureEdit != oldPerioExam.DateTMeasureEdit) return true;
        if (perioExam.Note != oldPerioExam.Note) return true;
        return false;
    }

    public static void Delete(long perioExamNum)
    {
        var command = "DELETE FROM perioexam "
                      + "WHERE PerioExamNum = " + SOut.Long(perioExamNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPerioExamNums)
    {
        if (listPerioExamNums == null || listPerioExamNums.Count == 0) return;
        var command = "DELETE FROM perioexam "
                      + "WHERE PerioExamNum IN(" + string.Join(",", listPerioExamNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}