using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Etrans835Attach : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long Etrans835AttachNum;

    ///<summary>FK to etrans.EtransNum.</summary>
    public long EtransNum;

    ///<summary>FK to claim.ClaimNum.  Can be 0, which indicates that the ERA claim does not have a match in OD.</summary>
    public long ClaimNum;

    ///<summary>Segment index for the CLP/Claim segment within the X12 document containing the 835.
    ///This index is unique, even if there are multiple 835 transactions within the X12 document.</summary>
    public int ClpSegmentIndex;

    ///<summary>Copied from etrans.DateTimeTrans based on EtransNum above.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public DateTime DateTimeTrans;

    ///<summary>Not a database column.  Copied from claim.ClinicNum for the associated ClaimNum foreign key.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public long ClinicNum;

    ///<summary>Not a database column.  Copied from insplan.CarrierNum for insplan.PlanNum=claim.PlanNum of the associated ClaimNum foreign key.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public long CarrierNum;
}