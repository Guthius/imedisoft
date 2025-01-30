using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ErxLogCrud
{
    public static List<ErxLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ErxLog> TableToList(DataTable table)
    {
        var retVal = new List<ErxLog>();
        ErxLog erxLog;
        foreach (DataRow row in table.Rows)
        {
            erxLog = new ErxLog();
            erxLog.ErxLogNum = SIn.Long(row["ErxLogNum"].ToString());
            erxLog.PatNum = SIn.Long(row["PatNum"].ToString());
            erxLog.MsgText = SIn.String(row["MsgText"].ToString());
            erxLog.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            erxLog.ProvNum = SIn.Long(row["ProvNum"].ToString());
            erxLog.UserNum = SIn.Long(row["UserNum"].ToString());
            retVal.Add(erxLog);
        }

        return retVal;
    }

    public static void Insert(ErxLog erxLog)
    {
        var command = "INSERT INTO erxlog (";

        command += "PatNum,MsgText,ProvNum,UserNum) VALUES(";

        command +=
            SOut.Long(erxLog.PatNum) + ","
                                     + DbHelper.ParamChar + "paramMsgText,"
                                     //DateTStamp can only be set by MySQL
                                     + SOut.Long(erxLog.ProvNum) + ","
                                     + SOut.Long(erxLog.UserNum) + ")";
        if (erxLog.MsgText == null) erxLog.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", SOut.StringParam(erxLog.MsgText));
        {
            erxLog.ErxLogNum = Db.NonQ(command, true, "ErxLogNum", "erxLog", paramMsgText);
        }
    }
}