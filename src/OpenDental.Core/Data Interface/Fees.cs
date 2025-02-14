using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;

namespace OpenDentBusiness;

public class Fees
{
    public static void Update(Fee fee, Fee feeOld = null, bool doCheckFeeSchedGroups = true)
    {
        //Check if this fee is associated to a FeeSchedGroup and update the rest of the group as needed.
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.UpsertGroupFees([fee]);
        if (feeOld != null)
            FeeCrud.Update(fee, feeOld);
        else
            FeeCrud.Update(fee);
    }
    
    public static List<Fee> GetByClinicNum(long clinicNum)
    {
        var command = "SELECT * FROM fee WHERE ClinicNum = " + clinicNum;
        return FeeCrud.SelectMany(command);
    }

    public static int GetCountByFeeSchedNum(long feeSchedNum)
    {
        var command = "SELECT COUNT(*) FROM fee WHERE FeeSched =" + feeSchedNum;
        return SIn.Int(Db.GetCount(command));
    }

    public static Fee GetFee(long codeNum, long feeSchedNum, long clinicNum, long provNum, List<Fee> listFees = null, DateTime dateEffective = new())
    {
        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        //use listFees if supplied regardless of the FeesUseCache pref since the fee cache is not really thread safe
        if (listFees != null) return GetFeeFromList(listFees, codeNum, feeSchedNum, clinicNum, provNum, dateEffective: dateEffective);
        return GetFeeFromDb(codeNum, feeSchedNum, clinicNum, provNum, dateEffective: dateEffective);
    }

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
				WHERE fee.CodeNum=" + codeNum + @"
				AND fee.FeeSched=" + feeSchedNum + @"
				AND fee.ClinicNum=" + clinicNum + @"
				AND fee.ProvNum=" + provNum + @"
				AND fee.DateEffective=" + SOut.Date(dateEffective);
        else
            command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSchedNum + " AND CodeNum=" + codeNum + " "
                      + "AND ClinicNum=" + clinicNum + " AND ProvNum=" + provNum + " "
                      + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                      + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                      + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        if (exactMatchForApi || FeeScheds.IsGlobal(feeSchedNum)) return FeeCrud.SelectOne(command);
        //Provider override
        command += " UNION ALL "
                   + "SELECT fee1.* FROM fee fee1 "
                   + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                   + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSchedNum + " AND CodeNum=" + codeNum + " "
                   + "AND ClinicNum=0 AND ProvNum=" + provNum + " "
                   + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        //Clinic override
        command += " UNION ALL "
                   + "SELECT fee1.* FROM fee fee1 "
                   + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                   + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSchedNum + " AND CodeNum=" + codeNum + " "
                   + "AND ClinicNum=" + clinicNum + " AND ProvNum=0 "
                   + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        //Unassigned clinic with no override
        command += " UNION ALL "
                   + "SELECT fee1.* FROM fee fee1 "
                   + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                   + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSchedNum + " AND CodeNum=" + codeNum + " "
                   + "AND ClinicNum=0 AND ProvNum=0 "
                   + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectOne(command);
        //Only the first result gets returned.
        //Using the UNION keeps it down to one query.
    }

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

    public static List<Fee> GetAllFeesForClinics(long codeNum, long feeSchedNum, long provNum, List<long> listClinicNums)
    {
        var command = "SELECT fee.* FROM fee "
                      + "WHERE fee.CodeNum=" + codeNum + " "
                      + "AND fee.FeeSched=" + feeSchedNum + " "
                      + "AND fee.ProvNum=" + provNum;
        if (!listClinicNums.IsNullOrEmpty()) command += " AND fee.ClinicNum IN(" + string.Join(",", listClinicNums) + ")";
        return FeeCrud.SelectMany(command);
    }

    public static List<Fee> GetListForScheds(long feeSched1, long clinicNum1 = 0, long provNum1 = 0, long feeSched2 = 0, long clinicNum2 = 0, long provNum2 = 0, long feeSched3 = 0, long clinicNum3 = 0, long provNum3 = 0, DateTime dateEffective = new())
    {
        return GetListForSchedsAndClinics(feeSched1, [clinicNum1], provNum1, feeSched2, [clinicNum2], provNum2, feeSched3, [clinicNum3], provNum3, dateEffective);
    }

    public static List<Fee> GetListForSchedsAndClinics(long feeSched1, List<long> listClinics1 = null, long provNum1 = 0, long feeSched2 = 0, List<long> listClinics2 = null, long provNum2 = 0, long feeSched3 = 0, List<long> listClinics3 = null, long provNum3 = 0, DateTime dateEffective = new())
    {
        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        var listClinicNums = new List<long> {0};
        if (!listClinics1.IsNullOrEmpty()) listClinicNums.AddRange(listClinics1);
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSched1 + " "
                      + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => x)) + ") AND ProvNum=" + provNum1 + " "
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
                       + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSched2 + " "
                       + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => x)) + ") AND ProvNum=" + provNum2 + " "
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
                       + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSched3 + " "
                       + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => x)) + ") AND ProvNum=" + provNum3 + " "
                       + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                       + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                       + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        }

        return FeeCrud.SelectMany(command);
    }

    public static List<Fee> GetListFromObjects(List<ProcedureCode> listProcedureCodes, List<string> listMedicalCodes, List<long> listProvNumsTreat, long patPriProv, long patSecProv, long patFeeSched, List<InsPlan> listInsPlans, List<long> listClinicNums, List<Appointment> listAppointments, List<SubstitutionLink> listSubstitutionLinks, long discountPlanNum, DateTime dateEffective = new())
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
        if (listProcedureCodes == null) return [];
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
        if (provFirst != null && provFirst.FeeScheduleId.HasValue && !listFeeScheds.Contains(provFirst.FeeScheduleId.Value)) listFeeScheds.Add(provFirst.FeeScheduleId.Value);
        //Add feesched for PracticeDefaultProv------------------------------------------------------------------------------------------------
        var provPracticeDefault = Providers.GetById(PrefC.GetLong(PrefName.PracticeDefaultProv));
        if (provPracticeDefault != null && provPracticeDefault.FeeScheduleId.HasValue && !listFeeScheds.Contains(provPracticeDefault.FeeScheduleId.Value)) listFeeScheds.Add(provPracticeDefault.FeeScheduleId.Value);
        //Add feescheds for all treating providers---------------------------------------------------------------------------------------------
        if (listProvNumsTreat != null)
            for (var i = 0; i < listProvNumsTreat.Count; i++)
            {
                var provTreat = Providers.GetById(listProvNumsTreat[i]);
                if (provTreat != null && provTreat.FeeScheduleId.HasValue && !listFeeScheds.Contains(provTreat.FeeScheduleId.Value)) listFeeScheds.Add(provTreat.FeeScheduleId.Value); //treating provs fee scheds
            }

        //Add feescheds for the patient's primary and secondary providers----------------------------------------------------------------------
        var providerPatPri = Providers.GetById(patPriProv);
        if (providerPatPri != null && providerPatPri.FeeScheduleId.HasValue && !listFeeScheds.Contains(providerPatPri.FeeScheduleId.Value)) listFeeScheds.Add(providerPatPri.FeeScheduleId.Value);
        var providerPatSec = Providers.GetById(patSecProv);
        if (providerPatSec != null && providerPatSec.FeeScheduleId.HasValue && !listFeeScheds.Contains(providerPatSec.FeeScheduleId.Value)) listFeeScheds.Add(providerPatSec.FeeScheduleId.Value);
        //Add feescheds for all procedurecode.ProvNumDefaults---------------------------------------------------------------------------------
        for (var i = 0; i < listProcedureCodes.Count; i++)
        {
            if (listProcedureCodes[i] == null) continue;
            var provNumDefault = listProcedureCodes[i].ProvNumDefault;
            if (provNumDefault == 0) continue;
            var provDefault = Providers.GetById(provNumDefault);
            if (provDefault != null && provDefault.FeeScheduleId.HasValue && !listFeeScheds.Contains(provDefault.FeeScheduleId.Value)) listFeeScheds.Add(provDefault.FeeScheduleId.Value);
        }

        //Add feescheds for appointment providers---------------------------------------------------------------------------------------------
        if (listAppointments != null)
            for (var i = 0; i < listAppointments.Count; i++)
            {
                var provAppt = Providers.GetById(listAppointments[i].ProvNum);
                if (provAppt != null && provAppt.FeeScheduleId.HasValue && !listFeeScheds.Contains(provAppt.FeeScheduleId.Value)) listFeeScheds.Add(provAppt.FeeScheduleId.Value);
                var provApptHyg = Providers.GetById(listAppointments[i].ProvHyg);
                if (provApptHyg != null && provApptHyg.FeeScheduleId.HasValue && !listFeeScheds.Contains(provApptHyg.FeeScheduleId.Value)) listFeeScheds.Add(provApptHyg.FeeScheduleId.Value);
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

        if (listFeeScheds.Count == 0 || listProcedureCodes.Count == 0) return [];
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND ClinicNum IN (0";
        if (listClinicNumsOut.Count != 0) command += "," + string.Join(",", listClinicNumsOut.Select(x => x));
        command += ")";
        if (listFeeScheds.Count != 0) command += " AND FeeSched IN(" + string.Join(",", listFeeScheds.Select(x => x)) + ")";
        if (listCodeNumsOut.Count != 0) command += " AND CodeNum IN(" + string.Join(",", listCodeNumsOut.Select(x => x)) + ")";
        command += " GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                   + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                   + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectMany(command);
    }

    public static List<Fee> GetListExact(long feeSched, long clinicNum, long provNum, DateTime dateEffective = new())
    {
        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSched + " "
                      + "AND ClinicNum=" + clinicNum + " AND ProvNum=" + provNum + " "
                      + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                      + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                      + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectMany(command);
    }

    public static List<Fee> GetListExact(long feeSched, List<long> listClinicNums, long provNum, DateTime dateEffective = new())
    {
        if (listClinicNums.IsNullOrEmpty()) return [];

        if (dateEffective == DateTime.MinValue) dateEffective = DateTime.Today;
        var command = "SELECT fee1.* FROM fee fee1 "
                      + "INNER JOIN (SELECT FeeSched, CodeNum, ClinicNum, ProvNum, MAX(DateEffective+INTERVAL 100 YEAR)-INTERVAL 100 YEAR MaxDateEffective FROM fee "
                      + "WHERE DateEffective<=" + SOut.Date(dateEffective) + " AND FeeSched=" + feeSched + " "
                      + "AND ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => x)) + ") AND ProvNum=" + provNum + " "
                      + "GROUP BY FeeSched, CodeNum, ClinicNum, ProvNum) fee2 "
                      + "ON fee1.CodeNum=fee2.CodeNum AND fee1.FeeSched=fee2.FeeSched AND fee1.ClinicNum=fee2.ClinicNum AND fee1.ProvNum=fee2.ProvNum "
                      + "WHERE fee1.DateEffective=fee2.MaxDateEffective";
        return FeeCrud.SelectMany(command);
    }

    public static void SynchList(List<Fee> listFeesNew, List<Fee> listFeesDb, bool doCheckFeeSchedGroups = true)
    {
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.SyncGroupFees(listFeesNew, listFeesDb);
        FeeCrud.Sync(listFeesNew, listFeesDb, Security.CurUser.UserNum);
    }

    public static List<Fee> GetFeesForCode(long codeNum, List<long> listClinicNums = null)
    {
        var command = "SELECT * FROM fee WHERE CodeNum=" + codeNum + " ";
        if (listClinicNums != null && listClinicNums.Count > 0) command += "AND ClinicNum IN(" + string.Join(",", listClinicNums.Select(x => x)) + ")";
        //ordering was being done in the form. Easier to do it here.
        command += " ORDER BY ClinicNum,ProvNum";
        return FeeCrud.SelectMany(command);
    }

    public static List<Fee> GetFeesForCodeNoOverrides(long codeNum)
    {
        var command = "SELECT * FROM fee WHERE CodeNum=" + codeNum + " "
                      + "AND ClinicNum=0 AND ProvNum=0";
        return FeeCrud.SelectMany(command);
    }

    public static double GetAmount(long codeNum, long feeSched, long clinicNum, long provNum, List<Fee> listFees = null, DateTime dateEffective = new())
    {
        if (FeeScheds.GetIsHidden(feeSched)) return -1; //you cannot obtain fees for hidden fee schedules
        var fee = GetFee(codeNum, feeSched, clinicNum, provNum, listFees, dateEffective);
        if (fee == null) return -1;
        return fee.Amount;
    }

    public static double GetAmount0(long codeNum, long feeSched, long clinicNum = 0, long provNum = 0, List<Fee> listFees = null, DateTime dateEffective = new())
    {
        var amountRet = GetAmount(codeNum, feeSched, clinicNum, provNum, listFees, dateEffective);
        if (amountRet == -1) return 0;
        return amountRet;
    }

    public static List<Fee> GetManyByFeeNum(List<long> listFeeNums)
    {
        var command = "SELECT * FROM fee WHERE FeeNum IN (" + string.Join(",", listFeeNums) + ")";
        return FeeCrud.SelectMany(command);
    }

    public static void Insert(Fee fee, bool doCheckFeeSchedGroups = true)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        fee.SecUserNumEntry = Security.CurUser.UserNum;
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.UpsertGroupFees([fee]);
        FeeCrud.Insert(fee);
    }

    public static void InsertMany(List<Fee> listFees, bool doCheckFeeSchedGroups = true)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        for (var i = 0; i < listFees.Count; i++) listFees[i].SecUserNumEntry = Security.CurUser.UserNum;
        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.UpsertGroupFees(listFees);
        FeeCrud.InsertMany(listFees);
    }
    
    public static void Delete(Fee fee, bool doCheckFeeSchedGroups = true)
    {
        //Even though we do not run a query in this method, there is a lot of back and forth and we should get to the server early to ensure less chattiness.

        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups)
            //If this fee isn't in a group don't bother checking.
            if (FeeSchedGroups.GetOneForFeeSchedAndClinic(fee.FeeSched, fee.ClinicNum) != null)
                FeeSchedGroups.DeleteGroupFees([fee.FeeNum]);

        Delete(fee.FeeNum);
    }
    
    public static void Delete(long feeNum)
    {
        ClearFkey(feeNum);
        var command = "DELETE FROM fee WHERE FeeNum=" + feeNum;
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFeeNums, bool doCheckFeeSchedGroups = true)
    {
        if (listFeeNums.Count == 0) return;

        if (PrefC.GetBool(PrefName.ShowFeeSchedGroups) && doCheckFeeSchedGroups) FeeSchedGroups.DeleteGroupFees(listFeeNums);
        ClearFkey(listFeeNums);
        var command = "DELETE FROM fee WHERE FeeNum IN (" + string.Join(",", listFeeNums) + ")";
        Db.NonQ(command);
    }

    public static void DeleteFees(long feeSched, long clinicNum, long provNum, DateTime dateEffective = new())
    {
        var command = "DELETE FROM fee WHERE "
                      + "FeeSched=" + feeSched + " AND ClinicNum=" + clinicNum + " AND ProvNum=" + provNum;
        if (dateEffective != DateTime.MinValue) command += " AND DateEffective<=" + SOut.Date(dateEffective);
        Db.NonQ(command);
    }
    
    public static List<Fee> IncreaseNew(int percent, int round, List<Fee> listFees, DateTime dateEffective = new())
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
    
    public static void ClearFkey(long feeNum)
    {
        FeeCrud.ClearFkey(feeNum);
    }

    public static void ClearFkey(List<long> listFeeNums)
    {
        FeeCrud.ClearFkey(listFeeNums);
    }

    public static bool IsFeeAmtEqual(Fee fee, string feeAmtNewStr)
    {
        //There is no fee in the database and the user didn't set a new fee value so there is no change.
        if (fee == null && feeAmtNewStr == "") return true;
        //Fee exists, but new amount is the same.
        if (fee != null && ((feeAmtNewStr != "" && fee.Amount == SIn.Double(feeAmtNewStr)) || (fee.Amount == -1 && feeAmtNewStr == ""))) return true;
        return false;
    }

    public static bool IsUsingEffectiveDate()
    {
        //Need this anymore?

        var command = "SELECT COUNT(*) FROM fee WHERE DateEffective>" + SOut.Date(DateTime.MinValue) + "";
        if (Db.GetLong(command) > 0) return true;
        return false;
    }

    public static bool CheckForDuplicate(Fee fee, DateTime dateEffective)
    {
        var command = "SELECT COUNT(*) FROM fee WHERE FeeNum!=" + fee.FeeNum + " AND FeeSched=" + fee.FeeSched + " AND CodeNum=" + fee.CodeNum
                      + " AND ClinicNum=" + fee.ClinicNum + " AND ProvNum=" + fee.ProvNum + " AND DateEffective=" + SOut.Date(dateEffective);
        if (Db.GetLong(command) > 0) return true;
        return false;
    }
}