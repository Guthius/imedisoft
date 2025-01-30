namespace OpenDental{
	partial class FormProcCodeEditMore {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProcCodeEditMore));
			this.gridMain = new OpenDental.UI.GridOD();
			this.odDatePickerDateEffective = new OpenDental.UI.ODDatePicker();
			this.labelDateEffective = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(12, 47);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(536, 481);
			this.gridMain.TabIndex = 4;
			this.gridMain.Title = "Fees";
			this.gridMain.TranslationName = "TableFees";
			this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// odDatePickerDateEffective
			// 
			this.odDatePickerDateEffective.BackColor = System.Drawing.Color.Transparent;
			this.odDatePickerDateEffective.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.odDatePickerDateEffective.Location = new System.Drawing.Point(321, 18);
			this.odDatePickerDateEffective.Name = "odDatePickerDateEffective";
			this.odDatePickerDateEffective.Size = new System.Drawing.Size(227, 23);
			this.odDatePickerDateEffective.TabIndex = 7;
			this.odDatePickerDateEffective.DateTextChanged += new System.EventHandler(this.odDatePickerDateEffective_DateTextChanged);
			// 
			// labelDateEffective
			// 
			this.labelDateEffective.Location = new System.Drawing.Point(286, 21);
			this.labelDateEffective.Name = "labelDateEffective";
			this.labelDateEffective.Size = new System.Drawing.Size(98, 20);
			this.labelDateEffective.TabIndex = 32;
			this.labelDateEffective.Text = "Date Effective";
			this.labelDateEffective.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// FormProcCodeEditMore
			// 
			this.ClientSize = new System.Drawing.Size(560, 540);
			this.Controls.Add(this.labelDateEffective);
			this.Controls.Add(this.odDatePickerDateEffective);
			this.Controls.Add(this.gridMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProcCodeEditMore";
			this.Text = "More Fees";
			this.Load += new System.EventHandler(this.FormProcCodeEditMore_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.GridOD gridMain;
		private UI.ODDatePicker odDatePickerDateEffective;
		private System.Windows.Forms.Label labelDateEffective;
	}
}