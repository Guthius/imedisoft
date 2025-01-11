using System;
using OpenDental.Graph.Base;

namespace OpenDental.Graph.Concrete
{
    public partial class IncomeGraphOptionsCtrl : BaseGraphOptionsCtrl
    {
        public bool IncludePaySplits
        {
            get => checkIncludePaySplits.Checked;
            set => checkIncludePaySplits.Checked = value;
        }

        public bool IncludeInsuranceClaimPayments
        {
            get => checkIncludeInsuranceClaimPayments.Checked;
            set => checkIncludeInsuranceClaimPayments.Checked = value;
        }
        
        public IncomeGraphOptionsCtrl()
        {
            InitializeComponent();
        }

        public override int GetPanelHeight()
        {
            return Height;
        }

        private void OnIncomeGraphInputsChanged(object sender, EventArgs e)
        {
            OnBaseInputsChanged(sender);
        }
    }
}