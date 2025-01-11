using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        ClaimAttach claimAttach;
        foreach (DataRow row in table.Rows)
        {
            claimAttach = new ClaimAttach();
            claimAttach.ClaimAttachNum = SIn.Long(row["ClaimAttachNum"].ToString());
            claimAttach.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            claimAttach.DisplayedFileName = SIn.String(row["DisplayedFileName"].ToString());
            claimAttach.ActualFileName = SIn.String(row["ActualFileName"].ToString());
            claimAttach.ImageReferenceId = SIn.Int(row["ImageReferenceId"].ToString());
            retVal.Add(claimAttach);
        }

        return retVal;
    }

    public static long Insert(ClaimAttach claimAttach)
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
        return claimAttach.ClaimAttachNum;
    }
}