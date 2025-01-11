using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EClipboardSheetDefs
{
    #region Insert

    
    public static long Insert(EClipboardSheetDef eClipboardSheetDef)
    {
        return EClipboardSheetDefCrud.Insert(eClipboardSheetDef);
    }

    #endregion Insert

    #region Update

    
    public static void Update(EClipboardSheetDef eClipboardSheetDef)
    {
        EClipboardSheetDefCrud.Update(eClipboardSheetDef);
    }

    #endregion Update

    #region Delete

    
    public static void Delete(long eClipboardSheetDefNum)
    {
        EClipboardSheetDefCrud.Delete(eClipboardSheetDefNum);
    }

    #endregion Delete

    #region Misc Methods

    /// <summary>
    ///     Inserts, updates, or deletes db rows to match listNew.  No need to pass in userNum, it's set before remoting role
    ///     check and passed to
    ///     the server if necessary.
    /// </summary>
    public static bool Sync(List<EClipboardSheetDef> listEClipboardSheetDefsNew, List<EClipboardSheetDef> listEClipboardSheetDefsOld)
    {
        return EClipboardSheetDefCrud.Sync(listEClipboardSheetDefsNew, listEClipboardSheetDefsOld);
    }

    #endregion Misc Methods

    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.
    /*
    #region Cache Pattern
    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class EClipboardSheetDefCache : CacheListAbs<EClipboardSheetDef> {
        protected override List<EClipboardSheetDef> GetCacheFromDb() {
            string command="SELECT * FROM eclipboardsheetdef";
            return Crud.EClipboardSheetDefCrud.SelectMany(command);
        }
        protected override List<EClipboardSheetDef> TableToList(DataTable table) {
            return Crud.EClipboardSheetDefCrud.TableToList(table);
        }
        protected override EClipboardSheetDef Copy(EClipboardSheetDef eClipboardSheetDef) {
            return eClipboardSheetDef.Copy();
        }
        protected override DataTable ListToTable(List<EClipboardSheetDef> listEClipboardSheetDefs) {
            return Crud.EClipboardSheetDefCrud.ListToTable(listEClipboardSheetDefs,"EClipboardSheetDef");
        }
        protected override void FillCacheIfNeeded() {
            EClipboardSheetDefs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static EClipboardSheetDefCache _eClipboardSheetDefCache=new EClipboardSheetDefCache();

    public static List<EClipboardSheetDef> GetDeepCopy(bool isShort=false) {
        return _eClipboardSheetDefCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort=false) {
        return _eClipboardSheetDefCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<EClipboardSheetDef> match,bool isShort=false) {
        return _eClipboardSheetDefCache.GetExists(match,isShort);
    }

    public static int GetFindIndex(Predicate<EClipboardSheetDef> match,bool isShort=false) {
        return _eClipboardSheetDefCache.GetFindIndex(match,isShort);
    }

    public static EClipboardSheetDef GetFirst(bool isShort=false) {
        return _eClipboardSheetDefCache.GetFirst(isShort);
    }

    public static EClipboardSheetDef GetFirst(Func<EClipboardSheetDef,bool> match,bool isShort=false) {
        return _eClipboardSheetDefCache.GetFirst(match,isShort);
    }

    public static EClipboardSheetDef GetFirstOrDefault(Func<EClipboardSheetDef,bool> match,bool isShort=false) {
        return _eClipboardSheetDefCache.GetFirstOrDefault(match,isShort);
    }

    public static EClipboardSheetDef GetLast(bool isShort=false) {
        return _eClipboardSheetDefCache.GetLast(isShort);
    }

    public static EClipboardSheetDef GetLastOrDefault(Func<EClipboardSheetDef,bool> match,bool isShort=false) {
        return _eClipboardSheetDefCache.GetLastOrDefault(match,isShort);
    }

    public static List<EClipboardSheetDef> GetWhere(Predicate<EClipboardSheetDef> match,bool isShort=false) {
        return _eClipboardSheetDefCache.GetWhere(match,isShort);
    }

    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _eClipboardSheetDefCache.FillCacheFromTable(table);
    }

    ///<summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    ///<param name="doRefreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _eClipboardSheetDefCache.GetTableFromCache(doRefreshCache);
    }
    #endregion Cache Pattern
    */

    #region Get Methods

    
    public static List<EClipboardSheetDef> Refresh()
    {
        var command = "SELECT * FROM eclipboardsheetdef";
        return EClipboardSheetDefCrud.SelectMany(command);
    }

    ///<summary>Gets one EClipboardSheetDef from the db.</summary>
    public static EClipboardSheetDef GetOne(long eClipboardSheetDefNum)
    {
        return EClipboardSheetDefCrud.SelectOne(eClipboardSheetDefNum);
    }

    public static List<EClipboardSheetDef> GetForClinic(long clinicNum)
    {
        var command = "SELECT * FROM eclipboardsheetdef WHERE ClinicNum=" + SOut.Long(clinicNum);
        return EClipboardSheetDefCrud.SelectMany(command);
    }

    public static bool IsSheetDefInUse(long sheetDefNum)
    {
        var command = "SELECT COUNT(*) FROM eclipboardsheetdef WHERE SheetDefNum=" + SOut.Long(sheetDefNum);
        return SIn.Int(Db.GetCount(command)) > 0;
    }

    public static bool IsEFormDefInUse(long eFormDefNum)
    {
        var command = "SELECT COUNT(*) FROM eclipboardsheetdef WHERE EFormDefNum=" + SOut.Long(eFormDefNum);
        return SIn.Int(Db.GetCount(command)) > 0;
    }

    public static List<EClipboardSheetDef> GetAllForSheetDefForOnceRule(long sheetDefNum)
    {
        var command = "SELECT * FROM eclipboardsheetdef WHERE SheetDefNum=" + SOut.Long(sheetDefNum) + " AND Frequency = " + SOut.Enum(EnumEClipFreq.Once);
        return EClipboardSheetDefCrud.SelectMany(command);
    }

    public static List<long> GetListIgnoreSheetDefNums(EClipboardSheetDef eClipboardSheetDef)
    {
        if (eClipboardSheetDef == null || eClipboardSheetDef.IgnoreSheetDefNums == null) return new List<long>();
        return eClipboardSheetDef.IgnoreSheetDefNums.Split(',').Select(x => SIn.Long(x)).ToList();
    }

    /// <summary>
    ///     Returns a list of EClipboardSheetDefs for the passed in sheetDefNums at the specified clinic. Only includes
    ///     when PrefillStatus=Once
    /// </summary>
    public static List<EClipboardSheetDef> GetManyEClipboardSheetDefsForOnceRuleAtClinic(List<long> listSheetDefNums, long clinicNum)
    {
        if (listSheetDefNums == null || listSheetDefNums.Count == 0) return new List<EClipboardSheetDef>();

        var command = "SELECT * FROM eclipboardsheetdef WHERE SheetDefNum IN (" + string.Join(",", listSheetDefNums) + ") "
                      + "AND ClinicNum=" + SOut.Long(clinicNum) + " AND Frequency = " + SOut.Enum(EnumEClipFreq.Once);
        return EClipboardSheetDefCrud.SelectMany(command);
    }

    /// <summary>
    ///     Filters out and returns a list of EClipboardSheetDefs based on ignore fields. Considers completed sheets,
    ///     existing sheets, and sheets to be generated. ClinicNum is from appointment.
    /// </summary>
    public static List<EClipboardSheetDef> FilterPrefillStatuses(List<EClipboardSheetDef> listEClipboardSheetDefs, List<Sheet> listSheetsCompleted, long clinicNum, List<Sheet> listSheetsInTerminal = null)
    {
        var listEClipboardSheetDefsRet = new List<EClipboardSheetDef>(listEClipboardSheetDefs);
        var listSheetDefNumsToRemove = new List<long>();
        //Remove any sheet defs that are set to EnumEClipFreq.Once and have been filled out or have a sheetdef filled out with a revision greathan or equal to the prefillstatusoverride revision id. 
        listEClipboardSheetDefsRet.RemoveAll(x => x.Frequency == EnumEClipFreq.Once && listSheetsCompleted.Any(y => y.SheetDefNum == x.SheetDefNum && y.RevID >= x.PrefillStatusOverride));
        //Remove any sheets in a EclipboardSheetDefs ignore list that are set to be filled out once.
        for (var i = 0; i < listEClipboardSheetDefsRet.Count; i++)
        {
            //If the prefill status is not set to once, then ignore lists are not used.
            if (listEClipboardSheetDefsRet[i].Frequency != EnumEClipFreq.Once) continue;
            var listSheetDefNumsIgnore = GetListIgnoreSheetDefNums(listEClipboardSheetDefsRet[i]);
            listSheetDefNumsToRemove.AddRange(listSheetDefNumsIgnore);
        }

        if (listSheetsInTerminal != null && listSheetsInTerminal.Count > 0)
        {
            //We now have the sheets to ignore from the newly adding sheet defs, now check the ones that exist in the terminal already.
            //If the office is using eClipboard default clinic settings for all clinics, use 0 regardless of what clinic their appointment is at.
            if (clinicNum > 0) clinicNum = ClinicPrefs.GetBool(PrefName.EClipboardUseDefaults, clinicNum) ? 0 : clinicNum;
            //Fetch the EClipboardSheetDefs that already exist, for the clinic the patient is checking in to.
            var listEClipboardSheetDefsForTerminal = GetManyEClipboardSheetDefsForOnceRuleAtClinic(listSheetsInTerminal.Select(x => x.SheetDefNum).ToList(), clinicNum);
            //Loop through existing sheets, concatenating their SheetsToIgnore fields.
            for (var i = 0; i < listEClipboardSheetDefsForTerminal.Count; i++) listSheetDefNumsToRemove.AddRange(GetListIgnoreSheetDefNums(listEClipboardSheetDefsForTerminal[i]));
        }

        //Remove all of the eClipboardSheetDefs from our result that are ignored by one of the sheets in the checklist.
        listEClipboardSheetDefsRet.RemoveAll(x => listSheetDefNumsToRemove.Distinct().Contains(x.SheetDefNum));
        return listEClipboardSheetDefsRet;
    }

    #endregion Get Methods
}