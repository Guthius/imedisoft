using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Medications
{
	/// <summary>
	///     Checks to see if the medication exists in the current cache.  If not, the local cache will get refreshed and
	///     then searched again.  If med is still not found, false is returned because the med does not exist.
	/// </summary>
	private static bool HasMedicationInCache(long medicationNum)
    {
        //Check if the medication exists in the cache.
        return GetContainsKey(medicationNum);
    }

    ///<summary>Only public so that the remoting works.  Do not call this from anywhere except in this class.</summary>
    public static List<Medication> GetListFromDb()
    {
        var command = "SELECT * FROM medication ORDER BY MedName"; // WHERE MedName LIKE '%"+POut.String(str)+"%' ORDER BY MedName";
        return MedicationCrud.SelectMany(command);
    }

    public static List<Medication> TableToList(DataTable table)
    {
        return MedicationCrud.TableToList(table);
    }

    ///<summary>Returns medications that contain the passed in string.  Blank for all.</summary>
    public static List<Medication> GetList(string str = "")
    {
        return GetWhere(x => str == "" || x.MedName.ToUpper().Contains(str.ToUpper()));
    }

    
    public static void Update(Medication medication)
    {
        MedicationCrud.Update(medication);
    }

    
    public static bool Update(Medication medication, Medication medicationOld)
    {
        return MedicationCrud.Update(medication, medicationOld);
    }

    
    public static long Insert(Medication medication)
    {
        return MedicationCrud.Insert(medication);
    }

    public static void InsertMany(List<Medication> listMedications, bool useExistingPK = false)
    {
        MedicationCrud.InsertMany(listMedications, useExistingPK);
    }

    ///<summary>Dependent brands and patients will already be checked.  Be sure to surround with try-catch.</summary>
    public static void Delete(Medication medication)
    {
        var s = IsInUse(medication);
        if (s != "") throw new ApplicationException(Lans.g("Medications", s));
        var command = "DELETE from medication WHERE medicationNum = '" + medication.MedicationNum + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Refreshes cache and then checks if any medication's generic num is a foreign key to med.MedicationNum aside
    ///     from itself
    /// </summary>
    public static bool IsInUseAsGeneric(Medication medication)
    {
        RefreshCache();
        //Any other medications using the given medication as a generic
        return _medicationCache.GetWhere(x => x.MedicationNum != medication.MedicationNum && x.GenericNum == medication.MedicationNum).FirstOrDefault() != null;
    }

    /// <summary>
    ///     Returns a string if medication is in use in medicationpat, allergydef, eduresources, or
    ///     preference.MedicationsIndicateNone. The string will explain where the medication is in use.
    /// </summary>
    public static string IsInUse(Medication medication)
    {
        List<string> listBrands;
        if (medication.MedicationNum == medication.GenericNum)
            listBrands = GetBrands(medication.MedicationNum);
        else
            listBrands = new List<string>();
        if (listBrands.Count > 0) return "You can not delete a medication that has brand names attached.";
        var command = "SELECT COUNT(*) FROM medicationpat WHERE MedicationNum=" + SOut.Long(medication.MedicationNum);
        if (SIn.Int(Db.GetCount(command)) != 0) return "Not allowed to delete medication because it is in use by a patient";
        command = "SELECT COUNT(*) FROM allergydef WHERE MedicationNum=" + SOut.Long(medication.MedicationNum);
        if (SIn.Int(Db.GetCount(command)) != 0) return "Not allowed to delete medication because it is in use by an allergy";
        command = "SELECT COUNT(*) FROM eduresource WHERE MedicationNum=" + SOut.Long(medication.MedicationNum);
        if (SIn.Int(Db.GetCount(command)) != 0) return "Not allowed to delete medication because it is in use by an education resource";
        command = "SELECT COUNT(*) FROM rxalert WHERE MedicationNum=" + SOut.Long(medication.MedicationNum);
        if (SIn.Int(Db.GetCount(command)) != 0) return "Not allowed to delete medication because it is in use by an Rx alert";
        //If any more tables are added here in the future, then also update GetAllInUseMedicationNums() to include the new table.
        if (PrefC.GetLong(PrefName.MedicationsIndicateNone) == medication.MedicationNum) return "Not allowed to delete medication because it is in use by a medication";
        return "";
    }

    public static List<long> GetAllInUseMedicationNums()
    {
        //If any more tables are added here in the future, then also update IsInUse() to include the new table.
        var command = "SELECT MedicationNum FROM medicationpat WHERE MedicationNum!=0 "
                      + "UNION SELECT MedicationNum FROM allergydef WHERE MedicationNum!=0 "
                      + "UNION SELECT MedicationNum FROM eduresource WHERE MedicationNum!=0 "
                      + "GROUP BY MedicationNum";
        var listMedicationNums = Db.GetListLong(command);
        if (PrefC.GetLong(PrefName.MedicationsIndicateNone) != 0) listMedicationNums.Add(PrefC.GetLong(PrefName.MedicationsIndicateNone));
        return listMedicationNums;
    }

    ///<summary>Returns a list of all patient names who are using this medication.</summary>
    public static List<string> GetPatNamesForMed(long medicationNum)
    {
        var command =
            "SELECT CONCAT(CONCAT(CONCAT(CONCAT(LName,', '),FName),' '),Preferred) FROM medicationpat,patient "
            + "WHERE medicationpat.PatNum=patient.PatNum "
            + "AND medicationpat.MedicationNum=" + SOut.Long(medicationNum);
        var table = DataCore.GetTable(command);
        var listNames = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listNames.Add(SIn.String(table.Rows[i][0].ToString()));
        return listNames;
    }

    ///<summary>Returns a list of all brands dependend on this generic. Only gets run if this is a generic.</summary>
    public static List<string> GetBrands(long medicationNum)
    {
        var command =
            "SELECT MedName FROM medication "
            + "WHERE GenericNum=" + medicationNum
            + " AND MedicationNum !=" + medicationNum; //except this med
        var table = DataCore.GetTable(command);
        var listBrands = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listBrands.Add(SIn.String(table.Rows[i][0].ToString()));
        return listBrands;
    }

    ///<summary>Returns null if not found.</summary>
    public static Medication GetMedication(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum)) return null; //Should never happen.
        return GetOne(medicationNum);
    }

    ///<summary>Deprecated.  Use GetMedication instead.</summary>
    public static Medication GetMedicationFromDb(long medicationNum)
    {
        var command = "SELECT * FROM medication WHERE MedicationNum=" + SOut.Long(medicationNum);
        return MedicationCrud.SelectOne(command);
    }

    ///<summary>Use for API. Matches without case sensitivity.</summary>
    public static Medication GetMedicationFromDbForApi(long medicationNum)
    {
        var command = "SELECT * FROM medication WHERE MedicationNum LIKE '" + SOut.Long(medicationNum) + "'";
        return MedicationCrud.SelectOne(command);
    }

    ///<summary>//Returns first medication with matching MedName, if not found returns null.</summary>
    public static Medication GetMedicationFromDbByName(string medicationName)
    {
        var command = "SELECT * FROM medication WHERE MedName='" + SOut.String(medicationName) + "' ORDER BY MedicationNum";
        var listMedications = MedicationCrud.SelectMany(command);
        if (listMedications.Count > 0) return listMedications[0];
        return null;
    }

    /// <summary>
    ///     Used by API. Returns first medication with matching MedName without case sensitivity, if not found returns
    ///     null.
    /// </summary>
    public static Medication GetMedicationFromDbByNameForApi(string medicationName)
    {
        var command = "SELECT * FROM medication WHERE MedName LIKE '" + SOut.String(medicationName) + "' ORDER BY MedicationNum";
        var listMedications = MedicationCrud.SelectMany(command);
        if (listMedications.Count > 0) return listMedications[0];
        return null;
    }

    ///<summary>Gets the generic medication for the specified medication Num. Returns null if not found.</summary>
    public static Medication GetGeneric(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum)) return null;
        return GetOne(GetOne(medicationNum).GenericNum);
    }

    ///<summary>Gets the medication name.  Also, generic in () if applicable.  Returns empty string if not found.</summary>
    public static string GetDescription(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum)) return "";
        var medication = GetOne(medicationNum);
        var medName = medication.MedName;
        if (medication.GenericNum == medication.MedicationNum) //this is generic
            return medName;
        if (!GetContainsKey(medication.GenericNum)) return medName;
        var medicationGeneric = GetOne(medication.GenericNum);
        return medName + "(" + medicationGeneric.MedName + ")";
    }

    ///<summary>Gets the medication name. Copied from GetDescription.</summary>
    public static string GetNameOnly(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum)) return "";
        return GetOne(medicationNum).MedName;
    }

    ///<summary>Gets the generic medication name, given it's generic Num.</summary>
    public static string GetGenericName(long genericNum)
    {
        if (!HasMedicationInCache(genericNum)) return "";
        return GetOne(genericNum).MedName;
    }

    /// <summary>
    ///     Gets the generic medication name, given it's generic Num.  Will search through the passed in list before
    ///     resorting to cache.
    /// </summary>
    public static string GetGenericName(long genericNum, Hashtable hashtable)
    {
        if (!hashtable.ContainsKey(genericNum))
            //Medication not found.  Refresh the cache and check again.
            return GetGenericName(genericNum);
        return ((Medication) hashtable[genericNum]).MedName;
    }

    public static List<long> GetChangedSinceMedicationNums(DateTime dateChangedSince)
    {
        var command = "SELECT MedicationNum FROM medication WHERE DateTStamp > " + SOut.DateT(dateChangedSince);
        var table = DataCore.GetTable(command);
        var listMedicationNums = new List<long>(table.Rows.Count);
        for (var i = 0; i < table.Rows.Count; i++) listMedicationNums.Add(SIn.Long(table.Rows[i]["MedicationNum"].ToString()));
        return listMedicationNums;
    }

    ///<summary>Used along with GetChangedSinceMedicationNums</summary>
    public static List<Medication> GetMultMedications(List<long> listMedicationNums)
    {
        var strMedicationNums = "";
        DataTable table;
        if (listMedicationNums.Count > 0)
        {
            for (var i = 0; i < listMedicationNums.Count; i++)
            {
                if (i > 0) strMedicationNums += "OR ";
                strMedicationNums += "MedicationNum='" + listMedicationNums[i] + "' ";
            }

            var command = "SELECT * FROM medication WHERE " + strMedicationNums;
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        return MedicationCrud.TableToList(table);
    }

    ///<summary>Deprecated.  Use MedicationPat.Refresh() instead.  Returns medication list for a specific patient.</summary>
    public static List<Medication> GetMedicationsByPat(long patNum)
    {
        var command = "SELECT medication.* "
                      + "FROM medication, medicationpat "
                      + "WHERE medication.MedicationNum=medicationpat.MedicationNum "
                      + "AND medicationpat.PatNum=" + SOut.Long(patNum);
        return MedicationCrud.SelectMany(command);
    }

    public static Medication GetMedicationFromDbByRxCui(long rxCui)
    {
        //an RxCui could be linked to multiple medications, the ORDER BY ensures we get the same medication every time we call this function
        var command = "SELECT * FROM medication WHERE RxCui=" + SOut.Long(rxCui) + " ORDER BY MedicationNum";
        return MedicationCrud.SelectOne(command);
    }

    public static List<Medication> GetAllMedsByRxCui(long rxCui)
    {
        return GetWhere(x => x.RxCui == rxCui).OrderBy(x => x.MedicationNum).ToList();
    }

    public static bool AreMedicationsEqual(Medication medication, Medication medicationOld)
    {
        if (medicationOld == null || medication == null
                                  || medicationOld.MedicationNum != medication.MedicationNum
                                  || medicationOld.MedName != medication.MedName
                                  || medicationOld.GenericNum != medication.GenericNum
                                  || medicationOld.Notes != medication.Notes
                                  || medicationOld.RxCui != medication.RxCui)
            return false;
        return true;
    }

    ///<summary>Returns the number of patients associated with the passed-in medicationNum.</summary>
    public static long CountPats(long medicationNum)
    {
        var command = "SELECT COUNT(DISTINCT medicationpat.PatNum) FROM medicationpat WHERE MedicationNum=" + SOut.Long(medicationNum);
        return SIn.Long(DataCore.GetScalar(command));
    }

    /// <summary>
    ///     Medication merge tool.  Returns the number of rows changed.  Deletes the medication associated with
    ///     medNumInto.
    /// </summary>
    public static long Merge(long medicationNumFrom, long medicationNumInto)
    {
        var listMedicationNumFKs = new List<string>(); //add any new FKs to this list.
        listMedicationNumFKs.Add("allergydef.MedicationNum");
        listMedicationNumFKs.Add("eduresource.MedicationNum");
        listMedicationNumFKs.Add("medication.GenericNum");
        listMedicationNumFKs.Add("medicationpat.MedicationNum");
        listMedicationNumFKs.Add("rxalert.MedicationNum");
        var command = "";
        long rowsChanged = 0;
        for (var i = 0; i < listMedicationNumFKs.Count; i++)
        {
            //actually change all of the FKs in the above tables.
            var listTableAndKeyNames = listMedicationNumFKs[i].Split('.').ToList();
            command = "UPDATE " + listTableAndKeyNames[0]
                                + " SET " + listTableAndKeyNames[1] + "=" + SOut.Long(medicationNumInto)
                                + " WHERE " + listTableAndKeyNames[1] + "=" + SOut.Long(medicationNumFrom);
            rowsChanged += Db.NonQ(command);
        }

        command = "SELECT medication.RxCui FROM medication WHERE MedicationNum=" + medicationNumInto; //update medicationpat's RxNorms to match medication.
        var rxNorm = DataCore.GetScalar(command);
        command = "UPDATE medicationpat SET RxCui=" + rxNorm + " WHERE MedicationNum=" + medicationNumInto;
        Db.NonQ(command);
        command = "SELECT * FROM ehrtrigger WHERE MedicationNumList LIKE '% " + SOut.Long(medicationNumFrom) + " %'";
        var listEhrTriggers = EhrTriggerCrud.SelectMany(command); //get all ehr triggers with matching mednum in mednumlist
        for (var i = 0; i < listEhrTriggers.Count; i++)
        {
            //for each trigger...
            var listMedicationNums = listEhrTriggers[i].MedicationNumList.Split(' ').ToList(); //get a list of their medicationNums.
            var containsMedicationNumInto = listMedicationNums.Any(x => x == SOut.Long(medicationNumInto));
            var strMedicationNumList = "";
            for (var j = 0; j < listMedicationNums.Count; j++)
            {
                //for each mednum in the MedicationList for the current trigger.
                var medicationNumTrimmed = listMedicationNums[j].Trim();
                if (medicationNumTrimmed == "") //because we use spaces as a buffer before and after mednums, this prevents empty spaces from being considered.
                    continue;
                if (containsMedicationNumInto)
                {
                    //if the list already contains medNumInto, 
                    if (medicationNumTrimmed == SOut.Long(medicationNumFrom))
                    {
                    }
                    else
                    {
                        strMedicationNumList += " " + medicationNumTrimmed + " ";
                    }
                }
                else
                {
                    //if the list doesn't contain medNumInto
                    if (medicationNumTrimmed == SOut.Long(medicationNumFrom))
                        strMedicationNumList += " " + medicationNumInto + " "; //replace medNumFrom with medNumInto
                    else
                        strMedicationNumList += " " + medicationNumTrimmed + " ";
                }
            } //end for each mednum

            listEhrTriggers[i].MedicationNumList = strMedicationNumList;
            EhrTriggers.Update(listEhrTriggers[i]); //update the ehrtrigger list.
        } //end for each trigger

        MedicationCrud.Delete(medicationNumFrom); //finally, delete the mednum.
        return rowsChanged;
    }

    #region Cache Pattern

    private class MedicationCache : CacheDictAbs<Medication, long, Medication>
    {
        protected override List<Medication> GetCacheFromDb()
        {
            var command = "SELECT * FROM medication ORDER BY MedName";
            return MedicationCrud.SelectMany(command);
        }

        protected override List<Medication> TableToList(DataTable dataTable)
        {
            return MedicationCrud.TableToList(dataTable);
        }

        protected override Medication Copy(Medication item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<long, Medication> dict)
        {
            return MedicationCrud.ListToTable(dict.Values.ToList(), "Medication");
        }

        protected override void FillCacheIfNeeded()
        {
            Medications.GetTableFromCache(false);
        }

        protected override long GetDictKey(Medication item)
        {
            return item.MedicationNum;
        }

        protected override Medication GetDictValue(Medication item)
        {
            return item;
        }

        protected override Medication CopyValue(Medication medication)
        {
            return medication.Copy();
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly MedicationCache _medicationCache = new();

    public static Medication GetFirstOrDefault(Func<Medication, bool> match, bool isShort = false)
    {
        return _medicationCache.GetFirstOrDefault(match, isShort);
    }

    public static Medication GetOne(long medicationNum)
    {
        return _medicationCache.GetOne(medicationNum);
    }

    public static List<Medication> GetWhere(Func<Medication, bool> match, bool isShort = false)
    {
        return _medicationCache.GetWhere(match, isShort);
    }

    public static bool GetContainsKey(long medicationNum)
    {
        return _medicationCache.GetContainsKey(medicationNum);
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
        _medicationCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _medicationCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _medicationCache.ClearCache();
    }

    #endregion Cache Pattern
}