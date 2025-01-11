using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AllergyDefCrud
{
    public static AllergyDef SelectOne(long allergyDefNum)
    {
        var command = "SELECT * FROM allergydef "
                      + "WHERE AllergyDefNum = " + SOut.Long(allergyDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static AllergyDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<AllergyDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AllergyDef> TableToList(DataTable table)
    {
        var retVal = new List<AllergyDef>();
        AllergyDef allergyDef;
        foreach (DataRow row in table.Rows)
        {
            allergyDef = new AllergyDef();
            allergyDef.AllergyDefNum = SIn.Long(row["AllergyDefNum"].ToString());
            allergyDef.Description = SIn.String(row["Description"].ToString());
            allergyDef.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            allergyDef.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            allergyDef.SnomedType = (SnomedAllergy) SIn.Int(row["SnomedType"].ToString());
            allergyDef.MedicationNum = SIn.Long(row["MedicationNum"].ToString());
            allergyDef.UniiCode = SIn.String(row["UniiCode"].ToString());
            retVal.Add(allergyDef);
        }

        return retVal;
    }

    public static long Insert(AllergyDef allergyDef)
    {
        var command = "INSERT INTO allergydef (";

        command += "Description,IsHidden,SnomedType,MedicationNum,UniiCode) VALUES(";

        command +=
            "'" + SOut.String(allergyDef.Description) + "',"
            + SOut.Bool(allergyDef.IsHidden) + ","
            //DateTStamp can only be set by MySQL
            + SOut.Int((int) allergyDef.SnomedType) + ","
            + SOut.Long(allergyDef.MedicationNum) + ","
            + "'" + SOut.String(allergyDef.UniiCode) + "')";
        {
            allergyDef.AllergyDefNum = Db.NonQ(command, true, "AllergyDefNum", "allergyDef");
        }
        return allergyDef.AllergyDefNum;
    }

    public static void Update(AllergyDef allergyDef)
    {
        var command = "UPDATE allergydef SET "
                      + "Description  = '" + SOut.String(allergyDef.Description) + "', "
                      + "IsHidden     =  " + SOut.Bool(allergyDef.IsHidden) + ", "
                      //DateTStamp can only be set by MySQL
                      + "SnomedType   =  " + SOut.Int((int) allergyDef.SnomedType) + ", "
                      + "MedicationNum=  " + SOut.Long(allergyDef.MedicationNum) + ", "
                      + "UniiCode     = '" + SOut.String(allergyDef.UniiCode) + "' "
                      + "WHERE AllergyDefNum = " + SOut.Long(allergyDef.AllergyDefNum);
        Db.NonQ(command);
    }
}