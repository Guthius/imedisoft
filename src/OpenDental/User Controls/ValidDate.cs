using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OpenDental;

public partial class ValidDate : TextBox
{
    private readonly ErrorProvider _errorProvider = new();
        
    public ValidDate()
    {
        InitializeComponent();
            
        _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime Value
    {
        get
        {
            if (!IsValid())
            {
                throw new Exception(_errorProvider.GetError(this));
            }

            return Text == "" ? DateTime.MinValue : DateTime.Parse(Text);
        }
        set
        {
            Text = value == DateTime.MinValue ? "" : value.ToShortDateString();

            ParseValue();
        }
    }

    public bool IsValid()
    {
        ParseValue();
        return string.IsNullOrEmpty(_errorProvider.GetError(this));
    }

    public void Validate()
    {
        ParseValue();
    }

    private void ValidDate_Validating(object sender, CancelEventArgs e)
    {
        ParseValue();
    }

    private void ParseValue()
    {
        if (Text == "")
        {
            _errorProvider.SetError(this, "");
            return;
        }

        var allNums = true;
        for (var i = 0; i < Text.Length; i++)
        {
            if (!char.IsNumber(Text, i))
            {
                allNums = false;
            }
        }

        DateTime dateTime;
        if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en")
        {
            if (allNums)
            {
                Text = Text.Length switch
                {
                    4 => Text.Substring(0, 2) + "/" + Text.Substring(2, 2),
                    6 => Text.Substring(0, 2) + "/" + Text.Substring(2, 2) + "/" + Text.Substring(4, 2),
                    8 => Text.Substring(0, 2) + "/" + Text.Substring(2, 2) + "/" + Text.Substring(4, 4),
                    _ => Text
                };
            }
        }

        try
        {
            Text = DateTime.Parse(Text).ToString("d");
                
            dateTime = DateTime.Parse(Text);
        }
        catch
        {
            _errorProvider.SetError(this, "Invalid date.");
            return;
        }

        switch (dateTime.Year)
        {
            case < 1880:
            case > 2100:
                _errorProvider.SetError(this, "Valid dates between 1880 and 2100");
                return;
                
            default:
                _errorProvider.SetError(this, "");
                break;
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        if (ReadOnly)
        {
            return;
        }

        base.OnKeyPress(e);

        if (e.KeyChar != '+' && e.KeyChar != '-')
        {
            return;
        }
            
        var regex = new Regex("[^0-9]");
        if (regex.Matches(Text).Count < 2)
        {
            return;
        }

        DateTime dateDisplayed;
        try
        {
            dateDisplayed = DateTime.Parse(Text);
        }
        catch
        {
            return;
        }

        var caret = SelectionStart;
            
        dateDisplayed = e.KeyChar switch
        {
            '+' when dateDisplayed.Date < DateTime.MaxValue.Date => dateDisplayed.AddDays(1),
            '-' when dateDisplayed.Date > DateTime.MinValue.Date => dateDisplayed.AddDays(-1),
            _ => dateDisplayed
        };

        Text = dateDisplayed.ToShortDateString();
        SelectionStart = caret;
            
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (ReadOnly)
        {
            return;
        }

        base.OnKeyDown(e);
        if (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down)
        {
            return;
        }

        DateTime dateDisplayed;
        try
        {
            dateDisplayed = DateTime.Parse(Text);
        }
        catch
        {
            return;
        }

        var caret = SelectionStart;

        dateDisplayed = e.KeyCode switch
        {
            Keys.Up when dateDisplayed.Date < DateTime.MaxValue.Date => dateDisplayed.AddDays(1),
            Keys.Down when dateDisplayed.Date > DateTime.MinValue.Date => dateDisplayed.AddDays(-1),
            _ => dateDisplayed
        };

        Text = dateDisplayed.ToShortDateString();
        SelectionStart = caret;
            
        e.Handled = true;
    }
}