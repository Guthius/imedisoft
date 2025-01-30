namespace OpenDental{
	partial class FormEServicesMobileCredentials {
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEServicesMobileCredentials));
			this.butVerifyAndSave = new OpenDental.UI.Button();
			this.comboBoxClinicPicker1 = new OpenDental.UI.ComboBoxClinicPicker();
			this.label1 = new System.Windows.Forms.Label();
			this.textUserName = new System.Windows.Forms.TextBox();
			this.groupAccountRecovery = new OpenDental.UI.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textValidPhone = new OpenDental.ValidPhone();
			this.label5 = new System.Windows.Forms.Label();
			this.textEmail = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.textConfirmPassword = new System.Windows.Forms.TextBox();
			this.labelPassword = new System.Windows.Forms.Label();
			this.labelConfirmPassword = new System.Windows.Forms.Label();
			this.labelPermissionRequired = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.groupAccountRecovery.SuspendLayout();
			this.SuspendLayout();
			// 
			// butVerifyAndSave
			// 
			this.butVerifyAndSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butVerifyAndSave.Location = new System.Drawing.Point(261, 349);
			this.butVerifyAndSave.Name = "butVerifyAndSave";
			this.butVerifyAndSave.Size = new System.Drawing.Size(109, 24);
			this.butVerifyAndSave.TabIndex = 3;
			this.butVerifyAndSave.Text = "Verify and Save";
			this.butVerifyAndSave.Click += new System.EventHandler(this.butVerifyAndSave_Click);
			// 
			// comboBoxClinicPicker1
			// 
			this.comboBoxClinicPicker1.Location = new System.Drawing.Point(21, 66);
			this.comboBoxClinicPicker1.Name = "comboBoxClinicPicker1";
			this.comboBoxClinicPicker1.Size = new System.Drawing.Size(268, 21);
			this.comboBoxClinicPicker1.TabIndex = 4;
			this.comboBoxClinicPicker1.SelectionChangeCommitted += new System.EventHandler(this.comboBoxClinicPicker1_SelectionChangeCommitted);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(36, 101);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 18);
			this.label1.TabIndex = 5;
			this.label1.Text = "User Name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textUserName
			// 
			this.textUserName.Location = new System.Drawing.Point(133, 100);
			this.textUserName.Name = "textUserName";
			this.textUserName.Size = new System.Drawing.Size(156, 20);
			this.textUserName.TabIndex = 1;
			// 
			// groupAccountRecovery
			// 
			this.groupAccountRecovery.Controls.Add(this.label3);
			this.groupAccountRecovery.Controls.Add(this.textValidPhone);
			this.groupAccountRecovery.Controls.Add(this.label5);
			this.groupAccountRecovery.Controls.Add(this.textEmail);
			this.groupAccountRecovery.Controls.Add(this.label2);
			this.groupAccountRecovery.Location = new System.Drawing.Point(21, 178);
			this.groupAccountRecovery.Name = "groupAccountRecovery";
			this.groupAccountRecovery.Size = new System.Drawing.Size(268, 118);
			this.groupAccountRecovery.TabIndex = 14;
			this.groupAccountRecovery.Text = "Account Recovery";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(6, 18);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(256, 38);
			this.label3.TabIndex = 15;
			this.label3.Text = "The below information is also used for verifying the user\'s identity when initial" +
    "ly registering.";
			// 
			// textValidPhone
			// 
			this.textValidPhone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textValidPhone.Location = new System.Drawing.Point(112, 85);
			this.textValidPhone.Name = "textValidPhone";
			this.textValidPhone.Size = new System.Drawing.Size(150, 20);
			this.textValidPhone.TabIndex = 5;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(10, 87);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 18);
			this.label5.TabIndex = 14;
			this.label5.Text = "Phone Number";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textEmail
			// 
			this.textEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textEmail.Location = new System.Drawing.Point(112, 59);
			this.textEmail.Name = "textEmail";
			this.textEmail.Size = new System.Drawing.Size(150, 20);
			this.textEmail.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(10, 61);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 18);
			this.label2.TabIndex = 12;
			this.label2.Text = "Email Address";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPassword
			// 
			this.textPassword.Location = new System.Drawing.Point(133, 126);
			this.textPassword.Name = "textPassword";
			this.textPassword.PasswordChar = '*';
			this.textPassword.Size = new System.Drawing.Size(156, 20);
			this.textPassword.TabIndex = 2;
			// 
			// textConfirmPassword
			// 
			this.textConfirmPassword.Location = new System.Drawing.Point(133, 152);
			this.textConfirmPassword.Name = "textConfirmPassword";
			this.textConfirmPassword.PasswordChar = '*';
			this.textConfirmPassword.Size = new System.Drawing.Size(156, 20);
			this.textConfirmPassword.TabIndex = 3;
			// 
			// labelPassword
			// 
			this.labelPassword.Location = new System.Drawing.Point(36, 128);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(95, 18);
			this.labelPassword.TabIndex = 17;
			this.labelPassword.Text = "Password";
			this.labelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelConfirmPassword
			// 
			this.labelConfirmPassword.Location = new System.Drawing.Point(22, 152);
			this.labelConfirmPassword.Name = "labelConfirmPassword";
			this.labelConfirmPassword.Size = new System.Drawing.Size(109, 18);
			this.labelConfirmPassword.TabIndex = 18;
			this.labelConfirmPassword.Text = "Re-Enter Password";
			this.labelConfirmPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPermissionRequired
			// 
			this.labelPermissionRequired.Location = new System.Drawing.Point(18, 299);
			this.labelPermissionRequired.Name = "labelPermissionRequired";
			this.labelPermissionRequired.Size = new System.Drawing.Size(271, 27);
			this.labelPermissionRequired.TabIndex = 19;
			this.labelPermissionRequired.Text = "EServicesSetup permission is required to set up / modify mobile settings.";
			this.labelPermissionRequired.Visible = false;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(18, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(359, 49);
			this.label4.TabIndex = 20;
			this.label4.Text = "These credentials are for ODTouch, eClipboard, and ODMobile.\r\nThey let the office" +
    " connect to the proper database and clinic.\r\nUsers of those apps will still need" +
    " individual user credentials.";
			// 
			// FormEServicesMobileSettings
			// 
			this.ClientSize = new System.Drawing.Size(382, 385);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.labelPermissionRequired);
			this.Controls.Add(this.textConfirmPassword);
			this.Controls.Add(this.labelConfirmPassword);
			this.Controls.Add(this.labelPassword);
			this.Controls.Add(this.textPassword);
			this.Controls.Add(this.groupAccountRecovery);
			this.Controls.Add(this.textUserName);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBoxClinicPicker1);
			this.Controls.Add(this.butVerifyAndSave);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormEServicesMobileSettings";
			this.Text = "Mobile App Credentials";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEServicesMobileSettings_FormClosing);
			this.Load += new System.EventHandler(this.FormEServicesMobileCredentials_Load);
			this.groupAccountRecovery.ResumeLayout(false);
			this.groupAccountRecovery.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenDental.UI.Button butVerifyAndSave;
		private UI.ComboBoxClinicPicker comboBoxClinicPicker1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textUserName;
		private OpenDental.UI.GroupBox groupAccountRecovery;
		private ValidPhone textValidPhone;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textEmail;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textPassword;
		private System.Windows.Forms.TextBox textConfirmPassword;
		private System.Windows.Forms.Label labelPassword;
		private System.Windows.Forms.Label labelConfirmPassword;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label labelPermissionRequired;
		private System.Windows.Forms.Label label4;
	}
}