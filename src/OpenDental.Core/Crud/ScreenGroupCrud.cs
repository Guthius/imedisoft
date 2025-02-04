using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ScreenGroupCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var screenGroup = new ScreenGroup
            {
                ScreenGroupNum = SIn.Long(row["ScreenGroupNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                SGDate = SIn.Date(row["SGDate"].ToString()),
                ProvName = SIn.String(row["ProvName"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                PlaceService = (PlaceOfService) SIn.Int(row["PlaceService"].ToString()),
                County = SIn.String(row["County"].ToString()),
                GradeSchool = SIn.String(row["GradeSchool"].ToString()),
                SheetDefNum = SIn.Long(row["SheetDefNum"].ToString())
            };
            retVal.Add(screenGroup);
        }

        return retVal;
    }

    public static void Insert(ScreenGroup screenGroup)
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
}