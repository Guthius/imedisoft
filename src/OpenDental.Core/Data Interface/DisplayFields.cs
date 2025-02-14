using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class DisplayFields
{
    public static void Insert(DisplayField displayField)
    {
        DisplayFieldCrud.Insert(displayField);
    }

    public static void Update(DisplayField displayField)
    {
        DisplayFieldCrud.Update(displayField);
    }

    public static void Delete(long displayFieldNum)
    {
        Db.NonQ("DELETE FROM displayfield WHERE DisplayFieldNum = " + displayFieldNum);
    }

    public static void DeleteForChartView(long chartViewNum)
    {
        Db.NonQ("DELETE FROM displayfield WHERE ChartViewNum = " + chartViewNum);
    }

    public static bool IsInUse(DisplayFieldCategory displayFieldCategory, string internalName)
    {
        if (string.IsNullOrEmpty(internalName))
        {
            return false;
        }

        var displayFields = GetForCategory(displayFieldCategory);

        return displayFields.Find(x => x.InternalName == internalName) != null;
    }

    public static List<DisplayField> GetForCategory(DisplayFieldCategory displayFieldCategory)
    {
        var displayFields = GetWhere(x => x.Category == displayFieldCategory);

        return displayFields.Count == 0 ? GetDefaultList(displayFieldCategory) : displayFields;
    }

    public static List<DisplayField> GetForChartView(long chartViewNum)
    {
        var displayFields = GetWhere(x => x.ChartViewNum == chartViewNum && x.Category == DisplayFieldCategory.None);

        return displayFields.Count == 0 ? GetDefaultList(DisplayFieldCategory.None) : displayFields;
    }

    public static List<DisplayField> GetDefaultList(DisplayFieldCategory displayFieldCategory)
    {
        var displayFields = new List<DisplayField>();

        switch (displayFieldCategory)
        {
            case DisplayFieldCategory.None:
                displayFields.Add(new DisplayField("Date", 67, displayFieldCategory));
                displayFields.Add(new DisplayField("Th", 27, displayFieldCategory));
                displayFields.Add(new DisplayField("Surf", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Dx", 28, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 218, displayFieldCategory));
                displayFields.Add(new DisplayField("Stat", 25, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 42, displayFieldCategory));
                displayFields.Add(new DisplayField("Amount", 48, displayFieldCategory));
                displayFields.Add(new DisplayField("Proc Code", 62, displayFieldCategory));
                displayFields.Add(new DisplayField("User", 62, displayFieldCategory));
                displayFields.Add(new DisplayField("Signed", 55, displayFieldCategory));
                break;

            case DisplayFieldCategory.PatientSelect:
                displayFields.Add(new DisplayField("LastName", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("First Name", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("Pref Name", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 30, displayFieldCategory));
                displayFields.Add(new DisplayField("SSN", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Hm Phone", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Wk Phone", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("PatNum", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Address", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Status", 65, displayFieldCategory));
                break;

            case DisplayFieldCategory.PatientInformation:
                displayFields.Add(new DisplayField("Last", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("First", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Middle", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Preferred", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Preferred Pronoun", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Title", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Salutation", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Status", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Gender", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Position", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Birthdate", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("SS#", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Address", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Address2", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("City", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("State", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Zip", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Hm Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Wk Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Wireless Ph", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("E-mail", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Contact Method", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ABC0", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Primary Provider", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec. Provider", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Payor Types", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Language", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Referrals", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Addr/Ph Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("PatFields", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Pat Restrictions", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ICE Name", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ICE Phone", 0, displayFieldCategory));
                break;

            case DisplayFieldCategory.AccountModule:
                displayFields.Add(new DisplayField("Date", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Code", 46, displayFieldCategory));
                displayFields.Add(new DisplayField("Tth", 26, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 270, displayFieldCategory));
                displayFields.Add(new DisplayField("Charges", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Credits", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Balance", 60, displayFieldCategory));
                break;

            case DisplayFieldCategory.RecallList:
                displayFields.Add(new DisplayField("Due Date", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient", 120, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 30, displayFieldCategory));
                displayFields.Add(new DisplayField("Type", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Interval", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("#Remind", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("LastRemind", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("Contact", 120, displayFieldCategory));
                displayFields.Add(new DisplayField("Status", 130, displayFieldCategory));
                displayFields.Add(new DisplayField("Note", 215, displayFieldCategory));
                break;

            case DisplayFieldCategory.ChartPatientInformation:
                displayFields.Add(new DisplayField("Age", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Preferred Pronoun", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ABC0", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Referred From", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Date First Visit", 0, displayFieldCategory));
                if (!PrefC.GetBool(PrefName.EasyHideHospitals))
                {
                    displayFields.Add(new DisplayField("Admit Date", 0, displayFieldCategory));
                    displayFields.Add(new DisplayField("Discharge Date", 0, displayFieldCategory));
                }

                displayFields.Add(new DisplayField("Prov. (Pri, Sec)", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Pri Ins", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec Ins", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Payor Types", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Premedicate", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Problems", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Med Urgent", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Medical Summary", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Service Notes", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Medications", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Allergies", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Pat Restrictions", 0, displayFieldCategory));
                break;

            case DisplayFieldCategory.ProcedureGroupNote:
                displayFields.Add(new DisplayField("Date", 67, displayFieldCategory));
                displayFields.Add(new DisplayField("Th", 27, displayFieldCategory));
                displayFields.Add(new DisplayField("Surf", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 203, displayFieldCategory));
                displayFields.Add(new DisplayField("Stat", 25, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 42, displayFieldCategory));
                displayFields.Add(new DisplayField("Amount", 48, displayFieldCategory));
                displayFields.Add(new DisplayField("Proc Code", 62, displayFieldCategory));
                break;

            case DisplayFieldCategory.TreatmentPlanModule:
                displayFields.Add(new DisplayField("Done", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Priority", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Tth", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Surf", 45, displayFieldCategory));
                displayFields.Add(new DisplayField("Code", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Sub", 28, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 202, displayFieldCategory));
                displayFields.Add(new DisplayField("Fee", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Allowed", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Pri Ins", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec Ins", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("DPlan", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Discount", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("Pat", 50, displayFieldCategory));
                break;

            case DisplayFieldCategory.OrthoChart:
                break;

            case DisplayFieldCategory.AppointmentBubble:
                displayFields.Add(new DisplayField("Patient Name", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient Picture", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Day", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Date", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Time", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Length", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Provider", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Production", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Confirmed", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ASAP", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Med Flag", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Med Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Lab", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Procedures", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Horizontal Line", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("PatNum", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ChartNum", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Home Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Work Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Wireless Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Contact Methods", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Insurance", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Address Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Fam Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Mod Note", 0, displayFieldCategory));
                break;

            case DisplayFieldCategory.AccountPatientInformation:
                break;

            case DisplayFieldCategory.StatementMainGrid:
                var i = 0;
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "date", Description = "Date", ColumnWidth = 75, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "patient", Description = "Patient", ColumnWidth = 100, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "ProcCode", Description = "Code", ColumnWidth = 45, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "tth", Description = "Tooth", ColumnWidth = 45, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "description", Description = "Description", ColumnWidth = 275, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "charges", Description = "Charges", ColumnWidth = 60, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "credits", Description = "Credits", ColumnWidth = 60, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "balance", Description = "Balance", ColumnWidth = 60, ItemOrder = ++i});
                break;

            case DisplayFieldCategory.FamilyRecallGrid:
                displayFields.Add(new DisplayField("Type", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Due Date", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Sched Date", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Notes", 255, displayFieldCategory));
                break;

            case DisplayFieldCategory.AppointmentEdit:
            case DisplayFieldCategory.PlannedAppointmentEdit:
                displayFields.Add(new DisplayField("Stat", 35, displayFieldCategory));
                displayFields.Add(new DisplayField("Priority", 45, displayFieldCategory));
                if (Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum))
                {
                    displayFields.Add(new DisplayField("Code", 125, displayFieldCategory));
                }
                else
                {
                    displayFields.Add(new DisplayField("Tth", 25, displayFieldCategory));
                    displayFields.Add(new DisplayField("Surf", 50, displayFieldCategory));
                    displayFields.Add(new DisplayField("Code", 50, displayFieldCategory));
                }

                displayFields.Add(new DisplayField("Description", 275, displayFieldCategory));
                displayFields.Add(new DisplayField("Fee", 60, displayFieldCategory));
                break;

            case DisplayFieldCategory.OutstandingInsReport:
                displayFields.Add(new DisplayField("Carrier", 145, displayFieldCategory));
                displayFields.Add(new DisplayField("Phone", 95, displayFieldCategory));
                displayFields.Add(new DisplayField("Type", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("User", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("PatName", 115, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("DateService", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("DateSent", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("DateSentOrig", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("TrackStat", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("DateStat", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Error", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Amount", 60, displayFieldCategory));
                break;

            case DisplayFieldCategory.CEMTSearchPatients:
                displayFields.Add(new DisplayField("Conn", 167, displayFieldCategory));
                displayFields.Add(new DisplayField("PatNum", 64, displayFieldCategory));
                displayFields.Add(new DisplayField("LName", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("FName", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("SSN", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("PatStatus", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("City", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("State", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Address", 167, displayFieldCategory));
                displayFields.Add(new DisplayField("Hm Phone", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("Email", 190, displayFieldCategory));
                displayFields.Add(new DisplayField("ChartNum", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Country", 60, displayFieldCategory));
                break;

            case DisplayFieldCategory.ArManagerUnsentGrid:
            case DisplayFieldCategory.ArManagerExcludedGrid:
                displayFields.Add(new DisplayField("Guarantor", 140, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 83, displayFieldCategory));
                displayFields.Add(new DisplayField("0-30 Days", 73, displayFieldCategory));
                displayFields.Add(new DisplayField("31-60 Days", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("61-90 Days", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("> 90 Days", 73, displayFieldCategory));
                displayFields.Add(new DisplayField("Total", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("-Ins Est", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("=Patient", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("PayPlan Due", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Paid", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("DateTime Suspended", 135, displayFieldCategory));
                break;

            case DisplayFieldCategory.ArManagerSentGrid:
                displayFields.Add(new DisplayField("Guarantor", 150, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("0-30 Days", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("31-60 Days", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("61-90 Days", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("> 90 Days", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Total", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("-Ins Est", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("=Patient", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("PayPlan Due", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Paid", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Demand Type", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Transaction", 184, displayFieldCategory));
                break;

            case DisplayFieldCategory.LimitedCustomStatement:
                displayFields.Add(new DisplayField("Date", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Guarantor", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Code", 46, displayFieldCategory));
                displayFields.Add(new DisplayField("Tth", 26, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 270, displayFieldCategory));
                displayFields.Add(new DisplayField("Charges", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Credits", 60, displayFieldCategory));
                break;

            case DisplayFieldCategory.SuperFamilyGridCols:
                displayFields.Add(new DisplayField("Name", 280, displayFieldCategory));
                displayFields.Add(new DisplayField("Stmt", 50, displayFieldCategory));
                break;
        }

        return displayFields;
    }

    public static List<DisplayField> GetAllAvailableList(DisplayFieldCategory displayFieldCategory)
    {
        var displayFields = new List<DisplayField>();
        switch (displayFieldCategory)
        {
            case DisplayFieldCategory.None:
                displayFields.Add(new DisplayField("Date", 67, displayFieldCategory));
                displayFields.Add(new DisplayField("Time", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Th", 27, displayFieldCategory));
                displayFields.Add(new DisplayField("Surf", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Dx", 28, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 218, displayFieldCategory));
                displayFields.Add(new DisplayField("Stat", 25, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 42, displayFieldCategory));
                displayFields.Add(new DisplayField("Amount", 48, displayFieldCategory));
                displayFields.Add(new DisplayField("Proc Code", 62, displayFieldCategory));
                displayFields.Add(new DisplayField("User", 62, displayFieldCategory));
                displayFields.Add(new DisplayField("Signed", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("Priority", 44, displayFieldCategory));
                displayFields.Add(new DisplayField("Date TP", 67, displayFieldCategory));
                displayFields.Add(new DisplayField("Date Entry", 67, displayFieldCategory));
                displayFields.Add(new DisplayField("Prognosis", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Length", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Abbr", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Locked", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("HL7 Sent", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Attachment", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("ClinicDesc", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 90, displayFieldCategory));
                break;

            case DisplayFieldCategory.PatientSelect:
                displayFields.Add(new DisplayField("LastName", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("First Name", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("MI", 25, displayFieldCategory));
                displayFields.Add(new DisplayField("Pref Name", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 30, displayFieldCategory));
                displayFields.Add(new DisplayField("SSN", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Hm Phone", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Wk Phone", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("PatNum", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("ChartNum", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Address", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Status", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Bill Type", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("City", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("State", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("Pri Prov", 85, displayFieldCategory));
                displayFields.Add(new DisplayField("Birthdate", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Site", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Email", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Wireless Ph", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec Prov", 85, displayFieldCategory));
                displayFields.Add(new DisplayField("LastVisit", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("NextVisit", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Invoice Number", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Specialty", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Ward", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("AdmitDate", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("DischargeDate", 100, displayFieldCategory));
                break;

            case DisplayFieldCategory.PatientInformation:
                displayFields.Add(new DisplayField("Last", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("First", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Middle", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Preferred", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Preferred Pronoun", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Title", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Salutation", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Status", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Gender", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Position", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Birthdate", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("SS#", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Address", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Address2", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("City", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("State", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Zip", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Hm Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Wk Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Wireless Ph", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("E-mail", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Contact Method", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ABC0", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Chart Num", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Ward", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("AdmitDate", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("DischargeDate", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Primary Provider", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec. Provider", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Payor Types", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Language", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ResponsParty", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Referrals", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Addr/Ph Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("PatFields", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Guardians", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Arrive Early", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Super Head", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Pat Restrictions", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ICE Name", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ICE Phone", 0, displayFieldCategory));
                break;

            case DisplayFieldCategory.AccountModule:
                displayFields.Add(new DisplayField("Date", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Code", 46, displayFieldCategory));
                displayFields.Add(new DisplayField("Tth", 26, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 270, displayFieldCategory));
                displayFields.Add(new DisplayField("Charges", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Credits", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Balance", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Signed", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Abbr", 110, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("ClinicDesc", 110, displayFieldCategory));
                break;

            case DisplayFieldCategory.RecallList:
                displayFields.Add(new DisplayField("Due Date", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient", 120, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 30, displayFieldCategory));
                displayFields.Add(new DisplayField("Type", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Interval", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("#Remind", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("LastRemind", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("Contact", 120, displayFieldCategory));
                displayFields.Add(new DisplayField("Status", 130, displayFieldCategory));
                displayFields.Add(new DisplayField("Note", 215, displayFieldCategory));
                displayFields.Add(new DisplayField("BillingType", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("WebSched", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Carrier Name", 100, displayFieldCategory));
                break;

            case DisplayFieldCategory.ChartPatientInformation:
                displayFields.Add(new DisplayField("Age", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Preferred Pronoun", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ABC0", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Referred From", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Date First Visit", 0, displayFieldCategory));
                if (!PrefC.GetBool(PrefName.EasyHideHospitals))
                {
                    displayFields.Add(new DisplayField("Admit Date", 0, displayFieldCategory));
                    displayFields.Add(new DisplayField("Discharge Date", 0, displayFieldCategory));
                }

                displayFields.Add(new DisplayField("Prov. (Pri, Sec)", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Pri Ins", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec Ins", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Payor Types", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Premedicate", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Problems", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Med Urgent", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Medical Summary", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Service Notes", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Medications", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Allergies", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("PatFields", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Birthdate", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("City", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("AskToArriveEarly", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Super Head", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient Portal", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Broken Appts", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Pat Restrictions", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Specialty", 0, displayFieldCategory));
                break;

            case DisplayFieldCategory.ProcedureGroupNote:
                displayFields.Add(new DisplayField("Date", 67, displayFieldCategory));
                displayFields.Add(new DisplayField("Th", 27, displayFieldCategory));
                displayFields.Add(new DisplayField("Surf", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 218, displayFieldCategory));
                displayFields.Add(new DisplayField("Stat", 25, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 42, displayFieldCategory));
                displayFields.Add(new DisplayField("Amount", 48, displayFieldCategory));
                displayFields.Add(new DisplayField("Proc Code", 62, displayFieldCategory));
                break;

            case DisplayFieldCategory.TreatmentPlanModule:
                displayFields.Add(new DisplayField("Done", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Priority", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Tth", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Surf", 45, displayFieldCategory));
                displayFields.Add(new DisplayField("Code", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Sub", 28, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 202, displayFieldCategory));
                displayFields.Add(new DisplayField("Fee", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Allowed", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Pri Ins", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec Ins", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("DPlan", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Discount", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("Pat", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Prognosis", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Dx", 28, displayFieldCategory));
                displayFields.Add(new DisplayField("Abbr", 110, displayFieldCategory));
                displayFields.Add(new DisplayField("Tax Est", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("DateTP", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 50, displayFieldCategory));
                displayFields.Add(new DisplayField(InternalNames.TreatmentPlanModule.Appt, 35, displayFieldCategory));
                displayFields.Add(new DisplayField(InternalNames.TreatmentPlanModule.CatPercUcr, 65, displayFieldCategory));
                break;

            case DisplayFieldCategory.OrthoChart:
                displayFields = GetForCategory(DisplayFieldCategory.OrthoChart);
                var distinctFieldNames = OrthoCharts.GetDistinctFieldNames();
                foreach (var fieldName in distinctFieldNames)
                {
                    if (displayFields.Exists(x => x.Description == fieldName)) continue;

                    displayFields.Add(new DisplayField("", 20, DisplayFieldCategory.OrthoChart)
                    {
                        IsNew = true,
                        Description = fieldName
                    });
                }

                break;

            case DisplayFieldCategory.AppointmentBubble:
                displayFields.Add(new DisplayField("Patient Name", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient Picture", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Day", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Date", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Time", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Length", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Provider", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Production", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Net Production", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Confirmed", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ASAP", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Med Flag", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Med Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Lab", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Procedures", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Horizontal Line", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("PatNum", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ChartNum", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Home Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Work Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Wireless Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Contact Methods", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Insurance", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Insurance Color", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Address Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Fam Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Appt Mod Note", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ReferralFrom", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("ReferralTo", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Referral From With Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Referral To With Phone", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Language", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Email", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Discount Plan", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Estimated Patient Portion", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("CareCredit Status", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("Verify Insurance", 0, displayFieldCategory));
                break;

            case DisplayFieldCategory.AccountPatientInformation:
                displayFields.Add(new DisplayField("Billing Type", 0, displayFieldCategory));
                displayFields.Add(new DisplayField("PatFields", 0, displayFieldCategory));
                break;

            case DisplayFieldCategory.StatementMainGrid:
                var i = 0;
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "date", Description = "Date", ColumnWidth = 75, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "prov", Description = "Prov", ColumnWidth = 60, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "patient", Description = "Patient", ColumnWidth = 100, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "ProcCode", Description = "Code", ColumnWidth = 45, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "tth", Description = "Tooth", ColumnWidth = 45, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "description", Description = "Description", ColumnWidth = 275, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "charges", Description = "Charges", ColumnWidth = 60, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "credits", Description = "Credits", ColumnWidth = 60, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "invoiceNum", Description = "Invoice#", ColumnWidth = 60, ItemOrder = ++i});
                displayFields.Add(new DisplayField {Category = displayFieldCategory, InternalName = "balance", Description = "Balance", ColumnWidth = 60, ItemOrder = ++i});
                break;

            case DisplayFieldCategory.FamilyRecallGrid:
                displayFields.Add(new DisplayField("Type", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Due Date", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Sched Date", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Notes", 255, displayFieldCategory));
                displayFields.Add(new DisplayField("Previous Date", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Interval", 80, displayFieldCategory));
                break;

            case DisplayFieldCategory.AppointmentEdit:
                displayFields.Add(new DisplayField("Stat", 35, displayFieldCategory));
                displayFields.Add(new DisplayField("Priority", 45, displayFieldCategory));
                if (Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum))
                {
                    displayFields.Add(new DisplayField("Code", 125, displayFieldCategory));
                }
                else
                {
                    displayFields.Add(new DisplayField("Tth", 25, displayFieldCategory));
                    displayFields.Add(new DisplayField("Surf", 50, displayFieldCategory));
                    displayFields.Add(new DisplayField("Code", 50, displayFieldCategory));
                }

                displayFields.Add(new DisplayField("Description", 255, displayFieldCategory));
                displayFields.Add(new DisplayField("Fee", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Abbreviation", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Layman's Term", 100, displayFieldCategory));
                break;

            case DisplayFieldCategory.PlannedAppointmentEdit:
                displayFields.Add(new DisplayField("Stat", 35, displayFieldCategory));
                displayFields.Add(new DisplayField("Priority", 45, displayFieldCategory));
                if (Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum))
                {
                    displayFields.Add(new DisplayField("Code", 125, displayFieldCategory));
                }
                else
                {
                    displayFields.Add(new DisplayField("Tth", 25, displayFieldCategory));
                    displayFields.Add(new DisplayField("Surf", 50, displayFieldCategory));
                    displayFields.Add(new DisplayField("Code", 50, displayFieldCategory));
                }

                displayFields.Add(new DisplayField("Description", 275, displayFieldCategory));
                displayFields.Add(new DisplayField("Fee", 60, displayFieldCategory));
                displayFields.Add(new DisplayField(InternalNames.PlannedAppointmentEdit.Abbreviation, 80, displayFieldCategory));
                break;

            case DisplayFieldCategory.OutstandingInsReport:
                displayFields.Add(new DisplayField("Carrier", 145, displayFieldCategory));
                displayFields.Add(new DisplayField("Phone", 95, displayFieldCategory));
                displayFields.Add(new DisplayField("Type", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("User", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("PatName", 115, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("DateService", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("DateSent", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("DateSentOrig", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("TrackStat", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("DateStat", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Error", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Amount", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("GroupNum", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("GroupName", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("SubName", 120, displayFieldCategory));
                displayFields.Add(new DisplayField("SubDOB", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("SubID", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("PatDOB", 65, displayFieldCategory));
                break;

            case DisplayFieldCategory.CEMTSearchPatients:
                displayFields.Add(new DisplayField("Conn", 167, displayFieldCategory));
                displayFields.Add(new DisplayField("PatNum", 64, displayFieldCategory));
                displayFields.Add(new DisplayField("LName", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("FName", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("SSN", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("PatStatus", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Age", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("City", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("State", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Address", 167, displayFieldCategory));
                displayFields.Add(new DisplayField("Hm Phone", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("Email", 190, displayFieldCategory));
                displayFields.Add(new DisplayField("ChartNum", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Country", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("MI", 25, displayFieldCategory));
                displayFields.Add(new DisplayField("Pref Name", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Wk Phone", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("Bill Type", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Pri Prov", 85, displayFieldCategory));
                displayFields.Add(new DisplayField("Birthdate", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Site", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Wireless Ph", 94, displayFieldCategory));
                displayFields.Add(new DisplayField("Sec Prov", 85, displayFieldCategory));
                displayFields.Add(new DisplayField("LastVisit", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("NextVisit", 70, displayFieldCategory));
                break;

            case DisplayFieldCategory.ArManagerUnsentGrid:
            case DisplayFieldCategory.ArManagerExcludedGrid:
                displayFields.Add(new DisplayField("Guarantor", 140, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("Billing Type", 83, displayFieldCategory));
                displayFields.Add(new DisplayField("0-30 Days", 73, displayFieldCategory));
                displayFields.Add(new DisplayField("31-60 Days", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("61-90 Days", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("> 90 Days", 73, displayFieldCategory));
                displayFields.Add(new DisplayField("Total", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("-Ins Est", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("=Patient", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("PayPlan Due", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Paid", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("DateTime Suspended", 135, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Proc", 65, displayFieldCategory));
                displayFields.Add(new DisplayField(InternalNames.ArManagerUnsentGrid.DateBalBegan, 95, displayFieldCategory));
                displayFields.Add(new DisplayField(InternalNames.ArManagerUnsentGrid.DaysBalBegan, 95, displayFieldCategory));
                break;

            case DisplayFieldCategory.ArManagerSentGrid:
                displayFields.Add(new DisplayField("Guarantor", 150, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 75, displayFieldCategory));
                displayFields.Add(new DisplayField("0-30 Days", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("31-60 Days", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("61-90 Days", 70, displayFieldCategory));
                displayFields.Add(new DisplayField("> 90 Days", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Total", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("-Ins Est", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("=Patient", 55, displayFieldCategory));
                displayFields.Add(new DisplayField("PayPlan Due", 80, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Paid", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Demand Type", 90, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Transaction", 184, displayFieldCategory));
                displayFields.Add(new DisplayField("Last Proc", 65, displayFieldCategory));
                displayFields.Add(new DisplayField(InternalNames.ArManagerSentGrid.DateBalBegan, 95, displayFieldCategory));
                displayFields.Add(new DisplayField(InternalNames.ArManagerSentGrid.DaysBalBegan, 95, displayFieldCategory));
                break;

            case DisplayFieldCategory.LimitedCustomStatement:
                displayFields.Add(new DisplayField("Date", 65, displayFieldCategory));
                displayFields.Add(new DisplayField("Patient", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Guarantor", 100, displayFieldCategory));
                displayFields.Add(new DisplayField("Prov", 40, displayFieldCategory));
                displayFields.Add(new DisplayField("Clinic", 50, displayFieldCategory));
                displayFields.Add(new DisplayField("Code", 46, displayFieldCategory));
                displayFields.Add(new DisplayField("Tth", 26, displayFieldCategory));
                displayFields.Add(new DisplayField("Description", 270, displayFieldCategory));
                displayFields.Add(new DisplayField("Charges", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Credits", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Signed", 60, displayFieldCategory));
                displayFields.Add(new DisplayField("Abbr", 110, displayFieldCategory));
                break;

            case DisplayFieldCategory.SuperFamilyGridCols:
                displayFields.Add(new DisplayField("Name", 280, displayFieldCategory));
                displayFields.Add(new DisplayField("Stmt", 50, displayFieldCategory));
                var patFieldDefs = PatFieldDefs.GetDeepCopy().FindAll(x => !x.IsHidden);
                foreach (var patFieldDef in patFieldDefs)
                {
                    displayFields.Add(new DisplayField("", 100, DisplayFieldCategory.SuperFamilyGridCols)
                    {
                        Description = patFieldDef.FieldName
                    });
                }

                break;
        }

        return displayFields;
    }

    public static void SaveListForCategory(List<DisplayField> displayFieldsShowing, DisplayFieldCategory displayFieldCategory)
    {
        var isDefault = true;

        var displayFieldsDefault = GetDefaultList(displayFieldCategory);
        if (displayFieldsShowing.Count != displayFieldsDefault.Count)
        {
            isDefault = false;
        }
        else
        {
            for (var i = 0; i < displayFieldsShowing.Count; i++)
            {
                if (displayFieldsShowing[i].Description != "")
                {
                    isDefault = false;
                    break;
                }

                if (displayFieldsShowing[i].InternalName != displayFieldsDefault[i].InternalName)
                {
                    isDefault = false;
                    break;
                }

                if (displayFieldsShowing[i].ColumnWidth != displayFieldsDefault[i].ColumnWidth)
                {
                    isDefault = false;
                    break;
                }
            }
        }

        Db.NonQ("DELETE FROM displayfield WHERE Category = " + (int) displayFieldCategory);
        if (isDefault)
        {
            return;
        }

        for (var i = 0; i < displayFieldsShowing.Count; i++)
        {
            displayFieldsShowing[i].ItemOrder = i;

            Insert(displayFieldsShowing[i]);
        }
    }

    public static void SaveListForChartView(List<DisplayField> displayFieldsShowing, long chartViewNum)
    {
        Db.NonQ("DELETE FROM displayfield WHERE ChartViewNum = " + chartViewNum);

        for (var i = 0; i < displayFieldsShowing.Count; i++)
        {
            displayFieldsShowing[i].ItemOrder = i;
            displayFieldsShowing[i].ChartViewNum = chartViewNum;

            Insert(displayFieldsShowing[i]);
        }
    }

    public class InternalNames
    {
        public class ChartView
        {
            public const string Date = "Date";
            public const string Time = "Time";
            public const string Th = "Th";
            public const string Surf = "Surf";
            public const string Dx = "Dx";
            public const string Description = "Description";
            public const string Stat = "Stat";
            public const string Prov = "Prov";
            public const string Amount = "Amount";
            public const string ProcCode = "Proc Code";
            public const string User = "User";
            public const string Signed = "Signed";
            public const string Priority = "Priority";
            public const string DateEntry = "Date Entry";
            public const string Prognosis = "Prognosis";
            public const string DateTp = "Date TP";
            public const string EndTime = "End Time";
            public const string Quadrant = "Quadrant";
            public const string Length = "Length";
            public const string Abbr = "Abbr";
            public const string Locked = "Locked";
            public const string HL7Sent = "HL7 Sent";
            public const string Clinic = "Clinic";
            public const string ClinicDesc = "ClinicDesc";
        }

        public class ArManagerUnsentGrid
        {
            public const string DateBalBegan = "Date Bal Began";
            public const string DaysBalBegan = "Days Bal Began";
        }

        public class ArManagerSentGrid
        {
            public const string DateBalBegan = "Date Bal Began";
            public const string DaysBalBegan = "Days Bal Began";
        }

        public class ArManagerExcludedGrid
        {
            public const string DateBalBegan = "Date Bal Began";
            public const string DaysBalBegan = "Days Bal Began";
        }

        public class TreatmentPlanModule
        {
            public const string Fee = "Fee";
            public const string Appt = "Appt";
            public const string CatPercUcr = "Cat% UCR";
        }

        public class PlannedAppointmentEdit
        {
            public const string Abbreviation = "Abbreviation";
        }
    }

    private class DisplayFieldCache : CacheListAbs<DisplayField>
    {
        protected override List<DisplayField> GetCacheFromDb()
        {
            return DisplayFieldCrud.SelectMany("SELECT * FROM displayfield ORDER BY ItemOrder");
        }

        protected override List<DisplayField> TableToList(DataTable dataTable)
        {
            return DisplayFieldCrud.TableToList(dataTable);
        }

        protected override DisplayField Copy(DisplayField item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<DisplayField> items)
        {
            return DisplayFieldCrud.ListToTable(items, "DisplayField");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly DisplayFieldCache Cache = new();

    private static List<DisplayField> GetWhere(Predicate<DisplayField> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}