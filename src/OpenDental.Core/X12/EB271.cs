using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class EB271
{
    private static readonly List<EB01> Eb01 =
    [
        new("1", "Active Coverage", InsBenefitType.ActiveCoverage),
        new("2", "Active - Full Risk Capitation", InsBenefitType.ActiveCoverage),
        new("3", "Active - Services Capitated", InsBenefitType.ActiveCoverage),
        new("4", "Active - Services Capitated to Primary Care Physician"),
        new("5", "Active - Pending Investigation"),
        new("6", "Inactive"),
        new("7", "Inactive - Pending Eligibility Update"),
        new("8", "Inactive - Pending Investigation"),
        new("A", "Co-Insurance", InsBenefitType.CoInsurance),
        new("B", "Co-Payment", InsBenefitType.CoPayment),
        new("C", "Deductible", InsBenefitType.Deductible),
        new("CB", "Coverage Basis"),
        new("D", "Benefit Description"),
        new("E", "Exclusions", InsBenefitType.Exclusions),
        new("F", "Limitations", InsBenefitType.Limitations),
        new("G", "Out of Pocket (Stop Loss)"),
        new("H", "Unlimited"),
        new("I", "Non-Covered", InsBenefitType.Exclusions),
        new("J", "Cost Containment"),
        new("K", "Reserve"),
        new("L", "Primary Care Provider"),
        new("M", "Pre-existing Condition"),
        new("MC", "Managed Care Coordinator"),
        new("N", "Services Restricted to Following Provider"),
        new("O", "Not Deemed a Medical Necessity"),
        new("P", "Benefit Disclaimer"),
        new("Q", "Second Surgical Opinion Required"),
        new("R", "Other or Additional Payor"),
        new("S", "Prior Year(s) History"),
        new("T", "Card(s) Reported Lost/Stolen"),
        new("U", "Contact Following Entity for Information"),
        new("V", "Cannot Process"),
        new("W", "Other Source of Data"),
        new("X", "Health Care Facility"),
        new("Y", "Spend Down")
    ];

    private static readonly List<EB02> Eb02 =
    [
        new("CHD", "Children Only"),
        new("DEP", "Dependents Only"),
        new("ECH", "Employee and Children", BenefitCoverageLevel.Family),
        new("EMP", "Employee Only"),
        new("ESP", "Employee and Spouse", BenefitCoverageLevel.Family),
        new("FAM", "Family", BenefitCoverageLevel.Family),
        new("IND", "Individual", BenefitCoverageLevel.Individual),
        new("SPC", "Spouse and Children"),
        new("SPO", "Spouse Only")
    ];

    private static readonly List<EB03> Eb03 =
    [
        new("1", "Medical Care"),
        new("2", "Surgical"),
        new("3", "Consultation"),
        new("4", "Diagnostic X-Ray", EbenefitCategory.DiagnosticXRay),
        new("5", "Diagnostic Lab"),
        new("6", "Radiation Therapy"),
        new("7", "Anesthesia"),
        new("8", "Surgical Assistance"),
        new("9", "Other Medical"),
        new("10", "Blood Charges"),
        new("11", "Used Durable Medical Equipment"),
        new("12", "Durable Medical Equipment Purchase"),
        new("13", "Ambulatory Service Center Facility"),
        new("14", "Renal Supplies in the Home"),
        new("15", "Alternate Method Dialysis"),
        new("16", "Chronic Renal Disease (CRD) Equipment"),
        new("17", "Pre-Admission Testing"),
        new("18", "Durable Medical Equipment Rental"),
        new("19", "Pneumonia Vaccine"),
        new("20", "Second Surgical Opinion"),
        new("21", "Third Surgical Opinion"),
        new("22", "Social Work"),
        new("23", "Diagnostic Dental", EbenefitCategory.Diagnostic),
        new("24", "Periodontics", EbenefitCategory.Periodontics),
        new("25", "Restorative", EbenefitCategory.Restorative),
        new("26", "Endodontics", EbenefitCategory.Endodontics),
        new("27", "Maxillofacial Prosthetics", EbenefitCategory.MaxillofacialProsth),
        new("28", "Adjunctive Dental Services", EbenefitCategory.Adjunctive),
        new("30", "Health Benefit Plan Coverage", EbenefitCategory.General),
        new("32", "Plan Waiting Period"),
        new("33", "Chiropractic"),
        new("34", "Chiropractic Office Visits"),
        new("35", "Dental Care", EbenefitCategory.General),
        new("36", "Dental Crowns", EbenefitCategory.Crowns),
        new("37", "Dental Accident", EbenefitCategory.Accident),
        new("38", "Orthodontics", EbenefitCategory.Orthodontics),
        new("39", "Prosthodontics", EbenefitCategory.Prosthodontics),
        new("40", "Oral Surgery", EbenefitCategory.OralSurgery),
        new("41", "Routine (Preventive) Dental", EbenefitCategory.RoutinePreventive),
        new("42", "Home Health Care"),
        new("43", "Home Health Prescriptions"),
        new("44", "Home Health Visits"),
        new("45", "Hospice"),
        new("46", "Respite Care"),
        new("47", "Hospital"),
        new("48", "Hospital - Inpatient"),
        new("49", "Hospital - Room and Board"),
        new("50", "Hospital - Outpatient"),
        new("51", "Hospital - Emergency Accident"),
        new("52", "Hospital - Emergency Medical"),
        new("53", "Hospital - Ambulatory Surgical"),
        new("54", "Long Term Care"),
        new("55", "Major Medical"),
        new("56", "Medically Related Transportation"),
        new("57", "Air Transportation"),
        new("58", "Cabulance"),
        new("59", "Licensed Ambulance"),
        new("60", "General Benefits"),
        new("61", "In-vitro Fertilization"),
        new("62", "MRI/CAT Scan"),
        new("63", "Donor Procedures"),
        new("64", "Acupuncture"),
        new("65", "Newborn Care"),
        new("66", "Pathology"),
        new("67", "Smoking Cessation"),
        new("68", "Well Baby Care"),
        new("69", "Maternity"),
        new("70", "Transplants"),
        new("71", "Audiology Exam"),
        new("72", "Inhalation Therapy"),
        new("73", "Diagnostic Medical"),
        new("74", "Private Duty Nursing"),
        new("75", "Prosthetic Device"),
        new("76", "Dialysis"),
        new("77", "Otological Exam"),
        new("78", "Chemotherapy"),
        new("79", "Allergy Testing"),
        new("80", "Immunizations"),
        new("81", "Routine Physical"),
        new("82", "Family Planning"),
        new("83", "Infertility"),
        new("84", "Abortion"),
        new("85", "AIDS"),
        new("86", "Emergency Services"),
        new("87", "Cancer"),
        new("88", "Pharmacy"),
        new("89", "Free Standing Prescription Drug"),
        new("90", "Mail Order Prescription Drug"),
        new("91", "Brand Name Prescription Drug"),
        new("92", "Generic Prescription Drug"),
        new("93", "Podiatry"),
        new("94", "Podiatry - Office Visits"),
        new("95", "Podiatry - Nursing Home Visits"),
        new("96", "Professional (Physician)"),
        new("97", "Anesthesiologist"),
        new("98", "Professional (Physician) Visit - Office"),
        new("99", "Professional (Physician) Visit - Inpatient"),
        new("A0", "Professional (Physician) Visit - Outpatient"),
        new("A1", "Professional (Physician) Visit - Nursing Home"),
        new("A2", "Professional (Physician) Visit - Skilled Nursing Facility"),
        new("A3", "Professional (Physician) Visit - Home"),
        new("A4", "Psychiatric"),
        new("A5", "Psychiatric - Room and Board"),
        new("A6", "Psychotherapy"),
        new("A7", "Psychiatric - Inpatient"),
        new("A8", "Psychiatric - Outpatient"),
        new("A9", "Rehabilitation"),
        new("AA", "Rehabilitation - Room and Board"),
        new("AB", "Rehabilitation - Inpatient"),
        new("AC", "Rehabilitation - Outpatient"),
        new("AD", "Occupational Therapy"),
        new("AE", "Physical Medicine"),
        new("AF", "Speech Therapy"),
        new("AG", "Skilled Nursing Care"),
        new("AH", "Skilled Nursing Care - Room and Board"),
        new("AI", "Substance Abuse"),
        new("AJ", "Alcoholism"),
        new("AK", "Drug Addiction"),
        new("AL", "Vision (Optometry)"),
        new("AM", "Frames"),
        new("AN", "Routine Exam"),
        new("AO", "Lenses"),
        new("AQ", "Nonmedically Necessary Physical"),
        new("AR", "Experimental Drug Therapy"),
        new("BA", "Independent Medical Evaluation"),
        new("BB", "Partial Hospitalization (Psychiatric)"),
        new("BC", "Day Care (Psychiatric)"),
        new("BD", "Cognitive Therapy"),
        new("BE", "Massage Therapy"),
        new("BF", "Pulmonary Rehabilitation"),
        new("BG", "Cardiac Rehabilitation"),
        new("BH", "Pediatric"),
        new("BI", "Nursery"),
        new("BJ", "Skin"),
        new("BK", "Orthopedic"),
        new("BL", "Cardiac"),
        new("BM", "Lymphatic"),
        new("BN", "Gastrointestinal"),
        new("BP", "Endocrine"),
        new("BQ", "Neurology"),
        new("BR", "Eye"),
        new("BS", "Invasive Procedures")
    ];

    private static readonly Dictionary<string, string> Eb04 = new()
    {
        {"12", "Medicare Secondary Working Aged Beneficiary or Spouse with Employer Group Health Plan"},
        {"13", "Medicare Secondary End-Stage Renal Disease Beneficiary in the 12 month coordination period with an employer�s group health plan"},
        {"14", "Medicare Secondary, No-fault Insurance including Auto is Primary"},
        {"15", "Medicare Secondary Worker�s Compensation"},
        {"16", "Medicare Secondary Public Health Service (PHS)or Other Federal Agency"},
        {"41", "Medicare Secondary Black Lung"},
        {"42", "Medicare Secondary Veteran�s Administration"},
        {"43", "Medicare Secondary Disabled Beneficiary Under Age 65 with Large Group Health Plan (LGHP)"},
        {"47", "Medicare Secondary, Other Liability Insurance is Primary"},
        {"AP", "Auto Insurance Policy"},
        {"C1", "Commercial"},
        {"CO", "Consolidated Omnibus Budget Reconciliation Act (COBRA)"},
        {"CP", "Medicare Conditionally Primary"},
        {"D", "Disability"},
        {"DB", "Disability Benefits"},
        {"EP", "Exclusive Provider Organization"},
        {"FF", "Family or Friends"},
        {"GP", "Group Policy"},
        {"HM", "Health Maintenance Organization (HMO)"},
        {"HN", "Health Maintenance Organization (HMO) - Medicare Risk"},
        {"HS", "Special Low Income Medicare Beneficiary"},
        {"IN", "Indemnity"},
        {"IP", "Individual Policy"},
        {"LC", "Long Term Care"},
        {"LD", "Long Term Policy"},
        {"LI", "Life Insurance"},
        {"LT", "Litigation"},
        {"MA", "Medicare Part A"},
        {"MB", "Medicare Part B"},
        {"MC", "Medicaid"},
        {"MH", "Medigap Part A"},
        {"MI", "Medigap Part B"},
        {"MP", "Medicare Primary"},
        {"OT", "Other"},
        {"PE", "Property Insurance - Personal"},
        {"PL", "Personal"},
        {"PP", "Personal Payment (Cash - No Insurance)"},
        {"PR", "Preferred Provider Organization (PPO)"},
        {"PS", "Point of Service (POS)"},
        {"QM", "Qualified Medicare Beneficiary"},
        {"RP", "Property Insurance - Real"},
        {"SP", "Supplemental Policy"},
        {"TF", "Tax Equity Fiscal Responsibility Act (TEFRA)"},
        {"WC", "Workers Compensation"},
        {"WU", "Wrap Up Policy"}
    };

    private static readonly List<EB06> Eb06 =
    [
        new("6", "Hour"),
        new("7", "Day"),
        new("13", "24 Hours"),
        new("21", "Years", BenefitTimePeriod.Years),
        new("22", "Service Year", BenefitTimePeriod.ServiceYear),
        new("23", "Calendar Year", BenefitTimePeriod.CalendarYear),
        new("24", "Year to Date"),
        new("25", "Contract"),
        new("26", "Episode"),
        new("27", "Visit"),
        new("28", "Outlier"),
        new("29", "Remaining"),
        new("30", "Exceeded"),
        new("31", "Not Exceeded"),
        new("32", "Lifetime", BenefitTimePeriod.Lifetime),
        new("33", "Lifetime Remaining"),
        new("34", "Month"),
        new("35", "Week"),
        new("36", "Admisson")
    ];

    private static readonly List<EB09> Eb09 =
    [
        new("99", "Quantity Used"),
        new("CA", "Covered - Actual"),
        new("CE", "Covered - Estimated"),
        new("DB", "Deductible Blood Units"),
        new("DY", "Days"),
        new("HS", "Hours"),
        new("LA", "Life-time Reserve - Actual"),
        new("LE", "Life-time Reserve - Estimated"),
        new("MN", "Month", BenefitQuantity.Months),
        new("P6", "Number of Services or Procedures", BenefitQuantity.NumberOfServices),
        new("QA", "Quantity Approved"),
        new("S7", "Age, High Value", BenefitQuantity.AgeLimit),
        new("S8", "Age, Low Value"),
        new("VS", "Visits", BenefitQuantity.Visits),
        new("YY", "Years", BenefitQuantity.Years)
    ];

    public readonly X12Segment Segment;
    public Benefit Benefitt;
    public readonly List<X12Segment> SupplementalSegments;

    public EB271(X12Segment segment, bool isInNetwork, bool isCoinsuranceInverted, X12Segment segHsd = null)
    {
        if (segment is null)
        {
            return;
        }

        Segment = segment;
        SupplementalSegments = [];

        var eb01Val = Eb01.Find(x => Segment.Get(1) == x.Code);
        var eb02Val = Eb02.Find(x => Segment.Get(2) == x.Code);
        var eb03Val = Eb03.Find(x => Segment.Get(3) == x.Code);
        var eb06Val = Eb06.Find(x => Segment.Get(6) == x.Code);
        var eb09Val = Eb09.Find(x => Segment.Get(9) == x.Code);

        ProcedureCode procedureCode = null;
        if (ProcedureCodes.IsValidCode(Segment.Get(13, 2)))
        {
            procedureCode = ProcedureCodes.GetProcCode(Segment.Get(13, 2));
        }

        if (!eb01Val.IsSupported || eb02Val is {IsSupported: false} || eb03Val is {IsSupported: false} || eb06Val is {IsSupported: false} || eb09Val is {IsSupported: false})
        {
            Benefitt = null;
            return;
        }

        switch (eb01Val.BenefitType)
        {
            case InsBenefitType.ActiveCoverage when Segment.Get(3) == "30":
            case InsBenefitType.ActiveCoverage when procedureCode is not null:
                Benefitt = null;
                return;
        }

        if (Segment.Get(8) != "")
        {
            if (procedureCode is null)
            {
                if (eb03Val is null || eb03Val.ServiceType == EbenefitCategory.None || eb03Val.ServiceType == EbenefitCategory.General)
                {
                    Benefitt = null;

                    return;
                }
            }
        }

        switch (eb01Val.BenefitType)
        {
            case InsBenefitType.CoPayment or InsBenefitType.CoInsurance when Segment.Get(7) != "":
            case InsBenefitType.Limitations when segHsd is null && Segment.Get(7) == "" && Segment.Get(7) == "":
                Benefitt = null;
                return;
        }

        switch (isInNetwork)
        {
            case true when Segment.Get(12) == "N" || Segment.Get(12) == "U":
            case false when Segment.Get(12) == "Y":
                Benefitt = null;
                return;
        }

        if (Segment.Get(10) != "" && eb09Val is null)
        {
            Benefitt = null;
            return;
        }

        if (eb09Val is not null && Segment.Get(10) == "")
        {
            Benefitt = null;
            return;
        }

        if (SIn.Double(Segment.Get(10)) > byte.MaxValue)
        {
            Benefitt = null;
            return;
        }

        if (segHsd is not null && SIn.Double(segHsd.Get(2)) > byte.MaxValue)
        {
            Benefitt = null;
            return;
        }

        Benefitt = new Benefit
        {
            BenefitType = eb01Val.BenefitType
        };

        if (eb02Val is not null)
        {
            Benefitt.CoverageLevel = eb02Val.CoverageLevel;
        }

        if (eb03Val is not null)
        {
            Benefitt.CovCatNum = CovCats.GetForEbenCat(eb03Val.ServiceType).CovCatNum;
        }

        if (eb06Val is not null)
        {
            Benefitt.TimePeriod = eb06Val.TimePeriod;
        }

        if (Segment.Get(7) != "")
        {
            Benefitt.MonetaryAmt = SIn.Double(Segment.Get(7));
        }

        if (Segment.Get(8) != "")
        {
            if (isCoinsuranceInverted && Benefitt.BenefitType == InsBenefitType.CoInsurance)
            {
                Benefitt.Percent = (int) (SIn.Double(Segment.Get(8)) * 100);
            }
            else
            {
                Benefitt.Percent = 100 - (int) (SIn.Double(Segment.Get(8)) * 100);
            }

            Benefitt.CoverageLevel = BenefitCoverageLevel.None;
        }

        if (eb09Val is not null)
        {
            Benefitt.QuantityQualifier = eb09Val.QuantityQualifier;
        }

        if (Segment.Get(10) != "")
        {
            Benefitt.Quantity = (byte) SIn.Double(Segment.Get(10));
        }

        if (procedureCode is not null)
        {
            Benefitt.CodeNum = procedureCode.CodeNum;
        }

        if (Benefitt.BenefitType != InsBenefitType.Limitations || procedureCode is null || segHsd is null)
        {
            return;
        }

        if (segHsd.Elements.Length < 6 || segHsd.Elements[2] == "" || segHsd.Elements[5] == "")
        {
            Benefitt = null;
            return;
        }

        Benefitt.Quantity = SIn.Byte(segHsd.Elements[2]);
        Benefitt.TimePeriod = Eb06.FirstOrDefault(x => x.Code == segHsd.Elements[5]).TimePeriod;
    }

    public string GetDescription(bool isMessageMode, bool isCoinsurancePatPays)
    {
        var containsAddress = false;
        var containsDate = false;

        foreach (var segment in SupplementalSegments)
        {
            switch (segment.SegmentID)
            {
                case "LS":
                    containsAddress = true;
                    break;

                case "DTP":
                    containsDate = true;
                    break;
            }
        }

        if (containsAddress)
        {
            return GetDescriptionForAddress();
        }

        if (containsDate)
        {
            return GetDescriptionForDate();
        }

        if (Segment.Get(1) == "1" && Segment.Get(13) != "")
        {
            return GetDescriptionForCodeCovered();
        }

        if (Segment.Get(1) == "A" && Segment.Get(8) != "" && Segment.Get(13) != "")
        {
            return GetDescriptionForPercentCode();
        }

        var result = "";

        var description = GetDescript(1, isMessageMode);
        if (description != "")
        {
            result += description;
        }

        if (Segment.Get(3) != "30")
        {
            description = GetDescript(3);
            if (description != "")
            {
                result += ", " + description;
            }
        }

        description = GetDescript(4);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(5);
        if (isMessageMode && description != "")
        {
            if (result != "")
            {
                result += ", ";
            }

            result += description;
        }

        description = GetDescript(6);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(7);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(8, isMessageMode, isCoinsurancePatPays);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(9);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(10);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(11);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(12);
        if (description != "")
        {
            result += ", " + description;
        }

        description = GetDescript(13);
        if (description != "")
        {
            result += ", " + description;
        }

        foreach (var segment in SupplementalSegments)
        {
            if (!isMessageMode)
            {
                continue;
            }

            if (segment.SegmentID == "MSG")
            {
                result += ", " + segment.Get(1);
            }
        }

        return result;
    }

    private string GetDescriptionForAddress()
    {
        var result = GetDescript(1) + "\r\n";

        foreach (var segment in SupplementalSegments)
        {
            if (segment.SegmentID == "NM1")
            {
                result += segment.Get(3) + " " + segment.Get(4) + "\r\n";
            }

            if (segment.SegmentID == "N3")
            {
                result += segment.Get(1) + " " + segment.Get(2) + "\r\n";
            }

            if (segment.SegmentID == "N4")
            {
                result += segment.Get(1) + ", " + segment.Get(2) + segment.Get(3);
            }
        }

        return result;
    }

    private string GetDescriptionForDate()
    {
        var result = "";

        foreach (var segment in SupplementalSegments)
        {
            if (segment.SegmentID == "DTP")
            {
                result += DTP271.GetQualifierDescription(segment.Get(1)) + ": " +
                          DTP271.GetDate(segment.Get(2), segment.Get(3));
            }
        }

        return result;
    }

    private string GetDescriptionForCodeCovered()
    {
        var description = GetDescript(13);
        if (description == "")
        {
            return "";
        }

        return "Covered: " + description;
    }

    private string GetDescriptionForPercentCode()
    {
        var result = GetDescript(8);

        var description = GetDescript(12);
        if (description != "")
        {
            result += ", " + description;
        }

        result += ", " + GetDescript(13);

        return result;
    }

    public string GetDescript(int elementPos, bool isMessageMode = false, bool isCoinsurancePatPays = true)
    {
        var elementCode = Segment.Get(elementPos);
        if (elementCode == "")
        {
            return "";
        }

        switch (elementPos)
        {
            case 1:
                var eb01Val = Eb01.Find(x => Segment.Get(1) == x.Code);
                if (eb01Val is null)
                {
                    return "";
                }

                if (eb01Val.Code == "D" && isMessageMode)
                {
                    return "";
                }

                return eb01Val.Description;

            case 2:
                var eb02Val = Eb02.Find(x => Segment.Get(2) == x.Code);
                return eb02Val is null ? "" : eb02Val.Description;

            case 3:
                var eb03Val = Eb03.Find(x => Segment.Get(3) == x.Code);
                return eb03Val is null ? "" : eb03Val.Description;

            case 4:
                return !Eb04.TryGetValue(elementCode, out var descript) ? "" : descript;

            case 5:
                return Segment.Get(5);

            case 6:
                var eb06Val = Eb06.Find(x => Segment.Get(6) == x.Code);
                return eb06Val is null ? "" : eb06Val.Description;

            case 7:
                return SIn.Double(elementCode).ToString("c");

            case 8:
                if (isMessageMode)
                {
                    return SIn.Double(elementCode) * 100 + "%";
                }

                var leadingStr = "Patient pays ";
                if (!isCoinsurancePatPays)
                {
                    leadingStr = "Insurance pays ";
                }

                return leadingStr + SIn.Double(elementCode) * 100 + "%";

            case 9:
                var eb09Val = Eb09.Find(x => Segment.Get(9) == x.Code);
                return eb09Val is null ? "" : eb09Val.Description;

            case 10:
                return elementCode;

            case 11:
                return "Authorization Required-" + elementCode;

            case 12:
                return elementCode switch
                {
                    "Y" => "In network",
                    "N" => "Out of network",
                    _ => "Unknown if in network"
                };

            case 13:
                var code = Segment.Get(13, 2);
                if (code == "")
                {
                    return "";
                }

                var procedureCode = ProcedureCodes.GetProcCode(code);
                return code + " - " + procedureCode.AbbrDesc;

            default:
                return "";
        }
    }

    public DateTime GetInsHistDate(PrefName prefName)
    {
        if (Segment.Get(1) != "F")
        {
            return DateTime.MinValue;
        }

        var x12SegmentDtp = SupplementalSegments.FirstOrDefault(x => x.SegmentID == "DTP" && x.Elements[1] == "304");
        var x12SegmentMsg = SupplementalSegments.FirstOrDefault(x => x.SegmentID == "MSG");
        if (x12SegmentDtp is null || x12SegmentMsg is null)
        {
            return DateTime.MinValue;
        }

        if (prefName == PrefName.NotApplicable)
        {
            return DateTime.MinValue;
        }

        DateTime result;
        try
        {
            result = DateTime.ParseExact(x12SegmentDtp.Get(3), "yyyymmdd", CultureInfo.InvariantCulture);
        }
        catch
        {
            return DateTime.MinValue;
        }

        if (result.Year < 1880 || result > DateTime.Today)
        {
            result = DateTime.MinValue;
        }

        return result;
    }

    public PrefName GetPrefNameInsHistDate()
    {
        var x12SegmentMsg = SupplementalSegments.FirstOrDefault(x => x.SegmentID == "MSG");

        return x12SegmentMsg is null ? PrefName.NotApplicable : GetPrefNameMatchedMsg(x12SegmentMsg.Get(1));
    }

    public string ToString(bool hasFreeFormText = true)
    {
        var result = Segment + "~";

        if (!hasFreeFormText)
        {
            var elements = result.Split('*');
            if (elements.Length >= 6)
            {
                elements[5] = "";
            }

            result = string.Join("*", elements);
        }

        foreach (var segment in SupplementalSegments)
        {
            result += "\r\n" + segment + "~";
        }

        return result;
    }

    private static PrefName GetPrefNameMatchedMsg(string value)
    {
        value = value.ToUpper().Replace("BENEFITCLASS=", "");

        return value switch
        {
            "BITEWING X-RAYS" => PrefName.InsHistBWCodes,
            "EXAMS" => PrefName.InsHistExamCodes,
            "FULL MOUTH/PANOREX" => PrefName.InsHistPanoCodes,
            "PROPHYLAXIS" => PrefName.InsHistProphyCodes,
            _ => PrefName.NotApplicable
        };
    }

    public static void SetInsuranceHistoryDates(List<EB271> eb271s, long patNum, InsSub insSub)
    {
        if (eb271s is null || eb271s.Count == 0)
        {
            return;
        }

        var proceduresEoAndC = Procedures.GetProcsByStatusForPat(patNum, ProcStat.EO, ProcStat.C);
        var procNums = proceduresEoAndC.Select(x => x.ProcNum).ToList();
        var claimProcsForInsHistProcs = ClaimProcs.GetForProcs(procNums).FindAll(y => y.InsSubNum == insSub.InsSubNum && y.Status.In(ClaimProcStatus.InsHist, ClaimProcStatus.Received));
        var patient = Patients.GetLim(patNum);
        
        foreach (var eb271 in eb271s)
        {
            var prefName = eb271.GetPrefNameInsHistDate();
            
            var dateInsHist = eb271.GetInsHistDate(prefName);
            if (dateInsHist == DateTime.MinValue)
            {
                continue;
            }

            var codeNums = ProcedureCodes.GetCodeNumsForInsHistPref(prefName);
            var procedure = Procedures.GetMostRecentInsHistProc(proceduresEoAndC, codeNums, prefName);
            var claimProcsForProc = new List<ClaimProc>();
            if (procedure is not null)
            {
                claimProcsForProc = claimProcsForInsHistProcs.FindAll(x => x.ProcNum == procedure.ProcNum);
            }

            Procedures.InsertOrUpdateInsHistProcedure(patient, prefName, dateInsHist, insSub.PlanNum, insSub.InsSubNum, procedure, claimProcsForProc);
        }
    }

    public class EB01
    {
        public EB01(string code, string description, InsBenefitType benefitType)
        {
            Code = code;
            Description = description;
            BenefitType = benefitType;
            IsSupported = true;
        }

        public EB01(string code, string description)
        {
            Code = code;
            Description = description;
            BenefitType = InsBenefitType.ActiveCoverage;
            IsSupported = false;
        }

        public InsBenefitType BenefitType { get; }
        public string Code { get; }
        public string Description { get; }
        public bool IsSupported { get; }
    }

    public class EB02
    {
        public EB02(string code, string description, BenefitCoverageLevel coverageLevel)
        {
            Code = code;
            Description = description;
            CoverageLevel = coverageLevel;
            IsSupported = true;
        }

        public EB02(string code, string description)
        {
            Code = code;
            Description = description;
            CoverageLevel = BenefitCoverageLevel.Individual;
            IsSupported = false;
        }

        public BenefitCoverageLevel CoverageLevel { get; }
        public string Code { get; }
        public string Description { get; }
        public bool IsSupported { get; }
    }

    public class EB03
    {
        public EB03(string code, string description, EbenefitCategory serviceType)
        {
            Code = code;
            Description = description;
            ServiceType = serviceType;
            IsSupported = true;
        }

        public EB03(string code, string description)
        {
            Code = code;
            Description = description;
            ServiceType = EbenefitCategory.None;
            IsSupported = false;
        }

        public EbenefitCategory ServiceType { get; }
        public string Code { get; }
        public string Description { get; }
        public bool IsSupported { get; }
    }

    public class EB06
    {
        public EB06(string code, string description, BenefitTimePeriod timePeriod)
        {
            Code = code;
            Description = description;
            TimePeriod = timePeriod;
            IsSupported = true;
        }

        public EB06(string code, string description)
        {
            Code = code;
            Description = description;
            TimePeriod = BenefitTimePeriod.None;
            IsSupported = false;
        }

        public BenefitTimePeriod TimePeriod { get; }
        public string Code { get; }
        public string Description { get; }
        public bool IsSupported { get; }
    }

    public class EB09
    {
        public EB09(string code, string description, BenefitQuantity quantityQualifier)
        {
            Code = code;
            Description = description;
            QuantityQualifier = quantityQualifier;
            IsSupported = true;
        }

        public EB09(string code, string description)
        {
            Code = code;
            Description = description;
            QuantityQualifier = BenefitQuantity.None;
            IsSupported = false;
        }

        public BenefitQuantity QuantityQualifier { get; }

        public string Code { get; }
        public string Description { get; }
        public bool IsSupported { get; }
    }
}