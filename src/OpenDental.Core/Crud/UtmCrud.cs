#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UtmCrud
{
    public static Utm SelectOne(long utmNum)
    {
        var command = "SELECT * FROM utm "
                      + "WHERE UtmNum = " + SOut.Long(utmNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Utm SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Utm> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Utm> TableToList(DataTable table)
    {
        var retVal = new List<Utm>();
        Utm utm;
        foreach (DataRow row in table.Rows)
        {
            utm = new Utm();
            utm.UtmNum = SIn.Long(row["UtmNum"].ToString());
            utm.CampaignName = SIn.String(row["CampaignName"].ToString());
            utm.MediumInfo = SIn.String(row["MediumInfo"].ToString());
            utm.SourceInfo = SIn.String(row["SourceInfo"].ToString());
            retVal.Add(utm);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Utm> listUtms, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Utm";
        var table = new DataTable(tableName);
        table.Columns.Add("UtmNum");
        table.Columns.Add("CampaignName");
        table.Columns.Add("MediumInfo");
        table.Columns.Add("SourceInfo");
        foreach (var utm in listUtms)
            table.Rows.Add(SOut.Long(utm.UtmNum), utm.CampaignName, utm.MediumInfo, utm.SourceInfo);
        return table;
    }

    public static long Insert(Utm utm)
    {
        return Insert(utm, false);
    }

    public static long Insert(Utm utm, bool useExistingPK)
    {
        var command = "INSERT INTO utm (";

        command += "CampaignName,MediumInfo,SourceInfo) VALUES(";

        command +=
            "'" + SOut.String(utm.CampaignName) + "',"
            + "'" + SOut.String(utm.MediumInfo) + "',"
            + "'" + SOut.String(utm.SourceInfo) + "')";
        {
            utm.UtmNum = Db.NonQ(command, true, "UtmNum", "utm");
        }
        return utm.UtmNum;
    }

    public static long InsertNoCache(Utm utm)
    {
        return InsertNoCache(utm, false);
    }

    public static long InsertNoCache(Utm utm, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO utm (";
        if (isRandomKeys || useExistingPK) command += "UtmNum,";
        command += "CampaignName,MediumInfo,SourceInfo) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(utm.UtmNum) + ",";
        command +=
            "'" + SOut.String(utm.CampaignName) + "',"
            + "'" + SOut.String(utm.MediumInfo) + "',"
            + "'" + SOut.String(utm.SourceInfo) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            utm.UtmNum = Db.NonQ(command, true, "UtmNum", "utm");
        return utm.UtmNum;
    }

    public static void Update(Utm utm)
    {
        var command = "UPDATE utm SET "
                      + "CampaignName= '" + SOut.String(utm.CampaignName) + "', "
                      + "MediumInfo  = '" + SOut.String(utm.MediumInfo) + "', "
                      + "SourceInfo  = '" + SOut.String(utm.SourceInfo) + "' "
                      + "WHERE UtmNum = " + SOut.Long(utm.UtmNum);
        Db.NonQ(command);
    }

    public static bool Update(Utm utm, Utm oldUtm)
    {
        var command = "";
        if (utm.CampaignName != oldUtm.CampaignName)
        {
            if (command != "") command += ",";
            command += "CampaignName = '" + SOut.String(utm.CampaignName) + "'";
        }

        if (utm.MediumInfo != oldUtm.MediumInfo)
        {
            if (command != "") command += ",";
            command += "MediumInfo = '" + SOut.String(utm.MediumInfo) + "'";
        }

        if (utm.SourceInfo != oldUtm.SourceInfo)
        {
            if (command != "") command += ",";
            command += "SourceInfo = '" + SOut.String(utm.SourceInfo) + "'";
        }

        if (command == "") return false;
        command = "UPDATE utm SET " + command
                                    + " WHERE UtmNum = " + SOut.Long(utm.UtmNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Utm utm, Utm oldUtm)
    {
        if (utm.CampaignName != oldUtm.CampaignName) return true;
        if (utm.MediumInfo != oldUtm.MediumInfo) return true;
        if (utm.SourceInfo != oldUtm.SourceInfo) return true;
        return false;
    }

    public static void Delete(long utmNum)
    {
        var command = "DELETE FROM utm "
                      + "WHERE UtmNum = " + SOut.Long(utmNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUtmNums)
    {
        if (listUtmNums == null || listUtmNums.Count == 0) return;
        var command = "DELETE FROM utm "
                      + "WHERE UtmNum IN(" + string.Join(",", listUtmNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}