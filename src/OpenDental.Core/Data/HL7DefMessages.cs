using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class HL7DefMessages
{
    public static List<HL7DefMessage> GetShallowFromDb(long hl7DefNum)
    {
        return HL7DefMessageCrud.SelectMany("SELECT * FROM hl7defmessage WHERE HL7DefNum = " + hl7DefNum + " ORDER BY ItemOrder");
    }

    public static List<HL7DefMessage> GetDeepFromCache(long hl7DefNum)
    {
        var hl7DefMessagesRet = new List<HL7DefMessage>();
        var hl7DefMessages = GetDeepCopy();

        foreach (var hl7DefMessage in hl7DefMessages)
        {
            if (hl7DefMessage.HL7DefNum != hl7DefNum)
            {
                continue;
            }

            hl7DefMessagesRet.Add(hl7DefMessage);
            hl7DefMessagesRet[hl7DefMessagesRet.Count - 1].ListHL7DefSegments = HL7DefSegments.GetDeepFromCache(hl7DefMessage.HL7DefMessageNum);
        }

        return hl7DefMessagesRet;
    }

    public static List<HL7DefMessage> GetDeepFromDb(long hl7DefNum)
    {
        var hl7DefMessages = GetShallowFromDb(hl7DefNum);

        foreach (var hl7DefMessage in hl7DefMessages)
        {
            hl7DefMessage.ListHL7DefSegments = HL7DefSegments.GetDeepFromDb(hl7DefMessage.HL7DefMessageNum);
        }

        return hl7DefMessages;
    }

    public static long Insert(HL7DefMessage hL7DefMessage)
    {
        return HL7DefMessageCrud.Insert(hL7DefMessage);
    }

    public static void Update(HL7DefMessage hL7DefMessage)
    {
        HL7DefMessageCrud.Update(hL7DefMessage);
    }

    public static void Delete(long hL7DefMessageNum)
    {
        Db.NonQ("DELETE FROM hl7defmessage WHERE HL7DefMessageNum = " + hL7DefMessageNum);
    }

    private class HL7DefMessageCache : CacheListAbs<HL7DefMessage>
    {
        protected override List<HL7DefMessage> GetCacheFromDb()
        {
            return HL7DefMessageCrud.SelectMany("SELECT * FROM hl7defmessage ORDER BY ItemOrder");
        }

        protected override List<HL7DefMessage> TableToList(DataTable dataTable)
        {
            return HL7DefMessageCrud.TableToList(dataTable);
        }

        protected override HL7DefMessage Copy(HL7DefMessage item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<HL7DefMessage> items)
        {
            return HL7DefMessageCrud.ListToTable(items, "HL7DefMessage");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly HL7DefMessageCache Cache = new();

    private static List<HL7DefMessage> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
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