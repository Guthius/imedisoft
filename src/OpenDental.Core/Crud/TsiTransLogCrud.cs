#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TsiTransLogCrud
{
    public static TsiTransLog SelectOne(long tsiTransLogNum)
    {
        var command = "SELECT * FROM tsitranslog "
                      + "WHERE TsiTransLogNum = " + SOut.Long(tsiTransLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TsiTransLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TsiTransLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TsiTransLog> TableToList(DataTable table)
    {
        var retVal = new List<TsiTransLog>();
        TsiTransLog tsiTransLog;
        foreach (DataRow row in table.Rows)
        {
            tsiTransLog = new TsiTransLog();
            tsiTransLog.TsiTransLogNum = SIn.Long(row["TsiTransLogNum"].ToString());
            tsiTransLog.PatNum = SIn.Long(row["PatNum"].ToString());
            tsiTransLog.UserNum = SIn.Long(row["UserNum"].ToString());
            tsiTransLog.TransType = (TsiTransType) SIn.Int(row["TransType"].ToString());
            tsiTransLog.TransDateTime = SIn.DateTime(row["TransDateTime"].ToString());
            tsiTransLog.ServiceType = (TsiServiceType) SIn.Int(row["ServiceType"].ToString());
            tsiTransLog.ServiceCode = (TsiServiceCode) SIn.Int(row["ServiceCode"].ToString());
            tsiTransLog.ClientId = SIn.String(row["ClientId"].ToString());
            tsiTransLog.TransAmt = SIn.Double(row["TransAmt"].ToString());
            tsiTransLog.AccountBalance = SIn.Double(row["AccountBalance"].ToString());
            tsiTransLog.FKeyType = (TsiFKeyType) SIn.Int(row["FKeyType"].ToString());
            tsiTransLog.FKey = SIn.Long(row["FKey"].ToString());
            tsiTransLog.RawMsgText = SIn.String(row["RawMsgText"].ToString());
            tsiTransLog.TransJson = SIn.String(row["TransJson"].ToString());
            tsiTransLog.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            tsiTransLog.AggTransLogNum = SIn.Long(row["AggTransLogNum"].ToString());
            retVal.Add(tsiTransLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TsiTransLog> listTsiTransLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TsiTransLog";
        var table = new DataTable(tableName);
        table.Columns.Add("TsiTransLogNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("TransType");
        table.Columns.Add("TransDateTime");
        table.Columns.Add("ServiceType");
        table.Columns.Add("ServiceCode");
        table.Columns.Add("ClientId");
        table.Columns.Add("TransAmt");
        table.Columns.Add("AccountBalance");
        table.Columns.Add("FKeyType");
        table.Columns.Add("FKey");
        table.Columns.Add("RawMsgText");
        table.Columns.Add("TransJson");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("AggTransLogNum");
        foreach (var tsiTransLog in listTsiTransLogs)
            table.Rows.Add(SOut.Long(tsiTransLog.TsiTransLogNum), SOut.Long(tsiTransLog.PatNum), SOut.Long(tsiTransLog.UserNum), SOut.Int((int) tsiTransLog.TransType), SOut.DateT(tsiTransLog.TransDateTime, false), SOut.Int((int) tsiTransLog.ServiceType), SOut.Int((int) tsiTransLog.ServiceCode), tsiTransLog.ClientId, SOut.Double(tsiTransLog.TransAmt), SOut.Double(tsiTransLog.AccountBalance), SOut.Int((int) tsiTransLog.FKeyType), SOut.Long(tsiTransLog.FKey), tsiTransLog.RawMsgText, tsiTransLog.TransJson, SOut.Long(tsiTransLog.ClinicNum), SOut.Long(tsiTransLog.AggTransLogNum));
        return table;
    }

    public static long Insert(TsiTransLog tsiTransLog)
    {
        return Insert(tsiTransLog, false);
    }

    public static long Insert(TsiTransLog tsiTransLog, bool useExistingPK)
    {
        var command = "INSERT INTO tsitranslog (";

        command += "PatNum,UserNum,TransType,TransDateTime,ServiceType,ServiceCode,ClientId,TransAmt,AccountBalance,FKeyType,FKey,RawMsgText,TransJson,ClinicNum,AggTransLogNum) VALUES(";

        command +=
            SOut.Long(tsiTransLog.PatNum) + ","
                                          + SOut.Long(tsiTransLog.UserNum) + ","
                                          + SOut.Int((int) tsiTransLog.TransType) + ","
                                          + DbHelper.Now() + ","
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
        var paramTransJson = new OdSqlParameter("paramTransJson", OdDbType.Text, SOut.StringParam(tsiTransLog.TransJson));
        {
            tsiTransLog.TsiTransLogNum = Db.NonQ(command, true, "TsiTransLogNum", "tsiTransLog", paramTransJson);
        }
        return tsiTransLog.TsiTransLogNum;
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
            sbRow.Append(DbHelper.Now());
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

    public static long InsertNoCache(TsiTransLog tsiTransLog)
    {
        return InsertNoCache(tsiTransLog, false);
    }

    public static long InsertNoCache(TsiTransLog tsiTransLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO tsitranslog (";
        if (isRandomKeys || useExistingPK) command += "TsiTransLogNum,";
        command += "PatNum,UserNum,TransType,TransDateTime,ServiceType,ServiceCode,ClientId,TransAmt,AccountBalance,FKeyType,FKey,RawMsgText,TransJson,ClinicNum,AggTransLogNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(tsiTransLog.TsiTransLogNum) + ",";
        command +=
            SOut.Long(tsiTransLog.PatNum) + ","
                                          + SOut.Long(tsiTransLog.UserNum) + ","
                                          + SOut.Int((int) tsiTransLog.TransType) + ","
                                          + DbHelper.Now() + ","
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
        var paramTransJson = new OdSqlParameter("paramTransJson", OdDbType.Text, SOut.StringParam(tsiTransLog.TransJson));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramTransJson);
        else
            tsiTransLog.TsiTransLogNum = Db.NonQ(command, true, "TsiTransLogNum", "tsiTransLog", paramTransJson);
        return tsiTransLog.TsiTransLogNum;
    }

    public static void Update(TsiTransLog tsiTransLog)
    {
        var command = "UPDATE tsitranslog SET "
                      + "PatNum        =  " + SOut.Long(tsiTransLog.PatNum) + ", "
                      + "UserNum       =  " + SOut.Long(tsiTransLog.UserNum) + ", "
                      + "TransType     =  " + SOut.Int((int) tsiTransLog.TransType) + ", "
                      //TransDateTime not allowed to change
                      + "ServiceType   =  " + SOut.Int((int) tsiTransLog.ServiceType) + ", "
                      + "ServiceCode   =  " + SOut.Int((int) tsiTransLog.ServiceCode) + ", "
                      + "ClientId      = '" + SOut.String(tsiTransLog.ClientId) + "', "
                      + "TransAmt      =  " + SOut.Double(tsiTransLog.TransAmt) + ", "
                      + "AccountBalance=  " + SOut.Double(tsiTransLog.AccountBalance) + ", "
                      + "FKeyType      =  " + SOut.Int((int) tsiTransLog.FKeyType) + ", "
                      + "FKey          =  " + SOut.Long(tsiTransLog.FKey) + ", "
                      + "RawMsgText    = '" + SOut.String(tsiTransLog.RawMsgText) + "', "
                      + "TransJson     =  " + DbHelper.ParamChar + "paramTransJson, "
                      + "ClinicNum     =  " + SOut.Long(tsiTransLog.ClinicNum) + ", "
                      + "AggTransLogNum=  " + SOut.Long(tsiTransLog.AggTransLogNum) + " "
                      + "WHERE TsiTransLogNum = " + SOut.Long(tsiTransLog.TsiTransLogNum);
        if (tsiTransLog.TransJson == null) tsiTransLog.TransJson = "";
        var paramTransJson = new OdSqlParameter("paramTransJson", OdDbType.Text, SOut.StringParam(tsiTransLog.TransJson));
        Db.NonQ(command, paramTransJson);
    }

    public static bool Update(TsiTransLog tsiTransLog, TsiTransLog oldTsiTransLog)
    {
        var command = "";
        if (tsiTransLog.PatNum != oldTsiTransLog.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(tsiTransLog.PatNum) + "";
        }

        if (tsiTransLog.UserNum != oldTsiTransLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(tsiTransLog.UserNum) + "";
        }

        if (tsiTransLog.TransType != oldTsiTransLog.TransType)
        {
            if (command != "") command += ",";
            command += "TransType = " + SOut.Int((int) tsiTransLog.TransType) + "";
        }

        //TransDateTime not allowed to change
        if (tsiTransLog.ServiceType != oldTsiTransLog.ServiceType)
        {
            if (command != "") command += ",";
            command += "ServiceType = " + SOut.Int((int) tsiTransLog.ServiceType) + "";
        }

        if (tsiTransLog.ServiceCode != oldTsiTransLog.ServiceCode)
        {
            if (command != "") command += ",";
            command += "ServiceCode = " + SOut.Int((int) tsiTransLog.ServiceCode) + "";
        }

        if (tsiTransLog.ClientId != oldTsiTransLog.ClientId)
        {
            if (command != "") command += ",";
            command += "ClientId = '" + SOut.String(tsiTransLog.ClientId) + "'";
        }

        if (tsiTransLog.TransAmt != oldTsiTransLog.TransAmt)
        {
            if (command != "") command += ",";
            command += "TransAmt = " + SOut.Double(tsiTransLog.TransAmt) + "";
        }

        if (tsiTransLog.AccountBalance != oldTsiTransLog.AccountBalance)
        {
            if (command != "") command += ",";
            command += "AccountBalance = " + SOut.Double(tsiTransLog.AccountBalance) + "";
        }

        if (tsiTransLog.FKeyType != oldTsiTransLog.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = " + SOut.Int((int) tsiTransLog.FKeyType) + "";
        }

        if (tsiTransLog.FKey != oldTsiTransLog.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(tsiTransLog.FKey) + "";
        }

        if (tsiTransLog.RawMsgText != oldTsiTransLog.RawMsgText)
        {
            if (command != "") command += ",";
            command += "RawMsgText = '" + SOut.String(tsiTransLog.RawMsgText) + "'";
        }

        if (tsiTransLog.TransJson != oldTsiTransLog.TransJson)
        {
            if (command != "") command += ",";
            command += "TransJson = " + DbHelper.ParamChar + "paramTransJson";
        }

        if (tsiTransLog.ClinicNum != oldTsiTransLog.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(tsiTransLog.ClinicNum) + "";
        }

        if (tsiTransLog.AggTransLogNum != oldTsiTransLog.AggTransLogNum)
        {
            if (command != "") command += ",";
            command += "AggTransLogNum = " + SOut.Long(tsiTransLog.AggTransLogNum) + "";
        }

        if (command == "") return false;
        if (tsiTransLog.TransJson == null) tsiTransLog.TransJson = "";
        var paramTransJson = new OdSqlParameter("paramTransJson", OdDbType.Text, SOut.StringParam(tsiTransLog.TransJson));
        command = "UPDATE tsitranslog SET " + command
                                            + " WHERE TsiTransLogNum = " + SOut.Long(tsiTransLog.TsiTransLogNum);
        Db.NonQ(command, paramTransJson);
        return true;
    }

    public static bool UpdateComparison(TsiTransLog tsiTransLog, TsiTransLog oldTsiTransLog)
    {
        if (tsiTransLog.PatNum != oldTsiTransLog.PatNum) return true;
        if (tsiTransLog.UserNum != oldTsiTransLog.UserNum) return true;
        if (tsiTransLog.TransType != oldTsiTransLog.TransType) return true;
        //TransDateTime not allowed to change
        if (tsiTransLog.ServiceType != oldTsiTransLog.ServiceType) return true;
        if (tsiTransLog.ServiceCode != oldTsiTransLog.ServiceCode) return true;
        if (tsiTransLog.ClientId != oldTsiTransLog.ClientId) return true;
        if (tsiTransLog.TransAmt != oldTsiTransLog.TransAmt) return true;
        if (tsiTransLog.AccountBalance != oldTsiTransLog.AccountBalance) return true;
        if (tsiTransLog.FKeyType != oldTsiTransLog.FKeyType) return true;
        if (tsiTransLog.FKey != oldTsiTransLog.FKey) return true;
        if (tsiTransLog.RawMsgText != oldTsiTransLog.RawMsgText) return true;
        if (tsiTransLog.TransJson != oldTsiTransLog.TransJson) return true;
        if (tsiTransLog.ClinicNum != oldTsiTransLog.ClinicNum) return true;
        if (tsiTransLog.AggTransLogNum != oldTsiTransLog.AggTransLogNum) return true;
        return false;
    }

    public static void Delete(long tsiTransLogNum)
    {
        var command = "DELETE FROM tsitranslog "
                      + "WHERE TsiTransLogNum = " + SOut.Long(tsiTransLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTsiTransLogNums)
    {
        if (listTsiTransLogNums == null || listTsiTransLogNums.Count == 0) return;
        var command = "DELETE FROM tsitranslog "
                      + "WHERE TsiTransLogNum IN(" + string.Join(",", listTsiTransLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}