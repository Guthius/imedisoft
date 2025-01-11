using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ScreenPats
{
    
    public static long Insert(ScreenPat screenPat)
    {
        return ScreenPatCrud.Insert(screenPat);
    }

    /// <summary></summary>
    public static List<ScreenPat> GetForScreenGroup(long screenGroupNum)
    {
        var command = "SELECT * FROM screenpat WHERE ScreenGroupNum =" + SOut.Long(screenGroupNum);
        return ScreenPatCrud.SelectMany(command);
    }

    ///<summary>Inserts, updates, or deletes rows to reflect changes between listScreenPats and stale listScreenPatsOld.</summary>
    public static bool Sync(List<ScreenPat> listScreenPats, List<ScreenPat> listScreenPatsOld)
    {
        return ScreenPatCrud.Sync(listScreenPats, listScreenPatsOld);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<ScreenPat> Refresh(long patNum){

        string command="SELECT * FROM screenpat WHERE PatNum = "+POut.Long(patNum);
        return Crud.ScreenPatCrud.SelectMany(command);
    }

    ///<summary>Gets one ScreenPat from the db.</summary>
    public static ScreenPat GetOne(long screenPatNum){

        return Crud.ScreenPatCrud.SelectOne(screenPatNum);
    }


    
    public static void Update(ScreenPat screenPat){

        Crud.ScreenPatCrud.Update(screenPat);
    }

    
    public static void Delete(long screenPatNum) {

        string command= "DELETE FROM screenpat WHERE ScreenPatNum = "+POut.Long(screenPatNum);
        Db.NonQ(command);
    }
    */
}