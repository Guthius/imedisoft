using System.Collections.Generic;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class EtransMessageTexts
{
    public static void Insert(EtransMessageText etransMessageText)
    {
        EtransMessageTextCrud.Insert(etransMessageText);
    }

    public static string GetMessageText(long etransMessageTextNum)
    {
        if (etransMessageTextNum == 0) return "";
        var command = "SELECT MessageText FROM etransmessagetext WHERE EtransMessageTextNum=" + SOut.Long(etransMessageTextNum);
        var msgText = DataCore.GetScalar(command);
        return TidyMessageTextX12(msgText);
    }

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

    public static EtransMessageText GetMostRecentForType(EtransType etransType)
    {
        var command = "SELECT etransmessagetext.* FROM etransmessagetext "
                      + "INNER JOIN etrans ON etrans.EtransMessageTextNum=etransmessagetext.EtransMessageTextNum "
                      + "WHERE Etype=" + SOut.Int((int) etransType) + " "
                      + "ORDER BY etrans.DateTimeTrans DESC";
        command = DbHelper.LimitOrderBy(command, 1); //Most recent entry if any.
        return EtransMessageTextCrud.SelectOne(command);
    }

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