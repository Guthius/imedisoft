using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class InsPlanPreferences
{
    public static InsPlanPreference GetOne(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, long planNum)
    {
        return InsPlanPreferenceCrud.SelectOne(
            $"""
             SELECT * FROM insplanpreference 
             WHERE FKey={fKey}
             AND FKeyType={(int) insPlanPrefFKeyType}
             AND PlanNum={planNum}
             """);
    }

    public static List<InsPlanPreference> GetManyForPlanNums(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, List<long> planNums)
    {
        if (planNums.Count == 0) return [];

        return InsPlanPreferenceCrud.SelectMany(
            $"""
             SELECT * FROM insplanpreference
             WHERE FKey={fKey} 
             AND FKeyType={(int) insPlanPrefFKeyType} 
             AND PlanNum IN ({string.Join(",", planNums)})
             """);
    }

    public static List<InsPlanPreference> GetManyForFKeys(List<long> fkeys, InsPlanPrefFKeyType insPlanPrefFKeyType, InsPlan insPlan)
    {
        if (fkeys.Count == 0 || insPlan == null) return [];

        return InsPlanPreferenceCrud.SelectMany(
            $"""
             SELECT * FROM insplanpreference 
             WHERE PlanNum={insPlan.PlanNum}
             AND FKeyType={(int) insPlanPrefFKeyType}
             AND FKey IN ({string.Join(",", fkeys)})
             """);
    }

    public static void UpsertMany(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, List<long> planNums, string valueString)
    {
        var insPlanPreferences = GetManyForPlanNums(fKey, insPlanPrefFKeyType, planNums);

        foreach (var planNum in planNums)
        {
            var insPlanPreference = insPlanPreferences.Find(x => x.PlanNum == planNum);
            if (insPlanPreference == null)
            {
                insPlanPreference = new InsPlanPreference
                {
                    PlanNum = planNum,
                    FKey = fKey,
                    FKeyType = insPlanPrefFKeyType,
                    ValueString = valueString
                };
                InsPlanPreferenceCrud.Insert(insPlanPreference);
                continue;
            }

            var insPlanPreferenceOld = insPlanPreference.Copy();

            insPlanPreference.ValueString = valueString;

            InsPlanPreferenceCrud.Update(insPlanPreference, insPlanPreferenceOld);
        }
    }

    public static void DeleteMany(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, List<long> planNums)
    {
        if (planNums.Count == 0) return;

        Db.NonQ(
            $"""
             DELETE FROM insplanpreference 
             WHERE FKey={fKey} 
             AND FKeyType={(int) insPlanPrefFKeyType} 
             AND PlanNum IN ({string.Join(",", planNums)})
             """);
    }

    public static bool NoBillIns(ProcedureCode procedureCode, InsPlan insPlan)
    {
        if (insPlan == null)
        {
            return procedureCode.NoBillIns;
        }

        var insPlanPreference = GetOne(procedureCode.CodeNum, InsPlanPrefFKeyType.ProcCodeNoBillIns, insPlan.PlanNum);
        if (insPlanPreference == null)
        {
            return procedureCode.NoBillIns;
        }

        var noBillInsOverride = SIn.Enum<NoBillInsOverride>(insPlanPreference.ValueString);

        return noBillInsOverride switch
        {
            NoBillInsOverride.BillToIns => false,
            NoBillInsOverride.DoNotUsuallyBillToIns => true,
            _ => procedureCode.NoBillIns
        };
    }

    public static bool NoBillIns(ProcedureCode procedureCode, List<InsPlanPreference> insPlanPreferences)
    {
        if (insPlanPreferences.IsNullOrEmpty())
        {
            return procedureCode.NoBillIns;
        }

        var insPlanPreference = insPlanPreferences.Find(x => x.FKey == procedureCode.CodeNum && x.FKeyType == InsPlanPrefFKeyType.ProcCodeNoBillIns);
        if (insPlanPreference == null)
        {
            return procedureCode.NoBillIns;
        }

        var noBillInsOverride = SIn.Enum<NoBillInsOverride>(insPlanPreference.ValueString);

        return noBillInsOverride switch
        {
            NoBillInsOverride.BillToIns => false,
            NoBillInsOverride.DoNotUsuallyBillToIns => true,
            _ => procedureCode.NoBillIns
        };
    }
}