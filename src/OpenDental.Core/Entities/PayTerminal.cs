using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PayTerminal : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PayTerminalNum;

    public string Name;

    ///<summary>FK to clinic.ClinicNum.</summary>
    public long ClinicNum;

    public string TerminalID;
}