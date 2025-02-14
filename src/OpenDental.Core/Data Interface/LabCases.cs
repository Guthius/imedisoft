using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class LabCases
{
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
                       "(" + (int) ApptStatus.Broken
                       + "," + (int) ApptStatus.Planned
                       + "," + (int) ApptStatus.None
                       + "," + (int) ApptStatus.Scheduled
                       + "," + (int) ApptStatus.UnschedList + ") ";
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

    public static List<LabCase> GetForPeriod(DateTime dateStart, DateTime dateEnd, List<long> listOperatoryNums)
    {
        if (listOperatoryNums != null && listOperatoryNums.Count == 0) return [];

        var command = "SELECT labcase.* FROM labcase,appointment "
                      + "WHERE labcase.AptNum=appointment.AptNum "
                      + "AND (appointment.AptStatus=1 OR appointment.AptStatus=2 OR appointment.AptStatus=4) " //scheduled,complete,or ASAP
                      + "AND AptDateTime >= " + SOut.Date(dateStart)
                      + " AND AptDateTime < " + SOut.Date(dateEnd.AddDays(1)); //midnight of the next morning.
        if (listOperatoryNums != null) command += " AND Op IN (" + string.Join(",", listOperatoryNums) + ")"; //count is at least 1 at this point
        return LabCaseCrud.SelectMany(command);
    }

    public static List<LabCase> GetForPlanned(long aptNum)
    {
        var command = "SELECT * FROM labcase "
                      + "WHERE labcase.PlannedAptNum=" + aptNum;
        return LabCaseCrud.SelectMany(command);
    }

    public static LabCase GetOne(long labCaseNum)
    {
        var command = "SELECT * FROM labcase WHERE LabCaseNum=" + labCaseNum;
        return LabCaseCrud.SelectOne(command);
    }

    public static List<LabCase> GetForPat(long patNum, bool isPlanned)
    {
        var command = "SELECT * FROM labcase WHERE PatNum=" + patNum + " AND ";
        if (isPlanned)
            command += "PlannedAptNum=0 AND AptNum=0"; //We only show lab cases that have not been attached to any kind of appt.
        else
            command += "AptNum=0";
        return LabCaseCrud.SelectMany(command);
    }

    public static void Insert(LabCase labCase)
    {
        LabCaseCrud.Insert(labCase);
    }
    
    public static void Update(LabCase labCase)
    {
        LabCaseCrud.Update(labCase);
    }

    public static void Delete(long labCaseNum)
    {
        //check for dependencies
        var command = "SELECT count(*) FROM sheet,sheetfield "
                      + "WHERE sheet.SheetNum=sheetfield.SheetNum"
                      + " AND sheet.PatNum= (SELECT PatNum FROM labcase WHERE labcase.LabCaseNum=" + labCaseNum + ")"
                      + " AND sheet.SheetType=" + (int) SheetTypeEnum.LabSlip
                      + " AND sheet.IsDeleted=0 "
                      + " AND sheetfield.FieldType=" + (int) SheetFieldType.Parameter
                      + " AND sheetfield.FieldName='LabCaseNum' "
                      + "AND sheetfield.FieldValue='" + labCaseNum + "'";
        if (SIn.Int(Db.GetCount(command)) != 0) throw new Exception(Lans.g("LabCases", "Cannot delete LabCase because lab slip is still attached."));
        //delete
        command = "DELETE FROM labcase WHERE LabCaseNum = " + labCaseNum;
        Db.NonQ(command);
    }

    public static void AttachToAppt(List<long> listLabCaseNums, long aptNum)
    {
        if (listLabCaseNums.IsNullOrEmpty()) return;

        var command = "UPDATE labcase SET AptNum=" + aptNum + " "
                      + "WHERE LabCaseNum IN (" + string.Join(",", listLabCaseNums.Select(x => x).ToArray()) + ")";
        Db.NonQ(command);
    }

    public static void AttachToPlannedAppt(List<long> listLabCaseNums, long plannedAptNum)
    {
        if (listLabCaseNums.IsNullOrEmpty()) return;

        var command = "UPDATE labcase SET PlannedAptNum=" + plannedAptNum + " "
                      + "WHERE LabCaseNum IN (" + string.Join(",", listLabCaseNums.Select(x => x).ToArray()) + ")";
        Db.NonQ(command);
    }

    public static List<LabCase> GetForApt(long aptNum)
    {
        var command = "SELECT * FROM labcase "
                      + "WHERE AptNum=" + aptNum;
        return LabCaseCrud.SelectMany(command);
    }

    public static List<LabCase> GetForApt(Appointment appointment)
    {
        if (appointment.AptNum == 0)
            //A newly created appointment would have no LabCases, so return an empty list
            return [];

        var command = "SELECT * FROM labcase ";
        if (appointment.AptStatus == ApptStatus.Planned)
            command += "WHERE PlannedAptNum=" + appointment.AptNum;
        else
            command += "WHERE AptNum=" + appointment.AptNum;
        return LabCaseCrud.SelectMany(command);
    }
}