using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public class PatFieldDefs
{
    public static void Update(PatFieldDef patFieldDef)
    {
        PatFieldDefCrud.Update(patFieldDef);
    }

    public static void Insert(PatFieldDef patFieldDef)
    {
        PatFieldDefCrud.Insert(patFieldDef);
    }

    public static void Delete(PatFieldDef patFieldDef)
    {
        var dataTable = DataCore.GetTable(
            "SELECT LName,FName FROM patient,patfield " +
            "WHERE patient.PatNum=patfield.PatNum " + 
            "AND FieldName='" + SOut.String(patFieldDef.FieldName) + "'");
        
        if (dataTable.Rows.Count > 0)
        {
            var message = "Not allowed to delete. Already in use by " + dataTable.Rows.Count + " patients, including\r\n";
            
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                if (i > 5)
                {
                    break;
                }
                
                message += dataTable.Rows[i][0] + ", " + dataTable.Rows[i][1] + "\r\n";
            }

            throw new ApplicationException(message);
        }

        Db.NonQ("DELETE FROM patfielddef WHERE PatFieldDefNum = " + patFieldDef.PatFieldDefNum);
    }
    
    public static PatFieldDef GetFieldDefByFieldName(string fieldName)
    {
        return GetFirstOrDefault(x => x.FieldName == fieldName);
    }

    public static string GetFieldName(long patFieldDefNum)
    {
        var patFieldDef = GetFirstOrDefault(x => x.PatFieldDefNum == patFieldDefNum, true);
        
        return patFieldDef == null ? "" : patFieldDef.FieldName;
    }

    private class PatFieldDefCache : CacheListAbs<PatFieldDef>
    {
        protected override List<PatFieldDef> GetCacheFromDb()
        {
            return PatFieldDefCrud.SelectMany("SELECT * FROM patfielddef ORDER BY ItemOrder");
        }

        protected override List<PatFieldDef> TableToList(DataTable dataTable)
        {
            return PatFieldDefCrud.TableToList(dataTable);
        }

        protected override PatFieldDef Copy(PatFieldDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<PatFieldDef> items)
        {
            return PatFieldDefCrud.ListToTable(items, "PatFieldDef");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override bool IsInListShort(PatFieldDef item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly PatFieldDefCache Cache = new();

    public static bool GetExists(Predicate<PatFieldDef> predicate, bool shortList = false)
    {
        return Cache.GetExists(predicate, shortList);
    }

    public static List<PatFieldDef> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static PatFieldDef GetFirstOrDefault(Func<PatFieldDef, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}