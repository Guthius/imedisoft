using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class UserClinics
{
    public static List<UserClinic> GetForUser(long userNum)
    {
        return GetWhere(x => x.UserNum == userNum);
    }

    public static List<UserClinic> GetForClinic(long clinicNum)
    {
        return GetWhere(x => x.ClinicNum == clinicNum);
    }

    public static bool Sync(List<UserClinic> listUserClinicsNew, long userNum)
    {
        var listUserClinicsOld = GetForUser(userNum);

        return UserClinicCrud.Sync(listUserClinicsNew, listUserClinicsOld);
    }

    private class UserClinicCache : CacheListAbs<UserClinic>
    {
        protected override List<UserClinic> GetCacheFromDb()
        {
            return UserClinicCrud.SelectMany("SELECT * FROM userclinic");
        }

        protected override List<UserClinic> TableToList(DataTable dataTable)
        {
            return UserClinicCrud.TableToList(dataTable);
        }

        protected override UserClinic Copy(UserClinic item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<UserClinic> items)
        {
            return UserClinicCrud.ListToTable(items, "UserClinic");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly UserClinicCache Cache = new();

    public static List<UserClinic> GetWhere(Predicate<UserClinic> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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