using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class AgingData
{
    public static Dictionary<long, PatAgingData> GetAgingData(bool singlePatient, bool includeChanged, bool excludeInsPending, bool excludeIfUnsentProcs, bool isSuperBills, List<long> listClinicNums, int daysExcludeInsPending = 0)
    {
        var dictPatAgingData = new Dictionary<long, PatAgingData>();
        var command = "";
        var guarOrPat = "guar";
        if (singlePatient)
        {
            guarOrPat = "patient";
        }

        var whereAndClinNum = "";
        var whereAndClaimDateSent = "";
        if (!listClinicNums.IsNullOrEmpty())
        {
            whereAndClinNum = $@"AND {guarOrPat}.ClinicNum IN ({string.Join(",", listClinicNums)})";
        }

        if (daysExcludeInsPending > 0)
        {
            //Only add to query if days are above 0, since this query line with a value of 0 would select nothing when we want to select everything pending.
            whereAndClaimDateSent = $@"AND claim.DateSent>CURDATE() - INTERVAL {daysExcludeInsPending} DAY";
        }

        if (includeChanged || excludeIfUnsentProcs)
        {
            command = $@"SELECT {guarOrPat}.PatNum,{guarOrPat}.ClinicNum,MAX(procedurelog.ProcDate) MaxProcDate";
            if (excludeIfUnsentProcs)
            {
                command += ",MAX(CASE WHEN insplan.IsMedical=1 THEN 0 ELSE COALESCE(claimproc.ProcNum,0) END)>0 HasUnsentProcs";
            }

            command += $@" FROM patient
					INNER JOIN patient guar ON guar.PatNum=patient.Guarantor
					INNER JOIN procedurelog ON procedurelog.PatNum = patient.PatNum ";
            if (excludeIfUnsentProcs)
            {
                command += $@"LEFT JOIN claimproc ON claimproc.ProcNum = procedurelog.ProcNum
						AND claimproc.NoBillIns=0
						AND claimproc.Status = {SOut.Int((int) ClaimProcStatus.Estimate)}
						AND procedurelog.ProcDate > CURDATE()-INTERVAL 6 MONTH
					LEFT JOIN insplan ON insplan.PlanNum=claimproc.PlanNum ";
            }

            command += $@"WHERE procedurelog.ProcFee > 0
					AND procedurelog.ProcStatus = {SOut.Int((int) ProcStat.C)}
					{whereAndClinNum}
					GROUP BY {guarOrPat}.PatNum
					ORDER BY NULL";
            using (var tableChangedAndUnsent = DataCore.GetTable(command))
            {
                foreach (DataRow row in tableChangedAndUnsent.Rows)
                {
                    var patNum = SIn.Long(row["PatNum"].ToString());
                    if (!dictPatAgingData.ContainsKey(patNum))
                    {
                        dictPatAgingData[patNum] = new PatAgingData(SIn.Long(row["ClinicNum"].ToString()));
                    }

                    if (includeChanged)
                    {
                        dictPatAgingData[patNum].ListPatAgingTransactions
                            .Add(new PatAgingTransaction(PatAgingTransaction.TransactionTypes.Procedure, SIn.Date(row["MaxProcDate"].ToString())));
                    }

                    if (excludeIfUnsentProcs)
                    {
                        dictPatAgingData[patNum].HasUnsentProcs = SIn.Bool(row["HasUnsentProcs"].ToString());
                    }
                }
            }
        }

        if (includeChanged)
        {
            command = $@"SELECT {guarOrPat}.PatNum,{guarOrPat}.ClinicNum,MAX(claimproc.DateCP) maxDateCP
					FROM claimproc
					INNER JOIN patient ON patient.PatNum = claimproc.PatNum
					INNER JOIN patient guar ON guar.PatNum=patient.Guarantor
					WHERE claimproc.InsPayAmt > 0
					{whereAndClinNum}
					GROUP BY {guarOrPat}.PatNum";
            using (var tableMaxPayDate = DataCore.GetTable(command))
            {
                foreach (DataRow row in tableMaxPayDate.Rows)
                {
                    var patNum = SIn.Long(row["PatNum"].ToString());
                    if (!dictPatAgingData.ContainsKey(patNum))
                    {
                        dictPatAgingData[patNum] = new PatAgingData(SIn.Long(row["ClinicNum"].ToString()));
                    }

                    dictPatAgingData[patNum].ListPatAgingTransactions
                        .Add(new PatAgingTransaction(PatAgingTransaction.TransactionTypes.ClaimProc, SIn.Date(row["maxDateCP"].ToString())));
                }
            }

            command = $@"SELECT {guarOrPat}.PatNum,{guarOrPat}.ClinicNum,MAX(payplancharge.ChargeDate) maxDatePPC,
						MAX(payplancharge.SecDateTEntry) maxDatePPCSDTE
					FROM payplancharge
					INNER JOIN patient ON patient.PatNum = payplancharge.PatNum
					INNER JOIN patient guar ON guar.PatNum=patient.Guarantor
					INNER JOIN payplan ON payplan.PayPlanNum = payplancharge.PayPlanNum
						AND payplan.PlanNum = 0 " //don't want insurance payment plans to make patients appear in the billing list
                      + $@"WHERE payplancharge.Principal + payplancharge.Interest>0
					AND payplancharge.ChargeType = {(int) PayPlanChargeType.Debit} "
                      //include all charges in the past or due 'PayPlanBillInAdvance' days into the future.
                      + $@"AND payplancharge.ChargeDate <= {SOut.Date(DateTime.Today.AddDays(PrefC.GetDouble(PrefName.PayPlansBillInAdvanceDays)))}
					{whereAndClinNum}
					GROUP BY {guarOrPat}.PatNum";
            using (var tableMaxPPCDate = DataCore.GetTable(command))
            {
                foreach (DataRow row in tableMaxPPCDate.Rows)
                {
                    var patNum = SIn.Long(row["PatNum"].ToString());
                    if (!dictPatAgingData.ContainsKey(patNum))
                    {
                        dictPatAgingData[patNum] = new PatAgingData(SIn.Long(row["ClinicNum"].ToString()));
                    }

                    dictPatAgingData[patNum].ListPatAgingTransactions
                        .Add(new PatAgingTransaction(
                            PatAgingTransaction.TransactionTypes.PayPlanCharge,
                            SIn.Date(row["maxDatePPC"].ToString()),
                            secDateTEntryTrans: SIn.Date(row["maxDatePPCSDTE"].ToString()))
                        );
                }
            }
        }

        if (excludeInsPending)
        {
            command = $@"SELECT {guarOrPat}.PatNum,{guarOrPat}.ClinicNum
					FROM claim
					INNER JOIN patient ON patient.PatNum=claim.PatNum
					INNER JOIN patient guar ON guar.PatNum=patient.Guarantor
					WHERE claim.ClaimStatus IN ('U','H','W','S','I')
					AND claim.ClaimType IN ('P','S','Other')
					{whereAndClinNum}
					{whereAndClaimDateSent}
					GROUP BY {guarOrPat}.PatNum";
            using (var tableInsPending = DataCore.GetTable(command))
            {
                foreach (DataRow row in tableInsPending.Rows)
                {
                    var patNum = SIn.Long(row["PatNum"].ToString());
                    if (!dictPatAgingData.ContainsKey(patNum))
                    {
                        dictPatAgingData[patNum] = new PatAgingData(SIn.Long(row["ClinicNum"].ToString()));
                    }

                    dictPatAgingData[patNum].HasPendingIns = true;
                }
            }
        }

        List<PatComm> listPatComms = [];
        using (var tableDateBalsBegan = Ledgers.GetDateBalanceBegan(null, isSuperBills, listClinicNums))
        {
            foreach (DataRow row in tableDateBalsBegan.Rows)
            {
                var patNum = SIn.Long(row["PatNum"].ToString());
                if (!dictPatAgingData.ContainsKey(patNum))
                {
                    dictPatAgingData[patNum] = new PatAgingData(SIn.Long(row["ClinicNum"].ToString()));
                }

                dictPatAgingData[patNum].DateBalBegan = SIn.Date(row["DateAccountAge"].ToString());
                dictPatAgingData[patNum].DateBalZero = SIn.Date(row["DateZeroBal"].ToString());
            }

            listPatComms = Patients.GetPatComms(tableDateBalsBegan.Select().Select(x => SIn.Long(x["PatNum"].ToString())).ToList(), null);
        }

        foreach (var pComm in listPatComms)
        {
            if (!dictPatAgingData.ContainsKey(pComm.PatNum))
            {
                dictPatAgingData[pComm.PatNum] = new PatAgingData(pComm.ClinicNum);
            }

            dictPatAgingData[pComm.PatNum].PatComm = pComm;
        }

        return dictPatAgingData;
    }

    public static DateTime GetDateLastTrans(List<PatAgingTransaction> patAgingTransactions, DateTime dateLastStatement)
    {
        //Procedures and claimprocs are straight forward in the sense that a statement will be required if their date is after dateLastStatement.
        //Payment plans are tricky in the sense that we have a preference that allows billing patients X days in advance.
        //If there is a valid procedure or claimproc date and it falls after dateLastStatement then this patient needs a statement.
        var dateLastTrans = new DateTime[]
        {
            GetMaxDateLastTransForType(patAgingTransactions, PatAgingTransaction.TransactionTypes.Procedure),
            GetMaxDateLastTransForType(patAgingTransactions, PatAgingTransaction.TransactionTypes.ClaimProc),
        }.Max();
        //Check to see if this patient has a payment plan that has a charge date greater than the last procedure or claimproc date.
        var datePayPlanChargeMax = GetMaxDateLastTransForType(patAgingTransactions, PatAgingTransaction.TransactionTypes.PayPlanCharge);
        if (datePayPlanChargeMax > dateLastTrans)
        {
            //There is a chance that this patient has already received a statement due to this payment plan charge due to the fact that we allow
            //for "billing X days in advance" for payment plans only (via PayPlansBillInAdvanceDays).
            var billInAdvanceDays = PrefC.GetLong(PrefName.PayPlansBillInAdvanceDays);
            //Only set dateLastTrans to a payment plan charge date if the charge falls outside of the "bill X days in advance" preference.
            //E.g. A statement on the 1st of the month is treated as having covored all payment plan charges until the 11th when pref is set to 10 days.
            //However, dateLastTrans needs to be set when a billing list is created on the 5th with a new payment plan charge on the 14th.
            //This is because the statement created on the 1st does not cover the payment plan charge on the 14th.
            var datePayPlanCreateMax = GetMaxDateLastTransForType(patAgingTransactions, PatAgingTransaction.TransactionTypes.PayPlanCharge, true);
            if (datePayPlanChargeMax > dateLastStatement.AddDays(billInAdvanceDays) || datePayPlanCreateMax > dateLastStatement)
            {
                dateLastTrans = datePayPlanChargeMax;
            }
        }

        return dateLastTrans;
    }

    private static DateTime GetMaxDateLastTransForType(List<PatAgingTransaction> patAgingTransactions, PatAgingTransaction.TransactionTypes transactionType, bool useSecDateTEntry = false)
    {
        var listTransForType = patAgingTransactions.FindAll(x => x.TransactionType == transactionType);
        if (listTransForType.IsNullOrEmpty())
        {
            return DateTime.MinValue;
        }

        if (useSecDateTEntry)
        {
            return listTransForType.Max(x => x.SecDateTEntryTrans).Date;
        }
        else
        {
            return listTransForType.Max(x => x.DateLastTrans);
        }
    }
}
    
public class PatAgingData(long clinicNum)
{
    public DateTime DateBalBegan = DateTime.MinValue;
    public DateTime DateBalZero = DateTime.MinValue;
    public readonly List<PatAgingTransaction> ListPatAgingTransactions = [];
    public PatComm PatComm;
    public bool HasUnsentProcs;
    public bool HasPendingIns;
    public readonly long ClinicNum = clinicNum;
}
    
public class PatAgingTransaction(PatAgingTransaction.TransactionTypes transactionType, DateTime dateLastTrans, DateTime secDateTEntryTrans = default)
{
    public readonly TransactionTypes TransactionType = transactionType;
    public DateTime DateLastTrans = dateLastTrans;
    public DateTime SecDateTEntryTrans = secDateTEntryTrans;

    public enum TransactionTypes
    {
        Procedure,
        ClaimProc,
        PayPlanCharge
    }
}