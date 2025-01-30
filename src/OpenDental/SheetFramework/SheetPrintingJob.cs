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
    private MedLab _medLab;

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

    public void Print(Sheet sheet, int copies = 1, bool isRxControlled = false, Statement stmt = null, MedLab medLab = null, bool isPrintDocument = true, bool isPreviewMode = false)
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

        Print(sheet, _dataSet, copies, isRxControlled, stmt, medLab, isPrintDocument, isPreviewMode);
    }

    public void Print(Sheet sheet, DataSet dataSet, int copies = 1, bool isRxControlled = false, Statement stmt = null, MedLab medLab = null, bool isPrintDocument = true, bool isPreviewMode = false)
    {
        try
        {
            TryPrint(sheet, dataSet, copies, isRxControlled, stmt, medLab, isPreviewMode);
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
            SheetTypeEnum.RxMulti => PrintSituation.RxMulti,
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

    public void PrintMultiRx(List<RxPat> listRxs)
    {
        var sheetDef = SheetDefs.GetInternalOrCustom(SheetInternalType.RxMulti);
        var rxSheetCountList = GetSheetRxCount(sheetDef); //gets the number of rx available in the sheet
        if (sheetDef.Parameters.Count == 0)
        {
            //adds parameters if internal sheet
            sheetDef.Parameters.Add(new SheetParameter(true, "ListRxNums"));
            sheetDef.Parameters.Add(new SheetParameter(true, "ListRxSheet"));
        }

        var batchRxList = new List<Sheet>(); //list of sheets to be batch printed
        if (rxSheetCountList.Count == 0)
        {
            ODMessageBox.Show("MuitiRx sheet is invalid. Please visit the manual to see what output fields must be added to the MultiRx Sheet.");
            return;
        }

        //Sort RxPats into batches. rxSheetCount is most rx's we can print on one sheet.
        var batchSize = rxSheetCountList.Count;
        var batchIdx = 0;
        var batches = new List<List<RxPat>>();
        for (var i = 0; i < listRxs.Count; i++)
        {
            if (SheetPrinting.ValidateRxForSheet(listRxs[i]) != "")
            {
                MsgBox.Show("Sheets", "One or more of the selected prescriptions is missing information.\r\nPlease fix and try again.");
                return;
            }

            if (i > 0 && i % batchSize == 0)
            {
                batchIdx++;
            }

            if (i % batchSize == 0)
            {
                batches.Add([]);
            }

            batches[batchIdx].Add(listRxs[i]);
        }

        //Fill and add sheets to batchRxList to be printed
        foreach (var listBatch in batches)
        {
            var sheet = SheetUtil.CreateSheet(sheetDef, listRxs[0].PatNum);
            SheetParameter.SetParameter(sheet, "ListRxNums", listBatch);
            SheetParameter.SetParameter(sheet, "ListRxSheet", rxSheetCountList);
            SheetFiller.FillFields(sheet);
            batchRxList.Add(sheet);
        }

        PrintBatch(batchRxList);
    }

    public bool PrintRx(Sheet sheet, RxPat rx)
    {
        var validationErrors = SheetPrinting.ValidateRxForSheet(rx);
        if (validationErrors != "" && IsRemotePrintingJob)
        {
            return false;
        }

        if (validationErrors != "")
        {
            ODMessageBox.Show("Cannot print until missing info is fixed: " + validationErrors);
            return false;
        }

        Print(sheet, 1, rx.IsControlled);
        return true;
    }

    public void DrawSheetFirstPage(Graphics g, Sheet sheet)
    {
        pd_DrawFieldsHelper(sheet, g, null);

        SheetDrawingJob.DrawFooter(sheet, g, null, PagesPrinted, _yPosPrint, _medLab);
    }

    public void TryPrint(Sheet sheet, DataSet dataSet, int copies = 1, bool isRxControlled = false, Statement stmt = null, MedLab medLab = null, bool isPreviewMode = false)
    {
        _dataSet = dataSet;
        _stmt = stmt;
        _medLab = medLab;
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
            SheetTypeEnum.Rx => isRxControlled ? PrintSituation.RxControlled : PrintSituation.Rx,
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

        SheetUtil.CalculateHeights(sheet, _dataSet, _stmt, _isPrinting, PrintMargin.Top, PrintMargin.Bottom, _medLab);
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
            printSituation: sit,
            margins: new Margins(0, 0, 0, 0),
            printoutOrigin: PrintoutOrigin.AtMargin,
            printoutOrientation: sheet.IsLandscape ? PrintoutOrientation.Landscape : PrintoutOrientation.Portrait,
            totalPages: pageCount,
            isForcedPreview: isPreviewMode,
            auditPatNum: sheet.PatNum,
            paperSize: paperSize,
            isRemotePrint: IsRemotePrintingJob,
            printerNumOverride: PrinterNumOverride
        );

        _isPrinting = false;

        GC.Collect();
    }

    private static List<int> GetSheetRxCount(SheetDef sheetDef)
    {
        var rxFieldList = new List<int>();
        var hasProvNameFL = false;
        var hasProvNameFL2 = false;
        var hasProvNameFL3 = false;
        var hasProvNameFL4 = false;
        var hasProvNameFL5 = false;
        var hasProvNameFL6 = false;
        var hasPatNameFL = false;
        var hasPatNameFL2 = false;
        var hasPatNameFL3 = false;
        var hasPatNameFL4 = false;
        var hasPatNameFL5 = false;
        var hasPatNameFL6 = false;
        var hasPatBirthdate = false;
        var hasPatBirthdate2 = false;
        var hasPatBirthdate3 = false;
        var hasPatBirthdate4 = false;
        var hasPatBirthdate5 = false;
        var hasPatBirthdate6 = false;
        var hasDrug = false;
        var hasDrug2 = false;
        var hasDrug3 = false;
        var hasDrug4 = false;
        var hasDrug5 = false;
        var hasDrug6 = false;

        foreach (var field in sheetDef.SheetFieldDefs)
        {
            switch (field.FieldName)
            {
                case "prov.nameFL":
                    hasProvNameFL = true;
                    break;
                case "prov.nameFL2":
                    hasProvNameFL2 = true;
                    break;
                case "prov.nameFL3":
                    hasProvNameFL3 = true;
                    break;
                case "prov.nameFL4":
                    hasProvNameFL4 = true;
                    break;
                case "prov.nameFL5":
                    hasProvNameFL5 = true;
                    break;
                case "prov.nameFL6":
                    hasProvNameFL6 = true;
                    break;
                case "pat.nameFL":
                    hasPatNameFL = true;
                    break;
                case "pat.nameFL2":
                    hasPatNameFL2 = true;
                    break;
                case "pat.nameFL3":
                    hasPatNameFL3 = true;
                    break;
                case "pat.nameFL4":
                    hasPatNameFL4 = true;
                    break;
                case "pat.nameFL5":
                    hasPatNameFL5 = true;
                    break;
                case "pat.nameFL6":
                    hasPatNameFL6 = true;
                    break;
                case "pat.Birthdate":
                    hasPatBirthdate = true;
                    break;
                case "pat.Birthdate2":
                    hasPatBirthdate2 = true;
                    break;
                case "pat.Birthdate3":
                    hasPatBirthdate3 = true;
                    break;
                case "pat.Birthdate4":
                    hasPatBirthdate4 = true;
                    break;
                case "pat.Birthdate5":
                    hasPatBirthdate5 = true;
                    break;
                case "pat.Birthdate6":
                    hasPatBirthdate6 = true;
                    break;
                case "Drug":
                    hasDrug = true;
                    break;
                case "Drug2":
                    hasDrug2 = true;
                    break;
                case "Drug3":
                    hasDrug3 = true;
                    break;
                case "Drug4":
                    hasDrug4 = true;
                    break;
                case "Drug5":
                    hasDrug5 = true;
                    break;
                case "Drug6":
                    hasDrug6 = true;
                    break;
            }
        }

        if (hasProvNameFL && hasPatNameFL && hasPatBirthdate && hasDrug)
        {
            rxFieldList.Add(1);
        }

        if (hasProvNameFL2 && hasPatNameFL2 && hasPatBirthdate2 && hasDrug2)
        {
            rxFieldList.Add(2);
        }

        if (hasProvNameFL3 && hasPatNameFL3 && hasPatBirthdate3 && hasDrug3)
        {
            rxFieldList.Add(3);
        }

        if (hasProvNameFL4 && hasPatNameFL4 && hasPatBirthdate4 && hasDrug4)
        {
            rxFieldList.Add(4);
        }

        if (hasProvNameFL5 && hasPatNameFL5 && hasPatBirthdate5 && hasDrug5)
        {
            rxFieldList.Add(5);
        }

        if (hasProvNameFL6 && hasPatNameFL6 && hasPatBirthdate6 && hasDrug6)
        {
            rxFieldList.Add(6);
        }

        return rxFieldList;
    }

    private void pd_DrawFieldsHelper(Sheet sheet, Graphics g, XGraphics gx, Sheet parentSheet = null)
    {
        var sheetDrawingJob = new SheetDrawingJob(_dataSet, _medLab, PagesPrinted, _yPosPrint, _yPosPrevious, _idxPreClaimPaidPrinted);

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
                    sheetDrawingJob.DrawFieldGrid(field, sheet, g, gx, _dataSet, _stmt, _medLab, true);
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

                case SheetFieldType.ScreenChart:
                    sheetDrawingJob.DrawFieldScreenChart(field, sheet, g, gx);
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
        SheetDrawingJob.DrawFooter(sheet, g, null, PagesPrinted, _yPosPrint, _medLab);
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