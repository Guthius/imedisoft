using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Employer : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EmployerNum;

    public string EmpName;
    public string Address;
    public string Address2;
    public string City;
    public string State;
    public string Zip;
    public string Phone;

    public Employer Copy()
    {
        return (Employer) MemberwiseClone();
    }
}