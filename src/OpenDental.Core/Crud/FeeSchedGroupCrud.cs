using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class FeeSchedGroupCrud
{
    public static FeeSchedGroup SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FeeSchedGroup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FeeSchedGroup> TableToList(DataTable table)
    {
        var retVal = new List<FeeSchedGroup>();
        foreach (DataRow row in table.Rows)
        {
            var feeSchedGroup = new FeeSchedGroup
            {
                FeeSchedGroupNum = SIn.Long(row["FeeSchedGroupNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                FeeSchedNum = SIn.Long(row["FeeSchedNum"].ToString()),
                ClinicNums = SIn.String(row["ClinicNums"].ToString())
            };
            retVal.Add(feeSchedGroup);
        }

        return retVal;
    }

    public static void Insert(FeeSchedGroup feeSchedGroup)
    {
        var command = "INSERT INTO feeschedgroup (";

        command += "Description,FeeSchedNum,ClinicNums) VALUES(";

        command +=
            "'" + SOut.String(feeSchedGroup.Description) + "',"
            + SOut.Long(feeSchedGroup.FeeSchedNum) + ","
            + "'" + SOut.String(feeSchedGroup.ClinicNums) + "')";
        {
            feeSchedGroup.FeeSchedGroupNum = Db.NonQ(command, true, "FeeSchedGroupNum", "feeSchedGroup");
        }
    }

    public static void Update(FeeSchedGroup feeSchedGroup)
    {
        var command = "UPDATE feeschedgroup SET "
                      + "Description     = '" + SOut.String(feeSchedGroup.Description) + "', "
                      + "FeeSchedNum     =  " + SOut.Long(feeSchedGroup.FeeSchedNum) + ", "
                      + "ClinicNums      = '" + SOut.String(feeSchedGroup.ClinicNums) + "' "
                      + "WHERE FeeSchedGroupNum = " + SOut.Long(feeSchedGroup.FeeSchedGroupNum);
        Db.NonQ(command);
    }

    public static void Delete(long feeSchedGroupNum)
    {
        var command = "DELETE FROM feeschedgroup "
                      + "WHERE FeeSchedGroupNum = " + SOut.Long(feeSchedGroupNum);
        Db.NonQ(command);
    }
}