using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Counties
{
    ///<summary>Gets county names similar to the one provided.</summary>
    public static List<County> Refresh(string name)
    {
        var command =
            "SELECT * from county "
            + "WHERE CountyName LIKE '" + SOut.String(name) + "%' "
            + "ORDER BY CountyName";
        var listCounties = CountyCrud.SelectMany(command);
        for (var i = 0; i < listCounties.Count; i++) listCounties[i].CountyNameOld = listCounties[i].CountyName;
        return listCounties;
    }

    /// <summary>
    ///     Gets an array of strings containing all the counties in alphabetical order.  Used for the screening interface
    ///     which must be simpler than the usual interface.
    /// </summary>
    public static List<string> GetListNames()
    {
        var command =
            "SELECT CountyName from county "
            + "ORDER BY CountyName";
        var table = DataCore.GetTable(command);
        var listStringNames = new List<string>();
        for (var i = 0; i < table.Rows.Count; i++) listStringNames.Add(SIn.String(table.Rows[i]["CountyName"].ToString()));
        return listStringNames;
    }

    ///<summary>Need to make sure Countyname not already in db.</summary>
    public static long Insert(County county)
    {
        return CountyCrud.Insert(county);
    }

    /// <summary>
    ///     Updates the Countyname and code in the County table, and also updates all patients that were using the
    ///     oldCounty name.
    /// </summary>
    public static void Update(County county)
    {
        //Can't use CRUD here because we're updating by the OldCountyName
        var command = "UPDATE county SET "
                      + "CountyName ='" + SOut.String(county.CountyName) + "'"
                      + ",CountyCode ='" + SOut.String(county.CountyCode) + "'"
                      + " WHERE CountyName = '" + SOut.String(county.CountyNameOld) + "'";
        Db.NonQ(command);
        //then, update all patients using that County
        command = "UPDATE patient SET "
                  + "County ='" + SOut.String(county.CountyName) + "'"
                  + " WHERE County = '" + SOut.String(county.CountyNameOld) + "'";
        Db.NonQ(command);
    }

    ///<summary>Must run UsedBy before running this.</summary>
    public static void Delete(County county)
    {
        var command = "DELETE from county WHERE CountyName = '" + SOut.String(county.CountyName) + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Use before DeleteCur to determine if this County name is in use. Returns a formatted string that can be used
    ///     to quickly display the names of all patients using the Countyname.
    /// </summary>
    public static string UsedBy(string countyName)
    {
        var command =
            "SELECT LName,FName FROM patient "
            + "WHERE County = '" + SOut.String(countyName) + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return "";
        var retVal = "";
        for (var i = 0; i < table.Rows.Count; i++)
        {
            retVal += SIn.String(table.Rows[i][0].ToString()) + ", "
                                                              + SIn.String(table.Rows[i][1].ToString());
            if (i < table.Rows.Count - 1) //if not the last row
                retVal += "\r";
        }

        return retVal;
    }

    /// <summary>
    ///     Use before Insert to determine if this County name already exists. Also used when closing patient edit window
    ///     to validate that the Countyname exists.
    /// </summary>
    public static bool DoesExist(string countyName)
    {
        var command =
            "SELECT * FROM county "
            + "WHERE CountyName = '" + SOut.String(countyName) + "' ";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return false;
        return true;
    }
}