using System;
using System.Collections.Generic;
using Imedisoft.Core.Data;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;



public class Userod : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long UserNum;

    public string UserName;
    public string Password;

    ///<summary>FK to employee.EmployeeNum. Used for timecards to block access by other users.</summary>
    public long EmployeeNum;

    ///<summary>FK to clinic.ClinicNum.  Default clinic for this user.  It causes new patients to default to this clinic when entered by this user.  
    ///If 0, then user has no default clinic or default clinic is HQ if clinics are enabled.</summary> 		
    public long ClinicNum;

    ///<summary>FK to provider.ProvNum.  It is possible to have multiple userods attached to a single provider.</summary>
    public long ProvNum;

    ///<summary>Set true to hide user from login list.</summary>
    public bool IsHidden;

    ///<summary>FK to tasklist.TaskListNum.  0 if no inbox setup yet.  It is assumed that the TaskList is in the main trunk, but this is not strictly enforced.  User can't delete an attached TaskList, but they could move it.</summary>
    public long TaskListInBox;
    
    ///<summary>If set to true, the BlockSubsc button will start out pressed for this user.</summary>
    public bool DefaultHidePopups;

    ///<summary>Gets set to true if strong passwords are turned on, and this user changes their password to a strong password.  We don't store actual passwords, so this flag is the only way to tell.</summary>
    public bool PasswordIsStrong;

    ///<summary>When true, prevents user from having access to clinics that are not in the corresponding userclinic table.
    ///Many places throughout the program will optionally remove the 'All' option from this user when true.</summary>
    public bool ClinicIsRestricted;

    ///<summary>The date and time of the most recent log in failure for this user.  Set to MinValue after user logs in successfully.</summary>
    public DateTime DateTFail;

    ///<summary>The number of times this user has failed to log into their account.  Set to 0 after user logs in successfully.</summary>
    public byte FailedAttempts;
    
    ///<summary>Boolean.  If true, the user's password needs to be reset on next login.</summary>
    public bool IsPasswordResetRequired;

    ///<summary>Minimum date if last login date and time is unknown.
    ///Otherwise contians the last date and time this user successfully logged in.</summary>
    public DateTime DateTLastLogin;

    public PasswordContainer GetPasswordContainer()
    {
        return Authentication.DecodePass(Password);
    }

    public void SetPassword(PasswordContainer passwordContainer)
    {
        Password = passwordContainer.ToString();
    }

    public string PasswordHash => GetPasswordContainer().Hash;

    public EServiceTypes EServiceType { get; set; }

    public Userod Copy()
    {
        return (Userod) MemberwiseClone();
    }

    public override string ToString()
    {
        return UserName;
    }

    public bool IsInUserGroup(long userGroupNum)
    {
        return Userods.IsInUserGroup(UserNum, userGroupNum);
    }

    public List<UserGroup> GetGroups()
    {
        return UserGroups.GetForUser(UserNum);
    }
}

public enum EServiceTypes
{
    None
}