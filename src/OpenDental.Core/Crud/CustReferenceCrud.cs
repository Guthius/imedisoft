using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CustReferenceCrud
{
    public static CustReference SelectOne(long custReferenceNum)
    {
        var command = "SELECT * FROM custreference "
                      + "WHERE CustReferenceNum = " + SOut.Long(custReferenceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<CustReference> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CustReference> TableToList(DataTable table)
    {
        var retVal = new List<CustReference>();
        CustReference custReference;
        foreach (DataRow row in table.Rows)
        {
            custReference = new CustReference();
            custReference.CustReferenceNum = SIn.Long(row["CustReferenceNum"].ToString());
            custReference.PatNum = SIn.Long(row["PatNum"].ToString());
            custReference.DateMostRecent = SIn.Date(row["DateMostRecent"].ToString());
            custReference.Note = SIn.String(row["Note"].ToString());
            custReference.IsBadRef = SIn.Bool(row["IsBadRef"].ToString());
            retVal.Add(custReference);
        }

        return retVal;
    }

    public static long Insert(CustReference custReference)
    {
        var command = "INSERT INTO custreference (";

        command += "PatNum,DateMostRecent,Note,IsBadRef) VALUES(";

        command +=
            SOut.Long(custReference.PatNum) + ","
                                            + SOut.Date(custReference.DateMostRecent) + ","
                                            + "'" + SOut.String(custReference.Note) + "',"
                                            + SOut.Bool(custReference.IsBadRef) + ")";
        {
            custReference.CustReferenceNum = Db.NonQ(command, true, "CustReferenceNum", "custReference");
        }
        return custReference.CustReferenceNum;
    }

    public static void Update(CustReference custReference)
    {
        var command = "UPDATE custreference SET "
                      + "PatNum          =  " + SOut.Long(custReference.PatNum) + ", "
                      + "DateMostRecent  =  " + SOut.Date(custReference.DateMostRecent) + ", "
                      + "Note            = '" + SOut.String(custReference.Note) + "', "
                      + "IsBadRef        =  " + SOut.Bool(custReference.IsBadRef) + " "
                      + "WHERE CustReferenceNum = " + SOut.Long(custReference.CustReferenceNum);
        Db.NonQ(command);
    }
}