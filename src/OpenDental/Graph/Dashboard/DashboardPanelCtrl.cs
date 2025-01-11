using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OpenDental.Graph.Base;
using OpenDental.Graph.Concrete;
using OpenDentBusiness;

namespace OpenDental.Graph.Dashboard
{
    public partial class DashboardPanelCtrl : UserControl
    {
        private bool _hasUnsavedChanges;

        public bool CanDelete
        {
            get
            {
                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                {
                    for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                    {
                        var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                        if (!(c is DashboardCellCtrl ctrl))
                        {
                            continue;
                        }

                        if (ctrl.HasDockedControl)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public int Rows => tableLayoutPanel.RowCount;

        public int Columns => tableLayoutPanel.ColumnCount;

        public int TabOrder { get; set; }

        public bool HasUnsavedChanges
        {
            get
            {
                if (_hasUnsavedChanges)
                {
                    return true;
                }

                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                {
                    for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                    {
                        var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                        if (!(c is DashboardCellCtrl ctrl))
                        {
                            continue;
                        }

                        if (ctrl.HasUnsavedChanges)
                        {
                            return true;
                        }
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

                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                {
                    for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                    {
                        var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                        if (!(c is DashboardCellCtrl ctrl))
                        {
                            continue;
                        }

                        ctrl.HasUnsavedChanges = false;
                    }
                }

                _hasUnsavedChanges = false;
            }
        }

        public List<DashboardCell> Cells
        {
            get
            {
                var ret = new List<DashboardCell>();
                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                {
                    for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                    {
                        var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                        if (!(c is DashboardCellCtrl ctrl))
                        {
                            continue;
                        }

                        if (!ctrl.HasDockedControl)
                        {
                            continue;
                        }

                        var cell = new DashboardCell
                        {
                            IsNew = true
                        };

                        if (ctrl.DockedControlTag is DashboardCell tag)
                        {
                            cell = tag;
                            cell.IsNew = false;
                        }

                        cell.CellColumn = columnIndex;
                        cell.CellRow = rowIndex;
                        if (!(ctrl.DockedControl is IDashboardDockContainer dockContainer))
                        {
                            throw new Exception("Unsupported DashboardCell type: " + ctrl.DockedControl.GetType() + ". Must implement IDashboardDockContainer.");
                        }

                        cell.CellType = dockContainer.GetCellType();
                        cell.CellSettings = dockContainer.GetCellSettings();
                        ret.Add(cell);
                    }
                }

                return ret;
            }
        }

        public DashboardLayout DbItem { get; }

        public bool IsEditMode
        {
            get => !splitContainerAddColumn.Panel2Collapsed;
            set
            {
                splitContainerAddColumn.Panel2Collapsed = !value;
                splitContainerAddRow.Panel2Collapsed = !value;
                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                {
                    for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                    {
                        var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                        if (!(c is DashboardCellCtrl ctrl))
                        {
                            continue;
                        }

                        ctrl.IsEditMode = IsEditMode;
                    }
                }
            }
        }

        public DashboardPanelCtrl(DashboardLayout dbItem = null)
        {
            InitializeComponent();
            DbItem = dbItem;
        }

        public Dictionary<Point, Chart> GetGraphsAsDictionary()
        {
            var ret = new Dictionary<Point, Chart>();
            for (var rows = 0; rows < Rows; rows++)
            {
                for (var cols = 0; cols < Columns; cols++)
                {
                    var graph = GetGraphAtPoint(rows, cols);
                    if (graph != null)
                    {
                        ret.Add(new Point(rows, cols), graph.Graph.GetChartForPrinting());
                    }
                }
            }

            return ret;
        }

        public GraphQuantityOverTimeFilter GetGraphAtPoint(int row, int col)
        {
            var c = tableLayoutPanel.GetControlFromPosition(col, row);
            if (!(c is DashboardCellCtrl ctrl))
            {
                return null;
            }

            if (!ctrl.HasDockedControl || !(ctrl.DockedControl is GraphQuantityOverTimeFilter filter))
            {
                return null;
            }

            return filter;
        }

        public void SetHightlightedAllCells(bool isHighlighted)
        {
            for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
            {
                for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                {
                    var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                    if (!(c is DashboardCellCtrl ctrl))
                    {
                        continue;
                    }

                    ctrl.IsHighlighted = isHighlighted;
                }
            }
        }

        public void SetCellLayout(int rows, int columns, List<DashboardCell> cells, GraphQuantityOverTime.OnGetColorFromSeriesGraphTypeArgs onGetSeriesColor)
        {
            try
            {
                tableLayoutPanel.SuspendLayout();
                tableLayoutPanel.Controls.Clear();
                tableLayoutPanel.RowStyles.Clear();
                tableLayoutPanel.ColumnStyles.Clear();
                tableLayoutPanel.RowCount = rows;
                tableLayoutPanel.ColumnCount = columns;
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent));
                    tableLayoutPanel.RowStyles[rowIndex].Height = 100 / (float) rows;
                }

                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
                    tableLayoutPanel.ColumnStyles[columnIndex].Width = 100 / (float) columns;
                }

                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                    {
                        var cell = cells.FirstOrDefault(x => x.CellColumn == columnIndex && x.CellRow == rowIndex);
                        DashboardDockContainer cellHolder = null;
                        if (cell != null)
                        {
                            //Currently all CellTypes return GraphQuantityOverTimeFilter. Add a switch here if we ever want to dock a different control type.
                            cellHolder = new GraphQuantityOverTimeFilter(cell.CellType, cell.CellSettings, onGetSeriesColor).CreateDashboardDockContainer(cell);
                        }

                        AddCell(columnIndex, rowIndex, cellHolder);
                    }
                }
            }
            finally
            {
                tableLayoutPanel.ResumeLayout();
            }
        }

        private int GetRowIndex(object sender)
        {
            if (!(sender is DashboardCellCtrl))
            {
                return -1;
            }

            return tableLayoutPanel.GetRow(GetCell(sender));
        }

        private int GetColumnIndex(object sender)
        {
            if (!(sender is DashboardCellCtrl))
            {
                return -1;
            }

            return tableLayoutPanel.GetColumn(GetCell(sender));
        }

        private static DashboardCellCtrl GetCell(object sender)
        {
            if (!(sender is DashboardCellCtrl ctrl))
            {
                return null;
            }

            return ctrl;
        }

        private void SetHightlightedColumn(object sender, bool isHighlighted)
        {
            var columnIndex = GetColumnIndex(sender);
            if (columnIndex < 0)
            {
                return;
            }

            for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
            {
                var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                if (!(c is DashboardCellCtrl ctrl))
                {
                    continue;
                }

                ctrl.IsHighlighted = isHighlighted;
            }
        }

        private void SetHightlightedRow(object sender, bool isHighlighted)
        {
            var rowIndex = GetRowIndex(sender);
            if (rowIndex < 0)
            {
                return;
            }

            for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
            {
                var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                if (!(c is DashboardCellCtrl ctrl))
                {
                    continue;
                }

                ctrl.IsHighlighted = isHighlighted;
            }
        }

        private bool ColumnHasItems(int columnIndex)
        {
            for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
            {
                var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                if (c is DashboardCellCtrl {HasDockedControl: true})
                {
                    return true;
                }
            }

            return false;
        }

        private bool RowHasItems(int rowIndex)
        {
            for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
            {
                var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                if (c is DashboardCellCtrl {HasDockedControl: true})
                {
                    return true;
                }
            }

            return false;
        }

        private void dashboardCell_DeleteColumnButtonMouseEnter(object sender, EventArgs e)
        {
            SetHightlightedColumn(sender, true);
        }

        private void dashboardCell_DeleteColumnButtonMouseLeave(object sender, EventArgs e)
        {
            SetHightlightedColumn(sender, false);
        }

        private void dashboardCell_DeleteRowButtonMouseEnter(object sender, EventArgs e)
        {
            SetHightlightedRow(sender, true);
        }

        private void dashboardCell_DeleteRowButtonMouseLeave(object sender, EventArgs e)
        {
            SetHightlightedRow(sender, false);
        }

        private void AddCell(int columnIndex, int rowIndex, DashboardDockContainer controlHolder = null)
        {
            var cell = new DashboardCellCtrl(controlHolder);
            cell.IsEditMode = IsEditMode;
            cell.Dock = DockStyle.Fill;
            cell.DeleteCellButtonClick += dashboardCell_DeleteCellButtonClick;
            cell.DeleteColumnButtonClick += dashboardCell_DeleteColumnButtonClick;
            cell.DeleteColumnButtonMouseEnter += dashboardCell_DeleteColumnButtonMouseEnter;
            cell.DeleteColumnButtonMouseLeave += dashboardCell_DeleteColumnButtonMouseLeave;
            cell.DeleteRowButtonClick += dashboardCell_DeleteRowButtonClick;
            cell.DeleteRowButtonMouseEnter += dashboardCell_DeleteRowButtonMouseEnter;
            cell.DeleteRowButtonMouseLeave += dashboardCell_DeleteRowButtonMouseLeave;
            tableLayoutPanel.Controls.Add(cell, columnIndex, rowIndex);
        }

        private void butAddRow_Click(object sender, EventArgs e)
        {
            try
            {
                tableLayoutPanel.SuspendLayout();
                var newRowIndex = tableLayoutPanel.RowCount;
                tableLayoutPanel.RowCount = newRowIndex + 1;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent));
                for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowStyles.Count; rowIndex++)
                {
                    tableLayoutPanel.RowStyles[rowIndex].Height = 100 / (float) tableLayoutPanel.RowStyles.Count;
                }

                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                {
                    AddCell(columnIndex, newRowIndex);
                }

                _hasUnsavedChanges = true;
            }
            finally
            {
                tableLayoutPanel.ResumeLayout();
            }
        }

        private void butAddColumn_Click(object sender, EventArgs e)
        {
            try
            {
                tableLayoutPanel.SuspendLayout();
                var newColumnIndex = tableLayoutPanel.ColumnCount;
                tableLayoutPanel.ColumnCount = newColumnIndex + 1;
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnStyles.Count; columnIndex++)
                {
                    tableLayoutPanel.ColumnStyles[columnIndex].Width = 100 / (float) tableLayoutPanel.ColumnStyles.Count;
                }

                for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                {
                    AddCell(newColumnIndex, rowIndex);
                }

                _hasUnsavedChanges = true;
            }
            finally
            {
                tableLayoutPanel.ResumeLayout();
            }
        }

        private void dashboardCell_DeleteRowButtonClick(object sender, EventArgs e)
        {
            try
            {
                tableLayoutPanel.SuspendLayout();
                var rowIndex = GetRowIndex(sender);
                if (RowHasItems(rowIndex))
                {
                    MessageBox.Show("Row " + rowIndex + " has items. Remove all items from row before continuing.");
                    return;
                }

                if (tableLayoutPanel.RowCount == 1)
                {
                    MessageBox.Show("Dashboard must contain a minimum of 1 row.");
                    return;
                }

                tableLayoutPanel.RowStyles.RemoveAt(rowIndex);
                for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                {
                    var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                    tableLayoutPanel.Controls.Remove(c);
                }

                for (var i = rowIndex + 1; i < tableLayoutPanel.RowCount; i++)
                {
                    for (var columnIndex = 0; columnIndex < tableLayoutPanel.ColumnCount; columnIndex++)
                    {
                        var c = tableLayoutPanel.GetControlFromPosition(columnIndex, i);
                        tableLayoutPanel.SetRow(c, i - 1);
                    }
                }

                tableLayoutPanel.RowCount--;
                _hasUnsavedChanges = true;
            }
            finally
            {
                tableLayoutPanel.ResumeLayout();
            }
        }

        private void dashboardCell_DeleteColumnButtonClick(object sender, EventArgs e)
        {
            try
            {
                tableLayoutPanel.SuspendLayout();
                var columnIndex = GetColumnIndex(sender);
                if (ColumnHasItems(columnIndex))
                {
                    MessageBox.Show("Column " + columnIndex + " has items. Remove all items from column before continuing.");
                    return;
                }

                if (tableLayoutPanel.ColumnCount == 1)
                {
                    MessageBox.Show("Dashboard must contain a minimum of 1 column.");
                    return;
                }

                tableLayoutPanel.ColumnStyles.RemoveAt(columnIndex);
                for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                {
                    var c = tableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
                    tableLayoutPanel.Controls.Remove(c);
                }

                for (var i = columnIndex + 1; i < tableLayoutPanel.ColumnCount; i++)
                {
                    for (var rowIndex = 0; rowIndex < tableLayoutPanel.RowCount; rowIndex++)
                    {
                        var c = tableLayoutPanel.GetControlFromPosition(i, rowIndex);
                        tableLayoutPanel.SetColumn(c, i - 1);
                    }
                }

                tableLayoutPanel.ColumnCount--;
                _hasUnsavedChanges = true;
            }
            finally
            {
                tableLayoutPanel.ResumeLayout();
            }
        }

        private void dashboardCell_DeleteCellButtonClick(object sender, EventArgs e)
        {
            var cell = GetCell(sender);
            if (cell == null)
            {
                return;
            }

            cell.RemoveDockedControl();
            _hasUnsavedChanges = true;
        }
    }
}