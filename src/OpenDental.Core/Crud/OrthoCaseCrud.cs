#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoCaseCrud
{
    public static OrthoCase SelectOne(long orthoCaseNum)
    {
        var command = "SELECT * FROM orthocase "
                      + "WHERE OrthoCaseNum = " + SOut.Long(orthoCaseNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoCase SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoCase> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoCase> TableToList(DataTable table)
    {
        var retVal = new List<OrthoCase>();
        OrthoCase orthoCase;
        foreach (DataRow row in table.Rows)
        {
            orthoCase = new OrthoCase();
            orthoCase.OrthoCaseNum = SIn.Long(row["OrthoCaseNum"].ToString());
            orthoCase.PatNum = SIn.Long(row["PatNum"].ToString());
            orthoCase.ProvNum = SIn.Long(row["ProvNum"].ToString());
            orthoCase.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            orthoCase.Fee = SIn.Double(row["Fee"].ToString());
            orthoCase.FeeInsPrimary = SIn.Double(row["FeeInsPrimary"].ToString());
            orthoCase.FeePat = SIn.Double(row["FeePat"].ToString());
            orthoCase.BandingDate = SIn.Date(row["BandingDate"].ToString());
            orthoCase.DebondDate = SIn.Date(row["DebondDate"].ToString());
            orthoCase.DebondDateExpected = SIn.Date(row["DebondDateExpected"].ToString());
            orthoCase.IsTransfer = SIn.Bool(row["IsTransfer"].ToString());
            orthoCase.OrthoType = SIn.Long(row["OrthoType"].ToString());
            orthoCase.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            orthoCase.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            orthoCase.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            orthoCase.IsActive = SIn.Bool(row["IsActive"].ToString());
            orthoCase.FeeInsSecondary = SIn.Double(row["FeeInsSecondary"].ToString());
            retVal.Add(orthoCase);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoCase> listOrthoCases, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoCase";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoCaseNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("Fee");
        table.Columns.Add("FeeInsPrimary");
        table.Columns.Add("FeePat");
        table.Columns.Add("BandingDate");
        table.Columns.Add("DebondDate");
        table.Columns.Add("DebondDateExpected");
        table.Columns.Add("IsTransfer");
        table.Columns.Add("OrthoType");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("IsActive");
        table.Columns.Add("FeeInsSecondary");
        foreach (var orthoCase in listOrthoCases)
            table.Rows.Add(SOut.Long(orthoCase.OrthoCaseNum), SOut.Long(orthoCase.PatNum), SOut.Long(orthoCase.ProvNum), SOut.Long(orthoCase.ClinicNum), SOut.Double(orthoCase.Fee), SOut.Double(orthoCase.FeeInsPrimary), SOut.Double(orthoCase.FeePat), SOut.DateT(orthoCase.BandingDate, false), SOut.DateT(orthoCase.DebondDate, false), SOut.DateT(orthoCase.DebondDateExpected, false), SOut.Bool(orthoCase.IsTransfer), SOut.Long(orthoCase.OrthoType), SOut.DateT(orthoCase.SecDateTEntry, false), SOut.Long(orthoCase.SecUserNumEntry), SOut.DateT(orthoCase.SecDateTEdit, false), SOut.Bool(orthoCase.IsActive), SOut.Double(orthoCase.FeeInsSecondary));
        return table;
    }

    public static long Insert(OrthoCase orthoCase)
    {
        return Insert(orthoCase, false);
    }

    public static long Insert(OrthoCase orthoCase, bool useExistingPK)
    {
        var command = "INSERT INTO orthocase (";

        command += "PatNum,ProvNum,ClinicNum,Fee,FeeInsPrimary,FeePat,BandingDate,DebondDate,DebondDateExpected,IsTransfer,OrthoType,SecDateTEntry,SecUserNumEntry,IsActive,FeeInsSecondary) VALUES(";

        command +=
            SOut.Long(orthoCase.PatNum) + ","
                                        + SOut.Long(orthoCase.ProvNum) + ","
                                        + SOut.Long(orthoCase.ClinicNum) + ","
                                        + SOut.Double(orthoCase.Fee) + ","
                                        + SOut.Double(orthoCase.FeeInsPrimary) + ","
                                        + SOut.Double(orthoCase.FeePat) + ","
                                        + SOut.Date(orthoCase.BandingDate) + ","
                                        + SOut.Date(orthoCase.DebondDate) + ","
                                        + SOut.Date(orthoCase.DebondDateExpected) + ","
                                        + SOut.Bool(orthoCase.IsTransfer) + ","
                                        + SOut.Long(orthoCase.OrthoType) + ","
                                        + DbHelper.Now() + ","
                                        + SOut.Long(orthoCase.SecUserNumEntry) + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + SOut.Bool(orthoCase.IsActive) + ","
                                        + SOut.Double(orthoCase.FeeInsSecondary) + ")";
        {
            orthoCase.OrthoCaseNum = Db.NonQ(command, true, "OrthoCaseNum", "orthoCase");
        }
        return orthoCase.OrthoCaseNum;
    }

    public static long InsertNoCache(OrthoCase orthoCase)
    {
        return InsertNoCache(orthoCase, false);
    }

    public static long InsertNoCache(OrthoCase orthoCase, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthocase (";
        if (isRandomKeys || useExistingPK) command += "OrthoCaseNum,";
        command += "PatNum,ProvNum,ClinicNum,Fee,FeeInsPrimary,FeePat,BandingDate,DebondDate,DebondDateExpected,IsTransfer,OrthoType,SecDateTEntry,SecUserNumEntry,IsActive,FeeInsSecondary) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoCase.OrthoCaseNum) + ",";
        command +=
            SOut.Long(orthoCase.PatNum) + ","
                                        + SOut.Long(orthoCase.ProvNum) + ","
                                        + SOut.Long(orthoCase.ClinicNum) + ","
                                        + SOut.Double(orthoCase.Fee) + ","
                                        + SOut.Double(orthoCase.FeeInsPrimary) + ","
                                        + SOut.Double(orthoCase.FeePat) + ","
                                        + SOut.Date(orthoCase.BandingDate) + ","
                                        + SOut.Date(orthoCase.DebondDate) + ","
                                        + SOut.Date(orthoCase.DebondDateExpected) + ","
                                        + SOut.Bool(orthoCase.IsTransfer) + ","
                                        + SOut.Long(orthoCase.OrthoType) + ","
                                        + DbHelper.Now() + ","
                                        + SOut.Long(orthoCase.SecUserNumEntry) + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + SOut.Bool(orthoCase.IsActive) + ","
                                        + SOut.Double(orthoCase.FeeInsSecondary) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoCase.OrthoCaseNum = Db.NonQ(command, true, "OrthoCaseNum", "orthoCase");
        return orthoCase.OrthoCaseNum;
    }

    public static void Update(OrthoCase orthoCase)
    {
        var command = "UPDATE orthocase SET "
                      + "PatNum            =  " + SOut.Long(orthoCase.PatNum) + ", "
                      + "ProvNum           =  " + SOut.Long(orthoCase.ProvNum) + ", "
                      + "ClinicNum         =  " + SOut.Long(orthoCase.ClinicNum) + ", "
                      + "Fee               =  " + SOut.Double(orthoCase.Fee) + ", "
                      + "FeeInsPrimary     =  " + SOut.Double(orthoCase.FeeInsPrimary) + ", "
                      + "FeePat            =  " + SOut.Double(orthoCase.FeePat) + ", "
                      + "BandingDate       =  " + SOut.Date(orthoCase.BandingDate) + ", "
                      + "DebondDate        =  " + SOut.Date(orthoCase.DebondDate) + ", "
                      + "DebondDateExpected=  " + SOut.Date(orthoCase.DebondDateExpected) + ", "
                      + "IsTransfer        =  " + SOut.Bool(orthoCase.IsTransfer) + ", "
                      + "OrthoType         =  " + SOut.Long(orthoCase.OrthoType) + ", "
                      //SecDateTEntry not allowed to change
                      + "SecUserNumEntry   =  " + SOut.Long(orthoCase.SecUserNumEntry) + ", "
                      //SecDateTEdit can only be set by MySQL
                      + "IsActive          =  " + SOut.Bool(orthoCase.IsActive) + ", "
                      + "FeeInsSecondary   =  " + SOut.Double(orthoCase.FeeInsSecondary) + " "
                      + "WHERE OrthoCaseNum = " + SOut.Long(orthoCase.OrthoCaseNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoCase orthoCase, OrthoCase oldOrthoCase)
    {
        var command = "";
        if (orthoCase.PatNum != oldOrthoCase.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(orthoCase.PatNum) + "";
        }

        if (orthoCase.ProvNum != oldOrthoCase.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(orthoCase.ProvNum) + "";
        }

        if (orthoCase.ClinicNum != oldOrthoCase.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(orthoCase.ClinicNum) + "";
        }

        if (orthoCase.Fee != oldOrthoCase.Fee)
        {
            if (command != "") command += ",";
            command += "Fee = " + SOut.Double(orthoCase.Fee) + "";
        }

        if (orthoCase.FeeInsPrimary != oldOrthoCase.FeeInsPrimary)
        {
            if (command != "") command += ",";
            command += "FeeInsPrimary = " + SOut.Double(orthoCase.FeeInsPrimary) + "";
        }

        if (orthoCase.FeePat != oldOrthoCase.FeePat)
        {
            if (command != "") command += ",";
            command += "FeePat = " + SOut.Double(orthoCase.FeePat) + "";
        }

        if (orthoCase.BandingDate.Date != oldOrthoCase.BandingDate.Date)
        {
            if (command != "") command += ",";
            command += "BandingDate = " + SOut.Date(orthoCase.BandingDate) + "";
        }

        if (orthoCase.DebondDate.Date != oldOrthoCase.DebondDate.Date)
        {
            if (command != "") command += ",";
            command += "DebondDate = " + SOut.Date(orthoCase.DebondDate) + "";
        }

        if (orthoCase.DebondDateExpected.Date != oldOrthoCase.DebondDateExpected.Date)
        {
            if (command != "") command += ",";
            command += "DebondDateExpected = " + SOut.Date(orthoCase.DebondDateExpected) + "";
        }

        if (orthoCase.IsTransfer != oldOrthoCase.IsTransfer)
        {
            if (command != "") command += ",";
            command += "IsTransfer = " + SOut.Bool(orthoCase.IsTransfer) + "";
        }

        if (orthoCase.OrthoType != oldOrthoCase.OrthoType)
        {
            if (command != "") command += ",";
            command += "OrthoType = " + SOut.Long(orthoCase.OrthoType) + "";
        }

        //SecDateTEntry not allowed to change
        if (orthoCase.SecUserNumEntry != oldOrthoCase.SecUserNumEntry)
        {
            if (command != "") command += ",";
            command += "SecUserNumEntry = " + SOut.Long(orthoCase.SecUserNumEntry) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (orthoCase.IsActive != oldOrthoCase.IsActive)
        {
            if (command != "") command += ",";
            command += "IsActive = " + SOut.Bool(orthoCase.IsActive) + "";
        }

        if (orthoCase.FeeInsSecondary != oldOrthoCase.FeeInsSecondary)
        {
            if (command != "") command += ",";
            command += "FeeInsSecondary = " + SOut.Double(orthoCase.FeeInsSecondary) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthocase SET " + command
                                          + " WHERE OrthoCaseNum = " + SOut.Long(orthoCase.OrthoCaseNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoCase orthoCase, OrthoCase oldOrthoCase)
    {
        if (orthoCase.PatNum != oldOrthoCase.PatNum) return true;
        if (orthoCase.ProvNum != oldOrthoCase.ProvNum) return true;
        if (orthoCase.ClinicNum != oldOrthoCase.ClinicNum) return true;
        if (orthoCase.Fee != oldOrthoCase.Fee) return true;
        if (orthoCase.FeeInsPrimary != oldOrthoCase.FeeInsPrimary) return true;
        if (orthoCase.FeePat != oldOrthoCase.FeePat) return true;
        if (orthoCase.BandingDate.Date != oldOrthoCase.BandingDate.Date) return true;
        if (orthoCase.DebondDate.Date != oldOrthoCase.DebondDate.Date) return true;
        if (orthoCase.DebondDateExpected.Date != oldOrthoCase.DebondDateExpected.Date) return true;
        if (orthoCase.IsTransfer != oldOrthoCase.IsTransfer) return true;
        if (orthoCase.OrthoType != oldOrthoCase.OrthoType) return true;
        //SecDateTEntry not allowed to change
        if (orthoCase.SecUserNumEntry != oldOrthoCase.SecUserNumEntry) return true;
        //SecDateTEdit can only be set by MySQL
        if (orthoCase.IsActive != oldOrthoCase.IsActive) return true;
        if (orthoCase.FeeInsSecondary != oldOrthoCase.FeeInsSecondary) return true;
        return false;
    }

    public static void Delete(long orthoCaseNum)
    {
        var command = "DELETE FROM orthocase "
                      + "WHERE OrthoCaseNum = " + SOut.Long(orthoCaseNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoCaseNums)
    {
        if (listOrthoCaseNums == null || listOrthoCaseNums.Count == 0) return;
        var command = "DELETE FROM orthocase "
                      + "WHERE OrthoCaseNum IN(" + string.Join(",", listOrthoCaseNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}