using System.ComponentModel;
using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Def : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DefNum;

    ///<summary>Enum:DefCat</summary>
    public DefCat Category;

    public int ItemOrder;

    ///<summary>Each category is a little different.  This field is usually the common name of the item.</summary>
    public string ItemName;

    ///<summary>This field can be used to store extra info about the item. Used extensively by ImageCategories to store single letter codes.</summary>
    public string ItemValue;

    ///<summary>Some categories include a color option.</summary>
    public Color ItemColor;

    public bool IsHidden;

    ///<summary>Returns a copy of the def.</summary>
    public Def Copy()
    {
        return (Def) MemberwiseClone();
    }
}

///<summary>Definition Category. Go to the definition setup window (FormDefinitions) in the program to see how each of these categories is used.
/// If you add a category, make sure to add it to the switch statement in DefL so the user can edit it.
/// Add a "NotUsed" description attribute to defs that shouldn't show up in FormDefinitions.</summary>
public enum DefCat
{
    ///<summary>0- Colors to display in Account module.</summary>
    [Description("Account Colors")]
    AccountColors = 0,

    ///<summary>1- Adjustment types.</summary>
    [Description("Adj Types")]
    AdjTypes = 1,

    ///<summary>2- Appointment confirmed types.</summary>
    [Description("Appt Confirmed")]
    ApptConfirmed = 2,

    ///<summary>3- Procedure quick add list for appointments. Example: D1023,D1024. Single tooth numbers are allowed, example D1151#8,D0220#15. This is really only useful for PAs. Tooth number is stored in user's nomenclature, not American numbering.</summary>
    [Description("Appt Procs Quick Add")]
    ApptProcsQuickAdd = 3,

    ///<summary>4- Billing types.</summary>
    [Description("Billing Types")]
    BillingTypes = 4,

    ///<summary>5- Not used.</summary>
    [Description("NotUsed")]
    ClaimFormats = 5,

    ///<summary>6- Not used.</summary>
    [Description("NotUsed")]
    DunningMessages = 6,

    ///<summary>7- Not used.</summary>
    [Description("NotUsed")]
    FeeSchedNamesOld = 7,

    ///<summary>8- Not used.</summary>
    [Description("NotUsed")]
    MedicalNotes = 8,

    ///<summary>9- Not used.</summary>
    [Description("NotUsed")]
    OperatoriesOld = 9,

    ///<summary>10- Payment types.</summary>
    [Description("Payment Types")]
    PaymentTypes = 10,

    ///<summary>11- Procedure code categories.</summary>
    [Description("Proc Code Categories")]
    ProcCodeCats = 11,

    ///<summary>12- Progress note colors.</summary>
    [Description("Prog Notes Colors")]
    ProgNoteColors = 12,

    ///<summary>13- Statuses for recall, reactivation, unscheduled, and next appointments.</summary>
    [Description("Recall/Unsched Status")]
    RecallUnschedStatus = 13,

    ///<summary>14- Not used.</summary>
    [Description("NotUsed")]
    ServiceNotes = 14,

    ///<summary>15- Not used.</summary>
    [Description("NotUsed")]
    DiscountTypes = 15,

    ///<summary>16- Diagnosis types.</summary>
    [Description("Diagnosis Types")]
    Diagnosis = 16,

    ///<summary>17- Colors to display in the Appointments module.</summary>
    [Description("Appointment Colors")]
    AppointmentColors = 17,

    ///<summary>18- Image categories. ItemValue can be one or more of the following, no delimiters. X = Show in Chart Module, M=Show Thumbnails, F = Show in Patient Forms, L = Show in Patient Portal, P = Show in Patient Pictures, S = Statements, T = Graphical Tooth Charts, R = Treatment Plans, E = Expanded, A = Payment Plans, C = Claim Attachments, B = Lab Cases, U = Autosave Forms, Y = Task Attachments, N = Claim Responses.</summary>
    [Description("Image Categories")]
    ImageCats = 18,

    ///<summary>19- Not used.</summary>
    [Description("NotUsed")]
    ApptPhoneNotes = 19,

    ///<summary>20- Treatment plan priority names.</summary>
    [Description("Treat' Plan Priorities")]
    TxPriorities = 20,

    ///<summary>21- Miscellaneous color options. See enum DefCatMisColors.</summary>
    [Description("Misc Colors")]
    MiscColors = 21,

    ///<summary>22- Colors for the graphical tooth chart.</summary>
    [Description("Chart Graphic Colors")]
    ChartGraphicColors = 22,

    ///<summary>23- Categories for the Contact list.</summary>
    [Description("Contact Categories")]
    ContactCategories = 23,

    ///<summary>24- Categories for Letter Merge.</summary>
    [Description("Letter Merge Cats")]
    LetterMergeCats = 24,

    ///<summary>25- Types of Schedule Blockouts.</summary>
    [Description("Blockout Types")]
    BlockoutTypes = 25,

    ///<summary>26- Categories of procedure buttons in Chart module</summary>
    [Description("Proc Button Categories")]
    ProcButtonCats = 26,

    ///<Summary>27- Types of commlog entries.</Summary>
    [Description("Commlog Types")]
    CommLogTypes = 27,

    ///<summary>28- Categories of Supplies</summary>
    [Description("Supply Categories")]
    SupplyCats = 28,

    ///<summary>29- Types of unearned income used in accrual accounting.</summary>
    [Description("PaySplit Unearned Types")]
    PaySplitUnearnedType = 29,

    ///<summary>30- Prognosis types.</summary>
    [Description("Prognosis")]
    Prognosis = 30,

    ///<summary>31- Custom Tracking, statuses such as 'review', 'hold', 'riskmanage', etc.</summary>
    [Description("Claim Custom Tracking")]
    ClaimCustomTracking = 31,

    ///<summary>32- PayType for claims such as 'Check', 'EFT', etc.</summary>
    [Description("Insurance Payment Types")]
    InsurancePaymentType = 32,

    ///<summary>33- Categories of priorities for tasks.</summary>
    [Description("Task Priorities")]
    TaskPriorities = 33,

    ///<summary>34- Categories for fee override colors.</summary>
    [Description("Fee Colors")]
    FeeColors = 34,

    ///<summary>35- Provider specialties.  General, Hygienist, Pediatric, Primary Care Physician, etc.</summary>
    [Description("Provider Specialties")]
    ProviderSpecialties = 35,

    ///<summary>36- Reason why a claim proc was rejected. This must be set on each individual claim proc.</summary>
    [Description("Claim Payment Tracking")]
    ClaimPaymentTracking = 36,

    ///<summary>37- Procedure quick charge list for patient accounts.</summary>
    [Description("Account Procs Quick Add")]
    AccountQuickCharge = 37,

    ///<summary>38- Insurance verification status such as 'Verified', 'Unverified', 'Pending Verification'.</summary>
    [Description("Insurance Verification Status")]
    InsuranceVerificationStatus = 38,

    ///<summary>39- Regions that clinics can be assigned to.</summary>
    [Description("Regions")]
    Regions = 39,

    ///<summary>40- ClaimPayment Payment Groups.</summary>
    [Description("Claim Payment Groups")]
    ClaimPaymentGroups = 40,

    ///<summary>41 - Auto Note Categories.  Used to categorize autonotes into custom categories.</summary>
    [Description("Auto Note Categories")]
    AutoNoteCats = 41,
    
    ///<summary>43 - Custom Claim Status Error Code.</summary>
    [Description("Claim Error Code")]
    ClaimErrorCode = 43,

    ///<summary>44 - Specialties that clinics perform.  Useful for separating patient clones across clinics.</summary>
    [Description("Clinic Specialties")]
    ClinicSpecialty = 44,
    
    ///<summary>46 - Carrier Group Name.</summary>
    [Description("Carrier Group Names")]
    CarrierGroupNames = 46,

    ///<summary>47 - PayPlanCategory</summary>
    [Description("Payment Plan Categories")]
    PayPlanCategories = 47,

    ///<summary>48 - Associates an insurance payment to an account number.  Currently only used with "Auto Deposits".</summary>
    [Description("Auto Deposit Account")]
    AutoDeposit = 48,

    ///<summary>49 - Code Group used for insurance filing.</summary>
    [Description("Insurance Filing Code Group")]
    InsuranceFilingCodeGroup = 49,

    ///<summary>50 - Time card adjustment types.
    ///Currently for PTO, but in future could be used for other types as well if we implement the Usage def field.</summary>
    [Description("Time Card Adj Types")]
    TimeCardAdjTypes = 50,
    
    ///<summary>52 - Categories for the Certifications feature.</summary>
    [Description("Certification Categories")]
    CertificationCategories = 52,
    
    ///<summary>54 - HQ Only task categories.</summary>
    [Description("Task Categories HqOnly")]
    TaskCategories = 54,

    ///<summary>55 - Operatory Types. This field is only informational. The value isn't used for functionality.</summary>
    [Description("Operatory Types")]
    OperatoryTypes = 55
}

public enum DefCatMiscColors
{
    FamilyModuleCoverage = 0,
    PerioBleeding = 1,
    PerioSuppuration = 2,
    ChartModuleMedical = 3,
    PerioPlaque = 4,
    PerioCalculus = 5,
    ChartTodaysProcs = 6,
    CommlogApptRelated = 7,
    FamilyModuleReferral = 8,

    ///<summary>9 - This color is used for the fields in the family module for the in case of emergency contacts.</summary>
    FamilyModuleICE = 9,

    FamilyModPatRestrict = 10,
    MainBorder = 11,
    MainBorderOutline = 12,
    MainBorderText = 13
}

public class DefCatOptions(DefCat defCat, bool canDelete = false, bool canEditName = true, bool canHide = true, bool enableColor = false, bool enableValue = false, bool isValidDefNum = false, bool showNoColor = false)
{
    public DefCat DefCat = defCat;
    public bool CanEditName = canEditName;
    public bool EnableValue = enableValue;
    public bool EnableColor = enableColor;
    public string HelpText;
    public bool CanDelete = canDelete;
    public bool CanHide = canHide;
    public string ValueText;
    public bool IsValueDefNum = isValidDefNum;
    public bool DoShowItemOrderInValue;
    public bool DoShowNoColor = showNoColor;
}