#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedLabSpecimenCrud
{
    public static MedLabSpecimen SelectOne(long medLabSpecimenNum)
    {
        var command = "SELECT * FROM medlabspecimen "
                      + "WHERE MedLabSpecimenNum = " + SOut.Long(medLabSpecimenNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedLabSpecimen SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedLabSpecimen> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedLabSpecimen> TableToList(DataTable table)
    {
        var retVal = new List<MedLabSpecimen>();
        MedLabSpecimen medLabSpecimen;
        foreach (DataRow row in table.Rows)
        {
            medLabSpecimen = new MedLabSpecimen();
            medLabSpecimen.MedLabSpecimenNum = SIn.Long(row["MedLabSpecimenNum"].ToString());
            medLabSpecimen.MedLabNum = SIn.Long(row["MedLabNum"].ToString());
            medLabSpecimen.SpecimenID = SIn.String(row["SpecimenID"].ToString());
            medLabSpecimen.SpecimenDescript = SIn.String(row["SpecimenDescript"].ToString());
            medLabSpecimen.DateTimeCollected = SIn.DateTime(row["DateTimeCollected"].ToString());
            retVal.Add(medLabSpecimen);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MedLabSpecimen> listMedLabSpecimens, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MedLabSpecimen";
        var table = new DataTable(tableName);
        table.Columns.Add("MedLabSpecimenNum");
        table.Columns.Add("MedLabNum");
        table.Columns.Add("SpecimenID");
        table.Columns.Add("SpecimenDescript");
        table.Columns.Add("DateTimeCollected");
        foreach (var medLabSpecimen in listMedLabSpecimens)
            table.Rows.Add(SOut.Long(medLabSpecimen.MedLabSpecimenNum), SOut.Long(medLabSpecimen.MedLabNum), medLabSpecimen.SpecimenID, medLabSpecimen.SpecimenDescript, SOut.DateT(medLabSpecimen.DateTimeCollected, false));
        return table;
    }

    public static long Insert(MedLabSpecimen medLabSpecimen)
    {
        return Insert(medLabSpecimen, false);
    }

    public static long Insert(MedLabSpecimen medLabSpecimen, bool useExistingPK)
    {
        var command = "INSERT INTO medlabspecimen (";

        command += "MedLabNum,SpecimenID,SpecimenDescript,DateTimeCollected) VALUES(";

        command +=
            SOut.Long(medLabSpecimen.MedLabNum) + ","
                                                + "'" + SOut.String(medLabSpecimen.SpecimenID) + "',"
                                                + "'" + SOut.String(medLabSpecimen.SpecimenDescript) + "',"
                                                + SOut.DateT(medLabSpecimen.DateTimeCollected) + ")";
        {
            medLabSpecimen.MedLabSpecimenNum = Db.NonQ(command, true, "MedLabSpecimenNum", "medLabSpecimen");
        }
        return medLabSpecimen.MedLabSpecimenNum;
    }

    public static long InsertNoCache(MedLabSpecimen medLabSpecimen)
    {
        return InsertNoCache(medLabSpecimen, false);
    }

    public static long InsertNoCache(MedLabSpecimen medLabSpecimen, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medlabspecimen (";
        if (isRandomKeys || useExistingPK) command += "MedLabSpecimenNum,";
        command += "MedLabNum,SpecimenID,SpecimenDescript,DateTimeCollected) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medLabSpecimen.MedLabSpecimenNum) + ",";
        command +=
            SOut.Long(medLabSpecimen.MedLabNum) + ","
                                                + "'" + SOut.String(medLabSpecimen.SpecimenID) + "',"
                                                + "'" + SOut.String(medLabSpecimen.SpecimenDescript) + "',"
                                                + SOut.DateT(medLabSpecimen.DateTimeCollected) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            medLabSpecimen.MedLabSpecimenNum = Db.NonQ(command, true, "MedLabSpecimenNum", "medLabSpecimen");
        return medLabSpecimen.MedLabSpecimenNum;
    }

    public static void Update(MedLabSpecimen medLabSpecimen)
    {
        var command = "UPDATE medlabspecimen SET "
                      + "MedLabNum        =  " + SOut.Long(medLabSpecimen.MedLabNum) + ", "
                      + "SpecimenID       = '" + SOut.String(medLabSpecimen.SpecimenID) + "', "
                      + "SpecimenDescript = '" + SOut.String(medLabSpecimen.SpecimenDescript) + "', "
                      + "DateTimeCollected=  " + SOut.DateT(medLabSpecimen.DateTimeCollected) + " "
                      + "WHERE MedLabSpecimenNum = " + SOut.Long(medLabSpecimen.MedLabSpecimenNum);
        Db.NonQ(command);
    }

    public static bool Update(MedLabSpecimen medLabSpecimen, MedLabSpecimen oldMedLabSpecimen)
    {
        var command = "";
        if (medLabSpecimen.MedLabNum != oldMedLabSpecimen.MedLabNum)
        {
            if (command != "") command += ",";
            command += "MedLabNum = " + SOut.Long(medLabSpecimen.MedLabNum) + "";
        }

        if (medLabSpecimen.SpecimenID != oldMedLabSpecimen.SpecimenID)
        {
            if (command != "") command += ",";
            command += "SpecimenID = '" + SOut.String(medLabSpecimen.SpecimenID) + "'";
        }

        if (medLabSpecimen.SpecimenDescript != oldMedLabSpecimen.SpecimenDescript)
        {
            if (command != "") command += ",";
            command += "SpecimenDescript = '" + SOut.String(medLabSpecimen.SpecimenDescript) + "'";
        }

        if (medLabSpecimen.DateTimeCollected != oldMedLabSpecimen.DateTimeCollected)
        {
            if (command != "") command += ",";
            command += "DateTimeCollected = " + SOut.DateT(medLabSpecimen.DateTimeCollected) + "";
        }

        if (command == "") return false;
        command = "UPDATE medlabspecimen SET " + command
                                               + " WHERE MedLabSpecimenNum = " + SOut.Long(medLabSpecimen.MedLabSpecimenNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(MedLabSpecimen medLabSpecimen, MedLabSpecimen oldMedLabSpecimen)
    {
        if (medLabSpecimen.MedLabNum != oldMedLabSpecimen.MedLabNum) return true;
        if (medLabSpecimen.SpecimenID != oldMedLabSpecimen.SpecimenID) return true;
        if (medLabSpecimen.SpecimenDescript != oldMedLabSpecimen.SpecimenDescript) return true;
        if (medLabSpecimen.DateTimeCollected != oldMedLabSpecimen.DateTimeCollected) return true;
        return false;
    }

    public static void Delete(long medLabSpecimenNum)
    {
        var command = "DELETE FROM medlabspecimen "
                      + "WHERE MedLabSpecimenNum = " + SOut.Long(medLabSpecimenNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedLabSpecimenNums)
    {
        if (listMedLabSpecimenNums == null || listMedLabSpecimenNums.Count == 0) return;
        var command = "DELETE FROM medlabspecimen "
                      + "WHERE MedLabSpecimenNum IN(" + string.Join(",", listMedLabSpecimenNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}