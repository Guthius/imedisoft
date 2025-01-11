using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class RxNorms
{
    /*
    #region CachePattern

    private class RxNormCache : CacheListAbs<RxNorm> {
        protected override List<RxNorm> GetCacheFromDb() {
            string command="SELECT * FROM RxNorm ORDER BY ItemOrder";
            return Crud.RxNormCrud.SelectMany(command);
        }
        protected override List<RxNorm> TableToList(DataTable table) {
            return Crud.RxNormCrud.TableToList(table);
        }
        protected override RxNorm Copy(RxNorm RxNorm) {
            return RxNorm.Clone();
        }
        protected override DataTable ListToTable(List<RxNorm> listRxNorms) {
            return Crud.RxNormCrud.ListToTable(listRxNorms,"RxNorm");
        }
        protected override void FillCacheIfNeeded() {
            RxNorms.GetTableFromCache(false);
        }
        protected override bool IsInListShort(RxNorm RxNorm) {
            return !RxNorm.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static RxNormCache _RxNormCache=new RxNormCache();

    ///<summary>A list of all RxNorms. Returns a deep copy.</summary>
    public static List<RxNorm> ListDeep {
        get {
            return _RxNormCache.ListDeep;
        }
    }

    ///<summary>A list of all visible RxNorms. Returns a deep copy.</summary>
    public static List<RxNorm> ListShortDeep {
        get {
            return _RxNormCache.ListShortDeep;
        }
    }

    ///<summary>A list of all RxNorms. Returns a shallow copy.</summary>
    public static List<RxNorm> ListShallow {
        get {
            return _RxNormCache.ListShallow;
        }
    }

    ///<summary>A list of all visible RxNorms. Returns a shallow copy.</summary>
    public static List<RxNorm> ListShort {
        get {
            return _RxNormCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _RxNormCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _RxNormCache.GetTableFromCache(doRefreshCache);
    }

    #endregion*/

    /// <summary>
    ///     RxNorm table is considered to be too small if less than 50 RxNorms in table,
    ///     because our default medication list contains 50 items, implying that the user has not imported RxNorms.
    /// </summary>
    public static bool IsRxNormTableSmall()
    {
        var command = "SELECT COUNT(*) FROM rxnorm";
        if (SIn.Int(Db.GetCount(command)) < 50) return true;
        return false;
    }

    public static RxNorm GetByRxCUI(string rxCui)
    {
        var command = "SELECT * FROM rxnorm WHERE RxCui='" + SOut.String(rxCui) + "' AND MmslCode=''";
        return RxNormCrud.SelectOne(command);
    }

    ///<summary>Never returns multums, only used for displaying after a search.</summary>
    public static List<RxNorm> GetListByCodeOrDesc(string codeOrDesc, bool isExact, bool ignoreNumbers)
    {
        var command = "SELECT * FROM rxnorm WHERE MmslCode='' ";
        if (isExact)
        {
            command += "AND (RxCui = '" + SOut.String(codeOrDesc) + "' OR Description = '" + SOut.String(codeOrDesc) + "')";
        }
        else
        {
            //Similar matches
            var charArraySeparators = new[] {' ', '\t', '\r', '\n'};
            var listSearchWords = codeOrDesc.Split(charArraySeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (listSearchWords.Count > 0)
                command += "AND ("
                           + "RxCui LIKE '%" + SOut.String(codeOrDesc) + "%' "
                           + " OR "
                           + "(" + string.Join(" AND ", listSearchWords.Select(x => "Description LIKE '%" + SOut.String(x) + "%'")) + ") "
                           + ")";
        }

        if (ignoreNumbers) command += "AND Description NOT REGEXP '.*[0-9]+.*' ";
        command += " ORDER BY Description";
        return RxNormCrud.SelectMany(command);
    }

    ///<summary>Used to return the multum code based on RxCui.  If blank, use the Description instead.</summary>
    public static string GetMmslCodeByRxCui(string rxCui)
    {
        var command = "SELECT MmslCode FROM rxnorm WHERE MmslCode!='' AND RxCui='" + rxCui + "'";
        return DataCore.GetScalar(command);
    }

    
    public static string GetDescByRxCui(string rxCui)
    {
        var command = "SELECT Description FROM rxnorm WHERE MmslCode='' AND RxCui='" + rxCui + "'";
        return DataCore.GetScalar(command);
    }

    ///<summary>Gets one RxNorm from the db.</summary>
    public static RxNorm GetOne(long rxNormNum)
    {
        return RxNormCrud.SelectOne(rxNormNum);
    }

    
    public static long Insert(RxNorm rxNorm)
    {
        return RxNormCrud.Insert(rxNorm);
    }

    
    public static void Update(RxNorm rxNorm)
    {
        RxNormCrud.Update(rxNorm);
    }

    
    public static List<RxNorm> GetAll()
    {
        var command = "SELECT * FROM rxnorm";
        return RxNormCrud.SelectMany(command);
    }

    ///<summary>Returns a list of just the codes for use in the codesystem import tool.</summary>
    public static List<string> GetAllCodes()
    {
        var listCodes = new List<string>();
        var command = "SELECT RxCui FROM rxnorm"; //will return some duplicates due to the nature of the data in the table. This is acceptable.
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listCodes.Add(table.Rows[i].ItemArray[0].ToString());
        return listCodes;
    }

    ///<summary>Returns the count of all RxNorm codes in the database.  RxNorms cannot be hidden.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM rxnorm";
        return SIn.Long(Db.GetCount(command));
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<RxNorm> Refresh(long patNum){

        string command="SELECT * FROM rxnorm WHERE PatNum = "+POut.Long(patNum);
        return Crud.RxNormCrud.SelectMany(command);
    }
    */
}