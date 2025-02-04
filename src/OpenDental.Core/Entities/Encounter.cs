using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Encounter : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EncounterNum;

    ///<summary>FK to patient.PatNum. </summary>
    public long PatNum;

    ///<summary>FK to provider.ProvNum. </summary>
    public long ProvNum;

    ///<summary>FK to ehrcode.CodeValue.  This code may not exist in the ehrcode table, it may have been chosen from a bigger list of available codes.  In that case, this will be a FK to a specific code system table identified by the CodeSystem column.  The code for this item from one of the code systems supported.  Examples: 185349003 or 406547006.</summary>
    public string CodeValue;

    ///<summary>FK to codesystem.CodeSystemName. This will determine which specific code system table the CodeValue is a FK to.  We only allow the following CodeSystems in this table: CDT, CPT, HCPCS, and SNOMEDCT. </summary>
    public string CodeSystem;

    public string Note;

    ///<summary>Date the encounter occurred</summary>
    public DateTime DateEncounter;
}