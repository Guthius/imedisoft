using System.Collections.Generic;
using System.Text;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EtransMessageTexts
{
    
    public static long Insert(EtransMessageText etransMessageText)
    {
        return EtransMessageTextCrud.Insert(etransMessageText);
    }

    ///<summary>If the message text is X12, then it always normalizes it to include carriage returns for better readability.</summary>
    public static string GetMessageText(long etransMessageTextNum)
    {
        if (etransMessageTextNum == 0) return "";
        var command = "SELECT MessageText FROM etransmessagetext WHERE EtransMessageTextNum=" + SOut.Long(etransMessageTextNum);
        var msgText = DataCore.GetScalar(command);
        return TidyMessageTextX12(msgText);
    }

    /// <summary>
    ///     This function is used to enhance readabilty of the X12 message when displayed.
    ///     This function is specifically for X12 messages and not for other formats (ex not for Canadian).
    /// </summary>
    private static string TidyMessageTextX12(string msgText)
    {
        if (!X12object.IsX12(msgText)) return msgText;
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < msgText.Length; i++)
            if (msgText[i] == '~' && i < msgText.Length - 1 && !msgText[i + 1].In('\n', '\r'))
                stringBuilder.Append("~\r\n");
            else
                stringBuilder.Append(msgText[i]);

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     Returns dictionary such that the key is an etransMessageTextNum and the value is the MessageText.
    ///     If the message text is X12, then it always normalizes it to include carriage returns for better readability.
    /// </summary>
    public static Dictionary<long, string> GetMessageTexts(List<long> listEtransMessageTextNums, bool isFormattingNeededX12 = true)
    {
        var retVal = new Dictionary<long, string>();
        if (listEtransMessageTextNums == null || listEtransMessageTextNums.Count == 0) return retVal;
        var command = "SELECT EtransMessageTextNum,MessageText FROM etransmessagetext WHERE EtransMessageTextNum IN(" + string.Join(",", listEtransMessageTextNums) + ")";
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var eTransMessageTextNum = SIn.Long(table.Rows[i]["EtransMessageTextNum"].ToString());
            var msgText = table.Rows[i]["MessageText"].ToString();
            if (isFormattingNeededX12) msgText = TidyMessageTextX12(msgText);
            retVal.Add(eTransMessageTextNum, msgText);
        }

        return retVal;
    }

    /// <summary>
    ///     Returns any EtransMessageText where the MessageText is identical to the given messageText.
    ///     Otherwise if none returns null.
    /// </summary>
    public static EtransMessageText GetMostRecentForType(EtransType etransType)
    {
        var command = "SELECT etransmessagetext.* FROM etransmessagetext "
                      + "INNER JOIN etrans ON etrans.EtransMessageTextNum=etransmessagetext.EtransMessageTextNum "
                      + "WHERE Etype=" + SOut.Int((int) etransType) + " "
                      + "ORDER BY etrans.DateTimeTrans DESC";
        command = DbHelper.LimitOrderBy(command, 1); //Most recent entry if any.
        return EtransMessageTextCrud.SelectOne(command);
    }

    /*
    
    public static void Update(EtransMessageText EtransMessageText) {

        string command= "UPDATE EtransMessageText SET "
            +"ClearingHouseNum = '"   +POut.PInt   (EtransMessageText.ClearingHouseNum)+"', "
            +"Etype= '"               +POut.PInt   ((int)EtransMessageText.Etype)+"', "
            +"Note= '"                +POut.PString(EtransMessageText.Note)+"', "
            +"EtransMessageTextMessageTextNum= '"+POut.PInt   (EtransMessageText.EtransMessageTextMessageTextNum)+"' "
            +"WHERE EtransMessageTextNum = "+POut.PInt(EtransMessageText.EtransMessageTextNum);
        Db.NonQ(command);
    }
*/

    
    public static void Delete(long etransMessageTextNum, long etransNum = 0)
    {
        if (etransMessageTextNum == 0) return;
        string command;
        if (etransNum == 0)
            command = "DELETE FROM etransmessagetext WHERE EtransMessageTextNum=" + SOut.Long(etransMessageTextNum);
        else
            //When a etransNum is specified we cannot delete the EtransMessageText row if it is associated to any other etransNum.
            command = "DELETE etransmessagetext FROM etransmessagetext "
                      + "LEFT JOIN etrans ON etrans.EtransMessageTextNum=etransmessagetext.EtransMessageTextNum AND etrans.EtransNum!=" + SOut.Long(etransNum) + " "
                      + "WHERE etransmessagetext.EtransMessageTextNum=" + SOut.Long(etransMessageTextNum) + " "
                      + "AND etrans.EtransNum IS NULL";
        Db.NonQ(command);
    }
}