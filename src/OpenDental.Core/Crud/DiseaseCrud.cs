using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DiseaseCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var disease = new Disease
            {
                DiseaseNum = SIn.Long(row["DiseaseNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DiseaseDefNum = SIn.Long(row["DiseaseDefNum"].ToString()),
                PatNote = SIn.String(row["PatNote"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                ProbStatus = (ProblemStatus) SIn.Int(row["ProbStatus"].ToString()),
                DateStart = SIn.Date(row["DateStart"].ToString()),
                DateStop = SIn.Date(row["DateStop"].ToString()),
                SnomedProblemType = SIn.String(row["SnomedProblemType"].ToString()),
                FunctionStatus = (FunctionalStatus) SIn.Int(row["FunctionStatus"].ToString())
            };
            retVal.Add(disease);
        }

        return retVal;
    }

    public static void Insert(Disease disease)
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
        var paramPatNote = new OdSqlParameter("paramPatNote", SOut.StringParam(disease.PatNote));
        {
            disease.DiseaseNum = Db.NonQ(command, true, "DiseaseNum", "disease", paramPatNote);
        }
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
        var paramPatNote = new OdSqlParameter("paramPatNote", SOut.StringParam(disease.PatNote));
        Db.NonQ(command, paramPatNote);
    }
}