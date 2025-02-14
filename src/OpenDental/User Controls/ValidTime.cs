using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenDental;

public class ValidTime : TextBox
{
    private readonly ErrorProvider _errorProvider = new();
    private readonly Container _components = null;
    
    public bool IsValid()
    {
        return _errorProvider.GetError(this) == "";
    }

    [Category("OD")]
    [Description("Default is false, meaning the format should look like '10:05:30 PM' for en-us. If short true, format should look like '10:05 PM'.")]
    [DefaultValue(false)]
    public bool IsShortTimeString { get; set; }

    public ValidTime()
    {
        InitializeComponent();
        
        _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        
        Size = new Size(120, 20);
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

        Validating += ValidTime_Validating;
        
        ResumeLayout(false);
    }
    
    private void ValidTime_Validating(object sender, CancelEventArgs e)
    {
        try
        {
            if (Text == "")
            {
                _errorProvider.SetError(this, "");
                return;
            }

            Text = IsShortTimeString ? DateTime.Parse(Text).ToShortTimeString() : DateTime.Parse(Text).ToLongTimeString();

            _errorProvider.SetError(this, "");
        }
        catch (Exception ex)
        {
            var message = ex.Message == "String was not recognized as a valid time." ? "Invalid time" : ex.Message;

            _errorProvider.SetError(this, message);
        }
    }
    
    public void ClearError()
    {
        _errorProvider.SetError(this, "");
    }
}