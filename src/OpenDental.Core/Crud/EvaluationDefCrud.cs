#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EvaluationDefCrud
{
    public static EvaluationDef SelectOne(long evaluationDefNum)
    {
        var command = "SELECT * FROM evaluationdef "
                      + "WHERE EvaluationDefNum = " + SOut.Long(evaluationDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EvaluationDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EvaluationDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EvaluationDef> TableToList(DataTable table)
    {
        var retVal = new List<EvaluationDef>();
        EvaluationDef evaluationDef;
        foreach (DataRow row in table.Rows)
        {
            evaluationDef = new EvaluationDef();
            evaluationDef.EvaluationDefNum = SIn.Long(row["EvaluationDefNum"].ToString());
            evaluationDef.SchoolCourseNum = SIn.Long(row["SchoolCourseNum"].ToString());
            evaluationDef.EvalTitle = SIn.String(row["EvalTitle"].ToString());
            evaluationDef.GradingScaleNum = SIn.Long(row["GradingScaleNum"].ToString());
            retVal.Add(evaluationDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EvaluationDef> listEvaluationDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EvaluationDef";
        var table = new DataTable(tableName);
        table.Columns.Add("EvaluationDefNum");
        table.Columns.Add("SchoolCourseNum");
        table.Columns.Add("EvalTitle");
        table.Columns.Add("GradingScaleNum");
        foreach (var evaluationDef in listEvaluationDefs)
            table.Rows.Add(SOut.Long(evaluationDef.EvaluationDefNum), SOut.Long(evaluationDef.SchoolCourseNum), evaluationDef.EvalTitle, SOut.Long(evaluationDef.GradingScaleNum));
        return table;
    }

    public static long Insert(EvaluationDef evaluationDef)
    {
        return Insert(evaluationDef, false);
    }

    public static long Insert(EvaluationDef evaluationDef, bool useExistingPK)
    {
        var command = "INSERT INTO evaluationdef (";

        command += "SchoolCourseNum,EvalTitle,GradingScaleNum) VALUES(";

        command +=
            SOut.Long(evaluationDef.SchoolCourseNum) + ","
                                                     + "'" + SOut.String(evaluationDef.EvalTitle) + "',"
                                                     + SOut.Long(evaluationDef.GradingScaleNum) + ")";
        {
            evaluationDef.EvaluationDefNum = Db.NonQ(command, true, "EvaluationDefNum", "evaluationDef");
        }
        return evaluationDef.EvaluationDefNum;
    }

    public static long InsertNoCache(EvaluationDef evaluationDef)
    {
        return InsertNoCache(evaluationDef, false);
    }

    public static long InsertNoCache(EvaluationDef evaluationDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO evaluationdef (";
        if (isRandomKeys || useExistingPK) command += "EvaluationDefNum,";
        command += "SchoolCourseNum,EvalTitle,GradingScaleNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(evaluationDef.EvaluationDefNum) + ",";
        command +=
            SOut.Long(evaluationDef.SchoolCourseNum) + ","
                                                     + "'" + SOut.String(evaluationDef.EvalTitle) + "',"
                                                     + SOut.Long(evaluationDef.GradingScaleNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            evaluationDef.EvaluationDefNum = Db.NonQ(command, true, "EvaluationDefNum", "evaluationDef");
        return evaluationDef.EvaluationDefNum;
    }

    public static void Update(EvaluationDef evaluationDef)
    {
        var command = "UPDATE evaluationdef SET "
                      + "SchoolCourseNum =  " + SOut.Long(evaluationDef.SchoolCourseNum) + ", "
                      + "EvalTitle       = '" + SOut.String(evaluationDef.EvalTitle) + "', "
                      + "GradingScaleNum =  " + SOut.Long(evaluationDef.GradingScaleNum) + " "
                      + "WHERE EvaluationDefNum = " + SOut.Long(evaluationDef.EvaluationDefNum);
        Db.NonQ(command);
    }

    public static bool Update(EvaluationDef evaluationDef, EvaluationDef oldEvaluationDef)
    {
        var command = "";
        if (evaluationDef.SchoolCourseNum != oldEvaluationDef.SchoolCourseNum)
        {
            if (command != "") command += ",";
            command += "SchoolCourseNum = " + SOut.Long(evaluationDef.SchoolCourseNum) + "";
        }

        if (evaluationDef.EvalTitle != oldEvaluationDef.EvalTitle)
        {
            if (command != "") command += ",";
            command += "EvalTitle = '" + SOut.String(evaluationDef.EvalTitle) + "'";
        }

        if (evaluationDef.GradingScaleNum != oldEvaluationDef.GradingScaleNum)
        {
            if (command != "") command += ",";
            command += "GradingScaleNum = " + SOut.Long(evaluationDef.GradingScaleNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE evaluationdef SET " + command
                                              + " WHERE EvaluationDefNum = " + SOut.Long(evaluationDef.EvaluationDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EvaluationDef evaluationDef, EvaluationDef oldEvaluationDef)
    {
        if (evaluationDef.SchoolCourseNum != oldEvaluationDef.SchoolCourseNum) return true;
        if (evaluationDef.EvalTitle != oldEvaluationDef.EvalTitle) return true;
        if (evaluationDef.GradingScaleNum != oldEvaluationDef.GradingScaleNum) return true;
        return false;
    }

    public static void Delete(long evaluationDefNum)
    {
        var command = "DELETE FROM evaluationdef "
                      + "WHERE EvaluationDefNum = " + SOut.Long(evaluationDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEvaluationDefNums)
    {
        if (listEvaluationDefNums == null || listEvaluationDefNums.Count == 0) return;
        var command = "DELETE FROM evaluationdef "
                      + "WHERE EvaluationDefNum IN(" + string.Join(",", listEvaluationDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}