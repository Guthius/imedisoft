using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class CdcrecCrud
{
    public static List<Cdcrec> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Cdcrec> TableToList(DataTable table)
    {
        var retVal = new List<Cdcrec>();
        foreach (DataRow row in table.Rows)
        {
            var cdcrec = new Cdcrec
            {
                CdcrecNum = SIn.Long(row["CdcrecNum"].ToString()),
                CdcrecCode = SIn.String(row["CdcrecCode"].ToString()),
                HeirarchicalCode = SIn.String(row["HeirarchicalCode"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(cdcrec);
        }

        return retVal;
    }

    public static void Insert(Cdcrec cdcrec)
    {
        var command = "INSERT INTO cdcrec (";

        command += "CdcrecCode,HeirarchicalCode,Description) VALUES(";

        command +=
            "'" + SOut.String(cdcrec.CdcrecCode) + "',"
            + "'" + SOut.String(cdcrec.HeirarchicalCode) + "',"
            + "'" + SOut.String(cdcrec.Description) + "')";
        {
            cdcrec.CdcrecNum = Db.NonQ(command, true, "CdcrecNum", "cdcrec");
        }
    }

    public static void Update(Cdcrec cdcrec)
    {
        var command = "UPDATE cdcrec SET "
                      + "CdcrecCode      = '" + SOut.String(cdcrec.CdcrecCode) + "', "
                      + "HeirarchicalCode= '" + SOut.String(cdcrec.HeirarchicalCode) + "', "
                      + "Description     = '" + SOut.String(cdcrec.Description) + "' "
                      + "WHERE CdcrecNum = " + SOut.Long(cdcrec.CdcrecNum);
        Db.NonQ(command);
    }
}