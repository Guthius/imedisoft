using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Medications
{
    private static bool HasMedicationInCache(long medicationNum)
    {
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

    public static void Insert(Medication medication)
    {
        MedicationCrud.Insert(medication);
    }

    public static void Delete(Medication medication)
    {
        var message = IsInUse(medication);

        if (!string.IsNullOrEmpty(message))
        {
            throw new ApplicationException(message);
        }

        Db.NonQ("DELETE from medication WHERE medicationNum = " + medication.MedicationNum);
    }

    public static string IsInUse(Medication medication)
    {
        var brands = medication.MedicationNum == medication.GenericNum ? GetBrands(medication.MedicationNum) : [];

        if (brands.Count > 0)
        {
            return "You can not delete a medication that has brand names attached.";
        }

        if (SIn.Int(Db.GetCount("SELECT COUNT(*) FROM medicationpat WHERE MedicationNum = " + medication.MedicationNum)) != 0)
        {
            return "Not allowed to delete medication because it is in use by a patient";
        }

        if (SIn.Int(Db.GetCount("SELECT COUNT(*) FROM allergydef WHERE MedicationNum = " + medication.MedicationNum)) != 0)
        {
            return "Not allowed to delete medication because it is in use by an allergy";
        }

        if (SIn.Int(Db.GetCount("SELECT COUNT(*) FROM eduresource WHERE MedicationNum = " + medication.MedicationNum)) != 0)
        {
            return "Not allowed to delete medication because it is in use by an education resource";
        }

        if (SIn.Int(Db.GetCount("SELECT COUNT(*) FROM rxalert WHERE MedicationNum = " + medication.MedicationNum)) != 0)
        {
            return "Not allowed to delete medication because it is in use by an Rx alert";
        }

        return PrefC.GetLong(PrefName.MedicationsIndicateNone) == medication.MedicationNum
            ? "Not allowed to delete medication because it is in use by a medication"
            : "";
    }

    public static List<long> GetAllInUseMedicationNums()
    {
        var medicationNums = Db.GetListLong(
            "SELECT MedicationNum FROM medicationpat WHERE MedicationNum != 0 " +
            "UNION SELECT MedicationNum FROM allergydef WHERE MedicationNum != 0 " +
            "UNION SELECT MedicationNum FROM eduresource WHERE MedicationNum != 0 " +
            "GROUP BY MedicationNum");

        if (PrefC.GetLong(PrefName.MedicationsIndicateNone) != 0)
        {
            medicationNums.Add(PrefC.GetLong(PrefName.MedicationsIndicateNone));
        }

        return medicationNums;
    }

    public static List<string> GetPatNamesForMed(long medicationNum)
    {
        var dataTable = DataCore.GetTable(
            $"""
             SELECT CONCAT(CONCAT(CONCAT(CONCAT(LName, ', '), FName), ' '), Preferred) 
             FROM medicationpat, patient 
             WHERE medicationpat.PatNum = patient.PatNum 
             AND medicationpat.MedicationNum = {medicationNum}
             """);

        var patientNames = new List<string>();
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            patientNames.Add(SIn.String(dataTable.Rows[i][0].ToString()));
        }

        return patientNames;
    }

    public static List<string> GetBrands(long medicationNum)
    {
        var dataTable = DataCore.GetTable("SELECT MedName FROM medication WHERE GenericNum = " + medicationNum + " AND MedicationNum != " + medicationNum);

        var brands = new List<string>();
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            brands.Add(SIn.String(dataTable.Rows[i][0].ToString()));
        }

        return brands;
    }

    public static Medication GetMedication(long medicationNum)
    {
        return !HasMedicationInCache(medicationNum) ? null : GetOne(medicationNum);
    }

    public static Medication GetGeneric(long medicationNum)
    {
        return !HasMedicationInCache(medicationNum) ? null : GetOne(GetOne(medicationNum).GenericNum);
    }

    public static string GetDescription(long medicationNum)
    {
        if (!HasMedicationInCache(medicationNum))
        {
            return "";
        }

        var medication = GetOne(medicationNum);

        var medName = medication.MedName;
        if (medication.GenericNum == medication.MedicationNum || !GetContainsKey(medication.GenericNum))
        {
            return medName;
        }

        var medicationGeneric = GetOne(medication.GenericNum);

        return medName + "(" + medicationGeneric.MedName + ")";
    }

    public static string GetGenericName(long genericNum)
    {
        return !HasMedicationInCache(genericNum) ? "" : GetOne(genericNum).MedName;
    }

    public static List<Medication> GetMedicationsByPat(long patNum)
    {
        return MedicationCrud.SelectMany(
            "SELECT medication.* " +
            "FROM medication, medicationpat " +
            "WHERE medication.MedicationNum=medicationpat.MedicationNum " +
            "AND medicationpat.PatNum=" + patNum);
    }

    public static List<Medication> GetAllMedsByRxCui(long rxCui)
    {
        return GetWhere(x => x.RxCui == rxCui).OrderBy(x => x.MedicationNum).ToList();
    }

    private class MedicationCache : CacheDictAbs<Medication, long, Medication>
    {
        protected override List<Medication> GetCacheFromDb()
        {
            return MedicationCrud.SelectMany("SELECT * FROM medication ORDER BY MedName");
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
            GetTableFromCache(false);
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

    public static Medication GetFirstOrDefault(Func<Medication, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static Medication GetOne(long medicationNum)
    {
        return Cache.GetOne(medicationNum);
    }

    public static List<Medication> GetWhere(Func<Medication, bool> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static bool GetContainsKey(long medicationNum)
    {
        return Cache.GetContainsKey(medicationNum);
    }
}