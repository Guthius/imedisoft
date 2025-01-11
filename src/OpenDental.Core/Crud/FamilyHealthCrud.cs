#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FamilyHealthCrud
{
    public static FamilyHealth SelectOne(long familyHealthNum)
    {
        var command = "SELECT * FROM familyhealth "
                      + "WHERE FamilyHealthNum = " + SOut.Long(familyHealthNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static FamilyHealth SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FamilyHealth> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FamilyHealth> TableToList(DataTable table)
    {
        var retVal = new List<FamilyHealth>();
        FamilyHealth familyHealth;
        foreach (DataRow row in table.Rows)
        {
            familyHealth = new FamilyHealth();
            familyHealth.FamilyHealthNum = SIn.Long(row["FamilyHealthNum"].ToString());
            familyHealth.PatNum = SIn.Long(row["PatNum"].ToString());
            familyHealth.Relationship = (FamilyRelationship) SIn.Int(row["Relationship"].ToString());
            familyHealth.DiseaseDefNum = SIn.Long(row["DiseaseDefNum"].ToString());
            familyHealth.PersonName = SIn.String(row["PersonName"].ToString());
            retVal.Add(familyHealth);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FamilyHealth> listFamilyHealths, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FamilyHealth";
        var table = new DataTable(tableName);
        table.Columns.Add("FamilyHealthNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("Relationship");
        table.Columns.Add("DiseaseDefNum");
        table.Columns.Add("PersonName");
        foreach (var familyHealth in listFamilyHealths)
            table.Rows.Add(SOut.Long(familyHealth.FamilyHealthNum), SOut.Long(familyHealth.PatNum), SOut.Int((int) familyHealth.Relationship), SOut.Long(familyHealth.DiseaseDefNum), familyHealth.PersonName);
        return table;
    }

    public static long Insert(FamilyHealth familyHealth)
    {
        return Insert(familyHealth, false);
    }

    public static long Insert(FamilyHealth familyHealth, bool useExistingPK)
    {
        var command = "INSERT INTO familyhealth (";

        command += "PatNum,Relationship,DiseaseDefNum,PersonName) VALUES(";

        command +=
            SOut.Long(familyHealth.PatNum) + ","
                                           + SOut.Int((int) familyHealth.Relationship) + ","
                                           + SOut.Long(familyHealth.DiseaseDefNum) + ","
                                           + "'" + SOut.String(familyHealth.PersonName) + "')";
        {
            familyHealth.FamilyHealthNum = Db.NonQ(command, true, "FamilyHealthNum", "familyHealth");
        }
        return familyHealth.FamilyHealthNum;
    }

    public static long InsertNoCache(FamilyHealth familyHealth)
    {
        return InsertNoCache(familyHealth, false);
    }

    public static long InsertNoCache(FamilyHealth familyHealth, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO familyhealth (";
        if (isRandomKeys || useExistingPK) command += "FamilyHealthNum,";
        command += "PatNum,Relationship,DiseaseDefNum,PersonName) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(familyHealth.FamilyHealthNum) + ",";
        command +=
            SOut.Long(familyHealth.PatNum) + ","
                                           + SOut.Int((int) familyHealth.Relationship) + ","
                                           + SOut.Long(familyHealth.DiseaseDefNum) + ","
                                           + "'" + SOut.String(familyHealth.PersonName) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            familyHealth.FamilyHealthNum = Db.NonQ(command, true, "FamilyHealthNum", "familyHealth");
        return familyHealth.FamilyHealthNum;
    }

    public static void Update(FamilyHealth familyHealth)
    {
        var command = "UPDATE familyhealth SET "
                      + "PatNum         =  " + SOut.Long(familyHealth.PatNum) + ", "
                      + "Relationship   =  " + SOut.Int((int) familyHealth.Relationship) + ", "
                      + "DiseaseDefNum  =  " + SOut.Long(familyHealth.DiseaseDefNum) + ", "
                      + "PersonName     = '" + SOut.String(familyHealth.PersonName) + "' "
                      + "WHERE FamilyHealthNum = " + SOut.Long(familyHealth.FamilyHealthNum);
        Db.NonQ(command);
    }

    public static bool Update(FamilyHealth familyHealth, FamilyHealth oldFamilyHealth)
    {
        var command = "";
        if (familyHealth.PatNum != oldFamilyHealth.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(familyHealth.PatNum) + "";
        }

        if (familyHealth.Relationship != oldFamilyHealth.Relationship)
        {
            if (command != "") command += ",";
            command += "Relationship = " + SOut.Int((int) familyHealth.Relationship) + "";
        }

        if (familyHealth.DiseaseDefNum != oldFamilyHealth.DiseaseDefNum)
        {
            if (command != "") command += ",";
            command += "DiseaseDefNum = " + SOut.Long(familyHealth.DiseaseDefNum) + "";
        }

        if (familyHealth.PersonName != oldFamilyHealth.PersonName)
        {
            if (command != "") command += ",";
            command += "PersonName = '" + SOut.String(familyHealth.PersonName) + "'";
        }

        if (command == "") return false;
        command = "UPDATE familyhealth SET " + command
                                             + " WHERE FamilyHealthNum = " + SOut.Long(familyHealth.FamilyHealthNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(FamilyHealth familyHealth, FamilyHealth oldFamilyHealth)
    {
        if (familyHealth.PatNum != oldFamilyHealth.PatNum) return true;
        if (familyHealth.Relationship != oldFamilyHealth.Relationship) return true;
        if (familyHealth.DiseaseDefNum != oldFamilyHealth.DiseaseDefNum) return true;
        if (familyHealth.PersonName != oldFamilyHealth.PersonName) return true;
        return false;
    }

    public static void Delete(long familyHealthNum)
    {
        var command = "DELETE FROM familyhealth "
                      + "WHERE FamilyHealthNum = " + SOut.Long(familyHealthNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFamilyHealthNums)
    {
        if (listFamilyHealthNums == null || listFamilyHealthNums.Count == 0) return;
        var command = "DELETE FROM familyhealth "
                      + "WHERE FamilyHealthNum IN(" + string.Join(",", listFamilyHealthNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}