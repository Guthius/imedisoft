using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDental.Cloud.Shared;
using OpenDental.Cloud.Storage;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TsiTransLogs
{
    #region Get Methods

    ///<summary>Returns all tsitranslogs for the patients in listPatNums.  Returns empty list if listPatNums is empty or null.</summary>
    public static List<TsiTransLog> SelectMany(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count < 1) return new List<TsiTransLog>();

        var command = "SELECT * FROM tsitranslog "
                      + "WHERE PatNum IN (" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")";
        return TsiTransLogCrud.SelectMany(command);
    }

    ///<summary>Returns all tsitranslogs for all patients.  Used in FormTsiHistory only.</summary>
    public static List<TsiTransLog> GetAll()
    {
        var command = "SELECT * FROM tsitranslog ORDER BY TransDateTime DESC";
        return TsiTransLogCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of PatNums for guars who have a TsiTransLog with type SS (suspend) less than 50 days ago who don't
    ///     have a TsiTransLog
    ///     with type CN (cancel), PF (paid in full), PT (paid in full, thank you), or PL (placement) with a more recent date,
    ///     since this would change the
    ///     account status from suspended to either closed/canceled or if the more recent message had type PL (placement) back
    ///     to active.
    /// </summary>
    public static List<long> GetSuspendedGuarNums()
    {
        var listStatusTransTypes = new List<int>();
        listStatusTransTypes.Add((int) TsiTransType.SS);
        listStatusTransTypes.Add((int) TsiTransType.CN);
        listStatusTransTypes.Add((int) TsiTransType.RI);
        listStatusTransTypes.Add((int) TsiTransType.PF);
        listStatusTransTypes.Add((int) TsiTransType.PT);
        listStatusTransTypes.Add((int) TsiTransType.PL);
        var command = "SELECT DISTINCT tsitranslog.PatNum "
                      + "FROM tsitranslog "
                      + "INNER JOIN ("
                      + "SELECT PatNum,MAX(TransDateTime) transDateTime "
                      + "FROM tsitranslog "
                      + "WHERE TransType IN(" + string.Join(",", listStatusTransTypes) + ") "
                      + "AND TransDateTime>" + SOut.DateT(DateTime.Now.AddDays(-50)) + " "
                      + "GROUP BY PatNum"
                      + ") mostRecentTrans ON tsitranslog.PatNum=mostRecentTrans.PatNum "
                      + "AND tsitranslog.TransDateTime=mostRecentTrans.transDateTime "
                      + "WHERE tsitranslog.TransType=" + (int) TsiTransType.SS;
        return Db.GetListLong(command);
    }

    public static bool IsGuarSuspended(long guarNum)
    {
        var listStatusTransTypes = new List<int>();
        listStatusTransTypes.Add((int) TsiTransType.SS);
        listStatusTransTypes.Add((int) TsiTransType.CN);
        listStatusTransTypes.Add((int) TsiTransType.RI);
        listStatusTransTypes.Add((int) TsiTransType.PF);
        listStatusTransTypes.Add((int) TsiTransType.PT);
        listStatusTransTypes.Add((int) TsiTransType.PL);
        var command = "SELECT (CASE WHEN tsitranslog.TransType=" + (int) TsiTransType.SS + " THEN 1 ELSE 0 END) isGuarSuspended "
                      + "FROM tsitranslog "
                      + "INNER JOIN ("
                      + "SELECT PatNum,MAX(TransDateTime) transDateTime "
                      + "FROM tsitranslog "
                      + "WHERE PatNum=" + SOut.Long(guarNum) + " "
                      + "AND TransType IN(" + string.Join(",", listStatusTransTypes) + ") "
                      + "AND TransDateTime>" + SOut.DateT(DateTime.Now.AddDays(-50)) + " "
                      + "GROUP BY PatNum"
                      + ") mostRecentLog ON tsitranslog.PatNum=mostRecentLog.PatNum AND tsitranslog.TransDateTime=mostRecentLog.transDateTime";
        return SIn.Bool(DataCore.GetScalar(command));
    }

    #endregion Get Methods

    #region Insert

    public static long Insert(TsiTransLog tsiTransLog)
    {
        return TsiTransLogCrud.Insert(tsiTransLog);
    }

    public static void InsertMany(List<TsiTransLog> listTsiTransLogs)
    {
        TsiTransLogCrud.InsertMany(listTsiTransLogs);
    }

    private static void InsertTsiLogsForAdjustment(long patGuar, Adjustment adjustment, string msgText, TsiTransType tsiTransType)
    {
        //insert tsitranslog for this transaction so the ODService won't send it to Transworld.  _isTsiAdj means Transworld received a payment on
        //behalf of this guar and took a percentage and send the rest to the office for the account.  This will result in a payment being entered
        //into the account, having been received from Transworld, and an adjustment to account for Transorld's cut.
        var patAging = Patients.GetAgingListFromGuarNums(new List<long> {patGuar}).FirstOrDefault(); //should only ever be 1
        if (patAging == null) return;

        var offsetAmt = adjustment.AdjAmt - patAging.ListTsiLogs.FindAll(x => x.FKeyType == TsiFKeyType.Adjustment && x.FKey == adjustment.AdjNum).Sum(x => x.TransAmt);
        if (CompareDouble.IsZero(offsetAmt)) return;

        var balFromMsgs = GetBalFromMsgs(patAging);
        if (CompareDouble.IsZero(balFromMsgs)) return;

        var tsiTransLog = new TsiTransLog();
        tsiTransLog.PatNum = patAging.PatNum;
        tsiTransLog.UserNum = Security.CurUser.UserNum;
        tsiTransLog.TransType = tsiTransType;
        //tsiTransLog.TransDateTime=DateTime.Now;//set on insert, not editable by user
        //tsiTransLog.ServiceType=TsiServiceType.Accelerator;//only valid for placement msgs
        //tsiTransLog.ServiceCode=TsiServiceCode.Diplomatic;//only valid for placement msgs
        tsiTransLog.ClientId = patAging.ListTsiLogs.FirstOrDefault()?.ClientId ?? ""; //can be blank, not used since this isn't really sent to Transworld
        tsiTransLog.TransAmt = offsetAmt;
        tsiTransLog.AccountBalance = balFromMsgs + offsetAmt;
        if (tsiTransType == TsiTransType.Excluded) tsiTransLog.AccountBalance = balFromMsgs;

        tsiTransLog.FKeyType = TsiFKeyType.Adjustment;
        tsiTransLog.FKey = adjustment.AdjNum;
        tsiTransLog.RawMsgText = msgText;
        //tsi.Translog.TransJson="";//only valid for placement msgs
        tsiTransLog.ClinicNum = 0;
        if (true) tsiTransLog.ClinicNum = patAging.ClinicNum;

        Insert(tsiTransLog);
    }

    /// <summary>Inserts a TsiTransLog for the adjustment if necessary.</summary>
    public static void CheckAndInsertLogsIfAdjTypeExcluded(Adjustment adjustment, bool isFromTsi = false)
    {
        var program = Programs.GetCur(ProgramName.Transworld);
        if (program == null || !program.Enabled) return;

        var patientGuar = Patients.GetGuarForPat(adjustment.PatNum);
        if (patientGuar == null || !IsTransworldEnabled(patientGuar.ClinicNum) || !Patients.IsGuarCollections(patientGuar.PatNum)) return;

        var msgText = "This was not a message sent to Transworld.  This adjustment was entered due to a payment received from Transworld.";
        var tsiTransType = TsiTransType.None;
        var listProgramProperties = ProgramProperties
            .GetWhere(x => x.ProgramNum == program.ProgramNum
                           && (x.PropertyDesc == ProgramProperties.PropertyDescs.TransWorld.SyncExcludePosAdjType
                               || x.PropertyDesc == ProgramProperties.PropertyDescs.TransWorld.SyncExcludeNegAdjType));
        //use guar's clinic if clinics are enabled and props for that clinic exist, otherwise use ClinicNum 0
        var listProgramPropertiesForClinic = listProgramProperties.FindAll(x => x.ClinicNum == patientGuar.ClinicNum);
        long clinicNum = 0;
        if (true && listProgramPropertiesForClinic.Count > 0)
            clinicNum = patientGuar.ClinicNum;
        else
            listProgramPropertiesForClinic = listProgramProperties.FindAll(x => x.ClinicNum == 0);

        if (listProgramPropertiesForClinic.Count != 0 //should always be props for ClinicNum 0
            && listProgramPropertiesForClinic.Any(x => SIn.Long(x.PropertyValue, false) == adjustment.AdjType))
        {
            //If this adjustment is an excluded type, mark it excluded regardless of if the adjustment is from TSI.
            //This means if an adjustment is created for a Collections patient using the "No - this adjustment is the result
            //of a payment received from TSI" option and the adjustment is marked as an excluded type then the previous
            //decision will be effectively overridden to to behave as though the adjustment was applied by the office.
            msgText = "Adjustment type is set to excluded type from transworld program properties.";
            tsiTransType = TsiTransType.Excluded;
        }
        else if (!isFromTsi)
        {
            return; //if this adjustment is not an excluded type and not from TSI, return
        }

        InsertTsiLogsForAdjustment(patientGuar.PatNum, adjustment, msgText, tsiTransType);
    }

    #endregion Insert

    #region Misc Methods

    /// <summary>Getting the balance from the messages from the patAging object using logs.</summary>
    public static double GetBalFromMsgs(PatAging patAging)
    {
        var tsiTransLog = patAging.ListTsiLogs.FirstOrDefault(x => x.TransType == TsiTransType.PL);
        if (tsiTransLog == null) return 0; //should never happen, this is a collection guarantor so there must be a placement log

        var balFromMsgs = tsiTransLog.AccountBalance
                          + patAging.ListTsiLogs
                              .Where(x => x.TransDateTime > tsiTransLog.TransDateTime
                                          && !x.TransType.In(TsiTransType.PL, TsiTransType.RI, TsiTransType.SS, TsiTransType.CN, TsiTransType.Agg, TsiTransType.Excluded))
                              .Sum(x => x.TransAmt);
        return balFromMsgs;
    }

    ///<summary>Returns true if the guarantor has been sent to TSI and has not been canceled or paid in full.</summary>
    public static bool HasGuarBeenSentToTSI(Patient patient)
    {
        if (patient == null || !IsTransworldEnabled(patient.ClinicNum)) return false;

        var listTsiTransTypes = new List<TsiTransType>();
        listTsiTransTypes.Add(TsiTransType.SS);
        listTsiTransTypes.Add(TsiTransType.CN);
        listTsiTransTypes.Add(TsiTransType.RI);
        listTsiTransTypes.Add(TsiTransType.PF);
        listTsiTransTypes.Add(TsiTransType.PT);
        listTsiTransTypes.Add(TsiTransType.PL);
        var tsiTransLogRecent = SelectMany(new List<long> {patient.Guarantor}).FindAll(x => listTsiTransTypes.Contains(x.TransType))
            .OrderBy(x => x.TransDateTime).LastOrDefault();
        if (tsiTransLogRecent == null) return false; //Not being managed by TSI

        //Check if the most recent log is of type SS, PL, or RI. 
        return tsiTransLogRecent.TransType.In(TsiTransType.SS, TsiTransType.PL, TsiTransType.RI);
    }

    public static bool ValidateClinicSftpDetails(List<ProgramProperty> listProgramProperties, bool hasConnection = true)
    {
        if (listProgramProperties == null || listProgramProperties.Count == 0) return false;

        var sftpAddress = listProgramProperties.Find(x => x.PropertyDesc == "SftpServerAddress")?.PropertyValue;
        int sftpPort;
        if (!int.TryParse(listProgramProperties.Find(x => x.PropertyDesc == "SftpServerPort")?.PropertyValue, out sftpPort)
            || sftpPort < ushort.MinValue //0
            || sftpPort > ushort.MaxValue) //65,535
            sftpPort = 22; //default to port 22

        var userName = listProgramProperties.Find(x => x.PropertyDesc == "SftpUsername")?.PropertyValue;
        var userPassword = Class1.TryDecrypt(listProgramProperties.Find(x => x.PropertyDesc == "SftpPassword")?.PropertyValue);
        if (string.IsNullOrWhiteSpace(sftpAddress) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userPassword)) return false;

        var stringArraySelectedServices = listProgramProperties.FirstOrDefault(x => x.PropertyDesc == "SelectedServices")
            ?.PropertyValue
            ?.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        if (stringArraySelectedServices.IsNullOrEmpty())
            //must have at least one service selected, i.e. Accelerator, Profit Recovery, and/or Collection
            return false;

        if (hasConnection) return Sftp.IsConnectionValid(sftpAddress, userName, userPassword, sftpPort);

        return true;
    }

    public static bool IsTransworldEnabled(long clinicNum)
    {
        var program = Programs.GetCur(ProgramName.Transworld);
        if (program == null || !program.Enabled) return false;

        var listProgramProperties = ProgramProperties.GetForProgram(program.ProgramNum);
        if (listProgramProperties.Count == 0) return false;

        var listProgramPropertiesForClinic = listProgramProperties.FindAll(x => x.ClinicNum == clinicNum);
        if (true && listProgramPropertiesForClinic.Count > 0) return ValidateClinicSftpDetails(listProgramPropertiesForClinic, false);

        listProgramPropertiesForClinic = listProgramProperties.FindAll(x => x.ClinicNum == 0);
        if (listProgramPropertiesForClinic.Count > 0) return ValidateClinicSftpDetails(listProgramPropertiesForClinic, false);

        return false;
    }

    /// <summary>
    ///     Sends an SFTP message to TSI to suspend the account for the guarantor passed in.  Returns empty string if
    ///     successful.
    ///     Returns a translated error message that should be displayed to the user if anything goes wrong.
    /// </summary>
    public static string SuspendGuar(Patient patient)
    {
        var patAging = Patients.GetAgingListFromGuarNums(new List<long> {patient.PatNum}).FirstOrDefault();
        if (patAging == null)
            //this would only happen if the patient was not in the db??, just in case
            return Lans.g("TsiTransLogs", "An error occurred when trying to send a suspend message to TSI.");

        long clinicNum = 0;
        if (true) clinicNum = patient.ClinicNum;

        var program = Programs.GetCur(ProgramName.Transworld);
        if (program == null)
            //shouldn't be possible, the program link should always exist, just in case
            return Lans.g("TsiTransLogs", "The Transworld program link does not exist.  Contact support.");

        var listProgramPropertiesAll = ProgramProperties.GetForProgram(program.ProgramNum);
        if (listProgramPropertiesAll.Count == 0)
            //shouldn't be possible, there should always be a set of props for ClinicNum 0 even if disabled, just in case
            return Lans.g("TsiTransLogs", "The Transworld program link is not setup properly.");

        var listProgramPropertiesClinic = listProgramPropertiesAll.FindAll(x => x.ClinicNum == clinicNum);
        var listProgramPropertiesClinicZero = listProgramPropertiesAll.FindAll(x => x.ClinicNum == 0);
        if (true && listProgramPropertiesClinic.Count == 0 && listProgramPropertiesClinicZero.Count > 0)
        {
            clinicNum = 0;
            listProgramPropertiesClinic = listProgramPropertiesClinicZero;
        }

        var clinicDesc = Clinics.GetDesc(clinicNum);
        if (clinicNum == 0) clinicDesc = "Headquarters";

        if (listProgramPropertiesClinic.Count == 0
            || !ValidateClinicSftpDetails(listProgramPropertiesClinic)) //the props should be valid, but this will test the connection using the props
            return Lans.g("TsiTransLogs", "The Transworld program link is not enabled") + " "
                                                                                        + (true ? Lans.g("TsiTransLogs", "for the guarantor's clinic") + ", " + clinicDesc + ", " : "")
                                                                                        + Lans.g("TsiTransLogs", "or is not setup properly.");

        var defBillTypeNew = Defs.GetDef(DefCat.BillingTypes, PrefC.GetLong(PrefName.TransworldPaidInFullBillingType));
        if (defBillTypeNew == null)
            return Lans.g("TsiTransLogs", "The default paid in full billing type is not set.  An automated suspend message cannot be sent until the "
                                          + "default paid in full billing type is set in the Transworld program link")
                   + (true ? " " + Lans.g("TsiTransLogs", "for the guarantor's clinic") + ", " + clinicDesc : "") + ".";

        var clientId = "";
        if (patAging.ListTsiLogs.Count > 0) clientId = patAging.ListTsiLogs[0].ClientId;

        if (string.IsNullOrEmpty(clientId)) clientId = listProgramPropertiesClinic.Find(x => x.PropertyDesc == "ClientIdAccelerator")?.PropertyValue;

        if (string.IsNullOrEmpty(clientId)) clientId = listProgramPropertiesClinic.Find(x => x.PropertyDesc == "ClientIdCollection")?.PropertyValue;

        if (string.IsNullOrEmpty(clientId))
            return Lans.g("TsiTransLogs", "There is no client ID in the Transworld program link")
                   + (true ? " " + Lans.g("TsiTransLogs", "for the guarantor's clinic") + ", " + clinicDesc : "") + ".";

        var sftpAddress = listProgramPropertiesClinic.Find(x => x.PropertyDesc == "SftpServerAddress")?.PropertyValue ?? "";
        int sftpPort;
        if (!int.TryParse(listProgramPropertiesClinic.Find(x => x.PropertyDesc == "SftpServerPort")?.PropertyValue ?? "", out sftpPort)) sftpPort = 22; //default to port 22

        var userName = listProgramPropertiesClinic.Find(x => x.PropertyDesc == "SftpUsername")?.PropertyValue ?? "";
        var userPassword = Class1.TryDecrypt(listProgramPropertiesClinic.Find(x => x.PropertyDesc == "SftpPassword")?.PropertyValue ?? "");
        if (new[] {sftpAddress, userName, userPassword}.Any(x => string.IsNullOrEmpty(x)))
            return Lans.g("TsiTransLogs", "The SFTP address, username, or password for the Transworld program link") + " "
                                                                                                                     + (true ? Lans.g("TsiTransLogs", "for the guarantor's clinic") + ", " + clinicDesc + ", " : "") + Lans.g("TsiTransLogs", "is blank.");

        var msg = TsiMsgConstructor.GenerateUpdate(patAging.PatNum, clientId, TsiTransType.SS, 0.00, patAging.AmountDue);
        var byteArrayFileContents = Encoding.ASCII.GetBytes(TsiMsgConstructor.GetUpdateFileHeader() + "\r\n" + msg);
        TaskStateUpload taskStateUpload = new Sftp.Upload(sftpAddress, userName, userPassword, sftpPort);
        taskStateUpload.Folder = "/xfer/incoming";
        taskStateUpload.FileName = "TsiUpdates_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
        taskStateUpload.ByteArray = byteArrayFileContents;
        taskStateUpload.HasExceptions = true;
        try
        {
            taskStateUpload.Execute();
        }
        catch (Exception ex)
        {
            return Lans.g("TsiTransLogs", "There was an error sending the update message to Transworld")
                   + (true ? " " + Lans.g("TsiTransLogs", "using the program properties for the guarantor's clinic") + ", " + clinicDesc : "") + ".\r\n"
                   + ex.Message;
        }

        //Upload was successful
        var tsiTransLog = new TsiTransLog();
        tsiTransLog.PatNum = patAging.PatNum;
        tsiTransLog.UserNum = Security.CurUser.UserNum;
        tsiTransLog.TransType = TsiTransType.SS;
        //tsiTransLog.TransDateTime=DateTime.Now;//set on insert, not editable by user
        //tsiTransLog.ServiceType=TsiServiceType.Accelerator;//only valid for placement msgs
        //tsiTransLog.ServiceCode=TsiServiceCode.Diplomatic;//only valid for placement msgs
        tsiTransLog.ClientId = clientId;
        tsiTransLog.TransAmt = 0.00;
        tsiTransLog.AccountBalance = patAging.AmountDue;
        tsiTransLog.FKeyType = TsiFKeyType.None; //only used for account trans updates
        tsiTransLog.FKey = 0; //only used for account trans updates
        tsiTransLog.RawMsgText = msg;
        tsiTransLog.ClinicNum = clinicNum;
        //tsiTransLog.TransJson="";//only valid for placement msgs
        Insert(tsiTransLog);
        //update family billing type to the paid in full billing type pref
        var listPatientsAll = Patients.GetFamily(patAging.Guarantor).ListPats.ToList();
        Patients.UpdateFamilyBillingType(defBillTypeNew.DefNum, patAging.PatNum);
        for (var i = 0; i < listPatientsAll.Count; i++)
        {
            if (listPatientsAll[i].BillingType == defBillTypeNew.DefNum) continue;

            var logTxt = "Patient billing type changed from '" + Defs.GetName(DefCat.BillingTypes, listPatientsAll[i].BillingType) + "' to '" + defBillTypeNew.ItemName
                         + "' due to a status update message being sent to Transworld from the account module.";
            SecurityLogs.MakeLogEntry(EnumPermType.PatientBillingEdit, listPatientsAll[i].PatNum, logTxt);
        }

        return "";
    }

    #endregion Misc Methods
}