using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CovCatCrud
{
    public static List<CovCat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CovCat> TableToList(DataTable table)
    {
        var retVal = new List<CovCat>();
        CovCat covCat;
        foreach (DataRow row in table.Rows)
        {
            covCat = new CovCat();
            covCat.CovCatNum = SIn.Long(row["CovCatNum"].ToString());
            covCat.Description = SIn.String(row["Description"].ToString());
            covCat.DefaultPercent = SIn.Int(row["DefaultPercent"].ToString());
            covCat.CovOrder = SIn.Int(row["CovOrder"].ToString());
            covCat.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            covCat.EbenefitCat = (EbenefitCategory) SIn.Int(row["EbenefitCat"].ToString());
            retVal.Add(covCat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<CovCat> listCovCats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "CovCat";
        var table = new DataTable(tableName);
        table.Columns.Add("CovCatNum");
        table.Columns.Add("Description");
        table.Columns.Add("DefaultPercent");
        table.Columns.Add("CovOrder");
        table.Columns.Add("IsHidden");
        table.Columns.Add("EbenefitCat");
        foreach (var covCat in listCovCats)
            table.Rows.Add(SOut.Long(covCat.CovCatNum), covCat.Description, SOut.Int(covCat.DefaultPercent), SOut.Int(covCat.CovOrder), SOut.Bool(covCat.IsHidden), SOut.Int((int) covCat.EbenefitCat));
        return table;
    }

    public static long Insert(CovCat covCat)
    {
        var command = "INSERT INTO covcat (";

        command += "Description,DefaultPercent,CovOrder,IsHidden,EbenefitCat) VALUES(";

        command +=
            "'" + SOut.String(covCat.Description) + "',"
            + SOut.Int(covCat.DefaultPercent) + ","
            + SOut.Int(covCat.CovOrder) + ","
            + SOut.Bool(covCat.IsHidden) + ","
            + SOut.Int((int) covCat.EbenefitCat) + ")";
        {
            covCat.CovCatNum = Db.NonQ(command, true, "CovCatNum", "covCat");
        }
        return covCat.CovCatNum;
    }

    public static void Update(CovCat covCat)
    {
        var command = "UPDATE covcat SET "
                      + "Description   = '" + SOut.String(covCat.Description) + "', "
                      + "DefaultPercent=  " + SOut.Int(covCat.DefaultPercent) + ", "
                      + "CovOrder      =  " + SOut.Int(covCat.CovOrder) + ", "
                      + "IsHidden      =  " + SOut.Bool(covCat.IsHidden) + ", "
                      + "EbenefitCat   =  " + SOut.Int((int) covCat.EbenefitCat) + " "
                      + "WHERE CovCatNum = " + SOut.Long(covCat.CovCatNum);
        Db.NonQ(command);
    }
}