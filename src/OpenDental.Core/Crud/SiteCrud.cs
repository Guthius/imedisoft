#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SiteCrud
{
    public static Site SelectOne(long siteNum)
    {
        var command = "SELECT * FROM site "
                      + "WHERE SiteNum = " + SOut.Long(siteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Site SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Site> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Site> TableToList(DataTable table)
    {
        var retVal = new List<Site>();
        Site site;
        foreach (DataRow row in table.Rows)
        {
            site = new Site();
            site.SiteNum = SIn.Long(row["SiteNum"].ToString());
            site.Description = SIn.String(row["Description"].ToString());
            site.Note = SIn.String(row["Note"].ToString());
            site.Address = SIn.String(row["Address"].ToString());
            site.Address2 = SIn.String(row["Address2"].ToString());
            site.City = SIn.String(row["City"].ToString());
            site.State = SIn.String(row["State"].ToString());
            site.Zip = SIn.String(row["Zip"].ToString());
            site.ProvNum = SIn.Long(row["ProvNum"].ToString());
            site.PlaceService = (PlaceOfService) SIn.Int(row["PlaceService"].ToString());
            retVal.Add(site);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Site> listSites, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Site";
        var table = new DataTable(tableName);
        table.Columns.Add("SiteNum");
        table.Columns.Add("Description");
        table.Columns.Add("Note");
        table.Columns.Add("Address");
        table.Columns.Add("Address2");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("Zip");
        table.Columns.Add("ProvNum");
        table.Columns.Add("PlaceService");
        foreach (var site in listSites)
            table.Rows.Add(SOut.Long(site.SiteNum), site.Description, site.Note, site.Address, site.Address2, site.City, site.State, site.Zip, SOut.Long(site.ProvNum), SOut.Int((int) site.PlaceService));
        return table;
    }

    public static long Insert(Site site)
    {
        return Insert(site, false);
    }

    public static long Insert(Site site, bool useExistingPK)
    {
        var command = "INSERT INTO site (";

        command += "Description,Note,Address,Address2,City,State,Zip,ProvNum,PlaceService) VALUES(";

        command +=
            "'" + SOut.String(site.Description) + "',"
            + DbHelper.ParamChar + "paramNote,"
            + "'" + SOut.String(site.Address) + "',"
            + "'" + SOut.String(site.Address2) + "',"
            + "'" + SOut.String(site.City) + "',"
            + "'" + SOut.String(site.State) + "',"
            + "'" + SOut.String(site.Zip) + "',"
            + SOut.Long(site.ProvNum) + ","
            + SOut.Int((int) site.PlaceService) + ")";
        if (site.Note == null) site.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(site.Note));
        {
            site.SiteNum = Db.NonQ(command, true, "SiteNum", "site", paramNote);
        }
        return site.SiteNum;
    }

    public static long InsertNoCache(Site site)
    {
        return InsertNoCache(site, false);
    }

    public static long InsertNoCache(Site site, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO site (";
        if (isRandomKeys || useExistingPK) command += "SiteNum,";
        command += "Description,Note,Address,Address2,City,State,Zip,ProvNum,PlaceService) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(site.SiteNum) + ",";
        command +=
            "'" + SOut.String(site.Description) + "',"
            + DbHelper.ParamChar + "paramNote,"
            + "'" + SOut.String(site.Address) + "',"
            + "'" + SOut.String(site.Address2) + "',"
            + "'" + SOut.String(site.City) + "',"
            + "'" + SOut.String(site.State) + "',"
            + "'" + SOut.String(site.Zip) + "',"
            + SOut.Long(site.ProvNum) + ","
            + SOut.Int((int) site.PlaceService) + ")";
        if (site.Note == null) site.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(site.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            site.SiteNum = Db.NonQ(command, true, "SiteNum", "site", paramNote);
        return site.SiteNum;
    }

    public static void Update(Site site)
    {
        var command = "UPDATE site SET "
                      + "Description = '" + SOut.String(site.Description) + "', "
                      + "Note        =  " + DbHelper.ParamChar + "paramNote, "
                      + "Address     = '" + SOut.String(site.Address) + "', "
                      + "Address2    = '" + SOut.String(site.Address2) + "', "
                      + "City        = '" + SOut.String(site.City) + "', "
                      + "State       = '" + SOut.String(site.State) + "', "
                      + "Zip         = '" + SOut.String(site.Zip) + "', "
                      + "ProvNum     =  " + SOut.Long(site.ProvNum) + ", "
                      + "PlaceService=  " + SOut.Int((int) site.PlaceService) + " "
                      + "WHERE SiteNum = " + SOut.Long(site.SiteNum);
        if (site.Note == null) site.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(site.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Site site, Site oldSite)
    {
        var command = "";
        if (site.Description != oldSite.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(site.Description) + "'";
        }

        if (site.Note != oldSite.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (site.Address != oldSite.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.String(site.Address) + "'";
        }

        if (site.Address2 != oldSite.Address2)
        {
            if (command != "") command += ",";
            command += "Address2 = '" + SOut.String(site.Address2) + "'";
        }

        if (site.City != oldSite.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(site.City) + "'";
        }

        if (site.State != oldSite.State)
        {
            if (command != "") command += ",";
            command += "State = '" + SOut.String(site.State) + "'";
        }

        if (site.Zip != oldSite.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(site.Zip) + "'";
        }

        if (site.ProvNum != oldSite.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(site.ProvNum) + "";
        }

        if (site.PlaceService != oldSite.PlaceService)
        {
            if (command != "") command += ",";
            command += "PlaceService = " + SOut.Int((int) site.PlaceService) + "";
        }

        if (command == "") return false;
        if (site.Note == null) site.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(site.Note));
        command = "UPDATE site SET " + command
                                     + " WHERE SiteNum = " + SOut.Long(site.SiteNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(Site site, Site oldSite)
    {
        if (site.Description != oldSite.Description) return true;
        if (site.Note != oldSite.Note) return true;
        if (site.Address != oldSite.Address) return true;
        if (site.Address2 != oldSite.Address2) return true;
        if (site.City != oldSite.City) return true;
        if (site.State != oldSite.State) return true;
        if (site.Zip != oldSite.Zip) return true;
        if (site.ProvNum != oldSite.ProvNum) return true;
        if (site.PlaceService != oldSite.PlaceService) return true;
        return false;
    }

    public static void Delete(long siteNum)
    {
        var command = "DELETE FROM site "
                      + "WHERE SiteNum = " + SOut.Long(siteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSiteNums)
    {
        if (listSiteNums == null || listSiteNums.Count == 0) return;
        var command = "DELETE FROM site "
                      + "WHERE SiteNum IN(" + string.Join(",", listSiteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}