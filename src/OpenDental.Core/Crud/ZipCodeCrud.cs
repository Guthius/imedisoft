using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ZipCodeCrud
{
    public static List<ZipCode> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ZipCode> TableToList(DataTable table)
    {
        var retVal = new List<ZipCode>();
        ZipCode zipCode;
        foreach (DataRow row in table.Rows)
        {
            zipCode = new ZipCode();
            zipCode.ZipCodeNum = SIn.Long(row["ZipCodeNum"].ToString());
            zipCode.ZipCodeDigits = SIn.String(row["ZipCodeDigits"].ToString());
            zipCode.City = SIn.String(row["City"].ToString());
            zipCode.State = SIn.String(row["State"].ToString());
            zipCode.IsFrequent = SIn.Bool(row["IsFrequent"].ToString());
            retVal.Add(zipCode);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ZipCode> listZipCodes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ZipCode";
        var table = new DataTable(tableName);
        table.Columns.Add("ZipCodeNum");
        table.Columns.Add("ZipCodeDigits");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("IsFrequent");
        foreach (var zipCode in listZipCodes)
            table.Rows.Add(SOut.Long(zipCode.ZipCodeNum), zipCode.ZipCodeDigits, zipCode.City, zipCode.State, SOut.Bool(zipCode.IsFrequent));
        return table;
    }

    public static long Insert(ZipCode zipCode)
    {
        var command = "INSERT INTO zipcode (";

        command += "ZipCodeDigits,City,State,IsFrequent) VALUES(";

        command +=
            "'" + SOut.String(zipCode.ZipCodeDigits) + "',"
            + "'" + SOut.String(zipCode.City) + "',"
            + "'" + SOut.String(zipCode.State) + "',"
            + SOut.Bool(zipCode.IsFrequent) + ")";
        {
            zipCode.ZipCodeNum = Db.NonQ(command, true, "ZipCodeNum", "zipCode");
        }
        return zipCode.ZipCodeNum;
    }

    public static void Update(ZipCode zipCode)
    {
        var command = "UPDATE zipcode SET "
                      + "ZipCodeDigits= '" + SOut.String(zipCode.ZipCodeDigits) + "', "
                      + "City         = '" + SOut.String(zipCode.City) + "', "
                      + "State        = '" + SOut.String(zipCode.State) + "', "
                      + "IsFrequent   =  " + SOut.Bool(zipCode.IsFrequent) + " "
                      + "WHERE ZipCodeNum = " + SOut.Long(zipCode.ZipCodeNum);
        Db.NonQ(command);
    }
}