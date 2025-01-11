#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ScreenGroupCrud
{
    public static ScreenGroup SelectOne(long screenGroupNum)
    {
        var command = "SELECT * FROM screengroup "
                      + "WHERE ScreenGroupNum = " + SOut.Long(screenGroupNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ScreenGroup SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ScreenGroup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ScreenGroup> TableToList(DataTable table)
    {
        var retVal = new List<ScreenGroup>();
        ScreenGroup screenGroup;
        foreach (DataRow row in table.Rows)
        {
            screenGroup = new ScreenGroup();
            screenGroup.ScreenGroupNum = SIn.Long(row["ScreenGroupNum"].ToString());
            screenGroup.Description = SIn.String(row["Description"].ToString());
            screenGroup.SGDate = SIn.Date(row["SGDate"].ToString());
            screenGroup.ProvName = SIn.String(row["ProvName"].ToString());
            screenGroup.ProvNum = SIn.Long(row["ProvNum"].ToString());
            screenGroup.PlaceService = (PlaceOfService) SIn.Int(row["PlaceService"].ToString());
            screenGroup.County = SIn.String(row["County"].ToString());
            screenGroup.GradeSchool = SIn.String(row["GradeSchool"].ToString());
            screenGroup.SheetDefNum = SIn.Long(row["SheetDefNum"].ToString());
            retVal.Add(screenGroup);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ScreenGroup> listScreenGroups, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ScreenGroup";
        var table = new DataTable(tableName);
        table.Columns.Add("ScreenGroupNum");
        table.Columns.Add("Description");
        table.Columns.Add("SGDate");
        table.Columns.Add("ProvName");
        table.Columns.Add("ProvNum");
        table.Columns.Add("PlaceService");
        table.Columns.Add("County");
        table.Columns.Add("GradeSchool");
        table.Columns.Add("SheetDefNum");
        foreach (var screenGroup in listScreenGroups)
            table.Rows.Add(SOut.Long(screenGroup.ScreenGroupNum), screenGroup.Description, SOut.DateT(screenGroup.SGDate, false), screenGroup.ProvName, SOut.Long(screenGroup.ProvNum), SOut.Int((int) screenGroup.PlaceService), screenGroup.County, screenGroup.GradeSchool, SOut.Long(screenGroup.SheetDefNum));
        return table;
    }

    public static long Insert(ScreenGroup screenGroup)
    {
        return Insert(screenGroup, false);
    }

    public static long Insert(ScreenGroup screenGroup, bool useExistingPK)
    {
        var command = "INSERT INTO screengroup (";

        command += "Description,SGDate,ProvName,ProvNum,PlaceService,County,GradeSchool,SheetDefNum) VALUES(";

        command +=
            "'" + SOut.String(screenGroup.Description) + "',"
            + SOut.Date(screenGroup.SGDate) + ","
            + "'" + SOut.String(screenGroup.ProvName) + "',"
            + SOut.Long(screenGroup.ProvNum) + ","
            + SOut.Int((int) screenGroup.PlaceService) + ","
            + "'" + SOut.String(screenGroup.County) + "',"
            + "'" + SOut.String(screenGroup.GradeSchool) + "',"
            + SOut.Long(screenGroup.SheetDefNum) + ")";
        {
            screenGroup.ScreenGroupNum = Db.NonQ(command, true, "ScreenGroupNum", "screenGroup");
        }
        return screenGroup.ScreenGroupNum;
    }

    public static long InsertNoCache(ScreenGroup screenGroup)
    {
        return InsertNoCache(screenGroup, false);
    }

    public static long InsertNoCache(ScreenGroup screenGroup, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO screengroup (";
        if (isRandomKeys || useExistingPK) command += "ScreenGroupNum,";
        command += "Description,SGDate,ProvName,ProvNum,PlaceService,County,GradeSchool,SheetDefNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(screenGroup.ScreenGroupNum) + ",";
        command +=
            "'" + SOut.String(screenGroup.Description) + "',"
            + SOut.Date(screenGroup.SGDate) + ","
            + "'" + SOut.String(screenGroup.ProvName) + "',"
            + SOut.Long(screenGroup.ProvNum) + ","
            + SOut.Int((int) screenGroup.PlaceService) + ","
            + "'" + SOut.String(screenGroup.County) + "',"
            + "'" + SOut.String(screenGroup.GradeSchool) + "',"
            + SOut.Long(screenGroup.SheetDefNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            screenGroup.ScreenGroupNum = Db.NonQ(command, true, "ScreenGroupNum", "screenGroup");
        return screenGroup.ScreenGroupNum;
    }

    public static void Update(ScreenGroup screenGroup)
    {
        var command = "UPDATE screengroup SET "
                      + "Description   = '" + SOut.String(screenGroup.Description) + "', "
                      + "SGDate        =  " + SOut.Date(screenGroup.SGDate) + ", "
                      + "ProvName      = '" + SOut.String(screenGroup.ProvName) + "', "
                      + "ProvNum       =  " + SOut.Long(screenGroup.ProvNum) + ", "
                      + "PlaceService  =  " + SOut.Int((int) screenGroup.PlaceService) + ", "
                      + "County        = '" + SOut.String(screenGroup.County) + "', "
                      + "GradeSchool   = '" + SOut.String(screenGroup.GradeSchool) + "', "
                      + "SheetDefNum   =  " + SOut.Long(screenGroup.SheetDefNum) + " "
                      + "WHERE ScreenGroupNum = " + SOut.Long(screenGroup.ScreenGroupNum);
        Db.NonQ(command);
    }

    public static bool Update(ScreenGroup screenGroup, ScreenGroup oldScreenGroup)
    {
        var command = "";
        if (screenGroup.Description != oldScreenGroup.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(screenGroup.Description) + "'";
        }

        if (screenGroup.SGDate.Date != oldScreenGroup.SGDate.Date)
        {
            if (command != "") command += ",";
            command += "SGDate = " + SOut.Date(screenGroup.SGDate) + "";
        }

        if (screenGroup.ProvName != oldScreenGroup.ProvName)
        {
            if (command != "") command += ",";
            command += "ProvName = '" + SOut.String(screenGroup.ProvName) + "'";
        }

        if (screenGroup.ProvNum != oldScreenGroup.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(screenGroup.ProvNum) + "";
        }

        if (screenGroup.PlaceService != oldScreenGroup.PlaceService)
        {
            if (command != "") command += ",";
            command += "PlaceService = " + SOut.Int((int) screenGroup.PlaceService) + "";
        }

        if (screenGroup.County != oldScreenGroup.County)
        {
            if (command != "") command += ",";
            command += "County = '" + SOut.String(screenGroup.County) + "'";
        }

        if (screenGroup.GradeSchool != oldScreenGroup.GradeSchool)
        {
            if (command != "") command += ",";
            command += "GradeSchool = '" + SOut.String(screenGroup.GradeSchool) + "'";
        }

        if (screenGroup.SheetDefNum != oldScreenGroup.SheetDefNum)
        {
            if (command != "") command += ",";
            command += "SheetDefNum = " + SOut.Long(screenGroup.SheetDefNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE screengroup SET " + command
                                            + " WHERE ScreenGroupNum = " + SOut.Long(screenGroup.ScreenGroupNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ScreenGroup screenGroup, ScreenGroup oldScreenGroup)
    {
        if (screenGroup.Description != oldScreenGroup.Description) return true;
        if (screenGroup.SGDate.Date != oldScreenGroup.SGDate.Date) return true;
        if (screenGroup.ProvName != oldScreenGroup.ProvName) return true;
        if (screenGroup.ProvNum != oldScreenGroup.ProvNum) return true;
        if (screenGroup.PlaceService != oldScreenGroup.PlaceService) return true;
        if (screenGroup.County != oldScreenGroup.County) return true;
        if (screenGroup.GradeSchool != oldScreenGroup.GradeSchool) return true;
        if (screenGroup.SheetDefNum != oldScreenGroup.SheetDefNum) return true;
        return false;
    }

    public static void Delete(long screenGroupNum)
    {
        var command = "DELETE FROM screengroup "
                      + "WHERE ScreenGroupNum = " + SOut.Long(screenGroupNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listScreenGroupNums)
    {
        if (listScreenGroupNums == null || listScreenGroupNums.Count == 0) return;
        var command = "DELETE FROM screengroup "
                      + "WHERE ScreenGroupNum IN(" + string.Join(",", listScreenGroupNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}