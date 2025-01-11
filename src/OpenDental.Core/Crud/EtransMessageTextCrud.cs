#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EtransMessageTextCrud
{
    public static EtransMessageText SelectOne(long etransMessageTextNum)
    {
        var command = "SELECT * FROM etransmessagetext "
                      + "WHERE EtransMessageTextNum = " + SOut.Long(etransMessageTextNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EtransMessageText SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EtransMessageText> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EtransMessageText> TableToList(DataTable table)
    {
        var retVal = new List<EtransMessageText>();
        EtransMessageText etransMessageText;
        foreach (DataRow row in table.Rows)
        {
            etransMessageText = new EtransMessageText();
            etransMessageText.EtransMessageTextNum = SIn.Long(row["EtransMessageTextNum"].ToString());
            etransMessageText.MessageText = SIn.String(row["MessageText"].ToString());
            retVal.Add(etransMessageText);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EtransMessageText> listEtransMessageTexts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EtransMessageText";
        var table = new DataTable(tableName);
        table.Columns.Add("EtransMessageTextNum");
        table.Columns.Add("MessageText");
        foreach (var etransMessageText in listEtransMessageTexts)
            table.Rows.Add(SOut.Long(etransMessageText.EtransMessageTextNum), etransMessageText.MessageText);
        return table;
    }

    public static long Insert(EtransMessageText etransMessageText)
    {
        return Insert(etransMessageText, false);
    }

    public static long Insert(EtransMessageText etransMessageText, bool useExistingPK)
    {
        var command = "INSERT INTO etransmessagetext (";

        command += "MessageText) VALUES(";

        command +=
            DbHelper.ParamChar + "paramMessageText)";
        if (etransMessageText.MessageText == null) etransMessageText.MessageText = "";
        var paramMessageText = new OdSqlParameter("paramMessageText", OdDbType.Text, SOut.StringParam(etransMessageText.MessageText));
        {
            etransMessageText.EtransMessageTextNum = Db.NonQ(command, true, "EtransMessageTextNum", "etransMessageText", paramMessageText);
        }
        return etransMessageText.EtransMessageTextNum;
    }

    public static long InsertNoCache(EtransMessageText etransMessageText)
    {
        return InsertNoCache(etransMessageText, false);
    }

    public static long InsertNoCache(EtransMessageText etransMessageText, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO etransmessagetext (";
        if (isRandomKeys || useExistingPK) command += "EtransMessageTextNum,";
        command += "MessageText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(etransMessageText.EtransMessageTextNum) + ",";
        command +=
            DbHelper.ParamChar + "paramMessageText)";
        if (etransMessageText.MessageText == null) etransMessageText.MessageText = "";
        var paramMessageText = new OdSqlParameter("paramMessageText", OdDbType.Text, SOut.StringParam(etransMessageText.MessageText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMessageText);
        else
            etransMessageText.EtransMessageTextNum = Db.NonQ(command, true, "EtransMessageTextNum", "etransMessageText", paramMessageText);
        return etransMessageText.EtransMessageTextNum;
    }

    public static void Update(EtransMessageText etransMessageText)
    {
        var command = "UPDATE etransmessagetext SET "
                      + "MessageText         =  " + DbHelper.ParamChar + "paramMessageText "
                      + "WHERE EtransMessageTextNum = " + SOut.Long(etransMessageText.EtransMessageTextNum);
        if (etransMessageText.MessageText == null) etransMessageText.MessageText = "";
        var paramMessageText = new OdSqlParameter("paramMessageText", OdDbType.Text, SOut.StringParam(etransMessageText.MessageText));
        Db.NonQ(command, paramMessageText);
    }

    public static bool Update(EtransMessageText etransMessageText, EtransMessageText oldEtransMessageText)
    {
        var command = "";
        if (etransMessageText.MessageText != oldEtransMessageText.MessageText)
        {
            if (command != "") command += ",";
            command += "MessageText = " + DbHelper.ParamChar + "paramMessageText";
        }

        if (command == "") return false;
        if (etransMessageText.MessageText == null) etransMessageText.MessageText = "";
        var paramMessageText = new OdSqlParameter("paramMessageText", OdDbType.Text, SOut.StringParam(etransMessageText.MessageText));
        command = "UPDATE etransmessagetext SET " + command
                                                  + " WHERE EtransMessageTextNum = " + SOut.Long(etransMessageText.EtransMessageTextNum);
        Db.NonQ(command, paramMessageText);
        return true;
    }

    public static bool UpdateComparison(EtransMessageText etransMessageText, EtransMessageText oldEtransMessageText)
    {
        if (etransMessageText.MessageText != oldEtransMessageText.MessageText) return true;
        return false;
    }

    public static void Delete(long etransMessageTextNum)
    {
        var command = "DELETE FROM etransmessagetext "
                      + "WHERE EtransMessageTextNum = " + SOut.Long(etransMessageTextNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEtransMessageTextNums)
    {
        if (listEtransMessageTextNums == null || listEtransMessageTextNums.Count == 0) return;
        var command = "DELETE FROM etransmessagetext "
                      + "WHERE EtransMessageTextNum IN(" + string.Join(",", listEtransMessageTextNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}