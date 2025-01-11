namespace OpenDental {
	partial class FormMessageReplacementTextEdit {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMessageReplacementTextEdit));
			this.butSave = new OpenDental.UI.Button();
			this.textBoxEditor = new OpenDental.ODtextBox();
			this.butReplacementText = new OpenDental.UI.Button();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(417, 434);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 2;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textBoxEditor
			// 
			this.textBoxEditor.AcceptsTab = true;
			this.textBoxEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxEditor.BackColor = System.Drawing.SystemColors.Window;
			this.textBoxEditor.DetectLinksEnabled = false;
			this.textBoxEditor.DetectUrls = false;
			this.textBoxEditor.Location = new System.Drawing.Point(12, 12);
			this.textBoxEditor.Name = "textBoxEditor";
			this.textBoxEditor.QuickPasteType = OpenDentBusiness.EnumQuickPasteType.PayPlan;
			this.textBoxEditor.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textBoxEditor.Size = new System.Drawing.Size(480, 416);
			this.textBoxEditor.TabIndex = 0;
			this.textBoxEditor.Text = "";
			// 
			// butReplacementText
			// 
			this.butReplacementText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butReplacementText.Location = new System.Drawing.Point(12, 434);
			this.butReplacementText.Name = "butReplacementText";
			this.butReplacementText.Size = new System.Drawing.Size(101, 24);
			this.butReplacementText.TabIndex = 1;
			this.butReplacementText.Text = "Replacements";
			this.butReplacementText.Click += new System.EventHandler(this.butReplacementText_Click);
			// 
			// FormMessageReplacementTextEdit
			// 
			this.ClientSize = new System.Drawing.Size(504, 470);
			this.Controls.Add(this.butReplacementText);
			this.Controls.Add(this.textBoxEditor);
			this.Controls.Add(this.butSave);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormMessageReplacementTextEdit";
			this.Text = "Payment Plan Terms and Conditions";
			this.Load += new System.EventHandler(this.FormMessageReplacementTextEdit_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenDental.UI.Button butSave;
		private ODtextBox textBoxEditor;
		private UI.Button butReplacementText;
	}
}