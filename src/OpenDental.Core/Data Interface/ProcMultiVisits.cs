using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProcMultiVisits
{
    #region Delete

    ///<summary>Does not send cache refresh signal.  Send the signal from calling code.</summary>
    public static void Delete(long procMultiVisitNum)
    {
        ProcMultiVisitCrud.Delete(procMultiVisitNum);
    }

    #endregion Delete

    #region Cache Pattern

    /// <summary>
    ///     The procmultivisit table could be quite large after a few years of use by a big organization.  However, the
    ///     number of In Process procedures should be a few thousand or less, even for a large organization. So we currently
    ///     store all of them in the cache. =Jordan We need to remove this cache pattern.
    /// </summary>
    private class ProcMultiVisitCache : CacheListAbs<ProcMultiVisit>
    {
        protected override List<ProcMultiVisit> GetCacheFromDb()
        {
            var command = "SELECT * FROM procmultivisit WHERE IsInProcess=1";
            return ProcMultiVisitCrud.SelectMany(command);
        }

        protected override List<ProcMultiVisit> TableToList(DataTable dataTable)
        {
            return ProcMultiVisitCrud.TableToList(dataTable);
        }

        protected override ProcMultiVisit Copy(ProcMultiVisit item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProcMultiVisit> items)
        {
            return ProcMultiVisitCrud.ListToTable(items, "ProcMultiVisit");
        }

        protected override void FillCacheIfNeeded()
        {
            ProcMultiVisits.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProcMultiVisitCache _procMultiVisitCache = new();

    public static List<ProcMultiVisit> GetDeepCopy(bool isShort = false)
    {
        return _procMultiVisitCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _procMultiVisitCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<ProcMultiVisit> match, bool isShort = false)
    {
        return _procMultiVisitCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<ProcMultiVisit> match, bool isShort = false)
    {
        return _procMultiVisitCache.GetFindIndex(match, isShort);
    }

    public static ProcMultiVisit GetFirst(bool isShort = false)
    {
        return _procMultiVisitCache.GetFirst(isShort);
    }

    public static ProcMultiVisit GetFirst(Func<ProcMultiVisit, bool> match, bool isShort = false)
    {
        return _procMultiVisitCache.GetFirst(match, isShort);
    }

    public static ProcMultiVisit GetFirstOrDefault(Func<ProcMultiVisit, bool> match, bool isShort = false)
    {
        return _procMultiVisitCache.GetFirstOrDefault(match, isShort);
    }

    public static ProcMultiVisit GetLast(bool isShort = false)
    {
        return _procMultiVisitCache.GetLast(isShort);
    }

    public static ProcMultiVisit GetLastOrDefault(Func<ProcMultiVisit, bool> match, bool isShort = false)
    {
        return _procMultiVisitCache.GetLastOrDefault(match, isShort);
    }

    public static List<ProcMultiVisit> GetWhere(Predicate<ProcMultiVisit> match, bool isShort = false)
    {
        return _procMultiVisitCache.GetWhere(match, isShort);
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _procMultiVisitCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _procMultiVisitCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _procMultiVisitCache.ClearCache();
    }

    #endregion Cache Pattern

    #region Insert

    ///<summary>Does not send cache refresh signal.  Send the signal from calling code.</summary>
    public static long Insert(ProcMultiVisit procMultiVisit)
    {
        return ProcMultiVisitCrud.Insert(procMultiVisit);
    }

    ///<summary>Will not create a group if there are less than 2 items in listProcs.  Also sends signal and refreshes cache.</summary>
    public static void CreateGroup(List<Procedure> listProcedures)
    {
        if (listProcedures.Count < 2) //No reason to make a "group" with 0 or 1 items.
            return;
        var listProcMultiVisits = new List<ProcMultiVisit>();
        for (var i = 0; i < listProcedures.Count; i++)
        {
            var procMultiVisit = new ProcMultiVisit();
            procMultiVisit.ProcNum = listProcedures[i].ProcNum;
            procMultiVisit.ProcStatus = listProcedures[i].ProcStatus;
            procMultiVisit.PatNum = listProcedures[i].PatNum;
            listProcMultiVisits.Add(procMultiVisit);
        }

        var isGroupInProcess = IsGroupInProcess(listProcMultiVisits); //Could be in process if grouped procs which are different statuses via menu.
        long groupProcMultiVisitNum = 0;
        for (var i = 0; i < listProcMultiVisits.Count; i++)
        {
            var procMultiVisit = listProcMultiVisits[i];
            procMultiVisit.IsInProcess = isGroupInProcess;
            if (i == 0)
            {
                groupProcMultiVisitNum = Insert(procMultiVisit);
                var procMultiVisitOld = procMultiVisit.Copy();
                procMultiVisit.GroupProcMultiVisitNum = groupProcMultiVisitNum;
                Update(procMultiVisit, procMultiVisitOld); //Have to update after insert, or else we cannot know what the primary key is.
            }
            else
            {
                procMultiVisit.GroupProcMultiVisitNum = groupProcMultiVisitNum;
                Insert(procMultiVisit);
            }

            var listClaimProcs = ClaimProcs.GetForProcs(new List<long> {procMultiVisit.ProcNum});
            var listClaims = Claims.GetClaimsFromClaimNums(listClaimProcs.Select(x => x.ClaimNum).ToList());
            for (var j = 0; j < listClaims.Count; j++)
                if (listClaims[j].ClaimStatus.In("U", "W", "H"))
                {
                    var claimOld = listClaims[j].Copy();
                    listClaims[j].ClaimStatus = "I";
                    Claims.Update(listClaims[j], claimOld);
                }
        }

        Signalods.SetInvalid(InvalidType.ProcMultiVisits);
        RefreshCache();
    }

    #endregion Insert

    #region Update

    ///<summary>Does not send cache refresh signal.  Send the signal from calling code.</summary>
    public static void Update(ProcMultiVisit procMultiVisit, ProcMultiVisit oldProcMultiVisit)
    {
        if (!ProcMultiVisitCrud.UpdateComparison(procMultiVisit, oldProcMultiVisit)) return; //No changes.  Save middle tier call.

        ProcMultiVisitCrud.Update(procMultiVisit, oldProcMultiVisit);
    }

    /// <summary>
    ///     Responsible for updating procedures in the group to "In Process" or "Not In Process", depending on the stat passed
    ///     in.
    ///     Also sends signal to cause cache refresh.  Returns list of ClaimNums which had claimClaimStatus modified (can be
    ///     empty list).
    /// </summary>
    public static List<long> UpdateGroupForProc(long procNum, ProcStat stat)
    {
        var listPmvs = GetGroupsForProcsFromDb(procNum);
        var pmv = listPmvs.FirstOrDefault(x => x.ProcNum == procNum);
        if (pmv == null) return new List<long>(); //Rare edge case.  Might happen is someone deletes the procedure at the same time another person is updating it.
        var isGroupInProcessOld = IsGroupInProcess(listPmvs);
        if (stat == ProcStat.D)
        {
            //If the procedure is deleted, also delete the procvisitmulti to reduce clutter.
            listPmvs.Remove(pmv); //Remove pmv from listpmvs.
            if (pmv.ProcMultiVisitNum == pmv.GroupProcMultiVisitNum && !listPmvs.IsNullOrEmpty())
            {
                //If the group points to the pmv to be removed and the group still exists.
                var replacementGPMVNum = listPmvs.First().ProcMultiVisitNum;
                UpdateGroupProcMultiVisitNumForGroup(pmv.ProcMultiVisitNum, replacementGPMVNum);
                foreach (var procMulti in listPmvs) //Replace all group numbers.
                    procMulti.GroupProcMultiVisitNum = replacementGPMVNum;
            }

            Delete(pmv.ProcMultiVisitNum);
        }
        else
        {
            var oldPmv = pmv.Copy();
            pmv.ProcStatus = stat;
            Update(pmv, oldPmv);
        }

        var isGroupInProcess = IsGroupInProcess(listPmvs);
        if (isGroupInProcess != isGroupInProcessOld) UpdateInProcessForGroup(pmv.GroupProcMultiVisitNum, isGroupInProcess);
        //Always send a signal and refresh the cache in case someone else is going to edit the group soon.
        Signalods.SetInvalid(InvalidType.ProcMultiVisits);
        RefreshCache();
        //Use ProcNum to get associated ClaimProc (if any)
        var listClaimProcsForProcMultiVisit = ClaimProcs.GetForProcs(listPmvs.Select(x => x.ProcNum).ToList());
        //Use claimProc.ClaimNum to determine if a claim is attached to the given In Process proc (ClaimNum!=0)
        var listClaimNums = listClaimProcsForProcMultiVisit.Select(x => x.ClaimNum).Distinct().ToList();
        var listClaims = Claims.GetClaimsFromClaimNums(listClaimNums).Where(x => x.ClaimType != "PreAuth").ToList(); //Ignore PreAuths
        var listModifiedClaimNums = new List<long>();
        for (var i = 0; i < listClaims.Count; i++)
        {
            //If a claim is not attached or the ClaimStatus is not "U", "W", "I", or "H", kick out.
            if (!listClaims[i].ClaimStatus.In("U", "W", "I", "H")) continue;
            var claimOld = listClaims[i].Copy();
            var listClaimProcsForClaim = new List<ClaimProc>();
            listClaimProcsForClaim = ClaimProcs.RefreshForClaim(listClaims[i].ClaimNum);
            if (listClaimProcsForClaim.Count == 0) //This is rare but still happens.  See DBM. 
                continue;
            var isProcsInProcess = listClaimProcsForClaim.Exists(x => IsProcInProcess(x.ProcNum));
            if (isProcsInProcess)
            {
                listClaims[i].ClaimStatus = "I";
            }
            else
            {
                listClaims[i].ClaimStatus = "W";
                if (listClaims[i].ClaimType != "P")
                {
                    //If the current claim isn't primary, we need to check if we should set the status to Hold Until Pri Received instead of Waiting to Send.
                    var listProcNums = listClaimProcsForClaim.Select(x => x.ProcNum).ToList();
                    var listClaimProcsForProcs = ClaimProcs.GetForProcs(listProcNums);
                    var listClaimsForClaimProcs = Claims.GetClaimsFromClaimNums(listClaimProcsForProcs.Select(x => x.ClaimNum).Distinct().ToList());
                    if (listClaimsForClaimProcs.Exists(x => x.ClaimType == "P")) //Check if a primary claim is attached to one of the same procedures that this non-primary claim is attached to
                        listClaims[i].ClaimStatus = "H"; //Set the status to Hold Until Pri Received instead of Waiting to Send
                }

                var listProcMultiVisitsForClaims = GetGroupsForProcsFromDb(listClaimProcsForClaim.Select(x => x.ProcNum).ToArray());
                //Set the ClaimProc.ProcDate and Claim.DateService as used to be done in AccountModules.CreateClaim()
                var listPatProcs = Procedures.Refresh(pmv.PatNum);
                foreach (var cp in listClaimProcsForClaim)
                {
                    var cpOld = cp.Copy();
                    //It is common for only some of the procedures in a multi visit group to be attached to the claim while others are not.
                    //For example, the Crown code with fee would be attached to the claim,
                    //while the Seat code is not attached to the claim because its fee is usually $0, which the UI blocks from attaching to the claim.
                    //We are required to use the greatest date for all procedures in the group when reporting the attached procedure dates on the claim.
                    var pmvForProc = listProcMultiVisitsForClaims.FirstOrDefault(x => x.ProcNum == cp.ProcNum);
                    if (pmvForProc == null)
                    {
                        //Procedure does not belong to a multi visit group.
                        //A procedure which is not part of a multi visit group might be attached to the claim along with a group of procedures.
                        //In this case, there will be no procvisitmulti row available.
                        //This case might also happen if one user deletes a procedure in the group right when another user is sending the claim.
                        var procForCp = listPatProcs.FirstOrDefault(x => x.ProcNum == cp.ProcNum);
                        if (procForCp != null) //procForCp can be null if cp is for a "By Total" payment (cp.ProcNum=0).
                            //For procs which are not multi visit, ensure the claimproc.ProcDate matches the procedure.ProcDate, to maintain old behavior.
                            cp.ProcDate = procForCp.ProcDate;
                    }
                    else
                    {
                        //Procedure is part of a multi visit group.
                        var listGroupPmvs = listProcMultiVisitsForClaims.FindAll(x => x.GroupProcMultiVisitNum == pmvForProc.GroupProcMultiVisitNum);
                        //Using the maximum group date on the claimproc.ProcDate will cause the insurance billable date to be the delivery date,
                        //while the attached proc.ProcDate will remain unchanged and be the billing date in the account to preserve statement history.
                        var dateForGroup = DateTime.MinValue; //Max ProcDate for the group.
                        foreach (var procMultiVisit in listGroupPmvs)
                        {
                            var procCur = listPatProcs.FirstOrDefault(x => x.ProcNum == procMultiVisit.ProcNum);
                            if (procCur != null && procCur.ProcDate > dateForGroup) //Can be null if the user deleted the proc after the multi visit group was created.
                                dateForGroup = procCur.ProcDate;
                        }

                        //If all procs were deleted from the multi visit group (possible?), then leave the cp.ProcDate alone because it is probably correct.
                        if (dateForGroup > DateTime.MinValue) cp.ProcDate = dateForGroup;
                    }

                    ClaimProcs.Update(cp, cpOld);
                }

                if (listProcMultiVisitsForClaims.Count > 0) //A multi visit claim.
                    //For most dental claims, the ProcDate will be the same for all procedures.
                    //For multiple visit procedures attached to claims, we must send the greatest date of the procedures in the group.
                    //(ex the delivery date of a crown)
                    listClaims[i].DateService = listClaimProcsForClaim.Max(x => x.ProcDate); //Max date of attached procs, after considering group date maximums.
                else //Not a multi visit claim.
                    listClaims[i].DateService = listClaimProcsForClaim[listClaimProcsForClaim.Count - 1].ProcDate; //Last proc from TP sort above.  Not sure why we chose this date.  Old behavior.
            }

            Claims.Update(listClaims[i], claimOld);
            listModifiedClaimNums.Add(listClaims[i].ClaimNum);
        }

        return listModifiedClaimNums;
    }

    /// <summary>
    ///     Updates the group IsInProcess values for all procedures to the specified bool value.
    ///     Does not send cache refresh signal.  Send the signal from calling code.
    /// </summary>
    public static void UpdateInProcessForGroup(long groupProcMultiVisitNum, bool isGroupInProcess)
    {
        var command = "UPDATE procmultivisit "
                      + "SET IsInProcess=" + SOut.Bool(isGroupInProcess) + " "
                      + "WHERE GroupProcMultiVisitNum=" + SOut.Long(groupProcMultiVisitNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Update the parameter GroupProcMultiVisitNum to a new value.
    ///     Does not send cache refresh signal.  Send the signal from calling code.
    /// </summary>
    public static void UpdateGroupProcMultiVisitNumForGroup(long groupProcMultiVisitNumOld, long groupProcMultiVisitNumNew)
    {
        var command = "UPDATE procmultivisit "
                      + "SET GroupProcMultiVisitNum=" + SOut.Long(groupProcMultiVisitNumNew) + " "
                      + "WHERE GroupProcMultiVisitNum=" + SOut.Long(groupProcMultiVisitNumOld);
        Db.NonQ(command);
    }

    #endregion Update

    #region Get Methods

    
    public static List<ProcMultiVisit> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM procmultivisit "
                      + "WHERE PatNum=" + SOut.Long(patNum);
        return ProcMultiVisitCrud.SelectMany(command);
    }

    ///<summary>Calls db to get most up to date information.</summary>
    public static List<ProcMultiVisit> GetGroupsForProcsFromDb(params long[] arrayProcNums)
    {
        if (arrayProcNums.IsNullOrEmpty()) return new List<ProcMultiVisit>();

        var command = "SELECT * FROM procmultivisit "
                      + "WHERE GroupProcMultiVisitNum IN (SELECT GroupProcMultiVisitNum FROM procmultivisit p2 WHERE p2.ProcNum IN (" + string.Join(",", arrayProcNums) + "))";
        return ProcMultiVisitCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Misc Methods

    /// <summary>
    ///     Returns true if the group of multi visits passed in is "In Process".
    ///     This method assumes that all ProcMultiVisits passed in are all of the entities for the group.
    ///     No db call.  Only uses the passed in list.
    /// </summary>
    public static bool IsGroupInProcess(List<ProcMultiVisit> listPmvs)
    {
        //A group is considered "In Process" if at least one procedure is treatment planned and at least one is complete.
        return listPmvs.Exists(x => x.ProcStatus.In(ProcStat.TP, ProcStat.TPi)) && listPmvs.Exists(x => x.ProcStatus.In(ProcStat.C));
    }

    /// <summary>
    ///     Returns true if the group the procedure belongs to is in process and the procedure status is set to "complete".
    ///     If isAssumedComplete, then will pretend the procedure for procNum is complete regardless of current status.
    /// </summary>
    public static bool IsProcInProcess(long procNum, bool isAssumedComplete = false)
    {
        var pmv = GetFirstOrDefault(x => x.ProcNum == procNum);
        //The existience of the procmultivisit in the cache means the procedure's group is In Process.
        if (pmv == null) return false;
        //Now all we need to check is ProcStatus.
        if (isAssumedComplete)
        {
            var listPmvs = GetWhere(x => x.GroupProcMultiVisitNum == pmv.GroupProcMultiVisitNum); //Makes a deep copy.
            for (var i = 0; i < listPmvs.Count; i++)
                if (listPmvs[i].ProcNum == procNum)
                {
                    listPmvs[i].ProcStatus = ProcStat.C; //We can safely change status directly, since GetWhere() returned a deep copy above.
                    break;
                }

            return IsGroupInProcess(listPmvs); //Since are are assuming proc is complete, all we need to know is if the group would be in process or not.
        }

        return pmv.IsInProcess && pmv.ProcStatus == ProcStat.C; //Part of an in process group and is complete.
    }

    #endregion
}