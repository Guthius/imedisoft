using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OpenDental.Graph.Base;
using OpenDental.Graph.Cache;
using OpenDental.Graph.Concrete;
using OpenDentBusiness;
using IntervalType = System.Windows.Forms.DataVisualization.Charting.IntervalType;

namespace OpenDental.Graph.Dashboard
{
    public partial class DashboardTabCtrl : UserControl
    {
        private bool _isEditMode;
        private bool _hasUnsavedChanges;

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                
                TabPage tabPage;
                DashboardPanelCtrl dashboardPanel;
                
                if (IsEditMode)
                {
                    tabControl.ImageList = tabImageList;
                    if (tabControl.TabCount == 0 || GetDashboardPanel(tabControl.TabCount - 1, out tabPage, out dashboardPanel))
                    {
                        tabControl.TabPages.Add(new TabPage {ImageIndex = 0, BackColor = SystemColors.Control, Text = "Add Tab",});
                    }

                    butPrintPage.Visible = false;
                }
                else
                {
                    tabControl.ImageList = null;
                    if (tabControl.TabCount > 0 && !GetDashboardPanel(tabControl.TabCount - 1, out tabPage, out dashboardPanel))
                    {
                        tabControl.TabPages.RemoveAt(tabControl.TabCount - 1);
                    }

                    butPrintPage.Visible = true;
                }

                for (var tabIndex = 0; tabIndex < tabControl.TabCount; tabIndex++)
                {
                    if (!GetDashboardPanel(tabIndex, out tabPage, out dashboardPanel))
                    {
                        continue;
                    }
                    
                    dashboardPanel.IsEditMode = IsEditMode;
                    SetTabPageImageIndex(tabPage, IsEditMode ? 1 : -1);
                }
            }
        }

        public bool HasUnsavedChanges
        {
            get
            {
                if (_hasUnsavedChanges)
                {
                    return true;
                }
                
                var tabCount = IsEditMode ? tabControl.TabCount - 1 : tabControl.TabCount;
                for (var i = 0; i < tabCount; ++i)
                {
                    if (!GetDashboardPanel(i, out _, out var dashboardPanel))
                    {
                        continue;
                    }

                    if (dashboardPanel.HasUnsavedChanges)
                    {
                        return true;
                    }
                }

                return false;
            }
            set
            {
                if (value)
                {
                    return;
                }

                var tabCount = IsEditMode ? tabControl.TabCount - 1 : tabControl.TabCount;
                for (var i = 0; i < tabCount; ++i)
                {
                    if (!GetDashboardPanel(i, out _, out var dashboardPanel))
                    {
                        continue;
                    }

                    dashboardPanel.HasUnsavedChanges = false;
                }

                _hasUnsavedChanges = false;
            }
        }
        
        public int TabPageCount => tabControl.TabCount;

        public DashboardTabCtrl()
        {
            InitializeComponent();
            AddTabPage();
        }
        
        public void GetDashboardLayout(out List<DashboardLayout> layouts)
        {
            layouts = new List<DashboardLayout>();
            var tabCount = IsEditMode ? tabControl.TabCount - 1 : tabControl.TabCount;
            for (var i = 0; i < tabCount; ++i)
            {
                if (!GetDashboardPanel(i, out var tabPage, out var dashboardPanel))
                {
                    continue;
                }

                var layout = new DashboardLayout
                {
                    IsNew = true,
                    DashboardTabName = tabPage.Text,
                    DashboardTabOrder = dashboardPanel.TabOrder,
                    DashboardRows = dashboardPanel.Rows,
                    DashboardColumns = dashboardPanel.Columns,
                    //The rest of the fields are filled below if this was an existing db row.
                    DashboardLayoutNum = 0,
                    UserNum = 0,
                    UserGroupNum = 0,
                };
                if (dashboardPanel.DbItem != null)
                {
                    //This was an existing db row so update.
                    layout.IsNew = false;
                    layout.DashboardLayoutNum = dashboardPanel.DbItem.DashboardLayoutNum;
                    layout.UserNum = dashboardPanel.DbItem.UserNum;
                    layout.UserGroupNum = dashboardPanel.DbItem.UserGroupNum;
                }

                layout.Cells = dashboardPanel.Cells;
                layouts.Add(layout);
            }
        }

        public void SetDashboardLayout(List<DashboardLayout> layouts, bool invalidateFirst, GraphQuantityOverTime.OnGetColorFromSeriesGraphTypeArgs onGetSeriesColor = null, GraphQuantityOverTimeFilter.GetGraphPointsForHqArgs onGetOdGraphPointsArgs = null)
        {
            //This may need to be uncommented if user is not being prompted to save changes in certain cases.
            //_hasUnsavedChanges=false;
            if (IsEditMode)
            {
                while (tabControl.TabCount > 1)
                {
                    //Leave the 'add' tab.
                    tabControl.TabPages.RemoveAt(0);
                }
            }
            else
            {
                while (tabControl.TabCount > 0)
                {
                    //Remove all tabs.
                    tabControl.TabPages.RemoveAt(0);
                }
            }

            //This will start cache threads.
            DashboardCache.RefreshLayoutsIfInvalid(layouts, false, invalidateFirst);
            this.SuspendLayout();
            layouts.OrderBy(x => x.DashboardTabOrder).ToList().ForEach(x =>
            {
                var dashboardPanel = new DashboardPanelCtrl(x);
                dashboardPanel.SetCellLayout(x.DashboardRows, x.DashboardColumns, x.Cells, onGetSeriesColor);
                AddTabPage(x.DashboardTabName, dashboardPanel);
            });
            if (tabControl.TabCount >= 1)
            {
                tabControl.SelectedIndex = 0;
            }

            this.ResumeLayout();
        }
        
        public void AddDefaultsTabPractice(bool hasPrompt)
        {
            if (!ValidateTabName("Practice Defaults"))
            {
                return;
            }

            //get current layouts
            GetDashboardLayout(out var layoutsCur);
            //create layout
            var layout = new DashboardLayout
            {
                DashboardColumns = 3,
                DashboardRows = 2,
                DashboardGroupName = "Default",
                DashboardTabName = "Practice Defaults",
                //assigned default layout to first tab.
                DashboardTabOrder = layoutsCur.Count,
                UserGroupNum = 0,
                UserNum = 0,
                IsNew = true,
            };
            var i = 0;
            //define cell settings. anything not set here will be the defaults defined in GraphQuantityOverTimeFilter
            for (var row = 0; row < layout.DashboardRows; row++)
            {
                for (var col = 0; col < layout.DashboardColumns; col++)
                {
                    var cell = new DashboardCell
                    {
                        CellRow = row,
                        CellColumn = col,
                    };
                    switch (i++)
                    {
                        case 0:
                            cell.CellType = DashboardCellType.ProductionGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    IncludeCompleteProcs = true,
                                    IncludeAdjustements = true,
                                    IncludeWriteoffs = true,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Production - Last 12 Months",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.Line,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.None,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.none,
                                })
                            });
                            break;
                        case 1:
                            cell.CellType = DashboardCellType.IncomeGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    IncludePaySplits = true,
                                    IncludeInsuranceClaims = true,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Income - Last 12 Months",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.Line,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.None,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.none,
                                })
                            });
                            break;
                        case 2:
                            cell.CellType = DashboardCellType.BrokenApptGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    CurRunFor = BrokenApptGraphOptionsCtrl.RunFor.Appointment
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Broken Appointments - Last 12 Months",
                                    QtyType = QuantityType.Count,
                                    SeriesType = SeriesChartType.Line,
                                    GroupByType = IntervalType.Weeks,
                                    LegendDock = LegendDockType.None,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.none,
                                })
                            });
                            break;
                        case 3:
                            cell.CellType = DashboardCellType.AccountsReceivableGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = "",
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Accounts Receivable - Last 12 Months",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.Line,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.None,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.none,
                                })
                            });
                            break;
                        case 4:
                            cell.CellType = DashboardCellType.NewPatientsGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = "",
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "New Patients - Last 12 Months",
                                    QtyType = QuantityType.Count,
                                    SeriesType = SeriesChartType.Line,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.None,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.none,
                                })
                            });
                            break;
                        default:
                            cell.CellType = DashboardCellType.ProductionGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    IncludeCompleteProcs = true,
                                    IncludeAdjustements = true,
                                    IncludeWriteoffs = true,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Production - Last 30 Days",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.Line,
                                    GroupByType = IntervalType.Days,
                                    LegendDock = LegendDockType.None,
                                    QuickRangePref = QuickRange.last30Days,
                                    BreakdownPref = BreakdownType.none,
                                })
                            });
                            break;
                    }

                    layout.Cells.Add(cell);
                }
            }

            layoutsCur.Add(layout);
            //set dashboard tab control layouts
            SetDashboardLayout(layoutsCur, false);
            _hasUnsavedChanges = _hasUnsavedChanges || hasPrompt;
        }

        public void AddDefaultsTabByGrouping(bool hasPrompt, GroupingOptionsCtrl.Grouping grouping)
        {
            var strGrouping = char.ToUpper(grouping.ToString()[0]) + grouping.ToString().Substring(1);
            if (!ValidateTabName(strGrouping + " Defaults"))
            {
                return;
            }

            //get current layouts
            GetDashboardLayout(out var layoutsCur);
            //create layout
            var layout = new DashboardLayout
            {
                DashboardColumns = 3,
                DashboardRows = 2,
                DashboardGroupName = "Default",
                DashboardTabName = strGrouping + " Defaults",
                //assigned default layout to first tab.
                DashboardTabOrder = layoutsCur.Count,
                UserGroupNum = 0,
                UserNum = 0,
                IsNew = true,
            };
            var i = 0;
            for (var row = 0; row < layout.DashboardRows; row++)
            {
                for (var col = 0; col < layout.DashboardColumns; col++)
                {
                    var cell = new DashboardCell
                    {
                        CellRow = row,
                        CellColumn = col,
                    };
                    switch (i++)
                    {
                        default:
                            cell.CellType = DashboardCellType.ProductionGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    CurGrouping = grouping,
                                    IncludeCompleteProcs = true,
                                    IncludeAdjustements = true,
                                    IncludeWriteoffs = true,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Production - Last 12 Months",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.StackedColumn,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.Bottom,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.items,
                                    BreakdownVal = 10,
                                })
                            });
                            break;
                        case 1:
                            cell.CellType = DashboardCellType.IncomeGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    CurGrouping = grouping,
                                    IncludePaySplits = true,
                                    IncludeInsuranceClaims = true,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Income - Last 12 Months",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.StackedColumn,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.Bottom,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.items,
                                    BreakdownVal = 10,
                                })
                            });
                            break;
                        case 2:
                            cell.CellType = DashboardCellType.BrokenApptGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    CurGrouping = grouping,
                                    CurRunFor = BrokenApptGraphOptionsCtrl.RunFor.Appointment,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Broken Appointments - Last 12 Months",
                                    QtyType = QuantityType.Count,
                                    SeriesType = SeriesChartType.StackedColumn,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.Bottom,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.items,
                                    BreakdownVal = 10,
                                })
                            });
                            break;
                        case 3:
                            cell.CellType = DashboardCellType.ProductionGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    CurGrouping = grouping,
                                    IncludeCompleteProcs = true,
                                    IncludeAdjustements = true,
                                    IncludeWriteoffs = true,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Production - Last 30 Days",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.StackedColumn,
                                    GroupByType = IntervalType.Days,
                                    LegendDock = LegendDockType.Bottom,
                                    QuickRangePref = QuickRange.last30Days,
                                    BreakdownPref = BreakdownType.items,
                                    BreakdownVal = 10,
                                })
                            });
                            break;
                        case 4:
                            cell.CellType = DashboardCellType.IncomeGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    CurGrouping = grouping,
                                    IncludePaySplits = true,
                                    IncludeInsuranceClaims = true,
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "Income - Last 30 Days",
                                    QtyType = QuantityType.Money,
                                    SeriesType = SeriesChartType.StackedColumn,
                                    GroupByType = IntervalType.Days,
                                    LegendDock = LegendDockType.Bottom,
                                    QuickRangePref = QuickRange.last30Days,
                                    BreakdownPref = BreakdownType.items,
                                    BreakdownVal = 10,
                                })
                            });
                            break;
                        case 5:
                            cell.CellType = DashboardCellType.NewPatientsGraph;
                            cell.CellSettings = ODGraphJson.Serialize(new ODGraphJson
                            {
                                FilterJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTimeFilter.GraphQuantityOverTimeFilterSettings
                                {
                                    CurGrouping = grouping
                                }),
                                GraphJson = ODGraphSettingsBase.Serialize(new GraphQuantityOverTime.QuantityOverTimeGraphSettings
                                {
                                    Title = "New Patients - Last 12 Months",
                                    QtyType = QuantityType.Count,
                                    SeriesType = SeriesChartType.StackedColumn,
                                    GroupByType = IntervalType.Months,
                                    LegendDock = LegendDockType.Bottom,
                                    QuickRangePref = QuickRange.last12Months,
                                    BreakdownPref = BreakdownType.items,
                                    BreakdownVal = 10,
                                })
                            });
                            break;
                    }

                    layout.Cells.Add(cell);
                }
            }

            layoutsCur.Add(layout);
            //set dashboard tab control layouts
            SetDashboardLayout(layoutsCur, true);
            _hasUnsavedChanges = _hasUnsavedChanges || hasPrompt;
        }
        
        private bool GetDashboardPanel(int tabIndex, out TabPage tabPage, out DashboardPanelCtrl dashboardPanel)
        {
            tabPage = tabControl.TabPages[tabIndex];
            dashboardPanel = null;
            if (tabPage.Controls.Count <= 0 || !(tabPage.Controls[0] is DashboardPanelCtrl))
            {
                return false;
            }

            dashboardPanel = (DashboardPanelCtrl) tabPage.Controls[0];
            return true;
        }

        private void AddTabPage(string tabText = "", DashboardPanelCtrl dashboardPanel = null)
        {
            var i = 1;
            if (string.IsNullOrEmpty(tabText))
            {
                tabText = "Tab " + i;
            }

            while (tabControl.TabPages.ContainsKey(tabText))
            {
                i++;
                tabText = "Tab " + i;
            }

            dashboardPanel ??= new DashboardPanelCtrl();

            var tabPage = new TabPage(tabText);
            tabPage.Name = tabText;
            tabPage.ImageIndex = IsEditMode ? 1 : -1; //Show the delete button if edit mode.
            tabPage.BackColor = SystemColors.Control;
            dashboardPanel.Dock = DockStyle.Fill;
            dashboardPanel.Name = tabText;
            dashboardPanel.IsEditMode = IsEditMode;
            tabPage.Controls.Add(dashboardPanel);
            tabControl.CreateControl();
            tabControl.TabPages.Insert(IsEditMode ? tabControl.TabCount - 1 : tabControl.TabCount, tabPage); //Insert before the 'add' tab if in edit mode.
            tabControl.SelectedTab = tabPage;
            RefreshTabOrdering();
        }

        private void DeleteTabPage(int tabIndex)
        {
            if (!IsEditMode)
            {
                return;
            }

            if (!GetDashboardPanel(tabIndex, out _, out var dashboardPanel))
            {
                return;
            }

            if (tabControl.TabCount == 2)
            {
                MessageBox.Show("Dashboard must contain a minimum of 1 tab.");
                return;
            }

            if (!dashboardPanel.CanDelete)
            {
                MessageBox.Show("Tab '" + dashboardPanel.Name + "' has items. Remove all items from tab before continuing.");
                return;
            }

            tabControl.TabPages.RemoveAt(tabIndex);
            tabControl.SelectedIndex = Math.Max(tabIndex - 1, 0);
            RefreshTabOrdering();
        }

        private void RefreshTabOrdering()
        {
            for (var tabIndex = 0; tabIndex < tabControl.TabCount; tabIndex++)
            {
                if (!GetDashboardPanel(tabIndex, out _, out var dashboardPanel))
                {
                    return;
                }

                dashboardPanel.TabOrder = tabIndex;
            }
        }

        private static void SetTabPageImageIndex(TabPage tab, int imageIndex)
        {
            if (tab.ImageIndex != imageIndex)
            {
                tab.ImageIndex = imageIndex;
            }
        }

        private bool IsLocationInTabHeaderBounds(int index, Point location)
        {
            return tabControl.GetTabRect(index).Contains(location);
        }

        private bool IsLocationInIconButtonBounds(int index, Point location)
        {
            if (!IsLocationInTabHeaderBounds(index, location))
            {
                return false;
            }

            var rectHeader = tabControl.GetTabRect(index);
            var rectImage = new Rectangle(new Point(rectHeader.Location.X + 6, rectHeader.Location.Y + 2), new Size(tabImageList.ImageSize.Width, tabImageList.ImageSize.Width));
            return rectImage.Contains(location);
        }

        private bool ValidateTabName(string tabName)
        {
            if (tabControl.TabPages.ContainsKey(tabName))
            {
                MessageBox.Show("Tab name '" + tabName + "' already exists.");
                return false;
            }

            return true;
        }
        
        private void tabControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!IsEditMode)
            {
                return;
            }

            if (IsLocationInTabHeaderBounds(tabControl.TabCount - 1, e.Location))
            {
                //Over the 'add' tab.
                return;
            }

            for (var i = 0; i < tabControl.TabCount; ++i)
            {
                if (!IsLocationInTabHeaderBounds(i, e.Location))
                {
                    continue;
                }
                
                var tabPage = tabControl.TabPages[i];
                
                using var formDashboardNamePrompt = new FormDashboardNamePrompt(tabPage.Name, ValidateTabName);
                if (formDashboardNamePrompt.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (tabPage.Name == formDashboardNamePrompt.TabName)
                {
                    return;
                }

                tabPage.Name = formDashboardNamePrompt.TabName;
                tabPage.Text = formDashboardNamePrompt.TabName;
                tabPage.Controls[0].Name = formDashboardNamePrompt.TabName;
                return;
            }
        }

        private void tabControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (!IsEditMode)
            {
                return;
            }

            for (var i = 0; i < (tabControl.TabCount); i++)
            {
                if (i == tabControl.TabCount - 1)
                {
                    if (IsLocationInTabHeaderBounds(i, e.Location))
                    {
                        //We are over the 'add' tab header.
                        AddTabPage();
                        _hasUnsavedChanges = true;
                    }
                }
                else if (IsLocationInIconButtonBounds(i, e.Location))
                {
                    //We are over the 'delete' icon.
                    DeleteTabPage(i);
                    _hasUnsavedChanges = true;
                }
            }
        }

        private void tabControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsEditMode)
            {
                return;
            }

            for (var i = 0; i < (tabControl.TabCount - 1); ++i)
            {
                //0 - add no hover
                //1 - delete no hover
                //2 - delete with hover
                if (!GetDashboardPanel(i, out var tabPage, out var dashboardPanel))
                {
                    continue;
                }

                if (IsLocationInIconButtonBounds(i, e.Location))
                {
                    //Hovering over 'delete' icon.
                    SetTabPageImageIndex(tabPage, 2);
                    dashboardPanel.SetHightlightedAllCells(true);
                }
                else
                {
                    //Not hovering over 'delete' icon.
                    SetTabPageImageIndex(tabPage, 1);
                    dashboardPanel.SetHightlightedAllCells(false);
                }
            }
        }

        private void butPrintPage_Click(object sender, EventArgs e)
        {
            if (!GetDashboardPanel(tabControl.SelectedIndex, out _, out var dashboardPanel))
            {
                MessageBox.Show("Can't print this page.  Please try printing the graphs individually.");
                return;
            }

            using var formPrintImage = new FormPrintImage(dashboardPanel);
            
            formPrintImage.ShowDialog();
        }

        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (IsEditMode && !GetDashboardPanel(e.TabPageIndex, out _, out _))
            {
                e.Cancel = true;
            }
        }
    }
}