using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormEtransEdit : FormODBase
{
    public Etrans EtransCur;

    private Etrans _etransAck;
    private int _linesPrinted;
    private string _messageText;

    public FormEtransEdit()
    {
        InitializeComponent();
    }

    private void FormEtransEdit_Load(object sender, EventArgs e)
    {
        _messageText = EtransMessageTexts.GetMessageText(EtransCur.EtransMessageTextNum);

        textMessageText.Text = _messageText;
        textDateTimeTrans.Text = EtransCur.DateTimeTrans.ToString(CultureInfo.InvariantCulture);
        textClaimNum.Text = EtransCur.ClaimNum.ToString();
        textBatchNumber.Text = EtransCur.BatchNumber.ToString();
        textTransSetNum.Text = EtransCur.TransSetNum.ToString();
        textAckCode.Text = EtransCur.AckCode;
        textNote.Text = EtransCur.Note;

        if (EtransCur.Etype == EtransType.ClaimSent)
        {
            if (X12object.IsX12(_messageText))
            {
                var x12Object = new X12object(_messageText);
                if (x12Object.IsFormat4010())
                {
                    var x837_4010 = new X837_4010(_messageText);
                    
                    checkAttachments.Checked = x837_4010.AttachmentsWereSent(EtransCur.ClaimNum); //This function does not currently work, so the corresponding checkbox is hidden on the form as well.
                }
                else if (x12Object.IsFormat5010())
                {
                    var x837_5010 = new X837_5010(_messageText);
                    
                    checkAttachments.Checked = x837_5010.AttachmentsWereSent(EtransCur.ClaimNum); //This function does not currently work, so the corresponding checkbox is hidden on the form as well.
                }
            }
        }

        if (EtransCur.AckEtransNum > 0)
        {
            _etransAck = Etranss.GetEtrans(EtransCur.AckEtransNum);
            if (_etransAck != null)
            {
                textAckMessage.Text = EtransMessageTexts.GetMessageText(_etransAck.EtransMessageTextNum);
                textAckDateTime.Text = _etransAck.DateTimeTrans.ToString();
                textAckNote.Text = _etransAck.Note;
            }
        }
        else
        {
            _etransAck = null;
            
            groupAck.Visible = false;
        }

        if (!CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            butPrintAck.Visible = false;
        }
    }

    private void butPrint_Click(object sender, EventArgs e)
    {
        _linesPrinted = 0;

        var isPrinted = PrinterL.TryPrintOrDebugRpPreview(pd2_PrintPage,
            "Etrans message text from " + EtransCur.DateTimeTrans.ToShortDateString() + " printed",
            auditPatNum: EtransCur.PatNum,
            margins: new Margins(75, 75, 50, 100));

        if (!isPrinted)
        {
            return;
        }

        EtransCur.Note = "Printed" + textNote.Text;

        Etranss.Update(EtransCur);

        DialogResult = DialogResult.OK;
    }

    private void pd2_PrintPage(object sender, PrintPageEventArgs e)
    {
        var rectangleBounds = e.MarginBounds;
        var g = e.Graphics;
        string text;
        float yPos = rectangleBounds.Top;
        var solidBrush = new SolidBrush(Color.Black);
        using var font = new Font(FontFamily.GenericMonospace, 9);
        float heightTxt;
        RectangleF rectangleF;
        while (yPos < rectangleBounds.Bottom && _linesPrinted < textMessageText.Lines.Length)
        {
            text = textMessageText.Lines[_linesPrinted];
            heightTxt = g.MeasureString(text, font, rectangleBounds.Width).Height;
            rectangleF = new RectangleF(rectangleBounds.X, yPos, rectangleBounds.Width, heightTxt);
            g.DrawString(text, font, solidBrush, rectangleF);
            yPos += rectangleF.Height;
            _linesPrinted++;
            if (textMessageText.Lines[_linesPrinted - 1].EndsWith("\f"))
            {
                break;
            }
        }

        e.HasMorePages = _linesPrinted < textMessageText.Lines.Length;

        g.Dispose();
    }

    private void butPrintAck_Click(object sender, EventArgs e)
    {
        try
        {
            new FormCCDPrint(_etransAck, textAckMessage.Text, isPAutoPrint: false);
        }
        catch (Exception ex)
        {
            using var msgBoxCopyPaste = new MsgBoxCopyPaste("Failed to preview acknowledgment.\r\n" + ex.Message);

            msgBoxCopyPaste.ShowDialog();
        }
    }

    private void butSave_Click(object sender, EventArgs e)
    {
        EtransCur.Note = textNote.Text;

        Etranss.Update(EtransCur);

        if (_etransAck is not null)
        {
            _etransAck.Note = textAckNote.Text;

            Etranss.Update(_etransAck);
        }

        DialogResult = DialogResult.OK;
    }
}