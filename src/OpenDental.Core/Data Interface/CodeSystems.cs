using System.Collections.Generic;
using System.IO;
using System.Linq;
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
}