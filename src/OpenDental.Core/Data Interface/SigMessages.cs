using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SigMessages
{
    ///<summary>Gets one SigMessage from the db.</summary>
    public static List<SigMessage> GetSigMessages(List<long> listSigMessageNums)
    {
        if (listSigMessageNums == null || listSigMessageNums.Count < 1) return new List<SigMessage>();
        var command = "SELECT * FROM sigmessage WHERE SigMessageNum IN (" + string.Join(",", listSigMessageNums) + ")";
        return SigMessageCrud.SelectMany(command);
    }

    ///<summary>Gets one SigMessage from the db.</summary>
    public static List<SigMessage> GetAll()
    {
        var command = "SELECT * FROM sigmessage";
        return SigMessageCrud.SelectMany(command);
    }

    /// <summary>
    ///     Only used when starting up to get the current button state.  Only gets unacked messages.
    ///     There may well be extra and useless messages included.  But only the lights will be used anyway, so it doesn't
    ///     matter.
    /// </summary>
    public static List<SigMessage> RefreshCurrentButState()
    {
        var listSigMessages = new List<SigMessage>();
        var command = @"SELECT * FROM sigmessage "
                      + "WHERE AckDateTime < " + SOut.DateT(new DateTime(1880, 1, 1)) + " "
                      + "ORDER BY MessageDateTime";
        listSigMessages = SigMessageCrud.SelectMany(command);
        listSigMessages.Sort();
        return listSigMessages;
    }

    ///<summary>Includes all messages, whether acked or not.  It's up to the UI to filter out acked if necessary.</summary>
    public static List<SigMessage> GetSigMessagesSinceDateTime(DateTime dateTimeSince)
    {
        var listSigMessages = new List<SigMessage>();
        var command = "SELECT * FROM sigmessage "
                      + "WHERE (MessageDateTime > " + SOut.DateT(dateTimeSince) + " "
                      + "OR AckDateTime > " + SOut.DateT(dateTimeSince) + " "
                      + "OR AckDateTime < " + SOut.Date(new DateTime(1880, 1, 1), true) + ") " //always include all unacked.
                      + "ORDER BY MessageDateTime";
        //note: this might return an occasional row that has both times newer.
        listSigMessages = SigMessageCrud.SelectMany(command);
        listSigMessages.Sort();
        return listSigMessages;
    }

    /// <summary>
    ///     When user clicks on a colored light, they intend to ack it to turn it off.  This acks all sigmessages with the
    ///     specified index.
    ///     This is in case multiple sigmessages have been created from different workstations.  This acks them all in one
    ///     shot.
    ///     Must specify a time because you only want to ack sigmessages earlier than the last time this workstation was
    ///     refreshed.
    ///     A newer sigmessage would not get acked. If this seems slow, then I will need to check to make sure all these tables
    ///     are properly indexed.
    ///     Inserts a signal for every SigMessageNum that was updated.
    /// </summary>
    public static void AckButton(int buttonIndex, DateTime dateTime)
    {
        var listSigMessageNums = new List<long>();
        var command = "SELECT DISTINCT sigmessage.SigMessageNum FROM sigmessage "
                      + "INNER JOIN sigelementdef ON (sigmessage.SigElementDefNumUser=sigelementdef.SigElementDefNum "
                      + "OR sigmessage.SigElementDefNumExtra=sigelementdef.SigElementDefNum "
                      + "OR sigmessage.SigElementDefNumMsg=sigelementdef.SigElementDefNum) "
                      + "WHERE sigmessage.AckDateTime < " + SOut.Date(new DateTime(1880, 1, 1), true) + " "
                      + "AND MessageDateTime <= " + SOut.DateT(dateTime) + " "
                      + "AND sigelementdef.LightRow=" + SOut.Long(buttonIndex);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return;
        listSigMessageNums = table.Select().Select(x => SIn.Long(x["SigMessageNum"].ToString())).ToList();
        command = "UPDATE sigmessage SET AckDateTime = " + DbHelper.Now() + " "
                  + "WHERE SigMessageNum IN (" + string.Join(",", listSigMessageNums) + ")";
        Db.NonQ(command);
        listSigMessageNums.ForEach(x => Signalods.SetInvalid(InvalidType.SigMessages, KeyType.SigMessage, x));
    }

    ///<summary>Acknowledge one sig message from the manage module grid.</summary>
    public static void AckSigMessage(SigMessage sigMessage)
    {
        //To ack a message, simply update the AckDateTime on the original row.
        sigMessage.AckDateTime = MiscData.GetNowDateTime();
        Update(sigMessage);
    }

    
    public static long Insert(SigMessage sigMessage)
    {
        return SigMessageCrud.Insert(sigMessage);
    }

    
    public static void Update(SigMessage sigMessage)
    {
        SigMessageCrud.Update(sigMessage);
    }

    ///<summary>Deletes all sigmessages older than 2 days.</summary>
    public static void ClearOldSigMessages()
    {
        //Get all ack'd messages older than two days.
        var command = "";
        //easier to read than using the DbHelper Functions
        command = "SELECT SigMessageNum FROM sigmessage WHERE AckDateTime > " + SOut.DateT(new DateTime(1880, 1, 1)) + " "
                  + "AND AckDateTime < DATE_ADD(NOW(),INTERVAL -2 DAY)";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count < 1) return; //Nothing to delete.
        //Delete all of the acks.
        command = "DELETE FROM sigmessage "
                  + "WHERE SigMessageNum IN (" + string.Join(",", table.Select().Select(x => SIn.Long(x["SigMessageNum"].ToString()))) + ")";
        Db.NonQ(command);
    }
}

//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
/*
#region CachePattern

        private class SigMessageCache : CacheListAbs<SigMessage> {
            protected override List<SigMessage> GetCacheFromDb() {
                string command="SELECT * FROM SigMessage ORDER BY ItemOrder";
                return Crud.SigMessageCrud.SelectMany(command);
            }
            protected override List<SigMessage> TableToList(DataTable table) {
                return Crud.SigMessageCrud.TableToList(table);
            }
            protected override SigMessage Copy(SigMessage SigMessage) {
                return SigMessage.Clone();
            }
            protected override DataTable ListToTable(List<SigMessage> listSigMessages) {
                return Crud.SigMessageCrud.ListToTable(listSigMessages,"SigMessage");
            }
            protected override void FillCacheIfNeeded() {
                SigMessages.GetTableFromCache(false);
            }
            protected override bool IsInListShort(SigMessage SigMessage) {
                return !SigMessage.IsHidden;
            }
        }

        ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
        private static SigMessageCache _SigMessageCache=new SigMessageCache();

        ///<summary>A list of all SigMessages. Returns a deep copy.</summary>
        public static List<SigMessage> ListDeep {
            get {
                return _SigMessageCache.ListDeep;
            }
        }

        ///<summary>A list of all visible SigMessages. Returns a deep copy.</summary>
        public static List<SigMessage> ListShortDeep {
            get {
                return _SigMessageCache.ListShortDeep;
            }
        }

        ///<summary>A list of all SigMessages. Returns a shallow copy.</summary>
        public static List<SigMessage> ListShallow {
            get {
                return _SigMessageCache.ListShallow;
            }
        }

        ///<summary>A list of all visible SigMessages. Returns a shallow copy.</summary>
        public static List<SigMessage> ListShort {
            get {
                return _SigMessageCache.ListShallowShort;
            }
        }

        ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
        public static DataTable RefreshCache() {
            return GetTableFromCache(true);
        }

        ///<summary>Fills the local cache with the passed in DataTable.</summary>
        public static void FillCacheFromTable(DataTable table) {
            _SigMessageCache.FillCacheFromTable(table);
        }

        ///<summary>Always refreshes the ClientWeb's cache.</summary>
        public static DataTable GetTableFromCache(bool doRefreshCache) {

            return _SigMessageCache.GetTableFromCache(doRefreshCache);
        }

        #endregion
*/
/*
Only pull out the methods below as you need them.  Otherwise, leave them commented out.


public static List<SigMessage> Refresh(long patNum){

    string command="SELECT * FROM sigmessage WHERE PatNum = "+POut.Long(patNum);
    return Crud.SigMessageCrud.SelectMany(command);
}

///<summary>Gets one SigMessage from the db.</summary>
public static SigMessage GetOne(long sigMessageNum){

    return Crud.SigMessageCrud.SelectOne(sigMessageNum);
}


public static void Delete(long sigMessageNum) {

    Crud.SigMessageCrud.Delete(sigMessageNum);
}




*/