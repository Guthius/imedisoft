using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EClipboardImageCaptureDefs
{
    #region Methods - Get

    ///<summary>Retrievs all the eclipboard image capture defs from the db.</summary>
    public static List<EClipboardImageCaptureDef> Refresh()
    {
        var command = "SELECT * FROM eclipboardimagecapturedef";
        return EClipboardImageCaptureDefCrud.SelectMany(command);
    }

    /*
    ///<summary>Gets one EClipboardImageCaptureDef from the db.</summary>
    public static EClipboardImageCaptureDef GetOne(long eClipboardImageCaptureDefNum){

        return Crud.EClipboardImageCaptureDefCrud.SelectOne(eClipboardImageCaptureDefNum);
    }
    */

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(EClipboardImageCaptureDef eClipboardImageCaptureDef)
    {
        return EClipboardImageCaptureDefCrud.Insert(eClipboardImageCaptureDef);
    }

    /*
    
    public static void Update(EClipboardImageCaptureDef eClipboardImageCaptureDef){

        Crud.EClipboardImageCaptureDefCrud.Update(eClipboardImageCaptureDef);
    }

    
    public static void Delete(long eClipboardImageCaptureDefNum) {

        Crud.EClipboardImageCaptureDefCrud.Delete(eClipboardImageCaptureDefNum);
    }
    */

    #endregion Methods - Modify


    #region Methods - Misc

    ///<summary>Inserts, updates, or deletes db rows to match listNew</summary>
    public static bool Sync(List<EClipboardImageCaptureDef> listEClipboardImageCaptureDefsNew, List<EClipboardImageCaptureDef> listEClipboardImageCaptureDefsOld)
    {
        return EClipboardImageCaptureDefCrud.Sync(listEClipboardImageCaptureDefsNew, listEClipboardImageCaptureDefsOld);
    }

    ///<summary>Returns true if eClipboard Image definition is currently being used.</summary>
    public static bool IsEClipboardImageDefInUse(long defNum)
    {
        var command = "SELECT COUNT(*) FROM eclipboardimagecapturedef WHERE DefNum=" + SOut.Long(defNum);
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    #endregion Methods - Misc
}