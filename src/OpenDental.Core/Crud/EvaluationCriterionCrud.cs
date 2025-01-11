#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EvaluationCriterionCrud
{
    public static EvaluationCriterion SelectOne(long evaluationCriterionNum)
    {
        var command = "SELECT * FROM evaluationcriterion "
                      + "WHERE EvaluationCriterionNum = " + SOut.Long(evaluationCriterionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EvaluationCriterion SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EvaluationCriterion> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EvaluationCriterion> TableToList(DataTable table)
    {
        var retVal = new List<EvaluationCriterion>();
        EvaluationCriterion evaluationCriterion;
        foreach (DataRow row in table.Rows)
        {
            evaluationCriterion = new EvaluationCriterion();
            evaluationCriterion.EvaluationCriterionNum = SIn.Long(row["EvaluationCriterionNum"].ToString());
            evaluationCriterion.EvaluationNum = SIn.Long(row["EvaluationNum"].ToString());
            evaluationCriterion.CriterionDescript = SIn.String(row["CriterionDescript"].ToString());
            evaluationCriterion.IsCategoryName = SIn.Bool(row["IsCategoryName"].ToString());
            evaluationCriterion.GradingScaleNum = SIn.Long(row["GradingScaleNum"].ToString());
            evaluationCriterion.GradeShowing = SIn.String(row["GradeShowing"].ToString());
            evaluationCriterion.GradeNumber = SIn.Float(row["GradeNumber"].ToString());
            evaluationCriterion.Notes = SIn.String(row["Notes"].ToString());
            evaluationCriterion.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            evaluationCriterion.MaxPointsPoss = SIn.Float(row["MaxPointsPoss"].ToString());
            retVal.Add(evaluationCriterion);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EvaluationCriterion> listEvaluationCriterions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EvaluationCriterion";
        var table = new DataTable(tableName);
        table.Columns.Add("EvaluationCriterionNum");
        table.Columns.Add("EvaluationNum");
        table.Columns.Add("CriterionDescript");
        table.Columns.Add("IsCategoryName");
        table.Columns.Add("GradingScaleNum");
        table.Columns.Add("GradeShowing");
        table.Columns.Add("GradeNumber");
        table.Columns.Add("Notes");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("MaxPointsPoss");
        foreach (var evaluationCriterion in listEvaluationCriterions)
            table.Rows.Add(SOut.Long(evaluationCriterion.EvaluationCriterionNum), SOut.Long(evaluationCriterion.EvaluationNum), evaluationCriterion.CriterionDescript, SOut.Bool(evaluationCriterion.IsCategoryName), SOut.Long(evaluationCriterion.GradingScaleNum), evaluationCriterion.GradeShowing, SOut.Float(evaluationCriterion.GradeNumber), evaluationCriterion.Notes, SOut.Int(evaluationCriterion.ItemOrder), SOut.Float(evaluationCriterion.MaxPointsPoss));
        return table;
    }

    public static long Insert(EvaluationCriterion evaluationCriterion)
    {
        return Insert(evaluationCriterion, false);
    }

    public static long Insert(EvaluationCriterion evaluationCriterion, bool useExistingPK)
    {
        var command = "INSERT INTO evaluationcriterion (";

        command += "EvaluationNum,CriterionDescript,IsCategoryName,GradingScaleNum,GradeShowing,GradeNumber,Notes,ItemOrder,MaxPointsPoss) VALUES(";

        command +=
            SOut.Long(evaluationCriterion.EvaluationNum) + ","
                                                         + "'" + SOut.String(evaluationCriterion.CriterionDescript) + "',"
                                                         + SOut.Bool(evaluationCriterion.IsCategoryName) + ","
                                                         + SOut.Long(evaluationCriterion.GradingScaleNum) + ","
                                                         + "'" + SOut.String(evaluationCriterion.GradeShowing) + "',"
                                                         + SOut.Float(evaluationCriterion.GradeNumber) + ","
                                                         + DbHelper.ParamChar + "paramNotes,"
                                                         + SOut.Int(evaluationCriterion.ItemOrder) + ","
                                                         + SOut.Float(evaluationCriterion.MaxPointsPoss) + ")";
        if (evaluationCriterion.Notes == null) evaluationCriterion.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluationCriterion.Notes));
        {
            evaluationCriterion.EvaluationCriterionNum = Db.NonQ(command, true, "EvaluationCriterionNum", "evaluationCriterion", paramNotes);
        }
        return evaluationCriterion.EvaluationCriterionNum;
    }

    public static long InsertNoCache(EvaluationCriterion evaluationCriterion)
    {
        return InsertNoCache(evaluationCriterion, false);
    }

    public static long InsertNoCache(EvaluationCriterion evaluationCriterion, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO evaluationcriterion (";
        if (isRandomKeys || useExistingPK) command += "EvaluationCriterionNum,";
        command += "EvaluationNum,CriterionDescript,IsCategoryName,GradingScaleNum,GradeShowing,GradeNumber,Notes,ItemOrder,MaxPointsPoss) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(evaluationCriterion.EvaluationCriterionNum) + ",";
        command +=
            SOut.Long(evaluationCriterion.EvaluationNum) + ","
                                                         + "'" + SOut.String(evaluationCriterion.CriterionDescript) + "',"
                                                         + SOut.Bool(evaluationCriterion.IsCategoryName) + ","
                                                         + SOut.Long(evaluationCriterion.GradingScaleNum) + ","
                                                         + "'" + SOut.String(evaluationCriterion.GradeShowing) + "',"
                                                         + SOut.Float(evaluationCriterion.GradeNumber) + ","
                                                         + DbHelper.ParamChar + "paramNotes,"
                                                         + SOut.Int(evaluationCriterion.ItemOrder) + ","
                                                         + SOut.Float(evaluationCriterion.MaxPointsPoss) + ")";
        if (evaluationCriterion.Notes == null) evaluationCriterion.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluationCriterion.Notes));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNotes);
        else
            evaluationCriterion.EvaluationCriterionNum = Db.NonQ(command, true, "EvaluationCriterionNum", "evaluationCriterion", paramNotes);
        return evaluationCriterion.EvaluationCriterionNum;
    }

    public static void Update(EvaluationCriterion evaluationCriterion)
    {
        var command = "UPDATE evaluationcriterion SET "
                      + "EvaluationNum         =  " + SOut.Long(evaluationCriterion.EvaluationNum) + ", "
                      + "CriterionDescript     = '" + SOut.String(evaluationCriterion.CriterionDescript) + "', "
                      + "IsCategoryName        =  " + SOut.Bool(evaluationCriterion.IsCategoryName) + ", "
                      + "GradingScaleNum       =  " + SOut.Long(evaluationCriterion.GradingScaleNum) + ", "
                      + "GradeShowing          = '" + SOut.String(evaluationCriterion.GradeShowing) + "', "
                      + "GradeNumber           =  " + SOut.Float(evaluationCriterion.GradeNumber) + ", "
                      + "Notes                 =  " + DbHelper.ParamChar + "paramNotes, "
                      + "ItemOrder             =  " + SOut.Int(evaluationCriterion.ItemOrder) + ", "
                      + "MaxPointsPoss         =  " + SOut.Float(evaluationCriterion.MaxPointsPoss) + " "
                      + "WHERE EvaluationCriterionNum = " + SOut.Long(evaluationCriterion.EvaluationCriterionNum);
        if (evaluationCriterion.Notes == null) evaluationCriterion.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluationCriterion.Notes));
        Db.NonQ(command, paramNotes);
    }

    public static bool Update(EvaluationCriterion evaluationCriterion, EvaluationCriterion oldEvaluationCriterion)
    {
        var command = "";
        if (evaluationCriterion.EvaluationNum != oldEvaluationCriterion.EvaluationNum)
        {
            if (command != "") command += ",";
            command += "EvaluationNum = " + SOut.Long(evaluationCriterion.EvaluationNum) + "";
        }

        if (evaluationCriterion.CriterionDescript != oldEvaluationCriterion.CriterionDescript)
        {
            if (command != "") command += ",";
            command += "CriterionDescript = '" + SOut.String(evaluationCriterion.CriterionDescript) + "'";
        }

        if (evaluationCriterion.IsCategoryName != oldEvaluationCriterion.IsCategoryName)
        {
            if (command != "") command += ",";
            command += "IsCategoryName = " + SOut.Bool(evaluationCriterion.IsCategoryName) + "";
        }

        if (evaluationCriterion.GradingScaleNum != oldEvaluationCriterion.GradingScaleNum)
        {
            if (command != "") command += ",";
            command += "GradingScaleNum = " + SOut.Long(evaluationCriterion.GradingScaleNum) + "";
        }

        if (evaluationCriterion.GradeShowing != oldEvaluationCriterion.GradeShowing)
        {
            if (command != "") command += ",";
            command += "GradeShowing = '" + SOut.String(evaluationCriterion.GradeShowing) + "'";
        }

        if (evaluationCriterion.GradeNumber != oldEvaluationCriterion.GradeNumber)
        {
            if (command != "") command += ",";
            command += "GradeNumber = " + SOut.Float(evaluationCriterion.GradeNumber) + "";
        }

        if (evaluationCriterion.Notes != oldEvaluationCriterion.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = " + DbHelper.ParamChar + "paramNotes";
        }

        if (evaluationCriterion.ItemOrder != oldEvaluationCriterion.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(evaluationCriterion.ItemOrder) + "";
        }

        if (evaluationCriterion.MaxPointsPoss != oldEvaluationCriterion.MaxPointsPoss)
        {
            if (command != "") command += ",";
            command += "MaxPointsPoss = " + SOut.Float(evaluationCriterion.MaxPointsPoss) + "";
        }

        if (command == "") return false;
        if (evaluationCriterion.Notes == null) evaluationCriterion.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(evaluationCriterion.Notes));
        command = "UPDATE evaluationcriterion SET " + command
                                                    + " WHERE EvaluationCriterionNum = " + SOut.Long(evaluationCriterion.EvaluationCriterionNum);
        Db.NonQ(command, paramNotes);
        return true;
    }

    public static bool UpdateComparison(EvaluationCriterion evaluationCriterion, EvaluationCriterion oldEvaluationCriterion)
    {
        if (evaluationCriterion.EvaluationNum != oldEvaluationCriterion.EvaluationNum) return true;
        if (evaluationCriterion.CriterionDescript != oldEvaluationCriterion.CriterionDescript) return true;
        if (evaluationCriterion.IsCategoryName != oldEvaluationCriterion.IsCategoryName) return true;
        if (evaluationCriterion.GradingScaleNum != oldEvaluationCriterion.GradingScaleNum) return true;
        if (evaluationCriterion.GradeShowing != oldEvaluationCriterion.GradeShowing) return true;
        if (evaluationCriterion.GradeNumber != oldEvaluationCriterion.GradeNumber) return true;
        if (evaluationCriterion.Notes != oldEvaluationCriterion.Notes) return true;
        if (evaluationCriterion.ItemOrder != oldEvaluationCriterion.ItemOrder) return true;
        if (evaluationCriterion.MaxPointsPoss != oldEvaluationCriterion.MaxPointsPoss) return true;
        return false;
    }

    public static void Delete(long evaluationCriterionNum)
    {
        var command = "DELETE FROM evaluationcriterion "
                      + "WHERE EvaluationCriterionNum = " + SOut.Long(evaluationCriterionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEvaluationCriterionNums)
    {
        if (listEvaluationCriterionNums == null || listEvaluationCriterionNums.Count == 0) return;
        var command = "DELETE FROM evaluationcriterion "
                      + "WHERE EvaluationCriterionNum IN(" + string.Join(",", listEvaluationCriterionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}