using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class QueryFilters
{
    public static string QueryFilterGroup;
    
    public static List<QueryFilter> GetAll()
    {
        return QueryFilterCrud.SelectMany("SELECT * FROM queryfilter ORDER BY groupName");
    }

    public static List<QueryFilter> GetForGroup(string groupName)
    {
        return QueryFilterCrud.SelectMany("SELECT * FROM queryfilter WHERE GroupName = '" + SOut.String(groupName) + "'");
    }
    
    public static void Insert(QueryFilter queryFilter)
    {
        QueryFilterCrud.Insert(queryFilter);
    }
    
    public static void Update(QueryFilter queryFilter)
    {
        QueryFilterCrud.Update(queryFilter);
    }
    
    public static void Delete(long filterNum)
    {
        QueryFilterCrud.Delete(filterNum);
    }
}