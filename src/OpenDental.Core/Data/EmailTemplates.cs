using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EmailTemplates
{
    public static void Insert(EmailTemplate emailTemplate)
    {
        EmailTemplateCrud.Insert(emailTemplate);
    }

    public static void Update(EmailTemplate emailTemplate)
    {
        EmailTemplateCrud.Update(emailTemplate);
    }

    public static void Delete(EmailTemplate emailTemplate)
    {
        Db.NonQ("DELETE from emailtemplate WHERE EmailTemplateNum = " + emailTemplate.EmailTemplateNum);
    }

    private class EmailTemplateCache : CacheListAbs<EmailTemplate>
    {
        protected override List<EmailTemplate> GetCacheFromDb()
        {
            return EmailTemplateCrud.SelectMany("SELECT * from emailtemplate ORDER BY Description");
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

    private static readonly EmailTemplateCache Cache = new();

    public static List<EmailTemplate> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
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