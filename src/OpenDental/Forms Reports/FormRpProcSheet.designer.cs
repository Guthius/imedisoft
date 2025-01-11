using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDental {
	public partial class FormRpProcSheet {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRpProcSheet));
			this.butRefresh = new OpenDental.UI.Button();
			this.gridMain = new OpenDental.UI.GridOD();
			this.butPrint = new OpenDental.UI.Button();
			this.comboClinics = new OpenDental.UI.ComboBoxClinicPicker();
			this.comboProviders = new OpenDental.UI.ComboBox();
			this.labelProvider = new System.Windows.Forms.Label();
			this.labelDatePickerTo = new System.Windows.Forms.Label();
			this.labelDatePickerFrom = new System.Windows.Forms.Label();
			this.datePickerTo = new OpenDental.UI.ODDatePicker();
			this.datePickerFrom = new OpenDental.UI.ODDatePicker();
			this.SuspendLayout();
			// 
			// butRefresh
			// 
			this.butRefresh.Location = new System.Drawing.Point(855, 10);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 24);
			this.butRefresh.TabIndex = 3;
			this.butRefresh.Text = "&Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.HScrollVisible = true;
			this.gridMain.Location = new System.Drawing.Point(12, 77);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(918, 698);
			this.gridMain.TabIndex = 55;
			this.gridMain.Title = "Individual Procedures";
			this.gridMain.TranslationName = "TableIndividualProcedures";
			// 
			// butPrint
			// 
			this.butPrint.Image = global::OpenDental.Properties.Resources.butPrintSmall;
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(855, 44);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(75, 24);
			this.butPrint.TabIndex = 56;
			this.butPrint.Text = "&Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// comboClinics
			// 
			this.comboClinics.IncludeAll = true;
			this.comboClinics.IncludeHiddenInAll = true;
			this.comboClinics.IncludeUnassigned = true;
			this.comboClinics.IsMultiSelect = true;
			this.comboClinics.Location = new System.Drawing.Point(378, 15);
			this.comboClinics.Name = "comboClinics";
			this.comboClinics.Size = new System.Drawing.Size(197, 20);
			this.comboClinics.TabIndex = 61;
			// 
			// comboProviders
			// 
			this.comboProviders.BackColor = System.Drawing.SystemColors.Window;
			this.comboProviders.Location = new System.Drawing.Point(415, 48);
			this.comboProviders.Name = "comboProviders";
			this.comboProviders.SelectionModeMulti = true;
			this.comboProviders.Size = new System.Drawing.Size(160, 20);
			this.comboProviders.TabIndex = 62;
			// 
			// labelProvider
			// 
			this.labelProvider.Location = new System.Drawing.Point(319, 48);
			this.labelProvider.Name = "labelProvider";
			this.labelProvider.Size = new System.Drawing.Size(94, 20);
			this.labelProvider.TabIndex = 63;
			this.labelProvider.Text = "Provider";
			this.labelProvider.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDatePickerTo
			// 
			this.labelDatePickerTo.Location = new System.Drawing.Point(37, 48);
			this.labelDatePickerTo.Name = "labelDatePickerTo";
			this.labelDatePickerTo.Size = new System.Drawing.Size(110, 16);
			this.labelDatePickerTo.TabIndex = 65;
			this.labelDatePickerTo.Text = "To";
			this.labelDatePickerTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDatePickerFrom
			// 
			this.labelDatePickerFrom.Location = new System.Drawing.Point(31, 18);
			this.labelDatePickerFrom.Name = "labelDatePickerFrom";
			this.labelDatePickerFrom.Size = new System.Drawing.Size(122, 16);
			this.labelDatePickerFrom.TabIndex = 64;
			this.labelDatePickerFrom.Text = "From";
			this.labelDatePickerFrom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// datePickerTo
			// 
			this.datePickerTo.BackColor = System.Drawing.Color.Transparent;
			this.datePickerTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.datePickerTo.Location = new System.Drawing.Point(98, 45);
			this.datePickerTo.Name = "datePickerTo";
			this.datePickerTo.Size = new System.Drawing.Size(202, 23);
			this.datePickerTo.TabIndex = 66;
			// 
			// datePickerFrom
			// 
			this.datePickerFrom.BackColor = System.Drawing.Color.Transparent;
			this.datePickerFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.datePickerFrom.Location = new System.Drawing.Point(98, 15);
			this.datePickerFrom.Name = "datePickerFrom";
			this.datePickerFrom.Size = new System.Drawing.Size(188, 23);
			this.datePickerFrom.TabIndex = 67;
			this.datePickerFrom.CalendarSelectionChanged += new System.EventHandler(this.datePickerFrom_CalendarSelectionChanged);
			// 
			// FormRpProcSheet
			// 
			this.AcceptButton = this.butRefresh;
			this.ClientSize = new System.Drawing.Size(942, 787);
			this.Controls.Add(this.labelDatePickerTo);
			this.Controls.Add(this.labelDatePickerFrom);
			this.Controls.Add(this.datePickerTo);
			this.Controls.Add(this.datePickerFrom);
			this.Controls.Add(this.comboProviders);
			this.Controls.Add(this.labelProvider);
			this.Controls.Add(this.comboClinics);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butRefresh);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormRpProcSheet";
			this.Text = "Daily Procedures Report";
			this.Load += new System.EventHandler(this.FormRpProcSheet_Load);
			this.ResumeLayout(false);

		}
		#endregion
		private OpenDental.UI.Button butRefresh;
		private UI.GridOD gridMain;
		private UI.Button butPrint;
		private UI.ComboBoxClinicPicker comboClinics;
		private UI.ComboBox comboProviders;
		private Label labelProvider;
		private Label labelDatePickerTo;
		private Label labelDatePickerFrom;
		private UI.ODDatePicker datePickerTo;
		private UI.ODDatePicker datePickerFrom;
	}
}
