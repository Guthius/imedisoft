using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class XChargeTransactions
{
    
    public static long Insert(XChargeTransaction xChargeTransaction)
    {
        return XChargeTransactionCrud.Insert(xChargeTransaction);
    }

    ///<summary>Gets one XChargeTransaction from the db that matches the given fields.</summary>
    public static XChargeTransaction GetOneMatch(string batchNum, string itemNum, long patNum, DateTime dateTTransaction, string transType)
    {
        var command = "SELECT * FROM xchargetransaction WHERE BatchNum = '" + SOut.String(batchNum) + "' AND ItemNum = '" + SOut.String(itemNum) + "' "
                      + "AND PatNum=" + SOut.Long(patNum) + " AND TransType='" + SOut.String(transType) + "' "
                      //We include transactions that are the same minute because we used to not store the seconds portion.
                      + "AND TransactionDateTime BETWEEN " + SOut.DateT(DateTools.ToBeginningOfMinute(dateTTransaction)) + " AND " + SOut.DateT(DateTools.ToEndOfMinute(dateTTransaction));
        return XChargeTransactionCrud.SelectOne(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<XChargeTransaction> Refresh(long patNum){

        string command="SELECT * FROM xchargetransaction WHERE PatNum = "+POut.Long(patNum);
        return Crud.XChargeTransactionCrud.SelectMany(command);
    }

    
    public static void Update(XChargeTransaction xChargeTransaction){

        Crud.XChargeTransactionCrud.Update(xChargeTransaction);
    }
*/
    
    public static void Delete(long xChargeTransactionNum)
    {
        var command = "DELETE FROM xchargetransaction WHERE XChargeTransactionNum = " + SOut.Long(xChargeTransactionNum);
        Db.NonQ(command);
    }

    public static DataTable GetMissingPaymentsTable(DateTime dateStart, DateTime dateEnd)
    {
        var command = "SELECT xchargetransaction.* "
                      + "FROM xchargetransaction "
                      + "WHERE " + DbHelper.BetweenDates("TransactionDateTime", dateStart, dateEnd) + " "
                      + "AND xchargetransaction.ResultCode=0"; //Valid entries to count have result code 0
        var listXChargeTransactions = XChargeTransactionCrud.SelectMany(command);
        command = "SELECT payment.* "
                  + "FROM payment "
                  //only payments with the same PaymentType as the X-Charge PaymentType for the clinic
                  + "INNER JOIN ("
                  + "SELECT ClinicNum,PropertyValue PaymentType FROM programproperty "
                  + "WHERE ProgramNum=" + SOut.Long(Programs.GetProgramNum(ProgramName.Xcharge)) + " AND PropertyDesc='PaymentType'"
                  + ") paytypes ON paytypes.ClinicNum=payment.ClinicNum AND paytypes.PaymentType=payment.PayType "
                  + "WHERE DateEntry BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd);
        var listPayments = PaymentCrud.SelectMany(command);
        for (var i = listXChargeTransactions.Count - 1; i >= 0; i--)
        {
            //Looping backwards in order to remove items
            var xChargeTransaction = listXChargeTransactions[i];
            var payment = listPayments.Where(x => x.PatNum == xChargeTransaction.PatNum)
                .Where(x => x.DateEntry.Date == xChargeTransaction.TransactionDateTime.Date)
                .Where(x => x.PayAmt.Equals(xChargeTransaction.Amount))
                .FirstOrDefault();
            if (payment == null)
                //The XCharge transaction does not have a corresponding payment.
                continue;

            listXChargeTransactions.RemoveAt(i);
            listPayments.Remove(payment); //So that the same payment does not get counted for more than one XCharge transaction.
        }

        var table = XChargeTransactionCrud.ListToTable(listXChargeTransactions);
        var listColumnsToKeep = new List<string>
        {
            "TransactionDateTime", "TransType", "ClerkID", "ItemNum", "PatNum", "CreditCardNum", "Expiration", "Result", "Amount", "BatchTotal"
        };
        //Remove columns we don't want.
        for (var i = table.Columns.Count - 1; i >= 0; i--)
        {
            if (listColumnsToKeep.Contains(table.Columns[i].ColumnName)) continue;

            table.Columns.RemoveAt(i);
        }

        //Reorder the column in the order we want them.
        for (var i = 0; i < listColumnsToKeep.Count; i++) table.Columns[listColumnsToKeep[i]].SetOrdinal(i);

        return table;
    }

    public static DataTable GetMissingXTransTable(DateTime dateStart, DateTime dateEnd)
    {
        var command = "SELECT payment.PatNum,LName,FName,payment.DateEntry,payment.PayDate,payment.PayNote,payment.PayAmt "
                      + "FROM patient "
                      + "INNER JOIN payment ON payment.PatNum=patient.PatNum "
                      //only payments with the same PaymentType as the X-Charge PaymentType for the clinic
                      + "INNER JOIN ("
                      + "SELECT ClinicNum,PropertyValue AS PaymentType FROM programproperty "
                      + "WHERE ProgramNum=" + SOut.Long(Programs.GetProgramNum(ProgramName.Xcharge)) + " AND PropertyDesc='PaymentType'"
                      + ") paytypes ON paytypes.ClinicNum=payment.ClinicNum AND paytypes.PaymentType=payment.PayType "
                      + "LEFT JOIN xchargetransaction ON xchargetransaction.PatNum=payment.PatNum "
                      + "AND " + DbHelper.DtimeToDate("TransactionDateTime") + "=payment.DateEntry "
                      + "AND (CASE WHEN xchargetransaction.ResultCode=5 THEN 0 ELSE xchargetransaction.Amount END)=payment.PayAmt "
                      + "AND xchargetransaction.ResultCode IN(0,5,10) "
                      + "WHERE payment.DateEntry BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                      + "AND TransactionDateTime IS NULL "
                      + "ORDER BY payment.PayDate ASC,LName,FName";
        return DataCore.GetTable(command);
    }

    public static DataTable GetXChargeTransactionValidateBatchData(DateTime dateStart, DateTime dateEnd)
    {
        var command =
            $@"SELECT 
						BatchNum,
						ROUND(SUM(IF(ResultCode = '000' OR ResultCode = '010', Amount,0)),2) Summed,
						SUBSTRING_INDEX(GROUP_CONCAT(BatchTotal ORDER BY TransActionDateTime DESC),',',1) Total,
						ROUND(SUBSTRING_INDEX(GROUP_CONCAT(BatchTotal ORDER BY TransActionDateTime DESC),',',1) - ROUND(SUM(IF(ResultCode = '000' OR ResultCode = '010', Amount,0)),2),2) Difference
					FROM xchargetransaction
					WHERE BatchNum != 0
					AND TransactionDateTime BETWEEN {SOut.Date(dateStart)} AND {SOut.Date(dateEnd)}
					GROUP BY BatchNum";
        return DataCore.GetTable(command);
    }
}