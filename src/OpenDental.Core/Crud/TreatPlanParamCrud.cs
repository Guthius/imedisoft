#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TreatPlanParamCrud
{
    public static TreatPlanParam SelectOne(long treatPlanParamNum)
    {
        var command = "SELECT * FROM treatplanparam "
                      + "WHERE TreatPlanParamNum = " + SOut.Long(treatPlanParamNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TreatPlanParam SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TreatPlanParam> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TreatPlanParam> TableToList(DataTable table)
    {
        var retVal = new List<TreatPlanParam>();
        TreatPlanParam treatPlanParam;
        foreach (DataRow row in table.Rows)
        {
            treatPlanParam = new TreatPlanParam();
            treatPlanParam.TreatPlanParamNum = SIn.Long(row["TreatPlanParamNum"].ToString());
            treatPlanParam.PatNum = SIn.Long(row["PatNum"].ToString());
            treatPlanParam.TreatPlanNum = SIn.Long(row["TreatPlanNum"].ToString());
            treatPlanParam.ShowDiscount = SIn.Bool(row["ShowDiscount"].ToString());
            treatPlanParam.ShowMaxDed = SIn.Bool(row["ShowMaxDed"].ToString());
            treatPlanParam.ShowSubTotals = SIn.Bool(row["ShowSubTotals"].ToString());
            treatPlanParam.ShowTotals = SIn.Bool(row["ShowTotals"].ToString());
            treatPlanParam.ShowCompleted = SIn.Bool(row["ShowCompleted"].ToString());
            treatPlanParam.ShowFees = SIn.Bool(row["ShowFees"].ToString());
            treatPlanParam.ShowIns = SIn.Bool(row["ShowIns"].ToString());
            retVal.Add(treatPlanParam);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TreatPlanParam> listTreatPlanParams, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TreatPlanParam";
        var table = new DataTable(tableName);
        table.Columns.Add("TreatPlanParamNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("TreatPlanNum");
        table.Columns.Add("ShowDiscount");
        table.Columns.Add("ShowMaxDed");
        table.Columns.Add("ShowSubTotals");
        table.Columns.Add("ShowTotals");
        table.Columns.Add("ShowCompleted");
        table.Columns.Add("ShowFees");
        table.Columns.Add("ShowIns");
        foreach (var treatPlanParam in listTreatPlanParams)
            table.Rows.Add(SOut.Long(treatPlanParam.TreatPlanParamNum), SOut.Long(treatPlanParam.PatNum), SOut.Long(treatPlanParam.TreatPlanNum), SOut.Bool(treatPlanParam.ShowDiscount), SOut.Bool(treatPlanParam.ShowMaxDed), SOut.Bool(treatPlanParam.ShowSubTotals), SOut.Bool(treatPlanParam.ShowTotals), SOut.Bool(treatPlanParam.ShowCompleted), SOut.Bool(treatPlanParam.ShowFees), SOut.Bool(treatPlanParam.ShowIns));
        return table;
    }

    public static long Insert(TreatPlanParam treatPlanParam)
    {
        return Insert(treatPlanParam, false);
    }

    public static long Insert(TreatPlanParam treatPlanParam, bool useExistingPK)
    {
        var command = "INSERT INTO treatplanparam (";

        command += "PatNum,TreatPlanNum,ShowDiscount,ShowMaxDed,ShowSubTotals,ShowTotals,ShowCompleted,ShowFees,ShowIns) VALUES(";

        command +=
            SOut.Long(treatPlanParam.PatNum) + ","
                                             + SOut.Long(treatPlanParam.TreatPlanNum) + ","
                                             + SOut.Bool(treatPlanParam.ShowDiscount) + ","
                                             + SOut.Bool(treatPlanParam.ShowMaxDed) + ","
                                             + SOut.Bool(treatPlanParam.ShowSubTotals) + ","
                                             + SOut.Bool(treatPlanParam.ShowTotals) + ","
                                             + SOut.Bool(treatPlanParam.ShowCompleted) + ","
                                             + SOut.Bool(treatPlanParam.ShowFees) + ","
                                             + SOut.Bool(treatPlanParam.ShowIns) + ")";
        {
            treatPlanParam.TreatPlanParamNum = Db.NonQ(command, true, "TreatPlanParamNum", "treatPlanParam");
        }
        return treatPlanParam.TreatPlanParamNum;
    }

    public static long InsertNoCache(TreatPlanParam treatPlanParam)
    {
        return InsertNoCache(treatPlanParam, false);
    }

    public static long InsertNoCache(TreatPlanParam treatPlanParam, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO treatplanparam (";
        if (isRandomKeys || useExistingPK) command += "TreatPlanParamNum,";
        command += "PatNum,TreatPlanNum,ShowDiscount,ShowMaxDed,ShowSubTotals,ShowTotals,ShowCompleted,ShowFees,ShowIns) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(treatPlanParam.TreatPlanParamNum) + ",";
        command +=
            SOut.Long(treatPlanParam.PatNum) + ","
                                             + SOut.Long(treatPlanParam.TreatPlanNum) + ","
                                             + SOut.Bool(treatPlanParam.ShowDiscount) + ","
                                             + SOut.Bool(treatPlanParam.ShowMaxDed) + ","
                                             + SOut.Bool(treatPlanParam.ShowSubTotals) + ","
                                             + SOut.Bool(treatPlanParam.ShowTotals) + ","
                                             + SOut.Bool(treatPlanParam.ShowCompleted) + ","
                                             + SOut.Bool(treatPlanParam.ShowFees) + ","
                                             + SOut.Bool(treatPlanParam.ShowIns) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            treatPlanParam.TreatPlanParamNum = Db.NonQ(command, true, "TreatPlanParamNum", "treatPlanParam");
        return treatPlanParam.TreatPlanParamNum;
    }

    public static void Update(TreatPlanParam treatPlanParam)
    {
        var command = "UPDATE treatplanparam SET "
                      + "PatNum           =  " + SOut.Long(treatPlanParam.PatNum) + ", "
                      + "TreatPlanNum     =  " + SOut.Long(treatPlanParam.TreatPlanNum) + ", "
                      + "ShowDiscount     =  " + SOut.Bool(treatPlanParam.ShowDiscount) + ", "
                      + "ShowMaxDed       =  " + SOut.Bool(treatPlanParam.ShowMaxDed) + ", "
                      + "ShowSubTotals    =  " + SOut.Bool(treatPlanParam.ShowSubTotals) + ", "
                      + "ShowTotals       =  " + SOut.Bool(treatPlanParam.ShowTotals) + ", "
                      + "ShowCompleted    =  " + SOut.Bool(treatPlanParam.ShowCompleted) + ", "
                      + "ShowFees         =  " + SOut.Bool(treatPlanParam.ShowFees) + ", "
                      + "ShowIns          =  " + SOut.Bool(treatPlanParam.ShowIns) + " "
                      + "WHERE TreatPlanParamNum = " + SOut.Long(treatPlanParam.TreatPlanParamNum);
        Db.NonQ(command);
    }

    public static bool Update(TreatPlanParam treatPlanParam, TreatPlanParam oldTreatPlanParam)
    {
        var command = "";
        if (treatPlanParam.PatNum != oldTreatPlanParam.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(treatPlanParam.PatNum) + "";
        }

        if (treatPlanParam.TreatPlanNum != oldTreatPlanParam.TreatPlanNum)
        {
            if (command != "") command += ",";
            command += "TreatPlanNum = " + SOut.Long(treatPlanParam.TreatPlanNum) + "";
        }

        if (treatPlanParam.ShowDiscount != oldTreatPlanParam.ShowDiscount)
        {
            if (command != "") command += ",";
            command += "ShowDiscount = " + SOut.Bool(treatPlanParam.ShowDiscount) + "";
        }

        if (treatPlanParam.ShowMaxDed != oldTreatPlanParam.ShowMaxDed)
        {
            if (command != "") command += ",";
            command += "ShowMaxDed = " + SOut.Bool(treatPlanParam.ShowMaxDed) + "";
        }

        if (treatPlanParam.ShowSubTotals != oldTreatPlanParam.ShowSubTotals)
        {
            if (command != "") command += ",";
            command += "ShowSubTotals = " + SOut.Bool(treatPlanParam.ShowSubTotals) + "";
        }

        if (treatPlanParam.ShowTotals != oldTreatPlanParam.ShowTotals)
        {
            if (command != "") command += ",";
            command += "ShowTotals = " + SOut.Bool(treatPlanParam.ShowTotals) + "";
        }

        if (treatPlanParam.ShowCompleted != oldTreatPlanParam.ShowCompleted)
        {
            if (command != "") command += ",";
            command += "ShowCompleted = " + SOut.Bool(treatPlanParam.ShowCompleted) + "";
        }

        if (treatPlanParam.ShowFees != oldTreatPlanParam.ShowFees)
        {
            if (command != "") command += ",";
            command += "ShowFees = " + SOut.Bool(treatPlanParam.ShowFees) + "";
        }

        if (treatPlanParam.ShowIns != oldTreatPlanParam.ShowIns)
        {
            if (command != "") command += ",";
            command += "ShowIns = " + SOut.Bool(treatPlanParam.ShowIns) + "";
        }

        if (command == "") return false;
        command = "UPDATE treatplanparam SET " + command
                                               + " WHERE TreatPlanParamNum = " + SOut.Long(treatPlanParam.TreatPlanParamNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TreatPlanParam treatPlanParam, TreatPlanParam oldTreatPlanParam)
    {
        if (treatPlanParam.PatNum != oldTreatPlanParam.PatNum) return true;
        if (treatPlanParam.TreatPlanNum != oldTreatPlanParam.TreatPlanNum) return true;
        if (treatPlanParam.ShowDiscount != oldTreatPlanParam.ShowDiscount) return true;
        if (treatPlanParam.ShowMaxDed != oldTreatPlanParam.ShowMaxDed) return true;
        if (treatPlanParam.ShowSubTotals != oldTreatPlanParam.ShowSubTotals) return true;
        if (treatPlanParam.ShowTotals != oldTreatPlanParam.ShowTotals) return true;
        if (treatPlanParam.ShowCompleted != oldTreatPlanParam.ShowCompleted) return true;
        if (treatPlanParam.ShowFees != oldTreatPlanParam.ShowFees) return true;
        if (treatPlanParam.ShowIns != oldTreatPlanParam.ShowIns) return true;
        return false;
    }

    public static void Delete(long treatPlanParamNum)
    {
        var command = "DELETE FROM treatplanparam "
                      + "WHERE TreatPlanParamNum = " + SOut.Long(treatPlanParamNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTreatPlanParamNums)
    {
        if (listTreatPlanParamNums == null || listTreatPlanParamNums.Count == 0) return;
        var command = "DELETE FROM treatplanparam "
                      + "WHERE TreatPlanParamNum IN(" + string.Join(",", listTreatPlanParamNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}