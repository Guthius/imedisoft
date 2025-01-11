using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimPaymentCrud
{
    public static ClaimPayment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ClaimPayment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimPayment> TableToList(DataTable table)
    {
        var retVal = new List<ClaimPayment>();
        ClaimPayment claimPayment;
        foreach (DataRow row in table.Rows)
        {
            claimPayment = new ClaimPayment();
            claimPayment.ClaimPaymentNum = SIn.Long(row["ClaimPaymentNum"].ToString());
            claimPayment.CheckDate = SIn.Date(row["CheckDate"].ToString());
            claimPayment.CheckAmt = SIn.Double(row["CheckAmt"].ToString());
            claimPayment.CheckNum = SIn.String(row["CheckNum"].ToString());
            claimPayment.BankBranch = SIn.String(row["BankBranch"].ToString());
            claimPayment.Note = SIn.String(row["Note"].ToString());
            claimPayment.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            claimPayment.DepositNum = SIn.Long(row["DepositNum"].ToString());
            claimPayment.CarrierName = SIn.String(row["CarrierName"].ToString());
            claimPayment.DateIssued = SIn.Date(row["DateIssued"].ToString());
            claimPayment.IsPartial = SIn.Bool(row["IsPartial"].ToString());
            claimPayment.PayType = SIn.Long(row["PayType"].ToString());
            claimPayment.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            claimPayment.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            claimPayment.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            claimPayment.PayGroup = SIn.Long(row["PayGroup"].ToString());
            retVal.Add(claimPayment);
        }

        return retVal;
    }

    public static long Insert(ClaimPayment claimPayment)
    {
        var command = "INSERT INTO claimpayment (";

        command += "CheckDate,CheckAmt,CheckNum,BankBranch,Note,ClinicNum,DepositNum,CarrierName,DateIssued,IsPartial,PayType,SecUserNumEntry,SecDateEntry,PayGroup) VALUES(";

        command +=
            SOut.Date(claimPayment.CheckDate) + ","
                                              + SOut.Double(claimPayment.CheckAmt) + ","
                                              + "'" + SOut.String(claimPayment.CheckNum) + "',"
                                              + "'" + SOut.String(claimPayment.BankBranch) + "',"
                                              + "'" + SOut.String(claimPayment.Note) + "',"
                                              + SOut.Long(claimPayment.ClinicNum) + ","
                                              + SOut.Long(claimPayment.DepositNum) + ","
                                              + "'" + SOut.String(claimPayment.CarrierName) + "',"
                                              + SOut.Date(claimPayment.DateIssued) + ","
                                              + SOut.Bool(claimPayment.IsPartial) + ","
                                              + SOut.Long(claimPayment.PayType) + ","
                                              + SOut.Long(claimPayment.SecUserNumEntry) + ","
                                              + DbHelper.Now() + ","
                                              //SecDateTEdit can only be set by MySQL
                                              + SOut.Long(claimPayment.PayGroup) + ")";
        {
            claimPayment.ClaimPaymentNum = Db.NonQ(command, true, "ClaimPaymentNum", "claimPayment");
        }
        return claimPayment.ClaimPaymentNum;
    }

    public static void Update(ClaimPayment claimPayment)
    {
        var command = "UPDATE claimpayment SET "
                      + "CheckDate      =  " + SOut.Date(claimPayment.CheckDate) + ", "
                      + "CheckAmt       =  " + SOut.Double(claimPayment.CheckAmt) + ", "
                      + "CheckNum       = '" + SOut.String(claimPayment.CheckNum) + "', "
                      + "BankBranch     = '" + SOut.String(claimPayment.BankBranch) + "', "
                      + "Note           = '" + SOut.String(claimPayment.Note) + "', "
                      + "ClinicNum      =  " + SOut.Long(claimPayment.ClinicNum) + ", "
                      + "DepositNum     =  " + SOut.Long(claimPayment.DepositNum) + ", "
                      + "CarrierName    = '" + SOut.String(claimPayment.CarrierName) + "', "
                      + "DateIssued     =  " + SOut.Date(claimPayment.DateIssued) + ", "
                      + "IsPartial      =  " + SOut.Bool(claimPayment.IsPartial) + ", "
                      + "PayType        =  " + SOut.Long(claimPayment.PayType) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "PayGroup       =  " + SOut.Long(claimPayment.PayGroup) + " "
                      + "WHERE ClaimPaymentNum = " + SOut.Long(claimPayment.ClaimPaymentNum);
        Db.NonQ(command);
    }
}