#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PearlRequestCrud
{
    public static PearlRequest SelectOne(long pearlRequestNum)
    {
        var command = "SELECT * FROM pearlrequest "
                      + "WHERE PearlRequestNum = " + SOut.Long(pearlRequestNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PearlRequest SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PearlRequest> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PearlRequest> TableToList(DataTable table)
    {
        var retVal = new List<PearlRequest>();
        PearlRequest pearlRequest;
        foreach (DataRow row in table.Rows)
        {
            pearlRequest = new PearlRequest();
            pearlRequest.PearlRequestNum = SIn.Long(row["PearlRequestNum"].ToString());
            pearlRequest.RequestId = SIn.String(row["RequestId"].ToString());
            pearlRequest.DocNum = SIn.Long(row["DocNum"].ToString());
            pearlRequest.RequestStatus = (EnumPearlStatus) SIn.Int(row["RequestStatus"].ToString());
            pearlRequest.DateTSent = SIn.Date(row["DateTSent"].ToString());
            pearlRequest.DateTChecked = SIn.Date(row["DateTChecked"].ToString());
            retVal.Add(pearlRequest);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PearlRequest> listPearlRequests, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PearlRequest";
        var table = new DataTable(tableName);
        table.Columns.Add("PearlRequestNum");
        table.Columns.Add("RequestId");
        table.Columns.Add("DocNum");
        table.Columns.Add("RequestStatus");
        table.Columns.Add("DateTSent");
        table.Columns.Add("DateTChecked");
        foreach (var pearlRequest in listPearlRequests)
            table.Rows.Add(SOut.Long(pearlRequest.PearlRequestNum), pearlRequest.RequestId, SOut.Long(pearlRequest.DocNum), SOut.Int((int) pearlRequest.RequestStatus), SOut.DateT(pearlRequest.DateTSent, false), SOut.DateT(pearlRequest.DateTChecked, false));
        return table;
    }

    public static long Insert(PearlRequest pearlRequest)
    {
        return Insert(pearlRequest, false);
    }

    public static long Insert(PearlRequest pearlRequest, bool useExistingPK)
    {
        var command = "INSERT INTO pearlrequest (";

        command += "RequestId,DocNum,RequestStatus,DateTSent,DateTChecked) VALUES(";

        command +=
            "'" + SOut.String(pearlRequest.RequestId) + "',"
            + SOut.Long(pearlRequest.DocNum) + ","
            + SOut.Int((int) pearlRequest.RequestStatus) + ","
            + SOut.Date(pearlRequest.DateTSent) + ","
            + SOut.Date(pearlRequest.DateTChecked) + ")";
        {
            pearlRequest.PearlRequestNum = Db.NonQ(command, true, "PearlRequestNum", "pearlRequest");
        }
        return pearlRequest.PearlRequestNum;
    }

    public static long InsertNoCache(PearlRequest pearlRequest)
    {
        return InsertNoCache(pearlRequest, false);
    }

    public static long InsertNoCache(PearlRequest pearlRequest, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO pearlrequest (";
        if (isRandomKeys || useExistingPK) command += "PearlRequestNum,";
        command += "RequestId,DocNum,RequestStatus,DateTSent,DateTChecked) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(pearlRequest.PearlRequestNum) + ",";
        command +=
            "'" + SOut.String(pearlRequest.RequestId) + "',"
            + SOut.Long(pearlRequest.DocNum) + ","
            + SOut.Int((int) pearlRequest.RequestStatus) + ","
            + SOut.Date(pearlRequest.DateTSent) + ","
            + SOut.Date(pearlRequest.DateTChecked) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            pearlRequest.PearlRequestNum = Db.NonQ(command, true, "PearlRequestNum", "pearlRequest");
        return pearlRequest.PearlRequestNum;
    }

    public static void Update(PearlRequest pearlRequest)
    {
        var command = "UPDATE pearlrequest SET "
                      + "RequestId      = '" + SOut.String(pearlRequest.RequestId) + "', "
                      + "DocNum         =  " + SOut.Long(pearlRequest.DocNum) + ", "
                      + "RequestStatus  =  " + SOut.Int((int) pearlRequest.RequestStatus) + ", "
                      + "DateTSent      =  " + SOut.Date(pearlRequest.DateTSent) + ", "
                      + "DateTChecked   =  " + SOut.Date(pearlRequest.DateTChecked) + " "
                      + "WHERE PearlRequestNum = " + SOut.Long(pearlRequest.PearlRequestNum);
        Db.NonQ(command);
    }

    public static bool Update(PearlRequest pearlRequest, PearlRequest oldPearlRequest)
    {
        var command = "";
        if (pearlRequest.RequestId != oldPearlRequest.RequestId)
        {
            if (command != "") command += ",";
            command += "RequestId = '" + SOut.String(pearlRequest.RequestId) + "'";
        }

        if (pearlRequest.DocNum != oldPearlRequest.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(pearlRequest.DocNum) + "";
        }

        if (pearlRequest.RequestStatus != oldPearlRequest.RequestStatus)
        {
            if (command != "") command += ",";
            command += "RequestStatus = " + SOut.Int((int) pearlRequest.RequestStatus) + "";
        }

        if (pearlRequest.DateTSent.Date != oldPearlRequest.DateTSent.Date)
        {
            if (command != "") command += ",";
            command += "DateTSent = " + SOut.Date(pearlRequest.DateTSent) + "";
        }

        if (pearlRequest.DateTChecked.Date != oldPearlRequest.DateTChecked.Date)
        {
            if (command != "") command += ",";
            command += "DateTChecked = " + SOut.Date(pearlRequest.DateTChecked) + "";
        }

        if (command == "") return false;
        command = "UPDATE pearlrequest SET " + command
                                             + " WHERE PearlRequestNum = " + SOut.Long(pearlRequest.PearlRequestNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PearlRequest pearlRequest, PearlRequest oldPearlRequest)
    {
        if (pearlRequest.RequestId != oldPearlRequest.RequestId) return true;
        if (pearlRequest.DocNum != oldPearlRequest.DocNum) return true;
        if (pearlRequest.RequestStatus != oldPearlRequest.RequestStatus) return true;
        if (pearlRequest.DateTSent.Date != oldPearlRequest.DateTSent.Date) return true;
        if (pearlRequest.DateTChecked.Date != oldPearlRequest.DateTChecked.Date) return true;
        return false;
    }

    public static void Delete(long pearlRequestNum)
    {
        var command = "DELETE FROM pearlrequest "
                      + "WHERE PearlRequestNum = " + SOut.Long(pearlRequestNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPearlRequestNums)
    {
        if (listPearlRequestNums == null || listPearlRequestNums.Count == 0) return;
        var command = "DELETE FROM pearlrequest "
                      + "WHERE PearlRequestNum IN(" + string.Join(",", listPearlRequestNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}