using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataConnectionBase;

namespace OpenDentBusiness;

public class DbHelper
{
    public static string ClobOrderBy(string columnName)
    {
        return columnName;
    }

    public static string LimitAnd(int n)
    {
        return "LIMIT " + n;
    }

    public static string LimitWhere(int n)
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

    public static string UseIndex(string indexName)
    {
        return "USE INDEX(" + indexName + ")";
    }

    public static string DateAddDay(string date, string days)
    {
        return "ADDDATE(" + date + "," + days + ")";
    }

    public static string DateAddMonth(string date, string months)
    {
        return "ADDDATE(" + date + ",INTERVAL " + months + " MONTH)";
    }

    public static string DateAddYear(string date, string years)
    {
        return "ADDDATE(" + date + ",INTERVAL " + years + " YEAR)";
    }

    public static string DateAddMinute(string date, string minutes)
    {
        return "ADDDATE(" + date + ",INTERVAL " + minutes + " MINUTE)";
    }

    public static string DateAddSecond(string date, string seconds)
    {
        return "ADDDATE(" + date + ",INTERVAL " + seconds + " SECOND)";
    }
    
    public static string DtimeToDate(string columnName)
    {
        return "DATE(" + columnName + ")";
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

                return columnName + " BETWEEN " + SOut.DateT(dateTime.Date) + " AND " + SOut.DateT(endDate);
            
            case ConditionOperator.NotEquals:
                if (dateTime != DateTime.MaxValue)
                {
                    endDate = endDate.Date.AddDays(1).AddSeconds(-1);
                }

                return columnName + " NOT BETWEEN " + SOut.DateT(dateTime.Date) + " AND " + SOut.DateT(endDate);
            
            case ConditionOperator.GreaterThan:
                if (dateTime != DateTime.MaxValue)
                {
                    endDate = endDate.Date.AddDays(1);
                }

                return columnName + ">=" + SOut.DateT(endDate);
            
            case ConditionOperator.LessThan:
                return columnName + " < " + SOut.DateT(dateTime.Date);
            
            case ConditionOperator.GreaterThanOrEqual:
                return columnName + ">=" + SOut.DateT(dateTime.Date);
            
            case ConditionOperator.LessThanOrEqual:
                if (dateTime != DateTime.MaxValue)
                {
                    endDate = endDate.Date.AddDays(1).AddSeconds(-1);
                }

                return columnName + "<=" + SOut.DateT(endDate);
            
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
    
    public static string Curdate()
    {
        return "CURDATE()";
    }
    
    public static string Now()
    {
        return "NOW()";
    }

    public static string Year(string date)
    {
        return "YEAR(" + date + ")";
    }

    public static string Regexp(string input, string pattern, bool matches = true)
    {
        return input + (matches ? "" : " NOT") + " REGEXP '" + pattern + "'";
    }

    public const string ParamChar = "@";

    public static bool IsMySqlReservedWord(string str)
    {
        var retval = str.ToUpper() switch
        {
            "ACCESSIBLE" or "ADD" or "ALL" or "ALTER" or "ANALYZE" or "AND" or "AS" or "ASC" or "ASENSITIVE" or "BEFORE" or "BETWEEN" or "BIGINT" or "BINARY" or "BLOB" or "BOTH" or "BY" or "CALL" or "CASCADE" or "CASE" or "CHANGE" or "CHAR" or "CHARACTER" or "CHECK" or "COLLATE" or "COLUMN" or "CONDITION" or "CONSTRAINT" or "CONTINUE" or "CONVERT" or "CREATE" or "CROSS" or "CURRENT_DATE" or "CURRENT_TIME" or "CURRENT_TIMESTAMP" or "CURRENT_USER" or "CURSOR" or "DATABASE" or "DATABASES" or "DAY_HOUR" or "DAY_MICROSECOND" or "DAY_MINUTE" or "DAY_SECOND" or "DEC" or "DECIMAL" or "DECLARE" or "DEFAULT" or "DELAYED" or "DELETE" or "DESC" or "DESCRIBE" or "DETERMINISTIC" or "DISTINCT" or "DISTINCTROW" or "DIV" or "DOUBLE" or "DROP" or "DUAL" or "EACH" or "ELSE" or "ELSEIF" or "ENCLOSED" or "ESCAPED" or "EXISTS" or "EXIT" or "EXPLAIN" or "FALSE" or "FETCH" or "FLOAT" or "FLOAT4" or "FLOAT8" or "FOR" or "FORCE" or "FOREIGN" or "FROM" or "FULLTEXT" or "GENERAL" or "GET" or "GRANT" or "GROUP" or "HAVING" or "HIGH_PRIORITY" or "HOUR_MICROSECOND" or "HOUR_MINUTE" or "HOUR_SECOND" or "IF" or "IGNORE" or "IGNORE_SERVER_IDS" or "IN" or "INDEX" or "INFILE" or "INNER" or "INOUT" or "INSENSITIVE" or "INSERT" or "INT" or "INT1" or "INT2" or "INT3" or "INT4" or "INT8" or "INTEGER" or "INTERVAL" or "INTO" or "IO_AFTER_GTIDS" or "IO_BEFORE_GTIDS" or "IS" or "ITERATE" or "JOIN" or "KEY" or "KEYS" or "KILL" or "LEADING" or "LEAVE" or "LEFT" or "LIKE" or "LIMIT" or "LINEAR" or "LINES" or "LOAD" or "LOCALTIME" or "LOCALTIMESTAMP" or "LOCK" or "LONG" or "LONGBLOB" or "LONGTEXT" or "LOOP" or "LOW_PRIORITY" or "MASTER_BIND" or "MASTER_HEARTBEAT_PERIOD" or "MASTER_SSL_VERIFY_SERVER_CERT" or "MATCH" or "MAXVALUE" or "MEDIUMBLOB" or "MEDIUMINT" or "MEDIUMTEXT" or "MIDDLEINT" or "MINUTE_MICROSECOND" or "MINUTE_SECOND" or "MOD" or "MODIFIES" or "NATURAL" or "NOT" or "NO_WRITE_TO_BINLOG" or "NULL" or "NUMERIC" or "ON" or "ONE_SHOT" or "OPTIMIZE" or "OPTION" or "OPTIONALLY" or "OR" or "ORDER" or "OUT" or "OUTER" or "OUTFILE" or "PARTITION" or "PRECISION" or "PRIMARY" or "PROCEDURE" or "PURGE" or "RANGE" or "READ" or "READS" or "READ_WRITE" or "REAL" or "REFERENCES" or "REGEXP" or "RELEASE" or "RENAME" or "REPEAT" or "REPLACE" or "REQUIRE" or "RESIGNAL" or "RESTRICT" or "RETURN" or "REVOKE" or "RIGHT" or "RLIKE" or "SCHEMA" or "SCHEMAS" or "SECOND_MICROSECOND" or "SELECT" or "SENSITIVE" or "SEPARATOR" or "SET" or "SHOW" or "SIGNAL" or "SLOW" or "SMALLINT" or "SPATIAL" or "SPECIFIC" or "SQL" or "SQLEXCEPTION" or "SQLSTATE" or "SQLWARNING" or "SQL_AFTER_GTIDS" or "SQL_BEFORE_GTIDS" or "SQL_BIG_RESULT" or "SQL_CALC_FOUND_ROWS" or "SQL_SMALL_RESULT" or "SSL" or "STARTING" or "STRAIGHT_JOIN" or "TABLE" or "TERMINATED" or "THEN" or "TINYBLOB" or "TINYINT" or "TINYTEXT" or "TO" or "TRAILING" or "TRIGGER" or "TRUE" or "UNDO" or "UNION" or "UNIQUE" or "UNLOCK" or "UNSIGNED" or "UPDATE" or "USAGE" or "USE" or "USING" or "UTC_DATE" or "UTC_TIME" or "UTC_TIMESTAMP" or "VALUES" or "VARBINARY" or "VARCHAR" or "VARCHARACTER" or "VARYING" or "WHEN" or "WHERE" or "WHILE" or "WITH" or "WRITE" or "XOR" or "YEAR_MONTH" or "ZEROFILL" => true,
            _ => false
        };

        return retval || Regex.IsMatch(str, WikiListHeaderWidths.DummyColName);
    }

    public static string IfNull(string expr, string nullValue, bool encapsulate = true)
    {
        if (encapsulate)
        {
            nullValue = "'" + nullValue + "'";
        }

        return "IFNULL(" + expr + "," + nullValue + ")";
    }
}