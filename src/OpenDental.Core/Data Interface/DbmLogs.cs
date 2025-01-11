using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class DbmLogs
{
    public static List<DbmLog> GetByMethodName(string methodName, DateTime date)
    {
        var command = "SELECT * FROM dbmlog WHERE dbmlog.MethodName='" + SOut.String(methodName) + "' AND dbmlog.DateTimeEntry>=" + SOut.DateT(date.Date);
        return DbmLogCrud.SelectMany(command);
    }

    public static void InsertMany(List<DbmLog> listDbmLogs)
    {
        DbmLogCrud.InsertMany(listDbmLogs);
    }
}