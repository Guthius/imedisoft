using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class GroupPermissions
{
	/// <summary>
	///     The maximum number of days allowed for the NewerDays column.
	///     Setting a NewerDays to a value higher than this will cause an exception to be thrown in the program.
	///     There is a DBM that will correct invalid NewerDays in the database.
	/// </summary>
	public const double NewerDaysMax = 3000;

    
    public static void Update(GroupPermission groupPermission)
    {
        if (groupPermission.NewerDate.Year > 1880 && groupPermission.NewerDays > 0) throw new Exception(Lans.g("GroupPermissions", "Date or days can be set, but not both."));
        if (!PermTakesDates(groupPermission.PermType))
            if (groupPermission.NewerDate.Year > 1880 || groupPermission.NewerDays > 0)
                throw new Exception(Lans.g("GroupPermissions", "This type of permission may not have a date or days set."));

        GroupPermissionCrud.Update(groupPermission);
    }

    ///<summary>Update that doesn't use the local cache or validation. Useful for multiple database connections.</summary>
    public static void UpdateNoCache(GroupPermission groupPermission)
    {
        GroupPermissionCrud.Update(groupPermission);
    }

    /// <summary>
    ///     Deletes GroupPermissions based on primary key.  Do not call this method unless you have checked specific
    ///     dependencies first.  E.g. after deleting this permission, there will still be a security admin user.  This method
    ///     is only called from the CEMT sync.  RemovePermission should probably be used instead.
    /// </summary>
    public static void Delete(GroupPermission groupPermission)
    {
        var command = "DELETE FROM grouppermission WHERE GroupPermNum = " + SOut.Long(groupPermission.GroupPermNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Deletes without using the cache. Cannot trust GroupPermNum when dealing with remote DB so we rely on every
    ///     other field to check.
    /// </summary>
    public static void DeleteNoCache(GroupPermission groupPermission)
    {
        var command = $@"DELETE FROM grouppermission 
				WHERE NewerDate={SOut.Date(groupPermission.NewerDate)} 
				AND NewerDays={SOut.Int(groupPermission.NewerDays)} 
				AND UserGroupNum={SOut.Long(groupPermission.UserGroupNum)} 
				AND PermType={SOut.Int((int) groupPermission.PermType)} 
				AND FKey={SOut.Long(groupPermission.FKey)}";
        Db.NonQ(command);
    }

    ///<summary>Delete all GroupPermissions for the specified PermType and UserGroupNum.</summary>
    public static void DeleteForPermTypeAndUserGroup(EnumPermType enumPermType, long userGroupNum)
    {
        var command = "DELETE FROM grouppermission WHERE PermType=" + SOut.Enum(enumPermType) + " AND UserGroupNum=" + SOut.Long(userGroupNum);
        Db.NonQ(command);
    }

    
    public static long Insert(GroupPermission groupPermission)
    {
        if (groupPermission.NewerDate.Year > 1880 && groupPermission.NewerDays > 0) throw new Exception(Lans.g("GroupPermissions", "Date or days can be set, but not both."));
        if (!PermTakesDates(groupPermission.PermType))
            if (groupPermission.NewerDate.Year > 1880 || groupPermission.NewerDays > 0)
                throw new Exception(Lans.g("GroupPermissions", "This type of permission may not have a date or days set."));

        if (groupPermission.PermType != EnumPermType.SecurityAdmin) return GroupPermissionCrud.Insert(groupPermission);
        //Make sure there are no hidden users in the group that is about to get the Security Admin permission.
        var command = "SELECT COUNT(*) FROM userod "
                      + "INNER JOIN usergroupattach ON usergroupattach.UserNum=userod.UserNum "
                      + "WHERE userod.IsHidden=1 "
                      + "AND usergroupattach.UserGroupNum=" + groupPermission.UserGroupNum;
        var count = SIn.Int(Db.GetCount(command));
        if (count != 0) //there are hidden users in this group
            throw new Exception(Lans.g("FormSecurity", "The Security Admin permission cannot be given to a user group with hidden users."));
        return GroupPermissionCrud.Insert(groupPermission);
    }

    ///<summary>Insertion logic that doesn't use the cache. Always ignores the PK and relies on auto-increment.</summary>
    public static long InsertNoCache(GroupPermission groupPermission)
    {
        var command = "INSERT INTO grouppermission (NewerDate,NewerDays,UserGroupNum,PermType,FKey) VALUES ("
                      + SOut.Date(groupPermission.NewerDate) + ","
                      + SOut.Int(groupPermission.NewerDays) + ","
                      + SOut.Long(groupPermission.UserGroupNum) + ","
                      + SOut.Int((int) groupPermission.PermType) + ","
                      + SOut.Long(groupPermission.FKey) + ")";
        return Db.GetLong(command);
    }

    
    public static void RemovePermission(long userGroupNum, EnumPermType enumPermType)
    {
        string command;
        if (enumPermType == EnumPermType.SecurityAdmin)
        {
            //need to make sure that at least one other user has this permission
            command = "SELECT COUNT(*) FROM (SELECT DISTINCT grouppermission.UserGroupNum "
                      + "FROM grouppermission "
                      + "INNER JOIN usergroupattach ON usergroupattach.UserGroupNum=grouppermission.UserGroupNum "
                      + "INNER JOIN userod ON userod.UserNum=usergroupattach.UserNum AND userod.IsHidden=0 "
                      + "WHERE grouppermission.PermType='" + SOut.Long((int) enumPermType) + "' "
                      + "AND grouppermission.UserGroupNum!=" + SOut.Long(userGroupNum) + ") t"; //This query is Oracle compatable
            if (DataCore.GetScalar(command) == "0") //no other users outside of this group have SecurityAdmin
                throw new Exception(Lans.g("FormSecurity", "There must always be at least one user in a user group that has the Security Admin permission."));
        }

        command = "DELETE FROM grouppermission WHERE UserGroupNum=" + SOut.Long(userGroupNum) + " "
                  + "AND PermType=" + SOut.Long((int) enumPermType);
        Db.NonQ(command);
    }

    public static bool Sync(List<GroupPermission> listGroupPermissionsNew, List<GroupPermission> listGroupPermissionsOld)
    {
        return GroupPermissionCrud.Sync(listGroupPermissionsNew, listGroupPermissionsOld);
    }

    /// <summary>
    ///     Gets a GroupPermission based on the supplied userGroupNum and permType.  If not found, then it returns null.
    ///     Used in FormSecurity when double clicking on a dated permission or when clicking the all button.
    /// </summary>
    public static GroupPermission GetPerm(long userGroupNum, EnumPermType enumPermType)
    {
        return GetFirstOrDefault(x => x.UserGroupNum == userGroupNum && x.PermType == enumPermType);
    }

    ///<summary>Gets a list of GroupPermissions for the supplied UserGroupNum.</summary>
    public static List<GroupPermission> GetPerms(long userGroupNum)
    {
        return GetWhere(x => x.UserGroupNum == userGroupNum);
    }

    /// <summary>
    ///     Gets a list of GroupPermissions for the supplied UserGroupNum without using the local cache.  Useful for
    ///     multithreaded connections.
    /// </summary>
    public static List<GroupPermission> GetPermsNoCache(long userGroupNum)
    {
        var listGroupPermissions = new List<GroupPermission>();
        var command = "SELECT * FROM grouppermission WHERE UserGroupNum=" + SOut.Long(userGroupNum);
        var tableGroupPerms = DataCore.GetTable(command);
        listGroupPermissions = GroupPermissionCrud.TableToList(tableGroupPerms);
        return listGroupPermissions;
    }

    ///<summary>Gets a list of GroupPermissions that are associated with reports. Uses Reports (22) permission.</summary>
    public static List<GroupPermission> GetPermsForReports(long userGroupNum = 0)
    {
        var listGroupPermissions = GetWhere(x => x.PermType == EnumPermType.Reports);
        if (userGroupNum > 0) listGroupPermissions.RemoveAll(x => x.UserGroupNum != userGroupNum);
        return listGroupPermissions;
    }

    /// <summary>
    ///     Gets a list of AdjustmentTypeDeny perms for a user group. Having an AdjustmentTypeDeny perm indicates the user
    ///     group does not have
    ///     permission to access (create,edit,edit zero) the adjustmenttype that has a defnum==fkey. Pattern approved by
    ///     Jordan.
    /// </summary>
    public static List<GroupPermission> GetAdjustmentTypeDenyPermsForUserGroup(long userGroupNum)
    {
        return GetWhere(x => x.PermType == EnumPermType.AdjustmentTypeDeny && x.UserGroupNum == userGroupNum);
    }

    /// <summary>
    ///     Gets a list of GroupPermissions that are associated with reports and the user groups that the passed in user.
    ///     Uses Reports (22) permission.
    /// </summary>
    public static List<GroupPermission> GetPermsForReports(Userod user)
    {
        return GetWhere(x => x.PermType == EnumPermType.Reports && user.IsInUserGroup(x.UserGroupNum));
    }

    /// <summary>
    ///     Used to check if user has permission to access the report. Pass in a list of DisplayReports to avoid a call to
    ///     the db.
    /// </summary>
    public static bool HasReportPermission(string reportName, Userod user, List<DisplayReport> listDisplayReports = null)
    {
        DisplayReport displayReport;
        if (listDisplayReports == null)
            displayReport = DisplayReports.GetAll(false).Find(x => x.InternalName == reportName);
        else
            displayReport = listDisplayReports.Find(x => x.InternalName == reportName);
        if (displayReport == null) //Report is probably hidden.
            return false;
        var listReportPermissions = GetPermsForReports(user);
        return listReportPermissions.Any(x => x.FKey.In(0, displayReport.DisplayReportNum)); //Zero FKey means access to every report.
    }

    ///<summary>Determines whether a single userGroup contains a specific permission.</summary>
    public static bool HasPermission(long userGroupNum, EnumPermType enumPermType, long fKey, List<GroupPermission> listGroupPermissions = null)
    {
        List<GroupPermission> listGroupPermissionsCopy;
        if (listGroupPermissions == null)
        {
            listGroupPermissionsCopy = GetWhere(x => x.UserGroupNum == userGroupNum && x.PermType == enumPermType);
        }
        else
        {
            listGroupPermissionsCopy = new List<GroupPermission>(listGroupPermissions);
            listGroupPermissionsCopy.RemoveAll(x => x.UserGroupNum != userGroupNum || x.PermType != enumPermType);
        }

        if (DoesPermissionTreatZeroFKeyAsAll(enumPermType) && listGroupPermissionsCopy.Any(x => x.FKey == 0)) //Access to everything.
            return true;
        return listGroupPermissionsCopy.Any(x => x.FKey == fKey);
    }

    ///<summary>Determines whether an individual user has a specific permission.</summary>
    public static bool HasPermission(Userod user, EnumPermType enumPermType, long fKey, List<GroupPermission> listGroupPermissions = null)
    {
        if (listGroupPermissions == null)
            listGroupPermissions = GetWhere(x => x.PermType == enumPermType && user.IsInUserGroup(x.UserGroupNum));
        else
            listGroupPermissions.RemoveAll(x => x.PermType != enumPermType && !user.IsInUserGroup(x.UserGroupNum));
        if (DoesPermissionTreatZeroFKeyAsAll(enumPermType) && listGroupPermissions.Any(x => x.FKey == 0)) //Access to everything.
            return true;
        return listGroupPermissions.Any(x => x.FKey == fKey);
    }

    /// <summary>
    ///     Checks if user has permission to access the passed-in adjustment type.
    ///     Unlike other permissions, if this permission node isn't checked then a user is not barred from creating this
    ///     specific adjustment type
    /// </summary>
    public static bool HasPermissionForAdjType(Def defAdjType, bool suppressMessage = true)
    {
        var listUserGroupsAdjTypeDeny = UserGroups.GetForPermission(EnumPermType.AdjustmentTypeDeny);
        var listUserGroupsForUser = UserGroups.GetForUser(Security.CurUser.UserNum, Security.CurUser.UserNumCEMT != 0);
        var listUserGroupsForUserWithAdjTypeDeny = listUserGroupsForUser.FindAll(x => listUserGroupsAdjTypeDeny.Any(y => y.UserGroupNum == x.UserGroupNum));
        var listUserGroupNums = listUserGroupsForUserWithAdjTypeDeny.Select(x => x.UserGroupNum).ToList();
        var listGroupPermissions = GetForUserGroups(listUserGroupNums, EnumPermType.AdjustmentTypeDeny)
            .FindAll(x => x.FKey == defAdjType.DefNum || x.FKey == 0); // Fkey of 0 means all adjTypeDefs were selected
        //Return true when not all the user's groups with AdjustmentTypeDeny have the adjTypeDef.DefNum checked or have the Fkey value of 0 so the adjustment is not blocked.
        if (listGroupPermissions.IsNullOrEmpty() || listGroupPermissions.Count != listUserGroupsForUser.Count) return true;
        if (suppressMessage) return false;
        var unauthorizedMessage = Lans.g("Security", "Not authorized.") + "\r\n"
                                                                        + Lans.g("Security", "A user with the SecurityAdmin permission must grant you access for adjustment type") + ":\r\n" + defAdjType.ItemName;
        MessageBox.Show(unauthorizedMessage);
        return false;
    }

    /// <summary>
    ///     Checks if user has permission to access the passed-in adjustment type then checks if the user has the
    ///     passed-in permission as well.
    /// </summary>
    public static bool HasPermissionForAdjType(EnumPermType enumPermType, Def defAdjType, bool supressMessage = true)
    {
        return HasPermissionForAdjType(enumPermType, defAdjType, DateTime.MinValue, supressMessage);
    }

    /// <summary>
    ///     Checks if user has permission to access the passed-in adjustment type then checks if the user has the passed-in
    ///     permission as well. Use this method if the permission
    ///     also takes in a date.
    /// </summary>
    public static bool HasPermissionForAdjType(EnumPermType enumPermType, Def defAdjType, DateTime dateTime, bool suppressMessage = true)
    {
        var canEdit = HasPermissionForAdjType(defAdjType, suppressMessage);
        if (!canEdit) return false;
        return Security.IsAuthorized(enumPermType, dateTime, suppressMessage);
    }

    public static bool DoesPermissionTreatZeroFKeyAsAll(EnumPermType enumPermType)
    {
        return enumPermType.In(EnumPermType.AdjustmentTypeDeny, EnumPermType.DashboardWidget, EnumPermType.Reports);
    }

    /// <summary>
    ///     Returns permissions associated to the passed-in usergroups.
    ///     Pass in a specific permType to only return GroupPermissions of that type.
    ///     Otherwise, will return all GroupPermissions for the UserGroups.
    /// </summary>
    public static List<GroupPermission> GetForUserGroups(List<long> listUserGroupNums, EnumPermType enumPermType = EnumPermType.None)
    {
        if (enumPermType == EnumPermType.None) return GetWhere(x => listUserGroupNums.Contains(x.UserGroupNum));
        return GetWhere(x => x.PermType == enumPermType && listUserGroupNums.Contains(x.UserGroupNum));
    }

    /// <summary>
    ///     Gets permissions that actually generate audit trail entries. Returns false for HQ-only preferences if not at
    ///     HQ.
    /// </summary>
    public static bool HasAuditTrail(EnumPermType enumPermType)
    {
        switch (enumPermType)
        {
            case EnumPermType.None:
            case EnumPermType.AppointmentsModule:
            case EnumPermType.ManageModule:
            case EnumPermType.StartupSingleUserOld:
            case EnumPermType.StartupMultiUserOld:
            case EnumPermType.TimecardsEditAll:
            case EnumPermType.AnesthesiaIntakeMeds:
            case EnumPermType.AnesthesiaControlMeds:
            case EnumPermType.EquipmentDelete:
            case EnumPermType.ProcEditShowFee:
            case EnumPermType.AdjustmentEditZero:
            case EnumPermType.EhrEmergencyAccess:
            case EnumPermType.EcwAppointmentRevise:
            case EnumPermType.ProcedureNoteFull:
            case EnumPermType.ProcedureNoteUser:
            case EnumPermType.GraphicalReports:
            case EnumPermType.EquipmentSetup:
            case EnumPermType.WikiListSetup:
            case EnumPermType.Copy:
            case EnumPermType.PatFamilyHealthEdit:
            case EnumPermType.PatientPortal:
            case EnumPermType.AdminDentalStudents:
            case EnumPermType.AdminDentalInstructors:
            case EnumPermType.OrthoChartEditUser:
            case EnumPermType.AdminDentalEvaluations:
            case EnumPermType.UserQueryAdmin:
            case EnumPermType.ProviderFeeEdit:
            case EnumPermType.ClaimHistoryEdit:
            case EnumPermType.PreAuthSentEdit:
            case EnumPermType.InsPlanVerifyList:
            case EnumPermType.ProviderAlphabetize:
            case EnumPermType.ClaimProcReceivedEdit:
            case EnumPermType.ReportProdIncAllProviders:
            case EnumPermType.ReportDailyAllProviders:
            case EnumPermType.SheetDelete:
            case EnumPermType.UpdateCustomTracking:
            case EnumPermType.InsPlanOrthoEdit:
            case EnumPermType.PopupEdit:
            case EnumPermType.InsPlanPickListExisting:
            case EnumPermType.GroupNoteEditSigned:
            case EnumPermType.WikiAdmin:
            case EnumPermType.ClaimView:
            case EnumPermType.TreatPlanSign:
            case EnumPermType.UnrestrictedSearch:
            case EnumPermType.ArchivedPatientEdit:
            case EnumPermType.InsuranceVerification:
            case EnumPermType.NewClaimsProcNotBilled:
            case EnumPermType.WebFormAccess:
            case EnumPermType.Zoom:
            case EnumPermType.CertificationEmployee:
            case EnumPermType.CertificationSetup:
            case EnumPermType.MedicationDefEdit:
            case EnumPermType.AllergyDefEdit:
            case EnumPermType.TextMessageSend:
            case EnumPermType.AdjustmentTypeDeny:
            case EnumPermType.SetupWizard:
            case EnumPermType.SupplierEdit:
            case EnumPermType.AppointmentResize:
            case EnumPermType.ViewAppointmentAuditTrail:
            case EnumPermType.ArchivedPatientSelect:
            case EnumPermType.ClaimProcFeeBilledToInsEdit:
            case EnumPermType.ChildDaycareEdit:
            case EnumPermType.PerioEditCopy:
            case EnumPermType.EFormDelete:
            case EnumPermType.ChartViewsEdit:
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Removes all FKey specific permissions and gives the user group a single 'zero FKey' permission for the type
    ///     passed in.
    /// </summary>
    public static void GiveUserGroupPermissionAll(long userGroupNum, EnumPermType enumPermType)
    {
        //Remove all permissions for the user group and perm type.
        var command = $"DELETE FROM grouppermission WHERE UserGroupNum={SOut.Long(userGroupNum)} AND PermType={SOut.Enum(enumPermType)}";
        Db.NonQ(command);
        //AdjustmentTypeDeny is a permission that denies access to a usergroup when they have this permission. When a user clicks 'Set All', they want the user group to have every permission.
        //This means they want the user group to have access to every adjustment type. So we need to delete all adjustment type deny permissions for this user group, which we do above. 
        //But we do NOT want to create a 0 FKey perm because that will indicate the user group does not have access to any adjusment type, so we return early.
        if (enumPermType == EnumPermType.AdjustmentTypeDeny) return;
        //Insert a new permission with a zero FKey.
        var groupPermission = new GroupPermission();
        groupPermission.NewerDate = DateTime.MinValue;
        groupPermission.NewerDays = 0;
        groupPermission.PermType = enumPermType;
        groupPermission.UserGroupNum = userGroupNum;
        groupPermission.FKey = 0;
        GroupPermissionCrud.Insert(groupPermission);
    }

    ///<summary>Gets the description for the specified permisssion.  Already translated.</summary>
    public static string GetDesc(EnumPermType enumPermType)
    {
        return Lans.g("enumPermissions", enumPermType.GetDescription()); //If Description attribute is not defined, will default to perm.ToString()
    }

    
    public static bool PermTakesDates(EnumPermType enumPermType)
    {
        if (enumPermType == EnumPermType.AccountingCreate //prevents backdating
            || enumPermType == EnumPermType.AccountingEdit
            || enumPermType == EnumPermType.AdjustmentCreate
            || enumPermType == EnumPermType.AdjustmentEdit
            || enumPermType == EnumPermType.ClaimDelete
            || enumPermType == EnumPermType.ClaimHistoryEdit
            || enumPermType == EnumPermType.ClaimProcReceivedEdit
            || enumPermType == EnumPermType.ClaimSentEdit
            || enumPermType == EnumPermType.CommlogEdit
            || enumPermType == EnumPermType.DepositSlips //prevents backdating
            || enumPermType == EnumPermType.EFormEdit
            || enumPermType == EnumPermType.EquipmentDelete
            || enumPermType == EnumPermType.ImageDelete
            || enumPermType == EnumPermType.InsPayEdit
            || enumPermType == EnumPermType.InsWriteOffEdit
            || enumPermType == EnumPermType.NewClaimsProcNotBilled
            || enumPermType == EnumPermType.OrthoChartEditFull
            || enumPermType == EnumPermType.OrthoChartEditUser
            || enumPermType == EnumPermType.PaymentEdit
            || enumPermType == EnumPermType.PerioEdit
            || enumPermType == EnumPermType.PreAuthSentEdit
            || enumPermType == EnumPermType.ProcComplCreate
            || enumPermType == EnumPermType.ProcCompleteEdit
            || enumPermType == EnumPermType.ProcCompleteNote
            || enumPermType == EnumPermType.ProcCompleteEditMisc
            || enumPermType == EnumPermType.ProcCompleteStatusEdit
            || enumPermType == EnumPermType.ProcCompleteAddAdj
            || enumPermType == EnumPermType.ProcExistingEdit
            || enumPermType == EnumPermType.ProcDelete
            || enumPermType == EnumPermType.SheetEdit
            || enumPermType == EnumPermType.TimecardDeleteEntry
            || enumPermType == EnumPermType.TreatPlanEdit
            || enumPermType == EnumPermType.TreatPlanSign
            || enumPermType == EnumPermType.PaymentCreate //to prevent backdating of newly created payments
            || enumPermType == EnumPermType.ImageEdit
            || enumPermType == EnumPermType.ImageExport
           )
            return true;
        return false;
    }

    /// <summary>
    ///     Returns a list of permissions that are included in the bitwise enum crudSLFKeyPerms passed in.
    ///     Used in DBM and the crud generator.  Needs to be updated every time a new CrudAuditPerm is added.
    /// </summary>
    public static List<EnumPermType> GetPermsFromCrudAuditPerm(CrudAuditPerm crudAuditPerm)
    {
        var listPerms = new List<EnumPermType>();
        //No check for none.
        if (crudAuditPerm.HasFlag(CrudAuditPerm.AppointmentCompleteEdit)) //b01
            listPerms.Add(EnumPermType.AppointmentCompleteEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.AppointmentCreate)) //b010
            listPerms.Add(EnumPermType.AppointmentCreate);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.AppointmentEdit)) //b0100
            listPerms.Add(EnumPermType.AppointmentEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.AppointmentMove)) //b01000
            listPerms.Add(EnumPermType.AppointmentMove);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.ClaimHistoryEdit)) //b010000
            listPerms.Add(EnumPermType.ClaimHistoryEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.ImageDelete)) //b0100000
            listPerms.Add(EnumPermType.ImageDelete);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.ImageEdit)) //b01000000
            listPerms.Add(EnumPermType.ImageEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.InsPlanChangeCarrierName)) //b010000000
            listPerms.Add(EnumPermType.InsPlanChangeCarrierName);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.RxCreate)) //b0100000000
            listPerms.Add(EnumPermType.RxCreate);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.RxEdit)) //b01000000000
            listPerms.Add(EnumPermType.RxEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.TaskNoteEdit)) //b010000000000
            listPerms.Add(EnumPermType.TaskNoteEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.PatientPortal)) //b0100000000000
            listPerms.Add(EnumPermType.PatientPortal);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.ProcFeeEdit)) //b01000000000000
            listPerms.Add(EnumPermType.ProcFeeEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.LogFeeEdit)) //b010000000000000
            listPerms.Add(EnumPermType.LogFeeEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.LogSubscriberEdit)) //b0100000000000000
            listPerms.Add(EnumPermType.LogSubscriberEdit);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.AppointmentDelete)) //b01000000000000000
            listPerms.Add(EnumPermType.AppointmentDelete);
        if (crudAuditPerm.HasFlag(CrudAuditPerm.AppointmentCompleteDelete)) //b010000000000000000
            listPerms.Add(EnumPermType.AppointmentCompleteDelete);
        return listPerms;
    }

    #region Misc Methods

    /// <summary>
    ///     Returns the Date that the user is restricted to for the passed-in permission.
    ///     Returns MinVal if the user is not restricted or does not have the permission.
    /// </summary>
    public static DateTime GetDateRestrictedForPermission(EnumPermType enumPermType, List<long> listUserGroupNums)
    {
        var nowDate = DateTime.MinValue;
        var getNowDate = new Func<DateTime>(() =>
        {
            if (nowDate.Year < 1880) nowDate = MiscData.GetNowDateTime().Date;
            return nowDate;
        });
        var dateTimeRet = DateTime.MinValue;
        var listGroupPermissions = GetForUserGroups(listUserGroupNums, enumPermType);
        //get the permission that applies
        var groupPermission = listGroupPermissions.OrderBy(y =>
        {
            if (y.NewerDays == 0 && y.NewerDate == DateTime.MinValue) return DateTime.MinValue;
            if (y.NewerDays == 0) return y.NewerDate;
            return getNowDate().AddDays(-y.NewerDays);
        }).FirstOrDefault();
        if (groupPermission == null)
        {
            //do not change retVal. The user does not have the permission.
        }
        else if (groupPermission.NewerDate.Year < 1880 && groupPermission.NewerDays == 0)
        {
            //do not change retVal. The user is not restricted by date.
        }
        else if (groupPermission.NewerDate.Year > 1880)
        {
            dateTimeRet = groupPermission.NewerDate;
        }
        else if (getNowDate().AddDays(-groupPermission.NewerDays) > dateTimeRet)
        {
            dateTimeRet = getNowDate().AddDays(-groupPermission.NewerDays);
        }

        return dateTimeRet;
    }

    ///<summary>Used for procedures with status EO, EC, or C. Returns Permissions.ProcExistingEdit for EO/EC</summary>
    public static EnumPermType SwitchExistingPermissionIfNeeded(EnumPermType enumPermType, Procedure procedure)
    {
        if (procedure.ProcStatus.In(ProcStat.EO, ProcStat.EC)) return EnumPermType.ProcExistingEdit;
        return enumPermType;
    }

    #endregion

    #region CachePattern

    private class GroupPermissionCache : CacheListAbs<GroupPermission>
    {
        protected override List<GroupPermission> GetCacheFromDb()
        {
            var command = "SELECT * FROM grouppermission";
            return GroupPermissionCrud.SelectMany(command);
        }

        protected override List<GroupPermission> TableToList(DataTable dataTable)
        {
            return GroupPermissionCrud.TableToList(dataTable);
        }

        protected override GroupPermission Copy(GroupPermission item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<GroupPermission> items)
        {
            return GroupPermissionCrud.ListToTable(items, "GroupPermission");
        }

        protected override void FillCacheIfNeeded()
        {
            GroupPermissions.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly GroupPermissionCache _GroupPermissionCache = new();

    public static GroupPermission GetFirstOrDefault(Func<GroupPermission, bool> match, bool isShort = false)
    {
        return _GroupPermissionCache.GetFirstOrDefault(match, isShort);
    }

    public static List<GroupPermission> GetWhere(Predicate<GroupPermission> match, bool isShort = false)
    {
        return _GroupPermissionCache.GetWhere(match, isShort);
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
        _GroupPermissionCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _GroupPermissionCache.GetTableFromCache(doRefreshCache);
    }

    ///<summary>Clears the cache.</summary>
    public static void ClearCache()
    {
        _GroupPermissionCache.ClearCache();
    }

    #endregion
}