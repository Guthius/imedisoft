using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<OrthoCase> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoCase> TableToList(DataTable table)
    {
        var retVal = new List<OrthoCase>();
        foreach (DataRow row in table.Rows)
        {
            var orthoCase = new OrthoCase
            {
                OrthoCaseNum = SIn.Long(row["OrthoCaseNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                Fee = SIn.Double(row["Fee"].ToString()),
                FeeInsPrimary = SIn.Double(row["FeeInsPrimary"].ToString()),
                FeePat = SIn.Double(row["FeePat"].ToString()),
                BandingDate = SIn.Date(row["BandingDate"].ToString()),
                DebondDate = SIn.Date(row["DebondDate"].ToString()),
                DebondDateExpected = SIn.Date(row["DebondDateExpected"].ToString()),
                IsTransfer = SIn.Bool(row["IsTransfer"].ToString()),
                OrthoType = SIn.Long(row["OrthoType"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                IsActive = SIn.Bool(row["IsActive"].ToString()),
                FeeInsSecondary = SIn.Double(row["FeeInsSecondary"].ToString())
            };
            retVal.Add(orthoCase);
        }

        return retVal;
    }

    public static long Insert(OrthoCase orthoCase)
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
                                        + "NOW()" + ","
                                        + SOut.Long(orthoCase.SecUserNumEntry) + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + SOut.Bool(orthoCase.IsActive) + ","
                                        + SOut.Double(orthoCase.FeeInsSecondary) + ")";
        {
            orthoCase.OrthoCaseNum = Db.NonQ(command, true, "OrthoCaseNum", "orthoCase");
        }
        return orthoCase.OrthoCaseNum;
    }

    public static void Update(OrthoCase orthoCase, OrthoCase oldOrthoCase)
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

        if (command == "") return;
        command = "UPDATE orthocase SET " + command
                                          + " WHERE OrthoCaseNum = " + SOut.Long(orthoCase.OrthoCaseNum);
        Db.NonQ(command);
    }

    public static void Delete(long orthoCaseNum)
    {
        var command = "DELETE FROM orthocase "
                      + "WHERE OrthoCaseNum = " + SOut.Long(orthoCaseNum);
        Db.NonQ(command);
    }
}