#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SchoolClassCrud
{
    public static SchoolClass SelectOne(long schoolClassNum)
    {
        var command = "SELECT * FROM schoolclass "
                      + "WHERE SchoolClassNum = " + SOut.Long(schoolClassNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SchoolClass SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SchoolClass> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SchoolClass> TableToList(DataTable table)
    {
        var retVal = new List<SchoolClass>();
        SchoolClass schoolClass;
        foreach (DataRow row in table.Rows)
        {
            schoolClass = new SchoolClass();
            schoolClass.SchoolClassNum = SIn.Long(row["SchoolClassNum"].ToString());
            schoolClass.GradYear = SIn.Int(row["GradYear"].ToString());
            schoolClass.Descript = SIn.String(row["Descript"].ToString());
            retVal.Add(schoolClass);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SchoolClass> listSchoolClasss, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SchoolClass";
        var table = new DataTable(tableName);
        table.Columns.Add("SchoolClassNum");
        table.Columns.Add("GradYear");
        table.Columns.Add("Descript");
        foreach (var schoolClass in listSchoolClasss)
            table.Rows.Add(SOut.Long(schoolClass.SchoolClassNum), SOut.Int(schoolClass.GradYear), schoolClass.Descript);
        return table;
    }

    public static long Insert(SchoolClass schoolClass)
    {
        return Insert(schoolClass, false);
    }

    public static long Insert(SchoolClass schoolClass, bool useExistingPK)
    {
        var command = "INSERT INTO schoolclass (";

        command += "GradYear,Descript) VALUES(";

        command +=
            SOut.Int(schoolClass.GradYear) + ","
                                           + "'" + SOut.String(schoolClass.Descript) + "')";
        {
            schoolClass.SchoolClassNum = Db.NonQ(command, true, "SchoolClassNum", "schoolClass");
        }
        return schoolClass.SchoolClassNum;
    }

    public static long InsertNoCache(SchoolClass schoolClass)
    {
        return InsertNoCache(schoolClass, false);
    }

    public static long InsertNoCache(SchoolClass schoolClass, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO schoolclass (";
        if (isRandomKeys || useExistingPK) command += "SchoolClassNum,";
        command += "GradYear,Descript) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(schoolClass.SchoolClassNum) + ",";
        command +=
            SOut.Int(schoolClass.GradYear) + ","
                                           + "'" + SOut.String(schoolClass.Descript) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            schoolClass.SchoolClassNum = Db.NonQ(command, true, "SchoolClassNum", "schoolClass");
        return schoolClass.SchoolClassNum;
    }

    public static void Update(SchoolClass schoolClass)
    {
        var command = "UPDATE schoolclass SET "
                      + "GradYear      =  " + SOut.Int(schoolClass.GradYear) + ", "
                      + "Descript      = '" + SOut.String(schoolClass.Descript) + "' "
                      + "WHERE SchoolClassNum = " + SOut.Long(schoolClass.SchoolClassNum);
        Db.NonQ(command);
    }

    public static bool Update(SchoolClass schoolClass, SchoolClass oldSchoolClass)
    {
        var command = "";
        if (schoolClass.GradYear != oldSchoolClass.GradYear)
        {
            if (command != "") command += ",";
            command += "GradYear = " + SOut.Int(schoolClass.GradYear) + "";
        }

        if (schoolClass.Descript != oldSchoolClass.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(schoolClass.Descript) + "'";
        }

        if (command == "") return false;
        command = "UPDATE schoolclass SET " + command
                                            + " WHERE SchoolClassNum = " + SOut.Long(schoolClass.SchoolClassNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SchoolClass schoolClass, SchoolClass oldSchoolClass)
    {
        if (schoolClass.GradYear != oldSchoolClass.GradYear) return true;
        if (schoolClass.Descript != oldSchoolClass.Descript) return true;
        return false;
    }

    public static void Delete(long schoolClassNum)
    {
        var command = "DELETE FROM schoolclass "
                      + "WHERE SchoolClassNum = " + SOut.Long(schoolClassNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSchoolClassNums)
    {
        if (listSchoolClassNums == null || listSchoolClassNums.Count == 0) return;
        var command = "DELETE FROM schoolclass "
                      + "WHERE SchoolClassNum IN(" + string.Join(",", listSchoolClassNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}