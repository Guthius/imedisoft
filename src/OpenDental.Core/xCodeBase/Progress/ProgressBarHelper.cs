namespace CodeBase;

public class ProgressBarHelper(string labelValue, string percentValue = "", double blockValue = 0, double blockMax = 0, ProgBarStyle progressStyle = ProgBarStyle.NoneSpecified, string tagString = "", int marqueeSpeed = 0, string labelTop = "", bool isLeftValHidden = false, bool isTopHidden = false, bool isPercentHidden = false, ProgBarEventType progressBarEventType = ProgBarEventType.ProgressBar, string errorMsg = "")
{
    public readonly string LabelValue = labelValue;
    public readonly string PercentValue = percentValue;
    public readonly double BlockValue = blockValue;
    public readonly double BlockMax = blockMax;
    public readonly string TagString = tagString;
    public ProgBarStyle ProgressStyle = progressStyle;
    public readonly ProgBarEventType ProgressBarEventType = progressBarEventType;
    public readonly int MarqueeSpeed = marqueeSpeed;
    public readonly string LabelTop = labelTop;
    public readonly bool IsValHidden = isLeftValHidden;
    public readonly bool IsTopHidden = isTopHidden;
    public readonly bool IsPercentHidden = isPercentHidden;
    public readonly string ErrorMsg = errorMsg;
}