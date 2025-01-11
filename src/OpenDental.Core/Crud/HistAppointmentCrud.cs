#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HistAppointmentCrud
{
    public static HistAppointment SelectOne(long histApptNum)
    {
        var command = "SELECT * FROM histappointment "
                      + "WHERE HistApptNum = " + SOut.Long(histApptNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HistAppointment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HistAppointment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HistAppointment> TableToList(DataTable table)
    {
        var retVal = new List<HistAppointment>();
        HistAppointment histAppointment;
        foreach (DataRow row in table.Rows)
        {
            histAppointment = new HistAppointment();
            histAppointment.HistApptNum = SIn.Long(row["HistApptNum"].ToString());
            histAppointment.HistUserNum = SIn.Long(row["HistUserNum"].ToString());
            histAppointment.HistDateTStamp = SIn.DateTime(row["HistDateTStamp"].ToString());
            histAppointment.HistApptAction = (HistAppointmentAction) SIn.Int(row["HistApptAction"].ToString());
            histAppointment.ApptSource = (EServiceTypes) SIn.Int(row["ApptSource"].ToString());
            histAppointment.AptNum = SIn.Long(row["AptNum"].ToString());
            histAppointment.PatNum = SIn.Long(row["PatNum"].ToString());
            histAppointment.AptStatus = (ApptStatus) SIn.Int(row["AptStatus"].ToString());
            histAppointment.Pattern = SIn.String(row["Pattern"].ToString());
            histAppointment.Confirmed = SIn.Long(row["Confirmed"].ToString());
            histAppointment.TimeLocked = SIn.Bool(row["TimeLocked"].ToString());
            histAppointment.Op = SIn.Long(row["Op"].ToString());
            histAppointment.Note = SIn.String(row["Note"].ToString());
            histAppointment.ProvNum = SIn.Long(row["ProvNum"].ToString());
            histAppointment.ProvHyg = SIn.Long(row["ProvHyg"].ToString());
            histAppointment.AptDateTime = SIn.DateTime(row["AptDateTime"].ToString());
            histAppointment.NextAptNum = SIn.Long(row["NextAptNum"].ToString());
            histAppointment.UnschedStatus = SIn.Long(row["UnschedStatus"].ToString());
            histAppointment.IsNewPatient = SIn.Bool(row["IsNewPatient"].ToString());
            histAppointment.ProcDescript = SIn.String(row["ProcDescript"].ToString());
            histAppointment.Assistant = SIn.Long(row["Assistant"].ToString());
            histAppointment.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            histAppointment.IsHygiene = SIn.Bool(row["IsHygiene"].ToString());
            histAppointment.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            histAppointment.DateTimeArrived = SIn.DateTime(row["DateTimeArrived"].ToString());
            histAppointment.DateTimeSeated = SIn.DateTime(row["DateTimeSeated"].ToString());
            histAppointment.DateTimeDismissed = SIn.DateTime(row["DateTimeDismissed"].ToString());
            histAppointment.InsPlan1 = SIn.Long(row["InsPlan1"].ToString());
            histAppointment.InsPlan2 = SIn.Long(row["InsPlan2"].ToString());
            histAppointment.DateTimeAskedToArrive = SIn.DateTime(row["DateTimeAskedToArrive"].ToString());
            histAppointment.ProcsColored = SIn.String(row["ProcsColored"].ToString());
            histAppointment.ColorOverride = Color.FromArgb(SIn.Int(row["ColorOverride"].ToString()));
            histAppointment.AppointmentTypeNum = SIn.Long(row["AppointmentTypeNum"].ToString());
            histAppointment.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            histAppointment.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            histAppointment.Priority = (ApptPriority) SIn.Int(row["Priority"].ToString());
            histAppointment.ProvBarText = SIn.String(row["ProvBarText"].ToString());
            histAppointment.PatternSecondary = SIn.String(row["PatternSecondary"].ToString());
            histAppointment.SecurityHash = SIn.String(row["SecurityHash"].ToString());
            histAppointment.ItemOrderPlanned = SIn.Int(row["ItemOrderPlanned"].ToString());
            retVal.Add(histAppointment);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HistAppointment> listHistAppointments, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HistAppointment";
        var table = new DataTable(tableName);
        table.Columns.Add("HistApptNum");
        table.Columns.Add("HistUserNum");
        table.Columns.Add("HistDateTStamp");
        table.Columns.Add("HistApptAction");
        table.Columns.Add("ApptSource");
        table.Columns.Add("AptNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("AptStatus");
        table.Columns.Add("Pattern");
        table.Columns.Add("Confirmed");
        table.Columns.Add("TimeLocked");
        table.Columns.Add("Op");
        table.Columns.Add("Note");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ProvHyg");
        table.Columns.Add("AptDateTime");
        table.Columns.Add("NextAptNum");
        table.Columns.Add("UnschedStatus");
        table.Columns.Add("IsNewPatient");
        table.Columns.Add("ProcDescript");
        table.Columns.Add("Assistant");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("IsHygiene");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("DateTimeArrived");
        table.Columns.Add("DateTimeSeated");
        table.Columns.Add("DateTimeDismissed");
        table.Columns.Add("InsPlan1");
        table.Columns.Add("InsPlan2");
        table.Columns.Add("DateTimeAskedToArrive");
        table.Columns.Add("ProcsColored");
        table.Columns.Add("ColorOverride");
        table.Columns.Add("AppointmentTypeNum");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("Priority");
        table.Columns.Add("ProvBarText");
        table.Columns.Add("PatternSecondary");
        table.Columns.Add("SecurityHash");
        table.Columns.Add("ItemOrderPlanned");
        foreach (var histAppointment in listHistAppointments)
            table.Rows.Add(SOut.Long(histAppointment.HistApptNum), SOut.Long(histAppointment.HistUserNum), SOut.DateT(histAppointment.HistDateTStamp, false), SOut.Int((int) histAppointment.HistApptAction), SOut.Int((int) histAppointment.ApptSource), SOut.Long(histAppointment.AptNum), SOut.Long(histAppointment.PatNum), SOut.Int((int) histAppointment.AptStatus), histAppointment.Pattern, SOut.Long(histAppointment.Confirmed), SOut.Bool(histAppointment.TimeLocked), SOut.Long(histAppointment.Op), histAppointment.Note, SOut.Long(histAppointment.ProvNum), SOut.Long(histAppointment.ProvHyg), SOut.DateT(histAppointment.AptDateTime, false), SOut.Long(histAppointment.NextAptNum), SOut.Long(histAppointment.UnschedStatus), SOut.Bool(histAppointment.IsNewPatient), histAppointment.ProcDescript, SOut.Long(histAppointment.Assistant), SOut.Long(histAppointment.ClinicNum), SOut.Bool(histAppointment.IsHygiene), SOut.DateT(histAppointment.DateTStamp, false), SOut.DateT(histAppointment.DateTimeArrived, false), SOut.DateT(histAppointment.DateTimeSeated, false), SOut.DateT(histAppointment.DateTimeDismissed, false), SOut.Long(histAppointment.InsPlan1), SOut.Long(histAppointment.InsPlan2), SOut.DateT(histAppointment.DateTimeAskedToArrive, false), histAppointment.ProcsColored, SOut.Int(histAppointment.ColorOverride.ToArgb()), SOut.Long(histAppointment.AppointmentTypeNum), SOut.Long(histAppointment.SecUserNumEntry), SOut.DateT(histAppointment.SecDateTEntry, false), SOut.Int((int) histAppointment.Priority), histAppointment.ProvBarText, histAppointment.PatternSecondary, histAppointment.SecurityHash, SOut.Int(histAppointment.ItemOrderPlanned));
        return table;
    }

    public static long Insert(HistAppointment histAppointment)
    {
        return Insert(histAppointment, false);
    }

    public static long Insert(HistAppointment histAppointment, bool useExistingPK)
    {
        var command = "INSERT INTO histappointment (";

        command += "HistUserNum,HistDateTStamp,HistApptAction,ApptSource,AptNum,PatNum,AptStatus,Pattern,Confirmed,TimeLocked,Op,Note,ProvNum,ProvHyg,AptDateTime,NextAptNum,UnschedStatus,IsNewPatient,ProcDescript,Assistant,ClinicNum,IsHygiene,DateTimeArrived,DateTimeSeated,DateTimeDismissed,InsPlan1,InsPlan2,DateTimeAskedToArrive,ProcsColored,ColorOverride,AppointmentTypeNum,SecUserNumEntry,SecDateTEntry,Priority,ProvBarText,PatternSecondary,SecurityHash,ItemOrderPlanned) VALUES(";

        command +=
            SOut.Long(histAppointment.HistUserNum) + ","
                                                   + DbHelper.Now() + ","
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
                                                   + SOut.DateT(histAppointment.AptDateTime) + ","
                                                   + SOut.Long(histAppointment.NextAptNum) + ","
                                                   + SOut.Long(histAppointment.UnschedStatus) + ","
                                                   + SOut.Bool(histAppointment.IsNewPatient) + ","
                                                   + "'" + SOut.String(histAppointment.ProcDescript) + "',"
                                                   + SOut.Long(histAppointment.Assistant) + ","
                                                   + SOut.Long(histAppointment.ClinicNum) + ","
                                                   + SOut.Bool(histAppointment.IsHygiene) + ","
                                                   //DateTStamp can only be set by MySQL
                                                   + SOut.DateT(histAppointment.DateTimeArrived) + ","
                                                   + SOut.DateT(histAppointment.DateTimeSeated) + ","
                                                   + SOut.DateT(histAppointment.DateTimeDismissed) + ","
                                                   + SOut.Long(histAppointment.InsPlan1) + ","
                                                   + SOut.Long(histAppointment.InsPlan2) + ","
                                                   + SOut.DateT(histAppointment.DateTimeAskedToArrive) + ","
                                                   + DbHelper.ParamChar + "paramProcsColored,"
                                                   + SOut.Int(histAppointment.ColorOverride.ToArgb()) + ","
                                                   + SOut.Long(histAppointment.AppointmentTypeNum) + ","
                                                   + SOut.Long(histAppointment.SecUserNumEntry) + ","
                                                   + SOut.DateT(histAppointment.SecDateTEntry) + ","
                                                   + SOut.Int((int) histAppointment.Priority) + ","
                                                   + "'" + SOut.String(histAppointment.ProvBarText) + "',"
                                                   + "'" + SOut.String(histAppointment.PatternSecondary) + "',"
                                                   + "'" + SOut.String(histAppointment.SecurityHash) + "',"
                                                   + SOut.Int(histAppointment.ItemOrderPlanned) + ")";
        if (histAppointment.Note == null) histAppointment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(histAppointment.Note));
        if (histAppointment.ProcsColored == null) histAppointment.ProcsColored = "";
        var paramProcsColored = new OdSqlParameter("paramProcsColored", OdDbType.Text, SOut.StringParam(histAppointment.ProcsColored));
        {
            histAppointment.HistApptNum = Db.NonQ(command, true, "HistApptNum", "histAppointment", paramNote, paramProcsColored);
        }
        return histAppointment.HistApptNum;
    }

    public static long InsertNoCache(HistAppointment histAppointment)
    {
        return InsertNoCache(histAppointment, false);
    }

    public static long InsertNoCache(HistAppointment histAppointment, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO histappointment (";
        if (isRandomKeys || useExistingPK) command += "HistApptNum,";
        command += "HistUserNum,HistDateTStamp,HistApptAction,ApptSource,AptNum,PatNum,AptStatus,Pattern,Confirmed,TimeLocked,Op,Note,ProvNum,ProvHyg,AptDateTime,NextAptNum,UnschedStatus,IsNewPatient,ProcDescript,Assistant,ClinicNum,IsHygiene,DateTimeArrived,DateTimeSeated,DateTimeDismissed,InsPlan1,InsPlan2,DateTimeAskedToArrive,ProcsColored,ColorOverride,AppointmentTypeNum,SecUserNumEntry,SecDateTEntry,Priority,ProvBarText,PatternSecondary,SecurityHash,ItemOrderPlanned) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(histAppointment.HistApptNum) + ",";
        command +=
            SOut.Long(histAppointment.HistUserNum) + ","
                                                   + DbHelper.Now() + ","
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
                                                   + SOut.DateT(histAppointment.AptDateTime) + ","
                                                   + SOut.Long(histAppointment.NextAptNum) + ","
                                                   + SOut.Long(histAppointment.UnschedStatus) + ","
                                                   + SOut.Bool(histAppointment.IsNewPatient) + ","
                                                   + "'" + SOut.String(histAppointment.ProcDescript) + "',"
                                                   + SOut.Long(histAppointment.Assistant) + ","
                                                   + SOut.Long(histAppointment.ClinicNum) + ","
                                                   + SOut.Bool(histAppointment.IsHygiene) + ","
                                                   //DateTStamp can only be set by MySQL
                                                   + SOut.DateT(histAppointment.DateTimeArrived) + ","
                                                   + SOut.DateT(histAppointment.DateTimeSeated) + ","
                                                   + SOut.DateT(histAppointment.DateTimeDismissed) + ","
                                                   + SOut.Long(histAppointment.InsPlan1) + ","
                                                   + SOut.Long(histAppointment.InsPlan2) + ","
                                                   + SOut.DateT(histAppointment.DateTimeAskedToArrive) + ","
                                                   + DbHelper.ParamChar + "paramProcsColored,"
                                                   + SOut.Int(histAppointment.ColorOverride.ToArgb()) + ","
                                                   + SOut.Long(histAppointment.AppointmentTypeNum) + ","
                                                   + SOut.Long(histAppointment.SecUserNumEntry) + ","
                                                   + SOut.DateT(histAppointment.SecDateTEntry) + ","
                                                   + SOut.Int((int) histAppointment.Priority) + ","
                                                   + "'" + SOut.String(histAppointment.ProvBarText) + "',"
                                                   + "'" + SOut.String(histAppointment.PatternSecondary) + "',"
                                                   + "'" + SOut.String(histAppointment.SecurityHash) + "',"
                                                   + SOut.Int(histAppointment.ItemOrderPlanned) + ")";
        if (histAppointment.Note == null) histAppointment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(histAppointment.Note));
        if (histAppointment.ProcsColored == null) histAppointment.ProcsColored = "";
        var paramProcsColored = new OdSqlParameter("paramProcsColored", OdDbType.Text, SOut.StringParam(histAppointment.ProcsColored));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote, paramProcsColored);
        else
            histAppointment.HistApptNum = Db.NonQ(command, true, "HistApptNum", "histAppointment", paramNote, paramProcsColored);
        return histAppointment.HistApptNum;
    }

    public static void Update(HistAppointment histAppointment)
    {
        var command = "UPDATE histappointment SET "
                      + "HistUserNum          =  " + SOut.Long(histAppointment.HistUserNum) + ", "
                      //HistDateTStamp not allowed to change
                      + "HistApptAction       =  " + SOut.Int((int) histAppointment.HistApptAction) + ", "
                      + "ApptSource           =  " + SOut.Int((int) histAppointment.ApptSource) + ", "
                      + "AptNum               =  " + SOut.Long(histAppointment.AptNum) + ", "
                      + "PatNum               =  " + SOut.Long(histAppointment.PatNum) + ", "
                      + "AptStatus            =  " + SOut.Int((int) histAppointment.AptStatus) + ", "
                      + "Pattern              = '" + SOut.String(histAppointment.Pattern) + "', "
                      + "Confirmed            =  " + SOut.Long(histAppointment.Confirmed) + ", "
                      + "TimeLocked           =  " + SOut.Bool(histAppointment.TimeLocked) + ", "
                      + "Op                   =  " + SOut.Long(histAppointment.Op) + ", "
                      + "Note                 =  " + DbHelper.ParamChar + "paramNote, "
                      + "ProvNum              =  " + SOut.Long(histAppointment.ProvNum) + ", "
                      + "ProvHyg              =  " + SOut.Long(histAppointment.ProvHyg) + ", "
                      + "AptDateTime          =  " + SOut.DateT(histAppointment.AptDateTime) + ", "
                      + "NextAptNum           =  " + SOut.Long(histAppointment.NextAptNum) + ", "
                      + "UnschedStatus        =  " + SOut.Long(histAppointment.UnschedStatus) + ", "
                      + "IsNewPatient         =  " + SOut.Bool(histAppointment.IsNewPatient) + ", "
                      + "ProcDescript         = '" + SOut.String(histAppointment.ProcDescript) + "', "
                      + "Assistant            =  " + SOut.Long(histAppointment.Assistant) + ", "
                      + "ClinicNum            =  " + SOut.Long(histAppointment.ClinicNum) + ", "
                      + "IsHygiene            =  " + SOut.Bool(histAppointment.IsHygiene) + ", "
                      //DateTStamp can only be set by MySQL
                      + "DateTimeArrived      =  " + SOut.DateT(histAppointment.DateTimeArrived) + ", "
                      + "DateTimeSeated       =  " + SOut.DateT(histAppointment.DateTimeSeated) + ", "
                      + "DateTimeDismissed    =  " + SOut.DateT(histAppointment.DateTimeDismissed) + ", "
                      + "InsPlan1             =  " + SOut.Long(histAppointment.InsPlan1) + ", "
                      + "InsPlan2             =  " + SOut.Long(histAppointment.InsPlan2) + ", "
                      + "DateTimeAskedToArrive=  " + SOut.DateT(histAppointment.DateTimeAskedToArrive) + ", "
                      + "ProcsColored         =  " + DbHelper.ParamChar + "paramProcsColored, "
                      + "ColorOverride        =  " + SOut.Int(histAppointment.ColorOverride.ToArgb()) + ", "
                      + "AppointmentTypeNum   =  " + SOut.Long(histAppointment.AppointmentTypeNum) + ", "
                      //SecUserNumEntry excluded from update
                      + "SecDateTEntry        =  " + SOut.DateT(histAppointment.SecDateTEntry) + ", "
                      + "Priority             =  " + SOut.Int((int) histAppointment.Priority) + ", "
                      + "ProvBarText          = '" + SOut.String(histAppointment.ProvBarText) + "', "
                      + "PatternSecondary     = '" + SOut.String(histAppointment.PatternSecondary) + "', "
                      + "SecurityHash         = '" + SOut.String(histAppointment.SecurityHash) + "', "
                      + "ItemOrderPlanned     =  " + SOut.Int(histAppointment.ItemOrderPlanned) + " "
                      + "WHERE HistApptNum = " + SOut.Long(histAppointment.HistApptNum);
        if (histAppointment.Note == null) histAppointment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(histAppointment.Note));
        if (histAppointment.ProcsColored == null) histAppointment.ProcsColored = "";
        var paramProcsColored = new OdSqlParameter("paramProcsColored", OdDbType.Text, SOut.StringParam(histAppointment.ProcsColored));
        Db.NonQ(command, paramNote, paramProcsColored);
    }

    public static bool Update(HistAppointment histAppointment, HistAppointment oldHistAppointment)
    {
        var command = "";
        if (histAppointment.HistUserNum != oldHistAppointment.HistUserNum)
        {
            if (command != "") command += ",";
            command += "HistUserNum = " + SOut.Long(histAppointment.HistUserNum) + "";
        }

        //HistDateTStamp not allowed to change
        if (histAppointment.HistApptAction != oldHistAppointment.HistApptAction)
        {
            if (command != "") command += ",";
            command += "HistApptAction = " + SOut.Int((int) histAppointment.HistApptAction) + "";
        }

        if (histAppointment.ApptSource != oldHistAppointment.ApptSource)
        {
            if (command != "") command += ",";
            command += "ApptSource = " + SOut.Int((int) histAppointment.ApptSource) + "";
        }

        if (histAppointment.AptNum != oldHistAppointment.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(histAppointment.AptNum) + "";
        }

        if (histAppointment.PatNum != oldHistAppointment.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(histAppointment.PatNum) + "";
        }

        if (histAppointment.AptStatus != oldHistAppointment.AptStatus)
        {
            if (command != "") command += ",";
            command += "AptStatus = " + SOut.Int((int) histAppointment.AptStatus) + "";
        }

        if (histAppointment.Pattern != oldHistAppointment.Pattern)
        {
            if (command != "") command += ",";
            command += "Pattern = '" + SOut.String(histAppointment.Pattern) + "'";
        }

        if (histAppointment.Confirmed != oldHistAppointment.Confirmed)
        {
            if (command != "") command += ",";
            command += "Confirmed = " + SOut.Long(histAppointment.Confirmed) + "";
        }

        if (histAppointment.TimeLocked != oldHistAppointment.TimeLocked)
        {
            if (command != "") command += ",";
            command += "TimeLocked = " + SOut.Bool(histAppointment.TimeLocked) + "";
        }

        if (histAppointment.Op != oldHistAppointment.Op)
        {
            if (command != "") command += ",";
            command += "Op = " + SOut.Long(histAppointment.Op) + "";
        }

        if (histAppointment.Note != oldHistAppointment.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (histAppointment.ProvNum != oldHistAppointment.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(histAppointment.ProvNum) + "";
        }

        if (histAppointment.ProvHyg != oldHistAppointment.ProvHyg)
        {
            if (command != "") command += ",";
            command += "ProvHyg = " + SOut.Long(histAppointment.ProvHyg) + "";
        }

        if (histAppointment.AptDateTime != oldHistAppointment.AptDateTime)
        {
            if (command != "") command += ",";
            command += "AptDateTime = " + SOut.DateT(histAppointment.AptDateTime) + "";
        }

        if (histAppointment.NextAptNum != oldHistAppointment.NextAptNum)
        {
            if (command != "") command += ",";
            command += "NextAptNum = " + SOut.Long(histAppointment.NextAptNum) + "";
        }

        if (histAppointment.UnschedStatus != oldHistAppointment.UnschedStatus)
        {
            if (command != "") command += ",";
            command += "UnschedStatus = " + SOut.Long(histAppointment.UnschedStatus) + "";
        }

        if (histAppointment.IsNewPatient != oldHistAppointment.IsNewPatient)
        {
            if (command != "") command += ",";
            command += "IsNewPatient = " + SOut.Bool(histAppointment.IsNewPatient) + "";
        }

        if (histAppointment.ProcDescript != oldHistAppointment.ProcDescript)
        {
            if (command != "") command += ",";
            command += "ProcDescript = '" + SOut.String(histAppointment.ProcDescript) + "'";
        }

        if (histAppointment.Assistant != oldHistAppointment.Assistant)
        {
            if (command != "") command += ",";
            command += "Assistant = " + SOut.Long(histAppointment.Assistant) + "";
        }

        if (histAppointment.ClinicNum != oldHistAppointment.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(histAppointment.ClinicNum) + "";
        }

        if (histAppointment.IsHygiene != oldHistAppointment.IsHygiene)
        {
            if (command != "") command += ",";
            command += "IsHygiene = " + SOut.Bool(histAppointment.IsHygiene) + "";
        }

        //DateTStamp can only be set by MySQL
        if (histAppointment.DateTimeArrived != oldHistAppointment.DateTimeArrived)
        {
            if (command != "") command += ",";
            command += "DateTimeArrived = " + SOut.DateT(histAppointment.DateTimeArrived) + "";
        }

        if (histAppointment.DateTimeSeated != oldHistAppointment.DateTimeSeated)
        {
            if (command != "") command += ",";
            command += "DateTimeSeated = " + SOut.DateT(histAppointment.DateTimeSeated) + "";
        }

        if (histAppointment.DateTimeDismissed != oldHistAppointment.DateTimeDismissed)
        {
            if (command != "") command += ",";
            command += "DateTimeDismissed = " + SOut.DateT(histAppointment.DateTimeDismissed) + "";
        }

        if (histAppointment.InsPlan1 != oldHistAppointment.InsPlan1)
        {
            if (command != "") command += ",";
            command += "InsPlan1 = " + SOut.Long(histAppointment.InsPlan1) + "";
        }

        if (histAppointment.InsPlan2 != oldHistAppointment.InsPlan2)
        {
            if (command != "") command += ",";
            command += "InsPlan2 = " + SOut.Long(histAppointment.InsPlan2) + "";
        }

        if (histAppointment.DateTimeAskedToArrive != oldHistAppointment.DateTimeAskedToArrive)
        {
            if (command != "") command += ",";
            command += "DateTimeAskedToArrive = " + SOut.DateT(histAppointment.DateTimeAskedToArrive) + "";
        }

        if (histAppointment.ProcsColored != oldHistAppointment.ProcsColored)
        {
            if (command != "") command += ",";
            command += "ProcsColored = " + DbHelper.ParamChar + "paramProcsColored";
        }

        if (histAppointment.ColorOverride != oldHistAppointment.ColorOverride)
        {
            if (command != "") command += ",";
            command += "ColorOverride = " + SOut.Int(histAppointment.ColorOverride.ToArgb()) + "";
        }

        if (histAppointment.AppointmentTypeNum != oldHistAppointment.AppointmentTypeNum)
        {
            if (command != "") command += ",";
            command += "AppointmentTypeNum = " + SOut.Long(histAppointment.AppointmentTypeNum) + "";
        }

        //SecUserNumEntry excluded from update
        if (histAppointment.SecDateTEntry != oldHistAppointment.SecDateTEntry)
        {
            if (command != "") command += ",";
            command += "SecDateTEntry = " + SOut.DateT(histAppointment.SecDateTEntry) + "";
        }

        if (histAppointment.Priority != oldHistAppointment.Priority)
        {
            if (command != "") command += ",";
            command += "Priority = " + SOut.Int((int) histAppointment.Priority) + "";
        }

        if (histAppointment.ProvBarText != oldHistAppointment.ProvBarText)
        {
            if (command != "") command += ",";
            command += "ProvBarText = '" + SOut.String(histAppointment.ProvBarText) + "'";
        }

        if (histAppointment.PatternSecondary != oldHistAppointment.PatternSecondary)
        {
            if (command != "") command += ",";
            command += "PatternSecondary = '" + SOut.String(histAppointment.PatternSecondary) + "'";
        }

        if (histAppointment.SecurityHash != oldHistAppointment.SecurityHash)
        {
            if (command != "") command += ",";
            command += "SecurityHash = '" + SOut.String(histAppointment.SecurityHash) + "'";
        }

        if (histAppointment.ItemOrderPlanned != oldHistAppointment.ItemOrderPlanned)
        {
            if (command != "") command += ",";
            command += "ItemOrderPlanned = " + SOut.Int(histAppointment.ItemOrderPlanned) + "";
        }

        if (command == "") return false;
        if (histAppointment.Note == null) histAppointment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(histAppointment.Note));
        if (histAppointment.ProcsColored == null) histAppointment.ProcsColored = "";
        var paramProcsColored = new OdSqlParameter("paramProcsColored", OdDbType.Text, SOut.StringParam(histAppointment.ProcsColored));
        command = "UPDATE histappointment SET " + command
                                                + " WHERE HistApptNum = " + SOut.Long(histAppointment.HistApptNum);
        Db.NonQ(command, paramNote, paramProcsColored);
        return true;
    }

    public static bool UpdateComparison(HistAppointment histAppointment, HistAppointment oldHistAppointment)
    {
        if (histAppointment.HistUserNum != oldHistAppointment.HistUserNum) return true;
        //HistDateTStamp not allowed to change
        if (histAppointment.HistApptAction != oldHistAppointment.HistApptAction) return true;
        if (histAppointment.ApptSource != oldHistAppointment.ApptSource) return true;
        if (histAppointment.AptNum != oldHistAppointment.AptNum) return true;
        if (histAppointment.PatNum != oldHistAppointment.PatNum) return true;
        if (histAppointment.AptStatus != oldHistAppointment.AptStatus) return true;
        if (histAppointment.Pattern != oldHistAppointment.Pattern) return true;
        if (histAppointment.Confirmed != oldHistAppointment.Confirmed) return true;
        if (histAppointment.TimeLocked != oldHistAppointment.TimeLocked) return true;
        if (histAppointment.Op != oldHistAppointment.Op) return true;
        if (histAppointment.Note != oldHistAppointment.Note) return true;
        if (histAppointment.ProvNum != oldHistAppointment.ProvNum) return true;
        if (histAppointment.ProvHyg != oldHistAppointment.ProvHyg) return true;
        if (histAppointment.AptDateTime != oldHistAppointment.AptDateTime) return true;
        if (histAppointment.NextAptNum != oldHistAppointment.NextAptNum) return true;
        if (histAppointment.UnschedStatus != oldHistAppointment.UnschedStatus) return true;
        if (histAppointment.IsNewPatient != oldHistAppointment.IsNewPatient) return true;
        if (histAppointment.ProcDescript != oldHistAppointment.ProcDescript) return true;
        if (histAppointment.Assistant != oldHistAppointment.Assistant) return true;
        if (histAppointment.ClinicNum != oldHistAppointment.ClinicNum) return true;
        if (histAppointment.IsHygiene != oldHistAppointment.IsHygiene) return true;
        //DateTStamp can only be set by MySQL
        if (histAppointment.DateTimeArrived != oldHistAppointment.DateTimeArrived) return true;
        if (histAppointment.DateTimeSeated != oldHistAppointment.DateTimeSeated) return true;
        if (histAppointment.DateTimeDismissed != oldHistAppointment.DateTimeDismissed) return true;
        if (histAppointment.InsPlan1 != oldHistAppointment.InsPlan1) return true;
        if (histAppointment.InsPlan2 != oldHistAppointment.InsPlan2) return true;
        if (histAppointment.DateTimeAskedToArrive != oldHistAppointment.DateTimeAskedToArrive) return true;
        if (histAppointment.ProcsColored != oldHistAppointment.ProcsColored) return true;
        if (histAppointment.ColorOverride != oldHistAppointment.ColorOverride) return true;
        if (histAppointment.AppointmentTypeNum != oldHistAppointment.AppointmentTypeNum) return true;
        //SecUserNumEntry excluded from update
        if (histAppointment.SecDateTEntry != oldHistAppointment.SecDateTEntry) return true;
        if (histAppointment.Priority != oldHistAppointment.Priority) return true;
        if (histAppointment.ProvBarText != oldHistAppointment.ProvBarText) return true;
        if (histAppointment.PatternSecondary != oldHistAppointment.PatternSecondary) return true;
        if (histAppointment.SecurityHash != oldHistAppointment.SecurityHash) return true;
        if (histAppointment.ItemOrderPlanned != oldHistAppointment.ItemOrderPlanned) return true;
        return false;
    }

    public static void Delete(long histApptNum)
    {
        var command = "DELETE FROM histappointment "
                      + "WHERE HistApptNum = " + SOut.Long(histApptNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHistApptNums)
    {
        if (listHistApptNums == null || listHistApptNums.Count == 0) return;
        var command = "DELETE FROM histappointment "
                      + "WHERE HistApptNum IN(" + string.Join(",", listHistApptNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}