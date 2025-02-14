using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Employees
{
    public static void Insert(Employee employee)
    {
        if (employee.LName == "" && employee.FName == "")
        {
            throw new ApplicationException("Must include either first name or last name");
        }

        EmployeeCrud.Insert(employee);
    }

    public static void Delete(long employeeNum)
    {
        if (Db.GetCount("SELECT COUNT(*) FROM clockevent WHERE EmployeeNum=" + employeeNum) != "0")
        {
            throw new ApplicationException("Not allowed to delete employee because of attached clock events.");
        }

        if (Db.GetCount("SELECT COUNT(*) FROM timeadjust WHERE EmployeeNum=" + employeeNum) != "0")
        {
            throw new ApplicationException("Not allowed to delete employee because of attached time adjustments.");
        }

        if (Db.GetCount("SELECT COUNT(*) FROM userod WHERE EmployeeNum=" + employeeNum) != "0")
        {
            throw new ApplicationException("Not allowed to delete employee because of attached user.");
        }

        Db.NonQ("UPDATE appointment SET Assistant=0 WHERE Assistant=" + employeeNum);

        var table = DataCore.GetTable("SELECT ScheduleNum FROM schedule WHERE EmployeeNum=" + employeeNum);

        var scheduleNums = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            scheduleNums.Add(table.Rows[i]["ScheduleNum"].ToString());
        }

        if (scheduleNums.Count > 0)
        {
            Db.NonQ("DELETE FROM scheduleop WHERE ScheduleNum IN(" + SOut.String(string.Join(",", scheduleNums)) + ")");
        }

        Db.NonQ("DELETE FROM schedule WHERE EmployeeNum=" + employeeNum);
        Db.NonQ("DELETE FROM employee WHERE EmployeeNum =" + employeeNum);
        Db.NonQ("DELETE FROM timecardrule WHERE EmployeeNum=" + employeeNum);
    }

    public static string GetName(Employee employee)
    {
        return employee.FName + " " + employee.MiddleI + " " + employee.LName;
    }

    public static string GetName(long employeeNum)
    {
        var employee = GetFirstOrDefault(x => x.EmployeeNum == employeeNum);

        return employee == null ? "" : GetName(employee);
    }

    public static string GetAbbr(long employeeNum)
    {
        var employee = GetFirstOrDefault(x => x.EmployeeNum == employeeNum);
        if (employee == null)
        {
            return string.Empty;
        }

        var name = employee.FName;
        if (name.Length > 2)
        {
            name = name.Substring(0, 2);
        }

        return name;
    }

    public static Employee GetEmp(long employeeNum)
    {
        return GetFirstOrDefault(x => x.EmployeeNum == employeeNum);
    }

    public static List<Employee> GetEmpsForClinic(long clinicNum, bool isAll = false, bool getUnassigned = false)
    {
        var employees = GetDeepCopy(true);
        if (clinicNum == 0 && isAll)
        {
            return employees;
        }

        var employeesWithClinic = new List<Employee>();
        var unassignedEmployees = new List<Employee>();
        var userClinics = new Dictionary<long, List<UserClinic>>();

        foreach (var employee in employees)
        {
            var users = Userods.GetUsersByEmployeeNum(employee.EmployeeNum);
            if (users.Count == 0)
            {
                unassignedEmployees.Add(employee);
                continue;
            }

            foreach (var user in users)
            {
                // At this point we know there is at least one Userod associated to this employee.
                if (user.ClinicNum == 0)
                {
                    // User's default clinic is HQ
                    unassignedEmployees.Add(employee);
                    continue;
                }

                if (!userClinics.ContainsKey(user.UserNum)) //User is restricted to a clinic(s).  Compare to clinicNum
                {
                    userClinics[user.UserNum] = UserClinics.GetForUser(user.UserNum);
                }

                if (userClinics[user.UserNum].Count == 0)
                {
                    unassignedEmployees.Add(employee);
                    employeesWithClinic.Add(employee);
                }
                else if (userClinics[user.UserNum].Exists(x => x.ClinicNum == clinicNum))
                {
                    employeesWithClinic.Add(employee);
                }
            }
        }

        if (getUnassigned)
        {
            return unassignedEmployees.Union(employeesWithClinic).OrderBy(GetName).ToList();
        }

        return clinicNum == 0
            ? unassignedEmployees
                .GroupBy(x => x.EmployeeNum)
                .Select(x => x.First())
                .ToList()
            : employeesWithClinic
                .GroupBy(x => x.EmployeeNum)
                .Select(x => x.First())
                .ToList();
    }

    public static void UpdateChanged(Employee employee, Employee employeeOld, bool doInvalidate = false)
    {
        if (employee.LName == "" && employee.FName == "")
        {
            throw new ApplicationException("Must include either first name or last name");
        }

        if (EmployeeCrud.Update(employee, employeeOld) && doInvalidate)
        {
            Signalods.SetInvalid(InvalidType.Employees);
        }
    }

    private class EmployeeCache : CacheListAbs<Employee>
    {
        protected override List<Employee> GetCacheFromDb()
        {
            return EmployeeCrud.SelectMany("SELECT * FROM employee ORDER BY IsHidden,FName,LName");
        }

        protected override List<Employee> TableToList(DataTable dataTable)
        {
            return EmployeeCrud.TableToList(dataTable);
        }

        protected override Employee Copy(Employee item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Employee> items)
        {
            return EmployeeCrud.ListToTable(items, "Employee");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override bool IsInListShort(Employee item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly EmployeeCache Cache = new();

    public static Employee GetFirstOrDefault(Func<Employee, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<Employee> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}