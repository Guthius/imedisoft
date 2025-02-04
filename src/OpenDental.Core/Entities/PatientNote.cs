using System;
using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PatientNote : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PatNum;

    ///<summary>Only one note per family stored with guarantor.</summary>
    public string FamFinancial;

    ///<summary>No longer used.</summary>
    public string ApptPhone;

    ///<summary>Medical Summary</summary>
    public string Medical;

    ///<summary>Service notes. Shows in Medical information window and in the Pt Info section of the Chart module.</summary>
    public string Service;

    ///<summary>Complete current Medical History</summary>
    public string MedicalComp;

    ///<summary>Shows in the Chart module normally just below the graphical tooth chart. Also known as Odontogram Notes.</summary>
    public string Treatment;

    ///<summary>In Case of Emergency Name.</summary>
    public string ICEName;

    ///<summary>In Case of Emergency Phone.</summary>
    public string ICEPhone;

    ///<summary>-1 by default. Overrides the default number of months for an ortho treatment for this patient.
    ///Gets automatically set to the current value found in the pref OrthoDefaultMonthsTreat when the first placement procedure has been completed and this value is -1.
    ///This column is an integer instead of a byte because it needs to store -1 so that users can override with the value of 0.
    ///When set to -1 the default practice value for the pref OrthoDefaultMonthsTreat is used.</summary>
    public int OrthoMonthsTreatOverride = -1;

    ///<summary>Overrides the date of the first ortho procedure for this patient to use for ortho case patients. 
    ///If MinDate, then the date is derived by looking at the first ortho procedure for this patient.</summary>
    public DateTime DateOrthoPlacementOverride;

    ///<summary>Timestamp automatically generated and user not allowed to change.  The actual date of entry.</summary>
    public DateTime SecDateTEntry;

    ///<summary>Automatically updated by MySQL every time a row is added or changed.</summary>
    public DateTime SecDateTEdit;

    ///<summary>Enum:PatConsentFlags None=0,ShareMedicationHistoryErx=1, Indicates if the patient consents for DoseSpot to access their medication
    ///history, bitwise.</summary>
    public PatConsentFlags Consent;

    ///<summary>FK to userod.UserNum. A real-time flag of which user is currently editing the Orth Chart.  Prevents concurrency issues. 0 indicates unlocked.</summary>
    public long UserNumOrthoLocked;

    ///<summary>Enum: PronounPreferred Patient Pronoun override. None indicates no override.</summary>
    public PronounPreferred Pronoun;

    public PatientNote Copy()
    {
        return (PatientNote) MemberwiseClone();
    }
}

[Flags]
public enum PatConsentFlags
{
    None = 0,

    [Description("Shared Medication History Erx")]
    ShareMedicationHistoryErx = 1
}

public enum PronounPreferred
{
    None,
    HeHim,
    SheHer,
    TheyThem
}