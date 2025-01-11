#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class VitalsignCrud
{
    public static Vitalsign SelectOne(long vitalsignNum)
    {
        var command = "SELECT * FROM vitalsign "
                      + "WHERE VitalsignNum = " + SOut.Long(vitalsignNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Vitalsign SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Vitalsign> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Vitalsign> TableToList(DataTable table)
    {
        var retVal = new List<Vitalsign>();
        Vitalsign vitalsign;
        foreach (DataRow row in table.Rows)
        {
            vitalsign = new Vitalsign();
            vitalsign.VitalsignNum = SIn.Long(row["VitalsignNum"].ToString());
            vitalsign.PatNum = SIn.Long(row["PatNum"].ToString());
            vitalsign.Height = SIn.Float(row["Height"].ToString());
            vitalsign.Weight = SIn.Float(row["Weight"].ToString());
            vitalsign.BpSystolic = SIn.Int(row["BpSystolic"].ToString());
            vitalsign.BpDiastolic = SIn.Int(row["BpDiastolic"].ToString());
            vitalsign.DateTaken = SIn.Date(row["DateTaken"].ToString());
            vitalsign.HasFollowupPlan = SIn.Bool(row["HasFollowupPlan"].ToString());
            vitalsign.IsIneligible = SIn.Bool(row["IsIneligible"].ToString());
            vitalsign.Documentation = SIn.String(row["Documentation"].ToString());
            vitalsign.ChildGotNutrition = SIn.Bool(row["ChildGotNutrition"].ToString());
            vitalsign.ChildGotPhysCouns = SIn.Bool(row["ChildGotPhysCouns"].ToString());
            vitalsign.WeightCode = SIn.String(row["WeightCode"].ToString());
            vitalsign.HeightExamCode = SIn.String(row["HeightExamCode"].ToString());
            vitalsign.WeightExamCode = SIn.String(row["WeightExamCode"].ToString());
            vitalsign.BMIExamCode = SIn.String(row["BMIExamCode"].ToString());
            vitalsign.EhrNotPerformedNum = SIn.Long(row["EhrNotPerformedNum"].ToString());
            vitalsign.PregDiseaseNum = SIn.Long(row["PregDiseaseNum"].ToString());
            vitalsign.BMIPercentile = SIn.Int(row["BMIPercentile"].ToString());
            vitalsign.Pulse = SIn.Int(row["Pulse"].ToString());
            retVal.Add(vitalsign);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Vitalsign> listVitalsigns, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Vitalsign";
        var table = new DataTable(tableName);
        table.Columns.Add("VitalsignNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("Height");
        table.Columns.Add("Weight");
        table.Columns.Add("BpSystolic");
        table.Columns.Add("BpDiastolic");
        table.Columns.Add("DateTaken");
        table.Columns.Add("HasFollowupPlan");
        table.Columns.Add("IsIneligible");
        table.Columns.Add("Documentation");
        table.Columns.Add("ChildGotNutrition");
        table.Columns.Add("ChildGotPhysCouns");
        table.Columns.Add("WeightCode");
        table.Columns.Add("HeightExamCode");
        table.Columns.Add("WeightExamCode");
        table.Columns.Add("BMIExamCode");
        table.Columns.Add("EhrNotPerformedNum");
        table.Columns.Add("PregDiseaseNum");
        table.Columns.Add("BMIPercentile");
        table.Columns.Add("Pulse");
        foreach (var vitalsign in listVitalsigns)
            table.Rows.Add(SOut.Long(vitalsign.VitalsignNum), SOut.Long(vitalsign.PatNum), SOut.Float(vitalsign.Height), SOut.Float(vitalsign.Weight), SOut.Int(vitalsign.BpSystolic), SOut.Int(vitalsign.BpDiastolic), SOut.DateT(vitalsign.DateTaken, false), SOut.Bool(vitalsign.HasFollowupPlan), SOut.Bool(vitalsign.IsIneligible), vitalsign.Documentation, SOut.Bool(vitalsign.ChildGotNutrition), SOut.Bool(vitalsign.ChildGotPhysCouns), vitalsign.WeightCode, vitalsign.HeightExamCode, vitalsign.WeightExamCode, vitalsign.BMIExamCode, SOut.Long(vitalsign.EhrNotPerformedNum), SOut.Long(vitalsign.PregDiseaseNum), SOut.Int(vitalsign.BMIPercentile), SOut.Int(vitalsign.Pulse));
        return table;
    }

    public static long Insert(Vitalsign vitalsign)
    {
        return Insert(vitalsign, false);
    }

    public static long Insert(Vitalsign vitalsign, bool useExistingPK)
    {
        var command = "INSERT INTO vitalsign (";

        command += "PatNum,Height,Weight,BpSystolic,BpDiastolic,DateTaken,HasFollowupPlan,IsIneligible,Documentation,ChildGotNutrition,ChildGotPhysCouns,WeightCode,HeightExamCode,WeightExamCode,BMIExamCode,EhrNotPerformedNum,PregDiseaseNum,BMIPercentile,Pulse) VALUES(";

        command +=
            SOut.Long(vitalsign.PatNum) + ","
                                        + SOut.Float(vitalsign.Height) + ","
                                        + SOut.Float(vitalsign.Weight) + ","
                                        + SOut.Int(vitalsign.BpSystolic) + ","
                                        + SOut.Int(vitalsign.BpDiastolic) + ","
                                        + SOut.Date(vitalsign.DateTaken) + ","
                                        + SOut.Bool(vitalsign.HasFollowupPlan) + ","
                                        + SOut.Bool(vitalsign.IsIneligible) + ","
                                        + DbHelper.ParamChar + "paramDocumentation,"
                                        + SOut.Bool(vitalsign.ChildGotNutrition) + ","
                                        + SOut.Bool(vitalsign.ChildGotPhysCouns) + ","
                                        + "'" + SOut.String(vitalsign.WeightCode) + "',"
                                        + "'" + SOut.String(vitalsign.HeightExamCode) + "',"
                                        + "'" + SOut.String(vitalsign.WeightExamCode) + "',"
                                        + "'" + SOut.String(vitalsign.BMIExamCode) + "',"
                                        + SOut.Long(vitalsign.EhrNotPerformedNum) + ","
                                        + SOut.Long(vitalsign.PregDiseaseNum) + ","
                                        + SOut.Int(vitalsign.BMIPercentile) + ","
                                        + SOut.Int(vitalsign.Pulse) + ")";
        if (vitalsign.Documentation == null) vitalsign.Documentation = "";
        var paramDocumentation = new OdSqlParameter("paramDocumentation", OdDbType.Text, SOut.StringParam(vitalsign.Documentation));
        {
            vitalsign.VitalsignNum = Db.NonQ(command, true, "VitalsignNum", "vitalsign", paramDocumentation);
        }
        return vitalsign.VitalsignNum;
    }

    public static long InsertNoCache(Vitalsign vitalsign)
    {
        return InsertNoCache(vitalsign, false);
    }

    public static long InsertNoCache(Vitalsign vitalsign, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO vitalsign (";
        if (isRandomKeys || useExistingPK) command += "VitalsignNum,";
        command += "PatNum,Height,Weight,BpSystolic,BpDiastolic,DateTaken,HasFollowupPlan,IsIneligible,Documentation,ChildGotNutrition,ChildGotPhysCouns,WeightCode,HeightExamCode,WeightExamCode,BMIExamCode,EhrNotPerformedNum,PregDiseaseNum,BMIPercentile,Pulse) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(vitalsign.VitalsignNum) + ",";
        command +=
            SOut.Long(vitalsign.PatNum) + ","
                                        + SOut.Float(vitalsign.Height) + ","
                                        + SOut.Float(vitalsign.Weight) + ","
                                        + SOut.Int(vitalsign.BpSystolic) + ","
                                        + SOut.Int(vitalsign.BpDiastolic) + ","
                                        + SOut.Date(vitalsign.DateTaken) + ","
                                        + SOut.Bool(vitalsign.HasFollowupPlan) + ","
                                        + SOut.Bool(vitalsign.IsIneligible) + ","
                                        + DbHelper.ParamChar + "paramDocumentation,"
                                        + SOut.Bool(vitalsign.ChildGotNutrition) + ","
                                        + SOut.Bool(vitalsign.ChildGotPhysCouns) + ","
                                        + "'" + SOut.String(vitalsign.WeightCode) + "',"
                                        + "'" + SOut.String(vitalsign.HeightExamCode) + "',"
                                        + "'" + SOut.String(vitalsign.WeightExamCode) + "',"
                                        + "'" + SOut.String(vitalsign.BMIExamCode) + "',"
                                        + SOut.Long(vitalsign.EhrNotPerformedNum) + ","
                                        + SOut.Long(vitalsign.PregDiseaseNum) + ","
                                        + SOut.Int(vitalsign.BMIPercentile) + ","
                                        + SOut.Int(vitalsign.Pulse) + ")";
        if (vitalsign.Documentation == null) vitalsign.Documentation = "";
        var paramDocumentation = new OdSqlParameter("paramDocumentation", OdDbType.Text, SOut.StringParam(vitalsign.Documentation));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDocumentation);
        else
            vitalsign.VitalsignNum = Db.NonQ(command, true, "VitalsignNum", "vitalsign", paramDocumentation);
        return vitalsign.VitalsignNum;
    }

    public static void Update(Vitalsign vitalsign)
    {
        var command = "UPDATE vitalsign SET "
                      + "PatNum            =  " + SOut.Long(vitalsign.PatNum) + ", "
                      + "Height            =  " + SOut.Float(vitalsign.Height) + ", "
                      + "Weight            =  " + SOut.Float(vitalsign.Weight) + ", "
                      + "BpSystolic        =  " + SOut.Int(vitalsign.BpSystolic) + ", "
                      + "BpDiastolic       =  " + SOut.Int(vitalsign.BpDiastolic) + ", "
                      + "DateTaken         =  " + SOut.Date(vitalsign.DateTaken) + ", "
                      + "HasFollowupPlan   =  " + SOut.Bool(vitalsign.HasFollowupPlan) + ", "
                      + "IsIneligible      =  " + SOut.Bool(vitalsign.IsIneligible) + ", "
                      + "Documentation     =  " + DbHelper.ParamChar + "paramDocumentation, "
                      + "ChildGotNutrition =  " + SOut.Bool(vitalsign.ChildGotNutrition) + ", "
                      + "ChildGotPhysCouns =  " + SOut.Bool(vitalsign.ChildGotPhysCouns) + ", "
                      + "WeightCode        = '" + SOut.String(vitalsign.WeightCode) + "', "
                      + "HeightExamCode    = '" + SOut.String(vitalsign.HeightExamCode) + "', "
                      + "WeightExamCode    = '" + SOut.String(vitalsign.WeightExamCode) + "', "
                      + "BMIExamCode       = '" + SOut.String(vitalsign.BMIExamCode) + "', "
                      + "EhrNotPerformedNum=  " + SOut.Long(vitalsign.EhrNotPerformedNum) + ", "
                      + "PregDiseaseNum    =  " + SOut.Long(vitalsign.PregDiseaseNum) + ", "
                      + "BMIPercentile     =  " + SOut.Int(vitalsign.BMIPercentile) + ", "
                      + "Pulse             =  " + SOut.Int(vitalsign.Pulse) + " "
                      + "WHERE VitalsignNum = " + SOut.Long(vitalsign.VitalsignNum);
        if (vitalsign.Documentation == null) vitalsign.Documentation = "";
        var paramDocumentation = new OdSqlParameter("paramDocumentation", OdDbType.Text, SOut.StringParam(vitalsign.Documentation));
        Db.NonQ(command, paramDocumentation);
    }

    public static bool Update(Vitalsign vitalsign, Vitalsign oldVitalsign)
    {
        var command = "";
        if (vitalsign.PatNum != oldVitalsign.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(vitalsign.PatNum) + "";
        }

        if (vitalsign.Height != oldVitalsign.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Float(vitalsign.Height) + "";
        }

        if (vitalsign.Weight != oldVitalsign.Weight)
        {
            if (command != "") command += ",";
            command += "Weight = " + SOut.Float(vitalsign.Weight) + "";
        }

        if (vitalsign.BpSystolic != oldVitalsign.BpSystolic)
        {
            if (command != "") command += ",";
            command += "BpSystolic = " + SOut.Int(vitalsign.BpSystolic) + "";
        }

        if (vitalsign.BpDiastolic != oldVitalsign.BpDiastolic)
        {
            if (command != "") command += ",";
            command += "BpDiastolic = " + SOut.Int(vitalsign.BpDiastolic) + "";
        }

        if (vitalsign.DateTaken.Date != oldVitalsign.DateTaken.Date)
        {
            if (command != "") command += ",";
            command += "DateTaken = " + SOut.Date(vitalsign.DateTaken) + "";
        }

        if (vitalsign.HasFollowupPlan != oldVitalsign.HasFollowupPlan)
        {
            if (command != "") command += ",";
            command += "HasFollowupPlan = " + SOut.Bool(vitalsign.HasFollowupPlan) + "";
        }

        if (vitalsign.IsIneligible != oldVitalsign.IsIneligible)
        {
            if (command != "") command += ",";
            command += "IsIneligible = " + SOut.Bool(vitalsign.IsIneligible) + "";
        }

        if (vitalsign.Documentation != oldVitalsign.Documentation)
        {
            if (command != "") command += ",";
            command += "Documentation = " + DbHelper.ParamChar + "paramDocumentation";
        }

        if (vitalsign.ChildGotNutrition != oldVitalsign.ChildGotNutrition)
        {
            if (command != "") command += ",";
            command += "ChildGotNutrition = " + SOut.Bool(vitalsign.ChildGotNutrition) + "";
        }

        if (vitalsign.ChildGotPhysCouns != oldVitalsign.ChildGotPhysCouns)
        {
            if (command != "") command += ",";
            command += "ChildGotPhysCouns = " + SOut.Bool(vitalsign.ChildGotPhysCouns) + "";
        }

        if (vitalsign.WeightCode != oldVitalsign.WeightCode)
        {
            if (command != "") command += ",";
            command += "WeightCode = '" + SOut.String(vitalsign.WeightCode) + "'";
        }

        if (vitalsign.HeightExamCode != oldVitalsign.HeightExamCode)
        {
            if (command != "") command += ",";
            command += "HeightExamCode = '" + SOut.String(vitalsign.HeightExamCode) + "'";
        }

        if (vitalsign.WeightExamCode != oldVitalsign.WeightExamCode)
        {
            if (command != "") command += ",";
            command += "WeightExamCode = '" + SOut.String(vitalsign.WeightExamCode) + "'";
        }

        if (vitalsign.BMIExamCode != oldVitalsign.BMIExamCode)
        {
            if (command != "") command += ",";
            command += "BMIExamCode = '" + SOut.String(vitalsign.BMIExamCode) + "'";
        }

        if (vitalsign.EhrNotPerformedNum != oldVitalsign.EhrNotPerformedNum)
        {
            if (command != "") command += ",";
            command += "EhrNotPerformedNum = " + SOut.Long(vitalsign.EhrNotPerformedNum) + "";
        }

        if (vitalsign.PregDiseaseNum != oldVitalsign.PregDiseaseNum)
        {
            if (command != "") command += ",";
            command += "PregDiseaseNum = " + SOut.Long(vitalsign.PregDiseaseNum) + "";
        }

        if (vitalsign.BMIPercentile != oldVitalsign.BMIPercentile)
        {
            if (command != "") command += ",";
            command += "BMIPercentile = " + SOut.Int(vitalsign.BMIPercentile) + "";
        }

        if (vitalsign.Pulse != oldVitalsign.Pulse)
        {
            if (command != "") command += ",";
            command += "Pulse = " + SOut.Int(vitalsign.Pulse) + "";
        }

        if (command == "") return false;
        if (vitalsign.Documentation == null) vitalsign.Documentation = "";
        var paramDocumentation = new OdSqlParameter("paramDocumentation", OdDbType.Text, SOut.StringParam(vitalsign.Documentation));
        command = "UPDATE vitalsign SET " + command
                                          + " WHERE VitalsignNum = " + SOut.Long(vitalsign.VitalsignNum);
        Db.NonQ(command, paramDocumentation);
        return true;
    }

    public static bool UpdateComparison(Vitalsign vitalsign, Vitalsign oldVitalsign)
    {
        if (vitalsign.PatNum != oldVitalsign.PatNum) return true;
        if (vitalsign.Height != oldVitalsign.Height) return true;
        if (vitalsign.Weight != oldVitalsign.Weight) return true;
        if (vitalsign.BpSystolic != oldVitalsign.BpSystolic) return true;
        if (vitalsign.BpDiastolic != oldVitalsign.BpDiastolic) return true;
        if (vitalsign.DateTaken.Date != oldVitalsign.DateTaken.Date) return true;
        if (vitalsign.HasFollowupPlan != oldVitalsign.HasFollowupPlan) return true;
        if (vitalsign.IsIneligible != oldVitalsign.IsIneligible) return true;
        if (vitalsign.Documentation != oldVitalsign.Documentation) return true;
        if (vitalsign.ChildGotNutrition != oldVitalsign.ChildGotNutrition) return true;
        if (vitalsign.ChildGotPhysCouns != oldVitalsign.ChildGotPhysCouns) return true;
        if (vitalsign.WeightCode != oldVitalsign.WeightCode) return true;
        if (vitalsign.HeightExamCode != oldVitalsign.HeightExamCode) return true;
        if (vitalsign.WeightExamCode != oldVitalsign.WeightExamCode) return true;
        if (vitalsign.BMIExamCode != oldVitalsign.BMIExamCode) return true;
        if (vitalsign.EhrNotPerformedNum != oldVitalsign.EhrNotPerformedNum) return true;
        if (vitalsign.PregDiseaseNum != oldVitalsign.PregDiseaseNum) return true;
        if (vitalsign.BMIPercentile != oldVitalsign.BMIPercentile) return true;
        if (vitalsign.Pulse != oldVitalsign.Pulse) return true;
        return false;
    }

    public static void Delete(long vitalsignNum)
    {
        var command = "DELETE FROM vitalsign "
                      + "WHERE VitalsignNum = " + SOut.Long(vitalsignNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listVitalsignNums)
    {
        if (listVitalsignNums == null || listVitalsignNums.Count == 0) return;
        var command = "DELETE FROM vitalsign "
                      + "WHERE VitalsignNum IN(" + string.Join(",", listVitalsignNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}