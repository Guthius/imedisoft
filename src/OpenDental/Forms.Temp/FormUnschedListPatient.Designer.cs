namespace OpenDental {
	partial class FormUnschedListPatient {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing&&(components!=null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUnschedListPatient));
            this.gridMain = new OpenDental.UI.GridOD();
            this.buttonAccept = new OpenDental.UI.Button();
            this.SuspendLayout();
            // 
            // gridMain
            // 
            this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridMain.Location = new System.Drawing.Point(12, 12);
            this.gridMain.Name = "gridMain";
            this.gridMain.Size = new System.Drawing.Size(570, 315);
            this.gridMain.TabIndex = 0;
            this.gridMain.Title = "Unscheduled Appointments";
            this.gridMain.TranslationName = "TableUnschedForPat";
            this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.GridMain_CellDoubleClick);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(490, 333);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(92, 24);
            this.buttonAccept.TabIndex = 1;
            this.buttonAccept.Text = "&OK";
            this.buttonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // FormUnschedListPatient
            // 
            this.ClientSize = new System.Drawing.Size(594, 369);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.gridMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormUnschedListPatient";
            this.Text = "Unscheduled Appointments for";
            this.Load += new System.EventHandler(this.FormPatientUnschedList_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private UI.GridOD gridMain;
		private UI.Button buttonAccept;
	}
}