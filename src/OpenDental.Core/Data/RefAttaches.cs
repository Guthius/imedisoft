using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class RefAttaches
{
    public static void Insert(RefAttach attach)
    {
        RefAttachCrud.Insert(attach);
    }

    public static void Delete(RefAttach attach)
    {
        Db.NonQ("UPDATE refattach SET ItemOrder=ItemOrder-1 WHERE PatNum = " + attach.PatNum + " AND ItemOrder > " + attach.ItemOrder);
        Db.NonQ("DELETE FROM refattach WHERE refattachnum = " + attach.RefAttachNum);
    }

    public static bool IsReferralAttached(long referralNum)
    {
        return Db.GetCount("SELECT COUNT(*) FROM refattach WHERE ReferralNum = " + referralNum) != "0";
    }

    public static List<RefAttach> Refresh(long patNum)
    {
        return RefreshFiltered(patNum, true, 0);
    }

    public static List<RefAttach> GetPatientData(long patNum)
    {
        return RefAttachCrud.SelectMany("SELECT * FROM refattach WHERE PatNum = " + patNum + " ORDER BY ItemOrder");
    }

    public static List<RefAttach> RefreshFiltered(long patNum, bool showAll, long procNum)
    {
        var commandText =
            "SELECT refattach.* FROM refattach " +
            "INNER JOIN referral ON refattach.ReferralNum = referral.ReferralNum " +
            "WHERE refattach.PatNum = " + patNum + " ";

        if (procNum != 0 && !showAll)
        {
            commandText += "AND refattach.ProcNum=" + procNum + " ";
        }

        commandText += "ORDER BY refattach.ItemOrder";

        return RefAttachCrud.SelectMany(commandText);
    }

    public static List<RefAttach> RefreshForReferralProcTrack(DateTime from, DateTime to, bool complete)
    {
        var commandText =
            "SELECT refattach.* FROM refattach " +
            "INNER JOIN referral ON refattach.ReferralNum = referral.ReferralNum " +
            "INNER JOIN procedurelog ON refattach.ProcNum = procedurelog.ProcNum " +
            "WHERE refattach.RefDate >= " + SOut.Date(from) + " " +
            "AND refattach.RefDate <= " + SOut.Date(to) + " ";

        if (!complete)
        {
            commandText += "AND refattach.DateProcComplete=" + SOut.Date(DateTime.MinValue) + " ";
        }

        commandText += "ORDER BY refattach.RefDate";

        return RefAttachCrud.SelectMany(commandText);
    }

    public static List<string> GetPats(long refNum, ReferralType refType)
    {
        var dataTable = DataCore.GetTable(
            "SELECT CONCAT(CONCAT(patient.LName, ', '), patient.FName) " +
            "FROM patient, refattach, referral " +
            "WHERE patient.PatNum = refattach.PatNum " +
            "AND refattach.ReferralNum = referral.ReferralNum " +
            "AND refattach.RefType = " + (int) refType + " " +
            "AND referral.ReferralNum = " + refNum);

        var patientNames = new List<string>();
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            patientNames.Add(dataTable.Rows[i][0].ToString());
        }

        return patientNames;
    }

    public static long GetReferralNum(long patNum)
    {
        return SIn.Long(DataCore.GetScalar(
            "SELECT ReferralNum FROM refattach " +
            "WHERE PatNum = " + patNum + " " +
            "AND RefType = " + (int) ReferralType.RefFrom + " " +
            "ORDER BY ItemOrder LIMIT 1"));
    }

    public static List<RefAttach> GetRefAttaches(List<long> patNums)
    {
        return patNums.Count == 0 ? [] : RefAttachCrud.SelectMany("SELECT * FROM refattach WHERE refattach.PatNum IN (" + string.Join(", ", patNums) + ")");
    }

    public static void Update(RefAttach attach)
    {
        RefAttachCrud.Update(attach);
    }

    public static void Update(RefAttach attach, RefAttach attachOld)
    {
        RefAttachCrud.Update(attach, attachOld);
    }
}