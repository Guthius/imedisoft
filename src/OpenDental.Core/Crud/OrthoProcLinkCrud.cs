using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoProcLinkCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var orthoProcLink = new OrthoProcLink
            {
                OrthoProcLinkNum = SIn.Long(row["OrthoProcLinkNum"].ToString()),
                OrthoCaseNum = SIn.Long(row["OrthoCaseNum"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                ProcLinkType = (OrthoProcType) SIn.Int(row["ProcLinkType"].ToString())
            };
            retVal.Add(orthoProcLink);
        }

        return retVal;
    }

    public static void Insert(OrthoProcLink orthoProcLink)
    {
        var command = "INSERT INTO orthoproclink (";

        command += "OrthoCaseNum,ProcNum,SecDateTEntry,SecUserNumEntry,ProcLinkType) VALUES(";

        command +=
            SOut.Long(orthoProcLink.OrthoCaseNum) + ","
                                                  + SOut.Long(orthoProcLink.ProcNum) + ","
                                                  + "NOW()" + ","
                                                  + SOut.Long(orthoProcLink.SecUserNumEntry) + ","
                                                  + SOut.Int((int) orthoProcLink.ProcLinkType) + ")";
        {
            orthoProcLink.OrthoProcLinkNum = Db.NonQ(command, true, "OrthoProcLinkNum", "orthoProcLink");
        }
    }

    public static void Update(OrthoProcLink orthoProcLink, OrthoProcLink oldOrthoProcLink)
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

        if (command == "") return;
        command = "UPDATE orthoproclink SET " + command
                                              + " WHERE OrthoProcLinkNum = " + SOut.Long(orthoProcLink.OrthoProcLinkNum);
        Db.NonQ(command);
    }

    public static void Delete(long orthoProcLinkNum)
    {
        var command = "DELETE FROM orthoproclink "
                      + "WHERE OrthoProcLinkNum = " + SOut.Long(orthoProcLinkNum);
        Db.NonQ(command);
    }
}