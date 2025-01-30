using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Medications
{
    private static bool HasMedicationInCache(long medicationNum)
    {
        //Check if the medication exists in the cache.
        return GetContainsKey(medicationNum);
    }

    public static List<Medication> GetList(string str = "")
    {
        return GetWhere(x => str == "" || x.MedName.ToUpper().Contains(str.ToUpper()));
    }

    public static void Update(Medication medication)
    {
        MedicationCrud.Update(medication);
    }

    public static long Insert(Medication medication)
    {
        return MedicationCrud.Insert(medication);
    }

    public static void Delete(Medication medication)
    {
        var s = IsInUse(medication);
        if (s != "") throw new ApplicationException(Lans.g("Medications", s));
        var command = "DELETE from medication WHERE medicationNum = '" + medication.MedicationNum + "'";
        Db.NonQ(command);
    }

    public static bool IsInUseAsGeneric(Medication medication)
    {
        RefreshCache();
        //Any other medications using the given medication as a generic
        return Cache.GetWhere(x => x.MedicationNum != medication.MedicationNum && x.GenericNum == medication.MedicationNum).FirstOrDefault() != null;
    }

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

    public static Medication GetMedication(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum)) return null; //Should never happen.
        return GetOne(medicationNum);
    }

    public static Medication GetMedicationFromDb(long medicationNum)
    {
        var command = "SELECT * FROM medication WHERE MedicationNum=" + SOut.Long(medicationNum);
        return MedicationCrud.SelectOne(command);
    }

    public static Medication GetMedicationFromDbByName(string medicationName)
    {
        var command = "SELECT * FROM medication WHERE MedName='" + SOut.String(medicationName) + "' ORDER BY MedicationNum";
        var listMedications = MedicationCrud.SelectMany(command);
        if (listMedications.Count > 0) return listMedications[0];
        return null;
    }

    public static Medication GetGeneric(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum)) return null;
        return GetOne(GetOne(medicationNum).GenericNum);
    }

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

    public static string GetNameOnly(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum)) return "";
        return GetOne(medicationNum).MedName;
    }

    public static string GetGenericName(long genericNum)
    {
        if (!HasMedicationInCache(genericNum)) return "";
        return GetOne(genericNum).MedName;
    }

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

    public static long CountPats(long medicationNum)
    {
        var command = "SELECT COUNT(DISTINCT medicationpat.PatNum) FROM medicationpat WHERE MedicationNum=" + SOut.Long(medicationNum);
        return SIn.Long(DataCore.GetScalar(command));
    }

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

    private static readonly MedicationCache Cache = new();

    public static Medication GetFirstOrDefault(Func<Medication, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static Medication GetOne(long medicationNum)
    {
        return Cache.GetOne(medicationNum);
    }

    public static List<Medication> GetWhere(Func<Medication, bool> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static bool GetContainsKey(long medicationNum)
    {
        return Cache.GetContainsKey(medicationNum);
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