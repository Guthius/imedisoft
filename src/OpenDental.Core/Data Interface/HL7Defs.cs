using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.HL7;

namespace OpenDentBusiness;


public class HL7Defs
{
    ///<summary>Gets an internal HL7Def from the database of the specified type.</summary>
    public static HL7Def GetInternalFromDb(HL7InternalType hL7InternalType)
    {
        var command = "SELECT * FROM hl7def WHERE IsInternal=1 "
                      + "AND InternalType='" + SOut.String(hL7InternalType.ToString()) + "'";
        return HL7DefCrud.SelectOne(command);
    }

    public static List<HL7Def> GetListInternalFromDb()
    {
        var command = "SELECT * FROM hl7def WHERE IsInternal=1";
        return HL7DefCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets from cache.  Will get all enabled defs that are not InternalType HL7InternalType.MedLabv2_3.
    ///     Only one def that is not MedLabv2_3 can be enabled so this is guaranteed to return only one def.
    /// </summary>
    public static HL7Def GetOneDeepEnabled()
    {
        return GetOneDeepEnabled(false);
    }

    /// <summary>
    ///     Gets from cache.  If isMedLabHL7 is true, this will only return the enabled def if it is
    ///     HL7InternalType.MedLabv2_3.
    ///     If false, then only those defs not of that type.  This will return null if no HL7defs are enabled.  Since only one
    ///     can be enabled,
    ///     this will return only one.  No need to check MiddleTierRole, cache is filled by calling GetTableRemotelyIfNeeded.
    /// </summary>
    public static HL7Def GetOneDeepEnabled(bool isMedLabHL7)
    {
        var hL7DefRet = GetFirstOrDefault(x => x.IsEnabled && isMedLabHL7 == (x.InternalType == HL7InternalType.MedLabv2_3));
        if (hL7DefRet == null) return null;
        if (hL7DefRet.IsInternal) //if internal, messages, segments, and fields will not be in the database
            GetDeepForInternal(hL7DefRet);
        else
            hL7DefRet.hl7DefMessages = HL7DefMessages.GetDeepFromCache(hL7DefRet.HL7DefNum);
        return hL7DefRet;
    }

    ///<summary>Gets a full deep list of all internal defs.  If one is enabled, then it might be in database.</summary>
    public static List<HL7Def> GetDeepInternalList()
    {
        var listHL7DefsInternalDb = GetListInternalFromDb();
        var listHL7DefsRet = new List<HL7Def>();
        HL7Def hL7Def;
        //Whether or not the def was in the db, internal def messages, segments, and fields will not be in the db.  GetDeep from C# code
        var listHL7InternalTypes = Enum.GetValues(typeof(HL7InternalType)).Cast<HL7InternalType>().ToList();
        for (var i = 0; i < listHL7InternalTypes.Count; i++)
        {
            hL7Def = listHL7DefsInternalDb.Find(x => x.InternalType == listHL7InternalTypes[i]); //might be null
            switch (listHL7InternalTypes[i])
            {
                case HL7InternalType.eCWFull:
                    listHL7DefsRet.Add(InternalEcwFull.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.eCWStandalone:
                    listHL7DefsRet.Add(InternalEcwStandalone.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.eCWTight:
                    listHL7DefsRet.Add(InternalEcwTight.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.Centricity:
                    listHL7DefsRet.Add(InternalCentricity.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.HL7v2_6:
                    listHL7DefsRet.Add(InternalHL7v2_6.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.MedLabv2_3:
                    listHL7DefsRet.Add(MedLabv2_3.GetDeepInternal(hL7Def));
                    continue;
                default:
                    continue;
            }
        }

        return listHL7DefsRet;
    }

    ///<summary>Gets from C# internal code rather than db</summary>
    private static void GetDeepForInternal(HL7Def hL7Def)
    {
        if (hL7Def.InternalType == HL7InternalType.eCWFull)
            hL7Def = InternalEcwFull.GetDeepInternal(hL7Def); //def that we're passing in is guaranteed to not be null
        else if (hL7Def.InternalType == HL7InternalType.eCWStandalone)
            hL7Def = InternalEcwStandalone.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.eCWTight)
            hL7Def = InternalEcwTight.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.Centricity)
            hL7Def = InternalCentricity.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.HL7v2_6)
            hL7Def = InternalHL7v2_6.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.MedLabv2_3) hL7Def = MedLabv2_3.GetDeepInternal(hL7Def);
        //no need to return a def because the original reference won't have been lost.
    }

    /// <summary>
    ///     Tells us whether there is an existing enabled HL7Def, excluding the def with excludeHL7DefNum.
    ///     If isMedLabHL7 is true, this will only check to see if a def of type HL7InternalType.MedLabv2_3 is enabled.
    ///     Otherwise, only defs not of that type will be checked.
    /// </summary>
    public static bool IsExistingHL7Enabled(long hL7DefNumExclude, bool isMedLabHL7)
    {
        var command = "SELECT COUNT(*) FROM hl7def WHERE IsEnabled=1 AND HL7DefNum != " + SOut.Long(hL7DefNumExclude);
        if (isMedLabHL7)
            command += " AND InternalType='" + SOut.String(HL7InternalType.MedLabv2_3.ToString()) + "'";
        else
            command += " AND InternalType!='" + SOut.String(HL7InternalType.MedLabv2_3.ToString()) + "'";
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    ///<summary>Tells us whether there is an existing enabled HL7Def that is not HL7InternalType.MedLabv2_3.</summary>
    public static bool IsExistingHL7Enabled()
    {
        return _HL7DefCache.GetWhere(x => x.IsEnabled && x.InternalType != HL7InternalType.MedLabv2_3).Count > 0;
    }

    ///<summary>Gets a full deep list of all defs that are not internal from the database.</summary>
    public static List<HL7Def> GetDeepCustomList()
    {
        var listHL7Defs = GetShallowFromDb();
        for (var d = 0; d < listHL7Defs.Count; d++) listHL7Defs[d].hl7DefMessages = HL7DefMessages.GetDeepFromDb(listHL7Defs[d].HL7DefNum);
        return listHL7Defs;
    }

    ///<summary>Gets shallow list of all defs that are not internal from the database</summary>
    public static List<HL7Def> GetShallowFromDb()
    {
        var command = "SELECT * FROM hl7def WHERE IsInternal=0";
        return HL7DefCrud.SelectMany(command);
    }

    /// <summary>
    ///     Only used from Unit Tests.  Since we clear the db of hl7Defs we have to insert this internal def not update
    ///     it.
    /// </summary>
    public static void EnableInternalForTests(HL7InternalType hL7InternalType)
    {
        HL7Def hl7Def = null;
        var listHL7Defs = GetDeepInternalList();
        for (var i = 0; i < listHL7Defs.Count; i++)
            if (listHL7Defs[i].InternalType == hL7InternalType)
            {
                hl7Def = listHL7Defs[i];
                break;
            }

        if (hl7Def == null) return;
        hl7Def.IsEnabled = true;
        Insert(hl7Def);
    }

    
    public static long Insert(HL7Def hL7Def)
    {
        return HL7DefCrud.Insert(hL7Def);
    }

    
    public static void Update(HL7Def hL7Def)
    {
        HL7DefCrud.Update(hL7Def);
    }

    
    public static void Delete(long hL7DefNum)
    {
        var command = "DELETE FROM hl7def WHERE HL7DefNum = " + SOut.Long(hL7DefNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class HL7DefCache : CacheListAbs<HL7Def>
    {
        protected override List<HL7Def> GetCacheFromDb()
        {
            var command = "SELECT * FROM hl7def ORDER BY Description";
            return HL7DefCrud.SelectMany(command);
        }

        protected override List<HL7Def> TableToList(DataTable dataTable)
        {
            return HL7DefCrud.TableToList(dataTable);
        }

        protected override HL7Def Copy(HL7Def item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<HL7Def> items)
        {
            return HL7DefCrud.ListToTable(items, "HL7Def");
        }

        protected override void FillCacheIfNeeded()
        {
            HL7Defs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly HL7DefCache _HL7DefCache = new();

    public static HL7Def GetFirstOrDefault(Func<HL7Def, bool> match, bool isShort = false)
    {
        return _HL7DefCache.GetFirstOrDefault(match, isShort);
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
        _HL7DefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _HL7DefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _HL7DefCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<HL7Def> Refresh(long patNum){

        string command="SELECT * FROM hl7def WHERE PatNum = "+POut.Long(patNum);
        return Crud.HL7DefCrud.SelectMany(command);
    }

    ///<summary>Gets one HL7Def from the db.</summary>
    public static HL7Def GetOne(long hL7DefNum){

        return Crud.HL7DefCrud.SelectOne(hL7DefNum);
    }




    */
}