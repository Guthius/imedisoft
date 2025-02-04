using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Guardian : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long GuardianNum;

    ///<summary>FK to patient.PatNum.  If Relationship is "Mother", then this PatNum is the child of the mother.</summary>
    public long PatNumChild;

    ///<summary>FK to patient.PatNum.  If Relationship is "Mother", then this is the PatNum of the mother.</summary>
    public long PatNumGuardian;

    public GuardianRelationship Relationship;

    ///<summary>True if this specifies a guardian relationship, or false if any other relationship.
    ///When this flag is true, the relationship will show in the "Guardians" appointment view field and in the family module "Guardians" display field for the patient.  This also grants PHI access in the patient portal to the specific patient designated via PatNumChild.</summary>
    public bool IsGuardian;
}

public enum GuardianRelationship
{
    ///<summary>0 - Added due to feature request.  Needed for EHR.</summary>
    Father,

    ///<summary>1 - Added due to feature request.  Needed for EHR.</summary>
    Mother,

    ///<summary>2 - Added due to feature request.</summary>
    Stepfather,

    ///<summary>3 - Added due to feature request.</summary>
    Stepmother,

    ///<summary>4 - Added due to feature request.</summary>
    Grandfather,

    ///<summary>5 - Added due to feature request.</summary>
    Grandmother,

    ///<summary>6 - Added due to feature request.</summary>
    Sitter,

    ///<summary>7 - Added for EHR.</summary>
    Brother,

    ///<summary>8 - Added for EHR.</summary>
    CareGiver,

    ///<summary>9 - Added for EHR.</summary>
    FosterChild,

    ///<summary>10 - Added for EHR.  Also meets request #154.</summary>
    Guardian,

    ///<summary>11 - Added for EHR.</summary>
    Grandparent,

    ///<summary>12 - Added for EHR.  Also meets request #154.</summary>
    Other,

    ///<summary>13 - Added for EHR.  Also meets request #154.</summary>
    Parent,

    ///<summary>14 - Added for EHR.</summary>
    Stepchild,

    ///<summary>15 - Added for EHR.</summary>
    Self,

    ///<summary>16 - Added for EHR.</summary>
    Sibling,

    ///<summary>17 - Added for EHR.  Also meets request #154.</summary>
    Sister,

    ///<summary>18 - Added for EHR.</summary>
    Spouse,

    ///<summary>19 - Added for EHR.</summary>
    Child,

    ///<summary>20 - Added for EHR.</summary>
    LifePartner,

    ///<summary>21 - Added for EHR.</summary>
    Friend,

    ///<summary>22 - Added for EHR.</summary>
    Grandchild
}