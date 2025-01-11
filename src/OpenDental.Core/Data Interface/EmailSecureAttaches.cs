using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EmailSecureAttaches
{
    
    public static long Insert(EmailSecureAttach emailSecureAttach)
    {
        return EmailSecureAttachCrud.Insert(emailSecureAttach);
    }

    public static void InsertMany(List<EmailSecureAttach> listEmailSecureAttaches)
    {
        EmailSecureAttachCrud.InsertMany(listEmailSecureAttaches);
    }

    
    public static void Update(EmailSecureAttach emailSecureAttach)
    {
        EmailSecureAttachCrud.Update(emailSecureAttach);
    }

    ///<summary>Gets EmailSecureAttaches that have not been successfully downloaded yet.</summary>
    public static List<EmailSecureAttach> GetOutstanding(List<long> listClinicNums)
    {
        //Has not been linked to an EmailAttach yet, therefore, not downloaded yet.
        var command = "SELECT * FROM emailsecureattach WHERE EmailAttachNum=0 ";
        if (!listClinicNums.IsNullOrEmpty()) command += "AND emailsecureattach.ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ")";
        return EmailSecureAttachCrud.SelectMany(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods


    ///<summary>Gets one EmailSecureAttach from the db.</summary>
    public static EmailSecureAttach GetOne(long emailSecureAttachNum){

        return Crud.EmailSecureAttachCrud.SelectOne(emailSecureAttachNum);
    }
    #endregion Get Methods
    #region Modification Methods

    
    public static void Delete(long emailSecureAttachNum) {

        Crud.EmailSecureAttachCrud.Delete(emailSecureAttachNum);
    }
    #endregion Modification Methods
    #region Misc Methods



    #endregion Misc Methods
    */
}