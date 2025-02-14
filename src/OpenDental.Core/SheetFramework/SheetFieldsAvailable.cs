using System;
using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SheetFieldsAvailable
{
    public static List<SheetFieldDef> GetSpecial(SheetTypeEnum sheetType, SheetFieldLayoutMode layoutMode)
    {
        var results = new List<SheetFieldDef>();

        switch (sheetType)
        {
            case SheetTypeEnum.PatientDashboardWidget:
                results.AddRange(
                [
                    SheetFieldDef.NewSpecial("familyInsurance", 0, 0, 193, 80),
                    SheetFieldDef.NewSpecial("individualInsurance", 0, 0, 193, 160),
                    SheetFieldDef.NewSpecial("toothChart", 0, 0, 500, 370),
                    SheetFieldDef.NewSpecial("toothChartLegend", 0, 0, 640, 14)
                ]);
                break;

            case SheetTypeEnum.TreatmentPlan:
            case SheetTypeEnum.ReferralLetter:
                results.AddRange(
                [
                    SheetFieldDef.NewSpecial("toothChart", 0, 0, 500, 370),
                    SheetFieldDef.NewSpecial("toothChartLegend", 0, 0, 640, 14)
                ]);
                break;

            case SheetTypeEnum.ChartModule:
                results.AddRange(
                [
                    SheetFieldDef.NewSpecial("ChartModuleTabs", 0, 0, 524, 259),
                    SheetFieldDef.NewSpecial("TreatmentNotes", 0, 0, 412, 69)
                ]);
                if (layoutMode != SheetFieldLayoutMode.MedicalPractice)
                {
                    results.AddRange(
                    [
                        SheetFieldDef.NewSpecial("toothChart", 0, 0, 410, 307),
                        SheetFieldDef.NewSpecial("TrackToothProcDates", 0, 0, 329, 27)
                    ]);
                }

                break;
        }

        return results;
    }

    public static List<SheetFieldDef> GetList(SheetTypeEnum sheetType, OutInCheck outInCheck)
    {
        return sheetType switch
        {
            SheetTypeEnum.LabelPatient => GetLabelPatient(outInCheck),
            SheetTypeEnum.LabelCarrier => GetLabelCarrier(outInCheck),
            SheetTypeEnum.LabelReferral => GetLabelReferral(outInCheck),
            SheetTypeEnum.ReferralSlip => GetReferralSlip(outInCheck),
            SheetTypeEnum.LabelAppointment => GetLabelAppointment(outInCheck),
            SheetTypeEnum.Consent => GetConsent(outInCheck),
            SheetTypeEnum.PatientLetter => GetPatientLetter(outInCheck),
            SheetTypeEnum.ReferralLetter => GetReferralLetter(outInCheck),
            SheetTypeEnum.PatientForm => GetPatientForm(outInCheck),
            SheetTypeEnum.RoutingSlip => GetRoutingSlip(outInCheck),
            SheetTypeEnum.MedicalHistory => GetMedicalHistory(outInCheck),
            SheetTypeEnum.LabSlip => GetLabSlip(outInCheck),
            SheetTypeEnum.ExamSheet => GetExamSheet(outInCheck),
            SheetTypeEnum.DepositSlip => GetDepositSlip(outInCheck),
            SheetTypeEnum.Statement => GetStatement(outInCheck),
            SheetTypeEnum.MedLabResults => GetMedLabResults(outInCheck),
            SheetTypeEnum.TreatmentPlan => GetTreatmentPlans(outInCheck),
            SheetTypeEnum.PaymentPlan => GetPaymentPlans(outInCheck),
            SheetTypeEnum.ERA => GetEra(outInCheck),
            SheetTypeEnum.ERAGridHeader => GetEraGridHeader(outInCheck),
            _ => []
        };
    }

    public static List<string> GetRadio(string fieldName)
    {
        return fieldName switch
        {
            "Gender" => Enum.GetNames(typeof(PatientGender)).ToList(),
            "ins1Relat" or "ins2Relat" => Enum.GetNames(typeof(Relat)).ToList(),
            "Position" => Enum.GetNames(typeof(PatientPosition)).ToList(),
            "PreferContactMethod" or "PreferConfirmMethod" or "PreferRecallMethod" => Enum.GetNames(typeof(ContactMethod)).ToList(),
            "StudentStatus" => ["Nonstudent", "Parttime", "Fulltime"],
            "Race" => Enum.GetNames(typeof(PatientRaceOld)).ToList(),
            _ => []
        };
    }

    private static SheetFieldDef NewOutput(string fieldName)
    {
        return SheetFieldDef.NewOutput(fieldName, 0, "", false, 0, 0, 0, 0);
    }

    private static SheetFieldDef NewInput(string fieldName)
    {
        return SheetFieldDef.NewInput(fieldName, 0, "", false, 0, 0, 0, 0);
    }

    private static SheetFieldDef NewCheck(string fieldName)
    {
        return SheetFieldDef.NewCheckBox(fieldName, 0, 0, 0, 0);
    }

    private static List<SheetFieldDef> GetLabelPatient(OutInCheck outInCheck)
    {
        if (outInCheck != OutInCheck.Out)
        {
            return [];
        }

        return
        [
            NewOutput("nameFL"),
            NewOutput("nameLF"),
            NewOutput("address"),
            NewOutput("cityStateZip"),
            NewOutput("ChartNumber"),
            NewOutput("PatNum"),
            NewOutput("dateTime.Today"),
            NewOutput("birthdate"),
            NewOutput("priProvName")
        ];
    }

    private static List<SheetFieldDef> GetLabelCarrier(OutInCheck outInCheck)
    {
        if (outInCheck != OutInCheck.Out)
        {
            return [];
        }

        return
        [
            NewOutput("CarrierName"),
            NewOutput("address"),
            NewOutput("cityStateZip")
        ];
    }

    private static List<SheetFieldDef> GetLabelReferral(OutInCheck outInCheck)
    {
        if (outInCheck != OutInCheck.Out)
        {
            return [];
        }

        return
        [
            NewOutput("nameFL"),
            NewOutput("address"),
            NewOutput("cityStateZip")
        ];
    }

    private static List<SheetFieldDef> GetReferralSlip(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("referral.nameFL"));
                sheetFieldDefs.Add(NewOutput("referral.address"));
                sheetFieldDefs.Add(NewOutput("referral.cityStateZip"));
                sheetFieldDefs.Add(NewOutput("referral.phone"));
                sheetFieldDefs.Add(NewOutput("referral.phone2"));
                sheetFieldDefs.Add(NewOutput("patient.nameFL"));
                sheetFieldDefs.Add(NewOutput("dateTime.Today"));
                sheetFieldDefs.Add(NewOutput("patient.WkPhone"));
                sheetFieldDefs.Add(NewOutput("patient.HmPhone"));
                sheetFieldDefs.Add(NewOutput("patient.WirelessPhone"));
                sheetFieldDefs.Add(NewOutput("patient.address"));
                sheetFieldDefs.Add(NewOutput("patient.cityStateZip"));
                sheetFieldDefs.Add(NewOutput("patient.provider"));
                break;

            case OutInCheck.In:
                sheetFieldDefs.Add(NewInput("notes"));
                break;

            case OutInCheck.Check:
                sheetFieldDefs.Add(NewCheck("misc"));
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetLabelAppointment(OutInCheck outInCheck)
    {
        if (outInCheck != OutInCheck.Out)
        {
            return [];
        }

        return
        [
            NewOutput("nameFL"),
            NewOutput("nameLF"),
            NewOutput("weekdayDateTime"),
            NewOutput("length")
        ];
    }

    private static List<SheetFieldDef> GetConsent(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("dateTime.Today"));
                sheetFieldDefs.Add(NewOutput("patient.nameFL"));
                break;

            case OutInCheck.In:
                sheetFieldDefs.Add(NewInput("toothNum"));
                sheetFieldDefs.Add(NewInput("misc"));
                break;

            case OutInCheck.Check:
                sheetFieldDefs.Add(NewCheck("misc"));
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetPatientLetter(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("PracticeTitle"));
                sheetFieldDefs.Add(NewOutput("PracticeAddress"));
                sheetFieldDefs.Add(NewOutput("practiceCityStateZip"));
                sheetFieldDefs.Add(NewOutput("patient.nameFL"));
                sheetFieldDefs.Add(NewOutput("patient.address"));
                sheetFieldDefs.Add(NewOutput("patient.cityStateZip"));
                sheetFieldDefs.Add(NewOutput("today.DayDate"));
                sheetFieldDefs.Add(NewOutput("patient.salutation"));
                sheetFieldDefs.Add(NewOutput("patient.priProvNameFL"));
                break;

            case OutInCheck.In:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetReferralLetter(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("PracticeTitle"));
                sheetFieldDefs.Add(NewOutput("PracticeAddress"));
                sheetFieldDefs.Add(NewOutput("PracticePhoneNumber"));
                sheetFieldDefs.Add(NewOutput("practiceCityStateZip"));
                sheetFieldDefs.Add(NewOutput("referral.phone"));
                sheetFieldDefs.Add(NewOutput("referral.phone2"));
                sheetFieldDefs.Add(NewOutput("referral.nameFL"));
                sheetFieldDefs.Add(NewOutput("referral.nameL"));
                sheetFieldDefs.Add(NewOutput("referral.address"));
                sheetFieldDefs.Add(NewOutput("referral.cityStateZip"));
                sheetFieldDefs.Add(NewOutput("today.DayDate"));
                sheetFieldDefs.Add(NewOutput("patient.nameFL"));
                sheetFieldDefs.Add(NewOutput("referral.salutation"));
                sheetFieldDefs.Add(NewOutput("patient.priProvNameFL"));
                sheetFieldDefs.Add(NewOutput("patient.Birthdate"));
                break;

            case OutInCheck.In:
                break;

            case OutInCheck.Check:
                sheetFieldDefs.Add(NewCheck("misc"));
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetPatientForm(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                break;

            case OutInCheck.In:
                sheetFieldDefs.Add(NewInput("Address"));
                sheetFieldDefs.Add(NewInput("Address2"));
                sheetFieldDefs.Add(NewInput("Birthdate"));
                sheetFieldDefs.Add(NewInput("City"));
                sheetFieldDefs.Add(NewInput("Email"));
                sheetFieldDefs.Add(NewInput("FName"));
                sheetFieldDefs.Add(NewInput("HmPhone"));
                sheetFieldDefs.Add(NewInput("ICEName"));
                sheetFieldDefs.Add(NewInput("ICEPhone"));
                sheetFieldDefs.Add(NewInput("ins1CarrierName"));
                sheetFieldDefs.Add(NewInput("ins1CarrierPhone"));
                sheetFieldDefs.Add(NewInput("ins1EmployerName"));
                sheetFieldDefs.Add(NewInput("ins1GroupName"));
                sheetFieldDefs.Add(NewInput("ins1GroupNum"));
                sheetFieldDefs.Add(NewInput("ins1SubscriberID"));
                sheetFieldDefs.Add(NewInput("ins1SubscriberNameF"));
                sheetFieldDefs.Add(NewInput("ins2CarrierName"));
                sheetFieldDefs.Add(NewInput("ins2CarrierPhone"));
                sheetFieldDefs.Add(NewInput("ins2EmployerName"));
                sheetFieldDefs.Add(NewInput("ins2GroupName"));
                sheetFieldDefs.Add(NewInput("ins2GroupNum"));
                sheetFieldDefs.Add(NewInput("ins2SubscriberID"));
                sheetFieldDefs.Add(NewInput("ins2SubscriberNameF"));
                sheetFieldDefs.Add(NewInput("LName"));
                sheetFieldDefs.Add(NewInput("MiddleI"));
                sheetFieldDefs.Add(NewInput("misc"));
                sheetFieldDefs.Add(NewInput("Preferred"));
                sheetFieldDefs.Add(NewInput("referredFrom"));
                sheetFieldDefs.Add(NewInput("SSN"));
                sheetFieldDefs.Add(NewInput("State"));
                sheetFieldDefs.Add(NewInput("StateNoValidation"));
                sheetFieldDefs.Add(NewInput("WkPhone"));
                sheetFieldDefs.Add(NewInput("WirelessPhone"));
                sheetFieldDefs.Add(NewInput("wirelessCarrier"));
                sheetFieldDefs.Add(NewInput("Zip"));
                break;

            case OutInCheck.Check:
                sheetFieldDefs.Add(NewCheck("addressAndHmPhoneIsSameEntireFamily"));
                sheetFieldDefs.Add(NewCheck("Gender"));
                sheetFieldDefs.Add(NewCheck("ins1Relat"));
                sheetFieldDefs.Add(NewCheck("ins2Relat"));
                sheetFieldDefs.Add(NewCheck("misc"));
                sheetFieldDefs.Add(NewCheck("Position"));
                sheetFieldDefs.Add(NewCheck("PreferConfirmMethod"));
                sheetFieldDefs.Add(NewCheck("PreferContactMethod"));
                sheetFieldDefs.Add(NewCheck("PreferRecallMethod"));
                sheetFieldDefs.Add(NewCheck("StudentStatus"));
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetRoutingSlip(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("appt.timeDate"));
                sheetFieldDefs.Add(NewOutput("appt.length"));
                sheetFieldDefs.Add(NewOutput("appt.providers"));
                sheetFieldDefs.Add(NewOutput("appt.procedures"));
                sheetFieldDefs.Add(NewOutput("appt.Note"));
                sheetFieldDefs.Add(NewOutput("appt.estPatientPortion"));
                sheetFieldDefs.Add(NewOutput("otherFamilyMembers"));
                sheetFieldDefs.Add(NewOutput("labName"));
                sheetFieldDefs.Add(NewOutput("dateLabSent"));
                sheetFieldDefs.Add(NewOutput("dateLabReceived"));
                sheetFieldDefs.Add(NewOutput("referral.FLName"));
                sheetFieldDefs.Add(NewOutput("referral.LName"));
                sheetFieldDefs.Add(NewOutput("referral.address"));
                sheetFieldDefs.Add(NewOutput("referral.cityStateZip"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetMedicalHistory(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                break;

            case OutInCheck.In:
                sheetFieldDefs.Add(NewInput("Birthdate"));
                sheetFieldDefs.Add(NewInput("FName"));
                sheetFieldDefs.Add(NewInput("LName"));
                sheetFieldDefs.Add(NewInput("misc"));
                sheetFieldDefs.Add(NewInput("ICEName"));
                sheetFieldDefs.Add(NewInput("ICEPhone"));
                sheetFieldDefs.Add(NewInput("inputMed1"));
                sheetFieldDefs.Add(NewInput("inputMed2"));
                sheetFieldDefs.Add(NewInput("inputMed3"));
                sheetFieldDefs.Add(NewInput("inputMed4"));
                sheetFieldDefs.Add(NewInput("inputMed5"));
                sheetFieldDefs.Add(NewInput("inputMed6"));
                sheetFieldDefs.Add(NewInput("inputMed7"));
                sheetFieldDefs.Add(NewInput("inputMed8"));
                sheetFieldDefs.Add(NewInput("inputMed9"));
                sheetFieldDefs.Add(NewInput("inputMed10"));
                sheetFieldDefs.Add(NewInput("inputMed11"));
                sheetFieldDefs.Add(NewInput("inputMed12"));
                sheetFieldDefs.Add(NewInput("inputMed13"));
                sheetFieldDefs.Add(NewInput("inputMed14"));
                sheetFieldDefs.Add(NewInput("inputMed15"));
                sheetFieldDefs.Add(NewInput("inputMed16"));
                sheetFieldDefs.Add(NewInput("inputMed17"));
                sheetFieldDefs.Add(NewInput("inputMed18"));
                sheetFieldDefs.Add(NewInput("inputMed19"));
                sheetFieldDefs.Add(NewInput("inputMed20"));
                break;

            case OutInCheck.Check:
                sheetFieldDefs.Add(NewCheck("allergy"));
                sheetFieldDefs.Add(NewCheck("problem"));
                sheetFieldDefs.Add(NewCheck("misc"));
                sheetFieldDefs.Add(NewInput("checkMed1"));
                sheetFieldDefs.Add(NewInput("checkMed2"));
                sheetFieldDefs.Add(NewInput("checkMed3"));
                sheetFieldDefs.Add(NewInput("checkMed4"));
                sheetFieldDefs.Add(NewInput("checkMed5"));
                sheetFieldDefs.Add(NewInput("checkMed6"));
                sheetFieldDefs.Add(NewInput("checkMed7"));
                sheetFieldDefs.Add(NewInput("checkMed8"));
                sheetFieldDefs.Add(NewInput("checkMed9"));
                sheetFieldDefs.Add(NewInput("checkMed10"));
                sheetFieldDefs.Add(NewInput("checkMed11"));
                sheetFieldDefs.Add(NewInput("checkMed12"));
                sheetFieldDefs.Add(NewInput("checkMed13"));
                sheetFieldDefs.Add(NewInput("checkMed14"));
                sheetFieldDefs.Add(NewInput("checkMed15"));
                sheetFieldDefs.Add(NewInput("checkMed16"));
                sheetFieldDefs.Add(NewInput("checkMed17"));
                sheetFieldDefs.Add(NewInput("checkMed18"));
                sheetFieldDefs.Add(NewInput("checkMed19"));
                sheetFieldDefs.Add(NewInput("checkMed20"));
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetLabSlip(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("lab.Description"));
                sheetFieldDefs.Add(NewOutput("lab.Phone"));
                sheetFieldDefs.Add(NewOutput("lab.Notes"));
                sheetFieldDefs.Add(NewOutput("lab.WirelessPhone"));
                sheetFieldDefs.Add(NewOutput("lab.Address"));
                sheetFieldDefs.Add(NewOutput("lab.CityStZip"));
                sheetFieldDefs.Add(NewOutput("lab.Email"));
                sheetFieldDefs.Add(NewOutput("appt.DateTime"));
                sheetFieldDefs.Add(NewOutput("labcase.DateTimeDue"));
                sheetFieldDefs.Add(NewOutput("labcase.DateTimeCreated"));
                sheetFieldDefs.Add(NewOutput("prov.nameFormal"));
                sheetFieldDefs.Add(NewOutput("prov.stateLicence"));
                sheetFieldDefs.Add(NewOutput("labcase.LabCaseNum"));
                break;

            case OutInCheck.In:
                sheetFieldDefs.Add(NewInput("notes"));
                sheetFieldDefs.Add(NewInput("labcase.Instructions"));
                sheetFieldDefs.Add(NewInput("misc"));
                break;

            case OutInCheck.Check:
                sheetFieldDefs.Add(NewCheck("misc"));
                break;
        }

        return sheetFieldDefs;
    }

    public static List<SheetFieldDef> GetExamSheet(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("patient.priProvNameFL"));
                sheetFieldDefs.Add(NewOutput("sheet.DateTimeSheet"));
                break;

            case OutInCheck.In:
                sheetFieldDefs.Add(NewInput("Birthdate"));
                sheetFieldDefs.Add(NewInput("FName"));
                sheetFieldDefs.Add(NewInput("LName"));
                sheetFieldDefs.Add(NewInput("MiddleI"));
                sheetFieldDefs.Add(NewInput("misc"));
                sheetFieldDefs.Add(NewInput("Preferred"));
                break;

            case OutInCheck.Check:
                sheetFieldDefs.Add(NewCheck("Gender"));
                sheetFieldDefs.Add(NewCheck("misc"));
                sheetFieldDefs.Add(NewCheck("Race"));
                break;
        }

        return sheetFieldDefs;
    }

    public static List<SheetFieldDef> GetDepositSlip(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("cashSumTotal"));
                sheetFieldDefs.Add(NewOutput("checkNumber01"));
                sheetFieldDefs.Add(NewOutput("checkNumber02"));
                sheetFieldDefs.Add(NewOutput("checkNumber03"));
                sheetFieldDefs.Add(NewOutput("checkNumber04"));
                sheetFieldDefs.Add(NewOutput("checkNumber05"));
                sheetFieldDefs.Add(NewOutput("checkNumber06"));
                sheetFieldDefs.Add(NewOutput("checkNumber07"));
                sheetFieldDefs.Add(NewOutput("checkNumber08"));
                sheetFieldDefs.Add(NewOutput("checkNumber09"));
                sheetFieldDefs.Add(NewOutput("checkNumber10"));
                sheetFieldDefs.Add(NewOutput("checkNumber11"));
                sheetFieldDefs.Add(NewOutput("checkNumber12"));
                sheetFieldDefs.Add(NewOutput("checkNumber13"));
                sheetFieldDefs.Add(NewOutput("checkNumber14"));
                sheetFieldDefs.Add(NewOutput("checkNumber15"));
                sheetFieldDefs.Add(NewOutput("checkNumber16"));
                sheetFieldDefs.Add(NewOutput("checkNumber17"));
                sheetFieldDefs.Add(NewOutput("checkNumber18"));
                sheetFieldDefs.Add(NewOutput("deposit.BankAccountInfo"));
                sheetFieldDefs.Add(NewOutput("deposit.DateDeposit"));
                sheetFieldDefs.Add(NewOutput("depositList"));
                sheetFieldDefs.Add(NewOutput("depositTotal"));
                sheetFieldDefs.Add(NewOutput("depositItemCount"));
                sheetFieldDefs.Add(NewOutput("depositItem01"));
                sheetFieldDefs.Add(NewOutput("depositItem02"));
                sheetFieldDefs.Add(NewOutput("depositItem03"));
                sheetFieldDefs.Add(NewOutput("depositItem04"));
                sheetFieldDefs.Add(NewOutput("depositItem05"));
                sheetFieldDefs.Add(NewOutput("depositItem06"));
                sheetFieldDefs.Add(NewOutput("depositItem07"));
                sheetFieldDefs.Add(NewOutput("depositItem08"));
                sheetFieldDefs.Add(NewOutput("depositItem09"));
                sheetFieldDefs.Add(NewOutput("depositItem10"));
                sheetFieldDefs.Add(NewOutput("depositItem11"));
                sheetFieldDefs.Add(NewOutput("depositItem12"));
                sheetFieldDefs.Add(NewOutput("depositItem13"));
                sheetFieldDefs.Add(NewOutput("depositItem14"));
                sheetFieldDefs.Add(NewOutput("depositItem15"));
                sheetFieldDefs.Add(NewOutput("depositItem16"));
                sheetFieldDefs.Add(NewOutput("depositItem17"));
                sheetFieldDefs.Add(NewOutput("depositItem18"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetStatement(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("accountNumber"));
                sheetFieldDefs.Add(NewOutput("statement.NoteBold"));
                sheetFieldDefs.Add(NewOutput("statement.Note"));
                sheetFieldDefs.Add(NewOutput("futureAppointments"));
                sheetFieldDefs.Add(NewOutput("totalLabel"));
                sheetFieldDefs.Add(NewOutput("totalValue"));
                sheetFieldDefs.Add(NewOutput("insEstLabel"));
                sheetFieldDefs.Add(NewOutput("insEstValue"));
                sheetFieldDefs.Add(NewOutput("balanceLabel"));
                sheetFieldDefs.Add(NewOutput("balanceValue"));
                sheetFieldDefs.Add(NewOutput("amountDueValue"));
                sheetFieldDefs.Add(NewOutput("invoicePaymentLabel"));
                sheetFieldDefs.Add(NewOutput("invoicePaymentValue"));
                sheetFieldDefs.Add(NewOutput("invoiceTotalLabel"));
                sheetFieldDefs.Add(NewOutput("invoiceTotalValue"));
                sheetFieldDefs.Add(NewOutput("invoicePayPlanLabel"));
                sheetFieldDefs.Add(NewOutput("invoicePayPlanValue"));
                sheetFieldDefs.Add(NewOutput("payPlanAmtDueValue"));
                sheetFieldDefs.Add(NewOutput("statementReceiptInvoice"));
                sheetFieldDefs.Add(NewOutput("returnAddress"));
                sheetFieldDefs.Add(NewOutput("billingAddress"));
                sheetFieldDefs.Add(NewOutput("statement.DateSent"));
                sheetFieldDefs.Add(NewOutput("statementIsCopy"));
                sheetFieldDefs.Add(NewOutput("statementIsTaxReceipt"));
                sheetFieldDefs.Add(NewOutput("providerLegend"));
                sheetFieldDefs.Add(NewOutput("statementURL"));
                sheetFieldDefs.Add(NewOutput("statementShortURL"));
                sheetFieldDefs.Add(NewOutput("StatementNum"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetMedLabResults(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("medlab.ClinicalInfo"));
                sheetFieldDefs.Add(NewOutput("medlab.dateEntered"));
                sheetFieldDefs.Add(NewOutput("medlab.DateTimeCollected"));
                sheetFieldDefs.Add(NewOutput("medlab.DateTimeReported"));
                sheetFieldDefs.Add(NewOutput("medlab.NoteLab"));
                sheetFieldDefs.Add(NewOutput("medlab.obsTests"));
                sheetFieldDefs.Add(NewOutput("medlab.ProvID"));
                sheetFieldDefs.Add(NewOutput("medlab.provNameLF"));
                sheetFieldDefs.Add(NewOutput("medlab.ProvNPI"));
                sheetFieldDefs.Add(NewOutput("medlab.PatAccountNum"));
                sheetFieldDefs.Add(NewOutput("medlab.PatAge"));
                sheetFieldDefs.Add(NewOutput("medlab.PatFasting"));
                sheetFieldDefs.Add(NewOutput("medlab.PatIDAlt"));
                sheetFieldDefs.Add(NewOutput("medlab.PatIDLab"));
                sheetFieldDefs.Add(NewOutput("medlab.SpecimenID"));
                sheetFieldDefs.Add(NewOutput("medlab.SpecimenIDAlt"));
                sheetFieldDefs.Add(NewOutput("medlab.TotalVolume"));
                sheetFieldDefs.Add(NewOutput("medLabFacilityAddr"));
                sheetFieldDefs.Add(NewOutput("medLabFacilityDir"));
                sheetFieldDefs.Add(NewOutput("patient.addrCityStZip"));
                sheetFieldDefs.Add(NewOutput("patient.Birthdate"));
                sheetFieldDefs.Add(NewOutput("patient.FName"));
                sheetFieldDefs.Add(NewOutput("patient.Gender"));
                sheetFieldDefs.Add(NewOutput("patient.HmPhone"));
                sheetFieldDefs.Add(NewOutput("patient.MiddleI"));
                sheetFieldDefs.Add(NewOutput("patient.LName"));
                sheetFieldDefs.Add(NewOutput("patient.PatNum"));
                sheetFieldDefs.Add(NewOutput("patient.SSN"));
                sheetFieldDefs.Add(NewOutput("practiceAddrCityStZip"));
                sheetFieldDefs.Add(NewOutput("PracticePh"));
                sheetFieldDefs.Add(NewOutput("PracticeTitle"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetTreatmentPlans(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("Heading"));
                sheetFieldDefs.Add(NewOutput("defaultHeading"));
                sheetFieldDefs.Add(NewOutput("Note"));
                sheetFieldDefs.Add(NewOutput("tpPatPortionEst"));
                sheetFieldDefs.Add(NewOutput("SignatureText"));
                sheetFieldDefs.Add(NewOutput("SignaturePracticeText"));
                sheetFieldDefs.Add(NewOutput("DateTSigned"));
                sheetFieldDefs.Add(NewOutput("DateTPracticeSigned"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetPaymentPlans(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("PracticeTitle"));
                sheetFieldDefs.Add(NewOutput("dateToday"));
                sheetFieldDefs.Add(NewOutput("nameLF"));
                sheetFieldDefs.Add(NewOutput("guarantor"));
                sheetFieldDefs.Add(NewOutput("Principal"));
                sheetFieldDefs.Add(NewOutput("DateOfAgreement"));
                sheetFieldDefs.Add(NewOutput("APR"));
                sheetFieldDefs.Add(NewOutput("totalFinanceCharge"));
                sheetFieldDefs.Add(NewOutput("totalCostOfLoan"));
                sheetFieldDefs.Add(NewOutput("Note"));
                sheetFieldDefs.Add(NewOutput("ccNumberMaskedWithExp"));
                sheetFieldDefs.Add(NewOutput("TermsAndConditions"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetEra(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("PayerName"));
                sheetFieldDefs.Add(NewOutput("PayerID"));
                sheetFieldDefs.Add(NewOutput("PayerAddress"));
                sheetFieldDefs.Add(NewOutput("PayerCity"));
                sheetFieldDefs.Add(NewOutput("PayerState"));
                sheetFieldDefs.Add(NewOutput("PayerZip"));
                sheetFieldDefs.Add(NewOutput("PayerContactInfo"));
                sheetFieldDefs.Add(NewOutput("PayeeName"));
                sheetFieldDefs.Add(NewOutput("PayeeId"));
                sheetFieldDefs.Add(NewOutput("TransHandlingDesc"));
                sheetFieldDefs.Add(NewOutput("PaymentMethod"));
                sheetFieldDefs.Add(NewOutput("AcctNumEndingIn"));
                sheetFieldDefs.Add(NewOutput("Check#"));
                sheetFieldDefs.Add(NewOutput("DateEffective"));
                sheetFieldDefs.Add(NewOutput("InsPaid"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    private static List<SheetFieldDef> GetEraGridHeader(OutInCheck outInCheck)
    {
        var sheetFieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                sheetFieldDefs.Add(NewOutput("Subscriber"));
                sheetFieldDefs.Add(NewOutput("Patient"));
                sheetFieldDefs.Add(NewOutput("ClaimIdentifier"));
                sheetFieldDefs.Add(NewOutput("PayorControlNum"));
                sheetFieldDefs.Add(NewOutput("Status"));
                sheetFieldDefs.Add(NewOutput("DateService"));
                sheetFieldDefs.Add(NewOutput("ClaimFee"));
                sheetFieldDefs.Add(NewOutput("InsPaid"));
                sheetFieldDefs.Add(NewOutput("PatientResponsibility"));
                sheetFieldDefs.Add(NewOutput("DatePayerReceived"));
                sheetFieldDefs.Add(NewOutput("ClaimIndexNum"));
                break;

            case OutInCheck.In:
            case OutInCheck.Check:
                break;
        }

        return sheetFieldDefs;
    }

    public class Today
    {
        public const string DayDate = "today.DayDate";
    }

    public class Patient
    {
        public const string NameFl = "patient.NameFL";
        public const string Salutation = "patient.Salutation";
        public const string Address = "patient.Address";
        public const string CityStateZip = "patient.CityStateZip";
        public const string HmPhone = "patient.HmPhone";
        public const string Birthdate = "patient.Birthdate";
        public const string PriProvNameFl = "patient.PriProvNameFL";
    }

    public class Prov
    {
        public const string NameFl = "prov.NameFL";
        public const string StateRxId = "prov.StateRxID";
        public const string StateLicense = "prov.StateLicense";
        public const string DeaNum = "prov.DEANum";
        public const string NationalProvId = "prov.NationalProvID";
    }

    public class Clinic
    {
        public const string Address = "clinic.Address";
        public const string CityStateZip = "clinic.CityStateZip";
        public const string Phone = "clinic.Phone";
    }

    public class Practice
    {
        public const string Title = "practice.Title";
        public const string Address = "practice.Address";
        public const string CityStateZip = "practice.CityStateZip";
        public const string Phone = "practice.Phone";
    }
}