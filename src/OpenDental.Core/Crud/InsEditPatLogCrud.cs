using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsEditPatLogCrud
{
    public static List<InsEditPatLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsEditPatLog> TableToList(DataTable table)
    {
        var retVal = new List<InsEditPatLog>();
        foreach (DataRow row in table.Rows)
        {
            var insEditPatLog = new InsEditPatLog
            {
                InsEditPatLogNum = SIn.Long(row["InsEditPatLogNum"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                LogType = (InsEditPatLogType) SIn.Int(row["LogType"].ToString()),
                FieldName = SIn.String(row["FieldName"].ToString()),
                OldValue = SIn.String(row["OldValue"].ToString()),
                NewValue = SIn.String(row["NewValue"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                ParentKey = SIn.Long(row["ParentKey"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(insEditPatLog);
        }

        return retVal;
    }

    public static void InsertMany(List<InsEditPatLog> listInsEditPatLogs, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listInsEditPatLogs.Count)
        {
            var insEditPatLog = listInsEditPatLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO inseditpatlog (");
                if (useExistingPK) sbCommands.Append("InsEditPatLogNum,");
                sbCommands.Append("FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(insEditPatLog.InsEditPatLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(insEditPatLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) insEditPatLog.LogType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.FieldName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.OldValue) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.NewValue) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(insEditPatLog.UserNum));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(insEditPatLog.ParentKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.Description) + "'");
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
                if (index == listInsEditPatLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}