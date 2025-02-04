using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ClaimAttachCrud
{
    public static List<ClaimAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimAttach> TableToList(DataTable table)
    {
        var retVal = new List<ClaimAttach>();
        foreach (DataRow row in table.Rows)
        {
            var claimAttach = new ClaimAttach
            {
                ClaimAttachNum = SIn.Long(row["ClaimAttachNum"].ToString()),
                ClaimNum = SIn.Long(row["ClaimNum"].ToString()),
                DisplayedFileName = SIn.String(row["DisplayedFileName"].ToString()),
                ActualFileName = SIn.String(row["ActualFileName"].ToString()),
                ImageReferenceId = SIn.Int(row["ImageReferenceId"].ToString())
            };
            retVal.Add(claimAttach);
        }

        return retVal;
    }

    public static void Insert(ClaimAttach claimAttach)
    {
        var command = "INSERT INTO claimattach (";

        command += "ClaimNum,DisplayedFileName,ActualFileName,ImageReferenceId) VALUES(";

        command +=
            SOut.Long(claimAttach.ClaimNum) + ","
                                            + "'" + SOut.String(claimAttach.DisplayedFileName) + "',"
                                            + "'" + SOut.String(claimAttach.ActualFileName) + "',"
                                            + SOut.Int(claimAttach.ImageReferenceId) + ")";
        {
            claimAttach.ClaimAttachNum = Db.NonQ(command, true, "ClaimAttachNum", "claimAttach");
        }
    }
}