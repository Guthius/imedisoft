using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Pharmacy : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PharmacyNum;

    ///<summary>NCPDPID assigned by NCPDP.  Not used yet.</summary>
    public string PharmID;

    ///<summary>For now, it can just be a common description.  Later, it might have to be an official designation.</summary>
    public string StoreName;

    public string Phone;
    public string Fax;
    public string Address;
    public string Address2;
    public string City;
    public string State;
    public string Zip;
    public string Note;

    ///<summary>The last date and time this row was altered.  Not user editable.</summary>
    public DateTime DateTStamp;

    public Pharmacy Copy()
    {
        return (Pharmacy) MemberwiseClone();
    }
}