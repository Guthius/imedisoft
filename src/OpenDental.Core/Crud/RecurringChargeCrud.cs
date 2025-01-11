#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RecurringChargeCrud
{
    public static RecurringCharge SelectOne(long recurringChargeNum)
    {
        var command = "SELECT * FROM recurringcharge "
                      + "WHERE RecurringChargeNum = " + SOut.Long(recurringChargeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RecurringCharge SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RecurringCharge> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RecurringCharge> TableToList(DataTable table)
    {
        var retVal = new List<RecurringCharge>();
        RecurringCharge recurringCharge;
        foreach (DataRow row in table.Rows)
        {
            recurringCharge = new RecurringCharge();
            recurringCharge.RecurringChargeNum = SIn.Long(row["RecurringChargeNum"].ToString());
            recurringCharge.PatNum = SIn.Long(row["PatNum"].ToString());
            recurringCharge.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            recurringCharge.DateTimeCharge = SIn.DateTime(row["DateTimeCharge"].ToString());
            recurringCharge.ChargeStatus = (RecurringChargeStatus) SIn.Int(row["ChargeStatus"].ToString());
            recurringCharge.FamBal = SIn.Double(row["FamBal"].ToString());
            recurringCharge.PayPlanDue = SIn.Double(row["PayPlanDue"].ToString());
            recurringCharge.TotalDue = SIn.Double(row["TotalDue"].ToString());
            recurringCharge.RepeatAmt = SIn.Double(row["RepeatAmt"].ToString());
            recurringCharge.ChargeAmt = SIn.Double(row["ChargeAmt"].ToString());
            recurringCharge.UserNum = SIn.Long(row["UserNum"].ToString());
            recurringCharge.PayNum = SIn.Long(row["PayNum"].ToString());
            recurringCharge.CreditCardNum = SIn.Long(row["CreditCardNum"].ToString());
            recurringCharge.ErrorMsg = SIn.String(row["ErrorMsg"].ToString());
            retVal.Add(recurringCharge);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RecurringCharge> listRecurringCharges, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RecurringCharge";
        var table = new DataTable(tableName);
        table.Columns.Add("RecurringChargeNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("DateTimeCharge");
        table.Columns.Add("ChargeStatus");
        table.Columns.Add("FamBal");
        table.Columns.Add("PayPlanDue");
        table.Columns.Add("TotalDue");
        table.Columns.Add("RepeatAmt");
        table.Columns.Add("ChargeAmt");
        table.Columns.Add("UserNum");
        table.Columns.Add("PayNum");
        table.Columns.Add("CreditCardNum");
        table.Columns.Add("ErrorMsg");
        foreach (var recurringCharge in listRecurringCharges)
            table.Rows.Add(SOut.Long(recurringCharge.RecurringChargeNum), SOut.Long(recurringCharge.PatNum), SOut.Long(recurringCharge.ClinicNum), SOut.DateT(recurringCharge.DateTimeCharge, false), SOut.Int((int) recurringCharge.ChargeStatus), SOut.Double(recurringCharge.FamBal), SOut.Double(recurringCharge.PayPlanDue), SOut.Double(recurringCharge.TotalDue), SOut.Double(recurringCharge.RepeatAmt), SOut.Double(recurringCharge.ChargeAmt), SOut.Long(recurringCharge.UserNum), SOut.Long(recurringCharge.PayNum), SOut.Long(recurringCharge.CreditCardNum), recurringCharge.ErrorMsg);
        return table;
    }

    public static long Insert(RecurringCharge recurringCharge)
    {
        return Insert(recurringCharge, false);
    }

    public static long Insert(RecurringCharge recurringCharge, bool useExistingPK)
    {
        var command = "INSERT INTO recurringcharge (";

        command += "PatNum,ClinicNum,DateTimeCharge,ChargeStatus,FamBal,PayPlanDue,TotalDue,RepeatAmt,ChargeAmt,UserNum,PayNum,CreditCardNum,ErrorMsg) VALUES(";

        command +=
            SOut.Long(recurringCharge.PatNum) + ","
                                              + SOut.Long(recurringCharge.ClinicNum) + ","
                                              + SOut.DateT(recurringCharge.DateTimeCharge) + ","
                                              + SOut.Int((int) recurringCharge.ChargeStatus) + ","
                                              + SOut.Double(recurringCharge.FamBal) + ","
                                              + SOut.Double(recurringCharge.PayPlanDue) + ","
                                              + SOut.Double(recurringCharge.TotalDue) + ","
                                              + SOut.Double(recurringCharge.RepeatAmt) + ","
                                              + SOut.Double(recurringCharge.ChargeAmt) + ","
                                              + SOut.Long(recurringCharge.UserNum) + ","
                                              + SOut.Long(recurringCharge.PayNum) + ","
                                              + SOut.Long(recurringCharge.CreditCardNum) + ","
                                              + DbHelper.ParamChar + "paramErrorMsg)";
        if (recurringCharge.ErrorMsg == null) recurringCharge.ErrorMsg = "";
        var paramErrorMsg = new OdSqlParameter("paramErrorMsg", OdDbType.Text, SOut.StringParam(recurringCharge.ErrorMsg));
        {
            recurringCharge.RecurringChargeNum = Db.NonQ(command, true, "RecurringChargeNum", "recurringCharge", paramErrorMsg);
        }
        return recurringCharge.RecurringChargeNum;
    }

    public static long InsertNoCache(RecurringCharge recurringCharge)
    {
        return InsertNoCache(recurringCharge, false);
    }

    public static long InsertNoCache(RecurringCharge recurringCharge, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO recurringcharge (";
        if (isRandomKeys || useExistingPK) command += "RecurringChargeNum,";
        command += "PatNum,ClinicNum,DateTimeCharge,ChargeStatus,FamBal,PayPlanDue,TotalDue,RepeatAmt,ChargeAmt,UserNum,PayNum,CreditCardNum,ErrorMsg) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(recurringCharge.RecurringChargeNum) + ",";
        command +=
            SOut.Long(recurringCharge.PatNum) + ","
                                              + SOut.Long(recurringCharge.ClinicNum) + ","
                                              + SOut.DateT(recurringCharge.DateTimeCharge) + ","
                                              + SOut.Int((int) recurringCharge.ChargeStatus) + ","
                                              + SOut.Double(recurringCharge.FamBal) + ","
                                              + SOut.Double(recurringCharge.PayPlanDue) + ","
                                              + SOut.Double(recurringCharge.TotalDue) + ","
                                              + SOut.Double(recurringCharge.RepeatAmt) + ","
                                              + SOut.Double(recurringCharge.ChargeAmt) + ","
                                              + SOut.Long(recurringCharge.UserNum) + ","
                                              + SOut.Long(recurringCharge.PayNum) + ","
                                              + SOut.Long(recurringCharge.CreditCardNum) + ","
                                              + DbHelper.ParamChar + "paramErrorMsg)";
        if (recurringCharge.ErrorMsg == null) recurringCharge.ErrorMsg = "";
        var paramErrorMsg = new OdSqlParameter("paramErrorMsg", OdDbType.Text, SOut.StringParam(recurringCharge.ErrorMsg));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramErrorMsg);
        else
            recurringCharge.RecurringChargeNum = Db.NonQ(command, true, "RecurringChargeNum", "recurringCharge", paramErrorMsg);
        return recurringCharge.RecurringChargeNum;
    }

    public static void Update(RecurringCharge recurringCharge)
    {
        var command = "UPDATE recurringcharge SET "
                      + "PatNum            =  " + SOut.Long(recurringCharge.PatNum) + ", "
                      + "ClinicNum         =  " + SOut.Long(recurringCharge.ClinicNum) + ", "
                      + "DateTimeCharge    =  " + SOut.DateT(recurringCharge.DateTimeCharge) + ", "
                      + "ChargeStatus      =  " + SOut.Int((int) recurringCharge.ChargeStatus) + ", "
                      + "FamBal            =  " + SOut.Double(recurringCharge.FamBal) + ", "
                      + "PayPlanDue        =  " + SOut.Double(recurringCharge.PayPlanDue) + ", "
                      + "TotalDue          =  " + SOut.Double(recurringCharge.TotalDue) + ", "
                      + "RepeatAmt         =  " + SOut.Double(recurringCharge.RepeatAmt) + ", "
                      + "ChargeAmt         =  " + SOut.Double(recurringCharge.ChargeAmt) + ", "
                      + "UserNum           =  " + SOut.Long(recurringCharge.UserNum) + ", "
                      + "PayNum            =  " + SOut.Long(recurringCharge.PayNum) + ", "
                      + "CreditCardNum     =  " + SOut.Long(recurringCharge.CreditCardNum) + ", "
                      + "ErrorMsg          =  " + DbHelper.ParamChar + "paramErrorMsg "
                      + "WHERE RecurringChargeNum = " + SOut.Long(recurringCharge.RecurringChargeNum);
        if (recurringCharge.ErrorMsg == null) recurringCharge.ErrorMsg = "";
        var paramErrorMsg = new OdSqlParameter("paramErrorMsg", OdDbType.Text, SOut.StringParam(recurringCharge.ErrorMsg));
        Db.NonQ(command, paramErrorMsg);
    }

    public static bool Update(RecurringCharge recurringCharge, RecurringCharge oldRecurringCharge)
    {
        var command = "";
        if (recurringCharge.PatNum != oldRecurringCharge.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(recurringCharge.PatNum) + "";
        }

        if (recurringCharge.ClinicNum != oldRecurringCharge.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(recurringCharge.ClinicNum) + "";
        }

        if (recurringCharge.DateTimeCharge != oldRecurringCharge.DateTimeCharge)
        {
            if (command != "") command += ",";
            command += "DateTimeCharge = " + SOut.DateT(recurringCharge.DateTimeCharge) + "";
        }

        if (recurringCharge.ChargeStatus != oldRecurringCharge.ChargeStatus)
        {
            if (command != "") command += ",";
            command += "ChargeStatus = " + SOut.Int((int) recurringCharge.ChargeStatus) + "";
        }

        if (recurringCharge.FamBal != oldRecurringCharge.FamBal)
        {
            if (command != "") command += ",";
            command += "FamBal = " + SOut.Double(recurringCharge.FamBal) + "";
        }

        if (recurringCharge.PayPlanDue != oldRecurringCharge.PayPlanDue)
        {
            if (command != "") command += ",";
            command += "PayPlanDue = " + SOut.Double(recurringCharge.PayPlanDue) + "";
        }

        if (recurringCharge.TotalDue != oldRecurringCharge.TotalDue)
        {
            if (command != "") command += ",";
            command += "TotalDue = " + SOut.Double(recurringCharge.TotalDue) + "";
        }

        if (recurringCharge.RepeatAmt != oldRecurringCharge.RepeatAmt)
        {
            if (command != "") command += ",";
            command += "RepeatAmt = " + SOut.Double(recurringCharge.RepeatAmt) + "";
        }

        if (recurringCharge.ChargeAmt != oldRecurringCharge.ChargeAmt)
        {
            if (command != "") command += ",";
            command += "ChargeAmt = " + SOut.Double(recurringCharge.ChargeAmt) + "";
        }

        if (recurringCharge.UserNum != oldRecurringCharge.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(recurringCharge.UserNum) + "";
        }

        if (recurringCharge.PayNum != oldRecurringCharge.PayNum)
        {
            if (command != "") command += ",";
            command += "PayNum = " + SOut.Long(recurringCharge.PayNum) + "";
        }

        if (recurringCharge.CreditCardNum != oldRecurringCharge.CreditCardNum)
        {
            if (command != "") command += ",";
            command += "CreditCardNum = " + SOut.Long(recurringCharge.CreditCardNum) + "";
        }

        if (recurringCharge.ErrorMsg != oldRecurringCharge.ErrorMsg)
        {
            if (command != "") command += ",";
            command += "ErrorMsg = " + DbHelper.ParamChar + "paramErrorMsg";
        }

        if (command == "") return false;
        if (recurringCharge.ErrorMsg == null) recurringCharge.ErrorMsg = "";
        var paramErrorMsg = new OdSqlParameter("paramErrorMsg", OdDbType.Text, SOut.StringParam(recurringCharge.ErrorMsg));
        command = "UPDATE recurringcharge SET " + command
                                                + " WHERE RecurringChargeNum = " + SOut.Long(recurringCharge.RecurringChargeNum);
        Db.NonQ(command, paramErrorMsg);
        return true;
    }

    public static bool UpdateComparison(RecurringCharge recurringCharge, RecurringCharge oldRecurringCharge)
    {
        if (recurringCharge.PatNum != oldRecurringCharge.PatNum) return true;
        if (recurringCharge.ClinicNum != oldRecurringCharge.ClinicNum) return true;
        if (recurringCharge.DateTimeCharge != oldRecurringCharge.DateTimeCharge) return true;
        if (recurringCharge.ChargeStatus != oldRecurringCharge.ChargeStatus) return true;
        if (recurringCharge.FamBal != oldRecurringCharge.FamBal) return true;
        if (recurringCharge.PayPlanDue != oldRecurringCharge.PayPlanDue) return true;
        if (recurringCharge.TotalDue != oldRecurringCharge.TotalDue) return true;
        if (recurringCharge.RepeatAmt != oldRecurringCharge.RepeatAmt) return true;
        if (recurringCharge.ChargeAmt != oldRecurringCharge.ChargeAmt) return true;
        if (recurringCharge.UserNum != oldRecurringCharge.UserNum) return true;
        if (recurringCharge.PayNum != oldRecurringCharge.PayNum) return true;
        if (recurringCharge.CreditCardNum != oldRecurringCharge.CreditCardNum) return true;
        if (recurringCharge.ErrorMsg != oldRecurringCharge.ErrorMsg) return true;
        return false;
    }

    public static void Delete(long recurringChargeNum)
    {
        var command = "DELETE FROM recurringcharge "
                      + "WHERE RecurringChargeNum = " + SOut.Long(recurringChargeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRecurringChargeNums)
    {
        if (listRecurringChargeNums == null || listRecurringChargeNums.Count == 0) return;
        var command = "DELETE FROM recurringcharge "
                      + "WHERE RecurringChargeNum IN(" + string.Join(",", listRecurringChargeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}