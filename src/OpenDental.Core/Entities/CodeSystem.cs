using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CodeSystem : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CodeSystemNum;

    public string CodeSystemName;

    ///<summary>Only used for display, not actually interpreted. Updated by Code System importer.  Examples: 2013 or 1</summary>
    public string VersionCur;

    ///<summary>Only used for display, not actually interpreted. Updated by Convert DB script.</summary>
    public string VersionAvail;

    ///<summary>Example: 2.16.840.1.113883.6.13</summary>
    public string HL7OID;

    public string Note;

    public CodeSystem()
    {
    }

    public CodeSystem(string name)
    {
        CodeSystemName = name;
    }
}