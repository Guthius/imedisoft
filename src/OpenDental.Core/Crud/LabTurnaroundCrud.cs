#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LabTurnaroundCrud
{
    public static LabTurnaround SelectOne(long labTurnaroundNum)
    {
        var command = "SELECT * FROM labturnaround "
                      + "WHERE LabTurnaroundNum = " + SOut.Long(labTurnaroundNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LabTurnaround SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LabTurnaround> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LabTurnaround> TableToList(DataTable table)
    {
        var retVal = new List<LabTurnaround>();
        LabTurnaround labTurnaround;
        foreach (DataRow row in table.Rows)
        {
            labTurnaround = new LabTurnaround();
            labTurnaround.LabTurnaroundNum = SIn.Long(row["LabTurnaroundNum"].ToString());
            labTurnaround.LaboratoryNum = SIn.Long(row["LaboratoryNum"].ToString());
            labTurnaround.Description = SIn.String(row["Description"].ToString());
            labTurnaround.DaysPublished = SIn.Int(row["DaysPublished"].ToString());
            labTurnaround.DaysActual = SIn.Int(row["DaysActual"].ToString());
            retVal.Add(labTurnaround);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LabTurnaround> listLabTurnarounds, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LabTurnaround";
        var table = new DataTable(tableName);
        table.Columns.Add("LabTurnaroundNum");
        table.Columns.Add("LaboratoryNum");
        table.Columns.Add("Description");
        table.Columns.Add("DaysPublished");
        table.Columns.Add("DaysActual");
        foreach (var labTurnaround in listLabTurnarounds)
            table.Rows.Add(SOut.Long(labTurnaround.LabTurnaroundNum), SOut.Long(labTurnaround.LaboratoryNum), labTurnaround.Description, SOut.Int(labTurnaround.DaysPublished), SOut.Int(labTurnaround.DaysActual));
        return table;
    }

    public static long Insert(LabTurnaround labTurnaround)
    {
        return Insert(labTurnaround, false);
    }

    public static long Insert(LabTurnaround labTurnaround, bool useExistingPK)
    {
        var command = "INSERT INTO labturnaround (";

        command += "LaboratoryNum,Description,DaysPublished,DaysActual) VALUES(";

        command +=
            SOut.Long(labTurnaround.LaboratoryNum) + ","
                                                   + "'" + SOut.String(labTurnaround.Description) + "',"
                                                   + SOut.Int(labTurnaround.DaysPublished) + ","
                                                   + SOut.Int(labTurnaround.DaysActual) + ")";
        {
            labTurnaround.LabTurnaroundNum = Db.NonQ(command, true, "LabTurnaroundNum", "labTurnaround");
        }
        return labTurnaround.LabTurnaroundNum;
    }

    public static long InsertNoCache(LabTurnaround labTurnaround)
    {
        return InsertNoCache(labTurnaround, false);
    }

    public static long InsertNoCache(LabTurnaround labTurnaround, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO labturnaround (";
        if (isRandomKeys || useExistingPK) command += "LabTurnaroundNum,";
        command += "LaboratoryNum,Description,DaysPublished,DaysActual) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(labTurnaround.LabTurnaroundNum) + ",";
        command +=
            SOut.Long(labTurnaround.LaboratoryNum) + ","
                                                   + "'" + SOut.String(labTurnaround.Description) + "',"
                                                   + SOut.Int(labTurnaround.DaysPublished) + ","
                                                   + SOut.Int(labTurnaround.DaysActual) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            labTurnaround.LabTurnaroundNum = Db.NonQ(command, true, "LabTurnaroundNum", "labTurnaround");
        return labTurnaround.LabTurnaroundNum;
    }

    public static void Update(LabTurnaround labTurnaround)
    {
        var command = "UPDATE labturnaround SET "
                      + "LaboratoryNum   =  " + SOut.Long(labTurnaround.LaboratoryNum) + ", "
                      + "Description     = '" + SOut.String(labTurnaround.Description) + "', "
                      + "DaysPublished   =  " + SOut.Int(labTurnaround.DaysPublished) + ", "
                      + "DaysActual      =  " + SOut.Int(labTurnaround.DaysActual) + " "
                      + "WHERE LabTurnaroundNum = " + SOut.Long(labTurnaround.LabTurnaroundNum);
        Db.NonQ(command);
    }

    public static bool Update(LabTurnaround labTurnaround, LabTurnaround oldLabTurnaround)
    {
        var command = "";
        if (labTurnaround.LaboratoryNum != oldLabTurnaround.LaboratoryNum)
        {
            if (command != "") command += ",";
            command += "LaboratoryNum = " + SOut.Long(labTurnaround.LaboratoryNum) + "";
        }

        if (labTurnaround.Description != oldLabTurnaround.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(labTurnaround.Description) + "'";
        }

        if (labTurnaround.DaysPublished != oldLabTurnaround.DaysPublished)
        {
            if (command != "") command += ",";
            command += "DaysPublished = " + SOut.Int(labTurnaround.DaysPublished) + "";
        }

        if (labTurnaround.DaysActual != oldLabTurnaround.DaysActual)
        {
            if (command != "") command += ",";
            command += "DaysActual = " + SOut.Int(labTurnaround.DaysActual) + "";
        }

        if (command == "") return false;
        command = "UPDATE labturnaround SET " + command
                                              + " WHERE LabTurnaroundNum = " + SOut.Long(labTurnaround.LabTurnaroundNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(LabTurnaround labTurnaround, LabTurnaround oldLabTurnaround)
    {
        if (labTurnaround.LaboratoryNum != oldLabTurnaround.LaboratoryNum) return true;
        if (labTurnaround.Description != oldLabTurnaround.Description) return true;
        if (labTurnaround.DaysPublished != oldLabTurnaround.DaysPublished) return true;
        if (labTurnaround.DaysActual != oldLabTurnaround.DaysActual) return true;
        return false;
    }

    public static void Delete(long labTurnaroundNum)
    {
        var command = "DELETE FROM labturnaround "
                      + "WHERE LabTurnaroundNum = " + SOut.Long(labTurnaroundNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLabTurnaroundNums)
    {
        if (listLabTurnaroundNums == null || listLabTurnaroundNums.Count == 0) return;
        var command = "DELETE FROM labturnaround "
                      + "WHERE LabTurnaroundNum IN(" + string.Join(",", listLabTurnaroundNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}