using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ChildParentLinkCrud
{
    public static List<ChildParentLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ChildParentLink> TableToList(DataTable table)
    {
        var retVal = new List<ChildParentLink>();
        ChildParentLink childParentLink;
        foreach (DataRow row in table.Rows)
        {
            childParentLink = new ChildParentLink();
            childParentLink.ChildParentLinkNum = SIn.Long(row["ChildParentLinkNum"].ToString());
            childParentLink.ChildNum = SIn.Long(row["ChildNum"].ToString());
            childParentLink.ChildParentNum = SIn.Long(row["ChildParentNum"].ToString());
            childParentLink.Relationship = SIn.String(row["Relationship"].ToString());
            retVal.Add(childParentLink);
        }

        return retVal;
    }

    public static long Insert(ChildParentLink childParentLink)
    {
        var command = "INSERT INTO childparentlink (";

        command += "ChildNum,ChildParentNum,Relationship) VALUES(";

        command +=
            SOut.Long(childParentLink.ChildNum) + ","
                                                + SOut.Long(childParentLink.ChildParentNum) + ","
                                                + "'" + SOut.String(childParentLink.Relationship) + "')";
        {
            childParentLink.ChildParentLinkNum = Db.NonQ(command, true, "ChildParentLinkNum", "childParentLink");
        }
        return childParentLink.ChildParentLinkNum;
    }

    public static void Update(ChildParentLink childParentLink)
    {
        var command = "UPDATE childparentlink SET "
                      + "ChildNum          =  " + SOut.Long(childParentLink.ChildNum) + ", "
                      + "ChildParentNum    =  " + SOut.Long(childParentLink.ChildParentNum) + ", "
                      + "Relationship      = '" + SOut.String(childParentLink.Relationship) + "' "
                      + "WHERE ChildParentLinkNum = " + SOut.Long(childParentLink.ChildParentLinkNum);
        Db.NonQ(command);
    }

    public static void Delete(long childParentLinkNum)
    {
        var command = "DELETE FROM childparentlink "
                      + "WHERE ChildParentLinkNum = " + SOut.Long(childParentLinkNum);
        Db.NonQ(command);
    }
}