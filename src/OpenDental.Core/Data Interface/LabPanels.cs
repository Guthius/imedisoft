using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LabPanels
{
    
    public static List<LabPanel> Refresh(long patNum)
    {
        var command = "SELECT * FROM labpanel WHERE PatNum=" + SOut.Long(patNum);
        return LabPanelCrud.SelectMany(command);
    }

    
    public static List<LabPanel> GetPanelsForOrder(long medicalOrderNum)
    {
        if (medicalOrderNum == 0) return new List<LabPanel>();

        var command = "SELECT * FROM labpanel WHERE MedicalOrderNum=" + SOut.Long(medicalOrderNum);
        return LabPanelCrud.SelectMany(command);
    }

    
    public static void Delete(long labPanelNum)
    {
        var command = "DELETE FROM labpanel WHERE LabPanelNum = " + SOut.Long(labPanelNum);
        Db.NonQ(command);
    }

    public static List<long> GetChangedSinceLabPanelNums(DateTime dateChangedSince, List<long> listPatNumsEligibleForUpload)
    {
        var strPatNumsEligibleForUpload = "";
        DataTable table;
        if (listPatNumsEligibleForUpload.Count > 0)
        {
            for (var i = 0; i < listPatNumsEligibleForUpload.Count; i++)
            {
                if (i > 0) strPatNumsEligibleForUpload += "OR ";
                strPatNumsEligibleForUpload += "PatNum='" + listPatNumsEligibleForUpload[i] + "' ";
            }

            var command = "SELECT LabPanelNum FROM labpanel WHERE DateTStamp > " + SOut.DateT(dateChangedSince) + " AND (" + strPatNumsEligibleForUpload + ")";
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        var listLabPanelNums = new List<long>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++) listLabPanelNums.Add(SIn.Long(table.Rows[i]["LabPanelNum"].ToString()));
        return listLabPanelNums;
    }

    ///<summary>Used along with GetChangedSinceLabPanelNums</summary>
    public static List<LabPanel> GetMultLabPanels(List<long> listLabPanelNums)
    {
        var strLabPanelNums = "";
        DataTable table;
        if (listLabPanelNums.Count > 0)
        {
            for (var i = 0; i < listLabPanelNums.Count; i++)
            {
                if (i > 0) strLabPanelNums += "OR ";
                strLabPanelNums += "LabPanelNum='" + listLabPanelNums[i] + "' ";
            }

            var command = "SELECT * FROM labpanel WHERE " + strLabPanelNums;
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        return LabPanelCrud.TableToList(table);
    }

    
    public static long Insert(LabPanel labPanel)
    {
        return LabPanelCrud.Insert(labPanel);
    }

    
    public static void Update(LabPanel labPanel)
    {
        LabPanelCrud.Update(labPanel);
    }

    ///<summary>Changes the value of the DateTStamp column to the current time stamp for all labpanels of a patient</summary>
    public static void ResetTimeStamps(long patNum)
    {
        var command = "UPDATE labpanel SET DateTStamp = CURRENT_TIMESTAMP WHERE PatNum =" + SOut.Long(patNum);
        Db.NonQ(command);
    }

    ///<summary>Gets one LabPanel from the db.</summary>
    public static LabPanel GetOne(long labPanelNum)
    {
        return LabPanelCrud.SelectOne(labPanelNum);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.




            
    public static List<LabPanel> Refresh(long patNum){

        string command="SELECT * FROM labpanel WHERE PatNum = "+POut.Long(patNum);
        return Crud.LabPanelCrud.SelectMany(command);
    }



    
    public static void Delete(long labPanelNum) {

        string command= "DELETE FROM labpanel WHERE LabPanelNum = "+POut.Long(labPanelNum);
        Db.NonQ(command);
    }
    */
}