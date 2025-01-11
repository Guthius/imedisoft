#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SignalodCrud
{
    public static Signalod SelectOne(long signalNum)
    {
        var command = "SELECT * FROM signalod "
                      + "WHERE SignalNum = " + SOut.Long(signalNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Signalod SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Signalod> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Signalod> TableToList(DataTable table)
    {
        var retVal = new List<Signalod>();
        Signalod signalod;
        foreach (DataRow row in table.Rows)
        {
            signalod = new Signalod();
            signalod.SignalNum = SIn.Long(row["SignalNum"].ToString());
            signalod.DateViewing = SIn.Date(row["DateViewing"].ToString());
            signalod.SigDateTime = SIn.DateTime(row["SigDateTime"].ToString());
            signalod.FKey = SIn.Long(row["FKey"].ToString());
            var fKeyType = row["FKeyType"].ToString();
            if (fKeyType == "")
                signalod.FKeyType = 0;
            else
                try
                {
                    signalod.FKeyType = (KeyType) Enum.Parse(typeof(KeyType), fKeyType);
                }
                catch
                {
                    signalod.FKeyType = 0;
                }

            signalod.IType = (InvalidType) SIn.Int(row["IType"].ToString());
            signalod.RemoteRole = SIn.Int(row["RemoteRole"].ToString());
            signalod.MsgValue = SIn.String(row["MsgValue"].ToString());
            retVal.Add(signalod);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Signalod> listSignalods, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Signalod";
        var table = new DataTable(tableName);
        table.Columns.Add("SignalNum");
        table.Columns.Add("DateViewing");
        table.Columns.Add("SigDateTime");
        table.Columns.Add("FKey");
        table.Columns.Add("FKeyType");
        table.Columns.Add("IType");
        table.Columns.Add("RemoteRole");
        table.Columns.Add("MsgValue");
        foreach (var signalod in listSignalods)
            table.Rows.Add(SOut.Long(signalod.SignalNum), SOut.DateT(signalod.DateViewing, false), SOut.DateT(signalod.SigDateTime, false), SOut.Long(signalod.FKey), SOut.Int((int) signalod.FKeyType), SOut.Int((int) signalod.IType), SOut.Int(signalod.RemoteRole), signalod.MsgValue);
        return table;
    }

    public static long Insert(Signalod signalod)
    {
        return Insert(signalod, false);
    }

    public static long Insert(Signalod signalod, bool useExistingPK)
    {
        var command = "INSERT INTO signalod (";

        command += "DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES(";

        command +=
            SOut.Date(signalod.DateViewing) + ","
                                            + DbHelper.Now() + ","
                                            + SOut.Long(signalod.FKey) + ","
                                            + "'" + SOut.String(signalod.FKeyType.ToString()) + "',"
                                            + SOut.Int((int) signalod.IType) + ","
                                            + SOut.Int(signalod.RemoteRole) + ","
                                            + DbHelper.ParamChar + "paramMsgValue)";
        if (signalod.MsgValue == null) signalod.MsgValue = "";
        var paramMsgValue = new OdSqlParameter("paramMsgValue", OdDbType.Text, SOut.StringParam(signalod.MsgValue));
        {
            signalod.SignalNum = Db.NonQ(command, true, "SignalNum", "signalod", paramMsgValue);
        }
        return signalod.SignalNum;
    }

    public static void InsertMany(List<Signalod> listSignalods)
    {
        InsertMany(listSignalods, false);
    }

    public static void InsertMany(List<Signalod> listSignalods, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSignalods.Count)
        {
            var signalod = listSignalods[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO signalod (");
                if (useExistingPK) sbCommands.Append("SignalNum,");
                sbCommands.Append("DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(signalod.SignalNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Date(signalod.DateViewing));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.Long(signalod.FKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(signalod.FKeyType.ToString()) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) signalod.IType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(signalod.RemoteRole));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(signalod.MsgValue) + "'");
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listSignalods.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Signalod signalod)
    {
        return InsertNoCache(signalod, false);
    }

    public static long InsertNoCache(Signalod signalod, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO signalod (";
        if (isRandomKeys || useExistingPK) command += "SignalNum,";
        command += "DateViewing,SigDateTime,FKey,FKeyType,IType,RemoteRole,MsgValue) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(signalod.SignalNum) + ",";
        command +=
            SOut.Date(signalod.DateViewing) + ","
                                            + DbHelper.Now() + ","
                                            + SOut.Long(signalod.FKey) + ","
                                            + "'" + SOut.String(signalod.FKeyType.ToString()) + "',"
                                            + SOut.Int((int) signalod.IType) + ","
                                            + SOut.Int(signalod.RemoteRole) + ","
                                            + DbHelper.ParamChar + "paramMsgValue)";
        if (signalod.MsgValue == null) signalod.MsgValue = "";
        var paramMsgValue = new OdSqlParameter("paramMsgValue", OdDbType.Text, SOut.StringParam(signalod.MsgValue));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMsgValue);
        else
            signalod.SignalNum = Db.NonQ(command, true, "SignalNum", "signalod", paramMsgValue);
        return signalod.SignalNum;
    }

    public static void Update(Signalod signalod)
    {
        var command = "UPDATE signalod SET "
                      + "DateViewing=  " + SOut.Date(signalod.DateViewing) + ", "
                      //SigDateTime not allowed to change
                      + "FKey       =  " + SOut.Long(signalod.FKey) + ", "
                      + "FKeyType   = '" + SOut.String(signalod.FKeyType.ToString()) + "', "
                      + "IType      =  " + SOut.Int((int) signalod.IType) + ", "
                      + "RemoteRole =  " + SOut.Int(signalod.RemoteRole) + ", "
                      + "MsgValue   =  " + DbHelper.ParamChar + "paramMsgValue "
                      + "WHERE SignalNum = " + SOut.Long(signalod.SignalNum);
        if (signalod.MsgValue == null) signalod.MsgValue = "";
        var paramMsgValue = new OdSqlParameter("paramMsgValue", OdDbType.Text, SOut.StringParam(signalod.MsgValue));
        Db.NonQ(command, paramMsgValue);
    }

    public static bool Update(Signalod signalod, Signalod oldSignalod)
    {
        var command = "";
        if (signalod.DateViewing.Date != oldSignalod.DateViewing.Date)
        {
            if (command != "") command += ",";
            command += "DateViewing = " + SOut.Date(signalod.DateViewing) + "";
        }

        //SigDateTime not allowed to change
        if (signalod.FKey != oldSignalod.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(signalod.FKey) + "";
        }

        if (signalod.FKeyType != oldSignalod.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = '" + SOut.String(signalod.FKeyType.ToString()) + "'";
        }

        if (signalod.IType != oldSignalod.IType)
        {
            if (command != "") command += ",";
            command += "IType = " + SOut.Int((int) signalod.IType) + "";
        }

        if (signalod.RemoteRole != oldSignalod.RemoteRole)
        {
            if (command != "") command += ",";
            command += "RemoteRole = " + SOut.Int(signalod.RemoteRole) + "";
        }

        if (signalod.MsgValue != oldSignalod.MsgValue)
        {
            if (command != "") command += ",";
            command += "MsgValue = " + DbHelper.ParamChar + "paramMsgValue";
        }

        if (command == "") return false;
        if (signalod.MsgValue == null) signalod.MsgValue = "";
        var paramMsgValue = new OdSqlParameter("paramMsgValue", OdDbType.Text, SOut.StringParam(signalod.MsgValue));
        command = "UPDATE signalod SET " + command
                                         + " WHERE SignalNum = " + SOut.Long(signalod.SignalNum);
        Db.NonQ(command, paramMsgValue);
        return true;
    }

    public static bool UpdateComparison(Signalod signalod, Signalod oldSignalod)
    {
        if (signalod.DateViewing.Date != oldSignalod.DateViewing.Date) return true;
        //SigDateTime not allowed to change
        if (signalod.FKey != oldSignalod.FKey) return true;
        if (signalod.FKeyType != oldSignalod.FKeyType) return true;
        if (signalod.IType != oldSignalod.IType) return true;
        if (signalod.RemoteRole != oldSignalod.RemoteRole) return true;
        if (signalod.MsgValue != oldSignalod.MsgValue) return true;
        return false;
    }

    public static void Delete(long signalNum)
    {
        var command = "DELETE FROM signalod "
                      + "WHERE SignalNum = " + SOut.Long(signalNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSignalNums)
    {
        if (listSignalNums == null || listSignalNums.Count == 0) return;
        var command = "DELETE FROM signalod "
                      + "WHERE SignalNum IN(" + string.Join(",", listSignalNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}