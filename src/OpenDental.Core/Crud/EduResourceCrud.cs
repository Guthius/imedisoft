#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EduResourceCrud
{
    public static EduResource SelectOne(long eduResourceNum)
    {
        var command = "SELECT * FROM eduresource "
                      + "WHERE EduResourceNum = " + SOut.Long(eduResourceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EduResource SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EduResource> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EduResource> TableToList(DataTable table)
    {
        var retVal = new List<EduResource>();
        EduResource eduResource;
        foreach (DataRow row in table.Rows)
        {
            eduResource = new EduResource();
            eduResource.EduResourceNum = SIn.Long(row["EduResourceNum"].ToString());
            eduResource.DiseaseDefNum = SIn.Long(row["DiseaseDefNum"].ToString());
            eduResource.MedicationNum = SIn.Long(row["MedicationNum"].ToString());
            eduResource.LabResultID = SIn.String(row["LabResultID"].ToString());
            eduResource.LabResultName = SIn.String(row["LabResultName"].ToString());
            eduResource.LabResultCompare = SIn.String(row["LabResultCompare"].ToString());
            eduResource.ResourceUrl = SIn.String(row["ResourceUrl"].ToString());
            eduResource.SmokingSnoMed = SIn.String(row["SmokingSnoMed"].ToString());
            retVal.Add(eduResource);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EduResource> listEduResources, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EduResource";
        var table = new DataTable(tableName);
        table.Columns.Add("EduResourceNum");
        table.Columns.Add("DiseaseDefNum");
        table.Columns.Add("MedicationNum");
        table.Columns.Add("LabResultID");
        table.Columns.Add("LabResultName");
        table.Columns.Add("LabResultCompare");
        table.Columns.Add("ResourceUrl");
        table.Columns.Add("SmokingSnoMed");
        foreach (var eduResource in listEduResources)
            table.Rows.Add(SOut.Long(eduResource.EduResourceNum), SOut.Long(eduResource.DiseaseDefNum), SOut.Long(eduResource.MedicationNum), eduResource.LabResultID, eduResource.LabResultName, eduResource.LabResultCompare, eduResource.ResourceUrl, eduResource.SmokingSnoMed);
        return table;
    }

    public static long Insert(EduResource eduResource)
    {
        return Insert(eduResource, false);
    }

    public static long Insert(EduResource eduResource, bool useExistingPK)
    {
        var command = "INSERT INTO eduresource (";

        command += "DiseaseDefNum,MedicationNum,LabResultID,LabResultName,LabResultCompare,ResourceUrl,SmokingSnoMed) VALUES(";

        command +=
            SOut.Long(eduResource.DiseaseDefNum) + ","
                                                 + SOut.Long(eduResource.MedicationNum) + ","
                                                 + "'" + SOut.String(eduResource.LabResultID) + "',"
                                                 + "'" + SOut.String(eduResource.LabResultName) + "',"
                                                 + "'" + SOut.String(eduResource.LabResultCompare) + "',"
                                                 + "'" + SOut.String(eduResource.ResourceUrl) + "',"
                                                 + "'" + SOut.String(eduResource.SmokingSnoMed) + "')";
        {
            eduResource.EduResourceNum = Db.NonQ(command, true, "EduResourceNum", "eduResource");
        }
        return eduResource.EduResourceNum;
    }

    public static long InsertNoCache(EduResource eduResource)
    {
        return InsertNoCache(eduResource, false);
    }

    public static long InsertNoCache(EduResource eduResource, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eduresource (";
        if (isRandomKeys || useExistingPK) command += "EduResourceNum,";
        command += "DiseaseDefNum,MedicationNum,LabResultID,LabResultName,LabResultCompare,ResourceUrl,SmokingSnoMed) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eduResource.EduResourceNum) + ",";
        command +=
            SOut.Long(eduResource.DiseaseDefNum) + ","
                                                 + SOut.Long(eduResource.MedicationNum) + ","
                                                 + "'" + SOut.String(eduResource.LabResultID) + "',"
                                                 + "'" + SOut.String(eduResource.LabResultName) + "',"
                                                 + "'" + SOut.String(eduResource.LabResultCompare) + "',"
                                                 + "'" + SOut.String(eduResource.ResourceUrl) + "',"
                                                 + "'" + SOut.String(eduResource.SmokingSnoMed) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eduResource.EduResourceNum = Db.NonQ(command, true, "EduResourceNum", "eduResource");
        return eduResource.EduResourceNum;
    }

    public static void Update(EduResource eduResource)
    {
        var command = "UPDATE eduresource SET "
                      + "DiseaseDefNum   =  " + SOut.Long(eduResource.DiseaseDefNum) + ", "
                      + "MedicationNum   =  " + SOut.Long(eduResource.MedicationNum) + ", "
                      + "LabResultID     = '" + SOut.String(eduResource.LabResultID) + "', "
                      + "LabResultName   = '" + SOut.String(eduResource.LabResultName) + "', "
                      + "LabResultCompare= '" + SOut.String(eduResource.LabResultCompare) + "', "
                      + "ResourceUrl     = '" + SOut.String(eduResource.ResourceUrl) + "', "
                      + "SmokingSnoMed   = '" + SOut.String(eduResource.SmokingSnoMed) + "' "
                      + "WHERE EduResourceNum = " + SOut.Long(eduResource.EduResourceNum);
        Db.NonQ(command);
    }

    public static bool Update(EduResource eduResource, EduResource oldEduResource)
    {
        var command = "";
        if (eduResource.DiseaseDefNum != oldEduResource.DiseaseDefNum)
        {
            if (command != "") command += ",";
            command += "DiseaseDefNum = " + SOut.Long(eduResource.DiseaseDefNum) + "";
        }

        if (eduResource.MedicationNum != oldEduResource.MedicationNum)
        {
            if (command != "") command += ",";
            command += "MedicationNum = " + SOut.Long(eduResource.MedicationNum) + "";
        }

        if (eduResource.LabResultID != oldEduResource.LabResultID)
        {
            if (command != "") command += ",";
            command += "LabResultID = '" + SOut.String(eduResource.LabResultID) + "'";
        }

        if (eduResource.LabResultName != oldEduResource.LabResultName)
        {
            if (command != "") command += ",";
            command += "LabResultName = '" + SOut.String(eduResource.LabResultName) + "'";
        }

        if (eduResource.LabResultCompare != oldEduResource.LabResultCompare)
        {
            if (command != "") command += ",";
            command += "LabResultCompare = '" + SOut.String(eduResource.LabResultCompare) + "'";
        }

        if (eduResource.ResourceUrl != oldEduResource.ResourceUrl)
        {
            if (command != "") command += ",";
            command += "ResourceUrl = '" + SOut.String(eduResource.ResourceUrl) + "'";
        }

        if (eduResource.SmokingSnoMed != oldEduResource.SmokingSnoMed)
        {
            if (command != "") command += ",";
            command += "SmokingSnoMed = '" + SOut.String(eduResource.SmokingSnoMed) + "'";
        }

        if (command == "") return false;
        command = "UPDATE eduresource SET " + command
                                            + " WHERE EduResourceNum = " + SOut.Long(eduResource.EduResourceNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EduResource eduResource, EduResource oldEduResource)
    {
        if (eduResource.DiseaseDefNum != oldEduResource.DiseaseDefNum) return true;
        if (eduResource.MedicationNum != oldEduResource.MedicationNum) return true;
        if (eduResource.LabResultID != oldEduResource.LabResultID) return true;
        if (eduResource.LabResultName != oldEduResource.LabResultName) return true;
        if (eduResource.LabResultCompare != oldEduResource.LabResultCompare) return true;
        if (eduResource.ResourceUrl != oldEduResource.ResourceUrl) return true;
        if (eduResource.SmokingSnoMed != oldEduResource.SmokingSnoMed) return true;
        return false;
    }

    public static void Delete(long eduResourceNum)
    {
        var command = "DELETE FROM eduresource "
                      + "WHERE EduResourceNum = " + SOut.Long(eduResourceNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEduResourceNums)
    {
        if (listEduResourceNums == null || listEduResourceNums.Count == 0) return;
        var command = "DELETE FROM eduresource "
                      + "WHERE EduResourceNum IN(" + string.Join(",", listEduResourceNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}