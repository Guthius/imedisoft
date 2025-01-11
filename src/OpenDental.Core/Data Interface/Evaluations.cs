using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Evaluations
{
    /*
    
    public static List<Evaluation> Refresh(long patNum){

        string command="SELECT * FROM evaluation WHERE PatNum = "+POut.Long(patNum);
        return Crud.EvaluationCrud.SelectMany(command);
    }
    */
    ///<summary>Gets one Evaluation from the db.</summary>
    public static Evaluation GetOne(long evaluationNum)
    {
        return EvaluationCrud.SelectOne(evaluationNum);
    }

    ///<summary>Gets all Evaluations from the DB.  Multiple filters are available.  Dates must be valid before calling this.</summary>
    public static DataTable GetFilteredList(DateTime dateStart, DateTime dateEnd, string lastName, string firstName, long uniqueID, long courseNum, long instructorNum)
    {
        var command = "SELECT evaluation.EvaluationNum,evaluation.EvalTitle,evaluation.DateEval,evaluation.StudentNum,evaluation.InstructNum,"
                      + "stu.LName,stu.FName,schoolcourse.CourseID,gradingscale.Description,evaluation.OverallGradeShowing FROM evaluation "
                      + "INNER JOIN provider ins ON ins.ProvNum=evaluation.InstructNum "
                      + "INNER JOIN provider stu ON stu.ProvNum=evaluation.StudentNum "
                      + "INNER JOIN schoolcourse ON schoolcourse.SchoolCourseNum=evaluation.SchoolCourseNum "
                      + "INNER JOIN gradingscale ON gradingscale.GradingScaleNum=evaluation.GradingScaleNum "
                      + "WHERE TRUE";
        if (!string.IsNullOrWhiteSpace(lastName)) command += " AND stu.LName LIKE '%" + SOut.String(lastName) + "%'";
        if (!string.IsNullOrWhiteSpace(firstName)) command += " AND stu.FName LIKE '%" + SOut.String(firstName) + "%'";
        if (uniqueID != 0) command += " AND evaluation.StudentNum = '" + SOut.Long(uniqueID) + "'";
        if (courseNum != 0) command += " AND schoolcourse.SchoolCourseNum = '" + SOut.Long(courseNum) + "'";
        if (instructorNum != 0) command += " AND evaluation.InstructNum = '" + SOut.Long(instructorNum) + "'";
        command += " AND evaluation.DateEval BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
        command += " ORDER BY DateEval,LName";
        return DataCore.GetTable(command);
    }

    ///<summary>Gets all Evaluations from the DB.  List filters are available.</summary>
    public static DataTable GetFilteredList(List<long> listCourseNums, List<long> listInstructorNums)
    {
        var command = "SELECT DISTINCT evaluation.StudentNum,stu.LName,stu.FName FROM evaluation "
                      + "INNER JOIN provider ins ON ins.ProvNum=evaluation.InstructNum "
                      + "INNER JOIN provider stu ON stu.ProvNum=evaluation.StudentNum "
                      + "INNER JOIN schoolcourse ON schoolcourse.SchoolCourseNum=evaluation.SchoolCourseNum "
                      + "WHERE TRUE";
        if (listCourseNums != null && listCourseNums.Count != 0)
        {
            command += " AND schoolcourse.SchoolCourseNum IN (";
            for (var i = 0; i < listCourseNums.Count; i++)
            {
                command += "'" + SOut.Long(listCourseNums[i]) + "'";
                if (i != listCourseNums.Count - 1)
                {
                    command += ",";
                    continue;
                }

                command += ")";
            }
        }

        if (listInstructorNums != null && listInstructorNums.Count != 0)
        {
            command += " AND ins.ProvNum IN (";
            for (var i = 0; i < listInstructorNums.Count; i++)
            {
                command += "'" + SOut.Long(listInstructorNums[i]) + "'";
                if (i != listInstructorNums.Count - 1)
                {
                    command += ",";
                    continue;
                }

                command += ")";
            }
        }

        command += " ORDER BY LName,FName";
        return DataCore.GetTable(command);
    }

    
    public static long Insert(Evaluation evaluation)
    {
        return EvaluationCrud.Insert(evaluation);
    }

    
    public static void Update(Evaluation evaluation)
    {
        EvaluationCrud.Update(evaluation);
    }

    
    public static void Delete(long evaluationNum)
    {
        var command = "DELETE FROM evaluation WHERE EvaluationNum = " + SOut.Long(evaluationNum);
        Db.NonQ(command);
    }
}