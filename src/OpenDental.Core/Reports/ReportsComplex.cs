using System;
using System.Data;

namespace OpenDentBusiness;

public class ReportsComplex
{
    public static DataTable GetTable(string command)
    {
        return DataCore.GetTable(command);
    }

    public static T RunFuncOnReportServer<T>(Func<T> func)
    {
        return func();
    }

    public static T RunFuncOnReadOnlyServer<T>(Func<T> func)
    {
        return func();
    }
}