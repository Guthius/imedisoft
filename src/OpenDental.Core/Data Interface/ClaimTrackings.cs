using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ClaimTrackings
{
    public static List<ClaimTracking> RefreshForUsers(ClaimTrackingType claimTrackingType, List<long> userNums)
    {
        if (userNums == null || userNums.Count == 0)
        {
            return [];
        }

        return ClaimTrackingCrud.SelectMany("SELECT * FROM claimtracking WHERE TrackingType='" + claimTrackingType + "' AND UserNum IN (" + string.Join(",", userNums) + ")");
    }

    public static List<ClaimTracking> RefreshForClaim(ClaimTrackingType claimTrackingType, long claimNum)
    {
        return claimNum == 0 ? [] : ClaimTrackingCrud.SelectMany("SELECT * FROM claimtracking WHERE TrackingType='" + claimTrackingType + "' AND ClaimNum=" + claimNum);
    }

    public static List<ClaimTracking> GetForClaim(long claimNum)
    {
        return claimNum == 0 ? [] : ClaimTrackingCrud.SelectMany("SELECT * FROM claimtracking WHERE ClaimNum=" + claimNum);
    }

    public static void Insert(ClaimTracking claimTracking)
    {
        ClaimTrackingCrud.Insert(claimTracking);
    }

    public static void InsertClaimProcReceived(long claimNum, long userNum, string note = "")
    {
        var commandText = 
            "SELECT COUNT(*) FROM claimtracking " +
            "WHERE TrackingType='" + ClaimTrackingType.ClaimProcReceived + "' " +
            "AND ClaimNum=" + claimNum + " " +
            "AND UserNum=" + userNum;
        
        if (Db.GetCount(commandText) != "0")
        {
            return;
        }
        
        ClaimTrackingCrud.Insert(new ClaimTracking
        {
            TrackingType = ClaimTrackingType.ClaimProcReceived,
            ClaimNum = claimNum,
            UserNum = userNum,
            Note = note
        });
    }

    public static void Update(ClaimTracking claimTracking)
    {
        ClaimTrackingCrud.Update(claimTracking);
    }

    public static void Sync(List<ClaimTracking> listClaimTrackings, List<ClaimTracking> listClaimTrackingsOld)
    {
        ClaimTrackingCrud.Sync(listClaimTrackings, listClaimTrackingsOld);
    }

    public static List<ClaimTracking> Assign(List<Tuple<long, long>> trackingNumsAndClaimNums, long assignUserNum)
    {
        var commandText = 
            "SELECT * FROM claimtracking " +
            "WHERE claimtracking.TrackingType = '" + SOut.String(ClaimTrackingType.ClaimUser.ToString()) + "' " + 
            "AND claimtracking.ClaimNum IN (" + string.Join(",", trackingNumsAndClaimNums.Select(x => x.Item2)) + ")";
        
        var claimTrackingsDb = ClaimTrackingCrud.SelectMany(commandText);
        var claimTrackingsNew = claimTrackingsDb.Select(x => x.Copy()).ToList();
        
        foreach (var (claimTrackingNum, claimNum) in trackingNumsAndClaimNums)
        {
            var claimTracking = new ClaimTracking();
            switch (claimTrackingNum)
            {
                case 0 when !claimTrackingsDb.Exists(x => x.ClaimNum == claimNum):
                {
                    if (assignUserNum == 0)
                    {
                        continue;
                    }

                    claimTracking.UserNum = assignUserNum;
                    claimTracking.ClaimNum = claimNum;
                    claimTracking.TrackingType = ClaimTrackingType.ClaimUser;
                    claimTrackingsNew.Add(claimTracking);
                    continue;
                }
                
                case 0:
                    claimTracking = claimTrackingsNew.FirstOrDefault(x => x.ClaimNum == claimNum);
                    claimTracking.UserNum = assignUserNum;
                    continue;
            }
            
            claimTracking = claimTrackingsNew.FirstOrDefault(x => x.ClaimTrackingNum == claimTrackingNum);
            if (claimTracking is null)
            {
                if (assignUserNum == 0)
                {
                    continue;
                }

                claimTracking = new ClaimTracking
                {
                    UserNum = assignUserNum,
                    ClaimNum = claimNum,
                    TrackingType = ClaimTrackingType.ClaimUser
                };
                
                claimTrackingsNew.Add(claimTracking);
            }

            if (assignUserNum == 0)
            {
                claimTrackingsNew.Remove(claimTracking);
            }
            else
            {
                claimTracking.UserNum = assignUserNum;
            }
        }

        Sync(claimTrackingsNew, claimTrackingsDb);
        
        return claimTrackingsNew;
    }

    public static void CopyToClaim(long claimOrigNum, long claimDestNum)
    {
        var claimTrackings = GetForClaim(claimOrigNum).OrderByDescending(x => x.DateTimeEntry).ToList();
        
        foreach (var claimTracking in claimTrackings)
        {
            claimTracking.ClaimNum = claimDestNum;
            claimTracking.Note = "Split claim original entry timestamp: " + claimTracking.DateTimeEntry + "\r\n" + claimTracking.Note;
            
            Insert(claimTracking);
        }
    }
}