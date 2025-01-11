using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InstallmentPlans
{
    ///<summary>Gets one InstallmentPlan from the db.</summary>
    public static InstallmentPlan GetOne(long installmentPlanNum)
    {
        return InstallmentPlanCrud.SelectOne(installmentPlanNum);
    }

    
    public static long Insert(InstallmentPlan installmentPlan)
    {
        return InstallmentPlanCrud.Insert(installmentPlan);
    }

    
    public static void Update(InstallmentPlan installmentPlan)
    {
        InstallmentPlanCrud.Update(installmentPlan);
    }

    
    public static void Delete(long installmentPlanNum)
    {
        var command = "DELETE FROM installmentplan WHERE InstallmentPlanNum = " + SOut.Long(installmentPlanNum);
        Db.NonQ(command);
    }

    #region Get Methods

    ///<summary>Gets the installment plan for this family.  If none, returns null.</summary>
    public static InstallmentPlan GetOneForFam(long guarNum)
    {
        InstallmentPlan installPlan;
        if (GetForFams(new List<long> {guarNum}).TryGetValue(guarNum, out installPlan)) return installPlan;
        return null;
    }

    /// <summary>
    ///     Gets the installment plans for these families.  If there are none for a family, the guarantor will not be present
    ///     in the dictionary.
    /// </summary>
    /// <returns>Dictionary where the key is the guarantor num and the value is the installment plan for the family.</returns>
    public static Dictionary<long, InstallmentPlan> GetForFams(List<long> listGuarNums)
    {
        if (listGuarNums.Count == 0) return new Dictionary<long, InstallmentPlan>();

        var command = "SELECT * FROM installmentplan WHERE PatNum IN(" + string.Join(",", listGuarNums.Select(x => SOut.Long(x))) + ") ";
        return InstallmentPlanCrud.SelectMany(command)
            .GroupBy(x => x.PatNum)
            .ToDictionary(x => x.Key, y => y.First()); //Only returning one installment plan per family.
    }

    /// <summary>
    ///     Gets the installment plans for these families.  If there are none for a family, the guarantor will not be
    ///     present in the list.
    /// </summary>
    /// <returns>A list of installment plans grouped by PatNum.</returns>
    public static List<InstallmentPlan> GetListForFams(List<long> listGuarNums)
    {
        if (listGuarNums.Count == 0) return new List<InstallmentPlan>();

        var command = "SELECT * FROM installmentplan WHERE PatNum IN(" + string.Join(",", listGuarNums.Select(x => SOut.Long(x))) + ") ";
        var listInstallmentPlans = InstallmentPlanCrud.SelectMany(command).ToList();
        return listInstallmentPlans; //Only returning one installment plan per family.
    }

    ///<summary>Gets the installment plans for a SuperFamily.  If none, returns empty list.</summary>
    public static List<InstallmentPlan> GetForSuperFam(long superFamNum)
    {
        List<InstallmentPlan> listPlans;
        if (GetForSuperFams(new List<long> {superFamNum}).TryGetValue(superFamNum, out listPlans)) return listPlans;
        return new List<InstallmentPlan>();
    }

    /// <summary>
    ///     Gets the installment plans for these  super families.  If there are none for a super family, the super family head
    ///     will not be
    ///     present in the dictionary.
    /// </summary>
    /// <returns>Dictionary where the key is the super family head and the value is the installment plan for the super family.</returns>
    public static Dictionary<long, List<InstallmentPlan>> GetForSuperFams(List<long> listSuperFamNums)
    {
        if (listSuperFamNums.Count == 0) return new Dictionary<long, List<InstallmentPlan>>();

        var command = "SELECT installmentplan.*,patient.SuperFamily FROM installmentplan "
                      + "INNER JOIN patient ON installmentplan.PatNum=patient.PatNum "
                      + "WHERE patient.SuperFamily IN(" + string.Join(",", listSuperFamNums.Select(x => SOut.Long(x))) + ") "
                      + "AND patient.HasSuperBilling=1 "
                      + "GROUP BY installmentplan.PatNum";
        var table = DataCore.GetTable(command);
        var listInstallmentPlans = InstallmentPlanCrud.TableToList(table);
        var dictPlans = new Dictionary<long, List<InstallmentPlan>>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var superFamNum = SIn.Long(table.Rows[i]["SuperFamily"].ToString());
            if (!dictPlans.ContainsKey(superFamNum)) dictPlans.Add(superFamNum, new List<InstallmentPlan>());
            dictPlans[superFamNum].Add(listInstallmentPlans[i]);
        }

        return dictPlans;
    }

    #endregion
}