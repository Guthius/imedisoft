#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DashboardARCrud
{
    public static DashboardAR SelectOne(long dashboardARNum)
    {
        var command = "SELECT * FROM dashboardar "
                      + "WHERE DashboardARNum = " + SOut.Long(dashboardARNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DashboardAR SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DashboardAR> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DashboardAR> TableToList(DataTable table)
    {
        var retVal = new List<DashboardAR>();
        DashboardAR dashboardAR;
        foreach (DataRow row in table.Rows)
        {
            dashboardAR = new DashboardAR();
            dashboardAR.DashboardARNum = SIn.Long(row["DashboardARNum"].ToString());
            dashboardAR.DateCalc = SIn.Date(row["DateCalc"].ToString());
            dashboardAR.BalTotal = SIn.Double(row["BalTotal"].ToString());
            dashboardAR.InsEst = SIn.Double(row["InsEst"].ToString());
            retVal.Add(dashboardAR);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DashboardAR> listDashboardARs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DashboardAR";
        var table = new DataTable(tableName);
        table.Columns.Add("DashboardARNum");
        table.Columns.Add("DateCalc");
        table.Columns.Add("BalTotal");
        table.Columns.Add("InsEst");
        foreach (var dashboardAR in listDashboardARs)
            table.Rows.Add(SOut.Long(dashboardAR.DashboardARNum), SOut.DateT(dashboardAR.DateCalc, false), SOut.Double(dashboardAR.BalTotal), SOut.Double(dashboardAR.InsEst));
        return table;
    }

    public static long Insert(DashboardAR dashboardAR)
    {
        return Insert(dashboardAR, false);
    }

    public static long Insert(DashboardAR dashboardAR, bool useExistingPK)
    {
        var command = "INSERT INTO dashboardar (";

        command += "DateCalc,BalTotal,InsEst) VALUES(";

        command +=
            SOut.Date(dashboardAR.DateCalc) + ","
                                            + SOut.Double(dashboardAR.BalTotal) + ","
                                            + SOut.Double(dashboardAR.InsEst) + ")";
        {
            dashboardAR.DashboardARNum = Db.NonQ(command, true, "DashboardARNum", "dashboardAR");
        }
        return dashboardAR.DashboardARNum;
    }

    public static long InsertNoCache(DashboardAR dashboardAR)
    {
        return InsertNoCache(dashboardAR, false);
    }

    public static long InsertNoCache(DashboardAR dashboardAR, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO dashboardar (";
        if (isRandomKeys || useExistingPK) command += "DashboardARNum,";
        command += "DateCalc,BalTotal,InsEst) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(dashboardAR.DashboardARNum) + ",";
        command +=
            SOut.Date(dashboardAR.DateCalc) + ","
                                            + SOut.Double(dashboardAR.BalTotal) + ","
                                            + SOut.Double(dashboardAR.InsEst) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            dashboardAR.DashboardARNum = Db.NonQ(command, true, "DashboardARNum", "dashboardAR");
        return dashboardAR.DashboardARNum;
    }

    public static void Update(DashboardAR dashboardAR)
    {
        var command = "UPDATE dashboardar SET "
                      + "DateCalc      =  " + SOut.Date(dashboardAR.DateCalc) + ", "
                      + "BalTotal      =  " + SOut.Double(dashboardAR.BalTotal) + ", "
                      + "InsEst        =  " + SOut.Double(dashboardAR.InsEst) + " "
                      + "WHERE DashboardARNum = " + SOut.Long(dashboardAR.DashboardARNum);
        Db.NonQ(command);
    }

    public static bool Update(DashboardAR dashboardAR, DashboardAR oldDashboardAR)
    {
        var command = "";
        if (dashboardAR.DateCalc.Date != oldDashboardAR.DateCalc.Date)
        {
            if (command != "") command += ",";
            command += "DateCalc = " + SOut.Date(dashboardAR.DateCalc) + "";
        }

        if (dashboardAR.BalTotal != oldDashboardAR.BalTotal)
        {
            if (command != "") command += ",";
            command += "BalTotal = " + SOut.Double(dashboardAR.BalTotal) + "";
        }

        if (dashboardAR.InsEst != oldDashboardAR.InsEst)
        {
            if (command != "") command += ",";
            command += "InsEst = " + SOut.Double(dashboardAR.InsEst) + "";
        }

        if (command == "") return false;
        command = "UPDATE dashboardar SET " + command
                                            + " WHERE DashboardARNum = " + SOut.Long(dashboardAR.DashboardARNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DashboardAR dashboardAR, DashboardAR oldDashboardAR)
    {
        if (dashboardAR.DateCalc.Date != oldDashboardAR.DateCalc.Date) return true;
        if (dashboardAR.BalTotal != oldDashboardAR.BalTotal) return true;
        if (dashboardAR.InsEst != oldDashboardAR.InsEst) return true;
        return false;
    }

    public static void Delete(long dashboardARNum)
    {
        var command = "DELETE FROM dashboardar "
                      + "WHERE DashboardARNum = " + SOut.Long(dashboardARNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDashboardARNums)
    {
        if (listDashboardARNums == null || listDashboardARNums.Count == 0) return;
        var command = "DELETE FROM dashboardar "
                      + "WHERE DashboardARNum IN(" + string.Join(",", listDashboardARNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}