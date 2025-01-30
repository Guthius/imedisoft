using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class CertEmployees
{
    public static List<CertEmployee> GetAll()
    {
        return CertEmployeeCrud.SelectMany("SELECT * FROM certemployee");
    }

    public static List<CertEmployee> GetAllForEmployee(long employeeNum)
    {
        return CertEmployeeCrud.SelectMany("SELECT * FROM certemployee WHERE EmployeeNum = " + employeeNum);
    }

    public static List<CertEmployee> GetAllForCert(long certNum)
    {
        return CertEmployeeCrud.SelectMany("SELECT * FROM certemployee WHERE CertNum = " + certNum);
    }

    public static CertEmployee GetOne(long certNum, long employeeNum)
    {
        return CertEmployeeCrud.SelectOne("SELECT * FROM certemployee WHERE CertNum = " + certNum + " AND EmployeeNum = " + employeeNum);
    }

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
}