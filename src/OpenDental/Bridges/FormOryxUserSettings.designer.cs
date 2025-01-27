namespace OpenDental{
	partial class FormOryxUserSettings {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOryxUserSettings));
			this.butSave = new OpenDental.UI.Button();
			this.textUsername = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(306, 86);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textUsername
			// 
			this.textUsername.Location = new System.Drawing.Point(162, 20);
			this.textUsername.Name = "textUsername";
			this.textUsername.Size = new System.Drawing.Size(187, 20);
			this.textUsername.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(81, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(79, 20);
			this.label4.TabIndex = 4;
			this.label4.Text = "Password";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPassword
			// 
			this.textPassword.Location = new System.Drawing.Point(162, 46);
			this.textPassword.Name = "textPassword";
			this.textPassword.PasswordChar = '*';
			this.textPassword.Size = new System.Drawing.Size(187, 20);
			this.textPassword.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(84, 20);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 20);
			this.label3.TabIndex = 5;
			this.label3.Text = "Username";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormOryxUserSettings
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(393, 122);
			this.Controls.Add(this.textUsername);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textPassword);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.butSave);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormOryxUserSettings";
			this.Text = "Oryx User Settings";
			this.Load += new System.EventHandler(this.FormUserSetting_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenDental.UI.Button butSave;
		private System.Windows.Forms.TextBox textUsername;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textPassword;
		private System.Windows.Forms.Label label3;
	}
}