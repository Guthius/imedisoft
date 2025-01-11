#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmployeeCrud
{
    public static Employee SelectOne(long employeeNum)
    {
        var command = "SELECT * FROM employee "
                      + "WHERE EmployeeNum = " + SOut.Long(employeeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Employee SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Employee> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Employee> TableToList(DataTable table)
    {
        var retVal = new List<Employee>();
        Employee employee;
        foreach (DataRow row in table.Rows)
        {
            employee = new Employee();
            employee.EmployeeNum = SIn.Long(row["EmployeeNum"].ToString());
            employee.LName = SIn.String(row["LName"].ToString());
            employee.FName = SIn.String(row["FName"].ToString());
            employee.MiddleI = SIn.String(row["MiddleI"].ToString());
            employee.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            employee.ClockStatus = SIn.String(row["ClockStatus"].ToString());
            employee.PhoneExt = SIn.Int(row["PhoneExt"].ToString());
            employee.PayrollID = SIn.String(row["PayrollID"].ToString());
            employee.WirelessPhone = SIn.String(row["WirelessPhone"].ToString());
            employee.EmailWork = SIn.String(row["EmailWork"].ToString());
            employee.EmailPersonal = SIn.String(row["EmailPersonal"].ToString());
            employee.IsFurloughed = SIn.Bool(row["IsFurloughed"].ToString());
            employee.IsWorkingHome = SIn.Bool(row["IsWorkingHome"].ToString());
            employee.ReportsTo = SIn.Long(row["ReportsTo"].ToString());
            retVal.Add(employee);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Employee> listEmployees, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Employee";
        var table = new DataTable(tableName);
        table.Columns.Add("EmployeeNum");
        table.Columns.Add("LName");
        table.Columns.Add("FName");
        table.Columns.Add("MiddleI");
        table.Columns.Add("IsHidden");
        table.Columns.Add("ClockStatus");
        table.Columns.Add("PhoneExt");
        table.Columns.Add("PayrollID");
        table.Columns.Add("WirelessPhone");
        table.Columns.Add("EmailWork");
        table.Columns.Add("EmailPersonal");
        table.Columns.Add("IsFurloughed");
        table.Columns.Add("IsWorkingHome");
        table.Columns.Add("ReportsTo");
        foreach (var employee in listEmployees)
            table.Rows.Add(SOut.Long(employee.EmployeeNum), employee.LName, employee.FName, employee.MiddleI, SOut.Bool(employee.IsHidden), employee.ClockStatus, SOut.Int(employee.PhoneExt), employee.PayrollID, employee.WirelessPhone, employee.EmailWork, employee.EmailPersonal, SOut.Bool(employee.IsFurloughed), SOut.Bool(employee.IsWorkingHome), SOut.Long(employee.ReportsTo));
        return table;
    }

    public static long Insert(Employee employee)
    {
        return Insert(employee, false);
    }

    public static long Insert(Employee employee, bool useExistingPK)
    {
        var command = "INSERT INTO employee (";

        command += "LName,FName,MiddleI,IsHidden,ClockStatus,PhoneExt,PayrollID,WirelessPhone,EmailWork,EmailPersonal,IsFurloughed,IsWorkingHome,ReportsTo) VALUES(";

        command +=
            "'" + SOut.String(employee.LName) + "',"
            + "'" + SOut.String(employee.FName) + "',"
            + "'" + SOut.String(employee.MiddleI) + "',"
            + SOut.Bool(employee.IsHidden) + ","
            + "'" + SOut.String(employee.ClockStatus) + "',"
            + SOut.Int(employee.PhoneExt) + ","
            + "'" + SOut.String(employee.PayrollID) + "',"
            + "'" + SOut.String(employee.WirelessPhone) + "',"
            + "'" + SOut.String(employee.EmailWork) + "',"
            + "'" + SOut.String(employee.EmailPersonal) + "',"
            + SOut.Bool(employee.IsFurloughed) + ","
            + SOut.Bool(employee.IsWorkingHome) + ","
            + SOut.Long(employee.ReportsTo) + ")";
        {
            employee.EmployeeNum = Db.NonQ(command, true, "EmployeeNum", "employee");
        }
        return employee.EmployeeNum;
    }

    public static long InsertNoCache(Employee employee)
    {
        return InsertNoCache(employee, false);
    }

    public static long InsertNoCache(Employee employee, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO employee (";
        if (isRandomKeys || useExistingPK) command += "EmployeeNum,";
        command += "LName,FName,MiddleI,IsHidden,ClockStatus,PhoneExt,PayrollID,WirelessPhone,EmailWork,EmailPersonal,IsFurloughed,IsWorkingHome,ReportsTo) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(employee.EmployeeNum) + ",";
        command +=
            "'" + SOut.String(employee.LName) + "',"
            + "'" + SOut.String(employee.FName) + "',"
            + "'" + SOut.String(employee.MiddleI) + "',"
            + SOut.Bool(employee.IsHidden) + ","
            + "'" + SOut.String(employee.ClockStatus) + "',"
            + SOut.Int(employee.PhoneExt) + ","
            + "'" + SOut.String(employee.PayrollID) + "',"
            + "'" + SOut.String(employee.WirelessPhone) + "',"
            + "'" + SOut.String(employee.EmailWork) + "',"
            + "'" + SOut.String(employee.EmailPersonal) + "',"
            + SOut.Bool(employee.IsFurloughed) + ","
            + SOut.Bool(employee.IsWorkingHome) + ","
            + SOut.Long(employee.ReportsTo) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            employee.EmployeeNum = Db.NonQ(command, true, "EmployeeNum", "employee");
        return employee.EmployeeNum;
    }

    public static void Update(Employee employee)
    {
        var command = "UPDATE employee SET "
                      + "LName        = '" + SOut.String(employee.LName) + "', "
                      + "FName        = '" + SOut.String(employee.FName) + "', "
                      + "MiddleI      = '" + SOut.String(employee.MiddleI) + "', "
                      + "IsHidden     =  " + SOut.Bool(employee.IsHidden) + ", "
                      + "ClockStatus  = '" + SOut.String(employee.ClockStatus) + "', "
                      + "PhoneExt     =  " + SOut.Int(employee.PhoneExt) + ", "
                      + "PayrollID    = '" + SOut.String(employee.PayrollID) + "', "
                      + "WirelessPhone= '" + SOut.String(employee.WirelessPhone) + "', "
                      + "EmailWork    = '" + SOut.String(employee.EmailWork) + "', "
                      + "EmailPersonal= '" + SOut.String(employee.EmailPersonal) + "', "
                      + "IsFurloughed =  " + SOut.Bool(employee.IsFurloughed) + ", "
                      + "IsWorkingHome=  " + SOut.Bool(employee.IsWorkingHome) + ", "
                      + "ReportsTo    =  " + SOut.Long(employee.ReportsTo) + " "
                      + "WHERE EmployeeNum = " + SOut.Long(employee.EmployeeNum);
        Db.NonQ(command);
    }

    public static bool Update(Employee employee, Employee oldEmployee)
    {
        var command = "";
        if (employee.LName != oldEmployee.LName)
        {
            if (command != "") command += ",";
            command += "LName = '" + SOut.String(employee.LName) + "'";
        }

        if (employee.FName != oldEmployee.FName)
        {
            if (command != "") command += ",";
            command += "FName = '" + SOut.String(employee.FName) + "'";
        }

        if (employee.MiddleI != oldEmployee.MiddleI)
        {
            if (command != "") command += ",";
            command += "MiddleI = '" + SOut.String(employee.MiddleI) + "'";
        }

        if (employee.IsHidden != oldEmployee.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(employee.IsHidden) + "";
        }

        if (employee.ClockStatus != oldEmployee.ClockStatus)
        {
            if (command != "") command += ",";
            command += "ClockStatus = '" + SOut.String(employee.ClockStatus) + "'";
        }

        if (employee.PhoneExt != oldEmployee.PhoneExt)
        {
            if (command != "") command += ",";
            command += "PhoneExt = " + SOut.Int(employee.PhoneExt) + "";
        }

        if (employee.PayrollID != oldEmployee.PayrollID)
        {
            if (command != "") command += ",";
            command += "PayrollID = '" + SOut.String(employee.PayrollID) + "'";
        }

        if (employee.WirelessPhone != oldEmployee.WirelessPhone)
        {
            if (command != "") command += ",";
            command += "WirelessPhone = '" + SOut.String(employee.WirelessPhone) + "'";
        }

        if (employee.EmailWork != oldEmployee.EmailWork)
        {
            if (command != "") command += ",";
            command += "EmailWork = '" + SOut.String(employee.EmailWork) + "'";
        }

        if (employee.EmailPersonal != oldEmployee.EmailPersonal)
        {
            if (command != "") command += ",";
            command += "EmailPersonal = '" + SOut.String(employee.EmailPersonal) + "'";
        }

        if (employee.IsFurloughed != oldEmployee.IsFurloughed)
        {
            if (command != "") command += ",";
            command += "IsFurloughed = " + SOut.Bool(employee.IsFurloughed) + "";
        }

        if (employee.IsWorkingHome != oldEmployee.IsWorkingHome)
        {
            if (command != "") command += ",";
            command += "IsWorkingHome = " + SOut.Bool(employee.IsWorkingHome) + "";
        }

        if (employee.ReportsTo != oldEmployee.ReportsTo)
        {
            if (command != "") command += ",";
            command += "ReportsTo = " + SOut.Long(employee.ReportsTo) + "";
        }

        if (command == "") return false;
        command = "UPDATE employee SET " + command
                                         + " WHERE EmployeeNum = " + SOut.Long(employee.EmployeeNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Employee employee, Employee oldEmployee)
    {
        if (employee.LName != oldEmployee.LName) return true;
        if (employee.FName != oldEmployee.FName) return true;
        if (employee.MiddleI != oldEmployee.MiddleI) return true;
        if (employee.IsHidden != oldEmployee.IsHidden) return true;
        if (employee.ClockStatus != oldEmployee.ClockStatus) return true;
        if (employee.PhoneExt != oldEmployee.PhoneExt) return true;
        if (employee.PayrollID != oldEmployee.PayrollID) return true;
        if (employee.WirelessPhone != oldEmployee.WirelessPhone) return true;
        if (employee.EmailWork != oldEmployee.EmailWork) return true;
        if (employee.EmailPersonal != oldEmployee.EmailPersonal) return true;
        if (employee.IsFurloughed != oldEmployee.IsFurloughed) return true;
        if (employee.IsWorkingHome != oldEmployee.IsWorkingHome) return true;
        if (employee.ReportsTo != oldEmployee.ReportsTo) return true;
        return false;
    }

    public static void Delete(long employeeNum)
    {
        var command = "DELETE FROM employee "
                      + "WHERE EmployeeNum = " + SOut.Long(employeeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmployeeNums)
    {
        if (listEmployeeNums == null || listEmployeeNums.Count == 0) return;
        var command = "DELETE FROM employee "
                      + "WHERE EmployeeNum IN(" + string.Join(",", listEmployeeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}