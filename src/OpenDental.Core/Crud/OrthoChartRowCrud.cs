using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoChartRowCrud
{
    public static List<OrthoChartRow> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoChartRow> TableToList(DataTable table)
    {
        var retVal = new List<OrthoChartRow>();
        foreach (DataRow row in table.Rows)
        {
            var orthoChartRow = new OrthoChartRow
            {
                OrthoChartRowNum = SIn.Long(row["OrthoChartRowNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateTimeService = SIn.DateTime(row["DateTimeService"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                Signature = SIn.String(row["Signature"].ToString())
            };
            retVal.Add(orthoChartRow);
        }

        return retVal;
    }

    public static void Insert(OrthoChartRow orthoChartRow)
    {
        var command = "INSERT INTO orthochartrow (";

        command += "PatNum,DateTimeService,UserNum,ProvNum,Signature) VALUES(";

        command +=
            SOut.Long(orthoChartRow.PatNum) + ","
                                            + SOut.DateTime(orthoChartRow.DateTimeService) + ","
                                            + SOut.Long(orthoChartRow.UserNum) + ","
                                            + SOut.Long(orthoChartRow.ProvNum) + ","
                                            + DbHelper.ParamChar + "paramSignature)";
        if (orthoChartRow.Signature == null) orthoChartRow.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(orthoChartRow.Signature));
        {
            orthoChartRow.OrthoChartRowNum = Db.NonQ(command, true, "OrthoChartRowNum", "orthoChartRow", paramSignature);
        }
    }

    public static bool Update(OrthoChartRow orthoChartRow, OrthoChartRow oldOrthoChartRow)
    {
        var command = "";
        if (orthoChartRow.PatNum != oldOrthoChartRow.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(orthoChartRow.PatNum) + "";
        }

        if (orthoChartRow.DateTimeService != oldOrthoChartRow.DateTimeService)
        {
            if (command != "") command += ",";
            command += "DateTimeService = " + SOut.DateTime(orthoChartRow.DateTimeService) + "";
        }

        if (orthoChartRow.UserNum != oldOrthoChartRow.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(orthoChartRow.UserNum) + "";
        }

        if (orthoChartRow.ProvNum != oldOrthoChartRow.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(orthoChartRow.ProvNum) + "";
        }

        if (orthoChartRow.Signature != oldOrthoChartRow.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (command == "") return false;
        if (orthoChartRow.Signature == null) orthoChartRow.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(orthoChartRow.Signature));
        command = "UPDATE orthochartrow SET " + command
                                              + " WHERE OrthoChartRowNum = " + SOut.Long(orthoChartRow.OrthoChartRowNum);
        Db.NonQ(command, paramSignature);
        return true;
    }

    public static void Delete(long orthoChartRowNum)
    {
        var command = "DELETE FROM orthochartrow "
                      + "WHERE OrthoChartRowNum = " + SOut.Long(orthoChartRowNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoChartRowNums)
    {
        if (listOrthoChartRowNums == null || listOrthoChartRowNums.Count == 0) return;
        var command = "DELETE FROM orthochartrow "
                      + "WHERE OrthoChartRowNum IN(" + string.Join(",", listOrthoChartRowNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}