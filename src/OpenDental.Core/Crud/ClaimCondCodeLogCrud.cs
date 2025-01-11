using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimCondCodeLogCrud
{
    public static ClaimCondCodeLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ClaimCondCodeLog> TableToList(DataTable table)
    {
        var retVal = new List<ClaimCondCodeLog>();
        ClaimCondCodeLog claimCondCodeLog;
        foreach (DataRow row in table.Rows)
        {
            claimCondCodeLog = new ClaimCondCodeLog();
            claimCondCodeLog.ClaimCondCodeLogNum = SIn.Long(row["ClaimCondCodeLogNum"].ToString());
            claimCondCodeLog.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            claimCondCodeLog.Code0 = SIn.String(row["Code0"].ToString());
            claimCondCodeLog.Code1 = SIn.String(row["Code1"].ToString());
            claimCondCodeLog.Code2 = SIn.String(row["Code2"].ToString());
            claimCondCodeLog.Code3 = SIn.String(row["Code3"].ToString());
            claimCondCodeLog.Code4 = SIn.String(row["Code4"].ToString());
            claimCondCodeLog.Code5 = SIn.String(row["Code5"].ToString());
            claimCondCodeLog.Code6 = SIn.String(row["Code6"].ToString());
            claimCondCodeLog.Code7 = SIn.String(row["Code7"].ToString());
            claimCondCodeLog.Code8 = SIn.String(row["Code8"].ToString());
            claimCondCodeLog.Code9 = SIn.String(row["Code9"].ToString());
            claimCondCodeLog.Code10 = SIn.String(row["Code10"].ToString());
            retVal.Add(claimCondCodeLog);
        }

        return retVal;
    }

    public static long Insert(ClaimCondCodeLog claimCondCodeLog)
    {
        var command = "INSERT INTO claimcondcodelog (";

        command += "ClaimNum,Code0,Code1,Code2,Code3,Code4,Code5,Code6,Code7,Code8,Code9,Code10) VALUES(";

        command +=
            SOut.Long(claimCondCodeLog.ClaimNum) + ","
                                                 + "'" + SOut.String(claimCondCodeLog.Code0) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code1) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code2) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code3) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code4) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code5) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code6) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code7) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code8) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code9) + "',"
                                                 + "'" + SOut.String(claimCondCodeLog.Code10) + "')";
        {
            claimCondCodeLog.ClaimCondCodeLogNum = Db.NonQ(command, true, "ClaimCondCodeLogNum", "claimCondCodeLog");
        }
        return claimCondCodeLog.ClaimCondCodeLogNum;
    }

    public static void Update(ClaimCondCodeLog claimCondCodeLog)
    {
        var command = "UPDATE claimcondcodelog SET "
                      + "ClaimNum           =  " + SOut.Long(claimCondCodeLog.ClaimNum) + ", "
                      + "Code0              = '" + SOut.String(claimCondCodeLog.Code0) + "', "
                      + "Code1              = '" + SOut.String(claimCondCodeLog.Code1) + "', "
                      + "Code2              = '" + SOut.String(claimCondCodeLog.Code2) + "', "
                      + "Code3              = '" + SOut.String(claimCondCodeLog.Code3) + "', "
                      + "Code4              = '" + SOut.String(claimCondCodeLog.Code4) + "', "
                      + "Code5              = '" + SOut.String(claimCondCodeLog.Code5) + "', "
                      + "Code6              = '" + SOut.String(claimCondCodeLog.Code6) + "', "
                      + "Code7              = '" + SOut.String(claimCondCodeLog.Code7) + "', "
                      + "Code8              = '" + SOut.String(claimCondCodeLog.Code8) + "', "
                      + "Code9              = '" + SOut.String(claimCondCodeLog.Code9) + "', "
                      + "Code10             = '" + SOut.String(claimCondCodeLog.Code10) + "' "
                      + "WHERE ClaimCondCodeLogNum = " + SOut.Long(claimCondCodeLog.ClaimCondCodeLogNum);
        Db.NonQ(command);
    }
}