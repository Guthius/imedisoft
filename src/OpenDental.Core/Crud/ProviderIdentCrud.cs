using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProviderIdentCrud
{
    public static List<ProviderIdent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProviderIdent> TableToList(DataTable table)
    {
        var retVal = new List<ProviderIdent>();
        foreach (DataRow row in table.Rows)
        {
            var providerIdent = new ProviderIdent
            {
                ProviderIdentNum = SIn.Long(row["ProviderIdentNum"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                PayorID = SIn.String(row["PayorID"].ToString()),
                SuppIDType = (ProviderSupplementalID) SIn.Int(row["SuppIDType"].ToString()),
                IDNumber = SIn.String(row["IDNumber"].ToString())
            };
            retVal.Add(providerIdent);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProviderIdent> listProviderIdents, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProviderIdent";
        var table = new DataTable(tableName);
        table.Columns.Add("ProviderIdentNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("PayorID");
        table.Columns.Add("SuppIDType");
        table.Columns.Add("IDNumber");
        foreach (var providerIdent in listProviderIdents)
            table.Rows.Add(SOut.Long(providerIdent.ProviderIdentNum), SOut.Long(providerIdent.ProvNum), providerIdent.PayorID, SOut.Int((int) providerIdent.SuppIDType), providerIdent.IDNumber);
        return table;
    }
}