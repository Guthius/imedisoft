using System;
using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class HistAppointment : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long HistApptNum;

    ///<summary>FK to userod.UserNum  Identifies the user that changed this appointment from previous state, not the person who originally wrote it.</summary>
    public long HistUserNum;

    ///<summary>The date and time that this appointment was edited and added to the Hist table.</summary>
    public DateTime HistDateTStamp;

    public HistAppointmentAction HistApptAction;
    public EServiceTypes ApptSource;
    
    #region Copies of Appointment Fields

    ///<summary>Copied from Appointment.</summary>
    public long AptNum;

    ///<summary>Copied from Appointment.</summary>
    public long PatNum;

    ///<summary>Copied from Appointment.</summary>
    public ApptStatus AptStatus;

    ///<summary>Copied from Appointment.</summary>
    public string Pattern;

    ///<summary>Copied from Appointment.</summary>
    public long Confirmed;

    ///<summary>Copied from Appointment.</summary>
    public bool TimeLocked;

    ///<summary>Copied from Appointment.</summary>
    public long Op;

    ///<summary>Copied from Appointment.</summary>
    public string Note;

    ///<summary>Copied from Appointment.</summary>
    public long ProvNum;

    ///<summary>Copied from Appointment.</summary>
    public long ProvHyg;

    ///<summary>Copied from Appointment.</summary>
    public DateTime AptDateTime;

    ///<summary>Copied from Appointment.</summary>
    public long NextAptNum;

    ///<summary>Copied from Appointment.</summary>
    public long UnschedStatus;

    ///<summary>Copied from Appointment.</summary>
    public bool IsNewPatient;

    ///<summary>Copied from Appointment.</summary>
    public string ProcDescript;

    ///<summary>Copied from Appointment.</summary>
    public long Assistant;

    ///<summary>Copied from Appointment.</summary>
    public long ClinicNum;

    ///<summary>Copied from Appointment.</summary>
    public bool IsHygiene;

    ///<summary>Not copied from Appointment. Automatically updated by MySQL every time a row is added or changed.</summary>
    public DateTime DateTStamp;

    ///<summary>Copied from Appointment.</summary>
    public DateTime DateTimeArrived;

    ///<summary>Copied from Appointment.</summary>
    public DateTime DateTimeSeated;

    ///<summary>Copied from Appointment.</summary>
    public DateTime DateTimeDismissed;

    ///<summary>Copied from Appointment.</summary>
    public long InsPlan1;

    ///<summary>Copied from Appointment.</summary>
    public long InsPlan2;

    ///<summary>Copied from Appointment.</summary>
    public DateTime DateTimeAskedToArrive;

    ///<summary>Copied from Appointment.</summary>
    public string ProcsColored;

    ///<summary>Copied from Appointment.</summary>
    public Color ColorOverride;

    ///<summary>Copied from Appointment.</summary>
    public long AppointmentTypeNum;

    ///<summary>Copied from Appointment.</summary>
    public long SecUserNumEntry;

    ///<summary>Copied from Appointment.</summary>
    public DateTime SecDateTEntry;

    ///<summary>Copied from Appointment.</summary>
    public ApptPriority Priority;

    ///<summary>Copied from Appointment.</summary>
    public string ProvBarText;

    ///<summary>Copied from Appointment.</summary>
    public string PatternSecondary;

    ///<summary>Copied from Appointment.</summary>
    public string SecurityHash;

    ///<summary>Copied from Appointment.</summary>
    public int ItemOrderPlanned;

    #endregion Copies of Task Fields

    ///<summary>Pass in the old appointment that needs to be recorded.</summary>
    public HistAppointment(Appointment appt)
    {
        SetAppt(appt);
    }

    ///<summary>Updates the base appointment object but maintains HistAppointment filed values.</summary>
    public void SetAppt(Appointment appt)
    {
        var arrayFieldInfos = typeof(Appointment).GetFields();
        for (var i = 0; i < arrayFieldInfos.Length; i++)
        {
            var fieldInfoHist = typeof(HistAppointment).GetField(arrayFieldInfos[i].Name);
            fieldInfoHist.SetValue(this, arrayFieldInfos[i].GetValue(appt));
        }
    }

    public Appointment ToAppt()
    {
        var appt = new Appointment();
        
        var fieldInfos = typeof(Appointment).GetFields();
        
        foreach (var fieldInfo in fieldInfos)
        {
            var fieldInfoHist = typeof(HistAppointment).GetField(fieldInfo.Name);
            
            fieldInfo.SetValue(appt, fieldInfoHist.GetValue(this));
        }

        return appt;
    }

    public HistAppointment()
    {
    }
}

public enum HistAppointmentAction
{
    Created,
    Changed,
    Missed,
    Cancelled,
    Deleted
}