using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ClaimTrackings
{
    public static List<ClaimTracking> RefreshForUsers(ClaimTrackingType claimTrackingType, List<long> listUserNums)
    {
        if (listUserNums == null || listUserNums.Count == 0) return new List<ClaimTracking>();

        var command = "SELECT * FROM claimtracking WHERE TrackingType='" + SOut.String(claimTrackingType.ToString()) + "' "
                      + "AND UserNum IN (" + string.Join(",", listUserNums) + ")";
        return ClaimTrackingCrud.SelectMany(command);
    }

    public static List<ClaimTracking> RefreshForClaim(ClaimTrackingType claimTrackingType, long claimNum)
    {
        if (claimNum == 0) return new List<ClaimTracking>();

        var command = "SELECT * FROM claimtracking WHERE TrackingType='" + SOut.String(claimTrackingType.ToString()) + "' "
                      + "AND ClaimNum=" + SOut.Long(claimNum);
        return ClaimTrackingCrud.SelectMany(command);
    }

    public static List<ClaimTracking> GetForClaim(long claimNum)
    {
        if (claimNum == 0) return new List<ClaimTracking>();

        var command = "SELECT * FROM claimtracking WHERE ClaimNum=" + SOut.Long(claimNum);
        return ClaimTrackingCrud.SelectMany(command);
    }

    public static long Insert(ClaimTracking claimTracking)
    {
        return ClaimTrackingCrud.Insert(claimTracking);
    }

    public static long InsertClaimProcReceived(long claimNum, long userNum, string note = "")
    {
        var command = "SELECT COUNT(*) FROM claimtracking WHERE TrackingType='" + SOut.String(ClaimTrackingType.ClaimProcReceived.ToString())
                                                                                + "' AND ClaimNum=" + SOut.Long(claimNum) + " AND UserNum='" + userNum + "'";
        if (Db.GetCount(command) != "0") return 0; //Do nothing.

        var claimTracking = new ClaimTracking
        {
            TrackingType = ClaimTrackingType.ClaimProcReceived,
            ClaimNum = claimNum,
            UserNum = userNum,
            Note = note
        };

        return ClaimTrackingCrud.Insert(claimTracking);
    }

    public static void Update(ClaimTracking claimTracking)
    {
        ClaimTrackingCrud.Update(claimTracking);
    }

    public static void Sync(List<ClaimTracking> listClaimTrackings, List<ClaimTracking> listClaimTrackingsOld)
    {
        ClaimTrackingCrud.Sync(listClaimTrackings, listClaimTrackingsOld);
    }

    public static void Delete(long claimTrackingNum)
    {
        ClaimTrackingCrud.Delete(claimTrackingNum);
    }

    #region Misc Methods

    /// <summary>
    ///     Attempts to create or update ClaimTrackings and calls sync to update the database at the end.
    ///     Will update ClaimTracking if one has been inserted for a given claim that did not have one prior to calling this
    ///     method.
    ///     When called please ensure dictClaimTracking has entries.
    /// </summary>
    public static List<ClaimTracking> Assign(List<ODTuple<long, long>> listTrackingNumsAndClaimNums, long assignUserNum)
    {
        var command = "SELECT * FROM claimtracking WHERE claimtracking.TrackingType='" + SOut.String(ClaimTrackingType.ClaimUser.ToString()) + "' "
                      + "AND claimtracking.ClaimNum IN(" + string.Join(",", listTrackingNumsAndClaimNums.Select(x => x.Item2).ToList()) + ")";
        var listClaimTrackingsDb = ClaimTrackingCrud.SelectMany(command); //up to date copy from the database
        var listClaimTrackingsNew = listClaimTrackingsDb.Select(x => x.Copy()).ToList();
        foreach (Tuple<long, long> claimTrackingEntry in listTrackingNumsAndClaimNums)
        {
            //Item1=>claim tracking num & Item2=>claim num
            var claimTracking = new ClaimTracking();
            if (claimTrackingEntry.Item1 == 0 //Given claim did not have an existing ClaimTracking when dictClaimTracking was constructed.
                && !listClaimTrackingsDb.Exists(x => x.ClaimNum == claimTrackingEntry.Item2)) //DB does not contain ClaimTracking row for this claimNum.
            {
                if (assignUserNum == 0) continue;

                claimTracking.UserNum = assignUserNum;
                claimTracking.ClaimNum = claimTrackingEntry.Item2; //dict value is ClaimNum
                claimTracking.TrackingType = ClaimTrackingType.ClaimUser;
                listClaimTrackingsNew.Add(claimTracking);
                continue;
            }

            if (claimTrackingEntry.Item1 == 0)
            {
                //claim tracking did not originally exist but someone modified while we were here and it exists in the database now.
                claimTracking = listClaimTrackingsNew.FirstOrDefault(x => x.ClaimNum == claimTrackingEntry.Item2);
                claimTracking.UserNum = assignUserNum;
                continue;
            }

            //claim tracking already exsisted in the db for this claim
            claimTracking = listClaimTrackingsNew.FirstOrDefault(x => x.ClaimTrackingNum == claimTrackingEntry.Item1);
            if (claimTracking == null)
            {
                //ClaimTracking existed when method called but has been removed since.
                if (assignUserNum == 0) continue; //ClaimTracking was already removed for us.

                claimTracking = new ClaimTracking();
                claimTracking.UserNum = assignUserNum;
                claimTracking.ClaimNum = claimTrackingEntry.Item2; //dict value is ClaimNum
                claimTracking.TrackingType = ClaimTrackingType.ClaimUser;
                listClaimTrackingsNew.Add(claimTracking);
            }

            if (assignUserNum == 0)
                listClaimTrackingsNew.Remove(claimTracking);
            else
                claimTracking.UserNum = assignUserNum;
        }

        Sync(listClaimTrackingsNew, listClaimTrackingsDb);
        return listClaimTrackingsNew;
    }

    /// <summary>
    ///     Supplied two claims, this function will make new copies of the custom trackings for claimOrig, attach them to
    ///     claimDest, and insert them.
    /// </summary>
    public static void CopyToClaim(long claimOrigNum, long claimDestNum)
    {
        var listClaimTrackings = GetForClaim(claimOrigNum).OrderByDescending(x => x.DateTimeEntry).ToList();
        for (var i = 0; i < listClaimTrackings.Count; i++)
        {
            listClaimTrackings[i].ClaimNum = claimDestNum;
            listClaimTrackings[i].Note = "Split claim original entry timestamp: "
                                         + listClaimTrackings[i].DateTimeEntry + "\r\n"
                                         + listClaimTrackings[i].Note;
            Insert(listClaimTrackings[i]);
        }
    }

    #endregion

    /*
    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    #region CachePattern

    private class ClaimTrackingCache : CacheListAbs<ClaimTracking> {
        protected override List<ClaimTracking> GetCacheFromDb() {
            string command="SELECT * FROM ClaimTracking ORDER BY ItemOrder";
            return Crud.ClaimTrackingCrud.SelectMany(command);
        }
        protected override List<ClaimTracking> TableToList(DataTable table) {
            return Crud.ClaimTrackingCrud.TableToList(table);
        }
        protected override ClaimTracking Copy(ClaimTracking ClaimTracking) {
            return ClaimTracking.Clone();
        }
        protected override DataTable ListToTable(List<ClaimTracking> listClaimTrackings) {
            return Crud.ClaimTrackingCrud.ListToTable(listClaimTrackings,"ClaimTracking");
        }
        protected override void FillCacheIfNeeded() {
            ClaimTrackings.GetTableFromCache(false);
        }
        protected override bool IsInListShort(ClaimTracking ClaimTracking) {
            return !ClaimTracking.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static ClaimTrackingCache _ClaimTrackingCache=new ClaimTrackingCache();

    ///<summary>A list of all ClaimTrackings. Returns a deep copy.</summary>
    public static List<ClaimTracking> ListDeep {
        get {
            return _ClaimTrackingCache.ListDeep;
        }
    }

    ///<summary>A list of all visible ClaimTrackings. Returns a deep copy.</summary>
    public static List<ClaimTracking> ListShortDeep {
        get {
            return _ClaimTrackingCache.ListShortDeep;
        }
    }

    ///<summary>A list of all ClaimTrackings. Returns a shallow copy.</summary>
    public static List<ClaimTracking> ListShallow {
        get {
            return _ClaimTrackingCache.ListShallow;
        }
    }

    ///<summary>A list of all visible ClaimTrackings. Returns a shallow copy.</summary>
    public static List<ClaimTracking> ListShort {
        get {
            return _ClaimTrackingCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _ClaimTrackingCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _ClaimTrackingCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */
}