using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>emailtemplates are refreshed as local data.</summary>
public class EmailTemplates
{
    
    public static long Insert(EmailTemplate emailTemplate)
    {
        return EmailTemplateCrud.Insert(emailTemplate);
    }

    
    public static void Update(EmailTemplate emailTemplate)
    {
        EmailTemplateCrud.Update(emailTemplate);
    }

    
    public static void Delete(EmailTemplate emailTemplate)
    {
        var command = "DELETE from emailtemplate WHERE EmailTemplateNum = '" + emailTemplate.EmailTemplateNum + "'";
        Db.NonQ(command);
    }

    #region CachePattern

    private class EmailTemplateCache : CacheListAbs<EmailTemplate>
    {
        protected override List<EmailTemplate> GetCacheFromDb()
        {
            var command = "SELECT * from emailtemplate ORDER BY Description";
            return EmailTemplateCrud.SelectMany(command);
        }

        protected override List<EmailTemplate> TableToList(DataTable dataTable)
        {
            return EmailTemplateCrud.TableToList(dataTable);
        }

        protected override EmailTemplate Copy(EmailTemplate item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<EmailTemplate> items)
        {
            return EmailTemplateCrud.ListToTable(items, "EmailTemplate");
        }

        protected override void FillCacheIfNeeded()
        {
            EmailTemplates.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly EmailTemplateCache _emailTemplateCache = new();

    public static List<EmailTemplate> GetDeepCopy(bool isShort = false)
    {
        return _emailTemplateCache.GetDeepCopy(isShort);
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
        _emailTemplateCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _emailTemplateCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _emailTemplateCache.ClearCache();
    }

    #endregion
}