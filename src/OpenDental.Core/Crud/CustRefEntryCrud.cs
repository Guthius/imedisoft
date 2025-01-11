using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CustRefEntryCrud
{
    public static List<CustRefEntry> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CustRefEntry> TableToList(DataTable table)
    {
        var retVal = new List<CustRefEntry>();
        CustRefEntry custRefEntry;
        foreach (DataRow row in table.Rows)
        {
            custRefEntry = new CustRefEntry();
            custRefEntry.CustRefEntryNum = SIn.Long(row["CustRefEntryNum"].ToString());
            custRefEntry.PatNumCust = SIn.Long(row["PatNumCust"].ToString());
            custRefEntry.PatNumRef = SIn.Long(row["PatNumRef"].ToString());
            custRefEntry.DateEntry = SIn.Date(row["DateEntry"].ToString());
            custRefEntry.Note = SIn.String(row["Note"].ToString());
            retVal.Add(custRefEntry);
        }

        return retVal;
    }

    public static long Insert(CustRefEntry custRefEntry)
    {
        var command = "INSERT INTO custrefentry (";

        command += "PatNumCust,PatNumRef,DateEntry,Note) VALUES(";

        command +=
            SOut.Long(custRefEntry.PatNumCust) + ","
                                               + SOut.Long(custRefEntry.PatNumRef) + ","
                                               + SOut.Date(custRefEntry.DateEntry) + ","
                                               + "'" + SOut.String(custRefEntry.Note) + "')";
        {
            custRefEntry.CustRefEntryNum = Db.NonQ(command, true, "CustRefEntryNum", "custRefEntry");
        }
        return custRefEntry.CustRefEntryNum;
    }

    public static void Update(CustRefEntry custRefEntry)
    {
        var command = "UPDATE custrefentry SET "
                      + "PatNumCust     =  " + SOut.Long(custRefEntry.PatNumCust) + ", "
                      + "PatNumRef      =  " + SOut.Long(custRefEntry.PatNumRef) + ", "
                      + "DateEntry      =  " + SOut.Date(custRefEntry.DateEntry) + ", "
                      + "Note           = '" + SOut.String(custRefEntry.Note) + "' "
                      + "WHERE CustRefEntryNum = " + SOut.Long(custRefEntry.CustRefEntryNum);
        Db.NonQ(command);
    }
}