#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoPlanLinkCrud
{
    public static OrthoPlanLink SelectOne(long orthoPlanLinkNum)
    {
        var command = "SELECT * FROM orthoplanlink "
                      + "WHERE OrthoPlanLinkNum = " + SOut.Long(orthoPlanLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoPlanLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoPlanLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoPlanLink> TableToList(DataTable table)
    {
        var retVal = new List<OrthoPlanLink>();
        OrthoPlanLink orthoPlanLink;
        foreach (DataRow row in table.Rows)
        {
            orthoPlanLink = new OrthoPlanLink();
            orthoPlanLink.OrthoPlanLinkNum = SIn.Long(row["OrthoPlanLinkNum"].ToString());
            orthoPlanLink.OrthoCaseNum = SIn.Long(row["OrthoCaseNum"].ToString());
            orthoPlanLink.LinkType = (OrthoPlanLinkType) SIn.Int(row["LinkType"].ToString());
            orthoPlanLink.FKey = SIn.Long(row["FKey"].ToString());
            orthoPlanLink.IsActive = SIn.Bool(row["IsActive"].ToString());
            orthoPlanLink.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            orthoPlanLink.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            retVal.Add(orthoPlanLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoPlanLink> listOrthoPlanLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoPlanLink";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoPlanLinkNum");
        table.Columns.Add("OrthoCaseNum");
        table.Columns.Add("LinkType");
        table.Columns.Add("FKey");
        table.Columns.Add("IsActive");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecUserNumEntry");
        foreach (var orthoPlanLink in listOrthoPlanLinks)
            table.Rows.Add(SOut.Long(orthoPlanLink.OrthoPlanLinkNum), SOut.Long(orthoPlanLink.OrthoCaseNum), SOut.Int((int) orthoPlanLink.LinkType), SOut.Long(orthoPlanLink.FKey), SOut.Bool(orthoPlanLink.IsActive), SOut.DateT(orthoPlanLink.SecDateTEntry, false), SOut.Long(orthoPlanLink.SecUserNumEntry));
        return table;
    }

    public static long Insert(OrthoPlanLink orthoPlanLink)
    {
        return Insert(orthoPlanLink, false);
    }

    public static long Insert(OrthoPlanLink orthoPlanLink, bool useExistingPK)
    {
        var command = "INSERT INTO orthoplanlink (";

        command += "OrthoCaseNum,LinkType,FKey,IsActive,SecDateTEntry,SecUserNumEntry) VALUES(";

        command +=
            SOut.Long(orthoPlanLink.OrthoCaseNum) + ","
                                                  + SOut.Int((int) orthoPlanLink.LinkType) + ","
                                                  + SOut.Long(orthoPlanLink.FKey) + ","
                                                  + SOut.Bool(orthoPlanLink.IsActive) + ","
                                                  + DbHelper.Now() + ","
                                                  + SOut.Long(orthoPlanLink.SecUserNumEntry) + ")";
        {
            orthoPlanLink.OrthoPlanLinkNum = Db.NonQ(command, true, "OrthoPlanLinkNum", "orthoPlanLink");
        }
        return orthoPlanLink.OrthoPlanLinkNum;
    }

    public static long InsertNoCache(OrthoPlanLink orthoPlanLink)
    {
        return InsertNoCache(orthoPlanLink, false);
    }

    public static long InsertNoCache(OrthoPlanLink orthoPlanLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthoplanlink (";
        if (isRandomKeys || useExistingPK) command += "OrthoPlanLinkNum,";
        command += "OrthoCaseNum,LinkType,FKey,IsActive,SecDateTEntry,SecUserNumEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoPlanLink.OrthoPlanLinkNum) + ",";
        command +=
            SOut.Long(orthoPlanLink.OrthoCaseNum) + ","
                                                  + SOut.Int((int) orthoPlanLink.LinkType) + ","
                                                  + SOut.Long(orthoPlanLink.FKey) + ","
                                                  + SOut.Bool(orthoPlanLink.IsActive) + ","
                                                  + DbHelper.Now() + ","
                                                  + SOut.Long(orthoPlanLink.SecUserNumEntry) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoPlanLink.OrthoPlanLinkNum = Db.NonQ(command, true, "OrthoPlanLinkNum", "orthoPlanLink");
        return orthoPlanLink.OrthoPlanLinkNum;
    }

    public static void Update(OrthoPlanLink orthoPlanLink)
    {
        var command = "UPDATE orthoplanlink SET "
                      + "OrthoCaseNum    =  " + SOut.Long(orthoPlanLink.OrthoCaseNum) + ", "
                      + "LinkType        =  " + SOut.Int((int) orthoPlanLink.LinkType) + ", "
                      + "FKey            =  " + SOut.Long(orthoPlanLink.FKey) + ", "
                      + "IsActive        =  " + SOut.Bool(orthoPlanLink.IsActive) + ", "
                      //SecDateTEntry not allowed to change
                      + "SecUserNumEntry =  " + SOut.Long(orthoPlanLink.SecUserNumEntry) + " "
                      + "WHERE OrthoPlanLinkNum = " + SOut.Long(orthoPlanLink.OrthoPlanLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoPlanLink orthoPlanLink, OrthoPlanLink oldOrthoPlanLink)
    {
        var command = "";
        if (orthoPlanLink.OrthoCaseNum != oldOrthoPlanLink.OrthoCaseNum)
        {
            if (command != "") command += ",";
            command += "OrthoCaseNum = " + SOut.Long(orthoPlanLink.OrthoCaseNum) + "";
        }

        if (orthoPlanLink.LinkType != oldOrthoPlanLink.LinkType)
        {
            if (command != "") command += ",";
            command += "LinkType = " + SOut.Int((int) orthoPlanLink.LinkType) + "";
        }

        if (orthoPlanLink.FKey != oldOrthoPlanLink.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(orthoPlanLink.FKey) + "";
        }

        if (orthoPlanLink.IsActive != oldOrthoPlanLink.IsActive)
        {
            if (command != "") command += ",";
            command += "IsActive = " + SOut.Bool(orthoPlanLink.IsActive) + "";
        }

        //SecDateTEntry not allowed to change
        if (orthoPlanLink.SecUserNumEntry != oldOrthoPlanLink.SecUserNumEntry)
        {
            if (command != "") command += ",";
            command += "SecUserNumEntry = " + SOut.Long(orthoPlanLink.SecUserNumEntry) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthoplanlink SET " + command
                                              + " WHERE OrthoPlanLinkNum = " + SOut.Long(orthoPlanLink.OrthoPlanLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoPlanLink orthoPlanLink, OrthoPlanLink oldOrthoPlanLink)
    {
        if (orthoPlanLink.OrthoCaseNum != oldOrthoPlanLink.OrthoCaseNum) return true;
        if (orthoPlanLink.LinkType != oldOrthoPlanLink.LinkType) return true;
        if (orthoPlanLink.FKey != oldOrthoPlanLink.FKey) return true;
        if (orthoPlanLink.IsActive != oldOrthoPlanLink.IsActive) return true;
        //SecDateTEntry not allowed to change
        if (orthoPlanLink.SecUserNumEntry != oldOrthoPlanLink.SecUserNumEntry) return true;
        return false;
    }

    public static void Delete(long orthoPlanLinkNum)
    {
        var command = "DELETE FROM orthoplanlink "
                      + "WHERE OrthoPlanLinkNum = " + SOut.Long(orthoPlanLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoPlanLinkNums)
    {
        if (listOrthoPlanLinkNums == null || listOrthoPlanLinkNums.Count == 0) return;
        var command = "DELETE FROM orthoplanlink "
                      + "WHERE OrthoPlanLinkNum IN(" + string.Join(",", listOrthoPlanLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}