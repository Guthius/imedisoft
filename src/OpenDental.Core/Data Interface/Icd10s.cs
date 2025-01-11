using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Icd10s
{
    //If this table type will exist as cached data, uncomment the CachePattern region below.
    /*
    #region CachePattern

    private class Icd10Cache : CacheListAbs<Icd10> {
        protected override List<Icd10> GetCacheFromDb() {
            string command="SELECT * FROM Icd10 ORDER BY ItemOrder";
            return Crud.Icd10Crud.SelectMany(command);
        }
        protected override List<Icd10> TableToList(DataTable table) {
            return Crud.Icd10Crud.TableToList(table);
        }
        protected override Icd10 Copy(Icd10 Icd10) {
            return Icd10.Clone();
        }
        protected override DataTable ListToTable(List<Icd10> listIcd10s) {
            return Crud.Icd10Crud.ListToTable(listIcd10s,"Icd10");
        }
        protected override void FillCacheIfNeeded() {
            Icd10s.GetTableFromCache(false);
        }
        protected override bool IsInListShort(Icd10 Icd10) {
            return !Icd10.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static Icd10Cache _Icd10Cache=new Icd10Cache();

    ///<summary>A list of all Icd10s. Returns a deep copy.</summary>
    public static List<Icd10> ListDeep {
        get {
            return _Icd10Cache.ListDeep;
        }
    }

    ///<summary>A list of all visible Icd10s. Returns a deep copy.</summary>
    public static List<Icd10> ListShortDeep {
        get {
            return _Icd10Cache.ListShortDeep;
        }
    }

    ///<summary>A list of all Icd10s. Returns a shallow copy.</summary>
    public static List<Icd10> ListShallow {
        get {
            return _Icd10Cache.ListShallow;
        }
    }

    ///<summary>A list of all visible Icd10s. Returns a shallow copy.</summary>
    public static List<Icd10> ListShort {
        get {
            return _Icd10Cache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _Icd10Cache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _Icd10Cache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    
    public static long Insert(Icd10 icd10)
    {
        return Icd10Crud.Insert(icd10);
    }

    
    public static void Update(Icd10 icd10)
    {
        Icd10Crud.Update(icd10);
    }

    public static List<Icd10> GetAll()
    {
        var command = "SELECT * FROM icd10";
        return Icd10Crud.SelectMany(command);
    }

    ///<summary>Returns a list of just the codes for use in update or insert logic.</summary>
    public static List<string> GetAllCodes()
    {
        var listIcd10Codes = new List<string>();
        var command = "SELECT Icd10Code FROM icd10";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listIcd10Codes.Add(table.Rows[i][0].ToString());
        return listIcd10Codes;
    }

    ///<summary>Returns the total number of ICD10 codes.  Some rows in the ICD10 table based on the IsCode column.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM icd10 WHERE IsCode!=0";
        return SIn.Long(Db.GetCount(command));
    }

    ///<summary>Gets one ICD10 object directly from the database by CodeValue.  If code does not exist, returns null.</summary>
    public static Icd10 GetByCode(string Icd10Code)
    {
        var command = "SELECT * FROM icd10 WHERE Icd10Code='" + SOut.String(Icd10Code) + "'";
        return Icd10Crud.SelectOne(command);
    }

    /// <summary>
    ///     Gets all ICD10 objects directly from the database by CodeValues.  If codes don't exist, it will return an
    ///     empty list.
    /// </summary>
    public static List<Icd10> GetByCodes(List<string> listIcd10Codes)
    {
        if (listIcd10Codes == null || listIcd10Codes.Count == 0) return new List<Icd10>();
        var command = "SELECT * FROM icd10 WHERE Icd10Code IN('" + string.Join("','", listIcd10Codes) + "')";
        return Icd10Crud.SelectMany(command);
    }

    ///<summary>Directly from db.</summary>
    public static bool CodeExists(string Icd10Code)
    {
        var command = "SELECT COUNT(*) FROM icd10 WHERE Icd10Code='" + SOut.String(Icd10Code) + "'";
        var count = Db.GetCount(command);
        if (count == "0") return false;
        return true;
    }

    public static List<Icd10> GetBySearchText(string searchText)
    {
        var listSearchTokens = searchText.Split(' ').ToList();
        var command = @"SELECT * FROM icd10 ";
        for (var i = 0; i < listSearchTokens.Count; i++)
        {
            if (i == 0)
                command += "WHERE ";
            else
                command += "AND ";
            command += "(Icd10Code LIKE '%" + SOut.String(listSearchTokens[i]) + "%' OR Description LIKE '%" + SOut.String(listSearchTokens[i]) + "%') ";
        }

        return Icd10Crud.SelectMany(command);
    }

    ///<summary>Gets one Icd10 from the db.</summary>
    public static Icd10 GetOne(long icd10Num)
    {
        return Icd10Crud.SelectOne(icd10Num);
    }

    ///<summary>Returns the code and description of the icd10.</summary>
    public static string GetCodeAndDescription(string icd10Code)
    {
        if (string.IsNullOrEmpty(icd10Code)) return "";

        var icd10 = GetByCode(icd10Code);
        if (icd10 == null) return "";
        return icd10.Icd10Code + "-" + icd10.Description;
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<Icd10> Refresh(long patNum){

        string command="SELECT * FROM icd10 WHERE PatNum = "+POut.Long(patNum);
        return Crud.Icd10Crud.SelectMany(command);
    }

    
    public static void Delete(long icd10Num) {

        string command= "DELETE FROM icd10 WHERE Icd10Num = "+POut.Long(icd10Num);
        Db.NonQ(command);
    }
    */
}