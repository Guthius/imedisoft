using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeBase
{
    public partial class FormProgressExtended : FormProgressBase
    {
        private readonly List<ODProgressBar> _progressBars = [];
        
        public event ProgressPausedHandler ProgressPaused;
        public event ProgressCanceledHandler ProgressCanceled;

        private bool _isPaused;
        private bool _isDone;
        
        public FormProgressExtended(string cancelButtonText = null)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(cancelButtonText))
            {
                butCancel.Text = cancelButtonText;
            }
        }

        private ODProgressBar AddNewProgressBar(string leftLabel, string topLabel, string rightLabel, int blockValue, int blockMax, string tagString, ProgBarStyle progStyle, int marqSpeed, bool isLeftHidden, bool isTopHidden, bool isRightHidden)
        {
            if (_progressBars.Count > 10)
            {
                return null;
            }

            var pbar = new ODProgressBar(leftLabel, topLabel, rightLabel, blockValue, blockMax, tagString, progStyle, marqSpeed, isLeftHidden, isTopHidden, isRightHidden);
            
            pbar.TabStop = false;
            
            var rowLocation = tableLayoutPanel1.RowCount;
            
            tableLayoutPanel1.Controls.Add(pbar, 1, rowLocation);
            
            pbar.Name = "pbar" + rowLocation;
            
            return pbar;
        }

        private static void UpdateProgressBar(ODProgressBar progBar, ProgressBarHelper progHelper)
        {
            progBar.ODProgUpdate(progHelper.LabelValue, progHelper.LabelTop, progHelper.PercentValue, (int) progHelper.BlockValue, (int) progHelper.BlockMax, progHelper.TagString, progHelper.ProgressStyle, progHelper.MarqueeSpeed, progHelper.IsValHidden, progHelper.IsTopHidden, progHelper.IsPercentHidden);
        }

        protected sealed override void UpdateProgress(string status, ProgressBarHelper progHelper, bool hasProgHelper)
        {
            if (!hasProgHelper)
            {
                return;
            }
            
            switch (progHelper.ProgressBarEventType)
            {
                case ProgBarEventType.BringToFront:
                    TopMost = true;
                    TopMost = false;
                    break;
                    
                case ProgBarEventType.Header:
                    Text = status;
                    break;
                    
                case ProgBarEventType.ProgressLog:
                    label4.Text = status;
                    break;
                    
                case ProgBarEventType.TextMsg:
                    status = status.Trim();
                    textMsg.AppendText((string.IsNullOrWhiteSpace(textMsg.Text) ? "" : "\r\n") + status.PadRight(60));
                    break;
                    
                case ProgBarEventType.WarningOff:
                case ProgBarEventType.AllowResume:
                    labelWarning.Visible = false;
                    butPause.Enabled = true;
                    butPause.Text = _isPaused ? "Resume" : "Pause";
                    break;
                    
                case ProgBarEventType.Done:
                    butCancel.Visible = true;
                    butCancel.Text = "Close";
                    butPause.Enabled = false;
                    _isDone = true;
                    break;
                    
                case ProgBarEventType.HideButtons:
                    butPause.Visible = false;
                    butCancel.Visible = false;
                    break;
                    
                case ProgBarEventType.ProgressBar:
                default:
                    if (!_progressBars.Exists(x => x.TagString.ToLower() == progHelper.TagString.ToLower()))
                    {
                        var progBar = AddNewProgressBar(progHelper.LabelValue, progHelper.LabelTop, progHelper.PercentValue, (int) progHelper.BlockValue, (int) progHelper.BlockMax, progHelper.TagString, progHelper.ProgressStyle, progHelper.MarqueeSpeed, progHelper.IsValHidden, progHelper.IsTopHidden, progHelper.IsPercentHidden);
                        if (progBar == null)
                        {
                            break;
                        }

                        _progressBars.Add(progBar);
                    }
                    else
                    {
                        var odBar = _progressBars.Find(x => x.TagString.ToLower() == progHelper.TagString.ToLower());
                        UpdateProgressBar(odBar, progHelper);
                    }

                    break;
            }
        }

        private void butPause_Click(object sender, EventArgs e)
        {
            _isPaused = !_isPaused;
            
            ProgressPaused?.Invoke(this, new ProgressPausedArgs(_isPaused));
            
            if (_isPaused)
            {
                butPause.Text = "Resume";

                butPause.Enabled = false;
                labelWarning.Visible = true;
            }
            else
            {
                butPause.Text = "Pause";
            }
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            ProgressCanceled?.Invoke(this, EventArgs.Empty);
            
            if (_isDone)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                butCancel.Enabled = false;
                butPause.Enabled = false;
                labelWarning.Visible = true;
            }
        }
    }

    public delegate void ProgressCanceledHandler(object sender, EventArgs e);

    public delegate void ProgressPausedHandler(object sender, ProgressPausedArgs e);

    public class ProgressPausedArgs(bool isPaused)
    {
        public bool IsPaused = isPaused;
    }
}