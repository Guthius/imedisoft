using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OpenDental;

public class ValidNum : TextBox
{
    private readonly Container _components = null;
    private readonly ErrorProvider _errorProvider1 = new();

    public ValidNum()
    {
        InitializeComponent();
        
        _errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Validating += ValidNum_Validating;
        Validated += ValidNum_Validated;

        ResumeLayout(false);
    }

    [Category("OD")]
    [Description("The minimum value that user can enter.")]
    [DefaultValue(0)]
    public int MinVal { get; set; } = 0;

    [Category("OD")]
    [Description("The maximum value that user can enter.")]
    [DefaultValue(255)]
    public int MaxVal { get; set; } = 255;

    [Category("OD")]
    [Description("When true, a zero value will show as zero instead of blank. Also when true, a blank entry will not be allowed. Default is true.")]
    [DefaultValue(true)]
    public bool ShowZero { get; set; } = true;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get
        {
            if (!IsValid())
            {
                throw new Exception(_errorProvider1.GetError(this));
            }

            if (!ShowZero && Text == "")
            {
                return 0;
            }

            return Convert.ToInt32(Text);
        }
        set
        {
            if (value == 0 && !ShowZero)
            {
                Text = "";
            }
            else
            {
                Text = value.ToString();
            }

            ParseValue();
        }
    }

    public bool IsValid()
    {
        ParseValue();

        return string.IsNullOrEmpty(_errorProvider1.GetError(this));
    }

    private void ValidNum_Validating(object sender, CancelEventArgs e)
    {
        ParseValue();
    }

    private void ValidNum_Validated(object sender, EventArgs e)
    {
    }

    private void ParseValue()
    {
        if (DesignMode)
        {
            return;
        }

        if (!ShowZero && Text == "")
        {
            _errorProvider1.SetError(this, ""); //sets no error message. Empty is OK.
            return;
        }

        int value;
        try
        {
            value = Convert.ToInt32(Text);
        }
        catch
        {
            _errorProvider1.SetError(this, "Must be a number. No letters or symbols allowed");
            return;
        }

        if (value > MaxVal)
        {
            _errorProvider1.SetError(this, "Number must be less than or equal to " + MaxVal);
            return;
        }

        if (value < MinVal)
        {
            _errorProvider1.SetError(this, "Number must be greater than or equal to " + MinVal);
            return;
        }

        _errorProvider1.SetError(this, "");
    }
}