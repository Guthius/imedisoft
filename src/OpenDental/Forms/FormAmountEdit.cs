using System;
using System.Windows.Forms;
using DataConnectionBase;

namespace OpenDental.Forms;

public partial class FormAmountEdit : FormODBase
{
    private readonly string _text;

    public decimal Amount;

    public FormAmountEdit(string text)
    {
        InitializeComponent();

        _text = text;
    }

    private void FormAmountEdit_Load(object sender, EventArgs e)
    {
        labelText.Text = _text;
        textAmount.Text = SOut.Decimal(Amount);
        textAmount.SelectionStart = 0;
        textAmount.SelectionLength = textAmount.Text.Length;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        Amount = SIn.Decimal(textAmount.Text);
        DialogResult = DialogResult.OK;
    }
}