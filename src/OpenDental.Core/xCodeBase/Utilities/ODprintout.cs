using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

namespace CodeBase;

public class ODprintout
{
    public readonly PrintSituation Situation;
    public int TotalPages;
    public readonly string AuditDescription;
    public readonly long AuditPatNum;
    public PrintoutErrorCode SettingsErrorCode = PrintoutErrorCode.Success;
    public Exception ErrorEx;

    public static int InvalidMinDefaultPageHeight;
    public static int InvalidMinDefaultPageWidth = -1;
    public static ODprintout CurPrintout;

    public bool HasValidSettings()
    {
        var serviceControllerArray = ServiceController.GetServices();
        var serviceControllerPrintSpooler = serviceControllerArray.FirstOrDefault(x => x.ServiceName == "Spooler");
        if (serviceControllerPrintSpooler is not {Status: ServiceControllerStatus.Running})
        {
            SettingsErrorCode = PrintoutErrorCode.InactivePrintSpoolerService;
            return false;
        }

        try
        {
            SettingsErrorCode = PrintoutErrorCode.Success;
            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                SettingsErrorCode = PrintoutErrorCode.NoInstalledPrinter;
                return false;
            }

            if (PrintDoc.PrinterSettings == null)
            {
                //Should not happen
                SettingsErrorCode = PrintoutErrorCode.PrinterSettingsNotFound;
                return false;
            }

            if (!PrintDoc.PrinterSettings.IsValid)
            {
                SettingsErrorCode = PrintoutErrorCode.InvalidPrinterSettings;
                return false;
            }
            
            if (PrintDoc.DefaultPageSettings.PrintableArea.Width == 0 || PrintDoc.DefaultPageSettings.PrintableArea.Height == 0)
            {
                //At least one valid printer is installed.
            }
        }
        catch (InvalidPrinterException ex)
        {
            SettingsErrorCode = PrintoutErrorCode.InvalidPrinterSettings;
            ErrorEx = ex;
            return false;
        }
        catch (Win32Exception wex)
        {
            SettingsErrorCode = PrintoutErrorCode.PrinterConnectionError;
            ErrorEx = wex;
            return false;
        }

        return true;
    }
    
    public PrintDocument PrintDoc { get; }

    public static PaperSize GetDefaultPaperSize()
    {
        try
        {
            return (new PrintDocument().DefaultPageSettings.PaperSize);
        }
        catch
        {
            return new PaperSize("default", 850, 1100);
        }
    }

    public ODprintout(PrintPageEventHandler printPageEventHandler = null, PrintSituation printSit = PrintSituation.Default, long auditPatNum = 0, string auditDescription = "", Margins margins = null, PrintoutOrigin printoutOrigin = PrintoutOrigin.Default, PaperSize paperSize = null, PrintoutOrientation printoutOrientation = PrintoutOrientation.Default, Duplex duplex = Duplex.Default, short copies = 1, int totalPages = 1) : base()
    {
        CurPrintout = this;
        PrintDoc = new PrintDocument();
        Situation = printSit;
        AuditPatNum = auditPatNum;
        AuditDescription = auditDescription;
        TotalPages = totalPages;
        if (!HasValidSettings())
        {
            return;
        }

        PrintDoc.PrintPage += printPageEventHandler;
        if (printoutOrientation != PrintoutOrientation.Default)
        {
            PrintDoc.DefaultPageSettings.Landscape = (printoutOrientation == PrintoutOrientation.Landscape) ? true : false;
        }

        if (printoutOrigin != PrintoutOrigin.Default)
        {
            PrintDoc.OriginAtMargins = (printoutOrigin == PrintoutOrigin.AtMargin) ? true : false;
        }

        PrintDoc.DefaultPageSettings.Margins = (margins ?? new Margins(25, 25, 40, 40));
        if (paperSize == null)
        {
            //This prevents a bug caused by some printer drivers not reporting their papersize.
            //But remember that other countries use A4 paper instead of 8 1/2 x 11.
            if ((InvalidMinDefaultPageWidth != -1 && PrintDoc.DefaultPageSettings.PrintableArea.Width <= InvalidMinDefaultPageWidth)
                || (InvalidMinDefaultPageHeight != -1 && PrintDoc.DefaultPageSettings.PrintableArea.Height <= InvalidMinDefaultPageHeight))
            {
                PrintDoc.DefaultPageSettings.PaperSize = new PaperSize("default", 850, 1100);
            }

            InvalidMinDefaultPageHeight = 0;
            InvalidMinDefaultPageWidth = -1;
        }
        else
        {
            PrintDoc.DefaultPageSettings.PaperSize = paperSize;
        }

        PrintDoc.PrinterSettings.Copies = copies;
        if (duplex != Duplex.Default)
        {
            PrintDoc.PrinterSettings.Duplex = duplex;
        }
    }

    public bool TryPrint()
    {
        if (SettingsErrorCode != PrintoutErrorCode.Success)
        {
            return false;
        }

        try
        {
            var activeForm = Form.ActiveForm;
            
            PrintDoc.Print();
            
            ODException.SwallowAnyException(() =>
            {
                //Sometimes after printing the application behind OD comes in front of OD. This is to help fix that.
                if (activeForm != null && (Form.ActiveForm == null || Form.ActiveForm.Name == ""))
                {
                    activeForm.Activate();
                }
            });
        }
        catch (Exception ex)
        {
            ErrorEx = ex;
            return false;
        }

        return true;
    }

    public bool TryPrintNoUI()
    {
        if (SettingsErrorCode != PrintoutErrorCode.Success)
        {
            return false;
        }

        try
        {
            PrintDoc.Print();
        }
        catch (Exception ex)
        {
            ErrorEx = ex;
                
            return false;
        }

        return true;
    }
}

public enum PrintSituation
{
    [Description("Default")]
    Default = 0,
        
    [Description("Statements")]
    Statement = 1,
        
    [Description("Labels - Single")]
    LabelSingle = 2,

    [Description("Claims")]
    Claim = 3,
        
    [Description("Treatment Plans and Perio")]
    TPPerio = 4,
        
    [Description("Labels - Sheet")]
    LabelSheet = 6,

    [Description("Postcards")]
    Postcard = 7,

    [Description("Appointments")]
    Appointments = 8,
    
    [Description("Receipts")]
    Receipt = 10,
}
    
public enum PrintoutErrorCode
{
    [Description("No error.")]
    Success,
    
    [Description("Error: No printers installed.")]
    NoInstalledPrinter,
    
    [Description("Error: Printers settings not found.")]
    PrinterSettingsNotFound,
    
    [Description("Error: Printer settings found but are flagged as invalid.")]
    InvalidPrinterSettings,
    
    [Description("Error: An error occurred while attempting to connect to the printer.")]
    PrinterConnectionError,

    [Description("Error: No active print spooler service found.")]
    InactivePrintSpoolerService,
}

public enum PrintoutOrigin
{
    Default,
    AtMargin
}
    
public enum PrintoutOrientation
{
    Default,
    Landscape,
    Portrait
}