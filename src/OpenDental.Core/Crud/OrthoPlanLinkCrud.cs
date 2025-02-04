using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoPlanLinkCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var orthoPlanLink = new OrthoPlanLink
            {
                OrthoPlanLinkNum = SIn.Long(row["OrthoPlanLinkNum"].ToString()),
                OrthoCaseNum = SIn.Long(row["OrthoCaseNum"].ToString()),
                LinkType = (OrthoPlanLinkType) SIn.Int(row["LinkType"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                IsActive = SIn.Bool(row["IsActive"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString())
            };
            retVal.Add(orthoPlanLink);
        }

        return retVal;
    }

    public static void Insert(OrthoPlanLink orthoPlanLink)
    {
        var command = "INSERT INTO orthoplanlink (";

        command += "OrthoCaseNum,LinkType,FKey,IsActive,SecDateTEntry,SecUserNumEntry) VALUES(";

        command +=
            SOut.Long(orthoPlanLink.OrthoCaseNum) + ","
                                                  + SOut.Int((int) orthoPlanLink.LinkType) + ","
                                                  + SOut.Long(orthoPlanLink.FKey) + ","
                                                  + SOut.Bool(orthoPlanLink.IsActive) + ","
                                                  + "NOW()" + ","
                                                  + SOut.Long(orthoPlanLink.SecUserNumEntry) + ")";
        {
            orthoPlanLink.OrthoPlanLinkNum = Db.NonQ(command, true, "OrthoPlanLinkNum", "orthoPlanLink");
        }
    }

    public static void Update(OrthoPlanLink orthoPlanLink, OrthoPlanLink oldOrthoPlanLink)
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

        if (command == "") return;
        command = "UPDATE orthoplanlink SET " + command
                                              + " WHERE OrthoPlanLinkNum = " + SOut.Long(orthoPlanLink.OrthoPlanLinkNum);
        Db.NonQ(command);
    }

    public static void Delete(long orthoPlanLinkNum)
    {
        var command = "DELETE FROM orthoplanlink "
                      + "WHERE OrthoPlanLinkNum = " + SOut.Long(orthoPlanLinkNum);
        Db.NonQ(command);
    }
}