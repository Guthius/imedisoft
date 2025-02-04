using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Deposits
{
    public static List<Deposit> GetForClinics(List<long> clinicNums, bool isUnattached)
    {
        var commandText =
            "SELECT deposit.*, " +
            "(CASE COUNT(DISTINCT COALESCE(clinic.ClinicNum, 0)) " +
            "WHEN 1 THEN COALESCE(clinic.Abbr, '(None)') ELSE CONCAT('(', COUNT(DISTINCT COALESCE(clinic.ClinicNum, 0)), ')') END) AS ClinicAbbr " +
            "FROM deposit " +
            "INNER JOIN (SELECT DISTINCT DepositNum, ClinicNum " +
            "FROM payment WHERE DepositNum != 0 ";

        if (clinicNums.Count != 0)
        {
            commandText += "AND ClinicNum IN (" + string.Join(",", clinicNums) + ") ";
        }

        commandText +=
            "UNION SELECT DISTINCT DepositNum, ClinicNum " +
            "FROM claimpayment " +
            "INNER JOIN definition ON claimpayment.PayType = definition.DefNum AND definition.ItemValue = '' " +
            "WHERE DepositNum != 0 ";

        if (clinicNums.Count != 0)
        {
            commandText += "AND ClinicNum IN (" + string.Join(",", clinicNums) + ") ";
        }
        

        commandText +=
            ") PayInfo ON PayInfo.DepositNum = deposit.DepositNum " +
            "LEFT JOIN clinic ON clinic.ClinicNum = PayInfo.ClinicNum ";

        if (isUnattached)
        {
            commandText += "WHERE NOT EXISTS(SELECT * FROM transaction WHERE deposit.DepositNum=transaction.DepositNum) ";
        }

        commandText += "GROUP BY deposit.DepositNum ";
        commandText += "ORDER BY deposit.DateDeposit";

        var dataTable = DataCore.GetTable(commandText);

        var deposits = DepositCrud.TableToList(dataTable);
        for (var i = 0; i < deposits.Count; i++)
        {
            deposits[i].ClinicAbbr = SIn.String(dataTable.Rows[i]["ClinicAbbr"].ToString());
        }

        return deposits;
    }

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

    public static void Delete(Deposit deposit)
    {
        if (deposit.DepositNum == 0)
        {
            return;
        }

        if (SIn.Long(Db.GetCount("SELECT COUNT(*) FROM transaction WHERE DepositNum = " + deposit.DepositNum)) > 0)
        {
            throw new ApplicationException("Cannot delete deposit because it is attached to a transaction.");
        }

        Db.NonQ("UPDATE payment SET DepositNum = 0 WHERE DepositNum = " + deposit.DepositNum);
        Db.NonQ("UPDATE claimpayment SET DepositNum = 0 WHERE DepositNum = " + deposit.DepositNum);

        DepositCrud.Delete(deposit.DepositNum);
    }

    public static void DetachFromDeposit(long depositNum, List<long> payNums, List<long> claimPaymentNums)
    {
        if (payNums.Count > 0)
        {
            Db.NonQ("UPDATE payment SET DepositNum = 0 WHERE DepositNum = " + depositNum + " AND PayNum IN (" + string.Join(",", payNums) + ")");
        }

        if (claimPaymentNums.Count > 0)
        {
            Db.NonQ("UPDATE claimpayment SET DepositNum = 0 WHERE DepositNum = " + depositNum + " AND ClaimPaymentNum IN (" + string.Join(",", claimPaymentNums) + ")");
        }
    }
}