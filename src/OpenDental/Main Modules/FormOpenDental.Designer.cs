using System.Windows.Forms;
using OpenDental.Main_Modules;

namespace OpenDental{
	partial class FormOpenDental{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing){
				components?.Dispose();
				_formCreditRecurringCharges?.Dispose();
				_formCertifications?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		private void InitializeComponent(){
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOpenDental));
            this.timerTimeIndic = new System.Windows.Forms.Timer(this.components);
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.imageListMain = new System.Windows.Forms.ImageList(this.components);
            this.menuPatient = new System.Windows.Forms.ContextMenu();
            this.menuLabel = new System.Windows.Forms.ContextMenu();
            this.menuEmail = new System.Windows.Forms.ContextMenu();
            this.menuLetter = new System.Windows.Forms.ContextMenu();
            this.labelMsg = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelWaitTime = new System.Windows.Forms.Label();
            this.menuText = new System.Windows.Forms.ContextMenu();
            this.menuItemTextMessagesAll = new System.Windows.Forms.MenuItem();
            this.menuItemTextMessagesReceived = new System.Windows.Forms.MenuItem();
            this.menuItemTextMessagesSent = new System.Windows.Forms.MenuItem();
            this.menuTask = new System.Windows.Forms.ContextMenu();
            this.menuItemTaskNewForUser = new System.Windows.Forms.MenuItem();
            this.menuItemTaskReminders = new System.Windows.Forms.MenuItem();
            this.menuCommlog = new System.Windows.Forms.ContextMenu();
            this.menuItemCommlogPersistent = new System.Windows.Forms.MenuItem();
            this.menuMain = new OpenDental.UI.MenuOD();
            this.splitContainer = new System.Windows.Forms.Panel();
            this.toolTipMap = new System.Windows.Forms.ToolTip(this.components);
            this.ToolBarMain = new OpenDental.UI.ToolBarOD();
            this.moduleBar = new OpenDental.ModuleBar();
            this.SuspendLayout();
            // 
            // timerTimeIndic
            // 
            this.timerTimeIndic.Interval = 60000;
            this.timerTimeIndic.Tick += new System.EventHandler(this.timerTimeIndic_Tick);
            // 
            // menuItem14
            // 
            this.menuItem14.Index = -1;
            this.menuItem14.Text = "-";
            // 
            // imageListMain
            // 
            this.imageListMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListMain.ImageStream")));
            this.imageListMain.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListMain.Images.SetKeyName(0, "Pat.gif");
            this.imageListMain.Images.SetKeyName(1, "commlog.gif");
            this.imageListMain.Images.SetKeyName(2, "email.gif");
            this.imageListMain.Images.SetKeyName(3, "tasksNicer.gif");
            this.imageListMain.Images.SetKeyName(4, "label.gif");
            this.imageListMain.Images.SetKeyName(5, "Text.gif");
            // 
            // menuPatient
            // 
            this.menuPatient.Popup += new System.EventHandler(this.menuPatient_Popup);
            // 
            // menuLabel
            // 
            this.menuLabel.Popup += new System.EventHandler(this.menuLabel_Popup);
            // 
            // menuEmail
            // 
            this.menuEmail.Popup += new System.EventHandler(this.menuEmail_Popup);
            // 
            // menuLetter
            // 
            this.menuLetter.Popup += new System.EventHandler(this.menuLetter_Popup);
            // 
            // labelMsg
            // 
            this.labelMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMsg.ForeColor = System.Drawing.Color.Firebrick;
            this.labelMsg.Location = new System.Drawing.Point(178, 41);
            this.labelMsg.Name = "labelMsg";
            this.labelMsg.Size = new System.Drawing.Size(34, 17);
            this.labelMsg.TabIndex = 53;
            this.labelMsg.Text = "00";
            this.labelMsg.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(110, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 15);
            this.label3.TabIndex = 92;
            this.label3.Text = "Messages";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(111, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 15);
            this.label2.TabIndex = 91;
            this.label2.Text = "WaitTime";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelWaitTime
            // 
            this.labelWaitTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWaitTime.ForeColor = System.Drawing.Color.Black;
            this.labelWaitTime.Location = new System.Drawing.Point(178, 56);
            this.labelWaitTime.Name = "labelWaitTime";
            this.labelWaitTime.Size = new System.Drawing.Size(30, 17);
            this.labelWaitTime.TabIndex = 53;
            this.labelWaitTime.Text = "00m";
            this.labelWaitTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // menuText
            // 
            this.menuText.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemTextMessagesAll,
            this.menuItemTextMessagesReceived,
            this.menuItemTextMessagesSent});
            // 
            // menuItemTextMessagesAll
            // 
            this.menuItemTextMessagesAll.Index = 0;
            this.menuItemTextMessagesAll.Text = "Text Messages All";
            this.menuItemTextMessagesAll.Click += new System.EventHandler(this.menuItemTextMessagesAll_Click);
            // 
            // menuItemTextMessagesReceived
            // 
            this.menuItemTextMessagesReceived.Index = 1;
            this.menuItemTextMessagesReceived.Text = "Text Messages Received";
            this.menuItemTextMessagesReceived.Click += new System.EventHandler(this.menuItemTextMessagesReceived_Click);
            // 
            // menuItemTextMessagesSent
            // 
            this.menuItemTextMessagesSent.Index = 2;
            this.menuItemTextMessagesSent.Text = "Text Messages Sent";
            this.menuItemTextMessagesSent.Click += new System.EventHandler(this.menuItemTextMessagesSent_Click);
            // 
            // menuTask
            // 
            this.menuTask.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemTaskNewForUser,
            this.menuItemTaskReminders});
            this.menuTask.Popup += new System.EventHandler(this.menuTask_Popup);
            // 
            // menuItemTaskNewForUser
            // 
            this.menuItemTaskNewForUser.Index = 0;
            this.menuItemTaskNewForUser.Text = "New for [User]";
            this.menuItemTaskNewForUser.Click += new System.EventHandler(this.menuItemTaskNewForUser_Click);
            // 
            // menuItemTaskReminders
            // 
            this.menuItemTaskReminders.Index = 1;
            this.menuItemTaskReminders.Text = "Reminders";
            this.menuItemTaskReminders.Click += new System.EventHandler(this.menuItemTaskReminders_Click);
            // 
            // menuCommlog
            // 
            this.menuCommlog.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemCommlogPersistent});
            // 
            // menuItemCommlogPersistent
            // 
            this.menuItemCommlogPersistent.Index = 0;
            this.menuItemCommlogPersistent.Text = "Persistent";
            this.menuItemCommlogPersistent.Click += new System.EventHandler(this.menuItemCommlogPersistent_Click);
            // 
            // menuMain
            // 
            this.menuMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(1230, 24);
            this.menuMain.TabIndex = 58;
            // 
            // splitContainer
            // 
            this.splitContainer.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.splitContainer.Location = new System.Drawing.Point(75, 49);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Size = new System.Drawing.Size(1155, 647);
            this.splitContainer.TabIndex = 59;
            // 
            // toolTipMap
            // 
            this.toolTipMap.AutoPopDelay = 20000;
            this.toolTipMap.InitialDelay = 0;
            this.toolTipMap.ReshowDelay = 0;
            // 
            // ToolBarMain
            // 
            this.ToolBarMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.ToolBarMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.ToolBarMain.ImageList = this.imageListMain;
            this.ToolBarMain.Location = new System.Drawing.Point(75, 24);
            this.ToolBarMain.Name = "ToolBarMain";
            this.ToolBarMain.Size = new System.Drawing.Size(1155, 25);
            this.ToolBarMain.TabIndex = 60;
            this.ToolBarMain.ButtonClick += new OpenDental.UI.ODToolBarButtonClickEventHandler(this.toolBarMain_ButtonClick);
            // 
            // moduleBar
            // 
            this.moduleBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.moduleBar.Location = new System.Drawing.Point(0, 24);
            this.moduleBar.Name = "moduleBar";
            this.moduleBar.SelectedIndex = 0;
            this.moduleBar.SelectedModule = OpenDentBusiness.EnumModuleType.Appointments;
            this.moduleBar.Size = new System.Drawing.Size(75, 672);
            this.moduleBar.TabIndex = 61;
            this.moduleBar.Text = "moduleBar1";
            this.moduleBar.ButtonClicked += new OpenDental.ButtonClickedEventHandler(this.moduleBar_ButtonClicked);
            // 
            // FormOpenDental
            // 
            this.ClientSize = new System.Drawing.Size(1230, 696);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.ToolBarMain);
            this.Controls.Add(this.moduleBar);
            this.Controls.Add(this.menuMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormOpenDental";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "Open Dental";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Deactivate += new System.EventHandler(this.FormOpenDental_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormOpenDental_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormOpenDental_FormClosed);
            this.Load += new System.EventHandler(this.FormOpenDental_Load);
            this.Shown += new System.EventHandler(this.FormOpenDental_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormOpenDental_KeyDown);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timerTimeIndic;
		private ImageList imageListMain;
		private ContextMenu menuPatient;
		private ContextMenu menuLabel;
		private ContextMenu menuEmail;
		private ContextMenu menuLetter;
		private ControlAppt controlAppt;
		private ControlFamily controlFamily;
		private ControlAccount controlAccount;
		private ControlTreat controlTreat;
		private ControlChart controlChart;
		private ControlImagesOld controlImagesOld;
		private ControlImages controlImages;
		private ControlManage controlManage;
		private Label labelWaitTime;
		private Label labelMsg;
		private MenuItem menuItem14;
		private ContextMenu menuText;
		private MenuItem menuItemTextMessagesReceived;
		private MenuItem menuItemTextMessagesSent;
		private MenuItem menuItemTextMessagesAll;
		private ContextMenu menuTask;
		private MenuItem menuItemTaskNewForUser;
		private MenuItem menuItemTaskReminders;
		private ContextMenu menuCommlog;
		private MenuItem menuItemCommlogPersistent;
		private UI.MenuOD menuMain;
		private Panel splitContainer;
		private Label label3;
		private Label label2;
		private ToolTip toolTipMap;
        private UI.ToolBarOD ToolBarMain;
        private ModuleBar moduleBar;
    }
}
