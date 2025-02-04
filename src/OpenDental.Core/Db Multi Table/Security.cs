using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Security
{
    private static Userod curUser;

    [ThreadStatic]
    private static Userod _curUserT;

    private static string _curComputerName;

    [ThreadStatic]
    private static string _curComputerNameT;

    public static bool IsUserLoggedIn;
    public static DateTime DateTimeLastActivity;

    public static Userod CurUser
    {
        get
        {
            if (_curUserT != null)
            {
                return _curUserT;
            }

            return curUser;
        }
        set
        {
            if (_curUserT == value && curUser == value)
            {
                return;
            }

            _curUserT = value;
            curUser = value;
            ODEvent.Fire(ODEventType.Userod, _curUserT?.UserNum ?? curUser?.UserNum ?? 0);
        }
    }

    public static string CurComputerName
    {
        get
        {
            if (_curComputerNameT != null)
            {
                //Allows an empty string.
                return _curComputerNameT;
            }

            if (_curComputerName == null)
            {
                _curComputerName = Environment.MachineName;
            }

            return _curComputerName;
        }
        set
        {
            if (!false)
            {
                _curComputerNameT = value;
            }

            _curComputerName = value;
        }
    }

    public static string PasswordTyped
    {
        set { }
    }

    public static bool IsAuthorized(EnumPermType perm)
    {
        return IsAuthorized(perm, DateTime.MinValue, false);
    }

    public static bool IsAuthorized(EnumPermType perm, long fKey, bool suppressMessage)
    {
        return IsAuthorized(perm, DateTime.MinValue, suppressMessage, true, 0, -1, 0, fKey);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date)
    {
        return IsAuthorized(perm, date, false);
    }

    public static bool IsAuthorized(EnumPermType perm, bool suppressMessage)
    {
        return IsAuthorized(perm, DateTime.MinValue, suppressMessage);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, bool suppressMessage)
    {
        return IsAuthorized(perm, date, suppressMessage, false);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, bool suppressMessage, bool suppressLockDateMessage)
    {
        return IsAuthorized(perm, date, suppressMessage, suppressLockDateMessage, 0, -1, 0, 0);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, long procCodeNum, double procCodeFee)
    {
        return IsAuthorized(perm, date, false, false, procCodeNum, procCodeFee, 0, 0);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, long procCodeNum, double procCodeFee, bool suppressMessage)
    {
        return IsAuthorized(perm, date, suppressMessage, suppressMessage, procCodeNum, procCodeFee, 0, 0);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, bool suppressMessage, bool suppressLockDateMessage, Userod curUser)
    {
        return IsAuthorized(perm, date, suppressMessage, suppressLockDateMessage, curUser, 0, -1, 0, 0);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, long procCodeNum = 0, double procCodeFee = -1, Action<string> actionNotAuthorized = null)
    {
        return IsAuthorized(perm, date, suppressMsgBox: true, suppressLockDateMessage: true, procCodeNum, procCodeFee, sheetDefNum: 0, fKey: 0, actionNotAuthorized);
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, bool suppressMsgBox, bool suppressLockDateMessage, long procCodeNum, double procCodeFee, long sheetDefNum, long fKey, Action<string> actionNotAuthorized = null)
    {
        if (CurUser == null)
        {
            var msg = Lans.g("Security", "Not authorized for") + "\r\n" + GroupPermissions.GetDesc(perm);
            if (!suppressMsgBox)
            {
                MessageBox.Show(msg);
            }

            actionNotAuthorized?.Invoke(msg);
            return false;
        }

        try
        {
            return IsAuthorized(perm, date, suppressMsgBox, suppressLockDateMessage, CurUser, procCodeNum, procCodeFee, sheetDefNum, fKey, actionNotAuthorized);
        }
        catch (Exception ex)
        {
            if (!suppressMsgBox)
            {
                MessageBox.Show(ex.Message);
            }

            actionNotAuthorized?.Invoke(ex.Message);
            return false;
        }
    }

    public static bool IsAuthorized(EnumPermType perm, DateTime date, bool suppressException, bool suppressLockDateMessage, Userod curUser, long procCodeNum, double procFee, long sheetDefNum, long fKey, Action<string> actionNotAuthorized = null)
    {
        date = date.Date; //Remove the time portion of date so we can compare strictly as a date later.
        //Check eConnector permission first.
        if (IsValidEServicePermission(perm))
        {
            return true;
        }

        string errorMsg;
        if (!GroupPermissions.HasPermission(curUser, perm, fKey))
        {
            errorMsg = Lans.g("Security", "Not authorized.") + "\r\n"
                                                             + Lans.g("Security", "A user with the SecurityAdmin permission must grant you access for") + ":\r\n"
                                                             + GroupPermissions.GetDesc(perm);
            if (!suppressException)
            {
                throw new Exception(errorMsg);
            }

            actionNotAuthorized?.Invoke(errorMsg);
            return false;
        }

        if (perm == EnumPermType.AccountingCreate || perm == EnumPermType.AccountingEdit)
        {
            if (date <= PrefC.GetDate(PrefName.AccountingLockDate))
            {
                errorMsg = Lans.g("Security", "Locked by Administrator.");
                if (!suppressException && !suppressLockDateMessage)
                {
                    throw new Exception(errorMsg);
                }

                actionNotAuthorized?.Invoke(errorMsg);
                return false;
            }
        }

        //Check the global security lock------------------------------------------------------------------------------------
        if (IsGlobalDateLock(perm, date, suppressException || suppressLockDateMessage, procCodeNum, procFee, sheetDefNum, actionNotAuthorized))
        {
            return false;
        }

        //Check date/days limits on individual permission----------------------------------------------------------------
        if (!GroupPermissions.PermTakesDates(perm))
        {
            return true;
        }

        //Include CEMT users, as a CEMT user could be logged in when this is checked.
        var dateLimit = GetDateLimit(perm, curUser.GetGroups().Select(x => x.UserGroupNum).ToList());
        if (date > dateLimit)
        {
            //authorized
            return true;
        }

        //Prevents certain bugs when 1/1/1 dates are passed in and compared----------------------------------------------
        //Handling of min dates.  There might be others, but we have to handle them individually to avoid introduction of bugs.
        if (perm == EnumPermType.ClaimDelete //older versions did not have SecDateEntry
            || perm == EnumPermType.ClaimSentEdit //no date sent was entered before setting claim received	
            || perm == EnumPermType.ProcComplCreate
            || perm == EnumPermType.ProcCompleteEdit
            || perm == EnumPermType.ProcCompleteStatusEdit
            || perm == EnumPermType.ProcCompleteNote
            || perm == EnumPermType.ProcCompleteAddAdj
            || perm == EnumPermType.ProcCompleteEditMisc
            || perm == EnumPermType.ProcExistingEdit //a completed EO or EC procedure with a min date.
            || perm == EnumPermType.InsPayEdit //a claim payment with no date.
            || perm == EnumPermType.InsWriteOffEdit //older versions did not have SecDateEntry or DateEntryC
            || perm == EnumPermType.TreatPlanEdit
            || perm == EnumPermType.AdjustmentCreate
            || perm == EnumPermType.AdjustmentEdit
            || perm == EnumPermType.CommlogEdit //usually from a conversion
            || perm == EnumPermType.ProcDelete //because older versions did not set the DateEntryC.
            || perm == EnumPermType.ImageDelete //In case an image has a document.DateCreated date of DateTime.MinVal.
            || perm == EnumPermType.PerioEdit //In case perio chart exam has a creation date of DateTime.MinValue.
            || perm == EnumPermType.PreAuthSentEdit //older versions did not have SecDateEntry
            || perm == EnumPermType.ClaimProcReceivedEdit //
            || perm == EnumPermType.PaymentCreate //Older versions did not have a date limitation to PaymentCreate
            || perm == EnumPermType.ImageEdit //In case an image has a document.DateCreated date of DateTime.MinVal.
            || perm == EnumPermType.ImageExport //In case an image has a document.DateCreated date of DateTime.MinVal.
            || perm == EnumPermType.SheetEdit //In case a sheet has a sheet.DateTimeSheet date of DateTime.MinVal, will still allow the office to edit the sheet.
            || perm == EnumPermType.EFormEdit)
        {
            if (date.Year < 1880 && dateLimit.Year < 1880)
            {
                return true;
            }
        }

        errorMsg = Lans.g("Security", "Not authorized for") + "\r\n"
                                                            + GroupPermissions.GetDesc(perm) + "\r\n" + Lans.g("Security", "Date limitation");
        if (!suppressException)
        {
            throw new Exception(errorMsg);
        }

        actionNotAuthorized?.Invoke(errorMsg);
        return false;
    }

    public static bool IsGlobalDateLock(EnumPermType perm, DateTime date, bool suppressMsgBox = false, long codeNum = 0, double procFee = -1, long sheetDefNum = 0, Action<string> actionNotAuthorized = null)
    {
        if (!(new[]
            {
                EnumPermType.AdjustmentCreate, EnumPermType.AdjustmentEdit, EnumPermType.PaymentCreate, EnumPermType.PaymentEdit, EnumPermType.ProcComplCreate, EnumPermType.ProcCompleteEdit, EnumPermType.ProcCompleteStatusEdit
                //,Permissions.ProcComplNote (corresponds to obsolete ProcComplEditLimited)
                //,Permissions.ProcComplAddAdj (corresponds to obsolete ProcComplEditLimited)
                //,Permissions.ProcComplEditMisc (corresponds to obsolete ProcComplEditLimited)
                //,Permissions.ProcExistingEdit//per Allen 6/26/2020 this should not be affected by the global date lock
                //,Permissions.ImageDelete
                ,
                EnumPermType.InsPayCreate, EnumPermType.InsPayEdit
                //,Permissions.InsWriteOffEdit//per Nathan 7/5/2016 this should not be affected by the global date lock
                ,
                EnumPermType.SheetEdit, EnumPermType.SheetDelete, EnumPermType.CommlogEdit
                //,Permissions.ClaimDelete //per Nathan 01/18/2018 this should not be affected by the global date lock
                ,
                EnumPermType.PayPlanEdit
                //,Permissions.ClaimHistoryEdit //per Nathan & Mark 03/01/2018 this should not be affected by the global lock date, not financial data.
            }).Contains(perm))
        {
            return false; //permission being checked is not affected by global lock date. (notice the ! 20 lines up)
        }

        if (date.Year == 1)
        {
            return false; //Invalid or MinDate passed in.
        }

        if (!PrefC.GetBool(PrefName.SecurityLockIncludesAdmin) && GroupPermissions.HasPermission(CurUser, EnumPermType.SecurityAdmin, 0))
        {
            return false; //admins are never affected by global date limitation when preference is false.
        }

        var listPermissionsCanBypassLockDate = new List<EnumPermType>()
        {
            EnumPermType.ProcCompleteEdit, EnumPermType.ProcCompleteAddAdj, EnumPermType.ProcCompleteEditMisc, EnumPermType.ProcCompleteStatusEdit, EnumPermType.ProcCompleteNote,
            EnumPermType.ProcComplCreate, EnumPermType.ProcExistingEdit
        };
        if (listPermissionsCanBypassLockDate.Contains(perm) && ProcedureCodes.CanBypassLockDate(codeNum, procFee))
        {
            return false;
        }

        if (perm.In(EnumPermType.SheetEdit, EnumPermType.SheetDelete) && sheetDefNum > 0 && SheetDefs.CanBypassLockDate(sheetDefNum))
        {
            return false;
        }

        //If global lock is Date based.
        if (date <= PrefC.GetDate(PrefName.SecurityLockDate))
        {
            var msg = Lans.g("Security", "Locked by Administrator before ") + PrefC.GetDate(PrefName.SecurityLockDate).ToShortDateString();
            if (!suppressMsgBox)
            {
                MessageBox.Show(msg);
            }

            actionNotAuthorized?.Invoke(msg);
            return true;
        }

        //If global lock is days based.
        var lockDays = PrefC.GetInt(PrefName.SecurityLockDays);
        if (lockDays > 0 && date <= DateTime.Today.AddDays(-lockDays))
        {
            var msg = Lans.g("Security", "Locked by Administrator before") + " " + lockDays.ToString() + " days.";
            if (!suppressMsgBox)
            {
                MessageBox.Show(msg);
            }

            actionNotAuthorized?.Invoke(msg);
            return true;
        }

        return false;
    }

    public static string GetComplexComputerName()
    {
        //If not RDP return CurComputerName for backwards compatibillity. Mimics ODEnvironment.MachineName
        if (typeof(SystemInformation).GetProperty("TerminalServerSession").GetValue(null).ToString() != "True")
        {
            return CurComputerName;
        }

        var arrayComputerNames = new string[] {CurComputerName, Environment.MachineName, Environment.MachineName};
        return string.Join(", ", arrayComputerNames.Where(x => !string.IsNullOrEmpty(x)).Distinct());
    }

    private static DateTime GetDateLimit(EnumPermType permType, List<long> listUserGroupNums)
    {
        return GroupPermissions.GetDateRestrictedForPermission(permType, listUserGroupNums);
    }

    public static int GetModule(int suggestI)
    {
        if (suggestI != -1 && IsAuthorized(PermofModule(suggestI), DateTime.MinValue, true))
        {
            return suggestI;
        }

        for (var i = 0; i < 7; i++)
        {
            if (IsAuthorized(PermofModule(i), DateTime.MinValue, true))
            {
                return i;
            }
        }

        return -1;
    }

    private static EnumPermType PermofModule(int i)
    {
        switch (i)
        {
            case 0:
                return EnumPermType.AppointmentsModule;
            case 1:
                return EnumPermType.FamilyModule;
            case 2:
                return EnumPermType.AccountModule;
            case 3:
                return EnumPermType.TPModule;
            case 4:
                return EnumPermType.ChartModule;
            case 5:
                return EnumPermType.ImagingModule;
            case 6:
                return EnumPermType.ManageModule;
        }

        return EnumPermType.None;
    }

    public static void SyncCurUser()
    {
        if (CurUser == null || CurUser.UserNum == 0)
        {
            //Usernum will be 0 for users instantiated for web. See InitWebcore.Init.
            return;
        }

        //Update CurUser with the user from the cache synchronizing any fields that could have been updated.  E.g. TaskListInBox
        CurUser = Userods.GetFirstOrDefault(x => x.UserNum == CurUser.UserNum);
        //The user could have been deleted and/or data loss could have occurred and the CurUser is no longer in the db.
        if (CurUser == null)
        {
            throw new ODException("The current user has been removed from the cache.");
        }
    }

    public static void SetUserCurT(Userod userT)
    {
        if (userT != null)
        {
            _curUserT = userT;
        }
    }

    private static bool IsValidEServicePermission(EnumPermType perm)
    {
        if (CurUser == null)
        {
            return false;
        }

        //Run specific checks against certain types of eServices.
        switch (CurUser.EServiceType)
        {
            case EServiceTypes.Broadcaster:
            case EServiceTypes.BroadcastMonitor:
            case EServiceTypes.ServiceMainHQ:
                return true; //These eServices are at HQ and we trust ourselves to have full permissions for any S class method.
            case EServiceTypes.EConnector:
                return IsPermAllowedEConnector(perm);
            case EServiceTypes.OpenDentalService:
                return IsPermAllowedOpenDentalService(perm);
            case EServiceTypes.None:
            default:
                return false; //Not an eService, let IsAuthorized handle the permission checking.
        }
    }

    private static bool IsPermAllowedEConnector(EnumPermType perm)
    {
        //We are typically on the customers eConnector and need to be careful when giving access to certain permission types.
        //Engineers must EXCPLICITLY add permissions to this switch statement as they need them.
        //Be very cautious when adding permissions because the flood gates for that permission will be opened once added.
        //E.g. we should never add a permission like Setup or SecurityAdmin.  If there is a need for such a thing, we need to rethink this paradigm.
        switch (perm)
        {
            //Add additional permissions to this case as needed to grant access.
            case EnumPermType.EmailSend:
                return true;
            default:
                return false;
        }
    }

    private static bool IsPermAllowedOpenDentalService(EnumPermType perm)
    {
        //We need to be careful when giving access to certain permission types.
        //Engineers must EXCPLICITLY add permissions to this switch statement as they need them.
        //Be very cautious when adding permissions because the flood gates for that permission will be opened once added.
        //E.g. we should never add a permission like Setup or SecurityAdmin.  If there is a need for such a thing, we need to rethink this paradigm.
        switch (perm)
        {
            //Add additional permissions to this case as needed to grant access.
            case EnumPermType.EmailSend:
                return true;
            default:
                return false;
        }
    }
}