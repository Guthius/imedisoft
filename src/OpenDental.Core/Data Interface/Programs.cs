using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Programs
{
    private static readonly List<string> LIST_TIGERVIEW_PHI_FIELDS = new()
    {
        "PatientID",
        "FirstName",
        "LastName",
        "MiddleName",
        "DOB",
        "Gender",
        "PatientSSN",
        "SubscriberSSN",
        "Email",
        "phHome",
        "phWork",
        "addrStreetNo",
        "addrStreetName",
        "addrSuiteNo",
        "addrCity",
        "addrState",
        "addrZip"
    };
    
    public static bool IsEnabledByHq(Program program, out string err)
    {
        err = "";
        if (program == null)
        {
            err = Lans.g("Programs", "The currently selected program could not be found.");
            return false;
        }

        if (DoUseCacheValues(program))
        {
            err = program.CustErr;
            if (string.IsNullOrWhiteSpace(err)) //if the CustErr wasn't set at HQ then we assume a customer is not able to use this program because they are not on support
                err = Lans.g("Program", "You must be on support to use this program.");
            return !program.IsDisabledByHq;
        }

        var hqProgram = HqProgram.GetAll().FirstOrDefault(x => x.ProgramNameAsString.Trim() == program.ProgName.Trim());
        if (hqProgram == null)
        {
            err = Lans.g("Programs", "The currently selected HQ program could not be found.");
            return false;
        }

        if (!hqProgram.IsEnabled)
        {
            err = hqProgram.CustErr;
            //Delete all programs disabled by HQ
            ProgramProperties.GetForProgram(program.ProgramNum).ForEach(x => ProgramProperties.Delete(x));
            Delete(program);
            if (string.IsNullOrWhiteSpace(err)) err = Lans.g("Program", program.ProgName + " has been removed.");
            return false;
        }

        return true;
    }
    
    public static bool IsEnabledByHq(ProgramName progName, out string err)
    {
        var progCur = GetCur(progName);
        return IsEnabledByHq(progCur, out err);
    }

    private static bool DoUseCacheValues(Program prog)
    {
        //Is not an OD defined program name or is not a program HQ is concerned with enabling/disabling.
        return !Enum.TryParse(prog.ProgName, out ProgramName progName)
               || !(HqProgram.IsInitialized() && HqProgram.GetAll().Any(x => x.ProgramNameAsString.Trim() == progName.ToString()));
    }
    
    public static bool Update(Program cur, Program old = null)
    {
        var isRefreshNeeded = false;

        if (old is null)
        {
            ProgramCrud.Update(cur);
            isRefreshNeeded = true;
        }
        else
        {
            isRefreshNeeded = ProgramCrud.Update(cur, old);
        }

        return isRefreshNeeded;
    }
    
    public static long Insert(Program Cur)
    {
        return ProgramCrud.Insert(Cur);
    }

    public static void Delete(Program prog)
    {
        var command = "DELETE from toolbutitem WHERE ProgramNum = " + SOut.Long(prog.ProgramNum);
        Db.NonQ(command);
        command = "DELETE from program WHERE ProgramNum = '" + prog.ProgramNum + "'";
        Db.NonQ(command);
    }

    public static bool IsEnabled(ProgramName progName)
    {
        var program = GetFirstOrDefault(x => x.ProgName == progName.ToString());
        if (program == null) return false;
        return program.Enabled;
    }
    
    public static bool IsEnabled(long programNum)
    {
        var program = GetFirstOrDefault(x => x.ProgramNum == programNum);
        return program == null ? false : program.Enabled;
    }

    public static Program GetProgram(long programNum)
    {
        return GetFirstOrDefault(x => x.ProgramNum == programNum);
    }

    public static Program GetCur(ProgramName progName)
    {
        return GetFirstOrDefault(x => x.ProgName == progName.ToString());
    }

    public static long GetProgramNum(ProgramName progName)
    {
        var program = GetCur(progName);
        return program == null ? 0 : program.ProgramNum;
    }

    public static List<string> GetListDisabledForWeb()
    {
        return PrefC.GetString(PrefName.ProgramLinksDisabledForWeb).Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public static bool UsingEcwTightMode()
    {
        if (IsEnabled(ProgramName.eClinicalWorks) && ProgramProperties.GetPropVal(ProgramName.eClinicalWorks, "eClinicalWorksMode") == "0") return true;
        return false;
    }

    public static bool UsingEcwFullMode()
    {
        if (IsEnabled(ProgramName.eClinicalWorks) && ProgramProperties.GetPropVal(ProgramName.eClinicalWorks, "eClinicalWorksMode") == "2") return true;
        return false;
    }

    public static bool UsingEcwTightOrFullMode()
    {
        if (UsingEcwTightMode() || UsingEcwFullMode()) return true;
        return false;
    }

    public static string GetProgramPath(Program program)
    {
        var overridePath = ProgramProperties.GetLocalPathOverrideForProgram(program.ProgramNum);
        if (overridePath != "") return overridePath;
        return program.Path;
    }

    public static string GetProgramPath(ProgramName progName)
    {
        return GetProgramPath(GetFirstOrDefault(x => x.ProgName == progName.ToString()));
    }

    public static bool IsStatic(Program prog)
    {
        //Currently there is just one static program. As more are created they will need to be added to this check.
        if (prog.ProgName == ProgramName.RapidCall.ToString()) return true;
        return false;
    }

    public static void ScrubExportedPatientData()
    {
        //List all program links here. If there is nothing to do for that link, then create a comment stating so.
        var path = "";
        //Adstra: Has no file paths containing outgoing pateint data from Open Dental.
        //AiDental: Has no file paths containing outgoing patient data from Open Dental
        //Apixia:
        ScrubFileForProperty(ProgramName.Apixia, "System path to Apixia Digital Imaging ini file", "", true); //C:\Program Files\Digirex\Switch.ini
        //Apteryx: Has no file paths containing outgoing patient data from Open Dental.
        //BioPAK: Has no file paths containing outgoing patient data from Open Dental.
        //CADI has no file paths containing outgoing patient data from Open Dental.
        //CallFire: Has no file paths containing outgoing patient data from Open Dental.
        //Camsight: Has no file paths containing outgoing patient data from Open Dental.
        //CaptureLink: Has no file paths containing outgoing patient data from Open Dental.
        //Carestream:
        ScrubFileForProperty(ProgramName.Carestream, "Patient.ini path", "", true); //C:\Carestream\Patient.ini
        //Cerec: Has no file paths containing outgoing patient data from Open Dental.
        //CliniView: Has no file paths containing outgoing patient data from Open Dental.
        //ClioSoft: Has no file paths containing outgoing patient data from Open Dental.
        //DBSWin:
        ScrubFileForProperty(ProgramName.DBSWin, "Text file path", "", true); //C:\patdata.txt
        //DentalEye: Has no file paths containing outgoing patient data from Open Dental.
        //DentalStudio: Has no file paths containing outgoing patient data from Open Dental.
        //DentForms: Has no file paths containing outgoing patient data from Open Dental.
        //DentX: Has no file paths containing outgoing patient data from Open Dental.
        //Dexis:
        ScrubFileForProperty(ProgramName.Dexis, "InfoFile path", "", true); //InfoFile.txt
        //Digora: Has no file paths containing outgoing patient data from Open Dental.
        //Divvy: Has no file paths containing outgoing patient data from Open Dental.
        //Dolphin:
        ScrubFileForProperty(ProgramName.Dolphin, "Filename", "", true); //C:\Dolphin\Import\Import.txt
        //DrCeph: Has no file paths containing outgoing patient data from Open Dental.
        //Dxis: Has no file paths containing outgoing patient data from Open Dental.
        //EasyNotesPro: Has no file paths containing outgoing patient data from Open Dental.
        //eClinicalWorks: HL7 files are created, but eCW is supposed to consume and delete them.
        //EvaSoft: Has no file paths containing outgoing patient data from Open Dental.
        //EzDenti:
        var program = GetCur(ProgramName.EzDenti);
        RemoveLinkageXMLFile(program);
        //Ez3Di:
        program = GetCur(ProgramName.Ez3Di);
        RemoveLinkageXMLFile(program);
        //FloridaProbe: Has no file paths containing outgoing patient data from Open Dental.
        //Guru: Has no file paths containing outgoing patient data from Open Dental.
        //HandyDentist: Has no file paths containing outgoing patient data from Open Dental.
        //HouseCalls:
        //Per Nathan(TaskNum:3517423), disable deleting Appt.txt on close for HouseCalls bridge.
        //ScrubFileForProperty(ProgramName.HouseCalls,"Export Path","Appt.txt",true);//C:\HouseCalls\Appt.txt
        //IAP: Has no file paths containing outgoing patient data from Open Dental.
        //iCat:
        ScrubFileForProperty(ProgramName.iCat, "XML output file path", "", true); //C:\iCat\Out\pm.xml
        //ImageFX: Has no file paths containing outgoing patient data from Open Dental.
        ScrubFileForProperty(ProgramName.JazzClassicCapture, "XML output file path", "", true); //C:\Program Files\Jazz Imaging LLC\Jazz Classic\Classic.exe
        ScrubFileForProperty(ProgramName.JazzClassicExamView, "XML output file path", "", true); //C:\Program Files\Jazz Imaging LLC\Jazz Classic\Classic.exe
        ScrubFileForProperty(ProgramName.JazzClassicPatientUpdate, "XML output file path", "", true); //C:\Program Files\Jazz Imaging LLC\Jazz Classic\Classic.exe
        //Lightyear: Has no file paths containing outgoing patient data from Open Dental.
        //MediaDent:
        ScrubFileForProperty(ProgramName.MediaDent, "Text file path", "", true); //C:\MediadentInfo.txt
        //MedLink: Has no file paths containing outgoing patient data from Open Dental.
        //MiPACS: Has no file paths containing outgoing patient data from Open Dental.
        //Mountainside: Has no file paths containing outgoing patient data from Open Dental.
        //NewCrop: Has no file paths containing outgoing patient data from Open Dental.
        ScrubFileForProperty(ProgramName.One2, "XML output file path", "", true); //C:\osstem\onevision\one2\one2.exe
        //Orion: Has no file paths containing outgoing patient data from Open Dental.
        //OrthoPlex: Has no file paths containing outgoing patient data from Open Dental.
        //Owandy: Has no file paths containing outgoing patient data from Open Dental.
        //PayConnect: Has no file paths containing outgoing patient data from Open Dental.
        //Patterson:
        ScrubFileForProperty(ProgramName.Patterson, "System path to Patterson Imaging ini", "", true); //C:\Program Files\PDI\Shared files\Imaging.ini
        //PerioPal: Has no file paths containing outgoing patient data from Open Dental.
        //Planmeca: Has no file paths containing outgoing patient data from Open Dental.
        //PORTRAY: Has no file paths containing outgoing patient data from Open Dental.
        //PracticeBooster: Has no file paths containing outgoing patient data from Open Dental.
        //PracticeWebReports: Has no file paths containing outgoing patient data from Open Dental.
        //PreXionAcquire: Has no file paths containing outgoing patient data from Open Dental.
        //PreXionViewer: Has no file paths containing outgoing patient data from Open Dental.
        //Progeny: Has no file paths containing outgoing patient data from Open Dental.
        //PT: Per our website "The files involved get deleted immediately after they are consumed."
        //PTupdate: Per our website "The files involved get deleted immediately after they are consumed."
        //RayMage: Has no file paths containing outgoing patient data from Open Dental.
        //Schick: Has no file paths containing outgoing patient data from Open Dental.
        //Shining3D: Has no file paths containing outgoing patient data from Open Dental.
        //Sirona:
        program = GetCur(ProgramName.Sirona);
        if (program.Enabled)
        {
            path = GetProgramPath(program);
            //read file C:\sidexis\sifiledb.ini
            var iniFile = Path.GetDirectoryName(path) + "\\sifiledb.ini";
            if (File.Exists(iniFile))
            {
                var sendBox = ReadValueFromIni("FromStation0", "File", iniFile);
                if (File.Exists(sendBox)) File.WriteAllText(sendBox, ""); //Clear the sendbox instead of deleting.
            }
        }

        //Sopro: Has no file paths containing outgoing patient data from Open Dental.
        //SteriSimple: Has no file paths containing outgoing patient data from Open Dental.
        //ThreeShape: Has no file paths containing outgoing patient data from Open Dental.
        //TigerView:
        program = GetCur(ProgramName.TigerView); //C:\Program Files\PDI\Shared files\Imaging.ini.  TigerView complains if the file is not present.
        if (program.Enabled)
        {
            var programProperty = ProgramProperties.GetPropForProgByDesc(program.ProgramNum, "Tiger1.ini path");
            if (File.Exists(programProperty.PropertyValue))
            {
                var listLines = new List<string>();
                try
                {
                    listLines = File.ReadAllLines(programProperty.PropertyValue).ToList();
                }
                catch
                {
                    //Another instance of OD might be closing at the same time, in which case the delete will fail. Could also be a permission issue or a concurrency issue. Ignore.
                }

                int index;
                for (var i = 0; i < LIST_TIGERVIEW_PHI_FIELDS.Count; i++)
                {
                    //Clear out all fields that contain PHI rather that clearing the whole file.
                    index = listLines.FindIndex(x => x.ToLower().TrimStart().StartsWith(LIST_TIGERVIEW_PHI_FIELDS[i].ToLower()));
                    if (index < 0) continue;
                    listLines[index] = $"{LIST_TIGERVIEW_PHI_FIELDS[i]}=";
                }

                if (!listLines.IsNullOrEmpty()) //Only try to write if the read was successful above.
                    try
                    {
                        File.WriteAllLines(programProperty.PropertyValue, listLines);
                    }
                    catch
                    {
                        //Another instance of OD might be closing at the same time, in which case the delete will fail. Could also be a permission issue or a concurrency issue. Ignore.
                    }
            }
        }

        //Trojan: Has no file paths containing outgoing patient data from Open Dental.
        //Trophy: Has no file paths containing outgoing patient data from Open Dental.
        //TrophyEnhanced: Has no file paths containing outgoing patient data from Open Dental.
        //Tscan: Has no file paths containing outgoing patient data from Open Dental.
        //UAppoint: Has no file paths containing outgoing patient data from Open Dental.
        //Vipersoft: Has no file paths containing outgoing patient data from Open Dental.
        //VixWin: Has no file paths containing outgoing patient data from Open Dental.
        //VixWinBase41: Has no file paths containing outgoing patient data from Open Dental.
        //VixWinOld: Has no file paths containing outgoing patient data from Open Dental.
        //Xcharge: Has no file paths containing outgoing patient data from Open Dental.
        //XVWeb: Has no file paths containing outgoing patient data from Open Dental.
        ScrubFileForProperty(ProgramName.XDR, "InfoFile path", "", true); //C:\XDRClient\Bin\infofile.txt
    }
    
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileStringFromIni(string section, string key, string def, StringBuilder retVal, int size, string filePath);

    private static string ReadValueFromIni(string section, string key, string iniFile)
    {
        var strBuild = new StringBuilder(255);
        var i = GetPrivateProfileStringFromIni(section, key, "", strBuild, 255, iniFile);
        return strBuild.ToString();
    }

    private static void ScrubFileForProperty(ProgramName programName, string strFileProperty, string strFilePropertySuffix, bool isRemovable)
    {
        var program = GetCur(programName);
        if (program is null) return;
        if (!program.Enabled) return;
        var strFileToScrub = ProgramProperties.GetPropVal(program.ProgramNum, strFileProperty);
        if (!strFilePropertySuffix.IsNullOrEmpty()) strFileToScrub = ODFileUtils.CombinePaths(strFileToScrub, strFilePropertySuffix);
        if (!File.Exists(strFileToScrub)) return;
        try
        {
            File.WriteAllText(strFileToScrub, ""); //Always clear the file contents, in case deleting fails below.
        }
        catch
        {
            //Another instance of OD might be closing at the same time, in which case the delete will fail. Could also be a permission issue or a concurrency issue. Ignore.
        }

        if (!isRemovable) return;
        try
        {
            File.Delete(strFileToScrub);
        }
        catch
        {
            //Another instance of OD might be closing at the same time, in which case the delete will fail. Could also be a permission issue or a concurrency issue. Ignore.
        }
    }

    public static bool HasMultipleCreditCardProgramsEnabled()
    {
        return new List<bool>
        {
            IsEnabled(ProgramName.EdgeExpress), IsEnabled(ProgramName.Xcharge), IsEnabled(ProgramName.PayConnect), IsEnabled(ProgramName.PaySimple)
        }.Count(x => x) >= 2;
    }

    public static void SendEnabledProgramsToHQ()
    {
        CustomerUpdatesProxy.SendAndReceiveUpdateRequestXml(); //Piggy back on this, we don't do anything with result just want to trigger some code.
    }

    public static void RemoveLinkageXMLFile(Program program)
    {
        if (program == null || !program.Enabled) return;
        var path = GetProgramPath(program);
        if (File.Exists(path))
        {
            var dir = Path.GetDirectoryName(path);
            var linkage = ODFileUtils.CombinePaths(dir, "linkage.xml");
            if (File.Exists(linkage))
                try
                {
                    File.Delete(linkage);
                }
                catch
                {
                    //Another instance of OD might be closing at the same time, in which case the delete will fail. Could also be a permission issue or a concurrency issue. Ignore.
                }
        }
    }

    public static ProgramName GetActiveImagingAIProgram()
    {
        if (IsEnabled(ProgramName.Pearl)) return ProgramName.Pearl;
        if (IsEnabled(ProgramName.BetterDiagnostics)) return ProgramName.BetterDiagnostics;
        return ProgramName.None;
    }
    
    private class ProgramCache : CacheListAbs<Program>
    {
        protected override List<Program> GetCacheFromDb()
        {
            var command = "SELECT * FROM program ORDER BY ProgDesc";
            return ProgramCrud.SelectMany(command);
        }

        protected override List<Program> TableToList(DataTable dataTable)
        {
            return ProgramCrud.TableToList(dataTable);
        }

        protected override Program Copy(Program item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Program> items)
        {
            return ProgramCrud.ListToTable(items, "Program");
        }

        protected override void FillCacheIfNeeded()
        {
            Programs.GetTableFromCache(false);
        }
    }

    private static readonly ProgramCache Cache = new();

    public static List<Program> GetListDeep(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Program GetFirstOrDefault(Func<Program, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static List<Program> GetWhere(Predicate<Program> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}