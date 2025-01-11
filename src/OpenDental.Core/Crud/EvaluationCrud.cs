#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EvaluationCrud
{
    public static Evaluation SelectOne(long evaluationNum)
    {
        var command = "SELECT * FROM evaluation "
                      + "WHERE EvaluationNum = " + SOut.Long(evaluationNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Evaluation SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Evaluation> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Evaluation> TableToList(DataTable table)
    {
        var retVal = new List<Evaluation>();
        Evaluation evaluation;
        foreach (DataRow row in table.Rows)
        {
            evaluation = new Evaluation();
            evaluation.EvaluationNum = SIn.Long(row["EvaluationNum"].ToString());
            evaluation.InstructNum = SIn.Long(row["InstructNum"].ToString());
            evaluation.StudentNum = SIn.Long(row["StudentNum"].ToString());
            evaluation.SchoolCourseNum = SIn.Long(row["SchoolCourseNum"].ToString());
            evaluation.EvalTitle = SIn.String(row["EvalTitle"].ToString());
            evaluation.DateEval = SIn.Date(row["DateEval"].ToString());
            evaluation.GradingScaleNum = SIn.Long(row["GradingScaleNum"].ToString());
            evaluation.OverallGradeShowing = SIn.String(row["OverallGradeShowing"].ToString());
            evaluation.OverallGradeNumber = SIn.Float(row["OverallGradeNumber"].ToString());
            evaluation.Notes = SIn.String(row["Notes"].ToString());
            retVal.Add(evaluation);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Evaluation> listEvaluations, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Evaluation";
        var table = new DataTable(tableName);
        table.Columns.Add("EvaluationNum");
        table.Columns.Add("InstructNum");
        table.Columns.Add("StudentNum");
        table.Columns.Add("SchoolCourseNum");
        table.Columns.Add("EvalTitle");
        table.Columns.Add("DateEval");
        table.Columns.Add("GradingScaleNum");
        table.Columns.Add("OverallGradeShowing");
        table.Columns.Add("OverallGradeNumber");
        table.Columns.Add("Notes");
        foreach (var evaluation in listEvaluations)
            table.Rows.Add(SOut.Long(evaluation.EvaluationNum), SOut.Long(evaluation.InstructNum), SOut.Long(evaluation.StudentNum), SOut.Long(evaluation.SchoolCourseNum), evaluation.EvalTitle, SOut.DateT(evaluation.DateEval, false), SOut.Long(evaluation.GradingScaleNum), evaluation.OverallGradeShowing, SOut.Float(evaluation.OverallGradeNumber), evaluation.Notes);
        return table;
    }

    public static long Insert(Evaluation evaluation)
    {
        return Insert(evaluation, false);
    }

    public static long Insert(Evaluation evaluation, bool useExistingPK)
    {
        var command = "INSERT INTO evaluation (";

        command += "InstructNum,StudentNum,SchoolCourseNum,EvalTitle,DateEval,GradingScaleNum,OverallGradeShowing,OverallGradeNumber,Notes) VALUES(";

        command +=
            SOut.Long(evaluation.InstructNum) + ","
                                              + SOut.Long(evaluation.StudentNum) + ","
                                              + SOut.Long(evaluation.SchoolCourseNum) + ","
                                              + "'" + SOut.String(evaluation.EvalTitle) + "',"
                                              + SOut.Date(evaluation.DateEval) + ","
                                              + SOut.Long(evaluation.GradingScaleNum) + ","
                                              + "'" + SOut.String(evaluation.OverallGradeShowing) + "',"
                                              + SOut.Float(evaluation.OverallGradeNumber) + ","
                                              + DbHelper.ParamChar + "paramNotes)";
        if (evaluation.Notes == null) evaluation.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluation.Notes));
        {
            evaluation.EvaluationNum = Db.NonQ(command, true, "EvaluationNum", "evaluation", paramNotes);
        }
        return evaluation.EvaluationNum;
    }

    public static long InsertNoCache(Evaluation evaluation)
    {
        return InsertNoCache(evaluation, false);
    }

    public static long InsertNoCache(Evaluation evaluation, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO evaluation (";
        if (isRandomKeys || useExistingPK) command += "EvaluationNum,";
        command += "InstructNum,StudentNum,SchoolCourseNum,EvalTitle,DateEval,GradingScaleNum,OverallGradeShowing,OverallGradeNumber,Notes) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(evaluation.EvaluationNum) + ",";
        command +=
            SOut.Long(evaluation.InstructNum) + ","
                                              + SOut.Long(evaluation.StudentNum) + ","
                                              + SOut.Long(evaluation.SchoolCourseNum) + ","
                                              + "'" + SOut.String(evaluation.EvalTitle) + "',"
                                              + SOut.Date(evaluation.DateEval) + ","
                                              + SOut.Long(evaluation.GradingScaleNum) + ","
                                              + "'" + SOut.String(evaluation.OverallGradeShowing) + "',"
                                              + SOut.Float(evaluation.OverallGradeNumber) + ","
                                              + DbHelper.ParamChar + "paramNotes)";
        if (evaluation.Notes == null) evaluation.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluation.Notes));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNotes);
        else
            evaluation.EvaluationNum = Db.NonQ(command, true, "EvaluationNum", "evaluation", paramNotes);
        return evaluation.EvaluationNum;
    }

    public static void Update(Evaluation evaluation)
    {
        var command = "UPDATE evaluation SET "
                      + "InstructNum        =  " + SOut.Long(evaluation.InstructNum) + ", "
                      + "StudentNum         =  " + SOut.Long(evaluation.StudentNum) + ", "
                      + "SchoolCourseNum    =  " + SOut.Long(evaluation.SchoolCourseNum) + ", "
                      + "EvalTitle          = '" + SOut.String(evaluation.EvalTitle) + "', "
                      + "DateEval           =  " + SOut.Date(evaluation.DateEval) + ", "
                      + "GradingScaleNum    =  " + SOut.Long(evaluation.GradingScaleNum) + ", "
                      + "OverallGradeShowing= '" + SOut.String(evaluation.OverallGradeShowing) + "', "
                      + "OverallGradeNumber =  " + SOut.Float(evaluation.OverallGradeNumber) + ", "
                      + "Notes              =  " + DbHelper.ParamChar + "paramNotes "
                      + "WHERE EvaluationNum = " + SOut.Long(evaluation.EvaluationNum);
        if (evaluation.Notes == null) evaluation.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluation.Notes));
        Db.NonQ(command, paramNotes);
    }

    public static bool Update(Evaluation evaluation, Evaluation oldEvaluation)
    {
        var command = "";
        if (evaluation.InstructNum != oldEvaluation.InstructNum)
        {
            if (command != "") command += ",";
            command += "InstructNum = " + SOut.Long(evaluation.InstructNum) + "";
        }

        if (evaluation.StudentNum != oldEvaluation.StudentNum)
        {
            if (command != "") command += ",";
            command += "StudentNum = " + SOut.Long(evaluation.StudentNum) + "";
        }

        if (evaluation.SchoolCourseNum != oldEvaluation.SchoolCourseNum)
        {
            if (command != "") command += ",";
            command += "SchoolCourseNum = " + SOut.Long(evaluation.SchoolCourseNum) + "";
        }

        if (evaluation.EvalTitle != oldEvaluation.EvalTitle)
        {
            if (command != "") command += ",";
            command += "EvalTitle = '" + SOut.String(evaluation.EvalTitle) + "'";
        }

        if (evaluation.DateEval.Date != oldEvaluation.DateEval.Date)
        {
            if (command != "") command += ",";
            command += "DateEval = " + SOut.Date(evaluation.DateEval) + "";
        }

        if (evaluation.GradingScaleNum != oldEvaluation.GradingScaleNum)
        {
            if (command != "") command += ",";
            command += "GradingScaleNum = " + SOut.Long(evaluation.GradingScaleNum) + "";
        }

        if (evaluation.OverallGradeShowing != oldEvaluation.OverallGradeShowing)
        {
            if (command != "") command += ",";
            command += "OverallGradeShowing = '" + SOut.String(evaluation.OverallGradeShowing) + "'";
        }

        if (evaluation.OverallGradeNumber != oldEvaluation.OverallGradeNumber)
        {
            if (command != "") command += ",";
            command += "OverallGradeNumber = " + SOut.Float(evaluation.OverallGradeNumber) + "";
        }

        if (evaluation.Notes != oldEvaluation.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = " + DbHelper.ParamChar + "paramNotes";
        }

        if (command == "") return false;
        if (evaluation.Notes == null) evaluation.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluation.Notes));
        command = "UPDATE evaluation SET " + command
                                           + " WHERE EvaluationNum = " + SOut.Long(evaluation.EvaluationNum);
        Db.NonQ(command, paramNotes);
        return true;
    }

    public static bool UpdateComparison(Evaluation evaluation, Evaluation oldEvaluation)
    {
        if (evaluation.InstructNum != oldEvaluation.InstructNum) return true;
        if (evaluation.StudentNum != oldEvaluation.StudentNum) return true;
        if (evaluation.SchoolCourseNum != oldEvaluation.SchoolCourseNum) return true;
        if (evaluation.EvalTitle != oldEvaluation.EvalTitle) return true;
        if (evaluation.DateEval.Date != oldEvaluation.DateEval.Date) return true;
        if (evaluation.GradingScaleNum != oldEvaluation.GradingScaleNum) return true;
        if (evaluation.OverallGradeShowing != oldEvaluation.OverallGradeShowing) return true;
        if (evaluation.OverallGradeNumber != oldEvaluation.OverallGradeNumber) return true;
        if (evaluation.Notes != oldEvaluation.Notes) return true;
        return false;
    }

    public static void Delete(long evaluationNum)
    {
        var command = "DELETE FROM evaluation "
                      + "WHERE EvaluationNum = " + SOut.Long(evaluationNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEvaluationNums)
    {
        if (listEvaluationNums == null || listEvaluationNums.Count == 0) return;
        var command = "DELETE FROM evaluation "
                      + "WHERE EvaluationNum IN(" + string.Join(",", listEvaluationNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}