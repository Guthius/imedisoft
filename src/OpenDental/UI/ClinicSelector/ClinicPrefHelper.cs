using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
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
        var listClinics = Clinics.GetForUserod(Security.CurUser, doIncludeHQ: true);
        listClinics.Add(Clinics.GetPracticeAsClinicZero());
        foreach (var prefName in prefNames.Distinct())
        {
            //Explicity load the entire list because some forms using the clinicPrefHelper might have a "use defaults" pref
            //that this ClinicPrefHelper doesn't and shouldn't know about. We will clean up unneccessary clinic prefs when 
            //we call SyncPrefs.
            foreach (var clinic in listClinics.Distinct())
            {
                _clinicPrefs.Add(new ClinicPref
                {
                    PrefName = prefName,
                    ClinicNum = clinic.Id,
                    ValueString = ClinicPrefs.GetPrefValue(prefName, clinic.Id)
                });
            }

            _prefNames.Add(prefName);
        }
    }

    public void ValChangedByUser(PrefName prefName, long clinicNum, string newVal)
    {
        if (clinicNum < 0)
        {
            //Shouldn't happen...
            return;
        }

        //Update the current value for the pref that we are storing in the list
        var clinicPref = _clinicPrefs.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName);
        if (clinicPref == null)
        {
            //Doesn't exist so create one
            clinicPref = new ClinicPref
            {
                PrefName = prefName,
                ClinicNum = clinicNum
            };
            _clinicPrefs.Add(clinicPref);
        }

        clinicPref.ValueString = newVal;
    }

    public int GetIntVal(PrefName prefName, long clinicNum)
    {
        if (clinicNum < 0)
        {
            //Shouldn't happen
            return -1;
        }

        if (_clinicPrefs.Any(x => x.ClinicNum == clinicNum && x.PrefName == prefName))
        {
            //we've already loaded this item, just load its checked value
            return SIn.Int(_clinicPrefs.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName).ValueString);
        }

        return SIn.Int(_clinicPrefs.FirstOrDefault(x => x.ClinicNum == 0 && x.PrefName == prefName).ValueString);
    }

    public bool GetBoolVal(PrefName prefName, long clinicNum)
    {
        if (clinicNum < 0)
        {
            //Shouldn't happen
            return false;
        }

        if (_clinicPrefs.Any(x => x.ClinicNum == clinicNum && x.PrefName == prefName))
        {
            //we've already loaded this item, just load its checked value
            return SIn.Bool(_clinicPrefs.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName).ValueString);
        }

        return SIn.Bool(_clinicPrefs.FirstOrDefault(x => x.ClinicNum == 0 && x.PrefName == prefName).ValueString);
    }

    public string GetStringVal(PrefName prefName, long clinicNum)
    {
        if (clinicNum < 0)
        {
            //Shouldn't happen
            return "";
        }

        if (_clinicPrefs.Any(x => x.ClinicNum == clinicNum && x.PrefName == prefName))
        {
            //we've already loaded this item, just load its checked value
            return _clinicPrefs.FirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName).ValueString;
        }

        return _clinicPrefs.FirstOrDefault(x => x.ClinicNum == 0 && x.PrefName == prefName).ValueString;
    }

    public bool GetDefaultBoolVal(PrefName prefName)
    {
        return SIn.Bool(_clinicPrefs.First(x => x.ClinicNum == 0 && x.PrefName == prefName).ValueString);
    }

    public string GetDefaultStringVal(PrefName prefName)
    {
        return _clinicPrefs.First(x => x.ClinicNum == 0 && x.PrefName == prefName).ValueString;
    }

    public List<ClinicPref> GetWhere(PrefName prefName, string valueString)
    {
        return _clinicPrefs.FindAll(x => x.PrefName == prefName && x.ValueString == valueString);
    }

    public bool SyncAllPrefs()
    {
        var ret = false;
        foreach (var prefName in _prefNames)
        {
            if (SyncPref(prefName))
            {
                ret = true;
            }
        }

        return ret;
    }

    public bool SyncPref(PrefName prefName)
    {
        //We ensured that our list had default (ClinicNum 0) prefs when we included defaults in Init(). Should always be available.
        var hqValue = _clinicPrefs.First(x => x.ClinicNum == 0 && x.PrefName == prefName).ValueString;
        //Save the default (HQ) pref first.
        var didSave = prefName.Update(hqValue);
        //Our list will likely have clinic-specific entries which are identical to HQ defaults. 
        //In this case, remove those duplicates so we don't save them to the db.
        _clinicPrefs.RemoveAll(x => x.ClinicNum != 0 && x.PrefName == prefName && x.ValueString.Equals(hqValue));
        var listNonDefaultClinicPrefs = _clinicPrefs.FindAll(x => x.ClinicNum > 0 && x.PrefName == prefName);
        if (ClinicPrefs.Sync(listNonDefaultClinicPrefs, ClinicPrefs.GetPrefAllClinics(prefName)))
        {
            didSave = true;
        }

        if (didSave)
        {
            Signalods.SetInvalid(InvalidType.ClinicPrefs);
            ClinicPrefs.RefreshCache();
        }

        return didSave;
    }

    public List<long> GetClinicsWithChanges()
    {
        List<long> ret = [];
        foreach (var prefName in _prefNames)
        {
            ret.AddRange(GetClinicsWithChanges(prefName));
        }

        return ret.Distinct().ToList();
    }

    public List<long> GetClinicsWithChanges(PrefName prefName)
    {
        var listClinicPrefsThisPrefName = _clinicPrefs.FindAll(x => x.PrefName == prefName);
        List<long> listRet = [];
        var listDb = ClinicPrefs.GetPrefAllClinics(prefName, includeDefault: true);
        listRet.AddRange(listClinicPrefsThisPrefName.Where(x =>
                //Get the items from the new list that aren't in the old list 
                !listDb.Select(y => y.ClinicNum).Contains(x.ClinicNum)
                //AND that aren't using the default preference value
                && x.ValueString != PrefC.GetString(prefName))
            //Add the clinic nums
            .Select(x => x.ClinicNum));
        //Add any items that have been deleted or updated.
        foreach (var oldCp in listDb)
        {
            var newCp = listClinicPrefsThisPrefName.FirstOrDefault(x => x.ClinicNum == oldCp.ClinicNum);
            if (newCp == null)
            {
                //Item was in db and now is not.
                listRet.Add(oldCp.ClinicNum);
                continue;
            }

            if (newCp.ValueString != oldCp.ValueString)
            {
                //Item has changed.
                listRet.Add(oldCp.ClinicNum);
            }
        }

        return listRet.Distinct().ToList();
    }
}