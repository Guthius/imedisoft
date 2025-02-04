using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class UpdateHistory : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long UpdateHistoryNum;

    ///<summary>DateTime that OD was updated to the Version.</summary>
    public DateTime DateTimeUpdated;

    ///<summary>The version that OD was updated to.</summary>
    public string ProgramVersion;

    ///<summary>Obfuscated string containing when and who accepted the license agreement.</summary>
    public string Signature;

    public UpdateHistory()
    {
    }

    public UpdateHistory(string version)
    {
        ProgramVersion = version;
    }
}