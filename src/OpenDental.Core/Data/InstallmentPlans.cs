using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class InstallmentPlans
{
    public static InstallmentPlan GetOne(long installmentPlanNum)
    {
        return InstallmentPlanCrud.SelectOne(installmentPlanNum);
    }

    public static void Insert(InstallmentPlan installmentPlan)
    {
        InstallmentPlanCrud.Insert(installmentPlan);
    }

    public static void Update(InstallmentPlan installmentPlan)
    {
        InstallmentPlanCrud.Update(installmentPlan);
    }

    public static void Delete(long installmentPlanNum)
    {
        Db.NonQ("DELETE FROM installmentplan WHERE InstallmentPlanNum = " + installmentPlanNum);
    }

    public static InstallmentPlan GetOneForFam(long guarNum)
    {
        return GetForFams([guarNum]).TryGetValue(guarNum, out var installPlan) ? installPlan : null;
    }

    public static Dictionary<long, InstallmentPlan> GetForFams(List<long> listGuarNums)
    {
        if (listGuarNums.Count == 0)
        {
            return new Dictionary<long, InstallmentPlan>();
        }

        return InstallmentPlanCrud
            .SelectMany("SELECT * FROM installmentplan WHERE PatNum IN (" + string.Join(",", listGuarNums) + ")")
            .GroupBy(x => x.PatNum)
            .ToDictionary(
                x => x.Key,
                y => y.First());
    }

    public static List<InstallmentPlan> GetListForFams(List<long> guarNums)
    {
        if (guarNums.Count == 0)
        {
            return [];
        }

        return InstallmentPlanCrud
            .SelectMany("SELECT * FROM installmentplan WHERE PatNum IN(" + string.Join(",", guarNums) + ")")
            .ToList();
    }

    public static List<InstallmentPlan> GetForSuperFam(long superFamNum)
    {
        return GetForSuperFams([superFamNum]).TryGetValue(superFamNum, out var plans) ? plans : [];
    }

    public static Dictionary<long, List<InstallmentPlan>> GetForSuperFams(List<long> superFamNums)
    {
        if (superFamNums.Count == 0)
        {
            return new Dictionary<long, List<InstallmentPlan>>();
        }

        var dataTable = DataCore.GetTable(
            "SELECT installmentplan.*, patient.SuperFamily FROM installmentplan " +
            "INNER JOIN patient ON installmentplan.PatNum = patient.PatNum " +
            "WHERE patient.SuperFamily IN (" + string.Join(",", superFamNums) + ") " +
            "AND patient.HasSuperBilling = 1 " +
            "GROUP BY installmentplan.PatNum");

        var installmentPlans = InstallmentPlanCrud.TableToList(dataTable);

        var plans = new Dictionary<long, List<InstallmentPlan>>();
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var superFamNum = SIn.Long(dataTable.Rows[i]["SuperFamily"].ToString());
            if (!plans.ContainsKey(superFamNum))
            {
                plans.Add(superFamNum, []);
            }

            plans[superFamNum].Add(installmentPlans[i]);
        }

        return plans;
    }
}