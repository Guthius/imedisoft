#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class AccountingAutoPayCrud
{
    public static List<AccountingAutoPay> SelectMany(string command)
    {
        return TableToList(DataCore.GetTable(command));
    }

    public static List<AccountingAutoPay> TableToList(DataTable table)
    {
        return table.Rows
            .Cast<DataRow>()
            .Select(row => new AccountingAutoPay
            {
                AccountingAutoPayNum = SIn.Long(row["AccountingAutoPayNum"].ToString()),
                PayType = SIn.Long(row["PayType"].ToString()),
                PickList = SIn.String(row["PickList"].ToString())
            })
            .ToList();
    }

    public static DataTable ListToTable(List<AccountingAutoPay> listAccountingAutoPays, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AccountingAutoPay";
        var table = new DataTable(tableName);
        table.Columns.Add("AccountingAutoPayNum");
        table.Columns.Add("PayType");
        table.Columns.Add("PickList");
        foreach (var accountingAutoPay in listAccountingAutoPays)
            table.Rows.Add(SOut.Long(accountingAutoPay.AccountingAutoPayNum), SOut.Long(accountingAutoPay.PayType), accountingAutoPay.PickList);
        return table;
    }

    public static long Insert(AccountingAutoPay accountingAutoPay)
    {
        var command = "INSERT INTO accountingautopay (";
        command += "PayType,PickList) VALUES(";
        command += SOut.Long(accountingAutoPay.PayType) + "," + "'" + SOut.String(accountingAutoPay.PickList) + "')";
        accountingAutoPay.AccountingAutoPayNum = Db.NonQ(command, true, "AccountingAutoPayNum", "accountingAutoPay");
        return accountingAutoPay.AccountingAutoPayNum;
    }
}