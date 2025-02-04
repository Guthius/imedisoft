using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InsBlueBookLog : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long InsBlueBookLogNum;

    ///<summary>FK to claimproc.ClaimProcNum. The claimproc for which the estimate was changed.</summary>
    public long ClaimProcNum;

    ///<summary>The new claimproc.InsEstTotal that was calculated by the Blue Book feature.</summary>
    public double AllowedFee;

    ///<summary>The date and time of entry. Not editable by user.</summary>
    public DateTime DateTEntry;

    ///<summary>Explanation of how the Blue Book feature obtained the new insurance estimate.</summary>
    public string Description;
}