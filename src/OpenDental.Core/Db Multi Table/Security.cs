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

    public static bool IsUserLoggedIn;
    public static DateTime DateTimeLastActivity;

    public static Userod CurUser
    {
        get => _curUserT ?? curUser;
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
        get { return _curComputerName ??= Environment.MachineName; }
        set => _curComputerName = value;
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
            var msg = "Not authorized for\r\n" + GroupPermissions.GetDesc(perm);
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
        date = date.Date;

        string errorMsg;
        if (!GroupPermissions.HasPermission(curUser, perm, fKey))
        {
            errorMsg = "Not authorized.\r\n" +
                       "A user with the SecurityAdmin permission must grant you access for:\r\n"
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
        if (!new[]
            {
                EnumPermType.AdjustmentCreate,
                EnumPermType.AdjustmentEdit,
                EnumPermType.PaymentCreate,
                EnumPermType.PaymentEdit,
                EnumPermType.ProcComplCreate,
                EnumPermType.ProcCompleteEdit,
                EnumPermType.ProcCompleteStatusEdit,
                EnumPermType.InsPayCreate,
                EnumPermType.InsPayEdit,
                EnumPermType.SheetEdit,
                EnumPermType.SheetDelete,
                EnumPermType.CommlogEdit,
                EnumPermType.PayPlanEdit
            }.Contains(perm))
        {
            return false;
        }

        if (date.Year == 1)
        {
            return false;
        }

        if (!PrefC.GetBool(PrefName.SecurityLockIncludesAdmin) && GroupPermissions.HasPermission(CurUser, EnumPermType.SecurityAdmin, 0))
        {
            return false;
        }

        var permissionsCanBypassLockDate = new List<EnumPermType>
        {
            EnumPermType.ProcCompleteEdit,
            EnumPermType.ProcCompleteAddAdj,
            EnumPermType.ProcCompleteEditMisc,
            EnumPermType.ProcCompleteStatusEdit,
            EnumPermType.ProcCompleteNote,
            EnumPermType.ProcComplCreate,
            EnumPermType.ProcExistingEdit
        };

        if (permissionsCanBypassLockDate.Contains(perm) && ProcedureCodes.CanBypassLockDate(codeNum, procFee))
        {
            return false;
        }

        if (perm is EnumPermType.SheetEdit or EnumPermType.SheetDelete && sheetDefNum > 0 && SheetDefs.CanBypassLockDate(sheetDefNum))
        {
            return false;
        }

        string msg;
        if (date <= PrefC.GetDate(PrefName.SecurityLockDate))
        {
            msg = "Locked by Administrator before " + PrefC.GetDate(PrefName.SecurityLockDate).ToShortDateString();
            if (!suppressMsgBox)
            {
                MessageBox.Show(msg);
            }

            actionNotAuthorized?.Invoke(msg);
            return true;
        }

        var lockDays = PrefC.GetInt(PrefName.SecurityLockDays);
        if (lockDays <= 0 || date > DateTime.Today.AddDays(-lockDays))
        {
            return false;
        }

        msg = "Locked by Administrator before " + lockDays + " days.";
        if (!suppressMsgBox)
        {
            MessageBox.Show(msg);
        }

        actionNotAuthorized?.Invoke(msg);
        return true;
    }

    private static DateTime GetDateLimit(EnumPermType permType, List<long> userGroupNums)
    {
        return GroupPermissions.GetDateRestrictedForPermission(permType, userGroupNums);
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
        return i switch
        {
            0 => EnumPermType.AppointmentsModule,
            1 => EnumPermType.FamilyModule,
            2 => EnumPermType.AccountModule,
            3 => EnumPermType.TPModule,
            4 => EnumPermType.ChartModule,
            5 => EnumPermType.ImagingModule,
            6 => EnumPermType.ManageModule,
            _ => EnumPermType.None
        };
    }

    public static void SyncCurUser()
    {
        if (CurUser == null || CurUser.UserNum == 0)
        {
            return;
        }

        CurUser = Userods.GetFirstOrDefault(x => x.UserNum == CurUser.UserNum);

        if (CurUser == null)
        {
            throw new ODException("The current user has been removed from the cache.");
        }
    }
}