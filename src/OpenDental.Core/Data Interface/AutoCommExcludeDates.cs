using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AutoCommExcludeDates
{
    public static List<AutoCommExcludeDate> Refresh(long clinicNum)
    {
        var command = $"SELECT * FROM autocommexcludedate WHERE ClinicNum = {clinicNum} ORDER BY autocommexcludedate.DateExclude ASC";
        return AutoCommExcludeDateCrud.SelectMany(command);
    }

    public static List<AutoCommExcludeDate> GetFutureForClinic(long clinicNum)
    {
        var command = $"SELECT * FROM autocommexcludedate WHERE ClinicNum = {clinicNum} AND DateExclude >= CURDATE() ORDER BY autocommexcludedate.DateExclude ASC";
        return AutoCommExcludeDateCrud.SelectMany(command);
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