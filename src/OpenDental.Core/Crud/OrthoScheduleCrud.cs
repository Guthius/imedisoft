using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoScheduleCrud
{
    public static OrthoSchedule SelectOne(long orthoScheduleNum)
    {
        var command = "SELECT * FROM orthoschedule "
                      + "WHERE OrthoScheduleNum = " + SOut.Long(orthoScheduleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoSchedule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoSchedule> TableToList(DataTable table)
    {
        var retVal = new List<OrthoSchedule>();
        foreach (DataRow row in table.Rows)
        {
            var orthoSchedule = new OrthoSchedule
            {
                OrthoScheduleNum = SIn.Long(row["OrthoScheduleNum"].ToString()),
                BandingDateOverride = SIn.Date(row["BandingDateOverride"].ToString()),
                DebondDateOverride = SIn.Date(row["DebondDateOverride"].ToString()),
                BandingAmount = SIn.Double(row["BandingAmount"].ToString()),
                VisitAmount = SIn.Double(row["VisitAmount"].ToString()),
                DebondAmount = SIn.Double(row["DebondAmount"].ToString()),
                IsActive = SIn.Bool(row["IsActive"].ToString())
            };
            retVal.Add(orthoSchedule);
        }

        return retVal;
    }

    public static long Insert(OrthoSchedule orthoSchedule)
    {
        var command = "INSERT INTO orthoschedule (";

        command += "BandingDateOverride,DebondDateOverride,BandingAmount,VisitAmount,DebondAmount,IsActive) VALUES(";

        command +=
            SOut.Date(orthoSchedule.BandingDateOverride) + ","
                                                         + SOut.Date(orthoSchedule.DebondDateOverride) + ","
                                                         + SOut.Double(orthoSchedule.BandingAmount) + ","
                                                         + SOut.Double(orthoSchedule.VisitAmount) + ","
                                                         + SOut.Double(orthoSchedule.DebondAmount) + ","
                                                         + SOut.Bool(orthoSchedule.IsActive) + ")";
        //SecDateTEdit can only be set by MySQL

        orthoSchedule.OrthoScheduleNum = Db.NonQ(command, true, "OrthoScheduleNum", "orthoSchedule");
        return orthoSchedule.OrthoScheduleNum;
    }

    public static void Update(OrthoSchedule orthoSchedule, OrthoSchedule oldOrthoSchedule)
    {
        var command = "";
        if (orthoSchedule.BandingDateOverride.Date != oldOrthoSchedule.BandingDateOverride.Date)
        {
            if (command != "") command += ",";
            command += "BandingDateOverride = " + SOut.Date(orthoSchedule.BandingDateOverride) + "";
        }

        if (orthoSchedule.DebondDateOverride.Date != oldOrthoSchedule.DebondDateOverride.Date)
        {
            if (command != "") command += ",";
            command += "DebondDateOverride = " + SOut.Date(orthoSchedule.DebondDateOverride) + "";
        }

        if (orthoSchedule.BandingAmount != oldOrthoSchedule.BandingAmount)
        {
            if (command != "") command += ",";
            command += "BandingAmount = " + SOut.Double(orthoSchedule.BandingAmount) + "";
        }

        if (orthoSchedule.VisitAmount != oldOrthoSchedule.VisitAmount)
        {
            if (command != "") command += ",";
            command += "VisitAmount = " + SOut.Double(orthoSchedule.VisitAmount) + "";
        }

        if (orthoSchedule.DebondAmount != oldOrthoSchedule.DebondAmount)
        {
            if (command != "") command += ",";
            command += "DebondAmount = " + SOut.Double(orthoSchedule.DebondAmount) + "";
        }

        if (orthoSchedule.IsActive != oldOrthoSchedule.IsActive)
        {
            if (command != "") command += ",";
            command += "IsActive = " + SOut.Bool(orthoSchedule.IsActive) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return;
        command = "UPDATE orthoschedule SET " + command
                                              + " WHERE OrthoScheduleNum = " + SOut.Long(orthoSchedule.OrthoScheduleNum);
        Db.NonQ(command);
    }

    public static void Delete(long orthoScheduleNum)
    {
        var command = "DELETE FROM orthoschedule "
                      + "WHERE OrthoScheduleNum = " + SOut.Long(orthoScheduleNum);
        Db.NonQ(command);
    }
}