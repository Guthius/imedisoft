using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class FieldDefLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long FieldDefLinkNum;

    ///<summary>A generic FieldDefNum FK to any particular field def item that will be defined by the FieldDefType column.</summary>
    public long FieldDefNum;

    ///<summary>Enum:FieldDefTypes Defines what FieldDefNum represents.</summary>
    public FieldDefTypes FieldDefType;

    ///<summary>Enum:FieldLocations Defines where this particular field def needs to be hidden.</summary>
    public FieldLocations FieldLocation;

    public FieldDefLink Clone()
    {
        return (FieldDefLink) MemberwiseClone();
    }
}

public enum FieldDefTypes
{
    Appointment,
    Patient
}

public enum FieldLocations
{
    Account,
    AppointmentEdit,
    Chart,
    Family,
    OrthoChart,
    GroupNote
}