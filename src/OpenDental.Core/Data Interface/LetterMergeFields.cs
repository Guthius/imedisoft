using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LetterMergeFields
{
    ///<summary>Inserts this lettermergefield into database.</summary>
    public static long Insert(LetterMergeField letterMergeField)
    {
        return LetterMergeFieldCrud.Insert(letterMergeField);
    }

    /*
    
    public void Update(){
        string command="UPDATE lettermergefield SET "
            +"LetterMergeNum = '"+POut.PInt   (LetterMergeNum)+"' "
            +",FieldName = '"    +POut.PString(FieldName)+"' "
            +"WHERE FieldNum = '"+POut.PInt(FieldNum)+"'";
        DataConnection dcon=new DataConnection();
        Db.NonQ(command);
    }*/

    /*
    
    public void Delete(){
        string command="DELETE FROM lettermergefield "
            +"WHERE FieldNum = "+POut.PInt(FieldNum);
        DataConnection dcon=new DataConnection();
        Db.NonQ(command);
    }*/

    /// <summary>
    ///     Called from LetterMerge.Refresh() to get all field names for a given letter.  The result is a collection of
    ///     strings representing field names.
    /// </summary>
    public static List<string> GetForLetter(long letterMergeNum)
    {
        return GetWhere(x => x.LetterMergeNum == letterMergeNum).Select(x => x.FieldName).ToList();
    }

    /// <summary>
    ///     Deletes all lettermergefields for the given letter.  This is then followed by adding them all back, which is
    ///     simpler than just updating.
    /// </summary>
    public static void DeleteForLetter(long letterMergeNum)
    {
        var command = "DELETE FROM lettermergefield "
                      + "WHERE LetterMergeNum = " + SOut.Long(letterMergeNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class LetterMergeFieldCache : CacheListAbs<LetterMergeField>
    {
        protected override List<LetterMergeField> GetCacheFromDb()
        {
            var command = "SELECT * FROM lettermergefield ORDER BY FieldName";
            return LetterMergeFieldCrud.SelectMany(command);
        }

        protected override List<LetterMergeField> TableToList(DataTable dataTable)
        {
            return LetterMergeFieldCrud.TableToList(dataTable);
        }

        protected override LetterMergeField Copy(LetterMergeField item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<LetterMergeField> items)
        {
            return LetterMergeFieldCrud.ListToTable(items, "LetterMergeField");
        }

        protected override void FillCacheIfNeeded()
        {
            LetterMergeFields.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly LetterMergeFieldCache _letterMergeFieldCache = new();

    public static List<LetterMergeField> GetWhere(Predicate<LetterMergeField> match, bool isShort = false)
    {
        return _letterMergeFieldCache.GetWhere(match, isShort);
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
        _letterMergeFieldCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _letterMergeFieldCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _letterMergeFieldCache.ClearCache();
    }

    #endregion
}