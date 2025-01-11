#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FHIRSubscriptionCrud
{
    public static FHIRSubscription SelectOne(long fHIRSubscriptionNum)
    {
        var command = "SELECT * FROM fhirsubscription "
                      + "WHERE FHIRSubscriptionNum = " + SOut.Long(fHIRSubscriptionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static FHIRSubscription SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FHIRSubscription> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FHIRSubscription> TableToList(DataTable table)
    {
        var retVal = new List<FHIRSubscription>();
        FHIRSubscription fHIRSubscription;
        foreach (DataRow row in table.Rows)
        {
            fHIRSubscription = new FHIRSubscription();
            fHIRSubscription.FHIRSubscriptionNum = SIn.Long(row["FHIRSubscriptionNum"].ToString());
            fHIRSubscription.Criteria = SIn.String(row["Criteria"].ToString());
            fHIRSubscription.Reason = SIn.String(row["Reason"].ToString());
            fHIRSubscription.SubStatus = (SubscriptionStatus) SIn.Int(row["SubStatus"].ToString());
            fHIRSubscription.ErrorNote = SIn.String(row["ErrorNote"].ToString());
            fHIRSubscription.ChannelType = (SubscriptionChannelType) SIn.Int(row["ChannelType"].ToString());
            fHIRSubscription.ChannelEndpoint = SIn.String(row["ChannelEndpoint"].ToString());
            fHIRSubscription.ChannelPayLoad = SIn.String(row["ChannelPayLoad"].ToString());
            fHIRSubscription.ChannelHeader = SIn.String(row["ChannelHeader"].ToString());
            fHIRSubscription.DateEnd = SIn.DateTime(row["DateEnd"].ToString());
            fHIRSubscription.APIKeyHash = SIn.String(row["APIKeyHash"].ToString());
            retVal.Add(fHIRSubscription);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FHIRSubscription> listFHIRSubscriptions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FHIRSubscription";
        var table = new DataTable(tableName);
        table.Columns.Add("FHIRSubscriptionNum");
        table.Columns.Add("Criteria");
        table.Columns.Add("Reason");
        table.Columns.Add("SubStatus");
        table.Columns.Add("ErrorNote");
        table.Columns.Add("ChannelType");
        table.Columns.Add("ChannelEndpoint");
        table.Columns.Add("ChannelPayLoad");
        table.Columns.Add("ChannelHeader");
        table.Columns.Add("DateEnd");
        table.Columns.Add("APIKeyHash");
        foreach (var fHIRSubscription in listFHIRSubscriptions)
            table.Rows.Add(SOut.Long(fHIRSubscription.FHIRSubscriptionNum), fHIRSubscription.Criteria, fHIRSubscription.Reason, SOut.Int((int) fHIRSubscription.SubStatus), fHIRSubscription.ErrorNote, SOut.Int((int) fHIRSubscription.ChannelType), fHIRSubscription.ChannelEndpoint, fHIRSubscription.ChannelPayLoad, fHIRSubscription.ChannelHeader, SOut.DateT(fHIRSubscription.DateEnd, false), fHIRSubscription.APIKeyHash);
        return table;
    }

    public static long Insert(FHIRSubscription fHIRSubscription)
    {
        return Insert(fHIRSubscription, false);
    }

    public static long Insert(FHIRSubscription fHIRSubscription, bool useExistingPK)
    {
        var command = "INSERT INTO fhirsubscription (";

        command += "Criteria,Reason,SubStatus,ErrorNote,ChannelType,ChannelEndpoint,ChannelPayLoad,ChannelHeader,DateEnd,APIKeyHash) VALUES(";

        command +=
            "'" + SOut.String(fHIRSubscription.Criteria) + "',"
            + "'" + SOut.String(fHIRSubscription.Reason) + "',"
            + SOut.Int((int) fHIRSubscription.SubStatus) + ","
            + DbHelper.ParamChar + "paramErrorNote,"
            + SOut.Int((int) fHIRSubscription.ChannelType) + ","
            + "'" + SOut.String(fHIRSubscription.ChannelEndpoint) + "',"
            + "'" + SOut.String(fHIRSubscription.ChannelPayLoad) + "',"
            + "'" + SOut.String(fHIRSubscription.ChannelHeader) + "',"
            + SOut.DateT(fHIRSubscription.DateEnd) + ","
            + "'" + SOut.String(fHIRSubscription.APIKeyHash) + "')";
        if (fHIRSubscription.ErrorNote == null) fHIRSubscription.ErrorNote = "";
        var paramErrorNote = new OdSqlParameter("paramErrorNote", OdDbType.Text, SOut.StringParam(fHIRSubscription.ErrorNote));
        {
            fHIRSubscription.FHIRSubscriptionNum = Db.NonQ(command, true, "FHIRSubscriptionNum", "fHIRSubscription", paramErrorNote);
        }
        return fHIRSubscription.FHIRSubscriptionNum;
    }

    public static long InsertNoCache(FHIRSubscription fHIRSubscription)
    {
        return InsertNoCache(fHIRSubscription, false);
    }

    public static long InsertNoCache(FHIRSubscription fHIRSubscription, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO fhirsubscription (";
        if (isRandomKeys || useExistingPK) command += "FHIRSubscriptionNum,";
        command += "Criteria,Reason,SubStatus,ErrorNote,ChannelType,ChannelEndpoint,ChannelPayLoad,ChannelHeader,DateEnd,APIKeyHash) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(fHIRSubscription.FHIRSubscriptionNum) + ",";
        command +=
            "'" + SOut.String(fHIRSubscription.Criteria) + "',"
            + "'" + SOut.String(fHIRSubscription.Reason) + "',"
            + SOut.Int((int) fHIRSubscription.SubStatus) + ","
            + DbHelper.ParamChar + "paramErrorNote,"
            + SOut.Int((int) fHIRSubscription.ChannelType) + ","
            + "'" + SOut.String(fHIRSubscription.ChannelEndpoint) + "',"
            + "'" + SOut.String(fHIRSubscription.ChannelPayLoad) + "',"
            + "'" + SOut.String(fHIRSubscription.ChannelHeader) + "',"
            + SOut.DateT(fHIRSubscription.DateEnd) + ","
            + "'" + SOut.String(fHIRSubscription.APIKeyHash) + "')";
        if (fHIRSubscription.ErrorNote == null) fHIRSubscription.ErrorNote = "";
        var paramErrorNote = new OdSqlParameter("paramErrorNote", OdDbType.Text, SOut.StringParam(fHIRSubscription.ErrorNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramErrorNote);
        else
            fHIRSubscription.FHIRSubscriptionNum = Db.NonQ(command, true, "FHIRSubscriptionNum", "fHIRSubscription", paramErrorNote);
        return fHIRSubscription.FHIRSubscriptionNum;
    }

    public static void Update(FHIRSubscription fHIRSubscription)
    {
        var command = "UPDATE fhirsubscription SET "
                      + "Criteria           = '" + SOut.String(fHIRSubscription.Criteria) + "', "
                      + "Reason             = '" + SOut.String(fHIRSubscription.Reason) + "', "
                      + "SubStatus          =  " + SOut.Int((int) fHIRSubscription.SubStatus) + ", "
                      + "ErrorNote          =  " + DbHelper.ParamChar + "paramErrorNote, "
                      + "ChannelType        =  " + SOut.Int((int) fHIRSubscription.ChannelType) + ", "
                      + "ChannelEndpoint    = '" + SOut.String(fHIRSubscription.ChannelEndpoint) + "', "
                      + "ChannelPayLoad     = '" + SOut.String(fHIRSubscription.ChannelPayLoad) + "', "
                      + "ChannelHeader      = '" + SOut.String(fHIRSubscription.ChannelHeader) + "', "
                      + "DateEnd            =  " + SOut.DateT(fHIRSubscription.DateEnd) + ", "
                      + "APIKeyHash         = '" + SOut.String(fHIRSubscription.APIKeyHash) + "' "
                      + "WHERE FHIRSubscriptionNum = " + SOut.Long(fHIRSubscription.FHIRSubscriptionNum);
        if (fHIRSubscription.ErrorNote == null) fHIRSubscription.ErrorNote = "";
        var paramErrorNote = new OdSqlParameter("paramErrorNote", OdDbType.Text, SOut.StringParam(fHIRSubscription.ErrorNote));
        Db.NonQ(command, paramErrorNote);
    }

    public static bool Update(FHIRSubscription fHIRSubscription, FHIRSubscription oldFHIRSubscription)
    {
        var command = "";
        if (fHIRSubscription.Criteria != oldFHIRSubscription.Criteria)
        {
            if (command != "") command += ",";
            command += "Criteria = '" + SOut.String(fHIRSubscription.Criteria) + "'";
        }

        if (fHIRSubscription.Reason != oldFHIRSubscription.Reason)
        {
            if (command != "") command += ",";
            command += "Reason = '" + SOut.String(fHIRSubscription.Reason) + "'";
        }

        if (fHIRSubscription.SubStatus != oldFHIRSubscription.SubStatus)
        {
            if (command != "") command += ",";
            command += "SubStatus = " + SOut.Int((int) fHIRSubscription.SubStatus) + "";
        }

        if (fHIRSubscription.ErrorNote != oldFHIRSubscription.ErrorNote)
        {
            if (command != "") command += ",";
            command += "ErrorNote = " + DbHelper.ParamChar + "paramErrorNote";
        }

        if (fHIRSubscription.ChannelType != oldFHIRSubscription.ChannelType)
        {
            if (command != "") command += ",";
            command += "ChannelType = " + SOut.Int((int) fHIRSubscription.ChannelType) + "";
        }

        if (fHIRSubscription.ChannelEndpoint != oldFHIRSubscription.ChannelEndpoint)
        {
            if (command != "") command += ",";
            command += "ChannelEndpoint = '" + SOut.String(fHIRSubscription.ChannelEndpoint) + "'";
        }

        if (fHIRSubscription.ChannelPayLoad != oldFHIRSubscription.ChannelPayLoad)
        {
            if (command != "") command += ",";
            command += "ChannelPayLoad = '" + SOut.String(fHIRSubscription.ChannelPayLoad) + "'";
        }

        if (fHIRSubscription.ChannelHeader != oldFHIRSubscription.ChannelHeader)
        {
            if (command != "") command += ",";
            command += "ChannelHeader = '" + SOut.String(fHIRSubscription.ChannelHeader) + "'";
        }

        if (fHIRSubscription.DateEnd != oldFHIRSubscription.DateEnd)
        {
            if (command != "") command += ",";
            command += "DateEnd = " + SOut.DateT(fHIRSubscription.DateEnd) + "";
        }

        if (fHIRSubscription.APIKeyHash != oldFHIRSubscription.APIKeyHash)
        {
            if (command != "") command += ",";
            command += "APIKeyHash = '" + SOut.String(fHIRSubscription.APIKeyHash) + "'";
        }

        if (command == "") return false;
        if (fHIRSubscription.ErrorNote == null) fHIRSubscription.ErrorNote = "";
        var paramErrorNote = new OdSqlParameter("paramErrorNote", OdDbType.Text, SOut.StringParam(fHIRSubscription.ErrorNote));
        command = "UPDATE fhirsubscription SET " + command
                                                 + " WHERE FHIRSubscriptionNum = " + SOut.Long(fHIRSubscription.FHIRSubscriptionNum);
        Db.NonQ(command, paramErrorNote);
        return true;
    }

    public static bool UpdateComparison(FHIRSubscription fHIRSubscription, FHIRSubscription oldFHIRSubscription)
    {
        if (fHIRSubscription.Criteria != oldFHIRSubscription.Criteria) return true;
        if (fHIRSubscription.Reason != oldFHIRSubscription.Reason) return true;
        if (fHIRSubscription.SubStatus != oldFHIRSubscription.SubStatus) return true;
        if (fHIRSubscription.ErrorNote != oldFHIRSubscription.ErrorNote) return true;
        if (fHIRSubscription.ChannelType != oldFHIRSubscription.ChannelType) return true;
        if (fHIRSubscription.ChannelEndpoint != oldFHIRSubscription.ChannelEndpoint) return true;
        if (fHIRSubscription.ChannelPayLoad != oldFHIRSubscription.ChannelPayLoad) return true;
        if (fHIRSubscription.ChannelHeader != oldFHIRSubscription.ChannelHeader) return true;
        if (fHIRSubscription.DateEnd != oldFHIRSubscription.DateEnd) return true;
        if (fHIRSubscription.APIKeyHash != oldFHIRSubscription.APIKeyHash) return true;
        return false;
    }

    public static void Delete(long fHIRSubscriptionNum)
    {
        var command = "DELETE FROM fhirsubscription "
                      + "WHERE FHIRSubscriptionNum = " + SOut.Long(fHIRSubscriptionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFHIRSubscriptionNums)
    {
        if (listFHIRSubscriptionNums == null || listFHIRSubscriptionNums.Count == 0) return;
        var command = "DELETE FROM fhirsubscription "
                      + "WHERE FHIRSubscriptionNum IN(" + string.Join(",", listFHIRSubscriptionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}