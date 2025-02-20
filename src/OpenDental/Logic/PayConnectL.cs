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

internal static class PayConnectL
{
    public static bool VoidOrRefundPayConnectPortalTransaction(PayConnectResponseWeb payConnectResponseWeb, Payment payment, PayConnectService.transType transType, string strRefNum, decimal amount)
    {
        if (transType is not (PayConnectService.transType.RETURN or PayConnectService.transType.VOID))
        {
            MsgBox.Show("PayConnectL", "Invalid transaction type. Please contact support for assistance.");
            return false;
        }

        var paySplits = PaySplits.GetForPayment(payment.PayNum);

        var creditCard = CreditCards.GetOneWithPayConenctToken(payConnectResponseWeb.PaymentToken);
        if (creditCard is null)
        {
            MsgBox.Show("PayConnectL", "Patient was not logged in for this payment, you must go through your payment merchant's portal to process this request.");
            return false;
        }

        var creditCardRequest = PayConnect.BuildSaleRequest(amount, creditCard.PayConnectToken, creditCard.PayConnectTokenExp.Year, creditCard.PayConnectTokenExp.Month, "", "", "", "", transType, strRefNum, false);
        var transResponse = PayConnect.ProcessCreditCard(creditCardRequest, payment.ClinicNum, MsgBox.Show);
        var payConnectResponse = PayConnectREST.ToPayConnectResponse(transResponse, creditCardRequest);
        var receipt = PayConnect.BuildReceiptString(creditCardRequest, transResponse, null, payment.ClinicNum);

        if (payConnectResponse is not {StatusCode: "0"})
        {
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

        var payNote =
            "Transaction Type: " + Enum.GetName(typeof(PayConnectService.transType), transType) + Environment.NewLine +
            "Status: " + payConnectResponse.Description + Environment.NewLine +
            "Amount: " + amount.ToString("C") + Environment.NewLine +
            "Auth Code: " + payConnectResponse.AuthCode + Environment.NewLine +
            "Ref Number: " + payConnectResponse.RefNumber;

        var paymentClone = Payments.InsertVoidPayment(payment, paySplits, receipt, payNote, payConnectResponseWeb.CCSource, payAmt: (double) amount);

        paymentClone.PayDate = DateTime.Now;

        PayConnectResponseWebs.Insert(new PayConnectResponseWeb
        {
            PatNum = payment.PatNum,
            PayNum = paymentClone.PayNum,
            CCSource = payConnectResponseWeb.CCSource,
            Amount = paymentClone.PayAmt,
            PayNote = paymentClone.PayNote + Environment.NewLine + "From within Open Dental Proper.",
            ProcessingStatus = PayConnectWebStatus.Completed,
            DateTimeEntry = DateTime.Now,
            DateTimeCompleted = DateTime.Now,
            IsTokenSaved = false,
            RefNumber = transResponse.RefNumber,
            TransType = transType,
            PaymentToken = payConnectResponseWeb.PaymentToken
        });

        SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, paymentClone.PatNum, Patients.GetLim(paymentClone.PatNum).GetNameLF() + ", " + paymentClone.PayAmt.ToString("c"));

        return true;
    }

    public static void PrintReceipt(string receipt, Patient patient)
    {
        var receiptLines = receipt.Split(Environment.NewLine, StringSplitOptions.None);

        var document = new Document
        {
            DefaultPageSetup =
            {
                PageWidth = Unit.FromInch(3.0),
                PageHeight = Unit.FromInch(0.181 * receiptLines.Length + 0.56),
                TopMargin = Unit.FromInch(0.25),
                LeftMargin = Unit.FromInch(0.25),
                RightMargin = Unit.FromInch(0.25)
            }
        };

        var font = MigraDocHelper.CreateFont(8, false);

        font.Name = FontFamily.GenericMonospace.Name;

        var section = document.AddSection();
        var paragraph = section.AddParagraph();

        paragraph.Format = new ParagraphFormat
        {
            Alignment = ParagraphAlignment.Left,
            Font = font
        };

        paragraph.AddFormattedText(receipt, font);

        var migraDocPrintDocument = new MigraDocPrintDocument();
        var documentRenderer = new DocumentRenderer(document);

        documentRenderer.PrepareDocument();

        migraDocPrintDocument.Renderer = documentRenderer;

        try
        {
            var printout = PrinterL.CreateODprintout(
                printSituation: PrintSituation.Receipt,
                auditPatNum: patient.PatNum,
                auditDescription: "PayConnect receipt printed"
            );

            if (!PrinterL.TrySetPrinter(printout))
            {
                return;
            }

            migraDocPrintDocument.PrinterSettings = printout.PrintDoc.PrinterSettings;
            migraDocPrintDocument.Print();
        }
        catch (Exception ex)
        {
            ODMessageBox.Show("Printer not available.\r\nOriginal error: " + ex.Message);
        }
    }
}