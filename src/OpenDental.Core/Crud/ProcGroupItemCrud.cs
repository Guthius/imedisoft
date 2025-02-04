using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcGroupItemCrud
{
    public static List<ProcGroupItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcGroupItem> TableToList(DataTable table)
    {
        var retVal = new List<ProcGroupItem>();
        foreach (DataRow row in table.Rows)
        {
            var procGroupItem = new ProcGroupItem
            {
                ProcGroupItemNum = SIn.Long(row["ProcGroupItemNum"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                GroupNum = SIn.Long(row["GroupNum"].ToString())
            };
            retVal.Add(procGroupItem);
        }

        return retVal;
    }

    public static void Insert(ProcGroupItem procGroupItem)
    {
        var command = "INSERT INTO procgroupitem (";

        command += "ProcNum,GroupNum) VALUES(";

        command +=
            SOut.Long(procGroupItem.ProcNum) + ","
                                             + SOut.Long(procGroupItem.GroupNum) + ")";
        {
            procGroupItem.ProcGroupItemNum = Db.NonQ(command, true, "ProcGroupItemNum", "procGroupItem");
        }
    }
}