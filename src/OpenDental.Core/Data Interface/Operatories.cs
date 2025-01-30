using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Operatories
{
    public static void Sync(List<Operatory> listOperatoriesNew, List<Operatory> listOperatoriesOld)
    {
        OperatoryCrud.Sync(listOperatoriesNew, listOperatoriesOld);
        //Regardless if changes were made during the sync, we need to make sure to sync the DefLinks for WSNPA appointment types.
        //This needs to happen after the sync call so that the PKs have been correctly set for listNew.
        var listDefLinksWSNPA = DefLinks.GetOperatoryDefLinksForCategory(DefCat.WebSchedNewPatApptTypes);
        var listDefLinksWSEP = DefLinks.GetOperatoryDefLinksForCategory(DefCat.WebSchedExistingApptTypes);
        for (var i = 0; i < listOperatoriesNew.Count; i++)
        {
            DefLinks.SyncWebSchedOpLinks(listOperatoriesNew[i], DefCat.WebSchedNewPatApptTypes, listDefLinksWSNPA);
            DefLinks.SyncWebSchedOpLinks(listOperatoriesNew[i], DefCat.WebSchedExistingApptTypes, listDefLinksWSEP);
        }

        //Delete any deflinks for operatories that are present within listOld but are not present within listNew.
        var listOpNumsDelete = listOperatoriesOld.Where(x => !listOperatoriesNew.Any(y => y.OperatoryNum == x.OperatoryNum))
            .Select(x => x.OperatoryNum)
            .Distinct()
            .ToList();
        DefLinks.DeleteAllForFKeys(listOpNumsDelete, DefLinkType.Operatory);
    }

    public static bool HasFutureApts(long operatoryNum, params ApptStatus[] apptStatusArrayIgnore)
    {
        var command = "SELECT COUNT(*) FROM appointment "
                      + "WHERE Op = " + SOut.Long(operatoryNum) + " ";
        if (apptStatusArrayIgnore.Length > 0)
        {
            command += "AND AptStatus NOT IN (";
            for (var i = 0; i < apptStatusArrayIgnore.Length; i++)
            {
                if (i > 0) command += ",";
                command += SOut.Int((int) apptStatusArrayIgnore[i]);
            }

            command += ") ";
        }

        command += "AND AptDateTime > " + "NOW()";
        return SIn.Int(DataCore.GetScalar(command)) > 0;
    }

    public static List<Appointment> MergeApptCheck(long opNumMaster, List<long> listOpNumsChild)
    {
        if (listOpNumsChild == null || listOpNumsChild.Count == 0) return new List<Appointment>();
        if (listOpNumsChild.Contains(opNumMaster)) throw new ApplicationException(Lans.g("Operatories", "The operatory to keep cannot be within the selected list of operatories to combine."));
        var command = "SELECT * FROM appointment "
                      + "WHERE Op IN (" + string.Join(",", listOpNumsChild.Concat(new[] {opNumMaster})) + ") "
                      + "AND AptStatus IN ("
                      + string.Join(",", new[] {(int) ApptStatus.Scheduled, (int) ApptStatus.Complete, (int) ApptStatus.Broken, (int) ApptStatus.PtNote}) + ")";
        var listAppointmentsAll = AppointmentCrud.SelectMany(command);
        return listAppointmentsAll;
    }

    public static bool HasConflict(Appointment appointment, List<Appointment> listAppointmentsAll)
    {
        if (listAppointmentsAll is null) return false;
        for (var i = 0; i < listAppointmentsAll.Count; i++)
        {
            if (listAppointmentsAll[i].AptNum == appointment.AptNum) continue;
            if (listAppointmentsAll[i].AptDateTime <= appointment.AptDateTime)
                if (listAppointmentsAll[i].AptDateTime.AddMinutes(listAppointmentsAll[i].Pattern.Length * 5) > appointment.AptDateTime)
                    return true;

            if (appointment.AptDateTime <= listAppointmentsAll[i].AptDateTime)
                if (appointment.AptDateTime.AddMinutes(appointment.Pattern.Length * 5) > listAppointmentsAll[i].AptDateTime)
                    return true;
        }

        return false;
    }

    public static void MergeOperatoriesIntoMaster(long opNumMaster, List<long> listOpNumsToMerge, List<Appointment> listAppointmentsToMerge)
    {
        var listOperatories = GetDeepCopy();
        var operatoryMaster = listOperatories.Find(x => x.OperatoryNum == opNumMaster);
        if (operatoryMaster == null) throw new ApplicationException(Lans.g("Operatories", "Operatory to merge into no longer exists."));
        if (listAppointmentsToMerge.Count > 0)
        {
            //All appts in listAppts are appts that we are going to move to new op.
            var listAppointmentsNew = listAppointmentsToMerge.Select(x => x.Copy()).ToList(); //Copy object so that we do not change original object in memory.
            for (var i = 0; i < listAppointmentsNew.Count; i++) //Associate to new op selection
                listAppointmentsNew[i].Op = opNumMaster;
            Appointments.Sync(listAppointmentsNew, listAppointmentsToMerge);
        }

        var listOperatoriesToMerge = listOperatories.Select(x => x.Copy()).ToList(); //Copy object so that we do not change original object in memory.
        var listOperatoriesFiltered = new List<Operatory>();
        for (var i = 0; i < listOperatoriesToMerge.Count; i++)
        {
            if (listOperatoriesToMerge[i].OperatoryNum == opNumMaster) continue;
            if (!listOpNumsToMerge.Contains(listOperatoriesToMerge[i].OperatoryNum)) continue;
            listOperatoriesToMerge[i].IsHidden = true;
            listOperatoriesFiltered.Add(listOperatoriesToMerge[i]);
        }

        Sync(listOperatoriesToMerge, listOperatories);
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0
            , Lans.g("Operatories", "The following operatories and all of their appointments were merged into the")
              + " " + operatoryMaster.Abbrev + " " + Lans.g("Operatories", "operatory;") + " "
              + string.Join(", ", listOperatoriesFiltered.Select(x => x.Abbrev)));
    }

    public static string GetAbbrev(long operatoryNum)
    {
        var operatory = GetFirstOrDefault(x => x.OperatoryNum == operatoryNum);
        if (operatory == null) return "";
        return operatory.Abbrev;
    }

    public static string GetOpName(long operatoryNum)
    {
        var operatory = GetFirstOrDefault(x => x.OperatoryNum == operatoryNum);
        if (operatory == null) return "";
        return operatory.OpName;
    }

    public static int GetOrder(long opNum)
    {
        return Cache.GetFindIndex(x => x.OperatoryNum == opNum, true);
    }

    public static Operatory GetOperatory(long operatoryNum)
    {
        return GetFirstOrDefault(x => x.OperatoryNum == operatoryNum);
    }

    public static List<Operatory> GetOperatories(List<long> listOpNums, bool isShort = false)
    {
        return GetWhere(x => listOpNums.Contains(x.OperatoryNum), isShort).ToList();
    }

    public static List<Operatory> GetOpsForClinic(long clinicNum)
    {
        return GetWhere(x => x.ClinicNum == clinicNum, true);
    }

    public static List<long> GetOpNumsForClinics(List<long> listClinicNums)
    {
        if (listClinicNums.IsNullOrEmpty()) return new List<long>();
        return GetWhere(x => listClinicNums.Contains(x.ClinicNum)).Select(x => x.OperatoryNum).ToList();
    }

    public static List<Operatory> GetOpsForWebSched()
    {
        //Only return the ops flagged as IsWebSched.
        return GetWhere(x => x.IsWebSched, true);
    }

    public static List<Operatory> GetOpsForWebSchedNewOrExistingPatAppts(bool isNewPat = true, bool isShort = true)
    {
        var defCat = DefCat.WebSchedExistingApptTypes;
        if (isNewPat) defCat = DefCat.WebSchedNewPatApptTypes;
        //Get all of the deflinks that are of type Operatory in order to get the operatory specific FKeys.
        var listOperatoryNums = DefLinks.GetOperatoryDefLinksForCategory(defCat)
            .Select(x => x.FKey)
            .Distinct()
            .ToList();
        return GetWhere(x => listOperatoryNums.Contains(x.OperatoryNum), isShort);
    }

    public static List<Operatory> GetOpsForDefAndCategory(long defNum, DefCat defCat, bool isShort = true)
    {
        var listOpNums = DefLinks.GetOperatoryDefLinksForCategory(defCat, isShort)
            .Where(x => x.DefNum == defNum)
            .Select(x => x.FKey)
            .Distinct()
            .ToList();
        return GetWhere(x => listOpNums.Contains(x.OperatoryNum), isShort);
    }

    private class OperatoryCache : CacheListAbs<Operatory>
    {
        protected override List<Operatory> GetCacheFromDb()
        {
            var command = @"
				SELECT operatory.*, CASE WHEN apptviewop.OpNum IS NULL THEN 0 ELSE 1 END IsInHQView
				FROM operatory
				LEFT JOIN (
					SELECT apptviewitem.OpNum
					FROM apptviewitem
					INNER JOIN apptview ON apptview.ApptViewNum = apptviewitem.ApptViewNum
						AND apptview.ClinicNum = 0
					GROUP BY apptviewitem.OpNum
				)apptviewop ON operatory.OperatoryNum = apptviewop.OpNum
				ORDER BY ItemOrder";
            return TableToList(DataCore.GetTable(command));
        }

        protected override List<Operatory> TableToList(DataTable dataTable)
        {
            var listOperatories = OperatoryCrud.TableToList(dataTable);
            //The IsInHQView flag is not important enough to cause filling the cache to fail.
            ODException.SwallowAnyException(() =>
            {
                for (var i = 0; i < dataTable.Rows.Count; i++) listOperatories[i].IsInHQView = SIn.Bool(dataTable.Rows[i]["IsInHQView"].ToString());
            });
            var listWSNPADefNums = DefLinks.GetOperatoryDefLinksForCategory(DefCat.WebSchedNewPatApptTypes).Select(x => x.DefNum).ToList();
            var listWSEPDefNums = DefLinks.GetOperatoryDefLinksForCategory(DefCat.WebSchedExistingApptTypes).Select(x => x.DefNum).ToList();
            var listDefLinksOp = DefLinks.GetDefLinksByType(DefLinkType.Operatory);
            //Web Sched operatory defs are important enough that we want this portion to fail if it has problems.
            //Create a dictionary comprised of Key: OperatoryNum and value: List of definition DefNums.
            //WSNPA
            var dictionaryWSNPAOperatoryDefNums = listDefLinksOp
                .Where(x => listWSNPADefNums.Contains(x.DefNum))
                .GroupBy(x => x.FKey) //FKey for DefLinkType.Operatory is OperatoryNum
                .ToDictionary(x => x.Key, x => x.Select(y => y.DefNum).ToList());
            foreach (var operatoryNum in dictionaryWSNPAOperatoryDefNums.Keys)
            {
                var operatory = listOperatories.FirstOrDefault(x => x.OperatoryNum == operatoryNum);
                if (operatory != null) operatory.ListWSNPAOperatoryDefNums = dictionaryWSNPAOperatoryDefNums[operatoryNum];
            }

            //WSEP
            var dictionaryWSEPOperatoryDefNums = listDefLinksOp
                .Where(x => listWSEPDefNums.Contains(x.DefNum))
                .GroupBy(x => x.FKey)
                .ToDictionary(x => x.Key, x => x.Select(y => y.DefNum).ToList());
            foreach (var operatoryNum in dictionaryWSEPOperatoryDefNums.Keys)
            {
                var operatory = listOperatories.FirstOrDefault(x => x.OperatoryNum == operatoryNum);
                if (operatory != null) operatory.ListWSEPOperatoryDefNums = dictionaryWSEPOperatoryDefNums[operatoryNum];
            }

            return listOperatories;
        }

        protected override Operatory Copy(Operatory item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Operatory> items)
        {
            var table = OperatoryCrud.ListToTable(items, "Operatory");
            //The IsInHQView flag is not important enough to cause filling the cache to fail.
            try
            {
                table.Columns.Add("IsInHQView");
                for (var i = 0; i < table.Rows.Count; i++) table.Rows[i]["IsInHQView"] = SOut.Bool(items[i].IsInHQView);
            }
            catch (Exception e)
            {
            }

            return table;
        }

        protected override void FillCacheIfNeeded()
        {
            Operatories.GetTableFromCache(false);
        }

        protected override bool IsInListShort(Operatory item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly OperatoryCache Cache = new();

    public static List<Operatory> GetWhere(Predicate<Operatory> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
    }

    public static List<Operatory> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Operatory GetFirstOrDefault(Func<Operatory, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
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