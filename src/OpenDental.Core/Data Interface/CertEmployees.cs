using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CertEmployees
{
    //Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    #region Methods - Get

    ///<summary>Gets all CertEmployees.</summary>
    public static List<CertEmployee> GetAll()
    {
        var command = "SELECT * FROM certemployee";
        return CertEmployeeCrud.SelectMany(command);
    }

    ///<summary>Gets all CertEmployees for one employee.</summary>
    public static List<CertEmployee> GetAllForEmployee(long employeeNum)
    {
        var command = "SELECT * FROM certemployee WHERE EmployeeNum = " + SOut.Long(employeeNum);
        return CertEmployeeCrud.SelectMany(command);
    }

    ///<summary>Gets all CertEmployees for one Cert.</summary>
    public static List<CertEmployee> GetAllForCert(long certNum)
    {
        var command = "SELECT * FROM certemployee WHERE CertNum = " + SOut.Long(certNum);
        return CertEmployeeCrud.SelectMany(command);
    }

    public static CertEmployee GetOne(long certNum, long employeeNum)
    {
        var command = "SELECT * FROM certemployee WHERE CertNum = " + SOut.Long(certNum) + " AND EmployeeNum = " + SOut.Long(employeeNum);
        return CertEmployeeCrud.SelectOne(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(CertEmployee certEmployee)
    {
        return CertEmployeeCrud.Insert(certEmployee);
    }

    
    public static void Update(CertEmployee certEmployee)
    {
        CertEmployeeCrud.Update(certEmployee);
    }

    
    public static void Delete(long certEmployeeNum)
    {
        CertEmployeeCrud.Delete(certEmployeeNum);
    }

    #endregion Methods - Modify
}