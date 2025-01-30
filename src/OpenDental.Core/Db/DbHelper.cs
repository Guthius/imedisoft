using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class DbHelper
{
    public static string LimitAnd(int n)
    {
        return "LIMIT " + n;
    }

    public static string LimitOrderBy(string commandText, int n)
    {
        return commandText + " LIMIT " + n;
    }

    public static string Concat(params string[] values)
    {
        return "CONCAT(" + string.Join(",", values) + ")";
    }

    public static string GroupConcat(string column, bool distinct = false, bool orderBy = false, string separator = ",")
    {
        return distinct switch
        {
            true when orderBy => "GROUP_CONCAT(DISTINCT " + column + " ORDER BY " + column + " SEPARATOR '" + separator + "')",
            true => "GROUP_CONCAT(DISTINCT " + column + " SEPARATOR '" + separator + "')",
            false when orderBy => "GROUP_CONCAT(" + column + " ORDER BY " + column + " SEPARATOR '" + separator + "')",
            _ => "GROUP_CONCAT(" + column + " SEPARATOR '" + separator + "')"
        };
    }
    
    public static string WhereIn(string query, bool select = true, params List<string>[] argLists)
    {
        const string commandText = "SHOW GLOBAL VARIABLES WHERE Variable_name='eq_range_index_dive_limit'";
        
        var table = DataCore.GetTable(commandText);
        
        var maxInValCount = 0;
        if (table.Rows.Count > 0)
        {
            maxInValCount = SIn.Int(table.Rows[0]["Value"].ToString()) - 1;
        }

        var queries = new List<string> {query};
        for (var i = 0; i < argLists.Length; i++)
        {
            var listColVals = argLists[i].Distinct().ToList();
            
            if (maxInValCount <= 0 || maxInValCount >= listColVals.Count)
            {
                for (var k = 0; k < queries.Count; k++)
                {
                    queries[k] = queries[k].Replace("{" + i + "}", string.Join(",", listColVals));
                }

                continue;
            }

            var listQs = new List<string>();
            for (var j = 0; j < listColVals.Count; j += maxInValCount)
            {
                var listColValsCur = listColVals.GetRange(j, Math.Min(maxInValCount, listColVals.Count - j));
                foreach (var q in queries)
                {
                    listQs.Add(q.Replace("{" + i + "}", string.Join(",", listColValsCur)));
                }
            }

            queries = listQs;
        }

        var separator = select ? " UNION ALL " : ";";
        
        return string.Join(separator, queries);
    }

    public static string UnionOrderBy(string columnName)
    {
        return columnName;
    }

    public static string DateTConditionColumn(string columnName, ConditionOperator comparison, DateTime dateTime)
    {
        var endDate = dateTime;
        
        switch (comparison)
        {
            case ConditionOperator.Equals:
                if (dateTime != DateTime.MaxValue)
                {
                    endDate = endDate.Date.AddDays(1).AddSeconds(-1);
                }

                return columnName + " BETWEEN " + SOut.DateTime(dateTime.Date) + " AND " + SOut.DateTime(endDate);
            
            case ConditionOperator.NotEquals:
                if (dateTime != DateTime.MaxValue)
                {
                    endDate = endDate.Date.AddDays(1).AddSeconds(-1);
                }

                return columnName + " NOT BETWEEN " + SOut.DateTime(dateTime.Date) + " AND " + SOut.DateTime(endDate);
            
            case ConditionOperator.GreaterThan:
                if (dateTime != DateTime.MaxValue)
                {
                    endDate = endDate.Date.AddDays(1);
                }

                return columnName + ">=" + SOut.DateTime(endDate);
            
            case ConditionOperator.LessThan:
                return columnName + " < " + SOut.DateTime(dateTime.Date);
            
            case ConditionOperator.GreaterThanOrEqual:
                return columnName + ">=" + SOut.DateTime(dateTime.Date);
            
            case ConditionOperator.LessThanOrEqual:
                if (dateTime != DateTime.MaxValue)
                {
                    endDate = endDate.Date.AddDays(1).AddSeconds(-1);
                }

                return columnName + "<=" + SOut.DateTime(endDate);
            
            default:
                throw new NotImplementedException(comparison + " not implemented yet.");
        }
    }
    
    public static string LongBetween(string colName, string value, bool fromMobile = false)
    {
        var stringBuilder = new StringBuilder();
        
        if (long.TryParse(value, out var result) && result > 0)
        {
            if (fromMobile)
            {
                stringBuilder.Append("OR (" + SOut.String(colName) + "=" + SOut.Long(result) + " ");
            }
            else
            {
                stringBuilder.Append("AND (" + SOut.String(colName) + "=" + SOut.Long(result) + " ");
            }
            
            for (var i = value.Length + 1; i <= long.MaxValue.ToString().Length; i++)
            {
                var startVal = value.PadRight(i, '0');
                var endVal = value.PadRight(i, '9');
                
                if (!long.TryParse(endVal, out _))
                {
                    break;
                }

                stringBuilder.Append(string.Format("OR " + SOut.String(colName) + " BETWEEN {0} AND {1} ", startVal, endVal));
            }

            stringBuilder.Append(")");
        }
        else if (value.Length > 0)
        {
            stringBuilder.Append("AND FALSE ");
        }

        return stringBuilder.ToString();
    }
    
    public static string BetweenDates(string columnName, DateTime from, DateTime to)
    {
        return DateTConditionColumn(columnName, ConditionOperator.GreaterThanOrEqual, from) + " AND " + 
               DateTConditionColumn(columnName, ConditionOperator.LessThanOrEqual, to);
    }

    public static string DateFormatColumn(string columnName, string format)
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("US"))
        {
            return "DATE_FORMAT(" + columnName + ",'" + format + "')";
        }

        return format switch
        {
            "%c/%d/%Y" => "DATE_FORMAT(" + columnName + ",'%d/%c/%Y')",
            "%m/%d/%Y" => "DATE_FORMAT(" + columnName + ",'%d/%m/%Y')",
            _ => throw new Exception("Unrecognized date format string.")
        };
    }

    public static string DateTFormatColumn(string columnName, string format)
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("US"))
        {
            return "DATE_FORMAT(" + columnName + ",'" + format + "')";
        }

        return format switch
        {
            "%c/%d/%Y %H:%i:%s" => "DATE_FORMAT(" + columnName + ",'%d/%c/%Y %H:%i:%s')",
            "%m/%d/%Y %H:%i:%s" => "DATE_FORMAT(" + columnName + ",'%d/%m/%Y %H:%i:%s')",
            _ => throw new Exception("Unrecognized datetime format string.")
        };
    }

    public static string Regexp(string input, string pattern, bool matches = true)
    {
        return input + (matches ? "" : " NOT") + " REGEXP '" + pattern + "'";
    }

    public const string ParamChar = "@";

    public static string IfNull(string expr, string nullValue, bool encapsulate = true)
    {
        if (encapsulate)
        {
            nullValue = "'" + nullValue + "'";
        }

        return "IFNULL(" + expr + "," + nullValue + ")";
    }
}