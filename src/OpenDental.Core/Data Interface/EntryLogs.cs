using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EntryLogs
{
    #region Get Methods

    ///<summary>Gets one EntryLog from the db.</summary>
    public static EntryLog GetOne(long entryLogNum)
    {
        return EntryLogCrud.SelectOne(entryLogNum);
    }

    #endregion

    #region Insert

    
    public static long Insert(EntryLog entryLog)
    {
        return EntryLogCrud.Insert(entryLog);
    }

    public static void InsertMany(List<EntryLog> listEntryLogs)
    {
        EntryLogCrud.InsertMany(listEntryLogs);
    }

    #endregion
}