using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness.Properties;

namespace OpenDentBusiness;

public class ClaimForms
{
    public static void Insert(ClaimForm claimForm, bool includeClaimFormItems)
    {
        var claimFormNum = ClaimFormCrud.Insert(claimForm);
        if (includeClaimFormItems)
            for (var i = 0; i < claimForm.Items.Count; i++)
            {
                claimForm.Items[i].ClaimFormNum = claimForm.ClaimFormNum; //so even though the ClaimFormNum is wrong, this line fixes it.
                ClaimFormItems.Insert(claimForm.Items[i]);
            }
    }

    public static ClaimForm DeserializeClaimForm(string path, string xmlData)
    {
        var claimForm = new ClaimForm();
        var xmlSerializer = new XmlSerializer(typeof(ClaimForm));
        if (xmlData == "")
        {
            //use path
            if (!File.Exists(path)) throw new ApplicationException(Lans.g("FormClaimForm", "File does not exist."));
            try
            {
                using (TextReader textReader = new StreamReader(path))
                {
                    claimForm = (ClaimForm) xmlSerializer.Deserialize(textReader);
                }
            }
            catch
            {
                throw new ApplicationException(Lans.g("FormClaimForm", "Invalid file format"));
            }
        }
        else
        {
            //use xmlData
            try
            {
                using (TextReader textReader = new StringReader(xmlData))
                {
                    claimForm = (ClaimForm) xmlSerializer.Deserialize(textReader);
                }
            }
            catch
            {
                throw new ApplicationException(Lans.g("FormClaimForm", "Invalid file format"));
            }
        }

        return claimForm;
    }

    public static void Update(ClaimForm claimForm)
    {
        //Synch the claim form items associated to this claim form first.
        ClaimFormItems.DeleteAllForClaimForm(claimForm.ClaimFormNum);
        for (var i = 0; i < claimForm.Items.Count; i++) ClaimFormItems.Insert(claimForm.Items[i]);
        //Now we can update any information specific to the claim form itself.
        ClaimFormCrud.Update(claimForm);
    }

    public static bool Delete(ClaimForm claimForm)
    {
        //first, do dependency testing
        var command = "SELECT * FROM insplan WHERE claimformnum = '"
                      + claimForm.ClaimFormNum + "' ";
        command += DbHelper.LimitAnd(1);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 1) return false;
        //Then, delete the claimform
        command = "DELETE FROM claimform "
                  + "WHERE ClaimFormNum = '" + SOut.Long(claimForm.ClaimFormNum) + "'";
        Db.NonQ(command);
        command = "DELETE FROM claimformitem "
                  + "WHERE ClaimFormNum = '" + SOut.Long(claimForm.ClaimFormNum) + "'";
        Db.NonQ(command);
        return true;
    }

    public static ClaimForm GetClaimForm(long claimFormNum)
    {
        return GetFirstOrDefault(x => x.ClaimFormNum == claimFormNum);
    }

    public static List<ClaimForm> GetInternalClaims()
    {
        var listClaimFormsInternal = new List<ClaimForm>();
        var resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        //No way to refactor dictionaryEntry out.
        foreach (DictionaryEntry item in resourceSet)
        {
            if (!item.Key.ToString().StartsWith("ClaimForm")) continue;
            //Resources that start with ClaimForm are serialized ClaimForm objects in XML.
            var claimForm = DeserializeClaimForm("", item.Value.ToString());
            claimForm.IsInternal = true;
            listClaimFormsInternal.Add(claimForm);
        }

        return listClaimFormsInternal;
    }

    public static long Reassign(long claimFormNumOld, long claimFormNumNew)
    {
        var command = "UPDATE insplan SET ClaimFormNum=" + SOut.Long(claimFormNumNew)
                                                         + " WHERE ClaimFormNum=" + SOut.Long(claimFormNumOld);
        return Db.NonQ(command);
    }

    public static void SetDefaultClaimForm(string claimFormDescriptFrom, string claimFormDescriptTo)
    {
        var claimFormFrom = GetDeepCopy().Find(x => x.Description.ToLower() == claimFormDescriptFrom.ToLower());
        var claimFormTo = GetDeepCopy().Find(x => x.Description.ToLower() == claimFormDescriptTo.ToLower());
        var defaultClaimFormNum = PrefC.GetLong(PrefName.DefaultClaimForm);
        if (claimFormFrom != null && claimFormTo != null)
        {
            Reassign(claimFormFrom.ClaimFormNum, claimFormTo.ClaimFormNum);
            if (defaultClaimFormNum == claimFormFrom.ClaimFormNum) Prefs.UpdateLong(PrefName.DefaultClaimForm, claimFormTo.ClaimFormNum);
        }
    }

    private class ClaimFormCache : CacheListAbs<ClaimForm>
    {
        protected override List<ClaimForm> GetCacheFromDb()
        {
            var command = "SELECT * FROM claimform";
            var listClaimForms = ClaimFormCrud.SelectMany(command);
            foreach (var cf in listClaimForms) cf.Items = ClaimFormItems.GetListForForm(cf.ClaimFormNum);
            return listClaimForms;
        }

        protected override List<ClaimForm> TableToList(DataTable dataTable)
        {
            var listClaimForms = ClaimFormCrud.TableToList(dataTable);
            foreach (var cf in listClaimForms) cf.Items = ClaimFormItems.GetListForForm(cf.ClaimFormNum);
            return listClaimForms;
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
            ClaimForms.GetTableFromCache(false);
        }

        protected override bool IsInListShort(ClaimForm item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly ClaimFormCache Cache = new();

    public static List<ClaimForm> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static ClaimForm GetFirstOrDefault(Func<ClaimForm, bool> match, bool isShort = false)
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