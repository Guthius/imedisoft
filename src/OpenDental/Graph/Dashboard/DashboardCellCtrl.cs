using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental.Graph.Dashboard
{
    public partial class DashboardCellCtrl : UserControl
    {
        public event EventHandler DeleteColumnButtonMouseEnter;
        public event EventHandler DeleteColumnButtonMouseLeave;
        public event EventHandler DeleteRowButtonMouseEnter;
        public event EventHandler DeleteRowButtonMouseLeave;
        public event EventHandler DeleteColumnButtonClick;
        public event EventHandler DeleteRowButtonClick;
        public event EventHandler DeleteCellButtonClick;

        private DashboardDockContainer _dockedControlHolder;
        private bool _isHightlighted;
        private bool _hasUnsavedChanges;

        private EventHandler OnEditClick => _dockedControlHolder?.OnEditClick;

        private EventHandler OnEditCancel
        {
            get { return _dockedControlHolder?.OnEditCancel ?? ((s, e) => { }); }
        }

        private EventHandler OnEditOk
        {
            get { return _dockedControlHolder?.OnEditOk ?? ((s, e) => { }); }
        }

        private EventHandler OnDropComplete => _dockedControlHolder?.OnDropComplete;

        private EventHandler OnRefreshCache
        {
            get { return _dockedControlHolder?.OnRefreshCache ?? ((s, e) => { }); }
        }

        public Control DockedControl => _dockedControlHolder?.Contr;

        public TableBase DockedControlTag => _dockedControlHolder?.DbItem;

        private DashboardDockContainer _dockedControl
        {
            get => _dockedControlHolder;
            set
            {
                if (value == null)
                {
                    _dockedControlHolder = null;
                    butDrag.Enabled = false;
                    butDeleteCell.Enabled = false;
                    butEdit.Enabled = false;
                    return;
                }

                if (value.Contr == this)
                {
                    return;
                }

                if (_dockedControlHolder != null)
                {
                    MessageBox.Show("This cell already has contains a graph. You must move or delete this graph before dragging a new graph onto the cell.");
                    return;
                }

                _dockedControlHolder = value;
                DockedControl.Dock = DockStyle.Fill;
                DockedControl.AllowDrop = true;
                Controls.Remove(pictureBox);
                Controls.Add(DockedControl);
                DockedControl.SendToBack();
                butDrag.Enabled = true;
                butDeleteCell.Enabled = true;
                butEdit.Enabled = OnEditClick != null;
                OnDropComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsHighlighted
        {
            get => _isHightlighted;
            set
            {
                if (IsHighlighted == value)
                {
                    return;
                }

                _isHightlighted = value;
                Refresh();
            }
        }

        public bool HasDockedControl => DockedControl != null;
        public bool IsEditMode { get; set; }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (value)
                {
                    return;
                }

                _hasUnsavedChanges = false;
            }
        }

        public DashboardCellCtrl() : this(null)
        {
        }

        public DashboardCellCtrl(DashboardDockContainer controlHolder)
        {
            InitializeComponent();

            _dockedControl = controlHolder;
        }

        public void RemoveDockedControl()
        {
            if (DockedControl == null)
            {
                return;
            }

            Controls.Remove(DockedControl);
            Controls.Add(pictureBox);

            pictureBox.SendToBack();
        }

        private void butDrag_MouseDown(object sender, MouseEventArgs e)
        {
            DockedControl?.DoDragDrop(_dockedControlHolder, DragDropEffects.All);
        }

        private void DashboardCell_DragDrop(object sender, DragEventArgs e)
        {
            BackColor = SystemColors.Control;

            _dockedControl = GetDroppedControl(e);

            _hasUnsavedChanges = true;
        }

        private void DashboardCell_DragEnter(object sender, DragEventArgs e)
        {
            if (!CanDrop(e))
            {
                return;
            }

            e.Effect = DragDropEffects.All;

            BackColor = Color.Red;
        }

        private void DashboardCell_DragLeave(object sender, EventArgs e)
        {
            BackColor = SystemColors.Control;
        }

        public bool CanDrop(DragEventArgs e)
        {
            return IsEditMode && GetDroppedControl(e) != null;
        }

        public DashboardDockContainer GetDroppedControl(DragEventArgs e)
        {
            if (DockedControl != null)
            {
                return null;
            }

            if (!e.Data.GetDataPresent(typeof(DashboardDockContainer)))
            {
                return null;
            }

            var holder = (DashboardDockContainer) e.Data.GetData(typeof(DashboardDockContainer));
            if (holder == null || holder.Contr == DockedControl)
            {
                return null;
            }

            return holder;
        }

        private void butRefresh_Click(object sender, EventArgs e)
        {
            OnRefreshCache(this, e);
        }

        private void butPrintPreview_Click(object sender, EventArgs e)
        {
            _dockedControlHolder?.Printer?.PrintPreview();
        }

        private void butEdit_Click(object sender, EventArgs e)
        {
            if (DockedControl == null)
            {
                return;
            }

            var holder = _dockedControl;
            OnEditClick?.Invoke(holder.Contr, EventArgs.Empty);

            var onEditOk = OnEditOk;
            var onEditCancel = OnEditCancel;

            using var formDashboardEditCell = new FormDashboardEditCell(holder.Contr, IsEditMode);
            if (formDashboardEditCell.ShowDialog() == DialogResult.OK)
            {
                _hasUnsavedChanges = true;

                onEditOk(holder.Contr, EventArgs.Empty);
            }
            else
            {
                onEditCancel(holder.Contr, EventArgs.Empty);
            }

            _dockedControl = holder;
        }

        private void butDeleteCell_Click(object sender, EventArgs e)
        {
            DeleteCellButtonClick?.Invoke(this, EventArgs.Empty);
        }

        private void butDeleteColumn_Click(object sender, EventArgs e)
        {
            DeleteColumnButtonClick?.Invoke(this, EventArgs.Empty);
        }

        private void butDeleteRow_Click(object sender, EventArgs e)
        {
            DeleteRowButtonClick?.Invoke(this, EventArgs.Empty);
        }

        private void butDeleteColumn_MouseEnter(object sender, EventArgs e)
        {
            DeleteColumnButtonMouseEnter?.Invoke(this, EventArgs.Empty);

            DashboardCell_MouseEnterLeave(sender, e);
        }

        private void butDeleteColumn_MouseLeave(object sender, EventArgs e)
        {
            DeleteColumnButtonMouseLeave?.Invoke(this, EventArgs.Empty);

            DashboardCell_MouseEnterLeave(sender, e);
        }

        private void butDeleteRow_MouseEnter(object sender, EventArgs e)
        {
            DeleteRowButtonMouseEnter?.Invoke(this, EventArgs.Empty);

            DashboardCell_MouseEnterLeave(sender, e);
        }

        private void butDeleteRow_MouseLeave(object sender, EventArgs e)
        {
            DeleteRowButtonMouseLeave?.Invoke(this, EventArgs.Empty);

            DashboardCell_MouseEnterLeave(sender, e);
        }

        private void butDeleteCell_MouseLeave(object sender, EventArgs e)
        {
            IsHighlighted = false;

            DashboardCell_MouseEnterLeave(sender, e);
        }

        private void butDeleteCell_MouseEnter(object sender, EventArgs e)
        {
            IsHighlighted = true;

            DashboardCell_MouseEnterLeave(sender, e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            if (e.Control != DockedControl)
            {
                return;
            }

            Controls.Add(pictureBox);
            pictureBox.SendToBack();
            _dockedControl = null;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            var controls = new List<Control> {e.Control};

            for (var i = 0; i < controls.Count; i++)
            {
                controls[i].MouseEnter += DashboardCell_MouseEnterLeave;
                controls[i].MouseLeave += DashboardCell_MouseEnterLeave;
                controls[i].MouseMove += DashboardCell_MouseEnterLeave;
                controls[i].MouseDown += DashboardCell_MouseDown;
                controls.AddRange(controls[i].Controls.Cast<Control>());
            }
        }

        private void DashboardCell_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || !IsEditMode)
            {
                return;
            }

            if (_dockedControlHolder == null)
            {
                deleteCellContentsToolStripMenuItem.Enabled = false;
                refreshToolStripMenuItem.Enabled = false;
                printToolStripMenuItem.Enabled = false;
                editToolStripMenuItem.Enabled = false;
            }

            contextMenuStripRight.Show(Cursor.Position);
        }

        private void DashboardCell_MouseEnterLeave(object sender, EventArgs e)
        {
            var rcThisToScreen = new Rectangle(new Point(0, 0), Size);
            var ptCursorScreen = PointToClient(Cursor.Position);
            if (rcThisToScreen.Contains(ptCursorScreen))
            {
                panelEditCell.Visible = IsEditMode;
                panelPrint.Visible = true;
            }
            else
            {
                panelEditCell.Visible = panelPrint.Visible = false;
            }
        }

        private void DashboardCell_Paint(object sender, PaintEventArgs e)
        {
            if (!IsHighlighted)
            {
                return;
            }

            const int thickness = 4;
            const int halfThickness = thickness / 2;

            using var pen = new Pen(Color.Red, thickness);

            e.Graphics.DrawRectangle(pen, new Rectangle(halfThickness, halfThickness, ClientSize.Width - thickness, ClientSize.Height - thickness));
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!panelEditCell.Visible && !panelPrint.Visible)
            {
                return;
            }

            DashboardCell_MouseEnterLeave(sender, e);
        }
    }
}