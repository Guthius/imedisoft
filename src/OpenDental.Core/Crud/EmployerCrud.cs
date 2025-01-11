#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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
        Employer employer;
        foreach (DataRow row in table.Rows)
        {
            employer = new Employer();
            employer.EmployerNum = SIn.Long(row["EmployerNum"].ToString());
            employer.EmpName = SIn.String(row["EmpName"].ToString());
            employer.Address = SIn.String(row["Address"].ToString());
            employer.Address2 = SIn.String(row["Address2"].ToString());
            employer.City = SIn.String(row["City"].ToString());
            employer.State = SIn.String(row["State"].ToString());
            employer.Zip = SIn.String(row["Zip"].ToString());
            employer.Phone = SIn.String(row["Phone"].ToString());
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

    public static long Insert(Employer employer)
    {
        return Insert(employer, false);
    }

    public static long Insert(Employer employer, bool useExistingPK)
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
        return employer.EmployerNum;
    }

    public static long InsertNoCache(Employer employer)
    {
        return InsertNoCache(employer, false);
    }

    public static long InsertNoCache(Employer employer, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO employer (";
        if (isRandomKeys || useExistingPK) command += "EmployerNum,";
        command += "EmpName,Address,Address2,City,State,Zip,Phone) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(employer.EmployerNum) + ",";
        command +=
            "'" + SOut.String(employer.EmpName) + "',"
            + "'" + SOut.String(employer.Address) + "',"
            + "'" + SOut.String(employer.Address2) + "',"
            + "'" + SOut.String(employer.City) + "',"
            + "'" + SOut.String(employer.State) + "',"
            + "'" + SOut.String(employer.Zip) + "',"
            + "'" + SOut.String(employer.Phone) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            employer.EmployerNum = Db.NonQ(command, true, "EmployerNum", "employer");
        return employer.EmployerNum;
    }

    public static void Update(Employer employer)
    {
        var command = "UPDATE employer SET "
                      + "EmpName    = '" + SOut.String(employer.EmpName) + "', "
                      + "Address    = '" + SOut.String(employer.Address) + "', "
                      + "Address2   = '" + SOut.String(employer.Address2) + "', "
                      + "City       = '" + SOut.String(employer.City) + "', "
                      + "State      = '" + SOut.String(employer.State) + "', "
                      + "Zip        = '" + SOut.String(employer.Zip) + "', "
                      + "Phone      = '" + SOut.String(employer.Phone) + "' "
                      + "WHERE EmployerNum = " + SOut.Long(employer.EmployerNum);
        Db.NonQ(command);
    }

    public static bool Update(Employer employer, Employer oldEmployer)
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

        if (command == "") return false;
        command = "UPDATE employer SET " + command
                                         + " WHERE EmployerNum = " + SOut.Long(employer.EmployerNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Employer employer, Employer oldEmployer)
    {
        if (employer.EmpName != oldEmployer.EmpName) return true;
        if (employer.Address != oldEmployer.Address) return true;
        if (employer.Address2 != oldEmployer.Address2) return true;
        if (employer.City != oldEmployer.City) return true;
        if (employer.State != oldEmployer.State) return true;
        if (employer.Zip != oldEmployer.Zip) return true;
        if (employer.Phone != oldEmployer.Phone) return true;
        return false;
    }

    public static void Delete(long employerNum)
    {
        var command = "DELETE FROM employer "
                      + "WHERE EmployerNum = " + SOut.Long(employerNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmployerNums)
    {
        if (listEmployerNums == null || listEmployerNums.Count == 0) return;
        var command = "DELETE FROM employer "
                      + "WHERE EmployerNum IN(" + string.Join(",", listEmployerNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}