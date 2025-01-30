using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class EmailAddresses
{
    public static readonly char[] AddressDelimiters = [';', ','];

    public static EmailAddress GetByClinic(long clinicNum, bool allowNullReturn = false)
    {
        EmailAddress emailAddress;

        var clinic = Clinics.GetClinic(clinicNum);
        if (clinic is null)
        {
            emailAddress = GetOne(PrefC.GetLong(PrefName.EmailDefaultAddressNum));
        }
        else
        {
            emailAddress = GetOne(clinic.EmailAddressId ?? 0) ?? GetOne(PrefC.GetLong(PrefName.EmailDefaultAddressNum));
        }

        if (emailAddress != null)
        {
            return emailAddress;
        }

        emailAddress = GetFirstOrDefault(x => x.UserNum == 0);
        if (emailAddress != null)
        {
            return emailAddress;
        }

        if (allowNullReturn)
        {
            return null;
        }

        emailAddress = new EmailAddress
        {
            EmailPassword = "",
            EmailUsername = "",
            Pop3ServerIncoming = "",
            SenderAddress = "",
            SMTPserver = ""
        };

        return emailAddress;
    }

    public static EmailAddress GetForUserDb(long userNum)
    {
        return EmailAddressCrud.SelectOne("SELECT * FROM emailaddress WHERE emailaddress.UserNum = " + userNum);
    }

    public static EmailAddress GetNewEmailDefault(long userNum, long clinicNum)
    {
        var emailAddress = GetForUserDb(userNum);

        return emailAddress ?? GetByClinic(clinicNum);
    }

    public static EmailAddress GetOne(long emailAddressNum)
    {
        return GetFirstOrDefault(x => x.EmailAddressNum == emailAddressNum);
    }

    public static EmailAddress GetOneFromDb(long emailAddressNum)
    {
        return EmailAddressCrud.SelectOne(emailAddressNum);
    }

    public static EmailAddress OverrideSenderAddressClinical(EmailAddress emailAddress, long clinicNum)
    {
        var clinic = Clinics.GetClinic(clinicNum);
        if (clinic is null)
        {
            return emailAddress;
        }

        if (clinic.EmailAliasOverride == "")
        {
            return emailAddress;
        }

        if (!Regex.IsMatch(emailAddress.SenderAddress, "^[^<>]+$"))
        {
            return emailAddress;
        }

        emailAddress.SenderAddress = clinic.EmailAliasOverride + " <" + emailAddress.SenderAddress + ">";

        return emailAddress;
    }

    public static bool AddressExists(string emailUserName, long emailAddressNumSkip = 0)
    {
        var emailAddresses = GetWhere(x =>
            x.UserNum == 0 &&
            x.EmailAddressNum != emailAddressNumSkip &&
            string.Equals(x.EmailUsername.Trim(), emailUserName.Trim(),
                StringComparison.CurrentCultureIgnoreCase));

        return emailAddresses.Count > 0;
    }

    public static bool ExistsValidEmail()
    {
        var emailAddress = GetFirstOrDefault(x => x.UserNum == 0 && x.SMTPserver != "");
        return emailAddress != null;
    }

    public static MailAddress GetValidMailAddress(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress)) return null;
        try
        {
            emailAddress = Regex.Replace(emailAddress, @"(@)(.+)$", MatchEvaluatorDomain, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }
        catch (Exception)
        {
            return null;
        }

        emailAddress = emailAddress.Trim().ToLower();

        MailAddress mailAddress;
        try
        {
            mailAddress = new MailAddress(emailAddress);
        }
        catch
        {
            return null;
        }

        if (!HasValidChars(mailAddress.Address))
        {
            return null;
        }

        return Regex.IsMatch(mailAddress.Address,
            """^(?(")(".+?(?<!\\)"@)|(([0-9a-z_]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z_])@))""" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100))
            ? mailAddress
            : null;
    }

    private static string MatchEvaluatorDomain(Match match)
    {
        //Per a previous summary comment, the following DomainMapper and RegularExpresion "Comes from Microsoft itself".
        //Use IdnMapping class to convert Unicode domain names.
        var idnMapping = new IdnMapping();
        //Pull out and process domain name (throws ArgumentException on invalid)
        var domainNormalized = idnMapping.GetAscii(match.Groups[2].Value);
        return match.Groups[1].Value + domainNormalized;
    }

    public static List<EmailAddress> GetEmailAddressesForComboBoxes(long userNum)
    {
        var emailAddressNumsToExclude = new List<long>();

        emailAddressNumsToExclude.AddRange(Clinics.GetDeepCopy().Select(x => x.EmailAddressId ?? 0).Distinct());
        emailAddressNumsToExclude.Add(PrefC.GetLong(PrefName.EmailDefaultAddressNum));
        emailAddressNumsToExclude.Add(PrefC.GetLong(PrefName.EmailNotifyAddressNum));

        var emailAddresses = new List<EmailAddress>
        {
            new()
            {
                EmailUsername = "Practice/Clinic"
            }
        };

        emailAddresses.AddRange(
            GetWhere(x => !emailAddressNumsToExclude.Contains(x.EmailAddressNum) && (x.UserNum == 0 || x.UserNum == userNum))
                .OrderByDescending(x => x.UserNum == userNum));
        
        return emailAddresses;
    }

    public static string GetDisplayStringForComboBox(EmailAddress emailAddress, long userNum, long emailAddressNumDefault = 0)
    {
        if (emailAddressNumDefault != 0 && emailAddress.EmailAddressNum == emailAddressNumDefault)
        {
            return "Default <" + emailAddress.EmailUsername + ">";
        }
        
        if (userNum == emailAddress.UserNum)
        {
            return "Me <" + emailAddress.EmailUsername + ">";
        }
        
        return emailAddress.EmailUsername;
    }

    public static bool HasValidChars(string emailAddress)
    {
        var bytes = Encoding.ASCII.GetBytes(emailAddress);
        
        var addressAfterEncoding = Encoding.ASCII.GetString(bytes);
        
        return string.Compare(emailAddress, addressAfterEncoding, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static List<string> GetValidAddresses(string emailAddresses)
    {
        emailAddresses ??= "";

        var emails = emailAddresses
            .Split(AddressDelimiters, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLower())
            .ToList();

        var results = new List<string>();
        for (var i = 0; i < emails.Count(); i++)
        {
            var mailAddress = GetValidMailAddress(emails[i]);
            if (mailAddress is null)
            {
                continue;
            }

            if (results.Contains(mailAddress.Address))
            {
                continue;
            }

            results.Add(mailAddress.Address);
        }

        return results;
    }

    public static void Insert(EmailAddress emailAddress)
    {
        EmailAddressCrud.Insert(emailAddress);
    }

    public static void Update(EmailAddress emailAddress)
    {
        EmailAddressCrud.Update(emailAddress);
    }

    public static void Delete(long emailAddressNum)
    {
        Db.NonQ("DELETE FROM emailaddress WHERE EmailAddressNum = " + emailAddressNum);
    }

    private class EmailAddressCache : CacheListAbs<EmailAddress>
    {
        protected override List<EmailAddress> GetCacheFromDb()
        {
            return EmailAddressCrud.SelectMany("SELECT * FROM emailaddress ORDER BY EmailUsername");
        }

        protected override List<EmailAddress> TableToList(DataTable dataTable)
        {
            return EmailAddressCrud.TableToList(dataTable);
        }

        protected override EmailAddress Copy(EmailAddress item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<EmailAddress> items)
        {
            return EmailAddressCrud.ListToTable(items, "EmailAddress");
        }

        protected override void FillCacheIfNeeded()
        {
            EmailAddresses.GetTableFromCache(false);
        }
    }

    private static readonly EmailAddressCache Cache = new();

    public static List<EmailAddress> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static EmailAddress GetFirstOrDefault(Func<EmailAddress, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static List<EmailAddress> GetWhere(Predicate<EmailAddress> match, bool isShort = false)
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