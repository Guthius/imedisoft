using System;
using System.ComponentModel;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Pref : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PrefNum;

    public string PrefName;
    public string ValueString;
    public string Comments;

    public Pref Copy()
    {
        return (Pref) MemberwiseClone();
    }
}

public enum PrefName
{
    ///<summary>ONLY USED FOR PREF-BOUND UI ELEMENTS or as temporary in memory place holder.  Never in the DB.</summary>
    [PrefName(ValueType = PrefValueType.NONE)]
    NotApplicable = 0,
    AccountingCashIncomeAccount = 1,

    ///<summary>The default cash payment type used to determine the CashSumTotal for deposit slips.</summary>
    AccountingCashPaymentType = 2,
    AccountingDepositAccounts = 3,
    AccountingIncomeAccount = 4,

    ///<summary>Boolean, true by defualt. When true, accounting invoice attachments are saved in the database.</summary>
    AccountingInvoiceAttachmentsSaveInDatabase = 5,
    AccountingLockDate = 6,

    ///<summary>Enum:AccountingSoftware 0=OpenDental, 1=QuickBooks, 2=QuickBooksOnline</summary>
    AccountingSoftware = 7,

    ///<summary>Boolean, false by default to preserve old functionality.  Allows users to make future dated patient payments when turned on.</summary>
    AccountAllowFutureDebits = 8,

    ///<summary>Defaulted to off, determines whether completed payment plans are visible in the account module.</summary>
    AccountShowCompletedPaymentPlans = 9,
    AccountShowPaymentNums = 10,
    ADAdescriptionsReset = 14,

    ///<summary>Boolean, true by default.  When set to true and a new family member is added, the new patient's email will be autofilled with the 
    ///guarantor's email.</summary>
    AddFamilyInheritsEmail = 15,

    ///<summary>Boolean. 1 by default. When enabled, addresses entered in formPatientEdit and addresses on patient forms imported using sheet and eform import will be verified with the USPS Addresses V3 API before saving.</summary>
    AddressVerifyWithUSPS = 16,

    ///<summary>ENUM:EnumAdjustmentBlockOrWarn When a user tries to make a negative adjustment that exceeds a remaining patient portion. 0 = Warn, 1 = Block, 2 = Allow.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    AdjustmentBlockNegativeExceedingPatPortion = 17,

    ///<summary>Boolean, true by default. When true, unattached pos/neg adjustments will counteract each other when running explicit linking logic.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    AdjustmentsOffsetEachOther = 19,

    ///<summary>Enum:ADPCompanyCode Used to generate the export file from FormTimeCardManage. Set in FormTimeCardSetup.</summary>
    ADPCompanyCode = 20,

    ///<summary>String, used to generate the ADP Run export file from FormTimeCardManage. Set in FormTimeCardSetup.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ADPRunIID = 21,

    ///<summary>Stored as DateTime, but cleared when aging finishes. Used only in OpenDentalService for automatic aging. The DateTime will be used as a flag to signal other
    ///connections that aging calculations have started and prevents another connection from running simultaneously.  In order to run aging, this will have to be cleared, either
    ///by the connection that set the flag when aging finishes, or by the user overriding the lock and manually clearing this pref.</summary>
    AgingBeginDateTime = 22,

    /// <summary>If true, aging will be calculated when receiving bulk checks for all families that bulk check applies to. Also applies to ERAs</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    AgingCalculateOnBatchClaimReceipt = 24,

    ///<summary>YN_DEFAULT_TRUE, 0-unknown,1-yes,2-no.
    ///For job 14902 - "Aging of AR change:  LIFO negative adjustments to (within) attached procedure and positive adjustments."</summary>
    [PrefName(ValueType = PrefValueType.YN_DEFAULT_TRUE)]
    AgingProcLifo = 27,

    ///<summary>This pref is hidden, so no UI to enable this feature.  If this is true, there will be a checkbox in the aging report window to age
    ///patient payments to payment plans.  Aging patient payments to payment plans will only work if the completed amounts on the payment plans are 0.
    ///Otherwise the payments and the completed amounts will essentially double the amounts of the payment plans in the aging calculation.  This is
    ///only for a specific customer, so no UI, defaults to false, only able to enable this via query.</summary>
    AgingReportShowAgePatPayplanPayments = 28,

    ///<summary>Stored as DateTime, but only the time is used.  This is the time of day during which aging will be calculated by the aging service.
    ///Aging will run during a one hour block of time starting with the time set.  If AgingBeginDateTime is not blank, aging will not be calculated.
    ///If AgingCalculatedMonthlyInsteadOfDaily is true, aging will not be calculated.  This will be blank if disabled.</summary>
    AgingServiceTimeDue = 29,

    ///<summary>How often to check for new alerts in Open Dental. Defaults to 180 (3 minutes).</summary>
    AlertCheckFrequencySeconds = 30,

    ///<summary>After this many minutes of inactivity, alerts will stop processing. Defaults to the same value as SignalInactiveMinutes.</summary>
    AlertInactiveMinutes = 31,

    ///<summary>FK to allergydef.AllergyDefNum</summary>
    AllergiesIndicateNone = 32,

    ///<summary>Boolean defaults to true.  If true, allows a user to email CC receipt otherwise not allowed.</summary>
    AllowEmailCCReceipt = 33,

    ///<summary>Enum:AllowedFeeSchedsAutomate. 0 by default. 0=None, 1=LegacyBlueBook. 2=BlueBook.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    AllowedFeeSchedsAutomate = 34,

    ///<summary>Boolean defauts to false.  If true, users can enter insurance payments that are for a future date.</summary>
    AllowFutureInsPayments = 35,

    ///<summary>Boolean, false by default to preserve old functionality.  Allows users to attach providers to prepayments while EnforceFully is on.
    ///</summary>
    AllowPrepayProvider = 36,

    ///<summary>Bool. Allows adjustments from FormClaimEdit. 0 by default.</summary>
    AllowProcAdjFromClaim = 37,
    AllowSettingProcsComplete = 38,

    ///<summary>DefNum for the default PaymentType for ODApi payments.</summary>
    ApiPaymentType = 39,
    AppointmentBubblesDisabled = 40,
    AppointmentBubblesNoteLength = 41,

    ///<summary>Reset calendar to today on clinic select.</summary>
    AppointmentClinicTimeReset = 42,

    ///<summary>Enum:SearchBehaviorCriteria 0=ProviderTime means based only on provider availability from the schedule., 1=ProviderTimeOperatory means based on provider schedule availability as well as the availabilty of their operatory (dynamic or directly assigned). This will prevent overlap of appointments.</summary>
    AppointmentSearchBehavior = 43,
    AppointmentTimeArrivedTrigger = 44,
    AppointmentTimeDismissedTrigger = 45,

    ///<summary>The number of minutes that the appointment schedule is broken up into.  E.g. "10" represents 10 minute increments.</summary>
    AppointmentTimeIncrement = 46,

    ///<summary>Set to true if appointment times are locked by default. If this is not set, and you are moving an appt or planned appt, the TimeLocked box should not get altered.</summary>
    AppointmentTimeIsLocked = 47,

    ///<summary>Used to set the color of the time indicator line in the appt module.  Stored as an int.</summary>
    AppointmentTimeLineColor = 48,
    AppointmentTimeSeatedTrigger = 49,

    ///<summary>Controls whether or not creating new appointments prompt to select an appointment type.</summary>
    AppointmentTypeShowPrompt = 50,

    ///<summary>Controls whether or not a warning will be displayed when selecting an appointment type would detach procedures from an appointment..</summary>
    AppointmentTypeShowWarning = 51,

    ///<summary>Integer in minutes.  Defaults to 30.  The defualt length of an appointment that is created without attaching any procedures.</summary>
    AppointmentWithoutProcsDefaultLength = 52,

    ///<summary>Boolean defauts to true.  If true, users can set appointments without procedures complete.</summary>
    ApptAllowEmptyComplete = 53,

    ///<summary>Boolean defauts to false.  If true, users can set future appointments complete.</summary>
    ApptAllowFutureComplete = 54,

    ///<summary>Int. Defaults to 4.  Number of days out to automatically refresh appointment module. -1 will get all appointments.</summary>
    ApptAutoRefreshRange = 55,
    ApptBubbleDelay = 56,

    ///<summary>True if the office has actived Arrivals in the UI.</summary>
    ApptArrivalAutoEnabled = 57,

    ///<summary>Boolean.  Defaults to false. Only used for the clinicpref table, not the preference table. When true, this clinic will use the 
    ///Appointment Arrival rules for ClinicNum 0.</summary>
    ApptArrivalUseDefaults = 58,

    ///<summary>True if the office has actived eConfirmations in the UI.</summary>
    ApptConfirmAutoEnabled = 59,

    ///<summary>True if HQ has confirmed that this office is signed up for eConfirmations.</summary>
    ApptConfirmAutoSignedUp = 60,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status in this list will enable the Send BYOD 
    ///link button in the kiosk manager.</summary>
    ApptConfirmByodEnabled = 61,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from 
    ///sending Arrival Response SMS and being marked Arrived.</summary>
    ApptConfirmExcludeArrivalResponse = 63,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from 
    ///sending Arrival SMS.</summary>
    ApptConfirmExcludeArrivalSend = 64,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment confirmed status that is in this list will not trigger an appointments confirmation 
    ///status to change when checking in a patient through eClipboard.</summary>
    ApptConfirmExcludeEclipboard = 65,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from EConfirmation RSVP updates.
    ///Prevents overwriting manual Confirmation status.</summary>
    ApptConfirmExcludeEConfirm = 66,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from EReminders.</summary>
    ApptConfirmExcludeERemind = 67,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from sending an EConfirmation.
    ///Prevents overwriting manual Confirmation status.</summary>
    ApptConfirmExcludeESend = 68,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from EThankYous.</summary>
    ApptConfirmExcludeEThankYou = 69,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from general messages.</summary>
    ApptConfirmExcludeGeneralMessage = 70,

    ///<summary>Comma delimited list of FK to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from new patient EThankYous.</summary>
    ApptConfirmExcludeNewPatThankYou = 71,

    ///<summary>FK to definition.DefNum.  If using automated confirmations, appointment set to this status when confirmation is sent.</summary>
    ApptEConfirmStatusSent = 74,

    ///<summary>FK to definition.DefNum.  If using automated confirmations, appointment set to this status when confirmation is confirmed.</summary>
    ApptEConfirmStatusAccepted = 75,

    ///<summary>FK to definition.DefNum.  If using automated confirmations, Anything that is not "Accepted" or "Sent".</summary>
    ApptEConfirmStatusDeclined = 76,

    ///<summary>FK to definition.DefNum.  If using automated confirmations, when failed by HQ for some reason.</summary>
    ApptEConfirmStatusSendFailed = 77,
    ApptExclamationShowForUnsentIns = 78,

    ///<summary>Float. Default 8. Valid between 1 and 40.</summary>
    ApptFontSize = 79,

    ///<summary>Boolean defaults to 0.  If true, adds the adjustment total to the net production in appointment module.</summary>
    ApptModuleAdjustmentsInProd = 82,

    ///<summary>Boolean defaults to 0, when true appt module will default to week view</summary>
    ApptModuleDefaultToWeek = 83,

    ///<summary>Bool; 0 by default. When false, calculates net and gross production by provider bars in each appointment view.
    ///When true, calulates net and gross production by appointments in the apppointment view. </summary>
    ApptModuleProductionUsesOps = 84,

    ///<summary>Enum: EnumApptProvPrompt. Determines the provider change behavior when moving or scheduling appointments.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    ApptModuleProviderPrompt = 85,

    ///<summary>Boolean defaults to 1 if there is relevant ortho chart info, when true appt menu will have an ortho chart item.</summary>
    ApptModuleShowOrthoChartItem = 86,

    ///<summary>Keeps the waiting room indicator times current.  Initially 1.</summary>
    ApptModuleRefreshesEveryMinute = 87,

    ///<summary>Boolean.  False by default.  If true, prevents from making changes (breaking, deleting, sending to unscheduled, changing status) 
    ///to completed appointments. The completed appointment MUST have completed procedures attached. If false, does nothing.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ApptPreventChangesToCompleted = 92,

    ///<summary>Integer</summary>
    ApptPrintColumnsPerPage = 93,

    ///<summary>Float</summary>
    ApptPrintFontSize = 94,

    ///<summary>Bool; True by default.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ApptPrintIsLandscape = 95,

    ///<summary>Stored as DateTime.  Currently the date portion is not used but might be used in future versions.</summary>
    ApptPrintTimeStart = 96,

    ///<summary>Stored as DateTime.  Currently the date portion is not used but might be used in future versions.</summary>
    ApptPrintTimeStop = 97,

    ///<summary>Int. Width of prov bar on individual appts, not the bars on left of screen.  Was historically 8.  In version 19.3, default was changed to 11 to allow text in provbar. Valid between 0 and 20.</summary>
    ApptProvbarWidth = 98,

    ///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
    ApptReminderDayInterval = 100,

    ///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
    ApptReminderHourInterval = 103,

    ///<summary>Enum:ApptSchedEnforceSpecialty Allow by default.  0=Allow, 1=Warn, 2=Block. Determines behavior when an appointment is scheduled in an
    ///operatory assigned to a clinic for a patient with a specialty that does not exist in the list of that clinic's specialties..</summary>
    ApptSchedEnforceSpecialty = 109,

    ///<summary>Boolean. When true allows appointments to overlap in the same operatory.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ApptsAllowOverlap = 110,

    ///<summary>Bool; True by default. When true, new appointments require at least one procedure to be attached.</summary>
    ApptsRequireProc = 111,

    ///<summary>Enum. Defaults to 0. 0=all colors seen in the Appt Module, 1=less color and without solid shading, 2=shades of gray and without solid shading.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    ApptPrintColorBehavior = 113,

    ///<summary>Bool; False by default.  When true, the secondary provider used when scheduling an appointment will use the Operatory's secondary provider no matter what.</summary>
    ApptSecondaryProviderConsiderOpOnly = 114,

    ///<summary>True if automated appointment thank yous are enabled for the entire DB. See ApptReminderRules for setup details.
    ///Permissions are still checked here at HQ so manually overriding this value will only make the program behave annoyingly, but won't break anything.</summary>
    ApptThankYouAutoEnabled = 115,

    ///<summary>Used as the value of the SUMMARY field in a .ics file for Appointment Thank You, Reminders, and eConfirmation auto replies
    ///[AddToCalendar] tag.</summary>
    ApptThankYouCalendarTitle = 116,

    ///<summary>Boolean.  Defaults to false. Only used for the clinicpref table, not the preference table. When true, this clinic will use the 
    ///Appointment Thank You rules for ClinicNum 0.</summary>
    ApptThankYouUseDefaults = 117,

    ///<summary>0 for Sunday, 1 for Monday. No other days supported. Sunday is now the default. Used to be Monday. No use supporting other days.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    ApptWeekViewStartDay = 118,

    ///<summary>Date, MinDate by default.  The Date that was set within the "Archive entries on or before:" field within the Archive tab of the 
    ///Backup window when the archive process was last ran successfully.</summary>
    [PrefName(ValueType = PrefValueType.DATE)]
    ArchiveDate = 119,

    ///<summary>In FormBackup the remove old data tab, if true make a backup when the user runs remove old data.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ArchiveDoBackupFirst = 120,

    ///<summary>Obfuscated password for the database user that will be used when directly connecting to the archive server. Not actually the hash.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ArchivePassHash = 122,

    ///<summary>The name of the server where the archive database should be located.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ArchiveServerName = 123,

    ///<summary>The user name for the database user that will be used when directly connecting to the archive server.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ArchiveUserName = 125,

    ///<summary>Default billing types selected when loading the Unsent Tab of the Accounts Receivable Manager.</summary>
    ArManagerBillingTypes = 126,

    ///<summary>Default state for the exclude if bad address (no zipcode) checkbox when loading the Unsent Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludeBadAddresses = 127,

    ///<summary>Default state for the exclude if unsent procs checkbox when loading the Unsent Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludeIfUnsentProcs = 128,

    ///<summary>Default state for the exclude if pending ins checkbox when loading the Unsent Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludeInsPending = 129,

    ///<summary>Default transaction types selected when loading the Sent Tab of the Accounts Receivable Manager - Sent tab.</summary>
    ArManagerLastTransTypes = 130,

    ///<summary>Default account age when loading the Sent Tab of the Accounts Receivable Manager.</summary>
    ArManagerSentAgeOfAccount = 131,

    ///<summary>Default number of days since the last payment when loading the Sent Tab of the Accounts Receivable Manager.</summary>
    ArManagerSentDaysSinceLastPay = 132,

    ///<summary>Default minimum balances when loading the Sent Tab of the Accounts Receivable Manager.</summary>
    ArManagerSentMinBal = 133,

    ///<summary>Default account age when loading the Unsent Tab of the Accounts Receivable Manager.</summary>
    ArManagerUnsentAgeOfAccount = 134,

    ///<summary>Default number of days since the last payment when loading the Unsent Tab of the Accounts Receivable Manager.</summary>
    ArManagerUnsentDaysSinceLastPay = 135,

    ///<summary>Default minimum balances when loading the Unsent Tab of the Accounts Receivable Manager.</summary>
    ArManagerUnsentMinBal = 136,

    ///<summary>Default state for the exclude if bad address (no zipcode) checkbox when loading the Excluded Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludedExcludeBadAddresses = 137,

    ///<summary>Default state for the exclude if unsent procs checkbox when loading the Excluded Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludedExcludeIfUnsentProcs = 138,

    ///<summary>Default state for the exclude if pending ins checkbox when loading the Excluded Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludedExcludeInsPending = 139,

    ///<summary>Default number of days since the last payment when loading the Excluded Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludedDaysSinceLastPay = 140,

    ///<summary>Default minimum balances when loading the Excluded Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludedMinBal = 141,

    ///<summary>Default account age when loading the Excluded Tab of the Accounts Receivable Manager.</summary>
    ArManagerExcludedAgeOfAccount = 142,

    ///<summary>If true, user is prompted if they would like to text ASAP list to offer the time slot created when breaking/unscheduling an appt. True by default.</summary>
    AsapPromptEnabled = 143,

    ///<summary>The template that is used when manually texting patients on the ASAP list.</summary>
    ASAPTextTemplate = 144,

    ///<summary>Enum - Enumerations.DataStorageType.  Normally 1 (AtoZ).  This used to be called AtoZfolderNotRequired, but that name was confusing.</summary>
    AtoZfolderUsed = 150,

    ///<summary>The number of audit trail entries that are displayed in the grid.</summary>
    AuditTrailEntriesDisplayed = 151,

    ///<summary>Boolean - Allows the audit trail to run on the report server. Defaults to true if reporting server is in use, otherwise false</summary>
    AuditTrailUseReportingServer = 152,

    ///<summary>Default folder for auto import in images module.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    AutoImportFolder = 155,

    ///<summary>Used to determine the runtime of the threads that do automatic communication in the listener.  Stored as a DateTime.</summary>
    AutomaticCommunicationTimeStart = 156,

    ///<summary>Used to determine the runtime of the threads that do automatic communication in the listener.  Stored as a DateTime.</summary>
    AutomaticCommunicationTimeEnd = 157,
    AutoResetTPEntryStatus = 160,

    /// <summary>Bool. True by default. When true, indexes are disabled for all tables when creating automatic backups. When disabled, the database will still need the indexes to be re-enabled. This is done on OD startup in Shared.EnableIndexesIfNeeded.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    BackupIndexesDisabled = 162,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BackupExcludeImageFolder = 163,

    [PrefName(ValueType = PrefValueType.STRING)]
    BackupFromPath = 164,
    BackupReminderLastDateRun = 165,

    [PrefName(ValueType = PrefValueType.STRING)]
    BackupRestoreAtoZToPath = 166,

    [PrefName(ValueType = PrefValueType.STRING)]
    BackupRestoreFromPath = 167,

    [PrefName(ValueType = PrefValueType.STRING)]
    BackupRestoreToPath = 168,

    [PrefName(ValueType = PrefValueType.STRING)]
    BackupToPath = 169,
    BadDebtAdjustmentTypes = 170,

    ///<summary>Boolean. False by default. When false, the Balance due in the Account module and statements will include estimated insurance payments. When true, the Balance due in the Account module and statements will exclude estimated insurance payments.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    BalancesDontSubtractIns = 171,
    BankAddress = 172,
    BankRouting = 173,

    [PrefName(ValueType = PrefValueType.STRING)]
    BillingAgeOfAccount = 174,
    BillingChargeAdjustmentType = 175,
    BillingChargeAmount = 176,
    BillingChargeLastRun = 177,

    ///<summary>Value is a string, either Billing or Finance.</summary>
    BillingChargeOrFinanceIsDefault = 178,

    ///<summary>Int, 0 by default. Claims where insurance has been pending for this many days or fewer will be ignored when processing Billing. Value of 0 means any claim where insurance is pending will be ignored, no matter how old. This will be 0 if BillingExcludeInsPending is set to false.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    BillingDaysExcludeInsPending = 179,
    BillingDefaultsInvoiceNote = 180,
    BillingDefaultsIntermingle = 181,
    BillingDefaultsLastDays = 182,

    ///<summary>The statement modes that will also receive a text message. Stored as a comma-separated list of integers where each item is the integer
    ///value of the StatementMode enum.</summary>
    BillingDefaultsModesToText = 183,

    [PrefName(ValueType = PrefValueType.STRING)]
    BillingDefaultsNote = 184,

    /// <summary>Boolean, false by default. Indicates if billing statements default to single patients(true) or guarantors(false).</summary>
    BillingDefaultsSinglePatient = 185,

    ///<summary>The template used for SMS text notifications for statements.</summary>
    BillingDefaultsSmsTemplate = 186,

    ///<summary>Value is an integer, identifying the max number of statements that can be sent per batch.  Default of 0, which indicates no limit.
    ///This preference is used for both printed statements and electronic ones.  It was decided to not rename the pref.</summary>
    BillingElectBatchMax = 187,

    ///<summary>Boolean, true by default.  Indicates if electronic billing should generate a PDF document.</summary>
    BillingElectCreatePDF = 189,
    BillingElectCreditCardChoices = 190,

    ///<summary>Boolean, true by default. If 1, prepend 'Adjust' to the Adjustment Type when building the electronic statement XML.</summary>
    BillingElectIncludeAdjustDescript = 191,

    ///<summary>Boolean, false by default. Used to output ClinicNum as "LocationID" for POS electronic billing, output to file mode.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingElectIncludeClinicNums = 192,

    ///<summary>No UI, can only be manually enabled by a programmer.  Only used for debugging electronic statements, because it will bloat the OpenDentImages folder.  Originally created to help with the "missing brackets bug" for EHG billing.</summary>
    BillingElectSaveHistory = 194,

    ///<summary>Output path for ClaimX EStatments.</summary>
    BillingElectStmtOutputPathClaimX = 195,

    ///<summary>Output path for EDS EStatments.</summary>
    BillingElectStmtOutputPathEds = 196,

    ///<summary>Output path for POS EStatments.</summary>
    BillingElectStmtOutputPathPos = 197,

    ///<summary>URL that EStatments are uploaded to for Dental X Change. Previously hardcoded in version 16.2.18 and below.</summary>
    BillingElectStmtUploadURL = 198,
    BillingElectVendorId = 200,
    BillingElectVendorPMSCode = 201,
    BillingEmailBodyText = 202,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingEmailIncludeAutograph = 203,
    BillingEmailSubject = 204,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingExcludeBadAddresses = 205,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingExcludeIfUnsentProcs = 206,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingExcludeInactive = 207,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingExcludeInsPending = 208,

    [PrefName(ValueType = PrefValueType.STRING)]
    BillingExcludeLessThan = 209,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingExcludeNegative = 210,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingIgnoreInPerson = 211,

    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingIncludeChanged = 212,

    ///<summary>Used with repeat charges to apply repeat charges to patient accounts on billing cycle date.</summary>
    BillingUseBillingCycleDay = 213,

    ///<summary>String. Comma-delimited list of definition.DefNums for Billing Types. An empty string denotes selecting all Billing Types.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    BillingSelectBillingTypes = 214,

    ///<summary>String. Comma-delimited list of insfilingcode.InsFilingCodeNums. An empty string denotes Unspecified. -1 in the list denotes selecting all Insurance Filing Codes.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    BillingSelectInsFilingCodes = 215,

    ///<summary>Boolean. Allows option to show activity on statements since the last $0 balance on the account (or family).</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    BillingShowTransSinceBalZero = 217,

    ///<summary>0=no,1=EHG,2=POS(xml file),3=ClaimX(xml file),4=EDS(xml file)</summary>
    BillingUseElectronic = 218,
    BirthdayPostcardMsg = 219,

    ///<summary>FK to definition.DefNum.  The adjustment type that will be used on the adjustment that is automatically created when an appointment is broken.</summary>
    BrokenAppointmentAdjustmentType = 226,

    ///<summary>Enumeration of type "BrokenApptProcedure".  Missed by default when D9986 is present.  This preference determines how broken appointments are handeld.</summary>
    BrokenApptProcedure = 227,

    ///<summary>Boolean.  0 by default.  When true, makes a commlog when an appointment is broken.</summary>
    BrokenApptCommLog = 229,

    ///<summary>Boolean.  0 by default.  When true, makes an adjustment when an appointment is broken.</summary>
    BrokenApptAdjustment = 230,

    ///<summary>Boolean. 0 by default. Require user to break scheduled appt before moving to a new day, the pinboard, the unscheduled list or deleting.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    BrokenApptRequiredOnMove = 231,

    ///<summmary>Int. Set by HQ</summmary>
    BytesPerSmsHeader = 232,

    ///<summmary>Int. Set by HQ</summmary>
    BytesPerSmsMessagePart = 233,

    ///<summary>Boolean.  True by default.  When true, Canadian PPO insurance plans create estimates for labs (default behavior for category percentage plans).</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    CanadaCreatePpoLabEst = 236,

    ///<summary>For Ontario Dental Association fee schedules.</summary>
    CanadaODAMemberNumber = 237,

    ///<summary>For Ontario Dental Association fee schedules.</summary>
    CanadaODAMemberPass = 238,

    ///<summary>Boolean.  0 by default.  If enabled, only CEMT can edit certain security settings.  Currently only used for global lock date.</summary>
    CentralManagerSecurityLock = 239,

    ///<summary>Blank by default.  Contains a key for the CEMT.  Each CEMT database contains a unique sync code.  Syncing from the CEMT will skip any databases without the correct sync code.</summary>
    CentralManagerSyncCode = 242,

    ///<summary>Preference to warn users when they have a nonpatient selected.</summary>
    ChartNonPatientWarn = 247,

    ///<summary>If true, then checkboxes for Tooth Chart Ortho Mode and Show Ortho Grids will get automatically checked and unchecked</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ChartOrthoTabAutomaticCheckboxes = 248,
    ClaimAttachExportPath = 250,

    ///<summary>If true, we will require claims to not have any missing data before they can be created/edited.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimEditRequireNoMissingData = 251,

    ///<summary>If true, we will display the Patient Repsonsibility in the claim edit and claim payment windows.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimEditShowPatResponsibility = 252,

    ///<summary>If true, users receives a warning when a received claim payment isn't finalized.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimFinalizeWarning = 253,

    ///<summary>If true, we will include the Pay Tracking column in the claim edit and claim payment windows.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimEditShowPayTracking = 254,

    ///<summary>Default value of "[PatNum]/".  Allows customization of ClaimIdentifier prefix format.</summary>
    ClaimIdPrefix = 255,
    ClaimFormTreatDentSaysSigOnFile = 256,

    ///<summary>When true, the default ordering provider on medical eclaim procedures will be set to the procedure treating provider.</summary>
    ClaimMedProvTreatmentAsOrdering = 257,

    ///<summary>Boolean, true by default.  When true, checks for "Unsent" or "Hold Until Pri Received" primary dental claims and prompts user to choose action on those claims.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimMedReceivedPromptForPrimaryClaim = 258,

    ///<summary>Boolean, false by default.  Receiving a medical claim will make primary dental claims with ClaimStatus 'Hold Until Pri Received' or 'Unsent' display a popup of actions for those primary claims.
    ///When true, this preference will remove the 'Do Nothing' option from that popup so the user has to change the status to
    ///'Waiting to Send' or send the primary claim.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimMedReceivedForcePrimaryStatus = 259,
    ClaimMedTypeIsInstWhenInsPlanIsMedical = 260,

    ///<summary>Enum:EclaimCobInsPaidBehavior.  Defaults to "ClaimLevel".  For X12 version 5010 eclaims.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    ClaimCobInsPaidBehavior = 261,

    ///<summary>True when using line item accounting.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimPayByTotalSplitsAuto = 262,

    ///<summary>Boolean, 0 by default.  When true, only Batch Insurance in Manage Module can be used to finalize payments.</summary>
    ClaimPaymentBatchOnly = 263,

    ///<summary>Int.  Valid values are >=0.  Use -1 to disable.  Used to be a date.  Now represents a rolling date, thus the name is a bit off.
    ///We decided to keep the name the same instead of deprecating and creating a new pref, because user never sees the pref name and to avoid bloat.
    ///Number of days (default 1) to subtract from the current date when deciding which NO PAYMENT claims and
    ///$0 claimprocs to consider when using the This Claim Only button from the Edit Claim window or when creating a batch payment from Manage module.
    ///Used for filtering Outstanding Claims from FormClaimPayBatch list, and when finalizing from Edit Claim window.</summary>
    ClaimPaymentNoShowZeroDate = 264,

    /// <summary>When true, does not select the first option as the default payment type. Payment type must be selected before clicking OK</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimPaymentPickStatementType = 265,

    ///<summary>When true, procedurecode overrides will send the override's description to insurance instead of the original procedurecode's description.</summary>
    ClaimPrintProcChartedDesc = 266,

    ///<summary>Receiving a primary claim will automatically recalculate the estimates on any claims that share procedures with the primary claim. 
    ///On by default.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimPrimaryReceivedRecalcSecondary = 267,

    ///<summary>Recieving a primary claim will make secondary claims with ClaimStatus 'Hold until Pri Recieved' display a popup of actions for those secondary claims.
    ///When true, this preference will remove the 'Do Nothing' option from that popup so the user has to change the status to
    ///'Waiting to Send' or send the secondary claim.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ClaimPrimaryRecievedForceSecondaryStatus = 268,

    ///<summary>Enum:ClaimProcCreditsGreaterThanProcFee.  Allow by default.  0=Allow, 1=Warn, 2=Block.  This preference either allows, warns or blocks the user from 
    ///entering an insurance payment on the Enter Payment screen if (for a procedure) the sum of the Ins Pay + Writeoff + any attached adjustments + and attached 
    ///patient payments > Procedure Fee </summary>
    ClaimProcAllowCreditsGreaterThanProcFee = 269,

    ///<summary>Boolean,  0 by default.  When true, allows claimprocs to be created for backdated completed procedures.</summary>
    ClaimProcsAllowedToBackdate = 270,

    ///<summary>For the Procedures Not Billed to Insurance report.  If true, when creating new claims from the report window, will group procedures
    ///by clinic and site.  If false, will block user from creating claims if the selected procedures for a specific patient have different
    ///clinis or different sites.  Default value is true to encourage automation.</summary>
    ClaimProcsNotBilledToInsAutoGroup = 271,

    ///<summary>Blank by default.  Computer name to receive reports from automatically.</summary>
    ClaimReportComputerName = 272,

    ///<summary>Boolean, 0 by default. When true, Open Dental Service will receive claim reports instead of the specified computer spawning a thread
    ///in FormOpenDental.</summary>
    ClaimReportReceivedByService = 273,

    ///<summary>Report receive interval. In minutes. 30 by default. If the ClaimReportReceiveLastDateTime preference is set, then this value will
    ///be 0.</summary>
    ClaimReportReceiveInterval = 274,

    ///<summary>Stores last time the reports were ran.</summary>
    ClaimReportReceiveLastDateTime = 275,

    ///<summary>Time to retrieve claim reports. Stored as a DateTime even though we only care about the time. If theClaimReportReceiveInterval 
    ///preference is set, then this value will be an empty string.</summary>
    ClaimReportReceiveTime = 276,

    ///<summary>Boolean.  0 by default.  If enabled, the Send Claims window will automatically validate e-claims upon loading the window.
    ///Validating all claims on load was old behavior that was significantly slowing down the loading of the send claims window.
    ///Several offices complained that we took away the validation until they attempt sending the claim.</summary>
    ClaimsSendWindowValidatesOnLoad = 277,

    ///<summary>Boolean.  0 by default.  If enabled, snapshots of claimprocs are created when claims are created.</summary>
    ClaimSnapshotEnabled = 278,

    ///<summary>DateTime where the time is the only useful part. 
    ///Stores the time of day that the OpenDentalService should create a claimsnapshot.</summary>
    ClaimSnapshotRunTime = 279,

    ///<summary>Enumeration of type "ClaimSnapshotTrigger".  ClaimCreate by default.  This preference determines how ClaimSnapshots get created. Stored as the enumeration.ToString(). Not available to most users to change. In the rare case that a customer needs it, there are instructions in UserControlFamilyGeneral for how to set it.</summary>
    ClaimSnapshotTriggerType = 280,

    ///<summary>Boolean. 1 by default. While a claim is being created, claim status "Hold Until Pri Received" doesn't 
    ///make sense if user is creating a primary claim already. However this has been allowed for a long time now so we want users who want to block 
    ///this ability have to go in and switch this preference off themselves.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PriClaimAllowSetToHoldUntilPriReceived = 281,

    ///<summary>When set to true, adding a claim tracking status to a claim requires an error.</summary>
    ClaimTrackingRequiresError = 282,

    ///<summary>Bool determines if 'None' will show as an option in the custom tracking form. Defaults to false,'None' will show as an option.</summary>
    ClaimTrackingStatusExcludesNone = 283,

    ///<summary>Enumeration of type "ClaimZeroDollarProcBehavior". Defaults to 0 (Allow).  Determines if $0 procedures can be attached to claims.</summary>
    ClaimZeroDollarProcBehavior = 284,
    ClaimsValidateACN = 285,
    ClearinghouseDefaultDent = 286,

    ///<summary>FK to clearinghouse.ClearingHouseNum.  Allows a different clearinghouse to be used for checking eligibility.
    ///Defaults to the current dental (or medical) clearinghouse which preserves old behavior.</summary>
    ClearinghouseDefaultEligibility = 287,
    ClearinghouseDefaultMed = 288,

    ///<summary>Boolean.  0 by default.  If enabled, new patients can be added with an usassigned clinic.</summary>
    ClinicAllowPatientsAtHeadquarters = 289,

    ///<summary>Boolean.  0 by default.  If enabled, lists clinics in alphabetical order.</summary>
    ClinicListIsAlphabetical = 290,

    ///<summary>String, "Workstation"(default), "User", "None". See FormMisc. Determines how recently viewed clinics should be tracked.</summary>
    ClinicTrackLast = 291,

    ///<summary>Boolean.  1 by default.  If enabled, displays 'Break' and 'Lunch' buttons in Manage Module, if disabled, changes 'Lunch' button text to
    ///'Break' and disables/hides true 'Break' button.  Effectively, enabling this preference means on-the-clock-breaks are allowed, and 
    ///disabling the preference means on-the-clock-breaks are not allowed.</summary>
    ClockEventAllowBreak = 292,

    ///<summary>Boolean. 0 by default. When set to true, all new clones will be put into their own family (guarantor as themselves)
    ///and then a super family will be created if one does not exist or the new clone will be associated to the master clone's super family.
    ///When set to false, all new clones blindly inherit their master's guarantor and super family settings.</summary>
    CloneCreateSuperFamily = 293,

    ///<summary>When a number of sessions of Open Dental Cloud is within the maximum minus this number an Alert will be created.</summary>
    CloudAlertWithinLimit = 295,

    ///<summary>Boolean.  False by default.  When true, causes CommLogs to auto-save on a timer.</summary>
    CommLogAutoSave = 303,
    ConfirmEmailMessage = 304,
    ConfirmEmailFamMessage = 305,
    ConfirmEmailSubject = 306,

    ///<summary>Boolean. False by default. When true, causes families to be grouped together for the purpose of confirmations.</summary>
    ConfirmGroupByFamily = 307,
    ConfirmPostcardMessage = 308,
    ConfirmPostcardFamMessage = 309,

    ///<summary>FK to definition.DefNum.  Initially 0.</summary>
    ConfirmStatusEmailed = 310,

    ///<summary>FK to definition.DefNum.</summary>
    ConfirmStatusTextMessaged = 311,

    ///<summary>The message that goes out to patients when doing a batch confirmation.</summary>
    ConfirmTextMessage = 312,
    ConfirmTextFamMessage = 313,
    CoPay_FeeSchedule_BlankLikeZero = 316,

    ///<summary>Boolean.  Typically set to true when an update is in progress and will be set to false when finished.  Otherwise true means that the database is in a corrupt state.</summary>
    CorruptedDatabase = 317,

    ///<summary>This is the default encounter code used for automatically generating encounters when specific actions are performed in Open Dental.  The code is displayed/set in FormEhrSettings.  We will set it and give the user a list of 9 suggested codes to use such that the encounters generated will cause the pateint to be considered part of the initial patient population in the 9 clinical quality measures tracked by OD.  CQMDefaultEncounterCodeSystem will identify the code system this code is from and the code value will be a FK to that code system.</summary>
    CQMDefaultEncounterCodeValue = 318,
    CQMDefaultEncounterCodeSystem = 319,
    CustomizedForPracticeWeb = 330,

    ///<summary>Boolean, false by default. When enabled, sql mode and replication status will not be changed. Used for Cloud Hosted Databases.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    DatabaseGlobalVariablesDontSet = 332,

    ///<summary>bool. Set to false by default. If true, the optimize database maintenance tool will be disabled.</summary>
    DatabaseMaintenanceDisableOptimize = 334,

    ///<summary>bool. Set to false by default. If true, database maintenance will skip table checks.</summary>
    DatabaseMaintenanceSkipCheckTable = 335,
    DateDepositsStarted = 338,
    DateLastAging = 339,

    //jordan The prefs below that start with "default" are badly named. Always start with most significant word, not least. Default should be suffix.
    DefaultCCProcs = 340,
    DefaultClaimForm = 341,

    ///<summary>Default folder for import in images module.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    DefaultImageImportFolder = 342,
    DefaultProcedurePlaceService = 343,

    ///<summary>Long. 0 by default. Used to assign a user group to a new user that is added by a user who does not have the SecurityAdmin user 
    ///permission.</summary>
    DefaultUserGroup = 344,

    ///<summary>Bool.  Default true 1. Applies to all computers.</summary>
    DirectX11ToothChartUseIfAvail = 346,

    ///<summary>Comma delimited list of procedure codes that represent Exam codes. D Code. Defaults to empty string.</summary>
    DiscountPlanExamCodes = 347,

    ///<summary>Comma delimited list of procedure codes that represent X-Ray codes. D Code. Defaults to empty string.</summary>
    DiscountPlanXrayCodes = 348,

    ///<summary>Comma delimited list of procedure codes that represent Prophylaxis codes. D Code. Defaults to empty string.</summary>
    DiscountPlanProphyCodes = 349,

    ///<summary>Comma delimited list of procedure codes that represent Fluoride codes. D Code. Defaults to empty string.</summary>
    DiscountPlanFluorideCodes = 350,

    ///<summary>Comma delimited list of procedure codes that represent Perio codes. D Code. Defaults to empty string.</summary>
    DiscountPlanPerioCodes = 351,

    ///<summary>Comma delimited list of procedure codes that represent Limited Exam codes. D Code. Defaults to empty string.</summary>
    DiscountPlanLimitedCodes = 352,

    ///<summary>Comma delimited list of procedure codes that represent PA codes. D Code.  Defaults to empty string.</summary>
    DiscountPlanPACodes = 353,

    ///<summary>Boolean. Set to true by default. When true, patient fields that do not have a matching patient field def will display at the bottom
    ///of the patient fields with gray text.</summary>
    DisplayRenamedPatFields = 354,

    ///<summary>The AtoZ folder path.</summary>
    DocPath = 357,

    /// <summary>Boolean. Determine whether or not to allow users to bypass OpenDentalLogin using ActiveDirectory. Default is false.</summary>
    DomainLoginEnabled = 359,

    ///<summary>Specifies the path to use when ActiveDirectory/Domain Logins are enabled.</summary>
    DomainLoginPath = 360,

    ///<summary>The ObjectGuid of the Active Directory domain object.</summary>
    DomainObjectGuid = 361,

    ///<summary>The ICD Diagnosis Code version primarily used by the practice.  Value of '9' for ICD-9, and '10' for ICD-10.</summary>
    DxIcdVersion = 363,

    ///<summary>For payment plans (formerly known as dynamic payment plans). This only gets updated when charges are issued for payment plans through the background service. It uses this date time to allow it to run exactly once per day. Charges can be manually run for a single payment plan at any time, but that won't affect this pref.</summary>
    [PrefName(ValueType = PrefValueType.DATETIME)]
    DynamicPayPlanLastDateTime = 364,

    ///<summary>FK to definition.DefNum Identifies a specific hidden PaySplitUnearnedType to allow unearned money to stay attached to dynamic payment plans (hides this money from the income transfer system).</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    DynamicPayPlanPrepaymentUnearnedType = 365,

    ///<summary>Defaults to 9AM.  The time the user has specified that they would like the service to run on each day.</summary>
    [PrefName(ValueType = PrefValueType.DATETIME)]
    DynamicPayPlanRunTime = 366,

    ///<summary>The date and time that the service started running today. Will be blank if not currently running.</summary>
    [PrefName(ValueType = PrefValueType.DATETIME)]
    DynamicPayPlanStartDateTime = 367,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyBasicModules = 368,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHideCapitation = 370,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHideClinical = 371,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHideDentalSchools = 372,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHideHospitals = 373,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHideInsurance = 374,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHideMedicaid = 375,
    EasyHidePrinters = 376,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHidePublicHealth = 377,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyHideRepeatCharges = 378,

    [PrefName(ValueType = PrefValueType.BOOL)]
    EasyNoClinics = 379,
    EclaimsSeparateTreatProv = 382,

    ///<summary>Use Optional Patient ID "PatID" in place of SubscriberID if this preference is checked. Both 4010 and 5010. Disabled by default for backward compatibility.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EclaimsSubscIDUsesPatID = 383,

    ///<summary>This is the global default space below each eForm field. It can be overridden for each form using EForm.SpaceBelowEachField and for each field using EFormField.SpaceBelow. Initial value is 20. Can be between 0 and 100.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    EformsSpaceBelowEachField = 406,

    ///<summary>This is the global default space to the right of each eForm field. It can be overridden for each form using EForm.SpaceToRightEachField and for each field using EFormField.SpaceToRight. Initial value is 10. Can be between 0 and 100.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    EformsSpaceToRightEachField = 407,

    ///<summary>Boolean, false by default.  When this is set and using eRx it will utilize the currently selected clinic instead of the patient's default clinic.</summary>
    ElectronicRxClinicUseSelected = 413,

    /// <summary>FK to EmailAddress.EmailAddressNum.  It is not required that a default be set.</summary>
    EmailDefaultAddressNum = 417,

    ///<summary>Enum EmailPlatform stored as string.  Determines if the default email sender will be Insecure(standard email), Secure(EmailHosting), or Direct(Secure WebMail).  Even though there are more options, only these three are currently supported.  While EmailPlatform is a Flags enum, this preference should only be set to single value. Used in ClinicPref table.</summary>
    EmailDefaultSendPlatform = 418,

    ///<summary>Bool which indicates, where applicable, all emails being sent should have the EmailDisclaimerTemplate appended to the end of the EmailBody.</summary>
    EmailDisclaimerIsOn = 419,

    ///<summary>String provides template for any email correspondence methods participating in Email Disclaimer. Must include [PostalAddress] tag.</summary>
    EmailDisclaimerTemplate = 420,

    ///<summary>Time interval in minutes describing how often to automatically check the email inbox for new messages. Default is 5 minutes.</summary>
    EmailInboxCheckInterval = 427,

    ///<summary>String that contains the HTML shell for emails. </summary>
    EmailMasterTemplate = 428,

    ///<summary>FK to EmailAddress.EmailAddressNum.  Used for webmail notifications (Patient Portal).</summary>
    EmailNotifyAddressNum = 429,

    ///<summary>Long.  The clinic used for Secure Email by clinic 0.</summary>
    EmailSecureDefaultClinic = 433,

    ///<summary>Enum. Flags MassEmailStatus. Defaults to None. Used for the clinicpref table. When activated and enabled, this practice or 
    ///clinic is enabled to send secure emails.</summary>
    EmailSecureStatus = 436,

    /// <summary>PrefValue is of type EmailPlatform, stored as string. 
    /// Clinic based pref which determines which email platform should be used to send statements during billing.</summary>
    EmailStatementsSecure = 441,

    /// <summary>Boolean. 0 means false and means it is not an EHR Emergency, and emergency access to the family module is not granted.</summary>
    EhrEmergencyNow = 444,

    ///<summary>Enables the auto-complete feature when filling out email addresses in the Email Edit window.</summary>
    EnableEmailAddressAutoComplete = 447,

    ///<summary>Warns the user if the Medicaid ID is not the proper number of digits for that state.</summary>
    EnforceMedicaidIDLength = 448,

    ///<summary>Enables refresh while typing in patient select. True by default.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EnterpriseAllowRefreshWhileTyping = 449,

    ///<summary>Applies to these 7 windows: Recall, Confirmation, Planned Appt Tracker, Unsched, ASAP, Ins Verification, Text Messaging.  If this preference is turned on , then the "All Clinics" option is hidden, preventing user from clicking on that option and overloading the server.</summary>
    EnterpriseApptList = 450,

    ///<summary>Boolean, false by default. When true, creating commlogs will not have default values preloaded for Type, Mode_, or SentOrReceived fields.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EnterpriseCommlogOmitDefaults = 451,

    ///<summary>Boolean, false by default. When true search patients only by exact phone number match of x digits. Number of digits from EnterpriseExactMatchPhoneNumDigits pref.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EnterpriseExactMatchPhone = 452,

    ///<summary>Number of digits in phone number for exact match setting in enterprise setup.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    EnterpriseExactMatchPhoneNumDigits = 453,

    ///<summary>Boolean, false by default. When true, hygiene procedures will use the primary provider's PPO fee rather than the hygienist's.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EnterpriseHygProcUsePriProvFee = 454,

    ///<summary>Boolean, false by default. When true, manual refreshes are enabled for enterprise.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EnterpriseManualRefreshMainTaskLists = 455,

    ///<summary>Boolean, false by default. When true the Appointment 'None' View will not be selected by default when there is at least one existing 
    ///Appointment view. False preserves current behavior - ie: Appointment 'None' View displays on first load of a clinic for a brand
    ///new user in a clinic.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EnterpriseNoneApptViewDefaultDisabled = 456,

    ///<summary>definition.DefNum of category InsurancePaymentType. Zero by default. Used for claimpayments made from ERA ACH payments.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    EraAchPaymentType = 457,

    ///<summary>Boolean, true by default. When true, blocks user from entering ERA payments using By Total payments.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EraAllowTotalPayments = 458,

    ///<summary>Enum:EraAutomationMode. 1 by default. 0=UseGlobal(not used in the preference table), 1=ReviewAll, 2=SemiAutomatic.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    EraAutomationBehavior = 459,

    ///<summary>Enum:EnumEraAutoPostWriteOff. 0 by default. 0=PriFromERA, 1=Always, 2=PriFromPlan</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    EraAutoPostWriteOff = 460,

    ///<summary>definition.DefNum of category InsurancePaymentType. Zero by default. Used for claimpayments made from ERA CHK payments.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    EraChkPaymentType = 461,

    ///<summary>definition.DefNum of category InsurancePaymentType. Zero by default. Used for claimpayments made from ERA payments of types other than ACH, CHK, and FWT.
    ///Will also be used for ACH, CHK, and FWT payments if their corresponding preferences aren't set (EraAchPaymentType, EraChkPaymentType, EraFwtPaymentType).
    ///If those preferences and this preference aren't set, we look for an InsurancePaymentType definition with the name "EFT" for ACH, "Check" for CHK, and "Wired" for FWT.
    ///The claimpayment type is set to zero (undefined) if a match is not found.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    EraDefaultPaymentType = 462,

    ///<summary>definition.DefNum of category InsurancePaymentType. Zero by default. Used for claimpayments made from ERA FWT payments.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    EraFwtPaymentType = 463,

    ///<summary>Boolean, false by default.  When true then there will be 1 page per each claim paid for an ERA header and ERA claim paid on printouts.</summary>
    EraPrintOneClaimPerPage = 464,

    ///<summary>Boolean, true by default.  When true, the ERA 'Verify and Enter Payment' window will post WriteOffs for procedures covered by category percentage or 
    ///medicaid/flat copay insurance plans.  When false, WriteOffs will not be posted for these insurance plan types.</summary>
    EraIncludeWOPercCoPay = 465,

    ///<summary>A comma delimited list of Claim Adjustment Reason Codes (CARCs). Defaults to "97,16,252,242,50,251,B7,226,250", a list provided by NADG. If the payment 
    ///information on an ERA applies any of the codes in this list to a claim or any of the claim's procedures, ERA auto-processing will not process the claim. Claims assigned
    ///these codes often need to be resubmitted with additional documentation, and automatically receiving them with no payment forces users to detach the claim from the check
    ///and change the status for the claim and each ClaimProc before resubmitting.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    EraNoAutoProcessCarcCodes = 466,

    ///<summary>Boolean, true by default.  When true loads database data for ERAs when loading the ERA 835s window.  Enterprise only for now.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EraRefreshOnLoad = 467,

    ///<summary>Boolean, false by default.  When true FormEtrans835 allows the user to filter by Control ID and shows the value in a grid column.
    ///Otherwise Control ID UI is not visable and can not be used.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EraShowControlIdFilter = 468,

    ///<summary>Boolean, true by default.  When true FormEtrans835s allows the user to filter by status and clinic and shows the values in a grid column.
    ///Otherwise Status and Clinic UI is not visible and can not be used. This makes FormEtrans835s load ERAs faster for large databases.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EraShowStatusAndClinic = 469,

    ///<summary>Boolean, false by default. When true, will use a list of dates instead of a date range when matching claims in Claims.GetClaimFromX12.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    EraStrictClaimMatching = 470,
    ExportPath = 473,

    ///<summary>Int.  Number of days before day of runtime to use as the 'ChangedSinceDate' for running automated family balancer.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    FamilyBalancerChangedSinceNumDays = 474,

    ///<summary>Boolean, false by default.  When true, uses pref FamilyBalancerLastDayRun as the 'ChangedSinceDate' and when false, uses pref FamilyBalancerChangedSinceNumDays.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    FamilyBalancerChangedSinceUseLastDayRun = 475,

    ///<summary>DateTime.  Timestamps the last time the automated family balancer was run.</summary>
    [PrefName(ValueType = PrefValueType.DATETIME)]
    FamilyBalancerDateLastRun = 476,

    ///<summary>Boolean, false by default.  When true, running automated family balancer will delete all transfers, regardless of date, if they have PayType=0 and Amt=0.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    FamilyBalancerDeleteAllTransfers = 477,

    ///<summary>Boolean, false by default.  When true, allows automated family balancer to run from OpenDentalService.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    FamilyBalancerEnabled = 478,

    ///<summary>Stored as a DateTime, only TimeOfDay is used to determine when family balancer should run.</summary>
    [PrefName(ValueType = PrefValueType.DATETIME)]
    FamilyBalancerTimeRun = 479,

    ///<summary>Boolean, true by default.  When true, runs automated family balancer using FIFO. When false, runs as rigorous.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    FamilyBalancerUseFIFO = 480,

    ///<summary>Allows guarantor access to all family health information in the patient portal.  Default is 1.</summary>
    FamPhiAccess = 481,
    FinanceChargeAdjustmentType = 483,
    FinanceChargeAPR = 484,
    FinanceChargeAtLeast = 485,
    FinanceChargeLastRun = 486,
    FinanceChargeOnlyIfOver = 487,

    ///<summary>Boolean. When true, blank fees in a Fixed Benefit type schedule will be treated as a $0 insurance coverage (patient owes the full amount 
    ///of the PPO fee).  When false, blank fees in a Fixed Benefit type schedule will not be treated as $0 insurance coverage(insurance is estimated 
    ///at the full amount of the PPO fee).</summary>
    FixedBenefitBlankLikeZero = 488,

    ///<summary>Double defaults to 0. Prevents clicks from happening in a window. 
    ///Currently used in ApptEdit to prevent procedures from being attached or detached for delay amount in tenths of a second.
    ///Will always be stored in en-US format.</summary>
    FormClickDelay = 489,

    /// <summary>Bool defaults to 0. When false, future dates transactions are not allowed. </summary>
    FutureTransDatesAllowed = 492,

    ///<summary>Used to store the ClinicNum of the last clinic the global update writeoff tool completed before being paused or interrupted.  Only stored if an
    ///unrestricted user runs the update for all clinics.  When starting the tool again for an unrestricted user and all clinics the tool will pick up
    ///on the next clinic (ordered by primary key).</summary>
    GlobalUpdateWriteOffLastClinicCompleted = 494,

    ///<summmary>String. Gsm chars worth 7 bits each</summmary>
    GsmCharSet = 495,

    ///<summary>String. Gsm chars worth 14 bits each</summary>
    GsmExtendedCharSet = 496,

    ///<summary>Encrypted.  Has no UI to change it, but status can be viewed in FormSupportStatus.  Used to validate whether customer is on support and should have access to Help, Sparks3D, Imaging, etc.  See the OpenDentalHelp project.</summary>
    HelpKey = 498,
    HL7FolderOut = 499,

    ///<summary>procedurelog.DiagnosticCode will be set to this for new procedures and complete procedures if this field was blank when set complete.
    ///This can be an ICD-9 or an ICD-10.  In future versions, could be another an ICD-11, ICD-12, etc.</summary>
    ICD9DefaultForNewProcs = 503,

    ///<summary>FK to Definition.DefNum. If 0, then it follows old behavior where it uses the top category.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    ImageCategoryDefault = 504,

    ///<summary>Use the old Images module, prior to the 2020 overhaul.  Required for Suni capture. False by default. The only interface for changing this is by enabling/disabling Suni.  This might be used for certain troubleshooting, but only in a test environment, never for fixing customer issues.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ImagesModuleUsesOld2020 = 506,
    ImageWindowingMax = 507,
    ImageWindowingMin = 508,

    ///<summary>Only for documents (images), not mounts.  MountDefs have their own place for defaults.  Scale, decimal places, and units, separated by spaces.  Example: "123.4 0 mm". The first two are required; units is optional. Converted into an ImageDraw of type ScaleValue in a lazy manner. Not created unless needed.</summary>
    ImagingDefaultScaleValue = 509,

    ///<summary>Sort images within each category in Imaging module by date created (descending). False by default.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ImagingOrderDescending = 510,

    ///<summary>Boolean.  False by default.  When enabled a fix is enabled within ODTextBox (RichTextBox) for foreign users that use 
    ///a different language input methodology that requires the composition of symbols in order to display their language correctly.
    ///E.g. the Korean symbol '역' (dur) will not display correctly inside ODTextBoxes without this set to true.</summary>
    ImeCompositionCompatibility = 511,

    ///<summary>YN_DEFAULT_FALSE. When enabled (1-Yes), income transfers are automatically made upon receiving a claim. 
    ///The income transfers are to reallocate patient payments associated to insurance production that has been overpaid to underpaid production on the same claim.
    ///Any income transfers made on behalf of this preference will ONLY move money between production associated to the same claim.
    ///Money will be transferred to unearned in the event that the entire claim has been overpaid.
    ///The default behavior is (0-Unknown) which will defer to the RigorousAccounting preference which will ONLY run when it is set to 0=Fully Enforced.
    ///Income transfers will NOT be automatically created when this preference is disabled (2-No).</summary>
    [PrefName(ValueType = PrefValueType.YN_DEFAULT_FALSE)]
    IncomeTransfersMadeUponClaimReceived = 512,

    ///<summary>Boolean. True by default. When true, negative production can be transferred to unearned as income via paysplits. When false, income transfers will not be allowed when negative production needs to be allocated.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    IncomeTransfersTreatNegativeProductionAsIncome = 513,

    ///<summary>Boolean.  False by default.  When true, when importing 834s, all patient plans that are not in the 834 will be replaced for the patient.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    Ins834DropExistingPatPlans = 514,
    Ins834ImportPath = 515,

    ///<summary>Boolean. True by default. Sets the checkbox for 834 preview that automatically creates employers when importing.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    Ins834IsEmployerCreate = 516,

    [PrefName(ValueType = PrefValueType.BOOL)]
    Ins834IsPatientCreate = 517,

    ///<summary>Boolean. True by default. Automatically marks sent claims and their ClaimProcs Received with $0 payments 
    ///for insurance subscriptions that don't assign benefits (InsSub.AssignBen is false).</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsAutoReceiveNoAssign = 518,

    /// <summary>Boolean. False by default. Controls automated InsVerify behavior. Determines if annual max is checked.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsBatchVerifyCheckAnnualMax = 519,

    /// <summary>Boolean. False by default. Controls automated InsVerify behavior.  Determines if deductible is checked.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsBatchVerifyCheckDeductible = 520,

    /// <summary>Boolean. False by default. Controls automated InsVerify behavior.  Determines if adjustments can be created.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsBatchVerifyCreateAdjustments = 521,

    /// <summary>Boolean. False by default. Controls automated InsVerify behavior.  Determines if ins history can be changed.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsBatchVerifyChangeInsHist = 522,

    /// <summary>Boolean. False by default. Controls automated InsVerify behavior.  Determines if ins effective dates can be changed.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsBatchVerifyChangeEffectiveDates = 523,

    ///<summary>0=Default practice provider, -1=Treating Provider. Otherwise, FK to provider.ProvNum.</summary>
    InsBillingProv = 537,

    ///<summary>Enum:InsBlueBookAllowedFeeMethod. 2 by default. 0=Median, 1=Average, 2=MostRecent. The method used by the Blue Book feature for generating estimates.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    InsBlueBookAllowedFeeMethod = 538,

    ///<summary>Enum:InsBlueBookAnonShareEnable 0 by default. 0=Off, 1=OffNoPrompt, 2=On When on, insurance payment data will be anonymously shared with Open Dental for the purpose of making improvements to the Blue Book feature.</summary>
    [PrefName(ValueType = PrefValueType.ENUM)]
    InsBlueBookAnonShareEnable = 539,

    ///<summary>Bool. True by default. When true, allows insplan.PlanNum to be used in FormInsBlueBookRule hierarchy.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsBlueBookUsePlanNumOverride = 540,

    ///<summary>Int. 80 by default. The percentage of the UCR fee that will be used as the Allowed amount when the Blue Book UcrFee rule is making an estimate.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    InsBlueBookUcrFeePercent = 541,
    InsDefaultCobRule = 542,
    InsDefaultPPOpercent = 543,
    InsDefaultShowUCRonClaims = 544,

    ///<summary>True if assigning benefits to provider, false if assigning benefits to patients. There is also a Clinic level override.</summary>
    InsDefaultAssignBen = 545,

    ///<summary>Boolean.  False by default.  When true, insurance estimates will be recalculated on ClaimProcs even when marked as Received.  When 
    ///false, ClaimProcs marked as Received will not recalculate insurance estimates.</summary>
    InsEstRecalcReceived = 546,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent bitewing codes used for insurance prior dates of service.  
    ///Defaults to InsBenBWCodes codes.</summary>
    [Description("Bitewing Ins Hist Codes")]
    InsHistBWCodes = 547,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent Debridement codes used for insurance prior dates of service.
    ///Defaults to InsBenFullDebridementCodes codes.</summary>
    [Description("Debridement Ins Hist Codes")]
    InsHistDebridementCodes = 548,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent exam codes used for insurance prior dates of service.
    ///Defaults to InsBenExamCodes codes.</summary>
    [Description("Exam Ins Hist Codes")]
    InsHistExamCodes = 549,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent pano codes used for insurance prior dates of service.
    ///Defaults to InsBenPanoCodes codes.</summary>
    [Description("FMX/Pano Ins Hist Codes")]
    InsHistPanoCodes = 550,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent perio maintenance codes used for insurance prior dates of service.
    ///Defaults to InsBenPerioMaintCodes codes.</summary>
    [Description("Perio Maint Ins Hist Codes")]
    InsHistPerioMaintCodes = 551,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent perio LL codes used for insurance prior dates of service.
    ///Defaults to InsBenSRPCodes codes</summary>
    [Description("Perio Scaling LL Ins Hist Codes")]
    InsHistPerioLLCodes = 552,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent perio LL codes used for insurance prior dates of service.
    ///Defaults to InsBenSRPCodes codes.</summary>
    [Description("Perio Scaling LR Ins Hist Codes")]
    InsHistPerioLRCodes = 553,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent perio UL codes used for insurance prior dates of service.
    ///Defaults to InsBenSRPCodes codes.</summary>
    [Description("Perio Scaling UL Ins Hist Codes")]
    InsHistPerioULCodes = 554,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent perio UR codes used for insurance prior dates of service.
    ///Defaults to InsBenSRPCodes codes.</summary>
    [Description("Perio Scaling UR Ins Hist Codes")]
    InsHistPerioURCodes = 555,

    ///<summary>Comma delimited list of procedure codes (D codes) that represent prophy codes used for insurance prior dates of service.
    ///Defaults to InsBenProphyCodes codes.</summary>
    [Description("Prophy Ins Hist Codes")]
    InsHistProphyCodes = 556,

    ///<summary>False by default. When true, blank entries in out-of-network fee schedules are treated as zero, resulting in a zero insurance estimate. The default false behavior would cause a blank entry to use the procedure fee for the estimate. Users could go through and enter zeros for those procedure fee schedules, but that's a lot of work.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsOutOfNetworkBlankLikeZero = 557,

    ///<summary>Prevents users from creating initial primary insurance payments where the sum of the payment and write-off exceed the adjusted procedure fee.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    InsPayNoInitialPrimaryMoreThanProc = 558,

    ///<summary>Boolean. True by default. When enabled, disallow writeoffs amount greater than procedure fee.</summary>
    InsPayNoWriteoffMoreThanProc = 559,

    ///<summary>String. Empty by default. When not empty, merging InsPlans will be disabled since InsPlans are merging on another machine</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    InsPlanMergeInProgress = 561,

    ///<summary>Boolean.  False by default.  When true, insurance plan with exclusions will always use UCR fee.</summary>
    InsPlanUseUcrFeeForExclusions = 562,

    ///<summary>Boolean.  False by default.  When true, insurance plans with exclusions are marked as Do Not Bill Ins.</summary>
    InsPlanExclusionsMarkDoNotBillIns = 563,

    [PrefName(ValueType = PrefValueType.BOOL)]
    ///<summary>Boolean - Defaults to false. Allows offices to cause processing estimates to zero out write-offs when annual maximum is surpassed entirely.</summary>
    InsPlansZeroWriteOffsOnAnnualMax = 564,

    [PrefName(ValueType = PrefValueType.BOOL)]
    ///<summary>Boolean - Defaults to false. Allows offices to cause processing estimates to zero out write-offs when aging or frequency limitations are exceeded.</summary>
    InsPlansZeroWriteOffsOnFreqOrAging = 565,

    ///<summary>Boolean.  False by default.  When enabled, procedure fees will always use the UCR fee.</summary>
    InsPpoAlwaysUseUcrFee = 566,

    ///<summary>0 by default.  If false, secondary PPO writeoffs will always be zero (normal).  At least one customer wants to see secondary writeoffs.</summary>
    InsPPOsecWriteoffs = 567,

    ///<summary>Boolean, false by default.  When true, treatment plan module and appointment scheduling checks for frequency conflicts.</summary>
    InsChecksFrequency = 568,
    InsurancePlansShared = 569,

    ///<summary>7 by default.  Number of days before displaying insurances that need verified.</summary>
    InsVerifyAppointmentScheduledDays = 570,

    [PrefName(ValueType = PrefValueType.INT)]
    ///<summary>Defaults to the value of InsVerifyAppointmentScheduledDays. Number of days before displaying a Medicaid plan that needs to be verified.</summary>
    InsVerifyAppointmentScheduledDaysMedicaid = 571,

    ///<summary>90 by default. Number of days before requiring insurance plans to be verified.</summary>
    InsVerifyBenefitEligibilityDays = 572,

    [PrefName(ValueType = PrefValueType.INT)]
    ///<summary>Defaults to the value of InsVerifyBenefitEligibilityDays. Number of days before requiring Medicaid insurance plans to be verified.</summary>
    InsVerifyBenefitEligibilityDaysMedicaid = 573,

    ///<summary>1 by default.  Number of days that a past appointment will show in the "Past Due" insurance verification grid.</summary>
    InsVerifyDaysFromPastDueAppt = 574,

    [PrefName(ValueType = PrefValueType.INT)]
    ///<summary>Defaults to the value of InsVerifyDaysFromPastDueAppt.  Number of days that past appointments will show in the "Past Due" ins verification grid for Medicaid.</summary>
    InsVerifyDaysFromPastDueApptMedicaid = 575,

    ///<summary>Boolean, false by default.  When true, defaults a filter to the current user instead of All when opening the InsVerifyList.</summary>
    InsVerifyDefaultToCurrentUser = 576,

    ///<summary>Boolean, false by default.  When true, excludes patient clones from the Insurance Verification List.</summary>
    InsVerifyExcludePatientClones = 577,

    ///<summary>Boolean, false by default.  When true, excludes patient plans associated to insurance plans that are marked "Do Not Verify" from the Insurance Verification List.</summary>
    InsVerifyExcludePatVerify = 578,

    ///<summary>Boolean, false by default.  When true, if an appointment is after the benefit renewal month for the insurance plan, make that InsPlan be reverified and postdate the insverify.DateLastVerified.</summary>
    InsVerifyFutureDateBenefitYear = 579,

    ///<summary>Boolean, false by default.  When true, if an appointment is after the benefit renewal month for the insurance plan, make that PatPlan be reverified and postdate the insverify.DateLastVerified.</summary>
    InsVerifyFutureDatePatEnrollmentYear = 580,

    [PrefName(ValueType = PrefValueType.STRING)]
    ///<summary>String, empty by default. Stores a comma delimited list of filing codes that determine which insurance plans use Medicaid verification.</summary>
    InsVerifyMedicaidFilingCodes = 581,

    ///<summary>30 by default.  Number of days before requiring patient plans to be verified.</summary>
    InsVerifyPatientEnrollmentDays = 582,

    [PrefName(ValueType = PrefValueType.INT)]
    ///<summary>Defaults to the value of InsVerifyPatientEnrollmentDays.  Number of days before requiring Medicaid patient plans to be verified.</summary>
    InsVerifyPatientEnrollmentDaysMedicaid = 583,

    ///<summary>Writeoff description displayed in the Account Module and on statements.  If blank, the default is "Writeoff".
    ///We are using "Writeoff" since "PPO Discount" was only used for a brief time in 15.3 while it was Beta and no customer requested it</summary>
    InsWriteoffDescript = 584,
    IntermingleFamilyDefault = 585,

    ///<summary>Preference to show writeoffs in the StatementInvoicePayment grid.</summary>
    InvoicePaymentsGridShowNetProd = 587,

    ///<summary>True if there is a row in the ehrprovkey table.  The OpenDentalService will check this preference and if it is false it will not query
    ///the procedurelog table for scheduled non-CPOE radiology procs.  When the first row is inserted into the ehrprovkey table, or if there is an
    ///existing row when the db is updated, this will be set to true.  Otherwise false.  Users can manually turn this pref on or off.</summary>
    IsAlertRadiologyProcsEnabled = 588,

    /// <summary>Indicates whether or not a clinic has ODTouch enabled. Will be a preference for clinic 0, otherwise a clinicpref.</summary>
    IsODTouchEnabled = 589,

    ///<summary>Enum.  Flags ItransNCpl.ItransUpdateFields: identifies what carrier fields to update when impotring carriers for ITRANS 2.0.</summary>
    ItransImportFields = 590,
    LabelPatientDefaultSheetDefNum = 593,

    ///<summary>Used to determine how many windows are displayed throughout the program, translation, charting, and other features. Version 15.4.1</summary>
    LanguageAndRegion = 594,

    ///<summary>Initially set to Declined to Specify.  Indicates which language from the LanguagesUsedByPatients preference is the language that indicates the patient declined to specify.  Text must exactly match a language in the list of available languages.  Can be blank if the user deletes the language from the list of available languages.</summary>
    LanguagesIndicateNone = 595,

    ///<summary>Comma-delimited list of three-letter language names and custom language names. Example spa for Spanish or fra for French. The custom language names are the full string name and are not necessarily supported by Microsoft. An example value might be "Declined to Specify,spa,fra,Tahitian".</summary>
    LanguagesUsedByPatients = 596,

    ///<summary>A long. The DefNum of the Adjustment Type that is used to create late charges.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    LateChargeAdjustmentType = 597,

    ///<summary>An integer number of days ago that a statement must be sent on or after to have a late charge applied to it.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    LateChargeDateRangeStart = 598,

    ///<summary>An integer number of days ago that a statement must be sent on or before to have a late charge applied to it.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    LateChargeDateRangeEnd = 599,

    ///<summary>A bool. Default setting for excluding guarantors that have not signed a truth in lending agreement from having late charges applied to their family's statements on FormLateCharges.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    LateChargeExcludeAccountNoTil = 600,

    ///<summary>A double. Default setting for excluding accounts below a certain balance from having late charges applied to its statements on FormLateCharges.</summary>
    [PrefName(ValueType = PrefValueType.DOUBLE)]
    LateChargeExcludeBalancesLessThan = 601,

    ///<summary>A bool. Determines whether or not a late charge can be assessed for existing late charge adjustments.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    LateChargeExcludeExistingLateCharges = 602,

    ///<summary>A date. The last date that the Late Charge tool was run on FormLateCharges.</summary>
    [PrefName(ValueType = PrefValueType.DATE)]
    LateChargeLastRunDate = 603,

    ///<summary>A double. The default maximum late charge that can be applied to a statement on FormLateCharges.</summary>
    [PrefName(ValueType = PrefValueType.DOUBLE)]
    LateChargeMax = 604,

    ///<summary>A double. The default minimum late charge that can be applied to a statement on FormLateCharges.</summary>
    [PrefName(ValueType = PrefValueType.DOUBLE)]
    LateChargeMin = 605,

    ///<summary>An integer. The default percentage of a statement's balance to charge as a late fee on FormLateCharges.</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    LateChargePercent = 606,

    ///<summary>A comma delimited list of DefNums. The default billing types that accounts must have for late charges to be applied to statements on the them on FormLateCharges.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    LateChargeDefaultBillingTypes = 607,
    LetterMergePath = 608,

    ///<summary>Boolean. Only used to override server time in the following places: Time Cards.</summary>
    LocalTimeOverridesServerTime = 610,
    MainWindowTitle = 611,

    ///<summary>Enum. Flags MassEmailStatus. Defaults to None. Used for the clinicpref table. When activated and enabled, this practice or 
    ///clinic is enabled to send mass emails.</summary>
    MassEmailStatus = 612,

    ///<summary>String. The Guid for a clinic's mass email account. Plain text.</summary>
    MassEmailGuid = 613,

    ///<summary>String. The secret for a clinic's mass email account. Equivalent to an API key or password. Plain text.</summary>
    MassEmailSecret = 614,

    ///<summary>Number of days after medication order start date until stop date.  Used when automatically inserting a medication order when creating
    ///a new Rx.  Default value is 7 days.  If set to 0 days, the automatic stop date will not be entered.</summary>
    MedDefaultStopDays = 617,

    ///<summary>New procs will use the fee amount tied to the medical code instead of the ADA code.</summary>
    MedicalFeeUsedForNewProcs = 618,

    ///<summary>FK to medication.MedicationNum</summary>
    MedicationsIndicateNone = 619,

    ///<summary>If MedLabReconcileDone=="0", a one time reconciliation of the MedLab HL7 messages is needed. The reconcile will reprocess the original
    ///HL7 messages for any MedLabs with PatNum=0 in order to create the embedded PDF files from the base64 text in the ZEF segments. The old method
    ///of waiting to extract these files until the message is manually attached to a patient was very slow using the middle tier. The new method is to
    ///create the PDF files and save them in the image folder in a subdirectory called "MedLabEmbeddedFiles" if a pat is not located from the details
    ///in the PID segment of the message. Attaching the MedLabs to a patient is now just a matter of moving the files to the patient's image folder.
    ///All files will now be extracted and stored, either in a pat's folder or in the "MedLabEmbeddedFiles" folder, by the HL7 service.</summary>
    MedLabReconcileDone = 620,

    ///<summary>True by default.  Will use the claimsnapshot table for calculating production in the Net Production Detail report if the date range is today's date only.</summary>
    NetProdDetailUseSnapshotToday = 635,

    ///<summary>Boolean. False by default. Set true if notes can only be signed by providers.</summary>
    NotesProviderSignatureOnly = 644,

    ///<summary>This is only true for New York Office of Mental Health.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    OmhNy = 648,

    ///<summary>Mark online payments as processed when an eClipboard/patient portal payment is auto-split.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    OnlinePaymentsMarkAsProcessed = 649,

    ///<summary>User-defined automatic ortho claim procedure.  D8670.auto by default. Can be overridden at the insplan level.</summary>
    OrthoAutoProcCodeNum = 655,

    ///<summary>User-defined comma separated string of procedure codes (D codes) that can be attached to ortho cases as banding procedures</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    OrthoBandingCodes = 656,

    ///<summary>When turned on, ortho case information is shown in the ortho chart.</summary>
    OrthoCaseInfoInOrthoChart = 657,

    ///<summary>This gets turned on to track down bugs in the ortho chart. Saves info to OrthoChartLog table.</summary>
    OrthoChartLoggingOn = 658,

    ///<summary>Determines whether claims with ortho procedures on them will automatically be marked as Ortho claims.</summary>
    OrthoClaimMarkAsOrtho = 659,

    ///<summary>When true, ortho claims' "OrthoDate" will be automatically set to the patient's first ortho procedure when created.</summary>
    OrthoClaimUseDatePlacement = 660,

    ///<summary>User-defined comma separated string of procedure codes (D codes) that can be attached to ortho cases as debond procedures</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    OrthoDebondCodes = 661,

    ///<summary>Defines for when a Debond code is completed, if the date from the completed procedure will overwrite the months treatment for the patient.
    ///This will also cap the Total Tx Time and Months in Treatment fields for the Auto Ortho Grid to not go above Tx Months Total.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    OrthoDebondProcCompletedSetsMonthsTreat = 662,

    ///<summary>Byte, 24 by default.  The default number of months ortho treatments last.  Overridden by patientnote.OrthoMonthsTreat.</summary>
    OrthoDefaultMonthsTreat = 663,

    ///<summary>There is no global pref to turn on and off all ortho features. This pref was hijacked to show Auto Ortho in Account module.</summary>
    OrthoEnabled = 664,

    ///<summary>Boolean. When true, blocks the user from entering payments on claims created by the Auto Ortho Tool.</summary>
    OrthoInsPayConsolidated = 665,

    ///<summary>Comma delimited list of procedure code CodeNum's, not D codes.  These procedures are used as flags in order to determine the Patients' DatePlacement.
    ///DatePlacement is the ProcDate of the first completed procedure that is associated to any of the procedure codes in this list.</summary>
    OrthoPlacementProcsList = 666,

    ///<summary>Show ortho button and tab in Chart module. The button was previously visible to everyone and had no pref.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    OrthoShowInChart = 667,

    ///<summary>User-defined comma separated string of procedure codes (D Codes) that can be attached to ortho cases as visit procedures</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    OrthoVisitCodes = 668,

    ///<summary>Enum:RpOutstandingIns.DateFilterTab. Defaults to DaysOld. Determines which date filter tab to default load in Outstanding Insurance 
    ///Report.</summary>
    OutstandingInsReportDateFilterTab = 669,
    PasswordsMustBeStrong = 670,

    ///<summary>Boolean.  False by default.  When true strong passwords require a special character (Non letter or digit).</summary>
    PasswordsStrongIncludeSpecial = 671,

    ///<summary>Boolean.  False by default.  When true and PasswordsMustBeStrong is also true users without strong passwords will be prompted to change their password at next login.</summary>
    PasswordsWeakChangeToStrong = 672,
    PatientAllSuperFamilySync = 673,

    ///<summary>The way that dates should be formatted when communicating with patients. Defaults to "d" which is equivalent to .ToShortDateString().
    ///User editable.  Whatever value is in this preference is intended to be passed to DateTime.ToString().
    ///Used in eReminders, eConfirms, manual confirmations, ASAP list texting, and other places.</summary>
    PatientCommunicationDateFormat = 674,

    ///<summary>The way that times should be formatted when communicating with patients through eServices. Defaults to "t" which is equivalent to .ToShortTimeString(). User editable. Whatever value is in this preference is intended to be passed to DateTime.ToString().
    PatientCommunicationTimeFormat = 675,
    PatientFormsShowConsent = 677,

    ///<summary>Bool, false by default. Global pref. When true, the current patient and module will be maintained when switching OD users.</summary>
    PatientMaintainedOnUserChange = 678,

    ///<summary>Bool, false by default.  If set to true, the patient table phone number values will be synced to the phonenumber table, and stored in the PhoneNumberDigits column
    ///with non-digit chars stripped out.  The select patient query will use the phonenumber table when searching for a pat by phone number.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PatientPhoneUsePhonenumberTable = 679,

    ///<summary>Free-form 'Body' text of the notification sent by this practice when a new secure EmailMessage is sent to patient.</summary>
    PatientPortalNotifyBody = 682,

    ///<summary>Free-form 'Subject' text of the notification sent by this practice when a new secure EmailMessage is sent to patient.</summary>
    PatientPortalNotifySubject = 683,
    PatientPortalURL = 685,

    ///<summary>Boolean. Defaults to false. When false: User can see patients not in their list of restricted clinics when they select clinics="All"
    ///in FromPatientSelect.cs. When true: Clinics="All" list in FormPatientSelect.cs will only show patients who have had an appointment at 
    ///user's unrestricted clinics or are assigned to one of the user's list of unrestricted clinics.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PatientSelectFilterRestrictedClinics = 686,

    ///<summary>1 by default.  The minimum allowed value is 1, maximum allowed is 10.  The minimum number of characters entered into a textbox in the
    ///patient select window before triggering a search and fill grid.  With every textbox text changed event, compares the textbox text length to
    ///this pref and if the length is less than this pref a timer is started with interval equal to PatientSelectSearchPauseMs.  If the length is >=
    ///this pref, the search and fill grid is triggered without waiting.</summary>
    PatientSelectSearchMinChars = 687,

    ///<summary>1 by default.  The minimum allowed value is 1, maximum allowed is 10,000.  The number of milliseconds to wait after the last character 
    ///is entered into a textbox in the patient select window before triggering a search and fill grid.  Only waits this long if the textbox text 
    ///length is &lt; PatientSelectSearchMinChars.</summary>
    PatientSelectSearchPauseMs = 688,

    ///<summary>Represents a bool with a third state for 'unset'.  Use Yes, No, Unknown enum.  If No, don't automatically
    ///search and fill the grid of the select patient window when all of the textboxes are blank.  If a patient was initially set on load and the user
    ///clears the search fields, the previous search results will remain in the grid.  The grid will be refilled when data is entered.</summary>
    PatientSelectSearchWithEmptyParams = 689,

    ///<summary>Boolean. True by default. When true, automatically 'shows inactive patients' in the patient selection window.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PatientSelectShowInactive = 690,
    PatientSelectUseFNameForPreferred = 691,

    ///<summary>Boolean. This is the default for new computers, otherwise it uses the computerpref PatSelectSearchMode.</summary>
    PatientSelectUsesSearchButton = 692,

    ///<summary>Boolean. False by default. When true, mask patient date of birth in ChartModule, FamilyModule, PatientSelect, PatientEdit</summary>
    ///<summary>1 by default. Set this to 1 to show the GetAll button in the patient select search window.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PatientSelectWindowShowGetAll = 693,
    PatientDOBMasked = 694,

    ///<summary>Boolean. True by default. When true, mask patient social security numbers in FamilyModule, PatientSelect, PatientEdit.</summary>
    PatientSSNMasked = 695,

    ///<summary>Boolean. False by default.  When true and assigning new primary insurance, the associated patients billing type will be inherited from insPlan.BillingType</summary>
    PatInitBillingTypeFromPriInsPlan = 696,

    ///<summary>The subdomain for the payment portal that is the first part of the customer's URL. Example: payportal portion of https://payportal.patientviewer.com/?cid=ABC123XYZ.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    PaymentPortalSubDomain = 697,

    ///<summary>0 by default.1=Prompt users to select payment type when creating new Payments.</summary>
    PaymentsPromptForPayType = 699,

    ///<summary>PayClinicSetting enum. 0 by default. 0=SelectedClinic, 1=PatientDefaultClinic, 2=SelectedExceptHQ</summary>
    PaymentClinicSetting = 701,

    ///<summary>Boolean, true by default. When true, merchant buttons are disabled for an already complete cc payment entry.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PaymentsCompletedDisableMerchantButtons = 702,

    ///<summary>When true, the payment window does not show paysplits by default.</summary>
    PaymentWindowDefaultHideSplits = 703,

    ///<summary>Int.  Represents PayPeriodInterval enum (Weekly, Bi-Weekly, Monthly). </summary>
    PayPeriodIntervalSetting = 704,

    ///<summary>Int.  If set, represents the number of days after the pay period the pay day is.</summary>
    PayPeriodPayAfterNumberOfDays = 705,

    ///<summary>Boolean.  True by default.  If true, pay days will fall before weekends.  If false, pay days will fall after weekends.</summary>
    PayPeriodPayDateBeforeWeekend = 706,

    ///<summary>Boolean.  True by default.  Pay Day cannot fall on weekend if true.</summary>
    PayPeriodPayDateExcludesWeekends = 707,

    ///<summary>Int. If set to 0, it's disabled, but any other number represents a day of the week. 1:Sunday, 2:Monday etc...</summary>
    PayPeriodPayDay = 708,

    /// <summary>Long. Stores the defnum of the neg adjustment type chosen to use for pay plan adjustments default. </summary>
    PayPlanAdjType = 709,

    /// <summary>bool. Set to false by default. If true, the "Due Now" column will be hidden from pay plans grid in acct module.</summary>
    PayPlanHideDueNow = 710,

    ///<summary>Boolean. True by default. If false, allows the payment plan window to not require the full lock box to be checked before saving.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PayPlanRequireLockForAPR = 711,
    PayPlansBillInAdvanceDays = 712,

    [PrefName(ValueType = PrefValueType.BOOL)]
    ///<summary>Boolean. False by default. If true, payment plans will use the date of production as the Date Showing. If false, payment plans use the payplanlink.SecDateTEntry as the date showing. That field is not editable and is the date that the user attached the proc to the pp. In the normal scenario of adding procs at the same time as pp creation, this is effectively the pp date. This also handles the scenario where you have a long-running pp and periodically add procedures to it. If we get complaints, we might add this as a date field that users can control.</summary>
    PayPlanItemDateShowProc = 713,

    ///<summary>Boolean.  False by default.  If true, payment plan window will exclude past activity in the amortization grid by default.</summary>
    PayPlansExcludePastActivity = 714,
    PayPlansUseSheets = 715,

    ///<summary>The Payment Plan version that the customer is using. Derives from PayPlanVersions enum.
    ///Valid values are 1, 2, 3 or 4. 1 is legacy(Do Not Age) 2 is the default(Age Credits and Debits).</summary>
    PayPlansVersion = 716,

    ///<summary>False by default. If true, when a PayPlan is signed from eClipboard a PDF will be generated and saved.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PayPlanSaveSignedToPdf = 717,

    [PrefName(ValueType = PrefValueType.STRING)]
    PayPlanTermsAndConditions = 718,

    ///<summary>Default email message template for Message-to-Pay emails. Not used by BillingL.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    PaymentPortalMsgToPayEmailMessageTemplate = 719,

    ///<summary>Default email subject template for Message-to-Pay emails.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    PaymentPortalMsgToPaySubjectTemplate = 720,

    ///<summary>Default sms message template for Message-to-Pay texts.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    PaymentPortalMsgToPayTextMessageTemplate = 721,

    ///<summary>False by default.  If true, PDF files will not preview with a single click. They will open with double-click. This might help with RDP.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PdfLaunchWindow = 723,
    PerioColorCAL = 724,
    PerioColorFurcations = 725,
    PerioColorFurcationsRed = 726,
    PerioColorGM = 727,
    PerioColorMGJ = 728,
    PerioColorProbing = 729,
    PerioColorProbingRed = 730,

    ///<summary>Contains perio exam measurement values for every tooth. Must be exactly 192 digits or empty (if no default has been set yet).</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    PerioDefaultProbeDepths = 731,

    ///<summary>True by default.  If true, the term "Gingival Margin" will be replaced with the term "Recession" within the Perio Chart in most cases. But we will still leave it as Gingival Margin if any GMs are positive.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    PerioRecessionInsteadOfGM = 732,

    ///<summary>3 by default.  If an auto Att Ging value is <= to this value, it will be shown in red. </summary>
    [PrefName(ValueType = PrefValueType.INT)]
    PerioRedAttGing = 733,
    PerioRedCAL = 734,
    PerioRedFurc = 735,
    PerioRedGing = 736,
    PerioRedMGJ = 737,
    PerioRedMob = 738,
    PerioRedProb = 739,

    ///<summary>Enabled by default.  When a new perio exam is created, will always mark all missing teeth as skipped.</summary>
    PerioSkipMissingTeeth = 740,

    ///<summary>Enabled by default.  When a tooth with an implant procedure completed will not be skipped on perio exams</summary>
    PerioTreatImplantsAsNotMissing = 741,

    ///<summary>Defines the procedure code to use for insplan.PerVisitInsAmount. Empty string by default. The Procedure code is usually set for 100% coverage. When an appt is scheduled or set complete, a new proc gets created with this code.</summary>
    PerVisitInsAmountProcCode = 742,

    ///<summary>Defines the procedure code to use for insplan.PerVisitPatAmount. Empty string by default. The Procedure code is usually set to default to "Do Not Bill to Ins". When an appt is scheduled or set complete, a new proc gets created with this code.</summary>
    PerVisitPatAmountProcCode = 743,

    ///<summary>Can be any int.  Defaults to 0.</summary>
    PlannedApptDaysFuture = 744,

    ///<summary>Can be any int.  Defaults to 365.</summary>
    PlannedApptDaysPast = 745,
    PracticeAddress = 747,
    PracticeAddress2 = 748,
    PracticeBankNumber = 749,
    PracticeBillingAddress = 750,
    PracticeBillingAddress2 = 751,
    PracticeBillingCity = 752,
    PracticeBillingPhone = 753,
    PracticeBillingST = 754,
    PracticeBillingZip = 755,
    PracticeCity = 756,
    PracticeDefaultBillType = 757,
    PracticeDefaultProv = 758,

    ///<summary>In USA and Canada, enforced to be exactly 10 digits or blank.</summary>
    PracticeFax = 759,

    ///<summary>This preference is used to hide/change certain OD features, like hiding the tooth chart and changing 'dentist' to 'provider'.</summary>
    PracticeIsMedicalOnly = 760,
    PracticePayToAddress = 761,
    PracticePayToAddress2 = 762,
    PracticePayToCity = 763,
    PracticePayToPhone = 764,
    PracticePayToST = 765,
    PracticePayToZip = 766,

    ///<summary>In USA and Canada, enforced to be exactly 10 digits or blank.</summary>
    PracticePhone = 767,
    PracticeST = 768,
    PracticeTitle = 769,
    PracticeZip = 770,

    ///<summary>Boolean.  False by default.  If true, checks "Preferred only" in FormReferralSelect.</summary>
    ShowPreferedReferrals = 771, //spelled wrong and sig root "referral" should be first.

    ///<summary>Enum:EmailType 0=Regular 1=Html 2=RawHtml. Used to determine format for email for patient portal web mail messages.</summary>
    PortalWebEmailTemplateType = 772,

    ///<summary>String of TimeSpan format: days.HH:mm:ss. Empty string by default.  Only used in FormPopupEdit. The value is added to the current date in order to suggest a DateTime for Date Disabled. Empty string means no automatic disabling.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    PopupsDisableTimeSpan = 773,

    ///<summary>YN_DEFAULT_FALSE. Indicates whether prepayments are allowed on TP procedures.</summary>
    [PrefName(ValueType = PrefValueType.YN_DEFAULT_FALSE)]
    PrePayAllowedForTpProcs = 776,

    ///<summary>FK to definition.DefNum for PaySplitUnearnedType defcat (29). Used to set paysplit.UnearnedType.</summary>
    PrepaymentUnearnedType = 777,

    ///<summary>Prints statements alphabetically by patients last name then first name within the billing window. If false, it will print each
    ///clinic together and then within each clinic, it will be alphabetized.</summary>
    PrintStatementsAlphabetically = 778,

    ///<summary>In Patient Edit and Add Family windows, the Primary Provider defaults to 'Select Provider' instead of the practice provider.</summary>
    PriProvDefaultToSelectProv = 779,

    ///<summary>FK to diseasedef.DiseaseDefNum</summary>
    ProblemsIndicateNone = 780,

    ///<summary>Determines default sort order of Proc Codes list when accessed from Lists -> Procedure Codes.  Enum:ProcCodeListSort, 0 by default.</summary>
    ProcCodeListSortOrder = 782,

    ///<summary>In FormProcCodes, this is the default for the ShowHidden checkbox.</summary>
    ProcCodeListShowHidden = 783,

    ///<summary>Users must use suggested auto codes for a procedure.</summary>
    ProcEditRequireAutoCodes = 784,

    ///<summary>Determines if and how we want to update a procedures ProcFee when changing providers.  
    ///0 - No prompt, don't change fee (default), 
    ///1 - No prompt, always change fee, 
    ///2 - Prompt - When patient portion changes, 
    ///3 - Prompt - Always
    ///</summary>
    ProcFeeUpdatePrompt = 785,
    ProcLockingIsAllowed = 786,

    ///<summary>Bool.  Defaults to false.  Custom feature that a customer paid for to merge the current and last procedure note together.
    ///The merging of the current and last procedure note will only happen when a concurrency issue has been identified.</summary>
    ProcNoteConcurrencyMerge = 787,

    ///<summary>Boolean, false by default. When true, users will be blocked from adding a signature to a procedure note or group note if there are incomplete autonotes. They will also be blocked from skipping/removing autonote prompts.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ProcNoteSigsBlockedAutoNoteIncomplete = 788,

    ///<summary>True by default.  Allows for substituting AutoNote text for [[text]] segments in a procedure's default note.</summary>
    ProcPromptForAutoNote = 789,

    ///<summary>If this is on, the claimproc's provider will inherit the provider on the procedure.
    ///If this is off AND the claimproc is attached to a claim, then the claimproc's provider and the procedure's provider are saved separately.
    ///Most users will keep this on so their providers stay in sync.  
    ///Pref created for customers who manually change claimproc Prov so they can have income attributed for specific prov for financial reasons.</summary>
    ProcProvChangesClaimProcWithClaim = 790,

    ///<summary>Frequency at which signals are processed. Also used by HQ to determine triage label refresh frequency.</summary>		
    ProcessSigsIntervalInSecs = 791,
    ProcGroupNoteDoesAggregate = 792,

    ///<summary>Hidden preference, no UI to edit this list, but it is present in all databases.  Comma delimited list of ProgramNames for programlinks that are disabled in Web
    ///mode for various reasons.  This can also contain string names for other disabled bridges, for example NewCrop.  We can manually edit this list.</summary>
    ProgramLinksDisabledForWeb = 794,

    ///<summary>Example format: "24.3.16.0"</summary>
    ProgramVersion = 796,

    ///<summary>Boolean, true by default.  When true, checks for "Unsent" or "Hold for Primary" secondary claims and automates the claim's status.</summary>
    PromptForSecondaryClaim = 797,
    ProviderIncomeTransferShows = 798,

    ///<summary>Bool.  Defaults to true.  When true, allow the Provider Payroll report to select Today's date in the date range.</summary>		
    ProviderPayrollAllowToday = 799,

    ///<summary>Comma-delimited list of strings.  Each entry represents a QuickBooks "class" which is used to separate deposits (typically by clinics).
    ///Empty by default.</summary>
    QuickBooksClassRefs = 802,

    ///<summary>Boolean, off by default.  Some users have clinics enabled but do not want QuickBooks to itemize their accounts.
    ///Class Refs are a way for QuickBooks to itemize if set up correctly.</summary>
    QuickBooksClassRefsEnabled = 803,
    QuickBooksCompanyFile = 804,

    ///<summary>Comma-delimited list of strings.  Each entry represents a QuickBooks deposit account.</summary>
    QuickBooksDepositAccounts = 805,

    ///<summary>Comma-delimited list of strings.  Each entry represents a QuickBooks income account.</summary>
    QuickBooksIncomeAccount = 806,

    ///<summary>Integer. Indicated by number of days between contact attempts.</summary>
    [PrefName(ValueType = PrefValueType.LONG_NEG_ONE_AS_ZERO)]
    ReactivationContactInterval = 809,

    ///<summary> long. -1=infinite, 0=zero; if stored as -1, displays as "".</summary>
    [PrefName(ValueType = PrefValueType.LONG_NEG_ONE_AS_BLANK)]
    ReactivationCountContactMax = 810,

    ///<summary>Defaults to 1095.  -1 indicates min for all dates</summary>
    [PrefName(ValueType = PrefValueType.LONG_NEG_ONE_AS_BLANK)]
    ReactivationDaysPast = 811,

    [PrefName(ValueType = PrefValueType.STRING)]
    ReactivationEmailFamMsg = 812,

    [PrefName(ValueType = PrefValueType.STRING)]
    ReactivationEmailMessage = 813,

    [PrefName(ValueType = PrefValueType.STRING)]
    ReactivationEmailSubject = 814,

    ///<summary>Boolean. False by default.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ReactivationGroupByFamily = 815,

    [PrefName(ValueType = PrefValueType.STRING)]
    ReactivationPostcardFamMsg = 816,

    [PrefName(ValueType = PrefValueType.STRING)]
    ReactivationPostcardMessage = 817,

    ///<summary>Integer.  3 by default.</summary>
    [PrefName(ValueType = PrefValueType.LONG_NEG_ONE_AS_BLANK)]
    ReactivationPostcardsPerSheet = 818,

    ///<summary>FK to definition.DefNum. Uses the existing RecallUnschedStatus DefCat.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    ReactivationStatusEmailed = 819,

    ///<summary>FK to definition.DefNum. Uses the existing RecallUnschedStatus DefCat.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    ReactivationStatusMailed = 821,

    ///<summary>When using a distinct read only server, stores the server name.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ReadOnlyServerCompName = 823,

    ///<summary>When using a distinct read only server, stores the database name.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ReadOnlyServerDbName = 824,

    ///<summary>String which stores SSL certificate for SkySQL.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ReadOnlyServerSslCa = 825,

    ///<summary>When using a distinct read only server, stores the mysql username.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ReadOnlyServerMySqlUser = 826,

    ///<summary>When using a distinct read only server, stores the hashed mysql password.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ReadOnlyServerMySqlPassHash = 827,

    ///<summary>When using a distinct read only server over middle tier, stores the uri.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ReadOnlyServerURI = 828,
    RecallAdjustDown = 829,
    RecallAdjustRight = 830,

    ///<summary>Defaults to 12 for new customers.  The number in this field is considered adult.  Only used when automatically adding procedures to a new recall appointment.</summary>
    RecallAgeAdult = 831,
    RecallCardsShowReturnAdd = 832,

    ///<summary>-1 indicates min for all dates</summary>
    RecallDaysFuture = 833,

    ///<summary>-1 indicates min for all dates</summary>
    RecallDaysPast = 834,
    RecallEmailFamMsg = 835,
    RecallEmailFamMsg2 = 836,
    RecallEmailFamMsg3 = 837,
    RecallEmailMessage = 838,
    RecallEmailMessage2 = 839,
    RecallEmailMessage3 = 840,
    RecallEmailSubject = 841,
    RecallEmailSubject2 = 842,
    RecallEmailSubject3 = 843,
    RecallExcludeIfAnyFutureAppt = 844,

    ///<summary>Boolean.</summary>
    RecallGroupByFamily = 845,

    ///<summary>long. -1=infinite, 0=zero; if stored as -1, displays as ""</summary>
    RecallMaxNumberAutoReminders = 846,

    ///<summary>long. -1=infinite, 0=zero; if stored as -1, displays as "".</summary>
    RecallMaxNumberReminders = 847,
    RecallPostcardFamMsg = 848,
    RecallPostcardFamMsg2 = 849,
    RecallPostcardFamMsg3 = 850,
    RecallPostcardMessage = 851,
    RecallPostcardMessage2 = 852,
    RecallPostcardMessage3 = 853,
    RecallPostcardsPerSheet = 854,

    ///<summary>Integer, 15 by default - Days since the first reminder was sent out before we try to send another one out. A value of -1 means disabled and a value
    ///of 0 is show all for the recall list. WebSchedRecalls will be prevented from sending if the value is not set to an integer greater than 0.</summary>
    RecallShowIfDaysFirstReminder = 855,

    ///<summary>Integer, 30 by default - Days since the second reminder (or more) was sent out before we try to send another one out. A value of -1 means disabled
    ///and a value of 0 is show all for the recall list. WebSchedRecalls will be prevented from sending if the value is not set to an integer greater than 0.</summary>
    RecallShowIfDaysSecondReminder = 856,
    RecallStatusEmailed = 857,
    RecallStatusEmailedTexted = 858,
    RecallStatusMailed = 859,
    RecallStatusTexted = 860,

    ///<summary>Used if younger than 12 on the recall date.</summary>
    RecallTypeSpecialChildProphy = 861,
    RecallTypeSpecialPerio = 862,
    RecallTypeSpecialProphy = 863,

    ///<summary>Comma-delimited list. FK to recalltype.RecallTypeNum.</summary>
    RecallTypesShowingInList = 864,

    ///<summary>If false, then it will only use email in the recall list if email is the preferred recall method.</summary>
    RecallUseEmailIfHasEmailAddress = 865,

    ///<summary>Boolean, true by default. Allows recurring charges even when the Patient balance is 0 or less on the account.</summary>
    RecurringChargesAllowedWhenNoPatBal = 866,

    ///<summary>Bool, false by default. If true, then OpenDentalService will run recurring charges on a set schedule.</summary>
    RecurringChargesAutomatedEnabled = 868,

    ///<summary>Stored as a DateTime, but only the time portion is used. This time will be when recurring charges are automatically run.</summary>
    RecurringChargesAutomatedTime = 869,

    ///<summary>Stored as DateTime, but cleared when recurring charges tool finishes.  The DateTime will be used as a flag to signal other connections
    ///that recurring charges have started and prevents OpenDentalService from running repeating charges.</summary>
    RecurringChargesBeginDateTime = 870,

    ///<summary>Boolean, true by default. Automatically deactivates recurring charge when a credit card is declined so that office does not waste money running them again.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    RecurringChargesInactivateDeclinedCards = 871,

    ///<summary>Stored as a long, defaults to 0. Default recurring payment charge type. When the value is 0, it will use the program property payment
    ///type. Does not apply to ACH payments.</summary>
    RecurringChargesPayTypeCC = 872,

    ///<summary>Defaults to false. Set the default state of checkShowInactive in FormCreditRecurringCharges.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    RecurringChargesShowInactive = 873,

    ///<summary>Bool, 0 by default.  When true, recurring charges will use the primary provider of the patient when creating paysplits.
    ///When false, the provider that the family is most in debt to will be used.</summary>
    RecurringChargesUsePriProv = 874,

    ///<summary>Bool, 0 by default.  When true, uses the transaction date for the recurring charge payment date.
    ///When false, the recurring charge date will be used as the recurring charge payment date.</summary>
    RecurringChargesUseTransDate = 875,

    ///<summary>Comma delimited list of redirect short urls that need to be excluded when users are sending out email communications to patients.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    RedirectShortURLsFromHQ = 876,

    ///<summary>FK to definition.DefNum, defaults to 0. Determines the adjustment type to be used for refund adjustments by default.</summary>
    [PrefName(ValueType = PrefValueType.LONG)]
    RefundAdjustmentType = 877,

    ///<summary>16 char alphanumeric, generated by OD at time of sale.  Applies to single office.</summary>
    RegistrationKey = 878,

    ///<summary>Boolean set to true by OD staff in rare situations.  For example, issued a replacement key, so old key disabled.  Not disabled when someone goes off support.  We use pref.HelpKey for that.</summary>
    RegistrationKeyIsDisabled = 879,

    ///<summary>Bool, false by default.  When true, the repeating charges tool will run automatically on a daily basis.</summary>
    RepeatingChargesAutomated = 882,

    ///<summary>Time, 08:00 am by default.  Defines the time of day that the repeating charges tool will run if set to run automatically.</summary>
    RepeatingChargesAutomatedTime = 883,

    ///<summary>Stored as DateTime, but cleared when repeating charges tool finishes.  The DateTime will be used as a flag to signal other connections
    ///that repeating charges have started and prevents another connection from running simultaneously. In order to run repeating charges, 
    ///this will have to be cleared, either by the connection that set the flag when repeating charges finishes or by an update query.</summary>
    RepeatingChargesBeginDateTime = 884,

    ///<summary>DateTime, the last date and time repeating charges was run.</summary>
    RepeatingChargesLastDateTime = 885,

    ///<summary>Bool, true by default.  When true, the repeating charges tool will run aging after posting charges.</summary>
    RepeatingChargesRunAging = 886,

    ///<summary>Bool, 0 by default. When on, the user will be prompted to overwrite existing blockouts when making a new blockout that overlaps the existing ones.</summary>
    ReplaceExistingBlockout = 887,

    ///<summary>When using a distinct reporting server, stores the server name.</summary>
    ReportingServerCompName = 891,

    ///<summary>When using a distinct reporting server, stores the database name.</summary>
    ReportingServerDbName = 892,

    ///<summary>When using a distinct reporting server, stores the mysql username.</summary>
    ReportingServerMySqlUser = 893,

    ///<summary>When using a distinct reporting server, stores the hashed mysql password.</summary>
    ReportingServerMySqlPassHash = 894,

    /// <summary>Used for connecting to Maria DB SkySQL.</summary>
    ReportingServerSslCa = 895,

    ///<summary>When using a distinct reporting server over middle tier, stores the uri.</summary>
    ReportingServerURI = 896,

    ///<summary>Boolean, on by default.</summary>
    ReportPandIhasClinicBreakdown = 897,

    ///<summary>Boolean, off by default.</summary>
    ReportPandIhasClinicInfo = 898,
    ReportPandIschedProdSubtractsWO = 899,

    ///<summary>Boolean, false by default. Used to allow a user to display hidden treatment planned prepayments in the payment report.</summary>
    ReportsDoShowHiddenTPPrepayments = 900,

    ///<summary>Comma delimited list of procedure codes (D codes) that will be excluded from the Incomplete Procedure Notes report.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    ReportsIncompleteProcsExcludeCodes = 901,

    ///<summary>Bool.  False by defualt, used to filter incomplete procedures by having no note in the Incomplete Procedures Report.</summary>
    ReportsIncompleteProcsNoNotes = 902,

    ///<summary>Bool.  False by defualt, used to filter incomplete procedures by having a note that is unsigned in the Incomplete Procedures Report.</summary>
    ReportsIncompleteProcsUnsigned = 903,

    /// <summary>Tri-state enumeration. 0 by default.  Determines which date is used when calculating certain reports.
    /// 0=Insurance payment date. 1=Procedure Date. 2=Claim Date/Payment Date.</summary>
    ReportsPPOwriteoffDefaultToProcDate = 904,

    ///<summary>Bool.  False by defualt, used to wrap columns when printing a custom report.</summary>
    ReportsWrapColumns = 905,

    ///<summary>Bool.  False by defualt, used to determine whether the reports progress bar will show a history or not.</summary>
    ReportsShowHistory = 906,
    ReportsShowPatNum = 907,

    ///<summary>Tri-state enumeration. 1 by default. 0=Fully Enforced. 1=Auto-split but don't enforce rigorous accounting. 2=Don't auto-split and don't enforce.</summary>
    RigorousAccounting = 909,

    ///<summary>Tri-state enumeration. 1 by default. 0=Fully Enforced. 1=Auto-link but don't enforce. 2=Don't auto-link and don't enforce.</summary>
    RigorousAdjustments = 910,

    ///<summary>Defaults to false.  When true, will require procedure code to be attached to controlled prescriptions.</summary>
    RxHasProc = 911,

    /// <summary>Defaults to false. When true will remove providers with no DEA number from list of providers to pick from when printing Rx sheets.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    RxHideProvsWithoutDEA = 912,

    ///<summary>FK to definition.DefNum.  Represents default adjustment types for sales tax adjustments.</summary>
    SalesTaxAdjustmentType = 914,

    ///<summary>FK to provider.ProvNum. 0 by default. Determines the global default provider to use when calculating sales tax.</summary>
    SalesTaxDefaultProvider = 915,

    ///<summary>Boolean. False by default. Determines whether sales tax is automatically applied.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    SalesTaxDoAutomate = 916,
    SalesTaxPercentage = 917,

    ///<summary>Procedure Code string for the procedure we want to represent Sales Tax.</summary>
    SalesTaxProcCode = 918,

    ///<summary>In Patient Edit window, no more smart checking for the 4 Same for Entire Family checkboxes.  Just always unchecked.  User can still check.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    SameForFamilyCheckboxesUnchecked = 919,

    ///<summary>Boolean. 1 by default.  Allows users to choose if they want to save a copy of any attachments they send to DXC.</summary>
    SaveDXCAttachments = 920,

    ///<summary>Boolean. 0 by default.  Allows users to choose if they want to save a copy of any attachments they send to DXC.</summary>
    SaveDXCSOAPAsXML = 921,

    ///<summary>Boolean. 1 by default. Allows users to choose if they want to save a copy of any attachments they send to EDS.</summary>
    SaveEDSAttachments = 922,

    ///<summary>Set to 1 by default. Selects all providers/employees when loading schedules.</summary>
    ScheduleProvEmpSelectAll = 926,
    ScheduleProvUnassigned = 927,

    ///<summary>Boolean. Off by default so that users will have to opt into utilizing the screening with sheets feature.
    ///Screening with sheets is extremely customized for Dental3 (D3)</summary>
    ScreeningsUseSheets = 928,

    ///<summary>Default true. When logging in with a badge, require a password. If false, then swiping the badge gets you directly in without further action. This is lower security, but some offices might like it.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    SecurityBadgesRequirePassword = 929,

    ///<summary>Aka Global Lock Date.</summary>
    SecurityLockDate = 932,

    ///<summary>Set to 0 to always grant permission. 1 means only today.</summary>
    SecurityLockDays = 933,
    SecurityLockIncludesAdmin = 934,

    ///<summary>Set to 0 to disable auto logoff.</summary>
    SecurityLogOffAfterMinutes = 935,

    ///<summary>Boolean.  False by default.  Allows users to set their own automatic logoff times.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    SecurityLogOffAllowUserOverride = 936,
    SecurityLogOffWithWindows = 937,

    ///<summary>Bool.  True by default.  When enabled and user is on support and on the most recent stable or on any beta version a BugSubmissions will be reported to HQ.</summary>
    SendUnhandledExceptionsToHQ = 939,

    ///<summary>FK to sheetdef.SheetDefNum.  The default chart module layout sheetdef.</summary>
    SheetsDefaultChartModule = 945,

    ///<summary>The DefNum for the default sheet def to use for Consent sheets</summary>
    SheetsDefaultConsent = 946,

    ///<summary>The DefNum for the default sheet def to use for Invoice statement sheets</summary>
    SheetsDefaultInvoice = 949,

    ///<summary>The DefNum for the default sheet def to use for Limited statement sheets</summary>
    SheetsDefaultLimited = 955,

    ///<summary>The DefNum for the default sheet def to use for Receipt statement sheets</summary>
    SheetsDefaultReceipt = 961,

    ///<summary>The DefNum for the default sheet def to use for Statement sheets</summary>
    SheetsDefaultStatement = 969,

    ///<summary>The DefNum for the default sheet def to use for TreatmentPlan sheets</summary>
    SheetsDefaultTreatmentPlan = 970,

    /// <summary>Set to 0 by default.  ClinicPref, indicates which ApptReminderTypes should use Short Codes.</summary>
    ShortCodeApptReminderTypes = 971,

    ///<summary>ClinicPref.  Individual clinics can specify a Clinic Title to use in the Short Code Opt In Reply, substitutes [YourDentist]
    ///"You'll now receive appointment messages from [YourDentist] Reply HELP for Help, Reply STOP to cancel. Msg&data rates may apply."</summary>
    ShortCodeOptInClinicTitle = 972,

    ///<summary>Set to true by default.  Allows the office to turn off the automated Short Code script window when completing an appointment.
    ///</summary>
    ShortCodeOptInOnApptComplete = 973,

    ///<summary>A script for the dentist to read to a patient for opting in to receive appointment reminders via Short Code.</summary>
    ShortCodeOptInScript = 975,

    ///<summary>A script for the dentist to read to a patient who has previously opted out of receiving apptointment reminders via Short Code.
    ///</summary>
    ShortCodeOptedOutScript = 976,
    ShowAccountFamilyCommEntries = 977,

    ///<summary>Set to 1 by default.  Prompts user to allocate unearned income after creating a claim.</summary>
    ShowAllocateUnearnedPaymentPrompt = 978,

    ///<summary>Set to 0 by default. Preference that controls if the auto deposit group box shows or not in FormClaimPayEdit.cs</summary>
    ShowAutoDeposit = 979,

    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowFeatureEhr = 980,

    ///<summary>Set to 0 by default. Controls if the Enterprise Setup Window will be available.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowFeatureEnterprise = 981,

    ///<summary>Set to 1 by default.  Shows a button in Edit Patient Information that lets users launch Google Maps.</summary>
    ShowFeatureGoogleMaps = 982,

    ///<summary>Set to 0 by default. When 0, the Late Charges tool is hidden, and the Billing/Finance Charges tool is accessable. When 1 the Late Charges tool is accessable, and the Billing/Finance Charges tool is hidden. </summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowFeatureLateCharges = 983,

    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowFeatureMedicalInsurance = 984,

    ///<summary>Set to 1 to enable the Synch Clone button in the Family module which allows users to create and synch clones of patients.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowFeaturePatientClone = 985,

    ///<summary>Set to 1 to enable the Reactivations tab in the Recall list.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowFeatureReactivations = 986,

    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowFeatureSuperfamilies = 987,

    ///<summary>Set to 0 by default.  For enterprise users.  When enabled users can setup Fee Schedule Groups so that whenever a Fee Schedule in a clinic is edited 
    ///it will automatically update that fee schedule for the other clinics in that group.</summary>
    ShowFeeSchedGroups = 988,

    ///<summary>0=None, 1=PatNum, 2=ChartNumber, 3=Birthdate</summary>
    ShowIDinTitleBar = 989,

    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowIncomeTransferManager = 990,

    ///<summary>Boolean.  True by default. If true then the user might be prompted to create a planned appointment when leaving the Chart Module.</summary>
    ShowPlannedAppointmentPrompt = 991,

    ///<summary>Boolean.	True by default. If true then users are able to show pronouns that do not necessarily match gender.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ShowPreferredPronounsForPats = 992,

    ///<summary>If enabled, allow Providers to digitally sign procedures and proc notes.</summary>
    SignatureAllowDigital = 996,

    ///<summary>Used to stop signals after a period of inactivity.  A value of 0 disables this feature.  Default value of 0 to maintain backward compatibility</summary>
    SignalInactiveMinutes = 997,

    ///<summary>Only used on startup.  The date in which stale signalods were removed.</summary>
    SignalLastClearedDate = 998,

    ///<summary>Blank if not signed. Date signed. For practice level contract, if using clinics see Clinic.SmsContractDate. Record of signing also kept at HQ.</summary>
    SmsContractDate = 999,

    /// <summary>Name of this Software.  Defaults to 'Open Dental Software'.</summary>
    SoftwareName = 1002,
    SolidBlockouts = 1003,
    SpellCheckIsEnabled = 1004,
    StatementAccountsUseChartNumber = 1005,
    StatementsCalcDueDate = 1006,

    ///<summary>Show payment notes.</summary>
    StatementShowNotes = 1008,
    StatementShowAdjNotes = 1009,
    StatementShowProcBreakdown = 1010,
    StatementShowReturnAddress = 1011,

    ///<summary>Deprecated. We no longer allow storing of credit card numbers.</summary>
    StoreCCnumbers = 1015,
    StoreCCtokens = 1016,
    SubscriberAllowChangeAlways = 1017,
    SuperFamSortStrategy = 1018,
    SuperFamNewPatAddIns = 1019,

    ///<summary>Defaults to true, unless BackupReminderLastDateRun is disabled (more than a decade into the future).
    ///When true, supplemental backups will be executed from the eConnector and copied to HQ as a last resort recovery solution.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    SupplementalBackupEnabled = 1020,

    ///<summary>Defaults to MinVal (0001-01-01 00:00:00).  Last date and time supplemental backup was successful.</summary>
    [PrefName(ValueType = PrefValueType.DATETIME)]
    SupplementalBackupDateLastComplete = 1021,

    ///<summary>Blank by default.	 Designed to be set to a network path (UNC) to another computer on the network.
    ///A local copy of the supplemental backup file will be placed here as a secondary measure for last resort recovery.
    ///A copy of the supplemental backup will be placed here before uploading to HQ.
    ///This way customers with databases larger than 1GB can make use of our backup system.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    SupplementalBackupNetworkPath = 1022,
    TaskAncestorsAllSetInVersion55 = 1025,

    ///<summary>Stores the image category that task attachment documents will be saved to. If 0, user will be unable to add or edit task attachments, just view existing attachments. 0 by default</summary>
    TaskAttachmentCategory = 1026,
    TaskListAlwaysShowsAtBottom = 1027,

    /// <summary> Boolean. True by default. If false, prevents users from attaching multiple tasks to an appointment.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    TasksForApptAllowMultiple = 1029,

    ///<summary>Determines how Tasks will be filtered from root node positions in UserControlTasks, i.e. when viewing a tab without having selected a TaskList.</summary>
    TasksGlobalFilterType = 1030,

    ///<summary>If true use task.Status to determine if task is new. Otherwise use task.IsUnread.</summary>
    TasksNewTrackedByUser = 1031,
    TasksShowOpenTickets = 1032,

    ///<summary>Boolean.  0 by default.  Sets appointment task lists to use special logic to sort by AptDateTime.</summary>
    TaskSortApptDateTime = 1033,

    ///<summary>Boolean.  Defaults to false to hide repeating tasks feature if no repeating tasks are in use when updating to 16.3.</summary>
    TasksUseRepeating = 1034,

    ///<summary>Keeps track of date of one-time cleanup of temp files.  Prevents continued annoying cleanups after the first month.</summary>
    TempFolderDateFirstCleaned = 1035,
    TerminalClosePassword = 1036,

    ///<summary>If true, treat Yes-No-Unknown status of Unknown as if it were a No.</summary>
    TextMsgOkStatusTreatAsNo = 1037,

    ///<summary>If true, then this prompts the office to send an opt out notification text to the patient when changing a patient's 'Text OK' status from 'Yes' (not from unknown) to 'No'. Default is false to not prompt or send any text.</summary>
    TextOptOutSendNotification = 1038,
    TextingDefaultClinicNum = 1039,

    ///<summary>Boolean, default is false. Only used by the clinicpref table. If true, the practice level ApptThankYouCalendarTitle will be displayed.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    ThankYouTitleUseDefault = 1042,
    TimeCardADPExportIncludesName = 1044,

    ///<summary>0=Sun,1=Mon...6=Sat</summary>
    TimeCardOvertimeFirstDayOfWeek = 1045,
    TimecardSecurityEnabled = 1046,

    ///<summary>Boolean.  0 by default.  When enabled, FormTimeCard and FormTimeCardMange display H:mm:ss instead of HH:mm</summary>
    TimeCardShowSeconds = 1047,
    TimeCardsMakesAdjustmentsForOverBreaks = 1048,

    ///<summary>bool</summary>
    TimeCardsUseDecimalInsteadOfColon = 1049,

    ///<summary>Users can not edit their own time card for past pay periods.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    TimecardUsersCantEditPastPayPeriods = 1050,
    TimecardUsersDontEditOwnCard = 1051,

    ///<summary>Boolean, false by default. When enabled, main title of FormOpenDental uses clinic abbr instead of description</summary>
    TitleBarClinicUseAbbr = 1052,
    TitleBarShowSite = 1053,

    /// <summary>Boolean. 0 by default.  When enabled, Main title bar and Account module patient select grid will display the Patient's 
    /// specialty.</summary>
    TitleBarShowSpecialty = 1054,

    ///<summary>Prepayments for TP procedures are non-refundable. When appointment is broken that has one of these payments, payment will go to the 
    ///broken appointment procedure instead of remaining on the TP procedures.</summary>
    TpPrePayIsNonRefundable = 1057,

    ///<summary>FK to definition.DefNum. The default unearned type that will be used for prepayments attached to tp procedures.</summary>
    TpUnearnedType = 1058,

    ///<summary>The date and time when the OpenDentalService last sent update messages for account debits and credits.</summary>
    TransworldDateTimeLastUpdated = 1059,

    ///<summary>Determines how often account activity is sent to Transworld.  Default is once per day at the time of day set in the
    ///TransworldServiceTimeDue pref.  User can adjust this to be more or less frequent.</summary>
    TransworldServiceSendFrequency = 1060,

    ///<summary>The time of day for the OpenDentalService to update Transoworld (TSI) with all payments and other debits and credits for families
    ///where the guarantor has been sent to TSI for collection.</summary>
    TransworldServiceTimeDue = 1061,

    ///<summary>FK to definition.DefNum.  Billing type the OpenDentalService will change guarantors to once an account is paid in full.</summary>
    TransworldPaidInFullBillingType = 1062,

    ///<summary>Boolean,  true by default. When enabled, all procedures considered in the treatment finder report will count towards general benefits.</summary>
    TreatFinderProcsAllGeneral = 1063,
    TreatmentPlanNote = 1064,
    TreatPlanDiscountAdjustmentType = 1065,

    ///<summary>Set to 0 to clear out previous discounts.</summary>
    TreatPlanDiscountPercent = 1066,
    TreatPlanItemized = 1067,

    ///<summary>True by default. Prompt user with name suggestion when saving a treatment plan.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    TreatPlanPromptSave = 1069,

    ///<summary>When a TP is signed a PDF will be generated and saved. If disabled, TPs will be redrawn with current data (pre 15.4 behavior).</summary>
    TreatPlanSaveSignedToPdf = 1070,
    TreatPlanShowCompleted = 1071,

    ///<summary>This preference merely defines what FormOpenDental.IsTreatPlanSortByTooth is on startup.
    ///When true, procedures in the treatment plan module sort by priority, date, toothnum, surface, then PK. 
    ///When false, does not sort by toothnum or surface. True by default to preserve old behavior.</summary>
    TreatPlanSortByTooth = 1074,

    ///<summary>Can be any int.  Defaults to 0.</summary>
    UnschedDaysFuture = 1086,

    ///<summary>Can be any int.  Defaults to 365.</summary>
    UnschedDaysPast = 1087,

    ///<summary>Bool, true by default, prevents recall appointments from being sent to the Unscheduled List.</summary>
    UnscheduledListNoRecalls = 1088,

    ///<summary>Hidden preference, no UI to enable this and is missing in most databases. Use GetStringNoCache() to get the value of this preference.
    ///If this preference exists and is set to 1, altering large tables will not use the large table helper method of creating a copy of the table
    ///structure, altering the empty copy, and then inserting all of the rows from the original table.  Instead, the alter table method will be run
    ///directly on the large table.</summary>
    UpdateAlterLargeTablesDirectly = 1089,

    ///<summary>Described in the Update Setup window and in the manual.  Can contain multiple db names separated by commas.  Should not include current db name.</summary>
    UpdateMultipleDatabases = 1092,
    UpdateServerAddress = 1093,
    UpdateShowMsiButtons = 1094,

    ///<summary>The next update date and time, set in FormUpdateSetup.  When this is set in the future, the main form's title bar will count down to the set time.</summary>
    UpdateDateTime = 1095,

    ///<summary>Use GetStringNoCache() to get the value of this preference.</summary>
    UpdateStreamLinePassword = 1096,
    UpdateWebProxyAddress = 1097,
    UpdateWebProxyPassword = 1098,
    UpdateWebProxyUserName = 1099,
    UpdateWebsitePath = 1100,

    ///<summary>Hidden preference, no UI to enable this feature, but is present in all databases.  Boolean.
    ///Set to true to make file selection windows use the flag ".ShowHelp=true;".
    ///Added as attemped fix to stop lockups when importing images.</summary>
    UseAlternateOpenFileDialogWindow = 1102,
    UseBillingAddressOnClaims = 1103,

    ///<summary>Enum:ToothNumberingNomenclature 0=Universal(American), 1=FDI, 2=Haderup, 3=Palmer</summary>
    UseInternationalToothNumbers = 1104,

    ///<summary>Boolean. 0 by default. When enabled, chart module procedures that are complete will use the provider's color as row's background color</summary>
    UseProviderColorsInChart = 1105,

    ///<summary>Boolean.  0 by default.  When enabled, users must enter their user name manually at the log on window.</summary>
    UserNameManualEntry = 1106,

    ///<summary>Boolean, 0 by default. When enabled, the User Query window will load with the 'Raw' format radio button pre-selected. Otherwise it will load with the 'Human-readable' format radio button pre-selected. This also determines how new userquery.DefaultFormatRaw will be set.</summary>
    [PrefName(ValueType = PrefValueType.BOOL)]
    UserQueryDefaultRaw = 1107,

    ///<summary>FK to DefNum of image category used when capturing a video image.</summary>
    VideoImageCategoryDefault = 1108,
    WaitingRoomAlertColor = 1119,

    ///<summary>0 to disable.  When enabled, sets rows to alert color based on wait time.</summary>
    WaitingRoomAlertTime = 1120,

    ///<summary>Boolean.  0 by default.  When enabled, the waiting room will filter itself by the selected appointment view.  0, normal filtering, will show all patients waiting for the entire practice (or entire clinic when using clinics).</summary>
    WaitingRoomFilterByView = 1121,

    ///<summary>The clinic GUID for the clinic in question, stored as ClinicPref. Stored at HQ in EserviceClinic.ClinicGuid.  All clinics (including 0) will have a ClinicGuid. Used for idenitification when creating the specific clinic URL. Example: ABC123XYZ portion of https://payportal.patientviewer.com/?cid=ABC123XYZ.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    WebAppClinicGuid = 1122,

    ///<summary>The web domain for web apps that is the second part of the customer's URL. Example: patientviewer.com portion of https://payportal.patientviewer.com/?cid=ABC123XYZ.</summary>
    [PrefName(ValueType = PrefValueType.STRING)]
    WebAppDomain = 1123,

    ///<summary>The template that will be used for Web Sched automation when a reminder for multiple recalls is sent to the same phone number.
    ///</summary>
    WebSchedAggregatedTextMessage = 1129,

    ///<summary>The template that will be used for Web Sched automation when a reminder for multiple recalls is sent to the same email.</summary>
    WebSchedAggregatedEmailBody = 1130,

    ///<summary>The template that will be used for Web Sched automation when a reminder for multiple recalls is sent to the same email.</summary>
    WebSchedAggregatedEmailSubject = 1131,

    ///<summary>The subject line used for Web Sched ASAP emails.</summary>
    WebSchedAsapEmailSubj = 1132,

    ///<summary>The template used for Web Sched ASAP email bodies.</summary>
    WebSchedAsapEmailTemplate = 1133,

    ///<summary>Enum:EmailType 0=Regular 1=Html 2=RawHtml. Used to determine format for email for web sched asap messages</summary>
    WebSchedAsapEmailTemplateType = 1134,

    ///<summary>The maximum number of texts allowed to be sent to a patient in a day. Blank means no limit.</summary>
    WebSchedAsapTextLimit = 1139,

    ///<summary>The template used for Web Sched ASAP texts.</summary>
    WebSchedAsapTextTemplate = 1140,
    WebSchedMessage = 1153,
    WebSchedMessageText = 1154,
    WebSchedMessage2 = 1155,
    WebSchedMessageText2 = 1156,
    WebSchedMessage3 = 1157,
    WebSchedMessageText3 = 1158,

    ///<summary>DefNum for the ApptConfirm status type that will automatically be assigned to Web Sched new patient appointments.</summary>
    WebSchedNewPatConfirmStatus = 1170,

    ///<summary>Enum: WebSchedProviderRules 0=FirstAvailable, 1=PrimaryProvider, 2=SecondaryProvider, 3=LastSeenHygienist</summary>
    [PrefName(ValueType = PrefValueType.INT)]
    WebSchedProviderRule = 1176,

    ///<summary>DefNum for the ApptConfirm status type that will automatically be assigned to Web Sched Recall appointments.</summary>
    WebSchedRecallConfirmStatus = 1181,

    ///<summary>Enum:EmailType 0=Regular 1=Html 2=RawHtml. Used to determine format for email for web sched recall messages</summary>
    WebSchedRecallEmailTemplateType = 1182,

    ///<summary>Enum:EmailType 0=Regular 1=Html 2=RawHtml. Used to determine format for email for web sched recall messages</summary>
    WebSchedRecallEmailTemplateType2 = 1183,

    ///<summary>Enum:EmailType 0=Regular 1=Html 2=RawHtml. Used to determine format for email for web sched recall messages</summary>
    WebSchedRecallEmailTemplateType3 = 1184,

    ///<summary>Enum:EmailType 0=Regular 1=Html 2=RawHtml. Used to determine format for email for web sched recall messages</summary>
    WebSchedRecallEmailTemplateTypeAgg = 1185,
    WebSchedSubject = 1190,
    WebSchedSubject2 = 1191,
    WebSchedSubject3 = 1192,

    ///<summary>Update Server: Version updates can only be performed from this computer, and the eConnector can only be installed on this computer.</summary>
    WebServiceServerName = 1214
}

public static class PrefNameExtensions
{
    public static PrefValueType GetValueType(this PrefName prefName)
    {
        return EnumTools.GetAttributeOrDefault<PrefNameAttribute>(prefName).ValueType;
    }

    public static string GetValueAsText(this PrefName prefName)
    {
        return prefName.GetValueType() switch
        {
            PrefValueType.NONE or PrefValueType.STRING => PrefC.GetString(prefName),
            PrefValueType.LONG => SOut.Long(PrefC.GetLong(prefName)),
            PrefValueType.LONG_NEG_ONE_AS_ZERO => PrefC.GetLong(prefName) == -1 ? "0" : PrefC.GetLong(prefName).ToString(),
            PrefValueType.LONG_NEG_ONE_AS_BLANK => PrefC.GetLong(prefName) == -1 ? "" : PrefC.GetLong(prefName).ToString(),
            _ => PrefC.GetString(prefName)
        };
    }

    public static bool Update(this PrefName prefName, object value)
    {
        switch (prefName.GetValueType())
        {
            case PrefValueType.NONE:
                return false;
            case PrefValueType.BOOL:
                return Prefs.UpdateBool(prefName, SIn.Bool(value.ToString()));
            case PrefValueType.ENUM:
            case PrefValueType.INT:
            case PrefValueType.COLOR:
            case PrefValueType.YN_DEFAULT_FALSE:
            case PrefValueType.YN_DEFAULT_TRUE:
                return Prefs.UpdateInt(prefName, SIn.Int(value.ToString()));
            case PrefValueType.LONG:
                return Prefs.UpdateLong(prefName, SIn.Long(value.ToString()));
            case PrefValueType.LONG_NEG_ONE_AS_ZERO:
            case PrefValueType.LONG_NEG_ONE_AS_BLANK:
                var newValue = SIn.Long(value.ToString());
                return newValue > 0 ? Prefs.UpdateLong(prefName, newValue) : Prefs.UpdateLong(prefName, -1);

            case PrefValueType.DOUBLE:
                return Prefs.UpdateDouble(prefName, SIn.Double(value.ToString()));
            case PrefValueType.DATE:
            case PrefValueType.DATETIME:
                return Prefs.UpdateDateT(prefName, (DateTime) value);
            case PrefValueType.STRING:
            default:
                return Prefs.UpdateString(prefName, value.ToString());
        }
    }
}

public class PrefNameAttribute : Attribute
{
    public PrefValueType ValueType { get; set; } = PrefValueType.NONE;
}

public enum PrefValueType
{
    NONE,
    BOOL,
    STRING,
    ENUM,
    INT,
    LONG,
    LONG_NEG_ONE_AS_ZERO,
    LONG_NEG_ONE_AS_BLANK,
    BYTE,
    DOUBLE,
    DATE,
    DATETIME,
    COLOR,

    ///<summary>Uses YN enum. 0=Unknown,1=Yes, or 2=No. If 0-Unknown, then this defaults to true. Y and N are considered overrides. This allows us to change the default behavior while still preserving user choices by changing type to YN_DEFAULT_FALSE.  This is overkill for most prefs, and bool is usually preferred.</summary>
    YN_DEFAULT_TRUE,

    ///<summary>Uses YN enum. 0=Unknown,1=Yes, or 2=No. If 0-Unknown, then this defaults to false. Y and N are considered overrides. This allows us to change the default behavior while still preserving user choices by changing type to YN_DEFAULT_TRUE.  This is overkill for most prefs, and bool is usually preferred.</summary>
    YN_DEFAULT_FALSE
}

///<summary>Used by pref "AppointmentSearchBehavior". </summary>
public enum SearchBehaviorCriteria
{
    ///<summary>0 - Based only on provider availability from the schedule.</summary>
    [Description("Provider Time")]
    ProviderTime,

    ///<summary>1 - Based on provider schedule availability as well as the availabilty of their operatory (dynamic or directly assigned).</summary>
    [Description("Provider Time Operatory")]
    ProviderTimeOperatory,
}

///<summary>Used by pref "AccountingSoftware".  0=OpenDental, 1=QuickBooks, 2=QuickBooksOnline</summary>
public enum AccountingSoftware
{
    [Description("Open Dental")]
    OpenDental,

    [Description("QuickBooks")]
    QuickBooks,

    [Description("QuickBooks Online")]
    QuickBooksOnline,
}

public enum RigorousAccounting
{
    ///<summary>0 - Payments are automatically split and paysplit validity is enforced.</summary>
    [Description("Rigorous")]
    EnforceFully,

    ///<summary>1 - Payments are automatically split, but paysplit validity is not enforced.</summary>
    [Description("Auto-Split Only")]
    AutoSplitOnly,

    ///<summary>2 - Payments are not automatically split, nor is paysplit validity enforced.</summary>
    [Description("Manual")]
    DontEnforce
}

public enum RigorousAdjustments
{
    ///<summary>0 - Automatically link adjustments and procedures, adjustment linking enforced.</summary>
    [Description("Rigorous")]
    EnforceFully,

    ///<summary>1 - Adjustment links are made automatically, but it can be edited.</summary>
    [Description("Link Only")]
    LinkOnly,

    ///<summary>2 - Adjustment links aren't made, nor are they enforced.</summary>
    [Description("Manual")]
    DontEnforce
}

///<summary>Used by pref "WebSchedProviderRule". Determines how Web Sched will decide on what provider time slots to show patients.</summary>
public enum WebSchedProviderRules
{
    ///<summary>0 - Dynamically picks the first available provider based on the time slot picked by the patient.</summary>
    FirstAvailable,

    ///<summary>1 - Only shows time slots that are available via the patient's primary provider.</summary>
    PrimaryProvider,

    ///<summary>2 - Only shows time slots that are available via the patient's secondary provider.</summary>
    SecondaryProvider,

    ///<summary>3 - Only shows time slots that are available via the patient's last seen hygienist.</summary>
    LastSeenHygienist
}

///<summary>How this database is being hosted.</summary>
public enum DatabaseModeEnum
{
    ///<summary>Customer is hosting their own database.</summary>
    Normal,

    ///<summary>Open Dental is hosting the database.</summary>
    Cloud,
}

public enum PPOWriteoffDateCalc
{
    /// <summary>0 - Use the insurance payment date when dating write-off estimates and adjustments in reports. </summary>
    [Description("Insurance Pay Date")]
    InsPayDate,

    /// <summary>1 - Use the date of the procedure when dating write-off estimates and adjustments in reports.</summary>
    [Description("Procedure Date")]
    ProcDate,

    /// <summary>2 - Uses initial claim date for write-off estimates and insurance payment date for writeoff adjustments in reports.</summary>
    [Description("Initial Claim Date/Ins Pay Date")]
    ClaimPayDate
}

///<summary>Different options for electronic statements.  Descriptions taken from the FormBillingDefaults.
///Stored in practice pref: BillingUseElectronic. Determines which vendor use use for electronic billing.</summary>
public enum BillingUseElectronicEnum
{
    ///<summary>0 - Not using electronic statements.</summary>
    [Description("No electronic billing")]
    None,

    ///<summary>1. DentalXChange sends xml string to web.</summary>
    [Description("DentalXChange")]
    EHG,

    ///<summary>2 - Prints to local xml file.</summary>
    [Description("Output to file")]
    POS,

    ///<summary>3 - ClaimX/ExtraDent prints to local xml file.</summary>
    [Description("ClaimX / ExtraDent")]
    ClaimX,

    ///<summary>4 - EDS prints to local xml file.</summary>
    [Description("EDS")]
    EDS,
}

///<summary>Indicates which blue book feature is activated. 0 if neither are active.</summary>
public enum AllowedFeeSchedsAutomate
{
    ///<summary>0 - None</summary>
    [Description("None")]
    None,

    ///<summary>1 - Legacy Blue Book feature</summary>
    [Description("Legacy Blue Book (Ver. 20.2 and earlier)")]
    LegacyBlueBook,

    ///<summary>2 - Blue Book feature</summary>
    [Description("Blue Book")]
    BlueBook,
}

///<summary>Method used by the Blue Book feature to generate estimates.</summary>
public enum InsBlueBookAllowedFeeMethod
{
    ///<summary>0 - Median</summary>
    [Description("Median")]
    Median,

    ///<summary>1 - Average</summary>
    [Description("Average")]
    Average,

    ///<summary>2 - MostRecent</summary>
    [Description("Most Recent")]
    MostRecent,
}

///<summary>Whether or not anonymous fee sharing for the blue book feature is turned on.</summary>
public enum InsBlueBookAnonShareEnable
{
    ///<summary>0 - Off</summary>
    Off,

    ///<summary>1 - Off. User won't be prompted to turn it on.</summary>
    OffNoPrompt,

    ///<summary>2 - On</summary>
    On,
}

///<summary>The level of automation for ERA processing. This setting can be overriden for individual Carriers.</summary>
public enum EraAutomationMode
{
    ///<summary>0 - Never used for the EraAutomationBehavior preference. Only used for Carrier.EraAutomationOverride to indicate that the carrier uses the EraAutomationBehavior preference instead of an override.</summary>
    [Description("Use Global Preference")]
    UseGlobal,

    ///<summary>1 - ERAs are manually processed.</summary>
    [Description("Review All")]
    ReviewAll,

    ///<summary>2 - Allows ERAs to be processed with a single button click.</summary>
    [Description("Semi-automatic")]
    SemiAutomatic,

    ///<summary>3 - When ERAs are imported, they are fully processed without any input from a user.</summary>
    [Description("Fully-automatic")]
    FullyAutomatic,
}

public enum EnumEraAutoPostWriteOff
{
    ///<summary>0 - only posts write-offs for primary</summary>
    [Description("Primary from ERA")]
    PriFromERA,

    ///<summary>1 - Always posts write-offs</summary>
    [Description("Always")]
    Always,

    ///<summary>2 - Uses the patplan ordinal to post write-off</summary>
    [Description("Primary from Plan")]
    PriFromPlan
}

///<summary>Indicates how the user prefers to print their Appt module.</summary>
public enum ApptPrintColorBehavior
{
    ///<summary>0 - Full Color.</summary>
    FullColor,

    ///<summary>1 - Less Color.</summary>
    LessColor,

    ///<summary>2 - Grayscale.</summary>
    Grayscale
}

///<summary> Indicates whether a user prefers to have negative adjustments with warnings, be blocked, or have no action when attempted.///</summary>
public enum EnumAdjustmentBlockOrWarn
{
    ///<summary> 0 - Warning.</summary>
    Warn,

    ///<summary> 1 - Block. </summary>
    Block,

    ///<summary> 2 - Allow.</summary>
    Allow
}

/// <summary>Indicates the desired behavior for an appointment change provider prompt.</summary>
public enum EnumApptProvPrompt
{
    ///<summary>0 - Prompt the user and default to yes when enter is clicked. This was the old default prior to this enum.</summary>
    [Description("Prompt, default Yes")]
    PromptDefaultYes,

    ///<summary>1 - Prompt the user and default to no when enter is clicked.</summary>
    [Description("Prompt, default No")]
    PromptDefaultNo,

    ///<summary>2 - Don't prompt the user and change the provider.</summary>
    [Description("Don't prompt, change provider")]
    NoPromptChange,

    ///<summary>3 - Don't prompt the user and don't change the provider.</summary>
    [Description("Don't prompt, don't change provider")]
    NoPromptNoChange,
}