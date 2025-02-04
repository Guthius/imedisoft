using System;
using System.Drawing;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.Rendering.Printing;
using OpenDental.Logic;
using OpenDentBusiness;
using Document = MigraDoc.DocumentObjectModel.Document;
using PayConnectService = OpenDentBusiness.PayConnectService;


namespace OpenDental;

class PayConnectL
{
    /// <summary>Only used to void or refund transactions from PayConnectPortal. Creates new cloned payment and paysplits for the refund or void.
    /// Returns true if the transaction was successful, otherwise false.</summary
    public static bool VoidOrRefundPayConnectPortalTransaction(PayConnectResponseWeb payConnectResponseWeb, Payment payment, PayConnectService.transType transType, string strRefNum, decimal amount)
    {
        if (!transType.In(PayConnectService.transType.RETURN, PayConnectService.transType.VOID))
        {
            MsgBox.Show("PayConnectL", "Invalid transaction type. Please contact support for assistance.");
            return false;
        }

        var listPaySplits = PaySplits.GetForPayment(payment.PayNum);
        var creditCardRequest = new PayConnectService.creditCardRequest();
        PayConnectResponse payConnectResponse = null;
        var receiptStr = "";
        var creditCard = CreditCards.GetOneWithPayConenctToken(payConnectResponseWeb.PaymentToken);
        if (creditCard == null)
        {
            MsgBox.Show("PayConnectL", "Patient was not logged in for this payment, you must go through your payment merchant's portal to process this request.");
            return false;
        }

        creditCardRequest = PayConnect.BuildSaleRequest(amount, creditCard.PayConnectToken, creditCard.PayConnectTokenExp.Year,
            creditCard.PayConnectTokenExp.Month, "", "", "", "", transType, strRefNum, false);
        var transResponse = PayConnect.ProcessCreditCard(creditCardRequest, payment.ClinicNum, x => MsgBox.Show(x));
        payConnectResponse = PayConnectREST.ToPayConnectResponse(transResponse, creditCardRequest);
        receiptStr = PayConnect.BuildReceiptString(creditCardRequest, transResponse, null, payment.ClinicNum);
        if (payConnectResponse == null || payConnectResponse.StatusCode != "0")
        {
            //error in transaction
            if (payConnectResponse == null)
            {
                MsgBox.Show("PayConnectL", "An unexpected error occurred when attempting to process this transaction. Please try again.");
            }
            else
            {
                MsgBox.Show(payConnectResponse.Description + ". Error Code: " + payConnectResponse.StatusCode);
            }

            return false;
        }

        //Record a new payment for the voided transaction
        var payNote = Lan.g("PayConnectL", "Transaction Type") + ": " + Enum.GetName(typeof(PayConnectService.transType), transType)
                      + Environment.NewLine + Lan.g("PayConnectL", "Status") + ": " + payConnectResponse.Description + Environment.NewLine
                      + Lan.g("PayConnectL", "Amount") + ": " + amount.ToString("C") + Environment.NewLine
                      + Lan.g("PayConnectL", "Auth Code") + ": " + payConnectResponse.AuthCode + Environment.NewLine
                      + Lan.g("PayConnectL", "Ref Number") + ": " + payConnectResponse.RefNumber;
        var paymentClone = Payments.InsertVoidPayment(payment, listPaySplits, receiptStr, payNote, payConnectResponseWeb.CCSource, payAmt: (double) amount);
        paymentClone.PayDate = DateTime.Now;
        var payConnectResponseWebNew = new PayConnectResponseWeb();
        payConnectResponseWebNew.PatNum = payment.PatNum;
        payConnectResponseWebNew.PayNum = paymentClone.PayNum;
        payConnectResponseWebNew.CCSource = payConnectResponseWeb.CCSource;
        payConnectResponseWebNew.Amount = paymentClone.PayAmt;
        payConnectResponseWebNew.PayNote = Lan.g("PayConnectL", paymentClone.PayNote + Environment.NewLine + "From within Open Dental Proper.");
        payConnectResponseWebNew.ProcessingStatus = PayConnectWebStatus.Completed;
        payConnectResponseWebNew.DateTimeEntry = DateTime.Now;
        payConnectResponseWebNew.DateTimeCompleted = DateTime.Now;
        payConnectResponseWebNew.IsTokenSaved = false;
        payConnectResponseWebNew.RefNumber = transResponse.RefNumber;
        payConnectResponseWebNew.TransType = transType;
        payConnectResponseWebNew.PaymentToken = payConnectResponseWeb.PaymentToken;
        PayConnectResponseWebs.Insert(payConnectResponseWebNew);
        SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, paymentClone.PatNum,
            Patients.GetLim(paymentClone.PatNum).GetNameLF() + ", " + paymentClone.PayAmt.ToString("c"));
        return true;
    }

    public static void PrintReceipt(string receiptStr, Patient patient)
    {
        var stringArrayReceiptLines = receiptStr.Split([Environment.NewLine], StringSplitOptions.None);
        var doc = new Document();
        doc.DefaultPageSetup.PageWidth = Unit.FromInch(3.0);
        doc.DefaultPageSetup.PageHeight = Unit.FromInch(0.181 * stringArrayReceiptLines.Length + 0.56); //enough to print receipt text plus 9/16 inch (0.56) extra space at bottom.
        doc.DefaultPageSetup.TopMargin = Unit.FromInch(0.25);
        doc.DefaultPageSetup.LeftMargin = Unit.FromInch(0.25);
        doc.DefaultPageSetup.RightMargin = Unit.FromInch(0.25);
        var bodyFontx = MigraDocHelper.CreateFont(8, false);
        bodyFontx.Name = FontFamily.GenericMonospace.Name;
        var section = doc.AddSection();
        var paragraph = section.AddParagraph();
        var paragraphFormat = new ParagraphFormat();
        paragraphFormat.Alignment = ParagraphAlignment.Left;
        paragraphFormat.Font = bodyFontx;
        paragraph.Format = paragraphFormat;
        paragraph.AddFormattedText(receiptStr, bodyFontx);
        var migraDocPrintDocument = new MigraDocPrintDocument();
        var documentRenderer = new DocumentRenderer(doc);
        documentRenderer.PrepareDocument();
        migraDocPrintDocument.Renderer = documentRenderer;
        try
        {
            var printout = PrinterL.CreateODprintout(
                printSituation: PrintSituation.Receipt,
                auditPatNum: patient.PatNum,
                auditDescription: Lans.g("PayConnectL", "PayConnect receipt printed")
            );
            if (PrinterL.TrySetPrinter(printout))
            {
                migraDocPrintDocument.PrinterSettings = printout.PrintDoc.PrinterSettings;
                migraDocPrintDocument.Print();
            }
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(Lan.g("PayConnectL", "Printer not available.") + "\r\n" + Lan.g("PayConnectL", "Original error") + ": " + ex.Message);
        }
    }
}