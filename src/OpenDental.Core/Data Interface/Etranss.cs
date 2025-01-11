using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Etranss
{
    ///<summary>Gets data for the history grid in the SendClaims window.  The listEtransType must contain as least one item.</summary>
    public static DataTable RefreshHistory(DateTime dateFrom, DateTime dateTo, List<EtransType> listEtransTypes)
    {
        var command = "SELECT (CASE WHEN etrans.PatNum=0 THEN etrans.PatientNameRaw "
                      + "ELSE CONCAT(CONCAT(patient.LName,', '),patient.FName) END) AS PatName,"
                      + "(CASE WHEN etrans.carrierNum=0 THEN etrans.CarrierNameRaw ELSE carrier.CarrierName END) AS CarrierName,"
                      + "clearinghouse.Description AS Clearinghouse,DateTimeTrans,etrans.OfficeSequenceNumber,"
                      + "etrans.CarrierTransCounter,Etype,etrans.ClaimNum,etrans.EtransNum,etrans.AckCode,etrans.Note,etrans.EtransMessageTextNum,etrans.TranSetId835,"
                      + "etrans.UserNum,etrans.PatNum "
                      + "FROM etrans "
                      + "LEFT JOIN carrier ON etrans.CarrierNum=carrier.CarrierNum "
                      + "LEFT JOIN patient ON patient.PatNum=etrans.PatNum "
                      + "LEFT JOIN clearinghouse ON clearinghouse.ClearinghouseNum=etrans.ClearinghouseNum WHERE "
                      + DbHelper.DtimeToDate("DateTimeTrans") + " >= " + SOut.Date(dateFrom) + " AND "
                      + DbHelper.DtimeToDate("DateTimeTrans") + " <= " + SOut.Date(dateTo) + " "
                      + "AND Etype IN (" + SOut.Long((int) listEtransTypes[0]);
        for (var i = 1; i < listEtransTypes.Count; i++) //String.Join doesn't work because there's no way to cast the enums to ints in the function, db uses longs.
            command += ", " + SOut.Long((int) listEtransTypes[i]);
        command += ") "
                   //For Canada, when the undo button is used from Manage | Send Claims, the ClaimNum is set to 0 instead of deleting the etrans entry.
                   //For transaction types related to claims where the claimnum=0, we do not want them to show in the history section of Manage | Send Claims because they have been undone.
                   + "AND (ClaimNum<>0 OR Etype NOT IN (" + SOut.Long((int) EtransType.Claim_CA) + "," + SOut.Long((int) EtransType.ClaimCOB_CA) + "," + SOut.Long((int) EtransType.Predeterm_CA) + "," + SOut.Long((int) EtransType.ClaimReversal_CA) + ")) "
                   + "ORDER BY DateTimeTrans";
        var table = DataCore.GetTable(command);
        var tableHist = new DataTable("Table");
        tableHist.Columns.Add("patName");
        tableHist.Columns.Add("CarrierName");
        tableHist.Columns.Add("Clearinghouse");
        tableHist.Columns.Add("dateTimeTrans");
        tableHist.Columns.Add("OfficeSequenceNumber");
        tableHist.Columns.Add("CarrierTransCounter");
        tableHist.Columns.Add("etype");
        tableHist.Columns.Add("Etype");
        tableHist.Columns.Add("ClaimNum");
        tableHist.Columns.Add("EtransNum");
        tableHist.Columns.Add("AckCode");
        tableHist.Columns.Add("ack");
        tableHist.Columns.Add("Note");
        tableHist.Columns.Add("EtransMessageTextNum");
        tableHist.Columns.Add("TranSetId835");
        tableHist.Columns.Add("UserNum");
        tableHist.Columns.Add("PatNum");
        DataRow dataRow;
        string etype;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            dataRow = tableHist.NewRow();
            dataRow["patName"] = table.Rows[i]["PatName"].ToString();
            dataRow["CarrierName"] = table.Rows[i]["CarrierName"].ToString();
            dataRow["Clearinghouse"] = table.Rows[i]["Clearinghouse"].ToString();
            dataRow["dateTimeTrans"] = SIn.DateTime(table.Rows[i]["DateTimeTrans"].ToString()).ToShortDateString();
            dataRow["OfficeSequenceNumber"] = table.Rows[i]["OfficeSequenceNumber"].ToString();
            dataRow["CarrierTransCounter"] = table.Rows[i]["CarrierTransCounter"].ToString();
            dataRow["Etype"] = table.Rows[i]["Etype"].ToString();
            etype = Lans.g("enumEtransType", ((EtransType) SIn.Long(table.Rows[i]["Etype"].ToString())).ToString());
            if (etype.EndsWith("_CA")) etype = etype.Substring(0, etype.Length - 3);
            dataRow["etype"] = etype;
            dataRow["ClaimNum"] = table.Rows[i]["ClaimNum"].ToString();
            dataRow["EtransNum"] = table.Rows[i]["EtransNum"].ToString();
            dataRow["AckCode"] = table.Rows[i]["AckCode"].ToString();
            if (table.Rows[i]["AckCode"].ToString() == "A")
                dataRow["ack"] = Lans.g("Etrans", "Accepted");
            else if (table.Rows[i]["AckCode"].ToString() == "R")
                dataRow["ack"] = Lans.g("Etrans", "Rejected");
            else if (table.Rows[i]["AckCode"].ToString() == "Recd")
                dataRow["ack"] = Lans.g("Etrans", "Received");
            else
                dataRow["ack"] = "";
            dataRow["Note"] = table.Rows[i]["Note"].ToString();
            dataRow["EtransMessageTextNum"] = table.Rows[i]["EtransMessageTextNum"].ToString();
            dataRow["TranSetId835"] = table.Rows[i]["TranSetId835"].ToString();
            dataRow["UserNum"] = table.Rows[i]["UserNum"].ToString();
            dataRow["PatNum"] = table.Rows[i]["PatNum"].ToString();
            tableHist.Rows.Add(dataRow);
        }

        return tableHist;
    }

    
    public static List<Etrans> GetHistoryOneClaim(long claimNum)
    {
        var command = "SELECT * FROM etrans WHERE ClaimNum=" + SOut.Long(claimNum) + " "
                      + "AND (Etype=" + SOut.Int((int) EtransType.Claim_CA) + " "
                      + "OR Etype=" + SOut.Int((int) EtransType.ClaimCOB_CA) + " "
                      + "OR Etype=" + SOut.Int((int) EtransType.Predeterm_CA) + " "
                      + "OR Etype=" + SOut.Int((int) EtransType.ClaimReversal_CA) + " "
                      + "OR Etype=" + SOut.Int((int) EtransType.ClaimSent) + " "
                      + "OR Etype=" + SOut.Int((int) EtransType.Claim_Ramq) + " "
                      + "OR Etype=" + SOut.Int((int) EtransType.ClaimPrinted) + ") "
                      + "ORDER BY DateTimeTrans DESC"; //Because when we want the most recent in the list, we use List[0].
        return EtransCrud.SelectMany(command);
    }

    ///<summary>Gets all types of transactions for the given claim number.</summary>
    public static List<Etrans> GetAllForOneClaim(long claimNum)
    {
        var command = "SELECT * FROM etrans WHERE ClaimNum=" + SOut.Long(claimNum);
        return EtransCrud.SelectMany(command);
    }

    
    public static Etrans GetEtrans(long etransNum)
    {
        var command = "SELECT * FROM etrans WHERE EtransNum=" + SOut.Long(etransNum);
        return EtransCrud.SelectOne(command);
    }

    
    public static List<Etrans> GetMany(params long[] listEtransNums)
    {
        if (listEtransNums.Length == 0) return new List<Etrans>();
        var command = "SELECT * FROM etrans WHERE EtransNum IN (" + string.Join(",", listEtransNums.Select(x => SOut.Long(x))) + ")";
        return EtransCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all Etrans for a Patient for the API. Also has filters for CarrierNum and ClaimNum.
    ///     Results can be truncated with the use of limit and offset. Please notify the API team if you change this code.
    /// </summary>
    public static List<Etrans> GetEtransForApi(int limit, int offset, long patNum, long carrierNum = 0, long claimNum = 0)
    {
        var command = "SELECT * FROM etrans WHERE PatNum=" + SOut.Long(patNum);
        if (carrierNum != 0) command += " AND CarrierNum=" + SOut.Long(carrierNum);
        if (claimNum != 0) command += " AND ClaimNum=" + SOut.Long(claimNum);
        command += " ORDER BY DateTimeTrans "
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return EtransCrud.SelectMany(command);
    }

    ///<summary>Gets all X12 835 etrans entries relating to a specific claim.</summary>
    public static List<Etrans> GetErasOneClaim(string claimIdentifier, DateTime dateTClaimService)
    {
        //The main goal of this check is to prevent null claimIdentifiers from causing an exception.
        //However, an empty claim identifier should also return an empty list because that is a terrible identifier IMO.
        if (string.IsNullOrEmpty(claimIdentifier)) return new List<Etrans>();

        var claimId = claimIdentifier;
        if (claimId.Length > 16)
            //Our claim identifiers in the database can be longer than 20 characters (mostly when using replication).
            //When the claim identifier is sent out on the claim, it is truncated to 20 characters.
            //Therefore, if the claim identifier is longer than 20 characters,
            //then it was truncated when sent out, so we have to look for claims beginning with the claim identifier given if there is not an exact match.
            //We also send shorter identifiers for some clearinghouses.  For example, the maximum claim identifier length for Denti-Cal is 17 characters.
            claimId = claimId.Substring(0, 16);
        var command = "SELECT * FROM etrans"
                      + " INNER JOIN etransmessagetext ON etransmessagetext.EtransMessageTextNum=etrans.EtransMessageTextNum"
                      + " AND etransmessagetext.MessageText REGEXP 'CLP." + SOut.String(claimId) + "'"
                      //CLP = match CLP, . = match any character, then up to 16 other chars after.
                      + " WHERE Etype=" + SOut.Int((int) EtransType.ERA_835) + " AND etrans.DateTimeTrans >= " + SOut.Date(dateTClaimService);
        if (claimIdentifier.Length < 16)
        {
            var tableEtrans = DataCore.GetTable(command);
            var listEtranss = EtransCrud.TableToList(tableEtrans);
            var listEtranssRetVal = new List<Etrans>();
            for (var i = 0; i < tableEtrans.Rows.Count; i++)
            {
                var messageText = SIn.String(tableEtrans.Rows[i]["MessageText"].ToString());
                var separator = messageText.Substring(3, 1); //The character that is used as a separator is always at the third index of the message text.
                if (messageText.Contains("CLP" + separator + claimId + separator)) listEtranssRetVal.Add(listEtranss[i]); //This Etrans has an exact match for the claimIdentifier, it's an accurate search result.
            }

            return listEtranssRetVal;
        }

        //If the claimIdentifier is > 16 we trust it's unique enough we don't need to do more searching.
        //Plus, we cannot trust any characters after the 16th character, since the identifier might have been truncated at some point.
        return EtransCrud.SelectMany(command);
    }

    ///<summary>Gets a list of all 270's and Canadian eligibilities for this plan.</summary>
    public static List<Etrans> GetList270ForPlan(long planNum, long insSubNum)
    {
        var command = "SELECT * FROM etrans WHERE PlanNum=" + SOut.Long(planNum)
                                                            + " AND InsSubNum=" + SOut.Long(insSubNum)
                                                            + " AND (Etype=" + SOut.Long((int) EtransType.BenefitInquiry270)
                                                            + " OR Etype=" + SOut.Long((int) EtransType.Eligibility_CA) + ")";
        return EtransCrud.SelectMany(command);
    }

    /// <summary>
    ///     Use for Canadian claims only. Finds the most recent etrans record which matches the unique
    ///     officeSequenceNumber specified. The officeSequenceNumber corresponds to field A02.
    /// </summary>
    public static Etrans GetForSequenceNumberCanada(string officeSequenceNumber)
    {
        var command = "SELECT * FROM etrans WHERE OfficeSequenceNumber=" + SOut.String(officeSequenceNumber) + " ORDER BY EtransNum DESC LIMIT 1";
        return EtransCrud.SelectOne(command);
    }

    /*
    
    public static Etrans GetAckForTrans(int etransNum) {

        //first, get the actual trans.
        string command="SELECT * FROM etrans WHERE EtransNum="+POut.PInt(etransNum);
        DataTable table=DataCore.GetTable(command);
        Etrans etrans=SubmitAndFill(table);
        command="SELECT * FROM etrans WHERE "
            +"Etype=21 "//ack997
            +"AND ClearingHouseNum="+POut.PInt(etrans.ClearingHouseNum)
            +" AND BatchNumber= "+POut.PInt(etrans.BatchNumber)
            +" AND DateTimeTrans < "+POut.PDateT(etrans.DateTimeTrans.AddDays(14))//less than 2wks in the future
            +" AND DateTimeTrans > "+POut.PDateT(etrans.DateTimeTrans.AddDays(-1));//and no more than one day before claim
        table=DataCore.GetTable(command);
        return SubmitAndFill(table);
    }*/

    /*
    private static List<Etrans> SubmitAndFill(DataTable table){
        Meth.NoCheckMiddleTierRole();
        //if(table.Rows.Count==0){
        //	return null;
        //}
        List<Etrans> retVal=new List<Etrans>();
        Etrans etrans;
        for(int i=0;i<table.Rows.Count;i++) {
            etrans=new Etrans();
            etrans.EtransNum           =PIn.Long(table.Rows[i][0].ToString());
            etrans.DateTimeTrans       =PIn.DateT(table.Rows[i][1].ToString());
            etrans.ClearingHouseNum    =PIn.Long(table.Rows[i][2].ToString());
            etrans.Etype               =(EtransType)PIn.Long(table.Rows[i][3].ToString());
            etrans.ClaimNum            =PIn.Long(table.Rows[i][4].ToString());
            etrans.OfficeSequenceNumber=PIn.Int(table.Rows[i][5].ToString());
            etrans.CarrierTransCounter =PIn.Int(table.Rows[i][6].ToString());
            etrans.CarrierTransCounter2=PIn.Int(table.Rows[i][7].ToString());
            etrans.CarrierNum          =PIn.Long(table.Rows[i][8].ToString());
            etrans.CarrierNum2         =PIn.Long(table.Rows[i][9].ToString());
            etrans.PatNum              =PIn.Long(table.Rows[i][10].ToString());
            etrans.BatchNumber         =PIn.Int(table.Rows[i][11].ToString());
            etrans.AckCode             =PIn.String(table.Rows[i][12].ToString());
            etrans.TransSetNum         =PIn.Int(table.Rows[i][13].ToString());
            etrans.Note                =PIn.String(table.Rows[i][14].ToString());
            etrans.EtransMessageTextNum=PIn.Long(table.Rows[i][15].ToString());
            etrans.AckEtransNum        =PIn.Long(table.Rows[i][16].ToString());
            etrans.PlanNum             =PIn.Long(table.Rows[i][17].ToString());
            retVal.Add(etrans);
        }
        return retVal;
    }*/

    ///<summary>DateTimeTrans handled automatically here.</summary>
    public static long Insert(Etrans etrans)
    {
        return EtransCrud.Insert(etrans);
    }

    
    public static void Update(Etrans etrans)
    {
        EtransCrud.Update(etrans);
    }

    ///<summary>Only updates fields in etrans that are different from etransOld.</summary>
    public static void Update(Etrans etrans, Etrans etransOld)
    {
        EtransCrud.Update(etrans, etransOld);
    }

    /// <summary>
    ///     Not for claim types, just other types, including Eligibility. This function gets run first.  Then, the
    ///     messagetext is created and an attempt is made to send the message.  Finally, the messagetext is added to the
    ///     etrans.  This is necessary because the transaction numbers must be incremented and assigned to each message before
    ///     creating the message and attempting to send.  If it fails, we will need to roll back.  Provide EITHER a carrierNum
    ///     OR a canadianNetworkNum.  Many transactions can be sent to a carrier or to a network.
    /// </summary>
    public static Etrans CreateCanadianOutput(long patNum, long carrierNum, long canadianNetworkNum
        , long clearinghouseNum, EtransType etype, long planNum, long insSubNum, long userNum, bool hasSecondary = false)
    {
        //validation of carrier vs network
        if (etype == EtransType.Eligibility_CA)
        {
            //only carrierNum is allowed (and required)
            if (carrierNum == 0) throw new ApplicationException("Carrier not supplied for Etranss.CreateCanadianOutput.");
            if (canadianNetworkNum != 0) throw new ApplicationException("NetworkNum not allowed for Etranss.CreateCanadianOutput.");
        }

        var etrans = new Etrans();
        //etrans.DateTimeTrans handled automatically
        etrans.ClearingHouseNum = clearinghouseNum;
        etrans.Etype = etype;
        etrans.ClaimNum = 0; //no claim involved
        etrans.PatNum = patNum;
        //CanadianNetworkNum?
        etrans.CarrierNum = carrierNum;
        etrans.PlanNum = planNum;
        etrans.InsSubNum = insSubNum;
        etrans.UserNum = userNum;
        etrans.BatchNumber = 0;
        //Get next OfficeSequenceNumber-----------------------------------------------------------------------------------------
        etrans = SetCanadianEtransFields(etrans, hasSecondary);
        Insert(etrans);
        return GetEtrans(etrans.EtransNum); //Since the DateTimeTrans is set upon insert, we need to read the record again in order to get the date.
    }

    /// <summary>
    ///     Throws exceptions.
    ///     When etrans.Etype is associated to a Canadian request EType, this runs multiple queries to set
    ///     etrans.CarrierTransCounter and
    ///     etrans.CarrierTransCounter2.  Otherwise returns without making any changes.
    ///     The etrans.CarrierNum, etrans.CarrierNum2 and etrans.Etype columns must be set prior to running this.
    /// </summary>
    public static Etrans SetCanadianEtransFields(Etrans etrans, bool hasSecondary = true)
    {
        if (!EnumTools.GetAttributeOrDefault<EtransTypeAttr>(etrans.Etype).IsCanadaType || !EnumTools.GetAttributeOrDefault<EtransTypeAttr>(etrans.Etype).IsRequestType) return etrans;

        etrans.OfficeSequenceNumber = 0;
        //find the next officeSequenceNumber
        var command = "SELECT MAX(OfficeSequenceNumber) FROM etrans";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            etrans.OfficeSequenceNumber = SIn.Int(table.Rows[0][0].ToString());
            if (etrans.OfficeSequenceNumber == 999999) //if the office has sent > 1 million messages, and has looped back around to 1.
                throw new ApplicationException("OfficeSequenceNumber has maxed out at 999999.  This program will need to be enhanced.");
        }

        etrans.OfficeSequenceNumber++;
        //find the next CarrierTransCounter for the primary carrier

        #region CarrierTransCounter

        etrans.CarrierTransCounter = 0;
        command = "SELECT MAX(CarrierTransCounter) FROM etrans "
                  + "WHERE CarrierNum=" + SOut.Long(etrans.CarrierNum);
        table = DataCore.GetTable(command);
        var tempCounter = 0;
        if (table.Rows.Count > 0) tempCounter = SIn.Int(table.Rows[0][0].ToString());
        if (tempCounter > etrans.CarrierTransCounter) etrans.CarrierTransCounter = tempCounter;
        command = "SELECT MAX(CarrierTransCounter2) FROM etrans "
                  + "WHERE CarrierNum2=" + SOut.Long(etrans.CarrierNum);
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0) tempCounter = SIn.Int(table.Rows[0][0].ToString());
        if (tempCounter > etrans.CarrierTransCounter) etrans.CarrierTransCounter = tempCounter;
        if (etrans.CarrierTransCounter == 99999) throw new ApplicationException("CarrierTransCounter has maxed out at 99999.  This program will need to be enhanced.");
        //maybe by adding a reset date to the preference table which will apply to all counters as a whole.
        etrans.CarrierTransCounter++;

        #endregion CarrierTransCounter

        if (!hasSecondary || etrans.CarrierNum2 == 0) return etrans;

        #region CarrierTransCounter2

        etrans.CarrierTransCounter2 = 1;
        command = "SELECT MAX(CarrierTransCounter) FROM etrans "
                  + "WHERE CarrierNum=" + SOut.Long(etrans.CarrierNum2);
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0) tempCounter = SIn.Int(table.Rows[0][0].ToString());
        if (tempCounter > etrans.CarrierTransCounter2) etrans.CarrierTransCounter2 = tempCounter;
        command = "SELECT MAX(CarrierTransCounter2) FROM etrans "
                  + "WHERE CarrierNum2=" + SOut.Long(etrans.CarrierNum2);
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0) tempCounter = SIn.Int(table.Rows[0][0].ToString());
        if (tempCounter > etrans.CarrierTransCounter2) etrans.CarrierTransCounter2 = tempCounter;
        if (etrans.CarrierTransCounter2 == 99999) throw new ApplicationException("CarrierTransCounter has maxed out at 99999.  This program will need to be enhanced.");
        etrans.CarrierTransCounter2++;

        #endregion CarrierTransCounter2

        return etrans;
    }

    /// <summary>
    ///     Inserts EtransMessageText row with given messageText then updates the etrans.EtransMessageTextNum in the DB based
    ///     on given etransNum.
    ///     CAUTION: This does not update the EtransMessageTextNum field of an object in memory.
    ///     Instead it returns the inserted EtransMessageTextNum, this should be used to update the in memory object if needed.
    /// </summary>
    public static long SetMessage(long etransNum, string messageText)
    {
        var msg = new EtransMessageText();
        msg.MessageText = messageText;
        EtransMessageTexts.Insert(msg);
        //string command=
        var command = "UPDATE etrans SET EtransMessageTextNum=" + SOut.Long(msg.EtransMessageTextNum) + " "
                      + "WHERE EtransNum = '" + SOut.Long(etransNum) + "'";
        Db.NonQ(command);
        return msg.EtransMessageTextNum;
    }

    /// <summary>
    ///     Changes the status of the claim back to W.  If it encounters an entry that's not a claim, it skips it for now.
    ///     Later, it will handle all types of undo.  It will also check Canadian claims to prevent alteration if an ack or EOB
    ///     has been received.
    /// </summary>
    public static void Undo(long etransNum)
    {
        //see if it's a claim.
        var command = "SELECT ClaimNum FROM etrans WHERE EtransNum=" + SOut.Long(etransNum);
        var table = DataCore.GetTable(command);
        var claimNum = SIn.Long(table.Rows[0][0].ToString());
        if (claimNum == 0) //if no claim
            return; //for now
        //future Canadian check will go here

        //Change the claim back to W.
        var claimOld = Claims.GetClaim(claimNum);
        command = "UPDATE claim SET ClaimStatus='W' WHERE ClaimNum=" + SOut.Long(claimNum);
        Db.NonQ(command);
        if (claimOld != null && Claims.IsClaimHashValid(claimOld))
        {
            //Should never be null. Only rehash claims that are already valid.
            var claim = Claims.GetClaim(claimNum);
            claim.SecurityHash = Claims.HashFields(claim);
            if (claimOld.SecurityHash != claim.SecurityHash) //Only bother updating if the SecurityHash is different.
                ClaimCrud.Update(claim);
        }
    }

    /// <summary>
    ///     Deletes the etrans entry.  Mostly used when the etrans entry was created, but then the communication with the
    ///     clearinghouse failed.
    ///     So this is just a rollback function.  Will not delete the message associated with the etrans.  That must be done
    ///     separately.
    /// </summary>
    public static void Delete(long etransNum)
    {
        var command = "DELETE FROM etrans WHERE EtransNum=" + SOut.Long(etransNum);
        Db.NonQ(command);
    }

    public static void Delete835(Etrans etrans)
    {
        EtransMessageTexts.Delete(etrans.EtransMessageTextNum, etrans.EtransNum);
        Delete(etrans.EtransNum);
        Etrans835Attaches.DeleteMany(-1, etrans.EtransNum);
    }

    /// <summary>
    ///     Sets the status of the claim to sent (if not already received), usually as part of printing.  Also makes an
    ///     entry in etrans.  If this is Canadian eclaims, then this function gets run first.  If the claim is to be sent
    ///     elecronically, then the messagetext is created after this method and an attempt is made to send the claim.
    ///     Finally, the messagetext is added to the etrans.  This is necessary because the transaction numbers must be
    ///     incremented and assigned to each claim before creating the message and attempting to send.  For Canadians, it will
    ///     always record the attempt as an etrans even if claim is not set to status of sent.
    /// </summary>
    public static Etrans SetClaimSentOrPrinted(long claimNum, string claimStatus, long patNum, long clearinghouseNum, EtransType etype, int batchNumber, long userNum)
    {
        var etrans = CreateEtransForClaim(claimNum, patNum, clearinghouseNum, etype, batchNumber, userNum);
        etrans = SetCanadianEtransFields(etrans); //etrans.CarrierNum, etrans.CarrierNum2 and etrans.EType all set prior to calling this.
        var claimStatusReceived = ClaimStatus.Received.GetDescription(true);
        var claimStatusInProcess = ClaimStatus.HoldForInProcess.GetDescription(true);
        if (claimStatus != claimStatusReceived && claimStatus != claimStatusInProcess) //We don't want to change the claim's status unnecessarily when printing / viewing
            Claims.SetClaimSent(claimNum);
        Insert(etrans);
        return GetEtrans(etrans.EtransNum); //Since the DateTimeTrans is set upon insert, we need to read the record again in order to get the date.
    }

    /// <summary>
    ///     Returns an etrans that has not been inserted into the DB.
    ///     Should only be called with etrans is related an EtransType that is of claim type, currently no validation is done
    ///     in this function to ensure this.
    /// </summary>
    public static Etrans CreateEtransForClaim(long claimNum, long patNum, long clearinghouseNum, EtransType etype, int batchNumber, long userNum)
    {
        var etrans = new Etrans();
        //etrans.DateTimeTrans handled automatically
        etrans.ClearingHouseNum = clearinghouseNum;
        etrans.Etype = etype;
        etrans.ClaimNum = claimNum;
        etrans.PatNum = patNum;
        etrans.UserNum = userNum;
        //Get the primary and secondary carrierNums for this claim.
        var command = "SELECT carrier1.CarrierNum,carrier2.CarrierNum AS CarrierNum2 FROM claim "
                      + "LEFT JOIN insplan insplan1 ON insplan1.PlanNum=claim.PlanNum "
                      + "LEFT JOIN carrier carrier1 ON carrier1.CarrierNum=insplan1.CarrierNum "
                      + "LEFT JOIN insplan insplan2 ON insplan2.PlanNum=claim.PlanNum2 "
                      + "LEFT JOIN carrier carrier2 ON carrier2.CarrierNum=insplan2.CarrierNum "
                      + "WHERE claim.ClaimNum=" + SOut.Long(claimNum);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            //The claim could have been deleted by someone else.  Don't worry about preserving the carrier information.  Set to 0.
            etrans.CarrierNum = SIn.Long(table.Rows[0][0].ToString());
            etrans.CarrierNum2 = SIn.Long(table.Rows[0][1].ToString()); //might be 0 if no secondary on this claim
        }
        else
        {
            etrans.Note = Lans.g(nameof(Etrans), "Primry carrier and secondary carrier are unknown due to missing claim.  Invalid ClaimNum.  "
                                                 + "Claim may have been deleted during sending.");
        }

        etrans.BatchNumber = batchNumber;
        return etrans;
    }

    /// <summary>
    ///     Etrans type will be figured out by this class.  Either TextReport, Acknowledge_997, Acknowledge_999, or
    ///     StatusNotify_277.
    /// </summary>
    public static void ProcessIncomingReport(DateTime dateTimeTrans, long hqClearinghouseNum, string messageText, long userNum)
    {
        var etrans = CreateEtrans(dateTimeTrans, hqClearinghouseNum, messageText, userNum);
        string command;
        var x12object = X12object.ToX12object(messageText);
        if (x12object != null)
        {
            //Is a correctly formatted X12 message.
            if (x12object.IsAckInterchange())
            {
                etrans.Etype = EtransType.Ack_Interchange;
                Insert(etrans);
                //At some point in the future, we should use TA101 to match to batch number and TA104 to get the ack code, 
                //then update historic etrans entries like we do for 997s, 999s and 277s.
            }
            else if (x12object.Is997())
            {
                var x997 = new X997(messageText);
                etrans.Etype = EtransType.Acknowledge_997;
                etrans.BatchNumber = x997.GetBatchNumber();
                Insert(etrans);
                var batchAck = x997.GetBatchAckCode();
                if (batchAck == "A" || batchAck == "R")
                {
                    //accepted or rejected
                    command = "UPDATE etrans SET AckCode='" + batchAck + "', "
                              + "AckEtransNum=" + SOut.Long(etrans.EtransNum)
                              + " WHERE BatchNumber=" + SOut.Long(etrans.BatchNumber)
                              + " AND ClearinghouseNum=" + SOut.Long(hqClearinghouseNum)
                              + " AND DateTimeTrans > " + SOut.DateT(dateTimeTrans.AddDays(-14))
                              + " AND DateTimeTrans < " + SOut.DateT(dateTimeTrans.AddDays(1))
                              + " AND AckEtransNum=0";
                    Db.NonQ(command);
                }
                else
                {
                    //partially accepted
                    var transNums = x997.GetTransNums();
                    string ack;
                    for (var i = 0; i < transNums.Count; i++)
                    {
                        ack = x997.GetAckForTrans(transNums[i]);
                        if (ack == "A" || ack == "R")
                        {
                            //accepted or rejected
                            command = "UPDATE etrans SET AckCode='" + ack + "', "
                                      + "AckEtransNum=" + SOut.Long(etrans.EtransNum)
                                      + " WHERE BatchNumber=" + SOut.Long(etrans.BatchNumber)
                                      + " AND TransSetNum=" + SOut.Long(transNums[i])
                                      + " AND ClearinghouseNum=" + SOut.Long(hqClearinghouseNum)
                                      + " AND DateTimeTrans > " + SOut.DateT(dateTimeTrans.AddDays(-14))
                                      + " AND DateTimeTrans < " + SOut.DateT(dateTimeTrans.AddDays(1))
                                      + " AND AckEtransNum=0";
                            Db.NonQ(command);
                        }
                    }
                }
                //none of the other fields make sense, because this ack could refer to many claims.
            }
            else if (x12object.Is999())
            {
                var x999 = new X999(messageText);
                etrans.Etype = EtransType.Acknowledge_999;
                etrans.BatchNumber = x999.GetBatchNumber();
                Insert(etrans);
                var batchAck = x999.GetBatchAckCode();
                if (batchAck == "A" || batchAck == "R")
                {
                    //accepted or rejected
                    command = "UPDATE etrans SET AckCode='" + batchAck + "', "
                              + "AckEtransNum=" + SOut.Long(etrans.EtransNum)
                              + " WHERE BatchNumber=" + SOut.Long(etrans.BatchNumber)
                              + " AND ClearinghouseNum=" + SOut.Long(hqClearinghouseNum)
                              + " AND DateTimeTrans > " + SOut.DateT(dateTimeTrans.AddDays(-14))
                              + " AND DateTimeTrans < " + SOut.DateT(dateTimeTrans.AddDays(1))
                              + " AND AckEtransNum=0";
                    Db.NonQ(command);
                }
                else
                {
                    //partially accepted
                    var transNums = x999.GetTransNums();
                    string ack;
                    for (var i = 0; i < transNums.Count; i++)
                    {
                        ack = x999.GetAckForTrans(transNums[i]);
                        if (ack != "A" && ack != "R") continue;
                        //accepted or rejected
                        command = "UPDATE etrans SET AckCode='" + ack + "', "
                                  + "AckEtransNum=" + SOut.Long(etrans.EtransNum)
                                  + " WHERE BatchNumber=" + SOut.Long(etrans.BatchNumber)
                                  + " AND TransSetNum=" + SOut.Long(transNums[i])
                                  + " AND ClearinghouseNum=" + SOut.Long(hqClearinghouseNum)
                                  + " AND DateTimeTrans > " + SOut.DateT(dateTimeTrans.AddDays(-14))
                                  + " AND DateTimeTrans < " + SOut.DateT(dateTimeTrans.AddDays(1))
                                  + " AND AckEtransNum=0";
                        Db.NonQ(command);
                    }
                }
                //none of the other fields make sense, because this ack could refer to many claims.
            }
            else if (X277.Is277(x12object))
            {
                var x277 = new X277(messageText);
                etrans.Etype = EtransType.StatusNotify_277;
                Insert(etrans);
                var listClaimIdentifiers = x277.GetClaimTrackingNumbers();
                //Dictionary to run one update command per ack code for many claims.
                var dictionaryClaimMatchesByAck = new Dictionary<string, List<X12ClaimMatch>>();
                for (var i = 0; i < listClaimIdentifiers.Count; i++)
                {
                    var x12ClaimMatch = new X12ClaimMatch();
                    x12ClaimMatch.ClaimIdentifier = listClaimIdentifiers[i];
                    var stringClaimInfoArray = x277.GetClaimInfo(x12ClaimMatch.ClaimIdentifier);
                    x12ClaimMatch.PatFname = SIn.String(stringClaimInfoArray[0]);
                    x12ClaimMatch.PatLname = SIn.String(stringClaimInfoArray[1]);
                    x12ClaimMatch.DateServiceStart = SIn.DateTime(stringClaimInfoArray[6]);
                    x12ClaimMatch.DateServiceEnd = SIn.DateTime(stringClaimInfoArray[7]);
                    x12ClaimMatch.ClaimFee = SIn.Double(stringClaimInfoArray[9]);
                    x12ClaimMatch.SubscriberId = SIn.String(stringClaimInfoArray[10]);
                    x12ClaimMatch.EtransNum = etrans.EtransNum;
                    var ack = stringClaimInfoArray[3];
                    if (!dictionaryClaimMatchesByAck.ContainsKey(ack)) dictionaryClaimMatchesByAck.Add(ack, new List<X12ClaimMatch>());
                    dictionaryClaimMatchesByAck[ack].Add(x12ClaimMatch);
                }

                foreach (var ack in dictionaryClaimMatchesByAck.Keys)
                {
                    var listClaimNums = Claims.GetClaimFromX12(dictionaryClaimMatchesByAck[ack]);
                    if (listClaimNums == null) continue;
                    listClaimNums = listClaimNums.Where(x => x != 0).ToList();
                    if (listClaimNums.Count == 0) continue;
                    //Locate the latest etrans entries for the claims based on DateTimeTrans with EType of ClaimSent or Claim_Ren and update the AckCode and AckEtransNum.
                    //We overwrite existing acks from 997s, 999s and older 277s.
                    command = "UPDATE etrans SET AckCode='" + ack + "', "
                              + "AckEtransNum=" + SOut.Long(etrans.EtransNum)
                              + " WHERE EType IN (" + SOut.Int((int) EtransType.ClaimSent) + "," + SOut.Int((int) EtransType.Claim_Ren) + ") "
                              + " AND ClaimNum IN(" + string.Join(",", listClaimNums.Select(x => SOut.Long(x))) + ")"
                              + " AND ClearinghouseNum=" + SOut.Long(hqClearinghouseNum)
                              + " AND DateTimeTrans > " + SOut.DateT(dateTimeTrans.AddDays(-14))
                              + " AND DateTimeTrans < " + SOut.DateT(dateTimeTrans.AddDays(1));
                    Db.NonQ(command);
                    //none of the other fields make sense, because this ack could refer to many claims.
                }
            }
            else if (X835.Is835(x12object))
            {
                etrans.Etype = EtransType.ERA_835;
                var listTranSetIds = x12object.GetTranSetIds();
                var listEtranss = new List<Etrans>();
                var list835s = new List<X835>();
                //We pull in the 835 data in two loops so that we can ensure the 835 is fully parsed before we create any etrans entries.
                for (var i = 0; i < listTranSetIds.Count; i++)
                {
                    etrans.TranSetId835 = listTranSetIds[i];
                    if (i > 0) etrans.EtransNum = 0; //To get a new record to insert.
                    var x835 = new X835(etrans, messageText, etrans.TranSetId835); //parse. If parsing fails, then no etrans entries will be inserted.
                    etrans.CarrierNameRaw = x835.PayerName;
                    var listUniquePatientNames = new List<string>();
                    for (var j = 0; j < x835.ListClaimsPaid.Count; j++)
                    {
                        var patName = x835.ListClaimsPaid[j].PatientName.ToString(false);
                        if (!listUniquePatientNames.Contains(patName)) listUniquePatientNames.Add(patName);
                    }

                    if (listUniquePatientNames.Count == 1)
                        etrans.PatientNameRaw = listUniquePatientNames[0];
                    else
                        etrans.PatientNameRaw = "(" + listUniquePatientNames.Count + " " + Lans.g("Etranss", "patients") + ")";
                    listEtranss.Add(etrans.Copy());
                    list835s.Add(x835);
                }

                //The 835 was completely parsed.  Create etrans entries.
                var listMatchedClaimNums = new List<long>();
                for (var i = 0; i < listEtranss.Count; i++)
                {
                    etrans = listEtranss[i];
                    var x835 = list835s[i];
                    x835.EtransSource.EtransNum = Insert(etrans); //insert
                    var listClaimNums = x835.ListClaimsPaid.Select(x => x.ClaimNum).Where(x => x != 0).ToList();
                    listMatchedClaimNums.AddRange(listClaimNums);
                    if (listClaimNums.Count == 0) continue;
                    Etrans835Attaches.CreateManyForNewEra(x835);
                    //Locate the latest etrans entries for the claim based on DateTimeTrans with EType of ClaimSent or Claim_Ren and update the AckCode and AckEtransNum.
                    //We overwrite existing acks from 997s, 999s, and 277s.
                    command = "UPDATE etrans SET AckCode='A', "
                              + "AckEtransNum=" + SOut.Long(etrans.EtransNum)
                              + " WHERE EType IN (0,3) " //ClaimSent and Claim_Ren
                              + " AND ClaimNum IN(" + string.Join(",", listClaimNums.Select(x => SOut.Long(x))) + ")"
                              + " AND ClearinghouseNum=" + SOut.Long(hqClearinghouseNum)
                              + " AND DateTimeTrans > " + SOut.DateT(dateTimeTrans.AddDays(-14))
                              + " AND DateTimeTrans < " + SOut.DateT(dateTimeTrans.AddDays(1));
                    Db.NonQ(command);
                    //none of the other fields make sense, because this ack could refer to many claims.
                }

                //Get all of the etrans that we should attempt to process.
                var listHx835_ShortClaimsAll = Hx835_ShortClaim.GetClaimsFromClaimNums(listMatchedClaimNums);
                var listHx835_ShortClaimPlanNumsAll = listHx835_ShortClaimsAll.Select(x => x.PlanNum).ToList();
                var listInsPlansAll = InsPlans.GetPlans(listHx835_ShortClaimPlanNumsAll);
                var listEtranssToProcesses = new List<Etrans>();
                var listClaimNumsMatchedToProcess = new List<long>();
                for (var i = 0; i < listEtranss.Count; i++)
                {
                    var listClaimNumsPaid = list835s[i].ListClaimsPaid.Select(x => x.ClaimNum).Where(x => x != 0).ToList();
                    var listHx835_ShortClaims = listHx835_ShortClaimsAll.FindAll(x => listClaimNumsPaid.Contains(x.ClaimNum));
                    var listPlanNumsForClaims = listHx835_ShortClaims.Select(x => x.PlanNum).Distinct().ToList();
                    var listInsPlansForClaims = listInsPlansAll.FindAll(x => listPlanNumsForClaims.Contains(x.PlanNum));
                    var listCarriers = Carriers.GetForInsPlans(listInsPlansForClaims);
                    if (IsEtransAutomatable(listCarriers, list835s[i].PayerName, true))
                    {
                        listEtranssToProcesses.Add(listEtranss[i]);
                        listClaimNumsMatchedToProcess.AddRange(listClaimNumsPaid);
                    }
                }

                //Try to auto-process ERAs.
                var listEtransNums = listEtranssToProcesses.Select(x => x.EtransNum).ToList();
                var listEtrans835Attaches = Etrans835Attaches.GetForEtransNumOrClaimNums(false, listEtransNums, listClaimNumsMatchedToProcess.ToArray());
                if (listEtranssToProcesses.Count > 0) TryAutoProcessEras(listEtranssToProcesses, listEtrans835Attaches, true);
            }
            else
            {
                //unknown type of X12 report.
                etrans.Etype = EtransType.TextReport;
                Insert(etrans);
            }
        }
        else
        {
            //not X12
            etrans.Etype = EtransType.TextReport;
            Insert(etrans);
        }
    }

    /// <summary>
    ///     Uses the attached claims, carriers with a matching name, or the global EraAutomationBehavior pref to determine if
    ///     an ERA is automatable.
    ///     If isFullyAutomatic is true, EraAutomationMode.FullyAutomatic is the only mode that will return true.
    /// </summary>
    public static bool IsEtransAutomatable(List<Carrier> listCarriersForClaims, string payerName, bool isFullyAutomatic)
    {
        var listEraAutomationModesAllowed = new List<EraAutomationMode> {EraAutomationMode.FullyAutomatic};
        if (!isFullyAutomatic) listEraAutomationModesAllowed.Add(EraAutomationMode.SemiAutomatic);
        if (listCarriersForClaims.Count > 0)
            //The ERA is automatable if it has any claims attached to it that are for a Carrier that allows automation.
            //The ERA is not automatable if it has one or more claims attached and all Carriers for all attached claims don't allow automation.
            return listCarriersForClaims.Any(x => listEraAutomationModesAllowed.Contains(x.GetEraAutomationMode()));
        //If there are no attached claims, we see if the Carrier name from the ERA matches one or more Carriers in the DB.
        var listCarriersNameMatches = Carriers.GetExactNames(payerName);
        if (listCarriersNameMatches.Count == 0)
            //If no claims are attached and no carriers have a matching name, we use the global preference for ERA automation.
            return listEraAutomationModesAllowed.Contains(PrefC.GetEnum<EraAutomationMode>(PrefName.EraAutomationBehavior));
        //If we have no attached claims and one or more names match, we allow automation if any name-matched carriers allow it.
        return listCarriersNameMatches.Any(x => listEraAutomationModesAllowed.Contains(x.GetEraAutomationMode()));
    }


    ///<summary>Creates new etrans object, does not insert to Etrans table though. Does insert EtransMessageText.</summary>
    public static Etrans CreateEtrans(DateTime dateTimeTrans, long hqClearinghouseNum, string messageText, long userNum)
    {
        var etrans = new Etrans();
        etrans.DateTimeTrans = dateTimeTrans;
        etrans.ClearingHouseNum = hqClearinghouseNum;
        var etransMessageText = new EtransMessageText();
        etransMessageText.MessageText = messageText;
        EtransMessageTexts.Insert(etransMessageText);
        etrans.EtransMessageTextNum = etransMessageText.EtransMessageTextNum;
        etrans.UserNum = userNum;
        return etrans;
    }

    /// <summary>Or Canadian elig.</summary>
    public static DateTime GetLastDate270(long planNum)
    {
        var command = "SELECT MAX(DateTimeTrans) FROM etrans "
                      + "WHERE (Etype=" + SOut.Int((int) EtransType.BenefitInquiry270) + " "
                      + "OR Etype=" + SOut.Int((int) EtransType.Eligibility_CA) + ") "
                      + " AND PlanNum=" + SOut.Long(planNum);
        return SIn.Date(DataCore.GetScalar(command));
    }

    ///<summary>Attempts to automatically receive claims and finalize payments for multiple ERAs.</summary>
    public static List<EraAutomationResult> TryAutoProcessEras(List<Etrans> listEtranss, List<Etrans835Attach> listEtrans835Attaches, bool isFullyAutomatic)
    {
        var listEtransMessageTextNums = listEtranss.Select(x => x.EtransMessageTextNum).ToList();
        var dictionaryMessageText835s = EtransMessageTexts.GetMessageTexts(listEtransMessageTextNums);
        var listEtrans835s = Etrans835s.GetByEtransNums(listEtranss.Select(x => x.EtransNum).ToArray());
        var listAllAutomationResults = new List<EraAutomationResult>();
        for (var i = 0; i < listEtranss.Count; i++)
        {
            var messageText835 = dictionaryMessageText835s[listEtranss[i].EtransMessageTextNum];
            var x12 = new X12object(messageText835);
            var listTranSetIds = x12.GetTranSetIds();
            var x835 = new X835(listEtranss[i], messageText835, listEtranss[i].TranSetId835, listEtrans835Attaches);
            var overallStatus = X835Status.Finalized;
            var countProcessedClaimsWithNameMismatches = 0;
            //If TranSetId835 is blank and we have multiple TranSetIds in the list, we know we are dealing with an Etrans from 14.2 or an older version
            //that represents multiple transactions from a single 835. We loop through the transactions and process each of them separately.
            if (listTranSetIds.Count >= 2 && listEtranss[i].TranSetId835 == "")
            {
                var listAutomationResults = AutoProcessMultiTransactionEtrans(listEtranss[i], messageText835, listTranSetIds, listEtrans835Attaches, isFullyAutomatic);
                countProcessedClaimsWithNameMismatches = listAutomationResults.Sum(x => x.CountProcessedClaimsWithNameMismatch);
                listAllAutomationResults.AddRange(listAutomationResults);
                if (listAutomationResults.Any(x => x.Status != X835Status.Finalized)) overallStatus = X835Status.Partial;
            }
            else
            {
                //Etrans has a single transaction
                var automationResult = TryAutoProcessEraEob(x835, listEtrans835Attaches, isFullyAutomatic);
                listAllAutomationResults.Add(automationResult);
                overallStatus = automationResult.Status;
                countProcessedClaimsWithNameMismatches = automationResult.CountProcessedClaimsWithNameMismatch;
            }

            X835AutoProcessed autoProcessedStatus;
            if (overallStatus != X835Status.Finalized)
            {
                autoProcessedStatus = X835AutoProcessed.SemiAutoIncomplete;
                if (isFullyAutomatic) autoProcessedStatus = X835AutoProcessed.FullAutoIncomplete;
            }
            else
            {
                autoProcessedStatus = X835AutoProcessed.SemiAutoComplete;
                if (isFullyAutomatic) autoProcessedStatus = X835AutoProcessed.FullAutoComplete;
            }

            var etrans835 = listEtrans835s.Find(x => x.EtransNum == listEtranss[i].EtransNum);
            if (etrans835 == null)
            {
                etrans835 = new Etrans835();
                etrans835.EtransNum = listEtranss[i].EtransNum;
            }

            //We make a single Etrans835 for etrans with multiple transactions, using the X835 for the first transaction.
            Etrans835s.Upsert(etrans835, x835, autoProcessedStatus);
            var etransOld = listEtranss[i].Copy();
            listEtranss[i].Note = EraAutomationResult.CreateEtransNote(overallStatus, listEtranss[i].Note, countProcessedClaimsWithNameMismatches);
            if (overallStatus == X835Status.Finalized)
                listEtranss[i].AckCode = "Recd";
            else
                listEtranss[i].AckCode = "";
            Update(listEtranss[i], etransOld); //Pass an old copy of the Etrans to ensure that we only update the AckCode.
        }

        return listAllAutomationResults;
    }

    /// <summary>
    ///     If etrans.TranSetId835 is blank and we have multiple TranSetIds in the list, we know we are dealing with an Etrans
    ///     from 14.2 or an older version
    ///     that represents multiple transactions from a single 835. We loop through the transactions and process each of them
    ///     separately.
    ///     This should not be called for newer etrans because they always represent a single transaction.
    /// </summary>
    private static List<EraAutomationResult> AutoProcessMultiTransactionEtrans(Etrans etrans, string messageText835,
        List<string> listTranSetIds, List<Etrans835Attach> listEtrans835Attaches, bool isFullyAutomatic)
    {
        var listEraAutomationResults = new List<EraAutomationResult>();
        for (var i = 0; i < listTranSetIds.Count; i++)
        {
            var x835 = new X835(etrans, messageText835, listTranSetIds[i], listEtrans835Attaches);
            var eraAutomationResult = TryAutoProcessEraEob(x835, listEtrans835Attaches, isFullyAutomatic);
            eraAutomationResult.TransactionNumber = i + 1;
            eraAutomationResult.TransactionCount = listTranSetIds.Count;
            listEraAutomationResults.Add(eraAutomationResult);
        }

        return listEraAutomationResults;
    }

    /// <summary>
    ///     Attempts to automatically receive claims and finalize payment for one EOB on an 835.
    ///     A deposit will be made if the ShowAutoDeposit pref is on.
    /// </summary>
    public static EraAutomationResult TryAutoProcessEraEob(X835 x835, List<Etrans835Attach> listEtrans835Attaches, bool isFullyAutomatic)
    {
        var listClaimsMatch = x835.RefreshClaims();
        var listHx835_ClaimsSplitToSkip = new List<Hx835_Claim>();
        var listHx835_ClaimsPaidToProcess = new List<Hx835_Claim>();
        var listClaimsToProcess = new List<Claim>();
        var eraAutomationResult = new EraAutomationResult();
        eraAutomationResult.X835Cur = x835;
        //Find matching claims and make sure they are attached and that split claims are attached.
        for (var i = 0; i < x835.ListClaimsPaid.Count; i++)
        {
            //These two conditions together indicate that the claim was manually detached by a user.
            if (x835.ListClaimsPaid[i].ClaimNum == 0 && x835.ListClaimsPaid[i].IsAttachedToClaim) continue;
            if (listHx835_ClaimsSplitToSkip.Any(x => x.ClpSegmentIndex == x835.ListClaimsPaid[i].ClpSegmentIndex))
                //Any Hx835_Claims in listSplitClaimsToSkip already have attaches and will be processed with the first split claim identified for a claim.
                continue;
            //Each iteration in this loop mimics attach creation from FormEtrans835Edit.gridClaimDetails_CellDoubleClick().
            var claim = listClaimsMatch.Find(x => x.ClaimNum == x835.ListClaimsPaid[i].ClaimNum);
            if (claim == null)
            {
                eraAutomationResult.ListPatNamesWithoutClaimMatch.Add(x835.ListClaimsPaid[i].PatientName.ToString());
                continue; //Couldn't find a matching claim, so skip to the next 835 claim.
            }

            var isAttachNeeded = !x835.ListClaimsPaid[i].IsAttachedToClaim;
            Etrans835Attaches.CreateForClaim(x835, x835.ListClaimsPaid[i], claim.ClaimNum, isAttachNeeded, listEtrans835Attaches, true);
            //Sync ClaimNum for all split claims in the same group.
            if (x835.ListClaimsPaid[i].IsSplitClaim)
            {
                var listHx835_ClaimsOtherSplit = x835.ListClaimsPaid[i].GetOtherNotDetachedSplitClaims();
                for (var j = 0; j < listHx835_ClaimsOtherSplit.Count; j++) Etrans835Attaches.CreateForClaim(x835, listHx835_ClaimsOtherSplit[j], claim.ClaimNum, isAttachNeeded, listEtrans835Attaches, true);
                //These will get processed with the current Hx835_Claim, so we don't want to add them to the listClaimsPaidToProcess.
                listHx835_ClaimsSplitToSkip.AddRange(listHx835_ClaimsOtherSplit);
            }

            //Add the 835 claim and the claim from DB to the lists of claims to process.
            listHx835_ClaimsPaidToProcess.Add(x835.ListClaimsPaid[i]);
            listClaimsToProcess.Add(claim);
        }

        var listClaimNums = listClaimsMatch.Select(x => x.ClaimNum).ToList();
        var listClaimProcsAll = ClaimProcs.RefreshForClaims(listClaimNums);
        var listHx835_ShortClaimProcsAll = listClaimProcsAll.Select(x => new Hx835_ShortClaimProc(x)).ToList();
        var listHx835_ShortClaims = listClaimsMatch.Select(x => new Hx835_ShortClaim(x)).ToList();
        var x835Status = x835.GetStatus(x835.GetClaimDataList(listHx835_ShortClaims), listHx835_ShortClaimProcsAll, listEtrans835Attaches);
        if (!x835Status.In(X835Status.Unprocessed, X835Status.Partial, X835Status.NotFinalized))
        {
            eraAutomationResult.DidEraStartAsFinalized = true;
            return eraAutomationResult;
        }

        //At this point we know that the user is allowed to make ins payments, the ERA has a status of unprocessed or partial,
        //and attaches are created for ERA claims and split claims. Now, we will try to process as many claims on the ERA as we can.
        var listPatNumsForClaims = listClaimsMatch.Select(x => x.PatNum).ToList();
        var listPlanNums = listClaimsToProcess.Select(x => x.PlanNum).ToList();
        var listPatients = Patients.GetMultPats(listPatNumsForClaims).ToList();
        var listInsPlans = InsPlans.GetPlans(listPlanNums);
        var listPayPlansValidInsForClaims = PayPlans.GetAllValidInsPayPlansForClaims(listClaimsToProcess);
        var listClaimProcsReceived = new List<ClaimProc>();
        for (var i = 0; i < listHx835_ClaimsPaidToProcess.Count; i++)
        {
            //Need to refresh this list in each loop iteration because listClaimProcsAll may have changed.
            listHx835_ShortClaimProcsAll = listClaimProcsAll.Select(x => new Hx835_ShortClaimProc(x)).ToList();
            if (listHx835_ClaimsPaidToProcess[i].IsProcessed(listHx835_ShortClaimProcsAll, listEtrans835Attaches))
            {
                eraAutomationResult.CountClaimsAlreadyProcessed++;
                continue;
            }

            var patient = listPatients.Find(x => x.PatNum == listClaimsToProcess[i].PatNum);
            var insPlan = listInsPlans.Find(x => x.PlanNum == listClaimsToProcess[i].PlanNum);
            //Get copy of claimprocs for claim from list so that we don't modify the list.
            var listClaimProcsForClaimCopy = listClaimProcsAll
                .Where(x => x.ClaimNum == listClaimsToProcess[i].ClaimNum)
                .Select(x => x.Copy())
                .ToList();
            var listPayPlans = FilterValidInsPayPlansForClaimHelper(listPayPlansValidInsForClaims, listClaimProcsAll, listClaimsToProcess[i]);
            var canClaimBeAutoProcessed = eraAutomationResult.CanClaimBeAutoProcessed(isFullyAutomatic, patient, insPlan, listHx835_ClaimsPaidToProcess[i], listPayPlans, listHx835_ShortClaimProcsAll,
                listClaimProcsForClaimCopy.Select(x => new Hx835_ShortClaimProc(x)).ToList(),
                listEtrans835Attaches);
            if (!canClaimBeAutoProcessed) continue;
            long payPlanNum = 0;
            if (listPayPlans.Count == 1)
                //We won't get here if listPayPlans.Count is greater than 1 because canClaimBeAutoProcessed will be false,
                //so it should be safe to choose the first PayPlanNum in the list.
                payPlanNum = listPayPlans[0].PayPlanNum;
            var isClaimRecieved = TryImportEraClaimData(x835, listHx835_ClaimsPaidToProcess[i], listClaimsToProcess[i],
                patient, true, listClaimProcsForClaimCopy, payPlanNum, eraAutomationResult);
            if (isClaimRecieved)
            {
                //If claim was received, claimprocs must have been modified and updated to DB, so update the claimprocs for the claim in our list.
                listClaimProcsAll.RemoveAll(x => x.ClaimNum == listClaimsToProcess[i].ClaimNum);
                listClaimProcsAll.AddRange(listClaimProcsForClaimCopy);
                listClaimProcsReceived.AddRange(listClaimProcsForClaimCopy);
                if (!listHx835_ClaimsPaidToProcess[i].DoesPatientNameMatch(patient)) eraAutomationResult.CountProcessedClaimsWithNameMismatch++;
            }
        }

        if (listClaimProcsReceived.Count > 0)
        {
            var listClaimsSecondarys = Claims.GetPrimaryOrSecondaryClaimsNotReceived(listClaimProcsReceived);
            for (var i = 0; i < listClaimsSecondarys.Count; i++)
            {
                var claimOld = listClaimsSecondarys[i].Copy();
                listClaimsSecondarys[i].ClaimStatus = "W"; //Waiting to send.
                //We don't need to update the secondary claims twice
                if (!PrefC.GetBool(PrefName.ClaimPrimaryReceivedRecalcSecondary)) Claims.Update(listClaimsSecondarys[i], claimOld);
            }

            if (PrefC.GetBool(PrefName.ClaimPrimaryReceivedRecalcSecondary) && listClaimsSecondarys.Count > 0) Claims.CalculateAndUpdateSecondaries(listClaimsSecondarys);
        }

        if (eraAutomationResult.AreAllClaimsReceived())
        {
            //Only attempt to finalize the payment if automation has processed all of the claims.
            var listClaimsForFinalization = x835.GetClaimsForFinalization(listClaimsMatch);
            var listClaimNumsForFinalization = listClaimsForFinalization.Select(x => x.ClaimNum).ToList();
            var listClaimProcsForFinalization = listClaimProcsAll.FindAll(x => listClaimNumsForFinalization.Contains(x.ClaimNum));
            eraAutomationResult.IsPaymentFinalized = TryFinalizeBatchPayment(x835, listClaimsForFinalization, listClaimProcsForFinalization,
                listPatients[0].ClinicNum, isAutomatic: true, eraAutomationResult: eraAutomationResult);
        }
        else
        {
            //Some claims could not be processed.
            eraAutomationResult.PaymentFinalizationError = Lans.g("X835", "Payment could not be finalized because one or more claims could not be processed.");
            eraAutomationResult.IsPaymentFinalized = false;
        }

        return eraAutomationResult;
    }

    ///<summary>Returns all etrans.EtransNum which correspond to ERA 835s and which do not have an Etrans835 record yet.</summary>
    public static List<long> GetErasMissingEtrans835(DateTime dateFrom, DateTime dateTo)
    {
        var dateTimeTrans = DbHelper.DtimeToDate("etrans.DateTimeTrans");
        var command = "SELECT etrans.EtransNum FROM etrans "
                      + "LEFT JOIN etrans835 ON etrans835.EtransNum=etrans.EtransNum "
                      + "WHERE etrans.Etype=" + (int) EtransType.ERA_835
                      + " AND " + dateTimeTrans + " >= " + SOut.Date(dateFrom) + " AND " + dateTimeTrans + " <= " + SOut.Date(dateTo)
                      + " AND etrans835.Etrans835Num IS NULL";
        return Db.GetListLong(command);
    }

    /// <summary>
    ///     Must pass in the list of payplans returned by PayPlans.GetAllValidInsPayPlansForClaims(), a list of all claimprocs
    ///     for claims being processed,
    ///     and the current claim being processed. Returns a list of insurance payplans that are valid for the claim passed in.
    /// </summary>
    public static List<PayPlan> FilterValidInsPayPlansForClaimHelper(List<PayPlan> listPayPlans, List<ClaimProc> listClaimProcsAlls, Claim claim)
    {
        var listClaimProcsForClaim = listClaimProcsAlls.FindAll(x => x.ClaimNum == claim.ClaimNum);
        var listPayPlanNumsForClaimProcs = listClaimProcsForClaim.Select(x => x.PayPlanNum).ToList();
        var payPlanForClaim = listPayPlans.Find(x => listPayPlanNumsForClaimProcs.Contains(x.PayPlanNum));
        if (payPlanForClaim != null)
            //If we find an insurance payplan that has claimprocs from the claim attached to it,
            //it is the only payplan we need because a claim can only be associated to one payplan.
            return new List<PayPlan> {payPlanForClaim};
        //The claim is not associated to a payplan yet, so we will return a list of payplans that are valid for it to associate to.
        var listPayPlansValidInsForClaim = new List<PayPlan>();
        var listPayPlanNumsForAllClaimProcs = listClaimProcsAlls.Select(x => x.PayPlanNum).ToList();
        for (var i = 0; i < listPayPlans.Count; i++)
        {
            if (listPayPlans[i].PatNum != claim.PatNum
                || listPayPlans[i].PlanNum != claim.PlanNum
                || listPayPlans[i].InsSubNum != claim.InsSubNum)
                continue; //Exclude payplans that aren't for the pat, insplan, and inssub of the claim.
            if (listPayPlanNumsForAllClaimProcs.Contains(listPayPlans[i].PayPlanNum)) continue; //If the payplan is associated to any claimproc in the list, it must be for a different claim.
            listPayPlansValidInsForClaim.Add(listPayPlans[i]); //We have a payplan that is valid for the claim and isn't attached to another claim.
        }

        return listPayPlansValidInsForClaim;
    }

    /// <summary>
    ///     Returns false if we are automatically processing an ERA but can't proceed.
    ///     Enter either by total and/or by procedure, depending on whether or not procedure detail was provided in the 835 for
    ///     this claim.
    ///     When isAutomatic is true, processing will not proceed if a by total payment would be made, or if we can't match all
    ///     claimprocs to payments.
    ///     This function creates the payment claimprocs.
    /// </summary>
    public static bool TryImportEraClaimData(X835 x835, Hx835_Claim hx835_ClaimPaid,
        Claim claim, Patient pat, bool isAutomatic, List<ClaimProc> listClaimProcsForClaim, long insPayPlanNum, EraAutomationResult eraAutomationResult = null)
    {
        var listClaimProcsOld = listClaimProcsForClaim.Select(x => x.Copy()).ToList();
        //CapClaim status is not considered because there should not be supplemental payments for capitaiton claims.
        var isSupplementalPay = claim.ClaimStatus == "R" || listClaimProcsForClaim.All(x => x.Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental));
        var listHx835_ClaimsNotDetachedPaid = new List<Hx835_Claim>();
        listHx835_ClaimsNotDetachedPaid.Add(hx835_ClaimPaid);
        if (hx835_ClaimPaid.IsSplitClaim) listHx835_ClaimsNotDetachedPaid.AddRange(hx835_ClaimPaid.GetOtherNotDetachedSplitClaims());
        var claimProcByTotal = new ClaimProc();
        claimProcByTotal.FeeBilled = 0; //All attached claimprocs will show in the grid and be used for the total sum.
        claimProcByTotal.DedApplied = (double) listHx835_ClaimsNotDetachedPaid.Sum(x => x.PatientDeductAmt);
        claimProcByTotal.AllowedOverride = (double) listHx835_ClaimsNotDetachedPaid.Sum(x => x.AllowedAmt);
        claimProcByTotal.InsPayAmt = (double) listHx835_ClaimsNotDetachedPaid.Sum(x => x.InsPaid);
        claimProcByTotal.WriteOff = 0;
        var isForPrimary = false;
        if (PrefC.GetEnum<EnumEraAutoPostWriteOff>(PrefName.EraAutoPostWriteOff) == EnumEraAutoPostWriteOff.PriFromERA)
            isForPrimary = hx835_ClaimPaid.CodeClp02.In("1", "19"); //"Processed as Primary", "Processed as Primary, Forwarded to Additional Payer(s)"
        else if (PrefC.GetEnum<EnumEraAutoPostWriteOff>(PrefName.EraAutoPostWriteOff) == EnumEraAutoPostWriteOff.PriFromPlan) isForPrimary = claim.ClaimType == "P";
        var canWriteOff = !isSupplementalPay && (isForPrimary || PrefC.GetEnum<EnumEraAutoPostWriteOff>(PrefName.EraAutoPostWriteOff) == EnumEraAutoPostWriteOff.Always);
        if (canWriteOff) claimProcByTotal.WriteOff = (double) listHx835_ClaimsNotDetachedPaid.Sum(x => x.WriteoffAmt);
        var listClaimProcsToEdit = new List<ClaimProc>();
        //Automatically set PayPlanNum if there is a payplan with matching PatNum, PlanNum, and InsSubNum that has not been paid in full.
        if (isSupplementalPay)
        {
            var listClaimProcsCopy = listClaimProcsForClaim.Select(x => x.Copy()).ToList();
            if (hx835_ClaimPaid.IsSplitClaim)
            {
                //Split supplemental payment, only CreateSuppClaimProcs for the sub set of split claim procs.
                for (var c = 0; c < listHx835_ClaimsNotDetachedPaid.Count; c++)
                for (var p = 0; p < listHx835_ClaimsNotDetachedPaid[c].ListProcs.Count; p++)
                {
                    var claimProc = listClaimProcsCopy.Find(x =>
                        x.ProcNum != 0 && (x.ProcNum == listHx835_ClaimsNotDetachedPaid[c].ListProcs[p].ProcNum //Consider using Hx835_Proc.TryGetMatchedClaimProc(...)
                                           || (x.CodeSent == listHx835_ClaimsNotDetachedPaid[c].ListProcs[p].ProcCodeBilled
                                               && (decimal) x.FeeBilled == listHx835_ClaimsNotDetachedPaid[c].ListProcs[p].ProcFee
                                               && x.Status == ClaimProcStatus.Received
                                               && x.TagOD == null))
                    );
                    if (claimProc == null) continue;
                    claimProc.TagOD = true;
                }

                //Remove all claimProcs that were not matched, to avoid entering payment on unmatched claimprocs.
                listClaimProcsCopy.RemoveAll(x => x.TagOD == null);
            }

            //Selection logic inside ClaimProcs.CreateSuppClaimProcs() mimics FormClaimEdit "Supplemental" button click.
            listClaimProcsToEdit = ClaimProcs.CreateSuppClaimProcs(listClaimProcsCopy, hx835_ClaimPaid.IsReversal, false);
            listClaimProcsForClaim.AddRange(listClaimProcsToEdit); //listClaimProcsToEdit is a subsSet of listClaimProcsForClaim, like above
        }
        else if (hx835_ClaimPaid.IsSplitClaim)
        {
            //Not supplemental, simply a split.
            var listHx835_Proc = listHx835_ClaimsNotDetachedPaid.SelectMany(x => x.ListProcs).ToList();
            //For split claims we only want to edit the sub-set of procs that exist on the internal claim.
            for (var i = 0; i < listHx835_Proc.Count; i++)
            {
                var claimProcFromClaim = listClaimProcsForClaim.Find(x =>
                    //Mimics proc matching in claimPaid.GetPaymentsForClaimProcs(...)
                    x.ProcNum != 0 && (x.ProcNum == listHx835_Proc[i].ProcNum //Consider using Hx835_Proc.TryGetMatchedClaimProc(...)
                                       || (x.CodeSent == listHx835_Proc[i].ProcCodeBilled
                                           && (decimal) x.FeeBilled == listHx835_Proc[i].ProcFee
                                           && x.Status == ClaimProcStatus.NotReceived
                                           && x.TagOD == null))
                );
                if (claimProcFromClaim == null) //Not found, By Total payment row will be inserted.
                    continue;
                claimProcFromClaim.TagOD = true;
                listClaimProcsToEdit.Add(claimProcFromClaim);
            }
        }
        else
        {
            //Original payment
            //Selection logic mimics FormClaimEdit "By Procedure" button selection logic.
            //Choose the claimprocs which are not received.
            for (var i = 0; i < listClaimProcsForClaim.Count; i++)
            {
                if (listClaimProcsForClaim[i].ProcNum == 0) //Exclude any "by total" claimprocs.  Choose claimprocs for procedures only.
                    continue;
                if (listClaimProcsForClaim[i].Status != ClaimProcStatus.NotReceived) //Ignore procedures already received.
                    continue;
                listClaimProcsToEdit.Add(listClaimProcsForClaim[i]); //Procedures not yet received.
            }

            //If all claimprocs are received, then choose claimprocs if not paid on.
            if (listClaimProcsToEdit.Count == 0)
                for (var i = 0; i < listClaimProcsForClaim.Count; i++)
                {
                    if (listClaimProcsForClaim[i].ProcNum == 0)
                        //Exclude any "by total" claimprocs.  Choose claimprocs for procedures only.
                        continue;
                    if (listClaimProcsForClaim[i].ClaimPaymentNum != 0)
                        //Exclude claimprocs already paid.
                        continue;
                    listClaimProcsToEdit.Add(listClaimProcsForClaim[i]); //Procedures not paid yet.
                }
        }

        var listHx835_ProcsUnassigned = listHx835_ClaimsNotDetachedPaid.SelectMany(x => x.ListProcs).ToList();
        //For each NotReceived/unpaid procedure on the claim where the procedure information can be successfully located on the EOB, enter the payment information.
        var listListHx835_ProcsForClaimProcs = Hx835_Claim.GetPaymentsForClaimProcs(listClaimProcsToEdit, listHx835_ProcsUnassigned);
        if (isAutomatic && listHx835_ProcsUnassigned.Count > 0)
        {
            //A claimproc was not found for one or more of the procedures on the ERA claim, so we cannot auto-process.
            if (isSupplementalPay) ClaimProcs.DeleteMany(listClaimProcsToEdit); //Supplemental claimProcs are pre inserted, delete if we do not post payment information.
            if (eraAutomationResult != null)
            {
                var errorMessage = Lans.g("X835", "One or more payments from the ERA could not be matched to a procedure on the claim.");
                eraAutomationResult.AddClaimError(pat, errorMessage);
            }

            return false;
        }

        for (var i = 0; i < listClaimProcsToEdit.Count; i++)
        {
            var claimProc = listClaimProcsToEdit[i];
            var listHx835_ProcsForProcNum = listListHx835_ProcsForClaimProcs[i];
            if (isAutomatic && listHx835_ProcsForProcNum.IsNullOrEmpty())
            {
                //Couldn't find an Hx835_Proc for one of the claimprocs that we are editing, so we cannot auto-process.
                if (isSupplementalPay) ClaimProcs.DeleteMany(listClaimProcsToEdit); //Supplemental claimProcs are pre inserted, delete if we do not post payment information.
                if (eraAutomationResult != null)
                {
                    var errorMessage = Lans.g("X835", "Payment information for one or more procedures on the claim were not found on the ERA.");
                    eraAutomationResult.AddClaimError(pat, errorMessage);
                }

                return false;
            }

            //If listProcsForProcNum.Count==0, then procedure payment details were not not found for this one specific procedure.
            //This can happen with procedures from older 837s, when we did not send out the procedure identifiers, in which case ProcNum would be 0.
            //Since we cannot place detail on the service line, we will leave the amounts for the procedure on the total payment line.
            //If listProcsForPorcNum.Count==1, then we know that the procedure was adjudicated as is or it might have been bundled, but we treat both situations the same way.
            //The 835 is required to include one line for each bundled procedure, which gives is a direct manner in which to associate each line to its original procedure.
            //If listProcForProcNum.Count > 1, then the procedure was either split or unbundled when it was adjudicated by the payer.
            //We will not bother to modify the procedure codes on the claim, because the user can see how the procedure was split or unbunbled by viewing the 835 details.
            //Instead, we will simply add up all of the partial payment lines for the procedure, and report the full payment amount on the original procedure.
            claimProc.DedApplied = 0;
            claimProc.AllowedOverride = 0;
            claimProc.InsPayAmt = 0;
            claimProc.ClaimAdjReasonCodes = "";
            if (claimProc.Status == ClaimProcStatus.Preauth) claimProc.InsPayEst = 0;
            claimProc.WriteOff = 0;
            if (isSupplementalPay)
                //This mimics how a normal supplemental payment is created in FormClaim edit "Supplemental" button click.
                //We do not do this in ClaimProcs.CreateSuppClaimProcs(...) for matching reasons.
                //Stops the claim totals from being incorrect.
                claimProc.FeeBilled = 0;
            var stringBuilder = new StringBuilder();
            var listClaimAdjReasonCodes = new List<string>();
            //TODO: Will the negative writeoff be cleared back to zero anywhere else (ex ClaimProc edit window)?
            for (var j = 0; j < listHx835_ProcsForProcNum.Count; j++)
            {
                var hx835_ProcPaidPartial = listHx835_ProcsForProcNum[j];
                claimProc.DedApplied += (double) hx835_ProcPaidPartial.DeductibleAmt;
                //Claim reversals purposefully exclude the Patient Responsibility Amount in the CLP segment.
                //See section 1.10.2.8 'Reversals and Corrections' on page 29 of the 835 documentation.
                if (hx835_ClaimPaid.IsReversal)
                    //Remove the PatientRespAmt from the AllowedAmt since it includes PatientRespAmt.
                    //This makes it so a Total Payment row for this particular difference is not erroneously suggested to the user.
                    claimProc.AllowedOverride += (double) (hx835_ProcPaidPartial.AllowedAmt - hx835_ProcPaidPartial.PatientPortionAmt);
                else
                    claimProc.AllowedOverride += (double) hx835_ProcPaidPartial.AllowedAmt;
                if (claimProc.Status == ClaimProcStatus.Preauth)
                    claimProc.InsPayEst += (double) hx835_ProcPaidPartial.PreAuthInsEst;
                else
                    claimProc.InsPayAmt += (double) hx835_ProcPaidPartial.InsPaid;
                if (canWriteOff) claimProc.WriteOff += (double) hx835_ProcPaidPartial.WriteoffAmt;
                if (stringBuilder.Length > 0) stringBuilder.Append("\r\n");
                stringBuilder.Append(hx835_ProcPaidPartial.GetRemarks());
                listClaimAdjReasonCodes.AddRange(hx835_ProcPaidPartial.ListProcAdjustments.Select(x => x.ReasonCode));
            }

            claimProc.ClaimAdjReasonCodes = string.Join(",", listClaimAdjReasonCodes.Distinct()); //Save all distinct reason codes as comma delimited list.
            claimProc.Remarks = stringBuilder.ToString();
            if (claim.ClaimType == "PreAuth")
            {
                claimProc.Status = ClaimProcStatus.Preauth;
            }
            else if (claim.ClaimType == "Cap")
            {
                //Do nothing.  The claimprocstatus will remain Capitation.
            }
            else
            {
                //Received or Supplemental
                if (isSupplementalPay)
                {
                    //This is already set in ClaimProcs.CreateSuppClaimProcs() above, but lets make it clear for others.
                    claimProc.Status = ClaimProcStatus.Supplemental;
                    claimProc.IsNew = true; //Used in FormEtrans835ClaimPay.FillGridProcedures().
                }
                else
                {
                    //Received.  Original payment
                    claimProc.Status = ClaimProcStatus.Received;
                    claimProc.PayPlanNum = insPayPlanNum; //Payment plans do not exist for PreAuths or Capitation claims, by definition.
                    if (hx835_ClaimPaid.IsSplitClaim) claimProc.IsNew = true; //Used in FormEtrans835ClaimPay.FillGridProcedures() to highlight the procs on this split claim
                }

                claimProc.DateEntry = DateTime.Now; //Date is was set rec'd or supplemental.
            }

            claimProc.DateCP = DateTime.Today;
        }

        //Limit the scope of the "By Total" payment to the new claimprocs only.
        //This "By Total" payment will account for any procedures that could not be matched to the
        //procedures reported on the ERA due to any changes that occurred after the claim was originally sent.
        for (var i = 0; i < listClaimProcsToEdit.Count; i++)
        {
            var claimProc = listClaimProcsToEdit[i];
            claimProcByTotal.DedApplied -= claimProc.DedApplied;
            claimProcByTotal.AllowedOverride -= claimProc.AllowedOverride;
            claimProcByTotal.InsPayAmt -= claimProc.InsPayAmt;
            if (canWriteOff) claimProcByTotal.WriteOff -= claimProc.WriteOff; //May cause cpByTotal.Writeoff to go negative if the user typed in the value for claimProc.Writeoff.
            if (isAutomatic)
                //We don't want to set AllowedOverrides for automatically processed claimprocs because the allowed amounts we calculate from ERAs may be
                //inaccurate, and we don't want to automatically create inaccurate blue book data. We set AllowedOverride to -1 for all claimprocs we
                //are editing after making cpByTotal calculations so that we don't
                //throw the calculations off and make a By Total claimproc that we shouldn't.
                claimProc.AllowedOverride = -1; //-1 represents a blank AllowedOverride.
        }

        //The writeoff may be negative if the user manually entered some payment amounts before loading this window, if UCR fee schedule incorrect, and is always negative for reversals.
        if (!hx835_ClaimPaid.IsReversal && claimProcByTotal.WriteOff < 0) claimProcByTotal.WriteOff = 0;
        var isByTotalIncluded = true;
        //Do not create a total payment if the payment contains all zero amounts, because it would not be useful.  Written to account for potential rounding errors in the amounts.
        if (Math.Round(claimProcByTotal.DedApplied, 2, MidpointRounding.AwayFromZero) == 0
            && Math.Round(claimProcByTotal.AllowedOverride, 2, MidpointRounding.AwayFromZero) == 0
            && Math.Round(claimProcByTotal.InsPayAmt, 2, MidpointRounding.AwayFromZero) == 0
            && Math.Round(claimProcByTotal.WriteOff, 2, MidpointRounding.AwayFromZero) == 0)
            isByTotalIncluded = false;
        if (claim.ClaimType == "PreAuth")
            //In the claim edit window we currently block users from entering PreAuth payments by total, presumably because total payments affect the patient balance.
            isByTotalIncluded = false;
        else if (claim.ClaimType == "Cap")
            //In the edit claim window, we currently warn and discourage users from entering Capitation payments by total, because total payments affect the patient balance.
            isByTotalIncluded = false;
        if (isByTotalIncluded)
        {
            if (isAutomatic)
            {
                //Cannot create by total payments when processing automatically.
                if (isSupplementalPay) ClaimProcs.DeleteMany(listClaimProcsToEdit); //Supplemental claimProcs are pre inserted, delete if we do not post payment information.
                if (eraAutomationResult != null)
                {
                    var errorMessage = Lans.g("FormEtrans835Edit", "Automatic processing would have resulted in an As Total payment.");
                    eraAutomationResult.AddClaimError(pat, errorMessage);
                }

                return false;
            }

            claimProcByTotal.Status = ClaimProcStatus.Received;
            if (isSupplementalPay) //Without this, two payment lines would show in the account module.
                claimProcByTotal.Status = ClaimProcStatus.Supplemental;
            claimProcByTotal.ClaimNum = claim.ClaimNum;
            claimProcByTotal.PatNum = claim.PatNum;
            claimProcByTotal.ProvNum = claim.ProvTreat;
            claimProcByTotal.PlanNum = claim.PlanNum;
            claimProcByTotal.InsSubNum = claim.InsSubNum;
            claimProcByTotal.DateCP = DateTime.Today;
            claimProcByTotal.ProcDate = claim.DateService;
            claimProcByTotal.DateEntry = DateTime.Now;
            claimProcByTotal.ClinicNum = claim.ClinicNum;
            claimProcByTotal.Remarks = string.Join("\r\n", listHx835_ClaimsNotDetachedPaid.Select(x => x.GetRemarks()));
            claimProcByTotal.PayPlanNum = insPayPlanNum;
            claimProcByTotal.IsNew = true; //Used in FormEtrans835ClaimPay.FillGridProcedures().
            //Add the total payment to the beginning of the list, so that the ins paid amount for the total payment will be highlighted when FormEtrans835ClaimPay loads.
            listClaimProcsForClaim.Insert(0, claimProcByTotal);
        }

        if (isAutomatic)
        {
            ReceiveEraPayment(claim, hx835_ClaimPaid, listClaimProcsForClaim, PrefC.GetBool(PrefName.EraIncludeWOPercCoPay), isSupplementalPay, isAutomatic: isAutomatic);
            if (eraAutomationResult != null) eraAutomationResult.CountClaimsProcessed++;
            if (PrefC.GetBool(PrefName.ClaimSnapshotEnabled))
            {
                var claimCur = Claims.GetClaim(listClaimProcsOld[0].ClaimNum);
                if (claimCur.ClaimType != "PreAuth") ClaimSnapshots.CreateClaimSnapshot(listClaimProcsOld, ClaimSnapshotTrigger.InsPayment, claimCur.ClaimType);
            }
        }

        return true;
    }

    /// <summary>
    ///     Receives the claim and to set the claim dates and totals properly. isIncludeWOPercCoPay=true causes WriteOffs to be
    ///     posted for
    ///     ClaimProcs associated to Category Percentage or Medicaid/Flat CoPay insurance plans, false does not post WriteOffs
    ///     for these insurance plan
    ///     types.  isSupplementalPay=true causes claim to not be marked received because Supplemental payments can only be
    ///     applied to previously
    ///     received claims, false allows the claim to be marked received if all ClaimProcs in listClaimProcsForClaim meet
    ///     requirements.
    /// </summary>
    public static void ReceiveEraPayment(Claim claim, Hx835_Claim hx835_ClaimPaid, List<ClaimProc> listClaimProcsForClaim, bool isIncludeWOPercCoPay,
        bool isSupplementalPay, InsPlan insPlan = null, bool isAutomatic = false)
    {
        var claimOld = claim.Copy();
        //Recalculate insurance paid, deductible, and writeoff amounts for the claim based on the final claimproc values, then save the results to the database.
        claim.InsPayAmt = 0;
        claim.DedApplied = 0;
        claim.WriteOff = 0;
        var insPlanCur = insPlan;
        if (!isIncludeWOPercCoPay && insPlanCur == null) //Might not want to include WOs, need to check plan type.
            insPlanCur = InsPlans.RefreshOne(claim.PlanNum);
        List<ClaimProc> listClaimProcsAllForProcs = null;
        for (var i = 0; i < listClaimProcsForClaim.Count; i++)
        {
            if (listClaimProcsForClaim[i].Status == ClaimProcStatus.Preauth)
            {
                //Mimics FormClaimEdit preauth by procedure logic.
                if (listClaimProcsAllForProcs == null)
                {
                    var listProcNumsForClaim = listClaimProcsForClaim.Select(x => x.ProcNum).ToList();
                    listClaimProcsAllForProcs = ClaimProcs.GetForProcs(listProcNumsForClaim); //Get All ClaimProcs for Procs on Claim.
                }

                ClaimProcs.SetInsEstTotalOverride(listClaimProcsForClaim[i].ProcNum, listClaimProcsForClaim[i].PlanNum,
                    listClaimProcsForClaim[i].InsSubNum, listClaimProcsForClaim[i].InsPayEst, listClaimProcsAllForProcs);
                ClaimProcs.Update(listClaimProcsForClaim[i]);
                continue; //SetInsEstTotalOverride() updates claimProc to database.
            }

            claim.InsPayAmt += listClaimProcsForClaim[i].InsPayAmt;
            claim.DedApplied += listClaimProcsForClaim[i].DedApplied;
            //If pref is off, Category Percentage or Medicaid/FlatCopay do not include Writeoff.
            if (!isIncludeWOPercCoPay && insPlanCur != null && insPlanCur.PlanType.In("", "f"))
                //Do not include WriteOff in claim total.
                //Also need to change the claimProc directly, this really only matters when automatically recieving the ERA payment.
                listClaimProcsForClaim[i].WriteOff = 0;
            claim.WriteOff += listClaimProcsForClaim[i].WriteOff;
            if (listClaimProcsForClaim[i].ClaimProcNum == 0) //Total payment claimproc which was created in FormEtrans835Edit just before loading this window.
                ClaimProcs.Insert(listClaimProcsForClaim[i]);
            else //Procedure claimproc, because the estimate already existed before entering payment.
                ClaimProcs.Update(listClaimProcsForClaim[i]);
        }

        if (!isSupplementalPay //Supplemental payments can only be applied to previously received claims
            //Split claims mark claimProcs recieved one at a time.
            && listClaimProcsForClaim.All(x => x.Status.In(
                ClaimProcStatus.Received, ClaimProcStatus.Supplemental, ClaimProcStatus.CapClaim, ClaimProcStatus.Preauth)))
        {
            //Do not mark received until all claim procs are handled.
            claim.ClaimStatus = "R"; //Received.
            claim.DateReceived = hx835_ClaimPaid.DateReceived;
        }

        if (isAutomatic)
            MakeEraClaimAutomationLog(claimOld, claim);
        else
            SecurityLogs.MakeLogEntry(EnumPermType.InsPayCreate, 0, "ERA claim payment received");
        Claims.Update(claim);
        ClaimProcs.RemoveSupplementalTransfersForClaims(claim.ClaimNum);
        InsBlueBooks.SynchForClaimNums(claim.ClaimNum);
        if (PrefC.GetBool(PrefName.ClaimPrimaryReceivedRecalcSecondary) && claim.ClaimStatus == "R" && claimOld.ClaimStatus != "R" && claim.ClaimType == "P") Claims.CalculateAndUpdateSecondariesFromPrimaries(new List<Claim> {claim});
        if (isAutomatic || Security.IsAuthorized(EnumPermType.PaymentCreate, DateTime.Today, true)) PaymentEdit.MakeIncomeTransferForClaimProcs(claim.PatNum, listClaimProcsForClaim);
    }

    ///<summary>Creates a security log entry indicating that a claim was recieved via ERA automation.</summary>
    private static void MakeEraClaimAutomationLog(Claim claimOld, Claim claimNew)
    {
        var stringBuilderLog = new StringBuilder();
        stringBuilderLog.Append(Lans.g("X835", "Claim payment received during automatic processing of ERA."));
        var old = Lans.g("X835", "Old");
        var strNew = Lans.g("X835", "new");
        var insurancePayment = Lans.g("X835", "insurance payment:");
        var deductableApplied = Lans.g("X835", "deductable applied:");
        var writeoff = Lans.g("X835", "writeoff:");
        if (!Equals(claimNew.InsPayAmt, claimOld.InsPayAmt))
            stringBuilderLog.Append($" {old} {insurancePayment} {claimOld.InsPayAmt.ToString("c")}, " +
                                    $"{strNew} {insurancePayment} {claimNew.InsPayAmt.ToString("c")}.");
        if (!Equals(claimNew.DedApplied, claimOld.DedApplied))
            stringBuilderLog.Append($" {old} {deductableApplied} {claimOld.DedApplied.ToString("c")}, " +
                                    $"{strNew} {deductableApplied} {claimNew.DedApplied.ToString("c")}.");
        if (!Equals(claimNew.WriteOff, claimOld.WriteOff))
            stringBuilderLog.Append($" {old} {writeoff} {claimOld.WriteOff.ToString("c")}, " +
                                    $"{strNew} {writeoff} {claimNew.WriteOff.ToString("c")}.");
        var stringLog = stringBuilderLog.ToString();
        SecurityLogs.MakeLogEntry(EnumPermType.InsPayCreate, claimNew.PatNum, stringLog, claimNew.ClaimNum, claimNew.SecDateTEdit);
    }

    /// <summary>
    ///     Returns false if an error is encountered and payment is not finalized.
    ///     Attempts to finalize the batch insurance payment for an ERA.
    ///     If isAutomatic is true, the batch payment is created without user input.
    ///     A deposit will be automatically madef or the payment if the ShowAutoDeposit pref is on.
    /// </summary>
    public static bool TryFinalizeBatchPayment(X835 x835, List<Claim> listClaims, List<ClaimProc> listClaimProcsAll, long clinicNum,
        ClaimPayment claimPayment = null, bool isAutomatic = false, EraAutomationResult eraAutomationResult = null)
    {
        //Date not considered here, but it will be considered when saving the claimpayment to prevent backdating.
        //When isAutomatic is true this check should be done in the forms that lead to this method being called.
        if (!isAutomatic && !Security.IsAuthorized(EnumPermType.InsPayCreate)) return false;
        if (listClaims.Count == 0)
        {
            if (isAutomatic && eraAutomationResult != null)
                eraAutomationResult.PaymentFinalizationError = Lans.g("X835", "Payment could not be finalized because all "
                                                                              + "claims have been detached from this ERA or are preauths (there is no payment).");
            return false;
        }

        if (listClaims.Exists(x => x.ClaimNum == 0 || x.ClaimStatus != "R"))
        {
            if (isAutomatic && eraAutomationResult != null) eraAutomationResult.PaymentFinalizationError = Lans.g("X835", "Payment could not be finalized because one or more claims are not marked received.");
            return false;
        }

        if (listClaimProcsAll.Exists(x => !x.Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental, ClaimProcStatus.CapClaim)))
        {
            if (isAutomatic && eraAutomationResult != null)
                eraAutomationResult.PaymentFinalizationError = Lans.g("X835", "Payment could not be finalized because not all "
                                                                              + "claim procedures have a status of Received, Supplemental, or CapClaim.");
            return false;
        }

        #region ClaimPayment creation

        if (claimPayment == null) claimPayment = new ClaimPayment();
        //Mimics FormClaimEdit.butBatch_Click(...)
        claimPayment.CheckDate = MiscData.GetNowDateTime().Date; //Today's date for easier tracking by the office and to avoid backdating before accounting lock dates.
        claimPayment.ClinicNum = clinicNum;
        claimPayment.CarrierName = x835.PayerName;
        claimPayment.CheckAmt = listClaimProcsAll.Where(x => x.ClaimPaymentNum == 0).Sum(x => x.InsPayAmt); //Ignore claimprocs associated to previously finalized payments.
        claimPayment.CheckNum = x835.TransRefNum;
        claimPayment.PayType = X835.GetInsurancePaymentTypeDefNum(x835.PaymentMethodCode);
        claimPayment.IsPartial = true; //This flag is changed to "false" when the payment is finalized from inside FormClaimPayBatch.
        if (isAutomatic)
        {
            //We shouldn't automatically make payments that don't match the amount on the ERA.
            if (!CompareDecimal.IsEqual((decimal) claimPayment.CheckAmt, x835.InsPaid))
            {
                if (eraAutomationResult != null)
                    eraAutomationResult.PaymentFinalizationError = Lans.g("X835", "Payment could not be finalized because the "
                                                                                  + "amount paid for claim procedures does not match the total payment from the ERA.");
                return false;
            }

            claimPayment.IsPartial = false;
            AutoFinalizeBatchPaymentHelper(claimPayment, listClaimProcsAll); //Inserts the claimPay
            return true;
        }

        ClaimPayments.Insert(claimPayment);
        var listClaimProcsToUpdate = listClaimProcsAll.FindAll(x => x.ClaimPaymentNum == 0 && x.IsTransfer == false);
        for (var i = 0; i < listClaimProcsToUpdate.Count; i++)
        {
            listClaimProcsToUpdate[i].ClaimPaymentNum = claimPayment.ClaimPaymentNum;
            ClaimProcs.Update(listClaimProcsToUpdate[i]);
        }

        #endregion ClaimPayment creation

        return true;
    }

    /// <summary>
    ///     Creates a deposit for the claim payment if PrefName.ShowAutoDeposit is true, logs the claim payment, and
    ///     updates ClaimProcs.
    /// </summary>
    private static void AutoFinalizeBatchPaymentHelper(ClaimPayment claimPayment, List<ClaimProc> listClaimProcs)
    {
        //AutoDeposit. Normally done in FormClaimPayEdit.butOK_Click()
        var doAutoDeposit = PrefC.GetBool(PrefName.ShowAutoDeposit);
        if (doAutoDeposit)
        {
            var deposit = new Deposit();
            //I don't think there is any way for us to know what batch number or account to use, so they are left as default values.
            //deposit.Batch="";
            //deposit.DepositAccountNum=0;
            deposit.Amount = claimPayment.CheckAmt;
            deposit.DateDeposit = claimPayment.CheckDate;
            claimPayment.DepositNum = Deposits.Insert(deposit);
            //Log deposit
            SecurityLogs.MakeLogEntry(EnumPermType.DepositSlips, 0
                , Lans.g("FormClaimPayEdit", "Auto Deposit created during automatic processing of ERA:") + " " + deposit.DateDeposit.ToShortDateString()
                  + " " + Lans.g("FormClaimPayEdit", "New") + " " + deposit.Amount.ToString("c"));
        }

        //Insert ClaimPayment and make log that would normally be made in FormClaimPayEdit.butOK_Click()
        ClaimPayments.Insert(claimPayment);
        SecurityLogs.MakeLogEntry(EnumPermType.InsPayCreate, 0,
            Lans.g("FormClaimPayEdit", "Insurance Payment created during automatic processing of ERA.") + " "
                                                                                                        + Lans.g("FormClaimPayEdit", "Carrier Name: ") + claimPayment.CarrierName + ", "
                                                                                                        + Lans.g("FormClaimPayEdit", "Total Amount: ") + claimPayment.CheckAmt.ToString("c") + ", "
                                                                                                        + Lans.g("FormClaimPayEdit", "Check Date: ") + claimPayment.CheckDate.ToShortDateString() + ", " //Date the check is entered in the system (i.e. today)
                                                                                                        + "ClaimPaymentNum: " + claimPayment.ClaimPaymentNum); //Column name, not translated.
        //Update the ClaimPaymentNum and DateCP for ClaimProcs
        //Only update claimprocs that are not already associated to another claim payment.
        //This will happen when this ERA contains claim reversals or corrections, both are entered as supplemental payments.
        var listClaimProcNums = listClaimProcs
            .Where(x => x.ClaimPaymentNum == 0 && x.IsTransfer == false)
            .Select(x => x.ClaimProcNum)
            .ToList();
        ClaimProcs.UpdateForClaimPayment(listClaimProcNums, claimPayment);
        //Sets DateInsFinalized for ClaimProcs. Normally done in FormClaimEdit.FormFinalizePaymentHelper()
        ClaimProcs.DateInsFinalizedHelper(claimPayment.ClaimPaymentNum, PrefC.DateClaimReceivedAfter);
    }
}