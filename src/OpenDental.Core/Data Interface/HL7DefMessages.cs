using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class HL7DefMessages
{
    ///<summary>Gets a list of all Messages for this def from the database. No child objects included.</summary>
    public static List<HL7DefMessage> GetShallowFromDb(long hL7DefNum)
    {
        var command = "SELECT * FROM hl7defmessage WHERE HL7DefNum='" + SOut.Long(hL7DefNum) + "' ORDER BY ItemOrder";
        return HL7DefMessageCrud.SelectMany(command);
    }

    ///<summary>Gets a full deep list of all Messages for this def from cache.</summary>
    public static List<HL7DefMessage> GetDeepFromCache(long hL7DefNum)
    {
        var listHL7DefMessagesRet = new List<HL7DefMessage>();
        var listHL7DefMessages = GetDeepCopy();
        for (var i = 0; i < listHL7DefMessages.Count; i++)
            if (listHL7DefMessages[i].HL7DefNum == hL7DefNum)
            {
                listHL7DefMessagesRet.Add(listHL7DefMessages[i]);
                listHL7DefMessagesRet[listHL7DefMessagesRet.Count - 1].ListHL7DefSegments = HL7DefSegments.GetDeepFromCache(listHL7DefMessages[i].HL7DefMessageNum);
            }

        return listHL7DefMessagesRet;
    }

    ///<summary>Gets a full deep list of all Messages for this def from the database.</summary>
    public static List<HL7DefMessage> GetDeepFromDb(long hL7DefNum)
    {
        var listHL7DefMessages = new List<HL7DefMessage>();
        listHL7DefMessages = GetShallowFromDb(hL7DefNum);
        for (var i = 0; i < listHL7DefMessages.Count; i++) listHL7DefMessages[i].ListHL7DefSegments = HL7DefSegments.GetDeepFromDb(listHL7DefMessages[i].HL7DefMessageNum);
        return listHL7DefMessages;
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
        var command = "DELETE FROM hl7defmessage WHERE HL7DefMessageNum = " + SOut.Long(hL7DefMessageNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class HL7DefMessageCache : CacheListAbs<HL7DefMessage>
    {
        protected override List<HL7DefMessage> GetCacheFromDb()
        {
            var command = "SELECT * FROM hl7defmessage ORDER BY ItemOrder";
            return HL7DefMessageCrud.SelectMany(command);
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
            HL7DefMessages.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly HL7DefMessageCache _HL7DefMessageCache = new();

    private static List<HL7DefMessage> GetDeepCopy(bool isShort = false)
    {
        return _HL7DefMessageCache.GetDeepCopy(isShort);
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
        _HL7DefMessageCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _HL7DefMessageCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _HL7DefMessageCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<HL7DefMessage> Refresh(long patNum){

        string command="SELECT * FROM hl7defmessage WHERE PatNum = "+POut.Long(patNum);
        return Crud.HL7DefMessageCrud.SelectMany(command);
    }

    ///<summary>Gets one HL7DefMessage from the db.</summary>
    public static HL7DefMessage GetOne(long hL7DefMessageNum){

        return Crud.HL7DefMessageCrud.SelectOne(hL7DefMessageNum);
    }

    */
}