using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class CanadianNetworkCrud
{
    public static List<CanadianNetwork> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CanadianNetwork> TableToList(DataTable table)
    {
        var retVal = new List<CanadianNetwork>();
        foreach (DataRow row in table.Rows)
        {
            var canadianNetwork = new CanadianNetwork
            {
                CanadianNetworkNum = SIn.Long(row["CanadianNetworkNum"].ToString()),
                Abbrev = SIn.String(row["Abbrev"].ToString()),
                Descript = SIn.String(row["Descript"].ToString()),
                CanadianTransactionPrefix = SIn.String(row["CanadianTransactionPrefix"].ToString()),
                CanadianIsRprHandler = SIn.Bool(row["CanadianIsRprHandler"].ToString())
            };
            retVal.Add(canadianNetwork);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<CanadianNetwork> listCanadianNetworks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "CanadianNetwork";
        var table = new DataTable(tableName);
        table.Columns.Add("CanadianNetworkNum");
        table.Columns.Add("Abbrev");
        table.Columns.Add("Descript");
        table.Columns.Add("CanadianTransactionPrefix");
        table.Columns.Add("CanadianIsRprHandler");
        foreach (var canadianNetwork in listCanadianNetworks)
            table.Rows.Add(SOut.Long(canadianNetwork.CanadianNetworkNum), canadianNetwork.Abbrev, canadianNetwork.Descript, canadianNetwork.CanadianTransactionPrefix, SOut.Bool(canadianNetwork.CanadianIsRprHandler));
        return table;
    }
}