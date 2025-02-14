using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDentBusiness.Properties;

namespace Imedisoft.Core.Data;

public static class ClaimForms
{
    public static void Insert(ClaimForm claimForm, bool includeClaimFormItems)
    {
        ClaimFormCrud.Insert(claimForm);

        if (!includeClaimFormItems)
        {
            return;
        }

        foreach (var claimFormItem in claimForm.Items)
        {
            claimFormItem.ClaimFormNum = claimForm.ClaimFormNum;
            ClaimFormItems.Insert(claimFormItem);
        }
    }

    public static ClaimForm DeserializeClaimForm(string path, string xmlData)
    {
        var xmlSerializer = new XmlSerializer(typeof(ClaimForm));
        if (string.IsNullOrEmpty(xmlData))
        {
            if (!File.Exists(path))
            {
                throw new ApplicationException("File does not exist.");
            }

            try
            {
                using var streamReader = new StreamReader(path);

                return (ClaimForm) xmlSerializer.Deserialize(streamReader);
            }
            catch
            {
                throw new ApplicationException("Invalid file format");
            }
        }

        try
        {
            using var stringReader = new StringReader(xmlData);

            return (ClaimForm) xmlSerializer.Deserialize(stringReader);
        }
        catch
        {
            throw new ApplicationException("Invalid file format");
        }
    }

    public static void Update(ClaimForm claimForm)
    {
        ClaimFormItems.DeleteAllForClaimForm(claimForm.ClaimFormNum);

        foreach (var claimFormItem in claimForm.Items)
        {
            ClaimFormItems.Insert(claimFormItem);
        }

        ClaimFormCrud.Update(claimForm);
    }

    public static bool Delete(ClaimForm claimForm)
    {
        var commandText = "SELECT * FROM insplan WHERE claimformnum = " + claimForm.ClaimFormNum + " LIMIT 1";

        var dataTable = DataCore.GetTable(commandText);
        if (dataTable.Rows.Count == 1)
        {
            return false;
        }

        Db.NonQ("DELETE FROM claimform WHERE ClaimFormNum = " + claimForm.ClaimFormNum);
        Db.NonQ("DELETE FROM claimformitem WHERE ClaimFormNum = " + claimForm.ClaimFormNum);
        return true;
    }

    public static ClaimForm GetClaimForm(long claimFormNum)
    {
        return GetFirstOrDefault(x => x.ClaimFormNum == claimFormNum);
    }

    public static List<ClaimForm> GetInternalClaims()
    {
        var internalClaimForms = new List<ClaimForm>();

        var resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

        foreach (DictionaryEntry item in resourceSet)
        {
            if (!item.Key.ToString().StartsWith("ClaimForm"))
            {
                continue;
            }

            var claimForm = DeserializeClaimForm("", item.Value.ToString());

            claimForm.IsInternal = true;

            internalClaimForms.Add(claimForm);
        }

        return internalClaimForms;
    }

    public static long Reassign(long claimFormNumOld, long claimFormNumNew)
    {
        return Db.NonQ("UPDATE insplan SET ClaimFormNum = " + claimFormNumNew + " WHERE ClaimFormNum = " + claimFormNumOld);
    }

    public static void SetDefaultClaimForm(string claimFormDescriptFrom, string claimFormDescriptTo)
    {
        var claimFormFrom = GetDeepCopy().Find(x => string.Equals(x.Description, claimFormDescriptFrom, StringComparison.CurrentCultureIgnoreCase));
        var claimFormTo = GetDeepCopy().Find(x => string.Equals(x.Description, claimFormDescriptTo, StringComparison.CurrentCultureIgnoreCase));

        if (claimFormFrom is null || claimFormTo is null)
        {
            return;
        }

        var defaultClaimFormNum = PrefC.GetLong(PrefName.DefaultClaimForm);

        Reassign(claimFormFrom.ClaimFormNum, claimFormTo.ClaimFormNum);

        if (defaultClaimFormNum == claimFormFrom.ClaimFormNum)
        {
            Prefs.UpdateLong(PrefName.DefaultClaimForm, claimFormTo.ClaimFormNum);
        }
    }

    private class ClaimFormCache : CacheListAbs<ClaimForm>
    {
        protected override List<ClaimForm> GetCacheFromDb()
        {
            var claimForms = ClaimFormCrud.SelectMany("SELECT * FROM claimform");

            foreach (var claimForm in claimForms)
            {
                claimForm.Items = ClaimFormItems.GetListForForm(claimForm.ClaimFormNum);
            }

            return claimForms;
        }

        protected override List<ClaimForm> TableToList(DataTable dataTable)
        {
            var claimForms = ClaimFormCrud.TableToList(dataTable);

            foreach (var claimForm in claimForms)
            {
                claimForm.Items = ClaimFormItems.GetListForForm(claimForm.ClaimFormNum);
            }

            return claimForms;
        }

        protected override ClaimForm Copy(ClaimForm item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ClaimForm> items)
        {
            return ClaimFormCrud.ListToTable(items, "ClaimForm");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override bool IsInListShort(ClaimForm item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly ClaimFormCache Cache = new();

    public static List<ClaimForm> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static ClaimForm GetFirstOrDefault(Func<ClaimForm, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
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