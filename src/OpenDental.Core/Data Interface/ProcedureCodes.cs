using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using Newtonsoft.Json;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProcedureCodes
{
    public const string GroupProcCode = "~GRP~";

    ///<summary>222 is the FeeSchedNum for the 'DO NOT EDIT' fee schedule in the customers database.</summary>
    public const long DoNotEditFeeSchedNum = 222;

    ///<summary>Hard-coded dictionary of eService codes and their corresponding ProcCode within the database at HQ.</summary>
    public static readonly List<EServiceCodeProcCode> ListEServiceProcCodes = new()
    {
        new EServiceCodeProcCode {EServiceCode = eServiceCode.Bundle, ProcCode = "042"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.ConfirmationOwn, ProcCode = "045"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.ConfirmationRequest, ProcCode = "040"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.EClipboard, ProcCode = "047"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.MobileWeb, ProcCode = "027"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.PatientPortal, ProcCode = "033"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.ResellerSoftwareOnly, ProcCode = "043"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.SoftwareOnly, ProcCode = "030"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.IntegratedTexting, ProcCode = "038"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.IntegratedTextingOwn, ProcCode = "046"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.IntegratedTextingUsage, ProcCode = "039"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.WebForms, ProcCode = "036"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.WebSched, ProcCode = "037"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.WebSchedNewPatAppt, ProcCode = "041"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.WebSchedASAP, ProcCode = "044"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.EmailMassUsage, ProcCode = "050"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.EmailSecureUsage, ProcCode = "051"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.EmailSecureAccess, ProcCode = "052"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.ODTouch, ProcCode = "055"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.ODTSurplus, ProcCode = "056"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.OCR, ProcCode = "057"},
        new EServiceCodeProcCode {EServiceCode = eServiceCode.FHIR, ProcCode = "048"}
    };

    private static string GetProcCodeForCodeNum(List<long> listCodeNums, string procCode, string procCodeCA)
    {
        if (listCodeNums.Count == 0)
        {
            if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) return procCodeCA;
            return procCode;
        }

        var codeNum = listCodeNums[0];
        var procedureCode = GetFirstOrDefaultFromList(x => x.CodeNum == codeNum); //this will always work because we already convered D code into CodeNum earlier
        var strProcCode = procedureCode.ProcCode;
        return strProcCode;
    }

    public static string GetProcCodeForEService(eServiceCode eService)
    {
        return ListEServiceProcCodes.FirstOrDefault(x => x.EServiceCode == eService).ProcCode;
    }

    ///<summary>If procCode is not an eService than returns Undefined. Otherwise returns eServiceCode linked to procCode.</summary>
    public static eServiceCode GetEServiceForProcCode(string procCode)
    {
        var ret = ListEServiceProcCodes.FirstOrDefault(x => x.ProcCode == procCode);
        if (ret == null) //No guarantee that the procCode passed into this method is an eService.
            return eServiceCode.Undefined;
        return ret.EServiceCode;
    }

    ///<summary>ResellerService.Fee2 is not supported for Integrated Texting, eConfirmations, and MassEmail.</summary>
    public static bool DoAllowResellerFee2Edit(string procCode)
    {
        return false;
    }

    ///<summary>Combine ProcedureCode and Fee into single class using HQ's DoNotEditFeeSchedNum. Used by signup portal.</summary>
    public static List<EServiceFee> GetAllEServiceFees()
    {
        var listESerivceProcCodes = GetAllEServiceProcCodes();
        var listProcCodes = GetWhere(x => listESerivceProcCodes.Contains(x.ProcCode));
        var listFees = Fees.GetListForScheds(DoNotEditFeeSchedNum);
        return listProcCodes.Select(procCode =>
        {
            var fee = listFees.FirstOrDefault(fee => fee.CodeNum == procCode.CodeNum);
            if (fee == null) throw new Exception("OD customers DO NOT EDIT FeeSchedule does not include a single valid Fee for ProcCode: " + procCode.ProcCode);
            return new EServiceFee {ESvcProcCode = procCode, ESvcFee = fee};
        }).ToList();
    }

    public static EServiceFee GetEServiceFeeByEService(eServiceCode eService)
    {
        var procCode = GetProcCodeForEService(eService);
        return GetAllEServiceFees().FirstOrDefault(x => x.ESvcProcCode.ProcCode == procCode);
    }

    public static List<string> GetAllEServiceProcCodes()
    {
        return ListEServiceProcCodes.Select(x => x.ProcCode).ToList();
    }

    public static List<ProcedureCode> GetChangedSince(DateTime dateTimeChangedSince)
    {
        var command = "SELECT * FROM procedurecode WHERE DateTStamp > " + SOut.DateT(dateTimeChangedSince);
        return ProcedureCodeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of procedurecodes attached to the given claim. Currently only used by the EDS attachment bridge
    ///     to fill in data required by the EDS API.
    /// </summary>
    public static List<ProcedureCode> GetForClaim(long claimNum)
    {
        var command = "SELECT pc.* " +
                      "FROM claimproc c " +
                      "INNER JOIN procedurelog p ON c.ProcNum=p.ProcNum " +
                      "INNER JOIN procedurecode pc ON p.CodeNum=pc.CodeNum " +
                      "WHERE c.ClaimNum=" + SOut.Long(claimNum);
        return ProcedureCodeCrud.SelectMany(command);
    }

    
    public static long Insert(ProcedureCode procedureCode)
    {
        //must have already checked procCode for nonduplicate.
        return ProcedureCodeCrud.Insert(procedureCode);
    }

    
    public static void InsertMany(List<ProcedureCode> listProcedureCodes)
    {
        if (listProcedureCodes.IsNullOrEmpty()) return;

        //must have already checked procCode for nonduplicate.
        ProcedureCodeCrud.InsertMany(listProcedureCodes);
    }

    
    public static void Update(ProcedureCode procedureCode)
    {
        ProcedureCodeCrud.Update(procedureCode);
    }

    
    public static bool Update(ProcedureCode procCode, ProcedureCode procCodeOld)
    {
        return ProcedureCodeCrud.Update(procCode, procCodeOld);
    }

    ///<summary>Counts all procedure codes, including hidden codes.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM procedurecode";
        return SIn.Long(Db.GetCount(command));
    }

    /// <summary>
    ///     Returns the ProcedureCode for the supplied procCode such as such as D####.
    ///     If no ProcedureCode is found, returns a new ProcedureCode.
    /// </summary>
    public static ProcedureCode GetProcCode(string myCode)
    {
        var procedureCode = new ProcedureCode();
        if (IsValidCode(myCode)) procedureCode = _procedureCodeCache.GetOne(myCode);
        return procedureCode;
    }

    /// <summary>
    ///     Returns a list of ProcedureCodes for the supplied procCodes such as such as D####.  Returns an empty list if
    ///     no matches.
    /// </summary>
    public static List<ProcedureCode> GetProcCodes(List<string> listCodes)
    {
        if (listCodes == null || listCodes.Count < 1) return new List<ProcedureCode>();
        return _procedureCodeCache.GetWhereForKey(x => listCodes.Contains(x));
    }

    /// <summary>
    ///     The new way of getting a procCode. Uses the primary key instead of string code. Returns new instance if not found.
    ///     Pass in a list of all codes to save from locking the cache if you are going to call this method repeatedly.
    /// </summary>
    public static ProcedureCode GetProcCode(long codeNum, List<ProcedureCode> listProcedureCodes = null)
    {
        if (codeNum == 0) return new ProcedureCode();
        if (listProcedureCodes == null) return _procedureCodeCache.GetFirstOrDefaultFromList(x => x.CodeNum == codeNum) ?? new ProcedureCode();
        for (var i = 0; i < listProcedureCodes.Count; i++)
            if (listProcedureCodes[i].CodeNum == codeNum)
                return listProcedureCodes[i];

        return new ProcedureCode();
    }

    ///<summary>Gets code from db to avoid having to constantly refresh in FormProcCodes</summary>
    public static ProcedureCode GetProcCodeFromDb(long codeNum)
    {
        var procedureCode = ProcedureCodeCrud.SelectOne(codeNum);
        if (procedureCode == null)
            //We clasically return an empty procedurecode object here instead of null.
            return new ProcedureCode();
        return procedureCode;
    }

    ///<summary>Supply the human readable proc code such as D####. If not found, returns 0.</summary>
    public static long GetCodeNum(string myCode)
    {
        if (IsValidCode(myCode)) return _procedureCodeCache.GetOne(myCode).CodeNum;
        return 0;
    }

    /// <summary>
    ///     Returns true if any procedure codes with the passed in CodeNums are hidden. Otherwise, false. Uses cache only,
    ///     no call to db.
    /// </summary>
    public static bool AreAnyProcCodesHidden(params long[] arrayCodeNums)
    {
        return GetProcCodesInHiddenCats(arrayCodeNums).Count > 0;
    }

    /// <summary>
    ///     Returns all hidden procedure codes with the passed in CodeNums are hidden. Otherwise, false. Uses cache only,
    ///     no call to db.
    /// </summary>
    public static List<string> GetProcCodesInHiddenCats(params long[] arrayCodeNums)
    {
        return arrayCodeNums
            .Select(x => GetFirstOrDefault(y => y.CodeNum == x && Defs.GetHidden(DefCat.ProcCodeCats, y.ProcCat))?.ProcCode)
            .Where(x => x != null) //GetFirstOrDefault will return null if code exists but not hidden, therefore still adds to the returned list with Select().
            .ToList();
    }

    /// <summary>
    ///     Gets a list of ProcedureCode for a given treatment area,including codes in hidden categories if
    ///     isHiddenIncluded=true.
    /// </summary>
    public static List<ProcedureCode> GetProcCodesByTreatmentArea(bool isHiddenIncluded, params TreatmentArea[] arrayTreatmentAreas)
    {
        return _procedureCodeCache.GetWhere(x => x.TreatArea.In(arrayTreatmentAreas)
                                                 && (isHiddenIncluded || !Defs.GetHidden(DefCat.ProcCodeCats, x.ProcCat)));
    }

    /// <summary>
    ///     If a substitute exists for the given proc code, then it will give the CodeNum of that code.
    ///     Otherwise, it will return the codeNum for the given procCode.
    /// </summary>
    public static long GetSubstituteCodeNum(string procCode, string toothNum, long planNum, List<SubstitutionLink> listSubLinks = null)
    {
        long codeNum = 0;
        if (string.IsNullOrEmpty(procCode)) return codeNum;
        ODException.SwallowAnyException(() =>
        {
            var procedureCode = _procedureCodeCache.GetOne(procCode);
            codeNum = procedureCode.CodeNum;
            listSubLinks = listSubLinks ?? SubstitutionLinks.GetAllForPlans(planNum);
            listSubLinks = listSubLinks.Where(x => x.PlanNum == planNum).ToList();
            //We allow multiple substitution codes for procedures now so we have to check to implement any new hierarchy
            var subLink = SubstitutionLinks.GetSubLinkByHierarchy(procedureCode, toothNum, listSubLinks);
            //Check procedure code level substitution if no insurance substitution override.
            if ((subLink == null || string.IsNullOrEmpty(subLink.SubstitutionCode) || !IsValidCode(subLink.SubstitutionCode))
                && !string.IsNullOrEmpty(procedureCode.SubstitutionCode) && IsValidCode(procedureCode.SubstitutionCode) && listSubLinks.All(x => x.CodeNum != codeNum))
                //Swallow any following exceptions because the old code would first check and make sure the key was in the dictionary.
                ODException.SwallowAnyException(() => { codeNum = GetSubstitutionCodeNumHelper(codeNum, procedureCode.SubstOnlyIf, procedureCode.SubstitutionCode, toothNum); });
            //Check insplan substituationlink(override) for the procedure.
            else if (subLink != null && !string.IsNullOrEmpty(subLink.SubstitutionCode) && IsValidCode(subLink.SubstitutionCode))
                ODException.SwallowAnyException(() => { codeNum = GetSubstitutionCodeNumHelper(codeNum, subLink.SubstOnlyIf, subLink.SubstitutionCode, toothNum); });
            if (codeNum == 0) //If somehow we got a bad value up above we'll revert back to the procedure code
                codeNum = procedureCode.CodeNum;
        });
        return codeNum;
    }

    
    private static long GetSubstitutionCodeNumHelper(long defaultCodeNum, SubstitutionCondition substitutionCondition, string substitutionCode, string toothNum)
    {
        var codeNum = defaultCodeNum;
        if (substitutionCondition == SubstitutionCondition.Never) return codeNum;

        if (substitutionCondition == SubstitutionCondition.Always)
            codeNum = _procedureCodeCache.GetOne(substitutionCode).CodeNum;
        else if (substitutionCondition == SubstitutionCondition.Molar && Tooth.IsMolar(toothNum))
            codeNum = _procedureCodeCache.GetOne(substitutionCode).CodeNum;
        else if (substitutionCondition == SubstitutionCondition.SecondMolar && Tooth.IsSecondMolar(toothNum))
            codeNum = _procedureCodeCache.GetOne(substitutionCode).CodeNum;
        else if (substitutionCondition == SubstitutionCondition.Posterior && Tooth.IsPosterior(toothNum)) codeNum = _procedureCodeCache.GetOne(substitutionCode).CodeNum;
        return codeNum;
    }

    /// <summary>
    ///     Gets the proc codes (D codes) as a comma separated list from the preference and finds the corresponding
    ///     CodeNums.
    /// </summary>
    public static List<long> GetCodeNumsForPref(PrefName prefName)
    {
        return GetCodeNumsForProcCodes(PrefC.GetString(prefName));
    }

    /// <summary>
    ///     Gets the proc codes (D codes) as a comma separated list from the CodeGroup cache and finds the corresponding
    ///     CodeNums.
    /// </summary>
    public static List<long> GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed codeGroupFixed)
    {
        var codeGroup = CodeGroups.GetOneForCodeGroupFixed(codeGroupFixed);
        if (codeGroup == null) return new List<long>();
        return GetCodeNumsForProcCodes(codeGroup.ProcCodes);
    }

    /// <summary>
    ///     Returns a list of CodeNums that have a ProcCode that starts with any of the comma separated ProcCodes passed in.
    ///     E.g. Insurance benefit logic invokes this method and must match procedure codes that start the same.
    /// </summary>
    public static List<long> GetCodeNumsForProcCodes(string procCodes)
    {
        var listCodes = procCodes.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        return GetWhereFromList(x => listCodes.Contains(x.ProcCode)).Select(x => x.CodeNum).ToList();
    }

    ///<summary>Gets the CodeNums for the passed in InsHist preference.</summary>
    public static List<long> GetCodeNumsForInsHistPref(PrefName prefName)
    {
        var listCodeNums = GetCodeNumsForPref(prefName);
        switch (prefName)
        {
            case PrefName.InsHistBWCodes:
                listCodeNums.AddRange(GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.BW));
                break;
            case PrefName.InsHistExamCodes:
                listCodeNums.AddRange(GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.Exam));
                break;
            case PrefName.InsHistPanoCodes:
                listCodeNums.AddRange(GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.PanoFMX));
                break;
            case PrefName.InsHistPerioLLCodes:
            case PrefName.InsHistPerioLRCodes:
            case PrefName.InsHistPerioULCodes:
            case PrefName.InsHistPerioURCodes:
                listCodeNums.AddRange(GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.SRP));
                break;
            case PrefName.InsHistPerioMaintCodes:
                listCodeNums.AddRange(GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.Perio));
                break;
            case PrefName.InsHistProphyCodes:
                listCodeNums.AddRange(GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.Prophy));
                break;
        }

        return listCodeNums;
    }

    ///<summary>Returns a list of CodeNums that are associated with any limitation benefits passed in.</summary>
    public static List<long> GetCodeNumsForAllLimitations(List<Benefit> listBenefits, InsPlan insPlan, long patPlanNum = 0, ProcedureCode procedureCode = null)
    {
        var listCodeNums = new List<long>();
        var listBenefitsLimitations = listBenefits.FindAll(x => Benefits.IsFrequencyLimitation(x));
        var benefitBW = listBenefitsLimitations.Find(x => Benefits.IsBitewingFrequency(x, patPlanNum));
        var benefitPano = listBenefitsLimitations.Find(x => Benefits.IsPanoFrequency(x, patPlanNum));
        var benefitExam = listBenefitsLimitations.Find(x => Benefits.IsExamFrequency(x, patPlanNum));
        var benefitProphy = listBenefitsLimitations.Find(x => Benefits.IsProphyFrequency(x, patPlanNum));
        var benefitSRP = listBenefitsLimitations.Find(x => Benefits.IsSRPFrequency(x, patPlanNum));
        var benefitFullDebridement = listBenefitsLimitations.Find(x => Benefits.IsFullDebridementFrequency(x, patPlanNum));
        var benefitPerio = listBenefitsLimitations.Find(x => Benefits.IsPerioMaintFrequency(x, patPlanNum));
        //Find any relevant CodeNums from associated InsHist preferences.
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitBW, PrefName.InsHistBWCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitPano, PrefName.InsHistPanoCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitExam, PrefName.InsHistExamCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitProphy, PrefName.InsHistProphyCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitSRP, PrefName.InsHistPerioLLCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitSRP, PrefName.InsHistPerioLRCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitSRP, PrefName.InsHistPerioULCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitSRP, PrefName.InsHistPerioURCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitFullDebridement, PrefName.InsHistDebridementCodes, procedureCode));
        listCodeNums.AddRange(GetInsHistCodeNumsForBenefit(benefitPerio, PrefName.InsHistPerioMaintCodes, procedureCode));
        //Loop through every benefit and add all of the CodeNums from any custom code groups that are associated with the benefit.
        for (var i = 0; i < listBenefitsLimitations.Count; i++)
        {
            if (listBenefitsLimitations[i].CodeNum > 0)
            {
                listCodeNums.Add(listBenefitsLimitations[i].CodeNum);
                continue;
            }

            if (listBenefitsLimitations[i].CodeGroupNum > 0)
            {
                var codeGroup = CodeGroups.GetOne(listBenefitsLimitations[i].CodeGroupNum);
                var listProcedureCodes = GetWhere(x => IsCodeInList(x.ProcCode, codeGroup.ProcCodes));
                var listCodeNumsFromCodeGroup = listProcedureCodes.Select(x => x.CodeNum).ToList();
                listCodeNums.AddRange(listCodeNumsFromCodeGroup);
            }
        }

        return listCodeNums;
    }

    /// <summary>
    ///     Returns the list of CodeNums related to the InsHist preference if the benefit passed in is NOT null and the
    ///     procedureCode passed in is either null or is associated with the preference. Otherwise, returns an empty list.
    /// </summary>
    private static List<long> GetInsHistCodeNumsForBenefit(Benefit benefit, PrefName prefNameInsHist, ProcedureCode procedureCode)
    {
        if (benefit == null) return new List<long>();
        var listCodeNums = GetCodeNumsForPref(prefNameInsHist);
        if (procedureCode == null || listCodeNums.Contains(procedureCode.CodeNum)) return listCodeNums; //The proc is not included or is part of the group
        return new List<long>();
    }

    ///<summary>Returns true if this code is marked as BypassIfZero and if this procFee is zero.</summary>
    public static bool CanBypassLockDate(long codeNum, double procFee)
    {
        var isBypassGlobalLock = false;
        var procedureCode = GetFirstOrDefaultFromList(x => x.CodeNum == codeNum);
        if (procedureCode != null && procedureCode.BypassGlobalLock == BypassLockStatus.BypassIfZero && CompareDouble.IsZero(procFee)) isBypassGlobalLock = true;
        return isBypassGlobalLock;
    }

    ///<summary>Returns true if any code is marked as BypassIfZero.</summary>
    public static bool DoAnyBypassLockDate()
    {
        var procedureCode = GetFirstOrDefaultFromList(x => x.BypassGlobalLock == BypassLockStatus.BypassIfZero);
        return procedureCode != null;
    }

    ///<summary>Pass in an optional list of procedure codes in order to use it instead of the cache.</summary>
    public static string GetStringProcCode(long codeNum, List<ProcedureCode> listProcedureCodes = null, bool doThrowIfMissing = true)
    {
        if (codeNum == 0) return "";
        //throw new ApplicationException("CodeNum cannot be zero.");
        ProcedureCode procedureCode;
        if (listProcedureCodes == null)
            procedureCode = GetFirstOrDefaultFromList(x => x.CodeNum == codeNum);
        else
            procedureCode = listProcedureCodes.FirstOrDefault(x => x.CodeNum == codeNum);
        if (procedureCode != null) return procedureCode.ProcCode;
        if (doThrowIfMissing) throw new ApplicationException("Missing codenum"); //Do not change this text without considering the two places where this string is used in logic.
        return "";
    }

    
    public static bool IsValidCode(string myCode)
    {
        if (string.IsNullOrEmpty(myCode)) return false;
        return _procedureCodeCache.GetContainsKey(myCode);
    }

    /// <summary>Throws exceptions. Validates ProcCodes and ToothNums.</summary>
    public static void ValidateProcedureCodeEntry(string[] stringArrayProcCodes, bool doAllowToothNum = false)
    {
        if (stringArrayProcCodes.IsNullOrEmpty()) throw new Exception(Lans.g("FormDefEdit", "Definition contains no valid code(s)"));
        for (var i = 0; i < stringArrayProcCodes.Length; i++)
        {
            //Examples: D0220#7,D0220#10,D0220#25
            //D0220,D0220,D0220  
            //D0220,D0220#10,D0220#25
            //Validate ToothNum
            string[] stringArrayProcCodeAndToothNum = null;
            if (doAllowToothNum)
            {
                stringArrayProcCodeAndToothNum = stringArrayProcCodes[i].Split('#'); //0: ProcCode, 1: ToothNum (if present)
                if (stringArrayProcCodeAndToothNum.Length > 2) throw new Exception(Lans.g("FormDefEdit", "Definition contains multiple tooth numbers. Only 1 default tooth number per procedure code is allowed."));
                if (stringArrayProcCodeAndToothNum.Length == 2)
                    if (!Tooth.IsValidEntry(stringArrayProcCodeAndToothNum[1]))
                        throw new Exception(Lans.g("FormDefEdit", "Definition contains invalid tooth number: ") + stringArrayProcCodeAndToothNum[1]);
            }

            //Validate ProcCode
            string stringProcCode;
            if (doAllowToothNum)
                stringProcCode = stringArrayProcCodeAndToothNum[0];
            else
                stringProcCode = stringArrayProcCodes[i];
            var procedureCode = GetProcCode(stringProcCode);
            if (procedureCode.CodeNum == 0)
            {
                //Now check to see if the trimmed version of the code does not exist either.
                procedureCode = GetProcCode(stringProcCode.Trim());
                if (procedureCode.CodeNum == 0) throw new Exception(Lans.g("FormDefEdit", "Definition contains invalid procedure code: ") + stringProcCode);
            }

            if (stringArrayProcCodeAndToothNum != null && stringArrayProcCodeAndToothNum.Length == 2 && procedureCode.TreatArea != TreatmentArea.Tooth) throw new Exception(Lans.g("FormDefEdit", "Definition contains treatment area mismatch. If adding a tooth number, the treatment area for the procedure code must be tooth."));
        }
    }

    ///<summary>Grouped by Category.  Used only in FormRpProcCodes.</summary>
    public static ProcedureCode[] GetProcList(Def[][] arrayDefs = null)
    {
        var listProcedureCodes = new List<ProcedureCode>();
        Def[] array = null;
        if (arrayDefs == null)
            array = Defs.GetDefsForCategory(DefCat.ProcCodeCats, true).ToArray();
        else
            array = arrayDefs[(int) DefCat.ProcCodeCats];
        var listProcedureCodesDeepCopy = GetListDeep();
        for (var j = 0; j < arrayDefs[(int) DefCat.ProcCodeCats].Length; j++)
        for (var k = 0; k < listProcedureCodesDeepCopy.Count; k++)
            if (arrayDefs[(int) DefCat.ProcCodeCats][j].DefNum == listProcedureCodesDeepCopy[k].ProcCat)
                listProcedureCodes.Add(listProcedureCodesDeepCopy[k].Copy());

        return listProcedureCodes.ToArray();
    }

    /// <summary>
    ///     Gets a list of procedure codes directly from the database.  If categories.length==0, then we will get for all
    ///     categories.  Categories are defnums.  FeeScheds are, for now, defnums.
    /// </summary>
    public static DataTable GetProcTable(string abbr, string desc, string code, List<long> listCategories, long feeSchedNum,
        long feeSched2Num, long feeSched3Num)
    {
        string whereCat;
        if (listCategories.Count == 0)
        {
            whereCat = "1";
        }
        else
        {
            whereCat = "(";
            for (var i = 0; i < listCategories.Count; i++)
            {
                if (i > 0) whereCat += " OR ";
                whereCat += "ProcCat=" + SOut.Long(listCategories[i]);
            }

            whereCat += ")";
        }

        var feeSched = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == feeSchedNum);
        var feeSched2 = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == feeSched2Num);
        var feeSched3 = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == feeSched3Num);
        //Query changed to be compatible with both MySQL and Oracle (not tested).
        var command = "SELECT ProcCat,Descript,AbbrDesc,procedurecode.ProcCode,";
        if (feeSched == null)
        {
            command += "-1 FeeAmt1,";
        }
        else
        {
            command += "CASE ";
            if (!feeSched.IsGlobal && Clinics.ClinicNum != 0) //Use local clinic fee if there's one present
                command += "WHEN (feeclinic1.Amount IS NOT NULL) THEN feeclinic1.Amount ";
            command += "WHEN (feehq1.Amount IS NOT NULL) THEN feehq1.Amount ELSE -1 END FeeAmt1,";
        }

        if (feeSched2 == null)
        {
            command += "-1 FeeAmt2,";
        }
        else
        {
            command += "CASE ";
            if (!feeSched2.IsGlobal && Clinics.ClinicNum != 0) //Use local clinic fee if there's one present
                command += "WHEN (feeclinic2.Amount IS NOT NULL) THEN feeclinic2.Amount ";
            command += "WHEN (feehq2.Amount IS NOT NULL) THEN feehq2.Amount ELSE -1 END FeeAmt2,";
        }

        if (feeSched3 == null)
        {
            command += "-1 FeeAmt3,";
        }
        else
        {
            command += "CASE ";
            if (!feeSched3.IsGlobal && Clinics.ClinicNum != 0) //Use local clinic fee if there's one present
                command += "WHEN (feeclinic3.Amount IS NOT NULL) THEN feeclinic3.Amount ";
            command += "WHEN (feehq3.Amount IS NOT NULL) THEN feehq3.Amount ELSE -1 END FeeAmt3,";
        }

        command += "procedurecode.CodeNum ";
        if (feeSched != null && !feeSched.IsGlobal && Clinics.ClinicNum != 0) command += ",CASE WHEN (feeclinic1.Amount IS NOT NULL) THEN 1 ELSE 0 END IsClinic1 ";
        if (feeSched2 != null && !feeSched2.IsGlobal && Clinics.ClinicNum != 0) command += ",CASE WHEN (feeclinic2.Amount IS NOT NULL) THEN 1 ELSE 0 END IsClinic2 ";
        if (feeSched3 != null && !feeSched3.IsGlobal && Clinics.ClinicNum != 0) command += ",CASE WHEN (feeclinic3.Amount IS NOT NULL) THEN 1 ELSE 0 END IsClinic3 ";
        command += "FROM procedurecode ";
        if (feeSched != null)
        {
            if (!feeSched.IsGlobal && Clinics.ClinicNum != 0) //Get local clinic fee if there's one present
                command += "LEFT JOIN fee feeclinic1 ON feeclinic1.CodeNum=procedurecode.CodeNum AND feeclinic1.FeeSched=" + SOut.Long(feeSched.FeeSchedNum)
                                                                                                                           + " AND feeclinic1.ClinicNum=" + SOut.Long(Clinics.ClinicNum) + " ";
            //Get the hq clinic fee if there's one present
            command += "LEFT JOIN fee feehq1 ON feehq1.CodeNum=procedurecode.CodeNum AND feehq1.FeeSched=" + SOut.Long(feeSched.FeeSchedNum)
                                                                                                           + " AND feehq1.ClinicNum=0 ";
        }

        if (feeSched2 != null)
        {
            if (!feeSched2.IsGlobal && Clinics.ClinicNum != 0) //Get local clinic fee if there's one present
                command += "LEFT JOIN fee feeclinic2 ON feeclinic2.CodeNum=procedurecode.CodeNum AND feeclinic2.FeeSched=" + SOut.Long(feeSched2.FeeSchedNum)
                                                                                                                           + " AND feeclinic2.ClinicNum=" + SOut.Long(Clinics.ClinicNum) + " ";
            //Get the hq clinic fee if there's one present
            command += "LEFT JOIN fee feehq2 ON feehq2.CodeNum=procedurecode.CodeNum AND feehq2.FeeSched=" + SOut.Long(feeSched2.FeeSchedNum)
                                                                                                           + " AND feehq2.ClinicNum=0 ";
        }

        if (feeSched3 != null)
        {
            if (!feeSched3.IsGlobal && Clinics.ClinicNum != 0) //Get local clinic fee if there's one present
                command += "LEFT JOIN fee feeclinic3 ON feeclinic3.CodeNum=procedurecode.CodeNum AND feeclinic3.FeeSched=" + SOut.Long(feeSched3.FeeSchedNum)
                                                                                                                           + " AND feeClinic3.ClinicNum=" + SOut.Long(Clinics.ClinicNum) + " ";
            //Get the hq clinic fee if there's one present
            command += "LEFT JOIN fee feehq3 ON feehq3.CodeNum=procedurecode.CodeNum AND feehq3.FeeSched=" + SOut.Long(feeSched3.FeeSchedNum)
                                                                                                           + " AND feehq3.ClinicNum=0 ";
        }

        command += "LEFT JOIN definition ON definition.DefNum=procedurecode.ProcCat "
                   + "WHERE " + whereCat + " "
                   + "AND Descript LIKE '%" + SOut.String(desc) + "%' "
                   + "AND AbbrDesc LIKE '%" + SOut.String(abbr) + "%' "
                   + "AND procedurecode.ProcCode LIKE '%" + SOut.String(code) + "%' "
                   + "ORDER BY definition.ItemOrder,procedurecode.ProcCode";
        return DataCore.GetTable(command);
    }

    ///<summary>Returns the LaymanTerm for the supplied codeNum, or the description if none present.</summary>
    public static string GetLaymanTerm(long codeNum)
    {
        var laymanTerm = "";
        var procedureCode = GetFirstOrDefaultFromList(x => x.CodeNum == codeNum);
        if (procedureCode != null) laymanTerm = procedureCode.LaymanTerm != "" ? procedureCode.LaymanTerm : procedureCode.Descript;
        return laymanTerm;
    }

    /// <summary>
    ///     Used to check whether codes starting with T exist and are in a visible category.  If so, it moves them to the
    ///     Obsolete category.  If the T code has never been used, then it deletes it.
    /// </summary>
    public static void TcodesClear()
    {
        //first delete any unused T codes
        var command = @"SELECT CodeNum,ProcCode FROM procedurecode
				WHERE CodeNum NOT IN(SELECT CodeNum FROM procedurelog)
				AND CodeNum NOT IN(SELECT CodeNum FROM autocodeitem)
				AND CodeNum NOT IN(SELECT CodeNum FROM procbuttonitem)
				AND CodeNum NOT IN(SELECT CodeNum FROM recalltrigger)
				AND CodeNum NOT IN(SELECT CodeNum FROM benefit)
				AND ProcCode NOT IN(SELECT CodeValue FROM encounter WHERE CodeSystem='CDT')
				AND ProcCode LIKE 'T%'";
        var table = DataCore.GetTable(command);
        var listCodeNums = new List<long>();
        var listRecallCodes = RecallTypes.GetDeepCopy()
            .SelectMany(x => x.Procedures.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
            .ToList();
        for (var i = 0; i < table.Rows.Count; i++)
            if (!listRecallCodes.Contains(SIn.String(table.Rows[i]["ProcCode"].ToString())))
                //The ProcCode is not attached to a recall type.
                listCodeNums.Add(SIn.Long(table.Rows[i]["CodeNum"].ToString()));

        if (listCodeNums.Count > 0)
        {
            ClearFkey(listCodeNums); //Zero securitylog FKey column for rows to be deleted.
            command = "SELECT FeeNum FROM fee WHERE CodeNum IN(" + string.Join(",", listCodeNums) + ")";
            var listFeeNums = Db.GetListLong(command);
            Fees.DeleteMany(listFeeNums);
            command = "DELETE FROM proccodenote WHERE CodeNum IN(" + string.Join(",", listCodeNums) + ")";
            Db.NonQ(command);
            command = "DELETE FROM procedurecode WHERE CodeNum IN(" + string.Join(",", listCodeNums) + ")";
            Db.NonQ(command);
        }

        //then, move any other T codes to obsolete category
        command = @"SELECT DISTINCT ProcCat FROM procedurecode,definition 
				WHERE procedurecode.ProcCode LIKE 'T%'
				AND definition.IsHidden=0
				AND procedurecode.ProcCat=definition.DefNum";
        table = DataCore.GetTable(command);
        var catNum = Defs.GetByExactName(DefCat.ProcCodeCats, "Obsolete"); //check to make sure an Obsolete category exists.
        Def def;
        if (catNum != 0)
        {
            //if a category exists with that name
            def = Defs.GetDef(DefCat.ProcCodeCats, catNum);
            if (!def.IsHidden)
            {
                def.IsHidden = true;
                Defs.Update(def);
                Defs.RefreshCache();
                var logText = Lans.g("Defintions", "Definition edited:") + " " + def.ItemName + " "
                              + Lans.g("Defintions", "with category:") + " " + def.Category.GetDescription();
                SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logText);
            }
        }

        if (catNum == 0)
        {
            var listDefs = Defs.GetDefsForCategory(DefCat.ProcCodeCats);
            def = new Def();
            def.Category = DefCat.ProcCodeCats;
            def.ItemName = "Obsolete";
            def.ItemOrder = listDefs.Count;
            def.IsHidden = true;
            Defs.Insert(def);
            Defs.RefreshCache();
            var logText = Lans.g("Defintions", "Definition created:") + " " + def.ItemName + " "
                          + Lans.g("Defintions", "with category:") + " " + def.Category.GetDescription();
            SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logText);
            catNum = def.DefNum;
        }

        for (var i = 0; i < table.Rows.Count; i++)
        {
            command = "UPDATE procedurecode SET ProcCat=" + SOut.Long(catNum)
                                                          + " WHERE ProcCat=" + table.Rows[i][0]
                                                          + " AND procedurecode.ProcCode LIKE 'T%'";
            Db.NonQ(command);
        }

        //finally, set Never Used category to be hidden.  This isn't really part of clearing Tcodes, but is required
        //because many customers won't have that category hidden
        catNum = Defs.GetByExactName(DefCat.ProcCodeCats, "Never Used");
        if (catNum != 0)
        {
            //if a category exists with that name
            def = Defs.GetDef(DefCat.ProcCodeCats, catNum);
            if (!def.IsHidden)
            {
                def.IsHidden = true;
                Defs.Update(def);
                Defs.RefreshCache();
                var logText = Lans.g("Defintions", "Definition edited:") + " " + def.ItemName + " "
                              + Lans.g("Defintions", "with category:") + " " + def.Category.GetDescription();
                SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logText);
            }
        }
    }

    public static void ResetApptProcsQuickAdd()
    {
        var command = "DELETE FROM definition WHERE Category=3";
        Db.NonQ(command);
        var array = new[]
        {
            "CompEx-4BW-Pano-Pro-Flo", "D0150,D0274,D0330,D1110,D1208",
            "CompEx-2BW-Pano-ChPro-Flo", "D0150,D0272,D0330,D1120,D1208",
            "PerEx-4BW-Pro-Flo", "D0120,D0274,D1110,D1208",
            "LimEx-PA", "D0140,D0220",
            "PerEx-4BW-Pro-Flo", "D0120,D0274,D1110,D1208",
            "PerEx-2BW-ChildPro-Flo", "D0120,D0272,D1120,D1208",
            "Comp Exam", "D0150",
            "Per Exam", "D0120",
            "Lim Exam", "D0140",
            "1 PA", "D0220",
            "2BW", "D0272",
            "4BW", "D0274",
            "Pano", "D0330",
            "Pro Adult", "D1110",
            "Fluor", "D1208",
            "Pro Child", "D1120",
            "PostOp", "N4101",
            "DentAdj", "N4102",
            "Consult", "D9310"
        };
        Def def;
        string[] arrayCodes;
        bool allvalid;
        var itemorder = 0;
        for (var i = 0; i < array.Length; i += 2)
        {
            //first, test all procedures for valid
            arrayCodes = array[i + 1].Split(',');
            allvalid = true;
            for (var c = 0; c < arrayCodes.Length; c++)
                if (!IsValidCode(arrayCodes[c]))
                    allvalid = false;

            if (!allvalid) continue;
            def = new Def();
            def.Category = DefCat.ApptProcsQuickAdd;
            def.ItemOrder = itemorder;
            def.ItemName = array[i];
            def.ItemValue = array[i + 1];
            Defs.Insert(def);
            var logText = Lans.g("Defintions", "Definition created:") + " " + def.ItemName + " "
                          + Lans.g("Defintions", "with category:") + " " + def.Category.GetDescription();
            SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logText);
            itemorder++;
        }
    }

    public static void ResetApptProcsQuickAddCA()
    {
        var command = "DELETE FROM definition WHERE Category=3";
        Db.NonQ(command);
        var array = new[]
        {
            "Exam Recall", "01202",
            "Exam Spec", "01204",
            "Consult", "01703",
            "Occlusal Adjustment", "16511",
            "Exam Comp Perm", "01103",
            "Exam Comp Mixed", "01102",
            "Exam Comp Pri", "01101",
            "Exam NP Limited", "01201",
            "Exam Emerg", "01205",
            "Polishing 1 unit", "11101",
            "Polishing .5 unit", "11107",
            "Scaling 1 unit", "11111",
            "Scaling 2 units", "11112",
            "Scaling 3 units", "11113",
            "Scaling .5 unit", "11117",
            "Fluoride Varnish", "12113",
            "Fluoride Foam", "12112",
            "Sealant", "13401",
            "Sealant add'l", "13409",
            "Pan", "02601",
            "CT Scan", "07043",
            "1 PA", "02111",
            "2 PA's", "02112",
            "3 PA's", "02113",
            "4 PA's", "02114",
            "1 BW", "02141",
            "2 BW's", "02142",
            "4 BW's", "02144",
            "OHI", "13211",
            "Post-op Check", "79601"
        };
        Def def;
        var itemorder = 0;
        for (var i = 0; i < array.Length; i += 2)
        {
            if (!IsValidCode(array[i + 1])) //first, test all procedures for valid
                continue;
            def = new Def
            {
                Category = DefCat.ApptProcsQuickAdd,
                ItemOrder = itemorder++,
                ItemName = array[i],
                ItemValue = array[i + 1]
            };
            Defs.Insert(def);
            var logText = Lans.g("Defintions", "Definition created:") + " " + def.ItemName + " "
                          + Lans.g("Defintions", "with category:") + " " + def.Category.GetDescription();
            SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logText);
        }
    }

    /// <summary>
    ///     Resets the descriptions and abbreviations for all ADA codes to the official wording.  Descriptions are
    ///     required by the license, we set the abbreviations.
    /// </summary>
    public static int ResetADAdescriptionsAndAbbrs()
    {
        return ResetADAdescriptionsAndAbbrs(GetADAcodes());
    }

    /// <summary>
    ///     Gets a list of procedure codes as defined by the ADA, plus procedure code settings for codes which are
    ///     commonly used and default settings for those which are not commonly used.
    /// </summary>
    public static List<ProcedureCode> GetADAcodes()
    {
        var listProcedureCodes = new List<ProcedureCode>();
        //Split raw data into non-empty lines of text in the file.
        var arrayAdaCodeLines = Class1.GetADAcodes().Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
        var arrayProcedureCodeSettingLines = Class1.GetProcedureCodeSettings().Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
        string[] arrayAdaDictionaryCode;
        //load our codes into a hashtable
        var hashTable = new Hashtable(); //key=adacode, value=entire row string
        for (var i = 0; i < arrayProcedureCodeSettingLines.Length; i++) hashTable.Add(arrayProcedureCodeSettingLines[i].Substring(0, 5), arrayProcedureCodeSettingLines[i]);
        for (var i = 0; i < arrayAdaCodeLines.Length; i++)
        {
            arrayAdaDictionaryCode = arrayAdaCodeLines[i].Split('\t');
            //Skips invalid text lines in the file.
            if (arrayAdaDictionaryCode[0].Length != 5) continue;
            var procedureCode = new ProcedureCode();
            procedureCode.ProcCode = SIn.String(arrayAdaDictionaryCode[0]);
            procedureCode.Descript = SIn.String(arrayAdaDictionaryCode[2].TrimEnd('"').TrimStart('"'));
            procedureCode.ProcCatDescript = "Never Used";
            procedureCode.ProcTime = "/X/";
            //look for a matching code in our list
            if (hashTable.ContainsKey(procedureCode.ProcCode))
            {
                var columns = ((string) hashTable[procedureCode.ProcCode]).Split('\t');
                procedureCode.TreatArea = (TreatmentArea) SIn.Int(columns[1]);
                //code.SetRecall=PIn.PBool(columns[2]);
                procedureCode.NoBillIns = SIn.Bool(columns[3]);
                procedureCode.IsProsth = SIn.Bool(columns[4]);
                procedureCode.IsHygiene = SIn.Bool(columns[5]);
                procedureCode.PaintType = (ToothPaintingType) SIn.Int(columns[6]);
                procedureCode.ProcCatDescript = SIn.String(columns[7]);
                procedureCode.ProcTime = SIn.String(columns[8]);
                procedureCode.AbbrDesc = SIn.String(columns[9]);
                procedureCode.IsRadiology = SIn.Bool(columns[10]);
                if (procedureCode.ProcCode == "D2391")
                {
                    procedureCode.SubstitutionCode = "D2140";
                    procedureCode.SubstOnlyIf = SubstitutionCondition.Always;
                }
                else if (procedureCode.ProcCode == "D2392")
                {
                    procedureCode.SubstitutionCode = "D2150";
                    procedureCode.SubstOnlyIf = SubstitutionCondition.Always;
                }
                else if (procedureCode.ProcCode == "D2393")
                {
                    procedureCode.SubstitutionCode = "D2160";
                    procedureCode.SubstOnlyIf = SubstitutionCondition.Always;
                }
                else if (procedureCode.ProcCode == "D2394")
                {
                    procedureCode.SubstitutionCode = "D2161";
                    procedureCode.SubstOnlyIf = SubstitutionCondition.Always;
                }
            }

            listProcedureCodes.Add(procedureCode);
        }

        return listProcedureCodes;
    }

    /// <summary>
    ///     Updates the treatment areas for all procedure codes to the ADA recommended treatment areas. Returns the count
    ///     of procedure codes updated
    /// </summary>
    public static int SetTreatAreasForADACodes()
    {
        var listProcedureCodes = GetAllCodes(); //Ordered by D-code.
        var arrayProcedureCodeTreatAreasLines = Class1.GetProcedureCodeTreatAreas() //~850 rows
            .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
        string[] arrayProcCodeTreatArea;
        var countProcCodesUpdated = 0;
        for (var i = 0; i < arrayProcedureCodeTreatAreasLines.Length; i++)
        {
            //Expects each row in the text file to be of the format D0120\t0\t0
            arrayProcCodeTreatArea = arrayProcedureCodeTreatAreasLines[i].Split("\t", StringSplitOptions.RemoveEmptyEntries);
            if (arrayProcCodeTreatArea.Length < 3) continue;
            var procedureCode = listProcedureCodes.Find(x => x.ProcCode == arrayProcCodeTreatArea[0]);
            if (procedureCode == null) continue;
            if (!Enum.TryParse(arrayProcCodeTreatArea[1], out TreatmentArea treatmentArea)
                || procedureCode.TreatArea == treatmentArea)
                continue;
            var procedureCodeOld = procedureCode.Copy();
            procedureCode.TreatArea = treatmentArea;
            procedureCode.AreaAlsoToothRange = SIn.Bool(arrayProcCodeTreatArea[2]);
            if (Update(procedureCode, procedureCodeOld)) countProcCodesUpdated += 1;
        }

        return countProcCodesUpdated;
    }

    /// <summary>
    ///     Resets the descriptions and abbreviations for all ADA codes to the official wording.  Descriptions are
    ///     required by the license, we set the abbreviations.
    /// </summary>
    public static int ResetADAdescriptionsAndAbbrs(List<ProcedureCode> listProcedureCodes)
    {
        ProcedureCode procedureCode;
        var count = 0;
        for (var i = 0; i < listProcedureCodes.Count; i++)
        {
            if (!IsValidCode(listProcedureCodes[i].ProcCode)) //If this code is not in this database
                continue;
            procedureCode = GetProcCode(listProcedureCodes[i].ProcCode);
            var datePrevious = procedureCode.DateTStamp;
            var isDescriptMatch = procedureCode.Descript == listProcedureCodes[i].Descript;
            var isDbProcAbbrDescBlank = string.IsNullOrWhiteSpace(procedureCode.AbbrDesc);
            if (!isDescriptMatch || isDbProcAbbrDescBlank) //Only increments one time for each code if there are changes necessary.
                count++;
            if (!isDescriptMatch)
            {
                //Update description.
                var oldDescript = procedureCode.Descript;
                procedureCode.Descript = listProcedureCodes[i].Descript;
                Update(procedureCode);
                SecurityLogs.MakeLogEntry(EnumPermType.ProcCodeEdit, 0, "Code " + procedureCode.ProcCode + " changed from '" + oldDescript + "' to '" + procedureCode.Descript + "' by D-Codes Tool."
                    , procedureCode.CodeNum, datePrevious);
            }

            if (isDbProcAbbrDescBlank)
            {
                //Update abbreviation if current code.AbbrDesc in db is blank.
                var oldAbbrDesc = procedureCode.AbbrDesc;
                procedureCode.AbbrDesc = listProcedureCodes[i].AbbrDesc;
                Update(procedureCode);
                SecurityLogs.MakeLogEntry(EnumPermType.ProcCodeEdit, 0, $"Code {procedureCode.ProcCode} changed from '{oldAbbrDesc}' to '{procedureCode.AbbrDesc}' by D-Codes Tool."
                    , procedureCode.CodeNum, datePrevious);
            }
        }

        return count;
        //don't forget to refresh procedurecodes.
    }

    /// <summary>
    ///     Returns true if the ADA missed appointment procedure code is in the database to identify how to track missed
    ///     appointments.
    /// </summary>
    public static bool HasMissedCode()
    {
        return _procedureCodeCache.GetContainsKey("D9986");
    }

    /// <summary>
    ///     Returns true if the ADA cancelled appointment procedure code is in the database to identify how to track
    ///     cancelled appointments.
    /// </summary>
    public static bool HasCancelledCode()
    {
        return _procedureCodeCache.GetContainsKey("D9987");
    }

    /// <summary>
    ///     Takes a procedure code and a comma-delimited list of procedure codes. Considers ranges. Returns true if the
    ///     provided code is in the list.
    /// </summary>
    public static bool IsCodeInList(string procCode, string codeListString)
    {
        var listCodesFromString = codeListString.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        for (var i = 0; i < listCodesFromString.Count; i++)
        {
            if (listCodesFromString[i] == procCode) return true;
            if (!listCodesFromString[i].Contains('-')) continue;
            var listProcCodesFromRange = listCodesFromString[i].Split("-", StringSplitOptions.RemoveEmptyEntries).ToList();
            var procCodeFirst = listProcCodesFromRange.First().ToLower();
            var procCodeLast = listProcCodesFromRange.Last().ToLower();
            var isCodeInRange = procCode.ToLower().CompareTo(procCodeFirst) >= 0 && procCode.ToLower().CompareTo(procCodeLast) <= 0;
            //Example: if range is D12300-D12399, then D12380a would of course be in range.
            //But if checking for D12399a, that would be false. This is the only edge case that might be unexpected, but we will not address it.
            if (isCodeInRange) return true;
        }

        return false;
    }

    /// <summary>
    ///     Gets a list of procedureCodes from the cache using a comma-delimited list of ProcCodes.
    ///     Returns a new list is the passed in string is empty.
    /// </summary>
    public static List<ProcedureCode> GetFromCommaDelimitedList(string codeStr)
    {
        var listProcedureCodes = new List<ProcedureCode>();
        if (string.IsNullOrEmpty(codeStr)) return listProcedureCodes;
        var listProcCodes = codeStr.Split(',').ToList();
        for (var i = 0; i < listProcCodes.Count; i++) listProcedureCodes.Add(GetProcCode(listProcCodes[i]));
        return listProcedureCodes;
    }

    public static List<ProcedureCode> GetAllCodes()
    {
        var command = "SELECT * from procedurecode ORDER BY ProcCode";
        return ProcedureCodeCrud.SelectMany(command);
    }

    ///<summary>Gets all procedurecodes from the database ordered by cat then code.  This is how the cache is ordered.</summary>
    public static List<ProcedureCode> GetAllCodesOrderedCatCode()
    {
        var command = "SELECT * FROM procedurecode ORDER BY ProcCat,ProcCode";
        return ProcedureCodeCrud.SelectMany(command);
    }

    ///<summary>Gets one procedurecode from db. Returns null if not found.</summary>
    public static ProcedureCode GetOneProcCodeForApi(long codeNum)
    {
        var command = "SELECT * FROM procedurecode "
                      + "WHERE CodeNum = '" + SOut.Long(codeNum) + "'";
        return ProcedureCodeCrud.SelectOne(command);
    }

    ///<summary>Gets procedurecodes from the database optionally filtered by DateTStamp.</summary>
    public static List<ProcedureCode> GetProcCodesForApi(int limit, int offset, DateTime dateTStamp)
    {
        var command = "SELECT * FROM procedurecode ";
        if (dateTStamp > DateTime.MinValue) command += "WHERE DateTStamp >= " + SOut.DateT(dateTStamp) + " ";
        command += "ORDER BY CodeNum " //Ensure order for limit and offset. Ordered by ProcCode in 23.3.24 and older.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return ProcedureCodeCrud.SelectMany(command);
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching codeNum as FKey and are related to
    ///     ProcedureCode.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the ProcedureCode table type.
    /// </summary>
    public static void ClearFkey(long codeNum)
    {
        ProcedureCodeCrud.ClearFkey(codeNum);
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching codeNums as FKey and are related to
    ///     ProcedureCode.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the ProcedureCode table type.
    /// </summary>
    public static void ClearFkey(List<long> listCodeNums)
    {
        ProcedureCodeCrud.ClearFkey(listCodeNums);
    }

    ///<summary>Gets all procedurecodes for the specified codenums.</summary>
    public static List<ProcedureCode> GetCodesForCodeNums(List<long> listCodeNums)
    {
        return _procedureCodeCache.GetWhere(x => listCodeNums.Contains(x.CodeNum));
    }

    /// <summary>
    ///     Gets all ortho procedure code nums from the OrthoPlacementProcsList preference if any are present.
    ///     Otherwise gets all procedure codes that start with D8
    /// </summary>
    public static List<long> GetOrthoBandingCodeNums()
    {
        var strListOrthoNums = PrefC.GetString(PrefName.OrthoPlacementProcsList);
        var listCodeNums = new List<long>();
        if (strListOrthoNums != "") return strListOrthoNums.Split(',').ToList().Select(x => SIn.Long(x)).ToList();

        return GetWhereFromList(x => x.ProcCode.ToUpper().StartsWith("D8")).Select(x => x.CodeNum).ToList();
    }

    public class EServiceCodeProcCode
    {
        public eServiceCode EServiceCode;
        public string ProcCode;
    }

    ///<summary>HQ only. Used as output for GetAllEServiceFees.</summary>
    public class EServiceFee
    {
        public Fee ESvcFee;
        public ProcedureCode ESvcProcCode;
    }

    #region Get Methods

    public static List<ProcedureCode> GetMandibularCodes()
    {
        var listMandibularCodes = new List<ProcedureCode>();
        ODException.SwallowAnyException(() => { listMandibularCodes = JsonConvert.DeserializeObject<List<ProcedureCode>>(Class1.GetMandibularCodes()); });
        //The list of mandibular proc codes can be null, due to DeserializeObject interrupting an empty string as null
        return listMandibularCodes ?? new List<ProcedureCode>();
    }

    public static List<ProcedureCode> GetMaxillaryCodes()
    {
        var listMaxillaryCodes = new List<ProcedureCode>();
        ODException.SwallowAnyException(() => { listMaxillaryCodes = JsonConvert.DeserializeObject<List<ProcedureCode>>(Class1.GetMaxillaryCodes()); });
        //The list of maxillary proc codes can be null, due to DeserializeObject interrupting an empty string as null
        return listMaxillaryCodes ?? new List<ProcedureCode>();
    }

    #endregion

    #region Cache Pattern

    ///<summary>Utilizes the NonPkAbs version of CacheDict because it uses ProcCode as the Key instead of the PK CodeNum.</summary>
    private class ProcedureCodeCache : CacheDictNonPkAbs<ProcedureCode, string, ProcedureCode>
    {
        protected override List<ProcedureCode> GetCacheFromDb()
        {
            var command = "SELECT * FROM procedurecode ORDER BY ProcCat,ProcCode";
            return ProcedureCodeCrud.SelectMany(command);
        }

        protected override List<ProcedureCode> TableToList(DataTable dataTable)
        {
            return ProcedureCodeCrud.TableToList(dataTable);
        }

        protected override ProcedureCode Copy(ProcedureCode item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<string, ProcedureCode> dict)
        {
            return ProcedureCodeCrud.ListToTable(dict.Values.ToList(), "ProcedureCode");
        }

        protected override void FillCacheIfNeeded()
        {
            ProcedureCodes.GetTableFromCache(false);
        }

        protected override string GetDictKey(ProcedureCode item)
        {
            return item.ProcCode;
        }

        protected override ProcedureCode GetDictValue(ProcedureCode item)
        {
            return item;
        }

        protected override ProcedureCode CopyValue(ProcedureCode procedureCode)
        {
            return procedureCode.Copy();
        }

        protected override DataTable ToDataTable(List<ProcedureCode> items)
        {
            return ProcedureCodeCrud.ListToTable(items);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProcedureCodeCache _procedureCodeCache = new();

    public static List<ProcedureCode> GetListDeep()
    {
        return _procedureCodeCache.GetDeepCopyList();
    }

    public static int GetCount(bool isShort = false)
    {
        return _procedureCodeCache.GetCount(isShort);
    }

    public static ProcedureCode GetOne(string procCode)
    {
        return _procedureCodeCache.GetOne(procCode);
    }

    public static ProcedureCode GetFirstOrDefault(Func<ProcedureCode, bool> match, bool isShort = false)
    {
        return _procedureCodeCache.GetFirstOrDefault(match, isShort);
    }

    public static ProcedureCode GetFirstOrDefaultFromList(Func<ProcedureCode, bool> match, bool isShort = false)
    {
        return _procedureCodeCache.GetFirstOrDefaultFromList(match, isShort);
    }

    public static List<ProcedureCode> GetWhere(Func<ProcedureCode, bool> match, bool isShort = false)
    {
        return _procedureCodeCache.GetWhere(match, isShort);
    }

    public static List<ProcedureCode> GetWhereFromList(Predicate<ProcedureCode> match, bool isShort = false)
    {
        return _procedureCodeCache.GetWhereFromList(match, isShort);
    }

    public static bool GetContainsKey(string procCode)
    {
        return _procedureCodeCache.GetContainsKey(procCode);
    }

    #region InsHist Preference Procedures

    ///<summary>Returns the first procedure code for the InsHist preference passed in.</summary>
    public static ProcedureCode GetByInsHistPref(PrefName prefName)
    {
        return GetProcCode(GetCodeNumsForPref(prefName).FirstOrDefault());
    }

    #endregion InsHist Preference Procedures

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _procedureCodeCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _procedureCodeCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _procedureCodeCache.ClearCache();
    }

    #endregion Cache Pattern
}