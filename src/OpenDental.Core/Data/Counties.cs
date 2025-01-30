using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Counties
{
    public static List<County> Refresh(string name)
    {
        var counties = CountyCrud.SelectMany("SELECT * from county WHERE CountyName LIKE '" + SOut.String(name) + "%' ORDER BY CountyName");

        foreach (var county in counties)
        {
            county.CountyNameOld = county.CountyName;
        }

        return counties;
    }

    public static List<string> GetListNames()
    {
        var dataTable = DataCore.GetTable("SELECT CountyName from county ORDER BY CountyName");

        var countyNames = new List<string>();
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            countyNames.Add(SIn.String(dataTable.Rows[i]["CountyName"].ToString()));
        }

        return countyNames;
    }

    public static void Insert(County county)
    {
        CountyCrud.Insert(county);
    }

    public static void Update(County county)
    {
        Db.NonQ("UPDATE county SET CountyName ='" + SOut.String(county.CountyName) + "', CountyCode ='" + SOut.String(county.CountyCode) + "' WHERE CountyName = '" + SOut.String(county.CountyNameOld) + "'");
        Db.NonQ("UPDATE patient SET County ='" + SOut.String(county.CountyName) + "' WHERE County = '" + SOut.String(county.CountyNameOld) + "'");
    }

    public static void Delete(County county)
    {
        Db.NonQ("DELETE from county WHERE CountyName = '" + SOut.String(county.CountyName) + "'");
    }

    public static string UsedBy(string countyName)
    {
        var dataTable = DataCore.GetTable("SELECT LName,FName FROM patient WHERE County = '" + SOut.String(countyName) + "'");
        if (dataTable.Rows.Count == 0)
        {
            return string.Empty;
        }

        var names = "";

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            names += SIn.String(dataTable.Rows[i][0].ToString()) + ", " + SIn.String(dataTable.Rows[i][1].ToString());
            if (i < dataTable.Rows.Count - 1)
            {
                names += "\r";
            }
        }

        return names;
    }

    public static bool DoesExist(string countyName)
    {
        var dataTable = DataCore.GetTable("SELECT * FROM county WHERE CountyName = '" + SOut.String(countyName) + "'");

        return dataTable.Rows.Count != 0;
    }
}