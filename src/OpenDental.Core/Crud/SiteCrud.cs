using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SiteCrud
{
    public static List<Site> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Site> TableToList(DataTable table)
    {
        var retVal = new List<Site>();
        foreach (DataRow row in table.Rows)
        {
            var site = new Site
            {
                SiteNum = SIn.Long(row["SiteNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                Address2 = SIn.String(row["Address2"].ToString()),
                City = SIn.String(row["City"].ToString()),
                State = SIn.String(row["State"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                PlaceService = (PlaceOfService) SIn.Int(row["PlaceService"].ToString())
            };
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

    public static void Insert(Site site)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(site.Note));
        {
            site.SiteNum = Db.NonQ(command, true, "SiteNum", "site", paramNote);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(site.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Delete(long siteNum)
    {
        var command = "DELETE FROM site "
                      + "WHERE SiteNum = " + SOut.Long(siteNum);
        Db.NonQ(command);
    }
}