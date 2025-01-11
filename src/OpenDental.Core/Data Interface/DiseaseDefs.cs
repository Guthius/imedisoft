using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DiseaseDefs
{
    ///<summary>Gets a DiseaseDef from the database. Returns null if not found.</summary>
    public static DiseaseDef GetOne(long diseaseDefNum)
    {
        var command = "SELECT * FROM diseasedef WHERE DiseaseDefNum=" + SOut.Long(diseaseDefNum);
        return DiseaseDefCrud.SelectOne(command);
    }

    ///<summary>Gets all DiseaseDefs from the database. Returns empty list if not found.</summary>
    public static List<DiseaseDef> GetDiseaseDefsForApi(int limit, int offset)
    {
        var command = "SELECT * FROM diseasedef ";
        command += "ORDER BY DiseaseDefNum " //same fixed order each time
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return DiseaseDefCrud.SelectMany(command);
    }

    ///<summary>Fixes item orders in DB if needed. Returns true if changes were made.</summary>
    public static bool FixItemOrders()
    {
        var isChanged = false;
        var listDiseaseDefs = GetDeepCopy();
        listDiseaseDefs.Sort(SortItemOrder);
        for (var i = 0; i < listDiseaseDefs.Count; i++)
        {
            if (listDiseaseDefs[i].ItemOrder == i) continue;
            listDiseaseDefs[i].ItemOrder = i;
            Update(listDiseaseDefs[i]);
            isChanged = true;
        }

        if (isChanged) RefreshCache();
        return isChanged;
    }

    
    public static void Update(DiseaseDef diseaseDef)
    {
        DiseaseDefCrud.Update(diseaseDef);
    }

    
    public static bool Update(DiseaseDef diseaseDef, DiseaseDef diseaseDefOld)
    {
        return DiseaseDefCrud.Update(diseaseDef, diseaseDefOld);
    }

    
    public static long Insert(DiseaseDef diseaseDef)
    {
        var retVal = DiseaseDefCrud.Insert(diseaseDef);
        return retVal;
    }

    ///<summary>Returns a list of diseasedefs that are in use, and should not be deleted.</summary>
    public static List<long> ValidateDeleteList()
    {
        //Get non-hidden diseasedefnums
        var listDiseaseDefNums = new List<long>();
        var command = "SELECT DiseaseDefNum FROM diseasedef";
        try
        {
            listDiseaseDefNums.AddRange(Db.GetListLong(command));
        }
        catch
        {
            //Do Nothing
        }

        var listDiseaseDefNumsNotDeletable = new List<long>();
        if (listDiseaseDefNums == null || listDiseaseDefNums.Count < 1) return listDiseaseDefNumsNotDeletable;
        //In use by preference
        if (listDiseaseDefNums.Contains(PrefC.GetLong(PrefName.ProblemsIndicateNone))) listDiseaseDefNumsNotDeletable.Add(PrefC.GetLong(PrefName.ProblemsIndicateNone));
        //Validate patient attached
        command = "SELECT diseasedef.DiseaseDefNum "
                  + "FROM diseasedef "
                  + "WHERE EXISTS(SELECT 1 FROM patient "
                  + "INNER JOIN disease ON patient.PatNum=disease.PatNum "
                  + "WHERE disease.DiseaseDefNum=diseasedef.DiseaseDefNum)";
        try
        {
            listDiseaseDefNumsNotDeletable.AddRange(Db.GetListLong(command));
        }
        catch
        {
            //Do Nothing
        }

        //Validate edu resource attached
        command = "SELECT diseasedef.DiseaseDefNum "
                  + "FROM diseasedef "
                  + "WHERE EXISTS(SELECT 1 FROM eduresource "
                  + "WHERE eduresource.DiseaseDefNum=diseasedef.DiseaseDefNum)";
        try
        {
            listDiseaseDefNumsNotDeletable.AddRange(Db.GetListLong(command));
        }
        catch
        {
            //Do Nothing
        }

        //Validate family health history attached
        command = "SELECT diseasedef.DiseaseDefNum "
                  + "FROM diseasedef "
                  + "WHERE EXISTS(SELECT 1 FROM patient "
                  + "INNER JOIN familyhealth ON patient.PatNum=familyhealth.PatNum "
                  + "WHERE familyhealth.DiseaseDefNum=diseasedef.DiseaseDefNum)";
        try
        {
            listDiseaseDefNumsNotDeletable.AddRange(Db.GetListLong(command));
        }
        catch
        {
            //Do Nothing
        }

        return listDiseaseDefNumsNotDeletable;
    }

    ///<summary>Returns a bool indicating if the passed in diseasedefnum is currently in use.</summary>
    public static bool IsDiseaseDefInUse(long diseaseDefNum)
    {
        if (diseaseDefNum == 0) return false;

        //In use by preference
        if (diseaseDefNum == PrefC.GetLong(PrefName.ProblemsIndicateNone)) return true;
        //Validate patient attached
        var command = "SELECT EXISTS (SELECT * FROM disease WHERE DiseaseDefNum=" + SOut.Long(diseaseDefNum) + ")";
        if (DataCore.GetScalar(command) == "1") return true;
        //Validate edu resource attached
        command = "SELECT EXISTS (SELECT * FROM eduresource WHERE DiseaseDefNum=" + SOut.Long(diseaseDefNum) + ")";
        if (DataCore.GetScalar(command) == "1") return true;
        //Validate family health history attached
        command = "SELECT EXISTS (SELECT * FROM familyhealth WHERE DiseaseDefNum=" + SOut.Long(diseaseDefNum) + ")";
        if (DataCore.GetScalar(command) == "1") return true;
        return false;
    }

    ///<summary>Returns the name of the disease, whether hidden or not.</summary>
    public static string GetName(long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.DiseaseDefNum == diseaseDefNum);
        if (diseaseDef == null) return "";
        return diseaseDef.DiseaseName;
    }

    /// <summary>
    ///     Returns the name of the disease based on SNOMEDCode, then if no match tries ICD9Code, then if no match returns
    ///     empty string. Used in EHR Patient Lists.
    /// </summary>
    public static string GetNameByCode(string SNOMEDorICD9Code)
    {
        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == SNOMEDorICD9Code);
        if (diseaseDef != null) return diseaseDef.DiseaseName;
        diseaseDef = GetFirstOrDefault(x => x.ICD9Code == SNOMEDorICD9Code);
        if (diseaseDef != null) return diseaseDef.DiseaseName;
        return "";
    }

    /// <summary>
    ///     Returns the DiseaseDefNum based on SNOMEDCode, then if no match tries ICD9Code, then if no match tries
    ///     ICD10Code, then if no match returns 0. Used in EHR Patient Lists and when automatically inserting pregnancy Dx from
    ///     FormVitalsignEdit2014.  Will match hidden diseases.
    /// </summary>
    public static long GetNumFromCode(string codeValue)
    {
        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == codeValue);
        if (diseaseDef != null) return diseaseDef.DiseaseDefNum;
        diseaseDef = GetFirstOrDefault(x => x.ICD9Code == codeValue);
        if (diseaseDef != null) return diseaseDef.DiseaseDefNum;
        diseaseDef = GetFirstOrDefault(x => x.Icd10Code == codeValue);
        if (diseaseDef != null) return diseaseDef.DiseaseDefNum;
        return 0;
    }

    /// <summary>
    ///     Returns the DiseaseDefNum based on SNOMEDCode.  If no match or if SnomedCode is an empty string returns 0.
    ///     Only matches SNOMEDCode, not ICD9 or ICD10.
    /// </summary>
    public static long GetNumFromSnomed(string snomedCode)
    {
        if (snomedCode == "") return 0;
        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == snomedCode);
        if (diseaseDef == null) return 0;
        return diseaseDef.DiseaseDefNum;
    }

    ///<summary>Returns the diseaseDef with the specified num.  Will match hidden diseasedefs.</summary>
    public static DiseaseDef GetItem(long diseaseDefNum)
    {
        return GetFirstOrDefault(x => x.DiseaseDefNum == diseaseDefNum);
    }

    /// <summary>
    ///     Returns the diseaseDefNum that exactly matches the specified string.  Used in import functions when you only
    ///     have the name to work with.  Can return 0 if no match.  Does not match hidden diseases.
    /// </summary>
    public static long GetNumFromName(string diseaseName)
    {
        return GetNumFromName(diseaseName, false);
    }

    /// <summary>
    ///     Returns the diseaseDefNum that exactly matches the specified string.  Will return 0 if no match.
    ///     Set matchHidden to true to match hidden diseasedefs as well.
    /// </summary>
    public static long GetNumFromName(string diseaseName, bool matchHidden)
    {
        var diseaseDef = _diseaseDefCache.GetFirstOrDefault(x => x.DiseaseName == diseaseName, !matchHidden);
        if (diseaseDef == null) return 0;
        return diseaseDef.DiseaseDefNum;
    }

    /// <summary>
    ///     Used by API. Returns a single diseaseDef with DiseaseName exactly matching the supplied string, or null if no
    ///     match found.
    /// </summary>
    public static DiseaseDef GetDiseaseDefByName(string diseaseName)
    {
        var command = "SELECT * FROM diseasedef WHERE DiseaseName LIKE '" + SOut.String(diseaseName) + "'"; //matches regardless of case 
        return DiseaseDefCrud.SelectOne(command);
    }

    /// <summary>
    ///     Returns the diseasedef that has a name exactly matching the specified string. Returns null if no match.  Does
    ///     not match hidden diseases.
    /// </summary>
    public static DiseaseDef GetFromName(string diseaseName)
    {
        return _diseaseDefCache.GetFirstOrDefault(x => x.DiseaseName == diseaseName, true);
    }

    
    public static List<long> GetChangedSinceDiseaseDefNums(DateTime dateT)
    {
        var command = "SELECT DiseaseDefNum FROM diseasedef WHERE DateTStamp > " + SOut.DateT(dateT);
        var table = DataCore.GetTable(command);
        var listDiseaseDefNums = new List<long>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++) listDiseaseDefNums.Add(SIn.Long(table.Rows[i]["DiseaseDefNum"].ToString()));
        return listDiseaseDefNums;
    }

    ///<summary>Used along with GetChangedSinceDiseaseDefNums</summary>
    public static List<DiseaseDef> GetMultDiseaseDefs(List<long> listDiseaseDefNums)
    {
        var strDiseaseDefNums = "";
        DataTable table;
        if (listDiseaseDefNums.Count > 0)
        {
            for (var i = 0; i < listDiseaseDefNums.Count; i++)
            {
                if (i > 0) strDiseaseDefNums += "OR ";
                strDiseaseDefNums += "DiseaseDefNum='" + listDiseaseDefNums[i] + "' ";
            }

            var command = "SELECT * FROM diseasedef WHERE " + strDiseaseDefNums;
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        var listDiseaseDefs = DiseaseDefCrud.TableToList(table);
        return listDiseaseDefs;
    }

    /// <summary>
    ///     Returns true if there exists a disease def containing the SNOMED code passed in, excluding the disease def
    ///     specified by diseaseDefNum.
    /// </summary>
    public static bool ContainsSnomed(string snomedCode, long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == snomedCode && x.DiseaseDefNum != diseaseDefNum);
        return diseaseDef != null;
    }

    /// <summary>
    ///     Returns true if there exists a disease def containing the ICD9 code passed in, excluding the disease def
    ///     specified by diseaseDefNum.
    /// </summary>
    public static bool ContainsICD9(string icd9Code, long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.ICD9Code == icd9Code && x.DiseaseDefNum != diseaseDefNum);
        return diseaseDef != null;
    }

    /// <summary>
    ///     Returns true if there exists a disease def containing the ICD10 code passed in, excluding the disease def
    ///     specified by diseaseDefNum.
    /// </summary>
    public static bool ContainsIcd10(string icd10Code, long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.Icd10Code == icd10Code && x.DiseaseDefNum != diseaseDefNum);
        return diseaseDef != null;
    }

    ///<summary>Sync pattern, must sync entire table. Probably only to be used in the master problem list window.</summary>
    public static void Sync(List<DiseaseDef> listDiseaseDefs, List<DiseaseDef> listDiseaseDefsOld)
    {
        DiseaseDefCrud.Sync(listDiseaseDefs, listDiseaseDefsOld);
    }

    /// <summary>
    ///     Get all diseasedefs that have a pregnancy code that applies to the three CQM measures with pregnancy as an
    ///     exclusion condition.
    /// </summary>
    public static List<DiseaseDef> GetAllPregDiseaseDefs()
    {
        var dictionaryEhrCodesPregCQMs = EhrCodes.GetCodesExistingInAllSets(new List<string> {"2.16.840.1.113883.3.600.1.1623", "2.16.840.1.113883.3.526.3.378"});
        var listDiseaseDefsToReturn = new List<DiseaseDef>();
        var listDiseaseDefs = GetDeepCopy();
        for (var i = 0; i < listDiseaseDefs.Count; i++)
        {
            if (dictionaryEhrCodesPregCQMs.ContainsKey(listDiseaseDefs[i].ICD9Code) && dictionaryEhrCodesPregCQMs[listDiseaseDefs[i].ICD9Code] == "ICD9CM")
            {
                listDiseaseDefsToReturn.Add(listDiseaseDefs[i]);
                continue;
            }

            if (dictionaryEhrCodesPregCQMs.ContainsKey(listDiseaseDefs[i].Icd10Code) && dictionaryEhrCodesPregCQMs[listDiseaseDefs[i].Icd10Code] == "ICD10CM")
            {
                listDiseaseDefsToReturn.Add(listDiseaseDefs[i]);
                continue;
            }

            if (dictionaryEhrCodesPregCQMs.ContainsKey(listDiseaseDefs[i].SnomedCode) && dictionaryEhrCodesPregCQMs[listDiseaseDefs[i].SnomedCode] == "SNOMEDCT") listDiseaseDefsToReturn.Add(listDiseaseDefs[i]);
        }

        return listDiseaseDefsToReturn;
    }

    ///<summary>Sorts alphabetically by DiseaseName, then by PK.</summary>
    public static int SortAlphabetically(DiseaseDef diseaseDef, DiseaseDef diseaseDefOther)
    {
        if (diseaseDef.DiseaseName != diseaseDefOther.DiseaseName) return diseaseDef.DiseaseName.CompareTo(diseaseDefOther.DiseaseName);
        return diseaseDef.DiseaseDefNum.CompareTo(diseaseDefOther.DiseaseDefNum);
    }

    public static int SortItemOrder(DiseaseDef diseaseDef, DiseaseDef diseaseDefOther)
    {
        if (diseaseDef.ItemOrder != diseaseDefOther.ItemOrder) return diseaseDef.ItemOrder.CompareTo(diseaseDefOther.ItemOrder);
        return diseaseDef.DiseaseDefNum.CompareTo(diseaseDefOther.DiseaseDefNum);
    }

    /// <summary>
    ///     Returns DiseaseDefNum for passed in pregnancy code. If pregnacy code doesn't have associated DiseaseDef,
    ///     creates DiseaseDef.
    /// </summary>
    public static long GetDefNumForDefaultPreg(string pregnancyCode)
    {
        var diseaseDefNum = GetNumFromCode(pregnancyCode); //see if the code is attached to a valid diseasedef
        if (diseaseDefNum != 0) return diseaseDefNum;
        //no diseasedef in db for the default code, create and insert def
        var diseaseDef = new DiseaseDef();
        diseaseDef.DiseaseName = "Pregnant";
        var pregnancyCodeSys = PrefC.GetString(PrefName.PregnancyDefaultCodeSystem);
        switch (pregnancyCodeSys)
        {
            case "ICD9CM":
                diseaseDef.ICD9Code = pregnancyCode;
                break;
            case "ICD10CM":
                diseaseDef.Icd10Code = pregnancyCode;
                break;
            case "SNOMEDCT":
                diseaseDef.SnomedCode = pregnancyCode;
                break;
        }

        diseaseDefNum = Insert(diseaseDef);
        RefreshCache();
        Signalods.SetInvalid(InvalidType.Diseases);
        SecurityLogs.MakeLogEntry(EnumPermType.ProblemDefEdit, 0, diseaseDef.DiseaseName + " added.");
        return diseaseDefNum;
    }

    #region CachePattern

    private class DiseaseDefCache : CacheListAbs<DiseaseDef>
    {
        protected override List<DiseaseDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM diseasedef ORDER BY ItemOrder";
            return DiseaseDefCrud.SelectMany(command);
        }

        protected override List<DiseaseDef> TableToList(DataTable dataTable)
        {
            return DiseaseDefCrud.TableToList(dataTable);
        }

        protected override DiseaseDef Copy(DiseaseDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<DiseaseDef> items)
        {
            return DiseaseDefCrud.ListToTable(items, "DiseaseDef");
        }

        protected override void FillCacheIfNeeded()
        {
            DiseaseDefs.GetTableFromCache(false);
        }

        protected override bool IsInListShort(DiseaseDef item)
        {
            return !item.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly DiseaseDefCache _diseaseDefCache = new();

    public static int GetCount(bool isShort = false)
    {
        return _diseaseDefCache.GetCount(isShort);
    }

    public static List<DiseaseDef> GetDeepCopy(bool isShort = false)
    {
        return _diseaseDefCache.GetDeepCopy(isShort);
    }

    public static List<DiseaseDef> GetWhere(Predicate<DiseaseDef> match, bool isShort = false)
    {
        return _diseaseDefCache.GetWhere(match, isShort);
    }

    public static DiseaseDef GetFirstOrDefault(Func<DiseaseDef, bool> match, bool isShort = false)
    {
        return _diseaseDefCache.GetFirstOrDefault(match, isShort);
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
        _diseaseDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _diseaseDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _diseaseDefCache.ClearCache();
    }

    #endregion

    /*public static DiseaseDef GetByICD9Code(string ICD9Code) {///<summary>Returns the diseasedef that has a name exactly matching the specified string. Returns null if no match.  Does not match hidden diseases.</summary>
            Meth.NoCheckMiddleTierRole();
            for(int i=0;i<List.Length;i++) {
                if(ICD9Code==List[i].ICD9Code) {
                    return List[i];
                }
            }
            return null;
        }*/
}