using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class UserOdPrefs
{
    public static bool Sync(List<UserOdPref> userOdPrefsNew, List<UserOdPref> userOdPrefsOld)
    {
        return UserOdPrefCrud.Sync(userOdPrefsNew, userOdPrefsOld);
    }

    public static bool Update(UserOdPref userOdPref, UserOdPref userOdPrefOld = null)
    {
        if (userOdPrefOld is not null)
        {
            return UserOdPrefCrud.Update(userOdPref, userOdPrefOld);
        }

        UserOdPrefCrud.Update(userOdPref);
        return true;
    }

    public static void Insert(UserOdPref pref)
    {
        UserOdPrefCrud.Insert(pref);
    }

    public static void InsertMany(List<UserOdPref> prefs)
    {
        UserOdPrefCrud.InsertMany(prefs);
    }

    public static void Upsert(UserOdPref pref)
    {
        if (pref.UserOdPrefNum == 0)
        {
            UserOdPrefCrud.Insert(pref);
            return;
        }

        UserOdPrefCrud.Update(pref);
    }

    public static void Delete(long userOdPrefNum)
    {
        UserOdPrefCrud.Delete(userOdPrefNum);
    }

    public static void DeleteManyForUserAndFkeyType(long userNum, UserOdFkeyType fkeyType)
    {
        Db.NonQ("DELETE FROM userodpref WHERE UserNum = " + userNum + " AND FkeyType = " + (int) fkeyType);
    }

    public static List<UserOdPref> GetByUserAndFkeyType(long userNum, UserOdFkeyType fkeyType)
    {
        return GetWhere(x => x.UserNum == userNum && x.FkeyType == fkeyType);
    }

    public static List<UserOdPref> GetByFkeyAndFkeyType(long fkey, UserOdFkeyType fkeyType)
    {
        return GetWhere(x => x.Fkey == fkey && x.FkeyType == fkeyType);
    }

    public static List<UserOdPref> GetByUserFkeyAndFkeyType(long userNum, long fkey, UserOdFkeyType fkeyType)
    {
        return GetWhere(x => x.UserNum == userNum && x.Fkey == fkey && x.FkeyType == fkeyType);
    }

    public static UserOdPref GetFirstOrNewByUserAndFkeyType(long userNum, UserOdFkeyType fkeyType)
    {
        var userOdPref = GetFirstOrDefault(x => x.UserNum == userNum && x.FkeyType == fkeyType) ?? new UserOdPref
        {
            IsNew = true,
            UserOdPrefNum = 0,
            Fkey = 0,
            FkeyType = fkeyType,
            UserNum = userNum,
            ValueString = "",
            ClinicNum = 0
        };

        return userOdPref;
    }

    public static UserOdPref GetByCompositeKey(long userNum, long fkey, UserOdFkeyType fkeyType, long clinicNum = 0)
    {
        var pref = GetFirstOrDefault(x =>
            x.UserNum == userNum &&
            x.Fkey == fkey &&
            x.FkeyType == fkeyType &&
            x.ClinicNum == clinicNum);

        return pref ?? new UserOdPref
        {
            IsNew = true,
            UserOdPrefNum = 0,
            Fkey = fkey,
            FkeyType = fkeyType,
            UserNum = userNum,
            ValueString = "",
            ClinicNum = clinicNum
        };
    }

    public static List<UserOdPref> GetByFkeyType(UserOdFkeyType userOdFkeyType)
    {
        return GetWhere(pref => pref.FkeyType == userOdFkeyType);
    }

    public static void DeleteForFkey(long userNum, UserOdFkeyType fkeyType, long fkey)
    {
        var commandText = "DELETE FROM userodpref WHERE Fkey = " + fkey + " AND FkeyType = " + (int) fkeyType;
        
        if (userNum != 0)
        {
            commandText += " AND UserNum = " + userNum;
        }

        Db.NonQ(commandText);
    }

    private class UserOdPrefCache : CacheListAbs<UserOdPref>
    {
        protected override List<UserOdPref> GetCacheFromDb()
        {
            return UserOdPrefCrud.SelectMany("SELECT * FROM userodpref");
        }

        protected override List<UserOdPref> TableToList(DataTable dataTable)
        {
            return UserOdPrefCrud.TableToList(dataTable);
        }

        protected override UserOdPref Copy(UserOdPref item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<UserOdPref> items)
        {
            return UserOdPrefCrud.ListToTable(items, "UserOdPref");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly UserOdPrefCache Cache = new();

    public static UserOdPref GetFirstOrDefault(Func<UserOdPref, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<UserOdPref> GetWhere(Predicate<UserOdPref> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
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