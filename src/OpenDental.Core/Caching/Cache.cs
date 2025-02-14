using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using OpenDentBusiness;

namespace Imedisoft.Core.Caching;

public class Cache
{
    private static readonly List<object> Items = [];

    public static void TrackCacheObject(object cacheToTrack)
    {
        if (!Items.Contains(cacheToTrack))
        {
            Items.Add(cacheToTrack);
        }
    }

    public static void Refresh(params InvalidType[] arrayITypes)
    {
        Refresh(true, arrayITypes);
    }
        
    public static void Refresh(bool doRefreshServerCache, params InvalidType[] arrayITypes)
    {
        GetCacheDs(doRefreshServerCache, arrayITypes);
    }

    public static void GetCacheDs(bool doRefreshServerCache, params InvalidType[] arrayITypes)
    {
        var prefix = "Refreshing Caches: ";
        var listITypes = arrayITypes.ToList();
        //so this part below only happens if direct or server------------------------------------------------
        var isAll = listITypes.Contains(InvalidType.AllLocal);

        var ds = new DataSet();
        //All cached public tables go here
        if (listITypes.Contains(InvalidType.AccountingAutoPays) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AccountingAutoPays);
            AccountingAutoPays.GetTableFromCache(doRefreshServerCache);
        }
            
        if (listITypes.Contains(InvalidType.AlertCategories) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AlertCategories);
            AlertCategories.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.AlertCategoryLinks) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AlertCategoryLinks);
            AlertCategoryLinks.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.AppointmentTypes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AppointmentTypes);
            AppointmentTypes.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.AutoCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AutoCodes);
            AutoCodes.GetTableFromCache(doRefreshServerCache);
            AutoCodeItems.GetTableFromCache(doRefreshServerCache);
            AutoCodeConds.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Automation) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Automation);
            Automations.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.AutoNotes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AutoNotes);
            AutoNotes.GetTableFromCache(doRefreshServerCache);
            AutoNoteControls.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Carriers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Carriers);
            Carriers.GetTableFromCache(doRefreshServerCache); //run on startup, after telephone reformat, after list edit.
        }

        if (listITypes.Contains(InvalidType.ClaimForms) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClaimForms);
            ClaimFormItems.GetTableFromCache(doRefreshServerCache);
            ClaimForms.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.ClearHouses) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClearHouses);
            Clearinghouses.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.ClinicPrefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClinicPrefs);
            ClinicPrefs.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.CodeGroups) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.CodeGroups);
            CodeGroups.GetTableFromCache(doRefreshServerCache);
        }

        //InvalidType.Clinics see InvalidType.Providers
        if (listITypes.Contains(InvalidType.Computers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Computers);
            Computers.GetTableFromCache(doRefreshServerCache);
            Printers.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Defs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Defs);
            Defs.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.DictCustoms) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DictCustoms);
            DictCustoms.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Diseases) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Diseases);
            DiseaseDefs.GetTableFromCache(doRefreshServerCache);
            Icd9s.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.DisplayFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DisplayFields);
            ChartViews.GetTableFromCache(doRefreshServerCache);
            DisplayFields.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.DisplayReports) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DisplayReports);
            DisplayReports.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Ebills) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Ebills);
            Ebills.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.ElectIDs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ElectIDs);
            ElectIDs.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Email) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Email);
            EmailAddresses.GetTableFromCache(doRefreshServerCache);
            EmailTemplates.GetTableFromCache(doRefreshServerCache);
            EmailAutographs.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Employees) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Employees);
            Employees.GetTableFromCache(doRefreshServerCache);
            PayPeriods.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Employers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Employers);
            Employers.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.FeeScheds) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.FeeScheds);
            FeeScheds.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.HL7Defs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.HL7Defs);
            HL7Defs.GetTableFromCache(doRefreshServerCache);
            HL7DefMessages.GetTableFromCache(doRefreshServerCache);
            HL7DefSegments.GetTableFromCache(doRefreshServerCache);
            HL7DefFields.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.InsCats) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.InsCats);
            CovCats.GetTableFromCache(doRefreshServerCache);
            CovSpans.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.InsFilingCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.InsFilingCodes);
            InsFilingCodes.GetTableFromCache(doRefreshServerCache);
            InsFilingCodeSubtypes.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Languages) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Languages);
            if (CultureInfo.CurrentCulture.Name != "en-US")
            {
                Lans.GetTableFromCache(doRefreshServerCache);
            }

            LanguagePats.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.LetterMerge) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.LetterMerge);
            LetterMergeFields.GetTableFromCache(doRefreshServerCache);
            LetterMerges.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.Operatories) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Operatories);
            Operatories.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.OrthoChartTabs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.OrthoChartTabs);
            OrthoChartTabs.GetTableFromCache(doRefreshServerCache);
            OrthoChartTabLinks.GetTableFromCache(doRefreshServerCache);
            OrthoHardwareSpecs.GetTableFromCache(doRefreshServerCache);
            OrthoRxs.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.PatFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.PatFields);
            PatFieldDefs.GetTableFromCache(doRefreshServerCache);
            PatFieldPickItems.GetTableFromCache(doRefreshServerCache);
            ApptFieldDefs.GetTableFromCache(doRefreshServerCache);
            FieldDefLinks.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Pharmacies) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Pharmacies);
            Pharmacies.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Prefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Prefs);
            Prefs.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.ProcButtons) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcButtons);
            ProcButtons.GetTableFromCache(doRefreshServerCache);
            ProcButtonItems.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.ProcMultiVisits) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcMultiVisits);
            ProcMultiVisits.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.ProcCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcCodes);
            ProcedureCodes.GetTableFromCache(doRefreshServerCache);
            ProcCodeNotes.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Programs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Programs);
            Programs.GetTableFromCache(doRefreshServerCache);
            ProgramProperties.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.ProviderClinicLink) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderClinicLink);
            ProviderClinicLinks.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.ProviderIdents) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderIdents);
            ProviderIdents.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Providers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Providers);
            Providers.GetTableFromCache();
            //Refresh the clinics as well because InvalidType.Providers has a comment that says "also includes clinics".  Also, there currently isn't an itype for Clinics.
            Clinics.RefreshCache();
        }

        if (listITypes.Contains(InvalidType.QuickPaste) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.QuickPaste);
            QuickPasteNotes.GetTableFromCache(doRefreshServerCache);
            QuickPasteCats.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.RecallTypes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.RecallTypes);
            RecallTypes.GetTableFromCache(doRefreshServerCache);
            RecallTriggers.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Referral) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Referral);
            Referrals.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.RequiredFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.RequiredFields);
            RequiredFields.GetTableFromCache(doRefreshServerCache);
            RequiredFieldConditions.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Security) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Security);
            //There is a chance that some future engineer will introduce a signal that tells another workstation to refresh the users when it shouldn't.
            //It is completely safe to skip over getting the user cache when IsCacheAllowed is false because the setter for that boolean nulls the cache.
            //This means that the cache will refill itself automatically the next time it is accessed as soon as the boolean flips back to true.
            if (Userods.GetIsCacheAllowed())
            {
                Userods.GetTableFromCache(doRefreshServerCache);
            }

            UserGroups.GetTableFromCache(doRefreshServerCache);
            GroupPermissions.GetTableFromCache(doRefreshServerCache);
            UserGroupAttaches.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Sheets) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sheets);
            SheetDefs.GetTableFromCache(doRefreshServerCache);
            SheetFieldDefs.GetTableFromCache(doRefreshServerCache);
            EFormDefs.GetTableFromCache(doRefreshServerCache);
            EFormFieldDefs.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.Sites) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sites);
            Sites.GetTableFromCache(doRefreshServerCache);
        } 
        
        if (listITypes.Contains(InvalidType.SmsPhones) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.SmsPhones);
            SmsPhones.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.Sops) || isAll)
        {
            //InvalidType.Sops is currently never used 11/14/2014
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sops);
            Sops.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.StateAbbrs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.StateAbbrs);
            StateAbbrs.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.ToolButsAndMounts) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ToolButsAndMounts);
            ToolButItems.GetTableFromCache(doRefreshServerCache);
            MountDefs.GetTableFromCache(doRefreshServerCache);
            ImagingDevices.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.UserClinics) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserClinics);
            UserClinics.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.UserOdPrefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserOdPrefs);
            UserOdPrefs.GetTableFromCache(doRefreshServerCache);
        }

        if (listITypes.Contains(InvalidType.UserQueries) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserQueries);
            UserQueries.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.Views) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Views);
            ApptViews.GetTableFromCache(doRefreshServerCache);
            ApptViewItems.GetTableFromCache(doRefreshServerCache);
            AppointmentRules.GetTableFromCache(doRefreshServerCache);
            ProcApptColors.GetTableFromCache(doRefreshServerCache);
        }
        
        if (listITypes.Contains(InvalidType.ZipCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ZipCodes);
            ZipCodes.GetTableFromCache(doRefreshServerCache);
        }
    }

    public static void ClearCaches()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            try
            {
                //We know that all objects in this list have a ClearCache method
                var typeCache = Items[i].GetType();
                var methodInfoClearCache = typeCache.GetMethod(nameof(CacheAbs<TableBase>.ClearCache)); //CacheAbs type doesn't matter, we just need *some* type
                methodInfoClearCache.Invoke(Items[i], null);
            }
            catch
            {
                // ignored
            }
        }
    }

    public static void ClearCaches(params InvalidType[] arrayITypes)
    {
        //No RemotingClient check needed; The server does not need to clear these caches because it already knows about the changes.
        //This is assuming that the workstation that was responsible for the cache change asked the MT server to update it's local cache.
        var prefix = Lans.g(nameof(Cache), "Clearing Caches") + ": ";
        var listITypes = arrayITypes.ToList();
        //so this part below only happens if direct or server------------------------------------------------
        var isAll = listITypes.Contains(InvalidType.AllLocal);

        //All cached public tables go here
        if (listITypes.Contains(InvalidType.AccountingAutoPays) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AccountingAutoPays);
            AccountingAutoPays.ClearCache();
        }

        if (listITypes.Contains(InvalidType.AlertCategories) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AlertCategories);
            AlertCategories.ClearCache();
        }

        if (listITypes.Contains(InvalidType.AlertCategoryLinks) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AlertCategoryLinks);
            AlertCategoryLinks.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.AppointmentTypes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AppointmentTypes);
            AppointmentTypes.ClearCache();
        }

        if (listITypes.Contains(InvalidType.AutoCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AutoCodes);
            AutoCodes.ClearCache();
            AutoCodeItems.ClearCache();
            AutoCodeConds.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Automation) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Automation);
            Automations.ClearCache();
        }

        if (listITypes.Contains(InvalidType.AutoNotes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AutoNotes);
            AutoNotes.ClearCache();
            AutoNoteControls.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Carriers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Carriers);
            Carriers.ClearCache();
        }

        if (listITypes.Contains(InvalidType.ClaimForms) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClaimForms);
            ClaimFormItems.ClearCache();
            ClaimForms.ClearCache();
        }

        if (listITypes.Contains(InvalidType.ClearHouses) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClearHouses);
            Clearinghouses.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.ClinicPrefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClinicPrefs);
            ClinicPrefs.ClearCache();
        }

        if (listITypes.Contains(InvalidType.CodeGroups) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.CodeGroups);
            CodeGroups.ClearCache();
        }

        //InvalidType.Clinics see InvalidType.Providers
        if (listITypes.Contains(InvalidType.Computers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Computers);
            Computers.ClearCache();
            Printers.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Defs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Defs);
            Defs.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.DictCustoms) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DictCustoms);
            DictCustoms.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Diseases) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Diseases);
            DiseaseDefs.ClearCache();
            Icd9s.ClearCache();
        }

        if (listITypes.Contains(InvalidType.DisplayFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DisplayFields);
            ChartViews.ClearCache();
            DisplayFields.ClearCache();
        }

        if (listITypes.Contains(InvalidType.DisplayReports) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DisplayReports);
            DisplayReports.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Ebills) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Ebills);
            Ebills.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.ElectIDs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ElectIDs);
            ElectIDs.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Email) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Email);
            EmailAddresses.ClearCache();
            EmailTemplates.ClearCache();
            EmailAutographs.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Employees) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Employees);
            Employees.ClearCache();
            PayPeriods.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Employers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Employers);
            Employers.ClearCache();
        }

        if (listITypes.Contains(InvalidType.FeeScheds) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.FeeScheds);
            FeeScheds.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.HL7Defs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.HL7Defs);
            HL7Defs.ClearCache();
            HL7DefMessages.ClearCache();
            HL7DefSegments.ClearCache();
            HL7DefFields.ClearCache();
        }

        if (listITypes.Contains(InvalidType.InsCats) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.InsCats);
            CovCats.ClearCache();
            CovSpans.ClearCache();
        }

        if (listITypes.Contains(InvalidType.InsFilingCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.InsFilingCodes);
            InsFilingCodes.ClearCache();
            InsFilingCodeSubtypes.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Languages) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Languages);
            if (CultureInfo.CurrentCulture.Name != "en-US")
            {
                Lans.ClearCache();
            }

            LanguagePats.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.LetterMerge) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.LetterMerge);
            LetterMergeFields.ClearCache();
            LetterMerges.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.Operatories) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Operatories);
            Operatories.ClearCache();
        }

        if (listITypes.Contains(InvalidType.OrthoChartTabs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.OrthoChartTabs);
            OrthoChartTabs.ClearCache();
            OrthoChartTabLinks.ClearCache();
            OrthoHardwareSpecs.ClearCache();
            OrthoRxs.ClearCache();
        }

        if (listITypes.Contains(InvalidType.PatFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.PatFields);
            PatFieldDefs.ClearCache();
            PatFieldPickItems.ClearCache();
            ApptFieldDefs.ClearCache();
            FieldDefLinks.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Pharmacies) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Pharmacies);
            Pharmacies.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Prefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Prefs);
            Prefs.ClearCache();
        }

        if (listITypes.Contains(InvalidType.ProcButtons) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcButtons);
            ProcButtons.ClearCache();
            ProcButtonItems.ClearCache();
        }

        if (listITypes.Contains(InvalidType.ProcMultiVisits) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcMultiVisits);
            ProcMultiVisits.ClearCache();
        }

        if (listITypes.Contains(InvalidType.ProcCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcCodes);
            ProcedureCodes.ClearCache();
            ProcCodeNotes.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Programs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Programs);
            Programs.ClearCache();
            ProgramProperties.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.ProviderClinicLink) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderClinicLink);
            ProviderClinicLinks.ClearCache();
        }

        if (listITypes.Contains(InvalidType.ProviderIdents) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderIdents);
            ProviderIdents.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Providers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Providers);
            Providers.ClearCache();
            //Refresh the clinics as well because InvalidType.Providers has a comment that says "also includes clinics".Also, there currently isn't an itype for Clinics.
            Clinics.ClearCache();
        }

        if (listITypes.Contains(InvalidType.QuickPaste) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.QuickPaste);
            QuickPasteNotes.ClearCache();
            QuickPasteCats.ClearCache();
        }

        if (listITypes.Contains(InvalidType.RecallTypes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.RecallTypes);
            RecallTypes.ClearCache();
            RecallTriggers.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Referral) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Referral);
            Referrals.ClearCache();
        }

        if (listITypes.Contains(InvalidType.RequiredFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.RequiredFields);
            RequiredFields.ClearCache();
            RequiredFieldConditions.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Security) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Security);
            Userods.ClearCache();
            UserGroups.ClearCache();
            GroupPermissions.ClearCache();
            UserGroupAttaches.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Sheets) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sheets);
            SheetDefs.ClearCache();
            SheetFieldDefs.ClearCache();
            EFormDefs.ClearCache();
            EFormFieldDefs.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.Sites) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sites);
            Sites.ClearCache();
        }

        if (listITypes.Contains(InvalidType.SmsPhones) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.SmsPhones);
            SmsPhones.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Sops) || isAll)
        {
            //InvalidType.Sops is currently never used 11/14/2014
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sops);
            Sops.ClearCache();
        }

        if (listITypes.Contains(InvalidType.StateAbbrs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.StateAbbrs);
            StateAbbrs.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.ToolButsAndMounts) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ToolButsAndMounts);
            ToolButItems.ClearCache();
            MountDefs.ClearCache();
            ImagingDevices.ClearCache();
        }

        if (listITypes.Contains(InvalidType.UserClinics) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserClinics);
            UserClinics.ClearCache();
        }

        if (listITypes.Contains(InvalidType.UserOdPrefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserOdPrefs);
            UserOdPrefs.ClearCache();
        }

        if (listITypes.Contains(InvalidType.UserQueries) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserQueries);
            UserQueries.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.Views) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Views);
            ApptViews.ClearCache();
            ApptViewItems.ClearCache();
            AppointmentRules.ClearCache();
            ProcApptColors.ClearCache();
        }
        
        if (listITypes.Contains(InvalidType.ZipCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ZipCodes);
            ZipCodes.ClearCache();
        }
    }
}