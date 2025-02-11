using System.Windows.Forms;

namespace OpenDental.Features.Providers.Forms {
	public partial class FormProvEdit {
		private System.ComponentModel.IContainer components = null;// Required designer variable.

		
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProvEdit));
			this.butSave = new OpenDental.UI.Button();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.groupBox2 = new OpenDental.UI.GroupBox();
			this.gridProvIdent = new OpenDental.UI.GridOD();
			this.butAdd = new OpenDental.UI.Button();
			this.butDelete = new OpenDental.UI.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tabControlProvider = new OpenDental.UI.TabControl();
			this.tabGeneral = new OpenDental.UI.TabPage();
			this.textPreferredName = new System.Windows.Forms.TextBox();
			this.label25 = new System.Windows.Forms.Label();
			this.odColorPickerOutline = new OpenDental.UI.ODColorPicker();
			this.odColorPickerAppt = new OpenDental.UI.ODColorPicker();
			this.butClinicOverrides = new OpenDental.UI.Button();
			this.labelTermDate = new System.Windows.Forms.Label();
			this.textProdGoalHr = new OpenDental.ValidDouble();
			this.labelProdGoalHr = new System.Windows.Forms.Label();
			this.textBirthdate = new OpenDental.ValidDate();
			this.label22 = new System.Windows.Forms.Label();
			this.textSchedRules = new System.Windows.Forms.TextBox();
			this.labelSchedRules = new System.Windows.Forms.Label();
			this.checkIsHiddenOnReports = new OpenDental.UI.CheckBox();
			this.label20 = new System.Windows.Forms.Label();
			this.textProviderID = new System.Windows.Forms.TextBox();
			this.comboProv = new OpenDental.UI.ComboBox();
			this.label19 = new System.Windows.Forms.Label();
			this.checkIsNotPerson = new OpenDental.UI.CheckBox();
			this.checkIsCDAnet = new OpenDental.UI.CheckBox();
			this.textTaxonomyOverride = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textCanadianOfficeNum = new System.Windows.Forms.TextBox();
			this.labelCanadianOfficeNum = new System.Windows.Forms.Label();
			this.textNationalProvID = new System.Windows.Forms.TextBox();
			this.labelNPI = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.textMedicaidID = new System.Windows.Forms.TextBox();
			this.textLName = new System.Windows.Forms.TextBox();
			this.textFName = new System.Windows.Forms.TextBox();
			this.textMI = new System.Windows.Forms.TextBox();
			this.textSuffix = new System.Windows.Forms.TextBox();
			this.textAbbr = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.checkSigOnFile = new OpenDental.UI.CheckBox();
			this.groupBox1 = new OpenDental.UI.GroupBox();
			this.radioTIN = new System.Windows.Forms.RadioButton();
			this.radioSSN = new System.Windows.Forms.RadioButton();
			this.textSSN = new System.Windows.Forms.TextBox();
			this.listSpecialty = new System.Windows.Forms.ListBox();
			this.listFeeSched = new System.Windows.Forms.ListBox();
			this.checkIsSecondary = new OpenDental.UI.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.checkIsHidden = new OpenDental.UI.CheckBox();
			this.labelColor = new System.Windows.Forms.Label();
			this.dateTerm = new OpenDental.UI.ODDatePicker();
			this.tabSupplementalIDs = new OpenDental.UI.TabPage();
			this.groupBox2.SuspendLayout();
			this.tabControlProvider.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabSupplementalIDs.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(808, 629);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 35;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.ButtonSave_Click);
			// 
			// colorDialog1
			// 
			this.colorDialog1.FullOpen = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.gridProvIdent);
			this.groupBox2.Controls.Add(this.butAdd);
			this.groupBox2.Controls.Add(this.butDelete);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Location = new System.Drawing.Point(19, 15);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(496, 157);
			this.groupBox2.TabIndex = 33;
			this.groupBox2.Text = "Supplemental Provider Identifiers";
			// 
			// gridProvIdent
			// 
			this.gridProvIdent.Location = new System.Drawing.Point(11, 58);
			this.gridProvIdent.Name = "gridProvIdent";
			this.gridProvIdent.Size = new System.Drawing.Size(319, 88);
			this.gridProvIdent.TabIndex = 45;
			this.gridProvIdent.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.GridProvIdent_CellDoubleClick);
			// 
			// butAdd
			// 
			this.butAdd.Icon = OpenDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(360, 59);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(90, 24);
			this.butAdd.TabIndex = 0;
			this.butAdd.Text = "Add";
			this.butAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
			// 
			// butDelete
			// 
			this.butDelete.Icon = OpenDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(360, 94);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(90, 24);
			this.butDelete.TabIndex = 1;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(14, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(481, 32);
			this.label2.TabIndex = 44;
			this.label2.Text = "This is where you store provider IDs assigned by individual insurance companies, " + "especially BC/BS.";
			// 
			// tabControlProvider
			// 
			this.tabControlProvider.Controls.Add(this.tabGeneral);
			this.tabControlProvider.Controls.Add(this.tabSupplementalIDs);
			this.tabControlProvider.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabControlProvider.Location = new System.Drawing.Point(12, 12);
			this.tabControlProvider.Name = "tabControlProvider";
			this.tabControlProvider.Size = new System.Drawing.Size(870, 611);
			this.tabControlProvider.TabIndex = 268;
			// 
			// tabGeneral
			// 
			this.tabGeneral.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (252)))), ((int) (((byte) (253)))), ((int) (((byte) (254)))));
			this.tabGeneral.Controls.Add(this.textPreferredName);
			this.tabGeneral.Controls.Add(this.butClinicOverrides);
			this.tabGeneral.Controls.Add(this.label25);
			this.tabGeneral.Controls.Add(this.odColorPickerOutline);
			this.tabGeneral.Controls.Add(this.odColorPickerAppt);
			this.tabGeneral.Controls.Add(this.labelTermDate);
			this.tabGeneral.Controls.Add(this.textProdGoalHr);
			this.tabGeneral.Controls.Add(this.labelProdGoalHr);
			this.tabGeneral.Controls.Add(this.textBirthdate);
			this.tabGeneral.Controls.Add(this.label22);
			this.tabGeneral.Controls.Add(this.textSchedRules);
			this.tabGeneral.Controls.Add(this.labelSchedRules);
			this.tabGeneral.Controls.Add(this.checkIsHiddenOnReports);
			this.tabGeneral.Controls.Add(this.label20);
			this.tabGeneral.Controls.Add(this.textProviderID);
			this.tabGeneral.Controls.Add(this.comboProv);
			this.tabGeneral.Controls.Add(this.label19);
			this.tabGeneral.Controls.Add(this.checkIsNotPerson);
			this.tabGeneral.Controls.Add(this.checkIsCDAnet);
			this.tabGeneral.Controls.Add(this.textTaxonomyOverride);
			this.tabGeneral.Controls.Add(this.label4);
			this.tabGeneral.Controls.Add(this.textCanadianOfficeNum);
			this.tabGeneral.Controls.Add(this.labelCanadianOfficeNum);
			this.tabGeneral.Controls.Add(this.textNationalProvID);
			this.tabGeneral.Controls.Add(this.labelNPI);
			this.tabGeneral.Controls.Add(this.label14);
			this.tabGeneral.Controls.Add(this.textMedicaidID);
			this.tabGeneral.Controls.Add(this.textLName);
			this.tabGeneral.Controls.Add(this.textFName);
			this.tabGeneral.Controls.Add(this.textMI);
			this.tabGeneral.Controls.Add(this.textSuffix);
			this.tabGeneral.Controls.Add(this.textAbbr);
			this.tabGeneral.Controls.Add(this.label13);
			this.tabGeneral.Controls.Add(this.checkSigOnFile);
			this.tabGeneral.Controls.Add(this.groupBox1);
			this.tabGeneral.Controls.Add(this.listSpecialty);
			this.tabGeneral.Controls.Add(this.listFeeSched);
			this.tabGeneral.Controls.Add(this.checkIsSecondary);
			this.tabGeneral.Controls.Add(this.label10);
			this.tabGeneral.Controls.Add(this.label9);
			this.tabGeneral.Controls.Add(this.label8);
			this.tabGeneral.Controls.Add(this.label7);
			this.tabGeneral.Controls.Add(this.label6);
			this.tabGeneral.Controls.Add(this.label5);
			this.tabGeneral.Controls.Add(this.label1);
			this.tabGeneral.Controls.Add(this.checkIsHidden);
			this.tabGeneral.Controls.Add(this.labelColor);
			this.tabGeneral.Controls.Add(this.dateTerm);
			this.tabGeneral.Location = new System.Drawing.Point(2, 21);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.tabGeneral.Size = new System.Drawing.Size(866, 588);
			this.tabGeneral.TabIndex = 0;
			this.tabGeneral.Text = "General";
			// 
			// textPreferredName
			// 
			this.textPreferredName.Location = new System.Drawing.Point(202, 164);
			this.textPreferredName.MaxLength = 100;
			this.textPreferredName.Name = "textPreferredName";
			this.textPreferredName.Size = new System.Drawing.Size(161, 20);
			this.textPreferredName.TabIndex = 273;
			// 
			// label25
			// 
			this.label25.Location = new System.Drawing.Point(63, 168);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(138, 14);
			this.label25.TabIndex = 345;
			this.label25.Text = "Preferred Name";
			this.label25.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// odColorPickerOutline
			// 
			this.odColorPickerOutline.BackgroundColor = System.Drawing.Color.Empty;
			this.odColorPickerOutline.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.odColorPickerOutline.Location = new System.Drawing.Point(201, 496);
			this.odColorPickerOutline.Name = "odColorPickerOutline";
			this.odColorPickerOutline.Size = new System.Drawing.Size(74, 21);
			this.odColorPickerOutline.TabIndex = 343;
			// 
			// odColorPickerAppt
			// 
			this.odColorPickerAppt.BackgroundColor = System.Drawing.Color.Empty;
			this.odColorPickerAppt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.odColorPickerAppt.Location = new System.Drawing.Point(201, 475);
			this.odColorPickerAppt.Name = "odColorPickerAppt";
			this.odColorPickerAppt.Size = new System.Drawing.Size(74, 21);
			this.odColorPickerAppt.TabIndex = 342;
			// 
			// butClinicOverrides
			// 
			this.butClinicOverrides.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butClinicOverrides.Location = new System.Drawing.Point(264, 364);
			this.butClinicOverrides.Name = "butClinicOverrides";
			this.butClinicOverrides.Size = new System.Drawing.Size(47, 21);
			this.butClinicOverrides.TabIndex = 329;
			this.butClinicOverrides.Text = "Edit";
			this.butClinicOverrides.UseVisualStyleBackColor = true;
			this.butClinicOverrides.Click += new System.EventHandler(this.ButtonClinicOverrides_Click);
			// 
			// labelTermDate
			// 
			this.labelTermDate.Location = new System.Drawing.Point(106, 457);
			this.labelTermDate.Name = "labelTermDate";
			this.labelTermDate.Size = new System.Drawing.Size(95, 16);
			this.labelTermDate.TabIndex = 330;
			this.labelTermDate.Text = "Term Date";
			this.labelTermDate.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textProdGoalHr
			// 
			this.textProdGoalHr.Location = new System.Drawing.Point(202, 561);
			this.textProdGoalHr.MaxVal = 100000000D;
			this.textProdGoalHr.MinVal = -100000000D;
			this.textProdGoalHr.Name = "textProdGoalHr";
			this.textProdGoalHr.Size = new System.Drawing.Size(102, 20);
			this.textProdGoalHr.TabIndex = 291;
			// 
			// labelProdGoalHr
			// 
			this.labelProdGoalHr.Location = new System.Drawing.Point(33, 561);
			this.labelProdGoalHr.Name = "labelProdGoalHr";
			this.labelProdGoalHr.Size = new System.Drawing.Size(168, 21);
			this.labelProdGoalHr.TabIndex = 328;
			this.labelProdGoalHr.Text = "Hourly Production Goal";
			this.labelProdGoalHr.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBirthdate
			// 
			this.textBirthdate.Location = new System.Drawing.Point(202, 185);
			this.textBirthdate.Name = "textBirthdate";
			this.textBirthdate.Size = new System.Drawing.Size(102, 20);
			this.textBirthdate.TabIndex = 274;
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(54, 189);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(147, 14);
			this.label22.TabIndex = 326;
			this.label22.Text = "Birthdate";
			this.label22.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textSchedRules
			// 
			this.textSchedRules.AcceptsReturn = true;
			this.textSchedRules.Location = new System.Drawing.Point(419, 240);
			this.textSchedRules.Multiline = true;
			this.textSchedRules.Name = "textSchedRules";
			this.textSchedRules.Size = new System.Drawing.Size(330, 60);
			this.textSchedRules.TabIndex = 300;
			// 
			// labelSchedRules
			// 
			this.labelSchedRules.Location = new System.Drawing.Point(417, 223);
			this.labelSchedRules.Name = "labelSchedRules";
			this.labelSchedRules.Size = new System.Drawing.Size(170, 14);
			this.labelSchedRules.TabIndex = 325;
			this.labelSchedRules.Text = "Scheduling Note";
			this.labelSchedRules.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// checkIsHiddenOnReports
			// 
			this.checkIsHiddenOnReports.Location = new System.Drawing.Point(419, 510);
			this.checkIsHiddenOnReports.Name = "checkIsHiddenOnReports";
			this.checkIsHiddenOnReports.Size = new System.Drawing.Size(158, 17);
			this.checkIsHiddenOnReports.TabIndex = 309;
			this.checkIsHiddenOnReports.Text = "Hidden On Reports";
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(64, 41);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(136, 14);
			this.label20.TabIndex = 323;
			this.label20.Text = "Provider ID";
			this.label20.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textProviderID
			// 
			this.textProviderID.Location = new System.Drawing.Point(202, 38);
			this.textProviderID.MaxLength = 255;
			this.textProviderID.Name = "textProviderID";
			this.textProviderID.ReadOnly = true;
			this.textProviderID.Size = new System.Drawing.Size(121, 20);
			this.textProviderID.TabIndex = 322;
			this.textProviderID.TabStop = false;
			// 
			// comboProv
			// 
			this.comboProv.Location = new System.Drawing.Point(202, 539);
			this.comboProv.Name = "comboProv";
			this.comboProv.Size = new System.Drawing.Size(161, 21);
			this.comboProv.TabIndex = 290;
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(34, 539);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(168, 21);
			this.label19.TabIndex = 321;
			this.label19.Text = "Claim Billing Prov Override";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsNotPerson
			// 
			this.checkIsNotPerson.Location = new System.Drawing.Point(419, 478);
			this.checkIsNotPerson.Name = "checkIsNotPerson";
			this.checkIsNotPerson.Size = new System.Drawing.Size(410, 17);
			this.checkIsNotPerson.TabIndex = 307;
			this.checkIsNotPerson.Text = "Not a Person (for example, a dummy provider representing the organization)";
			// 
			// checkIsCDAnet
			// 
			this.checkIsCDAnet.Location = new System.Drawing.Point(419, 430);
			this.checkIsCDAnet.Name = "checkIsCDAnet";
			this.checkIsCDAnet.Size = new System.Drawing.Size(168, 17);
			this.checkIsCDAnet.TabIndex = 304;
			this.checkIsCDAnet.Text = "Is CDAnet Member";
			this.checkIsCDAnet.Visible = false;
			// 
			// textTaxonomyOverride
			// 
			this.textTaxonomyOverride.Location = new System.Drawing.Point(595, 318);
			this.textTaxonomyOverride.MaxLength = 255;
			this.textTaxonomyOverride.Name = "textTaxonomyOverride";
			this.textTaxonomyOverride.Size = new System.Drawing.Size(154, 20);
			this.textTaxonomyOverride.TabIndex = 302;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(592, 301);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(154, 14);
			this.label4.TabIndex = 315;
			this.label4.Text = "Taxonomy Code Override";
			this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textCanadianOfficeNum
			// 
			this.textCanadianOfficeNum.Location = new System.Drawing.Point(202, 433);
			this.textCanadianOfficeNum.MaxLength = 20;
			this.textCanadianOfficeNum.Name = "textCanadianOfficeNum";
			this.textCanadianOfficeNum.Size = new System.Drawing.Size(102, 20);
			this.textCanadianOfficeNum.TabIndex = 283;
			// 
			// labelCanadianOfficeNum
			// 
			this.labelCanadianOfficeNum.Location = new System.Drawing.Point(60, 437);
			this.labelCanadianOfficeNum.Name = "labelCanadianOfficeNum";
			this.labelCanadianOfficeNum.Size = new System.Drawing.Size(141, 14);
			this.labelCanadianOfficeNum.TabIndex = 314;
			this.labelCanadianOfficeNum.Text = "Office Number";
			this.labelCanadianOfficeNum.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textNationalProvID
			// 
			this.textNationalProvID.Location = new System.Drawing.Point(202, 412);
			this.textNationalProvID.MaxLength = 20;
			this.textNationalProvID.Name = "textNationalProvID";
			this.textNationalProvID.Size = new System.Drawing.Size(102, 20);
			this.textNationalProvID.TabIndex = 282;
			// 
			// labelNPI
			// 
			this.labelNPI.Location = new System.Drawing.Point(60, 416);
			this.labelNPI.Name = "labelNPI";
			this.labelNPI.Size = new System.Drawing.Size(141, 14);
			this.labelNPI.TabIndex = 313;
			this.labelNPI.Text = "National Provider ID";
			this.labelNPI.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(62, 499);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(140, 16);
			this.label14.TabIndex = 312;
			this.label14.Text = "Highlight Outline Color";
			this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textMedicaidID
			// 
			this.textMedicaidID.Location = new System.Drawing.Point(202, 391);
			this.textMedicaidID.MaxLength = 20;
			this.textMedicaidID.Name = "textMedicaidID";
			this.textMedicaidID.Size = new System.Drawing.Size(102, 20);
			this.textMedicaidID.TabIndex = 280;
			// 
			// textLName
			// 
			this.textLName.Location = new System.Drawing.Point(202, 80);
			this.textLName.MaxLength = 100;
			this.textLName.Name = "textLName";
			this.textLName.Size = new System.Drawing.Size(161, 20);
			this.textLName.TabIndex = 269;
			// 
			// textFName
			// 
			this.textFName.Location = new System.Drawing.Point(202, 101);
			this.textFName.MaxLength = 100;
			this.textFName.Name = "textFName";
			this.textFName.Size = new System.Drawing.Size(161, 20);
			this.textFName.TabIndex = 270;
			// 
			// textMI
			// 
			this.textMI.Location = new System.Drawing.Point(202, 122);
			this.textMI.MaxLength = 100;
			this.textMI.Name = "textMI";
			this.textMI.Size = new System.Drawing.Size(63, 20);
			this.textMI.TabIndex = 271;
			// 
			// textSuffix
			// 
			this.textSuffix.Location = new System.Drawing.Point(202, 143);
			this.textSuffix.MaxLength = 100;
			this.textSuffix.Name = "textSuffix";
			this.textSuffix.Size = new System.Drawing.Size(102, 20);
			this.textSuffix.TabIndex = 272;
			// 
			// textAbbr
			// 
			this.textAbbr.Location = new System.Drawing.Point(202, 59);
			this.textAbbr.MaxLength = 255;
			this.textAbbr.Name = "textAbbr";
			this.textAbbr.Size = new System.Drawing.Size(121, 20);
			this.textAbbr.TabIndex = 268;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(60, 395);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(141, 14);
			this.label13.TabIndex = 311;
			this.label13.Text = "Medicaid ID";
			this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// checkSigOnFile
			// 
			this.checkSigOnFile.Location = new System.Drawing.Point(419, 462);
			this.checkSigOnFile.Name = "checkSigOnFile";
			this.checkSigOnFile.Size = new System.Drawing.Size(121, 17);
			this.checkSigOnFile.TabIndex = 306;
			this.checkSigOnFile.Text = "Signature on File";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.radioTIN);
			this.groupBox1.Controls.Add(this.radioSSN);
			this.groupBox1.Controls.Add(this.textSSN);
			this.groupBox1.Location = new System.Drawing.Point(194, 211);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(156, 80);
			this.groupBox1.TabIndex = 275;
			this.groupBox1.Text = "SSN or TIN (no dashes)";
			// 
			// radioTIN
			// 
			this.radioTIN.Location = new System.Drawing.Point(13, 35);
			this.radioTIN.Name = "radioTIN";
			this.radioTIN.Size = new System.Drawing.Size(104, 18);
			this.radioTIN.TabIndex = 1;
			this.radioTIN.Text = "TIN";
			this.radioTIN.Click += new System.EventHandler(this.RadioButtonTin_Click);
			// 
			// radioSSN
			// 
			this.radioSSN.Checked = true;
			this.radioSSN.Location = new System.Drawing.Point(13, 15);
			this.radioSSN.Name = "radioSSN";
			this.radioSSN.Size = new System.Drawing.Size(104, 18);
			this.radioSSN.TabIndex = 0;
			this.radioSSN.TabStop = true;
			this.radioSSN.Text = "SSN";
			this.radioSSN.Click += new System.EventHandler(this.RadioButtonSsn_Click);
			// 
			// textSSN
			// 
			this.textSSN.Location = new System.Drawing.Point(8, 54);
			this.textSSN.Name = "textSSN";
			this.textSSN.Size = new System.Drawing.Size(102, 20);
			this.textSSN.TabIndex = 2;
			// 
			// listSpecialty
			// 
			this.listSpecialty.Location = new System.Drawing.Point(598, 34);
			this.listSpecialty.Name = "listSpecialty";
			this.listSpecialty.Size = new System.Drawing.Size(154, 186);
			this.listSpecialty.TabIndex = 297;
			// 
			// listFeeSched
			// 
			this.listFeeSched.Location = new System.Drawing.Point(419, 34);
			this.listFeeSched.Name = "listFeeSched";
			this.listFeeSched.Size = new System.Drawing.Size(168, 186);
			this.listFeeSched.TabIndex = 296;
			// 
			// checkIsSecondary
			// 
			this.checkIsSecondary.Location = new System.Drawing.Point(419, 446);
			this.checkIsSecondary.Name = "checkIsSecondary";
			this.checkIsSecondary.Size = new System.Drawing.Size(155, 17);
			this.checkIsSecondary.TabIndex = 305;
			this.checkIsSecondary.Text = "Secondary Provider (Hyg)";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(57, 84);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(143, 14);
			this.label10.TabIndex = 298;
			this.label10.Text = "Last Name";
			this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(54, 147);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(147, 14);
			this.label9.TabIndex = 295;
			this.label9.Text = "Suffix (MD,DMD,DDS,etc)";
			this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(63, 105);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(138, 14);
			this.label8.TabIndex = 293;
			this.label8.Text = "First Name";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(94, 126);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(102, 14);
			this.label7.TabIndex = 338;
			this.label7.Text = "MI";
			this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(417, 17);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(105, 14);
			this.label6.TabIndex = 289;
			this.label6.Text = "Fee Schedule";
			this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(598, 17);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(116, 14);
			this.label5.TabIndex = 287;
			this.label5.Text = "Specialty";
			this.label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(65, 63);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 14);
			this.label1.TabIndex = 281;
			this.label1.Text = "Abbreviation";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// checkIsHidden
			// 
			this.checkIsHidden.Location = new System.Drawing.Point(419, 494);
			this.checkIsHidden.Name = "checkIsHidden";
			this.checkIsHidden.Size = new System.Drawing.Size(158, 17);
			this.checkIsHidden.TabIndex = 308;
			this.checkIsHidden.Text = "Hidden";
			// 
			// labelColor
			// 
			this.labelColor.Location = new System.Drawing.Point(62, 478);
			this.labelColor.Name = "labelColor";
			this.labelColor.Size = new System.Drawing.Size(140, 16);
			this.labelColor.TabIndex = 278;
			this.labelColor.Text = "Appointment Color";
			this.labelColor.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// dateTerm
			// 
			this.dateTerm.BackColor = System.Drawing.SystemColors.Window;
			this.dateTerm.CalendarLocation = OpenDental.UI.ODDatePicker.EnumCalendarLocation.ToTheRight;
			this.dateTerm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.dateTerm.Location = new System.Drawing.Point(139, 453);
			this.dateTerm.Name = "dateTerm";
			this.dateTerm.Size = new System.Drawing.Size(228, 21);
			this.dateTerm.TabIndex = 339;
			// 
			// tabSupplementalIDs
			// 
			this.tabSupplementalIDs.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (252)))), ((int) (((byte) (253)))), ((int) (((byte) (254)))));
			this.tabSupplementalIDs.Controls.Add(this.groupBox2);
			this.tabSupplementalIDs.Location = new System.Drawing.Point(2, 21);
			this.tabSupplementalIDs.Name = "tabSupplementalIDs";
			this.tabSupplementalIDs.Padding = new System.Windows.Forms.Padding(3);
			this.tabSupplementalIDs.Size = new System.Drawing.Size(866, 588);
			this.tabSupplementalIDs.TabIndex = 1;
			this.tabSupplementalIDs.Text = "Supplemental IDs";
			// 
			// FormProvEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(895, 665);
			this.Controls.Add(this.tabControlProvider);
			this.Controls.Add(this.butSave);
			this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProvEdit";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Edit Provider";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormProvEdit_Closing);
			this.Load += new System.EventHandler(this.FormProvEdit_Load);
			this.groupBox2.ResumeLayout(false);
			this.tabControlProvider.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.tabGeneral.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tabSupplementalIDs.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		#region UI Elements
		private OpenDental.UI.Button butSave;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private OpenDental.UI.GroupBox groupBox2;
		private System.Windows.Forms.Label label2;
		private OpenDental.UI.Button butDelete;
		private OpenDental.UI.Button butAdd;
		private OpenDental.UI.TabControl tabControlProvider;
		private OpenDental.UI.TabPage tabGeneral;
		private ValidDate textBirthdate;
		private Label label22;
		private TextBox textSchedRules;
		private Label labelSchedRules;
		private OpenDental.UI.CheckBox checkIsHiddenOnReports;
		private Label label20;
		private TextBox textProviderID;
		private UI.ComboBox comboProv;
		private Label label19;
		private OpenDental.UI.CheckBox checkIsNotPerson;
		private OpenDental.UI.CheckBox checkIsCDAnet;
		private TextBox textTaxonomyOverride;
		private Label label4;
		private TextBox textCanadianOfficeNum;
		private Label labelCanadianOfficeNum;
		private TextBox textNationalProvID;
		private Label labelNPI;
		private Label label14;
		private TextBox textMedicaidID;
		private TextBox textLName;
		private TextBox textFName;
		private TextBox textMI;
		private TextBox textSuffix;
		private TextBox textAbbr;
		private Label label13;
		private OpenDental.UI.CheckBox checkSigOnFile;
		private OpenDental.UI.GroupBox groupBox1;
		private RadioButton radioTIN;
		private RadioButton radioSSN;
		private TextBox textSSN;
		private System.Windows.Forms.ListBox listSpecialty;
		private System.Windows.Forms.ListBox listFeeSched;
		private OpenDental.UI.CheckBox checkIsSecondary;
		private Label label10;
		private Label label9;
		private Label label8;
		private Label label7;
		private Label label6;
		private Label label5;
		private Label label1;
		private OpenDental.UI.CheckBox checkIsHidden;
		private Label labelColor;
		private OpenDental.UI.TabPage tabSupplementalIDs;
		private ValidDouble textProdGoalHr;
		private Label labelProdGoalHr;
		private Label labelTermDate;
		private UI.ODDatePicker dateTerm;
		private OpenDental.UI.Button butClinicOverrides;
		private UI.ODColorPicker odColorPickerOutline;
		private UI.ODColorPicker odColorPickerAppt;
		private TextBox textPreferredName;
		private Label label25;
		private UI.GridOD gridProvIdent;
		#endregion UI Elements
	}
}
