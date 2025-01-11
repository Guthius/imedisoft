﻿
namespace OpenDental {
	partial class UserControlChartGeneral {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
		if(disposing && (components != null)) {
		components.Dispose();
		}
		base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.checkBoxRxClinicUseSelected = new OpenDental.UI.CheckBox();
			this.checkChartOrthoTabAutomaticCheckboxes = new OpenDental.UI.CheckBox();
			this.groupBoxFunctionality = new OpenDental.UI.GroupBox();
			this.comboProcCodeListSort = new OpenDental.UI.ComboBox();
			this.comboToothNomenclature = new OpenDental.UI.ComboBox();
			this.textMedDefaultStopDays = new System.Windows.Forms.TextBox();
			this.checkAutoClearEntryStatus = new OpenDental.UI.CheckBox();
			this.checkProvColorChart = new OpenDental.UI.CheckBox();
			this.label32 = new System.Windows.Forms.Label();
			this.labelToothNomenclature = new System.Windows.Forms.Label();
			this.checkIsAlertRadiologyProcsEnabled = new OpenDental.UI.CheckBox();
			this.label11 = new System.Windows.Forms.Label();
			this.checkScreeningsUseSheets = new OpenDental.UI.CheckBox();
			this.groupBoxPerio = new OpenDental.UI.GroupBox();
			this.checkPerioSkipMissingTeeth = new OpenDental.UI.CheckBox();
			this.checkPerioTreatImplantsAsNotMissing = new OpenDental.UI.CheckBox();
			this.groupBoxAppointments = new OpenDental.UI.GroupBox();
			this.checkChartNonPatientWarn = new OpenDental.UI.CheckBox();
			this.checkShowPlannedApptPrompt = new OpenDental.UI.CheckBox();
			this.groupBoxMedicalHistory = new OpenDental.UI.GroupBox();
			this.textProblemsIndicateNone = new System.Windows.Forms.TextBox();
			this.textMedicationsIndicateNone = new System.Windows.Forms.TextBox();
			this.textAllergiesIndicateNone = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.butProblemsIndicateNone = new OpenDental.UI.Button();
			this.butAllergiesIndicateNone = new OpenDental.UI.Button();
			this.butMedicationsIndicateNone = new OpenDental.UI.Button();
			this.groupBoxMedicalCodes = new OpenDental.UI.GroupBox();
			this.textICD9DefaultForNewProcs = new System.Windows.Forms.TextBox();
			this.checkMedicalFeeUsedForNewProcs = new OpenDental.UI.CheckBox();
			this.checkDxIcdVersion = new OpenDental.UI.CheckBox();
			this.labelIcdCodeDefault = new System.Windows.Forms.Label();
			this.butDiagnosisCode = new OpenDental.UI.Button();
			this.groupBoxFunctionality.SuspendLayout();
			this.groupBoxPerio.SuspendLayout();
			this.groupBoxAppointments.SuspendLayout();
			this.groupBoxMedicalHistory.SuspendLayout();
			this.groupBoxMedicalCodes.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxRxClinicUseSelected
			// 
			this.checkBoxRxClinicUseSelected.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBoxRxClinicUseSelected.Location = new System.Drawing.Point(3, 487);
			this.checkBoxRxClinicUseSelected.Name = "checkBoxRxClinicUseSelected";
			this.checkBoxRxClinicUseSelected.Size = new System.Drawing.Size(457, 17);
			this.checkBoxRxClinicUseSelected.TabIndex = 237;
			this.checkBoxRxClinicUseSelected.Text = "Rx use selected clinic from Clinics menu instead of selected patient\'s default cl" +
    "inic";
			// 
			// checkChartOrthoTabAutomaticCheckboxes
			// 
			this.checkChartOrthoTabAutomaticCheckboxes.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkChartOrthoTabAutomaticCheckboxes.Location = new System.Drawing.Point(84, 506);
			this.checkChartOrthoTabAutomaticCheckboxes.Name = "checkChartOrthoTabAutomaticCheckboxes";
			this.checkChartOrthoTabAutomaticCheckboxes.Size = new System.Drawing.Size(376, 31);
			this.checkChartOrthoTabAutomaticCheckboxes.TabIndex = 253;
			this.checkChartOrthoTabAutomaticCheckboxes.Text = "Automatically check and uncheck ortho mode and show grid checkboxes based on sele" +
    "ction of Ortho tab ";
			// 
			// groupBoxFunctionality
			// 
			this.groupBoxFunctionality.Controls.Add(this.comboProcCodeListSort);
			this.groupBoxFunctionality.Controls.Add(this.comboToothNomenclature);
			this.groupBoxFunctionality.Controls.Add(this.textMedDefaultStopDays);
			this.groupBoxFunctionality.Controls.Add(this.checkAutoClearEntryStatus);
			this.groupBoxFunctionality.Controls.Add(this.checkProvColorChart);
			this.groupBoxFunctionality.Controls.Add(this.label32);
			this.groupBoxFunctionality.Controls.Add(this.labelToothNomenclature);
			this.groupBoxFunctionality.Controls.Add(this.checkIsAlertRadiologyProcsEnabled);
			this.groupBoxFunctionality.Controls.Add(this.label11);
			this.groupBoxFunctionality.Controls.Add(this.checkScreeningsUseSheets);
			this.groupBoxFunctionality.Location = new System.Drawing.Point(20, 313);
			this.groupBoxFunctionality.Name = "groupBoxFunctionality";
			this.groupBoxFunctionality.Size = new System.Drawing.Size(450, 170);
			this.groupBoxFunctionality.TabIndex = 252;
			this.groupBoxFunctionality.Text = "Functionality";
			// 
			// comboProcCodeListSort
			// 
			this.comboProcCodeListSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboProcCodeListSort.Location = new System.Drawing.Point(185, 139);
			this.comboProcCodeListSort.Name = "comboProcCodeListSort";
			this.comboProcCodeListSort.Size = new System.Drawing.Size(255, 21);
			this.comboProcCodeListSort.TabIndex = 246;
			// 
			// comboToothNomenclature
			// 
			this.comboToothNomenclature.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboToothNomenclature.Location = new System.Drawing.Point(185, 95);
			this.comboToothNomenclature.Name = "comboToothNomenclature";
			this.comboToothNomenclature.Size = new System.Drawing.Size(255, 21);
			this.comboToothNomenclature.TabIndex = 241;
			// 
			// textMedDefaultStopDays
			// 
			this.textMedDefaultStopDays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textMedDefaultStopDays.Location = new System.Drawing.Point(401, 31);
			this.textMedDefaultStopDays.Name = "textMedDefaultStopDays";
			this.textMedDefaultStopDays.Size = new System.Drawing.Size(39, 20);
			this.textMedDefaultStopDays.TabIndex = 232;
			// 
			// checkAutoClearEntryStatus
			// 
			this.checkAutoClearEntryStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAutoClearEntryStatus.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkAutoClearEntryStatus.Location = new System.Drawing.Point(15, 76);
			this.checkAutoClearEntryStatus.Name = "checkAutoClearEntryStatus";
			this.checkAutoClearEntryStatus.Size = new System.Drawing.Size(425, 17);
			this.checkAutoClearEntryStatus.TabIndex = 240;
			this.checkAutoClearEntryStatus.Text = "Reset entry status to \'TreatPlan\' when switching patients";
			// 
			// checkProvColorChart
			// 
			this.checkProvColorChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkProvColorChart.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkProvColorChart.Location = new System.Drawing.Point(145, 120);
			this.checkProvColorChart.Name = "checkProvColorChart";
			this.checkProvColorChart.Size = new System.Drawing.Size(295, 17);
			this.checkProvColorChart.TabIndex = 243;
			this.checkProvColorChart.Text = "Use provider color in chart";
			// 
			// label32
			// 
			this.label32.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label32.Location = new System.Drawing.Point(50, 142);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(134, 17);
			this.label32.TabIndex = 247;
			this.label32.Text = "Procedure Code List sort";
			this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelToothNomenclature
			// 
			this.labelToothNomenclature.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelToothNomenclature.Location = new System.Drawing.Point(72, 97);
			this.labelToothNomenclature.Name = "labelToothNomenclature";
			this.labelToothNomenclature.Size = new System.Drawing.Size(112, 17);
			this.labelToothNomenclature.TabIndex = 242;
			this.labelToothNomenclature.Text = "Tooth Nomenclature";
			this.labelToothNomenclature.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsAlertRadiologyProcsEnabled
			// 
			this.checkIsAlertRadiologyProcsEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkIsAlertRadiologyProcsEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkIsAlertRadiologyProcsEnabled.Location = new System.Drawing.Point(6, 55);
			this.checkIsAlertRadiologyProcsEnabled.Name = "checkIsAlertRadiologyProcsEnabled";
			this.checkIsAlertRadiologyProcsEnabled.Size = new System.Drawing.Size(434, 17);
			this.checkIsAlertRadiologyProcsEnabled.TabIndex = 238;
			this.checkIsAlertRadiologyProcsEnabled.Text = "OpenDentalService alerts for scheduled non-CPOE radiology procedures";
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label11.Location = new System.Drawing.Point(39, 33);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(361, 17);
			this.label11.TabIndex = 234;
			this.label11.Text = "Medication Order default days until stop date";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkScreeningsUseSheets
			// 
			this.checkScreeningsUseSheets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkScreeningsUseSheets.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkScreeningsUseSheets.Location = new System.Drawing.Point(140, 10);
			this.checkScreeningsUseSheets.Name = "checkScreeningsUseSheets";
			this.checkScreeningsUseSheets.Size = new System.Drawing.Size(300, 17);
			this.checkScreeningsUseSheets.TabIndex = 235;
			this.checkScreeningsUseSheets.Text = "Screenings use Sheets";
			// 
			// groupBoxPerio
			// 
			this.groupBoxPerio.Controls.Add(this.checkPerioSkipMissingTeeth);
			this.groupBoxPerio.Controls.Add(this.checkPerioTreatImplantsAsNotMissing);
			this.groupBoxPerio.Location = new System.Drawing.Point(20, 255);
			this.groupBoxPerio.Name = "groupBoxPerio";
			this.groupBoxPerio.Size = new System.Drawing.Size(450, 54);
			this.groupBoxPerio.TabIndex = 251;
			this.groupBoxPerio.Text = "Perio";
			// 
			// checkPerioSkipMissingTeeth
			// 
			this.checkPerioSkipMissingTeeth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPerioSkipMissingTeeth.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkPerioSkipMissingTeeth.Location = new System.Drawing.Point(145, 10);
			this.checkPerioSkipMissingTeeth.Name = "checkPerioSkipMissingTeeth";
			this.checkPerioSkipMissingTeeth.Size = new System.Drawing.Size(295, 17);
			this.checkPerioSkipMissingTeeth.TabIndex = 244;
			this.checkPerioSkipMissingTeeth.Text = "Perio exams always skip missing teeth";
			// 
			// checkPerioTreatImplantsAsNotMissing
			// 
			this.checkPerioTreatImplantsAsNotMissing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPerioTreatImplantsAsNotMissing.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkPerioTreatImplantsAsNotMissing.Location = new System.Drawing.Point(145, 31);
			this.checkPerioTreatImplantsAsNotMissing.Name = "checkPerioTreatImplantsAsNotMissing";
			this.checkPerioTreatImplantsAsNotMissing.Size = new System.Drawing.Size(295, 17);
			this.checkPerioTreatImplantsAsNotMissing.TabIndex = 245;
			this.checkPerioTreatImplantsAsNotMissing.Text = "Perio exams treat implants as not missing";
			// 
			// groupBoxAppointments
			// 
			this.groupBoxAppointments.Controls.Add(this.checkChartNonPatientWarn);
			this.groupBoxAppointments.Controls.Add(this.checkShowPlannedApptPrompt);
			this.groupBoxAppointments.Location = new System.Drawing.Point(20, 193);
			this.groupBoxAppointments.Name = "groupBoxAppointments";
			this.groupBoxAppointments.Size = new System.Drawing.Size(450, 58);
			this.groupBoxAppointments.TabIndex = 250;
			this.groupBoxAppointments.Text = "Appointments";
			// 
			// checkChartNonPatientWarn
			// 
			this.checkChartNonPatientWarn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkChartNonPatientWarn.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkChartNonPatientWarn.Location = new System.Drawing.Point(105, 10);
			this.checkChartNonPatientWarn.Name = "checkChartNonPatientWarn";
			this.checkChartNonPatientWarn.Size = new System.Drawing.Size(335, 17);
			this.checkChartNonPatientWarn.TabIndex = 230;
			this.checkChartNonPatientWarn.Text = "Non-Patient warning";
			// 
			// checkShowPlannedApptPrompt
			// 
			this.checkShowPlannedApptPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkShowPlannedApptPrompt.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkShowPlannedApptPrompt.Location = new System.Drawing.Point(104, 31);
			this.checkShowPlannedApptPrompt.Name = "checkShowPlannedApptPrompt";
			this.checkShowPlannedApptPrompt.Size = new System.Drawing.Size(336, 17);
			this.checkShowPlannedApptPrompt.TabIndex = 239;
			this.checkShowPlannedApptPrompt.Text = "Prompt for Planned Appointment";
			// 
			// groupBoxMedicalHistory
			// 
			this.groupBoxMedicalHistory.Controls.Add(this.textProblemsIndicateNone);
			this.groupBoxMedicalHistory.Controls.Add(this.textMedicationsIndicateNone);
			this.groupBoxMedicalHistory.Controls.Add(this.textAllergiesIndicateNone);
			this.groupBoxMedicalHistory.Controls.Add(this.label8);
			this.groupBoxMedicalHistory.Controls.Add(this.label9);
			this.groupBoxMedicalHistory.Controls.Add(this.label14);
			this.groupBoxMedicalHistory.Controls.Add(this.butProblemsIndicateNone);
			this.groupBoxMedicalHistory.Controls.Add(this.butAllergiesIndicateNone);
			this.groupBoxMedicalHistory.Controls.Add(this.butMedicationsIndicateNone);
			this.groupBoxMedicalHistory.Location = new System.Drawing.Point(20, 10);
			this.groupBoxMedicalHistory.Name = "groupBoxMedicalHistory";
			this.groupBoxMedicalHistory.Size = new System.Drawing.Size(450, 93);
			this.groupBoxMedicalHistory.TabIndex = 232;
			this.groupBoxMedicalHistory.Text = "EHR Medical History";
			// 
			// textProblemsIndicateNone
			// 
			this.textProblemsIndicateNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textProblemsIndicateNone.Location = new System.Drawing.Point(265, 15);
			this.textProblemsIndicateNone.Name = "textProblemsIndicateNone";
			this.textProblemsIndicateNone.ReadOnly = true;
			this.textProblemsIndicateNone.Size = new System.Drawing.Size(146, 20);
			this.textProblemsIndicateNone.TabIndex = 219;
			// 
			// textMedicationsIndicateNone
			// 
			this.textMedicationsIndicateNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textMedicationsIndicateNone.Location = new System.Drawing.Point(265, 39);
			this.textMedicationsIndicateNone.Name = "textMedicationsIndicateNone";
			this.textMedicationsIndicateNone.ReadOnly = true;
			this.textMedicationsIndicateNone.Size = new System.Drawing.Size(146, 20);
			this.textMedicationsIndicateNone.TabIndex = 222;
			// 
			// textAllergiesIndicateNone
			// 
			this.textAllergiesIndicateNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textAllergiesIndicateNone.Location = new System.Drawing.Point(265, 63);
			this.textAllergiesIndicateNone.Name = "textAllergiesIndicateNone";
			this.textAllergiesIndicateNone.ReadOnly = true;
			this.textAllergiesIndicateNone.Size = new System.Drawing.Size(146, 20);
			this.textAllergiesIndicateNone.TabIndex = 226;
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.Location = new System.Drawing.Point(64, 18);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(200, 17);
			this.label8.TabIndex = 218;
			this.label8.Text = "Indicator patient has no problems";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label9.Location = new System.Drawing.Point(55, 42);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(209, 17);
			this.label9.TabIndex = 221;
			this.label9.Text = "Indicator patient has no medications";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label14
			// 
			this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label14.Location = new System.Drawing.Point(55, 66);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(209, 17);
			this.label14.TabIndex = 225;
			this.label14.Text = "Indicator patient has no allergies";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butProblemsIndicateNone
			// 
			this.butProblemsIndicateNone.Location = new System.Drawing.Point(419, 15);
			this.butProblemsIndicateNone.Name = "butProblemsIndicateNone";
			this.butProblemsIndicateNone.Size = new System.Drawing.Size(21, 21);
			this.butProblemsIndicateNone.TabIndex = 220;
			this.butProblemsIndicateNone.Text = "...";
			this.butProblemsIndicateNone.Click += new System.EventHandler(this.butProblemsIndicateNone_Click);
			// 
			// butAllergiesIndicateNone
			// 
			this.butAllergiesIndicateNone.Location = new System.Drawing.Point(419, 63);
			this.butAllergiesIndicateNone.Name = "butAllergiesIndicateNone";
			this.butAllergiesIndicateNone.Size = new System.Drawing.Size(21, 21);
			this.butAllergiesIndicateNone.TabIndex = 227;
			this.butAllergiesIndicateNone.Text = "...";
			this.butAllergiesIndicateNone.Click += new System.EventHandler(this.butAllergiesIndicateNone_Click);
			// 
			// butMedicationsIndicateNone
			// 
			this.butMedicationsIndicateNone.Location = new System.Drawing.Point(419, 39);
			this.butMedicationsIndicateNone.Name = "butMedicationsIndicateNone";
			this.butMedicationsIndicateNone.Size = new System.Drawing.Size(21, 21);
			this.butMedicationsIndicateNone.TabIndex = 223;
			this.butMedicationsIndicateNone.Text = "...";
			this.butMedicationsIndicateNone.Click += new System.EventHandler(this.butMedicationsIndicateNone_Click);
			// 
			// groupBoxMedicalCodes
			// 
			this.groupBoxMedicalCodes.Controls.Add(this.textICD9DefaultForNewProcs);
			this.groupBoxMedicalCodes.Controls.Add(this.checkMedicalFeeUsedForNewProcs);
			this.groupBoxMedicalCodes.Controls.Add(this.checkDxIcdVersion);
			this.groupBoxMedicalCodes.Controls.Add(this.labelIcdCodeDefault);
			this.groupBoxMedicalCodes.Controls.Add(this.butDiagnosisCode);
			this.groupBoxMedicalCodes.Location = new System.Drawing.Point(20, 107);
			this.groupBoxMedicalCodes.Name = "groupBoxMedicalCodes";
			this.groupBoxMedicalCodes.Size = new System.Drawing.Size(450, 82);
			this.groupBoxMedicalCodes.TabIndex = 249;
			this.groupBoxMedicalCodes.Text = "Medical Codes";
			// 
			// textICD9DefaultForNewProcs
			// 
			this.textICD9DefaultForNewProcs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textICD9DefaultForNewProcs.Location = new System.Drawing.Point(332, 52);
			this.textICD9DefaultForNewProcs.Name = "textICD9DefaultForNewProcs";
			this.textICD9DefaultForNewProcs.Size = new System.Drawing.Size(85, 20);
			this.textICD9DefaultForNewProcs.TabIndex = 229;
			// 
			// checkMedicalFeeUsedForNewProcs
			// 
			this.checkMedicalFeeUsedForNewProcs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkMedicalFeeUsedForNewProcs.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkMedicalFeeUsedForNewProcs.Location = new System.Drawing.Point(140, 10);
			this.checkMedicalFeeUsedForNewProcs.Name = "checkMedicalFeeUsedForNewProcs";
			this.checkMedicalFeeUsedForNewProcs.Size = new System.Drawing.Size(300, 17);
			this.checkMedicalFeeUsedForNewProcs.TabIndex = 228;
			this.checkMedicalFeeUsedForNewProcs.Text = "Use medical fee for new procedures";
			// 
			// checkDxIcdVersion
			// 
			this.checkDxIcdVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkDxIcdVersion.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkDxIcdVersion.Location = new System.Drawing.Point(104, 31);
			this.checkDxIcdVersion.Name = "checkDxIcdVersion";
			this.checkDxIcdVersion.Size = new System.Drawing.Size(336, 17);
			this.checkDxIcdVersion.TabIndex = 231;
			this.checkDxIcdVersion.Text = "Use ICD-10 Diagnosis Codes";
			this.checkDxIcdVersion.Click += new System.EventHandler(this.checkDxIcdVersion_Click);
			// 
			// labelIcdCodeDefault
			// 
			this.labelIcdCodeDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelIcdCodeDefault.Location = new System.Drawing.Point(3, 55);
			this.labelIcdCodeDefault.Name = "labelIcdCodeDefault";
			this.labelIcdCodeDefault.Size = new System.Drawing.Size(328, 17);
			this.labelIcdCodeDefault.TabIndex = 224;
			this.labelIcdCodeDefault.Text = "Default ICD-10 code for new procedures and when set complete";
			this.labelIcdCodeDefault.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butDiagnosisCode
			// 
			this.butDiagnosisCode.Location = new System.Drawing.Point(420, 52);
			this.butDiagnosisCode.Name = "butDiagnosisCode";
			this.butDiagnosisCode.Size = new System.Drawing.Size(21, 21);
			this.butDiagnosisCode.TabIndex = 233;
			this.butDiagnosisCode.Text = "...";
			this.butDiagnosisCode.Click += new System.EventHandler(this.butDiagnosisCode_Click);
			// 
			// UserControlChartGeneral
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.checkChartOrthoTabAutomaticCheckboxes);
			this.Controls.Add(this.groupBoxFunctionality);
			this.Controls.Add(this.groupBoxPerio);
			this.Controls.Add(this.groupBoxAppointments);
			this.Controls.Add(this.groupBoxMedicalHistory);
			this.Controls.Add(this.groupBoxMedicalCodes);
			this.Controls.Add(this.checkBoxRxClinicUseSelected);
			this.Name = "UserControlChartGeneral";
			this.Size = new System.Drawing.Size(494, 660);
			this.groupBoxFunctionality.ResumeLayout(false);
			this.groupBoxFunctionality.PerformLayout();
			this.groupBoxPerio.ResumeLayout(false);
			this.groupBoxAppointments.ResumeLayout(false);
			this.groupBoxMedicalHistory.ResumeLayout(false);
			this.groupBoxMedicalHistory.PerformLayout();
			this.groupBoxMedicalCodes.ResumeLayout(false);
			this.groupBoxMedicalCodes.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox textProblemsIndicateNone;
		private System.Windows.Forms.Label label8;
		private UI.Button butProblemsIndicateNone;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textMedicationsIndicateNone;
		private System.Windows.Forms.TextBox textAllergiesIndicateNone;
		private UI.Button butMedicationsIndicateNone;
		private System.Windows.Forms.TextBox textMedDefaultStopDays;
		private System.Windows.Forms.Label label11;
		private OpenDental.UI.CheckBox checkChartNonPatientWarn;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox textICD9DefaultForNewProcs;
		private System.Windows.Forms.Label labelIcdCodeDefault;
		private OpenDental.UI.CheckBox checkScreeningsUseSheets;
		private UI.Button butDiagnosisCode;
		private UI.Button butAllergiesIndicateNone;
		private OpenDental.UI.CheckBox checkDxIcdVersion;
		private OpenDental.UI.CheckBox checkMedicalFeeUsedForNewProcs;
		private OpenDental.UI.CheckBox checkShowPlannedApptPrompt;
		private OpenDental.UI.CheckBox checkIsAlertRadiologyProcsEnabled;
		private OpenDental.UI.CheckBox checkBoxRxClinicUseSelected;
		private UI.ComboBox comboToothNomenclature;
		private System.Windows.Forms.Label label32;
		private UI.ComboBox comboProcCodeListSort;
		private System.Windows.Forms.Label labelToothNomenclature;
		private OpenDental.UI.CheckBox checkPerioTreatImplantsAsNotMissing;
		private OpenDental.UI.CheckBox checkAutoClearEntryStatus;
		private OpenDental.UI.CheckBox checkPerioSkipMissingTeeth;
		private OpenDental.UI.CheckBox checkProvColorChart;
		private UI.GroupBox groupBoxMedicalCodes;
		private UI.GroupBox groupBoxMedicalHistory;
		private UI.GroupBox groupBoxAppointments;
		private UI.GroupBox groupBoxPerio;
		private UI.GroupBox groupBoxFunctionality;
		private OpenDental.UI.CheckBox checkChartOrthoTabAutomaticCheckboxes;
	}
}