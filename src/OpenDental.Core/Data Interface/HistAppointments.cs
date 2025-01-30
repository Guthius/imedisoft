using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class HistAppointments
{
    public static void Insert(HistAppointment histAppointment)
    {
        HistAppointmentCrud.Insert(histAppointment);
    }

    public static List<HistAppointment> GetForApt(long aptNum)
    {
        var command = "SELECT * FROM histappointment WHERE AptNum=" + SOut.Long(aptNum);
        return HistAppointmentCrud.SelectMany(command);
    }

    public static void CreateHistoryEntry(long apptNum, HistAppointmentAction histAppointmentAction)
    {
        //No need for additional DB check when appt was already deleted.
        Appointment appointment = null;
        if (histAppointmentAction != HistAppointmentAction.Deleted) appointment = Appointments.GetOneApt(apptNum);
        CreateHistoryEntry(appointment, histAppointmentAction, apptNum);
    }

    public static void CreateHistoryEntry(Appointment appointment, HistAppointmentAction histAppointmentAction, long aptNum = 0)
    {
        if (Security.CurUser == null) return;
        var histAppointment = new HistAppointment();
        histAppointment.HistUserNum = Security.CurUser.UserNum;
        histAppointment.ApptSource = Security.CurUser.EServiceType;
        histAppointment.HistApptAction = histAppointmentAction;
        if (appointment != null)
        {
            //Null if deleted
            histAppointment.SetAppt(appointment);
        }
        else
        {
            histAppointment.AptNum = aptNum;
            histAppointment.HistApptAction = HistAppointmentAction.Deleted;
        }

        Insert(histAppointment);
    }
}