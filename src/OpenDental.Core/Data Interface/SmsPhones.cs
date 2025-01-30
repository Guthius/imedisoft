using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class SmsPhones
{
    public enum MessageCharSet
    {
        Text, // 7-bit char set used in text messaging which represents the gsm char set
        Unicode // 16-bit char set used in text messaging
    }

    public const string Shortcode = "SHORTCODE";

    public static void Insert(SmsPhone smsPhone)
    {
        SmsPhoneCrud.Insert(smsPhone);
    }

    public static void Update(SmsPhone smsPhone)
    {
        SmsPhoneCrud.Update(smsPhone);
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
                  + ",SUM(CASE SmsPhoneNumber WHEN '" + SOut.String(Shortcode) + "' THEN 1 ELSE 0 END) FROM smstomobile "
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
            if (tableSmsUsageLocal.Rows[j]["PhoneNumber"].ToString() == strNoActivePhones && SIn.Long(table.Rows[i][4].ToString()) > 0) tableSmsUsageLocal.Rows[j]["PhoneNumber"] = SOut.String(Shortcode); //display "SHORTCODE" as primary number.
            break;
        }

        //Received Month
        command = "SELECT ClinicNum, COUNT(*),SUM(CASE SmsPhoneNumber WHEN '" + SOut.String(Shortcode) + "' THEN 1 ELSE 0 END) FROM smsfrommobile "
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
            if (tableSmsUsageLocal.Rows[j]["PhoneNumber"].ToString() == strNoActivePhones && SIn.Long(table.Rows[i][2].ToString()) > 0) tableSmsUsageLocal.Rows[j]["PhoneNumber"] = SOut.String(Shortcode); //display "SHORTCODE" as primary number.
            break;
        }

        #endregion

        return tableSmsUsageLocal;
    }

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

    public static bool IsIntegratedTextingEnabled()
    {
        if (true) return Clinics.GetFirstOrDefault(x => x.SmsContractSignedOn.HasValue) != null;
        return PrefC.GetDateT(PrefName.SmsContractDate).Year > 1880;
    }

    public static long GetClinicNumForTexting(long patNum)
    {
        if (!true || Clinics.GetCount() == 0) return 0; //0 used for no clinics
        var clinic = Clinics.GetClinic(Patients.GetPat(patNum).ClinicNum); //if patnum invalid will throw unhandled exception.
        if (clinic != null) //if pat assigned to invalid clinic or clinic num 0
            return clinic.Id;
        return PrefC.GetLong(PrefName.TextingDefaultClinicNum);
    }

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

    private static readonly SmsPhoneCache Cache = new();

    public static SmsPhone GetFirstOrDefault(Func<SmsPhone, bool> match, bool isShort = false)
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

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }
}