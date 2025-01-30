using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness.Misc;

namespace OpenDentBusiness;

public class PaySplits
{
    public static void Insert(PaySplit split)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        split.SecUserNumEntry = Security.CurUser.UserNum;
        split.SecurityHash = HashFields(split);
        PaySplitCrud.Insert(split);
    }

    public static PaySplit[] Refresh(long patNum)
    {
        /*This query was too slow
        string command=
            "SELECT DISTINCT paysplit.* FROM paysplit,payment "
            +"WHERE paysplit.PayNum=payment.PayNum "
            +"AND (paysplit.PatNum = '"+POut.Long(patNum)+"' OR payment.PatNum = '"+POut.Long(patNum)+"') "
            +"ORDER BY DatePay";*/
        //this query goes 10 times faster for very large databases
        var command = @"select DISTINCT paysplitunion.* FROM "
                      + "(SELECT DISTINCT paysplit.* FROM paysplit,payment "
                      + "WHERE paysplit.PayNum=payment.PayNum and payment.PatNum='" + SOut.Long(patNum) + "' "
                      + "UNION "
                      + "SELECT DISTINCT paysplit.* FROM paysplit,payment "
                      + "WHERE paysplit.PayNum = payment.PayNum AND paysplit.PatNum='" + SOut.Long(patNum) + "') paysplitunion "
                      + "ORDER BY paysplitunion.DatePay";
        return PaySplitCrud.SelectMany(command).ToArray();
    }

    public static List<PaySplit> GetPatientData(long patNum)
    {
        /*This query was too slow
        string command=
            "SELECT DISTINCT paysplit.* FROM paysplit,payment "
            +"WHERE paysplit.PayNum=payment.PayNum "
            +"AND (paysplit.PatNum = '"+POut.Long(patNum)+"' OR payment.PatNum = '"+POut.Long(patNum)+"') "
            +"ORDER BY DatePay";*/
        //this query goes 10 times faster for very large databases
        var command = @"select DISTINCT paysplitunion.* FROM "
                      + "(SELECT DISTINCT paysplit.* FROM paysplit,payment "
                      + "WHERE paysplit.PayNum=payment.PayNum and payment.PatNum='" + SOut.Long(patNum) + "' "
                      + "UNION "
                      + "SELECT DISTINCT paysplit.* FROM paysplit,payment " //Jordan-I think payment is not needed here
                      + "WHERE paysplit.PayNum = payment.PayNum AND paysplit.PatNum='" + SOut.Long(patNum) + "') paysplitunion "
                      + "ORDER BY paysplitunion.DatePay";
        return PaySplitCrud.SelectMany(command);
    }

    public static List<PaySplit> GetForAdjustments(List<long> listAdjustNums)
    {
        if (listAdjustNums == null || listAdjustNums.Count == 0) return new List<PaySplit>();

        var command = "SELECT * FROM paysplit WHERE AdjNum IN (" + string.Join(",", listAdjustNums) + ")";
        return PaySplitCrud.SelectMany(command);
    }

    public static List<PaySplit> GetForPayPlanCharges(List<long> listPayPlanChargeNums)
    {
        if (listPayPlanChargeNums.IsNullOrEmpty()) return new List<PaySplit>();

        var command = $"SELECT * FROM paysplit WHERE PayPlanChargeNum > 0 AND PayPlanChargeNum IN ({string.Join(",", listPayPlanChargeNums)})";
        return PaySplitCrud.SelectMany(command);
    }

    public static List<PaySplit> GetForPayment(long payNum)
    {
        var command =
            "SELECT * FROM paysplit "
            + "WHERE PayNum=" + SOut.Long(payNum);
        return PaySplitCrud.SelectMany(command);
    }

    public static List<PaySplit> GetForPayments(List<long> listPayNums)
    {
        if (listPayNums.IsNullOrEmpty()) return new List<PaySplit>();

        var command =
            "SELECT * FROM paysplit "
            + "WHERE PayNum IN(" + string.Join(",", listPayNums.Select(x => SOut.Long(x))) + ")";
        return PaySplitCrud.SelectMany(command);
    }

    public static List<PaySplit> GetForProcs(List<long> listProcNums)
    {
        if (listProcNums.IsNullOrEmpty()) return new List<PaySplit>();

        var command = $"SELECT * FROM paysplit WHERE paysplit.ProcNum IN ({string.Join(",", listProcNums)}) ";
        return PaySplitCrud.SelectMany(command);
    }

    public static void InsertMany(long payNum, List<PaySplit> listSplits)
    {
        foreach (var split in listSplits)
        {
            split.PayNum = payNum;
            split.SecurityHash = HashFields(split);
        }

        InsertMany(listSplits);
    }

    public static void InsertMany(List<PaySplit> listSplits)
    {
        if (listSplits.IsNullOrEmpty()) return;

        foreach (var split in listSplits)
        {
            //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
            split.SecUserNumEntry = Security.CurUser.UserNum;
            split.SecurityHash = HashFields(split);
        }

        PaySplitCrud.InsertMany(listSplits);
    }

    public static List<PaySplit> GetForProc(long procNum, PaySplit[] List)
    {
        var listPaySplits = new List<PaySplit>();
        for (var i = 0; i < List.Length; i++)
            if (List[i].ProcNum == procNum)
                listPaySplits.Add(List[i]);

        return listPaySplits;
    }

    public static double GetTotForProc(Procedure procCur)
    {
        return GetForProcs(ListTools.FromSingle(procCur.ProcNum)).Sum(x => x.SplitAmt);
    }
    
    public static double GetTotForProc(long procNum, PaySplit[] List, PaySplit paySplitToExclude, out int countSplitsAttached)
    {
        double retVal = 0;
        countSplitsAttached = 0;
        for (var i = 0; i < List.Length; i++)
        {
            if (List[i].IsSame(paySplitToExclude)) continue;
            if (List[i].ProcNum == procNum)
            {
                countSplitsAttached++;
                retVal += List[i].SplitAmt;
            }
        }

        return retVal;
    }

    public static List<PaySplit> GetForPats(List<long> listPatNums)
    {
        var command = "SELECT * FROM paysplit "
                      + "WHERE PatNum IN(" + string.Join(", ", listPatNums) + ")";
        return PaySplitCrud.SelectMany(command);
    }

    public static DataTable GetForPayPlan(long payPlanNum)
    {
        var command = "SELECT paysplit.*,payment.CheckNum,payment.PayAmt,payment.PayType "
                      + "FROM paysplit "
                      + "LEFT JOIN payment ON paysplit.PayNum=payment.PayNum "
                      + "WHERE paysplit.PayPlanNum=" + SOut.Long(payPlanNum) + " "
                      + "ORDER BY DatePay";
        var tableSplits = DataCore.GetTable(command);
        return tableSplits;
    }

    public static List<PaySplit> GetForPayPlans(List<long> listPayPlanNums)
    {
        if (listPayPlanNums.Count == 0) return new List<PaySplit>();

        var command = "SELECT paysplit.* "
                      + "FROM paysplit "
                      + "WHERE paysplit.PayPlanNum IN (" + SOut.String(string.Join(",", listPayPlanNums)) + ") "
                      + "ORDER BY DatePay";
        var listSplits = PaySplitCrud.SelectMany(command);
        return listSplits;
    }

    public static List<PaySplit> GetFromBundled(DataTable dataTable)
    {
        return PaySplitCrud.TableToList(dataTable);
    }

    public static List<PaySplit> GetUnearnedForAccount(List<long> listPatNums)
    {
        var command = "SELECT * FROM paysplit WHERE PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND UnearnedType!=0 "
                      + "UNION ALL "
                      + "SELECT paysplit.* FROM payment " //We use payment here so that we can filter the results based on payment.PatNum
                      + "INNER JOIN paysplit ON paysplit.PayNum=payment.PayNum "
                      + $"WHERE payment.PatNum IN ({string.Join(",", listPatNums)}) "
                      + $"AND paysplit.PatNum NOT IN ({string.Join(",", listPatNums)}) "
                      + "AND UnearnedType!=0 "
                      + "ORDER BY DatePay";
        return PaySplitCrud.SelectMany(command);
    }

    public static List<PaySplit> GetUnearnedForPats(List<long> listPatNums)
    {
        var listHiddenUnearnedTypes = GetHiddenUnearnedDefNums();
        var strHiddenUnearned = "";
        if (listHiddenUnearnedTypes.Count > 0) strHiddenUnearned = $"AND UnearnedType NOT IN({string.Join(",", listHiddenUnearnedTypes)})";
        var command = $@"SELECT * FROM paysplit
				WHERE PatNum IN ({string.Join(",", listPatNums)})
				AND UnearnedType!=0
				AND ProcNum=0
				{strHiddenUnearned}
				ORDER BY DatePay";
        return PaySplitCrud.SelectMany(command);
    }

    public static List<long> GetHiddenUnearnedDefNums()
    {
        return Defs.GetHiddenUnearnedDefs().Select(x => x.DefNum).ToList();
    }

    public static decimal GetTotalAmountOfUnearnedForPats(List<long> listPatNums, long payNumExcluded = 0)
    {
        var listUnearnedSplits = GetUnearnedForPats(listPatNums);
        //Remove any splits attached to the payment to exclude if one was passed in.
        if (payNumExcluded > 0) listUnearnedSplits.RemoveAll(x => x.PayNum == payNumExcluded);
        //At this point we know that the list of unearned splits contains all splits (negative and positive) that make up the unearned bucket.
        return (decimal) listUnearnedSplits.Sum(x => x.SplitAmt);
    }
    
    public static List<PaySplit> GetPaySplitsFromProc(long procNum, bool onlyUnearned = false)
    {
        return GetPaySplitsFromProcs(new List<long> {procNum}, onlyUnearned);
    }

    public static List<PaySplit> GetPaySplitsFromProcs(List<long> listProcNums, bool onlyUnearned = false)
    {
        if (listProcNums == null || listProcNums.Count < 1) return new List<PaySplit>();

        var command = "SELECT * FROM paysplit WHERE ProcNum IN(" + string.Join(",", listProcNums) + ")";
        if (onlyUnearned) command += " AND UnearnedType > 0";
        return PaySplitCrud.SelectMany(command);
    }

    public static void Update(PaySplit split)
    {
        if (IsPaySplitHashValid(split)) //Only rehash splits that are already valid
            split.SecurityHash = HashFields(split);
        PaySplitCrud.Update(split);
    }

    public static void UpdateAttachedPaySplits(Procedure proc)
    {
        Db.NonQ($@"UPDATE paysplit SET ProvNum = {SOut.Long(proc.ProvNum)} WHERE ProcNum = {SOut.Long(proc.ProcNum)}");
    }

    public static void UnlinkForAdjust(Adjustment adj)
    {
        Db.NonQ($@"UPDATE paysplit SET AdjNum = 0 WHERE AdjNum = {SOut.Long(adj.AdjNum)}");
    }

    public static void UpdateProvForAdjust(Adjustment adj, List<PaySplit> listSplits = null)
    {
        if (listSplits != null && listSplits.Count == 0) return;

        if (listSplits == null)
            Db.NonQ($@"UPDATE paysplit SET ProvNum = {SOut.Long(adj.ProvNum)} WHERE AdjNum = {SOut.Long(adj.AdjNum)}");
        else
            Db.NonQ($@"UPDATE paysplit SET ProvNum = {SOut.Long(adj.ProvNum)}
					WHERE SplitNum IN({string.Join(",", listSplits.Select(x => SOut.Long(x.SplitNum)))})");
    }

    public static bool Sync(List<PaySplit> listNew, long payNum)
    {
        var isHashNeeded = true;
        var listOld = GetForPayment(payNum);
        for (var i = 0; i < listNew.Count; i++)
        {
            isHashNeeded = true;
            //Only rehash existing splits that are already valid
            var paySplitOld = listOld.FirstOrDefault(x => listNew[i].SplitNum == x.SplitNum);
            if (paySplitOld != null) isHashNeeded = IsPaySplitHashValid(paySplitOld);
            //Hash splits that are either new, or rehash existing splits that are valid
            if (isHashNeeded) listNew[i].SecurityHash = HashFields(listNew[i]);
        }

        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        return PaySplitCrud.Sync(listNew, listOld, Security.CurUser.UserNum);
    }

    public static void DeleteMany(params long[] arraySplitNums)
    {
        if (arraySplitNums.IsNullOrEmpty()) return;

        var command = $"DELETE FROM paysplit WHERE SplitNum IN({string.Join(",", arraySplitNums.Select(x => SOut.Long(x)))})";
        Db.NonQ(command);
    }

    public static bool IsPaySplitAttached(long procNum)
    {
        var command = "SELECT COUNT(*) FROM paysplit WHERE ProcNum=" + SOut.Long(procNum);
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    public static string GetSecurityLogMsgDelete(PaySplit paySplit, Payment payment = null)
    {
        return $"Paysplit deleted for: {Patients.GetLim(paySplit.PatNum).GetNameLF()}, {paySplit.SplitAmt.ToString("c")}, with payment type "
               + $"'{Payments.GetPaymentTypeDesc(payment ?? Payments.GetPayment(paySplit.PayNum))}'";
    }

    public static string HashFields(PaySplit split)
    {
        var unhashedText = split.PatNum + split.SplitAmt.ToString("F2") + split.DatePay.ToString("yyyy-MM-dd");
        try
        {
            return Class1.CreateSaltedHash(unhashedText);
        }
        catch (Exception ex)
        {
            return ex.GetType().Name;
        }
    }

    public static bool IsPaySplitHashValid(PaySplit paySplit)
    {
        if (paySplit == null) return true;
        var dateHashStart = SecurityHash.GetHashingDate();
        if (paySplit.DatePay < dateHashStart) //old
            return true;
        if (paySplit.SecurityHash == HashFields(paySplit)) return true;
        return false;
    }
}