using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var computer = new Computer
            {
                ComputerNum = SIn.Long(row["ComputerNum"].ToString()),
                CompName = SIn.String(row["CompName"].ToString()),
                LastHeartBeat = SIn.DateTime(row["LastHeartBeat"].ToString())
            };
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
            table.Rows.Add(SOut.Long(computer.ComputerNum), computer.CompName, SOut.DateTime(computer.LastHeartBeat, false));
        return table;
    }
}