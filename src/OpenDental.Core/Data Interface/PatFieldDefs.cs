using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

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
        var command = "SELECT LName,FName FROM patient,patfield WHERE "
                      + "patient.PatNum=patfield.PatNum "
                      + "AND FieldName='" + SOut.String(patFieldDef.FieldName) + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            var s = Lans.g("PatFieldDef", "Not allowed to delete. Already in use by ") + table.Rows.Count
                                                                                       + " " + Lans.g("PatFieldDef", "patients, including") + " \r\n";
            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (i > 5) break;
                s += table.Rows[i][0] + ", " + table.Rows[i][1] + "\r\n";
            }

            throw new ApplicationException(s);
        }

        command = "DELETE FROM patfielddef WHERE PatFieldDefNum =" + SOut.Long(patFieldDef.PatFieldDefNum);
        Db.NonQ(command);
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
            PatFieldDefs.GetTableFromCache(false);
        }

        protected override bool IsInListShort(PatFieldDef item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly PatFieldDefCache Cache = new();

    public static bool GetExists(Predicate<PatFieldDef> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    public static List<PatFieldDef> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static PatFieldDef GetFirstOrDefault(Func<PatFieldDef, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}