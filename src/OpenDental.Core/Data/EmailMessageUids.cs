using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EmailMessageUids
{
    public static List<string> GetMsgIdsRecipientAddress(string strRecipientAddress)
    {
        return Db.GetListString("SELECT MsgId FROM emailmessageuid WHERE RecipientAddress='" + SOut.String(strRecipientAddress) + "' GROUP BY BINARY MsgId");
    }

    public static void Insert(EmailMessageUid emailMessageUid)
    {
        EmailMessageUidCrud.Insert(emailMessageUid);
    }
}