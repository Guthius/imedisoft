using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoChartLogCrud
{
    public static void Insert(OrthoChartLog orthoChartLog)
    {
        var command = "INSERT INTO orthochartlog (";

        command += "PatNum,ComputerName,DateTimeLog,DateTimeService,UserNum,ProvNum,OrthoChartRowNum,LogData) VALUES(";

        command +=
            SOut.Long(orthoChartLog.PatNum) + ","
                                            + "'" + SOut.String(orthoChartLog.ComputerName) + "',"
                                            + SOut.DateTime(orthoChartLog.DateTimeLog) + ","
                                            + SOut.DateTime(orthoChartLog.DateTimeService) + ","
                                            + SOut.Long(orthoChartLog.UserNum) + ","
                                            + SOut.Long(orthoChartLog.ProvNum) + ","
                                            + SOut.Long(orthoChartLog.OrthoChartRowNum) + ","
                                            + DbHelper.ParamChar + "paramLogData)";
        if (orthoChartLog.LogData == null) orthoChartLog.LogData = "";
        var paramLogData = new OdSqlParameter("paramLogData", SOut.StringParam(orthoChartLog.LogData));
        {
            orthoChartLog.OrthoChartLogNum = Db.NonQ(command, true, "OrthoChartLogNum", "orthoChartLog", paramLogData);
        }
    }
}