#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedLabFacAttachCrud
{
    public static MedLabFacAttach SelectOne(long medLabFacAttachNum)
    {
        var command = "SELECT * FROM medlabfacattach "
                      + "WHERE MedLabFacAttachNum = " + SOut.Long(medLabFacAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedLabFacAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedLabFacAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedLabFacAttach> TableToList(DataTable table)
    {
        var retVal = new List<MedLabFacAttach>();
        MedLabFacAttach medLabFacAttach;
        foreach (DataRow row in table.Rows)
        {
            medLabFacAttach = new MedLabFacAttach();
            medLabFacAttach.MedLabFacAttachNum = SIn.Long(row["MedLabFacAttachNum"].ToString());
            medLabFacAttach.MedLabNum = SIn.Long(row["MedLabNum"].ToString());
            medLabFacAttach.MedLabResultNum = SIn.Long(row["MedLabResultNum"].ToString());
            medLabFacAttach.MedLabFacilityNum = SIn.Long(row["MedLabFacilityNum"].ToString());
            retVal.Add(medLabFacAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MedLabFacAttach> listMedLabFacAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MedLabFacAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("MedLabFacAttachNum");
        table.Columns.Add("MedLabNum");
        table.Columns.Add("MedLabResultNum");
        table.Columns.Add("MedLabFacilityNum");
        foreach (var medLabFacAttach in listMedLabFacAttachs)
            table.Rows.Add(SOut.Long(medLabFacAttach.MedLabFacAttachNum), SOut.Long(medLabFacAttach.MedLabNum), SOut.Long(medLabFacAttach.MedLabResultNum), SOut.Long(medLabFacAttach.MedLabFacilityNum));
        return table;
    }

    public static long Insert(MedLabFacAttach medLabFacAttach)
    {
        return Insert(medLabFacAttach, false);
    }

    public static long Insert(MedLabFacAttach medLabFacAttach, bool useExistingPK)
    {
        var command = "INSERT INTO medlabfacattach (";

        command += "MedLabNum,MedLabResultNum,MedLabFacilityNum) VALUES(";

        command +=
            SOut.Long(medLabFacAttach.MedLabNum) + ","
                                                 + SOut.Long(medLabFacAttach.MedLabResultNum) + ","
                                                 + SOut.Long(medLabFacAttach.MedLabFacilityNum) + ")";
        {
            medLabFacAttach.MedLabFacAttachNum = Db.NonQ(command, true, "MedLabFacAttachNum", "medLabFacAttach");
        }
        return medLabFacAttach.MedLabFacAttachNum;
    }

    public static long InsertNoCache(MedLabFacAttach medLabFacAttach)
    {
        return InsertNoCache(medLabFacAttach, false);
    }

    public static long InsertNoCache(MedLabFacAttach medLabFacAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medlabfacattach (";
        if (isRandomKeys || useExistingPK) command += "MedLabFacAttachNum,";
        command += "MedLabNum,MedLabResultNum,MedLabFacilityNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medLabFacAttach.MedLabFacAttachNum) + ",";
        command +=
            SOut.Long(medLabFacAttach.MedLabNum) + ","
                                                 + SOut.Long(medLabFacAttach.MedLabResultNum) + ","
                                                 + SOut.Long(medLabFacAttach.MedLabFacilityNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            medLabFacAttach.MedLabFacAttachNum = Db.NonQ(command, true, "MedLabFacAttachNum", "medLabFacAttach");
        return medLabFacAttach.MedLabFacAttachNum;
    }

    public static void Update(MedLabFacAttach medLabFacAttach)
    {
        var command = "UPDATE medlabfacattach SET "
                      + "MedLabNum         =  " + SOut.Long(medLabFacAttach.MedLabNum) + ", "
                      + "MedLabResultNum   =  " + SOut.Long(medLabFacAttach.MedLabResultNum) + ", "
                      + "MedLabFacilityNum =  " + SOut.Long(medLabFacAttach.MedLabFacilityNum) + " "
                      + "WHERE MedLabFacAttachNum = " + SOut.Long(medLabFacAttach.MedLabFacAttachNum);
        Db.NonQ(command);
    }

    public static bool Update(MedLabFacAttach medLabFacAttach, MedLabFacAttach oldMedLabFacAttach)
    {
        var command = "";
        if (medLabFacAttach.MedLabNum != oldMedLabFacAttach.MedLabNum)
        {
            if (command != "") command += ",";
            command += "MedLabNum = " + SOut.Long(medLabFacAttach.MedLabNum) + "";
        }

        if (medLabFacAttach.MedLabResultNum != oldMedLabFacAttach.MedLabResultNum)
        {
            if (command != "") command += ",";
            command += "MedLabResultNum = " + SOut.Long(medLabFacAttach.MedLabResultNum) + "";
        }

        if (medLabFacAttach.MedLabFacilityNum != oldMedLabFacAttach.MedLabFacilityNum)
        {
            if (command != "") command += ",";
            command += "MedLabFacilityNum = " + SOut.Long(medLabFacAttach.MedLabFacilityNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE medlabfacattach SET " + command
                                                + " WHERE MedLabFacAttachNum = " + SOut.Long(medLabFacAttach.MedLabFacAttachNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(MedLabFacAttach medLabFacAttach, MedLabFacAttach oldMedLabFacAttach)
    {
        if (medLabFacAttach.MedLabNum != oldMedLabFacAttach.MedLabNum) return true;
        if (medLabFacAttach.MedLabResultNum != oldMedLabFacAttach.MedLabResultNum) return true;
        if (medLabFacAttach.MedLabFacilityNum != oldMedLabFacAttach.MedLabFacilityNum) return true;
        return false;
    }

    public static void Delete(long medLabFacAttachNum)
    {
        var command = "DELETE FROM medlabfacattach "
                      + "WHERE MedLabFacAttachNum = " + SOut.Long(medLabFacAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedLabFacAttachNums)
    {
        if (listMedLabFacAttachNums == null || listMedLabFacAttachNums.Count == 0) return;
        var command = "DELETE FROM medlabfacattach "
                      + "WHERE MedLabFacAttachNum IN(" + string.Join(",", listMedLabFacAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}