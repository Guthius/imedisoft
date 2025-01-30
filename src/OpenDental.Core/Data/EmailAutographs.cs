using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EmailAutographs
{
    public static EmailAutograph GetForOutgoing(List<EmailAutograph> emailAutographs, EmailAddress emailAddressOutgoing)
    {
        var emailUsername = EmailMessages.GetAddressSimple(emailAddressOutgoing.EmailUsername);
        var emailSender = EmailMessages.GetAddressSimple(emailAddressOutgoing.SenderAddress);

        foreach (var emailAutograph in emailAutographs)
        {
            var autographEmail = EmailMessages.GetAddressSimple(emailAutograph.EmailAddress.Trim());

            if ((!string.IsNullOrWhiteSpace(emailUsername) && autographEmail.Contains(emailUsername)) ||
                (!string.IsNullOrWhiteSpace(emailSender) && autographEmail.Contains(emailSender)))
            {
                return emailAutograph;
            }
        }

        return null;
    }

    public static void Insert(EmailAutograph emailAutograph)
    {
        EmailAutographCrud.Insert(emailAutograph);
    }

    public static void Update(EmailAutograph emailAutograph)
    {
        EmailAutographCrud.Update(emailAutograph);
    }

    public static void Delete(long emailAutographNum)
    {
        EmailAutographCrud.Delete(emailAutographNum);
    }

    private class EmailAutographCache : CacheListAbs<EmailAutograph>
    {
        protected override List<EmailAutograph> GetCacheFromDb()
        {
            return EmailAutographCrud.SelectMany("SELECT * FROM emailautograph ORDER BY Description");
        }

        protected override List<EmailAutograph> TableToList(DataTable dataTable)
        {
            return EmailAutographCrud.TableToList(dataTable);
        }

        protected override EmailAutograph Copy(EmailAutograph item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<EmailAutograph> items)
        {
            return EmailAutographCrud.ListToTable(items, "EmailAutograph");
        }

        protected override void FillCacheIfNeeded()
        {
            EmailAutographs.GetTableFromCache(false);
        }
    }

    private static readonly EmailAutographCache Cache = new();

    public static List<EmailAutograph> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
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