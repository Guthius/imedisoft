using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CertEmployee : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CertEmployeeNum;

    ///<summary>FK to cert.CertNum.</summary>
    public long CertNum;

    ///<summary>FK to employee.EmployeeNum.</summary>
    public long EmployeeNum;

    public DateTime DateCompleted;

    public string Note;

    ///<summary>FK to userod.UserNum. The user who made this entry.  Usually some sort of supervisor.</summary>
    public long UserNum;
}