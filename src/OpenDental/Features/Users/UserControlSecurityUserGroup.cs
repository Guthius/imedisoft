using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.ComponentModel;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;

namespace OpenDental;

public partial class UserControlSecurityUserGroup : UserControl
{
    private bool _isFillingList;
    private Dictionary<long, ProviderDto> _dictProvNumProvs;
    private Userod _selectedUser;
    private ContextMenu _contextMenuUsers;
    
    public delegate void SecurityTabsEventHandler(object sender, SecurityEventArgs e);

    public delegate DialogResult SecurityTreeEventHandler(object sender, SecurityEventArgs e);

    [Category("Security Events"), Description("Occurs when the Add User button is clicked.")]
    public event SecurityTabsEventHandler AddUserClick;

    [Category("Security Events"), Description("Occurs when the Copy User button is clicked.")]
    public event SecurityTabsEventHandler CopyUserClick;

    [Category("Security Events"), Description("Occurs when the Edit User button is clicked.")]
    public event SecurityTabsEventHandler EditUserClick;

    [Category("Security Events"), Description("Occurs when the Add User Group button is clicked.")]
    public event SecurityTabsEventHandler AddUserGroupClick;

    [Category("Security Events"), Description("Occurs when the Edit User Group button is clicked.")]
    public event SecurityTabsEventHandler EditUserGroupClick;

    [Category("Security Events"), Description("Occurs when the Report Permission is checked.")]
    public event SecurityTreeEventHandler ReportPermissionChecked;

    [Category("Security Events"), Description("Occurs when a date-editable Group Permission is checked.")]
    public event SecurityTreeEventHandler GroupPermissionChecked;

    [Category("Security Events"), Description("Occurs when the AdjustmentTypeDeny permission is checked.")]
    public event SecurityTreeEventHandler AdjustmentTypeDenyPermissionChecked;
    
    public Userod SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            gridUsers.SetAll(false);
            butCopyUser.Enabled = _selectedUser != null;
            if (_selectedUser == null)
            {
                labelUserCurr.Text = "No User Selected";
                return;
            }

            labelUserCurr.Text = _selectedUser.UserName;
            
            for (var i = 0; i < gridUsers.ListGridRows.Count; i++)
            {
                if (((Userod) gridUsers.ListGridRows[i].Tag).UserNum == _selectedUser.UserNum)
                {
                    gridUsers.SetSelected(i);
                    break;
                }
            }
        }
    }

    public UserGroup SelectedUserGroup
    {
        get => listUserGroupTabUserGroups.GetSelected<UserGroup>();
        set
        {
            if (value == null)
            {
                return;
            }

            listUserGroupTabUserGroups.SetSelectedKey<UserGroup>(value.UserGroupNum, x => x.UserGroupNum);
        }
    }
    
    public UserControlSecurityUserGroup()
    {
        InitializeComponent();
    }

    private void UserControlUserGroupSecurity_Load(object sender, EventArgs e)
    {
        if (CopyUserClick is not null)
        {
            _contextMenuUsers.MenuItems.Add("Copy User", ButtonCopyUser_Click);
            gridUsers.ContextMenu = _contextMenuUsers;
            butCopyUser.Visible = true;
        }

        if (DesignMode)
        {
            return;
        }

        securityTreeUser.FillTreePermissionsInitial();

        FillFilters();
        FillGridUsers();

        RefreshUserTabGroups();

        userControlSecurityTree.FillTreePermissionsInitial();
        FillListUserGroupTabUserGroups();
        userControlSecurityTree.FillForUserGroup(SelectedUserGroup.UserGroupNum);
        FillAssociatedUsers();
    }
    
    private void FillFilters()
    {
        foreach (UserFilters filterCur in Enum.GetValues(typeof(UserFilters)))
        {
            comboShowOnly.Items.Add(Lan.g(this, filterCur.GetDescription()), filterCur);
        }

        comboShowOnly.SelectedIndex = 0;
        
        labelClinic.Visible = true;
        
        comboClinic.Visible = true;
        comboClinic.Items.Clear();
        comboClinic.Items.Add("All Clinics");
        comboClinic.SelectedIndex = 0;
        foreach (var clinicCur in Clinics.GetDeepCopy(true))
        {
            comboClinic.Items.Add(clinicCur.Abbr, clinicCur);
        }

        comboGroups.Items.Clear();
        comboGroups.Items.Add("All Groups");

        comboGroups.SelectedIndex = 0;
        foreach (var groupCur in UserGroups.GetList())
        {
            comboGroups.Items.Add(groupCur.Description, groupCur);
        }
    }

    private List<Userod> GetFilteredUsersHelper()
    {
        var users = Userods.GetDeepCopy();

        _dictProvNumProvs ??= Providers.GetManyByIdNoCache(Userods.GetDeepCopy().Select(x => x.ProvNum).ToList()).ToDictionary(x => x.Id, x => x);

        if (!checkShowHidden.Checked)
        {
            users.RemoveAll(x => x.IsHidden);
        }

        long classNum = 0;
        switch (comboShowOnly.GetSelected<UserFilters>())
        {
            case UserFilters.Employees:
                users.RemoveAll(x => x.EmployeeNum == 0);
                break;

            case UserFilters.Providers:
                users.RemoveAll(x => x.ProvNum == 0);
                break;

            case UserFilters.Other:
                users.RemoveAll(x => x.EmployeeNum != 0 || x.ProvNum != 0);
                break;

            case UserFilters.AllUsers:
            default:
                break;
        }

        if (comboClinic.SelectedIndex > 0)
        {
            users.RemoveAll(x => x.ClinicNum != comboClinic.GetSelected<ClinicDto>().Id);
        }

        if (comboGroups.SelectedIndex > 0)
        {
            users.RemoveAll(x => !x.IsInUserGroup(comboGroups.GetSelected<UserGroup>().UserGroupNum));
        }

        if (!string.IsNullOrWhiteSpace(textPowerSearch.Text))
        {
            switch (comboShowOnly.GetSelected<UserFilters>())
            {
                case UserFilters.Employees:
                    users.RemoveAll(x => !Employees.GetName(x.EmployeeNum).ToLower().Contains(textPowerSearch.Text.ToLower()));
                    break;

                case UserFilters.Providers:
                    users.RemoveAll(x => !_dictProvNumProvs[x.ProvNum].Description.ToLower().Contains(textPowerSearch.Text.ToLower()));
                    break;

                case UserFilters.AllUsers:
                case UserFilters.Other:
                default:
                    users.RemoveAll(x => !x.UserName.ToLower().Contains(textPowerSearch.Text.ToLower()));
                    break;
            }
        }

        return users;
    }

    ///<summary>Refreshes the security tree in the "Users" tab.</summary>
    private void RefreshUserTree()
    {
        securityTreeUser.FillForUserGroup(listUserTabUserGroups.GetListSelected<UserGroup>().Select(x => x.UserGroupNum).ToList());
    }

    ///<summary>Refreshes the UserGroups list box on the "User" tab. Also refreshes the security tree. 
    ///Public so that it can be called from the Form that implements this control.</summary>
    public void RefreshUserTabGroups()
    {
        var listUserGroups = SelectedUser == null ? UserGroups.GetList() : SelectedUser.GetGroups();
        _isFillingList = true;
        listUserTabUserGroups.Items.Clear();
        for (var i = 0; i < listUserGroups.Count; i++)
        {
            listUserTabUserGroups.Items.Add(listUserGroups[i].Description, listUserGroups[i]);
            if (SelectedUser != null)
            {
                listUserTabUserGroups.SetSelected(i);
            }
        }

        _isFillingList = false;
        //RefreshTree takes a while (it has to draw many images) so this is to show the usergroup selections before loading the tree.
        Application.DoEvents();
        RefreshUserTree();
    }

    private void listUserTabUserGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isFillingList)
        {
            return;
        }

        RefreshUserTree();
    }

    private void comboShowOnly_SelectionIndexChanged(object sender, EventArgs e)
    {
        labelFilterType.Text = comboShowOnly.GetSelected<UserFilters>() switch
        {
            UserFilters.Employees => "Employee Name",
            UserFilters.Providers => "Provider Name",
            _ => "Username"
        };

        textPowerSearch.Text = string.Empty;
        FillGridUsers();
    }

    private void Filter_Changed(object sender, EventArgs e)
    {
        FillGridUsers();
    }

    public void FillGridUsers()
    {
        _isFillingList = true;
        
        var selectedUser = SelectedUser;
        
        gridUsers.BeginUpdate();
        
        gridUsers.Columns.Clear();
        gridUsers.Columns.Add(new GridColumn("Username", 90));
        gridUsers.Columns.Add(new GridColumn("Employee", 90));
        gridUsers.Columns.Add(new GridColumn("Provider", 90));
        gridUsers.Columns.Add(new GridColumn("Clinic", 80));
        gridUsers.Columns.Add(new GridColumn("Clinic\r\nRestr", 38, HorizontalAlignment.Center));
        gridUsers.Columns.Add(new GridColumn("Strong\r\nPwd", 45, HorizontalAlignment.Center));
        
        gridUsers.ListGridRows.Clear();
        
        var filteredUsers = GetFilteredUsersHelper();
        
        foreach (var user in filteredUsers)
        {
            var gridRow = new GridRow();
            
            gridRow.Cells.Add(user.UserName);
            gridRow.Cells.Add(Employees.GetName(user.EmployeeNum));
            gridRow.Cells.Add(Providers.GetLongDesc(user.ProvNum));
            gridRow.Cells.Add(Clinics.GetAbbr(user.ClinicNum));
            gridRow.Cells.Add(user.ClinicIsRestricted ? "X" : "");
            gridRow.Cells.Add(user.PasswordIsStrong ? "X" : "");
            gridRow.Tag = user;
            
            gridUsers.ListGridRows.Add(gridRow);
        }

        gridUsers.EndUpdate();
        
        _isFillingList = false;
        
        if (selectedUser is not null)
        {
            SelectedUser = filteredUsers.FirstOrDefault(x => x.UserNum == selectedUser.UserNum);
        }

        RefreshUserTabGroups();
    }

    private void ButtonAddUser_Click(object sender, EventArgs e)
    {
        if (_isFillingList)
        {
            return;
        }

        AddUserClick?.Invoke(this, new SecurityEventArgs(new Userod()));
    }

    private void ButtonCopyUser_Click(object sender, EventArgs e)
    {
        if (_isFillingList)
        {
            return;
        }

        CopyUserClick?.Invoke(sender, new SecurityEventArgs(SelectedUser));
    }

    private void gridUsers_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (_isFillingList)
        {
            return;
        }

        SelectedUser = gridUsers.SelectedTag<Userod>();
        
        RefreshUserTabGroups();
    }

    private void gridUsers_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (_isFillingList || SelectedUser == null)
        {
            return;
        }

        EditUserClick?.Invoke(this, new SecurityEventArgs(SelectedUser));
    }
    
    public void FillListUserGroupTabUserGroups()
    {
        _isFillingList = true;
        
        var selectedGroup = SelectedUserGroup;
        
        listUserGroupTabUserGroups.Items.Clear();
        
        foreach (var userGroup in UserGroups.GetList())
        {
            listUserGroupTabUserGroups.Items.Add(userGroup.Description, userGroup);
            
            if (selectedGroup != null && userGroup.UserGroupNum == selectedGroup.UserGroupNum)
            {
                listUserGroupTabUserGroups.SelectedItem = userGroup;
            }
        }

        _isFillingList = false;
        
        if (listUserGroupTabUserGroups.SelectedItem == null)
        {
            listUserGroupTabUserGroups.SetSelected(0);
        }
    }
    
    private void FillAssociatedUsers()
    {
        listAssociatedUsers.Items.Clear();
        
        var users = Userods.GetForGroup(SelectedUserGroup.UserGroupNum);
        
        foreach (var user in users)
        {
            listAssociatedUsers.Items.Add(user.UserName, user);
        }

        if (listAssociatedUsers.Items.Count == 0)
        {
            listAssociatedUsers.Items.Add("None");
        }
    }

    private void listUserGroupTabUserGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isFillingList || listUserGroupTabUserGroups.SelectedItem is null)
        {
            return;
        }

        userControlSecurityTree.FillForUserGroup(listUserGroupTabUserGroups.GetSelected<UserGroup>().UserGroupNum);
        
        FillAssociatedUsers();
    }

    private void ButtonAddGroup_Click(object sender, EventArgs e)
    {
        AddUserGroupClick?.Invoke(this, new SecurityEventArgs(new UserGroup()));
    }

    private void ButtonEditGroup_Click(object sender, EventArgs e)
    {
        if (listUserGroupTabUserGroups.SelectedIndex == -1)
        {
            MsgBox.Show(this, "Please select a User Group to edit.");
            return;
        }

        EditUserGroupClick?.Invoke(this, new SecurityEventArgs(SelectedUserGroup));
    }

    private void listUserGroupTabUserGroups_DoubleClick(object sender, EventArgs e)
    {
        EditUserGroupClick?.Invoke(this, new SecurityEventArgs(SelectedUserGroup));
    }

    private void butCollapseAll_Click(object sender, EventArgs e)
    {
        userControlSecurityTree.CollapseAll();
    }

    private void butExpandAll_Click(object sender, EventArgs e)
    {
        userControlSecurityTree.ExpandAll();
    }

    private void butSetAll_Click(object sender, EventArgs e)
    {
        userControlSecurityTree.SetAll();
    }

    private void butSetNone_Click(object sender, EventArgs e)
    {
        userControlSecurityTree.SetNone();
    }
    
    private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        if (tabControlMain.SelectedTab == tabPageUsers)
        {
            FillGridUsers();
            
            RefreshUserTabGroups();
        }
        else if (tabControlMain.SelectedTab == tabPageUserGroups)
        {
            FillAssociatedUsers();
        }
    }

    private DialogResult securityTreeUserGroup_ReportPermissionChecked(object sender, SecurityEventArgs e)
    {
        return ReportPermissionChecked?.Invoke(sender, e) ?? DialogResult.Cancel;
    }

    private enum UserFilters
    {
        [Description("All Users")]
        AllUsers = 0,
        Providers,
        Employees,
        Other,
    }

    private DialogResult securityTreeUserGroup_GroupPermissionChecked(object sender, SecurityEventArgs e)
    {
        return GroupPermissionChecked?.Invoke(sender, e) ?? DialogResult.Cancel;
    }

    private DialogResult securityTreeUserGroup_AdjustmentTypeDenyPermissionChecked(object sender, SecurityEventArgs e)
    {
        return AdjustmentTypeDenyPermissionChecked?.Invoke(sender, e) ?? DialogResult.Cancel;
    }

    private void securityTreeUser_LocationChanged(object sender, EventArgs e)
    {
    }
}

public class SecurityEventArgs
{
    public Userod User { get; }
    public UserGroup Group { get; }
    public GroupPermission Perm { get; }

    public SecurityEventArgs(Userod user)
    {
        User = user;
    }

    public SecurityEventArgs(UserGroup userGroup)
    {
        Group = userGroup;
    }

    public SecurityEventArgs(GroupPermission perm)
    {
        Perm = perm;
    }
}