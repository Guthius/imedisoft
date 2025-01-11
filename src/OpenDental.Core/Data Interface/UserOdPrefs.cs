using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class UserOdPrefs
{
    public static bool Sync(List<UserOdPref> listUserOdPrefsNew, List<UserOdPref> listUserOdPrefsOld)
    {
        return UserOdPrefCrud.Sync(listUserOdPrefsNew, listUserOdPrefsOld);
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

    public static long Insert(UserOdPref pref)
    {
        return UserOdPrefCrud.Insert(pref);
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

    public static void DeleteMany(long userNum, long fkey, UserOdFkeyType fkeyType)
    {
        Db.NonQ("DELETE FROM userodpref WHERE UserNum=" + userNum + " AND FkeyType=" + (int) fkeyType + " AND Fkey=" + fkey);
    }

    public static void DeleteManyForUserAndFkeyType(long userNum, UserOdFkeyType fkeyType)
    {
        Db.NonQ("DELETE FROM userodpref WHERE UserNum=" + userNum + " AND FkeyType=" + (int) fkeyType);
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

    public static List<UserOdPref> GetByUserAndFkeyAndFkeyType(long userNum, long fkey, UserOdFkeyType fkeyType, List<long> clinicNums = null)
    {
        var prefs = GetWhere(x => x.UserNum == userNum && x.Fkey == fkey && x.FkeyType == fkeyType);

        if (clinicNums is {Count: > 0})
        {
            prefs = prefs.Where(x => clinicNums.Contains(x.ClinicNum)).ToList();
        }

        return prefs;
    }

    public static UserOdPref GetByCompositeKey(long userNum, long fkey, UserOdFkeyType fkeyType, long clinicNum = 0)
    {
        return GetFirstOrDefault(pref =>
                   pref.UserNum == userNum &&
                   pref.Fkey == fkey &&
                   pref.FkeyType == fkeyType &&
                   pref.ClinicNum == clinicNum)
               ??
               new UserOdPref
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

    public static List<UserOdPref> GetAllByFkeyAndFkeyType(long fkey, UserOdFkeyType fkeyType)
    {
        return GetWhere(x => x.Fkey == fkey && x.FkeyType == fkeyType);
    }

    public static void DeleteForFkey(long userNum, UserOdFkeyType fkeyType, long fkey)
    {
        var command = "DELETE FROM userodpref WHERE Fkey=" + fkey + " AND FkeyType=" + (int) fkeyType;
        if (userNum != 0)
        {
            command += " AND UserNum=" + userNum;
        }

        Db.NonQ(command);
    }

    public static void DeleteForValueString(long userNum, UserOdFkeyType fkeyType, string valueString)
    {
        var command = "DELETE FROM userodpref WHERE ValueString='" + SOut.String(valueString) + "' AND FkeyType=" + (int) fkeyType;
        if (userNum != 0)
        {
            command += " AND UserNum=" + userNum;
        }

        Db.NonQ(command);
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
            UserOdPrefs.GetTableFromCache(false);
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
        GetTableFromCache(true);
    }

    public static void FillCacheFromTable(DataTable dataTable)
    {
        Cache.FillCacheFromTable(dataTable);
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