using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<Employee> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Employee> TableToList(DataTable table)
    {
        var retVal = new List<Employee>();
        foreach (DataRow row in table.Rows)
        {
            var employee = new Employee
            {
                EmployeeNum = SIn.Long(row["EmployeeNum"].ToString()),
                LName = SIn.String(row["LName"].ToString()),
                FName = SIn.String(row["FName"].ToString()),
                MiddleI = SIn.String(row["MiddleI"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                ClockStatus = SIn.String(row["ClockStatus"].ToString()),
                PhoneExt = SIn.Int(row["PhoneExt"].ToString()),
                PayrollID = SIn.String(row["PayrollID"].ToString()),
                WirelessPhone = SIn.String(row["WirelessPhone"].ToString()),
                EmailWork = SIn.String(row["EmailWork"].ToString()),
                EmailPersonal = SIn.String(row["EmailPersonal"].ToString()),
                IsFurloughed = SIn.Bool(row["IsFurloughed"].ToString()),
                IsWorkingHome = SIn.Bool(row["IsWorkingHome"].ToString()),
                ReportsTo = SIn.Long(row["ReportsTo"].ToString())
            };
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

    public static void Insert(Employee employee)
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
}