#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SchoolCourseCrud
{
    public static SchoolCourse SelectOne(long schoolCourseNum)
    {
        var command = "SELECT * FROM schoolcourse "
                      + "WHERE SchoolCourseNum = " + SOut.Long(schoolCourseNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SchoolCourse SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SchoolCourse> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SchoolCourse> TableToList(DataTable table)
    {
        var retVal = new List<SchoolCourse>();
        SchoolCourse schoolCourse;
        foreach (DataRow row in table.Rows)
        {
            schoolCourse = new SchoolCourse();
            schoolCourse.SchoolCourseNum = SIn.Long(row["SchoolCourseNum"].ToString());
            schoolCourse.CourseID = SIn.String(row["CourseID"].ToString());
            schoolCourse.Descript = SIn.String(row["Descript"].ToString());
            retVal.Add(schoolCourse);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SchoolCourse> listSchoolCourses, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SchoolCourse";
        var table = new DataTable(tableName);
        table.Columns.Add("SchoolCourseNum");
        table.Columns.Add("CourseID");
        table.Columns.Add("Descript");
        foreach (var schoolCourse in listSchoolCourses)
            table.Rows.Add(SOut.Long(schoolCourse.SchoolCourseNum), schoolCourse.CourseID, schoolCourse.Descript);
        return table;
    }

    public static long Insert(SchoolCourse schoolCourse)
    {
        return Insert(schoolCourse, false);
    }

    public static long Insert(SchoolCourse schoolCourse, bool useExistingPK)
    {
        var command = "INSERT INTO schoolcourse (";

        command += "CourseID,Descript) VALUES(";

        command +=
            "'" + SOut.String(schoolCourse.CourseID) + "',"
            + "'" + SOut.String(schoolCourse.Descript) + "')";
        {
            schoolCourse.SchoolCourseNum = Db.NonQ(command, true, "SchoolCourseNum", "schoolCourse");
        }
        return schoolCourse.SchoolCourseNum;
    }

    public static long InsertNoCache(SchoolCourse schoolCourse)
    {
        return InsertNoCache(schoolCourse, false);
    }

    public static long InsertNoCache(SchoolCourse schoolCourse, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO schoolcourse (";
        if (isRandomKeys || useExistingPK) command += "SchoolCourseNum,";
        command += "CourseID,Descript) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(schoolCourse.SchoolCourseNum) + ",";
        command +=
            "'" + SOut.String(schoolCourse.CourseID) + "',"
            + "'" + SOut.String(schoolCourse.Descript) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            schoolCourse.SchoolCourseNum = Db.NonQ(command, true, "SchoolCourseNum", "schoolCourse");
        return schoolCourse.SchoolCourseNum;
    }

    public static void Update(SchoolCourse schoolCourse)
    {
        var command = "UPDATE schoolcourse SET "
                      + "CourseID       = '" + SOut.String(schoolCourse.CourseID) + "', "
                      + "Descript       = '" + SOut.String(schoolCourse.Descript) + "' "
                      + "WHERE SchoolCourseNum = " + SOut.Long(schoolCourse.SchoolCourseNum);
        Db.NonQ(command);
    }

    public static bool Update(SchoolCourse schoolCourse, SchoolCourse oldSchoolCourse)
    {
        var command = "";
        if (schoolCourse.CourseID != oldSchoolCourse.CourseID)
        {
            if (command != "") command += ",";
            command += "CourseID = '" + SOut.String(schoolCourse.CourseID) + "'";
        }

        if (schoolCourse.Descript != oldSchoolCourse.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(schoolCourse.Descript) + "'";
        }

        if (command == "") return false;
        command = "UPDATE schoolcourse SET " + command
                                             + " WHERE SchoolCourseNum = " + SOut.Long(schoolCourse.SchoolCourseNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SchoolCourse schoolCourse, SchoolCourse oldSchoolCourse)
    {
        if (schoolCourse.CourseID != oldSchoolCourse.CourseID) return true;
        if (schoolCourse.Descript != oldSchoolCourse.Descript) return true;
        return false;
    }

    public static void Delete(long schoolCourseNum)
    {
        var command = "DELETE FROM schoolcourse "
                      + "WHERE SchoolCourseNum = " + SOut.Long(schoolCourseNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSchoolCourseNums)
    {
        if (listSchoolCourseNums == null || listSchoolCourseNums.Count == 0) return;
        var command = "DELETE FROM schoolcourse "
                      + "WHERE SchoolCourseNum IN(" + string.Join(",", listSchoolCourseNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}