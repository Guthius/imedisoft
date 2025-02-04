using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProcApptColor : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProcApptColorNum;

    ///<summary>Procedure code range defined by user.  Includes commas and dashes, but no spaces.  The codes need not be valid since they are ranges.</summary>
    public string CodeRange;

    ///<summary>Adds most recent completed date to ProcsColored</summary>
    public bool ShowPreviousDate;

    public Color ColorText;

    public ProcApptColor Copy()
    {
        return (ProcApptColor) MemberwiseClone();
    }
}