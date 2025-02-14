using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;

namespace OpenDentBusiness;

public class ProcedureCodes
{
    public const string GroupProcCode = "~GRP~";

    public static List<ProcedureCode> GetForClaim(long claimNum)
    {
        var command = "SELECT pc.* " +
                      "FROM claimproc c " +
                      "INNER JOIN procedurelog p ON c.ProcNum=p.ProcNum " +
                      "INNER JOIN procedurecode pc ON p.CodeNum=pc.CodeNum " +
                      "WHERE c.ClaimNum=" + claimNum;
        return ProcedureCodeCrud.SelectMany(command);
    }

    public static void Insert(ProcedureCode procedureCode)
    {
        //must have already checked procCode for nonduplicate.
        ProcedureCodeCrud.Insert(procedureCode);
    }

    public static void Update(ProcedureCode procedureCode)
    {
        ProcedureCodeCrud.Update(procedureCode);
    }

    public static bool Update(ProcedureCode procCode, ProcedureCode procCodeOld)
    {
        return ProcedureCodeCrud.Update(procCode, procCodeOld);
    }

    public static ProcedureCode GetProcCode(string myCode)
    {
        var procedureCode = new ProcedureCode();
        if (IsValidCode(myCode)) procedureCode = Cache.GetOne(myCode);
        return procedureCode;
    }

    public static List<ProcedureCode> GetProcCodes(List<string> listCodes)
    {
        if (listCodes == null || listCodes.Count < 1) return [];
        return Cache.GetWhereForKey(x => listCodes.Contains(x));
    }

    public static ProcedureCode GetProcCode(long codeNum, List<ProcedureCode> listProcedureCodes = null)
    {
        if (codeNum == 0) return new ProcedureCode();
        if (listProcedureCodes == null) return Cache.GetFirstOrDefaultFromList(x => x.CodeNum == codeNum) ?? new ProcedureCode();
        for (var i = 0; i < listProcedureCodes.Count; i++)
            if (listProcedureCodes[i].CodeNum == codeNum)
                return listProcedureCodes[i];

        return new ProcedureCode();
    }

    public static ProcedureCode GetProcCodeFromDb(long codeNum)
    {
        var procedureCode = ProcedureCodeCrud.SelectOne(codeNum);
        if (procedureCode == null)
            //We clasically return an empty procedurecode object here instead of null.
            return new ProcedureCode();
        return procedureCode;
    }

    public static long GetCodeNum(string myCode)
    {
        if (IsValidCode(myCode)) return Cache.GetOne(myCode).CodeNum;
        return 0;
    }

    public static bool AreAnyProcCodesHidden(params long[] arrayCodeNums)
    {
        return GetProcCodesInHiddenCats(arrayCodeNums).Count > 0;
    }

    public static List<string> GetProcCodesInHiddenCats(params long[] arrayCodeNums)
    {
        return arrayCodeNums
            .Select(x => GetFirstOrDefault(y => y.CodeNum == x && Defs.GetHidden(DefCat.ProcCodeCats, y.ProcCat))?.ProcCode)
            .Where(x => x != null) //GetFirstOrDefault will return null if code exists but not hidden, therefore still adds to the returned list with Select().
            .ToList();
    }

    public static List<ProcedureCode> GetProcCodesByTreatmentArea(bool isHiddenIncluded, params TreatmentArea[] arrayTreatmentAreas)
    {
        return Cache.GetWhere(x => x.TreatArea.In(arrayTreatmentAreas)
                                   && (isHiddenIncluded || !Defs.GetHidden(DefCat.ProcCodeCats, x.ProcCat)));
    }

    public static long GetSubstituteCodeNum(string procCode, string toothNum, long planNum, List<SubstitutionLink> listSubLinks = null)
    {
        long codeNum = 0;
        if (string.IsNullOrEmpty(procCode)) return codeNum;
        ODException.SwallowAnyException(() =>
        {
            var procedureCode = Cache.GetOne(procCode);
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
            codeNum = Cache.GetOne(substitutionCode).CodeNum;
        else if (substitutionCondition == SubstitutionCondition.Molar && Tooth.IsMolar(toothNum))
            codeNum = Cache.GetOne(substitutionCode).CodeNum;
        else if (substitutionCondition == SubstitutionCondition.SecondMolar && Tooth.IsSecondMolar(toothNum))
            codeNum = Cache.GetOne(substitutionCode).CodeNum;
        else if (substitutionCondition == SubstitutionCondition.Posterior && Tooth.IsPosterior(toothNum)) codeNum = Cache.GetOne(substitutionCode).CodeNum;
        return codeNum;
    }

    public static List<long> GetCodeNumsForPref(PrefName prefName)
    {
        return GetCodeNumsForProcCodes(PrefC.GetString(prefName));
    }

    public static List<long> GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed codeGroupFixed)
    {
        var codeGroup = CodeGroups.GetOneForCodeGroupFixed(codeGroupFixed);
        if (codeGroup == null) return [];
        return GetCodeNumsForProcCodes(codeGroup.ProcCodes);
    }

    public static List<long> GetCodeNumsForProcCodes(string procCodes)
    {
        var listCodes = procCodes.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        return GetWhereFromList(x => listCodes.Contains(x.ProcCode)).Select(x => x.CodeNum).ToList();
    }

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

    private static List<long> GetInsHistCodeNumsForBenefit(Benefit benefit, PrefName prefNameInsHist, ProcedureCode procedureCode)
    {
        if (benefit == null) return [];
        var listCodeNums = GetCodeNumsForPref(prefNameInsHist);
        if (procedureCode == null || listCodeNums.Contains(procedureCode.CodeNum)) return listCodeNums; //The proc is not included or is part of the group
        return [];
    }

    public static bool CanBypassLockDate(long codeNum, double procFee)
    {
        var isBypassGlobalLock = false;
        var procedureCode = GetFirstOrDefaultFromList(x => x.CodeNum == codeNum);
        if (procedureCode != null && procedureCode.BypassGlobalLock == BypassLockStatus.BypassIfZero && CompareDouble.IsZero(procFee)) isBypassGlobalLock = true;
        return isBypassGlobalLock;
    }

    public static bool DoAnyBypassLockDate()
    {
        var procedureCode = GetFirstOrDefaultFromList(x => x.BypassGlobalLock == BypassLockStatus.BypassIfZero);
        return procedureCode != null;
    }

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
        return Cache.GetContainsKey(myCode);
    }

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

    public static string GetLaymanTerm(long codeNum)
    {
        var laymanTerm = "";
        var procedureCode = GetFirstOrDefaultFromList(x => x.CodeNum == codeNum);
        if (procedureCode != null) laymanTerm = procedureCode.LaymanTerm != "" ? procedureCode.LaymanTerm : procedureCode.Descript;
        return laymanTerm;
    }

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
            .SelectMany(x => x.Procedures.Split([','], StringSplitOptions.RemoveEmptyEntries))
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
            command = "UPDATE procedurecode SET ProcCat=" + catNum
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
        var itemorder = 0;
        for (var i = 0; i < array.Length; i += 2)
        {
            if (!IsValidCode(array[i + 1]))
            {
                continue;
            }
            
            var def = new Def
            {
                Category = DefCat.ApptProcsQuickAdd,
                ItemOrder = itemorder++,
                ItemName = array[i],
                ItemValue = array[i + 1]
            };
            
            Defs.Insert(def);
            
            var logText = "Definition created: " + def.ItemName + " with category: " + def.Category.GetDescription();
            
            SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logText);
        }
    }

    public static int ResetADAdescriptionsAndAbbrs()
    {
        return ResetADAdescriptionsAndAbbrs(GetADAcodes());
    }

    public static List<ProcedureCode> GetADAcodes()
    {
        var listProcedureCodes = new List<ProcedureCode>();
        //Split raw data into non-empty lines of text in the file.
        var arrayAdaCodeLines = Class1.GetADAcodes().Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var arrayProcedureCodeSettingLines = Class1.GetProcedureCodeSettings().Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        string[] arrayAdaDictionaryCode;
        //load our codes into a hashtable
        
        var hashTable = new Hashtable(); //key=adacode, value=entire row string
        foreach (var t in arrayProcedureCodeSettingLines)
        {
            hashTable.Add(t.Substring(0, 5), t);
        }
        
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

    public static int SetTreatAreasForADACodes()
    {
        var listProcedureCodes = GetAllCodes(); //Ordered by D-code.
        var arrayProcedureCodeTreatAreasLines = Class1.GetProcedureCodeTreatAreas() //~850 rows
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
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

    public static int ResetADAdescriptionsAndAbbrs(List<ProcedureCode> procedureCodes)
    {
        ProcedureCode procedureCode;
        var count = 0;
        for (var i = 0; i < procedureCodes.Count; i++)
        {
            if (!IsValidCode(procedureCodes[i].ProcCode)) //If this code is not in this database
                continue;
            procedureCode = GetProcCode(procedureCodes[i].ProcCode);
            var datePrevious = procedureCode.DateTStamp;
            var isDescriptMatch = procedureCode.Descript == procedureCodes[i].Descript;
            var isDbProcAbbrDescBlank = string.IsNullOrWhiteSpace(procedureCode.AbbrDesc);
            if (!isDescriptMatch || isDbProcAbbrDescBlank) //Only increments one time for each code if there are changes necessary.
                count++;
            if (!isDescriptMatch)
            {
                //Update description.
                var oldDescript = procedureCode.Descript;
                procedureCode.Descript = procedureCodes[i].Descript;
                Update(procedureCode);
                SecurityLogs.MakeLogEntry(EnumPermType.ProcCodeEdit, 0, "Code " + procedureCode.ProcCode + " changed from '" + oldDescript + "' to '" + procedureCode.Descript + "' by D-Codes Tool."
                    , procedureCode.CodeNum, datePrevious);
            }

            if (isDbProcAbbrDescBlank)
            {
                //Update abbreviation if current code.AbbrDesc in db is blank.
                var oldAbbrDesc = procedureCode.AbbrDesc;
                procedureCode.AbbrDesc = procedureCodes[i].AbbrDesc;
                Update(procedureCode);
                SecurityLogs.MakeLogEntry(EnumPermType.ProcCodeEdit, 0, $"Code {procedureCode.ProcCode} changed from '{oldAbbrDesc}' to '{procedureCode.AbbrDesc}' by D-Codes Tool."
                    , procedureCode.CodeNum, datePrevious);
            }
        }

        return count;
    }

    public static bool HasMissedCode()
    {
        return Cache.GetContainsKey("D9986");
    }

    public static bool HasCancelledCode()
    {
        return Cache.GetContainsKey("D9987");
    }

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

    public static List<ProcedureCode> GetFromCommaDelimitedList(string codes)
    {
        return string.IsNullOrEmpty(codes) ? [] : codes.Split(',').Select(GetProcCode).ToList();
    }

    public static List<ProcedureCode> GetAllCodes()
    {
        return ProcedureCodeCrud.SelectMany("SELECT * FROM procedurecode ORDER BY ProcCode");
    }

    public static void ClearFkey(List<long> codeNums)
    {
        ProcedureCodeCrud.ClearFkey(codeNums);
    }

    public static List<ProcedureCode> GetCodesForCodeNums(List<long> codeNums)
    {
        return Cache.GetWhere(x => codeNums.Contains(x.CodeNum));
    }

    public static List<long> GetOrthoBandingCodeNums()
    {
        var orthoNums = PrefC.GetString(PrefName.OrthoPlacementProcsList);
        return orthoNums != "" 
            ? orthoNums.Split(',').ToList().Select(x => SIn.Long(x)).ToList() 
            : GetWhereFromList(x => x.ProcCode.ToUpper().StartsWith("D8")).Select(x => x.CodeNum).ToList();
    }
    
    public static List<ProcedureCode> GetMandibularCodes()
    {
        var mandibularCodes = new List<ProcedureCode>();
        
        ODException.SwallowAnyException(() => { mandibularCodes = JsonConvert.DeserializeObject<List<ProcedureCode>>(Class1.GetMandibularCodes()); });

        return mandibularCodes ?? [];
    }
    
    private class ProcedureCodeCache : CacheDictNonPkAbs<ProcedureCode, string, ProcedureCode>
    {
        protected override List<ProcedureCode> GetCacheFromDb()
        {
            return ProcedureCodeCrud.SelectMany("SELECT * FROM procedurecode ORDER BY ProcCat, ProcCode");
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
            GetTableFromCache(false);
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

    private static readonly ProcedureCodeCache Cache = new();

    public static List<ProcedureCode> GetListDeep()
    {
        return Cache.GetDeepCopyList();
    }

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
    }

    public static ProcedureCode GetOne(string procCode)
    {
        return Cache.GetOne(procCode);
    }

    public static ProcedureCode GetFirstOrDefault(Func<ProcedureCode, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static ProcedureCode GetFirstOrDefaultFromList(Func<ProcedureCode, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefaultFromList(predicate, shortList);
    }

    public static List<ProcedureCode> GetWhere(Func<ProcedureCode, bool> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static List<ProcedureCode> GetWhereFromList(Predicate<ProcedureCode> predicate, bool shortList = false)
    {
        return Cache.GetWhereFromList(predicate, shortList);
    }

    public static bool GetContainsKey(string procCode)
    {
        return Cache.GetContainsKey(procCode);
    }

    public static ProcedureCode GetByInsHistPref(PrefName prefName)
    {
        return GetProcCode(GetCodeNumsForPref(prefName).FirstOrDefault());
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
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