using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ClaimPayments
{
    public static DataTable GetForClaim(long claimNum)
    {
        var table = new DataTable();
        table.Columns.Add("amount");
        table.Columns.Add("payType");
        table.Columns.Add("BankBranch");
        table.Columns.Add("ClaimPaymentNum");
        table.Columns.Add("checkDate");
        table.Columns.Add("CheckNum");
        table.Columns.Add("Note");
        var listDataRows = new List<DataRow>();
        var command = "SELECT BankBranch,claimpayment.ClaimPaymentNum,CheckNum,CheckDate,"
                      + "SUM(claimproc.InsPayAmt) amount,Note,PayType "
                      + "FROM claimpayment,claimproc "
                      + "WHERE claimpayment.ClaimPaymentNum = claimproc.ClaimPaymentNum "
                      + "AND claimproc.ClaimNum = '" + SOut.Long(claimNum) + "' "
                      + "GROUP BY claimpayment.ClaimPaymentNum, BankBranch, CheckDate, CheckNum, Note, PayType";
        var tableRaw = DataCore.GetTable(command);
        DateTime date;
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            var dataRow = table.NewRow();
            dataRow["amount"] = SIn.Double(tableRaw.Rows[i]["amount"].ToString()).ToString("F");
            dataRow["payType"] = Defs.GetName(DefCat.InsurancePaymentType, SIn.Long(tableRaw.Rows[i]["PayType"].ToString()));
            dataRow["BankBranch"] = tableRaw.Rows[i]["BankBranch"].ToString();
            dataRow["ClaimPaymentNum"] = tableRaw.Rows[i]["ClaimPaymentNum"].ToString();
            date = SIn.Date(tableRaw.Rows[i]["CheckDate"].ToString());
            dataRow["checkDate"] = date.ToShortDateString();
            dataRow["CheckNum"] = tableRaw.Rows[i]["CheckNum"].ToString();
            dataRow["Note"] = tableRaw.Rows[i]["Note"].ToString();
            table.Rows.Add(dataRow);
        }

        return table;
    }

    /// <summary>
    ///     Gets all claimpayments of the specified claimpayment type, within the specified date range and from the specified
    ///     clinic.
    ///     0 for clinics means all clinics, 0 for claimpaytype means all types.
    /// </summary>
    public static DataTable GetForDateRange(DateTime dateFrom, DateTime dateTo, long clinicNum, long claimpayGroup)
    {
        var command = "SELECT claimpayment.*,"
                      + "(CASE WHEN (SELECT COUNT(*) FROM eobattach WHERE eobattach.ClaimPaymentNum=claimpayment.ClaimPaymentNum)>0 THEN 1 ELSE 0 END) hasEobAttach "
                      + "FROM claimpayment "
                      + "WHERE CheckDate >= " + SOut.Date(dateFrom) + " "
                      + "AND CheckDate <= " + SOut.Date(dateTo) + " ";
        if (clinicNum != 0) command += "AND ClinicNum=" + SOut.Long(clinicNum) + " ";

        if (claimpayGroup != 0) command += "AND PayGroup=" + SOut.Long(claimpayGroup) + " ";

        command += "ORDER BY CheckDate";
        return DataCore.GetTable(command);
    }

    public static List<ClaimPayment> GetClaimPaymentsWithEobAttach(List<long> listClaimPaymentNums)
    {
        if (listClaimPaymentNums.Count == 0) return new List<ClaimPayment>();

        var command = "SELECT * "
                      + "FROM claimpayment "
                      + "INNER JOIN eobattach ON eobattach.ClaimPaymentNum=claimpayment.ClaimPaymentNum "
                      + "WHERE claimpayment.ClaimPaymentNum IN(" + string.Join(",", listClaimPaymentNums.Select(x => SOut.Long(x))) + ") "
                      + "GROUP BY claimpayment.ClaimPaymentNum";
        return ClaimPaymentCrud.SelectMany(command);
    }

    public static List<ClaimPayment> GetByClaimPaymentNums(List<long> listClaimPaymentNums)
    {
        if (listClaimPaymentNums.Count == 0) return new List<ClaimPayment>();

        var command = "SELECT * "
                      + "FROM claimpayment "
                      + "WHERE ClaimPaymentNum IN(" + string.Join(",", listClaimPaymentNums.Select(x => SOut.Long(x))) + ")";
        return ClaimPaymentCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all unattached claimpayments for display in a new deposit.  Excludes payments before dateStart and
    ///     partials.
    /// </summary>
    public static List<ClaimPayment> GetForDeposit(DateTime dateStart, long clinicNum, List<long> listPayTypes)
    {
        var command =
            "SELECT * FROM claimpayment "
            + "INNER JOIN definition ON claimpayment.PayType=definition.DefNum "
            + "WHERE DepositNum = 0 "
            + "AND definition.ItemValue='' " //Check if payment type should show in the deposit slip.  'N'=not show, empty string means should show.
            + "AND CheckDate >= " + SOut.Date(dateStart);
        if (clinicNum != 0) command += " AND ClinicNum=" + SOut.Long(clinicNum);

        for (var i = 0; i < listPayTypes.Count; i++)
        {
            if (i == 0)
                command += " AND PayType IN (" + listPayTypes[0];
            else
                command += "," + listPayTypes[i];

            if (i == listPayTypes.Count - 1) command += ")";
        }

        command += " AND IsPartial=0" //Don't let users attach partial insurance payments to deposits.
                   + " AND CheckAmt!=0" //Users kept complaining about zero dollar insurance payments showing up, so don't show them.
                   //Order by the date on the check, and then the incremental order of the creation of each payment (doesn't affect random primary keys).
                   //It was an internal complaint that checks on the same date show up in a 'random' order.
                   //The real fix for this issue would be to add a time column and order by it by that instead of the PK.
                   + " ORDER BY CheckDate,ClaimPaymentNum"; //Not usual pattern to order by PK
        return ClaimPaymentCrud.SelectMany(command);
    }

    ///<summary>Gets all claimpayments for one specific deposit.</summary>
    public static ClaimPayment[] GetForDeposit(long depositNum)
    {
        var command =
            "SELECT * FROM claimpayment "
            + "INNER JOIN definition ON claimpayment.PayType=definition.DefNum "
            + "WHERE DepositNum = " + SOut.Long(depositNum)
            + " AND definition.ItemValue=''" //Check if payment type should show in the deposit slip.  'N'=not show, empty string means should show.
            //Order by the date on the check, and then the incremental order of the creation of each payment (doesn't affect random primary keys).
            //It was an internal complaint that checks on the same date show up in a 'random' order.
            //The real fix for this issue would be to add a time column and order by it by that instead of the PK.
            + " ORDER BY CheckDate,ClaimPaymentNum"; //Not usual pattern to order by PK
        return ClaimPaymentCrud.SelectMany(command).ToArray();
    }

    ///<summary>Gets one claimpayment directly from database.</summary>
    public static ClaimPayment GetOne(long claimPaymentNum)
    {
        var command =
            "SELECT * FROM claimpayment "
            + "WHERE ClaimPaymentNum = " + SOut.Long(claimPaymentNum);
        return ClaimPaymentCrud.SelectOne(command);
    }

    
    public static long Insert(ClaimPayment claimPayment)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        claimPayment.SecUserNumEntry = Security.CurUser.UserNum;
        return ClaimPaymentCrud.Insert(claimPayment);
    }

    ///<summary>If trying to change the amount and attached to a deposit, it will throw an error, so surround with try catch.</summary>
    public static void Update(ClaimPayment claimPayment, bool isDepNew = false)
    {
        if (!isDepNew && claimPayment.DepositNum != 0 && PrefC.GetBool(PrefName.ShowAutoDeposit))
        {
            var cmd = "SELECT deposit.Amount,SUM(COALESCE(claimpayment.CheckAmt,0))+SUM(COALESCE(payment.PayAmt,0)) depAmtOthers "
                      + "FROM deposit "
                      + "LEFT JOIN payment ON payment.DepositNum=deposit.DepositNum "
                      + "LEFT JOIN claimpayment ON claimpayment.DepositNum=deposit.DepositNum AND claimpayment.ClaimPaymentNum!=" + SOut.Long(claimPayment.ClaimPaymentNum) + " "
                      + "WHERE deposit.DepositNum=" + SOut.Long(claimPayment.DepositNum);
            var table = DataCore.GetTable(cmd);
            if (table.Rows.Count == 0)
                claimPayment.DepositNum = 0;
            else if (SIn.Double(table.Rows[0]["depAmtOthers"].ToString()) + claimPayment.CheckAmt != SIn.Double(table.Rows[0]["Amount"].ToString())) throw new ApplicationException(Lans.g("ClaimPayments", "Not allowed to change the amount on checks attached to deposits."));
        }
        else
        {
            var command = "SELECT DepositNum,CheckAmt FROM claimpayment "
                          + "WHERE ClaimPaymentNum=" + SOut.Long(claimPayment.ClaimPaymentNum);
            var table = DataCore.GetTable(command);
            if (table.Rows.Count == 0) return;

            if (table.Rows[0][0].ToString() != "0" //if claimpayment is already attached to a deposit
                && SIn.Double(table.Rows[0][1].ToString()) != claimPayment.CheckAmt) //and checkAmt changes
                throw new ApplicationException(Lans.g("ClaimPayments", "Not allowed to change the amount on checks attached to deposits."));
        }

        ClaimPaymentCrud.Update(claimPayment);
    }

    /// <summary>
    ///     Surround by try catch, because it will throw an exception if trying to delete a claimpayment attached to a
    ///     deposit or if there are eobs attached.
    /// </summary>
    public static void Delete(ClaimPayment claimPayment)
    {
        //validate deposits
        var command = "SELECT DepositNum FROM claimpayment "
                      + "WHERE ClaimPaymentNum=" + SOut.Long(claimPayment.ClaimPaymentNum);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return;

        if (table.Rows[0][0].ToString() != "0" && !HasAutoDeposit(claimPayment))
            //if claimpayment is already attached to a deposit and was not created automatically
            if (!ODBuild.IsDebug())
                throw new ApplicationException(Lans.g("ClaimPayments", "Not allowed to delete a payment attached to a deposit."));

        //validate eobs
        command = "SELECT COUNT(*) FROM eobattach WHERE ClaimPaymentNum=" + SOut.Long(claimPayment.ClaimPaymentNum);
        if (DataCore.GetScalar(command) != "0") throw new ApplicationException(Lans.g("ClaimPayments", "Not allowed to delete this payment because EOBs are attached."));

        if (table.Rows[0][0].ToString() != "0")
        {
            //deposit was created automatically. Delete deposit.
            var deposit = Deposits.GetOne(claimPayment.DepositNum);
            if (deposit != null) Deposits.Delete(deposit);
        }

        command = "UPDATE claimproc SET "
                  + "DateInsFinalized='0001-01-01' "
                  + "WHERE ClaimPaymentNum=" + SOut.Long(claimPayment.ClaimPaymentNum) + " "
                  + "AND (SELECT SecDateEntry FROM claimpayment WHERE ClaimPaymentNum=" + SOut.Long(claimPayment.ClaimPaymentNum) + ")=CURDATE()";
        Db.NonQ(command);
        command = "UPDATE claimproc SET "
                  + "ClaimPaymentNum=0 "
                  + "WHERE claimpaymentNum=" + SOut.Long(claimPayment.ClaimPaymentNum);
        //MessageBox.Show(string command);
        Db.NonQ(command);
        command = "DELETE FROM claimpayment "
                  + "WHERE ClaimPaymentnum =" + SOut.Long(claimPayment.ClaimPaymentNum);
        //MessageBox.Show(string command);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Returns the number of payments from the passed in claimpaymentnums that are attached to a deposit other than
    ///     IgnoreDepositNum.
    /// </summary>
    public static int GetCountAttachedToDeposit(List<long> listClaimPaymentNums, long ignoreDepositNum)
    {
        if (listClaimPaymentNums.Count == 0) return 0;

        var command = "";
        command = "SELECT COUNT(*) FROM claimpayment WHERE ClaimPaymentNum IN(" + string.Join(",", listClaimPaymentNums) + ") AND DepositNum!=0";
        if (ignoreDepositNum != 0) command += " AND DepositNum!=" + SOut.Long(ignoreDepositNum);

        return SIn.Int(Db.GetCount(command));
    }

    public static bool HasAutoDeposit(ClaimPayment claimPayment)
    {
        if (claimPayment == null || claimPayment.DepositNum == 0 || !PrefC.GetBool(PrefName.ShowAutoDeposit)) return false;

        //Per Mark on 07/16/2018
        //A deposit is consided an "Auto Deposit" if the ShowAutoDeposit preference is turned on
        //and only one claimpayment is attached to the deposit passed in. 
        var command = "SELECT COUNT(*) FROM claimpayment where DepositNum=" + SOut.Long(claimPayment.DepositNum);
        return SIn.Int(Db.GetCount(command)) == 1;
    }
}