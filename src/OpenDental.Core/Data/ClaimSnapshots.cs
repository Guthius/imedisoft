using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ClaimSnapshots
{
    public static List<ClaimSnapshot> GetByClaimProcNums(List<long> claimProcNums)
    {
        if (claimProcNums == null || claimProcNums.Count == 0)
        {
            return [];
        }

        return ClaimSnapshotCrud.SelectMany("SELECT * FROM claimsnapshot WHERE ClaimProcNum IN (" + string.Join(", ", claimProcNums) + ")");
    }

    public static void CreateClaimSnapshot(List<ClaimProc> claimProcs, ClaimSnapshotTrigger triggerType, string claimType)
    {
        if (!PrefC.GetBool(PrefName.ClaimSnapshotEnabled) || SIn.Enum<ClaimSnapshotTrigger>(PrefC.GetString(PrefName.ClaimSnapshotTriggerType), true) != triggerType)
        {
            return;
        }

        if (triggerType == ClaimSnapshotTrigger.Service)
        {
            CreateClaimSnapShotService();
            return;
        }

        var completedProcFees = Procedures
            .GetProcsFromClaimProcs(claimProcs)
            .ToDictionary(
                x => x.ProcNum,
                x => x.ProcFee);

        var claimSnapshotsOld = GetByClaimProcNums(claimProcs.Select(x => x.ClaimProcNum).ToList());

        foreach (var claimProc in claimProcs)
        {
            if (claimProc.Status is ClaimProcStatus.Preauth or ClaimProcStatus.Adjustment or ClaimProcStatus.Estimate or ClaimProcStatus.CapEstimate)
            {
                continue;
            }

            if (!completedProcFees.TryGetValue(claimProc.ProcNum, out var procFee))
            {
                procFee = 0;
            }

            var claimSnapshotExisting = claimSnapshotsOld
                .Find(snapshot =>
                    snapshot.DateTEntry.Date == DateTime.Today.Date &&
                    snapshot.ProcNum == claimProc.ProcNum &&
                    snapshot.ClaimProcNum == claimProc.ClaimProcNum &&
                    snapshot.ClaimType == claimType &&
                    snapshot.SnapshotTrigger != ClaimSnapshotTrigger.Service);

            if (claimSnapshotExisting != null)
            {
                SetSnapshotFields(claimSnapshotExisting, claimProc, procFee, triggerType, claimType);

                Update(claimSnapshotExisting);

                continue;
            }

            var claimSnapshot = new ClaimSnapshot();

            SetSnapshotFields(claimSnapshot, claimProc, procFee, triggerType, claimType);

            Insert(claimSnapshot);
        }
    }

    private static void SetSnapshotFields(ClaimSnapshot claimSnapshot, ClaimProc claimProc, double procFee, ClaimSnapshotTrigger claimSnapshotTrigger, string claimType)
    {
        claimSnapshot.ProcNum = claimProc.ProcNum;
        claimSnapshot.Writeoff = claimProc.WriteOff;
        claimSnapshot.InsPayEst = claimProc.InsEstTotal;
        claimSnapshot.Fee = procFee;
        claimSnapshot.ClaimProcNum = claimProc.ClaimProcNum;
        claimSnapshot.SnapshotTrigger = claimSnapshotTrigger;
        claimSnapshot.ClaimType = claimType;
    }

    private static void CreateClaimSnapShotService()
    {
        var proceduresCompleted = Procedures.GetCompletedByDateCompleteForDateRange(DateTime.Today, DateTime.Today);

        var claimProcs = ClaimProcs.GetForProcsWithOrdinal(proceduresCompleted.Select(x => x.ProcNum).ToList(), 1).FindAll(x => x.Status is not ClaimProcStatus.Preauth and not ClaimProcStatus.Adjustment);
        var patPlans = PatPlans.GetListByInsSubNums(claimProcs.Select(x => x.InsSubNum).ToList());

        claimProcs = claimProcs
            .OrderByDescending(x => x.ClaimNum)
            .ThenByDescending(x => x.SecDateEntry)
            .GroupBy(x => new {x.ProcNum, Ordinal = PatPlans.GetOrdinal(x.InsSubNum, patPlans.Where(y => y.PatNum == x.PatNum).ToList())})
            .Select(x => x.First())
            .ToList();

        foreach (var claimProc in claimProcs)
        {
            if (claimProc.Status is ClaimProcStatus.CapClaim or ClaimProcStatus.CapComplete or ClaimProcStatus.CapEstimate or ClaimProcStatus.Preauth or ClaimProcStatus.Supplemental or ClaimProcStatus.InsHist)
            {
                continue;
            }

            double procFee = 0;

            var procedure = proceduresCompleted.Find(x => x.ProcNum == claimProc.ProcNum);
            if (procedure != null)
            {
                procFee = procedure.ProcFee;
            }

            var writeoffAmt = claimProc.WriteOff;

            if (claimProc.Status != ClaimProcStatus.NotReceived && claimProc.Status != ClaimProcStatus.Received)
            {
                writeoffAmt = claimProc.WriteOffEstOverride != -1 ? claimProc.WriteOffEstOverride : claimProc.WriteOffEst;
            }

            Insert(new ClaimSnapshot
            {
                ProcNum = claimProc.ProcNum,
                Writeoff = writeoffAmt,
                InsPayEst = claimProc.InsEstTotal,
                Fee = procFee,
                ClaimProcNum = claimProc.ClaimProcNum,
                SnapshotTrigger = ClaimSnapshotTrigger.Service
            });
        }
    }

    public static void Insert(ClaimSnapshot claimSnapshot)
    {
        if (Db.GetCount("SELECT COUNT(*) FROM claimsnapshot WHERE ProcNum=" + claimSnapshot.ProcNum + " AND ClaimProcNum='" + claimSnapshot.ClaimProcNum + "'") != "0")
        {
            return;
        }

        ClaimSnapshotCrud.Insert(claimSnapshot);
    }

    public static void Update(ClaimSnapshot claimSnapshot)
    {
        ClaimSnapshotCrud.Update(claimSnapshot);
    }

    public static void DeleteForClaimProcs(List<long> claimProcNums)
    {
        if (claimProcNums == null || claimProcNums.Count < 1)
        {
            return;
        }

        Db.NonQ("DELETE FROM claimsnapshot WHERE ClaimProcNum IN (" + string.Join(",", claimProcNums) + ")");
    }
}