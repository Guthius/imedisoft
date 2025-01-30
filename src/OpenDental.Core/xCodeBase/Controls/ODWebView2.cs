using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace CodeBase.Controls;

public partial class ODWebView2 : WebView2
{
    public bool DoBlockNavigation = false;

    private bool _blockNavigation;

    public ODWebView2()
    {
        InitializeComponent();
    }

    public async Task Init()
    {
        var showError = false;

        try
        {
            if (CoreWebView2Environment.GetAvailableBrowserVersionString().IsNullOrEmpty())
            {
                showError = true;
            }
        }
        catch
        {
            showError = true;
        }

        if (showError)
        {
            const string warning =
                "Microsoft WebView2 is not available on this device. " +
                "To use this feature, the Microsoft WebView2 Runtime needs to be downloaded and installed on this machine.\r\n" +
                "Would you like to download the WebView2 Runtime now?";

            if (MessageBox.Show(warning, "Error", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ODException.SwallowAnyException(() => Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703"));
            }

            throw new ODException();
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var cacheFolder = ODFileUtils.CombinePaths(localAppData, "WindowsFormsWebView2");
        var environment = await CoreWebView2Environment.CreateAsync(null, cacheFolder);

        await EnsureCoreWebView2Async(environment);
    }

    public void CoreWebView2_InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        CoreWebView2.Settings.AreDevToolsEnabled = false;
        CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
    }

    public void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (_blockNavigation)
        {
            e.Cancel = true;
        }
    }

    public void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        _blockNavigation = DoBlockNavigation;
    }

    public void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        if (_blockNavigation)
        {
            e.Handled = true;
        }
    }

    public void OdWebView2Navigate(string path)
    {
        _blockNavigation = false;

        CoreWebView2.Navigate(path);
    }

    public void ClearCache(bool doDisplayError)
    {
        _ = ClearCache();

        if (doDisplayError)
        {
            MessageBox.Show(
                "Browser cache cleared. You may need to close or refresh this window. " +
                "If you are still experiencing problems, run the program as an Admin and try again.");
        }
    }

    private async Task ClearCache()
    {
        await CoreWebView2.CallDevToolsProtocolMethodAsync("Network.clearBrowserCache", "{}");
    }
}