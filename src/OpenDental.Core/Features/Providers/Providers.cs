using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDentBusiness;

namespace Imedisoft.Core.Features.Providers;

public class Providers
{
    public static List<long> GetInvalidProvsByTermDate(List<long> providerIds, DateTime dateCompare)
    {
        return GetWhere(x => providerIds.Any(y => y == x.Id) && x.TerminatedOn is not null && x.TerminatedOn.Value.Date < dateCompare.Date).Select(x => x.Id).ToList();
    }

    public static string CheckApptProvidersTermDates(Appointment apt, bool isSetComplete = false)
    {
        var message = "";

        var providerIds = new List<long> {apt.ProvNum, apt.ProvHyg};

        var invalidProviderIds = GetInvalidProvsByTermDate(providerIds, apt.AptDateTime);
        if (invalidProviderIds.Count == 0)
        {
            return string.Empty;
        }

        if (invalidProviderIds.Contains(apt.ProvNum))
        {
            message += "provider";
        }

        if (invalidProviderIds.Contains(apt.ProvHyg))
        {
            if (message != "")
            {
                message += " and ";
            }

            message += "hygienist";
        }

        if (invalidProviderIds.Contains(apt.ProvNum) && invalidProviderIds.Contains(apt.ProvHyg))
        {
            return "The " + message + " selected for this appointment have Term Dates prior to the selected day and time. " +
                   "Please select another " + message + (isSetComplete ? " to set the appointment complete." : ".");
        }

        return "The " + message + " selected for this appointment has a Term Date prior to the selected day and time. " +
               "Please select another " + message + (isSetComplete ? " to set the appointment complete." : ".");
    }

    public static List<ProviderDto> GetAll()
    {
        return Cache.GetDeepCopy();
    }

    public static ProviderDto GetById(long providerId)
    {
        return Cache.GetFirstOrDefault(x => x.Id == providerId);
    }

    public static ProviderDto GetByIdNoCache(long providerId)
    {
        throw new NotImplementedException();
    }

    public static List<ProviderDto> GetManyById(List<long> providerIds, bool shortList = false)
    {
        return Cache.GetWhere(x => providerIds.Contains(x.Id), shortList);
    }

    public static List<ProviderDto> GetManyByIdNoCache(List<long> providerIds)
    {
        throw new NotImplementedException();
    }

    public static string GetAbbr(long providerId, bool includeHidden = false)
    {
        var providerDto = GetById(providerId);
        if (providerDto is null)
        {
            return string.Empty;
        }

        var abbr = providerDto.Abbr;
        if (includeHidden && providerDto.IsHidden)
        {
            abbr += " (hidden)";
        }

        return abbr;
    }

    public static string GetLastName(long provNum, List<ProviderDto> providers = null)
    {
        var provider = providers is null ? GetById(provNum) : providers.FirstOrDefault(x => x.Id == provNum);

        return provider is null ? string.Empty : provider.LastName;
    }

    public static string GetFormalName(long provNum)
    {
        var provider = GetById(provNum);
        if (provider is null)
        {
            return string.Empty;
        }

        var formalName = (provider.FirstName + " " + provider.LastName).Trim();
        if (!string.IsNullOrWhiteSpace(provider.Suffix))
        {
            formalName = formalName + ", " + provider.Suffix;
        }

        return formalName;
    }

    public static string GetLongDesc(long provNum)
    {
        var provider = GetFirstOrDefault(x => x.Id == provNum);
        return provider == null ? "" : provider.Description;
    }

    public static Color GetColor(long providerId)
    {
        var provider = GetById(providerId);
        if (provider is null)
        {
            return Color.White;
        }

        var color = ColorTranslator.FromHtml(provider.Color);

        return color == Color.Transparent ? Color.White : color;
    }

    public static Color GetOutlineColor(long providerId)
    {
        var provider = GetById(providerId);
        if (provider is null)
        {
            return Color.White;
        }

        var color = ColorTranslator.FromHtml(provider.OutlineColor);

        return color == Color.Transparent ? Color.White : color;
    }

    public static bool IsSecondary(long providerId)
    {
        return GetById(providerId) is {IsSecondary: true};
    }

    public static int GetIndex(long providerId)
    {
        return Cache.GetDeepCopy(true).FindIndex(x => x.Id == providerId);
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

        var clinicInsBillingProv = Clinics.Clinics.GetClinic(clinicNum).BillingProviderId ?? 0;
        if (clinicInsBillingProv == 0) //default=0
            return PrefC.GetLong(PrefName.PracticeDefaultProv);

        if (clinicInsBillingProv == -1) //treat=-1
            return treatProv;

        return clinicInsBillingProv;
    }

    public static List<ProviderDto> GetProvsForClinic(long clinicNum)
    {
        return GetProvsForClinicList([clinicNum]);
    }

    public static List<ProviderDto> GetProvsForClinicList(List<long> listClinicNums)
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
            !dictProvUsers.ContainsKey(x.Id) //provider not associated to any users.
            || dictProvUsers[x.Id].Any(y => dictUserClinics[y].Count == 0) //provider associated with user not restricted to any clinics
            || dictProvUsers[x.Id].Any(y => dictUserClinics[y].Any(z => listClinicNums.Contains(z))), true); //provider associated to user restricted to clinic in listClinicNums
        //returns list of ProvNums from the above providers that are also not restricted to a clinic in listClinicNums
        return listProviders.Where(x => !listProvsRestrictedOtherClinics.Contains(x.Id)).ToList();
    }

    public static ProviderDto GetDefaultProvider(long clinicId = 0)
    {
        var clinic = Clinics.Clinics.GetClinic(clinicId);

        ProviderDto provider = null;

        if (clinic is {DefaultProviderId: not null})
        {
            provider = GetById(clinic.DefaultProviderId.Value);
        }

        return provider ?? GetById(PrefC.GetLong(PrefName.PracticeDefaultProv));
    }

    public static DataTable GetDefaultPracticeProvider()
    {
        var command = @"SELECT FName,LName,Suffix,StateLicense
				FROM provider WHERE provnum=" + PrefC.GetString(PrefName.PracticeDefaultProv);
        return DataCore.GetTable(command);
    }

    public static decimal GetProductionGoalForProviders(List<long> listProvNums, List<long> listOpNums, DateTime start, DateTime end)
    {
        var dictProvSchedHrs = Schedules.GetHoursSchedForProvsInRange(listProvNums, listOpNums, start, end);

        decimal total = 0;

        foreach (var kvp in dictProvSchedHrs)
        {
            var provider = GetById(kvp.Key);
            if (provider is not null)
            {
                total += (decimal) kvp.Value * provider.HourlyProductionGoal;
            }
        }

        return total;
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
                      + "AND provider.provNum=" + provNum;
        var count = SIn.Int(Db.GetCount(command));
        return count > 0;
    }

    public static bool IsSpecialtyInUse(long defNum)
    {
        var command = "SELECT COUNT(*) FROM provider WHERE Specialty=" + defNum;
        return Db.GetCount(command) != "0";
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
                                + " SET " + tableAndKeyName[1] + "=" + provNumInto
                                + " WHERE " + tableAndKeyName[1] + "=" + provNumFrom;
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
            command = $@"UPDATE providerclinic SET ProvNum = {provNumInto}
					WHERE ProviderClinicNum IN({string.Join(",", listProviderClinicNums.Select(x => x))})";
            Db.NonQ(command);
        }

        command = "UPDATE provider SET IsHidden=1 WHERE ProvNum=" + provNumFrom;
        Db.NonQ(command);
        command = "UPDATE provider SET ProvStatus=" + /*SOut.Int((int) ProviderStatus.Deleted)*/ 1 + " WHERE ProvNum=" + provNumFrom;
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

    public static ProviderDto GetUnearnedProv()
    {
        return new ProviderDto
        {
            Id = 0,
            Abbr = "No Provider"
        };
    }

    public static List<ProviderDto> GetListReports()
    {
        return Cache.GetWhere(x => !x.IsHiddenFromReports && !x.IsDeleted);
    }

    public static List<ProviderDto> GetListProvidersForClinic(long clinicNum)
    {
        return GetProvsForClinic(clinicNum).Where(x => !x.IsHiddenFromReports && !x.IsDeleted).ToList();
    }

    private class ProviderCache : ListCache<ProviderDto>
    {
        protected override List<ProviderDto> GetCacheFromDb()
        {
            return ProviderService.GetAll();
        }

        protected override bool InShortList(ProviderDto item)
        {
            return !item.IsHidden && !item.IsDeleted;
        }
    }

    private static readonly ProviderCache Cache = new();

    public static List<ProviderDto> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static bool GetExists(Func<ProviderDto, bool> predicate, bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList).Any(predicate);
    }

    public static ProviderDto GetFirst(bool shortList = false)
    {
        return Cache.GetFirst(shortList);
    }

    public static ProviderDto GetFirstOrDefault(Func<ProviderDto, bool> predicate, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(predicate, isShort);
    }

    public static List<ProviderDto> GetWhere(Predicate<ProviderDto> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.Refresh();
    }

    public static void GetTableFromCache()
    {
        Cache.Refresh();
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}