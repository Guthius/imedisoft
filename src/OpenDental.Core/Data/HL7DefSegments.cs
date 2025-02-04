using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class HL7DefSegments
{
    public static List<HL7DefSegment> GetShallowFromDb(long hL7DefMessageNum)
    {
        return HL7DefSegmentCrud.SelectMany("SELECT * FROM hl7defsegment WHERE HL7DefMessageNum='" + hL7DefMessageNum + "' ORDER BY ItemOrder");
    }

    public static List<HL7DefSegment> GetDeepFromCache(long hL7DefMessageNum)
    {
        var hl7DefSegmentsRet = new List<HL7DefSegment>();
        var hl7DefSegments = GetDeepCopy();

        foreach (var hl7DefSegment in hl7DefSegments)
        {
            if (hl7DefSegment.HL7DefMessageNum != hL7DefMessageNum)
            {
                continue;
            }

            hl7DefSegmentsRet.Add(hl7DefSegment);
            hl7DefSegmentsRet[hl7DefSegmentsRet.Count - 1].hl7DefFields = HL7DefFields.GetFromCache(hl7DefSegment.HL7DefSegmentNum);
        }

        return hl7DefSegmentsRet;
    }

    public static List<HL7DefSegment> GetDeepFromDb(long hL7DefMessageNum)
    {
        var hl7DefSegments = GetShallowFromDb(hL7DefMessageNum);

        foreach (var hl7DefSegment in hl7DefSegments)
        {
            hl7DefSegment.hl7DefFields = HL7DefFields.GetFromDb(hl7DefSegment.HL7DefSegmentNum);
        }

        return hl7DefSegments;
    }

    public static long Insert(HL7DefSegment hL7DefSegment)
    {
        return HL7DefSegmentCrud.Insert(hL7DefSegment);
    }

    public static void Update(HL7DefSegment hL7DefSegment)
    {
        HL7DefSegmentCrud.Update(hL7DefSegment);
    }

    public static void Delete(long hL7DefSegmentNum)
    {
        Db.NonQ("DELETE FROM hl7defsegment WHERE HL7DefSegmentNum = " + hL7DefSegmentNum);
    }

    private class HL7DefSegmentCache : CacheListAbs<HL7DefSegment>
    {
        protected override List<HL7DefSegment> GetCacheFromDb()
        {
            return HL7DefSegmentCrud.SelectMany("SELECT * FROM hl7defsegment ORDER BY ItemOrder");
        }

        protected override List<HL7DefSegment> TableToList(DataTable dataTable)
        {
            return HL7DefSegmentCrud.TableToList(dataTable);
        }

        protected override HL7DefSegment Copy(HL7DefSegment item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<HL7DefSegment> items)
        {
            return HL7DefSegmentCrud.ListToTable(items, "HL7DefSegment");
        }

        protected override void FillCacheIfNeeded()
        {
            HL7DefSegments.GetTableFromCache(false);
        }
    }

    private static readonly HL7DefSegmentCache Cache = new();

    public static List<HL7DefSegment> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
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