#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PayPeriodCrud
{
    public static PayPeriod SelectOne(long payPeriodNum)
    {
        var command = "SELECT * FROM payperiod "
                      + "WHERE PayPeriodNum = " + SOut.Long(payPeriodNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        PayPeriod payPeriod;
        foreach (DataRow row in table.Rows)
        {
            payPeriod = new PayPeriod();
            payPeriod.PayPeriodNum = SIn.Long(row["PayPeriodNum"].ToString());
            payPeriod.DateStart = SIn.Date(row["DateStart"].ToString());
            payPeriod.DateStop = SIn.Date(row["DateStop"].ToString());
            payPeriod.DatePaycheck = SIn.Date(row["DatePaycheck"].ToString());
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
            table.Rows.Add(SOut.Long(payPeriod.PayPeriodNum), SOut.DateT(payPeriod.DateStart, false), SOut.DateT(payPeriod.DateStop, false), SOut.DateT(payPeriod.DatePaycheck, false));
        return table;
    }

    public static long Insert(PayPeriod payPeriod)
    {
        return Insert(payPeriod, false);
    }

    public static long Insert(PayPeriod payPeriod, bool useExistingPK)
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
        return payPeriod.PayPeriodNum;
    }

    public static long InsertNoCache(PayPeriod payPeriod)
    {
        return InsertNoCache(payPeriod, false);
    }

    public static long InsertNoCache(PayPeriod payPeriod, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payperiod (";
        if (isRandomKeys || useExistingPK) command += "PayPeriodNum,";
        command += "DateStart,DateStop,DatePaycheck) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payPeriod.PayPeriodNum) + ",";
        command +=
            SOut.Date(payPeriod.DateStart) + ","
                                           + SOut.Date(payPeriod.DateStop) + ","
                                           + SOut.Date(payPeriod.DatePaycheck) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            payPeriod.PayPeriodNum = Db.NonQ(command, true, "PayPeriodNum", "payPeriod");
        return payPeriod.PayPeriodNum;
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

    public static bool Update(PayPeriod payPeriod, PayPeriod oldPayPeriod)
    {
        var command = "";
        if (payPeriod.DateStart.Date != oldPayPeriod.DateStart.Date)
        {
            if (command != "") command += ",";
            command += "DateStart = " + SOut.Date(payPeriod.DateStart) + "";
        }

        if (payPeriod.DateStop.Date != oldPayPeriod.DateStop.Date)
        {
            if (command != "") command += ",";
            command += "DateStop = " + SOut.Date(payPeriod.DateStop) + "";
        }

        if (payPeriod.DatePaycheck.Date != oldPayPeriod.DatePaycheck.Date)
        {
            if (command != "") command += ",";
            command += "DatePaycheck = " + SOut.Date(payPeriod.DatePaycheck) + "";
        }

        if (command == "") return false;
        command = "UPDATE payperiod SET " + command
                                          + " WHERE PayPeriodNum = " + SOut.Long(payPeriod.PayPeriodNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PayPeriod payPeriod, PayPeriod oldPayPeriod)
    {
        if (payPeriod.DateStart.Date != oldPayPeriod.DateStart.Date) return true;
        if (payPeriod.DateStop.Date != oldPayPeriod.DateStop.Date) return true;
        if (payPeriod.DatePaycheck.Date != oldPayPeriod.DatePaycheck.Date) return true;
        return false;
    }

    public static void Delete(long payPeriodNum)
    {
        var command = "DELETE FROM payperiod "
                      + "WHERE PayPeriodNum = " + SOut.Long(payPeriodNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayPeriodNums)
    {
        if (listPayPeriodNums == null || listPayPeriodNums.Count == 0) return;
        var command = "DELETE FROM payperiod "
                      + "WHERE PayPeriodNum IN(" + string.Join(",", listPayPeriodNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}