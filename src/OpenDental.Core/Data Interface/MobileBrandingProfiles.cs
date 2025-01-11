using System;
using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MobileBrandingProfiles
{
    #region Methods - Get

    public static MobileBrandingProfile GetByClinicNum(long clinicNum)
    {
        var command = "SELECT * FROM mobilebrandingprofile WHERE ClinicNum=" + clinicNum;
        return MobileBrandingProfileCrud.SelectOne(command);
    }

    #endregion

    #region Methods - Modify

    
    public static long Insert(MobileBrandingProfile mobileBrandingProfile)
    {
        return MobileBrandingProfileCrud.Insert(mobileBrandingProfile);
    }

    
    public static void Update(MobileBrandingProfile mobileBrandingProfile)
    {
        MobileBrandingProfileCrud.Update(mobileBrandingProfile);
    }

    
    public static void Delete(long mobileBrandingProfileNum)
    {
        MobileBrandingProfileCrud.Delete(mobileBrandingProfileNum);
    }

    /// <summary>
    ///     Deletes clinics MBP if they are using default. creates a duplicate of default MBP if they are not using
    ///     default, the MBP is null and the clinic has changes.
    /// </summary>
    public static void SynchMobileBrandingProfileClinicDefaults(List<long> listClinicNumsChanged)
    {
        var mobileBrandingProfileDefault = GetByClinicNum(0);
        for (var i = 0; i < listClinicNumsChanged.Count; i++)
        {
            var mobileBrandingProfile = GetByClinicNum(listClinicNumsChanged[i]);
            //try fetching the updated version
            var doUseEClipbardDefaultsforClinic = ClinicPrefs.GetBool(PrefName.EClipboardUseDefaults, listClinicNumsChanged[i]);
            if (doUseEClipbardDefaultsforClinic && mobileBrandingProfile != null)
            {
                //Was changed, and pref does not exist, or is true.
                //Defaults are in use.
                //Delete MBP if it exists.
                Delete(mobileBrandingProfile.MobileBrandingProfileNum);
                continue;
            }

            //Was changed, pref exists, and is false
            //Defaults are not in use.
            //Create a copy of the default MBP if no MBP exists, otherwise update existing to match default.
            if (mobileBrandingProfile == null && mobileBrandingProfileDefault != null)
            {
                mobileBrandingProfile = new MobileBrandingProfile();
                mobileBrandingProfile.ClinicNum = listClinicNumsChanged[i];
                mobileBrandingProfile.OfficeDescription = mobileBrandingProfileDefault.OfficeDescription;
                mobileBrandingProfile.LogoFilePath = mobileBrandingProfileDefault.LogoFilePath;
                mobileBrandingProfile.DateTStamp = DateTime.Now;
                mobileBrandingProfile.MobileBrandingProfileNum = Insert(mobileBrandingProfile);
            }
            //No else. In the case that MBP exists, the user created it after unchecking use defaults, so retain their changes.
        }
    }

    #endregion
}