using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ScreenPats
{
    public static long Insert(ScreenPat screenPat)
    {
        return ScreenPatCrud.Insert(screenPat);
    }

    public static List<ScreenPat> GetForScreenGroup(long screenGroupNum)
    {
        var command = "SELECT * FROM screenpat WHERE ScreenGroupNum =" + SOut.Long(screenGroupNum);
        return ScreenPatCrud.SelectMany(command);
    }

    public static bool Sync(List<ScreenPat> listScreenPats, List<ScreenPat> listScreenPatsOld)
    {
        return ScreenPatCrud.Sync(listScreenPats, listScreenPatsOld);
    }
}