using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EmailMessageUids
{
	/// <summary>
	///     Gets all unique email ids for the given recipient email address.  The result is used to determine which emails
	///     to download for a particular inbox address.
	/// </summary>
	public static List<string> GetMsgIdsRecipientAddress(string strRecipientAddress)
    {
        var command = "SELECT MsgId FROM emailmessageuid WHERE RecipientAddress='" + SOut.String(strRecipientAddress) + "' GROUP BY BINARY MsgId";
        return Db.GetListString(command);
    }

    
    public static long Insert(EmailMessageUid emailMessageUid)
    {
        return EmailMessageUidCrud.Insert(emailMessageUid);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    ///<summary>Gets one EmailMessageUid from the db.</summary>
    public static EmailMessageUid GetOne(long emailMessageUidNum){

        return Crud.EmailMessageUidCrud.SelectOne(emailMessageUidNum);
    }

    
    public static void Update(EmailMessageUid emailMessageUid){

        Crud.EmailMessageUidCrud.Update(emailMessageUid);
    }

    
    public static void Delete(long emailMessageUidNum) {

        string command= "DELETE FROM emailmessageuid WHERE EmailMessageUidNum = "+POut.Long(emailMessageUidNum);
        Db.NonQ(command);
    }
    */
}