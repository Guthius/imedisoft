using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;
using OpenDentBusiness.Misc;

namespace OpenDentBusiness;


public class Claims
{
    #region Get Methods

    /// <summary>
    ///     Returns a list of outstanding ClaimPaySplits for a given provider.
    ///     It will only get outstanding claims with a date of service past dateTerm.
    /// </summary>
    public static List<ClaimPaySplit> GetOutstandingClaimsByProvider(long provNum, DateTime dateTerm)
    {
        var command = "SELECT claim.ClaimNum,claim.PatNum,claim.ClaimStatus,claim.ClinicNum,claim.DateService,claim.ProvTreat,"
                      + "claim.ClaimFee feeBilled_," + DbHelper.Concat("patient.LName", "', '", "patient.FName") + " patName_,carrier.CarrierName,clinic.Description, "
                      + "0 ClaimPaymentNum,0 insPayAmt_,claim.ClaimIdentifier,0 PaymentRow " //These values are not used in the consuming method.
                      + "FROM claim "
                      + "LEFT JOIN patient ON claim.PatNum = patient.PatNum "
                      + "LEFT JOIN insplan ON claim.PlanNum = insplan.PlanNum "
                      + "LEFT JOIN carrier ON insplan.CarrierNum = carrier.CarrierNum "
                      + "LEFT JOIN clinic ON clinic.ClinicNum = claim.ClinicNum "
                      + "WHERE (claim.ProvBill = " + SOut.Long(provNum) + " "
                      + "OR claim.ProvTreat = " + SOut.Long(provNum) + " "
                      + "OR claim.ProvOrderOverride = " + SOut.Long(provNum) + ") "
                      + "AND claim.ClaimStatus != 'R' "
                      + "AND claim.DateService > " + SOut.Date(dateTerm) + " "
                      + "GROUP BY claim.ClaimNum "
                      + "ORDER BY clinic.Description,patient.PatNum";
        return ClaimPaySplitTableToList(DataCore.GetTable(command));
    }

    #endregion

    /// <summary>
    ///     Gets claimpaysplits attached to a claimpayment with the associated patient, insplan, and carrier. If
    ///     showUnattached it also shows all claimpaysplits that have not been attached to a claimpayment. Pass (0,true) to
    ///     just get all unattached (outstanding) claimpaysplits.
    /// </summary>
    public static List<ClaimPaySplit> RefreshByCheckOld(long claimPaymentNum, bool showUnattached)
    {
        var command =
            "SELECT claim.DateService,claim.ProvTreat,CONCAT(CONCAT(patient.LName,', '),patient.FName) patName_" //Changed from \"_patName\" to patName_ for MySQL 5.5. Also added checks for #<table> and $<table>
            + ",carrier.CarrierName,SUM(claimproc.FeeBilled) feeBilled_,SUM(claimproc.InsPayAmt) insPayAmt_,claim.ClaimNum"
            + ",claimproc.ClaimPaymentNum,(SELECT clinic.Description FROM clinic WHERE claimproc.ClinicNum = clinic.ClinicNum) Description,claim.PatNum,PaymentRow,claim.ClaimStatus,claim.ClaimIdentifier "
            + " FROM claim,patient,insplan,carrier,claimproc"
            + " WHERE claimproc.ClaimNum = claim.ClaimNum"
            + " AND patient.PatNum = claim.PatNum"
            + " AND insplan.PlanNum = claim.PlanNum"
            + " AND insplan.CarrierNum = carrier.CarrierNum"
            + " AND (claimproc.Status = '1' OR claimproc.Status = '4' OR claimproc.Status=5)" //received or supplemental or capclaim
            + " AND (claimproc.ClaimPaymentNum = '" + SOut.Long(claimPaymentNum) + "'";
        if (showUnattached) command += " OR (claimproc.InsPayAmt != 0 AND claimproc.ClaimPaymentNum = '0')";
        //else shows only items attached to this payment
        command += ")"
                   + " GROUP BY claim.DateService,claim.ProvTreat,CONCAT(CONCAT(patient.LName,', '),patient.FName) "
                   + ",carrier.CarrierName,claim.ClaimNum"
                   + ",claimproc.ClaimPaymentNum,claim.PatNum";
        command += " ORDER BY patName_";
        var table = DataCore.GetTable(command);
        return ClaimPaySplitTableToList(table);
    }

    /// <summary>Gets all outstanding claims for the batch payment window.</summary>
    /// <param name="carrierName">If not empty, will return claims with matching or partially matching carrier name.</param>
    /// <param name="dateClaimPay">DateClaimReceivedAfter preference. Only considers claims after this day.</param>
    public static List<ClaimPaySplit> GetOutstandingClaims(string carrierName, DateTime dateClaimPay)
    {
        //Per Nathan, it is OK to return the DateService in the query result to display in the batch insurance window,
        //because that is the date which will be displayed in the Account module when you use the GoTo feature from batch insurance window.
        var command = "SELECT outstanding.*,CONCAT(patient.LName,', ',patient.FName) AS patName_,";
        if (true && Clinics.GetCount() > 0)
            command += "IFNULL(clinic.Description,'') ";
        else
            command += "'' ";
        command += "AS Description FROM (" //Start outstanding
                   + "SELECT * FROM ("
                   + "SELECT claim.DateService,claim.ProvTreat,carrierA.CarrierName,claim.ClaimFee feeBilled_,claim.ClaimStatus,"
                   + "SUM(claimproc.InsPayAmt) insPayAmt_,claim.ClaimNum,0 AS ClaimPaymentNum,claim.ClinicNum,claim.PatNum,0 AS PaymentRow,"
                   + "SUM(CASE WHEN claimproc.ClaimPaymentNum=0 THEN 0 ELSE 1 END) AttachedCount,"
                   + "SUM(CASE WHEN claimproc.ClaimPaymentNum=0 AND claimproc.InsPayAmt!=0 THEN 1 ELSE 0 END) UnattachedPayCount,claim.ClaimIdentifier "
                   + "FROM (" //Start carrierA
                   + "SELECT insplan.PlanNum,carrier.CarrierName "
                   + "FROM carrier "
                   + "INNER JOIN insplan ON carrier.CarrierNum=insplan.CarrierNum ";
        if (carrierName != "") command += "WHERE carrier.CarrierName LIKE '%" + SOut.String(carrierName) + "%'";
        command += ") carrierA " //End carrierA
                   + "INNER JOIN claim ON carrierA.PlanNum=claim.PlanNum AND claim.ClaimType!='PreAuth' ";
        //See job #7423.
        //The claimproc.DateCP is essentially the same as the claim.DateReceived.
        //We used to use the claimproc.ProcDate, which is essentially the same as the claim.DateService.
        //Since the service date could be weeks or months in the past, it makes more sense to use the received date, which will be more recent.
        //Additionally, users found using the date of service to be unintuitive.
        //STRONG CAUTION not to use the claimproc.ProcDate here in the future.
        command += "INNER JOIN claimproc ON claimproc.ClaimNum=claim.ClaimNum "
                   + "WHERE "
                   + "claimproc.IsTransfer=0 AND "
                   + "(claim.ClaimStatus='S' OR "
                   + "(claim.ClaimStatus='R' AND (claimproc.InsPayAmt!=0 " + (dateClaimPay.Year > 1880 ? "OR claimproc.DateCP>=" + SOut.Date(dateClaimPay) : "") + "))) "
                   + "GROUP BY claim.ClaimNum";
        command += ") outstanding "
                   + "WHERE UnattachedPayCount > 0 " //Either unfinalized ins pay amounts on at least one claimproc on the claim,
                   //or if preference is enabled with a specific date, also include received "NO PAYMENT" claims.
                   //Always show Sent claims regardless of preference to match version 16.4 behavior (see job B8189).
                   + "OR (AttachedCount=0" + (dateClaimPay.Year > 1880 ? "" : " AND ClaimStatus='S'") + ")"
                   + ") outstanding " //End outstanding
                   + "INNER JOIN patient ON patient.PatNum = outstanding.PatNum ";
        if (true && Clinics.GetCount() > 0) command += "LEFT JOIN clinic ON clinic.ClinicNum = outstanding.ClinicNum ";
        return ClaimPaySplitTableToList(DataCore.GetTable(command)).OrderByDescending(x => x.Carrier.StartsWith(carrierName)).ThenBy(x => x.Carrier)
            .ThenBy(x => x.PatName).ToList();
    }

    /// <summary>Gets all 'claims' attached to the claimpayment.</summary>
    public static List<ClaimPaySplit> GetAttachedToPayment(long claimPaymentNum)
    {
        var command =
            "SELECT claim.DateService,claim.ProvTreat," + DbHelper.Concat("patient.LName", "', '", "patient.FName") + " patName_,"
            + "carrier.CarrierName,ClaimFee feeBilled_,SUM(claimproc.InsPayAmt) insPayAmt_,claim.ClaimNum,claim.ClaimStatus,"
            + "claimproc.ClaimPaymentNum,clinic.Description,claim.PatNum,claim.ClaimIdentifier,PaymentRow "
            + " FROM claim,patient,insplan,carrier,claimproc"
            + " LEFT JOIN clinic ON clinic.ClinicNum = claimproc.ClinicNum"
            + " WHERE claimproc.ClaimNum = claim.ClaimNum"
            + " AND patient.PatNum = claim.PatNum"
            + " AND insplan.PlanNum = claim.PlanNum"
            + " AND insplan.CarrierNum = carrier.CarrierNum"
            + " AND claimproc.ClaimPaymentNum = " + claimPaymentNum + " ";
        command += "GROUP BY claim.ClaimNum ";
        command += "ORDER BY claimproc.PaymentRow";
        var table = DataCore.GetTable(command);
        return ClaimPaySplitTableToList(table);
    }

    /// <summary>Gets all secondary claims for the related ClaimPaySplits. Called after a payment has been received.</summary>
    public static DataTable GetSecondaryClaims(List<ClaimPaySplit> listClaimPaySplitsAttached)
    {
        var command = "SELECT DISTINCT ProcNum FROM claimproc WHERE ClaimNum IN (";
        var claimNums = ""; //used twice
        for (var i = 0; i < listClaimPaySplitsAttached.Count; i++)
        {
            if (i > 0) claimNums += ",";
            claimNums += listClaimPaySplitsAttached[i].ClaimNum;
        }

        command += claimNums + ") AND ProcNum!=0";
        //List<ClaimProc> tempClaimProcs=ClaimProcCrud.SelectMany(command);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return new DataTable(); //No procedures are attached to these claims.  This frequently happens in conversions.  No need to look for related secondary claims.
        command = "SELECT claimproc.PatNum,claimproc.ProcDate"
                  + " FROM claimproc"
                  + " JOIN claim ON claimproc.ClaimNum=claim.ClaimNum"
                  + " WHERE ProcNum IN (";
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 0) command += ",";
            command += table.Rows[i]["ProcNum"].ToString();
        }

        command += ") AND claimproc.ClaimNum NOT IN (" + claimNums + ")"
                   + " AND ClaimType = 'S'"
                   + " GROUP BY claimproc.ClaimNum,claimproc.PatNum,claimproc.ProcDate";
        var tableSecondaryClaims = DataCore.GetTable(command);
        return tableSecondaryClaims;
    }

    /// <summary>
    ///     Returns 'Unsent' or 'Hold Until Pri Received' claims that have unsent claimprocs and are associated with the
    ///     procedures of the claimprocs passed in.
    ///     Set isSecondaryClaim true to only return secondary claims. Otherwise, only returns primary claims.
    ///     Typically used for getting secondary claims after primary is received or primary claims after a medical claim is
    ///     received.
    /// </summary>
    public static List<Claim> GetPrimaryOrSecondaryClaimsNotReceived(List<ClaimProc> listClaimProcsForClaims, bool isSecondaryClaim = true)
    {
        //Get a list of ProcNums associated with the claimprocs passed in..
        var listProcNumsOnClaims = listClaimProcsForClaims.FindAll(x => x.ProcNum > 0).Select(x => x.ProcNum).ToList();
        //Get list of NotReceived ClaimProcs associated with claims and the list of ProcNums.
        var listClaimProcsForProcsNotReceived = ClaimProcs.GetForProcs(listProcNumsOnClaims)
            .FindAll(x => x.Status == ClaimProcStatus.NotReceived && x.ClaimNum != 0);
        if (listClaimProcsForProcsNotReceived.Count == 0) return new List<Claim>(); //No unreceived claimprocs for procs.
        //Filter the claimprocs by ClaimType based on isSecondaryClaim passed in.
        var claimType = "P";
        if (isSecondaryClaim) claimType = "S";
        //Return 'Unsent' or 'Hold Until Pri Received' claims with the desired ClaimType.
        return GetClaimsFromClaimNums(listClaimProcsForProcsNotReceived.Select(x => x.ClaimNum).ToList())
            .FindAll(x => x.ClaimStatus.In("U", "H") && x.ClaimType == claimType);
    }

    
    public static List<ClaimPaySplit> GetInsPayNotAttachedForFixTool()
    {
        var command =
            "SELECT claim.DateService,claim.ProvTreat,CONCAT(CONCAT(patient.LName,', '),patient.FName) patName_"
            + ",carrier.CarrierName,SUM(claimproc.FeeBilled) feeBilled_,SUM(claimproc.InsPayAmt) insPayAmt_,claim.ClaimNum,claim.ClaimStatus"
            + ",claimproc.ClaimPaymentNum,(SELECT clinic.Description FROM clinic WHERE claimproc.ClinicNum = clinic.ClinicNum) Description,claim.PatNum,PaymentRow "
            + ",claim.ClaimIdentifier "
            + " FROM claim,patient,insplan,carrier,claimproc"
            + " WHERE claimproc.ClaimNum = claim.ClaimNum"
            + " AND patient.PatNum = claim.PatNum"
            + " AND insplan.PlanNum = claim.PlanNum"
            + " AND insplan.CarrierNum = carrier.CarrierNum"
            + " AND (claimproc.Status = '1' OR claimproc.Status = '4' OR claimproc.Status=5)" //received or supplemental or capclaim
            + " AND (claimproc.InsPayAmt != 0 AND claimproc.ClaimPaymentNum = '0')"
            + " AND claimproc.IsTransfer=0"
            + " GROUP BY claim.DateService,claim.ProvTreat,CONCAT(CONCAT(patient.LName,', '),patient.FName)"
            + ",carrier.CarrierName,claim.ClaimNum,claimproc.ClaimPaymentNum,claim.PatNum"
            + " ORDER BY patName_";
        var table = DataCore.GetTable(command);
        return ClaimPaySplitTableToList(table);
    }

    /// <summary>
    ///     Returns a list of the top CarrierNames that had the most claim volume for a defined starting number of days back
    ///     (using claim.DateSent)
    ///     through today. Only includes Sent and Received claims.
    /// </summary>
    public static List<string> GetTopVolumeCarrierNamesForClinicAndPeriod(long clinicNum, int numDaysBack, int numTopCarriers)
    {
        var listTopVolumeCarrierNames = new List<string>();
        var command = "SELECT carrier.CarrierName, COUNT(claim.ClaimNum) AS Total"
                      + " FROM claim"
                      + " INNER JOIN insplan"
                      + " ON insplan.PlanNum=claim.PlanNum"
                      + " INNER JOIN carrier"
                      + " ON insplan.CarrierNum=carrier.CarrierNum"
                      + " WHERE claim.ClaimStatus IN ('S','R')"
                      + " AND claim.ClinicNum=" + SOut.Long(clinicNum)
                      + " AND carrier.CarrierName!=''"
                      + " AND claim.DateSent BETWEEN " + SOut.Date(DateTime.Now.AddDays(-numDaysBack)) + " AND " + SOut.Date(DateTime.Now)
                      + " GROUP BY carrier.CarrierName"
                      + " ORDER BY Total DESC"
                      + " LIMIT " + SOut.Int(numTopCarriers);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
            for (var i = 0; i < table.Rows.Count; i++)
                listTopVolumeCarrierNames.Add(SIn.String(table.Rows[i]["CarrierName"].ToString()));

        return listTopVolumeCarrierNames;
    }

    
    private static List<ClaimPaySplit> ClaimPaySplitTableToList(DataTable table)
    {
        var listClaimPaySplits = new List<ClaimPaySplit>();
        ClaimPaySplit claimPaySplit;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            claimPaySplit = new ClaimPaySplit();
            claimPaySplit.DateClaim = SIn.Date(table.Rows[i]["DateService"].ToString());
            claimPaySplit.ProvAbbr = Providers.GetAbbr(SIn.Long(table.Rows[i]["ProvTreat"].ToString()));
            claimPaySplit.PatName = SIn.String(table.Rows[i]["patName_"].ToString());
            claimPaySplit.PatNum = SIn.Long(table.Rows[i]["PatNum"].ToString());
            claimPaySplit.Carrier = SIn.String(table.Rows[i]["CarrierName"].ToString());
            claimPaySplit.FeeBilled = SIn.Double(table.Rows[i]["feeBilled_"].ToString());
            claimPaySplit.InsPayAmt = SIn.Double(table.Rows[i]["insPayAmt_"].ToString());
            claimPaySplit.ClaimNum = SIn.Long(table.Rows[i]["ClaimNum"].ToString());
            claimPaySplit.ClaimPaymentNum = SIn.Long(table.Rows[i]["ClaimPaymentNum"].ToString());
            claimPaySplit.PaymentRow = SIn.Int(table.Rows[i]["PaymentRow"].ToString());
            claimPaySplit.ClinicDesc = SIn.String(table.Rows[i]["Description"].ToString());
            claimPaySplit.ClaimStatus = SIn.String(table.Rows[i]["ClaimStatus"].ToString());
            claimPaySplit.ClaimIdentifier = SIn.String(table.Rows[i]["ClaimIdentifier"].ToString());
            listClaimPaySplits.Add(claimPaySplit);
        }

        return listClaimPaySplits;
    }

    ///<summary>Gets the specified claim from the database.  Can be null.</summary>
    public static Claim GetClaim(long claimNum)
    {
        var command = "SELECT * FROM claim"
                      + " WHERE ClaimNum = " + claimNum;
        var retClaim = ClaimCrud.SelectOne(command);
        if (retClaim == null) return null;
        command = "SELECT * FROM claimattach WHERE ClaimNum = " + SOut.Long(claimNum);
        retClaim.Attachments = ClaimAttachCrud.SelectMany(command);
        return retClaim;
    }

    public static List<Claim> GetClaimsFromClaimNums(List<long> listClaimNums)
    {
        if (listClaimNums.IsNullOrEmpty()) return new List<Claim>();
        var command = $"SELECT * FROM claim WHERE ClaimNum IN ({string.Join(",", listClaimNums)})";
        return ClaimCrud.SelectMany(command);
    }

    ///<summary>Gets all claims for the specified patient. But without any attachments.</summary>
    public static List<Claim> Refresh(long patNum)
    {
        if (patNum == 0) return new List<Claim>();
        var command =
            "SELECT * FROM claim"
            + " WHERE PatNum = " + patNum
            + " ORDER BY dateservice";
        return ClaimCrud.SelectMany(command);
    }

    public static Claim GetFromList(List<Claim> listClaims, long claimNum)
    {
        for (var i = 0; i < listClaims.Count; i++)
            if (listClaims[i].ClaimNum == claimNum)
                return listClaims[i].Copy();

        return null;
    }

    
    public static long Insert(Claim claim)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        claim.SecUserNumEntry = Security.CurUser.UserNum;
        claim.SecurityHash = HashFields(claim);
        return ClaimCrud.Insert(claim);
    }

    
    public static void Update(Claim claim)
    {
        var claimOld = GetClaim(claim.ClaimNum);
        if (IsClaimHashValid(claimOld)) //Only rehash claims that are already valid.
            claim.SecurityHash = HashFields(claim);
        ClaimCrud.Update(claim);
        //now, delete all attachments and recreate.
        var command = "DELETE FROM claimattach WHERE ClaimNum=" + SOut.Long(claim.ClaimNum);
        Db.NonQ(command);
        for (var i = 0; i < claim.Attachments.Count; i++)
        {
            claim.Attachments[i].ClaimNum = claim.ClaimNum;
            ClaimAttaches.Insert(claim.Attachments[i]);
        }
    }

    ///<summary>Takes in a claim, checks to see if there's any differences, and then updates the database if necessary.</summary>
    public static void Update(Claim claim, Claim claimOld)
    {
        if (!ClaimCrud.UpdateComparison(claim, claimOld)) return;
        if (IsClaimHashValid(claimOld)) //Only rehash claims that are already valid.
            claim.SecurityHash = HashFields(claim);
        ClaimCrud.Update(claim, claimOld);
    }

    ///<summary>Deletes the claim and also deletes any Etrans835Attaches when specified.</summary>
    public static void Delete(Claim claim, List<long> listEtrans835AttachNums = null)
    {
        Etrans835Attaches.DeleteMany(listEtrans835AttachNums);
        ClaimCrud.Delete(claim.ClaimNum);
    }

    /// <summary>
    ///     Called from claimsend window and from Claim edit window.  Use 0 to get all waiting claims, or an actual
    ///     claimnum to get just one claim.
    /// </summary>
    public static ClaimSendQueueItem[] GetQueueList(long claimNum, long clinicNum, long customTracking)
    {
        var listClaimNums = new List<long>();
        if (claimNum != 0) listClaimNums.Add(claimNum);
        return GetQueueList(listClaimNums, clinicNum, customTracking);
    }

    /// <summary>
    ///     Called from claimsend window and from Claim edit window.  Use an empty listClaimNums to get all waiting
    ///     claims.
    /// </summary>
    public static ClaimSendQueueItem[] GetQueueList(List<long> listClaimNums, long clinicNum, long customTracking)
    {
        var listWhereAnds = new List<string>();
        if (listClaimNums.Count == 0)
            listWhereAnds.Add("claim.ClaimStatus IN ('W','P') ");
        else
            listWhereAnds.Add("claim.ClaimNum IN (" + string.Join(",", listClaimNums) + ") ");
        if (clinicNum > 0) listWhereAnds.Add("claim.ClinicNum=" + SOut.Long(clinicNum) + " ");
        if (customTracking > 0) listWhereAnds.Add("claim.CustomTracking=" + SOut.Long(customTracking) + " ");
        //Removed subselect query for HasIcd9 because we're grabbing all of the claims and claimprocs in the code anyway.
        //Much less punishing to offices that don't have medical procedures.
        var command = $@"SELECT claim.ClaimNum,carrier.NoSendElect,claim.ClaimStatus,carrier.CarrierName,patient.PatNum,carrier.ElectID,claim.MedType,
				claim.DateService,claim.ClinicNum,claim.CustomTracking,claim.ProvTreat,claim.SecDateTEdit,
				{DbHelper.Concat("patient.LName", "', '", "patient.FName", "IF(patient.MiddleI='','',CONCAT(' ',patient.MiddleI))")} patName,
				(SELECT MIN(patplan.Ordinal) FROM patplan WHERE patplan.PatNum=claim.PatNum AND patplan.InsSubNum=claim.InsSubNum) Ordinal
				FROM claim
				LEFT JOIN patient ON patient.PatNum=claim.PatNum
				LEFT JOIN insplan ON claim.PlanNum=insplan.PlanNum
				LEFT JOIN carrier ON insplan.CarrierNum=carrier.CarrierNum
				WHERE {string.Join("AND ", listWhereAnds)}
				ORDER BY claim.DateService,patient.LName,patient.FName";
        var dictionaryClaimRows = DataCore.GetTable(command).Select().GroupBy(x => SIn.Long(x["ClaimNum"].ToString())).ToDictionary(x => x.Key, x => x.First());
        var listClaimProcQueueds = ClaimProcs.GetClaimProcQueuedsForClaims(dictionaryClaimRows.Keys.ToList());
        var listProcQueueds = Procedures.GetProcQueuedsFromClaimProcQueueds(listClaimProcQueueds);
        var dictionaryProcCodeStrs = listProcQueueds.ToDictionary(x => x.ProcNum, x => ProcedureCodes.GetStringProcCode(x.CodeNum));
        var dictionaryProcCodeStrsPerClaim = listClaimProcQueueds
            .Where(x => dictionaryProcCodeStrs.ContainsKey(x.ProcNum) && !string.IsNullOrEmpty(dictionaryProcCodeStrs[x.ProcNum]))
            .GroupBy(x => x.ClaimNum, x => dictionaryProcCodeStrs[x.ProcNum])
            .ToDictionary(x => x.Key, x => string.Join(", ", x));
        //Get all of the ProcNums and decide if they meet "HasIcd9" criteria of IcdVersion 9 and at least one populated diagnostic code.
        var listProcNumsWithIcd9 = listProcQueueds.FindAll(x => x.IcdVersion == 9
                                                                && !string.IsNullOrWhiteSpace(x.DiagnosticCode + x.DiagnosticCode2 + x.DiagnosticCode3 + x.DiagnosticCode4)
        ).Select(x => x.ProcNum).ToList();
        //Group the ClaimNums with their associated ProcNums.
        var listClaimNumsWithIcd9 = listClaimProcQueueds.FindAll(x => listProcNumsWithIcd9.Contains(x.ProcNum)).Select(x => x.ClaimNum).Distinct().ToList();
        var claimSendQueueItemArray = dictionaryClaimRows.Select(x =>
        {
            var claimSendQueueItem = new ClaimSendQueueItem();
            claimSendQueueItem.ClaimNum = x.Key;
            claimSendQueueItem.NoSendElect = SIn.Enum<NoSendElectType>(x.Value["NoSendElect"].ToString());
            claimSendQueueItem.Ordinal = SIn.Int(x.Value["Ordinal"].ToString());
            claimSendQueueItem.PatName = SIn.String(x.Value["patName"].ToString());
            claimSendQueueItem.ClaimStatus = SIn.String(x.Value["ClaimStatus"].ToString());
            claimSendQueueItem.Carrier = SIn.String(x.Value["CarrierName"].ToString());
            claimSendQueueItem.PatNum = SIn.Long(x.Value["PatNum"].ToString());
            claimSendQueueItem.MedType = SIn.Enum<EnumClaimMedType>(x.Value["MedType"].ToString());
            claimSendQueueItem.ClearinghouseNum = Clearinghouses.AutomateClearinghouseHqSelection(SIn.String(x.Value["ElectID"].ToString()),
                SIn.Enum<EnumClaimMedType>(x.Value["MedType"].ToString()));
            claimSendQueueItem.DateService = SIn.Date(x.Value["DateService"].ToString());
            claimSendQueueItem.ClinicNum = SIn.Long(x.Value["ClinicNum"].ToString());
            claimSendQueueItem.CustomTracking = SIn.Long(x.Value["CustomTracking"].ToString());
            claimSendQueueItem.HasIcd9 = listClaimNumsWithIcd9.Contains(SIn.Long(x.Value["ClaimNum"].ToString()));
            claimSendQueueItem.ProvTreat = SIn.Long(x.Value["ProvTreat"].ToString());
            claimSendQueueItem.ProcedureCodeString = dictionaryProcCodeStrsPerClaim.TryGetValue(x.Key, out var procCodeStr) ? procCodeStr : "";
            claimSendQueueItem.SecDateTEdit = SIn.Date(x.Value["SecDateTEdit"].ToString());
            return claimSendQueueItem;
        }).ToArray();
        return claimSendQueueItemArray;
    }

    ///<summary>Supply claimnums. Called from X12 to begin the sorting process on claims going to one clearinghouse.</summary>
    public static List<X12TransactionItem> GetX12TransactionInfo(long claimNum)
    {
        var listClaimNums = new List<long>();
        listClaimNums.Add(claimNum);
        return GetX12TransactionInfo(listClaimNums);
    }

    ///<summary>Supply claimnums. Called from X12 to begin the sorting process on claims going to one clearinghouse.</summary>
    public static List<X12TransactionItem> GetX12TransactionInfo(List<long> listClaimNums)
    {
        //ArrayList queueItemss){
        var listX12TransactionItems = new List<X12TransactionItem>();
        if (listClaimNums.Count < 1) return listX12TransactionItems;
        string command;
        command = "SELECT carrier.ElectID,claim.ProvBill,inssub.Subscriber,"
                  + "claim.PatNum,claim.ClaimNum,CASE WHEN inssub.Subscriber!=claim.PatNum THEN 1 ELSE 0 END AS subscNotPatient "
                  + "FROM claim,insplan,inssub,carrier "
                  + "WHERE claim.PlanNum=insplan.PlanNum "
                  + "AND claim.InsSubNum=inssub.InsSubNum "
                  + "AND carrier.CarrierNum=insplan.CarrierNum "
                  + "AND claim.ClaimNum IN (" + string.Join(",", listClaimNums) + ") "
                  + "ORDER BY carrier.ElectID,claim.ProvBill,inssub.Subscriber,subscNotPatient,claim.PatNum";
        var table = DataCore.GetTable(command);
        //object[,] myA=new object[5,table.Rows.Count];
        X12TransactionItem x12TransactionItem;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            x12TransactionItem = new X12TransactionItem();
            x12TransactionItem.PayorId0 = SIn.String(table.Rows[i][0].ToString());
            x12TransactionItem.ProvBill1 = SIn.Long(table.Rows[i][1].ToString());
            x12TransactionItem.Subscriber2 = SIn.Long(table.Rows[i][2].ToString());
            x12TransactionItem.PatNum3 = SIn.Long(table.Rows[i][3].ToString());
            x12TransactionItem.ClaimNum4 = SIn.Long(table.Rows[i][4].ToString());
            listX12TransactionItems.Add(x12TransactionItem);
        }

        return listX12TransactionItems;
    }

    ///<summary>Also sets the DateSent to today.</summary>
    public static void SetClaimSent(long claimNum)
    {
        var claimOld = GetClaim(claimNum);
        var dateT = MiscData.GetNowDateTime();
        var command = "UPDATE claim SET ClaimStatus = 'S',"
                      + "DateSent=" + SOut.Date(dateT) + ", "
                      + "DateSentOrig=(CASE WHEN DateSentOrig='0001-01-01' THEN " + SOut.Date(dateT) + " ELSE DateSentOrig END) "
                      + "WHERE ClaimNum = " + SOut.Long(claimNum);
        Db.NonQ(command);
        if (claimOld != null && IsClaimHashValid(claimOld))
        {
            //Should never be null. Only rehash claims that are already valid.
            var claim = GetClaim(claimNum);
            claim.SecurityHash = HashFields(claim);
            if (claimOld.SecurityHash != claim.SecurityHash) //Only bother updating if the SecurityHash is different.
                ClaimCrud.Update(claim);
        }
    }

    public static bool IsClaimIdentifierInUse(string claimIdentifier, long claimNumExclude, string claimType)
    {
        var command = "SELECT COUNT(*) FROM claim WHERE ClaimIdentifier='" + SOut.String(claimIdentifier) + "' AND ClaimNum<>" + SOut.Long(claimNumExclude);
        if (claimType == "PreAuth")
            command += " AND ClaimType='PreAuth'";
        else
            command += " AND ClaimType!='PreAuth'";
        return DataCore.GetTable(command).Rows[0][0].ToString() != "0";
    }

    public static bool IsClaimPreAuth(Claim claim)
    {
        return claim != null && claim.ClaimType == "PreAuth";
    }

    public static bool IsReferralAttached(long referralNum)
    {
        var command = "SELECT COUNT(*) FROM claim WHERE OrderingReferralNum=" + SOut.Long(referralNum);
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    /// <summary>
    ///     Returns a list of claimnums matching the list of x12claims given.
    ///     The returned list is always same length as the list of x12claims, unless there is an error, in which case null is
    ///     returned.
    ///     If a claim in the database is not found for a specific x12claim, then a value of 0 will be placed into the return
    ///     list for that x12claim.
    ///     Each matched claim will either begin with the specified claimIdentifier, or will be for the patient name and
    ///     subscriber ID specified.
    /// </summary>
    public static List<long> GetClaimFromX12(List<X12ClaimMatch> listX12ClaimMatches)
    {
        if (listX12ClaimMatches.Count == 0) return null;

        #region Either get a list of dates for given X12ClaimMatches, or a dateMin and dateMax.

        var listDateTimes = new List<DateTime>();
        var dateMin = DateTime.MinValue;
        var dateMax = DateTime.MinValue;
        if (PrefC.GetBool(PrefName.EraStrictClaimMatching))
        {
            for (var i = 0; i < listX12ClaimMatches.Count; i++)
            {
                listDateTimes.AddRange(MiscUtils.GetDatesInRange(listX12ClaimMatches[i].DateServiceStart, listX12ClaimMatches[i].DateServiceEnd));
                for (var j = 0; j < listX12ClaimMatches[i].List835Procs.Count; j++) listDateTimes.AddRange(MiscUtils.GetDatesInRange(listX12ClaimMatches[i].List835Procs[j].DateServiceStart, listX12ClaimMatches[i].List835Procs[j].DateServiceEnd));
            }

            //Carrier will send 01/01/1900 as a default date instead of MinVal or omission.
            listDateTimes = listDateTimes.Distinct().Where(x => x.Year >= 1900).ToList();
            //If there are no dates to consider, return early.
            if (listDateTimes.Count == 0) return null;
        }
        else
        {
            //Usually claims from the same ERA will all have dates of service within a few weeks of each other.
            if (listX12ClaimMatches.FindAll(x => x.DateServiceStart.Year >= 1900).Count() > 0) dateMin = listX12ClaimMatches.FindAll(x => x.DateServiceStart.Year >= 1900).Select(x => x.DateServiceStart).Min(); //DateServiceStart can be 1900 for PreAuths.
            if (listX12ClaimMatches.FindAll(x => x.DateServiceEnd.Year >= 1900).Count() > 0) dateMax = listX12ClaimMatches.FindAll(x => x.DateServiceEnd.Year >= 1900).Select(x => x.DateServiceEnd).Max(); //DateServiceEnd can be 1900 for PreAuths.
            if (dateMin.Year < 1880 || dateMax.Year < 1880)
                //Service dates are required for us to continue.
                //In 227s, the claim dates of service are required and should be present.
                //In 835s, we pull the procedure dates up into the claim dates of service if the claim dates are of service are not present.
                return null;
        }

        #endregion

        #region Construct etrans/835 dictionary.  Use for matching loop and internal claim query.

        //Since listX12claims can contain values from different etrans/835s we want to run matching logic per each 835.
        //We have seen 835s contain split claims (one procedure per 835 claim in many cases) so we group by claimIdentifier to calculate their claimFees.
        //This allows us to more accurately match all grouped 835 claims in one pass for split claims.
        //Dictionary such that:
        //Key => EtransNum
        //Value => Dictionary such that: key => ClaimIdentifier and value => list of X12ClaimMatches.
        var dictionaryMatchesPerClaimId = listX12ClaimMatches
            .GroupBy(x => x.EtransNum)
            .ToDictionary(
                x => x.Key, //EtransNum
                x => x.GroupBy(y => y.ClaimIdentifier)
                    .ToDictionary(
                        y => y.Key, //ClaimIdentifier
                        y => y.ToList() //List of X12ClaimMatches
                    )
            );

        #endregion

        #region Get a list of fees for given X12ClaimMatches.  Claims in DB must match these because that is how we do initial matching.

        var dictionaryTotalClaimFee = new Dictionary<long, Dictionary<string, double>>();
        var listTotalClaimFees = new List<double>();
        foreach (var etransNum in dictionaryMatchesPerClaimId.Keys)
        {
            dictionaryTotalClaimFee[etransNum] = new Dictionary<string, double>();
            foreach (var claimIdentifier in dictionaryMatchesPerClaimId[etransNum].Keys)
            {
                var claimFee = dictionaryMatchesPerClaimId[etransNum][claimIdentifier]
                    .Sum(x => x.Is835Reversal ? 0 : x.ClaimFee); //Ignore claim reversals, because they negate the original claim fee.
                dictionaryTotalClaimFee[etransNum][claimIdentifier] = claimFee;
                if (!listTotalClaimFees.Contains(claimFee)) listTotalClaimFees.Add(claimFee);
            }
        }

        #endregion

        #region Get List of Claims For Date and Fee Ranges

        Dictionary<DateTime, List<DataRow>> dictionaryClaims;
        if (PrefC.GetBool(PrefName.EraStrictClaimMatching))
            dictionaryClaims = GetClaimTable(listDateTimes, listTotalClaimFees).Select()
                .GroupBy(x => SIn.Date(x["DateService"].ToString()))
                .ToDictionary(x => x.Key, x => x.ToList());
        else
            dictionaryClaims = GetClaimTable(dateMin, dateMax, listTotalClaimFees).Select()
                .GroupBy(x => SIn.Date(x["DateService"].ToString()))
                .ToDictionary(x => x.Key, x => x.ToList());

        #endregion

        #region Get claimProcs for given 835 procNums that are associated to a claim.

        var listAllEraProcNums = dictionaryMatchesPerClaimId
            .SelectMany(x => x.Value.SelectMany(y => y.Value)) //x.Value => Dictionary<string,List<X12ClaimMatch>> to one big List<X12ClaimMatch>
            .SelectMany(y => y.List835Procs.Select(z => z.ProcNum)).Distinct().ToList(); //List<X12ClaimMatch> to List<ProcNums>
        var listClaimProcStatuses = new List<ClaimProcStatus>(); //ClaimProcStatuses that have procNums.
        listClaimProcStatuses.Add(ClaimProcStatus.NotReceived);
        listClaimProcStatuses.Add(ClaimProcStatus.Received);
        listClaimProcStatuses.Add(ClaimProcStatus.Preauth);
        listClaimProcStatuses.Add(ClaimProcStatus.CapClaim);
        listClaimProcStatuses.Add(ClaimProcStatus.CapComplete);
        var listClaimProcsAll = ClaimProcs.GetForProcs(listAllEraProcNums, listClaimProcStatuses); //Only runs query if procNumList not empty
        var listClaimProcsAccount = listClaimProcsAll.FindAll(x => x.Status != ClaimProcStatus.Preauth);
        var listClaimProcsTreatPlan = listClaimProcsAll.FindAll(x => x.Status == ClaimProcStatus.Preauth);
        var listPatPlans = PatPlans.GetListByInsSubNums(listClaimProcsAll.Select(x => x.InsSubNum).ToList()); //Only runs query if list contains items.

        #endregion

        var listClaimNums = new List<long>(new long[listX12ClaimMatches.Count]); //Done this way to guarantee that each claimnum is initialized to 0.
        //For each provided etrans, we look at 1 group such that the key is the claimIdentifier and the value is the list of all claim matches assocaited to the claimIdentifier.
        //This means that each entry in the list of claim matches should share many fields like, claimIdentifier, patient FName, patient LName and subscriber ID.
        foreach (var etransNum in dictionaryMatchesPerClaimId.Keys) //Consider a single etrans at a time.
        foreach (var claimIdentifier in dictionaryMatchesPerClaimId[etransNum].Keys)
        {
            //Claims that are split by procedure from the carrier's side are grouped together by claimIdentifier above.
            var x12ClaimMatch = dictionaryMatchesPerClaimId[etransNum][claimIdentifier].First(); //Just use the first 835 claim to try and match because all fields we use should be identical. 
            var listEraProcNums = dictionaryMatchesPerClaimId[etransNum][claimIdentifier].SelectMany(x => x.List835Procs.Select(y => y.ProcNum)).ToList(); //All identified procNums reported from 835.
            //Begin with basic filtering by date of service and claim total fee.
            var listDataRowsDbClaims = new List<DataRow>();
            foreach (var d in dictionaryClaims.Keys.Where(x => (x >= x12ClaimMatch.DateServiceStart && x <= x12ClaimMatch.DateServiceEnd) || x.Year == 1))
                //PreAuth service date is 0001-01-01
                listDataRowsDbClaims.AddRange(dictionaryClaims[d].FindAll(x => SIn.Double(x["ClaimFee"].ToString()) == dictionaryTotalClaimFee[etransNum][claimIdentifier]));

            #region 835 ProcNum matching in conjunction with PlanNum and Ordinal matching.  Helps distinctly identify primary vs secondary claims.

            if (x12ClaimMatch.List835Procs.Count > 0 && x12ClaimMatch.List835Procs.First().MatchingVersion.In(EraProcMatchingFormat.X, EraProcMatchingFormat.Y))
            {
                var hx835_Proc = x12ClaimMatch.List835Procs.First(); //PlanNum and Ordinal should be the same for all procs.
                Dictionary<long, List<ClaimProc>> dictionaryClaimProcs = null;
                switch (hx835_Proc.MatchingVersion)
                {
                    case EraProcMatchingFormat.X:
                        //Matching 'x(procNum)/(ordinal)/(InsPlan.planNum)' format.
                        dictionaryClaimProcs = listClaimProcsAll //Keys are ClaimNum from database and values are list of claimprocs for claim.
                            .FindAll(x => listEraProcNums.Contains(x.ProcNum)
                                          && x.PlanNum == hx835_Proc.PlanNum
                                          && PatPlans.GetOrdinal(x.InsSubNum, listPatPlans.FindAll(y => y.PatNum == x.PatNum)) == hx835_Proc.PlanOrdinal
                            ) //List of claimProcs matched by ProcNum, PlanNum and ordinal.
                            .GroupBy(x => x.ClaimNum) //All claimProcs on the same claim should share pertinent fields.
                            .ToDictionary(x => x.Key, y => y.ToList()
                            );
                        break;
                    case EraProcMatchingFormat.Y:
                        //Matching 'y(procNum)/(ordinal)/(partial InsPlan.planNum)' format.
                        dictionaryClaimProcs = listClaimProcsAll //Keys are ClaimNum from database and values are list of claimprocs for claim.
                            .FindAll(x => listEraProcNums.Contains(x.ProcNum)
                                          && x.PlanNum.ToString().EndsWith(hx835_Proc.PartialPlanNum.ToString())
                                          && PatPlans.GetOrdinal(x.InsSubNum, listPatPlans.FindAll(y => y.PatNum == x.PatNum)) == hx835_Proc.PlanOrdinal
                            ) //List of claimProcs matched by ProcNum, partial PlanNum and ordinal.
                            .GroupBy(x => x.ClaimNum) //All claimProcs on the same claim should share pertinent fields.
                            .ToDictionary(x => x.Key, y => y.ToList()
                            );
                        break;
                }

                if (dictionaryClaimProcs.Count == 1)
                {
                    //Single claim match found.
                    foreach (var match in dictionaryMatchesPerClaimId[etransNum][claimIdentifier])
                    {
                        var index = listX12ClaimMatches.IndexOf(match);
                        listClaimNums[index] = dictionaryClaimProcs.Keys.First();
                    }

                    continue; //Move to the next claim.
                }
                //Either multiple matches or no matches. Either way consider other matching logic.
            }

            #endregion

            var listIndiciesForIdentifier = new List<int>(); //Stores indicies from listDbCLaims where we find a matching claimIdentifier.

            #region 835 ProcNum matching in conjunction with ClaimIdentifier matching.  Helps distinctly identify primary vs secondary claims.

            if (x12ClaimMatch.ClaimIdentifier.Length > 0 && x12ClaimMatch.ClaimIdentifier != "0")
            {
                //The following dicitonary is constructed so that we can compare the 835s procNums to internal claims.
                //Previously we have seen that the CLP02 flag matching (see below) isn't always trustworthy so we want to first consider exact 835s ClaimIdentifier matches.
                //So we construct a list of claimProcs for the given 835 proc nums and group them by claimNum since we really just care about the claim.
                var dictionaryClaimProcs = listClaimProcsAll.FindAll(x => listEraProcNums.Contains(x.ProcNum))
                    .GroupBy(x => x.ClaimNum)
                    .ToDictionary(x => x.Key, y => y.ToList()
                    );
                foreach (var cpClaimNum in dictionaryClaimProcs.Keys)
                {
                    for (var i = 0; i < listDataRowsDbClaims.Count; i++)
                    {
                        if (cpClaimNum != SIn.Long(listDataRowsDbClaims[i]["ClaimNum"].ToString()) //Not the claim we are looking for.
                            || x12ClaimMatch.ClaimIdentifier != SIn.String(listDataRowsDbClaims[i]["ClaimIdentifier"].ToString())) //Correct claim, but wrong claimIdentifier.
                            continue;
                        listIndiciesForIdentifier.Add(i); //Match based on claim identifier, claim date of service, claim fee and given procNums.
                        if (listIndiciesForIdentifier.Count > 1)
                            //don't need to continue looping if we find more than 1
                            break;
                    }

                    if (listIndiciesForIdentifier.Count > 1) break;
                }

                if (listIndiciesForIdentifier.Count == 1)
                {
                    //A single match based on claim identifier, claim date of service, claim fee and given procNums.
                    var claimNum = SIn.Long(listDataRowsDbClaims[listIndiciesForIdentifier[0]]["ClaimNum"].ToString());
                    foreach (var match in dictionaryMatchesPerClaimId[etransNum][claimIdentifier])
                    {
                        var index = listX12ClaimMatches.IndexOf(match);
                        listClaimNums[index] = claimNum;
                    }

                    continue; //Move to the next claim.
                }
            }

            #endregion

            #region 835 ProcNum and CLP02 matching and setting.

            var listClaimProcs = new List<ClaimProc>();
            switch (x12ClaimMatch.CodeClp02)
            {
                case "1": //"Processed as Primary"
                case "19": //"Processed as Primary, Forwarded to Additional Payer(s)"
                    listClaimProcs = ClaimProcs.GetForProcsWithOrdinalFromList(listEraProcNums, 1, listPatPlans, listClaimProcsAccount);
                    break;
                case "2": //"Processed as Secondary"
                case "20": //"Processed as Secondary, Forwarded to Additional Payer(s)"
                    listClaimProcs = ClaimProcs.GetForProcsWithOrdinalFromList(listEraProcNums, 2, listPatPlans, listClaimProcsAccount);
                    break;
                case "3": //"Processed as Tertiary"
                case "21": //"Processed as Tertiary, Forwarded to Additional Payer(s)"
                    listClaimProcs = ClaimProcs.GetForProcsWithOrdinalFromList(listEraProcNums, 3, listPatPlans, listClaimProcsAccount);
                    break;
                case "4": //"Denied"
                case "22": //"Reversal of Previous Payment"
                case "23": //"Not Our Claim, Forwarded to Additional Payer(s)"
                    //The odds of all the claim nums matching here is lower, because we could match both primary and secondary.
                    listClaimProcs = listClaimProcsAccount.FindAll(x => listEraProcNums.Contains(x.ProcNum));
                    break;
                case "25": //"Predetermination Pricing Only - No Payment"
                    listClaimProcs = listClaimProcsTreatPlan.FindAll(x => listEraProcNums.Contains(x.ProcNum));
                    break;
            }

            if (listClaimProcs.Count > 0)
            {
                //Successfully found internal claimProcs.
                var claimNumKey = listClaimProcs.First().ClaimNum;
                if (listClaimProcs.All(x => x.ClaimNum == claimNumKey))
                {
                    //All claimNums must match.
                    foreach (var match in dictionaryMatchesPerClaimId[etransNum][claimIdentifier])
                    {
                        var index = listX12ClaimMatches.IndexOf(match);
                        listClaimNums[index] = claimNumKey;
                    }

                    continue; //Move to the next claim.
                }
            }

            #endregion

            #region ClaimIdentifier matching and setting.

            //Look for claim matched by full or partial claim identifier.
            listIndiciesForIdentifier = new List<int>();
            if (x12ClaimMatch.ClaimIdentifier.Length > 0 && x12ClaimMatch.ClaimIdentifier != "0")
            {
                //Ensure an ID is present and that it is not for a printed claim (when ID=="0").
                //Look for a single exact match by claim identifier.  This step is first, so that the user can override claim association to the 835 or 277 by changing the claim identifier if desired.
                for (var i = 0; i < listDataRowsDbClaims.Count; i++)
                {
                    var claimId = SIn.String(listDataRowsDbClaims[i]["ClaimIdentifier"].ToString());
                    if (claimId == x12ClaimMatch.ClaimIdentifier) listIndiciesForIdentifier.Add(i);
                }

                if (listIndiciesForIdentifier.Count == 0 && x12ClaimMatch.ClaimIdentifier.Length > 15)
                    //No exact match found.  Look for similar claim identifiers if the identifer was possibly truncated when sent out.
                    //Our claim identifiers can be longer than 20 characters (mostly when using replication). When the claim identifier is sent out on the claim, it is truncated to 20
                    //characters. Therefore, if the claim identifier is longer than 20 characters, then it was truncated when sent out, so we have to look for claims beginning with the 
                    //claim identifier given if there is not an exact match.  We also send shorter identifiers for some clearinghouses.  For example, the maximum claim identifier length
                    //for Denti-Cal is 17 characters.
                    for (var i = 0; i < listDataRowsDbClaims.Count; i++)
                    {
                        var claimId = SIn.String(listDataRowsDbClaims[i]["ClaimIdentifier"].ToString());
                        if (claimId.StartsWith(x12ClaimMatch.ClaimIdentifier)) listIndiciesForIdentifier.Add(i);
                    }
            }

            if (listIndiciesForIdentifier.Count == 0)
            {
                //No matches were found for the identifier.  Continue to more advanced matching below.
            }
            else if (listIndiciesForIdentifier.Count == 1)
            {
                //A single match based on claim identifier, claim date of service, and claim fee.
                var claimNum = SIn.Long(listDataRowsDbClaims[listIndiciesForIdentifier[0]]["ClaimNum"].ToString());
                foreach (var match in dictionaryMatchesPerClaimId[etransNum][claimIdentifier])
                {
                    var index = listX12ClaimMatches.IndexOf(match);
                    listClaimNums[index] = claimNum;
                }

                continue; //Move to the next claim.
            }
            else if (listIndiciesForIdentifier.Count > 1)
            {
                //Edge case.
                //Multiple matches for the specified claim identifier AND date service AND fee.  The claim must have been split (rare because the split claims must have the same fee).
                //Continue to more advanced matching below, although it probably will not help.  We could enhance this specific scenario by picking a claim based on the procedures attached, but that is not a guarantee either.
            }

            #endregion

            #region Patient LName and FName (exact/partial) matching.

            //Locate claims exactly matching patient last name.
            var listDataRowsMatches = new List<DataRow>();
            var patLname = x12ClaimMatch.PatLname.Trim().ToLower();
            for (var i = 0; i < listDataRowsDbClaims.Count; i++)
            {
                var lastNameInDb = SIn.String(listDataRowsDbClaims[i]["LName"].ToString()).Trim().ToLower();
                if (lastNameInDb == patLname) listDataRowsMatches.Add(listDataRowsDbClaims[i]);
            }

            //Locate claims matching exact first name or partial first name, with a preference for exact match.
            var listDataRowsExactFirst = new List<DataRow>();
            var listDataRowsPartFirst = new List<DataRow>();
            var patFname = x12ClaimMatch.PatFname.Trim().ToLower();
            for (var i = 0; i < listDataRowsMatches.Count; i++)
            {
                var firstNameInDb = SIn.String(listDataRowsMatches[i]["FName"].ToString()).Trim().ToLower();
                if (firstNameInDb == patFname)
                    listDataRowsExactFirst.Add(listDataRowsMatches[i]);
                else if (firstNameInDb.Length >= 2 && patFname.StartsWith(firstNameInDb))
                    //Unfortunately, in the real world, we have observed carriers returning the patients first name followed by a space followed by the patient middle name all within the first name field.
                    //This issue is probably due to human error when the carrier's staff typed the patient name into their system.  All we can do is try to cope with this situation.
                    listDataRowsPartFirst.Add(listDataRowsMatches[i]);
            }

            if (listDataRowsExactFirst.Count > 0)
                listDataRowsMatches = listDataRowsExactFirst; //One or more exact matches found.  Ignore any partial matches.
            else
                listDataRowsMatches = listDataRowsPartFirst; //Use partial matches only if no exact matches were found.

            #endregion

            #region SubscriberID (exact/partial) matching.

            //Locate claims matching exact subscriber ID or partial subscriber ID, with a preference for exact match.
            var listDataRowsExactId = new List<DataRow>();
            var listDataRowsPartId = new List<DataRow>();
            var subscriberId = x12ClaimMatch.SubscriberId.Trim().ToUpper();
            for (var i = 0; i < listDataRowsMatches.Count; i++)
            {
                var subIdInDb = SIn.String(listDataRowsMatches[i]["SubscriberID"].ToString()).Trim().ToUpper();
                if (subIdInDb == subscriberId)
                    listDataRowsExactId.Add(listDataRowsMatches[i]);
                else if (subIdInDb.Length >= 3 && (subscriberId == subIdInDb.Substring(0, subIdInDb.Length - 1) || subscriberId == subIdInDb.Substring(0, subIdInDb.Length - 2)))
                    //Partial subscriber ID matches are somewhat common.
                    //Insurance companies sometimes create a base subscriber ID for all family members, then append a one or two digit number to make IDs unique for each family member.
                    //We have seen at least one real world example where the ERA contained the base subscriber ID instead of the patient specific ID.
                    //We also check that the subscriber ID in OD is at least 3 characters long, because we must allow for the 2 optional ending characters and we require an extra leading character to avoid matching blank IDs.
                    listDataRowsPartId.Add(listDataRowsMatches[i]);
                else if (subscriberId.Length >= 3 && (subIdInDb == subscriberId.Substring(0, subscriberId.Length - 1) || subIdInDb == subscriberId.Substring(0, subscriberId.Length - 2)))
                    //Partial match in the other direction.  Comparable to the scenario above.
                    listDataRowsPartId.Add(listDataRowsMatches[i]);
                else if (subscriberId.Length >= 3 && subIdInDb.TrimStart('0') == subscriberId)
                    //Allow matches for leading zeros.
                    listDataRowsPartId.Add(listDataRowsMatches[i]);
                else if (subIdInDb.Length >= 3 && subscriberId.TrimStart('0') == subIdInDb)
                    //Allow matches for leading zeros.
                    listDataRowsPartId.Add(listDataRowsMatches[i]);
            }

            if (listDataRowsExactId.Count > 0)
                listDataRowsMatches = listDataRowsExactId; //One or more exact matches found.  Ignore any partial matches.
            else
                listDataRowsMatches = listDataRowsPartId; //Use partial matches only if no exact matches were found.

            #endregion

            long matchClaimNum = 0;
            //We have finished locating the matches.  Now decide what to do based on the number of matches found.
            if (listDataRowsMatches.Count == 0)
                matchClaimNum = 0;
            else if (listDataRowsMatches.Count == 1)
                //A single match based on patient first name, patient last name, subscriber ID, claim date of service, and claim fee.
                matchClaimNum = SIn.Long(listDataRowsMatches[0]["ClaimNum"].ToString());
            else if (listDataRowsMatches.Count > 1)
                //Edge case.
                //Multiple matches (rare).  We might be able to pick the correct claim based on the attached procedures, but we can worry about this situation later if it happens more than we expect.
                matchClaimNum = 0;
            foreach (var match in dictionaryMatchesPerClaimId[etransNum][claimIdentifier])
            {
                var index = listX12ClaimMatches.IndexOf(match);
                listClaimNums[index] = matchClaimNum;
            }
        } //end foreach claim identifier

        //end foreach etrans/835
        return listClaimNums;
    }

    /// <summary>
    ///     We always require the claim fee and dates of service to match, then we use additional criteria to wisely choose
    ///     from the shorter list
    ///     of claims.  The list of claims with matching fee and date of service should be very short.  Worst case, the list
    ///     would contain all of the
    ///     claims if every claim had the same fee (rare).
    ///     Includes PreAuths which have a date of service 0001-01-01.
    /// </summary>
    public static DataTable GetClaimTable(List<DateTime> listDateTimes, List<double> listClaimFees)
    {
        var command = $@"SELECT a.ClaimNum,a.ClaimIdentifier,a.ClaimStatus,a.ClaimFee,a.DateService,patient.LName,patient.FName,inssub.SubscriberID
				FROM (
					SELECT claim.ClaimNum,claim.ClaimIdentifier,claim.ClaimStatus,ROUND(ClaimFee, 2) ClaimFee,claim.DateService,claim.PatNum,claim.InsSubNum,claim.PlanNum
					FROM claim
					WHERE DateService IN ({string.Join(",", listDateTimes.Select(x => SOut.Date(x)))})
					UNION
					SELECT claim.ClaimNum,claim.ClaimIdentifier,claim.ClaimStatus,ROUND(ClaimFee, 2) ClaimFee,claim.DateService,claim.PatNum,claim.InsSubNum,claim.PlanNum
					FROM claim
					WHERE ClaimType='PreAuth' AND DateService={DbHelper.Year("DateService")}=1 AND SecDateEntry>{SOut.Date(listDateTimes.Min())}-INTERVAL 1 YEAR
				) a
				INNER JOIN patient ON patient.PatNum=a.PatNum
				INNER JOIN inssub ON inssub.InsSubNum=a.InsSubNum AND a.PlanNum=inssub.PlanNum
				WHERE ClaimFee IN ({string.Join(",", listClaimFees.Select(x => SOut.Double(x)))})";
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     We always require the claim fee and dates of service to match, then we use additional criteria to wisely choose
    ///     from the shorter list
    ///     of claims.  The list of claims with matching fee and date of service should be very short.  Worst case, the list
    ///     would contain all of the
    ///     claims if every claim had the same fee (rare).
    ///     Includes PreAuths which have a date of service 0001-01-01.
    /// </summary>
    public static DataTable GetClaimTable(DateTime dateMin, DateTime dateMax, List<double> listClaimFees)
    {
        var command = $@"SELECT a.ClaimNum,a.ClaimIdentifier,a.ClaimStatus,a.ClaimFee,a.DateService,patient.LName,patient.FName,inssub.SubscriberID
				FROM (
					SELECT claim.ClaimNum,claim.ClaimIdentifier,claim.ClaimStatus,ROUND(ClaimFee, 2) ClaimFee,claim.DateService,claim.PatNum,claim.InsSubNum,claim.PlanNum
					FROM claim
					WHERE ({DbHelper.BetweenDates("DateService", dateMin, dateMax)})
					UNION
					SELECT claim.ClaimNum,claim.ClaimIdentifier,claim.ClaimStatus,ROUND(ClaimFee, 2) ClaimFee,claim.DateService,claim.PatNum,claim.InsSubNum,claim.PlanNum
					FROM claim
					WHERE ClaimType='PreAuth' AND DateService={DbHelper.Year("DateService")}=1 AND SecDateEntry>{SOut.Date(dateMin)}-INTERVAL 1 YEAR
				) a
				INNER JOIN patient ON patient.PatNum=a.PatNum
				INNER JOIN inssub ON inssub.InsSubNum=a.InsSubNum AND a.PlanNum=inssub.PlanNum
				WHERE ClaimFee IN ({string.Join(",", listClaimFees.Select(x => SOut.Double(x)))})";
        return DataCore.GetTable(command);
    }

    ///<summary>Returns the number of received claims attached to specified insplan.</summary>
    public static int GetCountReceived(long planNum)
    {
        return GetCountReceived(planNum, 0);
    }

    /// <summary>
    ///     Returns the number of received claims attached to specified subscriber with specified insplan.  Set insSubNum
    ///     to zero to check all claims for all patients for the plan.
    /// </summary>
    public static int GetCountReceived(long planNum, long insSubNum)
    {
        string command;
        command = "SELECT COUNT(*) "
                  + "FROM claim "
                  + "WHERE claim.ClaimStatus='R' "
                  + "AND claim.PlanNum=" + SOut.Long(planNum) + " ";
        if (insSubNum != 0) command += "AND claim.InsSubNum=" + SOut.Long(insSubNum);
        return SIn.Int(Db.GetCount(command));
    }

    ///<summary>Updates ClaimIdentifier for specified claim.</summary>
    public static void UpdateClaimIdentifier(long claimNum, string claimIdentifier)
    {
        var command = "UPDATE claim SET ClaimIdentifier='" + SOut.String(claimIdentifier) + "' WHERE ClaimNum=" + SOut.Long(claimNum);
        Db.NonQ(command);
    }

    ///<summary>Performs CalculateAndUpdateSecondaries given a list of primary claims. Grabs all necessary information.</summary>
    public static void CalculateAndUpdateSecondariesFromPrimaries(List<Claim> listPrimaryClaims)
    {
        if (listPrimaryClaims.Count < 1) return;
        var listClaimNums = listPrimaryClaims.Select(x => x.ClaimNum).ToList();
        var listClaimProcs = ClaimProcs.RefreshForClaims(listClaimNums);
        var listSecondaryClaims = GetPrimaryOrSecondaryClaimsNotReceived(listClaimProcs);
        CalculateAndUpdateSecondaries(listSecondaryClaims);
    }

    ///<summary>Performs CalculateAndUpdate on a list of secondary claims. Grabs all necessary information.</summary>
    public static void CalculateAndUpdateSecondaries(List<Claim> listSecondaryClaims)
    {
        if (listSecondaryClaims.Count < 1) return;
        var listSecondaryClaimNums = listSecondaryClaims.Select(x => x.ClaimNum).ToList();
        var listClaimProcs = ClaimProcs.RefreshForClaims(listSecondaryClaimNums);
        var listProcedures = Procedures.GetProcsFromClaimProcs(listClaimProcs);
        var listPatNums = listSecondaryClaims.Select(x => x.PatNum).Distinct().ToList();
        var listFamilies = Patients.GetFamilies(listPatNums);
        var listPatients = Patients.GetMultPats(listPatNums).ToList();
        var listInsSubs = InsSubs.GetPatientData(listPatients);
        var listInsPlans = InsPlans.GetPatientData(listInsSubs);
        var listPatPlans = PatPlans.GetPatPlansForPats(listPatNums);
        var listBenefits = Benefits.GetAllForPatPlans(listPatPlans, listInsSubs);
        for (var i = 0; i < listSecondaryClaims.Count; i++)
        {
            var patient = listPatients.Find(x => x.PatNum == listSecondaryClaims[i].PatNum);
            var listPatNumsFamily = listFamilies.Find(x => x.Guarantor.PatNum == patient.Guarantor).ListPats.Select(x => x.PatNum).ToList();
            var listInsSubNumsBySubscriber = listPatPlans.FindAll(x => listPatNumsFamily.Contains(x.PatNum)).Select(x => x.InsSubNum).ToList();
            var listInsSubsRelevant = listInsSubs.FindAll(x => listInsSubNumsBySubscriber.Contains(x.InsSubNum));
            var listClaimProcsRelevant = listClaimProcs.FindAll(x => x.ClaimNum == listSecondaryClaims[i].ClaimNum);
            var listProcNumsRelevant = listClaimProcsRelevant.Select(x => x.ProcNum).ToList();
            var listProceduresRelevant = listProcedures.FindAll(x => listProcNumsRelevant.Contains(x.ProcNum));
            var listInsPlanNumsRelevant = listInsSubsRelevant.Select(x => x.PlanNum).ToList();
            var listPatPlansRelevant = listPatPlans.FindAll(x => x.PatNum == patient.PatNum);
            var listPatPlanNumsRelevant = listPatPlansRelevant.Select(x => x.PatPlanNum).ToList();
            var listInsPlansRelevant = listInsPlans.FindAll(x => listInsPlanNumsRelevant.Contains(x.PlanNum));
            var listBenefitsRelevant = listBenefits.FindAll(x => listInsPlanNumsRelevant.Contains(x.PlanNum) || listPatPlanNumsRelevant.Contains(x.PatPlanNum));
            CalculateAndUpdate(listProceduresRelevant, listInsPlansRelevant, listSecondaryClaims[i], listPatPlansRelevant, listBenefitsRelevant, patient, listInsSubsRelevant);
        }
    }

    /// <summary>
    ///     Updates all claimproc estimates and also updates claim totals to db. Must supply procList which includes all
    ///     procedures that this
    ///     claim is linked to. Will also need to refresh afterwards to see the results.
    ///     If the Claim is "S" Sent or "R" Received, FeeBilled and ClaimFee will not be updated.
    /// </summary>
    public static void CalculateAndUpdate(List<Procedure> listProcedures, List<InsPlan> listInsPlans, Claim claim, List<PatPlan> listPatPlans, List<Benefit> listBenefits, Patient patient, List<InsSub> listInsSubs)
    {
        //we need more than just the claimprocs for this claim.
        //in order to run Procedures.ComputeEstimates, we need all claimprocs for all procedures attached to this claim
        var listClaimProcsAll = ClaimProcs.Refresh(claim.PatNum);
        var listClaimProcsForClaim = ClaimProcs.RefreshForClaim(claim.ClaimNum); //will be ordered by line number.
        var listSubstitutionLinks = SubstitutionLinks.GetAllForPlans(listInsPlans); //from db.  If we don't do it here, it will get done in loops in Procedures.ComputeEstimates.
        var listProcedureCodes = new List<ProcedureCode>();
        var isFeeBilledUpdateNeeded = !claim.ClaimStatus.In("R", "S"); //If claimCur is not received/sent then we can update the feeBilled/claimFee
        for (var i = 0; i < listProcedures.Count; i++) listProcedureCodes.Add(ProcedureCodes.GetProcCode(listProcedures[i].CodeNum));
        var discountPlanNum = DiscountPlanSubs.GetDiscountPlanNumForPat(patient.PatNum);
        var listFees = Fees.GetListFromObjects(listProcedureCodes, listProcedures.Select(x => x.MedicalCode).ToList(), listProcedures.Select(x => x.ProvNum).ToList(),
            patient.PriProv, patient.SecProv, patient.FeeSched, listInsPlans, listProcedures.Select(x => x.ClinicNum).ToList(),
            null, //appts not needed because not setting providers. We already have provs. 
            listSubstitutionLinks, discountPlanNum);
        double claimFee = 0;
        double dedApplied = 0;
        double insPayEst = 0;
        double insPayAmt = 0;
        double writeoff = 0;
        var insPlan = InsPlans.GetPlan(claim.PlanNum, listInsPlans);
        if (insPlan == null) return;
        var patPlanNum = PatPlans.GetPatPlanNum(claim.InsSubNum, listPatPlans);
        //first loop handles totals for received items.
        for (var i = 0; i < listClaimProcsForClaim.Count; i++)
        {
            if (listClaimProcsForClaim[i].Status != ClaimProcStatus.Received) continue; //disregard any status except Receieved.
            claimFee += listClaimProcsForClaim[i].FeeBilled;
            dedApplied += listClaimProcsForClaim[i].DedApplied;
            insPayEst += listClaimProcsForClaim[i].InsPayEst;
            insPayAmt += listClaimProcsForClaim[i].InsPayAmt;
            writeoff += listClaimProcsForClaim[i].WriteOff;
        }

        //loop again only for procs not received.
        //And for preauth.
        Procedure procedure;
        //InsPlan plan=InsPlans.GetPlan(claimCur.PlanNum,planList);
        var listClaimProcHists = ClaimProcs.GetHistList(claim.PatNum, listBenefits, listPatPlans, listInsPlans, claim.ClaimNum, claim.DateService, listInsSubs);
        var listClaimProcsOld = new List<ClaimProc>(); //make a copy
        for (var i = 0; i < listClaimProcsAll.Count; i++) listClaimProcsOld.Add(listClaimProcsAll[i].Copy());
        var listClaimProcHistsLoop = new List<ClaimProcHist>();
        //Get data for any OrthoCases that may be linked to procs in listProcedures
        var patNum = patient.PatNum;
        var listOrthoCases = OrthoCases.Refresh(patNum);
        var listOrthoProcLinksAll = OrthoProcLinks.GetManyByOrthoCases(listOrthoCases.Select(x => x.OrthoCaseNum).ToList());
        var listProcNums = listProcedures.Select(x => x.ProcNum).ToList();
        var listOrthoProcLinks = listOrthoProcLinksAll.FindAll(x => listProcNums.Contains(x.ProcNum));
        var listOrthoSchedules = new List<OrthoSchedule>();
        if (listOrthoProcLinks.Count > 0)
        {
            var listSchedulePlanLinksFKey = OrthoPlanLinks.GetAllForOrthoCasesByType(listOrthoCases.Select(x => x.OrthoCaseNum).ToList(), OrthoPlanLinkType.OrthoSchedule).Select(x => x.FKey).ToList();
            listOrthoSchedules = OrthoSchedules.GetMany(listSchedulePlanLinksFKey);
        }

        for (var i = 0; i < listClaimProcsForClaim.Count; i++)
        {
            //loop through each proc
            procedure = Procedures.GetProcFromList(listProcedures, listClaimProcsForClaim[i].ProcNum);
            //in order for ComputeEstimates to give accurate Writeoff when creating a claim, InsPayEst must be filled for the claimproc with status of NotReceived.
            //So, we must set it here.  We need to set it in the claimProcsAll list.  Find the matching one.
            for (var j = 0; j < listClaimProcsAll.Count; j++)
                if (listClaimProcsAll[j].ClaimProcNum == listClaimProcsForClaim[i].ClaimProcNum)
                    //same claimproc in a different list
                    if (listClaimProcsForClaim[i].Status == ClaimProcStatus.NotReceived)
                        //ignores received, etc
                        listClaimProcsAll[j].InsPayEst = ClaimProcs.GetInsEstTotal(listClaimProcsAll[j]);

            //When this is the secondary claim, HistList includes the primary estimates, which is something we don't want because the primary calculations gets confused.
            //So, we must remove those bad entries from histList.
            for (var h = listClaimProcHists.Count - 1; h >= 0; h--)
            {
                //loop through the histList backwards
                if (listClaimProcHists[h].ProcNum != procedure.ProcNum) continue; //Makes sure we will only be excluding histList entries for procs on this claim.
                //we already excluded this claimNum when getting the histList.
                if (listClaimProcHists[h].Status != ClaimProcStatus.NotReceived) continue; //The only ones that are a problem are the ones on the primary claim not received yet.
                listClaimProcHists.RemoveAt(h);
            }

            OrthoCase orthoCase = null;
            OrthoSchedule orthoSchedule = null;
            List<OrthoProcLink> listOrthoProcLinksForOrthoCase = null;
            var orthoProcLink = listOrthoProcLinks.Find(x => x.ProcNum == procedure.ProcNum);
            if (orthoProcLink != null)
            {
                var orthoCaseNum = orthoProcLink.OrthoCaseNum;
                orthoCase = listOrthoCases.Find(x => x.OrthoCaseNum == orthoCaseNum);
                orthoSchedule = listOrthoSchedules.Find(x => x.OrthoScheduleNum == orthoCaseNum);
                listOrthoProcLinksForOrthoCase = listOrthoProcLinksAll.FindAll(x => x.OrthoCaseNum == orthoCaseNum);
            }

            Procedures.ComputeEstimates(procedure, claim.PatNum, ref listClaimProcsAll, false, listInsPlans, listPatPlans, listBenefits, listClaimProcHists, listClaimProcHistsLoop, false, patient.Age
                , listInsSubs, listSubstLinks: listSubstitutionLinks, listFees: listFees,
                orthoProcLink: orthoProcLink, orthoCase: orthoCase, orthoSchedule: orthoSchedule, listOrthoProcLinksForOrthoCase: listOrthoProcLinksForOrthoCase);
            //then, add this information to loopList so that the next procedure is aware of it.
            //Exclude preauths becase thier estimates would incorrectly add both NotRecieved and Preauth estimates when calculating limitations.
            var listClaimProcs = listClaimProcsAll.FindAll(x => x.ProcNum == procedure.ProcNum && x.Status != ClaimProcStatus.Preauth);
            listClaimProcHistsLoop.AddRange(ClaimProcs.GetHistForProc(listClaimProcs, procedure, procedure.CodeNum));
        }

        //save changes in the list to the database
        ClaimProcs.Synch(ref listClaimProcsAll, listClaimProcsOld);
        listClaimProcsForClaim = ClaimProcs.RefreshForClaim(claim.ClaimNum);
        //But ClaimProcsAll has not been refreshed.
        for (var i = 0; i < listClaimProcsForClaim.Count; i++)
        {
            if (listClaimProcsForClaim[i].Status != ClaimProcStatus.NotReceived
                && listClaimProcsForClaim[i].Status != ClaimProcStatus.Preauth
                && listClaimProcsForClaim[i].Status != ClaimProcStatus.CapClaim)
                continue;
            procedure = Procedures.GetProcFromList(listProcedures, listClaimProcsForClaim[i].ProcNum);
            if (procedure.ProcNum == 0) continue; //ignores payments, etc
            //fee:
            if (!listClaimProcsForClaim[i].IsOverpay && isFeeBilledUpdateNeeded && insPlan.ClaimsUseUCR)
            {
                //use UCR for the provider of the procedure
                var provNum = procedure.ProvNum;
                if (provNum == 0) //if no prov set, then use practice default.
                    provNum = PrefC.GetLong(PrefName.PracticeDefaultProv);
                var providerFirst = Providers.GetFirst(); //Used in order to preserve old behavior...  If this fails, then old code would have failed.
                var provider = Providers.GetFirstOrDefault(x => x.ProvNum == provNum) ?? providerFirst;
                //get the fee based on code and prov fee sched
                var ppoFee = Fees.GetAmount0(procedure.CodeNum, provider.FeeSched, procedure.ClinicNum, provNum, listFees);
                var ucrFee = procedure.ProcFee; //Usual Customary and Regular (UCR) fee.  Also known as billed fee.
                if (ucrFee > ppoFee)
                    listClaimProcsForClaim[i].FeeBilled = procedure.Quantity * ucrFee;
                else
                    listClaimProcsForClaim[i].FeeBilled = procedure.Quantity * ppoFee;
            }
            else if (!listClaimProcsForClaim[i].IsOverpay && isFeeBilledUpdateNeeded)
            {
                //don't use ucr. Use the procedure fee instead.
                listClaimProcsForClaim[i].FeeBilled = procedure.ProcFeeTotal;
            }

            if (!listClaimProcsForClaim[i].IsOverpay) claimFee += listClaimProcsForClaim[i].FeeBilled;
            if (claim.ClaimType == "PreAuth" || claim.ClaimType == "Cap" || (claim.ClaimType == "Other" && !insPlan.IsMedical))
            {
                //12-18-2015 ==tg:  We added medical plans as an exclusion to the above logic.  In past versions Medical plans did not copy over values into
                //the claimproc InsPayEst, DedApplied, or Writeoff columns.  DG and I determined that for now this is acceptable.	 If we ever implement a 
                //medical claimtype in the future, or if there are issues with claims this will need to be changed.
                ClaimProcs.Update(listClaimProcsForClaim[i]); //only the fee gets calculated, the rest does not
                continue;
            }

            //ClaimProcs.ComputeBaseEst(ClaimProcsForClaim[i],ProcCur.ProcFee,ProcCur.ToothNum,ProcCur.CodeNum,plan,patPlanNum,benefitList,histList,loopList);
            listClaimProcsForClaim[i].InsPayEst = ClaimProcs.GetInsEstTotal(listClaimProcsForClaim[i]); //Yes, this is duplicated from further up.
            listClaimProcsForClaim[i].DedApplied = ClaimProcs.GetDedEst(listClaimProcsForClaim[i]);
            if (listClaimProcsForClaim[i].Status == ClaimProcStatus.NotReceived)
            {
                //(vs preauth)
                listClaimProcsForClaim[i].WriteOff = ClaimProcs.GetWriteOffEstimate(listClaimProcsForClaim[i]);
                writeoff += listClaimProcsForClaim[i].WriteOff;
                /*
                ClaimProcsForClaim[i].WriteOff=0;
                if(claimCur.ClaimType=="P" && plan.PlanType=="p") {//Primary && PPO
                    double insplanAllowed=Fees.GetAmount(ProcCur.CodeNum,plan.FeeSched);
                    if(insplanAllowed!=-1) {
                        ClaimProcsForClaim[i].WriteOff=ProcCur.ProcFee-insplanAllowed;
                    }
                    //else, if -1 fee not found, then do not show a writeoff. User can change writeoff if they disagree.
                }
                writeoff+=ClaimProcsForClaim[i].WriteOff;*/
            }

            dedApplied += listClaimProcsForClaim[i].DedApplied;
            insPayEst += listClaimProcsForClaim[i].InsPayEst;
            if (CultureInfo.CurrentCulture.Name.EndsWith("CA") && listProcedures.Exists(x => x.ProcNumLab == listClaimProcsForClaim[i].ProcNum))
            {
                //In Canada we will need to consider lab insurance estimates.
                var listLabProcNums = listProcedures.FindAll(x => x.ProcNumLab == listClaimProcsForClaim[i].ProcNum).Select(x => x.ProcNum).ToList();
                insPayEst += listClaimProcsAll.FindAll(x => listLabProcNums.Contains(x.ProcNum)
                                                            && x.InsSubNum == listClaimProcsForClaim[i].InsSubNum && x.PlanNum == listClaimProcsForClaim[i].PlanNum)
                    .Sum(x => ClaimProcs.GetInsEstTotal(x));
            }

            ClaimProcs.Update(listClaimProcsForClaim[i]);
            //but notice that the ClaimProcs lists are not refreshed until the loop is finished.
        } //for claimprocs.forclaim

        if (isFeeBilledUpdateNeeded) //if claimStatus is sent or recieved, don't update. This may be unneccessary but it doesn't hurt anything and it makes make our intentions clear
            claim.ClaimFee = claimFee;
        claim.DedApplied = dedApplied;
        claim.InsPayEst = insPayEst;
        claim.InsPayAmt = insPayAmt;
        claim.WriteOff = writeoff;
        //Cur=ClaimCur;
        Update(claim);
    }

    ///<summary>Creates a claim for a newly created repeat charge procedure.</summary>
    public static Claim CreateClaimForRepeatCharge(string claimType, List<PatPlan> listPatPlans, List<InsPlan> listInsPlans, List<ClaimProc> listClaimProcs,
        Procedure procedure, List<InsSub> listInsSubs, Patient patient)
    {
        long claimFormNum = 0;
        var insPlan = new InsPlan();
        var insSub = new InsSub();
        var relatOther = Relat.Self;
        switch (claimType)
        {
            case "P":
                insSub = InsSubs.GetSub(PatPlans.GetInsSubNum(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Primary, listPatPlans, listInsPlans, listInsSubs)), listInsSubs);
                insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlans);
                break;
            case "S":
                insSub = InsSubs.GetSub(PatPlans.GetInsSubNum(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Secondary, listPatPlans, listInsPlans, listInsSubs)), listInsSubs);
                insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlans);
                break;
            case "Med":
                //It's already been verified that a med plan exists
                insSub = InsSubs.GetSub(PatPlans.GetInsSubNum(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Medical, listPatPlans, listInsPlans, listInsSubs)), listInsSubs);
                insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlans);
                break;
        }

        var claimProc = Procedures.GetClaimProcEstimate(procedure.ProcNum, listClaimProcs, insPlan, insSub.InsSubNum);
        if (claimProc == null)
        {
            claimProc = new ClaimProc();
            ClaimProcs.CreateEst(claimProc, procedure, insPlan, insSub);
        }

        var claim = new Claim();
        claim.PatNum = procedure.PatNum;
        claim.DateService = procedure.ProcDate;
        claim.ClinicNum = procedure.ClinicNum;
        claim.PlaceService = procedure.PlaceService;
        claim.ClaimStatus = "W";
        claim.DateSent = DateTime.Today;
        claim.DateSentOrig = DateTime.MinValue;
        claim.PlanNum = insPlan.PlanNum;
        claim.InsSubNum = insSub.InsSubNum;
        InsSub insSub2;
        switch (claimType)
        {
            case "P":
                claim.PatRelat = PatPlans.GetRelat(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Primary, listPatPlans, listInsPlans, listInsSubs));
                claim.ClaimType = "P";
                claim.InsSubNum2 = PatPlans.GetInsSubNum(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Secondary, listPatPlans, listInsPlans, listInsSubs));
                insSub2 = InsSubs.GetSub(claim.InsSubNum2, listInsSubs);
                if (insSub2.PlanNum > 0 && InsPlans.RefreshOne(insSub2.PlanNum).IsMedical)
                {
                    claim.PlanNum2 = 0; //no sec ins
                    claim.PatRelat2 = Relat.Self;
                    break;
                }

                claim.PlanNum2 = insSub2.PlanNum; //might be 0 if no sec ins
                claim.PatRelat2 = PatPlans.GetRelat(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Secondary, listPatPlans, listInsPlans, listInsSubs));
                break;
            case "S":
                claim.PatRelat = PatPlans.GetRelat(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Secondary, listPatPlans, listInsPlans, listInsSubs));
                claim.ClaimType = "S";
                claim.InsSubNum2 = PatPlans.GetInsSubNum(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Primary, listPatPlans, listInsPlans, listInsSubs));
                insSub2 = InsSubs.GetSub(claim.InsSubNum2, listInsSubs);
                claim.PlanNum2 = insSub2.PlanNum;
                claim.PatRelat2 = PatPlans.GetRelat(listPatPlans, PatPlans.GetOrdinal(PriSecMed.Primary, listPatPlans, listInsPlans, listInsSubs));
                break;
            case "Med":
                claim.PatRelat = PatPlans.GetFromList(listPatPlans, insSub.InsSubNum).Relationship;
                claim.ClaimType = "Other";
                if (PrefC.GetBool(PrefName.ClaimMedTypeIsInstWhenInsPlanIsMedical))
                    claim.MedType = EnumClaimMedType.Institutional;
                else
                    claim.MedType = EnumClaimMedType.Medical;
                break;
            case "Other":
                claim.PatRelat = relatOther;
                claim.ClaimType = "Other";
                //plannum2 is not automatically filled in.
                claim.ClaimForm = claimFormNum;
                if (!insPlan.IsMedical) break;
                if (PrefC.GetBool(PrefName.ClaimMedTypeIsInstWhenInsPlanIsMedical))
                    claim.MedType = EnumClaimMedType.Institutional;
                else
                    claim.MedType = EnumClaimMedType.Medical;
                break;
        }

        if (insPlan.PlanType == "c") //if capitation
            claim.ClaimType = "Cap";
        claim.ProvTreat = procedure.ProvNum;
        if (Providers.GetIsSec(procedure.ProvNum)) claim.ProvTreat = patient.PriProv;
        //OK if zero, because auto select first in list when open claim
        claim.IsProsthesis = "N";
        claim.ProvBill = Providers.GetBillingProvNum(claim.ProvTreat, claim.ClinicNum); //OK if zero, because it will get fixed in claim
        claim.EmployRelated = YN.No;
        claim.ClaimForm = insPlan.ClaimFormNum;
        claim.AttachedFlags = "Mail";
        Insert(claim);
        claim.ClaimIdentifier = ConvertClaimId(claim, patient);
        Update(claim);
        //attach procedure
        claimProc.ClaimNum = claim.ClaimNum;
        if (insPlan.PlanType == "c") //if capitation
            claimProc.Status = ClaimProcStatus.CapClaim;
        else
            claimProc.Status = ClaimProcStatus.NotReceived;
        if (insPlan.UseAltCode && ProcedureCodes.GetProcCode(procedure.CodeNum).AlternateCode1 != "")
        {
            claimProc.CodeSent = ProcedureCodes.GetProcCode(procedure.CodeNum).AlternateCode1;
        }
        else if (insPlan.IsMedical && procedure.MedicalCode != "")
        {
            claimProc.CodeSent = procedure.MedicalCode;
        }
        else
        {
            claimProc.CodeSent = ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode;
            if (claimProc.CodeSent.Length > 5 && claimProc.CodeSent.Substring(0, 1) == "D") claimProc.CodeSent = claimProc.CodeSent.Substring(0, 5);
            if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
                if (claimProc.CodeSent.Length > 5)
                    //In Canadian e-claims, codes can contain letters or numbers and cannot be longer than 5 characters.
                    claimProc.CodeSent = claimProc.CodeSent.Substring(0, 5);
        }

        claimProc.LineNumber = 1;
        ClaimProcs.Update(claimProc);
        return claim;
    }

    ///<summary>Create claim for the automatic ortho procedure.</summary>
    public static Claim CreateClaimForOrthoProc(string claimType, PatPlan patPlan, InsPlan insPlan, InsSub insSub,
        ClaimProc claimProc, Procedure procedure, double feeBilled, DateTime dateBanding, int totalMonths, int monthsRem)
    {
        var claimProc2 = Procedures.GetClaimProcEstimate(procedure.ProcNum, new List<ClaimProc> {claimProc}, insPlan, insSub.InsSubNum);
        var listPatPlansForPat = PatPlans.Refresh(patPlan.PatNum);
        var listInsPlansForPat = InsPlans.GetByInsSubs(listPatPlansForPat.Select(x => x.InsSubNum).ToList());
        var listInsSubsForPat = InsSubs.GetMany(listPatPlansForPat.Select(x => x.InsSubNum).ToList());
        if (claimProc == null)
        {
            claimProc = new ClaimProc();
            ClaimProcs.CreateEst(claimProc, procedure, insPlan, insSub);
        }

        var claim = new Claim();
        claim.PatNum = procedure.PatNum;
        claim.DateService = procedure.ProcDate;
        claim.ClinicNum = procedure.ClinicNum;
        claim.PlaceService = procedure.PlaceService;
        claim.ClaimStatus = "W";
        claim.DateSent = DateTime.Today;
        claim.DateSentOrig = DateTime.MinValue;
        claim.PlanNum = insPlan.PlanNum;
        claim.InsSubNum = insSub.InsSubNum;
        claim.ClaimFee = feeBilled;
        if (PrefC.GetBool(PrefName.OrthoClaimMarkAsOrtho)) claim.IsOrtho = true;
        if (PrefC.GetBool(PrefName.OrthoClaimUseDatePlacement))
        {
            claim.OrthoDate = dateBanding;
            claim.OrthoTotalM = SIn.Byte(totalMonths.ToString(), false);
            claim.OrthoRemainM = SIn.Byte(monthsRem.ToString(), false);
        }

        InsSub insSub2;
        PatPlan patPlanOther;
        switch (claimType)
        {
            case "P":
                claim.PatRelat = patPlan.Relationship;
                claim.ClaimType = "P";
                patPlanOther = PatPlans.GetPatPlan(patPlan.PatNum, PatPlans.GetOrdinal(PriSecMed.Secondary, listPatPlansForPat, listInsPlansForPat, listInsSubsForPat));
                if (patPlanOther == null)
                {
                    claim.InsSubNum2 = 0;
                    claim.PlanNum2 = 0; //no sec ins
                    claim.PatRelat2 = Relat.Self;
                    break;
                }

                insSub2 = InsSubs.GetOne(patPlanOther.InsSubNum);
                if (insSub2.PlanNum > 0 && !InsPlans.RefreshOne(insSub2.PlanNum).IsMedical)
                {
                    claim.PlanNum2 = insSub2.PlanNum; //might be 0 if no sec ins
                    claim.PatRelat2 = patPlanOther.Relationship;
                    claim.InsSubNum2 = insSub2.InsSubNum;
                }

                break;
            case "S":
                claim.PatRelat = patPlan.Relationship;
                claim.ClaimType = "S";
                patPlanOther = PatPlans.GetPatPlan(patPlan.PatNum, PatPlans.GetOrdinal(PriSecMed.Primary, listPatPlansForPat, listInsPlansForPat, listInsSubsForPat));
                if (patPlanOther == null)
                {
                    //should never happen
                    claim.InsSubNum2 = 0;
                    claim.PlanNum2 = 0;
                    claim.PatRelat2 = Relat.Self;
                    break;
                }

                insSub2 = InsSubs.GetOne(patPlanOther.InsSubNum);
                if (insSub2.PlanNum > 0 && !InsPlans.RefreshOne(insSub2.PlanNum).IsMedical)
                {
                    claim.PlanNum2 = insSub2.PlanNum;
                    claim.PatRelat2 = patPlanOther.Relationship;
                    claim.InsSubNum2 = insSub2.InsSubNum;
                }

                break;
        }

        if (insPlan.PlanType == "c") //if capitation
            claim.ClaimType = "Cap";
        claim.ProvTreat = procedure.ProvNum;
        if (Providers.GetIsSec(procedure.ProvNum)) claim.ProvTreat = Patients.GetPat(procedure.PatNum).PriProv;
        //OK if zero, because auto select first in list when open claim
        claim.IsProsthesis = "N";
        claim.ProvBill = Providers.GetBillingProvNum(claim.ProvTreat, claim.ClinicNum); //OK if zero, because it will get fixed in claim
        claim.EmployRelated = YN.No;
        claim.ClaimForm = insPlan.ClaimFormNum;
        claim.AttachedFlags = "Mail";
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            //Defaults to X in edit claim form
            claim.CanadianIsInitialUpper = "X";
            claim.CanadianIsInitialLower = "X";
        }

        Insert(claim);
        claim.ClaimIdentifier = ConvertClaimId(claim);
        Update(claim);
        //attach procedure
        claimProc.ClaimNum = claim.ClaimNum;
        if (insPlan.PlanType == "c") //if capitation
            claimProc.Status = ClaimProcStatus.CapClaim;
        else
            claimProc.Status = ClaimProcStatus.NotReceived;
        if (insPlan.UseAltCode && ProcedureCodes.GetProcCode(procedure.CodeNum).AlternateCode1 != "")
        {
            claimProc.CodeSent = ProcedureCodes.GetProcCode(procedure.CodeNum).AlternateCode1;
        }
        else if (insPlan.IsMedical && procedure.MedicalCode != "")
        {
            claimProc.CodeSent = procedure.MedicalCode;
        }
        else
        {
            claimProc.CodeSent = ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode;
            if (claimProc.CodeSent.Length > 5 && claimProc.CodeSent.Substring(0, 1) == "D") claimProc.CodeSent = claimProc.CodeSent.Substring(0, 5);
            if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
                if (claimProc.CodeSent.Length > 5)
                    //In Canadian e-claims, codes can contain letters or numbers and cannot be longer than 5 characters.
                    claimProc.CodeSent = claimProc.CodeSent.Substring(0, 5);
        }

        claimProc.LineNumber = 1;
        claimProc.FeeBilled = feeBilled;
        ClaimProcs.Update(claimProc);
        return claim;
    }

    /// <summary>
    ///     Zeros securitylog FKey column for rows that are using the matching claimNums as FKey and are related to Claim.
    ///     Permtypes are generated from the AuditPerms property of the CrudTableAttribute within the Claim table type.
    /// </summary>
    public static void ClearFkey(List<long> listClaimNums)
    {
        ClaimCrud.ClearFkey(listClaimNums);
    }

    public static DateTime GetDateLastOrthoClaim(PatPlan patPlan, OrthoClaimType orthoClaimType)
    {
        var orthoDefaultAutoCodeNum = PrefC.GetLong(PrefName.OrthoAutoProcCodeNum);
        var command = "";
        if (orthoClaimType == OrthoClaimType.InitialPlusPeriodic)
            command = @"	
				SELECT MAX(claim.DateSent) LastSent
				FROM claim
				INNER JOIN claimproc ON claimproc.ClaimNum = claim.ClaimNum
				INNER JOIN insplan ON claim.PlanNum = insplan.PlanNum
				INNER JOIN procedurelog ON procedurelog.ProcNum = claimproc.ProcNum
					AND procedurelog.CodeNum LIKE 
						IF(insplan.OrthoAutoProcCodeNumOverride = 0, 
						" + orthoDefaultAutoCodeNum + @",
						insplan.OrthoAutoProcCodeNumOverride)
				WHERE claim.ClaimStatus IN ('S','R')
				AND claim.PatNum = " + patPlan.PatNum + @"
				AND claim.InsSubNum = " + patPlan.InsSubNum;
        else
            command = @"	
				SELECT MAX(claim.DateSent) LastSent
				FROM claim
				INNER JOIN claimproc ON claimproc.ClaimNum = claim.ClaimNum
				INNER JOIN insplan ON claim.PlanNum = insplan.PlanNum
				INNER JOIN procedurelog ON procedurelog.ProcNum = claimproc.ProcNum
				INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum
				INNER JOIN covspan ON covspan.FromCode <= procedurecode.ProcCode AND covspan.ToCode >= procedurecode.ProcCode
				INNER JOIN covcat ON covcat.CovCatNum = covspan.CovCatNum
					AND covcat.EbenefitCat = " + SOut.Int((int) EbenefitCategory.Orthodontics) + @"
				WHERE claim.ClaimStatus IN ('S','R')
				AND claim.PatNum = " + patPlan.PatNum + @"
				AND claim.InsSubNum = " + patPlan.InsSubNum;
        return SIn.Date(DataCore.GetScalar(command));
    }

    public static List<Claim> GetForPat(long patNum)
    {
        return ClaimCrud.SelectMany("SELECT * FROM claim WHERE PatNum = " + patNum);
    }

    /// <summary>
    ///     Gets the most recent ortho claim with a banding code attached.
    ///     Returns null if no ortho banding code nums found or no corresponding claim found.
    /// </summary>
    public static Claim GetOrthoBandingClaim(long patNum, long planNum)
    {
        var listProcCodeNums = ProcedureCodes.GetOrthoBandingCodeNums();
        if (listProcCodeNums == null || listProcCodeNums.Count < 1) return null;
        var command = @"
				SELECT claim.* 
				FROM claim
				WHERE claim.PatNum = " + SOut.Long(patNum) + @"
				AND claim.PlanNum = " + SOut.Long(planNum) + @"
				AND claim.IsOrtho = 1
				AND claim.ClaimStatus = 'R'
				AND EXISTS(
					SELECT * FROM claimproc
					INNER JOIN procedurelog ON claimproc.ProcNum = procedurelog.ProcNum
					INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum
						AND procedurecode.CodeNum IN (" + string.Join(",", listProcCodeNums) + @")
					WHERE claimproc.ClaimNum = claim.ClaimNum
				)
				ORDER BY claim.DateSent DESC";
        return ClaimCrud.SelectOne(command);
    }

    ///<summary>Returns the defalt/calculated claim ID based on the ClaimIdPrefix preference.</summary>
    public static string ConvertClaimId(Claim claim, Patient patient = null)
    {
        if (patient == null) patient = Patients.GetPat(claim.PatNum);
        return Patients.ReplacePatient(PrefC.GetString(PrefName.ClaimIdPrefix), patient) + claim.ClaimNum;
    }

    /// <summary>
    ///     Caller should validate claim and listClaimProcsToSplit prior to calling.
    ///     Inserts and updates a new split claim. Also updates the given claimOriginal to reflect new values.
    /// </summary>
    public static Claim InsertSplitClaim(Claim claimOriginal, List<ClaimProc> listClaimProcsToSplit, Patient patient = null)
    {
        var claimNew = claimOriginal.Copy();
        claimNew.ClaimFee = 0;
        claimNew.DedApplied = 0;
        claimNew.InsPayEst = 0;
        claimNew.InsPayAmt = 0;
        claimNew.WriteOff = 0;
        claimNew.CustomTracking = 0;
        Insert(claimNew); //We must insert here so that we have the primary key in the loop below.
        //Split claims can occur for two reasons:
        //1) The insurance company rejects a claim because of one procedure.  The office staff then split off the "faulty" procedure and submit the
        //original claim.  Then the office corrects the information on the procedure for the split claim and sends the split claim separately.  In this
        //case, the Claim Identifier on the split claim should be different than the Claim Identifier on the original claim, because both claims
        //are independent of each other.
        //2) The insurance company decides to split off one procedure because it will take more time to process than the other procedures.  They do
        //this so that the provider can receive most of their payment as quickly as possible.  In this case, the provider will notice on the EOB or ERA
        //that the claim was split and they will manually split the appropriate procedures from the original claim in OD.  The procedure on the split
        //claim has already been submitted to the insurance company and does not need to be sent.  The Claim Identifier on the original claim and split
        //claim will be the same when received in an ERA and should also be the same in OD.  However, if the Claim Identifier is different on the split
        //claim than on the original claim, ERA matching should still work because of our secondary matching methods.
        claimNew.ClaimIdentifier = ConvertClaimId(claimNew, patient);
        //Now this claim has been duplicated, except it has a new ClaimNum and new totals.  There are no attached claimprocs yet.
        for (var i = 0; i < listClaimProcsToSplit.Count; i++)
        {
            var claimProcOld = listClaimProcsToSplit[i].Copy();
            listClaimProcsToSplit[i].ClaimNum = claimNew.ClaimNum;
            listClaimProcsToSplit[i].PayPlanNum = 0; //detach from payplan if previously attached, claimprocs from two claims cannot be attached to the same payplan
            ClaimProcs.Update(listClaimProcsToSplit[i], claimProcOld); //moves it to the new claim
            claimNew.ClaimFee += listClaimProcsToSplit[i].FeeBilled;
            claimNew.DedApplied += listClaimProcsToSplit[i].DedApplied;
            claimNew.InsPayEst += listClaimProcsToSplit[i].InsPayEst;
            claimNew.InsPayAmt += listClaimProcsToSplit[i].InsPayAmt;
            claimNew.WriteOff += listClaimProcsToSplit[i].WriteOff;
        }

        Update(claimNew);
        ClaimTrackings.CopyToClaim(claimOriginal.ClaimNum, claimNew.ClaimNum);
        claimOriginal.ClaimFee -= claimNew.ClaimFee;
        claimOriginal.DedApplied -= claimNew.DedApplied;
        claimOriginal.InsPayEst -= claimNew.InsPayEst;
        claimOriginal.InsPayAmt -= claimNew.InsPayAmt;
        claimOriginal.WriteOff -= claimNew.WriteOff;
        Update(claimOriginal);
        ClaimProcs.RemoveSupplementalTransfersForClaims(claimOriginal.ClaimNum);
        InsBlueBooks.SynchForClaimNums(claimOriginal.ClaimNum, claimNew.ClaimNum);
        return claimNew;
    }

    /// <summary>
    ///     There is a Clinic override that will cause the InsPlan-level setting to be completely ignored.  Otherwise,
    ///     this just returns the insSub.AssignBen.  The override is based on the clinic of the subscriber.
    /// </summary>
    public static bool GetAssignmentOfBenefits(Claim claim, InsSub insSub)
    {
        var clinicPref = ClinicPrefs.GetPref(PrefName.InsDefaultAssignBen, claim.ClinicNum);
        if (clinicPref != null) //If the clinicpref exists, we know that we are always assigning to patient, which translates to false for the pref.
            //We don't actually check the value of the clinicpref.
            return false;
        return insSub.AssignBen;
    }

    ///<summary>Provides ClaimStatus enum from the current claim.</summary>
    public static ClaimStatus GetClaimStatusEnumFromCurClaim(Claim claim)
    {
        var listClaimStatuses = Enum.GetValues(typeof(ClaimStatus)).OfType<ClaimStatus>().ToList();
        if (!(PrefC.GetBool(PrefName.PriClaimAllowSetToHoldUntilPriReceived)
              || claim.ClaimType != "P" //Pref only applies to primary claims.
              || claim.ClaimStatus == ClaimStatus.HoldUntilPriReceived.GetDescription(true))) //Status not allowed but we must maintain current selection.
            listClaimStatuses = listClaimStatuses.FindAll(x => x != ClaimStatus.HoldUntilPriReceived); //Preference does not allow status and claim was not set to status prior to disabling
        for (var i = 0; i < listClaimStatuses.Count; i++)
        {
            if (claim.ClaimStatus != listClaimStatuses[i].GetDescription(true)) continue;
            return listClaimStatuses[i];
        }

        return ClaimStatus.Unsent; //Default
    }

    /// <summary>
    ///     Returns a list of strings detailing how procedures are over-credited and what their remaining balances would be.
    ///     Considers patient payments, insurance payments, write-offs, and adjustments.
    ///     Returns an empty list if no procedures are over-credited.
    /// </summary>
    public static List<string> GetAllCreditsGreaterThanProcFees(List<ClaimProc> listClaimProcsHypothetical)
    {
        var listProcDescripts = new List<string>();
        var listProceduresForClaimProcsDB = Procedures.GetManyProc(listClaimProcsHypothetical.Select(x => x.ProcNum).ToList(), false);
        //We want to use the ProcNums from the list of Procedures to get our other data so that we don't get splits and adjs for ProcNum 0.
        var listProcNums = listProceduresForClaimProcsDB.Select(x => x.ProcNum).ToList();
        var listClaimProcsDB = ClaimProcs.GetForProcs(listProcNums);
        var listPaySplitsForClaimProcsDB = PaySplits.GetPaySplitsFromProcs(listProcNums);
        var listAdjustmentsForClaimProcsDB = Adjustments.GetForProcs(listProcNums);
        for (var i = 0; i < listClaimProcsHypothetical.Count; i++)
        {
            var claimProc = listClaimProcsHypothetical[i];
            var procedure = listProceduresForClaimProcsDB.Find(x => x.ProcNum == claimProc.ProcNum);
            if (procedure == null) continue; //If ClaimProc is not associated to a procedure, it can't be directly overpaying one.
            //Get the sum of all insurance payments on this procedure. Add the InsPayAmt from the current claimproc separately because it may not be in DB yet.
            var insPaySum = (decimal) ClaimProcs.ProcInsPay(listClaimProcsDB.FindAll(x => x.ClaimProcNum != claimProc.ClaimProcNum), claimProc.ProcNum)
                            + (decimal) claimProc.InsPayAmt;
            //Get the sum of all write offs on this procedure. Add the Write-off from the current claimproc separately because it may not be in DB yet.
            var writeOffSum = (decimal) ClaimProcs.ProcWriteoff(listClaimProcsDB.FindAll(x => x.ClaimProcNum != claimProc.ClaimProcNum), claimProc.ProcNum)
                              + (decimal) claimProc.WriteOff;
            var fee = (decimal) procedure.ProcFeeTotal;
            //Get the sum of all adjustments made on this procedure.
            var adjAmtSum = listAdjustmentsForClaimProcsDB.FindAll(x => x.ProcNum == claimProc.ProcNum).Select(x => (decimal) x.AdjAmt).Sum();
            //Get the sum of all patient payments made on this procedure.
            var patPaySum = listPaySplitsForClaimProcsDB.FindAll(x => x.ProcNum == claimProc.ProcNum).Select(x => (decimal) x.SplitAmt).Sum();
            //Any changes to this calculation should also consider Claims.GetWriteOffGreaterThanProcFees() and Claims.GetWriteOffsAndInsPaymentsGreaterThanProcFees().
            //Calculate the remaining fee on the procedure after considering all patient payments, insurance payments, writeoffs, and adjustments.
            var feeRemaining = fee - patPaySum - insPaySum - writeOffSum + adjAmtSum;
            //In GetWriteOffGreaterThanProcFee() we check if the writeoff is > 0. We may want to check if insPayAmt+writeoff > 0 here.
            //Or we may only care if InsPayAmt+Writeoff is > 0 zero for the current claimproc. Should a user be blocked if a proc is overpaid,
            //but no value comes from the claimproc or claim we are evaluating?
            if (!CompareDecimal.IsLessThanZero(feeRemaining)) continue;
            listProcDescripts.Add(ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode
                                  + "\t" + Lans.g("ClaimS", "Fee") + ": " + fee.ToString("F")
                                  + "\t" + Lans.g("ClaimS", "Credits") + ": " + Math.Abs(-patPaySum - insPaySum - writeOffSum + adjAmtSum).ToString("F")
                                  + "\t" + Lans.g("ClaimS", "Remaining") + ": (" + Math.Abs(feeRemaining).ToString("F") + ")");
        }

        return listProcDescripts;
    }

    /// <summary>
    ///     Returns a list of strings detailing how write-offs over-credit the procedures and what their remaining balances
    ///     would be.
    ///     Considers write-offs and adjustments.
    ///     Returns an empty list if no write-offs over-credit the procedures.
    /// </summary>
    public static List<string> GetWriteOffsGreaterThanProcFees(List<ClaimProc> listClaimProcsHypothetical)
    {
        var listProcDescripts = new List<string>();
        var listProceduresForClaimProcsDB = Procedures.GetManyProc(listClaimProcsHypothetical.Select(x => x.ProcNum).ToList(), false);
        //We want to use the ProcNums from the list of Procedures to get our other data so that we don't get splits and adjs for ProcNum 0.
        var listProcNums = listProceduresForClaimProcsDB.Select(x => x.ProcNum).ToList();
        var listClaimProcsDB = ClaimProcs.GetForProcs(listProcNums);
        var listAdjustmentsForClaimProcsDB = Adjustments.GetForProcs(listProcNums);
        for (var i = 0; i < listClaimProcsHypothetical.Count; i++)
        {
            var claimProc = listClaimProcsHypothetical[i];
            var procedure = listProceduresForClaimProcsDB.Find(x => x.ProcNum == claimProc.ProcNum);
            if (procedure == null) continue; //If ClaimProc is not associated to a procedure, it can't be directly overpaying one.
            //Get the sum of all write offs on this procedure. Add the Write-off from the current claimproc separately because it may not be in DB yet.
            var writeoffSum = (decimal) ClaimProcs.ProcWriteoff(listClaimProcsDB.FindAll(x => x.ClaimProcNum != claimProc.ClaimProcNum), claimProc.ProcNum)
                              + (decimal) claimProc.WriteOff;
            var fee = (decimal) procedure.ProcFeeTotal;
            //Get the sum of all adjustments made on this procedure.
            var adjAmtSum = listAdjustmentsForClaimProcsDB.Where(x => x.ProcNum == claimProc.ProcNum).Select(x => (decimal) x.AdjAmt).Sum();
            //Any changes to this calculation should also consider Claims.GetAllCreditsGreaterThanProcFees() and Claims.GetWriteOffsAndInsPaymentsGreaterThanProcFees().
            //Calculate the remaining fee on the proedure after considering all writeoffs and adjustments.
            var feeRemaining = fee - writeoffSum + adjAmtSum;
            //We need to consider if the writeoff even has any value. This is how the original code was written,
            //but we may only care if the value of the current claimproc's writeoff is greater than zero. If that is the case just kick out above.
            if (!CompareDecimal.IsLessThanZero(feeRemaining) || !CompareDecimal.IsGreaterThanZero(writeoffSum)) continue;
            listProcDescripts.Add(ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode
                                  + "\t" + Lans.g("Claims", "Fee") + ": " + fee.ToString("F")
                                  + "\t" + Lans.g("Claims", "Adjustments") + ": " + adjAmtSum.ToString("F")
                                  + "\t" + Lans.g("Claims", "Write-off") + ": " + Math.Abs(writeoffSum).ToString("F")
                                  + "\t" + Lans.g("Claims", "Remaining") + ": (" + Math.Abs(feeRemaining).ToString("F") + ")");
        }

        return listProcDescripts;
    }

    /// <summary>
    ///     Returns a list of strings detailing how initial write-offs and ins payments from primary insurance over-credit the
    ///     procedures, and what their remaining balances would be.
    ///     Considers initial writ-offs and ins payments from primary insurance and adjustments.
    ///     Returns an empty list if no write-offs and ins payments over-credit the procedures.
    /// </summary>
    public static List<string> GetInitialPrimaryInsGreaterThanProcFees(List<ClaimProc> listClaimProcsHypothetical)
    {
        var listProcDescripts = new List<string>();
        var listProceduresForClaimProcsDB = Procedures.GetManyProc(listClaimProcsHypothetical.Select(x => x.ProcNum).ToList(), false);
        var listAdjustmentsForClaimProcsDB = Adjustments.GetForProcs(listProceduresForClaimProcsDB.Select(x => x.ProcNum).ToList());
        var listClaimsForClaimProcsDB = GetClaimsFromClaimNums(listClaimProcsHypothetical.Select(x => x.ClaimNum).ToList())
            .FindAll(x => x.ClaimType == "P");
        for (var i = 0; i < listClaimProcsHypothetical.Count; i++)
        {
            var claimProc = listClaimProcsHypothetical[i];
            var procedure = listProceduresForClaimProcsDB.Find(x => x.ProcNum == claimProc.ProcNum);
            var claim = listClaimsForClaimProcsDB.Find(x => x.ClaimNum == claimProc.ClaimNum);
            //Only consider claimprocs on primary insurance claims that are not supplemental payments and associated to a procedure.
            if (claim == null || procedure == null || claimProc.Status != ClaimProcStatus.Received) continue;
            var fee = (decimal) procedure.ProcFeeTotal;
            //Get the sum of all adjustments made on this procedure.
            var adjAmtSum = listAdjustmentsForClaimProcsDB.FindAll(x => x.ProcNum == claimProc.ProcNum).Select(x => (decimal) x.AdjAmt).Sum();
            var initialPrimaryInsCredits = (decimal) (claimProc.WriteOff + claimProc.InsPayAmt);
            //Any changes to this calculation should also consider Claims.GetAllCreditsGreaterThanProcFees() and Claims.GetWriteOffsGreaterThanProcFees().
            //Calculate the remaining fee on the procedure after considering writeoffs and pay amounts from primary insurance's initial payment and adjustments.
            var feeRemaining = fee - initialPrimaryInsCredits + adjAmtSum;
            if (!CompareDecimal.IsLessThanZero(feeRemaining) || !CompareDecimal.IsGreaterThanZero(initialPrimaryInsCredits)) continue;
            listProcDescripts.Add(ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode
                                  + "\t" + Lans.g("Claims", "Fee") + ": " + fee.ToString("F")
                                  + "\t" + Lans.g("Claims", "Adjustments") + ": " + adjAmtSum.ToString("F")
                                  + "\t" + Lans.g("Claims", "Initial Primary Ins Credits") + ": " + Math.Abs(initialPrimaryInsCredits).ToString("F")
                                  + "\t" + Lans.g("Claims", "Remaining") + ": (" + Math.Abs(feeRemaining).ToString("F") + ")");
        }

        return listProcDescripts;
    }

    /// <summary>
    ///     Receives a claim and its claimprocs if the InsAutoReceiveNoAssign pref is on.
    ///     Recalculates related secondary estimates if ClaimPrimaryReceivedRecalcSecondary pref is on.
    ///     Fields that have financial impact other than write-off are set to $0. Ignores preauths.
    ///     Returns true if the claim and claimprocs are received in this method.
    /// </summary>
    public static bool ReceiveAsNoPaymentIfNeeded(long claimNum)
    {
        var claim = GetClaim(claimNum);
        if (claim == null || claim.ClaimType == "PreAuth" || !ClinicPrefs.GetBool(PrefName.InsAutoReceiveNoAssign, claim.ClinicNum)) return false;
        var insSub = InsSubs.GetOne(claim.InsSubNum);
        var doAssignBenefits = GetAssignmentOfBenefits(claim, insSub);
        if (doAssignBenefits) return false;
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            //Canadian. en-CA or fr-CA
            var etransAck = Etranss.GetAllForOneClaim(claimNum).OrderByDescending(x => x.DateTimeTrans)
                .FirstOrDefault(x => x.Etype == EtransType.ClaimAck_CA);
            if (etransAck?.AckCode == "R") //Canadian claim was rejected.
                return false;
        }

        //Using RefreshForClaims() to get ClaimProcs for a single Claim because RefreshForClaim() excludes Canada labs.
        var listClaimProcs = ClaimProcs.RefreshForClaims(new List<long> {claim.ClaimNum});
        if (listClaimProcs.Any(x => x.Status == ClaimProcStatus.Received || x.InsPayAmt != 0)) return false;
        for (var i = 0; i < listClaimProcs.Count; i++)
        {
            var claimProcOld = listClaimProcs[i].Copy();
            var claimProcNew = listClaimProcs[i];
            //These fields don't get set for CapClaim procedures when a capitation claim is received.
            //Capitation claims only allow As Total payments. See FormClaimEdit.butPayTotal_Click()
            //These fields do get set when non-capitation claims are paid. See FormClaimEdit.butPayProc_Click()
            if (claimProcNew.Status != ClaimProcStatus.CapClaim && !claimProcNew.IsOverpay)
            {
                claimProcNew.Status = ClaimProcStatus.Received;
                claimProcNew.DateEntry = DateTime.Now;
                claimProcNew.DateCP = DateTime.Today;
            }

            claimProcNew.InsPayEst = 0; //This is okay because office should use preauths to determine estimates.
            claimProcNew.InsPayAmt = 0;
            claimProcNew.DedApplied = 0;
            claimProcNew.PayPlanNum = 0;
            //claimProcNew.WriteOff=0; Only matters for PPO. Don't clear in case manually entered.
            ClaimProcs.CreateAuditTrailEntryForClaimProcPayment(claimProcNew, claimProcOld, true);
            ClaimProcs.Update(claimProcNew, claimProcOld);
        }

        var logText = Lans.g("Claims", "A claim was automatically received by the 'Auto receive claims with no assignment of benefits' preference.");
        SecurityLogs.MakeLogEntry(EnumPermType.ClaimSentEdit, claim.PatNum, logText, claim.ClaimNum, LogSources.ClaimReceiveAutomatic, claim.SecDateTEdit, 0);
        claim.InsPayEst = 0; //This is okay because office should use preauths to determine estimates.
        claim.InsPayAmt = 0;
        claim.DedApplied = 0;
        //claim.WriteOff=0; Don't change this because we didn't change the ClaimProc write-offs above.
        claim.DateReceived = DateTime.Today;
        claim.ClaimStatus = "R";
        Update(claim);
        if (PrefC.GetBool(PrefName.ClaimPrimaryReceivedRecalcSecondary) && claim.ClaimType == "P") CalculateAndUpdateSecondariesFromPrimaries(new List<Claim> {claim});
        return true;
    }

    /// <summary>
    ///     Returns the salted hash for the claim. Will return an empty string if the calling program is unable to use
    ///     CDT.dll.
    /// </summary>
    public static string HashFields(Claim claim)
    {
        var unhashedText = claim.ClaimFee.ToString("F2") + claim.ClaimStatus + claim.InsPayEst.ToString("F2") + claim.InsPayAmt.ToString("F2");
        try
        {
            return Class1.CreateSaltedHash(unhashedText);
        }
        catch (Exception ex)
        {
            return ex.GetType().Name;
        }
    }

    /// <summary>
    ///     Validates the hash string in claim.SecurityHash. Returns true if it matches the expected hash, otherwise
    ///     false.
    /// </summary>
    public static bool IsClaimHashValid(Claim claim)
    {
        if (claim == null) return true;
        var dateHashStart = SecurityHash.GetHashingDate();
        if (claim.DateService < dateHashStart) //Too old, isn't hashed.
            return true;
        if (claim.SecurityHash == HashFields(claim)) return true;
        return false;
    }
} //end class Claims

/// <summary>
///     This is an odd class.  It holds data for the X12 (4010 only) generation process.  It replaces an older
///     multi-dimensional array, so the names are funny, but helpful to prevent bugs.  Not an actual database table.
/// </summary>
[Serializable]
public class X12TransactionItem
{
    public long ClaimNum4;
    public long PatNum3;
    public string PayorId0;
    public long ProvBill1;
    public long Subscriber2;
}

///<summary>Holds a list of claims to show in the claims 'queue' waiting to be sent.  Not an actual database table.</summary>
[Serializable]
public class ClaimSendQueueItem
{
    
    public string Carrier;

    
    public long ClaimNum;

    /// <summary>Single char: U,H,W,P,S,or R.</summary>
    /// <remarks>
    ///     U=Unsent, H=Hold until pri received, W=Waiting in queue, P=Probably sent, S=Sent, R=Received.  A(adj) is no
    ///     longer used.
    /// </remarks>
    public string ClaimStatus;

    ///<summary>ClearinghouseNum of HQ.</summary>
    public long ClearinghouseNum;

    
    public long ClinicNum;

    /// <summary>Used to save what tracking is used for filtering.</summary>
    public long CustomTracking;

    
    public DateTime DateService;

    ///<summary>Errors which will prevent FormClaimEdit.cs from saving the claim when the user clicks OK.</summary>
    public string ErrorsPreventingSave;

    ///<summary>Claim has procedures with IcdVersion=9 and at least one Diagnostic.</summary>
    public bool HasIcd9;

    /// <summary>
    ///     False by default.  For speed purposes, claims should only be validated once, which is just before they are
    ///     sent.
    /// </summary>
    public bool IsValid;

    ///<summary>Enum:EnumClaimMedType 0=Dental, 1=Medical, 2=Institutional</summary>
    public EnumClaimMedType MedType;

    
    public string MissingData;

    /// <summary>
    ///     Enum:NoSendElectType 0 - send electronically, 1 - don't send electronically, 2 - don't send non-primary (secondary,
    ///     tertiary, etc.) claims electronically.
    /// </summary>
    public NoSendElectType NoSendElect;

    ///<summary>The ordinal of the insurance plan for the subscriber associated with this claim.</summary>
    public int Ordinal;

    
    public string PatName;

    
    public long PatNum;

    ///<summary>Comma separated ProcedureCode string for this claim.</summary>
    public string ProcedureCodeString;

    ///<summary>The Provider of a given clinic.</summary>
    public long ProvTreat;

    ///<summary>Date the claim was last edited.</summary>
    public DateTime SecDateTEdit;

    
    public string Warnings;

    [XmlIgnore]
    public bool CanSendElect => NoSendElect == NoSendElectType.SendElect || (NoSendElect == NoSendElectType.NoSendSecondaryElect && Ordinal == 1);

    public ClaimSendQueueItem Copy()
    {
        return (ClaimSendQueueItem) MemberwiseClone();
    }
}

///<summary>Holds a list of claims to show in the Claim Pay Edit window.  Not an actual database table.</summary>
[Serializable]
public class ClaimPaySplit
{
    
    public string Carrier;

    
    public string ClaimIdentifier;

    
    public long ClaimNum;

    
    public long ClaimPaymentNum;

    
    public string ClaimStatus;

    
    public string ClinicDesc;

    
    public DateTime DateClaim;

    
    public double FeeBilled;

    
    public double InsPayAmt;

    
    public string PatName;

    
    public long PatNum;

    ///<summary>1-based</summary>
    public int PaymentRow;

    
    public string ProvAbbr;
}

///<summary>Different types of filters for the Claims Not Sent report.</summary>
public enum ClaimNotSentStatuses
{
    ///<summary>0</summary>
    All,

    ///<summary>1</summary>
    Primary,

    ///<summary>2</summary>
    Secondary,

    ///<summary>3</summary>
    Holding
}