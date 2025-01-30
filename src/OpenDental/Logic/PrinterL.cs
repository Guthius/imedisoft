using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Logic;

public class PrinterL
{
    public static bool HasComputerTable = true;

    public static PrintPreviewControl PrintPreviewControlOverride;

    public static ODprintout CreateODprintout(PrintPageEventHandler printPageEventHandler = null, string auditDescription = "", PrintSituation printSituation = PrintSituation.Default, long auditPatNum = 0, Margins margins = null, PrintoutOrigin printoutOrigin = PrintoutOrigin.Default, PaperSize paperSize = null, int totalPages = 1, PrintoutOrientation printoutOrientation = PrintoutOrientation.Default, Duplex duplex = Duplex.Default, short copies = 1, bool isErrorSuppressed = false)
    {
        var printout = new ODprintout(
            printPageEventHandler,
            printSituation,
            auditPatNum,
            auditDescription,
            margins,
            printoutOrigin,
            paperSize,
            printoutOrientation,
            duplex,
            copies,
            totalPages);
        
        if (!isErrorSuppressed && printout.SettingsErrorCode != PrintoutErrorCode.Success)
        {
            ShowError(printout);
        }

        return printout;
    }

    public static bool TryPrintOrDebugRpPreview(PrintPageEventHandler printPageEventHandler, string auditDescription, PrintoutOrientation printoutOrientation = PrintoutOrientation.Portrait, PrintSituation printSituation = PrintSituation.Default, long auditPatNum = 0, Margins margins = null, PrintoutOrigin printoutOrigin = PrintoutOrigin.Default, bool isForcedPreview = false)
    {
        var printout = new ODprintout(
            printPageEventHandler,
            printSituation,
            auditPatNum,
            auditDescription,
            margins,
            printoutOrigin,
            printoutOrientation: printoutOrientation,
            duplex: Duplex.Default
        );
        
        return isForcedPreview ? RpPreview(printout) : TryPrint(printout);
    }
    
    public static bool TryPrintOrDebugClassicPreview(PrintPageEventHandler printPageEventHandler, string auditDescription, Margins margins = null, int totalPages = 1, PrintSituation printSituation = PrintSituation.Default, PrintoutOrigin printoutOrigin = PrintoutOrigin.Default, PrintoutOrientation printoutOrientation = PrintoutOrientation.Default, bool isForcedPreview = false, long auditPatNum = 0, PaperSize paperSize = null, bool isRemotePrint = false, long printerNumOverride = 0)
    {
        var printout = new ODprintout(
            printPageEventHandler,
            printSituation,
            auditPatNum,
            auditDescription,
            margins,
            printoutOrigin,
            paperSize,
            printoutOrientation,
            totalPages: totalPages
        );
        
        if (isForcedPreview && !isRemotePrint)
        {
            return PreviewClassic(printout);
        }

        return TryPrint(printout, isRemotePrint: isRemotePrint, printerNumOverride: printerNumOverride);
    }
    
    public static bool TryPrint(PrintPageEventHandler printPageEventHandler, string auditDescription = "", long auditPatNum = 0, PrintSituation printSituation = PrintSituation.Default, Margins margins = null, PrintoutOrigin printoutOrigin = PrintoutOrigin.Default, PrintoutOrientation printoutOrientation = PrintoutOrientation.Default, Duplex duplex = Duplex.Default)
    {
        var printout = new ODprintout(
            printPageEventHandler,
            printSituation,
            auditPatNum,
            auditDescription,
            margins,
            printoutOrigin,
            printoutOrientation: printoutOrientation,
            duplex: duplex
        );
        return TryPrint(printout);
    }

    public static bool TryPrint(ODprintout printout, bool isRemotePrint = false, long printerNumOverride = 0)
    {
        if (!printout.HasValidSettings())
        {
            MsgBox.Show(printout.SettingsErrorCode.GetDescription());
            return false;
        }

        if (!TrySetPrinter(printout, isRemotePrint: isRemotePrint, printerNumOverride: printerNumOverride))
        {
            return false;
        }

        return isRemotePrint ? printout.TryPrintNoUI() : printout.TryPrint();
    }

    public static bool TryPreview(PrintPageEventHandler printPageEventHandler, string auditDescription, PrintSituation printSituation = PrintSituation.Default, Margins margins = null, PrintoutOrigin printoutOrigin = PrintoutOrigin.Default, PaperSize paperSize = null, PrintoutOrientation printoutOrientation = PrintoutOrientation.Default, int totalPages = 1, bool doCalculateTotalPages = false)
    {
        var printout = new ODprintout(
            printPageEventHandler,
            printSituation,
            auditDescription: auditDescription,
            margins: margins,
            printoutOrigin: printoutOrigin,
            paperSize: paperSize,
            printoutOrientation: printoutOrientation,
            totalPages: totalPages
        );

        if (!doCalculateTotalPages)
        {
            return PreviewClassic(printout);
        }
        
        printout.TotalPages = 0;
        printout.PrintDoc.PrintPage += (_, _) => printout.TotalPages++;

        return PreviewClassic(printout);
    }
    
    public static bool TrySetPrinter(ODprintout printout, bool isRemotePrint = false, long printerNumOverride = 0)
    {
        if (printout.SettingsErrorCode == PrintoutErrorCode.Success || isRemotePrint)
        {
            return SetPrinter(printout.PrintDoc.PrinterSettings, printout.Situation, printout.AuditPatNum, printout.AuditDescription, isRemotePrint, printerNumOverride: printerNumOverride);
        }
        
        ShowError(printout);
        return false;

    }

    public static bool SetPrinter(PrintDocument printDocument, PrintSituation printSituation, long patNum, string auditDescription)
    {
        return SetPrinter(printDocument.PrinterSettings, printSituation, patNum, auditDescription, false);
    }

    private static bool SetPrinter(PrinterSettings printerSettings, PrintSituation printSituation, long patNum, string auditDescription, bool isRemotePrint, long printerNumOverride = 0)
    {
        if (!HasComputerTable)
        {
            return true;
        }

        var serviceControllerArray = ServiceController.GetServices();
        var serviceController = serviceControllerArray.FirstOrDefault(x => x.ServiceName == "Spooler");
        
        if (serviceController is not {Status: ServiceControllerStatus.Running})
        {
            MsgBox.Show("Please start the Printer Spooler service to proceed with printing.");
            return false;
        }

        if (printerNumOverride != 0)
        {
            var printerOverride = Printers.GetFirstOrDefault(x => x.PrinterNum == printerNumOverride);
            var printFileName = GetFilePrinterPath(printerOverride);
            
            if (Printers.PrinterIsInstalled(printerOverride.PrinterName))
            {
                printerSettings.PrinterName = printerOverride.PrinterName;
            }

            if (printerOverride.IsVirtualPrinter && !string.IsNullOrWhiteSpace(printFileName))
            {
                printerSettings.PrintFileName = printFileName;
                printerSettings.PrintToFile = printerOverride.IsVirtualPrinter;
            }

            var isValidPrintConfiguration = Printers.PrinterIsInstalled(printerOverride.PrinterName) && (!printerOverride.IsVirtualPrinter || !string.IsNullOrWhiteSpace(printFileName));
            if (isRemotePrint && !isValidPrintConfiguration)
            {
                throw new ApplicationException("Invalid printer configuration.");
            }

            return isValidPrintConfiguration;
        }
        
        string printerName;
        
        var showPrompt = false;
        
        var printerForSit = Printers.GetForSit(PrintSituation.Default); 
        if (printerForSit is not null)
        {
            printerName = printerForSit.PrinterName;
            showPrompt = printerForSit.DisplayPrompt;
            if (Printers.PrinterIsInstalled(printerName))
            {
                printerSettings.PrinterName = printerName;
            }
        }
        
        if (printSituation != PrintSituation.Default)
        {
            var printerForSpecificSit = Printers.GetForSit(printSituation);
            if (printerForSpecificSit is not null)
            {
                printerForSit = printerForSpecificSit;
                printerName = printerForSit.PrinterName;
                
                showPrompt = printerForSit.DisplayPrompt;
                
                if (Printers.PrinterIsInstalled(printerName))
                {
                    printerSettings.PrinterName = printerName;
                }
            }
        }
        
        if (printerForSit is {IsVirtualPrinter: true})
        {
            var printerFilePath = GetFilePrinterPath(printerForSit);
            if (string.IsNullOrWhiteSpace(printerFilePath) && !isRemotePrint)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(printerFilePath) && isRemotePrint)
            {
                throw new ApplicationException("Invalid printer configuration.");
            }

            printerSettings.PrintToFile = printerForSit.IsVirtualPrinter;
            printerSettings.PrintFileName = printerFilePath;
        }
        
        if (showPrompt && !isRemotePrint)
        {
            var printDialog = new PrintDialog();
            
            printDialog.AllowSomePages = true;
            printDialog.PrinterSettings = printerSettings;
            
            var dialogResult = printDialog.ShowDialog();
            
            printerSettings.Collate = true;
            
            if (dialogResult != DialogResult.OK)
            {
                return false;
            }

            if (printerSettings.PrintRange != PrintRange.AllPages && printerSettings.ToPage < 1)
            {
                return false;
            }
        }
        
        if (!string.IsNullOrEmpty(auditDescription))
        {
            SecurityLogs.MakeLogEntry(EnumPermType.Printing, patNum, auditDescription);
        }

        return true;
    }

    public static string GetFilePrinterPath(Printer printer)
    {
        if (!printer.IsVirtualPrinter)
        {
            return "";
        }

        var aToZFullPath = ODFileUtils.RemoveTrailingSeparators(ImageStore.GetDataFolder());
        
        return Path.Combine(aToZFullPath, DateTime.Now.ToString("MM_dd_yy_H_mm_ss_fff") + "." + printer.FileExtension);
    }

    public static string GetErrorStringFromCode(PrintoutErrorCode printoutErrorCode)
    {
        var message = printoutErrorCode.GetDescription();
        
        if (printoutErrorCode != PrintoutErrorCode.Success)
        {
            message += "\r\nIf you do have a printer installed, restarting the workstation may solve the problem.";
        }

        return message;
    }

    public static void ShowError(ODprintout printout)
    {
        ShowError(GetErrorStringFromCode(printout.SettingsErrorCode), printout.ErrorEx);
    }
    
    private static void ShowError(string msgOverride = "", Exception ex = null)
    {
        var message = "There was an error while trying to print.";
        if (!string.IsNullOrEmpty(msgOverride))
        {
            message = msgOverride;
        }

        if (ex != null)
        {
            message += "\r\n" + ex.Message;
        }

        ODMessageBox.Show(message);
    }

    private static bool IsControlPreviewOverrideValid(ODprintout printout)
    {
        if (printout.SettingsErrorCode != PrintoutErrorCode.Success)
        {
            ShowError(printout);
            return false;
        }

        PrintPreviewControlOverride.Document = printout.PrintDoc;
        PrintPreviewControlOverride = null;
        
        return true;
    }

    private static bool RpPreview(ODprintout printout)
    {
        if (PrintPreviewControlOverride != null)
        {
            return IsControlPreviewOverrideValid(printout);
        }

        if (!printout.HasValidSettings())
        {
            MsgBox.Show(printout.SettingsErrorCode.GetDescription());
            return false;
        }

        var formRpPrintPreview = new FormRpPrintPreview(printout);
        
        formRpPrintPreview.ShowDialog();
        formRpPrintPreview.BringToFront();
        
        return formRpPrintPreview.DialogResult == DialogResult.OK;
    }

    private static bool PreviewClassic(ODprintout printout)
    {
        if (!printout.HasValidSettings())
        {
            MsgBox.Show(printout.SettingsErrorCode.GetDescription());
            return false;
        }

        if (PrintPreviewControlOverride != null)
        {
            return IsControlPreviewOverrideValid(printout);
        }

        MakeMarginsFitWithinHardMargins(printout.PrintDoc);
        
        using var formPrintPreview = new FormPrintPreview(printout);
        
        formPrintPreview.ShowDialog();
        formPrintPreview.BringToFront();
        
        return formPrintPreview.DialogResult == DialogResult.OK;
    }
    
    private static void MakeMarginsFitWithinHardMargins(PrintDocument printDocument)
    {
        var marginsDefault = printDocument.DefaultPageSettings.Margins;
        var hardMarginX = (int) printDocument.DefaultPageSettings.HardMarginX;
        var hardMarginY = (int) printDocument.DefaultPageSettings.HardMarginY;
        var marginsHard = new Margins(hardMarginX, hardMarginX, hardMarginY, hardMarginY);
        
        if (marginsDefault.Bottom < marginsHard.Bottom)
        {
            marginsDefault.Bottom = marginsHard.Bottom;
        }

        if (marginsDefault.Top < marginsHard.Top)
        {
            marginsDefault.Top = marginsHard.Top;
        }

        if (marginsDefault.Left < marginsHard.Left)
        {
            marginsDefault.Left = marginsHard.Left;
        }

        if (marginsDefault.Right < marginsHard.Right)
        {
            marginsDefault.Right = marginsHard.Right;
        }
    }

  
    public static bool HasValidSettings()
    {
        var odPrintout = new ODprintout();
        
        if (odPrintout.HasValidSettings())
        {
            return true;
        }
        
        MsgBox.Show(odPrintout.SettingsErrorCode.GetDescription());
        return false;
    }
}