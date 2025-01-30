using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class AutoCommExcludeDates
{
    public static List<AutoCommExcludeDate> Refresh(long clinicNum)
    {
        return AutoCommExcludeDateCrud.SelectMany($"SELECT * FROM autocommexcludedate WHERE ClinicNum = {clinicNum} ORDER BY autocommexcludedate.DateExclude ASC");
    }

    public static List<AutoCommExcludeDate> GetFutureForClinic(long clinicNum)
    {
        return AutoCommExcludeDateCrud.SelectMany($"SELECT * FROM autocommexcludedate WHERE ClinicNum = {clinicNum} AND DateExclude >= CURDATE() ORDER BY autocommexcludedate.DateExclude ASC");
    }

    public static void Insert(AutoCommExcludeDate autoCommExcludeDate)
    {
        AutoCommExcludeDateCrud.Insert(autoCommExcludeDate);
    }

    public static void Delete(long autoCommExcludeDateNum)
    {
        AutoCommExcludeDateCrud.Delete(autoCommExcludeDateNum);
    }
}