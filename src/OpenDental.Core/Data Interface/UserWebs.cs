using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CodeBase;
using DataConnectionBase;
using ODCrypt;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class UserWebs
{
    #region Insert

    
    public static long Insert(UserWeb userWeb)
    {
        return UserWebCrud.Insert(userWeb);
    }

    #endregion

    #region Delete

    
    public static void Delete(long userWebNum)
    {
        UserWebCrud.Delete(userWebNum);
    }

    #endregion

    #region Get Methods

    ///<summary>Returns true if the patient has portal access.</summary>
    public static bool HasPatientPortalAccess(long patNum)
    {
        var command = "SELECT * FROM userweb WHERE FKeyType=" + SOut.Int((int) UserWebFKeyType.PatientPortal) + " "
                      + "AND FKey=" + SOut.Long(patNum);
        var userWeb = UserWebCrud.SelectOne(command);
        if (userWeb != null && userWeb.PasswordHash != "") return true;

        return false;
    }

    public static UserWeb GetByFKeyAndType(long fKey, UserWebFKeyType userWebFKeyType, bool checkOnlineStatus = false)
    {
        var command = "SELECT * FROM userweb WHERE FKey=" + SOut.Long(fKey) + " AND FKeyType=" + SOut.Int((int) userWebFKeyType) + " ";
        if (checkOnlineStatus)
            //Check to see if the user no longer has Patient Portal access.  Only the PW salt will remain as the PW.
            command += "AND Password!='None$$'";

        return UserWebCrud.SelectOne(command);
    }

    #endregion

    #region Update

    
    public static void Update(UserWeb userWeb)
    {
        UserWebCrud.Update(userWeb);
    }

    
    public static void Update(UserWeb userWeb, UserWeb userWebOld)
    {
        UserWebCrud.Update(userWeb, userWebOld);
    }

    #endregion

    #region Misc Methods

    /// <summary>
    ///     Creates a username that is not yet in use. Should typically call UserWebs.GetNewPatientPortalCredentials() instead.
    ///     If you are not inserting the name into UserWeb immediately then listExcludedNames should persist across multiple
    ///     calls.
    /// </summary>
    public static string CreateUserNameFromPat(Patient patient, UserWebFKeyType userWebFKeyType, List<string> listExcludedNames)
    {
        var userName = "";
        var i = 0;
        while (true)
        {
            if (i > 1000) throw new ODException(Lans.g("UserWebs", "Unable to create username for patient."));

            userName = patient.FName + ODRandom.Next(100, 100000);
            if (!UserNameExists(userName, userWebFKeyType))
                if (!listExcludedNames.Contains(userName))
                    break; //New userName found 

            i++;
        }

        return userName;
    }

    ///<summary>Generates a random password 8 char long containing at least one uppercase, one lowercase, and one number.</summary>
    public static string PassGen(int length)
    {
        if (length < 0) length = 0;

        //Leave out characters that can cause confusion (o,O,0,l,1,I).
        var lowerCase = "abcdefgijkmnopqrstwxyz";
        var upperCase = "ABCDEFGHJKLMNPQRSTWXYZ";
        var numbers = "23456789";
        var allChars = lowerCase + upperCase + numbers;
        var passChars = "";
        //Grab a letter from each so know we have one of each.
        var stringArrayAllChars = new[] {lowerCase, upperCase, numbers};
        for (var i = 0; i < stringArrayAllChars.Length; i++) passChars += stringArrayAllChars[i][CryptUtil.Random<int>() % stringArrayAllChars[i].Length];

        //Start at 3 because we already added 3 characters
        for (var i = 3; i < length; i++) passChars += allChars[CryptUtil.Random<int>() % allChars.Length];

        //Now that we have our character set, now we do a Fisher-Yates shuffle.
        var charArray = passChars.ToCharArray();
        var arraySize = charArray.Length;
        int intRandom;
        char charTemp;
        for (var i = 0; i < arraySize; i++)
        {
            intRandom = i + CryptUtil.Random<int>() % (arraySize - i);
            charTemp = charArray[intRandom];
            charArray[intRandom] = charArray[i];
            charArray[i] = charTemp;
        }

        //Take a substring in case the requested length is 1 or 2 characters.
        return new string(charArray).Substring(0, length);
    }

    ///<summary>Generates a random password 8 char long containing at least one uppercase, one lowercase, and one number.</summary>
    public static string GenerateRandomPassword(int length)
    {
        //Chracters like o(letter O), 0 (Zero), l (letter l), 1 (one) etc are avoided because they can be ambigious.
        var passwordCharsLCase = "abcdefgijkmnopqrstwxyz";
        var passwordCharsUCase = "ABCDEFGHJKLMNPQRSTWXYZ";
        var passwordCharsNumeric = "23456789";
        //Create a local array containing supported password characters grouped by types.
        var charArrayGroups = new char[3][];
        charArrayGroups[0] = passwordCharsLCase.ToCharArray();
        charArrayGroups[1] = passwordCharsUCase.ToCharArray();
        charArrayGroups[2] = passwordCharsNumeric.ToCharArray();
        //Use this array to track the number of unused characters in each character group.
        var charsLeftInGroup = new int[charArrayGroups.Length];
        //Initially, all characters in each group are not used.
        for (var i = 0; i < charsLeftInGroup.Length; i++) charsLeftInGroup[i] = charArrayGroups[i].Length;

        //Use this array to track (iterate through) unused character groups.
        var leftGroupsOrder = new int[charArrayGroups.Length];
        //Initially, all character groups are not used.
        for (var i = 0; i < leftGroupsOrder.Length; i++) leftGroupsOrder[i] = i;

        //This array will hold password characters.
        var charArrayPassword = new char[length];
        //Index of the next character to be added to password.
        int nextCharIdx;
        //Index of the next character group to be processed.
        int nextGroupIdx;
        //Index which will be used to track not processed character groups.
        int nextLeftGroupsOrderIdx;
        //Index of the last non-processed character in a group.
        int lastCharIdx;
        //Index of the last non-processed group.
        var lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
        //Generate password characters one at a time.
        for (var i = 0; i < charArrayPassword.Length; i++)
        {
            //If only one character group remained unprocessed, process it;
            //otherwise, pick a random character group from the unprocessed
            //group list. To allow a special character to appear in the
            //first position, increment the second parameter of the Next
            //function call by one, i.e. lastLeftGroupsOrderIdx + 1.
            if (lastLeftGroupsOrderIdx == 0)
                nextLeftGroupsOrderIdx = 0;
            else
                nextLeftGroupsOrderIdx = ODRandom.Next(0, lastLeftGroupsOrderIdx);

            //Get the actual index of the character group, from which we will
            //pick the next character.
            nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];
            //Get the index of the last unprocessed characters in this group.
            lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;
            //If only one unprocessed character is left, pick it; otherwise,
            //get a random character from the unused character list.
            if (lastCharIdx == 0)
                nextCharIdx = 0;
            else
                nextCharIdx = ODRandom.Next(0, lastCharIdx + 1);

            //Add this character to the password.
            charArrayPassword[i] = charArrayGroups[nextGroupIdx][nextCharIdx];
            //If we processed the last character in this group, start over.
            if (lastCharIdx == 0)
            {
                charsLeftInGroup[nextGroupIdx] = charArrayGroups[nextGroupIdx].Length;
                //There are more unprocessed characters left.
            }
            else
            {
                //Swap processed character with the last unprocessed character
                //so that we don't pick it until we process all characters in
                //this group.
                if (lastCharIdx != nextCharIdx)
                {
                    var charTemp = charArrayGroups[nextGroupIdx][lastCharIdx];
                    charArrayGroups[nextGroupIdx][lastCharIdx] = charArrayGroups[nextGroupIdx][nextCharIdx];
                    charArrayGroups[nextGroupIdx][nextCharIdx] = charTemp;
                }

                //Decrement the number of unprocessed characters in
                //this group.
                charsLeftInGroup[nextGroupIdx]--;
            }

            //If we processed the last group, start all over.
            if (lastLeftGroupsOrderIdx == 0)
            {
                lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                //There are more unprocessed groups left.
            }
            else
            {
                //Swap processed group with the last unprocessed group
                //so that we don't pick it until we process all groups.
                if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                {
                    var intTemp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                    leftGroupsOrder[lastLeftGroupsOrderIdx] = leftGroupsOrder[nextLeftGroupsOrderIdx];
                    leftGroupsOrder[nextLeftGroupsOrderIdx] = intTemp;
                }

                //Decrement the number of unprocessed groups.
                lastLeftGroupsOrderIdx--;
            }
        }

        //Convert password characters into a string and return the result.
        return new string(charArrayPassword);
    }

    public static string ValidatePatientAccess(Patient patient)
    {
        var stringBuilderErrors = new StringBuilder();
        if (patient.FName.Trim() == "") stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Missing patient first name."));

        if (patient.LName.Trim() == "") stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Missing patient last name."));

        if (patient.Address.Trim() == "") stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Missing patient address line 1."));

        if (patient.City.Trim() == "") stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Missing patient city."));

        if (CultureInfo.CurrentCulture.Name.EndsWith("US") && patient.State.Trim().Length != 2) stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Invalid patient state.  Must be two letters."));

        if (patient.Birthdate.Year < 1880) stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Missing patient birth date."));

        if (patient.HmPhone.Trim() == "" && patient.WirelessPhone.Trim() == "" && patient.WkPhone.Trim() == "") stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Missing patient phone;  Must have home, wireless, or work phone."));

        if (patient.Email.Trim() == "") stringBuilderErrors.AppendLine(Lans.g("PatientPortal", "Missing patient email."));

        return stringBuilderErrors.ToString();
    }

    /// <summary>
    ///     Updates password info in db for given inputs if PlainTextPassword (Item2) is not empty.
    ///     Insert EhrMeasureEvent OnlineAccessProvided if previous db version of UserWeb had no PasswordHash (access has now
    ///     been granted to portal).
    ///     Returns true if UserWeb row was updated. Otherwise returns false.
    /// </summary>
    public static bool UpdateNewPatientPortalCredentials(PatientPortalCredential patientPortalCredential)
    {
        if (patientPortalCredential == null) return false;

        var userWeb = patientPortalCredential.UserWeb;
        var passwordContainer = patientPortalCredential.PasswordContainer;
        if (userWeb != null && !patientPortalCredential.HasAccessedPatientPortal)
        {
            //Only insert an EHR event if the password was previously blank (meaning they don't currently have access).
            if (string.IsNullOrEmpty(userWeb.PasswordHash))
            {
                var ehrMeasureEvent = new EhrMeasureEvent();
                ehrMeasureEvent.DateTEvent = DateTime.Now;
                ehrMeasureEvent.EventType = EhrMeasureEventType.OnlineAccessProvided;
                ehrMeasureEvent.PatNum = userWeb.FKey; //PatNum.
                ehrMeasureEvent.MoreInfo = "";
                EhrMeasureEvents.Insert(ehrMeasureEvent);
            }

            //New password was created so set the flag for the user to change on next login and update the db accordingly.
            userWeb.RequirePasswordChange = true;
            userWeb.LoginDetails = passwordContainer;
            Update(userWeb);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Generates a username and password if necessary for this patient. If the patient is not eligible to be given access,
    ///     this will return null.
    ///     Otherwise returns the UserWeb (Item1), PlainTextPassword (Item2), PasswordContainer (Item3).
    ///     If PlainTextPassword (Item2) is empty then assume new password generation was not necessary.
    ///     Will insert a new UserWeb if none found for this Patient. Will leave UserWeb.PasswordHash blank.
    ///     Call UpdateNewPatientPortalCredentials() using results of this method if you want to save password to db.
    /// </summary>
    /// <param name="passwordOverride">
    ///     If a password has already been generated for this patient, pass it in here so that the password returned
    ///     will match.
    /// </param>
    public static PatientPortalCredential GetNewPatientPortalCredentials(Patient patient, string passwordOverride = "")
    {
        if (string.IsNullOrEmpty(PrefC.GetString(PrefName.PatientPortalURL))) return null; //Haven't set up patient portal yet.

        var errors = ValidatePatientAccess(patient);
        if (!string.IsNullOrEmpty(errors)) return null; //Patient is missing necessary fields.

        var userWeb = GetByFKeyAndType(patient.PatNum, UserWebFKeyType.PatientPortal);
        if (userWeb == null)
        {
            userWeb = new UserWeb();
            userWeb.UserName = CreateUserNameFromPat(patient, UserWebFKeyType.PatientPortal, new List<string>());
            userWeb.FKey = patient.PatNum;
            userWeb.FKeyType = UserWebFKeyType.PatientPortal;
            userWeb.RequireUserNameChange = true;
            userWeb.LoginDetails = new PasswordContainer(HashTypes.None, "", "");
            userWeb.IsNew = true;
            //Always insert here. We may not ever end up updating UserWeb.PasswordHash if an email is not sent to this patient.
            //This will leave a UserWeb row with a UserName (for next time) but no password. This row will be updated with a password at the appropriate time.
            Insert(userWeb);
        }

        var isNewPasswordRequired = false;
        if (string.IsNullOrEmpty(userWeb.UserName))
        {
            //Fixing B11013 so new UserName and Password should be generated.
            userWeb.UserName = CreateUserNameFromPat(patient, UserWebFKeyType.PatientPortal, new List<string>());
            userWeb.RequireUserNameChange = true;
            //UserName fields have been changed so update db.
            Update(userWeb);
            isNewPasswordRequired = true;
        }

        if (userWeb.RequirePasswordChange)
            //Could be a new UserWeb or a subsequent invite being generated for the same patient (had a second appointment).
            isNewPasswordRequired = true;

        if (string.IsNullOrEmpty(userWeb.PasswordHash))
            //Patient has no password so portal access has not been previously granted.
            isNewPasswordRequired = true;

        var passwordPlainText = "";
        var passwordContainer = userWeb.LoginDetails;
        if (isNewPasswordRequired)
        {
            //PP invites will often times call this method and get this far but not actually want to save the new creds to the db.
            //For that reason we won't actually update the db with the new password here. 
            //The caller of this method will need to call ProcessNewPatientPortalCredentialsOut() if they really want this new password to persist to the db.
            passwordPlainText = passwordOverride;
            if (passwordOverride == "") passwordPlainText = GenerateRandomPassword(8);

            passwordContainer = Authentication.GenerateLoginDetails(passwordPlainText, HashTypes.SHA3_512);
        }

        return new PatientPortalCredential(userWeb, passwordPlainText, passwordContainer);
    }

    public static bool UserNameExists(string userName, UserWebFKeyType userWebFKeyType)
    {
        if (GetUserNameCount(userName, userWebFKeyType) != 0) return true;

        return false;
    }

    public static int GetUserNameCount(string userName, UserWebFKeyType userWebFKeyType)
    {
        var command = "SELECT COUNT(*) FROM userweb WHERE UserName='" + SOut.String(userName) + "' AND FKeyType=" + SOut.Int((int) userWebFKeyType);
        return SIn.Int(Db.GetCount(command));
    }

    #endregion
}