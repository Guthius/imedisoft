using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ScreenGroups
{
    
    public static List<ScreenGroup> Refresh(DateTime dateFrom, DateTime dateTo)
    {
        var command =
            "SELECT * from screengroup "
            + "WHERE SGDate >= " + SOut.DateT(dateFrom) + " "
            + "AND SGDate < " + SOut.DateT(dateTo.AddDays(1)) + " " //Was including entries form the next day. Changed from <= to <.
            //added one day since it's calculated based on midnight.
            + "ORDER BY SGDate,ScreenGroupNum";
        return ScreenGroupCrud.SelectMany(command);
    }

    public static ScreenGroup GetScreenGroup(long screenGroupNum)
    {
        var command =
            "SELECT * FROM screengroup WHERE ScreenGroupNum=" + SOut.Long(screenGroupNum);
        return ScreenGroupCrud.SelectOne(command);
    }

    
    public static long Insert(ScreenGroup screenGroup)
    {
        return ScreenGroupCrud.Insert(screenGroup);
    }

    
    public static void Update(ScreenGroup screenGroup)
    {
        ScreenGroupCrud.Update(screenGroup);
    }

    ///<summary>This will also delete all screen items, so may need to ask user first.</summary>
    public static void Delete(ScreenGroup screenGroup)
    {
        var command = "SELECT SheetNum FROM screen WHERE ScreenGroupNum=" + SOut.Long(screenGroup.ScreenGroupNum) + " AND SheetNum!=0";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) //Delete any attached sheets if the screen gets deleted.
            Sheets.Delete(SIn.Long(table.Rows[i]["SheetNum"].ToString()));
        command = "DELETE FROM screen WHERE ScreenGroupNum ='" + SOut.Long(screenGroup.ScreenGroupNum) + "'";
        Db.NonQ(command);
        command = "DELETE FROM screengroup WHERE ScreenGroupNum ='" + SOut.Long(screenGroup.ScreenGroupNum) + "'";
        Db.NonQ(command);
    }
}