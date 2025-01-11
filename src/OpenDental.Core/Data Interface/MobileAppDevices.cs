using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MobileAppDevices
{
    ///<summary>For any device whose clinic num is not in the list passed in, sets IsAllowed to false.</summary>
    public static void UpdateIsEClipboardAllowed(List<long> listClinicNums)
    {
        var command = "UPDATE mobileappdevice SET IsEclipboardEnabled=0";
        if (!listClinicNums.IsNullOrEmpty()) command += " WHERE ClinicNum NOT IN(" + string.Join(",", listClinicNums) + ")";
        Db.NonQ(command);
    }

    ///<summary>Returns true if this PatNum is currently linked to a MobileAppDevice row.</summary>
    public static bool PatientIsAlreadyUsingDevice(long patNum)
    {
        var command = $"SELECT COUNT(*) FROM mobileappdevice WHERE PatNum={SOut.Long(patNum)}";
        return SIn.Long(Db.GetCount(command)) > 0;
    }

    ///<summary>Returns true if this clinicNum is subscribed for eClipboard.</summary>
    public static bool IsClinicSignedUpForEClipboard(long clinicNum)
    {
        return PrefC.GetString(PrefName.EClipboardClinicsSignedUp)
            .Split(',')
            .Where(x => x != "")
            .Select(x => SIn.Long(x))
            .Any(x => x == clinicNum);
    }

    ///<summary>Returns true if this clinicNum is subscribed for MobileWeb.</summary>
    public static bool IsClinicSignedUpForMobileWeb(long clinicNum)
    {
        return GetClinicSignedUpForMobileWeb().Any(x => x == clinicNum);
    }

    /// <summary>
    ///     Returns list of ClinicNum(s) which are currently subscribed for MobileWeb.
    ///     Will include ClinicNum 0 if not using clinics and practice is enabled.
    /// </summary>
    public static List<long> GetClinicSignedUpForMobileWeb()
    {
        return PrefC.GetString(PrefName.MobileWebClinicsSignedUp)
            .Split(',')
            .Where(x => x != "")
            .Select(x => SIn.Long(x)).ToList();
    }

    ///<summary>Returns true if the primary key for the given keytype is set for a device, false otherwise.</summary>
    public static bool IsInUse(long patNum = 0, params MADPage[] madPagesArray)
    {
        var command = $"SELECT COUNT(*) FROM mobileappdevice WHERE DevicePage IN ({string.Join(",", madPagesArray.Select(x => SOut.Enum(x)).ToList())})";
        if (patNum > 0) command += $"AND PatNum={SOut.Long(patNum)} ";
        return SIn.Long(Db.GetCount(command)) > 0;
    }

    /// <summary>
    ///     Gets the most recently used device that has the passed in patient set. If UserNum is set or device does not
    ///     exist, returns null, should not create mobile notification. If no user is set or device row does not exist, create
    ///     mobile notification. Returns the MobileAppDevice object so that the caller does not have to do the same code to get
    ///     it. Centralizes all of the code needed to answer this question and provides the needed data to take action (ie.
    ///     creating a mobile notification for a given device). When the returned device is not null, we know it is safe to
    ///     use. If not creating a mobile notification no need for the device.
    /// </summary>
    public static MobileAppDevice ShouldCreateMobileNotification(long patNum)
    {
        var listMobileAppDevices = GetAll(patNum);
        var mobileAppDevice = listMobileAppDevices.OrderByDescending(x => x.LastCheckInActivity).FirstOrDefault();
        if (mobileAppDevice != null && mobileAppDevice.LastCheckInActivity > DateTime.Now.AddHours(-1) && mobileAppDevice.UserNum == 0) return mobileAppDevice;
        return null;
    }

    #region Get Methods

    ///<summary>Gets one MobileAppDevice from the database.</summary>
    public static MobileAppDevice GetOne(long mobileAppDeviceNum)
    {
        return MobileAppDeviceCrud.SelectOne(mobileAppDeviceNum);
    }

    /// <summary>
    ///     Gets all MobileAppDevices from the database. If patNum is provided then filters by patNum. PatNum of 0 get
    ///     unoccupied devices.
    /// </summary>
    public static List<MobileAppDevice> GetAll(long patNum = -1)
    {
        var command = "SELECT * FROM mobileappdevice";
        if (patNum > -1) command += $" WHERE PatNum={SOut.Long(patNum)}";
        return MobileAppDeviceCrud.SelectMany(command);
    }

    public static List<MobileAppDevice> GetForUser(Userod userod, bool doIncludeHQ = false)
    {
        var command = "SELECT * FROM mobileappdevice ";
        var listClinicsForUser = Clinics.GetForUserod(userod, doIncludeHQ);
        if (listClinicsForUser.Count == 0) return new List<MobileAppDevice>();
        command += $"WHERE ClinicNum in ({string.Join(",", listClinicsForUser.Select(x => x.Id))})";

        //Get valid BYOD devices and all other non BYOD MADs
        return MobileAppDeviceCrud.SelectMany(command).Where(x => !x.IsBYODDevice || (x.IsBYODDevice && x.PatNum != 0)).ToList();
    }

    public static MobileAppDevice GetForPat(long patNum, MADPage mADPage = MADPage.Undefined)
    {
        var command = $"SELECT * FROM mobileappdevice WHERE PatNum={SOut.Long(patNum)} ";
        if (mADPage != MADPage.Undefined) command = $"AND DevicePage={SOut.Enum(mADPage)} ";
        return MobileAppDeviceCrud.SelectOne(command);
    }

    public static List<MobileAppDevice> GetForClinic(long clinicNum)
    {
        var command = $"SELECT * FROM mobileappdevice WHERE ClinicNum={SOut.Long(clinicNum)}";
        return MobileAppDeviceCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Update

    public static void Update(MobileAppDevice mobileAppDevice)
    {
        MobileAppDeviceCrud.Update(mobileAppDevice);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    public static void Update(MobileAppDevice mobileAppDeviceNew, MobileAppDevice mobileAppDeviceOld)
    {
        MobileAppDeviceCrud.Update(mobileAppDeviceNew, mobileAppDeviceOld);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    ///<summary>Keeps MobileAppDevice table current so we know which patient is on which device and for how long.</summary>
    public static void SetPatNum(long mobileAppDeviceNum, long patNum)
    {
        string command;
        if (patNum == -1)
        {
            if ((GetOne(mobileAppDeviceNum) ?? new MobileAppDevice {IsBYODDevice = false}).IsBYODDevice)
            {
                MobileAppDeviceCrud.Delete(mobileAppDeviceNum);
                Signalods.SetInvalid(InvalidType.EClipboard);
                return;
            }

            command = "UPDATE mobileappdevice SET PatNum=" + SOut.Long(0) + ",LastCheckInActivity=" + SOut.DateT(DateTime.Now)
                      + " WHERE MobileAppDeviceNum=" + SOut.Long(mobileAppDeviceNum);
            Db.NonQ(command);
            Signalods.SetInvalid(InvalidType.EClipboard);
            return;
        }

        command = "UPDATE mobileappdevice SET PatNum=" + SOut.Long(patNum) + ",LastCheckInActivity=" + SOut.DateT(DateTime.Now)
                  + " WHERE MobileAppDeviceNum=" + SOut.Long(mobileAppDeviceNum);
        Db.NonQ(command);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    ///<summary>Syncs the two lists in the database.</summary>
    public static void Sync(List<MobileAppDevice> listMobileAppDevices, List<MobileAppDevice> listMobileAppDevicesDb)
    {
        if (MobileAppDeviceCrud.Sync(listMobileAppDevices, listMobileAppDevicesDb)) Signalods.SetInvalid(InvalidType.EClipboard);
    }

    #endregion Update

    #region Delete

    public static void Delete(MobileAppDevice mobileAppDevice)
    {
        if (IsClinicSignedUpForEClipboard(mobileAppDevice.ClinicNum)) MobileNotifications.IsAllowedChanged(mobileAppDevice.MobileAppDeviceNum, EnumAppTarget.eClipboard, false); //deleting so always false
        if (ClinicPrefs.IsODTouchAllowed(mobileAppDevice.ClinicNum)) MobileNotifications.IsAllowedChanged(mobileAppDevice.MobileAppDeviceNum, EnumAppTarget.ODTouch, false);
        MobileAppDeviceCrud.Delete(mobileAppDevice.MobileAppDeviceNum);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    public static void DeleteMany(List<MobileAppDevice> listMobileAppDevices)
    {
        if (listMobileAppDevices.IsNullOrEmpty()) return;

        for (var i = 0; i < listMobileAppDevices.Count; i++)
        {
            if (IsClinicSignedUpForEClipboard(listMobileAppDevices[i].ClinicNum)) MobileNotifications.IsAllowedChanged(listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.eClipboard, false); //deleting so always false
            if (ClinicPrefs.IsODTouchAllowed(listMobileAppDevices[i].ClinicNum)) MobileNotifications.IsAllowedChanged(listMobileAppDevices[i].MobileAppDeviceNum, EnumAppTarget.ODTouch, false); //deleting so always false
        }

        MobileAppDeviceCrud.DeleteMany(listMobileAppDevices.Select(x => x.MobileAppDeviceNum).ToList());
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    #endregion Delete
}