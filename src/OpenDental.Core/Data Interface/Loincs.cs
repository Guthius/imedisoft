using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Loincs
{
    /*
    #region CachePattern

    private class LoincCache : CacheListAbs<Loinc> {
        protected override List<Loinc> GetCacheFromDb() {
            string command="SELECT * FROM Loinc ORDER BY ItemOrder";
            return Crud.LoincCrud.SelectMany(command);
        }
        protected override List<Loinc> TableToList(DataTable table) {
            return Crud.LoincCrud.TableToList(table);
        }
        protected override Loinc Copy(Loinc Loinc) {
            return Loinc.Clone();
        }
        protected override DataTable ListToTable(List<Loinc> listLoincs) {
            return Crud.LoincCrud.ListToTable(listLoincs,"Loinc");
        }
        protected override void FillCacheIfNeeded() {
            Loincs.GetTableFromCache(false);
        }
        protected override bool IsInListShort(Loinc Loinc) {
            return !Loinc.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static LoincCache _LoincCache=new LoincCache();

    ///<summary>A list of all Loincs. Returns a deep copy.</summary>
    public static List<Loinc> ListDeep {
        get {
            return _LoincCache.ListDeep;
        }
    }

    ///<summary>A list of all visible Loincs. Returns a deep copy.</summary>
    public static List<Loinc> ListShortDeep {
        get {
            return _LoincCache.ListShortDeep;
        }
    }

    ///<summary>A list of all Loincs. Returns a shallow copy.</summary>
    public static List<Loinc> ListShallow {
        get {
            return _LoincCache.ListShallow;
        }
    }

    ///<summary>A list of all visible Loincs. Returns a shallow copy.</summary>
    public static List<Loinc> ListShort {
        get {
            return _LoincCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _LoincCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _LoincCache.GetTableFromCache(doRefreshCache);
    }

    #endregion*/

    
    public static long Insert(Loinc loinc)
    {
        return LoincCrud.Insert(loinc);
    }

    
    public static void Update(Loinc loinc)
    {
        LoincCrud.Update(loinc);
    }

    
    public static List<Loinc> GetAll()
    {
        var command = "SELECT * FROM loinc";
        return LoincCrud.SelectMany(command);
    }

    
    public static List<Loinc> GetBySearchString(string searchText)
    {
        string command;
        command = "SELECT * FROM loinc WHERE LoincCode LIKE '%" + SOut.String(searchText) + "%' OR NameLongCommon LIKE '%" + SOut.String(searchText)
                  + "%' ORDER BY RankCommonTests=0, RankCommonTests"; //common tests are at top of list.
        return LoincCrud.SelectMany(command);
    }

    ///<summary>Returns the count of all LOINC codes.  LOINC codes cannot be hidden.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM loinc";
        return SIn.Long(Db.GetCount(command));
    }

    ///<summary>Gets one Loinc from the db based on LoincCode, returns null if not found.</summary>
    public static Loinc GetByCode(string loincCode)
    {
        var command = "SELECT * FROM loinc WHERE LoincCode='" + SOut.String(loincCode) + "'";
        var listLoincs = LoincCrud.SelectMany(command);
        if (listLoincs.Count > 0) return listLoincs[0];
        return null;
    }

    /// <summary>
    ///     Gets a list of Loinc objects from the db based on codeList.  codeList is a comma-delimited list of LoincCodes
    ///     in the format "code,code,code,code".  Returns an empty list if none in the loinc table.
    /// </summary>
    public static List<Loinc> GetForCodeList(string codeList)
    {
        var listCodes = codeList.Split(',').ToList();
        var command = "SELECT * FROM loinc WHERE LoincCode IN(";
        for (var i = 0; i < listCodes.Count; i++)
        {
            if (i > 0) command += ",";
            command += "'" + SOut.String(listCodes[i]) + "'";
        }

        command += ") ";
        return LoincCrud.SelectMany(command);
    }

    ///<summary>CAUTION, this empties the entire loinc table. "DELETE FROM loinc"</summary>
    public static void DeleteAll()
    {
        var command = "DELETE FROM loinc";
        Db.NonQ(command);
        command = "ALTER TABLE loinc AUTO_INCREMENT = 1"; //resets the primary key to start counting from 1 again.
        Db.NonQ(command);
    }

    ///<summary>Returns a list of just the codes for use in update or insert logic.</summary>
    public static List<string> GetAllCodes()
    {
        var listCodes = new List<string>();
        var command = "SELECT LoincCode FROM loinc";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listCodes.Add(table.Rows[i].ItemArray[0].ToString());
        return listCodes;
    }

    ///<summary>Directly from db.</summary>
    public static bool CodeExists(string loincCode)
    {
        var command = "SELECT COUNT(*) FROM loinc WHERE LoincCode='" + SOut.String(loincCode) + "'";
        var count = Db.GetCount(command);
        if (count == "0") return false;
        return true;
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<Loinc> Refresh(long patNum){

        string command="SELECT * FROM loinc WHERE PatNum = "+POut.Long(patNum);
        return Crud.LoincCrud.SelectMany(command);
    }

    ///<summary>Gets one Loinc from the db.</summary>
    public static Loinc GetOne(long lOINCNum){

        return Crud.LoincCrud.SelectOne(lOINCNum);
    }

    
    public static void Delete(long lOINCNum) {

        string command= "DELETE FROM loinc WHERE LoincNum = "+POut.Long(lOINCNum);
        Db.NonQ(command);
    }
    */
}