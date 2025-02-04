using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PayTerminalCrud
{
    public static List<PayTerminal> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayTerminal> TableToList(DataTable table)
    {
        var retVal = new List<PayTerminal>();
        foreach (DataRow row in table.Rows)
        {
            var payTerminal = new PayTerminal
            {
                PayTerminalNum = SIn.Long(row["PayTerminalNum"].ToString()),
                Name = SIn.String(row["Name"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                TerminalID = SIn.String(row["TerminalID"].ToString())
            };
            retVal.Add(payTerminal);
        }

        return retVal;
    }

    public static void Insert(PayTerminal payTerminal)
    {
        var command = "INSERT INTO payterminal (";

        command += "Name,ClinicNum,TerminalID) VALUES(";

        command +=
            "'" + SOut.String(payTerminal.Name) + "',"
            + SOut.Long(payTerminal.ClinicNum) + ","
            + "'" + SOut.String(payTerminal.TerminalID) + "')";
        {
            payTerminal.PayTerminalNum = Db.NonQ(command, true, "PayTerminalNum", "payTerminal");
        }
    }

    public static void Update(PayTerminal payTerminal)
    {
        var command = "UPDATE payterminal SET "
                      + "Name          = '" + SOut.String(payTerminal.Name) + "', "
                      + "ClinicNum     =  " + SOut.Long(payTerminal.ClinicNum) + ", "
                      + "TerminalID    = '" + SOut.String(payTerminal.TerminalID) + "' "
                      + "WHERE PayTerminalNum = " + SOut.Long(payTerminal.PayTerminalNum);
        Db.NonQ(command);
    }

    public static void Delete(long payTerminalNum)
    {
        var command = "DELETE FROM payterminal "
                      + "WHERE PayTerminalNum = " + SOut.Long(payTerminalNum);
        Db.NonQ(command);
    }
}