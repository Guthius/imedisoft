using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class UserodApptView : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long UserodApptViewNum;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;

    ///<summary>FK to clinic.ClinicNum.  0 if clinics is not being used or if the user has not been assigned a clinic.</summary>
    public long ClinicNum;

    ///<summary>FK to apptview.ApptViewNum.</summary>
    public long ApptViewNum;
}