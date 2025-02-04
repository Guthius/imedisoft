using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Chart;
using OpenDentBusiness;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace OpenDental;

public class SheetPrinting
{
    public static Margins PrintMargin { get; } = new(0, 0, 40, 60);

    public static void Print(Sheet sheet, int copies = 1, bool isRxControlled = false, Statement stmt = null, bool isPrintDocument = true, bool isPreviewMode = false, bool isPrintRemote = false, long printerNumOverride = 0)
    {
        var sheetPrintingJob = new SheetPrintingJob {IsRemotePrintingJob = isPrintRemote, PrinterNumOverride = printerNumOverride};
        sheetPrintingJob.Print(sheet, copies, stmt, isPreviewMode);
    }

    public static void Print(Sheet sheet, DataSet dataSet, int copies = 1, bool isRxControlled = false, Statement stmt = null, bool isPrintRemote = false, long printerNumOverride = 0)
    {
        var sheetPrintingJob = new SheetPrintingJob {IsRemotePrintingJob = isPrintRemote, PrinterNumOverride = printerNumOverride};
        sheetPrintingJob.Print(sheet, dataSet, copies, stmt);
    }

    public static void PrintBatch(List<Sheet> sheetBatch)
    {
        var sheetPrintingJob = new SheetPrintingJob();
        sheetPrintingJob.PrintBatch(sheetBatch);
    }

    public static void DrawFieldGrid(SheetField field, Sheet sheet, Graphics g, XGraphics gx, DataSet dataSet, Statement stmt, bool isPrinting = false, Patient pat = null, Patient patGuar = null, float scaleMS = 1)
    {
        var sheetDrawingJob = new SheetDrawingJob();
        sheetDrawingJob.DrawFieldGrid(field, sheet, g, gx, dataSet, stmt, isPrinting, pat, patGuar, scaleMS);
    }

    public static void DrawProcsGraphics(List<Procedure> procList, ToothChartRelay toothChartRelay, List<ToothInitial> toothInitialList, bool isInPatientDashboard, Patient patCur = null, List<Appointment> listAppts = null)
    {
        //this method must also stay in OD.exe
        Procedure proc;
        string[] teeth;
        var listCurProvNums = toothChartRelay.GetPertinentProvNumsForToothColorPref(Security.CurUser, patCur, listAppts);
        for (var i = 0; i < procList.Count; i++)
        {
            proc = procList[i];

            if (proc.HideGraphics)
            {
                continue;
            }

            if (ProcedureCodes.GetProcCode(proc.CodeNum).PaintType == ToothPaintingType.Extraction && (
                    proc.ProcStatus == ProcStat.C
                    || proc.ProcStatus == ProcStat.EC
                    || proc.ProcStatus == ProcStat.EO
                ))
            {
                continue; //prevents the red X. Missing teeth already handled.
            }

            var procCode = ProcedureCodes.GetProcCode(proc.CodeNum);
            var doApplyColorPref = isInPatientDashboard && toothChartRelay.DoesToothColorPrefApply(listCurProvNums, proc.ProvNum);
            toothChartRelay.GetToothColors(procCode, proc.ProcStatus, doApplyColorPref, out var cDark, out var cLight);
            switch (ProcedureCodes.GetProcCode(proc.CodeNum).PaintType)
            {
                case ToothPaintingType.BridgeDark:
                    if (ToothInitials.ToothIsMissingOrHidden(toothInitialList, proc.ToothNum))
                    {
                        toothChartRelay.SetPontic(proc.ToothNum, cDark);
                    }
                    else
                    {
                        toothChartRelay.SetCrown(proc.ToothNum, cDark);
                    }

                    break;
                case ToothPaintingType.BridgeLight:
                    if (ToothInitials.ToothIsMissingOrHidden(toothInitialList, proc.ToothNum))
                    {
                        toothChartRelay.SetPontic(proc.ToothNum, cLight);
                    }
                    else
                    {
                        toothChartRelay.SetCrown(proc.ToothNum, cLight);
                    }

                    break;
                case ToothPaintingType.CrownDark:
                    toothChartRelay.SetCrown(proc.ToothNum, cDark);
                    break;
                case ToothPaintingType.CrownLight:
                    toothChartRelay.SetCrown(proc.ToothNum, cLight);
                    break;
                case ToothPaintingType.DentureDark:
                    if (proc.Surf == "U")
                    {
                        teeth = new string[14];
                        for (var t = 0; t < 14; t++)
                        {
                            teeth[t] = (t + 2).ToString();
                        }
                    }
                    else if (proc.Surf == "L")
                    {
                        teeth = new string[14];
                        for (var t = 0; t < 14; t++)
                        {
                            teeth[t] = (t + 18).ToString();
                        }
                    }
                    else
                    {
                        teeth = proc.ToothRange.Split(',');
                    }

                    for (var t = 0; t < teeth.Length; t++)
                    {
                        if (ToothInitials.ToothIsMissingOrHidden(toothInitialList, teeth[t]))
                        {
                            toothChartRelay.SetPontic(teeth[t], cDark);
                        }
                        else
                        {
                            toothChartRelay.SetCrown(teeth[t], cDark);
                        }
                    }

                    break;
                case ToothPaintingType.DentureLight:
                    if (proc.Surf == "U")
                    {
                        teeth = new string[14];
                        for (var t = 0; t < 14; t++)
                        {
                            teeth[t] = (t + 2).ToString();
                        }
                    }
                    else if (proc.Surf == "L")
                    {
                        teeth = new string[14];
                        for (var t = 0; t < 14; t++)
                        {
                            teeth[t] = (t + 18).ToString();
                        }
                    }
                    else
                    {
                        teeth = proc.ToothRange.Split(',');
                    }

                    for (var t = 0; t < teeth.Length; t++)
                    {
                        if (ToothInitials.ToothIsMissingOrHidden(toothInitialList, teeth[t]))
                        {
                            toothChartRelay.SetPontic(teeth[t], cLight);
                        }
                        else
                        {
                            toothChartRelay.SetCrown(teeth[t], cLight);
                        }
                    }

                    break;
                case ToothPaintingType.Extraction:
                    toothChartRelay.SetBigX(proc.ToothNum, cDark);
                    break;
                case ToothPaintingType.FillingDark:
                    toothChartRelay.SetSurfaceColors(proc.ToothNum, proc.Surf, cDark);
                    break;
                case ToothPaintingType.FillingLight:
                    toothChartRelay.SetSurfaceColors(proc.ToothNum, proc.Surf, cLight);
                    break;
                case ToothPaintingType.Implant:
                    toothChartRelay.SetImplant(proc.ToothNum, cDark);
                    break;
                case ToothPaintingType.PostBU:
                    toothChartRelay.SetBu(proc.ToothNum, cDark);
                    break;
                case ToothPaintingType.RCT:
                    toothChartRelay.SetRct(proc.ToothNum, cDark);
                    break;
                case ToothPaintingType.RetainedRoot:
                    toothChartRelay.SetRetainedRoot(proc.ToothNum, cDark);
                    break;
                case ToothPaintingType.Sealant:
                    toothChartRelay.SetSealant(proc.ToothNum, cDark);
                    break;
                case ToothPaintingType.SpaceMaintainer:
                    if (ProcedureCodes.GetProcCode(proc.CodeNum).TreatArea == TreatmentArea.Tooth && proc.ToothNum != "")
                    {
                        toothChartRelay.SetSpaceMaintainer(proc.ToothNum, cDark);
                    }
                    else if ((ProcedureCodes.GetProcCode(proc.CodeNum).TreatArea == TreatmentArea.ToothRange && proc.ToothRange != "")
                             || (ProcedureCodes.GetProcCode(proc.CodeNum).AreaAlsoToothRange && proc.ToothRange != ""))
                    {
                        teeth = proc.ToothRange.Split(',');
                        for (var t = 0; t < teeth.Length; t++)
                        {
                            toothChartRelay.SetSpaceMaintainer(teeth[t], cDark);
                        }
                    }
                    else if (ProcedureCodes.GetProcCode(proc.CodeNum).TreatArea == TreatmentArea.Quad)
                    {
                        teeth = proc.Surf switch
                        {
                            "UR" => ["4", "5", "6", "7", "8"],
                            "UL" => ["9", "10", "11", "12", "13"],
                            "LL" => ["20", "21", "22", "23", "24"],
                            "LR" => ["25", "26", "27", "28", "29"],
                            _ => []
                        };

                        foreach (var tooth in teeth)
                        {
                            toothChartRelay.SetSpaceMaintainer(tooth, cDark);
                        }
                    }

                    break;
                case ToothPaintingType.Text:
                    toothChartRelay.SetText(proc.ToothNum, cDark, ProcedureCodes.GetProcCode(proc.CodeNum).PaintText);
                    break;
                case ToothPaintingType.Veneer:
                    toothChartRelay.SetVeneer(proc.ToothNum, cLight);
                    break;
            }
        }
    }

    public static void DrawOrthoHardware(List<OrthoHardware> listOrthoHardwares, ToothChartRelay toothChartRelay)
    {
        var listOrthoHardwareSpecs = OrthoHardwareSpecs.GetDeepCopy();
        var listOrthoWires = new List<OrthoHardwares.OrthoWire>(); //also used for elastics
        for (var i = 0; i < listOrthoHardwares.Count; i++)
        {
            var orthoHardwareSpec = listOrthoHardwareSpecs.Find(x => x.OrthoHardwareSpecNum == listOrthoHardwares[i].OrthoHardwareSpecNum);
            if (listOrthoHardwares[i].OrthoHardwareType == EnumOrthoHardwareType.Bracket)
            {
                toothChartRelay.SetBracket(listOrthoHardwares[i].ToothRange, orthoHardwareSpec.ItemColor);
            }

            if (listOrthoHardwares[i].OrthoHardwareType == EnumOrthoHardwareType.Wire)
            {
                listOrthoWires.AddRange(OrthoHardwares.GetWires(listOrthoHardwares[i].ToothRange, orthoHardwareSpec.ItemColor));
            }

            if (listOrthoHardwares[i].OrthoHardwareType == EnumOrthoHardwareType.Elastic)
            {
                listOrthoWires.AddRange(OrthoHardwares.GetElastics(listOrthoHardwares[i].ToothRange, orthoHardwareSpec.ItemColor));
            }
        }

        for (var i = 0; i < listOrthoWires.Count; i++)
        {
            if (listOrthoWires[i].EnumOrthoWireType_ == OrthoHardwares.EnumOrthoWireType.BetweenBrackets)
            {
                toothChartRelay.AddOrthoWireBetweenBrackets(listOrthoWires[i].ToothIDstart, listOrthoWires[i].ToothIDend, listOrthoWires[i].ColorDraw);
            }

            if (listOrthoWires[i].EnumOrthoWireType_ == OrthoHardwares.EnumOrthoWireType.InBracket)
            {
                toothChartRelay.AddOrthoWireInBracket(listOrthoWires[i].ToothIDstart, listOrthoWires[i].ColorDraw);
            }

            if (listOrthoWires[i].EnumOrthoWireType_ == OrthoHardwares.EnumOrthoWireType.Elastic)
            {
                toothChartRelay.AddOrthoElastic(listOrthoWires[i].ToothIDstart, listOrthoWires[i].ToothIDend, listOrthoWires[i].ColorDraw);
            }
        }
    }

    public static Image GetToothChartHelper(long patNum, bool showCompleted, TreatPlan treatPlan = null, List<Procedure> listProceduresFilteredOverride = null, bool isInPatientDashboard = false, Patient patCur = null, List<Appointment> listAppts = null)
    {
        //This method is designed to stay in the OD.exe.
        var colorBackgroundIndex = 14;
        var colorTextIndex = 15;
        var width = 500;
        var height = 370;
        if (isInPatientDashboard)
        {
            colorBackgroundIndex = 10;
            colorTextIndex = 11;
            width = 410;
            height = 307;
        }

        Form formOldBitmap = null;
        var toothChartWrapper = new ToothChartWrapper();
        var toothChartRelay = new ToothChartRelay();
        toothChartRelay.SetToothChartWrapper(toothChartWrapper);
        if (ToothChartRelay.IsSparks3DPresent)
        {
            //no control to show
        }
        else
        {
            toothChartWrapper.Size = new Size(width, height);
            toothChartWrapper.UseHardware = ComputerPrefs.LocalComputer.GraphicsUseHardware;
            toothChartWrapper.PreferredPixelFormatNumber = ComputerPrefs.LocalComputer.PreferredPixelFormatNum;
            toothChartWrapper.DeviceFormat = new ToothChartDirectX.DirectXDeviceFormat(ComputerPrefs.LocalComputer.DirectXFormat);
            toothChartWrapper.DrawMode = ComputerPrefs.LocalComputer.GraphicsSimple;
            ComputerPrefs.LocalComputer.PreferredPixelFormatNum = toothChartWrapper.PreferredPixelFormatNumber;
            ComputerPrefs.Update(ComputerPrefs.LocalComputer);
            formOldBitmap = new Form();
            formOldBitmap.FormBorderStyle = FormBorderStyle.None;
            formOldBitmap.Size = new Size(width, height);
            formOldBitmap.Controls.Add(toothChartWrapper);
            //formOldBitmap.Show();
        }

        toothChartRelay.SetToothNumberingNomenclature((ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers));
        var listDefs = Defs.GetDefsForCategory(DefCat.ChartGraphicColors);
        toothChartRelay.ColorBackgroundMain = listDefs[colorBackgroundIndex].ItemColor;
        toothChartRelay.ColorText = listDefs[colorTextIndex].ItemColor;
        toothChartRelay.ResetTeeth();
        toothChartRelay.SetOrthoMode(false);
        var toothInitialList = patNum == 0 ? [] : ToothInitials.GetPatientData(patNum);
        //first, primary.  That way, you can still set a primary tooth missing afterwards.
        for (var i = 0; i < toothInitialList.Count; i++)
        {
            if (toothInitialList[i].InitialType == ToothInitialType.Primary)
            {
                toothChartRelay.SetPrimary(toothInitialList[i].ToothNum);
            }
        }

        for (var i = 0; i < toothInitialList.Count; i++)
        {
            switch (toothInitialList[i].InitialType)
            {
                case ToothInitialType.Missing:
                    toothChartRelay.SetMissing(toothInitialList[i].ToothNum);
                    break;
                case ToothInitialType.Hidden:
                    toothChartRelay.SetHidden(toothInitialList[i].ToothNum);
                    break;
                case ToothInitialType.Rotate:
                    toothChartRelay.MoveTooth(toothInitialList[i].ToothNum, toothInitialList[i].Movement, 0, 0, 0, 0, 0);
                    break;
                case ToothInitialType.TipM:
                    toothChartRelay.MoveTooth(toothInitialList[i].ToothNum, 0, toothInitialList[i].Movement, 0, 0, 0, 0);
                    break;
                case ToothInitialType.TipB:
                    toothChartRelay.MoveTooth(toothInitialList[i].ToothNum, 0, 0, toothInitialList[i].Movement, 0, 0, 0);
                    break;
                case ToothInitialType.ShiftM:
                    toothChartRelay.MoveTooth(toothInitialList[i].ToothNum, 0, 0, 0, toothInitialList[i].Movement, 0, 0);
                    break;
                case ToothInitialType.ShiftO:
                    toothChartRelay.MoveTooth(toothInitialList[i].ToothNum, 0, 0, 0, 0, toothInitialList[i].Movement, 0);
                    break;
                case ToothInitialType.ShiftB:
                    toothChartRelay.MoveTooth(toothInitialList[i].ToothNum, 0, 0, 0, 0, 0, toothInitialList[i].Movement);
                    break;
                case ToothInitialType.Drawing:
                    toothChartRelay.AddDrawingSegment(toothInitialList[i].Copy());
                    break;
                case ToothInitialType.Text:
                    toothChartRelay.AddText(toothInitialList[i].GetTextString(), toothInitialList[i].GetTextPoint(), toothInitialList[i].ColorDraw, toothInitialList[i].ToothInitialNum);
                    break;
            }
        }

        //We passed in a list of procs we want to see on the toothchart.  Use that.
        var listProceduresFiltered = new List<Procedure>();
        if (listProceduresFilteredOverride != null)
        {
            listProceduresFiltered = listProceduresFilteredOverride;
        }
        else if (patNum > 0)
        {
            //Custom list of procedures was not passed in, go get all for the patient like we always have.
            var listProceduresAll = Procedures.Refresh(patNum);
            listProceduresFiltered = OpenDentBusiness.SheetPrinting.FilterProceduresForToothChart(listProceduresAll, treatPlan, showCompleted);
        }

        listProceduresFiltered.Sort(OpenDentBusiness.SheetPrinting.CompareProcListFiltered);
        //Draw tooth chart
        DrawProcsGraphics(listProceduresFiltered, toothChartRelay, toothInitialList, isInPatientDashboard, patCur, listAppts);
        var listOrthoHardwares = OrthoHardwares.GetPatientData(patNum);
        DrawOrthoHardware(listOrthoHardwares, toothChartRelay);
        if (!ToothChartRelay.IsSparks3DPresent)
        {
            toothChartWrapper.AutoFinish = true;
        }

        toothChartRelay.EndUpdate();
        if (ToothChartRelay.IsSparks3DPresent)
        {
            Image retVal = toothChartRelay.GetBitmap();
            toothChartRelay.DisposeControl();
            return retVal;
        }
        else
        {
            Image retVal = toothChartRelay.GetBitmap();
            formOldBitmap.Close(); //automatically disposes, too
            return retVal;
        }
    }

    public static PdfDocument CreatePdf(Sheet sheet, string fullFileName = null, Statement stmt = null, DataSet dataSet = null, Patient pat = null, Patient patGuar = null, bool doSave = true)
    {
        var sheetDrawingJob = new SheetDrawingJob();
        var pdf = sheetDrawingJob.CreatePdf(sheet, stmt, dataSet, pat, patGuar);
        if (doSave)
        {
            SavePdfToFile(pdf, fullFileName);
        }

        return pdf;
    }

    public static void SavePdfToFile(PdfDocument pdf, string fileNameAndPath)
    {
        try
        {
            SheetDrawingJob.SavePdfToFile(pdf, fileNameAndPath);
        }
        catch (Exception ex)
        {
            FriendlyException.Show(Lans.g("PdfDocument", "An error has occurred while trying to create this document"), ex, true);
            return;
        }
    }

    public static int CreatePdfPage(Sheet sheet, PdfPage page, DataSet dataSet, Patient pat, Patient patGuar, int yPos, int pagesPrinted)
    {
        var sheetDrawingJob = new SheetDrawingJob();
        return sheetDrawingJob.CreatePdfPage(sheet, page, dataSet, pat, patGuar, yPos, pagesPrinted);
    }
}