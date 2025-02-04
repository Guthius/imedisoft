using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness.Properties;
using OpenDentBusiness.SheetFramework;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace OpenDentBusiness;

public class SheetDrawingJob
{
    private static readonly Margins PrintMargin = new(0, 0, 40, 60);

    private readonly DataSet _dataSet;
    private bool _isPrinting;
    private int _pagesPrinted;
    private Statement _stmt;
    private int _yPosPrint;

    public SheetDrawingJob()
    {
    }

    public SheetDrawingJob(DataSet dataSet, int pagesPrinted, int yPosPrint, int yPosPrev, int idxPrinted)
    {
        _dataSet = dataSet;
        _pagesPrinted = pagesPrinted;
        _yPosPrint = yPosPrint;
        YPosPrevious = yPosPrev;
        IdxPreClaimPaidPrinted = idxPrinted;
    }

    public int YPosPrevious { get; private set; }
    public int IdxPreClaimPaidPrinted { get; private set; }

    public PdfDocument CreatePdf(Sheet sheet, Statement stmt = null, DataSet dataSet = null, Patient pat = null, Patient patGuar = null)
    {
        if (dataSet == null && stmt != null && sheet.SheetType == SheetTypeEnum.Statement)
        {
            //This should never get hit.  This line of code is here just in case I forgot to update a random spot in our code.
            //Worst case scenario we will end up calling the database a few extra times for the same data set.
            //It use to call this method many, many times so anything is an improvement at this point. --DevinF
            if (stmt.SuperFamily != 0 || stmt.LimitedCustomFamily != EnumLimitedCustomFamily.None)
            {
                dataSet = AccountModules.GetSuperFamAccount(stmt, doShowHiddenPaySplits: stmt.IsReceipt);
            }
            else
            {
                dataSet = AccountModules.GetAccount(stmt.PatNum, stmt, doShowHiddenPaySplits: stmt.IsReceipt);
            }
        }

        Sheets.SetPageMargin(sheet, PrintMargin);
        
        _stmt = stmt;
        _isPrinting = true;
        _yPosPrint = 0;
        var pdfDocument = new PdfDocument();
        foreach (var field in sheet.SheetFields)
        {
            //validate all signatures before modifying any of the text fields.
            if (!field.FieldType.In(SheetFieldType.SigBox, SheetFieldType.SigBoxPractice))
            {
                continue;
            }

            field.SigKey = Sheets.GetSignatureKey(sheet);
        }

        if (stmt != null)
        {
            SheetUtil.SetDefaultValueForComboBoxes(sheet);
        }

        SheetUtil.CalculateHeights(sheet, dataSet, _stmt, _isPrinting, PrintMargin.Top, PrintMargin.Bottom, pat, patGuar);
        var pageCount = Sheets.CalculatePageCount(sheet, PrintMargin);
        for (var i = 0; i < pageCount; i++)
        {
            _pagesPrinted = i;

            var pdfPage = pdfDocument.AddPage();

            _yPosPrint = CreatePdfPage(sheet, pdfPage, dataSet, pat: pat, patGuar: patGuar, _yPosPrint);
        }

        _isPrinting = false;

        return pdfDocument;
    }

    public static void SavePdfToFile(PdfDocument pdf, string fullPathAndFilename)
    {
        pdf.Save(fullPathAndFilename);
    }

    public int CreatePdfPage(Sheet sheet, PdfPage page, DataSet dataSet, Patient pat, Patient patGuar, int yPos, int pagesPrinted = -1)
    {
        _yPosPrint = yPos;
        if (pagesPrinted > -1)
        {
            _pagesPrinted = pagesPrinted;
        }

        page.Width = p(sheet.Width);
        page.Height = p(sheet.Height);

        if (sheet.IsLandscape)
        {
            page.Orientation = PageOrientation.Landscape;
        }

        Sheets.SetPageMargin(sheet, PrintMargin);

        var gx = XGraphics.FromPdfPage(page);

        gx.SmoothingMode = XSmoothingMode.HighQuality;

        foreach (var field in sheet.SheetFields)
        {
            if (!FieldOnCurPageHelper(field, sheet, PrintMargin, yPos, _pagesPrinted))
            {
                continue;
            }

            switch (field.FieldType)
            {
                case SheetFieldType.Image:
                case SheetFieldType.PatImage:
                    DrawFieldImage(field, null, gx);
                    break;
                case SheetFieldType.Drawing:
                    DrawFieldDrawing(field, null, gx);
                    break;
                case SheetFieldType.Rectangle:
                    DrawFieldRectangle(field, null, gx, _yPosPrint);
                    break;
                case SheetFieldType.Line:
                    DrawFieldLine(field, null, gx);
                    break;
                case SheetFieldType.Special:
                    SheetPrinting.DrawFieldSpecial(sheet, field, null, gx, yPos);
                    break;
                case SheetFieldType.Grid:
                    DrawFieldGrid(field, sheet, null, gx, dataSet, _stmt, pat: pat, patGuar: patGuar);
                    break;
                case SheetFieldType.InputField:
                case SheetFieldType.OutputText:
                case SheetFieldType.StaticText:
                    DrawFieldText(field, sheet, null, gx, _yPosPrint);
                    break;
                case SheetFieldType.CheckBox:
                    DrawFieldCheckBox(field, null, gx);
                    break;
                case SheetFieldType.ComboBox:
                    DrawFieldComboBox(field, sheet, null, gx);
                    break;
                case SheetFieldType.SigBox:
                case SheetFieldType.SigBoxPractice:
                    DrawFieldSigBox(field, sheet, null, gx);
                    break;
                case SheetFieldType.Parameter:
                default:
                    //Parameter or possibly new field type.
                    break;
            }
        }

        DrawHeader(sheet, null, gx, _pagesPrinted, _yPosPrint);
        DrawFooter(sheet, null, gx, _pagesPrinted, _yPosPrint);

        gx.Dispose();

        yPos += sheet.HeightPage - (PrintMargin.Bottom + PrintMargin.Top);

        _pagesPrinted++;

        if (_pagesPrinted < Sheets.CalculatePageCount(sheet, PrintMargin))
        {
        }
        else
        {
            yPos = 0;
                
            _pagesPrinted = 0;
        }

        return yPos;
    }

    public void DrawFieldCheckBox(SheetField field, Graphics g, XGraphics gx)
    {
        if (field.FieldValue != "X")
        {
            return;
        }

        if (gx == null)
        {
            using var pen3 = new Pen(Brushes.Black, 1.6f);
                
            g.DrawLine(pen3, field.XPos, field.YPos - _yPosPrint, field.XPos + field.Width, field.YPos - _yPosPrint + field.Height);
            g.DrawLine(pen3, field.XPos + field.Width, field.YPos - _yPosPrint, field.XPos, field.YPos - _yPosPrint + field.Height);
        }
        else
        {
            var pen3 = new XPen(XColors.Black, p(1.6f));
            gx.DrawLine(pen3, p(field.XPos), p(field.YPos - _yPosPrint), p(field.XPos + field.Width), p(field.YPos - _yPosPrint + field.Height));
            gx.DrawLine(pen3, p(field.XPos + field.Width), p(field.YPos - _yPosPrint), p(field.XPos), p(field.YPos - _yPosPrint + field.Height));
        }
    }

    public void DrawFieldComboBox(SheetField field, Sheet sheet, Graphics g, XGraphics gx)
    {
        var comboChoice = field.FieldValue.Split(';')[0];
        var fontName = (string.IsNullOrEmpty(sheet.FontName) ? FontFamily.GenericMonospace.ToString() : sheet.FontName);
        if (gx == null)
        {
            //The 0.58 is to make it scale with height of the combobox.
            //See discussion over in FormSheetFillEdit.Paint.
            var font = new Font(fontName, field.Height * 0.58f, FontStyle.Regular);
            var bounds = new Rectangle(field.XPos, field.YPos - _yPosPrint, field.Width, field.Height);
            GraphicsHelper.DrawString(g, comboChoice, font, Brushes.Black, bounds, HorizontalAlignment.Left);
            font.Dispose();
        }
        else
        {
            var xfont = new XFont(fontName, field.Height * 0.58f, XFontStyle.Regular);
            var rect = new RectangleF(field.XPos, field.YPos - _yPosPrint, field.Width, field.Height);
            GraphicsHelper.DrawStringX(gx, comboChoice, xfont, XBrushes.Black, rect, HorizontalAlignment.Left);
        }
    }

    public void DrawFieldDrawing(SheetField field, Graphics g, XGraphics gx)
    {
        if (gx == null)
        {
            var pen = new Pen(Brushes.Black, 2f);
            var points = new List<Point>();
            var pairs = field.FieldValue.Split(new string[] {";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pairs)
            {
                points.Add(new Point(SIn.Int(p.Split(',')[0]), SIn.Int(p.Split(',')[1])));
            }

            for (var i = 1; i < points.Count; i++)
            {
                g.DrawLine(pen, points[i - 1].X, points[i - 1].Y - _yPosPrint, points[i].X, points[i].Y - _yPosPrint);
            }

            pen.Dispose();
            pen = null;
        }
        else
        {
            var pen = new XPen(XColors.Black, p(2));
            var points = new List<Point>();
            var pairs = field.FieldValue.Split(new string[] {";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p2 in pairs)
            {
                points.Add(new Point(SIn.Int(p2.Split(',')[0]), SIn.Int(p2.Split(',')[1])));
            }

            for (var i = 1; i < points.Count; i++)
            {
                gx.DrawLine(pen, p(points[i - 1].X), p(points[i - 1].Y - _yPosPrint), p(points[i].X), p(points[i].Y - _yPosPrint));
            }

            pen = null;
        }
    }

    public void DrawFieldGrid(SheetField field, Sheet sheet, Graphics g, XGraphics gx, DataSet dataSet, Statement stmt, bool isPrinting = false, Patient pat = null, Patient patGuar = null, float scaleMS = 1)
    {
        if (sheet.SheetType != SheetTypeEnum.ERA)
        {
            DrawFieldGridHelper(field, sheet, g, gx, dataSet, stmt, isPrinting, pat: pat, patGuar: patGuar, scaleMS: scaleMS);
            return;
        }

        //ERA sheets can only have grid field such that field.FieldName=="EraClaimsPaid".
        var param = SheetParameter.GetParamByName(sheet.Parameters, "IsSingleClaimPaid");
        var isSingleClaim = (param.ParamValue == null) ? false : true; //param is only set when true
        //This logic mimics SheetUtil.CalculateHeights(...)
        var isOneClaimPerPage = PrefC.GetBool(PrefName.EraPrintOneClaimPerPage);
        if (isSingleClaim)
        {
            //When printing a single claim we do not want to print the claim on the next page like we would when printing every claim.
            isOneClaimPerPage = false;
        }

        if (isOneClaimPerPage && _pagesPrinted == 0)
        {
            //First page is the ERA fields page.  Continue to next page unless this is a single claim being printed.
            return;
        }

        var gridHeaderSheetDef = SheetDefs.GetInternalOrCustom(SheetInternalType.ERAGridHeader);
        var era = (X835) SheetParameter.GetParamByName(sheet.Parameters, "ERA").ParamValue; //Required param
        var tablePaidProcs = SheetDataTableUtil.GetDataTableForGridType(sheet, dataSet, field.FieldName, stmt);
        var tableCodes = SheetDataTableUtil.GetDataTableForGridType(sheet, dataSet, "EraClaimsPaidCodes", stmt);
        var sheetGridHeader = SheetUtil.CreateSheet(gridHeaderSheetDef);
        var yPosStep = YPosPrevious;
        var printableHeight = sheet.Height - PrintMargin.Top - PrintMargin.Bottom;
        var pageMinY = (_pagesPrinted * printableHeight);
        var pageMaxY = ((_pagesPrinted + 1) * printableHeight);
        for (var i = IdxPreClaimPaidPrinted; i < era.ListClaimsPaid.Count; i++)
        {
            var claim = era.ListClaimsPaid[i];
            //Work with copy so that original position values do not change per iteration.
            var fieldCopy = field.Copy();
            var sheetGridHeaderCopy = sheetGridHeader.Copy();
            if (isOneClaimPerPage)
            {
                fieldCopy.YPos = PrintMargin.Top + 1; //Set field to top of page.  Plus 1 for padding.
                yPosStep += printableHeight;
            }

            fieldCopy.YPos += yPosStep;
            if (fieldCopy.YPos >= pageMaxY //Field is on the next page.
                || fieldCopy.YPos <= pageMaxY && fieldCopy.YPos + sheetGridHeader.Height > pageMaxY) //Header is split between 2 pages.  Let Next page handle.
            {
                break; //Done with this page.
            }

            //At this point claim has not been drawn on any previous page.
            if (fieldCopy.YPos <= pageMinY)
            {
                //Claim is not drawn and YPos is above the current page, so we must adjust it.
                yPosStep += ((pageMinY + PrintMargin.Top + 1) - fieldCopy.YPos); //The difference of what we are about to add the YPos.
                fieldCopy.YPos = pageMinY + PrintMargin.Top + 1; //Set to top of page, plus 1 for padding.
            }

            sheetGridHeaderCopy.SheetFields.ForEach(x =>
            {
                //Make header sheet possitions relative to parent sheet and grid field location.
                x.XPos += fieldCopy.XPos;
                x.YPos += fieldCopy.YPos;
            });
            SheetParameter.GetParamByName(sheetGridHeaderCopy.Parameters, "EraClaimPaid").ParamValue = claim; //Required param
            SheetParameter.GetParamByName(sheetGridHeaderCopy.Parameters, "ClaimIndexNum").ParamValue = (isSingleClaim ? 0 : era.ListClaimsPaid.IndexOf(claim) + 1); //0 index so +1
            SheetFiller.FillFields(sheetGridHeaderCopy);
            pd_DrawFieldsHelper(sheetGridHeaderCopy, g, gx, sheet);
            fieldCopy.YPos += sheetGridHeaderCopy.Height;
            var procGridHeight = DrawFieldGridHelper(fieldCopy, sheet, g, gx, dataSet, stmt, isPrinting, claim.ClpSegmentIndex, tablePaidProcs);
            var fieldCodes = fieldCopy.Copy();
            fieldCodes.FieldName = "EraClaimsPaidCodes";
            fieldCodes.YPos = fieldCopy.YPos + procGridHeight;
            var codeGridHeight = DrawFieldGridHelper(fieldCodes, sheet, g, gx, dataSet, stmt, isPrinting, claim.ClpSegmentIndex, tableCodes);
            var subFieldHeight = sheetGridHeaderCopy.Height + procGridHeight + codeGridHeight;
            if (isOneClaimPerPage)
            {
                if (subFieldHeight > printableHeight)
                {
                    //Grid wraps onto a second page.
                    yPosStep += printableHeight * (subFieldHeight / printableHeight); //Truncation intended.  Add extra pages when needed.
                }
            }
            else
            {
                subFieldHeight += 25;
                yPosStep += subFieldHeight;
            }

            IdxPreClaimPaidPrinted++;
            YPosPrevious = yPosStep;
        }
    }

    public int DrawFieldGridHelper(SheetField sheetField, Sheet sheet, Graphics g, XGraphics gx, DataSet dataSet, Statement stmt, bool isPrinting = false, long eraClaimSegmentIdx = -1, DataTable table = null, Patient pat = null, Patient patGuar = null, float scaleMS = 1)
    {
        if (stmt != null && stmt.StatementType == StmtType.LimitedStatement && sheetField.FieldName.StartsWith("StatementAging"))
        {
            return 0;
        }

        Sheets.SetPageMargin(sheet, PrintMargin);
        var odGrid = new GridOD(); //Only used for measurements, also contains printing/drawing logic.
        odGrid.BeginUpdate();
        odGrid.IsForSheets = true;
        odGrid.VScrollVisible = false;
        if (!string.IsNullOrEmpty(sheetField.FontName))
        {
            odGrid.FontForSheets = new Font(sheetField.FontName, sheetField.FontSize, sheetField.FontIsBold ? FontStyle.Bold : FontStyle.Regular);
        }

        var _yAdjCurRow = 0; //used to adjust for Titles, Headers, Rows, and footers (all considered part of the same row).
        var listDisplayFields = SheetUtil.GetGridColumnsAvailable(sheetField.FieldName);
        if (table == null)
        {
            table = SheetDataTableUtil.GetDataTableForGridType(sheet, dataSet, sheetField.FieldName, stmt, patGuar: patGuar);
        }

        if (sheet.SheetType == SheetTypeEnum.PaymentPlan)
        {
            if (!table.Columns.Contains("Adjustment"))
            {
                listDisplayFields.RemoveAll(x => x.InternalName == "Adjustment");
            }

            if (!table.Columns.Contains("Provider"))
            {
                listDisplayFields.RemoveAll(x => x.InternalName == "Provider");
            }
        }

        if (sheetField.FieldName == "TreatPlanMain")
        {
            var tpType = (TreatPlanType) SIn.Int(table.Rows[0]["paramTreatPlanType"].ToString());
            switch (tpType)
            {
                case TreatPlanType.Discount:
                    listDisplayFields.RemoveAll(x => x.InternalName == "Pri Ins" || x.InternalName == "Sec Ins" || x.InternalName == "Allowed");
                    break;
                case TreatPlanType.Insurance:
                    listDisplayFields.RemoveAll(x => x.InternalName == "DPlan");
                    break;
            }

            listDisplayFields.RemoveAll(x => x.InternalName == DisplayFields.InternalNames.TreatmentPlanModule.Appt);
        }

        FilterColumnsHelper(sheet, sheetField, listDisplayFields);
        odGrid.Width = listDisplayFields.Sum(x => x.ColumnWidth); //sum of empty list will return 0
        odGrid.Height = sheetField.Height;
        odGrid.SheetYPos = sheetField.YPos;
        odGrid.Title = sheetField.FieldName;
        if (stmt != null)
        {
            odGrid.Title += ((stmt.Intermingled || stmt.SinglePatient) ? ".Intermingled" : ".NotIntermingled"); //Important for calculating heights.
        }

        odGrid.SheetTopMargin = PrintMargin.Top;
        odGrid.SheetBottomMargin = PrintMargin.Bottom;
        odGrid.SheetPageHeight = sheet.HeightPage;

        #region Fill Grid, Set Text Alignment

        odGrid.BeginUpdate();
        odGrid.Columns.Clear();
        GridColumn col;
        foreach (var colCur in listDisplayFields)
        {
            if (string.IsNullOrEmpty(colCur.Description))
            {
                col = new GridColumn(colCur.InternalName, colCur.ColumnWidth);
            }
            else
            {
                col = new GridColumn(colCur.Description, colCur.ColumnWidth);
            }

            switch (sheetField.FieldName + "." + colCur.InternalName)
            {
                //Unusual switch statement to differentiate similar column names in different grids.
                case "StatementMain.charges":
                case "StatementMain.credits":
                case "StatementMain.balance":
                case "StatementPayPlan.charges":
                case "StatementPayPlan.credits":
                case "StatementPayPlan.balance":
                case "StatementPayPlanOld.charges":
                case "StatementPayPlanOld.credits":
                case "StatementPayPlanOld.balance":
                case "StatementDynamicPayPlan.charges":
                case "StatementDynamicPayPlan.credits":
                case "StatementDynamicPayPlan.balance":
                case "StatementPayPlanGrid.charges":
                case "StatementPayPlanGrid.credits":
                case "StatementPayPlanGrid.balance":
                case "StatementInvoicePayment.amt":
                case "TreatPlanMain.Allowed":
                case "TreatPlanMain.Fee":
                case "TreatPlanMain.Pri Ins":
                case "TreatPlanMain.Sec Ins":
                case "TreatPlanMain.DPlan":
                case "TreatPlanMain.Discount":
                case "TreatPlanMain.Pat":
                case "TreatPlanMain.Tax Est":
                case "TreatPlanMain." + DisplayFields.InternalNames.TreatmentPlanModule.CatPercUcr:
                case "TreatPlanBenefitsFamily.Primary":
                case "TreatPlanBenefitsFamily.Secondary":
                case "TreatPlanBenefitsIndividual.Primary":
                case "TreatPlanBenefitsIndividual.Secondary":
                case "PayPlanMain.Principal":
                case "PayPlanMain.Interest":
                case "PayPlanMain.due":
                case "PayPlanMain.payment":
                case "PayPlanMain.balance":
                case "PayPlanMain.Adjustment":
                case "EraClaimsPaid.FeeBilled":
                case "EraClaimsPaid.PatResp":
                case "EraClaimsPaid.Contractual":
                case "EraClaimsPaid.PayorReduct":
                case "EraClaimsPaid.OtherAdjust":
                case "EraClaimsPaid.InsPaid":
                case "EraClaimsPaid.RemarkCodes":
                case "EraClaimsPaid.AdjCodes":
                    col.TextAlign = HorizontalAlignment.Right;
                    break;
                case "StatementAging.Age00to30":
                case "StatementAging.Age31to60":
                case "StatementAging.Age61to90":
                case "StatementAging.Age90plus":
                case "StatementAging.AcctTotal":
                    if (sheet.SheetType == SheetTypeEnum.Statement && stmt != null && stmt.SuperFamily != 0)
                    {
                        col.TextAlign = HorizontalAlignment.Right;
                    }
                    else
                    {
                        col.TextAlign = HorizontalAlignment.Center;
                    }

                    break;
                case "StatementEnclosed.AmountDue":
                case "StatementEnclosed.DateDue":
                case "EraClaimsPaid.ProcCode":
                case "EraClaimsPaidCodes.Code":
                    col.TextAlign = HorizontalAlignment.Center;
                    break;
                default:
                    col.TextAlign = HorizontalAlignment.Left;
                    break;
            }

            odGrid.Columns.Add(col);
        }

        GridRow row;
        foreach (DataRow rowCur in table.Rows)
        {
            if (eraClaimSegmentIdx != -1 && rowCur["ClpSegmentIndex"].ToString() != eraClaimSegmentIdx.ToString())
            {
                continue;
            }

            row = new GridRow();
            foreach (var colCur in listDisplayFields)
            {
                //Some DisplayFields require additional formatting and/or logic to display properly in a grid.
                var value = SheetUtil.GetStringFromInternalName(sheetField, colCur, rowCur);
                row.Cells.Add(value);
            }

            if (table.Columns.Contains("PatNum"))
            {
                //Used for statments to determine account splitting.
                row.Tag = rowCur["PatNum"].ToString();
            }

            //Colored Text
            if (table.Columns.Contains("paramTextColor") && !string.IsNullOrEmpty(rowCur["paramTextColor"].ToString()))
            {
                var cRowText = Color.FromArgb(SIn.Int(rowCur["paramTextColor"].ToString()));
                if (!cRowText.IsEmpty)
                {
                    row.ColorText = cRowText;
                }
            }

            //Bold Text
            if (table.Columns.Contains("paramIsBold"))
            {
                row.Bold = (bool) rowCur["paramIsBold"];
            }

            if (table.Columns.Contains("paramIsBorderBoldBottom"))
            {
                if ((bool) rowCur["paramIsBorderBoldBottom"])
                {
                    row.ColorLborder = Color.Black;
                }
            }

            odGrid.ListGridRows.Add(row);
        }

        odGrid.EndUpdate(); //Calls ComputeRows and ComputeColumns, meaning the RowHeights int[] has been filled.

        #endregion

        for (var i = 0; i < odGrid.ListGridRows.Count; i++)
        {
            var gridSheetRow = odGrid.ListGridSheetRows[i];
            if (_isPrinting
                && (gridSheetRow.YPos - PrintMargin.Top < _yPosPrint //rows at the end of previous page
                    || gridSheetRow.YPos - sheet.HeightPage + PrintMargin.Bottom > _yPosPrint))
            {
                continue; //continue because we do not want to draw rows from other pages.
            }

            _yAdjCurRow = 0;
            //if(printRowCur.YPos<_yPosPrint
            //	|| printRowCur.YPos-_yPosPrint>sheet.HeightPage) {
            //	continue;//skip rows on previous page and rows on next page.
            //}

            #region Draw Title

            var heightGridTitle = 18;
            if (gridSheetRow.IsTitleRow)
            {
                using var font = new Font("Arial", 10, FontStyle.Bold);
                switch (sheetField.FieldName)
                {
                    //Draw titles differently for different grids.
                    case "StatementMain":
                        var patNum = SIn.Long(table.Rows[i]["PatNum"].ToString());
                        var patient = (pat == null || pat.PatNum != patNum ? Patients.GetPat(patNum) : pat);
                        var patName = "";
                        if (patient != null)
                        {
                            //should always be true
                            patName = patient.GetNameFLnoPref() + (stmt.SuperFamily != 0 ? " - " + patient.PatNum : ""); //Append patnum only if super statement
                        }

                        if (gx == null)
                        {
                            g.FillRectangle(Brushes.White, sheetField.XPos - 10, gridSheetRow.YPos - _yPosPrint, odGrid.Width + 10, heightGridTitle);
                            g.DrawString(patName, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), sheetField.XPos - 10, gridSheetRow.YPos - _yPosPrint);
                        }
                        else
                        {
                            gx.DrawRectangle(Brushes.White, p(sheetField.XPos - 10), p(gridSheetRow.YPos - _yPosPrint - 1), p(odGrid.Width + 10), p(heightGridTitle));
                            GraphicsHelper.DrawStringX(gx, patName,
                                new XFont(font.FontFamily.ToString(), font.Size, XFontStyle.Bold), XBrushes.Black,
                                new RectangleF(sheetField.XPos - 10, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width + 10, heightGridTitle), HorizontalAlignment.Left);
                            //gx.DrawString(patName,new XFont(_font.FontFamily.ToString(),_font.Size,XFontStyle.Bold),new SolidBrush(Color.Black),field.XPos-10,yPosGrid);
                        }

                        break;
                    case "StatementPayPlan":
                    case "StatementPayPlanOld":
                    case "StatementDynamicPayPlan":
                    case "StatementPayPlanGrid":
                        var text = "Payment Plans";
                        if (gx == null)
                        {
                            var sizeFString = g.MeasureString(text, font);
                            g.FillRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint, odGrid.Width, heightGridTitle);
                            g.DrawString(text, font, Brushes.Black, sheetField.XPos + (sheetField.Width - sizeFString.Width) / 2, gridSheetRow.YPos - _yPosPrint);
                        }
                        else
                        {
                            gx.DrawRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle);
                            GraphicsHelper.DrawStringX(gx, text,
                                new XFont(font.FontFamily.ToString(), font.Size, XFontStyle.Bold), XBrushes.Black,
                                new RectangleF(sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle), HorizontalAlignment.Center);
                            //gx.DrawString("Payment Plans",new XFont(_font.FontFamily.ToString(),_font.Size,XFontStyle.Bold),new SolidBrush(Color.Black),field.XPos+(field.Width-sSize.Width)/2,yPosGrid);
                        }

                        break;
                    //grid that shows all payments made on the day of the sheet or attached to procedures on the invoice. 
                    //only for invoices.
                    case "StatementInvoicePayment":
                        //drawing logic is mostly copied from PayPlan grid logic above.
                        if (gx == null)
                        {
                            var sizeFString = g.MeasureString("Payments & WriteOffs", font);
                            g.FillRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint, odGrid.Width, heightGridTitle);
                            g.DrawString("Payments & WriteOffs", font, new SolidBrush(Color.Black), sheetField.XPos, gridSheetRow.YPos - _yPosPrint);
                        }
                        else
                        {
                            gx.DrawRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle);
                            GraphicsHelper.DrawStringX(gx, "Payments",
                                new XFont(font.FontFamily.ToString(), font.Size, XFontStyle.Bold), XBrushes.Black,
                                new RectangleF(sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle), HorizontalAlignment.Center);
                        }

                        break;
                    case "TreatPlanBenefitsFamily":
                        if (gx == null)
                        {
                            var sizeFString = g.MeasureString("Family Insurance Benefits", font);
                            g.FillRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint, odGrid.Width, heightGridTitle);
                            g.DrawString("Family Insurance Benefits", font, Brushes.Black, sheetField.XPos + (sheetField.Width - sizeFString.Width) / 2, gridSheetRow.YPos - _yPosPrint);
                        }
                        else
                        {
                            gx.DrawRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle);
                            GraphicsHelper.DrawStringX(gx, "Family Insurance Benefits",
                                new XFont(font.FontFamily.ToString(), font.Size, XFontStyle.Bold), XBrushes.Black,
                                new RectangleF(sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle), HorizontalAlignment.Center);
                            //gx.DrawString("Payment Plans",new XFont(_font.FontFamily.ToString(),_font.Size,XFontStyle.Bold),new SolidBrush(Color.Black),field.XPos+(field.Width-sSize.Width)/2,yPosGrid);
                        }

                        break;
                    case "TreatPlanBenefitsIndividual":
                        if (gx == null)
                        {
                            var sizeFString = g.MeasureString("Individual Insurance Benefits", font);
                            g.FillRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint, odGrid.Width, heightGridTitle);
                            g.DrawString("Individual Insurance Benefits", font, Brushes.Black, sheetField.XPos + (sheetField.Width - sizeFString.Width) / 2, gridSheetRow.YPos - _yPosPrint);
                        }
                        else
                        {
                            gx.DrawRectangle(Brushes.White, sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle);
                            GraphicsHelper.DrawStringX(gx, "Individual Insurance Benefits",
                                new XFont(font.FontFamily.ToString(), font.Size, XFontStyle.Bold), XBrushes.Black,
                                new RectangleF(sheetField.XPos, gridSheetRow.YPos - _yPosPrint - 1, odGrid.Width, heightGridTitle), HorizontalAlignment.Center);
                            //gx.DrawString("Payment Plans",new XFont(_font.FontFamily.ToString(),_font.Size,XFontStyle.Bold),new SolidBrush(Color.Black),field.XPos+(field.Width-sSize.Width)/2,yPosGrid);
                        }

                        break;
                    default:
                        if (gx == null)
                        {
                            odGrid.SheetDrawTitle(g, sheetField.XPos, gridSheetRow.YPos - _yPosPrint, scaleMS: 1);
                        }
                        else
                        {
                            odGrid.SheetDrawTitleX(gx, sheetField.XPos, gridSheetRow.YPos - _yPosPrint);
                        }

                        break;
                }

                _yAdjCurRow += heightGridTitle;
            }

            #endregion

            #region Draw Header

            if (gridSheetRow.IsHeaderRow)
            {
                if (gx == null)
                {
                    odGrid.SheetDrawHeader(g, sheetField.XPos, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, scaleMS);
                }
                else
                {
                    odGrid.SheetDrawHeaderX(gx, sheetField.XPos, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow);
                }

                _yAdjCurRow += 15;
            }

            #endregion

            #region Draw Row

            if (gx == null)
            {
                odGrid.SheetDrawRow(i, g, sheetField.XPos, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, gridSheetRow.IsBottomRow, true, isPrinting, scaleMS);
            }
            else
            {
                odGrid.SheetDrawRowX(i, gx, sheetField.XPos, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, gridSheetRow.IsBottomRow, true);
            }

            _yAdjCurRow += odGrid.ListGridRows[i].State.HeightMain;

            #endregion

            #region Draw Footer (rare)

            if (gridSheetRow.IsFooterRow)
            {
                _yAdjCurRow += 2;
                switch (sheetField.FieldName)
                {
                    case "StatementPayPlan":
                    case "StatementPayPlanOld":
                    case "StatementDynamicPayPlan":
                    case "StatementPayPlanGrid":
                        var descript = "patientPayPlanDue";
                        var textAmountDue = "Payment Plan Amount Due: ";
                        if (sheetField.FieldName == "StatementDynamicPayPlan" || sheetField.FieldName == "StatementPayPlanGrid")
                        {
                            descript = "dynamicPayPlanDue";
                            textAmountDue = "Payment Plan Amount Due:";
                        }

                        var tableMisc = dataSet.Tables["misc"];
                        if (tableMisc == null)
                        {
                            tableMisc = new DataTable();
                        }

                        var payPlanDue = tableMisc.Rows.OfType<DataRow>().Where(x => x["descript"].ToString() == descript).Sum(x => SIn.Double(x["value"].ToString()));
                        if (gx == null)
                        {
                            var rf = new RectangleF(sheet.Width - 60 - sheetField.Width, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, sheetField.Width, heightGridTitle);
                            g.FillRectangle(Brushes.White, rf);
                            var sf = new StringFormat();
                            sf.Alignment = StringAlignment.Far;
                            g.DrawString(textAmountDue + payPlanDue.ToString("c"), new Font("Arial", 9, FontStyle.Bold), new SolidBrush(Color.Black), rf, sf);
                        }
                        else
                        {
                            gx.DrawRectangle(Brushes.White, p(sheet.Width - sheetField.Width - 60), p(gridSheetRow.YPos - _yPosPrint + _yAdjCurRow), p(sheetField.Width), p(heightGridTitle));
                            using (var _font = new Font("Arial", 9, FontStyle.Bold))
                            {
                                GraphicsHelper.DrawStringX(gx, textAmountDue + payPlanDue.ToString("c"), new XFont(_font.FontFamily.ToString(), _font.Size, XFontStyle.Bold), XBrushes.Black, new RectangleF(sheet.Width - sheetField.Width - 60, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, sheetField.Width, heightGridTitle), HorizontalAlignment.Right);
                            }
                        }

                        break;
                    case "StatementInvoicePayment":
                        //Table should be filled with payments for today & attached to procs on the invoice.
                        //drawing logic copied from payplan logic above.
                        if (table == null)
                        {
                            tableMisc = new DataTable();
                        }

                        var totalPayments = table.Select().Sum(x => SIn.Double(x["amt"].ToString()));
                        if (gx == null)
                        {
                            var rf = new RectangleF(sheet.Width - 60 - sheetField.Width, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, sheetField.Width, heightGridTitle);
                            g.FillRectangle(Brushes.White, rf);
                            var sf = new StringFormat();
                            sf.Alignment = StringAlignment.Far;
                            if (PrefC.GetBool(PrefName.InvoicePaymentsGridShowNetProd))
                            {
                                g.DrawString("Total Payments & WriteOffs: " + totalPayments.ToString("c"), new Font("Arial", 9, FontStyle.Bold), new SolidBrush(Color.Black), rf, sf);
                            }
                            else
                            {
                                g.DrawString("Total Payments: " + totalPayments.ToString("c"), new Font("Arial", 9, FontStyle.Bold), new SolidBrush(Color.Black), rf, sf);
                            }
                        }
                        else
                        {
                            gx.DrawRectangle(Brushes.White, p(sheet.Width - sheetField.Width - 60), p(gridSheetRow.YPos - _yPosPrint + _yAdjCurRow), p(sheetField.Width), p(heightGridTitle));
                            using (var _font = new Font("Arial", 9, FontStyle.Bold))
                            {
                                if (PrefC.GetBool(PrefName.InvoicePaymentsGridShowNetProd))
                                {
                                    GraphicsHelper.DrawStringX(gx, "Total Payments & WriteOffs: " + totalPayments.ToString("c"), new XFont(_font.FontFamily.ToString(), _font.Size, XFontStyle.Bold), XBrushes.Black, new RectangleF(sheet.Width - sheetField.Width - 60, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, sheetField.Width, heightGridTitle), HorizontalAlignment.Right);
                                }
                                else
                                {
                                    GraphicsHelper.DrawStringX(gx, "Total Payments: " + totalPayments.ToString("c"), new XFont(_font.FontFamily.ToString(), _font.Size, XFontStyle.Bold), XBrushes.Black, new RectangleF(sheet.Width - sheetField.Width - 60, gridSheetRow.YPos - _yPosPrint + _yAdjCurRow, sheetField.Width, heightGridTitle), HorizontalAlignment.Right);
                                }
                            }
                        }

                        break;
                }
            }

            #endregion
        }

        odGrid.Dispose();
        return odGrid.SheetPrintHeight;
    }

    public void DrawFieldImage(SheetField field, Graphics g, XGraphics gx, Bitmap image = null)
    {
        Bitmap bmpOriginal = null;
        ImageFormat bmpOriginalFormat = null;
        Document patDoc = null;
        var filePathAndName = "";
        if (image != null)
        {
            bmpOriginal = image;
            filePathAndName = "image Parameter";
        }
        else
        {
            #region Get the path for the image

            switch (field.FieldType)
            {
                case SheetFieldType.Image:
                    filePathAndName = ODFileUtils.CombinePaths(SheetUtil.GetClinicImagePath(field.FieldName), field.FieldName);
                    break;
                case SheetFieldType.PatImage:
                    if (field.FieldValue == "")
                    {
                        //There is no document object to use for display, but there may be a baked in image and that situation is dealt with below.
                        filePathAndName = "";
                        break;
                    }

                    if (field.FieldValue.StartsWith("MountNum:"))
                    {
                        var mountNum = SIn.Long(field.FieldValue.Substring(9));
                        bmpOriginal = MountHelper.GetBitmapOfMountFromDb(mountNum);
                        bmpOriginalFormat = ImageFormat.Jpeg;
                        break;
                    }

                    patDoc = Documents.GetByNum(SIn.Long(field.FieldValue));
                    var paths = Documents.GetPaths(new List<long> {patDoc.DocNum}, ImageStore.GetDataFolder());
                    if (paths.Count < 1)
                    {
                        //No path was found so we cannot draw the image.
                        return;
                    }

                    filePathAndName = paths[0];
                    break;
                default:
                    //not an image field
                    return;
            }

            #endregion

            #region Load the image into bmpOriginal

            if (field.FieldValue.StartsWith("MountNum:"))
            {
                //we already set the image
            }
            else if (field.FieldName == "Patient Info.gif")
            {
                bmpOriginal = Resources.Patient_Info;
                bmpOriginalFormat = ImageFormat.Gif;
            }
            else if (File.Exists(filePathAndName))
            {
                //Local AtoZ
                try
                {
                    bmpOriginal = new Bitmap(filePathAndName);
                    bmpOriginalFormat = bmpOriginal.RawFormat;
                }
                catch
                {
                    return; //If the image is not an actual image file, leave the image field blank.
                }
            }
            else
            {
                return;
            }

            if (field.FieldType == SheetFieldType.PatImage && !field.FieldValue.StartsWith("MountNum:") && patDoc.DocNum != 0)
            {
                var bmpCopy = ImageHelper.ApplyDocumentSettingsToImage(patDoc, bmpOriginal, ImageSettingFlags.ALL);
                bmpOriginal.Dispose();
                bmpOriginal = bmpCopy;
            }

            #endregion
        }

        #region Calculate the image ratio and location, set values for imgDrawWidth and imgDrawHeight

        //inscribe image in field while maintaining aspect ratio.
        var imgRatio = (float) bmpOriginal.Width / (float) bmpOriginal.Height;
        var fieldRatio = (float) field.Width / (float) field.Height;
        float imgDrawHeight = field.Height; //drawn size of image
        float imgDrawWidth = field.Width; //drawn size of image
        var adjustY = 0; //added to YPos
        var adjustX = 0; //added to XPos
        //For patient images, we need to make sure the images will fit and can maintain aspect ratio.
        if (field.FieldType == SheetFieldType.PatImage && imgRatio > fieldRatio)
        {
            //image is too wide
            //X pos and width of field remain unchanged
            //Y pos and height must change
            imgDrawHeight = (float) bmpOriginal.Height * ((float) field.Width / (float) bmpOriginal.Width); //img.Height*(width based scale) This also handles images that are too small.
            adjustY = (int) ((field.Height - imgDrawHeight) / 2f); //adjustY= half of the unused vertical field space
        }
        else if (field.FieldType == SheetFieldType.PatImage && imgRatio < fieldRatio)
        {
            //image is too tall
            //X pos and width must change
            //Y pos and height remain unchanged
            imgDrawWidth = (float) bmpOriginal.Width * ((float) field.Height / (float) bmpOriginal.Height); //img.Height*(width based scale) This also handles images that are too small.
            adjustX = (int) ((field.Width - imgDrawWidth) / 2f); //adjustY= half of the unused horizontal field space
        }
        else
        {
            //image ratio == field ratio
            //do nothing
        }

        #endregion

        //We used to scale down bmpOriginal here to avoid memory exceptions.
        //Doing so was causing significant quality loss when printing or creating pdfs with very large images.
        if (gx == null)
        {
            try
            {
                //Always use the original BMP so that very large images can be scaled by the graphics class thus keeping a high quality image by using interpolation.
                g.DrawImage(bmpOriginal,
                    new Rectangle(field.XPos + adjustX, field.YPos + adjustY - _yPosPrint, (int) imgDrawWidth, (int) imgDrawHeight),
                    new Rectangle(0, 0, bmpOriginal.Width, bmpOriginal.Height),
                    GraphicsUnit.Pixel);
            }
            catch (OutOfMemoryException)
            {
                throw new OutOfMemoryException(Lans.g("Sheets", "A static image on this sheet is too high in quality and cannot be printed.") + "\r\n"
                                                                                                                                              + Lans.g("Sheets", "Try printing to a different printer or lower the quality of the static image") + ":\r\n"
                                                                                                                                              + filePathAndName);
            }
        }
        else
        {
            MemoryStream ms = null;
            ImageHelper.GetBitmapPDF(filePathAndName, ms);
            var xI = XImage.FromGdiPlusImage(bmpOriginal);
            gx.DrawImage(xI, p(field.XPos + adjustX), p(field.YPos - _yPosPrint + adjustY), p(imgDrawWidth), p(imgDrawHeight));
            xI.Dispose();
            xI = null;
            if (ms != null)
            {
                ms.Dispose();
                ms = null;
            }
        }

        if (bmpOriginal != null)
        {
            bmpOriginal.Dispose();
            bmpOriginal = null;
        }
    }

    public void DrawFieldLine(SheetField field, Graphics g, XGraphics gx)
    {
        if (gx == null)
        {
            g.DrawLine((field.ItemColor.ToArgb() == Color.FromArgb(0).ToArgb() ? Pens.Black : new Pen(field.ItemColor, 1)),
                field.XPos, field.YPos - _yPosPrint,
                field.XPos + field.Width,
                field.YPos - _yPosPrint + field.Height);
        }
        else
        {
            gx.DrawLine((field.ItemColor.ToArgb() == Color.FromArgb(0).ToArgb() ? XPens.Black : new XPen(field.ItemColor, 1)),
                p(field.XPos), p(field.YPos - _yPosPrint),
                p(field.XPos + field.Width),
                p(field.YPos - _yPosPrint + field.Height));
        }
    }

    public static void DrawFieldRectangle(SheetField field, Graphics g, XGraphics gx, int yPosPrint)
    {
        if (gx == null)
        {
            g.DrawRectangle((field.ItemColor.ToArgb() == Color.FromArgb(0).ToArgb() ? Pens.Black : new Pen(field.ItemColor, 1)),
                field.XPos,
                field.YPos - yPosPrint,
                field.Width,
                field.Height);
        }
        else
        {
            gx.DrawRectangle((field.ItemColor.ToArgb() == Color.FromArgb(0).ToArgb() ? Pens.Black : new Pen(field.ItemColor, 1)),
                p(field.XPos),
                p(field.YPos - yPosPrint),
                p(field.Width),
                p(field.Height));
        }
    }

    public void DrawFieldSigBox(SheetField field, Sheet sheet, Graphics g, XGraphics gx)
    {
        var sigImage = new Bitmap(field.Width, field.Height);
        string strSigned = null; //will be set if SheetType is not TreatmentPlan or PaymentPlan, FieldValue.Length>0, and DateTimeSig.Year>1880
        if (sheet.SheetType == SheetTypeEnum.TreatmentPlan)
        {
            sigImage = GetSigTPHelper(sheet, field);
        }
        else if (sheet.SheetType == SheetTypeEnum.PaymentPlan)
        {
            sigImage = GetSigPPHelper(sheet, field);
        }
        else
        {
            using var wrapper = new SignatureBoxWrapper();
            wrapper.Width = field.Width;
            wrapper.Height = field.Height;
            if (field.FieldValue.Length > 0)
            {
                //a signature is present
                var sigIsTopaz = false;
                if (field.FieldValue[0] == '1')
                {
                    sigIsTopaz = true;
                }

                var signature = "";
                if (field.FieldValue.Length > 1)
                {
                    signature = field.FieldValue.Substring(1);
                }

                //string keyData=Sheets.GetSignatureKey(sheet);//can't do this because some of the fields might have different new line characters. Sig will be invalid.
                wrapper.FillSignature(sigIsTopaz, field.SigKey, signature);
                sigImage = wrapper.GetSigImage();
                if (field.DateTimeSig.Year > 1880)
                {
                    strSigned = (wrapper.IsOldSigXWebForms() ? Lans.g("", "Typed Signature in Webforms: ") : Lans.g("", "Signed: ")) + field.DateTimeSig.ToString();
                }
            }
        }

        var fontName = (string.IsNullOrEmpty(sheet.FontName) ? FontFamily.GenericMonospace.ToString() : sheet.FontName);
        var fontSizeSigned = 8.25; //Mimics FormSheetFillEdit.LayoutFields
        var sigWidth = field.Width - 2;
        var sigHeight = field.Height - 2;
        if (g != null)
        {
            var bounds = new Rectangle(field.XPos, field.YPos - _yPosPrint, sigWidth, sigHeight);
            g.DrawImage(sigImage, bounds);
            if (!string.IsNullOrEmpty(strSigned))
            {
                //not a TreatmentPlan or PaymentPlan, FielValue.Length>0, and DateTimeSig.Year>1880
                var boundsSigned = new Rectangle(bounds.X + 1, bounds.Y + bounds.Height - 15, sigWidth - 2, 14); //Height 14 taken from FormSheetFillEdit
                var font = new Font(fontName, (float) fontSizeSigned, FontStyle.Regular);
                GraphicsHelper.DrawString(g, strSigned, font, Brushes.Black, boundsSigned, HorizontalAlignment.Left);
                font.Dispose();
            }
        }
        else
        {
            gx.DrawImage(XImage.FromGdiPlusImage(sigImage), p(field.XPos), p(field.YPos - _yPosPrint), p(sigWidth), p(sigHeight));
            if (!string.IsNullOrEmpty(strSigned))
            {
                //not a TreatmentPlan or PaymentPlan, FielValue.Length>0, and DateTimeSig.Year>1880
                var boundsSigned = new RectangleF(field.XPos + 1, field.YPos - _yPosPrint + field.Height - 15, sigWidth - 2, 14); //Height 14 taken from FormSheetFillEdit
                var xfont = new XFont(fontName, fontSizeSigned, XFontStyle.Regular);
                GraphicsHelper.DrawStringX(gx, strSigned, xfont, Brushes.Black, boundsSigned, HorizontalAlignment.Left);
            }
        }

        sigImage.Dispose();
    }

    public static void DrawFieldText(SheetField field, Sheet sheet, Graphics g, XGraphics gx, int yPosPrint)
    {
        RectangleF boundsActual;
        if (gx == null)
        {
            var fontstyle = (field.FontIsBold ? FontStyle.Bold : FontStyle.Regular);
            var font = new Font(field.FontName, field.FontSize, fontstyle);
            var bounds = new Rectangle(field.XPos, field.YPos - yPosPrint, field.Width, field.Height);
            var brushText = (Brush) Brushes.Black.Clone();
            if (field.ItemColor.ToArgb() != Color.FromArgb(0).ToArgb())
            {
                brushText = new SolidBrush(field.ItemColor);
            }

            boundsActual = GraphicsHelper.DrawString(g, field.FieldValue, font, brushText, bounds, field.TextAlign);
            brushText?.Dispose();
            font?.Dispose();
        }
        else
        {
            var xfontstyle = (field.FontIsBold ? XFontStyle.Bold : XFontStyle.Regular);
            XFont xfont;
            try
            {
                xfont = new XFont(field.FontName, field.FontSize, xfontstyle);
            }
            catch (Exception)
            {
                //There are some fonts that PdfSharp does not support.  Instead of showing an error, use our default font.
                xfont = new XFont("Courier New", field.FontSize, xfontstyle);
            }

            #region SheetDef.DateTCreated Bug Fix 16020

            //-------------------------------------------------------------------------------------------------------
            //This region was commented out while fixing a PDF text field issue for JobNum:34170.
            //The real fix for this issue was correcting a drawing problem with the YPos for text fields within DrawStringX().
            //Below is the original block of code from JobNum:16020.
            //They went to the legnths of adding a new column for that fix which makes us feel it might be more important than we're giving it credit.
            //-------------------------------------------------------------------------------------------------------
            //Subtract 5 from YPos to compensate for downward shift of text fields that occurs when creating a PDF from sheet. We don't want to shift
            //sheetdefs created before this bug fix. They will have a DateTCreated of 0001-01-01 by default. Internal sheets and deleted sheetdefs will
            //return null. We still want to shift text fields in these cases.
            //SheetDef sheetDef=SheetDefs.GetSheetDef(sheet.SheetDefNum,false);
            //if(sheetDef==null || sheetDef.DateTCreated.Year > 1880) {
            //	field.YPos-=5;
            //}

            #endregion

            var rect = new RectangleF(field.XPos, field.YPos - yPosPrint, field.Width, field.Height);
            XBrush xbrushText = XBrushes.Black;
            if (field.ItemColor.ToArgb() != Color.FromArgb(0).ToArgb())
            {
                xbrushText = new XSolidBrush(field.ItemColor);
            }

            boundsActual = GraphicsHelper.DrawStringX(gx, field.FieldValue, xfont, xbrushText, rect, field.TextAlign);
            //xfont.Dispose();
            //xfont=null;
        }

        if (field.FieldType == SheetFieldType.OutputText)
        {
            switch (sheet.SheetType.ToString() + "." + field.FieldName)
            {
                case "TreatmentPlan.Note":
                    //Add plus 4 to width and height to allow for border thickness.
                    var width = Math.Max(field.Width, (int) Math.Ceiling(boundsActual.Width + 4));
                    var height = (int) Math.Ceiling(boundsActual.Height + 4);
                    if (gx == null)
                    {
                        g.DrawRectangle(Pens.DarkGray,
                            new Rectangle((int) boundsActual.X, (int) boundsActual.Y, width, height));
                    }
                    else
                    {
                        gx.DrawRectangle(XPens.DarkGray,
                            new XRect(p(boundsActual.X), p(boundsActual.Y), p(width), p(height)));
                    }

                    break;
            }
        }
    }

    public static void DrawHeader(Sheet sheet, Graphics g, XGraphics gx, int pagesPrinted, int yPosPrint)
    {
        if (pagesPrinted == 0)
        {
            return; //Never draw header on first page
        }

        //white-out the header.
        if (gx == null)
        {
            g.FillRectangle(Brushes.White, 0, 0, sheet.WidthPage, PrintMargin.Top);
        }
        else
        {
            gx.DrawRectangle(XPens.White, Brushes.White, p(0), p(0), p(sheet.WidthPage), p(PrintMargin.Top));
        }
    }

    public static void DrawFooter(Sheet sheet, Graphics g, XGraphics gx, int pagesPrinted, int yPosPrint)
    {
        if (Sheets.CalculatePageCount(sheet, PrintMargin) == 1 && sheet.SheetType != SheetTypeEnum.MedLabResults)
        {
            return; //Never draw footers on single page sheets.
        }

        //whiteout footer.
        if (gx == null)
        {
            g.FillRectangle(Brushes.White, 0, sheet.HeightPage - PrintMargin.Bottom, sheet.WidthPage, sheet.HeightPage);
        }
        else
        {
            gx.DrawRectangle(XPens.White, Brushes.White, 0, sheet.HeightPage - PrintMargin.Bottom, sheet.WidthPage, sheet.HeightPage);
        }
    }

    public static void DrawCalibration(Sheet sheet, Graphics g, PrintPageEventArgs e, XGraphics gx, PdfPage page)
    {
        var font = new Font("Calibri", 10f, FontStyle.Regular);
        var xfont = new XFont("Calibri", p(10f), XFontStyle.Regular);
        var sLineSize = 15;
        var mLineSize = 45;
        var lLineSize = 90;
        for (var pass = 0; pass < 3; pass++)
        {
            var xO = 0; //xOrigin
            var yO = 0; //yOrigin
            switch (pass)
            {
                case 0: xO = yO = 0; break;
                case 1:
                    xO = sheet.WidthPage / 2;
                    yO = sheet.HeightPage / 2;
                    break;
                case 2:
                    xO = sheet.WidthPage;
                    yO = sheet.HeightPage;
                    break;
            }

            for (var i = -100; i < 2000; i++)
            {
                if (i % 100 == 0 && pass == 0)
                {
                    //label Axis
                    if (g != null)
                    {
                        if (i == 0)
                        {
                            g.DrawString(i.ToString(), font, Brushes.Black, new PointF(4, 4)); //label 0
                        } //don't draw the zero twice
                        else
                        {
                            g.DrawString(i.ToString(), font, Brushes.Black, new PointF(xO + 75, i + 2)); //label Y-axis
                            g.DrawString(i.ToString(), font, Brushes.Black, new PointF(i + 2, yO + 75)); //label X-axis
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                        } //don't draw the zero twice
                        else
                        {
                            gx.DrawString(i.ToString(), xfont, XBrushes.Black, p(xO + 75), p(i + 2)); //label Y-axis
                            gx.DrawString(i.ToString(), xfont, XBrushes.Black, p(i + 2), p(yO + 75)); //label X-axis
                        }
                    }
                }

                if (i % 100 == 0)
                {
                    //draw large lines and label txt
                    if (g != null)
                    {
                        g.DrawLine(Pens.Black, new Point(-lLineSize + xO, i), new Point(+lLineSize + xO, i)); //Allong Y-axis
                        g.DrawLine(Pens.Black, new Point(i, -lLineSize + yO), new Point(i, +lLineSize + yO)); //Allong X-axis
                    }
                    else
                    {
                        gx.DrawLine(XPens.Black, p(-lLineSize + xO), p(i), p(+lLineSize + xO), p(i)); //Allong Y-axis
                        gx.DrawLine(XPens.Black, p(i), p(-lLineSize + yO), p(i), p(+lLineSize + yO)); //Allong X-axis
                    }
                }
                else if (i % 50 == 0)
                {
                    //draw 50px lines
                    if (g != null)
                    {
                        g.DrawLine(Pens.Black, new Point(-mLineSize + xO, i), new Point(+mLineSize + xO, i)); //Allong Y-axis
                        g.DrawLine(Pens.Black, new Point(i, -mLineSize + yO), new Point(i, +mLineSize + yO)); //Allong X-axis
                    }
                    else
                    {
                        gx.DrawLine(XPens.Black, p(-mLineSize + xO), p(i), p(+mLineSize + xO), p(i)); //Allong Y-axis
                        gx.DrawLine(XPens.Black, p(i), p(-mLineSize + yO), p(i), p(+mLineSize + yO)); //Allong X-axis
                    }
                }
                else if (i % 10 == 0)
                {
                    //draw small lines
                    if (g != null)
                    {
                        g.DrawLine(Pens.Black, new Point(-sLineSize + xO, i), new Point(+sLineSize + xO, i)); //Allong Y-axis
                        g.DrawLine(Pens.Black, new Point(i, -sLineSize + yO), new Point(i, +sLineSize + yO)); //Allong X-axis
                    }
                    else
                    {
                        gx.DrawLine(XPens.Black, new Point(-sLineSize + xO, i), new Point(+sLineSize + xO, i)); //Allong Y-axis
                        gx.DrawLine(XPens.Black, p(i), p(-sLineSize + yO), p(i), p(+sLineSize + yO)); //Allong X-axis
                    }
                }
                else if (i % 2 == 0)
                {
                    //draw dots
                    if (g != null)
                    {
                        g.DrawLine(Pens.Black, new Point(-1 + xO, i), new Point(+1 + xO, i)); //Allong Y-axis
                        g.DrawLine(Pens.Black, new Point(i, -1 + yO), new Point(i, +1 + yO)); //Allong X-axis
                    }
                    else
                    {
                        gx.DrawLine(XPens.Black, p(-1 + xO), p(i), p(+1 + xO), p(i)); //Allong Y-axis
                        gx.DrawLine(XPens.Black, p(i), p(-1 + yO), p(i), p(+1 + yO)); //Allong X-axis
                    }
                }
            } //end i -100=>2000
        } //end pass

        //infoBlock
        var settings = new PrinterSettings();
        if (g != null)
        {
            g.FillRectangle(Brushes.White, 110, 110, 480, 100);
            g.DrawRectangle(Pens.Black, 110, 110, 480, 100);
            g.DrawString("Sheet Height = " + sheet.HeightPage.ToString(), font, Brushes.Black, 112, 112);
            g.DrawString("Sheet Width = " + sheet.WidthPage.ToString(), font, Brushes.Black, 112, 124); //12px per line
            g.DrawString("DefaultPrinter = " + settings.PrinterName, font, Brushes.Black, 112, 136);
            g.DrawString("HardMarginX = " + e.PageSettings.HardMarginX, font, Brushes.Black, 112, 148);
            g.DrawString("HardMarginY = " + e.PageSettings.HardMarginY, font, Brushes.Black, 112, 160);
        }
        else
        {
            gx.DrawRectangle(XPens.Black, Brushes.White, p(110), p(110), p(480), p(100));
            gx.DrawRectangle(XPens.Black, p(110), p(110), p(480), p(100));
            gx.DrawString("Sheet Height = " + sheet.HeightPage.ToString(), xfont, XBrushes.Black, p(112), p(112));
            gx.DrawString("Sheet Width = " + sheet.WidthPage.ToString(), xfont, XBrushes.Black, p(112), p(124)); //12px per line
            gx.DrawString("DefaultPrinter = " + settings.PrinterName, xfont, XBrushes.Black, p(112), p(136));
            gx.DrawString("HardMarginX = " + settings.DefaultPageSettings.HardMarginX, xfont, XBrushes.Black, p(112), p(148));
            gx.DrawString("HardMarginY = " + settings.DefaultPageSettings.HardMarginY, xfont, XBrushes.Black, p(112), p(160));
            gx.DrawString("PDF TrimMargins ^v<> = " + page.TrimMargins.Top + "," + page.TrimMargins.Bottom + "," + page.TrimMargins.Left + "," + page.TrimMargins.Right, xfont, XBrushes.Black, p(112), p(172));
        }

        font.Dispose();
        font = null;
        xfont = null;
    }

    private void pd_DrawFieldsHelper(Sheet sheet, Graphics g, XGraphics gx, Sheet parentSheet = null)
    {
        //Begin drawing.
        foreach (var field in sheet.SheetFields)
        {
            if (parentSheet != null && !FieldOnCurPageHelper(field, parentSheet, PrintMargin, _yPosPrint, _pagesPrinted))
            {
                continue;
            }
            else if (parentSheet == null && !FieldOnCurPageHelper(field, sheet, PrintMargin, _yPosPrint, _pagesPrinted))
            {
                continue;
            }

            switch (field.FieldType)
            {
                case SheetFieldType.Image:
                case SheetFieldType.PatImage:
                    DrawFieldImage(field, g, gx);
                    break;
                case SheetFieldType.Drawing:
                    DrawFieldDrawing(field, g, gx);
                    break;
                case SheetFieldType.Rectangle:
                    DrawFieldRectangle(field, g, gx, _yPosPrint);
                    break;
                case SheetFieldType.Line:
                    DrawFieldLine(field, g, gx);
                    break;
                case SheetFieldType.Special:
                    SheetPrinting.DrawFieldSpecial(sheet, field, g, gx, _yPosPrint);
                    break;
                case SheetFieldType.Grid:
                    DrawFieldGrid(field, sheet, g, gx, _dataSet, _stmt, true);
                    break;
                case SheetFieldType.InputField:
                case SheetFieldType.OutputText:
                case SheetFieldType.StaticText:
                    DrawFieldText(field, sheet, g, gx, _yPosPrint);
                    break;
                case SheetFieldType.CheckBox:
                    DrawFieldCheckBox(field, g, gx);
                    break;
                case SheetFieldType.ComboBox:
                    DrawFieldComboBox(field, sheet, g, gx);
                    break;
                case SheetFieldType.SigBox:
                case SheetFieldType.SigBoxPractice:
                    DrawFieldSigBox(field, sheet, g, gx);
                    break;
                default:
                    //Parameter or possibly new field type.
                    break;
            }
        } //end foreach SheetField
    }

    public static bool FieldOnCurPageHelper(SheetField field, Sheet sheet, Margins margins, int topOfPrintableArea, int pagesPrinted)
    {
        //Even though _printMargins and _yPosPrint are available in this context they are passed in so for future compatibility with webforms.
        var pageCount = Sheets.CalculatePageCount(sheet, margins);
        var bottomOfPrintableArea = topOfPrintableArea + sheet.HeightPage - margins.Bottom;
        if (field.YPos >= bottomOfPrintableArea && pagesPrinted < pageCount - 1)
        {
            return false; //field is entirely on one of the next pages. Unless we are on the first or last page, then it could be in the bottom margin.
        }

        if (field.Bounds.Bottom - margins.Top <= topOfPrintableArea && pagesPrinted > 0)
        {
            return false; //field is entirely on one of the previous pages. Unless we are on the first page, then it is in the top margin.
        }

        return true; //field is all or partially on current page.
    }

    private void FilterColumnsHelper(Sheet sheet, SheetField field, List<DisplayField> Columns)
    {
        switch (sheet.SheetType + "." + field.FieldName)
        {
            case "TreatmentPlan.TreatPlanMain":
                bool checkShowDiscount;
                bool checkShowFees;
                bool checkShowIns;
                try
                {
                    checkShowDiscount = (bool) SheetParameter.GetParamByName(sheet.Parameters, "checkShowDiscount").ParamValue;
                    checkShowFees = (bool) SheetParameter.GetParamByName(sheet.Parameters, "checkShowFees").ParamValue;
                    checkShowIns = (bool) SheetParameter.GetParamByName(sheet.Parameters, "checkShowIns").ParamValue;
                }
                catch
                {
                    //if unable to find any assume default values of true
                    checkShowDiscount = true;
                    checkShowFees = true;
                    checkShowIns = true;
                }

                if (!checkShowFees)
                {
                    Columns.RemoveAll(x => x.InternalName == "Fee");
                }

                if (!checkShowIns)
                {
                    Columns.RemoveAll(x => x.InternalName.In("Pri Ins", "Sec Ins", "DPlan", "Allowed"));
                }

                if (!checkShowDiscount)
                {
                    Columns.RemoveAll(x => x.InternalName == "Discount");
                }

                if (!checkShowIns && !checkShowDiscount)
                {
                    Columns.RemoveAll(x => x.InternalName == "Pat");
                }

                //recenters the GridColumnStylesCollection on the page.
                field.XPos = (sheet.WidthPage - Columns.Sum(x => x.ColumnWidth)) / 2;
                break;
            case "Statement.StatementAging":
                Statement stmt = null;
                try
                {
                    stmt = (Statement) SheetParameter.GetParamByName(sheet.Parameters, "Statement").ParamValue;
                }
                catch (Exception)
                {
                }

                if (sheet.SheetType == SheetTypeEnum.Statement && stmt != null && stmt.SuperFamily == 0)
                {
                    Columns.RemoveAll(x => x.InternalName == "AcctTotal" || x.InternalName == "Account");
                    Columns.RemoveAll(x => x.InternalName == "PatNum");
                }

                break;
        }
    }

    private static Bitmap GetSigPPHelper(Sheet sheet, SheetField field)
    {
        var payPlan = (PayPlan) SheetParameter.GetParamByName(sheet.Parameters, "payplan").ParamValue;
        var keyData = (string) SheetParameter.GetParamByName(sheet.Parameters, "keyData").ParamValue;
        if (payPlan.Signature != "")
        {
            using var sigBoxWrapper = new SignatureBoxWrapper();
            sigBoxWrapper.FillSignature(payPlan.SigIsTopaz, keyData, payPlan.Signature);
            if (sigBoxWrapper.GetNumberOfTabletPoints(payPlan.SigIsTopaz) != 0)
            {
                return sigBoxWrapper.GetSigImage();
            }
        }

        return new Bitmap(field.Width, field.Height); //return blank image if sig invalid.
    }

    private static Bitmap GetSigTPHelper(Sheet sheet, SheetField field)
    {
        var treatPlan = (TreatPlan) SheetParameter.GetParamByName(sheet.Parameters, "TreatPlan").ParamValue;
        var sigIsTopaz = treatPlan.SigIsTopaz;
        if (field.FieldType == SheetFieldType.SigBox && treatPlan.Signature != "")
        {
            using var sigBoxWrapper = new SignatureBoxWrapper();
            sigBoxWrapper.SignatureMode = SignatureBoxWrapper.SigMode.TreatPlan;
            var keyData = TreatPlans.GetKeyDataForSignatureHash(treatPlan, treatPlan.ListProcTPs);
            sigBoxWrapper.FillSignature(sigIsTopaz, keyData, treatPlan.Signature);
            //There are two signature boxes and only one SigIsTopaz column.
            //The patient and the practice could have signed the treatment plan using different mediums so attempt to load both just in case.
            if (!sigBoxWrapper.IsValid)
            {
                sigIsTopaz = !sigIsTopaz;
                sigBoxWrapper.FillSignature(sigIsTopaz, keyData, treatPlan.Signature);
            }

            if (sigBoxWrapper.GetNumberOfTabletPoints(sigIsTopaz) != 0)
            {
                return sigBoxWrapper.GetSigImage();
            }
        }
        else if (field.FieldType == SheetFieldType.SigBoxPractice && treatPlan.SignaturePractice != "")
        {
            using var sigBoxWrapper = new SignatureBoxWrapper();
            sigBoxWrapper.SignatureMode = SignatureBoxWrapper.SigMode.TreatPlan;
            var keyData = TreatPlans.GetKeyDataForSignatureHash(treatPlan, treatPlan.ListProcTPs);
            sigBoxWrapper.FillSignature(sigIsTopaz, keyData, treatPlan.SignaturePractice);
            //There are two signature boxes and only one SigIsTopaz column.
            //The patient and the practice could have signed the treatment plan using different mediums so attempt to load both just in case.
            if (!sigBoxWrapper.IsValid)
            {
                sigIsTopaz = !sigIsTopaz;
                sigBoxWrapper.FillSignature(sigIsTopaz, keyData, treatPlan.SignaturePractice);
            }

            if (sigBoxWrapper.GetNumberOfTabletPoints(sigIsTopaz) != 0)
            {
                return sigBoxWrapper.GetSigImage();
            }
        }

        return new Bitmap(field.Width, field.Height);
    }

    private static double p(int pixels)
    {
        var xunit = XUnit.FromInch((double) pixels / 100d); //100 ppi
        return xunit.Point;
        //XUnit.FromInch((double)pixels/100);
    }

    private static double p(float pixels)
    {
        var xunit = XUnit.FromInch((double) pixels / 100d); //100 ppi
        return xunit.Point;
    }
}