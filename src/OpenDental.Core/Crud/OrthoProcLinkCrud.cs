#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoProcLinkCrud
{
    public static OrthoProcLink SelectOne(long orthoProcLinkNum)
    {
        var command = "SELECT * FROM orthoproclink "
                      + "WHERE OrthoProcLinkNum = " + SOut.Long(orthoProcLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoProcLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoProcLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoProcLink> TableToList(DataTable table)
    {
        var retVal = new List<OrthoProcLink>();
        OrthoProcLink orthoProcLink;
        foreach (DataRow row in table.Rows)
        {
            orthoProcLink = new OrthoProcLink();
            orthoProcLink.OrthoProcLinkNum = SIn.Long(row["OrthoProcLinkNum"].ToString());
            orthoProcLink.OrthoCaseNum = SIn.Long(row["OrthoCaseNum"].ToString());
            orthoProcLink.ProcNum = SIn.Long(row["ProcNum"].ToString());
            orthoProcLink.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            orthoProcLink.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            orthoProcLink.ProcLinkType = (OrthoProcType) SIn.Int(row["ProcLinkType"].ToString());
            retVal.Add(orthoProcLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoProcLink> listOrthoProcLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoProcLink";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoProcLinkNum");
        table.Columns.Add("OrthoCaseNum");
        table.Columns.Add("ProcNum");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("ProcLinkType");
        foreach (var orthoProcLink in listOrthoProcLinks)
            table.Rows.Add(SOut.Long(orthoProcLink.OrthoProcLinkNum), SOut.Long(orthoProcLink.OrthoCaseNum), SOut.Long(orthoProcLink.ProcNum), SOut.DateT(orthoProcLink.SecDateTEntry, false), SOut.Long(orthoProcLink.SecUserNumEntry), SOut.Int((int) orthoProcLink.ProcLinkType));
        return table;
    }

    public static long Insert(OrthoProcLink orthoProcLink)
    {
        return Insert(orthoProcLink, false);
    }

    public static long Insert(OrthoProcLink orthoProcLink, bool useExistingPK)
    {
        var command = "INSERT INTO orthoproclink (";

        command += "OrthoCaseNum,ProcNum,SecDateTEntry,SecUserNumEntry,ProcLinkType) VALUES(";

        command +=
            SOut.Long(orthoProcLink.OrthoCaseNum) + ","
                                                  + SOut.Long(orthoProcLink.ProcNum) + ","
                                                  + DbHelper.Now() + ","
                                                  + SOut.Long(orthoProcLink.SecUserNumEntry) + ","
                                                  + SOut.Int((int) orthoProcLink.ProcLinkType) + ")";
        {
            orthoProcLink.OrthoProcLinkNum = Db.NonQ(command, true, "OrthoProcLinkNum", "orthoProcLink");
        }
        return orthoProcLink.OrthoProcLinkNum;
    }

    public static long InsertNoCache(OrthoProcLink orthoProcLink)
    {
        return InsertNoCache(orthoProcLink, false);
    }

    public static long InsertNoCache(OrthoProcLink orthoProcLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthoproclink (";
        if (isRandomKeys || useExistingPK) command += "OrthoProcLinkNum,";
        command += "OrthoCaseNum,ProcNum,SecDateTEntry,SecUserNumEntry,ProcLinkType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoProcLink.OrthoProcLinkNum) + ",";
        command +=
            SOut.Long(orthoProcLink.OrthoCaseNum) + ","
                                                  + SOut.Long(orthoProcLink.ProcNum) + ","
                                                  + DbHelper.Now() + ","
                                                  + SOut.Long(orthoProcLink.SecUserNumEntry) + ","
                                                  + SOut.Int((int) orthoProcLink.ProcLinkType) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoProcLink.OrthoProcLinkNum = Db.NonQ(command, true, "OrthoProcLinkNum", "orthoProcLink");
        return orthoProcLink.OrthoProcLinkNum;
    }

    public static void Update(OrthoProcLink orthoProcLink)
    {
        var command = "UPDATE orthoproclink SET "
                      + "OrthoCaseNum    =  " + SOut.Long(orthoProcLink.OrthoCaseNum) + ", "
                      + "ProcNum         =  " + SOut.Long(orthoProcLink.ProcNum) + ", "
                      //SecDateTEntry not allowed to change
                      + "SecUserNumEntry =  " + SOut.Long(orthoProcLink.SecUserNumEntry) + ", "
                      + "ProcLinkType    =  " + SOut.Int((int) orthoProcLink.ProcLinkType) + " "
                      + "WHERE OrthoProcLinkNum = " + SOut.Long(orthoProcLink.OrthoProcLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoProcLink orthoProcLink, OrthoProcLink oldOrthoProcLink)
    {
        var command = "";
        if (orthoProcLink.OrthoCaseNum != oldOrthoProcLink.OrthoCaseNum)
        {
            if (command != "") command += ",";
            command += "OrthoCaseNum = " + SOut.Long(orthoProcLink.OrthoCaseNum) + "";
        }

        if (orthoProcLink.ProcNum != oldOrthoProcLink.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(orthoProcLink.ProcNum) + "";
        }

        //SecDateTEntry not allowed to change
        if (orthoProcLink.SecUserNumEntry != oldOrthoProcLink.SecUserNumEntry)
        {
            if (command != "") command += ",";
            command += "SecUserNumEntry = " + SOut.Long(orthoProcLink.SecUserNumEntry) + "";
        }

        if (orthoProcLink.ProcLinkType != oldOrthoProcLink.ProcLinkType)
        {
            if (command != "") command += ",";
            command += "ProcLinkType = " + SOut.Int((int) orthoProcLink.ProcLinkType) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthoproclink SET " + command
                                              + " WHERE OrthoProcLinkNum = " + SOut.Long(orthoProcLink.OrthoProcLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoProcLink orthoProcLink, OrthoProcLink oldOrthoProcLink)
    {
        if (orthoProcLink.OrthoCaseNum != oldOrthoProcLink.OrthoCaseNum) return true;
        if (orthoProcLink.ProcNum != oldOrthoProcLink.ProcNum) return true;
        //SecDateTEntry not allowed to change
        if (orthoProcLink.SecUserNumEntry != oldOrthoProcLink.SecUserNumEntry) return true;
        if (orthoProcLink.ProcLinkType != oldOrthoProcLink.ProcLinkType) return true;
        return false;
    }

    public static void Delete(long orthoProcLinkNum)
    {
        var command = "DELETE FROM orthoproclink "
                      + "WHERE OrthoProcLinkNum = " + SOut.Long(orthoProcLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoProcLinkNums)
    {
        if (listOrthoProcLinkNums == null || listOrthoProcLinkNums.Count == 0) return;
        var command = "DELETE FROM orthoproclink "
                      + "WHERE OrthoProcLinkNum IN(" + string.Join(",", listOrthoProcLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}