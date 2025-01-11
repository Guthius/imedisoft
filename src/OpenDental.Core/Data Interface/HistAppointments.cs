using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class HistAppointments
{
    #region Insert

    
    public static long Insert(HistAppointment histAppointment)
    {
        return HistAppointmentCrud.Insert(histAppointment);
    }

    #endregion

    #region Delete

    
    public static void Delete(long histApptNum)
    {
        HistAppointmentCrud.Delete(histApptNum);
    }

    #endregion

    #region Get Methods

    
    public static List<HistAppointment> Refresh(long patNum)
    {
        var command = "SELECT * FROM histappointment WHERE PatNum = " + SOut.Long(patNum);
        return HistAppointmentCrud.SelectMany(command);
    }

    ///<summary>Gets one HistAppointment from the db.</summary>
    public static HistAppointment GetOne(long histApptNum)
    {
        return HistAppointmentCrud.SelectOne(histApptNum);
    }

    ///<summary>Gets histappointments from database.</summary>
    public static List<HistAppointment> GetHistAppointmentsForApi(int limit, int offset,
        DateTime dateTStart, DateTime dateTEnd, long clinicNum, long patNum, int aptStatus, int histApptAction, long aptNum)
    {
        var command = "SELECT * FROM histappointment "
                      + "WHERE AptDateTime >= " + SOut.DateT(dateTStart) + " "
                      + "AND AptDateTime < " + SOut.DateT(dateTEnd) + " ";
        if (clinicNum > -1) command += "AND ClinicNum=" + SOut.Long(clinicNum) + " ";
        if (patNum > 0) command += "AND PatNum=" + SOut.Long(patNum) + " ";
        if (aptStatus > -1) command += "AND AptStatus=" + SOut.Int(aptStatus) + " ";
        if (histApptAction > -1) command += "AND HistApptAction=" + SOut.Int(histApptAction) + " ";
        if (aptNum > 0) command += "AND AptNum=" + SOut.Long(aptNum) + " ";
        command += "ORDER BY HistApptNum " //same fixed order each time
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return HistAppointmentCrud.SelectMany(command);
    }

    public static List<HistAppointment> GetForApt(long aptNum)
    {
        var command = "SELECT * FROM histappointment WHERE AptNum=" + SOut.Long(aptNum);
        return HistAppointmentCrud.SelectMany(command);
    }

    ///<summary>Gets all HistAppointments that have a DateTStamp after dateTimeSince.</summary>
    public static List<HistAppointment> GetChangedSince(DateTime dateTimeSince)
    {
        var command = "SELECT * FROM histappointment WHERE DateTStamp > " + SOut.DateT(dateTimeSince);
        return HistAppointmentCrud.SelectMany(command);
    }

    ///<summary>Gets all AptNums for HistAppointments that have a DateTStamp after dateTimeSince.</summary>
    public static List<long> GetAptNumsChangedSince(DateTime dateTimeSince)
    {
        var command = "SELECT AptNum FROM histappointment WHERE DateTStamp > " + SOut.DateT(dateTimeSince);
        return Db.GetListLong(command);
    }

    #endregion

    #region Misc Methods

    ///<summary>The other overload should be called when the action is Deleted so that the appt's fields will be recorded.</summary>
    public static void CreateHistoryEntry(long apptNum, HistAppointmentAction histAppointmentAction)
    {
        //No need for additional DB check when appt was already deleted.
        Appointment appointment = null;
        if (histAppointmentAction != HistAppointmentAction.Deleted) appointment = Appointments.GetOneApt(apptNum);
        CreateHistoryEntry(appointment, histAppointmentAction, apptNum);
    }

    ///<summary>When appt is null you must provide aptNum and HistApptAction will be set to deleted.</summary>
    public static HistAppointment CreateHistoryEntry(Appointment appointment, HistAppointmentAction histAppointmentAction, long aptNum = 0)
    {
        if (Security.CurUser == null) return null; //There is no user currently logged on so do not create a HistAppointment.
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
        return histAppointment;
    }

    #endregion
}