using System;
using System.Drawing;
using System.Windows.Forms;
using OpenDental.Graph.Base;
using OpenDental.Graph.Cache;
using OpenDental.Graph.Concrete;
using OpenDentBusiness;

namespace OpenDental.Graph.Dashboard
{
    public partial class FormDashboardEditTab : Form
    {
        private readonly EventHandler _onSetDb;

        public string DashboardGroupName => "Default";

        public bool IsEditMode
        {
            get => !splitContainer1.Panel1Collapsed;
            set
            {
                splitContainer1.Panel1Collapsed = !value;
                dashboardTabControl.IsEditMode = IsEditMode;
                setupToolStripMenuItem.Text = IsEditMode ? "Exit Setup" : "Setup";
                menuItemDefaultGraphs.Visible = IsEditMode;
                menuItemResetAR.Visible = IsEditMode;
                SetTitle();
            }
        }

        public FormDashboardEditTab(long provNum, bool useProvFilter, EventHandler onSetDb = null)
        {
            InitializeComponent();
            _onSetDb = onSetDb;
            DashboardCache.ProvNum = provNum;
            DashboardCache.UseProvFilter = useProvFilter;
            //Text is loaded here because loading it from the designer forces the designer to use a resource.resx file. This is slow for some reason but loading it here is quick.
            labelHelp.Text = @"Drag a graph type to a cell on the graphic reports editor.

Drag any existing graph in the editor to any empty cell within the editor. Note: The target cell must be empty.

Remove or add an entire row/column/tab by clicking the corresponding delete/add row/column/tab button. Note: All cells must be empty for a given row/column/tab before deleting.

Edit an individual graph's settings by clicking the edit button for that cell.

Double-click tab header to rename tab.";
            SetTitle();
            listItems.Items.Clear();
            //Do not add HQMessages for our customers' release version.
            //DashboardCellType.HQMessagesOut, DashboardCellType.HQMessagesSentReceived, DashboardCellType.HQMessagesBilling
            listItems.Items.Add(new DashboardListItem {CellType = DashboardCellType.ProductionGraph, Display = "Production",});
            listItems.Items.Add(new DashboardListItem {CellType = DashboardCellType.IncomeGraph, Display = "Income",});
            listItems.Items.Add(new DashboardListItem {CellType = DashboardCellType.AccountsReceivableGraph, Display = "A/R",});
            listItems.Items.Add(new DashboardListItem {CellType = DashboardCellType.NewPatientsGraph, Display = "New Patients",});
            listItems.Items.Add(new DashboardListItem {CellType = DashboardCellType.BrokenApptGraph, Display = "Broken Appointments",});
            //It is very important that the tab layout and subsequent tabpages be added in the ctor and NOT in the load event (as is the typical OD pattern).
            //Performing the layout here speeds this process by up considerably (I have seen as fast as 8x faster). -samo 1/25/16
            //https://social.msdn.microsoft.com/Forums/windows/en-US/12cc5748-21c8-4494-b975-8b05c7513979/tabcontrol-very-slow-with-lots-of-tabs
            RefreshData(false);
            if (!true)
            {
                addClinicDefaultToolStripMenuItem.Visible = false;
            }

            AddDefaultTabs();
            KeyDown += FormDashboardEditTab_KeyDown;
        }

        private void RefreshData(bool invalidateFirst)
        {
            _onSetDb?.Invoke(this, EventArgs.Empty);

            dashboardTabControl.SetDashboardLayout(DashboardLayouts.GetDashboardLayout(DashboardGroupName), invalidateFirst);
        }

        private void SetTitle()
        {
            var title = "Graphic Reports - " + DashboardGroupName;
            if (IsEditMode)
            {
                title += " (Edit)";
            }

            this.Text = title;
        }

        private void FormDashboardEditTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                Close();
            }
        }

        private void listItems_MouseDown(object sender, MouseEventArgs e)
        {
            var i = listItems.IndexFromPoint(new Point(e.X, e.Y));
            if (i < 0)
            {
                return;
            }

            if (!(listItems.SelectedItem is DashboardListItem item))
            {
                return;
            }

            var holder = new GraphQuantityOverTimeFilter(item.CellType).CreateDashboardDockContainer();

            holder.Contr.DoDragDrop(holder, DragDropEffects.All);
        }

        private void setupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsEditMode)
            {
                if (!Security.IsAuthorized(EnumPermType.GraphicalReportSetup))
                {
                    return;
                }

                SecurityLogs.MakeLogEntry(EnumPermType.GraphicalReportSetup, 0, "Accessed graphical reports setup controls.");
            }

            IsEditMode = !IsEditMode;
        }

        private void refreshDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dashboardTabControl.HasUnsavedChanges)
            {
                if (MessageBox.Show("You have unsaved changes. Click OK to continue and discard changes.", "Discard Changes?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }
            }

            RefreshData(true);

            AddDefaultTabs();
        }

        private void AddDefaultTabs()
        {
            if (dashboardTabControl.TabPageCount != 0)
            {
                return;
            }

            dashboardTabControl.AddDefaultsTabPractice(false);
            if (true)
            {
                dashboardTabControl.AddDefaultsTabByGrouping(false, GroupingOptionsCtrl.Grouping.Clinic);
            }

            dashboardTabControl.AddDefaultsTabByGrouping(false, GroupingOptionsCtrl.Grouping.Provider);
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            dashboardTabControl.GetDashboardLayout(out var layouts);
            DashboardLayouts.SetDashboardLayout(layouts, DashboardGroupName);
            dashboardTabControl.HasUnsavedChanges = false;

            RefreshData(false);

            MessageBox.Show("Graphic Report group saved: " + DashboardGroupName + ".");
        }

        private void FormDashboardEditTab_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!dashboardTabControl.HasUnsavedChanges)
            {
                return;
            }

            if (MessageBox.Show("You have unsaved changes. Click OK to continue and discard changes.", "Discard Changes?", MessageBoxButtons.OKCancel) != DialogResult.Cancel)
            {
                return;
            }

            e.Cancel = true;
        }

        private class DashboardListItem
        {
            public DashboardCellType CellType;
            public string Display;

            public override string ToString()
            {
                return Display;
            }
        }

        private void addPracticeDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dashboardTabControl.AddDefaultsTabPractice(true);
        }

        private void addClinicDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dashboardTabControl.AddDefaultsTabByGrouping(true, GroupingOptionsCtrl.Grouping.Clinic);
        }

        private void addProviderDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dashboardTabControl.AddDefaultsTabByGrouping(true, GroupingOptionsCtrl.Grouping.Provider);
        }

        private void menuItemResetAR_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to refresh your AR reports? This could take a long time.", "Continue?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                return;
            }

            if (dashboardTabControl.HasUnsavedChanges)
            {
                if (MessageBox.Show("You have unsaved changes. Click OK to continue and discard changes.", "Discard Changes?",
                        MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }
            }

            DashboardARs.Truncate();

            RefreshData(true);

            AddDefaultTabs();
        }
    }
}