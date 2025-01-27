namespace OpenDental{
	partial class FormInsPayFix {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInsPayFix));
			this.textBoxDescript = new System.Windows.Forms.TextBox();
			this.butRun = new OpenDental.UI.Button();
			this.SuspendLayout();
			// 
			// textBoxDescript
			// 
			this.textBoxDescript.BackColor = System.Drawing.SystemColors.Window;
			this.textBoxDescript.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxDescript.Location = new System.Drawing.Point(12, 12);
			this.textBoxDescript.Multiline = true;
			this.textBoxDescript.Name = "textBoxDescript";
			this.textBoxDescript.Size = new System.Drawing.Size(399, 72);
			this.textBoxDescript.TabIndex = 4;
			this.textBoxDescript.Text = resources.GetString("textBoxDescript.Text");
			// 
			// butRun
			// 
			this.butRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butRun.Location = new System.Drawing.Point(349, 102);
			this.butRun.Name = "butRun";
			this.butRun.Size = new System.Drawing.Size(75, 24);
			this.butRun.TabIndex = 3;
			this.butRun.Text = "&Run";
			this.butRun.Click += new System.EventHandler(this.butRun_Click);
			// 
			// FormInsPayFix
			// 
			this.ClientSize = new System.Drawing.Size(436, 138);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.textBoxDescript);
			this.Controls.Add(this.butRun);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormInsPayFix";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Insurance Payment Fix";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenDental.UI.Button butRun;
		private System.Windows.Forms.TextBox textBoxDescript;
	}
}