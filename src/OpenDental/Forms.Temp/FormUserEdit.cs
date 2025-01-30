using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;

namespace OpenDental;

public partial class FormUserEdit : FormODBase {
		
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
	private List<Provider> _listProviders;
	private bool _isFromAddUser;
	private bool _isFillingList;
	private UserOdPref _userOdPrefLogOffAfterMinutes;
	private string _logOffAfterMinutesInitialValue;

		
	public FormUserEdit(Userod userod,bool isFromAddUser=false) {
		InitializeComponent();

		UserodCur=userod.Copy();
		_isFromAddUser=isFromAddUser;
	}

	private void FormUserEdit_Load(object sender, System.EventArgs e) {
		_userOdPrefLogOffAfterMinutes=UserOdPrefs.GetByUserAndFkeyType(UserodCur.UserNum,UserOdFkeyType.LogOffTimerOverride).FirstOrDefault();
		_logOffAfterMinutesInitialValue=_userOdPrefLogOffAfterMinutes?.ValueString??"";
		textLogOffAfterMinutes.Text=_logOffAfterMinutesInitialValue;
		checkIsHidden.Checked=UserodCur.IsHidden;
		if(UserodCur.UserNum!=0) {
			textUserNum.Text=UserodCur.UserNum.ToString();
		}
		textUserName.Text=UserodCur.UserName;
		if(!string.IsNullOrEmpty(UserodCur.DomainUser) && UserodCur.DomainUser.Split('\\').Length>1) {
			textDomainUser.Text=UserodCur.DomainUser.Split('\\')[1];
		}
		if(!PrefC.GetBool(PrefName.DomainLoginEnabled)) {
			labelDomainUser.Visible=false;
			textDomainUser.Visible=false;
			butPickDomainUser.Visible=false;
		}
		checkRequireReset.Checked=UserodCur.IsPasswordResetRequired;
		_listUserGroups=UserGroups.GetList();
		_isFillingList=true;
		for(var i=0;i<_listUserGroups.Count;i++){
			listUserGroup.Items.Add(_listUserGroups[i].Description,_listUserGroups[i]);
			if(!_isFromAddUser && UserodCur.IsInUserGroup(_listUserGroups[i].UserGroupNum)) {
				listUserGroup.SetSelected(i);
			}
			if(_isFromAddUser && _listUserGroups[i].UserGroupNum==PrefC.GetLong(PrefName.DefaultUserGroup)) {
				listUserGroup.SetSelected(i);
			}
		}
		if(listUserGroup.SelectedIndices.Count==0){//never allowed to delete last group, so this won't fail
			listUserGroup.SelectedIndex=0;
		}
		_isFillingList=false;
		securityTreeUser.FillTreePermissionsInitial();
		RefreshUserTree();
		listEmployee.Items.Clear();
		listEmployee.Items.Add(Lan.g(this,"none"));
		listEmployee.SelectedIndex=0;
		_listEmployees=Employees.GetDeepCopy(true);
		for(var i=0;i<_listEmployees.Count;i++){
			listEmployee.Items.Add(Employees.GetName(_listEmployees[i]));
			if(UserodCur.EmployeeNum==_listEmployees[i].EmployeeNum) {
				listEmployee.SelectedIndex=i+1;
			}
		}
		listProv.Items.Clear();
		listProv.Items.Add(Lan.g(this,"none"));
		listProv.SelectedIndex=0;
		_listProviders=Providers.GetDeepCopy(true);
		for(var i=0;i<_listProviders.Count;i++) {
			listProv.Items.Add(_listProviders[i].GetLongDesc());
			if(UserodCur.ProvNum==_listProviders[i].ProvNum) {
				listProv.SelectedIndex=i+1;
			}
		}
		_listClinics=Clinics.GetDeepCopy(true);
		_listAlertSubsUserTypesOld=AlertSubs.GetAllForUser(UserodCur.UserNum);
		List<long> listClinicNumsSubscribed;
		var isAllClinicsSubscribed=false;
		if(_listAlertSubsUserTypesOld.Select(x => x.ClinicNum).Contains(-1)) {//User subscribed to all clinics
			isAllClinicsSubscribed=true;
			listClinicNumsSubscribed=_listClinics.Select(x => x.Id).Distinct().ToList();
		}
		else {
			listClinicNumsSubscribed=_listAlertSubsUserTypesOld.Select(x => x.ClinicNum).Distinct().ToList();
		}
		var listAlertCategoryNums=_listAlertSubsUserTypesOld.Select(x => x.AlertCategoryNum).Distinct().ToList();
		listAlertSubMulti.Items.Clear();
		_listAlertCategories=AlertCategories.GetDeepCopy();
		var listAlertCategoryNumsUser=_listAlertSubsUserTypesOld.Select(x => x.AlertCategoryNum).ToList();
		for(var i=0;i<_listAlertCategories.Count;i++) {
			listAlertSubMulti.Items.Add(Lan.g(this,_listAlertCategories[i].Description));
			listAlertSubMulti.SetSelected(i,listAlertCategoryNumsUser.Contains(_listAlertCategories[i].AlertCategoryNum));
		}
		if(!true) {
			tabClinics.Enabled=false;//Disables all controls in the clinics tab.  Tab is still selectable.
			listAlertSubsClinicsMulti.Visible=false;
			labelAlertClinic.Visible=false;
		}
		else {
			listClinic.Items.Clear();
			listClinic.Items.Add(Lan.g(this,"All"));
			listAlertSubsClinicsMulti.Items.Add(Lan.g(this,"All"));
			listAlertSubsClinicsMulti.Items.Add(Lan.g(this,"Headquarters"));
			if(UserodCur.ClinicNum==0) {//Unrestricted
				listClinic.SetSelected(0);
				listClinicMulti.Enabled=false;
			}
			if(isAllClinicsSubscribed) {//They are subscribed to all clinics
				listAlertSubsClinicsMulti.SetSelected(0);
			}
			else if(listClinicNumsSubscribed.Contains(0)) {//They are subscribed to Headquarters
				listAlertSubsClinicsMulti.SetSelected(1);
			}
			var listUserClinics=UserClinics.GetForUser(UserodCur.UserNum);
			for(var i=0;i<_listClinics.Count;i++) {
				listClinic.Items.Add(_listClinics[i].Abbr);
				listClinicMulti.Items.Add(_listClinics[i].Abbr);
				listAlertSubsClinicsMulti.Items.Add(_listClinics[i].Abbr);
				if(UserodCur.ClinicNum==_listClinics[i].Id) {
					listClinic.SetSelected(i+1);
				}
				if(UserodCur.ClinicNum!=0 && listUserClinics.Exists(x => x.ClinicNum==_listClinics[i].Id)) {
					listClinicMulti.SetSelected(i);//No "All" option, don't select i+1
				}
				if(!isAllClinicsSubscribed && _listAlertSubsUserTypesOld.Exists(x => x.ClinicNum==_listClinics[i].Id)) {
					listAlertSubsClinicsMulti.SetSelected(i+2);//All+HQ
				}
			}
		}
		if(string.IsNullOrEmpty(UserodCur.PasswordHash)){
			butPassword.Text=Lan.g(this,"Create Password");
		}
		if(IsNew) {
			butUnlock.Visible=false;
		}
		if(_isFromAddUser && !Security.IsAuthorized(EnumPermType.SecurityAdmin,true)) {
			butPassword.Visible=false;
			checkRequireReset.Checked=true;
			checkRequireReset.Enabled=false;
			butUnlock.Visible=false;
		}
		textBadgeId.Text=UserodCur.BadgeId;
	}

	///<summary>Refreshes the security tree in the "Users" tab.</summary>
	private void RefreshUserTree() {
		securityTreeUser.FillForUserGroup(listUserGroup.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).ToList());
	}

	private void listUserGroup_SelectedIndexChanged(object sender,EventArgs e) {
		if(_isFillingList) {
			return;
		}
		RefreshUserTree();
	}

	private void butPickDomainUser_Click(object sender,EventArgs e) {
		//DirectoryEntry does recognize an empty string as a valid LDAP entry and will just return all logins from all available domains
		//But all logins should be on the same domain, so this field is required
		if(string.IsNullOrWhiteSpace(PrefC.GetString(PrefName.DomainLoginPath))) {
			MsgBox.Show(this,"DomainLoginPath is missing in security settings. DomainLoginPath is required before assigning domain logins to user accounts.");
			return;
		}
		//Try to access the specified DomainLoginPath
		try {
			DirectoryEntry.Exists(PrefC.GetString(PrefName.DomainLoginPath));
		}
		catch(Exception ex) {
			ODMessageBox.Show(Lan.g(this,"An error occurred while attempting to access the provided DomainLoginPath:")+" "+ex.Message);
			return;
		}
		using var formDomainUserPick=new FormDomainUserPick();
		formDomainUserPick.ShowDialog();
		if(formDomainUserPick.DialogResult==DialogResult.OK && formDomainUserPick.SelectedDomainName!=null) { //only check for null, as empty string should clear the field
			UserodCur.DomainUser=$@"{PrefC.GetString(PrefName.DomainObjectGuid)}\{formDomainUserPick.SelectedDomainName}";
			textDomainUser.Text=formDomainUserPick.SelectedDomainName;
		}
	}

	private void listClinic_MouseClick(object sender,MouseEventArgs e) {
		var idx=listClinic.IndexFromPoint(e.Location);
		if(idx==-1){
			return;
		}
		if(idx==0){//all
			listClinicMulti.Enabled=false;
			listClinicMulti.SetAll(false);
		}
		else{
			listClinicMulti.Enabled=true;
		}
	}

	private void butPassword_Click(object sender, System.EventArgs e) {
		var isCreate=string.IsNullOrEmpty(UserodCur.PasswordHash);
		using var formUserPassword=new FormUserPassword(isCreate,UserodCur.UserName);
		formUserPassword.IsInSecurityWindow=true;
		formUserPassword.ShowDialog();
		if(formUserPassword.DialogResult==DialogResult.Cancel){
			return;
		}
		UserodCur.SetPassword(formUserPassword.PasswordContainer_);
		UserodCur.PasswordIsStrong=formUserPassword.IsPasswordStrong;
		_passwordTyped=formUserPassword.PasswordTyped;
		if(string.IsNullOrEmpty(UserodCur.PasswordHash)) {
			butPassword.Text=Lan.g(this,"Create Password");
		}
		else{
			butPassword.Text=Lan.g(this,"Change Password");
		}
	}

	private void butUnlock_Click(object sender,EventArgs e) {
		if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Users can become locked when invalid credentials have been entered several times in a row.\r\n"
		                                         +"Unlock this user so that more log in attempts can be made?"))
		{
			return;
		}
		UserodCur.DateTFail=DateTime.MinValue;
		UserodCur.FailedAttempts=0;
		try {
			Userods.Update(UserodCur);
			MsgBox.Show(this,"User has been unlocked.");
		}
		catch(Exception) {
			MsgBox.Show(this,"There was a problem unlocking this user.  Please call support or wait the allotted lock time.");
		}
	}

	private bool IsValidLogOffMinutes() {
		if(!(textLogOffAfterMinutes.Text=="") && (!int.TryParse(textLogOffAfterMinutes.Text,out var minutes) || minutes<0)) {
			MsgBox.Show(this,"Invalid 'Automatic logoff time in minutes'.\r\n" +
			                 "Must be blank, 0, or a positive integer.");
			return false;
		}
		return true;
	}

	private bool SaveLogOffPreferences() {
		var isCacheInvalid=false;
		if(textLogOffAfterMinutes.Text.IsNullOrEmpty() && !_logOffAfterMinutesInitialValue.IsNullOrEmpty()) {
			UserOdPrefs.Delete(_userOdPrefLogOffAfterMinutes.UserOdPrefNum);
			isCacheInvalid=true;
		}
		else if(textLogOffAfterMinutes.Text!=_logOffAfterMinutesInitialValue) { //Only do this if the value has changed
			if(_userOdPrefLogOffAfterMinutes==null) {
				_userOdPrefLogOffAfterMinutes=new UserOdPref { Fkey=0, FkeyType=UserOdFkeyType.LogOffTimerOverride, UserNum=UserodCur.UserNum };
			}
			_userOdPrefLogOffAfterMinutes.ValueString=textLogOffAfterMinutes.Text;
			UserOdPrefs.Upsert(_userOdPrefLogOffAfterMinutes);
			isCacheInvalid=true;
			if(!PrefC.GetBool(PrefName.SecurityLogOffAllowUserOverride)) {
				MsgBox.Show(this,"User logoff overrides will not take effect until the Global Security setting \"Allow user override for automatic logoff\" is checked");
			}
		}
		return isCacheInvalid;
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		if(textUserName.Text==""){
			MsgBox.Show(this,"Please enter a username.");
			return;
		}
		if(IsNew && textUserName.Text!=textUserName.Text.TrimEnd()) {
			MsgBox.Show(this,"User Name cannot end with white space.");
			return;
		}
		if(!_isFromAddUser && IsNew && PrefC.GetBool(PrefName.PasswordsMustBeStrong) && string.IsNullOrWhiteSpace(_passwordTyped)) {
			MsgBox.Show(this,"Password may not be blank when the strong password feature is turned on.");
			return;
		}
		if(true && listClinic.SelectedIndex==-1) {
			MsgBox.Show(this,"This user does not have a User Default Clinic set.  Please choose one to continue.");
			return;
		}
		if(listUserGroup.SelectedIndices.Count == 0) {
			MsgBox.Show(this,"Users must have at least one user group associated. Please select a user group to continue.");
			return;
		}
		if(_isFromAddUser && !Security.IsAuthorized(EnumPermType.SecurityAdmin,true)) {
			if(listUserGroup.SelectedIndices.Count!=1
			   || !listUserGroup.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).Contains(PrefC.GetLong(PrefName.DefaultUserGroup))) 
			{
				MsgBox.Show(this,"This user must be assigned to the default user group.");
				for(var i=0;i<listUserGroup.Items.Count;i++) {
					if(((UserGroup)listUserGroup.Items.GetObjectAt(i)).UserGroupNum==PrefC.GetLong(PrefName.DefaultUserGroup)) {
						listUserGroup.SetSelected(i);
					}
					else {
						listUserGroup.SetSelected(i,false);
					}
				}
				return;
			}
		}
		if(!IsValidLogOffMinutes()) {
			return;
		}
		var listUserClinics=new List<UserClinic>();
		if(true) {//Check to see if users have restricted clinics set.
			for(var i=0;i<listClinicMulti.SelectedIndices.Count;i++) {
				listUserClinics.Add(new UserClinic(_listClinics[listClinicMulti.SelectedIndices[i]].Id,UserodCur.UserNum));
			}
			//If they set the user up with a default clinic and it's not in the restricted list, return.
			if(listUserClinics.Count>0 && !listUserClinics.Exists(x => x.ClinicNum==_listClinics[listClinic.SelectedIndex-1].Id)) {
				MsgBox.Show(this,"User cannot have a default clinic that they are not restricted to.");
				return;
			}
		}
		if(!true || listClinic.SelectedIndex==0) {
			UserodCur.ClinicNum=0;
		}
		else {
			UserodCur.ClinicNum=_listClinics[listClinic.SelectedIndex-1].Id;
		}
		UserodCur.ClinicIsRestricted=false;//This is kept in sync with their choice of "All".
		if(listClinicMulti.SelectedIndices.Count>0) {
			UserodCur.ClinicIsRestricted=true;
		}
		UserodCur.IsHidden=checkIsHidden.Checked;
		UserodCur.IsPasswordResetRequired=checkRequireReset.Checked;
		UserodCur.UserName=textUserName.Text;
		if(listEmployee.SelectedIndex==0){
			UserodCur.EmployeeNum=0;
		}
		else{
			UserodCur.EmployeeNum=_listEmployees[listEmployee.SelectedIndex-1].EmployeeNum;
		}
		if(listProv.SelectedIndex==0) {
			var provider=Providers.GetProv(UserodCur.ProvNum);
			if(provider!=null) {
				provider.IsInstructor=false;//If there are more than 1 users associated to this provider, they will no longer be an instructor.
				Providers.Update(provider);	
			}
			UserodCur.ProvNum=0;
		}
		else {
			var provider=Providers.GetProv(UserodCur.ProvNum);
			if(provider!=null) {
				if(provider.ProvNum!=_listProviders[listProv.SelectedIndex-1].ProvNum) {
					provider.IsInstructor=false;//If there are more than 1 users associated to this provider, they will no longer be an instructor.
				}
				Providers.Update(provider);
			}
			UserodCur.ProvNum=_listProviders[listProv.SelectedIndex-1].ProvNum;
		}
		UserodCur.BadgeId=textBadgeId.Text;
		if(IsNew) {
			try {
				Userods.Insert(UserodCur,listUserGroup.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).ToList());
			}
			catch (Exception ex) {
				ODMessageBox.Show(ex.Message);
				return;
			}
			for(var i = 0;i<listUserClinics.Count;i++) {
				//Set the user clinic's UserNum to the one we just inserted.
				listUserClinics[i].UserNum=UserodCur.UserNum;
			}
			SecurityLogs.MakeLogEntry(EnumPermType.AddNewUser,0,"New user '"+UserodCur.UserName+"' added");
		}
		else{
			var listUserGroupsNew=listUserGroup.GetListSelected<UserGroup>();
			var listUserGroupsOld=UserodCur.GetGroups();
			try {
				Userods.Update(UserodCur,listUserGroupsNew.Select(x => x.UserGroupNum).ToList());
			}
			catch (Exception ex) {
				ODMessageBox.Show(ex.Message);
				return;
			}
			//if this is the current user, update the user, credentials, etc.
			if(UserodCur.UserNum==Security.CurUser.UserNum) {
				Security.CurUser=UserodCur.Copy();
				if(_passwordTyped!=null) {
					Security.PasswordTyped=_passwordTyped; //update the password typed for middle tier refresh
				}
			}
			//Log changes to the User's UserGroups.
			Func<List<UserGroup>,List<UserGroup>,List<UserGroup>> funcGetMissing=(listUserGroups1,listUserGroups2) => {
				var listUserGroupsRet=new List<UserGroup>();
				for(var i = 0;i<listUserGroups1.Count;i++) {
					if(listUserGroups2.Exists(x => x.UserGroupNum==listUserGroups1[i].UserGroupNum)) {
						continue;
					}
					listUserGroupsRet.Add(listUserGroups1[i]);
				}
				return listUserGroupsRet;
			};
			var listUserGroupsRemoved=funcGetMissing(listUserGroupsOld,listUserGroupsNew);
			var listUserGroupsAdded=funcGetMissing(listUserGroupsNew,listUserGroupsOld);
			if(listUserGroupsRemoved.Count>0) {//Only log if there are items in the list
				SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin,0,"User "+UserodCur.UserName+
				                                                       " removed from User group(s): "+string.Join(", ",listUserGroupsRemoved.Select(x => x.Description).ToArray())+" by: "+Security.CurUser.UserName);
			}
			if(listUserGroupsAdded.Count>0) {//Only log if there are items in the list.
				SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin,0,"User "+UserodCur.UserName+
				                                                       " added to User group(s): "+string.Join(", ",listUserGroupsAdded.Select(x => x.Description).ToArray())+" by: "+Security.CurUser.UserName);
			}
		}
		if(UserClinics.Sync(listUserClinics,UserodCur.UserNum)) {//Either syncs new list, or clears old list if no longer restricted.
			DataValid.SetInvalid(InvalidType.UserClinics);
		}
		var isUserOdPrefCacheInvalid=false;
		//Get eRx prefs for all other users. The same user can use the same ID at multiple clinics so we don't want to compare against the prefs of the currently selected user.
		var listOtherUserOdPrefs=UserOdPrefs.GetByFkeyAndFkeyType(Programs.GetCur(ProgramName.eRx).ProgramNum,UserOdFkeyType.Program).FindAll(x => x.UserNum!=UserodCur.UserNum);
		//This list is filled on load with all of the prefs for the current user and contains any changes made in FormUserPrefAdditional.
		DataValid.SetInvalid(InvalidType.Security);
		//List of AlertTypes that are selected.
		var listAlertCatagoryNumsUser=new List<long>();
		for(var i=0;i<listAlertSubMulti.SelectedIndices.Count;i++) {
			listAlertCatagoryNumsUser.Add(_listAlertCategories[listAlertSubMulti.SelectedIndices[i]].AlertCategoryNum);
		}
		var listClinicNums=new List<long>();
		for(var i=0;i<listAlertSubsClinicsMulti.SelectedIndices.Count;i++) {
			if(listAlertSubsClinicsMulti.SelectedIndices[i]==0) {//All
				listClinicNums.Add(-1);//Add All
				break;
			}
			if(listAlertSubsClinicsMulti.SelectedIndices[i]==1) {//HQ
				listClinicNums.Add(0);
				continue;
			}
			var clinic=_listClinics[listAlertSubsClinicsMulti.SelectedIndices[i]-2];//Subtract 2 for 'All' and 'HQ'
			listClinicNums.Add(clinic.Id);
		}
		var _listAlertSubsUserTypesNew=_listAlertSubsUserTypesOld.Select(x => x.Copy()).ToList();
		//Remove AlertTypes that have been deselected through either deslecting the type or clinic.
		_listAlertSubsUserTypesNew.RemoveAll(x => !listAlertCatagoryNumsUser.Contains(x.AlertCategoryNum));
		if(true) {
			_listAlertSubsUserTypesNew.RemoveAll(x => !listClinicNums.Contains(x.ClinicNum));
		}
		for(var i = 0;i<listAlertCatagoryNumsUser.Count;i++) {
			if(!true) {
				if(!_listAlertSubsUserTypesOld.Exists(x => x.AlertCategoryNum==listAlertCatagoryNumsUser[i])) {//Was not subscribed to type.
					_listAlertSubsUserTypesNew.Add(new AlertSub(UserodCur.UserNum,0,listAlertCatagoryNumsUser[i]));
				}
				continue;
			}
			//Clinics enabled.
			for(var j = 0;j<listClinicNums.Count;j++) {
				if(!_listAlertSubsUserTypesOld.Exists(x => x.ClinicNum==listClinicNums[j] && x.AlertCategoryNum==listAlertCatagoryNumsUser[i])) {//Was not subscribed to type.
					_listAlertSubsUserTypesNew.Add(new AlertSub(UserodCur.UserNum,listClinicNums[j],listAlertCatagoryNumsUser[i]));
					continue;
				}
			}
		}
		isUserOdPrefCacheInvalid|=SaveLogOffPreferences();
		AlertSubs.Sync(_listAlertSubsUserTypesNew,_listAlertSubsUserTypesOld);
		if(isUserOdPrefCacheInvalid) {
			DataValid.SetInvalid(InvalidType.UserOdPrefs);
		}
		DialogResult=DialogResult.OK;
	}

}