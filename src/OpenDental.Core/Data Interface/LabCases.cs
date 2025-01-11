using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LabCases
{
    ///<summary>Gets a filtered list of all labcases.</summary>
    public static DataTable Refresh(DateTime dateApptStart, DateTime dateApptEnd, bool showCompleted, bool showUnattached)
    {
        var table = new DataTable();
        DataRow dataRow;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("AptStatus");
        table.Columns.Add("aptStatus");
        table.Columns.Add("AptDateTime", typeof(DateTime));
        table.Columns.Add("aptDateTime");
        table.Columns.Add("AptNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("OpNum");
        table.Columns.Add("lab");
        table.Columns.Add("LabCaseNum");
        table.Columns.Add("patient");
        table.Columns.Add("phone");
        table.Columns.Add("ProcDescript");
        table.Columns.Add("status");
        table.Columns.Add("Instructions");
        var listDataRows = new List<DataRow>();
        //the first query only gets labcases that are attached to scheduled or planned appointments
        var command = "SELECT COALESCE(appointment.AptStatus,ap1.AptStatus) AptStatus, COALESCE(appointment.AptDateTime, ap1.AptDateTime) AS AptDateTime,"
                      + "COALESCE(appointment.AptNum, ap1.AptNum) AS AptNum, COALESCE(appointment.ClinicNum, ap1.ClinicNum) as ClinicNum, COALESCE(appointment.Op, ap1.Op) AS Op, DateTimeChecked,"
                      + "DateTimeRecd,DateTimeSent, LabCaseNum, laboratory.Description, LName, FName, Preferred, MiddleI, Phone,"
                      + "COALESCE(appointment.ProcDescript,ap1.ProcDescript) AS ProcDescript, Instructions "
                      + "FROM labcase "
                      + "LEFT JOIN appointment ap1 ON labcase.AptNum=0 AND labcase.PlannedAptNum=ap1.AptNum "
                      + "LEFT JOIN appointment ON labcase.AptNum > 0 AND labcase.AptNum=appointment.AptNum "
                      + "LEFT JOIN patient ON labcase.PatNum=patient.PatNum "
                      + "LEFT JOIN laboratory ON labcase.LaboratoryNum=laboratory.LaboratoryNum "
                      + "WHERE COALESCE(appointment.AptDateTime,ap1.AptDateTime)"
                      + "BETWEEN DATE(" + SOut.Date(dateApptStart) + ") AND DATE(" + SOut.Date(dateApptEnd.AddDays(1)) + ") ";
        if (!showCompleted)
            command += " AND COALESCE(appointment.AptStatus,ap1.AptStatus) IN " +
                       "(" + SOut.Long((int) ApptStatus.Broken)
                       + "," + SOut.Long((int) ApptStatus.Planned)
                       + "," + SOut.Long((int) ApptStatus.None)
                       + "," + SOut.Long((int) ApptStatus.Scheduled)
                       + "," + SOut.Long((int) ApptStatus.UnschedList) + ") ";
        var tableRaw = DataCore.GetTable(command);
        DateTime dateTimeAppt;
        DateTime dateStatus;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            var apptStatus = (ApptStatus) int.Parse(tableRaw.Rows[i]["AptStatus"].ToString()); //get the obj from the datatable, then convert to enum by casting to string->int->enum
            dataRow["aptStatus"] = Enum.GetName(typeof(ApptStatus), apptStatus);
            dateTimeAppt = SIn.DateTime(tableRaw.Rows[i]["AptDateTime"].ToString());
            dataRow["AptDateTime"] = dateTimeAppt;
            dataRow["aptDateTime"] = dateTimeAppt.ToShortDateString() + (apptStatus != ApptStatus.Planned ? " " + dateTimeAppt.ToShortTimeString() : "");
            dataRow["AptNum"] = tableRaw.Rows[i]["AptNum"].ToString();
            dataRow["ClinicNum"] = tableRaw.Rows[i]["ClinicNum"].ToString();
            dataRow["OpNum"] = tableRaw.Rows[i]["Op"].ToString();
            dataRow["lab"] = tableRaw.Rows[i]["Description"].ToString();
            dataRow["LabCaseNum"] = tableRaw.Rows[i]["LabCaseNum"].ToString();
            dataRow["patient"] = PatientLogic.GetNameLF(tableRaw.Rows[i]["LName"].ToString(), tableRaw.Rows[i]["FName"].ToString(),
                tableRaw.Rows[i]["Preferred"].ToString(), tableRaw.Rows[i]["MiddleI"].ToString());
            dataRow["phone"] = tableRaw.Rows[i]["Phone"].ToString();
            dataRow["ProcDescript"] = tableRaw.Rows[i]["ProcDescript"].ToString();
            dataRow["Instructions"] = tableRaw.Rows[i]["Instructions"].ToString();
            dateStatus = SIn.DateTime(tableRaw.Rows[i]["DateTimeChecked"].ToString());
            if (dateStatus.Year > 1880)
            {
                dataRow["status"] = Lans.g("FormLabCases", "Quality Checked");
                listDataRows.Add(dataRow);
                continue;
            }

            dateStatus = SIn.DateTime(tableRaw.Rows[i]["DateTimeRecd"].ToString());
            if (dateStatus.Year > 1880)
            {
                dataRow["status"] = Lans.g("FormLabCases", "Received");
                listDataRows.Add(dataRow);
                continue;
            }

            dateStatus = SIn.DateTime(tableRaw.Rows[i]["DateTimeSent"].ToString());
            if (dateStatus.Year > 1880)
            {
                dataRow["status"] = Lans.g("FormLabCases", "Sent"); //sent but not received
                listDataRows.Add(dataRow);
                continue;
            }

            dataRow["status"] = Lans.g("FormLabCases", "Not Sent");
            listDataRows.Add(dataRow);
        }

        if (showUnattached)
        {
            //Then, this second query gets labcases not attached to appointments.  No date filter.  No date displayed.
            command = "SELECT DateTimeChecked,DateTimeRecd,DateTimeSent,"
                      + "LabCaseNum,laboratory.Description,LName,FName,Preferred,MiddleI,Phone,Instructions "
                      + "FROM labcase "
                      + "LEFT JOIN patient ON labcase.PatNum=patient.PatNum "
                      + "LEFT JOIN laboratory ON labcase.LaboratoryNum=laboratory.LaboratoryNum "
                      + "WHERE AptNum=0 "
                      + "AND PlannedAptNum=0 ";
            tableRaw = DataCore.GetTable(command);
            for (var i = 0; i < tableRaw.Rows.Count; i++)
            {
                dataRow = table.NewRow();
                dataRow["AptDateTime"] = DateTime.MinValue;
                dataRow["aptDateTime"] = "";
                dataRow["AptNum"] = 0;
                dataRow["lab"] = tableRaw.Rows[i]["Description"].ToString();
                dataRow["LabCaseNum"] = tableRaw.Rows[i]["LabCaseNum"].ToString();
                dataRow["patient"] = PatientLogic.GetNameLF(tableRaw.Rows[i]["LName"].ToString(), tableRaw.Rows[i]["FName"].ToString(),
                    tableRaw.Rows[i]["Preferred"].ToString(), tableRaw.Rows[i]["MiddleI"].ToString());
                dataRow["phone"] = tableRaw.Rows[i]["Phone"].ToString();
                dataRow["ProcDescript"] = "";
                dataRow["status"] = "";
                dataRow["Instructions"] = tableRaw.Rows[i]["Instructions"].ToString();
                dateStatus = SIn.DateTime(tableRaw.Rows[i]["DateTimeChecked"].ToString());
                if (dateStatus.Year > 1880)
                {
                    dataRow["status"] = Lans.g("FormLabCases", "Quality Checked");
                    listDataRows.Add(dataRow);
                    continue;
                }

                dateStatus = SIn.DateTime(tableRaw.Rows[i]["DateTimeRecd"].ToString());
                if (dateStatus.Year > 1880)
                {
                    dataRow["status"] = Lans.g("FormLabCases", "Received");
                    listDataRows.Add(dataRow);
                    continue;
                }

                dateStatus = SIn.DateTime(tableRaw.Rows[i]["DateTimeSent"].ToString());
                if (dateStatus.Year > 1880)
                {
                    dataRow["status"] = Lans.g("FormLabCases", "Sent"); //sent but not received
                    listDataRows.Add(dataRow);
                    continue;
                }

                dataRow["status"] = Lans.g("FormLabCases", "Not Sent");
                listDataRows.Add(dataRow);
            }
        }

        listDataRows = listDataRows.OrderBy(x => x["AptDateTime"]).ToList();
        for (var i = 0; i < listDataRows.Count; i++) table.Rows.Add(listDataRows[i]);
        return table;
    }

    /// <summary>
    ///     Used when drawing the appointments for a day. Send in operatory nums to limit selection, null for all, useful
    ///     for clinic filtering.
    /// </summary>
    public static List<LabCase> GetForPeriod(DateTime dateStart, DateTime dateEnd, List<long> listOperatoryNums)
    {
        if (listOperatoryNums != null && listOperatoryNums.Count == 0) return new List<LabCase>();

        var command = "SELECT labcase.* FROM labcase,appointment "
                      + "WHERE labcase.AptNum=appointment.AptNum "
                      + "AND (appointment.AptStatus=1 OR appointment.AptStatus=2 OR appointment.AptStatus=4) " //scheduled,complete,or ASAP
                      + "AND AptDateTime >= " + SOut.Date(dateStart)
                      + " AND AptDateTime < " + SOut.Date(dateEnd.AddDays(1)); //midnight of the next morning.
        if (listOperatoryNums != null) command += " AND Op IN (" + string.Join(",", listOperatoryNums) + ")"; //count is at least 1 at this point
        return LabCaseCrud.SelectMany(command);
    }

    ///<summary>Used when drawing the planned appointment.</summary>
    public static List<LabCase> GetForPlanned(long aptNum)
    {
        var command = "SELECT * FROM labcase "
                      + "WHERE labcase.PlannedAptNum=" + SOut.Long(aptNum);
        return LabCaseCrud.SelectMany(command);
    }

    ///<summary>Gets one labcase from database.</summary>
    public static LabCase GetOne(long labCaseNum)
    {
        var command = "SELECT * FROM labcase WHERE LabCaseNum=" + SOut.Long(labCaseNum);
        return LabCaseCrud.SelectOne(command);
    }

    /// <summary>
    ///     Gets all labcases for a patient which have not been attached to an appointment.  Usually one or none.  Only
    ///     used when attaching a labcase from within an appointment.
    /// </summary>
    public static List<LabCase> GetForPat(long patNum, bool isPlanned)
    {
        var command = "SELECT * FROM labcase WHERE PatNum=" + SOut.Long(patNum) + " AND ";
        if (isPlanned)
            command += "PlannedAptNum=0 AND AptNum=0"; //We only show lab cases that have not been attached to any kind of appt.
        else
            command += "AptNum=0";
        return LabCaseCrud.SelectMany(command);
    }

    ///<summary>Gets a list of labCases optionally filtered for the API. Returns an empty list if not found.</summary>
    public static List<LabCase> GetLabCasesForApi(int limit, int offset, long patNum, long laboratoryNum, long aptNum, long plannedAptNum, long provNum)
    {
        var command = "SELECT * FROM labcase WHERE DateTStamp>=" + SOut.DateT(DateTime.MinValue) + " ";
        if (patNum > 0) command += "AND PatNum=" + SOut.Long(patNum) + " ";
        if (laboratoryNum > 0) command += "AND LaboratoryNum=" + SOut.Long(laboratoryNum) + " ";
        if (aptNum > -1) command += "AND AptNum=" + SOut.Long(aptNum) + " ";
        if (plannedAptNum > -1) command += "AND PlannedAptNum=" + SOut.Long(plannedAptNum) + " ";
        if (provNum > 0) command += "AND ProvNum=" + SOut.Long(provNum) + " ";
        command += "ORDER BY LabCaseNum " //Ensure order for limit and offset.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return LabCaseCrud.SelectMany(command);
    }

    
    public static long Insert(LabCase labCase)
    {
        return LabCaseCrud.Insert(labCase);
    }

    
    public static void Update(LabCase labCase)
    {
        LabCaseCrud.Update(labCase);
    }

    ///<summary>Surround with try/catch.  Checks dependencies first.  Throws exception if can't delete.</summary>
    public static void Delete(long labCaseNum)
    {
        //check for dependencies
        var command = "SELECT count(*) FROM sheet,sheetfield "
                      + "WHERE sheet.SheetNum=sheetfield.SheetNum"
                      + " AND sheet.PatNum= (SELECT PatNum FROM labcase WHERE labcase.LabCaseNum=" + SOut.Long(labCaseNum) + ")"
                      + " AND sheet.SheetType=" + SOut.Long((int) SheetTypeEnum.LabSlip)
                      + " AND sheet.IsDeleted=0 "
                      + " AND sheetfield.FieldType=" + SOut.Long((int) SheetFieldType.Parameter)
                      + " AND sheetfield.FieldName='LabCaseNum' "
                      + "AND sheetfield.FieldValue='" + SOut.Long(labCaseNum) + "'";
        if (SIn.Int(Db.GetCount(command)) != 0) throw new Exception(Lans.g("LabCases", "Cannot delete LabCase because lab slip is still attached."));
        //delete
        command = "DELETE FROM labcase WHERE LabCaseNum = " + SOut.Long(labCaseNum);
        Db.NonQ(command);
    }

    ///<summary>Attaches labcases to an appointment.</summary>
    public static void AttachToAppt(List<long> listLabCaseNums, long aptNum)
    {
        if (listLabCaseNums.IsNullOrEmpty()) return;

        var command = "UPDATE labcase SET AptNum=" + SOut.Long(aptNum) + " "
                      + "WHERE LabCaseNum IN (" + string.Join(",", listLabCaseNums.Select(x => SOut.Long(x)).ToArray()) + ")";
        Db.NonQ(command);
    }

    ///<summary>Attaches labcases to a planned appointment.</summary>
    public static void AttachToPlannedAppt(List<long> listLabCaseNums, long plannedAptNum)
    {
        if (listLabCaseNums.IsNullOrEmpty()) return;

        var command = "UPDATE labcase SET PlannedAptNum=" + SOut.Long(plannedAptNum) + " "
                      + "WHERE LabCaseNum IN (" + string.Join(",", listLabCaseNums.Select(x => SOut.Long(x)).ToArray()) + ")";
        Db.NonQ(command);
    }

    ///<summary>Frequently returns null.</summary>
    public static LabCase GetOneFromList(List<LabCase> listLabCases, long aptNum)
    {
        for (var i = 0; i < listLabCases.Count; i++)
            if (listLabCases[i].AptNum == aptNum)
                return listLabCases[i];

        return null;
    }

    ///<summary>Gets labcases for an appointment. Used when creating routing slips.</summary>
    public static List<LabCase> GetForApt(long aptNum)
    {
        var command = "SELECT * FROM labcase "
                      + "WHERE AptNum=" + SOut.Long(aptNum);
        return LabCaseCrud.SelectMany(command);
    }

    ///<summary>Gets the labcase for an appointment.  Used in the Appointment Edit window.</summary>
    public static List<LabCase> GetForApt(Appointment appointment)
    {
        if (appointment.AptNum == 0)
            //A newly created appointment would have no LabCases, so return an empty list
            return new List<LabCase>();

        var command = "SELECT * FROM labcase ";
        if (appointment.AptStatus == ApptStatus.Planned)
            command += "WHERE PlannedAptNum=" + SOut.Long(appointment.AptNum);
        else
            command += "WHERE AptNum=" + SOut.Long(appointment.AptNum);
        return LabCaseCrud.SelectMany(command);
    }
}