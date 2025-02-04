using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EmployerCrud
{
    public static Employer SelectOne(long employerNum)
    {
        var command = "SELECT * FROM employer "
                      + "WHERE EmployerNum = " + SOut.Long(employerNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Employer SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Employer> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Employer> TableToList(DataTable table)
    {
        var retVal = new List<Employer>();
        foreach (DataRow row in table.Rows)
        {
            var employer = new Employer
            {
                EmployerNum = SIn.Long(row["EmployerNum"].ToString()),
                EmpName = SIn.String(row["EmpName"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                Address2 = SIn.String(row["Address2"].ToString()),
                City = SIn.String(row["City"].ToString()),
                State = SIn.String(row["State"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                Phone = SIn.String(row["Phone"].ToString())
            };
            retVal.Add(employer);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Employer> listEmployers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Employer";
        var table = new DataTable(tableName);
        table.Columns.Add("EmployerNum");
        table.Columns.Add("EmpName");
        table.Columns.Add("Address");
        table.Columns.Add("Address2");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("Zip");
        table.Columns.Add("Phone");
        foreach (var employer in listEmployers)
            table.Rows.Add(SOut.Long(employer.EmployerNum), employer.EmpName, employer.Address, employer.Address2, employer.City, employer.State, employer.Zip, employer.Phone);
        return table;
    }

    public static void Insert(Employer employer)
    {
        var command = "INSERT INTO employer (";

        command += "EmpName,Address,Address2,City,State,Zip,Phone) VALUES(";

        command +=
            "'" + SOut.String(employer.EmpName) + "',"
            + "'" + SOut.String(employer.Address) + "',"
            + "'" + SOut.String(employer.Address2) + "',"
            + "'" + SOut.String(employer.City) + "',"
            + "'" + SOut.String(employer.State) + "',"
            + "'" + SOut.String(employer.Zip) + "',"
            + "'" + SOut.String(employer.Phone) + "')";
        {
            employer.EmployerNum = Db.NonQ(command, true, "EmployerNum", "employer");
        }
    }

    public static void Update(Employer employer, Employer oldEmployer)
    {
        var command = "";
        if (employer.EmpName != oldEmployer.EmpName)
        {
            if (command != "") command += ",";
            command += "EmpName = '" + SOut.String(employer.EmpName) + "'";
        }

        if (employer.Address != oldEmployer.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.String(employer.Address) + "'";
        }

        if (employer.Address2 != oldEmployer.Address2)
        {
            if (command != "") command += ",";
            command += "Address2 = '" + SOut.String(employer.Address2) + "'";
        }

        if (employer.City != oldEmployer.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(employer.City) + "'";
        }

        if (employer.State != oldEmployer.State)
        {
            if (command != "") command += ",";
            command += "State = '" + SOut.String(employer.State) + "'";
        }

        if (employer.Zip != oldEmployer.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(employer.Zip) + "'";
        }

        if (employer.Phone != oldEmployer.Phone)
        {
            if (command != "") command += ",";
            command += "Phone = '" + SOut.String(employer.Phone) + "'";
        }

        if (command == "") return;
        command = "UPDATE employer SET " + command
                                         + " WHERE EmployerNum = " + SOut.Long(employer.EmployerNum);
        Db.NonQ(command);
    }
}