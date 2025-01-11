using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDental {
	public partial class FormFeeEdit {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFeeEdit));
			this.label1 = new System.Windows.Forms.Label();
			this.textFee = new OpenDental.ValidDouble();
			this.butSave = new OpenDental.UI.Button();
			this.odDatePickerEffectiveDate = new OpenDental.UI.ODDatePicker();
			this.labelDateEffective = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(17, 49);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Fee";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textFee
			// 
			this.textFee.Location = new System.Drawing.Point(93, 45);
			this.textFee.MaxVal = 100000000D;
			this.textFee.MinVal = -100000000D;
			this.textFee.Name = "textFee";
			this.textFee.Size = new System.Drawing.Size(72, 20);
			this.textFee.TabIndex = 0;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(338, 167);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 25);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// odDatePickerEffectiveDate
			// 
			this.odDatePickerEffectiveDate.BackColor = System.Drawing.Color.Transparent;
			this.odDatePickerEffectiveDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.odDatePickerEffectiveDate.Location = new System.Drawing.Point(30, 16);
			this.odDatePickerEffectiveDate.Name = "odDatePickerEffectiveDate";
			this.odDatePickerEffectiveDate.Size = new System.Drawing.Size(227, 23);
			this.odDatePickerEffectiveDate.TabIndex = 30;
			// 
			// labelDateEffective
			// 
			this.labelDateEffective.Location = new System.Drawing.Point(13, 20);
			this.labelDateEffective.Name = "labelDateEffective";
			this.labelDateEffective.Size = new System.Drawing.Size(77, 20);
			this.labelDateEffective.TabIndex = 31;
			this.labelDateEffective.Text = "Date Effective";
			this.labelDateEffective.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(199, 18);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(253, 16);
			this.label2.TabIndex = 32;
			this.label2.Text = "leave blank to make it effective immediately";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormFeeEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(425, 204);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.labelDateEffective);
			this.Controls.Add(this.odDatePickerEffectiveDate);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.textFee);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormFeeEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Fee";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormFeeEdit_Closing);
			this.Load += new System.EventHandler(this.FormFeeEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.Label label1;
		private OpenDental.ValidDouble textFee;
		private OpenDental.UI.Button butSave;
		private UI.ODDatePicker odDatePickerEffectiveDate;
		private Label labelDateEffective;
		private Label label2;
	}
}
