using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class UserQueries
{
    public static readonly List<string> ListMassEmailBlacklistCmds =
    [
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
    ];

    public static List<QuerySetStmtObject> GetListQuerySetStmtObjs(string setStmt)
    {
        var strs = SplitQuery(setStmt, false, ",");
        for (var i = 0; i < strs.Count; i++)
        {
            var regex = new Regex(@"\s*set\s+", RegexOptions.IgnoreCase);

            strs[i] = regex.Replace(strs[i], "");
        }

        TrimList(strs);

        strs.RemoveAll(x => string.IsNullOrWhiteSpace(x) || !x.StartsWith("@") || x.StartsWith("@_"));

        var querySetStmtObjects = new List<QuerySetStmtObject>();

        foreach (var s in strs)
        {
            querySetStmtObjects.Add(new QuerySetStmtObject
            {
                Stmt = setStmt,
                Variable = s.Split(['='], 2).First(),
                Value = s.Split(['='], 2).Last()
            });
        }

        return querySetStmtObjects;
    }

    public static List<string> SplitQuery(string query, bool includeDelimeters = false, params string[] splitStrs)
    {
        var s = new List<string>();
        var total = "";
        var charQuoteMode = '-';
        var stackFuncs = new Stack<string>();

        foreach (var ch in query)
        {
            if (charQuoteMode != '-')
            {
                if (ch == charQuoteMode)
                {
                    charQuoteMode = '-';
                }

                total += ch;
            }
            else if (stackFuncs.Count > 0)
            {
                if ((total + ch).ToLower().EndsWith("case"))
                {
                    stackFuncs.Push("end");
                }
                else if ((total + ch).ToLower().EndsWith("("))
                {
                    stackFuncs.Push(")");
                }
                else if ((total + ch).ToLower().EndsWith(stackFuncs.Peek()))
                {
                    stackFuncs.Pop();
                }

                if (ch.In('\'', '"'))
                {
                    charQuoteMode = ch;
                }

                if (charQuoteMode == '-' && stackFuncs.Count == 0 && splitStrs.Contains(ch.ToString()))
                {
                    AddTotalStrToList(ch, includeDelimeters, ref total, ref s);
                }
                else
                {
                    total += ch;
                }
            }
            else
            {
                if ((total + ch).ToLower().EndsWith("case"))
                {
                    stackFuncs.Push("end");
                    total += ch;
                }
                else if ((total + ch).ToLower().EndsWith("("))
                {
                    stackFuncs.Push(")");
                    total += ch;
                }
                else if (splitStrs.Contains(ch.ToString()))
                {
                    AddTotalStrToList(ch, includeDelimeters, ref total, ref s);
                }
                else
                {
                    if (ch is '\'' or '"')
                    {
                        charQuoteMode = ch;
                    }

                    total += ch;
                }
            }
        }

        s.Add(total);

        return s;
    }

    private static void AddTotalStrToList(char c, bool includeDelimeters, ref string strTotal, ref List<string> listStrSplits)
    {
        if (includeDelimeters) strTotal += c;

        listStrSplits.Add(strTotal);
        strTotal = "";
    }

    public static string RemoveSqlComments(string queryText)
    {
        var regexBlockComments = new Regex(@"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/");
        var regexLineComments = new Regex(@"--.*");

        var queryNoComments = regexBlockComments.Replace(queryText, "");

        queryNoComments = regexLineComments.Replace(queryNoComments, "");

        return queryNoComments;
    }

    public static void TrimList(List<string> listTrims)
    {
        for (var i = 0; i < listTrims.Count; i++)
        {
            listTrims[i] = listTrims[i].Trim();
        }
    }

    public static List<string> ParseSetStatements(string queryText)
    {
        queryText = RemoveSqlComments(queryText);

        var parsedSetStmts = new List<string>();
        var splitQueries = SplitQuery(queryText, true, ";");

        foreach (var s in splitQueries)
        {
            parsedSetStmts.AddRange(s.Split(";", StringSplitOptions.RemoveEmptyEntries));
        }

        TrimList(parsedSetStmts);

        parsedSetStmts.RemoveAll(string.IsNullOrEmpty);
        parsedSetStmts = parsedSetStmts.FindAll(x => x.ToLower().StartsWith("set "));

        return parsedSetStmts;
    }

    public static bool ValidateQueryForMassEmail(string command)
    {
        command = RemoveSqlComments(command);

        var regexSchema = new Regex(@"SELECT.*[PatNum|\*].*FROM.*", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var regexBlacklist = new Regex(@$"\b({string.Join("|", ListMassEmailBlacklistCmds.Select(SOut.String))})\b", RegexOptions.IgnoreCase);

        if (!regexSchema.IsMatch(command))
        {
            return false;
        }

        return !regexBlacklist.IsMatch(command);
    }

    public static void Insert(UserQuery userQuery)
    {
        UserQueryCrud.Insert(userQuery);
    }

    public static void Delete(UserQuery userQuery)
    {
        Db.NonQ("DELETE from userquery WHERE querynum = " + userQuery.QueryNum);
    }

    public static void Update(UserQuery userQuery)
    {
        UserQueryCrud.Update(userQuery);
    }

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
            return UserQueryCrud.SelectMany("SELECT * FROM userquery ORDER BY description");
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

    private static readonly UserQueryCache Cache = new();

    public static List<UserQuery> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}

public class QuerySetStmtObject
{
    public string Stmt;
    public string Value;
    public string Variable;
}