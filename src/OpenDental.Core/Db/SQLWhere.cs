using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CodeBase;
using DataConnectionBase;

namespace OpenDentBusiness;

[Serializable]
public class SQLWhere
{
    public string WhereClause;

    public static SQLWhere Create<T>(string columnName, ComparisonOperator comparison, T value, bool doTreatDtAsDate = false, string tableName = "")
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            columnName = tableName.ToLower() + "." + columnName;
        }

        return new SQLWhere
        {
            WhereClause = (doTreatDtAsDate ? DbHelper.DtimeToDate(columnName) : columnName) + comparison.GetDescription() + POutObj(value, doTreatDtAsDate)
        };
    }

    public static SQLWhere CreateIn<T>(string columnName, List<T> listValues, bool doTreatDtAsDate = false, string tableName = "")
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            columnName = tableName.ToLower() + "." + columnName;
        }

        var sqlParam = new SQLWhere();
        if (listValues.Count == 0)
        {
            sqlParam.WhereClause = " FALSE ";
        }
        else
        {
            sqlParam.WhereClause = (doTreatDtAsDate ? DbHelper.DtimeToDate(columnName) : columnName) + " IN (" + string.Join(",", listValues.Select(x => POutObj(x, doTreatDtAsDate))) + ")";
        }

        return sqlParam;
    }

    public static SQLWhere CreateNotIn<T>(string columnName, List<T> listValues, bool doTreatDtAsDate = false, string tableName = "")
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            columnName = tableName.ToLower() + "." + columnName;
        }

        var sqlParam = new SQLWhere();
        if (listValues.Count == 0)
        {
            sqlParam.WhereClause = " TRUE ";
        }
        else
        {
            sqlParam.WhereClause = (doTreatDtAsDate ? DbHelper.DtimeToDate(columnName) : columnName) + " NOT IN (" + string.Join(",", listValues.Select(x => POutObj(x, doTreatDtAsDate))) + ")";
        }

        return sqlParam;
    }

    public static SQLWhere CreateBetween<T>(string columnName, T valueLower, T valueHigher, bool doTreatDtAsDate = false, string tableName = "")
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            columnName = tableName.ToLower() + "." + columnName;
        }

        return new SQLWhere
        {
            WhereClause = (doTreatDtAsDate ? DbHelper.DtimeToDate(columnName) : columnName) + " BETWEEN " + POutObj(valueLower, doTreatDtAsDate) + " AND " + POutObj(valueHigher, doTreatDtAsDate) + ""
        };
    }

    public static SQLWhere CreateNotBetween<T>(string columnName, T valueLower, T valueHigher, bool doTreatDtAsDate = false, string tableName = "")
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            columnName = tableName.ToLower() + "." + columnName;
        }

        return new SQLWhere
        {
            WhereClause = (doTreatDtAsDate ? DbHelper.DtimeToDate(columnName) : columnName) + " NOT BETWEEN " + POutObj(valueLower, doTreatDtAsDate) + " AND " + POutObj(valueHigher, doTreatDtAsDate) + ""
        };
    }

    public override string ToString()
    {
        return WhereClause;
    }
    
    private static string POutObj(object value, bool doTreatDtAsDate)
    {
        switch (value)
        {
            case bool b:
                return SOut.Bool(b);
            
            case int i:
                return SOut.Int(i);
            
            case long l:
                return SOut.Long(l);
            
            case DateTime time when doTreatDtAsDate:
                return SOut.Date(time);
            
            case DateTime time:
                return SOut.DateT(time);
            
            case string s:
                return "'" + SOut.String(s) + "'";
            
            case double d:
                return SOut.Double(d);
            
            case decimal value1:
                return SOut.Decimal(value1);
            
            case byte b1:
                return SOut.Byte(b1);
            
            case float f:
                return SOut.Float(f);
            
            case TimeSpan span:
                return "'" + SOut.TSpan(span) + "'";
        }
        
        if (value.GetType().IsEnum)
        {
            return ((int) value).ToString();
        }

        throw new NotImplementedException(value.GetType().Name + " has not been implemented in SQLWhere");
    }
}

public enum ComparisonOperator
{
    [Description("=")]
    Equals,

    [Description("!=")]
    NotEquals,

    [Description(">")]
    GreaterThan,

    [Description(">=")]
    GreaterThanOrEqual,

    [Description("<")]
    LessThan,

    [Description("<=")]
    LessThanOrEqual
}