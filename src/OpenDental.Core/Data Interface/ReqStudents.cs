using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ReqStudents
{
    ///<summary>Returns an empty list if the aptNum passed in is 0.</summary>
    public static List<ReqStudent> GetForAppt(long aptNum)
    {
        if (aptNum == 0) return new List<ReqStudent>();

        var command = "SELECT * FROM reqstudent WHERE AptNum=" + SOut.Long(aptNum) + " ORDER BY ProvNum,Descript";
        return ReqStudentCrud.SelectMany(command);
    }

    public static ReqStudent GetOne(long reqStudentNum)
    {
        var command = "SELECT * FROM reqstudent WHERE ReqStudentNum=" + SOut.Long(reqStudentNum);
        return ReqStudentCrud.SelectOne(reqStudentNum);
    }

    
    public static void Update(ReqStudent reqStudent)
    {
        ReqStudentCrud.Update(reqStudent);
    }

    
    public static long Insert(ReqStudent reqStudent)
    {
        return ReqStudentCrud.Insert(reqStudent);
    }

    ///<summary>Surround with try/catch.</summary>
    public static void Delete(long reqStudentNum)
    {
        var reqStudent = GetOne(reqStudentNum);
        //if a reqneeded exists, then disallow deletion.
        if (ReqNeededs.GetReq(reqStudent.ReqNeededNum) == null) throw new Exception(Lans.g("ReqStudents", "Cannot delete requirement.  Delete the requirement needed instead."));
        var command = "DELETE FROM reqstudent WHERE ReqStudentNum = " + SOut.Long(reqStudentNum);
        Db.NonQ(command);
    }

    public static DataTable RefreshOneStudent(long provNum)
    {
        var table = new DataTable();
        DataRow dataRow;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("appointment");
        table.Columns.Add("course");
        table.Columns.Add("done");
        table.Columns.Add("patient");
        table.Columns.Add("ReqStudentNum");
        table.Columns.Add("requirement");
        var command = "SELECT AptDateTime,CourseID,reqstudent.Descript ReqDescript,"
                      + "schoolcourse.Descript CourseDescript,reqstudent.DateCompleted, "
                      + "patient.LName,patient.FName,patient.MiddleI,patient.Preferred,ProcDescript,reqstudent.ReqStudentNum "
                      + "FROM reqstudent "
                      + "LEFT JOIN schoolcourse ON reqstudent.SchoolCourseNum=schoolcourse.SchoolCourseNum "
                      + "LEFT JOIN patient ON reqstudent.PatNum=patient.PatNum "
                      + "LEFT JOIN appointment ON reqstudent.AptNum=appointment.AptNum "
                      + "WHERE reqstudent.ProvNum=" + SOut.Long(provNum)
                      + " ORDER BY CourseID,ReqDescript";
        var tableRaw = DataCore.GetTable(command);
        DateTime dateTimeApt;
        DateTime dateCompleted;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            dateTimeApt = SIn.DateTime(tableRaw.Rows[i]["AptDateTime"].ToString());
            if (dateTimeApt.Year > 1880)
                dataRow["appointment"] = dateTimeApt.ToShortDateString() + " " + dateTimeApt.ToShortTimeString()
                                         + " " + tableRaw.Rows[i]["ProcDescript"];
            dataRow["course"] = tableRaw.Rows[i]["CourseID"].ToString(); //+" "+raw.Rows[i]["CourseDescript"].ToString();
            dateCompleted = SIn.Date(tableRaw.Rows[i]["DateCompleted"].ToString());
            if (dateCompleted.Year > 1880) dataRow["done"] = "X";
            dataRow["patient"] = PatientLogic.GetNameLF(tableRaw.Rows[i]["LName"].ToString(), tableRaw.Rows[i]["FName"].ToString(),
                tableRaw.Rows[i]["Preferred"].ToString(), tableRaw.Rows[i]["MiddleI"].ToString());
            dataRow["ReqStudentNum"] = tableRaw.Rows[i]["ReqStudentNum"].ToString();
            dataRow["requirement"] = tableRaw.Rows[i]["ReqDescript"].ToString();
            table.Rows.Add(dataRow);
        }

        return table;
    }

    public static DataTable RefreshManyStudents(long schoolClassNum, long schoolCourseNum)
    {
        var table = new DataTable();
        DataRow dataRow;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("donereq");
        table.Columns.Add("FName");
        table.Columns.Add("LName");
        table.Columns.Add("studentNum"); //ProvNum
        table.Columns.Add("totalreq"); //not used yet.  It will be changed to be based upon reqneeded. Or not used at all.
        var command = "SELECT COUNT(DISTINCT req2.ReqStudentNum) donereq,FName,LName,provider.ProvNum,"
                      + "COUNT(DISTINCT req1.ReqStudentNum) totalreq "
                      + "FROM provider "
                      + "LEFT JOIN reqstudent req1 ON req1.ProvNum=provider.ProvNum AND req1.SchoolCourseNum=" + SOut.Long(schoolCourseNum) + " "
                      + "LEFT JOIN reqstudent req2 ON req2.ProvNum=provider.ProvNum AND " + DbHelper.Year("req2.DateCompleted") + " > 1880 "
                      + "AND req2.SchoolCourseNum=" + SOut.Long(schoolCourseNum) + " "
                      + "WHERE provider.SchoolClassNum=" + SOut.Long(schoolClassNum)
                      + " GROUP BY FName,LName,provider.ProvNum "
                      + "ORDER BY LName,FName";
        var tableRaw = DataCore.GetTable(command);
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            dataRow["donereq"] = tableRaw.Rows[i]["donereq"].ToString();
            dataRow["FName"] = tableRaw.Rows[i]["FName"].ToString();
            dataRow["LName"] = tableRaw.Rows[i]["LName"].ToString();
            dataRow["studentNum"] = tableRaw.Rows[i]["ProvNum"].ToString();
            dataRow["totalreq"] = tableRaw.Rows[i]["totalreq"].ToString();
            table.Rows.Add(dataRow);
        }

        return table;
    }

    public static List<Provider> GetStudents(long schoolClassNum)
    {
        return Providers.GetWhere(x => x.SchoolClassNum == schoolClassNum, true);
    }

    ///<summary>Provider(student) is required.</summary>
    public static DataTable GetForCourseClass(long schoolCourseNum, long schoolClassNum)
    {
        var command = "SELECT Descript,ReqNeededNum "
                      + "FROM reqneeded ";
        //if(schoolCourse==0){
        //	command+="WHERE ProvNum="+POut.PInt(provNum);
        //}
        //else{
        command += "WHERE SchoolCourseNum=" + SOut.Long(schoolCourseNum)
                                            //+" AND ProvNum="+POut.PInt(provNum);
                                            //}
                                            + " AND SchoolClassNum=" + SOut.Long(schoolClassNum);
        command += " ORDER BY Descript";
        return DataCore.GetTable(command);
    }


    /// <summary>
    ///     All fields for all reqs will have already been set.  All except for reqstudent.ReqStudentNum if new.  Now,
    ///     they just have to be persisted to the database.
    /// </summary>
    public static void SynchApt(List<ReqStudent> listReqStudentsAttached, List<ReqStudent> listReqStudentsRemoved, long aptNum)
    {
        string command;
        //first, delete all that were removed from this appt
        if (listReqStudentsRemoved.Count(x => x.ReqStudentNum != 0) > 0)
        {
            command = "DELETE FROM reqstudent WHERE ReqStudentNum IN(" + string.Join(",", listReqStudentsRemoved.Where(x => x.ReqStudentNum != 0)
                .Select(x => x.ReqStudentNum)) + ")";
            Db.NonQ(command);
        }

        //second, detach all from this appt
        command = "UPDATE reqstudent SET AptNum=0 WHERE AptNum=" + SOut.Long(aptNum);
        Db.NonQ(command);
        if (listReqStudentsAttached.Count == 0) return;
        for (var i = 0; i < listReqStudentsAttached.Count; i++)
            if (listReqStudentsAttached[i].ReqStudentNum == 0)
                Insert(listReqStudentsAttached[i]);
            else
                Update(listReqStudentsAttached[i]);
    }

    ///<summary>Before reqneeded.Delete, this checks to make sure that req is not in use by students.  Used to prompt user.</summary>
    public static string InUseBy(long reqNeededNum)
    {
        var command = "SELECT LName,FName FROM provider,reqstudent "
                      + "WHERE provider.ProvNum=reqstudent.ProvNum "
                      + "AND reqstudent.ReqNeededNum=" + SOut.Long(reqNeededNum)
                      + " AND reqstudent.DateCompleted > " + SOut.Date(new DateTime(1880, 1, 1));
        var table = DataCore.GetTable(command);
        var retVal = "";
        for (var i = 0; i < table.Rows.Count; i++) retVal += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"] + "\r\n";
        return retVal;
    }

    /*
    ///<summary>Attaches a req to an appointment.  Importantly, it also sets the patNum to match the apt.</summary>
    public static void AttachToApt(int reqStudentNum,int aptNum) {
        string command="SELECT PatNum FROM appointment WHERE AptNum="+POut.PInt(aptNum);
        string patNum=Db.GetCount(command);
        command="UPDATE reqstudent SET AptNum="+POut.PInt(aptNum)
            +", PatNum="+patNum
            +" WHERE ReqStudentNum="+POut.PInt(reqStudentNum);
        Db.NonQ(command);
    }*/
}