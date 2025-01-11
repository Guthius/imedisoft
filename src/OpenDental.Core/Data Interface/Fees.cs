using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Fees
{
    #region Update

    ///<summary>Only set doCheckFeeSchedGroups to false from FeeSchedGroups.</summary>
    public static void Update(Fee fee, Fee feeOld = null, bool doCheckFeeSchedGroups = true)
    {
        //Check if this fee is associated to a FeeSchedGroup and update the rest of the group as needed.
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.UpsertGroupFees(new List<Fee> {fee});
        if (feeOld != null)
            FeeCrud.Update(fee, feeOld);
        else
            FeeCrud.Update(fee);
    }

    #endregion Update

    #region Get Methods

    ///<summary>Gets the list of fees by clinic num from the db.</summary>
    public static List<Fee> GetByClinicNum(long clinicNum)
    {
        var command = "SELECT * FROM fee WHERE ClinicNum = " + SOut.Long(clinicNum);
        return FeeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets the list of fees by feeschednums and clinicnums from the db.  Returns an empty list if listFeeSchedNums is
    ///     null or empty.
    ///     Throws an application exception if listClinicNums is null or empty.  Always provide at least one ClinicNum.
    ///     We throw instead of returning an empty list which would make it look like there are no fees for the fee schedules
    ///     passed in.
    ///     If this method returns an empty list it is because no valied fee schedules were given or the database truly doesn't
    ///     have any fees.
    /// </summary>
    public static List<FeeLim> GetByFeeSchedNumsClinicNums(List<long> listFeeSchedNums, List<long> listClinicNums)
    {
        if (listFeeSchedNums == null || listFeeSchedNums.Count == 0) return new List<FeeLim>(); //This won't hurt the FeeCache because there will be no corresponding fee schedules to "blank out".
        if (listClinicNums == null || listClinicNums.Count == 0)
            //Returning an empty list here would be detrimental to the FeeCache.
            throw new ApplicationException("Invalid listClinicNums passed into GetByFeeSchedNumsClinicNums()");
        var command = "SELECT FeeNum,Amount,FeeSched,CodeNum,ClinicNum,ProvNum,SecDateTEdit FROM fee "
                      + "WHERE FeeSched IN (" + string.Join(",", listFeeSchedNums.Select(x => SOut.Long(x))) + ") "
                      + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ")";
        var listFeeLimsRet = DataCore.GetTable(command).AsEnumerable()
            .Select(x => new FeeLim
            {
                FeeNum = SIn.Long(x["FeeNum"].ToString()),
                Amount = SIn.Double(x["Amount"].ToString()),
                FeeSched = SIn.Long(x["FeeSched"].ToString()),
                CodeNum = SIn.Long(x["CodeNum"].ToString()),
                ClinicNum = SIn.Long(x["ClinicNum"].ToString()),
                ProvNum = SIn.Long(x["ProvNum"].ToString()),
                SecDateTEdit = SIn.DateTime(x["SecDateTEdit"].ToString())
            }).ToList();
        return listFeeLimsRet;
    }

    ///<summary>Counts the number of fees in the db for this fee sched, including all clinic/prov/date effective overrides.</summary>
    public static int GetCountByFeeSchedNum(long feeSchedNum)
    {
        var command = "SELECT COUNT(*) FROM fee WHERE FeeSched =" + SOut.Long(feeSchedNum);
        return SIn.Int(Db.GetCount(command));
    }

    /// <summary>
    ///     Searches for the given codeNum and feeSchedNum and finds the most appropriate match for the
    ///     clinicNum/provNum/effective date.  If listFees is null, it will go to db. Default dateEffective is DateTime.Today.
    /// </summary>
    public static Fee GetFee(long codeNum, long feeSchedNum, long clinicNum, long provNum, List<Fee> listFees = null, DateTime dateEffective = new())
    {
        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        //use listFees if supplied regardless of the FeesUseCache pref since the fee cache is not really thread safe
        if (listFees != null) return GetFeeFromList(listFees, codeNum, feeSchedNum, clinicNum, provNum, dateEffective: dateEffective);
        return GetFeeFromDb(codeNum, feeSchedNum, clinicNum, provNum, dateEffective: dateEffective);
    }

    /// <summary>
    ///     Searches the db for a fee with the exact codeNum, feeSchedNum, clinicNum, and provNum provided.  Returns null
    ///     if no exact match found. The goal of this method is to have a way to check the database for "duplicate" fees before
    ///     adding more fees to the db. Set exactMatchForApi to true to exactly match all passed in parameters. dateEffective
    ///     should always be set.
    /// </summary>
    public static Fee GetFeeFromDb(long codeNum, long feeSchedNum, long clinicNum = 0, long provNum = 0, bool exactMatchForApi = false, DateTime dateEffective = new())
    {
        if (FeeScheds.IsGlobal(feeSchedNum) && !exactMatchForApi)
        {
            clinicNum = 0;
            provNum = 0;
        }

        //Search for exact match first.  This would include a clinic and provider override.
        var command = "";
        if (exactMatchForApi)
            command += @"SELECT fee.*
				FROM fee
				WHERE fee.CodeNum=" + SOut.Long(codeNum) + @"
				AND fee.FeeSched=" + SOut.Long(feeSchedNum) + @"
				AND fee.ClinicNum=" + SOut.Long(clinicNum) + @"
				AND fee.ProvNum=" + SOut.Long(provNum) + @"
				AND fee.DateEffective=" + SOut.Date(dateEffective);
        else
            command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSchedNum) + " AND CodeNum=" + SOut.Long(codeNum) + " "
                      + "AND ClinicNum=" + SOut.Long(clinicNum) + " AND ProvNum=" + SOut.Long(provNum) + " "
                      + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                      + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                      + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        if (exactMatchForApi || FeeScheds.IsGlobal(feeSchedNum)) return FeeCrud.SelectOne(command);
        //Provider override
        command += " UNION ALL "
                   + "SELECT fee1.* FROM fee fee1 "
                   + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                   + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSchedNum) + " AND CodeNum=" + SOut.Long(codeNum) + " "
                   + "AND ClinicNum=0 AND ProvNum=" + SOut.Long(provNum) + " "
                   + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        //Clinic override
        command += " UNION ALL "
                   + "SELECT fee1.* FROM fee fee1 "
                   + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                   + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSchedNum) + " AND CodeNum=" + SOut.Long(codeNum) + " "
                   + "AND ClinicNum=" + SOut.Long(clinicNum) + " AND ProvNum=0 "
                   + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        //Unassigned clinic with no override
        command += " UNION ALL "
                   + "SELECT fee1.* FROM fee fee1 "
                   + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                   + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSchedNum) + " AND CodeNum=" + SOut.Long(codeNum) + " "
                   + "AND ClinicNum=0 AND ProvNum=0 "
                   + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectOne(command);
        //Only the first result gets returned.
        //Using the UNION keeps it down to one query.
    }

    /// <summary>
    ///     Same logic as above, in Fees.GetFeeFromDb().
    ///     Typical to pass in a list of fees for just one or a few feescheds so that the search goes quickly.
    ///     When exactMatchForApi is true, this will return either the fee that matches the parameters exactly, or null if no
    ///     such fee exists.
    ///     When exactMatchForApi is false, and the fee schedule is global, we ignore the clinicNum and provNum and return the
    ///     HQ fee that matches the given codeNum and feeSchedNum.
    ///     When exactMatchForApi is false, and the fee schedule is not global, and no exact match exists we attempt to return
    ///     the closest matching fee in this order:
    ///     1 - The fee with the same codeNum, feeSchedNum, and providerNum, with a clinicNum of 0
    ///     2 - The fee with the same codeNum, feeSchedNum, and clinicNum, with a providerNum of 0
    ///     3 - The fee with the same codeNum, feeSchedNum, and both a clinicNum and providerNum of 0
    ///     If no partial match can be found, return null.
    /// </summary>
    private static Fee GetFeeFromList(List<Fee> listFees, long codeNum, long feeSched = 0, long clinicNum = 0, long provNum = 0, bool exactMatchForApi = false, DateTime dateEffective = new())
    {
        if (FeeScheds.IsGlobal(feeSched) && !exactMatchForApi)
        {
            //speed things up here with less loops
            clinicNum = 0;
            provNum = 0;
        }

        Fee fee;
        //listFeesDateValid is a list of fees that match the given parameters, but might have different dateEffectives.
        //We then take Max from that list.
        var listFeesDateValid = new List<Fee>();
        if (exactMatchForApi)
        {
            fee = listFees.Find(f => f.CodeNum == codeNum && f.FeeSched == feeSched && f.ClinicNum == clinicNum && f.ProvNum == provNum && f.DateEffective == dateEffective);
        }
        else
        {
            listFeesDateValid = listFees.FindAll(f => f.CodeNum == codeNum && f.FeeSched == feeSched && f.ClinicNum == clinicNum && f.ProvNum == provNum && f.DateEffective <= dateEffective);
            fee = listFeesDateValid.Find(f => listFeesDateValid.Max(x => x.DateEffective) == f.DateEffective);
        }

        if (fee != null) return fee; //match found.  Would include a clinic and provider override.
        if (exactMatchForApi || FeeScheds.IsGlobal(feeSched)) return null; //couldn't find exact match
        //no exact match exists, so we look for closest match
        //2: Prov override
        listFeesDateValid = listFees.FindAll(f => f.CodeNum == codeNum && f.FeeSched == feeSched && f.ClinicNum == 0 && f.ProvNum == provNum && f.DateEffective <= dateEffective);
        fee = listFeesDateValid.Find(f => listFeesDateValid.Max(x => x.DateEffective) == f.DateEffective);
        if (fee != null) return fee;
        //3: Clinic override
        listFeesDateValid = listFees.FindAll(f => f.CodeNum == codeNum && f.FeeSched == feeSched && f.ClinicNum == clinicNum && f.ProvNum == 0 && f.DateEffective <= dateEffective);
        fee = listFeesDateValid.Find(f => listFeesDateValid.Max(x => x.DateEffective) == f.DateEffective);
        if (fee != null) return fee;
        //4: Just unassigned clinic default
        listFeesDateValid = listFees.FindAll(f => f.CodeNum == codeNum && f.FeeSched == feeSched && f.ClinicNum == 0 && f.ProvNum == 0 && f.DateEffective <= dateEffective);
        fee = listFeesDateValid.Find(f => listFeesDateValid.Max(x => x.DateEffective) == f.DateEffective);
        //whether it's null or not:
        return fee;
    }

    ///<summary>Used by FeeSchedGroups. Does not use DateEffective in order to get all the fees regardless of effective date.</summary>
    public static List<Fee> GetAllFeesForClinics(long codeNum, long feeSchedNum, long provNum, List<long> listClinicNums)
    {
        var command = "SELECT fee.* FROM fee "
                      + "WHERE fee.CodeNum=" + SOut.Long(codeNum) + " "
                      + "AND fee.FeeSched=" + SOut.Long(feeSchedNum) + " "
                      + "AND fee.ProvNum=" + SOut.Long(provNum);
        if (!listClinicNums.IsNullOrEmpty()) command += " AND fee.ClinicNum IN(" + string.Join(",", listClinicNums.Select(SOut.Long)) + ")";
        return FeeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets fees for up to three feesched/clinic/prov/date effective combos. If filtering with a ClinicNum and/or
    ///     ProvNum, it only includes fees that match that clinicNum/provnum or have zero. Will also try to get the most recent
    ///     effective date if not the minimum value.  This reduces the result set if there are clinic/provider/date effective
    ///     overrides. This could easily scale to many thousands of clinics and providers.
    /// </summary>
    public static List<Fee> GetListForScheds(long feeSched1, long clinicNum1 = 0, long provNum1 = 0, long feeSched2 = 0, long clinicNum2 = 0, long provNum2 = 0, long feeSched3 = 0, long clinicNum3 = 0, long provNum3 = 0, DateTime dateEffective = new())
    {
        return GetListForSchedsAndClinics(feeSched1, new List<long> {clinicNum1}, provNum1, feeSched2, new List<long> {clinicNum2}, provNum2, feeSched3, new List<long> {clinicNum3}, provNum3, dateEffective);
    }

    /// <summary>
    ///     Gets fees for up to three feesched/clinic/prov/dateEffective combos. If filtering with a ClinicNum and/or
    ///     ProvNum, it only includes fees that match that clinicNum/provnum or have zero. Will also get the most recent
    ///     effective date based on the passed in dateEffective or DateTime.Today by default.  This reduces the result set if
    ///     there are clinic and provider overrides. This could easily scale to many thousands of clinics and providers.
    /// </summary>
    public static List<Fee> GetListForSchedsAndClinics(long feeSched1, List<long> listClinics1 = null, long provNum1 = 0, long feeSched2 = 0, List<long> listClinics2 = null,
        long provNum2 = 0, long feeSched3 = 0, List<long> listClinics3 = null, long provNum3 = 0, DateTime dateEffective = new())
    {
        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        var listClinicNums = new List<long> {0};
        if (!listClinics1.IsNullOrEmpty()) listClinicNums.AddRange(listClinics1);
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSched1) + " "
                      + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") AND ProvNum=" + SOut.Long(provNum1) + " "
                      + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                      + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                      + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        if (feeSched2 != 0)
        {
            listClinicNums.Clear();
            listClinicNums.Add(0);
            if (!listClinics2.IsNullOrEmpty()) listClinicNums.AddRange(listClinics2);
            command += " UNION SELECT fee1.* FROM fee fee1 "
                       + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                       + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSched2) + " "
                       + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") AND ProvNum=" + SOut.Long(provNum2) + " "
                       + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                       + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                       + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        }

        if (feeSched3 != 0)
        {
            listClinicNums.Clear();
            listClinicNums.Add(0);
            if (!listClinics3.IsNullOrEmpty()) listClinicNums.AddRange(listClinics3);
            command += " UNION SELECT fee1.* FROM fee fee1 "
                       + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                       + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSched3) + " "
                       + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") AND ProvNum=" + SOut.Long(provNum3) + " "
                       + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                       + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                       + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        }

        return FeeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all possible fees associated with the various objects passed in.  Gets fees from db based on code and fee
    ///     schedule combos.  Includes all provider overrides.  Includes default/no clinic as well as any specified clinic
    ///     overrides. Although the list always includes extra fees from scheds that we don't need, it's still a very small
    ///     list.  That list is then used repeatedly by other code in loops to find the actual individual fee amounts. Filters
    ///     the fees to the most recent effective date based on the DateEffective passed in.
    /// </summary>
    public static List<Fee> GetListFromObjects(List<ProcedureCode> listProcedureCodes, List<string> listMedicalCodes, List<long> listProvNumsTreat, long patPriProv,
        long patSecProv, long patFeeSched, List<InsPlan> listInsPlans, List<long> listClinicNums, List<Appointment> listAppointments,
        List<SubstitutionLink> listSubstitutionLinks, long discountPlanNum, DateTime dateEffective = new()
        //listCodeNums,listProvNumsTreat,listProcCodesProvNumDefault,patPriProv,patSecProv,patFeeSched,listInsPlans,listClinicNums
        //List<long> listProcCodesProvNumDefault
    )
    {
        //listMedicalCodes: it already automatically gets the medical codes from procCodes.  This is just for procs. If no procs yet, it will be null.
        //listMedicalCodes can be done by: listProcedures.Select(x=>x.MedicalCode).ToList();  //this is just the strings
        //One way to get listProvNumsTreat is listProcedures.Select(x=>x.ProvNum).ToList()
        //One way to specify a single provNum in listProvNumsTreat is new List<long>(){provNum}
        //One way to get clinicNums is listProcedures.Select(x=>x.ClinicNum).ToList()
        //Another way to get clinicNums is new List<long>(){clinicNum}.
        //These objects will be cleaned up, so they can have duplicates, zeros, invalid keys, nulls, etc
        //In some cases, we need to pass in a list of appointments to make sure we've included all possible providers, both ProvNum and ProvHyg
        //In that case, it's common to leave listProvNumsTreat null because we clearly do not have any of those providers set yet.

        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        if (listProcedureCodes == null) return new List<Fee>();
        var listCodeNumsOut = new List<long>();
        for (var i = 0; i < listProcedureCodes.Count; i++)
        {
            if (listProcedureCodes[i] == null) continue;
            if (!listCodeNumsOut.Contains(listProcedureCodes[i].CodeNum)) listCodeNumsOut.Add(listProcedureCodes[i].CodeNum);
            if (ProcedureCodes.IsValidCode(listProcedureCodes[i].MedicalCode))
            {
                var codeNumMed = ProcedureCodes.GetCodeNum(listProcedureCodes[i].MedicalCode);
                if (!listCodeNumsOut.Contains(codeNumMed)) listCodeNumsOut.Add(codeNumMed);
            }

            if (ProcedureCodes.IsValidCode(listProcedureCodes[i].SubstitutionCode))
            {
                var codeNumSub = ProcedureCodes.GetCodeNum(listProcedureCodes[i].SubstitutionCode);
                if (!listCodeNumsOut.Contains(codeNumSub)) listCodeNumsOut.Add(codeNumSub);
            }
        }

        if (listMedicalCodes != null)
            for (var i = 0; i < listMedicalCodes.Count; i++)
                if (ProcedureCodes.IsValidCode(listMedicalCodes[i]))
                {
                    var codeNumMed = ProcedureCodes.GetCodeNum(listMedicalCodes[i]);
                    if (!listCodeNumsOut.Contains(codeNumMed)) listCodeNumsOut.Add(codeNumMed);
                }

        if (listSubstitutionLinks != null)
            for (var i = 0; i < listSubstitutionLinks.Count; i++)
                //Grab all subst codes, since we don't know which ones we will need.
                if (ProcedureCodes.IsValidCode(listSubstitutionLinks[i].SubstitutionCode))
                {
                    var codeNum = ProcedureCodes.GetCodeNum(listSubstitutionLinks[i].SubstitutionCode);
                    if (!listCodeNumsOut.Contains(codeNum)) listCodeNumsOut.Add(codeNum);
                }

        //Fee schedules. Will potentially include many.=======================================================================================
        var listFeeScheds = new List<long>();
        //Add feesched for first provider (See Claims.CalculateAndUpdate)---------------------------------------------------------------------
        var provFirst = Providers.GetFirst();
        if (provFirst != null && provFirst.FeeSched != 0 && !listFeeScheds.Contains(provFirst.FeeSched)) listFeeScheds.Add(provFirst.FeeSched);
        //Add feesched for PracticeDefaultProv------------------------------------------------------------------------------------------------
        var provPracticeDefault = Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv));
        if (provPracticeDefault != null && provPracticeDefault.FeeSched != 0 && !listFeeScheds.Contains(provPracticeDefault.FeeSched)) listFeeScheds.Add(provPracticeDefault.FeeSched);
        //Add feescheds for all treating providers---------------------------------------------------------------------------------------------
        if (listProvNumsTreat != null)
            for (var i = 0; i < listProvNumsTreat.Count; i++)
            {
                var provTreat = Providers.GetProv(listProvNumsTreat[i]);
                if (provTreat != null && provTreat.FeeSched != 0 && !listFeeScheds.Contains(provTreat.FeeSched)) listFeeScheds.Add(provTreat.FeeSched); //treating provs fee scheds
            }

        //Add feescheds for the patient's primary and secondary providers----------------------------------------------------------------------
        var providerPatPri = Providers.GetProv(patPriProv);
        if (providerPatPri != null && providerPatPri.FeeSched != 0 && !listFeeScheds.Contains(providerPatPri.FeeSched)) listFeeScheds.Add(providerPatPri.FeeSched);
        var providerPatSec = Providers.GetProv(patSecProv);
        if (providerPatSec != null && providerPatSec.FeeSched != 0 && !listFeeScheds.Contains(providerPatSec.FeeSched)) listFeeScheds.Add(providerPatSec.FeeSched);
        //Add feescheds for all procedurecode.ProvNumDefaults---------------------------------------------------------------------------------
        for (var i = 0; i < listProcedureCodes.Count; i++)
        {
            if (listProcedureCodes[i] == null) continue;
            var provNumDefault = listProcedureCodes[i].ProvNumDefault;
            if (provNumDefault == 0) continue;
            var provDefault = Providers.GetProv(provNumDefault);
            if (provDefault != null && provDefault.FeeSched != 0 && !listFeeScheds.Contains(provDefault.FeeSched)) listFeeScheds.Add(provDefault.FeeSched);
        }

        //Add feescheds for appointment providers---------------------------------------------------------------------------------------------
        if (listAppointments != null)
            for (var i = 0; i < listAppointments.Count; i++)
            {
                var provAppt = Providers.GetProv(listAppointments[i].ProvNum);
                if (provAppt != null && provAppt.FeeSched != 0 && !listFeeScheds.Contains(provAppt.FeeSched)) listFeeScheds.Add(provAppt.FeeSched);
                var provApptHyg = Providers.GetProv(listAppointments[i].ProvHyg);
                if (provApptHyg != null && provApptHyg.FeeSched != 0 && !listFeeScheds.Contains(provApptHyg.FeeSched)) listFeeScheds.Add(provApptHyg.FeeSched);
            }

        //Add feesched for patient.  Rare. --------------------------------------------------------------------------------------------------
        if (patFeeSched != 0)
            if (!listFeeScheds.Contains(patFeeSched))
                listFeeScheds.Add(patFeeSched);

        //Add feesched for each insplan, both reg and allowed, and Manual Blue Book---------------------------------------------------------------------
        if (listInsPlans != null)
            for (var i = 0; i < listInsPlans.Count; i++)
            {
                if (listInsPlans[i].FeeSched != 0 && !listFeeScheds.Contains(listInsPlans[i].FeeSched)) listFeeScheds.Add(listInsPlans[i].FeeSched); //insplan feeSched
                if (listInsPlans[i].AllowedFeeSched != 0 && !listFeeScheds.Contains(listInsPlans[i].AllowedFeeSched)) listFeeScheds.Add(listInsPlans[i].AllowedFeeSched); //allowed feeSched
                if (listInsPlans[i].CopayFeeSched != 0 && !listFeeScheds.Contains(listInsPlans[i].CopayFeeSched)) listFeeScheds.Add(listInsPlans[i].CopayFeeSched); //copay feeSched
                if (listInsPlans[i].ManualFeeSchedNum != 0 && !listFeeScheds.Contains(listInsPlans[i].ManualFeeSchedNum)) listFeeScheds.Add(listInsPlans[i].ManualFeeSchedNum); //manual blue book feeSched
            }

        if (discountPlanNum != 0)
        {
            var discountPlanFeeSched = DiscountPlans.GetPlan(discountPlanNum).FeeSchedNum;
            if (!listFeeScheds.Contains(discountPlanFeeSched)) listFeeScheds.Add(discountPlanFeeSched);
        }

        //ClinicNums========================================================================================================================
        var listClinicNumsOut = new List<long>(); //usually empty or one entry
        if (listClinicNums != null)
            for (var i = 0; i < listClinicNums.Count; i++)
                if (listClinicNums[i] != 0 && !listClinicNumsOut.Contains(listClinicNums[i]))
                    listClinicNumsOut.Add(listClinicNums[i]); //proc ClinicNums

        if (listFeeScheds.Count == 0 || listProcedureCodes.Count == 0) return new List<Fee>();
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND ClinicNum IN (0";
        if (listClinicNumsOut.Count != 0) command += "," + string.Join(",", listClinicNumsOut.Select(x => SOut.Long(x)));
        command += ")";
        if (listFeeScheds.Count != 0) command += " AND FeeSched IN(" + string.Join(",", listFeeScheds.Select(x => SOut.Long(x))) + ")";
        if (listCodeNumsOut.Count != 0) command += " AND CodeNum IN(" + string.Join(",", listCodeNumsOut.Select(x => SOut.Long(x))) + ")";
        command += " GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets fees that exactly match criteria. Default dateEffective check is DateTime.Today. Exact refers to
    ///     everything except the date.
    /// </summary>
    public static List<Fee> GetListExact(long feeSched, long clinicNum, long provNum, DateTime dateEffective = new())
    {
        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSched) + " "
                      + "AND ClinicNum=" + SOut.Long(clinicNum) + " AND ProvNum=" + SOut.Long(provNum) + " "
                      + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                      + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                      + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Overload gets fees that exactly match criteria with a clinic list. Default dateEffective check is
    ///     DateTime.Today. Exact refers to everything except the date.
    /// </summary>
    public static List<Fee> GetListExact(long feeSched, List<long> listClinicNums, long provNum, DateTime dateEffective = new())
    {
        if (listClinicNums.IsNullOrEmpty()) return new List<Fee>();

        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + SOut.Long(feeSched) + " "
                      + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") AND ProvNum=" + SOut.Long(provNum) + " "
                      + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                      + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                      + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Pass in new list and original list.  This will synch everything with Db.  Leave doCheckFeeSchedGroups set to true
    ///     unless calling from
    ///     FeeSchedGroups.
    /// </summary>
    public static bool SynchList(List<Fee> listFeesNew, List<Fee> listFeesDb, bool doCheckFeeSchedGroups = true)
    {
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.SyncGroupFees(listFeesNew, listFeesDb);
        return FeeCrud.Sync(listFeesNew, listFeesDb, Security.CurUser.UserNum);
    }

    ///<summary>Gets from Db.  Returns all fees associated to the procedure code passed in.</summary>
    public static List<Fee> GetFeesForCode(long codeNum, List<long> listClinicNums = null)
    {
        var command = "SELECT * FROM fee WHERE CodeNum=" + SOut.Long(codeNum) + " ";
        if (listClinicNums != null && listClinicNums.Count > 0) command += "AND ClinicNum IN(" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ")";
        //ordering was being done in the form. Easier to do it here.
        command += " ORDER BY ClinicNum,ProvNum";
        return FeeCrud.SelectMany(command);
    }

    ///<summary>Gets fees from Db, not including any prov or clinic overrides.</summary>
    public static List<Fee> GetFeesForCodeNoOverrides(long codeNum)
    {
        var command = "SELECT * FROM fee WHERE CodeNum=" + SOut.Long(codeNum) + " "
                      + "AND ClinicNum=0 AND ProvNum=0";
        return FeeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns an amount if a fee has been entered.  Prefers local clinic fees over HQ fees.  Otherwise returns -1.
    ///     Not usually used directly.  If you don't pass in a list of Fees, it will go directly to the Db.
    /// </summary>
    public static double GetAmount(long codeNum, long feeSched, long clinicNum, long provNum, List<Fee> listFees = null, DateTime dateEffective = new())
    {
        if (FeeScheds.GetIsHidden(feeSched)) return -1; //you cannot obtain fees for hidden fee schedules
        var fee = GetFee(codeNum, feeSched, clinicNum, provNum, listFees, dateEffective);
        if (fee == null) return -1;
        return fee.Amount;
    }

    /// <summary>
    ///     Almost the same as GetAmount.  But never returns -1;  Returns an amount if a fee has been entered.
    ///     Prefers local clinic fees over HQ fees. Returns 0 if code can't be found.
    ///     If you don't pass in a list of fees, it will go directly to the database.
    /// </summary>
    public static double GetAmount0(long codeNum, long feeSched, long clinicNum = 0, long provNum = 0, List<Fee> listFees = null, DateTime dateEffective = new())
    {
        var amountRet = GetAmount(codeNum, feeSched, clinicNum, provNum, listFees, dateEffective);
        if (amountRet == -1) return 0;
        return amountRet;
    }

    ///<summary>Gets the UCR fee for the provided procedure.</summary>
    public static double GetFeeUCR(Procedure procedure)
    {
        var provNum = procedure.ProvNum;
        if (provNum == 0) //if no prov set, then use practice default.
            provNum = PrefC.GetLong(PrefName.PracticeDefaultProv);
        var providerFirst = Providers.GetFirst(); //Used in order to preserve old behavior...  If this fails, then old code would have failed.
        var provider = Providers.GetFirstOrDefault(x => x.ProvNum == provNum) ?? providerFirst;
        //get the fee based on code and prov fee sched
        var ppoFee = GetAmount0(procedure.CodeNum, provider.FeeSched, procedure.ClinicNum, provNum);
        var ucrFee = procedure.ProcFee;
        if (ucrFee > ppoFee) return procedure.Quantity * ucrFee;

        return procedure.Quantity * ppoFee;
    }

    public static List<Fee> GetManyByFeeNum(List<long> listFeeNums)
    {
        var command = "SELECT * FROM fee WHERE FeeNum IN (" + string.Join(",", listFeeNums) + ")";
        return FeeCrud.SelectMany(command);
    }

    ///<summary>Gets one Fee object from the database using the primary key. Returns null if not found.</summary>
    public static Fee GetOneByFeeNum(long feeNum)
    {
        return FeeCrud.SelectOne(feeNum);
    }

    /// <summary>
    ///     Gets a list of Fees to show the user what is in the Fee table. This method is going to ignore DateEffective to
    ///     be able to retrieve duplicate fees. Will only filter the list if there are values passed in. Returns an empty list
    ///     if none were found.
    /// </summary>
    public static List<Fee> GetFeesForApi(int limit, int offset, long feeSched, long codeNum, long clinicNum, long provNum)
    {
        var command = "SELECT * from fee"
                      + " WHERE SecDateTEdit>=" + SOut.DateT(DateTime.MinValue);
        if (feeSched > 0) command += " AND FeeSched=" + SOut.Long(feeSched);
        if (codeNum > 0) command += " AND CodeNum=" + SOut.Long(codeNum);
        if (clinicNum > -1) command += " AND ClinicNum=" + SOut.Long(clinicNum);
        if (provNum > -1) command += " AND ProvNum=" + SOut.Long(provNum);
        command += " ORDER BY FeeNum"
                   + " LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return FeeCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Insert

    ///<summary>Set doCheckFeeSchedGroups to false when calling this method from FeeSchedGroups to prevent infinitely looping.</summary>
    public static long Insert(Fee fee, bool doCheckFeeSchedGroups = true)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        fee.SecUserNumEntry = Security.CurUser.UserNum;
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.UpsertGroupFees(new List<Fee> {fee});
        return FeeCrud.Insert(fee);
    }

    /// <summary>Bulk Insert.  Only set doCheckFeeSchedGroups to false from FeeSchedGroups.</summary>
    public static void InsertMany(List<Fee> listFees, bool doCheckFeeSchedGroups = true)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listFees.Count; i++) listFees[i].SecUserNumEntry = Security.CurUser.UserNum;
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.UpsertGroupFees(listFees);
        FeeCrud.InsertMany(listFees);
    }

    #endregion Insert

    #region Delete

    ///<summary>Only set doCheckFeeSchedGroups to false from FeeSchedGroups.</summary>
    public static void Delete(Fee fee, bool doCheckFeeSchedGroups = true)
    {
        //Even though we do not run a query in this method, there is a lot of back and forth and we should get to the server early to ensure less chattiness.

        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups)
            //If this fee isn't in a group don't bother checking.
            if (FeeSchedGroups.GetOneForFeeSchedAndClinic(fee.FeeSched, fee.ClinicNum) != null)
                FeeSchedGroups.DeleteGroupFees(new List<long> {fee.FeeNum});

        Delete(fee.FeeNum);
    }

    
    public static void Delete(long feeNum)
    {
        ClearFkey(feeNum);
        var command = "DELETE FROM fee WHERE FeeNum=" + feeNum;
        Db.NonQ(command);
    }

    ///<summary>Only set doCheckFeeSchedGroups to false from FeeSchedGroups.</summary>
    public static void DeleteMany(List<long> listFeeNums, bool doCheckFeeSchedGroups = true)
    {
        if (listFeeNums.Count == 0) return;

        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.DeleteGroupFees(listFeeNums);
        ClearFkey(listFeeNums);
        var command = "DELETE FROM fee WHERE FeeNum IN (" + string.Join(",", listFeeNums) + ")";
        Db.NonQ(command);
    }

    ///<summary>Deletes all fees for the supplied FeeSched that aren't for the HQ clinic.</summary>
    public static void DeleteNonHQFeesForSched(long feeSched)
    {
        var command = "SELECT FeeNum FROM fee WHERE FeeSched=" + SOut.Long(feeSched) + " AND ClinicNum!=0";
        var listFeeNums = Db.GetListLong(command);
        DeleteMany(listFeeNums);
    }

    /// <summary>
    ///     Deletes all fees with the exact specified FeeSchedule, ClinicNum, and ProvNum combination. If a DateEffective
    ///     is passed in, then it will only delete the fees less than the effective date along with the passed in filters. To
    ///     include future fees, pass in DateTime.MinValue.
    /// </summary>
    public static void DeleteFees(long feeSched, long clinicNum, long provNum, DateTime dateEffective = new())
    {
        var command = "DELETE FROM fee WHERE "
                      + "FeeSched=" + SOut.Long(feeSched) + " AND ClinicNum=" + SOut.Long(clinicNum) + " AND ProvNum=" + SOut.Long(provNum);
        if (dateEffective != DateTime.MinValue) command += " AND DateEffective<=" + SOut.Date(dateEffective);
        Db.NonQ(command);
    }

    #endregion Delete

    #region Misc Methods

    /// <summary>
    ///     Increases the fees passed in by percent.  Round should be the number of decimal places, either 0,1,or 2.
    ///     This method will not manipulate listFees passed in, although there is no particular reason for this choice.
    ///     Simply increases every fee passed in by the percent specified and returns the results.
    ///     The following parameters are ignored: feeSchedNum, clinicNum, and provNum.
    ///     If dateEffective was passed in, then it will update each new fee to that Date. Otherwise set to today.
    /// </summary>
    public static List<Fee> IncreaseNew(long feeSchedNum, int percent, int round, List<Fee> listFees, long clinicNum, long provNum, DateTime dateEffective = new())
    {
        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        var listFeesRetVal = new List<Fee>();
        for (var i = 0; i < listFees.Count; i++)
        {
            if (listFees[i].Amount == 0 || listFees[i].Amount == -1)
            {
                listFeesRetVal.Add(listFees[i].Copy());
                continue;
            }

            var newVal = listFees[i].Amount * (1 + (double) percent / 100);
            if (round > 0)
                newVal = Math.Round(newVal, round);
            else
                newVal = Math.Round(newVal, MidpointRounding.AwayFromZero);
            var feeNew = listFees[i].Copy();
            feeNew.Amount = newVal;
            feeNew.DateEffective = dateEffective;
            listFeesRetVal.Add(feeNew);
        }

        return listFeesRetVal;
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching feeNum as FKey and are related to Fee.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the Fee table type.
    /// </summary>
    public static void ClearFkey(long feeNum)
    {
        FeeCrud.ClearFkey(feeNum);
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching feeNums as FKey and are related to Fee.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the Fee table type.
    /// </summary>
    public static void ClearFkey(List<long> listFeeNums)
    {
        FeeCrud.ClearFkey(listFeeNums);
    }

    /// <summary>
    ///     Returns true if the feeAmtNewStr is an amount that does not match fee, either because fee is null and feeAmtNewStr
    ///     is not, or because
    ///     fee not null and the feeAmtNewStr is an equal amount, including a blank entry.
    /// </summary>
    public static bool IsFeeAmtEqual(Fee fee, string feeAmtNewStr)
    {
        //There is no fee in the database and the user didn't set a new fee value so there is no change.
        if (fee == null && feeAmtNewStr == "") return true;
        //Fee exists, but new amount is the same.
        if (fee != null && ((feeAmtNewStr != "" && fee.Amount == SIn.Double(feeAmtNewStr)) || (fee.Amount == -1 && feeAmtNewStr == ""))) return true;
        return false;
    }

    ///<summary>Returns true if any fees have DateEffective set.</summary>
    public static bool IsUsingEffectiveDate()
    {
        //Need this anymore?

        var command = "SELECT COUNT(*) FROM fee WHERE DateEffective>" + SOut.Date(DateTime.MinValue) + "";
        if (Db.GetLong(command) > 0) return true;
        return false;
    }

    ///<summary>Returns true if there is already a fee with the passed in DateEffective to prevent duplicates.</summary>
    public static bool CheckForDuplicate(Fee fee, DateTime dateEffective)
    {
        var command = "SELECT COUNT(*) FROM fee WHERE FeeNum!=" + SOut.Long(fee.FeeNum) + " AND FeeSched=" + SOut.Long(fee.FeeSched) + " AND CodeNum=" + SOut.Long(fee.CodeNum)
                      + " AND ClinicNum=" + SOut.Long(fee.ClinicNum) + " AND ProvNum=" + SOut.Long(fee.ProvNum) + " AND DateEffective=" + SOut.Date(dateEffective);
        if (Db.GetLong(command) > 0) return true;
        return false;
    }

    #endregion Misc Methods
}