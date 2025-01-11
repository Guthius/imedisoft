#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrTriggerCrud
{
    public static EhrTrigger SelectOne(long ehrTriggerNum)
    {
        var command = "SELECT * FROM ehrtrigger "
                      + "WHERE EhrTriggerNum = " + SOut.Long(ehrTriggerNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrTrigger SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrTrigger> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrTrigger> TableToList(DataTable table)
    {
        var retVal = new List<EhrTrigger>();
        EhrTrigger ehrTrigger;
        foreach (DataRow row in table.Rows)
        {
            ehrTrigger = new EhrTrigger();
            ehrTrigger.EhrTriggerNum = SIn.Long(row["EhrTriggerNum"].ToString());
            ehrTrigger.Description = SIn.String(row["Description"].ToString());
            ehrTrigger.ProblemSnomedList = SIn.String(row["ProblemSnomedList"].ToString());
            ehrTrigger.ProblemIcd9List = SIn.String(row["ProblemIcd9List"].ToString());
            ehrTrigger.ProblemIcd10List = SIn.String(row["ProblemIcd10List"].ToString());
            ehrTrigger.ProblemDefNumList = SIn.String(row["ProblemDefNumList"].ToString());
            ehrTrigger.MedicationNumList = SIn.String(row["MedicationNumList"].ToString());
            ehrTrigger.RxCuiList = SIn.String(row["RxCuiList"].ToString());
            ehrTrigger.CvxList = SIn.String(row["CvxList"].ToString());
            ehrTrigger.AllergyDefNumList = SIn.String(row["AllergyDefNumList"].ToString());
            ehrTrigger.DemographicsList = SIn.String(row["DemographicsList"].ToString());
            ehrTrigger.LabLoincList = SIn.String(row["LabLoincList"].ToString());
            ehrTrigger.VitalLoincList = SIn.String(row["VitalLoincList"].ToString());
            ehrTrigger.Instructions = SIn.String(row["Instructions"].ToString());
            ehrTrigger.Bibliography = SIn.String(row["Bibliography"].ToString());
            ehrTrigger.Cardinality = (MatchCardinality) SIn.Int(row["Cardinality"].ToString());
            retVal.Add(ehrTrigger);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrTrigger> listEhrTriggers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrTrigger";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrTriggerNum");
        table.Columns.Add("Description");
        table.Columns.Add("ProblemSnomedList");
        table.Columns.Add("ProblemIcd9List");
        table.Columns.Add("ProblemIcd10List");
        table.Columns.Add("ProblemDefNumList");
        table.Columns.Add("MedicationNumList");
        table.Columns.Add("RxCuiList");
        table.Columns.Add("CvxList");
        table.Columns.Add("AllergyDefNumList");
        table.Columns.Add("DemographicsList");
        table.Columns.Add("LabLoincList");
        table.Columns.Add("VitalLoincList");
        table.Columns.Add("Instructions");
        table.Columns.Add("Bibliography");
        table.Columns.Add("Cardinality");
        foreach (var ehrTrigger in listEhrTriggers)
            table.Rows.Add(SOut.Long(ehrTrigger.EhrTriggerNum), ehrTrigger.Description, ehrTrigger.ProblemSnomedList, ehrTrigger.ProblemIcd9List, ehrTrigger.ProblemIcd10List, ehrTrigger.ProblemDefNumList, ehrTrigger.MedicationNumList, ehrTrigger.RxCuiList, ehrTrigger.CvxList, ehrTrigger.AllergyDefNumList, ehrTrigger.DemographicsList, ehrTrigger.LabLoincList, ehrTrigger.VitalLoincList, ehrTrigger.Instructions, ehrTrigger.Bibliography, SOut.Int((int) ehrTrigger.Cardinality));
        return table;
    }

    public static long Insert(EhrTrigger ehrTrigger)
    {
        return Insert(ehrTrigger, false);
    }

    public static long Insert(EhrTrigger ehrTrigger, bool useExistingPK)
    {
        var command = "INSERT INTO ehrtrigger (";

        command += "Description,ProblemSnomedList,ProblemIcd9List,ProblemIcd10List,ProblemDefNumList,MedicationNumList,RxCuiList,CvxList,AllergyDefNumList,DemographicsList,LabLoincList,VitalLoincList,Instructions,Bibliography,Cardinality) VALUES(";

        command +=
            "'" + SOut.String(ehrTrigger.Description) + "',"
            + DbHelper.ParamChar + "paramProblemSnomedList,"
            + DbHelper.ParamChar + "paramProblemIcd9List,"
            + DbHelper.ParamChar + "paramProblemIcd10List,"
            + DbHelper.ParamChar + "paramProblemDefNumList,"
            + DbHelper.ParamChar + "paramMedicationNumList,"
            + DbHelper.ParamChar + "paramRxCuiList,"
            + DbHelper.ParamChar + "paramCvxList,"
            + DbHelper.ParamChar + "paramAllergyDefNumList,"
            + DbHelper.ParamChar + "paramDemographicsList,"
            + DbHelper.ParamChar + "paramLabLoincList,"
            + DbHelper.ParamChar + "paramVitalLoincList,"
            + DbHelper.ParamChar + "paramInstructions,"
            + DbHelper.ParamChar + "paramBibliography,"
            + SOut.Int((int) ehrTrigger.Cardinality) + ")";
        if (ehrTrigger.ProblemSnomedList == null) ehrTrigger.ProblemSnomedList = "";
        var paramProblemSnomedList = new OdSqlParameter("paramProblemSnomedList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemSnomedList));
        if (ehrTrigger.ProblemIcd9List == null) ehrTrigger.ProblemIcd9List = "";
        var paramProblemIcd9List = new OdSqlParameter("paramProblemIcd9List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd9List));
        if (ehrTrigger.ProblemIcd10List == null) ehrTrigger.ProblemIcd10List = "";
        var paramProblemIcd10List = new OdSqlParameter("paramProblemIcd10List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd10List));
        if (ehrTrigger.ProblemDefNumList == null) ehrTrigger.ProblemDefNumList = "";
        var paramProblemDefNumList = new OdSqlParameter("paramProblemDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemDefNumList));
        if (ehrTrigger.MedicationNumList == null) ehrTrigger.MedicationNumList = "";
        var paramMedicationNumList = new OdSqlParameter("paramMedicationNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.MedicationNumList));
        if (ehrTrigger.RxCuiList == null) ehrTrigger.RxCuiList = "";
        var paramRxCuiList = new OdSqlParameter("paramRxCuiList", OdDbType.Text, SOut.StringParam(ehrTrigger.RxCuiList));
        if (ehrTrigger.CvxList == null) ehrTrigger.CvxList = "";
        var paramCvxList = new OdSqlParameter("paramCvxList", OdDbType.Text, SOut.StringParam(ehrTrigger.CvxList));
        if (ehrTrigger.AllergyDefNumList == null) ehrTrigger.AllergyDefNumList = "";
        var paramAllergyDefNumList = new OdSqlParameter("paramAllergyDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.AllergyDefNumList));
        if (ehrTrigger.DemographicsList == null) ehrTrigger.DemographicsList = "";
        var paramDemographicsList = new OdSqlParameter("paramDemographicsList", OdDbType.Text, SOut.StringParam(ehrTrigger.DemographicsList));
        if (ehrTrigger.LabLoincList == null) ehrTrigger.LabLoincList = "";
        var paramLabLoincList = new OdSqlParameter("paramLabLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.LabLoincList));
        if (ehrTrigger.VitalLoincList == null) ehrTrigger.VitalLoincList = "";
        var paramVitalLoincList = new OdSqlParameter("paramVitalLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.VitalLoincList));
        if (ehrTrigger.Instructions == null) ehrTrigger.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(ehrTrigger.Instructions));
        if (ehrTrigger.Bibliography == null) ehrTrigger.Bibliography = "";
        var paramBibliography = new OdSqlParameter("paramBibliography", OdDbType.Text, SOut.StringParam(ehrTrigger.Bibliography));
        {
            ehrTrigger.EhrTriggerNum = Db.NonQ(command, true, "EhrTriggerNum", "ehrTrigger", paramProblemSnomedList, paramProblemIcd9List, paramProblemIcd10List, paramProblemDefNumList, paramMedicationNumList, paramRxCuiList, paramCvxList, paramAllergyDefNumList, paramDemographicsList, paramLabLoincList, paramVitalLoincList, paramInstructions, paramBibliography);
        }
        return ehrTrigger.EhrTriggerNum;
    }

    public static long InsertNoCache(EhrTrigger ehrTrigger)
    {
        return InsertNoCache(ehrTrigger, false);
    }

    public static long InsertNoCache(EhrTrigger ehrTrigger, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrtrigger (";
        if (isRandomKeys || useExistingPK) command += "EhrTriggerNum,";
        command += "Description,ProblemSnomedList,ProblemIcd9List,ProblemIcd10List,ProblemDefNumList,MedicationNumList,RxCuiList,CvxList,AllergyDefNumList,DemographicsList,LabLoincList,VitalLoincList,Instructions,Bibliography,Cardinality) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrTrigger.EhrTriggerNum) + ",";
        command +=
            "'" + SOut.String(ehrTrigger.Description) + "',"
            + DbHelper.ParamChar + "paramProblemSnomedList,"
            + DbHelper.ParamChar + "paramProblemIcd9List,"
            + DbHelper.ParamChar + "paramProblemIcd10List,"
            + DbHelper.ParamChar + "paramProblemDefNumList,"
            + DbHelper.ParamChar + "paramMedicationNumList,"
            + DbHelper.ParamChar + "paramRxCuiList,"
            + DbHelper.ParamChar + "paramCvxList,"
            + DbHelper.ParamChar + "paramAllergyDefNumList,"
            + DbHelper.ParamChar + "paramDemographicsList,"
            + DbHelper.ParamChar + "paramLabLoincList,"
            + DbHelper.ParamChar + "paramVitalLoincList,"
            + DbHelper.ParamChar + "paramInstructions,"
            + DbHelper.ParamChar + "paramBibliography,"
            + SOut.Int((int) ehrTrigger.Cardinality) + ")";
        if (ehrTrigger.ProblemSnomedList == null) ehrTrigger.ProblemSnomedList = "";
        var paramProblemSnomedList = new OdSqlParameter("paramProblemSnomedList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemSnomedList));
        if (ehrTrigger.ProblemIcd9List == null) ehrTrigger.ProblemIcd9List = "";
        var paramProblemIcd9List = new OdSqlParameter("paramProblemIcd9List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd9List));
        if (ehrTrigger.ProblemIcd10List == null) ehrTrigger.ProblemIcd10List = "";
        var paramProblemIcd10List = new OdSqlParameter("paramProblemIcd10List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd10List));
        if (ehrTrigger.ProblemDefNumList == null) ehrTrigger.ProblemDefNumList = "";
        var paramProblemDefNumList = new OdSqlParameter("paramProblemDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemDefNumList));
        if (ehrTrigger.MedicationNumList == null) ehrTrigger.MedicationNumList = "";
        var paramMedicationNumList = new OdSqlParameter("paramMedicationNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.MedicationNumList));
        if (ehrTrigger.RxCuiList == null) ehrTrigger.RxCuiList = "";
        var paramRxCuiList = new OdSqlParameter("paramRxCuiList", OdDbType.Text, SOut.StringParam(ehrTrigger.RxCuiList));
        if (ehrTrigger.CvxList == null) ehrTrigger.CvxList = "";
        var paramCvxList = new OdSqlParameter("paramCvxList", OdDbType.Text, SOut.StringParam(ehrTrigger.CvxList));
        if (ehrTrigger.AllergyDefNumList == null) ehrTrigger.AllergyDefNumList = "";
        var paramAllergyDefNumList = new OdSqlParameter("paramAllergyDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.AllergyDefNumList));
        if (ehrTrigger.DemographicsList == null) ehrTrigger.DemographicsList = "";
        var paramDemographicsList = new OdSqlParameter("paramDemographicsList", OdDbType.Text, SOut.StringParam(ehrTrigger.DemographicsList));
        if (ehrTrigger.LabLoincList == null) ehrTrigger.LabLoincList = "";
        var paramLabLoincList = new OdSqlParameter("paramLabLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.LabLoincList));
        if (ehrTrigger.VitalLoincList == null) ehrTrigger.VitalLoincList = "";
        var paramVitalLoincList = new OdSqlParameter("paramVitalLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.VitalLoincList));
        if (ehrTrigger.Instructions == null) ehrTrigger.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(ehrTrigger.Instructions));
        if (ehrTrigger.Bibliography == null) ehrTrigger.Bibliography = "";
        var paramBibliography = new OdSqlParameter("paramBibliography", OdDbType.Text, SOut.StringParam(ehrTrigger.Bibliography));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramProblemSnomedList, paramProblemIcd9List, paramProblemIcd10List, paramProblemDefNumList, paramMedicationNumList, paramRxCuiList, paramCvxList, paramAllergyDefNumList, paramDemographicsList, paramLabLoincList, paramVitalLoincList, paramInstructions, paramBibliography);
        else
            ehrTrigger.EhrTriggerNum = Db.NonQ(command, true, "EhrTriggerNum", "ehrTrigger", paramProblemSnomedList, paramProblemIcd9List, paramProblemIcd10List, paramProblemDefNumList, paramMedicationNumList, paramRxCuiList, paramCvxList, paramAllergyDefNumList, paramDemographicsList, paramLabLoincList, paramVitalLoincList, paramInstructions, paramBibliography);
        return ehrTrigger.EhrTriggerNum;
    }

    public static void Update(EhrTrigger ehrTrigger)
    {
        var command = "UPDATE ehrtrigger SET "
                      + "Description      = '" + SOut.String(ehrTrigger.Description) + "', "
                      + "ProblemSnomedList=  " + DbHelper.ParamChar + "paramProblemSnomedList, "
                      + "ProblemIcd9List  =  " + DbHelper.ParamChar + "paramProblemIcd9List, "
                      + "ProblemIcd10List =  " + DbHelper.ParamChar + "paramProblemIcd10List, "
                      + "ProblemDefNumList=  " + DbHelper.ParamChar + "paramProblemDefNumList, "
                      + "MedicationNumList=  " + DbHelper.ParamChar + "paramMedicationNumList, "
                      + "RxCuiList        =  " + DbHelper.ParamChar + "paramRxCuiList, "
                      + "CvxList          =  " + DbHelper.ParamChar + "paramCvxList, "
                      + "AllergyDefNumList=  " + DbHelper.ParamChar + "paramAllergyDefNumList, "
                      + "DemographicsList =  " + DbHelper.ParamChar + "paramDemographicsList, "
                      + "LabLoincList     =  " + DbHelper.ParamChar + "paramLabLoincList, "
                      + "VitalLoincList   =  " + DbHelper.ParamChar + "paramVitalLoincList, "
                      + "Instructions     =  " + DbHelper.ParamChar + "paramInstructions, "
                      + "Bibliography     =  " + DbHelper.ParamChar + "paramBibliography, "
                      + "Cardinality      =  " + SOut.Int((int) ehrTrigger.Cardinality) + " "
                      + "WHERE EhrTriggerNum = " + SOut.Long(ehrTrigger.EhrTriggerNum);
        if (ehrTrigger.ProblemSnomedList == null) ehrTrigger.ProblemSnomedList = "";
        var paramProblemSnomedList = new OdSqlParameter("paramProblemSnomedList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemSnomedList));
        if (ehrTrigger.ProblemIcd9List == null) ehrTrigger.ProblemIcd9List = "";
        var paramProblemIcd9List = new OdSqlParameter("paramProblemIcd9List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd9List));
        if (ehrTrigger.ProblemIcd10List == null) ehrTrigger.ProblemIcd10List = "";
        var paramProblemIcd10List = new OdSqlParameter("paramProblemIcd10List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd10List));
        if (ehrTrigger.ProblemDefNumList == null) ehrTrigger.ProblemDefNumList = "";
        var paramProblemDefNumList = new OdSqlParameter("paramProblemDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemDefNumList));
        if (ehrTrigger.MedicationNumList == null) ehrTrigger.MedicationNumList = "";
        var paramMedicationNumList = new OdSqlParameter("paramMedicationNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.MedicationNumList));
        if (ehrTrigger.RxCuiList == null) ehrTrigger.RxCuiList = "";
        var paramRxCuiList = new OdSqlParameter("paramRxCuiList", OdDbType.Text, SOut.StringParam(ehrTrigger.RxCuiList));
        if (ehrTrigger.CvxList == null) ehrTrigger.CvxList = "";
        var paramCvxList = new OdSqlParameter("paramCvxList", OdDbType.Text, SOut.StringParam(ehrTrigger.CvxList));
        if (ehrTrigger.AllergyDefNumList == null) ehrTrigger.AllergyDefNumList = "";
        var paramAllergyDefNumList = new OdSqlParameter("paramAllergyDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.AllergyDefNumList));
        if (ehrTrigger.DemographicsList == null) ehrTrigger.DemographicsList = "";
        var paramDemographicsList = new OdSqlParameter("paramDemographicsList", OdDbType.Text, SOut.StringParam(ehrTrigger.DemographicsList));
        if (ehrTrigger.LabLoincList == null) ehrTrigger.LabLoincList = "";
        var paramLabLoincList = new OdSqlParameter("paramLabLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.LabLoincList));
        if (ehrTrigger.VitalLoincList == null) ehrTrigger.VitalLoincList = "";
        var paramVitalLoincList = new OdSqlParameter("paramVitalLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.VitalLoincList));
        if (ehrTrigger.Instructions == null) ehrTrigger.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(ehrTrigger.Instructions));
        if (ehrTrigger.Bibliography == null) ehrTrigger.Bibliography = "";
        var paramBibliography = new OdSqlParameter("paramBibliography", OdDbType.Text, SOut.StringParam(ehrTrigger.Bibliography));
        Db.NonQ(command, paramProblemSnomedList, paramProblemIcd9List, paramProblemIcd10List, paramProblemDefNumList, paramMedicationNumList, paramRxCuiList, paramCvxList, paramAllergyDefNumList, paramDemographicsList, paramLabLoincList, paramVitalLoincList, paramInstructions, paramBibliography);
    }

    public static bool Update(EhrTrigger ehrTrigger, EhrTrigger oldEhrTrigger)
    {
        var command = "";
        if (ehrTrigger.Description != oldEhrTrigger.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(ehrTrigger.Description) + "'";
        }

        if (ehrTrigger.ProblemSnomedList != oldEhrTrigger.ProblemSnomedList)
        {
            if (command != "") command += ",";
            command += "ProblemSnomedList = " + DbHelper.ParamChar + "paramProblemSnomedList";
        }

        if (ehrTrigger.ProblemIcd9List != oldEhrTrigger.ProblemIcd9List)
        {
            if (command != "") command += ",";
            command += "ProblemIcd9List = " + DbHelper.ParamChar + "paramProblemIcd9List";
        }

        if (ehrTrigger.ProblemIcd10List != oldEhrTrigger.ProblemIcd10List)
        {
            if (command != "") command += ",";
            command += "ProblemIcd10List = " + DbHelper.ParamChar + "paramProblemIcd10List";
        }

        if (ehrTrigger.ProblemDefNumList != oldEhrTrigger.ProblemDefNumList)
        {
            if (command != "") command += ",";
            command += "ProblemDefNumList = " + DbHelper.ParamChar + "paramProblemDefNumList";
        }

        if (ehrTrigger.MedicationNumList != oldEhrTrigger.MedicationNumList)
        {
            if (command != "") command += ",";
            command += "MedicationNumList = " + DbHelper.ParamChar + "paramMedicationNumList";
        }

        if (ehrTrigger.RxCuiList != oldEhrTrigger.RxCuiList)
        {
            if (command != "") command += ",";
            command += "RxCuiList = " + DbHelper.ParamChar + "paramRxCuiList";
        }

        if (ehrTrigger.CvxList != oldEhrTrigger.CvxList)
        {
            if (command != "") command += ",";
            command += "CvxList = " + DbHelper.ParamChar + "paramCvxList";
        }

        if (ehrTrigger.AllergyDefNumList != oldEhrTrigger.AllergyDefNumList)
        {
            if (command != "") command += ",";
            command += "AllergyDefNumList = " + DbHelper.ParamChar + "paramAllergyDefNumList";
        }

        if (ehrTrigger.DemographicsList != oldEhrTrigger.DemographicsList)
        {
            if (command != "") command += ",";
            command += "DemographicsList = " + DbHelper.ParamChar + "paramDemographicsList";
        }

        if (ehrTrigger.LabLoincList != oldEhrTrigger.LabLoincList)
        {
            if (command != "") command += ",";
            command += "LabLoincList = " + DbHelper.ParamChar + "paramLabLoincList";
        }

        if (ehrTrigger.VitalLoincList != oldEhrTrigger.VitalLoincList)
        {
            if (command != "") command += ",";
            command += "VitalLoincList = " + DbHelper.ParamChar + "paramVitalLoincList";
        }

        if (ehrTrigger.Instructions != oldEhrTrigger.Instructions)
        {
            if (command != "") command += ",";
            command += "Instructions = " + DbHelper.ParamChar + "paramInstructions";
        }

        if (ehrTrigger.Bibliography != oldEhrTrigger.Bibliography)
        {
            if (command != "") command += ",";
            command += "Bibliography = " + DbHelper.ParamChar + "paramBibliography";
        }

        if (ehrTrigger.Cardinality != oldEhrTrigger.Cardinality)
        {
            if (command != "") command += ",";
            command += "Cardinality = " + SOut.Int((int) ehrTrigger.Cardinality) + "";
        }

        if (command == "") return false;
        if (ehrTrigger.ProblemSnomedList == null) ehrTrigger.ProblemSnomedList = "";
        var paramProblemSnomedList = new OdSqlParameter("paramProblemSnomedList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemSnomedList));
        if (ehrTrigger.ProblemIcd9List == null) ehrTrigger.ProblemIcd9List = "";
        var paramProblemIcd9List = new OdSqlParameter("paramProblemIcd9List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd9List));
        if (ehrTrigger.ProblemIcd10List == null) ehrTrigger.ProblemIcd10List = "";
        var paramProblemIcd10List = new OdSqlParameter("paramProblemIcd10List", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemIcd10List));
        if (ehrTrigger.ProblemDefNumList == null) ehrTrigger.ProblemDefNumList = "";
        var paramProblemDefNumList = new OdSqlParameter("paramProblemDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.ProblemDefNumList));
        if (ehrTrigger.MedicationNumList == null) ehrTrigger.MedicationNumList = "";
        var paramMedicationNumList = new OdSqlParameter("paramMedicationNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.MedicationNumList));
        if (ehrTrigger.RxCuiList == null) ehrTrigger.RxCuiList = "";
        var paramRxCuiList = new OdSqlParameter("paramRxCuiList", OdDbType.Text, SOut.StringParam(ehrTrigger.RxCuiList));
        if (ehrTrigger.CvxList == null) ehrTrigger.CvxList = "";
        var paramCvxList = new OdSqlParameter("paramCvxList", OdDbType.Text, SOut.StringParam(ehrTrigger.CvxList));
        if (ehrTrigger.AllergyDefNumList == null) ehrTrigger.AllergyDefNumList = "";
        var paramAllergyDefNumList = new OdSqlParameter("paramAllergyDefNumList", OdDbType.Text, SOut.StringParam(ehrTrigger.AllergyDefNumList));
        if (ehrTrigger.DemographicsList == null) ehrTrigger.DemographicsList = "";
        var paramDemographicsList = new OdSqlParameter("paramDemographicsList", OdDbType.Text, SOut.StringParam(ehrTrigger.DemographicsList));
        if (ehrTrigger.LabLoincList == null) ehrTrigger.LabLoincList = "";
        var paramLabLoincList = new OdSqlParameter("paramLabLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.LabLoincList));
        if (ehrTrigger.VitalLoincList == null) ehrTrigger.VitalLoincList = "";
        var paramVitalLoincList = new OdSqlParameter("paramVitalLoincList", OdDbType.Text, SOut.StringParam(ehrTrigger.VitalLoincList));
        if (ehrTrigger.Instructions == null) ehrTrigger.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(ehrTrigger.Instructions));
        if (ehrTrigger.Bibliography == null) ehrTrigger.Bibliography = "";
        var paramBibliography = new OdSqlParameter("paramBibliography", OdDbType.Text, SOut.StringParam(ehrTrigger.Bibliography));
        command = "UPDATE ehrtrigger SET " + command
                                           + " WHERE EhrTriggerNum = " + SOut.Long(ehrTrigger.EhrTriggerNum);
        Db.NonQ(command, paramProblemSnomedList, paramProblemIcd9List, paramProblemIcd10List, paramProblemDefNumList, paramMedicationNumList, paramRxCuiList, paramCvxList, paramAllergyDefNumList, paramDemographicsList, paramLabLoincList, paramVitalLoincList, paramInstructions, paramBibliography);
        return true;
    }

    public static bool UpdateComparison(EhrTrigger ehrTrigger, EhrTrigger oldEhrTrigger)
    {
        if (ehrTrigger.Description != oldEhrTrigger.Description) return true;
        if (ehrTrigger.ProblemSnomedList != oldEhrTrigger.ProblemSnomedList) return true;
        if (ehrTrigger.ProblemIcd9List != oldEhrTrigger.ProblemIcd9List) return true;
        if (ehrTrigger.ProblemIcd10List != oldEhrTrigger.ProblemIcd10List) return true;
        if (ehrTrigger.ProblemDefNumList != oldEhrTrigger.ProblemDefNumList) return true;
        if (ehrTrigger.MedicationNumList != oldEhrTrigger.MedicationNumList) return true;
        if (ehrTrigger.RxCuiList != oldEhrTrigger.RxCuiList) return true;
        if (ehrTrigger.CvxList != oldEhrTrigger.CvxList) return true;
        if (ehrTrigger.AllergyDefNumList != oldEhrTrigger.AllergyDefNumList) return true;
        if (ehrTrigger.DemographicsList != oldEhrTrigger.DemographicsList) return true;
        if (ehrTrigger.LabLoincList != oldEhrTrigger.LabLoincList) return true;
        if (ehrTrigger.VitalLoincList != oldEhrTrigger.VitalLoincList) return true;
        if (ehrTrigger.Instructions != oldEhrTrigger.Instructions) return true;
        if (ehrTrigger.Bibliography != oldEhrTrigger.Bibliography) return true;
        if (ehrTrigger.Cardinality != oldEhrTrigger.Cardinality) return true;
        return false;
    }

    public static void Delete(long ehrTriggerNum)
    {
        var command = "DELETE FROM ehrtrigger "
                      + "WHERE EhrTriggerNum = " + SOut.Long(ehrTriggerNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrTriggerNums)
    {
        if (listEhrTriggerNums == null || listEhrTriggerNums.Count == 0) return;
        var command = "DELETE FROM ehrtrigger "
                      + "WHERE EhrTriggerNum IN(" + string.Join(",", listEhrTriggerNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}