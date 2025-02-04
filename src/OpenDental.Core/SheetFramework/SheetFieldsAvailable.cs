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
                results.AddRange([
                    SheetFieldDef.NewSpecial("familyInsurance", 0, 0, 193, 80),
                    SheetFieldDef.NewSpecial("individualInsurance", 0, 0, 193, 160),
                    SheetFieldDef.NewSpecial("toothChart", 0, 0, 500, 370),
                    SheetFieldDef.NewSpecial("toothChartLegend", 0, 0, 640, 14)
                ]);
                break;

            case SheetTypeEnum.TreatmentPlan:
            case SheetTypeEnum.ReferralLetter:
                results.AddRange([
                    SheetFieldDef.NewSpecial("toothChart", 0, 0, 500, 370),
                    SheetFieldDef.NewSpecial("toothChartLegend", 0, 0, 640, 14)
                ]);
                break;

            case SheetTypeEnum.ChartModule:
                results.AddRange([
                    SheetFieldDef.NewSpecial("ChartModuleTabs", 0, 0, 524, 259),
                    SheetFieldDef.NewSpecial("TreatmentNotes", 0, 0, 412, 69)
                ]);
                if (layoutMode != SheetFieldLayoutMode.MedicalPractice)
                {
                    results.AddRange([
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
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("nameFL"));
            list.Add(NewOutput("nameLF"));
            list.Add(NewOutput("address"));
            list.Add(NewOutput("cityStateZip"));
            list.Add(NewOutput("ChartNumber"));
            list.Add(NewOutput("PatNum"));
            list.Add(NewOutput("dateTime.Today"));
            list.Add(NewOutput("birthdate"));
            list.Add(NewOutput("priProvName"));
        }

        return list;
    }

    private static List<SheetFieldDef> GetLabelCarrier(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("CarrierName"));
            list.Add(NewOutput("address"));
            list.Add(NewOutput("cityStateZip"));
        }

        return list;
    }

    private static List<SheetFieldDef> GetLabelReferral(OutInCheck outInCheck)
    {
        var fieldDefs = new List<SheetFieldDef>();
        if (outInCheck != OutInCheck.Out)
        {
            return fieldDefs;
        }

        fieldDefs.Add(NewOutput("nameFL"));
        fieldDefs.Add(NewOutput("address"));
        fieldDefs.Add(NewOutput("cityStateZip"));

        return fieldDefs;
    }

    private static List<SheetFieldDef> GetReferralSlip(OutInCheck outInCheck)
    {
        var fieldDefs = new List<SheetFieldDef>();
        switch (outInCheck)
        {
            case OutInCheck.Out:
                fieldDefs.Add(NewOutput("referral.nameFL"));
                fieldDefs.Add(NewOutput("referral.address"));
                fieldDefs.Add(NewOutput("referral.cityStateZip"));
                fieldDefs.Add(NewOutput("referral.phone"));
                fieldDefs.Add(NewOutput("referral.phone2"));
                fieldDefs.Add(NewOutput("patient.nameFL"));
                fieldDefs.Add(NewOutput("dateTime.Today"));
                fieldDefs.Add(NewOutput("patient.WkPhone"));
                fieldDefs.Add(NewOutput("patient.HmPhone"));
                fieldDefs.Add(NewOutput("patient.WirelessPhone"));
                fieldDefs.Add(NewOutput("patient.address"));
                fieldDefs.Add(NewOutput("patient.cityStateZip"));
                fieldDefs.Add(NewOutput("patient.provider"));
                break;

            case OutInCheck.In:
                fieldDefs.Add(NewInput("notes"));
                break;

            case OutInCheck.Check:
                fieldDefs.Add(NewCheck("misc"));
                break;
        }

        return fieldDefs;
    }

    private static List<SheetFieldDef> GetLabelAppointment(OutInCheck outInCheck)
    {
        var fieldDefs = new List<SheetFieldDef>();
        if (outInCheck != OutInCheck.Out)
        {
            return fieldDefs;
        }

        fieldDefs.Add(NewOutput("nameFL"));
        fieldDefs.Add(NewOutput("nameLF"));
        fieldDefs.Add(NewOutput("weekdayDateTime"));
        fieldDefs.Add(NewOutput("length"));

        return fieldDefs;
    }

    private static List<SheetFieldDef> GetConsent(OutInCheck outInCheck)
    {
        var fieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                fieldDefs.Add(NewOutput("dateTime.Today"));
                fieldDefs.Add(NewOutput("patient.nameFL"));
                break;

            case OutInCheck.In:
                fieldDefs.Add(NewInput("toothNum"));
                fieldDefs.Add(NewInput("misc"));
                break;

            case OutInCheck.Check:
                fieldDefs.Add(NewCheck("misc"));
                break;
        }

        return fieldDefs;
    }

    private static List<SheetFieldDef> GetPatientLetter(OutInCheck outInCheck)
    {
        var fieldDefs = new List<SheetFieldDef>();

        switch (outInCheck)
        {
            case OutInCheck.Out:
                fieldDefs.Add(NewOutput("PracticeTitle"));
                fieldDefs.Add(NewOutput("PracticeAddress"));
                fieldDefs.Add(NewOutput("practiceCityStateZip"));
                fieldDefs.Add(NewOutput("patient.nameFL"));
                fieldDefs.Add(NewOutput("patient.address"));
                fieldDefs.Add(NewOutput("patient.cityStateZip"));
                fieldDefs.Add(NewOutput("today.DayDate"));
                fieldDefs.Add(NewOutput("patient.salutation"));
                fieldDefs.Add(NewOutput("patient.priProvNameFL"));
                break;

            case OutInCheck.In:
                break;
        }

        return fieldDefs;
    }

    private static List<SheetFieldDef> GetReferralLetter(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("PracticeTitle"));
            list.Add(NewOutput("PracticeAddress"));
            list.Add(NewOutput("PracticePhoneNumber"));
            list.Add(NewOutput("practiceCityStateZip"));
            list.Add(NewOutput("referral.phone"));
            list.Add(NewOutput("referral.phone2"));
            list.Add(NewOutput("referral.nameFL"));
            list.Add(NewOutput("referral.nameL"));
            list.Add(NewOutput("referral.address"));
            list.Add(NewOutput("referral.cityStateZip"));
            list.Add(NewOutput("today.DayDate"));
            list.Add(NewOutput("patient.nameFL"));
            list.Add(NewOutput("referral.salutation"));
            list.Add(NewOutput("patient.priProvNameFL"));
            list.Add(NewOutput("patient.Birthdate"));
        }
        else if (outInCheck == OutInCheck.In)
        {
            //none
        }
        else if (outInCheck == OutInCheck.Check)
        {
            list.Add(NewCheck("misc"));
        }

        return list;
    }

    private static List<SheetFieldDef> GetPatientForm(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            //I can't really think of any for this kind				
        }
        else if (outInCheck == OutInCheck.In)
        {
            list.Add(NewInput("Address"));
            list.Add(NewInput("Address2"));
            list.Add(NewInput("Birthdate"));
            list.Add(NewInput("City"));
            list.Add(NewInput("Email"));
            list.Add(NewInput("FName"));
            list.Add(NewInput("HmPhone"));
            list.Add(NewInput("ICEName"));
            list.Add(NewInput("ICEPhone"));
            list.Add(NewInput("ins1CarrierName"));
            list.Add(NewInput("ins1CarrierPhone"));
            list.Add(NewInput("ins1EmployerName"));
            list.Add(NewInput("ins1GroupName"));
            list.Add(NewInput("ins1GroupNum"));
            list.Add(NewInput("ins1SubscriberID"));
            list.Add(NewInput("ins1SubscriberNameF"));
            list.Add(NewInput("ins2CarrierName"));
            list.Add(NewInput("ins2CarrierPhone"));
            list.Add(NewInput("ins2EmployerName"));
            list.Add(NewInput("ins2GroupName"));
            list.Add(NewInput("ins2GroupNum"));
            list.Add(NewInput("ins2SubscriberID"));
            list.Add(NewInput("ins2SubscriberNameF"));
            list.Add(NewInput("LName"));
            list.Add(NewInput("MiddleI"));
            list.Add(NewInput("misc"));
            list.Add(NewInput("Preferred"));
            list.Add(NewInput("referredFrom"));
            list.Add(NewInput("SSN"));
            list.Add(NewInput("State"));
            list.Add(NewInput("StateNoValidation"));
            list.Add(NewInput("WkPhone"));
            list.Add(NewInput("WirelessPhone"));
            list.Add(NewInput("wirelessCarrier"));
            list.Add(NewInput("Zip"));
        }
        else if (outInCheck == OutInCheck.Check)
        {
            list.Add(NewCheck("addressAndHmPhoneIsSameEntireFamily"));
            list.Add(NewCheck("Gender"));
            list.Add(NewCheck("ins1Relat"));
            list.Add(NewCheck("ins2Relat"));
            list.Add(NewCheck("misc"));
            list.Add(NewCheck("Position"));
            list.Add(NewCheck("PreferConfirmMethod"));
            list.Add(NewCheck("PreferContactMethod"));
            list.Add(NewCheck("PreferRecallMethod"));
            list.Add(NewCheck("StudentStatus"));
        }

        return list;
    }

    private static List<SheetFieldDef> GetRoutingSlip(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("appt.timeDate"));
            list.Add(NewOutput("appt.length"));
            list.Add(NewOutput("appt.providers"));
            list.Add(NewOutput("appt.procedures"));
            list.Add(NewOutput("appt.Note"));
            list.Add(NewOutput("appt.estPatientPortion"));
            list.Add(NewOutput("otherFamilyMembers"));
            list.Add(NewOutput("labName"));
            list.Add(NewOutput("dateLabSent"));
            list.Add(NewOutput("dateLabReceived"));
            list.Add(NewOutput("referral.FLName"));
            list.Add(NewOutput("referral.LName"));
            list.Add(NewOutput("referral.address"));
            list.Add(NewOutput("referral.cityStateZip"));
            //most fields turned out to work best as static text.
        }
        else if (outInCheck == OutInCheck.In)
        {
            //Not applicable
        }
        else if (outInCheck == OutInCheck.Check)
        {
            //Not applicable
        }

        return list;
    }

    private static List<SheetFieldDef> GetMedicalHistory(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            //none
        }
        else if (outInCheck == OutInCheck.In)
        {
            list.Add(NewInput("Birthdate"));
            list.Add(NewInput("FName"));
            list.Add(NewInput("LName"));
            list.Add(NewInput("misc"));
            list.Add(NewInput("ICEName"));
            list.Add(NewInput("ICEPhone"));
            list.Add(NewInput("inputMed1"));
            list.Add(NewInput("inputMed2"));
            list.Add(NewInput("inputMed3"));
            list.Add(NewInput("inputMed4"));
            list.Add(NewInput("inputMed5"));
            list.Add(NewInput("inputMed6"));
            list.Add(NewInput("inputMed7"));
            list.Add(NewInput("inputMed8"));
            list.Add(NewInput("inputMed9"));
            list.Add(NewInput("inputMed10"));
            list.Add(NewInput("inputMed11"));
            list.Add(NewInput("inputMed12"));
            list.Add(NewInput("inputMed13"));
            list.Add(NewInput("inputMed14"));
            list.Add(NewInput("inputMed15"));
            list.Add(NewInput("inputMed16"));
            list.Add(NewInput("inputMed17"));
            list.Add(NewInput("inputMed18"));
            list.Add(NewInput("inputMed19"));
            list.Add(NewInput("inputMed20"));
        }
        else if (outInCheck == OutInCheck.Check)
        {
            list.Add(NewCheck("allergy"));
            list.Add(NewCheck("problem"));
            list.Add(NewCheck("misc"));
            list.Add(NewInput("checkMed1"));
            list.Add(NewInput("checkMed2"));
            list.Add(NewInput("checkMed3"));
            list.Add(NewInput("checkMed4"));
            list.Add(NewInput("checkMed5"));
            list.Add(NewInput("checkMed6"));
            list.Add(NewInput("checkMed7"));
            list.Add(NewInput("checkMed8"));
            list.Add(NewInput("checkMed9"));
            list.Add(NewInput("checkMed10"));
            list.Add(NewInput("checkMed11"));
            list.Add(NewInput("checkMed12"));
            list.Add(NewInput("checkMed13"));
            list.Add(NewInput("checkMed14"));
            list.Add(NewInput("checkMed15"));
            list.Add(NewInput("checkMed16"));
            list.Add(NewInput("checkMed17"));
            list.Add(NewInput("checkMed18"));
            list.Add(NewInput("checkMed19"));
            list.Add(NewInput("checkMed20"));
        }

        return list;
    }

    private static List<SheetFieldDef> GetLabSlip(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("lab.Description"));
            list.Add(NewOutput("lab.Phone"));
            list.Add(NewOutput("lab.Notes"));
            list.Add(NewOutput("lab.WirelessPhone"));
            list.Add(NewOutput("lab.Address"));
            list.Add(NewOutput("lab.CityStZip"));
            list.Add(NewOutput("lab.Email"));
            list.Add(NewOutput("appt.DateTime"));
            list.Add(NewOutput("labcase.DateTimeDue"));
            list.Add(NewOutput("labcase.DateTimeCreated"));
            list.Add(NewOutput("prov.nameFormal"));
            list.Add(NewOutput("prov.stateLicence"));
            list.Add(NewOutput("labcase.LabCaseNum"));
            //patient fields already handled with static text: name,age,gender.
            //other fields already handled: dateToday, practice address and phone.
        }
        else if (outInCheck == OutInCheck.In)
        {
            list.Add(NewInput("notes"));
            list.Add(NewInput("labcase.Instructions"));
            list.Add(NewInput("misc"));
        }
        else if (outInCheck == OutInCheck.Check)
        {
            list.Add(NewCheck("misc"));
        }

        return list;
    }

    public static List<SheetFieldDef> GetExamSheet(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("patient.priProvNameFL"));
            list.Add(NewOutput("sheet.DateTimeSheet"));
        }
        else if (outInCheck == OutInCheck.In)
        {
            list.Add(NewInput("Birthdate"));
            list.Add(NewInput("FName"));
            list.Add(NewInput("LName"));
            list.Add(NewInput("MiddleI"));
            list.Add(NewInput("misc"));
            list.Add(NewInput("Preferred"));
        }
        else if (outInCheck == OutInCheck.Check)
        {
            list.Add(NewCheck("Gender"));
            list.Add(NewCheck("misc"));
            list.Add(NewCheck("Race")); //This is really race/ethnicity combined.
        }

        return list;
    }

    public static List<SheetFieldDef> GetDepositSlip(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("cashSumTotal"));
            list.Add(NewOutput("checkNumber01"));
            list.Add(NewOutput("checkNumber02"));
            list.Add(NewOutput("checkNumber03"));
            list.Add(NewOutput("checkNumber04"));
            list.Add(NewOutput("checkNumber05"));
            list.Add(NewOutput("checkNumber06"));
            list.Add(NewOutput("checkNumber07"));
            list.Add(NewOutput("checkNumber08"));
            list.Add(NewOutput("checkNumber09"));
            list.Add(NewOutput("checkNumber10"));
            list.Add(NewOutput("checkNumber11"));
            list.Add(NewOutput("checkNumber12"));
            list.Add(NewOutput("checkNumber13"));
            list.Add(NewOutput("checkNumber14"));
            list.Add(NewOutput("checkNumber15"));
            list.Add(NewOutput("checkNumber16"));
            list.Add(NewOutput("checkNumber17"));
            list.Add(NewOutput("checkNumber18"));
            list.Add(NewOutput("deposit.BankAccountInfo"));
            list.Add(NewOutput("deposit.DateDeposit"));
            list.Add(NewOutput("depositList"));
            list.Add(NewOutput("depositTotal"));
            list.Add(NewOutput("depositItemCount"));
            list.Add(NewOutput("depositItem01"));
            list.Add(NewOutput("depositItem02"));
            list.Add(NewOutput("depositItem03"));
            list.Add(NewOutput("depositItem04"));
            list.Add(NewOutput("depositItem05"));
            list.Add(NewOutput("depositItem06"));
            list.Add(NewOutput("depositItem07"));
            list.Add(NewOutput("depositItem08"));
            list.Add(NewOutput("depositItem09"));
            list.Add(NewOutput("depositItem10"));
            list.Add(NewOutput("depositItem11"));
            list.Add(NewOutput("depositItem12"));
            list.Add(NewOutput("depositItem13"));
            list.Add(NewOutput("depositItem14"));
            list.Add(NewOutput("depositItem15"));
            list.Add(NewOutput("depositItem16"));
            list.Add(NewOutput("depositItem17"));
            list.Add(NewOutput("depositItem18"));
        }
        else if (outInCheck == OutInCheck.In)
        {
        }
        else if (outInCheck == OutInCheck.Check)
        {
        }

        return list;
    }

    private static List<SheetFieldDef> GetStatement(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("accountNumber"));
            list.Add(NewOutput("statement.NoteBold"));
            list.Add(NewOutput("statement.Note"));
            list.Add(NewOutput("futureAppointments"));
            list.Add(NewOutput("totalLabel"));
            list.Add(NewOutput("totalValue"));
            list.Add(NewOutput("insEstLabel"));
            list.Add(NewOutput("insEstValue"));
            list.Add(NewOutput("balanceLabel"));
            list.Add(NewOutput("balanceValue"));
            list.Add(NewOutput("amountDueValue"));
            list.Add(NewOutput("invoicePaymentLabel")); //only for invoices
            list.Add(NewOutput("invoicePaymentValue")); //only for invoices
            list.Add(NewOutput("invoiceTotalLabel")); //only for invoices
            list.Add(NewOutput("invoiceTotalValue")); //only for invoices
            list.Add(NewOutput("invoicePayPlanLabel")); //only for invoices
            list.Add(NewOutput("invoicePayPlanValue")); //only for invoices
            list.Add(NewOutput("payPlanAmtDueValue"));
            list.Add(NewOutput("statementReceiptInvoice"));
            list.Add(NewOutput("returnAddress"));
            list.Add(NewOutput("billingAddress"));
            list.Add(NewOutput("statement.DateSent"));
            list.Add(NewOutput("statementIsCopy"));
            list.Add(NewOutput("statementIsTaxReceipt"));
            list.Add(NewOutput("providerLegend"));
            list.Add(NewOutput("statementURL"));
            list.Add(NewOutput("statementShortURL"));
            list.Add(NewOutput("StatementNum"));
        }
        else if (outInCheck == OutInCheck.In)
        {
        }
        else if (outInCheck == OutInCheck.Check)
        {
        }

        return list;
    }

    private static List<SheetFieldDef> GetMedLabResults(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("medlab.ClinicalInfo"));
            list.Add(NewOutput("medlab.dateEntered"));
            list.Add(NewOutput("medlab.DateTimeCollected"));
            list.Add(NewOutput("medlab.DateTimeReported"));
            list.Add(NewOutput("medlab.NoteLab"));
            list.Add(NewOutput("medlab.obsTests"));
            list.Add(NewOutput("medlab.ProvID"));
            list.Add(NewOutput("medlab.provNameLF"));
            list.Add(NewOutput("medlab.ProvNPI"));
            list.Add(NewOutput("medlab.PatAccountNum"));
            list.Add(NewOutput("medlab.PatAge"));
            list.Add(NewOutput("medlab.PatFasting"));
            list.Add(NewOutput("medlab.PatIDAlt"));
            list.Add(NewOutput("medlab.PatIDLab"));
            list.Add(NewOutput("medlab.SpecimenID"));
            list.Add(NewOutput("medlab.SpecimenIDAlt"));
            list.Add(NewOutput("medlab.TotalVolume"));
            list.Add(NewOutput("medLabFacilityAddr"));
            list.Add(NewOutput("medLabFacilityDir"));
            list.Add(NewOutput("patient.addrCityStZip"));
            list.Add(NewOutput("patient.Birthdate"));
            list.Add(NewOutput("patient.FName"));
            list.Add(NewOutput("patient.Gender"));
            list.Add(NewOutput("patient.HmPhone"));
            list.Add(NewOutput("patient.MiddleI"));
            list.Add(NewOutput("patient.LName"));
            list.Add(NewOutput("patient.PatNum"));
            list.Add(NewOutput("patient.SSN"));
            list.Add(NewOutput("practiceAddrCityStZip"));
            list.Add(NewOutput("PracticePh"));
            list.Add(NewOutput("PracticeTitle"));
        }
        else if (outInCheck == OutInCheck.In)
        {
        }
        else if (outInCheck == OutInCheck.Check)
        {
        }

        return list;
    }

    private static List<SheetFieldDef> GetTreatmentPlans(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("Heading"));
            list.Add(NewOutput("defaultHeading"));
            list.Add(NewOutput("Note"));
            list.Add(NewOutput("tpPatPortionEst"));
            list.Add(NewOutput("SignatureText"));
            list.Add(NewOutput("SignaturePracticeText"));
            list.Add(NewOutput("DateTSigned"));
            list.Add(NewOutput("DateTPracticeSigned"));
        }
        else if (outInCheck == OutInCheck.In)
        {
        }
        else if (outInCheck == OutInCheck.Check)
        {
        }

        return list;
    }

    private static List<SheetFieldDef> GetPaymentPlans(OutInCheck outInCheck)
    {
        var list = new List<SheetFieldDef>();
        if (outInCheck == OutInCheck.Out)
        {
            list.Add(NewOutput("PracticeTitle"));
            list.Add(NewOutput("dateToday"));
            list.Add(NewOutput("nameLF"));
            list.Add(NewOutput("guarantor"));
            list.Add(NewOutput("Principal"));
            list.Add(NewOutput("DateOfAgreement"));
            list.Add(NewOutput("APR"));
            list.Add(NewOutput("totalFinanceCharge"));
            list.Add(NewOutput("totalCostOfLoan"));
            list.Add(NewOutput("Note"));
            list.Add(NewOutput("ccNumberMaskedWithExp"));
            list.Add(NewOutput("TermsAndConditions"));
        }
        else if (outInCheck == OutInCheck.In)
        {
        }
        else if (outInCheck == OutInCheck.Check)
        {
        }

        return list;
    }

    private static List<SheetFieldDef> GetEra(OutInCheck outInCheck)
    {
        var retList = new List<SheetFieldDef>();
        switch (outInCheck)
        {
            case OutInCheck.Out:
                retList.Add(NewOutput("PayerName"));
                retList.Add(NewOutput("PayerID"));
                retList.Add(NewOutput("PayerAddress"));
                retList.Add(NewOutput("PayerCity"));
                retList.Add(NewOutput("PayerState"));
                retList.Add(NewOutput("PayerZip"));
                retList.Add(NewOutput("PayerContactInfo"));
                retList.Add(NewOutput("PayeeName"));
                retList.Add(NewOutput("PayeeId"));
                retList.Add(NewOutput("TransHandlingDesc"));
                retList.Add(NewOutput("PaymentMethod"));
                retList.Add(NewOutput("AcctNumEndingIn"));
                retList.Add(NewOutput("Check#"));
                retList.Add(NewOutput("DateEffective"));
                retList.Add(NewOutput("InsPaid"));
                break;
            case OutInCheck.In:
                //none
                break;
            case OutInCheck.Check:
                //none
                break;
        }

        return retList;
    }

    private static List<SheetFieldDef> GetEraGridHeader(OutInCheck outInCheck)
    {
        var retList = new List<SheetFieldDef>();
        switch (outInCheck)
        {
            case OutInCheck.Out:
                retList.Add(NewOutput("Subscriber"));
                retList.Add(NewOutput("Patient"));
                retList.Add(NewOutput("ClaimIdentifier"));
                retList.Add(NewOutput("PayorControlNum"));
                retList.Add(NewOutput("Status"));
                retList.Add(NewOutput("DateService"));
                retList.Add(NewOutput("ClaimFee"));
                retList.Add(NewOutput("InsPaid"));
                retList.Add(NewOutput("PatientResponsibility"));
                retList.Add(NewOutput("DatePayerReceived"));
                retList.Add(NewOutput("ClaimIndexNum"));
                break;
            case OutInCheck.In:
                //none
                break;
            case OutInCheck.Check:
                //none
                break;
        }

        return retList;
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