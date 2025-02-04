using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDentBusiness.WebTypes.Shared.XWeb;

namespace OpenDental;

public partial class FormXWeb : FormODBase
{
    private readonly CreditCard _creditCard;
    private XWebTransactionType _xWebTransactionType;
    private readonly bool _createPayment;
    private readonly double _payAmtOriginal;

    public bool LockCardInfo { get; set; }
    public XWebResponse XWebResponse { get; set; }

    public FormXWeb(CreditCard creditCard, XWebTransactionType xWebTransactionType, bool createPayment, double payAmtOriginal = 0)
    {
        InitializeComponent();

        _creditCard = creditCard;
        _xWebTransactionType = xWebTransactionType;
        _createPayment = createPayment;
        _payAmtOriginal = payAmtOriginal;
    }

    private void FormXWeb_Load(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.PaymentCreate, DateTime.Today))
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (_xWebTransactionType == XWebTransactionType.CreditReturnTransaction)
        {
            radioReturn.Checked = true;
        }

        if (_creditCard is not null)
        {
            textCardNumber.Text = _creditCard.CCNumberMasked;
            textExpDate.Text = _creditCard.CCExpiration.ToString("MMy");
            textZipCode.Text = _creditCard.Zip;
        }

        if (!CompareDouble.IsZero(_payAmtOriginal))
        {
            textAmount.Text = _payAmtOriginal.ToString(CultureInfo.InvariantCulture);
        }

        if (!LockCardInfo)
        {
            return;
        }

        textCardNumber.ReadOnly = true;
        textCardNumber.BackColor = SystemColors.Control;
        textExpDate.ReadOnly = true;
        textExpDate.BackColor = SystemColors.Control;
        textNameOnCard.ReadOnly = true;
        textNameOnCard.BackColor = SystemColors.Control;
        textSecurityCode.ReadOnly = true;
        textSecurityCode.BackColor = SystemColors.Control;
        textZipCode.ReadOnly = true;
        textZipCode.BackColor = SystemColors.Control;
        textAmount.Focus();
    }

    private bool VerifyData()
    {
        if (textCardNumber.Text.Trim().Length < 5)
        {
            ShowError("Invalid Card Number.");
            return false;
        }

        try
        {
            if (!Regex.IsMatch(textExpDate.Text, @"^\d\d[/\- ]\d\d$") &&
                !Regex.IsMatch(textExpDate.Text, @"^\d{4}$"))
            {
                ShowError("Expiration format invalid.");
                return false;
            }
        }
        catch (Exception)
        {
            ShowError("Expiration format invalid.");
            return false;
        }

        if (_creditCard is null)
        {
            if (textCardNumber.Text.Any(x => !char.IsDigit(x)))
            {
                ShowError("Invalid card number.");
                return false;
            }
        }
        else if (_creditCard.XChargeToken == "" && Regex.IsMatch(textCardNumber.Text, @"X+[0-9]{4}"))
        {
            ShowError("There is no saved XWeb token for this credit card.  The card number and expiration must be re-entered.");
            return false;
        }

        if (!Regex.IsMatch(textAmount.Text, "^[0-9]+$") && !Regex.IsMatch(textAmount.Text, "^[0-9]*\\.[0-9]+$"))
        {
            ShowError("Invalid amount.");
            return false;
        }

        if (_xWebTransactionType == XWebTransactionType.CreditVoidTransaction && textRefNumber.Text == "")
        {
            ShowError("Ref Number required.");
            return false;
        }

        if (textPayNote.Text == "")
        {
            ShowError("Payment note required.");
            return false;
        }

        if (_payAmtOriginal == 0 || !CompareDouble.IsGreaterThan(Math.Abs(SIn.Double(textAmount.Text)), Math.Abs(_payAmtOriginal)))
        {
            return true;
        }

        ShowError("Amount cannot be greater than the original payment amount.");
        return false;
    }

    private bool ProcessSelectedTransaction()
    {
        var amount = SIn.Double(textAmount.Text);

        Cursor = Cursors.WaitCursor;

        if (_xWebTransactionType == XWebTransactionType.CreditReturnTransaction)
        {
            try
            {
                XWebResponse = XWebs.ReturnPayment(_creditCard.PatNum, textPayNote.Text, amount, _creditCard.CreditCardNum, _createPayment);
            }
            catch (ODException ex)
            {
                Cursor = Cursors.Default;

                ShowError(ex.Message);

                return false;
            }
        }

        Cursor = Cursors.Default;
        return true;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        _xWebTransactionType = radioReturn.Checked ? XWebTransactionType.CreditReturnTransaction : XWebTransactionType.Undefined;

        if (!VerifyData())
        {
            return;
        }

        if (ProcessSelectedTransaction())
        {
            DialogResult = DialogResult.OK;
        }
    }
}