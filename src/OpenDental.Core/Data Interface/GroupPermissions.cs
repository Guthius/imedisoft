using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public static class GroupPermissions
{
    public const double NewerDaysMax = 3000;

    public static void Update(GroupPermission groupPermission)
    {
        if (groupPermission.NewerDate.Year > 1880 && groupPermission.NewerDays > 0)
        {
            throw new Exception("Date or days can be set, but not both.");
        }

        if (!PermTakesDates(groupPermission.PermType))
        {
            if (groupPermission.NewerDate.Year > 1880 || groupPermission.NewerDays > 0)
            {
                throw new Exception("This type of permission may not have a date or days set.");
            }
        }

        GroupPermissionCrud.Update(groupPermission);
    }

    public static void DeleteForPermTypeAndUserGroup(EnumPermType permType, long userGroupNum)
    {
        Db.NonQ("DELETE FROM grouppermission WHERE PermType=" + (int) permType + " AND UserGroupNum=" + userGroupNum);
    }

    public static void Insert(GroupPermission groupPermission)
    {
        if (groupPermission.NewerDate.Year > 1880 && groupPermission.NewerDays > 0)
        {
            throw new Exception("Date or days can be set, but not both.");
        }

        if (!PermTakesDates(groupPermission.PermType))
        {
            if (groupPermission.NewerDate.Year > 1880 || groupPermission.NewerDays > 0)
            {
                throw new Exception("This type of permission may not have a date or days set.");
            }
        }

        if (groupPermission.PermType != EnumPermType.SecurityAdmin)
        {
            GroupPermissionCrud.Insert(groupPermission);
            return;
        }

        var count = SIn.Int(Db.GetCount(
            "SELECT COUNT(*) FROM userod " +
            "INNER JOIN usergroupattach ON usergroupattach.UserNum=userod.UserNum " +
            "WHERE userod.IsHidden=1 " +
            "AND usergroupattach.UserGroupNum=" + groupPermission.UserGroupNum));

        if (count != 0)
        {
            throw new Exception("The Security Admin permission cannot be given to a user group with hidden users.");
        }

        GroupPermissionCrud.Insert(groupPermission);
    }

    public static void RemovePermission(long userGroupNum, EnumPermType permType)
    {
        if (permType == EnumPermType.SecurityAdmin)
        {
            var command =
                "SELECT COUNT(*) FROM (SELECT DISTINCT grouppermission.UserGroupNum " +
                "FROM grouppermission " +
                "INNER JOIN usergroupattach ON usergroupattach.UserGroupNum=grouppermission.UserGroupNum " +
                "INNER JOIN userod ON userod.UserNum=usergroupattach.UserNum AND userod.IsHidden=0 " +
                "WHERE grouppermission.PermType=" + (int) permType + " " +
                "AND grouppermission.UserGroupNum!=" + userGroupNum + ") t";

            if (DataCore.GetScalar(command) == "0")
            {
                throw new Exception("There must always be at least one user in a user group that has the Security Admin permission.");
            }
        }

        Db.NonQ("DELETE FROM grouppermission WHERE UserGroupNum=" + userGroupNum + " AND PermType=" + (int) permType);
    }

    public static bool Sync(List<GroupPermission> listGroupPermissionsNew, List<GroupPermission> listGroupPermissionsOld)
    {
        return GroupPermissionCrud.Sync(listGroupPermissionsNew, listGroupPermissionsOld);
    }

    public static GroupPermission GetPerm(long userGroupNum, EnumPermType enumPermType)
    {
        return GetFirstOrDefault(x => x.UserGroupNum == userGroupNum && x.PermType == enumPermType);
    }

    public static List<GroupPermission> GetPermsForReports(long userGroupNum = 0)
    {
        var groupPermissions = GetWhere(x => x.PermType == EnumPermType.Reports);
        if (userGroupNum > 0)
        {
            groupPermissions.RemoveAll(x => x.UserGroupNum != userGroupNum);
        }

        return groupPermissions;
    }

    public static List<GroupPermission> GetAdjustmentTypeDenyPermsForUserGroup(long userGroupNum)
    {
        return GetWhere(x => x.PermType == EnumPermType.AdjustmentTypeDeny && x.UserGroupNum == userGroupNum);
    }

    public static List<GroupPermission> GetPermsForReports(Userod user)
    {
        return GetWhere(x => x.PermType == EnumPermType.Reports && user.IsInUserGroup(x.UserGroupNum));
    }

    public static bool HasReportPermission(string reportName, Userod user, List<DisplayReport> displayReports = null)
    {
        var displayReport = displayReports == null ? DisplayReports.GetAll(false).Find(x => x.InternalName == reportName) : displayReports.Find(x => x.InternalName == reportName);

        return displayReport != null && GetPermsForReports(user).Any(x => x.FKey.In(0, displayReport.DisplayReportNum));
    }

    public static bool HasPermission(long userGroupNum, EnumPermType permType, long fKey, List<GroupPermission> groupPermissions = null)
    {
        List<GroupPermission> groupPermissionsCopy;
        if (groupPermissions is null)
        {
            groupPermissionsCopy = GetWhere(x => x.UserGroupNum == userGroupNum && x.PermType == permType);
        }
        else
        {
            groupPermissionsCopy = new List<GroupPermission>(groupPermissions);
            groupPermissionsCopy.RemoveAll(x => x.UserGroupNum != userGroupNum || x.PermType != permType);
        }

        if (DoesPermissionTreatZeroFKeyAsAll(permType) && groupPermissionsCopy.Any(x => x.FKey == 0))
        {
            return true;
        }

        return groupPermissionsCopy.Any(x => x.FKey == fKey);
    }

    public static bool HasPermission(Userod user, EnumPermType permType, long fKey, List<GroupPermission> groupPermissions = null)
    {
        if (groupPermissions is null)
        {
            groupPermissions = GetWhere(x => x.PermType == permType && user.IsInUserGroup(x.UserGroupNum));
        }
        else
        {
            groupPermissions.RemoveAll(x => x.PermType != permType && !user.IsInUserGroup(x.UserGroupNum));
        }

        if (DoesPermissionTreatZeroFKeyAsAll(permType) && groupPermissions.Any(x => x.FKey == 0))
        {
            return true;
        }

        return groupPermissions.Any(x => x.FKey == fKey);
    }

    public static bool HasPermissionForAdjType(Def adjTypeDef, bool suppressMessage = true)
    {
        var userGroupsAdjTypeDeny = UserGroups.GetForPermission(EnumPermType.AdjustmentTypeDeny);
        var userGroupsForUser = UserGroups.GetForUser(Security.CurUser.UserNum);
        var userGroupsForUserWithAdjTypeDeny = userGroupsForUser.FindAll(x => userGroupsAdjTypeDeny.Any(y => y.UserGroupNum == x.UserGroupNum));
        var userGroupNums = userGroupsForUserWithAdjTypeDeny.Select(x => x.UserGroupNum).ToList();
        
        var groupPermissions = GetForUserGroups(userGroupNums, EnumPermType.AdjustmentTypeDeny).FindAll(x => x.FKey == adjTypeDef.DefNum || x.FKey == 0); 
        if (groupPermissions.IsNullOrEmpty() || groupPermissions.Count != userGroupsForUser.Count)
        {
            return true;
        }
        
        if (suppressMessage)
        {
            return false;
        }
        
        var unauthorizedMessage = 
            "Not authorized.\r\n" + 
            "A user with the SecurityAdmin permission must grant you access for adjustment type:\r\n" + 
            adjTypeDef.ItemName;
        
        MessageBox.Show(unauthorizedMessage);
        return false;
    }

    public static bool HasPermissionForAdjType(EnumPermType permType, Def adjTypeDef, bool supressMessage = true)
    {
        return HasPermissionForAdjType(permType, adjTypeDef, DateTime.MinValue, supressMessage);
    }

    public static bool HasPermissionForAdjType(EnumPermType permType, Def adjTypeDef, DateTime dateTime, bool suppressMessage = true)
    {
        var canEdit = HasPermissionForAdjType(adjTypeDef, suppressMessage);
        
        return canEdit && Security.IsAuthorized(permType, dateTime, suppressMessage);
    }

    public static bool DoesPermissionTreatZeroFKeyAsAll(EnumPermType permType)
    {
        return permType is EnumPermType.AdjustmentTypeDeny or EnumPermType.DashboardWidget or EnumPermType.Reports;
    }

    public static List<GroupPermission> GetForUserGroups(List<long> userGroupNums, EnumPermType permType = EnumPermType.None)
    {
        return permType == EnumPermType.None ? GetWhere(x => userGroupNums.Contains(x.UserGroupNum)) : GetWhere(x => x.PermType == permType && userGroupNums.Contains(x.UserGroupNum));
    }

    public static bool HasAuditTrail(EnumPermType permType)
    {
        return permType switch
        {
            EnumPermType.None or
                EnumPermType.AppointmentsModule or
                EnumPermType.ManageModule or
                EnumPermType.StartupSingleUserOld or
                EnumPermType.StartupMultiUserOld or
                EnumPermType.TimecardsEditAll or
                EnumPermType.AnesthesiaIntakeMeds or
                EnumPermType.AnesthesiaControlMeds or
                EnumPermType.EquipmentDelete or
                EnumPermType.ProcEditShowFee or
                EnumPermType.AdjustmentEditZero or
                EnumPermType.EhrEmergencyAccess or
                EnumPermType.EcwAppointmentRevise or
                EnumPermType.ProcedureNoteFull or
                EnumPermType.ProcedureNoteUser or
                EnumPermType.GraphicalReports or
                EnumPermType.EquipmentSetup or
                EnumPermType.WikiListSetup or
                EnumPermType.Copy or
                EnumPermType.PatFamilyHealthEdit or
                EnumPermType.PatientPortal or
                EnumPermType.AdminDentalStudents or
                EnumPermType.AdminDentalInstructors or
                EnumPermType.OrthoChartEditUser or
                EnumPermType.AdminDentalEvaluations or
                EnumPermType.UserQueryAdmin or
                EnumPermType.ProviderFeeEdit or
                EnumPermType.ClaimHistoryEdit or
                EnumPermType.PreAuthSentEdit or
                EnumPermType.InsPlanVerifyList or
                EnumPermType.ProviderAlphabetize or
                EnumPermType.ClaimProcReceivedEdit or
                EnumPermType.ReportProdIncAllProviders or
                EnumPermType.ReportDailyAllProviders or
                EnumPermType.SheetDelete or
                EnumPermType.UpdateCustomTracking or
                EnumPermType.InsPlanOrthoEdit or
                EnumPermType.PopupEdit or
                EnumPermType.InsPlanPickListExisting or
                EnumPermType.GroupNoteEditSigned or
                EnumPermType.WikiAdmin or
                EnumPermType.ClaimView or
                EnumPermType.TreatPlanSign or
                EnumPermType.UnrestrictedSearch or
                EnumPermType.ArchivedPatientEdit or
                EnumPermType.InsuranceVerification or
                EnumPermType.NewClaimsProcNotBilled or
                EnumPermType.WebFormAccess or
                EnumPermType.Zoom or
                EnumPermType.CertificationEmployee or
                EnumPermType.CertificationSetup or
                EnumPermType.MedicationDefEdit or
                EnumPermType.AllergyDefEdit or
                EnumPermType.TextMessageSend or
                EnumPermType.AdjustmentTypeDeny or
                EnumPermType.SetupWizard or
                EnumPermType.SupplierEdit or
                EnumPermType.AppointmentResize or
                EnumPermType.ViewAppointmentAuditTrail or
                EnumPermType.ArchivedPatientSelect or
                EnumPermType.ClaimProcFeeBilledToInsEdit or
                EnumPermType.PerioEditCopy or
                EnumPermType.EFormDelete or
                EnumPermType.ChartViewsEdit => false,
            _ => true
        };
    }

    public static void GiveUserGroupPermissionAll(long userGroupNum, EnumPermType permType)
    {
        Db.NonQ($"DELETE FROM grouppermission WHERE UserGroupNum={userGroupNum} AND PermType={(int) permType}");

        if (permType == EnumPermType.AdjustmentTypeDeny)
        {
            return;
        }

        GroupPermissionCrud.Insert(new GroupPermission
        {
            NewerDate = DateTime.MinValue,
            NewerDays = 0,
            PermType = permType,
            UserGroupNum = userGroupNum,
            FKey = 0
        });
    }

    public static string GetDesc(EnumPermType permType)
    {
        return permType.GetDescription();
    }

    public static bool PermTakesDates(EnumPermType permType)
    {
        return permType is EnumPermType.AccountingCreate or EnumPermType.AccountingEdit or EnumPermType.AdjustmentCreate or EnumPermType.AdjustmentEdit or EnumPermType.ClaimDelete or EnumPermType.ClaimHistoryEdit or EnumPermType.ClaimProcReceivedEdit or EnumPermType.ClaimSentEdit or EnumPermType.CommlogEdit or EnumPermType.DepositSlips or EnumPermType.EFormEdit or EnumPermType.EquipmentDelete or EnumPermType.ImageDelete or EnumPermType.InsPayEdit or EnumPermType.InsWriteOffEdit or EnumPermType.NewClaimsProcNotBilled or EnumPermType.OrthoChartEditFull or EnumPermType.OrthoChartEditUser or EnumPermType.PaymentEdit or EnumPermType.PerioEdit or EnumPermType.PreAuthSentEdit or EnumPermType.ProcComplCreate or EnumPermType.ProcCompleteEdit or EnumPermType.ProcCompleteNote or EnumPermType.ProcCompleteEditMisc or EnumPermType.ProcCompleteStatusEdit or EnumPermType.ProcCompleteAddAdj or EnumPermType.ProcExistingEdit or EnumPermType.ProcDelete or EnumPermType.SheetEdit or EnumPermType.TimecardDeleteEntry or EnumPermType.TreatPlanEdit or EnumPermType.TreatPlanSign or EnumPermType.PaymentCreate or EnumPermType.ImageEdit or EnumPermType.ImageExport;
    }

    public static DateTime GetDateRestrictedForPermission(EnumPermType permType, List<long> userGroupNums)
    {
        var nowDate = DateTime.MinValue;

        var getNowDate = new Func<DateTime>(() =>
        {
            if (nowDate.Year < 1880)
            {
                nowDate = MiscData.GetNowDateTime().Date;
            }

            return nowDate;
        });

        var result = DateTime.MinValue;
        var groupPermissions = GetForUserGroups(userGroupNums, permType);

        var groupPermission = groupPermissions
            .OrderBy(y =>
            {
                return y.NewerDays switch
                {
                    0 when y.NewerDate == DateTime.MinValue => DateTime.MinValue,
                    0 => y.NewerDate,
                    _ => getNowDate().AddDays(-y.NewerDays)
                };
            })
            .FirstOrDefault();

        if (groupPermission == null)
        {
        }
        else
            switch (groupPermission.NewerDate.Year)
            {
                case < 1880 when groupPermission.NewerDays == 0:
                    break;

                case > 1880:
                    result = groupPermission.NewerDate;
                    break;

                default:
                {
                    if (getNowDate().AddDays(-groupPermission.NewerDays) > result)
                    {
                        result = getNowDate().AddDays(-groupPermission.NewerDays);
                    }

                    break;
                }
            }

        return result;
    }

    public static EnumPermType SwitchExistingPermissionIfNeeded(EnumPermType permType, Procedure procedure)
    {
        return procedure.ProcStatus is ProcStat.EO or ProcStat.EC ? EnumPermType.ProcExistingEdit : permType;
    }

    private class GroupPermissionCache : CacheListAbs<GroupPermission>
    {
        protected override List<GroupPermission> GetCacheFromDb()
        {
            return GroupPermissionCrud.SelectMany("SELECT * FROM grouppermission");
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

    private static readonly GroupPermissionCache Cache = new();

    public static GroupPermission GetFirstOrDefault(Func<GroupPermission, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<GroupPermission> GetWhere(Predicate<GroupPermission> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}