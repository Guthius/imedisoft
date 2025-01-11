using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

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
        var prefix = Lans.g(nameof(Cache), "Refreshing Caches") + ": ";
        Logger.LogToPath("", LogPath.Signals, LogPhase.Start, "InvalidType(s): " + string.Join(" - ", arrayITypes.OrderBy(x => x.ToString())));
        var listITypes = arrayITypes.ToList();
        //so this part below only happens if direct or server------------------------------------------------
        var isAll = listITypes.Contains(InvalidType.AllLocal);

        var ds = new DataSet();
        //All cached public tables go here
        if (listITypes.Contains(InvalidType.AccountingAutoPays) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AccountingAutoPays);
            ds.Tables.Add(AccountingAutoPays.GetTableFromCache(doRefreshServerCache));
        }
            
        if (listITypes.Contains(InvalidType.AlertCategories) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AlertCategories);
            ds.Tables.Add(AlertCategories.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.AlertCategoryLinks) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AlertCategoryLinks);
            ds.Tables.Add(AlertCategoryLinks.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ApiSubscriptions) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ApiSubscriptions);
            ds.Tables.Add(ApiSubscriptions.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.AppointmentTypes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AppointmentTypes);
            ds.Tables.Add(AppointmentTypes.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.AutoCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AutoCodes);
            ds.Tables.Add(AutoCodes.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(AutoCodeItems.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(AutoCodeConds.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Automation) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Automation);
            ds.Tables.Add(Automations.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.AutoNotes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.AutoNotes);
            ds.Tables.Add(AutoNotes.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(AutoNoteControls.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Carriers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Carriers);
            ds.Tables.Add(Carriers.GetTableFromCache(doRefreshServerCache)); //run on startup, after telephone reformat, after list edit.
        }

        if (listITypes.Contains(InvalidType.ClaimForms) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClaimForms);
            ds.Tables.Add(ClaimFormItems.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ClaimForms.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ClearHouses) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClearHouses);
            ds.Tables.Add(Clearinghouses.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ClinicErxs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClinicErxs);
            ds.Tables.Add(ClinicErxs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ClinicPrefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClinicPrefs);
            ds.Tables.Add(ClinicPrefs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.CodeGroups) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.CodeGroups);
            ds.Tables.Add(CodeGroups.GetTableFromCache(doRefreshServerCache));
        }

        //InvalidType.Clinics see InvalidType.Providers
        if (listITypes.Contains(InvalidType.Computers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Computers);
            ds.Tables.Add(Computers.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(Printers.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Defs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Defs);
            ds.Tables.Add(Defs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.DentalSchools) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DentalSchools);
            ds.Tables.Add(SchoolClasses.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(SchoolCourses.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.DictCustoms) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DictCustoms);
            ds.Tables.Add(DictCustoms.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Diseases) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Diseases);
            ds.Tables.Add(DiseaseDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ICD9s.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.DisplayFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DisplayFields);
            ds.Tables.Add(ChartViews.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(DisplayFields.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.DisplayReports) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DisplayReports);
            ds.Tables.Add(DisplayReports.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Ebills) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Ebills);
            ds.Tables.Add(Ebills.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.EhrCodes))
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.EhrCodes);
            EhrCodes.UpdateList(); //Unusual pattern for an unusual "table".  Not really a table, but a mishmash of hard coded partial code systems that are needed for CQMs.
        }

        if (listITypes.Contains(InvalidType.ElectIDs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ElectIDs);
            ds.Tables.Add(ElectIDs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Email) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Email);
            ds.Tables.Add(EmailAddresses.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(EmailTemplates.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(EmailAutographs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Employees) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Employees);
            ds.Tables.Add(Employees.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(PayPeriods.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Employers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Employers);
            ds.Tables.Add(Employers.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.FeeScheds) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.FeeScheds);
            ds.Tables.Add(FeeScheds.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ERoutingDef) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.PatFields);
            ds.Tables.Add(ERoutingDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ERoutingActionDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ERoutingDefLinks.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.HL7Defs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.HL7Defs);
            ds.Tables.Add(HL7Defs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(HL7DefMessages.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(HL7DefSegments.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(HL7DefFields.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.InsCats) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.InsCats);
            ds.Tables.Add(CovCats.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(CovSpans.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.InsFilingCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.InsFilingCodes);
            ds.Tables.Add(InsFilingCodes.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(InsFilingCodeSubtypes.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Languages) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Languages);
            if (CultureInfo.CurrentCulture.Name != "en-US")
            {
                ds.Tables.Add(Lans.GetTableFromCache(doRefreshServerCache));
            }

            ds.Tables.Add(LanguagePats.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Letters) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Letters);
            ds.Tables.Add(Letters.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.LetterMerge) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.LetterMerge);
            ds.Tables.Add(LetterMergeFields.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(LetterMerges.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.LimitedBetaFeature) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.LimitedBetaFeature);
            ds.Tables.Add(LimitedBetaFeatures.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Medications) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Medications);
            ds.Tables.Add(Medications.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Operatories) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Operatories);
            ds.Tables.Add(Operatories.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.OrthoChartTabs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.OrthoChartTabs);
            ds.Tables.Add(OrthoChartTabs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(OrthoChartTabLinks.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(OrthoHardwareSpecs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(OrthoRxs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.PatFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.PatFields);
            ds.Tables.Add(PatFieldDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(PatFieldPickItems.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ApptFieldDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(FieldDefLinks.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Pharmacies) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Pharmacies);
            ds.Tables.Add(Pharmacies.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Prefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Prefs);
            ds.Tables.Add(Prefs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ProcButtons) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcButtons);
            ds.Tables.Add(ProcButtons.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ProcButtonItems.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ProcMultiVisits) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcMultiVisits);
            ds.Tables.Add(ProcMultiVisits.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ProcCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProcCodes);
            ds.Tables.Add(ProcedureCodes.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ProcCodeNotes.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Programs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Programs);
            ds.Tables.Add(Programs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ProgramProperties.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ProviderErxs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderErxs);
            ds.Tables.Add(ProviderErxs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ProviderClinicLink) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderClinicLink);
            ds.Tables.Add(ProviderClinicLinks.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ProviderIdents) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderIdents);
            ds.Tables.Add(ProviderIdents.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Providers) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Providers);
            ds.Tables.Add(Providers.GetTableFromCache(doRefreshServerCache));
            //Refresh the clinics as well because InvalidType.Providers has a comment that says "also includes clinics".  Also, there currently isn't an itype for Clinics.
            Clinics.RefreshCache();
        }

        if (listITypes.Contains(InvalidType.QuickPaste) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.QuickPaste);
            ds.Tables.Add(QuickPasteNotes.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(QuickPasteCats.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.RecallTypes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.RecallTypes);
            ds.Tables.Add(RecallTypes.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(RecallTriggers.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Referral) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Referral);
            ds.Tables.Add(Referrals.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.RequiredFields) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.RequiredFields);
            ds.Tables.Add(RequiredFields.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(RequiredFieldConditions.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Security) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Security);
            //There is a chance that some future engineer will introduce a signal that tells another workstation to refresh the users when it shouldn't.
            //It is completely safe to skip over getting the user cache when IsCacheAllowed is false because the setter for that boolean nulls the cache.
            //This means that the cache will refill itself automatically the next time it is accessed as soon as the boolean flips back to true.
            if (Userods.GetIsCacheAllowed())
            {
                ds.Tables.Add(Userods.GetTableFromCache(doRefreshServerCache));
            }

            ds.Tables.Add(UserGroups.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(GroupPermissions.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(UserGroupAttaches.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Sheets) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sheets);
            ds.Tables.Add(SheetDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(SheetFieldDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(EFormDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(EFormFieldDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(EFormImportRules.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.SigMessages) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.SigMessages);
            ds.Tables.Add(SigElementDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(SigButDefs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Sites) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sites);
            ds.Tables.Add(Sites.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.SmsBlockPhones) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.SmsBlockPhones);
            ds.Tables.Add(SmsBlockPhones.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.SmsPhones) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.SmsPhones);
            ds.Tables.Add(SmsPhones.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Sops) || isAll)
        {
            //InvalidType.Sops is currently never used 11/14/2014
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sops);
            ds.Tables.Add(Sops.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.StateAbbrs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.StateAbbrs);
            ds.Tables.Add(StateAbbrs.GetTableFromCache(doRefreshServerCache));
        }

        //InvalidTypes.Tasks not handled here.
        if (listITypes.Contains(InvalidType.TimeCardRules) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.TimeCardRules);
            ds.Tables.Add(TimeCardRules.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.ToolButsAndMounts) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ToolButsAndMounts);
            ds.Tables.Add(ToolButItems.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(MountDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ImagingDevices.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.UserClinics) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserClinics);
            ds.Tables.Add(UserClinics.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.UserOdPrefs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserOdPrefs);
            ds.Tables.Add(UserOdPrefs.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.UserQueries) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.UserQueries);
            ds.Tables.Add(UserQueries.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Vaccines) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Vaccines);
            ds.Tables.Add(VaccineDefs.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(DrugManufacturers.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(DrugUnits.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Views) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Views);
            ds.Tables.Add(ApptViews.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ApptViewItems.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(AppointmentRules.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(ProcApptColors.GetTableFromCache(doRefreshServerCache));
        }

        if (listITypes.Contains(InvalidType.Wiki) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Wiki);
            ds.Tables.Add(WikiListHeaderWidths.GetTableFromCache(doRefreshServerCache));
            ds.Tables.Add(WikiPages.RefreshCache());
        }

        if (listITypes.Contains(InvalidType.ZipCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ZipCodes);
            ds.Tables.Add(ZipCodes.GetTableFromCache(doRefreshServerCache));
        }

        Logger.LogToPath("", LogPath.Signals, LogPhase.End);
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
        Logger.LogToPath("", LogPath.Signals, LogPhase.Start, "InvalidType(s): " + string.Join(" - ", arrayITypes.OrderBy(x => x.ToString())));
        var listITypes = arrayITypes.ToList();
        //so this part below only happens if direct or server------------------------------------------------
        var isAll = false;
        if (listITypes.Contains(InvalidType.AllLocal))
        {
            isAll = true;
        }

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

        if (listITypes.Contains(InvalidType.ApiSubscriptions) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ApiSubscriptions);
            ApiSubscriptions.ClearCache();
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

        if (listITypes.Contains(InvalidType.ClinicErxs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ClinicErxs);
            ClinicErxs.ClearCache();
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

        if (listITypes.Contains(InvalidType.DentalSchools) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.DentalSchools);
            SchoolClasses.ClearCache();
            SchoolCourses.ClearCache();
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
            ICD9s.ClearCache();
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

        if (listITypes.Contains(InvalidType.EhrCodes))
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.EhrCodes + " (skipped)");
            //EhrCodes.UpdateList();//There is no such thing as Clearing the "Cache" of all EHR code systems.
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

        if (listITypes.Contains(InvalidType.ERoutingDef) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.PatFields);
            ERoutingDefs.ClearCache();
            ERoutingActionDefs.ClearCache();
            ERoutingDefLinks.ClearCache();
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

        if (listITypes.Contains(InvalidType.Letters) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Letters);
            Letters.ClearCache();
        }

        if (listITypes.Contains(InvalidType.LetterMerge) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.LetterMerge);
            LetterMergeFields.ClearCache();
            LetterMerges.ClearCache();
        }

        if (listITypes.Contains(InvalidType.LimitedBetaFeature) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.LimitedBetaFeature);
            LimitedBetaFeatures.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Medications) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Medications);
            Medications.ClearCache();
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

        if (listITypes.Contains(InvalidType.ProviderErxs) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ProviderErxs);
            ProviderErxs.ClearCache();
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
            EFormImportRules.ClearCache();
        }

        if (listITypes.Contains(InvalidType.SigMessages) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.SigMessages);
            SigElementDefs.ClearCache();
            SigButDefs.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Sites) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Sites);
            Sites.ClearCache();
        }

        if (listITypes.Contains(InvalidType.SmsBlockPhones) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.SmsBlockPhones);
            SmsBlockPhones.ClearCache();
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

        //InvalidTypes.Tasks not handled here.
        if (listITypes.Contains(InvalidType.TimeCardRules) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.TimeCardRules);
            TimeCardRules.ClearCache();
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

        if (listITypes.Contains(InvalidType.Vaccines) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Vaccines);
            VaccineDefs.ClearCache();
            DrugManufacturers.ClearCache();
            DrugUnits.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Views) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Views);
            ApptViews.ClearCache();
            ApptViewItems.ClearCache();
            AppointmentRules.ClearCache();
            ProcApptColors.ClearCache();
        }

        if (listITypes.Contains(InvalidType.Wiki) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.Wiki);
            WikiListHeaderWidths.ClearCache();
            WikiPages.ClearCache();
        }

        if (listITypes.Contains(InvalidType.ZipCodes) || isAll)
        {
            ODEvent.Fire(ODEventType.Cache, prefix + InvalidType.ZipCodes);
            ZipCodes.ClearCache();
        }

        Logger.LogToPath("", LogPath.Signals, LogPhase.End);
    }
}