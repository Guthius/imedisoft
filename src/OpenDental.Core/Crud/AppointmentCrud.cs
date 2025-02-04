using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AppointmentCrud
{
    public static Appointment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Appointment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Appointment> TableToList(DataTable table)
    {
        var retVal = new List<Appointment>();
        foreach (DataRow row in table.Rows)
        {
            var appointment = new Appointment
            {
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
            retVal.Add(appointment);
        }

        return retVal;
    }

    public static long Insert(Appointment appointment)
    {
        var command = "INSERT INTO appointment (";

        command += "PatNum,AptStatus,Pattern,Confirmed,TimeLocked,Op,Note,ProvNum,ProvHyg,AptDateTime,NextAptNum,UnschedStatus,IsNewPatient,ProcDescript,Assistant,ClinicNum,IsHygiene,DateTimeArrived,DateTimeSeated,DateTimeDismissed,InsPlan1,InsPlan2,DateTimeAskedToArrive,ProcsColored,ColorOverride,AppointmentTypeNum,SecUserNumEntry,SecDateTEntry,Priority,ProvBarText,PatternSecondary,SecurityHash,ItemOrderPlanned) VALUES(";

        command +=
            SOut.Long(appointment.PatNum) + ","
                                          + SOut.Int((int) appointment.AptStatus) + ","
                                          + "'" + SOut.String(appointment.Pattern) + "',"
                                          + SOut.Long(appointment.Confirmed) + ","
                                          + SOut.Bool(appointment.TimeLocked) + ","
                                          + SOut.Long(appointment.Op) + ","
                                          + DbHelper.ParamChar + "paramNote,"
                                          + SOut.Long(appointment.ProvNum) + ","
                                          + SOut.Long(appointment.ProvHyg) + ","
                                          + SOut.DateTime(appointment.AptDateTime) + ","
                                          + SOut.Long(appointment.NextAptNum) + ","
                                          + SOut.Long(appointment.UnschedStatus) + ","
                                          + SOut.Bool(appointment.IsNewPatient) + ","
                                          + DbHelper.ParamChar + "paramProcDescript,"
                                          + SOut.Long(appointment.Assistant) + ","
                                          + SOut.Long(appointment.ClinicNum) + ","
                                          + SOut.Bool(appointment.IsHygiene) + ","
                                          //DateTStamp can only be set by MySQL
                                          + SOut.DateTime(appointment.DateTimeArrived) + ","
                                          + SOut.DateTime(appointment.DateTimeSeated) + ","
                                          + SOut.DateTime(appointment.DateTimeDismissed) + ","
                                          + SOut.Long(appointment.InsPlan1) + ","
                                          + SOut.Long(appointment.InsPlan2) + ","
                                          + SOut.DateTime(appointment.DateTimeAskedToArrive) + ","
                                          + DbHelper.ParamChar + "paramProcsColored,"
                                          + SOut.Int(appointment.ColorOverride.ToArgb()) + ","
                                          + SOut.Long(appointment.AppointmentTypeNum) + ","
                                          + SOut.Long(appointment.SecUserNumEntry) + ","
                                          + "NOW()" + ","
                                          + SOut.Int((int) appointment.Priority) + ","
                                          + "'" + SOut.String(appointment.ProvBarText) + "',"
                                          + "'" + SOut.String(appointment.PatternSecondary) + "',"
                                          + "'" + SOut.String(appointment.SecurityHash) + "',"
                                          + SOut.Int(appointment.ItemOrderPlanned) + ")";
        if (appointment.Note == null) appointment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(appointment.Note));
        if (appointment.ProcDescript == null) appointment.ProcDescript = "";
        var paramProcDescript = new OdSqlParameter("paramProcDescript", SOut.StringParam(appointment.ProcDescript));
        if (appointment.ProcsColored == null) appointment.ProcsColored = "";
        var paramProcsColored = new OdSqlParameter("paramProcsColored", SOut.StringParam(appointment.ProcsColored));
        {
            appointment.AptNum = Db.NonQ(command, true, "AptNum", "appointment", paramNote, paramProcDescript, paramProcsColored);
        }
        return appointment.AptNum;
    }

    public static bool Update(Appointment appointment, Appointment oldAppointment)
    {
        var command = "";
        if (appointment.PatNum != oldAppointment.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(appointment.PatNum) + "";
        }

        if (appointment.AptStatus != oldAppointment.AptStatus)
        {
            if (command != "") command += ",";
            command += "AptStatus = " + SOut.Int((int) appointment.AptStatus) + "";
        }

        if (appointment.Pattern != oldAppointment.Pattern)
        {
            if (command != "") command += ",";
            command += "Pattern = '" + SOut.String(appointment.Pattern) + "'";
        }

        if (appointment.Confirmed != oldAppointment.Confirmed)
        {
            if (command != "") command += ",";
            command += "Confirmed = " + SOut.Long(appointment.Confirmed) + "";
        }

        if (appointment.TimeLocked != oldAppointment.TimeLocked)
        {
            if (command != "") command += ",";
            command += "TimeLocked = " + SOut.Bool(appointment.TimeLocked) + "";
        }

        if (appointment.Op != oldAppointment.Op)
        {
            if (command != "") command += ",";
            command += "Op = " + SOut.Long(appointment.Op) + "";
        }

        if (appointment.Note != oldAppointment.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (appointment.ProvNum != oldAppointment.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(appointment.ProvNum) + "";
        }

        if (appointment.ProvHyg != oldAppointment.ProvHyg)
        {
            if (command != "") command += ",";
            command += "ProvHyg = " + SOut.Long(appointment.ProvHyg) + "";
        }

        if (appointment.AptDateTime != oldAppointment.AptDateTime)
        {
            if (command != "") command += ",";
            command += "AptDateTime = " + SOut.DateTime(appointment.AptDateTime) + "";
        }

        if (appointment.NextAptNum != oldAppointment.NextAptNum)
        {
            if (command != "") command += ",";
            command += "NextAptNum = " + SOut.Long(appointment.NextAptNum) + "";
        }

        if (appointment.UnschedStatus != oldAppointment.UnschedStatus)
        {
            if (command != "") command += ",";
            command += "UnschedStatus = " + SOut.Long(appointment.UnschedStatus) + "";
        }

        if (appointment.IsNewPatient != oldAppointment.IsNewPatient)
        {
            if (command != "") command += ",";
            command += "IsNewPatient = " + SOut.Bool(appointment.IsNewPatient) + "";
        }

        if (appointment.ProcDescript != oldAppointment.ProcDescript)
        {
            if (command != "") command += ",";
            command += "ProcDescript = " + DbHelper.ParamChar + "paramProcDescript";
        }

        if (appointment.Assistant != oldAppointment.Assistant)
        {
            if (command != "") command += ",";
            command += "Assistant = " + SOut.Long(appointment.Assistant) + "";
        }

        if (appointment.ClinicNum != oldAppointment.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(appointment.ClinicNum) + "";
        }

        if (appointment.IsHygiene != oldAppointment.IsHygiene)
        {
            if (command != "") command += ",";
            command += "IsHygiene = " + SOut.Bool(appointment.IsHygiene) + "";
        }

        //DateTStamp can only be set by MySQL
        if (appointment.DateTimeArrived != oldAppointment.DateTimeArrived)
        {
            if (command != "") command += ",";
            command += "DateTimeArrived = " + SOut.DateTime(appointment.DateTimeArrived) + "";
        }

        if (appointment.DateTimeSeated != oldAppointment.DateTimeSeated)
        {
            if (command != "") command += ",";
            command += "DateTimeSeated = " + SOut.DateTime(appointment.DateTimeSeated) + "";
        }

        if (appointment.DateTimeDismissed != oldAppointment.DateTimeDismissed)
        {
            if (command != "") command += ",";
            command += "DateTimeDismissed = " + SOut.DateTime(appointment.DateTimeDismissed) + "";
        }

        if (appointment.InsPlan1 != oldAppointment.InsPlan1)
        {
            if (command != "") command += ",";
            command += "InsPlan1 = " + SOut.Long(appointment.InsPlan1) + "";
        }

        if (appointment.InsPlan2 != oldAppointment.InsPlan2)
        {
            if (command != "") command += ",";
            command += "InsPlan2 = " + SOut.Long(appointment.InsPlan2) + "";
        }

        if (appointment.DateTimeAskedToArrive != oldAppointment.DateTimeAskedToArrive)
        {
            if (command != "") command += ",";
            command += "DateTimeAskedToArrive = " + SOut.DateTime(appointment.DateTimeAskedToArrive) + "";
        }

        if (appointment.ProcsColored != oldAppointment.ProcsColored)
        {
            if (command != "") command += ",";
            command += "ProcsColored = " + DbHelper.ParamChar + "paramProcsColored";
        }

        if (appointment.ColorOverride != oldAppointment.ColorOverride)
        {
            if (command != "") command += ",";
            command += "ColorOverride = " + SOut.Int(appointment.ColorOverride.ToArgb()) + "";
        }

        if (appointment.AppointmentTypeNum != oldAppointment.AppointmentTypeNum)
        {
            if (command != "") command += ",";
            command += "AppointmentTypeNum = " + SOut.Long(appointment.AppointmentTypeNum) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateTEntry not allowed to change
        if (appointment.Priority != oldAppointment.Priority)
        {
            if (command != "") command += ",";
            command += "Priority = " + SOut.Int((int) appointment.Priority) + "";
        }

        if (appointment.ProvBarText != oldAppointment.ProvBarText)
        {
            if (command != "") command += ",";
            command += "ProvBarText = '" + SOut.String(appointment.ProvBarText) + "'";
        }

        if (appointment.PatternSecondary != oldAppointment.PatternSecondary)
        {
            if (command != "") command += ",";
            command += "PatternSecondary = '" + SOut.String(appointment.PatternSecondary) + "'";
        }

        if (appointment.SecurityHash != oldAppointment.SecurityHash)
        {
            if (command != "") command += ",";
            command += "SecurityHash = '" + SOut.String(appointment.SecurityHash) + "'";
        }

        if (appointment.ItemOrderPlanned != oldAppointment.ItemOrderPlanned)
        {
            if (command != "") command += ",";
            command += "ItemOrderPlanned = " + SOut.Int(appointment.ItemOrderPlanned) + "";
        }

        if (command == "") return false;
        if (appointment.Note == null) appointment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringNote(appointment.Note));
        if (appointment.ProcDescript == null) appointment.ProcDescript = "";
        var paramProcDescript = new OdSqlParameter("paramProcDescript", SOut.StringParam(appointment.ProcDescript));
        if (appointment.ProcsColored == null) appointment.ProcsColored = "";
        var paramProcsColored = new OdSqlParameter("paramProcsColored", SOut.StringParam(appointment.ProcsColored));
        command = "UPDATE appointment SET " + command
                                            + " WHERE AptNum = " + SOut.Long(appointment.AptNum);
        Db.NonQ(command, paramNote, paramProcDescript, paramProcsColored);
        return true;
    }

    public static bool Sync(List<Appointment> listNew, List<Appointment> listDB, long userNum)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Appointment>();
        var listUpdNew = new List<Appointment>();
        var listUpdDB = new List<Appointment>();
        var listDel = new List<Appointment>();
        listNew.Sort((x, y) => { return x.AptNum.CompareTo(y.AptNum); });
        listDB.Sort((x, y) => { return x.AptNum.CompareTo(y.AptNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            Appointment fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            Appointment fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.AptNum < fieldDB.AptNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.AptNum > fieldDB.AptNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++)
        {
            listIns[i].SecUserNumEntry = userNum;
            Insert(listIns[i]);
        }

        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        for (var i = 0; i < listDel.Count; i++) Appointments.Delete(listDel[i].AptNum);
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }

    public static void ClearFkey(long aptNum)
    {
        if (aptNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(aptNum) + " AND PermType IN (96,25,27,26,237,238)";
        Db.NonQ(command);
    }

    public static void ClearFkey(List<long> listAptNums)
    {
        if (listAptNums == null || listAptNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listAptNums.FindAll(x => x != 0)) + ") AND PermType IN (96,25,27,26,237,238)";
        Db.NonQ(command);
    }
}