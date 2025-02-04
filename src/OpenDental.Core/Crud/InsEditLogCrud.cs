using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsEditLogCrud
{
    public static InsEditLog SelectOne(long insEditLogNum)
    {
        var command = "SELECT * FROM inseditlog "
                      + "WHERE InsEditLogNum = " + SOut.Long(insEditLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsEditLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsEditLog> TableToList(DataTable table)
    {
        var retVal = new List<InsEditLog>();
        foreach (DataRow row in table.Rows)
        {
            var insEditLog = new InsEditLog
            {
                InsEditLogNum = SIn.Long(row["InsEditLogNum"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                LogType = (InsEditLogType) SIn.Int(row["LogType"].ToString()),
                FieldName = SIn.String(row["FieldName"].ToString()),
                OldValue = SIn.String(row["OldValue"].ToString()),
                NewValue = SIn.String(row["NewValue"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                ParentKey = SIn.Long(row["ParentKey"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(insEditLog);
        }

        return retVal;
    }

    public static void Insert(InsEditLog insEditLog)
    {
        var command = "INSERT INTO inseditlog (";

        command += "FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES(";

        command +=
            SOut.Long(insEditLog.FKey) + ","
                                       + SOut.Int((int) insEditLog.LogType) + ","
                                       + "'" + SOut.String(insEditLog.FieldName) + "',"
                                       + "'" + SOut.String(insEditLog.OldValue) + "',"
                                       + "'" + SOut.String(insEditLog.NewValue) + "',"
                                       + SOut.Long(insEditLog.UserNum) + ","
                                       //DateTStamp can only be set by MySQL
                                       + SOut.Long(insEditLog.ParentKey) + ","
                                       + "'" + SOut.String(insEditLog.Description) + "')";
        {
            insEditLog.InsEditLogNum = Db.NonQ(command, true, "InsEditLogNum", "insEditLog");
        }
    }

    public static void InsertMany(List<InsEditLog> listInsEditLogs, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listInsEditLogs.Count)
        {
            var insEditLog = listInsEditLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO inseditlog (");
                if (useExistingPK) sbCommands.Append("InsEditLogNum,");
                sbCommands.Append("FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(insEditLog.InsEditLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(insEditLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) insEditLog.LogType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.FieldName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.OldValue) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.NewValue) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(insEditLog.UserNum));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(insEditLog.ParentKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.Description) + "'");
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
                if (index == listInsEditLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}