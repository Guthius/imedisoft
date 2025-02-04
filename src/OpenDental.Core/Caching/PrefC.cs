using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Caching;

public class PrefC
{
    public static DateTime DateClaimReceivedAfter
    {
        get
        {
            var date = DateTime.MinValue;

            var days = GetInt(PrefName.ClaimPaymentNoShowZeroDate);
            if (days >= 0)
            {
                date = DateTime.Today.AddDays(-days);
            }

            return date;
        }
    }

    public static bool IsAgingAllowedToStart()
    {
        var dateTime = GetDateT(PrefName.AgingBeginDateTime);
        
        return dateTime == DateTime.MinValue || DateTime_.Now >= dateTime.AddHours(24);
    }

    public static string PatientCommunicationDateFormat
    {
        get
        {
            var format = GetString(PrefName.PatientCommunicationDateFormat);
            try
            {
                _ = DateTime.Today.ToString(format);
            }
            catch
            {
                format = "d";
            }

            return format;
        }
    }

    public static string PatientCommunicationTimeFormat
    {
        get
        {
            var format = GetString(PrefName.PatientCommunicationTimeFormat);
            try
            {
                _ = DateTime.Now.ToString(format);
            }
            catch
            {
                format = "t";
            }

            return format;
        }
    }

    public static long GetLong(PrefName prefName)
    {
        return SIn.Long(Prefs.GetOne(prefName).ValueString);
    }

    public static int GetInt(PrefName prefName)
    {
        return SIn.Int(Prefs.GetOne(prefName).ValueString);
    }

    public static byte GetByte(PrefName prefName)
    {
        return SIn.Byte(Prefs.GetOne(prefName).ValueString);
    }

    public static double GetDouble(PrefName prefName)
    {
        return SIn.Double(Prefs.GetOne(prefName).ValueString);
    }

    public static double GetDouble(PrefName prefName, bool doUseEnUsFormat)
    {
        return SIn.Double(Prefs.GetOne(prefName).ValueString, doUseEnUsFormat);
    }

    public static bool GetBool(PrefName prefName)
    {
        return SIn.Bool(Prefs.GetOne(prefName).ValueString);
    }

    public static bool GetYn(PrefName prefName)
    {
        var yn = (YN) SIn.Int(Prefs.GetOne(prefName).ValueString);
        switch (yn)
        {
            case YN.Yes:
                return true;

            case YN.No:
                return false;
        }

        var prefValueType = prefName.GetValueType();

        return prefValueType switch
        {
            PrefValueType.YN_DEFAULT_FALSE => false,
            PrefValueType.YN_DEFAULT_TRUE => true,
            _ => throw new ArgumentException("Invalid type")
        };
    }

    public static CheckState GetYnCheckState(PrefName prefName)
    {
        var yn = (YN) SIn.Int(Prefs.GetOne(prefName).ValueString);

        return yn switch
        {
            YN.Yes => CheckState.Checked,
            YN.No => CheckState.Unchecked,
            _ => CheckState.Indeterminate
        };
    }

    public static T GetEnum<T>(PrefName prefName) where T : struct, Enum
    {
        return SIn.Enum<T>(GetInt(prefName));
    }

    public static bool GetBoolSilent(PrefName prefName, bool silentDefault)
    {
        if (Prefs.DictIsNull())
        {
            return silentDefault;
        }

        Pref pref = null;
        ODException.SwallowAnyException(() => { pref = Prefs.GetOne(prefName); });
        return pref == null ? silentDefault : SIn.Bool(pref.ValueString);
    }

    public static string GetString(PrefName prefName)
    {
        return Prefs.GetOne(prefName).ValueString;
    }

    public static string GetStringNoCache(PrefName prefName)
    {
        return DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName='" + SOut.String(prefName.ToString()) + "'");
    }

    public static string GetStringSilent(PrefName prefName)
    {
        if (Prefs.DictIsNull())
        {
            return "";
        }

        Pref pref = null;

        ODException.SwallowAnyException(() => { pref = Prefs.GetOne(prefName); });
        return pref == null ? "" : pref.ValueString;
    }

    public static DateTime GetDate(PrefName prefName)
    {
        return SIn.Date(Prefs.GetOne(prefName).ValueString);
    }

    public static DateTime GetDateT(PrefName prefName)
    {
        return SIn.DateTime(Prefs.GetOne(prefName).ValueString);
    }

    public static Color GetColor(PrefName prefName)
    {
        return Color.FromArgb(SIn.Int(Prefs.GetOne(prefName).ValueString));
    }

    public static string GetRaw(string prefName)
    {
        return Prefs.GetOne(prefName).ValueString;
    }

    public static bool MakeIncomeTransferOnClaimReceived()
    {
        var incomeTransfersMadeUponClaimReceived = GetEnum<YN>(PrefName.IncomeTransfersMadeUponClaimReceived);
        if (incomeTransfersMadeUponClaimReceived != YN.Unknown)
        {
            return incomeTransfersMadeUponClaimReceived == YN.Yes;
        }

        var rigorousAccounting = GetEnum<YN>(PrefName.RigorousAccounting);

        return rigorousAccounting switch
        {
            YN.Unknown => true,
            YN.Yes or YN.No => false,
            _ => false
        };
    }

    public static CultureInfo GetLanguageAndRegion()
    {
        var cultureInfo = CultureInfo.CurrentCulture;

        ODException.SwallowAnyException(() =>
        {
            var pref = Prefs.GetOne("LanguageAndRegion");
            if (!string.IsNullOrEmpty(pref.ValueString))
            {
                cultureInfo = CultureInfo.GetCultureInfo(pref.ValueString);
            }
        });

        return cultureInfo;
    }

    public static bool HasOnlinePaymentEnabled(out ProgramName progEnabledForPayments, bool isForMobile = false)
    {
        progEnabledForPayments = ProgramName.None;

        var progXCharge = Programs.GetCur(ProgramName.Xcharge);
        var progEdgeExpress = Programs.GetCur(ProgramName.EdgeExpress);
        var progPayConnect = Programs.GetCur(ProgramName.PayConnect);
        var progPaySimple = Programs.GetCur(ProgramName.PaySimple);

        if (progEdgeExpress.Enabled)
        {
            var listEdgeExpressProps = ProgramProperties.GetForProgram(progEdgeExpress.ProgramNum);
            if (listEdgeExpressProps.Exists(x => x.PropertyDesc == ProgramProperties.PropertyDescs.EdgeExpress.IsOnlinePaymentsEnabled && x.PropertyValue == "1"))
            {
                progEnabledForPayments = ProgramName.EdgeExpress;
                return true;
            }
        }

        if (progXCharge.Enabled)
        {
            var listXChargeProps = ProgramProperties.GetForProgram(progXCharge.ProgramNum);
            if (listXChargeProps.Exists(x => x.PropertyDesc == "IsOnlinePaymentsEnabled" && x.PropertyValue == "1"))
            {
                progEnabledForPayments = ProgramName.Xcharge;
                return true;
            }
        }

        if (progPayConnect.Enabled)
        {
            var payConnectProperties = ProgramProperties.GetForProgram(progPayConnect.ProgramNum);
            if (payConnectProperties.Exists(x => x.PropertyDesc == PayConnect.ProgramProperties.PatientPortalPaymentsEnabled && x.PropertyValue == "1"))
            {
                progEnabledForPayments = ProgramName.PayConnect;
                return true;
            }
        }

        if (progPaySimple.Enabled && !isForMobile)
        {
            var paySimpleProperties = ProgramProperties.GetForProgram(progPaySimple.ProgramNum);
            if (!paySimpleProperties.Exists(x => x.PropertyDesc == PaySimple.PropertyDescs.PaySimpleIsOnlinePaymentsEnabled && x.PropertyValue == "1"))
            {
                return false;
            }

            progEnabledForPayments = ProgramName.PaySimple;
            return true;
        }

        return false;
    }

    public static bool HListIsNull()
    {
        return Prefs.DictIsNull();
    }

    public static bool IsTreatPlanSortByTooth { get; set; }

    public static string GetTempFolderPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "opendental");
        if (Directory.Exists(path))
        {
            return path;
        }

        Directory.CreateDirectory(path);

        if (DateTime.Today > GetDate(PrefName.TempFolderDateFirstCleaned).AddMonths(1))
        {
            return path;
        }

        var fileNames = Directory.GetFiles(Path.GetTempPath());

        foreach (var fileName in fileNames)
        {
            try
            {
                if (fileName.Substring(fileName.LastIndexOf('.')) == ".exe" ||
                    fileName.Substring(fileName.LastIndexOf('.')) == ".cs")
                {
                }
                else
                {
                    File.Delete(fileName);
                }
            }
            catch
            {
                // ignored
            }
        }

        return path;
    }

    public static string GetRandomTempFile(string ext)
    {
        return ODFileUtils.CreateRandomFile(GetTempFolderPath(), ext);
    }

    public static long GetDefaultSheetDefNum(SheetTypeEnum sheetType)
    {
        return GetLong(Prefs.GetSheetDefPref(sheetType));
    }

    public static int LogOffTimer
    {
        get
        {
            if (Security.CurUser != null && GetBool(PrefName.SecurityLogOffAllowUserOverride))
            {
                var userOverride = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.LogOffTimerOverride).FirstOrDefault();
                if (userOverride != null)
                {
                    if (!int.TryParse(userOverride.ValueString, out var logOffMins))
                    {
                        throw new ODException($"Invalid LogOffTimerOverride set for user.\r\n"
                                              + $"UserNum: {Security.CurUser.UserNum}\r\n"
                                              + $"ValueString: {userOverride.ValueString}");
                    }

                    return logOffMins;
                }
            }

            return GetInt(PrefName.SecurityLogOffAfterMinutes);
        }
    }

    public static string GetFirstShortUrl(string msgBodyText)
    {
        if (string.IsNullOrWhiteSpace(msgBodyText))
        {
            return "";
        }

        return GetString(PrefName.RedirectShortURLsFromHQ).Split(',').ToList().Find(msgBodyText.Contains) ?? "";
    }
}