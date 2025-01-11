using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class UserClinics
{
    public static List<UserClinic> GetForUser(long userNum)
    {
        return GetWhere(x => x.UserNum == userNum);
    }

    public static List<UserClinic> GetForClinic(long clinicNum)
    {
        return GetWhere(x => x.ClinicNum == clinicNum);
    }
    
    public static void Insert(UserClinic userClinic)
    {
        UserClinicCrud.Insert(userClinic);
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
            UserClinics.GetTableFromCache(false);
        }
    }

    private static readonly UserClinicCache Cache = new();
    
    public static List<UserClinic> GetWhere(Predicate<UserClinic> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }
    
    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static void FillCacheFromTable(DataTable table)
    {
        Cache.FillCacheFromTable(table);
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