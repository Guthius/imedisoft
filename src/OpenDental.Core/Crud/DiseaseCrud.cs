#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DiseaseCrud
{
    public static Disease SelectOne(long diseaseNum)
    {
        var command = "SELECT * FROM disease "
                      + "WHERE DiseaseNum = " + SOut.Long(diseaseNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Disease SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Disease> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Disease> TableToList(DataTable table)
    {
        var retVal = new List<Disease>();
        Disease disease;
        foreach (DataRow row in table.Rows)
        {
            disease = new Disease();
            disease.DiseaseNum = SIn.Long(row["DiseaseNum"].ToString());
            disease.PatNum = SIn.Long(row["PatNum"].ToString());
            disease.DiseaseDefNum = SIn.Long(row["DiseaseDefNum"].ToString());
            disease.PatNote = SIn.String(row["PatNote"].ToString());
            disease.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            disease.ProbStatus = (ProblemStatus) SIn.Int(row["ProbStatus"].ToString());
            disease.DateStart = SIn.Date(row["DateStart"].ToString());
            disease.DateStop = SIn.Date(row["DateStop"].ToString());
            disease.SnomedProblemType = SIn.String(row["SnomedProblemType"].ToString());
            disease.FunctionStatus = (FunctionalStatus) SIn.Int(row["FunctionStatus"].ToString());
            retVal.Add(disease);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Disease> listDiseases, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Disease";
        var table = new DataTable(tableName);
        table.Columns.Add("DiseaseNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DiseaseDefNum");
        table.Columns.Add("PatNote");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("ProbStatus");
        table.Columns.Add("DateStart");
        table.Columns.Add("DateStop");
        table.Columns.Add("SnomedProblemType");
        table.Columns.Add("FunctionStatus");
        foreach (var disease in listDiseases)
            table.Rows.Add(SOut.Long(disease.DiseaseNum), SOut.Long(disease.PatNum), SOut.Long(disease.DiseaseDefNum), disease.PatNote, SOut.DateT(disease.DateTStamp, false), SOut.Int((int) disease.ProbStatus), SOut.DateT(disease.DateStart, false), SOut.DateT(disease.DateStop, false), disease.SnomedProblemType, SOut.Int((int) disease.FunctionStatus));
        return table;
    }

    public static long Insert(Disease disease)
    {
        return Insert(disease, false);
    }

    public static long Insert(Disease disease, bool useExistingPK)
    {
        var command = "INSERT INTO disease (";

        command += "PatNum,DiseaseDefNum,PatNote,ProbStatus,DateStart,DateStop,SnomedProblemType,FunctionStatus) VALUES(";

        command +=
            SOut.Long(disease.PatNum) + ","
                                      + SOut.Long(disease.DiseaseDefNum) + ","
                                      + DbHelper.ParamChar + "paramPatNote,"
                                      //DateTStamp can only be set by MySQL
                                      + SOut.Int((int) disease.ProbStatus) + ","
                                      + SOut.Date(disease.DateStart) + ","
                                      + SOut.Date(disease.DateStop) + ","
                                      + "'" + SOut.String(disease.SnomedProblemType) + "',"
                                      + SOut.Int((int) disease.FunctionStatus) + ")";
        if (disease.PatNote == null) disease.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(disease.PatNote));
        {
            disease.DiseaseNum = Db.NonQ(command, true, "DiseaseNum", "disease", paramPatNote);
        }
        return disease.DiseaseNum;
    }

    public static long InsertNoCache(Disease disease)
    {
        return InsertNoCache(disease, false);
    }

    public static long InsertNoCache(Disease disease, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO disease (";
        if (isRandomKeys || useExistingPK) command += "DiseaseNum,";
        command += "PatNum,DiseaseDefNum,PatNote,ProbStatus,DateStart,DateStop,SnomedProblemType,FunctionStatus) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(disease.DiseaseNum) + ",";
        command +=
            SOut.Long(disease.PatNum) + ","
                                      + SOut.Long(disease.DiseaseDefNum) + ","
                                      + DbHelper.ParamChar + "paramPatNote,"
                                      //DateTStamp can only be set by MySQL
                                      + SOut.Int((int) disease.ProbStatus) + ","
                                      + SOut.Date(disease.DateStart) + ","
                                      + SOut.Date(disease.DateStop) + ","
                                      + "'" + SOut.String(disease.SnomedProblemType) + "',"
                                      + SOut.Int((int) disease.FunctionStatus) + ")";
        if (disease.PatNote == null) disease.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(disease.PatNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPatNote);
        else
            disease.DiseaseNum = Db.NonQ(command, true, "DiseaseNum", "disease", paramPatNote);
        return disease.DiseaseNum;
    }

    public static void Update(Disease disease)
    {
        var command = "UPDATE disease SET "
                      + "PatNum           =  " + SOut.Long(disease.PatNum) + ", "
                      + "DiseaseDefNum    =  " + SOut.Long(disease.DiseaseDefNum) + ", "
                      + "PatNote          =  " + DbHelper.ParamChar + "paramPatNote, "
                      //DateTStamp can only be set by MySQL
                      + "ProbStatus       =  " + SOut.Int((int) disease.ProbStatus) + ", "
                      + "DateStart        =  " + SOut.Date(disease.DateStart) + ", "
                      + "DateStop         =  " + SOut.Date(disease.DateStop) + ", "
                      + "SnomedProblemType= '" + SOut.String(disease.SnomedProblemType) + "', "
                      + "FunctionStatus   =  " + SOut.Int((int) disease.FunctionStatus) + " "
                      + "WHERE DiseaseNum = " + SOut.Long(disease.DiseaseNum);
        if (disease.PatNote == null) disease.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(disease.PatNote));
        Db.NonQ(command, paramPatNote);
    }

    public static bool Update(Disease disease, Disease oldDisease)
    {
        var command = "";
        if (disease.PatNum != oldDisease.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(disease.PatNum) + "";
        }

        if (disease.DiseaseDefNum != oldDisease.DiseaseDefNum)
        {
            if (command != "") command += ",";
            command += "DiseaseDefNum = " + SOut.Long(disease.DiseaseDefNum) + "";
        }

        if (disease.PatNote != oldDisease.PatNote)
        {
            if (command != "") command += ",";
            command += "PatNote = " + DbHelper.ParamChar + "paramPatNote";
        }

        //DateTStamp can only be set by MySQL
        if (disease.ProbStatus != oldDisease.ProbStatus)
        {
            if (command != "") command += ",";
            command += "ProbStatus = " + SOut.Int((int) disease.ProbStatus) + "";
        }

        if (disease.DateStart.Date != oldDisease.DateStart.Date)
        {
            if (command != "") command += ",";
            command += "DateStart = " + SOut.Date(disease.DateStart) + "";
        }

        if (disease.DateStop.Date != oldDisease.DateStop.Date)
        {
            if (command != "") command += ",";
            command += "DateStop = " + SOut.Date(disease.DateStop) + "";
        }

        if (disease.SnomedProblemType != oldDisease.SnomedProblemType)
        {
            if (command != "") command += ",";
            command += "SnomedProblemType = '" + SOut.String(disease.SnomedProblemType) + "'";
        }

        if (disease.FunctionStatus != oldDisease.FunctionStatus)
        {
            if (command != "") command += ",";
            command += "FunctionStatus = " + SOut.Int((int) disease.FunctionStatus) + "";
        }

        if (command == "") return false;
        if (disease.PatNote == null) disease.PatNote = "";
        var paramPatNote = new OdSqlParameter("paramPatNote", OdDbType.Text, SOut.StringParam(disease.PatNote));
        command = "UPDATE disease SET " + command
                                        + " WHERE DiseaseNum = " + SOut.Long(disease.DiseaseNum);
        Db.NonQ(command, paramPatNote);
        return true;
    }

    public static bool UpdateComparison(Disease disease, Disease oldDisease)
    {
        if (disease.PatNum != oldDisease.PatNum) return true;
        if (disease.DiseaseDefNum != oldDisease.DiseaseDefNum) return true;
        if (disease.PatNote != oldDisease.PatNote) return true;
        //DateTStamp can only be set by MySQL
        if (disease.ProbStatus != oldDisease.ProbStatus) return true;
        if (disease.DateStart.Date != oldDisease.DateStart.Date) return true;
        if (disease.DateStop.Date != oldDisease.DateStop.Date) return true;
        if (disease.SnomedProblemType != oldDisease.SnomedProblemType) return true;
        if (disease.FunctionStatus != oldDisease.FunctionStatus) return true;
        return false;
    }

    public static void Delete(long diseaseNum)
    {
        var command = "DELETE FROM disease "
                      + "WHERE DiseaseNum = " + SOut.Long(diseaseNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDiseaseNums)
    {
        if (listDiseaseNums == null || listDiseaseNums.Count == 0) return;
        var command = "DELETE FROM disease "
                      + "WHERE DiseaseNum IN(" + string.Join(",", listDiseaseNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}