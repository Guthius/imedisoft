using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Allergy : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AllergyNum;

    ///<summary>FK to allergydef.AllergyDefNum</summary>
    public long AllergyDefNum;

    ///<summary>FK to patient.PatNum</summary>
    public long PatNum;

    ///<summary>Adverse reaction description.  When importing from eForms, this is where the allergy name goes for "Other" when there's no match.</summary>
    public string Reaction;

    ///<summary>True if still an active allergy.  False helps hide it from the list of active allergies.</summary>
    public bool StatusIsActive;

    ///<summary>The historical date that the patient had the adverse reaction to this agent.</summary>
    public DateTime DateAdverseReaction;

    ///<summary>Snomed code for reaction.  Optional and independent of the Reaction text field.  Not needed for reporting.  Only used for CCD export/import.</summary>
    public string SnomedReaction;
}