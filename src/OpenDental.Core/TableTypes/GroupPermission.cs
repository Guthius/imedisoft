using System;
using System.Collections;
using System.ComponentModel;
using CodeBase;

namespace OpenDentBusiness{

	///<summary>Every user group has certain permissions.  This defines a permission for a group.  The absense of permission would cause that row to be deleted from this table.</summary>
	[Serializable]
	[CrudTable(IsSynchable=true)]

	public class GroupPermission:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long GroupPermNum;
		///<summary>Only granted permission if newer than this date.  Can be Minimum (01-01-0001) to always grant permission.</summary>
		public DateTime NewerDate;
		///<summary>Can be 0 to always grant permission.  Otherwise, only granted permission if item is newer than the given number of days.  1 would mean only if entered today.</summary>
		public int NewerDays;
		///<summary>FK to usergroup.UserGroupNum.  The user group for which this permission is granted.  If not authorized, then this groupPermission will have been deleted.</summary>
		public long UserGroupNum;
		///<summary>Enum:EnumPermType Some permissions will treat a zero FKey differently. Some denote it as having access to everything for that PermType. I.e. Reports.</summary>
		public EnumPermType PermType;
		///<summary>Generic foreign key to any other table.  Typically used in combination with PermType to give permission to specific things.</summary>
		public long FKey;

		
		public GroupPermission Copy(){
			return (GroupPermission)this.MemberwiseClone();
		}

	}

	///<summary>A hard-coded list of permissions which may be granted to usergroups.</summary>
	public enum EnumPermType {
		///<summary>0</summary>
		[Description("")]
		None = 0,
		///<summary>1</summary>
		[Description("Appointments Module")]
		AppointmentsModule = 1,
		///<summary>2</summary>
		[Description("Family Module")]
		FamilyModule = 2,
		///<summary>3</summary>
		[Description("Account Module")]
		AccountModule = 3,
		///<summary>4</summary>
		[Description("TreatmentPlan Module")]
		TPModule = 4,
		///<summary>5</summary>
		[Description("Chart Module")]
		ChartModule = 5,
		///<summary>6</summary>
		[Description("Imaging Module")]
		ImagingModule = 6,
		///<summary>7</summary>
		[Description("Manage Module")]
		ManageModule = 7,
		///<summary>8. Currently covers a wide variety of setup functions. </summary>
		[Description("Setup - Covers a wide variety of setup functions")]
		Setup = 8,
		///<summary>9</summary>
		[Description("Rx Create")]
		RxCreate = 9,
		///<summary>10 - DEPRECATED - Uses date restrictions. Covers editing/deleting of Completed, EO, and EC procs. 
		///Deleting procs of other statuses are covered by ProcDelete.
		///</summary>
		[Description("Edit Completed Procedure")]
		ProcComplEdit = 10,
		///<summary>11</summary>
		[Description("Choose Database")]
		ChooseDatabase = 11,
		///<summary>12</summary>
		[Description("Schedules - Practice and Provider")]
		Schedules = 12,
		///<summary>13</summary>
		[Description("Blockouts")]
		Blockouts = 13,
		///<summary>14. Uses date restrictions.</summary>
		[Description("Claim Sent Edit")]
		ClaimSentEdit = 14,
		///<summary>15. Uses date restrictions.</summary>
		[Description("Payment Create")]
		PaymentCreate = 15,
		///<summary>16. Uses date restrictions.</summary>
		[Description("Payment Edit")]
		PaymentEdit = 16,
		///<summary>17</summary>
		[Description("Adjustment Create")]
		AdjustmentCreate = 17,
		///<summary>18. Uses date restrictions.</summary>
		[Description("Adjustment Edit")]
		AdjustmentEdit = 18,
		///<summary>19</summary>
		[Description("User Query")]
		UserQuery = 19,
		///<summary>20.  Not used anymore.</summary>
		StartupSingleUserOld = 20,
		///<summary>21 Not used anymore.</summary>
		StartupMultiUserOld = 21,
		///<summary>22</summary>
		[Description("Reports")]
		Reports = 22,
		///<summary>23. Includes setting procedures complete.</summary>
		[Description("Create Completed Procedure (or set complete)")]
		ProcComplCreate = 23,
		///<summary>24. At least one user must have this permission.</summary>
		[Description("Security Admin")]
		SecurityAdmin = 24,
		///<summary>25. </summary>
		[Description("Appointment Create")]
		AppointmentCreate = 25,
		///<summary>26</summary>
		[Description("Appointment Move. Dragging, moving to pinboard, or setting broken or unscheduled")]
		AppointmentMove = 26,
		///<summary>27.  AppointmentDelete permission required in order to delete appointments.</summary>
		[Description("Appointment Edit. Does not include moving or resizing")]
		AppointmentEdit = 27,
		///<summary>28</summary>
		[Description("Backup")]
		Backup = 28,
		///<summary>29</summary>
		[Description("Edit All Time Cards")]
		TimecardsEditAll = 29,
		///<summary>30</summary>
		[Description("Deposit Slips")]
		DepositSlips = 30,
		///<summary>31. Uses date restrictions.</summary>
		[Description("Accounting Edit Entry")]
		AccountingEdit = 31,
		///<summary>32. Uses date restrictions.</summary>
		[Description("Accounting Create Entry")]
		AccountingCreate = 32,
		///<summary>33</summary>
		[Description("Accounting")]
		Accounting = 33,
		///<summary>34</summary>
		[Description("Intake Anesthetic Medications into Inventory")]
		AnesthesiaIntakeMeds = 34,
		///<summary>35</summary>
		[Description("Edit Anesthetic Records; Edit/Adjust Inventory Counts")]
		AnesthesiaControlMeds = 35,
		///<summary>36</summary>
		[Description("Insurance Payment Create")]
		InsPayCreate = 36,
		///<summary>37. Uses date restrictions. Edit Batch Insurance Payment.</summary>
		[Description("Insurance Payment Edit")]
		InsPayEdit = 37,
		///<summary>38. Uses date restrictions.</summary>
		[Description("Edit Treatment Plan")]
		TreatPlanEdit = 38,
		///<summary>39. DEPRECATED</summary>
		[Description("Reports - Production and Income, Aging")]
		ReportProdInc = 39,
		///<summary>40. Uses date restrictions.</summary>
		[Description("Time Card Delete Entry")]
		TimecardDeleteEntry = 40,
		///<summary>41. Uses date restrictions. All other equipment functions are covered by .Setup.</summary>
		[Description("Equipment Delete")]
		EquipmentDelete = 41,
		///<summary>42. Uses date restrictions. Also used in audit trail to log web form importing.</summary>
		[Description("Sheet Edit")]
		SheetEdit = 42,
		///<summary>43. Uses date restrictions.</summary>
		[Description("Commlog Edit")]
		CommlogEdit = 43,
		///<summary>44. Uses date restrictions. Allows deletion of images. SignedImageEdit permission is also needed to delete signed images.</summary>
		[Description("Image Delete")]
		ImageDelete = 44,
		///<summary>45. Uses date restrictions.</summary>
		[Description("Perio Chart Edit")]
		PerioEdit = 45,
		///<summary>46. Shows the fee textbox in the proc edit window.</summary>
		[Description("Show Procedure Fee")]
		ProcEditShowFee = 46,
		///<summary>47</summary>
		[Description("Adjustment Edit Zero Amount")]
		AdjustmentEditZero = 47,
		///<summary>48</summary>
		[Description("EHR Emergency Access")]
		EhrEmergencyAccess = 48,
		///<summary>49. Uses date restrictions.  This only applies to non-completed procs.  Deletion of completed procs is covered by ProcCompleteStatusEdit.</summary>
		[Description("TP Procedure Delete")]
		ProcDelete = 49,
		///<summary>51- Allows user to edit all providers. This is not fine-grained enough for extremely large organizations such as dental schools, so other permissions are being added as well.</summary>
		[Description("Provider Edit")]
		ProviderEdit = 51,
		///<summary>52</summary>
		[Description("eCW Appointment Revise")]
		EcwAppointmentRevise = 52,
		///<summary>53</summary>
		[Description("Procedure Note (full)")]
		ProcedureNoteFull = 53,
		///<summary>54</summary>
		[Description("Referral Add")]
		ReferralAdd = 54,
		///<summary>55</summary>
		[Description("Insurance Plan Change Subscriber")]
		InsPlanChangeSubsc = 55,
		///<summary>56</summary>
		[Description("Referral, Attach to Patient")]
		RefAttachAdd = 56,
		///<summary>57</summary>
		[Description("Referral, Delete from Patient")]
		RefAttachDelete = 57,
		///<summary>58</summary>
		[Description("Carrier Create")]
		CarrierCreate = 58,
		///<summary>59</summary>
		[Description("Reports - Graphical")]
		GraphicalReports = 59,
		///<summary>60</summary>
		[Description("Auto/Quick Note Edit")]
		AutoNoteQuickNoteEdit = 60,
		///<summary>61</summary>
		[Description("Equipment Setup")]
		EquipmentSetup = 61,
		///<summary>62</summary>
		[Description("Billing")]
		Billing = 62,
		///<summary>63</summary>
		[Description("Problem Definition Edit")]
		ProblemDefEdit = 63,
		///<summary>64- There is no user interface in the security window for this permission.  It is only used for tracking.</summary>
		[Description("Proc Fee Edit")]
		ProcFeeEdit = 64,
		///<summary>65- There is no user interface in the security window for this permission.  It is only used for tracking.  Only tracks changes to carriername, not any other carrier info.</summary>
		[Description("TP InsPlan Change Carrier Name")]
		InsPlanChangeCarrierName = 65,
		///<summary>66- (Was named TaskEdit prior to version 14.2.39) When editing an existing task: delete the task, edit original description, or double click on note rows.  Even if you don't have the permission, you can still edit your own task description (but not the notes) as long as it's in your inbox and as long as nobody but you has added any notes. </summary>
		[Description("Task Note Edit")]
		TaskNoteEdit = 66,
		///<summary>67- Add or delete lists and list columns..</summary>
		[Description("Wiki List Setup")]
		WikiListSetup = 67,
		///<summary>68- There is no user interface in the security window for this permission.  It is only used for tracking.  Tracks copying of patient information.  Required by EHR.</summary>
		[Description("Copy")]
		Copy = 68,
		///<summary>69- There is no user interface in the security window for this permission.  It is only used for tracking.  Tracks printing of patient information.  Required by EHR.</summary>
		[Description("Printing")]
		Printing = 69,
		///<summary>70- There is no user interface in the security window for this permission.  It is only used for tracking.  Tracks viewing of patient medical information.</summary>
		[Description("Medical Info Viewed")]
		MedicalInfoViewed = 70,
		///<summary>71- Tracks creation and editing of patient problems.</summary>
		[Description("Pat Problem List Edit")]
		PatProblemListEdit = 71,
		///<summary>72- Tracks creation and edting of patient medications.</summary>
		[Description("Pat Medication List Edit")]
		PatMedicationListEdit = 72,
		///<summary>73- Tracks creation and editing of patient allergies.</summary>
		[Description("Pat Allergy List Edit")]
		PatAllergyListEdit = 73,
		///<summary>74- There is no user interface in the security window for this permission.  It is only used for tracking.  Tracks creation and editing of patient family health history.</summary>
		[Description("Pat Family Health Edit")]
		PatFamilyHealthEdit = 74,
		///<summary>75- There is no user interface in the security window for this permission.  It is only used for tracking.  Patient Portal access of patient information.  Required by EHR.</summary>
		[Description("Patient Portal")]
		PatientPortal = 75,
		///<summary>76</summary>
		[Description("Rx Edit")]
		RxEdit = 76,
		///<summary>77- Assign this permission to a staff person who will administer setting up and editing Dental School Students in the system.</summary>
		[Description("Student Edit")]
		AdminDentalStudents = 77,
		///<summary>78- Assign this permission to an instructor who will be allowed to assign Grades to Dental School Students as well as manage classes assigned to them.</summary>
		[Description("Instructor Edit")]
		AdminDentalInstructors = 78,
		///<summary>79- Uses date restrictions.  Has a unique audit trail so that users can track specific ortho chart edits.</summary>
		[Description("Ortho Chart Edit (full)")]
		OrthoChartEditFull = 79,
		///<summary>80- There is no user interface in the security window for this permission.  It is only used for tracking.  Mainly used for ortho clinics.</summary>
		[Description("Patient Field Edit")]
		PatientFieldEdit = 80,
		///<summary>81- Assign this permission to a staff person who will edit evaluations in case of an emergency.  This is not meant to be a permanent permission given to a group.</summary>
		[Description("Admin Evaluation Edit")]
		AdminDentalEvaluations = 81,
		///<summary>82- There is no user interface in the security window for this permission.  It is only used for tracking.</summary>
		[Description("Treat Plan Discount Edit")]
		TreatPlanDiscountEdit = 82,
		///<summary>83- There is no user interface in the security window for this permission.  It is only used for tracking.</summary>
		[Description("User Log On Off")]
		UserLogOnOff = 83,
		///<summary>84- Allows user to edit other users' tasks.</summary>
		[Description("Task Edit")]
		TaskEdit = 84,
		///<summary>85- Allows user to send unsecured email</summary>
		[Description("Email Send")]
		EmailSend = 85,
		///<summary>86- Allows user to send webmail</summary>
		[Description("Webmail Send")]
		WebMailSend = 86,
		///<summary>87- Allows user to run, edit, and write non-released queries.</summary>
		[Description("User Query Admin")]
		UserQueryAdmin = 87,
		///<summary>88- Security permission for assignment of benefits.</summary>
		[Description("Insurance Plan Change Assignment of Benefits")]
		InsPlanChangeAssign = 88,
		///<summary>89- Uses date restrictions. Allows user to flip, rotate, resize, and crop image. Also allows editing of details on the "Item Info" window. SignedImageEdit permission is also needed to edit signed images.</summary>
		[Description("Image Edit")]
		ImageEdit = 89,
		///<summary>90- Allows editing of all measure events.  Also used to track changes made to events.</summary>
		[Description("EHR Measure Event Edit")]
		EhrMeasureEventEdit = 90,
		///<summary>91- Allows users to edit settings in the eServices Setup window.  Also causes the Listener Service monitor thread to start upon logging in.</summary>
		[Description("eServices Setup")]
		EServicesSetup = 91,
		///<summary>92- Allows users to edit Fee Schedules throughout the program.  Logs editing of fee schedule properties.</summary>
		[Description("Fee Schedule Edit")]
		FeeSchedEdit = 92,
		///<summary>93- Allows user to edit and delete provider specific fees overrides.</summary>
		[Description("Provider Fee Edit")]
		ProviderFeeEdit = 93,
		///<summary>94- Allows user to merge patients.</summary>
		[Description("Patient Merge")]
		PatientMerge = 94,
		///<summary>95- Only used in Claim History Status Edit</summary>
		[Description("Claim History Edit")]
		ClaimHistoryEdit = 95,
		///<summary>96- Allows user to edit a completed appointment. AppointmentCompleteDelete permission required in order to delete completed appointments.</summary>
		[Description("Completed Appointment Edit")]
		AppointmentCompleteEdit = 96,
		///<summary>97- Audit trail for deleting webmail messages.  There is no user interface in the security window for this permission.</summary>
		[Description("Webmail Delete")]
		WebMailDelete = 97,
		///<summary>98- Audit trail for saving a patient with required fields missing.  There is no user interface in the security window for this 
		///permission.</summary>
		[Description("Required Fields Missing")]
		RequiredFields = 98,
		///<summary>99- Allows user to merge referrals.</summary>
		[Description("Referral Merge")]
		ReferralMerge = 99,
		///<summary>100- There is no user interface in the security window for this permission.  It is only used for tracking.
		///Currently only used for tracking automatically changing the IsCpoe flag on procedures.  Can be enhanced to do more in the future.
		///There is only one place where we could have automatically changed IsCpoe without a corresponding log of a different permission.
		///That place is in the OnClosing of the Procedure Edit window.  We update this flag even when the user Cancels out of it.</summary>
		[Description("Proc Edit")]
		ProcEdit = 100,
		///<summary>101- Allows user to use the provider merge tool.</summary>
		[Description("Provider Merge")]
		ProviderMerge = 101,
		///<summary>102- Allows user to use the medication merge tool.</summary>
		[Description("Medication Merge")]
		MedicationMerge = 102,
		///<summary>103- Allow users to use the Quick Add tool in the Account module.</summary>
		[Description("Account Procs Quick Add")]
		AccountProcsQuickAdd = 103,
		///<summary>104- Allow users to send claims.</summary>
		[Description("Claim Send")]
		ClaimSend = 104,
		///<summary>105- Allow users to create new task lists.</summary>
		[Description("TaskList Create")]
		TaskListCreate = 105,
		///<summary>106 - Audit when a new patient is added.</summary>
		[Description("Patient Create")]
		PatientCreate = 106,
		///<summary>107- Allows changing the settings for graphical repots.</summary>
		[Description("Reports - Graphical Setup")]
		GraphicalReportSetup = 107,
		///<summary>108 - Audit when a patient is edited and restrict editing patients.</summary>
		[Description("Patient Edit")]
		PatientEdit = 108,
		///<summary>109 - Audit when an insurance plan is created.  Currently only used in X12 834 insurance plan import.</summary>
		[Description("Insurance Plan Create")]
		InsPlanCreate = 109,
		///<summary>110 - Audit when an insurance plan is edited.  Currently only used in X12 834 insurance plan import.</summary>
		[Description("Insurance Plan Edit")]
		InsPlanEdit = 110,
		///<summary>111 - InsSub Created. Currently only used in X12 834 insurance plan import and in API.</summary>
		[Description("Insurance Plan Create Subscriber")]
		InsPlanCreateSub = 111,
		///<summary>112 - Audit when an insurance subscriber is edited. Currently only used in X12 834 insurance plan import.</summary>
		[Description("Insurance Plan Edit Subscriber")]
		InsPlanEditSub = 112,
		///<summary>113 - Audit when a patient is added to an insurance plan.  Currently only used in X12 834 insurance plan import.</summary>
		[Description("Insurance Plan Add Patient")]
		InsPlanAddPat = 113,
		///<summary>114 - Audit when a patient is dropped from an insurance plan. Currently only used in X12 834 insurance plan import.</summary>
		[Description("Insurance Plan Drop Patient")]
		InsPlanDropPat = 114,
		///<summary>115 - Allows users to be assigned Insurance Verifications.</summary>
		[Description("Insurance Plan Verification Assign")]
		InsPlanVerifyList = 115,
		///<summary>116 - Allows users to bypass the global lock date to add paysplits.</summary>
		[Description("Pay Split Create after Global Lock Date")]
		SplitCreatePastLockDate = 116,
		///<summary>117 - DEPRECATED - Uses date restrictions.  Covers editing some fields of completed procs. </summary>
		[Description("Edit Completed Procedure (limited)")]
		ProcComplEditLimited = 117,
		///<summary>118 - Uses date restrictions based on the SecDateEntry field as the claim date.  Covers deleting a claim of any status
		///(Sent, Waiting to Send, Received, etc).</summary>
		[Description("Claim Delete")]
		ClaimDelete = 118,
		///<summary>119 - Covers editing the Write-off and Write-off Override fields for claimprocs as well as deleting/creating claimprocs.
		///<para>Uses date/days restriction based on the attached proc.DateEntryC; unless it's a total payment, then uses claimproc.SecDateEntry.</para>
		///<para>Applies to all plan types (i.e. PPO, Category%, Capitation, etc).</para></summary>
		[Description("Insurance Write-off Edit")]
		InsWriteOffEdit = 119,
		///<summary>120 - Allows users to change appointment confirmation status.</summary>
		[Description("Appointment Confirmation Status Edit")]
		ApptConfirmStatusEdit = 120,
		///<summary>121 - Audit trail for when users change graphical settings for another workstation in FormGraphics.cs.</summary>
		GraphicsRemoteEdit = 121,
		///<summary>122 - Audit Trail (Separated from SecurityAdmin permission)</summary>
		[Description("Audit Trail")]
		AuditTrail = 122,
		///<summary>123 - Allows the user to change the presenter on a treatment plan.</summary>
		[Description("Edit Treatment Plan Presenter")]
		TreatPlanPresenterEdit = 123,
		///<summary>124 - Allows users to use the Alphabetize Provider button from FormProviderSetup to permanently re-order providers.</summary>
		[Description("Providers Alphabetize")]
		ProviderAlphabetize = 124,
		///<summary>125 - Allows editing of claimprocs that are marked as received status.</summary>
		[Description("Claim Procedure Received Edit")]
		ClaimProcReceivedEdit = 125,
		///<summary>126 - Used to diagnose an error in statement creation. Audit Trail Permission Only</summary>
		StatementPatNumMismatch = 126,
		///<summary>127 - User has access to ODTouch.</summary>
		[Description("ODTouch/ODMobile")]
		MobileWeb = 127,
		///<summary>128 - For logging purposes only.  Used when PatPlans are created and not otherwise logged.</summary>
		PatPlanCreate = 128,
		///<summary>129 - Allows the user to change a patient's primary provider, with audit trail logging.</summary>
		[Description("Patient Primary Provider Edit")]
		PatPriProvEdit = 129,
		///<summary>130</summary>
		[Description("Referral Edit")]
		ReferralEdit = 130,
		///<summary>131 - Allows users to change a patient's billing type.</summary>
		[Description("Patient Billing Type Edit")]
		PatientBillingEdit = 131,
		///<summary>132 - Allows viewing annual prod inc of all providers instead of just a single provider.</summary>
		[Description("Production and Income - View All Providers")]
		ReportProdIncAllProviders = 132,
		///<summary>133 - Allows running daily reports. DEPRECATED.</summary>
		[Description("Reports - Daily")]
		ReportDaily = 133,
		///<summary>134 - Allows viewing daily prod inc of all providers instead of just a single provider</summary>
		[Description("Daily Reports - View All Providers")]
		ReportDailyAllProviders = 134,
		///<summary>135 - Allows user to change the appointment schedule flag.</summary>
		[Description("Patient Restriction Edit")]
		PatientApptRestrict = 135,
		///<summary>136 - Allows deleting sheets when they're associated to patients.</summary>
		[Description("Sheet Delete")]
		SheetDelete = 136,
		///<summary>137 - Allows updating custom tracking on claims.</summary>
		[Description("Update Custom Tracking")]
		UpdateCustomTracking = 137,
		///<summary>138 - Allows people to set graphics option for the workstation and other computers.</summary>
		[Description("Graphics Edit")]
		GraphicsEdit = 138,
		///<summary>139 - Allows user to change the fields within the Ortho tab of the Ins Plan Edit window.</summary>
		[Description("Insurance Plan Ortho Edit")]
		InsPlanOrthoEdit = 139,
		///<summary>140 - Allows user to change the provider on claimproc when claimproc is attached to a claim.</summary>
		[Description("Claim Procedure Provider Edit When Attached to Claim")]
		ClaimProcClaimAttachedProvEdit = 140,
		///<summary>141 - Audit when insurance plans are merged.</summary>
		[Description("Insurance Plan Combine")]
		InsPlanMerge = 141,
		///<summary>142 - Allows user to combine carriers.</summary>
		[Description("Insurance Carrier Combine")]
		InsCarrierCombine = 142,
		///<summary>143 - Allows user to edit popups. A user without this permission will still be able to edit their own popups.</summary>
		[Description("Popup Edit (other users)")]
		PopupEdit = 143,
		///<summary>144 - Allows user to select new insplan from list prior to dropping current insplan associated with a patplan.</summary>
		[Description("Change existing Ins Plan using Pick From List")]
		InsPlanPickListExisting = 144,
		///<summary>145 - Allows user to edit their own signed ortho charts even if they don't have full permission.</summary>
		[Description("Ortho Chart Edit (same user, signed)")]
		OrthoChartEditUser = 145,
		///<summary>146 - Allows user to edit procedure notes that they created themselves if they don't have full permission.</summary>
		[Description("Procedure Note (same user)")]
		ProcedureNoteUser = 146,
		///<summary>147 - Allows user to edit group notes signed by other users. If a user does not have this permission, they can still edit group notes
		///that they themselves have signed.</summary>
		[Description("Group Note Edit (other users, signed)")]
		GroupNoteEditSigned = 147,
		///<summary>148 - Allows user to lock and unlock wiki pages.  Also allows the user to edit locked wiki pages.</summary>
		[Description("Wiki Admin")]
		WikiAdmin = 148,
		///<summary>149 - Allows user to create, edit, close, and delete payment plans.</summary>
		[Description("Pay Plan Edit")]
		PayPlanEdit = 149,
		///<summary>150 - Used for logging when a claim is created, cancelled, or saved. </summary>
		ClaimEdit = 150,
		///<summary>151- Allows user to run command queries. Command queries are any non-SELECT queries for any non-temporary table.</summary>
		[Description("Command Query")]
		CommandQuery = 151,
		///<summary>152 - Gives user access to the replication setup window.</summary>
		[Description("Replication Setup")]
		ReplicationSetup = 152,
		///<summary>153 - Allows user to edit and delete sent and received pre-auths. Uses date restriction.</summary>
		[Description("PreAuth Sent Edit")]
		PreAuthSentEdit = 153,
		///<summary>154 - Edit fees (for logging only). Security log entry for this points to feeNum instead of CodeNum. </summary>
		LogFeeEdit = 154,
		///<summary>155 - Log ClaimProcEdit</summary>
		LogSubscriberEdit = 155,
		///<summary>156 - Logs changes to recalls, recalltypes, and recaltriggers.</summary>
		RecallEdit = 156,
		///<summary>157 - Allows users with this permission the ability to edit procedure codes.  Users with the Setup permission have this by default.
		///Logs changes made to individual proc codes (excluding fee changes) including when run from proc code tools.</summary>
		[Description("Procedure Code Edit")]
		ProcCodeEdit = 157,
		///<summary>158 - Allows users with this permission the ability to add new users. Security admins have this by default.</summary>
		[Description("Add New User")]
		AddNewUser = 158,
		///<summary>159 - Allows users with this permission the ability to view claims.</summary>
		[Description("Claim View")]
		ClaimView = 159,
		///<summary>160 - Allows users to run the Repeat Charge Tool.</summary>
		[Description("Repeating Charge Tool")]
		RepeatChargeTool = 160,
		///<summary>161 - Logs when a discount plan is added or dropped from a patient.</summary>
		DiscountPlanAddDrop = 161,
		///<summary>162 - Allows users with this permission the ability to sign treatment plans.</summary>
		[Description("Sign Treatment Plan")]
		TreatPlanSign = 162,
		///<summary>163 - Allows users with this permission to edit an existing EO or EC procedure.</summary>
		[Description("Edit EO or EC Procedures")]
		ProcExistingEdit = 163,
		///<summary>164 - Allows users to search for patients in all clinics even when they are restricted to clinics.
		///Also allows user to reassign patient clinic.</summary>
		[Description("Unrestricted Patient Search")]
		UnrestrictedSearch = 164,
		///<summary>165 - Allows users to edit patient information for archived patients. This really only stops editing inside Patient Edit window. Also see ArchivedPatientSelect. Blocking user from patient selection prevents changes to all the other tables.</summary>
		[Description("Archived Patient Edit")]
		ArchivedPatientEdit = 165,
		///<summary>169 - Allows user to set last verified dates for insurance benefits. Also allows access to FormInsVerificationList.</summary>
		[Description("Insurance Verification")]
		InsuranceVerification = 169,
		///<summary>170 - Logs when a credit card is moved from one patient to another.  Makes a log for both patients.  Audit Trail Permission Only.</summary>
		[Description("Credit Card Moved")]
		CreditCardMove = 170,
		///<summary>171 - Logs when aging is being ran and from where.</summary>
		[Description("Aging Ran")]
		AgingRan = 171,
		///<summary>173 - Allows user to view a specific Dashboard Widget.</summary>
		[Description("Dashboard Widget")]
		DashboardWidget = 173,
		///<summary>174 - Prevent users from creating bulk claims from the Procs Not Billed Report if past the lock date.</summary>
		[Description("Procedures Not Billed to Insurance, New Claims button")]
		NewClaimsProcNotBilled = 174,
		///<summary>175 - Logging into patient portal. Used for audit trail only.</summary>
		[Description("Patient Portal Login")]
		PatientPortalLogin = 175,
		///<summary>178- Logs when a reminder task is popped up.  Used for audit trail only.</summary>
		[Description("Task Reminder Popup")]
		TaskReminderPopup = 178,
		///<summary>179 - Logs when changes are made to supplemental backup settings inside the FormBackup window.</summary>
		SupplementalBackup = 179,
		/// <summary>180 - Logs when a user sends a Web Sched Recall through the Recall List. Used for audit trail only</summary>
		[Description("WebSched Recall Manually Sent")]
		WebSchedRecallManualSend = 180,
		/// <summary>181 - Allows the user to unmask patient SSN for temporary viewing.  Logs any unmasks in the audit trail</summary>
		[Description("Patient Social Security Number View")]
		PatientSSNView = 181,
		/// <summary>182 - Allows the user to unmask patient DOB for temporary viewing.  Logs any unmasks in the audit trail</summary>
		[Description("Patient Birthdate View")]
		PatientDOBView = 182,
		///<summary>183 - Logs when the family aging table has been truncated. For audit trails only.</summary>
		[Description("Truncate Family Aging")]
		FamAgingTruncate = 183,
		///<summary>184 - Logs when discount plans are merged. For audit trails only.</summary>
		[Description("Discount Plan Merged")]
		DiscountPlanMerge = 184,
		///<summary>185 - Uses date restrictions.  Allows user to change status of a completed procedure, or delete compeleted procedure</summary>
		[Description("Change Status or Delete a Completed Procedure")]
		ProcCompleteStatusEdit = 185,
		///<summary>186 - Allows user to add an adjustment to a procedure (date locked)</summary>
		[Description("Add Adjustment to Completed Procedure")]
		ProcCompleteAddAdj = 186,
		///<summary>187 - Misc Edit that includes "Do Not Bill Ins" and "Hide Graphics" (date locked)</summary>
		[Description("Miscellaneous edit on Completed Procedure")]
		ProcCompleteEditMisc = 187,
		///<summary>188 - Edit the note of a completed procedure</summary>
		[Description("Edit Note on Completed Procedure")]
		ProcCompleteNote = 188,
		///<summary>189 - Edit main information of a procedure that is not already covered by the other permissions. Is not all inclusive.</summary>
		[Description("Edit Completed Procedure")]
		ProcCompleteEdit = 189,
		///<summary>190 - User can create, edit, and delete time card adjustments for protected leave on their time card of the current pay period. Users that also have the Edit All Time Cards permission, have this permission for all time cards.</summary>
		[Description("Edit Protected Leave Time Card Adjustments")]
		ProtectedLeaveAdjustmentEdit = 190,
		///<summary>191 - Logs when a time card adjustment is created, edited, or deleted.</summary>
		[Description("Create, Edit, and Delete Time Card Adjustments")]
		TimeAdjustEdit = 191,
		///<summary>192 - Permission for users to monitor queries</summary>
		[Description("Query Monitor View")]
		QueryMonitor = 192,
		///<summary>193 - Permission for users to create commlogs.</summary>
		[Description("Commlog Create")]
		CommlogCreate = 193,
		///<summary>194 - Permission for users to modify and discard webforms</summary>
		[Description("Web Forms Access")]
		WebFormAccess = 194,
		///<summary>195 - Close other sessions of Open Dental Cloud</summary>
		[Description("Close Other Cloud Sessions")]
		CloseOtherSessions = 195,
		///<summary>196 - Permission for Repeating Charge creation.</summary>
		[Description("Repeating Charge Creation")]
		RepeatChargeCreate = 196,
		///<summary>197 - Permission for Repeating Charge update.</summary>	
		[Description("Repeating Charge Update")]
		RepeatChargeUpdate = 197,
		///<summary>198 - Permission for Repeating Charge deletion.</summary>
		[Description("Repeating Charge Deletion")]
		RepeatChargeDelete = 198,
		///<summary>199 - User can open the zoom window and edit zoom level. Used to block remote application users who all share the same computer.</summary>
		[Description("Zoom")]
		Zoom = 199,
		///<summary>200 - Permission for forms added to eclipboard mobile check in.</summary>
		[Description("Eclipboard Form Added")]
		FormAdded = 200,
		///<summary>201. Uses date restrictions.</summary>
		[Description("Image Export")]
		ImageExport = 201,
		///<summary>202. Permission to Scan, Import, and Create Images.</summary>
		[Description("Image Create")]
		ImageCreate = 202,
		///<summary>203 - Permission to update Employee Certifications.</summary>
		[Description("Certifications - Employee Completion")]
		CertificationEmployee = 203,
		///<summary>204 - Permission to set up Certifications.</summary>
		[Description("Certifications - Setup")]
		CertificationSetup = 204,
		///<summary>205 - Permission to create Employers.</summary>
		[Description("Employer - Create")]
		EmployerCreate = 205,
		///<summary>206 - Permission to allow users to login to ODCloud from any IP Address.</summary>
		[Description("Allow Login From Any Location")]
		AllowLoginFromAnyLocation = 206,
		///<summary>207 - Logging only. Creates an entry if a medicationpat.PatNote needs to be truncated before sending to DoseSpot.</summary>
		LogDoseSpotMedicationNoteEdit = 207,
		///<summary>208 - Allows user to edit a payment plan charge date that has an APR.</summary>
		[Description("Pay Plan Charge Date Edit")]
		PayPlanChargeDateEdit = 208,
		///<summary>209 - Logs when discount plans are added. For audit trails only.</summary>
		[Description("Discount Plan Add")]
		DiscountPlanAdd = 209,
		///<summary>210 - Logs when discount plans are edited. For audit trails only.</summary>
		[Description("Discount Plan Edit")]
		DiscountPlanEdit = 210,
		///<summary>211 - Permission to allow users without FeeSchedEdit permission to update fee schedule while receiving claims.</summary>
		[Description("Allow Editing Fee Schedule While Receiving Claims")]
		AllowFeeEditWhileReceivingClaim = 211,
		///<summary>212 - Permission for managing high security program properties.</summary>
		[Description("Manage High Security Program Properties")]
		ManageHighSecurityProgProperties = 212,
		///<summary>213 - Logs when a patient's credit card is edited.</summary>
		[Description("Credit Card Edit")]
		CreditCardEdit = 213,
		///<summary>214 - Allows user to edit medication definitions.</summary>
		[Description("Medication Definition Edit")]
		MedicationDefEdit = 214,
		///<summary>215 - Allows user to edit allergy definitions.</summary>
		[Description("Allergy Definition Edit")]
		AllergyDefEdit = 215,
		///<summary>216 - Allows user to setup and use Advertising features like Postcards.</summary>
		[Description("Advertising")]
		Advertising = 216,
		///<summary>217 - Allows user to view text messages.</summary>
		[Description("Text Message View")]
		TextMessageView = 217,
		///<summary>218 - Allows uer to send text messages.</summary>
		[Description("Text Message Send")]
		TextMessageSend = 218,
		///<summary>219 - Allows user to merge prescriptions.</summary>
		[Description("Rx Merge")]
		RxMerge = 219,
		///<summary>220 - Allows user to add or update Definitions.</summary>
		[Description("Definition Edit")]
		DefEdit = 220,
		///<summary>221 - Allows user to install Open Dental updates.</summary>
		[Description("Update Install")]
		UpdateInstall = 221,
		///<summary>222 - Denies users access to specific adjustment types. Special type of permission where having this permission actually 
		///denies users access. If a usergroup has an entry for this permission, then they do not have access to the adjustment type with the defnum
		///that is stored in grouppermission.FKey. Pattern approved by Jordan.</summary>
		[Description("Adjustment Type Deny")]
		AdjustmentTypeDeny = 222,
		///<summary>223 - Allows user to export statements as CSV files.</summary>
		[Description("Export CSV")]
		StatementCSV = 223,
		///<summary>224 - Allows users to edit carriers.</summary>
		[Description("Carrier Edit")]
		CarrierEdit = 224,
		///<summary>225 - Logs when API subscriptions are added or deleted. For audit trails only.</summary>
		[Description("API Subscription")]
		ApiSubscription = 225,
		///<summary>226 - Logs changes to global lock date. For audit trails only.</summary>
		[Description("Security Global")]
		SecurityGlobal = 226,
		///<summary>228 - Allows user to delete tasks.</summary>
		[Description("Task Delete")]
		TaskDelete = 228,
		///<summary>229 - Allows user to use setup wizard.</summary>
		[Description("Setup Wizard")]
		SetupWizard = 229,
		///<summary>230 - Allows user to use show features.</summary>
		[Description("Show Features")]
		ShowFeatures = 230,
		///<summary>231 - Allows user to setup printer.</summary>
		[Description("Printer Setup")]
		PrinterSetup = 231,
		///<summary>232 - Allows user to add provider.</summary>
		[Description("Provider Add")]
		ProviderAdd = 232,
		///<summary>233 - Allows user to edit clinic.</summary>
		[Description("Clinic Edit")]
		ClinicEdit = 233,
		///<summary>235 - Logs when registration keys are created. For audit trails only.</summary>
		[Description("Registration Key Create")]
		RegistrationKeyCreate = 235,
		///<summary>236 - Logs when registration keys are edited. For audit trails only.</summary>
		[Description("Registration Key Edit")]
		RegistrationKeyEdit = 236,
		///<summary>237 - Allows user to delete appointments.</summary>
		[Description("Appointment Delete")]
		AppointmentDelete = 237,
		///<summary>238 - Allows user to delete completed appointments.</summary>
		[Description("Completed Appointment Delete")]
		AppointmentCompleteDelete = 238,
		///<summary>239 - Logs when Appointment Types are edited. For audit trails only.</summary>
		[Description("Appointment Type Edit")]
		AppointmentTypeEdit = 239,
		///<summary>241 - Logs when web chat sessions are edited. For audit trails only.</summary>
		[Description("WebChat Edit")]
		WebChatEdit = 241,
		///<summary>242 - Allows users to access FormSuppliers</summary>
		[Description("Supplier Edit")]
		SupplierEdit = 242,
		///<summary>243 - Logs when any supply purchases are created, placed, or deleted.</summary>
		[Description("Supply Purchases")]
		SupplyPurchases = 243,
		///<summary>245 - Allows users to resize appointments.</summary>
		[Description("Appointment Resize")]
		AppointmentResize = 245,
		///<summary>246 - Logs when a user pays with a credit card. For Audit Trails only.</summary>
		[Description("Credit Card Terminal")]
		CreditCardTerminal = 246,
		///<summary>247 - Only for viewing the audit trail in FormEditAppointment</summary>
		[Description("View Appointment Audit Trail")]
		ViewAppointmentAuditTrail = 247,
		///<summary>248 - Logs when a user edits a payment plan charge.</summary>
		[Description("Pay Plan Charge Edit")]
		PayPlanChargeEdit = 248,
		///<summary>249 - Also see ArchivedPatientEdit. Blocking user from patient selection prevents changes to all the other tables besides the patient table.  It's more rigorous.</summary>
		[Description("Archived Patient Select")]
		ArchivedPatientSelect = 249,
		///<summary>252 - Ability to edit Fee Billed to Insurance in FormClaimProc</summary>
		[Description("Claim Procedure Fee Billed to Ins Edit")]
		ClaimProcFeeBilledToInsEdit = 252,
		///<summary>253 - Allow users to merge allergies.</summary>
		[Description("Allergy Merge")]
		AllergyMerge = 253,
		///<summary>255 - Allow users to edit BadgeIds in the userod table.</summary>
		[Description("BadgeId Edit")]
		BadgeIdEdit = 255,
		///<summary>256 - Internal Child Daycare only. Allow users to make changes to the daycare. Only used at HQ.</summary>
		[Description("Child Daycare Edit")]
		ChildDaycareEdit = 256,
		///<summary>257 - Allow users to copy perio charts in the Perio Chart window.</summary>
		[Description("Perio Chart Copy")]
		PerioEditCopy = 257,
		///<summary>258 - For audit trail only. Logs when a license is accepted by a user.</summary>
		[Description("License Accept")]
		LicenseAccept = 258,
		///<summary>259 - Uses date restrictions but no global lock date. Also used in audit trail to log importing.</summary>
		[Description("eForm Edit")]
		EFormEdit = 259,
		///<summary>260 - Allows deleting eForms when they're attached to patients. No date restrictions.</summary>
		[Description("eForm Delete")]
		EFormDelete = 260,
		///<summary>261 - Used for logging only. Can be used to log whenever mobile notifications are inserted into the database.</summary>
		[Description("Mobile Notifications")]
		MobileNotification = 261,
		///<summary>262 - Allows users to move chart views up and down, and add new chart views</summary>
		[Description("Chart View Edit")]
		ChartViewsEdit = 262,
		///<summary>263 - Allows disbanding of Super Families.</summary>
		[Description("Super Family Disband")]
		SuperFamilyDisband = 263,
		///<summary>264 - Allows creation of note and signature for images without a signature.</summary>
		[Description("Image Signature Create")]
		ImageSignatureCreate = 264,
		///<summary>265 - Allows editing and deletion of note and signature for images with a signature. Allows users with the ImageEdit permission to edit signed images. Allows users with the ImageDelete permission to delete signed images.</summary>
		[Description("Signed Image Edit")]
		SignedImageEdit = 265
	}
}


