using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ChildParentCrud
{
    public static ChildParent SelectOne(long childParentNum)
    {
        var command = "SELECT * FROM childparent "
                      + "WHERE ChildParentNum = " + SOut.Long(childParentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ChildParent> TableToList(DataTable table)
    {
        var retVal = new List<ChildParent>();
        ChildParent childParent;
        foreach (DataRow row in table.Rows)
        {
            childParent = new ChildParent();
            childParent.ChildParentNum = SIn.Long(row["ChildParentNum"].ToString());
            childParent.FName = SIn.String(row["FName"].ToString());
            childParent.LName = SIn.String(row["LName"].ToString());
            childParent.Notes = SIn.String(row["Notes"].ToString());
            childParent.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            childParent.BadgeId = SIn.String(row["BadgeId"].ToString());
            retVal.Add(childParent);
        }

        return retVal;
    }

    public static long Insert(ChildParent childParent)
    {
        var command = "INSERT INTO childparent (";

        command += "FName,LName,Notes,IsHidden,BadgeId) VALUES(";

        command +=
            "'" + SOut.String(childParent.FName) + "',"
            + "'" + SOut.String(childParent.LName) + "',"
            + "'" + SOut.String(childParent.Notes) + "',"
            + SOut.Bool(childParent.IsHidden) + ","
            + "'" + SOut.String(childParent.BadgeId) + "')";
        {
            childParent.ChildParentNum = Db.NonQ(command, true, "ChildParentNum", "childParent");
        }
        return childParent.ChildParentNum;
    }

    public static void Update(ChildParent childParent)
    {
        var command = "UPDATE childparent SET "
                      + "FName         = '" + SOut.String(childParent.FName) + "', "
                      + "LName         = '" + SOut.String(childParent.LName) + "', "
                      + "Notes         = '" + SOut.String(childParent.Notes) + "', "
                      + "IsHidden      =  " + SOut.Bool(childParent.IsHidden) + ", "
                      + "BadgeId       = '" + SOut.String(childParent.BadgeId) + "' "
                      + "WHERE ChildParentNum = " + SOut.Long(childParent.ChildParentNum);
        Db.NonQ(command);
    }
}