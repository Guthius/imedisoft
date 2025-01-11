using System;
using System.Collections.Generic;
using OpenDental.Graph.Base;
using OpenDentBusiness;

namespace OpenDental.Graph.Concrete
{
    public partial class BrokenApptGraphOptionsCtrl : BaseGraphOptionsCtrl
    {
        private List<Def> _listAdjTypes;

        public List<Def> ListAdjTypes => _listAdjTypes ??= Defs.GetPositiveAdjTypes();

        private List<BrokenApptProcedure> _listBrokenApptProcs;

        private List<BrokenApptProcedure> ListBrokenProcOptions
        {
            get
            {
                if (_listBrokenApptProcs != null)
                {
                    return _listBrokenApptProcs;
                }

                _listBrokenApptProcs = new List<BrokenApptProcedure>();
                switch ((BrokenApptProcedure) PrefC.GetInt(PrefName.BrokenApptProcedure))
                {
                    case BrokenApptProcedure.None:
                    case BrokenApptProcedure.Missed:
                        _listBrokenApptProcs.Add(BrokenApptProcedure.Missed);
                        break;
                    case BrokenApptProcedure.Cancelled:
                        _listBrokenApptProcs.Add(BrokenApptProcedure.Cancelled);
                        break;
                    case BrokenApptProcedure.Both:
                        _listBrokenApptProcs.Add(BrokenApptProcedure.Missed);
                        _listBrokenApptProcs.Add(BrokenApptProcedure.Cancelled);
                        _listBrokenApptProcs.Add(BrokenApptProcedure.Both);
                        break;
                }

                return _listBrokenApptProcs;
            }
        }

        public enum RunFor
        {
            Appointment,
            Procedure,
            Adjustment
        };

        public RunFor CurRunFor
        {
            get
            {
                if (radioRunAdjs.Checked)
                {
                    return RunFor.Adjustment;
                }

                return radioRunApts.Checked ? RunFor.Appointment : RunFor.Procedure;
            }
            set
            {
                switch (value)
                {
                    case RunFor.Adjustment:
                        radioRunAdjs.Checked = true;
                        break;

                    case RunFor.Appointment:
                        radioRunApts.Checked = true;
                        break;

                    case RunFor.Procedure:
                        radioRunProcs.Checked = true;
                        break;
                }
            }
        }

        public long AdjTypeDefNumCur
        {
            get => comboAdjType.SelectedIndex == -1 ? 0 : ListAdjTypes[comboAdjType.SelectedIndex].DefNum;
            set
            {
                for (var i = 0; i < ListAdjTypes.Count; i++)
                {
                    if (ListAdjTypes[i].DefNum != value)
                    {
                        continue;
                    }

                    comboAdjType.SelectedIndex = i;
                    return;
                }
            }
        }

        public BrokenApptProcedure BrokenApptCodeCur
        {
            get
            {
                if (comboBrokenProcType.SelectedIndex == -1)
                {
                    return (BrokenApptProcedure) PrefC.GetInt(PrefName.BrokenApptProcedure);
                }

                return ListBrokenProcOptions[comboBrokenProcType.SelectedIndex];
            }
            set
            {
                for (var i = 0; i < ListBrokenProcOptions.Count; i++)
                {
                    if (ListBrokenProcOptions[i] != value)
                    {
                        continue;
                    }

                    comboBrokenProcType.SelectedIndex = i;
                    return;
                }
            }
        }

        public BrokenApptGraphOptionsCtrl()
        {
            InitializeComponent();
            FillComboAdj();
            FillComboBrokenProc();
        }

        private void BrokenApptGraphOptionsCtrl_Load(object sender, EventArgs e)
        {
        }

        public override int GetPanelHeight()
        {
            return Height;
        }

        private void OnBrokenApptGraphOptionsChanged(object sender, EventArgs e)
        {
            comboAdjType.Enabled = radioRunAdjs.Checked;
            comboBrokenProcType.Enabled = radioRunProcs.Checked;

            OnBaseInputsChanged(sender);
        }

        private void FillComboAdj()
        {
            try
            {
                foreach (var adjType in ListAdjTypes)
                {
                    comboAdjType.Items.Add(adjType.ItemName);
                }
            }
            catch
            {
                // ignored
            }

            if (comboAdjType.Items.Count > 0)
            {
                return;
            }

            comboAdjType.Items.Add(Lans.g(this, "Adj types not setup"));
            radioRunAdjs.Enabled = false;
        }

        private void FillComboBrokenProc()
        {
            var index = 0;
            var brokenApptCodeDb = (BrokenApptProcedure) PrefC.GetInt(PrefName.BrokenApptProcedure);
            switch (brokenApptCodeDb)
            {
                case BrokenApptProcedure.None:
                case BrokenApptProcedure.Missed:
                    index = comboBrokenProcType.Items.Add(Lans.g(this, brokenApptCodeDb.ToString()) + ": (D9986)");
                    break;

                case BrokenApptProcedure.Cancelled:
                    index = comboBrokenProcType.Items.Add(Lans.g(this, brokenApptCodeDb.ToString()) + ": (D9987)");
                    break;

                case BrokenApptProcedure.Both:
                    comboBrokenProcType.Items.Add(Lans.g(this, BrokenApptProcedure.Missed.ToString()) + ": (D9986)");
                    comboBrokenProcType.Items.Add(Lans.g(this, BrokenApptProcedure.Cancelled.ToString()) + ": (D9987)");
                    index = comboBrokenProcType.Items.Add(Lans.g(this, brokenApptCodeDb.ToString()));
                    break;
            }

            comboBrokenProcType.SelectedIndex = index;
        }
    }
}