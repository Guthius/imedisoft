﻿using System.Windows.Forms;

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
			this.timerSignals = new System.Windows.Forms.Timer(this.components);
			this.panelSplitter = new System.Windows.Forms.Panel();
			this.menuSplitter = new System.Windows.Forms.ContextMenu();
			this.menuItemDockBottom = new System.Windows.Forms.MenuItem();
			this.menuItemDockRight = new System.Windows.Forms.MenuItem();
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
			this.lightSignalGrid1 = new OpenDental.UI.LightSignalGrid();
			this.menuMain = new OpenDental.UI.MenuOD();
			this.splitContainer = new OpenDental.UI.SplitContainer();
			this.splitterPanel1 = new OpenDental.UI.SplitterPanel();
			this.splitterPanel2 = new OpenDental.UI.SplitterPanel();
			this.toolTipMap = new System.Windows.Forms.ToolTip(this.components);
			this.splitContainer.SuspendLayout();
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
			// panelSplitter
			// 
			this.panelSplitter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.panelSplitter.Location = new System.Drawing.Point(71, 568);
			this.panelSplitter.Name = "panelSplitter";
			this.panelSplitter.Size = new System.Drawing.Size(769, 7);
			this.panelSplitter.TabIndex = 50;
			this.panelSplitter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelSplitter_MouseDown);
			this.panelSplitter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelSplitter_MouseMove);
			this.panelSplitter.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelSplitter_MouseUp);
			// 
			// menuSplitter
			// 
			this.menuSplitter.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemDockBottom,
            this.menuItemDockRight});
			// 
			// menuItemDockBottom
			// 
			this.menuItemDockBottom.Index = 0;
			this.menuItemDockBottom.Text = "Dock to Bottom";
			this.menuItemDockBottom.Click += new System.EventHandler(this.menuItemDockBottom_Click);
			// 
			// menuItemDockRight
			// 
			this.menuItemDockRight.Index = 1;
			this.menuItemDockRight.Text = "Dock to Right";
			this.menuItemDockRight.Click += new System.EventHandler(this.menuItemDockRight_Click);
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
			// lightSignalGrid1
			// 
			this.lightSignalGrid1.Location = new System.Drawing.Point(0, 489);
			this.lightSignalGrid1.Name = "lightSignalGrid1";
			this.lightSignalGrid1.Size = new System.Drawing.Size(50, 206);
			this.lightSignalGrid1.TabIndex = 20;
			this.lightSignalGrid1.Text = "lightSignalGrid1";
			this.lightSignalGrid1.ButtonClick += new OpenDental.UI.ODLightSignalGridClickEventHandler(this.lightSignalGrid1_ButtonClick);
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
			this.splitContainer.Controls.Add(this.splitterPanel1);
			this.splitContainer.Controls.Add(this.splitterPanel2);
			this.splitContainer.Cursor = System.Windows.Forms.Cursors.Default;
			this.splitContainer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.splitContainer.Location = new System.Drawing.Point(297, 67);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Panel1 = this.splitterPanel1;
			this.splitContainer.Panel2 = this.splitterPanel2;
			this.splitContainer.Size = new System.Drawing.Size(491, 268);
			this.splitContainer.SplitterDistance = 314;
			this.splitContainer.TabIndex = 59;
			this.splitContainer.SplitterMoved += new System.EventHandler(this.splitContainer_SplitterMoved);
			// 
			// splitterPanel1
			// 
			this.splitterPanel1.Location = new System.Drawing.Point(0, 0);
			this.splitterPanel1.Name = "splitterPanel1";
			this.splitterPanel1.Size = new System.Drawing.Size(314, 268);
			this.splitterPanel1.TabIndex = 13;
			// 
			// splitterPanel2
			// 
			this.splitterPanel2.Location = new System.Drawing.Point(318, 0);
			this.splitterPanel2.Name = "splitterPanel2";
			this.splitterPanel2.Size = new System.Drawing.Size(173, 268);
			this.splitterPanel2.TabIndex = 14;
			// 
			// toolTipMap
			// 
			this.toolTipMap.AutoPopDelay = 20000;
			this.toolTipMap.InitialDelay = 0;
			this.toolTipMap.ReshowDelay = 0;
			// 
			// FormOpenDental
			// 
			this.ClientSize = new System.Drawing.Size(1230, 696);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.panelSplitter);
			this.Controls.Add(this.lightSignalGrid1);
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
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timerTimeIndic;
		private OpenDental.UI.LightSignalGrid lightSignalGrid1;
		private Panel panelSplitter;
		private ContextMenu menuSplitter;
		private MenuItem menuItemDockBottom;
		private MenuItem menuItemDockRight;
		private ImageList imageListMain;
		private ContextMenu menuPatient;
		private ContextMenu menuLabel;
		private ContextMenu menuEmail;
		private ContextMenu menuLetter;
		private UserControlTasks userControlTasks1;
		private UserControlDashboard userControlDashboard;
		private ControlAppt controlAppt;
		private ControlFamily controlFamily;
		private ControlFamilyEcw controlFamilyEcw;
		private ControlAccount controlAccount;
		private ControlTreat controlTreat;
		private ControlChart controlChart;
		private ControlImagesOld controlImagesOld;
		private ControlImages controlImages;
		private ControlManage controlManage;
		private ModuleBar moduleBar;
		private OpenDental.UI.ToolBarOD ToolBarMain;
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
		private UI.SplitContainer splitContainer;
		private UI.SplitterPanel splitterPanel1;
		private UI.SplitterPanel splitterPanel2;
		private Label label3;
		private Label label2;
		private ToolTip toolTipMap;
	}
}
