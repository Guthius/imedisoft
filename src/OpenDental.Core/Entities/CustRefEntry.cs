using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CustRefEntry : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CustRefEntryNum;

    ///<summary>FK to patient.PatNum.  The customer seeking a reference.</summary>
    public long PatNumCust;

    ///<summary>FK to patient.PatNum.  The chosen reference.  This is the customer who was given as a reference to the new customer.</summary>
    public long PatNumRef;

    ///<summary>Date the reference was chosen.</summary>
    public DateTime DateEntry;

    public string Note;
}