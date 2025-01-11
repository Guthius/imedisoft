using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrLabNotes
{
    
    public static List<EhrLabNote> GetForLab(long ehrLabNum)
    {
        var command = "SELECT * FROM ehrlabnote WHERE EhrLabNum = " + SOut.Long(ehrLabNum) + " AND EhrLabResultNum=0";
        return EhrLabNoteCrud.SelectMany(command);
    }

    
    public static List<EhrLabNote> GetForLabResult(long ehrLabResultNum)
    {
        var command = "SELECT * FROM ehrlabnote WHERE EhrLabResultNum=" + SOut.Long(ehrLabResultNum);
        return EhrLabNoteCrud.SelectMany(command);
    }

    ///<summary>Deletes notes for lab results too.</summary>
    public static void DeleteForLab(long ehrLabNum)
    {
        var command = "DELETE FROM ehrlabnote WHERE EhrLabNum = " + SOut.Long(ehrLabNum);
        Db.NonQ(command);
    }

    
    public static long Insert(EhrLabNote ehrLabNote)
    {
        return EhrLabNoteCrud.Insert(ehrLabNote);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EhrLabNote> Refresh(long patNum){

        string command="SELECT * FROM ehrlabnote WHERE PatNum = "+POut.Long(patNum);
        return Crud.EhrLabNoteCrud.SelectMany(command);
    }

    ///<summary>Gets one EhrLabNote from the db.</summary>
    public static EhrLabNote GetOne(long ehrLabNoteNum){

        return Crud.EhrLabNoteCrud.SelectOne(ehrLabNoteNum);
    }

    
    public static void Update(EhrLabNote ehrLabNote){

        Crud.EhrLabNoteCrud.Update(ehrLabNote);
    }

    
    public static void Delete(long ehrLabNoteNum) {

        string command= "DELETE FROM ehrlabnote WHERE EhrLabNoteNum = "+POut.Long(ehrLabNoteNum);
        Db.NonQ(command);
    }
    */
}