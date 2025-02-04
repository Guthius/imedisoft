using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class RequiredFieldConditionCrud
{
    public static List<RequiredFieldCondition> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RequiredFieldCondition> TableToList(DataTable table)
    {
        var retVal = new List<RequiredFieldCondition>();
        foreach (DataRow row in table.Rows)
        {
            var requiredFieldCondition = new RequiredFieldCondition
            {
                RequiredFieldConditionNum = SIn.Long(row["RequiredFieldConditionNum"].ToString()),
                RequiredFieldNum = SIn.Long(row["RequiredFieldNum"].ToString())
            };
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

    public static void Insert(RequiredFieldCondition requiredFieldCondition)
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
    }
}