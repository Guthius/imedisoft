using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ApptFields
{
    #region Insert

    
    public static long Insert(ApptField apptField)
    {
        return ApptFieldCrud.Insert(apptField);
    }

    #endregion

    #region Update

    /// <summary>
    ///     Deletes any pre-existing appt fields for the AptNum and FieldName combo and then inserts the apptField passed
    ///     in.
    /// </summary>
    public static long Upsert(ApptField apptField)
    {
        //There could already be an appt field in the database due to concurrency.  Delete all entries prior to inserting the new one.
        DeleteFieldForAppt(apptField.FieldName, apptField.AptNum); //Last in wins.
        return Insert(apptField);
    }

    #endregion

    #region Delete

    ///<summary>Deletes all fields for the appointment and field name passed in.</summary>
    public static void DeleteFieldForAppt(string fieldName, long aptNum)
    {
        var command = $@"DELETE FROM apptfield 
				WHERE AptNum = {SOut.Long(aptNum)}
				AND FieldName ='{SOut.String(fieldName)}'";
        Db.NonQ(command);
    }

    #endregion

    #region Get Methods

    ///<summary>Gets one ApptField from the db.</summary>
    public static ApptField GetOne(long apptFieldNum)
    {
        return ApptFieldCrud.SelectOne(apptFieldNum);
    }

    public static List<ApptField> GetForAppt(long aptNum)
    {
        var command = "SELECT * FROM apptfield WHERE AptNum = " + SOut.Long(aptNum);
        return ApptFieldCrud.SelectMany(command);
    }

    #endregion
}