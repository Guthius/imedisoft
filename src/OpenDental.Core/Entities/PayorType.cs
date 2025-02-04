using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PayorType : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PayorTypeNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>Date of the beginning of new payor type.  End date is the DateStart of the next payor type entry.</summary>
    public DateTime DateStart;

    ///<summary>FK to sop.SopCode. Examples: 121, 3115, etc. </summary>
    public string SopCode;

    public string Note;
}