using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class Providers
{
    public static List<long> GetInvalidProvsByTermDate(List<long> listProvNums, DateTime dateCompare)
    {
        return GetWhere(x => listProvNums.Any(y => y == x.ProvNum) && x.DateTerm.Year > 1880 && x.DateTerm.Date < dateCompare.Date)
            .Select(x => x.ProvNum).ToList();
    }

    public static string CheckApptProvidersTermDates(Appointment apt, bool isSetComplete = false)
    {
        var message = "";
        var listProvNums = new List<long> {apt.ProvNum, apt.ProvHyg};
        var listInvalidProvNums = GetInvalidProvsByTermDate(listProvNums, apt.AptDateTime);
        if (listInvalidProvNums.Count == 0) return message;
        if (listInvalidProvNums.Contains(apt.ProvNum)) message += "provider";
        if (listInvalidProvNums.Contains(apt.ProvHyg))
        {
            if (message != "") message += " and ";
            message += "hygienist";
        }

        if (listInvalidProvNums.Contains(apt.ProvNum) && listInvalidProvNums.Contains(apt.ProvHyg)) //used for grammar
            message = "The " + message + " selected for this appointment have Term Dates prior to the selected day and time. "
                      + "Please select another " + message + (isSetComplete ? " to set the appointment complete." : ".");
        else
            message = "The " + message + " selected for this appointment has a Term Date prior to the selected day and time. "
                      + "Please select another " + message + (isSetComplete ? " to set the appointment complete." : ".");
        Lans.g("Providers", message);
        return message;
    }

    public static List<Provider> GetAll()
    {
        return ProviderCrud.SelectMany("SELECT * FROM provider");
    }

    public static void Update(Provider provider)
    {
        ProviderCrud.Update(provider);
    }

    public static long Insert(Provider provider)
    {
        return ProviderCrud.Insert(provider);
    }

    public static void MoveDownBelow(Provider provider)
    {
        Db.NonQ("UPDATE provider SET ItemOrder=ItemOrder+1 WHERE ProvNum!=" + provider.ProvNum + " AND ItemOrder>=" + provider.ItemOrder);
    }

    public static void Delete(Provider prov)
    {
        Db.NonQ("DELETE from provider WHERE provnum = " + prov.ProvNum);
    }

    public static string GetAbbr(long provNum, bool includeHidden = false)
    {
        var prov = Cache.GetFirstOrDefault(x => x.ProvNum == provNum);
        if (prov == null)
        {
            return "";
        }

        return includeHidden ? prov.GetAbbr() : prov.Abbr;
    }

    public static string GetLName(long provNum, List<Provider> listProvs = null)
    {
        var provider = listProvs == null ? GetLastOrDefault(x => x.ProvNum == provNum) : listProvs.LastOrDefault(x => x.ProvNum == provNum);
        return provider == null ? "" : provider.LName;
    }

    public static string GetFormalName(long provNum)
    {
        var provider = GetLastOrDefault(x => x.ProvNum == provNum);
        var retStr = "";
        if (provider != null)
        {
            retStr = provider.FName + " " + provider.LName;
            if (provider.Suffix != "") retStr += ", " + provider.Suffix;
        }

        return retStr;
    }

    public static string GetLongDesc(long provNum)
    {
        var provider = GetFirstOrDefault(x => x.ProvNum == provNum);
        return provider == null ? "" : provider.GetLongDesc();
    }

    public static Color GetColor(long provNum)
    {
        var prov = Cache.GetFirstOrDefault(x => x.ProvNum == provNum);
        if (prov == null || prov.ProvColor.ToArgb() == Color.Transparent.ToArgb() || prov.ProvColor.ToArgb() == 0) return Color.White;
        return prov.ProvColor;
    }

    public static Color GetOutlineColor(long provNum)
    {
        var prov = Cache.GetFirstOrDefault(x => x.ProvNum == provNum);
        if (prov == null || prov.OutlineColor.ToArgb() == Color.Transparent.ToArgb() || prov.OutlineColor.ToArgb() == 0) return Color.Black;
        return prov.OutlineColor;
    }

    public static bool GetIsSec(long provNum)
    {
        var prov = Cache.GetFirstOrDefault(x => x.ProvNum == provNum);
        return prov is {IsSecondary: true};
    }

    public static Provider GetProv(long provNum)
    {
        return Cache.GetFirstOrDefault(x => x.ProvNum == provNum);
    }

    public static Provider GetProvFromDb(long provNum)
    {
        return provNum == 0 ? null : ProviderCrud.SelectOne(provNum);
    }

    public static List<Provider> GetProvsByProvNums(List<long> listProvNums, bool isShort = false)
    {
        return GetWhere(x => listProvNums.Contains(x.ProvNum), isShort);
    }

    public static List<Provider> GetProvsByFLName(string lName, string fName)
    {
        if (string.IsNullOrWhiteSpace(lName) || string.IsNullOrWhiteSpace(fName)) return [];
        return GetWhere(x =>
            string.Equals(x.LName, lName, StringComparison.CurrentCultureIgnoreCase) &&
            string.Equals(x.FName, fName, StringComparison.CurrentCultureIgnoreCase));
    }

    public static List<Provider> GetProvsByNpiOrMedicaidId(string npi, string medicaidId)
    {
        var retval = new List<Provider>();
        if (npi == "") return retval;
        var listProvs = GetDeepCopy();
        for (var i = 0; i < listProvs.Count; i++)
            //if the prov has a NPI set and it's a match, add this prov to the list
            if (listProvs[i].NationalProvID != "")
            {
                if (listProvs[i].NationalProvID.Trim().ToLower() == npi.Trim().ToLower()) retval.Add(listProvs[i].Copy());
            }
            else
            {
                //if the NPI is blank and the Medicaid ID is set and it's a match, add this prov to the list
                if (listProvs[i].MedicaidID != ""
                    && listProvs[i].MedicaidID.Trim().ToLower() == medicaidId.Trim().ToLower())
                    retval.Add(listProvs[i].Copy());
            }

        return retval;
    }

    public static Provider GetProvByEcwID(string eID)
    {
        if (eID == "") return null;
        var provider = GetFirstOrDefault(x => x.EcwID == eID);
        if (provider != null) return provider;
        RefreshCache();
        return GetFirstOrDefault(x => x.EcwID == eID);
    }

    public static int GetIndex(long provNum)
    {
        return Cache.GetFindIndex(x => x.ProvNum == provNum, true);
    }

    public static long GetBillingProvNum(long treatProv, long clinicNum)
    {
        if (clinicNum == 0)
        {
            //If clinics are disabled don't use the clinic defaults, even if a clinicnum was passed in.
            if (PrefC.GetLong(PrefName.InsBillingProv) == 0) //default=0
                return PrefC.GetLong(PrefName.PracticeDefaultProv);

            if (PrefC.GetLong(PrefName.InsBillingProv) == -1) //treat=-1
                return treatProv;

            return PrefC.GetLong(PrefName.InsBillingProv);
        } //Using clinics, and a clinic was pased in

        var clinicInsBillingProv = Clinics.GetClinic(clinicNum).BillingProviderId ?? 0;
        if (clinicInsBillingProv == 0) //default=0
            return PrefC.GetLong(PrefName.PracticeDefaultProv);

        if (clinicInsBillingProv == -1) //treat=-1
            return treatProv;

        return clinicInsBillingProv;
    }

    public static List<Provider> GetProvsForClinic(long clinicNum)
    {
        return GetProvsForClinicList([clinicNum]);
    }

    public static List<Provider> GetProvsForClinicList(List<long> listClinicNums)
    {
        if (listClinicNums.IsNullOrEmpty()) return [];

        ProviderClinicLinks.RefreshCache();
        //Creates a dictionary of all UserNums specifically associated with a clinic and the list of each one's associated clinicNums 
        //The GetWhere uses a "UserClinicNum>-1" in its selection to behave as a "Where true" to retrieve everything from the cache 
        var dictUserClinicsReference = UserClinics.GetWhere(x => x.UserClinicNum > -1)
            .GroupBy(x => x.UserNum)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ClinicNum) //kvp (UserNumsAssociatedWithClinics, listAssociatedClinicNums)
                .ToList());
        //Creates a dictionary of all UserNums and list of either each one's associated clinics (if in above dictionary) or an empty list
        var dictUserClinics = Userods.GetDeepCopy()
            .ToDictionary(x => x.UserNum, x => dictUserClinicsReference.ContainsKey(x.UserNum) ? dictUserClinicsReference[x.UserNum] : []); //kvp (AllUserNums, listAssociatedClinicNumsIfAny)
        //Creates a dictionary of all ProvNums with each's list of associated UserNums (likely just one UserNum)
        var dictProvUsers = Userods.GetWhere(x => x.ProvNum > 0) //Where the user is a provider
            .GroupBy(x => x.ProvNum)
            .ToDictionary(x => x.Key, x => x.Select(y => y.UserNum) //kvp (provNum, listAssociatedUserNums)
                .ToList());
        //List of providers restricted to clinics not in listClinicNums
        var listProvsRestrictedOtherClinics = ProviderClinicLinks.GetProvsRestrictedToOtherClinics(listClinicNums);
        //Get providers that are not associated with any users OR a user not restricted to any clinic OR a provider associated to a user that is restricted to current clinic
        var listProviders = GetWhere(x =>
            !dictProvUsers.ContainsKey(x.ProvNum) //provider not associated to any users.
            || dictProvUsers[x.ProvNum].Any(y => dictUserClinics[y].Count == 0) //provider associated with user not restricted to any clinics
            || dictProvUsers[x.ProvNum].Any(y => dictUserClinics[y].Any(z => listClinicNums.Contains(z))), true); //provider associated to user restricted to clinic in listClinicNums
        //returns list of ProvNums from the above providers that are also not restricted to a clinic in listClinicNums
        return listProviders.Where(x => !listProvsRestrictedOtherClinics.Contains(x.ProvNum)).OrderBy(x => x.ItemOrder).ToList();
    }

    public static string GetDuplicateAbbrs()
    {
        var command = "SELECT Abbr FROM provider WHERE ProvStatus!=" + SOut.Int((int) ProviderStatus.Deleted);
        var listDuplicates = Db.GetListString(command).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        return string.Join(",", listDuplicates);
    }

    public static Provider GetDefaultProvider(long clinicNum = 0)
    {
        var clinic = Clinics.GetClinic(clinicNum);
        Provider provider = null;
        if (clinic is {DefaultProviderId: not null})
            provider = GetProv(clinic.DefaultProviderId.Value);
        return provider ?? GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv));
    }

    public static DataTable GetDefaultPracticeProvider()
    {
        var command = @"SELECT FName,LName,Suffix,StateLicense
				FROM provider WHERE provnum=" + PrefC.GetString(PrefName.PracticeDefaultProv);
        return DataCore.GetTable(command);
    }

    public static DataTable GetDefaultPracticeProvider2()
    {
        var command = @"SELECT FName,LName,Specialty " +
                      "FROM provider WHERE provnum=" +
                      SOut.Long(PrefC.GetLong(PrefName.PracticeDefaultProv));
        return DataCore.GetTable(command);
    }

    public static DataTable GetDefaultPracticeProvider3()
    {
        var command = @"SELECT NationalProvID " +
                      "FROM provider WHERE provnum=" +
                      SOut.Long(PrefC.GetLong(PrefName.PracticeDefaultProv));
        return DataCore.GetTable(command);
    }

    public static DataTable GetPrimaryProviders(long PatNum)
    {
        var command = @"SELECT Fname,Lname from provider
                        WHERE provnum in (select priprov from 
                        patient where patnum = " + PatNum + ")";
        return DataCore.GetTable(command);
    }

    public static Provider GetLastSeenHygienistForPat(long patNum)
    {
        //Look at all completed appointments and get the most recent secondary provider on it.
        var command = @"SELECT appointment.ProvHyg
				FROM appointment
				WHERE appointment.PatNum=" + SOut.Long(patNum) + @"
				AND appointment.ProvHyg!=0
				AND appointment.AptStatus=" + SOut.Int((int) ApptStatus.Complete) + @"
				ORDER BY AptDateTime DESC";
        var listPatHygNums = Db.GetListLong(command);
        //Now that we have all hygienists for this patient.  Lets find the last non-hidden hygienist and return that one.
        var listProviders = GetDeepCopy(true);
        var listProvNums = listProviders.Select(x => x.ProvNum).Distinct().ToList();
        var lastHygNum = listPatHygNums.FirstOrDefault(x => listProvNums.Contains(x));
        return listProviders.FirstOrDefault(x => x.ProvNum == lastHygNum);
    }

    public static List<Provider> GetProvidersForWebSched(long patNum, long clinicNum)
    {
        var listProviders = GetDeepCopy(true);
        var providerRule = SIn.Enum<WebSchedProviderRules>(
            ClinicPrefs.GetPref(PrefName.WebSchedProviderRule, clinicNum)?.ValueString ?? PrefC.GetString(PrefName.WebSchedProviderRule));
        switch (providerRule)
        {
            case WebSchedProviderRules.PrimaryProvider:
                var patPri = Patients.GetPat(patNum);
                var patPriProv = listProviders.Find(x => x.ProvNum == patPri.PriProv);
                if (patPriProv == null) throw new Exception(Lans.g("Providers", "Invalid primary provider set for patient."));
                return [patPriProv];
            case WebSchedProviderRules.SecondaryProvider:
                var patSec = Patients.GetPat(patNum);
                var patSecProv = listProviders.Find(x => x.ProvNum == patSec.SecProv);
                if (patSecProv == null) throw new Exception(Lans.g("Providers", "No secondary provider set for patient."));
                return [patSecProv];
            case WebSchedProviderRules.LastSeenHygienist:
                var lastHygProvider = GetLastSeenHygienistForPat(patNum);
                if (lastHygProvider == null) throw new Exception(Lans.g("Providers", "No last seen hygienist found for patient."));
                return [lastHygProvider];
            case WebSchedProviderRules.FirstAvailable:
            default:
                return listProviders;
        }
    }

    public static List<Provider> GetProvidersForWebSchedNewPatAppt(long clinicNum = 0)
    {
        //Currently all providers are allowed to be considered for new patient appointments.
        //This follows the "WebSchedProviderRules.FirstAvailable" logic for recall Web Sched appointments which is what Nathan agreed upon.
        //This method is here so that we have a central location to go and get these types of providers in case we change this in the future.
        if (clinicNum == 0) return GetWhere(x => !x.IsNotPerson, true); //Make sure that we only return not is not persons.

        return GetProvsForClinic(clinicNum).Where(x => !x.IsNotPerson).ToList();
    }

    public static List<Provider> GetMultProviders(List<long> provNums)
    {
        var provNumsDistinct = provNums.Distinct().ToList();
        if (provNumsDistinct.Count < 1) return [];
        var command = "";
        command = "SELECT * FROM provider WHERE ProvNum IN (" + string.Join(",", provNumsDistinct) + ")";
        return ProviderCrud.SelectMany(command);
    }

    public static decimal GetProductionGoalForProviders(List<long> listProvNums, List<long> listOpNums, DateTime start, DateTime end)
    {
        var dictProvSchedHrs = Schedules.GetHoursSchedForProvsInRange(listProvNums, listOpNums, start, end);
        decimal amt = 0;
        foreach (var kvp in dictProvSchedHrs)
        {
            var prov = GetProv(kvp.Key);
            if (prov != null) amt += (decimal) (kvp.Value * prov.HourlyProdGoalAmt);
        }

        return amt;
    }

    public static void RemoveProvFromFutureSchedule(long provNum)
    {
        if (provNum < 1)
            return;
        var provNums = new List<long> {provNum};
        RemoveProvsFromFutureSchedule(provNums);
    }

    public static void RemoveProvsFromFutureSchedule(List<long> provNums)
    {
        var provs = "";
        for (var i = 0; i < provNums.Count; i++)
        {
            if (provNums[i] < 1)
            {
                continue;
            }

            if (i > 0) provs += ",";
            provs += provNums[i].ToString();
        }

        if (provs == "")
        {
            return;
        }

        var command = "SELECT ScheduleNum FROM schedule WHERE ProvNum IN (" + provs + ") AND SchedDate > " + "NOW()";
        var table = DataCore.GetTable(command);
        var listScheduleNums = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listScheduleNums.Add(table.Rows[i]["ScheduleNum"].ToString());
        if (listScheduleNums.Count != 0)
        {
            command = "DELETE FROM scheduleop WHERE ScheduleNum IN(" + SOut.String(string.Join(",", listScheduleNums)) + ")";
            Db.NonQ(command);
        }

        command = "DELETE FROM schedule WHERE ProvNum IN (" + provs + ") AND SchedDate > " + "NOW()";
        Db.NonQ(command);
    }

    public static bool IsAttachedToUser(long provNum)
    {
        var command = "SELECT COUNT(*) FROM userod,provider "
                      + "WHERE userod.ProvNum=provider.ProvNum "
                      + "AND provider.provNum=" + SOut.Long(provNum);
        var count = SIn.Int(Db.GetCount(command));
        return count > 0;
    }

    public static bool IsSpecialtyInUse(long defNum)
    {
        var command = "SELECT COUNT(*) FROM provider WHERE Specialty=" + SOut.Long(defNum);
        return Db.GetCount(command) != "0";
    }

    public static List<Provider> GetProvsScheduledToday(long clinicNum = -1)
    {
        var listSchedulesForDate = Schedules.GetAllForDateAndType(DateTime.Today, ScheduleType.Provider);
        if (clinicNum >= 0) listSchedulesForDate.FindAll(x => x.ClinicNum == clinicNum);
        var listProvNums = listSchedulesForDate.Select(x => x.ProvNum).ToList();
        return GetMultProviders(listProvNums);
    }

    public static long Merge(long provNumFrom, long provNumInto)
    {
        var provNumForeignKeys = new[]
        {
            //add any new FKs to this list.
            "adjustment.ProvNum",
            "appointment.ProvNum",
            "appointment.ProvHyg",
            "apptviewitem.ProvNum",
            "claim.ProvTreat",
            "claim.ProvBill",
            "claim.ReferringProv",
            "claim.ProvOrderOverride",
            "claimproc.ProvNum",
            "clinic.DefaultProv",
            "clinic.InsBillingProv",
            "dispsupply.ProvNum",
            "ehrnotperformed.ProvNum",
            "emailmessage.ProvNumWebMail",
            "encounter.ProvNum",
            "equipment.ProvNumCheckedOut",
            "erxlog.ProvNum",
            "evaluation.InstructNum",
            "evaluation.StudentNum",
            "fee.ProvNum",
            "intervention.ProvNum",
            "labcase.ProvNum",
            "medicalorder.ProvNum",
            "medicationpat.ProvNum",
            "operatory.ProvDentist",
            "operatory.ProvHygienist",
            "orthocase.ProvNum",
            //"orthocharlog.ProvNum",
            "patient.PriProv",
            "patient.SecProv",
            "payplancharge.ProvNum",
            "paysplit.ProvNum",
            "perioexam.ProvNum",
            "proccodenote.ProvNum",
            "procedurecode.ProvNumDefault",
            "procedurelog.ProvNum",
            "procedurelog.ProvOrderOverride",
            "provider.ProvNumBillingOverride",
            //"providerclinic.ProvNum",
            "providerident.ProvNum",
            "refattach.ProvNum",
            "reqstudent.ProvNum",
            "reqstudent.InstructorNum",
            "rxpat.ProvNum",
            "schedule.ProvNum",
            "userod.ProvNum",
            "vaccinepat.ProvNumAdminister",
            "vaccinepat.ProvNumOrdering"
        };
        var command = "";
        long retVal = 0;
        for (var i = 0; i < provNumForeignKeys.Length; i++)
        {
            //actually change all of the FKs in the above tables.
            var tableAndKeyName = provNumForeignKeys[i].Split('.');
            command = "UPDATE " + tableAndKeyName[0]
                                + " SET " + tableAndKeyName[1] + "=" + SOut.Long(provNumInto)
                                + " WHERE " + tableAndKeyName[1] + "=" + SOut.Long(provNumFrom);
            retVal += Db.NonQ(command);
        }

        //Merge any providerclinic rows associated to the FROM provider where the INTO provider does not have a row for said clinic.
        var listProviderClinicsAll = ProviderClinics.GetByProvNums([provNumFrom, provNumInto]);
        var listProviderClinicsFrom = listProviderClinicsAll.FindAll(x => x.ProvNum == provNumFrom);
        var listProviderClinicsInto = listProviderClinicsAll.FindAll(x => x.ProvNum == provNumInto);
        var listProviderClinicNums = listProviderClinicsFrom.Where(x => !listProviderClinicsInto.Select(y => y.ClinicNum).Contains(x.ClinicNum))
            .Select(x => x.ProviderClinicNum)
            .ToList();
        if (!listProviderClinicNums.IsNullOrEmpty())
        {
            command = $@"UPDATE providerclinic SET ProvNum = {SOut.Long(provNumInto)}
					WHERE ProviderClinicNum IN({string.Join(",", listProviderClinicNums.Select(x => SOut.Long(x)))})";
            Db.NonQ(command);
        }

        command = "UPDATE provider SET IsHidden=1 WHERE ProvNum=" + SOut.Long(provNumFrom);
        Db.NonQ(command);
        command = "UPDATE provider SET ProvStatus=" + SOut.Int((int) ProviderStatus.Deleted) + " WHERE ProvNum=" + SOut.Long(provNumFrom);
        Db.NonQ(command);
        return retVal;
    }

    public static long CountPats(long provNum)
    {
        var command = "SELECT COUNT(DISTINCT patient.PatNum) FROM patient WHERE (patient.PriProv=" + provNum + " OR patient.SecProv=" + provNum + ") AND patient.PatStatus=0";
        var retVal = DataCore.GetScalar(command);
        return SIn.Long(retVal);
    }

    public static long CountClaims(long provNum)
    {
        var command = "SELECT COUNT(DISTINCT claim.ClaimNum) FROM claim WHERE claim.ProvBill=" + provNum + " OR claim.ProvTreat=" + provNum;
        var retVal = DataCore.GetScalar(command);
        return SIn.Long(retVal);
    }

    public static Provider GetUnearnedProv()
    {
        return new Provider
        {
            ProvNum = 0,
            Abbr = Lans.g("Providers", "No Provider")
        };
    }

    public static List<Provider> GetListReports()
    {
        return GetWhere(x => !x.IsHiddenReport && x.ProvStatus != ProviderStatus.Deleted);
    }

    public static List<Provider> GetListProvidersForClinic(long clinicNum)
    {
        return GetProvsForClinic(clinicNum).Where(x => !x.IsHiddenReport && x.ProvStatus != ProviderStatus.Deleted).ToList();
    }

    private class ProviderCache : CacheListAbs<Provider>
    {
        protected override List<Provider> GetCacheFromDb()
        {
            var command = "SELECT * FROM provider";
            if (true) command += " ORDER BY ItemOrder";
            return ProviderCrud.SelectMany(command);
        }

        protected override List<Provider> TableToList(DataTable dataTable)
        {
            return ProviderCrud.TableToList(dataTable);
        }

        protected override Provider Copy(Provider item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Provider> items)
        {
            return ProviderCrud.ListToTable(items, "Provider");
        }

        protected override void FillCacheIfNeeded()
        {
            Providers.GetTableFromCache(false);
        }

        protected override bool IsInListShort(Provider item)
        {
            return !item.IsHidden && item.ProvStatus != ProviderStatus.Deleted;
        }
    }

    private static readonly ProviderCache Cache = new();

    public static List<Provider> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static bool GetExists(Predicate<Provider> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    public static Provider GetFirst(bool isShort = false)
    {
        return Cache.GetFirst(isShort);
    }

    public static Provider GetFirst(Func<Provider, bool> match, bool isShort = false)
    {
        return Cache.GetFirst(match, isShort);
    }

    public static Provider GetFirstOrDefault(Func<Provider, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static Provider GetLastOrDefault(Func<Provider, bool> match, bool isShort = false)
    {
        return Cache.GetLastOrDefault(match, isShort);
    }

    public static List<Provider> GetWhere(Predicate<Provider> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
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