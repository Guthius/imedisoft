using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental;

public partial class ValidPhone : TextBox
{
    [Category("Behavior")]
    [DefaultValue(true)]
    [Description("Controls whether the content typed in will be automatically formatted (US and Canada only).")]
    public bool IsFormattingEnabled { get; set; } = true;
    
    public ValidPhone()
    {
        InitializeComponent();
    }

    private void ValidPhone_TextChanged(object sender, EventArgs e)
    {
        if (sender is not ValidPhone textPhone)
        {
            return;
        }

        if (!IsFormattingEnabled)
        {
            return;
        }

        var formattedText = TelephoneNumbers.AutoFormat(textPhone.Text);
        if (textPhone.Text == formattedText)
        {
            return;
        }

        var newSelectionStartPosition = Math.Max(textPhone.SelectionStart + formattedText.Length - textPhone.Text.Length, 0);
        
        textPhone.TextChanged -= ValidPhone_TextChanged;
        textPhone.Text = formattedText;
        textPhone.TextChanged += ValidPhone_TextChanged;
        textPhone.SelectionStart = newSelectionStartPosition;
    }
}