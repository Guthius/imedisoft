using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Employee : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EmployeeNum;

    public string LName;
    public string FName;
    public string MiddleI;

    public bool IsHidden;

    ///<summary>This is just text used to quickly display the clockstatus.  eg Working,Break,Lunch,Home, etc.</summary>
    public string ClockStatus;

    ///<summary>The phone extension for the employee.  e.g. 101,102,etc.  This field is only visible for user editing if the pref DockPhonePanelShow is true (1).</summary>
    public int PhoneExt;

    ///<summary>Used to store the payroll identification number used to generate payroll reports. ADP uses six digit number between 000051 and 999999.</summary>
    public string PayrollID;

    public string WirelessPhone;
    public string EmailWork;
    public string EmailPersonal;
    public bool IsFurloughed;
    public bool IsWorkingHome;

    ///<summary>FK to employee.EmployeeNum</summary>
    public long ReportsTo;

    public Employee Copy()
    {
        return (Employee) MemberwiseClone();
    }
}