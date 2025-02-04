using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<RecurringCharge> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RecurringCharge> TableToList(DataTable table)
    {
        var retVal = new List<RecurringCharge>();
        foreach (DataRow row in table.Rows)
        {
            var recurringCharge = new RecurringCharge
            {
                RecurringChargeNum = SIn.Long(row["RecurringChargeNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                DateTimeCharge = SIn.DateTime(row["DateTimeCharge"].ToString()),
                ChargeStatus = (RecurringChargeStatus) SIn.Int(row["ChargeStatus"].ToString()),
                FamBal = SIn.Double(row["FamBal"].ToString()),
                PayPlanDue = SIn.Double(row["PayPlanDue"].ToString()),
                TotalDue = SIn.Double(row["TotalDue"].ToString()),
                RepeatAmt = SIn.Double(row["RepeatAmt"].ToString()),
                ChargeAmt = SIn.Double(row["ChargeAmt"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                PayNum = SIn.Long(row["PayNum"].ToString()),
                CreditCardNum = SIn.Long(row["CreditCardNum"].ToString()),
                ErrorMsg = SIn.String(row["ErrorMsg"].ToString())
            };
            retVal.Add(recurringCharge);
        }

        return retVal;
    }

    public static void Insert(RecurringCharge recurringCharge)
    {
        var command = "INSERT INTO recurringcharge (";

        command += "PatNum,ClinicNum,DateTimeCharge,ChargeStatus,FamBal,PayPlanDue,TotalDue,RepeatAmt,ChargeAmt,UserNum,PayNum,CreditCardNum,ErrorMsg) VALUES(";

        command +=
            SOut.Long(recurringCharge.PatNum) + ","
                                              + SOut.Long(recurringCharge.ClinicNum) + ","
                                              + SOut.DateTime(recurringCharge.DateTimeCharge) + ","
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
        var paramErrorMsg = new OdSqlParameter("paramErrorMsg", SOut.StringParam(recurringCharge.ErrorMsg));
        {
            recurringCharge.RecurringChargeNum = Db.NonQ(command, true, "RecurringChargeNum", "recurringCharge", paramErrorMsg);
        }
    }

    public static void Update(RecurringCharge recurringCharge)
    {
        var command = "UPDATE recurringcharge SET "
                      + "PatNum            =  " + SOut.Long(recurringCharge.PatNum) + ", "
                      + "ClinicNum         =  " + SOut.Long(recurringCharge.ClinicNum) + ", "
                      + "DateTimeCharge    =  " + SOut.DateTime(recurringCharge.DateTimeCharge) + ", "
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
        var paramErrorMsg = new OdSqlParameter("paramErrorMsg", SOut.StringParam(recurringCharge.ErrorMsg));
        Db.NonQ(command, paramErrorMsg);
    }

    public static void Delete(long recurringChargeNum)
    {
        var command = "DELETE FROM recurringcharge "
                      + "WHERE RecurringChargeNum = " + SOut.Long(recurringChargeNum);
        Db.NonQ(command);
    }
}