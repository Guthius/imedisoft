using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CDT;
using CodeBase;
using DataConnectionBase;
using DentalXChange.Dps.Pos;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.Bridges;
using OpenDental.Properties;
using OpenDental.UI;
using OpenDentBusiness;
using PayConnectService = OpenDentBusiness.PayConnectService;

namespace OpenDental;

public partial class FormPayConnect : FormODBase
{
    private readonly Patient _patient;
    private readonly decimal _amountInit;
    private readonly bool _isAddingCard;
    private readonly long _clinicNum;

    private PayConnectService.transResponse _transResponse;
    private MagstripCardParser _magstripCardParser;
    private CreditCard _creditCard;
    private Program _program;
    private PayConnectResponse _payConnectResponse;
    private bool _hasSwipedCard;
    private int _expYear;
    private int _expMonth;

    public PayConnectService.creditCardRequest CreditCardRequest;
    public string ReceiptStr = string.Empty;
    public PayConnectService.transType TransType = PayConnectService.transType.SALE;
    public bool WasPaymentAttempted;

    public FormPayConnect(long clinicNum, Patient patient, decimal amount, CreditCard creditCard, bool isAddingCard = false)
    {
        InitializeComponent();

        _clinicNum = clinicNum;
        _patient = patient;
        _amountInit = amount;
        _creditCard = creditCard;
        _isAddingCard = isAddingCard;
    }

    private void FormPayConnect_Load(object sender, EventArgs e)
    {
        _program = Programs.GetCur(ProgramName.PayConnect);
        if (_program is null)
        {
            ShowError("PayConnect does not exist in the database.");
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (SIn.Bool(ProgramProperties.GetPropVal(_program.ProgramNum, "TerminalProcessingEnabled", _clinicNum)))
        {
            try
            {
                ODFileUtils.WriteAllText("DpsPos.dll.config", Resources.DpsPos_dll_config, false);
            }
            catch (Exception ex)
            {
                ShowException(ex, "Unable to create the config file for the terminal. Trying running the program as an administrator.");
            }
        }

        textAmount.Text = SOut.Decimal(_amountInit);
        if (_patient is null)
        {
            radioAuthorization.Enabled = false;
            radioVoid.Enabled = false;
            radioReturn.Enabled = false;
            textZipCode.ReadOnly = true;
            textNameOnCard.ReadOnly = true;
            checkSaveToken.Enabled = false;
            sigBoxWrapper.Enabled = false;
        }
        else
        {
            textZipCode.Text = _patient.Zip;
            textNameOnCard.Text = _patient.GetNameFL();

            checkSaveToken.Checked = PrefC.GetBool(PrefName.StoreCCtokens);

            if (PrefC.GetBool(PrefName.StoreCCnumbers))
            {
                labelStoreCCNumWarning.Visible = true;
            }

            FillFieldsFromCard();
        }

        if (!SIn.Bool(ProgramProperties.GetPropVal(_program.ProgramNum, "TerminalProcessingEnabled", _clinicNum)) || _isAddingCard)
        {
            groupProcessMethod.Visible = false;
            Height -= 55;
        }
        else
        {
            var procMethod = ProgramProperties.GetPropValForClinicOrDefault(_program.ProgramNum, PayConnect.ProgramProperties.DefaultProcessingMethod, _clinicNum);

            switch (procMethod)
            {
                case "0":
                    radioWebService.Checked = true;
                    break;

                case "1":
                    radioTerminal.Checked = true;
                    break;
            }
        }

        if (_isAddingCard)
        {
            radioAuthorization.Checked = true;
            TransType = PayConnectService.transType.AUTH;
            groupTransType.Enabled = false;
            labelAmount.Visible = false;
            textAmount.Visible = false;
            checkSaveToken.Checked = true;
            checkSaveToken.Enabled = false;
            checkForceDuplicate.Checked = true;
            checkForceDuplicate.Enabled = false;
        }

        if (SIn.Bool(ProgramProperties.GetPropVal(_program.ProgramNum, PayConnect.ProgramProperties.PayConnectPreventSavingNewCC, _clinicNum)))
        {
            textCardNumber.ReadOnly = true;
        }
    }

    private void FillFieldsFromCard()
    {
        if (_creditCard is null)
        {
            return;
        }

        if (_creditCard.CCNumberMasked != "")
        {
            var ccNum = _creditCard.CCNumberMasked;
            if (Regex.IsMatch(ccNum, "^\\d{12}(\\d{0,7})"))
            {
                var idxLast4Digits = ccNum.Length - 4;

                ccNum = new string('X', 12) + ccNum.Substring(idxLast4Digits);
            }

            textCardNumber.Text = ccNum;
        }

        if (_creditCard.CCExpiration is {Year: > 2005})
        {
            textExpDate.Text = _creditCard.CCExpiration.ToString("MMyy");
        }

        if (_creditCard.Zip != "")
        {
            textZipCode.Text = _creditCard.Zip;
        }

        if (_creditCard.PayConnectToken != "" && _creditCard.PayConnectTokenExp > DateTime.MinValue)
        {
            checkSaveToken.Checked = true;
            checkSaveToken.Enabled = false;
            textNameOnCard.ReadOnly = true;
            textCardNumber.ReadOnly = true;
            textExpDate.ReadOnly = true;
        }
        else if (!string.IsNullOrEmpty(_creditCard.XChargeToken) || !string.IsNullOrEmpty(_creditCard.PaySimpleToken))
        {
            textCardNumber.Text = "";
        }
    }

    private void radioSale_Click(object sender, EventArgs e)
    {
        radioForce.Checked = false;
        textRefNumber.Visible = false;
        labelRefNumber.Visible = false;

        TransType = PayConnectService.transType.SALE;

        if (radioWebService.Checked)
        {
            textCardNumber.Focus();
        }
        else
        {
            textAmount.Focus();
        }
    }

    private void radioAuthorization_Click(object sender, EventArgs e)
    {
        radioForce.Checked = false;
        textRefNumber.Visible = false;
        labelRefNumber.Visible = false;

        TransType = PayConnectService.transType.AUTH;

        if (radioWebService.Checked)
        {
            textCardNumber.Focus();
        }
        else
        {
            textAmount.Focus();
        }
    }

    private void radioVoid_Click(object sender, EventArgs e)
    {
        radioForce.Checked = false;
        textRefNumber.Visible = true;
        labelRefNumber.Visible = true;
        labelRefNumber.Text = "Ref Number";

        TransType = PayConnectService.transType.VOID;

        if (radioWebService.Checked)
        {
            textCardNumber.Focus();
        }
        else
        {
            textAmount.Focus();
        }
    }

    private void radioReturn_Click(object sender, EventArgs e)
    {
        radioForce.Checked = false;
        textRefNumber.Visible = true;
        labelRefNumber.Visible = true;
        labelRefNumber.Text = "Ref Number";

        TransType = PayConnectService.transType.RETURN;

        textSecurityCode.Text = "";
        if (radioWebService.Checked)
        {
            textCardNumber.Focus();
        }
        else
        {
            textAmount.Focus();
        }
    }

    private void radioReturn_Changed(object sender, EventArgs e)
    {
        textSecurityCode.Enabled = !radioReturn.Checked;
    }

    private void radioForce_Click(object sender, EventArgs e)
    {
        radioSale.Checked = false;
        radioAuthorization.Checked = false;
        radioVoid.Checked = false;
        radioReturn.Checked = false;
        radioForce.Checked = true;
        textRefNumber.Visible = true;
        labelRefNumber.Visible = true;
        labelRefNumber.Text = Lan.g(this, "Authorization Code");
        TransType = PayConnectService.transType.FORCE;
        if (radioWebService.Checked)
        {
            textCardNumber.Focus(); //Usually transaction type is chosen before card number is entered, but textCardNumber box must be selected in order for card swipe to work.
        }
        else
        {
            textAmount.Focus();
        }
    }

    private void radioWebService_CheckedChanged(object sender, EventArgs e)
    {
        if (!radioWebService.Checked)
        {
            return;
        }

        new[] {textCardNumber, textExpDate, textNameOnCard, textSecurityCode, textZipCode, textRefNumber, textAmount}.ForEach(x => x.ReadOnly = false);
        radioForce.Enabled = true;
        checkSaveToken.Enabled = true;
        checkForceDuplicate.Enabled = true;
        FillFieldsFromCard();
        textNameOnCard.Text = _patient.GetNameFL();
        if (SIn.Bool(ProgramProperties.GetPropVal(_program.ProgramNum, PayConnect.ProgramProperties.PayConnectPreventSavingNewCC, _clinicNum)))
        {
            textCardNumber.ReadOnly = true;
        }
    }

    private void radioTerminal_CheckedChanged(object sender, EventArgs e)
    {
        if (!radioTerminal.Checked)
        {
            return;
        }

        new[] {textCardNumber, textExpDate, textNameOnCard, textSecurityCode, textZipCode}.ForEach(x => x.ReadOnly = true);

        Clear();

        radioForce.Enabled = false;
        checkSaveToken.Checked = false;
        checkSaveToken.Enabled = false;

        textAmount.Focus();
    }

    private void textCardNumber_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (string.IsNullOrEmpty(textCardNumber.Text))
        {
            return;
        }

        if (textCardNumber.Text.StartsWith("%") && e.KeyChar == 13)
        {
            e.Handled = true;

            _hasSwipedCard = true;
        }

        if (!_hasSwipedCard)
        {
            return;
        }

        timerParseCardSwipe.Stop();
        timerParseCardSwipe.Start();
    }

    private void timerParseCardSwipe_Tick(object sender, EventArgs e)
    {
        timerParseCardSwipe.Stop();

        ParseSwipedCard(textCardNumber.Text);

        _hasSwipedCard = false;
    }

    private void ParseSwipedCard(string data)
    {
        Clear();

        try
        {
            _magstripCardParser = new MagstripCardParser(data);
        }
        catch (MagstripCardParseException)
        {
            ShowError("Could not read card, please try again.");
        }

        if (_magstripCardParser is null)
        {
            return;
        }

        textCardNumber.Text = _magstripCardParser.AccountNumber;
        textExpDate.Text = _magstripCardParser.ExpirationMonth.ToString().PadLeft(2, '0') + (_magstripCardParser.ExpirationYear % 100).ToString().PadLeft(2, '0');
        textNameOnCard.Text = _magstripCardParser.FirstName + " " + _magstripCardParser.LastName;

        GetNextControl(textNameOnCard, true)?.Focus();
    }

    private void Clear()
    {
        textCardNumber.Text = "";
        textExpDate.Text = "";
        textNameOnCard.Text = "";
        textSecurityCode.Text = "";
        textZipCode.Text = "";
    }

    private bool VerifyData()
    {
        _expYear = 0;
        _expMonth = 0;
        
        if (!Regex.IsMatch(textAmount.Text, "^[0-9]+$") && !Regex.IsMatch(textAmount.Text, "^[0-9]*\\.[0-9]+$"))
        {
            ShowError("Invalid amount.");
            return false;
        }

        if ((TransType == PayConnectService.transType.VOID || (TransType == PayConnectService.transType.RETURN && radioWebService.Checked)) && textRefNumber.Text == "")
        {
            ShowError("Ref Number required.");
            return false;
        }

        var paymentType = ProgramProperties.GetPropVal(_program.ProgramNum, "PaymentType", _clinicNum);
        if (Defs.GetDefsForCategory(DefCat.PaymentTypes, true).All(x => x.DefNum.ToString() != paymentType))
        {
            ShowError("The PayConnect payment type has not been set.");

            return false;
        }

        if (radioTerminal.Checked)
        {
            return true;
        }

        if (textCardNumber.Text.Trim().Length < 5)
        {
            ShowError("Invalid Card Number.");
            return false;
        }

        try
        {
            if (Regex.IsMatch(textExpDate.Text, @"^\d\d[/\- ]\d\d$"))
            {
                _expYear = SIn.Int("20" + textExpDate.Text.Substring(3, 2));
                _expMonth = SIn.Int(textExpDate.Text.Substring(0, 2));
            }
            else if (Regex.IsMatch(textExpDate.Text, @"^\d{4}$"))
            {
                _expYear = SIn.Int("20" + textExpDate.Text.Substring(2, 2));
                _expMonth = SIn.Int(textExpDate.Text.Substring(0, 2));
            }
            else
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

            if (!PayConnect.IsValidCardAndExp(textCardNumber.Text, _expYear, _expMonth, x => ODMessageBox.Show(x)))
            {
                ShowError("Card number or expiration date failed validation with PayConnect.");
                return false;
            }
        }
        else if (_creditCard.PayConnectToken == "" && Regex.IsMatch(textCardNumber.Text, @"X+[0-9]{4}"))
        {
            ShowError("There is no saved PayConnect token for this credit card.  The card number and expiration must be re-entered.");
            return false;
        }

        if (textNameOnCard.Text.Trim() == "" && _patient is not null)
        {
            ShowError("Name On Card required.");
            return false;
        }

        if (TransType == PayConnectService.transType.FORCE && textRefNumber.Text == "")
        {
            ShowError("Authorization Code required.");
            return false;
        }

        var password = Class1.TryDecrypt(ProgramProperties.GetPropVal(_program.ProgramNum, "Password", _clinicNum));
        if (ProgramProperties.GetPropVal(_program.ProgramNum, "Username", _clinicNum) != "" && password != "")
        {
            return true;
        }

        ShowError("The PayConnect username and/or password has not been set.");
        return false;
    }

    private PayConnectService.signatureResponse SendSignature(string refNumber)
    {
        if (!sigBoxWrapper.GetSigChanged() || string.IsNullOrEmpty(sigBoxWrapper.GetSignature("")))
        {
            return null;
        }

        var signatureRequest = new PayConnectService.signatureRequest
        {
            RefNumber = refNumber,
            SignatureType = PayConnectService.signatureType.JPEG
        };

        using (var image = sigBoxWrapper.GetSigImage())
        using (var memoryStream = new MemoryStream())
        {
            image.Save(memoryStream, ImageFormat.Jpeg);

            var bytes = memoryStream.ToArray();

            signatureRequest.SignatureData = Convert.ToBase64String(bytes);
        }

        return PayConnect.ProcessSignature(signatureRequest, _clinicNum, x => ODMessageBox.Show(x));
    }

    private bool ProcessPaymentWebService(int expYear, int expMonth)
    {
        var refNumber = "";
        if (TransType is PayConnectService.transType.VOID or PayConnectService.transType.RETURN)
        {
            refNumber = textRefNumber.Text;
        }

        string magData = null;
        if (_magstripCardParser != null)
        {
            magData = _magstripCardParser.Track2;
        }

        var cardNumber = textCardNumber.Text;

        if (_creditCard is not null && !string.IsNullOrEmpty(_creditCard.XChargeToken) && (StringTools.TruncateBeginning(cardNumber, 4) != StringTools.TruncateBeginning(_creditCard.CCNumberMasked, 4) || expYear != _creditCard.CCExpiration.Year || expMonth != _creditCard.CCExpiration.Month))
        {
            if (Confirm(
                    "The card number or expiration date entered does not match the X-Charge card on file. " +
                    "Do you wish to replace the X-Charge card with this one?"))
            {
                _creditCard.XChargeToken = "";
            }
            else
            {
                Cursor = Cursors.Default;
                return false;
            }
        }

        if (_creditCard != null && _creditCard.PayConnectToken != "")

        {
            if (_creditCard.PayConnectTokenExp.Date >= DateTime.Today.Date)
            {
                expYear = _creditCard.PayConnectTokenExp.Year;
                expMonth = _creditCard.PayConnectTokenExp.Month;
            }

            if (_creditCard.PayConnectTokenExp == DateTime.MinValue)
            {
                expYear = _creditCard.CCExpiration.Year;
                expMonth = _creditCard.CCExpiration.Month;
            }

            cardNumber = _creditCard.PayConnectToken;
        }
        else if (SIn.Bool(ProgramProperties.GetPropVal(_program.ProgramNum, PayConnect.ProgramProperties.PayConnectPreventSavingNewCC, _clinicNum)))
        {
            ShowError("Cannot add a new credit card.");
            return false;
        }

        var authCode = "";
        if (TransType == PayConnectService.transType.FORCE)
        {
            authCode = textRefNumber.Text;
        }

        CreditCardRequest = PayConnect.BuildSaleRequest(SIn.Decimal(textAmount.Text), cardNumber, expYear, expMonth, textNameOnCard.Text, textSecurityCode.Text, textZipCode.Text, magData, TransType, refNumber, checkSaveToken.Checked, authCode, checkForceDuplicate.Checked);

        _transResponse = PayConnect.ProcessCreditCard(CreditCardRequest, _clinicNum, x => ODMessageBox.Show(x));
        if (_transResponse is null || _transResponse.Status.code != 0)
        {
            return false;
        }

        if (_creditCard is {PayConnectTokenExp.Year: < 1880})
        {
            _creditCard.PayConnectTokenExp = _creditCard.CCExpiration;

            CreditCards.Update(_creditCard);
        }

        var signatureResponse = SendSignature(_transResponse.RefNumber);
        if (TransType is PayConnectService.transType.SALE or PayConnectService.transType.RETURN or PayConnectService.transType.VOID && _transResponse.Status.code == 0)
        {
            ReceiptStr = PayConnect.BuildReceiptString(CreditCardRequest, _transResponse, signatureResponse, _clinicNum);

            PayConnectL.PrintReceipt(ReceiptStr, _patient);
        }

        if (!PrefC.GetBool(PrefName.StoreCCnumbers) && !checkSaveToken.Checked)
        {
            return true;
        }

        if (_creditCard is null)
        {
            _creditCard = new CreditCard
            {
                IsNew = true,
                PatNum = _patient.PatNum
            };

            var creditCardsItemOrderCount = CreditCards.RefreshAll(_patient.PatNum);

            _creditCard.ItemOrder = creditCardsItemOrderCount.Count;
        }

        _creditCard.CCExpiration = new DateTime(expYear, expMonth, DateTime.DaysInMonth(expYear, expMonth));
        _creditCard.CCNumberMasked = PrefC.GetBool(PrefName.StoreCCnumbers)
            ? textCardNumber.Text
            : StringTools.TruncateBeginning(textCardNumber.Text, 4).PadLeft(textCardNumber.Text.Length, 'X');

        if (!_creditCard.IsNew && _transResponse.PaymentToken is not null)
        {
            var transactionTokenExpField = new DateTime(_transResponse.PaymentToken.Expiration.year, _transResponse.PaymentToken.Expiration.month, DateTime.DaysInMonth(_transResponse.PaymentToken.Expiration.year, _transResponse.PaymentToken.Expiration.month));

            if (_creditCard.PayConnectTokenExp != transactionTokenExpField)
            {
                _creditCard.PayConnectTokenExp = transactionTokenExpField;

                CreditCards.Update(_creditCard);
            }
        }

        _creditCard.CCSource = CreditCardSource.PayConnect;

        if (_creditCard.IsNew && checkSaveToken.Checked && _transResponse.PaymentToken != null)
        {
            _creditCard.Zip = textZipCode.Text;
            _creditCard.ClinicNum = _clinicNum;
            _creditCard.PayConnectToken = _transResponse.PaymentToken.TokenId;
            _creditCard.PayConnectTokenExp = new DateTime(_transResponse.PaymentToken.Expiration.year, _transResponse.PaymentToken.Expiration.month, DateTime.DaysInMonth(_transResponse.PaymentToken.Expiration.year, _transResponse.PaymentToken.Expiration.month));
            _creditCard.Procedures = PrefC.GetString(PrefName.DefaultCCProcs);

            CreditCards.Insert(_creditCard);

            SecurityLogs.MakeLogEntry(EnumPermType.CreditCardEdit, _patient.PatNum, "Credit Card Added");
        }
        else
        {
            if (_creditCard.CCSource == CreditCardSource.XServer)
            {
                _creditCard.CCSource = CreditCardSource.XServerPayConnect;
            }

            CreditCards.Update(_creditCard);
        }

        return true;
    }

    private bool ProcessPaymentTerminal()
    {
        PosRequest posRequest;

        try
        {
            if (radioSale.Checked)
            {
                posRequest = PosRequest.CreateSale(SIn.Decimal(textAmount.Text));
            }
            else if (radioAuthorization.Checked)
            {
                posRequest = PosRequest.CreateAuth(SIn.Decimal(textAmount.Text));
            }
            else if (radioVoid.Checked)
            {
                posRequest = PosRequest.CreateVoidByReference(textRefNumber.Text);
            }
            else if (radioReturn.Checked)
            {
                posRequest = textRefNumber.Text == ""
                    ? PosRequest.CreateRefund(SIn.Decimal(textAmount.Text))
                    : PosRequest.CreateRefund(SIn.Decimal(textAmount.Text), textRefNumber.Text);
            }
            else
            {
                ShowError("Please select a transaction type");
                return false;
            }

            posRequest.ForceDuplicate = checkForceDuplicate.Checked;
        }
        catch (Exception ex)
        {
            ShowError("Error creating request: " + ex.Message);

            return false;
        }

        var progressWin = new ProgressWin
        {
            ActionMain = () => _payConnectResponse = PayConnectTerminal.ToPayConnectResponse(DpsPos.ProcessCreditCard(posRequest)),
            ShowCancelButton = false,
            StartingMessage = "Processing payment on terminal",
            StopNotAllowedMessage = "Not allowed to stop. Please wait up to 2 minutes."
        };

        try
        {
            progressWin.ShowDialog();
        }
        catch
        {
            SecurityLogs.MakeLogEntry(EnumPermType.CreditCardTerminal, _patient.PatNum, "No response received.");

            ShowError(
                "A payment was initiated but no response was received. The payment may or may not have processed. " +
                "Verify payment with your Credit Card merchant.");

            return false;
        }

        if (progressWin.IsCancelled)
        {
            return false;
        }

        if (_payConnectResponse == null)
        {
            ShowError("Error processing card");
            return false;
        }

        if (_payConnectResponse.StatusCode != "0")
        {
            ShowError("Error message from Pay Connect:\r\n" + _payConnectResponse.Description);
            return false;
        }

        PayConnectService.signatureResponse signatureResponse = null;
        try
        {
            Cursor = Cursors.WaitCursor;

            signatureResponse = SendSignature(_payConnectResponse.RefNumber);

            Cursor = Cursors.Default;
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Default;

            ShowError("Card successfully charged. Error processing signature: " + ex.Message);
        }

        textCardNumber.Text = _payConnectResponse.CardNumber;
        textAmount.Text = _payConnectResponse.Amount.ToString("f");

        var wasSigned = signatureResponse?.Status is {code: 0};

        ReceiptStr = PayConnectTerminal.BuildReceiptString(_payConnectResponse, wasSigned, _clinicNum);

        PayConnectL.PrintReceipt(ReceiptStr, _patient);

        return true;
    }

    private void butSave_Click(object sender, EventArgs e)
    {
        Cursor = Cursors.WaitCursor;

        if (!VerifyData())
        {
            Cursor = Cursors.Default;
            return;
        }

        WasPaymentAttempted = true;

        var isSuccess = radioWebService.Checked ? ProcessPaymentWebService(_expYear, _expMonth) : ProcessPaymentTerminal();

        Cursor = Cursors.Default;

        if (isSuccess)
        {
            DialogResult = DialogResult.OK;
        }
        else if (!_isAddingCard)
        {
            DialogResult = DialogResult.Cancel;
        }
    }

    private void FormPayConnect_FormClosing(object sender, FormClosingEventArgs e)
    {
        sigBoxWrapper?.SetTabletState(0);
    }

    public string GetAmountCharged()
    {
        return TransType == PayConnectService.transType.RETURN
            ? SIn.Decimal("-" + textAmount.Text).ToString("F")
            : SIn.Decimal(textAmount.Text).ToString("F");
    }

    public string GetCardNumber()
    {
        return textCardNumber.Text;
    }

    public PayConnectResponse GetResponse()
    {
        return _transResponse != null ? PayConnectREST.ToPayConnectResponse(_transResponse, CreditCardRequest) : _payConnectResponse;
    }
}