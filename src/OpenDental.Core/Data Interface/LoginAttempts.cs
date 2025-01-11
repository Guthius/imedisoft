using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LoginAttempts
{
    ///<summary>Returns the login attempts for the given user in the last X minutes.</summary>
    public static int CountForUser(string userName, UserWebFKeyType userWebFKeyType, int lastXMinutes)
    {
        var command = $@"SELECT COUNT(*) FROM loginattempt WHERE UserName='{SOut.String(userName)}' AND LoginType={SOut.Int((int) userWebFKeyType)}
				AND DateTFail >= {DbHelper.DateAddMinute("NOW()", SOut.Int(-lastXMinutes))}";
        return SIn.Int(Db.GetCount(command));
    }

    
    public static long InsertFailed(string userName, UserWebFKeyType userWebFKeyType)
    {
        return LoginAttemptCrud.Insert(new LoginAttempt {UserName = userName, LoginType = userWebFKeyType});
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods
    
    public static List<LoginAttempt> Refresh(long patNum){

        string command="SELECT * FROM loginattempt WHERE PatNum = "+POut.Long(patNum);
        return Crud.LoginAttemptCrud.SelectMany(command);
    }

    ///<summary>Gets one LoginAttempt from the db.</summary>
    public static LoginAttempt GetOne(long loginAttemptNum){

        return Crud.LoginAttemptCrud.SelectOne(loginAttemptNum);
    }
    #endregion Get Methods
    #region Modification Methods
    #region Insert
    #endregion Insert
    #region Update
    
    public static void Update(LoginAttempt loginAttempt){

        Crud.LoginAttemptCrud.Update(loginAttempt);
    }
    #endregion Update
    #region Delete
    
    public static void Delete(long loginAttemptNum) {

        Crud.LoginAttemptCrud.Delete(loginAttemptNum);
    }
    #endregion Delete
    #endregion Modification Methods
    #region Misc Methods



    #endregion Misc Methods
    */
}