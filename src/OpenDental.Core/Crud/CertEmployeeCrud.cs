using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CertEmployeeCrud
{
    public static CertEmployee SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<CertEmployee> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CertEmployee> TableToList(DataTable table)
    {
        var retVal = new List<CertEmployee>();
        CertEmployee certEmployee;
        foreach (DataRow row in table.Rows)
        {
            certEmployee = new CertEmployee();
            certEmployee.CertEmployeeNum = SIn.Long(row["CertEmployeeNum"].ToString());
            certEmployee.CertNum = SIn.Long(row["CertNum"].ToString());
            certEmployee.EmployeeNum = SIn.Long(row["EmployeeNum"].ToString());
            certEmployee.DateCompleted = SIn.Date(row["DateCompleted"].ToString());
            certEmployee.Note = SIn.String(row["Note"].ToString());
            certEmployee.UserNum = SIn.Long(row["UserNum"].ToString());
            retVal.Add(certEmployee);
        }

        return retVal;
    }

    public static long Insert(CertEmployee certEmployee)
    {
        var command = "INSERT INTO certemployee (";

        command += "CertNum,EmployeeNum,DateCompleted,Note,UserNum) VALUES(";

        command +=
            SOut.Long(certEmployee.CertNum) + ","
                                            + SOut.Long(certEmployee.EmployeeNum) + ","
                                            + SOut.Date(certEmployee.DateCompleted) + ","
                                            + "'" + SOut.String(certEmployee.Note) + "',"
                                            + SOut.Long(certEmployee.UserNum) + ")";
        {
            certEmployee.CertEmployeeNum = Db.NonQ(command, true, "CertEmployeeNum", "certEmployee");
        }
        return certEmployee.CertEmployeeNum;
    }

    public static void Update(CertEmployee certEmployee)
    {
        var command = "UPDATE certemployee SET "
                      + "CertNum        =  " + SOut.Long(certEmployee.CertNum) + ", "
                      + "EmployeeNum    =  " + SOut.Long(certEmployee.EmployeeNum) + ", "
                      + "DateCompleted  =  " + SOut.Date(certEmployee.DateCompleted) + ", "
                      + "Note           = '" + SOut.String(certEmployee.Note) + "', "
                      + "UserNum        =  " + SOut.Long(certEmployee.UserNum) + " "
                      + "WHERE CertEmployeeNum = " + SOut.Long(certEmployee.CertEmployeeNum);
        Db.NonQ(command);
    }

    public static void Delete(long certEmployeeNum)
    {
        var command = "DELETE FROM certemployee "
                      + "WHERE CertEmployeeNum = " + SOut.Long(certEmployeeNum);
        Db.NonQ(command);
    }
}