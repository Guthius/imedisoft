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

		private void InitializeComponent(){
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlManage));
            this.timerUpdateTime = new System.Windows.Forms.Timer(this.components);
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
            this.groupBox1 = new OpenDental.UI.GroupBox();
            this.textFilterName = new System.Windows.Forms.TextBox();
            this.labelFilterName = new System.Windows.Forms.Label();
            this.butViewSched = new OpenDental.UI.Button();
            this.butManage = new OpenDental.UI.Button();
            this.butBreaks = new OpenDental.UI.Button();
            this.gridEmp = new OpenDental.UI.GridOD();
            this.labelCurrentTime = new System.Windows.Forms.Label();
            this.listBoxStatus = new OpenDental.UI.ListBox();
            this.butClockOut = new OpenDental.UI.Button();
            this.butTimeCard = new OpenDental.UI.Button();
            this.labelTime = new System.Windows.Forms.Label();
            this.butClockIn = new OpenDental.UI.Button();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerUpdateTime
            // 
            this.timerUpdateTime.Enabled = true;
            this.timerUpdateTime.Interval = 1000;
            this.timerUpdateTime.Tick += new System.EventHandler(this.timerUpdateTime_Tick);
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
            this.butAccounting.Image = ((System.Drawing.Image)(resources.GetObject("butAccounting.Image")));
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
            this.butBackup.Image = ((System.Drawing.Image)(resources.GetObject("butBackup.Image")));
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
            this.butDeposit.Image = ((System.Drawing.Image)(resources.GetObject("butDeposit.Image")));
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
            this.butSendClaims.Image = ((System.Drawing.Image)(resources.GetObject("butSendClaims.Image")));
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
            this.butTasks.Image = ((System.Drawing.Image)(resources.GetObject("butTasks.Image")));
            this.butTasks.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.butTasks.Location = new System.Drawing.Point(148, 19);
            this.butTasks.Name = "butTasks";
            this.butTasks.Size = new System.Drawing.Size(104, 26);
            this.butTasks.TabIndex = 21;
            this.butTasks.Text = "Tasks";
            this.butTasks.Click += new System.EventHandler(this.butTasks_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.textFilterName);
            this.groupBox1.Controls.Add(this.labelFilterName);
            this.groupBox1.Controls.Add(this.butViewSched);
            this.groupBox1.Controls.Add(this.butManage);
            this.groupBox1.Controls.Add(this.butBreaks);
            this.groupBox1.Controls.Add(this.gridEmp);
            this.groupBox1.Controls.Add(this.labelCurrentTime);
            this.groupBox1.Controls.Add(this.listBoxStatus);
            this.groupBox1.Controls.Add(this.butClockOut);
            this.groupBox1.Controls.Add(this.butTimeCard);
            this.groupBox1.Controls.Add(this.labelTime);
            this.groupBox1.Controls.Add(this.butClockIn);
            this.groupBox1.Location = new System.Drawing.Point(338, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 272);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.Text = "Time Clock";
            // 
            // textFilterName
            // 
            this.textFilterName.Location = new System.Drawing.Point(224, 13);
            this.textFilterName.Name = "textFilterName";
            this.textFilterName.Size = new System.Drawing.Size(100, 20);
            this.textFilterName.TabIndex = 26;
            this.textFilterName.TextChanged += new System.EventHandler(this.textFilterName_TextChanged);
            // 
            // labelFilterName
            // 
            this.labelFilterName.Location = new System.Drawing.Point(130, 12);
            this.labelFilterName.Name = "labelFilterName";
            this.labelFilterName.Size = new System.Drawing.Size(93, 20);
            this.labelFilterName.TabIndex = 25;
            this.labelFilterName.Text = "Filter by Name";
            this.labelFilterName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // butViewSched
            // 
            this.butViewSched.Location = new System.Drawing.Point(340, 94);
            this.butViewSched.Name = "butViewSched";
            this.butViewSched.Size = new System.Drawing.Size(108, 25);
            this.butViewSched.TabIndex = 24;
            this.butViewSched.Text = "View Schedule";
            this.butViewSched.Click += new System.EventHandler(this.butViewSched_Click);
            // 
            // butManage
            // 
            this.butManage.Location = new System.Drawing.Point(340, 13);
            this.butManage.Name = "butManage";
            this.butManage.Size = new System.Drawing.Size(108, 25);
            this.butManage.TabIndex = 23;
            this.butManage.Text = "Manage";
            this.butManage.Click += new System.EventHandler(this.butManage_Click);
            // 
            // butBreaks
            // 
            this.butBreaks.Location = new System.Drawing.Point(340, 67);
            this.butBreaks.Name = "butBreaks";
            this.butBreaks.Size = new System.Drawing.Size(108, 25);
            this.butBreaks.TabIndex = 22;
            this.butBreaks.Text = "View Breaks";
            this.butBreaks.Click += new System.EventHandler(this.ButtonBreaks_Click);
            // 
            // gridEmp
            // 
            this.gridEmp.Location = new System.Drawing.Point(21, 40);
            this.gridEmp.Name = "gridEmp";
            this.gridEmp.SelectionMode = OpenDental.UI.GridSelectionMode.MultiExtended;
            this.gridEmp.Size = new System.Drawing.Size(303, 220);
            this.gridEmp.TabIndex = 21;
            this.gridEmp.Title = "Employee";
            this.gridEmp.TranslationName = "TableEmpClock";
            this.gridEmp.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridEmp_CellDoubleClick);
            this.gridEmp.CellClick += new OpenDental.UI.ODGridClickEventHandler(this.gridEmp_CellClick);
            // 
            // labelCurrentTime
            // 
            this.labelCurrentTime.Location = new System.Drawing.Point(350, 121);
            this.labelCurrentTime.Name = "labelCurrentTime";
            this.labelCurrentTime.Size = new System.Drawing.Size(88, 17);
            this.labelCurrentTime.TabIndex = 20;
            this.labelCurrentTime.Text = "Server Time";
            this.labelCurrentTime.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // listBoxStatus
            // 
            this.listBoxStatus.Location = new System.Drawing.Point(341, 217);
            this.listBoxStatus.Name = "listBoxStatus";
            this.listBoxStatus.Size = new System.Drawing.Size(107, 43);
            this.listBoxStatus.TabIndex = 12;
            // 
            // butClockOut
            // 
            this.butClockOut.Location = new System.Drawing.Point(340, 189);
            this.butClockOut.Name = "butClockOut";
            this.butClockOut.Size = new System.Drawing.Size(108, 25);
            this.butClockOut.TabIndex = 14;
            this.butClockOut.Text = "Clock Out For:";
            this.butClockOut.Click += new System.EventHandler(this.ButtonClockOut_Click);
            // 
            // butTimeCard
            // 
            this.butTimeCard.Location = new System.Drawing.Point(340, 40);
            this.butTimeCard.Name = "butTimeCard";
            this.butTimeCard.Size = new System.Drawing.Size(108, 25);
            this.butTimeCard.TabIndex = 16;
            this.butTimeCard.Text = "View Time Card";
            this.butTimeCard.Click += new System.EventHandler(this.butTimeCard_Click);
            // 
            // labelTime
            // 
            this.labelTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTime.Location = new System.Drawing.Point(339, 138);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(109, 21);
            this.labelTime.TabIndex = 17;
            this.labelTime.Text = "12:00:00 PM";
            this.labelTime.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // butClockIn
            // 
            this.butClockIn.Location = new System.Drawing.Point(340, 162);
            this.butClockIn.Name = "butClockIn";
            this.butClockIn.Size = new System.Drawing.Size(108, 25);
            this.butClockIn.TabIndex = 11;
            this.butClockIn.Text = "Clock In";
            this.butClockIn.Click += new System.EventHandler(this.ButtonClockIn_Click);
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
            this.Controls.Add(this.groupBox1);
            this.Name = "ControlManage";
            this.Size = new System.Drawing.Size(908, 702);
            this.groupBox3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		#region Fields - Private - Windows Forms
		private UI.Button butAccounting;
		private UI.Button butBackup;
		private UI.Button butBilling;
		private UI.Button butBreaks;
		private UI.Button butClaimPay;
		private UI.Button butClockIn;
		private UI.Button butClockOut;
		private UI.Button butDeposit;
		private UI.Button butEmailInbox;
		private UI.Button butEras;
		private UI.Button butImportInsPlans;
		private UI.Button butManage;
		private UI.Button butSendClaims;
		private UI.Button butTasks;
		private UI.Button butTimeCard;
		private UI.Button butViewSched;
		private GridOD gridEmp;
		private OpenDental.UI.GroupBox groupBox1;
		private OpenDental.UI.GroupBox groupBox3;
		private Label labelCurrentTime;
		private Label labelTime;
		private OpenDental.UI.ListBox listBoxStatus;
		private System.ComponentModel.IContainer components;
		private Label labelFilterName;
		private System.Windows.Forms.TextBox textFilterName;
		private System.Windows.Forms.Timer timerUpdateTime;
        #endregion Fields - Private - Windows Forms

        private System.Windows.Forms.Integration.ElementHost elementHost1;
    }
}
