using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Eclaims;

namespace OpenDentBusiness;

public class Clearinghouses
{
    public static long Insert(Clearinghouse clearinghouse)
    {
        var clearinghouseNum = ClearinghouseCrud.Insert(clearinghouse);
        
        clearinghouse.HqClearinghouseNum = clearinghouseNum;
        ClearinghouseCrud.Update(clearinghouse);
        
        return clearinghouseNum;
    }

    public static void Delete(Clearinghouse clearinghouseHq)
    {
        Db.NonQ("DELETE FROM clearinghouse WHERE ClearinghouseNum = " + clearinghouseHq.ClearinghouseNum);
        Db.NonQ("DELETE FROM clearinghouse WHERE HqClearinghouseNum = " + clearinghouseHq.ClearinghouseNum);
    }

    private class ClearinghouseCache : CacheListAbs<Clearinghouse>
    {
        protected override List<Clearinghouse> GetCacheFromDb()
        {
            var clearinghouses = ClearinghouseCrud.SelectMany("SELECT * FROM clearinghouse WHERE ClinicNum=0 ORDER BY Description");

            clearinghouses.ForEach(x => x.Password = GetRevealPassword(x.Password));

            return clearinghouses;
        }

        protected override List<Clearinghouse> TableToList(DataTable dataTable)
        {
            return ClearinghouseCrud.TableToList(dataTable);
        }

        protected override Clearinghouse Copy(Clearinghouse item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Clearinghouse> items)
        {
            return ClearinghouseCrud.ListToTable(items, "Clearinghouse");
        }

        protected override void FillCacheIfNeeded()
        {
            Clearinghouses.GetTableFromCache(false);
        }

        protected override bool IsInListShort(Clearinghouse item)
        {
            return item.CommBridge != EclaimsCommBridge.MercuryDE;
        }
    }

    private static readonly ClearinghouseCache Cache = new();

    public static List<Clearinghouse> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Clearinghouse GetFirstOrDefault(Func<Clearinghouse, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<Clearinghouse> GetAllNonHq()
    {
        var clearinghouses = ClearinghouseCrud.SelectMany("SELECT * FROM clearinghouse WHERE ClinicNum!=0 ORDER BY Description");

        foreach (var clearinghouse in clearinghouses)
        {
            clearinghouse.Password = GetRevealPassword(clearinghouse.Password);
        }

        return clearinghouses;
    }

    public static Clearinghouse GetDefaultEligibility()
    {
        return GetClearinghouse(PrefC.GetLong(PrefName.ClearinghouseDefaultEligibility));
    }

    public static int GetNextBatchNumber(Clearinghouse clearinghouse)
    {
        var dataTable = DataCore.GetTable("SELECT LastBatchNumber FROM clearinghouse WHERE ClearinghouseNum = " + clearinghouse.HqClearinghouseNum);

        var batchNumber = SIn.Int(dataTable.Rows[0][0].ToString());
        if (clearinghouse.Eformat == ElectronicClaimFormat.Canadian)
        {
            if (batchNumber == 999999)
            {
                batchNumber = 1;
            }
            else
            {
                batchNumber++;
            }
        }
        else
        {
            if (batchNumber == 999)
            {
                batchNumber = 1;
            }
            else
            {
                batchNumber++;
            }
        }

        Db.NonQ("UPDATE clearinghouse SET LastBatchNumber=" + batchNumber + " WHERE ClearinghouseNum = " + clearinghouse.HqClearinghouseNum);

        return batchNumber;
    }

    public static long AutomateClearinghouseHqSelection(string payorId, EnumClaimMedType enumClaimMedType)
    {
        Clearinghouse clearinghouseHq = null;
        
        switch (enumClaimMedType)
        {
            case EnumClaimMedType.Dental when PrefC.GetLong(PrefName.ClearinghouseDefaultDent) == 0:
                return 0;
            
            case EnumClaimMedType.Dental:
                clearinghouseHq = GetClearinghouse(PrefC.GetLong(PrefName.ClearinghouseDefaultDent));
                break;
            
            case EnumClaimMedType.Medical or EnumClaimMedType.Institutional when PrefC.GetLong(PrefName.ClearinghouseDefaultMed) == 0:
            {
                var clearingHouses = GetDeepCopy();

                clearinghouseHq =
                    clearingHouses.Find(x =>
                        x.CommBridge == EclaimsCommBridge.EmdeonMedical &&
                        x.HqClearinghouseNum == x.ClearinghouseNum) ??
                    clearingHouses.Find(x => 
                        x.Eformat == ElectronicClaimFormat.x837_5010_med_inst && 
                        x.HqClearinghouseNum == x.ClearinghouseNum);

                return clearinghouseHq?.ClearinghouseNum ?? 0;
            }
            
            case EnumClaimMedType.Medical or EnumClaimMedType.Institutional:
                clearinghouseHq = GetClearinghouse(PrefC.GetLong(PrefName.ClearinghouseDefaultMed));
                break;
        }

        if (clearinghouseHq is null)
        {
            return 0;
        }

        var clearinghouseOverride = GetClearinghouseByPayorId(payorId);
        if (clearinghouseOverride is null)
        {
            return clearinghouseHq.ClearinghouseNum;
        }

        if (clearinghouseOverride.Eformat is ElectronicClaimFormat.x837D_4010 or ElectronicClaimFormat.x837D_5010_dental or ElectronicClaimFormat.Canadian or ElectronicClaimFormat.Ramq)
        {
            if (enumClaimMedType == EnumClaimMedType.Dental)
            {
                return clearinghouseOverride.ClearinghouseNum;
            }
        }

        if (clearinghouseOverride.Eformat != ElectronicClaimFormat.x837_5010_med_inst)
        {
            return clearinghouseHq.ClearinghouseNum;
        }

        return enumClaimMedType is EnumClaimMedType.Medical or EnumClaimMedType.Institutional 
            ? clearinghouseOverride.ClearinghouseNum 
            : clearinghouseHq.ClearinghouseNum;
    }

    private static Clearinghouse GetClearinghouseByPayorId(string payorId)
    {
        if (string.IsNullOrEmpty(payorId))
        {
            return null;
        }

        var clearinghouses = GetDeepCopy();

        foreach (var clearinghouse in clearinghouses)
        {
            var payorIDs = clearinghouse.Payors.Split(',').ToList();
            if (payorIDs.Contains(payorId))
            {
                return clearinghouse;
            }
        }

        return null;
    }

    public static Clearinghouse GetDefaultDental()
    {
        return GetClearinghouse(PrefC.GetLong(PrefName.ClearinghouseDefaultDent));
    }

    public static Clearinghouse GetClearinghouse(long clearinghouseNum)
    {
        return GetFirstOrDefault(x => x.ClearinghouseNum == clearinghouseNum);
    }

    public static string GetRevealPassword(string concealPassword)
    {
        Class1.RevealClearinghouse(concealPassword, out var revealedPassword);

        return revealedPassword;
    }

    public static Clearinghouse GetForClinic(Clearinghouse clearinghouseHq, long clinicNum)
    {
        if (clinicNum == 0)
        {
            return null;
        }

        var result = ClearinghouseCrud.SelectOne(
            "SELECT * FROM clearinghouse " +
            "WHERE HqClearinghouseNum=" + clearinghouseHq.ClearinghouseNum + " " +
            "AND ClinicNum = " + clinicNum);

        if (result is null)
        {
            return null;
        }

        result.Password = GetRevealPassword(result.Password);

        return result;
    }

    public static void Update(Clearinghouse clearinghouse, Clearinghouse clearinghouseOld)
    {
        ClearinghouseCrud.Update(clearinghouse, clearinghouseOld);
    }

    public static void Sync(List<Clearinghouse> listClearinghousesNew, List<Clearinghouse> listClearinghousesOld)
    {
        ClearinghouseCrud.Sync(listClearinghousesNew, listClearinghousesOld);
    }

    public static Clearinghouse OverrideFields(Clearinghouse clearinghouseHq, long clinicNum)
    {
        var clearinghouseClin = GetForClinic(clearinghouseHq, clinicNum);

        return OverrideFields(clearinghouseHq, clearinghouseClin);
    }

    public static Clearinghouse OverrideFields(Clearinghouse clearinghouseHq, Clearinghouse clearinghouseClin)
    {
        if (clearinghouseHq == null)
        {
            return null;
        }

        var result = clearinghouseHq.Copy();
        if (clearinghouseClin is null)
        {
            return result;
        }

        result.HqClearinghouseNum = clearinghouseClin.HqClearinghouseNum;
        result.ClearinghouseNum = clearinghouseClin.ClearinghouseNum;
        result.ClinicNum = clearinghouseClin.ClinicNum;
        result.IsEraDownloadAllowed = clearinghouseClin.IsEraDownloadAllowed;
        result.IsClaimExportAllowed = clearinghouseClin.IsClaimExportAllowed;

        if (!string.IsNullOrEmpty(clearinghouseClin.ExportPath))
        {
            result.ExportPath = clearinghouseClin.ExportPath;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.SenderTIN))
        {
            result.SenderTIN = clearinghouseClin.SenderTIN;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.Password))
        {
            result.Password = clearinghouseClin.Password;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.ResponsePath))
        {
            result.ResponsePath = clearinghouseClin.ResponsePath;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.ClientProgram))
        {
            result.ClientProgram = clearinghouseClin.ClientProgram;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.LoginID))
        {
            result.LoginID = clearinghouseClin.LoginID;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.SenderName))
        {
            result.SenderName = clearinghouseClin.SenderName;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.SenderTelephone))
        {
            result.SenderTelephone = clearinghouseClin.SenderTelephone;
        }

        if (!string.IsNullOrEmpty(clearinghouseClin.LocationID))
        {
            result.LocationID = clearinghouseClin.LocationID;
        }

        result.IsAttachmentSendAllowed = clearinghouseClin.IsAttachmentSendAllowed;

        return result;
    }

    public static void RetrieveReportsAutomatic(bool isAllClinics)
    {
        List<long> clinicNums;

        if (isAllClinics)
        {
            clinicNums = Clinics.GetDeepCopy(true).Select(x => x.Id).ToList();
            clinicNums.Add(0);
        }
        else
        {
            clinicNums = [Clinics.ClinicNum];
        }

        var result = IsTimeToRetrieveReports(true);

        var isTimeToRetrieve = result.IsSuccess;
        if (isTimeToRetrieve)
        {
            Prefs.UpdateDateT(PrefName.ClaimReportReceiveLastDateTime, DateTime.Now);
        }

        var clearinghouses = GetDeepCopy();
        var clearinghouseNumDefault = PrefC.GetLong(PrefName.ClearinghouseDefaultDent);

        foreach (var clearinghouse in clearinghouses)
        {
            foreach (var clinicNum in clinicNums)
            {
                var clearinghouseClin = OverrideFields(clearinghouse, clinicNum);
                RetrieveReportsAutomaticHelper(clearinghouseClin, clearinghouse, clearinghouseNumDefault, isTimeToRetrieve);
            }
        }
    }

    private static Result IsTimeToRetrieveReports(bool isAutomaticMode, IODProgressExtended progressExtended = null)
    {
        var result = new Result();

        progressExtended ??= new ODProgressExtendedNull();

        var dateTimeLastReport = SIn.DateTime(PrefC.GetStringNoCache(PrefName.ClaimReportReceiveLastDateTime));
        var minutesClaimReportReceiveInternal = SIn.Double(PrefC.GetStringNoCache(PrefName.ClaimReportReceiveInterval));
        var timeToReceive = DateTime.Now.Date + PrefC.GetDateT(PrefName.ClaimReportReceiveTime).TimeOfDay;
        var minutesDiff = DateTime.Now.Subtract(dateTimeLastReport).TotalMinutes;

        result.Msg = "";

        if (isAutomaticMode)
        {
            if (minutesClaimReportReceiveInternal != 0)
            {
                if (minutesDiff < minutesClaimReportReceiveInternal)
                {
                    result.IsSuccess = false;
                    return result;
                }
            }
            else
            {
                if (DateTime.Now.TimeOfDay < timeToReceive.TimeOfDay || dateTimeLastReport.Date == DateTime.Today)
                {
                    result.IsSuccess = false;
                    return result;
                }
            }
        }
        else if (minutesDiff < 1)
        {
            result.Msg = "Reports can only be retrieved once per minute.";

            progressExtended.UpdateProgress("Reports can only be retrieved once per minute. Attempting to import manually downloaded reports.");

            result.IsSuccess = false;
            return result;
        }

        result.IsSuccess = true;
        return result;
    }

    private static void RetrieveReportsAutomaticHelper(Clearinghouse clearinghouseClin, Clearinghouse clearinghouseHq, long clearinghouseNumDefault, bool isTimeToRetrieve)
    {
        if (!Directory.Exists(clearinghouseClin.ResponsePath))
        {
            return;
        }

        if (clearinghouseHq.ClearinghouseNum == clearinghouseNumDefault)
        {
            RetrieveAndImport(clearinghouseClin, true, isTimeToRetrieve: isTimeToRetrieve);
        }
        else if (clearinghouseHq.Eformat == ElectronicClaimFormat.None)
        {
            RetrieveAndImport(clearinghouseClin, true, isTimeToRetrieve: isTimeToRetrieve);
        }
        else if (clearinghouseHq.CommBridge == EclaimsCommBridge.BCBSGA)
        {
            BCBSGA.Retrieve(clearinghouseClin, true, new TerminalConnector());
        }
        else
            switch (clearinghouseHq.Eformat)
            {
                case ElectronicClaimFormat.Canadian when CultureInfo.CurrentCulture.Name.EndsWith("CA"):
                {
                    var providers = Providers.GetDeepCopy(true);
                    var officeNums = new List<string>();

                    foreach (var provider in providers)
                    {
                        if (!provider.IsCDAnet || provider.NationalProvID == "" || provider.CanadianOfficeNum == "")
                        {
                            continue;
                        }

                        if (officeNums.Contains(provider.CanadianOfficeNum))
                        {
                            continue;
                        }

                        officeNums.Add(provider.CanadianOfficeNum);
                        try
                        {
                            CanadianOutput.GetOutstandingForDefault(provider);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    break;
                }

                case ElectronicClaimFormat.Dutch when CultureInfo.CurrentCulture.Name.EndsWith("DE"):
                    RetrieveAndImport(clearinghouseClin, true, isTimeToRetrieve: isTimeToRetrieve);
                    break;

                default:
                {
                    if (clearinghouseHq.Eformat != ElectronicClaimFormat.Canadian && clearinghouseHq.Eformat != ElectronicClaimFormat.Dutch && CultureInfo.CurrentCulture.Name.EndsWith("US"))
                    {
                        RetrieveAndImport(clearinghouseClin, true, isTimeToRetrieve: isTimeToRetrieve);
                    }

                    break;
                }
            }
    }

    private static string RetrieveReports(Clearinghouse clearinghouseClin, bool isAutomaticMode, IODProgressExtended progressExtended = null)
    {
        progressExtended ??= new ODProgressExtendedNull();
        progressExtended.UpdateProgress("Beginning report retrieval...", "reports", "0%");

        if (progressExtended.IsPauseOrCancel())
        {
            return "Process canceled by user.";
        }

        if (clearinghouseClin.ISA08 == "113504607")
        {
            return "";
        }

        switch (clearinghouseClin.CommBridge)
        {
            case EclaimsCommBridge.None or EclaimsCommBridge.Renaissance or EclaimsCommBridge.RECS:
                return "";

            case EclaimsCommBridge.WebMD when !WebMD.Launch(clearinghouseClin, 0, isAutomaticMode, progressExtended):
                return "Error retrieving.\r\n" + WebMD.ErrorMessage;

            case EclaimsCommBridge.BCBSGA when !BCBSGA.Retrieve(clearinghouseClin, true, new TerminalConnector(), progressExtended):
                return "Error retrieving.\r\n" + BCBSGA.ErrorMessage;

            case EclaimsCommBridge.ClaimConnect when !Directory.Exists(clearinghouseClin.ResponsePath):
            {
                if (isAutomaticMode)
                {
                    return "";
                }

                break;
            }

            case EclaimsCommBridge.ClaimConnect when !ClaimConnect.Retrieve(clearinghouseClin, progressExtended):
            {
                if (!ClaimConnect.ErrorMessage.Contains(": 150\r\n"))
                {
                    return "Error retrieving.\r\n" + ClaimConnect.ErrorMessage;
                }

                if (isAutomaticMode)
                {
                    return "";
                }

                try
                {
                    Process.Start(@"http://www.dentalxchange.com");
                }
                catch
                {
                    return "Could not locate the site.";
                }

                return "Error retrieving.\r\n" + ClaimConnect.ErrorMessage;
            }

            case EclaimsCommBridge.AOS:
                try
                {
                    Process.Start(@"C:\Program files\AOS\AOSCommunicator\AOSCommunicator.exe");
                }
                catch
                {
                    return "Could not locate the file.";
                }

                break;

            case EclaimsCommBridge.MercuryDE when !MercuryDE.Launch(clearinghouseClin, 0, progressExtended):
                return "Error retrieving.\r\n" + MercuryDE.ErrorMessage;

            case EclaimsCommBridge.EmdeonMedical when !EmdeonMedical.Retrieve(clearinghouseClin, progressExtended):
                return "Error retrieving.\r\n" + EmdeonMedical.ErrorMessage;

            case EclaimsCommBridge.DentiCal when !DentiCal.Launch(clearinghouseClin, 0, progressExtended):
                return "Error retrieving.\r\n" + DentiCal.ErrorMessage;

            case EclaimsCommBridge.EDS:
            {
                var listEdsErrors = new List<string>();
                if (!EDS.Retrieve277s(clearinghouseClin, progressExtended))
                {
                    listEdsErrors.Add("Error retrieving.\r\n" + EDS.ErrorMessage);
                }

                if (!EDS.Retrieve835s(clearinghouseClin, progressExtended))
                {
                    listEdsErrors.Add("Error retrieving.\r\n" + EDS.ErrorMessage);
                }

                if (listEdsErrors.Count > 0)
                {
                    return string.Join("\r\n", listEdsErrors);
                }

                break;
            }

            case EclaimsCommBridge.Lantek:
                try
                {
                    Process.Start(@"C:\Lantek\Program\Trakker.exe");
                }
                catch
                {
                    return "Could not locate the file.";
                }

                break;
        }

        return "";
    }

    private static string ImportReportFiles(Clearinghouse clearinghouseClin, IODProgressExtended progressExtended = null)
    {
        progressExtended ??= new ODProgressExtendedNull();

        if (!Directory.Exists(clearinghouseClin.ResponsePath))
        {
            return "Report directory does not exist: " +
                   clearinghouseClin.ResponsePath + "\r\n" +
                   "Go to Setup, Family/Insurance, Clearinghouses, and double-click the desired clearinghouse to update the path.";
        }

        if (clearinghouseClin.Eformat == ElectronicClaimFormat.Canadian || clearinghouseClin.Eformat == ElectronicClaimFormat.Ramq)
        {
            return "";
        }

        progressExtended.UpdateProgress("Reading download files", "reports", "55%", 55);
        if (progressExtended.IsPauseOrCancel())
        {
            return "Import canceled by user.";
        }

        List<string> files;
        string path;

        try
        {
            files = Directory.GetFiles(clearinghouseClin.ResponsePath).ToList();

            path = Path.Combine(clearinghouseClin.ResponsePath, "Archive" + "_" + DateTime.Now.Year);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return "Access to the Report Path is denied.  Try running as administrator or contact your network administrator.";
        }

        var filesFailedToMove = new List<string>();
        var filesFailedToImport = new List<string>();

        progressExtended.UpdateProgress("Files read.");
        progressExtended.UpdateProgress("Importing files", "reports", "83%", 83);
        if (files.Count > 0)
        {
            progressExtended.UpdateProgressDetailed("Importing", tagString: "import");
        }
        else
        {
            progressExtended.UpdateProgress("No files to import.");
        }

        for (var i = 0; i < files.Count; i++)
        {
            var percentUpdated = i / files.Count * 100;

            progressExtended.UpdateProgress("Importing " + i + " / " + files.Count, "import", percentUpdated + "%", percentUpdated);

            if (progressExtended.IsPauseOrCancel())
            {
                return "Import canceled by user.";
            }

            var pathToFileSource = files[i];
            var pathToFileDestination = ODFileUtils.CombinePaths(path, Path.GetFileName(files[i]));
            try
            {
                File.Move(pathToFileSource, pathToFileDestination);
            }
            catch
            {
                filesFailedToMove.Add(pathToFileSource);

                continue;
            }

            try
            {
                Etranss.ProcessIncomingReport(
                    File.GetCreationTime(pathToFileDestination),
                    clearinghouseClin.HqClearinghouseNum,
                    File.ReadAllText(pathToFileDestination),
                    Security.CurUser.UserNum);
            }
            catch
            {
                filesFailedToImport.Add(pathToFileSource);

                File.Move(pathToFileDestination, pathToFileSource);
            }
        }

        var errorMessage = "";
        if (filesFailedToMove.Count > 0)
        {
            errorMessage = "Failed to move the following files to archive folder due to permission issues or duplicate file names:\r\n" + string.Join(",\r\n", filesFailedToMove);
        }

        if (filesFailedToImport.Count > 0)
        {
            errorMessage += "\r\n\r\nFailed to process following files due to malformed data:\r\n" + string.Join(",\r\n", filesFailedToImport);
        }

        return errorMessage;
    }

    public static string RetrieveAndImport(Clearinghouse clearinghouse, bool isAutomaticMode, IODProgressExtended progressExtended = null, bool isTimeToRetrieve = false)
    {
        progressExtended ??= new ODProgressExtendedNull();

        var result = IsTimeToRetrieveReports(isAutomaticMode, progressExtended);
        var errorMessage = result.Msg;

        var doRetrieveReports = isTimeToRetrieve || (!isAutomaticMode && result.IsSuccess);
        if (doRetrieveReports)
        {
            if (!isAutomaticMode)
            {
                Prefs.UpdateDateT(PrefName.ClaimReportReceiveLastDateTime, DateTime.Now);
            }

            errorMessage = RetrieveReports(clearinghouse, isAutomaticMode, progressExtended);

            if (errorMessage != "")
            {
                progressExtended.UpdateProgress("Error getting reports, attempting to import manually downloaded reports.");
            }

            progressExtended.UpdateProgress("Report retrieval successful. Attempting to import.");
        }

        if (isAutomaticMode && clearinghouse.ResponsePath.Trim() == "")
        {
            return "";
        }

        if (progressExtended.IsPauseOrCancel())
        {
            progressExtended.UpdateProgress("Canceled by user.");
            return errorMessage;
        }

        var importErrors = ImportReportFiles(clearinghouse, progressExtended);
        if (!string.IsNullOrWhiteSpace(importErrors))
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = importErrors;

                progressExtended.UpdateProgress("Error importing.");
            }
            else
            {
                errorMessage += "\r\n" + importErrors;
            }
        }

        if (string.IsNullOrWhiteSpace(errorMessage) && string.IsNullOrWhiteSpace(importErrors))
        {
            progressExtended.UpdateProgress("Import successful.");
        }

        return errorMessage;
    }

    public static string CheckClearinghouseDefaults()
    {
        if (PrefC.GetLong(PrefName.ClearinghouseDefaultDent) == 0)
        {
            return "No default dental clearinghouse defined.";
        }

        if (PrefC.GetBool(PrefName.ShowFeatureMedicalInsurance) && PrefC.GetLong(PrefName.ClearinghouseDefaultMed) == 0)
        {
            return "No default medical clearinghouse defined.";
        }

        return "";
    }

    public static void SyncOverridesForClinic(ref List<Clearinghouse> listClearinghousesOverrides, Clearinghouse clearinghouseNew)
    {
        if (clearinghouseNew.ClinicNum == 0)
        {
            return;
        }

        for (var i = 0; i < listClearinghousesOverrides.Count; i++)
        {
            if (listClearinghousesOverrides[i].HqClearinghouseNum != clearinghouseNew.HqClearinghouseNum ||
                listClearinghousesOverrides[i].ClinicNum != clearinghouseNew.ClinicNum)
            {
                continue;
            }

            var clearinghouseNumOverride = listClearinghousesOverrides[i].ClearinghouseNum;

            listClearinghousesOverrides[i] = clearinghouseNew.Copy();
            listClearinghousesOverrides[i].ClearinghouseNum = clearinghouseNumOverride;
        }
    }

    public static bool IsDisabledForWeb(Clearinghouse clearinghouse)
    {
        return IsDisabledForWeb(clearinghouse.Eformat, clearinghouse.CommBridge);
    }

    public static bool IsDisabledForWeb(ElectronicClaimFormat electronicClaimFormat, EclaimsCommBridge eclaimsCommBridge)
    {
        return electronicClaimFormat is ElectronicClaimFormat.Renaissance or ElectronicClaimFormat.Canadian || eclaimsCommBridge == EclaimsCommBridge.WebMD;
    }
}