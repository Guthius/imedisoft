using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Hcpcses
{
    //If this table type will exist as cached data, uncomment the CachePattern region below.
    /*
    #region CachePattern

    private class HcpcsCache : CacheListAbs<Hcpcs> {
        protected override List<Hcpcs> GetCacheFromDb() {
            string command="SELECT * FROM Hcpcs ORDER BY ItemOrder";
            return Crud.HcpcsCrud.SelectMany(command);
        }
        protected override List<Hcpcs> TableToList(DataTable table) {
            return Crud.HcpcsCrud.TableToList(table);
        }
        protected override Hcpcs Copy(Hcpcs Hcpcs) {
            return Hcpcs.Clone();
        }
        protected override DataTable ListToTable(List<Hcpcs> listHcpcss) {
            return Crud.HcpcsCrud.ListToTable(listHcpcss,"Hcpcs");
        }
        protected override void FillCacheIfNeeded() {
            Hcpcss.GetTableFromCache(false);
        }
        protected override bool IsInListShort(Hcpcs Hcpcs) {
            return !Hcpcs.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static HcpcsCache _HcpcsCache=new HcpcsCache();

    ///<summary>A list of all Hcpcss. Returns a deep copy.</summary>
    public static List<Hcpcs> ListDeep {
        get {
            return _HcpcsCache.ListDeep;
        }
    }

    ///<summary>A list of all visible Hcpcss. Returns a deep copy.</summary>
    public static List<Hcpcs> ListShortDeep {
        get {
            return _HcpcsCache.ListShortDeep;
        }
    }

    ///<summary>A list of all Hcpcss. Returns a shallow copy.</summary>
    public static List<Hcpcs> ListShallow {
        get {
            return _HcpcsCache.ListShallow;
        }
    }

    ///<summary>A list of all visible Hcpcss. Returns a shallow copy.</summary>
    public static List<Hcpcs> ListShort {
        get {
            return _HcpcsCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _HcpcsCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _HcpcsCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    
    public static long Insert(Hcpcs hcpcs)
    {
        return HcpcsCrud.Insert(hcpcs);
    }

    
    public static void Update(Hcpcs hcpcs)
    {
        HcpcsCrud.Update(hcpcs);
    }

    public static List<Hcpcs> GetAll()
    {
        var command = "SELECT * FROM hcpcs";
        return HcpcsCrud.SelectMany(command);
    }

    ///<summary>Returns a list of just the codes for use in update or insert logic.</summary>
    public static List<string> GetAllCodes()
    {
        var listStringsHcpcsCode = new List<string>();
        var command = "SELECT HcpcsCode FROM hcpcs";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listStringsHcpcsCode.Add(table.Rows[i][0].ToString());
        return listStringsHcpcsCode;
    }

    ///<summary>Returns the total count of HCPCS codes.  HCPCS codes cannot be hidden.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM hcpcs";
        return SIn.Long(Db.GetCount(command));
    }

    ///<summary>Returns the Hcpcs of the code passed in by looking in cache.  If code does not exist, returns null.</summary>
    public static Hcpcs GetByCode(string hcpcsCode)
    {
        var command = "SELECT * FROM hcpcs WHERE HcpcsCode='" + SOut.String(hcpcsCode) + "'";
        return HcpcsCrud.SelectOne(command);
    }

    ///<summary>Directly from db.</summary>
    public static bool CodeExists(string hcpcsCode)
    {
        var command = "SELECT COUNT(*) FROM hcpcs WHERE HcpcsCode='" + SOut.String(hcpcsCode) + "'";
        var count = Db.GetCount(command);
        if (count == "0") return false;
        return true;
    }

    public static List<Hcpcs> GetBySearchText(string searchText)
    {
        var listSearchTokens = searchText.Split(' ').ToList();
        var command = @"SELECT * FROM hcpcs ";
        for (var i = 0; i < listSearchTokens.Count; i++)
            if (i == 0)
                command += "WHERE (HcpcsCode LIKE '%" + SOut.String(listSearchTokens[i]) + "%' OR DescriptionShort LIKE '%" + SOut.String(listSearchTokens[i]) + "%') ";
            else
                command += "AND (HcpcsCode LIKE '%" + SOut.String(listSearchTokens[i]) + "%' OR DescriptionShort LIKE '%" + SOut.String(listSearchTokens[i]) + "%') ";

        return HcpcsCrud.SelectMany(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<Hcpcs> Refresh(long patNum){

        string command="SELECT * FROM hcpcs WHERE PatNum = "+POut.Long(patNum);
        return Crud.HcpcsCrud.SelectMany(command);
    }

    
    public static void Delete(long hcpcsNum) {

        string command= "DELETE FROM hcpcs WHERE HcpcsNum = "+POut.Long(hcpcsNum);
        Db.NonQ(command);
    }
    */
}