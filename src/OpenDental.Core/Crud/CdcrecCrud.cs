using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        Cdcrec cdcrec;
        foreach (DataRow row in table.Rows)
        {
            cdcrec = new Cdcrec();
            cdcrec.CdcrecNum = SIn.Long(row["CdcrecNum"].ToString());
            cdcrec.CdcrecCode = SIn.String(row["CdcrecCode"].ToString());
            cdcrec.HeirarchicalCode = SIn.String(row["HeirarchicalCode"].ToString());
            cdcrec.Description = SIn.String(row["Description"].ToString());
            retVal.Add(cdcrec);
        }

        return retVal;
    }

    public static long Insert(Cdcrec cdcrec)
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
        return cdcrec.CdcrecNum;
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