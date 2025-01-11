using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Userods
{
    public static List<Userod> GetAll()
    {
        return UserodCrud.TableToList(DataCore.GetTable("SELECT * FROM userod ORDER BY UserName"));
    }
    
    public static Userod GetUser(long userNum)
    {
        return GetFirstOrDefault(x => x.UserNum == userNum);
    }
    
    public static List<Userod> GetUsers(List<long> listUserNums)
    {
        return GetWhere(x => listUserNums.Contains(x.UserNum));
    }

    public static List<Userod> GetUsers()
    {
        var listUserodsNonHidden = new List<Userod>();
        var listUserodsLong = GetDeepCopy();
        for (var i = 0; i < listUserodsLong.Count; i++)
        {
            if (listUserodsLong[i].IsHidden)
            {
                continue;
            }

            if (listUserodsLong[i].UserNumCEMT != 0)
            {
                continue;
            }

            listUserodsNonHidden.Add(listUserodsLong[i]);
        }

        return listUserodsNonHidden;
    }

    ///<summary>Returns a list of all CEMT users.</summary>
    public static List<Userod> GetUsersForCEMT()
    {
        return GetWhere(x => x.UserNumCEMT != 0);
    }

    ///<summary>Returns null if not found.  Is not case sensitive.  isEcwTight isn't even used.</summary>
    public static Userod GetUserByName(string userName)
    {
        return GetFirstOrDefault(x => !x.IsHidden && x.UserName.ToLower() == userName.ToLower());
    }

    /// <summary>
    ///     Gets the first user with the matching userName passed in.  Not case sensitive.  Returns null if not found.
    ///     Does not use the cache to find a corresponding user with the passed in userName.  Every middle tier call passes
    ///     through here.
    /// </summary>
    public static Userod GetUserByNameNoCache(string userName)
    {
        var command = "SELECT * FROM userod WHERE UserName='" + SOut.String(userName) + "'";
        var listUserods = UserodCrud.TableToList(DataCore.GetTable(command));
        return listUserods.FirstOrDefault(x => !x.IsHidden && x.UserName.ToLower() == userName.ToLower());
    }

    ///<summary>Gets the user by usernum. Skips the cache.</summary>
    public static Userod GetUserByUserNumNoCache(long userNum)
    {
        return UserodCrud.SelectOne(userNum);
    }

    /// <summary>
    ///     Gets the first user with the matching badgeId passed in. Expecting int with 4 digits or less.  Returns null if
    ///     not found.
    /// </summary>
    public static Userod GetUserByBadgeId(string badgeId)
    {
        var command = "SELECT * FROM userod WHERE BadgeId <> '' AND BadgeId = RIGHT('" + SOut.String(badgeId) + "', LENGTH(BadgeId))";
        //Example BadgeId in db="123". Select compares "123" with RIGHT('00000123',3)
        var listUserods = UserodCrud.TableToList(DataCore.GetTable(command));
        return listUserods.FirstOrDefault();
    }

    ///<summary>Returns all users that are associated to the employee passed in.  Returns empty list if no matches found.</summary>
    public static List<Userod> GetUsersByEmployeeNum(long employeeNum)
    {
        return GetWhere(x => x.EmployeeNum == employeeNum);
    }

    ///<summary>Returns all users that are associated to the permission passed in. Returns empty list if no matches found.</summary>
    public static List<Userod> GetUsersByPermission(EnumPermType permissions, bool showHidden)
    {
        var listUserGroups = UserGroups.GetForPermission(permissions);
        var listUserNums = UserGroupAttaches.GetUserNumsForUserGroups(listUserGroups);
        return GetWhere(x => listUserNums.Contains(x.UserNum), !showHidden);
    }

    ///<summary>Gets all non-hidden users that have an associated provider.</summary>
    public static List<Userod> GetUsersWithProviders()
    {
        return GetWhere(x => x.ProvNum != 0, true);
    }

    ///<summary>Returns all users associated to the provider passed in.  Returns empty list if no matches found.</summary>
    public static List<Userod> GetUsersByProvNum(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum, true);
    }

    public static List<Userod> GetUsersByInbox(long taskListNum)
    {
        return GetWhere(x => x.TaskListInBox == taskListNum, true);
    }

    /// <summary>
    ///     Returns all users selectable for the insurance verification list.
    ///     Pass in an empty list to not filter by clinic.
    ///     Set isAssigning to false to return only users who have an insurance already assigned.
    /// </summary>
    public static List<Userod> GetUsersForVerifyList(List<long> listClinicNums, bool isAssigning, bool includeHiddenUsers = false)
    {
        var listUserNumsInInsVerify = InsVerifies.GetAllInsVerifyUserNums();
        var listUserNumsInClinic = new List<long>();
        if (listClinicNums.Count > 0)
        {
            var listUserClinics = new List<UserClinic>();
            for (var i = 0; i < listClinicNums.Count; i++) listUserNumsInClinic.AddRange(UserClinics.GetForClinic(listClinicNums[i]).Select(y => y.UserNum).Distinct().ToList());

            listUserNumsInClinic.AddRange(GetUsers().FindAll(x => !x.ClinicIsRestricted).Select(x => x.UserNum).Distinct().ToList()); //Always add unrestricted users into the list.
            listUserNumsInClinic = listUserNumsInClinic.Distinct().ToList(); //Remove duplicates that could possibly be in the list.
            if (listUserNumsInClinic.Count > 0) listUserNumsInInsVerify = listUserNumsInInsVerify.FindAll(x => listUserNumsInClinic.Contains(x));

            listUserNumsInInsVerify.AddRange(GetUsers(listUserNumsInInsVerify).FindAll(x => !x.ClinicIsRestricted).Select(x => x.UserNum).Distinct().ToList()); //Always add unrestricted users into the list.
            listUserNumsInInsVerify = listUserNumsInInsVerify.Distinct().ToList();
        }

        var listUserodsWithPerm = GetUsersByPermission(EnumPermType.InsPlanVerifyList, includeHiddenUsers);
        if (isAssigning)
        {
            if (listClinicNums.Count == 0) return listUserodsWithPerm; //Return unfiltered list of users with permission

            //Don't limit user list to already assigned insurance verifications.
            return listUserodsWithPerm.FindAll(x => listUserNumsInClinic.Contains(x.UserNum)); //Return users with permission, limited by their clinics
        }

        return listUserodsWithPerm.FindAll(x => listUserNumsInInsVerify.Contains(x.UserNum)); //Return users limited by permission, clinic, and having an insurance already assigned.
    }

    /// <summary>
    ///     Returns all non-hidden users associated with the domain user name passed in. Returns an empty list if no
    ///     matches found.
    /// </summary>
    public static List<Userod> GetUsersByDomainUserName(string domainUser)
    {
        return GetWhere(x => x.DomainUser.Equals(domainUser, StringComparison.InvariantCultureIgnoreCase), true);
    }

    ///<summary>This handles situations where we have a usernum, but not a user.  And it handles usernum of zero.</summary>
    public static string GetName(long userNum)
    {
        var userod = GetFirstOrDefault(x => x.UserNum == userNum);
        if (userod == null) return "";

        return userod.UserName;
    }

    ///<summary>Returns true if the user passed in is associated with a provider that has (or had) an EHR prov key.</summary>
    public static bool IsUserCpoe(Userod userod)
    {
        if (userod == null) return false;

        var provider = Providers.GetProv(userod.ProvNum);
        if (provider == null) return false;

        //Check to see if this provider has had a valid key at any point in history.
        return EhrProvKeys.HasProvHadKey(provider.LName, provider.FName);
    }

    /// <summary>
    ///     Searches the database for a corresponding user by username (not case sensitive).  Returns null is no match found.
    ///     Once a user has been found, if the number of failed log in attempts exceeds the limit an exception is thrown with a
    ///     message to display to the
    ///     user.  Then the hash of the plaintext password (if usingEcw is true, password needs to be hashed before passing
    ///     into this method) is checked
    ///     against the password hash that is currently in the database.  Once the plaintext password passed in is validated,
    ///     this method will upgrade the
    ///     hashing algorithm for the password (if necessary) and then returns the entire user object for the corresponding
    ///     user found.  Throws exceptions
    ///     with error message to display to the user if anything goes wrong.  Manipulates the appropriate log in failure
    ///     columns in the db as
    ///     needed.
    /// </summary>
    public static Userod CheckUserAndPassword(string userName, string plaintext, bool isEcw)
    {
        return CheckUserAndPassword(userName, plaintext, isEcw, true);
    }

    /// <summary>
    ///     Searches the database for a corresponding user by username (not case sensitive).  Returns null is no match found.
    ///     Once a user has been found, if the number of failed log in attempts exceeds the limit an exception is thrown with a
    ///     message to display to the
    ///     user.  Then the hash of the plaintext password (if usingEcw is true, password needs to be hashed before passing
    ///     into this method) is checked
    ///     against the password hash that is currently in the database.  Once the plaintext password passed in is validated,
    ///     this method will upgrade the
    ///     hashing algorithm for the password (if necessary) and then returns the entire user object for the corresponding
    ///     user found.  Throws exceptions
    ///     with error message to display to the user if anything goes wrong.  Manipulates the appropriate log in failure
    ///     columns in the db as
    ///     needed.  Null will be returned when hasExceptions is false and no matching user found, credentials are invalid, or
    ///     account is locked.
    /// </summary>
    public static Userod CheckUserAndPassword(string userName, string plaintext, bool isEcw, bool hasExceptions)
    {
        //Do not use the cache here because an administrator could have cleared the log in failure attempt columns for this user.
        //Also, middle tier calls this method every single time a process request comes to it.
        var userodDb = GetUserByNameNoCache(userName);
        if (userodDb == null)
        {
            if (hasExceptions) throw new ODException(Lans.g("Userods", "Invalid username or password."), ODException.ErrorCodes.CheckUserAndPasswordFailed);

            return null;
        }

        var dateTimeNowDb = MiscData.GetNowDateTime();
        //We found a user via matching just the username passed in.  Now we need to check to see if they have exceeded the log in failure attempts.
        //For now we are hardcoding a 5 minute delay when the user has failed to log in 5 times in a row.  
        //An admin user can reset the password or the failure attempt count for the user failing to log in via the Security window.
        if (userodDb.DateTFail.Year > 1880 //The user has failed to log in recently
            && dateTimeNowDb.Subtract(userodDb.DateTFail) < TimeSpan.FromMinutes(5) //The last failure has been within the last 5 minutes.
            && userodDb.FailedAttempts >= 5) //The user failed 5 or more times.
        {
            if (hasExceptions)
                throw new ApplicationException(Lans.g("Userods", "Account has been locked due to failed log in attempts."
                                                                 + "\r\nCall your security admin to unlock your account or wait at least 5 minutes."));

            return null;
        }

        var isPasswordValid = Authentication.CheckPassword(userodDb, plaintext, isEcw);
        var userodNew = userodDb.Copy();
        //If the last failed log in attempt was more than 5 minutes ago, reset the columns in the database so the user can try 5 more times.
        if (userodDb.DateTFail.Year > 1880 && dateTimeNowDb.Subtract(userodDb.DateTFail) > TimeSpan.FromMinutes(5))
        {
            userodNew.FailedAttempts = 0;
            userodNew.DateTFail = DateTime.MinValue;
        }

        if (!isPasswordValid)
        {
            userodNew.DateTFail = dateTimeNowDb;
            userodNew.FailedAttempts += 1;
        }

        //Synchronize the database with the results of the log in attempt above
        UserodCrud.Update(userodNew, userodDb);
        if (isPasswordValid)
        {
            //Upgrade the encryption for the password if this is not an eCW user (eCW uses md5) and the password is using an outdated hashing algorithm.
            if (!isEcw && !string.IsNullOrEmpty(plaintext) && userodNew.GetPasswordContainer().HashType != HashTypes.SHA3_512)
            {
                //Update the password to the default hash type which should be the most secure hashing algorithm possible.
                Authentication.UpdatePasswordUserod(userodNew, plaintext);
                //The above method is almost guaranteed to have changed the password for userNew so go back out the db and get the changes that were made.
                userodNew = GetUserNoCache(userodNew.UserNum);
            }

            return userodNew;
        }

        //Password was not valid.
        if (hasExceptions) throw new ODException(Lans.g("Userods", "Invalid username or password."), ODException.ErrorCodes.CheckUserAndPasswordFailed);

        return null;
    }

    /// <summary>
    ///     Updates all students/instructors to the specified user group.  Surround with try/catch because it can throw
    ///     exceptions.
    /// </summary>
    public static void UpdateUserGroupsForDentalSchools(UserGroup userGroup, bool isInstructor)
    {
        string command;
        //Check if the user group that the students or instructors are trying to go to has the SecurityAdmin permission.
        if (!GroupPermissions.HasPermission(userGroup.UserGroupNum, EnumPermType.SecurityAdmin, 0))
        {
            //We need to make sure that moving these users to the new user group does not eliminate all SecurityAdmin users in db.
            command = "SELECT COUNT(*) FROM usergroupattach "
                      + "INNER JOIN usergroup ON usergroupattach.UserGroupNum=usergroup.UserGroupNum "
                      + "INNER JOIN grouppermission ON grouppermission.UserGroupNum=usergroup.UserGroupNum "
                      + "WHERE usergroupattach.UserNum NOT IN "
                      + "(SELECT userod.UserNum FROM userod,provider "
                      + "WHERE userod.ProvNum=provider.ProvNum ";
            if (isInstructor)
            {
                command += "AND provider.IsInstructor=" + SOut.Bool(isInstructor) + ") ";
            }
            else
            {
                command += "AND provider.IsInstructor=" + SOut.Bool(isInstructor) + " ";
                command += "AND provider.SchoolClassNum!=0) ";
            }

            command += "AND grouppermission.PermType=" + SOut.Int((int) EnumPermType.SecurityAdmin) + " ";
            var lastAdmin = SIn.Int(Db.GetCount(command));
            if (lastAdmin == 0) throw new Exception("Cannot move students or instructors to the new user group because it would leave no users with the SecurityAdmin permission.");
        }

        command = "UPDATE userod INNER JOIN provider ON userod.ProvNum=provider.ProvNum "
                  + "SET UserGroupNum=" + SOut.Long(userGroup.UserGroupNum) + " "
                  + "WHERE provider.IsInstructor=" + SOut.Bool(isInstructor);
        if (!isInstructor) command += " AND provider.SchoolClassNum!=0";

        Db.NonQ(command);
    }

    ///<summary>Surround with try/catch because it can throw exceptions.</summary>
    public static void Update(Userod userod, List<long> listUserGroupNums = null)
    {
        Validate(false, userod, false, listUserGroupNums);
        UserodCrud.Update(userod);
        if (listUserGroupNums == null) return;

        UserGroupAttaches.SyncForUser(userod, listUserGroupNums);
    }

    /// <summary>
    ///     Surround with try/catch because it can throw exceptions.
    ///     Same as Update(), only the Validate call skips checking duplicate names for hidden users.
    /// </summary>
    public static void UpdatePassword(Userod userod, PasswordContainer passwordContainer, bool isPasswordStrong, bool includeCEMT = false)
    {
        var userodToUpdate = userod.Copy();
        userodToUpdate.SetPassword(passwordContainer);
        userodToUpdate.PasswordIsStrong = isPasswordStrong;
        var listUserGroups = userodToUpdate.GetGroups(includeCEMT);
        if (listUserGroups.Count < 1) throw new Exception(Lans.g("Userods", "The current user must be in at least one user group."));

        Validate(false, userodToUpdate, true, listUserGroups.Select(x => x.UserGroupNum).ToList());
        UserodCrud.Update(userodToUpdate);
    }

    ///<summary>Sets the TaskListInBox to 0 for any users that have this as their inbox.</summary>
    public static void DisassociateTaskListInBox(long taskListNum)
    {
        var command = "UPDATE userod SET TaskListInBox=0 WHERE TaskListInBox=" + SOut.Long(taskListNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     A user must always have at least one associated userGroupAttach. Pass in the usergroup(s) that should be attached.
    ///     Surround with try/catch because it can throw exceptions.
    /// </summary>
    public static long Insert(Userod userod, List<long> listUserGroupNums, bool isForCEMT = false)
    {
        if (userod.IsHidden && UserGroups.IsAdminGroup(listUserGroupNums)) throw new Exception(Lans.g("Userods", "Admins cannot be hidden."));

        Validate(true, userod, false, listUserGroupNums);
        var userNum = UserodCrud.Insert(userod);
        UserGroupAttaches.SyncForUser(userod, listUserGroupNums);
        if (isForCEMT)
        {
            userod.UserNumCEMT = userNum;
            UserodCrud.Update(userod);
        }

        return userNum;
    }

    /// <summary>
    ///     Surround with try/catch because it can throw exceptions.
    ///     We don't really need to make this public, but it's required in order to follow the RemotingRole pattern.
    ///     listUserGroupNum can only be null when validating for an Update.
    /// </summary>
    public static void Validate(bool isNew, Userod userod, bool excludeHiddenUsers, List<long> listUserGroupNums)
    {
        //should add a check that employeenum and provnum are not both set.
        //make sure username is not already taken
        string command;
        long excludeUserNum;
        if (isNew)
            excludeUserNum = 0;
        else
            excludeUserNum = userod.UserNum; //it's ok if the name matches the current username

        //It doesn't matter if the UserName is already in use if the user being updated is going to be hidden.  This check will block them from unhiding duplicate users.
        if (!userod.IsHidden)
        {
            //if the user is now not hidden
            //CEMT users will not be visible from within Open Dental.  Therefore, make a different check so that we can know if the name
            //the user typed in is a duplicate of a CEMT user.  In doing this, we are able to give a better message.
            if (!IsUserNameUnique(userod.UserName, excludeUserNum, excludeHiddenUsers, true)) throw new ApplicationException(Lans.g("Userods", "UserName already in use by CEMT member."));

            if (!IsUserNameUnique(userod.UserName, excludeUserNum, excludeHiddenUsers))
                //IsUserNameUnique doesn't care if it's a CEMT user or not.. It just gets a count based on username.
                throw new ApplicationException(Lans.g("Userods", "UserName already in use."));
        }

        if (listUserGroupNums == null)
            //Not validating UserGroup selections.
            return;

        if (listUserGroupNums.Count < 1) throw new ApplicationException(Lans.g("Userods", "The current user must be in at least one user group."));

        //an admin user can never be hidden
        command = "SELECT COUNT(*) FROM grouppermission "
                  + "WHERE PermType='" + SOut.Long((int) EnumPermType.SecurityAdmin) + "' "
                  + "AND UserGroupNum IN (" + string.Join(",", listUserGroupNums) + ") ";
        if (!isNew //Updating.
            && Db.GetCount(command) == "0" //if this user would not have admin
            && !IsSomeoneElseSecurityAdmin(userod)) //make sure someone else has admin
            throw new ApplicationException(Lans.g("Users", "At least one user must have Security Admin permission."));

        if (userod.IsHidden //hidden 
            && userod.UserNumCEMT == 0 //and non-CEMT
            && Db.GetCount(command) != "0") //if this user is admin
            throw new ApplicationException(Lans.g("Userods", "Admins cannot be hidden."));
    }

    /// <summary>Returns true if there is at least one user part of the SecurityAdmin permission excluding the user passed in.</summary>
    public static bool IsSomeoneElseSecurityAdmin(Userod userod)
    {
        var command = "SELECT COUNT(*) FROM userod "
                      + "INNER JOIN usergroupattach ON usergroupattach.UserNum=userod.UserNum "
                      + "INNER JOIN grouppermission ON usergroupattach.UserGroupNum=grouppermission.UserGroupNum "
                      + "WHERE grouppermission.PermType='" + SOut.Long((int) EnumPermType.SecurityAdmin) + "'"
                      + " AND userod.IsHidden =0"
                      + " AND userod.UserNum != " + SOut.Long(userod.UserNum);
        if (Db.GetCount(command) == "0")
            //there are no other users with this permission
            return false;

        return true;
    }

    public static bool IsUserNameUnique(string userName, long excludeUserNum, bool excludeHiddenUsers)
    {
        return IsUserNameUnique(userName, excludeUserNum, excludeHiddenUsers, false);
    }

    ///<summary>Supply 0 or -1 for the excludeUserNum to not exclude any.</summary>
    public static bool IsUserNameUnique(string userName, long excludeUserNum, bool excludeHiddenUsers, bool searchCEMTUsers)
    {
        if (userName == "") return false;

        var command = "SELECT COUNT(*) FROM userod WHERE ";
        //if(Programs.UsingEcwTight()){
        //	command+="BINARY ";//allows different usernames based on capitalization.//we no longer allow this
        //Does not need to be tested under Oracle because eCW users do not use Oracle.
        //}
        command += "UserName='" + SOut.String(userName) + "' "
                   + "AND UserNum !=" + SOut.Long(excludeUserNum) + " ";
        if (excludeHiddenUsers) command += "AND IsHidden=0 "; //not hidden

        if (searchCEMTUsers) command += "AND UserNumCEMT!=0";

        var table = DataCore.GetTable(command);
        if (table.Rows[0][0].ToString() == "0") return true;

        return false;
    }

    /// <summary>
    ///     Generates a unique username based on what is passed into it.
    ///     Returns null if given userName can not be easily identified as unique.
    /// </summary>
    /// <param name="userName">The username you are copying</param>
    /// <param name="excludeUserNum">The UserNum that is excluded when checking if a username is in use.</param>
    /// <param name="excludeHiddenUsers">
    ///     Set to true to exclude hidden patients when checking if a username is in use,
    ///     otherwise false
    /// </param>
    /// <param name="searchCEMTUsers">Set to true to include checking usernames that are associated to CEMT users.</param>
    /// <param name="uniqueUserName">
    ///     When returning true this is set to a unique username, otherwise null.</parm>
    ///     <returns></returns>
    public static bool TryGetUniqueUsername(string userName, long excludeUserNum, bool excludeHiddenUsers, bool searchCEMTUsers, out string uniqueUserName)
    {
        var attempt = 1;
        uniqueUserName = userName; //Default to given username, will change if not unique.
        while (!IsUserNameUnique(uniqueUserName, excludeUserNum, excludeHiddenUsers, searchCEMTUsers))
        {
            if (attempt > 100)
            {
                uniqueUserName = null;
                return false;
            }

            uniqueUserName = userName + $"({++attempt})";
        }

        return true;
    }

    /// <summary>
    ///     Inserts a new user into table and returns that new user. Not all fields are copied from original user.
    /// </summary>
    /// <param name="userod">The user that we will be copying from, not all fields are copied.</param>
    /// <param name="passwordContainer"></param>
    /// <param name="isPasswordStrong"></param>
    /// <param name="userName"></param>
    /// <param name="isForCemt">When true newly inserted user.UserNumCEMT will be set to the user.UserNum</param>
    /// <returns></returns>
    public static Userod CopyUser(Userod userod, PasswordContainer passwordContainer, bool isPasswordStrong, string userName = null, bool isForCemt = false)
    {
        if (!TryGetUniqueUsername(userName ?? userod.UserName + "(Copy)", 0, false, isForCemt, out var uniqueUserName)) return null;

        var userodCopy = new Userod();
        //if function is ever called outside of the security form this ensures that we will know if a user is a copy of another user
        userodCopy.UserName = uniqueUserName;
        userodCopy.SetPassword(passwordContainer);
        userodCopy.PasswordIsStrong = isPasswordStrong;
        userodCopy.ClinicIsRestricted = userod.ClinicIsRestricted;
        userodCopy.ClinicNum = userod.ClinicNum;
        //Insert also validates the user.
        userodCopy.UserNum = Insert(userodCopy, UserGroups.GetForUser(userod.UserNum, isForCemt).Select(x => x.UserGroupNum).ToList(), isForCemt);

        #region UserClinics

        var listUserClinics = new List<UserClinic>(UserClinics.GetForUser(userod.UserNum));
        listUserClinics.ForEach(x => x.UserNum = userodCopy.UserNum);
        UserClinics.Sync(listUserClinics, userodCopy.UserNum);

        #endregion

        #region Alerts

        var listAlertSubsUsers = AlertSubs.GetAllForUser(userod.UserNum);
        listAlertSubsUsers.ForEach(x => x.UserNum = userodCopy.UserNum);
        AlertSubs.Sync(listAlertSubsUsers, new List<AlertSub>());

        #endregion

        return userodCopy;
    }

    public static List<Userod> GetForGroup(long userGroupNum)
    {
        return GetWhere(x => x.IsInUserGroup(userGroupNum));
    }

    ///<summary>Gets a list of users for which the passed-in clinicNum is the only one they have access to.</summary>
    public static List<Userod> GetUsersOnlyThisClinic(long clinicNum)
    {
        var command = "SELECT userod.* "
                      + "FROM( "
                      + "SELECT userclinic.UserNum,COUNT(userclinic.ClinicNum) Clinics FROM userclinic "
                      + "GROUP BY userNum "
                      + "HAVING Clinics = 1 "
                      + ") users "
                      + "INNER JOIN userclinic ON userclinic.UserNum = users.UserNum "
                      + "AND userclinic.ClinicNum = " + SOut.Long(clinicNum) + " "
                      + "INNER JOIN userod ON userod.UserNum = userclinic.UserNum ";
        return UserodCrud.SelectMany(command);
    }

    /// <summary>Will return 0 if no inbox found for user.</summary>
    public static long GetInbox(long userNum)
    {
        var userod = GetFirstOrDefault(x => x.UserNum == userNum);
        if (userod == null) return 0;

        return userod.TaskListInBox;
    }

    /// <summary>
    ///     Returns empty string if password is strong enough.  Otherwise, returns explanation of why it's not strong
    ///     enough.
    /// </summary>
    public static string IsPasswordStrong(string password, bool requireStrong = false)
    {
        var strongPasswordMsg = " when the strong password feature is turned on";
        if (requireStrong)
            //Just used by the API, which always requires strong pw
            strongPasswordMsg = "";

        if (password == "") return Lans.g("FormUserPassword", "Password may not be blank" + strongPasswordMsg + ".");

        if (password.Length < 8) return Lans.g("FormUserPassword", "Password must be at least eight characters long" + strongPasswordMsg + ".");

        var containsCap = false;
        for (var i = 0; i < password.Length; i++)
            if (char.IsUpper(password[i]))
                containsCap = true;

        if (!containsCap) return Lans.g("FormUserPassword", "Password must contain at least one capital letter" + strongPasswordMsg + ".");

        var containsLower = false;
        for (var i = 0; i < password.Length; i++)
            if (char.IsLower(password[i]))
                containsLower = true;

        if (!containsLower) return Lans.g("FormUserPassword", "Password must contain at least one lower case letter" + strongPasswordMsg + ".");

        if (PrefC.GetBool(PrefName.PasswordsStrongIncludeSpecial))
        {
            var hasSpecial = false;
            for (var i = 0; i < password.Length; i++)
                if (!char.IsLetterOrDigit(password[i]))
                {
                    hasSpecial = true;
                    break;
                }

            if (!hasSpecial) return Lans.g("FormUserPassword", "Password must contain at least one special character when the 'strong passwords require a special character' feature is turned on.");
        }

        var containsNum = false;
        for (var i = 0; i < password.Length; i++)
            if (char.IsNumber(password[i]))
                containsNum = true;

        if (!containsNum) return Lans.g("FormUserPassword", "Password must contain at least one number" + strongPasswordMsg + ".");

        return "";
    }

    /// <summary>
    ///     This resets the strong password flag on all users after an admin turns off pref PasswordsMustBeStrong.  If
    ///     strong passwords are again turned on later, then each user will have to edit their password in order set the strong
    ///     password flag again.
    /// </summary>
    public static void ResetStrongPasswordFlags()
    {
        var command = "UPDATE userod SET PasswordIsStrong=0";
        Db.NonQ(command);
    }

    ///<summary>Returns true if the passed-in user is apart of the passed-in usergroup.</summary>
    public static bool IsInUserGroup(long userNum, long userGroupNum)
    {
        var listUserGroupAttaches = UserGroupAttaches.GetForUser(userNum);
        return listUserGroupAttaches.Select(x => x.UserGroupNum).Contains(userGroupNum);
    }

    #region Get Methods

    /// <summary>
    ///     Returns the UserNum of the first non-hidden admin user if they have no password set.
    ///     It is very important to order by UserName in order to preserve old behavior of only considering the first Admin
    ///     user we come across.
    ///     This method does not simply return the first admin user with no password.  It is explicit in only considering the
    ///     FIRST admin user.
    ///     Returns 0 if there are no admin users or the first admin user found has a password set.
    /// </summary>
    public static long GetFirstSecurityAdminUserNumNoPasswordNoCache()
    {
        //The query will order by UserName in order to preserve old behavior (mimics the cache).
        var command = @"SELECT userod.UserNum,CASE WHEN COALESCE(userod.Password,'')='' THEN 0 ELSE 1 END HasPassword 
				FROM userod
				INNER JOIN usergroupattach ON userod.UserNum=usergroupattach.UserNum
				INNER JOIN grouppermission ON usergroupattach.UserGroupNum=grouppermission.UserGroupNum 
				WHERE userod.IsHidden=0
				AND grouppermission.PermType=" + SOut.Int((int) EnumPermType.SecurityAdmin) + @"
				GROUP BY userod.UserNum
				ORDER BY userod.UserName
				LIMIT 1";
        var table = DataCore.GetTable(command);
        long userNumAdminNoPass = 0;
        if (table != null && table.Rows.Count > 0 && table.Rows[0]["HasPassword"].ToString() == "0")
            //The first admin user in the database does NOT have a password set.  Return their UserNum.
            userNumAdminNoPass = SIn.Long(table.Rows[0]["UserNum"].ToString());

        return userNumAdminNoPass;
    }

    ///<summary>Gets the corresponding user for the userNum passed in without using the cache.</summary>
    public static Userod GetUserNoCache(long userNum)
    {
        var command = "SELECT * FROM userod WHERE userod.UserNum=" + SOut.Long(userNum);
        return UserodCrud.SelectOne(command);
    }

    ///<summary>Gets the user name for the userNum passed in.  Returns empty string if not found in the database.</summary>
    public static string GetUserNameNoCache(long userNum)
    {
        var command = "SELECT userod.UserName FROM userod WHERE userod.UserNum=" + SOut.Long(userNum);
        return DataCore.GetScalar(command);
    }

    /// <summary>
    ///     Returns a list of non-hidden, non-CEMT user names.  Set hasOnlyCEMT to true if you only want non-hidden CEMT users.
    ///     Always returns all non-hidden users if PrefName.UserNameManualEntry is true.
    /// </summary>
    public static List<string> GetUserNamesNoCache()
    {
        var command = $@"SELECT userod.UserName FROM userod 
				WHERE userod.IsHidden=0 
				{(PrefC.GetBool(PrefName.UserNameManualEntry) ? " " : " AND userod.UserNumCEMT=0 ")}
				ORDER BY userod.UserName";
        return Db.GetListString(command);
    }

    /// <summary>
    ///     Returns all non-hidden UserNums (key) and UserNames (value) associated with the domain user name passed in.
    ///     Returns an empty dictionary if no matches were found.
    /// </summary>
    public static Dictionary<long, string> GetUsersByDomainUserNameNoCache(string domainUser)
    {
        var command = @"SELECT userod.UserNum, userod.UserName, userod.DomainUser 
				FROM userod 
				WHERE IsHidden=0";
        //Not sure how to do an InvariantCultureIgnoreCase via a query so doing it over in C# in order to preserve old behavior.
        var dictNonHiddenUsers = DataCore.GetTable(command).Select()
            .Where(x => SIn.String(x["DomainUser"].ToString()).Equals(domainUser, StringComparison.InvariantCultureIgnoreCase))
            .ToDictionary(x => SIn.Long(x["UserNum"].ToString()), x => SIn.String(x["UserName"].ToString()));
        return dictNonHiddenUsers;
    }

    #endregion

    #region Misc Methods

    ///<summary>Returns true if at least one admin user is present within the database.  Otherwise; false.</summary>
    public static bool HasSecurityAdminUserNoCache()
    {
        var command = @"SELECT COUNT(*) FROM userod
				INNER JOIN usergroupattach ON userod.UserNum=usergroupattach.UserNum
				INNER JOIN grouppermission ON usergroupattach.UserGroupNum=grouppermission.UserGroupNum 
				WHERE userod.IsHidden=0
				AND grouppermission.PermType=" + SOut.Int((int) EnumPermType.SecurityAdmin);
        return Db.GetCount(command) != "0";
    }
    
    ///<summary>Returns true if the user can sign notes. Uses the NotesProviderSignatureOnly preference to validate.</summary>
    public static bool CanUserSignNote(Userod userod = null)
    {
        var userodSig = userod;
        if (userod == null) userodSig = Security.CurUser;

        if (PrefC.GetBool(PrefName.NotesProviderSignatureOnly) && userodSig.ProvNum == 0) return false; //Prefernce is on and our user is not a provider.

        return true; //Either pref is off or it is on and user is a provider.
    }

    #endregion

    #region CachePattern

    private class UserodCache : CacheListAbs<Userod>
    {
        protected override List<Userod> GetCacheFromDb()
        {
            var command = "SELECT * FROM userod ORDER BY UserName";
            return UserodCrud.SelectMany(command);
        }

        protected override List<Userod> TableToList(DataTable dataTable)
        {
            return UserodCrud.TableToList(dataTable);
        }

        protected override Userod Copy(Userod item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Userod> items)
        {
            return UserodCrud.ListToTable(items, "Userod");
        }

        protected override void FillCacheIfNeeded()
        {
            Userods.GetTableFromCache(false);
        }

        protected override bool IsInListShort(Userod item)
        {
            return !item.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly UserodCache _userodCache = new();

    public static Userod GetFirstOrDefault(Func<Userod, bool> match, bool isShort = false)
    {
        return _userodCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Gets a deep copy of all matching items from the cache via ListLong.  Set isShort true to search through
    ///     ListShort instead.
    /// </summary>
    public static List<Userod> GetWhere(Predicate<Userod> match, bool isShort = false)
    {
        return _userodCache.GetWhere(match, isShort);
    }

    public static List<Userod> GetDeepCopy(bool isShort = false)
    {
        return _userodCache.GetDeepCopy(isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _userodCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool refreshCache)
    {
        var table = _userodCache.GetTableFromCache(refreshCache);
        Security.SyncCurUser(); //Cache can have a stale reference to the Security.CurUser to ensure it has a current one.
        return table;
    }

    public static void ClearCache()
    {
        _userodCache.ClearCache();
    }

    ///<summary>Returns the boolean indicating if the user cache has been turned off or not.</summary>
    public static bool GetIsCacheAllowed()
    {
        return _userodCache.IsCacheAllowed;
    }

    /// <summary>
    ///     Set isCacheAllowed false to immediately clear out the userod cache and then set the cache into a state where it
    ///     will throw an
    ///     exception if any method attempts to have the cache fill itself.  This is designed to keep sensitive data from being
    ///     cached until a
    ///     verified user has logged in to the program.  Once a user has logged in then it is acceptable to fill the userod
    ///     cache.
    /// </summary>
    public static void SetIsCacheAllowed(bool isCacheAllowed)
    {
        _userodCache.IsCacheAllowed = isCacheAllowed;
    }

    #endregion
}