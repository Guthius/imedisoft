#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ERoutingDefLinkCrud
{
    public static ERoutingDefLink SelectOne(long eRoutingDefLinkNum)
    {
        var command = "SELECT * FROM eroutingdeflink "
                      + "WHERE ERoutingDefLinkNum = " + SOut.Long(eRoutingDefLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ERoutingDefLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ERoutingDefLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERoutingDefLink> TableToList(DataTable table)
    {
        var retVal = new List<ERoutingDefLink>();
        ERoutingDefLink eRoutingDefLink;
        foreach (DataRow row in table.Rows)
        {
            eRoutingDefLink = new ERoutingDefLink();
            eRoutingDefLink.ERoutingDefLinkNum = SIn.Long(row["ERoutingDefLinkNum"].ToString());
            eRoutingDefLink.ERoutingDefNum = SIn.Long(row["ERoutingDefNum"].ToString());
            eRoutingDefLink.Fkey = SIn.Long(row["Fkey"].ToString());
            eRoutingDefLink.ERoutingType = (EnumERoutingType) SIn.Int(row["ERoutingType"].ToString());
            retVal.Add(eRoutingDefLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ERoutingDefLink> listERoutingDefLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ERoutingDefLink";
        var table = new DataTable(tableName);
        table.Columns.Add("ERoutingDefLinkNum");
        table.Columns.Add("ERoutingDefNum");
        table.Columns.Add("Fkey");
        table.Columns.Add("ERoutingType");
        foreach (var eRoutingDefLink in listERoutingDefLinks)
            table.Rows.Add(SOut.Long(eRoutingDefLink.ERoutingDefLinkNum), SOut.Long(eRoutingDefLink.ERoutingDefNum), SOut.Long(eRoutingDefLink.Fkey), SOut.Int((int) eRoutingDefLink.ERoutingType));
        return table;
    }

    public static long Insert(ERoutingDefLink eRoutingDefLink)
    {
        return Insert(eRoutingDefLink, false);
    }

    public static long Insert(ERoutingDefLink eRoutingDefLink, bool useExistingPK)
    {
        var command = "INSERT INTO eroutingdeflink (";

        command += "ERoutingDefNum,Fkey,ERoutingType) VALUES(";

        command +=
            SOut.Long(eRoutingDefLink.ERoutingDefNum) + ","
                                                      + SOut.Long(eRoutingDefLink.Fkey) + ","
                                                      + SOut.Int((int) eRoutingDefLink.ERoutingType) + ")";
        {
            eRoutingDefLink.ERoutingDefLinkNum = Db.NonQ(command, true, "ERoutingDefLinkNum", "eRoutingDefLink");
        }
        return eRoutingDefLink.ERoutingDefLinkNum;
    }

    public static long InsertNoCache(ERoutingDefLink eRoutingDefLink)
    {
        return InsertNoCache(eRoutingDefLink, false);
    }

    public static long InsertNoCache(ERoutingDefLink eRoutingDefLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eroutingdeflink (";
        if (isRandomKeys || useExistingPK) command += "ERoutingDefLinkNum,";
        command += "ERoutingDefNum,Fkey,ERoutingType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eRoutingDefLink.ERoutingDefLinkNum) + ",";
        command +=
            SOut.Long(eRoutingDefLink.ERoutingDefNum) + ","
                                                      + SOut.Long(eRoutingDefLink.Fkey) + ","
                                                      + SOut.Int((int) eRoutingDefLink.ERoutingType) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eRoutingDefLink.ERoutingDefLinkNum = Db.NonQ(command, true, "ERoutingDefLinkNum", "eRoutingDefLink");
        return eRoutingDefLink.ERoutingDefLinkNum;
    }

    public static void Update(ERoutingDefLink eRoutingDefLink)
    {
        var command = "UPDATE eroutingdeflink SET "
                      + "ERoutingDefNum    =  " + SOut.Long(eRoutingDefLink.ERoutingDefNum) + ", "
                      + "Fkey              =  " + SOut.Long(eRoutingDefLink.Fkey) + ", "
                      + "ERoutingType      =  " + SOut.Int((int) eRoutingDefLink.ERoutingType) + " "
                      + "WHERE ERoutingDefLinkNum = " + SOut.Long(eRoutingDefLink.ERoutingDefLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(ERoutingDefLink eRoutingDefLink, ERoutingDefLink oldERoutingDefLink)
    {
        var command = "";
        if (eRoutingDefLink.ERoutingDefNum != oldERoutingDefLink.ERoutingDefNum)
        {
            if (command != "") command += ",";
            command += "ERoutingDefNum = " + SOut.Long(eRoutingDefLink.ERoutingDefNum) + "";
        }

        if (eRoutingDefLink.Fkey != oldERoutingDefLink.Fkey)
        {
            if (command != "") command += ",";
            command += "Fkey = " + SOut.Long(eRoutingDefLink.Fkey) + "";
        }

        if (eRoutingDefLink.ERoutingType != oldERoutingDefLink.ERoutingType)
        {
            if (command != "") command += ",";
            command += "ERoutingType = " + SOut.Int((int) eRoutingDefLink.ERoutingType) + "";
        }

        if (command == "") return false;
        command = "UPDATE eroutingdeflink SET " + command
                                                + " WHERE ERoutingDefLinkNum = " + SOut.Long(eRoutingDefLink.ERoutingDefLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ERoutingDefLink eRoutingDefLink, ERoutingDefLink oldERoutingDefLink)
    {
        if (eRoutingDefLink.ERoutingDefNum != oldERoutingDefLink.ERoutingDefNum) return true;
        if (eRoutingDefLink.Fkey != oldERoutingDefLink.Fkey) return true;
        if (eRoutingDefLink.ERoutingType != oldERoutingDefLink.ERoutingType) return true;
        return false;
    }

    public static void Delete(long eRoutingDefLinkNum)
    {
        var command = "DELETE FROM eroutingdeflink "
                      + "WHERE ERoutingDefLinkNum = " + SOut.Long(eRoutingDefLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listERoutingDefLinkNums)
    {
        if (listERoutingDefLinkNums == null || listERoutingDefLinkNums.Count == 0) return;
        var command = "DELETE FROM eroutingdeflink "
                      + "WHERE ERoutingDefLinkNum IN(" + string.Join(",", listERoutingDefLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}