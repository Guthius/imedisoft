using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EtransMessageTextCrud
{
    public static EtransMessageText SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EtransMessageText> TableToList(DataTable table)
    {
        var retVal = new List<EtransMessageText>();
        foreach (DataRow row in table.Rows)
        {
            var etransMessageText = new EtransMessageText
            {
                EtransMessageTextNum = SIn.Long(row["EtransMessageTextNum"].ToString()),
                MessageText = SIn.String(row["MessageText"].ToString())
            };
            retVal.Add(etransMessageText);
        }

        return retVal;
    }

    public static void Insert(EtransMessageText etransMessageText)
    {
        var command = "INSERT INTO etransmessagetext (";

        command += "MessageText) VALUES(";

        command +=
            DbHelper.ParamChar + "paramMessageText)";
        if (etransMessageText.MessageText == null) etransMessageText.MessageText = "";
        var paramMessageText = new OdSqlParameter("paramMessageText", SOut.StringParam(etransMessageText.MessageText));
        {
            etransMessageText.EtransMessageTextNum = Db.NonQ(command, true, "EtransMessageTextNum", "etransMessageText", paramMessageText);
        }
    }
}