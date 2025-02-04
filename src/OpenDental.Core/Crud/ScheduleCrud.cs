using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ScheduleCrud
{
    public static List<Schedule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Schedule> TableToList(DataTable table)
    {
        var retVal = new List<Schedule>();
        foreach (DataRow row in table.Rows)
        {
            var schedule = new Schedule
            {
                ScheduleNum = SIn.Long(row["ScheduleNum"].ToString()),
                SchedDate = SIn.Date(row["SchedDate"].ToString()),
                StartTime = SIn.TimeSpan(row["StartTime"].ToString()),
                StopTime = SIn.TimeSpan(row["StopTime"].ToString()),
                SchedType = (ScheduleType) SIn.Int(row["SchedType"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                BlockoutType = SIn.Long(row["BlockoutType"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                Status = (SchedStatus) SIn.Int(row["Status"].ToString()),
                EmployeeNum = SIn.Long(row["EmployeeNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString())
            };
            retVal.Add(schedule);
        }

        return retVal;
    }

    public static void Insert(Schedule schedule)
    {
        var command = "INSERT INTO schedule (";

        command += "SchedDate,StartTime,StopTime,SchedType,ProvNum,BlockoutType,Note,Status,EmployeeNum,ClinicNum) VALUES(";

        command +=
            SOut.Date(schedule.SchedDate) + ","
                                          + SOut.Time(schedule.StartTime) + ","
                                          + SOut.Time(schedule.StopTime) + ","
                                          + SOut.Int((int) schedule.SchedType) + ","
                                          + SOut.Long(schedule.ProvNum) + ","
                                          + SOut.Long(schedule.BlockoutType) + ","
                                          + DbHelper.ParamChar + "paramNote,"
                                          + SOut.Int((int) schedule.Status) + ","
                                          + SOut.Long(schedule.EmployeeNum) + ","
                                          //DateTStamp can only be set by MySQL
                                          + SOut.Long(schedule.ClinicNum) + ")";
        if (schedule.Note == null) schedule.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(schedule.Note));
        {
            schedule.ScheduleNum = Db.NonQ(command, true, "ScheduleNum", "schedule", paramNote);
        }
    }

    public static void InsertMany(List<Schedule> listSchedules)
    {
        InsertMany(listSchedules, false);
    }

    public static void InsertMany(List<Schedule> listSchedules, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSchedules.Count)
        {
            var schedule = listSchedules[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO schedule (");
                if (useExistingPK) sbCommands.Append("ScheduleNum,");
                sbCommands.Append("SchedDate,StartTime,StopTime,SchedType,ProvNum,BlockoutType,Note,Status,EmployeeNum,ClinicNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(schedule.ScheduleNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Date(schedule.SchedDate));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(schedule.StartTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(schedule.StopTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) schedule.SchedType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(schedule.ProvNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(schedule.BlockoutType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(schedule.Note) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) schedule.Status));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(schedule.EmployeeNum));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(schedule.ClinicNum));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listSchedules.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(Schedule schedule)
    {
        var command = "UPDATE schedule SET "
                      + "SchedDate   =  " + SOut.Date(schedule.SchedDate) + ", "
                      + "StartTime   =  " + SOut.Time(schedule.StartTime) + ", "
                      + "StopTime    =  " + SOut.Time(schedule.StopTime) + ", "
                      + "SchedType   =  " + SOut.Int((int) schedule.SchedType) + ", "
                      + "ProvNum     =  " + SOut.Long(schedule.ProvNum) + ", "
                      + "BlockoutType=  " + SOut.Long(schedule.BlockoutType) + ", "
                      + "Note        =  " + DbHelper.ParamChar + "paramNote, "
                      + "Status      =  " + SOut.Int((int) schedule.Status) + ", "
                      + "EmployeeNum =  " + SOut.Long(schedule.EmployeeNum) + ", "
                      //DateTStamp can only be set by MySQL
                      + "ClinicNum   =  " + SOut.Long(schedule.ClinicNum) + " "
                      + "WHERE ScheduleNum = " + SOut.Long(schedule.ScheduleNum);
        if (schedule.Note == null) schedule.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(schedule.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Update(Schedule schedule, Schedule oldSchedule)
    {
        var command = "";
        if (schedule.SchedDate.Date != oldSchedule.SchedDate.Date)
        {
            if (command != "") command += ",";
            command += "SchedDate = " + SOut.Date(schedule.SchedDate) + "";
        }

        if (schedule.StartTime != oldSchedule.StartTime)
        {
            if (command != "") command += ",";
            command += "StartTime = " + SOut.Time(schedule.StartTime) + "";
        }

        if (schedule.StopTime != oldSchedule.StopTime)
        {
            if (command != "") command += ",";
            command += "StopTime = " + SOut.Time(schedule.StopTime) + "";
        }

        if (schedule.SchedType != oldSchedule.SchedType)
        {
            if (command != "") command += ",";
            command += "SchedType = " + SOut.Int((int) schedule.SchedType) + "";
        }

        if (schedule.ProvNum != oldSchedule.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(schedule.ProvNum) + "";
        }

        if (schedule.BlockoutType != oldSchedule.BlockoutType)
        {
            if (command != "") command += ",";
            command += "BlockoutType = " + SOut.Long(schedule.BlockoutType) + "";
        }

        if (schedule.Note != oldSchedule.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (schedule.Status != oldSchedule.Status)
        {
            if (command != "") command += ",";
            command += "Status = " + SOut.Int((int) schedule.Status) + "";
        }

        if (schedule.EmployeeNum != oldSchedule.EmployeeNum)
        {
            if (command != "") command += ",";
            command += "EmployeeNum = " + SOut.Long(schedule.EmployeeNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (schedule.ClinicNum != oldSchedule.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(schedule.ClinicNum) + "";
        }

        if (command == "") return;
        if (schedule.Note == null) schedule.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(schedule.Note));
        command = "UPDATE schedule SET " + command
                                         + " WHERE ScheduleNum = " + SOut.Long(schedule.ScheduleNum);
        Db.NonQ(command, paramNote);
    }

    public static bool UpdateComparison(Schedule schedule, Schedule oldSchedule)
    {
        if (schedule.SchedDate.Date != oldSchedule.SchedDate.Date) return true;
        if (schedule.StartTime != oldSchedule.StartTime) return true;
        if (schedule.StopTime != oldSchedule.StopTime) return true;
        if (schedule.SchedType != oldSchedule.SchedType) return true;
        if (schedule.ProvNum != oldSchedule.ProvNum) return true;
        if (schedule.BlockoutType != oldSchedule.BlockoutType) return true;
        if (schedule.Note != oldSchedule.Note) return true;
        if (schedule.Status != oldSchedule.Status) return true;
        if (schedule.EmployeeNum != oldSchedule.EmployeeNum) return true;
        //DateTStamp can only be set by MySQL
        if (schedule.ClinicNum != oldSchedule.ClinicNum) return true;
        return false;
    }
}