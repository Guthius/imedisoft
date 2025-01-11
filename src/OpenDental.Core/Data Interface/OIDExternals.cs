using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OIDExternals
{
    
    public static long Insert(OIDExternal oIDExternal)
    {
        return OIDExternalCrud.Insert(oIDExternal);
    }

    ///<summary>Under most circumstances this should not be used.</summary>
    public static void Update(OIDExternal oIDExternal)
    {
        OIDExternalCrud.Update(oIDExternal);
    }

    #region Get Methods

    /// <summary>Will return an OIDExternal if both the root and extension match exactly, returns null if not found.</summary>
    /// <param name="rootExternal">The OID of the object.</param>
    /// <param name="iDExternal">If object is identified by only the root, this value should be an empty string.</param>
    public static OIDExternal GetByRootAndExtension(string rootExternal, string iDExternal)
    {
        var command = "SELECT * FROM oidexternal WHERE rootExternal='" + SOut.String(rootExternal) + "' AND IDExternal='" + SOut.String(iDExternal) + "'";
        return OIDExternalCrud.SelectOne(command);
    }

    /// <summary>
    ///     Gets a list of all external ID's for the internal ID and type provided.  Used to construct outbound HL7
    ///     messages.
    /// </summary>
    public static List<OIDExternal> GetByInternalIDAndType(long idInternal, IdentifierType idType)
    {
        var command = "SELECT * FROM oidexternal WHERE IDType='" + idType + "' AND IDInternal=" + SOut.Long(idInternal);
        return OIDExternalCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets the OIDExternal of the corresponding external root.
    ///     This is useful when Open Dental is registering OIDs for other organizations where we don't want to include the
    ///     office's PatNum at HQ.
    ///     Returns null if a root was not found.
    /// </summary>
    public static OIDExternal GetByPartialRootExternal(string rootExternalPartial)
    {
        var command = "SELECT * FROM oidexternal WHERE rootExternal LIKE '" + SOut.String(rootExternalPartial) + "%'"
                      + " AND IDType='" + IdentifierType.Root + "'";
        return OIDExternalCrud.SelectOne(command);
    }

    /// <summary>
    ///     Gets the OidExternal for the given root/internal id/id type.  Should be unique.  Returns null if no match
    ///     found.
    /// </summary>
    public static OIDExternal GetOidExternal(string rootExternal, long idInternal, IdentifierType idType)
    {
        var command = "SELECT * FROM oidexternal WHERE rootExternal='" + SOut.String(rootExternal) + "'"
                      + " AND IDType='" + idType + "' AND IDInternal=" + SOut.Long(idInternal);
        return OIDExternalCrud.SelectOne(command);
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<OIDExternal> Refresh(long patNum){

        string command="SELECT * FROM oidexternal WHERE PatNum = "+POut.Long(patNum);
        return Crud.OIDExternalCrud.SelectMany(command);
    }

    ///<summary>Gets one OIDExternal from the db.</summary>
    public static OIDExternal GetOne(long oIDExternalNum){

        return Crud.OIDExternalCrud.SelectOne(oIDExternalNum);
    }

    
    public static void Delete(long oIDExternalNum) {

        string command= "DELETE FROM oidexternal WHERE OIDExternalNum = "+POut.Long(oIDExternalNum);
        Db.NonQ(command);
    }
    */
}