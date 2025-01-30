using System;
using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class MobileBrandingProfiles
{
    public static MobileBrandingProfile GetByClinicNum(long clinicNum)
    {
        return MobileBrandingProfileCrud.SelectOne("SELECT * FROM mobilebrandingprofile WHERE ClinicNum=" + clinicNum);
    }

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

    public static void SynchMobileBrandingProfileClinicDefaults(List<long> clinicNumsChanged)
    {
        var mobileBrandingProfileDefault = GetByClinicNum(0);

        foreach (var clinicNum in clinicNumsChanged)
        {
            var mobileBrandingProfile = GetByClinicNum(clinicNum);

            var useEClipbardDefaultsforClinic = ClinicPrefs.GetBool(PrefName.EClipboardUseDefaults, clinicNum);
            if (useEClipbardDefaultsforClinic && mobileBrandingProfile != null)
            {
                Delete(mobileBrandingProfile.MobileBrandingProfileNum);
                continue;
            }

            if (mobileBrandingProfile != null || mobileBrandingProfileDefault == null)
            {
                continue;
            }

            mobileBrandingProfile = new MobileBrandingProfile
            {
                ClinicNum = clinicNum,
                OfficeDescription = mobileBrandingProfileDefault.OfficeDescription,
                LogoFilePath = mobileBrandingProfileDefault.LogoFilePath,
                DateTStamp = DateTime.Now
            };

            mobileBrandingProfile.MobileBrandingProfileNum = Insert(mobileBrandingProfile);
        }
    }
}