namespace OpenDental.Forms{
	partial class FormApptTypes {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormApptTypes));
			this.butAdd = new OpenDental.UI.Button();
			this.gridMain = new OpenDental.UI.GridOD();
			this.checkPrompt = new OpenDental.UI.CheckBox();
			this.checkWarn = new OpenDental.UI.CheckBox();
			this.butOK = new OpenDental.UI.Button();
			this.SuspendLayout();
			// 
			// butAdd
			// 
			this.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butAdd.Icon = OpenDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(345, 88);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(79, 24);
			this.butAdd.TabIndex = 156;
			this.butAdd.Text = "Add";
			this.butAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(8, 52);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(331, 401);
			this.gridMain.TabIndex = 155;
			this.gridMain.Title = "Appointment Types";
			this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.GridMain_CellDoubleClick);
			// 
			// checkPrompt
			// 
			this.checkPrompt.Location = new System.Drawing.Point(11, 6);
			this.checkPrompt.Name = "checkPrompt";
			this.checkPrompt.Size = new System.Drawing.Size(382, 20);
			this.checkPrompt.TabIndex = 160;
			this.checkPrompt.Text = "New appointments prompt for appointment type";
			this.checkPrompt.CheckedChanged += new System.EventHandler(this.CheckBoxPrompt_CheckedChanged);
			// 
			// checkWarn
			// 
			this.checkWarn.Location = new System.Drawing.Point(11, 30);
			this.checkWarn.Name = "checkWarn";
			this.checkWarn.Size = new System.Drawing.Size(382, 20);
			this.checkWarn.TabIndex = 161;
			this.checkWarn.Text = "Warn users before disassociating procedures from an appointment";
			this.checkWarn.CheckedChanged += new System.EventHandler(this.CheckBoxWarn_CheckedChanged);
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(345, 428);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(79, 24);
			this.butOK.TabIndex = 162;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.ButtonAccept_Click);
			// 
			// FormApptTypes
			// 
			this.ClientSize = new System.Drawing.Size(434, 464);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.checkWarn);
			this.Controls.Add(this.checkPrompt);
			this.Controls.Add(this.butAdd);
			this.Controls.Add(this.gridMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormApptTypes";
			this.Text = "Setup Appointment Types";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormApptTypes_FormClosing);
			this.Load += new System.EventHandler(this.FormApptTypes_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.Button butAdd;
		private UI.GridOD gridMain;
		private OpenDental.UI.CheckBox checkPrompt;
		private OpenDental.UI.CheckBox checkWarn;
		private UI.Button butOK;
	}
}