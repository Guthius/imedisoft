#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReqStudentCrud
{
    public static ReqStudent SelectOne(long reqStudentNum)
    {
        var command = "SELECT * FROM reqstudent "
                      + "WHERE ReqStudentNum = " + SOut.Long(reqStudentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ReqStudent SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ReqStudent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ReqStudent> TableToList(DataTable table)
    {
        var retVal = new List<ReqStudent>();
        ReqStudent reqStudent;
        foreach (DataRow row in table.Rows)
        {
            reqStudent = new ReqStudent();
            reqStudent.ReqStudentNum = SIn.Long(row["ReqStudentNum"].ToString());
            reqStudent.ReqNeededNum = SIn.Long(row["ReqNeededNum"].ToString());
            reqStudent.Descript = SIn.String(row["Descript"].ToString());
            reqStudent.SchoolCourseNum = SIn.Long(row["SchoolCourseNum"].ToString());
            reqStudent.ProvNum = SIn.Long(row["ProvNum"].ToString());
            reqStudent.AptNum = SIn.Long(row["AptNum"].ToString());
            reqStudent.PatNum = SIn.Long(row["PatNum"].ToString());
            reqStudent.InstructorNum = SIn.Long(row["InstructorNum"].ToString());
            reqStudent.DateCompleted = SIn.Date(row["DateCompleted"].ToString());
            retVal.Add(reqStudent);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ReqStudent> listReqStudents, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ReqStudent";
        var table = new DataTable(tableName);
        table.Columns.Add("ReqStudentNum");
        table.Columns.Add("ReqNeededNum");
        table.Columns.Add("Descript");
        table.Columns.Add("SchoolCourseNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("AptNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("InstructorNum");
        table.Columns.Add("DateCompleted");
        foreach (var reqStudent in listReqStudents)
            table.Rows.Add(SOut.Long(reqStudent.ReqStudentNum), SOut.Long(reqStudent.ReqNeededNum), reqStudent.Descript, SOut.Long(reqStudent.SchoolCourseNum), SOut.Long(reqStudent.ProvNum), SOut.Long(reqStudent.AptNum), SOut.Long(reqStudent.PatNum), SOut.Long(reqStudent.InstructorNum), SOut.DateT(reqStudent.DateCompleted, false));
        return table;
    }

    public static long Insert(ReqStudent reqStudent)
    {
        return Insert(reqStudent, false);
    }

    public static long Insert(ReqStudent reqStudent, bool useExistingPK)
    {
        var command = "INSERT INTO reqstudent (";

        command += "ReqNeededNum,Descript,SchoolCourseNum,ProvNum,AptNum,PatNum,InstructorNum,DateCompleted) VALUES(";

        command +=
            SOut.Long(reqStudent.ReqNeededNum) + ","
                                               + "'" + SOut.String(reqStudent.Descript) + "',"
                                               + SOut.Long(reqStudent.SchoolCourseNum) + ","
                                               + SOut.Long(reqStudent.ProvNum) + ","
                                               + SOut.Long(reqStudent.AptNum) + ","
                                               + SOut.Long(reqStudent.PatNum) + ","
                                               + SOut.Long(reqStudent.InstructorNum) + ","
                                               + SOut.Date(reqStudent.DateCompleted) + ")";
        {
            reqStudent.ReqStudentNum = Db.NonQ(command, true, "ReqStudentNum", "reqStudent");
        }
        return reqStudent.ReqStudentNum;
    }

    public static long InsertNoCache(ReqStudent reqStudent)
    {
        return InsertNoCache(reqStudent, false);
    }

    public static long InsertNoCache(ReqStudent reqStudent, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO reqstudent (";
        if (isRandomKeys || useExistingPK) command += "ReqStudentNum,";
        command += "ReqNeededNum,Descript,SchoolCourseNum,ProvNum,AptNum,PatNum,InstructorNum,DateCompleted) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(reqStudent.ReqStudentNum) + ",";
        command +=
            SOut.Long(reqStudent.ReqNeededNum) + ","
                                               + "'" + SOut.String(reqStudent.Descript) + "',"
                                               + SOut.Long(reqStudent.SchoolCourseNum) + ","
                                               + SOut.Long(reqStudent.ProvNum) + ","
                                               + SOut.Long(reqStudent.AptNum) + ","
                                               + SOut.Long(reqStudent.PatNum) + ","
                                               + SOut.Long(reqStudent.InstructorNum) + ","
                                               + SOut.Date(reqStudent.DateCompleted) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            reqStudent.ReqStudentNum = Db.NonQ(command, true, "ReqStudentNum", "reqStudent");
        return reqStudent.ReqStudentNum;
    }

    public static void Update(ReqStudent reqStudent)
    {
        var command = "UPDATE reqstudent SET "
                      + "ReqNeededNum   =  " + SOut.Long(reqStudent.ReqNeededNum) + ", "
                      + "Descript       = '" + SOut.String(reqStudent.Descript) + "', "
                      + "SchoolCourseNum=  " + SOut.Long(reqStudent.SchoolCourseNum) + ", "
                      + "ProvNum        =  " + SOut.Long(reqStudent.ProvNum) + ", "
                      + "AptNum         =  " + SOut.Long(reqStudent.AptNum) + ", "
                      + "PatNum         =  " + SOut.Long(reqStudent.PatNum) + ", "
                      + "InstructorNum  =  " + SOut.Long(reqStudent.InstructorNum) + ", "
                      + "DateCompleted  =  " + SOut.Date(reqStudent.DateCompleted) + " "
                      + "WHERE ReqStudentNum = " + SOut.Long(reqStudent.ReqStudentNum);
        Db.NonQ(command);
    }

    public static bool Update(ReqStudent reqStudent, ReqStudent oldReqStudent)
    {
        var command = "";
        if (reqStudent.ReqNeededNum != oldReqStudent.ReqNeededNum)
        {
            if (command != "") command += ",";
            command += "ReqNeededNum = " + SOut.Long(reqStudent.ReqNeededNum) + "";
        }

        if (reqStudent.Descript != oldReqStudent.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(reqStudent.Descript) + "'";
        }

        if (reqStudent.SchoolCourseNum != oldReqStudent.SchoolCourseNum)
        {
            if (command != "") command += ",";
            command += "SchoolCourseNum = " + SOut.Long(reqStudent.SchoolCourseNum) + "";
        }

        if (reqStudent.ProvNum != oldReqStudent.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(reqStudent.ProvNum) + "";
        }

        if (reqStudent.AptNum != oldReqStudent.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(reqStudent.AptNum) + "";
        }

        if (reqStudent.PatNum != oldReqStudent.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(reqStudent.PatNum) + "";
        }

        if (reqStudent.InstructorNum != oldReqStudent.InstructorNum)
        {
            if (command != "") command += ",";
            command += "InstructorNum = " + SOut.Long(reqStudent.InstructorNum) + "";
        }

        if (reqStudent.DateCompleted.Date != oldReqStudent.DateCompleted.Date)
        {
            if (command != "") command += ",";
            command += "DateCompleted = " + SOut.Date(reqStudent.DateCompleted) + "";
        }

        if (command == "") return false;
        command = "UPDATE reqstudent SET " + command
                                           + " WHERE ReqStudentNum = " + SOut.Long(reqStudent.ReqStudentNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ReqStudent reqStudent, ReqStudent oldReqStudent)
    {
        if (reqStudent.ReqNeededNum != oldReqStudent.ReqNeededNum) return true;
        if (reqStudent.Descript != oldReqStudent.Descript) return true;
        if (reqStudent.SchoolCourseNum != oldReqStudent.SchoolCourseNum) return true;
        if (reqStudent.ProvNum != oldReqStudent.ProvNum) return true;
        if (reqStudent.AptNum != oldReqStudent.AptNum) return true;
        if (reqStudent.PatNum != oldReqStudent.PatNum) return true;
        if (reqStudent.InstructorNum != oldReqStudent.InstructorNum) return true;
        if (reqStudent.DateCompleted.Date != oldReqStudent.DateCompleted.Date) return true;
        return false;
    }

    public static void Delete(long reqStudentNum)
    {
        var command = "DELETE FROM reqstudent "
                      + "WHERE ReqStudentNum = " + SOut.Long(reqStudentNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReqStudentNums)
    {
        if (listReqStudentNums == null || listReqStudentNums.Count == 0) return;
        var command = "DELETE FROM reqstudent "
                      + "WHERE ReqStudentNum IN(" + string.Join(",", listReqStudentNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}