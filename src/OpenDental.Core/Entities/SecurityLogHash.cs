using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class SecurityLogHash : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SecurityLogHashNum;

    ///<summary>FK to securityLog.SecurityLogNum.</summary>
    public long SecurityLogNum;

    ///<summary>The SHA-256 hash of PermType, UserNum, LogDateTime, LogText, and PatNum, all concatenated together.  This hash has length of 32 bytes encoded as base64.  Used to detect if the entry has been altered outside of Open Dental.</summary>
    public string LogHash;
}