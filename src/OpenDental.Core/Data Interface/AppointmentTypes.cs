using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AppointmentTypes
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
        if (appointmentTypeNum == 0) //New appointment type, so should not be associated with any appointments.
            return "";
        var command = "SELECT COUNT(*) FROM appointment WHERE AppointmentTypeNum = " + SOut.Long(appointmentTypeNum);
        if (SIn.Int(Db.GetCount(command)) > 0) return "Not allowed to delete appointment types that are in use on an appointment.";
        command = "SELECT COUNT(*) FROM deflink "
                  + "WHERE LinkType = " + SOut.Int((int) DefLinkType.AppointmentType) + " "
                  + "AND FKey = " + SOut.Long(appointmentTypeNum) + " ";
        if (SIn.Int(Db.GetCount(command)) > 0)
            //This message will need to change in the future if more definition categories utilize appointment types with the deflink table.
            return "Not allowed to delete appointment types that are in use by Web Sched New Pat Appt Types definitions.";
        return "";
    }
    
    public static string CheckRequiredProcsAttached(long appointmentTypeNum, List<Procedure> listProcedures)
    {
        var message = "";
        var appointmentType = GetOne(appointmentTypeNum);
        if (appointmentType != null && appointmentType.RequiredProcCodesNeeded != EnumRequiredProcCodesNeeded.None)
        {
            //Should never be null.
            var listProcCodesRequiredForAppointmentType = appointmentType.CodeStrRequired.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(); //Includes duplicates.
            //Get the ProcCodes of the selected Procedures.
            var listProceduresSelected = listProcedures;
            var listCodeNumsSelected = listProceduresSelected.Select(x => x.CodeNum).ToList();
            var listProcCodesSelected = new List<string>();
            for (var i = 0; i < listCodeNumsSelected.Count; i++)
            {
                var procedureCode = ProcedureCodes.GetFirstOrDefault(x => x.CodeNum == listCodeNumsSelected[i]); //Should never return null.
                listProcCodesSelected.Add(procedureCode.ProcCode);
            }

            //Figure out how many of our required procedures are present in the selected codes, and which ones are not.
            var requiredCodesSelected = 0;
            var listRequiredProcCodesMissing = new List<string>();
            for (var i = 0; i < listProcCodesRequiredForAppointmentType.Count; i++)
            {
                var requiredProcCode = listProcCodesRequiredForAppointmentType[i];
                if (listProcCodesSelected.Contains(requiredProcCode))
                {
                    requiredCodesSelected++;
                    listProcCodesSelected.Remove(requiredProcCode);
                    continue;
                }

                listRequiredProcCodesMissing.Add(requiredProcCode);
            }

            //If RequiredProcCodesNeeded is at least one, check for at least one CodeStrRequired code selected.
            if (appointmentType.RequiredProcCodesNeeded == EnumRequiredProcCodesNeeded.AtLeastOne)
                if (requiredCodesSelected == 0)
                {
                    message = "Appointment Type" + " \"" + appointmentType.AppointmentTypeName + "\" " + "must contain at least one of the following procedures:"
                              + "\r\n" + string.Join(", ", listProcCodesRequiredForAppointmentType);
                    return message;
                }

            //If its all, make sure all CodeStrRequired codes are selected
            if (appointmentType.RequiredProcCodesNeeded == EnumRequiredProcCodesNeeded.All)
                if (requiredCodesSelected != listProcCodesRequiredForAppointmentType.Count)
                {
                    message = "Appointment Type" + " \"" + appointmentType.AppointmentTypeName + "\" " + "requires the following procedures:"
                              + "\r\n" + string.Join(", ", listProcCodesRequiredForAppointmentType)
                              + "\r\n\r\n" + "The following procedures are missing from this appointment:"
                              + "\r\n" + string.Join(", ", listRequiredProcCodesMissing);
                    return message;
                }
        }

        return message;
    }

    public static int SortItemOrder(AppointmentType appointmentType1, AppointmentType appointmentType2)
    {
        if (appointmentType1.ItemOrder != appointmentType2.ItemOrder) return appointmentType1.ItemOrder.CompareTo(appointmentType2.ItemOrder);
        return appointmentType1.AppointmentTypeNum.CompareTo(appointmentType2.AppointmentTypeNum);
    }
    
    public static string GetName(long appointmentTypeNum)
    {
        var retVal = "";
        var appointmentType = GetFirstOrDefault(x => x.AppointmentTypeNum == appointmentTypeNum);
        if (appointmentType != null)
        {
            retVal = appointmentType.AppointmentTypeName;
            if (appointmentType.IsHidden) retVal += Lans.g("AppointmentTypes", "(hidden)");
        }

        return retVal;
    }
    
    public static string GetTimePatternForAppointmentType(AppointmentType appointmentType, long provNumDentist = 0, long provNumHyg = 0)
    {
        string timePattern;
        if (string.IsNullOrEmpty(appointmentType.Pattern))
        {
            //Dynamically calculate the timePattern from the procedure codes associated to the appointment type passed in.
            var listProcCodeStrings = appointmentType.CodeStr.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var listCodeNums = new List<long>();
            for (var i = 0; i < listProcCodeStrings.Count(); i++) listCodeNums.Add(ProcedureCodes.GetProcCode(listProcCodeStrings[i]).CodeNum);
            timePattern = Appointments.CalculatePattern(provNumDentist, provNumHyg, listCodeNums, true);
        }
        else
        {
            timePattern = appointmentType.Pattern; //Already in 5 minute increment so no conversion required.
        }

        return timePattern;
    }

    public static AppointmentType GetApptTypeForDef(long defNum)
    {
        var listDefLinks = DefLinks.GetDefLinksByType(DefLinkType.AppointmentType);
        var defLink = listDefLinks.FirstOrDefault(x => x.DefNum == defNum);
        if (defLink == null) return null;
        return GetFirstOrDefault(x => x.AppointmentTypeNum == defLink.FKey, true);
    }
    
    private class AppointmentTypeCache : CacheListAbs<AppointmentType>
    {
        protected override List<AppointmentType> GetCacheFromDb()
        {
            var command = "SELECT * FROM appointmenttype ORDER BY ItemOrder";
            return AppointmentTypeCrud.SelectMany(command);
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
            AppointmentTypes.GetTableFromCache(false);
        }

        protected override bool IsInListShort(AppointmentType item)
        {
            return !item.IsHidden;
        }
    }
    
    private static readonly AppointmentTypeCache Cache = new();

    public static List<AppointmentType> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static AppointmentType GetFirstOrDefault(Func<AppointmentType, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static List<AppointmentType> GetWhere(Predicate<AppointmentType> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
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