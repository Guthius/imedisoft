using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SigMessages
{
    public static List<SigMessage> GetSigMessages(List<long> listSigMessageNums)
    {
        if (listSigMessageNums == null || listSigMessageNums.Count < 1) return new List<SigMessage>();
        var command = "SELECT * FROM sigmessage WHERE SigMessageNum IN (" + string.Join(",", listSigMessageNums) + ")";
        return SigMessageCrud.SelectMany(command);
    }

    public static List<SigMessage> RefreshCurrentButState()
    {
        var listSigMessages = new List<SigMessage>();
        var command = @"SELECT * FROM sigmessage "
                      + "WHERE AckDateTime < " + SOut.DateTime(new DateTime(1880, 1, 1)) + " "
                      + "ORDER BY MessageDateTime";
        listSigMessages = SigMessageCrud.SelectMany(command);
        listSigMessages.Sort();
        return listSigMessages;
    }

    public static List<SigMessage> GetSigMessagesSinceDateTime(DateTime dateTimeSince)
    {
        var listSigMessages = new List<SigMessage>();
        var command = "SELECT * FROM sigmessage "
                      + "WHERE (MessageDateTime > " + SOut.DateTime(dateTimeSince) + " "
                      + "OR AckDateTime > " + SOut.DateTime(dateTimeSince) + " "
                      + "OR AckDateTime < " + SOut.Date(new DateTime(1880, 1, 1), true) + ") " //always include all unacked.
                      + "ORDER BY MessageDateTime";
        //note: this might return an occasional row that has both times newer.
        listSigMessages = SigMessageCrud.SelectMany(command);
        listSigMessages.Sort();
        return listSigMessages;
    }

    public static void AckButton(int buttonIndex, DateTime dateTime)
    {
        var listSigMessageNums = new List<long>();
        var command = "SELECT DISTINCT sigmessage.SigMessageNum FROM sigmessage "
                      + "INNER JOIN sigelementdef ON (sigmessage.SigElementDefNumUser=sigelementdef.SigElementDefNum "
                      + "OR sigmessage.SigElementDefNumExtra=sigelementdef.SigElementDefNum "
                      + "OR sigmessage.SigElementDefNumMsg=sigelementdef.SigElementDefNum) "
                      + "WHERE sigmessage.AckDateTime < " + SOut.Date(new DateTime(1880, 1, 1), true) + " "
                      + "AND MessageDateTime <= " + SOut.DateTime(dateTime) + " "
                      + "AND sigelementdef.LightRow=" + SOut.Long(buttonIndex);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return;
        listSigMessageNums = table.Select().Select(x => SIn.Long(x["SigMessageNum"].ToString())).ToList();
        command = "UPDATE sigmessage SET AckDateTime = " + "NOW()" + " "
                  + "WHERE SigMessageNum IN (" + string.Join(",", listSigMessageNums) + ")";
        Db.NonQ(command);
        listSigMessageNums.ForEach(x => Signalods.SetInvalid(InvalidType.SigMessages, KeyType.SigMessage, x));
    }

    public static void AckSigMessage(SigMessage sigMessage)
    {
        //To ack a message, simply update the AckDateTime on the original row.
        sigMessage.AckDateTime = MiscData.GetNowDateTime();
        Update(sigMessage);
    }

    public static void Insert(SigMessage sigMessage)
    {
        SigMessageCrud.Insert(sigMessage);
    }

    public static void Update(SigMessage sigMessage)
    {
        SigMessageCrud.Update(sigMessage);
    }

    public static void ClearOldSigMessages()
    {
        //Get all ack'd messages older than two days.
        var command = "";
        //easier to read than using the DbHelper Functions
        command = "SELECT SigMessageNum FROM sigmessage WHERE AckDateTime > " + SOut.DateTime(new DateTime(1880, 1, 1)) + " "
                  + "AND AckDateTime < DATE_ADD(NOW(),INTERVAL -2 DAY)";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count < 1) return; //Nothing to delete.
        //Delete all of the acks.
        command = "DELETE FROM sigmessage "
                  + "WHERE SigMessageNum IN (" + string.Join(",", table.Select().Select(x => SIn.Long(x["SigMessageNum"].ToString()))) + ")";
        Db.NonQ(command);
    }
}