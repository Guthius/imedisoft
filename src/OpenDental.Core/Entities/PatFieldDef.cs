using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PatFieldDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PatFieldDefNum;

    ///<summary>This is treated as the key. The name of the field that the user will be allowed to fill in the patient info window.</summary>
    public string FieldName;

    public PatFieldType FieldType;

    ///<summary>Deprecated. Use patfieldpickitem.</summary>
    public string PickList;

    public int ItemOrder;
    public bool IsHidden;

    public PatFieldDef Copy()
    {
        return (PatFieldDef) MemberwiseClone();
    }
}

public enum PatFieldType
{
    Text = 0,
    PickList = 1,

    /// <summary>
    /// Stored in db as entered, already localized.
    /// For example, it could be 2/04/11, 2/4/11, 2/4/2011, or any other variant.
    /// This makes it harder to create queries that filter by date, but easier to display dates as part of results.
    /// </summary>
    Date = 2,

    /// <summary>
    /// If checked, value stored as "1".
    /// If unchecked, row deleted.
    /// </summary>
    Checkbox = 3,

    /// <summary>
    /// Numbers only.
    /// </summary>
    Currency = 4,

    /// <summary>
    /// DEPRECATED. (Only used 16.3.1, deprecated by 16.3.4)
    /// </summary>
    InCaseOfEmergency = 5
}