using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CDT;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.Misc;

namespace OpenDentBusiness;


public class PaySplits
{
    #region Insert

    
    public static long Insert(PaySplit split)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        split.SecUserNumEntry = Security.CurUser.UserNum;
        split.SecurityHash = HashFields(split);
        return PaySplitCrud.Insert(split);
    }

    #endregion

    #region Get Methods

    /// <summary>
    ///     Returns all paySplits for the given patNum, organized by DatePay.  WARNING! Also includes related paysplits
    ///     that aren't actually attached to patient.  Includes any split where payment is for this patient.
    /// </summary>
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

    /// <summary>
    ///     Returns all paySplits for the given patNum, organized by DatePay.  WARNING! Also includes related paysplits
    ///     that aren't actually attached to patient.  Includes any split where payment is for this patient.
    /// </summary>
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

    ///<summary>Returns a list of paysplits that have AdjNum of any of the passed in adjustments.</summary>
    public static List<PaySplit> GetForAdjustments(List<long> listAdjustNums)
    {
        if (listAdjustNums == null || listAdjustNums.Count == 0) return new List<PaySplit>();

        var command = "SELECT * FROM paysplit WHERE AdjNum IN (" + string.Join(",", listAdjustNums) + ")";
        return PaySplitCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns all payment splits associated with the payment plan charges provided. Ignores payment splits with
    ///     PayPlanChargeNum of 0.
    /// </summary>
    public static List<PaySplit> GetForPayPlanCharges(List<long> listPayPlanChargeNums)
    {
        if (listPayPlanChargeNums.IsNullOrEmpty()) return new List<PaySplit>();

        var command = $"SELECT * FROM paysplit WHERE PayPlanChargeNum > 0 AND PayPlanChargeNum IN ({string.Join(",", listPayPlanChargeNums)})";
        return PaySplitCrud.SelectMany(command);
    }

    ///<summary>Used from payment window to get all paysplits for the payment.</summary>
    public static List<PaySplit> GetForPayment(long payNum)
    {
        var command =
            "SELECT * FROM paysplit "
            + "WHERE PayNum=" + SOut.Long(payNum);
        return PaySplitCrud.SelectMany(command);
    }

    /// <summary>
    ///     Used by the ODApi, acquires a list of PaySplits from the DB based on a payNum and/or patNum.
    ///     Returns null if nothing is found.
    /// </summary>
    public static List<PaySplit> GetPaySplitsForApi(long payNum, long patNum, int limit, int offset)
    {
        var command = "SELECT * FROM paysplit WHERE SecDateTEdit>=" + SOut.DateT(DateTime.MinValue) + " ";
        if (payNum != 0) command += "AND PayNum=" + SOut.Long(payNum) + " ";
        if (patNum != 0) command += "AND PatNum=" + SOut.Long(patNum) + " ";
        command += "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return PaySplitCrud.SelectMany(command);
    }

    ///<summary>Gets the splits for all the payments passed in.</summary>
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

    ///<summary>Inserts all paysplits with the provided payNum. All paysplits should be for the same payment. </summary>
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

    ///<summary>Gets one paysplit using the specified SplitNum.</summary>
    public static PaySplit GetOne(long splitNum)
    {
        var command = "SELECT * FROM paysplit WHERE SplitNum=" + SOut.Long(splitNum);
        return PaySplitCrud.SelectOne(command);
    }

    ///<summary>Used from FormPayment to return the total payments for a procedure without requiring a supplied list.</summary>
    public static string GetTotForProc(long procNum)
    {
        var command = "SELECT SUM(paysplit.SplitAmt) FROM paysplit "
                      + "WHERE paysplit.ProcNum=" + SOut.Long(procNum);
        return DataCore.GetScalar(command);
    }

    ///<summary>Returns all paySplits for the given procNum. Must supply a list of all paysplits for the patient.</summary>
    public static List<PaySplit> GetForProc(long procNum, PaySplit[] List)
    {
        var listPaySplits = new List<PaySplit>();
        for (var i = 0; i < List.Length; i++)
            if (List[i].ProcNum == procNum)
                listPaySplits.Add(List[i]);

        return listPaySplits;
    }

    ///<summary>Used from FormAdjust to display and calculate payments for procs attached to adjustments.</summary>
    public static double GetTotForProc(Procedure procCur)
    {
        return GetForProcs(ListTools.FromSingle(procCur.ProcNum)).Sum(x => x.SplitAmt);
    }

    /// <summary>
    ///     Used from FormPaySplitEdit.  Returns total payments for a procedure for all paysplits other than the supplied
    ///     excluded paysplit.
    /// </summary>
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

    /// <summary>
    ///     Used once in ContrAccount.  WARNING!  The returned list of 'paysplits' are not real paysplits.  They are
    ///     actually grouped by patient and date.  Only the DateEntry, SplitAmt, PatNum, and ProcNum(one of many) are filled.
    ///     Must supply a list which would include all paysplits for this payment.
    /// </summary>
    public static ArrayList GetGroupedForPayment(long payNum, PaySplit[] List)
    {
        var retVal = new ArrayList();
        int matchI;
        for (var i = 0; i < List.Length; i++)
            if (List[i].PayNum == payNum)
            {
                //find a 'paysplit' with matching DateEntry and patnum
                matchI = -1;
                for (var j = 0; j < retVal.Count; j++)
                    if (((PaySplit) retVal[j]).DateEntry == List[i].DateEntry && ((PaySplit) retVal[j]).PatNum == List[i].PatNum)
                    {
                        matchI = j;
                        break;
                    }

                if (matchI == -1)
                {
                    retVal.Add(new PaySplit());
                    matchI = retVal.Count - 1;
                    ((PaySplit) retVal[matchI]).DateEntry = List[i].DateEntry;
                    ((PaySplit) retVal[matchI]).PatNum = List[i].PatNum;
                }

                if (((PaySplit) retVal[matchI]).ProcNum == 0 && List[i].ProcNum != 0) ((PaySplit) retVal[matchI]).ProcNum = List[i].ProcNum;
                ((PaySplit) retVal[matchI]).SplitAmt += List[i].SplitAmt;
            }

        return retVal;
    }

    ///<summary>Used in Payment window to get all paysplits for a single patient without using a supplied list.</summary>
    public static List<PaySplit> GetForPats(List<long> listPatNums)
    {
        var command = "SELECT * FROM paysplit "
                      + "WHERE PatNum IN(" + string.Join(", ", listPatNums) + ")";
        return PaySplitCrud.SelectMany(command);
    }

    /// <summary>
    ///     Used once in ContrAccount to just get the splits for a single patient.  The supplied list also contains splits
    ///     that are not necessarily for this one patient.
    /// </summary>
    public static PaySplit[] GetForPatient(long patNum, PaySplit[] List)
    {
        var retVal = new ArrayList();
        for (var i = 0; i < List.Length; i++)
            if (List[i].PatNum == patNum)
                retVal.Add(List[i]);

        var retList = new PaySplit[retVal.Count];
        retVal.CopyTo(retList);
        return retList;
    }

    /// <summary>
    ///     For a given PayPlan, returns a table of PaySplits with additional payment information.
    ///     The additional information from the payment table will be columns titled "CheckNum", "PayAmt", and "PayType"
    /// </summary>
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

    ///<summary>For a given PayPlan, returns a list of PaySplits associated to that PayPlan.</summary>
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

    /// <summary>
    ///     Gets paysplits from a provided datatable.  This was originally part of GetForPayPlan but can't be because it's
    ///     passed through the Middle Tier.
    /// </summary>
    public static List<PaySplit> GetFromBundled(DataTable dataTable)
    {
        return PaySplitCrud.TableToList(dataTable);
    }

    ///<summary>Used once in ContrAccount.  Usually returns 0 unless there is a payplan for this payment and patient.</summary>
    public static long GetPayPlanNum(long payNum, long patNum, PaySplit[] List)
    {
        for (var i = 0; i < List.Length; i++)
            if (List[i].PayNum == payNum && List[i].PatNum == patNum && List[i].PayPlanNum != 0)
                return List[i].PayPlanNum;

        return 0;
    }

    /// <summary>
    ///     Returns every unearned PaySplit associated to the patients passed in, including TP prepayments.
    ///     This method can include some PaySplits that aren't directly associated to the patients passed in.
    ///     These PaySplits will be splits that are indirectly associated to the patients passed in via the payment.
    ///     E.g. a PatNum passed in is the PatNum on a payment but the payment has no PaySplits associated to the PatNum on the
    ///     payment.
    ///     These are payments made to another family / patient in the database. The Account module needs to know about these
    ///     splits.
    /// </summary>
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

    /// <summary>
    ///     Returns unearned PaySplits associated to the patients that contribute to the unearned bucket.
    ///     Hidden unearned types are ignored as well as any unearned PaySplit attached to a procedure (TP prepayments).
    /// </summary>
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

    /// <summary>Gets a list of all unearned types that are marked as hidden on account.</summary>
    public static List<long> GetHiddenUnearnedDefNums()
    {
        return Defs.GetHiddenUnearnedDefs().Select(x => x.DefNum).ToList();
    }

    ///<summary>Returns the total amount of unearned for the patients. Provide a payNumExcluded to ignore a specific payment.</summary>
    public static decimal GetTotalAmountOfUnearnedForPats(List<long> listPatNums, long payNumExcluded = 0)
    {
        var listUnearnedSplits = GetUnearnedForPats(listPatNums);
        //Remove any splits attached to the payment to exclude if one was passed in.
        if (payNumExcluded > 0) listUnearnedSplits.RemoveAll(x => x.PayNum == payNumExcluded);
        //At this point we know that the list of unearned splits contains all splits (negative and positive) that make up the unearned bucket.
        return (decimal) listUnearnedSplits.Sum(x => x.SplitAmt);
    }

    /// <summary>
    ///     Takes a procNum and returns a list of all paysplits associated to the procedure. Returns an empty list if
    ///     there are none.
    /// </summary>
    public static List<PaySplit> GetPaySplitsFromProc(long procNum, bool onlyUnearned = false)
    {
        return GetPaySplitsFromProcs(new List<long> {procNum}, onlyUnearned);
    }

    /// <summary>
    ///     Takes a list of procNums and returns a list of all paysplits associated to the procedures. Returns an empty
    ///     list if there are none.
    /// </summary>
    public static List<PaySplit> GetPaySplitsFromProcs(List<long> listProcNums, bool onlyUnearned = false)
    {
        if (listProcNums == null || listProcNums.Count < 1) return new List<PaySplit>();

        var command = "SELECT * FROM paysplit WHERE ProcNum IN(" + string.Join(",", listProcNums) + ")";
        if (onlyUnearned) command += " AND UnearnedType > 0";
        return PaySplitCrud.SelectMany(command);
    }

    #endregion

    #region Update

    
    public static void Update(PaySplit split)
    {
        if (IsPaySplitHashValid(split)) //Only rehash splits that are already valid
            split.SecurityHash = HashFields(split);
        PaySplitCrud.Update(split);
    }

    
    public static void Update(PaySplit paySplit, PaySplit oldPaySplit)
    {
        if (IsPaySplitHashValid(oldPaySplit)) //Only rehash splits that are already valid
            paySplit.SecurityHash = HashFields(paySplit);
        PaySplitCrud.Update(paySplit, oldPaySplit);
    }

    /// <summary>
    ///     Takes a procedure and updates the provnum of each of the paysplits attached.
    ///     Does nothing if there are no paysplits attached to the passed-in procedure.
    /// </summary>
    public static void UpdateAttachedPaySplits(Procedure proc)
    {
        Db.NonQ($@"UPDATE paysplit SET ProvNum = {SOut.Long(proc.ProvNum)} WHERE ProcNum = {SOut.Long(proc.ProcNum)}");
    }

    ///<summary>Unlinks all paysplits that are currently linked to the passed-in adjustment. (Sets paysplit.AdjNum to 0)</summary>
    public static void UnlinkForAdjust(Adjustment adj)
    {
        Db.NonQ($@"UPDATE paysplit SET AdjNum = 0 WHERE AdjNum = {SOut.Long(adj.AdjNum)}");
    }

    /// <summary>
    ///     Updates the provnum of all paysplits for a supplied adjustment.  Supply a list of splits to use that instead
    ///     of querying the database.
    /// </summary>
    public static void UpdateProvForAdjust(Adjustment adj, List<PaySplit> listSplits = null)
    {
        if (listSplits != null && listSplits.Count == 0) return;

        if (listSplits == null)
            Db.NonQ($@"UPDATE paysplit SET ProvNum = {SOut.Long(adj.ProvNum)} WHERE AdjNum = {SOut.Long(adj.AdjNum)}");
        else
            Db.NonQ($@"UPDATE paysplit SET ProvNum = {SOut.Long(adj.ProvNum)}
					WHERE SplitNum IN({string.Join(",", listSplits.Select(x => SOut.Long(x.SplitNum)))})");
    }

    /// <summary>
    ///     Goes to the db to grab current paysplits for the passed in paynum to insert, update, or delete db rows to
    ///     match listNew.. Hashes new paySplits and existing valid splits.
    /// </summary>
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

    #endregion

    #region Delete

    ///<summary>Deletes the paysplit.</summary>
    public static void Delete(PaySplit split)
    {
        var command = "DELETE from paysplit WHERE SplitNum = " + SOut.Long(split.SplitNum);
        Db.NonQ(command);
    }

    ///<summary>Deletes the paysplits by the SplitNums passed in.</summary>
    public static void DeleteMany(params long[] arraySplitNums)
    {
        if (arraySplitNums.IsNullOrEmpty()) return;

        var command = $"DELETE FROM paysplit WHERE SplitNum IN({string.Join(",", arraySplitNums.Select(x => SOut.Long(x)))})";
        Db.NonQ(command);
    }

    ///<summary>Used from payment window AutoSplit button to delete paysplits when clicking AutoSplit more than once.</summary>
    public static void DeleteForPayment(long payNum)
    {
        var command = "DELETE FROM paysplit"
                      + " WHERE PayNum=" + SOut.Long(payNum);
        Db.NonQ(command);
    }

    #endregion

    #region Misc Methods

    ///<summary>Returns true if a paysplit is attached to the associated procnum. Returns false otherwise.</summary>
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

    /// <summary>
    ///     Returns the salted hash for the paysplit. Will return an empty string if the calling program is unable to use
    ///     CDT.dll.
    /// </summary>
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

    /// <summary>
    ///     Validates the hash string in paysplit.SecurityHash. Returns true if it matches the expected hash, otherwise
    ///     false.
    /// </summary>
    public static bool IsPaySplitHashValid(PaySplit paySplit)
    {
        if (paySplit == null) return true;
        var dateHashStart = SecurityHash.GetHashingDate();
        if (paySplit.DatePay < dateHashStart) //old
            return true;
        if (paySplit.SecurityHash == HashFields(paySplit)) return true;
        return false;
    }

    #endregion
}