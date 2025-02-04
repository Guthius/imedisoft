using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ClaimFormItem : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ClaimFormItemNum;

    ///<summary>FK to claimform.ClaimFormNum</summary>
    public long ClaimFormNum;

    ///<summary>If this item is an image.  Usually only one per claimform.  eg ADA2002.emf.  Otherwise it MUST be left blank, or it will trigger an error that the image cannot be found.</summary>
    public string ImageFileName;

    ///<summary>Must be one of the hardcoded available fieldnames for claims.</summary>
    public string FieldName;

    ///<summary>For dates, the format string. ie MM/dd/yyyy or M d y among many other possibilities.</summary>
    public string FormatString;

    public float XPos;
    public float YPos;
    public float Width;
    public float Height;

    public ClaimFormItem Copy()
    {
        return (ClaimFormItem) MemberwiseClone();
    }
}