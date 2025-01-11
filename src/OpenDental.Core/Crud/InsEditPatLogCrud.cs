#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsEditPatLogCrud
{
    public static InsEditPatLog SelectOne(long insEditPatLogNum)
    {
        var command = "SELECT * FROM inseditpatlog "
                      + "WHERE InsEditPatLogNum = " + SOut.Long(insEditPatLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsEditPatLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsEditPatLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsEditPatLog> TableToList(DataTable table)
    {
        var retVal = new List<InsEditPatLog>();
        InsEditPatLog insEditPatLog;
        foreach (DataRow row in table.Rows)
        {
            insEditPatLog = new InsEditPatLog();
            insEditPatLog.InsEditPatLogNum = SIn.Long(row["InsEditPatLogNum"].ToString());
            insEditPatLog.FKey = SIn.Long(row["FKey"].ToString());
            insEditPatLog.LogType = (InsEditPatLogType) SIn.Int(row["LogType"].ToString());
            insEditPatLog.FieldName = SIn.String(row["FieldName"].ToString());
            insEditPatLog.OldValue = SIn.String(row["OldValue"].ToString());
            insEditPatLog.NewValue = SIn.String(row["NewValue"].ToString());
            insEditPatLog.UserNum = SIn.Long(row["UserNum"].ToString());
            insEditPatLog.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            insEditPatLog.ParentKey = SIn.Long(row["ParentKey"].ToString());
            insEditPatLog.Description = SIn.String(row["Description"].ToString());
            retVal.Add(insEditPatLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsEditPatLog> listInsEditPatLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsEditPatLog";
        var table = new DataTable(tableName);
        table.Columns.Add("InsEditPatLogNum");
        table.Columns.Add("FKey");
        table.Columns.Add("LogType");
        table.Columns.Add("FieldName");
        table.Columns.Add("OldValue");
        table.Columns.Add("NewValue");
        table.Columns.Add("UserNum");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("ParentKey");
        table.Columns.Add("Description");
        foreach (var insEditPatLog in listInsEditPatLogs)
            table.Rows.Add(SOut.Long(insEditPatLog.InsEditPatLogNum), SOut.Long(insEditPatLog.FKey), SOut.Int((int) insEditPatLog.LogType), insEditPatLog.FieldName, insEditPatLog.OldValue, insEditPatLog.NewValue, SOut.Long(insEditPatLog.UserNum), SOut.DateT(insEditPatLog.DateTStamp, false), SOut.Long(insEditPatLog.ParentKey), insEditPatLog.Description);
        return table;
    }

    public static long Insert(InsEditPatLog insEditPatLog)
    {
        return Insert(insEditPatLog, false);
    }

    public static long Insert(InsEditPatLog insEditPatLog, bool useExistingPK)
    {
        var command = "INSERT INTO inseditpatlog (";

        command += "FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES(";

        command +=
            SOut.Long(insEditPatLog.FKey) + ","
                                          + SOut.Int((int) insEditPatLog.LogType) + ","
                                          + "'" + SOut.String(insEditPatLog.FieldName) + "',"
                                          + "'" + SOut.String(insEditPatLog.OldValue) + "',"
                                          + "'" + SOut.String(insEditPatLog.NewValue) + "',"
                                          + SOut.Long(insEditPatLog.UserNum) + ","
                                          //DateTStamp can only be set by MySQL
                                          + SOut.Long(insEditPatLog.ParentKey) + ","
                                          + "'" + SOut.String(insEditPatLog.Description) + "')";
        {
            insEditPatLog.InsEditPatLogNum = Db.NonQ(command, true, "InsEditPatLogNum", "insEditPatLog");
        }
        return insEditPatLog.InsEditPatLogNum;
    }

    public static void InsertMany(List<InsEditPatLog> listInsEditPatLogs)
    {
        InsertMany(listInsEditPatLogs, false);
    }

    public static void InsertMany(List<InsEditPatLog> listInsEditPatLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listInsEditPatLogs.Count)
        {
            var insEditPatLog = listInsEditPatLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO inseditpatlog (");
                if (useExistingPK) sbCommands.Append("InsEditPatLogNum,");
                sbCommands.Append("FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(insEditPatLog.InsEditPatLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(insEditPatLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) insEditPatLog.LogType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.FieldName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.OldValue) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.NewValue) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(insEditPatLog.UserNum));
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(insEditPatLog.ParentKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(insEditPatLog.Description) + "'");
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
                if (index == listInsEditPatLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(InsEditPatLog insEditPatLog)
    {
        return InsertNoCache(insEditPatLog, false);
    }

    public static long InsertNoCache(InsEditPatLog insEditPatLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO inseditpatlog (";
        if (isRandomKeys || useExistingPK) command += "InsEditPatLogNum,";
        command += "FKey,LogType,FieldName,OldValue,NewValue,UserNum,ParentKey,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insEditPatLog.InsEditPatLogNum) + ",";
        command +=
            SOut.Long(insEditPatLog.FKey) + ","
                                          + SOut.Int((int) insEditPatLog.LogType) + ","
                                          + "'" + SOut.String(insEditPatLog.FieldName) + "',"
                                          + "'" + SOut.String(insEditPatLog.OldValue) + "',"
                                          + "'" + SOut.String(insEditPatLog.NewValue) + "',"
                                          + SOut.Long(insEditPatLog.UserNum) + ","
                                          //DateTStamp can only be set by MySQL
                                          + SOut.Long(insEditPatLog.ParentKey) + ","
                                          + "'" + SOut.String(insEditPatLog.Description) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            insEditPatLog.InsEditPatLogNum = Db.NonQ(command, true, "InsEditPatLogNum", "insEditPatLog");
        return insEditPatLog.InsEditPatLogNum;
    }

    public static void Update(InsEditPatLog insEditPatLog)
    {
        var command = "UPDATE inseditpatlog SET "
                      + "FKey            =  " + SOut.Long(insEditPatLog.FKey) + ", "
                      + "LogType         =  " + SOut.Int((int) insEditPatLog.LogType) + ", "
                      + "FieldName       = '" + SOut.String(insEditPatLog.FieldName) + "', "
                      + "OldValue        = '" + SOut.String(insEditPatLog.OldValue) + "', "
                      + "NewValue        = '" + SOut.String(insEditPatLog.NewValue) + "', "
                      + "UserNum         =  " + SOut.Long(insEditPatLog.UserNum) + ", "
                      //DateTStamp can only be set by MySQL
                      + "ParentKey       =  " + SOut.Long(insEditPatLog.ParentKey) + ", "
                      + "Description     = '" + SOut.String(insEditPatLog.Description) + "' "
                      + "WHERE InsEditPatLogNum = " + SOut.Long(insEditPatLog.InsEditPatLogNum);
        Db.NonQ(command);
    }

    public static bool Update(InsEditPatLog insEditPatLog, InsEditPatLog oldInsEditPatLog)
    {
        var command = "";
        if (insEditPatLog.FKey != oldInsEditPatLog.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(insEditPatLog.FKey) + "";
        }

        if (insEditPatLog.LogType != oldInsEditPatLog.LogType)
        {
            if (command != "") command += ",";
            command += "LogType = " + SOut.Int((int) insEditPatLog.LogType) + "";
        }

        if (insEditPatLog.FieldName != oldInsEditPatLog.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(insEditPatLog.FieldName) + "'";
        }

        if (insEditPatLog.OldValue != oldInsEditPatLog.OldValue)
        {
            if (command != "") command += ",";
            command += "OldValue = '" + SOut.String(insEditPatLog.OldValue) + "'";
        }

        if (insEditPatLog.NewValue != oldInsEditPatLog.NewValue)
        {
            if (command != "") command += ",";
            command += "NewValue = '" + SOut.String(insEditPatLog.NewValue) + "'";
        }

        if (insEditPatLog.UserNum != oldInsEditPatLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(insEditPatLog.UserNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (insEditPatLog.ParentKey != oldInsEditPatLog.ParentKey)
        {
            if (command != "") command += ",";
            command += "ParentKey = " + SOut.Long(insEditPatLog.ParentKey) + "";
        }

        if (insEditPatLog.Description != oldInsEditPatLog.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(insEditPatLog.Description) + "'";
        }

        if (command == "") return false;
        command = "UPDATE inseditpatlog SET " + command
                                              + " WHERE InsEditPatLogNum = " + SOut.Long(insEditPatLog.InsEditPatLogNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(InsEditPatLog insEditPatLog, InsEditPatLog oldInsEditPatLog)
    {
        if (insEditPatLog.FKey != oldInsEditPatLog.FKey) return true;
        if (insEditPatLog.LogType != oldInsEditPatLog.LogType) return true;
        if (insEditPatLog.FieldName != oldInsEditPatLog.FieldName) return true;
        if (insEditPatLog.OldValue != oldInsEditPatLog.OldValue) return true;
        if (insEditPatLog.NewValue != oldInsEditPatLog.NewValue) return true;
        if (insEditPatLog.UserNum != oldInsEditPatLog.UserNum) return true;
        //DateTStamp can only be set by MySQL
        if (insEditPatLog.ParentKey != oldInsEditPatLog.ParentKey) return true;
        if (insEditPatLog.Description != oldInsEditPatLog.Description) return true;
        return false;
    }

    public static void Delete(long insEditPatLogNum)
    {
        var command = "DELETE FROM inseditpatlog "
                      + "WHERE InsEditPatLogNum = " + SOut.Long(insEditPatLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsEditPatLogNums)
    {
        if (listInsEditPatLogNums == null || listInsEditPatLogNums.Count == 0) return;
        var command = "DELETE FROM inseditpatlog "
                      + "WHERE InsEditPatLogNum IN(" + string.Join(",", listInsEditPatLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}