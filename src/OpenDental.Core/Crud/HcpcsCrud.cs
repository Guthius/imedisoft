using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HcpcsCrud
{
    public static List<Hcpcs> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Hcpcs> TableToList(DataTable table)
    {
        var retVal = new List<Hcpcs>();
        foreach (DataRow row in table.Rows)
        {
            var hcpcs = new Hcpcs
            {
                HcpcsNum = SIn.Long(row["HcpcsNum"].ToString()),
                HcpcsCode = SIn.String(row["HcpcsCode"].ToString()),
                DescriptionShort = SIn.String(row["DescriptionShort"].ToString())
            };
            retVal.Add(hcpcs);
        }

        return retVal;
    }

    public static void Insert(Hcpcs hcpcs)
    {
        var command = "INSERT INTO hcpcs (";

        command += "HcpcsCode,DescriptionShort) VALUES(";

        command +=
            "'" + SOut.String(hcpcs.HcpcsCode) + "',"
            + "'" + SOut.String(hcpcs.DescriptionShort) + "')";
        {
            hcpcs.HcpcsNum = Db.NonQ(command, true, "HcpcsNum", "hcpcs");
        }
    }

    public static void Update(Hcpcs hcpcs)
    {
        var command = "UPDATE hcpcs SET "
                      + "HcpcsCode       = '" + SOut.String(hcpcs.HcpcsCode) + "', "
                      + "DescriptionShort= '" + SOut.String(hcpcs.DescriptionShort) + "' "
                      + "WHERE HcpcsNum = " + SOut.Long(hcpcs.HcpcsNum);
        Db.NonQ(command);
    }
}