#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsEditLogCrud
{
    public static InsEditLog SelectOne(long insEditLogNum)
    {
        var command = "SELECT * FROM inseditlog "
                      + "WHERE InsEditLogNum = " + SOut.Long(insEditLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsEditLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsEditLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsEditLog> TableToList(DataTable table)
    {
        var retVal = new List<InsEditLog>();
        InsEditLog insEditLog;
        foreach (DataRow row in table.Rows)
        {
            insEditLog = new InsEditLog();
            insEditLog.InsEditLogNum = SIn.Long(row["InsEditLogNum"].ToString());
            insEditLog.FKey = SIn.Long(row["FKey"].ToString());
            insEditLog.LogType = (InsEditLogType) SIn.Int(row["LogType"].ToString());
            insEditLog.FieldName = SIn.String(row["FieldName"].ToString());
            insEditLog.OldValue = SIn.String(row["OldValue"].ToString());
            insEditLog.NewValue = SIn.String(row["NewValue"].ToString());
            insEditLog.UserNum = SIn.Long(row["UserNum"].ToString());
            insEditLog.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            insEditLog.ParentKey = SIn.Long(row["ParentKey"].ToString());
            insEditLog.Description = SIn.String(row["Description"].ToString());
            retVal.Add(insEditLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsEditLog> listInsEditLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsEditLog";
        var table = new DataTable(tableName);
        table.Columns.Add("InsEditLogNum");
        table.Columns.Add("FKey");
        table.Columns.Add("LogType");
        table.Columns.Add("FieldName");
        table.Columns.Add("OldValue");
        table.Columns.Add("NewValue");
        table.Columns.Add("UserNum");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("ParentKey");
        table.Columns.Add("Description");
        foreach (var insEditLog in listInsEditLogs)
            table.Rows.Add(SOut.Long(insEditLog.InsEditLogNum), SOut.Long(insEditLog.FKey), SOut.Int((int) insEditLog.LogType), insEditLog.FieldName, insEditLog.OldValue, insEditLog.NewValue, SOut.Long(insEditLog.UserNum), SOut.DateT(insEditLog.DateTStamp, false), SOut.Long(insEditLog.ParentKey), insEditLog.Description);
        return table;
    }

    public static long Insert(InsEditLog insEditLog)
    {
        return Insert(insEditLog, false);
    }

    public static long Insert(InsEditLog insEditLog, bool useExistingPK)
    {
        var command = "INSERT INTO inseditlog (";

        command += "FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES(";

        command +=
            SOut.Long(insEditLog.FKey) + ","
                                       + SOut.Int((int) insEditLog.LogType) + ","
                                       + "'" + SOut.String(insEditLog.FieldName) + "',"
                                       + "'" + SOut.String(insEditLog.OldValue) + "',"
                                       + "'" + SOut.String(insEditLog.NewValue) + "',"
                                       + SOut.Long(insEditLog.UserNum) + ","
                                       //DateTStamp can only be set by MySQL
                                       + SOut.Long(insEditLog.ParentKey) + ","
                                       + "'" + SOut.String(insEditLog.Description) + "')";
        {
            insEditLog.InsEditLogNum = Db.NonQ(command, true, "InsEditLogNum", "insEditLog");
        }
        return insEditLog.InsEditLogNum;
    }

    public static void InsertMany(List<InsEditLog> listInsEditLogs)
    {
        InsertMany(listInsEditLogs, false);
    }

    public static void InsertMany(List<InsEditLog> listInsEditLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listInsEditLogs.Count)
        {
            var insEditLog = listInsEditLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO inseditlog (");
                if (useExistingPK) sbCommands.Append("InsEditLogNum,");
                sbCommands.Append("FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(insEditLog.InsEditLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(insEditLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) insEditLog.LogType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.FieldName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.OldValue) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.NewValue) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(insEditLog.UserNum));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(insEditLog.ParentKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditLog.Description) + "'");
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
                if (index == listInsEditLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(InsEditLog insEditLog)
    {
        return InsertNoCache(insEditLog, false);
    }

    public static long InsertNoCache(InsEditLog insEditLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO inseditlog (";
        if (isRandomKeys || useExistingPK) command += "InsEditLogNum,";
        command += "FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insEditLog.InsEditLogNum) + ",";
        command +=
            SOut.Long(insEditLog.FKey) + ","
                                       + SOut.Int((int) insEditLog.LogType) + ","
                                       + "'" + SOut.String(insEditLog.FieldName) + "',"
                                       + "'" + SOut.String(insEditLog.OldValue) + "',"
                                       + "'" + SOut.String(insEditLog.NewValue) + "',"
                                       + SOut.Long(insEditLog.UserNum) + ","
                                       //DateTStamp can only be set by MySQL
                                       + SOut.Long(insEditLog.ParentKey) + ","
                                       + "'" + SOut.String(insEditLog.Description) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            insEditLog.InsEditLogNum = Db.NonQ(command, true, "InsEditLogNum", "insEditLog");
        return insEditLog.InsEditLogNum;
    }

    public static void Update(InsEditLog insEditLog)
    {
        var command = "UPDATE inseditlog SET "
                      + "FKey         =  " + SOut.Long(insEditLog.FKey) + ", "
                      + "LogType      =  " + SOut.Int((int) insEditLog.LogType) + ", "
                      + "FieldName    = '" + SOut.String(insEditLog.FieldName) + "', "
                      + "OldValue     = '" + SOut.String(insEditLog.OldValue) + "', "
                      + "NewValue     = '" + SOut.String(insEditLog.NewValue) + "', "
                      + "UserNum      =  " + SOut.Long(insEditLog.UserNum) + ", "
                      //DateTStamp can only be set by MySQL
                      + "ParentKey    =  " + SOut.Long(insEditLog.ParentKey) + ", "
                      + "Description  = '" + SOut.String(insEditLog.Description) + "' "
                      + "WHERE InsEditLogNum = " + SOut.Long(insEditLog.InsEditLogNum);
        Db.NonQ(command);
    }

    public static bool Update(InsEditLog insEditLog, InsEditLog oldInsEditLog)
    {
        var command = "";
        if (insEditLog.FKey != oldInsEditLog.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(insEditLog.FKey) + "";
        }

        if (insEditLog.LogType != oldInsEditLog.LogType)
        {
            if (command != "") command += ",";
            command += "LogType = " + SOut.Int((int) insEditLog.LogType) + "";
        }

        if (insEditLog.FieldName != oldInsEditLog.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(insEditLog.FieldName) + "'";
        }

        if (insEditLog.OldValue != oldInsEditLog.OldValue)
        {
            if (command != "") command += ",";
            command += "OldValue = '" + SOut.String(insEditLog.OldValue) + "'";
        }

        if (insEditLog.NewValue != oldInsEditLog.NewValue)
        {
            if (command != "") command += ",";
            command += "NewValue = '" + SOut.String(insEditLog.NewValue) + "'";
        }

        if (insEditLog.UserNum != oldInsEditLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(insEditLog.UserNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (insEditLog.ParentKey != oldInsEditLog.ParentKey)
        {
            if (command != "") command += ",";
            command += "ParentKey = " + SOut.Long(insEditLog.ParentKey) + "";
        }

        if (insEditLog.Description != oldInsEditLog.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(insEditLog.Description) + "'";
        }

        if (command == "") return false;
        command = "UPDATE inseditlog SET " + command
                                           + " WHERE InsEditLogNum = " + SOut.Long(insEditLog.InsEditLogNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(InsEditLog insEditLog, InsEditLog oldInsEditLog)
    {
        if (insEditLog.FKey != oldInsEditLog.FKey) return true;
        if (insEditLog.LogType != oldInsEditLog.LogType) return true;
        if (insEditLog.FieldName != oldInsEditLog.FieldName) return true;
        if (insEditLog.OldValue != oldInsEditLog.OldValue) return true;
        if (insEditLog.NewValue != oldInsEditLog.NewValue) return true;
        if (insEditLog.UserNum != oldInsEditLog.UserNum) return true;
        //DateTStamp can only be set by MySQL
        if (insEditLog.ParentKey != oldInsEditLog.ParentKey) return true;
        if (insEditLog.Description != oldInsEditLog.Description) return true;
        return false;
    }

    public static void Delete(long insEditLogNum)
    {
        var command = "DELETE FROM inseditlog "
                      + "WHERE InsEditLogNum = " + SOut.Long(insEditLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsEditLogNums)
    {
        if (listInsEditLogNums == null || listInsEditLogNums.Count == 0) return;
        var command = "DELETE FROM inseditlog "
                      + "WHERE InsEditLogNum IN(" + string.Join(",", listInsEditLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}