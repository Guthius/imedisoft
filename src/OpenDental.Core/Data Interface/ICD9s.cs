using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ICD9s
{
    
    public static List<ICD9> GetByCodeOrDescription(string searchTxt)
    {
        var command = "SELECT * FROM icd9 WHERE ICD9Code LIKE '%" + SOut.String(searchTxt) + "%' "
                      + "OR Description LIKE '%" + SOut.String(searchTxt) + "%'";
        return ICD9Crud.SelectMany(command);
    }

    ///<summary>Gets one ICD9 from the db.</summary>
    public static ICD9 GetOne(long iCD9Num)
    {
        return ICD9Crud.SelectOne(iCD9Num);
    }

    
    public static List<ICD9> GetAll()
    {
        var command = "SELECT * FROM icd9";
        return ICD9Crud.SelectMany(command);
    }

    ///<summary>Returns the total count of ICD9 codes.  ICD9 codes cannot be hidden.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM icd9";
        return SIn.Long(Db.GetCount(command));
    }

    ///<summary>Directly from db.</summary>
    public static bool CodeExists(string iCD9Code)
    {
        var command = "SELECT COUNT(*) FROM icd9 WHERE ICD9Code = '" + SOut.String(iCD9Code) + "'";
        var count = Db.GetCount(command);
        if (count == "0") return false;
        return true;
    }

    
    public static long Insert(ICD9 icd9)
    {
        return ICD9Crud.Insert(icd9);
    }

    
    public static void Update(ICD9 icd9)
    {
        ICD9Crud.Update(icd9);
    }

    
    public static void Delete(long icd9Num)
    {
        var command = "SELECT LName,FName,patient.PatNum FROM patient,disease,diseasedef,icd9 WHERE "
                      + "patient.PatNum=disease.PatNum "
                      + "AND disease.DiseaseDefNum=diseasedef.DiseaseDefNum "
                      + "AND diseasedef.ICD9Code=icd9.ICD9Code "
                      + "AND icd9.ICD9Num='" + SOut.Long(icd9Num) + "' "
                      + "GROUP BY patient.PatNum";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0)
        {
            command = "DELETE FROM icd9 WHERE ICD9Num = " + SOut.Long(icd9Num);
            Db.NonQ(command);
            return;
        }

        var exceptionMsg = Lans.g("ICD9", "Not allowed to delete. Already in use by ") + table.Rows.Count
                                                                                       + " " + Lans.g("ICD9", "patients, including") + " \r\n";
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 5) break;
            exceptionMsg += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"] + " - " + table.Rows[i]["PatNum"] + "\r\n";
        }

        throw new ApplicationException(exceptionMsg);
    }

    ///<summary>This method uploads only the ICD9s that are used by the disease table. This is to reduce upload time.</summary>
    public static List<long> GetChangedSinceICD9Nums(DateTime dateTChangedSince)
    {
        //string command="SELECT ICD9Num FROM icd9 WHERE DateTStamp > "+POut.DateT(changedSince);//Dennis: delete this line later
        var command = "SELECT ICD9Num FROM icd9 WHERE DateTStamp > " + SOut.DateT(dateTChangedSince)
                                                                     + " AND ICD9Num in (SELECT ICD9Num FROM disease)";
        var table = DataCore.GetTable(command);
        var listIcd9Nums = new List<long>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++) listIcd9Nums.Add(SIn.Long(table.Rows[i]["ICD9Num"].ToString()));
        return listIcd9Nums;
    }

    ///<summary>Used along with GetChangedSinceICD9Nums</summary>
    public static List<ICD9> GetMultICD9s(List<long> listICD9Nums)
    {
        var strICD9Nums = "";
        var table = new DataTable();
        if (listICD9Nums.Count > 0)
        {
            for (var i = 0; i < listICD9Nums.Count; i++)
            {
                if (i > 0) strICD9Nums += "OR ";
                strICD9Nums += "ICD9Num='" + listICD9Nums[i] + "' ";
            }

            var command = "SELECT * FROM icd9 WHERE " + strICD9Nums;
            table = DataCore.GetTable(command);
        }

        return ICD9Crud.TableToList(table);
    }

    ///<summary>Returns the code and description of the icd9.</summary>
    public static string GetCodeAndDescription(string iCD9Code)
    {
        if (string.IsNullOrEmpty(iCD9Code)) return "";

        var iCD9 = GetFirstOrDefault(x => x.ICD9Code == iCD9Code);
        if (iCD9 == null) return "";
        return iCD9.ICD9Code + "-" + iCD9.Description;
    }

    ///<summary>Returns the ICD9 of the code passed in by looking in cache.  If code does not exist, returns null.</summary>
    public static ICD9 GetByCode(string iCD9Code)
    {
        return GetFirstOrDefault(x => x.ICD9Code == iCD9Code);
    }

    ///<summary>Returns true if descriptions have not been updated to non-Caps Lock.  Always returns false if not MySQL.</summary>
    public static bool IsOldDescriptions()
    {
        var command = @"SELECT COUNT(*) FROM icd9 WHERE BINARY description = UPPER(description)"; //count rows that are all caps
        if (SIn.Int(DataCore.GetScalar(command)) > 10000) //"Normal" DB should have 4, might be more if hand entered, over 10k means it is the old import.
            return true;
        return false;
    }

    ///<summary>Returns a list of just the codes for use in update or insert logic.</summary>
    public static List<string> GetAllCodes()
    {
        var listICD9Codes = new List<string>();
        var command = "SELECT icd9code FROM icd9";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listICD9Codes.Add(table.Rows[i].ItemArray[0].ToString());
        return listICD9Codes;
    }

    ///<summary>Returns true if any of the procs have a ICD9 code.</summary>
    public static bool HasICD9Codes(List<Procedure> listProcedures)
    {
        var listICD9Codes = new List<string>();
        var listProceduresICD9 = listProcedures.FindAll(x => x.IcdVersion == 9);
        listICD9Codes.AddRange(listProceduresICD9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode)).Select(x => x.DiagnosticCode));
        listICD9Codes.AddRange(listProceduresICD9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode2)).Select(x => x.DiagnosticCode2));
        listICD9Codes.AddRange(listProceduresICD9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode3)).Select(x => x.DiagnosticCode3));
        listICD9Codes.AddRange(listProceduresICD9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode4)).Select(x => x.DiagnosticCode4));
        if (listICD9Codes.Count != 0) return true;
        return false;
    }

    #region CachePattern

    private class ICD9Cache : CacheListAbs<ICD9>
    {
        protected override List<ICD9> GetCacheFromDb()
        {
            var command = "SELECT * FROM icd9 ORDER BY ICD9Code";
            return ICD9Crud.SelectMany(command);
        }

        protected override List<ICD9> TableToList(DataTable dataTable)
        {
            return ICD9Crud.TableToList(dataTable);
        }

        protected override ICD9 Copy(ICD9 item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ICD9> items)
        {
            return ICD9Crud.ListToTable(items, "ICD9");
        }

        protected override void FillCacheIfNeeded()
        {
            ICD9s.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ICD9Cache _ICD9Cache = new();

    public static ICD9 GetFirstOrDefault(Func<ICD9, bool> match, bool isShort = false)
    {
        return _ICD9Cache.GetFirstOrDefault(match, isShort);
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
        _ICD9Cache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _ICD9Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _ICD9Cache.ClearCache();
    }

    #endregion
}