using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SmsPhones
{
    public enum MessageCharSet
    {
        Text, //7-bit char set used in text messaging which represents the gsm char set
        Unicode //16-bit char set used in text messaging
    }

    /// <summary>
    ///     Used to display "SHORTCODE" on the customer side as the phone number for SMS sent via Short Code.  End user does
    ///     not need to see the
    ///     specific short code number we use at HQ.  This ensures we do not recored this communication on a different valid
    ///     SmsPhone/VLN that it didn't
    ///     truly take place on.  However, on the HQ side, we want records of this communication to be listed as having taken
    ///     place on the actual Short
    ///     Code number.
    /// </summary>
    public const string SHORTCODE = "SHORTCODE";

    ///<summary>Gets one SmsPhone from the db. Returns null if not found.</summary>
    public static SmsPhone GetByPhone(string phoneNumber)
    {
        var command = "SELECT * FROM smsphone WHERE PhoneNumber='" + SOut.String(phoneNumber) + "'";
        return SmsPhoneCrud.SelectOne(command);
    }

    
    public static long Insert(SmsPhone smsPhone)
    {
        return SmsPhoneCrud.Insert(smsPhone);
    }

    
    public static void Update(SmsPhone smsPhone)
    {
        SmsPhoneCrud.Update(smsPhone);
    }

    ///<summary>This will only be called by HQ via the listener in the event that this number has been cancelled.</summary>
    public static void UpdateToInactive(string phoneNumber)
    {
        var smsPhone = GetByPhone(phoneNumber);
        if (smsPhone == null) return;
        smsPhone.DateTimeInactive = DateTime.Now;
        SmsPhoneCrud.Update(smsPhone);
    }

    ///<summary>Gets sms phones when not using clinics.</summary>
    public static List<SmsPhone> GetForPractice()
    {
        //Get for practice is just getting for clinic num 0
        return GetForClinics(new List<long> {0}); //clinic num 0
    }

    public static List<SmsPhone> GetForClinics(List<long> listClinicNums)
    {
        if (listClinicNums.Count == 0) return new List<SmsPhone>();

        var command = "SELECT * FROM smsphone WHERE ClinicNum IN (" + string.Join(",", listClinicNums) + ")";
        return SmsPhoneCrud.SelectMany(command);
    }

    public static List<SmsPhone> GetAll()
    {
        var command = "SELECT * FROM smsphone";
        return SmsPhoneCrud.SelectMany(command);
    }

    public static DataTable GetSmsUsageLocal(List<long> listClinicNums, DateTime dateMonth, List<SmsPhone> listSmsPhones)
    {
        #region Initialize tableSmsUsageLocal DataTable

        var strNoActivePhones = "No Active Phones";
        var listSmsPhonesClinicNum = listSmsPhones.FindAll(x => listClinicNums.Contains(x.ClinicNum));
        var dateStart = dateMonth.Date.AddDays(1 - dateMonth.Day); //remove time portion and day of month portion. Remainder should be midnight of the first of the month
        var dateEnd = dateStart.AddMonths(1); //This should be midnight of the first of the following month.
        //This query builds the data table that will be filled from several other queries, instead of writing one large complex query.
        //It is written this way so that the queries are simple to write and understand, and makes Oracle compatibility easier to maintain.
        var command = @"SELECT 
				CAST(0 AS DECIMAL(25,0)) ClinicNum,
				' ' PhoneNumber,
				' ' CountryCode,
				0 SentMonth,
				0.0 SentCharge,
				0.0 SentDiscount,
				0.0 SentPreDiscount,
				0 ReceivedMonth,
				0.0 ReceivedCharge 
				FROM
				DUAL"; //this is a cute way to get a data table with the correct layout without having to query any real data.
        var tableSmsUsageLocal = DataCore.GetTable(command).Clone(); //use .Clone() to get schema only, with no rows.
        tableSmsUsageLocal.TableName = "SmsUsageLocal";
        for (var i = 0; i < listClinicNums.Count; i++)
        {
            var dataRow = tableSmsUsageLocal.NewRow();
            dataRow["ClinicNum"] = listClinicNums[i];
            dataRow["PhoneNumber"] = strNoActivePhones;
            var smsPhoneFirstActive = listSmsPhonesClinicNum
                .FindAll(x => x.ClinicNum == listClinicNums[i]) //phones for this clinic
                .FindAll(x => x.DateTimeInactive.Year < 1880) //that are active
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.DateTimeActive)
                .FirstOrDefault();
            if (smsPhoneFirstActive != null)
            {
                dataRow["PhoneNumber"] = smsPhoneFirstActive.PhoneNumber;
                dataRow["CountryCode"] = smsPhoneFirstActive.CountryCode;
            }

            dataRow["SentMonth"] = 0;
            dataRow["SentCharge"] = 0.0;
            dataRow["SentDiscount"] = 0.0;
            dataRow["SentPreDiscount"] = 0.0;
            dataRow["ReceivedMonth"] = 0;
            dataRow["ReceivedCharge"] = 0.0;
            tableSmsUsageLocal.Rows.Add(dataRow);
        }

        #endregion

        #region Fill tableSmsUsageLocal DataTable

        //Sent Last Month
        command = "SELECT ClinicNum, COUNT(*), ROUND(SUM(MsgChargeUSD),2),ROUND(SUM(MsgDiscountUSD),2)"
                  + ",SUM(CASE SmsPhoneNumber WHEN '" + SOut.String(SHORTCODE) + "' THEN 1 ELSE 0 END) FROM smstomobile "
                  + "WHERE DateTimeSent >=" + SOut.Date(dateStart) + " "
                  + "AND DateTimeSent<" + SOut.Date(dateEnd) + " "
                  + "AND MsgChargeUSD>0 GROUP BY ClinicNum";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        for (var j = 0; j < tableSmsUsageLocal.Rows.Count; j++)
        {
            if (tableSmsUsageLocal.Rows[j]["ClinicNum"].ToString() != table.Rows[i]["ClinicNum"].ToString()) continue;
            tableSmsUsageLocal.Rows[j]["SentMonth"] = table.Rows[i][1]; //.ToString();
            tableSmsUsageLocal.Rows[j]["SentCharge"] = table.Rows[i][2]; //.ToString();
            tableSmsUsageLocal.Rows[j]["SentDiscount"] = table.Rows[i][3];
            tableSmsUsageLocal.Rows[j]["SentPreDiscount"] = SIn.Double(tableSmsUsageLocal.Rows[j]["SentCharge"].ToString()) + SIn.Double(tableSmsUsageLocal.Rows[j]["SentDiscount"].ToString());
            //No active phone but at least one of these messages sent from Short Code
            if (tableSmsUsageLocal.Rows[j]["PhoneNumber"].ToString() == strNoActivePhones && SIn.Long(table.Rows[i][4].ToString()) > 0) tableSmsUsageLocal.Rows[j]["PhoneNumber"] = SOut.String(SHORTCODE); //display "SHORTCODE" as primary number.
            break;
        }

        //Received Month
        command = "SELECT ClinicNum, COUNT(*),SUM(CASE SmsPhoneNumber WHEN '" + SOut.String(SHORTCODE) + "' THEN 1 ELSE 0 END) FROM smsfrommobile "
                  + "WHERE DateTimeReceived >=" + SOut.Date(dateStart) + " "
                  + "AND DateTimeReceived<" + SOut.Date(dateEnd) + " "
                  + "GROUP BY ClinicNum";
        table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        for (var j = 0; j < tableSmsUsageLocal.Rows.Count; j++)
        {
            if (tableSmsUsageLocal.Rows[j]["ClinicNum"].ToString() != table.Rows[i]["ClinicNum"].ToString()) continue;
            tableSmsUsageLocal.Rows[j]["ReceivedMonth"] = table.Rows[i][1].ToString();
            tableSmsUsageLocal.Rows[j]["ReceivedCharge"] = "0";
            //No active phone but at least one of these messages sent from Short Code
            if (tableSmsUsageLocal.Rows[j]["PhoneNumber"].ToString() == strNoActivePhones && SIn.Long(table.Rows[i][2].ToString()) > 0) tableSmsUsageLocal.Rows[j]["PhoneNumber"] = SOut.String(SHORTCODE); //display "SHORTCODE" as primary number.
            break;
        }

        #endregion

        return tableSmsUsageLocal;
    }

    /// <summary>
    ///     Find all phones in the db (by PhoneNumber) and sync with listPhonesSync. If a given PhoneNumber does not already
    ///     exist then insert the SmsPhone.
    ///     If a given PhoneNumber exists in the local db but does not exist in the HQ-provided listPhoneSync, then deacitvate
    ///     that phone locallly.
    ///     Return true if a change has been made to the database.
    /// </summary>
    public static bool UpdateOrInsertFromList(List<SmsPhone> listSmsPhonesSync)
    {
        //Get all phones so we can filter as needed below.
        var command = "SELECT * FROM smsphone";
        var listSmsPhonesDb = SmsPhoneCrud.SelectMany(command);
        var listSmsPhonesFiltered = new List<SmsPhone>();
        var isChanged = false;
        //Deal with phones that occur in the HQ-supplied list.
        for (var i = 0; i < listSmsPhonesSync.Count; i++)
        {
            listSmsPhonesFiltered = listSmsPhonesDb.FindAll(x => x.PhoneNumber == listSmsPhonesSync[i].PhoneNumber);
            if (listSmsPhonesFiltered.IsNullOrEmpty())
            {
                //This phone does not yet exist in the DB.
                Insert(listSmsPhonesSync[i]);
                isChanged = true;
                continue;
            }

            //We don't expect this to happen often and if it does the list would be short.
            for (var j = 0; j < listSmsPhonesFiltered.Count; j++)
            {
                //This phone already exists. Update it to look like the phone we are trying to insert.
                listSmsPhonesFiltered[j].ClinicNum = listSmsPhonesSync[i].ClinicNum; //The clinic may have changed so set it to the new clinic.
                listSmsPhonesFiltered[j].CountryCode = listSmsPhonesSync[i].CountryCode;
                listSmsPhonesFiltered[j].DateTimeActive = listSmsPhonesSync[i].DateTimeActive;
                listSmsPhonesFiltered[j].DateTimeInactive = listSmsPhonesSync[i].DateTimeInactive;
                listSmsPhonesFiltered[j].InactiveCode = listSmsPhonesSync[i].InactiveCode;
                Update(listSmsPhonesFiltered[j]);
                isChanged = true;
            }
        }

        //Deal with phones which are in the local db but that do not occur in the HQ-supplied list.
        var listSmsPhones = listSmsPhonesDb.FindAll(x => !listSmsPhonesSync.Any(y => y.PhoneNumber == x.PhoneNumber));
        for (var i = 0; i < listSmsPhones.Count; i++)
        {
            //This phone not found at HQ so deactivate it.
            listSmsPhones[i].DateTimeInactive = DateTime.Now;
            listSmsPhones[i].InactiveCode = "Phone not found at HQ";
            Update(listSmsPhones[i]);
            isChanged = true;
        }

        return isChanged;
    }

    ///<summary>Returns current clinic limit minus message usage for current calendar month.</summary>
    public static double GetClinicBalance(long clinicNum)
    {
        double limit = 0;
        if (true)
        {
            if (clinicNum == 0 && Clinics.GetCount(true) > 0) //Sending text for "Unassigned" patient. Use the first non-hidden clinic. (for now)
                clinicNum = Clinics.GetFirst(true).Id;
            var clinic = Clinics.GetClinic(clinicNum);
            if (clinic != null && clinic.SmsContractSignedOn.HasValue) limit = clinic.SmsMonthlyLimit;
        }
        else
        {
            if (PrefC.GetDate(PrefName.SmsContractDate).Year > 1880) limit = PrefC.GetDouble(PrefName.SmsMonthlyLimit, true);
        }

        var dateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var dateEnd = dateStart.AddMonths(1);
        var command = "SELECT SUM(MsgChargeUSD) FROM smstomobile WHERE ClinicNum=" + SOut.Long(clinicNum) + " "
                      + "AND DateTimeSent>=" + SOut.Date(dateStart) + " AND DateTimeSent<" + SOut.Date(dateEnd);
        limit -= SIn.Double(DataCore.GetScalar(command));
        return limit;
    }

    /// <summary>
    ///     Returns true if texting is enabled for any clinics (including hidden), or if not using clinics, if it is
    ///     enabled for the practice.
    /// </summary>
    public static bool IsIntegratedTextingEnabled()
    {
        if (true) return Clinics.GetFirstOrDefault(x => x.SmsContractSignedOn.HasValue) != null;
        return PrefC.GetDateT(PrefName.SmsContractDate).Year > 1880;
    }

    /// <summary>
    ///     Returns 0 if clinics not in use, or patient.ClinicNum if assigned to a clinic, or ClinicNum of the default
    ///     texting clinic.
    /// </summary>
    public static long GetClinicNumForTexting(long patNum)
    {
        if (!true || Clinics.GetCount() == 0) return 0; //0 used for no clinics
        var clinic = Clinics.GetClinic(Patients.GetPat(patNum).ClinicNum); //if patnum invalid will throw unhandled exception.
        if (clinic != null) //if pat assigned to invalid clinic or clinic num 0
            return clinic.Id;
        return PrefC.GetLong(PrefName.TextingDefaultClinicNum);
    }

    /// <summary>
    ///     This method calculates the number of messages will be sent. Given string of text to determine how many with a
    ///     standard charset or unicode text.
    /// </summary>
    public static int CalculateMessagePartsNumber(string text)
    {
        var countHeaderBytes = PrefC.GetInt(PrefName.BytesPerSmsHeader);
        var gsmChars = PrefC.GetString(PrefName.GsmCharSet);
        var gsmExtendedChars = PrefC.GetString(PrefName.GsmExtendedCharSet);
        double bytesPerMessagePart = PrefC.GetInt(PrefName.BytesPerSmsMessagePart);
        return CalculateMessageParts(text, countHeaderBytes, gsmChars, gsmExtendedChars, bytesPerMessagePart);
    }

    public static int CalculateMessageParts(string text, int countHeaderBytes, string gsmChars, string extendedChars, double countBytesPerMessagePart)
    {
        if (text.Length == 0) return 0;
        var countBytesForWholeMessage = GetCountBytesForMessage(text, gsmChars, extendedChars);
        if (countBytesForWholeMessage <= countBytesPerMessagePart) return 1;
        countBytesPerMessagePart -= countHeaderBytes;
        if (countBytesPerMessagePart <= 0) //safe guard in case we try to divide by 0
            return 1;
        var value = countBytesForWholeMessage / countBytesPerMessagePart;
        return (int) Math.Ceiling(value);
    }

    public static double GetCountBytesForMessage(string text, string charsGsm, string charsGsmExtended)
    {
        var messageCharSetType = GetMessageCharSet(text, charsGsm, charsGsmExtended);
        double countBytesForMessage;
        if (messageCharSetType == MessageCharSet.Unicode)
        {
            countBytesForMessage = text.Length * 2;
            return countBytesForMessage;
        }

        double bitsForMessage = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (charsGsm.Contains(text[i]))
            {
                bitsForMessage += 7; //standard gsm char set uses 7 bits
                continue;
            }

            if (charsGsmExtended.Contains(text[i]))
            {
                bitsForMessage += 14; //extended gsm char set uses 14 bits
                continue;
            }

            bitsForMessage += 7; //Couldn't find the correct value but as this method is an estimate, we want to at least add one more character
        }

        countBytesForMessage = Math.Ceiling(bitsForMessage / 8.0);
        return countBytesForMessage;
    }

    public static MessageCharSet GetMessageCharSet(string text, string charsGsm, string charsGsmExtended)
    {
        //text.length should already have been verified in CalculateMessageParts(...)
        for (var i = 0; i < text.Length; i++)
            if (!charsGsm.Contains(text[i]) && !charsGsmExtended.Contains(text[i]))
                //we are not supported by GSM
                return MessageCharSet.Unicode;

        return MessageCharSet.Text;
    }

    #region Cache Pattern

    private class SmsPhoneCache : CacheListAbs<SmsPhone>
    {
        protected override List<SmsPhone> GetCacheFromDb()
        {
            var command = "SELECT * FROM smsphone";
            return SmsPhoneCrud.SelectMany(command);
        }

        protected override List<SmsPhone> TableToList(DataTable dataTable)
        {
            return SmsPhoneCrud.TableToList(dataTable);
        }

        protected override SmsPhone Copy(SmsPhone item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SmsPhone> items)
        {
            return SmsPhoneCrud.ListToTable(items, "SmsPhone");
        }

        protected override void FillCacheIfNeeded()
        {
            SmsPhones.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SmsPhoneCache _smsPhoneCache = new();

    public static List<SmsPhone> GetDeepCopy(bool isShort = false)
    {
        return _smsPhoneCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _smsPhoneCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<SmsPhone> match, bool isShort = false)
    {
        return _smsPhoneCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<SmsPhone> match, bool isShort = false)
    {
        return _smsPhoneCache.GetFindIndex(match, isShort);
    }

    public static SmsPhone GetFirst(bool isShort = false)
    {
        return _smsPhoneCache.GetFirst(isShort);
    }

    public static SmsPhone GetFirst(Func<SmsPhone, bool> match, bool isShort = false)
    {
        return _smsPhoneCache.GetFirst(match, isShort);
    }

    public static SmsPhone GetFirstOrDefault(Func<SmsPhone, bool> match, bool isShort = false)
    {
        return _smsPhoneCache.GetFirstOrDefault(match, isShort);
    }

    public static SmsPhone GetLast(bool isShort = false)
    {
        return _smsPhoneCache.GetLast(isShort);
    }

    public static SmsPhone GetLastOrDefault(Func<SmsPhone, bool> match, bool isShort = false)
    {
        return _smsPhoneCache.GetLastOrDefault(match, isShort);
    }

    public static List<SmsPhone> GetWhere(Predicate<SmsPhone> match, bool isShort = false)
    {
        return _smsPhoneCache.GetWhere(match, isShort);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _smsPhoneCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _smsPhoneCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _smsPhoneCache.ClearCache();
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    #endregion Cache Pattern

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<SmsPhone> Refresh(long patNum){

        string command="SELECT * FROM smsvln WHERE PatNum = "+POut.Long(patNum);
        return Crud.SmsVlnCrud.SelectMany(command);
    }

    ///<summary>Gets one SmsPhone from the db.</summary>
    public static SmsPhone GetOne(long smsVlnNum){

        return Crud.SmsVlnCrud.SelectOne(smsVlnNum);
    }

    
    public static void Delete(long smsVlnNum) {

        string command= "DELETE FROM smsvln WHERE SmsVlnNum = "+POut.Long(smsVlnNum);
        Db.NonQ(command);
    }
    */
}