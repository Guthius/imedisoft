using System.Windows.Forms;

namespace CodeBase
{
    public partial class ODProgressBar : UserControl
    {
        private ProgBarStyle _progressStyle;
        private bool _isLabelTopHidden;
        private bool _isLabelRightHidden;
        private bool _isLabelLeftHidden;

        public string TagString { get; set; }

        public ODProgressBar()
        {
            InitializeComponent();
            labelLeftText.Text = "";
            labelTopText.Text = "";
            labelPercentComplete.Text = "";
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            TagString = "";
            _progressStyle = ProgBarStyle.NoneSpecified;
            progressBar.MarqueeAnimationSpeed = 0;
            _isLabelLeftHidden = false;
            _isLabelTopHidden = false;
            _isLabelRightHidden = false;
        }

        public ODProgressBar(string labelLeftText, string labelTopText = "", string labelPercentText = "", int blockValue = 0, int blockMax = 100, string tagString = "", ProgBarStyle progressStyle = ProgBarStyle.NoneSpecified, int marqueeSpeed = 0, bool isLabelLeftHidden = false, bool isLabelTopHidden = false, bool isLabelRightHidden = false)
        {
            InitializeComponent();
            
            this.labelLeftText.Text = labelLeftText;
            this.labelTopText.Text = labelTopText;
            
            labelPercentComplete.Text = labelPercentText;
            progressBar.Maximum = blockMax;
            progressBar.Value = blockValue;
            TagString = tagString;
            
            switch (progressStyle)
            {
                case ProgBarStyle.Blocks:
                    progressBar.Style = ProgressBarStyle.Blocks;
                    break;
                
                case ProgBarStyle.Marquee:
                    progressBar.Style = ProgressBarStyle.Marquee;
                    break;
                
                case ProgBarStyle.NoneSpecified:
                case ProgBarStyle.Continuous:
                default:
                    progressBar.Style = ProgressBarStyle.Continuous;
                    break;
            }

            progressBar.MarqueeAnimationSpeed = marqueeSpeed;
            
            _isLabelLeftHidden = isLabelLeftHidden;
            _isLabelTopHidden = isLabelTopHidden;
            _isLabelRightHidden = isLabelRightHidden;
            
            LayoutHelper();
        }

        public void ODProgUpdate(string labelLeftText, string labelTopText, string labelPercentText, int blockValue, int blockMax, string tagString, ProgBarStyle progressStyle, int marqueeSpeed = 0, bool isLabelLeftHidden = false, bool isLabelTopHidden = false, bool isLabelRightHidden = false)
        {
            this.labelLeftText.Text = labelLeftText;
            this.labelTopText.Text = labelTopText;
            
            if (labelPercentText != null)
            {
                labelPercentComplete.Text = labelPercentText;
            }

            if (blockMax != 0)
            {
                progressBar.Maximum = blockMax;
            }

            if (blockValue != 0)
            {
                if (blockValue > progressBar.Maximum || blockValue < progressBar.Minimum)
                {
                    progressBar.Value = progressBar.Maximum;
                }
                else
                {
                    progressBar.Value = blockValue;
                }
            }

            progressBar.Tag = tagString;
            switch (progressStyle)
            {
                case ProgBarStyle.Blocks:
                    progressBar.Style = ProgressBarStyle.Blocks;
                    break;
                
                case ProgBarStyle.Marquee:
                    progressBar.Style = ProgressBarStyle.Marquee;
                    break;
                
                case ProgBarStyle.NoneSpecified:
                case ProgBarStyle.Continuous:
                default:
                    progressBar.Style = ProgressBarStyle.Continuous;
                    break;
            }

            if (marqueeSpeed != 0)
            {
                progressBar.MarqueeAnimationSpeed = marqueeSpeed;
            }

            progressBar.MarqueeAnimationSpeed = marqueeSpeed;
            
            _isLabelLeftHidden = isLabelLeftHidden;
            _isLabelTopHidden = isLabelTopHidden;
            _isLabelRightHidden = isLabelRightHidden;
            
            LayoutHelper();
        }

        private void LayoutHelper()
        {
            if (_isLabelLeftHidden)
            {
                labelLeftText.Visible = false;
                progressBar.SetBounds(labelLeftText.Location.X, progressBar.Location.Y
                    , (labelLeftText.Location.X) + (labelPercentComplete.Location.X - 6), progressBar.Height);
            }

            if (_isLabelRightHidden)
            {
                labelPercentComplete.Visible = false;
                progressBar.Width = Width - (progressBar.Location.X + 6);
            }

            if (_isLabelTopHidden)
            {
                labelTopText.Visible = false;
                Height -= labelTopText.Height - 5;
            }
        }
    }
}