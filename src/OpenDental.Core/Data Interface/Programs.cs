using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Programs
{
    /// <summary>List of the ini fields for the TigerView bridge that contain PHI.</summary>
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

    /// <summary>
    ///     Checks to see if we have disabled the current program at HQ. Handles null. Sets and translates the out
    ///     parameter where possible.
    /// </summary>
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


    /// <summary>
    ///     Checks to see if we have disabled the current program at HQ. Handles null Sets and translates the out
    ///     parameter where possible.
    /// </summary>
    public static bool IsEnabledByHq(ProgramName progName, out string err)
    {
        var progCur = GetCur(progName);
        return IsEnabledByHq(progCur, out err);
    }


    ///<summary>Checks to see if we have enabled the current plugin at HQ. </summary>
    public static bool IsDllEnabledByHq(Program progCur)
    {
        var listHqPrograms = HqProgram.GetAll().ToList();
        var hqProgram = listHqPrograms.FirstOrDefault(x => x.PluginDllName.Trim() == progCur.PluginDllName.Trim());
        if (hqProgram == null) return false;
        return hqProgram.IsEnabled;
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

    /// <summary>
    ///     This can only be called by the user if it is a program link that they created.  Included program links cannot
    ///     be deleted.  If doing something similar from ClassConversion, must delete any dependent ProgramProperties first.
    ///     It will delete ToolButItems for you.
    /// </summary>
    public static void Delete(Program prog)
    {
        var command = "DELETE from toolbutitem WHERE ProgramNum = " + SOut.Long(prog.ProgramNum);
        Db.NonQ(command);
        command = "DELETE from program WHERE ProgramNum = '" + prog.ProgramNum + "'";
        Db.NonQ(command);
    }

    ///<summary>Returns true if a Program link with the given name or number exists and is enabled. Handles null.</summary>
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

    ///<summary>Returns true if a Program link with the given name exists and is enabled.</summary>
    public static bool IsEnabledNoCache(ProgramName programName)
    {
        var command = $"SELECT Enabled FROM program WHERE ProgName = '{SOut.String(programName.ToString())}'";
        var table = DataCore.GetTable(command);
        var isEnabled = false;
        if (table.Rows.Count > 0) isEnabled = SIn.Bool(table.Rows[0]["Enabled"].ToString());
        return isEnabled;
    }

    ///<summary>Returns the Program of the passed in ProgramNum.  Will be null if a Program is not found.</summary>
    public static Program GetProgram(long programNum)
    {
        return GetFirstOrDefault(x => x.ProgramNum == programNum);
    }

    ///<summary>Supply a valid program Name, and this will set Cur to be the corresponding Program object.</summary>
    public static Program GetCur(ProgramName progName)
    {
        return GetFirstOrDefault(x => x.ProgName == progName.ToString());
    }

    ///<summary>Supply a valid program Name.  Will return 0 if not found.</summary>
    public static long GetProgramNum(ProgramName progName)
    {
        var program = GetCur(progName);
        return program == null ? 0 : program.ProgramNum;
    }

    /// <summary>
    ///     These programs do not work in cloud mode for various reasons. We will restore them as our cloud customers
    ///     request them.
    /// </summary>
    public static List<string> GetListDisabledForWeb()
    {
        return PrefC.GetString(PrefName.ProgramLinksDisabledForWeb).Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>Using eClinicalWorks tight integration.</summary>
    public static bool UsingEcwTightMode()
    {
        if (IsEnabled(ProgramName.eClinicalWorks) && ProgramProperties.GetPropVal(ProgramName.eClinicalWorks, "eClinicalWorksMode") == "0") return true;
        return false;
    }

    /// <summary>Using eClinicalWorks full mode.</summary>
    public static bool UsingEcwFullMode()
    {
        if (IsEnabled(ProgramName.eClinicalWorks) && ProgramProperties.GetPropVal(ProgramName.eClinicalWorks, "eClinicalWorksMode") == "2") return true;
        return false;
    }

    /// <summary>
    ///     Returns true if using eCW in tight or full mode.  In these modes, appointments ARE allowed to overlap because
    ///     we block users from seeing them.
    /// </summary>
    public static bool UsingEcwTightOrFullMode()
    {
        if (UsingEcwTightMode() || UsingEcwFullMode()) return true;
        return false;
    }

    ///<summary>Returns the local override path if available or returns original program path.  Always returns a valid path.</summary>
    public static string GetProgramPath(Program program)
    {
        var overridePath = ProgramProperties.GetLocalPathOverrideForProgram(program.ProgramNum);
        if (overridePath != "") return overridePath;
        return program.Path;
    }

    ///<summary>Returns the local override path if available or returns original program path.  Always returns a valid path.</summary>
    public static string GetProgramPath(ProgramName progName)
    {
        return GetProgramPath(GetFirstOrDefault(x => x.ProgName == progName.ToString()));
    }

    /// <summary>
    ///     Returns true if input program is a static program. Static programs are ones we do not want the user to be able
    ///     to modify in some way.
    /// </summary>
    public static bool IsStatic(Program prog)
    {
        //Currently there is just one static program. As more are created they will need to be added to this check.
        if (prog.ProgName == ProgramName.RapidCall.ToString()) return true;
        return false;
    }

    /// <summary>
    ///     For each enabled bridge, if the bridge uses a file to transmit patient data to the other software, then we need to
    ///     remove the files or clear the files when OD is exiting.
    ///     Required for EHR 2014 module d.7 (as stated by proctor).
    /// </summary>
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

    ///<summary>Needed for Sirona bridge data scrub in ScrubExportedPatientData().</summary>
    [DllImport("kernel32")] //this is the Windows function for reading from ini files.
    private static extern int GetPrivateProfileStringFromIni(string section, string key, string def
        , StringBuilder retVal, int size, string filePath);

    ///<summary>Needed for Sirona bridge data scrub in ScrubExportedPatientData().</summary>
    private static string ReadValueFromIni(string section, string key, string iniFile)
    {
        var strBuild = new StringBuilder(255);
        var i = GetPrivateProfileStringFromIni(section, key, "", strBuild, 255, iniFile);
        return strBuild.ToString();
    }

    /// <summary>
    ///     If isRemovable is false, then the file referenced in the program property will be cleared.
    ///     If isRemovable is true, then the file referenced in the program property will be deleted.
    /// </summary>
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

    ///<summary>Returns true if more than 1 credit card processing program is enabled.</summary>
    public static bool HasMultipleCreditCardProgramsEnabled()
    {
        return new List<bool>
        {
            IsEnabled(ProgramName.EdgeExpress), IsEnabled(ProgramName.Xcharge), IsEnabled(ProgramName.PayConnect), IsEnabled(ProgramName.PaySimple)
        }.Count(x => x) >= 2;
    }

    ///<summary>Called when we want to inform HQ about changes maded to enabled programs.</summary>
    public static void SendEnabledProgramsToHQ()
    {
        CustomerUpdatesProxy.SendAndReceiveUpdateRequestXml(); //Piggy back on this, we don't do anything with result just want to trigger some code.
    }

    /// <summary>Called to delete the linkage.xml file used by EzDenti and Ez3Di.</summary>
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

    /// <summary>
    ///     Returns ProgramName.None if no Imaging AI program is used. If more than one is enabled, defaults to Pearl
    ///     because it is billed externally.
    /// </summary>
    public static ProgramName GetActiveImagingAIProgram()
    {
        if (IsEnabled(ProgramName.Pearl)) return ProgramName.Pearl;
        if (IsEnabled(ProgramName.BetterDiagnostics)) return ProgramName.BetterDiagnostics;
        return ProgramName.None;
    }

    #region Cache Pattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProgramCache _programCache = new();

    public static List<Program> GetListDeep(bool isShort = false)
    {
        return _programCache.GetDeepCopy(isShort);
    }

    public static Program GetFirstOrDefault(Func<Program, bool> match, bool isShort = false)
    {
        return _programCache.GetFirstOrDefault(match, isShort);
    }

    public static List<Program> GetWhere(Predicate<Program> match, bool isShort = false)
    {
        return _programCache.GetWhere(match, isShort);
    }

    public static bool HListIsNull()
    {
        return _programCache.ListIsNull();
    }

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
        _programCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _programCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _programCache.ClearCache();
    }

    #endregion Cache Pattern
}