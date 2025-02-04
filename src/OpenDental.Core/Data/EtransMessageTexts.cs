using System.Collections.Generic;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EtransMessageTexts
{
    public static void Insert(EtransMessageText etransMessageText)
    {
        EtransMessageTextCrud.Insert(etransMessageText);
    }

    public static string GetMessageText(long etransMessageTextNum)
    {
        if (etransMessageTextNum == 0)
        {
            return "";
        }

        var messageText = DataCore.GetScalar("SELECT MessageText FROM etransmessagetext WHERE EtransMessageTextNum = " + etransMessageTextNum);

        return TidyMessageTextX12(messageText);
    }

    private static string TidyMessageTextX12(string messageText)
    {
        if (!X12object.IsX12(messageText))
        {
            return messageText;
        }

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < messageText.Length; i++)
        {
            if (messageText[i] == '~' && i < messageText.Length - 1 && !messageText[i + 1].In('\n', '\r'))
            {
                stringBuilder.Append("~\r\n");
            }
            else
            {
                stringBuilder.Append(messageText[i]);
            }
        }

        return stringBuilder.ToString();
    }

    public static Dictionary<long, string> GetMessageTexts(List<long> etransMessageTextNums, bool isFormattingNeededX12 = true)
    {
        if (etransMessageTextNums == null || etransMessageTextNums.Count == 0)
        {
            return new Dictionary<long, string>();
        }
        
        var dataTable = DataCore.GetTable(
            "SELECT EtransMessageTextNum, MessageText " +
            "FROM etransmessagetext " +
            "WHERE EtransMessageTextNum IN (" + string.Join(", ", etransMessageTextNums) + ")");
        
        var messageTexts = new Dictionary<long, string>();
        
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var eTransMessageTextNum = SIn.Long(dataTable.Rows[i]["EtransMessageTextNum"].ToString());
            
            var messageText = dataTable.Rows[i]["MessageText"].ToString();
            if (isFormattingNeededX12)
            {
                messageText = TidyMessageTextX12(messageText);
            }
            
            messageTexts.Add(eTransMessageTextNum, messageText);
        }

        return messageTexts;
    }

    public static EtransMessageText GetMostRecentForType(EtransType etransType)
    {
        return EtransMessageTextCrud.SelectOne(
            "SELECT etransmessagetext.* FROM etransmessagetext " +
            "INNER JOIN etrans ON etrans.EtransMessageTextNum = etransmessagetext.EtransMessageTextNum " + 
            "WHERE Etype = " + (int) etransType + " " + 
            "ORDER BY etrans.DateTimeTrans DESC LIMIT 1");
    }

    public static void Delete(long etransMessageTextNum, long etransNum = 0)
    {
        if (etransMessageTextNum == 0)
        {
            return;
        }
        
        string commandText;
        if (etransNum == 0)
        {
            commandText = "DELETE FROM etransmessagetext WHERE EtransMessageTextNum = " + etransMessageTextNum;
        }
        else
        {
            commandText = 
                "DELETE etransmessagetext FROM etransmessagetext " + 
                "LEFT JOIN etrans ON etrans.EtransMessageTextNum = etransmessagetext.EtransMessageTextNum AND etrans.EtransNum != " + etransNum + " " + 
                "WHERE etransmessagetext.EtransMessageTextNum = " + etransMessageTextNum + " " + 
                "AND etrans.EtransNum IS NULL";
        }
        
        Db.NonQ(commandText);
    }
}