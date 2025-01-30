using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;
using PdfSharp.Pdf;

namespace OpenDentBusiness;

public class MobileNotifications
{
    public static void Insert(MobileNotification mobileNotification)
    {
        MobileNotificationCrud.Insert(mobileNotification);
    }
    
    public static void CI_CheckinPatient(long patNum, long mobileAppDeviceNum)
    {
        var patient = Patients.GetPat(patNum);
        if (patient == null) return;
        //See CI_CheckinPatient for input details.
        var listTags = new List<string> {patient.FName, patient.LName, patient.Birthdate.Ticks.ToString(), patNum.ToString()};
        InsertMobileNotification(MobileNotificationType.CI_CheckinPatient, mobileAppDeviceNum, EnumAppTarget.eClipboard, listTags: listTags);
    }

    public static void CI_AddSheet(long patNum, long sheetNum)
    {
        var patient = Patients.GetPat(patNum);
        if (patient == null) return;
        var listMobileAppDevices = MobileAppDevices.GetAll(patNum);
        //Remove all devices that belong to a clinic that does not allow patients to fill out forms in eClipboard.
        listMobileAppDevices.RemoveAll(x => !ClinicPrefs.GetBool(PrefName.EClipboardPresentAvailableFormsOnCheckIn, x.ClinicNum));
        if (listMobileAppDevices.IsNullOrEmpty()) return;
        var sheet = Sheets.GetOne(sheetNum);
        if (sheet == null) return;
        //See CI_AddSheet for input details.
        for (var i = 0; i < listMobileAppDevices.Count; i++)
        {
            var listPrimaryKeys = new List<long> {patient.PatNum, sheet.SheetNum};
            InsertMobileNotification(MobileNotificationType.CI_AddSheet, listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys);
        }
    }

    public static void CI_AddEForm(long patNum, long eFormNum)
    {
        var patient = Patients.GetPat(patNum);
        if (patient == null) return;
        var listMobileAppDevices = MobileAppDevices.GetAll(patNum);
        if (listMobileAppDevices.IsNullOrEmpty()) return;
        var eForm = EForms.GetEForm(eFormNum);
        if (eForm == null) return;
        for (var i = 0; i < listMobileAppDevices.Count; i++)
        {
            var listPrimaryKeys = new List<long> {patNum, eFormNum};
            InsertMobileNotification(MobileNotificationType.CI_AddEForm, listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys);
        }
    }

    public static void CI_RemoveSheet(long patNum, long sheetNum)
    {
        var patient = Patients.GetPat(patNum);
        //Intentionally not getting the actual Sheet from the db here. It may have already been deleted, now time to get it off the device.
        if (patient == null) return;
        var listMobileAppDevices = MobileAppDevices.GetAll(patNum);
        if (listMobileAppDevices.IsNullOrEmpty()) return;
        //See CI_RemoveSheet for input details.
        for (var i = 0; i < listMobileAppDevices.Count; i++)
        {
            var listPrimaryKeys = new List<long> {patient.PatNum, sheetNum};
            InsertMobileNotification(MobileNotificationType.CI_RemoveSheet, listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys);
        }
    }

    public static void CI_RemoveEForm(long patNum, long eFormNum)
    {
        var patient = Patients.GetPat(patNum);
        //Intentionally not getting the actual eForm from the db here. It may have already been deleted, now time to get it off the device.
        if (patient == null) return;
        var listMobileAppDevices = MobileAppDevices.GetAll(patNum);
        if (listMobileAppDevices.IsNullOrEmpty()) return;
        for (var i = 0; i < listMobileAppDevices.Count; i++)
        {
            var listPrimaryKeys = new List<long> {patient.PatNum, eFormNum};
            InsertMobileNotification(MobileNotificationType.CI_RemoveEForm, listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys);
        }
    }

    public static void CI_GoToCheckin(long mobileAppDeviceNum)
    {
        //See CI_GoToCheckin for input details.
        InsertMobileNotification(MobileNotificationType.CI_GoToCheckin, mobileAppDeviceNum, EnumAppTarget.eClipboard);
    }

    public static void CI_NewEClipboardPrefs(long clinicNum)
    {
        var allowSelfCheckin = SOut.Bool(ClinicPrefs.GetBool(PrefName.EClipboardAllowSelfCheckIn, clinicNum));
        var eClipboardMessageComplete = ClinicPrefs.GetPrefValue(PrefName.EClipboardMessageComplete, clinicNum);
        var allowSelfPortrait = SOut.Bool(EClipboardImageCaptureDefs.Refresh().Any(x => x.ClinicNum == clinicNum && x.IsSelfPortrait));
        var showAvailableFormsOnCheckin = SOut.Bool(ClinicPrefs.GetBool(PrefName.EClipboardPresentAvailableFormsOnCheckIn, clinicNum));
        var multiPageCheckin = SOut.Bool(ClinicPrefs.GetBool(PrefName.EClipboardHasMultiPageCheckIn, clinicNum));
        var allowPaymentOnCheckin = SOut.Bool(ClinicPrefs.GetBool(PrefName.EClipboardAllowPaymentOnCheckin, clinicNum));
        var listMobileAppDevices = MobileAppDevices.GetForClinic(clinicNum);
        for (var i = 0; i < listMobileAppDevices.Count; i++)
        {
            var listTags = new List<string>();
            listTags.Add(allowSelfCheckin);
            listTags.Add(eClipboardMessageComplete);
            listTags.Add(allowSelfPortrait);
            listTags.Add(showAvailableFormsOnCheckin);
            listTags.Add(multiPageCheckin);
            listTags.Add(allowPaymentOnCheckin);
            //See CI_NewEClipboardPrefs for input details.
            InsertMobileNotification(MobileNotificationType.CI_NewEClipboardPrefs, listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.eClipboard,
                listTags: listTags);
        }
    }

    public static void CI_SendTreatmentPlan(PdfDocument pdfDocument, TreatPlan treatPlan, bool hasPracticeSig, long mobileAppDeviceNum)
    {
        long mobileDataByteNum = -1;
        var listTags = new List<string> {treatPlan.Heading, hasPracticeSig.ToString(), treatPlan.DateTP.Ticks.ToString()};
        mobileDataByteNum = MobileDataBytes.InsertPDF(pdfDocument, treatPlan.PatNum, null, eActionType.TreatmentPlan, listTags);
        //update the treatment plan MobileAppDeviceNum so we now it is on an device
        TreatPlans.UpdateMobileAppDeviceNum(treatPlan, mobileAppDeviceNum);
        Signalods.SetInvalid(InvalidType.TPModule, KeyType.PatNum, treatPlan.PatNum);
        Signalods.SetInvalid(InvalidType.EClipboard);
        var listPrimaryKeys = new List<long> {mobileDataByteNum, treatPlan.PatNum, treatPlan.TreatPlanNum};
        InsertMobileNotification(MobileNotificationType.CI_TreatmentPlan, mobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys, listTags);
    }

    public static void CI_RemoveTreatmentPlan(long mobileAppDeviceNum, TreatPlan treatPlan)
    {
        var listPrimaryKeys = new List<long> {treatPlan.PatNum, treatPlan.TreatPlanNum};
        InsertMobileNotification(MobileNotificationType.CI_RemoveTreatmentPlan, mobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys);
        //TreatPlanParams are only inserted into the database if this pref is true
        if (PrefC.GetBool(PrefName.TreatPlanSaveSignedToPdf)) TreatPlanParams.DeleteByTreatPlanNum(treatPlan.TreatPlanNum);
        //Treatment plan is being removed from device, so the MobileAppDeviceNum needs to be 0
        TreatPlans.UpdateMobileAppDeviceNum(treatPlan, 0);
        Signalods.SetInvalid(InvalidType.TPModule, KeyType.PatNum, treatPlan.PatNum);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    public static string CI_SendPayment(long mobileAppDeviceNum, long patNum)
    {
        var listPrimaryKeys = ListTools.FromSingle(patNum);
        var errMsg = "";
        try
        {
            InsertMobileNotification(MobileNotificationType.CI_SendPayment, mobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys);
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }

        return errMsg;
    }

    public static void CI_RefreshPayment(long mobileAppDeviceNum, long patNum)
    {
        var errMsg = "";
        try
        {
            InsertMobileNotification(MobileNotificationType.CI_RefreshPayment, mobileAppDeviceNum, EnumAppTarget.eClipboard, new List<long> {patNum});
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }
    }

    public static void CI_SendPaymentPlan(PdfDocument pdfDocument, PayPlan payPlan, long mobileAppDeviceNum)
    {
        long mobileDataByteNum = -1;
        var patient = Patients.GetPat(payPlan.PatNum);
        var listTags = new List<string> {payPlan.PlanNum.ToString(), payPlan.PayPlanDate.Ticks.ToString(), patient.GetNameFirstOrPrefL()};
        mobileDataByteNum = MobileDataBytes.InsertPDF(pdfDocument, payPlan.PatNum, null, eActionType.PaymentPlan, listTags);
        //update the payment plan MobileAppDeviceNum so we now it is on an device
        PayPlans.UpdateMobileAppDeviceNum(payPlan, mobileAppDeviceNum);
        Signalods.SetInvalid(InvalidType.AccModule, KeyType.PatNum, payPlan.PatNum);
        Signalods.SetInvalid(InvalidType.EClipboard);
        var listPrimaryKeys = new List<long> {mobileDataByteNum, payPlan.PatNum, payPlan.PayPlanNum};
        InsertMobileNotification(MobileNotificationType.CI_PaymentPlan, mobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys, listTags);
    }

    public static void CI_RemovePaymentPlan(long mobileAppDeviceNum, PayPlan payPlan)
    {
        var listPrimaryKeys = new List<long> {payPlan.PatNum, payPlan.PayPlanNum};
        InsertMobileNotification(MobileNotificationType.CI_RemovePaymentPlan, mobileAppDeviceNum, EnumAppTarget.eClipboard, listPrimaryKeys);
        //Treatment plan is being removed from device, so the MobileAppDeviceNum needs to be 0
        PayPlans.UpdateMobileAppDeviceNum(payPlan, 0);
        Signalods.SetInvalid(InvalidType.AccModule, KeyType.PatNum, payPlan.PatNum);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }
    
    public static void ODM_LogoutClinic(long clinicNum)
    {
        var listMobileAppDevices = MobileAppDevices.GetForClinic(clinicNum);
        for (var i = 0; i < listMobileAppDevices.Count; i++)
            //See ODM_LogoutODUser for input details.
            InsertMobileNotification(MobileNotificationType.ODM_LogoutODUser, listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.ODMobile);
    }

    public static string ODT_ExamSheet(long patNum, long sheetNum, long mobileAppDeviceNum)
    {
        var listPrimaryKeys = new List<long> {patNum, sheetNum};
        var errMsg = "";
        try
        {
            InsertMobileNotification(MobileNotificationType.ODT_ExamSheet, mobileAppDeviceNum, EnumAppTarget.ODTouch, listPrimaryKeys);
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }

        return errMsg;
    }
    
    public static void IsAllowedChanged(long mobileAppDeviceNum, EnumAppTarget enumAppTarget, bool isAllowed)
    {
        var listTags = ListTools.FromSingle(SOut.Bool(isAllowed));
        //See CI_IsAllowedChanged for input details.
        InsertMobileNotification(MobileNotificationType.IsAllowedChanged, mobileAppDeviceNum, enumAppTarget, listTags: listTags);
    }

    public static void InsertPrintError(long mobileAppDeviceNum, EnumAppTarget enumAppTarget, string message)
    {
        var listTags = ListTools.FromSingle(message);
        InsertMobileNotification(MobileNotificationType.ODT_PrintError, mobileAppDeviceNum, enumAppTarget, listTags: listTags);
    }
    
    public static void InsertMobileNotification(MobileNotificationType mobileNotificationType, long mobileAppDeviceNum, EnumAppTarget enumAppTarget, List<long> listPrimaryKeys = null, List<string> listTags = null)
    {
        var mobileNotification = new MobileNotification();
        mobileNotification.NotificationType = mobileNotificationType;
        mobileNotification.PrimaryKeys = JsonConvert.SerializeObject(listPrimaryKeys ?? new List<long>());
        mobileNotification.Tags = JsonConvert.SerializeObject(listTags ?? new List<string>());
        mobileNotification.AppTarget = enumAppTarget;
        var mobileAppDevice = MobileAppDevices.GetOne(mobileAppDeviceNum);
        if (mobileAppDevice == null) throw new Exception("MobileAppDeviceNum not found: " + mobileAppDeviceNum);
        mobileNotification.DeviceId = mobileAppDevice.UniqueID;
        //Validate that this clinic is signed up for eClipboard if the mobile notification is related to eClipboard
        if (enumAppTarget == EnumAppTarget.eClipboard && !MobileAppDevices.IsClinicSignedUpForEClipboard(mobileAppDevice.ClinicNum)) throw new Exception($"ClinicNum {mobileAppDevice.ClinicNum} is not signed up for eClipboard.");
        //Validate that this clinic is signed up for MobileWeb if the mobile notification is related to ODMobile
        if (enumAppTarget == EnumAppTarget.ODMobile && !MobileAppDevices.IsClinicSignedUpForMobileWeb(mobileAppDevice.ClinicNum))
        {
            if (mobileNotificationType != MobileNotificationType.ODM_LogoutODUser) //Logout is allowed to be sent to non-specific clinicNum. All others are not.
                throw new Exception($"ClinicNum {mobileAppDevice.ClinicNum} is not signed up for ODMobile.");
        }
        else if (enumAppTarget == EnumAppTarget.ODTouch && !ClinicPrefs.IsOdTouchAllowed(mobileAppDevice.ClinicNum))
        {
            throw new Exception($"ClinicNum {mobileAppDevice.ClinicNum} is not signed up for ODTouch.");
        }

        mobileNotification.DateTimeEntry = DateTime_.Now;
        mobileNotification.DateTimeExpires = DateTime_.Now.AddMinutes(10);
        Insert(mobileNotification);
    }
}