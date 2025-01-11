using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CovSpanCrud
{
    public static List<CovSpan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CovSpan> TableToList(DataTable table)
    {
        var retVal = new List<CovSpan>();
        CovSpan covSpan;
        foreach (DataRow row in table.Rows)
        {
            covSpan = new CovSpan();
            covSpan.CovSpanNum = SIn.Long(row["CovSpanNum"].ToString());
            covSpan.CovCatNum = SIn.Long(row["CovCatNum"].ToString());
            covSpan.FromCode = SIn.String(row["FromCode"].ToString());
            covSpan.ToCode = SIn.String(row["ToCode"].ToString());
            retVal.Add(covSpan);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<CovSpan> listCovSpans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "CovSpan";
        var table = new DataTable(tableName);
        table.Columns.Add("CovSpanNum");
        table.Columns.Add("CovCatNum");
        table.Columns.Add("FromCode");
        table.Columns.Add("ToCode");
        foreach (var covSpan in listCovSpans)
            table.Rows.Add(SOut.Long(covSpan.CovSpanNum), SOut.Long(covSpan.CovCatNum), covSpan.FromCode, covSpan.ToCode);
        return table;
    }

    public static long Insert(CovSpan covSpan)
    {
        var command = "INSERT INTO covspan (";

        command += "CovCatNum,FromCode,ToCode) VALUES(";

        command +=
            SOut.Long(covSpan.CovCatNum) + ","
                                         + "'" + SOut.String(covSpan.FromCode) + "',"
                                         + "'" + SOut.String(covSpan.ToCode) + "')";
        {
            covSpan.CovSpanNum = Db.NonQ(command, true, "CovSpanNum", "covSpan");
        }
        return covSpan.CovSpanNum;
    }

    public static void Update(CovSpan covSpan)
    {
        var command = "UPDATE covspan SET "
                      + "CovCatNum =  " + SOut.Long(covSpan.CovCatNum) + ", "
                      + "FromCode  = '" + SOut.String(covSpan.FromCode) + "', "
                      + "ToCode    = '" + SOut.String(covSpan.ToCode) + "' "
                      + "WHERE CovSpanNum = " + SOut.Long(covSpan.CovSpanNum);
        Db.NonQ(command);
    }
}