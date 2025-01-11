#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RequiredFieldConditionCrud
{
    public static RequiredFieldCondition SelectOne(long requiredFieldConditionNum)
    {
        var command = "SELECT * FROM requiredfieldcondition "
                      + "WHERE RequiredFieldConditionNum = " + SOut.Long(requiredFieldConditionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RequiredFieldCondition SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RequiredFieldCondition> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RequiredFieldCondition> TableToList(DataTable table)
    {
        var retVal = new List<RequiredFieldCondition>();
        RequiredFieldCondition requiredFieldCondition;
        foreach (DataRow row in table.Rows)
        {
            requiredFieldCondition = new RequiredFieldCondition();
            requiredFieldCondition.RequiredFieldConditionNum = SIn.Long(row["RequiredFieldConditionNum"].ToString());
            requiredFieldCondition.RequiredFieldNum = SIn.Long(row["RequiredFieldNum"].ToString());
            var conditionType = row["ConditionType"].ToString();
            if (conditionType == "")
                requiredFieldCondition.ConditionType = 0;
            else
                try
                {
                    requiredFieldCondition.ConditionType = (RequiredFieldName) Enum.Parse(typeof(RequiredFieldName), conditionType);
                }
                catch
                {
                    requiredFieldCondition.ConditionType = 0;
                }

            requiredFieldCondition.Operator = (ConditionOperator) SIn.Int(row["Operator"].ToString());
            requiredFieldCondition.ConditionValue = SIn.String(row["ConditionValue"].ToString());
            requiredFieldCondition.ConditionRelationship = (LogicalOperator) SIn.Int(row["ConditionRelationship"].ToString());
            retVal.Add(requiredFieldCondition);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RequiredFieldCondition> listRequiredFieldConditions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RequiredFieldCondition";
        var table = new DataTable(tableName);
        table.Columns.Add("RequiredFieldConditionNum");
        table.Columns.Add("RequiredFieldNum");
        table.Columns.Add("ConditionType");
        table.Columns.Add("Operator");
        table.Columns.Add("ConditionValue");
        table.Columns.Add("ConditionRelationship");
        foreach (var requiredFieldCondition in listRequiredFieldConditions)
            table.Rows.Add(SOut.Long(requiredFieldCondition.RequiredFieldConditionNum), SOut.Long(requiredFieldCondition.RequiredFieldNum), SOut.Int((int) requiredFieldCondition.ConditionType), SOut.Int((int) requiredFieldCondition.Operator), requiredFieldCondition.ConditionValue, SOut.Int((int) requiredFieldCondition.ConditionRelationship));
        return table;
    }

    public static long Insert(RequiredFieldCondition requiredFieldCondition)
    {
        return Insert(requiredFieldCondition, false);
    }

    public static long Insert(RequiredFieldCondition requiredFieldCondition, bool useExistingPK)
    {
        var command = "INSERT INTO requiredfieldcondition (";

        command += "RequiredFieldNum,ConditionType,Operator,ConditionValue,ConditionRelationship) VALUES(";

        command +=
            SOut.Long(requiredFieldCondition.RequiredFieldNum) + ","
                                                               + "'" + SOut.String(requiredFieldCondition.ConditionType.ToString()) + "',"
                                                               + SOut.Int((int) requiredFieldCondition.Operator) + ","
                                                               + "'" + SOut.String(requiredFieldCondition.ConditionValue) + "',"
                                                               + SOut.Int((int) requiredFieldCondition.ConditionRelationship) + ")";
        {
            requiredFieldCondition.RequiredFieldConditionNum = Db.NonQ(command, true, "RequiredFieldConditionNum", "requiredFieldCondition");
        }
        return requiredFieldCondition.RequiredFieldConditionNum;
    }

    public static long InsertNoCache(RequiredFieldCondition requiredFieldCondition)
    {
        return InsertNoCache(requiredFieldCondition, false);
    }

    public static long InsertNoCache(RequiredFieldCondition requiredFieldCondition, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO requiredfieldcondition (";
        if (isRandomKeys || useExistingPK) command += "RequiredFieldConditionNum,";
        command += "RequiredFieldNum,ConditionType,Operator,ConditionValue,ConditionRelationship) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(requiredFieldCondition.RequiredFieldConditionNum) + ",";
        command +=
            SOut.Long(requiredFieldCondition.RequiredFieldNum) + ","
                                                               + "'" + SOut.String(requiredFieldCondition.ConditionType.ToString()) + "',"
                                                               + SOut.Int((int) requiredFieldCondition.Operator) + ","
                                                               + "'" + SOut.String(requiredFieldCondition.ConditionValue) + "',"
                                                               + SOut.Int((int) requiredFieldCondition.ConditionRelationship) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            requiredFieldCondition.RequiredFieldConditionNum = Db.NonQ(command, true, "RequiredFieldConditionNum", "requiredFieldCondition");
        return requiredFieldCondition.RequiredFieldConditionNum;
    }

    public static void Update(RequiredFieldCondition requiredFieldCondition)
    {
        var command = "UPDATE requiredfieldcondition SET "
                      + "RequiredFieldNum         =  " + SOut.Long(requiredFieldCondition.RequiredFieldNum) + ", "
                      + "ConditionType            = '" + SOut.String(requiredFieldCondition.ConditionType.ToString()) + "', "
                      + "Operator                 =  " + SOut.Int((int) requiredFieldCondition.Operator) + ", "
                      + "ConditionValue           = '" + SOut.String(requiredFieldCondition.ConditionValue) + "', "
                      + "ConditionRelationship    =  " + SOut.Int((int) requiredFieldCondition.ConditionRelationship) + " "
                      + "WHERE RequiredFieldConditionNum = " + SOut.Long(requiredFieldCondition.RequiredFieldConditionNum);
        Db.NonQ(command);
    }

    public static bool Update(RequiredFieldCondition requiredFieldCondition, RequiredFieldCondition oldRequiredFieldCondition)
    {
        var command = "";
        if (requiredFieldCondition.RequiredFieldNum != oldRequiredFieldCondition.RequiredFieldNum)
        {
            if (command != "") command += ",";
            command += "RequiredFieldNum = " + SOut.Long(requiredFieldCondition.RequiredFieldNum) + "";
        }

        if (requiredFieldCondition.ConditionType != oldRequiredFieldCondition.ConditionType)
        {
            if (command != "") command += ",";
            command += "ConditionType = '" + SOut.String(requiredFieldCondition.ConditionType.ToString()) + "'";
        }

        if (requiredFieldCondition.Operator != oldRequiredFieldCondition.Operator)
        {
            if (command != "") command += ",";
            command += "Operator = " + SOut.Int((int) requiredFieldCondition.Operator) + "";
        }

        if (requiredFieldCondition.ConditionValue != oldRequiredFieldCondition.ConditionValue)
        {
            if (command != "") command += ",";
            command += "ConditionValue = '" + SOut.String(requiredFieldCondition.ConditionValue) + "'";
        }

        if (requiredFieldCondition.ConditionRelationship != oldRequiredFieldCondition.ConditionRelationship)
        {
            if (command != "") command += ",";
            command += "ConditionRelationship = " + SOut.Int((int) requiredFieldCondition.ConditionRelationship) + "";
        }

        if (command == "") return false;
        command = "UPDATE requiredfieldcondition SET " + command
                                                       + " WHERE RequiredFieldConditionNum = " + SOut.Long(requiredFieldCondition.RequiredFieldConditionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(RequiredFieldCondition requiredFieldCondition, RequiredFieldCondition oldRequiredFieldCondition)
    {
        if (requiredFieldCondition.RequiredFieldNum != oldRequiredFieldCondition.RequiredFieldNum) return true;
        if (requiredFieldCondition.ConditionType != oldRequiredFieldCondition.ConditionType) return true;
        if (requiredFieldCondition.Operator != oldRequiredFieldCondition.Operator) return true;
        if (requiredFieldCondition.ConditionValue != oldRequiredFieldCondition.ConditionValue) return true;
        if (requiredFieldCondition.ConditionRelationship != oldRequiredFieldCondition.ConditionRelationship) return true;
        return false;
    }

    public static void Delete(long requiredFieldConditionNum)
    {
        var command = "DELETE FROM requiredfieldcondition "
                      + "WHERE RequiredFieldConditionNum = " + SOut.Long(requiredFieldConditionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRequiredFieldConditionNums)
    {
        if (listRequiredFieldConditionNums == null || listRequiredFieldConditionNums.Count == 0) return;
        var command = "DELETE FROM requiredfieldcondition "
                      + "WHERE RequiredFieldConditionNum IN(" + string.Join(",", listRequiredFieldConditionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}