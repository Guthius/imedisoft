using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class HL7DefFields
{
    /// <summary>Gets it straight from the database instead of from cache.</summary>
    public static List<HL7DefField> GetFromDb(long hl7DefSegmentNum)
    {
        var command = "SELECT * FROM hl7deffield WHERE HL7DefSegmentNum='" + SOut.Long(hl7DefSegmentNum) + "' ORDER BY OrdinalPos";
        return HL7DefFieldCrud.SelectMany(command);
    }

    /// <summary>Gets the field list from the cache.</summary>
    public static List<HL7DefField> GetFromCache(long hl7DefSegmentNum)
    {
        return GetWhere(x => x.HL7DefSegmentNum == hl7DefSegmentNum);
    }

    
    public static long Insert(HL7DefField hL7DefField)
    {
        return HL7DefFieldCrud.Insert(hL7DefField);
    }

    
    public static void Update(HL7DefField hL7DefField)
    {
        HL7DefFieldCrud.Update(hL7DefField);
    }

    
    public static void Delete(long hL7DefFieldNum)
    {
        var command = "DELETE FROM hl7deffield WHERE HL7DefFieldNum = " + SOut.Long(hL7DefFieldNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class HL7DefFieldCache : CacheListAbs<HL7DefField>
    {
        protected override List<HL7DefField> GetCacheFromDb()
        {
            var command = "SELECT * FROM hl7deffield ORDER BY OrdinalPos";
            return HL7DefFieldCrud.SelectMany(command);
        }

        protected override List<HL7DefField> TableToList(DataTable dataTable)
        {
            return HL7DefFieldCrud.TableToList(dataTable);
        }

        protected override HL7DefField Copy(HL7DefField item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<HL7DefField> items)
        {
            return HL7DefFieldCrud.ListToTable(items, "HL7DefField");
        }

        protected override void FillCacheIfNeeded()
        {
            HL7DefFields.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly HL7DefFieldCache _HL7DefFieldCache = new();

    public static List<HL7DefField> GetWhere(Predicate<HL7DefField> match, bool isShort = false)
    {
        return _HL7DefFieldCache.GetWhere(match, isShort);
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
        _HL7DefFieldCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _HL7DefFieldCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _HL7DefFieldCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<HL7DefField> Refresh(long patNum){

        string command="SELECT * FROM hl7deffield WHERE PatNum = "+POut.Long(patNum);
        return Crud.HL7DefFieldCrud.SelectMany(command);
    }

    ///<summary>Gets one HL7DefField from the db.</summary>
    public static HL7DefField GetOne(long hL7DefFieldNum){

        return Crud.HL7DefFieldCrud.SelectOne(hL7DefFieldNum);
    }

    */
}