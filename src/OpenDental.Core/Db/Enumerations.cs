using System;
using System.ComponentModel;

namespace OpenDentBusiness;

public enum YN
{
    Unknown,
    Yes,
    No
}

public enum Relat
{
    Self,
    Spouse,
    Child,
    Employee,
    HandicapDep,
    SignifOther,
    InjuredPlaintiff,
    LifePartner,
    Dependent
}

public enum Month
{
    Jan = 1,
    Feb,
    Mar,
    Apr,
    May,
    Jun,
    Jul,
    Aug,
    Sep,
    Oct,
    Nov,
    Dec
}

public enum ProcStat
{
    ///<summary>1- Treatment Plan.</summary>
    TP = 1,

    ///<summary>2- Complete.</summary>
    C,

    ///<summary>3- Existing Current Provider.</summary>
    EC,

    ///<summary>4- Existing Other Provider.</summary>
    EO,

    ///<summary>5- Referred Out.</summary>
    R,

    ///<summary>6- Deleted.</summary>
    D,

    ///<summary>7- Condition.</summary>
    Cn,

    ///<summary>8- Treatment Plan inactive.</summary>
    TPi
}

///<summary>The pseudo statuses inside this extended enum must always be mutually exclusive of the values inside the ProcStat enum.
///These statuses are transalted via class type "enumProcStat" (ex Lan.g("enumProcStat",ProcStatExt.InProcess))</summary>
public class ProcStatExt
{
    ///<summary>I - Stands for "Invalid".</summary>
    public const string Invalid = "I";

    ///<summary>C/P - Stands for "Complete (In Process)".</summary>
    public const string InProcess = "C/P";
}

public enum TreatmentArea
{
    None,
    Surf,
    Tooth,
    Mouth,
    Quad,
    Sextant,
    Arch,
    ToothRange
}

public enum InvalidType
{
    AllLocal = 2,
    Task = 3,
    ProcCodes = 4,
    Prefs = 5,
    Views = 6,
    AutoCodes = 7,
    Carriers = 8,
    ClearHouses = 9,
    Computers = 10,
    InsCats = 11,
    Employees = 12,
    Defs = 14,
    Email = 15,
    QuickPaste = 18,
    Security = 19,
    Programs = 20,
    ToolButsAndMounts = 21,
    Providers = 22,
    ClaimForms = 23,
    ZipCodes = 24,
    LetterMerge = 25,
    Operatories = 27,
    TaskPopup = 28,
    Sites = 29,
    Pharmacies = 30,
    Sheets = 31,
    RecallTypes = 32,
    FeeScheds = 33,
    DisplayFields = 36,
    PatFields = 37,
    AccountingAutoPays = 38,
    ProcButtons = 39,

    ///<summary>40.  Includes ICD9s.</summary>
    Diseases = 40,

    ///<summary>41. Includes LanguagePats</summary>
    Languages = 41,

    ///<summary>42</summary>
    AutoNotes = 42,

    ///<summary>43</summary>
    ElectIDs = 43,

    ///<summary>44</summary>
    Employers = 44,
    ProviderIdents = 45,
    ShutDownNow = 46,
    InsFilingCodes = 47,
    Automation = 49,
    TimeCardRules = 51,
    HL7Defs = 53,
    DictCustoms = 54,
    Sops = 56,
    AppointmentTypes = 58,
    SmsTextMsgReceivedUnreadCount = 60,
   
    StateAbbrs = 64,
    RequiredFields = 65,
    Ebills = 66,
    UserClinics = 67,
    Appointment = 68,
    OrthoChartTabs = 69,

    ///<summary>72. THIS IS NOT CACHED. But is used to make server run the alert logic in OpenDentalService.</summary>
    AlertItems = 72,

    ///<summary>74. Used to refresh the active kiosk grid in FormTerminalManager and loaded patient with list of forms in FormTerminal.</summary>
    Kiosk = 74,

    ///<summary>75</summary>
    ClinicPrefs = 75,

    ///<summary>76. Not addresses or templates, but inbox and sent messages.</summary>
    EmailMessages = 76,
    
    ///<summary>79.</summary>
    AlertCategories = 79,

    ///<summary>80.</summary>
    AlertCategoryLinks = 80,

    ///<summary>81. Used in updating menu item in report menu.</summary>
    UnfinalizedPayMenuUpdate = 81,
    
    ///<summary>83.</summary>
    DisplayReports = 83,

    ///<summary>84.</summary>
    UserQueries = 84,

    ///<summary>85. Schedules are not cached, but alerts other workstations if the schedules were changed</summary>
    Schedules = 85,

    ///<summary>88.</summary>
    SmsPhones = 88,

    ///<summary>90. Used for tracking refreshes on tabs 'for [User]', 'New for [User]', 'Main', 'Reminders'.</summary>
    TaskList = 90,

    ///<summary>91. Used for tracking refreshes on tab 'Open Tasks'.</summary>
    TaskAuthor = 91,

    ///<summary>92. Used for tracking refreshes on tab 'Patient Tasks'.</summary>
    TaskPatient = 92,

    ///<summary>93. Used for refreshing the Referral cache.</summary>
    Referral = 93,

    ///<summary>94. Used for refreshing "In Process" pseudo procedure statuses.</summary>
    ProcMultiVisits = 94,

    ///<summary>95. Used for refreshing the ProviderClinicLink cache.</summary>
    ProviderClinicLink = 95,

    ///<summary>97. Used for refreshing the TP module for a specific patient. PatNum used in FKey.</summary>
    TPModule = 97,

    ///<summary>98. Used for closing Cloud sessions. ActiveInstanceNum is the Fkey.</summary>
    ActiveInstance = 98,

    ///<summary>99. Used internally by OD HQ.</summary>
    PhoneEmpDefaults = 99,

    ///<summary>100. Used for refreshing user preferences.</summary>
    UserOdPrefs = 100,

    ///<summary>102. Used to refresh the Account Module for a specific patient. PatNum used in FKey.</summary>
    AccModule = 102,
    
    ///<summary>104. Used to refresh Perio Chart. patient.PatNum used in FKey.</summary>
    PerioExams = 104,

    ///<summary>105. </summary>
    EmailInboxRetrieve = 105,
    
    ///<summary>110. Group of codes used with frequency limitations.</summary>
    CodeGroups = 110,

    ///<summary>111. Used to refresh the billing list when it's open and a patient's account was adjusted. Works immediately on the current computer and at the signal interval of about 10 seconds on other computers.</summary>
    BillingList = 111,

    ///<summary>112. Indicates that database connection settings have changed and the cached connections should be reinitialized.</summary>
    ConnectionStoreClear = 112,

    ///<summary>114. Instructs a specific computer to print the RemotePrintRequest that is attached as json. This signal is generated by eConnector in response to a print request from ODTouch or possibly others.</summary>
    Print = 114
}

///<summary>Appointment status.</summary>
public enum ApptStatus
{
    ///<summary>0- No appointment should ever have this status.</summary>
    None,

    ///<summary>1- Shows as a regularly scheduled appointment.</summary>
    Scheduled,

    ///<summary>2- Shows greyed out.</summary>
    Complete,

    ///<summary>3- Only shows on unscheduled list.</summary>
    UnschedList,

    ///<summary>4- Deprecated in 17.4.1. Use Appointment.Priority instead. </summary>
    ASAP,

    ///<summary>5- Shows with a big X on it.</summary>
    Broken,

    ///<summary>6- Planned appointment.  Only shows in Chart module. User not allowed to change this status, and it does not display as one of the options.</summary>
    Planned,

    ///<summary>7- Patient "post-it" note on the schedule. Shows light yellow. Shows on day scheduled just like appt, as well as in prog notes, etc.</summary>
    PtNote,

    ///<summary>8- Patient "post-it" note completed</summary>
    PtNoteCompleted
}

public enum PatientStatus
{
    Patient,

    [Description("Non-patient")]
    NonPatient,

    Inactive,
    Archived,
    Deleted,
    Deceased,
    Prospective
}

///<summary>Known as administrativeGender (HL7 OID of 2.16.840.1.113883.5.1) Male=M, Female=F, Unknown=Undifferentiated=UN.</summary>
public enum PatientGender
{
    Male,
    Female,
    Unknown,
    Other
}

public enum PatientPosition
{
    Single,
    Married,
    Child,
    Widowed,
    Divorced
}

public enum ScheduleType
{
    Practice,
    Provider,
    Blockout,
    Employee,
    WebSchedASAP,
}

public enum BlockoutAction
{
    Cut,
    Copy,
    Paste,
    Delete,
    Create,
    Edit,
    Clear
}

public enum ProcCodeListSort
{
    Category,
    ProcCode
}

public enum OtherResult
{
    Cancel,
    CreateNew,
    GoTo,
    CopyToPinBoard,
    NewToPinBoard,
    PinboardAndSearch
}

public enum SchedStatus
{
    Open,
    Closed,
    Holiday
}

public enum AutoCondition
{
    Anterior,
    Posterior,
    Premolar,
    Molar,
    One_Surf,
    Two_Surf,
    Three_Surf,
    Four_Surf,
    Five_Surf,
    First,
    EachAdditional,
    Maxillary,
    Mandibular,
    Primary,
    Permanent,
    Pontic,
    Retainer,
    AgeOver18
}

public enum SubstitutionCondition
{
    Always,
    Molar,
    SecondMolar,
    Never,
    Posterior
}

public enum ClaimProcStatus
{
    ///<summary>0: For claims that have been created or sent, but have not been received.</summary>
    NotReceived,

    ///<summary>1: For claims that have been received.</summary>
    Received,

    ///<summary>2: For preauthorizations.</summary>
    Preauth,

    ///<summary>3: The only place that this status is used is to make adjustments to benefits from the coverage window.  It is never attached to a claim.</summary>
    Adjustment,

    ///<summary>4:This differs from Received only slightly.  It's for additional payments on procedures already received.  Most fields are blank.</summary>
    Supplemental,

    ///<summary>5: CapClaim is used when you want to send a claim to a capitation insurance company.  These are similar to Supplemental in that there will always be a duplicate claimproc for a procedure. The first claimproc tracks the copay and writeoff, has a status of CapComplete, and is never attached to a claim. The second claimproc has status of CapClaim.</summary>
    CapClaim,

    ///<summary>6: Estimates have replaced the fields that were in the procedure table.  Once a procedure is complete, the claimprocstatus will still be Estimate.  An Estimate can be attached to a claim and status gets changed to NotReceived.</summary>
    Estimate,

    ///<summary>7: For capitation procedures that are complete.  This replaces the old procedurelog.CapCoPay field. This stores the copay and writeoff amounts.  The copay is only there for reference, while it is the writeoff that actually affects the balance. Never attached to a claim. If procedure is TP, then status will be CapEstimate.  Only set to CapComplete if procedure is Complete.</summary>
    CapComplete,

    ///<summary>8: For capitation procedures that are still estimates rather than complete.  When procedure is completed, this can be changed to CapComplete, but never to anything else.</summary>
    CapEstimate,

    ///<summary>9: For InsHist procedures.</summary>
    InsHist
}

public enum TimeClockStatus
{
    [Description("Home")]
    Home,

    [Description("Lunch")]
    Lunch,

    [Description("Break")]
    Break
}

public enum PatientRaceOld
{
    Unknown,
    Multiracial,
    HispanicLatino,
    AfricanAmerican,
    White,
    HawaiiOrPacIsland,
    AmericanIndian,
    Asian,
    Other,
    Aboriginal,
    BlackHispanic
}

public enum PatientGrade
{
    Unknown,
    First,
    Second,
    Third,
    Fourth,
    Fifth,
    Sixth,
    Seventh,
    Eighth,
    Ninth,
    Tenth,
    Eleventh,
    Twelfth,
    PrenatalWIC,
    PreK,
    Kindergarten,
    Other
}

public enum TreatmentUrgency
{
    Unknown,
    NoProblems,
    NeedsCare,
    Urgent
}

///<summary>Each item in the enum should uniquely describe the location of a single TextBoxOD.  No sharing.  In many cases, this is tied to QuickPasteCat to determine which category to default to when opening.</summary>
public enum EnumQuickPasteType
{
    ///<summary>0 - If None is used for a TextRich, then QuickPasteNotes will be disabled.</summary>
    None = 0,
    
    Procedure = 1,
    Appointment = 2,
    CommLog = 3,
    Adjustment = 4,
    Claim = 5,
    Email = 6,
    InsPlan = 7,
    Letter = 8,
    MedicalSummary = 9,
    ServiceNotes = 10,
    MedicalHistory = 11,
    MedicationEdit = 12,
    MedicationPat = 13,
    PatAddressNote = 14,
    Payment = 15,
    PayPlan = 16,
    Query = 17,
    Referral = 18,
    Rx = 19,
    FinancialNotes = 20,
    ChartTreatment = 21,
    MedicalUrgent = 22,
    Statement = 23,
    Recall = 24,
    Popup = 25,
    TxtMsg = 26,
    Task = 27,
    Schedule = 28,
    TreatPlan = 29,
    ClaimCustomTrack = 30,
    AutoNotePrompt = 31,
    Lab = 34,
    Etrans834Import = 36,
    ProgramLink = 39,

    ///<summary>43-Just autonotes, not quickpaste.</summary>
    Sheets = 43
}

///<summary>For every type of electronic claim format that Open Dental can create, there will be an item in this enumeration.  All e-claim formats are hard coded due to complexity.</summary>
public enum ElectronicClaimFormat
{
    ///<summary>0-Not in database, but used in various places in program.</summary>
    None,

    ///<summary>1-The American standard through 12/31/11.</summary>
    x837D_4010,

    ///<summary>2-Proprietary format for Renaissance.</summary>
    Renaissance,

    ///<summary>3-CDAnet format version 4.</summary>
    Canadian,

    ///<summary>4-CSV file adaptable for use in Netherlands.</summary>
    Dutch,

    ///<summary>5-The American standard starting on 1/1/12.</summary>
    x837D_5010_dental,

    ///<summary>6-Either professional or medical.  The distiction is stored at the claim level.</summary>
    x837_5010_med_inst,

    ///<summary>7-A specific Canadian carrier located in Quebec which has their own format.</summary>
    Ramq,
}

public enum ProviderSupplementalID
{
    BlueCross,
    BlueShield,
    SiteNumber,
    CommercialNumber
}

///<summary>Each clearinghouse can have a hard-coded comm bridge which handles all the communications of transfering the claim files to the clearinghouse/carrier.  Does not just include X12, but can include any format at all.</summary>
public enum EclaimsCommBridge
{
    ///<summary>0-No comm bridge will be activated. The claim files will be created to the specified path, but they will not be uploaded.</summary>
    None,

    WebMD,
    BCBSGA,
    Renaissance,
    ClaimConnect,
    RECS,
    Inmediata,
    AOS,
    PostnTrack,

    ///<summary>9 Canadian clearinghouse.</summary>
    ITRANS,

    Tesia,
    MercuryDE,
    ClaimX,
    DentiCal,
    EmdeonMedical,

    ///<summary>15 Canadian clearinghouse.</summary>
    Claimstream,

    ///<summary>16 UK clearinghouse.</summary>
    NHS,
    
    EDS,
    Ramq,
    EdsMedical,
    Lantek,

    ///<summary>21 Canadian clearinghouse.  Similar to ITRANS except supports certificate and carrier list web fetching.</summary>
    ITRANS2,
    VyneDental,
}

///<summary>Used in the benefit table.  Corresponds to X12 EB01.</summary>
public enum InsBenefitType
{
    ///<summary>0- Informational only. Not usually used.  Would only be used if you are just indicating that the patient is covered, but without any specifics.</summary>
    ActiveCoverage,

    ///<summary>1- Used for percentages to indicate portion that insurance will cover.  When interpreting electronic benefit information, this is the opposite percentage, the percentage that the patient will pay after deductible.</summary>
    CoInsurance,

    ///<summary>2- The deductible amount.  Might be two entries if, for instance, deductible is waived on preventive.</summary>
    Deductible,

    ///<summary>3- Informational only. A dollar amount.</summary>
    CoPayment,

    ///<summary>4- Services that are simply not covered at all.</summary>
    Exclusions,

    ///<summary>5- Covers a variety of limitations, including Max, frequency, fee reductions, etc.</summary>
    Limitations,

    ///<summary>6- Sets a period of time after the effective date where a benefit will not be used.</summary>
    WaitingPeriod
}

///<summary>Used in the benefit table.  Corresponds to X12 EB06.</summary>
public enum BenefitTimePeriod
{
    ///<summary>0- A timeperiod is frequenly not needed.  For example, percentages.</summary>
    None,

    ///<summary>1- The renewal month is not Jan.  In this case, we need to know the effective date so that we know which month the benefits start over in.</summary>
    ServiceYear,

    ///<summary>2- Renewal month is Jan.</summary>
    CalendarYear,

    ///<summary>3- Usually used for ortho max.</summary>
    Lifetime,

    ///<summary>4- Wouldn't be used alone.  Years would again be specified in the quantity field along with a number.</summary>
    Years,

    ///<summary>5- # in last 12 months.  Does not care about when benefit year begins. Looks at previous 12 months.</summary>
    NumberInLast12Months,
}

///<summary>Used in the benefit table in conjunction with an integer quantity.</summary>
public enum BenefitQuantity
{
    ///<summary>0- This is used a lot. Most benefits do not need any sort of quantity.</summary>
    None,

    ///<summary>1- For example, two exams per year</summary>
    NumberOfServices,

    ///<summary>2- For example, 18 when fluoride only covered to 18 y.o.</summary>
    AgeLimit,

    ///<summary>3- For example, copay per 1 visit.</summary>
    Visits,

    ///<summary>4- For example, pano every 5 years.</summary>
    Years,

    ///<summary>5- For example, BWs every 6 months.</summary>
    Months
}

///<summary>Used in the benefit table.</summary>
public enum BenefitCoverageLevel
{
    ///<summary>0- Since this is a situational X12 field, we can also have none.  Typical for percentages and copayments.</summary>
    None,

    ///<summary>1- The default for deductibles and maximums.</summary>
    Individual,

    ///<summary>2- For example, family deductible or family maximum.</summary>
    Family
}

///<summary>The X12 benefit categories.  Used to link the user-defined CovCats to the corresponding X12 category.</summary>
public enum EbenefitCategory
{
    ///<summary>0- Default.  Applies to all codes.</summary>
    None,

    ///<summary>1- X12: 30 and 35. All ADA codes except ortho.  D0000-D7999 and D9000-D9999</summary>
    General,

    ///<summary>2- X12: 23. ADA D0000-D0999.  This includes DiagnosticXray.</summary>
    Diagnostic,

    ///<summary>3- X12: 24. ADA D4000</summary>
    Periodontics,

    ///<summary>4- X12: 25. ADA D2000-D2699, and D2800-D2999.</summary>
    Restorative,

    ///<summary>5- X12: 26. ADA D3000</summary>
    Endodontics,

    ///<summary>6- X12: 27. ADA D5900-D5999</summary>
    MaxillofacialProsth,

    ///<summary>7- X12: 36. Exclusive subcategory of restorative.  D2700-D2799</summary>
    Crowns,

    ///<summary>8- X12: 37. ADA range?</summary>
    Accident,

    ///<summary>9- X12: 38. ADA D8000-D8999</summary>
    Orthodontics,

    ///<summary>10- X12: 39. ADA D5000-D5899 (removable), and D6200-D6899 (fixed)</summary>
    Prosthodontics,

    ///<summary>11- X12: 40. ADA D7000</summary>
    OralSurgery,

    ///<summary>12- X12: 41. ADA D1000</summary>
    RoutinePreventive,

    ///<summary>13- X12: 4. ADA D0200-D0399.  So this is like an optional category which is otherwise considered to be diagnosic.</summary>
    DiagnosticXRay,

    ///<summary>14- X12: 28. ADA D9000-D9999</summary>
    Adjunctive
}

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Income,
    Expense
}

public enum ToothPaintingType
{
    None,
    Extraction,
    Implant,
    RCT,
    PostBU,
    FillingDark,
    FillingLight,
    CrownDark,
    CrownLight,
    
    BridgeDark,
    BridgeLight,
    DentureDark,
    DentureLight,
    Sealant,
    Veneer,
    Text,
    RetainedRoot,
    SpaceMaintainer
}

public enum TerminalStatusEnum
{
    Standby,
    PatientInfo,
    Medical,

    /// <summary>
    /// Only the patient info tab will be visible.
    /// This is just to let patient up date their address and phone number.
    /// </summary>
    UpdateOnly
}

public enum SignalElementType
{
    /// <summary>
    /// To and From lists.
    /// Not tied in any way to the users that are part of security.
    /// </summary>
    User,

    /// <summary>
    /// Typically used to insert "family" before "phone" signals.
    /// </summary>
    Extra,

    /// <summary>
    /// Elements of this type show in the last column and trigger the message to be sent.
    /// </summary>
    Message
}

public enum ContactMethod
{
    None,
    DoNotCall,
    HmPhone,
    WkPhone,
    WirelessPh,
    Email,
    SeeNotes,
    Mail,
    TextMessage
}

public enum ReferralToStatus
{
    None,
    Declined,
    Scheduled,
    Consulted,
    InTreatment,
    Complete
}

public enum SmokingSnoMed
{
    ///<summary>0 - UnknownIfEver</summary>
    _266927001,

    ///<summary>1 - SmokerUnknownCurrent</summary>
    _77176002,

    ///<summary>2 - NeverSmoked</summary>
    _266919005,

    ///<summary>3 - FormerSmoker</summary>
    _8517006,

    ///<summary>4 - CurrentSomeDay</summary>
    _428041000124106,

    ///<summary>5 - CurrentEveryDay</summary>
    _449868002,

    ///<summary>6 - LightSmoker</summary>
    _428061000124105,

    ///<summary>7 - HeavySmoker</summary>
    _428071000124103
}

public enum ProblemStatus
{
    Active,
    Resolved,
    Inactive
}

public enum ListenerServiceType
{
    /// <summary>
    /// Opt-in required to use the proxy service.
    /// </summary>
    ListenerServiceProxy,

    /// <summary>
    /// Customer is off by HQ's choice. This can only be undone by HQ.
    /// </summary>
    DisabledByHQ
}

public enum SortStrategy
{
    [Description("Name Asc")]
    NameAsc,

    [Description("Name Desc")]
    NameDesc,

    [Description("PatNum Asc")]
    PatNumAsc,

    [Description("PatNum Desc")]
    PatNumDesc
}

public enum ClaimSnapshotTrigger
{
    [Description("Claim Created")]
    ClaimCreate,

    [Description("Service - Specific Time")]
    Service,

    [Description("Insurance Payment Received")]
    InsPayment
}

public enum APIPermission
{
    AppointmentCreate,
    AppointmentRead,
    AppointmentUpdate,
    AppointmentDelete,
    PatientCreate,
    PatientRead,
    PatientUpdate,
    Subscriptions,
    LocationRead,
    OrganizationRead,
    PractitionerRead,
    ScheduleRead, 
    CapabilityStatement,
    AllergyIntoleranceRead,
    MedicationRead,
    ConditionRead,
    ServiceRequestRead,
    ServiceRequestCreate,
    ProcedureRead,
    ProcedureCreate,
    ProcedureUpdate,
    PaymentRead,
    PaymentCreate,
    CommunicationRead,
    CommunicationCreate,

    //MediaCreate,
    //MediaRead,
    ///<summary>For non-FHIR, this includes all GETs and Subscriptions POST. This is in the free tier.</summary>
    ApiReadAll = 25,

    ///<summary>For non-FHIR, this covers all permissions that are not separately listed. Can be extensive. Schedules, etc.</summary>
    ApiAllOthers,

    ///<summary>For non-FHIR, includes commlog entries, setting confirm status on appts, popups</summary>
    ApiComm,

    ///<summary>For non-FHIR, allows uploading images and PDFs. This is separate because these uploads might consume a lot of bandwidth.</summary>
    ApiDocuments,

    ///<summary>For non-FHIR, run any read-only query.  Returns max 1000 rows at a time, so the query itself must include pagination using sql limit.</summary>
    ApiQueries,

    ///<summary>For non-FHIR, includes edit or create appointments</summary>
    ApiAppointments,

    ///<summary>For non-FHIR, POST and PUT for claimprocs (insAdj), inssubs, and patplans (this is very complex and not intended for most users)</summary>
    ApiInsurance,

    ///<summary>For non-FHIR, includes edit or create patients.</summary>
    ApiPatients,

    ///<summary>For non-FHIR, allows creating payments. </summary>
    ApiPayments,

    ///<summary>For non-FHIR, allows creating payplans. </summary>
    ApiPayPlans,

    ///<summary>For non-FHIR, ProcedureLog POST, PUT & DELETE. </summary>
    ApiProcedureLogs,

    ///<summary>For non-FHIR, for rarely used Setup resources. </summary>
    ApiSetup,

    ///<summary>For non-FHIR, ASAPComm POST. </summary>
    ApiTextingASAP,

    ///<summary>For non-FHIR, reduces throttle to 500ms and increases remote limit to 1000 (and local/service limit to 10000). </summary>
    ApiEnterprise
}

///<summary>Will be deprecated soon. FHIRKeyStatus has mostly replaced this.</summary>
public enum APIKeyStatus
{
    ///<summary>Able to perform read operations. By default, all keys have this status.</summary>
    ReadEnabled,

    ///<summary>For an API key to have write permissions, it must first pay ODHQ a fee. This key can still perform read operations.</summary>
    WritePending,

    ///<summary>Able to perform read and write operations.</summary>
    WriteEnabled,

    ///<summary>ODHQ has purposely turned off write permissions for this key. This key can still perform read operations.</summary>
    WriteDisabled,

    ///<summary>Read and write operations disabled.</summary>
    Disabled
}

public enum FHIRKeyStatus
{
    ///<summary>Nobody knows!</summary>
    Unknown,

    ///<summary>This key can be used for queries.</summary>
    Enabled,

    ///<summary>The OD customer has disabled this key.</summary>
    [Description("Disabled by Customer")]
    DisabledByCustomer,

    ///<summary>The 3rd party developer has disabled this key.</summary>
    [Description("Disabled by Developer")]
    DisabledByDeveloper,

    ///<summary>Open Dental HQ has disabled this key.</summary>
    [Description("Disabled by Open Dental")]
    DisabledByHQ,

    ///<summary>The key is authorized for read only transactions. For backwards compatability.</summary>
    [Description("Read Enabled")]
    EnabledReadOnly,
}

public enum OAuthApplicationNames
{
    ///<summary>0</summary>
    Dropbox,

    ///<summary>1 - This uses Google's Out-Of-Band (OOB) OAuth flow. Google is depricating that flow on 10/03/2022.</summary>
    Google,

    ///<summary>2</summary>
    QuickBooksOnline,

    ///<summary>3 - This flow replaces OOB. It should be used instead of "Google".</summary>
    GoogleLoopbackIpAddressFlow,

    ///<summary>4 - Used for getting the Open Dental PayConnect2 API key.</summary>
    PayConnect2,

    ///<summary>5 - Used for getting the Open Dental PayConnect2 DLL key.</summary>
    PayConnect2Dll,

    ///<summary>6 - Used for getting the Open Dental DoseSpot Subscription key</summary>
    DoseSpotSubscriptionKey,

    ///<summary>7 - Used for getting the Open Dental DoseSpot ClientID</summary>
    DoseSpotClientID,

    ///<summary>8 - Gets a token generated by our client admin credentials to move offices from V1 to V2.</summary>
    DoseSpotAdminID,

    ///<summary>7 - Used for getting the Open Dental DoseSpot V2 ClientID</summary>
    MedispanClientID,
}

public enum DataStorageType
{
    LocalAtoZ,
    DropboxAtoZ,
    SftpAtoZ
}

public enum SignupPortalPermission
{
    ///<summary>This user is denied access to the signup portal.</summary>
    Denied,

    ///<summary>The user is only able to sign up any clinic for any eService</summary>
    FullPermission,

    ///<summary>The user is only able to see what clinics are signed up for eServices</summary>
    ReadOnly,

    ///<summary>The user is viewing the Signup Portal from HQ.</summary>
    FromHQ
}

public enum ClaimProcCreditsGreaterThanProcFee
{
    Allow,
    Warn,
    Block,
}

public enum ApptSchedEnforceSpecialty
{
    [Description("Don't Enforce")]
    DontEnforce,

    [Description("Warn")]
    Warn,
    
    [Description("Block")]
    Block,
}

public enum BlockoutType
{
    /// <summary>
    /// Do not schedule an appointment over this blockout.
    /// </summary>
    [Description("NS")]
    NoSchedule,

    /// <summary>
    /// Do not allow this blockout to be cut or copied and do not allow another blockout to be pasted on this blockout.
    /// </summary>
    [Description("DC")]
    DontCopy,
}

public enum ApptPriority
{
    /// <summary>
    /// Default priority
    /// </summary>
    Normal,

    /// <summary>
    /// Used to identify items for the ASAP list
    /// </summary>
    ASAP
}

public enum RecallPriority
{
    /// <summary>
    /// Default priority
    /// </summary>
    Normal,

    /// <summary>
    /// Used to identify items for the ASAP list
    /// </summary>
    ASAP
}

public enum WebSchedVerifyType
{
    None,
    Text,
    Email,
    TextAndEmail
}

public enum FrequencyUnit
{
    /// <summary>
    /// Default frequency is repeating once per day (1 Days).
    /// </summary>
    Days,
        
    Hours,
    Minutes
}

public enum ClaimZeroDollarProcBehavior
{
    /// <summary>Default value for the ClaimZeroDollarProcBehavior preference.  Allows all procedures to be attached to a claim</summary>
    Allow,

    /// <summary>Prompts the user to confirm attaching the $0 procedures to claims.</summary>
    Warn,

    /// <summary>Always block users from creating a claim for $0 procedures.</summary>
    Block,
}

///<summary>Differentiate between transaction types.</summary>
public enum AccountEntryType
{
    ///<summary>0 - adjustment.AdjNum.  Can be a positive (Debit) or negative (Credit) adjustment to the amount owed.</summary>
    Adjustment = 0,

    ///<summary>1 - claimproc.ClaimProcNum. For ins payments and/or writeoffs entered.</summary>
    ClaimPayment,

    ///<summary>2 - paysplit.SplitNum.  Patient payment on an account.</summary>
    Payment,

    ///<summary>3 - procedurelog.ProcNum.  Positive (debit, increases the amount owed).</summary>
    Procedure,

    ///<summary> 4 - claim</summary>
    Claim,

    PayPlanCharge
}

public enum SupplementalBackupStatuses
{
    Disabled,
    Enabled,
}

[Flags]
public enum HostedEmailStatus
{
    [Description("NotActivated")]
    NotActivated = 0,

    /// <summary>The absence of this flag prevents Enabled flag from having any effect.</summary>
    [Description("Signed Up")]
    SignedUp = 1,

    /// <summary>The absense of this flag is equivalent to Disabled.</summary>
    [Description("Enabled")]
    Enabled = 2,
}

public enum WebSchedNotifyType
{
    [Description("ASAP")]
    ASAP,
    
    [Description("NewPat")]
    NewPat,

    [Description("Recall")]
    Recall,

    [Description("ExistingPat")]
    ExistingPat,
}

public enum FromASAPShowAppointment
{
    [Description("All Appointments")]
    All,

    [Description("Non-Hygiene Appointments")]
    NonHygiene,

    [Description("Hygiene Appointments")]
    Hygiene
}

public enum LimitationTypeMet
{
    None,

    /// <summary>Individual annual max exceeded</summary>
    PeriodMax,

    /// <summary>Family annual max exceeded</summary>
    FamilyPeriodMax,

    /// <summary>Patient age exceeds limit</summary>
    Aging,

    /// <summary>Frequency limit exceeded</summary>
    Frequency
}