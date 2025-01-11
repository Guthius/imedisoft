using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.Properties;

namespace OpenDentBusiness;


public class ClaimForms
{
	/// <summary>
	///     Inserts this claimform into database and retrieves the new primary key.
	///     Assigns all claimformitems to the claimform and inserts them if the bool is true.
	/// </summary>
	public static long Insert(ClaimForm claimForm, bool includeClaimFormItems)
    {
        var claimFormNum = ClaimFormCrud.Insert(claimForm);
        if (includeClaimFormItems)
            for (var i = 0; i < claimForm.Items.Count; i++)
            {
                claimForm.Items[i].ClaimFormNum = claimForm.ClaimFormNum; //so even though the ClaimFormNum is wrong, this line fixes it.
                ClaimFormItems.Insert(claimForm.Items[i]);
            }

        return claimFormNum;
    }

	/// <summary>
	///     Can be called externally as part of the conversion sequence.  Surround with try catch.
	///     Returns the claimform object from the xml file or xml data passed in that can then be inserted if needed.
	///     If xmlData is provided then path will be ignored.  If xmlData is not provided, a valid path is required.
	/// </summary>
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

    /// <summary>
    ///     Called when cancelling out of creating a new claimform, and from the claimform window when clicking delete.
    ///     Returns true if successful or false if dependencies found.
    /// </summary>
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

    ///<summary>Returns the claim form specified by the given claimFormNum</summary>
    public static ClaimForm GetClaimForm(long claimFormNum)
    {
        return GetFirstOrDefault(x => x.ClaimFormNum == claimFormNum);
    }

    ///<summary>Returns a list of all internal claims within the OpenDentBusiness resources.  Throws exceptions.</summary>
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

    ///<summary>Returns number of insplans affected.</summary>
    public static long Reassign(long claimFormNumOld, long claimFormNumNew)
    {
        var command = "UPDATE insplan SET ClaimFormNum=" + SOut.Long(claimFormNumNew)
                                                         + " WHERE ClaimFormNum=" + SOut.Long(claimFormNumOld);
        return Db.NonQ(command);
    }

    ///<summary>Sets the Default Claim Form to the Default description passed in.</summary>
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

    #region Cache Pattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ClaimFormCache _claimFormCache = new();

    public static List<ClaimForm> GetDeepCopy(bool isShort = false)
    {
        return _claimFormCache.GetDeepCopy(isShort);
    }

    public static ClaimForm GetFirstOrDefault(Func<ClaimForm, bool> match, bool isShort = false)
    {
        return _claimFormCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _claimFormCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _claimFormCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _claimFormCache.ClearCache();
    }

    #endregion Cache Pattern
}