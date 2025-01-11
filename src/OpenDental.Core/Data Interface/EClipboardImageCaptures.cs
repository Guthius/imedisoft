using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EClipboardImageCaptures
{
    #region Methods - Get

    ///<summary>Returns all the image capture records of images a patient has submitted via eClipboard.</summary>
    public static List<EClipboardImageCapture> GetManyByPatNum(long patNum)
    {
        var command = "SELECT * FROM eclipboardimagecapture WHERE PatNum = " + SOut.Long(patNum);
        return EClipboardImageCaptureCrud.SelectMany(command);
    }

    ///<summary>Returns image captures with the following docNum.</summary>
    /*public static List<EClipboardImageCapture> GetManyByDocNum(long docNum) {

        string command="SELECT * FROM eclipboardimagecapture WHERE DocNum = "+POut.Long(docNum);
        return Crud.EClipboardImageCaptureCrud.SelectMany(command);
    }

    ///<summary>Gets one EClipboardImageCapture from the db.</summary>
    public static EClipboardImageCapture GetOne(long EClipboardImageCaptureNum){

        return Crud.EClipboardImageCaptureCrud.SelectOne(EClipboardImageCaptureNum);
    }
    */

    #endregion Methods - Get

    #region Methods - Modify

    ///<summary>Insert a new EClipboardImageCapture object into the db.</summary>
    public static long Insert(EClipboardImageCapture eClipboardImageCapture)
    {
        return EClipboardImageCaptureCrud.Insert(eClipboardImageCapture);
    }

    ///<summary>Updates an existing EClipboardImageCapture record in the db.</summary>
    public static void Update(EClipboardImageCapture eClipboardImageCapture)
    {
        EClipboardImageCaptureCrud.Update(eClipboardImageCapture);
    }

    public static void Upsert(EClipboardImageCapture eClipboardImageCapture)
    {
        //No need to call remoting check, not going to the database (yet)
        if (eClipboardImageCapture.EClipboardImageCaptureNum > 0)
            Update(eClipboardImageCapture);
        else
            Insert(eClipboardImageCapture);
    }

    /*
    
    public static void Delete(long EClipboardImageCaptureNum) {

        Crud.EClipboardImageCaptureCrud.Delete(EClipboardImageCaptureNum);
    }
    */

    
    /*public static void DeleteMany(List<long> listEClipboardImageCaptureNums) {

        Crud.EClipboardImageCaptureCrud.DeleteMany(listEClipboardImageCaptureNums);
    }*/

    ///<summary>Deletes every EClipboardImageCapture record in the DB that references the passed in DocNum.</summary>
    public static void DeleteByDocNum(long docNum)
    {
        var command = "DELETE FROM eclipboardimagecapture WHERE DocNum = " + SOut.Long(docNum);
        Db.NonQ(command);
    }

    #endregion Methods - Modify
}