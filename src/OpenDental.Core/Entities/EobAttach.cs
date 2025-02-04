using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EobAttach : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EobAttachNum;

    ///<summary>FK to claimpayment.ClaimPaymentNum</summary>
    public long ClaimPaymentNum;

    ///<summary>Date/time created.</summary>
    public DateTime DateTCreated;

    ///<summary>The file is stored in the A-Z folder in 'EOBs' folder.  This field stores the name of the file.  The files are named automatically based on Date/time along with EobAttachNum for uniqueness.</summary>
    public string FileName;

    ///<summary>The raw file data encoded as base64.  Only used if there is no AtoZ folder.</summary>
    public string RawBase64;
}