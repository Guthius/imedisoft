#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoHardwareCrud
{
    public static OrthoHardware SelectOne(long orthoHardwareNum)
    {
        var command = "SELECT * FROM orthohardware "
                      + "WHERE OrthoHardwareNum = " + SOut.Long(orthoHardwareNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoHardware SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoHardware> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoHardware> TableToList(DataTable table)
    {
        var retVal = new List<OrthoHardware>();
        OrthoHardware orthoHardware;
        foreach (DataRow row in table.Rows)
        {
            orthoHardware = new OrthoHardware();
            orthoHardware.OrthoHardwareNum = SIn.Long(row["OrthoHardwareNum"].ToString());
            orthoHardware.PatNum = SIn.Long(row["PatNum"].ToString());
            orthoHardware.DateExam = SIn.Date(row["DateExam"].ToString());
            orthoHardware.OrthoHardwareType = (EnumOrthoHardwareType) SIn.Int(row["OrthoHardwareType"].ToString());
            orthoHardware.OrthoHardwareSpecNum = SIn.Long(row["OrthoHardwareSpecNum"].ToString());
            orthoHardware.ToothRange = SIn.String(row["ToothRange"].ToString());
            orthoHardware.Note = SIn.String(row["Note"].ToString());
            orthoHardware.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            retVal.Add(orthoHardware);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoHardware> listOrthoHardwares, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoHardware";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoHardwareNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateExam");
        table.Columns.Add("OrthoHardwareType");
        table.Columns.Add("OrthoHardwareSpecNum");
        table.Columns.Add("ToothRange");
        table.Columns.Add("Note");
        table.Columns.Add("IsHidden");
        foreach (var orthoHardware in listOrthoHardwares)
            table.Rows.Add(SOut.Long(orthoHardware.OrthoHardwareNum), SOut.Long(orthoHardware.PatNum), SOut.DateT(orthoHardware.DateExam, false), SOut.Int((int) orthoHardware.OrthoHardwareType), SOut.Long(orthoHardware.OrthoHardwareSpecNum), orthoHardware.ToothRange, orthoHardware.Note, SOut.Bool(orthoHardware.IsHidden));
        return table;
    }

    public static long Insert(OrthoHardware orthoHardware)
    {
        return Insert(orthoHardware, false);
    }

    public static long Insert(OrthoHardware orthoHardware, bool useExistingPK)
    {
        var command = "INSERT INTO orthohardware (";

        command += "PatNum,DateExam,OrthoHardwareType,OrthoHardwareSpecNum,ToothRange,Note,IsHidden) VALUES(";

        command +=
            SOut.Long(orthoHardware.PatNum) + ","
                                            + SOut.Date(orthoHardware.DateExam) + ","
                                            + SOut.Int((int) orthoHardware.OrthoHardwareType) + ","
                                            + SOut.Long(orthoHardware.OrthoHardwareSpecNum) + ","
                                            + "'" + SOut.String(orthoHardware.ToothRange) + "',"
                                            + "'" + SOut.String(orthoHardware.Note) + "',"
                                            + SOut.Bool(orthoHardware.IsHidden) + ")";
        {
            orthoHardware.OrthoHardwareNum = Db.NonQ(command, true, "OrthoHardwareNum", "orthoHardware");
        }
        return orthoHardware.OrthoHardwareNum;
    }

    public static long InsertNoCache(OrthoHardware orthoHardware)
    {
        return InsertNoCache(orthoHardware, false);
    }

    public static long InsertNoCache(OrthoHardware orthoHardware, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthohardware (";
        if (isRandomKeys || useExistingPK) command += "OrthoHardwareNum,";
        command += "PatNum,DateExam,OrthoHardwareType,OrthoHardwareSpecNum,ToothRange,Note,IsHidden) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoHardware.OrthoHardwareNum) + ",";
        command +=
            SOut.Long(orthoHardware.PatNum) + ","
                                            + SOut.Date(orthoHardware.DateExam) + ","
                                            + SOut.Int((int) orthoHardware.OrthoHardwareType) + ","
                                            + SOut.Long(orthoHardware.OrthoHardwareSpecNum) + ","
                                            + "'" + SOut.String(orthoHardware.ToothRange) + "',"
                                            + "'" + SOut.String(orthoHardware.Note) + "',"
                                            + SOut.Bool(orthoHardware.IsHidden) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoHardware.OrthoHardwareNum = Db.NonQ(command, true, "OrthoHardwareNum", "orthoHardware");
        return orthoHardware.OrthoHardwareNum;
    }

    public static void Update(OrthoHardware orthoHardware)
    {
        var command = "UPDATE orthohardware SET "
                      + "PatNum              =  " + SOut.Long(orthoHardware.PatNum) + ", "
                      + "DateExam            =  " + SOut.Date(orthoHardware.DateExam) + ", "
                      + "OrthoHardwareType   =  " + SOut.Int((int) orthoHardware.OrthoHardwareType) + ", "
                      + "OrthoHardwareSpecNum=  " + SOut.Long(orthoHardware.OrthoHardwareSpecNum) + ", "
                      + "ToothRange          = '" + SOut.String(orthoHardware.ToothRange) + "', "
                      + "Note                = '" + SOut.String(orthoHardware.Note) + "', "
                      + "IsHidden            =  " + SOut.Bool(orthoHardware.IsHidden) + " "
                      + "WHERE OrthoHardwareNum = " + SOut.Long(orthoHardware.OrthoHardwareNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoHardware orthoHardware, OrthoHardware oldOrthoHardware)
    {
        var command = "";
        if (orthoHardware.PatNum != oldOrthoHardware.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(orthoHardware.PatNum) + "";
        }

        if (orthoHardware.DateExam.Date != oldOrthoHardware.DateExam.Date)
        {
            if (command != "") command += ",";
            command += "DateExam = " + SOut.Date(orthoHardware.DateExam) + "";
        }

        if (orthoHardware.OrthoHardwareType != oldOrthoHardware.OrthoHardwareType)
        {
            if (command != "") command += ",";
            command += "OrthoHardwareType = " + SOut.Int((int) orthoHardware.OrthoHardwareType) + "";
        }

        if (orthoHardware.OrthoHardwareSpecNum != oldOrthoHardware.OrthoHardwareSpecNum)
        {
            if (command != "") command += ",";
            command += "OrthoHardwareSpecNum = " + SOut.Long(orthoHardware.OrthoHardwareSpecNum) + "";
        }

        if (orthoHardware.ToothRange != oldOrthoHardware.ToothRange)
        {
            if (command != "") command += ",";
            command += "ToothRange = '" + SOut.String(orthoHardware.ToothRange) + "'";
        }

        if (orthoHardware.Note != oldOrthoHardware.Note)
        {
            if (command != "") command += ",";
            command += "Note = '" + SOut.String(orthoHardware.Note) + "'";
        }

        if (orthoHardware.IsHidden != oldOrthoHardware.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(orthoHardware.IsHidden) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthohardware SET " + command
                                              + " WHERE OrthoHardwareNum = " + SOut.Long(orthoHardware.OrthoHardwareNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoHardware orthoHardware, OrthoHardware oldOrthoHardware)
    {
        if (orthoHardware.PatNum != oldOrthoHardware.PatNum) return true;
        if (orthoHardware.DateExam.Date != oldOrthoHardware.DateExam.Date) return true;
        if (orthoHardware.OrthoHardwareType != oldOrthoHardware.OrthoHardwareType) return true;
        if (orthoHardware.OrthoHardwareSpecNum != oldOrthoHardware.OrthoHardwareSpecNum) return true;
        if (orthoHardware.ToothRange != oldOrthoHardware.ToothRange) return true;
        if (orthoHardware.Note != oldOrthoHardware.Note) return true;
        if (orthoHardware.IsHidden != oldOrthoHardware.IsHidden) return true;
        return false;
    }

    public static void Delete(long orthoHardwareNum)
    {
        var command = "DELETE FROM orthohardware "
                      + "WHERE OrthoHardwareNum = " + SOut.Long(orthoHardwareNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoHardwareNums)
    {
        if (listOrthoHardwareNums == null || listOrthoHardwareNums.Count == 0) return;
        var command = "DELETE FROM orthohardware "
                      + "WHERE OrthoHardwareNum IN(" + string.Join(",", listOrthoHardwareNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}