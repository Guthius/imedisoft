using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PatRestrictionCrud
{
    public static List<PatRestriction> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatRestriction> TableToList(DataTable table)
    {
        var retVal = new List<PatRestriction>();
        foreach (DataRow row in table.Rows)
        {
            var patRestriction = new PatRestriction
            {
                PatRestrictionNum = SIn.Long(row["PatRestrictionNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                PatRestrictType = (PatRestrict) SIn.Int(row["PatRestrictType"].ToString())
            };
            retVal.Add(patRestriction);
        }

        return retVal;
    }

    public static void Insert(PatRestriction patRestriction)
    {
        var command = "INSERT INTO patrestriction (";

        command += "PatNum,PatRestrictType) VALUES(";

        command +=
            SOut.Long(patRestriction.PatNum) + ","
                                             + SOut.Int((int) patRestriction.PatRestrictType) + ")";
        {
            patRestriction.PatRestrictionNum = Db.NonQ(command, true, "PatRestrictionNum", "patRestriction");
        }
    }
}