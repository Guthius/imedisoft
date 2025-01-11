#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EvaluationCriterionDefCrud
{
    public static EvaluationCriterionDef SelectOne(long evaluationCriterionDefNum)
    {
        var command = "SELECT * FROM evaluationcriteriondef "
                      + "WHERE EvaluationCriterionDefNum = " + SOut.Long(evaluationCriterionDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EvaluationCriterionDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EvaluationCriterionDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EvaluationCriterionDef> TableToList(DataTable table)
    {
        var retVal = new List<EvaluationCriterionDef>();
        EvaluationCriterionDef evaluationCriterionDef;
        foreach (DataRow row in table.Rows)
        {
            evaluationCriterionDef = new EvaluationCriterionDef();
            evaluationCriterionDef.EvaluationCriterionDefNum = SIn.Long(row["EvaluationCriterionDefNum"].ToString());
            evaluationCriterionDef.EvaluationDefNum = SIn.Long(row["EvaluationDefNum"].ToString());
            evaluationCriterionDef.CriterionDescript = SIn.String(row["CriterionDescript"].ToString());
            evaluationCriterionDef.IsCategoryName = SIn.Bool(row["IsCategoryName"].ToString());
            evaluationCriterionDef.GradingScaleNum = SIn.Long(row["GradingScaleNum"].ToString());
            evaluationCriterionDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            evaluationCriterionDef.MaxPointsPoss = SIn.Float(row["MaxPointsPoss"].ToString());
            retVal.Add(evaluationCriterionDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EvaluationCriterionDef> listEvaluationCriterionDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EvaluationCriterionDef";
        var table = new DataTable(tableName);
        table.Columns.Add("EvaluationCriterionDefNum");
        table.Columns.Add("EvaluationDefNum");
        table.Columns.Add("CriterionDescript");
        table.Columns.Add("IsCategoryName");
        table.Columns.Add("GradingScaleNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("MaxPointsPoss");
        foreach (var evaluationCriterionDef in listEvaluationCriterionDefs)
            table.Rows.Add(SOut.Long(evaluationCriterionDef.EvaluationCriterionDefNum), SOut.Long(evaluationCriterionDef.EvaluationDefNum), evaluationCriterionDef.CriterionDescript, SOut.Bool(evaluationCriterionDef.IsCategoryName), SOut.Long(evaluationCriterionDef.GradingScaleNum), SOut.Int(evaluationCriterionDef.ItemOrder), SOut.Float(evaluationCriterionDef.MaxPointsPoss));
        return table;
    }

    public static long Insert(EvaluationCriterionDef evaluationCriterionDef)
    {
        return Insert(evaluationCriterionDef, false);
    }

    public static long Insert(EvaluationCriterionDef evaluationCriterionDef, bool useExistingPK)
    {
        var command = "INSERT INTO evaluationcriteriondef (";

        command += "EvaluationDefNum,CriterionDescript,IsCategoryName,GradingScaleNum,ItemOrder,MaxPointsPoss) VALUES(";

        command +=
            SOut.Long(evaluationCriterionDef.EvaluationDefNum) + ","
                                                               + "'" + SOut.String(evaluationCriterionDef.CriterionDescript) + "',"
                                                               + SOut.Bool(evaluationCriterionDef.IsCategoryName) + ","
                                                               + SOut.Long(evaluationCriterionDef.GradingScaleNum) + ","
                                                               + SOut.Int(evaluationCriterionDef.ItemOrder) + ","
                                                               + SOut.Float(evaluationCriterionDef.MaxPointsPoss) + ")";
        {
            evaluationCriterionDef.EvaluationCriterionDefNum = Db.NonQ(command, true, "EvaluationCriterionDefNum", "evaluationCriterionDef");
        }
        return evaluationCriterionDef.EvaluationCriterionDefNum;
    }

    public static long InsertNoCache(EvaluationCriterionDef evaluationCriterionDef)
    {
        return InsertNoCache(evaluationCriterionDef, false);
    }

    public static long InsertNoCache(EvaluationCriterionDef evaluationCriterionDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO evaluationcriteriondef (";
        if (isRandomKeys || useExistingPK) command += "EvaluationCriterionDefNum,";
        command += "EvaluationDefNum,CriterionDescript,IsCategoryName,GradingScaleNum,ItemOrder,MaxPointsPoss) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(evaluationCriterionDef.EvaluationCriterionDefNum) + ",";
        command +=
            SOut.Long(evaluationCriterionDef.EvaluationDefNum) + ","
                                                               + "'" + SOut.String(evaluationCriterionDef.CriterionDescript) + "',"
                                                               + SOut.Bool(evaluationCriterionDef.IsCategoryName) + ","
                                                               + SOut.Long(evaluationCriterionDef.GradingScaleNum) + ","
                                                               + SOut.Int(evaluationCriterionDef.ItemOrder) + ","
                                                               + SOut.Float(evaluationCriterionDef.MaxPointsPoss) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            evaluationCriterionDef.EvaluationCriterionDefNum = Db.NonQ(command, true, "EvaluationCriterionDefNum", "evaluationCriterionDef");
        return evaluationCriterionDef.EvaluationCriterionDefNum;
    }

    public static void Update(EvaluationCriterionDef evaluationCriterionDef)
    {
        var command = "UPDATE evaluationcriteriondef SET "
                      + "EvaluationDefNum         =  " + SOut.Long(evaluationCriterionDef.EvaluationDefNum) + ", "
                      + "CriterionDescript        = '" + SOut.String(evaluationCriterionDef.CriterionDescript) + "', "
                      + "IsCategoryName           =  " + SOut.Bool(evaluationCriterionDef.IsCategoryName) + ", "
                      + "GradingScaleNum          =  " + SOut.Long(evaluationCriterionDef.GradingScaleNum) + ", "
                      + "ItemOrder                =  " + SOut.Int(evaluationCriterionDef.ItemOrder) + ", "
                      + "MaxPointsPoss            =  " + SOut.Float(evaluationCriterionDef.MaxPointsPoss) + " "
                      + "WHERE EvaluationCriterionDefNum = " + SOut.Long(evaluationCriterionDef.EvaluationCriterionDefNum);
        Db.NonQ(command);
    }

    public static bool Update(EvaluationCriterionDef evaluationCriterionDef, EvaluationCriterionDef oldEvaluationCriterionDef)
    {
        var command = "";
        if (evaluationCriterionDef.EvaluationDefNum != oldEvaluationCriterionDef.EvaluationDefNum)
        {
            if (command != "") command += ",";
            command += "EvaluationDefNum = " + SOut.Long(evaluationCriterionDef.EvaluationDefNum) + "";
        }

        if (evaluationCriterionDef.CriterionDescript != oldEvaluationCriterionDef.CriterionDescript)
        {
            if (command != "") command += ",";
            command += "CriterionDescript = '" + SOut.String(evaluationCriterionDef.CriterionDescript) + "'";
        }

        if (evaluationCriterionDef.IsCategoryName != oldEvaluationCriterionDef.IsCategoryName)
        {
            if (command != "") command += ",";
            command += "IsCategoryName = " + SOut.Bool(evaluationCriterionDef.IsCategoryName) + "";
        }

        if (evaluationCriterionDef.GradingScaleNum != oldEvaluationCriterionDef.GradingScaleNum)
        {
            if (command != "") command += ",";
            command += "GradingScaleNum = " + SOut.Long(evaluationCriterionDef.GradingScaleNum) + "";
        }

        if (evaluationCriterionDef.ItemOrder != oldEvaluationCriterionDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(evaluationCriterionDef.ItemOrder) + "";
        }

        if (evaluationCriterionDef.MaxPointsPoss != oldEvaluationCriterionDef.MaxPointsPoss)
        {
            if (command != "") command += ",";
            command += "MaxPointsPoss = " + SOut.Float(evaluationCriterionDef.MaxPointsPoss) + "";
        }

        if (command == "") return false;
        command = "UPDATE evaluationcriteriondef SET " + command
                                                       + " WHERE EvaluationCriterionDefNum = " + SOut.Long(evaluationCriterionDef.EvaluationCriterionDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EvaluationCriterionDef evaluationCriterionDef, EvaluationCriterionDef oldEvaluationCriterionDef)
    {
        if (evaluationCriterionDef.EvaluationDefNum != oldEvaluationCriterionDef.EvaluationDefNum) return true;
        if (evaluationCriterionDef.CriterionDescript != oldEvaluationCriterionDef.CriterionDescript) return true;
        if (evaluationCriterionDef.IsCategoryName != oldEvaluationCriterionDef.IsCategoryName) return true;
        if (evaluationCriterionDef.GradingScaleNum != oldEvaluationCriterionDef.GradingScaleNum) return true;
        if (evaluationCriterionDef.ItemOrder != oldEvaluationCriterionDef.ItemOrder) return true;
        if (evaluationCriterionDef.MaxPointsPoss != oldEvaluationCriterionDef.MaxPointsPoss) return true;
        return false;
    }

    public static void Delete(long evaluationCriterionDefNum)
    {
        var command = "DELETE FROM evaluationcriteriondef "
                      + "WHERE EvaluationCriterionDefNum = " + SOut.Long(evaluationCriterionDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEvaluationCriterionDefNums)
    {
        if (listEvaluationCriterionDefNums == null || listEvaluationCriterionDefNums.Count == 0) return;
        var command = "DELETE FROM evaluationcriteriondef "
                      + "WHERE EvaluationCriterionDefNum IN(" + string.Join(",", listEvaluationCriterionDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}