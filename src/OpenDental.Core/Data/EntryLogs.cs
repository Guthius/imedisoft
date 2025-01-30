using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class EntryLogs
{
    public static void Insert(EntryLog entryLog)
    {
        EntryLogCrud.Insert(entryLog);
    }

    public static void InsertMany(List<EntryLog> listEntryLogs)
    {
        EntryLogCrud.InsertMany(listEntryLogs);
    }
}