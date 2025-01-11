using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsPlanPreferences
{
    #region Methods - Get

    ///<summary>Gets single InsPlanPreference entry from the db by insPlanPrefNum.</summary>
    public static InsPlanPreference GetOne(long insPlanPrefNum)
    {
        return InsPlanPreferenceCrud.SelectOne(insPlanPrefNum);
    }

    ///<summary>Gets single InsPlanPreference entry from the db using a combination of FkeyType/Fkey/PlanNum.</summary>
    public static InsPlanPreference GetOne(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, long planNum)
    {
        var command = $@"SELECT * FROM insplanpreference 
				WHERE FKey={SOut.Long(fKey)}
				AND FKeyType={SOut.Int((int) insPlanPrefFKeyType)}
				AND PlanNum={SOut.Long(planNum)}";
        return InsPlanPreferenceCrud.SelectOne(command);
    }

    ///<summary>Retrieves InsPlanPreference entries for multiple insurance plans based on FkeyType and FKey.</summary>
    public static List<InsPlanPreference> GetManyForPlanNums(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, List<long> listPlanNums)
    {
        if (listPlanNums.Count == 0) return new List<InsPlanPreference>();

        var command = $@"SELECT * FROM insplanpreference
				WHERE FKey={SOut.Long(fKey)} 
				AND FKeyType={SOut.Int((int) insPlanPrefFKeyType)} 
				AND PlanNum IN ({string.Join(",", listPlanNums)})";
        return InsPlanPreferenceCrud.SelectMany(command);
    }

    ///<summary>Retrieves InsPlanPreference entries for multiple FKeys based on FkeyType and planNum.</summary>
    public static List<InsPlanPreference> GetManyForFKeys(List<long> listFKeys, InsPlanPrefFKeyType insPlanPrefFKeyType, InsPlan insPlan)
    {
        if (listFKeys.Count == 0 || insPlan == null) return new List<InsPlanPreference>();

        var command = $@"SELECT * FROM insplanpreference 
				WHERE PlanNum={SOut.Long(insPlan.PlanNum)}
				AND FKeyType={SOut.Int((int) insPlanPrefFKeyType)}
				AND FKey IN ({string.Join(",", listFKeys)})";
        return InsPlanPreferenceCrud.SelectMany(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(InsPlanPreference insPlanPreference)
    {
        return InsPlanPreferenceCrud.Insert(insPlanPreference);
    }

    
    public static void Update(InsPlanPreference insPlanPreference)
    {
        InsPlanPreferenceCrud.Update(insPlanPreference);
    }

    ///<summary>Updates or inserts a InsPlanPreference object for every PlanNum provided.</summary>
    public static void UpsertMany(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, List<long> listPlanNums, string valueString)
    {
        var listInsPlanPreferences = GetManyForPlanNums(fKey, insPlanPrefFKeyType, listPlanNums);
        for (var i = 0; i < listPlanNums.Count; i++)
        {
            var insPlanPreference = listInsPlanPreferences.Find(x => x.PlanNum == listPlanNums[i]);
            if (insPlanPreference == null)
            {
                insPlanPreference = new InsPlanPreference();
                insPlanPreference.PlanNum = listPlanNums[i];
                insPlanPreference.FKey = fKey;
                insPlanPreference.FKeyType = insPlanPrefFKeyType;
                insPlanPreference.ValueString = valueString;
                InsPlanPreferenceCrud.Insert(insPlanPreference);
                continue;
            }

            var insPlanPreferenceOld = insPlanPreference.Copy();
            insPlanPreference.ValueString = valueString;
            InsPlanPreferenceCrud.Update(insPlanPreference, insPlanPreferenceOld);
        }
    }

    ///<summary>Deletes a single InsPlanPreference entry in the database using insPlanPrefNum</summary>
    public static void Delete(long insPlanPrefNum)
    {
        InsPlanPreferenceCrud.Delete(insPlanPrefNum);
    }


    ///<summary>Deletes all of the rows that match the fKey and fKeyType combination for the ins plans passed in.</summary>
    public static void DeleteMany(long fKey, InsPlanPrefFKeyType insPlanPrefFKeyType, List<long> listPlanNums)
    {
        if (listPlanNums.Count == 0) return;

        var command = $@"DELETE FROM insplanpreference 
				WHERE FKey={SOut.Long(fKey)} 
				AND FKeyType={SOut.Int((int) insPlanPrefFKeyType)} 
				AND PlanNum IN ({string.Join(",", listPlanNums)})";
        Db.NonQ(command);
    }

    #endregion Methods - Modify

    #region Methods - Misc

    /// <summary>
    ///     Makes a call to the db. Checks if Insurance plan has NoBillIns override for the passed in procedureCode. If no,
    ///     returns the procedure code NoBillIns value.
    ///     If insurance plan has an override, then returns the insurance plan NoBillIns override value.
    /// </summary>
    public static bool NoBillIns(ProcedureCode procedureCode, InsPlan insPlan)
    {
        if (insPlan == null) return procedureCode.NoBillIns;
        var insPlanPreference = GetOne(procedureCode.CodeNum, InsPlanPrefFKeyType.ProcCodeNoBillIns, insPlan.PlanNum);
        if (insPlanPreference == null) return procedureCode.NoBillIns;
        var noBillInsOverride = SIn.Enum<NoBillInsOverride>(insPlanPreference.ValueString);
        switch (noBillInsOverride)
        {
            case NoBillInsOverride.BillToIns:
                return false;
            case NoBillInsOverride.DoNotUsuallyBillToIns:
                return true;
        }

        return procedureCode.NoBillIns;
    }

    /// <summary>
    ///     Does not make a call to the db. Checks if Insurance plan has NoBillIns override in the passed in list of
    ///     InsPlanPreferences for the specified procedureCode.
    ///     If no, returns the procedure code NoBillIns value. If insurance plan has an override, then returns the insurance
    ///     plan NoBillIns override value.
    ///     Use this to avoid making multiple calls to db.
    /// </summary>
    public static bool NoBillIns(ProcedureCode procedureCode, List<InsPlanPreference> listInsPlanPreferences)
    {
        if (listInsPlanPreferences.IsNullOrEmpty()) return procedureCode.NoBillIns;
        var insPlanPreference = listInsPlanPreferences.Find(x => x.FKey == procedureCode.CodeNum && x.FKeyType == InsPlanPrefFKeyType.ProcCodeNoBillIns);
        if (insPlanPreference == null) return procedureCode.NoBillIns;
        var noBillInsOverride = SIn.Enum<NoBillInsOverride>(insPlanPreference.ValueString);
        switch (noBillInsOverride)
        {
            case NoBillInsOverride.BillToIns:
                return false;
            case NoBillInsOverride.DoNotUsuallyBillToIns:
                return true;
        }

        return procedureCode.NoBillIns;
    }

    #endregion Methods - Misc
}