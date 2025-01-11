using System;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental.Graph.Base
{
    public partial class BaseGraphOptionsCtrl : UserControl
    {
        public event EventHandler InputsChanged;

        public BaseGraphOptionsCtrl()
        {
            InitializeComponent();
        }

        protected void OnBaseInputsChanged(object sender)
        {
            if (sender is RadioButton {Checked: false})
            {
                return;
            }

            InputsChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual int GetPanelHeight()
        {
            return 63;
        }
        
        public virtual bool HasGroupOptions => true;
    }
}