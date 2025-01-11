using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using ODCrypt;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SessionTokens
{
    
    public static long Insert(SessionToken sessionToken)
    {
        return SessionTokenCrud.Insert(sessionToken);
    }

    ///<summary>Generates a new token and inserts it into the database.</summary>
    public static SessionToken GenerateToken(SessionTokenType sessionTokenType, long fkey)
    {
        var sessionToken = new SessionToken();
        sessionToken.FKey = fkey;
        sessionToken.TokenType = sessionTokenType;
        sessionToken.Expiration = MiscData.GetNowDateTime().AddHours(24);
        //Using 128 bits because it would be impossible to brute force using today's techniques
        sessionToken.TokenPlainText = CryptUtil.RandomString(128);
        sessionToken.SessionTokenHash = GetHash(sessionToken.TokenPlainText);
        Insert(sessionToken);
        //These fields don't need to be sent to the client.
        sessionToken.SessionTokenHash = "";
        sessionToken.SessionTokenNum = 0;
        return sessionToken;
    }

    ///<summary>Hashes the session token using SHA3.</summary>
    private static string GetHash(string sessionToken)
    {
        //Salt not necessary because the token is almost certainly unique.
        var passwordContainer = new PasswordContainer(HashTypes.SHA3_512, "", Authentication.HashPasswordSHA512(sessionToken));
        return passwordContainer.ToString();
    }

    ///<summary>Checks that the passed in token is valid. Throws if not. Returns the matching token from the database.</summary>
    public static SessionToken CheckToken(string sessionToken, SessionTokenType sessionTokenType, long fkey)
    {
        return CheckToken(sessionToken, new List<SessionTokenType> {sessionTokenType}, fkey);
    }

    ///<summary>Checks that the passed in token is valid. Throws if not. Returns the matching token from the database.</summary>
    public static SessionToken CheckToken(string token, List<SessionTokenType> listTokenTypes, long fkey)
    {
        if (token.IsNullOrEmpty()) throw new ODException("Invalid credentials");
        if (listTokenTypes.Count == 0) throw new Exception("Token type required");
        var tokenHash = GetHash(token);
        var command = $"SELECT *,NOW() DatabaseTime FROM sessiontoken WHERE SessionTokenHash='{SOut.String(tokenHash)}' ";
        if (listTokenTypes.Any(x => x != SessionTokenType.Undefined)) command += $"AND TokenType IN({string.Join(",", listTokenTypes.Select(x => SOut.Int((int) x)))}) ";
        var table = DataCore.GetTable(command);
        var listSessionTokens = SessionTokenCrud.TableToList(table);
        if (listSessionTokens.Count == 0) throw new ODException("Invalid credentials");
        var sessionToken = listSessionTokens[0];
        var dateTimeDb = SIn.DateTime(table.Rows[0]["DatabaseTime"].ToString());
        if (sessionToken.Expiration < dateTimeDb)
            //Normally won't get here because apps will request a new token before it expires.
            throw new ODException("Session has expired.", ODException.ErrorCodes.SessionExpired);
        var isAuthorized = false;
        if (fkey == 0 || fkey == sessionToken.FKey
                      //Check if the FKey for the token is allowed to view the patient for the request
                      || (listTokenTypes.Contains(SessionTokenType.PatientPortal) && Patients.GetPatNumsForPhi(sessionToken.FKey).Contains(fkey)))
            isAuthorized = true;
        if (!isAuthorized)
            //For example, the token is for a different patient than the patient for which information is being requested.
            throw new ODException("Invalid credentials");
        return sessionToken;
    }

    ///<summary>Deletes the token if it is present in the database.</summary>
    public static void DeleteToken(string sessionToken)
    {
        if (sessionToken.IsNullOrEmpty()) throw new ODException("Invalid credentials");
        var tokenHash = GetHash(sessionToken);
        var command = $"DELETE FROM sessiontoken WHERE SessionTokenHash='{SOut.String(tokenHash)}'";
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods
    
    public static List<SessionToken> Refresh(long patNum){

        string command="SELECT * FROM sessiontoken WHERE PatNum = "+POut.Long(patNum);
        return Crud.SessionTokenCrud.SelectMany(command);
    }

    ///<summary>Gets one SessionToken from the db.</summary>
    public static SessionToken GetOne(long sessionTokenNum){

        return Crud.SessionTokenCrud.SelectOne(sessionTokenNum);
    }
    #endregion Get Methods
    #region Modification Methods
    
    public static void Update(SessionToken sessionToken){

        Crud.SessionTokenCrud.Update(sessionToken);
    }
    
    public static void Delete(long sessionTokenNum) {

        Crud.SessionTokenCrud.Delete(sessionTokenNum);
    }
    #endregion Modification Methods
    #region Misc Methods



    #endregion Misc Methods
    */
}