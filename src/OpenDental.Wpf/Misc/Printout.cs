using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Documents;
using CodeBase;

//for PrintoutErrorCode

namespace OpenDental.Drawing
{
    public class Printout
    {
        ///<summary>Is set when an error occurs during loading and printing.</summary>
        public Exception ErrorEx = null;

        ///<summary>This is usually created internally by PrinterL during TryPrint.</summary>
        public FixedDocument FixedDocument_;

        ///<summary>When calling some TryPrint methods, this can force a print preview.</summary>
        public bool IsForcedPreview = false;

        ///<summary>Is set when an error occurs during loading and printing.</summary>
        public PrintoutErrorCode SettingsErrorCode = PrintoutErrorCode.Success;

        ///<summary>Optional. Default is left:0.25,top:0.4,right:0.25,bottom:0.4.</summary>
        public Thickness thicknessMarginInches;

        ///<summary>This should point to a method that takes a Graphics as an argument. Gets called for each page, just like the old WinForms. Should return true if it has more pages.</summary>
        public Func<Graphics, bool> FuncPrintPage;

        ///<summary>This is just what shows in the UI of the printer in Windows.</summary>
        public string Descript;

        ///<summary>Returns true if there is a valid printer installed on this machine. If false then SettingsErrorCode will contain more detailed information.</summary>
        public bool HasValidSettings()
        {
            SettingsErrorCode = PrintoutErrorCode.Success;
            var serviceControllerArray = ServiceController.GetServices();
            var serviceControllerPrintSpooler = serviceControllerArray.FirstOrDefault(x => x.ServiceName == "Spooler");
            if (serviceControllerPrintSpooler == null || serviceControllerPrintSpooler.Status != ServiceControllerStatus.Running)
            {
                SettingsErrorCode = PrintoutErrorCode.InactivePrintSpoolerService;
                return false;
            }

            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                SettingsErrorCode = PrintoutErrorCode.NoInstalledPrinter;
                return false;
            }

            var printerSettings = new PrinterSettings();
            if (!printerSettings.IsValid)
            {
                SettingsErrorCode = PrintoutErrorCode.InvalidPrinterSettings;
                return false;
            }

            try
            {
                //Try to find the printable area.  If there are no printers, it will throw an InvalidPrinterException.
                //We use the following pattern to determine which page size to use.
                //The values are not used here, we simply are accessing them the same way to see if an exception is thrown.
                //Therefore we will know when we check later the code will not fail.
                if (printerSettings.DefaultPageSettings.PrintableArea.Width == 0 || printerSettings.DefaultPageSettings.PrintableArea.Height == 0)
                {
                    //At least one valid printer is installed.
                }
            }
            catch (InvalidPrinterException ex)
            {
                //No printers installed.
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
    }
}