using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoChartCrud
{
    public static List<OrthoChart> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoChart> TableToList(DataTable table)
    {
        var retVal = new List<OrthoChart>();
        foreach (DataRow row in table.Rows)
        {
            var orthoChart = new OrthoChart
            {
                OrthoChartNum = SIn.Long(row["OrthoChartNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateService = SIn.Date(row["DateService"].ToString()),
                FieldName = SIn.String(row["FieldName"].ToString()),
                FieldValue = SIn.String(row["FieldValue"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                OrthoChartRowNum = SIn.Long(row["OrthoChartRowNum"].ToString())
            };
            retVal.Add(orthoChart);
        }

        return retVal;
    }

    public static void Insert(OrthoChart orthoChart)
    {
        var command = "INSERT INTO orthochart (";

        command += "PatNum,DateService,FieldName,FieldValue,UserNum,ProvNum,OrthoChartRowNum) VALUES(";

        command +=
            SOut.Long(orthoChart.PatNum) + ","
                                         + SOut.Date(orthoChart.DateService) + ","
                                         + "'" + SOut.String(orthoChart.FieldName) + "',"
                                         + DbHelper.ParamChar + "paramFieldValue,"
                                         + SOut.Long(orthoChart.UserNum) + ","
                                         + SOut.Long(orthoChart.ProvNum) + ","
                                         + SOut.Long(orthoChart.OrthoChartRowNum) + ")";
        if (orthoChart.FieldValue == null) orthoChart.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", SOut.StringParam(orthoChart.FieldValue));
        {
            orthoChart.OrthoChartNum = Db.NonQ(command, true, "OrthoChartNum", "orthoChart", paramFieldValue);
        }
    }
}