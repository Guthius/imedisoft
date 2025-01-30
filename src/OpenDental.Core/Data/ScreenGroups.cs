using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ScreenGroups
{
    public static List<ScreenGroup> Refresh(DateTime dateFrom, DateTime dateTo)
    {
        return ScreenGroupCrud.SelectMany(
            "SELECT * from screengroup " +
            "WHERE SGDate >= " + SOut.DateTime(dateFrom) + " " +
            "AND SGDate < " + SOut.DateTime(dateTo.AddDays(1)) + " " +
            "ORDER BY SGDate,ScreenGroupNum");
    }

    public static ScreenGroup GetScreenGroup(long screenGroupNum)
    {
        return ScreenGroupCrud.SelectOne("SELECT * FROM screengroup WHERE ScreenGroupNum=" + screenGroupNum);
    }

    public static void Insert(ScreenGroup screenGroup)
    {
        ScreenGroupCrud.Insert(screenGroup);
    }

    public static void Update(ScreenGroup screenGroup)
    {
        ScreenGroupCrud.Update(screenGroup);
    }

    public static void Delete(ScreenGroup screenGroup)
    {
        var dataTable = DataCore.GetTable("SELECT SheetNum FROM screen WHERE ScreenGroupNum=" + screenGroup.ScreenGroupNum + " AND SheetNum!=0");

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            Sheets.Delete(SIn.Long(dataTable.Rows[i]["SheetNum"].ToString()));
        }

        Db.NonQ("DELETE FROM screen WHERE ScreenGroupNum =" + screenGroup.ScreenGroupNum);
        Db.NonQ("DELETE FROM screengroup WHERE ScreenGroupNum =" + screenGroup.ScreenGroupNum);
    }
}