using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDentBusiness;
using PdfSharp.Drawing;

namespace OpenDental;

class SheetPrintingJob
{
    private DataSet _dataSet;
    private int _idxPreClaimPaidPrinted;
    private bool _isPrinting;
    private List<Sheet> _listSheets;

#if DEBUG
    private readonly bool _printCalibration = false;
#endif

    private static readonly Margins PrintMargin = new(0, 0, 40, 60);

    private int _sheetsPrinted;
    private Statement _stmt;
    private int _yPosPrevious;
    private int _yPosPrint;
    public bool IsRemotePrintingJob = false;
    public long PrinterNumOverride = 0;

    public int PagesPrinted { get; private set; }

    public void Print(Sheet sheet, int copies = 1, Statement stmt = null, bool isPreviewMode = false)
    {
        if (sheet.SheetType == SheetTypeEnum.Statement && stmt != null)
        {
            if (stmt.SuperFamily != 0 || stmt.LimitedCustomFamily != EnumLimitedCustomFamily.None)
            {
                _dataSet = AccountModules.GetSuperFamAccount(stmt, doShowHiddenPaySplits: stmt.IsReceipt);
            }
            else
            {
                _dataSet = AccountModules.GetAccount(stmt.PatNum, stmt, doShowHiddenPaySplits: stmt.IsReceipt);
            }
        }

        Print(sheet, _dataSet, copies, stmt, isPreviewMode);
    }

    public void Print(Sheet sheet, DataSet dataSet, int copies = 1, Statement stmt = null, bool isPreviewMode = false)
    {
        try
        {
            TryPrint(sheet, dataSet, copies, stmt, isPreviewMode);
        }
        catch (InvalidPrinterException)
        {
        }
    }

    public void PrintBatch(List<Sheet> sheetBatch)
    {
        _listSheets = sheetBatch;
        _sheetsPrinted = 0;

        PagesPrinted = 0;

        _yPosPrint = 0;

        var sit = sheetBatch[0].SheetType switch
        {
            SheetTypeEnum.LabelPatient or SheetTypeEnum.LabelCarrier or SheetTypeEnum.LabelReferral => PrintSituation.LabelSingle,
            SheetTypeEnum.ReferralSlip => PrintSituation.Default,
            _ => PrintSituation.Default
        };

        foreach (var s in _listSheets)
        {
            SheetUtil.CalculateHeights(s, null, null, _isPrinting, PrintMargin.Top, PrintMargin.Bottom);
        }

        Margins margins;
        const int pageCount = 0;
        {
            foreach (var s in _listSheets)
            {
                s.SheetFields.Sort(SheetFields.SortDrawingOrderLayers);
            }

            margins = new Margins(0, 0, 0, 0);
        }

        PrinterL.TryPrintOrDebugClassicPreview(pd_PrintPage,
            "Batch of " + sheetBatch[0].Description + " printed",
            margins,
            pageCount,
            sit,
            PrintoutOrigin.AtMargin,
            sheetBatch[0].IsLandscape ? PrintoutOrientation.Landscape : PrintoutOrientation.Portrait
        );
    }

    public void DrawSheetFirstPage(Graphics g, Sheet sheet)
    {
        pd_DrawFieldsHelper(sheet, g, null);

        SheetDrawingJob.DrawFooter(sheet, g, null, PagesPrinted, _yPosPrint);
    }

    public void TryPrint(Sheet sheet, DataSet dataSet, int copies = 1, Statement stmt = null, bool isPreviewMode = false)
    {
        _dataSet = dataSet;
        _stmt = stmt;
        _isPrinting = true;
        _sheetsPrinted = 0;
        _yPosPrint = 0;

        PaperSize paperSize = null;

        if (sheet.SheetType is SheetTypeEnum.LabelPatient or SheetTypeEnum.LabelCarrier or SheetTypeEnum.LabelAppointment or SheetTypeEnum.LabelReferral)
        {
            if (sheet.Width > 0 && sheet.Height > 0)
            {
                paperSize = new PaperSize("Default", sheet.Width, sheet.Height);
            }
        }

        var sit = sheet.SheetType switch
        {
            SheetTypeEnum.LabelPatient or SheetTypeEnum.LabelCarrier or SheetTypeEnum.LabelReferral or SheetTypeEnum.LabelAppointment => PrintSituation.LabelSingle,
            SheetTypeEnum.ReferralSlip => PrintSituation.Default,
            SheetTypeEnum.Statement => PrintSituation.Statement,
            SheetTypeEnum.TreatmentPlan => PrintSituation.TPPerio,
            _ => PrintSituation.Default
        };

        Sheets.SetPageMargin(sheet, PrintMargin);

        foreach (var field in sheet.SheetFields)
        {
            if (field.FieldType is not (SheetFieldType.SigBox or SheetFieldType.SigBoxPractice))
            {
                continue;
            }

            field.SigKey = Sheets.GetSignatureKey(sheet);
        }

        if (stmt != null)
        {
            SheetUtil.SetDefaultValueForComboBoxes(sheet);
        }

        SheetUtil.CalculateHeights(sheet, _dataSet, _stmt, _isPrinting, PrintMargin.Top, PrintMargin.Bottom);
        _listSheets = [];
        for (var i = 0; i < copies; i++)
        {
            _listSheets.Add(sheet.Copy());
        }

        var pageCount = 0;
        if (isPreviewMode)
        {
            foreach (var s in _listSheets)
            {
                pageCount += Sheets.CalculatePageCount(s, PrintMargin);
            }
        }

        ODprintout.InvalidMinDefaultPageWidth = 0;
        ODprintout.InvalidMinDefaultPageHeight = -1;

        PrinterL.TryPrintOrDebugClassicPreview(pd_PrintPage,
            sheet.Description + " sheet from " + sheet.DateTimeSheet.ToShortDateString() + " printed",
            margins: new Margins(0, 0, 0, 0),
            totalPages: pageCount,
            printSituation: sit,
            printoutOrigin: PrintoutOrigin.AtMargin,
            printoutOrientation: sheet.IsLandscape ? PrintoutOrientation.Landscape : PrintoutOrientation.Portrait,
            isForcedPreview: isPreviewMode,
            auditPatNum: sheet.PatNum, paperSize: paperSize, isRemotePrint: IsRemotePrintingJob, printerNumOverride: PrinterNumOverride);

        _isPrinting = false;

        GC.Collect();
    }

    private void pd_DrawFieldsHelper(Sheet sheet, Graphics g, XGraphics gx, Sheet parentSheet = null)
    {
        var sheetDrawingJob = new SheetDrawingJob(_dataSet, PagesPrinted, _yPosPrint, _yPosPrevious, _idxPreClaimPaidPrinted);

        foreach (var field in sheet.SheetFields)
        {
            if (parentSheet != null && !SheetDrawingJob.FieldOnCurPageHelper(field, parentSheet, PrintMargin, _yPosPrint, PagesPrinted))
            {
                continue;
            }

            if (parentSheet == null && !SheetDrawingJob.FieldOnCurPageHelper(field, sheet, PrintMargin, _yPosPrint, PagesPrinted))
            {
                continue;
            }

            switch (field.FieldType)
            {
                case SheetFieldType.Image:
                case SheetFieldType.PatImage:
                    sheetDrawingJob.DrawFieldImage(field, g, gx);
                    break;

                case SheetFieldType.Drawing:
                    sheetDrawingJob.DrawFieldDrawing(field, g, gx);
                    break;

                case SheetFieldType.Rectangle:
                    SheetDrawingJob.DrawFieldRectangle(field, g, gx, _yPosPrint);
                    break;

                case SheetFieldType.Line:
                    sheetDrawingJob.DrawFieldLine(field, g, gx);
                    break;

                case SheetFieldType.Special:
                    OpenDentBusiness.SheetPrinting.DrawFieldSpecial(sheet, field, g, gx, _yPosPrint);
                    break;

                case SheetFieldType.Grid:
                    sheetDrawingJob.DrawFieldGrid(field, sheet, g, gx, _dataSet, _stmt, true);
                    _yPosPrevious = sheetDrawingJob.YPosPrevious;
                    _idxPreClaimPaidPrinted = sheetDrawingJob.IdxPreClaimPaidPrinted;
                    break;

                case SheetFieldType.InputField:
                case SheetFieldType.OutputText:
                case SheetFieldType.StaticText:
                    SheetDrawingJob.DrawFieldText(field, sheet, g, gx, _yPosPrint);
                    break;

                case SheetFieldType.CheckBox:
                    sheetDrawingJob.DrawFieldCheckBox(field, g, gx);
                    break;

                case SheetFieldType.ComboBox:
                    sheetDrawingJob.DrawFieldComboBox(field, sheet, g, gx);
                    break;

                case SheetFieldType.SigBox:
                case SheetFieldType.SigBoxPractice:
                    sheetDrawingJob.DrawFieldSigBox(field, sheet, g, gx);
                    break;
            }
        }
    }

    private void pd_PrintPage(object sender, PrintPageEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic; //Necessary for very large images that need to be scaled down.
        var sheet = _listSheets[_sheetsPrinted];
        Sheets.SetPageMargin(sheet, PrintMargin);
        if (PagesPrinted == 0 && sheet.SheetType == SheetTypeEnum.ERA)
        {
            _idxPreClaimPaidPrinted = 0;
            _yPosPrevious = 0;
        }

        try
        {
            pd_DrawFieldsHelper(sheet, g, null);
        }
        catch (OutOfMemoryException ex)
        {
            //Cancel the print job because there is a static image on this sheet which is to big for the printer to handle.
            ODMessageBox.Show(ex.Message); //Custom message that is already translated.
            e.Cancel = true;
            return;
        }

        SheetDrawingJob.DrawHeader(sheet, g, null, PagesPrinted, _yPosPrint);
        SheetDrawingJob.DrawFooter(sheet, g, null, PagesPrinted, _yPosPrint);
#if DEBUG
        if (_printCalibration)
        {
            SheetDrawingJob.DrawCalibration(sheet, g, e, null, null);
        }
#endif
        g.Dispose();

        _yPosPrint += sheet.HeightPage - PrintMargin.Bottom - PrintMargin.Top;
        PagesPrinted++;
        if (PagesPrinted < Sheets.CalculatePageCount(sheet, PrintMargin))
        {
            e.HasMorePages = true;
        }
        else
        {
            _yPosPrint = 0;

            PagesPrinted = 0;

            _sheetsPrinted++;

            if (_sheetsPrinted < _listSheets.Count)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;

                _sheetsPrinted = 0;
            }
        }
    }
}