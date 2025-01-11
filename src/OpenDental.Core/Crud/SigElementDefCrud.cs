#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SigElementDefCrud
{
    public static SigElementDef SelectOne(long sigElementDefNum)
    {
        var command = "SELECT * FROM sigelementdef "
                      + "WHERE SigElementDefNum = " + SOut.Long(sigElementDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SigElementDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SigElementDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SigElementDef> TableToList(DataTable table)
    {
        var retVal = new List<SigElementDef>();
        SigElementDef sigElementDef;
        foreach (DataRow row in table.Rows)
        {
            sigElementDef = new SigElementDef();
            sigElementDef.SigElementDefNum = SIn.Long(row["SigElementDefNum"].ToString());
            sigElementDef.LightRow = SIn.Byte(row["LightRow"].ToString());
            sigElementDef.LightColor = Color.FromArgb(SIn.Int(row["LightColor"].ToString()));
            sigElementDef.SigElementType = (SignalElementType) SIn.Int(row["SigElementType"].ToString());
            sigElementDef.SigText = SIn.String(row["SigText"].ToString());
            sigElementDef.Sound = SIn.String(row["Sound"].ToString());
            sigElementDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            retVal.Add(sigElementDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SigElementDef> listSigElementDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SigElementDef";
        var table = new DataTable(tableName);
        table.Columns.Add("SigElementDefNum");
        table.Columns.Add("LightRow");
        table.Columns.Add("LightColor");
        table.Columns.Add("SigElementType");
        table.Columns.Add("SigText");
        table.Columns.Add("Sound");
        table.Columns.Add("ItemOrder");
        foreach (var sigElementDef in listSigElementDefs)
            table.Rows.Add(SOut.Long(sigElementDef.SigElementDefNum), SOut.Byte(sigElementDef.LightRow), SOut.Int(sigElementDef.LightColor.ToArgb()), SOut.Int((int) sigElementDef.SigElementType), sigElementDef.SigText, sigElementDef.Sound, SOut.Int(sigElementDef.ItemOrder));
        return table;
    }

    public static long Insert(SigElementDef sigElementDef)
    {
        return Insert(sigElementDef, false);
    }

    public static long Insert(SigElementDef sigElementDef, bool useExistingPK)
    {
        var command = "INSERT INTO sigelementdef (";

        command += "LightRow,LightColor,SigElementType,SigText,Sound,ItemOrder) VALUES(";

        command +=
            SOut.Byte(sigElementDef.LightRow) + ","
                                              + SOut.Int(sigElementDef.LightColor.ToArgb()) + ","
                                              + SOut.Int((int) sigElementDef.SigElementType) + ","
                                              + "'" + SOut.String(sigElementDef.SigText) + "',"
                                              + DbHelper.ParamChar + "paramSound,"
                                              + SOut.Int(sigElementDef.ItemOrder) + ")";
        if (sigElementDef.Sound == null) sigElementDef.Sound = "";
        var paramSound = new OdSqlParameter("paramSound", OdDbType.Text, SOut.StringParam(sigElementDef.Sound));
        {
            sigElementDef.SigElementDefNum = Db.NonQ(command, true, "SigElementDefNum", "sigElementDef", paramSound);
        }
        return sigElementDef.SigElementDefNum;
    }

    public static long InsertNoCache(SigElementDef sigElementDef)
    {
        return InsertNoCache(sigElementDef, false);
    }

    public static long InsertNoCache(SigElementDef sigElementDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sigelementdef (";
        if (isRandomKeys || useExistingPK) command += "SigElementDefNum,";
        command += "LightRow,LightColor,SigElementType,SigText,Sound,ItemOrder) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sigElementDef.SigElementDefNum) + ",";
        command +=
            SOut.Byte(sigElementDef.LightRow) + ","
                                              + SOut.Int(sigElementDef.LightColor.ToArgb()) + ","
                                              + SOut.Int((int) sigElementDef.SigElementType) + ","
                                              + "'" + SOut.String(sigElementDef.SigText) + "',"
                                              + DbHelper.ParamChar + "paramSound,"
                                              + SOut.Int(sigElementDef.ItemOrder) + ")";
        if (sigElementDef.Sound == null) sigElementDef.Sound = "";
        var paramSound = new OdSqlParameter("paramSound", OdDbType.Text, SOut.StringParam(sigElementDef.Sound));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramSound);
        else
            sigElementDef.SigElementDefNum = Db.NonQ(command, true, "SigElementDefNum", "sigElementDef", paramSound);
        return sigElementDef.SigElementDefNum;
    }

    public static void Update(SigElementDef sigElementDef)
    {
        var command = "UPDATE sigelementdef SET "
                      + "LightRow        =  " + SOut.Byte(sigElementDef.LightRow) + ", "
                      + "LightColor      =  " + SOut.Int(sigElementDef.LightColor.ToArgb()) + ", "
                      + "SigElementType  =  " + SOut.Int((int) sigElementDef.SigElementType) + ", "
                      + "SigText         = '" + SOut.String(sigElementDef.SigText) + "', "
                      + "Sound           =  " + DbHelper.ParamChar + "paramSound, "
                      + "ItemOrder       =  " + SOut.Int(sigElementDef.ItemOrder) + " "
                      + "WHERE SigElementDefNum = " + SOut.Long(sigElementDef.SigElementDefNum);
        if (sigElementDef.Sound == null) sigElementDef.Sound = "";
        var paramSound = new OdSqlParameter("paramSound", OdDbType.Text, SOut.StringParam(sigElementDef.Sound));
        Db.NonQ(command, paramSound);
    }

    public static bool Update(SigElementDef sigElementDef, SigElementDef oldSigElementDef)
    {
        var command = "";
        if (sigElementDef.LightRow != oldSigElementDef.LightRow)
        {
            if (command != "") command += ",";
            command += "LightRow = " + SOut.Byte(sigElementDef.LightRow) + "";
        }

        if (sigElementDef.LightColor != oldSigElementDef.LightColor)
        {
            if (command != "") command += ",";
            command += "LightColor = " + SOut.Int(sigElementDef.LightColor.ToArgb()) + "";
        }

        if (sigElementDef.SigElementType != oldSigElementDef.SigElementType)
        {
            if (command != "") command += ",";
            command += "SigElementType = " + SOut.Int((int) sigElementDef.SigElementType) + "";
        }

        if (sigElementDef.SigText != oldSigElementDef.SigText)
        {
            if (command != "") command += ",";
            command += "SigText = '" + SOut.String(sigElementDef.SigText) + "'";
        }

        if (sigElementDef.Sound != oldSigElementDef.Sound)
        {
            if (command != "") command += ",";
            command += "Sound = " + DbHelper.ParamChar + "paramSound";
        }

        if (sigElementDef.ItemOrder != oldSigElementDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(sigElementDef.ItemOrder) + "";
        }

        if (command == "") return false;
        if (sigElementDef.Sound == null) sigElementDef.Sound = "";
        var paramSound = new OdSqlParameter("paramSound", OdDbType.Text, SOut.StringParam(sigElementDef.Sound));
        command = "UPDATE sigelementdef SET " + command
                                              + " WHERE SigElementDefNum = " + SOut.Long(sigElementDef.SigElementDefNum);
        Db.NonQ(command, paramSound);
        return true;
    }

    public static bool UpdateComparison(SigElementDef sigElementDef, SigElementDef oldSigElementDef)
    {
        if (sigElementDef.LightRow != oldSigElementDef.LightRow) return true;
        if (sigElementDef.LightColor != oldSigElementDef.LightColor) return true;
        if (sigElementDef.SigElementType != oldSigElementDef.SigElementType) return true;
        if (sigElementDef.SigText != oldSigElementDef.SigText) return true;
        if (sigElementDef.Sound != oldSigElementDef.Sound) return true;
        if (sigElementDef.ItemOrder != oldSigElementDef.ItemOrder) return true;
        return false;
    }

    public static void Delete(long sigElementDefNum)
    {
        var command = "DELETE FROM sigelementdef "
                      + "WHERE SigElementDefNum = " + SOut.Long(sigElementDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSigElementDefNums)
    {
        if (listSigElementDefNums == null || listSigElementDefNums.Count == 0) return;
        var command = "DELETE FROM sigelementdef "
                      + "WHERE SigElementDefNum IN(" + string.Join(",", listSigElementDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}