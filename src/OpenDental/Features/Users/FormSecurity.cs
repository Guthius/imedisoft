using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormSecurity : FormODBase
{
    public FormSecurity()
    {
        InitializeComponent();
    }

    private void FormSecurityEdit_Load(object sender, EventArgs e)
    {
        LayoutMenu();
        
        userControlSecurityUserGroup.Height = ClientSize.Height - userControlSecurityUserGroup.Top - 5;
    }

    private void LayoutMenu()
    {
        menuMain.BeginUpdate();
        menuMain.Add(new MenuItemOD("Global Security Settings", globalSecuritySettingsToolStripMenuItem_Click));
        menuMain.EndUpdate();
    }

    private void globalSecuritySettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var formGlobalSecurity = new FormGlobalSecurity();
        
        formGlobalSecurity.ShowDialog();
    }

    private void userControlSecurityTabs_AddUserClick(object sender, SecurityEventArgs e)
    {
        var userod = new Userod();
        
        using var formUserEdit = new FormUserEdit(userod);
        
        formUserEdit.IsNew = true;
        
        if (formUserEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        userControlSecurityUserGroup.FillGridUsers();
        userControlSecurityUserGroup.SelectedUser = formUserEdit.UserodCur;
        userControlSecurityUserGroup.RefreshUserTabGroups();
    }

    private void UserControlSecurityTabs_CopyUserClick(object sender, SecurityEventArgs e)
    {
        var user = e.User;
        if (user is null)
        {
            ShowError("Please select a user.");
            return;
        }

        if (!Userods.TryGetUniqueUsername(user.UserName + "(Copy)", 0, false, out var newUserName))
        {
            ShowError("Could not generate a unique username.");
            return;
        }
        
        using var formUserPassword = new FormUserPassword(false, newUserName, isCopiedUser: true);
        
        formUserPassword.IsInSecurityWindow = true;
        
        if (formUserPassword.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var userodNew = Userods.CopyUser(user, formUserPassword.Password, formUserPassword.IsPasswordStrong, newUserName);
        
        DataValid.SetInvalid(InvalidType.Security, InvalidType.UserClinics);
        
        userControlSecurityUserGroup.FillGridUsers();
        userControlSecurityUserGroup.SelectedUser = userodNew;
        userControlSecurityUserGroup.RefreshUserTabGroups();
    }

    private void userControlSecurityTabs_EditUserClick(object sender, SecurityEventArgs e)
    {
        using var formUserEdit = new FormUserEdit(e.User);

        if (formUserEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        userControlSecurityUserGroup.FillGridUsers();
        userControlSecurityUserGroup.RefreshUserTabGroups();
    }

    private void userControlSecurityTabs_AddUserGroupClick(object sender, SecurityEventArgs e)
    {
        var userGroup = new UserGroup();
        
        var frmUserGroupEdit = new FrmUserGroupEdit(userGroup)
        {
            IsNew = true
        };
        
        frmUserGroupEdit.ShowDialog();

        if (!frmUserGroupEdit.IsDialogOK)
        {
            return;
        }
        
        userControlSecurityUserGroup.FillListUserGroupTabUserGroups();
        userControlSecurityUserGroup.SelectedUserGroup = userGroup;
    }

    private void userControlSecurityTabs_EditUserGroupClick(object sender, SecurityEventArgs e)
    {
        var frmUserGroupEdit = new FrmUserGroupEdit(e.Group);
        
        frmUserGroupEdit.ShowDialog();
        
        if (frmUserGroupEdit.IsDialogOK)
        {
            userControlSecurityUserGroup.FillListUserGroupTabUserGroups();
        }
    }

    private DialogResult userControlSecurityTabs_ReportPermissionChecked(object sender, SecurityEventArgs e)
    {
        var groupPermission = e.Perm;
        
        using var formReportSetup = new FormReportSetup(groupPermission.UserGroupNum, true);
        
        formReportSetup.ShowDialog();
        
        return formReportSetup.DialogResult;
    }

    private DialogResult userControlSecurityTabs_GroupPermissionChecked(object sender, SecurityEventArgs e)
    {
        using var formGroupPermEdit = new FormGroupPermEdit(e.Perm);
        
        formGroupPermEdit.ShowDialog();
        
        return formGroupPermEdit.DialogResult;
    }

    private DialogResult userControlSecurityTabs_AdjustmentTypeDenyPermissionChecked(object sender, SecurityEventArgs e)
    {
        var listGroupPermissionsOld = GroupPermissions.GetAdjustmentTypeDenyPermsForUserGroup(e.Perm.UserGroupNum);
        var listDefsAll = Defs.GetDefsForCategory(DefCat.AdjTypes);
        
        var listDefs = Defs.GetDefs(DefCat.AdjTypes, listGroupPermissionsOld.Select(x => x.FKey).ToList());
        if (listGroupPermissionsOld.Any(x => x.FKey == 0))
        {
            listDefs = listDefsAll.Select(x => x.Copy()).ToList();
        }

        using var formDefinitionPicker = new FormDefinitionPicker(DefCat.AdjTypes, listDefs);
        
        formDefinitionPicker.IsMultiSelectionMode = true;
        formDefinitionPicker.HasShowHiddenOption = true;
        
        if (formDefinitionPicker.ShowDialog() != DialogResult.OK)
        {
            return DialogResult.Cancel;
        }
        
        var listGroupPermissionsNew = new List<GroupPermission>();
        
        GroupPermission groupPermission;
        
        var listDefsSelected = formDefinitionPicker.SelectedDefs;
        if (listDefsSelected.Count == listDefsAll.Count)
        {
            groupPermission = new GroupPermission
            {
                UserGroupNum = e.Perm.UserGroupNum,
                PermType = EnumPermType.AdjustmentTypeDeny,
                FKey = 0
            };
            
            listGroupPermissionsNew.Add(groupPermission);
            
            GroupPermissions.Sync(listGroupPermissionsNew, listGroupPermissionsOld);
            
            return DialogResult.OK;
        }

        for (var i = 0; i < listDefsSelected.Count; i++)
        {
            groupPermission = listGroupPermissionsOld.Find(x => x.FKey == listDefsSelected[i].DefNum);
            
            if (groupPermission is null)
            {
                groupPermission = new GroupPermission
                {
                    UserGroupNum = e.Perm.UserGroupNum,
                    PermType = EnumPermType.AdjustmentTypeDeny,
                    FKey = listDefsSelected[i].DefNum
                };
            }

            listGroupPermissionsNew.Add(groupPermission);
        }

        GroupPermissions.Sync(listGroupPermissionsNew, listGroupPermissionsOld);
        return DialogResult.OK;
    }

    private void FormSecurityEdit_FormClosing(object sender, FormClosingEventArgs e)
    {
        DataValid.SetInvalid(InvalidType.Security);
    }
}