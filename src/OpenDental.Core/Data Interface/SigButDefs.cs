using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SigButDefs
{
    
    public static void Update(SigButDef sigButDef)
    {
        SigButDefCrud.Update(sigButDef);
    }

    
    public static long Insert(SigButDef sigButDef)
    {
        return SigButDefCrud.Insert(sigButDef);
    }

    ///<summary>No need to surround with try/catch, because all deletions are allowed.</summary>
    public static void Delete(SigButDef sigButDef)
    {
        var command = "DELETE FROM sigbutdef WHERE SigButDefNum =" + SOut.Long(sigButDef.SigButDefNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Loops through the SigButDefs passed in and updates the database if any of the ButtonIndexes changed.  Returns
    ///     true if any changes were made to the database so that the calling class can invalidate the cache.
    /// </summary>
    public static bool UpdateButtonIndexIfChanged(SigButDef[] sigButDefArray)
    {
        var hasChanges = false;
        var listSigButDefs = GetDeepCopy();
        for (var i = 0; i < listSigButDefs.Count; i++)
        for (var j = 0; j < sigButDefArray.Length; j++)
        {
            if (listSigButDefs[i].SigButDefNum != sigButDefArray[j].SigButDefNum) continue;

            //This is the same SigButDef
            if (listSigButDefs[i].ButtonIndex != sigButDefArray[j].ButtonIndex)
            {
                hasChanges = true;
                Update(sigButDefArray[j]); //Update the database with the new button index.
            }
        }

        return hasChanges;
    }

    /// <summary>
    ///     Used in Setup.  The returned list also includes defaults.  The supplied computer name can be blank for the
    ///     default setup.
    /// </summary>
    public static SigButDef[] GetByComputer(string computerName)
    {
        var listSigButDefs = GetWhere(x => x.ComputerName == "" || x.ComputerName.ToUpper() == computerName.ToUpper());
        listSigButDefs.Sort(CompareButtonsByIndex);
        return listSigButDefs.ToArray();
    }

    private static int CompareButtonsByIndex(SigButDef sigButDefX, SigButDef sigButDefY)
    {
        if (sigButDefX.ButtonIndex != sigButDefY.ButtonIndex) return sigButDefX.ButtonIndex.CompareTo(sigButDefY.ButtonIndex);
        //we compair y to x here due to a nuance in the way light buttons are drawn. This makes computer specific
        //buttons drawn "on-top-of" the default buttons.
        return sigButDefY.ComputerName.CompareTo(sigButDefX.ComputerName);
    }

    /// <summary>
    ///     Moves the selected item up in the supplied sub list.  Does not update the cache because the user could want to
    ///     potentially move buttons around a lot.
    /// </summary>
    public static List<SigButDef> MoveUp(SigButDef sigButDefSelected, SigButDef[] sigButDefArraySub)
    {
        if (sigButDefSelected.ButtonIndex == 0) //already at top
            return sigButDefArraySub.ToList();
        SigButDef sigButDefOccupied = null;
        var occupiedIdx = -1;
        var selectedIdx = -1;
        for (var i = 0; i < sigButDefArraySub.Length; i++)
        {
            if (sigButDefArraySub[i].SigButDefNum != sigButDefSelected.SigButDefNum //if not the selected object
                && sigButDefArraySub[i].ButtonIndex == sigButDefSelected.ButtonIndex - 1
                && (sigButDefArraySub[i].ComputerName != "" || sigButDefSelected.ComputerName == ""))
            {
                //We want to swap positions with the selected button, which happens if we are moving a default button or moving to a non-default button.
                sigButDefOccupied = sigButDefArraySub[i].Copy();
                occupiedIdx = i;
            }

            if (sigButDefArraySub[i].SigButDefNum == sigButDefSelected.SigButDefNum) selectedIdx = i;
        }

        if (sigButDefOccupied != null) sigButDefArraySub[occupiedIdx].ButtonIndex++;
        sigButDefArraySub[selectedIdx].ButtonIndex--;
        var listSigButDefs = new List<SigButDef>();
        for (var i = 0; i < sigButDefArraySub.Length; i++) listSigButDefs.Add(sigButDefArraySub[i].Copy());
        listSigButDefs.Sort(CompareButtonsByIndex);
        return listSigButDefs;
    }

    /// <summary>
    ///     Moves the selected item down in the supplied sub list.  Does not update the cache because the user could want
    ///     to potentially move buttons around a lot.
    /// </summary>
    public static List<SigButDef> MoveDown(SigButDef sigButDefSelected, SigButDef[] sigButDefArraySub)
    {
        var occupiedIdx = -1;
        var selectedIdx = -1;
        SigButDef sigButDefOccupied = null;
        for (var i = 0; i < sigButDefArraySub.Length; i++)
        {
            if (sigButDefArraySub[i].SigButDefNum != sigButDefSelected.SigButDefNum //if not the selected object
                && sigButDefArraySub[i].ButtonIndex == sigButDefSelected.ButtonIndex + 1
                && (sigButDefArraySub[i].ComputerName != "" || sigButDefSelected.ComputerName == ""))
            {
                //We want to swap positions with the selected button, which happens if we are moving a default button or moving to a non-default button.
                sigButDefOccupied = sigButDefArraySub[i].Copy();
                occupiedIdx = i;
            }

            if (sigButDefArraySub[i].SigButDefNum == sigButDefSelected.SigButDefNum) selectedIdx = i;
        }

        if (sigButDefOccupied != null) sigButDefArraySub[occupiedIdx].ButtonIndex--;
        sigButDefArraySub[selectedIdx].ButtonIndex++;
        var listSigButDefs = new List<SigButDef>();
        for (var i = 0; i < sigButDefArraySub.Length; i++) listSigButDefs.Add(sigButDefArraySub[i].Copy());
        listSigButDefs.Sort(CompareButtonsByIndex);
        ;
        return listSigButDefs;
    }

    /// <summary>
    ///     Returns the SigButDef with the specified buttonIndex.  Used from the setup page.  The supplied list will
    ///     already have been filtered by computername.  Supply buttonIndex in 0-based format.
    /// </summary>
    public static SigButDef GetByIndex(int buttonIndex, List<SigButDef> listSigButDefsSub)
    {
        for (var i = 0; i < listSigButDefsSub.Count; i++)
            if (listSigButDefsSub[i].ButtonIndex == buttonIndex)
                //Will always return a specific computer's button over a default if there are 2 buttons with the same index.  See CompareButtonsByIndex.
                return listSigButDefsSub[i].Copy();

        return null;
    }

    /// <summary>
    ///     Returns the SigButDef with the specified buttonIndex.  Used from the setup page.  The supplied list will
    ///     already have been filtered by computername.  Supply buttonIndex in 0-based format.
    /// </summary>
    public static SigButDef GetByIndex(int buttonIndex, SigButDef[] sigButDefArraySub)
    {
        for (var i = 0; i < sigButDefArraySub.Length; i++)
            if (sigButDefArraySub[i].ButtonIndex == buttonIndex)
                //Will always return a specific computer's button over a default if there are 2 buttons with the same index.  See CompareButtonsByIndex.
                return sigButDefArraySub[i].Copy();

        return null;
    }

    /// <summary>
    ///     A unique synchronization method designed for HQ only. Propagates the messaging buttons for the 'All' computer
    ///     to computers found in the phonecomp table.
    /// </summary>
    public static void SynchTheAllComputerWithPhoneComps()
    {
        ODEvent.Fire(ODEventType.ProgressBar, "Collecting data for synchronization process...");
        var listSigButDefsOld = GetDeepCopy();
        //Find all of the SigButDefs for the 'All' computer (empty ComputerName).
        var listSigButDefsAllComputer = listSigButDefsOld.FindAll(x => string.IsNullOrEmpty(x.ComputerName));
        //Start a new list of SigButDefs for the synch method. Initialize the new list with a shallow copy of the 'All' computer items since they need to stay.
        var listSigButDefsNew = new List<SigButDef>(listSigButDefsAllComputer);
        ODEvent.Fire(ODEventType.ProgressBar, "Synchronizing SigButDef changes to the database...");
        SigButDefCrud.Sync(listSigButDefsNew, listSigButDefsOld);
        ODEvent.Fire(ODEventType.ProgressBar, "Done");
    }

    #region CachePattern

    private class SigButDefCache : CacheListAbs<SigButDef>
    {
        protected override List<SigButDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM sigbutdef ORDER BY ButtonIndex";
            return SigButDefCrud.SelectMany(command);
        }

        protected override List<SigButDef> TableToList(DataTable dataTable)
        {
            return SigButDefCrud.TableToList(dataTable);
        }

        protected override SigButDef Copy(SigButDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SigButDef> items)
        {
            return SigButDefCrud.ListToTable(items, "SigButDef");
        }

        protected override void FillCacheIfNeeded()
        {
            SigButDefs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SigButDefCache _sigButDefCache = new();

    public static List<SigButDef> GetDeepCopy(bool isShort = false)
    {
        return _sigButDefCache.GetDeepCopy(isShort);
    }

    public static List<SigButDef> GetWhere(Predicate<SigButDef> match, bool isShort = false)
    {
        return _sigButDefCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _sigButDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _sigButDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _sigButDefCache.ClearCache();
    }

    #endregion
}