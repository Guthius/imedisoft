using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EmailMessageUidCrud
{
    public static void Insert(EmailMessageUid emailMessageUid)
    {
        var command = "INSERT INTO emailmessageuid (";

        command += "MsgId,RecipientAddress) VALUES(";

        command +=
            DbHelper.ParamChar + "paramMsgId,"
                               + "'" + SOut.String(emailMessageUid.RecipientAddress) + "')";
        if (emailMessageUid.MsgId == null) emailMessageUid.MsgId = "";
        var paramMsgId = new OdSqlParameter("paramMsgId", SOut.StringParam(emailMessageUid.MsgId));
        {
            emailMessageUid.EmailMessageUidNum = Db.NonQ(command, true, "EmailMessageUidNum", "emailMessageUid", paramMsgId);
        }
    }
}