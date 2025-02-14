using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AppointmentTypes
{
    public static void Sync(List<AppointmentType> listAppointmentTypesNew, List<AppointmentType> listAppointmentTypesOld)
    {
        AppointmentTypeCrud.Sync(listAppointmentTypesNew, listAppointmentTypesOld);
    }

    public static AppointmentType GetOne(long appointmentTypeNum)
    {
        return GetFirstOrDefault(x => x.AppointmentTypeNum == appointmentTypeNum);
    }

    public static string CheckInUse(long appointmentTypeNum)
    {
        if (appointmentTypeNum == 0)
        {
            return "";
        }

        var command = "SELECT COUNT(*) FROM appointment WHERE AppointmentTypeNum = " + appointmentTypeNum;
        if (SIn.Int(Db.GetCount(command)) > 0)
        {
            return "Not allowed to delete appointment types that are in use on an appointment.";
        }

        command =
            "SELECT COUNT(*) FROM deflink " +
            "WHERE LinkType = " + (int) DefLinkType.AppointmentType + " " +
            "AND FKey = " + appointmentTypeNum;

        return SIn.Int(Db.GetCount(command)) > 0 ? "Not allowed to delete appointment types that are in use by Web Sched New Pat Appt Types definitions." : "";
    }

    public static string CheckRequiredProcsAttached(long appointmentTypeNum, List<Procedure> procedures)
    {
        var message = "";
        var appointmentType = GetOne(appointmentTypeNum);

        if (appointmentType == null || appointmentType.RequiredProcCodesNeeded == EnumRequiredProcCodesNeeded.None)
        {
            return message;
        }

        var procCodesRequiredForAppointmentType = appointmentType.CodeStrRequired.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

        var selectedCodeNums = procedures.Select(x => x.CodeNum).ToList();
        var selectedProcCodes = new List<string>();

        foreach (var codeNum in selectedCodeNums)
        {
            var procedureCode = ProcedureCodes.GetFirstOrDefault(x => x.CodeNum == codeNum);

            selectedProcCodes.Add(procedureCode.ProcCode);
        }

        var requiredCodesSelected = 0;
        var requiredProcCodesMissing = new List<string>();

        foreach (var requiredProcCode in procCodesRequiredForAppointmentType)
        {
            if (selectedProcCodes.Contains(requiredProcCode))
            {
                requiredCodesSelected++;

                selectedProcCodes.Remove(requiredProcCode);
                continue;
            }

            requiredProcCodesMissing.Add(requiredProcCode);
        }

        switch (appointmentType.RequiredProcCodesNeeded)
        {
            case EnumRequiredProcCodesNeeded.AtLeastOne when requiredCodesSelected == 0:
                message =
                    "Appointment Type \"" + appointmentType.AppointmentTypeName + "\" must contain at least one of the following procedures:\r\n" +
                    string.Join(", ", procCodesRequiredForAppointmentType);
                return message;

            case EnumRequiredProcCodesNeeded.All when requiredCodesSelected != procCodesRequiredForAppointmentType.Count:
                message =
                    "Appointment Type \"" + appointmentType.AppointmentTypeName + "\" requires the following procedures:\r\n" +
                    string.Join(", ", procCodesRequiredForAppointmentType) +
                    "\r\n\r\nThe following procedures are missing from this appointment:" +
                    "\r\n" +
                    string.Join(", ", requiredProcCodesMissing);
                return message;
        }

        return message;
    }

    public static int SortItemOrder(AppointmentType appointmentType1, AppointmentType appointmentType2)
    {
        return appointmentType1.ItemOrder != appointmentType2.ItemOrder
            ? appointmentType1.ItemOrder.CompareTo(appointmentType2.ItemOrder)
            : appointmentType1.AppointmentTypeNum.CompareTo(appointmentType2.AppointmentTypeNum);
    }

    public static string GetName(long appointmentTypeNum)
    {
        var appointmentType = GetFirstOrDefault(x => x.AppointmentTypeNum == appointmentTypeNum);
        if (appointmentType == null)
        {
            return string.Empty;
        }

        var typeName = appointmentType.AppointmentTypeName;
        if (appointmentType.IsHidden)
        {
            typeName += "(hidden)";
        }

        return typeName;
    }

    public static string GetTimePatternForAppointmentType(AppointmentType appointmentType, long provNumDentist = 0, long provNumHyg = 0)
    {
        string timePattern;

        if (string.IsNullOrEmpty(appointmentType.Pattern))
        {
            var procCodeStrings = appointmentType.CodeStr.Split([','], StringSplitOptions.RemoveEmptyEntries).ToList();
            var codeNums = new List<long>();

            foreach (var procCode in procCodeStrings)
            {
                codeNums.Add(ProcedureCodes.GetProcCode(procCode).CodeNum);
            }

            timePattern = Appointments.CalculatePattern(provNumDentist, provNumHyg, codeNums, true);
        }
        else
        {
            timePattern = appointmentType.Pattern;
        }

        return timePattern;
    }

    public static AppointmentType GetApptTypeForDef(long defNum)
    {
        var defLinks = DefLinks.GetDefLinksByType(DefLinkType.AppointmentType);
        var defLink = defLinks.FirstOrDefault(x => x.DefNum == defNum);

        return defLink == null ? null : GetFirstOrDefault(x => x.AppointmentTypeNum == defLink.FKey, true);
    }

    private class AppointmentTypeCache : CacheListAbs<AppointmentType>
    {
        protected override List<AppointmentType> GetCacheFromDb()
        {
            return AppointmentTypeCrud.SelectMany("SELECT * FROM appointmenttype ORDER BY ItemOrder");
        }

        protected override List<AppointmentType> TableToList(DataTable dataTable)
        {
            return AppointmentTypeCrud.TableToList(dataTable);
        }

        protected override AppointmentType Copy(AppointmentType item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<AppointmentType> items)
        {
            return AppointmentTypeCrud.ListToTable(items, "AppointmentType");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override bool IsInListShort(AppointmentType item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly AppointmentTypeCache Cache = new();

    public static List<AppointmentType> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static AppointmentType GetFirstOrDefault(Func<AppointmentType, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<AppointmentType> GetWhere(Predicate<AppointmentType> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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