using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class HL7ProcAttaches
{
    
    public static long Insert(HL7ProcAttach hL7ProcAttach)
    {
        return HL7ProcAttachCrud.Insert(hL7ProcAttach);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<HL7ProcAttach> Refresh(long patNum){

        string command="SELECT * FROM hl7procattach WHERE PatNum = "+POut.Long(patNum);
        return Crud.HL7ProcAttachCrud.SelectMany(command);
    }

    ///<summary>Gets one HL7ProcAttach from the db.</summary>
    public static HL7ProcAttach GetOne(long hL7ProcAttachNum){

        return Crud.HL7ProcAttachCrud.SelectOne(hL7ProcAttachNum);
    }

    
    public static void Update(HL7ProcAttach hL7ProcAttach){

        Crud.HL7ProcAttachCrud.Update(hL7ProcAttach);
    }

    
    public static void Delete(long hL7ProcAttachNum) {

        Crud.HL7ProcAttachCrud.Delete(hL7ProcAttachNum);
    }

    */
}