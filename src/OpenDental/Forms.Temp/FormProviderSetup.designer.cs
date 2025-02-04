using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDental {
	public partial class FormProviderSetup {
		private System.ComponentModel.IContainer components = null;

		
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProviderSetup));
			this.butAdd = new OpenDental.UI.Button();
			this.butCreateUsers = new OpenDental.UI.Button();
			this.groupCreateUsers = new OpenDental.UI.GroupBox();
			this.comboUserGroup = new OpenDental.UI.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupMovePats = new OpenDental.UI.GroupBox();
			this.butMoveSec = new OpenDental.UI.Button();
			this.butProvPick = new OpenDental.UI.Button();
			this.textMoveTo = new System.Windows.Forms.TextBox();
			this.butReassign = new OpenDental.UI.Button();
			this.labelReassign = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.butMovePri = new OpenDental.UI.Button();
			this.gridMain = new OpenDental.UI.GridOD();
			this.checkShowDeleted = new OpenDental.UI.CheckBox();
			this.checkShowHidden = new OpenDental.UI.CheckBox();
			this.labelSearch = new System.Windows.Forms.Label();
			this.textSearch = new System.Windows.Forms.TextBox();
			this.groupCreateUsers.SuspendLayout();
			this.groupMovePats.SuspendLayout();
			this.SuspendLayout();
			// 
			// butAdd
			// 
			this.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butAdd.Icon = OpenDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(885, 522);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(82, 24);
			this.butAdd.TabIndex = 6;
			this.butAdd.Text = "&Add";
			this.butAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
			// 
			// butCreateUsers
			// 
			this.butCreateUsers.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butCreateUsers.Location = new System.Drawing.Point(182, 42);
			this.butCreateUsers.Name = "butCreateUsers";
			this.butCreateUsers.Size = new System.Drawing.Size(82, 24);
			this.butCreateUsers.TabIndex = 15;
			this.butCreateUsers.Text = "Create";
			this.butCreateUsers.Click += new System.EventHandler(this.butCreateUsers_Click);
			// 
			// groupCreateUsers
			// 
			this.groupCreateUsers.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupCreateUsers.Controls.Add(this.comboUserGroup);
			this.groupCreateUsers.Controls.Add(this.label3);
			this.groupCreateUsers.Controls.Add(this.butCreateUsers);
			this.groupCreateUsers.Location = new System.Drawing.Point(703, 192);
			this.groupCreateUsers.Name = "groupCreateUsers";
			this.groupCreateUsers.Size = new System.Drawing.Size(273, 76);
			this.groupCreateUsers.TabIndex = 2;
			this.groupCreateUsers.Text = "Create Users";
			// 
			// comboUserGroup
			// 
			this.comboUserGroup.BackColor = System.Drawing.SystemColors.Window;
			this.comboUserGroup.Location = new System.Drawing.Point(98, 14);
			this.comboUserGroup.Name = "comboUserGroup";
			this.comboUserGroup.SelectionModeMulti = true;
			this.comboUserGroup.Size = new System.Drawing.Size(166, 21);
			this.comboUserGroup.TabIndex = 19;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 14);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(91, 18);
			this.label3.TabIndex = 18;
			this.label3.Text = "User Group";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupMovePats
			// 
			this.groupMovePats.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupMovePats.Controls.Add(this.butMoveSec);
			this.groupMovePats.Controls.Add(this.butProvPick);
			this.groupMovePats.Controls.Add(this.textMoveTo);
			this.groupMovePats.Controls.Add(this.butReassign);
			this.groupMovePats.Controls.Add(this.labelReassign);
			this.groupMovePats.Controls.Add(this.label2);
			this.groupMovePats.Controls.Add(this.butMovePri);
			this.groupMovePats.Location = new System.Drawing.Point(703, 273);
			this.groupMovePats.Name = "groupMovePats";
			this.groupMovePats.Size = new System.Drawing.Size(273, 132);
			this.groupMovePats.TabIndex = 3;
			this.groupMovePats.Text = "Move Patients";
			// 
			// butMoveSec
			// 
			this.butMoveSec.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butMoveSec.Location = new System.Drawing.Point(182, 46);
			this.butMoveSec.Name = "butMoveSec";
			this.butMoveSec.Size = new System.Drawing.Size(82, 24);
			this.butMoveSec.TabIndex = 15;
			this.butMoveSec.Text = "Move Sec";
			this.butMoveSec.UseVisualStyleBackColor = true;
			this.butMoveSec.Click += new System.EventHandler(this.butMoveSec_Click);
			// 
			// butProvPick
			// 
			this.butProvPick.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butProvPick.Location = new System.Drawing.Point(237, 17);
			this.butProvPick.Name = "butProvPick";
			this.butProvPick.Size = new System.Drawing.Size(27, 26);
			this.butProvPick.TabIndex = 23;
			this.butProvPick.Text = "...";
			this.butProvPick.Click += new System.EventHandler(this.butProvPick_Click);
			// 
			// textMoveTo
			// 
			this.textMoveTo.Location = new System.Drawing.Point(98, 19);
			this.textMoveTo.MaxLength = 15;
			this.textMoveTo.Name = "textMoveTo";
			this.textMoveTo.ReadOnly = true;
			this.textMoveTo.Size = new System.Drawing.Size(135, 20);
			this.textMoveTo.TabIndex = 22;
			// 
			// butReassign
			// 
			this.butReassign.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butReassign.Location = new System.Drawing.Point(182, 98);
			this.butReassign.Name = "butReassign";
			this.butReassign.Size = new System.Drawing.Size(82, 24);
			this.butReassign.TabIndex = 15;
			this.butReassign.Text = "Reassign";
			this.butReassign.Click += new System.EventHandler(this.butReassign_Click);
			// 
			// labelReassign
			// 
			this.labelReassign.Location = new System.Drawing.Point(8, 85);
			this.labelReassign.Name = "labelReassign";
			this.labelReassign.Size = new System.Drawing.Size(168, 44);
			this.labelReassign.TabIndex = 18;
			this.labelReassign.Text = "Reassigns patients to a different primary provider based on past procedures\r\n";
			this.labelReassign.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(3, 21);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(94, 18);
			this.label2.TabIndex = 18;
			this.label2.Text = "To Provider";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butMovePri
			// 
			this.butMovePri.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butMovePri.Location = new System.Drawing.Point(94, 46);
			this.butMovePri.Name = "butMovePri";
			this.butMovePri.Size = new System.Drawing.Size(82, 24);
			this.butMovePri.TabIndex = 15;
			this.butMovePri.Text = "Move Pri";
			this.butMovePri.Click += new System.EventHandler(this.butMovePri_Click);
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.HScrollVisible = true;
			this.gridMain.Location = new System.Drawing.Point(7, 31);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(688, 664);
			this.gridMain.TabIndex = 13;
			this.gridMain.Title = "Providers";
			this.gridMain.TranslationName = "TableProviderSetup";
			this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.GridMain_CellDoubleClick);
			// 
			// checkShowDeleted
			// 
			this.checkShowDeleted.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkShowDeleted.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkShowDeleted.Location = new System.Drawing.Point(561, 12);
			this.checkShowDeleted.Name = "checkShowDeleted";
			this.checkShowDeleted.Size = new System.Drawing.Size(134, 14);
			this.checkShowDeleted.TabIndex = 27;
			this.checkShowDeleted.Text = "Show Deleted";
			this.checkShowDeleted.CheckedChanged += new System.EventHandler(this.checkShowDeleted_CheckedChanged);
			// 
			// checkShowHidden
			// 
			this.checkShowHidden.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkShowHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkShowHidden.Checked = true;
			this.checkShowHidden.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowHidden.Location = new System.Drawing.Point(421, 12);
			this.checkShowHidden.Name = "checkShowHidden";
			this.checkShowHidden.Size = new System.Drawing.Size(134, 14);
			this.checkShowHidden.TabIndex = 28;
			this.checkShowHidden.Text = "Show Hidden";
			this.checkShowHidden.Click += new System.EventHandler(this.checkShowHidden_Click);
			// 
			// labelSearch
			// 
			this.labelSearch.Location = new System.Drawing.Point(6, 7);
			this.labelSearch.Name = "labelSearch";
			this.labelSearch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.labelSearch.Size = new System.Drawing.Size(55, 13);
			this.labelSearch.TabIndex = 45;
			this.labelSearch.Text = "Search";
			this.labelSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textSearch
			// 
			this.textSearch.Location = new System.Drawing.Point(62, 5);
			this.textSearch.Name = "textSearch";
			this.textSearch.Size = new System.Drawing.Size(181, 20);
			this.textSearch.TabIndex = 34;
			// 
			// FormProviderSetup
			// 
			this.ClientSize = new System.Drawing.Size(982, 707);
			this.Controls.Add(this.labelSearch);
			this.Controls.Add(this.textSearch);
			this.Controls.Add(this.checkShowHidden);
			this.Controls.Add(this.checkShowDeleted);
			this.Controls.Add(this.groupMovePats);
			this.Controls.Add(this.groupCreateUsers);
			this.Controls.Add(this.butAdd);
			this.Controls.Add(this.gridMain);
			this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProviderSetup";
			this.ShowInTaskbar = false;
			this.Text = "Provider Setup";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormProviderSelect_Closing);
			this.Load += new System.EventHandler(this.FormProviderSetup_Load);
			this.Shown += new System.EventHandler(this.FormProviderSetup_Shown);
			this.groupCreateUsers.ResumeLayout(false);
			this.groupMovePats.ResumeLayout(false);
			this.groupMovePats.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private OpenDental.UI.Button butAdd;
		private OpenDental.UI.GridOD gridMain;
		private OpenDental.UI.Button butCreateUsers;
		private OpenDental.UI.GroupBox groupCreateUsers;
		private Label label3;
		private OpenDental.UI.GroupBox groupMovePats;
		private Label label2;
		private UI.Button butMovePri;
		private UI.Button butReassign;
		private Label labelReassign;
		private UI.Button butProvPick;
		private TextBox textMoveTo;
		private UI.Button butMoveSec;
		private OpenDental.UI.CheckBox checkShowDeleted;
		private OpenDental.UI.CheckBox checkShowHidden;
		private UI.ComboBox comboUserGroup;
		private Label labelSearch;
		private TextBox textSearch;
	}
}
