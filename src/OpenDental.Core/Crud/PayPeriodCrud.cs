using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PayPeriodCrud
{
    public static PayPeriod SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayPeriod> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayPeriod> TableToList(DataTable table)
    {
        var retVal = new List<PayPeriod>();
        foreach (DataRow row in table.Rows)
        {
            var payPeriod = new PayPeriod
            {
                PayPeriodNum = SIn.Long(row["PayPeriodNum"].ToString()),
                DateStart = SIn.Date(row["DateStart"].ToString()),
                DateStop = SIn.Date(row["DateStop"].ToString()),
                DatePaycheck = SIn.Date(row["DatePaycheck"].ToString())
            };
            retVal.Add(payPeriod);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PayPeriod> listPayPeriods, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayPeriod";
        var table = new DataTable(tableName);
        table.Columns.Add("PayPeriodNum");
        table.Columns.Add("DateStart");
        table.Columns.Add("DateStop");
        table.Columns.Add("DatePaycheck");
        foreach (var payPeriod in listPayPeriods)
            table.Rows.Add(SOut.Long(payPeriod.PayPeriodNum), SOut.DateTime(payPeriod.DateStart, false), SOut.DateTime(payPeriod.DateStop, false), SOut.DateTime(payPeriod.DatePaycheck, false));
        return table;
    }

    public static void Insert(PayPeriod payPeriod)
    {
        var command = "INSERT INTO payperiod (";

        command += "DateStart,DateStop,DatePaycheck) VALUES(";

        command +=
            SOut.Date(payPeriod.DateStart) + ","
                                           + SOut.Date(payPeriod.DateStop) + ","
                                           + SOut.Date(payPeriod.DatePaycheck) + ")";
        {
            payPeriod.PayPeriodNum = Db.NonQ(command, true, "PayPeriodNum", "payPeriod");
        }
    }

    public static void Update(PayPeriod payPeriod)
    {
        var command = "UPDATE payperiod SET "
                      + "DateStart   =  " + SOut.Date(payPeriod.DateStart) + ", "
                      + "DateStop    =  " + SOut.Date(payPeriod.DateStop) + ", "
                      + "DatePaycheck=  " + SOut.Date(payPeriod.DatePaycheck) + " "
                      + "WHERE PayPeriodNum = " + SOut.Long(payPeriod.PayPeriodNum);
        Db.NonQ(command);
    }
}