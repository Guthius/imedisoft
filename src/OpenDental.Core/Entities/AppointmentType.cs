using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AppointmentType : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AppointmentTypeNum;

    public string AppointmentTypeName;
    
    public Color AppointmentTypeColor;

    ///<summary>0 based</summary>
    public int ItemOrder;

    public bool IsHidden;
    
    public string Pattern;

    ///<summary>Comma delimited list of procedure codes.  E.g. T1234,T4321,N3214</summary>
    public string CodeStr;

    ///<summary>Comma delimited list of procedure codes that are required for this appt type.  E.g. T1234,T4321,N3214.</summary>
    public string CodeStrRequired;

    ///<summary>Enum:EnumRequiredProcCodesNeeded 0=None,1=AtLeastOne,2=All</summary>
    public EnumRequiredProcCodesNeeded RequiredProcCodesNeeded;

    ///<summary>Comma delimited list of Blockout Types (definition.DefNums where definition.Category=25) this appointment type can be associated to.</summary>
    public string BlockoutTypes;

    public AppointmentType Copy()
    {
        return (AppointmentType) MemberwiseClone();
    }
}

///<summary>Governs how many of the ProcCodes on an AppointmentType that are required on an appointment.</summary>
public enum EnumRequiredProcCodesNeeded
{
    ///<summary>No ProcCodes from CodeStrRequired are needed to schedule appointments of this AppointmentType.</summary>
    None,

    ///<summary>At least one ProcCode from CodeStrRequired is needed to schedule appointments of this AppointmentType.</summary>
    AtLeastOne,

    ///<summary>All ProcCodes from CodeStrRequired are needed to schedule appointments of this AppointmentType.</summary>
    All
}