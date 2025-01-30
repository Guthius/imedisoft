using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class HL7DefFields
{
    public static List<HL7DefField> GetFromDb(long hl7DefSegmentNum)
    {
        return HL7DefFieldCrud.SelectMany("SELECT * FROM hl7deffield WHERE HL7DefSegmentNum = " + hl7DefSegmentNum + " ORDER BY OrdinalPos");
    }

    public static List<HL7DefField> GetFromCache(long hl7DefSegmentNum)
    {
        return GetWhere(x => x.HL7DefSegmentNum == hl7DefSegmentNum);
    }

    public static void Insert(HL7DefField hL7DefField)
    {
        HL7DefFieldCrud.Insert(hL7DefField);
    }

    public static void Update(HL7DefField hL7DefField)
    {
        HL7DefFieldCrud.Update(hL7DefField);
    }

    public static void Delete(long hL7DefFieldNum)
    {
        Db.NonQ("DELETE FROM hl7deffield WHERE HL7DefFieldNum = " + hL7DefFieldNum);
    }

    private class HL7DefFieldCache : CacheListAbs<HL7DefField>
    {
        protected override List<HL7DefField> GetCacheFromDb()
        {
            return HL7DefFieldCrud.SelectMany("SELECT * FROM hl7deffield ORDER BY OrdinalPos");
        }

        protected override List<HL7DefField> TableToList(DataTable dataTable)
        {
            return HL7DefFieldCrud.TableToList(dataTable);
        }

        protected override HL7DefField Copy(HL7DefField item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<HL7DefField> items)
        {
            return HL7DefFieldCrud.ListToTable(items, "HL7DefField");
        }

        protected override void FillCacheIfNeeded()
        {
            HL7DefFields.GetTableFromCache(false);
        }
    }

    private static readonly HL7DefFieldCache Cache = new();

    public static List<HL7DefField> GetWhere(Predicate<HL7DefField> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
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