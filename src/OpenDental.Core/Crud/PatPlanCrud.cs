#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatPlanCrud
{
    public static PatPlan SelectOne(long patPlanNum)
    {
        var command = "SELECT * FROM patplan "
                      + "WHERE PatPlanNum = " + SOut.Long(patPlanNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatPlan SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatPlan> TableToList(DataTable table)
    {
        var retVal = new List<PatPlan>();
        PatPlan patPlan;
        foreach (DataRow row in table.Rows)
        {
            patPlan = new PatPlan();
            patPlan.PatPlanNum = SIn.Long(row["PatPlanNum"].ToString());
            patPlan.PatNum = SIn.Long(row["PatNum"].ToString());
            patPlan.Ordinal = SIn.Byte(row["Ordinal"].ToString());
            patPlan.IsPending = SIn.Bool(row["IsPending"].ToString());
            patPlan.Relationship = (Relat) SIn.Int(row["Relationship"].ToString());
            patPlan.PatID = SIn.String(row["PatID"].ToString());
            patPlan.InsSubNum = SIn.Long(row["InsSubNum"].ToString());
            patPlan.OrthoAutoFeeBilledOverride = SIn.Double(row["OrthoAutoFeeBilledOverride"].ToString());
            patPlan.OrthoAutoNextClaimDate = SIn.Date(row["OrthoAutoNextClaimDate"].ToString());
            patPlan.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            patPlan.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(patPlan);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatPlan> listPatPlans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatPlan";
        var table = new DataTable(tableName);
        table.Columns.Add("PatPlanNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("Ordinal");
        table.Columns.Add("IsPending");
        table.Columns.Add("Relationship");
        table.Columns.Add("PatID");
        table.Columns.Add("InsSubNum");
        table.Columns.Add("OrthoAutoFeeBilledOverride");
        table.Columns.Add("OrthoAutoNextClaimDate");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var patPlan in listPatPlans)
            table.Rows.Add(SOut.Long(patPlan.PatPlanNum), SOut.Long(patPlan.PatNum), SOut.Byte(patPlan.Ordinal), SOut.Bool(patPlan.IsPending), SOut.Int((int) patPlan.Relationship), patPlan.PatID, SOut.Long(patPlan.InsSubNum), SOut.Double(patPlan.OrthoAutoFeeBilledOverride), SOut.DateT(patPlan.OrthoAutoNextClaimDate, false), SOut.DateT(patPlan.SecDateTEntry, false), SOut.DateT(patPlan.SecDateTEdit, false));
        return table;
    }

    public static long Insert(PatPlan patPlan)
    {
        return Insert(patPlan, false);
    }

    public static long Insert(PatPlan patPlan, bool useExistingPK)
    {
        var command = "INSERT INTO patplan (";

        command += "PatNum,Ordinal,IsPending,Relationship,PatID,InsSubNum,OrthoAutoFeeBilledOverride,OrthoAutoNextClaimDate,SecDateTEntry) VALUES(";

        command +=
            SOut.Long(patPlan.PatNum) + ","
                                      + SOut.Byte(patPlan.Ordinal) + ","
                                      + SOut.Bool(patPlan.IsPending) + ","
                                      + SOut.Int((int) patPlan.Relationship) + ","
                                      + "'" + SOut.String(patPlan.PatID) + "',"
                                      + SOut.Long(patPlan.InsSubNum) + ","
                                      + SOut.Double(patPlan.OrthoAutoFeeBilledOverride) + ","
                                      + SOut.Date(patPlan.OrthoAutoNextClaimDate) + ","
                                      + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL

        patPlan.PatPlanNum = Db.NonQ(command, true, "PatPlanNum", "patPlan");
        return patPlan.PatPlanNum;
    }

    public static long InsertNoCache(PatPlan patPlan)
    {
        return InsertNoCache(patPlan, false);
    }

    public static long InsertNoCache(PatPlan patPlan, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patplan (";
        if (isRandomKeys || useExistingPK) command += "PatPlanNum,";
        command += "PatNum,Ordinal,IsPending,Relationship,PatID,InsSubNum,OrthoAutoFeeBilledOverride,OrthoAutoNextClaimDate,SecDateTEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patPlan.PatPlanNum) + ",";
        command +=
            SOut.Long(patPlan.PatNum) + ","
                                      + SOut.Byte(patPlan.Ordinal) + ","
                                      + SOut.Bool(patPlan.IsPending) + ","
                                      + SOut.Int((int) patPlan.Relationship) + ","
                                      + "'" + SOut.String(patPlan.PatID) + "',"
                                      + SOut.Long(patPlan.InsSubNum) + ","
                                      + SOut.Double(patPlan.OrthoAutoFeeBilledOverride) + ","
                                      + SOut.Date(patPlan.OrthoAutoNextClaimDate) + ","
                                      + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            patPlan.PatPlanNum = Db.NonQ(command, true, "PatPlanNum", "patPlan");
        return patPlan.PatPlanNum;
    }

    public static void Update(PatPlan patPlan)
    {
        var command = "UPDATE patplan SET "
                      + "PatNum                    =  " + SOut.Long(patPlan.PatNum) + ", "
                      + "Ordinal                   =  " + SOut.Byte(patPlan.Ordinal) + ", "
                      + "IsPending                 =  " + SOut.Bool(patPlan.IsPending) + ", "
                      + "Relationship              =  " + SOut.Int((int) patPlan.Relationship) + ", "
                      + "PatID                     = '" + SOut.String(patPlan.PatID) + "', "
                      + "InsSubNum                 =  " + SOut.Long(patPlan.InsSubNum) + ", "
                      + "OrthoAutoFeeBilledOverride=  " + SOut.Double(patPlan.OrthoAutoFeeBilledOverride) + ", "
                      + "OrthoAutoNextClaimDate    =  " + SOut.Date(patPlan.OrthoAutoNextClaimDate) + " "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE PatPlanNum = " + SOut.Long(patPlan.PatPlanNum);
        Db.NonQ(command);
    }

    public static bool Update(PatPlan patPlan, PatPlan oldPatPlan)
    {
        var command = "";
        if (patPlan.PatNum != oldPatPlan.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(patPlan.PatNum) + "";
        }

        if (patPlan.Ordinal != oldPatPlan.Ordinal)
        {
            if (command != "") command += ",";
            command += "Ordinal = " + SOut.Byte(patPlan.Ordinal) + "";
        }

        if (patPlan.IsPending != oldPatPlan.IsPending)
        {
            if (command != "") command += ",";
            command += "IsPending = " + SOut.Bool(patPlan.IsPending) + "";
        }

        if (patPlan.Relationship != oldPatPlan.Relationship)
        {
            if (command != "") command += ",";
            command += "Relationship = " + SOut.Int((int) patPlan.Relationship) + "";
        }

        if (patPlan.PatID != oldPatPlan.PatID)
        {
            if (command != "") command += ",";
            command += "PatID = '" + SOut.String(patPlan.PatID) + "'";
        }

        if (patPlan.InsSubNum != oldPatPlan.InsSubNum)
        {
            if (command != "") command += ",";
            command += "InsSubNum = " + SOut.Long(patPlan.InsSubNum) + "";
        }

        if (patPlan.OrthoAutoFeeBilledOverride != oldPatPlan.OrthoAutoFeeBilledOverride)
        {
            if (command != "") command += ",";
            command += "OrthoAutoFeeBilledOverride = " + SOut.Double(patPlan.OrthoAutoFeeBilledOverride) + "";
        }

        if (patPlan.OrthoAutoNextClaimDate.Date != oldPatPlan.OrthoAutoNextClaimDate.Date)
        {
            if (command != "") command += ",";
            command += "OrthoAutoNextClaimDate = " + SOut.Date(patPlan.OrthoAutoNextClaimDate) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE patplan SET " + command
                                        + " WHERE PatPlanNum = " + SOut.Long(patPlan.PatPlanNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PatPlan patPlan, PatPlan oldPatPlan)
    {
        if (patPlan.PatNum != oldPatPlan.PatNum) return true;
        if (patPlan.Ordinal != oldPatPlan.Ordinal) return true;
        if (patPlan.IsPending != oldPatPlan.IsPending) return true;
        if (patPlan.Relationship != oldPatPlan.Relationship) return true;
        if (patPlan.PatID != oldPatPlan.PatID) return true;
        if (patPlan.InsSubNum != oldPatPlan.InsSubNum) return true;
        if (patPlan.OrthoAutoFeeBilledOverride != oldPatPlan.OrthoAutoFeeBilledOverride) return true;
        if (patPlan.OrthoAutoNextClaimDate.Date != oldPatPlan.OrthoAutoNextClaimDate.Date) return true;
        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long patPlanNum)
    {
        var command = "DELETE FROM patplan "
                      + "WHERE PatPlanNum = " + SOut.Long(patPlanNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatPlanNums)
    {
        if (listPatPlanNums == null || listPatPlanNums.Count == 0) return;
        var command = "DELETE FROM patplan "
                      + "WHERE PatPlanNum IN(" + string.Join(",", listPatPlanNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}