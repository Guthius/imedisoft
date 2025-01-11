using System.Windows.Forms;

namespace OpenDental.Graph.Dashboard
{
    public partial class FormDashboardNamePrompt : Form
    {
        public delegate bool ValidateTabNameArgs(string tabName);

        private readonly ValidateTabNameArgs _validateTabNameArgs;

        public string TabName => textBoxTabName.Text;

        public FormDashboardNamePrompt(string tabName, ValidateTabNameArgs validateTabNameArgs)
        {
            InitializeComponent();
            textBoxTabName.Text = tabName;
            _validateTabNameArgs = validateTabNameArgs;
        }

        private void FormDashboardNamePrompt_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                return;
            }

            if (string.IsNullOrEmpty(TabName))
            {
                MessageBox.Show("Tab Name is empty.");
                e.Cancel = true;
            }

            if (!_validateTabNameArgs(TabName))
            {
                e.Cancel = true;
            }
        }
    }
}