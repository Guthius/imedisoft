using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<PerioExam> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PerioExam> TableToList(DataTable table)
    {
        var retVal = new List<PerioExam>();
        foreach (DataRow row in table.Rows)
        {
            var perioExam = new PerioExam
            {
                PerioExamNum = SIn.Long(row["PerioExamNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ExamDate = SIn.Date(row["ExamDate"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                DateTMeasureEdit = SIn.DateTime(row["DateTMeasureEdit"].ToString()),
                Note = SIn.String(row["Note"].ToString())
            };
            retVal.Add(perioExam);
        }

        return retVal;
    }

    public static void Insert(PerioExam perioExam)
    {
        var command = "INSERT INTO perioexam (";

        command += "PatNum,ExamDate,ProvNum,DateTMeasureEdit,Note) VALUES(";

        command +=
            SOut.Long(perioExam.PatNum) + ","
                                        + SOut.Date(perioExam.ExamDate) + ","
                                        + SOut.Long(perioExam.ProvNum) + ","
                                        + "NOW()" + ","
                                        + DbHelper.ParamChar + "paramNote)";
        if (perioExam.Note == null) perioExam.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(perioExam.Note));
        {
            perioExam.PerioExamNum = Db.NonQ(command, true, "PerioExamNum", "perioExam", paramNote);
        }
    }

    public static void Update(PerioExam perioExam)
    {
        var command = "UPDATE perioexam SET "
                      + "PatNum          =  " + SOut.Long(perioExam.PatNum) + ", "
                      + "ExamDate        =  " + SOut.Date(perioExam.ExamDate) + ", "
                      + "ProvNum         =  " + SOut.Long(perioExam.ProvNum) + ", "
                      + "DateTMeasureEdit=  " + SOut.DateTime(perioExam.DateTMeasureEdit) + ", "
                      + "Note            =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE PerioExamNum = " + SOut.Long(perioExam.PerioExamNum);
        if (perioExam.Note == null) perioExam.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(perioExam.Note));
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
            command += "DateTMeasureEdit = " + SOut.DateTime(perioExam.DateTMeasureEdit) + "";
        }

        if (perioExam.Note != oldPerioExam.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (perioExam.Note == null) perioExam.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(perioExam.Note));
        command = "UPDATE perioexam SET " + command
                                          + " WHERE PerioExamNum = " + SOut.Long(perioExam.PerioExamNum);
        Db.NonQ(command, paramNote);
        return true;
    }
}