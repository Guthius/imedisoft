using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Cvxs
{
    //If this table type will exist as cached data, uncomment the CachePattern region below.
    /*
    #region CachePattern

    private class CvxCache : CacheListAbs<Cvx> {
        protected override List<Cvx> GetCacheFromDb() {
            string command="SELECT * FROM Cvx ORDER BY ItemOrder";
            return Crud.CvxCrud.SelectMany(command);
        }
        protected override List<Cvx> TableToList(DataTable table) {
            return Crud.CvxCrud.TableToList(table);
        }
        protected override Cvx Copy(Cvx Cvx) {
            return Cvx.Clone();
        }
        protected override DataTable ListToTable(List<Cvx> listCvxs) {
            return Crud.CvxCrud.ListToTable(listCvxs,"Cvx");
        }
        protected override void FillCacheIfNeeded() {
            Cvxs.GetTableFromCache(false);
        }
        protected override bool IsInListShort(Cvx Cvx) {
            return !Cvx.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static CvxCache _CvxCache=new CvxCache();

    ///<summary>A list of all Cvxs. Returns a deep copy.</summary>
    public static List<Cvx> ListDeep {
        get {
            return _CvxCache.ListDeep;
        }
    }

    ///<summary>A list of all visible Cvxs. Returns a deep copy.</summary>
    public static List<Cvx> ListShortDeep {
        get {
            return _CvxCache.ListShortDeep;
        }
    }

    ///<summary>A list of all Cvxs. Returns a shallow copy.</summary>
    public static List<Cvx> ListShallow {
        get {
            return _CvxCache.ListShallow;
        }
    }

    ///<summary>A list of all visible Cvxs. Returns a shallow copy.</summary>
    public static List<Cvx> ListShort {
        get {
            return _CvxCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _CvxCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _CvxCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    
    public static long Insert(Cvx cvx)
    {
        return CvxCrud.Insert(cvx);
    }

    
    public static void Update(Cvx cvx)
    {
        CvxCrud.Update(cvx);
    }

    public static List<Cvx> GetAll()
    {
        var command = "SELECT * FROM cvx";
        return CvxCrud.SelectMany(command);
    }

    ///<summary>Gets one Cvx object directly from the database by CodeValue.  If code does not exist, returns null.</summary>
    public static Cvx GetByCode(string cvxCode)
    {
        var command = "SELECT * FROM Cvx WHERE CvxCode='" + SOut.String(cvxCode) + "'";
        return CvxCrud.SelectOne(command);
    }

    ///<summary>Gets one Cvx by CvxNum directly from the db.</summary>
    public static Cvx GetOneFromDb(string cvxCode)
    {
        var command = "SELECT * FROM cvx WHERE CvxCode='" + SOut.String(cvxCode) + "'";
        return CvxCrud.SelectOne(command);
    }

    ///<summary>Directly from db.</summary>
    public static bool CodeExists(string cvxCode)
    {
        var command = "SELECT COUNT(*) FROM cvx WHERE CvxCode='" + SOut.String(cvxCode) + "'";
        var count = Db.GetCount(command);
        if (count == "0") return false;
        return true;
    }

    /// <summary>
    ///     Returns the total count of CVX codes.  CVS codes cannot be hidden, but might in the future be set
    ///     active/inactive using the IsActive flag.
    /// </summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM cvx";
        return SIn.Long(Db.GetCount(command));
    }

    public static List<Cvx> GetBySearchText(string searchText)
    {
        var listSearchTokens = searchText.Split(' ').ToList();
        var command = @"SELECT * FROM cvx WHERE ";
        for (var i = 0; i < listSearchTokens.Count; i++)
        {
            if (i > 0) command += "AND ";
            command += "(CvxCode LIKE '%" + SOut.String(listSearchTokens[i]) + "%' OR Description LIKE '%" + SOut.String(listSearchTokens[i]) + "%') ";
        }

        return CvxCrud.SelectMany(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<Cvx> Refresh(long patNum){

        string command="SELECT * FROM cvx WHERE PatNum = "+POut.Long(patNum);
        return Crud.CvxCrud.SelectMany(command);
    }

    ///<summary>Gets one Cvx from the db.</summary>
    public static Cvx GetOne(long cvxNum){

        return Crud.CvxCrud.SelectOne(cvxNum);
    }

    
    public static void Delete(long cvxNum) {

        string command= "DELETE FROM cvx WHERE CvxNum = "+POut.Long(cvxNum);
        Db.NonQ(command);
    }
    */
}