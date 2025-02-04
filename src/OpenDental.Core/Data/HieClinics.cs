using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class HieClinics
{
    public static bool IsEnabled()
    {
        return Db.GetCount("SELECT COUNT(*) FROM hieclinic WHERE PathExportCCD != '' AND IsEnabled = 1") != "0";
    }

    public static List<HieClinic> Refresh()
    {
        return HieClinicCrud.SelectMany("SELECT * FROM hieclinic");
    }

    public static void Sync(List<HieClinic> hieClinics)
    {
        HieClinicCrud.Sync(hieClinics, Refresh());
    }
}