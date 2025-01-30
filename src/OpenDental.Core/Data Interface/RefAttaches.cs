using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class RefAttaches
{
    public static void Insert(RefAttach attach)
    {
        RefAttachCrud.Insert(attach);
    }

    public static void Delete(RefAttach attach)
    {
        var command = "UPDATE refattach SET ItemOrder=ItemOrder-1 WHERE PatNum=" + SOut.Long(attach.PatNum)
                                                                                 + " AND ItemOrder > " + SOut.Int(attach.ItemOrder);
        Db.NonQ(command);
        command = "DELETE FROM refattach "
                  + "WHERE refattachnum = " + SOut.Long(attach.RefAttachNum);
        Db.NonQ(command);
    }

    public static bool IsReferralAttached(long referralNum)
    {
        var command = "SELECT COUNT(*) FROM refattach WHERE ReferralNum = '" + SOut.Long(referralNum) + "'";
        return Db.GetCount(command) != "0";
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
        //Inner join with referral table on ReferralNum to ignore invalid RefAttaches.  DBM removes these invalid rows anyway.
        var command = "SELECT refattach.* FROM refattach "
                      + "INNER JOIN referral ON refattach.ReferralNum=referral.ReferralNum "
                      + "WHERE refattach.PatNum = " + SOut.Long(patNum) + " ";
        if (procNum != 0) //for procedure
            if (!showAll)
                //hide regular referrals
                command += "AND refattach.ProcNum=" + SOut.Long(procNum) + " ";

        command += "ORDER BY refattach.ItemOrder";
        return RefAttachCrud.SelectMany(command);
    }

    public static List<RefAttach> RefreshForReferralProcTrack(DateTime dateFrom, DateTime dateTo, bool complete)
    {
        //Inner join with referral table on ReferralNum to ignore invalid RefAttaches.  DBM removes these invalid rows anyway.
        var command = "SELECT refattach.* FROM refattach "
                      + "INNER JOIN referral ON refattach.ReferralNum=referral.ReferralNum "
                      + "INNER JOIN procedurelog ON refattach.ProcNum=procedurelog.ProcNum "
                      + "WHERE refattach.RefDate>=" + SOut.Date(dateFrom) + " "
                      + "AND refattach.RefDate<=" + SOut.Date(dateTo) + " ";
        if (!complete) command += "AND refattach.DateProcComplete=" + SOut.Date(DateTime.MinValue) + " ";
        command += "ORDER BY refattach.RefDate";
        return RefAttachCrud.SelectMany(command);
    }

    public static List<string> GetPats(long refNum, ReferralType refType)
    {
        var command = "SELECT CONCAT(CONCAT(patient.LName,', '),patient.FName) "
                      + "FROM patient,refattach,referral "
                      + "WHERE patient.PatNum=refattach.PatNum "
                      + "AND refattach.ReferralNum=referral.ReferralNum "
                      + "AND refattach.RefType=" + SOut.Int((int) refType) + " "
                      + "AND referral.ReferralNum=" + SOut.Long(refNum);
        var table = DataCore.GetTable(command);
        var listStrings = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listStrings.Add(table.Rows[i][0].ToString());
        return listStrings;
    }

    public static long GetReferralNum(long patNum)
    {
        var command = "SELECT ReferralNum "
                      + "FROM refattach "
                      + "WHERE refattach.PatNum =" + SOut.Long(patNum) + " "
                      + "AND refattach.RefType=" + SOut.Int((int) ReferralType.RefFrom) + " "
                      + "ORDER BY ItemOrder ";
        command = DbHelper.LimitOrderBy(command, 1);
        return SIn.Long(DataCore.GetScalar(command));
    }

    public static List<RefAttach> GetRefAttaches(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<RefAttach>();
        var command = "SELECT * FROM refattach "
                      + "WHERE refattach.PatNum IN (" + string.Join(",", listPatNums) + ")";
        return RefAttachCrud.SelectMany(command);
    }

    public static List<RefAttach> GetRefAttachesForSummaryOfCareForPat(long patNum)
    {
        return RefAttachCrud.SelectMany(
            "SELECT * FROM refattach " +
            "WHERE PatNum = " + patNum + " " +
            "AND RefType=" + (int) ReferralType.RefTo + " " +
            "AND IsTransitionOfCare=1 AND ProvNum!=0 " +
            "ORDER BY ItemOrder");
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