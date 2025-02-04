using System.Collections.Generic;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EntryLogCrud
{
    public static void Insert(EntryLog entryLog)
    {
        var command = "INSERT INTO entrylog (";

        command += "UserNum,FKeyType,FKey,LogSource,EntryDateTime) VALUES(";

        command +=
            SOut.Long(entryLog.UserNum) + ","
                                        + SOut.Int((int) entryLog.FKeyType) + ","
                                        + SOut.Long(entryLog.FKey) + ","
                                        + SOut.Int((int) entryLog.LogSource) + ","
                                        + "NOW()" + ")";
        {
            entryLog.EntryLogNum = Db.NonQ(command, true, "EntryLogNum", "entryLog");
        }
    }

    public static void InsertMany(List<EntryLog> listEntryLogs)
    {
        InsertMany(listEntryLogs, false);
    }

    public static void InsertMany(List<EntryLog> listEntryLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listEntryLogs.Count)
        {
            var entryLog = listEntryLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO entrylog (");
                if (useExistingPK) sbCommands.Append("EntryLogNum,");
                sbCommands.Append("UserNum,FKeyType,FKey,LogSource,EntryDateTime) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(entryLog.EntryLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(entryLog.UserNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) entryLog.FKeyType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(entryLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) entryLog.LogSource));
            sbRow.Append(",");
            sbRow.Append("NOW()");
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
                if (index == listEntryLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}