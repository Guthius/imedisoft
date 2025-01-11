namespace OpenDental{
	partial class FormImageDrawColor {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImageDrawColor));
			this.butSave = new OpenDental.UI.Button();
			this.labelColorFore = new System.Windows.Forms.Label();
			this.butColorFore = new System.Windows.Forms.Button();
			this.labelColorTextBack = new System.Windows.Forms.Label();
			this.butColorTextBack = new System.Windows.Forms.Button();
			this.checkTransparent = new OpenDental.UI.CheckBox();
			this.labelMount = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(226, 110);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// labelColorFore
			// 
			this.labelColorFore.Location = new System.Drawing.Point(12, 53);
			this.labelColorFore.Name = "labelColorFore";
			this.labelColorFore.Size = new System.Drawing.Size(173, 18);
			this.labelColorFore.TabIndex = 159;
			this.labelColorFore.Text = "Text, Line, and Polygon Color";
			this.labelColorFore.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butColorFore
			// 
			this.butColorFore.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.butColorFore.Location = new System.Drawing.Point(186, 52);
			this.butColorFore.Name = "butColorFore";
			this.butColorFore.Size = new System.Drawing.Size(30, 20);
			this.butColorFore.TabIndex = 160;
			this.butColorFore.Click += new System.EventHandler(this.butColorFore_Click);
			// 
			// labelColorTextBack
			// 
			this.labelColorTextBack.Location = new System.Drawing.Point(34, 79);
			this.labelColorTextBack.Name = "labelColorTextBack";
			this.labelColorTextBack.Size = new System.Drawing.Size(151, 18);
			this.labelColorTextBack.TabIndex = 165;
			this.labelColorTextBack.Text = "Text Background Color";
			this.labelColorTextBack.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butColorTextBack
			// 
			this.butColorTextBack.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.butColorTextBack.Location = new System.Drawing.Point(186, 78);
			this.butColorTextBack.Name = "butColorTextBack";
			this.butColorTextBack.Size = new System.Drawing.Size(30, 20);
			this.butColorTextBack.TabIndex = 166;
			this.butColorTextBack.Click += new System.EventHandler(this.butColorTextBack_Click);
			// 
			// checkTransparent
			// 
			this.checkTransparent.Location = new System.Drawing.Point(224, 80);
			this.checkTransparent.Name = "checkTransparent";
			this.checkTransparent.Size = new System.Drawing.Size(117, 18);
			this.checkTransparent.TabIndex = 167;
			this.checkTransparent.Text = "Transparent";
			this.checkTransparent.Click += new System.EventHandler(this.checkTransparent_Click);
			// 
			// labelMount
			// 
			this.labelMount.Location = new System.Drawing.Point(16, 18);
			this.labelMount.Name = "labelMount";
			this.labelMount.Size = new System.Drawing.Size(238, 31);
			this.labelMount.TabIndex = 170;
			this.labelMount.Text = "If you want to change the default mount colors, go to the Mount Info window.";
			// 
			// FormImageDrawColor
			// 
			this.ClientSize = new System.Drawing.Size(313, 146);
			this.Controls.Add(this.labelMount);
			this.Controls.Add(this.checkTransparent);
			this.Controls.Add(this.labelColorTextBack);
			this.Controls.Add(this.butColorTextBack);
			this.Controls.Add(this.labelColorFore);
			this.Controls.Add(this.butColorFore);
			this.Controls.Add(this.butSave);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormImageDrawColor";
			this.Text = "Edit Color";
			this.Load += new System.EventHandler(this.FormImageDrawEdit_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenDental.UI.Button butSave;
		private System.Windows.Forms.Label labelColorFore;
		private System.Windows.Forms.Button butColorFore;
		private System.Windows.Forms.Label labelColorTextBack;
		private System.Windows.Forms.Button butColorTextBack;
		private OpenDental.UI.CheckBox checkTransparent;
		private System.Windows.Forms.Label labelMount;
	}
}