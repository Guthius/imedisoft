namespace CodeBase
{
    public class ProgressBarHelper
    {
        public string LabelValue = "";
        public string PercentValue = "";
        public double BlockValue;
        public double BlockMax;
        public string TagString = "";
        public ProgBarStyle ProgressStyle = ProgBarStyle.Marquee;
        public ProgBarEventType ProgressBarEventType = ProgBarEventType.ProgressBar;
        public int MarqueeSpeed;
        public string LabelTop = "";
        public bool IsValHidden;
        public bool IsTopHidden;
        public bool IsPercentHidden;
        public string ErrorMsg = "";

        public ProgressBarHelper()
        {
        }

        public ProgressBarHelper(string labelValue, string percentValue = "", double blockValue = 0, double blockMax = 0, ProgBarStyle progressStyle = ProgBarStyle.NoneSpecified, string tagString = "", int marqueeSpeed = 0, string labelTop = "", bool isLeftValHidden = false, bool isTopHidden = false, bool isPercentHidden = false, ProgBarEventType progressBarEventType = ProgBarEventType.ProgressBar, string errorMsg = "")
        {
            LabelValue = labelValue;
            PercentValue = percentValue;
            BlockValue = blockValue;
            BlockMax = blockMax;
            ProgressStyle = progressStyle;
            ProgressBarEventType = progressBarEventType;
            TagString = tagString;
            MarqueeSpeed = marqueeSpeed;
            LabelTop = labelTop;
            IsValHidden = isLeftValHidden;
            IsTopHidden = isTopHidden;
            IsPercentHidden = isPercentHidden;
            ErrorMsg = errorMsg;
        }
    }
}