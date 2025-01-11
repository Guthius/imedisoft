using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class UserodApptViews
{
    /// <summary>
    ///     Gets the most recent UserodApptView from the db for the user and clinic.  clinicNum can be 0.  Returns null if
    ///     no match found.
    /// </summary>
    public static UserodApptView GetOneForUserAndClinic(long userNum, long clinicNum)
    {
        var command = "SELECT * FROM userodapptview "
                      + "WHERE UserNum = " + SOut.Long(userNum) + " "
                      + "AND ClinicNum = " + SOut.Long(clinicNum) + " "; //If clinicNum of 0 passed in, we MUST filter by 0 because that is a valid entry in the db.
        return UserodApptViewCrud.SelectOne(command);
    }

    public static void InsertOrUpdate(long userNum, long clinicNum, long apptViewNum)
    {
        var userodApptView = new UserodApptView();
        userodApptView.UserNum = userNum;
        userodApptView.ClinicNum = clinicNum;
        userodApptView.ApptViewNum = apptViewNum;
        //Check if there is already a row in the database for this user, clinic, and apptview.
        var userodApptViewDb = GetOneForUserAndClinic(userodApptView.UserNum, userodApptView.ClinicNum);
        if (userodApptViewDb == null)
        {
            Insert(userodApptView);
        }
        else if (userodApptViewDb.ApptViewNum != userodApptView.ApptViewNum)
        {
            userodApptViewDb.ApptViewNum = userodApptView.ApptViewNum;
            Update(userodApptViewDb);
        }
    }

    public static void Insert(UserodApptView userodApptView)
    {
        UserodApptViewCrud.Insert(userodApptView);
    }

    public static void Update(UserodApptView userodApptView)
    {
        UserodApptViewCrud.Update(userodApptView);
    }
}