using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ApptField : TableBase
{
    ///<summary>FK to appointment.AptNum</summary>
    public long AptNum;

    ///<summary>FK to apptfielddef.FieldName.  The full name is shown here for ease of use when running queries.  But the user is only allowed to change fieldNames in the patFieldDef setup window.</summary>
    public string FieldName;

    ///<summary>Any text that the user types in.  Will later allow some automation.</summary>
    public string FieldValue;
}