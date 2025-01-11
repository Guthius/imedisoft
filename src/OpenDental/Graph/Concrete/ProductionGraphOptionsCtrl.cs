using System;
using OpenDental.Graph.Base;

namespace OpenDental.Graph.Concrete
{
    public partial class ProductionGraphOptionsCtrl : BaseGraphOptionsCtrl
    {
        public bool IncludeAdjustments
        {
            get => checkIncludeAdjustments.Checked;
            set => checkIncludeAdjustments.Checked = value;
        }

        public bool IncludeCompletedProcs
        {
            get => checkIncludeCompletedProcs.Checked;
            set => checkIncludeCompletedProcs.Checked = value;
        }

        public bool IncludeWriteoffs
        {
            get => checkIncludeWriteoffs.Checked;
            set => checkIncludeWriteoffs.Checked = value;
        }

        public ProductionGraphOptionsCtrl()
        {
            InitializeComponent();
        }

        public override int GetPanelHeight()
        {
            return Height;
        }

        private void OnProductionGraphInputsChanged(object sender, EventArgs e)
        {
            OnBaseInputsChanged(sender);
        }
    }
}