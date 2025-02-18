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
        var visibleUsers = new List<Userod>();
        var users = GetDeepCopy();

        foreach (var user in users)
        {
            if (user.IsHidden)
            {
                continue;
            }

            visibleUsers.Add(user);
        }

        return visibleUsers;
    }

    public static Userod GetUserByNameNoCache(string userName)
    {
        var commandText = "SELECT * FROM userod WHERE UserName='" + SOut.String(userName) + "'";

        var users = UserodCrud.TableToList(DataCore.GetTable(commandText));

        return users.FirstOrDefault(x => !x.IsHidden && string.Equals(x.UserName, userName, StringComparison.CurrentCultureIgnoreCase));
    }

    public static Userod GetUserByBadgeId(string badgeId)
    {
        var commandText = "SELECT * FROM userod WHERE BadgeId <> '' AND BadgeId = RIGHT('" + SOut.String(badgeId) + "', LENGTH(BadgeId))";

        var users = UserodCrud.TableToList(DataCore.GetTable(commandText));

        return users.FirstOrDefault();
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

    public static List<Userod> GetUsersForVerifyList(List<long> clinicNums, bool isAssigning, bool includeHiddenUsers = false)
    {
        var userNumsInInsVerify = InsVerifies.GetAllInsVerifyUserNums();
        var userNumsInClinic = new List<long>();

        if (clinicNums.Count > 0)
        {
            foreach (var clinicNum in clinicNums)
            {
                userNumsInClinic.AddRange(UserClinics.GetForClinic(clinicNum).Select(y => y.UserNum).Distinct());
            }

            userNumsInClinic.AddRange(GetUsers().Where(x => !x.ClinicIsRestricted).Select(x => x.UserNum).Distinct());
            userNumsInClinic = userNumsInClinic.Distinct().ToList();

            if (userNumsInClinic.Count > 0)
            {
                userNumsInInsVerify = userNumsInInsVerify.FindAll(x => userNumsInClinic.Contains(x));
            }

            userNumsInInsVerify.AddRange(GetUsers(userNumsInInsVerify).Where(x => !x.ClinicIsRestricted).Select(x => x.UserNum).Distinct());
            userNumsInInsVerify = userNumsInInsVerify.Distinct().ToList();
        }

        var usersWithPermission = GetUsersByPermission(EnumPermType.InsPlanVerifyList, includeHiddenUsers);
        if (!isAssigning)
        {
            return usersWithPermission.FindAll(x => userNumsInInsVerify.Contains(x.UserNum));
        }

        return clinicNums.Count == 0 ? usersWithPermission : usersWithPermission.FindAll(x => userNumsInClinic.Contains(x.UserNum));
    }

    public static string GetName(long userNum)
    {
        var user = GetFirstOrDefault(x => x.UserNum == userNum);

        return user == null ? "" : user.UserName;
    }

    public static bool IsUserCpoe()
    {
        return false;
    }

    public static Userod CheckUserAndPassword(string userName, string plaintext, bool hasExceptions = true)
    {
        var user = GetUserByNameNoCache(userName);
        if (user is null)
        {
            if (hasExceptions)
            {
                throw new ODException("Invalid username or password.", ODException.ErrorCodes.CheckUserAndPasswordFailed);
            }

            return null;
        }

        var dateTimeNowDb = MiscData.GetNowDateTime();

        if (user.DateTFail.Year > 1880 && dateTimeNowDb.Subtract(user.DateTFail) < TimeSpan.FromMinutes(5) && user.FailedAttempts >= 5)
        {
            if (hasExceptions)
            {
                throw new ApplicationException(
                    "Account has been locked due to failed log in attempts.\r\n" +
                    "Call your security admin to unlock your account or wait at least 5 minutes.");
            }

            return null;
        }

        var isPasswordValid = Authentication.CheckPassword(user, plaintext);
        var updatedUser = user.Copy();

        if (user.DateTFail.Year > 1880 && dateTimeNowDb.Subtract(user.DateTFail) > TimeSpan.FromMinutes(5))
        {
            updatedUser.FailedAttempts = 0;
            updatedUser.DateTFail = DateTime.MinValue;
        }

        if (!isPasswordValid)
        {
            updatedUser.DateTFail = dateTimeNowDb;
            updatedUser.FailedAttempts += 1;
        }

        UserodCrud.Update(updatedUser, user);
        if (isPasswordValid)
        {
            if (string.IsNullOrEmpty(plaintext) || updatedUser.GetPasswordContainer().HashType == HashTypes.SHA3_512)
            {
                return updatedUser;
            }

            Authentication.UpdatePasswordUserod(updatedUser, plaintext);

            updatedUser = GetUserNoCache(updatedUser.UserNum);

            return updatedUser;
        }

        if (hasExceptions)
        {
            throw new ODException("Invalid username or password.", ODException.ErrorCodes.CheckUserAndPasswordFailed);
        }

        return null;
    }

    public static void Update(Userod user, List<long> userGroupNums = null)
    {
        Validate(false, user, false, userGroupNums);

        UserodCrud.Update(user);

        if (userGroupNums is null)
        {
            return;
        }

        UserGroupAttaches.SyncForUser(user, userGroupNums);
    }

    public static void UpdatePassword(Userod user, PasswordContainer passwordContainer, bool isPasswordStrong)
    {
        var userodToUpdate = user.Copy();

        userodToUpdate.SetPassword(passwordContainer);
        userodToUpdate.PasswordIsStrong = isPasswordStrong;

        var userGroups = userodToUpdate.GetGroups();
        if (userGroups.Count < 1)
        {
            throw new Exception("The current user must be in at least one user group.");
        }

        Validate(false, userodToUpdate, true, userGroups.Select(x => x.UserGroupNum).ToList());

        UserodCrud.Update(userodToUpdate);
    }

    public static void DisassociateTaskListInBox(long taskListNum)
    {
        Db.NonQ("UPDATE userod SET TaskListInBox = 0 WHERE TaskListInBox = " + taskListNum);
    }

    public static long Insert(Userod userod, List<long> userGroupNums)
    {
        if (userod.IsHidden && UserGroups.IsAdminGroup(userGroupNums))
        {
            throw new Exception("Admins cannot be hidden.");
        }

        Validate(true, userod, false, userGroupNums);

        var userNum = UserodCrud.Insert(userod);

        UserGroupAttaches.SyncForUser(userod, userGroupNums);

        return userNum;
    }

    public static void Validate(bool isNew, Userod userod, bool excludeHiddenUsers, List<long> userGroupNums)
    {
        var excludeUserNum = isNew ? 0 : userod.UserNum;

        if (!userod.IsHidden)
        {
            if (!IsUserNameUnique(userod.UserName, excludeUserNum, excludeHiddenUsers))
            {
                throw new ApplicationException("UserName already in use by CEMT member.");
            }

            if (!IsUserNameUnique(userod.UserName, excludeUserNum, excludeHiddenUsers))
            {
                throw new ApplicationException("UserName already in use.");
            }
        }

        if (userGroupNums == null)
        {
            return;
        }

        if (userGroupNums.Count == 0)
        {
            throw new ApplicationException("The current user must be in at least one user group.");
        }

        var commandText =
            "SELECT COUNT(*) FROM grouppermission " +
            "WHERE PermType = " + (int) EnumPermType.SecurityAdmin + " " +
            "AND UserGroupNum IN (" + string.Join(", ", userGroupNums) + ") ";

        if (!isNew && Db.GetCount(commandText) == "0" && !IsSomeoneElseSecurityAdmin(userod))
        {
            throw new ApplicationException("At least one user must have Security Admin permission.");
        }

        if (userod.IsHidden && Db.GetCount(commandText) != "0")
        {
            throw new ApplicationException("Admins cannot be hidden.");
        }
    }

    public static bool IsSomeoneElseSecurityAdmin(Userod userod)
    {
        var commandText =
            "SELECT COUNT(*) FROM userod " +
            "INNER JOIN usergroupattach ON usergroupattach.UserNum = userod.UserNum " +
            "INNER JOIN grouppermission ON usergroupattach.UserGroupNum = grouppermission.UserGroupNum " +
            "WHERE grouppermission.PermType = " + (int) EnumPermType.SecurityAdmin + " " +
            "AND userod.IsHidden = 0 AND userod.UserNum != " + userod.UserNum;

        return Db.GetCount(commandText) != "0";
    }

    public static bool IsUserNameUnique(string userName, long excludeUserNum, bool excludeHiddenUsers)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return false;
        }

        var commandText = "SELECT COUNT(*) FROM userod WHERE UserName = '" + SOut.String(userName) + "' AND UserNum != " + excludeUserNum;
        if (excludeHiddenUsers)
        {
            commandText += " AND IsHidden = 0";
        }

        var dataTable = DataCore.GetTable(commandText);

        return dataTable.Rows[0][0].ToString() == "0";
    }

    public static bool TryGetUniqueUsername(string userName, long excludeUserNum, bool excludeHiddenUsers, out string uniqueUserName)
    {
        var attempt = 1;

        uniqueUserName = userName;

        while (!IsUserNameUnique(uniqueUserName, excludeUserNum, excludeHiddenUsers))
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

    public static Userod CopyUser(Userod userod, PasswordContainer passwordContainer, bool isPasswordStrong, string userName = null)
    {
        if (!TryGetUniqueUsername(userName ?? userod.UserName + "(Copy)", 0, false, out var uniqueUserName))
        {
            return null;
        }

        var userodCopy = new Userod();
        userodCopy.UserName = uniqueUserName;
        userodCopy.SetPassword(passwordContainer);
        userodCopy.PasswordIsStrong = isPasswordStrong;
        userodCopy.ClinicIsRestricted = userod.ClinicIsRestricted;
        userodCopy.ClinicNum = userod.ClinicNum;

        userodCopy.UserNum = Insert(userodCopy, UserGroups.GetForUser(userod.UserNum).Select(x => x.UserGroupNum).ToList());

        var userClinics = new List<UserClinic>(UserClinics.GetForUser(userod.UserNum));
        userClinics.ForEach(x => x.UserNum = userodCopy.UserNum);
        UserClinics.Sync(userClinics, userodCopy.UserNum);

        var alertSubsUsers = AlertSubs.GetAllForUser(userod.UserNum);
        alertSubsUsers.ForEach(x => x.UserNum = userodCopy.UserNum);
        AlertSubs.Sync(alertSubsUsers, []);

        return userodCopy;
    }

    public static List<Userod> GetForGroup(long userGroupNum)
    {
        return GetWhere(x => x.IsInUserGroup(userGroupNum));
    }

    public static List<Userod> GetUsersOnlyThisClinic(long clinicNum)
    {
        return UserodCrud.SelectMany(
            "SELECT userod.* FROM( " +
            "SELECT userclinic.UserNum, COUNT(userclinic.ClinicNum) Clinics FROM userclinic " +
            "GROUP BY userNum HAVING Clinics = 1 ) users " +
            "INNER JOIN userclinic ON userclinic.UserNum = users.UserNum " +
            "AND userclinic.ClinicNum = " + clinicNum + " " +
            "INNER JOIN userod ON userod.UserNum = userclinic.UserNum");
    }

    public static long GetInbox(long userNum)
    {
        var userod = GetFirstOrDefault(x => x.UserNum == userNum);

        return userod?.TaskListInBox ?? 0;
    }

    public static string IsPasswordStrong(string password)
    {
        if (password == "")
        {
            return "Password may not be blank.";
        }

        if (password.Length < 8)
        {
            return "Password must be at least eight characters long.";
        }

        var containsCap = false;
        foreach (var ch in password)
        {
            if (char.IsUpper(ch))
            {
                containsCap = true;
            }
        }

        if (!containsCap)
        {
            return "Password must contain at least one capital letter.";
        }

        var containsLower = false;
        foreach (var ch in password)
        {
            if (char.IsLower(ch))
            {
                containsLower = true;
            }
        }

        if (!containsLower)
        {
            return "Password must contain at least one lower case letter.";
        }

        if (!PrefC.GetBool(PrefName.PasswordsStrongIncludeSpecial))
        {
            return password.Any(char.IsNumber) ? "" : "Password must contain at least one number.";
        }

        var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
        if (!hasSpecial)
        {
            return "Password must contain at least one special character when the 'strong passwords require a special character' feature is turned on.";
        }

        return password.Any(char.IsNumber) ? "" : "Password must contain at least one number.";
    }

    public static void ResetStrongPasswordFlags()
    {
        Db.NonQ("UPDATE userod SET PasswordIsStrong = 0");
    }

    public static bool IsInUserGroup(long userNum, long userGroupNum)
    {
        return UserGroupAttaches.GetForUser(userNum).Select(x => x.UserGroupNum).Contains(userGroupNum);
    }

    public static long GetFirstSecurityAdminUserNumNoPasswordNoCache()
    {
        var commandText =
            $"""
             SELECT userod.UserNum, CASE WHEN COALESCE(userod.Password, '') = '' THEN 0 ELSE 1 END HasPassword 
             FROM userod
             INNER JOIN usergroupattach ON userod.UserNum = usergroupattach.UserNum
             INNER JOIN grouppermission ON usergroupattach.UserGroupNum = grouppermission.UserGroupNum 
             WHERE userod.IsHidden = 0
             AND grouppermission.PermType = {(int) EnumPermType.SecurityAdmin}
             GROUP BY userod.UserNum
             ORDER BY userod.UserName
             LIMIT 1
             """;

        var dataTable = DataCore.GetTable(commandText);

        long userNumAdminNoPass = 0;
        if (dataTable is {Rows.Count: > 0} && dataTable.Rows[0]["HasPassword"].ToString() == "0")
        {
            userNumAdminNoPass = SIn.Long(dataTable.Rows[0]["UserNum"].ToString());
        }

        return userNumAdminNoPass;
    }

    public static Userod GetUserNoCache(long userNum)
    {
        return UserodCrud.SelectOne("SELECT * FROM userod WHERE UserNum = " + userNum);
    }

    public static string GetUserNameNoCache(long userNum)
    {
        return DataCore.GetScalar("SELECT UserName FROM userod WHERE UserNum = " + userNum);
    }

    public static List<string> GetUserNamesNoCache()
    {
        return Db.GetListString(
            $"""
             SELECT userod.UserName FROM userod 
             WHERE userod.IsHidden = 0 
             {(PrefC.GetBool(PrefName.UserNameManualEntry) ? " " : " AND userod.UserNumCEMT=0 ")}
             ORDER BY userod.UserName
             """);
    }

    public static bool HasSecurityAdminUserNoCache()
    {
        var command = @"SELECT COUNT(*) FROM userod
				INNER JOIN usergroupattach ON userod.UserNum=usergroupattach.UserNum
				INNER JOIN grouppermission ON usergroupattach.UserGroupNum=grouppermission.UserGroupNum 
				WHERE userod.IsHidden=0
				AND grouppermission.PermType=" + (int) EnumPermType.SecurityAdmin;
        return Db.GetCount(command) != "0";
    }

    public static bool CanUserSignNote(Userod userod = null)
    {
        userod ??= Security.CurUser;

        return !PrefC.GetBool(PrefName.NotesProviderSignatureOnly) || userod.ProvNum != 0;
    }

    private class UserodCache : CacheListAbs<Userod>
    {
        protected override List<Userod> GetCacheFromDb()
        {
            return UserodCrud.SelectMany("SELECT * FROM userod ORDER BY UserName");
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
            GetTableFromCache(false);
        }

        protected override bool IsInListShort(Userod item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly UserodCache Cache = new();

    public static Userod GetFirstOrDefault(Func<Userod, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<Userod> GetWhere(Predicate<Userod> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static List<Userod> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);

        Security.SyncCurUser();
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