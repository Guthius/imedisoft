using System;
using System.Windows.Forms;
using OpenDental.Graph.Base;

namespace OpenDental.Graph.Dashboard
{
    public partial class FormDashboardEditCell : Form
    {
        private readonly IODGraphPrinter _printer;

        public FormDashboardEditCell(Control dockControl, bool allowSave)
        {
            InitializeComponent();
            if (dockControl is IODGraphPrinter printer)
            {
                _printer = printer;
            }

            if (dockControl is IDashboardDockContainer container)
            {
                Text = "Edit Cell - " + container.GetCellType();
            }

            splitContainer.Panel1.Controls.Add(dockControl);
            butOk.Enabled = allowSave;
            if (!allowSave)
            {
                labelChanges.Visible = true;
            }
        }

        private void butPrintPreview_Click(object sender, EventArgs e)
        {
            _printer?.PrintPreview();
        }
    }
}