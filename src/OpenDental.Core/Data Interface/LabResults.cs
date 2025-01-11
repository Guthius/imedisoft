using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LabResults
{
    public static List<LabResult> GetForPanel(long labPanelNum)
    {
        var command = "SELECT * FROM labresult WHERE LabPanelNum = " + SOut.Long(labPanelNum);
        return LabResultCrud.SelectMany(command);
    }

    
    public static void Delete(long labResultNum)
    {
        var command = "DELETE FROM labresult WHERE LabResultNum = " + SOut.Long(labResultNum);
        Db.NonQ(command);
    }

    ///<summary>Deletes all Lab Results associated with Lab Panel.</summary>
    public static void DeleteForPanel(long labPanelNum)
    {
        var command = "DELETE FROM labresult WHERE LabPanelNum = " + SOut.Long(labPanelNum);
        Db.NonQ(command);
    }

    public static List<long> GetChangedSinceLabResultNums(DateTime dateChangedSince)
    {
        var command = "SELECT LabResultNum FROM labresult WHERE DateTStamp > " + SOut.DateT(dateChangedSince);
        var table = DataCore.GetTable(command);
        var listLabResultNums = new List<long>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++) listLabResultNums.Add(SIn.Long(table.Rows[i]["LabResultNum"].ToString()));
        return listLabResultNums;
    }

    ///<summary>Used along with GetChangedSinceLabResultNums</summary>
    public static List<LabResult> GetMultLabResults(List<long> listLabResultNums)
    {
        var strLabResultNums = "";
        DataTable table;
        if (listLabResultNums.Count > 0)
        {
            for (var i = 0; i < listLabResultNums.Count; i++)
            {
                if (i > 0) strLabResultNums += "OR ";
                strLabResultNums += "LabResultNum='" + listLabResultNums[i] + "' ";
            }

            var command = "SELECT * FROM labresult WHERE " + strLabResultNums;
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        return LabResultCrud.TableToList(table);
    }

    ///<summary>Get all lab results for one patient.</summary>
    public static List<LabResult> GetAllForPatient(long patNum)
    {
        var command = "SELECT * FROM labresult WHERE LabPanelNum IN (SELECT LabPanelNum FROM labpanel WHERE PatNum=" + SOut.Long(patNum) + ")";
        return LabResultCrud.SelectMany(command);
    }

    ///<summary>Insert new lab results.</summary>
    public static long Insert(LabResult labResult)
    {
        return LabResultCrud.Insert(labResult);
    }

    ///<summary>Update existing lab results.</summary>
    public static void Update(LabResult labResult)
    {
        LabResultCrud.Update(labResult);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<LabResult> Refresh(long patNum){

        string command="SELECT * FROM labresult WHERE PatNum = "+POut.Long(patNum);
        return Crud.LabResultCrud.SelectMany(command);
    }

    ///<summary>Gets one LabResult from the db.</summary>
    public static LabResult GetOne(long labResultNum){

        return Crud.LabResultCrud.SelectOne(labResultNum);
    }



    */

    /// <summary>Returns the text for a SnomedAllergy Enum as it should appear in human readable form for a CCD.</summary>
    public static string GetAbnormalFlagDesc(LabAbnormalFlag labAbnormalFlag)
    {
        string result;
        switch (labAbnormalFlag)
        {
            case LabAbnormalFlag.Above:
                result = "above high normal";
                break;
            case LabAbnormalFlag.Normal:
                result = "normal";
                break;
            case LabAbnormalFlag.Below:
                result = "below normal";
                break;
            case LabAbnormalFlag.None:
                result = "";
                break;
            default:
                result = "Error";
                break;
        }

        return result;
    }
}