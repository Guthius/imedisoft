using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class FeeSchedNotes
{
    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.
    /*
    #region Cache Pattern
    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class FeeSchedNoteCache : CacheListAbs<FeeSchedNote> {
        protected override List<FeeSchedNote> GetCacheFromDb() {
            string command="SELECT * FROM feeschednote";
            return Crud.FeeSchedNoteCrud.SelectMany(command);
        }
        protected override List<FeeSchedNote> TableToList(DataTable table) {
            return Crud.FeeSchedNoteCrud.TableToList(table);
        }
        protected override FeeSchedNote Copy(FeeSchedNote feeSchedNote) {
            return feeSchedNote.Copy();
        }
        protected override DataTable ListToTable(List<FeeSchedNote> listFeeSchedNotes) {
            return Crud.FeeSchedNoteCrud.ListToTable(listFeeSchedNotes,"FeeSchedNote");
        }
        protected override void FillCacheIfNeeded() {
            FeeSchedNotes.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static FeeSchedNoteCache _feeSchedNoteCache=new FeeSchedNoteCache();

    public static void ClearCache() {
        _feeSchedNoteCache.ClearCache();
    }

    public static List<FeeSchedNote> GetDeepCopy(bool isShort=false) {
        return _feeSchedNoteCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort=false) {
        return _feeSchedNoteCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<FeeSchedNote> match,bool isShort=false) {
        return _feeSchedNoteCache.GetExists(match,isShort);
    }

    public static int GetFindIndex(Predicate<FeeSchedNote> match,bool isShort=false) {
        return _feeSchedNoteCache.GetFindIndex(match,isShort);
    }

    public static FeeSchedNote GetFirst(bool isShort=false) {
        return _feeSchedNoteCache.GetFirst(isShort);
    }

    public static FeeSchedNote GetFirst(Func<FeeSchedNote,bool> match,bool isShort=false) {
        return _feeSchedNoteCache.GetFirst(match,isShort);
    }

    public static FeeSchedNote GetFirstOrDefault(Func<FeeSchedNote,bool> match,bool isShort=false) {
        return _feeSchedNoteCache.GetFirstOrDefault(match,isShort);
    }

    public static FeeSchedNote GetLast(bool isShort=false) {
        return _feeSchedNoteCache.GetLast(isShort);
    }

    public static FeeSchedNote GetLastOrDefault(Func<FeeSchedNote,bool> match,bool isShort=false) {
        return _feeSchedNoteCache.GetLastOrDefault(match,isShort);
    }

    public static List<FeeSchedNote> GetWhere(Predicate<FeeSchedNote> match,bool isShort=false) {
        return _feeSchedNoteCache.GetWhere(match,isShort);
    }

    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _feeSchedNoteCache.FillCacheFromTable(table);
    }

    ///<summary>Returns the cache in the form of a DataTable. Always refreshes the ClientMT's cache.</summary>
    ///<param name="doRefreshCache">If true, will refresh the cache if MiddleTierRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _feeSchedNoteCache.GetTableFromCache(doRefreshCache);
    }
    #endregion Cache Pattern
    */

    #region Methods - Get

    
    public static List<FeeSchedNote> Refresh(long patNum)
    {
        var command = "SELECT * FROM feeschednote WHERE PatNum = " + SOut.Long(patNum);
        return FeeSchedNoteCrud.SelectMany(command);
    }

    ///<summary>Gets one FeeSchedNote from the db.</summary>
    public static FeeSchedNote GetOne(long feeSchedNoteNum)
    {
        return FeeSchedNoteCrud.SelectOne(feeSchedNoteNum);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(FeeSchedNote feeSchedNote)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        feeSchedNote.SecUserNumEntry = Security.CurUser.UserNum;
        return FeeSchedNoteCrud.Insert(feeSchedNote);
    }

    
    public static void Update(FeeSchedNote feeSchedNote)
    {
        FeeSchedNoteCrud.Update(feeSchedNote);
    }

    
    public static void Delete(long feeSchedNoteNum)
    {
        FeeSchedNoteCrud.Delete(feeSchedNoteNum);
    }

    #endregion Methods - Modify

    #region Methods - Misc

    ///<summary>Used when feeSched.IsGlobal is true, or any time you want all notes regardless of clinic.</summary>
    public static List<FeeSchedNote> GetNotesForGlobal(long feeSchedNum)
    {
        var command = "Select * from feeschednote where feeSchedNum=" + feeSchedNum;
        var table = DataCore.GetTable(command);
        return FeeSchedNoteCrud.TableToList(table);
    }

    ///<summary>Converts a list of clinic nums to a comma delimited string of clinic nums.</summary>
    public static string ConvertClinicNumsToString(List<long> listClinicNums)
    {
        var str = string.Join(",", listClinicNums);
        return str;
    }

    ///<summary>Converts a comma delimited string of clinic nums to a list of clinic nums.</summary>
    public static List<long> ConvertStringToClinicsNums(string s)
    {
        var stringClinicNums = s.Split(',').ToList();
        var listClinicNums = new List<long>();
        for (var i = 0; i < stringClinicNums.Count; i++)
        {
            if (stringClinicNums[i].IsNullOrEmpty()) break;
            listClinicNums.Add(long.Parse(stringClinicNums[i]));
        }

        return listClinicNums;
    }

    ///<summary>Converts an entire fee schedule note list's ClinicNums to populate the ListClinicNums field.</summary>
    public static List<FeeSchedNote> ConvertListStringsToClinicNums(List<FeeSchedNote> listFeeSchedNotes)
    {
        for (var i = 0; i < listFeeSchedNotes.Count; i++) listFeeSchedNotes[i].ListClinicNums = ConvertStringToClinicsNums(listFeeSchedNotes[i].ClinicNums);
        return listFeeSchedNotes;
    }

    #endregion Methods - Misc
}