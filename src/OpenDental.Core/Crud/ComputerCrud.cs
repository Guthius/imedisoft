using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ComputerCrud
{
    public static List<Computer> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Computer> TableToList(DataTable table)
    {
        var retVal = new List<Computer>();
        Computer computer;
        foreach (DataRow row in table.Rows)
        {
            computer = new Computer();
            computer.ComputerNum = SIn.Long(row["ComputerNum"].ToString());
            computer.CompName = SIn.String(row["CompName"].ToString());
            computer.LastHeartBeat = SIn.DateTime(row["LastHeartBeat"].ToString());
            retVal.Add(computer);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Computer> listComputers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Computer";
        var table = new DataTable(tableName);
        table.Columns.Add("ComputerNum");
        table.Columns.Add("CompName");
        table.Columns.Add("LastHeartBeat");
        foreach (var computer in listComputers)
            table.Rows.Add(SOut.Long(computer.ComputerNum), computer.CompName, SOut.DateT(computer.LastHeartBeat, false));
        return table;
    }

    public static long Insert(Computer computer)
    {
        var command = "INSERT INTO computer (";

        command += "CompName,LastHeartBeat) VALUES(";

        command +=
            "'" + SOut.String(computer.CompName) + "',"
            + SOut.DateT(computer.LastHeartBeat) + ")";
        {
            computer.ComputerNum = Db.NonQ(command, true, "ComputerNum", "computer");
        }
        return computer.ComputerNum;
    }
}