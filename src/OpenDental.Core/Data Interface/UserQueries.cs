using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class UserQueries
{
    /// <summary>
    ///     List of commands that modify a database. Typically used to parse out modification queries for MassEmail user
    ///     queries.
    /// </summary>
    public static List<string> ListMassEmailBlacklistCmds = new()
    {
        "INSERT",
        "DELETE",
        "ALTER",
        "DROP",
        "ADD",
        "BACKUP",
        "COLUMN",
        "CREATE",
        "SET",
        "UPDATE",
        "TRUNCATE"
    };

    /// <summary>
    ///     Returns the list of variables in the query contained within the passed-in SET statement.
    ///     Pass in one SET statement. Used in conjunction with GetListVals.
    /// </summary>
    public static List<QuerySetStmtObject> GetListQuerySetStmtObjs(string setStmt)
    {
        var listStrSplits = SplitQuery(setStmt, false, ",");
        for (var i = 0; i < listStrSplits.Count; i++)
        {
            var regex = new Regex(@"\s*set\s+", RegexOptions.IgnoreCase);
            listStrSplits[i] = regex.Replace(listStrSplits[i], "");
        }

        TrimList(listStrSplits);
        listStrSplits.RemoveAll(x => string.IsNullOrWhiteSpace(x) || !x.StartsWith("@") || x.StartsWith("@_"));
        var listQuerySetStmtObjects = new List<QuerySetStmtObject>();
        for (var i = 0; i < listStrSplits.Count; i++)
        {
            var querySetStmtObject = new QuerySetStmtObject();
            querySetStmtObject.Stmt = setStmt;
            querySetStmtObject.Variable = listStrSplits[i].Split(new[] {'='}, 2).First();
            querySetStmtObject.Value = listStrSplits[i].Split(new[] {'='}, 2).Last();
            listQuerySetStmtObjects.Add(querySetStmtObject);
        }

        return listQuerySetStmtObjects;
    }

    /// <summary>
    ///     Splits the given query string on the passed-in split string parameters.
    ///     DOES NOT split on the split strings when within single quotes, double quotes, parans, or case/if/concat statements.
    /// </summary>
    public static List<string> SplitQuery(string strQuery, bool includeDelimeters = false, params string[] listSplitStrs)
    {
        var listStrSplits = new List<string>(); //returned list of strings.
        var strTotal = "";
        var charQuoteMode = '-'; //tracks whether we are currently within quotes.
        var stackFuncs = new Stack<string>(); //tracks whether we are currently within a CASE, IF, or CONCAT statement.
        foreach (var c in strQuery)
            //jordan ok to leave foreach
            if (charQuoteMode != '-')
            {
                if (c == charQuoteMode) charQuoteMode = '-';

                strTotal += c;
            }
            else if (stackFuncs.Count > 0)
            {
                if ((strTotal + c).ToLower().EndsWith("case"))
                    stackFuncs.Push("end");
                else if ((strTotal + c).ToLower().EndsWith("("))
                    stackFuncs.Push(")");
                else if ((strTotal + c).ToLower().EndsWith(stackFuncs.Peek())) stackFuncs.Pop();

                if (c.In('\'', '"'))
                    //Function has quotes. Set quote mode.
                    charQuoteMode = c;

                //Only split string if we are not in quote mode and not in a function.
                if (charQuoteMode == '-' && stackFuncs.Count == 0 && listSplitStrs.Contains(c.ToString()))
                    AddTotalStrToList(c, includeDelimeters, ref strTotal, ref listStrSplits);
                else
                    strTotal += c;
            }
            else
            {
                if ((strTotal + c).ToLower().EndsWith("case"))
                {
                    stackFuncs.Push("end");
                    strTotal += c;
                }
                else if ((strTotal + c).ToLower().EndsWith("("))
                {
                    stackFuncs.Push(")");
                    strTotal += c;
                }
                else if (listSplitStrs.Contains(c.ToString()))
                {
                    AddTotalStrToList(c, includeDelimeters, ref strTotal, ref listStrSplits);
                }
                else
                {
                    if (c == '\'' || c == '"') charQuoteMode = c;

                    strTotal += c;
                }
            }

        listStrSplits.Add(strTotal);
        return listStrSplits;
    }

    /// <summary>
    ///     Adds the totalStr to the listStrSplit passed in and then clears the totalStr.  Sets totalStr to the delimeter if
    ///     includeDelimeters
    ///     is true.
    /// </summary>
    private static void AddTotalStrToList(char c, bool includeDelimeters, ref string strTotal, ref List<string> listStrSplits)
    {
        if (includeDelimeters) strTotal += c;

        listStrSplits.Add(strTotal);
        strTotal = "";
    }

    /// <summary>
    ///     Returns a string with SQL comments removed.
    ///     E.g. removes /**/ and -- SQL comments from the query passed in.
    /// </summary>
    public static string RemoveSQLComments(string queryText)
    {
        var regexBlockComments = new Regex(@"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/");
        var regexLineComments = new Regex(@"--.*");
        var queryNoComments = regexBlockComments.Replace(queryText, "");
        queryNoComments = regexLineComments.Replace(queryNoComments, "");
        return queryNoComments;
    }

    ///<summary>Helper method to remove leading and trailing spaces from every element in a list of strings.</summary>
    public static void TrimList(List<string> listTrims)
    {
        for (var i = 0; i < listTrims.Count; i++) listTrims[i] = listTrims[i].Trim();
    }

    /// <summary>
    ///     Takes the passed-in query text and returns a list of SET statements within the query. Pass in the entire
    ///     query.
    /// </summary>
    public static List<string> ParseSetStatements(string queryText)
    {
        queryText = RemoveSQLComments(queryText);
        var listParsedSetStmts = new List<string>(); //Returned list of set statements.
        var listSplitQueries = SplitQuery(queryText, true, ";");
        for (var i = 0; i < listSplitQueries.Count; i++)
            //The list of set statements returned from SplitQuery will include the delimiter(";"). Split each of the set statements using the c# splitter 
            //with the delimiter ";" again incase the query's set statements have invalid apostrophes. We can do this because we don't allow users to enter
            //";" inside a SET statement value.
            listParsedSetStmts.AddRange(listSplitQueries[i].Split(";", StringSplitOptions.RemoveEmptyEntries));

        TrimList(listParsedSetStmts);
        listParsedSetStmts.RemoveAll(x => string.IsNullOrEmpty(x));
        listParsedSetStmts = listParsedSetStmts.FindAll(x => x.ToLower().StartsWith("set "));
        return listParsedSetStmts;
    }

    ///<summary>Takes in a string and returns true if it is safe to run in mass email user queries.</summary>
    public static bool ValidateQueryForMassEmail(string command)
    {
        command = RemoveSQLComments(command);
        var regexSchema = new Regex(@"SELECT.*[PatNum|\*].*FROM.*", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var regexBlacklist = new Regex(@$"\b({string.Join("|", ListMassEmailBlacklistCmds.Select(x => SOut.String(x)))})\b", RegexOptions.IgnoreCase);
        if (!regexSchema.IsMatch(command)) return false;

        if (regexBlacklist.IsMatch(command)) return false;

        return true;
    }

    
    public static long Insert(UserQuery userQuery)
    {
        return UserQueryCrud.Insert(userQuery);
    }

    
    public static void Delete(UserQuery userQuery)
    {
        var command = "DELETE from userquery WHERE querynum = '" + SOut.Long(userQuery.QueryNum) + "'";
        Db.NonQ(command);
    }

    
    public static void Update(UserQuery userQuery)
    {
        UserQueryCrud.Update(userQuery);
    }

    #region CachePattern

    private class UserQueryCache : CacheListAbs<UserQuery>
    {
        protected override UserQuery Copy(UserQuery item)
        {
            return item.Copy();
        }

        protected override void FillCacheIfNeeded()
        {
            UserQueries.GetTableFromCache(false);
        }

        protected override List<UserQuery> GetCacheFromDb()
        {
            var command = "SELECT * FROM userquery ORDER BY description";
            return UserQueryCrud.SelectMany(command);
        }

        protected override DataTable ToDataTable(List<UserQuery> items)
        {
            return UserQueryCrud.ListToTable(items, "UserQuery");
        }

        protected override List<UserQuery> TableToList(DataTable dataTable)
        {
            return UserQueryCrud.TableToList(dataTable);
        }

        protected override bool IsInListShort(UserQuery item)
        {
            return item.IsReleased;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly UserQueryCache _userQueryCache = new();

    public static List<UserQuery> GetDeepCopy(bool isShort = false)
    {
        return _userQueryCache.GetDeepCopy(isShort);
    }

    public static List<UserQuery> GetWhere(Predicate<UserQuery> match, bool isShort = false)
    {
        return _userQueryCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _userQueryCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return _userQueryCache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        _userQueryCache.ClearCache();
    }

    #endregion
}

///<summary>A tiny class that contains a single SET statement's variable, value, and the entire statement.</summary>
public class QuerySetStmtObject
{
    public string Stmt;
    public string Value;
    public string Variable;
}