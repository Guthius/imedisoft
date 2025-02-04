using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HistAppointmentCrud
{
    public static List<HistAppointment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HistAppointment> TableToList(DataTable table)
    {
        var retVal = new List<HistAppointment>();
        foreach (DataRow row in table.Rows)
        {
            var histAppointment = new HistAppointment
            {
                HistApptNum = SIn.Long(row["HistApptNum"].ToString()),
                HistUserNum = SIn.Long(row["HistUserNum"].ToString()),
                HistDateTStamp = SIn.DateTime(row["HistDateTStamp"].ToString()),
                HistApptAction = (HistAppointmentAction) SIn.Int(row["HistApptAction"].ToString()),
                ApptSource = (EServiceTypes) SIn.Int(row["ApptSource"].ToString()),
                AptNum = SIn.Long(row["AptNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                AptStatus = (ApptStatus) SIn.Int(row["AptStatus"].ToString()),
                Pattern = SIn.String(row["Pattern"].ToString()),
                Confirmed = SIn.Long(row["Confirmed"].ToString()),
                TimeLocked = SIn.Bool(row["TimeLocked"].ToString()),
                Op = SIn.Long(row["Op"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                ProvHyg = SIn.Long(row["ProvHyg"].ToString()),
                AptDateTime = SIn.DateTime(row["AptDateTime"].ToString()),
                NextAptNum = SIn.Long(row["NextAptNum"].ToString()),
                UnschedStatus = SIn.Long(row["UnschedStatus"].ToString()),
                IsNewPatient = SIn.Bool(row["IsNewPatient"].ToString()),
                ProcDescript = SIn.String(row["ProcDescript"].ToString()),
                Assistant = SIn.Long(row["Assistant"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                IsHygiene = SIn.Bool(row["IsHygiene"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                DateTimeArrived = SIn.DateTime(row["DateTimeArrived"].ToString()),
                DateTimeSeated = SIn.DateTime(row["DateTimeSeated"].ToString()),
                DateTimeDismissed = SIn.DateTime(row["DateTimeDismissed"].ToString()),
                InsPlan1 = SIn.Long(row["InsPlan1"].ToString()),
                InsPlan2 = SIn.Long(row["InsPlan2"].ToString()),
                DateTimeAskedToArrive = SIn.DateTime(row["DateTimeAskedToArrive"].ToString()),
                ProcsColored = SIn.String(row["ProcsColored"].ToString()),
                ColorOverride = Color.FromArgb(SIn.Int(row["ColorOverride"].ToString())),
                AppointmentTypeNum = SIn.Long(row["AppointmentTypeNum"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                Priority = (ApptPriority) SIn.Int(row["Priority"].ToString()),
                ProvBarText = SIn.String(row["ProvBarText"].ToString()),
                PatternSecondary = SIn.String(row["PatternSecondary"].ToString()),
                SecurityHash = SIn.String(row["SecurityHash"].ToString()),
                ItemOrderPlanned = SIn.Int(row["ItemOrderPlanned"].ToString())
            };
            retVal.Add(histAppointment);
        }

        return retVal;
    }

    public static void Insert(HistAppointment histAppointment)
    {
        var command = "INSERT INTO histappointment (";

        command += "HistUserNum,HistDateTStamp,HistApptAction,ApptSource,AptNum,PatNum,AptStatus,Pattern,Confirmed,TimeLocked,Op,Note,ProvNum,ProvHyg,AptDateTime,NextAptNum,UnschedStatus,IsNewPatient,ProcDescript,Assistant,ClinicNum,IsHygiene,DateTimeArrived,DateTimeSeated,DateTimeDismissed,InsPlan1,InsPlan2,DateTimeAskedToArrive,ProcsColored,ColorOverride,AppointmentTypeNum,SecUserNumEntry,SecDateTEntry,Priority,ProvBarText,PatternSecondary,SecurityHash,ItemOrderPlanned) VALUES(";

        command +=
            SOut.Long(histAppointment.HistUserNum) + ","
                                                   + "NOW()" + ","
                                                   + SOut.Int((int) histAppointment.HistApptAction) + ","
                                                   + SOut.Int((int) histAppointment.ApptSource) + ","
                                                   + SOut.Long(histAppointment.AptNum) + ","
                                                   + SOut.Long(histAppointment.PatNum) + ","
                                                   + SOut.Int((int) histAppointment.AptStatus) + ","
                                                   + "'" + SOut.String(histAppointment.Pattern) + "',"
                                                   + SOut.Long(histAppointment.Confirmed) + ","
                                                   + SOut.Bool(histAppointment.TimeLocked) + ","
                                                   + SOut.Long(histAppointment.Op) + ","
                                                   + DbHelper.ParamChar + "paramNote,"
                                                   + SOut.Long(histAppointment.ProvNum) + ","
                                                   + SOut.Long(histAppointment.ProvHyg) + ","
                                                   + SOut.DateTime(histAppointment.AptDateTime) + ","
                                                   + SOut.Long(histAppointment.NextAptNum) + ","
                                                   + SOut.Long(histAppointment.UnschedStatus) + ","
                                                   + SOut.Bool(histAppointment.IsNewPatient) + ","
                                                   + "'" + SOut.String(histAppointment.ProcDescript) + "',"
                                                   + SOut.Long(histAppointment.Assistant) + ","
                                                   + SOut.Long(histAppointment.ClinicNum) + ","
                                                   + SOut.Bool(histAppointment.IsHygiene) + ","
                                                   //DateTStamp can only be set by MySQL
                                                   + SOut.DateTime(histAppointment.DateTimeArrived) + ","
                                                   + SOut.DateTime(histAppointment.DateTimeSeated) + ","
                                                   + SOut.DateTime(histAppointment.DateTimeDismissed) + ","
                                                   + SOut.Long(histAppointment.InsPlan1) + ","
                                                   + SOut.Long(histAppointment.InsPlan2) + ","
                                                   + SOut.DateTime(histAppointment.DateTimeAskedToArrive) + ","
                                                   + DbHelper.ParamChar + "paramProcsColored,"
                                                   + SOut.Int(histAppointment.ColorOverride.ToArgb()) + ","
                                                   + SOut.Long(histAppointment.AppointmentTypeNum) + ","
                                                   + SOut.Long(histAppointment.SecUserNumEntry) + ","
                                                   + SOut.DateTime(histAppointment.SecDateTEntry) + ","
                                                   + SOut.Int((int) histAppointment.Priority) + ","
                                                   + "'" + SOut.String(histAppointment.ProvBarText) + "',"
                                                   + "'" + SOut.String(histAppointment.PatternSecondary) + "',"
                                                   + "'" + SOut.String(histAppointment.SecurityHash) + "',"
                                                   + SOut.Int(histAppointment.ItemOrderPlanned) + ")";
        if (histAppointment.Note == null) histAppointment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(histAppointment.Note));
        if (histAppointment.ProcsColored == null) histAppointment.ProcsColored = "";
        var paramProcsColored = new OdSqlParameter("paramProcsColored", SOut.StringParam(histAppointment.ProcsColored));
        {
            histAppointment.HistApptNum = Db.NonQ(command, true, "HistApptNum", "histAppointment", paramNote, paramProcsColored);
        }
    }
}