using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Icd9s
{
    public static List<ICD9> GetByCodeOrDescription(string searchTxt)
    {
        return ICD9Crud.SelectMany("SELECT * FROM icd9 WHERE ICD9Code LIKE '%" + SOut.String(searchTxt) + "%' OR Description LIKE '%" + SOut.String(searchTxt) + "%'");
    }
    
    public static List<ICD9> GetAll()
    {
        return ICD9Crud.SelectMany("SELECT * FROM icd9");
    }

    public static void Insert(ICD9 icd9)
    {
        ICD9Crud.Insert(icd9);
    }
    
    public static void Update(ICD9 icd9)
    {
        ICD9Crud.Update(icd9);
    }
    
    public static string GetCodeAndDescription(string icd9Code)
    {
        if (string.IsNullOrEmpty(icd9Code))
        {
            return "";
        }

        var iCd9 = GetFirstOrDefault(x => x.ICD9Code == icd9Code);
        if (iCd9 == null)
        {
            return "";
        }
        
        return iCd9.ICD9Code + "-" + iCd9.Description;
    }

    public static ICD9 GetByCode(string iCd9Code)
    {
        return GetFirstOrDefault(x => x.ICD9Code == iCd9Code);
    }

    public static bool IsOldDescriptions()
    {
        return SIn.Int(DataCore.GetScalar("SELECT COUNT(*) FROM icd9 WHERE BINARY description = UPPER(description)")) > 10000;
    }

    public static bool HasIcd9Codes(List<Procedure> procedures)
    {
        var icd9Codes = new List<string>();
        var proceduresIcd9 = procedures.FindAll(x => x.IcdVersion == 9);
        
        icd9Codes.AddRange(proceduresIcd9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode)).Select(x => x.DiagnosticCode));
        icd9Codes.AddRange(proceduresIcd9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode2)).Select(x => x.DiagnosticCode2));
        icd9Codes.AddRange(proceduresIcd9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode3)).Select(x => x.DiagnosticCode3));
        icd9Codes.AddRange(proceduresIcd9.Where(x => !string.IsNullOrEmpty(x.DiagnosticCode4)).Select(x => x.DiagnosticCode4));
        
        return icd9Codes.Count != 0;
    }
    
    private class ICD9Cache : CacheListAbs<ICD9>
    {
        protected override List<ICD9> GetCacheFromDb()
        {
            return ICD9Crud.SelectMany("SELECT * FROM icd9 ORDER BY ICD9Code");
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
            GetTableFromCache(false);
        }
    }

    private static readonly ICD9Cache Cache = new();

    public static ICD9 GetFirstOrDefault(Func<ICD9, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}