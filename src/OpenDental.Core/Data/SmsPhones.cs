using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class SmsPhones
{
    public enum MessageCharSet
    {
        Text,
        Unicode
    }

    public static double GetClinicBalance(long clinicNum)
    {
        double limit = 0;

        if (clinicNum == 0 && Clinics.GetCount(true) > 0)
        {
            clinicNum = Clinics.GetFirst(true).Id;
        }

        var clinic = Clinics.GetClinic(clinicNum);
        if (clinic is {SmsContractSignedOn: not null})
        {
            limit = clinic.SmsMonthlyLimit;
        }

        var dateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var dateEnd = dateStart.AddMonths(1);

        var command = "SELECT SUM(MsgChargeUSD) FROM smstomobile WHERE ClinicNum=" + (clinicNum) + " AND DateTimeSent>=" + SOut.Date(dateStart) + " AND DateTimeSent<" + SOut.Date(dateEnd);

        limit -= SIn.Double(DataCore.GetScalar(command));

        return limit;
    }

    public static bool IsIntegratedTextingEnabled()
    {
        return Clinics.GetFirstOrDefault(x => x.SmsContractSignedOn.HasValue) != null;
    }

    public static long GetClinicNumForTexting(long patNum)
    {
        if (Clinics.GetCount() == 0)
        {
            return 0;
        }

        var clinic = Clinics.GetClinic(Patients.GetPat(patNum).ClinicNum);

        return clinic?.Id ?? PrefC.GetLong(PrefName.TextingDefaultClinicNum);
    }

    public static int CalculateMessagePartsNumber(string text)
    {
        var countHeaderBytes = PrefC.GetInt(PrefName.BytesPerSmsHeader);

        var gsmChars = PrefC.GetString(PrefName.GsmCharSet);
        var gsmExtendedChars = PrefC.GetString(PrefName.GsmExtendedCharSet);

        var bytesPerMessagePart = PrefC.GetInt(PrefName.BytesPerSmsMessagePart);

        return CalculateMessageParts(text, countHeaderBytes, gsmChars, gsmExtendedChars, bytesPerMessagePart);
    }

    public static int CalculateMessageParts(string text, int countHeaderBytes, string gsmChars, string extendedChars, double countBytesPerMessagePart)
    {
        if (text.Length == 0)
        {
            return 0;
        }

        var countBytesForWholeMessage = GetCountBytesForMessage(text, gsmChars, extendedChars);
        if (countBytesForWholeMessage <= countBytesPerMessagePart)
        {
            return 1;
        }

        countBytesPerMessagePart -= countHeaderBytes;
        if (countBytesPerMessagePart <= 0)
        {
            return 1;
        }

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
        foreach (var ch in text)
        {
            if (charsGsm.Contains(ch))
            {
                bitsForMessage += 7;
                continue;
            }

            if (charsGsmExtended.Contains(ch))
            {
                bitsForMessage += 14;
                continue;
            }

            bitsForMessage += 7;
        }

        countBytesForMessage = Math.Ceiling(bitsForMessage / 8.0);

        return countBytesForMessage;
    }

    public static MessageCharSet GetMessageCharSet(string text, string charsGsm, string charsGsmExtended)
    {
        foreach (var ch in text)
        {
            if (!charsGsm.Contains(ch) && !charsGsmExtended.Contains(ch))
            {
                return MessageCharSet.Unicode;
            }
        }

        return MessageCharSet.Text;
    }

    private class SmsPhoneCache : CacheListAbs<SmsPhone>
    {
        protected override List<SmsPhone> GetCacheFromDb()
        {
            return SmsPhoneCrud.SelectMany("SELECT * FROM smsphone");
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
            GetTableFromCache(false);
        }
    }

    private static readonly SmsPhoneCache Cache = new();

    public static SmsPhone GetFirstOrDefault(Func<SmsPhone, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}