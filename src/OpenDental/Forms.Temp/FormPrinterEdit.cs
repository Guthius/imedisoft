using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormPrinterEdit : FormODBase
{
    private readonly PrintSituation _printSituation;

    public FormPrinterEdit(Printer printer)
    {
        InitializeComponent();

        _printSituation = printer.PrintSit;
        
        textFileExtension.Text = printer.FileExtension;
        
        checkPrompt.Checked = printer.DisplayPrompt;
        checkVirtualPrinter.Checked = printer.IsVirtualPrinter;
        
        textSituation.Text = _printSituation.GetDescription();
        
        FillComboPrinter(printer.PrinterName);
    }

    private void FillComboPrinter(string printerName)
    {
        PrinterSettings.StringCollection installedPrinters = null;
        try
        {
            installedPrinters = PrinterSettings.InstalledPrinters;
        }
        catch (Exception ex)
        {
            ShowException(ex, "Unable to access installed printers.");
            
            DialogResult = DialogResult.Cancel;
            
            return;
        }

        comboPrinter.Items.Clear();
        comboPrinter.Items.Add(_printSituation == PrintSituation.Default ? "Windows default" : "default");

        for (var i = 0; i < installedPrinters.Count; i++)
        {
            comboPrinter.Items.Add(installedPrinters[i]);
            if (printerName == installedPrinters[i])
            {
                comboPrinter.SelectedIndex = i + 1;
            }
        }

        if (comboPrinter.SelectedIndex == -1)
        {
            comboPrinter.SelectedIndex = 0;
        }
    }

    private void butSave_Click(object sender, EventArgs e)
    {
        var printerName = "";
        var isChecked = checkPrompt.Checked;
        
        if (comboPrinter.SelectedIndex > 0)
        {
            printerName = comboPrinter.SelectedItem.ToString();
        }

        Printers.PutForSit(_printSituation, Environment.MachineName, printerName, isChecked, isVirtual: checkVirtualPrinter.Checked, fileExtension: textFileExtension.Text);
        
        DataValid.SetInvalid(InvalidType.Computers);
        
        Printers.RefreshCache();
        
        DialogResult = DialogResult.OK;
    }
}