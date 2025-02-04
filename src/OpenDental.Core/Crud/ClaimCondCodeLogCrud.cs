using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var claimCondCodeLog = new ClaimCondCodeLog
            {
                ClaimCondCodeLogNum = SIn.Long(row["ClaimCondCodeLogNum"].ToString()),
                ClaimNum = SIn.Long(row["ClaimNum"].ToString()),
                Code0 = SIn.String(row["Code0"].ToString()),
                Code1 = SIn.String(row["Code1"].ToString()),
                Code2 = SIn.String(row["Code2"].ToString()),
                Code3 = SIn.String(row["Code3"].ToString()),
                Code4 = SIn.String(row["Code4"].ToString()),
                Code5 = SIn.String(row["Code5"].ToString()),
                Code6 = SIn.String(row["Code6"].ToString()),
                Code7 = SIn.String(row["Code7"].ToString()),
                Code8 = SIn.String(row["Code8"].ToString()),
                Code9 = SIn.String(row["Code9"].ToString()),
                Code10 = SIn.String(row["Code10"].ToString())
            };
            retVal.Add(claimCondCodeLog);
        }

        return retVal;
    }

    public static void Insert(ClaimCondCodeLog claimCondCodeLog)
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