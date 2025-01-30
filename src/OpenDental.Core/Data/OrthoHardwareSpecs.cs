using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class OrthoHardwareSpecs
{
    private class OrthoHardwareSpecCache : CacheListAbs<OrthoHardwareSpec>
    {
        protected override List<OrthoHardwareSpec> GetCacheFromDb()
        {
            return OrthoHardwareSpecCrud.SelectMany("SELECT * FROM orthohardwarespec ORDER BY OrthoHardwareType,ItemOrder");
        }

        protected override List<OrthoHardwareSpec> TableToList(DataTable dataTable)
        {
            return OrthoHardwareSpecCrud.TableToList(dataTable);
        }

        protected override OrthoHardwareSpec Copy(OrthoHardwareSpec item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<OrthoHardwareSpec> items)
        {
            return OrthoHardwareSpecCrud.ListToTable(items, "OrthoHardwareSpec");
        }

        protected override void FillCacheIfNeeded()
        {
            OrthoHardwareSpecs.GetTableFromCache(false);
        }

        protected override bool IsInListShort(OrthoHardwareSpec item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly OrthoHardwareSpecCache Cache = new();

    public static List<OrthoHardwareSpec> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static OrthoHardwareSpec GetFirstOrDefault(Func<OrthoHardwareSpec, bool> predicate, bool shortList = false)
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

    public static void Insert(OrthoHardwareSpec orthoHardwareSpec)
    {
        OrthoHardwareSpecCrud.Insert(orthoHardwareSpec);
    }

    public static void Update(OrthoHardwareSpec orthoHardwareSpec)
    {
        OrthoHardwareSpecCrud.Update(orthoHardwareSpec);
    }

    public static void Delete(long orthoHardwareSpecNum)
    {
        var count = Db.GetCount("SELECT COUNT(*) FROM orthohardware WHERE OrthoHardwareSpecNum = " + orthoHardwareSpecNum);
        if (count != "0")
        {
            throw new Exception("Already in use by patients. Hide instead of Deleting.");
        }

        count = Db.GetCount("SELECT COUNT(*) FROM orthorx WHERE OrthoHardwareSpecNum = " + orthoHardwareSpecNum);
        if (count != "0")
        {
            throw new Exception("Already in use by Ortho Prescription. Hide instead of Deleting or unlink this Hardware Spec from Ortho Prescriptions.");
        }

        OrthoHardwareSpecCrud.Delete(orthoHardwareSpecNum);
    }
}