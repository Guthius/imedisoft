using System.Windows.Forms;
using OpenDental.UI;

namespace OpenDental.Main_Modules {
	partial class ControlManage {

		#region Dispose
		
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion Dispose

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlManage));
			this.groupBox3 = new OpenDental.UI.GroupBox();
			this.butEras = new OpenDental.UI.Button();
			this.butImportInsPlans = new OpenDental.UI.Button();
			this.butEmailInbox = new OpenDental.UI.Button();
			this.butClaimPay = new OpenDental.UI.Button();
			this.butBilling = new OpenDental.UI.Button();
			this.butAccounting = new OpenDental.UI.Button();
			this.butBackup = new OpenDental.UI.Button();
			this.butDeposit = new OpenDental.UI.Button();
			this.butSendClaims = new OpenDental.UI.Button();
			this.butTasks = new OpenDental.UI.Button();
			this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox3
			// 
			this.groupBox3.BackColor = System.Drawing.Color.White;
			this.groupBox3.Controls.Add(this.butEras);
			this.groupBox3.Controls.Add(this.butImportInsPlans);
			this.groupBox3.Controls.Add(this.butEmailInbox);
			this.groupBox3.Controls.Add(this.butClaimPay);
			this.groupBox3.Controls.Add(this.butBilling);
			this.groupBox3.Controls.Add(this.butAccounting);
			this.groupBox3.Controls.Add(this.butBackup);
			this.groupBox3.Controls.Add(this.butDeposit);
			this.groupBox3.Controls.Add(this.butSendClaims);
			this.groupBox3.Controls.Add(this.butTasks);
			this.groupBox3.Location = new System.Drawing.Point(17, 5);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(272, 181);
			this.groupBox3.TabIndex = 23;
			this.groupBox3.Text = "Daily";
			// 
			// butEras
			// 
			this.butEras.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butEras.Location = new System.Drawing.Point(148, 123);
			this.butEras.Name = "butEras";
			this.butEras.Size = new System.Drawing.Size(104, 26);
			this.butEras.TabIndex = 30;
			this.butEras.Text = "ERAs";
			this.butEras.Click += new System.EventHandler(this.butEras_Click);
			// 
			// butImportInsPlans
			// 
			this.butImportInsPlans.Location = new System.Drawing.Point(148, 149);
			this.butImportInsPlans.Name = "butImportInsPlans";
			this.butImportInsPlans.Size = new System.Drawing.Size(104, 26);
			this.butImportInsPlans.TabIndex = 29;
			this.butImportInsPlans.Text = "Import Ins Plans";
			this.butImportInsPlans.Click += new System.EventHandler(this.butImportInsPlans_Click);
			// 
			// butEmailInbox
			// 
			this.butEmailInbox.Icon = OpenDental.UI.EnumIcons.Email;
			this.butEmailInbox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butEmailInbox.Location = new System.Drawing.Point(148, 97);
			this.butEmailInbox.Name = "butEmailInbox";
			this.butEmailInbox.Size = new System.Drawing.Size(104, 26);
			this.butEmailInbox.TabIndex = 28;
			this.butEmailInbox.Text = "Emails";
			this.butEmailInbox.Click += new System.EventHandler(this.butEmailInbox_Click);
			// 
			// butClaimPay
			// 
			this.butClaimPay.Location = new System.Drawing.Point(16, 45);
			this.butClaimPay.Name = "butClaimPay";
			this.butClaimPay.Size = new System.Drawing.Size(104, 26);
			this.butClaimPay.TabIndex = 25;
			this.butClaimPay.Text = "Batch Ins";
			this.butClaimPay.Click += new System.EventHandler(this.ButtonClaimPay_Click);
			// 
			// butBilling
			// 
			this.butBilling.Location = new System.Drawing.Point(16, 71);
			this.butBilling.Name = "butBilling";
			this.butBilling.Size = new System.Drawing.Size(104, 26);
			this.butBilling.TabIndex = 25;
			this.butBilling.Text = "Billing";
			this.butBilling.Click += new System.EventHandler(this.ButtonBilling_Click);
			// 
			// butAccounting
			// 
			this.butAccounting.Image = ((System.Drawing.Image) (resources.GetObject("butAccounting.Image")));
			this.butAccounting.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAccounting.Location = new System.Drawing.Point(148, 71);
			this.butAccounting.Name = "butAccounting";
			this.butAccounting.Size = new System.Drawing.Size(104, 26);
			this.butAccounting.TabIndex = 24;
			this.butAccounting.Text = "Accounting";
			this.butAccounting.Click += new System.EventHandler(this.ButtonAccounting_Click);
			// 
			// butBackup
			// 
			this.butBackup.Image = ((System.Drawing.Image) (resources.GetObject("butBackup.Image")));
			this.butBackup.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butBackup.Location = new System.Drawing.Point(148, 45);
			this.butBackup.Name = "butBackup";
			this.butBackup.Size = new System.Drawing.Size(104, 26);
			this.butBackup.TabIndex = 22;
			this.butBackup.Text = "Backup";
			this.butBackup.Click += new System.EventHandler(this.ButtonBackup_Click);
			// 
			// butDeposit
			// 
			this.butDeposit.Image = ((System.Drawing.Image) (resources.GetObject("butDeposit.Image")));
			this.butDeposit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDeposit.Location = new System.Drawing.Point(16, 97);
			this.butDeposit.Name = "butDeposit";
			this.butDeposit.Size = new System.Drawing.Size(104, 26);
			this.butDeposit.TabIndex = 23;
			this.butDeposit.Text = "Deposits";
			this.butDeposit.Click += new System.EventHandler(this.butDeposit_Click);
			// 
			// butSendClaims
			// 
			this.butSendClaims.Image = ((System.Drawing.Image) (resources.GetObject("butSendClaims.Image")));
			this.butSendClaims.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butSendClaims.Location = new System.Drawing.Point(16, 19);
			this.butSendClaims.Name = "butSendClaims";
			this.butSendClaims.Size = new System.Drawing.Size(104, 26);
			this.butSendClaims.TabIndex = 20;
			this.butSendClaims.Text = "Send Claims";
			this.butSendClaims.Click += new System.EventHandler(this.butSendClaims_Click);
			// 
			// butTasks
			// 
			this.butTasks.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butTasks.Image = ((System.Drawing.Image) (resources.GetObject("butTasks.Image")));
			this.butTasks.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butTasks.Location = new System.Drawing.Point(148, 19);
			this.butTasks.Name = "butTasks";
			this.butTasks.Size = new System.Drawing.Size(104, 26);
			this.butTasks.TabIndex = 21;
			this.butTasks.Text = "Tasks";
			this.butTasks.Click += new System.EventHandler(this.butTasks_Click);
			// 
			// elementHost1
			// 
			this.elementHost1.Location = new System.Drawing.Point(3, 327);
			this.elementHost1.Name = "elementHost1";
			this.elementHost1.Size = new System.Drawing.Size(773, 372);
			this.elementHost1.TabIndex = 24;
			this.elementHost1.Text = "elementHost1";
			this.elementHost1.Child = null;
			// 
			// ControlManage
			// 
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.elementHost1);
			this.Controls.Add(this.groupBox3);
			this.Name = "ControlManage";
			this.Size = new System.Drawing.Size(908, 702);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		#region Fields - Private - Windows Forms
		private UI.Button butAccounting;
		private UI.Button butBackup;
		private UI.Button butBilling;
		private UI.Button butClaimPay;
		private UI.Button butDeposit;
		private UI.Button butEmailInbox;
		private UI.Button butEras;
		private UI.Button butImportInsPlans;
		private UI.Button butSendClaims;
		private UI.Button butTasks;
		private OpenDental.UI.GroupBox groupBox3;
		private System.ComponentModel.IContainer components;
        #endregion Fields - Private - Windows Forms

        private System.Windows.Forms.Integration.ElementHost elementHost1;
    }
}
