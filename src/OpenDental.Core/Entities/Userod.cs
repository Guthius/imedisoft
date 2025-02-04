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

    ///<summary>Deprecated. Use UserGroupAttaches to link Userods to UserGroups.</summary>
    public long UserGroupNum;

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

    /// <summary> Defaults to 3 (regular user) unless specified. Helps populates the Anesthetist, Surgeon, Assistant and Circulator dropdowns properly on FormAnestheticRecord/// </summary>
    public int AnesthProvType;

    ///<summary>If set to true, the BlockSubsc button will start out pressed for this user.</summary>
    public bool DefaultHidePopups;

    ///<summary>Gets set to true if strong passwords are turned on, and this user changes their password to a strong password.  We don't store actual passwords, so this flag is the only way to tell.</summary>
    public bool PasswordIsStrong;

    ///<summary>When true, prevents user from having access to clinics that are not in the corresponding userclinic table.
    ///Many places throughout the program will optionally remove the 'All' option from this user when true.</summary>
    public bool ClinicIsRestricted;

    ///<summary>If set to true, the BlockInbox button will start out pressed for this user.</summary>
    public bool InboxHidePopups;

    ///<summary>FK to userod.UserNum.  The user num within the Central Manager database.  Only editable via CEMT.  Can change when CEMT syncs.</summary>
    public long UserNumCEMT;

    ///<summary>The date and time of the most recent log in failure for this user.  Set to MinValue after user logs in successfully.</summary>
    public DateTime DateTFail;

    ///<summary>The number of times this user has failed to log into their account.  Set to 0 after user logs in successfully.</summary>
    public byte FailedAttempts;

    /// <summary>The username for the ActiveDirectory user to link the account to.</summary>
    public string DomainUser;

    ///<summary>Boolean.  If true, the user's password needs to be reset on next login.</summary>
    public bool IsPasswordResetRequired;

    ///<summary>A hashed pin that is used for mobile web validation on eClipboard. Not used in OD proper.</summary>
    public string MobileWebPin;

    ///<summary>The number of attempts the mobile web pin has failed. Reset on successful attempt.</summary>
    public byte MobileWebPinFailedAttempts;

    ///<summary>Minimum date if last login date and time is unknown.
    ///Otherwise contians the last date and time this user successfully logged in.</summary>
    public DateTime DateTLastLogin;

    ///<summary>Pin for ODT. This is the hashed value of the pin. Not used in OD proper.</summary>
    public string EClipboardClinicalPin;

    ///<summary>A unique number that corresponds to the number on an employee badge. The last numbers on an employee badge. Will be 1 to 4 digits. These numbers are assigned to the badges by the factory. We order a specific range of badges, such as 1801-2000, which are assigned in order and not reused to avoid duplicates. The first four digits on the badges are not used by the Lenel OnGuard software, so we do not use them here either.</summary>
    public string BadgeId;

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
    None,
    EConnector,
    Broadcaster,
    BroadcastMonitor,
    ServiceMainHQ,
    OpenDentalService,
    OpenDentalAPIService
}