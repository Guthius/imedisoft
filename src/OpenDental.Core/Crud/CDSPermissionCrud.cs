using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CDSPermissionCrud
{
    public static CDSPermission SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<CDSPermission> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CDSPermission> TableToList(DataTable table)
    {
        var retVal = new List<CDSPermission>();
        CDSPermission cDSPermission;
        foreach (DataRow row in table.Rows)
        {
            cDSPermission = new CDSPermission();
            cDSPermission.CDSPermissionNum = SIn.Long(row["CDSPermissionNum"].ToString());
            cDSPermission.UserNum = SIn.Long(row["UserNum"].ToString());
            cDSPermission.SetupCDS = SIn.Bool(row["SetupCDS"].ToString());
            cDSPermission.ShowCDS = SIn.Bool(row["ShowCDS"].ToString());
            cDSPermission.ShowInfobutton = SIn.Bool(row["ShowInfobutton"].ToString());
            cDSPermission.EditBibliography = SIn.Bool(row["EditBibliography"].ToString());
            cDSPermission.ProblemCDS = SIn.Bool(row["ProblemCDS"].ToString());
            cDSPermission.MedicationCDS = SIn.Bool(row["MedicationCDS"].ToString());
            cDSPermission.AllergyCDS = SIn.Bool(row["AllergyCDS"].ToString());
            cDSPermission.DemographicCDS = SIn.Bool(row["DemographicCDS"].ToString());
            cDSPermission.LabTestCDS = SIn.Bool(row["LabTestCDS"].ToString());
            cDSPermission.VitalCDS = SIn.Bool(row["VitalCDS"].ToString());
            retVal.Add(cDSPermission);
        }

        return retVal;
    }

    public static long Insert(CDSPermission cDSPermission)
    {
        var command = "INSERT INTO cdspermission (";

        command += "UserNum,SetupCDS,ShowCDS,ShowInfobutton,EditBibliography,ProblemCDS,MedicationCDS,AllergyCDS,DemographicCDS,LabTestCDS,VitalCDS) VALUES(";

        command +=
            SOut.Long(cDSPermission.UserNum) + ","
                                             + SOut.Bool(cDSPermission.SetupCDS) + ","
                                             + SOut.Bool(cDSPermission.ShowCDS) + ","
                                             + SOut.Bool(cDSPermission.ShowInfobutton) + ","
                                             + SOut.Bool(cDSPermission.EditBibliography) + ","
                                             + SOut.Bool(cDSPermission.ProblemCDS) + ","
                                             + SOut.Bool(cDSPermission.MedicationCDS) + ","
                                             + SOut.Bool(cDSPermission.AllergyCDS) + ","
                                             + SOut.Bool(cDSPermission.DemographicCDS) + ","
                                             + SOut.Bool(cDSPermission.LabTestCDS) + ","
                                             + SOut.Bool(cDSPermission.VitalCDS) + ")";
        {
            cDSPermission.CDSPermissionNum = Db.NonQ(command, true, "CDSPermissionNum", "cDSPermission");
        }
        return cDSPermission.CDSPermissionNum;
    }

    public static void Update(CDSPermission cDSPermission)
    {
        var command = "UPDATE cdspermission SET "
                      + "UserNum         =  " + SOut.Long(cDSPermission.UserNum) + ", "
                      + "SetupCDS        =  " + SOut.Bool(cDSPermission.SetupCDS) + ", "
                      + "ShowCDS         =  " + SOut.Bool(cDSPermission.ShowCDS) + ", "
                      + "ShowInfobutton  =  " + SOut.Bool(cDSPermission.ShowInfobutton) + ", "
                      + "EditBibliography=  " + SOut.Bool(cDSPermission.EditBibliography) + ", "
                      + "ProblemCDS      =  " + SOut.Bool(cDSPermission.ProblemCDS) + ", "
                      + "MedicationCDS   =  " + SOut.Bool(cDSPermission.MedicationCDS) + ", "
                      + "AllergyCDS      =  " + SOut.Bool(cDSPermission.AllergyCDS) + ", "
                      + "DemographicCDS  =  " + SOut.Bool(cDSPermission.DemographicCDS) + ", "
                      + "LabTestCDS      =  " + SOut.Bool(cDSPermission.LabTestCDS) + ", "
                      + "VitalCDS        =  " + SOut.Bool(cDSPermission.VitalCDS) + " "
                      + "WHERE CDSPermissionNum = " + SOut.Long(cDSPermission.CDSPermissionNum);
        Db.NonQ(command);
    }
}