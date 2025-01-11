using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CommOptOutCrud
{
    public static CommOptOut SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<CommOptOut> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CommOptOut> TableToList(DataTable table)
    {
        var retVal = new List<CommOptOut>();
        CommOptOut commOptOut;
        foreach (DataRow row in table.Rows)
        {
            commOptOut = new CommOptOut();
            commOptOut.CommOptOutNum = SIn.Long(row["CommOptOutNum"].ToString());
            commOptOut.PatNum = SIn.Long(row["PatNum"].ToString());
            commOptOut.OptOutSms = (CommOptOutType) SIn.Int(row["OptOutSms"].ToString());
            commOptOut.OptOutEmail = (CommOptOutType) SIn.Int(row["OptOutEmail"].ToString());
            retVal.Add(commOptOut);
        }

        return retVal;
    }

    public static void InsertMany(List<CommOptOut> listCommOptOuts)
    {
        InsertMany(listCommOptOuts, false);
    }

    public static void InsertMany(List<CommOptOut> listCommOptOuts, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listCommOptOuts.Count)
        {
            var commOptOut = listCommOptOuts[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO commoptout (");
                if (useExistingPK) sbCommands.Append("CommOptOutNum,");
                sbCommands.Append("PatNum,OptOutSms,OptOutEmail) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(commOptOut.CommOptOutNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(commOptOut.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) commOptOut.OptOutSms));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) commOptOut.OptOutEmail));
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
                if (index == listCommOptOuts.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(CommOptOut commOptOut, CommOptOut oldCommOptOut)
    {
        var command = "";
        if (commOptOut.PatNum != oldCommOptOut.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(commOptOut.PatNum) + "";
        }

        if (commOptOut.OptOutSms != oldCommOptOut.OptOutSms)
        {
            if (command != "") command += ",";
            command += "OptOutSms = " + SOut.Int((int) commOptOut.OptOutSms) + "";
        }

        if (commOptOut.OptOutEmail != oldCommOptOut.OptOutEmail)
        {
            if (command != "") command += ",";
            command += "OptOutEmail = " + SOut.Int((int) commOptOut.OptOutEmail) + "";
        }

        if (command == "") return;
        command = "UPDATE commoptout SET " + command
                                           + " WHERE CommOptOutNum = " + SOut.Long(commOptOut.CommOptOutNum);
        Db.NonQ(command);
    }
}