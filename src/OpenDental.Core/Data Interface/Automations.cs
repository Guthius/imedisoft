using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Automations
{
    public static void Insert(Automation automation)
    {
        AutomationCrud.Insert(automation);
    }

    public static void Update(Automation automation)
    {
        AutomationCrud.Update(automation);
    }

    public static void Delete(Automation automation)
    {
        Db.NonQ("DELETE FROM automation WHERE AutomationNum = " + automation.AutomationNum);
    }

    public static bool Trigger<T>(
        EnumAutomationTrigger automationTrigger,
        List<string> listProcCodes,
        long patNum,
        Dictionary<long, Dictionary<long, DateTime>> dictionaryBlockedAutomations,
        Action<string> actionShowMsg,
        Func<string, string, bool> funcYesNoMsgPrompt,
        Action<Commlog> actionShowCommLog,
        Action<Sheet> actionShowSheetFillEdit,
        Func<List<Procedure>, Image> funcCreateToothChartImage,
        long aptNum = 0,
        T triggerObj = default)
    {
        if (patNum == 0)
        {
            return false;
        }

        var automations = GetDeepCopy();
        var didAutomationHappen = false;

        foreach (var automation in automations)
        {
            if (automation.Autotrigger != automationTrigger) continue;
            if (automationTrigger is EnumAutomationTrigger.ProcedureComplete or EnumAutomationTrigger.ProcSchedule)
            {
                if (listProcCodes == null || listProcCodes.Count == 0)
                {
                    continue;
                }

                var arrayCodes = automation.ProcCodes.Split(',');
                if (listProcCodes.All(x => !arrayCodes.Contains(x)))
                {
                    continue;
                }
            }

            var listAutomationConditions = AutomationConditions.GetListByAutomationNum(automation.AutomationNum);
            if (listAutomationConditions.Count > 0 && !CheckAutomationConditions(listAutomationConditions, patNum, triggerObj))
            {
                continue;
            }

            SheetDef sheetDef;
            Sheet sheet;
            Appointment appointmentNew;
            Appointment appointmentOld;

            bool isPatApptSchedRestricted;

            switch (automation.AutoAction)
            {
                case AutomationAction.CreateCommlog:
                    actionShowCommLog.Invoke(new Commlog
                    {
                        PatNum = patNum,
                        CommDateTime = DateTime.Now,
                        CommType = automation.CommType,
                        Note = automation.MessageContent,
                        Mode_ = CommItemMode.None,
                        UserNum = Security.CurUser.UserNum,
                        IsNew = true
                    });

                    didAutomationHappen = true;
                    continue;

                case AutomationAction.PopUp:
                    actionShowMsg?.Invoke(automation.MessageContent);

                    didAutomationHappen = true;
                    continue;

                case AutomationAction.PopUpThenDisable10Min:
                    var automationNum = automation.AutomationNum;
                    var hasAutomationBlock = dictionaryBlockedAutomations.ContainsKey(automationNum);

                    switch (hasAutomationBlock)
                    {
                        case true when dictionaryBlockedAutomations[automationNum].ContainsKey(patNum):
                            continue;

                        case true:
                            dictionaryBlockedAutomations[automationNum].Add(patNum, DateTime.Now.AddMinutes(10));
                            break;

                        default:
                            dictionaryBlockedAutomations.Add(automationNum,
                                new Dictionary<long, DateTime>
                                {
                                    {patNum, DateTime.Now.AddMinutes(10)}
                                });
                            break;
                    }

                    actionShowMsg?.Invoke(automation.MessageContent);

                    didAutomationHappen = true;
                    continue;

                case AutomationAction.PrintPatientLetter:
                case AutomationAction.ShowExamSheet:
                case AutomationAction.ShowConsentForm:
                    sheetDef = SheetDefs.GetSheetDef(automation.SheetDefNum);
                    sheet = SheetUtil.CreateSheet(sheetDef, patNum);
                    SheetParameter.SetParameter(sheet, "PatNum", patNum);
                    SheetFiller.FillFields(sheet);
                    SheetUtil.CalculateHeights(sheet);
                    actionShowSheetFillEdit.Invoke(sheet);

                    didAutomationHappen = true;
                    continue;

                case AutomationAction.PrintReferralLetter:
                    var referralNum = RefAttaches.GetReferralNum(patNum);
                    if (referralNum == 0)
                    {
                        actionShowMsg?.Invoke("This patient has no referral source entered.");
                        didAutomationHappen = true;
                        continue;
                    }

                    sheetDef = SheetDefs.GetSheetDef(automation.SheetDefNum);
                    sheet = SheetUtil.CreateSheet(sheetDef, patNum);
                    SheetParameter.SetParameter(sheet, "PatNum", patNum);
                    SheetParameter.SetParameter(sheet, "ReferralNum", referralNum);

                    if (sheetDef.SheetFieldDefs.Any(x => (x.FieldType == SheetFieldType.Grid && x.FieldName == "ReferralLetterProceduresCompleted") || (x.FieldType == SheetFieldType.Special && x.FieldName == "toothChart")))
                    {
                        var procs = Procedures.GetCompletedForDateRange(DateTime.Today, DateTime.Today, listPatNums: [patNum], includeNote: true, includeGroupNote: true);
                        if (sheetDef.SheetFieldDefs.Any(x => x.FieldType == SheetFieldType.Grid && x.FieldName == "ReferralLetterProceduresCompleted"))
                        {
                            SheetParameter.SetParameter(sheet, "CompletedProcs", procs);
                        }

                        if (sheetDef.SheetFieldDefs.Any(x => x.FieldType == SheetFieldType.Special && x.FieldName == "toothChart"))
                        {
                            SheetParameter.SetParameter(sheet, "toothChartImg", funcCreateToothChartImage.Invoke(procs));
                        }
                    }

                    SheetFiller.FillFields(sheet);
                    SheetUtil.CalculateHeights(sheet);
                    actionShowSheetFillEdit.Invoke(sheet);

                    didAutomationHappen = true;
                    continue;

                case AutomationAction.SetApptASAP:
                    appointmentNew = Appointments.GetOneApt(aptNum);
                    if (appointmentNew is null)
                    {
                        actionShowMsg?.Invoke("Invalid appointment for automation.");
                        didAutomationHappen = true;
                        continue;
                    }

                    appointmentOld = appointmentNew.Copy();
                    appointmentNew.Priority = ApptPriority.ASAP;
                    Appointments.Update(appointmentNew, appointmentOld);
                    continue;

                case AutomationAction.SetApptType:
                    appointmentNew = Appointments.GetOneApt(aptNum);
                    if (appointmentNew is null)
                    {
                        actionShowMsg?.Invoke("Invalid appointment for automation.");

                        didAutomationHappen = true;
                        continue;
                    }

                    appointmentOld = appointmentNew.Copy();
                    appointmentNew.AppointmentTypeNum = automation.AppointmentTypeNum;

                    var appointmentType = AppointmentTypes.GetFirstOrDefault(x => x.AppointmentTypeNum == appointmentNew.AppointmentTypeNum);
                    if (appointmentType != null)
                    {
                        appointmentNew.ColorOverride = appointmentType.AppointmentTypeColor;
                        appointmentNew.Pattern = AppointmentTypes.GetTimePatternForAppointmentType(appointmentType);

                        var procs = Appointments.ApptTypeMissingProcHelper(appointmentNew, appointmentType, []);

                        Procedures.UpdateAptNums(procs.Select(x => x.ProcNum).ToList(), appointmentNew.AptNum, appointmentNew.AptStatus == ApptStatus.Planned);
                    }

                    Appointments.Update(appointmentNew, appointmentOld);
                    continue;

                case AutomationAction.PatRestrictApptSchedTrue:
                    if (!Security.IsAuthorized(EnumPermType.PatientApptRestrict, true))
                    {
                        SecurityLogs.MakeLogEntry(EnumPermType.PatientApptRestrict, patNum, "Attempt to restrict patient scheduling was blocked due to lack of user permission.");
                        continue;
                    }

                    isPatApptSchedRestricted = PatRestrictions.IsRestricted(patNum, PatRestrict.ApptSchedule);

                    PatRestrictions.Upsert(patNum, PatRestrict.ApptSchedule);
                    PatRestrictions.InsertPatRestrictApptChangeSecurityLog(patNum, isPatApptSchedRestricted, PatRestrictions.IsRestricted(patNum, PatRestrict.ApptSchedule));

                    didAutomationHappen = true;
                    continue;

                case AutomationAction.PatRestrictApptSchedFalse:
                    if (!Security.IsAuthorized(EnumPermType.PatientApptRestrict, true))
                    {
                        SecurityLogs.MakeLogEntry(EnumPermType.PatientApptRestrict, patNum, "Attempt to allow patient scheduling was blocked due to lack of user permission.");
                        continue;
                    }

                    isPatApptSchedRestricted = PatRestrictions.IsRestricted(patNum, PatRestrict.ApptSchedule);

                    PatRestrictions.RemovePatRestriction(patNum, PatRestrict.ApptSchedule);
                    PatRestrictions.InsertPatRestrictApptChangeSecurityLog(patNum, isPatApptSchedRestricted, PatRestrictions.IsRestricted(patNum, PatRestrict.ApptSchedule));

                    didAutomationHappen = true;
                    continue;

                case AutomationAction.ChangePatStatus:
                    var pat = Patients.GetPat(patNum);
                    var patOld = pat.Copy();

                    pat.PatStatus = automation.PatStatus;

                    if (patOld.PatStatus != pat.PatStatus && patOld.PatStatus == PatientStatus.Archived && PatientLinks.WasPatientMerged(patOld.PatNum))
                    {
                        actionShowMsg?.Invoke("Not allowed to change the status of a merged patient.");
                        continue;
                    }

                    switch (pat.PatStatus)
                    {
                        case PatientStatus.Deceased:
                            if (patOld.PatStatus == PatientStatus.Deceased)
                            {
                                break;
                            }

                            var futureAppts = Appointments.GetFutureSchedApts(pat.PatNum);
                            if (futureAppts.Count <= 0)
                            {
                                break;
                            }

                            var apptDates = string.Join("\r\n", futureAppts.Take(10).Select(x => x.AptDateTime.ToString(CultureInfo.InvariantCulture)));
                            if (futureAppts.Count > 10)
                            {
                                apptDates += "(...)";
                            }

                            if (!funcYesNoMsgPrompt.Invoke(
                                    "This patient has scheduled appointments in the future:\r\n" + apptDates + "\r\n" +
                                    "Would you like to delete them and set the patient to Deceased?",
                                    "Delete future appointments?"))
                            {
                                continue;
                            }

                            foreach (var appointment in futureAppts)
                            {
                                Appointments.Delete(appointment.AptNum, true);
                            }

                            break;
                    }

                    Patients.UpdateRecalls(pat, patOld, "ChangePatStatus automation");

                    if (Patients.Update(pat, patOld))
                    {
                        SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, patNum, "Patient status changed from " + patOld.PatStatus.GetDescription() + " to " + automation.PatStatus.GetDescription() + " through ChangePatStatus automation.");
                    }

                    didAutomationHappen = true;

                    continue;
            }
        }

        return didAutomationHappen;
    }

    private static bool CheckAutomationConditions<T>(List<AutomationCondition> automationConditions, long patNum, T triggerObj = default)
    {
        foreach (var automationCondition in automationConditions)
        {
            switch (automationCondition.CompareField)
            {
                case AutoCondField.NeedsSheet:
                    if (NeedsSheet(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.Problem:
                    if (!ProblemComparison(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.Medication:
                    if (!MedicationComparison(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.Allergy:
                    if (!AllergyComparison(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.Age:
                    if (!AgeComparison(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.Gender:
                    if (!GenderComparison(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.InsuranceNotEffective:
                    if (!InsuranceNotEffectiveComparison(patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.BillingType:
                    if (!BillingTypeComparison(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.PlanNum:
                    if (!PlanNumComparison(automationCondition, patNum))
                    {
                        return false;
                    }

                    break;

                case AutoCondField.ClaimContainsProcCode:
                    if (!DoesClaimContainProcCode(automationCondition, triggerObj))
                    {
                        return false;
                    }

                    break;
            }
        }

        return true;
    }

    private class AutomationCache : CacheListAbs<Automation>
    {
        protected override List<Automation> GetCacheFromDb()
        {
            return AutomationCrud.SelectMany("SELECT * FROM automation");
        }

        protected override List<Automation> TableToList(DataTable dataTable)
        {
            return AutomationCrud.TableToList(dataTable);
        }

        protected override Automation Copy(Automation item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Automation> items)
        {
            return AutomationCrud.ListToTable(items, "Automation");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly AutomationCache Cache = new();

    public static List<Automation> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Automation GetFirstOrDefault(Func<Automation, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    private static bool NeedsSheet(AutomationCondition automationCondition, long patNum)
    {
        var sheets = Sheets.GetForPatientForToday(patNum);

        switch (automationCondition.Comparison)
        {
            case AutoCondComparison.Equals:
                foreach (var sheet in sheets)
                {
                    if (sheet.Description == automationCondition.CompareString)
                    {
                        return true;
                    }
                }

                break;

            case AutoCondComparison.Contains:
                foreach (var sheet in sheets)
                {
                    if (sheet.Description.ToLower().Contains(automationCondition.CompareString.ToLower()))
                    {
                        return true;
                    }
                }

                break;
        }

        return false;
    }

    private static bool ProblemComparison(AutomationCondition automationCondition, long patNum)
    {
        var diseases = Diseases.Refresh(patNum, true);

        switch (automationCondition.Comparison)
        {
            case AutoCondComparison.Equals:
                foreach (var disease in diseases)
                {
                    if (DiseaseDefs.GetName(disease.DiseaseDefNum) == automationCondition.CompareString)
                    {
                        return true;
                    }
                }

                break;

            case AutoCondComparison.Contains:
                foreach (var disease in diseases)
                {
                    if (DiseaseDefs.GetName(disease.DiseaseDefNum).ToLower().Contains(automationCondition.CompareString.ToLower()))
                    {
                        return true;
                    }
                }

                break;
        }

        return false;
    }

    private static bool MedicationComparison(AutomationCondition automationCondition, long patNum)
    {
        var medications = Medications.GetMedicationsByPat(patNum);

        switch (automationCondition.Comparison)
        {
            case AutoCondComparison.Equals:
                foreach (var medication in medications)
                {
                    if (medication.MedName == automationCondition.CompareString)
                    {
                        return true;
                    }
                }

                break;

            case AutoCondComparison.Contains:
                foreach (var medication in medications)
                {
                    if (medication.MedName.ToLower().Contains(automationCondition.CompareString.ToLower()))
                    {
                        return true;
                    }
                }

                break;
        }

        return false;
    }

    private static bool AllergyComparison(AutomationCondition automationCondition, long patNum)
    {
        var allergyDefs = AllergyDefs.GetAllergyDefs(patNum, false);

        return automationCondition.Comparison switch
        {
            AutoCondComparison.Equals =>
                allergyDefs.Any(x => x.Description == automationCondition.CompareString),

            AutoCondComparison.Contains =>
                allergyDefs.Any(x => x.Description.ToLower().Contains(automationCondition.CompareString.ToLower())),

            _ => false
        };
    }

    private static bool AgeComparison(AutomationCondition automationCondition, long patNum)
    {
        var pat = Patients.GetPat(patNum);

        if (!int.TryParse(automationCondition.CompareString, out var ageTrigger))
        {
            return false;
        }

        return automationCondition.Comparison switch
        {
            AutoCondComparison.Equals => pat.Age == ageTrigger,
            AutoCondComparison.Contains => pat.Age.ToString().Contains(automationCondition.CompareString),
            AutoCondComparison.GreaterThan => pat.Age > ageTrigger,
            AutoCondComparison.LessThan => pat.Age < ageTrigger,
            _ => false
        };
    }

    private static bool GenderComparison(AutomationCondition automationCondition, long patNum)
    {
        var pat = Patients.GetPat(patNum);

        return automationCondition.Comparison switch
        {
            AutoCondComparison.Equals =>
                string.Equals(pat.Gender.ToString().Substring(0, 1), automationCondition.CompareString, StringComparison.CurrentCultureIgnoreCase),

            AutoCondComparison.Contains =>
                pat.Gender.ToString().Substring(0, 1).ToLower().Contains(automationCondition.CompareString.ToLower()),

            _ => false
        };
    }

    private static bool InsuranceNotEffectiveComparison(long patNum)
    {
        var patPlan = PatPlans.GetPatPlan(patNum, 1);
        if (patPlan == null)
        {
            return false;
        }

        var insSub = InsSubs.GetOne(patPlan.InsSubNum);
        if (insSub == null)
        {
            return false;
        }

        return DateTime.Today < insSub.DateEffective || DateTime.Today > insSub.DateTerm;
    }

    private static bool BillingTypeComparison(AutomationCondition automationCondition, long patNum)
    {
        var pat = Patients.GetPat(patNum);

        var billTypeDef = Defs.GetDef(DefCat.BillingTypes, pat.BillingType);
        if (billTypeDef is null)
        {
            return false;
        }

        return automationCondition.Comparison switch
        {
            AutoCondComparison.Equals =>
                string.Equals(billTypeDef.ItemName, automationCondition.CompareString, StringComparison.CurrentCultureIgnoreCase),

            AutoCondComparison.Contains =>
                billTypeDef.ItemName.ToLower().Contains(automationCondition.CompareString.ToLower()),

            _ => false
        };
    }

    private static bool PlanNumComparison(AutomationCondition automationCondition, long patNum)
    {
        var patPlans = PatPlans.Refresh(patNum);
        if (patPlans.Count == 0)
        {
            return false;
        }

        var insSubs = InsSubs.GetMany(patPlans.Select(x => x.InsSubNum).ToList());
        return automationCondition.Comparison switch
        {
            AutoCondComparison.Equals =>
                insSubs.Any(x => string.Equals(x.PlanNum.ToString(), automationCondition.CompareString, StringComparison.CurrentCultureIgnoreCase)),

            AutoCondComparison.Contains =>
                insSubs.Any(x => x.PlanNum.ToString().ToLower().Contains(automationCondition.CompareString.ToLower())),

            _ => false
        };
    }

    private static bool DoesClaimContainProcCode<T>(AutomationCondition automationCondition, T triggerObj)
    {
        if (triggerObj is not List<ClaimProc> claimProcs)
        {
            return false;
        }

        try
        {
            var procedures = Procedures.GetManyProc(claimProcs.Select(x => x.ProcNum).ToList(), false);
            var procedureCodesOnClaim = ProcedureCodes.GetCodesForCodeNums(procedures.Select(x => x.CodeNum).ToList());
            var procCodes = procedureCodesOnClaim.Select(x => x.ProcCode).ToList();

            return procCodes.Contains(automationCondition.CompareString);
        }
        catch
        {
            return false;
        }
    }
}