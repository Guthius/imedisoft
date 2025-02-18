using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Forms;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormUserEdit : FormODBase
{
    public bool IsNew;
    public Userod UserodCur;

    private List<AlertSub> _listAlertSubsUserTypesOld;
    private List<UserGroup> _listUserGroups;
    private List<ClinicDto> _listClinics;

    ///<summary>The password that was entered in FormUserPassword.</summary>
    private string _passwordTyped;

    ///<summary>The alert categories that are available to be selected. Some alert types will not be displayed if this is not OD HQ.</summary>
    private List<AlertCategory> _listAlertCategories;

    private List<Employee> _listEmployees;
    private List<ProviderDto> _listProviders;
    private bool _isFromAddUser;
    private bool _isFillingList;
    private UserOdPref _userOdPrefLogOffAfterMinutes;
    private string _logOffAfterMinutesInitialValue;


    public FormUserEdit(Userod userod, bool isFromAddUser = false)
    {
        InitializeComponent();

        UserodCur = userod.Copy();

        _isFromAddUser = isFromAddUser;
    }

    private void FormUserEdit_Load(object sender, EventArgs e)
    {
        _userOdPrefLogOffAfterMinutes = UserOdPrefs.GetByUserAndFkeyType(UserodCur.UserNum, UserOdFkeyType.LogOffTimerOverride).FirstOrDefault();
        _logOffAfterMinutesInitialValue = _userOdPrefLogOffAfterMinutes?.ValueString ?? "";
        textLogOffAfterMinutes.Text = _logOffAfterMinutesInitialValue;
        checkIsHidden.Checked = UserodCur.IsHidden;
        if (UserodCur.UserNum != 0)
        {
            textUserNum.Text = UserodCur.UserNum.ToString();
        }

        textUserName.Text = UserodCur.UserName;
        if (!PrefC.GetBool(PrefName.DomainLoginEnabled))
        {
            labelDomainUser.Visible = false;
            textDomainUser.Visible = false;
            butPickDomainUser.Visible = false;
        }

        checkRequireReset.Checked = UserodCur.IsPasswordResetRequired;
        _listUserGroups = UserGroups.GetList();
        _isFillingList = true;
        for (var i = 0; i < _listUserGroups.Count; i++)
        {
            listUserGroup.Items.Add(_listUserGroups[i].Description, _listUserGroups[i]);
            if (!_isFromAddUser && UserodCur.IsInUserGroup(_listUserGroups[i].UserGroupNum))
            {
                listUserGroup.SetSelected(i);
            }

            if (_isFromAddUser && _listUserGroups[i].UserGroupNum == PrefC.GetLong(PrefName.DefaultUserGroup))
            {
                listUserGroup.SetSelected(i);
            }
        }

        if (listUserGroup.SelectedIndices.Count == 0)
        {
            listUserGroup.SelectedIndex = 0;
        }

        _isFillingList = false;

        securityTreeUser.FillTreePermissionsInitial();

        RefreshUserTree();

        listEmployee.Items.Clear();
        listEmployee.Items.Add("none");
        listEmployee.SelectedIndex = 0;

        _listEmployees = Employees.GetDeepCopy(true);
        for (var i = 0; i < _listEmployees.Count; i++)
        {
            listEmployee.Items.Add(Employees.GetName(_listEmployees[i]));
            if (UserodCur.EmployeeNum == _listEmployees[i].EmployeeNum)
            {
                listEmployee.SelectedIndex = i + 1;
            }
        }

        listProv.Items.Clear();
        listProv.Items.Add("none");
        listProv.SelectedIndex = 0;

        _listProviders = Providers.GetDeepCopy(true);
        for (var i = 0; i < _listProviders.Count; i++)
        {
            listProv.Items.Add(_listProviders[i].Description);
            if (UserodCur.ProvNum == _listProviders[i].Id)
            {
                listProv.SelectedIndex = i + 1;
            }
        }

        _listClinics = Clinics.GetDeepCopy(true);
        _listAlertSubsUserTypesOld = AlertSubs.GetAllForUser(UserodCur.UserNum);
        List<long> listClinicNumsSubscribed;
        var isAllClinicsSubscribed = false;
        if (_listAlertSubsUserTypesOld.Select(x => x.ClinicNum).Contains(-1))
        {
            //User subscribed to all clinics
            isAllClinicsSubscribed = true;
            listClinicNumsSubscribed = _listClinics.Select(x => x.Id).Distinct().ToList();
        }
        else
        {
            listClinicNumsSubscribed = _listAlertSubsUserTypesOld.Select(x => x.ClinicNum).Distinct().ToList();
        }

        var listAlertCategoryNums = _listAlertSubsUserTypesOld.Select(x => x.AlertCategoryNum).Distinct().ToList();
        listAlertSubMulti.Items.Clear();
        _listAlertCategories = AlertCategories.GetDeepCopy();
        var listAlertCategoryNumsUser = _listAlertSubsUserTypesOld.Select(x => x.AlertCategoryNum).ToList();
        for (var i = 0; i < _listAlertCategories.Count; i++)
        {
            listAlertSubMulti.Items.Add(Lan.g(this, _listAlertCategories[i].Description));
            listAlertSubMulti.SetSelected(i, listAlertCategoryNumsUser.Contains(_listAlertCategories[i].AlertCategoryNum));
        }

        listClinic.Items.Clear();
        listClinic.Items.Add("All");

        listAlertSubsClinicsMulti.Items.Add("All");
        listAlertSubsClinicsMulti.Items.Add("Headquarters");
        if (UserodCur.ClinicNum == 0)
        {
            //Unrestricted
            listClinic.SetSelected(0);
            listClinicMulti.Enabled = false;
        }

        if (isAllClinicsSubscribed)
        {
            //They are subscribed to all clinics
            listAlertSubsClinicsMulti.SetSelected(0);
        }
        else if (listClinicNumsSubscribed.Contains(0))
        {
            //They are subscribed to Headquarters
            listAlertSubsClinicsMulti.SetSelected(1);
        }

        var listUserClinics = UserClinics.GetForUser(UserodCur.UserNum);
        for (var i = 0; i < _listClinics.Count; i++)
        {
            listClinic.Items.Add(_listClinics[i].Abbr);
            listClinicMulti.Items.Add(_listClinics[i].Abbr);
            listAlertSubsClinicsMulti.Items.Add(_listClinics[i].Abbr);
            if (UserodCur.ClinicNum == _listClinics[i].Id)
            {
                listClinic.SetSelected(i + 1);
            }

            if (UserodCur.ClinicNum != 0 && listUserClinics.Exists(x => x.ClinicNum == _listClinics[i].Id))
            {
                listClinicMulti.SetSelected(i);
            }

            if (!isAllClinicsSubscribed && _listAlertSubsUserTypesOld.Exists(x => x.ClinicNum == _listClinics[i].Id))
            {
                listAlertSubsClinicsMulti.SetSelected(i + 2);
            }
        }

        if (string.IsNullOrEmpty(UserodCur.PasswordHash))
        {
            butPassword.Text = "Create Password";
        }

        if (IsNew)
        {
            butUnlock.Visible = false;
        }

        if (_isFromAddUser && !Security.IsAuthorized(EnumPermType.SecurityAdmin, true))
        {
            butPassword.Visible = false;
            checkRequireReset.Checked = true;
            checkRequireReset.Enabled = false;
            butUnlock.Visible = false;
        }
    }

    private void RefreshUserTree()
    {
        securityTreeUser.FillForUserGroup(listUserGroup.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).ToList());
    }

    private void listUserGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isFillingList)
        {
            return;
        }

        RefreshUserTree();
    }

    private void butPickDomainUser_Click(object sender, EventArgs e)
    {
    }

    private void listClinic_MouseClick(object sender, MouseEventArgs e)
    {
        var idx = listClinic.IndexFromPoint(e.Location);
        if (idx == -1)
        {
            return;
        }

        if (idx == 0)
        {
            //all
            listClinicMulti.Enabled = false;
            listClinicMulti.SetAll(false);
        }
        else
        {
            listClinicMulti.Enabled = true;
        }
    }

    private void butPassword_Click(object sender, EventArgs e)
    {
        var isCreate = string.IsNullOrEmpty(UserodCur.PasswordHash);
        using var formUserPassword = new FormUserPassword(isCreate, UserodCur.UserName);
        formUserPassword.IsInSecurityWindow = true;
        formUserPassword.ShowDialog();
        if (formUserPassword.DialogResult == DialogResult.Cancel)
        {
            return;
        }

        UserodCur.SetPassword(formUserPassword.Password);
        UserodCur.PasswordIsStrong = formUserPassword.IsPasswordStrong;
        _passwordTyped = formUserPassword.PasswordTyped;

        if (string.IsNullOrEmpty(UserodCur.PasswordHash))
        {
            butPassword.Text = "Create Password";
        }
        else
        {
            butPassword.Text = "Change Password";
        }
    }

    private void ButtonUnlock_Click(object sender, EventArgs e)
    {
        if (!Confirm("Users can become locked when invalid credentials have been entered several times in a row.\r\n" +
                     "Unlock this user so that more log in attempts can be made?"))
        {
            return;
        }

        UserodCur.DateTFail = DateTime.MinValue;
        UserodCur.FailedAttempts = 0;

        try
        {
            Userods.Update(UserodCur);

            ShowInfo("User has been unlocked.");
        }
        catch (Exception)
        {
            ShowError("There was a problem unlocking this user. Please call support or wait the allotted lock time.");
        }
    }

    private bool IsValidLogOffMinutes()
    {
        if (textLogOffAfterMinutes.Text != "" && (!int.TryParse(textLogOffAfterMinutes.Text, out var minutes) || minutes < 0))
        {
            ShowError(
                "Invalid 'Automatic logoff time in minutes'.\r\n" +
                "Must be blank, 0, or a positive integer.");

            return false;
        }

        return true;
    }

    private bool SaveLogOffPreferences()
    {
        var isCacheInvalid = false;
        if (textLogOffAfterMinutes.Text.IsNullOrEmpty() && !_logOffAfterMinutesInitialValue.IsNullOrEmpty())
        {
            UserOdPrefs.Delete(_userOdPrefLogOffAfterMinutes.UserOdPrefNum);
            isCacheInvalid = true;
        }
        else if (textLogOffAfterMinutes.Text != _logOffAfterMinutesInitialValue)
        {
            //Only do this if the value has changed
            if (_userOdPrefLogOffAfterMinutes == null)
            {
                _userOdPrefLogOffAfterMinutes = new UserOdPref {Fkey = 0, FkeyType = UserOdFkeyType.LogOffTimerOverride, UserNum = UserodCur.UserNum};
            }

            _userOdPrefLogOffAfterMinutes.ValueString = textLogOffAfterMinutes.Text;
            UserOdPrefs.Upsert(_userOdPrefLogOffAfterMinutes);
            isCacheInvalid = true;

            if (!PrefC.GetBool(PrefName.SecurityLogOffAllowUserOverride))
            {
                Warn("User logoff overrides will not take effect until the Global Security setting \"Allow user override for automatic logoff\" is checked");
            }
        }

        return isCacheInvalid;
    }

    private void butSave_Click(object sender, EventArgs e)
    {
        if (textUserName.Text == "")
        {
            ShowError("Please enter a username.");

            return;
        }

        if (IsNew && textUserName.Text != textUserName.Text.TrimEnd())
        {
            ShowError("User Name cannot end with white space.");
            return;
        }

        if (!_isFromAddUser && IsNew && PrefC.GetBool(PrefName.PasswordsMustBeStrong) && string.IsNullOrWhiteSpace(_passwordTyped))
        {
            ShowError("Password may not be blank when the strong password feature is turned on.");
            return;
        }

        if (listClinic.SelectedIndex == -1)
        {
            ShowError("This user does not have a User Default Clinic set.  Please choose one to continue.");
            return;
        }

        if (listUserGroup.SelectedIndices.Count == 0)
        {
            ShowError("Users must have at least one user group associated. Please select a user group to continue.");
            return;
        }

        if (_isFromAddUser && !Security.IsAuthorized(EnumPermType.SecurityAdmin, true))
        {
            if (listUserGroup.SelectedIndices.Count != 1 || !listUserGroup.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).Contains(PrefC.GetLong(PrefName.DefaultUserGroup)))
            {
                ShowError("This user must be assigned to the default user group.");
                
                for (var i = 0; i < listUserGroup.Items.Count; i++)
                {
                    if (((UserGroup) listUserGroup.Items.GetObjectAt(i)).UserGroupNum == PrefC.GetLong(PrefName.DefaultUserGroup))
                    {
                        listUserGroup.SetSelected(i);
                    }
                    else
                    {
                        listUserGroup.SetSelected(i, false);
                    }
                }

                return;
            }
        }

        if (!IsValidLogOffMinutes())
        {
            return;
        }

        var userClinics = new List<UserClinic>();
        foreach (var index in listClinicMulti.SelectedIndices)
        {
            userClinics.Add(new UserClinic(_listClinics[index].Id, UserodCur.UserNum));
        }

        if (userClinics.Count > 0 && !userClinics.Exists(x => x.ClinicNum == _listClinics[listClinic.SelectedIndex - 1].Id))
        {
            ShowError("User cannot have a default clinic that they are not restricted to.");
            
            return;
        }

        UserodCur.ClinicNum = listClinic.SelectedIndex == 0 ? 0 : _listClinics[listClinic.SelectedIndex - 1].Id;
        UserodCur.ClinicIsRestricted = listClinicMulti.SelectedIndices.Count > 0;
        UserodCur.IsHidden = checkIsHidden.Checked;
        UserodCur.IsPasswordResetRequired = checkRequireReset.Checked;
        UserodCur.UserName = textUserName.Text;
        UserodCur.EmployeeNum = listEmployee.SelectedIndex == 0 ? 0 : _listEmployees[listEmployee.SelectedIndex - 1].EmployeeNum;
        UserodCur.ProvNum = listProv.SelectedIndex == 0 ? 0 : _listProviders[listProv.SelectedIndex - 1].Id;

        if (IsNew)
        {
            try
            {
                Userods.Insert(UserodCur, listUserGroup.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).ToList());
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                
                return;
            }

            foreach (var userClinic in userClinics)
            {
                userClinic.UserNum = UserodCur.UserNum;
            }

            SecurityLogs.MakeLogEntry(EnumPermType.AddNewUser, 0, "New user '" + UserodCur.UserName + "' added");
        }
        else
        {
            var listUserGroupsNew = listUserGroup.GetListSelected<UserGroup>();
            var listUserGroupsOld = UserodCur.GetGroups();
            try
            {
                Userods.Update(UserodCur, listUserGroupsNew.Select(x => x.UserGroupNum).ToList());
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return;
            }
            
            if (UserodCur.UserNum == Security.CurUser.UserNum)
            {
                Security.CurUser = UserodCur.Copy();
            }

            //Log changes to the User's UserGroups.
            Func<List<UserGroup>, List<UserGroup>, List<UserGroup>> funcGetMissing = (listUserGroups1, listUserGroups2) =>
            {
                var listUserGroupsRet = new List<UserGroup>();
                for (var i = 0; i < listUserGroups1.Count; i++)
                {
                    if (listUserGroups2.Exists(x => x.UserGroupNum == listUserGroups1[i].UserGroupNum))
                    {
                        continue;
                    }

                    listUserGroupsRet.Add(listUserGroups1[i]);
                }

                return listUserGroupsRet;
            };
            
            var listUserGroupsRemoved = funcGetMissing(listUserGroupsOld, listUserGroupsNew);
            var listUserGroupsAdded = funcGetMissing(listUserGroupsNew, listUserGroupsOld);
            
            if (listUserGroupsRemoved.Count > 0)
            {
                //Only log if there are items in the list
                SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, "User " + UserodCur.UserName + " removed from User group(s): " + string.Join(", ", listUserGroupsRemoved.Select(x => x.Description).ToArray()) + " by: " + Security.CurUser.UserName);
            }

            if (listUserGroupsAdded.Count > 0)
            {
                //Only log if there are items in the list.
                SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, "User " + UserodCur.UserName + " added to User group(s): " + string.Join(", ", listUserGroupsAdded.Select(x => x.Description).ToArray()) + " by: " + Security.CurUser.UserName);
            }
        }

        if (UserClinics.Sync(userClinics, UserodCur.UserNum))
        {
            DataValid.SetInvalid(InvalidType.UserClinics);
        }

        var isUserOdPrefCacheInvalid = false;
        
        DataValid.SetInvalid(InvalidType.Security);
        
        var listAlertCatagoryNumsUser = new List<long>();
        for (var i = 0; i < listAlertSubMulti.SelectedIndices.Count; i++)
        {
            listAlertCatagoryNumsUser.Add(_listAlertCategories[listAlertSubMulti.SelectedIndices[i]].AlertCategoryNum);
        }

        var clinicNums = new List<long>();
        for (var i = 0; i < listAlertSubsClinicsMulti.SelectedIndices.Count; i++)
        {
            if (listAlertSubsClinicsMulti.SelectedIndices[i] == 0)
            {
                //All
                clinicNums.Add(-1); //Add All
                break;
            }

            if (listAlertSubsClinicsMulti.SelectedIndices[i] == 1)
            {
                //HQ
                clinicNums.Add(0);
                continue;
            }

            var clinic = _listClinics[listAlertSubsClinicsMulti.SelectedIndices[i] - 2];
            
            clinicNums.Add(clinic.Id);
        }

        var listAlertSubsUserTypesNew = _listAlertSubsUserTypesOld.Select(x => x.Copy()).ToList();
        
        listAlertSubsUserTypesNew.RemoveAll(x => !listAlertCatagoryNumsUser.Contains(x.AlertCategoryNum));
        listAlertSubsUserTypesNew.RemoveAll(x => !clinicNums.Contains(x.ClinicNum));

        foreach (var t in listAlertCatagoryNumsUser)
        {
            foreach (var clinicNum in clinicNums)
            {
                if (!_listAlertSubsUserTypesOld.Exists(x => x.ClinicNum == clinicNum && x.AlertCategoryNum == t))
                {
                    //Was not subscribed to type.
                    listAlertSubsUserTypesNew.Add(new AlertSub(UserodCur.UserNum, clinicNum, t));
                    continue;
                }
            }
        }

        isUserOdPrefCacheInvalid |= SaveLogOffPreferences();
        AlertSubs.Sync(listAlertSubsUserTypesNew, _listAlertSubsUserTypesOld);
        if (isUserOdPrefCacheInvalid)
        {
            DataValid.SetInvalid(InvalidType.UserOdPrefs);
        }

        DialogResult = DialogResult.OK;
    }
}