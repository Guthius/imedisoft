using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ApptFieldDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ApptFieldDefNum;

    ///<summary>The name of the field that the user will be allowed to fill in the appt edit window.  Duplicates are prevented.</summary>
    public string FieldName;

    ///<summary>Enum:ApptFieldType Text=0,PickList=1</summary>
    public ApptFieldType FieldType;

    ///<summary>The text that contains pick list values, each separated by \r\n.  Length 4000.</summary>
    public string PickList;

    public int ItemOrder;

    public ApptFieldDef Clone()
    {
        return (ApptFieldDef) MemberwiseClone();
    }
}

public enum ApptFieldType
{
    ///<summary>0</summary>
    Text,

    ///<summary>1</summary>
    PickList
}