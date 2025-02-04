using System;
using System.ComponentModel;

namespace OpenDentBusiness;

///<Summary>Different types of sheets that can be used.</Summary>
public enum SheetTypeEnum{
	///<Summary>0-Requires SheetParameter for PatNum. Does not get saved to db.</Summary>
	LabelPatient = 0,
	///<Summary>1-Requires SheetParameter for CarrierNum. Does not get saved to db.</Summary>
	LabelCarrier = 1,
	///<Summary>2-Requires SheetParameter for ReferralNum. Does not get saved to db.</Summary>
	LabelReferral = 2,
	///<Summary>3-Requires SheetParameters for PatNum,ReferralNum.</Summary>
	[SheetType(CanAutoSave=true)]
	ReferralSlip = 3,
	///<Summary>4-Requires SheetParameter for AptNum. Does not get saved to db.</Summary>
	LabelAppointment = 4,
	///<summary>6-Requires SheetParameter for PatNum.</summary>
	[SheetType(CanAutoSave=true)]
	Consent = 6,
	///<summary>7-Requires SheetParameter for PatNum.</summary>
	[SheetType(CanAutoSave=true)]
	PatientLetter = 7,
	///<summary>8-Requires SheetParameters for PatNum,ReferralNum.</summary>
	[SheetType(CanAutoSave=true)]
	ReferralLetter = 8,
	///<Summary>9-Requires SheetParameter for PatNum.</Summary>
	[SheetType(CanAutoSave=true)]
	PatientForm = 9,
	///<Summary>10-Requires SheetParameter for AptNum.  Does not get saved to db.</Summary>
	RoutingSlip = 10,
	///<Summary>11-Requires SheetParameter for PatNum.</Summary>
	[SheetType(CanAutoSave=true)]
	MedicalHistory = 11,
	///<Summary>12-Requires SheetParameter for PatNum, LabCaseNum.</Summary>
	[SheetType(CanAutoSave=true)]
	LabSlip = 12,
	///<Summary>13-Requires SheetParameter for PatNum.</Summary>
	[SheetType(CanAutoSave=true)]
	ExamSheet = 13,
	///<summary>14-Requires SheetParameter for DepositNum.</summary>
	DepositSlip = 14,
	///<summary>15-Requires SheetParameter for PatNum.</summary>
	Statement = 15,
	///<summary>16-Requires SheetParameters for PatNum,MedLab,MedLabResult.</summary>
	MedLabResults = 16,
	///<summary>17-Requires SheetParameters for PatNum,TreatmentPlan.</summary>
	TreatmentPlan = 17,
	///<summary>19-Used for Payment Plans to Sheets.</summary>
	[SheetType(CanAutoSave=true)]
	PaymentPlan = 19,
	/*StatementHeader,
	TxPlanHeader,
	Postcard*/
	///<summary>21</summary>
	ERA = 21,
	///<summary>22</summary>
	ERAGridHeader = 22,
	///<summary>24-Deprecated. No longer needed when change was made to only display one Patient Dashboard at a time.  Defines the layout of a 
	///patient specific dashboard sheet.  Not directly user editable.  Each sheetfielddef linked to this sheet type further links a
	///PatientDashboardWidget type sheet to this PatientDashboard sheet, allowing users to place various PatientDashboardWidgets on their personal
	///PatientDashboard.</summary>
	[Description("Deprecated(PatientDashboardLayout)")]
	PatientDashboard = 24,
	///<summary>25-Defines the layout and elements of a Patient Dashboard.  Editable from Dashboard Setup with Setup permissions.</summary>
	[Description("Patient Dashboard")]
	PatientDashboardWidget = 25,
	///<summary>26</summary>
	[SheetLayoutAttribute(isChartModule:true,SheetFieldLayoutMode.Ecw,SheetFieldLayoutMode.MedicalPractice)]
	ChartModule = 26,
	///<summary>27-Not designed to be saved to the db.  Useful when needing a "none" or "all" default option for UI.</summary>
	None = 27,
}

///<summary>For sheetFields</summary>
public enum GrowthBehaviorEnum {
	///<Summary>Not allowed to grow.  Max size would be Height and Width.</Summary>
	None,
	///<Summary>Can grow down if needed, and will push nearby objects out of the way so that there is no overlap.</Summary>
	DownLocal,
	///<Summary>Can grow down, and will push down all objects on the sheet that are below it.  Mostly used when drawing grids.</Summary>
	DownGlobal,
	///<summary>Used with dynamic grids to grow the grid to fill to the right and bottom of the parent control, does not check for overlap.</summary>
	[SheetGrowthAttribute(true)]
	FillRightDown,
	///<summary>Used with dynamic grids to grow the grid to fill to the bottom of the parent control, does not check for overlap.</summary>
	[SheetGrowthAttribute(true)]
	FillDown,
	///<summary>Used with dynamic grids to grow the grid to fill to the right of the parent control, does not check for overlap.</summary>
	[SheetGrowthAttribute(true)]
	FillRight,
	///<summary>Used with dynamic grids to grow the grid to fill vertical space in parent control and fit grid width to include all columns.
	///Primarily for ProgressNotes grid in Chart Module.</summary>
	[SheetGrowthAttribute(true,true)]
	FillDownFitColumns,
}

	
public enum SheetFieldType {
	///<Summary>0-Pulled from the database to be printed on the sheet.  Or also possibly just generated at runtime even though not pulled from the database.   User still allowed to change the output text as they are filling out the sheet so that it can different from what was initially generated.</Summary>
	OutputText = 0,
	///<Summary>1-A blank box that the user is supposed to fill in.</Summary>
	[Description("Input")]
	InputField = 1,
	///<Summary>2-This is text that is defined as part of the sheet and will never change from sheet to sheet.  </Summary>
	StaticText = 2,
	///<summary>3-Stores a parameter other than the PatNum.  Not meant to be seen on the sheet.  Only used for SheetField, not SheetFieldDef.</summary>
	Parameter = 3,
	///<Summary>4-Any image of any size, typically a background image for a form.</Summary>
	Image = 4,
	///<summary>5-One sequence of dots that makes a line.  Continuous without any breaks.  Each time the pen is picked up, it creates a new field row in the database.</summary>
	Drawing = 5,
	///<Summary>6-A simple line drawn from x,y to x+width,y+height.  So for these types, we must allow width and height to be negative or zero.</Summary>
	Line = 6,
	///<Summary>7-A simple rectangle outline.</Summary>
	Rectangle = 7,
	///<summary>8-A clickable area on the screen.  It's a form of input, so treated similarly to an InputField.  The X will go from corner to corner of the rectangle specified.  It can also behave like a radio button</summary>
	[Description("Check Box")]
	CheckBox = 8,
	///<summary>9-A signature box, either Topaz pad or directly on the screen with stylus/mouse.  The signature is encrypted based an a hash of all
	///other field values in the entire sheet, excluding other SigBoxes.  The order is critical.</summary>
	[Description("Signature")]
	SigBox = 9,
	///<Summary>10-An image specific to one patient.</Summary>
	PatImage = 10,
	///<Summary>11-Special: Used for ToothChart, ToothChartLegend, and 3 more chart module items.</Summary>
	Special = 11,
	///<summary>12-Grid: Placeable grids similar to ODGrids. Used primarily in statements.</summary>
	Grid = 12,
	///<summary>13-ComboBox: Placeable combo box for selecting filled options.</summary>
	[Description("Combo Box")]
	ComboBox = 13,
	///<summary>15-MobileHeader: The parent field of a group of fields. All fields in between this field and the next MobileHeader will be grouped toghether in the mobile view.
	///EG... "Personal", "Address and Home Phone", "Insurance".</summary>
	[Description("Header")]
	MobileHeader = 15,
	///<summary>16-A signature box, either Topaz pad or directly on the screen with stylus/mouse.  The signature is encrypted based an a hash of all 
	///other field values in the entire sheet, excluding other SigBoxes.  The order is critical.</summary>
	[Description("Practice Signature")]
	SigBoxPractice = 16
	//<summary></summary>
	//RadioButton

	//<Summary>Not yet supported.  This might be redundant, and we might use border element instead as the preferred way of drawing a box.</Summary>
	//Box
}

public enum SheetInternalType{
	LabelPatientMail = 0,
	LabelPatientLFAddress = 1,
	LabelPatientLFChartNumber = 2,
	LabelPatientLFPatNum = 3,
	LabelPatientRadiograph = 4,
	///<summary>Users are NEVER allowed to use this sheet type. The custom print logic used for this sheet type does not allow any customization.</summary>
	[SheetInternal(DoShowInInternalList=false)]
	LabelText = 5,
	LabelCarrier = 6,
	LabelReferral = 7,
	ReferralSlip = 8,
	LabelAppointment = 9,
	Consent = 11,
	PatientLetter = 12,
	PatientLetterTxFinder = 13,
	ReferralLetter = 14,
	RoutingSlip = 15,
	PatientRegistration = 16,
	FinancialAgreement = 17,
	HIPAA = 18,
	MedicalHist = 19,
	LabSlip = 20,
	ExamSheet = 21,
	DepositSlip = 22,
	Statement = 23,
	///<summary>Users are NEVER allowed to use this sheet type. It is for internal use only. It should be hidden in all lists and unselectable.</summary>
	[SheetInternal(DoShowInInternalList=false)]
	MedLabResults = 24,
	TreatmentPlan = 25,
	PaymentPlan = 27,
	ERA = 29,
	ERAGridHeader = 30,
	[SheetInternal(DoShowInInternalList=false)]
	PatientTransferCEMT = 32,
	ChartModule = 33,
	[SheetInternal(DoShowInInternalList=false,DoShowInDashboardSetup=true)]
	PatientDashboard = 34,
	[SheetInternal(DoShowInInternalList=false,DoShowInDashboardSetup=true)]
	PatientDashboardToothChart = 35,
	COVID19 = 36,
}

public enum OutInCheck{
	Out,
	In,
	Check
}

///<summary>For Chart layout. If we alter this list, we must edit the XML resource files that store our default sheetDefs.</summary>
public enum SheetFieldLayoutMode {
	///<summary>Valid for every SheetTypeEnum. When SheetTypeEnum is associated to a dynamic layout this is the way the layout will show by default.</summary>
	Default,
	///<summary>Deprecated. Chart module dynamic layout when we are viewing the SheetFieldLayoutMode.Default and the Treatment Plans checkbox is checked.</summary>
	[DescriptionAttribute("Treatment plan view")]
	TreatPlan,
	///<summary>Chart module dynamic layout when ECW is enabled.</summary>
	[DescriptionAttribute("ECW enabled view")]
	Ecw,
	///<summary>Deprecated. Chart module dynamic layout when ECW is enabled and the Treatment Plans checkbox is checked.</summary>
	[DescriptionAttribute("ECW treatment plan view")]
	EcwTreatPlan,
	///<summary>Deprecated. Chart module dynamic layout when Orion is enabled.</summary>
	[DescriptionAttribute("Orion enabled view")]
	Orion,
	///<summary>Deprecated. Chart module dynamic layout when Orion is enabled and the Treatment Plans checkbox is checked.</summary>
	[DescriptionAttribute("Orion treatment plan view")]
	OrionTreatPlan,
	///<summary>Chart module dynamic layout when current clinic is associated to a medical clinic or practice.</summary>
	[DescriptionAttribute("Medical Practice")]
	MedicalPractice,
	///<summary>Deprecated. Chart module dynamic layout when current clinic is associated to a medical clinic or practice and the Treatment Plans checkbox is checked.</summary>
	[DescriptionAttribute("Medical practice treatment plan view")]
	MedicalPracticeTreatPlan,
}

public class SheetTypeAttribute:Attribute {
	///<summary>This will indicate if the auto save check box should be enabled in FormSheetDef. Will only matter if offices have opted to allow saving sheets as images</summary>
	public bool CanAutoSave=false;
	public SheetTypeAttribute() { }
}
	
public class SheetInternalAttribute:Attribute {
	public bool DoShowInInternalList=true;
	public bool DoShowInDashboardSetup=false;

	public SheetInternalAttribute() {
		//Empty constructor needed, default values used.
	}
}

///<summary>Used when we want to link a SheetTypeEnum to a set of SheetFieldLayoutModes.  Ex: ChartModule link to ECW or Medical etc.  SheetTypeEnums which are not chart module will have IsChartModule=false and ArraySheetFieldLayoutModes will only contain SheetFieldLayoutMode.Default.</summary>
public class SheetLayoutAttribute:Attribute {
	///<summary>False by default. True when the SheetTypeEnum is associated to a dynamic layout SheetEnumType, like SheetEnumType.ChartModule.</summary>
	public bool IsChartModule=false;
	///<summary>Always contains SheetFieldLayoutMode.Default.</summary>
	public SheetFieldLayoutMode[] ArraySheetFieldLayoutModes=new SheetFieldLayoutMode[] { SheetFieldLayoutMode.Default };

	public SheetLayoutAttribute() {
		//Empty constructor needed, default values used.
	}

	///<summary>The arraySheetFieldLayoutModes must not contain Default option as it will be automatically included.</summary>
	public SheetLayoutAttribute(bool isChartModule,params SheetFieldLayoutMode[] arraySheetFieldLayoutModes) {
		IsChartModule=isChartModule;
		ArraySheetFieldLayoutModes=new SheetFieldLayoutMode[arraySheetFieldLayoutModes.Length+1];//+1 for default.
		ArraySheetFieldLayoutModes[0]=SheetFieldLayoutMode.Default;
		for(var i=0;i<arraySheetFieldLayoutModes.Length;i++) {
			ArraySheetFieldLayoutModes[i+1]=arraySheetFieldLayoutModes[i];
		}
	}
}

///<summary>Used to identify various dynamic sheet related enums.</summary>
public class SheetGrowthAttribute:Attribute {
	///<summary>False by default.
	///True when the GrowthBehavior is associated to a dynamic layout sheetFieldDef, like chart module grids.</summary>
	public bool IsDynamic=false;
	///<summary>False by default. When true the associated GrowthBehaviorEnum will only show in the UI for ODGrid sheetFieldDefs.</summary>
	public bool IsGridOnly=false;

	public SheetGrowthAttribute() {
		//Empty constructor needed, default values used.
	}

	public SheetGrowthAttribute(bool isDynamic,bool isGridOnly=false) {
		IsDynamic=isDynamic;
		IsGridOnly=isGridOnly;
	}
}