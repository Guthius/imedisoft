#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ResellerServiceCrud
{
    public static ResellerService SelectOne(long resellerServiceNum)
    {
        var command = "SELECT * FROM resellerservice "
                      + "WHERE ResellerServiceNum = " + SOut.Long(resellerServiceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ResellerService SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ResellerService> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ResellerService> TableToList(DataTable table)
    {
        var retVal = new List<ResellerService>();
        ResellerService resellerService;
        foreach (DataRow row in table.Rows)
        {
            resellerService = new ResellerService();
            resellerService.ResellerServiceNum = SIn.Long(row["ResellerServiceNum"].ToString());
            resellerService.ResellerNum = SIn.Long(row["ResellerNum"].ToString());
            resellerService.CodeNum = SIn.Long(row["CodeNum"].ToString());
            resellerService.Fee = SIn.Double(row["Fee"].ToString());
            resellerService.HostedUrl = SIn.String(row["HostedUrl"].ToString());
            resellerService.FeeRetail = SIn.Double(row["FeeRetail"].ToString());
            resellerService.Fee2 = SIn.Double(row["Fee2"].ToString());
            resellerService.Fee1Duration = TimeSpan.FromTicks(SIn.Long(row["Fee1Duration"].ToString()));
            retVal.Add(resellerService);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ResellerService> listResellerServices, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ResellerService";
        var table = new DataTable(tableName);
        table.Columns.Add("ResellerServiceNum");
        table.Columns.Add("ResellerNum");
        table.Columns.Add("CodeNum");
        table.Columns.Add("Fee");
        table.Columns.Add("HostedUrl");
        table.Columns.Add("FeeRetail");
        table.Columns.Add("Fee2");
        table.Columns.Add("Fee1Duration");
        foreach (var resellerService in listResellerServices)
            table.Rows.Add(SOut.Long(resellerService.ResellerServiceNum), SOut.Long(resellerService.ResellerNum), SOut.Long(resellerService.CodeNum), SOut.Double(resellerService.Fee, 4), resellerService.HostedUrl, SOut.Double(resellerService.FeeRetail, 4), SOut.Double(resellerService.Fee2), SOut.Long(resellerService.Fee1Duration.Ticks));
        return table;
    }

    public static long Insert(ResellerService resellerService)
    {
        return Insert(resellerService, false);
    }

    public static long Insert(ResellerService resellerService, bool useExistingPK)
    {
        var command = "INSERT INTO resellerservice (";

        command += "ResellerNum,CodeNum,Fee,HostedUrl,FeeRetail,Fee2,Fee1Duration) VALUES(";

        command +=
            SOut.Long(resellerService.ResellerNum) + ","
                                                   + SOut.Long(resellerService.CodeNum) + ","
                                                   + SOut.Double(resellerService.Fee, 4) + ","
                                                   + "'" + SOut.String(resellerService.HostedUrl) + "',"
                                                   + SOut.Double(resellerService.FeeRetail, 4) + ","
                                                   + SOut.Double(resellerService.Fee2) + ","
                                                   + "'" + SOut.Long(resellerService.Fee1Duration.Ticks) + "')";
        {
            resellerService.ResellerServiceNum = Db.NonQ(command, true, "ResellerServiceNum", "resellerService");
        }
        return resellerService.ResellerServiceNum;
    }

    public static long InsertNoCache(ResellerService resellerService)
    {
        return InsertNoCache(resellerService, false);
    }

    public static long InsertNoCache(ResellerService resellerService, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO resellerservice (";
        if (isRandomKeys || useExistingPK) command += "ResellerServiceNum,";
        command += "ResellerNum,CodeNum,Fee,HostedUrl,FeeRetail,Fee2,Fee1Duration) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(resellerService.ResellerServiceNum) + ",";
        command +=
            SOut.Long(resellerService.ResellerNum) + ","
                                                   + SOut.Long(resellerService.CodeNum) + ","
                                                   + SOut.Double(resellerService.Fee, 4) + ","
                                                   + "'" + SOut.String(resellerService.HostedUrl) + "',"
                                                   + SOut.Double(resellerService.FeeRetail, 4) + ","
                                                   + SOut.Double(resellerService.Fee2) + ","
                                                   + "'" + SOut.Long(resellerService.Fee1Duration.Ticks) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            resellerService.ResellerServiceNum = Db.NonQ(command, true, "ResellerServiceNum", "resellerService");
        return resellerService.ResellerServiceNum;
    }

    public static void Update(ResellerService resellerService)
    {
        var command = "UPDATE resellerservice SET "
                      + "ResellerNum       =  " + SOut.Long(resellerService.ResellerNum) + ", "
                      + "CodeNum           =  " + SOut.Long(resellerService.CodeNum) + ", "
                      + "Fee               =  " + SOut.Double(resellerService.Fee, 4) + ", "
                      + "HostedUrl         = '" + SOut.String(resellerService.HostedUrl) + "', "
                      + "FeeRetail         =  " + SOut.Double(resellerService.FeeRetail, 4) + ", "
                      + "Fee2              =  " + SOut.Double(resellerService.Fee2) + ", "
                      + "Fee1Duration      =  " + SOut.Long(resellerService.Fee1Duration.Ticks) + " "
                      + "WHERE ResellerServiceNum = " + SOut.Long(resellerService.ResellerServiceNum);
        Db.NonQ(command);
    }

    public static bool Update(ResellerService resellerService, ResellerService oldResellerService)
    {
        var command = "";
        if (resellerService.ResellerNum != oldResellerService.ResellerNum)
        {
            if (command != "") command += ",";
            command += "ResellerNum = " + SOut.Long(resellerService.ResellerNum) + "";
        }

        if (resellerService.CodeNum != oldResellerService.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(resellerService.CodeNum) + "";
        }

        if (resellerService.Fee != oldResellerService.Fee)
        {
            if (command != "") command += ",";
            command += "Fee = " + SOut.Double(resellerService.Fee, 4) + "";
        }

        if (resellerService.HostedUrl != oldResellerService.HostedUrl)
        {
            if (command != "") command += ",";
            command += "HostedUrl = '" + SOut.String(resellerService.HostedUrl) + "'";
        }

        if (resellerService.FeeRetail != oldResellerService.FeeRetail)
        {
            if (command != "") command += ",";
            command += "FeeRetail = " + SOut.Double(resellerService.FeeRetail, 4) + "";
        }

        if (resellerService.Fee2 != oldResellerService.Fee2)
        {
            if (command != "") command += ",";
            command += "Fee2 = " + SOut.Double(resellerService.Fee2) + "";
        }

        if (resellerService.Fee1Duration != oldResellerService.Fee1Duration)
        {
            if (command != "") command += ",";
            command += "Fee1Duration = '" + SOut.Long(resellerService.Fee1Duration.Ticks) + "'";
        }

        if (command == "") return false;
        command = "UPDATE resellerservice SET " + command
                                                + " WHERE ResellerServiceNum = " + SOut.Long(resellerService.ResellerServiceNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ResellerService resellerService, ResellerService oldResellerService)
    {
        if (resellerService.ResellerNum != oldResellerService.ResellerNum) return true;
        if (resellerService.CodeNum != oldResellerService.CodeNum) return true;
        if (resellerService.Fee != oldResellerService.Fee) return true;
        if (resellerService.HostedUrl != oldResellerService.HostedUrl) return true;
        if (resellerService.FeeRetail != oldResellerService.FeeRetail) return true;
        if (resellerService.Fee2 != oldResellerService.Fee2) return true;
        if (resellerService.Fee1Duration != oldResellerService.Fee1Duration) return true;
        return false;
    }

    public static void Delete(long resellerServiceNum)
    {
        var command = "DELETE FROM resellerservice "
                      + "WHERE ResellerServiceNum = " + SOut.Long(resellerServiceNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listResellerServiceNums)
    {
        if (listResellerServiceNums == null || listResellerServiceNums.Count == 0) return;
        var command = "DELETE FROM resellerservice "
                      + "WHERE ResellerServiceNum IN(" + string.Join(",", listResellerServiceNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}