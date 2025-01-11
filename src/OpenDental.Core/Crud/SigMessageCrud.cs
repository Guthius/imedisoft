#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SigMessageCrud
{
    public static SigMessage SelectOne(long sigMessageNum)
    {
        var command = "SELECT * FROM sigmessage "
                      + "WHERE SigMessageNum = " + SOut.Long(sigMessageNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SigMessage SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SigMessage> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SigMessage> TableToList(DataTable table)
    {
        var retVal = new List<SigMessage>();
        SigMessage sigMessage;
        foreach (DataRow row in table.Rows)
        {
            sigMessage = new SigMessage();
            sigMessage.SigMessageNum = SIn.Long(row["SigMessageNum"].ToString());
            sigMessage.ButtonText = SIn.String(row["ButtonText"].ToString());
            sigMessage.ButtonIndex = SIn.Int(row["ButtonIndex"].ToString());
            sigMessage.SynchIcon = SIn.Byte(row["SynchIcon"].ToString());
            sigMessage.FromUser = SIn.String(row["FromUser"].ToString());
            sigMessage.ToUser = SIn.String(row["ToUser"].ToString());
            sigMessage.MessageDateTime = SIn.DateTime(row["MessageDateTime"].ToString());
            sigMessage.AckDateTime = SIn.DateTime(row["AckDateTime"].ToString());
            sigMessage.SigText = SIn.String(row["SigText"].ToString());
            sigMessage.SigElementDefNumUser = SIn.Long(row["SigElementDefNumUser"].ToString());
            sigMessage.SigElementDefNumExtra = SIn.Long(row["SigElementDefNumExtra"].ToString());
            sigMessage.SigElementDefNumMsg = SIn.Long(row["SigElementDefNumMsg"].ToString());
            retVal.Add(sigMessage);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SigMessage> listSigMessages, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SigMessage";
        var table = new DataTable(tableName);
        table.Columns.Add("SigMessageNum");
        table.Columns.Add("ButtonText");
        table.Columns.Add("ButtonIndex");
        table.Columns.Add("SynchIcon");
        table.Columns.Add("FromUser");
        table.Columns.Add("ToUser");
        table.Columns.Add("MessageDateTime");
        table.Columns.Add("AckDateTime");
        table.Columns.Add("SigText");
        table.Columns.Add("SigElementDefNumUser");
        table.Columns.Add("SigElementDefNumExtra");
        table.Columns.Add("SigElementDefNumMsg");
        foreach (var sigMessage in listSigMessages)
            table.Rows.Add(SOut.Long(sigMessage.SigMessageNum), sigMessage.ButtonText, SOut.Int(sigMessage.ButtonIndex), SOut.Byte(sigMessage.SynchIcon), sigMessage.FromUser, sigMessage.ToUser, SOut.DateT(sigMessage.MessageDateTime, false), SOut.DateT(sigMessage.AckDateTime, false), sigMessage.SigText, SOut.Long(sigMessage.SigElementDefNumUser), SOut.Long(sigMessage.SigElementDefNumExtra), SOut.Long(sigMessage.SigElementDefNumMsg));
        return table;
    }

    public static long Insert(SigMessage sigMessage)
    {
        return Insert(sigMessage, false);
    }

    public static long Insert(SigMessage sigMessage, bool useExistingPK)
    {
        var command = "INSERT INTO sigmessage (";

        command += "ButtonText,ButtonIndex,SynchIcon,FromUser,ToUser,MessageDateTime,AckDateTime,SigText,SigElementDefNumUser,SigElementDefNumExtra,SigElementDefNumMsg) VALUES(";

        command +=
            "'" + SOut.String(sigMessage.ButtonText) + "',"
            + SOut.Int(sigMessage.ButtonIndex) + ","
            + SOut.Byte(sigMessage.SynchIcon) + ","
            + "'" + SOut.String(sigMessage.FromUser) + "',"
            + "'" + SOut.String(sigMessage.ToUser) + "',"
            + DbHelper.Now() + ","
            + SOut.DateT(sigMessage.AckDateTime) + ","
            + "'" + SOut.String(sigMessage.SigText) + "',"
            + SOut.Long(sigMessage.SigElementDefNumUser) + ","
            + SOut.Long(sigMessage.SigElementDefNumExtra) + ","
            + SOut.Long(sigMessage.SigElementDefNumMsg) + ")";
        {
            sigMessage.SigMessageNum = Db.NonQ(command, true, "SigMessageNum", "sigMessage");
        }
        return sigMessage.SigMessageNum;
    }

    public static long InsertNoCache(SigMessage sigMessage)
    {
        return InsertNoCache(sigMessage, false);
    }

    public static long InsertNoCache(SigMessage sigMessage, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sigmessage (";
        if (isRandomKeys || useExistingPK) command += "SigMessageNum,";
        command += "ButtonText,ButtonIndex,SynchIcon,FromUser,ToUser,MessageDateTime,AckDateTime,SigText,SigElementDefNumUser,SigElementDefNumExtra,SigElementDefNumMsg) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sigMessage.SigMessageNum) + ",";
        command +=
            "'" + SOut.String(sigMessage.ButtonText) + "',"
            + SOut.Int(sigMessage.ButtonIndex) + ","
            + SOut.Byte(sigMessage.SynchIcon) + ","
            + "'" + SOut.String(sigMessage.FromUser) + "',"
            + "'" + SOut.String(sigMessage.ToUser) + "',"
            + DbHelper.Now() + ","
            + SOut.DateT(sigMessage.AckDateTime) + ","
            + "'" + SOut.String(sigMessage.SigText) + "',"
            + SOut.Long(sigMessage.SigElementDefNumUser) + ","
            + SOut.Long(sigMessage.SigElementDefNumExtra) + ","
            + SOut.Long(sigMessage.SigElementDefNumMsg) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            sigMessage.SigMessageNum = Db.NonQ(command, true, "SigMessageNum", "sigMessage");
        return sigMessage.SigMessageNum;
    }

    public static void Update(SigMessage sigMessage)
    {
        var command = "UPDATE sigmessage SET "
                      + "ButtonText           = '" + SOut.String(sigMessage.ButtonText) + "', "
                      + "ButtonIndex          =  " + SOut.Int(sigMessage.ButtonIndex) + ", "
                      + "SynchIcon            =  " + SOut.Byte(sigMessage.SynchIcon) + ", "
                      + "FromUser             = '" + SOut.String(sigMessage.FromUser) + "', "
                      + "ToUser               = '" + SOut.String(sigMessage.ToUser) + "', "
                      //MessageDateTime not allowed to change
                      + "AckDateTime          =  " + SOut.DateT(sigMessage.AckDateTime) + ", "
                      + "SigText              = '" + SOut.String(sigMessage.SigText) + "', "
                      + "SigElementDefNumUser =  " + SOut.Long(sigMessage.SigElementDefNumUser) + ", "
                      + "SigElementDefNumExtra=  " + SOut.Long(sigMessage.SigElementDefNumExtra) + ", "
                      + "SigElementDefNumMsg  =  " + SOut.Long(sigMessage.SigElementDefNumMsg) + " "
                      + "WHERE SigMessageNum = " + SOut.Long(sigMessage.SigMessageNum);
        Db.NonQ(command);
    }

    public static bool Update(SigMessage sigMessage, SigMessage oldSigMessage)
    {
        var command = "";
        if (sigMessage.ButtonText != oldSigMessage.ButtonText)
        {
            if (command != "") command += ",";
            command += "ButtonText = '" + SOut.String(sigMessage.ButtonText) + "'";
        }

        if (sigMessage.ButtonIndex != oldSigMessage.ButtonIndex)
        {
            if (command != "") command += ",";
            command += "ButtonIndex = " + SOut.Int(sigMessage.ButtonIndex) + "";
        }

        if (sigMessage.SynchIcon != oldSigMessage.SynchIcon)
        {
            if (command != "") command += ",";
            command += "SynchIcon = " + SOut.Byte(sigMessage.SynchIcon) + "";
        }

        if (sigMessage.FromUser != oldSigMessage.FromUser)
        {
            if (command != "") command += ",";
            command += "FromUser = '" + SOut.String(sigMessage.FromUser) + "'";
        }

        if (sigMessage.ToUser != oldSigMessage.ToUser)
        {
            if (command != "") command += ",";
            command += "ToUser = '" + SOut.String(sigMessage.ToUser) + "'";
        }

        //MessageDateTime not allowed to change
        if (sigMessage.AckDateTime != oldSigMessage.AckDateTime)
        {
            if (command != "") command += ",";
            command += "AckDateTime = " + SOut.DateT(sigMessage.AckDateTime) + "";
        }

        if (sigMessage.SigText != oldSigMessage.SigText)
        {
            if (command != "") command += ",";
            command += "SigText = '" + SOut.String(sigMessage.SigText) + "'";
        }

        if (sigMessage.SigElementDefNumUser != oldSigMessage.SigElementDefNumUser)
        {
            if (command != "") command += ",";
            command += "SigElementDefNumUser = " + SOut.Long(sigMessage.SigElementDefNumUser) + "";
        }

        if (sigMessage.SigElementDefNumExtra != oldSigMessage.SigElementDefNumExtra)
        {
            if (command != "") command += ",";
            command += "SigElementDefNumExtra = " + SOut.Long(sigMessage.SigElementDefNumExtra) + "";
        }

        if (sigMessage.SigElementDefNumMsg != oldSigMessage.SigElementDefNumMsg)
        {
            if (command != "") command += ",";
            command += "SigElementDefNumMsg = " + SOut.Long(sigMessage.SigElementDefNumMsg) + "";
        }

        if (command == "") return false;
        command = "UPDATE sigmessage SET " + command
                                           + " WHERE SigMessageNum = " + SOut.Long(sigMessage.SigMessageNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SigMessage sigMessage, SigMessage oldSigMessage)
    {
        if (sigMessage.ButtonText != oldSigMessage.ButtonText) return true;
        if (sigMessage.ButtonIndex != oldSigMessage.ButtonIndex) return true;
        if (sigMessage.SynchIcon != oldSigMessage.SynchIcon) return true;
        if (sigMessage.FromUser != oldSigMessage.FromUser) return true;
        if (sigMessage.ToUser != oldSigMessage.ToUser) return true;
        //MessageDateTime not allowed to change
        if (sigMessage.AckDateTime != oldSigMessage.AckDateTime) return true;
        if (sigMessage.SigText != oldSigMessage.SigText) return true;
        if (sigMessage.SigElementDefNumUser != oldSigMessage.SigElementDefNumUser) return true;
        if (sigMessage.SigElementDefNumExtra != oldSigMessage.SigElementDefNumExtra) return true;
        if (sigMessage.SigElementDefNumMsg != oldSigMessage.SigElementDefNumMsg) return true;
        return false;
    }

    public static void Delete(long sigMessageNum)
    {
        var command = "DELETE FROM sigmessage "
                      + "WHERE SigMessageNum = " + SOut.Long(sigMessageNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSigMessageNums)
    {
        if (listSigMessageNums == null || listSigMessageNums.Count == 0) return;
        var command = "DELETE FROM sigmessage "
                      + "WHERE SigMessageNum IN(" + string.Join(",", listSigMessageNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}