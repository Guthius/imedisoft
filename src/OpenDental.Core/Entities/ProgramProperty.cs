using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProgramProperty : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProgramPropertyNum;

    ///<summary>FK to program.ProgramNum</summary>
    public long ProgramNum;

    ///<summary>The description or prompt for this property.  Blank for workstation overrides of program path.
    ///Many bridges use this description as an "internal description". This way it can act like a FK in order to look up this particular property.  Users cannot edit.</summary>
    public string PropertyDesc;

    ///<summary>The value. Could contain FK to other tables.</summary>
    public string PropertyValue;

    ///<summary>The human-readable name of the computer on the network (not the IP address).  Only used when overriding program path.  Blank for typical Program Properties.</summary>
    public string ComputerName;

    ///<summary>FK to clinic.ClinicNum.  This is only used by a few bridges.  Set to 0 for most bridges.</summary>
    public long ClinicNum;

    ///<summary>Is true if the program property is sensitive information that would need to be masked in the UI. False by default.</summary>
    public bool IsMasked;

    ///<summary>Is true if the program property is a high security property. False by default.</summary>
    public bool IsHighSecurity;

    public ProgramProperty Copy()
    {
        return (ProgramProperty) MemberwiseClone();
    }
}