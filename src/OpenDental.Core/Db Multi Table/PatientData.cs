using System;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public partial class PatientData
{
    public Family Family;
    public long PatNum;
    public Patient Patient;

    public void ClearAll()
    {
        var dataTypes = (EnumPdTable[]) Enum.GetValues(typeof(EnumPdTable));
        
        Clear(dataTypes);
        
        Family = null;
        Patient = null;
        PatientNote = null;
    }

    public void ClearAndFill(params EnumPdTable[] dataTypes)
    {
        Clear(dataTypes);
        FillIfNeeded(dataTypes);
    }
}

public enum EnumPdTable
{
    Patient,
    InsSub,
    InsPlan,
    PatPlan,
    Benefit,
    ClaimProc,
    Adjustment,
    Allergy,
    Appointment,
    ClaimProcHist,
    Disease,
    Document,
    MedicationPat,
    Mount,
    OrthoCase,
    OrthoChart,
    OrthoHardware,
    PatField,
    PatientNote,
    PatientSuperFamHead,
    PatRestriction,
    PayorType,
    PaySplit,
    Procedure,
    ProcGroupItem,
    ProcMultiVisit,
    RefAttach,
    TableProgNotes,
    TablePlannedAppts,
    ToothInitial,
    UserWebHasPortalAccess
}