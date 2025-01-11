using System;
using System.Windows.Forms;

namespace OpenDental.Graph.Base
{
    public partial class GroupingOptionsCtrl : UserControl
    {
        public event EventHandler InputsChanged;

        public enum Grouping
        {
            Provider,
            Clinic
        }

        public Grouping CurGrouping
        {
            get => radioGroupProvs.Checked ? Grouping.Provider : Grouping.Clinic;
            set
            {
                switch (value)
                {
                    case Grouping.Provider:
                        radioGroupProvs.Checked = true;
                        break;
                    case Grouping.Clinic:
                        radioGroupClinics.Checked = true;
                        break;
                }
            }
        }

        public GroupingOptionsCtrl()
        {
            InitializeComponent();
        }

        protected void OnBaseInputsChanged()
        {
            InputsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RadioGroupByChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton {Checked: false})
            {
                return;
            }

            OnBaseInputsChanged();
        }
    }
}