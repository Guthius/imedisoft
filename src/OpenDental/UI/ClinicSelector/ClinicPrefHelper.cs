using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace OpenDental;

public class ClinicPrefHelper
{
    private readonly List<ClinicPref> _clinicPrefs = [];
    private readonly List<PrefName> _prefNames = [];

    public ClinicPrefHelper(params PrefName[] prefNames)
    {
        var clinicDtos = Clinics.GetForUserod(Security.CurUser, doIncludeHQ: true);
        
        clinicDtos.Add(Clinics.GetPracticeAsClinicZero());
        
        foreach (var prefName in prefNames.Distinct())
        {
            foreach (var clinicDto in clinicDtos.Distinct())
            {
                _clinicPrefs.Add(new ClinicPref
                {
                    PrefName = prefName,
                    ClinicNum = clinicDto.Id,
                    ValueString = ClinicPrefs.GetPrefValue(prefName, clinicDto.Id)
                });
            }

            _prefNames.Add(prefName);
        }
    }

    public void ValChangedByUser(PrefName prefName, long clinicNum, string newVal)
    {
        if (clinicNum < 0)
        {
            return;
        }

        var clinicPref = _clinicPrefs.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName);
        if (clinicPref is null)
        {
            clinicPref = new ClinicPref
            {
                PrefName = prefName,
                ClinicNum = clinicNum
            };
            
            _clinicPrefs.Add(clinicPref);
        }

        clinicPref.ValueString = newVal;
    }

    public bool GetBoolVal(PrefName prefName, long clinicNum)
    {
        if (clinicNum < 0)
        {
            return false;
        }

        return SIn.Bool(_clinicPrefs.Any(x => x.ClinicNum == clinicNum && x.PrefName == prefName) 
            ? _clinicPrefs.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName)?.ValueString 
            : _clinicPrefs.FirstOrDefault(x => x.ClinicNum == 0 && x.PrefName == prefName)?.ValueString);
    }

    public string GetStringVal(PrefName prefName, long clinicNum)
    {
        if (clinicNum < 0)
        {
            return "";
        }

        return _clinicPrefs.Any(x => x.ClinicNum == clinicNum && x.PrefName == prefName) 
            ? _clinicPrefs.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName)?.ValueString 
            : _clinicPrefs.FirstOrDefault(x => x.ClinicNum == 0 && x.PrefName == prefName)?.ValueString;
    }

    public void SyncAllPrefs()
    {
        foreach (var prefName in _prefNames)
        {
            SyncPref(prefName);
        }
    }

    public bool SyncPref(PrefName prefName)
    {
        var hqValue = _clinicPrefs.First(x => x.ClinicNum == 0 && x.PrefName == prefName).ValueString;
        
        var saved = prefName.Update(hqValue);
        
        _clinicPrefs.RemoveAll(x => x.ClinicNum != 0 && x.PrefName == prefName && x.ValueString.Equals(hqValue));
        
        var nonDefaultClinicPrefs = _clinicPrefs.FindAll(x => x.ClinicNum > 0 && x.PrefName == prefName);
        if (ClinicPrefs.Sync(nonDefaultClinicPrefs, ClinicPrefs.GetPrefAllClinics(prefName)))
        {
            saved = true;
        }

        if (!saved)
        {
            return false;
        }
        
        Signalods.SetInvalid(InvalidType.ClinicPrefs);
        ClinicPrefs.RefreshCache();

        return true;
    }
}