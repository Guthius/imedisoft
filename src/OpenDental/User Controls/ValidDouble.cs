using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace OpenDental;

public class ValidDouble : TextBox
{
    private readonly ErrorProvider _errorProvider1 = new();

    [Category("OD")]
    [Description("The maximum value that user can enter.")]
    public double MaxVal { get; set; } = 100000000;

    [Category("OD")]
    [Description("The minimum value that user can enter.")]
    public double MinVal { get; set; } = -100000000;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Value
    {
        get
        {
            if (!IsValid())
            {
                throw new Exception(_errorProvider1.GetError(this));
            }

            if (Text == "" && MinVal == 0.01)
            {
                return 0.01;
            }

            if (Text == "")
            {
                return 0;
            }

            return Convert.ToDouble(Text);
        }
        set
        {
            Text = value.ToString(CultureInfo.InvariantCulture);

            ParseValue();
        }
    }

    public bool IsValid()
    {
        ParseValue();

        return string.IsNullOrEmpty(_errorProvider1.GetError(this));
    }

    public ValidDouble()
    {
        InitializeComponent();

        _errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Validating += ValidNum_Validating;
        
        ResumeLayout(false);
    }

    private void ValidNum_Validating(object sender, CancelEventArgs e)
    {
        ParseValue();
    }

    private void ParseValue()
    {
        if (Text == "" && MinVal == 0.01)
        {
            _errorProvider1.SetError(this, "");
            
            return;
        }

        if (Text == "")
        {
            if (0 < MinVal || 0 > MaxVal)
            {
                _errorProvider1.SetError(this, "Zero or blank is not allowed.");
                
                return;
            }

            _errorProvider1.SetError(this, "");
            return;
        }

        try
        {
            if (Convert.ToDouble(Text) > MaxVal)
            {
                throw new Exception("Number must be less than or equal to " + MaxVal);
            }

            if (Convert.ToDouble(Text) < MinVal)
            {
                throw new Exception("Number must be greater than or equal to " + (MinVal));
            }

            _errorProvider1.SetError(this, "");
        }
        catch (Exception ex)
        {
            var message = ex.Message == "Input string was not in a correct format." ? "Must be a number. No letters or symbols allowed" : ex.Message;

            _errorProvider1.SetError(this, message);
        }
    }
}
