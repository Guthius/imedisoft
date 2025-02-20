using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDental {
	public partial class FormUpdateSetup {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUpdateSetup));
            this.textWebsitePath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textUpdateServerAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new OpenDental.UI.Button();
            this.buttonChangeTime = new OpenDental.UI.Button();
            this.textUpdateTime = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textWebsitePath
            // 
            this.textWebsitePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textWebsitePath.Location = new System.Drawing.Point(200, 38);
            this.textWebsitePath.Name = "textWebsitePath";
            this.textWebsitePath.Size = new System.Drawing.Size(372, 20);
            this.textWebsitePath.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(65, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Website Path for Updates";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textUpdateServerAddress
            // 
            this.textUpdateServerAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textUpdateServerAddress.Location = new System.Drawing.Point(200, 12);
            this.textUpdateServerAddress.Name = "textUpdateServerAddress";
            this.textUpdateServerAddress.Size = new System.Drawing.Size(372, 20);
            this.textUpdateServerAddress.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server Address for Updates";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(497, 113);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 26);
            this.buttonSave.TabIndex = 7;
            this.buttonSave.Text = "&Save";
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // buttonChangeTime
            // 
            this.buttonChangeTime.Location = new System.Drawing.Point(386, 63);
            this.buttonChangeTime.Name = "buttonChangeTime";
            this.buttonChangeTime.Size = new System.Drawing.Size(67, 23);
            this.buttonChangeTime.TabIndex = 6;
            this.buttonChangeTime.Text = "Change";
            this.buttonChangeTime.Click += new System.EventHandler(this.ButtonChangeTime_Click);
            // 
            // textUpdateTime
            // 
            this.textUpdateTime.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textUpdateTime.Location = new System.Drawing.Point(200, 64);
            this.textUpdateTime.Name = "textUpdateTime";
            this.textUpdateTime.ReadOnly = true;
            this.textUpdateTime.Size = new System.Drawing.Size(180, 20);
            this.textUpdateTime.TabIndex = 5;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(70, 67);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(124, 13);
            this.label12.TabIndex = 4;
            this.label12.Text = "Update Notification Time";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormUpdateSetup
            // 
            this.ClientSize = new System.Drawing.Size(584, 151);
            this.Controls.Add(this.buttonChangeTime);
            this.Controls.Add(this.textUpdateTime);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.textUpdateServerAddress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textWebsitePath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonSave);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUpdateSetup";
            this.ShowInTaskbar = false;
            this.Text = "Update Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormUpdateSetup_FormClosing);
            this.Load += new System.EventHandler(this.FormUpdateSetup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		private OpenDental.UI.Button buttonSave;
		private TextBox textWebsitePath;
		private Label label3;
		private TextBox textUpdateServerAddress;
		private Label label1;
		private UI.Button buttonChangeTime;
		private TextBox textUpdateTime;
		private Label label12;
	}
}
