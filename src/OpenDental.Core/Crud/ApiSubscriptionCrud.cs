using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ApiSubscriptionCrud
{
    public static List<ApiSubscription> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApiSubscription> TableToList(DataTable table)
    {
        var retVal = new List<ApiSubscription>();
        ApiSubscription apiSubscription;
        foreach (DataRow row in table.Rows)
        {
            apiSubscription = new ApiSubscription();
            apiSubscription.ApiSubscriptionNum = SIn.Long(row["ApiSubscriptionNum"].ToString());
            apiSubscription.EndPointUrl = SIn.String(row["EndPointUrl"].ToString());
            apiSubscription.Workstation = SIn.String(row["Workstation"].ToString());
            apiSubscription.CustomerKey = SIn.String(row["CustomerKey"].ToString());
            apiSubscription.WatchTable = SIn.String(row["WatchTable"].ToString());
            apiSubscription.PollingSeconds = SIn.Int(row["PollingSeconds"].ToString());
            apiSubscription.UiEventType = SIn.String(row["UiEventType"].ToString());
            apiSubscription.DateTimeStart = SIn.DateTime(row["DateTimeStart"].ToString());
            apiSubscription.DateTimeStop = SIn.DateTime(row["DateTimeStop"].ToString());
            apiSubscription.Note = SIn.String(row["Note"].ToString());
            retVal.Add(apiSubscription);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ApiSubscription> listApiSubscriptions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ApiSubscription";
        var table = new DataTable(tableName);
        table.Columns.Add("ApiSubscriptionNum");
        table.Columns.Add("EndPointUrl");
        table.Columns.Add("Workstation");
        table.Columns.Add("CustomerKey");
        table.Columns.Add("WatchTable");
        table.Columns.Add("PollingSeconds");
        table.Columns.Add("UiEventType");
        table.Columns.Add("DateTimeStart");
        table.Columns.Add("DateTimeStop");
        table.Columns.Add("Note");
        foreach (var apiSubscription in listApiSubscriptions)
            table.Rows.Add(SOut.Long(apiSubscription.ApiSubscriptionNum), apiSubscription.EndPointUrl, apiSubscription.Workstation, apiSubscription.CustomerKey, apiSubscription.WatchTable, SOut.Int(apiSubscription.PollingSeconds), apiSubscription.UiEventType, SOut.DateT(apiSubscription.DateTimeStart, false), SOut.DateT(apiSubscription.DateTimeStop, false), apiSubscription.Note);
        return table;
    }

    public static void Insert(ApiSubscription apiSubscription)
    {
        var command = "INSERT INTO apisubscription (";

        command += "EndPointUrl,Workstation,CustomerKey,WatchTable,PollingSeconds,UiEventType,DateTimeStart,DateTimeStop,Note) VALUES(";

        command +=
            "'" + SOut.String(apiSubscription.EndPointUrl) + "',"
            + "'" + SOut.String(apiSubscription.Workstation) + "',"
            + "'" + SOut.String(apiSubscription.CustomerKey) + "',"
            + "'" + SOut.String(apiSubscription.WatchTable) + "',"
            + SOut.Int(apiSubscription.PollingSeconds) + ","
            + "'" + SOut.String(apiSubscription.UiEventType) + "',"
            + SOut.DateT(apiSubscription.DateTimeStart) + ","
            + SOut.DateT(apiSubscription.DateTimeStop) + ","
            + "'" + SOut.String(apiSubscription.Note) + "')";
        {
            apiSubscription.ApiSubscriptionNum = Db.NonQ(command, true, "ApiSubscriptionNum", "apiSubscription");
        }
    }
}