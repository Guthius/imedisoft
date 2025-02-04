using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class GlobalFormOpenDental
{
    public delegate void GoToModuleDelegate(EnumModuleType moduleType, DateTime? dateSelected = null, List<long> listPinApptNums = null, long selectedAptNum = 0, long claimNum = 0, long patNum = 0, long docNum = 0, bool doShowSearch = false);

    public static GoToModuleDelegate GoToModule;
    
    public static event EventHandler<ModuleEventArgs> EventModuleSelected;
    public static event EventHandler<PatientSelectedEventArgs> EventPatientSelected;
    public static event EventHandler<bool> EventRefreshCurrentModule;
    public static event EventHandler<bool> EventLockODForMountAcquire;

    public delegate bool SendTextDelegate(long patNum, string startingText = "");

    public static SendTextDelegate SendTextMessage;

    public static event EventHandler<List<Signalod>> EventProcessSignalODs;

    public static Control ControlMainForm;

    public static void ProcessSignalODs(List<Signalod> listSignalods)
    {
        EventProcessSignalODs?.Invoke(null, listSignalods);
    }

    public static void LockODForMountAcquire(bool isEnabled)
    {
        EventLockODForMountAcquire?.Invoke(null, isEnabled);
    }

    public static void PatientSelected(Patient patient, bool isRefreshCurModule, bool isApptRefreshDataPat = true, bool hasForcedRefresh = false)
    {
        var e = new PatientSelectedEventArgs();
        e.Patient_ = patient;
        e.IsRefreshCurModule = isRefreshCurModule;
        e.IsApptRefreshDataPat = isApptRefreshDataPat;
        e.HasForcedRefresh = hasForcedRefresh;
        EventPatientSelected?.Invoke(null, e);
    }

    public static void RefreshCurrentModule(bool isClinicRefresh)
    {
        //this must be thread safe because it gets launched from a Word_DocumentBeforeClose event handler which is on a different thread.
        //We also want to launch it async so that the Word UI won't get bogged down.
        if (EventRefreshCurrentModule == null)
        {
            //No subscribers
            return;
        }

        //we need a control in the UI thread in order to use invoke: ControlMainForm
        if (ControlMainForm.InvokeRequired)
        {
            ControlMainForm.BeginInvoke((Action) (() => EventRefreshCurrentModule(null, isClinicRefresh)));
        }
        else
        {
            EventRefreshCurrentModule(null, isClinicRefresh);
        }
    }
}

public class PatientSelectedEventArgs
{
    public Patient Patient_;
    public bool IsRefreshCurModule;
    public bool IsApptRefreshDataPat = true;
    public bool HasForcedRefresh;
}

public class ModuleEventArgs(DateTime dateSelected, List<long> listPinApptNums, long selectedAptNum, EnumModuleType moduleType, long claimNum, long patNum, long docNum, bool doShowSearch = false)
{
    public DateTime DateSelected = dateSelected;
    public List<long> ListPinApptNums = listPinApptNums;
    public long SelectedAptNum = selectedAptNum;
    public EnumModuleType ModuleType = moduleType;
    public long ClaimNum = claimNum;
    public long PatNum = patNum;
    public long DocNum = docNum;
    public bool DoShowSearch = doShowSearch;
}

public enum EnumModuleType
{
    None,
    Appointments,
    Family,
    Account,
    TreatPlan,
    Chart,
    Imaging,
    Manage
}