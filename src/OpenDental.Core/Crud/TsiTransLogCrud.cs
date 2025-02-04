using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TsiTransLogCrud
{
    public static List<TsiTransLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TsiTransLog> TableToList(DataTable table)
    {
        var retVal = new List<TsiTransLog>();
        foreach (DataRow row in table.Rows)
        {
            var tsiTransLog = new TsiTransLog
            {
                TsiTransLogNum = SIn.Long(row["TsiTransLogNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                TransType = (TsiTransType) SIn.Int(row["TransType"].ToString()),
                TransDateTime = SIn.DateTime(row["TransDateTime"].ToString()),
                ServiceType = (TsiServiceType) SIn.Int(row["ServiceType"].ToString()),
                ServiceCode = (TsiServiceCode) SIn.Int(row["ServiceCode"].ToString()),
                ClientId = SIn.String(row["ClientId"].ToString()),
                TransAmt = SIn.Double(row["TransAmt"].ToString()),
                AccountBalance = SIn.Double(row["AccountBalance"].ToString()),
                FKeyType = (TsiFKeyType) SIn.Int(row["FKeyType"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                RawMsgText = SIn.String(row["RawMsgText"].ToString()),
                TransJson = SIn.String(row["TransJson"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                AggTransLogNum = SIn.Long(row["AggTransLogNum"].ToString())
            };
            retVal.Add(tsiTransLog);
        }

        return retVal;
    }

    public static void Insert(TsiTransLog tsiTransLog)
    {
        var command = "INSERT INTO tsitranslog (";

        command += "PatNum,UserNum,TransType,TransDateTime,ServiceType,ServiceCode,ClientId,TransAmt,AccountBalance,FKeyType,FKey,RawMsgText,TransJson,ClinicNum,AggTransLogNum) VALUES(";

        command +=
            SOut.Long(tsiTransLog.PatNum) + ","
                                          + SOut.Long(tsiTransLog.UserNum) + ","
                                          + SOut.Int((int) tsiTransLog.TransType) + ","
                                          + "NOW()" + ","
                                          + SOut.Int((int) tsiTransLog.ServiceType) + ","
                                          + SOut.Int((int) tsiTransLog.ServiceCode) + ","
                                          + "'" + SOut.String(tsiTransLog.ClientId) + "',"
                                          + SOut.Double(tsiTransLog.TransAmt) + ","
                                          + SOut.Double(tsiTransLog.AccountBalance) + ","
                                          + SOut.Int((int) tsiTransLog.FKeyType) + ","
                                          + SOut.Long(tsiTransLog.FKey) + ","
                                          + "'" + SOut.String(tsiTransLog.RawMsgText) + "',"
                                          + DbHelper.ParamChar + "paramTransJson,"
                                          + SOut.Long(tsiTransLog.ClinicNum) + ","
                                          + SOut.Long(tsiTransLog.AggTransLogNum) + ")";
        if (tsiTransLog.TransJson == null) tsiTransLog.TransJson = "";
        var paramTransJson = new OdSqlParameter("paramTransJson", SOut.StringParam(tsiTransLog.TransJson));
        {
            tsiTransLog.TsiTransLogNum = Db.NonQ(command, true, "TsiTransLogNum", "tsiTransLog", paramTransJson);
        }
    }

    public static void InsertMany(List<TsiTransLog> listTsiTransLogs)
    {
        InsertMany(listTsiTransLogs, false);
    }

    public static void InsertMany(List<TsiTransLog> listTsiTransLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listTsiTransLogs.Count)
        {
            var tsiTransLog = listTsiTransLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO tsitranslog (");
                if (useExistingPK) sbCommands.Append("TsiTransLogNum,");
                sbCommands.Append("PatNum,UserNum,TransType,TransDateTime,ServiceType,ServiceCode,ClientId,TransAmt,AccountBalance,FKeyType,FKey,RawMsgText,TransJson,ClinicNum,AggTransLogNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(tsiTransLog.TsiTransLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(tsiTransLog.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(tsiTransLog.UserNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) tsiTransLog.TransType));
            sbRow.Append(",");
            sbRow.Append("NOW()");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) tsiTransLog.ServiceType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) tsiTransLog.ServiceCode));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(tsiTransLog.ClientId) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Double(tsiTransLog.TransAmt));
            sbRow.Append(",");
            sbRow.Append(SOut.Double(tsiTransLog.AccountBalance));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) tsiTransLog.FKeyType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(tsiTransLog.FKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(tsiTransLog.RawMsgText) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(tsiTransLog.TransJson) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(tsiTransLog.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(tsiTransLog.AggTransLogNum));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listTsiTransLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}