using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class CodeSystems
{
    public delegate void ProgressArgs(int numTotal, int numDone);

    public static List<CodeSystem> GetForCurrentVersion()
    {
        return CodeSystemCrud.SelectMany("SELECT * FROM codesystem WHERE CodeSystemName NOT IN ('AdministrativeSex','CDT')");
    }

    public static void Update(CodeSystem codeSystem)
    {
        CodeSystemCrud.Update(codeSystem);
    }

    public static void UpdateCurrentVersion(CodeSystem codeSystem)
    {
        codeSystem.VersionCur = codeSystem.VersionAvail;
        CodeSystemCrud.Update(codeSystem);
    }

    public static void UpdateCurrentVersion(CodeSystem codeSystem, string versionId)
    {
        if (string.CompareOrdinal(codeSystem.VersionCur, versionId) > 0)
        {
            return;
        }

        codeSystem.VersionCur = versionId;
        CodeSystemCrud.Update(codeSystem);
    }

    public static void ImportCdcrec(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var dictionaryCdcrecs = Cdcrecs.GetAll().ToDictionary(x => x.CdcrecCode, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayCdcrecs;
        var cdcrec = new Cdcrec();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArrayCdcrecs = stringArrayLines[i].Split('\t');
            if (dictionaryCdcrecs.ContainsKey(stringArrayCdcrecs[0]))
            {
                //code already exists
                cdcrec = dictionaryCdcrecs[stringArrayCdcrecs[0]];
                if (updateExisting &&
                    (cdcrec.HeirarchicalCode != stringArrayCdcrecs[1]
                     || cdcrec.Description != stringArrayCdcrecs[2]))
                {
                    cdcrec.HeirarchicalCode = stringArrayCdcrecs[1];
                    cdcrec.Description = stringArrayCdcrecs[2];
                    Cdcrecs.Update(cdcrec);
                    numCodesUpdated++;
                }

                continue;
            }

            cdcrec.CdcrecCode = stringArrayCdcrecs[0];
            cdcrec.HeirarchicalCode = stringArrayCdcrecs[1];
            cdcrec.Description = stringArrayCdcrecs[2];
            Cdcrecs.Insert(cdcrec);
            numCodesImported++;
        }
    }

    public static void ImportCpt(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, string versionID)
    {
        if (tempFileName == null) return;

        var dictionaryCodes = Cpts.GetAll().ToDictionary(x => x.CptCode, x => x.Description);
        var regex = new Regex(@"^([\d]{4}[\d\w])\s+(.+?)$"); //Regex = "At the beginning of the string, find five numbers, followed by a white space (tab or space) followed by one or more characters (but as few as possible) to the end of the line."
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayCpts;
        var isHeader = true;
        var cpt = new Cpt();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            if (isHeader)
            {
                if (!regex.IsMatch(stringArrayLines[i]))
                    //if(!lines[i].Contains("\t")) {	
                    continue; //Copyright info is present at the head of the file.

                isHeader = false;
            }

            stringArrayCpts = new string[2];
            stringArrayCpts[0] = regex.Match(stringArrayLines[i]).Groups[1].Value; //First five alphanumeric characters
            stringArrayCpts[1] = regex.Match(stringArrayLines[i]).Groups[2].Value; //Everything after the 6th character
            if (dictionaryCodes.Keys.Contains(stringArrayCpts[0]))
            {
                //code already exists
                Cpts.UpdateDescription(stringArrayCpts[0], stringArrayCpts[1], versionID);
                if (dictionaryCodes[stringArrayCpts[0]] != stringArrayCpts[1])
                    //The description is different
                    numCodesUpdated++;
            }
            else
            {
                cpt.CptCode = stringArrayCpts[0];
                cpt.Description = stringArrayCpts[1];
                cpt.VersionIDs = versionID;
                Cpts.Insert(cpt);
                numCodesImported++;
            }
        }
    }

    public static void ImportCvx(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var dictionaryCodes = Cvxs.GetAll().ToDictionary(x => x.CvxCode, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayCvxs;
        var cvx = new Cvx();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArrayCvxs = stringArrayLines[i].Split('\t');
            if (dictionaryCodes.ContainsKey(stringArrayCvxs[0]))
            {
                //code already exists
                cvx = dictionaryCodes[stringArrayCvxs[0]];
                if (updateExisting && cvx.Description != stringArrayCvxs[1])
                {
                    //We do want to update and description is different.
                    cvx.Description = stringArrayCvxs[1];
                    Cvxs.Update(cvx);
                    numCodesUpdated++;
                }

                continue;
            }

            cvx.CvxCode = stringArrayCvxs[0];
            cvx.Description = stringArrayCvxs[1];
            Cvxs.Insert(cvx);
            numCodesImported++;
        }
    }

    public static void ImportHcpcs(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var dictionaryHcpcs = Hcpcses.GetAll().ToDictionary(x => x.HcpcsCode, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayHcpcs;
        var hcpcs = new Hcpcs();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArrayHcpcs = stringArrayLines[i].Split('\t');
            if (dictionaryHcpcs.ContainsKey(stringArrayHcpcs[0]))
            {
                //code already exists
                hcpcs = dictionaryHcpcs[stringArrayHcpcs[0]];
                if (updateExisting && hcpcs.DescriptionShort != stringArrayHcpcs[1])
                {
                    hcpcs.DescriptionShort = stringArrayHcpcs[1];
                    Hcpcses.Update(hcpcs);
                    numCodesUpdated++;
                }

                continue;
            }

            hcpcs.HcpcsCode = stringArrayHcpcs[0];
            hcpcs.DescriptionShort = stringArrayHcpcs[1];
            Hcpcses.Insert(hcpcs);
            numCodesImported++;
        }
    }

    public static void ImportIcd10(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var dictionaryIcd10s = Icd10s.GetAll().ToDictionary(x => x.Icd10Code, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayIcd10s;
        var icd10 = new Icd10();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArrayIcd10s = stringArrayLines[i].Split('\t');
            if (dictionaryIcd10s.ContainsKey(stringArrayIcd10s[0]))
            {
                //code already exists
                icd10 = dictionaryIcd10s[stringArrayIcd10s[0]];
                if (updateExisting &&
                    (icd10.Description != stringArrayIcd10s[1] || icd10.IsCode != stringArrayIcd10s[2])) //Code informatin is different
                {
                    icd10.Description = stringArrayIcd10s[1];
                    icd10.IsCode = stringArrayIcd10s[2];
                    Icd10s.Update(icd10);
                    numCodesUpdated++;
                }

                continue;
            }

            icd10.Icd10Code = stringArrayIcd10s[0];
            icd10.Description = stringArrayIcd10s[1];
            icd10.IsCode = stringArrayIcd10s[2];
            Icd10s.Insert(icd10);
            numCodesImported++;
        }
    }

    public static void ImportIcd9(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        //Customers may have an old codeset that has a truncated uppercase description, if so we want to update with new descriptions.
        var isDescriptionsOld = Icd9s.IsOldDescriptions();
        var dictionaryCodes = Icd9s.GetAll().ToDictionary(x => x.ICD9Code, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayICD9s;
        var icd9 = new ICD9();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArrayICD9s = stringArrayLines[i].Split('\t');
            if (dictionaryCodes.ContainsKey(stringArrayICD9s[0]))
            {
                //code already exists
                icd9 = dictionaryCodes[stringArrayICD9s[0]];
                if ((isDescriptionsOld || updateExisting) && icd9.Description != stringArrayICD9s[1])
                {
                    //The new description does not match the description in the database.
                    icd9.Description = stringArrayICD9s[1];
                    Icd9s.Update(icd9);
                    numCodesUpdated++;
                }

                continue;
            }

            icd9.ICD9Code = stringArrayICD9s[0];
            icd9.Description = stringArrayICD9s[1];
            Icd9s.Insert(icd9);
            numCodesImported++;
        }
    }

    public static void ImportLoinc(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var dictionaryLoincs = Loincs.GetAll().ToDictionary(x => x.LoincCode, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayLoincs;
        var loincOld = new Loinc();
        var loincNew = new Loinc();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArrayLoincs = stringArrayLines[i].Split('\t');
            loincNew.LoincCode = stringArrayLoincs[0];
            loincNew.Component = stringArrayLoincs[1];
            loincNew.PropertyObserved = stringArrayLoincs[2];
            loincNew.TimeAspct = stringArrayLoincs[3];
            loincNew.SystemMeasured = stringArrayLoincs[4];
            loincNew.ScaleType = stringArrayLoincs[5];
            loincNew.MethodType = stringArrayLoincs[6];
            loincNew.StatusOfCode = stringArrayLoincs[7];
            loincNew.NameShort = stringArrayLoincs[8];
            loincNew.ClassType = stringArrayLoincs[9];
            loincNew.UnitsRequired = stringArrayLoincs[10] == "Y";
            loincNew.OrderObs = stringArrayLoincs[11];
            loincNew.HL7FieldSubfieldID = stringArrayLoincs[12];
            loincNew.ExternalCopyrightNotice = stringArrayLoincs[13];
            loincNew.NameLongCommon = stringArrayLoincs[14];
            loincNew.UnitsUCUM = stringArrayLoincs[15];
            loincNew.RankCommonTests = SIn.Int(stringArrayLoincs[16]);
            loincNew.RankCommonOrders = SIn.Int(stringArrayLoincs[17]);
            if (dictionaryLoincs.ContainsKey(stringArrayLoincs[0]))
            {
                //code already exists; arrayLoinc[0]==Loinc Code
                loincOld = dictionaryLoincs[stringArrayLoincs[0]];
                if (updateExisting &&
                    (loincOld.LoincCode != stringArrayLoincs[0]
                     || loincOld.Component != stringArrayLoincs[1]
                     || loincOld.PropertyObserved != stringArrayLoincs[2]
                     || loincOld.TimeAspct != stringArrayLoincs[3]
                     || loincOld.SystemMeasured != stringArrayLoincs[4]
                     || loincOld.ScaleType != stringArrayLoincs[5]
                     || loincOld.MethodType != stringArrayLoincs[6]
                     || loincOld.StatusOfCode != stringArrayLoincs[7]
                     || loincOld.NameShort != stringArrayLoincs[8]
                     || loincOld.ClassType != stringArrayLoincs[9]
                     || loincOld.UnitsRequired != (stringArrayLoincs[10] == "Y")
                     || loincOld.OrderObs != stringArrayLoincs[11]
                     || loincOld.HL7FieldSubfieldID != stringArrayLoincs[12]
                     || loincOld.ExternalCopyrightNotice != stringArrayLoincs[13]
                     || loincOld.NameLongCommon != stringArrayLoincs[14]
                     || loincOld.UnitsUCUM != stringArrayLoincs[15]
                     || loincOld.RankCommonTests != SIn.Int(stringArrayLoincs[16])
                     || loincOld.RankCommonOrders != SIn.Int(stringArrayLoincs[17])))
                {
                    Loincs.Update(loincNew);
                    numCodesUpdated++;
                }

                continue;
            }

            Loincs.Insert(loincNew);
            numCodesImported++;
        }
    }

    public static void ImportRxNorm(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        //RxNorms can have two codes for each RxCui. One RxNorm will have a value in the MmslCode and a blank description and the other will have a
        //value in the Description and a blank MmslCode. 
        var listRxNorms = RxNorms.GetAll();
        var dictionaryRxNormsMmslCodes = listRxNorms.Where(x => x.MmslCode != "").ToDictionary(x => x.RxCui, x => x);
        var dictionaryRxNormsDefinitions = listRxNorms.Where(x => x.Description != "").ToDictionary(x => x.RxCui, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArrayRxNorms;
        var rxNorm = new RxNorm();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //Each loop should read exactly one line of code. Each line will NOT be a unique code.
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArrayRxNorms = stringArrayLines[i].Split('\t');
            if (dictionaryRxNormsMmslCodes.ContainsKey(stringArrayRxNorms[0]))
            {
                //code with an MmslCode already exists
                rxNorm = dictionaryRxNormsMmslCodes[stringArrayRxNorms[0]];
                if (updateExisting)
                    if (stringArrayRxNorms[1] != "" && stringArrayRxNorms[1] != rxNorm.MmslCode)
                    {
                        rxNorm.MmslCode = stringArrayRxNorms[1];
                        rxNorm.Description = ""; //Should be blank for all MMSL code entries. See below for non-MMSL entries with descriptions.
                        RxNorms.Update(rxNorm);
                        numCodesUpdated++;
                    }

                continue;
            }

            if (dictionaryRxNormsDefinitions.ContainsKey(stringArrayRxNorms[0]))
            {
                //code with a Description already exists
                rxNorm = dictionaryRxNormsDefinitions[stringArrayRxNorms[0]];
                if (updateExisting)
                {
                    var newDescript = stringArrayRxNorms[2];
                    //if(newDescript.Length>255) {
                    //	newDescript=newDescript.Substring(0,255);//Description column is only varchar(255) so some descriptions will get truncated.
                    //}
                    //if(arrayRxNorm[2]!="" && newDescript!=rxNorm.Description) {
                    if (stringArrayRxNorms[2] != "" && stringArrayRxNorms[2] != rxNorm.Description)
                    {
                        rxNorm.MmslCode = ""; //should be blank for all entries that have a description.
                        rxNorm.Description = stringArrayRxNorms[2];
                        RxNorms.Update(rxNorm);
                        numCodesUpdated++;
                    }
                }

                continue;
            }

            rxNorm.RxCui = stringArrayRxNorms[0];
            rxNorm.MmslCode = stringArrayRxNorms[1];
            rxNorm.Description = stringArrayRxNorms[2];
            RxNorms.Insert(rxNorm);
            numCodesImported++;
        }
    }

    public static void ImportSnomed(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var listSnomeds = Snomeds.GetAll();
        var stringArrayLines = File.ReadAllLines(tempFileName);
        string[] stringArraySnomeds;
        var snomed = new Snomed();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            stringArraySnomeds = stringArrayLines[i].Split('\t');
            if (stringArraySnomeds.Length < 2) continue; //Line is not formatted properly. Do not import this code.

            snomed = listSnomeds.Find(x => x.SnomedCode == stringArraySnomeds[0]);
            if (snomed == null)
            {
                //Adding a new SNOMED
                snomed = new Snomed();
                snomed.SnomedCode = stringArraySnomeds[0];
                snomed.Description = stringArraySnomeds[1];
                Snomeds.Insert(snomed);
                listSnomeds.Add(snomed); //To prevent inserting duplicates
                numCodesImported++;
                continue;
            }

            if (updateExisting && snomed.Description != stringArraySnomeds[1])
            {
                snomed.Description = stringArraySnomeds[1];
                Snomeds.Update(snomed);
                numCodesUpdated++;
            }
        }
    }

    public static void ImportSop(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numcodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var dictionarySops = Sops.GetDeepCopy().ToDictionary(x => x.SopCode, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        var sop = new Sop();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            var stringArraySops = stringArrayLines[i].Split('\t');
            if (dictionarySops.ContainsKey(stringArraySops[0]))
            {
                //code already exists
                sop = dictionarySops[stringArraySops[0]];
                if (updateExisting && sop.Description != stringArraySops[1])
                {
                    sop.Description = stringArraySops[1];
                    Sops.Update(sop);
                    numcodesUpdated++;
                }

                continue;
            }

            sop.SopCode = stringArraySops[0];
            sop.Description = stringArraySops[1];
            Sops.Insert(sop);
            numCodesImported++;
        }
    }

    public static void ImportUcum(string tempFileName, ProgressArgs progressArgs, ref bool quit, ref int numCodesImported, ref int numCodesUpdated, bool updateExisting)
    {
        if (tempFileName == null) return;

        var dictionaryUcums = Ucums.GetAll().ToDictionary(x => x.UcumCode, x => x);
        var stringArrayLines = File.ReadAllLines(tempFileName);
        var ucum = new Ucum();
        for (var i = 0; i < stringArrayLines.Length; i++)
        {
            //each loop should read exactly one line of code. and each line of code should be a unique code
            if (quit) return;

            if (i % 100 == 0) progressArgs(i + 1, stringArrayLines.Length);

            var stringArrayUcums = stringArrayLines[i].Split('\t');
            if (dictionaryUcums.ContainsKey(stringArrayUcums[0]))
            {
                //code already exists
                ucum = dictionaryUcums[stringArrayUcums[0]];
                if (updateExisting && ucum.Description != stringArrayUcums[1])
                {
                    ucum.Description = stringArrayUcums[1];
                    Ucums.Update(ucum);
                    numCodesUpdated++;
                }

                continue;
            }

            ucum.UcumCode = stringArrayUcums[0];
            ucum.Description = stringArrayUcums[1];
            ucum.IsInUse = false;
            Ucums.Insert(ucum);
            numCodesImported++;
        }
    }
}