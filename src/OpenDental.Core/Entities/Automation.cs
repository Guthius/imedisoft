using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Automation : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AutomationNum;

    public string Description;

    public EnumAutomationTrigger Autotrigger;

    ///<summary>If this has a CompleteProcedure trigger, this is a comma-delimited list of codes that will trigger the action.</summary>
    public string ProcCodes;

    ///<summary>Enum:AutomationAction The action taken as a result of the trigger.  To get more than one action, create multiple automation entries.</summary>
    public AutomationAction AutoAction;

    ///<summary>FK to sheetdef.SheetDefNum.  If the action is to print a sheet, then this tells which sheet to print.  So it must be a custom sheet.  Also, not that this organization does not allow passing parameters to the sheet such as which procedures were completed, or which appt was broken.</summary>
    public long SheetDefNum;

    ///<summary>FK to definition.DefNum. Only used if action is CreateCommlog.</summary>
    public long CommType;

    ///<summary>If a commlog action, then this is the text that goes in the commlog.  If this is a ShowStatementNoteBold action, then this is the NoteBold. Might later be expanded to work with email or to use variables.</summary>
    public string MessageContent;

    ///<summary>Enum:ApptStatus . This column is not used anymore.</summary>
    public ApptStatus AptStatus;

    ///<summary>FK to appointmenttype.AppointmentTypeNum.</summary>
    public long AppointmentTypeNum;

    ///<summary>Enum:PatientStatus - used to determine which status to change to for ChangePatientStatus automation actions. Should never be 'Deleted'</summary>
    public PatientStatus PatStatus;
    
    public Automation Copy()
    {
        return (Automation) MemberwiseClone();
    }
}

public enum EnumAutomationTrigger
{
    ProcedureComplete = 0,
    ApptBreak = 1,
    ApptNewPatCreate = 2,

    /// <summary>
    /// Regardless of module.
    /// Usually only used with conditions.
    /// </summary>
    PatientOpen = 3,

    ApptCreate = 4,

    /// <summary>
    /// Attaching a procedure to a scheduled appointment.
    /// </summary>
    ProcSchedule = 5,

    BillingTypeSet = 6,
    ClaimCreate = 8,
    ClaimOpen = 9,
    ApptComplete = 10
}

public enum AutomationAction
{
    PrintPatientLetter = 0,
    CreateCommlog = 1,

    /// <summary>
    /// If a referral does not exist for this patient, then notify user instead.
    /// </summary>
    PrintReferralLetter = 2,

    ShowExamSheet = 3,
    PopUp = 4,
    SetApptASAP = 5,
    ShowConsentForm = 6,
    SetApptType = 7,

    /// <summary>
    /// Similar to PopUp, but will only show once per WS per 10 minutes.
    /// </summary>
    PopUpThenDisable10Min = 8,

    /// <summary>
    /// When triggered, automatically restricts patient from being scheduled.
    /// See also PatRestriction.cs
    /// </summary>
    PatRestrictApptSchedTrue = 9,

    /// <summary>
    /// When triggered, automatically removes patient from scheduling restriction.
    /// See also PatRestriction.cs
    /// </summary>
    PatRestrictApptSchedFalse = 10,

    /// <summary>
    /// When triggered, automatically set a patient's status to the status type in the PatStatus column. Delete should never be used.
    /// </summary>
    [Description("Change Pat Status")]
    ChangePatStatus = 12
}