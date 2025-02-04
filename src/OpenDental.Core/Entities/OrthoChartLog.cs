using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoChartLog : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoChartLogNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    public string ComputerName;

    ///<summary>DateTime that this log entry was made</summary>	
    public DateTime DateTimeLog;

    ///<summary>DateTime of the chart row.</summary>	
    public DateTime DateTimeService;

    ///<summary>FK to userod.UserNum.  The user that created or last edited an ortho chart field.</summary>
    public long UserNum;

    ///<summary>FK to provider.ProvNum.</summary>
    public long ProvNum;

    ///<summary>FK to orthochartrow.OrthoChartRowNum.</summary>
    public long OrthoChartRowNum;

    ///<summary>This can be long and complex -- whatever you want. MediumText, so max length=16M.</summary>
    public string LogData;
}