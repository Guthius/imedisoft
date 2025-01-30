using System.Collections.Generic;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DbmLogCrud
{
    public static void InsertMany(List<DbmLog> listDbmLogs)
    {
        InsertMany(listDbmLogs, false);
    }

    public static void InsertMany(List<DbmLog> listDbmLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listDbmLogs.Count)
        {
            var dbmLog = listDbmLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO dbmlog (");
                if (useExistingPK) sbCommands.Append("DbmLogNum,");
                sbCommands.Append("UserNum,FKey,FKeyType,ActionType,DateTimeEntry,MethodName,LogText) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(dbmLog.DbmLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(dbmLog.UserNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(dbmLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) dbmLog.FKeyType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) dbmLog.ActionType));
            sbRow.Append(",");
            sbRow.Append("NOW()");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(dbmLog.MethodName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(dbmLog.LogText) + "'");
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
                if (index == listDbmLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}