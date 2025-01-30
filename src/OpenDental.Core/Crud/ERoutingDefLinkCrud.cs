using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ERoutingDefLinkCrud
{
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

    public static void Insert(ERoutingDefLink eRoutingDefLink)
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
    }
}