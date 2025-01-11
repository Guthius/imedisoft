using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PatientPortalInvites
{
    
    public static List<PatientPortalInvite> Refresh(long patNum)
    {
        var command = "SELECT * FROM patientportalinvite WHERE PatNum = " + SOut.Long(patNum);
        return PatientPortalInviteCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets a list of all PatientPortalInvites matching the passed in parameters. To get all PatientPortalInvites, pass in
    ///     no parameters.
    /// </summary>
    public static List<PatientPortalInvite> GetMany(params SQLWhere[] whereClause)
    {
        var listWheres = new List<SQLWhere>();
        foreach (var where in whereClause) listWheres.Add(where);
        return GetMany(listWheres);
    }

    ///<summary>Gets a list of all PatientPortalInvites matching the passed in parameters.</summary>
    public static List<PatientPortalInvite> GetMany(List<SQLWhere> listWheres)
    {
        var command = "SELECT * FROM patientportalinvite ";
        if (listWheres != null && listWheres.Count > 0) command += "WHERE " + string.Join(" AND ", listWheres);
        return PatientPortalInviteCrud.SelectMany(command);
    }

    
    public static void InsertMany(List<PatientPortalInvite> listPatientPortalInvites)
    {
        PatientPortalInviteCrud.InsertMany(listPatientPortalInvites);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods


    ///<summary>Gets one PatientPortalInvite from the db.</summary>
    public static PatientPortalInvite GetOne(long patientPortalInviteNum){

        return Crud.PatientPortalInviteCrud.SelectOne(patientPortalInviteNum);
    }
    #endregion
    #region Modification Methods
        #region Insert
    
    public static long Insert(PatientPortalInvite patientPortalInvite){

        return Crud.PatientPortalInviteCrud.Insert(patientPortalInvite);
    }
        #endregion
        #region Update
    
    public static void Update(PatientPortalInvite patientPortalInvite){

        Crud.PatientPortalInviteCrud.Update(patientPortalInvite);
    }
        #endregion
        #region Delete
    
    public static void Delete(long patientPortalInviteNum) {

        Crud.PatientPortalInviteCrud.Delete(patientPortalInviteNum);
    }
        #endregion
    #endregion
    #region Misc Methods



    #endregion
    */
}