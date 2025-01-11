using System;
using OpenDental.Graph.Base;

namespace OpenDental.Graph.Concrete
{
    public partial class HqMessagesRealTimeOptionsCtrl : BaseGraphOptionsCtrl
    {
        public HqMessagesRealTimeOptionsCtrl()
        {
            InitializeComponent();
        }

        public override int GetPanelHeight()
        {
            return Height;
        }

        public override bool HasGroupOptions => false;

        private void OnBrokenApptGraphOptionsChanged(object sender, EventArgs e)
        {
            OnBaseInputsChanged(sender);
        }
    }
}