using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Deposits
{
    ///<summary>Gets all Deposits, ordered by DateDeposit, DepositNum.  </summary>
    public static List<Deposit> Refresh()
    {
        var command = "SELECT * FROM deposit "
                      + "ORDER BY DateDeposit";
        return DepositCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all Deposits, as well as the clinic(s) associated to the deposit.  The listClinicNums cannot be null.  If
    ///     listClinicNums is empty, then will return the deposits for all clinics.
    /// </summary>
    public static List<Deposit> GetForClinics(List<long> listClinicNums, bool isUnattached)
    {
        var command = "SELECT deposit.*,"
                      + "(CASE COUNT(DISTINCT COALESCE(clinic.ClinicNum,0)) WHEN 1 THEN COALESCE(clinic.Abbr,'(None)') ELSE CONCAT('(',COUNT(DISTINCT COALESCE(clinic.ClinicNum,0)),')') END) AS		ClinicAbbr "
                      + "FROM deposit "
                      + "INNER JOIN ( "
                      + "SELECT DISTINCT DepositNum,ClinicNum "
                      + "FROM payment "
                      + "WHERE DepositNum!=0 ";
        if (listClinicNums.Count != 0) command += "AND ClinicNum IN (" + string.Join(",", listClinicNums) + ") ";

        command += "UNION " //This will remove duplicates if any exist between the two tables being unioned.
                   + "SELECT DISTINCT DepositNum,ClinicNum "
                   + "FROM claimpayment "
                   + "INNER JOIN definition ON claimpayment.PayType=definition.DefNum AND definition.ItemValue='' "
                   + "WHERE DepositNum!=0 ";
        if (listClinicNums.Count != 0) command += "AND ClinicNum IN (" + string.Join(",", listClinicNums) + ") ";

        command += ") PayInfo ON PayInfo.DepositNum=deposit.DepositNum "
                   + "LEFT JOIN clinic ON clinic.ClinicNum=PayInfo.ClinicNum "; //LEFT JOIN since there may be some ClinicNum=0.
        if (isUnattached) command += "WHERE NOT EXISTS(SELECT * FROM transaction WHERE deposit.DepositNum=transaction.DepositNum) ";
        command += "GROUP BY deposit.DepositNum ";
        command += "ORDER BY deposit.DateDeposit";
        var table = DataCore.GetTable(command);
        var listDeposits = DepositCrud.TableToList(table);
        for (var i = 0; i < listDeposits.Count; i++) listDeposits[i].ClinicAbbr = SIn.String(table.Rows[i]["ClinicAbbr"].ToString());

        return listDeposits;
    }

    ///<summary>Gets only Deposits which are not attached to transactions.</summary>
    public static List<Deposit> GetUnattached()
    {
        var command = "SELECT * FROM deposit "
                      + "WHERE NOT EXISTS(SELECT * FROM transaction WHERE deposit.DepositNum=transaction.DepositNum) "
                      + "ORDER BY DateDeposit";
        return DepositCrud.SelectMany(command);
    }

    ///<summary>Gets a single deposit directly from the database.</summary>
    public static Deposit GetOne(long depositNum)
    {
        return DepositCrud.SelectOne(depositNum);
    }

    
    public static void Update(Deposit deposit)
    {
        DepositCrud.Update(deposit);
    }

    
    public static void Update(Deposit deposit, Deposit depositOld)
    {
        DepositCrud.Update(deposit, depositOld);
    }

    
    public static long Insert(Deposit deposit)
    {
        return DepositCrud.Insert(deposit);
    }

    /// <summary>
    ///     Returns without making any changes if dep.DepositNum==0.  Also handles detaching all payments and claimpayments.
    ///     Throws exception if
    ///     deposit is attached as a source document to a transaction.  The program should have detached the deposit from the
    ///     transaction ahead of time, so
    ///     I would never expect the program to throw this exception unless there was a bug.
    /// </summary>
    public static void Delete(Deposit deposit)
    {
        if (deposit.DepositNum == 0) return;

        //check dependencies
        var command = "SELECT COUNT(*) FROM transaction WHERE DepositNum =" + SOut.Long(deposit.DepositNum);
        if (SIn.Long(Db.GetCount(command)) > 0) throw new ApplicationException(Lans.g("Deposits", "Cannot delete deposit because it is attached to a transaction."));

        //ready to delete
        command = "UPDATE payment SET DepositNum=0 WHERE DepositNum=" + SOut.Long(deposit.DepositNum);
        Db.NonQ(command);
        command = "UPDATE claimpayment SET DepositNum=0 WHERE DepositNum=" + SOut.Long(deposit.DepositNum);
        Db.NonQ(command);
        DepositCrud.Delete(deposit.DepositNum);
    }

    ///<summary>Detach specific payments and claimpayments from passed in deposit.</summary>
    public static void DetachFromDeposit(long depositNum, List<long> listPayNums, List<long> listClaimPaymentNums)
    {
        var command = "";
        if (listPayNums.Count > 0)
        {
            command = "UPDATE payment SET DepositNum=0 WHERE DepositNum=" + SOut.Long(depositNum)
                                                                          + " AND PayNum IN(" + string.Join(",", listPayNums) + ")";
            Db.NonQ(command);
        }

        if (listClaimPaymentNums.Count > 0)
        {
            command = "UPDATE claimpayment SET DepositNum=0 WHERE DepositNum=" + SOut.Long(depositNum)
                                                                               + " AND ClaimPaymentNum IN(" + string.Join(",", listClaimPaymentNums) + ")";
            Db.NonQ(command);
        }
    }
}