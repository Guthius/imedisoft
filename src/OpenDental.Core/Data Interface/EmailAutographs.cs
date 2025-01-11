using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EmailAutographs
{
    ///<summary>Searches the cache of EmailAutographs and returns the first match, otherwise null.</summary>
    public static EmailAutograph GetForOutgoing(List<EmailAutograph> listEmailAutographs, EmailAddress emailAddressOutgoing)
    {
        var emailUsername = EmailMessages.GetAddressSimple(emailAddressOutgoing.EmailUsername);
        var emailSender = EmailMessages.GetAddressSimple(emailAddressOutgoing.SenderAddress);
        string autographEmail;
        for (var i = 0; i < listEmailAutographs.Count; i++)
        {
            autographEmail = EmailMessages.GetAddressSimple(listEmailAutographs[i].EmailAddress.Trim());
            //Use Contains() because an autograph can theoretically have multiple email addresses associated with it.
            if ((!string.IsNullOrWhiteSpace(emailUsername) && autographEmail.Contains(emailUsername))
                || (!string.IsNullOrWhiteSpace(emailSender) && autographEmail.Contains(emailSender)))
                return listEmailAutographs[i];
        }

        return null;
    }

    /// <summary>
    ///     Gets the first autograph that matches the EmailAddress.SenderAddress if it has one, otherwise
    ///     the first that matches EmailAddress.EmailUserName. Returns null if neither yield a match.
    /// </summary>
    public static EmailAutograph GetFirstOrDefaultForEmailAddress(EmailAddress emailAddress)
    {
        var addressToMatch = emailAddress.GetFrom();
        return GetFirstOrDefault(x => emailAddress.GetFrom() == x.EmailAddress);
    }

    /////<summary>Gets one EmailAutograph from the db.</summary>
    //public static EmailAutograph GetOne(long emailAutographNum){
    //	
    //	return Crud.EmailAutographCrud.SelectOne(emailAutographNum);
    //}

    ///<summary>Insert one EmailAutograph in the database.</summary>
    public static long Insert(EmailAutograph emailAutograph)
    {
        return EmailAutographCrud.Insert(emailAutograph);
    }

    ///<summary>Updates an existing EmailAutograph in the database.</summary>
    public static void Update(EmailAutograph emailAutograph)
    {
        EmailAutographCrud.Update(emailAutograph);
    }

    ///<summary>Delete on EmailAutograph from the database.</summary>
    public static void Delete(long emailAutographNum)
    {
        EmailAutographCrud.Delete(emailAutographNum);
    }

    #region CachePattern

    private class EmailAutographCache : CacheListAbs<EmailAutograph>
    {
        protected override List<EmailAutograph> GetCacheFromDb()
        {
            var command = "SELECT * FROM emailautograph ORDER BY " + DbHelper.ClobOrderBy("Description");
            return EmailAutographCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly EmailAutographCache _emailAutographCache = new();

    public static List<EmailAutograph> GetDeepCopy(bool isShort = false)
    {
        return _emailAutographCache.GetDeepCopy(isShort);
    }

    public static EmailAutograph GetFirstOrDefault(Func<EmailAutograph, bool> match, bool isShort = false)
    {
        return _emailAutographCache.GetFirstOrDefault(match, isShort);
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
        _emailAutographCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _emailAutographCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _emailAutographCache.ClearCache();
    }

    #endregion
}