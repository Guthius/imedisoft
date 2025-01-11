using OpenDental.Graph.Base;

namespace OpenDental.Graph.Concrete {
	partial class GraphQuantityOverTimeFilter {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.splitContainerOptions = new System.Windows.Forms.SplitContainer();
			this.groupingOptionsCtrl1 = new GroupingOptionsCtrl();
			this.Graph = new GraphQuantityOverTime();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerOptions)).BeginInit();
			this.splitContainerOptions.Panel1.SuspendLayout();
			this.splitContainerOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.IsSplitterFixed = true;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.splitContainerOptions);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.Graph);
			this.splitContainer.Size = new System.Drawing.Size(788, 410);
			this.splitContainer.SplitterDistance = 60;
			this.splitContainer.SplitterWidth = 1;
			this.splitContainer.TabIndex = 9;
			// 
			// splitContainerOptions
			// 
			this.splitContainerOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerOptions.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainerOptions.Location = new System.Drawing.Point(0, 0);
			this.splitContainerOptions.Name = "splitContainerOptions";
			// 
			// splitContainerOptions.Panel1
			// 
			this.splitContainerOptions.Panel1.Controls.Add(this.groupingOptionsCtrl1);
			this.splitContainerOptions.Size = new System.Drawing.Size(788, 60);
			this.splitContainerOptions.SplitterDistance = 120;
			this.splitContainerOptions.TabIndex = 0;
			// 
			// groupingOptionsCtrl1
			// 
			this.groupingOptionsCtrl1.CurGrouping = GroupingOptionsCtrl.Grouping.Provider;
			this.groupingOptionsCtrl1.Location = new System.Drawing.Point(5, 0);
			this.groupingOptionsCtrl1.Name = "groupingOptionsCtrl1";
			this.groupingOptionsCtrl1.Size = new System.Drawing.Size(112, 60);
			this.groupingOptionsCtrl1.TabIndex = 0;
			this.groupingOptionsCtrl1.InputsChanged += new System.EventHandler(this.OnFormInputsChanged);
			// 
			// graph
			// 
			this.Graph.BackColor = System.Drawing.Color.Transparent;
			this.Graph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Graph.BreakdownPref = BreakdownType.items;
			this.Graph.BreakdownVal = 5;
			this.Graph.ChartSubTitle = "";
			this.Graph.CountItemDescription = "Completed Procedures";
			this.Graph.DateFrom = new System.DateTime(1880, 1, 1, 0, 0, 0, 0);
			this.Graph.DateTo = new System.DateTime(2180, 1, 1, 0, 0, 0, 0);
			this.Graph.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Graph.GraphTitle = "Production";
			this.Graph.GroupByType = System.Windows.Forms.DataVisualization.Charting.IntervalType.Weeks;
			this.Graph.IsLoading = false;
			this.Graph.LegendDock = LegendDockType.Bottom;
			this.Graph.LegendTitle = "Provider";
			this.Graph.Location = new System.Drawing.Point(0, 0);
			this.Graph.MoneyItemDescription = "Income";
			this.Graph.Name = "Graph";
			this.Graph.QtyType = QuantityType.Money;
			this.Graph.QuickRangePref = QuickRange.allTime;
			this.Graph.SeriesType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedArea;
			this.Graph.ShowFilters = true;
			this.Graph.Size = new System.Drawing.Size(788, 349);
			this.Graph.TabIndex = 1;
			this.Graph.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.Graph.UseBuiltInColors = false;
			this.Graph.OnGetGetColor = new GraphQuantityOverTime.OnGetColorArgs(this.graph_OnGetGetColor);
			// 
			// GraphQuantityOverTimeFilter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.splitContainer);
			this.Name = "GraphQuantityOverTimeFilter";
			this.Size = new System.Drawing.Size(788, 410);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainerOptions.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerOptions)).EndInit();
			this.splitContainerOptions.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.SplitContainer splitContainerOptions;
		private GroupingOptionsCtrl groupingOptionsCtrl1;
	}
}
