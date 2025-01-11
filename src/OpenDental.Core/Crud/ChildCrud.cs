using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ChildCrud
{
    public static Child SelectOne(long childNum)
    {
        var command = "SELECT * FROM child "
                      + "WHERE ChildNum = " + SOut.Long(childNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Child> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Child> TableToList(DataTable table)
    {
        var retVal = new List<Child>();
        Child child;
        foreach (DataRow row in table.Rows)
        {
            child = new Child();
            child.ChildNum = SIn.Long(row["ChildNum"].ToString());
            child.ChildRoomNumPrimary = SIn.Long(row["ChildRoomNumPrimary"].ToString());
            child.FName = SIn.String(row["FName"].ToString());
            child.LName = SIn.String(row["LName"].ToString());
            child.BirthDate = SIn.Date(row["BirthDate"].ToString());
            child.Notes = SIn.String(row["Notes"].ToString());
            child.BadgeId = SIn.String(row["BadgeId"].ToString());
            child.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            retVal.Add(child);
        }

        return retVal;
    }

    public static long Insert(Child child)
    {
        var command = "INSERT INTO child (";

        command += "ChildRoomNumPrimary,FName,LName,BirthDate,Notes,BadgeId,IsHidden) VALUES(";

        command +=
            SOut.Long(child.ChildRoomNumPrimary) + ","
                                                 + "'" + SOut.String(child.FName) + "',"
                                                 + "'" + SOut.String(child.LName) + "',"
                                                 + SOut.Date(child.BirthDate) + ","
                                                 + "'" + SOut.String(child.Notes) + "',"
                                                 + "'" + SOut.String(child.BadgeId) + "',"
                                                 + SOut.Bool(child.IsHidden) + ")";
        {
            child.ChildNum = Db.NonQ(command, true, "ChildNum", "child");
        }
        return child.ChildNum;
    }

    public static void Update(Child child)
    {
        var command = "UPDATE child SET "
                      + "ChildRoomNumPrimary=  " + SOut.Long(child.ChildRoomNumPrimary) + ", "
                      + "FName              = '" + SOut.String(child.FName) + "', "
                      + "LName              = '" + SOut.String(child.LName) + "', "
                      + "BirthDate          =  " + SOut.Date(child.BirthDate) + ", "
                      + "Notes              = '" + SOut.String(child.Notes) + "', "
                      + "BadgeId            = '" + SOut.String(child.BadgeId) + "', "
                      + "IsHidden           =  " + SOut.Bool(child.IsHidden) + " "
                      + "WHERE ChildNum = " + SOut.Long(child.ChildNum);
        Db.NonQ(command);
    }

    public static void Delete(long childNum)
    {
        var command = "DELETE FROM child "
                      + "WHERE ChildNum = " + SOut.Long(childNum);
        Db.NonQ(command);
    }
}