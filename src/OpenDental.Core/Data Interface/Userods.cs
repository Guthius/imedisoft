using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

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

    public static Userod GetUserByNameNoCache(string userName)
    {
        var command = "SELECT * FROM userod WHERE UserName='" + SOut.String(userName) + "'";
        var listUserods = UserodCrud.TableToList(DataCore.GetTable(command));
        return listUserods.FirstOrDefault(x => !x.IsHidden && x.UserName.ToLower() == userName.ToLower());
    }

    public static Userod GetUserByUserNumNoCache(long userNum)
    {
        return UserodCrud.SelectOne(userNum);
    }

    public static Userod GetUserByBadgeId(string badgeId)
    {
        var command = "SELECT * FROM userod WHERE BadgeId <> '' AND BadgeId = RIGHT('" + SOut.String(badgeId) + "', LENGTH(BadgeId))";
        //Example BadgeId in db="123". Select compares "123" with RIGHT('00000123',3)
        var listUserods = UserodCrud.TableToList(DataCore.GetTable(command));
        return listUserods.FirstOrDefault();
    }

    public static List<Userod> GetUsersByEmployeeNum(long employeeNum)
    {
        return GetWhere(x => x.EmployeeNum == employeeNum);
    }

    public static List<Userod> GetUsersByPermission(EnumPermType permissions, bool showHidden)
    {
        var listUserGroups = UserGroups.GetForPermission(permissions);
        var listUserNums = UserGroupAttaches.GetUserNumsForUserGroups(listUserGroups);
        return GetWhere(x => listUserNums.Contains(x.UserNum), !showHidden);
    }

    public static List<Userod> GetUsersWithProviders()
    {
        return GetWhere(x => x.ProvNum != 0, true);
    }

    public static List<Userod> GetUsersByProvNum(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum, true);
    }

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

    public static string GetName(long userNum)
    {
        var userod = GetFirstOrDefault(x => x.UserNum == userNum);
        if (userod == null) return "";

        return userod.UserName;
    }

    public static bool IsUserCpoe(Userod userod)
    {
        return false;
    }

    public static Userod CheckUserAndPassword(string userName, string plaintext, bool isEcw)
    {
        return CheckUserAndPassword(userName, plaintext, isEcw, true);
    }

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

    public static void Update(Userod userod, List<long> listUserGroupNums = null)
    {
        Validate(false, userod, false, listUserGroupNums);
        UserodCrud.Update(userod);
        if (listUserGroupNums == null) return;

        UserGroupAttaches.SyncForUser(userod, listUserGroupNums);
    }

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

    public static void DisassociateTaskListInBox(long taskListNum)
    {
        var command = "UPDATE userod SET TaskListInBox=0 WHERE TaskListInBox=" + SOut.Long(taskListNum);
        Db.NonQ(command);
    }

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
        userodCopy.UserNum = Insert(userodCopy, UserGroups.GetForUser(userod.UserNum).Select(x => x.UserGroupNum).ToList(), isForCemt);

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

    public static long GetInbox(long userNum)
    {
        var userod = GetFirstOrDefault(x => x.UserNum == userNum);
        if (userod == null) return 0;

        return userod.TaskListInBox;
    }

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

    public static void ResetStrongPasswordFlags()
    {
        var command = "UPDATE userod SET PasswordIsStrong=0";
        Db.NonQ(command);
    }

    public static bool IsInUserGroup(long userNum, long userGroupNum)
    {
        var listUserGroupAttaches = UserGroupAttaches.GetForUser(userNum);
        return listUserGroupAttaches.Select(x => x.UserGroupNum).Contains(userGroupNum);
    }

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

    public static Userod GetUserNoCache(long userNum)
    {
        var command = "SELECT * FROM userod WHERE userod.UserNum=" + SOut.Long(userNum);
        return UserodCrud.SelectOne(command);
    }

    public static string GetUserNameNoCache(long userNum)
    {
        var command = "SELECT userod.UserName FROM userod WHERE userod.UserNum=" + SOut.Long(userNum);
        return DataCore.GetScalar(command);
    }

    public static List<string> GetUserNamesNoCache()
    {
        var command = $@"SELECT userod.UserName FROM userod 
				WHERE userod.IsHidden=0 
				{(PrefC.GetBool(PrefName.UserNameManualEntry) ? " " : " AND userod.UserNumCEMT=0 ")}
				ORDER BY userod.UserName";
        return Db.GetListString(command);
    }

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
    
    public static bool HasSecurityAdminUserNoCache()
    {
        var command = @"SELECT COUNT(*) FROM userod
				INNER JOIN usergroupattach ON userod.UserNum=usergroupattach.UserNum
				INNER JOIN grouppermission ON usergroupattach.UserGroupNum=grouppermission.UserGroupNum 
				WHERE userod.IsHidden=0
				AND grouppermission.PermType=" + SOut.Int((int) EnumPermType.SecurityAdmin);
        return Db.GetCount(command) != "0";
    }

    public static bool CanUserSignNote(Userod userod = null)
    {
        var userodSig = userod;
        if (userod == null) userodSig = Security.CurUser;

        if (PrefC.GetBool(PrefName.NotesProviderSignatureOnly) && userodSig.ProvNum == 0) return false; //Prefernce is on and our user is not a provider.

        return true; //Either pref is off or it is on and user is a provider.
    }
    
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
    
    private static readonly UserodCache Cache = new();

    public static Userod GetFirstOrDefault(Func<Userod, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static List<Userod> GetWhere(Predicate<Userod> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static List<Userod> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        var table = Cache.GetTableFromCache(refreshCache);
        Security.SyncCurUser(); //Cache can have a stale reference to the Security.CurUser to ensure it has a current one.
        return table;
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static bool GetIsCacheAllowed()
    {
        return Cache.IsCacheAllowed;
    }

    public static void SetIsCacheAllowed(bool isCacheAllowed)
    {
        Cache.IsCacheAllowed = isCacheAllowed;
    }
}