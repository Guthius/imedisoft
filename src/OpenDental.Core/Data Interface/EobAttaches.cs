using System;
using System.Collections.Generic;
using System.IO;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EobAttaches
{
    ///<summary>Gets all EobAttaches for a given claimpayment.</summary>
    public static List<EobAttach> Refresh(long claimPaymentNum)
    {
        var command = "SELECT * FROM eobattach WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum) + " "
                      + "ORDER BY DateTCreated";
        return EobAttachCrud.SelectMany(command);
    }

    ///<summary>Gets one EobAttach from the db.</summary>
    public static EobAttach GetOne(long eobAttachNum)
    {
        return EobAttachCrud.SelectOne(eobAttachNum);
    }

    ///<summary>Tests to see whether an attachment exists on this claimpayment.</summary>
    public static bool Exists(long claimPaymentNum)
    {
        var command = "SELECT COUNT(*) FROM eobattach WHERE ClaimPaymentNum=" + SOut.Long(claimPaymentNum);
        if (DataCore.GetScalar(command) == "0") return false;
        return true;
    }

    /// <summary>
    ///     Set the extension before calling.  Inserts a new eobattach into db, creates a filename based on EobAttachNum,
    ///     and then updates the db with this filename.  Should always refresh the eobattach after calling this method in order
    ///     to get the correct filename for RemotingRole.ClientWeb.
    /// </summary>
    public static long Insert(EobAttach eobAttach)
    {
        eobAttach.EobAttachNum = EobAttachCrud.Insert(eobAttach);
        //If the current filename is just an extension, then assign it a unique name.
        if (eobAttach.FileName == Path.GetExtension(eobAttach.FileName))
        {
            var extension = eobAttach.FileName;
            eobAttach.FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_") + eobAttach.EobAttachNum + extension;
            Update(eobAttach);
        }

        return eobAttach.EobAttachNum;
    }

    
    public static void Update(EobAttach eobAttach)
    {
        EobAttachCrud.Update(eobAttach);
    }

    
    public static void Delete(long eobAttachNum)
    {
        var command = "DELETE FROM eobattach WHERE EobAttachNum = " + SOut.Long(eobAttachNum);
        Db.NonQ(command);
    }
}